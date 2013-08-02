using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Shell;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;


namespace BetterExplorerControls
{
	/// <summary>
	/// Interaction logic for BreadcrumbBarControl.xaml
	/// </summary>
	public partial class BreadcrumbBarControl : UserControl
	{
    private ObservableCollection<BreadcrumbBarFSItem> hl { get; set; }
    private TextBox Undertextbox;
    private bool Needfilter;
    private bool IsFiltered;
    private bool IsEcsPressed;

		public BreadcrumbBarControl()
		{
			InitializeComponent();
      this.hl = new ObservableCollection<BreadcrumbBarFSItem>();
      this.Loaded += BreadcrumbBarControl_Loaded;
		}

    void BreadcrumbBarControl_Loaded(object sender, RoutedEventArgs e)
    {
      Undertextbox = (TextBox)HistoryCombo.Template.FindName("PART_EditableTextBox", HistoryCombo);
      Undertextbox.AddHandler(TextBox.TextInputEvent,
                   new TextCompositionEventHandler(Undertextbox_TextInput),
                   true);
      Undertextbox.TextChanged += Undertextbox_TextChanged;

    }

    void Undertextbox_TextChanged(object sender, TextChangedEventArgs e)
    {
        
    }

    void Undertextbox_TextInput(object sender, TextCompositionEventArgs e)
    {
      if ((e.Text.All(Char.IsLetterOrDigit) || e.Text.All(Char.IsSymbol) || e.Text.All(Char.IsWhiteSpace)) && e.Text != "\r")
      {
        
        if (!HistoryCombo.IsDropDownOpen)
          Needfilter = true;
        HistoryCombo.IsDropDownOpen = true;
        if (!String.IsNullOrEmpty(Undertextbox.Text) && Needfilter)
        {
          HistoryCombo.Items.Filter += a =>
          {
            if (((BreadcrumbBarFSItem)a).RealPath.ToLower().StartsWith(Undertextbox.Text.ToLower()) || ((BreadcrumbBarFSItem)a).DisplayName.ToLower().StartsWith(Undertextbox.Text.ToLower()))
            {
              return true;
            }
            return false;
          };
          IsFiltered = true;
        }
      }
      Undertextbox.SelectionLength = 0;
      Undertextbox.SelectionStart = Undertextbox.Text.Length;
      Undertextbox.SelectedText = "";
    }

		void g_Click(object sender, RoutedEventArgs e)
		{
			HistoryCombo.Text = (string)(sender as MenuItem).Header;
      PathEventArgs args = new PathEventArgs(ShellObject.FromParsingName(HistoryCombo.Text.Trim().StartsWith("%") ? Environment.ExpandEnvironmentVariables(HistoryCombo.Text) : HistoryCombo.Text));
      OnNavigateRequested(args);
      ExitEditMode();
		}

		private BreadcrumbBarItem furthestrightitem;

		private bool writetohistory = true;

		public void ClearHistory()
		{
			hl.Clear();
		}

