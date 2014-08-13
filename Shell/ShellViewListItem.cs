using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {
	public class ShellViewListItem {
		public String ParsingName { get; set; }
		public String DisplayName { get; set; }
		public DateTime ModifiedDate { get; set ;}
		public String Type { get; set; }
		public long Size { get; set; }
		public ShellViewListItem Parent { get; set; }

		public Dictionary<PROPERTYKEY, Object> SubItems { get; set; }


		public ShellViewListItem() {
			this.SubItems = new Dictionary<PROPERTYKEY, object>();
		}
		public ShellViewListItem(ShellItem item) {
			this.SubItems = new Dictionary<PROPERTYKEY, object>();
			if (item.Parent != null) this.Parent = new ShellViewListItem(item.Parent); else this.Parent = null;
			this.ParsingName = item.ParsingName;
			this.DisplayName = item.DisplayName;
		}

		public ShellItem ToShellItem() {
			return ShellItem.ToShellParsingName(this.ParsingName);
		}
	}
}
