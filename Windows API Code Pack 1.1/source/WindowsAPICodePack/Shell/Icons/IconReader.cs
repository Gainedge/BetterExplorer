using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using TAFactory.IconPack;

namespace Microsoft.WindowsAPICodePack.Shell
{
    public class IconReader
    {
        public List<IconFile> ReadIcons(string pathLibary)
        {
            var icons = IconHelper.ExtractAllIcons(pathLibary);

            var iconFiles = new List<IconFile>();

            foreach(var icon in icons)
            {
                var iconFile = new IconFile 
                                { 
                                    Icon = icon,
                                    Index = iconFiles.Count
                                };
                iconFiles.Add(iconFile);
            }

            return iconFiles;
        }
        public List<IconFile> ReadIcons(string pathLibary, Size size)
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
