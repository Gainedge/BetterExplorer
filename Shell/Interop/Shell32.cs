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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

#pragma warning disable 1591

namespace BExplorer.Shell.Interop {

	public enum CSIDL {
		DESKTOP = 0x0000,
		INTERNET = 0x0001,
		PROGRAMS = 0x0002,
		CONTROLS = 0x0003,
		PRINTERS = 0x0004,
		PERSONAL = 0x0005,
		FAVORITES = 0x0006,
		STARTUP = 0x0007,
		RECENT = 0x0008,
		SENDTO = 0x0009,
		BITBUCKET = 0x000a,
		STARTMENU = 0x000b,
		MYDOCUMENTS = PERSONAL,
		MYMUSIC = 0x000d,
		MYVIDEO = 0x000e,
		DESKTOPDIRECTORY = 0x0010,
		DRIVES = 0x0011,
		NETWORK = 0x0012,
		NETHOOD = 0x0013,
		FONTS = 0x0014,
		TEMPLATES = 0x0015,
		COMMON_STARTMENU = 0x0016,
		COMMON_PROGRAMS = 0x0017,
		COMMON_STARTUP = 0x0018,
		COMMON_DESKTOPDIRECTORY = 0x0019,
		APPDATA = 0x001a,
		PRINTHOOD = 0x001b,
		LOCAL_APPDATA = 0x001c,
		ALTSTARTUP = 0x001d,
		COMMON_ALTSTARTUP = 0x001e,
		COMMON_FAVORITES = 0x001f,
		INTERNET_CACHE = 0x0020,
		COOKIES = 0x0021,
		HISTORY = 0x0022,
		COMMON_APPDATA = 0x0023,
		WINDOWS = 0x0024,
		SYSTEM = 0x0025,
		PROGRAM_FILES = 0x0026,
		MYPICTURES = 0x0027,
		PROFILE = 0x0028,
		SYSTEMX86 = 0x0029,
		PROGRAM_FILESX86 = 0x002a,
		PROGRAM_FILES_COMMON = 0x002b,
		PROGRAM_FILES_COMMONX86 = 0x002c,
		COMMON_TEMPLATES = 0x002d,
		COMMON_DOCUMENTS = 0x002e,
		COMMON_ADMINTOOLS = 0x002f,
		ADMINTOOLS = 0x0030,
		CONNECTIONS = 0x0031,
		COMMON_MUSIC = 0x0035,
		COMMON_PICTURES = 0x0036,
		COMMON_VIDEO = 0x0037,
		RESOURCES = 0x0038,
		RESOURCES_LOCALIZED = 0x0039,
		COMMON_OEM_LINKS = 0x003a,
		CDBURN_AREA = 0x003b,
		COMPUTERSNEARME = 0x003d,
	}

	/*
	public enum FFFP_MODE {
		EXACTMATCH,
		NEARESTPARENTMATCH,
	}
	*/

	[Flags]
	public enum FOLDERFLAGS {
		AutoArrange = 0x00000001,
		AbbreviatedNames = 0x00000002,
		SnapToGrid = 0x00000004,
		OwnerData = 0x00000008,
		BestFitWindow = 0x00000010,
		Desktop = 0x00000020,
		SingleSelection = 0x00000040,
		NoSubfolders = 0x00000080,
		Transparent = 0x00000100,
		NoClientEdge = 0x00000200,
		NoScroll = 0x00000400,
		AlignLeft = 0x00000800,
		NoIcons = 0x00001000,
		ShowSelectionAlways = 0x00002000,
		NoVisible = 0x00004000,
		SingleClickActivate = 0x00008000,
		NoWebView = 0x00010000,
		HideFilenames = 0x00020000,
		CheckSelect = 0x00040000,
		NoEnumRefresh = 0x00080000,
		NoGrouping = 0x00100000,
		FullRowSelect = 0x00200000,
		NoFilters = 0x00400000,
		NoColumnHeaders = 0x00800000,
		NoHeaderInAllViews = 0x01000000,
		ExtendedTiles = 0x02000000,
		TriCheckSelect = 0x04000000,
		AutoCheckSelect = 0x08000000,
		NoBrowserViewState = 0x10000000,
		SubsetGroups = 0x20000000,
		UseSearchFolders = 0x40000000,
		AllowRightToLeftReading = unchecked((int)0x80000000)
	}

	public enum FOLDERVIEWMODE : uint {
		FIRST = 1,
		ICON = 1,
		SMALLICON = 2,
		LIST = 3,
		DETAILS = 4,
		THUMBNAIL = 5,
		TILE = 6,
		THUMBSTRIP = 7,
		LAST = 7
	}

	public enum SHCONTF {
		CHECKING_FOR_CHILDREN = 0x00010,
		FOLDERS = 0x00020,
		NONFOLDERS = 0x00040,
		INCLUDEHIDDEN = 0x00080,
		INIT_ON_FIRST_NEXT = 0x00100,
		NETPRINTERSRCH = 0x00200,
		SHAREABLE = 0x00400,
		STORAGE = 0x00800,
		NAVIGATION_ENUM = 0x01000,
		FASTITEMS = 0x02000,
		SFLATLIST = 0x04000,
		ENABLE_ASYNC = 0x08000,
		INCLUDESUPERHIDDEN = 0x10000
	}

	[Flags]
	public enum SFGAO : uint {
		CANCOPY = 0x00000001,
		CANMOVE = 0x00000002,
		CANLINK = 0x00000004,
		STORAGE = 0x00000008,
		CANRENAME = 0x00000010,
		CANDELETE = 0x00000020,
		HASPROPSHEET = 0x00000040,
		DROPTARGET = 0x00000100,
		CAPABILITYMASK = 0x00000177,
		ENCRYPTED = 0x00002000,
		ISSLOW = 0x00004000,
		GHOSTED = 0x00008000,
		LINK = 0x00010000,
		SHARE = 0x00020000,
		READONLY = 0x00040000,
		HIDDEN = 0x00080000,
		DISPLAYATTRMASK = 0x000FC000,
		STREAM = 0x00400000,
		STORAGEANCESTOR = 0x00800000,
		VALIDATE = 0x01000000,
		REMOVABLE = 0x02000000,
		COMPRESSED = 0x04000000,
		BROWSABLE = 0x08000000,
		FILESYSANCESTOR = 0x10000000,
		FOLDER = 0x20000000,
		FILESYSTEM = 0x40000000,
		HASSUBFOLDER = 0x80000000,
		CONTENTSMASK = 0x80000000,
		STORAGECAPMASK = 0x70C50008,
	}

