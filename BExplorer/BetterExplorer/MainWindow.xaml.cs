// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Gainedge ORG">
//   Better Explorer (c)2016
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace BetterExplorer {
  using BEHelper;
  using BetterExplorer.UsbEject;
  using BetterExplorerControls;
  using BExplorer.Shell;
  using BExplorer.Shell._Plugin_Interfaces;
  using BExplorer.Shell.CommonFileDialogs;
  using BExplorer.Shell.DropTargetHelper;
  using BExplorer.Shell.Interop;
  using Fluent;
  using LTR.IO.ImDisk;
  using Microsoft.Win32;
  using Settings;
  using SevenZip;
  using Shell32;
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Drawing;
  using System.Drawing.Imaging;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using System.ServiceModel;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;
  using System.Windows.Input;
  using System.Windows.Interop;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;
  using System.Windows.Shell;
  using System.Windows.Threading;
  using System.Xml;
  using System.Xml.Linq;
  using TaskDialogInterop;
  using wyDay.Controls;
  using Clipboards = System.Windows.Forms.Clipboard;
  using ContextMenu = Fluent.ContextMenu;
  using Image = System.Windows.Controls.Image;
  using MenuItem = Fluent.MenuItem;
  using WIN = System.Windows;

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Fluent.RibbonWindow {

    #region DLLImports

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref BExplorer.Shell.DataObject.Win32Point pt);

    //[DllImport("user32.dll", SetLastError = true)]
    //private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

    //[DllImport("user32.dll", SetLastError = true)]
    //private static extern int UnregisterHotKey(IntPtr hwnd, int id);

    [DllImport("winmm.dll")]
    static extern Int32 mciSendString(String command, StringBuilder buffer, Int32 bufferSize, IntPtr hwndCallback);

    #endregion

    #region Private Members
    private bool _IsCalledFromLoading, isOnLoad;
    private MenuItem misa, misd, misag, misdg;
    private ShellView _ShellListView = new ShellView();
    private bool IsNeedEnsureVisible;
    private ClipboardMonitor cbm = new ClipboardMonitor();
    private ContextMenu _CMHistory = new ContextMenu();
    private WIN.Shell.JumpList AppJL = new WIN.Shell.JumpList();
    //private List<string> Archives = new List<string>(new[] { ".rar", ".zip", ".7z", ".tar", ".gz", ".xz", ".bz2" });
    private List<string> Images = new List<string>(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".wmf" });
    private string SelectedArchive = "";
    private bool KeepBackstageOpen = false;
    string sessionid = DateTime.UtcNow.ToFileTimeUtc().ToString();
    string logdir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\ActionLog\\";
    WIN.Forms.Timer updateCheckTimer = new WIN.Forms.Timer();
    Double CommandPromptWinHeight;

    //List<LVItemColor> LVItemsColor { get; set; }
    ContextMenu chcm;

    WIN.Forms.Timer focusTimer = new WIN.Forms.Timer() { Interval = 500 };
    private WIN.Forms.Timer _ProgressTimer = new WIN.Forms.Timer() { Interval = 1000, Enabled = false };
    private string _DBPath = Path.Combine(KnownFolders.RoamingAppData.ParsingName, @"BExplorer\Settings.sqlite");

    private IntPtr Handle;
    private ObservableCollectionEx<LVItemColor> LVItemsColorCol { get; set; }
    public Dictionary<String, Dictionary<IListItemEx, List<string>>> Badges { get; set; }
    #endregion

    public bool IsMultipleWindowsOpened { get; set; }

    #region Events

    private void btnAbout_Click(object sender, RoutedEventArgs e) => new fmAbout(this).ShowDialog();
    private void btnBugtracker_Click(object sender, RoutedEventArgs e) => Process.Start("http://bugs.gainedge.org/public/betterexplorer");
    private void TheRibbon_CustomizeQuickAccessToolbar(object sender, EventArgs e) => CustomizeQAT.Open(this, this.TheRibbon);

    private void btnConsolePane_Click(object sender, RoutedEventArgs e) {
      Settings.BESettings.IsConsoleShown = this.btnConsolePane.IsChecked.Value;
      if (this.btnConsolePane.IsChecked.Value) {
        this.rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);
        this.rCommandPrompt.MinHeight = 100;
        this.spCommandPrompt.Height = GridLength.Auto;
        if (!this.ctrlConsole.IsProcessRunning) {
          this.ctrlConsole.ChangeFolder(this._ShellListView.CurrentFolder.ParsingName, this._ShellListView.CurrentFolder.IsFileSystem);
        }
      } else {
        this.rCommandPrompt.MinHeight = 0;
        this.rCommandPrompt.Height = new GridLength(0);
        this.spCommandPrompt.Height = new GridLength(0);
        this.ctrlConsole.StopProcess();
      }
    }

    private void backstage_IsOpenChanged(object sender, DependencyPropertyChangedEventArgs e) {
      if ((Boolean)e.NewValue) {
        this._ShellListView.IsFocusAllowed = false;
        this.backstage.Focus();
      } else {
        this._ShellListView.IsFocusAllowed = true;
      }
      this.autoUpdater.Visibility = Visibility.Visible;
      this.autoUpdater.UpdateLayout();

      if (this.KeepBackstageOpen) {
        this.backstage.IsOpen = true;
        this.KeepBackstageOpen = false;
      }
    }

    /// <summary>
    /// Loads initial position of the main window
    /// </summary>
    private void LoadInitialWindowPositionAndState() {
      this.Width = BESettings.LastWindowWidth;
      this.Height = BESettings.LastWindowHeight;

      var location = new System.Drawing.Point();
      try {
        location = new System.Drawing.Point((int)BESettings.LastWindowPosLeft, (int)BESettings.LastWindowPosTop);
      } catch { }

      this.Left = location.X;
      this.Top = location.Y;

      switch (BESettings.LastWindowState) {
        case 2:
          this.WindowState = WindowState.Maximized;
          break;
        case 1:
          this.WindowState = WindowState.Minimized;
          break;
        case 0:
          this.WindowState = WindowState.Normal;
          break;
        default:
          this.WindowState = WindowState.Maximized;
          break;
      }

      this.chkRibbonMinimizedGlass.IsChecked = BESettings.IsGlassOnRibonMinimized;
      this.TheRibbon.IsMinimized = BESettings.IsRibonMinimized;

      //CommandPrompt window size
      this.CommandPromptWinHeight = BESettings.CmdWinHeight;
      this.rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);

      if (BESettings.IsConsoleShown) {
        this.rCommandPrompt.MinHeight = 100;
        this.rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);
        this.spCommandPrompt.Height = GridLength.Auto;
      } else {
        this.rCommandPrompt.MinHeight = 0;
        this.rCommandPrompt.Height = new GridLength(0);
        this.spCommandPrompt.Height = new GridLength(0);
      }
    }

    private void LoadColorCodesFromFile() {
      Task.Run(() => {
        var itemColorSettingsLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"BExplorer\itemcolors.cfg");

        if (File.Exists(itemColorSettingsLocation)) {
          var docs = XDocument.Load(itemColorSettingsLocation);

          docs.Root.Elements("ItemColorRow")
            .Select(
              element =>
                new LVItemColor(
                  element.Elements().ToArray()[0].Value,
                  WIN.Media.Color.FromArgb(
                    BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[0],
                    BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[1],
                    BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[2],
                    BitConverter.GetBytes(Convert.ToInt32(element.Elements().ToArray()[1].Value))[3])))
            .ToList().ForEach(e => this.LVItemsColorCol.Add(e));

        }
      });
    }

    private void RibbonWindow_Initialized(object sender, EventArgs e) {
      this.LoadInitialWindowPositionAndState();
      this.LoadColorCodesFromFile();

      AppCommands.RoutedNewTab.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
      AppCommands.RoutedEnterInBreadCrumbCombo.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Alt));
      AppCommands.RoutedChangeTab.InputGestures.Add(new KeyGesture(Key.Tab, ModifierKeys.Control));
      AppCommands.RoutedCloseTab.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
      AppCommands.RoutedNavigateBack.InputGestures.Add(new KeyGesture(Key.Left, ModifierKeys.Alt));
      AppCommands.RoutedNavigateFF.InputGestures.Add(new KeyGesture(Key.Right, ModifierKeys.Alt));
      AppCommands.RoutedNavigateUp.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));
      AppCommands.RoutedGotoSearch.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
    }

    #region ViewEnumerationComplete

    /// <summary>
    /// Sets up btnSort and btnGroup so they have the correct items after navigating 
    /// </summary>
    private void SetSortingAndGroupingButtons() {
      this.btnSort.Items.Clear();
      this.btnGroup.Items.Clear();

      try {
        foreach (Collumns item in this._ShellListView.Collumns.Where(x => x != null)) {
          var lastSortedColumn = this._ShellListView.Collumns.FirstOrDefault(w => w.ID == this._ShellListView.LastSortedColumnId);
          if (lastSortedColumn != null) {
            var IsChecked1 = (item.pkey.fmtid == lastSortedColumn.pkey.fmtid) && (item.pkey.pid == lastSortedColumn.pkey.pid);
            this.btnSort.Items.Add(Utilities.Build_MenuItem(item.Name, item, checkable: true, isChecked: IsChecked1, GroupName: "GR2", onClick: this.mi_Click));
          }
          var IsCheckable2 = this._ShellListView.LastGroupCollumn != null && (item.pkey.fmtid == this._ShellListView.LastGroupCollumn.pkey.fmtid) && (item.pkey.pid == this._ShellListView.LastGroupCollumn.pkey.pid);
          this.btnGroup.Items.Add(Utilities.Build_MenuItem(item.Name, item, checkable: true, isChecked: IsCheckable2, GroupName: "GR3", onClick: this.mig_Click));
        }
      } catch (Exception ex) {
        //FIXME: I disable this message because of strange null after filter
        MessageBox.Show("BetterExplorer had an issue loading the visible columns for the current view. You might not be able to sort or group items.", ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
      }

      this.btnSort.Items.Add(new Separator() { Focusable = false });
      this.misa = Utilities.Build_MenuItem(this.FindResource("miAscending"), checkable: true, GroupName: "GR1", onClick: this.misa_Click);
      this.misd = Utilities.Build_MenuItem(this.FindResource("miDescending"), checkable: true, GroupName: "GR1", onClick: this.misd_Click);

      if (this._ShellListView.LastSortOrder == WIN.Forms.SortOrder.Ascending)
        this.misa.IsChecked = true;
      else
        this.misd.IsChecked = true;

      this.btnSort.Items.Add(this.misa);
      this.btnSort.Items.Add(this.misd);

      this.btnGroup.Items.Add(Utilities.Build_MenuItem("(none)", GroupName: "GR3", checkable: true, isChecked: this._ShellListView.LastGroupCollumn == null, onClick: this.misng_Click));
      this.btnGroup.Items.Add(new Separator());

      this.misag = Utilities.Build_MenuItem(this.FindResource("miAscending"), checkable: true, GroupName: "GR4", onClick: this.misag_Click);
      this.misdg = Utilities.Build_MenuItem(this.FindResource("miDescending"), checkable: true, GroupName: "GR4", onClick: this.misag_Click);

      if (this._ShellListView.LastGroupOrder == WIN.Forms.SortOrder.Ascending)
        this.misag.IsChecked = true;
      else
        this.misdg.IsChecked = true;

      this.btnGroup.Items.Add(this.misag);
      this.btnGroup.Items.Add(this.misdg);
    }

    void misag_Click(object sender, RoutedEventArgs e) {
      this._ShellListView.SetGroupOrder();
    }

    private void SetupColumnsButton() {
      var allAvailColls = this._ShellListView.AllAvailableColumns.Values.ToList();
      this.btnMoreColls.Items.Clear();
      this.chcm.Items.Clear();

      for (int j = 1; j < 10; j++) {
        //TODO: Try to remove this Try Catch!!
        try {
          var IsChecked = this._ShellListView.Collumns.Any(col => col.pkey.fmtid == allAvailColls[j].pkey.fmtid && col.pkey.pid == allAvailColls[j].pkey.pid);
          this.btnMoreColls.Items.Add(Utilities.Build_MenuItem(allAvailColls[j].Name, allAvailColls[j], checkable: true, onClick: this.mic_Click, isChecked: IsChecked));
          this.chcm.Items.Add(Utilities.Build_MenuItem(allAvailColls[j].Name, allAvailColls[j], checkable: true, onClick: this.mic_Click, isChecked: IsChecked));
        } catch (Exception) {
        }
      }

      int ItemsCount = this._ShellListView.Items.Count;
      this.sbiItemsCount.Visibility = ItemsCount == 0 ? Visibility.Collapsed : Visibility.Visible;
      this.sbiItemsCount.Content = ItemsCount == 1 ? "1 item" : ItemsCount + " items";
      this.sbiSelItemsCount.Visibility = this._ShellListView.GetSelectedCount() == 0 ? Visibility.Collapsed : Visibility.Visible;
      this.spSelItems.Visibility = this.sbiSelItemsCount.Visibility;

      this.btnMoreColls.Items.Add(new Separator());
      this.btnMoreColls.Items.Add(Utilities.Build_MenuItem(this.FindResource("btnMoreColCP"), allAvailColls, onClick: this.micm_Click));
      this.btnMoreColls.Tag = allAvailColls;

      this.chcm.Items.Add(new Separator());
      this.chcm.Items.Add(Utilities.Build_MenuItem(this.FindResource("btnMoreColCP"), allAvailColls, onClick: this.micm_Click));
    }

    #endregion

    void misd_Click(object sender, RoutedEventArgs e) {
      foreach (var item in this.btnSort.Items.OfType<MenuItem>().Where(item => item.IsChecked && item != (sender as MenuItem))) {
        this._ShellListView.SetSortCollumn(true, (Collumns)item.Tag, WIN.Forms.SortOrder.Descending);
      }
    }

    void misa_Click(object sender, RoutedEventArgs e) {
      foreach (var item in this.btnSort.Items.OfType<MenuItem>().Where(item => item.IsChecked && item != (sender as MenuItem))) {
        this._ShellListView.SetSortCollumn(true, (Collumns)item.Tag, WIN.Forms.SortOrder.Ascending);
      }
    }

    void micm_Click(object sender, RoutedEventArgs e) {
      var fMoreCollumns = new MoreColumns();
      fMoreCollumns.PopulateAvailableColumns((List<Collumns>)(sender as FrameworkElement).Tag, this._ShellListView, this.PointToScreen(Mouse.GetPosition(this)));
    }

    void mic_Click(object sender, RoutedEventArgs e) {
      var mi = (sender as MenuItem);
      Collumns col = (Collumns)mi.Tag;
      this._ShellListView.SetColInView(col, !mi.IsChecked);
      //if (mi.IsChecked) {
      //  this.pnlShellViewControl.AddHeaderColumn(col);
      //} else {
      //  this.pnlShellViewControl.RemoveHeaderColumn(col);
      //}
    }

    void miItem_Click(object sender, RoutedEventArgs e) {
      MenuItem mi = sender as MenuItem;
      ShellItem SaveLoc = mi.Tag as ShellItem;

      if (this._ShellListView.CurrentFolder.ParsingName.Contains(KnownFolders.Libraries.ParsingName) && this._ShellListView.CurrentFolder.ParsingName.EndsWith("library-ms")) {
        var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(this._ShellListView.CurrentFolder.ParsingName), false);
        lib.DefaultSaveFolder = SaveLoc.ParsingName;
        lib.Close();
      } else if (this._ShellListView.GetFirstSelectedItem().ParsingName.Contains(KnownFolders.Libraries.ParsingName)) {
        var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(this._ShellListView.GetFirstSelectedItem().ParsingName), false);
        lib.DefaultSaveFolder = SaveLoc.ParsingName;
        lib.Close();
      }
    }

    void LinksFolderWarcher_Renamed(object sender, RenamedEventArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
          () => this.btnFavorites.Items.OfType<MenuItem>().First(x => (x.Tag as ShellItem).ParsingName == e.OldFullPath).Header = Path.GetFileNameWithoutExtension(e.Name)));
    }

    void LinksFolderWarcher_Deleted(object sender, FileSystemEventArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
          () => this.btnFavorites.Items.Remove(this.btnFavorites.Items.OfType<MenuItem>().First(item => item.Header.ToString() == Path.GetFileNameWithoutExtension(e.Name)))));
    }

    void LinksFolderWarcher_Created(object sender, FileSystemEventArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                      (Action)(() => {
                        if (Path.GetExtension(e.FullPath).ToLowerInvariant() == ".lnk") {
                          var so = new ShellItem(e.FullPath);
                          so.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                          so.Thumbnail.CurrentSize = new WIN.Size(16, 16);
                          ImageSource icon = so.Thumbnail.BitmapSource;

                          this.btnFavorites.Items.Add(Utilities.Build_MenuItem(so.DisplayName, so, icon, onClick: this.mif_Click));
                        }
                      }));
    }

    /// <summary>
    /// Sets up each ribbon tab after select or navigate in ShellListView
    /// </summary>
    /// <param name="selectedItemsCount">The number of items selected</param>
    /// <param name="selectedItem">The last selected item</param>
    /// <remarks>
    /// Only used in SetupUIOnSelectOrNavigate
    /// </remarks>
    private void SetUpRibbonTabsVisibilityOnSelectOrNavigate(int selectedItemsCount, IListItemEx selectedItem) {
      #region Search Contextual Tab
      this.ctgSearch.Visibility = this.BooleanToVisibiliy(this._ShellListView.CurrentFolder.IsSearchFolder);
      if (this.ctgSearch.Visibility == Visibility.Visible && !this._ShellListView.CurrentFolder.IsSearchFolder) {
        this.ctgSearch.Visibility = Visibility.Collapsed;
        this.TheRibbon.SelectedTabItem = this.HomeTab;
      }
      #endregion

      #region Folder Tools Context Tab
      this.ctgFolderTools.Visibility = this.BooleanToVisibiliy((selectedItemsCount == 1 && selectedItem.IsFolder && selectedItem.IsFileSystem && !selectedItem.IsDrive && !selectedItem.IsNetworkPath));

      if (BESettings.AutoSwitchFolderTools && this.ctgFolderTools.Visibility == Visibility.Visible) {
        this.TheRibbon.SelectedTabItem = this.ctgFolderTools.Items[0];
      }
      #endregion

      #region Drive Contextual Tab
      this.ctgDrive.Visibility = this.BooleanToVisibiliy(this._ShellListView.CurrentFolder.IsDrive || (selectedItemsCount == 1 && (selectedItem.IsDrive || (selectedItem.Parent != null && selectedItem.Parent.IsDrive))));
      if (BESettings.AutoSwitchDriveTools && this.ctgDrive.Visibility == Visibility.Visible && selectedItemsCount == 1 && selectedItem.IsDrive) {
        this.TheRibbon.SelectedTabItem = this.ctgDrive.Items[0];
      }
      #endregion

      #region Library Context Tab
      this.ctgLibraries.Visibility = this.BooleanToVisibiliy((selectedItemsCount == 1 && this._ShellListView.CurrentFolder.ParsingName.Equals(KnownFolders.Libraries.ParsingName)) || (selectedItemsCount == 1 && selectedItem.Parent != null && selectedItem.Parent.ParsingName.Equals(KnownFolders.Libraries.ParsingName)));

      if (this.ctgLibraries.Visibility == Visibility.Visible && BESettings.AutoSwitchLibraryTools) {
        this.TheRibbon.SelectedTabItem = this.ctgLibraries.Items[0];
      }

      /*
        if (ctgLibraries.Visibility == Visibility.Visible && _ShellListView.CurrentFolder.ParsingName.Equals(KnownFolders.Libraries.ParsingName)) {
          if (selectedItem != null && selectedItemsCount == 1)
            SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(selectedItem.ParsingName), false));
        } else if (ctgLibraries.Visibility == Visibility.Visible && _ShellListView.CurrentFolder.Parent.ParsingName.Equals(KnownFolders.Libraries.ParsingName)) {
          if (selectedItemsCount == 1)
            SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(_ShellListView.CurrentFolder.ParsingName), false));
        }
        if (selectedItemsCount == 0) {
          ctgLibraries.Visibility = Visibility.Collapsed;
        }
      */
      if (selectedItemsCount == 0)
        this.ctgLibraries.Visibility = Visibility.Collapsed;
      else if (selectedItemsCount > 1) { } else if (this.ctgLibraries.Visibility == Visibility.Visible && this._ShellListView.CurrentFolder.ParsingName.Equals(KnownFolders.Libraries.ParsingName))
        this.SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(selectedItem.ParsingName), false));
      else if (this.ctgLibraries.Visibility == Visibility.Visible && this._ShellListView.CurrentFolder.Parent.ParsingName.Equals(KnownFolders.Libraries.ParsingName))
        this.SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(this._ShellListView.CurrentFolder.ParsingName), false));

      #endregion

      #region Archive Contextual Tab
      this.ctgArchive.Visibility = WIN.Visibility.Collapsed; //TODO: Restore this: BooleanToVisibiliy(selectedItemsCount == 1 && Archives.Contains(Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant()));
      if (BESettings.AutoSwitchArchiveTools && this.ctgArchive.Visibility == Visibility.Visible)
        this.TheRibbon.SelectedTabItem = this.ctgArchive.Items[0];

      #endregion

      #region Application Context Tab
      this.ctgExe.Visibility = this.BooleanToVisibiliy(selectedItemsCount == 1 && !selectedItem.IsFolder && (Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant() == ".exe" || Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant() == ".msi"));
      if (BESettings.AutoSwitchApplicationTools && this.ctgExe.Visibility == Visibility.Visible) {
        this.TheRibbon.SelectedTabItem = this.ctgExe.Items[0];
      }
      #endregion

      #region Image Context Tab
      this.ctgImage.Visibility = this.BooleanToVisibiliy(selectedItemsCount == 1 && !selectedItem.IsFolder && this.Images.Contains(Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant()));
      if (this.ctgImage.Visibility == Visibility.Visible) {
        try {
          if (new FileInfo(selectedItem.ParsingName).Length != 0) {
            using (var cvt = new Bitmap(selectedItem.ParsingName)) {
              this.imgSizeDisplay.WidthData = cvt.Width.ToString();
              this.imgSizeDisplay.HeightData = cvt.Height.ToString();

              if (BESettings.AutoSwitchImageTools)
                this.TheRibbon.SelectedTabItem = this.ctgImage.Items[0];
            }
          }
        } catch (Exception) {
          MessageBox.Show("Image was invalid");
        }
      }
      #endregion

      #region Virtual Disk Context Tab
      this.ctgVirtualDisk.Visibility = this.BooleanToVisibiliy(selectedItemsCount == 1 && !selectedItem.IsFolder && Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant() == ".iso");
      if (BESettings.AutoSwitchVirtualDriveTools && this.ctgVirtualDisk.Visibility == Visibility.Visible) {
        this.TheRibbon.SelectedTabItem = this.ctgVirtualDisk.Items[0];
      }
      #endregion

    }

    /// <summary>
    /// Sets up the status bar
    /// </summary>
    /// <param name="selectedItemsCount">The number of items currently selected</param>
    private void SetUpStatusBarOnSelectOrNavigate(int selectedItemsCount) {
      this.spSelItems.Visibility = this.BooleanToVisibiliy(selectedItemsCount > 0);
      this.sbiSelItemsCount.Visibility = this.BooleanToVisibiliy(selectedItemsCount > 0);
      if (selectedItemsCount == 1)
        this.sbiSelItemsCount.Content = "1 item selected";
      else if (selectedItemsCount > 1)
        this.sbiSelItemsCount.Content = selectedItemsCount.ToString() + " items selected";
    }

    private void SetUpButtonsStateOnSelectOrNavigate(int selectedItemsCount, IListItemEx selectedItem) {
      this.btnBadges.IsEnabled = selectedItemsCount > 0;
      this.btnCopy.IsEnabled = selectedItemsCount > 0;
      this.btnCopyto.IsEnabled = selectedItemsCount > 0;
      this.btnMoveto.IsEnabled = selectedItemsCount > 0;
      this.btnCut.IsEnabled = selectedItemsCount > 0;
      this.btnDelete.IsEnabled = selectedItem != null && selectedItem.IsFileSystem;
      this.btnRename.IsEnabled = selectedItem != null && (selectedItem.IsFileSystem || (selectedItem.Parent != null && selectedItem.Parent.Equals(KnownFolders.Libraries)));
      this.btnProperties3.IsEnabled = selectedItemsCount > 0;
      if (selectedItem != null) {
        var rg = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + Path.GetExtension(selectedItem.ParsingName) + @"\OpenWithProgids");
        if (rg == null)
          this.btnEdit.IsEnabled = false;
        else {
          string filetype = rg.GetValueNames()[0];
          rg.Close();

          using (var rgtype = Registry.ClassesRoot.OpenSubKey(filetype + @"\shell\edit\command")) {
            this.btnEdit.IsEnabled = !(rgtype == null);
          }
        }
      }

      this.btnSelAll.IsEnabled = selectedItemsCount != this._ShellListView.Items.Count;
      this.btnSelNone.IsEnabled = selectedItemsCount > 0;
      this.btnShare.IsEnabled = selectedItemsCount == 1 && selectedItem.IsFolder;
      this.btnAdvancedSecurity.IsEnabled = selectedItemsCount == 1;
      this.btnHideSelItems.IsEnabled = this._ShellListView.CurrentFolder.IsFileSystem;
    }

    private void SetupLibrariesTab(ShellLibrary lib) {
      this.IsFromSelectionOrNavigation = true;
      this.chkPinNav.IsChecked = lib.IsPinnedToNavigationPane;
      this.IsFromSelectionOrNavigation = false;

      foreach (ShellItem item in lib) {
        item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
        item.Thumbnail.CurrentSize = new WIN.Size(16, 16);

        this.btnDefSave.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.Thumbnail.BitmapSource, GroupName: "GRDS1", checkable: true,
                                                      isChecked: item.ParsingName == lib.DefaultSaveFolder, onClick: this.miItem_Click));
      }

      this.btnDefSave.IsEnabled = lib.Count != 0;
      lib.Close();
    }

    /// <summary>
    /// Does setup required for the UI when navigation occurs to a new folder
    /// </summary>
    private void SetupUIOnSelectOrNavigate() {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        this.btnDefSave.Items.Clear();
        var selItemsCount = this._ShellListView.GetSelectedCount();
        var selectedItem = this._ShellListView.GetFirstSelectedItem();

        if (selectedItem == null) {
          this.btnOpenWith.IsEnabled = false;
        } else {
          var mnu = new ShellContextMenu(this._ShellListView, false);

          try {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
              var presentationSource = PresentationSource.FromVisual(this.btnOpenWith);
              if (presentationSource != null) {
                var tempPoint = this.btnOpenWith.PointToScreen(new WIN.Point(0, 0));
                var itemMenuCount = mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)this.btnOpenWith.ActualHeight), 1, false);
                this.btnOpenWith.IsEnabled = itemMenuCount > 0 && selItemsCount == 1;
              } else {
                this.btnOpenWith.IsEnabled = false;
              }
            }));
          } catch {
            this.btnOpenWith.IsEnabled = false;
          }
        }

        this.btnNewItem.IsEnabled = this._ShellListView.CurrentFolder.IsFileSystem || this._ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName;
        if (selectedItem != null && selectedItem.IsFileSystem && BESettings.IsPreviewPaneEnabled && !selectedItem.IsFolder && selItemsCount == 1)
          this.Previewer.FileName = selectedItem.ParsingName;
        else if (!String.IsNullOrEmpty(this.Previewer.FileName))
          this.Previewer.FileName = null;

        //Set up ribbon contextual tabs on selection changed
        this.SetUpRibbonTabsVisibilityOnSelectOrNavigate(selItemsCount, selectedItem);
        this.SetUpButtonsStateOnSelectOrNavigate(selItemsCount, selectedItem);
      }));
    }

    bool IsFromSelectionOrNavigation = false;

    void cbm_ClipboardChanged(object sender, Tuple<WIN.Forms.IDataObject> e) {
      this.btnPaste.IsEnabled = e.Item1.GetDataPresent(DataFormats.FileDrop) || e.Item1.GetDataPresent("Shell IDList Array");
      this.btnPasetShC.IsEnabled = e.Item1.GetDataPresent(DataFormats.FileDrop) || e.Item1.GetDataPresent("Shell IDList Array");
    }

    #endregion

    #region Conditional Select

    private void miSelAllByType_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.GetSelectedCount() > 0) {
        var typePK = new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 4 };
        var flt = this._ShellListView.SelectedItems.Select(item => item.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant());
        var items = this._ShellListView.Items.Where(w => flt.Contains(w.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant())).ToArray();
        this._ShellListView.SelectItems(items, true);
        this.btnCondSel.IsDropDownOpen = false;
      }
    }

    private void miSelAllByDate_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.GetSelectedCount() > 0) {
        var typePK = new PROPERTYKEY() { fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 15 };
        var flt = this._ShellListView.SelectedItems.Select(item => DateTime.Parse(item.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant()).Date);
        var items = this._ShellListView.Items.Where(w => flt.Contains(DateTime.Parse(w.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant()).Date)).ToArray();
        this._ShellListView.SelectItems(items, true);
        this.btnCondSel.IsDropDownOpen = false;
      }
    }

    private void btnCondSel_Click(object sender, RoutedEventArgs e) {
      this.btnCondSel.IsDropDownOpen = false;
      ConditionalSelectForm.Open(this._ShellListView);
    }

    #endregion

    #region Size Chart

    private void btnFSizeChart_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.GetSelectedCount() > 0) {
        if ((this._ShellListView.GetFirstSelectedItem().IsFolder || this._ShellListView.GetFirstSelectedItem().IsDrive) && this._ShellListView.GetFirstSelectedItem().IsFileSystem) {
          FolderSizeWindow.Open(this._ShellListView.GetFirstSelectedItem().ParsingName, this);
          return;
        }
      } else if ((this._ShellListView.CurrentFolder.IsFolder || this._ShellListView.CurrentFolder.IsDrive) && this._ShellListView.CurrentFolder.IsFileSystem) {
        FolderSizeWindow.Open(this._ShellListView.CurrentFolder.ParsingName, this);
      }
    }

    private void btnSizeChart_Click(object sender, RoutedEventArgs e) {
      FolderSizeWindow.Open(this._ShellListView.CurrentFolder.ParsingName, this);
    }

    #endregion

    #region Home Tab

    private void btnctDocuments_Click(object sender, RoutedEventArgs e) => this.SetFOperation(KnownFolders.Documents.ParsingName, OperationType.Copy);
    private void btnctDesktop_Click(object sender, RoutedEventArgs e) => this.SetFOperation(KnownFolders.Desktop.ParsingName, OperationType.Copy);
    private void btnctDounloads_Click(object sender, RoutedEventArgs e) => this.SetFOperation(KnownFolders.Downloads.ParsingName, OperationType.Copy);
    private void btnmtDocuments_Click(object sender, RoutedEventArgs e) => this.SetFOperation(KnownFolders.Documents.ParsingName, OperationType.Move);
    private void btnmtDesktop_Click(object sender, RoutedEventArgs e) => this.SetFOperation(KnownFolders.Desktop.ParsingName, OperationType.Move);
    private void btnmtDounloads_Click(object sender, RoutedEventArgs e) => this.SetFOperation(KnownFolders.Downloads.ParsingName, OperationType.Move);
    private void btnCopyto_Click(object sender, RoutedEventArgs e) => this.btnctOther_Click(sender, e);
    private void btnMoveto_Click(object sender, RoutedEventArgs e) => this.btnmtOther_Click(sender, e);
    private void btnCut_Click(object sender, RoutedEventArgs e) => this._ShellListView.CutSelectedFiles();
    private void btnOpenWith_Click(object sender, RoutedEventArgs e) => Process.Start($"\"{this._ShellListView.GetFirstSelectedItem().ParsingName}\"");
    private void btnPaste_Click(object sender, RoutedEventArgs e) => this._ShellListView.PasteAvailableFiles();
    private void btnDelete_Click(object sender, RoutedEventArgs e) => this.MenuItem_Click(sender, e);
    private void btnRename_Click(object sender, RoutedEventArgs e) => this._ShellListView.RenameSelectedItem();
    private void btnSelAll_Click(object sender, RoutedEventArgs e) => this._ShellListView.SelectAll();
    private void btnSelNone_Click(object sender, RoutedEventArgs e) => this._ShellListView.DeSelectAllItems();
    private void MenuItem_Click(object sender, RoutedEventArgs e) => this._ShellListView.DeleteSelectedFiles(true);
    private void MenuItem_Click_1(object sender, RoutedEventArgs e) => this._ShellListView.DeleteSelectedFiles(false);
    private void btnProperties_Click(object sender, RoutedEventArgs e) => this._ShellListView.ShowPropPage(this.Handle, this._ShellListView.GetFirstSelectedItem().ParsingName, "");
    private void btnInvSel_Click(object sender, RoutedEventArgs e) => this._ShellListView.InvertSelection();
    private void btnNewWindow_Click(object sender, RoutedEventArgs e) => Process.Start(Assembly.GetExecutingAssembly().Location, "/nw");
    void miow_Click(object sender, RoutedEventArgs e) => ((AssociationItem)(sender as MenuItem).Tag).Invoke();



    private void miJunctionpoint_Click(object sender, RoutedEventArgs e) {
      string pathForDrop = this._ShellListView.CurrentFolder.ParsingName.Replace(@"\\", @"\");
      var files = new string[0];
      if (Clipboards.ContainsData("Shell IDList Array"))
        files = Clipboards.GetDataObject().ToShellItemArray().ToArray().Select(s => new ShellItem(s).ParsingName).ToArray();
      else
        files = Clipboards.GetFileDropList().OfType<string>().ToArray();

      foreach (string item in files) {
        var o = new ShellItem(item);
        JunctionPointUtils.JunctionPoint.Create($@"{pathForDrop}\{o.GetDisplayName(SIGDN.NORMALDISPLAY)}", o.ParsingName, true);
        this.AddToLog($@"Created Junction Point at {pathForDrop}\{o.GetDisplayName(SIGDN.NORMALDISPLAY)} linked to {o.ParsingName}");
      }
    }

    private void miCreateSymlink_Click(object sender, RoutedEventArgs e) {
      var items = new IListItemEx[0];
      items = Clipboards.ContainsData("Shell IDList Array") ? Clipboards.GetDataObject().ToShellItemArray().ToArray().Select(s => FileSystemListItem.InitializeWithIShellItem(this._ShellListView.LVHandle, s)).ToArray() : Clipboards.GetFileDropList().OfType<string>().ToArray().Select(s => FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, s)).ToArray();
      var pathForDrop = this._ShellListView.CurrentFolder.ParsingName.Replace(@"\\", @"\");
      var exePath = Utilities.AppDirectoryItem("BetterExplorerOperations.exe");
      var linkItems = items.Select(s => new LinkItem() {
        IsDirectory = s.IsFolder,
        DestinationData = pathForDrop + @"\" + s.DisplayName,
        SourceData = s.ParsingName
      }).ToArray();

      Task.Run(() => {
        using (var proc = new Process()) {
          proc.StartInfo = new ProcessStartInfo {
            FileName = exePath,
            Verb = "runas",
            UseShellExecute = true,
            Arguments = $"/env /user:Administrator \"{exePath}\""
          };

          proc.Start();
          ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
          var address = new EndpointAddress(new Uri("net.tcp://localhost:60000/BEComChannel"));
          var binding = new NetTcpBinding() { MaxReceivedMessageSize = 4000000, MaxBufferPoolSize = 4000000, MaxBufferSize = 4000000 };
          binding.Security = new NetTcpSecurity() { Mode = SecurityMode.Message };
          var factory = new ChannelFactory<IBetterExplorerCommunication>(binding, address);
          var beSvc = factory.CreateChannel();
          try {
            beSvc.CreateLink(new LinkData() { Items = linkItems });
          } finally {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => this._ShellListView.UnvalidateDirectory()));
          }

          proc.WaitForExit();
          if (proc.ExitCode == 1)
            MessageBox.Show("Error in creating symlink", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          else {
            Thread.Sleep(1000);
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => this._ShellListView.UnvalidateDirectory()));
          }
        }
      });

    }

    private void btnHistory_Click(object sender, RoutedEventArgs e) {
      this._ShellListView.ShowPropPage(this.Handle, this._ShellListView.GetFirstSelectedItem().ParsingName,
                                      User32.LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "twext.dll"), 1024, "Previous Versions"));
    }

    private void btnBackstageExit_Click(object sender, RoutedEventArgs e) {
      //! We call Shutdown() so to explicit shutdown the app regardless of windows closing cancel flag.
      if (this.IsMultipleWindowsOpened)
        this.Close();
      else
        Application.Current.Shutdown();
    }

    void mif_Click(object sender, RoutedEventArgs e) {
      using (var obj = (sender as MenuItem).Tag as ShellItem)
      using (var lnk = new ShellLink(obj.ParsingName)) {
        this.NavigationController(FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, lnk.TargetPIDL));
      }
    }

    private void btnCopy_Click(object sender, RoutedEventArgs e) {
      var sc = new StringCollection();
      sc.AddRange(this._ShellListView.SelectedItems.Select(x => x.ParsingName).ToArray());
      Clipboards.SetFileDropList(sc);
    }


    private void btnPathCopy_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.SelectedItems.Count > 1)
        Clipboards.SetText(this._ShellListView.SelectedItems.Select(item => "\r\n" + item.ParsingName).Aggregate((x, y) => x + y).Trim());
      else if (this._ShellListView.SelectedItems.Count == 1)
        Clipboards.SetText(this._ShellListView.GetFirstSelectedItem().ParsingName);
      else
        Clipboards.SetText(this._ShellListView.CurrentFolder.ParsingName);
    }


    // New Folder/Library
    private void btnNewFolder_Library(object sender, RoutedEventArgs e) {
      //We should focus the ListView or on some circumstances new folder does not start renaming after folder is created
      this._ShellListView.Focus();
      this._ShellListView.IsRenameNeeded = true;

      if (this._ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName)
        this._ShellListView.CreateNewLibrary(this.FindResource("btnNewLibraryCP").ToString());
      else
        this._ShellListView.CreateNewFolder(null);
    }

    private void btnPasetShC_Click(object sender, RoutedEventArgs e) {
      string PathForDrop = this._ShellListView.CurrentFolder.ParsingName;
      foreach (string item in Clipboards.GetFileDropList()) {
        using (var shortcut = new ShellLink()) {
          var o = new ShellItem(item);
          shortcut.Target = item;
          shortcut.WorkingDirectory = Path.GetDirectoryName(item);
          shortcut.Description = o.GetDisplayName(SIGDN.NORMALDISPLAY);
          shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
          shortcut.Save($"{PathForDrop}\\{o.GetDisplayName(SIGDN.NORMALDISPLAY)}.lnk");
          this.AddToLog($"Shortcut created at {PathForDrop}\\{o.GetDisplayName(SIGDN.NORMALDISPLAY)} from source {item}");
        }
      }
    }

    private void btnmtOther_Click(object sender, RoutedEventArgs e) {
      var dlg = new FolderSelectDialog();
      if (dlg.ShowDialog())
        this.SetFOperation(dlg.FileName, OperationType.Move);
    }

    private void SetFOperation(String fileName, OperationType opType) {
      var obj = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, fileName.ToShellParsingName());
      if (opType == OperationType.Copy)
        this._ShellListView.DoCopy(obj);
      else if (opType == OperationType.Move)
        this._ShellListView.DoMove(obj);
    }

    private void SetFOperation(IListItemEx obj, OperationType opType) {
      if (opType == OperationType.Copy)
        this._ShellListView.DoCopy(obj);
      else if (opType == OperationType.Move)
        this._ShellListView.DoMove(obj);
    }

    private void btnctOther_Click(object sender, RoutedEventArgs e) {
      var dlg = new FolderSelectDialog();
      if (dlg.ShowDialog())
        this.SetFOperation(dlg.FileName, OperationType.Copy);

      this._ShellListView.Focus();
    }

    private void btnNewItem_Click(object sender, RoutedEventArgs e) {
      var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = 0 };
      BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS, true);
    }

    private void btnEdit_Click(object sender, RoutedEventArgs e) {
      new Process() {
        StartInfo = new ProcessStartInfo {
          FileName = this._ShellListView.GetFirstSelectedItem().ParsingName,
          Verb = "edit",
          UseShellExecute = true,
        }
      }.Start();
    }

    private void btnFavorites_Click(object sender, RoutedEventArgs e) {
      var selectedItems = this._ShellListView.SelectedItems;
      if (selectedItems.Count == 1) {
        var link = new ShellLink();
        link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
        link.Target = this._ShellListView.GetFirstSelectedItem().ParsingName;
        link.Save($@"{KnownFolders.Links.ParsingName}\{this._ShellListView.GetFirstSelectedItem().DisplayName}.lnk");
        link.Dispose();
      }

      if (selectedItems.Count == 0) {
        var link = new ShellLink();
        link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
        link.Target = this._ShellListView.CurrentFolder.ParsingName;
        link.Save($@"{KnownFolders.Links.ParsingName}\{this._ShellListView.CurrentFolder.DisplayName}.lnk");
        link.Dispose();
      }
    }

    #endregion

    #region Drive Tools / Virtual Disk Tools

    private void btnDefragDrive_Click(object sender, RoutedEventArgs e) {
      string DriveLetter = "";

      if (!this._ShellListView.SelectedItems.Any())
        DriveLetter = this._ShellListView.CurrentFolder.ParsingName;
      else if (Directory.GetLogicalDrives().Contains(this._ShellListView.SelectedItems[0].ParsingName))
        DriveLetter = this._ShellListView.SelectedItems[0].ParsingName;
      else
        DriveLetter = this._ShellListView.CurrentFolder.ParsingName;

      Process.Start(Path.Combine(Environment.SystemDirectory, "dfrgui.exe"), $"/u /v {DriveLetter.Replace("\\", "")}");
    }

    private char GetDriveLetterFromDrivePath(string path) => path.Substring(0, 1).ToCharArray()[0];

    private void btnFormatDrive_Click(object sender, RoutedEventArgs e) {
      if (MessageBox.Show("Are you sure you want to do this?", this.FindResource("btnFormatDriveCP").ToString(), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
        var formatDriveThread = new Thread(() => {
          string DriveLetter =
          this._ShellListView.SelectedItems.Any() ?
          DriveLetter = Directory.GetLogicalDrives().Contains(this._ShellListView.SelectedItems[0].ParsingName) ? this._ShellListView.SelectedItems[0].ParsingName : this._ShellListView.CurrentFolder.ParsingName
          :
          DriveLetter = this._ShellListView.CurrentFolder.ParsingName;

          BExplorer.Shell.Interop.Shell32.FormatDrive(IntPtr.Zero, DriveLetter);
        });

        formatDriveThread.Start();
      }
    }

    private void btnCleanDrive_Click(object sender, RoutedEventArgs e) {
      string DriveLetter = "";
      if (this._ShellListView.SelectedItems.Any())
        DriveLetter = Directory.GetLogicalDrives().Contains(this._ShellListView.SelectedItems[0].ParsingName) ? this._ShellListView.SelectedItems[0].ParsingName : this._ShellListView.CurrentFolder.ParsingName;
      else
        DriveLetter = this._ShellListView.CurrentFolder.ParsingName;

      Process.Start("Cleanmgr.exe", "/d" + DriveLetter.Replace(":\\", ""));
    }

    private void OpenCDTray(char DriveLetter) {
      mciSendString($"open {DriveLetter}: type CDAudio alias drive{DriveLetter}", null, 0, IntPtr.Zero);
      mciSendString($"set drive{DriveLetter} door open", null, 0, IntPtr.Zero);
    }

    private void CloseCDTray(char DriveLetter) {
      mciSendString($"open {DriveLetter}: type CDAudio alias drive{DriveLetter}", null, 0, IntPtr.Zero);
      mciSendString($"set drive{DriveLetter} door closed", null, 0, IntPtr.Zero);
    }

    private void btnOpenTray_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.GetFirstSelectedItem()?.GetDriveInfo().DriveType == DriveType.CDRom)
        this.OpenCDTray(this.GetDriveLetterFromDrivePath(this._ShellListView.GetFirstSelectedItem().ParsingName));
    }

    private void btnCloseTray_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.GetFirstSelectedItem()?.GetDriveInfo().DriveType == DriveType.CDRom)
        this.CloseCDTray(this.GetDriveLetterFromDrivePath(this._ShellListView.GetFirstSelectedItem().ParsingName));
    }

    private void EjectDisk(char DriveLetter) {
      Thread t = new Thread(() => {
        Thread.Sleep(10);
        var vdc = new VolumeDeviceClass();
        foreach (Volume item in vdc.Devices) {
          if (this.GetDriveLetterFromDrivePath(item.LogicalDrive) == DriveLetter) {
            var veto = item.Eject(false);
            if (veto != Native.PNP_VETO_TYPE.TypeUnknown) {
              if (veto == Native.PNP_VETO_TYPE.Ok) {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (Action)(() => {
                                  this.beNotifyIcon.ShowBalloonTip("Information", $"It is safe to remove {item.LogicalDrive}", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                                  var tabsForRemove = this.tcMain.Items.OfType<Wpf.Controls.TabItem>().Where(w => {
                                    var root = String.Empty;
                                    try {
                                      root = Path.GetPathRoot(w.ShellObject.ParsingName.ToShellParsingName());
                                    } catch { }
                                    return !String.IsNullOrEmpty(root) && (w.ShellObject.IsFileSystem &&
                                                    root.ToLowerInvariant() == $"{DriveLetter}:\\".ToLowerInvariant());
                                  }).ToArray();

                                  foreach (Wpf.Controls.TabItem tab in tabsForRemove) {
                                    this.tcMain.RemoveTabItem(tab);
                                  }
                                }));
              } else {
                var message = String.Empty;
                var obj = new ShellItem(item.LogicalDrive);
                switch (veto) {
                  case Native.PNP_VETO_TYPE.Ok:
                    break;
                  case Native.PNP_VETO_TYPE.TypeUnknown:
                    break;
                  case Native.PNP_VETO_TYPE.LegacyDevice:
                    break;
                  case Native.PNP_VETO_TYPE.PendingClose:
                    break;
                  case Native.PNP_VETO_TYPE.WindowsApp:
                    break;
                  case Native.PNP_VETO_TYPE.WindowsService:
                    break;
                  case Native.PNP_VETO_TYPE.OutstandingOpen:
                    message = $"The device {obj.GetDisplayName(SIGDN.NORMALDISPLAY)} can not be disconnected because is in use!";
                    break;
                  case Native.PNP_VETO_TYPE.Device:
                    break;
                  case Native.PNP_VETO_TYPE.Driver:
                    break;
                  case Native.PNP_VETO_TYPE.IllegalDeviceRequest:
                    break;
                  case Native.PNP_VETO_TYPE.InsufficientPower:
                    break;
                  case Native.PNP_VETO_TYPE.NonDisableable:
                    message = $"The device {obj.GetDisplayName(SIGDN.NORMALDISPLAY)} does not support disconnecting!";
                    break;
                  case Native.PNP_VETO_TYPE.LegacyDriver:
                    break;
                  default:
                    break;
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => this.beNotifyIcon.ShowBalloonTip("Error", message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error)));
              }
            }
            break;
          }
        }
      });
      t.SetApartmentState(ApartmentState.STA);
      t.Start();
    }

    private void btnEjectDevice_Click(object sender, RoutedEventArgs e) {
      var firstSelectedItem = this._ShellListView.GetFirstSelectedItem();
      if (firstSelectedItem?.GetDriveInfo().DriveType == DriveType.Removable || firstSelectedItem.GetDriveInfo().DriveType == DriveType.Fixed) {
        this.EjectDisk(this.GetDriveLetterFromDrivePath(firstSelectedItem.ParsingName));
        //USBEject.EjectDrive(GetDriveLetterFromDrivePath(firstSelectedItem.ParsingName));
      }
    }

    // Virtual Disk Tools
    private bool CheckImDiskInstalled() {
      try {
        ImDiskAPI.GetDeviceList();
        return true;
      } catch (DllNotFoundException) {
        return false;
      }
    }

    public void ShowInstallImDiskMessage() {
      if (MessageBox.Show("It appears you do not have the ImDisk Virtual Disk Driver installed. This driver is used to power Better Explorer's ISO-mounting features. \n\nWould you like to visit ImDisk's website to install the product? (http://www.ltr-data.se/opencode.html/)", "ImDisk Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
        Process.Start("http://www.ltr-data.se/opencode.html/#ImDisk");
      }
    }

    private void btnAdvMountIso_Click(object sender, RoutedEventArgs e) {
      if (!this.CheckImDiskInstalled()) {
        this.ShowInstallImDiskMessage();
        return;
      }

      var mi = new MountIso() { Owner = this };
      mi.ShowDialog();
      if (mi.yep) {
        string DriveLetter = String.Format("{0}:", mi.chkPreselect.IsChecked == true ? ImDiskAPI.FindFreeDriveLetter() : (char)mi.cbbLetter.SelectedItem);
        long size = mi.chkPresized.IsChecked == true ? 0 : Convert.ToInt64(mi.txtSize.Text);

        ImDiskFlags imflags;
        switch (mi.cbbType.SelectedIndex) {
          case 0:
            //Hard Drive
            imflags = ImDiskFlags.DeviceTypeHD;
            break;
          case 1:
            // CD/DVD
            imflags = ImDiskFlags.DeviceTypeCD;
            break;
          case 2:
            // Floppy Disk
            imflags = ImDiskFlags.DeviceTypeFD;
            break;
          case 3:
            // Raw Data
            imflags = ImDiskFlags.DeviceTypeRAW;
            break;
          default:
            imflags = ImDiskFlags.DeviceTypeCD;
            break;
        }

        switch (mi.cbbAccess.SelectedIndex) {
          case 0:
            // Access directly
            imflags |= ImDiskFlags.FileTypeDirect;
            break;
          case 1:
            // Copy to memory
            imflags |= ImDiskFlags.FileTypeAwe;
            break;
          default:
            imflags |= ImDiskFlags.FileTypeDirect;
            break;
        }

        if (mi.chkRemovable.IsChecked == true)
          imflags |= ImDiskFlags.Removable;
        if (mi.chkReadOnly.IsChecked == true)
          imflags |= ImDiskFlags.ReadOnly;

        ImDiskAPI.CreateDevice(size, 0, 0, 0, 0, imflags, this._ShellListView.GetFirstSelectedItem().ParsingName, false, DriveLetter, IntPtr.Zero);
      }
    }

    private void btnMountIso_Click(object sender, RoutedEventArgs e) {
      try {
        var freeDriveLetter = $"{ImDiskAPI.FindFreeDriveLetter()}:";
        ImDiskAPI.CreateDevice(0, 0, 0, 0, 0, ImDiskFlags.Auto, this._ShellListView.GetFirstSelectedItem().ParsingName, false, freeDriveLetter, IntPtr.Zero);
      } catch (DllNotFoundException) {
        this.ShowInstallImDiskMessage();
      } catch (Exception ex) {
        MessageBox.Show("An error occurred while trying to mount this file. \n\n" + ex.Message, ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void btnWriteIso_Click(object sender, RoutedEventArgs e) {
      Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "isoburn.exe"), $"\"{this._ShellListView.GetFirstSelectedItem().ParsingName}\"");
    }

    private void btnUnmountDrive_Click(object sender, RoutedEventArgs e) {
      //SelectedDriveID was NEVER anything but 0
      uint SelectedDriveID = 0;

      try {
        if (!this.CheckImDiskInstalled())
          this.ShowInstallImDiskMessage();
        else if ((ImDiskAPI.QueryDevice(SelectedDriveID).Flags & ImDiskFlags.DeviceTypeCD) != 0)
          ImDiskAPI.ForceRemoveDevice(SelectedDriveID);
        else
          ImDiskAPI.RemoveDevice(SelectedDriveID);
      } catch {
        if (MessageBox.Show("The drive could not be removed. Would you like to try to force a removal?", "Remove Drive Failed", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
          ImDiskAPI.ForceRemoveDevice(SelectedDriveID);
        }
      }
      this._ShellListView.RefreshContents();
      this.ctgDrive.Visibility = Visibility.Collapsed;
    }

    #endregion

    #region Application Tools

    private void btnPin_Click(object sender, RoutedEventArgs e) => User32.PinUnpinToTaskbar(this._ShellListView.GetFirstSelectedItem().ParsingName);
    private void btnPinToStart_Click(object sender, RoutedEventArgs e) => User32.PinUnpinToStartMenu(this._ShellListView.GetFirstSelectedItem().ParsingName);
    private void btnRunAs_Click(object sender, RoutedEventArgs e) => CredUI.RunProcesssAsUser(this._ShellListView.GetFirstSelectedItem().ParsingName);

    private void btnRunAsAdmin_Click(object sender, RoutedEventArgs e) {
      var FileName = this._ShellListView.GetFirstSelectedItem().ParsingName;
      Process.Start(new ProcessStartInfo {
        FileName = FileName,
        Verb = "runas",
        UseShellExecute = true,
        Arguments = $"/env /user:Administrator \"{FileName}\""
      });
    }

    #endregion

    #region Backstage - Information Tab

    private void Button_Click_6(object sender, RoutedEventArgs e) {
      this.backstage.IsOpen = true;
      this.autoUpdater.Visibility = WIN.Visibility.Visible;
      this.autoUpdater.UpdateLayout();

      switch (this.autoUpdater.UpdateStepOn) {
        case UpdateStepOn.Checking:
        case UpdateStepOn.DownloadingUpdate:
        case UpdateStepOn.ExtractingUpdate:
          this.autoUpdater.Cancel();
          break;
        case UpdateStepOn.UpdateReadyToInstall:
        case UpdateStepOn.UpdateAvailable:
          break;
        case UpdateStepOn.UpdateDownloaded:
          this.autoUpdater.InstallNow();
          break;
        default:
          this.autoUpdater.ForceCheckForUpdate(true);
          break;
      }
    }

    private void Button_Click_7(object sender, RoutedEventArgs e) => Process.Start("http://gainedge.org/better-explorer/");

    #endregion

    #region Path to String HelperFunctions / Other HelperFunctions

    private Visibility BooleanToVisibiliy(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    private void AddToLog(string value) {
      try {
        if (BESettings.EnableActionLog) {
          if (!Directory.Exists(this.logdir))
            Directory.CreateDirectory(this.logdir);

          using (var sw = new StreamWriter($"{this.logdir}{this.sessionid}.txt", true)) {
            sw.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " : " + value);
          }
        }
      } catch (Exception exe) {
        MessageBox.Show("An error occurred while writing to the log file. This error can be avoided if you disable the action logging feature. Please report this issue at http://bugs.gainedge.org/public/betterexplorer. \r\n\r\n Here is some information about the error: \r\n\r\n" + exe.Message + "\r\n\r\n" + exe.ToString(), "Error While Writing to Log", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    #endregion

    #region Updater

    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
      if (this.isOnLoad)
        return;

      Settings.BESettings.IsUpdateCheck = true;
      this.updateCheckTimer.Start();
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
      if (this.isOnLoad)
        return;

      Settings.BESettings.IsUpdateCheck = false;
    }

    private void rbCheckInterval_Click(object sender, RoutedEventArgs e) {
      if (this.rbDaily.IsChecked.Value)
        Settings.BESettings.UpdateCheckInterval = 1;
      else if (this.rbMonthly.IsChecked.Value)
        Settings.BESettings.UpdateCheckInterval = 30;
      else
        Settings.BESettings.UpdateCheckInterval = 7;
    }

    private void chkUpdateStartupCheck_Click(object sender, RoutedEventArgs e) {
      Settings.BESettings.IsUpdateCheckStartup = this.chkUpdateStartupCheck.IsChecked.Value;
    }

    private void UpdateTypeCheck_Click(object sender, RoutedEventArgs e) {
      Settings.BESettings.UpdateCheckType = this.rbReleases.IsChecked.Value ? 0 : 1;
    }

    #endregion

    #region On Startup

    /// <summary>
    /// Gets the badges from the folder Badges located in the .EXE's directory and the badges from SQLite database
    /// </summary>
    /// <returns></returns>
    private Dictionary<String, Dictionary<IListItemEx, List<string>>> LoadBadgesData() {
      var result = new Dictionary<String, Dictionary<IListItemEx, List<string>>>();
      var badgesDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Badges");
      var badgesIshellItem = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, badgesDirectory);
      foreach (var item in badgesIshellItem.Where(w => w.IsFolder)) {
        var innerDict = new Dictionary<IListItemEx, List<string>>();
        foreach (var badgeItem in item.Where(w => w.Extension.ToLowerInvariant() == ".ico")) {
          innerDict.Add(badgeItem, new List<String>());
        }

        result.Add(item.DisplayName, innerDict);
      }

      try {
        var m_dbConnection = new System.Data.SQLite.SQLiteConnection($"Data Source={this._DBPath};Version=3;");
        m_dbConnection.Open();

        var command1 = new System.Data.SQLite.SQLiteCommand("SELECT * FROM badges", m_dbConnection);

        var Reader = command1.ExecuteReader();
        while (Reader.Read()) {
          var values = Reader.GetValues();
          var path = values.GetValues("Path").Single();
          var collectionName = values.GetValues("Collection").Single();
          var badgeName = values.GetValues("Badge").Single();
          var badgeDBItem = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, Path.Combine(badgesDirectory, collectionName, badgeName));
          var collectionDict = result[collectionName];
          var collectionItemKey = collectionDict.Keys.SingleOrDefault(w => w.ParsingName.Equals(badgeDBItem.ParsingName));

          if (collectionItemKey != null) {
            result[collectionName][collectionItemKey].Add(path);
          }
        }
        Reader.Close();
      } catch (Exception) {
      }

      return result;
    }

    /// <summary>
    /// Adds all default items to <see cref="btnFavorites"/>
    /// </summary>
    /// <remarks>
    /// 1. Sets OpenFavorites's OnClick event to open <see cref="KnownFolders.Links"/>
    /// 2. Adds all links from <see cref="KnownFolders.Links"/> that are not hidden 
    /// </remarks>
    private void SetUpFavoritesMenu() {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
        this.btnFavorites.Visibility = Visibility.Visible;
        var favoritesItem = BExplorer.Shell.Utilities.WindowsVersion == WindowsVersions.Windows10
        ? FileSystemListItem.ToFileSystemItem(IntPtr.Zero, "shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}")
        : FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Links).Pidl);

        var openFavorites = new MenuItem() { Header = "Open Favorites" };
        openFavorites.Click += (x, y) => this.tcMain.NewTab(favoritesItem, true);

        this.btnFavorites.Items.Add(openFavorites);
        this.btnFavorites.Items.Add(new Separator());

        foreach (var item in favoritesItem.Where(w => !w.IsHidden && (w.IsFolder || w.IsLink)).OrderBy(o => o.DisplayName)) {
          if (item.IsLink) {
            var link = new ShellLink(item.ParsingName);
            var targetItem = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, link.TargetPIDL);
            if (targetItem.IsFolder) {
              this.btnFavorites.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default), onClick: this.mif_Click));
            }

            targetItem.Dispose();
            link.Dispose();
          } else {
            this.btnFavorites.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default),
              onClick: this.mif_Click));
          }
        }
      }));
    }

    private void InitializeExplorerControl() {
      //this.ShellTree.NodeClick += this.ShellTree_NodeClick;
      this._ShellListView.Navigated += this.ShellListView_Navigated;
      this._ShellListView.ViewStyleChanged += this.ShellListView_ViewStyleChanged;
      this._ShellListView.SelectionChanged += this.ShellListView_SelectionChanged;
      this._ShellListView.LVItemsColorCodes = this.LVItemsColorCol;
      this._ShellListView.ItemUpdated += this.ShellListView_ItemUpdated;
      this._ShellListView.ColumnHeaderRightClick += this.ShellListView_ColumnHeaderRightClick;
      this._ShellListView.KeyJumpKeyDown += this.ShellListView_KeyJumpKeyDown;
      this._ShellListView.KeyJumpTimerDone += this.ShellListView_KeyJumpTimerDone;
      //this.ShellListView.ItemDisplayed += ShellListView_ItemDisplayed;
      this._ShellListView.OnListViewColumnDropDownClicked += this.ShellListView_OnListViewColumnDropDownClicked;
      this._ShellListView.Navigating += this.ShellListView_Navigating;
      this._ShellListView.ItemMiddleClick += (sender, e) => this.tcMain.NewTab(e.Folder, false);
      this._ShellListView.BeginItemLabelEdit += this.ShellListView_BeginItemLabelEdit;
      this._ShellListView.EndItemLabelEdit += this.ShellListView_EndItemLabelEdit;
      this._ShellListView.OnListViewCollumnsChanged += this._ShellListView_OnListViewCollumnsChanged;
      //this._ShellListView.OnLVScroll += (sender, args) => {
      //  if (this._IsScrollingManually) {
      //    this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() => {
      //      if (args.ScrollInfo.nMax == 0 || args.ScrollInfo.nPage > args.ScrollInfo.nMax) {
      //        this.sbLVVertical.Visibility = Visibility.Collapsed;
      //      } else {

      //        this.sbLVVertical.Visibility = Visibility.Visible;
      //        //this.sbLVVertical.Minimum = args.ScrollInfo.nMin;
      //        //this.sbLVVertical.Maximum = args.ScrollInfo.nMax - args.ScrollInfo.nPage;
      //        //this.sbLVVertical.ViewportSize = this._ShellListView.View == ShellViewStyle.Details ? ((args.ScrollInfo.nPage * 16) * 0.01) : args.ScrollInfo.nPage - 20;
      //        //this.sbLVVertical.LargeChange = this._ShellListView.View == ShellViewStyle.Details ? (int)args.ScrollInfo.nPage : 4 * (this._ShellListView.IconSize + 50);
      //        //this.sbLVVertical.SmallChange = this._ShellListView.View == ShellViewStyle.Details ? 1 : 40;
      //        //this._ScrollOldValue = args.ScrollInfo.nPos;
      //        //this.sbLVVertical.Value = args.ScrollInfo.nPos;
      //      }

      //      this._IsScrollingManually = false;
      //    }));
      //    return;
      //  }

      //  var detailsItemHeight = 16;
      //  this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() => {

      //    if (args.ScrollInfo.nMax == 0 || args.ScrollInfo.nPage > args.ScrollInfo.nMax) {
      //      this.sbLVVertical.Visibility = Visibility.Collapsed;
      //    } else {

      //      this.sbLVVertical.Visibility = Visibility.Visible;
      //      this.sbLVVertical.Minimum = 0;
      //      this.sbLVVertical.Maximum = (args.ScrollInfo.nMax - args.ScrollInfo.nPage + 1);
      //      this.sbLVVertical.ViewportSize = this._ShellListView.View == ShellViewStyle.Details ? (args.ScrollInfo.nPage) : args.ScrollInfo.nPage;
      //      this.sbLVVertical.LargeChange = this._ShellListView.View == ShellViewStyle.Details ? (int)args.ScrollInfo.nPage : 4 * (this._ShellListView.IconSize + 50);
      //      this.sbLVVertical.SmallChange = this._ShellListView.View == ShellViewStyle.Details ? 1 : this._ShellListView.IconSize + 50;
      //      this.sbLVVertical.Value = args.ScrollInfo.nPos;
      //      this._ScrollOldValue = this.sbLVVertical.Value;
      //    }

      //  }));
      //};
      //this._ShellListView.AfterCollumsPopulate += (sender, args) => {
      //  if (args.Collumn != null) {
      //    this.pnlShellViewControl.AddHeaderColumn(args.Collumn);
      //  } else if (args.Collumns != null) {
      //    this.pnlShellViewControl.AddHeaderColumns(args.Collumns);
      //  } else {
      //    this.pnlShellViewControl.ClearHeaderColumns();
      //  }
      //};
      this._ShellListView.BadgesData = this.Badges;
    }

    private void _ShellListView_OnListViewCollumnsChanged(object sender, CollumnsChangedArgs e) => this.SetSortingAndGroupingButtons();

    void ShellListView_OnListViewColumnDropDownClicked(object sender, ListViewColumnDropDownArgs e) {
      //TODO: Add Events for when an item's check has been changed
      var packUri = "pack://application:,,,/BetterExplorer;component/Images/stack16.png";
      var menu = new ListviewColumnDropDown() {
        Placement = PlacementMode.AbsolutePoint,
        HorizontalOffset = e.ActionPoint.X,
        VerticalOffset = e.ActionPoint.Y,
        IsOpen = true,
        StaysOpen = true,
      };

      var Things = new List<string>();
      var SelectedColumn = this._ShellListView.Collumns[e.ColumnIndex];
      if (SelectedColumn.CollumnType == typeof(String)) {
        Things.AddRange(new[] { "0 - 9", "A - H", "I - P", "Q - Z", "Other" });
      } else if (SelectedColumn.CollumnType == typeof(DateTime)) {
        var Container = new ItemsControl();
        Container.Items.Add(new MenuItem() { Icon = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource, Header = "Select a date or date range:", HorizontalContentAlignment = HorizontalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch, IsCheckable = true, StaysOpenOnClick = true });
        Container.Items.Add(new Calendar() { SelectionMode = CalendarSelectionMode.SingleRange, Margin = new Thickness(30, 0, 0, 0) });
        menu.AddItem(Container);

        Things.AddRange(new[] { "A long time ago", "Earlier this year", "Earlier this month", "Last week", "Today" });
      } else if (SelectedColumn.CollumnType == typeof(long)) {
        Things.AddRange(new[] { "Tiny (0 - 10 KB)", "Small (10 - 100 KB)", "Medium (100 KB - 1 MB)", "Large (1 - 16 MB)", "Huge (16 - 128 MB)", "Unspecified" });
      } else if (SelectedColumn.CollumnType == typeof(Type)) {
        var distictItems = this._ShellListView.Items.Select(s => s.GetPropertyValue(SelectedColumn.pkey, SelectedColumn.CollumnType).Value).Distinct().Cast<String>().ToArray().OrderBy(o => o);
        Things.AddRange(distictItems);
      }

      foreach (var item in Things) {
        var mnuItem = new MenuItem() {
          Icon = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource,
          IsCheckable = true,
          Header = item,
          HorizontalContentAlignment = HorizontalAlignment.Stretch,
          HorizontalAlignment = HorizontalAlignment.Stretch,
          StaysOpenOnClick = true
        };

        mnuItem.Click += new RoutedEventHandler(delegate (object s, RoutedEventArgs re) {
          var over = Mouse.DirectlyOver;
          if (!(over is Image)) {
            menu.IsOpen = false;
          }
        });
        menu.AddItem(mnuItem);
        menu.Closed += (o, args) => {
          var rect = default(User32.RECT);

          if (User32.SendMessage(this._ShellListView.LVHeaderHandle, 0x1200 + 7, e.ColumnIndex, ref rect) == IntPtr.Zero) {
            throw new Win32Exception();
          }

          User32.InvalidateRect(this._ShellListView.LVHeaderHandle, ref rect, false);
        };
      }
    }

    protected override void OnSourceInitialized(EventArgs e) {
      base.OnSourceInitialized(e);
      this.Handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
    }

    private void LoadRegistryRelatedSettings() {
      RegistryKey rk = Registry.CurrentUser;

      switch (BESettings.CurrentTheme) {
        case "Dark":
          this.btnTheme.IsChecked = true;
          break;
      }

      switch (Settings.BESettings.UpdateCheckInterval) {
        case 1:
          this.rbDaily.IsChecked = true;
          break;
        case 7:
          this.rbWeekly.IsChecked = true;
          break;
        case 30:
          this.rbMonthly.IsChecked = true;
          break;
      }

      switch (Settings.BESettings.UpdateCheckType) {
        case 0:
          this.rbReleases.IsChecked = true;
          break;
        case 1:
          this.rbReleasTest.IsChecked = true;
          break;
      }

      this.chkUpdateCheck.IsChecked = Settings.BESettings.IsUpdateCheck;
      this.chkUpdateStartupCheck.IsChecked = Settings.BESettings.IsUpdateCheckStartup;

      this.btnConsolePane.IsChecked = BESettings.IsConsoleShown;
      this.chkIsFlyout.IsChecked = BESettings.HFlyoutEnabled;//
      this.btnInfoPane.IsChecked = BESettings.IsInfoPaneEnabled;

      if (BESettings.IsInfoPaneEnabled) {
        this.rPreviewPane.Height = new GridLength(BESettings.InfoPaneHeight);
        this.rPreviewPaneSplitter.Height = new GridLength(1);
      } else {
        this.rPreviewPane.Height = new GridLength(0);
        this.rPreviewPaneSplitter.Height = new GridLength(0);
      }

      this.btnPreviewPane.IsChecked = BESettings.IsPreviewPaneEnabled;

      if (BESettings.IsPreviewPaneEnabled) {
        this.clPreview.Width = new GridLength(BESettings.PreviewPaneWidth);
        this.clPreviewSplitter.Width = new GridLength(1);
      } else {
        this.clPreview.Width = new GridLength(0);
        this.clPreviewSplitter.Width = new GridLength(0);
      }

      this.btnNavigationPane.IsChecked = BESettings.IsNavigationPaneEnabled;

      this._ShellListView.ShowCheckboxes = BESettings.ShowCheckboxes;
      this.chkShowCheckBoxes.IsChecked = this._ShellListView.ShowCheckboxes;
      this.chkIsTerraCopyEnabled.IsChecked = BESettings.IsFileOpExEnabled;

      this.chkIsCFO.IsChecked = BESettings.IsCustomFO;
      this.chkIsRestoreTabs.IsChecked = BESettings.IsRestoreTabs;
      this.chkTraditionalNameGrouping.IsChecked = BESettings.IsTraditionalNameGrouping;

      // if this instance has the /norestore switch, do not load tabs from previous session, even if it is set in the Registry
      if (App.IsStartWithStartupTab) {
        BESettings.IsRestoreTabs = false;
      }

      //this.chkIsLastTabCloseApp.IsChecked = (int)rks.GetValue("IsLastTabCloseApp", 1) == 1;

      this.chkLogHistory.IsChecked = BESettings.EnableActionLog;
      if (BESettings.EnableActionLog) {
        this.chkLogHistory.Visibility = Visibility.Visible;
        this.ShowLogsBorder.Visibility = Visibility.Visible;
        this.paddinglbl8.Visibility = Visibility.Visible;
      }


      this.chkFolder.IsChecked = BESettings.AutoSwitchFolderTools;
      this.chkArchive.IsChecked = BESettings.AutoSwitchArchiveTools;
      this.chkImage.IsChecked = BESettings.AutoSwitchImageTools;
      this.chkApp.IsChecked = BESettings.AutoSwitchApplicationTools;
      this.chkLibrary.IsChecked = BESettings.AutoSwitchLibraryTools;
      this.chkDrive.IsChecked = BESettings.AutoSwitchDriveTools;
      this.chkVirtualTools.IsChecked = BESettings.AutoSwitchVirtualDriveTools;

      // load OverwriteOnImages setting (default is false)
      this.chkOverwriteImages.IsChecked = BESettings.OverwriteImageWhileEditing;

      // load Saved Tabs Directory location (if different from default)
      this.txtDefSaveTabs.Text = BESettings.SavedTabsDirectory;

      if (this.tcMain.StartUpLocation == "") {
        this.tcMain.StartUpLocation = BESettings.StartupLocation;
      }

      try {
        var rkbe = Registry.ClassesRoot;
        var rksbe = rkbe.OpenSubKey(@"Folder\shell\open\command", RegistryKeyPermissionCheck.ReadSubTree);
        this.chkIsDefault.IsChecked = rksbe.GetValue("DelegateExecute", "-1").ToString() == "-1";
        this.chkIsDefault.IsEnabled = true;
        rksbe.Close();
        rkbe.Close();
      } catch (Exception) {
        this.chkIsDefault.IsChecked = false;
        this.chkIsDefault.IsEnabled = false;
      }

      var rksfe = rk.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", RegistryKeyPermissionCheck.ReadSubTree);
      this.chkTreeExpand.IsChecked = (int)rksfe.GetValue("NavPaneExpandToCurrentFolder", 0) == 1;
      rksfe.Close();
      rk.Close();


      var sho = new ShellItem(BESettings.StartupLocation.ToShellParsingName());
      this.btnSetCurrentasStartup.Header = sho.DisplayName;
      sho.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
      this.btnSetCurrentasStartup.Icon = sho.Thumbnail.SmallBitmapSource;


      this.miTabManager.IsEnabled = Directory.Exists(BESettings.SavedTabsDirectory);
      this.autoUpdater.DaysBetweenChecks = Settings.BESettings.UpdateCheckInterval;

      try {
        this.autoUpdater.UpdateType = Settings.BESettings.IsUpdateCheck ? UpdateType.OnlyCheck : UpdateType.DoNothing;
        if (Settings.BESettings.IsUpdateCheckStartup)
          this.CheckForUpdate();
      } catch (IOException) {
        this.stiUpdate.Content = "Switch to another BetterExplorer window or restart to check for updates.";
        this.btnUpdateCheck.IsEnabled = false;
      }

      if (App.IsStartMinimized) {
        this.Visibility = Visibility.Hidden;
        this.WindowState = WindowState.Minimized;
      }

      if (!this.TheRibbon.IsMinimized) {
        this.TheRibbon.SelectedTabItem = this.HomeTab;
        this.TheRibbon.SelectedTabIndex = 0;
      }
    }

    private void SetsUpJumpList() {
      //sets up Jump List
      try {
        this.AppJL.ShowRecentCategory = true;
        this.AppJL.ShowFrequentCategory = true;
        JumpList.SetJumpList(Application.Current, this.AppJL);
        this.AppJL.JumpItems.Add(new JumpTask() {
          ApplicationPath = Process.GetCurrentProcess().MainModule.FileName,
          Arguments = "t",
          Title = "Open Tab",
          Description = "Opens new tab with default location"
        });
        this.AppJL.JumpItems.Add(new JumpTask() {
          ApplicationPath = Process.GetCurrentProcess().MainModule.FileName,
          Arguments = "/nw",
          Title = "New Window",
          Description = "Creates a new window with default location"
        });

        this.AppJL.Apply();
      } catch {
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) {
      this.sbLVVertical.Track.Thumb.DragDelta += (o, args) => {
        this._IsScrollingManually = true;
        User32.SendMessage(this._ShellListView.LVHandle, 0x1000 + 20, 0, (Int32)Math.Ceiling(args.VerticalChange * (8.5)));
        //args.Handled = true;
        //this.sbLVVertical.Value = this.sbLVVertical.Value + args.VerticalChange;
      };
      this._ScrollDeferer.Interval = 100;
      this._ScrollDeferer.Stop();
      this._ScrollDeferer.Tick += (o, args) => {
        this._ScrollDeferer.Stop();

        this._IsScrollingManually = true;
        var delta = this.sbLVVertical.Value - this._ScrollOldValue;

        //User32.LockWindowUpdate(this._ShellListView.LVHandle);
        if (Math.Abs(delta) > 0) {
          //User32.LockWindowUpdate(this._ShellListView.LVHandle);
          User32.SendMessage(this._ShellListView.LVHandle, 0x1000 + 20, 0, (Int32)Math.Round(delta * (this._ShellListView.View == ShellViewStyle.Details ? 1 : 1)));
          this._ScrollOldValue = this.sbLVVertical.Value;
          //User32.LockWindowUpdate(IntPtr.Zero);
        } else {
          User32.SendMessage(this._ShellListView.LVHandle, 0x1000 + 20, 0, (Int32)Math.Round(delta * (this._ShellListView.View == ShellViewStyle.Details ? 1 : 1)));
          this._ScrollOldValue = this.sbLVVertical.Value;
        }

        //User32.LockWindowUpdate(IntPtr.Zero);
      };
      this._ProgressTimer.Tick += (obj, args) => {
        if (this.bcbc.ProgressValue + 2 == this.bcbc.ProgressMaximum) {
          this.bcbc.ProgressMaximum = this.bcbc.ProgressMaximum + 2;
          this.bcbc.SetProgressValue(this.bcbc.ProgressValue + 2, TimeSpan.FromMilliseconds(0));
        } else {
          this.bcbc.SetProgressValue(this.bcbc.ProgressValue + 2, TimeSpan.FromMilliseconds(450));
        }
      };

      this._ProgressTimer.Stop();
      this.TheRibbon.UpdateLayout();
      this.grdItemTextColor.ItemsSource = this.LVItemsColorCol;
      this._keyjumpTimer.Interval = 1000;
      this._keyjumpTimer.Tick += this._keyjumpTimer_Tick;
      //this.ShellTreeHost.Child = this.ShellTree;
      this.ShellViewHost.Child = this._ShellListView;

      this.stvTreeView.ShellListView = this._ShellListView;
      //this.ctrlConsole.ShellListView = this._ShellListView;
      this.autoUpdater.UpdateAvailable += this.AutoUpdater_UpdateAvailable;
      this.updateCheckTimer.Interval = 10000;//3600000 * 3;
      this.updateCheckTimer.Tick += new EventHandler(this.updateCheckTimer_Tick);
      this.updateCheckTimer.Enabled = false;

      this.UpdateRecycleBinInfos();
      bool exitApp = false;

      try {
        //Sets up FileSystemWatcher for Favorites folder
        var LinksFolderWarcher = new FileSystemWatcher(KnownFolders.Links.ParsingName);
        LinksFolderWarcher.Created += this.LinksFolderWarcher_Created;
        LinksFolderWarcher.Deleted += this.LinksFolderWarcher_Deleted;
        LinksFolderWarcher.Renamed += this.LinksFolderWarcher_Renamed;
        LinksFolderWarcher.EnableRaisingEvents = true;

        //Set up Favorites Menu

        //Task.Run(() => SetUpFavoritesMenu());
        this.SetUpFavoritesMenu();

        //Load the ShellSettings
        this._IsCalledFromLoading = true;
        var statef = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref statef, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS | BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, false);
        this.chkHiddenFiles.IsChecked = statef.fShowAllObjects == 1;
        this._ShellListView.ShowHidden = this.chkHiddenFiles.IsChecked.Value;
        this.stvTreeView.IsShowHiddenItems = this.chkHiddenFiles.IsChecked.Value;
        this.chkExtensions.IsChecked = statef.fShowExtensions == 1;
        this._ShellListView.IsFileExtensionShown = statef.fShowExtensions == 1;
        this._IsCalledFromLoading = false;

        this.isOnLoad = true;

        //'load from Registry
        this.LoadRegistryRelatedSettings();

        //'set up Explorer control
        this.InitializeExplorerControl();

        this.ViewGallery.SelectedIndex = 2;

        if (this.chkUpdateCheck.IsChecked.Value) {
          this.updateCheckTimer.Start();
        }

        this.AddToLog("Session Began");
        this.isOnLoad = false;
        this.SetsUpJumpList();

        //Setup Clipboard monitor
        this.cbm.ClipboardChanged += this.cbm_ClipboardChanged;

        if (exitApp) {
          Application.Current.Shutdown();
          return;
        }

        // Set up Column Header menu
        this.chcm = new ContextMenu() { Placement = PlacementMode.MousePoint };

        //Set up Version Info
        this.verNumber.Content = "Version " + (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault() as AssemblyInformationalVersionAttribute).InformationalVersion;
        this.lblArchitecture.Content = Kernel32.IsThis64bitProcess() ? "64-bit version" : "32-bit version";

        this.tcMain_Setup(null, null);
        //'set StartUp location
        if (Application.Current.Properties["cmd"] != null && Application.Current.Properties["cmd"].ToString() != "-minimized") {
          var cmd = Application.Current.Properties["cmd"].ToString();
          if (cmd != "/nw" && cmd != "/t") {
            var sho = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, cmd.ToShellParsingName());
            this.tcMain.NewTab(sho, true);
          }
        } else {
          this.InitializeInitialTabs();
        }

        if (!File.Exists("Settings.xml"))
          return;
        var Settings = XElement.Load("Settings.xml");

        if (Settings.Element("DropDownItems") != null) {
          foreach (var item in Settings.Element("DropDownItems").Elements()) {
            this.bcbc.DropDownItems.Add(item.Value);
          }
        }

        this.focusTimer.Tick += this.focusTimer_Tick;
      } catch (Exception exe) {
        MessageBox.Show($"An error occurred while loading the window. Please report this issue at http://bugs.gainedge.org/public/betterexplorer. \r\n\r\n Here is some information about the error: \r\n\r\n{exe.Message}\r\n\r\n{exe}", "Error While Loading", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void AutoUpdater_UpdateAvailable(object sender, EventArgs e) {
      if (this._IsCheckUpdateFromTimer && !this._IsUpdateNotificationMessageBoxShown) {
        this._IsUpdateNotificationMessageBoxShown = true;
        var newVersion = this.autoUpdater.Version;
        var changes = this.autoUpdater.Changes;
        var config = new TaskDialogOptions();

        config.Owner = this;
        config.Title = "Update";
        config.MainInstruction = "There is new updated version " + newVersion + " available!";
        config.Content = "The new version have the following changes:\r\n" + changes;
        config.ExpandedInfo = "You can download and install the new version immediately by clicking \"Download & Install\" button.\r\nYou can skip this version from autoupdate check by clicking \"Skip this version\" button.";
        //config.VerificationText = "Don't show me this message again";
        config.CustomButtons = new string[] { "&Download & Install", "Skip this version", "&Close" };
        config.MainIcon = VistaTaskDialogIcon.SecurityWarning;

        if (newVersion.Contains("RC") || newVersion.Contains("Nightly") || newVersion.Contains("Beta") || newVersion.Contains("Alpha")) {
          config.FooterText = "This is an experimental version and may contains bugs. Use at your own risk!";
          config.FooterIcon = VistaTaskDialogIcon.Warning;
        } else {
          config.FooterText = "This is a final version and can be installed safely!";
          config.FooterIcon = VistaTaskDialogIcon.SecuritySuccess;
        }

        config.AllowDialogCancellation = true;
        config.Callback = this.taskDialog_Callback;

        TaskDialogResult res = TaskDialog.Show(config);
        this._IsCheckUpdateFromTimer = false;
        this._IsUpdateNotificationMessageBoxShown = false;
      }
    }

    private bool taskDialog_Callback(IActiveTaskDialog dialog, VistaTaskDialogNotificationArgs args, object callbackData) {
      bool result = false;

      switch (args.Notification) {
        case VistaTaskDialogNotification.ButtonClicked:
          if (args.ButtonId == 500) {
            this.autoUpdater.ReadyToBeInstalled += this.AutoUpdater_ReadyToBeInstalled;
            this.autoUpdater.InstallNow();
          } else if (args.ButtonId == 501) {
          }
          break;
      }

      return result;
    }

    private void AutoUpdater_ReadyToBeInstalled(object sender, EventArgs e) {
      this.autoUpdater.ReadyToBeInstalled -= this.AutoUpdater_ReadyToBeInstalled;
      this.autoUpdater.InstallNow();
    }

    #endregion

    #region On Closing

    private void SaveSettings(String openedTabs) {
      BESettings.LastWindowWidth = this.Width;
      BESettings.LastWindowHeight = this.Height;
      BESettings.LastWindowPosLeft = this.Left;
      BESettings.LastWindowPosTop = this.Top;

      BESettings.CurrentTheme = this.btnTheme.IsChecked == true ? "Dark" : "Light";

      switch (this.WindowState) {
        case WIN.WindowState.Maximized:
          BESettings.LastWindowState = 2;
          break;
        case WIN.WindowState.Minimized:
          BESettings.LastWindowState = 1;
          break;
        case WIN.WindowState.Normal:
          BESettings.LastWindowState = 0;
          break;
        default:
          BESettings.LastWindowState = -1;
          break;
      }

      BESettings.IsRibonMinimized = this.TheRibbon.IsMinimized;
      BESettings.OpenedTabs = openedTabs;
      BESettings.RTLMode = this.FlowDirection == FlowDirection.RightToLeft;
      BESettings.ShowCheckboxes = this._ShellListView.ShowCheckboxes;
      BESettings.TabBarAlignment = this.TabbaBottom.IsChecked == true ? "bottom" : "top";

      if (BESettings.IsPreviewPaneEnabled)
        BESettings.PreviewPaneWidth = this.clPreview.ActualWidth;
      if (BESettings.IsInfoPaneEnabled)
        BESettings.InfoPaneHeight = this.rPreviewPane.ActualHeight;
      if (BESettings.IsConsoleShown)
        BESettings.CmdWinHeight = this.rCommandPrompt.ActualHeight;

      BESettings.SaveSettings();
    }

    private void RibbonWindow_Closing(object sender, CancelEventArgs e) {
      var itemColorSettingsLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"BExplorer\itemcolors.cfg");
      var doc = new XmlDocument();
      XmlElement elementRoot = doc.CreateElement(string.Empty, "Root", string.Empty);
      foreach (var element in this.LVItemsColorCol) {
        XmlElement elementRow = doc.CreateElement(string.Empty, "ItemColorRow", string.Empty);
        XmlElement elementExtension = doc.CreateElement(string.Empty, "Extensions", string.Empty);
        elementExtension.InnerText = element.ExtensionList;
        XmlElement elementColor = doc.CreateElement(string.Empty, "Color", string.Empty);
        elementColor.InnerText = BitConverter.ToInt32(new byte[] { element.TextColor.A, element.TextColor.R, element.TextColor.G, element.TextColor.B }, 0).ToString();
        elementRow.AppendChild(elementExtension);
        elementRow.AppendChild(elementColor);
        elementRoot.AppendChild(elementRow);
      }
      doc.AppendChild(elementRoot);
      doc.Save(itemColorSettingsLocation);

      if (this.OwnedWindows.OfType<FileOperationDialog>().Any()) {
        if (MessageBox.Show("Are you sure you want to cancel all running file operation tasks?", "", MessageBoxButton.YesNo) == MessageBoxResult.No) {
          e.Cancel = true;
          return;
        }
      }

      if (this.WindowState != WindowState.Minimized) {
        this.SaveSettings(string.Concat(from item in this.tcMain.Items.Cast<Wpf.Controls.TabItem>() select ";" + item.ShellObject.ParsingName));
      }

      this._ShellListView.SaveSettingsToDatabase(this._ShellListView.CurrentFolder);
      //SaveHistoryToFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\history.txt", this.bcbc.DropDownItems.OfType<String>().Select(s => s).ToList());
      this.AddToLog("Session Ended");
      if (!this.IsMultipleWindowsOpened) {
        e.Cancel = true;
        App.IsStartMinimized = true;

        this.WindowState = WindowState.Minimized;
        this.Visibility = Visibility.Hidden;
      } else {
        this.beNotifyIcon.Visibility = Visibility.Collapsed;
      }

      if (!File.Exists("Settings.xml"))
        new XElement("Settings").Save("Settings.xml");
      //var Data = bcbc.DropDownItems;

      var Settings = XElement.Load("Settings.xml");
      if (Settings.Element("DropDownItems") == null)
        Settings.Add(new XElement("DropDownItems"));
      else
        Settings.Element("DropDownItems").RemoveAll();

      foreach (var item in this.bcbc.DropDownItems.OfType<string>().Reverse().Take(15)) {
        Settings.Element("DropDownItems").Add(new XElement("Item", item));
      }
      //Settings.Save("Settings.xml");
    }

    #endregion

    #region Change Ribbon Color (Theme)

    public void ChangeRibbonTheme(string themeName, bool IsMetro = false) => this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
      switch (themeName) {
        case "Dark":
          ThemeManager.ChangeAppTheme(Application.Current, "BaseDark");
          break;
        default:
          ThemeManager.ChangeAppTheme(Application.Current, "BaseLight");
          break;
      }
      Settings.BESettings.CurrentTheme = themeName;
    }));

    #endregion

    #region Archive Commands

    private void btnExtractNow_Click(object sender, RoutedEventArgs e) {
      if (this.chkUseNewFolder.IsChecked == true) {
        string OutputLoc = $"{this.txtExtractLocation.Text}\\{Utilities.RemoveExtensionsFromFile(this._ShellListView.GetFirstSelectedItem().ParsingName, new FileInfo(this._ShellListView.GetFirstSelectedItem().ParsingName).Extension)}";

        try {
          Directory.CreateDirectory(OutputLoc);
          this.ExtractToLocation(this.SelectedArchive, OutputLoc);
        } catch (Exception) {
          MessageBoxResult wtd = MessageBox.Show($"The directory {OutputLoc} already exists. Would you like for BetterExplorer to extract there instead?", "Folder Exists", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
          switch (wtd) {
            case MessageBoxResult.Cancel:
              break;
            case MessageBoxResult.No:
              break;
            case MessageBoxResult.None:
              break;
            case MessageBoxResult.OK:
              break;
            case MessageBoxResult.Yes:
              this.ExtractToLocation(this.SelectedArchive, OutputLoc);
              break;
            default:
              break;
          }
        }
      } else {
        this.ExtractToLocation(this.SelectedArchive, this.txtExtractLocation.Text);
      }
    }

    private void btnChooseLocation_Click(object sender, RoutedEventArgs e) {
      var dlg = new FolderSelectDialog();
      if (dlg.ShowDialog())
        this.txtExtractLocation.Text = dlg.FileName;
    }

    private void ExtractToLocation(string archive, string output) {
      var selectedItems = new List<string>() { archive };
      var archiveProcressScreen = new ArchiveProcressScreen(selectedItems, output, ArchiveAction.Extract, "");
      archiveProcressScreen.ExtractionCompleted += new ArchiveProcressScreen.ExtractionCompleteEventHandler(this.ExtractionHasCompleted);
      this.AddToLog($"Archive Extracted to {output} from source {archive}");
      archiveProcressScreen.Show();
    }

    private void ExtractionHasCompleted(object sender, ArchiveEventArgs e) {
      if (this.chkOpenResults.IsChecked == true)
        this.tcMain.NewTab(e.OutputLocation);
    }

    private void miExtractToLocation_Click(object sender, RoutedEventArgs e) {
      var selectedItems = this._ShellListView.SelectedItems.Select(item => item.ParsingName).ToList();

      try {
        var CAI = new CreateArchive(selectedItems, false, this._ShellListView.GetFirstSelectedItem().ParsingName);
        CAI.Show(this.GetWin32Window());
      } catch {

      }
    }

    private void miExtractHere_Click(object sender, RoutedEventArgs e) {
      string FileName = this._ShellListView.GetFirstSelectedItem().ParsingName;
      var extractor = new SevenZipExtractor(FileName);
      string DirectoryName = Path.GetDirectoryName(FileName);
      string ArchName = Path.GetFileNameWithoutExtension(FileName);
      extractor.Extracting += new EventHandler<ProgressEventArgs>(this.extractor_Extracting);
      extractor.ExtractionFinished += new EventHandler<EventArgs>(this.extractor_ExtractionFinished);
      extractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(this.extractor_FileExtractionStarted);
      extractor.FileExtractionFinished += new EventHandler<FileInfoEventArgs>(this.extractor_FileExtractionFinished);
      extractor.PreserveDirectoryStructure = true;
      string Separator = "";
      if (DirectoryName[DirectoryName.Length - 1] != Char.Parse(@"\"))
        Separator = @"\";
      this.AddToLog($"Extracted Archive to {DirectoryName}{Separator}{ArchName} from source {FileName}");
      extractor.BeginExtractArchive(DirectoryName + Separator + ArchName);
    }

    void extractor_FileExtractionFinished(object sender, FileInfoEventArgs e) {
      //throw new NotImplementedException();
    }

    void extractor_FileExtractionStarted(object sender, FileInfoEventArgs e) {
      //throw new NotImplementedException();
    }

    void extractor_ExtractionFinished(object sender, EventArgs e) {
      //throw new NotImplementedException();
      (sender as SevenZipExtractor)?.Dispose();
    }

    void extractor_Extracting(object sender, ProgressEventArgs e) {
      //throw new NotImplementedException();
    }

    private void btnExtract_Click(object sender, RoutedEventArgs e) => this.miExtractHere_Click(sender, e);

    private void btnCheckArchive_Click(object sender, RoutedEventArgs e) => new Thread(new ThreadStart(this.DoCheck)).Start();

    private void DoCheck() {
      string FileName = this._ShellListView.GetFirstSelectedItem().ParsingName;
      var extractor = new SevenZipExtractor(FileName);
      if (!extractor.Check())
        MessageBox.Show("Not Pass");
      else
        MessageBox.Show("Pass");

      extractor.Dispose();
    }

    private void btnViewArchive_Click(object sender, RoutedEventArgs e) {
      var name = this._ShellListView.SelectedItems.First().ParsingName;
      string ICON_DLLPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
      var archiveDetailView = new ArchiveDetailView(ICON_DLLPATH, name);
      archiveDetailView.Show(this.GetWin32Window());

      //ArchiveViewWindow g = new ArchiveViewWindow( ShellListView.GetFirstSelectedItem(), IsPreviewPaneEnabled, IsInfoPaneEnabled);
      //g.Show();
    }

    private void btnCreateArchive_Click(object sender, RoutedEventArgs e) {
      //ArchiveCreateWizard acw = new ArchiveCreateWizard(ShellListView.SelectedItems, ShellListView.CurrentFolder.ParsingName);
      //         acw.win = this;
      //         acw.LoadStrings();
      //acw.Show();
      //AddToLog("Archive Creation Wizard begun. Current location: " + ShellListView.CurrentFolder.ParsingName);
    }

    #endregion

    #region Library Commands

    private void btnChangeLibIcon_Click(object sender, RoutedEventArgs e) => new IconView().LoadIcons(this._ShellListView, true);

    private void btnOLItem_Click(object sender, RoutedEventArgs e) {
      //this._ShellListView.IsLibraryInModify = true;
      this._ShellListView.CurrentRefreshedItemIndex = this._ShellListView.GetFirstSelectedItemIndex();
      var NeededFile = this._ShellListView.GetSelectedCount() == 1 ? this._ShellListView.GetFirstSelectedItem() : this._ShellListView.CurrentFolder;
      var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(NeededFile.ParsingName), false);

      switch ((sender as MenuItem).Tag.ToString()) {
        case "gu":
          lib.LibraryType = LibraryFolderType.Generic;
          lib.Close();
          break;
        case "doc":
          lib.LibraryType = LibraryFolderType.Documents;
          lib.Close();
          break;
        case "pic":
          lib.LibraryType = LibraryFolderType.Pictures;
          lib.Close();
          break;
        case "vid":
          lib.LibraryType = LibraryFolderType.Videos;
          lib.Close();
          break;
        case "mu":
          lib.LibraryType = LibraryFolderType.Music;
          lib.Close();
          break;
        default:
          break;
      }
    }

    private void chkPinNav_CheckChanged(object sender, RoutedEventArgs e) {
      //this._ShellListView.IsLibraryInModify = true;
      this._ShellListView.CurrentRefreshedItemIndex = this._ShellListView.GetFirstSelectedItemIndex();
      var NeededFile = this._ShellListView.GetSelectedCount() == 1 ? this._ShellListView.GetFirstSelectedItem() : this._ShellListView.CurrentFolder;
      try {
        var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(NeededFile.ParsingName), false);
        if (!this.IsFromSelectionOrNavigation)
          lib.IsPinnedToNavigationPane = e.RoutedEvent.Name == "Checked";

        lib.Close();
      } catch (FileNotFoundException) { }
    }

    private void btnManageLib_Click(object sender, RoutedEventArgs e) {
      this._ShellListView.CurrentRefreshedItemIndex = this._ShellListView.GetFirstSelectedItemIndex();
      try {
        ShellLibrary.ShowManageLibraryUI(this._ShellListView.GetFirstSelectedItem().ComInterface,
                                        this.Handle, "Choose which folders will be in this library", "A library gathers content from all of the folders listed below and puts it all in one window for you to see.", true);
      } catch {
        ShellLibrary.ShowManageLibraryUI(this._ShellListView.CurrentFolder.ComInterface,
                                        this.Handle, "Choose which folders will be in this library", "A library gathers content from all of the folders listed below and puts it all in one window for you to see.", true);
      }
    }

    #endregion

    #region Navigation (Back/Forward Arrows) and Up Button

    private void leftNavBut_Click(object sender, RoutedEventArgs e) {
      this.tcMain.isGoingBackOrForward = true;
      this.NavigationController((this.tcMain.SelectedItem as Wpf.Controls.TabItem).log.NavigateBack());
    }

    private void rightNavBut_Click(object sender, RoutedEventArgs e) {
      this.tcMain.isGoingBackOrForward = true;
      this.NavigationController((this.tcMain.SelectedItem as Wpf.Controls.TabItem).log.NavigateForward());
    }

    private void downArrow_Click(object sender, RoutedEventArgs e) {
      this._CMHistory.Items.Clear();
      if (this.tcMain.SelectedItem == null)
        return;
      var nl = ((Wpf.Controls.TabItem)this.tcMain.SelectedItem).log;
      var i = 0;
      foreach (var item in nl.HistoryItemsList) {
        if (item != null) {
          var itemCopy = item.Clone();
          this._CMHistory.Items.Add(Utilities.Build_MenuItem(itemCopy.DisplayName, itemCopy, itemCopy.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default),
                                                                                                           checkable: true, isChecked: i == nl.CurrentLocPos, GroupName: "G1", onClick: this.miItems_Click));
        }
        i++;
      }

      this._CMHistory.Placement = PlacementMode.Bottom;
      this._CMHistory.PlacementTarget = this.navBarGrid;
      this._CMHistory.IsOpen = true;
    }

    void miItems_Click(object sender, RoutedEventArgs e) {
      var item = (IListItemEx)(sender as MenuItem).Tag;
      if (item != null) {
        this.tcMain.isGoingBackOrForward = true;
        NavigationLog nl = (this.tcMain.Items[this.tcMain.SelectedIndex] as Wpf.Controls.TabItem).log;
        (sender as MenuItem).IsChecked = true;
        nl.CurrentLocPos = this._CMHistory.Items.IndexOf(sender);
        this.NavigationController(item);
      }
    }

    private void btnUpLevel_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.CanNavigateParent) {
        this.IsNeedEnsureVisible = true;
        this._ShellListView.NavigateParent();
      }
    }

    #endregion

    #region View Tab/Status Bar

    private void btnMoreColls_Click(object sender, RoutedEventArgs e) => this.micm_Click(sender, e);
    private void btnAutosizeColls_Click(object sender, RoutedEventArgs e) => this._ShellListView.AutosizeAllColumns(-1);

    private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (this._IsShouldRiseViewChanged)
        this._ShellListView.ResizeIcons((int)e.NewValue);
    }

    void mig_Click(object sender, RoutedEventArgs e) {
      this._ShellListView.EnableGroups();
      this._ShellListView.GenerateGroupsFromColumn((sender as MenuItem).Tag as Collumns);
    }

    private Boolean _IsShouldRiseViewChanged = true;

    private void StatusBar_Folder_Buttons(object sender, RoutedEventArgs e) {
      this._IsShouldRiseViewChanged = false;
      if (this._ShellListView == null)
        return;
      else if (sender == this.btnSbDetails) {
        this._ShellListView.View = ShellViewStyle.Details;
      } else if (sender == this.btnSbIcons) {
        this._ShellListView.View = ShellViewStyle.Medium;
      } else if (sender == this.btnSbTiles) {
        this._ShellListView.View = ShellViewStyle.Tile;
      }

      this._IsShouldRiseViewChanged = true;
    }

    void mi_Click(object sender, RoutedEventArgs e) {
      var item = (sender as MenuItem);
      var parentButton = item.Parent as DropDownButton;
      var ascitem = (MenuItem)parentButton.Items[parentButton.Items.IndexOf(this.misa)];

      var Sort = ascitem.IsChecked ? WIN.Forms.SortOrder.Ascending : WIN.Forms.SortOrder.Descending;
      this._ShellListView.SetSortCollumn(true, (Collumns)item.Tag, Sort);
    }

    void misng_Click(object sender, RoutedEventArgs e) {
      (sender as MenuItem).IsChecked = true;
      if (this._ShellListView.IsGroupsEnabled)
        this._ShellListView.DisableGroups();
    }

    private void inRibbonGallery1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (!this._IsShouldRiseViewChanged || !this._ShellListView.IsViewSelectionAllowed)
        return;
      e.Handled = true;
      if (e.AddedItems.Count == 0)
        return;
      var selectedItem = e.AddedItems[0];
      var selectedItemIndex = this.ViewGallery.Items.IndexOf(selectedItem);
      this._IsShouldRiseViewChanged = false;
      switch (selectedItemIndex) {
        case 0:
          this._ShellListView.View = ShellViewStyle.ExtraLargeIcon;
          break;
        case 1:
          this._ShellListView.View = ShellViewStyle.LargeIcon;
          break;
        case 2:
          this._ShellListView.View = ShellViewStyle.Medium;
          break;
        case 3:
          this._ShellListView.View = ShellViewStyle.SmallIcon;
          break;
        case 4:
          this._ShellListView.View = ShellViewStyle.List;
          break;
        case 5:
          this._ShellListView.View = ShellViewStyle.Details;
          break;
        case 6:
          this._ShellListView.View = ShellViewStyle.Tile;
          break;
        case 7:
          this._ShellListView.View = ShellViewStyle.Content;
          break;
        case 8:
          this._ShellListView.View = ShellViewStyle.Thumbstrip;
          break;
        default:
          break;
      }
      this._IsShouldRiseViewChanged = true;
    }

    private void chkHiddenFiles_Checked(object sender, RoutedEventArgs e) {
      if (this._IsCalledFromLoading)
        return;
      this.Dispatcher.BeginInvoke(new Action(
                      delegate () {
                        var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = 1 };
                        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS, true);
                        this._ShellListView.ShowHidden = true;

                        this.stvTreeView.IsShowHiddenItems = true;
                        this.stvTreeView.RefreshContents();
                      }
      ));
    }

    private void chkHiddenFiles_Unchecked(object sender, RoutedEventArgs e) {
      if (this._IsCalledFromLoading)
        return;
      this.Dispatcher.BeginInvoke(new Action(
                      delegate () {
                        var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = 0 };
                        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS, true);
                        this._ShellListView.ShowHidden = false;

                        this.stvTreeView.IsShowHiddenItems = false;
                        this.stvTreeView.RefreshContents();
                      }
      ));
    }

    private void chkExtensions_Checked(object sender, RoutedEventArgs e) {
      if (this._IsCalledFromLoading)
        return;
      this.Dispatcher.BeginInvoke(new Action(
                      delegate () {
                        var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
                        state.fShowExtensions = 1;
                        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, true);
                        this._ShellListView.IsFileExtensionShown = true;
                        this._ShellListView.RefreshContents();
                      }
      ));
    }

    private void chkExtensions_Unchecked(object sender, RoutedEventArgs e) {
      if (this._IsCalledFromLoading)
        return;
      this.Dispatcher.BeginInvoke(new Action(
                      delegate () {
                        var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
                        state.fShowExtensions = 0;
                        BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, true);
                        this._ShellListView.IsFileExtensionShown = false;
                        this._ShellListView.RefreshContents();
                      }
      ));
    }

    #endregion

    #region Hide items

    private void btnHideSelItems_Click(object sender, RoutedEventArgs e) {
      foreach (var item in this._ShellListView.SelectedItems.Where(x => x.IsFolder)) {
        var di = new DirectoryInfo(item.ParsingName);
        di.Attributes |= System.IO.FileAttributes.Hidden;
      }
      foreach (var item in this._ShellListView.SelectedItems.Where(x => !x.IsFolder & !x.IsNetworkPath & !x.IsDrive)) {
        var di = new FileInfo(item.ParsingName);
        di.Attributes |= System.IO.FileAttributes.Hidden;
      }

      this._ShellListView.RefreshContents();
    }

    [Obsolete("Why do we have this")]
    private void miHideItems_Click(object sender, RoutedEventArgs e) {
      //FIXME:
      //ShellItemCollection SelItems = ShellListView.SelectedItems;
      //pd = new IProgressDialog(this.Handle);
      //pd.Title = "Applying attributes";
      //pd.CancelMessage = "Please wait while the operation is cancelled";
      //pd.Maximum = 100;
      //pd.Value = 0;
      //pd.Line1 = "Applying attributes to:";
      //pd.Line3 = "Calculating Time Remaining...";
      //pd.ShowDialog(IProgressDialog.PROGDLG.Normal, IProgressDialog.PROGDLG.AutoTime, IProgressDialog.PROGDLG.NoMinimize);
      //Thread hthread = new Thread(new ParameterizedThreadStart(DoHideShowWithChilds));
      //hthread.Start(SelItems);
    }

    #endregion

    #region Share Tab Commands (excluding Archive)

    private void btnDisconectDrive_Click(object sender, RoutedEventArgs e) => BExplorer.Shell.Interop.Shell32.WNetDisconnectDialog(this.Handle, 1);
    private void Button_Click_4(object sender, RoutedEventArgs e) => this._ShellListView.OpenShareUI(); //TODO: Rename Button_Click_4

    private void btnMapDrive_Click(object sender, RoutedEventArgs e) {
      BExplorer.Shell.Interop.Shell32.MapDrive(this.Handle, this._ShellListView.SelectedItems.Count() == 1 ? this._ShellListView.GetFirstSelectedItem().ParsingName : String.Empty);
    }

    private void btnAdvancedSecurity_Click(object sender, RoutedEventArgs e) {
      this._ShellListView.ShowPropPage(this.Handle, this._ShellListView.GetFirstSelectedItem().ParsingName, User32.LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 9046, "Security1"));
    }

    #endregion

    #region Change Language

    private void TranslationComboBox_DropDownOpened(object sender, EventArgs e) => (sender as Fluent.ComboBox).Focus();
    private void TranslationHelp_Click(object sender, RoutedEventArgs e) => Process.Start(this.TranslationURL.Text);

    private void TranslationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      e.Handled = true;
      if (this.IsLoaded) {
        BESettings.Locale = ((TranslationComboBoxItem)e.AddedItems[0]).LocaleCode;
        ((App)Application.Current).SelectCulture(((TranslationComboBoxItem)e.AddedItems[0]).LocaleCode);

        if (this._ShellListView.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
          this.btnCreateFolder.Header = this.FindResource("btnNewLibraryCP");       //"New Library";
          this.stNewFolder.Title = this.FindResource("btnNewLibraryCP").ToString(); //"New Library";
          this.stNewFolder.Text = "Creates a new library in the current folder.";
        } else {
          this.btnCreateFolder.Header = this.FindResource("btnNewFolderCP");        //"New Folder";
          this.stNewFolder.Title = this.FindResource("btnNewFolderCP").ToString();  //"New Folder";
          this.stNewFolder.Text = "Creates a new folder in the current folder";
        }
      }
    }

    private void btnRemoveLangSetting_Click(object sender, RoutedEventArgs e) {
      RegistryKey rk = Registry.CurrentUser;
      RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);

      rks.DeleteValue(@"Locale");

      rks.Close();
      rk.Close();
    }

    #endregion

    #region Image Editing

    private void btnResize_Click(object sender, RoutedEventArgs e) => ResizeImage.Open(this._ShellListView.GetFirstSelectedItem());

    private void Convert_Images(object sender, RoutedEventArgs e) {
      ImageFormat format = null;
      string extension = null;

      if (sender == this.ConvertToJPG) {
        format = ImageFormat.Jpeg;
        extension = ".jpg";
      } else if (sender == this.ConvertToPNG) {
        format = ImageFormat.Png;
        extension = ".png";
      } else if (sender == this.ConvertToGIF) {
        format = ImageFormat.Gif;
        extension = ".gif";
      } else if (sender == this.ConvertToBMP) {
        format = ImageFormat.Bmp;
        extension = ".bmp";
      } else if (sender == this.ConvertToJPG) {
        format = ImageFormat.Wmf;
        extension = ".wmf";
      } else {
        throw new Exception("Invalid Sender");
      }

      foreach (var item in this._ShellListView.SelectedItems) {
        var cvt = new Bitmap(item.ParsingName);
        string namen = Utilities.RemoveExtensionsFromFile(item.ParsingName, new FileInfo(item.ParsingName).Extension);
        try {
          this.AddToLog("Converted Image from " + item.ParsingName + " to new file " + namen + extension);
          var newFilePath = namen + extension;
          cvt.Save(newFilePath, format);
          this._ShellListView.UnvalidateDirectory();
        } catch (Exception) {
          MessageBox.Show("There appears to have been an issue with converting the file. Make sure the filename \"" + Utilities.RemoveExtensionsFromFile(this._ShellListView.GetFirstSelectedItem().DisplayName, new System.IO.FileInfo(item.ParsingName).Extension) + extension + "\" does already not exist.", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        cvt.Dispose();
      }
    }

    /// <summary>
    /// Sets the wallpaper based on the sender
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Set_Wallpaper(object sender, RoutedEventArgs e) {
      Wallpaper.Style ThisStyle;

      if (sender == this.btnWallpaper)
        ThisStyle = Wallpaper.Style.Stretched;
      else if (sender == this.miWallFill)
        ThisStyle = Wallpaper.Style.Fill;
      else if (sender == this.miWallFit)
        ThisStyle = Wallpaper.Style.Fit;
      else if (sender == this.miWallStretch)
        ThisStyle = Wallpaper.Style.Stretched;
      else if (sender == this.miWallTile)
        ThisStyle = Wallpaper.Style.Tiled;
      else if (sender == this.miWallCenter)
        ThisStyle = Wallpaper.Style.Centered;
      else
        throw new Exception("Invalid Sender");

      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)(() => {
        var TheWall = new Wallpaper();
        TheWall.Set(new Uri(this._ShellListView.GetFirstSelectedItem().ParsingName), ThisStyle);
      }));
    }

    private void RotateImages(object sender, RoutedEventArgs e) {
      RotateFlipType Rotation;
      string DefaultName_Addon = null;

      switch ((sender as Control).Name) {
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

      foreach (FileSystemListItem item in this._ShellListView.SelectedItems) {
        var cvt = new Bitmap(item.ParsingName);
        cvt.RotateFlip(Rotation);
        if (BESettings.OverwriteImageWhileEditing) {
          cvt.Save(item.ParsingName);
        } else {
          string ext = item.ParsingName.Substring(item.ParsingName.LastIndexOf(".", StringComparison.Ordinal));
          string name = item.ParsingName;
          string namen = Utilities.RemoveExtensionsFromFile(name, new System.IO.FileInfo(name).Extension);
          var newFilePath = namen + DefaultName_Addon + ext;
          cvt.Save(newFilePath);
          this._ShellListView.UnvalidateDirectory();
        }
        cvt.Dispose();
        this.AddToLog("Rotated image " + item.ParsingName);
      }
    }

    #endregion

    #region Folder Tools Commands

    private void btnChangeFoldericon_Click(object sender, RoutedEventArgs e) => new IconView().LoadIcons(this._ShellListView, false);

    private void btnClearFoldericon_Click(object sender, RoutedEventArgs e) => this._ShellListView.ClearFolderIcon(this._ShellListView.GetFirstSelectedItem().ParsingName);

    #endregion

    #region Registry Setting Changes / BetterExplorerOperations Calls / Action Log

    //private void chkIsRestoreTabs_CheckChanged(object sender, RoutedEventArgs e) => Utilities.SetRegistryValue("IsRestoreTabs", e.RoutedEvent.Name == "Checked" ? 1 : 0);
    //private void gridSplitter1_DragCompleted(object sender, DragCompletedEventArgs e) => Utilities.SetRegistryValue("SearchBarWidth", SearchBarColumn.Width.Value);
    //private void SearchBarReset_Click(object sender, RoutedEventArgs e) => Utilities.SetRegistryValue("SearchBarWidth", SearchBarColumn.Width.Value);
    //private void chkIsCFO_Click(object sender, RoutedEventArgs e) => Utilities.SetRegistryValue("IsCustomFO", chkIsCFO.IsChecked.Value == true ? 1 : 0);

    private void chkIsRestoreTabs_CheckChanged(object sender, RoutedEventArgs e) {
      BESettings.IsRestoreTabs = e.RoutedEvent.Name == "Checked";
    }
    private void gridSplitter1_DragCompleted(object sender, DragCompletedEventArgs e) {
      BESettings.SearchBarWidth = this.SearchBarColumn.Width.Value;
    }
    private void SearchBarReset_Click(object sender, RoutedEventArgs e) {
      BESettings.SearchBarWidth = this.SearchBarColumn.Width.Value;
    }
    private void chkIsCFO_Click(object sender, RoutedEventArgs e) {
      BESettings.IsCustomFO = this.chkIsCFO.IsChecked.Value == true;
    }

    private Process Rename_CheckChanged_Helper() {
      string ExePath = Utilities.AppDirectoryItem("BetterExplorerOperations.exe");
      Process proc = new Process();
      proc.StartInfo = new ProcessStartInfo {
        FileName = ExePath,
        Verb = "runas",
        UseShellExecute = true,
        Arguments = $"/env /user:Administrator \"{ExePath}\""
      };
      proc.Start();
      Thread.Sleep(1000);

      return proc;
    }

    private void chkIsDefault_CheckChanged(object sender, RoutedEventArgs e) {
      //TODO: Delete Dead Code!!
      if (this.isOnLoad)
        return;
      var proc = this.Rename_CheckChanged_Helper();

      if (this.chkIsDefault.IsChecked == true) {
        //int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
        //int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x77654");
        //proc.WaitForExit();
        //if (proc.ExitCode == -1) {
        //	isOnLoad = true;
        //	(sender as Fluent.CheckBox).IsChecked = false;
        //	isOnLoad = false;
        //	MessageBox.Show("Can't set Better Explorer as default!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
      } else {
        //WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x77655");
        //proc.WaitForExit();
        //if (proc.ExitCode == -1) {
        //	isOnLoad = true;
        //	(sender as Fluent.CheckBox).IsChecked = true;
        //	isOnLoad = false;
        //	MessageBox.Show("Can't restore default filemanager!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
      }
    }

    private void chkTreeExpand_CheckChanged(object sender, RoutedEventArgs e) {
      //TODO: Delete Dead Code!!
      if (this.isOnLoad)
        return;
      var proc = this.Rename_CheckChanged_Helper();

      if (this.chkTreeExpand.IsChecked == true) {
        //int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
        //int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x88775");
        //proc.WaitForExit();
      } else {
        //int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
        //int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x88776");
        //proc.WaitForExit();
      }
    }

    private void btnSetCurrentasStartup_Click(object sender, RoutedEventArgs e) {
      this.backstage.IsOpen = true;
      string CurrentLocString = this._ShellListView.CurrentFolder.ParsingName;
      this.tcMain.StartUpLocation = CurrentLocString;
      this.btnSetCurrentasStartup.Header = this._ShellListView.CurrentFolder.DisplayName;
      //btnSetCurrentasStartup.Icon = ShellListView.CurrentFolder.Thumbnail.BitmapSource;

      BESettings.StartupLocation = CurrentLocString;
    }

    private void chkIsFlyout_CheckChanged(object sender, RoutedEventArgs e) {
      if (!this.isOnLoad) {
        BESettings.IsFileOpExEnabled = e.RoutedEvent.Name == "Checked";
      }
    }

    private void chkIsTerraCopyEnabled_CheckChanged(object sender, RoutedEventArgs e) {
      if (!this.isOnLoad) {
        BESettings.IsFileOpExEnabled = e.RoutedEvent.Name == "Checked";
      }
    }

    private void chkShowCheckBoxes_CheckChanged(object sender, RoutedEventArgs e) {
      this._ShellListView.ShowCheckboxes = e.RoutedEvent.Name == "Checked";
      this._ShellListView.RefreshContents();
    }

    private void chkOverwriteImages_Checked(object sender, RoutedEventArgs e) {
      BESettings.OverwriteImageWhileEditing = e.RoutedEvent.Name == "Checked";
    }

    private void chkRibbonMinimizedGlass_Click(object sender, RoutedEventArgs e) {
      BESettings.IsGlassOnRibonMinimized = this.chkRibbonMinimizedGlass.IsChecked.Value == true;

      if (!this.TheRibbon.IsMinimized) {
      } else if (this.chkRibbonMinimizedGlass.IsChecked.Value) {
        var p = this.ShellViewHost.TransformToAncestor(this).Transform(new WIN.Point(0, 0));
        this.GlassFrameThickness = new Thickness(8, p.Y, 8, 8);
      } else {
        var p = this.backstage.TransformToAncestor(this).Transform(new WIN.Point(0, 0));
        this.GlassFrameThickness = new Thickness(8, p.Y + this.backstage.ActualHeight, 8, 8);
      }
    }

    private void chkLogHistory_CheckChanged(object sender, RoutedEventArgs e) {
      BESettings.EnableActionLog = e.RoutedEvent.Name == "Checked";
      BESettings.EnableActionLog = e.RoutedEvent.Name == "Checked";
    }

    private void btnShowLogs_Click(object sender, RoutedEventArgs e) {
      try {
        if (!Directory.Exists(this.logdir))
          Directory.CreateDirectory(this.logdir);
        this.tcMain.NewTab(this.logdir);
        this.backstage.IsOpen = false;
      } catch (Exception exe) {
        MessageBox.Show("An error occurred while trying to open the logs folder. Please report this issue at http://bugs.gainedge.org/public/betterexplorer. \r\n\r\n Here is some information about the error: \r\n\r\n" + exe.Message + "\r\n\r\n" + exe.ToString(), "Error While Opening Log Folder", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    #endregion

    #region Change Pane Visibility

    private void btnNavigationPane_CheckChanged(object sender, RoutedEventArgs e) {
      if (this.isOnLoad) {
      } else if (e.RoutedEvent.Name == "Checked") {
        this.TreeGrid.ColumnDefinitions[0].Width = new GridLength(200);
        BESettings.IsNavigationPaneEnabled = true;
      } else {
        this.TreeGrid.ColumnDefinitions[0].Width = new GridLength(1);
        BESettings.IsNavigationPaneEnabled = false;
      }
    }

    private void btnPreviewPane_CheckChanged(object sender, RoutedEventArgs e) {
      if (this.isOnLoad) {
      } else if (e.RoutedEvent.Name == "Checked") {
        this.clPreview.Width = new GridLength(BESettings.PreviewPaneWidth);
        this.clPreviewSplitter.Width = new GridLength(1);
        var selectedItem = this._ShellListView.SelectedItems.FirstOrDefault();
        if (selectedItem != null && selectedItem.IsFileSystem && this._ShellListView.GetSelectedCount() == 1 && !selectedItem.IsFolder) {
          this.Previewer.FileName = selectedItem.ParsingName;
        }

        BESettings.IsPreviewPaneEnabled = true;
      } else {
        this.clPreview.Width = new GridLength(0);
        this.clPreviewSplitter.Width = new GridLength(0);
        this.Previewer.FileName = null;
        BESettings.IsPreviewPaneEnabled = false;
      }
    }

    private void btnInfoPane_CheckChanged(object sender, RoutedEventArgs e) {
      if (this.isOnLoad) {
      } else if (e.RoutedEvent.Name == "Checked") {
        this.rPreviewPane.Height = new GridLength(BESettings.InfoPaneHeight);
        this.rPreviewPaneSplitter.Height = new GridLength(1);
        BESettings.IsInfoPaneEnabled = true;
      } else {
        this.rPreviewPane.Height = new GridLength(0);
        this.rPreviewPaneSplitter.Height = new GridLength(0);
        BESettings.IsInfoPaneEnabled = false;
      }
    }

    #endregion

    #region Search

    private void edtSearchBox_BeginSearch(object sender, SearchRoutedEventArgs e) => this.DoSearch();

    public void DoSearch() {
      try {
        if (this.edtSearchBox.FullSearchTerms != "")
          this._ShellListView.Navigate_Full(this.edtSearchBox.FullSearchTerms, true, true);
      } catch (Exception ex) {
        MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK);
      }
    }

    private void btnSearch_Click(object sender, RoutedEventArgs e) => this.DoSearch();

    private void edtSearchBox_RequestCriteriaChange(object sender, SearchRoutedEventArgs e) {
      if (e.SearchTerms.StartsWith("author:"))
        this.AuthorToggle_Click(sender, new RoutedEventArgs(e.RoutedEvent));
      else if (e.SearchTerms.StartsWith("ext:"))
        this.ToggleButton_Click_1(sender, new RoutedEventArgs(e.RoutedEvent));
      else if (e.SearchTerms.StartsWith("subject:"))
        this.SubjectToggle_Click(sender, new RoutedEventArgs(e.RoutedEvent));
      else if (e.SearchTerms.StartsWith("size:"))
        this.miCustomSize_Click(sender, new RoutedEventArgs(e.RoutedEvent));
      else if (e.SearchTerms.StartsWith("date:"))
        this.dcCustomTime_Click(sender, new RoutedEventArgs(e.RoutedEvent));
      else if (e.SearchTerms.StartsWith("modified:"))
        this.dmCustomTime_Click(sender, new RoutedEventArgs(e.RoutedEvent));
      else {
        var T = "You have discovered an error in this program. Please tell us which filter you were trying to edit and any other details we should know. \r\n\r\nYour filter: ";
        MessageBox.Show(T + e.SearchTerms, "Oops! Found a Bug!", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void edtSearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
      this.ctgSearch.Visibility = Visibility.Visible;
      if (!this.TheRibbon.IsMinimized)
        this.TheRibbon.SelectedTabItem = this.tbSearch;
    }

    private void edtSearchBox_FiltersCleared(object sender, EventArgs e) {
      this.scSize.IsChecked = false;
      this.ExtToggle.IsChecked = false;
      this.AuthorToggle.IsChecked = false;
      this.SubjectToggle.IsChecked = false;
      this.dcsplit.IsChecked = false;
      this.dmsplit.IsChecked = false;


      foreach (var item in this.scSize.Items.OfType<MenuItem>().Union(this.dcsplit.Items.OfType<MenuItem>()).Union(this.dmsplit.Items.OfType<MenuItem>())) {
        item.IsChecked = false;
      }
    }

    private void MenuItem_Click_3(object sender, RoutedEventArgs e) {
      this.edtSearchBox.ModifiedCondition = (string)((FrameworkElement)sender).Tag;
      this.dmsplit.IsChecked = true;
    }

    private void MenuItem_Click_4(object sender, RoutedEventArgs e) {
      this.edtSearchBox.DateCondition = (string)((FrameworkElement)sender).Tag;
      this.dcsplit.IsChecked = true;
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e) {
      ((Fluent.ToggleButton)sender).IsChecked = true;
      this.edtSearchBox.KindCondition = (string)((FrameworkElement)sender).Tag;
      this.edtSearchBox.Focus();
    }

    private void MenuItem_Click_5(object sender, RoutedEventArgs e) {
      this.edtSearchBox.SizeCondition = (string)((FrameworkElement)sender).Tag;
      this.scSize.IsChecked = true;
    }

    private void ToggleButton_Click_1(object sender, RoutedEventArgs e) {
      var dat = new StringSearchCriteriaDialog("ext", this.edtSearchBox.ExtensionCondition, this.FindResource("btnExtCP") as string);
      dat.ShowDialog();
      if (dat.Confirm) {
        this.edtSearchBox.ExtensionCondition = "ext:" + dat.textBox1.Text;
        this.ExtToggle.IsChecked = dat.textBox1.Text.Any();
      } else {
        this.ExtToggle.IsChecked = Utilities.GetValueOnly("ext", this.edtSearchBox.ExtensionCondition).Any();
      }
    }

    private void AuthorToggle_Click(object sender, RoutedEventArgs e) {
      var dat = new StringSearchCriteriaDialog("author", this.edtSearchBox.AuthorCondition, this.FindResource("btnAuthorCP") as string);
      dat.ShowDialog();
      if (dat.Confirm) {
        this.edtSearchBox.AuthorCondition = "author:" + dat.textBox1.Text;
        this.AuthorToggle.IsChecked = dat.textBox1.Text.Any();
      } else {
        this.AuthorToggle.IsChecked = Utilities.GetValueOnly("author", this.edtSearchBox.AuthorCondition).Any();
      }
    }

    private void SubjectToggle_Click(object sender, RoutedEventArgs e) {
      var dat = new StringSearchCriteriaDialog("subject", this.edtSearchBox.SubjectCondition, this.FindResource("btnSubjectCP") as string);
      dat.ShowDialog();
      if (dat.Confirm) {
        this.edtSearchBox.SubjectCondition = "subject:" + dat.textBox1.Text;
        this.SubjectToggle.IsChecked = dat.textBox1.Text.Any();
      } else {
        this.SubjectToggle.IsChecked = Utilities.GetValueOnly("subject", this.edtSearchBox.SubjectCondition).Any();
      }
    }

    private void miCustomSize_Click(object sender, RoutedEventArgs e) {
      var dat = new SizeSearchCriteriaDialog();
      string sd = Utilities.GetValueOnly("size", this.edtSearchBox.SizeCondition);
      dat.curval.Text = sd;
      dat.ShowDialog();

      if (dat.Confirm) {
        this.edtSearchBox.SizeCondition = "size:" + dat.GetSizeQuery();
        this.scSize.IsChecked = dat.GetSizeQuery().Any();
      } else {
        this.scSize.IsChecked = dat.GetSizeQuery().Length > 5;
      }
    }

    private void dcCustomTime_Click(object sender, RoutedEventArgs e) {
      var star = new SDateSearchCriteriaDialog(this.FindResource("btnODateCCP") as string);

      star.DateCriteria = Utilities.GetValueOnly("date", this.edtSearchBox.DateCondition);
      //star.textBlock1.Text = "Set Date Created Filter";
      star.ShowDialog();

      if (star.Confirm)
        this.edtSearchBox.DateCondition = "date:" + star.DateCriteria;
      this.dcsplit.IsChecked = this.edtSearchBox.UseDateCondition;
    }

    private void dmCustomTime_Click(object sender, RoutedEventArgs e) {
      var star = new SDateSearchCriteriaDialog(this.FindResource("btnODateModCP") as string);

      star.DateCriteria = Utilities.GetValueOnly("modified", this.edtSearchBox.ModifiedCondition);
      //star.textBlock1.Text = "Set Date Modified Filter";
      star.ShowDialog();

      if (star.Confirm)
        this.edtSearchBox.ModifiedCondition = "modified:" + star.DateCriteria;
      this.dmsplit.IsChecked = this.edtSearchBox.UseModifiedCondition;
    }

    #endregion

    #region AutoSwitch

    private void chkFolder_CheckChanged(object sender, RoutedEventArgs e) => BESettings.AutoSwitchFolderTools = e.RoutedEvent.Name == "Checked";
    private void chkDrive_CheckChanged(object sender, RoutedEventArgs e) => BESettings.AutoSwitchDriveTools = e.RoutedEvent.Name == "Checked";
    private void chkArchive_CheckChanged(object sender, RoutedEventArgs e) => BESettings.AutoSwitchArchiveTools = e.RoutedEvent.Name == "Checked";
    private void chkApp_CheckChanged(object sender, RoutedEventArgs e) => BESettings.AutoSwitchApplicationTools = e.RoutedEvent.Name == "Checked";
    private void chkImage_CheckChanged(object sender, RoutedEventArgs e) => BESettings.AutoSwitchImageTools = e.RoutedEvent.Name == "Checked";
    private void chkLibrary_CheckChanged(object sender, RoutedEventArgs e) => BESettings.AutoSwitchLibraryTools = e.RoutedEvent.Name == "Checked";
    private void chkVirtualTools_CheckChanged(object sender, RoutedEventArgs e) => BESettings.AutoSwitchVirtualDriveTools = e.RoutedEvent.Name == "Checked";

    #endregion

    #region Tabs

    private void btnNewTab_Click(object sender, RoutedEventArgs e) => this.tcMain.NewTab();
    private void btnTabClone_Click(object sender, RoutedEventArgs e) => this.tcMain.CloneTabItem(this.tcMain.SelectedItem as Wpf.Controls.TabItem);
    private void btnTabCloseC_Click(object sender, RoutedEventArgs e) => this.tcMain.RemoveTabItem(this.tcMain.SelectedItem as Wpf.Controls.TabItem);
    private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e) => (this.tcMain.SelectedItem as Wpf.Controls.TabItem)?.BringIntoView();
    void newt_PreviewMouseDown(object sender, MouseButtonEventArgs e) => this.tcMain.IsInTabDragDrop = false;
    void newt_Leave(object sender, DragEventArgs e) => DropTarget.Create.DragLeave();
    void mim_Click(object sender, RoutedEventArgs e) => this.SetFOperation(((sender as MenuItem).Tag as IListItemEx), OperationType.Move);
    void mico_Click(object sender, RoutedEventArgs e) => this.SetFOperation(((sender as MenuItem).Tag as IListItemEx), OperationType.Copy);

    private void btnUndoClose_Click(object sender, RoutedEventArgs e) {
      this.tcMain.ReOpenTab(this.tcMain.ReopenableTabs[this.tcMain.ReopenableTabs.Count - 1]);
      this.btnUndoClose.IsEnabled = this.tcMain.ReopenableTabs.Any();
    }

    void gli_Click(object sender, NavigationLogEventArgs e) {
      this.tcMain.ReOpenTab(e.NavigationLog);
      this.btnUndoClose.IsEnabled = this.tcMain.ReopenableTabs.Any();
    }

    void gli_Click(object sender, Tuple<string> e) {
      var list = SaveTabs.LoadTabList($"{BESettings.SavedTabsDirectory}{e.Item1}.txt");
      if (list.Count > 0) {
        foreach (var tabItem in this.tcMain.Items.OfType<Wpf.Controls.TabItem>().ToArray()) {
          this.tcMain.RemoveTabItem(tabItem, true, true);
        }
      }
      for (int i = 0; i < list.Count; i++) {
        var tabitem = this.tcMain.NewTab(list[i].ToShellParsingName());
        if (i == list.Count - 1)
          this.tcMain.SelectedItem = tabitem;
      }
    }

    private void GoToSearchBox(object sender, ExecutedRoutedEventArgs e) {
      this.edtSearchBox.Focus();
      Keyboard.Focus(this.edtSearchBox);
    }
    void newt_PreviewMouseMove(object sender, MouseEventArgs e) {
      var tabItem = e.Source as Wpf.Controls.TabItem;

      if (tabItem == null)
        this.tcMain.IsInTabDragDrop = false;
      else if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
        DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
    }
    void newt_Drop(object sender, DragEventArgs e) {
      e.Handled = true;
      var tabItemTarget = e.Source as Wpf.Controls.TabItem;

      var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;

      if (tabItemSource == null) {
        if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem) {
          e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;

          switch (e.Effects) {
            case DragDropEffects.All:
              break;
            case DragDropEffects.Copy:
              this._ShellListView.DoCopy(e.Data, (sender as Wpf.Controls.TabItem).ShellObject);
              break;
            case DragDropEffects.Link:
              break;
            case DragDropEffects.Move:
              this._ShellListView.DoMove(e.Data, (sender as Wpf.Controls.TabItem).ShellObject);
              break;
            case DragDropEffects.None:
              break;
            case DragDropEffects.Scroll:
              break;
            default:
              break;
          }
        } else {
          e.Effects = DragDropEffects.None;
        }

        WIN.Point pt = e.GetPosition(sender as IInputElement);
        var wpt = new BExplorer.Shell.DataObject.Win32Point() { X = (int)pt.X, Y = (int)pt.Y };
        DropTarget.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
        this._TabDropData = null;
      } else if (!tabItemTarget.Equals(tabItemSource)) {
        var tabControl = tabItemTarget.Parent as TabControl;
        int targetIndex = tabControl.Items.IndexOf(tabItemTarget);

        int tabState = tabItemSource == this._CurrentlySelectedItem ? 0 : 1;

        tabControl.Items.Remove(tabItemSource);
        tabControl.Items.Insert(targetIndex, tabItemSource);
        this.tcMain.IsInTabDragDrop = false;
        if (tabState == 1)
          tabControl.SelectedItem = this._CurrentlySelectedItem;
        else if (tabState == 0)
          tabControl.SelectedIndex = targetIndex;

        this.tcMain.IsInTabDragDrop = true;
      }
      //tcMain.IsSelectionHandled = false;
    }

    void newt_DragOver(object sender, DragEventArgs e) {
      e.Handled = true;

      var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;

      if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem)
        e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
      else if (tabItemSource != null)
        e.Effects = DragDropEffects.Move;
      else
        e.Effects = DragDropEffects.None;

      var desc = new BExplorer.Shell.DataObject.DropDescription();

      switch (e.Effects) {
        case DragDropEffects.Copy:
          desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Copy;
          desc.szMessage = "Copy To %1";
          break;
        case DragDropEffects.Move:
          desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Move;
          desc.szMessage = "Move To %1";
          break;
        case DragDropEffects.None:
          desc.type = (int)BExplorer.Shell.DataObject.DropImageType.None;
          desc.szMessage = "Cant drop here!";
          break;
        default:
          desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Invalid;
          desc.szMessage = "";
          break;
      }
      desc.szInsert = (sender as Wpf.Controls.TabItem).ShellObject.DisplayName;
      ((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);

      var ptw = new BExplorer.Shell.DataObject.Win32Point();
      GetCursorPos(ref ptw);

      if (e.Data.GetType() != typeof(Wpf.Controls.TabItem)) {
        var wpt = new BExplorer.Shell.DataObject.Win32Point() { X = ptw.X, Y = ptw.Y };
        DropTarget.Create.DragOver(ref wpt, (int)e.Effects);
      }
    }
    System.Runtime.InteropServices.ComTypes.IDataObject _TabDropData;
    void newt_DragEnter(object sender, DragEventArgs e) {
      e.Handled = true;
      this._CurrentlySelectedItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
      var tabItem = e.Source as Wpf.Controls.TabItem;
      if (tabItem == null)
        return;

      this._TabDropData = (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data;

      if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem)
        e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
      else
        e.Effects = DragDropEffects.None;

      var ptw = new BExplorer.Shell.DataObject.Win32Point();
      GetCursorPos(ref ptw);
      e.Effects = DragDropEffects.None;
      var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;

      if (tabItemSource == null) {
        var wpt = new BExplorer.Shell.DataObject.Win32Point() { X = ptw.X, Y = ptw.Y };
        DropTarget.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
      } else if (e.Data.GetDataPresent(typeof(Wpf.Controls.TabItem))) {
        e.Effects = DragDropEffects.Move;
      }
    }

    #endregion

    #region Tab Controls

    private void FolderTabs_Placement(object sender, RoutedEventArgs e) {
      if (sender == this.TabbaTop) {
        Grid.SetRow(this.tcMain, 3);
        this.divNav.Visibility = Visibility.Hidden;
        this.rTabbarTop.Height = new GridLength(25);
        this.rTabbarBot.Height = new GridLength(0);
        this.tcMain.TabStripPlacement = Dock.Top;
      } else {
        Grid.SetRow(this.tcMain, 7);
        this.divNav.Visibility = Visibility.Visible;
        this.rTabbarTop.Height = new GridLength(0);
        this.rTabbarBot.Height = new GridLength(25);
        this.tcMain.TabStripPlacement = Dock.Bottom;
      }
    }

    private void miSaveCurTabs_Click(object sender, RoutedEventArgs e) {
      var objs = new List<IListItemEx>(from Wpf.Controls.TabItem x in this.tcMain.Items select x.ShellObject);
      String str = Utilities.CombinePaths(objs, "|");
      var list = SaveTabs.CreateFromString(str);

      var Name = BetterExplorer.Tabs.NameTabList.Open(this);
      if (Name == null)
        return;

      if (!Directory.Exists(BESettings.SavedTabsDirectory))
        Directory.CreateDirectory(BESettings.SavedTabsDirectory);

      SaveTabs.SaveTabList(list, $"{BESettings.SavedTabsDirectory}{Name}.txt");
      this.miTabManager.IsEnabled = true;
    }

    private void miClearUndoList_Click(object sender, RoutedEventArgs e) {
      this.tcMain.ReopenableTabs.Clear();
      this.btnUndoClose.IsDropDownOpen = false;
      this.btnUndoClose.IsEnabled = false;

      foreach (var item in
          from Item in this.tcMain.Items.OfType<Wpf.Controls.TabItem>()
          from m in Item.mnu.Items.OfType<MenuItem>()
          where m.Tag?.ToString() == "UCTI"
          select m) {
        item.IsEnabled = false;
      }
    }

    private void btnUndoClose_DropDownOpened(object sender, EventArgs e) {
      this.rotGallery.Items.Clear();
      foreach (NavigationLog item in this.tcMain.ReopenableTabs) {
        var gli = new UndoCloseGalleryItem();
        gli.LoadData(item);
        gli.Click += this.gli_Click;
        this.rotGallery.Items.Add(gli);
      }
    }

    private void rotGallery_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      e.Handled = true;
      var Item = (e.AddedItems[0] as UndoCloseGalleryItem);
      Item?.PerformClickEvent();
    }

    private void btnChangeTabsFolder_Click(object sender, RoutedEventArgs e) {
      var ctf = new FolderSelectDialog();
      ctf.Title = "Change Tab Folder";
      ctf.InitialDirectory = new DirectoryInfo(BESettings.SavedTabsDirectory).Parent.FullName;
      if (ctf.ShowDialog()) {
        BESettings.SavedTabsDirectory = ctf.FileName + "\\";
        this.txtDefSaveTabs.Text = ctf.FileName + "\\";
      }
    }

    private void stGallery_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      e.Handled = true;
      if (e.AddedItems.Count > 0)
        (e.AddedItems[0] as SavedTabsListGalleryItem).PerformClickEvent();
    }

    private void btnSavedTabs_DropDownOpened(object sender, EventArgs e) {
      var o = new List<string>();

      if (Directory.Exists(BESettings.SavedTabsDirectory)) {
        foreach (string item in Directory.GetFiles(BESettings.SavedTabsDirectory)) {
          o.Add(Utilities.RemoveExtensionsFromFile(new ShellItem(item).GetDisplayName(SIGDN.NORMALDISPLAY), item.Substring(item.LastIndexOf("."))));
        }
      }

      this.stGallery.Items.Clear();
      foreach (string item in o) {
        var gli = new SavedTabsListGalleryItem(item);
        gli.Directory = BESettings.SavedTabsDirectory;
        gli.Click += this.gli_Click;
        gli.SetUpTooltip((this.FindResource("tabTabsCP") as string));
        this.stGallery.Items.Add(gli);
      }
    }

    private void miTabManager_Click(object sender, RoutedEventArgs e) {
      string sstdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\";
      if (Directory.Exists(sstdir))
        new Tabs.TabManager() { MainForm = this }.Show();
    }

    #endregion

    #region Recycle Bin
    BackgroundWorker rb = new BackgroundWorker();

    public void UpdateRecycleBinInfos() {
      var allDrives = Directory.GetLogicalDrives();
      int count = 0;// (int)sqrbi.i64NumItems;
      long size = 0;// sqrbi.i64Size;
                    //Task.Run(() => {
      foreach (var drive in allDrives) {
        var sqrbi = new BExplorer.Shell.Interop.Shell32.SHQUERYRBINFO() { cbSize = 24 };
        //char[] charsToTrim = { Char.Parse(@"\") };
        int hresult = BExplorer.Shell.Interop.Shell32.SHQueryRecycleBin(drive, ref sqrbi);
        count += (int)sqrbi.i64NumItems;
        size += (long)sqrbi.i64Size;
      }

      this.Dispatcher.Invoke(DispatcherPriority.Background,
              (Action)(() => {
                if (count > 0) {
                  this.miRestoreALLRB.Visibility = Visibility.Visible;
                  this.miEmptyRB.Visibility = Visibility.Visible;
                  this.btnRecycleBin.LargeIcon = @"..\Images\RecycleBinFull32.png";
                  this.btnRecycleBin.Icon = @"..\Images\RecycleBinFull16.png";
                  this.btnRecycleBin.UpdateLayout();
                  this.lblRBItems.Visibility = Visibility.Visible;
                  this.lblRBItems.Text = String.Format("{0} Items", count);
                  this.lblRBSize.Text = ShlWapi.StrFormatByteSize(size);
                  this.lblRBSize.Visibility = Visibility.Visible;

                } else {
                  this.miEmptyRB.Visibility = Visibility.Collapsed;
                  this.miRestoreALLRB.Visibility = Visibility.Collapsed;
                  this.miRestoreRBItems.Visibility = Visibility.Collapsed;
                  this.btnRecycleBin.LargeIcon = @"..\Images\RecycleBinEmpty32.png";
                  this.btnRecycleBin.Icon = @"..\Images\RecycleBinEmpty16.png";
                  this.lblRBItems.Text = "0 Items";
                  this.lblRBItems.Visibility = Visibility.Collapsed;
                  this.lblRBSize.Text = "0 bytes";
                  this.lblRBSize.Visibility = Visibility.Collapsed;
                }
              }));
    }

    private void miEmptyRB_Click(object sender, RoutedEventArgs e) {
      BExplorer.Shell.Interop.Shell32.SHEmptyRecycleBin(this.Handle, string.Empty, 0);
      this.UpdateRecycleBinInfos();
    }

    private void miOpenRB_Click(object sender, RoutedEventArgs e) {
      this.NavigationController(FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.RecycleBin).Pidl));
    }

    private void miRestoreRBItems_Click(object sender, RoutedEventArgs e) {
      foreach (var item in this._ShellListView.SelectedItems.ToArray()) {
        var recycler = new Shell().NameSpace(10);
        for (int i = 0; i < recycler.Items().Count; i++) {
          FolderItem FI = recycler.Items().Item(i);
          var fileName = recycler.GetDetailsOf(FI, 0);
          if (Path.GetExtension(fileName) == "")
            fileName += Path.GetExtension(FI.Path);

          // Necessary for systems with hidden file extensions.
          string FilePath = recycler.GetDetailsOf(FI, 1);
          if (item.FileSystemPath == Path.Combine(FilePath, fileName)) {
            DoVerb(FI, "ESTORE");
            break;
          }
        }
      }

      this.UpdateRecycleBinInfos();
    }

    private bool DoVerb(FolderItem Item, string Verb) {
      var Found = Item.Verbs().OfType<FolderItemVerb>().FirstOrDefault(FIVerb => FIVerb.Name.ToUpper().Contains(Verb.ToUpper()));
      Found?.DoIt();
      return Found != null;
    }

    private void miRestoreALLRB_Click(object sender, RoutedEventArgs e) {
      //TODO: Fix Unreachable Code
      Folder Recycler = new Shell().NameSpace(10);

      for (int i = 0; i < Recycler.Items().Count; i++) {
        FolderItem FI = Recycler.Items().Item(i);
        this.DoVerb(FI, "ESTORE");
        //TODO: Why do we have a break here
        break;
      }

      this.UpdateRecycleBinInfos();
    }

    #endregion

    #region Console
    private void ConsoleClear_Click(object sender, RoutedEventArgs e) => this.ctrlConsole.ClearConsole();
    void cmi_Click(object sender, RoutedEventArgs e) => this.ctrlConsole.EnqueleInput((sender as Fluent.MenuItem).Header.ToString());

    private void ctrlConsole_OnConsoleInput(object sender, Tuple<string> args) {
      if (args.Item1.Trim().ToLowerInvariant().StartsWith("cd"))
        this.NavigationController(FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, args.Item1.ToLowerInvariant().Replace("cd", String.Empty).Replace("/d", String.Empty).Trim()));

      var cmi = new MenuItem() { Header = args.Item1 };
      cmi.Click += this.cmi_Click;
      this.btnConsoleHistory.Items.Add(cmi);
    }
    #endregion

    #region EasyAccess

    private void btnEasyAccess_DropDownOpened(object sender, EventArgs e) {
      if (this._ShellListView.GetSelectedCount() == 1 && this._ShellListView.GetFirstSelectedItem().IsFolder) {
        this.mnuIncludeInLibrary.Items.Clear();

        foreach (ShellItem lib in KnownFolders.Libraries) {
          lib.Thumbnail.CurrentSize = new WIN.Size(16, 16);
          this.mnuIncludeInLibrary.Items.Add(Utilities.Build_MenuItem(
              lib.DisplayName, ShellLibrary.Load(Path.GetFileNameWithoutExtension(lib.ParsingName), true), lib.Thumbnail.BitmapSource, onClick: this.mli_Click)
          );
        }

        this.mnuIncludeInLibrary.Items.Add(new Separator());

        var mln = new MenuItem() { Header = "Create new library" };
        mln.Click += this.mln_Click;
        this.mnuIncludeInLibrary.Items.Add(mln);
        this.mnuIncludeInLibrary.IsEnabled = true;
      } else {
        this.mnuIncludeInLibrary.IsEnabled = false;
      }
    }

    void mln_Click(object sender, RoutedEventArgs e) {
      var lib = this._ShellListView.CreateNewLibrary(this._ShellListView.GetFirstSelectedItem().DisplayName);
      if (this._ShellListView.GetFirstSelectedItem().IsFolder)
        lib.Add(this._ShellListView.GetFirstSelectedItem().ParsingName);
    }

    void mli_Click(object sender, RoutedEventArgs e) {
      var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(((ShellItem)(sender as Fluent.MenuItem).Tag).ParsingName), false);
      if (this._ShellListView.GetFirstSelectedItem().IsFolder)
        lib.Add(this._ShellListView.GetFirstSelectedItem().ParsingName);
    }

    #endregion

    #region ShellListView

    void ShellListView_EndItemLabelEdit(object sender, bool e) {
      //_ShellListView.FileNameChangeAttempt(this.txtEditor.Text, e);
      //this.Editor.Visibility = WIN.Visibility.Collapsed;
      //this.Editor.IsOpen = false;
    }

    void ShellListView_BeginItemLabelEdit(object sender, RenameEventArgs e) {
      //if (this.Editor.IsOpen) return;
      //var isSmall = this._ShellListView.IconSize == 16;
      //if (isSmall) {
      //	this.txtEditor.TextWrapping = TextWrapping.WrapWithOverflow;
      //	this.txtEditor.TextAlignment = TextAlignment.Left;
      //} else {
      //	this.txtEditor.TextWrapping = TextWrapping.Wrap;
      //	this.txtEditor.TextAlignment = TextAlignment.Center;
      //}

      //var itemRect = this._ShellListView.GetItemBounds(e.ItemIndex, 0);
      //var itemLabelRect = this._ShellListView.GetItemBounds(e.ItemIndex, 2);
      //this.txtEditor.Text = this._ShellListView.Items[e.ItemIndex].DisplayName;
      //var point = this.ShellViewHost.PointToScreen(new WIN.Point(isSmall ? itemLabelRect.Left : itemRect.Left, itemLabelRect.Top - (isSmall ? 1 : 0)));
      //this.Editor.HorizontalOffset = point.X;
      //this.Editor.VerticalOffset = point.Y;

      //this.txtEditor.MaxWidth = isSmall ? Double.PositiveInfinity : itemRect.Width;
      //this.txtEditor.MaxHeight = isSmall ? itemLabelRect.Height + 2 : Double.PositiveInfinity;

      //this.Editor.Width = isSmall ? this.txtEditor.Width : itemRect.Width;
      //this.Editor.Height = this.txtEditor.Height + 2;
      //this.Editor.Visibility = WIN.Visibility.Visible;
      //this.Editor.IsOpen = true;
      //this.txtEditor.Focus();

      //var isCheckedExtensions = this.chkExtensions.IsChecked;
      //if (isCheckedExtensions != null && (isCheckedExtensions.Value & this.txtEditor.Text.Contains(".") && !this._ShellListView.GetFirstSelectedItem().IsFolder)) {
      //	var lastIndexOfDot = this.txtEditor.Text.LastIndexOf(".", StringComparison.Ordinal);
      //	this.txtEditor.SelectionStart = 0;
      //	this.txtEditor.SelectionLength = lastIndexOfDot;
      //} else {
      //	this.txtEditor.SelectAll();
      //}

      //Keyboard.Focus(this.txtEditor);
    }

    void ShellListView_Navigating(object sender, NavigatingEventArgs e) {
      if (this._ShellListView.CurrentFolder == null)
        return;
      if (e.IsFirstItemAvailable) {
        this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) (() => {
          if (this.bcbc.RootItem.Items.OfType<ShellItem>().Last().IsSearchFolder) {
            this.bcbc.RootItem.Items.RemoveAt(this.bcbc.RootItem.Items.Count - 1);
          }


          var pidl = e.Folder.PIDL.ToString();
          this.bcbc.SetPathWithoutNavigate(pidl);
        }));


        this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) (() => {
          var tab = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
          if (tab != null && this._ShellListView.GetSelectedCount() > 0) {
            if (tab.SelectedItems != null)
              tab.SelectedItems.AddRange(this._ShellListView.SelectedItems.Select(s => s.ParsingName).ToList());
            else
              tab.SelectedItems = this._ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
          }

          this.Title = "Better Explorer - " + e.Folder.DisplayName;
        }));

        if (e.Folder.IsSearchFolder) {
          this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) (() => {
            var selectedTabItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
            if (selectedTabItem != null) {
              selectedTabItem.Header = e.Folder.DisplayName;
              selectedTabItem.Icon = e.Folder.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
              selectedTabItem.ShellObject = e.Folder;
              selectedTabItem.ToolTip = e.Folder.ParsingName.Replace("%20", " ").Replace("%3A", ":").Replace("%5C", @"\");
            }
          }));
        } else {
          this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) (() => { this.edtSearchBox.ClearSearchText(); }));
        }
      } else {
        this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action) (() => {
          this.btnCancelNavigation.Visibility = Visibility.Visible;
          this.btnGoNavigation.Visibility = Visibility.Collapsed;
          this._ProgressTimer.Start();
        }));
        this._ShellListView.Focus();
      }
    }

    void ShellTree_NodeClick(object sender, WIN.Forms.TreeNodeMouseClickEventArgs e) {
      if (e.Button == WIN.Forms.MouseButtons.Middle) {
        var item = e.Node?.Tag as IListItemEx;
        if ((item?.IsLink).Value) {
          var shellLink = new ShellLink(item.ParsingName);
          item = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, shellLink.TargetPIDL);
          shellLink.Dispose();
        }

        if (item != null)
          this.tcMain.NewTab(item, false);
      }
    }

    WIN.Forms.Timer _keyjumpTimer = new WIN.Forms.Timer();

    void ShellListView_KeyJumpTimerDone(object sender, EventArgs e) { this._keyjumpTimer?.Stop(); this._keyjumpTimer?.Start(); }

    void _keyjumpTimer_Tick(object sender, EventArgs e) {
      //key jump done
      this.KeyJumpGrid.IsOpen = false;
      var timer = sender as WIN.Forms.Timer;
      timer?.Stop();
    }

    void ShellListView_KeyJumpKeyDown(object sender, WIN.Forms.KeyEventArgs e) {
      //add key for key jump
      this.KeyJumpGrid.IsOpen = true;
      this.txtKeyJump.Text = this._ShellListView.KeyJumpString;
    }

    void ShellListView_ColumnHeaderRightClick(object sender, WIN.Forms.MouseEventArgs e) {
      //is where the more columns menu should be added
      this.chcm.IsOpen = true;
    }

    void ShellListView_ItemUpdated(object sender, ItemUpdatedEventArgs e) {
      this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => {
        if (e.UpdateType == ItemUpdateType.RecycleBin) {
          this.UpdateRecycleBinInfos();
        }
        if (e.UpdateType != ItemUpdateType.Renamed && e.UpdateType != ItemUpdateType.Updated) {
          var itemsCount = this._ShellListView.Items.Count;
          this.sbiItemsCount.Visibility = itemsCount == 0 ? Visibility.Collapsed : Visibility.Visible;
          this.sbiItemsCount.Content = itemsCount == 1 ? "1 item" : itemsCount + " items";
        }
        //if (e.UpdateType == ItemUpdateType.Created && this._ShellListView.IsRenameNeeded) {
        //	_ShellListView.SelectItemByIndex(e.NewItemIndex, true, true);
        //	_ShellListView.RenameSelectedItem();
        //	this._ShellListView.IsRenameNeeded = false;
        //}
        if (e.UpdateType == ItemUpdateType.DriveRemoved || (e.UpdateType == ItemUpdateType.Deleted && e.NewItem.IsFolder)) {
          foreach (var tab in this.tcMain.Items.OfType<Wpf.Controls.TabItem>().ToArray().Where(w => w.ShellObject.ParsingName.StartsWith(e.NewItem.ParsingName))) {
            this.tcMain.RemoveTabItem(tab, false);
          }
        }

        this._ShellListView.Focus();
      }));
    }

    void ShellListView_SelectionChanged(object sender, EventArgs e) {
      if (!this._ShellListView.IsNavigationInProgress && !this._ShellListView.IsSearchNavigating)
        this.SetupUIOnSelectOrNavigate();
      if (BESettings.IsInfoPaneEnabled)
        Task.Run(() => this.DetailsPanel.FillPreviewPane(this._ShellListView));
      this.SetUpStatusBarOnSelectOrNavigate(this._ShellListView.GetSelectedCount());
    }

    #endregion

    #region On Navigated

    void ShellListView_Navigated(object sender, NavigatedEventArgs e) {
      //return;
      this._ProgressTimer.Stop();
      this.btnCancelNavigation.Visibility = Visibility.Collapsed;
      this.btnGoNavigation.Visibility = Visibility.Visible;
      this.SetupUIOnSelectOrNavigate();

      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        this.SetupColumnsButton();
        this.SetSortingAndGroupingButtons();

        if (!this.tcMain.isGoingBackOrForward) {
          var Current = (this.tcMain.SelectedItem as Wpf.Controls.TabItem).log;
          Current.ClearForwardItems();
          if (Current.CurrentLocation != e.Folder)
            Current.CurrentLocation = e.Folder;
        }

        this.tcMain.isGoingBackOrForward = false;
        this.SetupUIonNavComplete(e);

        if (Settings.BESettings.IsConsoleShown)
          this.ctrlConsole.ChangeFolder(e.Folder.ParsingName, e.Folder.IsFileSystem);
      }));

      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        this.ConstructMoveToCopyToMenu();
        this.SetUpJumpListOnNavComplete();
        this.SetUpButtonVisibilityOnNavComplete(this.SetUpNewFolderButtons());
      }));

      if (BESettings.IsInfoPaneEnabled) {
        Task.Run(() => {
          this.DetailsPanel.FillPreviewPane(this._ShellListView);
        });
      }

      this.Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() => {
        this._ShellListView.Focus(false, true);
        var selectedItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
        if (selectedItem == null) {
          this.tcMain.SelectedItem = this.tcMain.Items.OfType<Wpf.Controls.TabItem>().Last();
          selectedItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
        }
        var oldCurrentItem = selectedItem.ShellObject;
        var curentFolder = this._ShellListView.CurrentFolder.Clone();
        selectedItem.Header = this._ShellListView.CurrentFolder.DisplayName;
        selectedItem.Icon = curentFolder.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
        selectedItem.ShellObject = this._ShellListView.CurrentFolder;
        if (selectedItem != null) {
          var selectedPaths = selectedItem.SelectedItems;
          if (selectedPaths?.Count > 0) {
            foreach (var path in selectedPaths.ToArray()) {
              var sho = this._ShellListView.Items.FirstOrDefault(w => w.ParsingName == path);
              if (sho != null) {
                var index = sho.ItemIndex;
                this._ShellListView.SelectItemByIndex(index, path.Equals(selectedPaths.Last(), StringComparison.InvariantCultureIgnoreCase) || this.IsNeedEnsureVisible);
                this.IsNeedEnsureVisible = false;
                selectedPaths.Remove(path);
              }
            }
          } else {
            var realItem = this._ShellListView.Items.ToArray().FirstOrDefault(w => w.GetUniqueID() == oldCurrentItem.GetUniqueID());
            if (realItem != null) {
              this._ShellListView.SelectItems(new[] { realItem }, true);
            } else {
              if (!curentFolder.ParsingName.Contains(oldCurrentItem.ParsingName)) {
                var parents = new List<IListItemEx>();
                var parent = oldCurrentItem.Parent;
                while (parent != null) {
                  parents.Add(parent);
                  realItem = this._ShellListView.Items.ToArray().FirstOrDefault(w => w.GetUniqueID() == parent.GetUniqueID());
                  if (realItem != null) {
                    this._ShellListView.SelectItems(new[] { realItem }, true);
                    break;
                  }
                  parent = parent.Parent;
                }
              }
            }

            //this._ShellListView.ScrollToTop();
          }
        }
        oldCurrentItem.Dispose();
        curentFolder.Dispose();
      }));

      ////This initially setup the statusbar after program opens
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        this.SetUpStatusBarOnSelectOrNavigate(this._ShellListView.SelectedItems == null ? 0 : this._ShellListView.GetSelectedCount());
      }));

      //Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) (() => {
      //    Thread.Sleep(1500);
      if (this.bcbc.ProgressValue > 2)
        this.bcbc.SetProgressValue(this.bcbc.ProgressMaximum, TimeSpan.FromMilliseconds(750));
      else
        this.bcbc.SetProgressValue(0, TimeSpan.FromSeconds(0));
      this.bcbc.ProgressMaximum = 100;
      //this.bcbc.SetProgressValue(0, TimeSpan.FromSeconds(0));
      //}));


    }

    /// <summary>
    /// Sets up the UI after the ShellListView has navigated
    /// </summary>
    /// <param name="e"></param>
    private void SetupUIonNavComplete(NavigatedEventArgs e) {
      this.btnSizeChart.IsEnabled = e.Folder.IsFileSystem;
      this.btnAutosizeColls.IsEnabled = this._ShellListView.View == ShellViewStyle.Details;

      if (e.Folder.ParsingName != KnownFolders.RecycleBin.ParsingName) {
        this.miRestoreALLRB.Visibility = Visibility.Collapsed;
      } else if (this._ShellListView.Items.Any()) {
        this.miRestoreALLRB.Visibility = Visibility.Visible;
      }

      bool isFuncAvail;
      int selectedItemsCount = this._ShellListView.GetSelectedCount();
      if (selectedItemsCount == 1) {
        isFuncAvail = this._ShellListView.GetFirstSelectedItem().IsFileSystem || this._ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName;
      } else {
        isFuncAvail = true;
        if (!(this._ShellListView.CurrentFolder.IsFolder && !this._ShellListView.CurrentFolder.IsDrive && !this._ShellListView.CurrentFolder.IsSearchFolder))
          this.ctgFolderTools.Visibility = Visibility.Collapsed;
      }

      bool IsChanged = selectedItemsCount > 0;
      this.btnCopy.IsEnabled = IsChanged;
      this.btnCut.IsEnabled = IsChanged;
      this.btnRename.IsEnabled = IsChanged;
      this.btnDelete.IsEnabled = IsChanged && isFuncAvail;
      this.btnCopyto.IsEnabled = IsChanged;
      this.btnMoveto.IsEnabled = IsChanged;
      this.btnSelNone.IsEnabled = IsChanged;

      this.leftNavBut.IsEnabled = (this.tcMain.SelectedItem as Wpf.Controls.TabItem).log.CanNavigateBackwards;
      this.rightNavBut.IsEnabled = (this.tcMain.SelectedItem as Wpf.Controls.TabItem).log.CanNavigateForwards;
      this.btnUpLevel.IsEnabled = this._ShellListView.CanNavigateParent;
    }

    /// <summary>
    /// Adds the current folder to the recent category of the Better Exlorer's <see cref="JumpList">JumpList</see>
    /// </summary>
    private void SetUpJumpListOnNavComplete() {
      var pidl = IntPtr.Zero;

      try {
        pidl = this._ShellListView.CurrentFolder.AbsolutePidl;
        var sfi = new SHFILEINFO();

        if (pidl != IntPtr.Zero && !this._ShellListView.CurrentFolder.IsFileSystem) {
          var res = BExplorer.Shell.Interop.Shell32.SHGetFileInfo(pidl, 0, out sfi, Marshal.SizeOf(sfi), SHGFI.IconLocation | SHGFI.SmallIcon | SHGFI.PIDL);
        }

        if (this._ShellListView.CurrentFolder.IsFileSystem) {
          BExplorer.Shell.Interop.Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(this._ShellListView.CurrentFolder.ParsingName), 0, out sfi, Marshal.SizeOf(sfi), SHGFI.IconLocation | SHGFI.SmallIcon);
        }

        JumpList.AddToRecentCategory(new JumpTask() {
          ApplicationPath = Process.GetCurrentProcess().MainModule.FileName,
          Arguments = "\"" + this._ShellListView.CurrentFolder.ParsingName + "\"",
          Title = this._ShellListView.CurrentFolder.DisplayName,
          IconResourcePath = sfi.szDisplayName,
          IconResourceIndex = sfi.iIcon
        });

        this.AppJL.Apply();
      } finally {
        if (pidl != IntPtr.Zero)
          Marshal.FreeCoTaskMem(pidl);
      }
    }

    private bool SetUpNewFolderButtons() {
      var currentFolder = FileSystemListItem.ToFileSystemItem(this._ShellListView.CurrentFolder.ParentHandle, this._ShellListView.CurrentFolder.PIDL);
      if (currentFolder.Parent == null) {
        return false;
      } else if (currentFolder.ParsingName == KnownFolders.Libraries.ParsingName) {
        this.btnCreateFolder.Header = this.FindResource("btnNewLibraryCP");  //"New Library";
        this.stNewFolder.Title = this.FindResource("btnNewLibraryCP").ToString();//"New Library";
        this.stNewFolder.Text = "Creates a new library in the current folder.";
        this.stNewFolder.Image = new BitmapImage(new Uri(@"/BetterExplorer;component/Images/newlib32.png", UriKind.Relative));
        this.btnCreateFolder.LargeIcon = @"..\Images\newlib32.png";
        this.btnCreateFolder.Icon = @"..\Images\newlib16.png";

        return true;
      } else if (currentFolder.IsFileSystem || currentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
        this.btnCreateFolder.Header = this.FindResource("btnNewFolderCP");//"New Folder";
        this.stNewFolder.Title = this.FindResource("btnNewFolderCP").ToString(); //"New Folder";
        this.stNewFolder.Text = "Creates a new folder in the current folder";
        this.stNewFolder.Image = new BitmapImage(new Uri(@"/BetterExplorer;component/Images/folder_new32.png", UriKind.Relative));
        this.btnCreateFolder.LargeIcon = @"..\Images\folder_new32.png";
        this.btnCreateFolder.Icon = @"..\Images\folder_new16.png";

        return false;
      } else {
        return false;
      }
    }

    private void SetUpButtonVisibilityOnNavComplete(bool isinLibraries) {
      if (this._ShellListView.CurrentFolder.ParsingName.Contains(KnownFolders.Libraries.ParsingName) && this._ShellListView.CurrentFolder.ParsingName != KnownFolders.Libraries.ParsingName) {
        if (this._ShellListView.GetSelectedCount() == 1) {
          this.ctgLibraries.Visibility = Visibility.Visible;
          this.SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(this._ShellListView.CurrentFolder.ParsingName), false));
        }
        this.ctgFolderTools.Visibility = Visibility.Collapsed;
        this.ctgImage.Visibility = Visibility.Collapsed;
        this.ctgArchive.Visibility = Visibility.Collapsed;
        this.ctgVirtualDisk.Visibility = Visibility.Collapsed;
        this.ctgExe.Visibility = Visibility.Collapsed;
      } else if (!this._ShellListView.CurrentFolder.ParsingName.ToLowerInvariant().EndsWith("library-ms")) {
        this.btnDefSave.Items.Clear();
        this.ctgLibraries.Visibility = Visibility.Collapsed;
      }

      this.ctgDrive.Visibility = this._ShellListView.CurrentFolder.IsDrive ? Visibility.Visible : Visibility.Collapsed;

      if (isinLibraries) {
        this.ctgFolderTools.Visibility = Visibility.Collapsed;
      }
    }

    /// <summary>
    /// Navigates to the <paramref name="Destination"/> When Destination != Current Folder
    /// </summary>
    /// <param name="Destination">The folder you want to navigate to</param>
    private void NavigationController(IListItemEx Destination) {
      if (!Destination.Equals(this._ShellListView.CurrentFolder))
        this._ShellListView.Navigate_Full(Destination, true);
    }

    private void OnBreadcrumbbarNavigate(IListItemEx Destination) {
      this.IsNeedEnsureVisible = true;
      this.NavigationController(Destination);
    }

    #endregion

    #region Misc

    public MainWindow() {
      this.Badges = this.LoadBadgesData();
      this.DataContext = this;

      this.LVItemsColorCol = new ObservableCollectionEx<LVItemColor>();
      this.CommandBindings.AddRange(new[] {
            new CommandBinding(AppCommands.RoutedNavigateBack, this.leftNavBut_Click),
            new CommandBinding(AppCommands.RoutedNavigateFF, this.rightNavBut_Click),
            new CommandBinding(AppCommands.RoutedNavigateUp, this.btnUpLevel_Click),
            new CommandBinding(AppCommands.RoutedGotoSearch, this.GoToSearchBox),
            new CommandBinding(AppCommands.RoutedNewTab, (sender, e) => this.tcMain.NewTab()),
            new CommandBinding(AppCommands.RoutedEnterInBreadCrumbCombo, (sender, e) => { this._ShellListView.IsFocusAllowed = false; this.bcbc.SetInputState(); }),
            new CommandBinding(AppCommands.RoutedChangeTab, (sender, e) => {
                int selIndex = this.tcMain.SelectedIndex == this.tcMain.Items.Count - 1 ? 0 : this.tcMain.SelectedIndex + 1;
                this.tcMain.SelectedItem = this.tcMain.Items[selIndex];
            }),
            new CommandBinding(AppCommands.RoutedCloseTab, (sender, e) => {
                if (this.tcMain.SelectedIndex == 0 && this.tcMain.Items.Count == 1) {
                    this.Close();
                    return;
                }
                int CurSelIndex = this.tcMain.SelectedIndex;
                this.tcMain.SelectedItem = this.tcMain.SelectedIndex == 0 ? this.tcMain.Items[1] : this.tcMain.Items[CurSelIndex - 1];
                this.tcMain.Items.RemoveAt(CurSelIndex);
            })
        });

      // loads current Ribbon color theme
      try {
        switch (Settings.BESettings.CurrentTheme) {
          case "Dark":
            ThemeManager.ChangeAppTheme(Application.Current, "BaseDark");
            break;
          default:
            ThemeManager.ChangeAppTheme(Application.Current, "BaseLight");
            break;
        }
      } catch (Exception ex) {
        MessageBox.Show($"An error occurred while trying to load the theme data from the Registry. \n\r \n\r{ex.Message}\n\r \n\rPlease let us know of this issue at http://bugs.gainedge.org/public/betterexplorer", "RibbonTheme Error - " + ex.ToString());
      }

      // loads current UI language (uses en-US if default)
      try {
        //load current UI language in case there is no specified registry value
        ((App)Application.Current).SelectCulture(BESettings.Locale, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\translation.xaml");
      } catch (Exception ex) {
        MessageBox.Show($"An error occurred while trying to load the locale data from the Registry. \n\r \n\r{ex.Message}\n\r \n\rPlease let us know of this issue at http://bugs.gainedge.org/public/betterexplorer", "Locale Load Error - " + ex);
      }

      // gets values from registry to be applied after initialization
      //bool rtlset = Convert.ToString(rks.GetValue("RTLMode", "notset")) != "notset";

      var rtlMode = BESettings.RTLMode;

      //Main Initialization routine
      this.InitializeComponent();

      // sets up ComboBox to select the current UI language
      this.TranslationComboBox.SelectedItem = this.TranslationComboBox.Items.OfType<TranslationComboBoxItem>().FirstOrDefault(x => x.LocaleCode == BESettings.Locale);

      if (!rtlMode) {
        rtlMode = (this.TranslationComboBox.SelectedItem as TranslationComboBoxItem).UsesRTL;
      }

      this.SearchBarColumn.Width = new GridLength(BESettings.SearchBarWidth);

      // prepares RTL mode
      this.FlowDirection = rtlMode ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

      // sets tab bar alignment
      if (BESettings.TabBarAlignment == "top")
        this.TabbaTop.IsChecked = true;
      else if (BESettings.TabBarAlignment == "bottom")
        this.TabbaBottom.IsChecked = true;
      else
        this.TabbaTop.IsChecked = true;

      // allows user to change language
    }

    private void beNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {
      this.Visibility = Visibility.Visible;
      if (this.WindowState == WindowState.Minimized) {
        User32.ShowWindow(this.Handle, User32.ShowWindowCommands.Restore);
      }

      this.Activate();
      this.Topmost = true;  // important
      this.Topmost = false; // important
      this.Focus();         // important
    }

    void ShellListView_ViewStyleChanged(object sender, BExplorer.Shell.ViewChangedEventArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
          (ThreadStart)(() => {
            this._IsShouldRiseViewChanged = false;
            if (e.ThumbnailSize <= 48 && e.CurrentView != ShellViewStyle.Medium) {
              this.zoomSlider.IsEnabled = false;
            } else {
              this.zoomSlider.Value = e.ThumbnailSize;
              this.zoomSlider.IsEnabled = true;
            }

            this.btnAutosizeColls.IsEnabled = e.CurrentView == ShellViewStyle.Details;
            this.btnSbTiles.IsChecked = e.CurrentView == ShellViewStyle.Tile;

            this.ViewGallery.SelectedIndex = -1;

            if (e.CurrentView == ShellViewStyle.ExtraLargeIcon && e.ThumbnailSize == 256) {
              this.ViewGallery.SelectedIndex = 0;
            } else if (e.CurrentView == ShellViewStyle.LargeIcon && e.ThumbnailSize == 96) {
              this.ViewGallery.SelectedIndex = 1;
            } else if (e.CurrentView == ShellViewStyle.Medium && e.ThumbnailSize == 48) {
              this.ViewGallery.SelectedIndex = 2;
              this.btnSbIcons.IsChecked = true;
            } else if (e.CurrentView == ShellViewStyle.SmallIcon) {
              this.ViewGallery.SelectedIndex = 3;
            } else {
              this.btnSbIcons.IsChecked = false;
            }

            if (e.CurrentView == ShellViewStyle.List) {
              this.ViewGallery.SelectedIndex = 4;
            } else if (e.CurrentView == ShellViewStyle.Details) {
              this.ViewGallery.SelectedIndex = 5;
              this.btnSbDetails.IsChecked = true;
            } else {
              this.btnSbDetails.IsChecked = false;
            }

            if (e.CurrentView == ShellViewStyle.Tile) {
              this.ViewGallery.SelectedIndex = 6;
            } else if (e.CurrentView == ShellViewStyle.Content) {
              this.ViewGallery.SelectedIndex = 7;
            } else if (e.CurrentView == ShellViewStyle.Thumbstrip) {
              this.ViewGallery.SelectedIndex = 8;
            }
            this._IsShouldRiseViewChanged = true;
          }));
    }

    private void btnNewItem_DropDownOpened(object sender, EventArgs e) {
      var mnu = new ShellContextMenu(this._ShellListView, true);

      var controlPos = this.btnNewItem.TransformToAncestor(Application.Current.MainWindow).Transform(new WIN.Point(0, 0));
      var tempPoint = this.PointToScreen(new WIN.Point(controlPos.X, controlPos.Y));
      mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)this.btnNewItem.ActualHeight));
      this.btnNewItem.IsDropDownOpen = false;
    }

    private void mnuPinToStart_Click(object sender, RoutedEventArgs e) {
      if (this._ShellListView.GetSelectedCount() == 1) {
        string loc = KnownFolders.StartMenu.ParsingName + @"\" + this._ShellListView.GetFirstSelectedItem().DisplayName + ".lnk";
        var link = new ShellLink();
        link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
        link.Target = this._ShellListView.GetFirstSelectedItem().ParsingName;
        link.Save(loc);
        link.Dispose();

        User32.PinUnpinToStartMenu(loc);
      }

      if (this._ShellListView.GetSelectedCount() == 0) {
        string loc = KnownFolders.StartMenu.ParsingName + @"\" + this._ShellListView.CurrentFolder.DisplayName + ".lnk";
        ShellLink link = new ShellLink();
        link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
        link.Target = this._ShellListView.CurrentFolder.ParsingName;
        link.Save(loc);
        link.Dispose();

        User32.PinUnpinToStartMenu(loc);
      }
    }

    private void tmpButtonB_Click(object sender, RoutedEventArgs e) => MessageBox.Show("This button currently does nothing");

    private void RibbonWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (!Keyboard.IsKeyDown(Key.LeftAlt) && (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)) {
        e.Handled = true;
        var keyText = String.Empty;
        switch (e.Key) {
          case Key.Left:
            keyText = "{LEFT}";
            break;
          case Key.Right:
            keyText = "{RIGHT}";
            break;
          case Key.Up:
            keyText = "{UP}";
            break;
          case Key.Down:
            keyText = "{DOWN}";
            break;
        }

        WIN.Forms.SendKeys.SendWait(keyText);
      }
    }

    private void ToolBar_SizeChanged(object sender, SizeChangedEventArgs e) {
      var toolBar = sender as ToolBar;
      var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
      if (overflowGrid != null)
        overflowGrid.Visibility = toolBar.HasOverflowItems ? Visibility.Visible : Visibility.Collapsed;

      var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
      if (mainPanelBorder != null)
        mainPanelBorder.Margin = toolBar.HasOverflowItems ? new Thickness(0, 0, 11, 0) : new Thickness(0);
    }

    private void btnPaypal_Click(object sender, RoutedEventArgs e) {
      string url = "";
      string business = "dimitarcenevjp@gmail.com";       // your PayPal email
      string description = "Donation%20for%20Better%20Explorer";  // '%20' represents a space. remember HTML!
      string country = "US";                    // AU, US, etc.
      string currency = "USD";                  // AUD, USD, etc.

      url = $"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business={business}&lc={country}&item_name={description}&currency_code={currency}&bn=PP%2dDonationsBF";

      Process.Start(url);
    }

    private void btnTest_Click(object sender, RoutedEventArgs e) {
      /*
    We could easily move this to another project and send that method			 
    */

      //Following could be an example of what the most basic plugin could look like
      //We could also separate plugins so they could be enabled WHEN
      //Always OR Folder_Selected OR File_Selected 
      Action<string, string> pluginExampleActivateBasic = (string pluginPath, string currentFileOrFolder) => Process.Start(pluginPath, currentFileOrFolder);

      var Tab = new Fluent.RibbonTabItem() { Header = "Plugins", ToolTip = "Plugins" };
      this.TheRibbon.Tabs.Add(Tab);
      var groupBox1 = new RibbonGroupBox() { Header = "Test" };
      Tab.Groups.Add(groupBox1);
      var XML =
                                      @"<Shortcuts>
            <Shortcut Name='Test' Path = 'C:\Aaron'/>
          </Shortcuts>";

      var xDoc = XElement.Parse(XML);
      var shortcuts = xDoc.Elements("Shortcut");

      var dropDown = new SplitButton();
      groupBox1.Items.Add(dropDown);

      foreach (var Node in xDoc.Elements("Shortcut")) {
        var item = new MenuItem() { Header = Node.Attribute("Name").Value };
        item.Click += (x, y) => Process.Start(Node.Attribute("Path").Value);
        dropDown.Items.Add(item);
      }
    }

    private void tcMain_Setup(object sender, RoutedEventArgs e) {
      this.tcMain.newt_DragEnter = this.newt_DragEnter;
      this.tcMain.newt_DragOver = this.newt_DragOver;
      this.tcMain.newt_Drop = this.newt_Drop;
      this.tcMain.newt_Leave = this.newt_Leave;
      this.tcMain.newt_GiveFeedback = this.newt_GiveFeedback;
      this.tcMain.newt_PreviewMouseMove = this.newt_PreviewMouseMove;
      this.tcMain.newt_PreviewMouseDown = this.newt_PreviewMouseDown;
      this.tcMain.ConstructMoveToCopyToMenu += this.ConstructMoveToCopyToMenu;
      this.tcMain.DefaultTabPath = this.tcMain.StartUpLocation.ToShellParsingName();
      this.tcMain.PreviewSelectionChanged += this.tcMain_PreviewSelectionChanged;
      //    tcMain.StartUpLocation = Utilities.GetRegistryValue("StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();
      //    if (tcMain.StartUpLocation == "") {
      //BESettings.StartupLocation = KnownFolders.Libraries.ParsingName;
      //BESettings.SaveSettings();
      //      tcMain.StartUpLocation = KnownFolders.Libraries.ParsingName;
      //    }

      this.tcMain.StartUpLocation = BESettings.StartupLocation;
    }

    void tcMain_PreviewSelectionChanged(object sender, Wpf.Controls.PreviewSelectionChangedEventArgs e) {
      //if (tcMain.IsInTabDragDrop) {
      //	e.Cancel = true;
      //	return;
      //}

      //if (e.RemovedItems.Count > 0) {
      //	var tab = e.RemovedItems[0] as Wpf.Controls.TabItem;

      //	if (tab != null && this._ShellListView.GetSelectedCount() > 0) {
      //		tab.SelectedItems = this._ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
      //	}
      //}

      //if (e.AddedItems.Count == 0 || tcMain.SelectNewTabOnCreate == false) return;
      //tcMain.IsInTabDragDrop = true;
      //var newTab = e.AddedItems[0] as Wpf.Controls.TabItem;
      //if (this._ShellListView.CurrentFolder == null || !this._ShellListView.CurrentFolder.Equals(newTab.ShellObject) && tcMain.CurrentTabItem == null) {
      //	SelectTab(newTab);
      //} else if (!tcMain.IsSelectionHandled) {
      //	SelectTab(newTab);
      //	//btnUndoClose
      //	btnUndoClose.Items.Clear();
      //	foreach (var item in tcMain.ReopenableTabs) {
      //		btnUndoClose.Items.Add(item.CurrentLocation);
      //	}
      //}
      ////else if (e.RemovedItems.Count == 0) {
      ////	e.Cancel = true;
      ////	SelectTab(newTab);
      ////	tcMain.SelectedItem = e.AddedItems[0];
      ////} else if (e.RemovedItems[0] == tcMain.CurrentTabItem) {
      ////	e.Cancel = true;
      ////	tcMain.IsSelectionHandled = false;
      ////	tcMain.SelectedItem = e.RemovedItems[0];
      ////	tcMain.CurrentTabItem = null;
      ////}

      //tcMain.IsSelectionHandled = true;
      //this._ShellListView.Focus();
      //this._CurrentlySelectedItem = tcMain.SelectedItem as Wpf.Controls.TabItem;
    }

    private void newt_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
      e.Handled = true;
      e.UseDefaultCursors = true;
      if (this._TabDropData == null)
        return;
      var doo = new WIN.Forms.DataObject(this._TabDropData);
      if (doo.GetDataPresent("DragWindow")) {
        IntPtr hwnd = ShellView.GetIntPtrFromData(doo.GetData("DragWindow"));
        User32.PostMessage(hwnd, 0x403, IntPtr.Zero, IntPtr.Zero);
      } else {
        e.UseDefaultCursors = true;
      }

      if (ShellView.IsDropDescriptionValid(this._TabDropData)) {
        e.UseDefaultCursors = false;
        this.Cursor = Cursors.Arrow;
      } else {
        e.UseDefaultCursors = true;
      }

      if (ShellView.IsShowingLayered(doo)) {
        e.UseDefaultCursors = false;
        this.Cursor = Cursors.Arrow;
      } else {
        e.UseDefaultCursors = true;
      }
    }

    private void btnResetLibrary_Click(object sender, RoutedEventArgs e) => WIN.Forms.MessageBox.Show("This does nothing"); //TODO: Add functionality or remove

    private void RibbonWindow_Activated(object sender, EventArgs e) {
      //tcMain.CurrentTabItem = tcMain.SelectedItem as Wpf.Controls.TabItem;
      this.tcMain.IsSelectionHandled = true;
      this.focusTimer.Start();
    }

    private void RibbonWindow_StateChanged(object sender, EventArgs e) {
      //tcMain.CurrentTabItem = tcMain.SelectedItem as Wpf.Controls.TabItem;
      this.tcMain.IsSelectionHandled = true;
      //if (this.WindowState != WindowState.Minimized && this.IsActive) focusTimer.Start();
    }

    void focusTimer_Tick(object sender, EventArgs e) {
      this._ShellListView.Focus();
      this.focusTimer.Stop();
    }

    private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      var badgeIconPath = ((sender as Image)?.Tag as IListItemEx)?.ParsingName;
      var selectedItemPath = this._ShellListView.GetFirstSelectedItem().ParsingName;
      this.SaveBadgeForItem(selectedItemPath, badgeIconPath);
      this.Badges = this.LoadBadgesData();
      this._ShellListView.BadgesData = this.Badges;
      this._ShellListView.RefreshItem(this._ShellListView.GetFirstSelectedItemIndex(), true);
      this.btnBadges.IsDropDownOpen = false;
    }

    private void SaveBadgeForItem(String itemPath, String badgePath, Boolean isClear = false) {
      var m_dbConnection = new System.Data.SQLite.SQLiteConnection("Data Source=" + this._DBPath + ";Version=3;");
      m_dbConnection.Open();
      if (isClear) {
        var command3 = new System.Data.SQLite.SQLiteCommand("DELETE FROM badges WHERE Path=@Path", m_dbConnection);
        command3.Parameters.AddWithValue("Path", itemPath);
        command3.ExecuteNonQuery();
      } else {
        var command1 = new System.Data.SQLite.SQLiteCommand("SELECT * FROM badges WHERE Path=@Path", m_dbConnection);
        command1.Parameters.AddWithValue("Path", itemPath);
        var Reader = command1.ExecuteReader();
        var sql = Reader.Read()
        ? @"UPDATE badges  SET Collection = @Collection, Badge = @Badge	 WHERE Path = @Path"
        : @"INSERT INTO badges (Path, Collection, Badge) VALUES (@Path, @Collection, @Badge)";

        var command2 = new System.Data.SQLite.SQLiteCommand(sql, m_dbConnection);
        command2.Parameters.AddWithValue("Path", itemPath);
        command2.Parameters.AddWithValue("Collection", Path.GetFileName(Path.GetDirectoryName(badgePath)));
        command2.Parameters.AddWithValue("Badge", Path.GetFileName(badgePath));
        command2.ExecuteNonQuery();
        Reader.Close();
      }

      m_dbConnection.Close();
    }

    private void MenuItem_Click_2(object sender, RoutedEventArgs e) {
      this.SaveBadgeForItem(this._ShellListView.GetFirstSelectedItem().ParsingName, String.Empty, true);
      this.Badges = this.LoadBadgesData();
      this._ShellListView.BadgesData = this.Badges;
      this._ShellListView.RefreshItem(this._ShellListView.GetFirstSelectedItemIndex(), true);
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e) {
      this._ShellListView.CancelNavigation();
      this._ProgressTimer.Stop();
      this.bcbc.SetProgressValue(this.bcbc.ProgressMaximum, TimeSpan.FromMilliseconds(750));
      this.bcbc.ProgressMaximum = 100;
      this.btnCancelNavigation.Visibility = Visibility.Collapsed;
      this.btnGoNavigation.Visibility = Visibility.Visible;
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e) {
      this._ShellListView.RefreshContents();
      this.SetSortingAndGroupingButtons();
      this.SetupUIOnSelectOrNavigate();
    }

    void bcbc_OnEditModeToggle(object sender, Odyssey.Controls.EditModeToggleEventArgs e) {
      this._ShellListView.IsFocusAllowed = e.IsExit;
      if (!e.IsExit)
        this.bcbc.Focus();
    }

    private void bcbc_BreadcrumbItemDropDownOpened(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e) {
      this._ShellListView.IsFocusAllowed = false;
      this.bcbc.Focus();
    }

    private void bcbc_BreadcrumbItemDropDownClosed(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e) => this._ShellListView.IsFocusAllowed = true;

    private void btnOpenWith_DropDownOpened(object sender, EventArgs e) {
      var mnu = new ShellContextMenu(this._ShellListView, false);

      var controlPos = this.btnOpenWith.TransformToAncestor(Application.Current.MainWindow).Transform(new WIN.Point(0, 0));
      var tempPoint = this.PointToScreen(new WIN.Point(controlPos.X, controlPos.Y));
      mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)this.btnOpenWith.ActualHeight), 1);
      this.btnOpenWith.IsDropDownOpen = false;
    }

    private void btnSort_DropDownOpened(object sender, EventArgs e) {
      var button = sender as DropDownButton;
      foreach (MenuItem item in button.Items.OfType<MenuItem>()) {
        var column = item.Tag as Collumns;
        if (column == this._ShellListView.Collumns.FirstOrDefault(w => w.ID == this._ShellListView.LastSortedColumnId)) {
          item.IsChecked = true;
          break;
        }
      }
      button.Items.OfType<MenuItem>().Last().IsChecked = this._ShellListView.LastSortOrder == WIN.Forms.SortOrder.Descending;
      button.Items.OfType<MenuItem>().ToArray()[button.Items.OfType<MenuItem>().Count() - 2].IsChecked = this._ShellListView.LastSortOrder == WIN.Forms.SortOrder.Ascending;
    }

    private void btnGroup_DropDownOpened(object sender, EventArgs e) {
      var button = sender as DropDownButton;
      foreach (MenuItem item in button.Items.OfType<MenuItem>()) {
        var column = item.Tag as Collumns;
        if (column == this._ShellListView.LastGroupCollumn) {
          item.IsChecked = true;
          break;
        }
      }
      button.Items.OfType<MenuItem>().Last().IsChecked = this._ShellListView.LastGroupOrder == WIN.Forms.SortOrder.Descending;
      button.Items.OfType<MenuItem>().ToArray()[button.Items.OfType<MenuItem>().Count() - 2].IsChecked = this._ShellListView.LastGroupOrder == WIN.Forms.SortOrder.Ascending;
    }

    private void btnStartPowerShellClick(object sender, RoutedEventArgs e) {
      if (this.ctrlConsole.IsProcessRunning)
        this.ctrlConsole.StopProcess();
      //ctrlConsole.StartPowerShell();
    }

    private void chkTraditionalNameGrouping_CheckChanged(Object sender, RoutedEventArgs e) {
      BESettings.IsTraditionalNameGrouping = e.RoutedEvent.Name == "Checked";
    }

    private void btnResetFolderSettings_OnClick(object sender, RoutedEventArgs e) => this._ShellListView.ResetFolderSettings();

    #endregion

    private void BtnTheme_OnChecked(Object sender, RoutedEventArgs e) {
      this.ChangeRibbonTheme("Dark");
      this._ShellListView.ChangeTheme(ThemeColors.Dark);
      this.KeepBackstageOpen = true;
    }

    private void BtnTheme_OnUnchecked(Object sender, RoutedEventArgs e) {
      this.ChangeRibbonTheme("Light");
      this._ShellListView.ChangeTheme(ThemeColors.Light);
      this.KeepBackstageOpen = true;
    }

    private Double _ScrollOldValue = 0d;
    private Boolean _IsScrollingManually = false;
    private System.Windows.Forms.Timer _ScrollDeferer = new System.Windows.Forms.Timer();

    private void ScrollBar_OnScroll(Object sender, System.Windows.Controls.Primitives.ScrollEventArgs e) {
      //this.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, (Action) (() => { }));
      //this._IsScrollingManually = true;
      ////User32.LockWindowUpdate(this._ShellListView.LVHandle);
      //this._ShellListView.ScrollSyncEvent.Reset();

      //// this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() => {
      ////var currentValue = this.sbLVVertical.Value / (this._ShellListView.View == ShellViewStyle.Details ? 16 : 1);
      //var delta = this.sbLVVertical.Value - this._ScrollOldValue;
      //var ratio = (this.sbLVVertical.Maximum / this.sbLVVertical.ViewportSize);
      ////var newValue = (Int32) Math.Round(delta);
      ////if (this._ShellListView.View == ShellViewStyle.Details) {
      ////  User32.SendMessage(this._ShellListView.LVHandle, 0x1000 + 20, 0, delta < 0 ? -16 : 16);
      ////  this._ScrollOldValue = Math.Ceiling(e.NewValue);
      ////} else if (this._ShellListView.View != ShellViewStyle.Details) {
      //if (Math.Abs(delta) >= ratio) {
      //User32.SendMessage(this._ShellListView.LVHandle, 0x1000 + 20, 0, (Int32)Math.Ceiling(delta*(16)));
      ////this.sbLVVertical.Value = this._ScrollOldValue + Math.Round(delta);
      //this._ScrollOldValue = this.sbLVVertical.Value;

      //}
      //}

      //if (Math.Abs(delta) >= this.sbLVVertical.SmallChange) {
      //User32.LockWindowUpdate(this._ShellListView.LVHandle);

      //User32.LockWindowUpdate(IntPtr.Zero);

      //}

      //if (!this._ScrollDeferer.Enabled) {
      //this._ScrollDeferer.Stop();
      //this._ScrollDeferer.Start();
      //if (e.ScrollEventType == ScrollEventType.EndScroll) {
      //  //}
      //  //}
      //  this._ScrollDeferer.Stop();
      //  this._IsScrollingManually = false;
      //  this._ScrollOldValue = e.NewValue;
      //  User32.SendMessage(this._ShellListView.LVHandle, 0x1000 + 20, 0, (Int32) Math.Ceiling(delta) * (this._ShellListView.View == ShellViewStyle.Details ? 14 : 1));
      //  //}));

      //}



    }
  }
}
