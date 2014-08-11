using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using BExplorer.Shell.Interop;
using System.Runtime.InteropServices;
using System.IO;
using System.Security;
using F = System.Windows.Forms;

namespace BExplorer.Shell {
	public partial class ShellTreeViewEx : UserControl {
		#region Event Handlers
		public event EventHandler<TreeNodeMouseClickEventArgs> NodeClick;
		#endregion

		#region Public Members
		public TreeViewBase ShellTreeView;
		public Boolean IsShowHiddenItems { get; set; }
		public ShellView ShellListView {
			get {
				return _ShellListView;
			}
			set {
				_ShellListView = value;
				_ShellListView.Navigated += ShellListView_Navigated;
			}
		}
		#endregion

		#region Private Members

		private TreeNode cuttedNode { get; set; }
		private int folderImageListIndex;
		private ShellView _ShellListView;
		private List<IntPtr> UpdatedImages = new List<IntPtr>();
		private List<IntPtr> CheckedFroChilds = new List<IntPtr>();
		private SyncQueue<IntPtr> imagesQueue = new SyncQueue<IntPtr>(); //7000
		private SyncQueue<IntPtr> childsQueue = new SyncQueue<IntPtr>(); //7000
		private Thread imagesThread;
		private Thread childsThread;
		private Boolean isFromTreeview;
		private Boolean _IsNavigate;
		#endregion

		#region Private Methods
		private void InitRootItems() {
			this.imagesQueue.Clear();
			this.childsQueue.Clear();
			this.UpdatedImages.Clear();
			this.CheckedFroChilds.Clear();
			var favoritesItem = (ShellItem)KnownFolders.Links;
			TreeNode favoritesRoot = new TreeNode((favoritesItem).DisplayName);
			favoritesRoot.Tag = KnownFolders.Links;
			favoritesRoot.ImageIndex = ((ShellItem)KnownFolders.Favorites).GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			favoritesRoot.SelectedImageIndex = favoritesRoot.ImageIndex;
			if (favoritesItem.Count() > 0)
				favoritesRoot.Nodes.Add("<!EMPTY!>");

			var librariesItem = (ShellItem)KnownFolders.Libraries;
			TreeNode librariesRoot = new TreeNode(librariesItem.DisplayName);
			librariesRoot.Tag = KnownFolders.Libraries;
			librariesRoot.ImageIndex = librariesItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			librariesRoot.SelectedImageIndex = librariesRoot.ImageIndex;
			if (librariesItem.HasSubFolders)
				librariesRoot.Nodes.Add("<!EMPTY!>");


			var computerItem = (ShellItem)KnownFolders.Computer;
			TreeNode computerRoot = new TreeNode(computerItem.DisplayName);
			computerRoot.Tag = KnownFolders.Computer;
			computerRoot.ImageIndex = computerItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			computerRoot.SelectedImageIndex = computerRoot.ImageIndex;
			if (computerItem.HasSubFolders)
				computerRoot.Nodes.Add("<!EMPTY!>");

			var networkItem = (ShellItem)KnownFolders.Network;
			var networkRoot = new TreeNode(networkItem.DisplayName);
			networkRoot.Tag = networkItem;
			networkRoot.ImageIndex = networkItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			networkRoot.SelectedImageIndex = networkRoot.ImageIndex;

			ShellTreeView.Nodes.Add(favoritesRoot);
			favoritesRoot.Expand();
			ShellTreeView.Nodes.Add(new TreeNode());
			ShellTreeView.Nodes.Add(librariesRoot);
			ShellTreeView.Nodes.Add(new TreeNode());
			ShellTreeView.Nodes.Add(computerRoot);
			ShellTreeView.Nodes.Add(new TreeNode());
			ShellTreeView.Nodes.Add(networkRoot);
			librariesRoot.Expand();
			computerRoot.Expand();
		}

