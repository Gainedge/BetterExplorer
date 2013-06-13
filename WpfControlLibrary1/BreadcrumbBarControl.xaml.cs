using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Shell;
using System.Threading;
using System.IO;
using GongSolutions.Shell;
using WindowsHelper;
using System.Runtime.InteropServices;


namespace BetterExplorerControls
{
	/// <summary>
	/// Interaction logic for BreadcrumbBarControl.xaml
	/// </summary>
	public partial class BreadcrumbBarControl : UserControl
	{
    bool ShouldUpdateDropDown = false;
		public BreadcrumbBarControl()
		{
			InitializeComponent();
		}

		private List<string> hl = new List<string>();


		private void Grid_MouseEnter(object sender, MouseEventArgs e)
		{
			ddGrid.Background = new SolidColorBrush(Colors.PowderBlue);
			//ddBorder.BorderBrush = new SolidColorBrush(Colors.LightBlue);
		}

		private void Grid_MouseLeave(object sender, MouseEventArgs e)
		{
			ddGrid.Background = new SolidColorBrush(Colors.White);
			//ddBorder.BorderBrush = new SolidColorBrush(Colors.White);
		}


		private ContextMenu CreateHistoryMenu(bool isFiltered = false)
		{
			ContextMenu hm = new ContextMenu();
      List<string> items = null;
      if (isFiltered)
      {
        items = hl.Where(c => c.ToLowerInvariant().Contains(HistoryCombo.Text.ToLowerInvariant())).ToList();
      }
      else
      {
        items = hl;
      }
			foreach (string item in items)
			{
				MenuItem g = new MenuItem();
				g.Header = item;
				g.Width = grdMain.ActualWidth - 4;
				g.Click += new RoutedEventHandler(g_Click);
				hm.Items.Add(g);
			}

			return hm;
		}

		void g_Click(object sender, RoutedEventArgs e)
		{
			HistoryCombo.Text = (string)(sender as MenuItem).Header;
      PathEventArgs args = new PathEventArgs(ShellObject.FromParsingName(HistoryCombo.Text));
      OnNavigateRequested(args);
      ExitEditMode();
			//throw new NotImplementedException();
		}
		ContextMenu mnu;
		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ddGrid.Background = new SolidColorBrush(Colors.SkyBlue);
			//ddBorder.BorderBrush = new SolidColorBrush(Colors.DeepSkyBlue);
			EnterEditMode();
			mnu = CreateHistoryMenu();
			mnu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			mnu.PlacementTarget = this;
			mnu.IsOpen = true;
		}

