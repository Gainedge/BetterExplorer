using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BExplorer.Shell.Interop;
using ThumbnailGenerator;

namespace BExplorer.Shell._Plugin_Interfaces {
	public class FileSystemListItem : IListItemEx {

		#region Private Members
		private ShellItem _Item { get; set; }
		#endregion

		#region IListItemEx Members
		public IShellItem ComInterface => this._Item.ComInterface;

		public string DisplayName { get; set; }

		public bool IsNeedRefreshing { get; set; }

		public bool IsInvalid { get; set; }

		public bool IsOnlyLowQuality { get; set; }

		public bool IsThumbnailLoaded { get; set; }

		public bool IsInitialised { get; set; }

		public int OverlayIconIndex { get; set; }

		public Interop.IExtractIconPWFlags IconType {
			get { return this.IsParentSearchFolder ? IExtractIconPWFlags.GIL_PERINSTANCE : this._Item.GetIconType(); }
			
		}

		public IntPtr ILPidl => this._Item.ILPidl;

		public IntPtr PIDL { get; set; }

		public IntPtr AbsolutePidl => this._Item.AbsolutePidl;

		public int ShieldedIconIndex { get; set; }

		public bool IsIconLoaded { get; set; }

		public string ParsingName { get; set; }

		public string Extension { get; set; }

		public string FileSystemPath { get; set; }

		public bool IsBrowsable { get; set; }

		public bool IsFolder { get; set; }

		public bool HasSubFolders => this._Item.HasSubFolders;

		public bool IsHidden { get; set; }

		public bool IsFileSystem { get; set; }

		public bool IsNetworkPath { get; set; }

		public bool IsDrive { get; set; }

		public bool IsShared { get; set; }

		public bool IsParentSearchFolder { get; set; }

		public Int32 GroupIndex { get; set; }

		public Int32 RCWThread { get; set; } 

		public IShellFolder IFolder { get; set; }

		private void Initialize_Helper(ShellItem folder, IntPtr lvHandle, int index) {
			this.DisplayName = folder.DisplayName;
			this.ParsingName = folder.ParsingName;
			this.ItemIndex = index;
			this.PIDL = folder.Pidl;
			//this.IFolder = folder.GetIShellFolder();
			this.ParentHandle = lvHandle;
			this.IsFileSystem = folder.IsFileSystem;
			this.IsNetworkPath = folder.IsNetworkPath;
			this.Extension = folder.Extension;
			this.IsDrive = folder.IsDrive;
			this.IsHidden = folder.IsHidden;
			this.OverlayIconIndex = -1;
			this.ShieldedIconIndex = -1;
			this.IsShared = folder.IsShared;
			//this.IconType = folder.IconType;
			this.IsFolder = folder.IsFolder;
			this.IsSearchFolder = folder.IsSearchFolder;
			this._Item = folder;
		}

		private void Initialize_Helper2(ShellItem parent, IntPtr pidl, IntPtr lvHandle, int index) {
			var folder = new ShellItem(parent, pidl);
			this.DisplayName = folder.DisplayName;
			this.ParsingName = folder.ParsingName;
			this.ItemIndex = index;
			this.PIDL = folder.Pidl;
			this.ParentHandle = lvHandle;
			this.IsFileSystem = folder.IsFileSystem;
			this.IsNetworkPath = folder.IsNetworkPath;
			this.Extension = folder.Extension;
			this.IsDrive = folder.IsDrive;
			this.IsHidden = folder.IsHidden;
			this.OverlayIconIndex = -1;
			this.ShieldedIconIndex = -1;
			this.IsShared = folder.IsShared;
			//this.IconType = folder.IconType;
			this.IsFolder = folder.IsFolder;
			this.IsSearchFolder = folder.IsSearchFolder;
			this._Item = folder;
		}

		public void Initialize(IntPtr lvHandle, IntPtr pidl, int index) {
			Initialize_Helper(new ShellItem(pidl), lvHandle, index);
		}

