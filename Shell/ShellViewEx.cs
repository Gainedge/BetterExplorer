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
using Microsoft.Win32;
using SQLite = System.Data.SQLite;
using F = System.Windows.Forms;
using BExplorer.Shell.Interop;
using System.Xml.Linq;

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
		/// the seleted item appearing above.
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

	public enum ItemUpdateType {
		Renamed,
		Created,
		Deleted,
		Updated
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

		/// <summary> Occurs when the control gains focus </summary>
		public new event EventHandler GotFocus;

		public event EventHandler<ItemDisplayedEventArgs> ItemDisplayed;

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

		/// <summary> Raised whenever a key is pressed </summary>
		public new event KeyEventHandler KeyDown;

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

		private void OnViewChanged(ViewChangedEventArgs e) {
			if (ViewStyleChanged != null) {
				ViewStyleChanged(this, e);
			}
		}

		private void OnNavigated(NavigatedEventArgs e) {
			if (Navigated != null) {
				Navigated(this, e);
			}
		}

		private void OnItemDisplayed(ShellItem item, int index) {
			if (ItemDisplayed != null) {
				ItemDisplayed(this, new ItemDisplayedEventArgs(item, index));
			}
		}

		/// <summary> Triggers the Navigating event. </summary>
		[DebuggerStepThrough()]
		public virtual void OnNavigating(NavigatingEventArgs ea) {
			if (Navigating != null)
				Navigating(this, ea);
		}

		#endregion Event Handler

		#region Public Members

		public ToolTip ToolTip;
		public List<Collumns> AllAvailableColumns = new List<Collumns>();
		public List<Collumns> Collumns = new List<Collumns>();
		public List<ListViewGroupEx> Groups = new List<ListViewGroupEx>();
		public ShellNotifications Notifications = new ShellNotifications();

		/*
		[Obsolete("Convert this into a method", false)]
		public String NewName { private get; set; }
		*/

		[Obsolete("Try to remove this!!")]
		private int ItemForRename { get; set; } //TODO: Find out why this is used in so many places and try to stop that!!!!!
		private bool ItemForRealName_IsAny { get { return ItemForRename != -1; } }


		public bool IsRenameNeeded { get; set; }

		//public Boolean IsGroupsEnabled { get; private set; }
		public Boolean IsGroupsEnabled { get { return LastGroupCollumn != null; } }



		/// <summary> Returns the key jump string as it currently is.</summary>
		public string KeyJumpString { get; private set; }
		//public string KeyJumpString { get { return _keyjumpstr; } }

		[Obsolete("Not Used", true)]
		public List<string> RecommendedPrograms(string ext) {
			List<string> progs = new List<string>();

			using (RegistryKey rk = Registry.ClassesRoot.OpenSubKey(ext + @"\OpenWithList")) {
				if (rk != null) progs.AddRange(rk.GetSubKeyNames());
				//foreach (string item in rk.GetSubKeyNames()) progs.Add(item);
			}

			using (RegistryKey rk = Registry.ClassesRoot.OpenSubKey(ext + @"\OpenWithProgids")) {
				if (rk != null) progs.AddRange(rk.GetValueNames());
				//foreach (string item in rk.GetValueNames()) progs.Add(item);				
			}

			return progs;
		}

		/*
		/// <summary>
		/// Returns the index of the first item whose display name starts with the search string.
		/// </summary>
		/// <param name="search"> The string for which to search for. </param>
		/// <returns> The index of an item within the list view. </returns>
		[Obsolete("Never Used", true)]
		private int GetFirstIndexOf(string search) { return GetFirstIndexOf(search, 0); }
		*/


		/*
		/// <summary>
		/// Gets a value indicating whether a previous page in navigation history is available,
		/// which allows the <see cref="ShellView.NavigateBack" /> method to succeed.
		/// </summary>
		[Browsable(false)]
		[Obsolete("Not Used", true)]
		public bool CanNavigateBack { get { return History.CanNavigateBack; } }
		*/

		/*
		/// <summary>
		/// Gets a value indicating whether a subsequent page in navigation history is available,
		/// which allows the <see cref="ShellView.NavigateForward" /> method to succeed.
		/// </summary>
		[Browsable(false)]
		[Obsolete("Not Used", true)]
		public bool CanNavigateForward { get { return History.CanNavigateForward; } }
		*/

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


		/*
		///<summary> Gets the <see cref="ShellView" />'s navigation history. </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected ShellHistory History { get; private set; }
		*/

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

					//: Try removing this Try Catch!
					/*
					try {
						selItems.Add(this.Items[index]);

						//DraggedItemIndexes.Add(index);
					}
					catch (Exception) {
						this.SelectedIndexes.Remove(index);
					}
					*/
				}
				return selItems;
			}
		}

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

		public Boolean ShowCheckboxes {
			get { return _showCheckBoxes; }
			set {
				if (value) {
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.CheckBoxes, (int)ListViewExtendedStyles.CheckBoxes);
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT);
				}
				else {
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.CheckBoxes, 0);
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, 0);
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
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(256);
						iconsize = 256;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.LargeIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(96);
						iconsize = 96;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.Medium:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(48);
						iconsize = 48;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.SmallIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_SMALLICON, 0);
						ResizeIcons(16);
						iconsize = 16;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.List:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_LIST, 0);
						ResizeIcons(16);
						iconsize = 16;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.Details:
						this.UpdateColsInView(true);
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_DETAILS, 0);
						ResizeIcons(16);
						iconsize = 16;
						RefreshItemsCountInternal();
						break;

					case ShellViewStyle.Thumbnail:
						break;

					case ShellViewStyle.Tile:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_TILE, 0);
						ResizeIcons(48);
						iconsize = 48;
						RefreshItemsCountInternal();
						//LVTILEVIEWINFO tvi = new LVTILEVIEWINFO();
						//tvi.cbSize = Marshal.SizeOf(typeof(LVTILEVIEWINFO));
						//tvi.dwMask = (int)LVTVIM.LVTVIM_COLUMNS | (int)LVTVIM.LVTVIM_TILESIZE;
						//tvi.dwFlags = (int)LVTVIF.LVTVIF_AUTOSIZE;
						//tvi.cLines = 4;
						//var a = User32.SendMessage(this.LVHandle, (int)BExplorer.Shell.Interop.MSG.LVM_SETTILEVIEWINFO, 0, tvi);
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
				OnViewChanged(new ViewChangedEventArgs(value, iconsize));
			}
		}

		public int CurrentRefreshedItemIndex = -1;

		//public Boolean Cancel = false;
		#endregion Public Members

		#region Private Members
		//private Boolean _IsNavigationInProgress = false;

		//[Obsolete("Never Actually Used")]
		//private Boolean _IsInRenameMode = false;


		//private ShellHistory m_History;
		//private int _iconSize;
		private Boolean _showCheckBoxes = false;

		private Boolean _ShowHidden;

		private F.Timer _ResetTimer = new F.Timer();
		private Thread MaintenanceThread;
		private List<Int32> DraggedItemIndexes = new List<int>();
		private F.Timer _KeyJumpTimer = new F.Timer();
		//private string _keyjumpstr = "";
		private ShellItem _kpreselitem = null;
		private LVIS _IsDragSelect = 0;
		private BackgroundWorker bw = new BackgroundWorker();
		//private ConcurrentDictionary<int, Bitmap> cache = new ConcurrentDictionary<int, Bitmap>();
		private Thread _IconCacheLoadingThread;

		private Bitmap ExeFallBack16;
		private Bitmap ExeFallBack256;
		private Bitmap ExeFallBack32;
		private Bitmap ExeFallBack48;
		//[Obsolete("Not Used", true)]
		//private Bitmap Shield16;
		//[Obsolete("Not Used", true)]
		//private Bitmap Shield256;
		//[Obsolete("Never Actually Used")]
		//private Bitmap Shield32;
		//[Obsolete("Not Used", true)]
		//private Bitmap Shield48;
		private int ShieldIconIndex;
		private ImageList extra = new ImageList(ImageListSize.ExtraLarge);
		//private List<int> IndexesWithThumbnail = new List<int>();
		//[Obsolete("Never Actually Used")]
		//private bool IsDoubleNavFinished = false;
		private ImageList jumbo = new ImageList(ImageListSize.Jumbo);
		private ImageList large = new ImageList(ImageListSize.Large);
		//private ShellItem m_CurrentFolder;
		private ShellViewStyle m_View;
		private SyncQueue<int> overlayQueue = new SyncQueue<int>(); //3000
		//private Dictionary<int, int> overlays = new Dictionary<int, int>();
		private Thread _OverlaysLoadingThread;
		private Thread _ShieldLoadingThread;
		private F.Timer selectionTimer = new F.Timer();
		//private Dictionary<int, int> shieldedIcons = new Dictionary<int, int>();
		private SyncQueue<int> shieldQueue = new SyncQueue<int>(); //3000
		private ImageList small = new ImageList(ImageListSize.SystemSmall);
		private Thread _IconLoadingThread;
		private Thread _UpdateSubitemValuesThread;
		//private ConcurrentDictionary<int, ConcurrentDictionary<Collumns, object>> SubItems = new ConcurrentDictionary<int, ConcurrentDictionary<Collumns, object>>();
		private SyncQueue<int> ThumbnailsForCacheLoad = new SyncQueue<int>(); //5000
		private SyncQueue<Tuple<int, int, PROPERTYKEY>> ItemsForSubitemsUpdate = new SyncQueue<Tuple<int, int, PROPERTYKEY>>(); //5000
		//private List<int> cachedIndexes = new List<int>();
		private ConcurrentBag<Tuple<int, PROPERTYKEY, object>> SubItemValues = new ConcurrentBag<Tuple<int, PROPERTYKEY, object>>();
		private ManualResetEvent resetEvent = new ManualResetEvent(true);
		private SyncQueue<int> waitingThumbnails = new SyncQueue<int>(); //3000
		private List<int> _CuttedIndexes = new List<int>();
		//private int LastI = 0;
		//private int CurrentI = 0;
		private const int SW_SHOW = 5;
		private const uint SEE_MASK_INVOKEIDLIST = 12;
		private int _LastSelectedIndexByDragDrop = -1;

		#endregion Private Members

		#region Initializer

		/// <summary> Main constructor </summary>
		public ShellView() {
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.EnableNotifyMessage, true);
			this.ItemForRename = -1;
			InitializeComponent();
			this.Items = new List<ShellItem>();
			this.LVItemsColorCodes = new List<LVItemColor>();
			this.AllAvailableColumns = this.AvailableColumns();
			this.AllowDrop = true;
			_IconLoadingThread = new Thread(_IconsLoadingThreadRun) { IsBackground = true, Priority = ThreadPriority.AboveNormal };
			_IconLoadingThread.Start();
			_IconCacheLoadingThread = new Thread(_IconCacheLoadingThreadRun) { IsBackground = true, Priority = ThreadPriority.Normal };
			_IconCacheLoadingThread.SetApartmentState(ApartmentState.STA);
			_IconCacheLoadingThread.Start();
			_OverlaysLoadingThread = new Thread(_OverlaysLoadingThreadRun) { IsBackground = true, Priority = ThreadPriority.BelowNormal };
			_OverlaysLoadingThread.Start();
			_ShieldLoadingThread = new Thread(_ShieldLoadingThreadRun) { IsBackground = true, Priority = ThreadPriority.BelowNormal };
			_ShieldLoadingThread.Start();
			_UpdateSubitemValuesThread = new Thread(_UpdateSubitemValuesThreadRun) { IsBackground = true, Priority = ThreadPriority.Normal };
			_UpdateSubitemValuesThread.Start();
			//History = new ShellHistory();
			_ResetTimer.Interval = 200;
			_ResetTimer.Tick += resetTimer_Tick;

			Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO() { cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO)) };

			Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_APPLICATION, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
			ExeFallBack48 = extra.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			ExeFallBack256 = jumbo.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			ExeFallBack16 = small.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();

			Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_SHIELD, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
			//Shield48 = extra.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			//Shield256 = jumbo.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			//Shield16 = small.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			ShieldIconIndex = defIconInfo.iSysIconIndex;

			this.KeyDown += ShellView_KeyDown;
			this.MouseUp += ShellView_MouseUp;
			this.GotFocus += ShellView_GotFocus;
		}

		#endregion Initializer

		#region Events

		private void selectionTimer_Tick(object sender, EventArgs e) {
			if (this.ItemForRename != this.GetFirstSelectedItemIndex())
				this.EndLabelEdit();
			if (MouseButtons != F.MouseButtons.Left) {
				//RedrawWindow();
				OnSelectionChanged();
				if (KeyJumpTimerDone != null) {
					KeyJumpTimerDone(this, EventArgs.Empty);
				}
				(sender as F.Timer).Stop();
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
				if (selectionTimer.Enabled) {
					selectionTimer.Stop();
					selectionTimer.Start();
				}
				else {
					selectionTimer.Start();
				}
			}
		}

		private void ShellView_GotFocus(object sender, EventArgs e) {
			this.Focus();
			User32.SetForegroundWindow(this.LVHandle);
		}

		private void ShellView_KeyDown(object sender, KeyEventArgs e) {
			if (ItemForRealName_IsAny) {
				if (e.KeyCode == Keys.Escape) {
					this.EndLabelEdit(true);
				}
				if (e.KeyCode == Keys.F2) {
					//TODO: implement a conditional selection inside rename textbox!
				}
				e.Handled = true;
				return;
			}
			if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
				switch (e.KeyCode) {
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
						AsyncUnbuffCopy copy = new AsyncUnbuffCopy();
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

			if (e.KeyCode == Keys.Escape) {
				foreach (var index in this._CuttedIndexes) {
					LVITEM item = new LVITEM();
					item.mask = LVIF.LVIF_STATE;
					item.stateMask = LVIS.LVIS_CUT;
					item.state = 0;
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, index, ref item);
				}
				this._CuttedIndexes.Clear();
				F.Clipboard.Clear();
			}
			if (e.KeyCode == Keys.Delete) {
				this.DeleteSelectedFiles((Control.ModifierKeys & Keys.Shift) != Keys.Shift);
			}
			if (e.KeyCode == Keys.F5) {
				this.RefreshContents();
			}
		}

		#endregion Events

		#region Overrides

		protected override void OnDragDrop(F.DragEventArgs e) {
			int row = -1;
			int collumn = -1;
			this.HitTest(PointToClient(new System.Drawing.Point(e.X, e.Y)), out row, out collumn);
			ShellItem destination = row != -1 ? Items[row] : CurrentFolder;

			//TODO: Find out if we can remove this select and just use an If Then
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

			var wp = new Win32Point() { X = e.X, Y = e.Y };

			if (e.Data.GetDataPresent("DragImageBits"))
				DropTargetHelper.Get.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
			if (_LastSelectedIndexByDragDrop != -1 & !DraggedItemIndexes.Contains(_LastSelectedIndexByDragDrop))
				this.DeselectItemByIndex(_LastSelectedIndexByDragDrop);
		}

		protected override void OnDragLeave(EventArgs e) {
			DropTargetHelper.Get.Create.DragLeave();
		}



		internal static void Drag_SetEffect(F.DragEventArgs e) {
			if ((e.KeyState & (8 + 32)) == (8 + 32) && (e.AllowedEffect & F.DragDropEffects.Link) == F.DragDropEffects.Link) {
				// KeyState 8 + 32 = CTL + ALT

				// Link drag-and-drop effect.
				e.Effect = F.DragDropEffects.Link;
			}
			else if ((e.KeyState & 32) == 32 && (e.AllowedEffect & F.DragDropEffects.Link) == F.DragDropEffects.Link) {
				// ALT KeyState for link.
				e.Effect = F.DragDropEffects.Link;
			}
			else if ((e.KeyState & 4) == 4 && (e.AllowedEffect & F.DragDropEffects.Move) == F.DragDropEffects.Move) {
				// SHIFT KeyState for move.
				e.Effect = F.DragDropEffects.Move;
			}
			else if ((e.KeyState & 8) == 8 && (e.AllowedEffect & F.DragDropEffects.Copy) == F.DragDropEffects.Copy) {
				// CTL KeyState for copy.
				e.Effect = F.DragDropEffects.Copy;
			}
			else if ((e.AllowedEffect & F.DragDropEffects.Move) == F.DragDropEffects.Move) {
				// By default, the drop action should be move, if allowed.
				e.Effect = F.DragDropEffects.Move;
			}
			else
				e.Effect = F.DragDropEffects.Copy;
		}

		protected override void OnDragOver(F.DragEventArgs e) {
			var wp = new Win32Point() { X = e.X, Y = e.Y };
			Drag_SetEffect(e);


			int row = -1;
			int collumn = -1;
			this.HitTest(PointToClient(new System.Drawing.Point(e.X, e.Y)), out row, out collumn);

			if (_LastSelectedIndexByDragDrop != -1 && !DraggedItemIndexes.Contains(_LastSelectedIndexByDragDrop)) {
				this.DeselectItemByIndex(_LastSelectedIndexByDragDrop);
			}

			if (row != -1) {
				this.SelectItemByIndex(row);
			}
			else if (_LastSelectedIndexByDragDrop != -1 & !DraggedItemIndexes.Contains(_LastSelectedIndexByDragDrop)) {
				this.DeselectItemByIndex(_LastSelectedIndexByDragDrop);
			}

			_LastSelectedIndexByDragDrop = row;

			if (e.Data.GetDataPresent("DragImageBits"))
				DropTargetHelper.Get.Create.DragOver(ref wp, (int)e.Effect);
		}

		protected override void OnDragEnter(F.DragEventArgs e) {
			var wp = new Win32Point() { X = e.X, Y = e.Y };
			Drag_SetEffect(e);

			if (e.Data.GetDataPresent("DragImageBits"))
				DropTargetHelper.Get.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (int)e.Effect);
		}

		protected override void OnQueryContinueDrag(F.QueryContinueDragEventArgs e) {
			base.OnQueryContinueDrag(e);
		}

		protected override void OnGiveFeedback(F.GiveFeedbackEventArgs e) {
			e.UseDefaultCursors = false;
			Cursor.Current = Cursors.Arrow;
			base.OnGiveFeedback(e);
			F.Application.DoEvents();
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

		private void UpdateItem(ShellItem obj1, ShellItem obj2) {
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
			}
			else {
				ShellItem theItem = Items.SingleOrDefault(s => s.ParsingName == obj1.ParsingName);
				if (theItem != null) {
					int itemIndex = Items.IndexOf(theItem);
					Items[itemIndex] = obj2;
					ItemsHashed.Remove(theItem);
					ItemsHashed.Add(obj2, itemIndex);
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_UPDATE, itemIndex, 0);
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
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMINDEXRECT, ref lviLe, ref labelBounds);
			return new Rect(labelBounds.Left, labelBounds.Top, labelBounds.Right - labelBounds.Left, labelBounds.Bottom - labelBounds.Top);
		}

		public void Test_ChangeName(string NewName) {
			if (ItemForRealName_IsAny && this.Items != null && this.Items.Count >= ItemForRename) {
				var item = this.Items[ItemForRename];
				if (NewName.ToLowerInvariant() != item.DisplayName.ToLowerInvariant()) {
					RenameShellItem(item.ComInterface, NewName);
					this.RedrawWindow();
				}
				ItemForRename = -1;
			}
			this.IsFocusAllowed = true;
		}

		private void BeginLabelEdit(int itemIndex) {
			//this._IsInRenameMode = true;
			this.IsFocusAllowed = false;
			this.ItemForRename = itemIndex;
			if (this.BeginItemLabelEdit != null) {
				this.BeginItemLabelEdit.Invoke(this, new RenameEventArgs(itemIndex));
			}

			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_UPDATE, itemIndex, 0);
			RedrawWindow();

			//LVITEMINDEX lviLe = new LVITEMINDEX();
			//lviLe.iItem = itemIndex;
			//lviLe.iGroup = this.GetGroupIndex(itemIndex);
			//var labelBounds = new User32.RECT();
			//labelBounds.Left = 2;
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMINDEXRECT, ref lviLe, ref labelBounds);
			//var size = TextRenderer.MeasureText(this.Items[itemIndex].DisplayName, this._Editor.Font);
			//this._Editor.Width = labelBounds.Right - labelBounds.Left + 4;
			//this._Editor.Height = labelBounds.Bottom - labelBounds.Top + 4;
			//this._Editor.Location = new System.Drawing.Point(labelBounds.Left - 2, labelBounds.Top - 2);
			//this._Editor.Text = this.Items[itemIndex].DisplayName;
			//this._Editor.Tag = itemIndex;
			////User32.SetParent(this._Editor.Handle, this.LVHandle);
			//this._Editor.Parent = this;
			////User32.ShowWindow(this._Editor.Handle, User32.ShowWindowCommands.Show);
			//this._Editor.Show();
			////this._Editor.Focus();
			//this._Editor.SelectAll();
		}

		private void EndLabelEdit(Boolean isCancel = false) {
			if (this.EndItemLabelEdit != null) {
				this.EndItemLabelEdit.Invoke(this, !isCancel);
			}
			/*
			if (ItemForRename != -1 && this.Items != null && this.Items.Count >= ItemForRename) {
				var item = this.Items[ItemForRename];
				if (!String.IsNullOrEmpty(NewName)) {
					if (!isCancel && item != null) {
						if (NewName.ToLowerInvariant() != item.DisplayName.ToLowerInvariant()) {
							RenameShellItem(item.ComInterface, NewName);
							NewName = String.Empty;
							this.RedrawWindow();
						}
					}
				}
			}
			this.IsFocusAllowed = true;
			*/
		}

		protected override void WndProc(ref Message m) {
			try {
				bool isSmallIcons = (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details);

				//TODO: Remove Extra If(...)
				if (m.Msg == (int)WM.WM_PARENTNOTIFY) {
					if (User32.LOWORD((int)m.WParam) == (int)WM.WM_MBUTTONDOWN) {
						OnItemMiddleClick();
					}
				}
				base.WndProc(ref m);
				if (m.Msg == ShellNotifications.WM_SHNOTIFY) {
					if (Notifications.NotificationReceipt(m.WParam, m.LParam)) {
						foreach (NotifyInfos info in Notifications.NotificationsReceived.ToArray()) {
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_CREATE || info.Notification == ShellNotifications.SHCNE.SHCNE_MKDIR) {
								var obj = new ShellItem(info.Item1);
								if (obj.Extension.ToLowerInvariant() != ".tmp" && obj.Parent.Equals(this.CurrentFolder)) {
									var itemIndex = InsertNewItem(obj);
									if (this.ItemUpdated != null)
										this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Created, obj, null, itemIndex));
								}
								else {
									var affectedItem = this.Items.SingleOrDefault(s => s.Equals(obj.Parent));
									if (affectedItem != null) {
										var index = this.Items.IndexOf(affectedItem);
										this.RefreshItem(index, true);
									}
								}
								Notifications.NotificationsReceived.Remove(info);
							}

							//
							//TODO: Can we replace all of these if(...) with a switch?
							//

							//TODO: Should this be and Elser If(...)?
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_UPDATEITEM) {
								var obj = new ShellItem(info.Item1);
								var exisitingItem = this.Items.Where(w => w.Equals(obj)).SingleOrDefault();
								if (exisitingItem == null) {
									//TODO: Check Changes to If(...)
									if (obj.Extension.ToLowerInvariant() != ".tmp" && obj.Parent.Equals(this.CurrentFolder)) {
										var itemIndex = InsertNewItem(obj);
										this.RefreshItem(itemIndex, true);
										if (this.ItemUpdated != null)
											this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Created, obj, null, itemIndex));
									}
								}
								//else
								//{
								//	Items.Remove(obj);
								//	ItemsHashed.Remove(obj);
								//	if (obj.Extension.ToLowerInvariant() != ".tmp")
								//	{
								//		if (obj.Parent.Equals(this.CurrentFolder))
								//		{
								//			var itemIndex = InsertNewItem(obj);
								//			if (this.ItemUpdated != null)
								//				this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Created, obj, null, itemIndex));
								//		}
								//	}
								//}
								Notifications.NotificationsReceived.Remove(info);
							}
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_UPDATEDIR) {
								var obj = new ShellItem(info.Item1);
								var item = this.ItemsHashed.SingleOrDefault(s => s.Key == obj);
								if (item.Key != null) {
									this.RefreshItem(item.Value, true);
								}
								Notifications.NotificationsReceived.Remove(info);
							}
							//TODO: Should this be and Else If(...)?
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_DELETE || info.Notification == ShellNotifications.SHCNE.SHCNE_RMDIR) {
								var obj = new ShellItem(info.Item1);
								if (!String.IsNullOrEmpty(obj.ParsingName)) {
									ShellItem theItem = Items.SingleOrDefault(s => s.Equals(obj));
									if (theItem != null) {
										Items.Remove(theItem);
										if (this.IsGroupsEnabled) {
											this.SetGroupOrder(false);
										}
										this.SetSortCollumn(this.LastSortedColumnIndex, this.LastSortOrder, false);

										if (this.ItemUpdated != null)
											this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Deleted, obj, null, -1));
									}
								}
								Notifications.NotificationsReceived.Remove(info);
							}
							//TODO: Should this be and Else If(...)?
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER || info.Notification == ShellNotifications.SHCNE.SHCNE_RENAMEITEM) {
								ShellItem obj1 = new ShellItem(info.Item1), obj2 = new ShellItem(info.Item2);
								if (!String.IsNullOrEmpty(obj1.ParsingName) && !String.IsNullOrEmpty(obj2.ParsingName)) {
									UpdateItem(obj1, obj2);
								}
								Notifications.NotificationsReceived.Remove(info);
							}
							//TODO: Should this be and Else If(...)?
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_DRIVEADD) {
								//TODO: Check Change. Moved [obj] Inside the If(...)
								if (this.CurrentFolder.Equals(KnownFolders.Computer)) {
									var obj = new ShellItem(info.Item1);
									this.InsertNewItem(obj);
								}

								Notifications.NotificationsReceived.Remove(info);
							}
							//TODO: Should this be and Else If(...)?
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_DRIVEREMOVED) {
								//TODO: Check Change. Moved [obj] Inside the If(...)
								if (this.CurrentFolder.Equals(KnownFolders.Computer)) {
									var obj = new ShellItem(info.Item1);
									Items.Remove(obj);
									ItemsHashed.Remove(obj);
									if (this.IsGroupsEnabled) this.SetGroupOrder(false);

									User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
								}

								Notifications.NotificationsReceived.Remove(info);
							}
							//TODO: Should this be and Else If(...)?
							if (info.Notification == ShellNotifications.SHCNE.SHCNE_ATTRIBUTES) {
								var obj = new ShellItem(info.Item1);
								var exisitingItem = this.ItemsHashed.Where(w => w.Key.Equals(obj)).SingleOrDefault();
								if (exisitingItem.Key != null) {
									this.RefreshItem(exisitingItem.Value, true);
									if (this.ItemUpdated != null)
										this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Updated, obj, null, exisitingItem.Value));
								}
								Notifications.NotificationsReceived.Remove(info);
							}
						}
					}
					this.Focus();
				}

				if (m.Msg == 78) {
					var nmhdrHeader = (NMHEADER)(m.GetLParam(typeof(NMHEADER)));
					//TODO: Combine Else If(...)s and Remove If for MessageBox
					if (nmhdrHeader.hdr.code == (int)HDN.HDN_DROPDOWN) {
						F.MessageBox.Show(nmhdrHeader.iItem.ToString());
					}
					else if (nmhdrHeader.hdr.code == (int)HDN.HDN_BEGINTRACKW) {
						if (this.View != ShellViewStyle.Details) m.Result = (IntPtr)1;
					}
					else if (nmhdrHeader.hdr.code == (int)HDN.HDN_BEGINTRACKW) {
						if (this.View != ShellViewStyle.Details) m.Result = (IntPtr)1;
					}

					NMHDR nmhdr = new NMHDR();
					nmhdr = (NMHDR)m.GetLParam(nmhdr.GetType());
					switch ((int)nmhdr.code) {
						case WNM.LVN_ENDLABELEDITW:
							var nmlvedit = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
							if (!String.IsNullOrEmpty(nmlvedit.item.pszText)) {
								RenameShellItem(this.Items[nmlvedit.item.iItem].ComInterface, nmlvedit.item.pszText);
								this.RedrawWindow();
							}
							break;

						case WNM.LVN_GETDISPINFOW:
							var nmlv = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
							if (Items.Count == 0 || Items.Count - 1 < nmlv.item.iItem)
								break;
							var currentItem = Items[nmlv.item.iItem];

							if ((nmlv.item.mask & LVIF.LVIF_TEXT) == LVIF.LVIF_TEXT) {
								if (nmlv.item.iSubItem == 0) {
									//nmlv.item.pszText = this.View == ShellViewStyle.Tile ? String.Empty : (!String.IsNullOrEmpty(NewName) ? (ItemForRename == nmlv.item.iItem ? "" : currentItem.DisplayName) : currentItem.DisplayName);
									if (this.ItemForRealName_IsAny) {
										if (this.GetFirstSelectedItemIndex() == nmlv.item.iItem) {
											nmlv.item.pszText = "";
										}
									}
									else {
										nmlv.item.pszText = this.View == ShellViewStyle.Tile ? String.Empty : currentItem.DisplayName;
									}
									Marshal.StructureToPtr(nmlv, m.LParam, false);
								}
								else if (isSmallIcons) {
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
											}
											else
												val = valueCached.Item3.ToString();

											nmlv.item.pszText = val;
											Marshal.StructureToPtr(nmlv, m.LParam, false);
										}
										else {
											ShellItem temp = !(currentItem.IsNetDrive || currentItem.IsNetworkPath) && !currentItem.ParsingName.StartsWith("::") ?
												new ShellItem(currentItem.ParsingName) : currentItem;


											/*
											if (!(currentItem.IsNetDrive || currentItem.IsNetworkPath) && !currentItem.ParsingName.StartsWith("::")) 
												temp = new ShellItem(currentItem.ParsingName);
											else 
												temp = currentItem;
											*/
											IShellItem2 isi2 = (IShellItem2)temp.ComInterface;
											Guid guid = new Guid(InterfaceGuids.IPropertyStore);
											IPropertyStore propStore = null;
											isi2.GetPropertyStore(GetPropertyStoreOptions.FastPropertiesOnly, ref guid, out propStore);
											PROPERTYKEY pk = currentCollumn.pkey;
											PropVariant pvar = new PropVariant();
											if (propStore != null && propStore.GetValue(ref pk, pvar) == HResult.S_OK) {
												//if (propStore.GetValue(ref pk, pvar) == HResult.S_OK) {
												String val = String.Empty;
												if (pvar.Value != null) {
													if (currentCollumn.CollumnType == typeof(DateTime)) {
														val = ((DateTime)pvar.Value).ToString(Thread.CurrentThread.CurrentCulture);
													}
													else if (currentCollumn.CollumnType == typeof(long)) {
														val = String.Format("{0} KB", (Math.Ceiling(Convert.ToDouble(pvar.Value.ToString()) / 1024).ToString("# ### ### ##0"))); //ShlWapi.StrFormatByteSize(Convert.ToInt64(pvar.Value.ToString()));
													}
													else if (currentCollumn.CollumnType == typeof(PerceivedType)) {
														val = ((PerceivedType)pvar.Value).ToString();
													}
													else if (currentCollumn.CollumnType == typeof(FileAttributes)) {
														var resultString = this.GetFilePropertiesString(pvar.Value);
														val = resultString;
													}
													else {
														val = pvar.Value.ToString();
													}
													nmlv.item.pszText = val;
													Marshal.StructureToPtr(nmlv, m.LParam, false);
													pvar.Dispose();
												}
												else {
													ItemsForSubitemsUpdate.Enqueue(new Tuple<int, int, PROPERTYKEY>(nmlv.item.iItem, nmlv.item.iSubItem, pk));
												}
											}
											//}
										}
									}
									catch {
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

						case WNM.LVN_COLUMNCLICK:
							NMLISTVIEW nlcv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));

							/*
							if (!this.IsGroupsEnabled) {
								SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
							}
							else {
								if (this.LastGroupCollumn != this.Collumns[nlcv.iSubItem]) {
									SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
								}
								this.SetGroupOrder(); //this.SetGroupOrder(false);
							}
							*/

							if (!this.IsGroupsEnabled) {
								SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
							}
							else if (this.LastGroupCollumn == this.Collumns[nlcv.iSubItem]) {
								this.SetGroupOrder();
							}
							else {
								SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
								this.SetGroupOrder(false);
							}


							break;

						case WNM.LVN_GETINFOTIP:
							NMLVGETINFOTIP nmGetInfoTip = (NMLVGETINFOTIP)m.GetLParam(typeof(NMLVGETINFOTIP));
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
							ToolTip.Left = Cursor.Position.X;
							ToolTip.Top = Cursor.Position.Y;
							ToolTip.ShowTooltip();

							break;

						case WNM.LVN_ODFINDITEM:
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
							}
							else {
								int selindOver = GetFirstIndexOf(KeyJumpString, 0);
								if (selindOver != -1) {
									m.Result = (IntPtr)(selindOver);
									if (IsGroupsEnabled)
										this.SelectItemByIndex(selindOver, true, true);
								}
							}

							break;

						case WNM.LVN_INCREMENTALSEARCH:
							var incrementalSearch = (NMLVFINDITEM)m.GetLParam(typeof(NMLVFINDITEM));
							break;

						case -175:
							var nmlvLe = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
							BeginLabelEdit(nmlvLe.item.iItem);
							m.Result = (IntPtr)1;
							break;

						case WNM.LVN_ITEMACTIVATE:
							if (this.ToolTip != null && this.ToolTip.IsVisible)
								this.ToolTip.HideTooltip();
							if (ItemForRealName_IsAny) {
								this.EndLabelEdit();
							}
							else {
								var iac = new NMITEMACTIVATE();
								iac = (NMITEMACTIVATE)m.GetLParam(iac.GetType());
								try {
									ShellItem selectedItem = Items[iac.iItem];
									if (selectedItem.IsFolder) {
										Navigate_Full(selectedItem, true);
									}
									else if (selectedItem.IsLink && selectedItem.ParsingName.EndsWith(".lnk")) {
										var shellLink = new ShellLink(selectedItem.ParsingName);
										var newSho = new ShellItem(shellLink.TargetPIDL);
										if (newSho.IsFolder)
											Navigate_Full(newSho, true);
									}
									else {
										StartProcessInCurrentDirectory(selectedItem);
									}
								}
								catch (Exception) {
								}

							}

							break;

						case WNM.LVN_BEGINSCROLL:
							this.EndLabelEdit();
							resetEvent.Reset();
							_ResetTimer.Stop();
							//this.Cancel = true;
							ToolTip.HideTooltip();
							ThumbnailsForCacheLoad.Clear();
							overlayQueue.Clear();
							shieldQueue.Clear();
							//! to be revised this for performace
							try {
								if (MaintenanceThread != null && MaintenanceThread.IsAlive) MaintenanceThread.Abort();
								MaintenanceThread = new Thread(() => {
									while (ItemsForSubitemsUpdate.queue.Count > 0) {
										//Thread.Sleep(1);
										var item = ItemsForSubitemsUpdate.Dequeue();
										var itemBounds = new User32.RECT();
										LVITEMINDEX lvi = new LVITEMINDEX();
										lvi.iItem = item.Item1;
										lvi.iGroup = this.GetGroupIndex(item.Item1);
										User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
										Rectangle r = new Rectangle(itemBounds.Left, itemBounds.Top, itemBounds.Right - itemBounds.Left, itemBounds.Bottom - itemBounds.Top);
										if (r.IntersectsWith(this.ClientRectangle)) {
											ItemsForSubitemsUpdate.Enqueue(item);
										}
									}

									while (waitingThumbnails.queue.Count > 0) {
										//Thread.Sleep(1);
										var iconIndex = waitingThumbnails.Dequeue();
										var itemBounds = new User32.RECT();
										LVITEMINDEX lvi = new LVITEMINDEX();
										lvi.iItem = iconIndex;
										lvi.iGroup = this.GetGroupIndex(iconIndex);
										User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
										Rectangle r = new Rectangle(itemBounds.Left, itemBounds.Top, itemBounds.Right - itemBounds.Left, itemBounds.Bottom - itemBounds.Top);
										if (r.IntersectsWith(this.ClientRectangle)) {
											waitingThumbnails.Enqueue(iconIndex);
										}
									}
								});
								MaintenanceThread.Start();
							}
							catch (ThreadAbortException) {
							}
							GC.Collect();

							break;

						case WNM.LVN_ENDSCROLL:
							//this.Cancel = false;
							_ResetTimer.Start();
							//Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
							break;

						case -100:
							F.MessageBox.Show("AM");
							break;

						case WNM.LVN_ITEMCHANGED:
							//RedrawWindow();

							NMLISTVIEW nlv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
							if ((nlv.uChanged & LVIF.LVIF_STATE) == LVIF.LVIF_STATE) {
								/*
								if (ItemForRealName_IsAny && nlv.iItem != -1 && nlv.iItem != this.ItemForRename)
									this.EndLabelEdit();
								*/

								ToolTip.HideTooltip();

								selectionTimer.Interval = 100;

								//TODO: Fix this
								selectionTimer.Tick -= selectionTimer_Tick;
								selectionTimer.Tick += selectionTimer_Tick;
								this._IsDragSelect = nlv.uNewState;
								if (IsGroupsEnabled) {
									if (nlv.iItem != -1) {
										var itemBounds = new User32.RECT();
										LVITEMINDEX lvi = new LVITEMINDEX();
										lvi.iItem = nlv.iItem;
										lvi.iGroup = this.GetGroupIndex(nlv.iItem);
										User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
										RedrawWindow(itemBounds);
									}
									else {
										RedrawWindow();
									}
								}
								//if ((nlv.uNewState & LVIS.LVIS_SELECTED) == 0)
								//{
								//RedrawWindow();
								//}
								if (!selectionTimer.Enabled) {
									selectionTimer.Start();
								}
							}

							break;

						case WNM.LVN_ODSTATECHANGED:
							//RedrawWindow();
							OnSelectionChanged();
							break;

						case WNM.LVN_KEYDOWN:
							NMLVKEYDOWN nkd = (NMLVKEYDOWN)m.GetLParam(typeof(NMLVKEYDOWN));
							Keys key = (Keys)((int)nkd.wVKey);
							if (KeyDown != null) {
								KeyDown(this, new KeyEventArgs(key));
							}
							if (!ItemForRealName_IsAny) {
								switch (nkd.wVKey) {
									case (short)Keys.F2:
										RenameSelectedItem();
										break;

									case (short)Keys.Enter:
										var selectedItem = this.GetFirstSelectedItem();
										if (selectedItem.IsFolder) {
											Navigate(selectedItem);
										}
										else if (selectedItem.IsLink && selectedItem.ParsingName.EndsWith(".lnk")) {
											var shellLink = new ShellLink(selectedItem.ParsingName);
											var newSho = new ShellItem(shellLink.TargetPIDL);
											if (newSho.IsFolder)
												Navigate(newSho);
											else
												StartProcessInCurrentDirectory(newSho);

											shellLink.Dispose();
										}
										else {
											StartProcessInCurrentDirectory(selectedItem);
										}
										break;
								}

								this.Focus();
							}
							else {
								System.Windows.Input.InputManager.Current.ProcessInput(new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice,
										System.Windows.Input.Keyboard.PrimaryDevice.ActiveSource, Environment.TickCount, System.Windows.Input.KeyInterop.KeyFromVirtualKey(nkd.wVKey)) {
											RoutedEvent = System.Windows.Controls.Control.KeyDownEvent
										});
								m.Result = (IntPtr)1;
								switch (nkd.wVKey) {
									case (short)Keys.Enter:
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

						case WNM.LVN_GROUPINFO:
							//RedrawWindow();
							break;

						case WNM.LVN_HOTTRACK:
							NMLISTVIEW nlvHotTrack = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
							if (nlvHotTrack.iItem != ToolTip.ItemIndex) {
								ToolTip.HideTooltip();
							}
							this.Focus();
							break;

						case WNM.LVN_BEGINDRAG:
							//uint CFSTR_SHELLIDLIST =
							//	User32.RegisterClipboardFormat("Shell IDList Array");
							//	F.DataObject dobj = new F.DataObject("")
							//Task.Run(() =>
							//{
							//	this.BeginInvoke(new MethodInvoker(() =>
							//	{
							this.DraggedItemIndexes.Clear();
							IntPtr dataObjPtr = IntPtr.Zero;
							System.Runtime.InteropServices.ComTypes.IDataObject dataObject = this.SelectedItems.ToArray().GetIDataObject(out dataObjPtr);

							uint ef = 0;
							Shell32.SHDoDragDrop(this.Handle, dataObject, null, unchecked((uint)F.DragDropEffects.All | (uint)F.DragDropEffects.Link), out ef);

							//	}));
							//});
							//Ole32.DoDragDrop(ddataObject, this, F.DragDropEffects.All, out effect);
							//DragSourceHelper.DoDragDrop(this, new System.Drawing.Point(0, 0), F.DragDropEffects.Copy, new KeyValuePair<string, object>("Shell IDList Array", new ShellItemArray(this.SelectedItems.Select(s => s.m_ComInterface).ToArray())));
							break;

						case WNM.NM_RCLICK:
							var nmhdrHdn = (NMHEADER)(m.GetLParam(typeof(NMHEADER)));
							if (nmhdrHdn.iItem != -1 && nmhdrHdn.hdr.hwndFrom == this.LVHandle) {
								var selitems = this.SelectedItems;
								NMITEMACTIVATE itemActivate = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
								ShellContextMenu cm = new ShellContextMenu(selitems.ToArray());
								cm.ShowContextMenu(this, itemActivate.ptAction, CMF.CANRENAME);
							}
							else if (nmhdrHdn.iItem == -1) {
								NMITEMACTIVATE itemActivate = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
								ShellContextMenu cm = new ShellContextMenu(new ShellItem[1] { this.CurrentFolder }, SVGIO.SVGIO_BACKGROUND);
								cm.ShowContextMenu(this, itemActivate.ptAction, 0, true);
							}
							else if (ColumnHeaderRightClick != null) {
								ColumnHeaderRightClick(this, new MouseEventArgs(F.MouseButtons.Right, 1, MousePosition.X, MousePosition.Y, 0));
							}
							break;

						case WNM.NM_CLICK:
							break;

						case WNM.NM_SETFOCUS:
							if (IsGroupsEnabled)
								RedrawWindow();
							if (this.ToolTip != null && this.ToolTip.IsVisible)
								this.ToolTip.HideTooltip();
							OnGotFocus();
							this.IsFocusAllowed = true;
							break;

						case WNM.NM_KILLFOCUS:
							/*
							if (this.ItemForRename != -1)
								EndLabelEdit();
							*/
							if (IsGroupsEnabled)
								RedrawWindow();
							if (this.ToolTip != null && this.ToolTip.IsVisible)
								this.ToolTip.HideTooltip();
							//OnLostFocus();
							this.Focus();
							break;

						case CustomDraw.NM_CUSTOMDRAW: {
								if (nmhdr.hwndFrom == this.LVHandle) {
									User32.SendMessage(this.LVHandle, 296, User32.MAKELONG(1, 1), 0);
									var nmlvcd = (User32.NMLVCUSTOMDRAW)m.GetLParam(typeof(BExplorer.Shell.Interop.User32.NMLVCUSTOMDRAW));
									var index = (int)nmlvcd.nmcd.dwItemSpec;
									var hdc = nmlvcd.nmcd.hdc;
									if (nmlvcd.dwItemType == 1)
										return;
									ShellItem sho = null;

									if (Items.Count > index) {
										sho = Items[index];
									}

									//TODO: Consider [if (Items.Count > index) {] AND [if (sho != null)] Into 1 [If]
									System.Drawing.Color? textColor = null;
									if (sho != null) {
										if (this.LVItemsColorCodes != null && this.LVItemsColorCodes.Count > 0) {
											if (!String.IsNullOrEmpty(sho.Extension)) {
												var extItemsAvailable = this.LVItemsColorCodes.Where(c => c.ExtensionList.Contains(sho.Extension)).Count() > 0;
												if (extItemsAvailable) {
													var color = this.LVItemsColorCodes.Where(c => c.ExtensionList.ToLowerInvariant().Contains(sho.Extension)).Select(c => c.TextColor).SingleOrDefault();
													textColor = color;
												}
											}
										}
									}
									switch (nmlvcd.nmcd.dwDrawStage) {
										case CustomDraw.CDDS_PREPAINT:
											m.Result = (IntPtr)(CustomDraw.CDRF_NOTIFYITEMDRAW | CustomDraw.CDRF_NOTIFYPOSTPAINT | 0x40);
											break;

										case CustomDraw.CDDS_POSTPAINT:
											m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
											break;

										case CustomDraw.CDDS_ITEMPREPAINT:
											if (textColor != null) {
												nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
												Marshal.StructureToPtr(nmlvcd, m.LParam, false);

												m.Result = (IntPtr)(CustomDraw.CDRF_NEWFONT | CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW | 0x40);
											}
											else {
												m.Result = (IntPtr)(CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW | 0x40);
											}
											break;

										case CustomDraw.CDDS_ITEMPREPAINT | CustomDraw.CDDS_SUBITEM:
											if (textColor == null) {
												m.Result = (IntPtr)CustomDraw.CDRF_DODEFAULT;
											}
											else {
												nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
												Marshal.StructureToPtr(nmlvcd, m.LParam, false);
												m.Result = (IntPtr)CustomDraw.CDRF_NEWFONT;
											}
											break;

										case CustomDraw.CDDS_ITEMPOSTPAINT:
											if (nmlvcd.clrTextBk != 0) {
												var itemBounds = nmlvcd.nmcd.rc;
												LVITEMINDEX lvi = new LVITEMINDEX();
												lvi.iItem = index;
												lvi.iGroup = this.GetGroupIndex(index);
												//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);

												var iconBounds = new User32.RECT();

												iconBounds.Left = 1;

												User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref iconBounds);

												LVITEM lvItem = new LVITEM();
												lvItem.iItem = index;
												lvItem.iGroupId = lvi.iGroup;
												lvItem.iGroup = lvi.iGroup;
												lvItem.mask = LVIF.LVIF_STATE;
												lvItem.stateMask = LVIS.LVIS_SELECTED;

												LVITEM lvItemImageMask = new LVITEM();
												lvItemImageMask.iItem = index;
												lvItemImageMask.iGroupId = lvi.iGroup;
												lvItemImageMask.iGroup = lvi.iGroup;
												lvItemImageMask.mask = LVIF.LVIF_STATE;
												lvItemImageMask.stateMask = LVIS.LVIS_STATEIMAGEMASK;

												if (sho != null) {
													if (sho.OverlayIconIndex == -1) {
														overlayQueue.Enqueue(index);
													}
													if (sho.IsShielded == -1) {
														string shoExtension = sho.Extension;
														if (shoExtension == ".exe" || shoExtension == ".com" || shoExtension == ".bat")
															shieldQueue.Enqueue(index);
													}
													if (IconSize != 16) {
														var thumbnail = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.CacheOnly);
														if (sho.IsNeedRefreshing) {
															thumbnail = sho.Thumbnail.RefreshThumbnail((uint)IconSize);
															sho.IsNeedRefreshing = false;
														}
														if (thumbnail != null) {
															if (((thumbnail.Width > thumbnail.Height && thumbnail.Width != IconSize) || (thumbnail.Width < thumbnail.Height && thumbnail.Height != IconSize) || thumbnail.Width == thumbnail.Height && thumbnail.Width != IconSize)) {
																ThumbnailsForCacheLoad.Enqueue(index);
															}
															else {
																sho.IsThumbnailLoaded = true;
																sho.IsNeedRefreshing = false;
															}
															using (Graphics g = Graphics.FromHdc(hdc)) {
																var cutFlag = User32.SendMessage(this.LVHandle, Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_CUT);
																if (sho.IsHidden || cutFlag != 0 || this._CuttedIndexes.Contains(index))
																	thumbnail = Helpers.ChangeOpacity(thumbnail, 0.5f);
																g.DrawImageUnscaled(thumbnail, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - thumbnail.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - thumbnail.Height) / 2, thumbnail.Width, thumbnail.Height));

																if (this.ShowCheckboxes && View != ShellViewStyle.Details && View != ShellViewStyle.List) {
																	var res = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMW, 0, ref lvItemImageMask);

																	if ((nmlvcd.nmcd.uItemState & CDIS.HOT) == CDIS.HOT || (uint)lvItemImageMask.state == (2 << 12)) {
																		res = User32.SendMessage(this.LVHandle, Shell.Interop.MSG.LVM_GETITEMW, 0, ref lvItem);
																		var checkboxOffsetH = 14;
																		var checkboxOffsetV = 2;
																		if (View == ShellViewStyle.Tile || View == ShellViewStyle.SmallIcon)
																			checkboxOffsetH = 2;
																		if (View == ShellViewStyle.Tile)
																			checkboxOffsetV = 1;

																		CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV),
																			lvItem.state != 0 ? F.VisualStyles.CheckBoxState.CheckedNormal : F.VisualStyles.CheckBoxState.UncheckedNormal
																		);

																		//if (lvItem.state != 0) {
																		//	CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV), F.VisualStyles.CheckBoxState.CheckedNormal);
																		//}
																		//else {
																		//	CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV), F.VisualStyles.CheckBoxState.UncheckedNormal);
																		//}
																	}
																}
															}
															thumbnail.Dispose();
															thumbnail = null;
														}
														else {
															if (!sho.IsThumbnailLoaded)
																ThumbnailsForCacheLoad.Enqueue(index);
															if ((sho.IconType & IExtractIconPWFlags.GIL_PERCLASS) == IExtractIconPWFlags.GIL_PERCLASS) {
																var icon = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly);
																if (icon != null) {
																	sho.IsIconLoaded = true;
																	using (Graphics g = Graphics.FromHdc(hdc)) {
																		var cutFlag = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_CUT);
																		if (sho.IsHidden || cutFlag != 0 || this._CuttedIndexes.Contains(index))
																			icon = Helpers.ChangeOpacity(icon, 0.5f);
																		g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));

																		if (this.ShowCheckboxes && View != ShellViewStyle.Details && View != ShellViewStyle.List) {
																			var res = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMW, 0, ref lvItemImageMask);

																			if ((nmlvcd.nmcd.uItemState & CDIS.HOT) == CDIS.HOT || (uint)lvItemImageMask.state == (2 << 12)) {
																				var checkboxOffsetH = 14;
																				var checkboxOffsetV = 2;

																				//TODO: Check this. why is [View == ShellViewStyle.Tile] being used 2 times
																				if (View == ShellViewStyle.Tile || View == ShellViewStyle.SmallIcon)
																					checkboxOffsetH = 2;
																				if (View == ShellViewStyle.Tile)
																					checkboxOffsetV = 1;
																				res = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMW, 0, ref lvItem);
																				CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV),
																					lvItem.state != 0 ? F.VisualStyles.CheckBoxState.CheckedNormal : F.VisualStyles.CheckBoxState.UncheckedNormal
																				);
																			}
																		}
																	}
																	icon.Dispose();
																}
															}
															else if ((sho.IconType & IExtractIconPWFlags.GIL_PERINSTANCE) == IExtractIconPWFlags.GIL_PERINSTANCE) {
																if (!sho.IsIconLoaded) {
																	waitingThumbnails.Enqueue(index);
																	using (Graphics g = Graphics.FromHdc(hdc)) {
																		if (IconSize == 16) {
																			g.DrawImage(ExeFallBack16, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		}
																		else if (IconSize <= 48) {
																			g.DrawImage(ExeFallBack48, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		}
																		else if (IconSize <= 256) {
																			g.DrawImage(ExeFallBack256, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		}
																	}
																}
																else {
																	Bitmap icon = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly);
																	if (icon != null) {
																		sho.IsIconLoaded = true;
																		using (Graphics g = Graphics.FromHdc(hdc)) {
																			var cutFlag = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_CUT);
																			if (sho.IsHidden || cutFlag != 0 || this._CuttedIndexes.Contains(index))
																				icon = Helpers.ChangeOpacity(icon, 0.5f);
																			g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));

																			if (this.ShowCheckboxes && View != ShellViewStyle.Details && View != ShellViewStyle.List) {
																				var res = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMW, 0, ref lvItemImageMask);

																				if ((nmlvcd.nmcd.uItemState & CDIS.HOT) == CDIS.HOT || (uint)lvItemImageMask.state == (2 << 12)) {
																					var checkboxOffsetH = 14;
																					var checkboxOffsetV = 2;

																					//TODO: Check this. why is [View == ShellViewStyle.Tile] being used 2 times
																					if (View == ShellViewStyle.Tile || View == ShellViewStyle.SmallIcon)
																						checkboxOffsetH = 2;
																					if (View == ShellViewStyle.Tile)
																						checkboxOffsetV = 1;

																					res = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMW, 0, ref lvItem);
																					CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV),
																						lvItem.state != 0 ? F.VisualStyles.CheckBoxState.CheckedNormal : F.VisualStyles.CheckBoxState.UncheckedNormal
																					);
																				}
																			}
																		}
																		icon.Dispose();
																	}
																}
															}
														}
													}
													else {
														sho.IsThumbnailLoaded = true;
														if ((sho.IconType & IExtractIconPWFlags.GIL_PERCLASS) == IExtractIconPWFlags.GIL_PERCLASS) {
															var icon = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly);
															if (icon != null) {
																sho.IsIconLoaded = true;
																using (Graphics g = Graphics.FromHdc(hdc)) {
																	var cutFlag = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_CUT);
																	if (sho.IsHidden || cutFlag != 0 || this._CuttedIndexes.Contains(index))
																		icon = Helpers.ChangeOpacity(icon, 0.5f);
																	g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));
																}
																icon.Dispose();
															}
														}
														else if ((sho.IconType & IExtractIconPWFlags.GIL_PERINSTANCE) == IExtractIconPWFlags.GIL_PERINSTANCE) {
															if (!sho.IsIconLoaded) {
																waitingThumbnails.Enqueue(index);
																using (Graphics g = Graphics.FromHdc(hdc)) {
																	g.DrawImage(ExeFallBack16, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																}
															}
															else {
																Bitmap icon = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly);
																if (icon != null) {
																	sho.IsIconLoaded = true;
																	using (Graphics g = Graphics.FromHdc(hdc)) {
																		var cutFlag = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_CUT);
																		if (sho.IsHidden || cutFlag != 0 || this._CuttedIndexes.Contains(index))
																			icon = Helpers.ChangeOpacity(icon, 0.5f);
																		g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));
																	}
																	icon.Dispose();
																}
															}
														}
													}


													//TODO: Double Check
													if (sho.OverlayIconIndex > 0) {
														if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon) {
															small.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - 16));
														}
														else if (this.IconSize > 180) {
															jumbo.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
														}
														else if (this.IconSize > 64) {
															extra.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 50));
														}
														else {
															large.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 32));
														}
													}

													/*
													if (sho.OverlayIconIndex > 0) {
														if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon) {
															small.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - 16));
														}
														else {
															if (this.IconSize > 180) {
																jumbo.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
															}
															else
																if (this.IconSize > 64) {
																	extra.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 50));
																}
																else {
																	large.DrawOverlay(hdc, sho.OverlayIconIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 32));
																}
														}
													}
													*/

													//TODO: Check Change, I think its correct
													if (sho.IsShielded > 0) {
														if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon) {
															small.DrawIcon(hdc, sho.IsShielded, new System.Drawing.Point(iconBounds.Right - 10, iconBounds.Bottom - 10), 8);
														}
														else if (this.IconSize > 180) {
															jumbo.DrawIcon(hdc, sho.IsShielded, new System.Drawing.Point(iconBounds.Right - this.IconSize / 3, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
														}
														else if (this.IconSize > 64) {
															extra.DrawIcon(hdc, sho.IsShielded, new System.Drawing.Point(iconBounds.Right - 60, iconBounds.Bottom - 50));
														}
														else {
															large.DrawIcon(hdc, sho.IsShielded, new System.Drawing.Point(iconBounds.Right - 42, iconBounds.Bottom - 32));
														}
													}

													if (View == ShellViewStyle.Tile) {
														var lableBounds = new User32.RECT();

														lableBounds.Left = 2;

														User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref lableBounds);

														using (Graphics g = Graphics.FromHdc(hdc)) {
															StringFormat fmt = new StringFormat();
															fmt.Trimming = StringTrimming.EllipsisCharacter;
															fmt.Alignment = StringAlignment.Center;
															fmt.Alignment = StringAlignment.Near;
															fmt.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
															fmt.LineAlignment = StringAlignment.Center;
															RectangleF lblrectTiles = new RectangleF(lableBounds.Left, itemBounds.Top + 4, lableBounds.Right - lableBounds.Left, 20);
															Font font = System.Drawing.SystemFonts.IconTitleFont;
															SolidBrush textBrush = new SolidBrush(textColor == null ? System.Drawing.SystemColors.ControlText : textColor.Value);
															g.DrawString(sho.DisplayName, font, textBrush, lblrectTiles, fmt);
															font.Dispose();
															textBrush.Dispose();
														}
													}
													if (!sho.IsInitialised) {
														OnItemDisplayed(sho, index);
														sho.IsInitialised = true;
													}
												}
											}
											m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
											break;
									}
								}
							}
							break;
					}
				}
			}
			catch (Exception ex) {
				resetEvent.Set();
				base.DefWndProc(ref m);
			}
		}

		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);
			//this.Invalidate();
			//this.Refresh();
			User32.MoveWindow(this.LVHandle, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, true);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);

			var il = new F.ImageList();
			il.ImageSize = new System.Drawing.Size(48, 48);
			var ils = new F.ImageList();
			ils.ImageSize = new System.Drawing.Size(16, 16);

			ComCtl32.INITCOMMONCONTROLSEX icc = new ComCtl32.INITCOMMONCONTROLSEX();
			icc.dwSize = Marshal.SizeOf(typeof(ComCtl32.INITCOMMONCONTROLSEX));
			icc.dwICC = 1;
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
			//User32.SendMessage(this.LVHandle, 4170, 0, 0);
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SET, 1, ils.Handle);
			UxTheme.SetWindowTheme(this.LVHandle, "Explorer", 0);

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
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.colum, (int)ListViewExtendedStyles.AutosizeColumns);
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.TrackSelect, (int)ListViewExtendedStyles.TrackSelect);
			//CurrentFolder = (ShellItem) KnownFolders.Desktop;
			this.Focus();
			User32.SetForegroundWindow(this.LVHandle);
		}

		protected override void OnHandleDestroyed(EventArgs e) {
			try {
				if (_IconLoadingThread.IsAlive)
					_IconLoadingThread.Abort();
				if (_IconCacheLoadingThread.IsAlive)
					_IconCacheLoadingThread.Abort();
				if (_OverlaysLoadingThread.IsAlive)
					_OverlaysLoadingThread.Abort();
				if (_ShieldLoadingThread.IsAlive)
					_ShieldLoadingThread.Abort();
				if (_UpdateSubitemValuesThread.IsAlive)
					_UpdateSubitemValuesThread.Abort();
				if (MaintenanceThread != null && MaintenanceThread.IsAlive)
					MaintenanceThread.Abort();
			}
			catch (ThreadAbortException) {
			}
			base.OnHandleDestroyed(e);
		}

		#endregion Overrides

		#region Public Methods

		/*
		public void ShowFileProperties() {
			IntPtr doPtr = IntPtr.Zero;
			if (Shell32.SHMultiFileProperties(this.SelectedItems.ToArray().GetIDataObject(out doPtr), 0) != 0 /*S_OK/) {
				throw new Win32Exception();
			}
		}
		*/
		public void ShowPropPage(IntPtr HWND, string filename, string proppage) {
			Shell32.SHObjectProperties(HWND, 0x2, filename, proppage);
		}

		public void ShowFileProperties(string Filename) {
			Shell32.SHELLEXECUTEINFO info = new Shell32.SHELLEXECUTEINFO();
			info.cbSize = Marshal.SizeOf(info);
			info.lpVerb = "properties";
			info.lpFile = Filename;
			info.nShow = SW_SHOW;
			info.fMask = SEE_MASK_INVOKEIDLIST;
			Shell32.ShellExecuteEx(ref info);
		}

		private void RedrawItem(int index) {
			//F.Application.DoEvents();
			if (index >= Items.Count - 1) return;
			var sho = Items[index];
			//F.Application.DoEvents();
			var itemBounds = new User32.RECT();
			itemBounds.Left = 1;
			LVITEMINDEX lvi = new LVITEMINDEX();
			lvi.iItem = index;
			lvi.iGroup = this.GetGroupIndex(index);
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

		public void UpdateItem(int index) {
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_UPDATE, index, 0);
		}

		/// <summary> Navigates to the parent of the currently displayed folder. </summary>
		public void NavigateParent() {
			/*
			this.SaveSettingsToDatabase(this.CurrentFolder);
			Navigate(CurrentFolder.Parent, false, false);
			*/

			//TODO: Check to see if we really want to set the current folder to the Parent!!
			Navigate_Full(CurrentFolder.Parent, true);
		}

		public void RefreshContents() {
			//Navigate(this.CurrentFolder, true, true);
			Navigate_Full(this.CurrentFolder, true, refresh: true);
		}

		public void RefreshItem(int index, Boolean IsForceRedraw = false) {
			if (IsForceRedraw) {
				this.Items[index] = new ShellItem(this.Items[index].Pidl);
				this.Items[index].IsNeedRefreshing = true;
				this.Items[index].OverlayIconIndex = -1;
			}
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_REDRAWITEMS, index, index);
		}

		public void RenameItem(int index) {
			this.Focus();
			this.BeginLabelEdit(index);
			//this._IsInRenameMode = true;
		}

		public void RenameSelectedItem() {
			//this.Focus();
			this.RenameItem(this.GetFirstSelectedItemIndex());
		}

		public void CutSelectedFiles() {
			foreach (var index in this.SelectedIndexes) {
				LVITEM item = new LVITEM();
				item.mask = LVIF.LVIF_STATE;
				item.stateMask = LVIS.LVIS_CUT;
				item.state = LVIS.LVIS_CUT;
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, index, ref item);
			}
			this._CuttedIndexes.AddRange(this.SelectedIndexes.ToArray());
			IntPtr dataObjPtr = IntPtr.Zero;
			//System.Runtime.InteropServices.ComTypes.IDataObject dataObject = GetIDataObject(this.SelectedItems.ToArray(), out dataObjPtr);
			var ddataObject = new F.DataObject();
			// Copy or Cut operation (5 = copy; 2 = cut)
			ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new byte[] { 2, 0, 0, 0 }));
			ddataObject.SetData("Shell IDList Array", true, this.SelectedItems.ToArray().CreateShellIDList());
			F.Clipboard.SetDataObject(ddataObject, true);
		}

		public void CopySelectedFiles() {
			IntPtr dataObjPtr = IntPtr.Zero;
			//System.Runtime.InteropServices.ComTypes.IDataObject dataObject = GetIDataObject(this.SelectedItems.ToArray(), out dataObjPtr);
			var ddataObject = new F.DataObject();
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
				var shellItemArray = dataObject.ToShellItemArray();
				var items = shellItemArray.ToArray();
				try {
					FOperationProgressSink sink = new FOperationProgressSink(view);
					IIFileOperation fo = new IIFileOperation(sink, handle, true);
					foreach (var item in items) {
						if (dropEffect == System.Windows.DragDropEffects.Copy)
							fo.CopyItem(item, this.CurrentFolder.ComInterface, String.Empty);
						else
							fo.MoveItem(item, this.CurrentFolder.ComInterface, null);
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

		void sink_OnOperation(object sender, OperationEventArgs e) {
			//var theNewItem = new ShellItem(e.Item);
			//this.InsertNewItem(theNewItem);
		}

		private void Do_Copy_OR_Move_Helper(bool Copy, ShellItem destination, IShellItem[] Items) {
			var handle = this.Handle;
			var thread = new Thread(() => {
				IIFileOperation fo = new IIFileOperation(handle);
				foreach (var item in Items) {
					if (Copy)
						fo.CopyItem(item, destination.ComInterface, null); //Might require String.Empty
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
			var thread = new Thread(() => {
				IShellItemArray shellItemArray = null;
				IShellItem[] items = null;
				if (((F.DataObject)dataObject).ContainsFileDropList()) {
					items = ((F.DataObject)dataObject).GetFileDropList().OfType<String>().Select(s => new ShellItem(s.ToShellParsingName()).ComInterface).ToArray();
				}
				else {
					shellItemArray = dataObject.ToShellItemArray();
					items = shellItemArray.ToArray();
				}
				try {
					IIFileOperation fo = new IIFileOperation(handle);
					foreach (var item in items) {
						if (Copy)
							fo.CopyItem(item, destination.ComInterface, String.Empty);
						else
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
				FOperationProgressSink sink = new FOperationProgressSink(view);
				IIFileOperation fo = new IIFileOperation(sink, handle, isRecycling);
				foreach (var item in this.SelectedItems.Select(s => s.ComInterface).ToArray()) {
					fo.DeleteItem(item);
				}
				fo.PerformOperations();
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		public void RenameShellItem(IShellItem item, String newName) {
			IIFileOperation fo = new IIFileOperation(true);
			fo.RenameItem(item, newName);
			fo.PerformOperations();
		}

		public void ResizeIcons(int value) {
			try {
				IconSize = value;
				//cache.Clear();
				ThumbnailsForCacheLoad.Clear();
				waitingThumbnails.Clear();
				foreach (var obj in this.Items) {
					obj.IsIconLoaded = false;
				}
				var il = new F.ImageList();
				il.ImageSize = new System.Drawing.Size(value, value);
				var ils = new F.ImageList();
				ils.ImageSize = new System.Drawing.Size(16, 16);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETICONSPACING, 0, (IntPtr)User32.MAKELONG(value + 28, value + 42));
			}
			catch (Exception) {
			}
		}

		/// <summary> Runs an application as an administrator. </summary>
		/// <param name="ExePath"> The path of the application. </param>
		public void RunExeAsAdmin(string ExePath) {
			Process.Start(new ProcessStartInfo {
				FileName = ExePath,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = String.Format("/env /user:Administrator \"{0}\"", ExePath),
			});
		}

		public void SelectAll() {
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED;
			item.state = LVIS.LVIS_SELECTED;
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, -1, ref item);
			this.Focus();
		}

		public void SelectItems(ShellItem[] ShellObjectArray) {
			this.DeSelectAllItems();
			foreach (ShellItem item in ShellObjectArray) {
				LVITEMINDEX lvii = new LVITEMINDEX();
				lvii.iItem = ItemsHashed[item];
				lvii.iGroup = this.GetGroupIndex(ItemsHashed[item]);
				LVITEM lvi = new LVITEM();
				lvi.mask = LVIF.LVIF_STATE;
				lvi.stateMask = LVIS.LVIS_SELECTED;
				lvi.state = LVIS.LVIS_SELECTED;
				User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMINDEXSTATE, ref lvii, ref lvi);
			}
			this.Focus();
		}

		public void SelectItemByIndex(int index, bool ensureVisisble = false, bool deselectOthers = false) {
			LVITEMINDEX lvii = new LVITEMINDEX();
			lvii.iItem = index;
			lvii.iGroup = this.GetGroupIndex(index);
			LVITEM lvi = new LVITEM();
			lvi.mask = LVIF.LVIF_STATE;
			lvi.stateMask = LVIS.LVIS_SELECTED;
			lvi.state = LVIS.LVIS_SELECTED;
			if (deselectOthers) {
				LVITEMINDEX lviid = new LVITEMINDEX();
				lviid.iItem = -1;
				lviid.iGroup = 0;
				LVITEM lviDeselect = new LVITEM();
				lviDeselect.mask = LVIF.LVIF_STATE;
				lviDeselect.stateMask = LVIS.LVIS_SELECTED;
				lviDeselect.state = 0;
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMINDEXSTATE, ref lviid, ref lviDeselect);
			}
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMINDEXSTATE, ref lvii, ref lvi);
			if (ensureVisisble)
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_ENSUREVISISBLE, index, 0);
			this.Focus();
		}

		/*
		public void DropHighLightIndex(int index, bool ensureVisisble = false) {
			LVITEM lvi = new LVITEM();
			lvi.mask = LVIF.LVIF_STATE;
			lvi.stateMask = LVIS.LVIS_DROPHILITED | LVIS.LVIS_SELECTED;
			lvi.state = LVIS.LVIS_DROPHILITED | LVIS.LVIS_SELECTED;
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, index, ref lvi);
			if (ensureVisisble)
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_ENSUREVISISBLE, index, 0);
			this.Focus();
		}
		*/

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
			}
			else if (this.Collumns.Count(s => s.pkey.fmtid == col.pkey.fmtid && s.pkey.pid == col.pkey.pid) == 0) {
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
			}
			else {
				// Set the column number that is to be sorted; default to ascending.
				this.LastSortedColumnIndex = colIndex;
				this.LastSortOrder = Order;
			}
			if (Order == SortOrder.Ascending) {
				this.Items = this.Items.Where(w => this.ShowHidden ? true : !w.IsHidden).OrderByDescending(o => o.IsFolder).ThenBy(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToList();
			}
			else {
				this.Items = this.Items.Where(w => this.ShowHidden ? true : !w.IsHidden).OrderByDescending(o => o.IsFolder).ThenByDescending(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToList();
			}

			var i = 0;
			this.ItemsHashed = this.Items.Distinct().ToDictionary(k => k, el => i++);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
			this.SetSortIcon(colIndex, Order);
			this.SelectItems(selectedItems);
		}

		/*
		/// <summary>
		/// Navigates the <see cref="ShellView"/> control forwards to the
		/// requested folder in the navigation history.
		/// </summary>
		///
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the
		/// <see cref="NavigateForward"/> method to implement a drop-down menu
		/// on a <b>Forward</b> button similar to the one in Windows Explorer,
		/// which will allow your users to return to a folder in the 'forward'
		/// navigation history.
		/// </remarks>
		///
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		///
		/// <exception cref="Exception">
		/// The requested folder is not present in the
		/// <see cref="ShellView"/>'s 'forward' history.
		/// </exception>
		public void NavigateForward(ShellItem folder) {
			History.MoveForward(folder);
			CurrentFolder = folder;
		}
		*/

		/*
		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the next folder
		/// in the navigation history.
		/// </summary>
		///
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateForward"/>
		/// method to implement a <b>Forward</b> button similar to the one
		/// in Windows Explorer, allowing your users to return to the next
		/// folder in the navigation history after navigating backward.
		/// </para>
		///
		/// <para>
		/// Use the <see cref="CanNavigateForward"/> property to determine
		/// whether the navigation history is available and contains a folder
		/// located after the current one.  This property is useful, for
		/// example, to change the enabled state of a <b>Forward</b> button
		/// when the ShellView control navigates to or leaves the end of the
		/// navigation history.
		/// </para>
		/// </remarks>
		///
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate forwards through.
		/// </exception>
		public void NavigateForward() {
			CurrentFolder = History.MoveForward();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}
		*/

		/*
		/// <summary>
		/// Navigates the <see cref="ShellView"/> control backwards to the
		/// requested folder in the navigation history.
		/// </summary>
		///
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a drop-down menu on a <b>Back</b> button similar
		/// to the one in Windows Explorer, which will allow your users to return
		/// to a previous folder in the navigation history.
		/// </remarks>
		///
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		///
		/// <exception cref="Exception">
		/// The requested folder is not present in the
		/// <see cref="ShellView"/>'s 'back' history.
		/// </exception>
		public void NavigateBack(ShellItem folder) {
			History.MoveBack(folder);
			CurrentFolder = folder;
		}
		*/

		/*
		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the previous folder
		/// in the navigation history.
		/// </summary>
		///
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a <b>Back</b> button similar to the one in
		/// Windows Explorer, which will allow your users to return to a
		/// previous folder in the navigation history.
		/// </para>
		///
		/// <para>
		/// Use the <see cref="CanNavigateBack"/> property to determine whether
		/// the navigation history is available and contains a previous page.
		/// This property is useful, for example, to change the enabled state
		/// of a Back button when the ShellView control navigates to or leaves
		/// the beginning of the navigation history.
		/// </para>
		/// </remarks>
		///
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate backwards through.
		/// </exception>
		public void NavigateBack() {
			CurrentFolder = History.MoveBack();
			//RecreateShellView();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}
		*/


		/// <summary>
		/// Navigate to a folder, set it as the current folder and optionally save the folder's settings to the database.
		/// </summary>
		/// <param name="destination">The folder you want to navigate to.</param>
		/// <param name="SaveFolderSettings">Should the folder's settings be saved?</param>
		/// <param name="isInSameTab"></param>
		public void Navigate_Full(ShellItem destination, bool SaveFolderSettings, Boolean isInSameTab = false, bool refresh = false) {
			if (SaveFolderSettings) {
				SaveSettingsToDatabase(this.CurrentFolder);
			}

			if (destination == null || !destination.IsFolder) return;
			Navigate(destination, isInSameTab, refresh);
		}

		/// <summary>
		/// Navigate to a folder.
		/// </summary>
		/// <param name="destination">The folder you want to navigate to.</param>
		/// <param name="isInSameTab"></param>
		private void Navigate(ShellItem destination, Boolean isInSameTab = false, bool refresh = false) {
			if (!refresh)
				this.OnNavigating(new NavigatingEventArgs(destination, isInSameTab));

			//Unregister notifications and clear all collections
			this.Notifications.UnregisterChangeNotify();
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
			if (ToolTip == null)
				this.ToolTip = new ToolTip();

			var folderSettings = new FolderSettings();
			var isThereSettings = LoadSettingsFromDatabase(destination, out folderSettings);

			if (isThereSettings) {
				if (folderSettings.Columns != null) {
					this.RemoveAllCollumns();
					foreach (var collumn in folderSettings.Columns.Elements()) {
						var theColumn = this.AllAvailableColumns.Where(w => w.ID == collumn.Attribute("ID").Value).Single();
						if (this.Collumns.Count(c => c.ID == theColumn.ID) == 0) {
							if (collumn.Attribute("Width").Value != "0") {
								int width = Convert.ToInt32(collumn.Attribute("Width").Value);
								theColumn.Width = width;
							}
							this.Collumns.Add(theColumn);
							var column = theColumn.ToNativeColumn(folderSettings.View == ShellViewStyle.Details);
							User32.SendMessage(this.LVHandle, Interop.MSG.LVM_INSERTCOLUMN, this.Collumns.Count - 1, ref column);
							if (folderSettings.View != ShellViewStyle.Details) {
								this.AutosizeColumn(this.Collumns.Count - 1, -2);
							}
						}
						else {
							int colIndex = this.Collumns.IndexOf(this.Collumns.SingleOrDefault(s => s.ID == theColumn.ID));
							this.Collumns.RemoveAt(colIndex);
							User32.SendMessage(this.LVHandle, Interop.MSG.LVM_DELETECOLUMN, colIndex, 0);
							if (collumn.Attribute("Width").Value != "0") {
								int width = Convert.ToInt32(collumn.Attribute("Width").Value);
								theColumn.Width = width;
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

				IntPtr headerhandle = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETHEADER, 0, 0);

				for (int i = 0; i < this.Collumns.Count; i++) {
					this.Collumns[i].SetSplitButton(headerhandle, i);
				}
			}
			else {
				this.RemoveAllCollumns();
				this.AddDefaultColumns(false, true);
			}

			//TODO: Figure out if the folder is actually sorted and we are not incorrectly setting the sort icon.
			this.SetSortIcon(folderSettings.SortColumn, folderSettings.SortOrder == SortOrder.None ? SortOrder.Ascending : folderSettings.SortOrder);

			this.View = isThereSettings ? folderSettings.View : ShellViewStyle.Medium;
			if (folderSettings.View == ShellViewStyle.Details || folderSettings.View == ShellViewStyle.SmallIcon || folderSettings.View == ShellViewStyle.List)
				ResizeIcons(16);

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

			if (isThereSettings) {

				SetSortCollumn(folderSettings.SortColumn, folderSettings.SortOrder, false);
			}
			else if (destination.ParsingName.ToLowerInvariant() == KnownFolders.Computer.ParsingName.ToLowerInvariant()) {
				this.Items = this.Items.ToList();
			}
			else {
				this.Items = this.Items.OrderByDescending(o => o.IsFolder).ThenBy(o => o.DisplayName).ToList();
			}

			if (!isThereSettings) {
				var i = 0;
				this.ItemsHashed = this.Items.Distinct().ToDictionary(k => k, el => i++);
			}

			Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

			if (!isThereSettings) {
				this.LastSortedColumnIndex = 0;
				this.LastSortOrder = SortOrder.Ascending;
			}


			Notifications.RegisterChangeNotify(this.Handle, destination, true);

			if (!isThereSettings)
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
			if (IsGroupsEnabled) {
				this.Groups.Clear();
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_REMOVEALLGROUPS, 0, 0);
				GenerateGroupsFromColumn(this.Collumns.First());
			}

			var NavArgs = new NavigatedEventArgs(destination, this.CurrentFolder, isInSameTab);
			this.CurrentFolder = destination;
			if (!refresh)
				this.OnNavigated(NavArgs);

			//AutosizeAllColumns(this.View != ShellViewStyle.Details ? -2 : -1);

			//if (this.View != ShellViewStyle.Details)
			//	AutosizeAllColumns(-2);
			//else
			//	AutosizeAllColumns(-1);
			this.Focus();
		}

		/*
		private static int Compare(ShellItem x, ShellItem y) {
			return String.Compare(x.DisplayName, y.DisplayName);
		}
		*/

		public void DisableGroups() {
			this.Groups.Clear();
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_REMOVEALLGROUPS, 0, 0);
			const int LVM_ENABLEGROUPVIEW = 0x1000 + 157;
			var x = User32.SendMessage(this.LVHandle, LVM_ENABLEGROUPVIEW, 0, 0);
			//System.Diagnostics.Debug.WriteLine(x);

			const int LVM_SETOWNERDATACALLBACK = 0x10BB;
			x = User32.SendMessage(this.LVHandle, LVM_SETOWNERDATACALLBACK, 0, 0);
			//IsGroupsEnabled = false;
			this.LastGroupCollumn = null;

			//Aaron Campf
			//this.SetSortIcon(this.LastSortedColumnIndex, this.LastSortOrder);
		}

		public void EnableGroups() {
			var g = new VirtualGrouping(this);

			const int LVM_SETOWNERDATACALLBACK = 0x10BB;
			IntPtr ptr = Marshal.GetComInterfaceForObject(g, typeof(IOwnerDataCallback));
			var x = User32.SendMessage(this.LVHandle, LVM_SETOWNERDATACALLBACK, ptr, IntPtr.Zero);
			Marshal.Release(ptr);

			const int LVM_ENABLEGROUPVIEW = 0x1000 + 157;

			x = (int)User32.SendMessage(this.LVHandle, LVM_ENABLEGROUPVIEW, 1, 0);
			//IsGroupsEnabled = true;
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
				ListViewGroupEx testgrn = new ListViewGroupEx();
				testgrn.Items = this.Items.Where(w => w.DisplayName.ToUpperInvariant().First() >= Char.Parse("0") && w.DisplayName.ToUpperInvariant().First() <= Char.Parse("9")).ToArray();
				testgrn.Header = String.Format("0 - 9 ({0})", testgrn.Items.Count());
				testgrn.Index = reversed ? i-- : i++;
				this.Groups.Add(testgrn);

				ListViewGroupEx testgr = new ListViewGroupEx();
				testgr.Items = this.Items.Where(w => w.DisplayName.ToUpperInvariant().First() >= Char.Parse("A") && w.DisplayName.ToUpperInvariant().First() <= Char.Parse("H")).ToArray();
				testgr.Header = String.Format("A - H ({0})", testgr.Items.Count());
				testgr.Index = reversed ? i-- : i++;
				this.Groups.Add(testgr);

				ListViewGroupEx testgr2 = new ListViewGroupEx();
				testgr2.Items = this.Items.Where(w => w.DisplayName.ToUpperInvariant().First() >= Char.Parse("I") && w.DisplayName.ToUpperInvariant().First() <= Char.Parse("P")).ToArray();
				testgr2.Header = String.Format("I - P ({0})", testgr2.Items.Count());
				testgr2.Index = reversed ? i-- : i++;
				this.Groups.Add(testgr2);

				ListViewGroupEx testgr3 = new ListViewGroupEx();
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
			}
			else if (col.CollumnType == typeof(long)) {
				var j = reversed ? 7 : 0;
				ListViewGroupEx uspec = new ListViewGroupEx();
				uspec.Items = this.Items.Where(w => w.IsFolder).ToArray();
				uspec.Header = String.Format("Unspecified ({0})", uspec.Items.Count());
				uspec.Index = reversed ? j-- : j++;
				this.Groups.Add(uspec);

				ListViewGroupEx testgrn = new ListViewGroupEx();
				testgrn.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) == 0 && !w.IsFolder).ToArray();
				testgrn.Header = String.Format("Empty ({0})", testgrn.Items.Count());
				testgrn.Index = reversed ? j-- : j++;
				this.Groups.Add(testgrn);

				ListViewGroupEx testgr = new ListViewGroupEx();
				testgr.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 0 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 10 * 1024).ToArray();
				testgr.Header = String.Format("Very Small ({0})", testgr.Items.Count());
				testgr.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr);

				ListViewGroupEx testgr2 = new ListViewGroupEx();
				testgr2.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 10 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 100 * 1024).ToArray();
				testgr2.Header = String.Format("Small ({0})", testgr2.Items.Count());
				testgr2.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr2);

				ListViewGroupEx testgr3 = new ListViewGroupEx();
				testgr3.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 100 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 1 * 1024 * 1024).ToArray();
				testgr3.Header = String.Format("Medium ({0})", testgr3.Items.Count());
				testgr3.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr3);

				ListViewGroupEx testgr4 = new ListViewGroupEx();
				testgr4.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 1 * 1024 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 16 * 1024 * 1024).ToArray();
				testgr4.Header = String.Format("Big ({0})", testgr4.Items.Count());
				testgr4.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr4);

				ListViewGroupEx testgr5 = new ListViewGroupEx();
				testgr5.Items = this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) > 16 * 1024 * 1024 && Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(long)).Value) <= 128 * 1024 * 1024).ToArray();
				testgr5.Header = String.Format("Huge ({0})", testgr5.Items.Count());
				testgr5.Index = reversed ? j-- : j++;
				this.Groups.Add(testgr5);

				ListViewGroupEx testgr6 = new ListViewGroupEx();
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
			}
			else {
				var groups = this.Items.GroupBy(k => k.GetPropertyValue(col.pkey, typeof(String)).Value, e => e).OrderBy(o => o.Key);
				var i = reversed ? groups.Count() - 1 : 0;
				foreach (var group in groups) {
					var groupItems = group.Select(s => s).ToArray();
					ListViewGroupEx gr = new ListViewGroupEx();
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

			//TODO: Check the following Change
			//Note:	User:	Aaron Campf	Date: 8/9/2014	Message: The below code did not set the icon properly
			/*
			if (this.Collumns[this.LastSortedColumnIndex] == col || reversed)
				this.SetSortIcon(this.Collumns.IndexOf(col), this.LastGroupOrder);
			*/
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
			LVITEMINDEX lvi = new LVITEMINDEX();
			lvi.iItem = -1;
			lvi.iGroup = 0;
			User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
			if (lvi.iItem == -1 || this.Items.Count < lvi.iItem) return null;
			return this.Items[lvi.iItem];
		}

		public int GetFirstSelectedItemIndex() {
			LVITEMINDEX lvi = new LVITEMINDEX();
			lvi.iItem = -1;
			lvi.iGroup = 0;
			User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
			if (lvi.iItem == -1) return -1;
			return lvi.iItem;
		}

		public void _ShieldLoadingThreadRun() {
			while (true) {
				//Application.DoEvents();

				//while (shieldQueue.Count == 0)
				//{
				//	Thread.Sleep(5);
				//}
				resetEvent.WaitOne();
				Thread.Sleep(4);
				try {
					var index = shieldQueue.Dequeue();
					//Application.DoEvents();
					var shoTemp = Items[index];
					ShellItem sho = !(shoTemp.IsNetDrive || shoTemp.IsNetworkPath) && shoTemp.ParsingName.StartsWith("::") ? shoTemp : new ShellItem(shoTemp.ParsingName);

					var shieldOverlay = 0;
					if ((sho.GetShield() & IExtractIconPWFlags.GIL_SHIELD) != 0) {
						shieldOverlay = ShieldIconIndex;
					}

					shoTemp.IsShielded = shieldOverlay;
					if (shieldOverlay > 0) {
						this.RedrawItem(index);
					}

					//Application.DoEvents();
				}
				catch {
				}
			}
		}

		public void _OverlaysLoadingThreadRun() {
			while (true) {
				//Application.DoEvents();

				//while (overlayQueue.Count == 0)
				//{
				//	Thread.Sleep(5);
				//}
				Thread.Sleep(3);
				try {
					var index = overlayQueue.Dequeue();

					//if (this.Cancel)
					//	continue;
					//Application.DoEvents();
					var shoTemp = Items[index];
					ShellItem sho = !(shoTemp.IsNetDrive || shoTemp.IsNetworkPath) && shoTemp.ParsingName.StartsWith("::") ? shoTemp : new ShellItem(shoTemp.ParsingName);

					int overlayIndex = 0;
					small.GetIconIndexWithOverlay(sho.Pidl, out overlayIndex);
					shoTemp.OverlayIconIndex = overlayIndex;
					if (overlayIndex > 0)
						RedrawItem(index);
					resetEvent.WaitOne();
					//Application.DoEvents();
				}
				catch (Exception) {
				}
			}
		}

		public void _IconsLoadingThreadRun() {
			while (true) {
				//resetEvent.WaitOne();
				Thread.Sleep(1);
				//Application.DoEvents();

				try {
					var index = waitingThumbnails.Dequeue();
					//if (User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_ISITEMVISIBLE, index, 0) == IntPtr.Zero)
					//	continue;
					//var itemBounds = new User32.RECT();
					//LVITEMINDEX lvi = new LVITEMINDEX();
					//lvi.iItem = index;
					//lvi.iGroup = this.GetGroupIndex(index);
					//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
					//Rectangle r = new Rectangle(itemBounds.Left, itemBounds.Top, itemBounds.Right - itemBounds.Left, itemBounds.Bottom - itemBounds.Top);
					//if (!r.IntersectsWith(this.ClientRectangle))
					//	continue;
					resetEvent.WaitOne();
					var sho = Items[index];
					ShellItem temp = !(sho.IsNetDrive || sho.IsNetworkPath) && sho.ParsingName.StartsWith("::") ? sho : new ShellItem(sho.ParsingName);

					var icon = temp.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
					if (icon != null) {
						sho.IsIconLoaded = true;
						//sho.IsNeedRefreshing = false;
						//if (!cache.ContainsKey(index)) {
						//	cache.TryAdd(index, new Bitmap(icon));
						//	this.RedrawItem(index);
						//	icon.Dispose();
						//	icon = null;
						//}
						//else {
						//	cache[index] = new Bitmap(icon);
						//	this.RedrawItem(index);
						//	icon.Dispose();
						//	icon = null;
						//}
						this.RedrawItem(index);
					}

					//Application.DoEvents();
				}
				catch {
				}
			}
		}

		public void _IconCacheLoadingThreadRun() {
			while (true) {
				resetEvent.WaitOne();
				Thread.Sleep(1);
				try {
					var index = ThumbnailsForCacheLoad.Dequeue();
					if (index >= Items.Count) {
						continue;
					}
					var sho = Items[index];
					var thumb = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.Default);
					sho.IsThumbnailLoaded = true;
					sho.IsNeedRefreshing = false;
					if (thumb != null) {
						this.RedrawItem(index);
						thumb.Dispose();
						thumb = null;
					}
				}
				catch {
				}
			}
		}

		public void _UpdateSubitemValuesThreadRun() {
			while (true) {
				resetEvent.WaitOne();
				Thread.Sleep(1);
				var index = ItemsForSubitemsUpdate.Dequeue();
				//if (this.Cancel)
				//	continue;
				try {
					if (User32.SendMessage(this.LVHandle, Interop.MSG.LVM_ISITEMVISIBLE, index.Item1, 0) != IntPtr.Zero) {
						//	continue;
						//if (this.Cancel)
						//	continue;
						//Application.DoEvents();

						var currentItem = Items[index.Item1];
						ShellItem temp = null;
						if (!(currentItem.IsNetDrive || currentItem.IsNetworkPath) && !currentItem.ParsingName.StartsWith("::")) {
							temp = new ShellItem(currentItem.ParsingName);
						}
						else {
							temp = currentItem;
						}
						int hash = currentItem.GetHashCode();
						IShellItem2 isi2 = (IShellItem2)temp.ComInterface;
						var pvar = new PropVariant();
						var pk = index.Item3;
						Guid guid = new Guid(InterfaceGuids.IPropertyStore);
						IPropertyStore propStore = null;
						isi2.GetPropertyStore(GetPropertyStoreOptions.Default, ref guid, out propStore);
						if (propStore != null && propStore.GetValue(ref pk, pvar) == HResult.S_OK) {
							//if (SubItemValues.ToArray().Count(c => c.Item1 == hash && c.Item2.fmtid == pk.fmtid && c.Item2.pid == pk.pid) == 0) {
							if (!SubItemValues.Any(c => c.Item1 == hash && c.Item2.fmtid == pk.fmtid && c.Item2.pid == pk.pid)) {
								SubItemValues.Add(new Tuple<int, PROPERTYKEY, object>(hash, pk, pvar.Value));
								this.RedrawItem(index.Item1);
							}
							pvar.Dispose();
						}
					}
				}
				catch {
					//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_UPDATE, index.Item1, 0);
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

		/*
		public static void StartCompartabilityWizzard() {
			Process.Start("msdt.exe", "-id PCWDiagnostic");
		}
		*/

		public void CleanupDrive() {
			string DriveLetter = "";
			if (SelectedItems.Count > 0)
				DriveLetter = Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName) ? SelectedItems[0].ParsingName : this.CurrentFolder.ParsingName;
			else
				DriveLetter = this.CurrentFolder.ParsingName;

			Process.Start("Cleanmgr.exe", "/d" + DriveLetter.Replace(":\\", ""));
		}

		/*
		public string CreateNewFolder() {
			string name = "New Folder";
			int suffix = 0;

			do {
				//TODO: Check
				if (this.CurrentFolder.Parent == null) {
					name = String.Format("{0}\\New Folder ({1})", this.CurrentFolder.ParsingName, ++suffix);
				}
				else if (this.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
					ShellLibrary lib = ShellLibrary.Load(this.CurrentFolder.DisplayName, true);
					name = String.Format("{0}\\New Folder ({1})", lib.DefaultSaveFolder, ++suffix);
					lib.Close();
				}
				else {
					name = String.Format("{0}\\New Folder ({1})", this.CurrentFolder.ParsingName, ++suffix);
				}

				/*
				if (this.CurrentFolder.Parent != null) {
					if (this.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
						ShellLibrary lib = ShellLibrary.Load(this.CurrentFolder.DisplayName, true);
						name = String.Format("{0}\\New Folder ({1})", lib.DefaultSaveFolder, ++suffix);
						lib.Close();
					}
					else
						name = String.Format("{0}\\New Folder ({1})", this.CurrentFolder.ParsingName, ++suffix);
				}
				else
					name = String.Format("{0}\\New Folder ({1})", this.CurrentFolder.ParsingName, ++suffix);
				/
			} while (Directory.Exists(name) || File.Exists(name));

			ERROR result = Shell32.SHCreateDirectory(IntPtr.Zero, name);

			switch (result) {
				case ERROR.FILE_EXISTS:
				case ERROR.ALREADY_EXISTS:
					throw new IOException("The directory already exists");
				case ERROR.BAD_PATHNAME:
					throw new IOException("Bad pathname");
				case ERROR.FILENAME_EXCED_RANGE:
					throw new IOException("The filename is too long");
			}
			return name;
		}
		*/

		public string CreateNewFolder(string name = "New Folder") {
			int suffix = 0;
			string endname = name;

			do {
				//TODO: Check
				if (this.CurrentFolder.Parent == null) {
					endname = String.Format("{0}\\" + name + " ({1})", this.CurrentFolder.ParsingName, ++suffix);
				}
				else if (this.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
					ShellLibrary lib = ShellLibrary.Load(this.CurrentFolder.DisplayName, true);
					endname = String.Format("{0}\\" + name + " ({1})", lib.DefaultSaveFolder, ++suffix);
					lib.Close();
				}
				else {
					endname = String.Format("{0}\\" + name + " ({1})", this.CurrentFolder.ParsingName, ++suffix);
				}

				/*
				if (this.CurrentFolder.Parent != null) {
					if (this.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
						ShellLibrary lib = ShellLibrary.Load(this.CurrentFolder.DisplayName, true);
						endname = String.Format("{0}\\" + name + " ({1})", lib.DefaultSaveFolder, ++suffix);
						lib.Close();
					}
					else
						endname = String.Format("{0}\\" + name + " ({1})", this.CurrentFolder.ParsingName, ++suffix);
				}
				else
					endname = String.Format("{0}\\" + name + " ({1})", this.CurrentFolder.ParsingName, ++suffix);
				*/
			} while (Directory.Exists(endname) || File.Exists(endname));

			ERROR result = Shell32.SHCreateDirectory(IntPtr.Zero, endname);

			switch (result) {
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
			}
			catch {
			}
			if (lib != null) {
				do {
					endname = String.Format(name + "({0})", ++suffix);
					try {
						lib = ShellLibrary.Load(endname, true);
					}
					catch {
						lib = null;
					}
				} while (lib != null);
			}

			return new ShellLibrary(endname, false);

			//return libcreate.GetDisplayName(DisplayNameType.Default);
		}

		/*
		[Obsolete("Never Used", true)]
		private void SetLVBackgroundImage(Bitmap bitmap) {
			Helpers.SetListViewBackgroundImage(this.LVHandle, bitmap);
		}
		*/

		public void SetFolderIcon(string wszPath, string wszExpandedIconPath, int iIcon) {
			HResult hr;

			Shell32.LPSHFOLDERCUSTOMSETTINGS fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS();
			fcs.dwSize = (uint)Marshal.SizeOf(fcs);
			fcs.dwMask = Shell32.FCSM_ICONFILE;
			fcs.pszIconFile = wszExpandedIconPath.Replace(@"\\", @"\");
			fcs.cchIconFile = 0;
			fcs.iIconIndex = iIcon;

			// Set the folder icon
			hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath.Replace(@"\\", @"\"), Shell32.FCS_FORCEWRITE);

			if (hr == HResult.S_OK) {
				// Update the icon cache
				SHFILEINFO sfi = new SHFILEINFO();
				var res = Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(wszPath), 0, out sfi, (int)Marshal.SizeOf(sfi), SHGFI.IconLocation);
				int iIconIndex = Shell32.Shell_GetCachedImageIndex(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0);
				Shell32.SHUpdateImage(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0, iIconIndex);
				//RefreshExplorer();
				Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEIMAGE,
				Shell32.HChangeNotifyFlags.SHCNF_DWORD | Shell32.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, IntPtr.Zero, (IntPtr)sfi.iIcon);
			}

			Items[this.GetFirstSelectedItemIndex()] = new ShellItem(wszPath);
			//this.UpdateItem(this.SelectedIndexes[0]);
			this.RefreshItem(this.SelectedIndexes[0]);
			//return hr;
		}

		public HResult ClearFolderIcon(string wszPath) {
			HResult hr;

			Shell32.LPSHFOLDERCUSTOMSETTINGS fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS();
			fcs.dwSize = (uint)Marshal.SizeOf(fcs);
			fcs.dwMask = Shell32.FCSM_ICONFILE;
			hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath, Shell32.FCS_FORCEWRITE);
			if (hr == HResult.S_OK) {
				// Update the icon cache
				SHFILEINFO sfi = new SHFILEINFO();
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
			if (SelectedItems.Count > 0) {
				if (Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName)) {
					DriveLetter = SelectedItems[0].ParsingName;
				}
				else {
					DriveLetter = this.CurrentFolder.ParsingName;
				}
			}
			else {
				DriveLetter = this.CurrentFolder.ParsingName;
			}
			Process.Start(Path.Combine(Environment.SystemDirectory, "dfrgui.exe"), "/u /v " + DriveLetter.Replace("\\", ""));
		}

		public void DeSelectAllItems() {
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED;
			item.state = 0;
			User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, -1, ref item);
			this.Focus();
		}

		private void DeselectItemByIndex(int index) {
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED;
			item.state = 0;
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
					var mainWin = System.Windows.Application.Current.MainWindow;
					if (mainWin.IsActive || !isActiveCheck) {
						if (IsFocusAllowed && this.Bounds.Contains(Cursor.Position)) {
							var res = User32.SetFocus(this.LVHandle);
							//this._IsInRenameMode = false;
						}
					}
				}
			}
			//On Exception do nothing (usually it happens on app exit)
			catch { }

			//}
		}

		public void FormatDrive(IntPtr handle) {
			string DriveLetter = "";
			if (SelectedItems.Count > 0)
				DriveLetter = Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName) ? SelectedItems[0].ParsingName : this.CurrentFolder.ParsingName;
			else
				DriveLetter = this.CurrentFolder.ParsingName;

			Shell32.FormatDrive(handle, DriveLetter);
		}

		public int GetItemsCount() { return this.Items.Count; }

		public int GetSelectedCount() {
			return (int)User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETSELECTEDCOUNT, 0, 0);
		}

		public void InvertSelection() {
			int itemCount = (int)User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMCOUNT, 0, 0);

			for (int n = 0; n < itemCount; ++n) {
				var state = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_GETITEMSTATE, n, LVIS.LVIS_SELECTED);

				LVITEM item_new = new LVITEM();
				item_new.mask = LVIF.LVIF_STATE;
				item_new.stateMask = LVIS.LVIS_SELECTED;
				item_new.state = (state & LVIS.LVIS_SELECTED) == LVIS.LVIS_SELECTED ? 0 : LVIS.LVIS_SELECTED;
				User32.SendMessage(this.LVHandle, Interop.MSG.LVM_SETITEMSTATE, n, ref item_new);
			}
			this.Focus();
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
			//bool found = false;
			int i = startindex;


			while (true) {
				//TODO: Check
				if (i >= Items.Count) {
					//found = true;
					//i = -1;
					return -1;
				}
				else if (Items[i].GetDisplayName(SIGDN.NORMALDISPLAY).ToUpperInvariant().StartsWith(search.ToUpperInvariant())) {
					//found = true;
					return i;
				}
				else {
					i++;
				}
			}


			/*
			while (!found) {
				//TODO: Check
				if (i >= Items.Count) {
					found = true;
					i = -1;
				}
				else if (Items[i].GetDisplayName(SIGDN.NORMALDISPLAY).ToUpperInvariant().StartsWith(search.ToUpperInvariant())) {
					found = true;
				}
				else {
					i++;
				}
			}

			return i;
			*/
		}

		/*
		[Obsolete("Never Used", true)]
		private string GetStringFromAcceptedKeyCodeString(string str) {
			str = str.ToUpperInvariant();
			if (str.Length == 1) {
				return str;
			}
			else if (str == "SPACE") {
				return " ";
			}
			else if (str == "OEMPERIOD") {
				return ".";
			}
			else if (str == "OEMMINUS") {
				return "-";
			}
			else {
				return "";
			}
		}
		*/
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
			//User32.InvalidateRect(this.LVHandle, ref rect, false);
			User32.RedrawWindow(this.LVHandle, ref rect, IntPtr.Zero, 0x0001/*RDW_INVALIDATE*/| 0x100);
			//User32.UpdateWindow(this.LVHandle);
		}

		internal void OnGotFocus() {
			if (GotFocus != null) {
				GotFocus(this, EventArgs.Empty);
			}
		}

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
			}
			else {
				resultString += "-";
			}
			if (isDirectory) {
				resultString += "D";
			}
			else {
				resultString += "-";
			}
			if (isHidden) {
				resultString += "H";
			}
			else {
				resultString += "-";
			}
			if (isReadOnly) {
				resultString += "R";
			}
			else {
				resultString += "-";
			}
			if (isSystem) {
				resultString += "S";
			}
			else {
				resultString += "-";
			}
			if (isTemp) {
				resultString += "T";
			}
			else {
				resultString += "-";
			}
			return resultString;
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

				//var sql = "";
				var Reader = command1.ExecuteReader();
				if (Reader.Read()) {
					var Values = Reader.GetValues();
					if (Values.Count > 0) {
						result = true;
						var view = Values.GetValues("View").FirstOrDefault();
						var lastSortedColumnIndex = Values.GetValues("LastSortedColumn").FirstOrDefault();
						var lastSortOrder = Values.GetValues("LastSortOrder").FirstOrDefault();
						if (view != null) {
							folderSetting.View = (ShellViewStyle)Enum.Parse(typeof(ShellViewStyle), view);
						}
						if (lastSortedColumnIndex != null) {
							folderSetting.SortColumn = Int32.Parse(lastSortedColumnIndex);
							folderSetting.SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), lastSortOrder);
						}

						/* New Stuff */
						var collumns = Values.GetValues("Columns").FirstOrDefault();

						folderSetting.Columns = collumns != null ? XElement.Parse(collumns) : null;
						//this.Collumns
						this.LastGroupOrder = Values["LastGroupOrder"] == "Ascending" ? SortOrder.Ascending : SortOrder.Descending;
					}
				}

				Reader.Close();
			}
			catch (Exception) {
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
									SET Path = @Path, LastSortOrder = @LastSortOrder, LastGroupOrder = @LastGroupOrder, LastGroupCollumn = @LastGroupCollumn, View = @View, LastSortedColumn = @LastSortedColumn, Columns = @Columns
									WHERE Path = @Path"
									:
									@"INSERT into foldersettings (Path, LastSortOrder, LastGroupOrder, LastGroupCollumn, View, LastSortedColumn, Columns)
									VALUES (@Path, @LastSortOrder, @LastGroupOrder, @LastGroupCollumn, @View, @LastSortedColumn, @Columns)";


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

				/*New Values */

				{ "Columns", Columns_XML.ToString()}
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

		private void AutosizeColumn(int index, int autosizeStyle) {
			User32.SendMessage(this.LVHandle, LVM.SETCOLUMNWIDTH, index, autosizeStyle);
		}
		public void AutosizeAllColumns(int autosizeParam) {
			this.SuspendLayout();
			for (int i = 0; i < this.Collumns.Count; i++) {
				AutosizeColumn(i, autosizeParam);
			}
			this.ResumeLayout();
		}

		/// <summary>
		/// This is only to be used in SetSortCollumn(...)
		/// </summary>
		/// <param name="columnIndex"></param>
		/// <param name="order"></param>
		private void SetSortIcon(int columnIndex, SortOrder order) {
			//TODO: Consider Merging this into SetSortCollumn(...)

			IntPtr columnHeader = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETHEADER, 0, 0);
			for (int columnNumber = 0; columnNumber <= this.Collumns.Count - 1; columnNumber++) {
				var item = new HDITEM {
					mask = HDITEM.Mask.Format
				};

				if (User32.SendMessage(columnHeader, BExplorer.Shell.Interop.MSG.HDM_GETITEM, columnNumber, ref item) == IntPtr.Zero) {
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
				}
				else {
					item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
				}

				if (User32.SendMessage(columnHeader, BExplorer.Shell.Interop.MSG.HDM_SETITEM, columnNumber, ref item) == IntPtr.Zero) {
					throw new Win32Exception();
				}
			}
		}

	}
}