		private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			ddGrid.Background = new SolidColorBrush(Colors.PowderBlue);
			//ddBorder.BorderBrush = new SolidColorBrush(Colors.LightBlue);
		}

		private BreadcrumbBarItem furthestrightitem;

		private bool writetohistory = true;

		public void ClearHistory()
		{
			hl.Clear();
		}

		public List<string> HistoryItems
		{
			get
			{
				List<string> hilist = new List<string>();

				foreach (string item in hl)
				{
					hilist.Add(item);
				}

				return hilist;
			}
			set
			{
				foreach (string item in value)
				{
					hl.Add(item);
				}
			}
		}

		private DragEventHandler de;
		private DragEventHandler dl;
		private DragEventHandler dm;
		private DragEventHandler dp;

		public delegate void PathEventHandler(object sender, PathEventArgs e);

		// An event that clients can use to be notified whenever the
		// elements of the list change:
		public event PathEventHandler NavigateRequested;

		// Invoke the Changed event; called whenever list changes:
		protected virtual void OnNavigateRequested(PathEventArgs e)
		{
			if (NavigateRequested != null)
				NavigateRequested(this, e);
		}

		public void SetDragHandlers(DragEventHandler dragenter, DragEventHandler dragleave, DragEventHandler dragover, DragEventHandler drop)
		{
			de = dragenter;
			dl = dragleave;
			dm = dragover;
			dp = drop;
		}

		public bool RecordHistory
		{
			get
			{
				return writetohistory;
			}
			set
			{
				writetohistory = value;
			}
		}

		private List<ShellObject> GetPaths(ShellObject currloc)
		{
			List<ShellObject> res = new List<ShellObject>();
			ShellObject subject = currloc;

			bool apf = false;

			while (apf == false)
			{
				res.Add(subject);
				if (subject.Parent != null)
				{
					subject = subject.Parent;
				}
				else
				{
					apf = true;
				}
			}

			return res;
		}

		public void LoadDirectory(ShellObject currloc, bool loadDragEvents = true)
		{
			this.elPanel.Children.Clear();
			GetBreadCrumbItems(GetPaths(currloc));
			if (loadDragEvents == true)
			{
				foreach (BreadcrumbBarItem item in this.elPanel.Children)
				{
					item.AllowDrop = true;
					item.DragEnter += de;
					item.DragLeave += dl;
					item.DragOver += dm;
					item.Drop += dp;
				}
			}
		}

		public ShellObject GetDirectoryAtPoint(Point pt)
		{
			try
			{
				return ((BreadcrumbBarItem)elPanel.InputHitTest(pt)).ShellObject;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void UpdateLastItem(int CurLocationCount)
		{
			BreadcrumbBarItem lastitem =
					elPanel.Children[elPanel.Children.Count - 1] as BreadcrumbBarItem;
			lastitem.SetChildren(CurLocationCount > 0);
		}

		private void GetBreadCrumbItems(List<ShellObject> items)
		{
			ShellObject lastmanstanding = items[0];
			items.Reverse();

			foreach (ShellObject thing in items)
			{
				bool isSearch = false;
				try
				{
					isSearch = thing.IsSearchFolder;
				}
				catch
				{
					isSearch = false;
				}
				BreadcrumbBarItem duh = new BreadcrumbBarItem();
				if (!isSearch)
				{
					duh.LoadDirectory(thing);
				}
				else
				{
					thing.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					thing.Thumbnail.CurrentSize = new Size(16, 16);
					duh.pathName.Text = thing.GetDisplayName(DisplayNameType.Default);
					duh.PathImage.Source = thing.Thumbnail.BitmapSource;
					duh.MenuBorder.Visibility = System.Windows.Visibility.Collapsed;
					duh.grid1.Visibility = System.Windows.Visibility.Collapsed;
				}

				duh.NavigateRequested += new BreadcrumbBarItem.PathEventHandler(duh_NavigateRequested);
        duh.ContextMenuRequested += duh_ContextMenuRequested;

				this.elPanel.Children.Add(duh);

				if (thing == lastmanstanding)
				{
					furthestrightitem = duh;
					duh.BringIntoView();
				}
			}
		}

    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
      public int X;
      public int Y;

      public static implicit operator Point(POINT point)
      {
        return new Point(point.X, point.Y);
      }
    }

    /// <summary>
    /// Retrieves the cursor's position, in screen coordinates.
    /// </summary>
    /// <see>See MSDN documentation for further information.</see>
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    public static Point GetCursorPosition()
    {
      POINT lpPoint;
      GetCursorPos(out lpPoint);
      //bool success = User32.GetCursorPos(out lpPoint);
      // if (!success)

      return lpPoint;
    }

    void duh_ContextMenuRequested(object sender, PathEventArgs e)
    {
      ShellContextMenu cm = new ShellContextMenu(e.ShellObject);
      ShellObject[] dirs = new ShellObject[1];
      dirs[0] = e.ShellObject;
      Point relativePoint = this.TransformToAncestor(Application.Current.MainWindow)
                          .Transform(new Point(0, 0));
      Point realCoordinates = Application.Current.MainWindow.PointToScreen(relativePoint);
      cm.ShowContextMenu(new System.Drawing.Point((int)GetCursorPosition().X, (int)realCoordinates.Y + (int)this.Height));
    }

		void duh_MouseDoubleClick(object sender, EventArgs e)
		{
		}

		private string FixShellPathsInEditMode(string LastPath)
		{
			string LLastPath = LastPath;
			Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
							(ThreadStart)(() =>
							{

								foreach (ShellObject item in KnownFolders.All)
								{
									LLastPath = LLastPath.Replace(item.ParsingName, item.GetDisplayName(DisplayNameType.Default));
								}
							}));
			return LLastPath.Replace(".library-ms", "");
		}

		void duh_NavigateRequested(object sender, PathEventArgs e)
		{
			OnNavigateRequested(e);
			LastPath = e.ShellObject.ParsingName;
		}

		public string LastPath = "";

		private void HistoryCombo_GotFocus(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			FocusManager.SetIsFocusScope(this, true);
      
			HistoryCombo.Text = LastPath; //FixShellPathsInEditMode(LastPath);
      if (elPanel.Visibility == System.Windows.Visibility.Visible)
      {
        ExitEditMode();
      }
		}

		private void HistoryCombo_LostFocus(object sender, RoutedEventArgs e)
		{

			e.Handled = true;
			ExitEditMode();
		}

		private void HistoryCombo_MouseLeave(object sender, MouseEventArgs e)
		{

		}

		private void HistoryCombo_MouseUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			//elPanel.Visibility = System.Windows.Visibility.Collapsed;
			//if (LastPath != "")
			//{
			//    HistoryCombo.Text = FixShellPathsInEditMode(LastPath);
			//}
      if (e.LeftButton == MouseButtonState.Pressed)
			  EnterEditMode();
		}

		private void stackPanel1_MouseUp(object sender, MouseButtonEventArgs e)
		{

		}

    public bool IsInEditMode
    {
      get;
      set;
    }

		public void ExitEditMode()
		{

      if (mnu != null)
        mnu.IsOpen = false;
      IsInEditMode = false;
			elPanel.Visibility = System.Windows.Visibility.Visible;
			//if (HistoryCombo.Text != "")
			//{
			//    LastPath = HistoryCombo.Text;
			//}

			HistoryCombo.Text = "";
			elPanel.Focusable = true;
			elPanel.IsHitTestVisible = false;
			elPanel.Focus();
			elPanel.Focusable = false;
			elPanel.IsHitTestVisible = true;

		}

		public void EnterEditMode()
		{
      IsInEditMode = true;
			elPanel.Visibility = System.Windows.Visibility.Collapsed;
			if (LastPath != "")
			{
				HistoryCombo.Text = LastPath; //FixShellPathsInEditMode(LastPath);
			}
			else
			{
				HistoryCombo.Text = furthestrightitem.ShellObject.ParsingName; //FixShellPathsInEditMode(furthestrightitem.ShellObject.ParsingName);
			}
      
			//HistoryCombo.Focus();

      
		}

		private void HistoryCombo_KeyUp(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.Key == Key.Enter)
			{
				try
				{
          PathEventArgs ea = null;
          var path = String.Empty;
          if (HistoryCombo.Text.Trim().StartsWith("%"))
          {
            path = Environment.ExpandEnvironmentVariables(HistoryCombo.Text);
          }
          else
          {
            path = HistoryCombo.Text;
          }

          ea = new PathEventArgs(ShellObject.FromParsingName(path));
          
					OnNavigateRequested(ea);
					if (writetohistory == true)
					{
						if (hl.Contains(HistoryCombo.Text) == false)
						{
							hl.Add(HistoryCombo.Text);
						}
					}
				}
				catch (Exception)
				{

					// For now just handle the exception. later will be fixed to navigate correct path.
				}
        ExitEditMode();
			}
			if (e.Key == Key.Escape)
			{

				ExitEditMode();
			}
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
      if (furthestrightitem != null)
				furthestrightitem.BringIntoView();
			
		}

		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			//MessageBox.Show(e.Delta.ToString(), "Mouse Wheel Change");
			//itemholder.ScrollToHorizontalOffset(0);
		}

		private void HistoryCombo_DropDownOpened(object sender, EventArgs e)
		{
			HistoryCombo.Focus();
			HistoryCombo.Text = FixShellPathsInEditMode(LastPath);
			elPanel.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void HistoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			HistoryCombo.Focus();
			elPanel.Visibility = System.Windows.Visibility.Collapsed;
		}

		private void HistoryCombo_TextChanged(object sender, TextChangedEventArgs e)
		{
      if (IsInEditMode)
      {

        ContextMenu mnuTemp = CreateHistoryMenu(true);
        if (mnuTemp.Items.Count == 0)
        {
          if (mnu != null && mnu.IsOpen)
            mnu.IsOpen = false;
        }
        if (mnu == null)
          mnu = mnuTemp;
        else
        {
          if (mnuTemp.Items.Count > 0)
            mnu.Items.Clear();
          foreach (var item in mnuTemp.Items)
          {
            MenuItem g = new MenuItem();
            g.Header = (item as MenuItem).Header;
            g.Focusable = false;
            g.Width = grdMain.ActualWidth - 4;
            g.Click += new RoutedEventHandler(g_Click);
            mnu.Items.Add(g);
          }
        }
        mnu.Focusable = false;
        mnu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        mnu.PlacementTarget = this;
        if (mnuTemp.Items.Count > 0)
        {
          mnu.IsOpen = true;
        }
        else
        {
          if (mnu.IsOpen)
            mnu.IsOpen = false;
        }

        mnuTemp = null;
      }
		}

		private void HistoryCombo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			e.Handled = true;
			try
			{
				if (!mnu.IsOpen)
				{
					ExitEditMode();
				}
			}
			catch
			{

			}

		}

		private void HistoryCombo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			e.Handled = true;
		}

    private void HistoryCombo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (!IsInEditMode)
        EnterEditMode();
      TextBox tb = (sender as TextBox);

      if (tb == null)
      {
        return;
      }

      if (!tb.IsKeyboardFocusWithin)
      {
        tb.SelectAll();
        e.Handled = true;
        tb.Focus();
      }
      //e.Handled = true;
    }

    private void UserControl_GotFocus(object sender, RoutedEventArgs e)
    {
      itemholder.Focus();
    }

	}
}
