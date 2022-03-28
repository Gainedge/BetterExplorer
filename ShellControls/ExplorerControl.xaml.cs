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
using Icon = WPFUI.Common.Icon;
using IDataObject = System.Windows.Forms.IDataObject;
using Point = System.Windows.Point;
using ScrollEventArgs = ShellControls.ShellListView.ScrollEventArgs;
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
    public static List<LVItemColor> ColorCodes;
    public ExplorerControl() {
      InitializeComponent();
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
      this.ShellViewEx.EndItemLabelEdit += ShellViewExOnEndItemLabelEdit;

    }

    private void ShellViewExOnEndItemLabelEdit(Object? sender, Boolean e) {
      
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
          break;
        case 1:
          this.txtSelection.Text = "1 Selected Item";
          break;
        default:
          this.txtSelection.Text = $"{selectedCount} Selected Items";
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

            ColorCodes = docs.Root.Elements("ItemColorRow")
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
      startupLoc.Initialize(this.ShellViewEx.LVHandle, BESettings.StartupLocation.ToShellParsingName(), 0);
      this._Folder = startupLoc;
    }

    public ExplorerControl(IListItemEx folder, Boolean isFromTab = false) : this(isFromTab) {
      this._Folder = folder;
    }
    public ExplorerControl(String folder, Boolean isFromTab = false) : this(isFromTab) {
      var startupLoc = new FileSystemListItem();
      startupLoc.Initialize(this.ShellViewEx.LVHandle, folder.ToShellParsingName(), 0);
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
        if (this.bcMain.RootItem.Items.OfType<IListItemEx>().Last().IsSearchFolder) {
          this.bcMain.RootItem.Items.RemoveAt(this.bcMain.RootItem.Items.Count - 1);
        }


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
      this.OnUpdateTabInfo?.Invoke(this, new TabUpdateEventArgs(e.Folder, true));
    }

    private void ShellViewExOnNavigated(object? sender, NavigatedEventArgs e) {
      this.OnUpdateTabInfo?.Invoke(this, new TabUpdateEventArgs(e.Folder, false));
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        this.SetSelectedState();
        

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
    }
    private void ExplorerControl_OnLoaded(object sender, RoutedEventArgs e) {
      Application.Current.MainWindow.Title = this.ShellViewEx.CurrentFolder?.DisplayName ?? "Better Explorer";
      this.LoadColorCodesFromFile();
      if (this._IsLoaded) {
        this.tbHiddenFiles.IsChecked = ExplorerControl.IsShowHidden;
        this.ShellViewEx.ShowHidden = ExplorerControl.IsShowHidden;
        this.tbFileExtensions.IsChecked = ExplorerControl.IsShowExtensions;
        this.ShellViewEx.IsFileExtensionShown = ExplorerControl.IsShowExtensions;
      }

      if (this.AllowNavigation) {
        this.InitializeShellBrowser();
      } else {
        this.AllowNavigation = true;
      }
      var statef = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();

      if (!this._IsLoaded) {
        this._IsLoaded = true;
        this.OnUpdateTabInfo?.Invoke(this, new TabUpdateEventArgs(this._Folder, false));
        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref statef, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS | BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, false);
        this.tbHiddenFiles.IsChecked = statef.fShowAllObjects == 1;
        this.ShellViewEx.ShowHidden = this.tbHiddenFiles.IsChecked.Value;
        ExplorerControl.IsShowHidden = this.ShellViewEx.ShowHidden;
        this.tbFileExtensions.IsChecked = statef.fShowExtensions == 1;
        this.ShellViewEx.IsFileExtensionShown = statef.fShowExtensions == 1;
        ExplorerControl.IsShowExtensions = this.ShellViewEx.IsFileExtensionShown;
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

    public void InitializeShellBrowser() {
      if (!this._IsInitialyInitialized) {
        this.ShellViewHost.Child = this.ShellViewEx;
        this._ShellTreeView.ShellListView = this.ShellViewEx;
        this.ShellTreeViewHost.Child = this._ShellTreeView;

        this.ShellViewEx.Navigate_Full(this._Folder, true);

        this._IsInitialyInitialized = true;
      }
    }

    private void btnBack_Click(object sender, RoutedEventArgs e) {
      this._IsLogNavigation = true;
      this.OnBackClick?.Invoke(this.ShellViewEx, EventArgs.Empty);
      this.ShellViewEx.Navigate_Full(this.Log.NavigateBack(), true);
    }

    private void btnUp_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.NavigateParent();
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
      this.ShellViewEx.DeleteSelectedFiles(!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift));
    }

    private void btnViewTiles_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.Tile;
    }

    private void btnViewSmallIcons_Click(object sender, RoutedEventArgs e) {
      this.ShellViewEx.View = ShellViewStyle.SmallIcon;
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
      var menuItems = new List<Win32ContextMenuItem>();
      var sortbyItem = new Win32ContextMenuItem(menuItems);
      sortbyItem.Label = "Sort By";
      var sortSubItems = new List<Win32ContextMenuItem>();
      foreach (var column in this.lvHeader.Columns.OfType<ListViewColumnHeader>()) {
        var header = (column.Header as ShellListViewColumnHeader);
        var sortcolItem = new Win32ContextMenuItem(sortSubItems);
        sortcolItem.Label = header?.Content.ToString();
        sortcolItem.IsChecked = header.Collumn.ID == this.ShellViewEx.LastSortedColumnId;
        sortcolItem.Click += (o, args) => {
          this.ShellViewEx.SetSortCollumn(true, header.Collumn, this.ShellViewEx.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
          contextMenu.IsOpen = false;
        };
        sortSubItems.Add(sortcolItem);
      }

      sortbyItem.SubItems = sortSubItems;

      menuItems.Add(sortbyItem);
      var groupbyItem = new Win32ContextMenuItem(menuItems);
      groupbyItem.Label = "Group By";
      var groupSubItems = new List<Win32ContextMenuItem>();
      foreach (var column in this.lvHeader.Columns.OfType<ListViewColumnHeader>()) {
        var header = (column.Header as ShellListViewColumnHeader);
        var groupcolItem = new Win32ContextMenuItem(groupSubItems);
        groupcolItem.Label = header?.Content.ToString();
        groupcolItem.IsChecked = header.Collumn.ID == this.ShellViewEx.LastGroupCollumn?.ID;
        groupcolItem.Click += (o, args) => {
          this.ShellViewEx.EnableGroups();
          this.ShellViewEx.GenerateGroupsFromColumn(header.Collumn);
          contextMenu.IsOpen = false;
        };
        groupSubItems.Add(groupcolItem);
      }
      var groupcolNoneItem = new Win32ContextMenuItem(groupSubItems);
      groupcolNoneItem.Label = "(None)";
      groupcolNoneItem.IsChecked = this.ShellViewEx.LastGroupCollumn == null || this.ShellViewEx.LastGroupCollumn.ID == String.Empty;
      groupcolNoneItem.Click += (o, args) => {
        if (this.ShellViewEx.IsGroupsEnabled)
          this.ShellViewEx.DisableGroups();
        contextMenu.IsOpen = false;
      };
      groupSubItems.Add(groupcolNoneItem);
      groupbyItem.SubItems = groupSubItems;

      menuItems.Add(groupbyItem);
      menuItems.Add(new Win32ContextMenuItem(menuItems) { Type = MenuItemType.MFT_SEPARATOR });
      var ascendingMenuItem = new Win32ContextMenuItem(menuItems) { Label = "Ascending", IsChecked = this.ShellViewEx.LastSortOrder == SortOrder.Ascending };
      ascendingMenuItem.Click += (o, args) => {
        this.ShellViewEx.SetSortCollumn(true, this.ShellViewEx.Collumns.Single(s => s.ID == this.ShellViewEx.LastSortedColumnId), SortOrder.Ascending);
        contextMenu.IsOpen = false;
      };
      var descendingMenuItem = new Win32ContextMenuItem(menuItems) { Label = "Descending", IsChecked = this.ShellViewEx.LastSortOrder == SortOrder.Descending };
      descendingMenuItem.Click += (o, args) => {
        this.ShellViewEx.SetSortCollumn(true, this.ShellViewEx.Collumns.Single(s => s.ID == this.ShellViewEx.LastSortedColumnId), SortOrder.Descending);
        contextMenu.IsOpen = false;
      };
      menuItems.Add(ascendingMenuItem);
      menuItems.Add(descendingMenuItem);
      contextMenu.MenuItems = menuItems.ToArray();
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
      var menuItems = new List<Win32ContextMenuItem>();
      var multiSelectItem = new Win32ContextMenuItem(menuItems);
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
      menuItems.Add(multiSelectItem);
      var selectAllItem = new Win32ContextMenuItem(menuItems);
      selectAllItem.Label = "Select All        (Ctrl+A)";
      selectAllItem.Glyph = Icon.AppFolder20;
      selectAllItem.Click += (o, args) => {
        this.ShellViewEx.SelectAll();
        contextMenu.IsOpen = false;
      };
      menuItems.Add(selectAllItem);
      var invertSelectionItem = new Win32ContextMenuItem(menuItems);
      invertSelectionItem.Label = "Invert Selection";
      invertSelectionItem.Glyph = Icon.PositionBackward20;
      invertSelectionItem.Click += (o, args) => {
        this.ShellViewEx.InvertSelection();
        contextMenu.IsOpen = false;
      };
      menuItems.Add(invertSelectionItem);
      var clearSelectionItem = new Win32ContextMenuItem(menuItems);
      clearSelectionItem.Label = "Select None";
      clearSelectionItem.Glyph = Icon.CheckboxUnchecked24;
      clearSelectionItem.Click += (o, args) => {
        this.ShellViewEx.DeSelectAllItems();
        contextMenu.IsOpen = false;
      };
      menuItems.Add(clearSelectionItem);
      contextMenu.MenuItems = menuItems.ToArray();
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
  }
}
