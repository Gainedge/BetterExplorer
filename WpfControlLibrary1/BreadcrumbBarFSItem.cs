using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace BetterExplorerControls
{
    public class BreadcrumbBarFSItem
    {
        public String DisplayName { get; set; }
        public String RealPath { get; set; }

        public BreadcrumbBarFSItem(ShellObject fsItem)
        {
            this.DisplayName = fsItem.ParsingName;
            if (fsItem.ParsingName.StartsWith(":"))
            {
                Thread t = new Thread(() =>
                {
                    foreach (ShellObject item in KnownFolders.All)
                    {
                        this.DisplayName = this.DisplayName.Replace(item.ParsingName, item.GetDisplayName(DisplayNameType.Default)).Replace(".library-ms", "");
                    }
                });
                t.Start();
            }
            this.RealPath = fsItem.ParsingName;
        }
        public BreadcrumbBarFSItem(String display, String real)
        {
            this.DisplayName = display;
            this.RealPath = real;
        }
    }
}
