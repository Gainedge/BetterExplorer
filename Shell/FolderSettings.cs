using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BExplorer.Shell {
	public class FolderSettings {
		public ShellViewStyle View { get; set; }
		public Int32 SortColumn { get; set; }
		public SortOrder SortOrder { get; set; }
	}
}
