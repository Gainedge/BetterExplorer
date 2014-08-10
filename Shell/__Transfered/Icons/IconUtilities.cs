using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;


namespace Microsoft.WindowsAPICodePack.Shell {
	/// <summary>
	/// Converts Icon to ImageSource
	/// </summary>
	internal static class IconUtilities {
		/*
		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern bool DeleteObject(IntPtr hObject);
		*/

		/*
		public static ImageSource ToImageSource(this Icon icon) {
			Bitmap bitmap = icon.ToBitmap();
			IntPtr hBitmap = bitmap.GetHbitmap();

			ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
			  hBitmap,
			  IntPtr.Zero,
			  Int32Rect.Empty,
			  BitmapSizeOptions.FromEmptyOptions());

			if (!DeleteObject(hBitmap)) {
				throw new Win32Exception();
			}

			return wpfBitmap;
		}
		*/ 
	}
}
