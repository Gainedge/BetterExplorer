using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {
	public static class Gdi32 {
		private const int SRCCOPY = 0xCC0020;

		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		public static extern int BitBlt(
			IntPtr hdcDest,     // handle to destination DC (device context)
			int nXDest,         // x-coord of destination upper-left corner
			int nYDest,         // y-coord of destination upper-left corner
			int nWidth,         // width of destination rectangle
			int nHeight,        // height of destination rectangle
			IntPtr hdcSrc,      // handle to source DC
			int nXSrc,          // x-coordinate of source upper-left corner
			int nYSrc,          // y-coordinate of source upper-left corner
			int dwRop  // raster operation code
			);

		[DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend")]
		public static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
			 int nWidthDest, int nHeightDest,
			 IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
			 BLENDFUNCTION blendFunction);
		[StructLayout(LayoutKind.Sequential)]
		public struct BLENDFUNCTION {
			byte BlendOp;
			byte BlendFlags;
			byte SourceConstantAlpha;
			byte AlphaFormat;

			public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format) {
				BlendOp = op;
				BlendFlags = flags;
				SourceConstantAlpha = alpha;
				AlphaFormat = format;
			}
		}

		public const byte AC_SRC_OVER = 0x00;
		public const byte AC_SRC_ALPHA = 0x01;

		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		public static extern void DeleteObject(IntPtr obj);
		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
		public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);
		[DllImport("gdi32.dll")]
		public static extern bool GetBitmapDimensionEx(IntPtr hBitmap, out BExplorer.Shell.Interop.Size lpDimension);

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct BITMAP {
			public Int32 bmType;
			public Int32 bmWidth;
			public Int32 bmHeight;
			public Int32 bmWidthBytes;
			public Int16 bmPlanes;
			public Int16 bmBitsPixel;
			public IntPtr bmBits;
		}
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct BITMAPINFOHEADER {
			public int biSize;
			public int biWidth;
			public int biHeight;
			public Int16 biPlanes;
			public Int16 biBitCount;
			public int biCompression;
			public int biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public int biClrUsed;
			public int bitClrImportant;
		}
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct DIBSECTION {
			public BITMAP dsBm;
			public BITMAPINFOHEADER dsBmih;
			public int dsBitField1;
			public int dsBitField2;
			public int dsBitField3;
			public IntPtr dshSection;
			public int dsOffset;
		}
		[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
		public static extern int GetObjectBitmap(IntPtr hObject, int nCount, ref
			 BITMAP lpObject);
		[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
		public static extern int GetObjectDIBSection(IntPtr hObject, int nCount,
		 IntPtr lpObject);

		[DllImport("gdi32.dll")]
		public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, IntPtr lpvObject);
		public static BITMAP GetHBitmapInfo(IntPtr hBitmap) {
			BITMAP bmData = new BITMAP();
			//GetObject(hBitmap, Marshal.SizeOf(typeof(BITMAP)), bmData);
			return bmData;
		}
		public static void BitBltDraw(IntPtr destDC, IntPtr hBitmap, int x, int y, int iconSize, Boolean isHidden = false) {
			IntPtr destCDC = CreateCompatibleDC(destDC);
			IntPtr oldSource = SelectObject(destCDC, hBitmap);
			AlphaBlend(destDC, x, y, iconSize, iconSize, destCDC, 0, 0, iconSize, iconSize, new BLENDFUNCTION(AC_SRC_OVER, 0, (byte)(isHidden ? 0x7f : 0xff), AC_SRC_ALPHA));
			SelectObject(destCDC, oldSource);
			DeleteObject(destCDC);
			DeleteObject(oldSource);
			DeleteObject(hBitmap);
		}
		public static void BitBltDrawu(IntPtr destDC, IntPtr hBitmap, int x, int y, int iconSize, Boolean useBitBlt = false) {
			BITMAP bmp = new BITMAP();
			GCHandle hndl = GCHandle.Alloc(bmp, GCHandleType.Pinned);

			IntPtr ptrToBitmap = hndl.AddrOfPinnedObject();
			//var fg =  GetObject(hBitmap, Marshal.SizeOf(typeof(BITMAP)), ptrToBitmap);
			//bmp = (BITMAP)Marshal.PtrToStructure(ptrToBitmap, typeof(BITMAP));
			DIBSECTION dibsection = new DIBSECTION();
			GetObjectDIBSection(hBitmap, Marshal.SizeOf(typeof(DIBSECTION)), ptrToBitmap);
			dibsection = (DIBSECTION)Marshal.PtrToStructure(ptrToBitmap, typeof(DIBSECTION));
			Interop.Size sz = new Interop.Size();
			//GetBitmapDimensionEx(hBitmap, out )
			hndl.Free();
			IntPtr destCDC = CreateCompatibleDC(destDC);
			//BITMAPINFO bmpInfo = new BITMAPINFO();
			//var header = new BITMAPINFOHEADER();
			//header.biBitCount = 32;
			//header.biPlanes = 1;
			//header.biCompression = BitmapCompressionMode.BI_RGB;
			//header.biSize = (uint)Marshal.SizeOf(header);
			//header.biWidth = 48;
			//header.biHeight = 48;
			//header.biSizeImage = (uint)48 * (uint)48 * 4;
			//bmpInfo.bmiHeader = header;
			//IntPtr bits;
			//var newHBitmap = CreateDIBSection(destCDC, ref bmpInfo, 0, out bits, IntPtr.Zero, 0);
			IntPtr oldSource = SelectObject(destCDC, hBitmap);
			AlphaBlend(destDC, x, y, iconSize, iconSize, destCDC, 0, 0, iconSize, iconSize, new BLENDFUNCTION(AC_SRC_OVER, 0, 0xff, AC_SRC_ALPHA));
			SelectObject(destCDC, oldSource);
			DeleteObject(destCDC);
			DeleteObject(oldSource);
			DeleteObject(hBitmap);
		}
	}
}
