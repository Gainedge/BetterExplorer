using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using BExplorer.Shell.Interop;
using System.Runtime.InteropServices;
using System.IO;
using System.Security;

namespace BExplorer.Shell
{
	public partial class ShellTreeViewEx : UserControl
	{
		public TreeViewBase ShellTreeView;
		private int folderImageListIndex;
		private ShellView _ShellListView;
		public ShellView ShellListView
		{
			get
			{
				return _ShellListView;
			}
			set
			{
				_ShellListView = value;
				_ShellListView.Navigated += ShellListView_Navigated; 
			}
		}
		private List<IntPtr> UpdatedImages = new List<IntPtr>();
		private List<IntPtr> CheckedFroChilds = new List<IntPtr>();
		private SyncQueue<IntPtr> imagesQueue = new SyncQueue<IntPtr>(7000);
		private SyncQueue<IntPtr> childsQueue = new SyncQueue<IntPtr>(7000);
		private Thread imagesThread;
		private Thread childsThread;
		private Boolean isFromTreeview;
		private void InitTreeView()
		{
			this.AllowDrop = true;
			this.ShellTreeView = new TreeViewBase();
			this.ShellTreeView.Dock = DockStyle.Fill;
			this.ShellTreeView.BackColor = Color.White;
			this.ShellTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ShellTreeView.AllowDrop = true;
			this.ShellTreeView.HideSelection = false;
			this.ShellTreeView.ShowLines = false;
			this.ShellTreeView.HotTracking = true;
			this.ShellTreeView.LabelEdit = true;
			this.ShellTreeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
			this.ShellTreeView.DrawNode += ShellTreeView_DrawNode;
			this.ShellTreeView.BeforeExpand += ShellTreeView_BeforeExpand;
			this.ShellTreeView.AfterExpand += ShellTreeView_AfterExpand;
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
			if (this.ShellListView != null)
			{
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

		public void CopySelectedFiles()
		{
			IntPtr dataObjPtr = IntPtr.Zero;

			var selectedItems = new ShellItem[1];
			selectedItems[0] = this.ShellTreeView.SelectedNode.Tag as ShellItem;
			//System.Runtime.InteropServices.ComTypes.IDataObject dataObject = GetIDataObject(this.SelectedItems.ToArray(), out dataObjPtr);
			System.Windows.Forms.DataObject ddataObject = new System.Windows.Forms.DataObject();
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 5, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, selectedItems.CreateShellIDList());
			System.Windows.Forms.Clipboard.SetDataObject(ddataObject, true);

		}

		private TreeNode cuttedNode { get; set; }
		public void CutSelectedFiles()
		{
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
			System.Windows.Forms.IDataObject ddataObject = new System.Windows.Forms.DataObject();
			// Copy or Cut operation (5 = copy; 2 = cut)
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 2, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, selectedItems.CreateShellIDList());
			System.Windows.Forms.Clipboard.SetDataObject(ddataObject, true);

		}
		public void PasteAvailableFiles()
		{
			var selectedItem = this.ShellTreeView.SelectedNode.Tag as ShellItem;
			if (selectedItem == null)
				return;

			var handle = this.Handle;
			var thread = new Thread(() =>
			{
				var dataObject = System.Windows.Forms.Clipboard.GetDataObject();
				var dragDropEffect = System.Windows.DragDropEffects.Copy;
				var dropEffect = dataObject.ToDropEffect();
				var shellItemArray = dataObject.ToShellItemArray();
				var items = shellItemArray.ToArray();
				try
				{
					IIFileOperation fo = new IIFileOperation(handle);
					foreach (var item in items)
					{
						if (dropEffect == System.Windows.DragDropEffects.Copy)
						{
							fo.CopyItem(item, selectedItem.m_ComInterface, String.Empty);
						}
						else
						{
							fo.MoveItem(item, selectedItem.m_ComInterface, null);
						}
					}

					fo.PerformOperations();
				}
				catch (SecurityException)
				{
					throw;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}
		public void DoCopy(System.Windows.Forms.IDataObject dataObject, ShellItem destination)
		{
			var handle = this.Handle;
			var thread = new Thread(() =>
			{
				var shellItemArray = dataObject.ToShellItemArray();
				var items = shellItemArray.ToArray();
				try
				{
					IIFileOperation fo = new IIFileOperation(handle);
					foreach (var item in items)
					{
						fo.CopyItem(item, destination.m_ComInterface, String.Empty);
					}

					fo.PerformOperations();
				}
				catch (SecurityException)
				{
					throw;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		public void DoMove(System.Windows.Forms.IDataObject dataObject, ShellItem destination)
		{
			var handle = this.Handle;
			var thread = new Thread(() =>
			{
				var shellItemArray = dataObject.ToShellItemArray();
				var items = shellItemArray.ToArray();
				try
				{
					IIFileOperation fo = new IIFileOperation(handle);
					foreach (var item in items)
					{
						fo.MoveItem(item, destination.m_ComInterface, null);
					}

					fo.PerformOperations();
				}
				catch (SecurityException)
				{
					throw;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();

		}
		void ShellTreeView_DragDrop(object sender, DragEventArgs e)
		{
			var hittestInfo =  this.ShellTreeView.HitTest(PointToClient(new System.Drawing.Point(e.X, e.Y)));
			ShellItem destination = null;
			if (hittestInfo.Node != null)
			{
				destination = hittestInfo.Node.Tag as ShellItem;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
			switch (e.Effect)
			{
				case System.Windows.Forms.DragDropEffects.All:
					break;
				case System.Windows.Forms.DragDropEffects.Copy:
					this.DoCopy(e.Data, destination);
					break;
				case System.Windows.Forms.DragDropEffects.Link:
					System.Windows.MessageBox.Show("link");
					break;
				case System.Windows.Forms.DragDropEffects.Move:
					this.DoMove(e.Data, destination);
					break;
				case System.Windows.Forms.DragDropEffects.None:
					break;
				case System.Windows.Forms.DragDropEffects.Scroll:
					break;
				default:
					break;
			}

			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			Win32Point wp = new Win32Point();
			wp.x = e.X;
			wp.y = e.Y;

			if (e.Data.GetDataPresent("DragImageBits"))
				dropHelper.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);

			//if (_LastSelectedIndexByDragDrop != -1 & !DraggedItemIndexes.Contains(_LastSelectedIndexByDragDrop))
			//{
			//	this.DeselectItemByIndex(_LastSelectedIndexByDragDrop);
			//}
		}

		void ShellTreeView_DragLeave(object sender, EventArgs e)
		{
			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			dropHelper.DragLeave();
		}

		void ShellTreeView_DragOver(object sender, DragEventArgs e)
		{
			if ((e.KeyState & (8 + 32)) == (8 + 32) &&
				(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Link) == System.Windows.Forms.DragDropEffects.Link)
			{
				// KeyState 8 + 32 = CTL + ALT 

				// Link drag-and-drop effect.
				e.Effect = System.Windows.Forms.DragDropEffects.Link;

			}
			else if ((e.KeyState & 32) == 32 &&
				(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Link) == System.Windows.Forms.DragDropEffects.Link)
			{

				// ALT KeyState for link.
				e.Effect = System.Windows.Forms.DragDropEffects.Link;

			}
			else if ((e.KeyState & 4) == 4 &&
				(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Move) == System.Windows.Forms.DragDropEffects.Move)
			{

				// SHIFT KeyState for move.
				e.Effect = System.Windows.Forms.DragDropEffects.Move;

			}
			else if ((e.KeyState & 8) == 8 &&
				(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Copy) == System.Windows.Forms.DragDropEffects.Copy)
			{

				// CTL KeyState for copy.
				e.Effect = System.Windows.Forms.DragDropEffects.Copy;

			}
			else if ((e.AllowedEffect & System.Windows.Forms.DragDropEffects.Move) == System.Windows.Forms.DragDropEffects.Move)
			{

				// By default, the drop action should be move, if allowed.
				e.Effect = System.Windows.Forms.DragDropEffects.Move;

			}
			else
				e.Effect = System.Windows.Forms.DragDropEffects.None;

			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			Win32Point wp = new Win32Point();
			wp.x = e.X;
			wp.y = e.Y;

			//int row = -1;
			//int collumn = -1;
			//this.HitTest(PointToClient(new System.Drawing.Point(e.X, e.Y)), out row, out collumn);

			//if (_LastSelectedIndexByDragDrop != -1 && !DraggedItemIndexes.Contains(_LastSelectedIndexByDragDrop))
			//{
			//	this.DeselectItemByIndex(_LastSelectedIndexByDragDrop);
			//}

			//if (row != -1)
			//{
			//	this.SelectItemByIndex(row);
			//}
			//else
			//{
			//	if (_LastSelectedIndexByDragDrop != -1 & !DraggedItemIndexes.Contains(_LastSelectedIndexByDragDrop))
			//	{
			//		this.DeselectItemByIndex(_LastSelectedIndexByDragDrop);
			//	}
			//}
			//_LastSelectedIndexByDragDrop = row;

			if (e.Data.GetDataPresent("DragImageBits"))
				dropHelper.DragOver(ref wp, (int)e.Effect);
		}

		void ShellTreeView_DragEnter(object sender, DragEventArgs e)
		{
			if ((e.KeyState & (8 + 32)) == (8 + 32) &&
						(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Link) == System.Windows.Forms.DragDropEffects.Link)
			{
				// KeyState 8 + 32 = CTL + ALT 

				// Link drag-and-drop effect.
				e.Effect = System.Windows.Forms.DragDropEffects.Link;

			}
			else if ((e.KeyState & 32) == 32 &&
				(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Link) == System.Windows.Forms.DragDropEffects.Link)
			{

				// ALT KeyState for link.
				e.Effect = System.Windows.Forms.DragDropEffects.Link;

			}
			else if ((e.KeyState & 4) == 4 &&
				(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Move) == System.Windows.Forms.DragDropEffects.Move)
			{

				// SHIFT KeyState for move.
				e.Effect = System.Windows.Forms.DragDropEffects.Move;

			}
			else if ((e.KeyState & 8) == 8 &&
				(e.AllowedEffect & System.Windows.Forms.DragDropEffects.Copy) == System.Windows.Forms.DragDropEffects.Copy)
			{

				// CTL KeyState for copy.
				e.Effect = System.Windows.Forms.DragDropEffects.Copy;

			}
			else if ((e.AllowedEffect & System.Windows.Forms.DragDropEffects.Move) == System.Windows.Forms.DragDropEffects.Move)
			{

				// By default, the drop action should be move, if allowed.
				e.Effect = System.Windows.Forms.DragDropEffects.Move;

			}
			else
				e.Effect = System.Windows.Forms.DragDropEffects.None;

			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			Win32Point wp = new Win32Point();
			wp.x = e.X;
			wp.y = e.Y;

			if (e.Data.GetDataPresent("DragImageBits"))
				dropHelper.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
		}

		void ShellTreeView_KeyDown(object sender, KeyEventArgs e)
		{
			if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
			{
				switch (e.KeyCode)
				{
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
			if (e.KeyCode == Keys.F2)
			{
				this.RenameSelectedNode();
			}
			if (e.KeyCode == Keys.Escape)
			{
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

		void ShellTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Label != null)
			{
				if (e.Label.Length > 0)
				{
					if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1)
					{
						// Stop editing without canceling the label change.
						e.Node.EndEdit(false);
						IIFileOperation fo = new IIFileOperation(this.Handle);
						fo.RenameItem((e.Node.Tag as ShellItem).m_ComInterface, e.Label);
						fo.PerformOperations();
					}
					else
					{
						/* Cancel the label edit action, inform the user, and 
							 place the node in edit mode again. */
						e.CancelEdit = true;
						MessageBox.Show("Invalid tree node label.\n" +
							 "The invalid characters are: '@','.', ',', '!'",
							 "Node Label Edit");
						e.Node.BeginEdit();
					}
				}
				else
				{
					/* Cancel the label edit action, inform the user, and 
						 place the node in edit mode again. */
					e.CancelEdit = true;
					MessageBox.Show("Invalid tree node label.\nThe label cannot be blank",
						 "Node Label Edit");
					e.Node.BeginEdit();
				}
			}
		}

		void ShellTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				//this.ShellTreeView.SelectedNode = e.Node;
				ShellContextMenu cm = new ShellContextMenu(e.Node.Tag as ShellItem);
				cm.ShowContextMenu(this, e.Location, CMF.CANRENAME);
			}
		}

		void ShellTreeView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			IntPtr dataObjPtr = IntPtr.Zero;
			var shellItem = ((e.Item as TreeNode).Tag as ShellItem);
			if (shellItem != null)
			{
				System.Runtime.InteropServices.ComTypes.IDataObject dataObject = shellItem.GetIDataObject(out dataObjPtr);

				uint ef = 0;
				Shell32.SHDoDragDrop(this.ShellListView.Handle, dataObject, null, unchecked((uint)System.Windows.Forms.DragDropEffects.All | (uint)System.Windows.Forms.DragDropEffects.Link), out ef);
			}
		}

		public void RenameSelectedNode()
		{
			var node = this.ShellTreeView.SelectedNode;
			if (node != null && !node.IsEditing)
			{
				node.BeginEdit();
			}
		}

		public void RenameNode(TreeNode node)
		{
			if (node != null && !node.IsEditing)
			{
				node.BeginEdit();
			}
		}
		void SelectItem(ShellItem item)
		{
			var nodes = this.ShellTreeView.Nodes.OfType<TreeNode>().SelectMany(s => s.Nodes.OfType<TreeNode>()).ToArray();
			var separators = new char[] {
				Path.DirectorySeparatorChar,  
				Path.AltDirectorySeparatorChar  
			};
			var directories = item.ParsingName.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			List<ShellItem> items = new List<ShellItem>();

			for (int i = 0; i < directories.Length; i++)
			{
				if (i == 0)
				{
					items.Add(new ShellItem(directories[i].ToShellParsingName()));
				}
				else
				{
					string path = String.Empty;
					for (int j = 0; j <= i; j++)
					{
						if (j == 0)
						{
							path = directories[j];
						}
						else
						{
							path = String.Format("{0}{1}{2}", path, Path.DirectorySeparatorChar, directories[j]);
						}
					}
					var shellItem = new ShellItem(path.ToShellParsingName());
					items.Add(shellItem);
				}
			}

			foreach (var sho in items)
			{
				var theNode = nodes.Where(wr => wr.Tag != null).Where(w => (w.Tag as ShellItem) != null && (w.Tag as ShellItem).ParsingName.ToLowerInvariant() == sho.ParsingName.ToLowerInvariant()).SingleOrDefault();
				if (theNode != null)
				{
					if (items.Last() == sho)
					{
						this.ShellTreeView.SelectedNode = theNode;
						theNode.EnsureVisible();
					}
					else
					{
						theNode.Expand();
						nodes = theNode.Nodes.OfType<TreeNode>().ToArray();
					}
				}
				else
				{
					return;
				}
			}
		}
		void ShellListView_Navigated(object sender, NavigatedEventArgs e)
		{
			if (!this.isFromTreeview)
			{
				this.SelectItem(e.Folder);
			}
		}

		void ShellTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var sho = e.Node.Tag as ShellItem;
			if (sho != null)
			{
				this.isFromTreeview = true;
				this.ShellListView.Navigate(sho);
				e.Node.Expand();
				this.isFromTreeview = false;
			}
		}

		void ShellTreeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		public void LoadChilds()
		{
			while (true)
			{
				var handle = childsQueue.Dequeue();
				TreeNode node = null;
				IntPtr treeHandle = IntPtr.Zero;
				this.ShellTreeView.Invoke((Action)(() =>
				{
					node = TreeNode.FromHandle(ShellTreeView, handle);
					treeHandle = this.ShellTreeView.Handle;
				}));
				var pidl = (node.Tag as ShellItem).Pidl;
				
				var childItem = node.Nodes.OfType<TreeNode>().FirstOrDefault();
				if (childItem != null)
				{
					var nodeHandle = childItem.Handle;
					Thread.Sleep(1);
					Application.DoEvents();
					CheckForChildFolders(nodeHandle, pidl, treeHandle, handle);
				}
			}
		}
		public void LoadTreeImages()
		{
			while (true)
			{
				var handle = imagesQueue.Dequeue();
				TreeNode node = null;
				IntPtr treeHandle = IntPtr.Zero;
				this.ShellTreeView.Invoke((Action)(() =>
				{
					node = TreeNode.FromHandle(ShellTreeView, handle);
					treeHandle = this.ShellTreeView.Handle;
				}));
				var hash = (node.Tag as ShellItem).GetHashCode();
				var pidl = (node.Tag as ShellItem).Pidl;
				var nodeHandle = handle;
				Thread.Sleep(1);
				Application.DoEvents();

				SetNodeImage(nodeHandle, pidl, treeHandle, !(node.Parent != null && (node.Parent.Tag as ShellItem).ParsingName == KnownFolders.Links.ParsingName));
			}
		}
		void ShellTreeView_HandleDestroyed(object sender, EventArgs e)
		{
			if (imagesThread != null)
				imagesThread.Abort();
			if (childsThread != null)
				childsThread.Abort();
		}

		void ShellTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Action == TreeViewAction.Expand)
			{
				if (e.Node.Nodes.Count > 0 && e.Node.Nodes[0].Text == "<!EMPTY!>")
				{
					e.Node.Nodes.Clear();
					imagesQueue.Clear();
					childsQueue.Clear();
					var sho = e.Node.Tag as ShellItem;
					foreach (var item in sho.Where(w => (w.IsFolder || w.IsLink) && w.IsHidden == false))
					{
						var itemNode = new TreeNode(item.DisplayName);
						itemNode.Tag = item;
						itemNode.ImageIndex = this.folderImageListIndex;//item.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
						itemNode.Nodes.Add("<!EMPTY!>");
						e.Node.Nodes.Add(itemNode);
					}
				}
			}
		}

		void SetNodeImage(IntPtr node, IntPtr pidl, IntPtr m_TreeViewHandle, Boolean isOverlayed)
		{
			try
			{
				TVITEMW itemInfo = new TVITEMW();

				// We need to set the images for the item by sending a 
				// TVM_SETITEMW message, as we need to set the overlay images,
				// and the .Net TreeView API does not support overlays.
				itemInfo.mask = TVIF.TVIF_IMAGE | TVIF.TVIF_SELECTEDIMAGE |
												TVIF.TVIF_STATE;
				itemInfo.hItem = node;
				itemInfo.iImage = ShellItem.GetSystemImageListIndex(pidl,
							ShellIconType.SmallIcon, ShellIconFlags.OverlayIndex);
				if (isOverlayed)
				{
					itemInfo.state = (TVIS)(itemInfo.iImage >> 16);
					itemInfo.stateMask = TVIS.TVIS_OVERLAYMASK;
				}
				itemInfo.iSelectedImage = ShellItem.GetSystemImageListIndex(pidl,
						ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
				this.UpdatedImages.Add(node);
				User32.SendMessage(m_TreeViewHandle, BExplorer.Shell.Interop.MSG.TVM_SETITEMW,
						0, ref itemInfo);
			}
			catch (Exception)
			{

			}
		}

		void CheckForChildFolders(IntPtr node, IntPtr pidl, IntPtr m_TreeViewHandle, IntPtr parentHandle)
		{
			try
			{
				var sho = new ShellItem(pidl);
				if (!sho.HasSubFolders)
				{
					User32.SendMessage(m_TreeViewHandle, BExplorer.Shell.Interop.MSG.TVM_DELETEITEM, 0, node);
				}
				this.CheckedFroChilds.Add(parentHandle);
				sho.Dispose();
			}
			catch (Exception)
			{

			}
		}
		private void InitRootItems()
		{
			var favoritesItem = (ShellItem)KnownFolders.Links;
			TreeNode favoritesRoot = new TreeNode((favoritesItem).DisplayName);
			favoritesRoot.Tag = KnownFolders.Links;
			favoritesRoot.ImageIndex = ((ShellItem)KnownFolders.Favorites).GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			if (favoritesItem.Count() > 0)
				favoritesRoot.Nodes.Add("<!EMPTY!>");

			var librariesItem = (ShellItem)KnownFolders.Libraries;
			TreeNode librariesRoot = new TreeNode(librariesItem.DisplayName);
			librariesRoot.Tag = KnownFolders.Libraries;
			librariesRoot.ImageIndex = librariesItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			if (librariesItem.HasSubFolders)
				librariesRoot.Nodes.Add("<!EMPTY!>");
			

			var computerItem = (ShellItem)KnownFolders.Computer;
			TreeNode computerRoot = new TreeNode(computerItem.DisplayName);
			computerRoot.Tag = KnownFolders.Computer;
			computerRoot.ImageIndex = computerItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);
			if (computerItem.HasSubFolders)
				computerRoot.Nodes.Add("<!EMPTY!>");
			
			var networkItem = (ShellItem)KnownFolders.Network;
			var networkRoot = new TreeNode(networkItem.DisplayName);
			networkRoot.Tag = networkItem;
			networkRoot.ImageIndex = networkItem.GetSystemImageListIndex(ShellIconType.SmallIcon, ShellIconFlags.OpenIcon);

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
		public ShellTreeViewEx()
		{
			InitTreeView();

			this.Controls.Add(ShellTreeView);

			InitializeComponent();

			InitRootItems();
		}
		public void RefreshContents()
		{
			this.ShellTreeView.Nodes.Clear();
			InitRootItems();
		}
		void ShellTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = !String.IsNullOrEmpty(e.Node.Text);
			if (e.Node.Tag != null)
			{
				if (!UpdatedImages.Contains(e.Node.Handle))
					imagesQueue.Enqueue(e.Node.Handle);
				if (!CheckedFroChilds.Contains(e.Node.Handle))
					childsQueue.Enqueue(e.Node.Handle);
			}
		}
	}
}
