using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAFactory.IconPack;

namespace BExplorer.Shell
{
    public static class Icons
    {
        public class IconFile
        {
            public Icon Icon { get; set; }
            public int Index { get; set; }
        }

        public static List<IconFile> ReadIcons(string pathLibary, Size size)
        {
            var icons = IconHelper.ExtractAllIcons(pathLibary);
            var iconFiles = new List<IconFile>();

            foreach (var icon in icons)
            {
                var iconFile = new IconFile
                {
                    Icon = IconHelper.GetBestFitIcon(icon, size, false),
                    Index = iconFiles.Count
                };
                iconFiles.Add(iconFile);
            }

            return iconFiles;
        }
    }
}
