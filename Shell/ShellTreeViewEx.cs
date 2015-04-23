using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using F = System.Windows.Forms;
using BExplorer.Shell._Plugin_Interfaces;

namespace BExplorer.Shell {

  public class NodeSorter : IComparer {
    public int Compare(object x, object y) {
      TreeNode tx = (TreeNode)x;
      TreeNode ty = (TreeNode)y;
      if (tx.Tag == null || ty.Tag == null)
        return 0;

      return (tx.Tag as IListItemEx).DisplayName.CompareTo((ty.Tag as IListItemEx).DisplayName);
    }
  }

  public partial class ShellTreeViewEx : UserControl {

    /*
     * Should we use HashSets not Lists?
     * http://stackoverflow.com/questions/150750/hashset-vs-list-performance
     * http://blog.goyello.com/2013/01/30/6-more-things-c-developers-should-not-do/
     * http://www.c-sharpcorner.com/UploadFile/0f68f2/comparative-analysis-of-list-hashset-and-sortedset/
     * 
     * 
     * Performance Test Code!
     * http://stackoverflow.com/questions/25615764/list-add-vs-hashset-add-for-small-collections-in-c-sharp
     * 
     * I think HashSets are actually worse as the lists are to small
     */

    #region Event Handlers

    public event EventHandler<TreeNodeMouseClickEventArgs> NodeClick;
    public event EventHandler<NavigatedEventArgs> AfterSelect;
    #endregion Event Handlers

    #region Public Members

    public TreeViewBase ShellTreeView;
    public Boolean IsShowHiddenItems { get; set; }

    public ShellView ShellListView {
      private get {
        return _ShellListView;
      }
      set {
        _ShellListView = value;
        _ShellListView.Navigated += ShellListView_Navigated;
      }
    }

    #endregion Public Members

    #region Private Members
    private string _SearchingForFolders = "Searching for folders...";
    private TreeNode cuttedNode { get; set; }
    private ManualResetEvent _ResetEvent = new ManualResetEvent(true);
    private Int32 folderImageListIndex;
    private ShellView _ShellListView;
    private List<IntPtr> UpdatedImages = new List<IntPtr>();
    private List<IntPtr> CheckedFroChilds = new List<IntPtr>();
    private ConQueue<IntPtr> imagesQueue = new ConQueue<IntPtr>(); //7000
    private ConQueue<IntPtr> childsQueue = new ConQueue<IntPtr>(); //7000
    private Thread imagesThread;
    private Thread childsThread;
    private Boolean isFromTreeview;
    private Boolean _IsNavigate;
    private String _EmptyItemString = "<!EMPTY!>";
    private Boolean _AcceptSelection = true;
    private ShellNotifications _NotificationNetWork = new ShellNotifications();
    private ShellNotifications _NotificationGlobal = new ShellNotifications();
    private System.Runtime.InteropServices.ComTypes.IDataObject _DataObject { get; set; }

    #endregion Private Members

    #region Private Methods

