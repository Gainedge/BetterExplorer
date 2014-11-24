using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using SQLite = System.Data.SQLite;
using F = System.Windows.Forms;
using BExplorer.Shell.Interop;
using System.Xml.Linq;
using DPoint = System.Drawing.Point;
using Input = System.Windows.Input;
using System.Runtime.ExceptionServices;
using BExplorer.Shell.DropTargetHelper;

namespace BExplorer.Shell {

	#region Substructures and classes

	/// <summary> Specifies how list items are displayed in a <see cref="ShellView" /> control. </summary>
	public enum ShellViewStyle {

		/// <summary> Items appear in a grid and icon size is 256x256 </summary>
		ExtraLargeIcon,

		/// <summary> Items appear in a grid and icon size is 96x96 </summary>
		LargeIcon,

		/// <summary> Each item appears as a full-sized icon with a label below it. </summary>
		Medium,

		/// <summary> Each item appears as a small icon with a label to its right. </summary>
		SmallIcon,

		/// <summary>
		/// Each item appears as a small icon with a label to its right. Items are arranged in columns.
		/// </summary>
		List,

		/// <summary>
		/// Each item appears on a separate line with further information about each item arranged
		/// in columns. The left-most column contains a small icon and label.
		/// </summary>
		Details,

		/// <summary> Each item appears with a thumbnail picture of the file's content. </summary>
		Thumbnail,

		/// <summary>
		/// Each item appears as a full-sized icon with the item label and file information to the
		/// right of it.
		/// </summary>
		Tile,

		/// <summary>
		/// Each item appears in a thumbstrip at the bottom of the control, with a large preview of
		/// the selected item appearing above.
		/// </summary>
		Thumbstrip,

		/// <summary> Each item appears in a item that occupy the whole view width </summary>
		Content,
	}

	public class RenameEventArgs : EventArgs {

		public int ItemIndex { get; private set; }

		public RenameEventArgs(int itemIndex) {
			this.ItemIndex = itemIndex;
		}
	}

	public class NavigatedEventArgs : EventArgs, IDisposable {
		/// <summary> The folder that is navigated to. </summary>
		public ShellItem Folder { get; set; }
		public ShellItem OldFolder { get; set; }

		public Boolean isInSameTab { get; set; }

		public void Dispose() {
			if (Folder != null) {
				Folder.Dispose();
				Folder = null;
			}
			if (OldFolder != null) {
				OldFolder.Dispose();
				OldFolder = null;
			}
		}

		public NavigatedEventArgs(ShellItem folder, ShellItem old) {
			Folder = folder;
			OldFolder = old;
		}
		public NavigatedEventArgs(ShellItem folder, ShellItem old, bool isInSame) {
			Folder = folder;
			OldFolder = old;
			isInSameTab = isInSame;
		}
	}

	/// <summary> Provides information for the <see cref="ShellView.Navigating" /> event. </summary>
	public class NavigatingEventArgs : EventArgs, IDisposable {
		/// <summary> The folder being navigated to. </summary>
		public ShellItem Folder { get; private set; }

		/// <summary> Gets/sets a value indicating whether the navigation should be canceled. </summary>
		[Obsolete("Never used")]
		public bool Cancel { get; private set; }

		public Boolean IsNavigateInSameTab { get; private set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="NavigatingEventArgs"/> class.
		/// </summary>
		/// <param name="folder">The folder being navigated to.</param>
		/// <param name="isInSameTab"></param>
		public NavigatingEventArgs(ShellItem folder, bool isInSameTab) {
			Folder = folder;
			IsNavigateInSameTab = isInSameTab;
		}

		public void Dispose() {
			if (Folder != null) {
				Folder.Dispose();
				Folder = null;
			}
		}
	}

	/*
	public class ItemDisplayedEventArgs : EventArgs, IDisposable {

		public ShellItem DisplayedItem { get; private set; }

		public int DisplayedItemIndex { get; private set; }

		public ItemDisplayedEventArgs(ShellItem item, int index) {
			this.DisplayedItem = item;
			this.DisplayedItemIndex = index;
		}

		public void Dispose() {
			if (DisplayedItem != null) {
				DisplayedItem.Dispose();
				DisplayedItem = null;
			}
		}
	}
	*/

	public enum ItemUpdateType {
		Renamed,
		Created,
		Deleted,
		Updated,
		DriveRemoved,
		RecycleBin
	}

	public class ItemUpdatedEventArgs : EventArgs {

		public ItemUpdateType UpdateType { get; private set; }

		public ShellItem PreviousItem { get; private set; }

		public ShellItem NewItem { get; private set; }

		public int NewItemIndex { get; private set; }

		public ItemUpdatedEventArgs(ItemUpdateType type, ShellItem newItem, ShellItem previousItem, int index) {
			this.UpdateType = type;
			this.NewItem = newItem;
			this.PreviousItem = previousItem;
			this.NewItemIndex = index;
		}
	}

	public class ViewChangedEventArgs : EventArgs {

		public Int32 ThumbnailSize { get; private set; }

		/// <summary> The current ViewStyle </summary>
		public ShellViewStyle CurrentView { get; private set; }

		public ViewChangedEventArgs(ShellViewStyle view, Int32? thumbnailSize) {
			CurrentView = view;
			if (thumbnailSize != null)
				ThumbnailSize = thumbnailSize.Value;
		}
	}

	#endregion Substructures and classes

	/// <summary> The ShellFileListView class that visualize contents of a directory </summary>
	public partial class ShellView : UserControl {

		#region Event Handler

		/*
		/// <summary> Occurs when the control gains focus </summary>
		public new event EventHandler GotFocus;
		*/

		//public event EventHandler<ItemDisplayedEventArgs> ItemDisplayed;

		public event EventHandler<NavigatingEventArgs> Navigating;

		/*
		/// <summary> Occurs when the control loses focus </summary>
		public new event EventHandler LostFocus;
		*/

		/// <summary> Occurs when the <see cref="ShellView" /> control navigates to a new folder. </summary>
		public event EventHandler<NavigatedEventArgs> Navigated;

		/// <summary>
		/// Occurs when the <see cref="ShellView"/>'s current selection
		/// changes.
		/// </summary>
		///
		/// <remarks>
		/// <b>Important:</b> When <see cref="ShowWebView"/> is set to
		/// <see langref="true"/>, this event will not occur. This is due to
		/// a limitation in the underlying windows control.
		/// </remarks>
		public event EventHandler SelectionChanged;

		public event EventHandler<ItemUpdatedEventArgs> ItemUpdated;

		public event EventHandler<ViewChangedEventArgs> ViewStyleChanged;

		public event EventHandler<NavigatedEventArgs> ItemMiddleClick;

		/// <summary>
		/// Occurs when the user right-clicks on the blank area of the column header area
		/// </summary>
		public event MouseEventHandler ColumnHeaderRightClick;

		/*
		/// <summary> Raised whenever a key is pressed </summary>
		public new event KeyEventHandler KeyDown;
		*/

		/// <summary>
		/// Raised whenever a key is pressed, with the intention of doing a key jump. Please use
		/// <see cref="KeyDown" /> to catch when any key is pressed.
		/// </summary>
		public event KeyEventHandler KeyJumpKeyDown;

		public event EventHandler<RenameEventArgs> BeginItemLabelEdit;

		/// <summary>Raised whenever file/folder name is finished editing. Boolean: is event canceled</summary>
		public event EventHandler<bool> EndItemLabelEdit;

		/// <summary> Raised when the timer finishes for the Key Jump timer. </summary>
		public event EventHandler KeyJumpTimerDone;

		/*
		private void OnViewChanged(ViewChangedEventArgs e) {
			if (ViewStyleChanged != null) {
				ViewStyleChanged(this, e);
			}
		}
		*/

		/*
		private void OnNavigated(NavigatedEventArgs e) {
			if (Navigated != null) {
				Navigated(this, e);
			}
		}
		*/

		/*
		private void OnItemDisplayed(ShellItem item, int index) {
			if (ItemDisplayed != null) {
				ItemDisplayed(this, new ItemDisplayedEventArgs(item, index));
			}
		}*/

		/*
		/// <summary> Triggers the Navigating event. </summary>
		[DebuggerStepThrough()]
		public virtual void OnNavigating(NavigatingEventArgs ea) {
			if (Navigating != null)
				Navigating(this, ea);
		}
		*/
		#endregion Event Handler

		#region Public Members

		public ToolTip ToolTip;
		public MessageHandler MessageHandlerWindow;
		public List<Collumns> AllAvailableColumns = new List<Collumns>();
		public List<Collumns> Collumns = new List<Collumns>();
		public List<ListViewGroupEx> Groups = new List<ListViewGroupEx>();
		public bool IsRenameNeeded { get; set; }
		public Boolean IsGroupsEnabled { get { return LastGroupCollumn != null; } }

