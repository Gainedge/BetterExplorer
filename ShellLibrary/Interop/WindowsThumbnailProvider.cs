using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media.Imaging;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using ShellLibrary.Interop;
using Windows.Storage.FileProperties;
using static BExplorer.Shell.Interop.Gdi32;
using static ThumbnailGenerator.WindowsThumbnailProvider;

namespace ThumbnailGenerator {
  [Flags]
  public enum ThumbnailOptions {
    None = 0x00,
    BiggerSizeOk = 0x01,
    InMemoryOnly = 0x02,
    IconOnly = 0x04,
    ThumbnailOnly = 0x08,
    InCacheOnly = 0x10,
  }

  public class WindowsThumbnailProvider {
    private const String IShellItem2Guid = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";
    private const String IShellImageFactoryGuid = "bcc18b79-ba16-442f-80c4-8a59c30c463b";

    /*
      [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern int SHCreateItemFromParsingName(
          [MarshalAs(UnmanagedType.LPWStr)] string path,
        // The following parameter is not used - binding context.
          IntPtr pbc,
          ref Guid riid,
          [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);
    */
    [DllImport("shell32.dll", PreserveSig = false)]
    internal static extern Int32 SHCreateItemFromIDList(
        IntPtr pidl,
        ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

    [DllImport("shell32.dll", PreserveSig = false)]
    internal static extern Int32 SHCreateItemFromIDList(
      IntPtr pidl,
      ref Guid riid,
      [MarshalAs(UnmanagedType.Interface)] out Object shellItem);

    /*
      [DllImport("gdi32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      internal static extern bool DeleteObject(IntPtr hObject);
    */

    //[ComImport]
    //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    //internal interface IShellItem {
    //  void BindToHandler(IntPtr pbc,
    //      [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
    //      [MarshalAs(UnmanagedType.LPStruct)]Guid riid,
    //      out IntPtr ppv);

    //  void GetParent(out IShellItem ppsi);
    //  void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);
    //  void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
    //  void Compare(IShellItem psi, uint hint, out int piOrder);
    //};

    internal enum SIGDN : UInt32 {
      NORMALDISPLAY = 0,
      PARENTRELATIVEPARSING = 0x80018001,
      PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
      DESKTOPABSOLUTEPARSING = 0x80028000,
      PARENTRELATIVEEDITING = 0x80031001,
      DESKTOPABSOLUTEEDITING = 0x8004c000,
      FILESYSPATH = 0x80058000,
      URL = 0x80068000
    }

    internal enum HResult {
      Ok = 0x0000,
      False = 0x0001,
      InvalidArguments = unchecked((Int32)0x80070057),
      OutOfMemory = unchecked((Int32)0x8007000E),
      NoInterface = unchecked((Int32)0x80004002),
      Fail = unchecked((Int32)0x80004005),
      ElementNotFound = unchecked((Int32)0x80070490),
      TypeElementNotFound = unchecked((Int32)0x8002802B),
      NoObject = unchecked((Int32)0x800401E5),
      Win32ErrorCanceled = 1223,
      Canceled = unchecked((Int32)0x800704C7),
      ResourceInUse = unchecked((Int32)0x800700AA),
      AccessDenied = unchecked((Int32)0x80030005)
    }

    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellItemImageFactory {
      [PreserveSig]
      HResult GetImage(
      [In, MarshalAs(UnmanagedType.Struct)] NativeSize size,
      [In] ThumbnailOptions flags,
      [Out] out IntPtr phbm);
    }

    public enum WTS_ALPHATYPE {
      /// <summary>The bitmap is an unknown format. The Shell tries nonetheless to detect whether the image has an alpha channel.</summary>
      WTSAT_UNKNOWN = 0x0,

      /// <summary>The bitmap is an RGB image without alpha. The alpha channel is invalid and the Shell ignores it.</summary>
      WTSAT_RGB = 0x1,

      /// <summary>The bitmap is an ARGB image with a valid alpha channel.</summary>
      WTSAT_ARGB = 0x2
    }

    [ComImport, Guid("e357fccd-a995-4576-b01f-234630154e96"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IThumbnailProvider {
      /// <summary>Gets a thumbnail image and alpha type.</summary>
      /// <param name="cx">
      /// The maximum thumbnail size, in pixels. The Shell draws the returned bitmap at this size or smaller. The returned bitmap
      /// should fit into a square of width and height cx, though it does not need to be a square image. The Shell scales the bitmap to
      /// render at lower sizes. For example, if the image has a 6:4 aspect ratio, then the returned bitmap should also have a 6:4
      /// aspect ratio.
      /// </param>
      /// <param name="phbmp">
      /// When this method returns, contains a pointer to the thumbnail image handle. The image must be a DIB section and 32 bits per
      /// pixel. The Shell scales down the bitmap if its width or height is larger than the size specified by cx. The Shell always
      /// respects the aspect ratio and never scales a bitmap larger than its original size.
      /// </param>
      /// <param name="pdwAlpha">
      /// When this method returns, contains a pointer to one of the following values from the WTS_ALPHATYPE enumeration.
      /// </param>
      /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
      [PreserveSig]
      HResult GetThumbnail(UInt32 cx, out IntPtr phbmp, out WTS_ALPHATYPE pdwAlpha);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeSize {
      private Int32 width;
      private Int32 height;

      public Int32 Width { set { width = value; } }
      public Int32 Height { set { height = value; } }
    };

    /*
      [StructLayout(LayoutKind.Sequential)]
      public struct RGBQUAD {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
      }
    */
    /*
      public static Bitmap GetThumbnail(string fileName, int width, int height, ThumbnailOptions options) {
        IntPtr hBitmap = GetHBitmap(Path.GetFullPath(fileName), width, height, options);

        try {
          // return a System.Drawing.Bitmap from the hBitmap
          return GetBitmapFromHBitmap(hBitmap);
        } finally {
          // delete HBitmap to avoid memory leaks
          DeleteObject(hBitmap);
        }
      }
    */
    public static IntPtr GetThumbnail(IListItemEx item, Int32 width, Int32 height, ThumbnailOptions options, Boolean isForThumbnailSource) {
      IntPtr hBitmap = GetHBitmap(item, width, height, options, isForThumbnailSource);

      return hBitmap;
    }

    public static IntPtr GetThumbnail(IShellItem nativeShellItem, Int32 width, Int32 height, ThumbnailOptions options) {
      IntPtr hBitmap = GetHBitmap(nativeShellItem, width, height, options);

      return hBitmap;
    }

    /*
      public static Bitmap GetBitmapFromHBitmap(IntPtr nativeHBitmap) {
        Bitmap bmp = Bitmap.FromHbitmap(nativeHBitmap);

        if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
          return bmp;

        return CreateAlphaBitmap(bmp, PixelFormat.Format32bppArgb);
      }
    */

    /*
      public static Bitmap CreateAlphaBitmap(Bitmap srcBitmap, PixelFormat targetPixelFormat) {
        Bitmap result = new Bitmap(srcBitmap.Width, srcBitmap.Height, targetPixelFormat);

        Rectangle bmpBounds = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);

        BitmapData srcData = srcBitmap.LockBits(bmpBounds, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);

        bool isAlplaBitmap = false;

        try {
          for (int y = 0; y <= srcData.Height - 1; y++) {
            for (int x = 0; x <= srcData.Width - 1; x++) {
              Color pixelColor = Color.FromArgb(
                  Marshal.ReadInt32(srcData.Scan0, (srcData.Stride * y) + (4 * x)));

              if (pixelColor.A > 0 & pixelColor.A < 255) {
                isAlplaBitmap = true;
              }

              result.SetPixel(x, y, pixelColor);
            }
          }
        } finally {
          srcBitmap.UnlockBits(srcData);
        }

        if (isAlplaBitmap) {
          return result;
        } else {
          return srcBitmap;
        }
      }
    */

    private static IntPtr GetHBitmap(IListItemEx item, Int32 width, Int32 height, ThumbnailOptions options, Boolean isForThumbnailSource = false) {
      Object nativeShellItem;
      var shellItem2Guid = new Guid(IShellImageFactoryGuid);
      var retCode = SHCreateItemFromIDList(item.PIDL, ref shellItem2Guid, out nativeShellItem);

      if (retCode != 0) {
        return IntPtr.Zero;
      }

      var nativeSize = default(NativeSize);
      nativeSize.Width = width;
      nativeSize.Height = height;

      IntPtr hBitmap;
      var hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, options, out hBitmap);

      var perceivedType = PerceivedType.Unspecified;
      var percTypeVal = item.GetPropertyValue(SystemProperties.PerceivedType, typeof(PerceivedType))?.Value;
      if (percTypeVal != null) {
        perceivedType = (PerceivedType)percTypeVal;
      }

      if ((hr != HResult.Ok || isForThumbnailSource) && (perceivedType == PerceivedType.Image || perceivedType == PerceivedType.Video) && !item.IsFolder && (options & ThumbnailOptions.IconOnly) == 0) {
        var flags = Windows.Storage.FileProperties.ThumbnailOptions.ResizeThumbnail;
        if ((options & ThumbnailOptions.InCacheOnly) == ThumbnailOptions.InCacheOnly) {
          flags |= Windows.Storage.FileProperties.ThumbnailOptions.ReturnOnlyIfCached;
        }

        Windows.Storage.IStorageItemProperties storageItem = null;
        try {
          if (item.IsFileSystem) {

            storageItem = item.IsFolder ? Windows.Storage.StorageFolder.GetFolderFromPathAsync(item.ParsingName).GetAwaiter().GetResult() : Windows.Storage.StorageFile.GetFileFromPathAsync(item.ParsingName).GetAwaiter().GetResult();
          }
        } catch {
          hBitmap = IntPtr.Zero;
          item.IsNeedRefreshing = true;
        }

        if (storageItem != null) {
          var thumb = storageItem?.GetThumbnailAsync(ThumbnailMode.SingleItem, (UInt32)width, flags).GetAwaiter().GetResult();
          var retry = 0;
          while (isForThumbnailSource && thumb != null && thumb.Type == ThumbnailType.Icon && retry < 10) {
            Thread.Sleep(100);
            thumb = storageItem?.GetThumbnailAsync(ThumbnailMode.SingleItem, (UInt32)width, flags).GetAwaiter().GetResult();
            retry++;
          }

          if (thumb != null && thumb.Type == ThumbnailType.Image) {
            using (var thumbNailStream = thumb.AsStreamForRead()) {
              var bmp = (Bitmap)Image.FromStream(thumbNailStream);
              hBitmap = bmp.GetHbitmap();
              bmp.Dispose();
              item.IsThumbnailLoaded = true;
              item.IsNeedRefreshing = false;
              hr = HResult.Ok;
            }
          }
        }
      }

      Marshal.ReleaseComObject(nativeShellItem);

      if (hr == HResult.Ok) {
        return hBitmap;
      }

      return IntPtr.Zero;
    }

    private static IntPtr GetHBitmap(IShellItem nativeShellItem, Int32 width, Int32 height, ThumbnailOptions options) {

      NativeSize nativeSize = default;
      nativeSize.Width = width;
      nativeSize.Height = height;

      IntPtr hBitmap;
      var hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, options, out hBitmap);

      Marshal.ReleaseComObject(nativeShellItem);

      if (hr == HResult.Ok) {
        return hBitmap;
      }

      return IntPtr.Zero;
    }
  }
}