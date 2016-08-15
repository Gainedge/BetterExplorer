using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BExplorer.Shell.Interop {
	public static class Gdi32 {

		#region Constants
		public const int SRCCOPY = 0xCC0020;
		public const byte AC_SRC_OVER = 0x00;
		public const byte AC_SRC_ALPHA = 0x01; 
		#endregion

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


		[StructLayout(LayoutKind.Sequential)]
		public struct RGBQUAD {
			public byte rgbBlue;
			public byte rgbGreen;
			public byte rgbRed;
			public byte rgbReserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAP {
			public Int32 bmType;
			public Int32 bmWidth;
			public Int32 bmHeight;
			public Int32 bmWidthBytes;
			public Int16 bmPlanes;
			public Int16 bmBitsPixel;
			public IntPtr bmBits;
		}

		[StructLayout(LayoutKind.Sequential)]
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
		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE {
			public int cx;
			public int cy;

			public SIZE(int cx, int cy) {
				this.cx = cx;
				this.cy = cy;
			}
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct DIBSECTION {
			public BITMAP dsBm;
			public BITMAPINFOHEADER dsBmih;
			public int dsBitField1;
			public int dsBitField2;
			public int dsBitField3;
			public IntPtr dshSection;
			public int dsOffset;
		}

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

		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);

		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		public static extern void DeleteObject(IntPtr obj);

		[DllImport("gdi32.dll", EntryPoint = "GetObject")]
		public static extern int GetObjectDIBSection(IntPtr hObject, int nCount, ref DIBSECTION lpObject);

    [DllImport("gdi32.dll", EntryPoint = "GetObject")]
    public static extern int GetObjectBitmap(IntPtr hObject, int nCount, [Out] IntPtr lpObject);

		public static void GetBitmapDimentions(IntPtr ipd, out int width, out int height) {
			// get the info about the HBITMAP inside the IPictureDisp
			DIBSECTION dibsection = new DIBSECTION();
			var res = GetObjectDIBSection(ipd, Marshal.SizeOf(dibsection), ref dibsection);
			width = dibsection.dsBm.bmWidth;
			height = dibsection.dsBm.bmHeight;
		}

		public static void ConvertPixelByPixel(IntPtr ipd, out int width, out int height) {
			// get the info about the HBITMAP inside the IPictureDisp
			DIBSECTION dibsection = new DIBSECTION();
			var res = GetObjectDIBSection(ipd, Marshal.SizeOf(dibsection), ref dibsection);
			width = dibsection.dsBm.bmWidth;
			height = dibsection.dsBm.bmHeight;
			unsafe {
				//Check is that 32bit bitmap
				if (dibsection.dsBmih.biBitCount == 32)
				{
					// get a pointer to the raw bits
					RGBQUAD* pBits = (RGBQUAD*)(void*)dibsection.dsBm.bmBits;
          
          // copy each pixel manually and premultiply the color values
          for (int x = 0; x < dibsection.dsBmih.biWidth; x++)
						for (int y = 0; y < dibsection.dsBmih.biHeight; y++) {
							int offset = y * dibsection.dsBmih.biWidth + x;
              if (pBits[offset].rgbReserved > 0 && (pBits[offset].rgbBlue > pBits[offset].rgbReserved || pBits[offset].rgbGreen > pBits[offset].rgbReserved || pBits[offset].rgbRed > pBits[offset].rgbReserved)) {
                pBits[offset].rgbBlue = (byte)((((int)pBits[offset].rgbBlue * (int)pBits[offset].rgbReserved + 1) * 257) >> 16);
                pBits[offset].rgbGreen = (byte)((((int)pBits[offset].rgbGreen * (int)pBits[offset].rgbReserved + 1) * 257) >> 16);
                pBits[offset].rgbRed = (byte)((((int)pBits[offset].rgbRed * (int)pBits[offset].rgbReserved + 1) * 257) >> 16);
              }
						}
				}
			}
		}

		[DllImport("user32.dll", EntryPoint = "GetDC", CharSet = CharSet.Auto)]
		public static extern IntPtr GetDeviceContext(IntPtr hWnd);
		[DllImport("gdi32", EntryPoint = "CreateCompatibleBitmap")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

		public static void NativeDraw(IntPtr destDC, IntPtr hBitmap, int x, int y, int iconSize, Boolean isHidden = false) {
			IntPtr destCDC = CreateCompatibleDC(destDC);
			IntPtr oldSource = SelectObject(destCDC, hBitmap);
			AlphaBlend(destDC, x, y, iconSize, iconSize, destCDC, 0, 0, iconSize, iconSize, new BLENDFUNCTION(AC_SRC_OVER, 0, (byte)(isHidden ? 0x7f : 0xff), AC_SRC_ALPHA));
			SelectObject(destCDC, oldSource);
			DeleteObject(destCDC);
			DeleteObject(oldSource);
			DeleteObject(hBitmap);
		}
		public static void NativeDraw(IntPtr destDC, IntPtr hBitmap, int x, int y, int iconSizeWidth, int iconSizeHeight, Boolean isHidden = false) {
			IntPtr destCDC = CreateCompatibleDC(destDC);
			IntPtr oldSource = SelectObject(destCDC, hBitmap);
			AlphaBlend(destDC, x, y, iconSizeWidth, iconSizeHeight, destCDC, 0, 0, iconSizeWidth, iconSizeHeight, new BLENDFUNCTION(AC_SRC_OVER, 0, (byte)(isHidden ? 0x7f : 0xff), AC_SRC_ALPHA));
			SelectObject(destCDC, oldSource);
			DeleteObject(destCDC);
			DeleteObject(oldSource);
			DeleteObject(hBitmap);
		}
	}
}
