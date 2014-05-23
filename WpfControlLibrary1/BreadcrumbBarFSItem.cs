
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace BetterExplorerControls {
	public class BreadcrumbBarFSItem {
		public String DisplayName { get; set; }
		public String RealPath { get; set; }

		public BreadcrumbBarFSItem(String display, String real) {
			this.DisplayName = display;
			this.RealPath = real;
		}

		public BreadcrumbBarFSItem(ShellItem fsItem) {
			this.DisplayName = fsItem.ParsingName;
			if (fsItem.ParsingName.StartsWith(":")) {
				Thread t = new Thread(() => {
					foreach (ShellItem item in KnownFolders.All) {
						this.DisplayName = this.DisplayName.Replace(item.ParsingName, item.GetDisplayName(SIGDN.NORMALDISPLAY)).Replace(".library-ms", "");
					}
				});
				t.Start();
			}
			this.RealPath = fsItem.ParsingName;
		}
	}
}