	[Flags]
	public enum SHCIDS : uint {
		ALLFIELDS = 0x80000000,
		CANONICALONLY = 0x10000000,
		BITMASK = 0xFFFF0000,
		COLUMNMASK = 0x0000FFFF,
	}

	public enum SHCNE : uint {
		RENAMEITEM = 0x00000001,
		CREATE = 0x00000002,
		DELETE = 0x00000004,
		MKDIR = 0x00000008,
		RMDIR = 0x00000010,
		MEDIAINSERTED = 0x00000020,
		MEDIAREMOVED = 0x00000040,
		DRIVEREMOVED = 0x00000080,
		DRIVEADD = 0x00000100,
		NETSHARE = 0x00000200,
		NETUNSHARE = 0x00000400,
		ATTRIBUTES = 0x00000800,
		UPDATEDIR = 0x00001000,
		UPDATEITEM = 0x00002000,
		SERVERDISCONNECT = 0x00004000,
		UPDATEIMAGE = 0x00008000,
		DRIVEADDGUI = 0x00010000,
		RENAMEFOLDER = 0x00020000,
		FREESPACE = 0x00040000,
		EXTENDED_EVENT = 0x04000000,
		ASSOCCHANGED = 0x08000000,
		DISKEVENTS = 0x0002381F,
		GLOBALEVENTS = 0x0C0581E0,
		ALLEVENTS = 0x7FFFFFFF,
		INTERRUPT = 0x80000000,
	}

	public enum SHCNRF {
		InterruptLevel = 0x0001,
		ShellLevel = 0x0002,
		RecursiveInterrupt = 0x1000,
		NewDelivery = 0x8000,
	}

	[Flags]
	public enum SHGFI : uint {
		/// <summary>get icon</summary>
		Icon = 0x000000100,
		/// <summary>get display name</summary>
		DisplayName = 0x000000200,
		/// <summary>get type name</summary>
		TypeName = 0x000000400,
		/// <summary>get attributes</summary>
		Attributes = 0x000000800,
		/// <summary>get icon location</summary>
		IconLocation = 0x000001000,
		/// <summary>return exe type</summary>
		ExeType = 0x000002000,
		/// <summary>get system icon index</summary>
		SysIconIndex = 0x000004000,
		/// <summary>put a link overlay on icon</summary>
		LinkOverlay = 0x000008000,
		/// <summary>show icon in selected state</summary>
		Selected = 0x000010000,
		/// <summary>get only specified attributes</summary>
		Attr_Specified = 0x000020000,
		/// <summary>get large icon</summary>
		LargeIcon = 0x000000000,
		/// <summary>get small icon</summary>
		SmallIcon = 0x000000001,
		/// <summary>get open icon</summary>
		OpenIcon = 0x000000002,
		/// <summary>get shell size icon</summary>
		ShellIconSize = 0x000000004,
		/// <summary>pszPath is a pidl</summary>
		PIDL = 0x000000008,
		/// <summary>use passed dwFileAttribute</summary>
		UseFileAttributes = 0x000000010,
		/// <summary>apply the appropriate overlays</summary>
		AddOverlays = 0x000000020,
		/// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
		OverlayIndex = 0x000000040,
	}

	public enum SHGNO {
		NORMAL = 0x0000,
		INFOLDER = 0x0001,
		FOREDITING = 0x1000,
		FORADDRESSBAR = 0x4000,
		FORPARSING = 0x8000,
	}

	public enum SICHINT : uint {
		DISPLAY = 0x00000000,
		CANONICAL = 0x10000000,
		ALLFIELDS = 0x80000000
	}

	public enum SIGDN : uint {
		NORMALDISPLAY = 0,
		PARENTRELATIVEPARSING = 0x80018001,
		PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
		DESKTOPABSOLUTEPARSING = 0x80028000,
		PARENTRELATIVEEDITING = 0x80031001,
		DESKTOPABSOLUTEEDITING = 0x8004c000,
		FILESYSPATH = 0x80058000,
		URL = 0x80068000
	}

	public enum SVSI : uint {
		DESELECT = 0x00000000,
		SELECT = 0x00000001,
		EDIT = 0x00000003,
		DESELECTOTHERS = 0x00000004,
		ENSUREVISIBLE = 0x00000008,
		FOCUSED = 0x00000010,
		TRANSLATEPT = 0x00000020,
		SELECTIONMARK = 0x00000040,
		SPOSITIONITEM = 0x00000080,
		CHECK = 0x00000100,
		CHECK2 = 0x00000200,
		SKEYBOARDSELECT = 0x00000401,
		NOTAKEFOCUS = 0x40000000
	}

	public enum ERROR {
		SUCCESS,
		FILE_EXISTS = 80,
		BAD_PATHNAME = 161,
		ALREADY_EXISTS = 183,
		FILENAME_EXCED_RANGE = 206,
		CANCELLED = 1223,
	}

	public struct FOLDERSETTINGS {
		public FOLDERVIEWMODE ViewMode;
		public FOLDERFLAGS fFlags;
	}

	/*
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct KNOWNFOLDER_DEFINITION {
		public int category;
		public IntPtr pszName;
		public IntPtr pszDescription;
		public Guid fidParent;
		public IntPtr pszRelativePath;
		public IntPtr pszParsingName;
		public IntPtr pszTooltip;
		public IntPtr pszLocalizedName;
		public IntPtr pszIcon;
		public IntPtr pszSecurity;
		public uint dwAttributes;
		public int kfdFlags;
		public Guid ftidType;
	}
	*/

