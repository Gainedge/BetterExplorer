using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;
using WPFUI.Win32;

namespace BetterExplorer {
	public partial class MoreColumns : Form {
		public MoreColumns() {
			InitializeComponent();
			UxTheme.SetWindowTheme(lvColumns.Handle, "explorer", 0);
		}

		ShellView BrowserControl;
		public bool IsBeforeShow = true;
		protected override void WndProc(ref Message m) {
			if (m.Msg == 0x84 /* WM_NCHITTEST */) {
				m.Result = (IntPtr)1;
				return;
			}
			base.WndProc(ref m);
		}
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				unchecked {
					cp.Style |= (int)0x80000000;    // WS_POPUP
					cp.Style |= 0x40000;            // WS_THICKFRAME
				}
				return cp;
			}
		}

		public void PopulateAvailableColumns(List<Collumns> AvailableCols, ShellView ShellView, System.Windows.Point Location) {
			BrowserControl = ShellView;
			for (int i = 1; i < AvailableCols.Count; i++) {
				if (!String.IsNullOrEmpty(AvailableCols[i].Name)) {
					ListViewItem lvi = new ListViewItem(AvailableCols[i].Name);
					lvi.Tag = AvailableCols[i];
					if (AvailableCols[i].IsColumnHandler)
						lvi.ForeColor = Color.Red;
					foreach (Collumns collumn in ShellView.Collumns) {
						if (collumn.pkey.fmtid == AvailableCols[i].pkey.fmtid && collumn.pkey.pid == AvailableCols[i].pkey.pid) {
							lvi.Checked = true;
						}
					}

					lvColumns.Items.Add(lvi);
				}
			}
			Opacity = 0;
			if (lvColumns.Items.Count > 0)
				Show(ShellView);
			this.Location = new Point((int)Location.X, (int)Location.Y);
			//this.lvColumns.Sort(); //'this didn't do anything... lol.
			this.lvColumns.Sorting = SortOrder.Ascending;
			Opacity = 255;
		}

		private void MoreColumns_Deactivate(object sender, EventArgs e) {
			Close();
		}

		private void lvColumns_ItemChecked(object sender, ItemCheckedEventArgs e) {
			if (!IsBeforeShow) {
				Collumns col = ((Collumns)e.Item.Tag);
				BrowserControl.SetColInView(col, !e.Item.Checked);
			}
		}

		private void MoreColumns_Activated(object sender, EventArgs e) {
			IsBeforeShow = false;
		}
	}
}
