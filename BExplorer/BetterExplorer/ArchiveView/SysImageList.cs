using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FIcon = BExplorer.Shell.Interop.FileIcon;

namespace SystemImageList {

	#region Public Enumerations

	/// <summary>
	/// Available system image list sizes
	/// </summary>
	public enum SysImageListSize : int {

		/// <summary>
		/// System Large Icon Size (typically 32x32)
		/// </summary>
		largeIcons = 0x0,

		/// <summary>
		/// System Small Icon Size (typically 16x16)
		/// </summary>
		smallIcons = 0x1,

		/// <summary>
		/// System Extra Large Icon Size (typically 48x48).
		/// Only available under XP; under other OS the
		/// Large Icon ImageList is returned.
		/// </summary>
		extraLargeIcons = 0x2,

		/// <summary>
		/// System Jumbo Icon set (typically 256x256).
		/// Only available on Windows Vista and up.
		/// On other systems Large Icon is displayed.
		/// </summary>
		jumboIcons = 0x4
	}

	/// <summary>
	/// Flags controlling how the Image List item is
	/// drawn
	/// </summary>
	[Flags]
	public enum ImageListDrawItemConstants : int {

		/// <summary>
		/// Draw item normally.
		/// </summary>
		ILD_NORMAL = 0x0,

		/// <summary>
		/// Draw item transparently.
		/// </summary>
		ILD_TRANSPARENT = 0x1,

		/// <summary>
		/// Draw item blended with 25% of the specified foreground colour
		/// or the Highlight colour if no foreground colour specified.
		/// </summary>
		ILD_BLEND25 = 0x2,

		/// <summary>
		/// Draw item blended with 50% of the specified foreground colour
		/// or the Highlight colour if no foreground colour specified.
		/// </summary>
		ILD_SELECTED = 0x4,

		/// <summary>
		/// Draw the icon's mask
		/// </summary>
		ILD_MASK = 0x10,

		/// <summary>
		/// Draw the icon image without using the mask
		/// </summary>
		ILD_IMAGE = 0x20,

		/// <summary>
		/// Draw the icon using the ROP specified.
		/// </summary>
		ILD_ROP = 0x40,

		/// <summary>
		/// Preserves the alpha channel in dest. XP only.
		/// </summary>
		ILD_PRESERVEALPHA = 0x1000,

		/// <summary>
		/// Scale the image to cx, cy instead of clipping it.  XP only.
		/// </summary>
		ILD_SCALE = 0x2000,

		/// <summary>
		/// Scale the image to the current DPI of the display. XP only.
		/// </summary>
		ILD_DPISCALE = 0x4000
	}

	/// <summary>
	/// Enumeration containing XP ImageList Draw State options
	/// </summary>
	[Flags]
	public enum ImageListDrawStateConstants : int {

		/// <summary>
		/// The image state is not modified.
		/// </summary>
		ILS_NORMAL = (0x00000000),

		/// <summary>
		/// Adds a glow effect to the icon, which causes the icon to appear to
		/// glow
		/// with a given color around the edges. (Note: does not appear to be
		/// implemented)
		/// </summary>
		ILS_GLOW = (0x00000001), //The color for the glow effect is passed to the

		///IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS.
		/// <summary>
		/// Adds a drop shadow effect to the icon. (Note: does not appear to be
		/// implemented)
		/// </summary>
		ILS_SHADOW = (0x00000002), //The color for the drop shadow effect is

		/// passed to the IImageList::Draw method in the crEffect member of
		/// IMAGELISTDRAWPARAMS.
		/// <summary>
		/// Saturates the icon by increasing each color component
		/// of the RGB triplet for each pixel in the icon. (Note: only ever
		/// appears
		/// to result in a completely unsaturated icon)
		/// </summary>
		ILS_SATURATE = (0x00000004), // The amount to increase is indicated by

