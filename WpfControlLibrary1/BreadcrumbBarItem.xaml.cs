using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using BExplorer.Shell;
using BExplorer.Shell.Interop;

namespace BetterExplorerControls {

	/// <summary>
	/// Interaction logic for BreadcrumbBarItem.xaml
	/// </summary>
	public partial class BreadcrumbBarItem : UserControl {

		#region Properties

		public delegate void PathEventHandler(object sender, PathEventArgs e);

		//[Obsolete("Never used")]
		//private string path = "";

		private Fluent.ContextMenu DropDownMenu = new Fluent.ContextMenu();

		//private bool children = true;
		private ShellItem so;

		//public bool HasDropDownMenu { get { return children; } }
		public bool HasDropDownMenu { get; private set; }

		public ShellItem ShellItem {
			get { return so; }
			set { LoadDirectory(value); }
		}

		#endregion Properties

		#region Events

		public event PathEventHandler NavigateRequested;

		protected virtual void OnNavigateRequested(PathEventArgs e) {
			if (NavigateRequested != null)
				NavigateRequested(this, e);
		}

		public event PathEventHandler ContextMenuRequested;

		protected virtual void OnContextMenuRequested(PathEventArgs e) {
			if (ContextMenuRequested != null)
				ContextMenuRequested(this, e);
		}

		public event MouseButtonEventHandler ButtonClick;

		protected virtual void OnButtonClick(MouseButtonEventArgs e) {
			if (ButtonClick != null)
				ButtonClick(this, e);
		}

		public event EventHandler MenuOpened;

		protected virtual void OnMenuOpened(EventArgs e) {
			if (MenuOpened != null)
				MenuOpened(this, e);
		}

		public event EventHandler MenuClosed;

		protected virtual void OnMenuClosed(EventArgs e) {
			if (MenuClosed != null)
				MenuClosed(this, e);
		}

		#endregion Events

		#region Private Events

		private void UserControl_MouseEnter(object sender, MouseEventArgs e) {
			ShowSelectedColors();
		}

		private void UserControl_MouseLeave(object sender, MouseEventArgs e) {
			HideSelectedColors();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			e.Handled = true;
			ShellItem con = this.ShellItem;
			List<ShellItem> joe = new List<ShellItem>();
			foreach (ShellItem item in con) {
				if (item.IsFolder) {
					if (!item.ParsingName.EndsWith(".zip")) {
						joe.Add(item);
					}
				}
			}

			joe.Sort(delegate(ShellItem j1, ShellItem j2) { return j1.GetDisplayName(SIGDN.NORMALDISPLAY).CompareTo(j2.GetDisplayName(SIGDN.NORMALDISPLAY)); });
			DropDownMenu.Items.Clear();
			foreach (ShellItem thing in joe) {
				Fluent.MenuItem pan = new Fluent.MenuItem();
				thing.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				thing.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
				pan.Icon = thing.Thumbnail.BitmapSource;
				var header = thing.GetDisplayName(SIGDN.NORMALDISPLAY);
				var posunderscore = header.IndexOf("_");
				if (posunderscore != -1)
					header = header.Insert(posunderscore, "_");
				pan.Header = header;
				pan.Height = 23;
				pan.Tag = thing;

				pan.Click += new RoutedEventHandler(MenuItemClicked);
				this.DropDownMenu.Items.Add(pan);
			}
			if (DropDownMenu.Items.Count > 0) {
				DropDownMenu.Placement = PlacementMode.Bottom;
				DropDownMenu.PlacementTarget = Base;
				DropDownMenu.IsOpen = true;
			}
		}

		private void Grid_MouseUp(object sender, MouseButtonEventArgs e) {
			//e.Handled = true;
			OnButtonClick(e);
			if (e.ChangedButton == MouseButton.Left) {
				OnNavigateRequested(new PathEventArgs(this.ShellItem));
			}
			else if (e.ChangedButton == MouseButton.Right) {
				OnContextMenuRequested(new PathEventArgs(this.ShellItem));
			}
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			//e.Handled = true;
		}