		void CheckForChildFolders(IntPtr node, IntPtr pidl, IntPtr m_TreeViewHandle, IntPtr parentHandle) {
			try {
				var sho = new ShellItem(pidl);
				if (!sho.HasSubFolders) {
					User32.SendMessage(m_TreeViewHandle, BExplorer.Shell.Interop.MSG.TVM_DELETEITEM, 0, node);
				}
				this.CheckedFroChilds.Add(parentHandle);
				sho.Dispose();
			}
			catch (Exception) {

			}
		}

		void SetNodeImage(IntPtr node, IntPtr pidl, IntPtr m_TreeViewHandle, Boolean isOverlayed) {
			try {
				TVITEMW itemInfo = new TVITEMW();

				// We need to set the images for the item by sending a 
				// TVM_SETITEMW message, as we need to set the overlay images,
				// and the .Net TreeView API does not support overlays.
				itemInfo.mask = TVIF.TVIF_IMAGE | TVIF.TVIF_SELECTEDIMAGE |
												TVIF.TVIF_STATE;
				itemInfo.hItem = node;
				itemInfo.iImage = ShellItem.GetSystemImageListIndex(pidl,
							ShellIconType.SmallIcon, ShellIconFlags.OverlayIndex);
				if (isOverlayed) {
					itemInfo.state = (TVIS)(itemInfo.iImage >> 16);
					itemInfo.stateMask = TVIS.TVIS_OVERLAYMASK;
				}
				itemInfo.iSelectedImage = ShellItem.GetSystemImageListIndex(pidl,
						ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
				this.UpdatedImages.Add(node);
				User32.SendMessage(m_TreeViewHandle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW,
						0, ref itemInfo);
			}
			catch (Exception) {

			}
		}
		void SelectItem(ShellItem item) {
			if (item.IsSearchFolder)
				return;

			var listNodes = this.ShellTreeView.Nodes.OfType<TreeNode>().ToList();
			listNodes.AddRange(this.ShellTreeView.Nodes.OfType<TreeNode>().SelectMany(s => s.Nodes.OfType<TreeNode>()).ToArray());
			var nodes = listNodes.ToArray();
			var separators = new char[] {
				Path.DirectorySeparatorChar,  
				Path.AltDirectorySeparatorChar  
			};
			var directories = item.ParsingName.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			List<ShellItem> items = new List<ShellItem>();

			for (int i = 0; i < directories.Length; i++) {
				if (i == 0) {
					items.Add(new ShellItem(directories[i].ToShellParsingName()));
				}
				else {
					string path = String.Empty;
					for (int j = 0; j <= i; j++) {
						if (j == 0) {
							path = directories[j];
						}
						else {
							path = String.Format("{0}{1}{2}", path, Path.DirectorySeparatorChar, directories[j]);
						}
					}
					var shellItem = new ShellItem(path.ToShellParsingName());
					items.Add(shellItem);
				}
			}

			foreach (var sho in items) {
				var theNode = nodes.Where(wr => wr.Tag != null).Where(w => (w.Tag as ShellItem) != null && (w.Tag as ShellItem).GetDisplayName(SIGDN.DESKTOPABSOLUTEEDITING).ToLowerInvariant() == sho.GetDisplayName(SIGDN.DESKTOPABSOLUTEEDITING).ToLowerInvariant()).SingleOrDefault();
				if (theNode != null) {
					if (items.Last() == sho) {
						if (this.ShellTreeView.SelectedNode != theNode) {
							this.ShellTreeView.SelectedNode = theNode;
							theNode.EnsureVisible();
						}
					}
					else {
						theNode.Expand();
						nodes = theNode.Nodes.OfType<TreeNode>().ToArray();
					}
				}
				else {
					return;
				}
			}
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
			this.ShellTreeView.MouseMove += ShellListView_MouseMove;
			this.ShellTreeView.MouseEnter += ShellTreeView_MouseEnter;
			this.ShellTreeView.MouseLeave += ShellTreeView_MouseLeave;
			if (this.ShellListView != null) {
				this.ShellListView.Navigated += ShellListView_Navigated;
			}
			SystemImageList.UseSystemImageList(ShellTreeView);
			ShellTreeView.FullRowSelect = true;
			Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO();
			defIconInfo.cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO));
			Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_FOLDER, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
			this.folderImageListIndex = defIconInfo.iSysIconIndex;
			imagesThread = new Thread(new ThreadStart(LoadTreeImages));
			imagesThread.IsBackground = true;
			imagesThread.Start();
			childsThread = new Thread(new ThreadStart(LoadChilds));
			childsThread.IsBackground = true;
			childsThread.Start();
		}

