using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {
	/// <summary>A Group of items in <see cref="ShellView"/></summary>
	public class ListViewGroupEx {

		/// <summary>The ShellItems in the group</summary>
		public ShellItem[] Items { get; set; }

		/// <summary>The Header/Text of the Group</summary>
		public String Header { get; set; }

		/// <summary>The Index of the Group</summary>
		public int Index { get; set; }
	}
}
