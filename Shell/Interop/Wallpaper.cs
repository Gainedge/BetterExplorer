using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BExplorer.Shell.Interop {
	public class Wallpaper {
		const int SPI_SETDESKWALLPAPER = 20;
		const int SPIF_UPDATEINIFILE = 0x01;
		const int SPIF_SENDWININICHANGE = 0x02;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

		public enum Style : int {
			Tiled,
			Centered,
			Stretched,
			Fit,
			Fill
		}

		public void Set(Uri uri, Style style) {
			Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

			System.Drawing.Image img = System.Drawing.Image.FromStream(s);
			string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
			img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			switch (style)
			{
				case Style.Tiled:
					key.SetValue(@"WallpaperStyle", 1.ToString());
					key.SetValue(@"TileWallpaper", 1.ToString());
					break;
				case Style.Centered:
					key.SetValue(@"WallpaperStyle", 1.ToString());
					key.SetValue(@"TileWallpaper", 0.ToString());
					break;
				case Style.Stretched:
					key.SetValue(@"WallpaperStyle", 2.ToString());
					key.SetValue(@"TileWallpaper", 0.ToString());
					break;
				case Style.Fit:
					key.SetValue(@"WallpaperStyle", 6.ToString());
					key.SetValue(@"TileWallpaper", 0.ToString());
					break;
				case Style.Fill:
					key.SetValue(@"WallpaperStyle", 10.ToString());
					key.SetValue(@"TileWallpaper", 0.ToString());
					break;
			}			

			key.Close();
			SystemParametersInfo(SPI_SETDESKWALLPAPER,
				0,
				tempPath,
				SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
		}
	}
}
