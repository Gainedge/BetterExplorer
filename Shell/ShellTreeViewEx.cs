using System;
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

namespace BExplorer.Shell {

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

		private TreeNode cuttedNode { get; set; }
		private ManualResetEvent _ResetEvent = new ManualResetEvent(true);
		private Int32 folderImageListIndex;
		private ShellView _ShellListView;
		private List<IntPtr> UpdatedImages = new List<IntPtr>();
		private List<IntPtr> CheckedFroChilds = new List<IntPtr>();
		private SyncQueue<IntPtr> imagesQueue = new SyncQueue<IntPtr>(); //7000
		private SyncQueue<IntPtr> childsQueue = new SyncQueue<IntPtr>(); //7000
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
			var favoritesItem = (ShellItem)KnownFolders.Links;
			var favoritesRoot = new TreeNode((favoritesItem).DisplayName);
			favoritesRoot.Tag = KnownFolders.Links;
			favoritesRoot.ImageIndex = ((ShellItem)KnownFolders.Favorites).GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			favoritesRoot.SelectedImageIndex = favoritesRoot.ImageIndex;

			if (favoritesItem.Count() > 0)
				favoritesRoot.Nodes.Add(_EmptyItemString);

			var librariesItem = (ShellItem)KnownFolders.Libraries;
			var librariesRoot = new TreeNode(librariesItem.DisplayName);
			librariesRoot.Tag = KnownFolders.Libraries;
			librariesRoot.ImageIndex = librariesItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			librariesRoot.SelectedImageIndex = librariesRoot.ImageIndex;
			if (librariesItem.HasSubFolders)
				librariesRoot.Nodes.Add(_EmptyItemString);

			var computerItem = (ShellItem)KnownFolders.Computer;
			var computerRoot = new TreeNode(computerItem.DisplayName);
			computerRoot.Tag = KnownFolders.Computer;
			computerRoot.ImageIndex = computerItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			computerRoot.SelectedImageIndex = computerRoot.ImageIndex;
			if (computerItem.HasSubFolders)
				computerRoot.Nodes.Add(_EmptyItemString);