		/// <summary> Returns the key jump string as it currently is.</summary>
		public string KeyJumpString { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the folder currently being browsed by the <see
		/// cref="ShellView" /> has parent folder which can be navigated to by calling <see
		/// cref="NavigateParent" />.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateParent { get { return CurrentFolder != ShellItem.Desktop; } }

		/// <summary>
		/// Gets/sets a <see cref="ShellItem" /> describing the folder currently being browsed by
		/// the <see cref="ShellView" />.
		/// </summary>
		[Browsable(false)]
		public ShellItem CurrentFolder { get; private set; }

		public int IconSize { get; private set; }

		public List<ShellItem> Items { get; private set; }

		public Dictionary<ShellItem, int> ItemsHashed { get; set; }

		public int LastSortedColumnIndex { get; private set; }

		public SortOrder LastSortOrder { get; private set; }

		public Collumns LastGroupCollumn { get; private set; }

		public SortOrder LastGroupOrder { get; private set; }

		public IntPtr LVHandle { get; private set; }

		public List<LVItemColor> LVItemsColorCodes { get; set; }

		public List<ShellItem> SelectedItems {
			get {
				var Data = this.SelectedIndexes.ToArray();
				var selItems = new List<ShellItem>();
				DraggedItemIndexes.AddRange(Data);

				foreach (var index in Data) {
					var Item = this.Items.ElementAtOrDefault(index);
					if (Item == null)
						this.SelectedIndexes.Remove(index);
					else
						selItems.Add(Item);
				}
				return selItems;
			}
		}
		public Boolean ShowCheckboxes {
			get { return _showCheckBoxes; }
			set {
				if (value) {
					User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.CheckBoxes, (int)ListViewExtendedStyles.CheckBoxes);
					User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT);
				} else {
					User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.CheckBoxes, 0);
					User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, 0);
				}
				_showCheckBoxes = value;
			}
		}

		public Boolean ShowHidden {
			get { return _ShowHidden; }
			set {
				_ShowHidden = value;
				this.RefreshContents();
			}
		}

		/// <summary> Gets or sets how items are displayed in the control. </summary>
		[DefaultValue(ShellViewStyle.Medium), Category("Appearance")]
		public ShellViewStyle View {
			get { return m_View; }
			set {
				m_View = value;
				var iconsize = 16;
				switch (value) {
					case ShellViewStyle.ExtraLargeIcon:
						User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(256);
						iconsize = 256;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.LargeIcon:
						User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(96);
						iconsize = 96;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.Medium:
						User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(48);
						iconsize = 48;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.SmallIcon:
						User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_SMALLICON, 0);
						ResizeIcons(16);
						iconsize = 16;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.List:
						User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_LIST, 0);
						ResizeIcons(16);
						iconsize = 16;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.Details:
						this.UpdateColsInView(true);
						User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_DETAILS, 0);
						ResizeIcons(16);
						iconsize = 16;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.Thumbnail:
						break;

					case ShellViewStyle.Tile:
						User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_TILE, 0);
						ResizeIcons(48);
						iconsize = 48;
						RefreshItemsCountInternal();
						//LVTILEVIEWINFO tvi = new LVTILEVIEWINFO();
						//tvi.cbSize = Marshal.SizeOf(typeof(LVTILEVIEWINFO));
						//tvi.dwMask = (int)LVTVIM.LVTVIM_COLUMNS | (int)LVTVIM.LVTVIM_TILESIZE;
						//tvi.dwFlags = (int)LVTVIF.LVTVIF_AUTOSIZE;
						//tvi.cLines = 4;
						//var a = User32.SendMessage(this.LVHandle, (int)MSG.LVM_SETTILEVIEWINFO, 0, tvi);
						break;

					case ShellViewStyle.Thumbstrip:
						break;

					case ShellViewStyle.Content:
						break;

					default:
						break;
				}

				if (value != ShellViewStyle.Details) {
					this.UpdateColsInView();
					AutosizeAllColumns(-2);
				}
				//OnViewChanged(new ViewChangedEventArgs(value, iconsize));

				if (ViewStyleChanged != null) {
					ViewStyleChanged(this, new ViewChangedEventArgs(value, iconsize));
				}
			}
		}

		public int CurrentRefreshedItemIndex = -1;
		public FileSystemWatcher fsw = new FileSystemWatcher();
		#endregion Public Members

		#region Private Members

		private List<int> SelectedIndexes {
			get {
				List<int> selItems = new List<int>();
				int iStart = -1;
				LVITEMINDEX lvi = new LVITEMINDEX();
				while (lvi.iItem != -1) {
					lvi.iItem = iStart;
					lvi.iGroup = this.GetGroupIndex(iStart);
					User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
					iStart = lvi.iItem;

					//TODO: Find out if we even need this IF Then
					if (lvi.iItem != -1) selItems.Add(lvi.iItem);
				}

				return selItems;
			}
		}
		private bool ItemForRealName_IsAny { get { return ItemForRename != -1; } }
		private int ItemForRename { get; set; }
		private System.Runtime.InteropServices.ComTypes.IDataObject dataObject { get; set; }
		private Boolean _showCheckBoxes = false;
		private Boolean _ShowHidden;
		private F.Timer _ResetTimer = new F.Timer();
		private List<Int32> DraggedItemIndexes = new List<int>();
		private F.Timer _KeyJumpTimer = new F.Timer();
		private ShellItem _kpreselitem = null;
		private LVIS _IsDragSelect = 0;
		private BackgroundWorker bw = new BackgroundWorker();
		private Thread _IconCacheLoadingThread;
		private Bitmap ExeFallBack16;
		private Bitmap ExeFallBack256;
		private Bitmap ExeFallBack32;
		private Bitmap ExeFallBack48;
		private int ShieldIconIndex;
		private int _SharedIconIndex;
		private ImageList extra = new ImageList(ImageListSize.ExtraLarge);
		private ImageList jumbo = new ImageList(ImageListSize.Jumbo);
		private ImageList large = new ImageList(ImageListSize.Large);
		private ShellViewStyle m_View;
		private SyncQueue<int> overlayQueue = new SyncQueue<int>(); //3000
		private Thread _OverlaysLoadingThread;
		private F.Timer selectionTimer = new F.Timer();
		private SyncQueue<int> shieldQueue = new SyncQueue<int>(); //3000
		private ImageList small = new ImageList(ImageListSize.SystemSmall);
		private Thread _IconLoadingThread;
		private Thread _UpdateSubitemValuesThread;
		private SyncQueue<int> ThumbnailsForCacheLoad = new SyncQueue<int>(); //5000
		private SyncQueue<Tuple<int, int, PROPERTYKEY>> ItemsForSubitemsUpdate = new SyncQueue<Tuple<int, int, PROPERTYKEY>>(); //5000
		private ConcurrentBag<Tuple<int, PROPERTYKEY, object>> SubItemValues = new ConcurrentBag<Tuple<int, PROPERTYKEY, object>>();
		private ManualResetEvent resetEvent = new ManualResetEvent(true);
		private SyncQueue<int> waitingThumbnails = new SyncQueue<int>(); //3000
		private List<int> _CuttedIndexes = new List<int>();
		private int _LastDropHighLightedItemIndex = -1;

		#endregion Private Members

		#region Initializer

		/// <summary> Main constructor </summary>
		public ShellView() {
			//this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ContainerControl | ControlStyles.CacheText , true);
			this.ItemForRename = -1;
			InitializeComponent();
			this.Items = new List<ShellItem>();
			this.LVItemsColorCodes = new List<LVItemColor>();
			this.AllAvailableColumns = this.AvailableColumns();
			this.AllowDrop = true;
			_IconLoadingThread = new Thread(_IconsLoadingThreadRun) { IsBackground = true, Priority = ThreadPriority.AboveNormal };
			_IconLoadingThread.Start();
			_IconCacheLoadingThread = new Thread(_IconCacheLoadingThreadRun) { IsBackground = true, Priority = ThreadPriority.BelowNormal };
			_IconCacheLoadingThread.Start();
			_OverlaysLoadingThread = new Thread(_OverlaysLoadingThreadRun) { IsBackground = true, Priority = ThreadPriority.Normal };
			_OverlaysLoadingThread.Start();
			_UpdateSubitemValuesThread = new Thread(_UpdateSubitemValuesThreadRun) { Priority = ThreadPriority.BelowNormal };
			_UpdateSubitemValuesThread.Start();
			_ResetTimer.Interval = 500;
			_ResetTimer.Tick += resetTimer_Tick;

			var defIconInfo = new Shell32.SHSTOCKICONINFO() { cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO)) };

			Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_APPLICATION, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
			ExeFallBack48 = extra.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			ExeFallBack256 = jumbo.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			ExeFallBack16 = small.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();

			Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_SHIELD, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
			ShieldIconIndex = defIconInfo.iSysIconIndex;

			Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_SHARE, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
			_SharedIconIndex = defIconInfo.iSysIconIndex;

			//this.KeyDown += ShellView_KeyDown;
			this.MouseUp += ShellView_MouseUp;
			//this.GotFocus += ShellView_GotFocus;
			selectionTimer.Interval = 500;
			selectionTimer.Tick += selectionTimer_Tick;

		}

		void fsw_Changed(object sender, FileSystemEventArgs e) {
			try {
        if (e.ChangeType == WatcherChangeTypes.Renamed) {
          this.IsRenameInProgress = false;
        }
				var objUpdate = new ShellItem(e.FullPath);
				var exisitingItem = this.Items.Where(w => w.Equals(objUpdate)).SingleOrDefault();
				if (exisitingItem != null) {
					this.RefreshItem(this.Items.IndexOf(exisitingItem), true);
				}
				if (objUpdate != null && this.CurrentFolder != null && objUpdate.ParsingName == this.CurrentFolder.ParsingName) {
					this.UnvalidateDirectory();
				}
				objUpdate.Dispose();
			} catch (FileNotFoundException) {

			}
		}

		void fsw_Deleted(object sender, FileSystemEventArgs e) {

			if (!String.IsNullOrEmpty(e.FullPath)) {
				ShellItem theItem = this.Items.ToArray().SingleOrDefault(s => s.ParsingName.ToLowerInvariant() == e.FullPath.ToLowerInvariant());
				if (theItem != null) {
					this.Items.Remove(theItem);
					if (this.IsGroupsEnabled) this.SetGroupOrder(false);
					this.SetSortCollumn(this.LastSortedColumnIndex, this.LastSortOrder, false);
				}
			}
		}

		void fsw_Created(object sender, FileSystemEventArgs e) {
			if (!File.Exists(e.FullPath) && !Directory.Exists(e.FullPath))
				return;
			try {
				var obj = new ShellItem(e.FullPath);
				var existingItem = this.Items.SingleOrDefault(s => s.Equals(obj));
				if (existingItem == null && (obj.Parent != null && obj.Parent.Equals(this.CurrentFolder))) {
					if (obj.Extension.ToLowerInvariant() != ".tmp") {
						var itemIndex = this.InsertNewItem(obj);
						// 					this.Invoke(new MethodInvoker(() => {
						this.RaiseItemUpdated(ItemUpdateType.Created, null, obj, itemIndex);
						// 					}));
					} else {
						var affectedItem = this.Items.SingleOrDefault(s => s.Equals(obj.Parent));
						if (affectedItem != null) {
							var index = this.Items.IndexOf(affectedItem);
							this.RefreshItem(index, true);
						}
					}
				}
			} catch (Exception) {

			}
		}

		#endregion Initializer

		#region Events

		private void selectionTimer_Tick(object sender, EventArgs e) {
			if (MouseButtons != F.MouseButtons.Left) {
				(sender as F.Timer).Stop();
				OnSelectionChanged();
				if (KeyJumpTimerDone != null) {
					KeyJumpTimerDone(this, EventArgs.Empty);
				}

			}
			if (this.ItemForRename != this.GetFirstSelectedItemIndex() && !this.IsRenameInProgress) {
				(sender as F.Timer).Stop();
				this.EndLabelEdit();
			}
		}

		private void _KeyJumpTimer_Tick(object sender, EventArgs e) {
			if (KeyJumpTimerDone != null) {
				KeyJumpTimerDone(this, EventArgs.Empty);
			}

			_KeyJumpTimer.Enabled = false;

			//process key jump
			DeSelectAllItems();
			int startindex = 0;
			if (_kpreselitem != null) {
				if (_kpreselitem.GetDisplayName(SIGDN.NORMALDISPLAY).ToUpperInvariant().StartsWith(KeyJumpString.ToUpperInvariant())) {
					startindex = Items.IndexOf(_kpreselitem) + 1;
				}
			}

			int selind = GetFirstIndexOf(KeyJumpString, startindex);
			if (selind != -1) {
				SelectItemByIndex(selind, true);
			}

			KeyJumpString = "";
		}

		private void resetTimer_Tick(object sender, EventArgs e) {
			(sender as F.Timer).Stop();
			resetEvent.Set();
			//RedrawWindow();
			//GC.WaitForPendingFinalizers();
			//GC.Collect();
			Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
		}

		private void ShellView_MouseUp(object sender, MouseEventArgs e) {
			if (_IsDragSelect == LVIS.LVIS_SELECTED) {
				if (selectionTimer.Enabled)
					selectionTimer.Stop();

				selectionTimer.Start();
			}
		}

		private void ShellView_GotFocus() {
			this.Focus();
			User32.SetForegroundWindow(this.LVHandle);
		}

		private void ShellView_KeyDown(Keys e) {
			if (ItemForRealName_IsAny) {
				if (e == Keys.Escape) {
					this.EndLabelEdit(true);
				}
				if (e == Keys.F2) {
					//TODO: implement a conditional selection inside rename textbox!
				}
				return;
			}
			if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
				switch (e) {
					case Keys.A:
						SelectAll();
						break;

					case Keys.Add:
						break;

					case Keys.Alt:
						break;

					case Keys.Apps:
						break;

					case Keys.Attn:
						break;

					case Keys.B:
						break;

					case Keys.Back:
						break;

					case Keys.BrowserBack:
						break;

					case Keys.BrowserFavorites:
						break;

					case Keys.BrowserForward:
						break;

					case Keys.BrowserHome:
						break;

					case Keys.BrowserRefresh:
						break;

					case Keys.BrowserSearch:
						break;

					case Keys.BrowserStop:
						break;

					case Keys.C:
						this.CopySelectedFiles();
						break;

					case Keys.Cancel:
						break;

					case Keys.Capital:
						break;

					case Keys.Clear:
						break;

					case Keys.Control:
						break;

					case Keys.ControlKey:
						break;

					case Keys.Crsel:
						break;

					case Keys.D:
						DeSelectAllItems();
						break;

					case Keys.D0:
						break;

					case Keys.D1:
						break;

					case Keys.D2:
						break;

					case Keys.D3:
						break;

					case Keys.D4:
						break;

					case Keys.D5:
						break;

					case Keys.D6:
						break;

					case Keys.D7:
						break;

					case Keys.D8:
						break;

					case Keys.D9:
						break;

					case Keys.Decimal:
						break;

					case Keys.Delete:
						break;

					case Keys.Divide:
						break;

					case Keys.Down:
						break;

					case Keys.E:
						break;

					case Keys.End:
						break;

					case Keys.Enter:
						break;

					case Keys.EraseEof:
						break;

					case Keys.Escape:
						break;

					case Keys.Execute:
						break;

					case Keys.Exsel:
						break;

					case Keys.F:
						break;

					case Keys.F1:
						break;

					case Keys.F10:
						break;

					case Keys.F11:
						break;

					case Keys.F12:
						break;

					case Keys.F13:
						break;

					case Keys.F14:
						break;

					case Keys.F15:
						break;

					case Keys.F16:
						break;

					case Keys.F17:
						break;

					case Keys.F18:
						break;

					case Keys.F19:
						break;

					case Keys.F2:
						break;

					case Keys.F20:
						break;

					case Keys.F21:
						break;

					case Keys.F22:
						break;

					case Keys.F23:
						break;

					case Keys.F24:
						break;

					case Keys.F3:
						break;

					case Keys.F4:
						break;

					case Keys.F5:
						break;

					case Keys.F6:
						break;

					case Keys.F7:
						break;

					case Keys.F8:
						break;

					case Keys.F9:
						break;

					case Keys.FinalMode:
						break;

					case Keys.G:
						break;

					case Keys.H:
						break;

					case Keys.HanguelMode:
						break;

					case Keys.HanjaMode:
						break;

					case Keys.Help:
						break;

					case Keys.Home:
						break;

					case Keys.I:
						InvertSelection();
						break;

					case Keys.IMEAccept:
						break;

					case Keys.IMEConvert:
						break;

					case Keys.IMEModeChange:
						break;

					case Keys.IMENonconvert:
						break;

					case Keys.Insert:
						break;

					case Keys.J:
						break;

					case Keys.JunjaMode:
						break;

					case Keys.K:
						break;

					case Keys.KeyCode:
						break;

					case Keys.L:
						break;

					case Keys.LButton:
						break;

					case Keys.LControlKey:
						break;

					case Keys.LMenu:
						break;

					case Keys.LShiftKey:
						break;

					case Keys.LWin:
						break;

					case Keys.LaunchApplication1:
						break;

					case Keys.LaunchApplication2:
						break;

					case Keys.LaunchMail:
						break;

					case Keys.Left:
						break;

					case Keys.LineFeed:
						break;

					case Keys.M:
						break;

					case Keys.MButton:
						break;

					case Keys.MediaNextTrack:
						break;

					case Keys.MediaPlayPause:
						break;

					case Keys.MediaPreviousTrack:
						break;

					case Keys.MediaStop:
						break;

					case Keys.Menu:
						break;

					case Keys.Modifiers:
						break;

					case Keys.Multiply:
						break;

					case Keys.N:
						break;

					case Keys.NoName:
						break;

					case Keys.None:
						break;

					case Keys.NumLock:
						break;

					case Keys.NumPad0:
						break;

					case Keys.NumPad1:
						break;

					case Keys.NumPad2:
						break;

					case Keys.NumPad3:
						break;

					case Keys.NumPad4:
						break;

					case Keys.NumPad5:
						break;

					case Keys.NumPad6:
						break;

					case Keys.NumPad7:
						break;

					case Keys.NumPad8:
						break;

					case Keys.NumPad9:
						break;

					case Keys.O:
						break;

					case Keys.Oem1:
						break;

					case Keys.Oem102:
						break;

					case Keys.Oem2:
						break;

					case Keys.Oem3:
						break;

					case Keys.Oem4:
						break;

					case Keys.Oem5:
						break;

					case Keys.Oem6:
						break;

					case Keys.Oem7:
						break;

					case Keys.Oem8:
						break;

					case Keys.OemClear:
						break;

					case Keys.OemMinus:
						break;

					case Keys.OemPeriod:
						break;

					case Keys.Oemcomma:
						break;

					case Keys.Oemplus:
						break;

					case Keys.P:
						break;

					case Keys.Pa1:
						break;

					case Keys.PageDown:
						break;

					case Keys.PageUp:
						break;

					case Keys.Pause:
						break;

					case Keys.Play:
						break;

					case Keys.Print:
						break;

					case Keys.PrintScreen:
						break;

					case Keys.ProcessKey:
						break;

					case Keys.Q:
						break;

					case Keys.R:
						break;

					case Keys.RButton:
						break;

					case Keys.RControlKey:
						break;

					case Keys.RMenu:
						break;

					case Keys.RShiftKey:
						break;

					case Keys.RWin:
						break;

					case Keys.Right:
						break;

					case Keys.S:
						break;

					case Keys.Scroll:
						break;

					case Keys.Select:
						break;

					case Keys.SelectMedia:
						break;

					case Keys.Separator:
						break;

					case Keys.Shift:
						break;

					case Keys.ShiftKey:
						break;

					case Keys.Space:
						break;

					case Keys.Subtract:
						break;

					case Keys.T:
						break;

					case Keys.Tab:
						break;

					case Keys.U:
						var copy = new AsyncUnbuffCopy();
						copy.AsyncCopyFileUnbuffered(@"J:\Downloads\advinst.msi", @"J:\Downloads\advinst(2).msi", true, false, false, 4, false, 100000);
						break;

					case Keys.Up:
						break;

					case Keys.V:
						this.PasteAvailableFiles();
						break;

					case Keys.VolumeDown:
						break;

					case Keys.VolumeMute:
						break;

					case Keys.VolumeUp:
						break;

					case Keys.W:
						break;

					case Keys.X:
						this.CutSelectedFiles();
						break;

					case Keys.XButton1:
						break;

					case Keys.XButton2:
						break;

					case Keys.Y:
						break;

					case Keys.Z:
						break;

					case Keys.Zoom:
						break;

					default:
						break;
				}
			}

			if (e == Keys.Escape) {
				foreach (var index in this._CuttedIndexes) {
					var item = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_CUT, state = 0 };
					User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMSTATE, index, ref item);
				}
				this._CuttedIndexes.Clear();
				F.Clipboard.Clear();
			}
			if (e == Keys.Delete) {
				this.DeleteSelectedFiles((Control.ModifierKeys & Keys.Shift) != Keys.Shift);
			}
			if (e == Keys.F5) {
				this.RefreshContents();
			}
		}

		#endregion Events

		#region Overrides

		protected override void OnDragDrop(F.DragEventArgs e) {
			int row = -1;
			int collumn = -1;
			this.HitTest(PointToClient(new DPoint(e.X, e.Y)), out row, out collumn);
			ShellItem destination = row != -1 ? Items[row] : CurrentFolder;
			if (!destination.IsFolder || (this.DraggedItemIndexes.Count > 0 && this.DraggedItemIndexes.Contains(row))) {
				if ((e.Effect == F.DragDropEffects.Link || e.Effect == F.DragDropEffects.Copy) && destination.Parent != null && destination.Parent.IsFolder) {
					if (e.Effect == F.DragDropEffects.Copy) {
						this.DoCopy(e.Data, destination);
					}
				} else
					e.Effect = F.DragDropEffects.None;
			} else {
				switch (e.Effect) {
					case F.DragDropEffects.Copy:
						this.DoCopy(e.Data, destination);
						break;
					case F.DragDropEffects.Link:
						System.Windows.MessageBox.Show("Link creation not implemented yet!", "Not implemented", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
			}
			var wp = new BExplorer.Shell.DataObject.Win32Point() { X = e.X, Y = e.Y };

			if (e.Data.GetDataPresent("DragImageBits"))
				DropTargetHelper.Get.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
			else
				base.OnDragDrop(e);
			this.RefreshItem(_LastDropHighLightedItemIndex);
			_LastDropHighLightedItemIndex = -1;
		}

		protected override void OnDragLeave(EventArgs e) {
			try {
				this.RefreshItem(_LastDropHighLightedItemIndex);
				_LastDropHighLightedItemIndex = -1;
			} catch {

			}
			DropTargetHelper.Get.Create.DragLeave();
		}

		internal static void Drag_SetEffect(F.DragEventArgs e) {
			if ((e.KeyState & (8 + 32)) == (8 + 32) && (e.AllowedEffect & F.DragDropEffects.Link) == F.DragDropEffects.Link) {
				// KeyState 8 + 32 = CTL + ALT

				// Link drag-and-drop effect.
				e.Effect = F.DragDropEffects.Link;
			} else if ((e.KeyState & 32) == 32 && (e.AllowedEffect & F.DragDropEffects.Link) == F.DragDropEffects.Link) {
				// ALT KeyState for link.
				e.Effect = F.DragDropEffects.Link;
			} else if ((e.KeyState & 4) == 4 && (e.AllowedEffect & F.DragDropEffects.Move) == F.DragDropEffects.Move) {
				// SHIFT KeyState for move.
				e.Effect = F.DragDropEffects.Move;
			} else if ((e.KeyState & 8) == 8 && (e.AllowedEffect & F.DragDropEffects.Copy) == F.DragDropEffects.Copy) {
				// CTL KeyState for copy.
				e.Effect = F.DragDropEffects.Copy;
			} else if ((e.AllowedEffect & F.DragDropEffects.Move) == F.DragDropEffects.Move) {
				// By default, the drop action should be move, if allowed.
				e.Effect = F.DragDropEffects.Move;
			} else
				e.Effect = F.DragDropEffects.Copy;
		}

		protected override void OnDragOver(F.DragEventArgs e) {

			var wp = new BExplorer.Shell.DataObject.Win32Point() { X = e.X, Y = e.Y };
			Drag_SetEffect(e);

			int row = -1;
			int collumn = -1;
			this.HitTest(PointToClient(new DPoint(e.X, e.Y)), out row, out collumn);
			BExplorer.Shell.DataObject.DropDescription descinvalid = new DataObject.DropDescription();
			descinvalid.type = (int)BExplorer.Shell.DataObject.DropImageType.Invalid;
			((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(descinvalid);
			if (row != -1) {
				this.RefreshItem(_LastDropHighLightedItemIndex);
				this._LastDropHighLightedItemIndex = row;
				this.RefreshItem(row);
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
				desc.szInsert = this.Items[row].DisplayName;
				if (this.DraggedItemIndexes.Contains(row) || !this.Items[row].IsFolder) {
					if (this.Items[row].Extension == ".exe") {
						desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Copy;
						desc.szMessage = "Open With %1";
					} else {
						desc.type = (int)BExplorer.Shell.DataObject.DropImageType.None;
						desc.szMessage = "Cant Drop Here!";
					}
				}
				((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);
			} else {
				this.RefreshItem(_LastDropHighLightedItemIndex);
				this._LastDropHighLightedItemIndex = -1;
				if (e.Effect == F.DragDropEffects.Link) {
					BExplorer.Shell.DataObject.DropDescription desc = new DataObject.DropDescription();
					desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Link;
					desc.szMessage = "Create Link in %1";
					desc.szInsert = this.CurrentFolder.DisplayName;
					((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);
				} else if (e.Effect == F.DragDropEffects.Copy) {
					BExplorer.Shell.DataObject.DropDescription desc = new DataObject.DropDescription();
					desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Link;
					desc.szMessage = "Create a copy in %1";
					desc.szInsert = this.CurrentFolder.DisplayName;
					((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);
				}
			}
			if (e.Data.GetDataPresent("DragImageBits")) {
				DropTargetHelper.Get.Create.DragOver(ref wp, (int)e.Effect);
			} else {
				base.OnDragOver(e);
			}
		}

		protected override void OnDragEnter(F.DragEventArgs e) {
			var wp = new BExplorer.Shell.DataObject.Win32Point() { X = e.X, Y = e.Y };
			Drag_SetEffect(e);

			if (e.Data.GetDataPresent("DragImageBits")) {
				DropTargetHelper.Get.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
			} else {
				base.OnDragEnter(e);
			}
		}


		protected override void OnQueryContinueDrag(F.QueryContinueDragEventArgs e) {
			base.OnQueryContinueDrag(e);
		}


		protected override void OnGiveFeedback(F.GiveFeedbackEventArgs e) {
			e.UseDefaultCursors = true;
			var doo = new F.DataObject(dataObject);
			if (doo.GetDataPresent("DragWindow")) {
				IntPtr hwnd = GetIntPtrFromData(doo.GetData("DragWindow"));
				User32.PostMessage(hwnd, 0x403, IntPtr.Zero, IntPtr.Zero);
			} else {
				e.UseDefaultCursors = true;
			}
			if (IsDropDescriptionValid(dataObject)) {
				e.UseDefaultCursors = false;
				Cursor.Current = Cursors.Arrow;
			} else {
				e.UseDefaultCursors = true;
			}
			if (IsShowingLayered(doo)) {
				e.UseDefaultCursors = false;
				Cursor.Current = Cursors.Arrow;

			} else {
				e.UseDefaultCursors = true;
			}




			//base.OnGiveFeedback(e);
		}
		public static bool IsShowingLayered(F.DataObject dataObject) {
			if (dataObject.GetDataPresent("IsShowingLayered")) {
				object data = dataObject.GetData("IsShowingLayered");
				if (data != null)
					return GetBooleanFromData(data);
			}

			return false;
		}

		private static bool GetBooleanFromData(object data) {
			if (data is Stream) {
				Stream stream = data as Stream;
				BinaryReader reader = new BinaryReader(stream);
				return reader.ReadBoolean();
			}
			// Anything else isn't supported for now
			return false;
		}



		public static bool IsDropDescriptionValid(System.Runtime.InteropServices.ComTypes.IDataObject dataObject) {
			object data = dataObject.GetDropDescription();
			if (data is BExplorer.Shell.DataObject.DropDescription)
				return (BExplorer.Shell.DataObject.DropImageType)((BExplorer.Shell.DataObject.DropDescription)data).type != BExplorer.Shell.DataObject.DropImageType.Invalid;
			return false;
		}

		public static IntPtr GetIntPtrFromData(object data) {
			byte[] buf = null;

			if (data is MemoryStream) {
				buf = new byte[4];
				if (4 != ((MemoryStream)data).Read(buf, 0, 4))
					throw new ArgumentException("Could not read an IntPtr from the MemoryStream");
			}
			if (data is byte[]) {
				buf = (byte[])data;
				if (buf.Length < 4)
					throw new ArgumentException("Could not read an IntPtr from the byte array");
			}

			if (buf == null)
				throw new ArgumentException("Could not read an IntPtr from the " + data.GetType().ToString());

			int p = (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];
			return new IntPtr(p);
		}


		public Int32 InsertNewItem(ShellItem obj) {
			if (!Items.Contains(obj) && !String.IsNullOrEmpty(obj.ParsingName)) {
				Items.Add(obj);
				this.SetSortCollumn(this.LastSortedColumnIndex, this.LastSortOrder, false);
				if (this.IsGroupsEnabled) SetGroupOrder(false);
			}

			var itemIndex = Items.Count - 1;
			if (!ItemsHashed.TryGetValue(obj, out itemIndex)) itemIndex = -1;
			return itemIndex;
		}

		public void UpdateItem(ShellItem obj1, ShellItem obj2) {
			if (this.CurrentRefreshedItemIndex != -1) {
				ShellItem tempItem = Items.SingleOrDefault(s => s.CachedParsingName == obj2.CachedParsingName);
				if (tempItem == null) {
					Items.Insert(this.CurrentRefreshedItemIndex == -1 ? 0 : CurrentRefreshedItemIndex, obj2);
					ItemsHashed.Add(obj2, this.CurrentRefreshedItemIndex == -1 ? 0 : CurrentRefreshedItemIndex);
					if (this.IsGroupsEnabled) {
						this.SetGroupOrder(false);
					}
					this.SetSortCollumn(this.LastSortedColumnIndex, this.LastSortOrder, false);

					if (this.ItemUpdated != null)
						this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Created, obj2, null, ItemsHashed[obj2]));

					this.SelectItemByIndex(ItemsHashed[obj2], true, true);
				}
			} else {
				ShellItem theItem = Items.SingleOrDefault(s => s.ParsingName == obj1.ParsingName);
				if (theItem != null) {
					int itemIndex = Items.IndexOf(theItem);
					Items[itemIndex] = obj2;
					ItemsHashed.Remove(theItem);
					ItemsHashed.Add(obj2, itemIndex);
					User32.SendMessage(this.LVHandle, MSG.LVM_UPDATE, itemIndex, 0);
					if (this.IsGroupsEnabled) {
						this.SetGroupOrder(false);
					}
					this.SetSortCollumn(this.LastSortedColumnIndex, this.LastSortOrder, false);
					RedrawWindow();
					if (this.ItemUpdated != null)
						this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Renamed, obj2, obj1, ItemsHashed[obj2]));

					this.SelectItemByIndex(ItemsHashed[obj2], true, true);
				}
			}
			this.CurrentRefreshedItemIndex = -1;
		}

		public System.Windows.Rect GetItemBounds(int index, int mode) {
			LVITEMINDEX lviLe = new LVITEMINDEX();
			lviLe.iItem = index;
			lviLe.iGroup = this.GetGroupIndex(index);
			var labelBounds = new User32.RECT();
			labelBounds.Left = mode;
			User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lviLe, ref labelBounds);
			return new Rect(labelBounds.Left, labelBounds.Top, labelBounds.Right - labelBounds.Left, labelBounds.Bottom - labelBounds.Top);
		}
		public void RaiseRecycleBinUpdated() {
			if (this.ItemUpdated != null)
				this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.RecycleBin, null, null, -1));
		}

		public void RaiseItemUpdated(ItemUpdateType type, ShellItem old, ShellItem newItem, int index) {
			if (this.ItemUpdated != null)
				this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(type, newItem, old, index));
		}

		
		protected override void WndProc(ref Message m) {
			try {

				#region Staring
				//TODO: Remove Extra If(...)
				if (m.Msg == (int)WM.WM_PARENTNOTIFY) {
					if (User32.LOWORD((int)m.WParam) == (int)WM.WM_MBUTTONDOWN) {
						OnItemMiddleClick();
					}
				}

				#endregion
				base.WndProc(ref m);
				#region m.Msg == 78
				if (m.Msg == 78) {

					#region Starting
					var nmhdrHeader = (NMHEADER)(m.GetLParam(typeof(NMHEADER)));
					if (nmhdrHeader.hdr.code == (int)HDN.HDN_DROPDOWN) {
						F.MessageBox.Show(nmhdrHeader.iItem.ToString());
					} else if (nmhdrHeader.hdr.code == (int)HDN.HDN_BEGINTRACKW) {
						if (this.View != ShellViewStyle.Details) m.Result = (IntPtr)1;
					}
					/*
					else if (nmhdrHeader.hdr.code == (int)HDN.HDN_BEGINTRACKW) {
						if (this.View != ShellViewStyle.Details) m.Result = (IntPtr)1;
					}
					*/
					#endregion

					var nmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));
					switch ((int)nmhdr.code) {
						case WNM.LVN_ENDLABELEDITW:
							#region Case
							var nmlvedit = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
							if (!String.IsNullOrEmpty(nmlvedit.item.pszText)) {
								RenameShellItem(this.Items[nmlvedit.item.iItem].ComInterface, nmlvedit.item.pszText);
								this.RedrawWindow();
							}
							break;
							#endregion

						case WNM.LVN_GETDISPINFOW:
							#region Case
							var nmlv = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
							if (Items.Count == 0 || Items.Count - 1 < nmlv.item.iItem)
								break;
							var currentItem = Items[nmlv.item.iItem];

							if ((nmlv.item.mask & LVIF.LVIF_TEXT) == LVIF.LVIF_TEXT) {
								if (nmlv.item.iSubItem == 0) {
									//nmlv.item.pszText = this.View == ShellViewStyle.Tile ? String.Empty : (!String.IsNullOrEmpty(NewName) ? (ItemForRename == nmlv.item.iItem ? "" : currentItem.DisplayName) : currentItem.DisplayName);
									try {
										nmlv.item.pszText = this.View == ShellViewStyle.Tile ? String.Empty : currentItem.DisplayName;
										if (this.ItemForRealName_IsAny) {
											if (this.GetFirstSelectedItemIndex() == nmlv.item.iItem) {
												nmlv.item.pszText = "";
											}
										}
									} catch (Exception) {

									}

									Marshal.StructureToPtr(nmlv, m.LParam, false);
								} else if (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details) {
									//TODO: Try to remove the Try Catch
									try {
										var hash = currentItem.GetHashCode();
										Collumns currentCollumn = this.Collumns[nmlv.item.iSubItem];
										var valueCached = SubItemValues.ToArray().FirstOrDefault(s => s.Item1 == hash && s.Item2.fmtid == currentCollumn.pkey.fmtid && s.Item2.pid == currentCollumn.pkey.pid);
										if (valueCached != null && valueCached.Item3 != null) {
											String val = String.Empty;
											if (currentCollumn.CollumnType == typeof(DateTime))
												val = ((DateTime)valueCached.Item3).ToString(Thread.CurrentThread.CurrentCulture);
											else if (currentCollumn.CollumnType == typeof(long))
												val = String.Format("{0} KB", (Math.Ceiling(Convert.ToDouble(valueCached.Item3.ToString()) / 1024).ToString("# ### ### ##0"))); //ShlWapi.StrFormatByteSize(Convert.ToInt64(pvar.Value.ToString()));
											else if (currentCollumn.CollumnType == typeof(PerceivedType))
												val = ((PerceivedType)valueCached.Item3).ToString();
											else if (currentCollumn.CollumnType == typeof(FileAttributes)) {
												var resultString = this.GetFilePropertiesString(valueCached.Item3);
												val = resultString;
											} else
												val = valueCached.Item3.ToString();

											nmlv.item.pszText = val;
											Marshal.StructureToPtr(nmlv, m.LParam, false);
										} else {
											ShellItem temp = !(currentItem.IsNetDrive || currentItem.IsNetworkPath) && !currentItem.ParsingName.StartsWith("::") ?
													new ShellItem(currentItem.ParsingName) : currentItem;

											/*
											if (!(currentItem.IsNetDrive || currentItem.IsNetworkPath) && !currentItem.ParsingName.StartsWith("::")) 
												temp = new ShellItem(currentItem.ParsingName);
											else 
												temp = currentItem;
											*/
											var isi2 = (IShellItem2)temp.ComInterface;
											var guid = new Guid(InterfaceGuids.IPropertyStore);
											IPropertyStore propStore = null;
											isi2.GetPropertyStore(GetPropertyStoreOptions.FastPropertiesOnly, ref guid, out propStore);
											PROPERTYKEY pk = currentCollumn.pkey;
											var pvar = new PropVariant();
											if (propStore != null && propStore.GetValue(ref pk, pvar) == HResult.S_OK) {
												//if (propStore.GetValue(ref pk, pvar) == HResult.S_OK) {
												String val = String.Empty;
												if (pvar.Value != null) {
													if (currentCollumn.CollumnType == typeof(DateTime))
														val = ((DateTime)pvar.Value).ToString(Thread.CurrentThread.CurrentCulture);
													else if (currentCollumn.CollumnType == typeof(long))
														val = String.Format("{0} KB", (Math.Ceiling(Convert.ToDouble(pvar.Value.ToString()) / 1024).ToString("# ### ### ##0")));
													//ShlWapi.StrFormatByteSize(Convert.ToInt64(pvar.Value.ToString()));
													else if (currentCollumn.CollumnType == typeof(PerceivedType))
														val = ((PerceivedType)pvar.Value).ToString();
													else if (currentCollumn.CollumnType == typeof(FileAttributes))
														val = this.GetFilePropertiesString(pvar.Value);
													else
														val = pvar.Value.ToString();

													nmlv.item.pszText = val;
													Marshal.StructureToPtr(nmlv, m.LParam, false);
													pvar.Dispose();
												} else {
													ItemsForSubitemsUpdate.Enqueue(new Tuple<int, int, PROPERTYKEY>(nmlv.item.iItem, nmlv.item.iSubItem, pk));
												}
											}
											//}
										}
									} catch {
									}
									//var currentItem = Items[nmlv.item.iItem];
									//var hash = currentItem.GetHashCode();
									//if (hash != null)
									//{
									//	ConcurrentDictionary<Collumns, object> dictionaryValues = null;
									//	if (SubItems.TryGetValue(hash, out dictionaryValues))
									//	{
									//		Collumns currentCollumn = this.Collumns[nmlv.item.iSubItem];
									//		object value = null;
									//		if (dictionaryValues.TryGetValue(currentCollumn, out value))
									//		{
									//			String val = String.Empty;
									//			if (value != null)
									//			{
									//				if (currentCollumn.CollumnType == typeof(DateTime))
									//				{
									//					val = ((DateTime)value).ToString(Thread.CurrentThread.CurrentCulture);
									//				}
									//				else if (currentCollumn.CollumnType == typeof(long))
									//				{
									//					val = String.Format("{0} KB", (Math.Ceiling(Convert.ToDouble(value.ToString()) / 1024).ToString("# ### ### ##0"))); //ShlWapi.StrFormatByteSize(Convert.ToInt64(pvar.Value.ToString()));
									//				}
									//				else
									//				{
									//					val = value.ToString();
									//				}
									//			}
									//			nmlv.item.pszText = val;
									//			Marshal.StructureToPtr(nmlv, m.LParam, false);
									//		}
									//		else
									//		{
									//			ItemsForSubitemsUpdate.Enqueue(nmlv.item.iItem);
									//		}
									//		dictionaryValues = null;
									//	}
									//	else
									//	{
									//		ItemsForSubitemsUpdate.Enqueue(nmlv.item.iItem);
									//	}
									//}
								}
							}

							break;
							#endregion

						case WNM.LVN_COLUMNCLICK:
							#region Case
							var nlcv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
							if (!this.IsGroupsEnabled) {
								SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
							} else if (this.LastGroupCollumn == this.Collumns[nlcv.iSubItem]) {
								this.SetGroupOrder();
							} else {
								SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
								this.SetGroupOrder(false);
							}
							break;
							#endregion

						case WNM.LVN_GETINFOTIP:
							#region Case
							var nmGetInfoTip = (NMLVGETINFOTIP)m.GetLParam(typeof(NMLVGETINFOTIP));
							if (this.Items.Count == 0)
								break;
							var itemInfotip = this.Items[nmGetInfoTip.iItem];
							char[] charBuf = ("\0").ToCharArray();
							Marshal.Copy(charBuf, 0, nmGetInfoTip.pszText, Math.Min(charBuf.Length, nmGetInfoTip.cchTextMax));
							Marshal.StructureToPtr(nmGetInfoTip, m.LParam, false);

							if (ToolTip.IsVisible)
								ToolTip.HideTooltip();

							ToolTip.CurrentItem = itemInfotip;
							ToolTip.ItemIndex = nmGetInfoTip.iItem;
							ToolTip.Type = nmGetInfoTip.dwFlags;
							ToolTip.Left = -500;
							ToolTip.Top = -500;
							ToolTip.ShowTooltip();
							break;
							#endregion

						case WNM.LVN_ODFINDITEM:
							#region Case
							if (this.ToolTip != null && this.ToolTip.IsVisible)
								this.ToolTip.HideTooltip();
							var findItem = (NMLVFINDITEM)m.GetLParam(typeof(NMLVFINDITEM));
							KeyJumpString = findItem.lvfi.psz;

							if (KeyJumpKeyDown != null) {
								KeyJumpKeyDown(this, new KeyEventArgs(Keys.A));
							}
							int startindex = this.GetFirstSelectedItemIndex() + (KeyJumpString.Length > 1 ? 0 : 1);
							int selind = GetFirstIndexOf(KeyJumpString, startindex);
							if (selind != -1) {
								m.Result = (IntPtr)(selind);
								if (IsGroupsEnabled)
									this.SelectItemByIndex(selind, true, true);
							} else {
								int selindOver = GetFirstIndexOf(KeyJumpString, 0);
								if (selindOver != -1) {
									m.Result = (IntPtr)(selindOver);
									if (IsGroupsEnabled)
										this.SelectItemByIndex(selindOver, true, true);
								}
							}
							break;
							#endregion

						case WNM.LVN_INCREMENTALSEARCH: //TODO: Deal with this useless code
							#region Case
							var incrementalSearch = (NMLVFINDITEM)m.GetLParam(typeof(NMLVFINDITEM));
							break;
							#endregion

						case -175:
							#region Case
							var nmlvLe = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
							RenameItem(nmlvLe.item.iItem);
							m.Result = (IntPtr)1;
							break;
							#endregion

						case WNM.LVN_ITEMACTIVATE:
							#region Case
							if (this.ToolTip != null && this.ToolTip.IsVisible)
								this.ToolTip.HideTooltip();
							if (ItemForRealName_IsAny) {
								this.EndLabelEdit();
							} else {
								var iac = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
								ShellItem selectedItem = Items[iac.iItem];
								if (selectedItem.IsFolder) {
									Navigate_Full(selectedItem, true);
								} else if (selectedItem.IsLink && selectedItem.ParsingName.EndsWith(".lnk")) {
									var shellLink = new ShellLink(selectedItem.ParsingName);
									var newSho = new ShellItem(shellLink.TargetPIDL);
									if (newSho.IsFolder)
										Navigate_Full(newSho, true);
								} else {
									StartProcessInCurrentDirectory(selectedItem);
								}
							}
							break;
							#endregion

						case WNM.LVN_BEGINSCROLL:
							#region Case
							this.EndLabelEdit();
							resetEvent.Reset();
							_ResetTimer.Stop();
							ToolTip.HideTooltip();
							break;
							#endregion

						case WNM.LVN_ENDSCROLL:
							#region Case
							_ResetTimer.Start();

							break;
							#endregion

						case -100:
							#region Case
							F.MessageBox.Show("AM");
							break;
							#endregion

						case WNM.LVN_ITEMCHANGED:
							#region Case
							var nlv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
							if ((nlv.uChanged & LVIF.LVIF_STATE) == LVIF.LVIF_STATE) {
								this._IsDragSelect = nlv.uNewState;
								if (IsGroupsEnabled) {
									if (nlv.iItem != -1) {
										var itemBounds = new User32.RECT();
										var lvi = new LVITEMINDEX();
										lvi.iItem = nlv.iItem;
										lvi.iGroup = this.GetGroupIndex(nlv.iItem);
										User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
										RedrawWindow(itemBounds);
									} else {
										RedrawWindow();
									}
								}
								if (!selectionTimer.Enabled) {
									selectionTimer.Start();
								}
							}

							break;
							#endregion

						case WNM.LVN_ODSTATECHANGED:
							#region Case
							OnSelectionChanged();
							break;
							#endregion

						case WNM.LVN_KEYDOWN:
							#region Case
							var nkd = (NMLVKEYDOWN)m.GetLParam(typeof(NMLVKEYDOWN));
							ShellView_KeyDown((Keys)((int)nkd.wVKey));

							if (!ItemForRealName_IsAny) {
								switch (nkd.wVKey) {
									case (short)Keys.F2:
										RenameSelectedItem();
										break;

									case (short)Keys.Enter:
										var selectedItem = this.GetFirstSelectedItem();
										if (selectedItem.IsFolder) {
											Navigate(selectedItem);
										} else if (selectedItem.IsLink && selectedItem.ParsingName.EndsWith(".lnk")) {
											var shellLink = new ShellLink(selectedItem.ParsingName);
											var newSho = new ShellItem(shellLink.TargetPIDL);
											if (newSho.IsFolder)
												Navigate(newSho);
											else
												StartProcessInCurrentDirectory(newSho);

											shellLink.Dispose();
										} else {
											StartProcessInCurrentDirectory(selectedItem);
										}
										break;
								}

								this.Focus();
							} else {
								Input.InputManager.Current.ProcessInput(
									new Input.KeyEventArgs(Input.Keyboard.PrimaryDevice, Input.Keyboard.PrimaryDevice.ActiveSource, Environment.TickCount,
										Input.KeyInterop.KeyFromVirtualKey(nkd.wVKey)) {
											RoutedEvent = System.Windows.Controls.Control.KeyDownEvent
										});
								m.Result = (IntPtr)1;
								switch (nkd.wVKey) {
									case (short)Keys.Enter:
                    if (!this.IsRenameInProgress)
										  this.EndLabelEdit();
										this.Focus();
										break;

									case (short)Keys.Escape:
										this.EndLabelEdit(true);
										this.Focus();
										break;

									default:
										break;
								}
							}
							break;
							#endregion

						case WNM.LVN_GROUPINFO: //TODO: Deal with this useless code
							#region Case
							//RedrawWindow();
							break;

							#endregion

						case WNM.LVN_HOTTRACK:
							#region Case
							var nlvHotTrack = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
							if (nlvHotTrack.iItem != ToolTip.ItemIndex) {
								ToolTip.HideTooltip();
								this.Focus();
							}

							break;
							#endregion

						case WNM.LVN_BEGINDRAG:
							#region Case
							this.DraggedItemIndexes.Clear();
							IntPtr dataObjPtr = IntPtr.Zero;
							dataObject = this.SelectedItems.ToArray().GetIDataObject(out dataObjPtr);
							uint ef = 0;
							var ishell2 = (BExplorer.Shell.DataObject.IDragSourceHelper2)new DragDropHelper();
							ishell2.SetFlags(1);
							var wp = new BExplorer.Shell.DataObject.Win32Point();
							wp.X = Cursor.Position.X;
							wp.Y = Cursor.Position.Y;
							ishell2.InitializeFromWindow(this.Handle, ref wp, dataObject);
							DoDragDrop(dataObject, F.DragDropEffects.All | F.DragDropEffects.Link);
							//Shell32.SHDoDragDrop(this.Handle, dataObject, null, unchecked((uint)F.DragDropEffects.All | (uint)F.DragDropEffects.Link), out ef);
							break;
							#endregion

						case WNM.NM_RCLICK:
							#region Case
							var nmhdrHdn = (NMHEADER)(m.GetLParam(typeof(NMHEADER)));
							var itemActivate = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
              if (this.ToolTip != null)
                this.ToolTip.HideTooltip();

							if (nmhdrHdn.iItem != -1 && nmhdrHdn.hdr.hwndFrom == this.LVHandle) {
								var selitems = this.SelectedItems;
								var cm = new ShellContextMenu(selitems.ToArray());
								cm.ShowContextMenu(this, itemActivate.ptAction, CMF.CANRENAME);
							} else if (nmhdrHdn.iItem == -1) {
								var cm = new ShellContextMenu(new ShellItem[1] { this.CurrentFolder }, SVGIO.SVGIO_BACKGROUND, this);
								cm.ShowContextMenu(this, itemActivate.ptAction, 0, true);
							} else if (ColumnHeaderRightClick != null) {
								ColumnHeaderRightClick(this, new MouseEventArgs(F.MouseButtons.Right, 1, MousePosition.X, MousePosition.Y, 0));
							}
							break;
							#endregion

						case WNM.NM_CLICK: //TODO: Deal with this useless code
							#region Case
							break;
							#endregion

						case WNM.NM_SETFOCUS:
							#region Case
							if (IsGroupsEnabled)
								RedrawWindow();
							ShellView_GotFocus();
							this.IsFocusAllowed = true;
							break;
							#endregion

						case WNM.NM_KILLFOCUS:
							#region Case
              if (this.ItemForRename != -1 && !this.IsRenameInProgress)
                EndLabelEdit();
							if (IsGroupsEnabled)
								RedrawWindow();
							if (this.ToolTip != null && this.ToolTip.IsVisible)
								this.ToolTip.HideTooltip();
							//OnLostFocus();
							this.Focus();
							break;
							#endregion

						case CustomDraw.NM_CUSTOMDRAW:
							#region Case
							if (nmhdr.hwndFrom == this.LVHandle) {
								#region Starting
								User32.SendMessage(this.LVHandle, 296, User32.MAKELONG(1, 1), 0);
								var nmlvcd = (User32.NMLVCUSTOMDRAW)m.GetLParam(typeof(User32.NMLVCUSTOMDRAW));
								var index = (int)nmlvcd.nmcd.dwItemSpec;
								var hdc = nmlvcd.nmcd.hdc;
								if (nmlvcd.dwItemType == 1)
									return;

								ShellItem sho = Items.Count > index ? Items[index] : null;



								System.Drawing.Color? textColor = null;
								if (sho != null && this.LVItemsColorCodes != null && this.LVItemsColorCodes.Count > 0 && !String.IsNullOrEmpty(sho.Extension)) {
									var extItemsAvailable = this.LVItemsColorCodes.Where(c => c.ExtensionList.Contains(sho.Extension)).Count() > 0;
									if (extItemsAvailable) {
										var color = this.LVItemsColorCodes.Where(c => c.ExtensionList.ToLowerInvariant().Contains(sho.Extension)).Select(c => c.TextColor).SingleOrDefault();
										textColor = color;
									}
								}
								#endregion

								switch (nmlvcd.nmcd.dwDrawStage) {
									case CustomDraw.CDDS_PREPAINT:
										#region Case
										m.Result = (IntPtr)(CustomDraw.CDRF_NOTIFYITEMDRAW | CustomDraw.CDRF_NOTIFYPOSTPAINT | 0x40);
										break;
										#endregion

									case CustomDraw.CDDS_POSTPAINT:
										#region Case
										m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
										break;
										#endregion

									case CustomDraw.CDDS_ITEMPREPAINT:
										#region Case
										if ((nmlvcd.nmcd.uItemState & CDIS.DROPHILITED) == CDIS.DROPHILITED && index != _LastDropHighLightedItemIndex) {
											nmlvcd.nmcd.uItemState = CDIS.DEFAULT;
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);
										}
										if (index == _LastDropHighLightedItemIndex) {
											nmlvcd.nmcd.uItemState |= CDIS.DROPHILITED;
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);
										}
										//using (var graphics = Graphics.FromHdc(hdc)) {
										//	graphics.FillRectangle(Brushes.WhiteSmoke, new RectangleF(nmlvcd.nmcd.rc.Left, nmlvcd.nmcd.rc.Top, nmlvcd.nmcd.rc.Right - nmlvcd.nmcd.rc.Left, nmlvcd.nmcd.rc.Bottom - nmlvcd.nmcd.rc.Top));
										//}
										//if (this.View == ShellViewStyle.Details) {
										//	nmlvcd.clrTextBk = ColorTranslator.ToWin32(Color.WhiteSmoke);
										//	Marshal.StructureToPtr(nmlvcd, m.LParam, false);
										//}

										if (textColor != null) {
											nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);

											m.Result = (IntPtr)(CustomDraw.CDRF_NEWFONT | CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW | 0x40);
										} else {
											m.Result = (IntPtr)(CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW | 0x40);
										}
										break;
										#endregion

									case CustomDraw.CDDS_ITEMPREPAINT | CustomDraw.CDDS_SUBITEM:
										#region Case
										//if (this.View == ShellViewStyle.Details && nmlvcd.iSubItem > 0) {
										//	nmlvcd.clrTextBk = ColorTranslator.ToWin32(Color.WhiteSmoke);
										//	Marshal.StructureToPtr(nmlvcd, m.LParam, false);
										//}
										if (textColor == null) {
											m.Result = (IntPtr)CustomDraw.CDRF_DODEFAULT;
										} else {
											nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);
											m.Result = (IntPtr)CustomDraw.CDRF_NEWFONT;
										}
										break;
										#endregion

									case CustomDraw.CDDS_ITEMPOSTPAINT:
										#region Case
										if (nmlvcd.clrTextBk != 0 && nmlvcd.dwItemType == 0) {
											var itemBounds = nmlvcd.nmcd.rc;
											var lvi = new LVITEMINDEX();
											lvi.iItem = index;
											lvi.iGroup = this.GetGroupIndex(index);
											var iconBounds = new User32.RECT() { Left = 1 };
											User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref iconBounds);
											var lvItem = new LVITEM() {
												iItem = index,
												iGroupId = lvi.iGroup,
												iGroup = lvi.iGroup,
												mask = LVIF.LVIF_STATE,
												stateMask = LVIS.LVIS_SELECTED
											};
											var lvItemImageMask = new LVITEM() {
												iItem = index,
												iGroupId = lvi.iGroup,
												iGroup = lvi.iGroup,
												mask = LVIF.LVIF_STATE,
												stateMask = LVIS.LVIS_STATEIMAGEMASK
											};

											if (sho != null) {
												var cutFlag = (User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_CUT) & LVIS.LVIS_CUT) == LVIS.LVIS_CUT;
												if (sho.OverlayIconIndex == -1) {
													overlayQueue.Enqueue(index);
												}

												if (IconSize != 16) {
													WTS_CACHEFLAGS flags;
													bool retrieved = false;
													IntPtr hThumbnail = IntPtr.Zero;
													//thumbnailResult = sho.Thumbnail.ExtractAndDrawThumbnail(hdc, (uint)IconSize, out flags, iconBounds, out retrieved, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index));
													hThumbnail = sho.Thumbnail.GetHBitmap(IconSize, true);
													int width = 0;
													int height = 0;
													if (hThumbnail != IntPtr.Zero) {
														Gdi32.ConvertPixelByPixel(hThumbnail, out width, out height);
														Gdi32.NativeDraw(hdc, hThumbnail, iconBounds.Left + (iconBounds.Right - iconBounds.Left - width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - height) / 2, width, height, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index));
														Gdi32.DeleteObject(hThumbnail);
														retrieved = true;
													}
													if (retrieved && sho.IsNeedRefreshing) {
														sho.Thumbnail.ExtractAndDrawThumbnail(hdc, (uint)IconSize, out flags, iconBounds, out retrieved, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index), true);
														sho.IsNeedRefreshing = false;
													}
													if (retrieved) {
														if (((width > height && width != IconSize) || (height > width && height != IconSize) || (width == height && width != IconSize)) && sho.IsOnlyLowQuality == false) {
															ThumbnailsForCacheLoad.Enqueue(index);
														} else {
															sho.IsThumbnailLoaded = true;
															sho.IsNeedRefreshing = false;
														}
													} else {
														if (!sho.IsThumbnailLoaded)
															ThumbnailsForCacheLoad.Enqueue(index);
														if ((sho.IconType & IExtractIconPWFlags.GIL_PERCLASS) == IExtractIconPWFlags.GIL_PERCLASS) {
															var hbitmap = sho.Thumbnail.GetHBitmap(IconSize);

															Gdi32.NativeDraw(hdc, hbitmap, iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index));

															sho.IsIconLoaded = true;
														} else if ((sho.IconType & IExtractIconPWFlags.GIL_PERINSTANCE) == IExtractIconPWFlags.GIL_PERINSTANCE) {
															if (sho.Parent != null && (sho.Parent.ParsingName == KnownFolders.Network.ParsingName || sho.Parent.IsSearchFolder)) {
																var hIconExe = sho.Thumbnail.GetHBitmap(IconSize);
																Gdi32.NativeDraw(hdc, hIconExe, iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index));
																sho.IsIconLoaded = true;
															} else
																if (!sho.IsIconLoaded) {
																	waitingThumbnails.Enqueue(index);
																	using (var g = Graphics.FromHdc(hdc)) {
																		if (IconSize == 16) {
																			g.DrawImage(ExeFallBack16, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		} else if (IconSize <= 48) {
																			g.DrawImage(ExeFallBack48, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		} else if (IconSize <= 256) {
																			g.DrawImage(ExeFallBack256, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		}
																	}
																} else {
																	var hIconExe = sho.Thumbnail.GetHBitmap(IconSize);
																	Gdi32.NativeDraw(hdc, hIconExe, iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index));
																	sho.IsIconLoaded = true;
																}
														}
													}
													using (var g = Graphics.FromHdc(hdc)) {
														if (this.ShowCheckboxes && View != ShellViewStyle.Details && View != ShellViewStyle.List) {
															var res = User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMW, 0, ref lvItemImageMask);

															if ((nmlvcd.nmcd.uItemState & CDIS.HOT) == CDIS.HOT || (uint)lvItemImageMask.state == (2 << 12)) {
																res = User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMW, 0, ref lvItem);
																var checkboxOffsetH = 14;
																var checkboxOffsetV = 2;
																if (View == ShellViewStyle.Tile || View == ShellViewStyle.SmallIcon)
																	checkboxOffsetH = 2;
																if (View == ShellViewStyle.Tile)
																	checkboxOffsetV = 1;

																CheckBoxRenderer.DrawCheckBox(g, new DPoint(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV),
																	lvItem.state != 0 ? F.VisualStyles.CheckBoxState.CheckedNormal : F.VisualStyles.CheckBoxState.UncheckedNormal
																);
															}
														}
													}
													if (sho.OverlayIconIndex > 0) {
														if (this.IconSize > 180) {
															jumbo.DrawOverlay(hdc, sho.OverlayIconIndex, new DPoint(iconBounds.Left, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
														} else if (this.IconSize > 64) {
															extra.DrawOverlay(hdc, sho.OverlayIconIndex, new DPoint(iconBounds.Left + 10, iconBounds.Bottom - 50));
														} else {
															large.DrawOverlay(hdc, sho.OverlayIconIndex, new DPoint(iconBounds.Left + 10, iconBounds.Bottom - 32));
														}
													}
													if (sho.IsShielded > 0) {
														if (this.IconSize > 180) {
															jumbo.DrawIcon(hdc, sho.IsShielded, new DPoint(iconBounds.Right - this.IconSize / 3, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
														} else if (this.IconSize > 64) {
															extra.DrawIcon(hdc, sho.IsShielded, new DPoint(iconBounds.Right - 60, iconBounds.Bottom - 50));
														} else {
															large.DrawIcon(hdc, sho.IsShielded, new DPoint(iconBounds.Right - 42, iconBounds.Bottom - 32));
														}
													}
													if (sho.IsShared) {
														if (this.IconSize > 180) {
															jumbo.DrawIcon(hdc, this._SharedIconIndex, new DPoint(iconBounds.Right - this.IconSize / 3, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
														} else if (this.IconSize > 64) {
															extra.DrawIcon(hdc, this._SharedIconIndex, new DPoint(iconBounds.Right - 60, iconBounds.Bottom - 50));
														} else {
															large.DrawIcon(hdc, this._SharedIconIndex, new DPoint(iconBounds.Right - 42, iconBounds.Bottom - 32));
														}
													}
													if (View == ShellViewStyle.Tile) {
														var lableBounds = new User32.RECT() { Left = 2 };
														User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref lableBounds);

														using (var g = Graphics.FromHdc(hdc)) {
															var fmt = new StringFormat();
															fmt.Trimming = StringTrimming.EllipsisCharacter;
															fmt.Alignment = StringAlignment.Center;
															fmt.Alignment = StringAlignment.Near;
															fmt.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
															fmt.LineAlignment = StringAlignment.Center;

															var lblrectTiles = new RectangleF(lableBounds.Left, itemBounds.Top + 4, lableBounds.Right - lableBounds.Left, 20);
															Font font = System.Drawing.SystemFonts.IconTitleFont;
															var textBrush = new SolidBrush(textColor ?? System.Drawing.SystemColors.ControlText);
															g.DrawString(sho.DisplayName, font, textBrush, lblrectTiles, fmt);
															font.Dispose();
															textBrush.Dispose();
														}
													}
												} else {
													sho.IsThumbnailLoaded = true;
													if ((sho.IconType & IExtractIconPWFlags.GIL_PERCLASS) == IExtractIconPWFlags.GIL_PERCLASS) {
														var hIconExe = sho.Thumbnail.GetHBitmap(IconSize);
														if (hIconExe != IntPtr.Zero) {
															sho.IsIconLoaded = true;
															Gdi32.NativeDraw(hdc, hIconExe, iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index));
														}
													} else if ((sho.IconType & IExtractIconPWFlags.GIL_PERINSTANCE) == IExtractIconPWFlags.GIL_PERINSTANCE) {
														if (!sho.IsIconLoaded) {
															waitingThumbnails.Enqueue(index);
															using (Graphics g = Graphics.FromHdcInternal(hdc)) {
																g.DrawImage(ExeFallBack16, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
															}
														} else {
															var hIconExe = sho.Thumbnail.GetHBitmap(IconSize);
															if (hIconExe != IntPtr.Zero) {
																sho.IsIconLoaded = true;
																Gdi32.NativeDraw(hdc, hIconExe, iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index));
															}
														}
													}
													if (sho.OverlayIconIndex > 0) {
														small.DrawOverlay(hdc, sho.OverlayIconIndex, new DPoint(iconBounds.Left, iconBounds.Bottom - 16));

													}
													if (sho.IsShielded > 0) {

														small.DrawIcon(hdc, sho.IsShielded, new DPoint(iconBounds.Right - 10, iconBounds.Bottom - 10), 8);

													}
													if (sho.IsShared) {

														small.DrawIcon(hdc, this._SharedIconIndex, new DPoint(iconBounds.Right - 10, iconBounds.Bottom - 10), 8);

													}
												}

												if (!sho.IsInitialised) {
													//OnItemDisplayed(sho, index);
													sho.IsInitialised = true;
												}
											}
										}
										m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
										break;
										#endregion
								}
							}

							break;
							#endregion
					}
				}
				#endregion

			} catch (Exception ex) {
				resetEvent.Set();
			}
		}

		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);
			User32.MoveWindow(this.LVHandle, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, true);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			MessageHandlerWindow = new MessageHandler(this);

			var il = new F.ImageList() { ImageSize = new System.Drawing.Size(48, 48) };
			var ils = new F.ImageList() { ImageSize = new System.Drawing.Size(16, 16) };
			var icc = new ComCtl32.INITCOMMONCONTROLSEX() { dwSize = Marshal.SizeOf(typeof(ComCtl32.INITCOMMONCONTROLSEX)), dwICC = 1 };
			var res = ComCtl32.InitCommonControlsEx(ref icc);

			this.LVHandle = User32.CreateWindowEx(0, "SysListView32", "", User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_CLIPCHILDREN | User32.WindowStyles.WS_CLIPSIBLINGS |
						(User32.WindowStyles)User32.LVS_EDITLABELS | (User32.WindowStyles)User32.LVS_OWNERDATA | (User32.WindowStyles)User32.LVS_SHOWSELALWAYS | (User32.WindowStyles)User32.LVS_AUTOARRANGE,
								0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, this.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			User32.ShowWindow(this.LVHandle, User32.ShowWindowCommands.Show);

			this.AddDefaultColumns(true);

			IntPtr headerhandle = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETHEADER, 0, 0);
			for (int i = 0; i < this.Collumns.Count; i++) {
				this.Collumns[i].SetSplitButton(headerhandle, i);
			}

			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);

			this.View = ShellViewStyle.Medium;

			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.HeaderInAllViews, (int)ListViewExtendedStyles.HeaderInAllViews);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_AUTOAUTOARRANGE, (int)ListViewExtendedStyles.LVS_EX_AUTOAUTOARRANGE);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER, (int)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.FullRowSelect, (int)ListViewExtendedStyles.FullRowSelect);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.HeaderDragDrop, (int)ListViewExtendedStyles.HeaderDragDrop);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LabelTip, (int)ListViewExtendedStyles.LabelTip);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.InfoTip, (int)ListViewExtendedStyles.InfoTip);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.UnderlineHot, (int)ListViewExtendedStyles.UnderlineHot);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.AutosizeColumns, (int)ListViewExtendedStyles.AutosizeColumns);
			//User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.colum, (int)ListViewExtendedStyles.AutosizeColumns);
			//User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.TrackSelect, (int)ListViewExtendedStyles.TrackSelect);
			this.Focus();
			User32.SetForegroundWindow(this.LVHandle);
			UxTheme.SetWindowTheme(this.LVHandle, "Explorer", 0);
			MessageHandlerWindow.Show();
			// 			var dwin = User32.GetShellWindow();
			// 			F.MessageBox.Show(dwin.ToString());


		}

		protected override void OnHandleDestroyed(EventArgs e) {
			try {
				this.MessageHandlerWindow.Close();
				if (_IconLoadingThread.IsAlive)
					_IconLoadingThread.Abort();
				if (_IconCacheLoadingThread.IsAlive)
					_IconCacheLoadingThread.Abort();
				if (_OverlaysLoadingThread.IsAlive)
					_OverlaysLoadingThread.Abort();
				if (_UpdateSubitemValuesThread.IsAlive)
					_UpdateSubitemValuesThread.Abort();
			} catch (ThreadAbortException) { } catch { }
			base.OnHandleDestroyed(e);
		}

		#endregion Overrides

		#region Public Methods


		public void ShowFileProperties() {
			IntPtr doPtr = IntPtr.Zero;
			if (Shell32.SHMultiFileProperties(this.SelectedItems.ToArray().GetIDataObject(out doPtr), 0) != 0 /*S_OK*/) {
				throw new Win32Exception();
			}
		}

		public void ShowPropPage(IntPtr HWND, string filename, string proppage) { Shell32.SHObjectProperties(HWND, 0x2, filename, proppage); }


		private void RedrawItem(int index) {
			var itemBounds = new User32.RECT() { Left = 1 };
			var lvi = new LVITEMINDEX() { iItem = index, iGroup = this.GetGroupIndex(index) };
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
			itemBounds.Left -= 2;
			itemBounds.Top -= 2;
			itemBounds.Bottom += 2;
			itemBounds.Right += 2;

			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_REDRAWITEMS, index, index);
			for (int i = 0; i < 1; i++) {
				if (IsGroupsEnabled) {
					RedrawWindow(itemBounds);
				}
			}
		}

		public void UpdateItem(int index) { User32.SendMessage(this.LVHandle, Interop.MSG.LVM_UPDATE, index, 0); }

		/// <summary> Navigates to the parent of the currently displayed folder. </summary>
		public void NavigateParent() { Navigate_Full(CurrentFolder.Parent, true); }

		public void RefreshContents() { Navigate_Full(this.CurrentFolder, true, refresh: true); }

		public void RefreshItem(int index, Boolean IsForceRedraw = false, Boolean convertName = true) {
			if (IsForceRedraw) {
				try {
					this.Items[index] = convertName ? ShellItem.ToShellParsingName(this.Items[index].ParsingName) : new ShellItem(this.Items[index].CachedParsingName);
					this.Items[index].IsNeedRefreshing = true;
					this.Items[index].IsInvalid = true;
					this.Items[index].OverlayIconIndex = -1;
					this.Items[index].IsOnlyLowQuality = false;
				} catch {
					//In case the event late and the file is not there anymore or changed catch the exception
				}
			}
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_REDRAWITEMS, index, index);
			if (this.IsGroupsEnabled) {
				var itemBounds = this.GetItemBounds(index, 0);
				var rect = new User32.RECT() {
					Left = (int)itemBounds.Left,
					Right = (int)itemBounds.Right,
					Top = (int)itemBounds.Top,
					Bottom = (int)itemBounds.Bottom,
				};
				this.RedrawWindow(rect);
			}
		}

		private void RenameItem(int index) {
			this.IsFocusAllowed = false;
			this.ItemForRename = index;
			if (this.BeginItemLabelEdit != null) {
				this.BeginItemLabelEdit.Invoke(this, new RenameEventArgs(index));
			}

			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_UPDATE, index, 0);
			RedrawWindow();
		}

		public void RenameSelectedItem() { this.RenameItem(this.GetFirstSelectedItemIndex()); }

		public void CutSelectedFiles() {
			foreach (var index in this.SelectedIndexes) {
				var item = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_CUT, state = LVIS.LVIS_CUT };
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, index, ref item);
			}

			this._CuttedIndexes.AddRange(this.SelectedIndexes.ToArray());
			var ddataObject = new F.DataObject();
			// Copy or Cut operation (5 = copy; 2 = cut)
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 2, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, this.SelectedItems.ToArray().CreateShellIDList());
			F.Clipboard.SetDataObject(ddataObject, true);
		}

		public void CopySelectedFiles() {
			var ddataObject = new F.DataObject();
			// Copy or Cut operation (5 = copy; 2 = cut)
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 5, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, this.SelectedItems.ToArray().CreateShellIDList());
			F.Clipboard.SetDataObject(ddataObject, true);
		}

		public void PasteAvailableFiles() {
			var handle = this.Handle;
			var view = this;
			var thread = new Thread(() => {
				var dataObject = F.Clipboard.GetDataObject();
				var dropEffect = dataObject.ToDropEffect();
				if (dataObject.GetDataPresent("Shell IDList Array")) {
					var shellItemArray = dataObject.ToShellItemArray();
					var items = shellItemArray.ToArray();
					try {
						var sink = new FOperationProgressSink(view);
						var fo = new IIFileOperation(sink, handle, true);
						foreach (var item in items) {
							if (dropEffect == System.Windows.DragDropEffects.Copy)
								fo.CopyItem(item, this.CurrentFolder);
							else
								fo.MoveItem(item, this.CurrentFolder.ComInterface, null);
							Marshal.ReleaseComObject(item);
						}
						Marshal.ReleaseComObject(shellItemArray);
						shellItemArray = null;
						items = null;

						fo.PerformOperations();
					} catch (SecurityException) {
						throw;
					}
				} else if (dataObject.GetDataPresent("FileDrop")) {
					var items = ((String[])dataObject.GetData("FileDrop")).Select(s => ShellItem.ToShellParsingName(s).ComInterface).ToArray();
					try {
						var sink = new FOperationProgressSink(view);
						var fo = new IIFileOperation(sink, handle, true);
						foreach (var item in items) {
							if (dropEffect == System.Windows.DragDropEffects.Copy)
								fo.CopyItem(item, this.CurrentFolder);
							else
								fo.MoveItem(item, this.CurrentFolder.ComInterface, null);
							Marshal.ReleaseComObject(item);
						}
						items = null;

						fo.PerformOperations();
					} catch (SecurityException) {
						throw;
					}
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		private void Do_Copy_OR_Move_Helper(bool Copy, ShellItem destination, IShellItem[] Items) {
			var handle = this.Handle;
			var thread = new Thread(() => {
				var fo = new IIFileOperation(handle);
				foreach (var item in Items) {
					if (Copy)
						fo.CopyItem(item, destination);
					else
						fo.MoveItem(item, destination.ComInterface, null);
				}
				fo.PerformOperations();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		private void Do_Copy_OR_Move_Helper_2(bool Copy, ShellItem destination, F.IDataObject dataObject) {
			var handle = this.Handle;
			IShellItemArray shellItemArray = null;
			IShellItem[] items = null;
			if (((F.DataObject)dataObject).ContainsFileDropList()) {
				items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => ShellItem.ToShellParsingName(s).ComInterface).ToArray();
			} else {
				shellItemArray = dataObject.ToShellItemArray();
				items = shellItemArray.ToArray();
			}
			var thread = new Thread(() => {
				try {
					var fo = new IIFileOperation(handle);
					foreach (var item in items) {
						if (Copy)
							fo.CopyItem(item, destination);
						else
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

		public void DoCopy(ShellItem destination) {
			Do_Copy_OR_Move_Helper(true, destination, this.SelectedItems.Select(s => s.ComInterface).ToArray());
		}

		public void DoCopy(System.Windows.IDataObject dataObject, ShellItem destination) {
			Do_Copy_OR_Move_Helper(true, destination, dataObject.ToShellItemArray().ToArray());
		}

		public void DoCopy(F.IDataObject dataObject, ShellItem destination) {
			Do_Copy_OR_Move_Helper_2(true, destination, dataObject);
		}

		public void DoMove(System.Windows.IDataObject dataObject, ShellItem destination) {
			Do_Copy_OR_Move_Helper(false, destination, dataObject.ToShellItemArray().ToArray());
		}

		public void DoMove(ShellItem destination) {
			Do_Copy_OR_Move_Helper(false, destination, this.SelectedItems.Select(s => s.ComInterface).ToArray());
		}

		public void DoMove(F.IDataObject dataObject, ShellItem destination) {
			Do_Copy_OR_Move_Helper_2(false, destination, dataObject);
		}

		public void DeleteSelectedFiles(Boolean isRecycling) {
			var handle = this.Handle;
			var view = this;
			var thread = new Thread(() => {
				var sink = new FOperationProgressSink(view);
				var fo = new IIFileOperation(sink, handle, isRecycling);
				foreach (var item in this.SelectedItems.Select(s => s.ComInterface).ToArray()) {
					fo.DeleteItem(item);
				}
				fo.PerformOperations();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		public void RenameShellItem(IShellItem item, String newName) {
			var fo = new IIFileOperation(this.LVHandle);
			fo.RenameItem(item, newName);
			fo.PerformOperations();
		}

		public void ResizeIcons(int value) {
			try {
				IconSize = value;
				ThumbnailsForCacheLoad.Clear();
				waitingThumbnails.Clear();
				foreach (var obj in this.Items) {
					obj.IsIconLoaded = false;
				}
				var il = new F.ImageList() { ImageSize = new System.Drawing.Size(value, value) };
				var ils = new F.ImageList() { ImageSize = new System.Drawing.Size(16, 16) };
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETICONSPACING, 0, (IntPtr)User32.MAKELONG(value + 28, value + 42));
			} catch (Exception) {
			}
		}

		/// <summary> Runs an application as an administrator. </summary>
		/// <param name="ExePath"> The path of the application. </param>
		public static void RunExeAsAdmin(string ExePath) {
			Process.Start(new ProcessStartInfo {
				FileName = ExePath,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = String.Format("/env /user:Administrator \"{0}\"", ExePath),
			});
		}

		public void SelectAll() {
			var item = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = LVIS.LVIS_SELECTED };
			User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMSTATE, -1, ref item);
			this.Focus();
		}

		public void SelectItems(ShellItem[] ShellObjectArray) {
			this.DeSelectAllItems();
			foreach (ShellItem item in ShellObjectArray) {
				try {
					var lvii = new LVITEMINDEX() { iItem = ItemsHashed[item], iGroup = this.GetGroupIndex(ItemsHashed[item]) };
					var lvi = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = LVIS.LVIS_SELECTED };
					User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMINDEXSTATE, ref lvii, ref lvi);
				} catch (Exception) {
					//catch the given key was not found. It happen on fast delete of items
				}
			}
			this.Focus();
		}

		public void SelectItemByIndex(int index, bool ensureVisisble = false, bool deselectOthers = false) {
			var lvii = new LVITEMINDEX() { iItem = index, iGroup = this.GetGroupIndex(index) };
			var lvi = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = LVIS.LVIS_SELECTED };

			if (deselectOthers) {
				var lviid = new LVITEMINDEX() { iItem = -1, iGroup = 0 };
				var lviDeselect = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = 0 };
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMINDEXSTATE, ref lviid, ref lviDeselect);
			}

			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMINDEXSTATE, ref lvii, ref lvi);
			if (ensureVisisble) User32.SendMessage(this.LVHandle, Interop.MSG.LVM_ENSUREVISISBLE, index, 0);
			this.Focus();
		}

		public void DropHighLightItemByIndex(int index) {
			var lvii = new LVITEMINDEX() { iItem = -1, iGroup = this.GetGroupIndex(index) };
			var lvi = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_DROPHILITED, state = LVIS.LVIS_DROPHILITED };
			var u = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMINDEXSTATE, ref lvii, ref lvi);

			this.Focus();
		}

		private void UpdateColsInView(bool isDetails = false) {
			IntPtr headerhandle = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETHEADER, 0, 0);
			foreach (var col in this.Collumns) {
				var colIndex = this.Collumns.IndexOf(col);
				var colNative = col.ToNativeColumn(isDetails);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETCOLUMN, colIndex, ref colNative);
				col.SetSplitButton(headerhandle, colIndex);
			}
		}

		public void SetColInView(Collumns col, bool Remove) {
			if (Remove) {
				Collumns theColumn = this.Collumns.SingleOrDefault(s => s.pkey.fmtid == col.pkey.fmtid && s.pkey.pid == col.pkey.pid);
				if (theColumn != null) {
					int colIndex = this.Collumns.IndexOf(theColumn);
					this.Collumns.Remove(theColumn);
					User32.SendMessage(this.LVHandle, Interop.MSG.LVM_DELETECOLUMN, colIndex, 0);
				}
			} else if (this.Collumns.Count(s => s.pkey.fmtid == col.pkey.fmtid && s.pkey.pid == col.pkey.pid) == 0) {
				this.Collumns.Add(col);
				var column = col.ToNativeColumn(this.View == ShellViewStyle.Details);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_INSERTCOLUMN, this.Collumns.Count - 1, ref column);
				if (this.View != ShellViewStyle.Details) {
					this.AutosizeColumn(this.Collumns.Count - 1, -2);
				}
			}

			IntPtr headerhandle = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETHEADER, 0, 0);
			for (int i = 0; i < this.Collumns.Count; i++) {
				this.Collumns[i].SetSplitButton(headerhandle, i);
			}
		}

		public void RemoveAllCollumns() {
			for (int i = this.Collumns.ToArray().Count() - 1; i > 1; i--) {
				this.Collumns.RemoveAt(i);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_DELETECOLUMN, i, 0);
			}
		}

		public void SetSortCollumn(int colIndex, SortOrder Order, Boolean reverseOrder = true) {
			var selectedItems = this.SelectedItems.ToArray();
			if (colIndex == this.LastSortedColumnIndex && reverseOrder) {
				// Reverse the current sort direction for this column.
				this.LastSortOrder = this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
			} else {
				// Set the column number that is to be sorted; default to ascending.
				this.LastSortedColumnIndex = colIndex;
				this.LastSortOrder = Order;
			}
			if (Order == SortOrder.Ascending) {
				this.Items = this.Items.Where(w => this.ShowHidden ? true : !w.IsHidden).OrderByDescending(o => o.IsFolder).ThenBy(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToList();
			} else {
				this.Items = this.Items.Where(w => this.ShowHidden ? true : !w.IsHidden).OrderByDescending(o => o.IsFolder).ThenByDescending(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToList();
			}

			var i = 0;
			this.ItemsHashed = this.Items.Distinct().ToDictionary(k => k, el => i++, new ShellItemComparer());
			User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
			this.SetSortIcon(colIndex, Order);
			User32.SendMessage(this.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, this.LastSortedColumnIndex, 0);
			this.SelectItems(selectedItems);
		}

		/// <summary>
		/// Navigate to a folder, set it as the current folder and optionally save the folder's settings to the database.
		/// </summary>
		/// <param name="destination">The folder you want to navigate to.</param>
		/// <param name="SaveFolderSettings">Should the folder's settings be saved?</param>
		/// <param name="isInSameTab"></param>
		/// <param name="refresh">Should the List be Refreshed?</param>
		public void Navigate_Full(ShellItem destination, bool SaveFolderSettings, Boolean isInSameTab = false, bool refresh = false) {
			if (SaveFolderSettings) {
				SaveSettingsToDatabase(this.CurrentFolder);
			}

			if (destination == null || !destination.IsFolder) return;
			Navigate(destination, isInSameTab, refresh);
		}

		public void UnvalidateDirectory() {
			var newItems = this.CurrentFolder.Where(w => this.ShowHidden ? true : w.IsHidden == this.ShowHidden).ToArray();
			var removedItems = this.Items.Except(newItems, new ShellItemComparer());
			foreach (var obj in removedItems.ToArray()) {
				Items.Remove(obj);
				this.SetSortCollumn(this.LastSortedColumnIndex, this.LastSortOrder, false);
				if (this.IsGroupsEnabled) {
					this.SetGroupOrder(false);
				}
				//if (this.ItemUpdated != null)
				//	this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Deleted, obj, null, -1));
			}
			foreach (var obj in newItems) {
				F.Application.DoEvents();
				var existingItem = this.Items.SingleOrDefault(s => s.Equals(obj));
				if (existingItem == null) {
					if (obj.Extension.ToLowerInvariant() != ".tmp" && obj.Parent.Equals(this.CurrentFolder)) {
						var itemIndex = InsertNewItem(obj);
						//if (this.ItemUpdated != null)
						//	this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Created, obj, null, itemIndex));
					} else {
						var affectedItem = this.Items.SingleOrDefault(s => s.Equals(obj.Parent));
						if (affectedItem != null) {
							var index = this.Items.IndexOf(affectedItem);
							this.RefreshItem(index, true);
						}
					}
				}
			}
			newItems = null;
			removedItems = null;
			Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
		}





		/// <summary>
		/// Navigate to a folder.
		/// </summary>
		/// <param name="destination">The folder you want to navigate to.</param>
		/// <param name="isInSameTab"></param>
		/// <param name="refresh">Should the List be Refreshed?</param>
		private void Navigate(ShellItem destination, Boolean isInSameTab = false, bool refresh = false) {
			if (!refresh && Navigating != null) {
				Navigating(this, new NavigatingEventArgs(destination, isInSameTab));
			}

			Items.Clear();
			ItemsForSubitemsUpdate.Clear();
			waitingThumbnails.Clear();
			overlayQueue.Clear();
			shieldQueue.Clear();
			this._CuttedIndexes.Clear();
			this.SubItemValues.Clear();

			//Clear the LsitView
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMCOUNT, 0, 0);
			this.ItemForRename = -1;

			if (destination == null)
				return;

			MessageHandlerWindow.ReinitNotify(destination);
			this.fsw.Created -= fsw_Created;
			this.fsw.Deleted -= fsw_Deleted;
			this.fsw.Changed -= fsw_Changed;
			this.fsw.EnableRaisingEvents = false;
			if (destination.IsFileSystem || destination.IsNetworkPath) {

				try {
					this.fsw.Path = destination.ParsingName;
					this.fsw.EnableRaisingEvents = true;
					this.fsw.Created += fsw_Created;
					this.fsw.Deleted += fsw_Deleted;
					this.fsw.Changed += fsw_Changed;
				} catch {
					//In case of invalid path
				}
			}
			if (ToolTip == null)
				this.ToolTip = new ToolTip();

			var folderSettings = new FolderSettings();
			var isThereSettings = LoadSettingsFromDatabase(destination, out folderSettings);

			this.View = isThereSettings ? folderSettings.View : ShellViewStyle.Medium;
			if (folderSettings.View == ShellViewStyle.Details || folderSettings.View == ShellViewStyle.SmallIcon || folderSettings.View == ShellViewStyle.List)
				ResizeIcons(16);
			else {
				if (folderSettings.IconSize >= 16)
					this.ResizeIcons(folderSettings.IconSize);
			}

			if (isThereSettings) {
				if (folderSettings.Columns != null) {
					this.RemoveAllCollumns();
					foreach (var collumn in folderSettings.Columns.Elements()) {
						var theColumn = this.AllAvailableColumns.Where(w => w.ID == collumn.Attribute("ID").Value).Single();
						if (this.Collumns.Count(c => c.ID == theColumn.ID) == 0) {
							if (collumn.Attribute("Width").Value != "0") {
								theColumn.Width = Convert.ToInt32(collumn.Attribute("Width").Value);
							}
							this.Collumns.Add(theColumn);
							var column = theColumn.ToNativeColumn(folderSettings.View == ShellViewStyle.Details);
							User32.SendMessage(this.LVHandle, Interop.MSG.LVM_INSERTCOLUMN, this.Collumns.Count - 1, ref column);
							if (folderSettings.View != ShellViewStyle.Details) {
								this.AutosizeColumn(this.Collumns.Count - 1, -2);
							}
						} else {
							int colIndex = this.Collumns.IndexOf(this.Collumns.SingleOrDefault(s => s.ID == theColumn.ID));
							this.Collumns.RemoveAt(colIndex);
							User32.SendMessage(this.LVHandle, Interop.MSG.LVM_DELETECOLUMN, colIndex, 0);
							if (collumn.Attribute("Width").Value != "0") {
								theColumn.Width = Convert.ToInt32(collumn.Attribute("Width").Value);
							}
							this.Collumns.Add(theColumn);
							var column = theColumn.ToNativeColumn(folderSettings.View == ShellViewStyle.Details);
							User32.SendMessage(this.LVHandle, Interop.MSG.LVM_INSERTCOLUMN, this.Collumns.Count - 1, ref column);
							if (folderSettings.View != ShellViewStyle.Details) {
								this.AutosizeColumn(this.Collumns.Count - 1, -2);
							}
						}

					}
				}



			} else {
				this.RemoveAllCollumns();
				this.AddDefaultColumns(false, true);
			}
			IntPtr headerhandle = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETHEADER, 0, 0);
			for (int i = 0; i < this.Collumns.Count; i++) {
				this.Collumns[i].SetSplitButton(headerhandle, i);
			}
			if (this.View != ShellViewStyle.Details)
				AutosizeAllColumns(-2);

			//TODO: Figure out if the folder is actually sorted and we are not incorrectly setting the sort icon.
			this.SetSortIcon(folderSettings.SortColumn, folderSettings.SortOrder == SortOrder.None ? SortOrder.Ascending : folderSettings.SortOrder);



			int CurrentI = 0, LastI = 0;
			foreach (var Shell in destination) {
				F.Application.DoEvents();
				if (this.Items.Count > 0 && this.Items.Last().Parent != Shell.Parent) {
					break;
				}
				if (this.ShowHidden ? true : !Shell.IsHidden)
					this.Items.Add(Shell);
				CurrentI++;
				if (CurrentI - LastI >= (destination.IsSearchFolder ? 70 : 2000)) {
					F.Application.DoEvents();
					this.SetSortCollumn(isThereSettings ? folderSettings.SortColumn : 0, isThereSettings ? folderSettings.SortOrder : SortOrder.Ascending, false);
					if (destination.IsSearchFolder)
						Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
					LastI = CurrentI;
				}
			}
			resetEvent.Set();
			if (isThereSettings) {
				SetSortCollumn(folderSettings.SortColumn, folderSettings.SortOrder, false);
			} else if (destination.ParsingName.ToLowerInvariant() == KnownFolders.Computer.ParsingName.ToLowerInvariant()) {
				this.Items = this.Items.OrderBy(o => o.ParsingName).ToList();
			} else {
				this.Items = this.Items.OrderByDescending(o => o.IsFolder).ThenBy(o => o.DisplayName).ToList();
			}



			if (!isThereSettings) {
				var i = 0;
				this.ItemsHashed = this.Items.Distinct().ToDictionary(k => k, el => i++, new ShellItemComparer());
			}

			Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

			if (!isThereSettings) {
				this.LastSortedColumnIndex = 0;
				this.LastSortOrder = SortOrder.Ascending;
			}

			if (!isThereSettings)
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
			if (!String.IsNullOrEmpty(folderSettings.GroupCollumn)) {
				var colData = this.AllAvailableColumns.Where(w => w.ID == folderSettings.GroupCollumn).SingleOrDefault();
				if (colData != null) {
					this.EnableGroups();
					this.GenerateGroupsFromColumn(colData, folderSettings.GroupOrder == SortOrder.Descending);
				} else {
					this.DisableGroups();
				}
			} else {
				this.DisableGroups();
			}


			var NavArgs = new NavigatedEventArgs(destination, this.CurrentFolder, isInSameTab);
			this.CurrentFolder = destination;
			if (!refresh && Navigated != null) {
				Navigated(this, NavArgs);
			}

			this.Focus();
		}

		public void DisableGroups() {
			this.Groups.Clear();
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_REMOVEALLGROUPS, 0, 0);
			const int LVM_ENABLEGROUPVIEW = 0x1000 + 157;
			User32.SendMessage(this.LVHandle, LVM_ENABLEGROUPVIEW, 0, 0);
			const int LVM_SETOWNERDATACALLBACK = 0x10BB;
			User32.SendMessage(this.LVHandle, LVM_SETOWNERDATACALLBACK, 0, 0);
			this.LastGroupCollumn = null;
			this.LastGroupOrder = SortOrder.None;
		}

		public void EnableGroups() {
			IntPtr ptr = Marshal.GetComInterfaceForObject(new VirtualGrouping(this), typeof(IOwnerDataCallback));

			const int LVM_SETOWNERDATACALLBACK = 0x10BB;
			User32.SendMessage(this.LVHandle, LVM_SETOWNERDATACALLBACK, ptr, IntPtr.Zero);
			Marshal.Release(ptr);

			const int LVM_ENABLEGROUPVIEW = 0x1000 + 157;
			User32.SendMessage(this.LVHandle, LVM_ENABLEGROUPVIEW, 1, 0);
		}

		/// <summary>
		/// Generates Groups
		/// </summary>
		/// <param name="col">The column you want to group by</param>
		/// <param name="reversed">Reverse order (This needs to be explained better)</param>
		public void GenerateGroupsFromColumn(Collumns col, Boolean reversed = false) { //TODO: Document Better!
			if (col == null) return;
			int LVM_INSERTGROUP = 0x1000 + 145;
			this.Groups.Clear();
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_REMOVEALLGROUPS, 0, 0);
			if (col.CollumnType == typeof(String)) {
				var i = reversed ? 3 : 0;
				var testgrn = new ListViewGroupEx();
				testgrn.Items = this.Items.Where(w => w.DisplayName.ToUpperInvariant().First() >= Char.Parse("0") && w.DisplayName.ToUpperInvariant().First() <= Char.Parse("9")).ToArray();
				testgrn.Header = String.Format("0 - 9 ({0})", testgrn.Items.Count());
				testgrn.Index = reversed ? i-- : i++;
				this.Groups.Add(testgrn);

				var testgr = new ListViewGroupEx();
				testgr.Items = this.Items.Where(w => w.DisplayName.ToUpperInvariant().First() >= Char.Parse("A") && w.DisplayName.ToUpperInvariant().First() <= Char.Parse("H")).ToArray();
				testgr.Header = String.Format("A - H ({0})", testgr.Items.Count());
				testgr.Index = reversed ? i-- : i++;
				this.Groups.Add(testgr);

				var testgr2 = new ListViewGroupEx();
				testgr2.Items = this.Items.Where(w => w.DisplayName.ToUpperInvariant().First() >= Char.Parse("I") && w.DisplayName.ToUpperInvariant().First() <= Char.Parse("P")).ToArray();
				testgr2.Header = String.Format("I - P ({0})", testgr2.Items.Count());
				testgr2.Index = reversed ? i-- : i++;
				this.Groups.Add(testgr2);

				var testgr3 = new ListViewGroupEx();
				testgr3.Items = this.Items.Where(w => w.DisplayName.ToUpperInvariant().First() >= Char.Parse("Q") && w.DisplayName.ToUpperInvariant().First() <= Char.Parse("Z")).ToArray();
				testgr3.Header = String.Format("Q - Z ({0})", testgr3.Items.Count());
				testgr3.Index = reversed ? i-- : i++;
				this.Groups.Add(testgr3);

				if (reversed)
					this.Groups.Reverse();

				foreach (var group in this.Groups) {
					var nativeGroup = group.ToNativeListViewGroup();
					User32.SendMessage(this.LVHandle, LVM_INSERTGROUP, -1, ref nativeGroup);
				}
			} else if (col.CollumnType == typeof(long)) {
				var j = reversed ? 7 : 0;
				var uspec = new ListViewGroupEx();
				uspec.Items = this.Items.Where(w => w.IsFolder).ToArray();
				uspec.Header = String.Format("Unspecified ({0})", uspec.Items.Count());
				uspec.Index = reversed ? j-- : j++;
				this.Groups.Add(uspec);

				var testgrn = new ListViewGroupEx();
				testgrn.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) == 0 && !w.IsFolder).ToArray();
				testgrn.Header = String.Format("Empty ({0})", testgrn.Items.Count());
				testgrn.Index = reversed ? j-- : j++;
				this.Groups.Add(testgrn);

				var testgr = new ListViewGroupEx();
				testgr.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 0 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 10 * 1024).ToArray();
				testgr.Header = String.Format("Very Small ({0})", testgr.Items.Count());
				testgr.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr);

				var testgr2 = new ListViewGroupEx();
				testgr2.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 10 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 100 * 1024).ToArray();
				testgr2.Header = String.Format("Small ({0})", testgr2.Items.Count());
				testgr2.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr2);

				var testgr3 = new ListViewGroupEx();
				testgr3.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 100 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 1 * 1024 * 1024).ToArray();
				testgr3.Header = String.Format("Medium ({0})", testgr3.Items.Count());
				testgr3.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr3);

				var testgr4 = new ListViewGroupEx();
				testgr4.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 1 * 1024 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 16 * 1024 * 1024).ToArray();
				testgr4.Header = String.Format("Big ({0})", testgr4.Items.Count());
				testgr4.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr4);

				var testgr5 = new ListViewGroupEx();
				testgr5.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 16 * 1024 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 128 * 1024 * 1024).ToArray();
				testgr5.Header = String.Format("Huge ({0})", testgr5.Items.Count());
				testgr5.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr5);

				var testgr6 = new ListViewGroupEx();
				testgr6.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 128 * 1024 * 1024).ToArray();
				testgr6.Header = String.Format("Gigantic ({0})", testgr6.Items.Count());
				testgr6.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr6);

				if (reversed)
					this.Groups.Reverse();

				foreach (var group in this.Groups) {
					var nativeGroup = group.ToNativeListViewGroup();
					User32.SendMessage(this.LVHandle, LVM_INSERTGROUP, -1, ref nativeGroup);
				}
			} else {
				var groups = this.Items.GroupBy(k => k.GetPropertyValue(col.pkey, typeof(String)).Value, e => e).OrderBy(o => o.Key);
				var i = reversed ? groups.Count() - 1 : 0;
				foreach (var group in groups) {
					var groupItems = group.Select(s => s).ToArray();
					var gr = new ListViewGroupEx();
					gr.Items = groupItems;
					gr.Header = String.Format("{0} ({1})", group.Key.ToString(), groupItems.Count());
					gr.Index = reversed ? i-- : i++;
					this.Groups.Add(gr);
				}

				if (reversed) this.Groups.Reverse();
				foreach (var group in this.Groups) {
					var nativeGroup = group.ToNativeListViewGroup();
					User32.SendMessage(this.LVHandle, LVM_INSERTGROUP, -1, ref nativeGroup);
				}
			}

			this.LastGroupCollumn = col;
			this.LastGroupOrder = reversed ? SortOrder.Descending : SortOrder.Ascending;
			this.SetSortIcon(this.Collumns.IndexOf(col), this.LastGroupOrder);
			RefreshItemsCountInternal();
		}

		/// <summary>
		/// Sets the Sort order of the Groups
		/// </summary>
		/// <param name="reverse">Reverse the Current Sort Order?</param>
		public void SetGroupOrder(Boolean reverse = true) {
			this.GenerateGroupsFromColumn(this.LastGroupCollumn, reverse ? this.LastGroupOrder == SortOrder.Ascending : false);
		}

		[DebuggerStepThrough]
		public ShellItem GetFirstSelectedItem() {
			var lvi = new LVITEMINDEX() { iItem = -1, iGroup = 0 };
			User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
			if (lvi.iItem == -1 || this.Items.Count < lvi.iItem) return null;
			return this.Items[lvi.iItem];
		}

		public int GetFirstSelectedItemIndex() {
			var lvi = new LVITEMINDEX() { iItem = -1, iGroup = 0 };
			User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
			if (lvi.iItem == -1) return -1;
			return lvi.iItem;
		}


		private bool ThreadRun_Helper(SyncQueue<int> queue, bool useComplexCheck, ref int index) {
			index = queue.Dequeue();
			var itemBounds = new User32.RECT();
			var lvi = new LVITEMINDEX() { iItem = index, iGroup = this.GetGroupIndex(index) };
			User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
			var r = new Rectangle(itemBounds.Left, itemBounds.Top, itemBounds.Right - itemBounds.Left, itemBounds.Bottom - itemBounds.Top);

			if (useComplexCheck)
				return index < Items.Count && r.IntersectsWith(this.ClientRectangle);
			else
				return r.IntersectsWith(this.ClientRectangle);
		}
		public void _OverlaysLoadingThreadRun() {
			while (true) {
				F.Application.DoEvents();
				resetEvent.WaitOne();
				try {
					int index = 0;
					if (!ThreadRun_Helper(overlayQueue, false, ref index)) continue;
					var shoTemp = Items[index];
					ShellItem sho = !(shoTemp.IsNetDrive || shoTemp.IsNetworkPath) && shoTemp.ParsingName.StartsWith("::") ? shoTemp : ShellItem.ToShellParsingName(shoTemp.ParsingName);

					int overlayIndex = 0;
					small.GetIconIndexWithOverlay(sho.Pidl, out overlayIndex);
					shoTemp.OverlayIconIndex = overlayIndex;
					if (overlayIndex > 0)
						RedrawItem(index);

				} catch (Exception) {
					F.Application.DoEvents();
				}
			}
		}

		public void _IconsLoadingThreadRun() {
			while (true) {
				resetEvent.WaitOne();
				try {
					int index = 0;
					if (!ThreadRun_Helper(waitingThumbnails, false, ref index)) continue;
					var sho = Items[index];
					if (!sho.IsIconLoaded) {
						ShellItem temp = !(sho.IsNetDrive || sho.IsNetworkPath) && sho.ParsingName.StartsWith("::") ? sho : new ShellItem(sho.ParsingName);

						var icon = temp.Thumbnail.GetHBitmap(IconSize);

						var shieldOverlay = 0;
						if (sho.IsShielded == -1) {
							if ((temp.GetShield() & IExtractIconPWFlags.GIL_SHIELD) != 0) {
								shieldOverlay = ShieldIconIndex;
							}

							sho.IsShielded = shieldOverlay;
						}
						if (icon != IntPtr.Zero || shieldOverlay > 0) {
							sho.IsIconLoaded = true;
							Gdi32.DeleteObject(icon);
							this.RedrawItem(index);
						}
					}


				} catch {
					F.Application.DoEvents();
				}
			}
		}

		public void _IconCacheLoadingThreadRun() {
			while (true) {
				resetEvent.WaitOne();
				try {
					int index = 0;
					if (!ThreadRun_Helper(ThumbnailsForCacheLoad, true, ref index)) continue;
					var sho = Items[index];
					var result = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.Default);
					sho.IsThumbnailLoaded = true;
					sho.IsNeedRefreshing = false;
					if (result != null) {
						if ((result.Width > result.Height && result.Width != IconSize) || (result.Width < result.Height && result.Height != IconSize) || (result.Width == result.Height && result.Width != IconSize)) {
							sho.IsOnlyLowQuality = true;
						} else {
							sho.IsOnlyLowQuality = false;
						}
						F.Application.DoEvents();
						this.RefreshItem(index);
						result.Dispose();
					}

				} catch {
					F.Application.DoEvents();
				}
			}
		}

		public void _UpdateSubitemValuesThreadRun() {
			while (true) {
				resetEvent.WaitOne();
				Thread.Sleep(1);
				var index = ItemsForSubitemsUpdate.Dequeue();
				try {
					if (User32.SendMessage(this.LVHandle, Interop.MSG.LVM_ISITEMVISIBLE, index.Item1, 0) != IntPtr.Zero) {
						var currentItem = Items[index.Item1];
						ShellItem temp = currentItem.ParsingName.StartsWith("::") && currentItem.IsNetDrive ? currentItem : new ShellItem(currentItem.ParsingName);
						int hash = currentItem.GetHashCode();
						var isi2 = (IShellItem2)temp.ComInterface;
						var pvar = new PropVariant();
						var pk = index.Item3;
						var guid = new Guid(InterfaceGuids.IPropertyStore);
						IPropertyStore propStore = null;
						isi2.GetPropertyStore(GetPropertyStoreOptions.Default, ref guid, out propStore);
						if (propStore != null && propStore.GetValue(ref pk, pvar) == HResult.S_OK) {
							if (!SubItemValues.Any(c => c.Item1 == hash && c.Item2.fmtid == pk.fmtid && c.Item2.pid == pk.pid)) {
								SubItemValues.Add(new Tuple<int, PROPERTYKEY, object>(hash, pk, pvar.Value));
								this.RedrawItem(index.Item1);
							}
							pvar.Dispose();
						}
					}
				} catch {
					F.Application.DoEvents();
				}
			}
		}

		/// <summary> Runs an application as an another user. </summary>
		/// <param name="ExePath">  The path of the application. </param>
		/// <param name="username"> The path of the username to use. </param>
		public static void RunExeAsAnotherUser(string ExePath, string username) {
			Process.Start(new ProcessStartInfo {
				FileName = ExePath,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = String.Format("/env /user:{0} \"{1}\"", username, ExePath),
			});
		}


		public static void StartCompartabilityWizzard() {
			Process.Start("msdt.exe", "-id PCWDiagnostic");
		}


		public void CleanupDrive() {
			string DriveLetter = "";
			if (SelectedItems.Count > 0)
				DriveLetter = Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName) ? SelectedItems[0].ParsingName : this.CurrentFolder.ParsingName;
			else
				DriveLetter = this.CurrentFolder.ParsingName;

			Process.Start("Cleanmgr.exe", "/d" + DriveLetter.Replace(":\\", ""));
		}


		public void ScrollToTop() {
			var res = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_ENSUREVISISBLE, 0, 0);
		}
		public string CreateNewFolder(string name = "New Folder") {
			int suffix = 0;
			string endname = name;

			do {
				//TODO: Check
				if (this.CurrentFolder.Parent == null) {
					endname = String.Format("{0}\\" + name + " ({1})", this.CurrentFolder.ParsingName, ++suffix);
				} else if (this.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
					ShellLibrary lib = ShellLibrary.Load(this.CurrentFolder.DisplayName, true);
					endname = String.Format("{0}\\" + name + " ({1})", lib.DefaultSaveFolder, ++suffix);
					lib.Close();
				} else {
					endname = String.Format("{0}\\" + name + " ({1})", this.CurrentFolder.ParsingName, ++suffix);
				}

			} while (Directory.Exists(endname) || File.Exists(endname));

			switch (Shell32.SHCreateDirectory(IntPtr.Zero, endname)) {
				case ERROR.FILE_EXISTS:
				case ERROR.ALREADY_EXISTS:
					throw new IOException("The directory already exists");
				case ERROR.BAD_PATHNAME:
					throw new IOException("Bad pathname");
				case ERROR.FILENAME_EXCED_RANGE:
					throw new IOException("The filename is too long");
			}
			return endname;
		}

		public ShellLibrary CreateNewLibrary() { return CreateNewLibrary("New Library"); }

		public ShellLibrary CreateNewLibrary(string name) {
			string endname = name;
			int suffix = 0;
			ShellLibrary lib = null;
			try {
				lib = ShellLibrary.Load(endname, true);
			} catch {
			}
			if (lib != null) {
				do {
					endname = String.Format(name + "({0})", ++suffix);
					try {
						lib = ShellLibrary.Load(endname, true);
					} catch {
						lib = null;
					}
				} while (lib != null);
			}

			return new ShellLibrary(endname, false);
		}


		private void SetLVBackgroundImage(Bitmap bitmap) {
			Helpers.SetListViewBackgroundImage(this.LVHandle, bitmap);
		}

		public void SetFolderIcon(string wszPath, string wszExpandedIconPath, int iIcon) {
			HResult hr;
			var fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS() { iIconIndex = iIcon, cchIconFile = 0, dwMask = Shell32.FCSM_ICONFILE };
			fcs.dwSize = (uint)Marshal.SizeOf(fcs);
			fcs.pszIconFile = wszExpandedIconPath.Replace(@"\\", @"\");

			// Set the folder icon
			hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath.Replace(@"\\", @"\"), Shell32.FCS_FORCEWRITE);

			if (hr == HResult.S_OK) {
				// Update the icon cache
				var sfi = new SHFILEINFO();
				var res = Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(wszPath), 0, out sfi, (int)Marshal.SizeOf(sfi), SHGFI.IconLocation);
				int iIconIndex = Shell32.Shell_GetCachedImageIndex(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0);
				Shell32.SHUpdateImage(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0, iIconIndex);
				//RefreshExplorer();
				Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEIMAGE,
				Shell32.HChangeNotifyFlags.SHCNF_DWORD | Shell32.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, IntPtr.Zero, (IntPtr)sfi.iIcon);
			}

			Items[this.GetFirstSelectedItemIndex()] = new ShellItem(wszPath);
			this.RefreshItem(this.SelectedIndexes[0]);
		}

		public HResult ClearFolderIcon(string wszPath) {
			HResult hr;
			var fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS() { dwMask = Shell32.FCSM_ICONFILE };
			fcs.dwSize = (uint)Marshal.SizeOf(fcs);

			hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath, Shell32.FCS_FORCEWRITE);
			if (hr == HResult.S_OK) {
				// Update the icon cache
				var sfi = new SHFILEINFO();
				Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(wszPath.Replace(@"\\", @"\")), 0, out sfi, (int)Marshal.SizeOf(sfi), SHGFI.IconLocation);
				int iIconIndex = Shell32.Shell_GetCachedImageIndex(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0);
				Shell32.SHUpdateImage(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0, iIconIndex);
				Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEIMAGE,
				Shell32.HChangeNotifyFlags.SHCNF_DWORD | Shell32.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, IntPtr.Zero, (IntPtr)sfi.iIcon);
			}
			Items[this.SelectedIndexes[0]] = new ShellItem(wszPath);
			//this.UpdateItem(this.SelectedIndexes[0]);
			this.RefreshItem(this.SelectedIndexes[0]);
			return hr;
		}

		public void DefragDrive() {
			string DriveLetter = "";
			if (SelectedItems.Any()) {
				if (Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName))
					DriveLetter = SelectedItems[0].ParsingName;
				else
					DriveLetter = this.CurrentFolder.ParsingName;
			} else {
				DriveLetter = this.CurrentFolder.ParsingName;
			}

			Process.Start(Path.Combine(Environment.SystemDirectory, "dfrgui.exe"), "/u /v " + DriveLetter.Replace("\\", ""));
		}

		public void DeSelectAllItems() {
			var item = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = 0 };
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, -1, ref item);
			this.Focus();
		}

		private void DeselectItemByIndex(int index) {
			LVITEM item = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = 0 };
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, index, ref item);
		}

		/*
		[Obsolete("Never Used", true)]
		private void RemoveDropHighLightItemByIndex(int index) {
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED | LVIS.LVIS_DROPHILITED;
			item.state = 0;
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, index, ref item);
		}
		*/

		public Boolean IsFocusAllowed = true;

		/// <summary> Gives the ShellListView focus </summary>
		public void Focus(Boolean isActiveCheck = true) {
			try {
				if (User32.GetForegroundWindow() != this.LVHandle) {
					this.Invoke(new MethodInvoker(() => {
						var mainWin = System.Windows.Application.Current.MainWindow;
						if (mainWin.IsActive || !isActiveCheck) {
							if (IsFocusAllowed && this.Bounds.Contains(Cursor.Position)) {
								User32.SetFocus(this.LVHandle); //var res = 
								//this._IsInRenameMode = false;
							}
						}
					}));
				}
			}
				//On Exception do nothing (usually it happens on app exit)
			catch { }

			//}
		}

		public void FormatDrive(IntPtr handle) {
			string DriveLetter =
				SelectedItems.Count > 0 ?
				DriveLetter = Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName) ? SelectedItems[0].ParsingName : this.CurrentFolder.ParsingName
				:
				DriveLetter = this.CurrentFolder.ParsingName;

			Shell32.FormatDrive(handle, DriveLetter);
		}

		public int GetSelectedCount() {
			return (int)User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETSELECTEDCOUNT, 0, 0);
		}

		public void InvertSelection() {
			int itemCount = (int)User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMCOUNT, 0, 0);

			for (int n = 0; n < itemCount; ++n) {
				var state = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMSTATE, n, LVIS.LVIS_SELECTED);
				var item_new = new LVITEM() {
					mask = LVIF.LVIF_STATE,
					stateMask = LVIS.LVIS_SELECTED,
					state = (state & LVIS.LVIS_SELECTED) == LVIS.LVIS_SELECTED ? 0 : LVIS.LVIS_SELECTED
				};

				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, n, ref item_new);
			}
			this.Focus();
		}

		public void AutosizeAllColumns(int autosizeParam) {
			this.SuspendLayout();
			for (int i = 0; i < this.Collumns.Count; i++) {
				AutosizeColumn(i, autosizeParam);
			}
			this.ResumeLayout();
		}

		#endregion Public Methods

		#region Private Methods

		private int GetGroupIndex(int itemIndex) {
			if (itemIndex == -1 || itemIndex >= this.Items.Count) return 0;
			var item = this.Items[itemIndex];
			var Found = this.Groups.FirstOrDefault(x => x.Items.Contains(item));
			return Found == null ? 0 : Found.Index;
		}

		/*
		private static BitmapFrame CreateResizedImage(IntPtr hBitmap, int width, int height, int margin) {
			var source = Imaging.CreateBitmapSourceFromHBitmap(
															hBitmap,
															IntPtr.Zero,
															System.Windows.Int32Rect.Empty,
															BitmapSizeOptions.FromEmptyOptions()).Clone();
			Gdi32.DeleteObject(hBitmap);

			var group = new DrawingGroup();
			RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.Fant);
			group.Children.Add(new ImageDrawing(source, new Rect(0, 0, width, height)));
			var targetVisual = new DrawingVisual();
			var targetContext = targetVisual.RenderOpen();
			targetContext.DrawDrawing(group);
			var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
			targetContext.Close();
			target.Render(targetVisual);
			return BitmapFrame.Create(target);
		}
		*/

		/// <summary>
		/// Returns the index of the first item whose display name starts with the search string.
		/// </summary>
		/// <param name="search">     The string for which to search for. </param>
		/// <param name="startindex">
		/// The index from which to start searching. Enter '0' to search all items.
		/// </param>
		/// <returns> The index of an item within the list view. </returns>
		private int GetFirstIndexOf(string search, int startindex) {
			int i = startindex;
			while (true) {
				if (i >= Items.Count) {
					return -1;
				} else if (Items[i].GetDisplayName(SIGDN.NORMALDISPLAY).ToUpperInvariant().StartsWith(search.ToUpperInvariant())) {
					return i;
				} else {
					i++;
				}
			}
		}

		private void StartProcessInCurrentDirectory(ShellItem item) {
			Process.Start(new ProcessStartInfo() {
				FileName = item.ParsingName,
				WorkingDirectory = this.CurrentFolder.ParsingName
			});
		}

		private void RedrawWindow() {
			//User32.RedrawWindow(this.LVHandle, IntPtr.Zero, IntPtr.Zero,
			//										 0x0001/*RDW_INVALIDATE*/);
			User32.InvalidateRect(this.LVHandle, IntPtr.Zero, false);
		}

		private void RedrawWindow(User32.RECT rect) {
			User32.InvalidateRect(this.LVHandle, ref rect, false);
		}

		/*
		internal void OnGotFocus() {
			if (GotFocus != null) {
				GotFocus(this, EventArgs.Empty);
			}
		}
		*/

		/*
		internal void OnLostFocus() {
			if (LostFocus != null) {
				LostFocus(this, EventArgs.Empty);
			}
		}
		*/

		internal void OnSelectionChanged() {
			if (SelectionChanged != null) {


				SelectionChanged(this, EventArgs.Empty);
			}
		}

		internal void OnItemMiddleClick() {
			if (ItemMiddleClick != null) {
				var row = -1;
				var column = -1;
				this.HitTest(this.PointToClient(Cursor.Position), out row, out column);
				if (row != -1 && this.Items[row].IsFolder) {
					ItemMiddleClick.Invoke(this, new NavigatedEventArgs(this.Items[row], this.Items[row]));
				}
			}
		}

		private new void ResumeLayout() {
			User32.SendMessage(this.LVHandle, (int)WM.WM_SETREDRAW, 1, 0);
		}

		private new void SuspendLayout() {
			User32.SendMessage(this.LVHandle, (int)WM.WM_SETREDRAW, 0, 0);
		}

		private void RefreshItemsCountInternal() {
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMCOUNT, 0, 0);
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
		}

		private string GetFilePropertiesString(Object value) {
			var valueFA = (FileAttributes)value;
			var isArhive = ((valueFA & FileAttributes.Archive) == FileAttributes.Archive);
			var isDirectory = ((valueFA & FileAttributes.Directory) == FileAttributes.Directory);
			var isHidden = ((valueFA & FileAttributes.Hidden) == FileAttributes.Hidden);
			var isReadOnly = ((valueFA & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
			var isSystem = ((valueFA & FileAttributes.System) == FileAttributes.System);
			var isTemp = ((valueFA & FileAttributes.Temporary) == FileAttributes.Temporary);
			var resultString = String.Empty;
			if (isArhive) {
				resultString += "A";
			} else {
				resultString += "-";
			}
			if (isDirectory) {
				resultString += "D";
			} else {
				resultString += "-";
			}
			if (isHidden) {
				resultString += "H";
			} else {
				resultString += "-";
			}
			if (isReadOnly) {
				resultString += "R";
			} else {
				resultString += "-";
			}
			if (isSystem) {
				resultString += "S";
			} else {
				resultString += "-";
			}
			if (isTemp) {
				resultString += "T";
			} else {
				resultString += "-";
			}
			return resultString;
		}


		/// <summary>
		/// This is only to be used in SetSortCollumn(...)
		/// </summary>
		/// <param name="columnIndex"></param>
		/// <param name="order"></param>
		private void SetSortIcon(int columnIndex, SortOrder order) {
			//TODO: Consider Merging this into SetSortCollumn(...)

			IntPtr columnHeader = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
			for (int columnNumber = 0; columnNumber <= this.Collumns.Count - 1; columnNumber++) {
				var item = new HDITEM {
					mask = HDITEM.Mask.Format
				};

				if (User32.SendMessage(columnHeader, MSG.HDM_GETITEM, columnNumber, ref item) == IntPtr.Zero) {
					throw new Win32Exception();
				}

				if (order != SortOrder.None && columnNumber == columnIndex) {
					switch (order) {
						case SortOrder.Ascending:
							item.fmt &= ~HDITEM.Format.SortDown;
							item.fmt |= HDITEM.Format.SortUp;
							break;
						case SortOrder.Descending:
							item.fmt &= ~HDITEM.Format.SortUp;
							item.fmt |= HDITEM.Format.SortDown;
							break;
					}
				} else {
					item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
				}

				if (User32.SendMessage(columnHeader, MSG.HDM_SETITEM, columnNumber, ref item) == IntPtr.Zero) {
					throw new Win32Exception();
				}
			}
		}

		private void AutosizeColumn(int index, int autosizeStyle) {
			User32.SendMessage(this.LVHandle, LVM.SETCOLUMNWIDTH, index, autosizeStyle);
		}

		#endregion Private Methods

		#region Database

		private Boolean LoadSettingsFromDatabase(ShellItem directory, out FolderSettings folderSettings) {
			var result = false;
			var folderSetting = new FolderSettings();
			try {
				var m_dbConnection = new SQLite.SQLiteConnection("Data Source=Settings.sqlite;Version=3;");
				m_dbConnection.Open();

				var command1 = new SQLite.SQLiteCommand("SELECT * FROM foldersettings WHERE Path=@0", m_dbConnection);
				command1.Parameters.AddWithValue("0", directory.ParsingName);

				var Reader = command1.ExecuteReader();
				if (Reader.Read()) {
					var Values = Reader.GetValues();
					if (Values.Count > 0) {
						result = true;
						var view = Values.GetValues("View").FirstOrDefault();
						var iconSize = Values.GetValues("IconSize").FirstOrDefault();
						var lastSortedColumnIndex = Values.GetValues("LastSortedColumn").FirstOrDefault();
						var lastSortOrder = Values.GetValues("LastSortOrder").FirstOrDefault();
						var lastGroupedColumnId = Values.GetValues("LastGroupCollumn").FirstOrDefault();
						var lastGroupoupOrder = Values.GetValues("LastGroupOrder").FirstOrDefault();
						if (view != null) {
							folderSetting.View = (ShellViewStyle)Enum.Parse(typeof(ShellViewStyle), view);
						}
						if (lastSortedColumnIndex != null) {
							folderSetting.SortColumn = Int32.Parse(lastSortedColumnIndex);
							folderSetting.SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), lastSortOrder);
						}

						folderSetting.GroupCollumn = lastGroupedColumnId;
						folderSetting.GroupOrder = lastGroupoupOrder == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending; ;


						var collumns = Values.GetValues("Columns").FirstOrDefault();

						folderSetting.Columns = collumns != null ? XElement.Parse(collumns) : null;
						if (String.IsNullOrEmpty(iconSize)) {
							folderSetting.IconSize = 48;
						} else {
							folderSetting.IconSize = Int32.Parse(iconSize);
						}

					}
				}

				Reader.Close();
			} catch (Exception) {
			}
			folderSettings = folderSetting;
			return result;
		}



		public void SaveSettingsToDatabase(ShellItem destination) {
			if (CurrentFolder == null) return;
			if (!CurrentFolder.IsFolder) return;

			var m_dbConnection = new SQLite.SQLiteConnection("Data Source=Settings.sqlite;Version=3;");
			m_dbConnection.Open();

			var command1 = new SQLite.SQLiteCommand("SELECT * FROM foldersettings WHERE Path=@Path", m_dbConnection);
			command1.Parameters.AddWithValue("Path", destination.ParsingName);
			var Reader = command1.ExecuteReader();
			var sql = Reader.Read() ?
									@"UPDATE foldersettings 
										SET Path = @Path, LastSortOrder = @LastSortOrder, LastGroupOrder = @LastGroupOrder, LastGroupCollumn = @LastGroupCollumn, 
												View = @View, LastSortedColumn = @LastSortedColumn, Columns = @Columns, IconSize = @IconSize
									WHERE Path = @Path"
									:
																 @"INSERT into foldersettings (Path, LastSortOrder, LastGroupOrder, LastGroupCollumn, View, LastSortedColumn, Columns, IconSize)
                                                                               VALUES (@Path, @LastSortOrder, @LastGroupOrder, @LastGroupCollumn, @View, @LastSortedColumn, @Columns, @IconSize)";


			int[] orders = new int[this.Collumns.Count];
			User32.SendMessage(this.LVHandle, (uint)Interop.MSG.LVM_GETCOLUMNORDERARRAY, orders.Length, orders);

			var Columns_XML = new System.Xml.Linq.XElement("Columns");
			foreach (var index in orders) {
				var collumn = this.Collumns[index];
				var width = (int)User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETCOLUMNWIDTH, index, 0);
				var XML = new System.Xml.Linq.XElement("Column");
				XML.Add(new System.Xml.Linq.XAttribute("ID", collumn.ID == null ? "" : collumn.ID.ToString()));
				XML.Add(new System.Xml.Linq.XAttribute("Width", collumn.ID == null ? "" : width.ToString()));
				Columns_XML.Add(XML);
			}

			var Values = new Dictionary<string, string>() {
				{ "Path", destination.ParsingName },
				{ "LastSortOrder", LastSortOrder.ToString() },
				{ "LastGroupOrder", LastGroupOrder.ToString() },
				{ "LastGroupCollumn", LastGroupCollumn == null ? null : LastGroupCollumn.ID },
				{ "View", View.ToString() },
				{ "LastSortedColumn", LastSortedColumnIndex.ToString() },
        { "Columns", Columns_XML.ToString()},
        { "IconSize", this.IconSize.ToString() }
			};

			var command2 = new SQLite.SQLiteCommand(sql, m_dbConnection);
			foreach (var item in Values) {
				command2.Parameters.AddWithValue(item.Key, item.Value);
			}
			command2.ExecuteNonQuery();
			Reader.Close();
			m_dbConnection.Close();
		}

		#endregion Database

		#region Rename File
    public bool IsRenameInProgress = false;
		public void FileNameChangeAttempt(string NewName, bool Cancel) {
			if (ItemForRealName_IsAny && this.Items != null && this.Items.Count >= ItemForRename) {
				var item = this.Items[ItemForRename];
				if (!Cancel) {


					if (item.DisplayName != NewName) {
            IsRenameInProgress = true;
						RenameShellItem(item.ComInterface, NewName);
					}
				}
				this.RedrawWindow();
			}
			ItemForRename = -1;
			this.IsFocusAllowed = true;
		}

		/*
		private void BeginLabelEdit(int itemIndex) {
			//this._IsInRenameMode = true;
			this.IsFocusAllowed = false;
			this.ItemForRename = itemIndex;
			if (this.BeginItemLabelEdit != null) {
				this.BeginItemLabelEdit.Invoke(this, new RenameEventArgs(itemIndex));
			}

			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_UPDATE, itemIndex, 0);
			RedrawWindow();
		}
		*/

		private void EndLabelEdit(Boolean isCancel = false) {
			if (this.EndItemLabelEdit != null) {
				this.EndItemLabelEdit.Invoke(this, isCancel);
				this.ItemForRename = -1;
			}
		}

		#endregion


		public void OpenShareUI() {
			HResult hr = Shell32.ShowShareFolderUI(this.Handle, Marshal.StringToHGlobalAuto(this.GetFirstSelectedItem().ParsingName.Replace(@"\\", @"\")));
		}

		public void MapDrive(IntPtr intPtr, string path) {
			Shell32.MapDrive(intPtr, path);
		}

		public void DisconnectDrive(IntPtr handle, int type) {
			Shell32.WNetDisconnectDialog(handle, type);
		}

		#region IDropSource Members

		public new HResult QueryContinueDrag(bool fEscapePressed, int grfKeyState) {
			if (grfKeyState == 0) {
				return HResult.DRAGDROP_S_DROP;
			} else {
				return HResult.S_OK;
			}
			if (fEscapePressed)
				return HResult.DRAGDROP_S_CANCEL;
			return HResult.DRAGDROP_S_DROP;
		}

		public new HResult GiveFeedback(int dwEffect) {
			//dwEffect = (int)F.DragDropEffects.All;
			System.Windows.Forms.DataObject doo = new F.DataObject(dataObject);
			var obj = doo.GetData("DropDescription");
			if (obj != null) {
				var uu = 1;
			}
			return HResult.S_OK;
		}

		#endregion
	}
}