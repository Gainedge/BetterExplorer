using System;
using System.Windows.Forms;
using System.Xml.Linq;
using BExplorer.Shell;

namespace ShellControls.ShellListView {
	/// <summary>Container for folder settings stored in the database</summary>
	public class FolderSettings {
		/// <summary>Specifies how list items where last displayed in a <see cref="ShellView" /> control.</summary>
		public ShellViewStyle View { get; set; }
		public int IconSize { get; set; }
		/// <summary>The Column to short</summary>
		public String SortColumn { get; set; }
		/// <summary>The sort direction</summary>
		public SortOrder SortOrder { get; set; }
		public String GroupCollumn { get; set; }
		public SortOrder GroupOrder { get; set; }
		/// <summary>
		/// The columns that should be displayed into the view
		/// </summary>
		public XElement Columns { get; set; }
	}
}
