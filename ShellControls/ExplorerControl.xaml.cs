using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using BetterExplorer;
using BetterExplorerControls;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using Settings;
using ShellControls.ShellContextMenu;
using ShellControls.ShellListView;
using ShellControls.ShellTreeView;
using WPFUI.Controls;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using Control = System.Windows.Controls.Control;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Icon = WPFUI.Common.Icon;
using IDataObject = System.Windows.Forms.IDataObject;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MSG = BExplorer.Shell.Interop.MSG;
using Point = System.Windows.Point;
using ScrollEventArgs = ShellControls.ShellListView.ScrollEventArgs;
using Size = System.Windows.Size;
using UserControl = System.Windows.Controls.UserControl;

namespace ShellControls {
  /// <summary>
  /// Interaction logic for ExplorerControl.xaml
  /// </summary>
  public partial class ExplorerControl : UserControl {
    private static readonly List<string> Images = new List<string>(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".wmf" });
    private static readonly List<string> Movies = new List<string>(new[] { ".avi", ".mp4", ".ts", ".wmv", ".mkv", ".mpeg", ".mov" });
    private static readonly List<string> Subtitles = new List<string>(new[] { ".sub", ".srt" });
    public event EventHandler OnBackClick;
    public event EventHandler OnForwardClick;
    public event EventHandler<TabUpdateEventArgs> OnUpdateTabInfo;
    public NavigationLog Log { get; set; }
    private ClipboardMonitor cbm = new ClipboardMonitor();
    public Boolean AllowNavigation { get; set; }
    public ShellView ShellViewEx;
    private ShellTreeViewEx _ShellTreeView;
    private IListItemEx _Folder;
    private Boolean _IsLoaded = false;
    private Boolean _IsLogNavigation = false;
    private Boolean _IsFromTabCreation = false;
    private Boolean _IsInitialyInitialized = false;
    private static Boolean IsShowHidden;
    private static Boolean IsShowExtensions;
    public static List<LVItemColor>? ColorCodes;
    public ExplorerControl() {
      InitializeComponent();
    }

    private void ComponentDispatcherOnThreadFilterMessage(ref System.Windows.Interop.MSG msg, ref Boolean handled) {
      if (msg.message == 0x100) {
        handled = true;
        Keys keyData = ((Keys)((int)((long)msg.wParam))) | System.Windows.Forms.Control.ModifierKeys;
        User32.SetForegroundWindow(this.ShellViewEx.Handle);
        User32.keybd_event((byte)keyData, 0x42, 0x1, UIntPtr.Zero);
        User32.keybd_event((byte)keyData, 0x42, 0x1 | 0x2, UIntPtr.Zero);
        System.Windows.Forms.Application.DoEvents();
      } else if (msg.message == 0x101) {

      }
    }

    private void ComponentDispatcher_ThreadPreprocessMessage(ref System.Windows.Interop.MSG msg, ref Boolean handled) {
      if (msg.message == 0x100) {
        handled = true;
        Keys keyData = ((Keys)((int)((long)msg.wParam))) | System.Windows.Forms.Control.ModifierKeys;
        User32.SetForegroundWindow(this.ShellViewEx.Handle);
        User32.keybd_event((byte)keyData, 0x42, 0x1, UIntPtr.Zero);
        User32.keybd_event((byte)keyData, 0x42, 0x1 | 0x2, UIntPtr.Zero);
        System.Windows.Forms.Application.DoEvents();
      } else if (msg.message == 0x101) {

      }
    }

    private void ShellViewExOnBeginItemLabelEdit(Object? sender, RenameEventArgs e) {
      var item = this.ShellViewEx.Items[e.ItemIndex];
      this.txtRename.Text = item.DisplayName;
      if (item.IsFolder) {
        this.txtRename.SelectAll();
      } else {
        this.txtRename.SelectionStart = 0;
        var indexLastDot = this.ShellViewEx.IsFileExtensionShown ? this.txtRename.Text.LastIndexOf(".", StringComparison.Ordinal) : this.txtRename.Text.Length;
        this.txtRename.SelectionLength = indexLastDot;
      }

      this.pnlRename.IsOpen = true;
      this.txtRename.Focus();
    }

    private void ShellViewExOnEndItemLabelEdit(Object? sender, Boolean e) {
      if (!e) {
        var item = this.ShellViewEx.Items[this.ShellViewEx._ItemForRename];
        if (!item.DisplayName.Equals(this.txtRename.Text, StringComparison.InvariantCultureIgnoreCase)) {
          this.ShellViewEx.RenameShellItem(item.ComInterface, this.txtRename.Text, item.DisplayName != Path.GetFileName(item.ParsingName) && !item.IsFolder, item.Extension);
        }
      }
      this.pnlRename.IsOpen = false;
    }


