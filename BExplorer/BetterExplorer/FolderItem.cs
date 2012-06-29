using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using Microsoft.WindowsAPICodePack.Shell;

namespace BetterExplorer
{
    public class FolderItem
    {
        public string Folder { get; set; }
        public ImageSource Image { get; set; }
        public string DisplayName { get; set; }

        public FolderItem()
            : base()
        {
            
        }
    }
}