			var networkItem = (ShellItem)KnownFolders.Network;
			var networkRoot = new TreeNode(networkItem.DisplayName);
			networkRoot.Tag = networkItem;
			networkRoot.ImageIndex = networkItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
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
			}
			catch (Exception) {
			}
		}

		private void SelectItem(ShellItem item) {
			if (item.IsSearchFolder)
				return;

			var listNodes = this.ShellTreeView.Nodes.OfType<TreeNode>().ToList();
			listNodes.AddRange(this.ShellTreeView.Nodes.OfType<TreeNode>().SelectMany(s => s.Nodes.OfType<TreeNode>()).ToArray());
			var nodes = listNodes.ToArray();
			var separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
			var directories = item.ParsingName.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			var items = new List<ShellItem>();

			for (int i = 0; i < directories.Length; i++) {
				if (i == 0) {
					items.Add(ShellItem.ToShellParsingName(directories[i]));// new ShellItem(directories[i].ToShellParsingName()));
				}
				else {
					string path = String.Empty;
					for (int j = 0; j <= i; j++) {
						if (j == 0)
							path = directories[j];
						else
							path = String.Format("{0}{1}{2}", path, Path.DirectorySeparatorChar, directories[j]);
					}

					items.Add(ShellItem.ToShellParsingName(path));
				}
			}

			foreach (var sho in items) {
				//TODO: Test Change
				//Note: Should we use FirstOrDefault() NOT SingleOrDefault() ??

				var theNodes = nodes.Where(w => w.Tag is ShellItem && ((ShellItem)w.Tag).GetHashCode() == sho.GetHashCode()).ToList();
				var theNode = theNodes.FirstOrDefault();

				if (theNode == null) {
					return;
				}
				else if (items.Last() == sho) {
					if (this.ShellTreeView.SelectedNode != theNode) {
						this.ShellTreeView.SelectedNode = theNode;
						theNode.EnsureVisible();
					}
				}
				else {
					theNode.Expand();
					nodes = theNode.Nodes.OfType<TreeNode>().ToArray();
				}

				if (theNodes.Count() > 1)
					theNodes.Skip(1).ToList().ForEach(d => this.ShellTreeView.SelectedNode.Parent.Nodes.Remove(d));
			}

			foreach (var sho in items) {
				var theNodes = nodes.Where(wr => wr.Tag is ShellItem && ((ShellItem)wr.Tag).GetHashCode() == sho.GetHashCode()).ToArray();
				var theNode = theNodes.FirstOrDefault();
				if (theNode == null) {
					return;
				}
				else if (items.Last() == sho) {
					if (this.ShellTreeView.SelectedNode != theNode) {
						this.ShellTreeView.SelectedNode = theNode;
						theNode.EnsureVisible();
					}
				}
				else {
					theNode.Expand();
					nodes = theNode.Nodes.OfType<TreeNode>().ToArray();
				}
				if (theNodes.Count() > 1) {
					theNodes.Skip(1).ToList().ForEach(d => this.ShellTreeView.SelectedNode.Parent.Nodes.Remove(d));
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
			imagesThread = new Thread(new ThreadStart(LoadTreeImages)) { IsBackground = true };
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
			}
			else {
				e.UseDefaultCursors = true;
			}

			if (ShellView.IsDropDescriptionValid(this._DataObject)) {
				e.UseDefaultCursors = false;
				Cursor.Current = Cursors.Arrow;
			}
			else {
				e.UseDefaultCursors = true;
			}

			if (ShellView.IsShowingLayered(doo)) {
				e.UseDefaultCursors = false;
				Cursor.Current = Cursors.Arrow;
			}
			else {
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
				SelectItem(this.ShellListView.CurrentFolder);
		}

		[System.Diagnostics.DebuggerStepThrough]
		public void LoadTreeImages() {
			while (true) {
				this._ResetEvent.WaitOne();
				var handle = imagesQueue.Dequeue();
				TreeNode node = null;
				IntPtr treeHandle = IntPtr.Zero;
				var hash = -1;
				var pidl = IntPtr.Zero;
				var visible = false;

				this.Invoke((Action)(() => {
					if (this.ShellTreeView != null) {
						node = TreeNode.FromHandle(ShellTreeView, handle);
						treeHandle = ShellTreeView.Handle;
						if (node != null)
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
					}
				}));
				if (node != null) {
					var item = node.Tag as ShellItem;
					if (item != null) {
						ShellItem newItem = null;
						try {
							newItem = ShellItem.ToShellParsingName(item.ParsingName);
						}
						catch (Exception) {
							newItem = item;
						}
						if (node != null && newItem != null) {
							try {
								hash = newItem.GetHashCode();
								pidl = newItem.AbsolutePidl;
								newItem.Dispose();
							}
							catch (Exception) {

							}
						}
					}
				}
				if (visible) {
					var nodeHandle = handle;
					//Thread.Sleep(1);
					//Application.DoEvents();

					SetNodeImage(nodeHandle, pidl, treeHandle, !(node.Parent != null && (node.Parent.Tag as ShellItem).ParsingName == KnownFolders.Links.ParsingName));
				}
			}
		}

		public void LoadChilds() {
			while (true) {
				this._ResetEvent.WaitOne();
				var handle = childsQueue.Dequeue();
				TreeNode node = null;
				IntPtr treeHandle = IntPtr.Zero;
				var visible = true;
				var pidl = IntPtr.Zero;

				//! Why do we have an invoke here -> Because we have TreeNode.FromHandle that accept the control inside thread
				this.Invoke((Action)(() => {
					node = TreeNode.FromHandle(ShellTreeView, handle);
					treeHandle = this.ShellTreeView.Handle;
					if (node != null) {
						visible = node.IsVisible;
						pidl = ((ShellItem)node.Tag).Pidl;
					}
				}));

				if (!visible || pidl == IntPtr.Zero)
					continue;

				if (node != null && node.Nodes.Count > 0) {
					var childItem = node.Nodes[0];
					if (childItem != null) {
						var nodeHandle = childItem.Handle;

						//TODO: Try to remove this Try Catch! It's slowing this down!!
						//TODO: Have the try catch only around the error causing code
						try {
							var sho = new ShellItem(pidl);
							if (!sho.HasSubFolders) {
								User32.SendMessage(treeHandle, BExplorer.Shell.Interop.MSG.TVM_DELETEITEM, 0, nodeHandle);
							}
							this.CheckedFroChilds.Add(handle);
							sho.Dispose();
						}
						catch (Exception) {
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

		public void DoMove(F.IDataObject dataObject, ShellItem destination) {
			var handle = this.Handle;
			var thread = new Thread(() => {
				var items = new IShellItem[0];
				if (dataObject.GetDataPresent("FileDrop"))
					items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => ShellItem.ToShellParsingName(s).ComInterface).ToArray();
				else
					items = dataObject.ToShellItemArray().ToArray();

				try {
					var fo = new IIFileOperation(handle);
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
				var items = new IShellItem[0];
				if (dataObject.GetDataPresent("FileDrop"))
					items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => ShellItem.ToShellParsingName(s).ComInterface).ToArray();
				else
					items = dataObject.ToShellItemArray().ToArray();

				try {
					var fo = new IIFileOperation(handle);
					foreach (var item in items) {
						fo.CopyItem(item, destination);
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
				var dropEffect = dataObject.ToDropEffect();
				var items = new IShellItem[0];
				if (dataObject.GetDataPresent("FileDrop"))
					items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => ShellItem.ToShellParsingName(s).ComInterface).ToArray();
				else
					items = dataObject.ToShellItemArray().ToArray();


				try {
					var fo = new IIFileOperation(handle);
					foreach (var item in items) {
						if (dropEffect == System.Windows.DragDropEffects.Copy)
							fo.CopyItem(item, selectedItem);
						else
							fo.MoveItem(item, selectedItem.ComInterface, null);
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
			var item = new TVITEMW() {
				mask = TVIF.TVIF_STATE,
				stateMask = TVIS.TVIS_CUT,
				state = TVIS.TVIS_CUT,
				hItem = this.ShellTreeView.SelectedNode.Handle,
			};

			User32.SendMessage(this.ShellTreeView.Handle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW, 0, ref item);

			this.cuttedNode = this.ShellTreeView.SelectedNode;
			var selectedItems = new[] { this.ShellTreeView.SelectedNode.Tag as ShellItem };
			var ddataObject = new F.DataObject();
			// Copy or Cut operation (5 = copy; 2 = cut)
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 2, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, selectedItems.CreateShellIDList());
			F.Clipboard.SetDataObject(ddataObject, true);
		}

		public void CopySelectedFiles() {
			var selectedItems = new[] { this.ShellTreeView.SelectedNode.Tag as ShellItem };
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
					var item = e.Node.Tag as ShellItem;
					if (!UpdatedImages.Contains(e.Node.Handle) && (item != null && item.Parent != null && item.Parent.ParsingName != KnownFolders.Network.ParsingName))
						imagesQueue.Enqueue(e.Node.Handle);
					if (!CheckedFroChilds.Contains(e.Node.Handle))
						childsQueue.Enqueue(e.Node.Handle);
				}
			}
			catch (Exception) {
				e.DrawDefault = true;
				//Do Nothing but prevent UI freeze
			}
		}

		private async void ShellTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
			if (e.Action == TreeViewAction.Collapse)
				this._AcceptSelection = false;
			if (e.Action == TreeViewAction.Expand) {
				this._ResetEvent.Reset();
				if (e.Node.Nodes.Count > 0 && e.Node.Nodes[0].Text == this._EmptyItemString) {
					e.Node.Nodes.Clear();
					imagesQueue.Clear();
					childsQueue.Clear();
					var sho = (ShellItem)e.Node.Tag;
					ShellItem lvSho = this.ShellListView != null && this.ShellListView.CurrentFolder != null ? this.ShellListView.CurrentFolder : null;
					var node = e.Node;
					node.Nodes.Add("Searching for folders...");
					var t = new Thread(() => {
						//var nodes = await Task.Run(() => {
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

							if ((sho.IsNetDrive || sho.IsNetworkPath) && sho.ParsingName != KnownFolders.Network.ParsingName) {
								itemNode.ImageIndex = this.folderImageListIndex;
							}
							else if (itemReal.IconType == IExtractIconPWFlags.GIL_PERCLASS || sho.ParsingName == KnownFolders.Network.ParsingName) {
								itemNode.ImageIndex = itemReal.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
								itemNode.SelectedImageIndex = itemNode.ImageIndex;
							}
							else {
								itemNode.ImageIndex = this.folderImageListIndex;
							}

							itemNode.Nodes.Add(this._EmptyItemString);
							if (item.ParsingName.EndsWith(".library-ms")) {
								var library = ShellLibrary.Load(item.DisplayName, false);
								if (library.IsPinnedToNavigationPane)
									nodesTemp.Add(itemNode);

								library.Close();
							}
							else {
								nodesTemp.Add(itemNode);
							}
							//Application.DoEvents();
						}
						//return nodesTemp;
						//});
						this.Invoke((Action)(() => {
							if (node.Nodes.Count == 1 && node.Nodes[0].Text == "Searching for folders...")
								node.Nodes.RemoveAt(0);
							node.Nodes.AddRange(nodesTemp.ToArray());
							if (lvSho != null)
								SelectItem(lvSho);
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
			var sho = e.Node != null ? e.Node.Tag as ShellItem : null;
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
				if (this._IsNavigate) {
					this.ShellListView.Navigate_Full(linkSho ?? sho, true, true);
				}

				this._IsNavigate = false;
				this.isFromTreeview = false;
			}
		}

		private void ShellListView_Navigated(object sender, NavigatedEventArgs e) {
			if (!this.isFromTreeview) {
				this.SelectItem(e.Folder);
			}
		}

		private void ShellTreeView_ItemDrag(object sender, ItemDragEventArgs e) {
			IntPtr dataObjPtr = IntPtr.Zero;
			var shellItem = ((e.Item as TreeNode).Tag as ShellItem);
			if (shellItem != null) {
				System.Runtime.InteropServices.ComTypes.IDataObject dataObject = shellItem.GetIDataObject(out dataObjPtr);

				uint ef = 0;
				Shell32.SHDoDragDrop(this.ShellListView.Handle, dataObject, null, unchecked((uint)F.DragDropEffects.All | (uint)F.DragDropEffects.Link), out ef);
			}
		}

		private void ShellTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
			if (e.Button == F.MouseButtons.Right) {
				if (e.Node.Tag != null)
					new ShellContextMenu(e.Node.Tag as ShellItem).ShowContextMenu(this, e.Location, CMF.CANRENAME);
			}
			else if (e.Button == F.MouseButtons.Left) {
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
			ShellItem destination = null;
			if (hittestInfo.Node == null)
				e.Effect = DragDropEffects.None;
			else
				destination = hittestInfo.Node.Tag as ShellItem;

			switch (e.Effect) {
				case F.DragDropEffects.Copy:
					this.DoCopy(e.Data, destination);
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
		[HandleProcessCorruptedStateExceptions]
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			if (m.Msg == ShellNotifications.WM_SHNOTIFY) {
				//MessageBox.Show("1");
				if (this._NotificationGlobal.NotificationReceipt(m.WParam, m.LParam)) {
					var computerNode = this.ShellTreeView.Nodes.OfType<TreeNode>().Single(s => s.Tag != null && (s.Tag as ShellItem).ParsingName == KnownFolders.Computer.ParsingName);
					foreach (NotifyInfos info in this._NotificationGlobal.NotificationsReceived.ToArray()) {
						switch (info.Notification) {
							case ShellNotifications.SHCNE.SHCNE_RENAMEITEM:
								break;
							case ShellNotifications.SHCNE.SHCNE_MKDIR:
								break;
							case ShellNotifications.SHCNE.SHCNE_RMDIR:
								break;
							case ShellNotifications.SHCNE.SHCNE_MEDIAINSERTED:
							case ShellNotifications.SHCNE.SHCNE_MEDIAREMOVED:
								var objDm = new ShellItem(info.Item1);
								var exisitingMItem = computerNode.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as ShellItem).ParsingName == objDm.ParsingName);
								if (exisitingMItem != null) {
									exisitingMItem.Text = objDm.DisplayName;
									exisitingMItem.ImageIndex = objDm.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
									exisitingMItem.SelectedImageIndex = exisitingMItem.ImageIndex;
								}
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEREMOVED:
								var objDr = new ShellItem(info.Item1);
								try {
									computerNode.Nodes.Remove(computerNode.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as ShellItem).ParsingName == objDr.ParsingName));
								}
								catch (NullReferenceException) {

								}
								objDr.Dispose();
								break;
							case ShellNotifications.SHCNE.SHCNE_DRIVEADD:
								var objDa = new ShellItem(info.Item1);
								var exisitingItem = computerNode.Nodes.OfType<TreeNode>().SingleOrDefault(s => s.Tag != null && (s.Tag as ShellItem).ParsingName == objDa.ParsingName);
								if (exisitingItem == null) {
									var newDrive = new TreeNode(objDa.DisplayName);
									newDrive.Tag = objDa;
									newDrive.ImageIndex = objDa.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
									newDrive.SelectedImageIndex = newDrive.ImageIndex;
									if (objDa.HasSubFolders)
										newDrive.Nodes.Add(_EmptyItemString);
									var nodesList = computerNode.Nodes.OfType<TreeNode>().Where(w => w.Tag != null).Select(s => s.Tag as ShellItem).ToList();
									nodesList.Add(objDa);
									nodesList = nodesList.OrderBy(o => o.ParsingName).ToList();
									var indexToInsert = nodesList.IndexOf(objDa);
									nodesList = null;
									GC.Collect();
									computerNode.Nodes.Insert(indexToInsert, newDrive);
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
							case ShellNotifications.SHCNE.SHCNE_CREATE:
								try {
									var sho = new ShellItem(info.Item1);
									var existingItem = this.ShellTreeView.Nodes.OfType<TreeNode>().Last().Nodes.OfType<TreeNode>().Where(w => (w.Tag as ShellItem) != null && (w.Tag as ShellItem).ParsingName == sho.ParsingName).Count();
									if (existingItem > 0)
										break;
									var node = new TreeNode(sho.DisplayName);
									node.ImageIndex = sho.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);// this.folderImageListIndex;
									node.SelectedImageIndex = node.ImageIndex;
									node.Tag = sho;
									if (sho != null && sho.Parent != null && sho.Parent.ParsingName == KnownFolders.Network.ParsingName) {
										this.ShellTreeView.Nodes.OfType<TreeNode>().Last().Nodes.Add(node);
									}
								}
								catch (AccessViolationException) {

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