		/// the frame member in the IMAGELISTDRAWPARAMS method.
		/// <summary>
		/// Alpha blends the icon. Alpha blending controls the transparency
		/// level of an icon, according to the value of its alpha channel.
		/// (Note: does not appear to be implemented).
		/// </summary>
		ILS_ALPHA = (0x00000008) //The value of the alpha channel is indicated by

		///the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel
		///can be from 0 to 255, with 0 being completely transparent, and 255 being
		///completely opaque.
	}

	/// <summary>
	/// Flags specifying the state of the icon to draw from the Shell
	/// </summary>
	[Flags]
	public enum ShellIconStateConstants {

		/// <summary>
		/// Get icon in normal state
		/// </summary>
		ShellIconStateNormal = 0,

		/// <summary>
		/// Put a link overlay on icon
		/// </summary>
		ShellIconStateLinkOverlay = 0x8000,

		/// <summary>
		/// show icon in selected state
		/// </summary>
		ShellIconStateSelected = 0x10000,

		/// <summary>
		/// get open icon
		/// </summary>
		ShellIconStateOpen = 0x2,

		/// <summary>
		/// apply the appropriate overlays
		/// </summary>
		ShellIconAddOverlays = 0x000000020,
	}

	#endregion Public Enumerations

	#region SysImageList

	/// <summary>
	/// Summary description for SysImageList.
	/// </summary>
	public class SysImageList : IDisposable {

		#region UnmanagedCode

		private const int MAX_PATH = 260;

		[DllImport("shell32")]
		private static extern IntPtr SHGetFileInfo(
		   string pszPath,
		   int dwFileAttributes,
		   ref SHFILEINFO psfi,
		   uint cbFileInfo,
		   uint uFlags);


		private const int FILE_ATTRIBUTE_NORMAL = 0x80;
		private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

		private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
		private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
		private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
		private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
		private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
		private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
		private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xFF;

		[DllImport("comctl32")]
		private extern static int ImageList_Draw(
		   IntPtr hIml,
		   int i,
		   IntPtr hdcDst,
		   int x,
		   int y,
		   int fStyle);

		[DllImport("comctl32")]
		private extern static int ImageList_DrawIndirect(
		   ref IMAGELISTDRAWPARAMS pimldp);

		[DllImport("comctl32")]
		private extern static int ImageList_GetIconSize(
		   IntPtr himl,
		   ref int cx,
		   ref int cy);

		[DllImport("comctl32")]
		private extern static IntPtr ImageList_GetIcon(
		   IntPtr himl,
		   int i,
		   int flags);

		/// <summary>
		/// SHGetImageList is not exported correctly in XP.  See KB316931
		/// http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
		/// Apparently (and hopefully) ordinal 727 isn't going to change.
		/// </summary>
		[DllImport("shell32.dll", EntryPoint = "#727")]
		private extern static int SHGetImageList(
		   int iImageList,
		   ref Guid riid,
		   ref IImageList ppv
		   );

		[DllImport("shell32.dll", EntryPoint = "#727")]
		private extern static int SHGetImageListHandle(
		   int iImageList,
		   ref Guid riid,
		   ref IntPtr handle
		   );

		#endregion UnmanagedCode

		#region Private ImageList structures
		/*
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			private int left;
			private int top;
			private int right;
			private int bottom;
		}
		*/
		/*
		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			private int x;
			private int y;
		}
		*/