		public ObservableCollection<BreadcrumbBarFSItem> HistoryItems
		{
			get
			{
        ObservableCollection<BreadcrumbBarFSItem> hilist = new ObservableCollection<BreadcrumbBarFSItem>();

				foreach (var item in hl)
				{
					hilist.Add(item);
				}

				return hilist;
			}
			set
			{
				foreach (var item in value)
				{
					hl.Add(item);
				}
        HistoryCombo.ItemsSource = hl;
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
      return lpPoint;
    }

    void duh_ContextMenuRequested(object sender, PathEventArgs e)
    {
      ShellObject[] dirs = new ShellObject[1];
      dirs[0] = e.ShellObject;
      Point relativePoint = this.TransformToAncestor(Application.Current.MainWindow)
                          .Transform(new Point(0, 0));
      Point realCoordinates = Application.Current.MainWindow.PointToScreen(relativePoint);
      ContextShellMenu cm = new ContextShellMenu(dirs);
      cm.ShowContextMenu(new System.Drawing.Point((int)GetCursorPosition().X, (int)realCoordinates.Y + (int)this.Height));
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
      
			//HistoryCombo.Text = LastPath; 

      EnterEditMode();
		}

		private void HistoryCombo_LostFocus(object sender, RoutedEventArgs e)
		{

			e.Handled = true;
      if (IsInEditMode)
			  ExitEditMode();
		}


		private void HistoryCombo_MouseUp(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;

      if (e.LeftButton == MouseButtonState.Released)
      {
        IsEcsPressed = false;
        if (!IsInEditMode)
          EnterEditMode();
        Needfilter = false;

      }
		}

    public bool IsInEditMode
    {
      get;
      set;
    }

		public void ExitEditMode()
		{
      //FocusManager.SetIsFocusScope(this, false);
      IsInEditMode = false;
      elPanel.Visibility = System.Windows.Visibility.Visible;
      
      if (Undertextbox != null)
        Undertextbox.Visibility = System.Windows.Visibility.Hidden;
		}

		public void EnterEditMode()
		{
        IsInEditMode = true;
        elPanel.Visibility = System.Windows.Visibility.Collapsed;
        elPanel.Focusable = false;
        if (Undertextbox != null)
        {
          Undertextbox.Visibility = System.Windows.Visibility.Visible;
          //Undertextbox.SelectAll();
        }
        var obj = furthestrightitem.ShellObject;

        if (obj.ParsingName.StartsWith(":"))
        {
          var correctItems = KnownFolders.All.Where(w => obj.ParsingName.Contains(w.ParsingName)).ToArray();
          foreach (var item in correctItems)
          {
            ShellObject realItem = (ShellObject)item;
            HistoryCombo.Text = obj.ParsingName.Replace(realItem.ParsingName, realItem.GetDisplayName(DisplayNameType.Default)).Replace(".library-ms", "");
            realItem.Dispose();
          }
          
        }
        else
        {
          HistoryCombo.Text = furthestrightitem.ShellObject.ParsingName;
        }
        FocusManager.SetIsFocusScope(this, true);
		}

    private void RequestNavigation(String Path)
    {
      PathEventArgs ea = null;
      var path = String.Empty;
      BreadcrumbBarFSItem item = null;
      if (Path.Trim().StartsWith("%"))
      {
        path = Environment.ExpandEnvironmentVariables(Path);
        item = new BreadcrumbBarFSItem(Path, path);
      }
      else
      {
        path = Path;
        item = new BreadcrumbBarFSItem(ShellObject.FromParsingName(Path.StartsWith(":") ? "shell:" + Path : Path));
      }

      ea = new PathEventArgs(ShellObject.FromParsingName(path));

      OnNavigateRequested(ea);
      if (writetohistory == true)
      {
        if (hl.Select(s => s.RealPath).Contains(Path) == false)
        {
          hl.Add(item);
        }
      }
    }
    private void HistoryCombo_KeyUp(object sender, KeyEventArgs e)
		{
			e.Handled = true;
      IsEcsPressed = false;
			if (e.Key == Key.Enter)
			{
        IsEcsPressed = true;
				try
				{
          RequestNavigation(HistoryCombo.Text);
				}
				catch (Exception)
				{

					// For now just handle the exception. later will be fixed to navigate correct path.
				}
        ExitEditMode();
			}
			if (e.Key == Key.Escape)
			{
        IsEcsPressed = true;
				ExitEditMode();
			}
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
      if (furthestrightitem != null)
				furthestrightitem.BringIntoView();
			
		}

		private void HistoryCombo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			e.Handled = true;

			try
			{
				if (HistoryCombo.IsDropDownOpen)
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
      FocusManager.SetIsFocusScope(this, true);
      if (!IsInEditMode && !IsEcsPressed)
      {
        EnterEditMode();
        Undertextbox.Focus();
        Undertextbox.SelectAll();
      }
		}

  

    private void UserControl_GotFocus(object sender, RoutedEventArgs e)
    {
      itemholder.Focus();
    }

    private void HistoryCombo_DropDownClosed(object sender, EventArgs e)
    {
      Needfilter = true;
    }

    private void HistoryCombo_DropDownOpened(object sender, EventArgs e)
    {
      if (IsFiltered)
      {
        HistoryCombo.Items.Filter += a =>
        {
          return true;
        };
        IsFiltered = false;
      }
      if (!IsInEditMode)
        EnterEditMode();
      Undertextbox.Focus();

      if (HistoryCombo.Items.Count == 0)
      {
        HistoryCombo.IsDropDownOpen = false;
      }
     
    }

    private void HistoryCombo_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
        IsEcsPressed = true;
    }

    private void HistoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      try
      {
        RequestNavigation((e.AddedItems[0] as BreadcrumbBarFSItem).RealPath);
        IsEcsPressed = true;
      }
      catch (Exception)
      {
        //For now just handle the exception. later will be fixed to navigate correct path.
      }
      ExitEditMode();
    }
	}
}
