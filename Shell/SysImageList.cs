using BExplorer.Shell.Interop;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BExplorer.Shell {

	public static class Win32Api {
		//[DllImport("shell32.dll", EntryPoint = "ExtractIconEx", CharSet = CharSet.Auto)]
		//public static extern int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)] string file, int index, out IntPtr largeIconHandle, out IntPtr smallIconHandle, int icons);

		[DllImport("shell32.dll", EntryPoint = "SHGetFileInfo", CharSet = CharSet.Auto)]
		public static extern IntPtr SHGetFileInfo(IntPtr pszPath, FileAttributes attr, ref SHFILEINFO psfi, int cbSizeFileInfo, SHGetFileInfoOptions uFlags);
		//Note:	Date: 8/28/2014	User: Aaron Campf	Message: Replaces below code with above code
		//public static extern IntPtr SHGetFileInfo(IntPtr pszPath, FileAttributes attr, ref ImageList.SHFileInfo psfi, int cbSizeFileInfo, SHGetFileInfoOptions uFlags);

		[DllImport("shell32.dll", EntryPoint = "#727")]
		public extern static int SHGetImageList(ImageListSize iImageList, ref Guid riid, out IImageList2 ppv);

		[DllImport("shell32.dll", EntryPoint = "#727")]
		public extern static int SHGetImageListHandle(
								 ImageListSize iImageList,
								 ref Guid riid,
								 ref IntPtr handle
								 );

		//[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		//public extern static IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr newValue);

		//[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
		//public extern static IntPtr SetWindowLongPtr64(IntPtr hwnd, int nIndex, IntPtr newValue);

		//[DllImport("kernel32.dll")]
		//public static extern IntPtr LoadLibrary(String lpFileName);
		//[DllImport("kernel32.dll")]
		//public static extern IntPtr GetProcAddress(IntPtr hModule, String lpProcName);
		//[DllImport("kernel32.dll")]
		//public static extern Boolean FreeLibrary(IntPtr hLibModule);

		//[DllImport("KERNEL32.DLL", EntryPoint = "CloseHandle", CharSet = CharSet.Auto, SetLastError = true)]
		//public static extern bool CloseHandle(IntPtr handle);

		//[DllImport("KERNEL32.DLL", EntryPoint = "UnmapViewOfFile", CharSet = CharSet.Auto, SetLastError = true)]
		//public static extern bool UnmapViewOfFile(IntPtr hMap);

		//[DllImport("KERNEL32.DLL", EntryPoint = "GetLastError", CharSet = CharSet.Auto)]
		//public static extern int GetLastError();

		//[DllImport("KERNEL32.DLL", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Auto)]
		//public static extern void CopyMemory(IntPtr dst, IntPtr src, IntPtr length);

		//[DllImport("USER32.DLL", EntryPoint = "GetActiveWindow", CharSet = CharSet.Auto)]
		//public static extern IntPtr GetActiveWindow();

		//[DllImport("USER32.DLL", EntryPoint = "SetActiveWindow", CharSet = CharSet.Auto)]
		//public static extern IntPtr SetActiveWindow(IntPtr hwnd);


		///// <summary>
		///// EChEð\¦·éB
		///// </summary>
		///// <param name="hwnd">\¦·éEChEÌnh</param>
		///// <param name="cmdShow">EChEÌóÔ</param>
		///// <returns>int</returns>

		///// <summary>
		///// EChEðOÊÉ\¦·éB
		///// </summary>
		///// <param name="hWnd">OÊ¢\¦·éEChEÌnh</param>
		///// <returns>bool</returns>
		//[DllImport("USER32.DLL", EntryPoint = "SetForegroundWindow", CharSet = CharSet.Auto)]
		//public static extern bool SetForegroundWindow(IntPtr hWnd);

		//[DllImport("USER32.DLL", EntryPoint = "GetForegroundWindow", CharSet = CharSet.Auto)]
		//public static extern IntPtr GetForegroundWindow();


		//[DllImport("USER32.DLL", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
		//public static extern IntPtr SendMessage(IntPtr hwnd, int msg, int wParam, int lParam);


		///// <summary>
		///// IMEÌReLXgðæ¾·éB
		///// </summary>
		///// <param name="hwnd">hwnd</param>
		///// <returns>ReLXgÌ|C^</returns>
		//[DllImport("imm32.dll", EntryPoint = "ImmGetContext", CharSet = CharSet.Auto)]
		//public static extern IntPtr ImmGetContext(IntPtr hwnd);

		///// <summary>
		///// IMEÌóÔðÏX·éB
		///// </summary>
		///// <param name="hIMC">IMEÌReLXgÌ|C^</param>
		///// <param name="fOpen">IMEðJ­©Ç¤©</param>
		///// <returns>bool</returns>
		//[DllImport("imm32.dll", EntryPoint = "ImmSetOpenStatus", CharSet = CharSet.Auto)]
		//public static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);

		//public delegate bool EnumWindowsProc(IntPtr hWnd, object lParam);

		//[DllImport("user32", EntryPoint = "EnumWindows", CharSet = CharSet.Auto)]
		//public static extern int EnumWindows(EnumWindowsProc lpEnumFunc, object lParam);

		//[DllImport("user32", EntryPoint = "GetWindowThreadProcessId", CharSet = CharSet.Auto)]
		//public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);


		//[DllImport("user32", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
		//public static extern bool SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)] string text);


		//public static readonly IntPtr HWND_TOP = IntPtr.Zero;
		//public static readonly IntPtr HWND_BOTTOM = (IntPtr)1;
		//public static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
		//public static readonly IntPtr HWND_NOTOPMOST = (IntPtr)(-2);

		//[DllImport("user32", EntryPoint = "GetWindowRect", CharSet = CharSet.Auto)]
		//public static extern int GetWindowRect(IntPtr hWnd, ref Rectangle rect);

		//[DllImport("user32", EntryPoint = "IsWindow", CharSet = CharSet.Auto)]
		//public static extern bool IsWindow(IntPtr hWnd);



		//[DllImport("user32", EntryPoint = "IsWindowVisible", CharSet = CharSet.Auto)]
		//public static extern bool IsWindowVisible(IntPtr Handle);

		//[DllImport("user32", EntryPoint = "IsWindowEnabled", CharSet = CharSet.Auto)]
		//public static extern bool IsWindowEnabled(IntPtr Handle);

		//[DllImport("user32", EntryPoint = "EnableWindow", CharSet = CharSet.Auto)]
		//public static extern bool IsWindowEnabled(IntPtr Handle, bool enable);




		//[DllImport("user32.dll", EntryPoint = "GetKeyboardState", CharSet = CharSet.Auto)]
		//internal static extern int GetKeyboardState(byte[] lpKeyState);

		//public static byte[] GetKeyboardState() {
		//	byte[] keyState = new byte[256];
		//	int result = GetKeyboardState(keyState);
		//	if (result == 0) {
		//		throw new Win32Exception();
		//	}
		//	return keyState;
		//}


		//[DllImport("user32.dll", EntryPoint = "GetDC", CharSet = CharSet.Auto)]
		//public static extern IntPtr GetDeviceContext(IntPtr hWnd);

		//[DllImport("user32.dll", EntryPoint = "ReleaseDC", CharSet = CharSet.Auto)]
		//public static extern IntPtr ReleaseDeviceContext(IntPtr hWnd, IntPtr hDc);

		//[DllImport("gdi32.dll", EntryPoint = "BitBlt", CharSet = CharSet.Auto)]
		//public static extern bool BitBlt(IntPtr hdcDst, int xDst, int yDsk, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rasterOp);
		/*
		public static System.Drawing.Rectangle GetTotalBound(){
			int x, y, w, h;
			int minX = 0, maxX = 0, minY = 0, maxY = 0;
			foreach(Screen s in Screen.AllScreens){
					x = s.Bounds.X;
					y = s.Bounds.Y;
					w = s.Bounds.X + s.Bounds.Width;
					h = s.Bounds.Y + s.Bounds.Height;
					if(x < minX)
							minX = x;
					if(y < minY)
							minY = y;
					if(maxX < w)
							maxX = w;
					if(maxY < h)
							maxY = h;
			}
			System.Drawing.Rectangle r = new System.Drawing.Rectangle(minX, minY, maxX - minX, maxY - minY);
			return r;
		}

		public static Bitmap GetDesktopBitmap(){
			System.Drawing.Rectangle rect = GetTotalBound();
			Bitmap bitmap = new Bitmap(rect.Width, rect.Height);
			Graphics g = Graphics.FromImage(bitmap);
			IntPtr desktopHandle = IntPtr.Zero;
			IntPtr desktopDeviceContext = IntPtr.Zero;
			IntPtr imageDeviceContext = IntPtr.Zero;
			try{
					desktopHandle = GetDesktopWindow();
					desktopDeviceContext = GetDeviceContext(desktopHandle);
					imageDeviceContext = g.GetHdc();
					BitBlt(imageDeviceContext, 0, 0, rect.Width, rect.Height, desktopDeviceContext, rect.X, rect.Y, 0xCC0020);
			}finally{
					ReleaseDeviceContext(desktopHandle, desktopDeviceContext);
					if(imageDeviceContext != IntPtr.Zero){
							g.ReleaseHdc(imageDeviceContext);
					}
			}
			return bitmap;
		}
		*/

		[DllImport("USER32.DLL", EntryPoint = "DestroyIcon", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DestroyIcon(IntPtr hIcon);

		//[DllImport("kernel32", EntryPoint = "GetLogicalDrives", CharSet = CharSet.Auto)]
		//internal static extern long InternalGetLogicalDrives();

		//[Obsolete]
		//public static string[] GetLogicalDrives() {
		//	List<string> drives = new List<string>();
		//	long d = Win32Api.InternalGetLogicalDrives();
		//	for (int i = 0; i < 26; i++) {
		//		if (((1 << i) & d) > 0) {
		//			drives.Add(((char)('A' + i)) + ":\\");
		//		}
		//	}
		//	return drives.ToArray();
		//}

	}

	/*
public enum IconSize {
	Large = 0x0,
	Small = 0x1,
}
*/

	[Flags]
	public enum SHGetFileInfoOptions : uint {
		Icon = 0x000000100,                 // get icon
		DisplayName = 0x000000200,          // get display name
		TypeName = 0x000000400,             // get type name
		Attributes = 0x000000800,           // get attributes
		IconLocation = 0x000001000,         // get icon location
		ExeType = 0x000002000,              // return exe type
		SysIconIndex = 0x000004000,         // get system icon index
		LinkOverlay = 0x000008000,          // put a link overlay on icon
		Selected = 0x000010000,             // show icon in selected state
											// (NTDDI_VERSION >= NTDDI_WIN2K)
		SpecifiedAttributes = 0x000020000,  // get only specified attributes
		LargeIcon = 0x000000000,            // get large icon
		SmallIcon = 0x000000001,            // get small icon
		OpenIcon = 0x000000002,             // get open icon
		ShellIconSize = 0x000000004,        // get shell size icon
		Pidl = 0x000000008,                 // pszPath is a pidl
		UseFileAttributes = 0x000000010,    // use passed dwFileAttribute
											// (_WIN32_IE >= 0x0500)
		AddOverlays = 0x000000020,          // apply the appropriate overlays
		OverlayIndex = 0x000000040,         // Get the index of the overlay
	}


	/// <summary>Provides attributes for files and directories.</summary>
	[Flags]
	public enum FileAttributes : uint {
		None = 0x00000000,
		ReadOnly = 0x00000001,
		Hidden = 0x00000002,
		System = 0x00000004,
		Directory = 0x00000010,
		Archive = 0x00000020,
		Normal = 0x00000080,
		Temporary = 0x00000100,
	}

	/// <summary>
	/// A wrapper of IImageList.
	/// </summary>
	/// <remarks>
	/// </remarks>
	public class ImageList : IDisposable {

		/*
		#region SHFileInfo
		//[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		//public struct SHFileInfo {
		//	public IntPtr hIcon;
		//	public int iIcon;
		//	public uint dwAttributes;
		//	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		//	public string szDisplayName;
		//	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		//	public string szTypeName;
		//}
		#endregion
		*/

		#region Locals

		//private ImageListSize _Size;
		private IImageList2 _ImageList;
		//private static Guid IID_ImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
		private static Guid IID_ImageList2 = new Guid("192B9D83-50FC-457B-90A0-2B82A8B5DAE1");

		#endregion

		#region Property

		/*
public void SetSize(int size) {
	ImageList_Destroy(this.Handle);
	this._ImageList = Marshal.GetObjectForIUnknown(ImageList_Create(size, size, 0x00020000 | 0x00000020, 30, 30)) as IImageList2;
	this._ImageList.SetImageCount(3000);
}

private Lazy<Int32Size> _SizePixels;

public ImageListSize Size {
	get {
		return this._Size;
	}
}

public int Width {
	get {
		return this._SizePixels.Value.Width;
	}
}

public int Height {
	get {
		return this._SizePixels.Value.Height;
	}
}
*/

		/*
private IntPtr Handle {
	get {
		return Marshal.GetIUnknownForObject(this._ImageList);
	}
}
*/

		/*
public static ImageListSize MaxSize {
	get {
		if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
			if (Environment.OSVersion.Version.Major >= 6) {
				return ImageListSize.Jumbo;
			}
			else {
				return ImageListSize.ExtraLarge;
			}
		}
		return ImageListSize.Jumbo;
	}
}
*/

		#endregion

		#region Method


		/*
		[Obsolete("Consider Inlining")]
		private int GetIconIndex(IntPtr path) {
			var options = SHGetFileInfoOptions.SysIconIndex | SHGetFileInfoOptions.Pidl;
			var shfi = new SHFILEINFO();
			var shfiSize = Marshal.SizeOf(shfi.GetType());
			IntPtr retVal = Win32Api.SHGetFileInfo(path, FileAttributes.None, ref shfi, shfiSize, options);
			if (shfi.hIcon != IntPtr.Zero) {
				Win32Api.DestroyIcon(shfi.hIcon);
			}

			if (retVal.Equals(IntPtr.Zero)) {
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			else {
				return shfi.iIcon;
			}
		}
		*/


		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="overlayIndex">Index of the overlay icon that use for Draw or GetIndexOfOverlay method.</param>
		/// <returns></returns>
		public int GetIconIndexWithOverlay(IntPtr path, out int overlayIndex) {
			var options = SHGetFileInfoOptions.SysIconIndex | SHGetFileInfoOptions.OverlayIndex | SHGetFileInfoOptions.Icon | SHGetFileInfoOptions.AddOverlays | SHGetFileInfoOptions.Pidl;
			var shfi = new SHFILEINFO();
			var shfiSize = Marshal.SizeOf(shfi.GetType());
			IntPtr retVal = Win32Api.SHGetFileInfo(path, FileAttributes.None, ref shfi, shfiSize, options);
			if (shfi.hIcon != IntPtr.Zero) {
				Win32Api.DestroyIcon(shfi.hIcon);
			}

			if (retVal.Equals(IntPtr.Zero)) {
				//throw new Win32Exception(Marshal.GetLastWin32Error());
				overlayIndex = 0;
				return 0;
			}
			else {
				/* brakes stack on optimized build
				int idx = shfi.iIcon & 0xFFFFFF;
				int iOverlay = shfi.iIcon >> 24;
				overlayIndex = iOverlay;
				return idx;
				*/
				overlayIndex = shfi.iIcon >> 24;
				return shfi.iIcon & 0xFFFFFF;
			}
		}

		/*
		public int GetIconIndex(IntPtr path) {
			var options = SHGetFileInfoOptions.SysIconIndex | SHGetFileInfoOptions.Icon | SHGetFileInfoOptions.Pidl;
			var shfi = new SHFILEINFO();
			var shfiSize = Marshal.SizeOf(shfi.GetType());
			IntPtr retVal = Win32Api.SHGetFileInfo(path, FileAttributes.None, ref shfi, shfiSize, options);
			if (shfi.hIcon != IntPtr.Zero) {
				Win32Api.DestroyIcon(shfi.hIcon);
			}

			if (retVal.Equals(IntPtr.Zero)) {
				//throw new Win32Exception(Marshal.GetLastWin32Error());

				return 0;
			}
			else {
				//brakes stack on optimized build
				//int idx = shfi.iIcon & 0xFFFFFF;
				//int iOverlay = shfi.iIcon >> 24;
				//overlayIndex = iOverlay;
				//return idx;
				

				return shfi.iIcon & 0xFFFFFF;
			}
		}
		*/

		/*
		public IntPtr GetHIcon(int index) {
			IntPtr hIcon;
			var hresult = this._ImageList.GetIcon(index, ImageListDrawOptions.PreserveAlpha, out hIcon);
			Marshal.ThrowExceptionForHR(hresult);
			if (hIcon != IntPtr.Zero) {
				return hIcon;
			}
			else {
				throw new Win32Exception();
			}
		}
		*/

		/*
		public Icon GetIcon(int index) {
			return this.GetIcon(index, ImageListDrawOptions.Async);
		}
		*/

		/*
		/// <summary>
		/// Creates an icon from an image and a mask in an image list.
		/// </summary>
		/// <param name="index">A value of type int that contains the index of the image. </param>
		/// <param name="options">A combination of flags that specify the drawing style.</param>
		/// <returns>The created icon</returns>
		public Icon GetIcon(int index, ImageListDrawOptions options = ImageListDrawOptions.Async) {
			IntPtr hIcon;
			var hresult = this._ImageList.GetIcon(index, options, out hIcon);
			Marshal.ThrowExceptionForHR(hresult);

			if (hIcon == IntPtr.Zero)
				throw new Win32Exception();
			else
				return Icon.FromHandle(hIcon);
		}
		*/

		/*
		public Icon GetIcon(IntPtr path) {
			return this.GetIcon(path, ImageListDrawOptions.Normal);
		}
		*/

		/*
		[Obsolete("Not Used", true)]
		public Icon GetIcon(IntPtr path, ImageListDrawOptions options = ImageListDrawOptions.Normal) {
			return this.GetIcon(this.GetIconIndex(path), options);
		}
		*/

		/*
		[Obsolete("Not Used", true)]
		public struct Int32Size {
			public static Int32Size Empty {
				get { return new Int32Size(); }
			}

			public int Width { get; set; }
			public int Height { get; set; }

			public Int32Size(int width, int height)
				: this() {
				Width = width;
				Height = height;
			}
		}
		*/

		/*
		[Obsolete("Not Used, Try to Delete!!!", true)]
		private Int32Size GetSizePixels() {
			int x;
			int y;
			var hresult = this._ImageList.GetIconSize(out x, out y);
			Marshal.ThrowExceptionForHR(hresult);
			return new Int32Size(x, y);
		}
		*/

		/// <summary>
		/// Draws the icon
		/// </summary>
		/// <param name="hdc">A handle to the destination device context.</param>
		/// <param name="overlayIndex">The index of the overlay</param>
		/// <param name="location">The x and y coordinates that specifies where the image is drawn.</param>
		/// <param name="newSize">The new size of the image (Double Check)</param>
		public void DrawOverlay(IntPtr hdc, int overlayIndex, Point location, int newSize = -1) {
			DrawInternal(hdc, GetIndexOfOverlay(overlayIndex), 0, ImageListDrawOptions.Normal | ImageListDrawOptions.Transparent, ImageListDrawStates.Normal, 0, location, newSize);
		}

		/// <summary>
		/// Draws the icon
		/// </summary>
		/// <param name="hdc">A handle to the destination device context.</param>
		/// <param name="index">The zero-based index of the image to be drawn.</param>
		/// <param name="location">The x and y coordinates that specifies where the image is drawn.</param>
		/// <param name="newSize">The new size of the image (Double Check)</param>
		/// <param name="hidden"></param>
		public void DrawIcon(IntPtr hdc, int index, Point location, int newSize = -1, Boolean hidden = false) {
			DrawInternal(hdc, index, 0, hidden ? ImageListDrawOptions.Selected | ImageListDrawOptions.Transparent : ImageListDrawOptions.Image | ImageListDrawOptions.Transparent, ImageListDrawStates.Alpha, hidden ? 192 : 255, location, newSize);
		}

		/*
[Obsolete("Not Used, Try to Delete!!!", true)]
public void Draw(IntPtr hdc, int index, int overlayIndex, ImageListDrawOptions options, Point location, int newSize = -1) {
	this.DrawInternal(hdc, index, overlayIndex, options, ImageListDrawStates.Normal, 0, location, newSize);
}
*/

		/// <summary>
		/// Draws the icon
		/// </summary>
		/// <param name="hdc">A handle to the destination device context.</param>
		/// <param name="index">The zero-based index of the image to be drawn.</param>
		/// <param name="overlayIndex">The index of the overlay</param>
		/// <param name="options"></param>
		/// <param name="state">A flag that specifies the drawing state. This member can contain one or more image list state flags. You must use comctl32.dll version 6 to use this member. See the Remarks.</param>
		/// <param name="alpha">Used with the alpha blending effect.</param>
		/// <param name="location">The x and y coordinates that specifies where the image is drawn.</param>
		/// <param name="newSize">The new size of the image (Double Check)</param>
		private void DrawInternal(IntPtr hdc, int index, int overlayIndex, ImageListDrawOptions options, ImageListDrawStates state, int alpha, Point location, int newSize) {
			var param = new IMAGELISTDRAWPARAMS() {
				//himl = this.Handle;
				himl = this.Handle,
				hdcDst = hdc,
				rgbBk = -1,
				i = index,
				x = location.X,
				y = location.Y,
				fStyle = ((int)options | (overlayIndex << 8) | (newSize == -1 ? 0 : (int)ImageListDrawOptions.Scale)),
				fState = state,
				Frame = alpha
			};

			param.cx = param.cy = newSize == -1 ? 0 : newSize;
			param.cbSize = Marshal.SizeOf(param);

			var hresult = this._ImageList.Draw(ref param);
			Marshal.ThrowExceptionForHR(hresult);
		}

		/// <summary>
		/// Retrieves a specified image from the list of images used as overlay masks.
		/// </summary>
		/// <param name="overlayIndex">A value of type int that contains the one-based index of the overlay mask. </param>
		/// <returns>A pointer to an int that receives the zero-based index of an image in the image list. This index identifies the image that is used as an overlay mask. </returns>
		public int GetIndexOfOverlay(int overlayIndex) {
			int idx;
			var hresult = this._ImageList.GetOverlayImage(overlayIndex, out idx);
			Marshal.ThrowExceptionForHR(hresult);
			return idx;
		}

		/*
		[Obsolete("Not Used, Try to Delete!!!", true)]
		[DllImport("comctl32.dll", SetLastError = true)]
		static extern IntPtr ImageList_Create(int cx, int cy, uint flags, int cInitial, int cGrow);

		[Obsolete("Not Used, Try to Delete!!!", true)]
		[DllImport("Comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool ImageList_Destroy(IntPtr himl);
		*/
		public IntPtr Handle { get; set; }
		#endregion

		#region IDisposable

		private bool _Disposed = false;

		public void Dispose() {
			ComCtl32.IntImageList_Destroy(this.Handle);
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		public virtual void Dispose(bool disposing) {
			if (!this._Disposed) {
				if (this._ImageList != null) {
					Marshal.ReleaseComObject(this._ImageList);
				}
				this._ImageList = null;
			}
			this._Disposed = true;
		}

		~ImageList() {
			this.Dispose(false);
		}

		#endregion

		public ImageList(ImageListSize size) {
			//this._Size = size;
			//this._SizePixels = new Lazy<Int32Size>(this.GetSizePixels);
			var handle = IntPtr.Zero;
			var hresult = Win32Api.SHGetImageList(size, ref IID_ImageList2, out this._ImageList);
			Win32Api.SHGetImageListHandle(size, ref IID_ImageList2, ref handle);
			this.Handle = handle;
			Marshal.ThrowExceptionForHR(hresult);
		}

		/*
		public ImageList(Int32 size) {
			var CLSID_ImageList = Guid.Parse("7C476BA2-02B1-48f4-8048-B24619DDC058");
			var refIImageList2 = typeof(IImageList).GUID;
			var IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
			var ptr = IntPtr.Zero;
			//var result = Ole32.CoCreateInstance(ref CLSID_ImageList, IntPtr.Zero, Ole32.CLSCTX.INPROC_SERVER, ref IID_IUnknown, out ptr);
			ptr = ComCtl32.ImageList_Create(size, size, 0x00000020 | 0x00010000 | 0x00020000, 0, 1);
			this._ImageList = (IImageList2)Marshal.GetObjectForIUnknown(ptr);
			this.Handle = ptr;
		}
		*/

		/*
		public void ExpandImageList(Int32 count) {
			this._ImageList.SetImageCount(count);
		}
		*/

		/*
		public Int32 AddImage(IntPtr hBitmap) {
			var index = this.GetCount();

			this._ImageList.SetImageCount(index + 1);
			this._ImageList.Replace2(index, hBitmap, IntPtr.Zero, IntPtr.Zero, 0x0001 | 0x0010);
			return index;
		}
		*/
		
		/*
		public void SetSize(Int32 size) {
			this._ImageList.SetIconSize(size, size);
		}
		*/

		/*
		public void ReplaceImage(Int32 index, IntPtr hBitmap) {
			if (index > this.GetCount()) return;
			this._ImageList.Replace2(index, hBitmap, IntPtr.Zero, IntPtr.Zero, 0x0001 | 0x0010);
		}
		*/

		/*
		public void SetOverlayImage(Int32 index, Int32 overlay) {
			this._ImageList.SetOverlayImage(index, overlay);
		}
		*/

		/*
		public Int32 GetCount() {
			var count = -1;
			this._ImageList.GetImageCount(ref count);
			return count;
		}
		*/

		/*
		public Size GetOriginalImageSize(Int32 index) {
			Int32 width = 0;
			Int32 height = 0;
			this._ImageList.GetOriginalSize(index, 0x00000000, out width, out height);
			return new Size(width, height);
		}
		*/

		/*
		public void SetOriginalImageSize(Int32 index, Int32 width, Int32 height) {
			this._ImageList.SetOriginalSize(index, width, height);
		}
		*/
	}

	#region enum

	public enum ImageListSize : int {
		Large = 0,
		Small = 1,
		ExtraLarge = 2,
		SystemSmall = 3,
		/// <summary>
		/// Vista or higher
		/// </summary>
		Jumbo = 4,
	}

	[Flags]
	public enum ImageListDrawOptions : uint {
		/// <summary>
		/// Draw item normally.
		/// </summary>
		Normal = 0x0,
		/// <summary>
		/// Draw item transparently.
		/// </summary>
		Transparent = 0x1,
		/// <summary>
		/// Draw item blended with 25% of the specified foreground colour
		/// or the Highlight colour if no foreground colour specified.
		/// </summary>
		Blend25 = 0x2,
		/// <summary>
		/// Draw item blended with 50% of the specified foreground colour
		/// or the Highlight colour if no foreground colour specified.
		/// </summary>
		Selected = 0x4,
		/// <summary>
		/// Draw the icon's mask
		/// </summary>
		Mask = 0x10,
		/// <summary>
		/// Draw the icon image without using the mask
		/// </summary>
		Image = 0x20,
		/// <summary>
		/// Draw the icon using the ROP specified.
		/// </summary>
		RasterOperation = 0x40,
		/// <summary>
		/// Preserves the alpha channel in dest. XP only.
		/// </summary>
		PreserveAlpha = 0x1000,
		/// <summary>
		/// Scale the image to cx, cy instead of clipping it.  XP only.
		/// </summary>
		Scale = 0x2000,
		/// <summary>
		/// Scale the image to the current DPI of the display. XP only.
		/// </summary>
		DpiScale = 0x4000,
		/// <summary>
		/// Vista or higher
		/// </summary>
		Async = 0x8000,
	}

	[Flags]
	public enum ImageListDrawStates {
		/// <summary>
		///   The image state is not modified.
		/// </summary>
		Normal = (0x00000000),
		/// <summary>
		///   Adds a glow effect to the icon, which causes the icon to appear to glow 
		///   with a given color around the edges. (Note: does not appear to be
		///   implemented)
		/// </summary>
		Glow = (0x00000001),
		//The color for the glow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
		/// <summary>
		///   Adds a drop shadow effect to the icon. (Note: does not appear to be
		///   implemented)
		/// </summary>
		Shadow = (0x00000002),
		//The color for the drop shadow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
		/// <summary>
		///   Saturates the icon by increasing each color component 
		///   of the RGB triplet for each pixel in the icon. (Note: only ever appears
		///   to result in a completely unsaturated icon)
		/// </summary>
		Saturates = (0x00000004),
		// The amount to increase is indicated by the frame member in the IMAGELISTDRAWPARAMS method. 
		/// <summary>
		///   Alpha blends the icon. Alpha blending controls the transparency 
		///   level of an icon, according to the value of its alpha channel. 
		///   (Note: does not appear to be implemented).
		/// </summary>
		Alpha = (0x00000008)
		//The value of the alpha channel is indicated by the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel can be from 0 to 255, with 0 being completely transparent, and 255 being completely opaque. 
	}

	#endregion

	#region IImageList

	[ComImportAttribute()]
	[GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IImageList {
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
		int Draw(
				ref IMAGELISTDRAWPARAMS pimldp);

		[PreserveSig]
		int Remove(
				int i);

		[PreserveSig]
		int GetIcon(
				int i,
				ImageListDrawOptions flags,
				out IntPtr picon);

		[PreserveSig]
		int GetImageInfo(
				int i,
				ref IMAGEINFO pImageInfo);

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
		int Clone(
				ref Guid riid,
				ref IntPtr ppv);

		[PreserveSig]
		int GetImageRect(
				int i,
				ref Rectangle prc);

		[PreserveSig]
		int GetIconSize(
				out int cx,
				out int cy);

		[PreserveSig]
		int SetIconSize(
				int cx,
				int cy);

		[PreserveSig]
		int GetImageCount(
				ref int pi);

		[PreserveSig]
		int SetImageCount(
				int uNewCount);

		[PreserveSig]
		int SetBkColor(
				int clrBk,
				ref int pclr);

		[PreserveSig]
		int GetBkColor(
				ref int pclr);

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
		int DragLeave(
				IntPtr hwndLock);

		[PreserveSig]
		int DragMove(
				int x,
				int y);

		[PreserveSig]
		int SetDragCursorImage(
				ref IImageList punk,
				int iDrag,
				int dxHotspot,
				int dyHotspot);

		[PreserveSig]
		int DragShowNolock(
				int fShow);

		[PreserveSig]
		int GetDragImage(
				ref Point ppt,
				ref Point pptHotspot,
				ref Guid riid,
				ref IntPtr ppv);

		[PreserveSig]
		int GetItemFlags(
				int i,
				ref int dwFlags);

		[PreserveSig]
		int GetOverlayImage(
				int iOverlay,
				out int piIndex);
	}

	[ComImportAttribute()]
	[GuidAttribute("192B9D83-50FC-457B-90A0-2B82A8B5DAE1")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IImageList2 {
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
		int Draw(
			ref IMAGELISTDRAWPARAMS pimldp);

		[PreserveSig]
		int Remove(
			int i);

		
		/// <summary>
		/// Creates an icon from an image and a mask in an image list. 
		/// </summary>
		/// <param name="i">A value of type int that contains the index of the image.</param>
		/// <param name="flags">A combination of flags that specify the drawing style. For a list of values, see IImageList::Draw. </param>
		/// <param name="picon">A pointer to an int that contains the handle to the icon if successful, or NULL if otherwise.</param>
		/// <returns></returns>
		[PreserveSig]
		int GetIcon(
			int i,
			ImageListDrawOptions flags,
			out IntPtr picon);
		

		[PreserveSig]
		int GetImageInfo(
			int i,
			ref IMAGEINFO pImageInfo);

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
		int Clone(
			ref Guid riid,
			ref IntPtr ppv);

		[PreserveSig]
		int GetImageRect(
			int i,
			ref Rectangle prc);

		[PreserveSig]
		int GetIconSize(
			out int cx,
			out int cy);

		[PreserveSig]
		int SetIconSize(
			int cx,
			int cy);

		[PreserveSig]
		int GetImageCount(
			ref int pi);

		[PreserveSig]
		int SetImageCount(
			int uNewCount);

		[PreserveSig]
		int SetBkColor(
			int clrBk,
			ref int pclr);

		[PreserveSig]
		int GetBkColor(
			ref int pclr);

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
		int DragLeave(
			IntPtr hwndLock);

		[PreserveSig]
		int DragMove(
			int x,
			int y);

		[PreserveSig]
		int SetDragCursorImage(
			ref IImageList punk,
			int iDrag,
			int dxHotspot,
			int dyHotspot);

		[PreserveSig]
		int DragShowNolock(
			int fShow);

		[PreserveSig]
		int GetDragImage(
			ref Point ppt,
			ref Point pptHotspot,
			ref Guid riid,
			ref IntPtr ppv);

		[PreserveSig]
		int GetItemFlags(
			int i,
			ref int dwFlags);

		/// <summary>
		/// Retrieves a specified image from the list of images used as overlay masks.	
		/// </summary>
		/// <param name="iOverlay">A value of type int that contains the one-based index of the overlay mask.</param>
		/// <param name="piIndex">A pointer to an int that receives the zero-based index of an image in the image list. This index identifies the image that is used as an overlay mask. </param>
		/// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
		[PreserveSig]
		int GetOverlayImage(
			int iOverlay,
			out int piIndex);

		HResult Resize(
				int cxNewIconSize,
				int cyNewIconSize);

		HResult GetOriginalSize(
				/* [in] */ int iImage,
				/* [in] */ uint dwFlags,
				/* [annotation][out] */
				out int pcx,
				/* [annotation][out] */
				out int pcy);

		HResult SetOriginalSize(
			/* [in] */ int iImage,
			/* [in] */ int cx,
			/* [in] */ int cy);

		HResult SetCallback(
			/* [annotation][unique][in] */
			IntPtr punk);

		HResult GetCallback(
			/* [in] */ ref Guid riid,
			/* [annotation][iid_is][out] */
			out IntPtr ppv);

		HResult ForceImagePresent(
			/* [in] */ int iImage,
			uint dwFlags);

		HResult DiscardImages(
			/* [in] */ int iFirstImage,
			/* [in] */ int iLastImage,
			/* [in] */ uint dwFlags);

		HResult PreloadImages(
			/* [annotation][in] */
			ref IMAGELISTDRAWPARAMS pimldp);

		HResult GetStatistics(
			/* [annotation][out][in] */
			out IntPtr pils);

		HResult Initialize(
			/* [in] */ int cx,
			/* [in] */ int cy,
			/* [in] */ uint flags,
			/* [in] */ int cInitial,
			/* [in] */ int cGrow);
		[PreserveSig]
		HResult Replace2(
			/* [in] */ int i,
			/* [annotation][in] */
			IntPtr hbmImage,
			/* [annotation][unique][in] */
			IntPtr hbmMask,
			/* [annotation][unique][in] */
			IntPtr punk,
			/* [in] */ uint dwFlags);

		HResult ReplaceFromImageList(
			/* [in] */ int i,
			/* [annotation][in] */
			IImageList pil,
			/* [in] */ int iSrc,
			/* [annotation][unique][in] */
			IntPtr punk,
			/* [in] */ uint dwFlags);
	}

	#endregion

	#region Structs

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGELISTDRAWPARAMS {
		/// <summary>The size of this structure, in bytes. </summary>
		public int cbSize;
		/// <summary>A handle to the image list that contains the image to be drawn. </summary>
		public IntPtr himl;
		/// <summary>The zero-based index of the image to be drawn. </summary>
		public int i;
		/// <summary>A handle to the destination device context. </summary>
		public IntPtr hdcDst;
		/// <summary>The x-coordinate that specifies where the image is drawn. </summary>
		public int x;
		/// <summary>The y-coordinate that specifies where the image is drawn. </summary>
		public int y;
		/// <summary>A value that specifies the number of pixels to draw, relative to the upper-left corner of the drawing operation as specified by xBitmap and yBitmap. If cx and cy are zero, then Draw draws the entire valid section. The method does not ensure that the parameters are valid.</summary>
		public int cx;
		/// <summary>A value that specifies the number of pixels to draw, relative to the upper-left corner of the drawing operation as specified by xBitmap and yBitmap. If cx and cy are zero, then Draw draws the entire valid section. The method does not ensure that the parameters are valid.</summary>
		public int cy;
		/// <summary>The x-coordinate that specifies the upper-left corner of the drawing operation in reference to the image itself. Pixels of the image that are to the left of xBitmap and above yBitmap do not appear. </summary>
		public int xBitmap;        // x offest from the upperleft of bitmap
		/// <summary>The y-coordinate that specifies the upper-left corner of the drawing operation in reference to the image itself. Pixels of the image that are to the left of xBitmap and above yBitmap do not appear. </summary>
		public int yBitmap;        // y offset from the upperleft of bitmap
		/// <summary>The image background color. This parameter can be an application-defined RGB value or one of the following values. </summary>
		public int rgbBk;
		/// <summary>The image foreground color. This member is used only if fStyle includes the ILD_BLEND25 or ILD_BLEND50 flag. This parameter can be an application-defined RGB value or one of the following values: </summary>
		public int rgbFg;
		/// <summary>A flag specifying the drawing style and, optionally, the overlay image. See the comments section at the end of this topic for information on the overlay image. This member can contain one or more image list drawing flags. </summary>
		public int fStyle;
		/// <summary>A value specifying a raster operation code. These codes define how the color data for the source rectangle will be combined with the color data for the destination rectangle to achieve the final color. This member is ignored if fStyle does not include the ILD_ROP flag. Some common raster operation codes include: </summary>
		public int dwRop;
		/// <summary>A flag that specifies the drawing state. This member can contain one or more image list state flags. You must use comctl32.dll version 6 to use this member. See the Remarks.</summary>
		public ImageListDrawStates fState;
		/// <summary>Used with the alpha blending effect.</summary>
		public int Frame;
		/// <summary>A color used for the glow and shadow effects. You must use comctl32.dll version 6 to use this member. See the Remarks. </summary>
		public int crEffect;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGEINFO {
		public IntPtr hbmImage;
		public IntPtr hbmMask;
		public int Unused1;
		public int Unused2;
		public Rectangle rcImage;
	}

	#endregion

}
