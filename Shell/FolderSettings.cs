using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BExplorer.Shell {
	//TODO: Consider moving Database Code/Logic here

	/// <summary>Container for folder settings stored in the database</summary>
	public class FolderSettings {
		/// <summary>Specifies how list items where last displayed in a <see cref="ShellView" /> control.</summary>
		public ShellViewStyle View { get; set; }
		/// <summary>The Column to short</summary>
		public Int32 SortColumn { get; set; }
		/// <summary>The sort direction</summary>
		public SortOrder SortOrder { get; set; }
		/// <summary>
		/// The columns that should be displayed into the view
		/// </summary>
		public XElement Columns { get; set; }
	}
}