		public void InitializeWithParent(ShellItem parent, IntPtr lvHandle, IntPtr pidl, int index) {
			Initialize_Helper2(parent, pidl, lvHandle, index);
		}

		public void InitializeWithShellItem(ShellSearchFolder item, IntPtr lvHandle, int index) {
			Initialize_Helper(item, lvHandle, index);
			this.searchFolder = item;
		}

		public ShellSearchFolder searchFolder { get; set; }

		public Dictionary<Interop.PROPERTYKEY, object> ColumnValues { get; set; }


		public int ItemIndex { get; set; }

		public IntPtr ParentHandle { get; set; }

		public static IListItemEx InitializeWithIShellItem(IntPtr lvHandle, IShellItem item) {
			var fsItem = new FileSystemListItem();
			fsItem.Initialize(lvHandle, new ShellItem(item).Pidl, 0);
			return fsItem;
		}

		public void Initialize(IntPtr lvHandle, string path, int index) {
			var item = new ShellItem(path);
			this.DisplayName = item.DisplayName;
			this.ParsingName = item.ParsingName;
			this.ItemIndex = index;
			this.ParentHandle = lvHandle;
			this.IsFileSystem = item.IsFileSystem;
			this.IsNetworkPath = item.IsNetworkPath;
			this.Extension = item.Extension;
			this.IsDrive = item.IsDrive;
			this.IsHidden = item.IsHidden;
			this.OverlayIconIndex = -1;
			this.ShieldedIconIndex = -1;
			this.PIDL = item.Pidl;
			//this.IFolder = item.GetIShellFolder();
			//this.HasSubFolders = item.HasSubFolders;
			this.IsShared = item.IsShared;
			//this.IconType = item.IconType;
			this.IsFolder = item.IsFolder;
			this.IsSearchFolder = item.IsSearchFolder;
			this._Item = item;
			//item.Dispose();
		}

		public void Initialize(IntPtr lvHandle, string path) {
			throw new NotImplementedException();
		}

		public void Initialize(IntPtr lvHandle, IntPtr pidl) {
			throw new NotImplementedException();
		}

		public FileSystemListItem() {
			this.GroupIndex = -1;
			this.ItemIndex = -1;
		}