		[StructLayout(LayoutKind.Sequential)]
		private struct IMAGELISTDRAWPARAMS {
			public int cbSize;
			public IntPtr himl;
			public int i;
			public IntPtr hdcDst;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public int xBitmap;        // x offest from the upperleft of bitmap
			public int yBitmap;        // y offset from the upperleft of bitmap
			public int rgbBk;
			public int rgbFg;
			public int fStyle;
			public int dwRop;
			public int fState;
			public int Frame;
			public int crEffect;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct IMAGEINFO {
			public IntPtr hbmImage;
			public IntPtr hbmMask;
			public int Unused1;
			public int Unused2;
			public BExplorer.Shell.Interop.User32.RECT rcImage;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SHFILEINFO {
			public IntPtr hIcon;
			public int iIcon;
			public int dwAttributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string szDisplayName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		#endregion Private ImageList structures

		#region Private ImageList COM Interop (XP)

		[ComImportAttribute()]
		[GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
		[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
		//helpstring("Image List"),
		private interface IImageList {

			[PreserveSig]
			int Add(
			   IntPtr hbmImage,
			   IntPtr hbmMask,
			   ref int pi);

			[PreserveSig]
			int ReplaceIcon(
			   int i,
			   IntPtr hicon,
			   ref int pi);

			[PreserveSig]
			int SetOverlayImage(
			   int iImage,
			   int iOverlay);

			[PreserveSig]
			int Replace(
			   int i,
			   IntPtr hbmImage,
			   IntPtr hbmMask);

			[PreserveSig]
			int AddMasked(
			   IntPtr hbmImage,
			   int crMask,
			   ref int pi);

			[PreserveSig]
			int Draw(ref IMAGELISTDRAWPARAMS pimldp);

			[PreserveSig]
			int Remove(int i);

			[PreserveSig]
			int GetIcon(
			   int i,
			   int flags,
			   ref IntPtr picon);

			[PreserveSig]
			int GetImageInfo(int i, ref IMAGEINFO pImageInfo);

			[PreserveSig]
			int Copy(
			   int iDst,
			   IImageList punkSrc,
			   int iSrc,
			   int uFlags);

			[PreserveSig]
			int Merge(
			   int i1,
			   IImageList punk2,
			   int i2,
			   int dx,
			   int dy,
			   ref Guid riid,
			   ref IntPtr ppv);

			[PreserveSig]
			int Clone(ref Guid riid, ref IntPtr ppv);

			[PreserveSig]
			int GetImageRect(int i, ref BExplorer.Shell.Interop.User32.RECT prc);

			[PreserveSig]
			int GetIconSize(ref int cx, ref int cy);

			[PreserveSig]
			int SetIconSize(int cx, int cy);

			[PreserveSig]
			int GetImageCount(ref int pi);

			[PreserveSig]
			int SetImageCount(int uNewCount);

			[PreserveSig]
			int SetBkColor(int clrBk, ref int pclr);

			[PreserveSig]
			int GetBkColor(ref int pclr);

			[PreserveSig]
			int BeginDrag(
			   int iTrack,
			   int dxHotspot,
			   int dyHotspot);

			[PreserveSig]
			int EndDrag();

			[PreserveSig]
			int DragEnter(
			   IntPtr hwndLock,
			   int x,
			   int y);

			[PreserveSig]
			int DragLeave(IntPtr hwndLock);

			[PreserveSig]
			int DragMove(int x, int y);

			[PreserveSig]
			int SetDragCursorImage(
			   ref IImageList punk,
			   int iDrag,
			   int dxHotspot,
			   int dyHotspot);

			[PreserveSig]
			int DragShowNolock(int fShow);

			[PreserveSig]
			int GetDragImage(
			   ref BExplorer.Shell.Interop.POINT ppt,
			   ref BExplorer.Shell.Interop.POINT pptHotspot,
			   ref Guid riid,
			   ref IntPtr ppv);

			[PreserveSig]
			int GetItemFlags(int i, ref int dwFlags);

			[PreserveSig]
			int GetOverlayImage(int iOverlay, ref int piIndex);
		};

		#endregion Private ImageList COM Interop (XP)

		#region Member Variables

		private IntPtr hIml = IntPtr.Zero;
		private IImageList iImageList = null;
		private SysImageListSize size = SysImageListSize.smallIcons;
		private bool disposed = false;

		#endregion Member Variables

		#region Implementation

		#region Properties

		/// <summary>
		/// Gets the hImageList handle
		/// </summary>
		public IntPtr Handle {
			get {
				return this.hIml;
			}
		}

		/// <summary>
		/// Gets/sets the size of System Image List to retrieve.
		/// </summary>
		public SysImageListSize ImageListSize {
			get {
				return size;
			}
			set {
				size = value;
				create();
			}
		}

		/// <summary>
		/// Returns the size of the Image List Icons.
		/// </summary>
		public System.Drawing.Size Size {
			get {
				int cx = 0;
				int cy = 0;
				if (iImageList == null) {
					ImageList_GetIconSize(
					   hIml,
					   ref cx,
					   ref cy);
				}
				else {
					iImageList.GetIconSize(ref cx, ref cy);
				}
				System.Drawing.Size sz = new System.Drawing.Size(
				   cx, cy);
				return sz;
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Returns a GDI+ copy of the icon from the ImageList
		/// at the specified index.
		/// </summary>
		/// <param name="index">The index to get the icon for</param>
		/// <returns>The specified icon</returns>
		public Icon Icon(int index) {
			Icon icon = null;

			IntPtr hIcon = IntPtr.Zero;
			if (iImageList == null) {
				hIcon = ImageList_GetIcon(
				   hIml,
				   index,
				   (int)ImageListDrawItemConstants.ILD_TRANSPARENT);
			}
			else {
				iImageList.GetIcon(
				   index,
				   (int)ImageListDrawItemConstants.ILD_TRANSPARENT,
				   ref hIcon);
			}

			if (hIcon != IntPtr.Zero) {
				icon = System.Drawing.Icon.FromHandle(hIcon);
			}
			return icon;
		}

		/// <summary>
		/// Return the index of the icon for the specified file, always using
		/// the cached version where possible.
		/// </summary>
		/// <param name="fileName">Filename to get icon for</param>
		/// <returns>Index of the icon</returns>
		public int IconIndex(string fileName) {
			return IconIndex(fileName, false);
		}

		/// <summary>
		/// Returns the index of the icon for the specified file
		/// </summary>
		/// <param name="fileName">Filename to get icon for</param>
		/// <param name="forceLoadFromDisk">If True, then hit the disk to get the
		/// icon,
		/// otherwise only hit the disk if no cached icon is available.</param>
		/// <returns>Index of the icon</returns>
		public int IconIndex(
		   string fileName,
		   bool forceLoadFromDisk) {
			return IconIndex(
			   fileName,
			   forceLoadFromDisk,
			   ShellIconStateConstants.ShellIconStateNormal);
		}

		/// <summary>
		/// Returns the index of the icon for the specified file
		/// </summary>
		/// <param name="fileName">Filename to get icon for</param>
		/// <param name="forceLoadFromDisk">If True, then hit the disk to get the
		/// icon,
		/// otherwise only hit the disk if no cached icon is available.</param>
		/// <param name="iconState">Flags specifying the state of the icon
		/// returned.</param>
		/// <returns>Index of the icon</returns>
		public int IconIndex(string fileName, bool forceLoadFromDisk, ShellIconStateConstants iconState) {
			FIcon.SHGetFileInfoConstants dwFlags = FIcon.SHGetFileInfoConstants.SHGFI_SYSICONINDEX;
			int dwAttr = 0;
			if (size == SysImageListSize.smallIcons) {
				dwFlags |= FIcon.SHGetFileInfoConstants.SHGFI_SMALLICON;
			}

			// We can choose whether to access the disk or not. If you don't
			// hit the disk, you may get the wrong icon if the icon is
			// not cached. Also only works for files.
			if (!forceLoadFromDisk) {
				dwFlags |= FIcon.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES;
				dwAttr = FILE_ATTRIBUTE_NORMAL;
			}
			else {
				dwAttr = 0;
			}

			// sFileSpec can be any file. You can specify a
			// file that does not exist and still get the
			// icon, for example sFileSpec = "C:\PANTS.DOC"
			SHFILEINFO shfi = new SHFILEINFO();
			uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());
			IntPtr retVal = SHGetFileInfo(
			   fileName, dwAttr, ref shfi, shfiSize,
			   ((uint)(dwFlags) | (uint)iconState));

			if (retVal.Equals(IntPtr.Zero)) {
				System.Diagnostics.Debug.Assert((!retVal.Equals(IntPtr.Zero)), "Failed to get icon index");
				return 0;
			}
			else {
				return shfi.iIcon;
			}
		}

		/// <summary>
		/// Draws an image
		/// </summary>
		/// <param name="hdc">Device context to draw to</param>
		/// <param name="index">Index of image to draw</param>
		/// <param name="x">X Position to draw at</param>
		/// <param name="y">Y Position to draw at</param>
		public void DrawImage(
		   IntPtr hdc,
		   int index,
		   int x,
		   int y
		   ) {
			DrawImage(hdc, index, x, y,
			 ImageListDrawItemConstants.ILD_TRANSPARENT);
		}

		/// <summary>
		/// Draws an image using the specified flags
		/// </summary>
		/// <param name="hdc">Device context to draw to</param>
		/// <param name="index">Index of image to draw</param>
		/// <param name="x">X Position to draw at</param>
		/// <param name="y">Y Position to draw at</param>
		/// <param name="flags">Drawing flags</param>
		public void DrawImage(
		   IntPtr hdc,
		   int index,
		   int x,
		   int y,
		   ImageListDrawItemConstants flags
		   ) {
			if (iImageList == null) {
				int ret = ImageList_Draw(
				   hIml,
				   index,
				   hdc,
				   x,
				   y,
				   (int)flags);
			}
			else {
				IMAGELISTDRAWPARAMS pimldp = new IMAGELISTDRAWPARAMS();
				pimldp.hdcDst = hdc;
				pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
				pimldp.i = index;
				pimldp.x = x;
				pimldp.y = y;
				pimldp.rgbFg = -1;
				pimldp.fStyle = (int)flags;
				iImageList.Draw(ref pimldp);
			}
		}

		/// <summary>
		/// Draws an image using the specified flags and specifies
		/// the size to clip to (or to stretch to if ILD_SCALE
		/// is provided).
		/// </summary>
		/// <param name="hdc">Device context to draw to</param>
		/// <param name="index">Index of image to draw</param>
		/// <param name="x">X Position to draw at</param>
		/// <param name="y">Y Position to draw at</param>
		/// <param name="flags">Drawing flags</param>
		/// <param name="cx">Width to draw</param>
		/// <param name="cy">Height to draw</param>
		public void DrawImage(
		   IntPtr hdc,
		   int index,
		   int x,
		   int y,
		   ImageListDrawItemConstants flags,
		   int cx,
		   int cy
		   ) {
			IMAGELISTDRAWPARAMS pimldp = new IMAGELISTDRAWPARAMS();
			pimldp.hdcDst = hdc;
			pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
			pimldp.i = index;
			pimldp.x = x;
			pimldp.y = y;
			pimldp.cx = cx;
			pimldp.cy = cy;
			pimldp.fStyle = (int)flags;
			if (iImageList == null) {
				pimldp.himl = hIml;
				int ret = ImageList_DrawIndirect(ref pimldp);
			}
			else {
				iImageList.Draw(ref pimldp);
			}
		}

		/// <summary>
		/// Draws an image using the specified flags and state on XP systems.
		/// </summary>
		/// <param name="hdc">Device context to draw to</param>
		/// <param name="index">Index of image to draw</param>
		/// <param name="x">X Position to draw at</param>
		/// <param name="y">Y Position to draw at</param>
		/// <param name="flags">Drawing flags</param>
		/// <param name="cx">Width to draw</param>
		/// <param name="cy">Height to draw</param>
		/// <param name="foreColor">Fore colour to blend with when using the
		/// ILD_SELECTED or ILD_BLEND25 flags</param>
		/// <param name="stateFlags">State flags</param>
		/// <param name="glowOrShadowColor">If stateFlags include ILS_GLOW, then
		/// the colour to use for the glow effect.  Otherwise if stateFlags
		/// includes
		/// ILS_SHADOW, then the colour to use for the shadow.</param>
		/// <param name="saturateColorOrAlpha">If stateFlags includes ILS_ALPHA,
		/// then the alpha component is applied to the icon. Otherwise if
		/// ILS_SATURATE is included, then the (R,G,B) components are used
		/// to saturate the image.</param>
		public void DrawImage(
		   IntPtr hdc,
		   int index,
		   int x,
		   int y,
		   ImageListDrawItemConstants flags,
		   int cx,
		   int cy,
		   System.Drawing.Color foreColor,
		   ImageListDrawStateConstants stateFlags,
		   System.Drawing.Color saturateColorOrAlpha,
		   System.Drawing.Color glowOrShadowColor
		   ) {
			IMAGELISTDRAWPARAMS pimldp = new IMAGELISTDRAWPARAMS();
			pimldp.hdcDst = hdc;
			pimldp.cbSize = Marshal.SizeOf(pimldp.GetType());
			pimldp.i = index;
			pimldp.x = x;
			pimldp.y = y;
			pimldp.cx = cx;
			pimldp.cy = cy;
			pimldp.rgbFg = Color.FromArgb(0,
			   foreColor.R, foreColor.G, foreColor.B).ToArgb();
			Console.WriteLine("{0}", pimldp.rgbFg);
			pimldp.fStyle = (int)flags;
			pimldp.fState = (int)stateFlags;
			if ((stateFlags & ImageListDrawStateConstants.ILS_ALPHA) ==
			   ImageListDrawStateConstants.ILS_ALPHA) {
				// Set the alpha:
				pimldp.Frame = (int)saturateColorOrAlpha.A;
			}
			else if ((stateFlags & ImageListDrawStateConstants.ILS_SATURATE) ==
			   ImageListDrawStateConstants.ILS_SATURATE) {
				// discard alpha channel:
				saturateColorOrAlpha = Color.FromArgb(0,
				   saturateColorOrAlpha.R,
				   saturateColorOrAlpha.G,
				   saturateColorOrAlpha.B);
				// set the saturate color
				pimldp.Frame = saturateColorOrAlpha.ToArgb();
			}
			glowOrShadowColor = Color.FromArgb(0,
			   glowOrShadowColor.R,
			   glowOrShadowColor.G,
			   glowOrShadowColor.B);
			pimldp.crEffect = glowOrShadowColor.ToArgb();
			if (iImageList == null) {
				pimldp.himl = hIml;
				int ret = ImageList_DrawIndirect(ref pimldp);
			}
			else {
				iImageList.Draw(ref pimldp);
			}
		}

		/// <summary>
		/// Determines if the system is running Windows XP
		/// or above
		/// </summary>
		/// <returns>True if system is running XP or above, False
		///otherwise</returns>
		private bool isXpOrAbove() {
			bool ret = false;
			if (Environment.OSVersion.Version.Major > 5) {
				ret = true;
			}
			else if ((Environment.OSVersion.Version.Major == 5) &&
			   (Environment.OSVersion.Version.Minor >= 1)) {
				ret = true;
			}
			return ret;
			//return false;
		}

		/// <summary>
		/// Creates the SystemImageList
		/// </summary>
		private void create() {
			// forget last image list if any:
			hIml = IntPtr.Zero;

			if (isXpOrAbove()) {
				// Get the System IImageList object from the Shell:
				Guid iidImageList = new
				 Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
				int ret = SHGetImageList(
				   (int)size,
				   ref iidImageList,
				   ref iImageList
				   );
				// the image list handle is the IUnknown pointer, but
				// using Marshal.GetIUnknownForObject doesn't return
				// the right value.  It really doesn't hurt to make
				// a second call to get the handle:
				SHGetImageListHandle((int)size, ref iidImageList, ref hIml);
			}
			else {
				// Prepare flags:
				FIcon.SHGetFileInfoConstants dwFlags =
					FIcon.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES |
					FIcon.SHGetFileInfoConstants.SHGFI_SYSICONINDEX;
				if (size == SysImageListSize.smallIcons) {
					dwFlags |= FIcon.SHGetFileInfoConstants.SHGFI_SMALLICON;
				}
				// Get image list
				SHFILEINFO shfi = new SHFILEINFO();
				uint shfiSize = (uint)Marshal.SizeOf(shfi.GetType());

				// Call SHGetFileInfo to get the image list handle
				// using an arbitrary file:
				hIml = SHGetFileInfo(
				   ".txt",
				   FILE_ATTRIBUTE_NORMAL,
				   ref shfi,
				   shfiSize,
				   (uint)dwFlags);
				System.Diagnostics.Debug.Assert((hIml != IntPtr.Zero), "Failed tocreate Image List<>");
			}
		}

		#endregion Methods

		#region Constructor, Dispose, Destructor

		/// <summary>
		/// Creates a Small Icons SystemImageList
		/// </summary>
		public SysImageList() {
			create();
		}

		/// <summary>
		/// Creates a SystemImageList with the specified size
		/// </summary>
		/// <param name="size">Size of System ImageList</param>
		public SysImageList(SysImageListSize size) {
			this.size = size;
			create();
		}

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
			if (!disposed) {
				if (disposing) {
					if (iImageList != null) {
						Marshal.ReleaseComObject(iImageList);
					}
					iImageList = null;
				}
			}
			disposed = true;
		}

		/// <summary>
		/// Finalise for SysImageList
		/// </summary>
		~SysImageList() {
			Dispose(false);
		}
	}

		#endregion Constructor, Dispose, Destructor

		#endregion Implementation

	#endregion SysImageList

	#region SysImageListHelper

	/// <summary>
	/// Helper Methods for Connecting SysImageList to Common Controls
	/// </summary>
	public class SysImageListHelper {

		#region UnmanagedMethods

		private const int LVM_FIRST = 0x1000;
		private const int LVM_SETIMAGELIST = (LVM_FIRST + 3);

		private const int LVSIL_NORMAL = 0;
		private const int LVSIL_SMALL = 1;
		private const int LVSIL_STATE = 2;

		private const int TV_FIRST = 0x1100;
		private const int TVM_SETIMAGELIST = (TV_FIRST + 9);

		private const int TVSIL_NORMAL = 0;
		private const int TVSIL_STATE = 2;

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(
		   IntPtr hWnd,
		   int wMsg,
		   IntPtr wParam,
		   IntPtr lParam);

		#endregion UnmanagedMethods

		/// <summary>
		/// Associates a SysImageList with a ListView control
		/// </summary>
		/// <param name="listView">ListView control to associate ImageList
		/// with</param>
		/// <param name="sysImageList">System Image List to associate</param>
		/// <param name="forStateImages">Whether to add ImageList as
		/// StateImageList</param>
		public static void SetListViewImageList(
		   ListView listView,
		   SysImageList sysImageList,
		   bool forStateImages
		   ) {
			IntPtr wParam = (IntPtr)LVSIL_NORMAL;
			if (sysImageList.ImageListSize == SysImageListSize.smallIcons) {
				wParam = (IntPtr)LVSIL_SMALL;
			}
			if (forStateImages) {
				wParam = (IntPtr)LVSIL_STATE;
			}
			SendMessage(
			   listView.Handle,
			   LVM_SETIMAGELIST,
			   wParam,
			   sysImageList.Handle);
		}

		/// <summary>
		/// Associates a SysImageList with a TreeView control
		/// </summary>
		/// <param name="treeView">TreeView control to associated ImageList
		/// with</param>
		/// <param name="sysImageList">System Image List to associate</param>
		/// <param name="forStateImages">Whether to add ImageList as
		/// StateImageList</param>
		public static void SetTreeViewImageList(
		   TreeView treeView,
		   SysImageList sysImageList,
		   bool forStateImages
		   ) {
			IntPtr wParam = (IntPtr)TVSIL_NORMAL;
			if (forStateImages) {
				wParam = (IntPtr)TVSIL_STATE;
			}
			SendMessage(
			   treeView.Handle,
			   TVM_SETIMAGELIST,
			   wParam,
			   sysImageList.Handle);
		}
	}

	#endregion SysImageListHelper
}