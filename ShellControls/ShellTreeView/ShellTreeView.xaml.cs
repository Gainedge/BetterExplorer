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
using System.Windows.Threading;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace ShellControls.ShellTreeView {
  /// <summary>
  /// Interaction logic for ShellTreeView.xaml
  /// </summary>
  public partial class ShellTreeView : UserControl {
    #region Public Members

    /// <summary>Do you want to show hidden items/nodes in the list</summary>
    public bool IsShowHiddenItems { get; set; }

    /// <summary>The <see cref="ShellView">List</see> that this is paired with</summary>
    public ShellView ShellListView {
      get => this._ShellListView;

      set {
        this._ShellListView = value;
        this._ShellListView.Navigated += this.ShellListView_Navigated;
        this._ShellListView.NewItemAvailable += ShellListViewOnNewItemAvailable;
      }
    }

    public ImageListEx SmallImageList;

    private void ShellListViewOnNewItemAvailable(Object sender, ItemUpdatedEventArgs e) {
      if (e.NewItem.Parent.ParsingName == KnownFolders.Network.ParsingName && e.NewItem.IsFolder) {
        var newTreeItem = new FilesystemTreeViewItem(this, e.NewItem);
        var networkRoot = this.Roots.OfType<FilesystemTreeViewItem>().Last();
        if (networkRoot.Items.OfType<FilesystemTreeViewItem>().Count(c => c.FsItem.ParsingName.Equals(e.NewItem.ParsingName, StringComparison.InvariantCultureIgnoreCase)) == 0) {
          networkRoot.Items.Add(newTreeItem);
        }
      }
    }

    #endregion

    #region Event Handlers

    public event EventHandler<TreeViewMiddleClickEventArgs> NodeClick;

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
      this.SmallImageList = new ImageListEx(16);
      this.Roots = new ObservableCollection<Object>();
      InitializeComponent();
      this.InitRootItems();
    }

    /// <summary>Refreshes/rebuilds all nods (clears nodes => initializes root items => selects current folder from <see cref="ShellListView"/>)</summary>
    public void RefreshContents() {
      this.Roots.Clear();
      this.InitRootItems();

      if (this.ShellListView?.CurrentFolder != null) {
        this.SelItem(this.ShellListView.CurrentFolder);
      }
    }

    private FilesystemTreeViewItem FromItem(IListItemEx item, FilesystemTreeViewItem rootNode) {
      if (rootNode == null) {
        return null;
      }

      foreach (var node in rootNode.Items.OfType<FilesystemTreeViewItem>()) {
        if (node.FsItem?.Equals(item) == true) {
          if (!rootNode.IsExpanded) {
            rootNode.IsExpanded = true;
          }
          return node;
        }

        var next = this.FromItem(item, node);
        if (next != null) {
          return next;
        }
      }

      return null;
    }

    private FilesystemTreeViewItem FromItem(IListItemEx item) {
      foreach (var node in this.tvShellTreeViewInternal.Items.OfType<FilesystemTreeViewItem>().Where(w => {
        var nodeItem = w.FsItem;
        //if (nodeItem != null) {
        //  nodeItem = FileSystemListItem.ToFileSystemItem(item.ParentHandle, nodeItem.PIDL);
        //}
        return nodeItem != null && (!nodeItem.ParsingName.Equals(KnownFolders.Links.ParsingName) && !nodeItem.ParsingName.Equals("::{679f85cb-0220-4080-b29b-5540cc05aab6}"));
      })) {
        if ((node.FsItem)?.Equals(item) == true) {
          return node;
        }

        var next = this.FromItem(item, node);
        if (next != null) {
          return next;
        }
      }

      return null;
    }

    Stack<IListItemEx> parents = new Stack<IListItemEx>();

    private void FindItem(IListItemEx item) {
      var nodeNext = this.tvShellTreeViewInternal.Items.OfType<FilesystemTreeViewItem>().FirstOrDefault(s => s.FsItem != null && s.FsItem.Equals(item));
      if (nodeNext == null) {
        this.parents.Push(item);
        if (item.Parent != null) {
          this.FindItem(item.Parent.Clone());
        }
      } else {
        while (this.parents.Count > 0) {
          var obj = this.parents.Pop();
          var newNode = this.FromItem(obj);
          if (newNode != null && !newNode.IsExpanded) {
            newNode.IsExpanded = true;
          }
        }
      }
    }

    private void SelItem(IListItemEx item) {
      // item = FileSystemListItem.ToFileSystemItem(item.ParentHandle, item.PIDL);
      var node = this.FromItem(item);
      if (node == null) {
        this.FindItem(item.Clone());
      } else {
        node.IsSelected = true;
      }
    }

    private void ShellListView_Navigated(Object sender, NavigatedEventArgs e) {
      if (this.isFromTreeview) {
        return;
      }
      //this.SelItem(e.Folder);
      // TODO: Try to reenable this since sometimes it causes an exceptions
      var thread = new Thread(() => { this.SelItem(e.Folder); });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }


    private void TvShellTreeViewInternal_OnExpanded(Object sender, RoutedEventArgs e) {
      //if (e. == TreeViewAction.Collapse) {
      //  this._AcceptSelection = false;
      //}
      var treeViewItem = e.OriginalSource as FrameworkElement;
      if (treeViewItem.DataContext is FilesystemTreeViewItem fstItem && !fstItem.IsLoaded) {
        //fstItem.Items.Clear();
        //var navThread = new Thread(() => {

        var items = new List<Object>();
        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)(() => {
          var sho = fstItem.FsItem;
          //fstItem.Items.Clear();
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
            items.Add(new FilesystemTreeViewItem(this, item));
          }
          fstItem.Items.AddRange(items);
          fstItem.IsLoaded = true;
          var lvSho = this.ShellListView?.CurrentFolder?.Clone();
          if (lvSho != null) {
            //this.SelItem(lvSho);
            //var item = this.tvShellTreeViewInternal.ItemContainerGenerator.ContainerFromItemRecursive(this.tvShellTreeViewInternal.SelectedItem);
            //item?.BringIntoView();
          }
        }));

        //});
        //navThread.SetApartmentState(ApartmentState.STA);
        //navThread.Start();
      }
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
      //var item = sender as TreeViewItem;
      //var filesystemTreeViewItem = item.DataContext as FilesystemTreeViewItem;
      //var directlyOver = Mouse.DirectlyOver as FrameworkElement;
      //if (directlyOver != null && filesystemTreeViewItem.FsItem.ParsingName == (directlyOver.DataContext as FilesystemTreeViewItem).FsItem.ParsingName) {

      //  var ch = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(item,0),0),0) as Visual;
      //  var margin = (Thickness) ch.GetValue(MarginProperty);
      //  GeneralTransform childTransform = ch.TransformToAncestor(this.ScrollViewer);
      //  Rect rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), item.RenderSize));
      //  //rectangle.X = rectangle.Left + margin.Left;
      //  var svRect = new Rect(new Point(0, 0), this.ScrollViewer.RenderSize);
      //  if (rectangle.Left < 0) {
      //    this.ScrollViewer.ScrollToHorizontalOffset(margin.Left);
      //  }
      //}

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

    private void TvShellTreeViewInternal_OnPreviewMouseUp(Object sender, MouseButtonEventArgs e) {
      //e.Handled = true;
      if (e.OriginalSource is ToggleButton) {
        return;
      }
      var shItem = ((e.OriginalSource as FrameworkElement)?.DataContext as FilesystemTreeViewItem)?.FsItem;
      switch (e.ChangedButton) {
        case MouseButton.Left:
          this.NavigateListView(shItem);
          break;
        case MouseButton.Middle:
          this.NodeClick?.Invoke(this, new TreeViewMiddleClickEventArgs() { Item = shItem });
          break;
        case MouseButton.Right:
          var pt = this.PointToScreen(e.GetPosition(this));
          new ShellContextMenu.ShellContextMenu(this.ShellListView, shItem).ShowContextMenu(new System.Drawing.Point((Int32)pt.X, (Int32)pt.Y), CMF.CANRENAME);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void EventSetter_OnHandler(Object sender, RoutedEventArgs e) {
      e.Handled = true;
      var item = this.tvShellTreeViewInternal.ItemContainerGenerator.ContainerFromItemRecursive(e.OriginalSource);
      item?.BringIntoView();
    }

    private void ShellTreeView_OnUnloaded(Object sender, RoutedEventArgs e) {
      this.SmallImageList.Dispose();
    }
  }
}
