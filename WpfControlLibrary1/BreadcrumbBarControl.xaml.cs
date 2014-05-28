using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using BExplorer.Shell;
using BExplorer.Shell.Interop;

namespace BetterExplorerControls {

	/// <summary> Interaction logic for BreadcrumbBarControl.xaml </summary>
	public partial class BreadcrumbBarControl : UserControl { //TODO: See To Do List Document

		#region Being Removed

		[Obsolete("Being Removed", true)]
		public string LastPath = "";

		[Obsolete("Being Removed", true)]
		public bool RecordHistory { get; set; }  //<- writetohistory

		[Obsolete("Being Removed", true)]
		private bool Needfilter;

		[Obsolete("Being Removed", true)]
		private bool IsFiltered;

		[Obsolete("Not Used!!!", true)]
		private void g_Click(object sender, RoutedEventArgs e) {
			HistoryCombo.Text = (string)(sender as MenuItem).Header;
			PathEventArgs args = new PathEventArgs(new ShellItem(HistoryCombo.Text.Trim().StartsWith("%") ? Environment.ExpandEnvironmentVariables(HistoryCombo.Text) : HistoryCombo.Text.ToShellParsingName()));
			OnNavigateRequested(args);
			ExitEditMode();
		}

		[Obsolete("Not Used!!!", true)]
		private void HistoryCombo_DropDownOpened(object sender, EventArgs e) {
			/*
			//if (IsFiltered) {
			//	HistoryCombo.Items.Filter += a => true;
			//	IsFiltered = false;
			//}

			if (!IsInEditMode) EnterEditMode();

			Undertextbox.Focus();
			if (HistoryCombo.Items.Count == 0) {
				HistoryCombo.IsDropDownOpen = false;
			}
			*/
		}

		/// <summary> Invoke the Changed event; called whenever list changes: </summary>
		[Obsolete("Was only used 1 time", true)]
		protected virtual void OnRefreshRequested() { if (RefreshRequested != null) RefreshRequested(this); }


		//private bool IsEcsPressed;
		[Obsolete("I think we could reduce the scope of this to inside methods only", true)]
		private bool IsLeavingControl;

		[Obsolete("Anything that produces a situation where we would need this is now considered an error and THAT should be fixed!", true)]
		private bool IsInEditMode { get; set; }

		[Obsolete("Exact same effect without it", false)]
		private void HistoryCombo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			//e.Handled = true;
			//if (!IsInEditMode && !IsLeavingControl) {
			//	EnterEditMode();
			//	//Undertextbox.Focus();
			//}
		}