		IListItemEx[] IListItemEx.GetSubItems(bool isEnumHidden) {
			TaskScheduler taskScheduler = null;
			try {
				taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			} catch (InvalidOperationException) {

				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
				taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			}

			Task<FileSystemListItem[]> taskk = Task.Factory.StartNew(() => {
				var fsiList = new List<FileSystemListItem>();
				IShellFolder folder = this.GetIShellFolder();
				HResult navRes;
				IEnumIDList enumId = ShellItem.GetIEnumIDList(folder, SHCONTF.FOLDERS | SHCONTF.INCLUDEHIDDEN | SHCONTF.INCLUDESUPERHIDDEN | SHCONTF.FASTITEMS |
					SHCONTF.NONFOLDERS | SHCONTF.ENABLE_ASYNC, out navRes);
				this.NavigationStatus = navRes;
				uint count;
				IntPtr pidl;

				//if (enumId == null) {
				//  break;
				//}
				System.Windows.Forms.Application.DoEvents();
				//Thread.Sleep(1);
				//HResult result = enumId.Next(1, out pidl, out count);
				var i = 0;
				var parentItem = this.IsSearchFolder ? this._Item : new ShellItem(this.ParsingName.ToShellParsingName());

				while (enumId.Next(1, out pidl, out count) == HResult.S_OK) {
					var fsi = new FileSystemListItem();
					fsi.InitializeWithParent(parentItem, this.ParentHandle, pidl, i++);
					fsiList.Add(fsi);
					Shell32.ILFree(pidl);
					//if (this.IsSearchFolder)
					//	Thread.Sleep(1);
					//System.Windows.Forms.Application.DoEvents();
					//result = enumId.Next(1, out pidl, out count);
				}

				//if (result != HResult.S_FALSE) {
				//Marshal.ThrowExceptionForHR((int)result);
				//}

				parentItem.Dispose();
				return fsiList.ToArray();
				//yield break;
			}, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
			//taskk.Start(TaskScheduler.FromCurrentSynchronizationContext());
			taskk.Wait();
			return taskk.Result;
		}

		public HResult NavigationStatus { get; set; }
		public IEnumerator<IListItemEx> GetEnumerator() {
			IShellFolder folder = this.GetIShellFolder();
			HResult navRes;
			IEnumIDList enumId = ShellItem.GetIEnumIDList(folder, SHCONTF.FOLDERS | SHCONTF.INCLUDEHIDDEN | SHCONTF.INCLUDESUPERHIDDEN  | SHCONTF.FASTITEMS |
					SHCONTF.NONFOLDERS | SHCONTF.ENABLE_ASYNC, out navRes);
			this.NavigationStatus = navRes;
			uint count;
			IntPtr pidl;

			if (enumId == null) {
				yield break;
			}

			HResult result = enumId.Next(1, out pidl, out count);
			var i = 0;
			//var parentItem = new ShellItem(this._Item.Pidl); //this._Item;//this.IsSearchFolder ? this._Item : new ShellItem(this.ParsingName.ToShellParsingName());
			while (result == HResult.S_OK) {
				var fsi = new FileSystemListItem();
				fsi.InitializeWithParent(this.IsSearchFolder? this.searchFolder : this._Item, this.ParentHandle, pidl, i++);
				fsi.IsParentSearchFolder = this.IsSearchFolder;
				yield return fsi;
				Shell32.ILFree(pidl);
				result = enumId.Next(1, out pidl, out count);
			}

			if (result != HResult.S_FALSE) {
				//Marshal.ThrowExceptionForHR((int)result);
			}

			//parentItem.Dispose();
			yield break;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		//public PropVariant GetPropertyValue(PROPERTYKEY pkey, Type type) => this._Item.GetPropertyValue(pkey, type);
		public PropVariant GetPropertyValue(PROPERTYKEY pkey, Type type) => this._Item.GetPropertyValue(pkey);


		public System.Drawing.Bitmap Thumbnail(int size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source) =>
				this._Item.GetShellThumbnail(size, format, source);

		public System.Windows.Media.Imaging.BitmapSource ThumbnailSource(int size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source) {
			
			if (this.IsSearchFolder)
				this._Item.ComInterface = this.searchFolder.m_SearchComInterface;

			this._Item.Thumbnail.CurrentSize = new System.Windows.Size(size, size);
			this._Item.Thumbnail.RetrievalOption = source;
			this._Item.Thumbnail.FormatOption = format;
			return this._Item.Thumbnail.BitmapSource;
		}

		public bool IsSearchFolder { get; set; }

		public IListItemEx Parent {
			get {
				if (this.IsSearchFolder || this._Item.Parent == null) return null;

				var parent = new FileSystemListItem();
				parent.Initialize(this.ParentHandle, this._Item.Parent.Pidl, 0);
				return parent;
			}
		}

		public IShellFolder GetIShellFolder() => this._Item.GetIShellFolder();

		public bool IsLink => this._Item.IsLink;

		public string ToolTipText => this._Item.ToolTipText;

		public System.IO.DriveInfo GetDriveInfo() => this._Item.GetDriveInfo();


		public HResult ExtractAndDrawThumbnail(IntPtr hdc, uint iconSize, out WTS_CACHEFLAGS flags, User32.RECT iconBounds, out bool retrieved, bool isHidden, bool isRefresh = false) {
			return this._Item.Thumbnail.ExtractAndDrawThumbnail(hdc, iconSize, out flags, iconBounds, out retrieved, isHidden, isRefresh);
		}

		public IntPtr GetHBitmap(int iconSize, bool isThumbnail, bool isForce = false) {
			//var bmp = this._Item.Thumbnail.GetHBitmap(iconSize, isThumbnail, isForce);
			//return bmp;
			ThumbnailOptions options = ThumbnailOptions.None;
			if (isThumbnail) {
				options = ThumbnailOptions.ThumbnailOnly;
				if (!isForce)
					options |= ThumbnailOptions.InCacheOnly;
			} else {
				options |= ThumbnailOptions.IconOnly;
			}
			return WindowsThumbnailProvider.GetThumbnail(this.PIDL, iconSize, iconSize, options);
		}

		public static FileSystemListItem ToFileSystemItem(IntPtr parentHandle, String path) {
			var fsItem = new FileSystemListItem();
			fsItem.Initialize(parentHandle, path, 0);
			return fsItem;
		}

		public static FileSystemListItem ToFileSystemItem(IntPtr parentHandle, IntPtr pidl) {
			var fsItem = new FileSystemListItem();
			fsItem.Initialize(parentHandle, pidl, 0);
			return fsItem;
		}

		public static FileSystemListItem ToFileSystemItem(IntPtr parentHandle, ShellSearchFolder folder) {
			var fsItem = new FileSystemListItem();
			fsItem.InitializeWithShellItem(folder, parentHandle, 0);
			return fsItem;
		}

		public string GetDisplayName(SIGDN type) => this._Item.GetDisplayName(type);

		public IExtractIconPWFlags GetShield() => this._Item.GetShield();

		public int GetSystemImageListIndex(IntPtr pidl, ShellIconType type, ShellIconFlags flags) {
			var info = new SHFILEINFO();
			IntPtr result = Shell32.SHGetFileInfo(pidl, 0, out info, Marshal.SizeOf(info),
													SHGFI.Icon | SHGFI.SysIconIndex | SHGFI.OverlayIndex | SHGFI.PIDL | (SHGFI)type | (SHGFI)flags);

			if (result == IntPtr.Zero) {
				throw new Exception("Error retrieving shell folder icon");
			}

			User32.DestroyIcon(info.hIcon);
			return info.iIcon;
		}

		public Boolean RefreshThumb(int iconSize, out WTS_CACHEFLAGS flags) => this._Item.Thumbnail.RefreshThumbnail((uint)iconSize, out flags);


		public IntPtr Icon { get; set; }

		public int GetUniqueID() => this.ParsingName.GetHashCode();

		public Boolean IsRCWSet { get; set; }

		public void RecreateRCW() {
			//return;
			if (this.IsSearchFolder) return;
			//if (this.RCWThread != Thread.CurrentThread.ManagedThreadId) {
				this._Item = new ShellItem(this.ParsingName.ToShellParsingName());
			//	this.RCWThread = Thread.CurrentThread.ManagedThreadId;
			//}
		}

		#endregion

		#region IEquatable<IListItemEx> Members
		public bool Equals(IListItemEx other) {
			if (other == null) return false;
			return other.ParsingName.Equals(this.ParsingName, StringComparison.InvariantCultureIgnoreCase);
		}
		#endregion

		#region IEqualityComparer<IListItemEx> Members

		public bool Equals(IListItemEx x, IListItemEx y) => x.Equals(y);

		public int GetHashCode(IListItemEx obj) {
			//TODO: Deal With Dead Code!!
			return 0;
			if (Object.ReferenceEquals(obj, null)) return 0;

			//Get hash code for the Name field if it is not null.
			int hashProductName = obj.ParsingName == null ? 0 : obj.ParsingName.GetHashCode();

			////Get hash code for the Code field.
			//int hashProductCode = product.Code.GetHashCode();

			//Calculate the hash code for the product.
			return hashProductName;
		}

		#endregion

		#region IDisposable Members

		//public void Dispose() { if (this._Item != null) this._Item.Dispose(); }
		public void Dispose() => this._Item?.Dispose();

		#endregion

	}
}