    private void ShellViewExOnViewStyleChanged(object? sender, ViewChangedEventArgs e) {
      this.btnViewDetails.IsChecked = e.CurrentView == ShellViewStyle.Details;
      this.btnViewContents.IsChecked = e.CurrentView == ShellViewStyle.Content;
      this.btnViewExLarge.IsChecked = e.CurrentView == ShellViewStyle.ExtraLargeIcon;
      this.btnViewLarge.IsChecked = e.CurrentView == ShellViewStyle.LargeIcon;
      this.btnViewMedium.IsChecked = e.CurrentView == ShellViewStyle.Medium;
      this.btnViewSmall.IsChecked = e.CurrentView == ShellViewStyle.SmallIcon;
      this.btnViewTiles.IsChecked = e.CurrentView == ShellViewStyle.Tile;
    }

    private void CbmOnClipboardChanged(object? sender, Tuple<IDataObject> e) {
      this.btnPaste.IsEnabled = e.Item1.GetDataPresent(DataFormats.FileDrop) || e.Item1.GetDataPresent("Shell IDList Array");
    }

    private void ShellViewExOnSelectionChanged(object? sender, EventArgs e) {
      this.SetSelectedState();
      if (this.ShellViewEx.GetFirstSelectedItem()?.IsFolder == true) {
        this.btnChangeFolderIcon.Visibility = Visibility.Visible;
        this.btnClearForlderIcon.Visibility = Visibility.Visible;
      } else {
        this.btnChangeFolderIcon.Visibility = Visibility.Collapsed;
        this.btnClearForlderIcon.Visibility = Visibility.Collapsed;
      }
    }

    private void SetSelectedState() {
      var selectedCount = this.ShellViewEx.GetSelectedCount();
      var selectedItem = this.ShellViewEx.GetFirstSelectedItem();

      this.btnCut.IsEnabled = selectedCount > 0;
      this.btnCopy.IsEnabled = selectedCount > 0;
      this.btnRename.IsEnabled = selectedCount > 0;
      this.btnDelete.IsEnabled = selectedCount > 0;
      this.btnCut.IsEnabled = selectedCount > 0;
      this.btnShare.IsEnabled = selectedCount > 0 && selectedItem.IsFolder == false;

      if (selectedCount == 1 && !selectedItem.IsFolder &&
          ExplorerControl.Images.Contains(Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant())) {
        this.ctgImageTools.Visibility = Visibility.Visible;
        //this.txtImgdWidth = 
      } else {
        this.ctgImageTools.Visibility = Visibility.Collapsed;
      }

      switch (selectedCount) {
        case 0:
          this.txtSelection.Text = String.Empty;
          this.pnlSelectedCount.Visibility = Visibility.Collapsed;
          break;
        case 1:
          this.txtSelection.Text = "1 Selected Item";
          this.pnlSelectedCount.Visibility = Visibility.Visible;
          break;
        default:
          this.txtSelection.Text = $"{selectedCount} Selected Items";
          this.pnlSelectedCount.Visibility = Visibility.Visible;
          break;
      }
    }

