using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using TAFactory.IconPack;

namespace BExplorer.Shell {
	public class IconReader {
		public List<IconFile> ReadIcons(string pathLibary) {
			var iconFiles = new List<IconFile>();
			foreach (var icon in IconHelper.ExtractAllIcons(pathLibary)) {
				iconFiles.Add(
					new IconFile {
						Icon = icon,
						Index = iconFiles.Count
					}
				);
			}

			return iconFiles;
		}
		public List<IconFile> ReadIcons(string pathLibary, Size size) {
			var iconFiles = new List<IconFile>();
			foreach (var icon in IconHelper.ExtractAllIcons(pathLibary)) {
				iconFiles.Add(
					new IconFile {
						Icon = IconHelper.GetBestFitIcon(icon, size, false),
						Index = iconFiles.Count
					}
				);
			}

			return iconFiles;
		}

	}
}