	public struct SHChangeNotifyEntry {
		public IntPtr pidl;
		public bool fRecursive;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct SHFILEINFO {
		public IntPtr hIcon;
		public int iIcon;
		public uint dwAttributes;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	public struct SHNOTIFYSTRUCT {
		public IntPtr dwItem1;
		public IntPtr dwItem2;
	}

	[StructLayout(LayoutKind.Explicit, Size = 264)]
	public struct STRRET {
		[FieldOffset(0)]
		public UInt32 uType;
		[FieldOffset(4)]
		public IntPtr pOleStr;
		[FieldOffset(4)]
		public IntPtr pStr;
		[FieldOffset(4)]
		public UInt32 uOffset;
		[FieldOffset(4)]
		public IntPtr cStr;
	}

	public static class Shell32 {


		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr SHChangeNotification_Lock(IntPtr windowHandle, int processId, out IntPtr pidl, out uint lEvent);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern Boolean SHChangeNotification_Unlock(IntPtr hLock);


		[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		public static extern void SHCreateShellItemArrayFromDataObject(
			[In] System.Runtime.InteropServices.ComTypes.IDataObject pdo,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppv);

		[DllImport("Shell32", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		internal static extern int SHShowManageLibraryUI(
			[In, MarshalAs(UnmanagedType.Interface)] IShellItem library,
			[In] IntPtr hwndOwner,
			[In] string title,
			[In] string instruction,
			[In] LibraryManageDialogOptions lmdOptions);

		/// <summary>
		/// Describes the event that has occurred.
		/// Typically, only one event is specified at a time.
		/// If more than one event is specified, the values contained
		/// in the <i>dwItem1</i> and <i>dwItem2</i>
		/// parameters must be the same, respectively, for all specified events.
		/// This parameter can be one or more of the following values.
		/// </summary>
		/// <remarks>
		/// <para><b>Windows NT/2000/XP:</b> <i>dwItem2</i> contains the index
		/// in the system image list that has changed.
		/// <i>dwItem1</i> is not used and should be <see langword="null"/>.</para>
		/// <para><b>Windows 95/98:</b> <i>dwItem1</i> contains the index
		/// in the system image list that has changed.
		/// <i>dwItem2</i> is not used and should be <see langword="null"/>.</para>
		/// </remarks>
		[Flags]
		public enum HChangeNotifyEventID {
			/// <summary>
			/// All events have occurred.
			/// </summary>
			SHCNE_ALLEVENTS = 0x7FFFFFFF,

			/// <summary>
			/// A file type association has changed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
			/// must be specified in the <i>uFlags</i> parameter.
			/// <i>dwItem1</i> and <i>dwItem2</i> are not used and must be <see langword="null"/>.
			/// </summary>
			SHCNE_ASSOCCHANGED = 0x08000000,

			/// <summary>
			/// The attributes of an item or folder have changed.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the item or folder that has changed.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_ATTRIBUTES = 0x00000800,

			/// <summary>
			/// A nonfolder item has been created.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the item that was created.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_CREATE = 0x00000002,

			/// <summary>
			/// A nonfolder item has been deleted.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the item that was deleted.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_DELETE = 0x00000004,

			/// <summary>
			/// A drive has been added.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the root of the drive that was added.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_DRIVEADD = 0x00000100,

			/// <summary>
			/// A drive has been added and the Shell should create a new window for the drive.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the root of the drive that was added.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_DRIVEADDGUI = 0x00010000,

			/// <summary>
			/// A drive has been removed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the root of the drive that was removed.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_DRIVEREMOVED = 0x00000080,

			/// <summary>
			/// Not currently used.
			/// </summary>
			SHCNE_EXTENDED_EVENT = 0x04000000,

			/// <summary>
			/// The amount of free space on a drive has changed.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the root of the drive on which the free space changed.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_FREESPACE = 0x00040000,

			/// <summary>
			/// Storage media has been inserted into a drive.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the root of the drive that contains the new media.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_MEDIAINSERTED = 0x00000020,

			/// <summary>
			/// Storage media has been removed from a drive.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the root of the drive from which the media was removed.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_MEDIAREMOVED = 0x00000040,

			/// <summary>
			/// A folder has been created. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
			/// or <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the folder that was created.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_MKDIR = 0x00000008,

			/// <summary>
			/// A folder on the local computer is being shared via the network.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the folder that is being shared.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_NETSHARE = 0x00000200,

			/// <summary>
			/// A folder on the local computer is no longer being shared via the network.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the folder that is no longer being shared.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_NETUNSHARE = 0x00000400,

			/// <summary>
			/// The name of a folder has changed.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the previous pointer to an item identifier list (PIDL) or name of the folder.
			/// <i>dwItem2</i> contains the new PIDL or name of the folder.
			/// </summary>
			SHCNE_RENAMEFOLDER = 0x00020000,

			/// <summary>
			/// The name of a nonfolder item has changed.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the previous PIDL or name of the item.
			/// <i>dwItem2</i> contains the new PIDL or name of the item.
			/// </summary>
			SHCNE_RENAMEITEM = 0x00000001,

			/// <summary>
			/// A folder has been removed.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the folder that was removed.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_RMDIR = 0x00000010,

			/// <summary>
			/// The computer has disconnected from a server.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the server from which the computer was disconnected.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// </summary>
			SHCNE_SERVERDISCONNECT = 0x00004000,

			/// <summary>
			/// The contents of an existing folder have changed,
			/// but the folder still exists and has not been renamed.
			/// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
			/// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
			/// <i>dwItem1</i> contains the folder that has changed.
			/// <i>dwItem2</i> is not used and should be <see langword="null"/>.
			/// If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or
			/// SHCNE_RENAMEFOLDER, respectively, instead.
			/// </summary>
			SHCNE_UPDATEDIR = 0x00001000,

			/// <summary>
			/// An image in the system image list has changed.
			/// <see cref="HChangeNotifyFlags.SHCNF_DWORD"/> must be specified in <i>uFlags</i>.
			/// </summary>
			SHCNE_UPDATEIMAGE = 0x00008000,

			SHCNE_UPDATEITEM = 0x00002000,


		}

		/// <summary>
		/// Flags that indicate the meaning of the <i>dwItem1</i> and <i>dwItem2</i> parameters.
		/// The uFlags parameter must be one of the following values.
		/// </summary>
		[Flags]
		public enum HChangeNotifyFlags {
			/// <summary>
			/// The <i>dwItem1</i> and <i>dwItem2</i> parameters are DWORD values.
			/// </summary>
			SHCNF_DWORD = 0x0003,
			/// <summary>
			/// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of ITEMIDLIST structures that
			/// represent the item(s) affected by the change.
			/// Each ITEMIDLIST must be relative to the desktop folder.
			/// </summary>
			SHCNF_IDLIST = 0x0000,
			/// <summary>
			/// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
			/// maximum length MAX_PATH that contain the full path names
			/// of the items affected by the change.
			/// </summary>
			SHCNF_PATHA = 0x0001,
			/// <summary>
			/// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
			/// maximum length MAX_PATH that contain the full path names
			/// of the items affected by the change.
			/// </summary>
			SHCNF_PATHW = 0x0005,
			/// <summary>
			/// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
			/// represent the friendly names of the printer(s) affected by the change.
			/// </summary>
			SHCNF_PRINTERA = 0x0002,
			/// <summary>
			/// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
			/// represent the friendly names of the printer(s) affected by the change.
			/// </summary>
			SHCNF_PRINTERW = 0x0006,
			/// <summary>
			/// The function should not return until the notification
			/// has been delivered to all affected components.
			/// As this flag modifies other data-type flags, it cannot by used by itself.
			/// </summary>
			SHCNF_FLUSH = 0x1000,
			/// <summary>
			/// The function should begin delivering notifications to all affected components
			/// but should return as soon as the notification process has begun.
			/// As this flag modifies other data-type flags, it cannot by used by itself.
			/// </summary>
			SHCNF_FLUSHNOWAIT = 0x2000
		}

		/// <summary>
		/// Notifies the system of an event that an application has performed. 
		/// An application should use this function if it performs an action that may affect the Shell.
		/// </summary>
		/// <param name="wEventId">Describes the event that has occurred. Typically, only one event is specified at a time. 
		/// If more than one event is specified, the values contained in the dwItem1 and dwItem2 parameters must be the same, respectively, for all specified events. This parameter can be one or more of the following values.</param>
		/// <param name="uFlags">Flags that, when combined bitwise with SHCNF_TYPE, indicate the meaning of the dwItem1 and dwItem2 parameters.</param>
		/// <param name="dwItem1">Optional. First event-dependent value.</param>
		/// <param name="dwItem2">Optional. Second event-dependent value.</param>
		[DllImport("shell32.dll")]
		public static extern void SHChangeNotify(HChangeNotifyEventID wEventId, HChangeNotifyFlags uFlags, IntPtr dwItem1, IntPtr dwItem2);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int Shell_GetCachedImageIndex(string pwszIconPath, int iIconIndex, uint uIconFlags);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern void SHUpdateImage(string pszHashItem, int iIndex, uint uFlags, int iImageIndex);

		[DllImport("Shell32.dll", CharSet = CharSet.Auto)]
		public static extern HResult SHGetSetFolderCustomSettings(ref LPSHFOLDERCUSTOMSETTINGS pfcs, string pszPath, UInt32 dwReadWrite);

		[DllImport("shell32.dll", SetLastError = true)]
		public static extern bool SHObjectProperties(IntPtr hwnd, uint shopObjectType, [MarshalAs(UnmanagedType.LPWStr)] string pszObjectName, [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyPage);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct LPSHFOLDERCUSTOMSETTINGS {
			public UInt32 dwSize;
			public UInt32 dwMask;
			public IntPtr pvid;
			public string pszWebViewTemplate;
			public UInt32 cchWebViewTemplate;
			public string pszWebViewTemplateVersion;
			public string pszInfoTip;
			public UInt32 cchInfoTip;
			public IntPtr pclsid;
			public UInt32 dwFlags;
			public string pszIconFile;
			public UInt32 cchIconFile;
			public int iIconIndex;
			public string pszLogo;
			public UInt32 cchLogo;
		}

		public static UInt32 FCS_READ = 0x00000001;
		public static UInt32 FCS_FORCEWRITE = 0x00000002;
		public static UInt32 FCS_WRITE = FCS_READ | FCS_FORCEWRITE;

		public static UInt32 FCSM_VIEWID = 0x00000001;    // deprecated
		public static UInt32 FCSM_WEBVIEWTEMPLATE = 0x00000002;  // deprecated
		public static UInt32 FCSM_INFOTIP = 0x00000004;
		public static UInt32 FCSM_CLSID = 0x00000008;
		public static UInt32 FCSM_ICONFILE = 0x00000010;
		public static UInt32 FCSM_LOGO = 0x00000020;
		public static UInt32 FCSM_FLAGS = 0x00000040;

		//public static UInt32 SHGFI_ICONLOCATION = 0x000001000;

		[DllImport("shell32.dll", EntryPoint = "#660")]
		public static extern bool FileIconInit(bool bFullInit);

		[DllImport("shell32.dll", EntryPoint = "#18")]
		public static extern IntPtr ILClone(IntPtr pidl);

		[DllImport("shell32.dll", EntryPoint = "#25")]
		public static extern IntPtr ILCombine(IntPtr pidl1, IntPtr pidl2);

		/*
		[DllImport("shell32.dll")]
		public static extern IntPtr ILCreateFromPath(string pszPath);
		*/

		[DllImport("shell32.dll", EntryPoint = "#16")]
		public static extern IntPtr ILFindLastID(IntPtr pidl);

		[DllImport("shell32.dll", EntryPoint = "#155")]
		public static extern void ILFree(IntPtr pidl);

		[DllImport("shell32.dll", EntryPoint = "#21")]
		public static extern bool ILIsEqual(IntPtr pidl1, IntPtr pidl2);

		[DllImport("shell32.dll", EntryPoint = "#23")]
		public static extern bool ILIsParent(IntPtr pidl1, IntPtr pidl2, bool fImmediate);

		[DllImport("shell32.dll", EntryPoint = "#17")]
		public static extern bool ILRemoveLastID(IntPtr pidl);

		[DllImport("shell32.dll", EntryPoint = "#71")]
		public static extern bool Shell_GetImageLists(out IntPtr lphimlLarge, out IntPtr lphimlSmall);

		/*
		[System.Runtime.InteropServices.DllImport("Kernel32.dll")]
		public static extern Boolean CloseHandle(IntPtr handle);
		*/

		[DllImport("shell32.dll", EntryPoint = "#2")]
		public static extern uint SHChangeNotifyRegister(IntPtr hWnd, SHCNRF fSources, SHCNE fEvents, uint wMsg, int cEntries, ref SHChangeNotifyEntry pFsne);

		[DllImport("shell32.dll", EntryPoint = "#4")]
		public static extern bool SHChangeNotifyUnregister(uint hNotify);

		[DllImport("shell32.dll", EntryPoint = "#165", CharSet = CharSet.Unicode)]
		public static extern ERROR SHCreateDirectory(IntPtr hwnd, string pszPath);

		[DllImport("shell32.dll", PreserveSig = false)]
		public static extern IShellItem SHCreateItemFromIDList(
				[In] IntPtr pidl,
				[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		public static extern IShellItem SHCreateItemFromParsingName(
				[In] string pszPath,
				[In] IntPtr pbc,
				[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		public static extern IShellItem SHCreateItemWithParent(
				[In] IntPtr pidlParent,
				[In] IShellFolder psfParent,
				[In] IntPtr pidl,
				[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

		[DllImport("shell32.dll", PreserveSig = false)]
		public static extern IShellFolder SHGetDesktopFolder();

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SHGetFileInfo(IntPtr pszPath, int dwFileAttributes, out SHFILEINFO psfi, int cbFileInfo, SHGFI uFlags);

		[DllImport("shfolder.dll")]
		public static extern HResult SHGetFolderPath(
				[In] IntPtr hwndOwner,
				[In] CSIDL nFolder,
				[In] IntPtr hToken,
				[In] uint dwFlags,
				[Out] StringBuilder pszPath);

		[DllImport("shell32.dll")]
		public static extern HResult SHDoDragDrop(IntPtr hwnd, IDataObject pdtobj, IDropSource pdsrc, uint dwEffect, out uint pdwEffect);

		public enum ASSOC_FILTER {
			ASSOC_FILTER_NONE = 0x00000000,
			ASSOC_FILTER_RECOMMENDED = 0x00000001
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
		public delegate int funcNext(IntPtr refer, int celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 1)] IntPtr[] rgelt, [Out] out int pceltFetched);

		[UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
		public delegate int funcGetName(IntPtr refer, [Out, MarshalAs(UnmanagedType.LPWStr)] out String ppsz);

		[DllImport("Shell32", EntryPoint = "SHAssocEnumHandlers", CharSet = CharSet.Unicode)]
		public extern static HResult SHAssocEnumHandlers([MarshalAs(UnmanagedType.LPWStr)] string pszExtra, [In] ASSOC_FILTER afFilter, out IntPtr ppEnumHandler);

		[DllImport("shell32.dll", PreserveSig = false)]
		public static extern IntPtr SHGetIDListFromObject([In, MarshalAs(UnmanagedType.IUnknown)] object punk);

		[DllImport("shell32.dll")]
		public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [Out] StringBuilder pszPath);

		[DllImport("shell32.dll")]
		public static extern HResult SHGetSpecialFolderLocation(IntPtr hwndOwner, CSIDL nFolder, out IntPtr ppidl);


		[DllImport("shell32.dll", SetLastError = true)]
		public static extern int SHMultiFileProperties(System.Runtime.InteropServices.ComTypes.IDataObject pdtobj, int flags);
		/*
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
		

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHELLEXECUTEINFO {
			public int cbSize;
			public uint fMask;
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpVerb;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpFile;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpParameters;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpDirectory;
			public int nShow;
			public IntPtr hInstApp;
			public IntPtr lpIDList;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpClass;
			public IntPtr hkeyClass;
			public uint dwHotKey;
			public IntPtr hIcon;
			public IntPtr hProcess;
		}
		*/

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		public static extern int SHEmptyRecycleBin(IntPtr hWnd, string pszRootPath, uint dwFlags);

		[DllImport("shell32.dll", SetLastError = true)]
		static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

		public static string[] CommandLineToArgs(string commandLine) {
			int argc;
			var argv = CommandLineToArgvW(commandLine, out argc);
			if (argv == IntPtr.Zero)
				throw new System.ComponentModel.Win32Exception();
			try {
				var args = new string[argc];
				for (var i = 0; i < args.Length; i++) {
					var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
					args[i] = Marshal.PtrToStringUni(p);
				}
				return args;
			} finally {
				Marshal.FreeHGlobal(argv);
			}
		}

		[DllImport("shell32.dll")]
		public static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out()] out IntPtr pidl, uint sfgaoIn, [Out()] out uint psfgaoOut);

		[DllImport("shell32.dll")]
		public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

		/*
		[DllImport("shell32.dll")]
		public static extern void GetCurrentProcessExplicitAppUserModelID([Out(), MarshalAs(UnmanagedType.LPWStr)] out string AppID);
		*/

		public enum SHSTOCKICONID : uint {
			SIID_DOCNOASSOC = 0,
			SIID_DOCASSOC = 1,
			SIID_APPLICATION = 2,
			SIID_FOLDER = 3,
			SIID_FOLDEROPEN = 4,
			SIID_DRIVE525 = 5,
			SIID_DRIVE35 = 6,
			SIID_DRIVEREMOVE = 7,
			SIID_DRIVEFIXED = 8,
			SIID_DRIVENET = 9,
			SIID_DRIVENETDISABLED = 10,
			SIID_DRIVECD = 11,
			SIID_DRIVERAM = 12,
			SIID_WORLD = 13,
			SIID_SERVER = 15,
			SIID_PRINTER = 16,
			SIID_MYNETWORK = 17,
			SIID_FIND = 22,
			SIID_HELP = 23,
			SIID_SHARE = 28,
			SIID_LINK = 29,
			SIID_SLOWFILE = 30,
			SIID_RECYCLER = 31,
			SIID_RECYCLERFULL = 32,
			SIID_MEDIACDAUDIO = 40,
			SIID_LOCK = 47,
			SIID_AUTOLIST = 49,
			SIID_PRINTERNET = 50,
			SIID_SERVERSHARE = 51,
			SIID_PRINTERFAX = 52,
			SIID_PRINTERFAXNET = 53,
			SIID_PRINTERFILE = 54,
			SIID_STACK = 55,
			SIID_MEDIASVCD = 56,
			SIID_STUFFEDFOLDER = 57,
			SIID_DRIVEUNKNOWN = 58,
			SIID_DRIVEDVD = 59,
			SIID_MEDIADVD = 60,
			SIID_MEDIADVDRAM = 61,
			SIID_MEDIADVDRW = 62,
			SIID_MEDIADVDR = 63,
			SIID_MEDIADVDROM = 64,
			SIID_MEDIACDAUDIOPLUS = 65,
			SIID_MEDIACDRW = 66,
			SIID_MEDIACDR = 67,
			SIID_MEDIACDBURN = 68,
			SIID_MEDIABLANKCD = 69,
			SIID_MEDIACDROM = 70,
			SIID_AUDIOFILES = 71,
			SIID_IMAGEFILES = 72,
			SIID_VIDEOFILES = 73,
			SIID_MIXEDFILES = 74,
			SIID_FOLDERBACK = 75,
			SIID_FOLDERFRONT = 76,
			SIID_SHIELD = 77,
			SIID_WARNING = 78,
			SIID_INFO = 79,
			SIID_ERROR = 80,
			SIID_KEY = 81,
			SIID_SOFTWARE = 82,
			SIID_RENAME = 83,
			SIID_DELETE = 84,
			SIID_MEDIAAUDIODVD = 85,
			SIID_MEDIAMOVIEDVD = 86,
			SIID_MEDIAENHANCEDCD = 87,
			SIID_MEDIAENHANCEDDVD = 88,
			SIID_MEDIAHDDVD = 89,
			SIID_MEDIABLURAY = 90,
			SIID_MEDIAVCD = 91,
			SIID_MEDIADVDPLUSR = 92,
			SIID_MEDIADVDPLUSRW = 93,
			SIID_DESKTOPPC = 94,
			SIID_MOBILEPC = 95,
			SIID_USERS = 96,
			SIID_MEDIASMARTMEDIA = 97,
			SIID_MEDIACOMPACTFLASH = 98,
			SIID_DEVICECELLPHONE = 99,
			SIID_DEVICECAMERA = 100,
			SIID_DEVICEVIDEOCAMERA = 101,
			SIID_DEVICEAUDIOPLAYER = 102,
			SIID_NETWORKCONNECT = 103,
			SIID_INTERNET = 104,
			SIID_ZIPFILE = 105,
			SIID_SETTINGS = 106,
			SIID_DRIVEHDDVD = 132,
			SIID_DRIVEBD = 133,
			SIID_MEDIAHDDVDROM = 134,
			SIID_MEDIAHDDVDR = 135,
			SIID_MEDIAHDDVDRAM = 136,
			SIID_MEDIABDROM = 137,
			SIID_MEDIABDR = 138,
			SIID_MEDIABDRE = 139,
			SIID_CLUSTEREDDRIVE = 140,
			SIID_MAX_ICONS = 175

		}

		[Flags]
		public enum SHGSI : uint {
			SHGSI_ICONLOCATION = 0,
			SHGSI_ICON = 0x000000100,
			SHGSI_SYSICONINDEX = 0x000004000,
			SHGSI_LINKOVERLAY = 0x000008000,
			SHGSI_SELECTED = 0x000010000,
			SHGSI_LARGEICON = 0x000000000,
			SHGSI_SMALLICON = 0x000000001,
			SHGSI_SHELLICONSIZE = 0x000000004
		}

		[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SHSTOCKICONINFO {
			public UInt32 cbSize;
			public IntPtr hIcon;
			public Int32 iSysIconIndex;
			public Int32 iIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szPath;
		}

		[DllImport("Shell32.dll", SetLastError = false)]
		public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

		internal static IntPtr PidlFromParsingName(string name) {
			IntPtr pidl = IntPtr.Zero;

			uint sfgao;
			try {
				SHParseDisplayName(
									name, IntPtr.Zero, out pidl, 0,
									out sfgao);
			} catch (Exception) {


			}

			return pidl;
		}

		/*
		internal static IntPtr PidlFromShellItem(IShellItem nativeShellItem) {
			IntPtr unknown = Marshal.GetIUnknownForObject(nativeShellItem);
			return PidlFromUnknown(unknown);
		}

		internal static IntPtr PidlFromUnknown(IntPtr unknown) {
			try {
				IntPtr pidl = SHGetIDListFromObject(unknown);
				return pidl;
			}
			catch (Exception) {

				return IntPtr.Zero;
			}
		}
		*/

		[DllImport("kernel32.dll")]
		public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

		[DllImport("shell32.dll")]
		public extern static void SHGetSetSettings(ref SHELLSTATE lpss, SSF dwMask, bool bSet);

		/*
		[DllImport("shell32.dll", EntryPoint = "#162", CharSet = CharSet.Unicode)]
		public static extern IntPtr SHSimpleIDListFromPath(string szPath);
		*/

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint ILGetSize(IntPtr pidl);

		/// <summary>
		/// Formats a Drive
		/// </summary>
		/// <param name="hwnd"></param>
		/// <param name="drive"></param>
		/// <param name="fmtID"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		[DllImport("shell32.dll")]
		private static extern uint SHFormatDrive(IntPtr hwnd, uint drive, SHFormatFlags fmtID, SHFormatOptions options);

		public enum SHFormatFlags : uint {
			SHFMT_ID_DEFAULT = 0xFFFF,
		}

		[Flags]
		public enum SHFormatOptions : uint {
			SHFMT_OPT_FULL = 0x1,
			SHFMT_OPT_SYSONLY = 0x2,
		}

		/*
		public const uint SHFMT_ERROR = 0xFFFFFFFF;
		public const uint SHFMT_CANCEL = 0xFFFFFFFE;
		public const uint SHFMT_NOFORMAT = 0xFFFFFFD;
		*/

		/// <summary>
		/// Format a Drive by given Drive letter
		/// </summary>
		/// <param name="DriveLetter">The Drive letter</param>
		/// <returns>Error or Success Code</returns>
		public static uint FormatDrive(IntPtr Handle, string DriveLetter) {
			DriveInfo drive = new DriveInfo(DriveLetter);
			byte[] bytes = Encoding.ASCII.GetBytes(drive.Name.ToCharArray());
			uint driveNumber = Convert.ToUInt32(bytes[0] - Encoding.ASCII.GetBytes(new[] { 'A' })[0]);
			uint Result = SHFormatDrive(Handle, driveNumber, SHFormatFlags.SHFMT_ID_DEFAULT,
					 SHFormatOptions.SHFMT_OPT_FULL);
			return Result;
		}
		[DllImport("kernel32.dll")]
		public static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
		[DllImport("kernel32.dll")]
		public static extern IntPtr GlobalFree(IntPtr hMem);
		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
		static extern void CopyMemory(IntPtr destination, NETRESOURCE source, uint length);
		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
		public static extern bool PathIsNetworkPath(string pszPath);
		public enum ResourceScope {
			RESOURCE_CONNECTED = 1,
			RESOURCE_GLOBALNET,
			RESOURCE_REMEMBERED,
			RESOURCE_RECENT,
			RESOURCE_CONTEXT
		}
		public enum ResourceType {
			RESOURCETYPE_ANY,
			RESOURCETYPE_DISK,
			RESOURCETYPE_PRINT,
			RESOURCETYPE_RESERVED
		}
		public enum ResourceUsage {
			RESOURCEUSAGE_CONNECTABLE = 0x00000001,
			RESOURCEUSAGE_CONTAINER = 0x00000002,
			RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
			RESOURCEUSAGE_SIBLING = 0x00000008,
			RESOURCEUSAGE_ATTACHED = 0x00000010,
			RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
		}
		public enum ResourceDisplayType {
			RESOURCEDISPLAYTYPE_GENERIC,
			RESOURCEDISPLAYTYPE_DOMAIN,
			RESOURCEDISPLAYTYPE_SERVER,
			RESOURCEDISPLAYTYPE_SHARE,
			RESOURCEDISPLAYTYPE_FILE,
			RESOURCEDISPLAYTYPE_GROUP,
			RESOURCEDISPLAYTYPE_NETWORK,
			RESOURCEDISPLAYTYPE_ROOT,
			RESOURCEDISPLAYTYPE_SHAREADMIN,
			RESOURCEDISPLAYTYPE_DIRECTORY,
			RESOURCEDISPLAYTYPE_TREE,
			RESOURCEDISPLAYTYPE_NDSCONTAINER
		}
		[StructLayout(LayoutKind.Sequential)]
		public class NETRESOURCE {
			public ResourceScope dwScope = 0;
			public ResourceType dwType = 0;
			public ResourceDisplayType dwDisplayType = 0;
			public ResourceUsage dwUsage = 0;
			public string lpLocalName = null;
			public IntPtr lpRemoteName = IntPtr.Zero;
			public string lpComment = null;
			public string lpProvider = null;
		}

		[DllImport("Ntshrui.dll")]
		public static extern HResult ShowShareFolderUI(IntPtr hwndParent, IntPtr pszPath);
		[Flags]
		public enum DisconnectDialogFlags :
		int {
			UpdateProfile = 0x00000001,
			NoForce = 0x00000040,
		}
		[Flags]
		public enum ConnectDialogFlags :
		int {
			ReadOnlyPath = 0x00000001,
			// Conn_point = 0x00000002,
			UseMru = 0x00000004,
			HideBox = 0x00000008,
			Persist = 0x00000010,
			NotPersist = 0x00000020,
		}
		public struct ConnectDialogInfo {
			public uint StructureSize;
			public IntPtr Owner;
			public IntPtr ConnectResource;
			public ConnectDialogFlags Flags;
			public int DeviceNumber;
		}
		[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct DisconnectDialogInfo {
			public uint StructureSize;
			public IntPtr Owner;
			[MarshalAs(UnmanagedType.LPTStr, SizeConst = 200)]
			public string LocalName;
			[MarshalAs(UnmanagedType.LPTStr, SizeConst = 200)]
			public string RemoteName;
			public DisconnectDialogFlags Flags;
		}
		[DllImport("mpr.dll", EntryPoint = "WNetConnectionDialog", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int WNetConnectionDialog(IntPtr whnd, int dwType);
		[DllImport("mpr.dll", CharSet = CharSet.Auto)]
		public static extern int WNetConnectionDialog1(ConnectDialogInfo connDlgStruct);
		/// <summary>
		/// Disconnect Mapped Network Drive dialog
		/// </summary>
		/// <param name="whnd"></param>
		/// <param name="dwType"></param>
		/// <returns></returns>
		[DllImport("mpr.dll", EntryPoint = "WNetDisconnectDialog", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int WNetDisconnectDialog(IntPtr whnd, int dwType);
		[DllImport("mpr.dll", CharSet = CharSet.Auto)]
		public static extern int WNetDisconnectDialog1(DisconnectDialogInfo disConnDlgStruct);
		public static void MapDrive(IntPtr owner, String remotePath) {
			if (String.IsNullOrEmpty(remotePath) || !PathIsNetworkPath(remotePath)) {
				WNetConnectionDialog(owner, 1);
			} else {
				ConnectDialogInfo info = new ConnectDialogInfo();
				info.Owner = owner;
				info.Flags = ConnectDialogFlags.Persist | ConnectDialogFlags.ReadOnlyPath;
				NETRESOURCE res = new NETRESOURCE();
				res.dwType = ResourceType.RESOURCETYPE_DISK;
				res.lpRemoteName = Marshal.StringToHGlobalAuto(remotePath);
				IntPtr resptr = GlobalAlloc(0x0040 | 0x0000, (UIntPtr)Marshal.SizeOf(res));
				CopyMemory(resptr, res, (uint)Marshal.SizeOf(res));
				info.ConnectResource = resptr;
				info.StructureSize = (uint)Marshal.SizeOf(info);
				var result = WNetConnectionDialog1(info);
				GlobalFree(resptr);
			}
		}

		[Flags]
		public enum SSF {
			SSF_SHOWALLOBJECTS = 0x00000001,
			SSF_SHOWEXTENSIONS = 0x00000002,
			SSF_HIDDENFILEEXTS = 0x00000004,
			SSF_SERVERADMINUI = 0x00000004,
			SSF_SHOWCOMPCOLOR = 0x00000008,
			SSF_SORTCOLUMNS = 0x00000010,
			SSF_SHOWSYSFILES = 0x00000020,
			SSF_DOUBLECLICKINWEBVIEW = 0x00000080,
			SSF_SHOWATTRIBCOL = 0x00000100,
			SSF_DESKTOPHTML = 0x00000200,
			SSF_WIN95CLASSIC = 0x00000400,
			SSF_DONTPRETTYPATH = 0x00000800,
			SSF_SHOWINFOTIP = 0x00002000,
			SSF_MAPNETDRVBUTTON = 0x00001000,
			SSF_NOCONFIRMRECYCLE = 0x00008000,
			SSF_HIDEICONS = 0x00004000,
			SSF_FILTER = 0x00010000,
			SSF_WEBVIEW = 0x00020000,
			SSF_SHOWSUPERHIDDEN = 0x00040000,
			SSF_SEPPROCESS = 0x00080000,
			SSF_NONETCRAWLING = 0x00100000,
			SSF_STARTPANELON = 0x00200000,
			SSF_SHOWSTARTPAGE = 0x00400000,
			SSF_SHOWSTATUSBAR = 0x04000000,
			SSF_AUTOCHECKSELECT = 0x00800000,
		}

		[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct SHELLSTATE {

			public uint bitvector1;

			/// DWORD->unsigned int
			public uint dwWin95Unused;

			/// UINT->unsigned int
			public uint uWin95Unused;

			/// LONG->int
			public int lParamSort;

			/// int
			public int iSortDirection;

			/// UINT->unsigned int
			public uint version;

			/// UINT->unsigned int
			public uint uNotUsed;

			/// fSepProcess : 1
			///fStartPanelOn : 1
			///fShowStartPage : 1
			///fSpareFlags : 13
			public uint bitvector2;
			public uint bitvector3;

			public uint fShowAllObjects {
				get {
					return ((uint)((this.bitvector1 & 1u)));
				}
				set {
					this.bitvector1 = ((uint)((value | this.bitvector1)));
				}
			}

			public uint fShowExtensions {
				get {
					return ((uint)(((this.bitvector1 & 2u)
											/ 2)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 2)
											| this.bitvector1)));
				}
			}

			public uint fNoConfirmRecycle {
				get {
					return ((uint)(((this.bitvector1 & 4u)
											/ 4)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 4)
											| this.bitvector1)));
				}
			}

			public uint fShowSysFiles {
				get {
					return ((uint)(((this.bitvector1 & 8u)
											/ 8)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 8)
											| this.bitvector1)));
				}
			}

			public uint fShowCompColor {
				get {
					return ((uint)(((this.bitvector1 & 16u)
											/ 16)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 16)
											| this.bitvector1)));
				}
			}

			public uint fDoubleClickInWebView {
				get {
					return ((uint)(((this.bitvector1 & 32u)
											/ 32)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 32)
											| this.bitvector1)));
				}
			}

			public uint fDesktopHTML {
				get {
					return ((uint)(((this.bitvector1 & 64u)
											/ 64)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 64)
											| this.bitvector1)));
				}
			}

			public uint fWin95Classic {
				get {
					return ((uint)(((this.bitvector1 & 128u)
											/ 128)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 128)
											| this.bitvector1)));
				}
			}

			public uint fDontPrettyPath {
				get {
					return ((uint)(((this.bitvector1 & 256u)
											/ 256)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 256)
											| this.bitvector1)));
				}
			}

			public uint fShowAttribCol {
				get {
					return ((uint)(((this.bitvector1 & 512u)
											/ 512)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 512)
											| this.bitvector1)));
				}
			}