		[Obsolete("Exact same effect without it", false)]
		private void HistoryCombo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			//e.Handled = true;
			//if (HistoryCombo.IsDropDownOpen) {
			//	ExitEditMode();
			//}
		}

		#endregion Being Removed


		#region Properties

		private TextBox Undertextbox;
		private BreadcrumbBarItem furthestrightitem;
		private DragEventHandler de, dl, dm, dp;

		private ObservableCollection<BreadcrumbBarFSItem> hl { get; set; }

		public delegate void PathEventHandler(object sender, PathEventArgs e);

		/// <summary>
		/// An event that clients can use to be notified whenever the elements of the list change:
		/// </summary>
		public event PathEventHandler NavigateRequested;

		[Obsolete("Might replace with Action<Object>")]
		public delegate void RefreshHandler(object sender);

		/// <summary>
		/// An event that clients can use to be notified whenever the elements of the list change:
		/// </summary>
		public event RefreshHandler RefreshRequested;

		public ObservableCollection<BreadcrumbBarFSItem> HistoryItems {
			get {
				return new ObservableCollection<BreadcrumbBarFSItem>(hl.ToArray());
			}
			set {
				foreach (var item in value) {
					hl.Add(item);
				}
				Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)(() => {
					HistoryCombo.ItemsSource = hl;
				}));
			}
		}

		#endregion Properties

		#region Control Events

		private void BreadcrumbBarControl_Loaded(object sender, RoutedEventArgs e) {
			Undertextbox = (TextBox)HistoryCombo.Template.FindName("PART_EditableTextBox", HistoryCombo);
			Undertextbox.AddHandler(TextBox.TextInputEvent, new TextCompositionEventHandler(Undertextbox_TextInput), true);
			Undertextbox.GotKeyboardFocus += Undertextbox_GotKeyboardFocus;
		}

		private void Undertextbox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			Undertextbox.SelectAll();
		}

		private void Undertextbox_TextInput(object sender, TextCompositionEventArgs e) {
			if ((e.Text.All(Char.IsLetterOrDigit) || e.Text.All(Char.IsSymbol) || e.Text.All(Char.IsWhiteSpace)) && e.Text != "\r") {
				//if (!HistoryCombo.IsDropDownOpen) Needfilter = true;
				HistoryCombo.IsDropDownOpen = true;
				if (!String.IsNullOrEmpty(Undertextbox.Text) && !HistoryCombo.IsDropDownOpen) {
					HistoryCombo.Items.Filter += a => {
						if (((BreadcrumbBarFSItem)a).RealPath.ToLower().StartsWith(Undertextbox.Text.ToLower()) || ((BreadcrumbBarFSItem)a).DisplayName.ToLower().StartsWith(Undertextbox.Text.ToLower())) {
							return true;
						}
						return false;
					};
					//IsFiltered = true;
				}
			}
			Undertextbox.SelectionLength = 0;
			Undertextbox.SelectionStart = Undertextbox.Text.Length;
			Undertextbox.SelectedText = "";
		}

		private void duh_ContextMenuRequested(object sender, PathEventArgs e) {
			//ShellItem[] dirs = new ShellItem[1];
			//dirs[0] = e.ShellItem;
			//var dirs = new[] { e.ShellItem };

			Point relativePoint = this.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
			Point realCoordinates = Application.Current.MainWindow.PointToScreen(relativePoint);
			ShellContextMenu cm = new ShellContextMenu(new[] { e.ShellItem });
			cm.ShowContextMenu(new System.Drawing.Point((int)GetCursorPosition().X, (int)realCoordinates.Y + (int)this.Height));
		}

		[Obsolete("Try to inline this IF possible")]
		private void duh_NavigateRequested(object sender, PathEventArgs e) {
			OnNavigateRequested(e);
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
			if (furthestrightitem != null)
				furthestrightitem.BringIntoView();
		}

		private void btnRefreshExplorer_Click(object sender, RoutedEventArgs e) {
			if (RefreshRequested != null) RefreshRequested(this);
		}

		#endregion Control Events

		#region Random Private

		private void RequestNavigation(String Path) {
			//IsLeavingControl = true;
			if (!System.IO.Directory.Exists(Path)) {
				//ExitEditMode_IfNeeded() must be run to prevent endless MessageBox loop!
				//ExitEditMode_IfNeeded();
				ExitEditMode();
				MessageBox.Show("Better Explorer Cannot find '" + HistoryCombo.Text + "' Check the spelling and try again", "Better Explorer",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);

				return;
			}

			PathEventArgs ea = null;
			var path = String.Empty;
			BreadcrumbBarFSItem item = null;
			if (Path.Trim().StartsWith("%")) {
				path = Environment.ExpandEnvironmentVariables(Path);
				item = new BreadcrumbBarFSItem(Path, path);
			}
			else {
				path = Path.ToShellParsingName();
				item = new BreadcrumbBarFSItem(new ShellItem(path));
			}

			ea = new PathEventArgs(new ShellItem(path));

			OnNavigateRequested(ea);
			//if (RecordHistory) {
			if (hl.Select(s => s.RealPath).Contains(Path) == false) {
				hl.Add(item);
			}
			//}

			ExitEditMode();
		}

		private void GetBreadCrumbItems(List<ShellItem> items) {
			ShellItem lastmanstanding = items[0];
			items.Reverse();

			foreach (ShellItem thing in items) {
				BreadcrumbBarItem duh = new BreadcrumbBarItem();
				if (!thing.IsSearchFolder) {
					duh.LoadDirectory(thing);
				}
				else {
					thing.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					thing.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
					duh.pathName.Text = thing.GetDisplayName(SIGDN.NORMALDISPLAY);
					duh.PathImage.Source = thing.Thumbnail.BitmapSource;
					duh.MenuBorder.Visibility = System.Windows.Visibility.Collapsed;
					duh.grid1.Visibility = System.Windows.Visibility.Collapsed;
				}

				duh.NavigateRequested += new BreadcrumbBarItem.PathEventHandler(duh_NavigateRequested);
				duh.ContextMenuRequested += duh_ContextMenuRequested;

				this.elPanel.Children.Add(duh);

				if (thing == lastmanstanding) {
					furthestrightitem = duh;
					duh.BringIntoView();
				}
			}
		}

		private List<ShellItem> GetPaths(ShellItem currloc) {
			List<ShellItem> res = new List<ShellItem>();
			ShellItem subject = currloc;

			do {
				res.Add(subject);
				subject = subject.Parent;
			} while (subject != null);

			/*
			bool apf = false;

			while (!apf) {
				res.Add(subject);
				if (subject.Parent != null)
					subject = subject.Parent;
				else
					apf = true;
			}
			*/

			return res;
		}

		/// <summary> Invoke the Changed event; called whenever list changes: </summary>
		protected virtual void OnNavigateRequested(PathEventArgs e) {
			if (NavigateRequested != null) NavigateRequested(this, e);
		}

		#endregion Random Private

		#region Random Public

		public void ClearHistory() {
			hl.Clear();
		}

		public void SetDragHandlers(DragEventHandler dragenter, DragEventHandler dragleave, DragEventHandler dragover, DragEventHandler drop) {
			de = dragenter;
			dl = dragleave;
			dm = dragover;
			dp = drop;
		}

		public void LoadDirectory(ShellItem currloc, bool loadDragEvents = true) {
			this.elPanel.Children.Clear();
			GetBreadCrumbItems(GetPaths(currloc));
			if (loadDragEvents) {
				foreach (BreadcrumbBarItem item in this.elPanel.Children) {
					item.AllowDrop = true;
					item.DragEnter += de;
					item.DragLeave += dl;
					item.DragOver += dm;
					item.Drop += dp;
				}
			}
		}

		public void ExitEditMode() {
			//if (!this.IsLeavingControl) {
			//	this.ToString();
			//}

			//IsLeavingControl = true;
			//IsInEditMode = false;
			elPanel.Visibility = System.Windows.Visibility.Visible;

			if (Undertextbox != null)
				Undertextbox.Visibility = System.Windows.Visibility.Hidden;
		}

		public void EnterEditMode() {
			//if (this.IsInEditMode) {
			//	this.ToString();
			//}
			////else if (this.IsLeavingControl) {
			////	this.ToString();
			////}

			//IsLeavingControl = false;
			//IsInEditMode = true;
			elPanel.Visibility = System.Windows.Visibility.Collapsed;

			if (Undertextbox != null) {
				Undertextbox.Visibility = System.Windows.Visibility.Visible;
			}

			var obj = furthestrightitem.ShellItem;

			if (obj != null) {
				HistoryCombo.Text = obj.GetDisplayName(SIGDN.DESKTOPABSOLUTEEDITING);
				Undertextbox.Focus();
			}
		}

		#endregion Random Public

		#region Cursor Stuff

		/// <summary> Retrieves the cursor's position, in screen coordinates. </summary>
		/// <see> See MSDN documentation for further information. </see>
		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(out POINT lpPoint);

		/// <summary> Struct representing a point. </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			public int X;
			public int Y;

			public static implicit operator Point(POINT point) {
				return new Point(point.X, point.Y);
			}
		}

		public static Point GetCursorPosition() {
			POINT lpPoint;
			GetCursorPos(out lpPoint);
			return lpPoint;
		}

		#endregion Cursor Stuff

		public BreadcrumbBarControl() {
			InitializeComponent();
			this.hl = new ObservableCollection<BreadcrumbBarFSItem>();
			this.Loaded += BreadcrumbBarControl_Loaded;
		}

		private void HistoryCombo_GotFocus(object sender, RoutedEventArgs e) {
			e.Handled = true;
			EnterEditMode();
		}

		private void HistoryCombo_LostFocus(object sender, RoutedEventArgs e) {
			e.Handled = true;

			//This is calling ExitEditMode_IfNeeded() an extra time
			//ExitEditMode_IfNeeded();
			//if (IsInEditMode || IsLeavingControl)
			//if (IsInEditMode)
			ExitEditMode();
		}

		private void HistoryCombo_MouseUp(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			if (e.LeftButton == MouseButtonState.Released) {
				//IsLeavingControl = false;
				//if (!IsInEditMode)
				EnterEditMode();
			}
		}

		private void HistoryCombo_KeyUp(object sender, KeyEventArgs e) {
			//TODO: Test this out
			e.Handled = true;
			//IsLeavingControl = e.Key == Key.Enter || e.Key == Key.Escape;
			if (e.Key == Key.Enter)
				RequestNavigation(HistoryCombo.Text.ToShellParsingName());
		}

		private void HistoryCombo_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) ExitEditMode();
		}

		private void HistoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			RequestNavigation((e.AddedItems[0] as BreadcrumbBarFSItem).RealPath);
		}
	}
}