    private void LoadColorCodesFromFile() {
      if (ColorCodes == null) {
        Task.Run(() => {
          var itemColorSettingsLocation =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
              @"BExplorer\itemcolors.cfg");

          if (File.Exists(itemColorSettingsLocation)) {
            var docs = XDocument.Load(itemColorSettingsLocation);

            ColorCodes = docs.Root?.Elements("ItemColorRow")?
              .Select(
                element =>
                  new LVItemColor(
                    element.Elements().ToArray()[0].Value,
                    Color.FromArgb(
                      BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[0],
                      BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[1],
                      BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[2],
                      BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[3])))
              .ToList();

            ColorCodes.ForEach(e => this.ShellViewEx.LVItemsColorCodes.Add(e));

          }
        });
      } else {
        this.ShellViewEx.LVItemsColorCodes.Clear();
        ColorCodes.ForEach(e => this.ShellViewEx.LVItemsColorCodes.Add(e));
      }
    }

    private Boolean _PreventSBValueChange = false;
    private void ShellViewExOnOnLVScroll(object? sender, ScrollEventArgs e) {

      //this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
      //  () => {
      //if (!e.IsPositionChangedOnly) {
      //this.sbVertical.Minimum = e.ScrollInfo.nMin;
      //this.sbVertical.Maximum = e.ScrollInfo.nMax - e.ScrollInfo.nPage;
      ////  //this.sbVertical.SmallChange = 140;
      ////  //this.sbVertical.LargeChange = 120*3;
      ////  //var vpsize = this.ShellViewEx.ClientRectangle.Height / 16D;
      ////  //if (vpsize)
      //  this.sbVertical.ViewportSize = this.ShellViewEx.ClientRectangle.Height / 20;
      //}

      //this._PreventSBValueChange = true;
      //this.sbVertical.Value = e.ScrollInfo.nPos;
      //}));

    }

    public ExplorerControl(Boolean isFromTab) : this() {
      this._IsFromTabCreation = isFromTab;
      var startupLoc = new FileSystemListItem();
      startupLoc.Initialize(this.ShellViewEx?.LVHandle ?? IntPtr.Zero, BESettings.StartupLocation.ToShellParsingName(), 0);
      this._Folder = startupLoc;
    }

    public ExplorerControl(IListItemEx folder, Boolean isFromTab = false) : this(isFromTab) {
      this._Folder = folder;
    }
    public ExplorerControl(String folder, Boolean isFromTab = false) : this(isFromTab) {
      var startupLoc = new FileSystemListItem();
      startupLoc.Initialize(this.ShellViewEx?.LVHandle ?? IntPtr.Zero, folder.ToShellParsingName(), 0);
      this._Folder = startupLoc;
    }
    private void ShellViewExOnNavigating(object? sender, NavigatingEventArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        this.txtSearch.WatermarkText = "Search " + e.Folder.DisplayName;
        if (e.Folder.IsFileSystem) {
          this.ctgFolderTools.Visibility = Visibility.Visible;
        } else {
          this.ctgFolderTools.Visibility = Visibility.Collapsed;
        }

        if (this.ShellViewEx.GetFirstSelectedItem()?.IsFolder == true) {
          this.btnChangeFolderIcon.Visibility = Visibility.Visible;
          this.btnClearForlderIcon.Visibility = Visibility.Visible;
        } else {
          this.btnChangeFolderIcon.Visibility = Visibility.Collapsed;
          this.btnClearForlderIcon.Visibility = Visibility.Collapsed;
        }
      }));
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => {
        //if (this.bcMain.RootItem.Items.OfType<IListItemEx>().Last().IsSearchFolder) {
        //  this.bcMain.RootItem.Items.RemoveAt(this.bcMain.RootItem.Items.Count - 1);
        //}


        var pidl = e.Folder.PIDL.ToString();
        this.bcMain.SetPathWithoutNavigate(pidl);

        if (this._IsLogNavigation) {
          this._IsLogNavigation = false;
        } else {
          this.Log.AddHistoryEntry(e.Folder);
        }
        this.btnBack.IsEnabled = this.Log.CanNavigateBackwards;
        this.btnForward.IsEnabled = this.Log.CanNavigateForwards;
        this.btnUp.IsEnabled = this.ShellViewEx.CanNavigateParent;
      }));
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => { this.OnUpdateTabInfo?.Invoke(this, new TabUpdateEventArgs(e.Folder, true)); }));
      //this.OnUpdateTabInfo?.Invoke(this, new TabUpdateEventArgs(e.Folder, true));
    }

    private void ShellViewExOnNavigated(object? sender, NavigatedEventArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => { this.OnUpdateTabInfo?.Invoke(this, new TabUpdateEventArgs(e.Folder, false)); }));
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        this.SetSelectedState();

        if (this.ShellViewEx.Items.Count == 1) {
          this.txtTotalCount.Text = "1 item";
        } else if (this.ShellViewEx.Items.Count > 1) {
          this.txtTotalCount.Text = $"{this.ShellViewEx.Items.Count} items";
        } else {
          this.txtTotalCount.Text = String.Empty;
        }

        Window.GetWindow(this)!.Title = e.Folder.DisplayName;

        //if (this.ShellViewEx.CurrentFolder.IsFileSystem) {
        //  this.ctgFolderTools.Visibility = Visibility.Visible;
        //} else {
        //  this.ctgFolderTools.Visibility = Visibility.Collapsed;
        //}
        //if (this.ShellViewEx.GetFirstSelectedItem()?.IsFolder == true) {
        //  this.btnChangeFolderIcon.Visibility = Visibility.Visible;
        //  this.btnClearForlderIcon.Visibility = Visibility.Visible;
        //} else {
        //  this.btnChangeFolderIcon.Visibility = Visibility.Collapsed;
        //  this.btnClearForlderIcon.Visibility = Visibility.Collapsed;
        //}
      }));
    }
    private void OnBreadcrumbbarNavigate(IListItemEx destination) {
      //this.IsNeedEnsureVisible = true;
      this.ShellViewEx.Navigate_Full(destination, true, true);
      //this.ShellViewWEx.Navigate(destination);
    }
    private void ExplorerControl_OnLoaded(object sender, RoutedEventArgs e) {
      
      

      if (this.AllowNavigation) {
        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
          this.InitializeShellBrowser();
          this.LoadColorCodesFromFile();
          if (this._IsLoaded) {
            this.tbHiddenFiles.IsChecked = ExplorerControl.IsShowHidden;
            this.ShellViewEx.ShowHidden = ExplorerControl.IsShowHidden;
            this.tbFileExtensions.IsChecked = ExplorerControl.IsShowExtensions;
            this.ShellViewEx.IsFileExtensionShown = ExplorerControl.IsShowExtensions;
          }

          Application.Current.MainWindow.Title = this.ShellViewEx.CurrentFolder?.DisplayName ?? "Better Explorer";
        }));
      } else {
        this.AllowNavigation = true;
      }

      var gv = (GridView)this.llvHeader.View;
      gv.Columns.CollectionChanged += gridView_CollectionChanged;
      

      if (!this._IsLoaded) {
        this._IsLoaded = true;
        this.OnUpdateTabInfo?.Invoke(this, new TabUpdateEventArgs(this._Folder, false));
        
        
        if (this._IsFromTabCreation) {
          if (this.pnlShellBrowser.Visibility == Visibility.Hidden) {
            this.pnlShellBrowser.Visibility = Visibility.Visible;
          }
        }
        var parentWindow = Window.GetWindow(this);
        parentWindow.SizeChanged += (o, args) => {
          if (this.pnlShellBrowser.Visibility == Visibility.Hidden) {
            this.pnlShellBrowser.Visibility = Visibility.Visible;
          }
        };
      }
    }

    private void gridView_CollectionChanged(Object? sender, NotifyCollectionChangedEventArgs e) {
      if (e.Action == NotifyCollectionChangedAction.Move) {
        if (e.OldStartingIndex == 0 || e.NewStartingIndex == 0) {
          return;
        }
        var gv = (GridView)this.llvHeader.View;
        var gvc = gv.Columns;
        this.ShellViewEx.RearangeColumns(gvc.OfType<ListViewColumnHeader>());
      }
    }

    public void InitializeShellBrowser() {
      if (!this._IsInitialyInitialized) {
        this.cbm.ClipboardChanged += CbmOnClipboardChanged;
        this._ShellTreeView = new ShellTreeViewEx(this.sbVerticalTree);
        this.ShellViewEx = new ShellView();
        this.ShellViewEx.VScroll = this.sbVertical;
        this.ShellViewEx.Header = this.lvHeader;
        this.ShellViewEx.Navigated += ShellViewExOnNavigated;
        this.ShellViewEx.Navigating += ShellViewExOnNavigating;
        this.ShellViewEx.OnLVScroll += ShellViewExOnOnLVScroll;
        this.ShellViewEx.SelectionChanged += ShellViewExOnSelectionChanged;
        this.ShellViewEx.ViewStyleChanged += ShellViewExOnViewStyleChanged;
        this.ShellViewEx.BeginItemLabelEdit += ShellViewExOnBeginItemLabelEdit;
        this.ShellViewEx.EndItemLabelEdit += ShellViewExOnEndItemLabelEdit;
        this.ShellViewHost.Child = this.ShellViewEx;
        this._ShellTreeView.ShellListView = this.ShellViewEx;
        this.ShellTreeViewHost.Child = this._ShellTreeView;
        //this.ShellViewWEx.Navigating += ShellViewExOnNavigating;
        //this.ShellViewWEx.Navigated += ShellViewExOnNavigated;

        this.ShellViewEx.Navigate_Full(this._Folder, true);

        //this.ShellViewWEx.Navigate(this._Folder);

        this._IsInitialyInitialized = true;
      }
      var statef = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
      BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref statef, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS | BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, false);
      this.tbHiddenFiles.IsChecked = statef.fShowAllObjects == 1;
      this.ShellViewEx.ShowHidden = this.tbHiddenFiles.IsChecked.Value;
      ExplorerControl.IsShowHidden = this.ShellViewEx.ShowHidden;
      this.tbFileExtensions.IsChecked = statef.fShowExtensions == 1;
      this.ShellViewEx.IsFileExtensionShown = statef.fShowExtensions == 1;
      ExplorerControl.IsShowExtensions = this.ShellViewEx.IsFileExtensionShown;
    }

    private void btnBack_Click(object sender, RoutedEventArgs e) {
      this._IsLogNavigation = true;
      this.OnBackClick?.Invoke(this.ShellViewEx, EventArgs.Empty);
      this.ShellViewEx.Navigate_Full(this.Log.NavigateBack(), true);
    }

    private void btnUp_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.NavigateParent();
      //this.ShellViewWEx.NavigateParent();
    }

    private void btnForward_Click(object sender, RoutedEventArgs e) {
      this._IsLogNavigation = true;
      this.OnForwardClick?.Invoke(this.ShellViewEx, EventArgs.Empty);
      this.ShellViewEx.Navigate_Full(this.Log.NavigateForward(), true);
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.DisableGroups();
    }

    private void btnViewDetails_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.Details;
    }

    private void btnViewMediumIcons_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.Medium;
      //this.ShellViewWEx.IconSize = 72;
    }

    private void BtnCut_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.CutSelectedFiles();
    }

    private void BtnCopy_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.CopySelectedFiles();
    }

    private void BtnPaste_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.PasteAvailableFiles();
    }

    private void BtnRename_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.RenameSelectedItem();
    }

    private void BtnShare_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.OpenShareUI();
    }

    private void BtnDelete_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.DeleteSelectedFiles((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift);
    }

    private void btnViewTiles_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.Tile;
    }

    private void btnViewSmallIcons_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.SmallIcon;
      //this.ShellViewWEx.IconSize = 48;
    }

    private void btnViewLargeIcons_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.LargeIcon;
    }

    private void btnViewExtraLargeIcons_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.ExtraLargeIcon;
    }

    private void btnViewContents_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.Content;
    }

    private void tbHiddenItems_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.ShowHidden = (sender as ToggleButton)?.IsChecked == true;
      ExplorerControl.IsShowHidden = this.ShellViewEx.ShowHidden;
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => {
        var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = (this.ShellViewEx.ShowHidden ? 1u : 0) };
        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state,
          BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS, true);
        this.ShellViewEx.RefreshContents();
      }));
    }

    private void tbFileExtensions_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.IsFileExtensionShown = (sender as ToggleButton)?.IsChecked == true;
      ExplorerControl.IsShowExtensions = this.ShellViewEx.IsFileExtensionShown;
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => {
        var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
        state.fShowExtensions = this.ShellViewEx.IsFileExtensionShown ? 1u : 0;
        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, true);
        this.ShellViewEx.RefreshContents();
      }));
    }

    private void btnSortGroup_Click(object sender, RoutedEventArgs e) {
      var contextMenu = new AcrylicShellContextMenu();
      //var menuItems = new List<Win32ContextMenuItem>();
      var sortbyItem = new Win32ContextMenuItem(contextMenu.MenuItems);
      sortbyItem.Label = "Sort By";

      foreach (var column in this.lvHeader.Columns.OfType<ListViewColumnHeader>()) {
        var header = (column.Header as ShellListViewColumnHeader);
        var sortcolItem = new Win32ContextMenuItem(sortbyItem.SubItems);
        sortcolItem.Label = header?.Content.ToString();
        sortcolItem.IsChecked = header.Collumn.ID == this.ShellViewEx.LastSortedColumnId;
        sortcolItem.Click += (o, args) => {
          this.ShellViewEx.SetSortCollumn(true, header.Collumn, this.ShellViewEx.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
          contextMenu.IsOpen = false;
        };
        sortbyItem.SubItems.Add(sortcolItem);
      }

      //sortbyItem.SubItems = sortSubItems;

      contextMenu.MenuItems.Add(sortbyItem);
      var groupbyItem = new Win32ContextMenuItem(contextMenu.MenuItems);
      groupbyItem.Label = "Group By";
      //var groupSubItems = new List<Win32ContextMenuItem>();
      foreach (var column in this.lvHeader.Columns.OfType<ListViewColumnHeader>()) {
        var header = (column.Header as ShellListViewColumnHeader);
        var groupcolItem = new Win32ContextMenuItem(groupbyItem.SubItems);
        groupcolItem.Label = header?.Content.ToString();
        groupcolItem.IsChecked = header.Collumn.ID == this.ShellViewEx.LastGroupCollumn?.ID;
        groupcolItem.Click += (o, args) => {
          this.ShellViewEx.EnableGroups();
          this.ShellViewEx.GenerateGroupsFromColumn(header.Collumn);
          contextMenu.IsOpen = false;
        };
        groupbyItem.SubItems.Add(groupcolItem);
      }
      var groupcolNoneItem = new Win32ContextMenuItem(groupbyItem.SubItems);
      groupcolNoneItem.Label = "(None)";
      groupcolNoneItem.IsChecked = this.ShellViewEx.LastGroupCollumn == null || this.ShellViewEx.LastGroupCollumn.ID == String.Empty;
      groupcolNoneItem.Click += (o, args) => {
        if (this.ShellViewEx.IsGroupsEnabled)
          this.ShellViewEx.DisableGroups();
        contextMenu.IsOpen = false;
      };
      groupbyItem.SubItems.Add(groupcolNoneItem);
      //groupbyItem.SubItems = groupSubItems;

      contextMenu.MenuItems.Add(groupbyItem);
      contextMenu.MenuItems.Add(new Win32ContextMenuItem(contextMenu.MenuItems) { Type = MenuItemType.MFT_SEPARATOR });
      var ascendingMenuItem = new Win32ContextMenuItem(contextMenu.MenuItems) { Label = "Ascending", IsChecked = this.ShellViewEx.LastSortOrder == SortOrder.Ascending };
      ascendingMenuItem.Click += (o, args) => {
        this.ShellViewEx.SetSortCollumn(true, this.ShellViewEx.Collumns.Single(s => s.ID == this.ShellViewEx.LastSortedColumnId), SortOrder.Ascending);
        contextMenu.IsOpen = false;
      };
      var descendingMenuItem = new Win32ContextMenuItem(contextMenu.MenuItems) { Label = "Descending", IsChecked = this.ShellViewEx.LastSortOrder == SortOrder.Descending };
      descendingMenuItem.Click += (o, args) => {
        this.ShellViewEx.SetSortCollumn(true, this.ShellViewEx.Collumns.Single(s => s.ID == this.ShellViewEx.LastSortedColumnId), SortOrder.Descending);
        contextMenu.IsOpen = false;
      };
      contextMenu.MenuItems.Add(ascendingMenuItem);
      contextMenu.MenuItems.Add(descendingMenuItem);
      contextMenu.IsSmallRounding = true;
      //contextMenu.MenuItems = menuItems.ToArray();
      contextMenu.IsSimpleMenu = true;
      contextMenu.PlacementTarget = this.btnSortGroup;
      contextMenu.Placement = PlacementMode.Custom;
      contextMenu.CustomPopupPlacementCallback = (size, targetSize, offset) => {
        var placement1 =
          new CustomPopupPlacement(new Point(-((size.Width - targetSize.Width) / 2), targetSize.Height + 5), PopupPrimaryAxis.Horizontal);
        return new[] { placement1 };
      };
      contextMenu.IsOpen = true;
    }

    private void BtnSelection_OnClick(object sender, RoutedEventArgs e) {
      var contextMenu = new AcrylicShellContextMenu();
      //var menuItems = new List<Win32ContextMenuItem>();
      var multiSelectItem = new Win32ContextMenuItem(contextMenu.MenuItems);
      multiSelectItem.Label = "Multiselect";
      multiSelectItem.Glyph = Icon.Multiselect20;
      multiSelectItem.Click += (o, args) => {
        this.ShellViewEx.ShowCheckboxes = !this.ShellViewEx.ShowCheckboxes;
        if (this.ShellViewEx.ShowCheckboxes) {
          User32.SendMessage(this.ShellViewEx.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, (Int32)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT);
          User32.SendMessage(this.ShellViewEx.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.CheckBoxes, (Int32)ListViewExtendedStyles.CheckBoxes);
        } else {
          User32.SendMessage(this.ShellViewEx.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, 0);
          User32.SendMessage(this.ShellViewEx.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.CheckBoxes, 0);
        }
        contextMenu.IsOpen = false;
      };
      contextMenu.MenuItems.Add(multiSelectItem);
      var selectAllItem = new Win32ContextMenuItem(contextMenu.MenuItems);
      selectAllItem.Label = "Select All        (Ctrl+A)";
      selectAllItem.Glyph = Icon.AppFolder20;
      selectAllItem.Click += (o, args) => {
        this.ShellViewEx.SelectAll();
        contextMenu.IsOpen = false;
      };
      contextMenu.MenuItems.Add(selectAllItem);
      var invertSelectionItem = new Win32ContextMenuItem(contextMenu.MenuItems);
      invertSelectionItem.Label = "Invert Selection";
      invertSelectionItem.Glyph = Icon.PositionBackward20;
      invertSelectionItem.Click += (o, args) => {
        this.ShellViewEx.InvertSelection();
        contextMenu.IsOpen = false;
      };
      contextMenu.MenuItems.Add(invertSelectionItem);
      var clearSelectionItem = new Win32ContextMenuItem(contextMenu.MenuItems);
      clearSelectionItem.Label = "Select None";
      clearSelectionItem.Glyph = Icon.CheckboxUnchecked24;
      clearSelectionItem.Click += (o, args) => {
        this.ShellViewEx.DeSelectAllItems();
        contextMenu.IsOpen = false;
      };
      contextMenu.MenuItems.Add(clearSelectionItem);
      contextMenu.IsSmallRounding = true;
      //contextMenu.MenuItems = menuItems.ToArray();
      contextMenu.IsSimpleMenu = true;
      contextMenu.PlacementTarget = this.btnSelection;
      contextMenu.Placement = PlacementMode.Custom;
      contextMenu.CustomPopupPlacementCallback = (size, targetSize, offset) => {
        var placement1 =
          new CustomPopupPlacement(new Point(-((size.Width - targetSize.Width) / 2), targetSize.Height + 5), PopupPrimaryAxis.Horizontal);
        return new[] { placement1 };
      };
      contextMenu.IsOpen = true;
    }

    private void BtnUsedSpace_OnClick(object sender, RoutedEventArgs e) {
      FolderSizeWindow.Open(this.ShellViewEx.CurrentFolder.ParsingName, Window.GetWindow(this));
    }

    private void BtnChangeFolderIcon_OnClick(object sender, RoutedEventArgs e) {
      new IconView().LoadIcons(this.ShellViewEx, false);
    }

    private void BtnClearForlderIcon_OnClick(object sender, RoutedEventArgs e) {
      this.ShellViewEx.ClearFolderIcon(this.ShellViewEx.GetFirstSelectedItem().ParsingName);
    }

    private void ExplorerControl_OnUnloaded(object sender, RoutedEventArgs e) {
      //this.ShellViewEx.KillAllThreads();
    }

    private void TxtSearch_OnSearchExecuted(Object sender, RoutedEventArgs e) {
      var args = (SearchExecutedRoutedEventArgs)e;
      this.ShellViewEx.Navigate_Full(args.SearchString, true, true);
    }

    private void RotateImages_Click(Object sender, RoutedEventArgs e) {
      RotateFlipType Rotation;
      string DefaultName_Addon = null;

      switch ((sender as Control)?.Name) {
        case "btnRotateLeft":
          Rotation = RotateFlipType.Rotate270FlipNone;
          DefaultName_Addon = "_Rotated270";
          break;
        case "btnRotateRight":
          Rotation = RotateFlipType.Rotate90FlipNone;
          DefaultName_Addon = "_Rotated90";
          break;
        case "btnFlipX":
          Rotation = RotateFlipType.RotateNoneFlipX;
          DefaultName_Addon = "_FlippedX";
          break;
        case "btnFlipY":
          Rotation = RotateFlipType.RotateNoneFlipY;
          DefaultName_Addon = "_FlippedY";
          break;
        default:
          throw new Exception("Invalid sender");
      }

      foreach (var item in this.ShellViewEx.SelectedItems) {
        var cvt = new Bitmap(item.ParsingName);
        cvt.RotateFlip(Rotation);
        if (BESettings.OverwriteImageWhileEditing) {
          cvt.Save(item.ParsingName);
        } else {
          string ext = item.ParsingName.Substring(item.ParsingName.LastIndexOf(".", StringComparison.Ordinal));
          string name = item.ParsingName;
          string namen = name.RemoveExtensionsFromFile(new System.IO.FileInfo(name).Extension);
          var newFilePath = namen + DefaultName_Addon + ext;
          cvt.Save(newFilePath);
          this.ShellViewEx.UnvalidateDirectory();
        }
        cvt.Dispose();
      }
    }

    private void BtnTest_OnClick(Object sender, RoutedEventArgs e) {
      var acrilicCm = new AcrylicContextMenu();
      acrilicCm.Placement = PlacementMode.Bottom;
      acrilicCm.PlacementTarget = this.btnTest;
      var mi = new MenuItem();
      mi.Header = "TestMenu";
      acrilicCm.Items.Add(mi);
      acrilicCm.IsOpen = true;
    }

    private void ShellViewHost_OnPreviewGotKeyboardFocus(Object sender, KeyboardFocusChangedEventArgs e) {
      //e.Handled = true;
    }

    private void ShellViewHost_OnKeyDown(Object sender, KeyEventArgs e) {
      //e.Handled = true;
      //System.Windows.Forms.Application.DoEvents();
      //Keyboard.Focus(this.ShellViewHost);
      //return;
      //if (e.Key < Key.End || e.Key > Key.Down) {
      //  return;
      //}

      //e.Handled = true;
      //User32.SetForegroundWindow(this.ShellViewEx.LVHandle);

      var fe = Keyboard.FocusedElement;
      if ((e.Key >= Key.Prior && e.Key <= Key.Down)) {
        var keyCode = (byte)KeyInterop.VirtualKeyFromKey(e.Key);
        User32.keybd_event(keyCode, 0x42, 0x1, UIntPtr.Zero);
        User32.keybd_event(keyCode, 0x42, 0x1 | 0x2, UIntPtr.Zero);
        System.Windows.Forms.Application.DoEvents();
        e.Handled = true;
      }
      //SendKeys.SendWait(e.Key.ToWinFormsKeyString());
    }

    private void ShellViewHost_OnPreviewMouseDown(Object sender, MouseButtonEventArgs e) {
      //e.Handled = true;
      //this.ShellViewEx.Focus(false, true);
    }

    private void ShellViewHost_OnKeyUp(Object sender, KeyEventArgs e) {
      //this.ShellViewEx.Focus(false, true);
      //e.Handled = true;
      ////User32.SetForegroundWindow(this.ShellViewEx.LVHandle);
      //var keyCode = (byte)KeyInterop.VirtualKeyFromKey(e.Key);
      //User32.keybd_event(keyCode, 0x42, 0x1 | 0x2, UIntPtr.Zero);
      //System.Windows.Forms.Application.DoEvents();
    }
    private CustomPopupPlacement[] CustomPopupPlacementCallbackLocal(Size popupsize, Size targetsize, Point offset) {
      if (this.ShellViewEx._ItemForRename == -1) {
        this.pnlRename.IsOpen = false;
        return Array.Empty<CustomPopupPlacement>();
      }
      var lvi = default(LVITEMINDEX);
      lvi.iItem = this.ShellViewEx._ItemForRename;
      lvi.iGroup = this.ShellViewEx.GetGroupIndex(this.ShellViewEx._ItemForRename);
      var itemBounds = new User32.RECT();
      User32.SendMessage(this.ShellViewEx.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
      var labelBounds = new User32.RECT() { Left = 2 };
      User32.SendMessage(this.ShellViewEx.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref labelBounds);
      var xPos = itemBounds.X + 13;
      var yPos = itemBounds.Y + 2;
      if (this.ShellViewEx.View != ShellViewStyle.Details) {
        labelBounds.Left = labelBounds.Left + 6;
        labelBounds.Right = labelBounds.Right - 12;
        labelBounds.Top = labelBounds.Top - 8;
        labelBounds.Bottom = labelBounds.Bottom - 8;
      }
      var labelBoundsReal = new User32.RECT() { Left = 2 };
      User32.SendMessage(this.ShellViewEx.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref labelBoundsReal);
      if (this.ShellViewEx.IconSize == 16) {
        this.txtRename.HorizontalAlignment = HorizontalAlignment.Left;
        if (this.ShellViewEx.View == ShellViewStyle.Details) {
          labelBoundsReal.Left = labelBoundsReal.Left + 3;
          labelBoundsReal.Right = labelBoundsReal.Right - 2;
        }

        xPos = labelBoundsReal.X;
        yPos = labelBoundsReal.Y + 10;
        this.txtRename.MaxWidth = itemBounds.Right - xPos - 10;
      } else {
        if (this.ShellViewEx.View == ShellViewStyle.Tile) {
          labelBoundsReal.Top = labelBoundsReal.Top + 16;
          labelBoundsReal.Left = labelBoundsReal.Left + 2;
          labelBoundsReal.Bottom = labelBoundsReal.Bottom + 16;
          labelBoundsReal.Right = labelBoundsReal.Right - 16;
          this.txtRename.HorizontalAlignment = HorizontalAlignment.Left;
          xPos = labelBoundsReal.X;
          this.txtRename.MaxWidth = itemBounds.Right - xPos - 10;
        } else {
          this.txtRename.HorizontalAlignment = HorizontalAlignment.Center;
          if (labelBounds.Left <= itemBounds.Left + 16) {
            labelBoundsReal.Left = labelBoundsReal.Left + 12;
            labelBoundsReal.Right = labelBoundsReal.Right - 16;
          }

          labelBoundsReal.Top = labelBoundsReal.Top - 26;
          labelBoundsReal.Bottom = labelBoundsReal.Bottom - 8;
          labelBoundsReal.Left = labelBoundsReal.Left + 3;
          labelBoundsReal.Right = labelBoundsReal.Right + 3;
          labelBoundsReal.Width = labelBoundsReal.Width + 2;
          this.txtRename.MaxWidth = itemBounds.Width - 20;
        }
        yPos = labelBoundsReal.Y - 2;
      }

      this.pnlRename.Width = itemBounds.Width - 15;
      var placement1 =
        new CustomPopupPlacement(new Point(xPos, targetsize.Height + yPos), PopupPrimaryAxis.Horizontal);
      return new[] { placement1 };
    }

    private void ShellViewHost_OnGotFocus(Object sender, RoutedEventArgs e) {
      //e.Handled = true;
    }

    private void TxtRename_OnKeyUp(Object sender, KeyEventArgs e) {
      if (e.Key == Key.Escape) {
        this.ShellViewEx.EndLabelEdit(true);
      } else if (e.Key == Key.Enter) {
          var item = this.ShellViewEx.Items[this.ShellViewEx._ItemForRename];
          if (!item.DisplayName.Equals(this.txtRename.Text, StringComparison.InvariantCultureIgnoreCase)) {
            this.ShellViewEx.RenameShellItem(item.ComInterface, this.txtRename.Text, item.DisplayName != Path.GetFileName(item.ParsingName) && !item.IsFolder, item.Extension);
          }
          this.pnlRename.IsOpen = false;
          this.ShellViewEx.EndLabelEdit();
      }
    }

    private void TxtRename_OnPreviewMouseMove(Object sender, MouseEventArgs e) {
      //if (e.LeftButton == MouseButtonState.Pressed) {
      //  e.Handled = true;
      //  this.txtRename.Select(this.txtRename.SelectionLength, 0);
      //}
    }

    private void TxtRename_OnLostKeyboardFocus(Object sender, KeyboardFocusChangedEventArgs e) {
      //this.ShellViewEx.EndLabelEdit(true);
    }

    private void TxtRename_OnLostFocus(Object sender, RoutedEventArgs e) {
      //this.ShellViewEx.EndLabelEdit(true);
    }

    private void PnlRename_OnClosed(Object? sender, EventArgs e) {
      this.ShellViewEx.EndLabelEdit();
    }
  }
}
