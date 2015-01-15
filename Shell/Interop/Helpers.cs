using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {
	public static class Helpers {

		/*
		/// <summary>
		/// Change the opacity of an image
		/// </summary>
		/// <param name="originalImage">The original image</param>
		/// <param name="opacity">Opacity, where 1.0 is no opacity, 0.0 is full transparency</param>
		/// <returns>The changed image</returns>
		public static Bitmap ChangeImageOpacity(Bitmap originalImage, double opacity) {
			if ((originalImage.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed) {
				// Cannot modify an image with indexed colors
				return originalImage;
			}

			Bitmap bmp = (Bitmap)originalImage.Clone();

			// Specify a pixel format.
			PixelFormat pxf = PixelFormat.Format32bppArgb;

			// Lock the bitmap's bits.
			Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
			BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

			// Get the address of the first line.
			IntPtr ptr = bmpData.Scan0;

			// Declare an array to hold the bytes of the bitmap.
			// This code is specific to a bitmap with 32 bits per pixels 
			// (32 bits = 4 bytes, 3 for RGB and 1 byte for alpha).
			int numBytes = bmp.Width * bmp.Height * 4;
			byte[] argbValues = new byte[numBytes];

			// Copy the ARGB values into the array.
			System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);

			// Manipulate the bitmap, such as changing the
			// RGB values for all pixels in the the bitmap.
			for (int counter = 0; counter < argbValues.Length; counter += 4) {
				// argbValues is in format BGRA (Blue, Green, Red, Alpha)

				// If 100% transparent, skip pixel
				if (argbValues[counter + 4 - 1] == 0)
					continue;

				int pos = 0;
				pos++; // B value
				pos++; // G value
				pos++; // R value

				argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
			}

			// Copy the ARGB values back to the bitmap
			System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);

			// Unlock the bits.
			bmp.UnlockBits(bmpData);

			return bmp;
		}
		*/

		/*
		public static Bitmap ChangeOpacity(Image img, float opacityvalue) {
			Bitmap bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
			using (Graphics graphics = Graphics.FromImage(bmp))
			{
				ColorMatrix colormatrix = new ColorMatrix();
				colormatrix.Matrix33 = opacityvalue;
				ImageAttributes imgAttribute = new ImageAttributes();
				imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
			}
			return bmp;
		}
		*/

		internal static string GetParsingName(IShellItem shellItem) {
			if (shellItem == null) { return null; }

			string path = null;

			IntPtr pszPath = shellItem.GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING);

			if (pszPath != IntPtr.Zero) {
				path = Marshal.PtrToStringAuto(pszPath);
				Marshal.FreeCoTaskMem(pszPath);
				pszPath = IntPtr.Zero;
			}

			return path;

		}

		/*
		public static void SetListViewBackgroundImage(IntPtr lvHandle, Bitmap bitmap) {

			var lvBkImage = new LVBKIMAGE();

			lvBkImage.ulFlags = LVBKIF.STYLE_WATERMARK | LVBKIF.FLAG_ALPHABLEND;

			lvBkImage.hbm = bitmap.GetHbitmap();

			lvBkImage.cchImageMax = 0;

			lvBkImage.xOffsetPercent = 100;

			lvBkImage.yOffsetPercent = 100;



			IntPtr lbkImageptr = Marshal.AllocHGlobal(Marshal.SizeOf(lvBkImage));
			Marshal.StructureToPtr(lvBkImage, lbkImageptr, false);
			User32.SendMessage(lvHandle, MSG.LVM_SETBKIMAGE, 0, lbkImageptr);
			//Gdi32.DeleteObject(lvBkImage.hbm);
			Marshal.FreeHGlobal(lbkImageptr);

		}
		*/

	}
}
