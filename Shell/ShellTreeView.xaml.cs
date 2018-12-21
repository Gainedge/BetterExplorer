using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace BExplorer.Shell {
  /// <summary>
  /// Interaction logic for ShellTreeView.xaml
  /// </summary>
  public partial class ShellTreeView : UserControl {
    #region Public Members

    /// <summary>Do you want to show hidden items/nodes in the list</summary>
    public bool IsShowHiddenItems { get; set; }

    /// <summary>The <see cref="ShellView">List</see> that this is paired with</summary>
    public ShellView ShellListView {
      private get { return this._ShellListView; }

      set {
        this._ShellListView = value;
        //this._ShellListView.Navigated += this.ShellListView_Navigated;
      }
    }
    #endregion

    #region Event Handlers

    public event EventHandler<TreeNodeMouseClickEventArgs> NodeClick;

    public event EventHandler<NavigatedEventArgs> AfterSelect;

    public ObservableCollection<Object> Roots { get; set; }

    #endregion Event Handlers

    #region Private Members

    //private TreeViewBase ShellTreeView;
    private List<string> _PathsToBeAdd = new List<string>();
    private string _SearchingForFolders = "Searching for folders...";

    private TreeNode cuttedNode { get; set; }

    private ManualResetEvent _ResetEvent = new ManualResetEvent(true);
    private int folderImageListIndex;
    private ShellView _ShellListView;
    private List<IntPtr> UpdatedImages = new List<IntPtr>();
    private List<IntPtr> CheckedFroChilds = new List<IntPtr>();
    private SyncQueue<IntPtr> imagesQueue = new SyncQueue<IntPtr>(); // 7000
    private SyncQueue<IntPtr> childsQueue = new SyncQueue<IntPtr>(); // 7000
    private Thread imagesThread;
    private Thread childsThread;
    private bool isFromTreeview;
    private bool _IsNavigate;
    private string _EmptyItemString = "<!EMPTY!>";
    private bool _AcceptSelection = true;
    private ShellNotifications _NotificationNetWork = new ShellNotifications();
    private ShellNotifications _NotificationGlobal = new ShellNotifications();

    private System.Runtime.InteropServices.ComTypes.IDataObject _DataObject { get; set; }

    #endregion

    #region Private Methods

    private void InitRootItems() {
      this.imagesQueue.Clear();
      this.childsQueue.Clear();
      this.UpdatedImages.Clear();
      this.CheckedFroChilds.Clear();

      var favoritesItem = Utilities.WindowsVersion == WindowsVersions.Windows10
        ? FileSystemListItem.ToFileSystemItem(IntPtr.Zero, "shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}")
        : FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Links).Pidl);
      var favoritesRoot = new FilesystemTreeViewItem(this, favoritesItem);
      //favoritesRoot.ImageIndex = favoritesItem.GetSystemImageListIndex(favoritesItem.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      //favoritesRoot.SelectedImageIndex = favoritesRoot.ImageIndex;

      //if (favoritesItem.Count() > 0) {
      //  favoritesRoot.Items.Add(new TreeViewItem() { Header = this._EmptyItemString });
      //}

      var librariesItem = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Libraries).Pidl);
      var librariesRoot = new FilesystemTreeViewItem(this, librariesItem);
      //librariesRoot.Tag = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Libraries).Pidl);
      //librariesRoot.ImageIndex = librariesItem.GetSystemImageListIndex(librariesItem.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      //librariesRoot.SelectedImageIndex = librariesRoot.ImageIndex;
      //if (librariesItem.HasSubFolders) {
      //  librariesRoot.Items.Add(new TreeViewItem() { Header = this._EmptyItemString });
      //}

      var computerItem = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Computer).Pidl);
      var computerRoot = new FilesystemTreeViewItem(this, computerItem);
      //computerRoot.Header = computerItem.DisplayName;
      //computerRoot.Tag = computerItem;
      //computerRoot.ImageIndex = computerItem.GetSystemImageListIndex(computerItem.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      //computerRoot.SelectedImageIndex = computerRoot.ImageIndex;
      //if (computerItem.HasSubFolders) {
      //  computerRoot.Items.Add(new TreeViewItem() { Header = this._EmptyItemString });
      //}

      var networkItem = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Network).Pidl);
      var networkRoot = new FilesystemTreeViewItem(this, networkItem);
      //networkRoot.Header = networkItem.DisplayName;
      //networkRoot.Tag = networkItem;
      //networkRoot.ImageIndex = networkItem.GetSystemImageListIndex(networkItem.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      //networkRoot.SelectedImageIndex = networkRoot.ImageIndex;
      //networkRoot.Items.Add(new TreeViewItem() { Header = this._EmptyItemString });
      favoritesRoot.IsExpanded = true;
      librariesRoot.IsExpanded = true;
      computerRoot.IsExpanded = true;
      this.Roots.Add(favoritesRoot);
      this.Roots.Add(new TreeViewItem());
      this.Roots.Add(librariesRoot);
      this.Roots.Add(new TreeViewItem());
      this.Roots.Add(computerRoot);
      this.Roots.Add(new TreeViewItem());
      this.Roots.Add(networkRoot);




    }
    #endregion

    public ShellTreeView() {
      this.DataContext = this;
      ShellItem.IsCareForMessageHandle = false;
      this.Roots = new ObservableCollection<Object>();
      InitializeComponent();
      this.InitRootItems();
    }



    private void TvShellTreeViewInternal_OnExpanded(Object sender, RoutedEventArgs e) {
      //if (e. == TreeViewAction.Collapse) {
      //  this._AcceptSelection = false;
      //}
      var treeViewItem = e.OriginalSource as TreeViewItem;
      if (treeViewItem.DataContext is FilesystemTreeViewItem fstItem && fstItem.Items.Count == 1 && fstItem.Items.First() is TreeViewItem) {
        fstItem.Items.Clear();
        this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
          var sho = fstItem.FsItem;
          foreach (var item in sho.ParsingName != KnownFolders.Computer.ParsingName
            ? sho.GetContents(this.IsShowHiddenItems)
              .Where(w => (sho != null && !sho.IsFileSystem && System.IO.Path.GetExtension(sho?.ParsingName)?.ToLowerInvariant() != ".library-ms") ||
                          (w.IsFolder || w.IsLink || w.IsDrive)).OrderBy(o => o.DisplayName)
            : sho.GetContents(this.IsShowHiddenItems)
              .Where(w => (sho != null && !sho.IsFileSystem && System.IO.Path.GetExtension(sho?.ParsingName)?.ToLowerInvariant() != ".library-ms") || (w.IsFolder || w.IsLink || w.IsDrive))) {
            if (item != null && !item.IsFolder && !item.IsLink) {
              continue;
            }
            //System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate { }));
            if (item?.IsLink == true) {
              try {
                var shellLink = new ShellLink(item.ParsingName);
                var linkTarget = shellLink.TargetPIDL;
                var itemLinkReal = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, linkTarget);
                shellLink.Dispose();
                if (!itemLinkReal.IsFolder) {
                  continue;
                }
              } catch { }
            }
            fstItem.Items.Add(new FilesystemTreeViewItem(this, item));
          }
        }));
      }
      //return;
      //var treeViewItem = e.Source as FilesystemTreeViewItem;
      //  this._ResetEvent.Set();
      //  if (treeViewItem.Items.Count > 0 && (treeViewItem.Items.OfType<TreeViewItem>().First().Header.ToString() == this._EmptyItemString || treeViewItem.Items.OfType<TreeViewItem>().First().Header.ToString() == this._SearchingForFolders)) {
      //    treeViewItem.Items.Clear();
      //    this.imagesQueue.Clear();
      //    this.childsQueue.Clear();
      //    var sho = treeViewItem.Tag as IListItemEx;
      //    if (sho == null) {
      //      return;
      //    }
      //    //if (sho != null) {
      //    //  sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, sho.PIDL);
      //    //}
      //    // var lvSho = this.ShellListView != null && this.ShellListView.CurrentFolder != null ? this.ShellListView.CurrentFolder.Clone() : null;
      //    var lvSho = this.ShellListView?.CurrentFolder?.Clone();

      //    //var thread = new Thread(() => {
      //    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) (() => {
      //      var node = treeViewItem;
      //      node.Items.Add(new TreeViewItem() {Header = this._SearchingForFolders});
      //      var nodesTemp = new List<TreeViewItem>();
      //      if (sho?.IsLink == true) {
      //        try {
      //          var shellLink = new ShellLink(sho.ParsingName);
      //          var linkTarget = shellLink.TargetPIDL;
      //          sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, linkTarget);
      //          shellLink.Dispose();
      //        } catch { }
      //      }

      //      foreach (var item in sho.ParsingName != KnownFolders.Computer.ParsingName
      //        ? sho.GetContents(this.IsShowHiddenItems)
      //          .Where(w => (sho != null && !sho.IsFileSystem && System.IO.Path.GetExtension(sho?.ParsingName)?.ToLowerInvariant() != ".library-ms") ||
      //                      (w.IsFolder || w.IsLink || w.IsDrive)).OrderBy(o => o.DisplayName)
      //        : sho.GetContents(this.IsShowHiddenItems)
      //          .Where(w => (sho != null && !sho.IsFileSystem && System.IO.Path.GetExtension(sho?.ParsingName)?.ToLowerInvariant() != ".library-ms") || (w.IsFolder || w.IsLink || w.IsDrive))) {
      //        if (item != null && !item.IsFolder && !item.IsLink) {
      //          continue;
      //        }

      //        if (item?.IsLink == true) {
      //          try {
      //            var shellLink = new ShellLink(item.ParsingName);
      //            var linkTarget = shellLink.TargetPIDL;
      //            var itemLinkReal = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, linkTarget);
      //            shellLink.Dispose();
      //            if (!itemLinkReal.IsFolder) {
      //              continue;
      //            }
      //          } catch { }
      //        }

      //        var itemNode = new TreeViewItem();
      //        itemNode.Header = item.DisplayName;

      //        // IListItemEx itemReal = null;
      //        // if (item.Parent?.Parent != null && item.Parent.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
      //        var itemReal = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, item.PIDL);

      //        itemNode.Tag = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, item.PIDL);

      //        if (sho.IsNetworkPath && sho.ParsingName != KnownFolders.Network.ParsingName) {
      //          //itemNode.ImageIndex = this.folderImageListIndex;
      //        } else if (itemReal.IconType == IExtractIconPWFlags.GIL_PERCLASS || sho.ParsingName == KnownFolders.Network.ParsingName) {
      //          //itemNode.ImageIndex = itemReal.GetSystemImageListIndex(itemReal.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      //          //itemNode.SelectedImageIndex = itemNode.ImageIndex;
      //        } else {
      //          //itemNode.ImageIndex = this.folderImageListIndex;
      //        }

      //        if (item.HasSubFolders) {
      //          itemNode.Items.Add(new TreeViewItem() {Header = this._EmptyItemString});
      //        }

      //        if (item.ParsingName.EndsWith(".library-ms")) {
      //          var library = ShellLibrary.Load(Path.GetFileNameWithoutExtension(item.ParsingName), false);
      //          if (library.IsPinnedToNavigationPane) {
      //            nodesTemp.Add(itemNode);
      //          }

      //          library.Close();
      //        } else {
      //          nodesTemp.Add(itemNode);
      //        }

      //        // Application.DoEvents();
      //      }


      //      if (node.Items.Count == 1 && node.Items.OfType<TreeViewItem>().First().Header.ToString() == this._SearchingForFolders) {
      //        node.Items.RemoveAt(0);
      //      }

      //      //foreach (var element in nodesTemp) {
      //        node.ItemsSource = nodesTemp;
      //      //}


      //      if (lvSho != null) {
      //        //this.SelItem(lvSho);
      //      }
      //    }));
      //}));
      //});
      //thread.SetApartmentState(ApartmentState.STA);
      //thread.Start();
      //}

    }
    private ScrollViewer _ScrollViewer { get; set; }
    public ScrollViewer ScrollViewer {
      get {
        if (this._ScrollViewer == null) {
          DependencyObject border = VisualTreeHelper.GetChild(this.tvShellTreeViewInternal, 0);
          if (border != null) {
            this._ScrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
          }
        }

        return this._ScrollViewer;
      }
    }

    private void OnTreeViewItem_Hover(Object sender, MouseEventArgs e) {
      var item = sender as TreeViewItem;
      var filesystemTreeViewItem = item.DataContext as FilesystemTreeViewItem;
      var directlyOver = Mouse.DirectlyOver as FrameworkElement;
      if (directlyOver != null && filesystemTreeViewItem.FsItem.ParsingName == (directlyOver.DataContext as FilesystemTreeViewItem).FsItem.ParsingName) {

        var ch = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(item,0),0),0) as Visual;
        var margin = (Thickness) ch.GetValue(MarginProperty);
        GeneralTransform childTransform = ch.TransformToAncestor(this.ScrollViewer);
        Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), item.RenderSize));
        //rectangle.X = rectangle.Left + margin.Left;
        var svRect = new Rect(new Point(0, 0), this.ScrollViewer.RenderSize);
        if (rectangle.Left < 0) {
          this.ScrollViewer.ScrollToHorizontalOffset(margin.Left);
        }
      }

      //this.ScrollViewer.ScrollToLeftEnd();
    }

    private void TvShellTreeViewInternal_OnRequestBringIntoView(Object sender, RequestBringIntoViewEventArgs e) {
      //e.Handled = true;
    }

    public void NavigateListView(IListItemEx sho) {
      if (sho != null) {
        IListItemEx linkSho = null;
        if (sho.IsLink) {
          try {
            var shellLink = new ShellLink(sho.ParsingName);
            var linkTarget = shellLink.TargetPIDL;
            linkSho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, linkTarget);
            shellLink.Dispose();
          } catch {
          }
        }

        this.isFromTreeview = true;
        if (true) {
          this.ShellListView.Navigate_Full(linkSho ?? sho, true, true);
        }

        this._IsNavigate = false;
        this.isFromTreeview = false;
      }

    }

    private void TvShellTreeViewInternal_OnSelectedItemChanged(Object sender, RoutedPropertyChangedEventArgs<Object> e) {
      e.Handled = true;
    }

    private void TvShellTreeViewInternal_OnPreviewMouseUp(Object sender, MouseButtonEventArgs e) {
      //e.Handled = true;
      if (e.OriginalSource is ToggleButton) {
        return;
      }
      var shItem = ((e.OriginalSource as FrameworkElement)?.DataContext as FilesystemTreeViewItem)?.FsItem;
      this.NavigateListView(shItem);
    }
  }
}