		private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			//e.Handled = true;
			//if (e.ClickCount == 2)
			//{
			//    if (MouseDoubleClick != null)
			//    {
			//        MouseDoubleClick(this, EventArgs.Empty);
			//    }

			//}
		}

		private void btnDropDown_MouseDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
		}

		private void btnDropDown_MouseUp(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
		}

		private void DropDownMenu_Closed(object sender, RoutedEventArgs e) {
			OnMenuClosed(EventArgs.Empty);
			expandArrow.Visibility = Visibility.Visible;
			ddArrow.Visibility = Visibility.Hidden;
			HideSelectedColors();
		}

		private void DropDownMenu_Opened(object sender, RoutedEventArgs e) {
			OnMenuOpened(EventArgs.Empty);
			expandArrow.Visibility = Visibility.Hidden;
			ddArrow.Visibility = Visibility.Visible;
			ShowSelectedColors();
		}

		private void MenuItemClicked(object sender, RoutedEventArgs e) {
			Fluent.MenuItem pan = (Fluent.MenuItem)sender;
			OnNavigateRequested(new PathEventArgs(pan.Tag as ShellItem));
		}

		#endregion Private Events

		#region Helpers

		public void LoadDirectory(ShellItem obj) {
			obj.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			obj.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
			this.PathImage.Source = obj.Thumbnail.BitmapSource;
			this.pathName.Text = obj.GetDisplayName(SIGDN.NORMALDISPLAY);
			this.so = obj;

			Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)(() => {
				if (obj.ParsingName == KnownFolders.Network.ParsingName || obj.ParsingName.StartsWith(@"\\")) {
					SetChildren(true);
					grid1.Visibility = Visibility.Visible;
					MenuBorder.Visibility = Visibility.Visible;
				}
				else {
					try {
						//ShellItem con = obj;
						List<ShellItem> joe = new List<ShellItem>();
						//foreach (ShellItem item in con) {
						foreach (ShellItem item in obj) {
							if (item.IsFolder) {
								if (item.ParsingName.ToLower().EndsWith(".zip") == false && item.ParsingName.ToLower().EndsWith(".cab") == false) {
									joe.Add(item);
								}
							}
						}
						SetChildren(joe.Count > 0);
					}
					catch {
						SetChildren(false);
					}
				}
			}));
		}

		private void SetChildren(bool isON) {
			expandArrow.Visibility = isON ? Visibility.Visible : Visibility.Collapsed;
			grid1.Visibility = isON ? Visibility.Visible : Visibility.Collapsed;
			//children = isON;
			HasDropDownMenu = isON;
		}

		private string IncludeTrailingBackslash(string Path) {
			string CharToInsert = "";
			if (Path[Path.Length - 1] != Char.Parse(@"\")) {
				CharToInsert = @"\";
			}
			return Path + CharToInsert;
		}

		private void ShowSelectedColors() {
			this.SelectionBackground.Visibility = Visibility.Visible;
			this.PathBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 127, 177));

			if (HasDropDownMenu) {
				this.MenuBackground.Visibility = Visibility.Visible;
				this.MenuBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 127, 177));
			}
		}

		private void HideSelectedColors() {
			this.SelectionBackground.Visibility = Visibility.Hidden;
			this.PathBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(1, 60, 127, 177));

			if (HasDropDownMenu) {
				this.MenuBackground.Visibility = Visibility.Hidden;
				this.MenuBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(1, 60, 127, 177));
			}
		}

		#endregion Helpers

		public BreadcrumbBarItem() {
			InitializeComponent();
			DropDownMenu.Opened += new RoutedEventHandler(DropDownMenu_Opened);
			DropDownMenu.Closed += new RoutedEventHandler(DropDownMenu_Closed);
		}
	}
}

public class PathEventArgs {
	public string Path { get; private set; }
	public ShellItem ShellItem { get; private set; }

	public PathEventArgs(ShellItem obj = null) {
		ShellItem = obj;
		Path = obj.ParsingName;
	}
}