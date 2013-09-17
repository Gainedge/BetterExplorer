using Microsoft.WindowsAPICodePack.Shell;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.WindowsAPICodePack.Controls.WindowsForms
{
  /// Win32APIðµ¤ÃINXB
  /// </summary>
  public static class Win32Api
  {
    [DllImport("shell32.dll", EntryPoint = "ExtractIconEx", CharSet = CharSet.Auto)]
    public static extern int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)] string file, int index, out IntPtr largeIconHandle, out IntPtr smallIconHandle, int icons);

    [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo", CharSet = CharSet.Auto)]
    public static extern IntPtr SHGetFileInfo(IntPtr pszPath, FileAttributes attr, ref ImageList.SHFileInfo psfi, int cbSizeFileInfo, SHGetFileInfoOptions uFlags);

    [DllImport("shell32.dll", EntryPoint = "#727")]
    public extern static int SHGetImageList(ImageListSize iImageList, ref Guid riid, out IImageList2 ppv);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    public extern static IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr newValue);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    public extern static IntPtr SetWindowLongPtr64(IntPtr hwnd, int nIndex, IntPtr newValue);

    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(String lpFileName);
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, String lpProcName);
    [DllImport("kernel32.dll")]
    public static extern Boolean FreeLibrary(IntPtr hLibModule);

    [DllImport("KERNEL32.DLL", EntryPoint = "CloseHandle", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("KERNEL32.DLL", EntryPoint = "UnmapViewOfFile", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool UnmapViewOfFile(IntPtr hMap);

    [DllImport("KERNEL32.DLL", EntryPoint = "GetLastError", CharSet = CharSet.Auto)]
    public static extern int GetLastError();

    [DllImport("KERNEL32.DLL", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Auto)]
    public static extern void CopyMemory(IntPtr dst, IntPtr src, IntPtr length);

    [DllImport("USER32.DLL", EntryPoint = "GetActiveWindow", CharSet = CharSet.Auto)]
    public static extern IntPtr GetActiveWindow();

    [DllImport("USER32.DLL", EntryPoint = "SetActiveWindow", CharSet = CharSet.Auto)]
    public static extern IntPtr SetActiveWindow(IntPtr hwnd);


    /// <summary>
    /// EChEð\¦·éB
    /// </summary>
    /// <param name="hwnd">\¦·éEChEÌnh</param>
    /// <param name="cmdShow">EChEÌóÔ</param>
    /// <returns>int</returns>

    /// <summary>
    /// EChEðOÊÉ\¦·éB
    /// </summary>
    /// <param name="hWnd">OÊ¢\¦·éEChEÌnh</param>
    /// <returns>bool</returns>
    [DllImport("USER32.DLL", EntryPoint = "SetForegroundWindow", CharSet = CharSet.Auto)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("USER32.DLL", EntryPoint = "GetForegroundWindow", CharSet = CharSet.Auto)]
    public static extern IntPtr GetForegroundWindow();


    [DllImport("USER32.DLL", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hwnd, int msg, int wParam, int lParam);


    /// <summary>
    /// IMEÌReLXgðæ¾·éB
    /// </summary>
    /// <param name="hwnd">hwnd</param>
    /// <returns>ReLXgÌ|C^</returns>
    [DllImport("imm32.dll", EntryPoint = "ImmGetContext", CharSet = CharSet.Auto)]
    public static extern IntPtr ImmGetContext(IntPtr hwnd);

    /// <summary>
    /// IMEÌóÔðÏX·éB
    /// </summary>
    /// <param name="hIMC">IMEÌReLXgÌ|C^</param>
    /// <param name="fOpen">IMEðJ­©Ç¤©</param>
    /// <returns>bool</returns>
    [DllImport("imm32.dll", EntryPoint = "ImmSetOpenStatus", CharSet = CharSet.Auto)]
    public static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);

    public delegate bool EnumWindowsProc(IntPtr hWnd, object lParam);

    [DllImport("user32", EntryPoint = "EnumWindows", CharSet = CharSet.Auto)]
    public static extern int EnumWindows(EnumWindowsProc lpEnumFunc, object lParam);

    [DllImport("user32", EntryPoint = "GetWindowThreadProcessId", CharSet = CharSet.Auto)]
    public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);


    [DllImport("user32", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
    public static extern bool SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)] string text);


    public static readonly IntPtr HWND_TOP = IntPtr.Zero;
    public static readonly IntPtr HWND_BOTTOM = (IntPtr)1;
    public static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
    public static readonly IntPtr HWND_NOTOPMOST = (IntPtr)(-2);

    [DllImport("user32", EntryPoint = "GetWindowRect", CharSet = CharSet.Auto)]
    public static extern int GetWindowRect(IntPtr hWnd, ref Rectangle rect);

    [DllImport("user32", EntryPoint = "IsWindow", CharSet = CharSet.Auto)]
    public static extern bool IsWindow(IntPtr hWnd);



    [DllImport("user32", EntryPoint = "IsWindowVisible", CharSet = CharSet.Auto)]
    public static extern bool IsWindowVisible(IntPtr Handle);

    [DllImport("user32", EntryPoint = "IsWindowEnabled", CharSet = CharSet.Auto)]
    public static extern bool IsWindowEnabled(IntPtr Handle);

    [DllImport("user32", EntryPoint = "EnableWindow", CharSet = CharSet.Auto)]
    public static extern bool IsWindowEnabled(IntPtr Handle, bool enable);


   

    [DllImport("user32.dll", EntryPoint = "GetKeyboardState", CharSet = CharSet.Auto)]
    internal static extern int GetKeyboardState(byte[] lpKeyState);

    public static byte[] GetKeyboardState()
    {
      byte[] keyState = new byte[256];
      int result = GetKeyboardState(keyState);
      if (result == 0)
      {
        throw new Win32Exception();
      }
      return keyState;
    }

   
    [DllImport("user32.dll", EntryPoint = "GetDC", CharSet = CharSet.Auto)]
    public static extern IntPtr GetDeviceContext(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "ReleaseDC", CharSet = CharSet.Auto)]
    public static extern IntPtr ReleaseDeviceContext(IntPtr hWnd, IntPtr hDc);

    [DllImport("gdi32.dll", EntryPoint = "BitBlt", CharSet = CharSet.Auto)]
    public static extern bool BitBlt(IntPtr hdcDst, int xDst, int yDsk, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rasterOp);
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

    [DllImport("kernel32", EntryPoint = "GetLogicalDrives", CharSet = CharSet.Auto)]
    internal static extern long InternalGetLogicalDrives();

    [Obsolete]
    public static string[] GetLogicalDrives()
    {
      List<string> drives = new List<string>();
      long d = Win32Api.InternalGetLogicalDrives();
      for (int i = 0; i < 26; i++)
      {
        if (((1 << i) & d) > 0)
        {
          drives.Add(((char)('A' + i)) + ":\\");
        }
      }
      return drives.ToArray();
    }

  }

  public enum IconSize
  {
    Large = 0x0,
    Small = 0x1,
  }

  [Flags]
  public enum SHGetFileInfoOptions : uint
  {
    Icon = 0x000000100,    // get icon
    DisplayName = 0x000000200,    // get display name
    TypeName = 0x000000400,    // get type name
    Attributes = 0x000000800,    // get attributes
    IconLocation = 0x000001000,    // get icon location
    ExeType = 0x000002000,    // return exe type
    SysIconIndex = 0x000004000,    // get system icon index
    LinkOverlay = 0x000008000,    // put a link overlay on icon
    Selected = 0x000010000,    // show icon in selected state
    // (NTDDI_VERSION >= NTDDI_WIN2K)
    SpecifiedAttributes = 0x000020000,    // get only specified attributes
    LargeIcon = 0x000000000,    // get large icon
    SmallIcon = 0x000000001,    // get small icon
    OpenIcon = 0x000000002,    // get open icon
    ShellIconSize = 0x000000004,    // get shell size icon
    Pidl = 0x000000008,    // pszPath is a pidl
    UseFileAttributes = 0x000000010,    // use passed dwFileAttribute
    // (_WIN32_IE >= 0x0500)
    AddOverlays = 0x000000020,    // apply the appropriate overlays
    OverlayIndex = 0x000000040,    // Get the index of the overlay
  }

  [Flags]
  public enum FileAttributes : uint
  {
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
	/// This works on XP or higher.
	/// </remarks>
	public class ImageList : IDisposable {
		private ImageListSize _Size;
		private IImageList2 _ImageList;
		private static Guid IID_ImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
    private static Guid IID_ImageList2 = new Guid("192B9D83-50FC-457B-90A0-2B82A8B5DAE1");
    

		public ImageList(ImageListSize size){
			this._Size = size;
			this._SizePixels = new Lazy<Int32Size>(this.GetSizePixels);
			var hresult = Win32Api.SHGetImageList(size, ref IID_ImageList2, out this._ImageList);
			Marshal.ThrowExceptionForHR(hresult);
		}

		#region Method

		public int GetIconIndex(IntPtr path){
			var options = SHGetFileInfoOptions.SysIconIndex | SHGetFileInfoOptions.Pidl;
			var shfi = new SHFileInfo();
			var shfiSize = Marshal.SizeOf(shfi.GetType());
			IntPtr retVal = Win32Api.SHGetFileInfo(path, FileAttributes.None, ref shfi, shfiSize, options);
			if(shfi.hIcon != IntPtr.Zero){
				Win32Api.DestroyIcon(shfi.hIcon);
			}

			if (retVal.Equals(IntPtr.Zero)){
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}else{
				return shfi.iIcon;
			}
		}
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHFileInfo
    {
      public IntPtr hIcon;
      public int iIcon;
      public uint dwAttributes;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string szDisplayName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
      public string szTypeName;
    }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="overlayIndex">Index of the overlay icon that use for Draw or GetIndexOfOverlay method.</param>
		/// <returns></returns>
		public int GetIconIndexWithOverlay(IntPtr path, out int overlayIndex){
			var options = SHGetFileInfoOptions.SysIconIndex | SHGetFileInfoOptions.OverlayIndex | SHGetFileInfoOptions.Icon | SHGetFileInfoOptions.AddOverlays | SHGetFileInfoOptions.Pidl;
			var shfi = new SHFileInfo();
			var shfiSize = Marshal.SizeOf(shfi.GetType());
			IntPtr retVal = Win32Api.SHGetFileInfo(path, FileAttributes.None, ref shfi, shfiSize, options);
			if(shfi.hIcon != IntPtr.Zero){
				Win32Api.DestroyIcon(shfi.hIcon);
			}

			if (retVal.Equals(IntPtr.Zero)){
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}else{
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

		public Icon GetIcon(int index){
			return this.GetIcon(index, ImageListDrawOptions.Normal);
		}
		public Icon GetIcon(int index, ImageListDrawOptions options){
			IntPtr hIcon;
			var hresult = this._ImageList.GetIcon(index, options, out hIcon);
			Marshal.ThrowExceptionForHR(hresult);
			if(hIcon != IntPtr.Zero){
				return Icon.FromHandle(hIcon);
			}else{
				throw new Win32Exception();
			}
		}

		public Icon GetIcon(IntPtr path){
			return this.GetIcon(path, ImageListDrawOptions.Normal);
		}
		public Icon GetIcon(IntPtr path, ImageListDrawOptions options){
			return this.GetIcon(this.GetIconIndex(path), options);
		}
    public struct Int32Size
    {
      public static Int32Size Empty
      {
        get { return new Int32Size(); }
      }

      public int Width { get; set; }
      public int Height { get; set; }

      public Int32Size(int width, int height)
        : this()
      {
        Width = width;
        Height = height;
      }
    }

		private Int32Size GetSizePixels(){
			int x;
			int y;
			var hresult = this._ImageList.GetIconSize(out x, out y);
			Marshal.ThrowExceptionForHR(hresult);
			return new Int32Size(x, y);
		}


    public void DrawOverlay(IntPtr hdc, int overlayIndex, Point location, int newSize = -1)
    {
      DrawInternal(hdc, GetIndexOfOverlay(overlayIndex), 0, ImageListDrawOptions.Normal  | ImageListDrawOptions.Transparent, ImageListDrawStates.Normal, 0, location, newSize);
    }
		public void Draw(IntPtr hdc, int index, int overlayIndex, ImageListDrawOptions options, Point location, int newSize = -1){
			 this.DrawInternal(hdc, index, overlayIndex, options, ImageListDrawStates.Normal, 0, location, newSize);
		}

    private void DrawInternal(IntPtr hdc, int index, int overlayIndex, ImageListDrawOptions options, ImageListDrawStates state, int alpha, Point location, int newSize)
    {
				var param = new IMAGELISTDRAWPARAMS();
				param.cbSize = Marshal.SizeOf(param);
				param.himl = this.Handle;
				param.hdcDst = hdc;
        param.rgbBk = -1;
        param.i = index;
        param.x = location.X;
        param.y = location.Y;
        param.cx = param.cy = newSize == -1 ? 0 : newSize;
        param.fStyle = ((int)options | (overlayIndex << 8) | (newSize == -1 ? 0 : (int)ImageListDrawOptions.Scale));
				param.fState = state;
				param.Frame = alpha;
        
				var hresult = this._ImageList.Draw(ref param);
				Marshal.ThrowExceptionForHR(hresult);
		}

		public int GetIndexOfOverlay(int overlayIndex){
			int idx;
			var hresult = this._ImageList.GetOverlayImage(overlayIndex, out idx);
			Marshal.ThrowExceptionForHR(hresult);
			return idx;
		}
    [DllImport("comctl32.dll", SetLastError = true)]
    static extern IntPtr ImageList_Create(int cx, int cy, uint flags, int cInitial, int cGrow);
    [DllImport("Comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool ImageList_Destroy(IntPtr himl);
		#endregion

		#region Property
    public void SetSize (int size){
      ImageList_Destroy(this.Handle);
      this._ImageList = Marshal.GetObjectForIUnknown(ImageList_Create(size, size, 0x00020000 | 0x00000020, 30, 30)) as IImageList2;
      this._ImageList.SetImageCount(3000);
      //this._ImageList.SetIconSize(size, size);
      int pi = -1;
      this._ImageList.GetImageCount(ref pi);
      var obj = (ShellObject)KnownFolders.Windows;
      for (int i = 0; i < 30; i++)
      {
        var res = this._ImageList.Replace(i, obj.GetShellThumbnail(size).GetHbitmap(), IntPtr.Zero);
      }
    }
		public ImageListSize Size{
			get{
				return this._Size;
			}
		}

		private Lazy<Int32Size> _SizePixels;
		public int Width{
			get{
				return this._SizePixels.Value.Width;
			}
		}

		public int Height{
			get{
				return this._SizePixels.Value.Height;
			}
		}

		public IntPtr Handle{
			get{
				return Marshal.GetIUnknownForObject(this._ImageList);
			}
		}

		public static ImageListSize MaxSize{
			get{
				if(Environment.OSVersion.Platform == PlatformID.Win32NT){
					if(Environment.OSVersion.Version.Major >= 6){
						return ImageListSize.Jumbo;
					}else{
						return ImageListSize.ExtraLarge;
					}
				}
				return ImageListSize.Jumbo;
			}
		}

		#endregion

		#region IDisposable

		private bool _Disposed = false;

		public void Dispose(){
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		public virtual void Dispose(bool disposing){
			if (!this._Disposed){
				if (this._ImageList != null){
					Marshal.ReleaseComObject(this._ImageList);
				}
				this._ImageList = null;
			}
			this._Disposed = true;
		}

		~ImageList(){
			this.Dispose(false);
		}
	
		#endregion
	}

	#region enum

	public enum ImageListSize : int{
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
	public enum ImageListDrawOptions : uint{
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
	public enum ImageListDrawStates{
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
	public interface IImageList{
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
	};

  [ComImportAttribute()]
  [GuidAttribute("192B9D83-50FC-457B-90A0-2B82A8B5DAE1")]
  [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IImageList2 : IImageList
  {
    [PreserveSig]
    int Initialize(
      int cx,
      int cy,
      uint flags,
      int cInitial,
      int cGrow);
    [PreserveSig]
    int Replace2(
      int i,
      IntPtr hbmImage,
      IntPtr hbmMask,
      IntPtr punk,
      uint dwFlags);
  };

	#endregion

	#region Structs

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGELISTDRAWPARAMS{
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
		public ImageListDrawStates fState;
		public int Frame;
		public int crEffect;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGEINFO{
		public IntPtr hbmImage;
		public IntPtr hbmMask;
		public int Unused1;
		public int Unused2;
		public Rectangle rcImage;
	}

	#endregion

}