		void ShellTreeView_MouseLeave(object sender, EventArgs e) {
			if (this.ShellListView != null) {
				this.ShellListView.IsFocusAllowed = true;
			}
		}

		void ShellTreeView_MouseEnter(object sender, EventArgs e) {
			if (this.ShellListView != null) {
				this.ShellListView.IsFocusAllowed = false;
			}
			this.ShellTreeView.Focus();
		}

		void ShellListView_MouseMove(object sender, MouseEventArgs e) {
			if (!this.ShellTreeView.Focused)
				this.ShellTreeView.Focus();
		}

		void ShellTreeView_MouseDown(object sender, MouseEventArgs e) {
			if (NodeClick != null) {
				var treeNode = this.ShellTreeView.GetNodeAt(e.X, e.Y);
				NodeClick.Invoke(this, new TreeNodeMouseClickEventArgs(treeNode, e.Button, e.Clicks, e.X, e.Y));
			}
		}

		#endregion

		#region Public Methods
		public void RefreshContents() {
			this.ShellTreeView.Nodes.Clear();
			InitRootItems();
			if (this.ShellListView != null && this.ShellListView.CurrentFolder != null) {
				SelectItem(this.ShellListView.CurrentFolder);
			}
		}
		public void LoadTreeImages() {
			while (true) {
				var handle = imagesQueue.Dequeue();
				TreeNode node = null;
				IntPtr treeHandle = IntPtr.Zero;
				var hash = -1;
				var pidl = IntPtr.Zero;
				var visible = false;
				this.ShellTreeView.Invoke((Action)(() => {
					node = TreeNode.FromHandle(ShellTreeView, handle);
					if (node != null) {
						treeHandle = this.ShellTreeView.Handle;
						hash = (node.Tag as ShellItem).GetHashCode();
						pidl = (node.Tag as ShellItem).AbsolutePidl;
						visible = node.IsVisible;
					}
				}));
				if (visible) {
					var nodeHandle = handle;
					Thread.Sleep(1);
					Application.DoEvents();

					SetNodeImage(nodeHandle, pidl, treeHandle, !(node.Parent != null && (node.Parent.Tag as ShellItem).ParsingName == KnownFolders.Links.ParsingName));
				}
			}
		}

		public void LoadChilds() {
			while (true) {
				var handle = childsQueue.Dequeue();
				TreeNode node = null;
				IntPtr treeHandle = IntPtr.Zero;
				var visible = true;
				var pidl = IntPtr.Zero;
				Thread.Sleep(2);
				Application.DoEvents();
				this.ShellTreeView.Invoke((Action)(() => {
					node = TreeNode.FromHandle(ShellTreeView, handle);
					treeHandle = this.ShellTreeView.Handle;
					if (node != null) {
						visible = node.IsVisible;
						pidl = (node.Tag as ShellItem).Pidl;
					}
				}));

				if (!visible || pidl == IntPtr.Zero)
					continue;

				if (node != null) {
					var childItem = node.Nodes.OfType<TreeNode>().FirstOrDefault();
					if (childItem != null) {
						var nodeHandle = childItem.Handle;

						CheckForChildFolders(nodeHandle, pidl, treeHandle, handle);
					}
				}
			}
		}

		public void RenameNode(TreeNode node) {
			if (node != null && !node.IsEditing) {
				node.BeginEdit();
			}
		}

		public void RenameSelectedNode() {
			var node = this.ShellTreeView.SelectedNode;
			if (node != null && !node.IsEditing) {
				node.BeginEdit();
			}
		}

