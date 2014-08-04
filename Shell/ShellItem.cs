// BExplorer.Shell - A Windows Shell library for .Net.
// Copyright (C) 2007-2009 Steven J. Kirk
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the Free
// Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
// Boston, MA 2110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using BExplorer.Shell.Interop;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace BExplorer.Shell {

	#region Helpers

	/// <summary>
	/// Enumerates the types of shell icons.
	/// </summary>
	public enum ShellIconType {

		/// <summary>The system large icon type</summary>
		LargeIcon = SHGFI.LARGEICON,

		/// <summary>The system shell icon type</summary>
		ShellIcon = SHGFI.SHELLICONSIZE,

		/// <summary>The system small icon type</summary>
		SmallIcon = SHGFI.SMALLICON,
	}

	/// <summary>
	/// Enumerates the optional styles that can be applied to shell icons.
	/// </summary>
	[Flags]
	public enum ShellIconFlags {

		/// <summary>The icon is displayed opened.</summary>
		OpenIcon = SHGFI.ICON,

		/// <summary>Get the overlay for the icon as well.</summary>
		OverlayIndex = SHGFI.OVERLAYINDEX
	}

	internal class ShellItemConverter : TypeConverter {

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}
			else {
				return base.CanConvertFrom(context, sourceType);
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(InstanceDescriptor)) {
				return true;
			}
			else {
				return base.CanConvertTo(context, destinationType);
			}
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if (value is string) {
				string s = (string)value;

				if (s.Length == 0) {
					return ShellItem.Desktop;
				}
				else {
					return new ShellItem(s);
				}
			}
			else {
				return base.ConvertFrom(context, culture, value);
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (value is ShellItem) {
				Uri uri = ((ShellItem)value).ToUri();

				if (destinationType == typeof(string)) {
					if (uri.Scheme == "file") {
						return uri.LocalPath;
					}
					else {
						return uri.ToString();
					}
				}
				else if (destinationType == typeof(InstanceDescriptor)) {
					return new InstanceDescriptor(typeof(ShellItem).GetConstructor(new Type[] { typeof(string) }), new object[] { uri.ToString() });
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	#endregion Helpers

	/// <summary>
	/// Represents an item in the Windows Shell namespace.
	/// </summary>
	[TypeConverter(typeof(ShellItemConverter))]
	public class ShellItem : IEnumerable<ShellItem>, IDisposable {

		#region Properties
		private static ShellItem m_Desktop;

		private int? hashValue;
		public int IsShielded = -1;

		/// <summary>
		/// Gets the underlying <see cref="IShellItem"/> COM interface.
		/// </summary>
		public IShellItem ComInterface { get { return m_ComInterface; } protected set { m_ComInterface = value; } }
		//public IShellItem ComInterface { get { return m_ComInterface; } }
		protected IShellItem m_ComInterface;

		private ShellThumbnail thumbnail;

		internal bool IsNeedRefreshing { get; set; }
		internal bool IsThumbnailLoaded { get; set; }
		public bool IsIconLoaded { get; set; }
		//public bool ISRedrawed { get; set; }
		internal bool IsInitialised { get; set; }
		//public Bitmap ThumbnailIcon { get; set; }
		internal int OverlayIconIndex { get; set; }
		internal IExtractIconPWFlags IconType { get; private set; }
		public String CachedParsingName { get; private set; }
		public IntPtr ILPidl { get { return Shell32.ILFindLastID(Pidl); } }

		/// <summary>
		/// Gets the thumbnail of the ShellObject.
		/// </summary>
		public ShellThumbnail Thumbnail {
			get {
				if (thumbnail == null) thumbnail = new ShellThumbnail(this);
				return thumbnail;
			}
		}



		/// <summary>
		/// Gets the item's parsing name.
		/// </summary>
		public string ParsingName { get { return GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING); } }

		/// <summary>
		/// Gets the item's shell icon.
		/// </summary>
		internal Icon ShellIcon {
			get {
				SHFILEINFO info = new SHFILEINFO();
				IntPtr result = Shell32.SHGetFileInfo(Pidl, 0, out info,
						Marshal.SizeOf(info),
						SHGFI.ADDOVERLAYS | SHGFI.ICON |
						SHGFI.SHELLICONSIZE | SHGFI.PIDL);

				if (result == IntPtr.Zero) {
					throw new Exception("Error retrieving shell folder icon");
				}

				return Icon.FromHandle(info.hIcon);
			}
		}

		/// <summary>
		/// Gets the normal display name of the item.
		/// </summary>
		public string DisplayName { get { return GetDisplayName(SIGDN.NORMALDISPLAY); } }

		/// <summary>
		/// Gets the file system path of the item.
		/// </summary>
		public string FileSystemPath { get { return GetDisplayName(SIGDN.FILESYSPATH); } }

		/*
		/// <summary>
		/// Gets a value indicating whether the item is a file system item
		/// or the child of a file system item.
		/// </summary>
		public bool IsFileSystemAncestor {
			get {
				SFGAO sfgao;
				ComInterface.GetAttributes(SFGAO.FILESYSANCESTOR, out sfgao);
				return sfgao != 0;
			}
		}
		*/

		/// <summary>
		/// Gets a PIDL representing the item.
		/// </summary>
		public IntPtr Pidl { get { return GetIDListFromObject(ComInterface); } }

		/// <summary>
		/// Gets a value indicating whether the item is a folder.
		/// </summary>
		public bool IsFolder {
			get {
				SFGAO sfgao;
				ComInterface.GetAttributes(SFGAO.FOLDER, out sfgao);
				SFGAO sfgao2;
				ComInterface.GetAttributes(SFGAO.STREAM, out sfgao2);
				return sfgao != 0 && sfgao2 == 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item has subfolders.
		/// </summary>
		public bool HasSubFolders { get { return COM_Attribute_Check(SFGAO.HASSUBFOLDER); } }

		/// <summary>
		/// Gets a value indicating whether the item is a file system item.
		/// </summary>
		public bool IsFileSystem {
			get {
				//TODO: try removing this Try Catch!
				try {
					return COM_Attribute_Check(SFGAO.FILESYSTEM);
				}
				catch {
					return false;
				}
			}
		}

		public bool IsShared { get { return COM_Attribute_Check(SFGAO.SHARE); } }

		/*
		/// <summary>
		/// Gets a value indicating whether the item is read-only.
		/// </summary>
		public bool IsReadOnly { get { return COM_Attribute_Check(SFGAO.READONLY); } }
		*/

		/// <summary>Gets a value indicating whether the item is Hidden.</summary>
		public bool IsHidden {
			get {
				try {
					return COM_Attribute_Check(SFGAO.HIDDEN);
				}
				catch (FileNotFoundException) {
					return false;
				}
				catch (NullReferenceException) {
					// NativeShellItem is null
					return false;
				}
			}
		}

		/// <summary>
		/// Gets a value that determines if this ShellObject is a link or shortcut.
		/// </summary>
		public bool IsLink {
			get {
				try {
					return COM_Attribute_Check(SFGAO.LINK);
				}
				catch (FileNotFoundException) {
					return false;
				}
				catch (NullReferenceException) {
					// NativeShellItem is null
					return false;
				}
			}
		}

		/// <summary>Returns the extension of the specified path string.</summary>
		///<value>
		///     The extension of the specified path (including the period "."), or null,
		///     or System.String.Empty. If path is null, System.IO.Path.GetExtension(System.String)
		///     returns null. If path does not have extension information, System.IO.Path.GetExtension(System.String)
		///     returns System.String.Empty.
		/// </value>
		/// <exception cref="System.ArgumentException">
		/// path contains one or more of the invalid characters defined in System.IO.Path.GetInvalidPathChars().
		/// </exception>
		public String Extension { get { return Path.GetExtension(this.ParsingName).ToLowerInvariant(); } }

		public bool IsDrive {
			get {
				try {
					return Directory.GetLogicalDrives().Contains(ParsingName) && Kernel32.GetDriveType(ParsingName) != DriveType.Network;
				}
				catch {
					return false;
				}
			}
		}

		public bool IsNetworkPath {
			get {
				//TODO: Test

				if (ParsingName.StartsWith("::")) {
					return false;
				}
				else if (!ParsingName.StartsWith(@"/") && !ParsingName.StartsWith(@"\")) {
					string rootPath = System.IO.Path.GetPathRoot(ParsingName);	// get drive's letter
					DriveInfo driveInfo = new DriveInfo(rootPath);				// get info about the drive
					return driveInfo.DriveType == DriveType.Network;			// return true if a network drive
				}
				else {
					return true; // is a UNC path
				}

			}
		}

		public bool IsNetDrive {
			get {
				try {
					return Directory.GetLogicalDrives().Contains(ParsingName) && Kernel32.GetDriveType(ParsingName) == DriveType.Network;
				}
				catch {
					return false;
				}
			}
		}

		public bool IsSearchFolder {
			get {
				try {
					return (!ParsingName.StartsWith("::") && !IsFileSystem && !ParsingName.StartsWith(@"\\") && !ParsingName.Contains(":\\")) || ParsingName.EndsWith(".search-ms");
				}
				catch {
					return false;
				}
			}
		}

		/// <summary>
		/// Gets the item's parent.
		/// </summary>
		public ShellItem Parent {
			get {
				IShellItem item;
				HResult result = ComInterface.GetParent(out item);

				if (result == HResult.S_OK) {
					return new ShellItem(item);
				}
				else if (result == HResult.MK_E_NOOBJECT) {
					return null;
				}
				else {
					Marshal.ThrowExceptionForHR((int)result);
					return null;
				}
			}
		}

		public IntPtr AbsolutePidl {
			get {
				uint attr;
				IntPtr pidl;
				Shell32.SHParseDisplayName(this.ParsingName, IntPtr.Zero, out pidl, 0, out attr);
				return pidl;
			}
		}

		/// <summary>
		/// Gets the item's tooltip text.
		/// </summary>
		public string ToolTipText {
			get {
				IntPtr result;
				IQueryInfo queryInfo;
				IntPtr infoTipPtr;
				string infoTip;

				try {
					IntPtr relativePidl = Shell32.ILFindLastID(Pidl);
					Parent.GetIShellFolder().GetUIObjectOf(IntPtr.Zero, 1, new IntPtr[] { relativePidl }, typeof(IQueryInfo).GUID, 0, out result);
				}
				catch (Exception) {
					return string.Empty;
				}

				queryInfo = (IQueryInfo)Marshal.GetTypedObjectForIUnknown(result, typeof(IQueryInfo));
				queryInfo.GetInfoTip(0x00000001 | 0x00000008, out infoTipPtr);
				infoTip = Marshal.PtrToStringUni(infoTipPtr);
				Ole32.CoTaskMemFree(infoTipPtr);
				return infoTip;
			}
		}

		#endregion Properties

		#region Value Getters

		#region Enumerator

		/// <summary>
		/// Returns an enumerator detailing the child items of the
		/// <see cref="ShellItem"/>.
		/// </summary>
		///
		/// <remarks>
		/// This method returns all child item including hidden
		/// items.
		/// </remarks>
		///
		/// <returns>
		/// An enumerator over all child items.
		/// </returns>
		public IEnumerator<ShellItem> GetEnumerator() {
			return GetEnumerator(SHCONTF.FOLDERS | SHCONTF.INCLUDEHIDDEN | SHCONTF.INCLUDESUPERHIDDEN |
					SHCONTF.NONFOLDERS | SHCONTF.FASTITEMS | SHCONTF.ENABLE_ASYNC);
		}

		/// <summary>
		/// Returns an enumerator detailing the child items of the
		/// <see cref="ShellItem"/>.
		/// </summary>
		///
		/// <param name="filter">
		/// A filter describing the types of child items to be included.
		/// </param>
		///
		/// <returns>
		/// An enumerator over all child items.
		/// </returns>
		public IEnumerator<ShellItem> GetEnumerator(SHCONTF filter) {
			IShellFolder folder = GetIShellFolder();
			IEnumIDList enumId = GetIEnumIDList(folder, filter);
			uint count;
			IntPtr pidl;
			HResult result;

			if (enumId == null) {
				yield break;
			}

			result = enumId.Next(1, out pidl, out count);
			while (result == HResult.S_OK) {
				yield return new ShellItem(this, pidl);
				Shell32.ILFree(pidl);
				result = enumId.Next(1, out pidl, out count);
			}

			if (result != HResult.S_FALSE) {
				Marshal.ThrowExceptionForHR((int)result);
			}

			yield break;
		}

		#endregion


		public DriveInfo GetDriveInfo() { return IsDrive || IsNetDrive ? new DriveInfo(ParsingName) : null; }

		/// <summary>
		/// Returns the name of the item in the specified style.
		/// </summary>
		///
		/// <param name="sigdn">
		/// The style of display name to return.
		/// </param>
		///
		/// <returns>
		/// A string containing the display name of the item.
		/// </returns>
		public string GetDisplayName(SIGDN sigdn) {
			try {
				IntPtr resultPtr = ComInterface.GetDisplayName(sigdn);
				string result = Marshal.PtrToStringUni(resultPtr);
				Marshal.FreeCoTaskMem(resultPtr);
				return result;
			}
			catch (Exception) {
				return String.Empty;
			}
		}

		private IExtractIconPWFlags GetIconType() {
			var parsingName = this.ParsingName.ToLowerInvariant();
			if (this.IsLink)
				return IExtractIconPWFlags.GIL_PERINSTANCE | IExtractIconPWFlags.GIL_FORCENOSHIELD;
			if (parsingName.ToLowerInvariant().EndsWith(".htm") || parsingName.ToLowerInvariant().EndsWith(".html") || parsingName.ToLowerInvariant().EndsWith(".xml"))
				return IExtractIconPWFlags.GIL_PERCLASS | IExtractIconPWFlags.GIL_FORCENOSHIELD;
			IExtractIcon iextract = null;
			IShellFolder ishellfolder = null;
			StringBuilder str = null;
			IntPtr result;

			if (this.Parent == null) {
				return 0;
			}

			try {
				var guid = new Guid("000214fa-0000-0000-c000-000000000046");
				uint res = 0;
				ishellfolder = this.Parent.GetIShellFolder();
				IntPtr[] pidls = new IntPtr[1];
				pidls[0] = Shell32.ILFindLastID(this.Pidl);
				ishellfolder.GetUIObjectOf(
					IntPtr.Zero,
					1,
					pidls,
					ref guid,
					res,
					out result
				);
				if (result == IntPtr.Zero) {
					pidls = null;
					Marshal.ReleaseComObject(ishellfolder);
					return IExtractIconPWFlags.GIL_PERCLASS;
				}
				iextract = (IExtractIcon)Marshal.GetTypedObjectForIUnknown(result, typeof(IExtractIcon));
				str = new StringBuilder(512);
				int index = -1;
				IExtractIconPWFlags flags;
				iextract.GetIconLocation(IExtractIconUFlags.GIL_ASYNC, str, 512, out index, out flags);
				pidls = null;
				Marshal.ReleaseComObject(ishellfolder);
				Marshal.ReleaseComObject(iextract);
				ishellfolder = null;
				iextract = null;
				str = null;
				return flags;
			}
			catch (Exception) {
				if (ishellfolder != null)
					Marshal.ReleaseComObject(ishellfolder);
				if (iextract != null)
					Marshal.ReleaseComObject(iextract);
				return 0;
			}
		}

		public IExtractIconPWFlags GetShield() {
			IExtractIcon iextract = null;
			IShellFolder ishellfolder = null;
			StringBuilder str = null;
			IntPtr result;
			try {
				var guid = new Guid("000214fa-0000-0000-c000-000000000046");
				uint res = 0;
				ishellfolder = this.Parent.GetIShellFolder();
				IntPtr[] pidls = new IntPtr[1];
				pidls[0] = Shell32.ILFindLastID(this.Pidl);
				ishellfolder.GetUIObjectOf(IntPtr.Zero,
				1, pidls,
				ref guid, res, out result);
				iextract = (IExtractIcon)Marshal.GetTypedObjectForIUnknown(result, typeof(IExtractIcon));
				str = new StringBuilder(512);
				int index = -1;
				IExtractIconPWFlags flags;
				iextract.GetIconLocation(IExtractIconUFlags.GIL_CHECKSHIELD, str, 512, out index, out flags);
				pidls = null;
				if (ishellfolder != null)
					Marshal.ReleaseComObject(ishellfolder);
				if (iextract != null)
					Marshal.ReleaseComObject(iextract);
				str = null;
				return flags;
			}
			catch (Exception) {
				if (ishellfolder != null)
					Marshal.ReleaseComObject(ishellfolder);
				if (iextract != null)
					Marshal.ReleaseComObject(iextract);
				str = null;
				return 0;
			}
		}

		/*
		public int GetFallbackIconIndex() {
			try {
				var guid = new Guid("000214fa-0000-0000-c000-000000000046");
				IntPtr result;
				uint res = 0;
				var ishellfolder = this.Parent.GetIShellFolder();
				ishellfolder.GetUIObjectOf(IntPtr.Zero,
				(uint)1, new IntPtr[1] { this.ILPidl },
				guid, res, out result);
				var iextract = (IExtractIcon)Marshal.GetTypedObjectForIUnknown(result, typeof(IExtractIcon));
				var str = new StringBuilder(512);
				int index = -1;
				IExtractIconPWFlags flags;
				iextract.GetIconLocation(IExtractIconUFlags.GIL_DEFAULTICON | IExtractIconUFlags.GIL_FORSHELL | IExtractIconUFlags.GIL_OPENICON, str, 512, out index, out flags);

				return index;
			}
			catch (Exception) {
				return 0;
			}
		}
		*/

		/*
		public int GetItemIndexInCollection(ShellItem[] collection) { return Array.IndexOf(collection, this); }
		*/


		/// <summary>
		/// Gets the Bitmap of this ShellItem's Icon
		/// </summary>
		/// <param name="Size"></param>
		/// <param name="format"></param>
		/// <param name="retrieve"></param>
		/// <returns></returns>
		internal Bitmap GetShellThumbnail(int Size, ShellThumbnailFormatOption format = ShellThumbnailFormatOption.Default, ShellThumbnailRetrievalOption retrieve = ShellThumbnailRetrievalOption.Default) {
			this.Thumbnail.RetrievalOption = retrieve;
			this.Thumbnail.FormatOption = format;
			this.Thumbnail.CurrentSize = new System.Windows.Size(Size, Size);
			return this.Thumbnail.Bitmap;
		}

		/// <summary>
		/// Returns an <see cref="ComTypes.IDataObject"/> representing the
		/// item. This object is used in drag and drop operations.
		/// </summary>
		public System.Runtime.InteropServices.ComTypes.IDataObject GetIDataObject() {
			IntPtr res;
			HResult result = ComInterface.BindToHandler(IntPtr.Zero, BHID.SFUIObject, typeof(ComTypes.IDataObject).GUID, out res);
			return (System.Runtime.InteropServices.ComTypes.IDataObject)Marshal.GetTypedObjectForIUnknown(res, typeof(System.Runtime.InteropServices.ComTypes.IDataObject));
		}

		public List<AssociationItem> GetAssocList() {
			var assocList = new List<AssociationItem>();
			IntPtr enumAssocPtr;
			var h = Shell32.SHAssocEnumHandlers(Path.GetExtension(ParsingName), Shell32.ASSOC_FILTER.ASSOC_FILTER_RECOMMENDED, out enumAssocPtr);
			IntPtr pUnk = Marshal.ReadIntPtr(enumAssocPtr);
			IntPtr pFunc = Marshal.ReadIntPtr(pUnk + 3 * IntPtr.Size);
			Shell32.funcNext Next = (Shell32.funcNext)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(Shell32.funcNext));

			IntPtr[] funcs = new IntPtr[15];
			int num;
			int res = Next(enumAssocPtr, 15, funcs, out num);
			if (res == 0) {
				for (int i = 0; i < num; i++) {
					var funcpUnk = Marshal.ReadIntPtr(funcs[i]);
					var getNamepFunc = Marshal.ReadIntPtr(funcpUnk + 3 * IntPtr.Size);
					var getNameUIpFunc = Marshal.ReadIntPtr(funcpUnk + 4 * IntPtr.Size);
					Shell32.funcGetName GetName = (Shell32.funcGetName)Marshal.GetDelegateForFunctionPointer(getNamepFunc, typeof(Shell32.funcGetName));
					Shell32.funcGetName GetUIName = (Shell32.funcGetName)Marshal.GetDelegateForFunctionPointer(getNameUIpFunc, typeof(Shell32.funcGetName));
					String path = String.Empty;
					String displayName = String.Empty;
					GetName(funcs[i], out path);
					GetUIName(funcs[i], out displayName);
					assocList.Add(new AssociationItem(this) { DisplayName = displayName, ApplicationPath = path });
					Marshal.Release(funcs[i]);
					Marshal.Release(funcpUnk);
					Marshal.Release(getNamepFunc);
					Marshal.Release(getNameUIpFunc);
				}
			}
			Marshal.Release(enumAssocPtr);
			Marshal.Release(pUnk);

			return assocList;
		}

		/*
		/// <summary>
		/// Returns an <see cref="IDropTarget"/> representing the
		/// item. This object is used in drag and drop operations.
		/// </summary>
		public IDropTarget GetIDropTarget(System.Windows.Forms.Control control) {
			IntPtr result = GetIShellFolder().CreateViewObject(control.Handle, typeof(IDropTarget).GUID);
			return (IDropTarget)Marshal.GetTypedObjectForIUnknown(result, typeof(IDropTarget));
		}
		*/

		/// <summary>
		/// Returns an <see cref="IShellFolder"/> representing the
		/// item.
		/// </summary>
		public IShellFolder GetIShellFolder() {
			IntPtr res;
			HResult result = ComInterface.BindToHandler(IntPtr.Zero, BHID.SFObject, typeof(IShellFolder).GUID, out res);
			IShellFolder iShellFolder = (IShellFolder)Marshal.GetTypedObjectForIUnknown(res, typeof(IShellFolder));
			return iShellFolder;
		}

		public PropVariant GetPropertyValue(PROPERTYKEY pkey, Type type) {
			PropVariant pvar = new PropVariant();
			IShellItem2 isi2 = (IShellItem2)ComInterface;
			isi2.GetProperty(ref pkey, pvar);
			return pvar;

			//TODO: do we need all of the above code


			/*
			if (isi2.GetProperty(ref pkey, pvar) != HResult.S_OK) {
				//String value = String.Empty;
				//if (pvar.Value != null)
				//{
				//	//if (currentCollumn.CollumnType == typeof(DateTime))
				//	//{
				//	//	value = ((DateTime)pvar.Value).ToString(Thread.CurrentThread.CurrentCulture);
				//	//}
				//	//else if (currentCollumn.CollumnType == typeof(long))
				//	//{
				//	//	value = String.Format("{0} KB", (Math.Ceiling(Convert.ToDouble(pvar.Value.ToString()) / 1024).ToString("# ### ### ##0"))); //ShlWapi.StrFormatByteSize(Convert.ToInt64(pvar.Value.ToString()));
				//	//}
				//	//else
				//	//{
				//	//	value = pvar.Value.ToString();
				//	//}
				//}
				//nmlv.item.pszText = value;
				//Marshal.StructureToPtr(nmlv, m.LParam, false);
			}
			return pvar;
			*/
		}

		/// <summary>
		/// Returns an enumerator detailing the child items of the
		/// <see cref="ShellItem"/>.
		/// </summary>
		///
		/// <remarks>
		/// This method returns all child item including hidden
		/// items.
		/// </remarks>
		///
		/// <returns>
		/// An enumerator over all child items.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		/// <summary>
		/// Gets the index in the system image list of the icon representing
		/// the item.
		/// </summary>
		///
		/// <param name="type">
		/// The type of icon to retrieve.
		/// </param>
		///
		/// <param name="flags">
		/// Flags detailing additional information to be conveyed by the icon.
		/// </param>
		///
		/// <returns></returns>
		public int GetSystemImageListIndex(ShellIconType type, ShellIconFlags flags) {
			SHFILEINFO info = new SHFILEINFO();
			IntPtr result = Shell32.SHGetFileInfo(Pidl, 0, out info,
					Marshal.SizeOf(info),
					SHGFI.ICON | SHGFI.SYSICONINDEX | SHGFI.OVERLAYINDEX | SHGFI.PIDL |
					(SHGFI)type | (SHGFI)flags);

			if (result == IntPtr.Zero) {
				throw new Exception("Error retrieving shell folder icon");
			}

			User32.DestroyIcon(info.hIcon);
			return info.iIcon;
		}

		public static int GetSystemImageListIndex(IntPtr pidl, ShellIconType type, ShellIconFlags flags) {
			SHFILEINFO info = new SHFILEINFO();
			IntPtr result = Shell32.SHGetFileInfo(pidl, 0, out info,
					Marshal.SizeOf(info),
					SHGFI.ICON | SHGFI.SYSICONINDEX | SHGFI.OVERLAYINDEX | SHGFI.PIDL |
					(SHGFI)type | (SHGFI)flags);

			if (result == IntPtr.Zero) {
				throw new Exception("Error retrieving shell folder icon");
			}

			User32.DestroyIcon(info.hIcon);
			return info.iIcon;
		}

		/*
		public static int GetSystemImageListDefaultIndex(IntPtr pidl, bool IsFolder) {
			SHFILEINFO info = new SHFILEINFO();
			IntPtr result = Shell32.SHGetFileInfo(pidl, IsFolder ? (int)FileAttributes.Directory : 0, out info,
					Marshal.SizeOf(info),
					SHGFI.SYSICONINDEX);

			if (result == IntPtr.Zero) {
				throw new Exception("Error retrieving shell folder icon");
			}

			return info.iIcon;
		}
		*/

		#endregion Value Getters

		#region Constructors

		private void Constructor_Helper() {
			this.IconType = GetIconType();
			this.CachedParsingName = this.ParsingName;
			this.OverlayIconIndex = -1;
		} //TODO: Figure out if this should be added in protected ShellItem() { }

		[Obsolete("Try to remove this!!!")]
		protected ShellItem() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellItem"/> class.
		/// </summary>
		///
		/// <remarks>
		/// Takes a <see cref="Uri"/> containing the location of the ShellItem.
		/// This constructor accepts URIs using two schemes:
		///
		/// - file: A file or folder in the computer's filesystem, e.g.
		///         file:///D:/Folder
		/// - shell: A virtual folder, or a file or folder referenced from
		///          a virtual folder, e.g. shell:///Personal/file.txt
		/// </remarks>
		///
		/// <param name="uri">
		/// A <see cref="Uri"/> containing the location of the ShellItem.
		/// </param>
		public ShellItem(Uri uri) {
			Initialize(uri);
			Constructor_Helper();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellItem"/> class.
		/// </summary>
		///
		/// <remarks>
		/// Takes a <see cref="string"/> containing the location of the ShellItem.
		/// This constructor accepts URIs using two schemes:
		///
		/// - file: A file or folder in the computer's filesystem, e.g.
		///         file:///D:/Folder
		/// - shell: A virtual folder, or a file or folder referenced from
		///          a virtual folder, e.g. shell:///Personal/file.txt
		/// </remarks>
		///
		/// <param name="path">
		/// A string containing a Uri with the location of the ShellItem.
		/// </param>
		public ShellItem(string path) {
			Uri newUri = new Uri(path);
			Initialize(newUri);
			Constructor_Helper();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellItem"/> class.
		/// </summary>
		///
		/// <remarks>
		/// Takes an <see cref="Environment.SpecialFolder"/> containing the
		/// location of the folder.
		/// </remarks>
		///
		/// <param name="folder">
		/// An <see cref="Environment.SpecialFolder"/> containing the
		/// location of the folder.
		/// </param>
		public ShellItem(Environment.SpecialFolder folder) {
			IntPtr pidl;

			if (Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, (CSIDL)folder, out pidl) == HResult.S_OK) {
				try {
					ComInterface = CreateItemFromIDList(pidl);
				}
				finally {
					Shell32.ILFree(pidl);
				}
			}
			else {
				// SHGetSpecialFolderLocation does not support many common
				// CSIDL values on Windows 98, but SHGetFolderPath in
				// ShFolder.dll does, so fall back to it if necessary. We
				// try SHGetSpecialFolderLocation first because it returns
				// a PIDL which is preferable to a path as it can express
				// virtual folder locations.
				StringBuilder path = new StringBuilder();
				Marshal.ThrowExceptionForHR((int)Shell32.SHGetFolderPath(IntPtr.Zero, (CSIDL)folder, IntPtr.Zero, 0, path));
				ComInterface = CreateItemFromParsingName(path.ToString());
			}
			Constructor_Helper();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellItem"/> class.
		/// </summary>
		///
		/// <remarks>
		/// Creates a ShellItem which is a named child of <paramref name="parent"/>.
		/// </remarks>
		///
		/// <param name="parent">
		/// The parent folder of the item.
		/// </param>
		///
		/// <param name="name">
		/// The name of the child item.
		/// </param>
		public ShellItem(ShellItem parent, string name) {
			if (parent.IsFileSystem) {
				// If the parent folder is in the file system, our best
				// chance of success is to use the FileSystemPath to
				// create the new item. Folders other than Desktop don't
				// seem to implement ParseDisplayName properly.
				ComInterface = CreateItemFromParsingName(Path.Combine(parent.FileSystemPath, name));
			}
			else {
				IShellFolder folder = parent.GetIShellFolder();
				uint eaten;
				IntPtr pidl;
				uint attributes = 0;

				folder.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, name, out eaten, out pidl, ref attributes);

				try {
					ComInterface = CreateItemFromIDList(pidl);
				}
				finally {
					Shell32.ILFree(pidl);
				}
			}
			Constructor_Helper();
		}

		public ShellItem(IntPtr pidl) {
			ComInterface = CreateItemFromIDList(pidl);
			Constructor_Helper();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShellItem"/> class.
		/// </summary>
		///
		/// <param name="comInterface">
		/// An <see cref="IShellItem"/> representing the folder.
		/// </param>
		public ShellItem(IShellItem comInterface) {
			ComInterface = comInterface;
			this.CachedParsingName = this.ParsingName;
			this.OverlayIconIndex = -1;
		} //TODO: Consider adding Constructor_Helper();

		internal ShellItem(ShellItem parent, IntPtr pidl) {
			ComInterface = CreateItemWithParent(parent, pidl);
			Constructor_Helper();
		}

		#endregion Constructors

		#region Comparisons

		/// <summary>
		/// Implements the == (equality) operator.
		/// </summary>
		/// <param name="leftShellObject">First object to compare.</param>
		/// <param name="rightShellObject">Second object to compare.</param>
		/// <returns>True if leftShellObject equals rightShellObject; false otherwise.</returns>
		public static bool operator ==(ShellItem leftShellObject, ShellItem rightShellObject) {
			if ((object)leftShellObject == null) {
				return ((object)rightShellObject == null);
			}
			return leftShellObject.Equals(rightShellObject);
		}

		/// <summary>
		/// Implements the != (inequality) operator.
		/// </summary>
		/// <param name="leftShellObject">First object to compare.</param>
		/// <param name="rightShellObject">Second object to compare.</param>
		/// <returns>True if leftShellObject does not equal leftShellObject; false otherwise.</returns>
		public static bool operator !=(ShellItem leftShellObject, ShellItem rightShellObject) {
			return !(leftShellObject == rightShellObject);
		}

		public override int GetHashCode() {
			if (!hashValue.HasValue) {
				uint size = Shell32.ILGetSize(Pidl);
				if (size != 0) {
					byte[] pidlData = new byte[size];
					Marshal.Copy(Pidl, pidlData, 0, (int)size);
					byte[] hashData = ShellItem.hashProvider.ComputeHash(pidlData);
					hashValue = BitConverter.ToInt32(hashData, 0);
				}
				else {
					hashValue = 0;
				}
			}
			return hashValue.Value;
		}

		/// <summary>
		/// Compares two <see cref="IShellItem"/>s. The comparison is carried
		/// out by display order.
		/// </summary>
		///
		/// <param name="item">
		/// The item to compare.
		/// </param>
		///
		/// <returns>
		/// 0 if the two items are equal. A negative number if
		/// <see langword="this"/> is before <paramref name="item"/> in
		/// display order. A positive number if
		/// <see langword="this"/> comes after <paramref name="item"/> in
		/// display order.
		/// </returns>
		public int Compare(ShellItem item) {
			int result = ComInterface.Compare(item.ComInterface, SICHINT.DISPLAY);
			return result;
		}

		/// <see langword="true"/> if the two objects refer to the same
		/// folder, <see langword="false"/> otherwise.
		/// <summary>
		/// Determines if two ShellObjects are identical.
		/// </summary>
		/// <param name="other">The ShellObject to comare this one to.</param>
		/// <returns>True if the ShellObjects are equal, false otherwise.</returns>
		/// 		
		public bool Equals(ShellItem other) {
			if (other == null) return false;
			if (String.IsNullOrEmpty(this.CachedParsingName) || string.IsNullOrEmpty(other.CachedParsingName))
				return this.ParsingName == other.ParsingName;

			return this.CachedParsingName == other.CachedParsingName;
		}

		/// <summary>
		/// Returns whether this object is equal to another.
		/// </summary>
		/// <param name="obj">The object to compare against.</param>
		/// <returns>Equality result.</returns>
		public override bool Equals(object obj) {
			return this.Equals(obj as ShellItem);
		}

		#endregion Comparisons

		#region Static Stuff

		private static bool RunningVista { get { return Environment.OSVersion.Version.Major >= 6; } }
		private static MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();

		/// <summary>
		/// Gets the Desktop folder.
		/// </summary>
		public static ShellItem Desktop {
			get {
				if (m_Desktop == null) {
					IShellItem item;
					IntPtr pidl;
					Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, (CSIDL)Environment.SpecialFolder.Desktop, out pidl);

					try {
						item = CreateItemFromIDList(pidl);
					}
					finally {
						Shell32.ILFree(pidl);
					}

					m_Desktop = new ShellItem(item);
				}
				return m_Desktop;
			}
		}

		private void InitializeFromShellUri(Uri uri) {
			//TODO: add shell folders handling here
			//KnownFolderManager manager = new KnownFolderManager();
			string path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
			string knownFolder;
			string restOfPath;
			int separatorIndex = path.IndexOf('/');

			if (separatorIndex != -1) {
				knownFolder = path.Substring(0, separatorIndex);
				restOfPath = path.Substring(separatorIndex + 1);
			}
			else {
				knownFolder = path;
				restOfPath = string.Empty;
			}

			IKnownFolder knownFolderI = KnownFolderHelper.FromParsingName(knownFolder);
			if (knownFolderI != null)
				ComInterface = (knownFolderI as ShellItem).ComInterface;
			else if (knownFolder.StartsWith(KnownFolders.Libraries.ParsingName)) {
				ShellLibrary lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(knownFolder), true);
				if (lib != null) {
					ComInterface = lib.ComInterface;
				}
			}

			//m_ComInterface = manager.GetFolder(knownFolder).CreateShellItem().ComInterface;

			if (restOfPath != string.Empty) {
				ComInterface = this[restOfPath.Replace('/', '\\')].ComInterface;
			}
		}

		private static IShellItem CreateItemFromIDList(IntPtr pidl) {
			if (RunningVista) {
				return Shell32.SHCreateItemFromIDList(pidl, typeof(IShellItem).GUID);
			}
			else {
				return new Interop.VistaBridge.ShellItemImpl(pidl, false);
			}
		}

		private static IShellItem CreateItemFromParsingName(string path) {
			if (RunningVista) {
				return Shell32.SHCreateItemFromParsingName(path, IntPtr.Zero, typeof(IShellItem).GUID);
			}
			else {
				IShellFolder desktop = Desktop.GetIShellFolder();
				uint attributes = 0;
				uint eaten;
				IntPtr pidl;

				desktop.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, path, out eaten, out pidl, ref attributes);
				return new Interop.VistaBridge.ShellItemImpl(pidl, true);
			}
		}

		private static IShellItem CreateItemWithParent(ShellItem parent, IntPtr pidl) {
			if (RunningVista) {
				return Shell32.SHCreateItemWithParent(IntPtr.Zero, parent.GetIShellFolder(), pidl, typeof(IShellItem).GUID);
			}
			else {
				Interop.VistaBridge.ShellItemImpl impl = (Interop.VistaBridge.ShellItemImpl)parent.ComInterface;
				return new Interop.VistaBridge.ShellItemImpl(Shell32.ILCombine(impl.Pidl, pidl), true);
			}
		}

		private static IntPtr GetIDListFromObject(IShellItem item) {
			if (RunningVista) {
				return item != null ? Shell32.SHGetIDListFromObject(item) : IntPtr.Zero;
			}
			else {
				return ((Interop.VistaBridge.ShellItemImpl)item).Pidl;
			}
		}

		private static IEnumIDList GetIEnumIDList(IShellFolder folder, SHCONTF flags) {
			IEnumIDList result;

			if (folder.EnumObjects(IntPtr.Zero, flags, out result) == HResult.S_OK) {
				return result;
			}
			else {
				return null;
			}
		}

		#endregion Static Stuff

		#region Dispose

		/// <summary>
		/// Clears up any resources associated with the SystemImageList
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Clears up any resources associated with the SystemImageList
		/// when disposing is true.
		/// </summary>
		/// <param name="disposing">Whether the object is being disposed</param>
		public virtual void Dispose(bool disposing) {
			if (disposing) {
				if (ComInterface != null) {
					Marshal.FinalReleaseComObject(ComInterface);
				}
				ComInterface = null;
			}
		}

		#endregion Dispose

		#region Checks

		/*
		/// <summary>
		/// Tests whether the <see cref="ShellItem"/> is the immediate parent
		/// of another item.
		/// </summary>
		///
		/// <param name="item">
		/// The potential child item.
		/// </param>
		public bool IsImmediateParentOf(ShellItem item) { return IsFolder && Shell32.ILIsParent(Pidl, item.Pidl, true); }
		*/

		/// <summary>
		/// Tests whether the <see cref="ShellItem"/> is the parent of
		/// another item.
		/// </summary>
		///
		/// <param name="item">
		/// The potential child item.
		/// </param>
		public bool IsParentOf(ShellItem item) { return IsFolder && Shell32.ILIsParent(Pidl, item.Pidl, false); }

		#endregion Checks

		#region Misc

		/// <summary>
		/// Gets a child item.
		/// </summary>
		///
		/// <param name="name">
		/// The name of the child item.
		/// </param>
		public ShellItem this[string name] { get { return new ShellItem(this, name); } }

		/// <summary>
		/// Returns a string representation of the <see cref="ShellItem"/>.
		/// </summary>
		public override string ToString() {
			return this.DisplayName;
		}

		/// <summary>
		/// Returns a URI representation of the <see cref="ShellItem"/>.
		/// </summary>
		public Uri ToUri() {
			StringBuilder path = new StringBuilder("shell:///");

			if (this.ParsingName.StartsWith("::")) {
				path.Append(this.ParsingName);
				return new Uri(path.ToString());
			}
			return new Uri(this.FileSystemPath);
		}

		private void Initialize(Uri uri) {
			if (uri.Scheme == "file") {
				ComInterface = CreateItemFromParsingName(uri.LocalPath);
			}
			else if (uri.Scheme == "shell") {
				InitializeFromShellUri(uri);
			}
			else {
				throw new InvalidOperationException("Invalid URI scheme");
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private bool COM_Attribute_Check(SFGAO Check) {
			SFGAO sfgao;
			ComInterface.GetAttributes(Check, out sfgao);
			return sfgao != 0;
		}

		/*
		public void GetAssocHandlers() {
			if (this.m_ComInterface != null) {
				var result = IntPtr.Zero;
				this.m_ComInterface.BindToHandler(IntPtr.Zero, BHID.SFAssocHandlers, typeof(IEnumAssocHandlers).GUID, out result);
				var enumHandlers = Marshal.GetObjectForIUnknown(result) as IEnumAssocHandlers;
				IAssocHandler assoc = null;
				uint items = 0;
				while (enumHandlers.Next(1, out assoc, out items) == HResult.S_OK) {
					var h = assoc;
				}
			}
		}
		*/

		#endregion


		/// <summary>
		/// Converts a File/Folder path into a proper string used to create a <see cref="ShellItem"/>
		/// </summary>
		/// <param name="path">The path you want to convert</param>
		/// <returns></returns>
		public static ShellItem ToShellParsingName(String path) {
			if (path.IndexOf("::") == 0 && !path.StartsWith(@"\\")) {
				return new ShellItem(String.Format("shell:{0}", path));
			}
			else if (!path.EndsWith(Path.DirectorySeparatorChar.ToString())) {
				return new ShellItem(String.Format("{0}{1}", path, Path.DirectorySeparatorChar));
			}
			else {
				return new ShellItem(path);
			}
		} //TODO: Consider making this a constructor!

	}
}