    private void InitRootItems() {
      this.imagesQueue.Clear();
      this.childsQueue.Clear();
      this.UpdatedImages.Clear();
      this.CheckedFroChilds.Clear();
      var favoritesItem = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Links).ParsingName.ToShellParsingName());
      var favoritesRoot = new TreeNode((favoritesItem).DisplayName);
      favoritesRoot.Tag = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Links).ParsingName.ToShellParsingName());
      favoritesRoot.ImageIndex = ((ShellItem)KnownFolders.Favorites).GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      favoritesRoot.SelectedImageIndex = favoritesRoot.ImageIndex;

      if (favoritesItem.Count() > 0)
        favoritesRoot.Nodes.Add(_EmptyItemString);

      var librariesItem = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Libraries).ParsingName.ToShellParsingName());
      var librariesRoot = new TreeNode(librariesItem.DisplayName);
      librariesRoot.Tag = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Libraries).ParsingName.ToShellParsingName()); ;
      librariesRoot.ImageIndex = librariesItem.GetSystemImageListIndex(librariesItem.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      librariesRoot.SelectedImageIndex = librariesRoot.ImageIndex;
      if (librariesItem.HasSubFolders)
        librariesRoot.Nodes.Add(_EmptyItemString);

      var computerItem = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Computer).ParsingName.ToShellParsingName());
      var computerRoot = new TreeNode(computerItem.DisplayName);
      computerRoot.Tag = computerItem;
      computerRoot.ImageIndex = computerItem.GetSystemImageListIndex(computerItem.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      computerRoot.SelectedImageIndex = computerRoot.ImageIndex;
      if (computerItem.HasSubFolders)
        computerRoot.Nodes.Add(_EmptyItemString);

      var networkItem = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.Network).ParsingName.ToShellParsingName());
      var networkRoot = new TreeNode(networkItem.DisplayName);
      networkRoot.Tag = networkItem;
      networkRoot.ImageIndex = networkItem.GetSystemImageListIndex(networkItem.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
      networkRoot.SelectedImageIndex = networkRoot.ImageIndex;
      networkRoot.Nodes.Add(_EmptyItemString);

      ShellTreeView.Nodes.Add(favoritesRoot);
      favoritesRoot.Expand();

      ShellTreeView.Nodes.AddRange(new[] {
				new TreeNode(), librariesRoot, new TreeNode(), computerRoot, new TreeNode(), networkRoot
			});

      librariesRoot.Expand();
      computerRoot.Expand();
    }

    private void SetNodeImage(IntPtr node, IntPtr pidl, IntPtr m_TreeViewHandle, Boolean isOverlayed) {
      try {
        var itemInfo = new TVITEMW();

        // We need to set the images for the item by sending a
        // TVM_SETITEMW message, as we need to set the overlay images,
        // and the .Net TreeView API does not support overlays.
        itemInfo.mask = TVIF.TVIF_IMAGE | TVIF.TVIF_SELECTEDIMAGE | TVIF.TVIF_STATE;
        itemInfo.hItem = node;
        itemInfo.iImage = ShellItem.GetSystemImageListIndex(pidl, ShellIconType.SmallIcon, ShellIconFlags.OverlayIndex);
        if (isOverlayed) {
          itemInfo.state = (TVIS)(itemInfo.iImage >> 16);
          itemInfo.stateMask = TVIS.TVIS_OVERLAYMASK;
        }
        itemInfo.iSelectedImage = ShellItem.GetSystemImageListIndex(pidl, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
        this.UpdatedImages.Add(node);
        User32.SendMessage(m_TreeViewHandle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW, 0, ref itemInfo);
      } catch (Exception) {
      }
    }

    public TreeNode FromItem(IListItemEx item, TreeNode rootNode) {
      foreach (TreeNode node in rootNode.Nodes) {
        if ((node.Tag as IListItemEx) != null && (node.Tag as IListItemEx).Equals(item)) return node;
        TreeNode next = FromItem(item, node);
        if (next != null) return next;
      }
      return null;
    }

    public TreeNode FromItem(IListItemEx item) {
      foreach (TreeNode node in this.ShellTreeView.Nodes) {
        if ((node.Tag as IListItemEx) != null && (node.Tag as IListItemEx).Equals(item)) return node;
        TreeNode next = FromItem(item, node);
        if (next != null) return next;
      }
      return null;
    }

    Stack<IListItemEx> parents = new Stack<IListItemEx>();
    public void FindItem(IListItemEx item) {
      var nodeNext = this.ShellTreeView.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as IListItemEx).Equals(item));
      if (nodeNext == null) {
        parents.Push(item);
        if (item.Parent != null) {
          this.FindItem(item.Parent);
        }
      } else {
        while (parents.Count > 0) {
          var obj = parents.Pop();
          this.Invoke((Action)(() => {
            var newNode = this.FromItem(obj);
            if (newNode != null && !newNode.IsExpanded) {
              newNode.Expand();
            }
          }));
        }
      }
    }
    public void SelItem(IListItemEx item) {
      var node = this.FromItem(item);
      if (node != null) {
        this.Invoke((Action)(() => {
          this.ShellTreeView.SelectedNode = node;
        }));
        return;
      }

      this.FindItem(item);
    }

    public void DeleteItem(IListItemEx item) {
      TreeNode itemNode = null;
      foreach (TreeNode node in this.ShellTreeView.Nodes) {
        itemNode = this.FromItem(item, node);
        if (itemNode != null) break;
      }

      if (itemNode != null) {
        itemNode.Remove();
      }
    }

    public void AddItem(IListItemEx item) {
      TreeNode itemNode = null;
      foreach (TreeNode node in this.ShellTreeView.Nodes) {
        try {
          itemNode = this.FromItem(item.Parent, node);
        } catch (Exception) {
          continue;
        }
        if (itemNode != null) break;
      }

      if (itemNode != null) {
        var node = new TreeNode(item.DisplayName);
        IListItemEx itemReal = null;
        //if (item.Parent != null && item.Parent.Parent != null && item.Parent.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
        //  itemReal = ShellItem.ToShellParsingName(item.ParsingName);
        //} else {
        //  itemReal = item;
        //}
        itemReal = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, item.ParsingName.ToShellParsingName());
        node.Tag = itemReal;
        var oldnodearray = itemNode.Nodes.OfType<TreeNode>().ToList();
        if (oldnodearray.SingleOrDefault(s => s.Tag != null && (s.Tag as IListItemEx).Equals(itemReal)) == null)
          oldnodearray.Add(node);
        var newArray = oldnodearray.OrderBy(o => o.Text).ToArray();
        this.ShellTreeView.BeginUpdate();
        itemNode.Nodes.Clear();
        itemNode.Nodes.AddRange(newArray);
        this.ShellTreeView.EndUpdate();
        if (itemNode != null && itemNode.Nodes.Count > 0) {
          var newNode = itemNode.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as IListItemEx).Equals(itemReal));
          if (newNode != null) {
            SetNodeImage(newNode.Handle, itemReal.PIDL, this.ShellTreeView.Handle, !(newNode.Parent != null && (newNode.Parent.Tag as IListItemEx).ParsingName == KnownFolders.Links.ParsingName));
          }
        }
      }
    }

    public void RenameItem(IListItemEx prevItem, IListItemEx newItem) {
      this.DeleteItem(prevItem);
      this.AddItem(newItem);
    }

    private void InitTreeView() {
      this.AllowDrop = true;
      this.ShellTreeView = new TreeViewBase();
      this.ShellTreeView.Dock = DockStyle.Fill;
      this.ShellTreeView.BackColor = Color.White;
      this.ShellTreeView.BorderStyle = F.BorderStyle.None;
      this.ShellTreeView.AllowDrop = true;
      this.ShellTreeView.HideSelection = false;
      this.ShellTreeView.ShowLines = false;
      this.ShellTreeView.HotTracking = true;
      this.ShellTreeView.LabelEdit = true;
      this.ShellTreeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
      this.ShellTreeView.DrawNode += ShellTreeView_DrawNode;
      this.ShellTreeView.BeforeExpand += ShellTreeView_BeforeExpand;
      this.ShellTreeView.AfterExpand += ShellTreeView_AfterExpand;
      this.ShellTreeView.MouseDown += ShellTreeView_MouseDown;
      this.ShellTreeView.HandleDestroyed += ShellTreeView_HandleDestroyed;
      this.ShellTreeView.ItemDrag += ShellTreeView_ItemDrag;
      this.ShellTreeView.AfterSelect += ShellTreeView_AfterSelect;
      this.ShellTreeView.NodeMouseClick += ShellTreeView_NodeMouseClick;
      this.ShellTreeView.AfterLabelEdit += ShellTreeView_AfterLabelEdit;
      this.ShellTreeView.KeyDown += ShellTreeView_KeyDown;
      this.ShellTreeView.DragEnter += ShellTreeView_DragEnter;
      this.ShellTreeView.DragOver += ShellTreeView_DragOver;
      this.ShellTreeView.DragLeave += ShellTreeView_DragLeave;
      this.ShellTreeView.DragDrop += ShellTreeView_DragDrop;
      this.ShellTreeView.GiveFeedback += ShellTreeView_GiveFeedback;
      this.ShellTreeView.MouseMove += ShellListView_MouseMove;
      this.ShellTreeView.MouseEnter += ShellTreeView_MouseEnter;
      this.ShellTreeView.MouseLeave += ShellTreeView_MouseLeave;
      this.ShellTreeView.MouseWheel += ShellTreeView_MouseWheel;
      this.ShellTreeView.VerticalScroll += ShellTreeView_VerticalScroll;
      this.ShellTreeView.BeforeSelect += ShellTreeView_BeforeSelect;
      if (this.ShellListView != null) {
        this.ShellListView.Navigated += ShellListView_Navigated;
      }
      SystemImageList.UseSystemImageList(ShellTreeView);
      ShellTreeView.FullRowSelect = true;
      var defIconInfo = new Shell32.SHSTOCKICONINFO() { cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO)) };
      Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_FOLDER, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
      this.folderImageListIndex = defIconInfo.iSysIconIndex;
      imagesThread = new Thread(new ThreadStart(LoadTreeImages)) { IsBackground = false };
      imagesThread.SetApartmentState(ApartmentState.STA);
      imagesThread.Start();
      childsThread = new Thread(new ThreadStart(LoadChilds)) { IsBackground = true };
      childsThread.Start();
    }

    void ShellTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e) {

    }

    void ShellTreeView_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
      e.UseDefaultCursors = true;
      var doo = new System.Windows.Forms.DataObject(this._DataObject);
      if (doo.GetDataPresent("DragWindow")) {
        IntPtr hwnd = ShellView.GetIntPtrFromData(doo.GetData("DragWindow"));
        User32.PostMessage(hwnd, 0x403, IntPtr.Zero, IntPtr.Zero);
      } else {
        e.UseDefaultCursors = true;
      }

      if (ShellView.IsDropDescriptionValid(this._DataObject)) {
        e.UseDefaultCursors = false;
        Cursor.Current = Cursors.Arrow;
      } else {
        e.UseDefaultCursors = true;
      }

      if (ShellView.IsShowingLayered(doo)) {
        e.UseDefaultCursors = false;
        Cursor.Current = Cursors.Arrow;
      } else {
        e.UseDefaultCursors = true;
      }

      base.OnGiveFeedback(e);
    }

    void ShellTreeView_VerticalScroll(object sender, EventArgs e) {
      //childsQueue.Clear();
      //imagesQueue.Clear();
    }

    void ShellTreeView_MouseWheel(object sender, MouseEventArgs e) {
      //childsQueue.Clear();
      //imagesQueue.Clear();
    }

    private void ShellTreeView_MouseLeave(object sender, EventArgs e) {
      if (this.ShellListView != null) {
        this.ShellListView.IsFocusAllowed = true;
      }
    }

    private void ShellTreeView_MouseEnter(object sender, EventArgs e) {
      if (this.ShellListView != null)
        this.ShellListView.IsFocusAllowed = false;

      this.ShellTreeView.Focus();
    }

    private void ShellListView_MouseMove(object sender, MouseEventArgs e) {
      if (!this.ShellTreeView.Focused)
        this.ShellTreeView.Focus();
    }

    private void ShellTreeView_MouseDown(object sender, MouseEventArgs e) {
      if (NodeClick != null) {
        var treeNode = this.ShellTreeView.GetNodeAt(e.X, e.Y);
        NodeClick.Invoke(this, new TreeNodeMouseClickEventArgs(treeNode, e.Button, e.Clicks, e.X, e.Y));
      }
    }

    #endregion Private Methods

    #region Public Methods

    public void RefreshContents() {
      this.ShellTreeView.Nodes.Clear();
      InitRootItems();

      if (this.ShellListView != null && this.ShellListView.CurrentFolder != null)
        this.SelItem(this.ShellListView.CurrentFolder);
    }

    public void RequestTreeImage(IntPtr handle) {
      //return;
      //this._ResetEvent.WaitOne();
      var t = new Thread(() => {
        Application.DoEvents();
        Thread.Sleep(1);
        TreeNode node = null;
        IntPtr treeHandle = IntPtr.Zero;
        var hash = -1;
        var pidl = IntPtr.Zero;
        var visible = false;
        if (this.ShellTreeView != null) {
          this.ShellTreeView.Invoke((Action)(() => {

            try {
              node = TreeNode.FromHandle(ShellTreeView, handle);
              treeHandle = ShellTreeView.Handle;
              if (node != null) {
                visible = node.IsVisible;
                var item = node.Tag as IListItemEx;
                pidl = item.AbsolutePidl;
              }
            } catch { }

          }));

          if (visible) {
            var nodeHandle = handle;
            Thread.Sleep(1);
            Application.DoEvents();

            SetNodeImage(nodeHandle, pidl, treeHandle, !(node.Parent != null && (node.Parent.Tag as IListItemEx).ParsingName == KnownFolders.Links.ParsingName));
          }
        }
      });
      t.Start();
    }

    public void LoadTreeImages() {
      //return;
      while (true) {
        //this._ResetEvent.WaitOne();
        Application.DoEvents();
        Thread.Sleep(1);
        var handle = imagesQueue.Dequeue();
        TreeNode node = null;
        IntPtr treeHandle = IntPtr.Zero;
        var hash = -1;
        var pidl = IntPtr.Zero;
        var visible = false;
        if (this.ShellTreeView != null) {
          this.ShellTreeView.Invoke((Action)(() => {

            node = TreeNode.FromHandle(ShellTreeView, handle);
            treeHandle = ShellTreeView.Handle;
            if (node != null) {
              visible = node.IsVisible;

              //if (node != null) {
              //	var item = node.Tag as ShellItem;
              //	if (item != null) {
              //		ShellItem newItem = null;
              //		try {
              //			newItem = ShellItem.ToShellParsingName(item.ParsingName);
              //		} catch (Exception) {
              //			newItem = item;
              //		}
              //		if (node != null && newItem != null && this.ShellTreeView != null) {
              //			treeHandle = this.ShellTreeView.Handle;
              //			hash = newItem.GetHashCode();
              //			pidl = newItem.AbsolutePidl;
              //			visible = node.IsVisible;
              //			newItem.Dispose();
              //		}
              //	}
              //}
              var item = node.Tag as IListItemEx;
              pidl = item.AbsolutePidl;
            }

          }));
        }
        //if (node != null) {
        //      var item = node.Tag as IListItemEx;
        //	if (item != null) {
        //        //IListItemEx newItem = null;
        //		//try {
        //		//	newItem = FileSystemListItem.ToFileSystemItem (IntPtr.Zero, item.ParsingName.ToShellParsingName());
        //		//}
        //		//catch (Exception) {
        //			newItem = item;
        //		//}
        //		if (node != null && newItem != null) {
        //			try {
        //				pidl = newItem.AbsolutePidl;
        //				//newItem.Dispose();
        //			}
        //			catch (Exception) {

        //			}
        //		}
        //	}
        //}
        if (visible) {
          var nodeHandle = handle;
          Thread.Sleep(1);
          Application.DoEvents();

          SetNodeImage(nodeHandle, pidl, treeHandle, !(node.Parent != null && (node.Parent.Tag as IListItemEx).ParsingName == KnownFolders.Links.ParsingName));
        }
      }
    }

    public void RequestLoadChilds(IntPtr handle) {
      var t = new Thread(() => {
        Application.DoEvents();
        Thread.Sleep(1);
        this._ResetEvent.WaitOne();
        TreeNode node = null;
        IntPtr treeHandle = IntPtr.Zero;
        var visible = true;
        //var pidl = IntPtr.Zero;
        if (ShellTreeView != null) {
          this.ShellTreeView.Invoke((Action)(() => {

            node = TreeNode.FromHandle(ShellTreeView, handle);
            treeHandle = this.ShellTreeView.Handle;
            if (node != null) {
              visible = node.IsVisible;
              //if (node.Tag != null)
              //  pidl = ((IListItemEx)node.Tag).PIDL;
            }

          }));
        }

        if (!visible)
          return;

        if (node != null && node.Nodes.Count > 0) {
          var childItem = node.Nodes[0];
          if (childItem != null) {
            var nodeHandle = childItem.Handle;

            //TODO: Try to remove this Try Catch! It's slowing this down!!
            //TODO: Have the try catch only around the error causing code
            try {
              var sho = (node.Tag as IListItemEx);// FileSystemListItem.ToFileSystemItem(IntPtr.Zero, pidl);
              if (!sho.HasSubFolders) {
                User32.SendMessage(treeHandle, BExplorer.Shell.Interop.MSG.TVM_DELETEITEM, 0, nodeHandle);
              }
              this.CheckedFroChilds.Add(handle);
              //sho.Dispose();
            } catch (Exception) {
            }
          }
        }
      });
      t.Start();
    }

    public void LoadChilds() {
      while (true) {
        this._ResetEvent.WaitOne();
        var handle = childsQueue.Dequeue();
        TreeNode node = null;
        IntPtr treeHandle = IntPtr.Zero;
        var visible = true;
        //var pidl = IntPtr.Zero;
        if (ShellTreeView != null) {
          this.ShellTreeView.Invoke((Action)(() => {

            node = TreeNode.FromHandle(ShellTreeView, handle);
            treeHandle = this.ShellTreeView.Handle;
            if (node != null) {
              visible = node.IsVisible;
              //if (node.Tag != null)
              //  pidl = ((IListItemEx)node.Tag).PIDL;
            }

          }));
        }

        if (!visible)
          continue;

        if (node != null && node.Nodes.Count > 0) {
          var childItem = node.Nodes[0];
          if (childItem != null) {
            var nodeHandle = childItem.Handle;

            //TODO: Try to remove this Try Catch! It's slowing this down!!
            //TODO: Have the try catch only around the error causing code
            try {
              var sho = (node.Tag as IListItemEx);// FileSystemListItem.ToFileSystemItem(IntPtr.Zero, pidl);
              if (!sho.HasSubFolders) {
                User32.SendMessage(treeHandle, BExplorer.Shell.Interop.MSG.TVM_DELETEITEM, 0, nodeHandle);
              }
              this.CheckedFroChilds.Add(handle);
              //sho.Dispose();
            } catch (Exception) {
            }
          }
        }
      }
    }

    /*
    public void RenameNode(TreeNode node) {
      if (node != null && !node.IsEditing) {
        node.BeginEdit();
      }
    }
    */

    public void RenameSelectedNode() {
      var node = this.ShellTreeView.SelectedNode;
      if (node != null && !node.IsEditing) {
        node.BeginEdit();
      }
    }

    public void DoMove(F.IDataObject dataObject, IListItemEx destination) {
      var handle = this.Handle;
      var thread = new Thread(() => {
        var items = new IShellItem[0];
        if (dataObject.GetDataPresent("FileDrop"))
          items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => FileSystemListItem.ToFileSystemItem(IntPtr.Zero, s.ToShellParsingName()).ComInterface).ToArray();
        else
          items = dataObject.ToShellItemArray().ToArray();

        try {
          var fo = new IIFileOperation(handle);
          foreach (var item in items) {
            fo.MoveItem(item, destination.ComInterface, null);
          }

          fo.PerformOperations();
        } catch (SecurityException) {
          throw;
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    public void DoCopy(F.IDataObject dataObject, IListItemEx destination) {
      var handle = this.Handle;
      var thread = new Thread(() => {
        var items = new IShellItem[0];
        if (dataObject.GetDataPresent("FileDrop"))
          items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => FileSystemListItem.ToFileSystemItem(IntPtr.Zero, s.ToShellParsingName()).ComInterface).ToArray();
        else
          items = dataObject.ToShellItemArray().ToArray();

        try {
          var fo = new IIFileOperation(handle);
          foreach (var item in items) {
            fo.CopyItem(item, destination);
          }

          fo.PerformOperations();
        } catch (SecurityException) {
          throw;
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    public void PasteAvailableFiles() {
      var selectedItem = this.ShellTreeView.SelectedNode.Tag as IListItemEx;
      if (selectedItem == null)
        return;
      var handle = this.Handle;
      var thread = new Thread(() => {
        var dataObject = F.Clipboard.GetDataObject();
        var dropEffect = dataObject.ToDropEffect();
        var items = new IShellItem[0];
        if (dataObject.GetDataPresent("FileDrop"))
          items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => FileSystemListItem.ToFileSystemItem(IntPtr.Zero, s.ToShellParsingName()).ComInterface).ToArray();
        else
          items = dataObject.ToShellItemArray().ToArray();


        try {
          var fo = new IIFileOperation(handle);
          foreach (var item in items) {
            //if (dropEffect == System.Windows.DragDropEffects.Copy)
            //	fo.CopyItem(item, selectedItem);
            //else
            //	fo.MoveItem(item, selectedItem.ComInterface, null);
          }

          fo.PerformOperations();
        } catch (SecurityException) {
          throw;
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    public void CutSelectedFiles() {
      var item = new TVITEMW() {
        mask = TVIF.TVIF_STATE,
        stateMask = TVIS.TVIS_CUT,
        state = TVIS.TVIS_CUT,
        hItem = this.ShellTreeView.SelectedNode.Handle,
      };

      User32.SendMessage(this.ShellTreeView.Handle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW, 0, ref item);

      this.cuttedNode = this.ShellTreeView.SelectedNode;
      var selectedItems = new[] { this.ShellTreeView.SelectedNode.Tag as IListItemEx };
      var ddataObject = new F.DataObject();
      // Copy or Cut operation (5 = copy; 2 = cut)
      ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 2, 0, 0, 0 }));
      ddataObject.SetData("Shell IDList Array", true, selectedItems.CreateShellIDList());
      F.Clipboard.SetDataObject(ddataObject, true);
    }

    public void CopySelectedFiles() {
      var selectedItems = new[] { this.ShellTreeView.SelectedNode.Tag as IListItemEx };
      var ddataObject = new F.DataObject();
      ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 5, 0, 0, 0 }));
      ddataObject.SetData("Shell IDList Array", true, selectedItems.CreateShellIDList());
      F.Clipboard.SetDataObject(ddataObject, true);
    }

    #endregion Public Methods

    #region Events

    private void ShellTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e) {
      e.DrawDefault = !String.IsNullOrEmpty(e.Node.Text);
      try {
        if (e.Node.Tag != null) {
          var item = e.Node.Tag as IListItemEx;
          if (!UpdatedImages.Contains(e.Node.Handle) && (item != null && item.Parent != null && item.Parent.ParsingName != KnownFolders.Network.ParsingName))
            this.RequestTreeImage(e.Node.Handle);
          if (!CheckedFroChilds.Contains(e.Node.Handle))
            this.RequestLoadChilds(e.Node.Handle);
        }
      } catch (Exception) {
        e.DrawDefault = true;
        //Do Nothing but prevent UI freeze
      }
    }

    private async void ShellTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
      if (e.Action == TreeViewAction.Collapse)
        this._AcceptSelection = false;
      if (e.Action == TreeViewAction.Expand) {
        this._ResetEvent.Reset();
        if (e.Node.Nodes.Count > 0 && (e.Node.Nodes[0].Text == this._EmptyItemString || e.Node.Nodes[0].Text == this._SearchingForFolders)) {
          e.Node.Nodes.Clear();
          imagesQueue.Clear();
          childsQueue.Clear();
          var sho = (IListItemEx)e.Node.Tag;
          var lvSho = this.ShellListView != null && this.ShellListView.CurrentFolder != null ? this.ShellListView.CurrentFolder : null;
          var node = e.Node;
          node.Nodes.Add(this._SearchingForFolders);
          var t = new Thread(() => {
            //var nodes = await Task.Run(() => {
            var nodesTemp = new List<TreeNode>();
            foreach (var item in sho.Where(w => sho.IsFileSystem || Path.GetExtension(sho.ParsingName).ToLowerInvariant() == ".library-ms" ? ((w.IsFolder || w.IsLink) && (this.IsShowHiddenItems ? true : w.IsHidden == false)) : true)) {
              var itemNode = new TreeNode(item.DisplayName);
              IListItemEx itemReal = null;
              if (item.Parent != null && item.Parent.Parent != null && item.Parent.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
                itemReal = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, item.ParsingName.ToShellParsingName());
              } else {
                itemReal = item;
              }
              itemNode.Tag = itemReal;

              if ((sho.IsNetworkPath || sho.IsNetworkPath) && sho.ParsingName != KnownFolders.Network.ParsingName) {
                itemNode.ImageIndex = this.folderImageListIndex;
              } else if (itemReal.IconType == IExtractIconPWFlags.GIL_PERCLASS || sho.ParsingName == KnownFolders.Network.ParsingName) {
                itemNode.ImageIndex = itemReal.GetSystemImageListIndex(itemReal.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
                itemNode.SelectedImageIndex = itemNode.ImageIndex;
              } else {
                itemNode.ImageIndex = this.folderImageListIndex;
              }

              itemNode.Nodes.Add(this._EmptyItemString);
              if (item.ParsingName.EndsWith(".library-ms")) {
                var library = ShellLibrary.Load(Path.GetFileNameWithoutExtension(item.ParsingName), false);
                if (library.IsPinnedToNavigationPane)
                  nodesTemp.Add(itemNode);

                library.Close();
              } else {
                nodesTemp.Add(itemNode);
              }
              //Application.DoEvents();
            }
            //return nodesTemp;
            //});
            this.Invoke((Action)(() => {
              if (node.Nodes.Count == 1 && node.Nodes[0].Text == _SearchingForFolders)
                node.Nodes.RemoveAt(0);
              node.Nodes.AddRange(nodesTemp.ToArray());
              if (lvSho != null)
                this.SelItem(lvSho);
            }));

          });
          t.Start();
        }
      }
    }

    private void ShellTreeView_HandleDestroyed(object sender, EventArgs e) {
      if (imagesThread != null)
        imagesThread.Abort();
      if (childsThread != null)
        childsThread.Abort();
    }

    private void ShellTreeView_AfterExpand(object sender, TreeViewEventArgs e) {
      GC.Collect();
      this._ResetEvent.Set();
    }

    private void ShellTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
      if (!this._AcceptSelection) {
        this._AcceptSelection = true;
        return;
      }
      var sho = e.Node != null ? e.Node.Tag as IListItemEx : null;
      if (sho != null) {
        IListItemEx linkSho = null;
        if (sho.IsLink) {
          try {
            var shellLink = new ShellLink(sho.ParsingName);
            var linkTarget = shellLink.TargetPIDL;
            linkSho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, linkTarget);
            shellLink.Dispose();
          } catch { }
        }

        this.isFromTreeview = true;
        if (this._IsNavigate) {
          this.ShellListView.Navigate_Full(linkSho ?? sho, true, true);
        }

        this._IsNavigate = false;
        this.isFromTreeview = false;
      }
    }

    private void ShellListView_Navigated(object sender, NavigatedEventArgs e) {
      if (!this.isFromTreeview) {
        this.SelItem(e.Folder);
      }
    }

    private void ShellTreeView_ItemDrag(object sender, ItemDragEventArgs e) {
      IntPtr dataObjPtr = IntPtr.Zero;
      var shellItem = ((e.Item as TreeNode).Tag as IListItemEx);
      if (shellItem != null) {
        //System.Runtime.InteropServices.ComTypes.IDataObject dataObject = shellItem.GetIDataObject(out dataObjPtr);

        uint ef = 0;
        //Shell32.SHDoDragDrop(this.ShellListView.Handle, dataObject, null, unchecked((uint)F.DragDropEffects.All | (uint)F.DragDropEffects.Link), out ef);
      }
    }

    private void ShellTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
      if (e.Button == F.MouseButtons.Right) {
        if (e.Node.Tag != null)
          new ShellContextMenu(e.Node.Tag as IListItemEx).ShowContextMenu(this, e.Location, CMF.CANRENAME);
      } else if (e.Button == F.MouseButtons.Left) {
        if (e.X > e.Node.Bounds.Left - 5 - 16)
          this._IsNavigate = true;
      }
    }

    private void ShellTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
      if (e.Label != null) {
        if (e.Label.Length > 0) {
          if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1) {
            // Stop editing without canceling the label change.
            e.Node.EndEdit(false);
            var fo = new IIFileOperation(this.Handle);
            fo.RenameItem((e.Node.Tag as IListItemEx).ComInterface, e.Label);
            fo.PerformOperations();
          } else {
            /* Cancel the label edit action, inform the user, and
               place the node in edit mode again. */
            e.CancelEdit = true;
            MessageBox.Show("Invalid tree node label.\n" +
               "The invalid characters are: '@','.', ',', '!'",
               "Node Label Edit");
            e.Node.BeginEdit();
          }
        } else {
          /* Cancel the label edit action, inform the user, and place the node in edit mode again. */
          e.CancelEdit = true;
          MessageBox.Show("Invalid tree node label.\nThe label cannot be blank", "Node Label Edit");
          e.Node.BeginEdit();
        }
      }
    }

    private void ShellTreeView_KeyDown(object sender, KeyEventArgs e) {
      if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
        switch (e.KeyCode) {
          case Keys.C:
            this.CopySelectedFiles();
            break;

          case Keys.V:
            this.PasteAvailableFiles();
            break;

          case Keys.X:
            this.CutSelectedFiles();
            break;
        }
      }
      if (e.KeyCode == Keys.F2) {
        this.RenameSelectedNode();
      }
      if (e.KeyCode == Keys.F5) {
        this.RefreshContents();
      }
      if (e.KeyCode == Keys.Escape) {
        var item = new TVITEMW() { mask = TVIF.TVIF_STATE, stateMask = TVIS.TVIS_CUT, state = 0, hItem = this.cuttedNode.Handle };
        User32.SendMessage(this.ShellTreeView.Handle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW, 0, ref item);
        Clipboard.Clear();
      }
    }

    private void ShellTreeView_DragEnter(object sender, DragEventArgs e) {
      this._DataObject = (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data;
      var wp = new BExplorer.Shell.DataObject.Win32Point() { X = e.X, Y = e.Y };
      ShellView.Drag_SetEffect(e);

      if (e.Data.GetDataPresent("DragImageBits"))
        DropTargetHelper.Get.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
      else
        base.OnDragEnter(e);
    }

    private void ShellTreeView_DragOver(object sender, DragEventArgs e) {
      var wp = new BExplorer.Shell.DataObject.Win32Point() { X = e.X, Y = e.Y };
      ShellView.Drag_SetEffect(e);
      BExplorer.Shell.DataObject.DropDescription descinvalid = new DataObject.DropDescription();
      descinvalid.type = (int)BExplorer.Shell.DataObject.DropImageType.Invalid;
      ((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(descinvalid);
      var node = this.ShellTreeView.GetNodeAt(PointToClient(new Point(e.X, e.Y)));
      if (node != null && !String.IsNullOrEmpty(node.Text) && node.Text != this._EmptyItemString) {
        User32.SendMessage(this.ShellTreeView.Handle, BExplorer.Shell.Interop.MSG.TVM_SETHOT, 0, node.Handle);
        BExplorer.Shell.DataObject.DropDescription desc = new DataObject.DropDescription();
        switch (e.Effect) {
          case System.Windows.Forms.DragDropEffects.Copy:
            desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Copy;
            desc.szMessage = "Copy To %1";
            break;
          case System.Windows.Forms.DragDropEffects.Link:
            desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Link;
            desc.szMessage = "Create Link in %1";
            break;
          case System.Windows.Forms.DragDropEffects.Move:
            desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Move;
            desc.szMessage = "Move To %1";
            break;
          case System.Windows.Forms.DragDropEffects.None:
            desc.type = (int)BExplorer.Shell.DataObject.DropImageType.None;
            desc.szMessage = "";
            break;
          default:
            desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Invalid;
            desc.szMessage = "";
            break;
        }
        desc.szInsert = node.Text;
        ((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);
      }

      if (e.Data.GetDataPresent("DragImageBits"))
        DropTargetHelper.Get.Create.DragOver(ref wp, (int)e.Effect);
      else
        base.OnDragOver(e);
    }

    private void ShellTreeView_DragLeave(object sender, EventArgs e) {
      DropTargetHelper.Get.Create.DragLeave();
    }

    private void ShellTreeView_DragDrop(object sender, DragEventArgs e) {
      var hittestInfo = this.ShellTreeView.HitTest(PointToClient(new Point(e.X, e.Y)));
      IListItemEx destination = null;
      if (hittestInfo.Node == null)
        e.Effect = DragDropEffects.None;
      else
        destination = hittestInfo.Node.Tag as IListItemEx;

      switch (e.Effect) {
        case F.DragDropEffects.Copy:
          //this.DoCopy(e.Data, destination);
          break;

        case F.DragDropEffects.Link:
          System.Windows.MessageBox.Show("link");
          break;

        case F.DragDropEffects.Move:
          this.DoMove(e.Data, destination);
          break;

        case F.DragDropEffects.All:
        case F.DragDropEffects.None:
        case F.DragDropEffects.Scroll:
          break;

        default:
          break;
      }

      var wp = new BExplorer.Shell.DataObject.Win32Point() { X = e.X, Y = e.Y };

      if (e.Data.GetDataPresent("DragImageBits"))
        DropTargetHelper.Get.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
      else
        base.OnDragDrop(e);
    }

    #endregion Events

    #region Initializer

    public ShellTreeViewEx() {
      InitTreeView();

      this.Controls.Add(ShellTreeView);

      InitializeComponent();

      InitRootItems();
      this._NotificationNetWork.RegisterChangeNotify(this.Handle, ShellNotifications.CSIDL.CSIDL_NETWORK, false);
      this._NotificationGlobal.RegisterChangeNotify(this.Handle, ShellNotifications.CSIDL.CSIDL_DESKTOP, true);
    }

    #endregion Initializer
    protected override void OnHandleDestroyed(EventArgs e) {
      this._NotificationNetWork.UnregisterChangeNotify();
      this._NotificationGlobal.UnregisterChangeNotify();
      base.OnHandleDestroyed(e);
    }
    private List<String> _PathsToBeAdd = new List<String>();
    [HandleProcessCorruptedStateExceptions]
    protected override void WndProc(ref Message m) {
      base.WndProc(ref m);
      if (m.Msg == ShellNotifications.WM_SHNOTIFY) {
        //MessageBox.Show("1");
        if (this._NotificationGlobal.NotificationReceipt(m.WParam, m.LParam)) {
          var computerNode = this.ShellTreeView.Nodes.OfType<TreeNode>().Single(s => s.Tag != null && (s.Tag as IListItemEx).ParsingName == KnownFolders.Computer.ParsingName);
          foreach (NotifyInfos info in this._NotificationGlobal.NotificationsReceived.ToArray()) {
            switch (info.Notification) {
              case ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER:
                var objPrevDir = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item1);
                var objNewDir = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item2);
                this.RenameItem(objPrevDir, objNewDir);
                break;
              case ShellNotifications.SHCNE.SHCNE_MKDIR:
                try {
                  var objAddDir = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item1);
                  this.AddItem(objAddDir);
                } catch (FileNotFoundException) { }
                break;
              case ShellNotifications.SHCNE.SHCNE_RMDIR:
                var objDelDir = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item1);
                this.DeleteItem(objDelDir);
                break;
              case ShellNotifications.SHCNE.SHCNE_MEDIAINSERTED:
              case ShellNotifications.SHCNE.SHCNE_MEDIAREMOVED:
                var objDm = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item1);
                var exisitingMItem = computerNode.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as IListItemEx).Equals(objDm));
                if (exisitingMItem != null) {
                  exisitingMItem.Text = objDm.DisplayName;
                  exisitingMItem.ImageIndex = objDm.GetSystemImageListIndex(objDm.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
                  exisitingMItem.SelectedImageIndex = exisitingMItem.ImageIndex;
                }
                break;
              case ShellNotifications.SHCNE.SHCNE_DRIVEREMOVED:
                var objDr = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item1);
                try {
                  computerNode.Nodes.Remove(computerNode.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as IListItemEx).ParsingName == objDr.ParsingName));
                } catch (NullReferenceException) {

                }
                objDr.Dispose();
                break;
              case ShellNotifications.SHCNE.SHCNE_DRIVEADD:
                var objDa = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item1);

                var exisitingItem = computerNode.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as IListItemEx).Equals(objDa));
                if (exisitingItem == null && this._PathsToBeAdd.Count(c => c.Equals(objDa.ParsingName, StringComparison.InvariantCultureIgnoreCase)) == 0) {
                  this._PathsToBeAdd.Add(objDa.ParsingName);
                  var newDrive = new TreeNode(objDa.DisplayName);
                  newDrive.Tag = objDa;
                  newDrive.ImageIndex = objDa.GetSystemImageListIndex(objDa.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
                  newDrive.SelectedImageIndex = newDrive.ImageIndex;
                  if (objDa.HasSubFolders)
                    newDrive.Nodes.Add(_EmptyItemString);
                  var nodesList = computerNode.Nodes.OfType<TreeNode>().Where(w => w.Tag != null).Select(s => s.Tag as IListItemEx).ToList();
                  nodesList.Add(objDa);
                  nodesList = nodesList.OrderBy(o => o.ParsingName).ToList();
                  var indexToInsert = nodesList.IndexOf(objDa);
                  nodesList = null;
                  GC.Collect();
                  computerNode.Nodes.Insert(indexToInsert, newDrive);
                  this._PathsToBeAdd.Clear();
                }
                break;
              case ShellNotifications.SHCNE.SHCNE_UPDATEDIR:
                break;
              default:
                break;
            }
            this._NotificationGlobal.NotificationsReceived.Remove(info);
          }
        }
        if (this._NotificationNetWork.NotificationReceipt(m.WParam, m.LParam)) {
          foreach (NotifyInfos info in _NotificationNetWork.NotificationsReceived.ToArray()) {
            switch (info.Notification) {
              case ShellNotifications.SHCNE.SHCNE_RENAMEITEM:
                break;
              case ShellNotifications.SHCNE.SHCNE_MKDIR:
              //case ShellNotifications.SHCNE.SHCNE_CREATE:
                try {
                  var sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, info.Item1);
                  if (sho.Parent == null || !sho.Parent.ParsingName.Equals(KnownFolders.Network.ParsingName, StringComparison.InvariantCultureIgnoreCase))
                    break;
                  var existingItem = this.ShellTreeView.Nodes.OfType<TreeNode>().Last().Nodes.OfType<TreeNode>().Where(w => (w.Tag as IListItemEx) != null && (w.Tag as IListItemEx).ParsingName == sho.ParsingName).Count();
                  if (existingItem > 0)
                    break;
                  var node = new TreeNode(sho.DisplayName);
                  node.ImageIndex = sho.GetSystemImageListIndex(sho.PIDL, ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);// this.folderImageListIndex;
                  node.SelectedImageIndex = node.ImageIndex;
                  node.Tag = sho;
                  if (sho.HasSubFolders) {
                    node.Nodes.Add(this._SearchingForFolders);
                  }
                  //if (sho != null && sho.Parent != null && sho.Parent.ParsingName == KnownFolders.Network.ParsingName) {
                  //var firstNode = this.ShellTreeView.Nodes.OfType<TreeNode>().Last().Nodes.OfType<TreeNode>().FirstOrDefault();
                  //if (firstNode != null && firstNode.Text.Equals(this._SearchingForFolders)) {
                  //  firstNode.Remove();
                  //}
                  this.ShellTreeView.Nodes.OfType<TreeNode>().Last().Nodes.Add(node);
                  //}
                } catch (AccessViolationException) {

                } catch (NullReferenceException) {

                }
                break;
              case ShellNotifications.SHCNE.SHCNE_DELETE:
                break;
              case ShellNotifications.SHCNE.SHCNE_RMDIR:
                break;
              case ShellNotifications.SHCNE.SHCNE_MEDIAINSERTED:
                break;
              case ShellNotifications.SHCNE.SHCNE_MEDIAREMOVED:
                break;
              case ShellNotifications.SHCNE.SHCNE_DRIVEREMOVED:
                break;
              case ShellNotifications.SHCNE.SHCNE_DRIVEADD:
                break;
              case ShellNotifications.SHCNE.SHCNE_NETSHARE:
                break;
              case ShellNotifications.SHCNE.SHCNE_NETUNSHARE:
                break;
              case ShellNotifications.SHCNE.SHCNE_ATTRIBUTES:
                break;
              case ShellNotifications.SHCNE.SHCNE_UPDATEDIR:
                break;
              case ShellNotifications.SHCNE.SHCNE_UPDATEITEM:
                break;
              case ShellNotifications.SHCNE.SHCNE_SERVERDISCONNECT:
                break;
              case ShellNotifications.SHCNE.SHCNE_UPDATEIMAGE:
                break;
              case ShellNotifications.SHCNE.SHCNE_DRIVEADDGUI:
                break;
              case ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER:
                break;
              case ShellNotifications.SHCNE.SHCNE_FREESPACE:
                break;
              case ShellNotifications.SHCNE.SHCNE_EXTENDED_EVENT:
                break;
              case ShellNotifications.SHCNE.SHCNE_ASSOCCHANGED:
                break;
              case ShellNotifications.SHCNE.SHCNE_DISKEVENTS:
                break;
              case ShellNotifications.SHCNE.SHCNE_GLOBALEVENTS:
                break;
              case ShellNotifications.SHCNE.SHCNE_ALLEVENTS:
                break;
              case ShellNotifications.SHCNE.SHCNE_INTERRUPT:
                break;
              default:
                break;
            }
            _NotificationNetWork.NotificationsReceived.Remove(info);
          }

        }
      }
    }
  }
}