		public void DoMove(F.IDataObject dataObject, ShellItem destination) {
			var handle = this.Handle;
			var thread = new Thread(() => {
				var shellItemArray = dataObject.ToShellItemArray();
				var items = shellItemArray.ToArray();
				try {
					IIFileOperation fo = new IIFileOperation(handle);
					foreach (var item in items) {
						fo.MoveItem(item, destination.ComInterface, null);
					}

					fo.PerformOperations();
				}
				catch (SecurityException) {
					throw;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

		}
		public void DoCopy(F.IDataObject dataObject, ShellItem destination) {
			var handle = this.Handle;
			var thread = new Thread(() => {
				var shellItemArray = dataObject.ToShellItemArray();
				var items = shellItemArray.ToArray();
				try {
					IIFileOperation fo = new IIFileOperation(handle);
					foreach (var item in items) {
						fo.CopyItem(item, destination.ComInterface, String.Empty);
					}

					fo.PerformOperations();
				}
				catch (SecurityException) {
					throw;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}
		public void PasteAvailableFiles() {
			var selectedItem = this.ShellTreeView.SelectedNode.Tag as ShellItem;
			if (selectedItem == null)
				return;

			var handle = this.Handle;
			var thread = new Thread(() => {
				var dataObject = F.Clipboard.GetDataObject();
				var dragDropEffect = System.Windows.DragDropEffects.Copy;
				var dropEffect = dataObject.ToDropEffect();
				var shellItemArray = dataObject.ToShellItemArray();
				var items = shellItemArray.ToArray();
				try {
					IIFileOperation fo = new IIFileOperation(handle);
					foreach (var item in items) {
						if (dropEffect == System.Windows.DragDropEffects.Copy) {
							fo.CopyItem(item, selectedItem.ComInterface, String.Empty);
						}
						else {
							fo.MoveItem(item, selectedItem.ComInterface, null);
						}
					}

					fo.PerformOperations();
				}
				catch (SecurityException) {
					throw;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}
		public void CutSelectedFiles() {
			TVITEMW item = new TVITEMW();
			item.hItem = this.ShellTreeView.SelectedNode.Handle;
			item.mask = TVIF.TVIF_STATE;
			item.stateMask = TVIS.TVIS_CUT;
			item.state = TVIS.TVIS_CUT;
			User32.SendMessage(this.ShellTreeView.Handle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW,
						0, ref item);

			this.cuttedNode = this.ShellTreeView.SelectedNode;
			var selectedItems = new ShellItem[1];
			selectedItems[0] = this.ShellTreeView.SelectedNode.Tag as ShellItem;
			var ddataObject = new F.DataObject();
			// Copy or Cut operation (5 = copy; 2 = cut)
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 2, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, selectedItems.CreateShellIDList());
			F.Clipboard.SetDataObject(ddataObject, true);

		}
		public void CopySelectedFiles() {
			IntPtr dataObjPtr = IntPtr.Zero;

			var selectedItems = new ShellItem[1];
			selectedItems[0] = this.ShellTreeView.SelectedNode.Tag as ShellItem;
			//System.Runtime.InteropServices.ComTypes.IDataObject dataObject = GetIDataObject(this.SelectedItems.ToArray(), out dataObjPtr);
			var ddataObject = new F.DataObject();
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 5, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, selectedItems.CreateShellIDList());
			F.Clipboard.SetDataObject(ddataObject, true);

		}
		#endregion

		#region Events
		void ShellTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e) {
			e.DrawDefault = !String.IsNullOrEmpty(e.Node.Text);
			if (e.Node.Tag != null) {
				var item = e.Node.Tag as ShellItem;

				//if (item != null && item.Parent != null && (item.Parent.IsNetDrive || item.Parent.IsNetworkPath))
				//{
				if (!UpdatedImages.Contains(e.Node.Handle))
					imagesQueue.Enqueue(e.Node.Handle);
				//}
				if (!CheckedFroChilds.Contains(e.Node.Handle))
					childsQueue.Enqueue(e.Node.Handle);
			}
		}

		async void ShellTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
			if (e.Action == TreeViewAction.Expand) {
				if (e.Node.Nodes.Count > 0 && e.Node.Nodes[0].Text == "<!EMPTY!>") {
					e.Node.Nodes.Clear();
					imagesQueue.Clear();
					childsQueue.Clear();
					var sho = e.Node.Tag as ShellItem;
					ShellItem lvSho = this.ShellListView != null && this.ShellListView.CurrentFolder != null ? this.ShellListView.CurrentFolder : null;
					var node = e.Node;
					node.Nodes.Add("Searching for folders...");
					var nodes = await Task.Run(() => {
						var nodesTemp = new List<TreeNode>();
						foreach (var item in sho.Where(w => sho.IsFileSystem || Path.GetExtension(sho.ParsingName).ToLowerInvariant() == ".library-ms" ? ((w.IsFolder || w.IsLink) && (this.IsShowHiddenItems ? true : w.IsHidden == false)) : true)) {
							var itemNode = new TreeNode(item.DisplayName);
							ShellItem itemReal = null;
							if (item.Parent != null && item.Parent.Parent != null && item.Parent.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
								itemReal = ShellItem.ToShellParsingName(item.ParsingName);
							}
							else {
								itemReal = item;
							}
							itemNode.Tag = itemReal;

							if (sho.IsNetDrive || sho.IsNetworkPath) {
								itemNode.ImageIndex = this.folderImageListIndex;
							}
							else {
								if (itemReal.IconType == IExtractIconPWFlags.GIL_PERCLASS) {
									itemNode.ImageIndex = itemReal.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
									itemNode.SelectedImageIndex = itemNode.ImageIndex;
								}
								else {
									itemNode.ImageIndex = this.folderImageListIndex;
								}

							}
							itemNode.Nodes.Add("<!EMPTY!>");
							nodesTemp.Add(itemNode);
							Application.DoEvents();
						}
						return nodesTemp;
					});

					if (node.Nodes.Count == 1 && node.Nodes[0].Text == "Searching for folders...")
						node.Nodes.RemoveAt(0);
					node.Nodes.AddRange(nodes.ToArray());

					if (lvSho != null)
						SelectItem(lvSho);
				}
			}
		}
		void ShellTreeView_HandleDestroyed(object sender, EventArgs e) {
			if (imagesThread != null)
				imagesThread.Abort();
			if (childsThread != null)
				childsThread.Abort();
		}

		void ShellTreeView_AfterExpand(object sender, TreeViewEventArgs e) {
			GC.Collect();
		}

		void ShellTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
			var sho = e.Node.Tag as ShellItem;
			if (sho != null) {
				ShellItem linkSho = null;
				if (sho.IsLink) {
					try {
						var shellLink = new ShellLink(sho.ParsingName);
						var linkTarget = shellLink.TargetPIDL;
						linkSho = new ShellItem(linkTarget);
						shellLink.Dispose();
					}
					catch { }
				}
				this.isFromTreeview = true;
				if (this._IsNavigate)
					this.ShellListView.Navigate(linkSho ?? sho, true);

				this._IsNavigate = false;
				this.isFromTreeview = false;
				e.Node.Expand();
			}
		}
		void ShellListView_Navigated(object sender, NavigatedEventArgs e) {
			if (!this.isFromTreeview) {
				this.SelectItem(e.Folder);
			}


		}

		void ShellTreeView_ItemDrag(object sender, ItemDragEventArgs e) {
			IntPtr dataObjPtr = IntPtr.Zero;
			var shellItem = ((e.Item as TreeNode).Tag as ShellItem);
			if (shellItem != null) {
				System.Runtime.InteropServices.ComTypes.IDataObject dataObject = shellItem.GetIDataObject(out dataObjPtr);

				uint ef = 0;
				Shell32.SHDoDragDrop(this.ShellListView.Handle, dataObject, null, unchecked((uint)F.DragDropEffects.All | (uint)F.DragDropEffects.Link), out ef);
			}
		}

		void ShellTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
			if (e.Button == F.MouseButtons.Right) {
				//this.ShellTreeView.SelectedNode = e.Node;
				ShellContextMenu cm = new ShellContextMenu(e.Node.Tag as ShellItem);
				cm.ShowContextMenu(this, e.Location, CMF.CANRENAME);
			}
			else if (e.Button == F.MouseButtons.Left) {
				this._IsNavigate = true;
			}

		}

		void ShellTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
			if (e.Label != null) {
				if (e.Label.Length > 0) {
					if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1) {
						// Stop editing without canceling the label change.
						e.Node.EndEdit(false);
						IIFileOperation fo = new IIFileOperation(this.Handle);
						fo.RenameItem((e.Node.Tag as ShellItem).ComInterface, e.Label);
						fo.PerformOperations();
					}
					else {
						/* Cancel the label edit action, inform the user, and 
							 place the node in edit mode again. */
						e.CancelEdit = true;
						MessageBox.Show("Invalid tree node label.\n" +
							 "The invalid characters are: '@','.', ',', '!'",
							 "Node Label Edit");
						e.Node.BeginEdit();
					}
				}
				else {
					/* Cancel the label edit action, inform the user, and 
						 place the node in edit mode again. */
					e.CancelEdit = true;
					MessageBox.Show("Invalid tree node label.\nThe label cannot be blank",
						 "Node Label Edit");
					e.Node.BeginEdit();
				}
			}
		}

		void ShellTreeView_KeyDown(object sender, KeyEventArgs e) {
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
			if (e.KeyCode == Keys.Escape) {
				TVITEMW item = new TVITEMW();
				item.hItem = this.cuttedNode.Handle;
				item.mask = TVIF.TVIF_STATE;
				item.stateMask = TVIS.TVIS_CUT;
				item.state = 0;
				User32.SendMessage(this.ShellTreeView.Handle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW,
							0, ref item);
				Clipboard.Clear();
			}
		}

		void ShellTreeView_DragEnter(object sender, DragEventArgs e) {
			var wp = new Win32Point() { X = e.X, Y = e.Y };
			ShellView.Drag_SetEffect(e);

			if (e.Data.GetDataPresent("DragImageBits")) {
				DropTargetHelper.Get.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
			}
		}

		void ShellTreeView_DragOver(object sender, DragEventArgs e) {
			var wp = new Win32Point() { X = e.X, Y = e.Y };
			ShellView.Drag_SetEffect(e);

			if (e.Data.GetDataPresent("DragImageBits"))
				DropTargetHelper.Get.Create.DragOver(ref wp, (int)e.Effect);
		}

		void ShellTreeView_DragLeave(object sender, EventArgs e) {
			DropTargetHelper.Get.Create.DragLeave();
		}

		void ShellTreeView_DragDrop(object sender, DragEventArgs e) {
			var hittestInfo = this.ShellTreeView.HitTest(PointToClient(new System.Drawing.Point(e.X, e.Y)));
			ShellItem destination = null;
			if (hittestInfo.Node == null)
				e.Effect = DragDropEffects.None;
			else
				destination = hittestInfo.Node.Tag as ShellItem;

			switch (e.Effect) {
				case F.DragDropEffects.All:
					break;
				case F.DragDropEffects.Copy:
					this.DoCopy(e.Data, destination);
					break;
				case F.DragDropEffects.Link:
					System.Windows.MessageBox.Show("link");
					break;
				case F.DragDropEffects.Move:
					this.DoMove(e.Data, destination);
					break;
				case F.DragDropEffects.None:
					break;
				case F.DragDropEffects.Scroll:
					break;
				default:
					break;
			}

			Win32Point wp = new Win32Point();
			wp.X = e.X;
			wp.Y = e.Y;

			if (e.Data.GetDataPresent("DragImageBits"))
				DropTargetHelper.Get.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);

			//if (_LastSelectedIndexByDragDrop != -1 & !DraggedItemIndexes.Contains(_LastSelectedIndexByDragDrop))
			//{
			//	this.DeselectItemByIndex(_LastSelectedIndexByDragDrop);
			//}
		}
		#endregion

		#region Initializer
		public ShellTreeViewEx() {
			InitTreeView();

			this.Controls.Add(ShellTreeView);

			InitializeComponent();

			InitRootItems();
		}
		#endregion
	}
}
