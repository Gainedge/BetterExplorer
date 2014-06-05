using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for SavedTabsListGalleryItem.xaml
	/// </summary>
	public partial class SavedTabsListGalleryItem : UserControl {
		private SavedTabsList lst;
		public string Directory { get; set; }


		/*
		public SavedTabsListGalleryItem() {
			Directory = "";
			InitializeComponent();
		}
		*/

		public SavedTabsListGalleryItem(string loc) {
			InitializeComponent();
			Directory = "";
			Location = loc;
		}

		public SavedTabsListGalleryItem(string loc, bool selected) {
			InitializeComponent();
			Location = loc;
			if (selected) {
				SetSelected();
			}
			else {
				SetDeselected();
			}
		}

		public string Location {
			get { return tabTitle.Text; }
			set { tabTitle.Text = value; }
		}

		


		public void SetUpTooltip(string tabs) {
			lst = SavedTabsList.LoadTabList(String.Format("{0}{1}.txt", Directory, Location));
			StringBuilder blah = new StringBuilder(lst.Count);
			foreach (string item in lst) {
				blah.AppendLine(item);
			}
			string de = String.Format("{0}: {1}\n\r", tabs, lst.Count.ToString()) + blah;

			this.ToolTip = de.Remove(de.Length - 2);
		}

		public delegate void PathStringEventHandler(object sender, Tuple<string> e);

		// An event that clients can use to be notified whenever the
		// elements of the list change:
		public event PathStringEventHandler Click;
		//public event EventHandler MouseDoubleClick;

		// Invoke the Changed event; called whenever list changes:
		protected virtual void OnClick(Tuple<string> e) {
			if (Click != null)
				Click(this, e);
		}

		private void UserControl_MouseUp(object sender, MouseButtonEventArgs e) {
			OnClick(new Tuple<string>(tabTitle.Text));
		}

		public void PerformClickEvent() {
			OnClick(new Tuple<string>(tabTitle.Text));
		}

		public void SetSelected() {
			this.Background = new SolidColorBrush(Color.FromRgb(0, 50, 255));
			this.tabTitle.Foreground = new SolidColorBrush(Colors.White);
		}

		public void SetDeselected() {
			this.Background = new SolidColorBrush(Color.FromArgb(1, 255, 255, 255));
			this.tabTitle.Foreground = new SolidColorBrush(Colors.Black);
		}

	}
}

/*
public class PathStringEventArgs
{
	string _obj;

	public PathStringEventArgs(string loc)
	{
		_obj = loc;
	}

	public string PathString
	{
		get
		{
			return _obj;
		}
	}
  }
*/