			public uint fMapNetDrvBtn {
				get {
					return ((uint)(((this.bitvector1 & 1024u)
											/ 1024)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 1024)
											| this.bitvector1)));
				}
			}

			public uint fShowInfoTip {
				get {
					return ((uint)(((this.bitvector1 & 2048u)
											/ 2048)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 2048)
											| this.bitvector1)));
				}
			}

			public uint fHideIcons {
				get {
					return ((uint)(((this.bitvector1 & 4096u)
											/ 4096)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 4096)
											| this.bitvector1)));
				}
			}

			public uint fWebView {
				get {
					return ((uint)(((this.bitvector1 & 8192u)
											/ 8192)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 8192)
											| this.bitvector1)));
				}
			}

			public uint fFilter {
				get {
					return ((uint)(((this.bitvector1 & 16384u)
											/ 16384)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 16384)
											| this.bitvector1)));
				}
			}

			public uint fShowSuperHidden {
				get {
					return ((uint)(((this.bitvector1 & 32768u)
											/ 32768)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 32768)
											| this.bitvector1)));
				}
			}

			public uint fNoNetCrawling {
				get {
					return ((uint)(((this.bitvector1 & 65536u)
											/ 65536)));
				}
				set {
					this.bitvector1 = ((uint)(((value * 65536)
											| this.bitvector1)));
				}
			}

			public uint fSepProcess {
				get {
					return ((uint)((this.bitvector2 & 1u)));
				}
				set {
					this.bitvector2 = ((uint)((value | this.bitvector2)));
				}
			}

			public uint fStartPanelOn {
				get {
					return ((uint)(((this.bitvector2 & 2u)
											/ 2)));
				}
				set {
					this.bitvector2 = ((uint)(((value * 2)
											| this.bitvector2)));
				}
			}

			public uint fShowStartPage {
				get {
					return ((uint)(((this.bitvector2 & 4u)
											/ 4)));
				}
				set {
					this.bitvector2 = ((uint)(((value * 4)
											| this.bitvector2)));
				}
			}

			public uint fAutoCheckSelect {
				get {
					return ((uint)(((this.bitvector2 & 8u)
											/ 8)));
				}
				set {
					this.bitvector2 = ((uint)(((value * 8)
											| this.bitvector2)));
				}
			}

			public uint fShowStatusBar {
				get {
					return ((uint)(((this.bitvector2 & 64u)
											/ 64)));
				}
				set {
					this.bitvector2 = ((uint)(((value * 64)
											| this.bitvector2)));
				}
			}

			public uint fSpareFlags {
				get {
					return ((uint)(((this.bitvector2 & 65528u)
											/ 8)));
				}
				set {
					this.bitvector2 = ((uint)(((value * 8)
											| this.bitvector2)));
				}
			}
		}
	}
}
