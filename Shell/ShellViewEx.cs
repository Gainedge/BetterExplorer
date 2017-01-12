using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.DropTargetHelper;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;
using DPoint = System.Drawing.Point;

using F = System.Windows.Forms;

using SQLite = System.Data.SQLite;

namespace BExplorer.Shell {
  #region Substructures and classes

  /// <summary> Specifies how list items are displayed in a <see cref="ShellView" /> control. </summary>
  public enum ShellViewStyle {
    /// <summary> Items appear in a grid and icon size is 256x256 </summary>
    ExtraLargeIcon = 256,

    /// <summary> Items appear in a grid and icon size is 96x96 </summary>
    LargeIcon = 96,

    /// <summary> Each item appears as a full-sized icon with a label below it. </summary>
    Medium = 48,

    /// <summary> Each item appears as a small icon with a label to its right. </summary>
    SmallIcon = 15,

    /// <summary>
    /// Each item appears as a small icon with a label to its right. Items are arranged in columns.
    /// </summary>
    List = 14,

    /// <summary>
    /// Each item appears on a separate line with further information about each item arranged
    /// in columns. The left-most column contains a small icon and label.
    /// </summary>
    Details = 16,

    /// <summary> Each item appears with a thumbnail picture of the file's content. </summary>
    Thumbnail = 0,

    /// <summary>
    /// Each item appears as a full-sized icon with the item label and file information to the
    /// right of it.
    /// </summary>
    Tile = 47,

    /// <summary>
    /// Each item appears in a thumbstrip at the bottom of the control, with a large preview of
    /// the selected item appearing above.
    /// </summary>
    Thumbstrip = 46,

    /// <summary> Each item appears in a item that occupy the whole view width </summary>
    Content = 45,
  }

  public class RenameEventArgs : EventArgs {
    public Int32 ItemIndex { get; private set; }

    public RenameEventArgs(Int32 itemIndex) {
      this.ItemIndex = itemIndex;
    }
  }

  public class CollumnsChangedArgs : EventArgs {
    public Boolean IsRemove { get; set; }

    public CollumnsChangedArgs(Boolean isRemove) {
      this.IsRemove = isRemove;
    }
  }

  public class ListViewColumnDropDownArgs : EventArgs {
    public Int32 ColumnIndex { get; set; }
    public DPoint ActionPoint { get; set; }

    public ListViewColumnDropDownArgs(Int32 colIndex, DPoint pt) {
      this.ColumnIndex = colIndex;
      this.ActionPoint = pt;
    }
  }

  public class NavigatedEventArgs : EventArgs, IDisposable {
    /// <summary> The folder that is navigated to. </summary>
    public IListItemEx Folder { get; set; }

    public IListItemEx OldFolder { get; set; }
    public Boolean isInSameTab { get; set; }

    public void Dispose() {
        Folder?.Dispose();
        Folder = null;
        OldFolder?.Dispose();
        OldFolder = null;
    }

    public NavigatedEventArgs(IListItemEx folder, IListItemEx old) {
      Folder = folder;
      OldFolder = old;
    }

    public NavigatedEventArgs(IListItemEx folder, IListItemEx old, Boolean isInSame) {
      Folder = folder;
      OldFolder = old;
      isInSameTab = isInSame;
    }
  }

  /// <summary> Provides information for the <see cref="ShellView.Navigating" /> event. </summary>
  public class NavigatingEventArgs : EventArgs, IDisposable {
    /// <summary> The folder being navigated to. </summary>
    public IListItemEx Folder { get; private set; }

    public Boolean IsNavigateInSameTab { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigatingEventArgs"/> class.
    /// </summary>
    /// <param name="folder">The folder being navigated to.</param>
    /// <param name="isInSameTab"></param>
    public NavigatingEventArgs(IListItemEx folder, Boolean isInSameTab) {
      Folder = folder;
      IsNavigateInSameTab = isInSameTab;
    }

    public void Dispose() {
        Folder = null;
    }
  }

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

    public IListItemEx PreviousItem { get; private set; }

    public IListItemEx NewItem { get; private set; }

    public Int32 NewItemIndex { get; private set; }

    public ItemUpdatedEventArgs(ItemUpdateType type, IListItemEx newItem, IListItemEx previousItem, Int32 index) {
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
      if (thumbnailSize != null) ThumbnailSize = thumbnailSize.Value;
    }
  }

  #endregion Substructures and classes

  /// <summary> The ShellFileListView class that visualize contents of a directory </summary>
  public partial class ShellView : UserControl {

    #region Event Handler

    public event EventHandler<NavigatingEventArgs> Navigating;

    /// <summary> Occurs when the <see cref="ShellView" /> control navigates to a new folder. </summary>
    public event EventHandler<NavigatedEventArgs> Navigated;

    public event EventHandler<ListViewColumnDropDownArgs> OnListViewColumnDropDownClicked;

    public event EventHandler<CollumnsChangedArgs> OnListViewCollumnsChanged;

    /// <summary>
    /// Occurs when the <see cref="ShellView"/>'s current selection
    /// changes.
    /// </summary>
    public event EventHandler SelectionChanged;

    public event EventHandler<ItemUpdatedEventArgs> ItemUpdated;

    public event EventHandler<ViewChangedEventArgs> ViewStyleChanged;

    public event EventHandler<NavigatedEventArgs> ItemMiddleClick;

    /// <summary>
    /// Occurs when the user right-clicks on the blank area of the column header area
    /// </summary>
    public event MouseEventHandler ColumnHeaderRightClick;

    /// <summary>
    /// Raised whenever a key is pressed, with the intention of doing a key jump. Please use
    /// KeyDown to catch when any key is pressed.
    /// </summary>
    public event KeyEventHandler KeyJumpKeyDown;

    public event EventHandler<RenameEventArgs> BeginItemLabelEdit;

    /// <summary>Raised whenever file/folder name is finished editing. Boolean: is event canceled</summary>
    public event EventHandler<Boolean> EndItemLabelEdit;

    /// <summary> Raised when the timer finishes for the Key Jump timer. </summary>
    public event EventHandler KeyJumpTimerDone;

    #endregion Event Handler

    #region Public Members
    public ToolTip ToolTip;
    public Dictionary<PROPERTYKEY, Collumns> AllAvailableColumns;
    public List<Collumns> Collumns = new List<Collumns>();
    public List<ListViewGroupEx> Groups = new List<ListViewGroupEx>();
    public Boolean IsRenameNeeded { get; set; }
    //public Boolean IsLibraryInModify { get; set; }
    public Boolean IsFileExtensionShown { get; set; }
    public Boolean IsCancelRequested;
    public Boolean IsNavigationCancelRequested = false;
    public Boolean IsNavigationInProgress = false;
    public Boolean IsGroupsEnabled { get; set; }
    //public Boolean IsTraditionalNameGrouping { get; set; }

    /// <summary> Returns the key jump string as it currently is.</summary>
    public String KeyJumpString { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the folder currently being browsed by the <see
    /// cref="ShellView" /> has parent folder which can be navigated to by calling <see
    /// cref="NavigateParent" />.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Boolean CanNavigateParent => CurrentFolder.ParsingName != ShellItem.Desktop.ParsingName;

    /// <summary>
    /// Gets/sets a <see cref="ShellItem" /> describing the folder currently being browsed by
    /// the <see cref="ShellView" />.
    /// </summary>
    [Browsable(false)]
    public IListItemEx CurrentFolder { get; private set; }

    public Int32 IconSize { get; private set; }
    public List<IListItemEx> Items { get; private set; }
    public String LastSortedColumnId { get; private set; }
    public SortOrder LastSortOrder { get; private set; }
    public Collumns LastGroupCollumn { get; private set; }
    public SortOrder LastGroupOrder { get; private set; }
    public IntPtr LVHandle { get; private set; }
    public ObservableCollectionEx<LVItemColor> LVItemsColorCodes { get; set; }
    public ImageListEx LargeImageList;
    public ImageListEx SmallImageList;

	/// <summary>Returns the currently selected item and removes any items in <see cref="_SelectedIndexes"/> not in <see cref="Items"/>  </summary>
	public List<IListItemEx> SelectedItems {
      get {
        var data = this._SelectedIndexes.ToArray();
        var selItems = new List<IListItemEx>();
        _DraggedItemIndexes.AddRange(data);

        foreach (var index in data) {
          var item = this.Items.ElementAtOrDefault(index);
          if (item == null)
            this._SelectedIndexes.Remove(index);
          else
            selItems.Add(item);
        }

        return selItems;
      }
    }

    public Boolean ShowCheckboxes {
      get { return _ShowCheckBoxes; }
      set {
        if (value) {
          User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, (Int32)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT);
          User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.CheckBoxes, (Int32)ListViewExtendedStyles.CheckBoxes);
        } else {
          User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, 0);
          User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.CheckBoxes, 0);
        }

        _ShowCheckBoxes = value;
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
      get { return _MView; }
      set {
        _MView = value;
        this.IsViewSelectionAllowed = false;
        switch (value) {
          case ShellViewStyle.ExtraLargeIcon:
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_ICON, 0);
            ResizeIcons(256);
            break;

          case ShellViewStyle.LargeIcon:
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_ICON, 0);
            ResizeIcons(96);
            break;

          case ShellViewStyle.Medium:
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_ICON, 0);
            ResizeIcons(48);
            break;

          case ShellViewStyle.SmallIcon:
            ResizeIcons(16);
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_SMALLICON, 0);
            break;

          case ShellViewStyle.List:
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_LIST, 0);
            ResizeIcons(16);
            break;

          case ShellViewStyle.Details:
            this.UpdateColsInView(true);
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_DETAILS, 0);
            ResizeIcons(16);
            break;

          case ShellViewStyle.Thumbnail:
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_ICON, 0);
            break;

          case ShellViewStyle.Tile:
            var isComputer = this.RequestedCurrentLocation.ParsingName.Equals(KnownFolders.Computer.ParsingName);
            User32.SendMessage(this.LVHandle, MSG.LVM_SETVIEW, (Int32)LV_VIEW.LV_VIEW_TILE, 0);
            if (isComputer) {
              var tvi = new LVTILEVIEWINFO {
                cLines = 2,
                rcLabelMargin = new User32.RECT() { Left = 2, Right = 0, Bottom = 60, Top = 5 },
                cbSize = (UInt32)Marshal.SizeOf(typeof(LVTILEVIEWINFO)),
                dwMask = (UInt32)LVTVIM.LVTVIM_COLUMNS | (UInt32)LVTVIM.LVTVIM_LABELMARGIN | (UInt32)LVTVIM.LVTVIM_TILESIZE,
                dwFlags = (UInt32)LVTVIF.LVTVIF_FIXEDSIZE,
                sizeTile = new INTEROP_SIZE() { cx = 250, cy = 60 },
              };

              User32.SendMessage(this.LVHandle, (Int32)MSG.LVM_SETTILEVIEWINFO, 0, ref tvi);
            } else {
              var tvi = new LVTILEVIEWINFO {
                cLines = 2,
                rcLabelMargin = new User32.RECT() { Left = 0, Right = 0, Bottom = 0, Top = 0 },
                cbSize = (UInt32)Marshal.SizeOf(typeof(LVTILEVIEWINFO)),
                dwMask = (UInt32)LVTVIM.LVTVIM_COLUMNS | (UInt32)LVTVIM.LVTVIM_LABELMARGIN | (UInt32)LVTVIM.LVTVIM_TILESIZE,
                dwFlags = (UInt32)LVTVIF.LVTVIF_FIXEDSIZE,
                sizeTile = new INTEROP_SIZE() { cx = 250, cy = 60 },
              };

              User32.SendMessage(this.LVHandle, (Int32)MSG.LVM_SETTILEVIEWINFO, 0, ref tvi);
            }

            ResizeIcons(48);
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

        ViewStyleChanged?.Invoke(this, new ViewChangedEventArgs(value, this.IconSize));
        this.IsViewSelectionAllowed = true;
      }
    }

    public Int32 CurrentRefreshedItemIndex = -1; //TODO: Find out if we need this property

	/// <summary>Are we/have we navigated to a search folder</summary>
	public Boolean IsSearchNavigating = false;

    public Boolean IsRenameInProgress = false;

    #endregion Public Members

    #region Private Members
	
	private ShellNotifications Notifications = new ShellNotifications();
    private IListView _IIListView;
    private FileSystemWatcher _FsWatcher = new FileSystemWatcher();
    private ListViewEditor _EditorSubclass;
    private readonly F.Timer _UnvalidateTimer = new F.Timer();
    private readonly F.Timer _MaintenanceTimer = new F.Timer();
    private readonly F.Timer _NavWaitTimer = new F.Timer() { Interval = 150, Enabled = false };
    private readonly String _DBPath = Path.Combine(KnownFolders.RoamingAppData.ParsingName, @"BExplorer\Settings.sqlite");

    private List<Int32> _SelectedIndexes {
      get {
        var selItems = new List<Int32>();
        Int32 iStart = -1;
        var lvi = new LVITEMINDEX();
        while (lvi.iItem != -1) {
          try {
            lvi = this.ToLvItemIndex(iStart);
            User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
            iStart = lvi.iItem;
          } catch { }

          if (lvi.iItem != -1) {
            if (!selItems.Contains(lvi.iItem))
              selItems.Add(lvi.iItem);
            else
              break;
          }
        }

        return selItems;
      }
    }

    private Boolean _ItemForRealNameIsAny => _ItemForRename != -1;
    private Int32 _ItemForRename { get; set; }
    private Boolean _IsCanceledOperation { get; set; }
    private Int32 _LastItemForRename { get; set; }
    private System.Runtime.InteropServices.ComTypes.IDataObject _DataObject { get; set; }
    private Boolean _ShowCheckBoxes = false;
    private Boolean _ShowHidden;
    private F.Timer _ResetTimer = new F.Timer();
    private readonly List<Int32> _DraggedItemIndexes = new List<Int32>();
    private F.Timer _KeyJumpTimer = new F.Timer();
    private IListItemEx _Kpreselitem = null;
    private LVIS _IsDragSelect = 0;
    private BackgroundWorker _Bw = new BackgroundWorker();

    private ShellViewStyle _MView;


    private F.Timer _SelectionTimer = new F.Timer();
    private ImageList _Small = new ImageList(ImageListSize.SystemSmall);

    private ManualResetEvent _ResetEvent = new ManualResetEvent(true);

    private readonly List<Int32> _CuttedIndexes = new List<Int32>();
    private Int32 _LastDropHighLightedItemIndex = -1;

    public Dictionary<String, Dictionary<IListItemEx, List<String>>> BadgesData;
    private readonly QueueEx<Tuple<ItemUpdateType, IListItemEx>> _ItemsQueue = new QueueEx<Tuple<ItemUpdateType, IListItemEx>>();
    public IListItemEx RequestedCurrentLocation { get; set; }
    private readonly List<String> _TemporaryFiles = new List<String>();
    private Boolean _IsDisplayEmptyText = false;
    private readonly List<Thread> _Threads = new List<Thread>();
    public Boolean IsViewSelectionAllowed = true;
    private readonly ManualResetEvent _Mre = new ManualResetEvent(false);
    private readonly HashSet<IntPtr> _AddedItems = new HashSet<IntPtr>();
    private readonly F.Timer _SearchTimer = new F.Timer();
    private F.Timer _ItemLoadingTimer = new F.Timer();
    private readonly ManualResetEvent _Smre = new ManualResetEvent(true);

    #endregion Private Members

    #region Initializer

    /// <summary> Main constructor </summary>
    public ShellView() {
      this._ItemForRename = -1;
      this.SetStyle(ControlStyles.DoubleBuffer |
      ControlStyles.UserPaint |
      ControlStyles.AllPaintingInWmPaint,
      true);
      this.UpdateStyles();
      InitializeComponent();
      this.Items = new List<IListItemEx>();
      this.AllAvailableColumns = this.AvailableColumns();
      this.AllowDrop = true;
      this.LargeImageList = new ImageListEx(48);
      this.SmallImageList = new ImageListEx(16);
      this.LargeImageList.AttachToListView(this, 0);
      this.SmallImageList.AttachToListView(this, 1);
      _ResetTimer.Interval = 450;
      _ResetTimer.Tick += resetTimer_Tick;
      this.MouseUp += ShellView_MouseUp;
      _SelectionTimer.Interval = 600;
      _SelectionTimer.Tick += selectionTimer_Tick;
    }

    #endregion Initializer

    #region Events

    private void _MaintenanceTimer_Tick(Object sender, EventArgs e) {
      new Thread(() => {
        var curProcess = Process.GetCurrentProcess();
        if (curProcess.WorkingSet64 > 100 * 1024 * 1024) Shell32.SetProcessWorkingSetSize(curProcess.Handle, -1, -1);
        curProcess.Dispose();
      }).Start();
    }

    private void _UnvalidateTimer_Tick(Object sender, EventArgs e) {
      this._UnvalidateTimer.Stop();
      if (this.CurrentFolder == null) return;
      try {
        while (_ItemsQueue.Count() > 0) {
          var obj = _ItemsQueue.Dequeue();
          if (obj.Item1 == ItemUpdateType.RecycleBin) {
            this.RaiseRecycleBinUpdated();
          }
          if (obj.Item1 == ItemUpdateType.Deleted) {
            //var worker = new Thread(() => {
            //var itemForDelete = this.Items.ToArray().SingleOrDefault(s =>
            //		s.Equals(obj.Item2) || (
            //			obj.Item2.Extension.Equals(".library-ms") &&
            //			s.ParsingName.Equals(Path.Combine(KnownFolders.Libraries.ParsingName,
            //				Path.GetFileName(obj.Item2.ParsingName)))
            //		)
            //);

            //if (itemForDelete != null) {
            Items.Remove(obj.Item2);
            this._AddedItems.Remove(obj.Item2.PIDL);
            //TODO: Make this to work in threaded environment
            //itemForDelete.Dispose();
            //}

            obj.Item2.Dispose();
            //this.Invoke((Action)(this.ResortListViewItems));
            //});
            //worker.SetApartmentState(ApartmentState.STA);
            //worker.Start();
          } else if (obj.Item1 == ItemUpdateType.Created) {
            if (obj.Item2.IsInCurrentFolder(this.CurrentFolder) && !this.Items.Contains(obj.Item2, new ShellItemEqualityComparer())) {
              obj.Item2.ItemIndex = this.Items.Count;
              this.Items.Add(obj.Item2);
              this._AddedItems.Add(obj.Item2.PIDL);
            }
          } else if (obj.Item1 != ItemUpdateType.RecycleBin) {
            var existingItem = this.Items.FirstOrDefault(s => s.Equals(obj.Item2));
            if (existingItem == null) {
              if (obj.Item2.ParsingName.StartsWith(this.CurrentFolder.ParsingName)) {
                if (!this.Items.Contains(obj.Item2, new ShellItemEqualityComparer()) &&
                        !String.IsNullOrEmpty(obj.Item2.ParsingName)) {
                  obj.Item2.ItemIndex = this.Items.Count;
                  this.Items.Add(obj.Item2);
                  this._AddedItems.Add(obj.Item2.PIDL);
                }
              } else {
                var affectedItem = this.Items.FirstOrDefault(s => s.Equals(obj.Item2.Parent));
                if (affectedItem != null) {
                  var index = affectedItem.ItemIndex;
                  this.RefreshItem(index, true);
                }
              }
            } else {
              if (this.IconSize == 16)
                this.SmallImageList.EnqueueOverlay(existingItem.ItemIndex);
              else
                this.LargeImageList.EnqueueOverlay(existingItem.ItemIndex);
              this.RefreshItem(existingItem.ItemIndex, true);
            }
            //this.ResortListViewItems();
          } else {
            continue;
          }
        }

        foreach (var path in this._TemporaryFiles.ToArray()) {
          var item = this.Items.ToArray().SingleOrDefault(s => s.ParsingName.ToLower().Equals(path.ToLower()));
          if (item?.IsFolder == false && !File.Exists(path))
            this.Items.Remove(item);
        }

        ////if (this.Items.Count != this.CurrentFolder.Count()) {
        //var maintenanceThread = new Thread(() => {
        //	var deletedItems = this.Items.ToArray().Where(p => !this.CurrentFolder.Any(p2 => p2.Equals(p)));
        //	foreach (var deletedItem in deletedItems) {
        //		Items.Remove(deletedItem);
        //		//this._AddedItems.Remove(deletedItem.PIDL);
        //		//deletedItem.Dispose();
        //	}
        //	this.Invoke((Action) (() => {
        //		this.ResortListViewItems();
        //	}));
        //});
        //maintenanceThread.Start();


        //}
        this.ResortListViewItems();

        this.ItemUpdated?.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Created, null, null, -1));
      } catch (Exception) {
      }

      Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

    private void ResortListViewItems() {
      var col = this.Collumns.FirstOrDefault(w => w.ID == this.LastSortedColumnId);
      this.SetSortCollumn(true, col, this.LastSortOrder, false);
      if (this.IsGroupsEnabled) this.SetGroupOrder(false);
    }

    private void Column_OnClick(Int32 iItem) {
      var rect = new User32.RECT();
      IntPtr headerhandle = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);

      if (User32.SendMessage(headerhandle, MSG.HDM_GETITEMDROPDOWNRECT, iItem, ref rect) == 0) throw new Win32Exception();
      var pt = this.PointToScreen(new DPoint(rect.Left, rect.Bottom));
      this.OnListViewColumnDropDownClicked?.Invoke(this.Collumns[iItem], new ListViewColumnDropDownArgs(iItem, pt));
    }

    private void selectionTimer_Tick(Object sender, EventArgs e) {
      if (MouseButtons != MouseButtons.Left) {
        (sender as F.Timer)?.Stop();
        OnSelectionChanged();
        KeyJumpTimerDone?.Invoke(this, EventArgs.Empty);
      }
      if (this._ItemForRename != this.GetFirstSelectedItemIndex() && !this.IsRenameInProgress) {
        (sender as F.Timer)?.Stop();
        this.EndLabelEdit();
      }
    }

    private void resetTimer_Tick(Object sender, EventArgs e) {
      (sender as F.Timer)?.Stop();
      _ResetEvent.Set();
      this.LargeImageList.ResetEvent.Set();
      this.IsCancelRequested = false;
      Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

    private void ShellView_MouseUp(Object sender, MouseEventArgs e) {
      if (_IsDragSelect == LVIS.LVIS_SELECTED) {
        if (_SelectionTimer.Enabled) _SelectionTimer.Stop();
        _SelectionTimer.Start();
      }
    }

    private void ShellView_GotFocus() {
      this.Focus();
      User32.SetForegroundWindow(this.LVHandle);
    }

    private Boolean ShellView_KeyDown(Keys e) {
      if (System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox && e != Keys.Escape && e != Keys.Enter) {
        var key = System.Windows.Input.KeyInterop.KeyFromVirtualKey((Int32)e);                            // Key to send
        var target = System.Windows.Input.Keyboard.FocusedElement as System.Windows.Controls.TextBox;   // Target element
        var routedEvent = System.Windows.Input.Keyboard.KeyDownEvent; // Event to send

        target.RaiseEvent(
                new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice, PresentationSource.FromVisual(target), 0, key) { RoutedEvent = routedEvent }
        );
        return false;
      }

      if (_ItemForRealNameIsAny) {
        if (e == Keys.Escape)
          this.EndLabelEdit(true);
        else if (e == Keys.F2) {
          //TODO: implement a conditional selection inside rename textbox!
        } else if (e == Keys.Enter)
          this.EndLabelEdit();
      }
      if ((Control.ModifierKeys & Keys.Control) == Keys.Control &&
                      !(System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox)) {
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
            this.NavigateParent();
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
            //var copy = new AsyncUnbuffCopy();
            //copy.AsyncCopyFileUnbuffered(@"J:\Downloads\advinst.msi", @"J:\Downloads\advinst(2).msi", true, false, false, 4096*5, false, 100000);
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
      } else {
        if (e == Keys.Back || e == Keys.BrowserBack) {
          this.NavigateParent();
        }
        if (e == Keys.Escape) {
          foreach (var index in this._CuttedIndexes) {
            this._IIListView.SetItemState(index, LVIF.LVIF_STATE, LVIS.LVIS_CUT, 0);
          }
          if (this._CuttedIndexes.Any()) {
            this._CuttedIndexes.Clear();
            F.Clipboard.Clear();
          }
        }
        if (e == Keys.Delete) {
          this.DeleteSelectedFiles((Control.ModifierKeys & Keys.Shift) != Keys.Shift);
        }
        if (e == Keys.F5) {
          this.RefreshContents();
        }
      }
      return true;
    }

    #endregion Events

    #region Overrides

    protected override void OnDragDrop(F.DragEventArgs e) {
      Int32 row = -1;
      Int32 collumn = -1;
      this.HitTest(PointToClient(new DPoint(e.X, e.Y)), out row, out collumn);
      var destination = row != -1 ? Items[row] : CurrentFolder;
      if (!destination.IsFolder || (this._DraggedItemIndexes.Count > 0 && this._DraggedItemIndexes.Contains(row))) {
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
      var wp = new DataObject.Win32Point() { X = e.X, Y = e.Y };

      if (e.Data.GetDataPresent("DragImageBits"))
        DropTarget.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (Int32)e.Effect);
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

      DropTarget.Create.DragLeave();
    }

    protected override void OnDragOver(F.DragEventArgs e) {
      var wp = new DataObject.Win32Point() { X = e.X, Y = e.Y };
      Drag_SetEffect(e);

      int row = -1, collumn = -1;
      this.HitTest(PointToClient(new DPoint(e.X, e.Y)), out row, out collumn);
      var descinvalid = new DataObject.DropDescription();
      descinvalid.type = (int)DataObject.DropImageType.Invalid;
      var ddResult = ((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(descinvalid);
      if (row != -1) {
        this.RefreshItem(_LastDropHighLightedItemIndex);
        this._LastDropHighLightedItemIndex = row;
        this.RefreshItem(row);
        if (ddResult == HResult.S_OK) {
          var desc = new DataObject.DropDescription();
          switch (e.Effect) {
            case F.DragDropEffects.Copy:
              desc.type = (int)DataObject.DropImageType.Copy;
              desc.szMessage = "Copy To %1";
              break;

            case F.DragDropEffects.Link:
              desc.type = (int)DataObject.DropImageType.Link;
              desc.szMessage = "Create Link in %1";
              break;

            case F.DragDropEffects.Move:
              desc.type = (int)DataObject.DropImageType.Move;
              desc.szMessage = "Move To %1";
              break;

            case F.DragDropEffects.None:
              desc.type = (int)DataObject.DropImageType.None;
              desc.szMessage = "";
              break;

            default:
              desc.type = (int)DataObject.DropImageType.Invalid;
              desc.szMessage = "";
              break;
          }
          desc.szInsert = this.Items[row].DisplayName;
          if (this._DraggedItemIndexes.Contains(row) || !this.Items[row].IsFolder) {
            if (this.Items[row].Extension == ".exe") {
              desc.type = (int)DataObject.DropImageType.Copy;
              desc.szMessage = "Open With %1";
            } else {
              desc.type = (Int32)DataObject.DropImageType.None;
              desc.szMessage = "Cant Drop Here!";
            }
          }
                        ((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);
        }
      } else {
        this.RefreshItem(_LastDropHighLightedItemIndex);
        this._LastDropHighLightedItemIndex = -1;
        if (ddResult == HResult.S_OK) {
          if (e.Effect == F.DragDropEffects.Link) {
            DataObject.DropDescription desc = new DataObject.DropDescription();
            desc.type = (int)DataObject.DropImageType.Link;
            desc.szMessage = "Create Link in %1";
            desc.szInsert = this.CurrentFolder.DisplayName;
            ((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);
          } else if (e.Effect == F.DragDropEffects.Copy) {
            DataObject.DropDescription desc = new DataObject.DropDescription();
            desc.type = (int)DataObject.DropImageType.Link;
            desc.szMessage = "Create a copy in %1";
            desc.szInsert = this.CurrentFolder.DisplayName;
            ((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);
          }
        }
      }

      if (e.Data.GetDataPresent("DragImageBits"))
        DropTarget.Create.DragOver(ref wp, (Int32)e.Effect);
      else
        base.OnDragOver(e);
    }

    protected override void OnDragEnter(F.DragEventArgs e) {
      var wp = new DataObject.Win32Point() { X = e.X, Y = e.Y };
      Drag_SetEffect(e);

      if (e.Data.GetDataPresent("DragImageBits"))
        DropTarget.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wp, (Int32)e.Effect);
      else
        base.OnDragEnter(e);
    }

    protected override void OnQueryContinueDrag(F.QueryContinueDragEventArgs e) => base.OnQueryContinueDrag(e);

    protected override void OnGiveFeedback(F.GiveFeedbackEventArgs e) {
      e.UseDefaultCursors = true;
      var doo = new F.DataObject(_DataObject);

      if (doo.GetDataPresent("DragWindow")) {
        IntPtr hwnd = GetIntPtrFromData(doo.GetData("DragWindow"));
        User32.PostMessage(hwnd, 0x403, IntPtr.Zero, IntPtr.Zero);
      } else {
        e.UseDefaultCursors = true;
      }
      if (IsDropDescriptionValid(_DataObject)) {
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
    }

    [HandleProcessCorruptedStateExceptions]
    protected override void WndProc(ref Message m) {
      try {
        if (m.Msg == (Int32)WM.WM_PARENTNOTIFY && User32.LOWORD((Int32)m.WParam) == (Int32)WM.WM_MBUTTONDOWN) OnItemMiddleClick();
        base.WndProc(ref m);

        if (m.Msg == ShellNotifications.WM_SHNOTIFY) {
          this.ProcessShellNotifications(ref m);
        }

        #region m.Msg == 78

        if (m.Msg == 78) {
          #region Starting

          var nmhdrHeader = (NMHEADER)(m.GetLParam(typeof(NMHEADER)));
          if (nmhdrHeader.hdr.code == (Int32)HDN.HDN_DROPDOWN)
            Column_OnClick(nmhdrHeader.iItem);
          //F.MessageBox.Show(nmhdrHeader.iItem.ToString());
          else if (nmhdrHeader.hdr.code == (Int32)HDN.HDN_BEGINTRACKW)
            if (this.View != ShellViewStyle.Details) m.Result = (IntPtr)1;

          /*
else if (nmhdrHeader.hdr.code == (int)HDN.HDN_BEGINTRACKW)
if (this.View != ShellViewStyle.Details) m.Result = (IntPtr)1;
*/

          #endregion Starting

          var nmhdr = (NMHDR)m.GetLParam(typeof(NMHDR));
          switch (nmhdr.code) {
            case WNM.LVN_GETEMPTYMARKUP:
              if (this._IsDisplayEmptyText) {
                var nmlvem = (NMLVEMPTYMARKUP)m.GetLParam(typeof(NMLVEMPTYMARKUP));
                nmlvem.dwFlags = 0x1;
                nmlvem.szMarkup = "Working on it...";
                Marshal.StructureToPtr(nmlvem, m.LParam, false);
                m.Result = (IntPtr)1;
              } else {
                m.Result = IntPtr.Zero;
              }
              break;

            case WNM.LVN_ENDLABELEDITW:

              #region Case

              var nmlvedit = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
              if (!String.IsNullOrEmpty(nmlvedit.item.pszText)) {
                var item = this.Items[nmlvedit.item.iItem];
                RenameShellItem(item.ComInterface, nmlvedit.item.pszText, (item.DisplayName != Path.GetFileName(item.ParsingName)) && !item.IsFolder, item.Extension);
                this.EndLabelEdit();
              }

              this._EditorSubclass?.DestroyHandle();
              break;

            #endregion Case

            case WNM.LVN_GETDISPINFOW:

              #region Case

              var nmlv = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));
              if (Items.Count == 0 || Items.Count - 1 < nmlv.item.iItem)
                break;
              var currentItem = this.IsSearchNavigating ? Items[nmlv.item.iItem].Clone() : Items[nmlv.item.iItem];

              if ((nmlv.item.mask & LVIF.LVIF_TEXT) == LVIF.LVIF_TEXT) {
                if (nmlv.item.iSubItem == 0) {
                  nmlv.item.pszText = currentItem.DisplayName;
                  Marshal.StructureToPtr(nmlv, m.LParam, false);
                } else {
                  if ((View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details) || (this.View == ShellViewStyle.Tile && this.AllAvailableColumns.Count >= nmlv.item.iSubItem)) {
                    var currentCollumn = this.View == ShellViewStyle.Tile
                        ? this.AllAvailableColumns.Values.ToArray()[nmlv.item.iSubItem]
                        : this.Collumns[nmlv.item.iSubItem];


                    Object valueCached;
                    if (currentItem.ColumnValues.TryGetValue(currentCollumn.pkey, out valueCached)) {
                      String val = String.Empty;
                      if (valueCached != null) {
                        if (currentCollumn.CollumnType == typeof(DateTime))
                          val = ((DateTime)valueCached).ToString(Thread.CurrentThread.CurrentUICulture);
                        else if (currentCollumn.CollumnType == typeof(Int64))
                          val = $"{Math.Ceiling(Convert.ToDouble(valueCached.ToString()) / 1024):# ### ### ##0} KB";
                        else if (currentCollumn.CollumnType == typeof(PerceivedType))
                          val = ((PerceivedType)valueCached).ToString();
                        else if (currentCollumn.CollumnType == typeof(FileAttributes))
                          val = this.GetFilePropertiesString(valueCached);
                        else
                          val = valueCached.ToString();
                      }

                      nmlv.item.pszText = val.Trim();
                    } else {
                      var temp = currentItem;
                      var isi2 = (IShellItem2)temp.ComInterface;
                      var guid = new Guid(InterfaceGuids.IPropertyStore);
                      IPropertyStore propStore = null;
                      isi2.GetPropertyStore(GetPropertyStoreOptions.FastPropertiesOnly, ref guid, out propStore);
                      PROPERTYKEY pk = currentCollumn.pkey;
                      var pvar = new PropVariant();
                      if (propStore != null && propStore.GetValue(ref pk, pvar) == HResult.S_OK) {
                        if (pvar.Value == null) {
                          if (this.IconSize == 16) {
                            this.SmallImageList.EnqueueSubitemsGet(Tuple.Create(nmlv.item.iItem, nmlv.item.iSubItem, pk));
                          } else {
                            this.LargeImageList.EnqueueSubitemsGet(Tuple.Create(nmlv.item.iItem, nmlv.item.iSubItem, pk));
                          }
                        } else {
                          var val = String.Empty;
                          if (currentCollumn.CollumnType == typeof(DateTime))
                            val = ((DateTime)pvar.Value).ToString(Thread.CurrentThread.CurrentUICulture);
                          else if (currentCollumn.CollumnType == typeof(Int64))
                            val =
                                $"{Math.Ceiling(Convert.ToDouble(pvar.Value.ToString()) / 1024):# ### ### ##0} KB";
                          else if (currentCollumn.CollumnType == typeof(PerceivedType))
                            val = ((PerceivedType)pvar.Value).ToString();
                          else if (currentCollumn.CollumnType == typeof(FileAttributes))
                            val = this.GetFilePropertiesString(pvar.Value);
                          else
                            val = pvar.Value.ToString();

                          nmlv.item.pszText = val.Trim();
                          pvar.Dispose();
                        }
                      }
                    }
                  }

                  Marshal.StructureToPtr(nmlv, m.LParam, false);
                }
              }

              if ((nmlv.item.mask & LVIF.LVIF_COLUMNS) == LVIF.LVIF_COLUMNS && this.CurrentFolder?.ParsingName.Equals(KnownFolders.Computer.ParsingName) == false) {
                var refGuidPDL = typeof(IPropertyDescriptionList).GUID;
                var refGuidPD = typeof(IPropertyDescription).GUID;
                var iShellItem2 = (IShellItem2)currentItem.ComInterface;

                var ptrPDL = IntPtr.Zero;
                iShellItem2.GetPropertyDescriptionList(SpecialProperties.PropListTileInfo, ref refGuidPDL,
                        out ptrPDL);
                IPropertyDescriptionList propertyDescriptionList = (IPropertyDescriptionList)Marshal.GetObjectForIUnknown(ptrPDL);
                var descriptionsCount = 0u;
                propertyDescriptionList.GetCount(out descriptionsCount);
                nmlv.item.cColumns = (int)descriptionsCount;
                var columns = new int[nmlv.item.cColumns];
                Marshal.Copy(nmlv.item.puColumns, columns, 0, nmlv.item.cColumns);
                for (uint i = 0; i < descriptionsCount; i++) {
                  IPropertyDescription propertyDescription = null;
                  propertyDescriptionList.GetAt(i, ref refGuidPD, out propertyDescription);
                  PROPERTYKEY pkey;
                  propertyDescription.GetPropertyKey(out pkey);
                  Collumns column = null;
                  if (this.AllAvailableColumns.TryGetValue(pkey, out column)) {
                    columns[i] = column.Index;
                  } else {
                    columns[i] = 0;
                  }
                }
                Marshal.Copy(columns, 0, nmlv.item.puColumns, nmlv.item.cColumns);
                Marshal.StructureToPtr(nmlv, m.LParam, false);
              }
              break;

            #endregion Case

            case WNM.LVN_COLUMNCLICK:

              #region Case

              var nlcv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
              var sortOrder = SortOrder.Ascending;
              if (this.LastSortedColumnId == this.Collumns[nlcv.iSubItem].ID) {
                sortOrder = this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
              }

              if (!this.IsGroupsEnabled) {
                SetSortCollumn(true, this.Collumns[nlcv.iSubItem], sortOrder);
              } else if (this.LastGroupCollumn == this.Collumns[nlcv.iSubItem]) {
                this.SetGroupOrder();
              } else {
                SetSortCollumn(true, this.Collumns[nlcv.iSubItem], sortOrder);
                this.SetGroupOrder(false);
              }
              break;

            #endregion Case

            case WNM.LVN_GETINFOTIP:

              #region Case

              var nmGetInfoTip = (NMLVGETINFOTIP)m.GetLParam(typeof(NMLVGETINFOTIP));
              if (this.Items.Count == 0)
                break;
              if (ToolTip == null)
                ToolTip = new ToolTip(this);

              var itemInfotip = this.Items[nmGetInfoTip.iItem];
              Char[] charBuf = ("\0").ToCharArray();
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

            #endregion Case

            case WNM.LVN_ODFINDITEM:

              #region Case

              if (this.ToolTip != null && this.ToolTip.IsVisible)
                this.ToolTip.HideTooltip();
              var findItem = (NMLVFINDITEM)m.GetLParam(typeof(NMLVFINDITEM));
              KeyJumpString = findItem.lvfi.psz;

              KeyJumpKeyDown?.Invoke(this, new KeyEventArgs(Keys.A));
              Int32 startindex = this.GetFirstSelectedItemIndex() + (KeyJumpString.Length > 1 ? 0 : 1);
              Int32 selind = GetFirstIndexOf(KeyJumpString, startindex);
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

            #endregion Case

            case -175:

              #region Case

              var nmlvLe = (NMLVDISPINFO)m.GetLParam(typeof(NMLVDISPINFO));

              if (this.ToolTip != null && this.ToolTip.IsVisible)
                this.ToolTip.HideTooltip();

              this.IsFocusAllowed = false;
              this._IsCanceledOperation = false;
              this._ItemForRename = nmlvLe.item.iItem;
              this.BeginItemLabelEdit?.Invoke(this, new RenameEventArgs(this._ItemForRename));
              m.Result = (IntPtr)0;

              var editControl = User32.SendMessage(this.LVHandle, 0x1018, 0, 0);
              var indexLastDot = this.Items[this._ItemForRename].DisplayName.LastIndexOf(".", StringComparison.Ordinal);
              User32.SendMessage(editControl, 0x00B1, 0, indexLastDot);
              break;

            #endregion Case

            case WNM.LVN_ITEMACTIVATE:

              #region Case

              if (this.ToolTip != null && this.ToolTip.IsVisible) this.ToolTip.HideTooltip();
              if (_ItemForRealNameIsAny && this.IsRenameInProgress) {
                this.EndLabelEdit();
              } else {
                var iac = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
                var selectedItem = Items[iac.iItem];
                if (selectedItem.IsFolder) {
                  Navigate_Full(selectedItem, true);
                } else if (selectedItem.IsLink || selectedItem.ParsingName.EndsWith(".lnk")) {
                  var shellLink = new ShellLink(selectedItem.ParsingName);
                  var newSho = FileSystemListItem.ToFileSystemItem(this.LVHandle, shellLink.TargetPIDL);
                  if (newSho.IsFolder)
                    Navigate_Full(newSho, true);
                } else {
                  StartProcessInCurrentDirectory(selectedItem);
                }
              }
              break;

            #endregion Case

            case WNM.LVN_BEGINSCROLL:

              #region Case

              this.EndLabelEdit();
              this.LargeImageList.ResetEvent.Reset();
              _ResetEvent.Reset();
              _ResetTimer.Stop();
              this.ToolTip?.HideTooltip();
              break;

            #endregion Case

            case WNM.LVN_ENDSCROLL:

              #region Case

              _ResetTimer.Start();
              //this.resetEvent.Set();

              break;

            #endregion Case

            case -100:

              #region Case

              F.MessageBox.Show("AM");
              break;

            #endregion Case

            case WNM.LVN_ITEMCHANGED:

              #region Case

              var nlv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
              if ((nlv.uChanged & LVIF.LVIF_STATE) == LVIF.LVIF_STATE) {
                this._IsDragSelect = nlv.uNewState;
                if (nlv.iItem != _LastItemForRename)
                  _LastItemForRename = -1;
                if (!_SelectionTimer.Enabled)
                  _SelectionTimer.Start();
              }

              break;

            #endregion Case

            case WNM.LVN_ODSTATECHANGED:

              #region Case

              OnSelectionChanged();
              break;

            #endregion Case

            case WNM.LVN_KEYDOWN:

              #region Case

              var nkd = (NMLVKEYDOWN)m.GetLParam(typeof(NMLVKEYDOWN));
              if (!ShellView_KeyDown((Keys)((int)nkd.wVKey))) {
                m.Result = (IntPtr)1;
                break;
              }

              if (nkd.wVKey == (short)Keys.F2 && !(System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox)) {
                RenameSelectedItem();
              }

              if (!_ItemForRealNameIsAny && !this.IsRenameInProgress && !(System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox)) {
                switch (nkd.wVKey) {
                  case (short)Keys.Enter:
                    if (this._IsCanceledOperation) {
                      //this.IsRenameInProgress = false;
                      break;
                    }
                    var selectedItem = this.GetFirstSelectedItem();
                    if (selectedItem.IsFolder) {
                      Navigate(selectedItem, false, false, this.IsNavigationInProgress);
                    } else if (selectedItem.IsLink && selectedItem.ParsingName.EndsWith(".lnk")) {
                      var shellLink = new ShellLink(selectedItem.ParsingName);
                      var newSho = new FileSystemListItem();
                      newSho.Initialize(this.LVHandle, shellLink.TargetPIDL);
                      if (newSho.IsFolder)
                        Navigate(newSho, false, false, this.IsNavigationInProgress);
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
                switch (nkd.wVKey) {
                  case (Int16)Keys.Enter:
                    if (!this.IsRenameInProgress)
                      this.EndLabelEdit();
                    this.Focus();
                    break;

                  case (Int16)Keys.Escape:
                    this.EndLabelEdit(true);
                    this.Focus();
                    break;

                  default:
                    break;
                }
                if (System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox) {
                  m.Result = (IntPtr)1;
                  break;
                }
              }
              break;

            #endregion Case

            case WNM.LVN_GROUPINFO: //TODO: Deal with this useless code

              #region Case

              //RedrawWindow();
              break;

            #endregion Case

            case WNM.LVN_HOTTRACK:

              #region Case

              var nlvHotTrack = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
              if (ToolTip != null && nlvHotTrack.iItem != ToolTip.ItemIndex && ToolTip.ItemIndex > -1) {
                ToolTip.HideTooltip();
                this.Focus();
              }

              break;

            #endregion Case

            case WNM.LVN_BEGINDRAG:

              #region Case

              this._DraggedItemIndexes.Clear();
              var dataObjPtr = IntPtr.Zero;
              _DataObject = this.SelectedItems.ToArray().GetIDataObject(out dataObjPtr);
              //uint ef = 0;
              var ishell2 = (DataObject.IDragSourceHelper2)new DragDropHelper();
              ishell2.SetFlags(1);
              var wp = new DataObject.Win32Point() { X = Cursor.Position.X, Y = Cursor.Position.Y };
              ishell2.InitializeFromWindow(this.Handle, ref wp, _DataObject);
              DoDragDrop(_DataObject, F.DragDropEffects.All | F.DragDropEffects.Link);
              //Shell32.SHDoDragDrop(this.Handle, dataObject, null, unchecked((uint)F.DragDropEffects.All | (uint)F.DragDropEffects.Link), out ef);
              break;

            #endregion Case

            case WNM.NM_RCLICK:

              #region Case

              var nmhdrHdn = (NMHEADER)(m.GetLParam(typeof(NMHEADER)));
              var itemActivate = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
              this.ToolTip?.HideTooltip();
              this.IsFocusAllowed = false;
              if (nmhdrHdn.iItem != -1 && nmhdrHdn.hdr.hwndFrom == this.LVHandle) {
                //Workaround for cases where on right click over an ites the item is not actually selected
                if (this.GetSelectedCount() == 0) {
                  this.SelectItemByIndex(nmhdrHdn.iItem);
                }
                var selitems = this.SelectedItems;
                var cm = new ShellContextMenu(selitems.ToArray(), SVGIO.SVGIO_SELECTION, this);
                cm.ShowContextMenu(this, itemActivate.ptAction, CMF.CANRENAME);
              } else if (nmhdrHdn.iItem == -1) {
                var cm = new ShellContextMenu(new IListItemEx[1] { this.CurrentFolder }, SVGIO.SVGIO_BACKGROUND, this);
                cm.ShowContextMenu(this, itemActivate.ptAction, 0, true);
              } else {
                this.IsFocusAllowed = true;
                this.ColumnHeaderRightClick?.Invoke(this, new MouseEventArgs(F.MouseButtons.Right, 1, MousePosition.X, MousePosition.Y, 0));
              }

              break;

            #endregion Case

            case WNM.NM_CLICK: //TODO: Deal with this useless code

              #region Case

              break;

            #endregion Case

            case WNM.NM_SETFOCUS:

              #region Case

              if (IsGroupsEnabled)
                RedrawWindow();
              ShellView_GotFocus();
              this.IsFocusAllowed = true;
              break;

            #endregion Case

            case WNM.NM_KILLFOCUS:

              #region Case

              if (this._ItemForRename != -1 && !this.IsRenameInProgress)
                EndLabelEdit();
              if (IsGroupsEnabled)
                RedrawWindow();
              this.ToolTip?.HideTooltip();
              //OnLostFocus();
              if (this.IsRenameInProgress) {
                this.Focus(false);
              }
              break;

            #endregion Case

            case CustomDraw.NM_CUSTOMDRAW:
              this.ProcessCustomDraw(ref m, ref nmhdr);
              break;
          }
        }

        #endregion m.Msg == 78
      } catch {
      }
    }

    protected override void OnSizeChanged(EventArgs e) {
      base.OnSizeChanged(e);
      User32.MoveWindow(this.LVHandle, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, false);
    }

    protected override void OnHandleCreated(EventArgs e) {
      base.OnHandleCreated(e);

      Notifications.RegisterChangeNotify(this.Handle, ShellNotifications.CSIDL.CSIDL_DESKTOP, true);
      this._UnvalidateTimer.Interval = 350;
      this._UnvalidateTimer.Tick += _UnvalidateTimer_Tick;
      this._UnvalidateTimer.Stop();

      this._MaintenanceTimer.Interval = 1000 * 15;
      this._MaintenanceTimer.Tick += _MaintenanceTimer_Tick;
      this._MaintenanceTimer.Start();

      this._SearchTimer.Interval = 750;
      this._SearchTimer.Enabled = false;
      this._SearchTimer.Tick += (sender, args) => {
        if (this.Items.Count > 0) {
          this._Smre.Reset();
          this.Items = this.Items.OrderBy(o => o.DisplayName).ToList();
          for (Int32 j = 0; j < this.Items.Count; j++) {
            this.Items[j].ItemIndex = j;
          }
          this._IIListView.SetItemCount(this.Items.Count, 0x2);
          this._Smre.Set();
        }
      };
      this._SearchTimer.Stop();

      this._NavWaitTimer.Tick += (sender, args) => {
        this.BeginInvoke((Action)(() => {
          this._IsDisplayEmptyText = true;
          this._IIListView.ResetEmptyText();
        }));
      };
      this._NavWaitTimer.Stop();

      var icc = new ComCtl32.INITCOMMONCONTROLSEX() { dwSize = Marshal.SizeOf(typeof(ComCtl32.INITCOMMONCONTROLSEX)), dwICC = 1 };
      var res = ComCtl32.InitCommonControlsEx(ref icc);

      this.LVHandle = User32.CreateWindowEx(0, "SysListView32", "", User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_CLIPCHILDREN | User32.WindowStyles.WS_CLIPSIBLINGS |
                                                                      (User32.WindowStyles)User32.LVS_EDITLABELS | (User32.WindowStyles)User32.LVS_OWNERDATA | (User32.WindowStyles)User32.LVS_SHOWSELALWAYS | (User32.WindowStyles)User32.LVS_AUTOARRANGE,
                                                                       0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, this.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

      User32.ShowWindow(this.LVHandle, User32.ShowWindowCommands.Show);

      this.AddDefaultColumns(true);

      IntPtr headerhandle = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
      for (Int32 i = 0; i < this.Collumns.Count; i++) {
        this.Collumns[i].SetSplitButton(headerhandle, i);
      }


      this.IsViewSelectionAllowed = false;
      this.View = ShellViewStyle.Medium;

      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.HeaderInAllViews, (Int32)ListViewExtendedStyles.HeaderInAllViews);
      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER, (Int32)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER);
      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.FullRowSelect, (Int32)ListViewExtendedStyles.FullRowSelect);
      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.HeaderDragDrop, (Int32)ListViewExtendedStyles.HeaderDragDrop);
      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.LabelTip, (Int32)ListViewExtendedStyles.LabelTip);
      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.InfoTip, (Int32)ListViewExtendedStyles.InfoTip);
      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.UnderlineHot, (Int32)ListViewExtendedStyles.UnderlineHot);
      User32.SendMessage(this.LVHandle, MSG.LVM_SetExtendedStyle, (Int32)ListViewExtendedStyles.AutosizeColumns, (Int32)ListViewExtendedStyles.AutosizeColumns);

      IntPtr iiListViewPrt = IntPtr.Zero;
      var iid = typeof(IListView).GUID;
      User32.SendMessage(this.LVHandle, 0x10BD, ref iid, out iiListViewPrt);
      this._IIListView = (IListView)Marshal.GetTypedObjectForIUnknown(iiListViewPrt, typeof(IListView));

      this._IIListView.SetSelectionFlags(1, 1);

      this.Focus();
      User32.SetForegroundWindow(this.LVHandle);
      UxTheme.SetWindowTheme(this.LVHandle, "Explorer", 0);
      ShellItem.MessageHandle = this.LVHandle;
      this.IsViewSelectionAllowed = true;
    }

    protected override void OnHandleDestroyed(EventArgs e) {
      try {
        this._FsWatcher?.Dispose();
        this.Notifications.UnregisterChangeNotify();
        this.LargeImageList.Dispose();
        this.SmallImageList.Dispose();
        var t = new Thread(() => {
          this._Mre.Reset();
          foreach (var thread in this._Threads) {
            if (thread.IsAlive) thread.Abort();
          }
        });

        t.Start();
      } catch (ThreadAbortException) { } catch { }
      base.OnHandleDestroyed(e);
    }

    #endregion Overrides

    #region Public Methods

    public void RaiseMiddleClickOnItem(IListItemEx item) {
		this.ItemMiddleClick?.Invoke(this, new NavigatedEventArgs(item, item));
    }

    /// <summary>
    /// Saves the current <paramref name="destination">destination</paramref> settings to the SQLite database
    /// </summary>
    /// <param name="destination">The destination whos settings you want to save</param>
    public void SaveSettingsToDatabase(IListItemEx destination) {
      if (CurrentFolder == null || !CurrentFolder.IsFolder) return;

      var m_dbConnection = new SQLite.SQLiteConnection("Data Source=" + this._DBPath + ";Version=3;");
      m_dbConnection.Open();

      var command1 = new SQLite.SQLiteCommand("SELECT * FROM foldersettings WHERE Path=@Path", m_dbConnection);
      command1.Parameters.AddWithValue("Path", destination.ParsingName);
      var Reader = command1.ExecuteReader();
      var sql = Reader.Read() ? @"UPDATE foldersettings
							SET Path = @Path, LastSortOrder = @LastSortOrder, LastGroupOrder = @LastGroupOrder, LastGroupCollumn = @LastGroupCollumn,
									 View = @View, LastSortedColumn = @LastSortedColumn, Columns = @Columns, IconSize = @IconSize
							 WHERE Path = @Path" : @"INSERT into foldersettings (Path, LastSortOrder, LastGroupOrder, LastGroupCollumn, View, LastSortedColumn, Columns, IconSize)
							VALUES (@Path, @LastSortOrder, @LastGroupOrder, @LastGroupCollumn, @View, @LastSortedColumn, @Columns, @IconSize)";

      Int32[] orders = new Int32[this.Collumns.Count];
      User32.SendMessage(this.LVHandle, (UInt32)MSG.LVM_GETCOLUMNORDERARRAY, orders.Length, orders);

      var Columns_XML = new XElement("Columns");
      foreach (var index in orders) {
        var collumn = this.Collumns[index];
        var width = (Int32)User32.SendMessage(this.LVHandle, MSG.LVM_GETCOLUMNWIDTH, index, 0);
        var XML = new XElement("Column");
        XML.Add(new XAttribute("ID", collumn.ID == null ? "" : collumn.ID.ToString()));
        XML.Add(new XAttribute("Width", collumn.ID == null ? "" : width.ToString()));
        Columns_XML.Add(XML);
      }

      var Values = new Dictionary<String, String>() {
        { "Path", destination.ParsingName },
        { "LastSortOrder", LastSortOrder.ToString() },
        { "LastGroupOrder", LastGroupOrder.ToString() },
        { "LastGroupCollumn", LastGroupCollumn == null ? null : LastGroupCollumn.ID },
        { "View", View.ToString() },
        { "LastSortedColumn", LastSortedColumnId.ToString() },
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

    /// <summary>Resets the current folder's settings by deting it from the SQLIte database</summary>
    public void ResetFolderSettings() {
      var m_dbConnection = new SQLite.SQLiteConnection("Data Source=" + this._DBPath + ";Version=3;");
      m_dbConnection.Open();
      new SQLite.SQLiteCommand("DELETE FROM foldersettings", m_dbConnection).ExecuteNonQuery();
    }

    public static Boolean IsDropDescriptionValid(System.Runtime.InteropServices.ComTypes.IDataObject dataObject) {
      Object data = dataObject.GetDropDescription();
	  return data is DataObject.DropDescription ?
				(DataObject.DropImageType)((DataObject.DropDescription)data).type != DataObject.DropImageType.Invalid :
				false;
    }

    public static IntPtr GetIntPtrFromData(Object data) {
      Byte[] buf = null;

      if (data is MemoryStream) {
        buf = new Byte[4];
        if (4 != ((MemoryStream)data).Read(buf, 0, 4))
          throw new ArgumentException("Could not read an IntPtr from the MemoryStream");
      }
      if (data is Byte[]) {
        buf = (Byte[])data;
        if (buf.Length < 4)
          throw new ArgumentException("Could not read an IntPtr from the byte array");
      }

      if (buf == null)
        throw new ArgumentException("Could not read an IntPtr from the " + data.GetType().ToString());

      Int32 p = (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];
      return new IntPtr(p);
    }

    /// <summary>
    /// Inserts a new item into the control If and only If it is new. Returns the item's index OR -1 if already existing
    /// </summary>
    /// <param name="obj">The item you want to insert</param>
    /// <returns>If item is new Then returns <see cref="IListItemEx.ItemIndex">obj.ItemIndex</see> Else returns -1</returns>
    public Int32 InsertNewItem(IListItemEx obj) {
      if (!this._AddedItems.Contains(obj.PIDL) && !String.IsNullOrEmpty(obj.ParsingName) && obj.IsInCurrentFolder(this.CurrentFolder)) {
        this.Items.Add(obj);
        this._AddedItems.Add(obj.PIDL);
        var col = this.AllAvailableColumns.FirstOrDefault(w => w.Value.ID == this.LastSortedColumnId).Value;
        this.SetSortCollumn(true, col, this.LastSortOrder, false);
        if (this.IsGroupsEnabled) this.SetGroupOrder(false);
        var itemIndex = obj.ItemIndex;
        return itemIndex;
      }
      return -1;
    }

    public void UpdateItem(IListItemEx obj1, IListItemEx obj2) {
      if (!obj2.IsInCurrentFolder(this.CurrentFolder) || obj2.Equals(obj1)) return;
      var items = this.Items.ToArray();
      var oldItem =
        items.SingleOrDefault(s =>
          s.Equals(obj1) ||
          (obj1.Extension.Equals(".library-ms") &&
          s.ParsingName.Equals(Path.Combine(KnownFolders.Libraries.ParsingName, Path.GetFileName(obj1.ParsingName)))));

      var theItem =
        items.FirstOrDefault(s =>
        s.ParsingName == obj2.ParsingName ||
        (obj2.Extension.Equals(".library-ms") && s.ParsingName.Equals(Path.Combine(KnownFolders.Libraries.ParsingName, Path.GetFileName(obj2.ParsingName)))));

      if (theItem == null) {
        if (oldItem != null) {
          this.Items.Remove(oldItem);
          this._AddedItems.Remove(oldItem.PIDL);
        }
        this.Items.Add(obj2.Extension.Equals(".library-ms")
            ? FileSystemListItem.InitializeWithIShellItem(this.LVHandle,
                ShellLibrary.Load(obj2.DisplayName, true).ComInterface)
            : obj2);
        var col = this.AllAvailableColumns.FirstOrDefault(w => w.Value.ID == this.LastSortedColumnId).Value;
        this.SetSortCollumn(true, col, this.LastSortOrder, false);
        if (this.IsGroupsEnabled) this.SetGroupOrder(false);
        var obj2Real =
            this.Items.FirstOrDefault(s =>
            s.ParsingName == obj2.ParsingName ||
            (obj2.Extension.Equals(".library-ms") && s.ParsingName.Equals(Path.Combine(KnownFolders.Libraries.ParsingName, Path.GetFileName(obj2.ParsingName)))));

        if (obj2Real != null) {
          this.SelectItemByIndex(obj2Real.ItemIndex, true, true);
          this.RefreshItem(obj2Real.ItemIndex, true);
        }
      } else if (oldItem == null && obj2.Extension == String.Empty) {
        //probably a temporary file
        this._TemporaryFiles.Add(obj2.ParsingName);
      }

      this.IsFocusAllowed = true;
      this.Focus();
    }

    public void RaiseRecycleBinUpdated() => this.ItemUpdated?.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.RecycleBin, null, null, -1));

    public void RaiseItemUpdated(ItemUpdateType type, IListItemEx old, IListItemEx newItem, Int32 index) {
      this.ItemUpdated?.Invoke(this, new ItemUpdatedEventArgs(type, newItem, old, index));
    }

    public static Boolean IsShowingLayered(F.DataObject dataObject) {
      if (dataObject.GetDataPresent("IsShowingLayered")) {
        Object data = dataObject.GetData("IsShowingLayered");
        if (data != null)
          return data is Stream ? new BinaryReader(data as Stream).ReadBoolean() : false;
      }

      return false;
    }

    /// <summary>If the <see cref="GetFirstSelectedItem">Current</see> item <see cref="IListItemEx.IsFolder">IsFolder</see> Then navigate to it Else open item</summary>
    public void OpenOrNavigateItem() {
      var selectedItem = this.GetFirstSelectedItem();
      if (selectedItem.IsFolder)
        this.Navigate_Full(selectedItem, true, true, false);
      else
        Process.Start(selectedItem.ParsingName);
    }

    public Int32 GetGroupIndex(Int32 itemIndex) => itemIndex == -1 || itemIndex >= this.Items.Count ? -1 : this.Items[itemIndex].GroupIndex;

    public void OpenShareUI() => Shell32.ShowShareFolderUI(this.Handle, Marshal.StringToHGlobalAuto(this.GetFirstSelectedItem().ParsingName.Replace(@"\\", @"\")));

    public void ShowPropPage(IntPtr HWND, String filename, String proppage) => Shell32.SHObjectProperties(HWND, 0x2, filename, proppage);

    public void UpdateItem(Int32 index) => this.BeginInvoke(new MethodInvoker(() => this._IIListView.UpdateItem(index)));

    /// <summary> Navigates to the parent of the currently displayed folder. </summary>
    public void NavigateParent() {
      if (this.CurrentFolder != null)
        Navigate_Full(this.CurrentFolder.Parent, true, true);
    }

    /// <summary>Refreshes the contact (by navigating to the current folder If and only If the current folder is not null)</summary>
    public void RefreshContents() {
      if (this.CurrentFolder != null)
        Navigate_Full(this.CurrentFolder, true, refresh: true);
    }

    /// <summary>
    /// Refreshes a single item
    /// </summary>
    /// <param name="index">The index of the item you want to refresh</param>
    /// <param name="isForceRedraw">If <c>True</c> Resets everything in the Item to indicate that it needs to be refreshed/reloaded</param>
    public void RefreshItem(Int32 index, Boolean isForceRedraw = false) {
      if (isForceRedraw) {
        try {
          _ResetEvent.Set();
          var newItem = FileSystemListItem.ToFileSystemItem(this.LVHandle, this.Items[index].ParsingName.ToShellParsingName());
          newItem.GroupIndex = this.Items[index].GroupIndex;
          newItem.ItemIndex = index;
          this.Items[index] = newItem;
          this.Items[index].IsNeedRefreshing = true;
          this.Items[index].IsInvalid = true;
          //this.Items[index].OverlayIconIndex = -1;
          this.SmallImageList.EnqueueOverlay(index);
          this.Items[index].IsOnlyLowQuality = false;
          this.Items[index].IsIconLoaded = false;

        } catch (FileNotFoundException) {
          _ResetEvent.Set();
          //In case the event late and the file is not there anymore or changed catch the exception
          var newItem = FileSystemListItem.ToFileSystemItem(this.LVHandle, this.Items[index].PIDL);
          newItem.GroupIndex = this.Items[index].GroupIndex;
          newItem.ItemIndex = index;
          this.Items[index] = newItem;
          this.Items[index].IsNeedRefreshing = true;
          this.Items[index].IsInvalid = true;
          //this.Items[index].OverlayIconIndex = -1;
          this.SmallImageList.EnqueueOverlay(index);
          this.Items[index].IsOnlyLowQuality = false;
          this.Items[index].IsIconLoaded = false;
        } catch { }
      }

      this.Invoke(new MethodInvoker(() => {
        this._IIListView.UpdateItem(index);
        this._IIListView.RedrawItems(index, index);
      }));
    }

    /// <summary>Renames the first selected item</summary>
    public void RenameSelectedItem() => this.RenameItem(this.GetFirstSelectedItemIndex());

    /// <summary>
    /// Renames the item at the specified index
    /// </summary>
    /// <param name="index">The index of the item you want to rename</param>
    public void RenameSelectedItem(Int32 index) => this.RenameItem(index);

    public void CutSelectedFiles() {
      foreach (var index in this._SelectedIndexes) {
        var item = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_CUT, state = LVIS.LVIS_CUT };
        User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMSTATE, index, ref item);
      }

      this._CuttedIndexes.AddRange(this._SelectedIndexes.ToArray());
      var ddataObject = new F.DataObject();
      // Copy or Cut operation (5 = copy; 2 = cut)
      ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new Byte[] { 2, 0, 0, 0 }));
      ddataObject.SetData("Shell IDList Array", true, this.SelectedItems.ToArray().CreateShellIDList());
      F.Clipboard.SetDataObject(ddataObject, true);
    }

    public void CopySelectedFiles() {
      var ddataObject = new F.DataObject();
      // Copy or Cut operation (5 = copy; 2 = cut)
      ddataObject.SetData("Preferred DropEffect", true, new MemoryStream(new Byte[] { 5, 0, 0, 0 }));
      ddataObject.SetData("Shell IDList Array", true, this.SelectedItems.ToArray().CreateShellIDList());
      F.Clipboard.SetDataObject(ddataObject, true);
    }

    public void PasteAvailableFiles() {
      var handle = this.Handle;
      var view = this;
      var thread = new Thread(() => {
        var dataObject = F.Clipboard.GetDataObject();
        var dropEffect = dataObject.ToDropEffect();
        if (dataObject != null && dataObject.GetDataPresent("Shell IDList Array")) {
          var shellItemArray = dataObject.ToShellItemArray();
          var items = shellItemArray.ToArray();

          try {
            var sink = new FOperationProgressSink(view);
            var controlItem = FileSystemListItem.InitializeWithIShellItem(this.LVHandle, items.First()).Parent;
            var fo = new IIFileOperation(sink, handle, true, controlItem.Equals(this.CurrentFolder));
            if (dropEffect == System.Windows.DragDropEffects.Copy) {
              fo.CopyItems(shellItemArray, this.CurrentFolder);
            } else {
              fo.MoveItems(shellItemArray, this.CurrentFolder);
            }

            fo.PerformOperations();
            Marshal.ReleaseComObject(shellItemArray);
          } catch (SecurityException) {
            throw;
          }
        } else if (dataObject != null && dataObject.GetDataPresent("FileDrop")) {
          var items = ((String[])dataObject.GetData("FileDrop")).Select(s => ShellItem.ToShellParsingName(s).ComInterface).ToArray();
          try {
            var sink = new FOperationProgressSink(view);
            var controlItem = FileSystemListItem.InitializeWithIShellItem(this.LVHandle, items.First()).Parent;
            var fo = new IIFileOperation(sink, handle, true, controlItem.Equals(this.CurrentFolder));
            foreach (var item in items) {
              if (dropEffect == System.Windows.DragDropEffects.Copy)
                fo.CopyItem(item, this.CurrentFolder);
              else
                fo.MoveItem(item, this.CurrentFolder, null);
            }

            fo.PerformOperations();
          } catch (SecurityException) {
            throw;
          }
        }
        this.LargeImageList.SupressThumbnailGeneration(false);
      });

      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

    public void DoCopy(IListItemEx destination) => Do_Copy_OR_Move_Helper(true, destination, this.SelectedItems.Select(s => s.ComInterface).ToArray());

    public void DoCopy(System.Windows.IDataObject dataObject, IListItemEx destination) => Do_Copy_OR_Move_Helper(true, destination, dataObject.ToShellItemArray().ToArray());

    public void DoCopy(F.IDataObject dataObject, IListItemEx destination) => Do_Copy_OR_Move_Helper_2(true, destination, dataObject);

    public void DoMove(System.Windows.IDataObject dataObject, IListItemEx destination) => Do_Copy_OR_Move_Helper(false, destination, dataObject.ToShellItemArray().ToArray());

    public void DoMove(IListItemEx destination) => Do_Copy_OR_Move_Helper(false, destination, this.SelectedItems.Select(s => s.ComInterface).ToArray());

    public void DoMove(F.IDataObject dataObject, IListItemEx destination) => Do_Copy_OR_Move_Helper_2(false, destination, dataObject);

    public void DeleteSelectedFiles(Boolean isRecycling) {
      var handle = this.Handle;
      var view = this;
      var thread = new Thread(() => {
        var sink = new FOperationProgressSink(view);
        var fo = new IIFileOperation(sink, handle, isRecycling);
        foreach (var item in this.SelectedItems) {
          fo.DeleteItem(item);
          this.BeginInvoke(new MethodInvoker(() => {
            this._IIListView.SetItemState(item.ItemIndex, LVIF.LVIF_STATE, LVIS.LVIS_SELECTED, 0);
          }));
        }

        fo.PerformOperations();
        if (isRecycling) {
          this.RaiseRecycleBinUpdated();
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    public void RenameShellItem(IShellItem item, String newName, Boolean isAddFileExtension, String extension = "") {
      var handle = this.Handle;
      var sink = new FOperationProgressSink(this);
      var fo = new IIFileOperation(sink, handle, false);
      fo.RenameItem(item, isAddFileExtension ? newName + extension : newName);
      fo.PerformOperations();
      if (fo.GetAnyOperationAborted()) {
        this._IsCanceledOperation = true;
      }
    }

    /// <summary>
    /// Resizes the icons
    /// </summary>
    /// <param name="value">The icon size you want</param>
    public void ResizeIcons(Int32 value) {
      try {
        IconSize = value;
        foreach (var obj in this.Items.ToArray()) {
          obj.IsIconLoaded = false;
          obj.IsNeedRefreshing = true;
        }

        this.LargeImageList.ResizeImages(value);
        this.LargeImageList.AttachToListView(this, 0);
        this.SmallImageList.AttachToListView(this, 1);
        var newW = 0;
        var newH = 0;
        this._IIListView?.SetIconSpacing(value + 45, value + 38, out newW, out newH);
        //User32.SendMessage(this.LVHandle, MSG.LVM_SETICONSPACING, 0, (IntPtr)User32.MAKELONG((Int32)(value * 1.3), (Int32)(value * 1.4)));
      } catch (Exception) {
      }
    }

    /// <summary>Selects all items and sets this to focus</summary>
    public void SelectAll() {
      this._IIListView.SetItemState(-1, LVIF.LVIF_STATE, LVIS.LVIS_SELECTED, LVIS.LVIS_SELECTED);
      this.Focus();
    }

    /// <summary>
    /// Selects only the specified items. First runs <see cref="DeSelectAllItems">DeSelectAllItems</see> Then selects all items on a separate thread.
    /// </summary>
    /// <param name="shellObjectArray"></param>
    public void SelectItems(IListItemEx[] shellObjectArray) {
      this.DeSelectAllItems();
      var selectionThread = new Thread(() => {
        foreach (var item in shellObjectArray) {
          try {
            var exestingItem = this.Items.FirstOrDefault(s => s.Equals(item));
            var itemIndex = exestingItem?.ItemIndex ?? -1;
            var lvii = new LVITEMINDEX() { iItem = itemIndex, iGroup = this.GetGroupIndex(itemIndex) };
            var lvi = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = LVIS.LVIS_SELECTED };
            User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMINDEXSTATE, ref lvii, ref lvi);
          } catch (Exception) {
            //catch the given key was not found. It happen on fast delete of items
          }
        }
        this.Focus();
      });

      selectionThread.SetApartmentState(ApartmentState.STA);
      selectionThread.Start();
    }

    /// <summary>
    /// Set this to focus then select an item by its index
    /// </summary>
    /// <param name="index">Index of item</param>
    /// <param name="ensureVisability">Ensure that the item is visible?</param>
    /// <param name="deselectOthers">Deselect all other items?</param>
    public void SelectItemByIndex(Int32 index, Boolean ensureVisability = false, Boolean deselectOthers = false) {
      this.Focus();
      if (deselectOthers) {
        this.BeginInvoke((Action)(() => {
          this._IIListView.SetItemState(-1, LVIF.LVIF_STATE, LVIS.LVIS_SELECTED, 0);
        }));
      }

      var lvii = this.ToLvItemIndex(index);
      var lvi = new LVITEM() { mask = LVIF.LVIF_STATE, stateMask = LVIS.LVIS_SELECTED, state = LVIS.LVIS_SELECTED };
      User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMINDEXSTATE, ref lvii, ref lvi);

      if (ensureVisability) {
        this.BeginInvoke((Action)(() => this._IIListView.EnsureItemVisible(lvii, true)));
      }
    }

    public void SetColInView(Collumns col, Boolean remove) {
      if (remove) {
        var theColumn = this.Collumns.FirstOrDefault(s => s.pkey.fmtid == col.pkey.fmtid && s.pkey.pid == col.pkey.pid);
        if (theColumn != null) {
          var colIndex = this.Collumns.IndexOf(theColumn);
          this.Collumns.Remove(theColumn);
          User32.SendMessage(this.LVHandle, MSG.LVM_DELETECOLUMN, colIndex, 0);
          if (theColumn.ID == this.LastSortedColumnId) {
            User32.SendMessage(this.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, -1, 0);
          }
        }
      } else if (!this.Collumns.Any(s => s.pkey.fmtid == col.pkey.fmtid && s.pkey.pid == col.pkey.pid)) {
        this.Collumns.Add(col);
        var column = col.ToNativeColumn(this.View == ShellViewStyle.Details);
        var colIndex = this.Collumns.Count - 1;
        User32.SendMessage(this.LVHandle, MSG.LVM_INSERTCOLUMN, colIndex, ref column);
        if (col.ID == this.LastSortedColumnId) {
          this.SetSortIcon(colIndex, this.LastSortOrder);
          User32.SendMessage(this.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, colIndex, 0);
        }
        if (this.View != ShellViewStyle.Details) {
          this.AutosizeColumn(this.Collumns.Count - 1, -2);
        }
      }

      var headerhandle = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
      for (var i = 0; i < this.Collumns.Count; i++) {
        this.Collumns[i].SetSplitButton(headerhandle, i);
      }

		this.OnListViewCollumnsChanged?.Invoke(this, new CollumnsChangedArgs(remove));
    }

    public void RemoveAllCollumns() {
      for (var i = this.Collumns.ToArray().Count() - 1; i > 1; i--) {
        this.Collumns.RemoveAt(i);
        User32.SendMessage(this.LVHandle, MSG.LVM_DELETECOLUMN, i, 0);
      }
    }

    public void SetSortCollumn(Boolean isReorder, Collumns column, SortOrder order, Boolean reverseOrder = true) {
      if (column == null) return;
      try {
        var itemsArray = this.Items;
        var selectedItems = this.SelectedItems.ToArray();
        if (column.ID == this.LastSortedColumnId && reverseOrder) {
          // Reverse the current sort direction for this column.
          this.LastSortOrder = this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        } else {
          // Set the column number that is to be sorted; default to ascending.
          this.LastSortedColumnId = column.ID;
          this.LastSortOrder = order;
        }
        if (isReorder) {
          var itemsQuery = itemsArray.Where(w => this.ShowHidden || !w.IsHidden).OrderByDescending(o => o.IsFolder);
          if (column.CollumnType != typeof(String)) {
            if (order == SortOrder.Ascending) {
              this.Items =
                              itemsQuery.ThenBy(
                                              o =>
                                                              o.GetPropertyValue(column.pkey, typeof(String)).Value ?? "1")
                                              .ToList();
            } else {
              this.Items =
                              itemsQuery.ThenByDescending(
                                              o =>
                                                              o.GetPropertyValue(column.pkey, typeof(String)).Value ?? "1")
                                              .ToList();
            }
          } else {
            if (order == SortOrder.Ascending) {
              this.Items =
                              itemsQuery.ThenBy(
                                              o =>
                                                              o.GetPropertyValue(column.pkey, typeof(String)).Value == null
                                                                              ? "1"
                                                                              : o.GetPropertyValue(column.pkey, typeof(String)).Value.ToString(), NaturalStringComparer.Default)
                                              .ToList();
            } else {
              this.Items =
                              itemsQuery.ThenByDescending(
                                              o =>
                                                              o.GetPropertyValue(column.pkey, typeof(String)).Value == null
                                                                              ? "1"
                                                                              : o.GetPropertyValue(column.pkey, typeof(String)).Value.ToString(), NaturalStringComparer.Default)
                                              .ToList();
            }
          }
          var i = 0;
          this.Items.ForEach(e => e.ItemIndex = i++);
        }
        this.BeginInvoke((Action)(() => {
          this._IIListView.SetItemCount(this.Items.Count, 0x2);
        }));

        var colIndexReal = this.Collumns.IndexOf(this.Collumns.FirstOrDefault(w => w.ID == this.LastSortedColumnId));
        if (colIndexReal > -1) {
          User32.SendMessage(this.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, colIndexReal, 0);
          this.SetSortIcon(colIndexReal, order);
        } else {
          User32.SendMessage(this.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, -1, 0);
        }

        if (!this.IsRenameInProgress) this.SelectItems(selectedItems);
      } catch {
      }
    }

    /// <summary>
    /// Navigate to a folder, set it as the current folder and optionally save the folder's settings to the database.
    /// </summary>
    /// <param name="destination">The folder you want to navigate to.</param>
    /// <param name="saveFolderSettings">Should the folder's settings be saved?</param>
    /// <param name="isInSameTab"></param>
    /// <param name="refresh">Should the List be Refreshed?</param>
    public void Navigate_Full(IListItemEx destination, Boolean saveFolderSettings, Boolean isInSameTab = false, Boolean refresh = false) {
      this.IsSearchNavigating = false;

      if (destination == null || !destination.IsFolder) return;
      _ResetEvent.Set();
      Navigate(destination, isInSameTab, refresh, this.IsNavigationInProgress);
    }

    /// <summary>
    /// Navigates to a search folder
    /// </summary>
    /// <param name="SearchQuery">The query of the search</param>
    /// <param name="saveFolderSettings">Should the folder's settings be saved?</param>
    /// <param name="isInSameTab"></param>
    /// <param name="refresh">Should the List be Refreshed?</param>
    public void Navigate_Full(String SearchQuery, Boolean saveFolderSettings, Boolean isInSameTab = false, Boolean refresh = false) {
      this.IsSearchNavigating = true;
      if (saveFolderSettings) SaveSettingsToDatabase(this.CurrentFolder);

      _ResetEvent.Set();
      var searchCondition = SearchConditionFactory.ParseStructuredQuery(this.PrepareSearchQuery(SearchQuery));
      var shellItem = new ShellItem(this.CurrentFolder.PIDL);
      var searchFolder = new ShellSearchFolder(searchCondition, shellItem);
      IListItemEx searchItem = FileSystemListItem.ToFileSystemItem(this.LVHandle, searchFolder);
      this.NavigateSearch(searchItem, isInSameTab, refresh, this.IsNavigationInProgress);
    }

    /// <summary>Invalidates the director</summary>
    /// <remarks>Starts restarts <see cref="_UnvalidateTimer"/></remarks>
    public void UnvalidateDirectory() {
      Action worker = () => {
        if (this._UnvalidateTimer.Enabled) this._UnvalidateTimer.Stop();
        this._UnvalidateTimer.Start();
      };

      if (this.InvokeRequired)
        this.BeginInvoke((Action)(() => worker()));
      else
        worker();
    }

    /// <summary>Cancels navigation</summary>
    public void CancelNavigation() {
      this._SearchTimer.Stop();
      this.IsCancelRequested = true;
      if (this._Threads.Any()) {
        _Mre.Set();
        this._ResetEvent.Set();
        foreach (var thread in this._Threads.ToArray()) {
          thread.Abort();
          this._Threads.Remove(thread);
        }
      }
    }

    /// <summary>Disables/Removes grouping</summary>
    public void DisableGroups() {
      if (!this.IsGroupsEnabled) return;
      this.Groups.Clear();
      this._IIListView.RemoveAllGroups();
      this._IIListView.EnableGroupView(0);
      this._IIListView.SetOwnerDataCallback(IntPtr.Zero);
      this.LastGroupCollumn = null;
      this.LastGroupOrder = SortOrder.None;
      this.IsGroupsEnabled = false;
    }

    /// <summary>Enables/Adds groupings</summary>
    public void EnableGroups() {
      if (this.IsGroupsEnabled || this.IsSearchNavigating) return;
      var ptr = Marshal.GetComInterfaceForObject(new VirtualGrouping(this), typeof(IOwnerDataCallback));
      this._IIListView.SetOwnerDataCallback(ptr);
      Marshal.Release(ptr);
      this._IIListView.EnableGroupView(1);
      this.IsGroupsEnabled = true;
    }

    /// <summary>
    /// Generates Groups
    /// </summary>
    /// <param name="col">The column you want to group by</param>
    /// <param name="reversed">Reverse order (This needs to be explained better)</param>
    [HandleProcessCorruptedStateExceptions]
    public void GenerateGroupsFromColumn(Collumns col, Boolean reversed = false) {
      if (col == null) return;
      this.BeginInvoke(new MethodInvoker(() => this._IIListView.RemoveAllGroups()));

      this.Groups.Clear();
      if (col.CollumnType == typeof(String)) {
        if (Settings.BESettings.IsTraditionalNameGrouping) {
          var groups = this.Items.ToArray().GroupBy(k => k.DisplayName.ToUpperInvariant().First(), e => e).OrderBy(o => o.Key);
          var i = reversed ? groups.Count() - 1 : 0;
          foreach (var group in groups) {
            var groupItems = group.Select(s => s).ToArray();
            groupItems.ToList().ForEach(c => c.GroupIndex = this.Groups.Count);
            this.Groups.Add(new ListViewGroupEx() { Items = groupItems, Index = reversed ? i-- : i++, Header = $"{group.Key.ToString()} ({groupItems.Count()})" });
          }
        } else {
          var i = reversed ? 3 : 0;

          Action<String, String, Boolean> addNameGroup = (String char1, String char2, Boolean isOthers) => {
            var testgrn = new ListViewGroupEx();
            if (isOthers) {
              testgrn.Items =
                              this.Items.Where(
                                              w =>
                                                              (w.DisplayName.ToUpperInvariant().First() < Char.Parse("A") ||
                                                              w.DisplayName.ToUpperInvariant().First() > Char.Parse("Z")) && (w.DisplayName.ToUpperInvariant().First() < Char.Parse("0") || w.DisplayName.ToUpperInvariant().First() > Char.Parse("9"))).ToArray();
            } else {
              testgrn.Items =
                              this.Items.Where(
                                              w =>
                                                              w.DisplayName.ToUpperInvariant().First() >= Char.Parse(char1) &&
                                                              w.DisplayName.ToUpperInvariant().First() <= Char.Parse(char2)).ToArray();
            }
            testgrn.Header = isOthers
                            ? char1 + $" ({testgrn.Items.Count()})"
                            : char1 + " - " + char2 + $" ({testgrn.Items.Count()})";
            testgrn.Index = reversed ? i-- : i++;
            this.Groups.Add(testgrn);
          };

          addNameGroup("0", "9", false);
          addNameGroup("A", "H", false);
          addNameGroup("I", "P", false);
          addNameGroup("Q", "Z", false);
          addNameGroup("Others", String.Empty, true);
        }
      } else if (col.CollumnType == typeof(Int64)) {
        var j = reversed ? 7 : 0;
        //TODO: Upgrade next to use an Action<>

        var uspec = new ListViewGroupEx();
        uspec.Items = this.Items.Where(w => w.IsFolder).ToArray();
        uspec.Header = $"Unspecified ({uspec.Items.Count()})";
        uspec.Index = reversed ? j-- : j++;
        this.Groups.Add(uspec);

        var testgrn = new ListViewGroupEx();
        testgrn.Items =
                this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) == 0 && !w.IsFolder)
                        .ToArray();
        testgrn.Header = $"Empty ({testgrn.Items.Count()})";
        testgrn.Index = reversed ? j-- : j++;
        this.Groups.Add(testgrn);

        var testgr = new ListViewGroupEx();
        testgr.Items =
                this.Items.Where(
                        w =>
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) > 0 &&
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) <= 10 * 1024).ToArray();
        testgr.Header = $"Very Small ({testgr.Items.Count()})";
        testgr.Index = reversed ? j-- : j++;
        this.Groups.Add(testgr);

        var testgr2 = new ListViewGroupEx();
        testgr2.Items =
                this.Items.Where(
                        w =>
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) > 10 * 1024 &&
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) <= 100 * 1024).ToArray();
        testgr2.Header = $"Small ({testgr2.Items.Count()})";
        testgr2.Index = reversed ? j-- : j++;
        this.Groups.Add(testgr2);

        var testgr3 = new ListViewGroupEx();
        testgr3.Items =
                this.Items.Where(
                        w =>
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) > 100 * 1024 &&
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) <= 1 * 1024 * 1024).ToArray();
        testgr3.Header = $"Medium ({testgr3.Items.Count()})";
        testgr3.Index = reversed ? j-- : j++;
        this.Groups.Add(testgr3);

        var testgr4 = new ListViewGroupEx();
        testgr4.Items =
                this.Items.Where(
                        w =>
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) > 1 * 1024 * 1024 &&
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) <= 16 * 1024 * 1024).ToArray();
        testgr4.Header = $"Big ({testgr4.Items.Count()})";
        testgr4.Index = reversed ? j-- : j++;
        this.Groups.Add(testgr4);

        var testgr5 = new ListViewGroupEx();
        testgr5.Items =
                this.Items.Where(
                        w =>
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) > 16 * 1024 * 1024 &&
                                Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) <= 128 * 1024 * 1024).ToArray();
        testgr5.Header = $"Huge ({testgr5.Items.Count()})";
        testgr5.Index = reversed ? j-- : j++;
        this.Groups.Add(testgr5);

        var testgr6 = new ListViewGroupEx();
        testgr6.Items =
                this.Items.Where(w => Convert.ToInt64(w.GetPropertyValue(col.pkey, typeof(Int64)).Value) > 128 * 1024 * 1024)
                        .ToArray();
        testgr6.Header = $"Gigantic ({testgr6.Items.Count()})";
        testgr6.Index = reversed ? j-- : j++;
        this.Groups.Add(testgr6);
      } else if (col.CollumnType == typeof(PerceivedType)) {
        var groups = this.Items.GroupBy(k => k.GetPropertyValue(col.pkey, typeof(String)).Value, e => e).OrderBy(o => o.Key);
        var i = reversed ? groups.Count() - 1 : 0;
        foreach (var group in groups.ToArray()) {
          var groupItems = group.Select(s => s).ToArray();
          this.Groups.Add(new ListViewGroupEx() { Items = groupItems, Index = reversed ? i-- : i++, Header = $"{((PerceivedType)group.Key).ToString()} ({groupItems.Count()})" });
        }
      } else {
        var groups = this.Items.GroupBy(k => k.GetPropertyValue(col.pkey, typeof(String)).Value, e => e).OrderBy(o => o.Key);
        var i = reversed ? groups.Count() - 1 : 0;
        foreach (var group in groups.ToArray()) {
          var groupItems = group.Select(s => s).ToArray();
          this.Groups.Add(new ListViewGroupEx() { Items = groupItems, Index = reversed ? i-- : i++, Header = $"{group.Key.ToString()} ({groupItems.Count()})" });
        }
      }

      if (reversed) this.Groups.Reverse();
      this.Invoke(new MethodInvoker(() => {
        this._IIListView.SetItemCount(this.Items.Count, 0x2);
      }));
      foreach (var group in this.Groups.ToArray()) {
        group.Items.ToList().ForEach(e => e.GroupIndex = group.Index);
        var nativeGroup = group.ToNativeListViewGroup();
        var insertedPosition = -1;
        this.BeginInvoke(new MethodInvoker(() => {
          this._IIListView.InsertGroup(-1, nativeGroup, out insertedPosition);
        }));
      }

      this.LastGroupCollumn = col;
      this.LastGroupOrder = reversed ? SortOrder.Descending : SortOrder.Ascending;
      this.SetSortIcon(this.Collumns.IndexOf(col), this.LastGroupOrder);
    }

    /// <summary>
    /// Sets the Sort order of the Groups
    /// </summary>
    /// <param name="reverse">Reverse the Current Sort Order?</param>
    public void SetGroupOrder(Boolean reverse = true) => GenerateGroupsFromColumn(LastGroupCollumn, reverse && LastGroupOrder == SortOrder.Ascending);

    /// <summary>Returns the first selected item OR null if there is no selected item</summary>
    [DebuggerStepThrough]
    public IListItemEx GetFirstSelectedItem() {
      var lvi = this.ToLvItemIndex(-1);
      User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
      return lvi.iItem == -1 || this.Items.Count < lvi.iItem ? null : this.Items[lvi.iItem];
    }

    /// <summary>Returns the first selected item's index OR -1 if there is no selected item</summary>
    public Int32 GetFirstSelectedItemIndex() {
      var lvi = this.ToLvItemIndex(-1);
      User32.SendMessage(this.LVHandle, LVM.GETNEXTITEMINDEX, ref lvi, LVNI.LVNI_SELECTED);
      return lvi.iItem;
    }

    /// <summary>
    /// Creates a new folder in the current directory and assigns a default name if none is specified. Returns the name
    /// </summary>
    /// <param name="name">The name of the new folder</param>
    /// <returns>Returns the name and assigns a default name if none is specified</returns>
    public String CreateNewFolder(String name) {
      if (String.IsNullOrEmpty(name)) {
        name = User32.LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 30396, "New Folder");
      }

      var fo = new IIFileOperation(this.Handle, false);
      fo.NewItem(this.CurrentFolder, name, FileAttributes.Directory | FileAttributes.Normal);
      fo.PerformOperations();

      return name;
    }

    /// <summary>
    /// Creates a new library folder
    /// </summary>
    /// <param name="name">The name of the lbrary folder youi want</param>
    /// <returns></returns>
    public ShellLibrary CreateNewLibrary(String name) {
      String endname = name;
      Int32 suffix = 0;
      ShellLibrary lib = null;
      try {
        lib = ShellLibrary.Load(endname, true);
      } catch {
      }

      if (lib != null) {
        do {
          endname = name + $"({++suffix})";
          try {
            lib = ShellLibrary.Load(endname, true);
          } catch {
            lib = null;
          }
        } while (lib != null);
      }

      return new ShellLibrary(endname, false);
    }

    /// <summary>
    /// Sets the folder's icon
    /// </summary>
    /// <param name="wszPath">??</param>
    /// <param name="wszExpandedIconPath">??</param>
    /// <param name="iIcon">??</param>
    public void SetFolderIcon(String wszPath, String wszExpandedIconPath, Int32 iIcon) {
      var fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS() { iIconIndex = iIcon, cchIconFile = 0, dwMask = Shell32.FCSM_ICONFILE };
      fcs.dwSize = (UInt32)Marshal.SizeOf(fcs);
      fcs.pszIconFile = wszExpandedIconPath.Replace(@"\\", @"\");
      // Set the folder icon
      HResult hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath.Replace(@"\\", @"\"), Shell32.FCS_FORCEWRITE);
      if (hr == HResult.S_OK) this.UpdateIconCacheForFolder(wszPath); // Update the icon cache			
      this.RefreshItem(this._SelectedIndexes[0], true);
    }

    private void UpdateIconCacheForFolder(String wszPath) {
      var sfi = new SHFILEINFO();
      var res = Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(wszPath), 0, out sfi, (Int32)Marshal.SizeOf(sfi),
                      SHGFI.IconLocation);
      Int32 iIconIndex = Shell32.Shell_GetCachedImageIndex(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0);
      Shell32.SHUpdateImage(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0x0002, iIconIndex);
      Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEIMAGE,
                      Shell32.HChangeNotifyFlags.SHCNF_DWORD | Shell32.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, IntPtr.Zero,
                      (IntPtr)sfi.iIcon);
    }

    /// <summary>
    /// Removes the folder's icon
    /// </summary>
    /// <param name="wszPath">??</param>
    /// <returns></returns>
    public HResult ClearFolderIcon(String wszPath) {
      var fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS() { dwMask = Shell32.FCSM_ICONFILE };
      fcs.dwSize = (UInt32)Marshal.SizeOf(fcs);

      HResult hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath, Shell32.FCS_FORCEWRITE);
      if (hr == HResult.S_OK) {
        // Update the icon cache
        this.UpdateIconCacheForFolder(wszPath.Replace(@"\\", @"\"));
      }

      this.RefreshItem(this._SelectedIndexes[0]);
      return hr;
    }

    /// <summary>Sets focus to tis control then deselects all items</summary>
    public void DeSelectAllItems() {
      this.BeginInvoke(new MethodInvoker(() => {
        this._IIListView.SetItemState(-1, LVIF.LVIF_STATE, LVIS.LVIS_SELECTED, 0);
      }));
      this.Focus();
    }

    public Boolean IsFocusAllowed = true;

	/// <summary>
	/// Gives the ShellListView focus
	/// </summary>
	/// <param name="isActiveCheck">Require this application's MainWindow to be activate the control</param>
	/// <param name="isForce">Force this to make the control active no matter what</param>
	public void Focus(Boolean isActiveCheck = true, Boolean isForce = false) {
      if (System.Windows.Application.Current == null) return;
      if (User32.GetForegroundWindow() != this.LVHandle) {
        this.Invoke(new MethodInvoker(() => {
          if (isForce || ((System.Windows.Application.Current.MainWindow.IsActive || !isActiveCheck) && (IsFocusAllowed && this.Bounds.Contains(Cursor.Position))))
            User32.SetFocus(this.LVHandle);
        }));
      }
    }

    public Int32 GetSelectedCount() => (Int32)User32.SendMessage(this.LVHandle, MSG.LVM_GETSELECTEDCOUNT, 0, 0);

    /// <summary>Inverse the selection of items</summary>
    public void InvertSelection() {
      Int32 itemCount = 0;
      this._IIListView.GetItemCount(out itemCount);

      for (Int32 n = 0; n < itemCount; ++n) {
        var state = (LVIS)0;
        this._IIListView.GetItemState(n, LVIF.LVIF_STATE, LVIS.LVIS_SELECTED, out state);
        this._IIListView.SetItemState(n, LVIF.LVIF_STATE, LVIS.LVIS_SELECTED, (state & LVIS.LVIS_SELECTED) == LVIS.LVIS_SELECTED ? 0 : LVIS.LVIS_SELECTED);
      }
      this.Focus();
    }

    /// <summary>
    /// Automatically resize all controls
    /// </summary>
    /// <param name="autosizeParam">??</param>
    public void AutosizeAllColumns(Int32 autosizeParam) {
      for (Int32 i = 0; i < this.Collumns.Count; i++) {
        AutosizeColumn(i, autosizeParam);
      }
    }

    #endregion Public Methods

    #region Private Methods

    private void EndLabelEdit(Boolean isCancel = false) {
      if (this._ItemForRename == -1 && !this.IsRenameInProgress)
        return;
      if (this.EndItemLabelEdit != null) {
        this.EndItemLabelEdit.Invoke(this, isCancel);
        if (this._ItemForRename > -1) {
          //this.UpdateItem(this._ItemForRename);
          //this.RefreshItem(this._ItemForRename, true);
        }
        this._ItemForRename = -1;
        this._IsCanceledOperation = isCancel;
      }
      this.Focus();
    }

    private Boolean ThreadRun_Helper(SyncQueue<Int32?> queue, Boolean useComplexCheck, ref Int32? index) {
      try {
        index = queue.Dequeue();
        if (index == null) {
          return false;
        } else {
          var result = User32.SendMessage(this.LVHandle, Interop.MSG.LVM_ISITEMVISIBLE, index.Value, 0) != IntPtr.Zero;
          //var itemBounds = new User32.RECT();
          //var lvi = this.ToLvItemIndex(index.Value);

          //User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);

          //var r = new Rectangle(itemBounds.Left, itemBounds.Top, itemBounds.Right - itemBounds.Left, itemBounds.Bottom - itemBounds.Top);

          //if (useComplexCheck)
          //	return index < Items.Count && r.IntersectsWith(this.ClientRectangle);
          //else
          //	return r.IntersectsWith(this.ClientRectangle);
          return result;
        }
      } catch {
        return false;
      }
    }

    private async void RetrieveThumbnailByIndex(Int32 index) {
      await Task.Run(() => {
        if (this.IsCancelRequested) return;

        //F.Application.DoEvents();
        var itemBounds = new User32.RECT();
        var lvi = new LVITEMINDEX() { iItem = index, iGroup = this.GetGroupIndex(index) };
        User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);

        var r = new Rectangle(itemBounds.Left, itemBounds.Top, itemBounds.Right - itemBounds.Left, itemBounds.Bottom - itemBounds.Top);

        if (r.IntersectsWith(this.ClientRectangle)) {
          var sho = Items[index];
          var icon = sho.GetHBitmap(IconSize, true, true);
          sho.IsThumbnailLoaded = true;
          sho.IsNeedRefreshing = false;
          if (icon != IntPtr.Zero) {
            Int32 width = 0, height = 0;
            Gdi32.ConvertPixelByPixel(icon, out width, out height);
            sho.IsOnlyLowQuality = (width > height && width != IconSize) || (width < height && height != IconSize) || (width == height && width != IconSize);
            Gdi32.DeleteObject(icon);
            this.RedrawItem(index);
          }
        }
      });
    }

    private String PrepareSearchQuery(String query) {
      var prefix = "System.Generic.String:";
      if (query.StartsWith("*.")) prefix = "fileextension:";
      if (query.Contains(":")) prefix = String.Empty;
      return prefix + query;
    }

    /// <summary>
    /// Navigate to a folder.
    /// </summary>
    /// <param name="destination">The folder you want to navigate to.</param>
    /// <param name="isInSameTab">Do the navigation happens in same tab</param>
    /// <param name="refresh">Should the List be Refreshed?</param>
    /// <param name="isCancel">this.IsNavigationCancelRequested = isCancel</param>
    [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
    [HandleProcessCorruptedStateExceptions]
    private void Navigate(IListItemEx destination, Boolean isInSameTab = false, Boolean refresh = false, Boolean isCancel = false) {
      this.SaveSettingsToDatabase(this.CurrentFolder);

      // TODO: Document isCancel Param better
      if (destination == null) {
        return;
      }
      destination = FileSystemListItem.ToFileSystemItem(destination.ParentHandle, destination.PIDL);
      if (this.RequestedCurrentLocation == destination && !refresh) {
        return;
      }
      //if (this.RequestedCurrentLocation != destination) {
      //  this.IsCancelRequested = true;
      //}
      this.LargeImageList.ResetEvent.Set();
      this.SmallImageList.ResetEvent.Set();
      this._ResetEvent.Set();

      if (this._Threads.Count > 0) {
        this._Mre.Set();
        this._ResetEvent.Set();
        this.LargeImageList.ResetEvent.Set();
        this.SmallImageList.ResetEvent.Set();
        foreach (var thread in this._Threads.ToArray()) {
          thread.Abort();
          this._Threads.Remove(thread);
        }
      }

      this._UnvalidateTimer.Stop();
      this._IsDisplayEmptyText = false;
      User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, 0, 0);
      this.DisableGroups();
      this.Focus(false, true);
      this._ItemForRename = -1;
      this._LastItemForRename = -1;
      this.Items.Clear();
      this._AddedItems.Clear();
      this.LargeImageList.ReInitQueues();
      this.SmallImageList.ReInitQueues();
      this._CuttedIndexes.Clear();

      FolderSettings folderSettings;
      var isThereSettings = false;

      isThereSettings = LoadSettingsFromDatabase(destination, out folderSettings);
      this.RequestedCurrentLocation = destination;
      if (!refresh)
        Navigating?.Invoke(this, new NavigatingEventArgs(destination, isInSameTab));

      var columns = new Collumns();
      Int32 CurrentI = 0, LastI = 0, K = 0;
      this.IsNavigationInProgress = true;

      _ResetTimer.Stop();
      if (isThereSettings) {
        if (folderSettings.Columns != null) {
          this.RemoveAllCollumns();
          foreach (var collumn in folderSettings.Columns.Elements()) {
            var theColumn = this.AllAvailableColumns.FirstOrDefault(w => w.Value.ID == collumn.Attribute("ID").Value).Value;//.Single();
            if (theColumn == null) continue;
            if (this.Collumns.Any(c => c.ID == theColumn?.ID)) continue;
            if (collumn.Attribute("Width").Value != "0") theColumn.Width = Convert.ToInt32(collumn.Attribute("Width").Value);
            this.Collumns.Add(theColumn);
            var column = theColumn.ToNativeColumn(folderSettings.View == ShellViewStyle.Details);
            User32.SendMessage(this.LVHandle, MSG.LVM_INSERTCOLUMN, this.Collumns.Count - 1, ref column);
            if (folderSettings.View != ShellViewStyle.Details) this.AutosizeColumn(this.Collumns.Count - 1, -2);
          }
        }
      } else {
        this.RemoveAllCollumns();
        this.AddDefaultColumns(false, true);
      }

      if (!String.IsNullOrEmpty(folderSettings.GroupCollumn)) {
        var colData = this.AllAvailableColumns.FirstOrDefault(w => w.Value.ID == folderSettings.GroupCollumn).Value;
        if (colData != null)
          this.EnableGroups();
        else
          this.DisableGroups();
      } else {
        this.DisableGroups();
      }

      columns = this.AllAvailableColumns.FirstOrDefault(w => w.Value.ID == folderSettings.SortColumn).Value;
      this.IsViewSelectionAllowed = false;
      if (!isThereSettings) {
        this.View = ShellViewStyle.Details;
      }

      if (folderSettings.View == ShellViewStyle.Details || folderSettings.View == ShellViewStyle.SmallIcon || folderSettings.View == ShellViewStyle.List) {
        this.ResizeIcons(16);
        this.View = folderSettings.View;
      } else if (folderSettings.IconSize >= 16) {
        this.ResizeIcons(folderSettings.IconSize);
        var view = (ShellViewStyle)folderSettings.IconSize;
        if (folderSettings.IconSize != 48 && folderSettings.IconSize != 96 && folderSettings.IconSize != 256)
          this.View = ShellViewStyle.Thumbnail;
        else
          this.View = folderSettings.View;
      }

      this.IsViewSelectionAllowed = true;
      this.Invoke((Action)(() => this._NavWaitTimer.Start()));

      var navigationThread = new Thread(() => {
        this.IsCancelRequested = false;
        destination = FileSystemListItem.ToFileSystemItem(destination.ParentHandle, destination.PIDL);
        if (destination.IsFileSystem) {
          if (this._FsWatcher != null) {
            this._FsWatcher.EnableRaisingEvents = false;
            this._FsWatcher.Dispose();
            try {
              this._FsWatcher = new FileSystemWatcher(@destination.ParsingName);
              //this._FsWatcher.InternalBufferSize = 64 * 1024 * 1024;
              this._FsWatcher.Changed += (sender, args) => {
                try {
                  var objUpdateItem = FileSystemListItem.ToFileSystemItem(this.LVHandle, args.FullPath);
                  if (objUpdateItem.IsInCurrentFolder(this.CurrentFolder)) {
                    var exisitingUItem = this.Items.ToArray().FirstOrDefault(w => w.Equals(objUpdateItem));
                    if (exisitingUItem != null)
                      this.RefreshItem(exisitingUItem.ItemIndex, true);

                    if (this.RequestedCurrentLocation != null && objUpdateItem.Equals(this.RequestedCurrentLocation))
                      this.UnvalidateDirectory();
                  }
                } catch (FileNotFoundException) {
                  //Probably a temporary file 
                  this._TemporaryFiles.Add(args.FullPath);
                } catch {
                }
              };
              this._FsWatcher.Error += (sender, args) => {
                var ex = args.GetException();
              };
              this._FsWatcher.Created += (sender, args) => {
                try {
                  //var existing = this.Items.FirstOrDefault(s => s.ParsingName.Equals(args.FullPath));
                  //if (existing != null) return;
                  if (Path.GetExtension(args.FullPath).ToLowerInvariant() == ".tmp" ||
                          Path.GetExtension(args.FullPath) == String.Empty) {
                    if (!this._TemporaryFiles.Contains(args.FullPath))
                      this._TemporaryFiles.Add(args.FullPath);
                  }
                  var obj = FileSystemListItem.ToFileSystemItem(this.LVHandle, args.FullPath);
                  if (obj.IsInCurrentFolder(this.CurrentFolder)) {
                    if (this.IsRenameNeeded) {
                      var existingItem = this.Items.ToArray().FirstOrDefault(s => s.Equals(obj));
                      if (existingItem == null) {
                        var itemIndex = this.InsertNewItem(obj);
                        this.SelectItemByIndex(itemIndex, true, true);
                        this.RenameSelectedItem(itemIndex);
                        this.IsRenameNeeded = false;
                      } else {
                        this.RenameSelectedItem(existingItem.ItemIndex);
                      }
                    } else {
                      if (this._ItemsQueue.Enqueue(new Tuple<ItemUpdateType, IListItemEx>(ItemUpdateType.Created, obj)))
                        this.UnvalidateDirectory();
                    }
                  }
                } catch (FileNotFoundException) {
                  this.QueueDeleteItem(args);
                } catch { }
              };

              this._FsWatcher.Deleted += (sender, args) => this.QueueDeleteItem(args);
              this._FsWatcher.Renamed += (sender, args) => { };
              this._FsWatcher.IncludeSubdirectories = false;
              this._FsWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.Attributes |
              NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            } catch (ArgumentException) {
              this._FsWatcher = new FileSystemWatcher();
            }
          }

          try {
            if (this._FsWatcher != null && !String.IsNullOrEmpty(this._FsWatcher.Path)) {
              this._FsWatcher.EnableRaisingEvents = true;
            }
          } catch (FileNotFoundException) { }
        }

        this.RequestedCurrentLocation = destination;
        var column = columns ?? this.AllAvailableColumns.Single(s => s.Value.ID == "A0").Value;
        var order = folderSettings.SortOrder;
        var content = destination;

        foreach (var shellItem in destination.IsNetworkPath ? destination.TakeWhile(shellItem => !this.IsCancelRequested) : content.TakeWhile(shellItem => !this.IsCancelRequested)) {
          CurrentI++;

          if (!this.RequestedCurrentLocation.Equals(shellItem.Parent) && this.IsNavigationCancelRequested) {
            this.IsNavigationCancelRequested = false;
            return;
          }

          if (this.ShowHidden || !shellItem.IsHidden) {
            shellItem.ItemIndex = K++;
            this.Items.Add(shellItem);
            if (CurrentI == 1) {
              this.Invoke((Action)(() => {
                this._NavWaitTimer.Stop();
                this._IsDisplayEmptyText = false;
                this._IIListView.ResetEmptyText();
              }));
            }
          }

          var delta = CurrentI - LastI;
          if (delta >= 4750) {
            LastI = CurrentI;
            this.BeginInvoke((MethodInvoker)(() => this._IIListView.SetItemCount(this.Items.Count, 0x2)));
          }
        }

        this.IsCancelRequested = false;
        this.IsNavigationInProgress = false;

        if (this.RequestedCurrentLocation.NavigationStatus != HResult.S_OK) {
          GC.Collect();
          Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
          if (this._Threads.Count <= 1) return;
          _Mre.Set();
          this._ResetEvent.Set();
          this._Threads[0].Abort();
          this._Threads.RemoveAt(0);
          return;
        }

        var headerhandle = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
        for (var i = 0; i < this.Collumns.Count; i++) {
          this.Collumns[i].SetSplitButton(headerhandle, i);
        }

        if (this.View != ShellViewStyle.Details) AutosizeAllColumns(-2);

        //User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, this.Items.Count, 0x2);

        var sortColIndex = this.Collumns.SingleOrDefault(s => s.ID == columns?.ID)?.Index;
        if (sortColIndex != null) {
          this.SetSortIcon(sortColIndex.Value, folderSettings.SortOrder == SortOrder.None ? SortOrder.Ascending : folderSettings.SortOrder);
        }

        if (isThereSettings) {
          if (columns?.ID == "A0" && String.Equals(this.RequestedCurrentLocation.ParsingName, KnownFolders.Computer.ParsingName, StringComparison.InvariantCultureIgnoreCase))
            this.SetSortCollumn(true, this.AvailableColumns().Single(s => s.Value.ID == "A180").Value, SortOrder.Ascending, false);
          else
            this.SetSortCollumn(true, columns, folderSettings.SortOrder, false);
        } else if (String.Equals(this.RequestedCurrentLocation.ParsingName, KnownFolders.Computer.ParsingName, StringComparison.InvariantCultureIgnoreCase)) {
          this.Items = this.Items.OrderBy(o => o.ParsingName).ToList();
          var i = 0;
          this.Items.ForEach(e => e.ItemIndex = i++);
          User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, this.Items.Count, 0x2);
        } else {
          this.Items = this.Items.OrderByDescending(o => o.IsFolder).ThenBy(o => o.DisplayName).ToList();
          var i = 0;
          this.Items.ToList().ForEach(e => e.ItemIndex = i++);
          User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, this.Items.Count, 0x2);
        }

        if (this.IsGroupsEnabled) {
          var colData = this.AllAvailableColumns.FirstOrDefault(w => w.Value.ID == folderSettings.GroupCollumn).Value;
          this.GenerateGroupsFromColumn(colData, folderSettings.GroupOrder == SortOrder.Descending);
        }

        if (!isThereSettings) {
          this.LastSortedColumnId = "A0";
          this.LastSortOrder = SortOrder.Ascending;
          this.SetSortIcon(0, SortOrder.Ascending);
          User32.SendMessage(this.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, 0, 0);
        }

        this._IsDisplayEmptyText = false;
        this.BeginInvoke((Action)(() => {
          var navArgs = new NavigatedEventArgs(this.RequestedCurrentLocation, this.CurrentFolder, isInSameTab);
          this.CurrentFolder = this.RequestedCurrentLocation;
          if (!refresh)
            Navigated?.Invoke(this, navArgs);
        }));

        this.Invoke((Action)(() => {
          this._NavWaitTimer.Stop();
          this._IsDisplayEmptyText = false;
          this._IIListView.ResetEmptyText();
        }));

        GC.Collect();
        Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        this.Invoke((Action)(() => {
          if (this._SearchTimer.Enabled)
            this._SearchTimer.Stop();
        }));

        _Mre.Reset();
        _Mre.WaitOne();
      });

      navigationThread.SetApartmentState(ApartmentState.STA);
      this._Threads.Add(navigationThread);
      navigationThread.Start();
    }

    private void QueueDeleteItem(FileSystemEventArgs args) {
      this._TemporaryFiles.Remove(args.FullPath);
      var existingItem = this.Items.ToArray().FirstOrDefault(s => s.ParsingName.Equals(args.FullPath));
      if (existingItem != null && (existingItem.IsFolder || this._TemporaryFiles.Count(c => c.Contains(Path.GetFileName(existingItem.ParsingName))) == 0)) {
          this._ItemsQueue.Enqueue(Tuple.Create(ItemUpdateType.Deleted, existingItem), true);
          this.UnvalidateDirectory();
      }
    }

    private void NavigateSearch(IListItemEx destination, Boolean isInSameTab = false, Boolean refresh = false, Boolean isCancel = false) {
      SaveSettingsToDatabase(this.CurrentFolder);
      if (destination == null) return;
      if (this.RequestedCurrentLocation == destination && !refresh) return;
      //if (this._RequestedCurrentLocation != destination) {
      //}

      _ResetEvent.Set();

      if (this._Threads.Any()) {
        _Mre.Set();
        this._ResetEvent.Set();
        foreach (var thread in this._Threads.ToArray()) {
          thread.Abort();
          this._Threads.Remove(thread);
        }
      }

      this._UnvalidateTimer.Stop();
      this._IsDisplayEmptyText = false;
      User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, 0, 0);
      this.DisableGroups();

      this._ItemForRename = -1;
      this._LastItemForRename = -1;

      Items.Clear();
      this._AddedItems.Clear();
      this.LargeImageList.ReInitQueues();
      this.SmallImageList.ReInitQueues();
      this._CuttedIndexes.Clear();
      this.RequestedCurrentLocation = destination;
      if (!refresh)
        Navigating?.Invoke(this, new NavigatingEventArgs(destination, isInSameTab));

      var columns = new Collumns();
      Int32 CurrentI = 0, LastI = 0, K = 0;
      this.IsNavigationInProgress = true;

      _ResetTimer.Stop();

      this.RemoveAllCollumns();
      this.AddDefaultColumns(false, true);

      this.IsViewSelectionAllowed = true;
      this.Invoke((Action)(() => this._NavWaitTimer.Start()));
      var navigationThread = new Thread(() => {
        destination = FileSystemListItem.ToFileSystemItem(destination.ParentHandle, destination.PIDL);
        this.RequestedCurrentLocation = destination;
        this.Invoke((Action)(() => {
          if (!this._SearchTimer.Enabled) {
            this._SearchTimer.Start();
          }
        }));

        foreach (var shellItem in destination.TakeWhile(shellItem => !this.IsCancelRequested)) {
          CurrentI++;
          _Smre.WaitOne();

          if (this.ShowHidden || !shellItem.IsHidden) {
            shellItem.ItemIndex = K++;
            this.Items.Add(shellItem);
            if (CurrentI == 1) {
              this.Invoke((Action)(() => {
                this._NavWaitTimer.Stop();
                this._IsDisplayEmptyText = false;
                this._IIListView.ResetEmptyText();
              }));
            }
          }

          var delta = CurrentI - LastI;
          if (delta >= (this.IsSearchNavigating ? 1 : 5000)) {
            LastI = CurrentI;
          }
          if (this.IsSearchNavigating && delta >= 20)
            Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        }
        this.IsCancelRequested = false;
        this.IsNavigationInProgress = false;

        if (this.RequestedCurrentLocation.NavigationStatus != HResult.S_OK) {
          this.Invoke((Action)(() => {
            if (this._SearchTimer.Enabled)
              this._SearchTimer.Stop();
          }));
          this.BeginInvoke((Action)(() => {
            var navArgs = new NavigatedEventArgs(this.RequestedCurrentLocation, this.CurrentFolder, isInSameTab);
            this.CurrentFolder = this.RequestedCurrentLocation;

            if (!refresh)
              Navigated?.Invoke(this, navArgs);
          }));

          GC.Collect();
          Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
          if (this._Threads.Count <= 1) return;
          _Mre.Set();
          this._ResetEvent.Set();
          this._Threads[0].Abort();
          this._Threads.RemoveAt(0);
          return;
        }

        var headerhandle = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
        for (var i = 0; i < this.Collumns.Count; i++) {
          this.Collumns[i].SetSplitButton(headerhandle, i);
        }

        if (this.View != ShellViewStyle.Details) AutosizeAllColumns(-2);

        var sortColIndex = 0;
        if (sortColIndex > -1) this.SetSortIcon(sortColIndex, SortOrder.Ascending);

        this.SetSortCollumn(false, this.Collumns.First(), SortOrder.Ascending, false);

        this.BeginInvoke((Action)(() => {
          var navArgs = new NavigatedEventArgs(this.RequestedCurrentLocation, this.CurrentFolder, isInSameTab);
          this.CurrentFolder = this.RequestedCurrentLocation;
          if (!refresh)
            Navigated?.Invoke(this, navArgs);
        }));

        GC.Collect();
        Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
        this.Invoke((Action)(() => {
          if (this._SearchTimer.Enabled)
            this._SearchTimer.Stop();
        }));
        _Mre.Reset();
        _Mre.WaitOne();
      });
      navigationThread.SetApartmentState(ApartmentState.STA);
      this._Threads.Add(navigationThread);
      navigationThread.Start();
    }

    /*
[Obsolete("Not Used")]
private void NavigateNet(IListItemEx destination, Boolean isInSameTab = false, Boolean refresh = false, Boolean isCancel = false)
{
SaveSettingsToDatabase(this.CurrentFolder);
//TODO: Document isCancel Param better
if (destination == null) return;
if (this._RequestedCurrentLocation == destination && !refresh) return;

resetEvent.Set();

if (this.Threads.Count > 0)
{
mre.Set();
this.resetEvent.Set();
foreach (var thread in this.Threads.ToArray())
{
	thread.Abort();
	this.Threads.Remove(thread);
}
}

this._UnvalidateTimer.Stop();
this.IsDisplayEmptyText = false;
User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, 0, 0);
this.DisableGroups();

this._ItemForRename = -1;
this._LastItemForRename = -1;

Items.Clear();
this._AddedItems.Clear();
ItemsForSubitemsUpdate.Clear();
waitingThumbnails.Clear();
overlayQueue.Clear();
shieldQueue.Clear();
this._CuttedIndexes.Clear();
this.SubItemValues.Clear();
this._RequestedCurrentLocation = destination;
if (!refresh)
Navigating?.Invoke(this, new NavigatingEventArgs(destination, isInSameTab));

var columns = new Collumns();
//var isFailed = true;
Int32 CurrentI = 0, LastI = 0, K = 0;
this.IsNavigationInProgress = true;
_ResetTimer.Stop();

this.RemoveAllCollumns();
this.AddDefaultColumns(false, true);
this.IsViewSelectionAllowed = true;

var navigationThread = new Thread(() =>
{
destination = FileSystemListItem.ToFileSystemItem(destination.ParentHandle, destination.PIDL);
this._RequestedCurrentLocation = destination;
this.Invoke((Action)(() =>
{
	if (!this._SearchTimer.Enabled)
	{
		this._SearchTimer.Start();
	}
}));
foreach (var shellItem in destination.TakeWhile(shellItem => !this.IsCancelRequested))
{
	CurrentI++;
	//if (CurrentI == 1) {
	//  isFailed = false;
	//}
	smre.WaitOne();

	if (this.ShowHidden || !shellItem.IsHidden)
	{
		shellItem.ItemIndex = K++;
		this.Items.Add(shellItem);
	}

	var delta = CurrentI - LastI;
	if (delta >= (this.IsSearchNavigating ? 1 : 5000))
		LastI = CurrentI;
	if (this.IsSearchNavigating && delta >= 20)
		Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
}

this.IsCancelRequested = false;
this.IsNavigationInProgress = false;

if (this._RequestedCurrentLocation.NavigationStatus != HResult.S_OK)
{
	this.Invoke((Action)(() =>
	{
		if (this._SearchTimer.Enabled)
			this._SearchTimer.Stop();
	}));
	GC.Collect();
	Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
	if (this.Threads.Count <= 1) return;
	mre.Set();
	this.resetEvent.Set();
	this.Threads[0].Abort();
	this.Threads.RemoveAt(0);
	return;
}

var headerhandle = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
for (var i = 0; i < this.Collumns.Count; i++)
{
	this.Collumns[i].SetSplitButton(headerhandle, i);
}

if (this.View != ShellViewStyle.Details) AutosizeAllColumns(-2);

var sortColIndex = 0;
if (sortColIndex > -1) this.SetSortIcon(sortColIndex, SortOrder.Ascending);

this.SetSortCollumn(false, this.Collumns.First(), SortOrder.Ascending, false);

this.BeginInvoke((Action)(() =>
{
	var navArgs = new NavigatedEventArgs(this._RequestedCurrentLocation, this.CurrentFolder, isInSameTab);
	this.CurrentFolder = this._RequestedCurrentLocation;
	if (!refresh)
		Navigated?.Invoke(this, navArgs);
}));

GC.Collect();
Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
this.Invoke((Action)(() =>
{
	if (this._SearchTimer.Enabled)
		this._SearchTimer.Stop();
}));
mre.Reset();
mre.WaitOne();
this.Focus();
});

navigationThread.SetApartmentState(ApartmentState.STA);
this.Threads.Add(navigationThread);
navigationThread.Start();
}
*/

    private Boolean LoadSettingsFromDatabase(IListItemEx directory, out FolderSettings folderSettings) {
      var result = false;
      var folderSetting = new FolderSettings();
      if (directory.IsSearchFolder) {
        folderSettings = folderSetting;
        return false;
      }

      try {
        var mDBConnection = new SQLite.SQLiteConnection("Data Source=" + this._DBPath + ";Version=3;");
        mDBConnection.Open();

        var command1 = new SQLite.SQLiteCommand("SELECT * FROM foldersettings WHERE Path=@0", mDBConnection);
        command1.Parameters.AddWithValue("0", directory.ParsingName);

        var reader = command1.ExecuteReader();
        if (reader.Read()) {
          var values = reader.GetValues();
          if (values.Count > 0) {
            result = true;
            var view = values.GetValues("View").FirstOrDefault();
            var iconSize = values.GetValues("IconSize").FirstOrDefault();
            var lastSortedColumnIndex = values.GetValues("LastSortedColumn").FirstOrDefault();
            var lastSortOrder = values.GetValues("LastSortOrder").FirstOrDefault();
            var lastGroupedColumnId = values.GetValues("LastGroupCollumn").FirstOrDefault();
            var lastGroupoupOrder = values.GetValues("LastGroupOrder").FirstOrDefault();

            if (view != null)
              folderSetting.View = (ShellViewStyle)Enum.Parse(typeof(ShellViewStyle), view);

            if (lastSortedColumnIndex != null) {
              folderSetting.SortColumn = lastSortedColumnIndex;
              folderSetting.SortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), lastSortOrder);
            }

            folderSetting.GroupCollumn = lastGroupedColumnId;
            folderSetting.GroupOrder = lastGroupoupOrder == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending;

            var collumns = values.GetValues("Columns").FirstOrDefault();
            folderSetting.Columns = collumns != null ? XElement.Parse(collumns) : null;

            if (String.IsNullOrEmpty(iconSize))
              folderSetting.IconSize = 48;
            else
              folderSetting.IconSize = Int32.Parse(iconSize);
          }
        }

        reader.Close();
      } catch (Exception) {
      }

      folderSettings = folderSetting;
      return result;
    }

    private void RenameItem(Int32 index) {
      this.Focus(false, true);
      this.IsFocusAllowed = false;
      this._IsCanceledOperation = false;
      this._ItemForRename = index;
      this.IsRenameInProgress = true;
      var ptr = IntPtr.Zero;
      this.BeginInvoke(new MethodInvoker(() => this._IIListView.EditLabel(this.ToLvItemIndex(index), IntPtr.Zero, out ptr)));
    }

    private void Do_Copy_OR_Move_Helper(Boolean copy, IListItemEx destination, IShellItem[] items) {
      var handle = this.Handle;
      var thread = new Thread(() => {
        var fo = new IIFileOperation(handle);
        foreach (var item in items) {
          if (copy)
            fo.CopyItem(item, destination);
          else
            fo.MoveItem(item, destination, null);
        }
        fo.PerformOperations();
      });

      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    private void Do_Copy_OR_Move_Helper_2(Boolean copy, IListItemEx destination, F.IDataObject dataObject) {
      IntPtr handle = this.Handle;
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
            if (copy)
              fo.CopyItem(item, destination);
            else
              fo.MoveItem(item, destination, null);
          }

          fo.PerformOperations();
        } catch (SecurityException) {
          throw;
        }
      });

      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    private void UpdateColsInView(Boolean isDetails = false) {
      IntPtr headerhandle = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
      foreach (var col in this.Collumns.ToArray()) {
        var colIndex = this.Collumns.IndexOf(col);
        var colNative = col.ToNativeColumn(isDetails);
        User32.SendMessage(this.LVHandle, MSG.LVM_SETCOLUMN, colIndex, ref colNative);
        col.SetSplitButton(headerhandle, colIndex);
        if (col.ID == this.LastSortedColumnId) {
          this.SetSortIcon(colIndex, this.LastSortOrder);
        }
      }
    }

    private LVITEMINDEX ToLvItemIndex(Int32 index) => new LVITEMINDEX() { iItem = index, iGroup = this.GetGroupIndex(index) };

    public void RedrawItem(Int32 index, Int32 delay = -1) {
      if (delay > -1) {
        Thread.Sleep(delay);
      }
      var itemBounds = new User32.RECT() { Left = 1 };
      var lvi = new LVITEMINDEX() { iItem = index, iGroup = this.GetGroupIndex(index) };
      User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
      itemBounds.Left -= 2;
      itemBounds.Top -= 2;
      itemBounds.Bottom += 2;
      itemBounds.Right += 2;
      this.Invoke(new MethodInvoker(() => this._IIListView.RedrawItems(index, index)));

      //TODO: Find out why we have this loop
      for (Int32 i = 0; i < 1; i++) {
        if (IsGroupsEnabled) RedrawWindow(itemBounds);
      }
    }

    private void ProcessShellNotifications(ref Message m) {
      if (Notifications.NotificationReceipt(m.WParam, m.LParam)) {
        foreach (NotifyInfos info in Notifications.NotificationsReceived.ToArray()) {
          try {
            switch (info.Notification) {
              case ShellNotifications.SHCNE.SHCNE_MKDIR:
              case ShellNotifications.SHCNE.SHCNE_CREATE:
                try {
                  var obj = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1);
                  if (obj.IsInCurrentFolder(this.CurrentFolder)) {
                    if (this.IsRenameNeeded) {
                      var existingItem = this.Items.FirstOrDefault(s => s.Equals(obj));
                      if (existingItem == null) {
                        var itemIndex = this.InsertNewItem(obj);
                        this.SelectItemByIndex(itemIndex, true, true);
                        this.RenameSelectedItem(itemIndex);
                        this.IsRenameNeeded = false;
                      } else {
                        this.RenameSelectedItem(existingItem.ItemIndex);
                      }
                    } else if (this._ItemsQueue.Enqueue(new Tuple<ItemUpdateType, IListItemEx>(ItemUpdateType.Created, obj))) {
                      this.UnvalidateDirectory();
                    }
                  }
                } catch (FileNotFoundException) { }
                break;

              case ShellNotifications.SHCNE.SHCNE_RMDIR:
              case ShellNotifications.SHCNE.SHCNE_DELETE:
                var objDelete = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1);
                if (this._ItemsQueue.Enqueue(Tuple.Create(ItemUpdateType.RecycleBin, FileSystemListItem.InitializeWithIShellItem(IntPtr.Zero, ((ShellItem)KnownFolders.RecycleBin).ComInterface)))) {
                  this.UnvalidateDirectory();
                }
                if (objDelete.IsInCurrentFolder(this.CurrentFolder) && this._ItemsQueue.Enqueue(Tuple.Create(ItemUpdateType.Deleted, objDelete.Clone()), true)) {
                  this.UnvalidateDirectory();
                  this.RaiseItemUpdated(ItemUpdateType.Deleted, null, objDelete, -1);
                  objDelete.Dispose();
                  break;
                }
                break;

              case ShellNotifications.SHCNE.SHCNE_UPDATEDIR:
                IListItemEx objUpdate = null;
                try {
                  objUpdate = FileSystemListItem.ToFileSystemItem(this.LVHandle, Shell32.ILFindLastID(info.Item1));
                } catch { }
                if (objUpdate.IsInCurrentFolder(this.CurrentFolder)) {
                  this.UnvalidateDirectory();
                }
                break;

              case ShellNotifications.SHCNE.SHCNE_UPDATEITEM:
                var objUpdateItem = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1);
                if (objUpdateItem.IsInCurrentFolder(this.CurrentFolder)) {
                  var exisitingUItem = this.Items.ToArray().FirstOrDefault(w => w.Equals(objUpdateItem));
                  if (exisitingUItem != null) {
                    if (this.View == ShellViewStyle.Details) {
                      foreach (var collumn in this.Collumns) {
                        if (collumn.Index > 0 && this.IconSize == 16) {
						  this.SmallImageList.EnqueueSubitemsGet(new Tuple<Int32, Int32, PROPERTYKEY>(exisitingUItem.ItemIndex, collumn.Index, collumn.pkey));
                        }
                      }
                    }

                    if (this._ItemsQueue.Enqueue(new Tuple<ItemUpdateType, IListItemEx>(ItemUpdateType.Updated, exisitingUItem)))
                      this.UnvalidateDirectory();
                    //this.RefreshItem(exisitingUItem.ItemIndex, true);
                  }
                }
                break;

              case ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER:
              case ShellNotifications.SHCNE.SHCNE_RENAMEITEM:
                var obj1 = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1);
                var obj2 = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item2);
                if (!String.IsNullOrEmpty(obj1.ParsingName) && !String.IsNullOrEmpty(obj2.ParsingName))
                  this.UpdateItem(obj1, obj2);
                this.IsRenameInProgress = false;
                break;

              case ShellNotifications.SHCNE.SHCNE_NETSHARE:
              case ShellNotifications.SHCNE.SHCNE_NETUNSHARE:
              case ShellNotifications.SHCNE.SHCNE_ATTRIBUTES:
                var objNetA = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1);
                var exisitingItemNetA = this.Items.FirstOrDefault(w => w.Equals(objNetA));
                if (this.View == ShellViewStyle.Details) {
                  foreach (var collumn in this.Collumns) {
                    if (collumn.Index > 0 &&this.IconSize == 16) {
                        this.SmallImageList.EnqueueSubitemsGet(new Tuple<Int32, Int32, PROPERTYKEY>(exisitingItemNetA.ItemIndex, collumn.Index, collumn.pkey));
                    }
                  }
                }
                this._ItemsQueue.Enqueue(new Tuple<ItemUpdateType, IListItemEx>(ItemUpdateType.Updated, exisitingItemNetA));
                break;

              case ShellNotifications.SHCNE.SHCNE_MEDIAINSERTED:
              case ShellNotifications.SHCNE.SHCNE_MEDIAREMOVED:
                if (this.CurrentFolder.ParsingName == KnownFolders.Computer.ParsingName) {
                  var objMedia = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1);
                  var exisitingItem = this.Items.SingleOrDefault(w => w.Equals(objMedia));
                  if (exisitingItem != null)
                    this.UpdateItem(exisitingItem.ItemIndex);
                }
                break;

              case ShellNotifications.SHCNE.SHCNE_DRIVEREMOVED:
                var objDr = FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1);
                if (this.CurrentFolder != null && this.CurrentFolder.ParsingName.Equals(KnownFolders.Computer.ParsingName)) {
                  this.Items.Remove(objDr);
                  var i = 0;
                  this.Items.ToList().ForEach(e => e.ItemIndex = i++);
                  if (this.IsGroupsEnabled) this.SetGroupOrder(false);

                  User32.SendMessage(this.LVHandle, MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
                }
                this.RaiseItemUpdated(ItemUpdateType.DriveRemoved, null, objDr, -1);
                break;

              case ShellNotifications.SHCNE.SHCNE_DRIVEADD:
                if (this.CurrentFolder != null && this.CurrentFolder.ParsingName.Equals(KnownFolders.Computer.ParsingName))
                  this.InsertNewItem(FileSystemListItem.ToFileSystemItem(this.LVHandle, info.Item1));
                break;
              case ShellNotifications.SHCNE.SHCNE_FREESPACE:
                //if (this._ItemsQueue.Enqueue(Tuple.Create(ItemUpdateType.RecycleBin, FileSystemListItem.InitializeWithIShellItem(IntPtr.Zero, ((ShellItem)KnownFolders.RecycleBin).ComInterface)))) {
                //	this.UnvalidateDirectory();
                //}
                break;
              case ShellNotifications.SHCNE.SHCNE_UPDATEIMAGE:

                break;
            }
          } catch {
          }

          Notifications.NotificationsReceived.Remove(info);
        }
      }
    }

    internal static void Drag_SetEffect(F.DragEventArgs e) {
      if ((e.KeyState & (8 + 32)) == (8 + 32) && (e.AllowedEffect & F.DragDropEffects.Link) == F.DragDropEffects.Link)
        e.Effect = F.DragDropEffects.Link;  // Link drag-and-drop effect.// KeyState 8 + 32 = CTL + ALT
      else if ((e.KeyState & 32) == 32 && (e.AllowedEffect & F.DragDropEffects.Link) == F.DragDropEffects.Link)
        e.Effect = F.DragDropEffects.Link;  // ALT KeyState for link.
      else if ((e.KeyState & 4) == 4 && (e.AllowedEffect & F.DragDropEffects.Move) == F.DragDropEffects.Move)
        e.Effect = F.DragDropEffects.Move;  // SHIFT KeyState for move
      else if ((e.KeyState & 8) == 8 && (e.AllowedEffect & F.DragDropEffects.Copy) == F.DragDropEffects.Copy)
        e.Effect = F.DragDropEffects.Copy;  // CTL KeyState for copy.
      else if ((e.AllowedEffect & F.DragDropEffects.Move) == F.DragDropEffects.Move)
        e.Effect = F.DragDropEffects.Move;  // By default, the drop action should be move, if allowed.
      else
        e.Effect = F.DragDropEffects.Copy;
    }

    private IListItemEx GetBadgeForPath(String path) {
      var allBadges = this.BadgesData.SelectMany(s => s.Value).ToArray();
      var foundBadge = allBadges.Where(w => w.Value.Count(c => c.ToLowerInvariant().Equals(path.ToLowerInvariant())) > 0).FirstOrDefault();
      return foundBadge.Equals(default(KeyValuePair<IListItemEx, List<String>>)) ? null : foundBadge.Key;
    }

    internal void OnSelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);

    //private new void ResumeLayout() => User32.SendMessage(this.LVHandle, (int)WM.WM_SETREDRAW, 1, 0);
    //private new void SuspendLayout() => User32.SendMessage(this.LVHandle, (int)WM.WM_SETREDRAW, 0, 0);
    [Obsolete("Contains No Code")]
    private void RedrawWindow() { }//User32.InvalidateRect(this.LVHandle, IntPtr.Zero, false);

    [Obsolete("Contains No Code")]
    private void RedrawWindow(User32.RECT rect) { }// => User32.InvalidateRect(this.LVHandle, ref rect, false);

    /// <summary>
    /// Returns the index of the first item whose display name starts with the search string.
    /// </summary>
    /// <param name="search">     The string for which to search for. </param>
    /// <param name="startindex">
    /// The index from which to start searching. Enter '0' to search all items.
    /// </param>
    /// <returns> The index of an item within the list view. </returns>
    private Int32 GetFirstIndexOf(String search, Int32 startindex) {
      Int32 i = startindex;
      while (true) {
        if (i >= Items.Count)
          return -1;
        else if (Items[i].DisplayName.ToUpperInvariant().StartsWith(search.ToUpperInvariant()))
          return i;
        else
          i++;
      }
    }

    private void StartProcessInCurrentDirectory(IListItemEx item) {
      Process.Start(new ProcessStartInfo() {
        FileName = item.ParsingName,
        WorkingDirectory = this.CurrentFolder.ParsingName
      });
    }

    internal void OnItemMiddleClick() {
      if (ItemMiddleClick != null) {
        var row = -1;
        var column = -1;
        this.HitTest(this.PointToClient(Cursor.Position), out row, out column);
        if (row != -1 && this.Items[row].IsFolder)
          ItemMiddleClick.Invoke(this, new NavigatedEventArgs(this.Items[row], this.Items[row]));
      }
    }

    private String GetFilePropertiesString(Object value) {
      var valueFA = (FileAttributes)value;
      var isArhive = ((valueFA & FileAttributes.Archive) == FileAttributes.Archive);
      var isDirectory = ((valueFA & FileAttributes.Directory) == FileAttributes.Directory);
      var isHidden = ((valueFA & FileAttributes.Hidden) == FileAttributes.Hidden);
      var isReadOnly = ((valueFA & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
      var isSystem = ((valueFA & FileAttributes.System) == FileAttributes.System);
      var isTemp = ((valueFA & FileAttributes.Temporary) == FileAttributes.Temporary);
      var resultString = String.Empty;

      resultString += isArhive ? "A" : "-";
      resultString += isDirectory ? "D" : "-";
      resultString += isHidden ? "H" : "-";
      resultString += isReadOnly ? "R" : "-";
      resultString += isSystem ? "S" : "-";
      resultString += isTemp ? "T" : "-";

      return resultString;
    }

    /// <summary>
    /// This is only to be used in SetSortCollumn(...)
    /// </summary>
    /// <param name="columnIndex"></param>
    /// <param name="order"></param>
    private void SetSortIcon(Int32 columnIndex, SortOrder order) {
      IntPtr columnHeader = User32.SendMessage(this.LVHandle, MSG.LVM_GETHEADER, 0, 0);
      for (Int32 columnNumber = 0; columnNumber <= this.Collumns.Count - 1; columnNumber++) {
        var item = new HDITEM { mask = HDITEM.Mask.Format };

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


    private void AutosizeColumn(Int32 index, Int32 autosizeStyle) => User32.SendMessage(this.LVHandle, LVM.SETCOLUMNWIDTH, index, autosizeStyle);

    private Int32 _CurrentDrawIndex = -1;
    [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
    private void ProcessCustomDrawPostPaint(ref Message m, User32.NMLVCUSTOMDRAW nmlvcd, Int32 index, IntPtr hdc, IListItemEx sho, Color? textColor) {
      if (nmlvcd.clrTextBk != 0 && nmlvcd.dwItemType == 0 && this._CurrentDrawIndex == -1) {
        //var perceivedType = (PerceivedType)sho.GetPropertyValue(SystemProperties.PerceivedType, typeof(PerceivedType)).Value;
        this._CurrentDrawIndex = index;
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
          var cutFlag = (User32.SendMessage(this.LVHandle, MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_CUT) & LVIS.LVIS_CUT) ==
                                                                  LVIS.LVIS_CUT;
          if (this.IconSize == 16) {
            this.SmallImageList.DrawIcon(hdc, index, sho, iconBounds,
                    sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index), (nmlvcd.nmcd.uItemState & CDIS.HOT) == CDIS.HOT);
          } else {
            this.LargeImageList.DrawIcon(hdc, index, sho, iconBounds,
                    sho.IsHidden || cutFlag || this._CuttedIndexes.Contains(index), (nmlvcd.nmcd.uItemState & CDIS.HOT) == CDIS.HOT);
          }

          if (!sho.IsInitialised) sho.IsInitialised = true;
        }
        m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
      } else {
        m.Result = IntPtr.Zero;
      }

      this._CurrentDrawIndex = -1;
    }

    private void DrawNormalFolderSubitemsInTiledView(IListItemEx sho, RectangleF lblrectTiles, Graphics g, StringFormat fmt) {
      var lblrectSubiTem2 = new RectangleF(lblrectTiles.Left, lblrectTiles.Bottom + 1, lblrectTiles.Width, 15);
      var lblrectSubiTem3 = new RectangleF(lblrectTiles.Left, lblrectTiles.Bottom + 17, lblrectTiles.Width, 15);
      Font subItemFont = System.Drawing.SystemFonts.IconTitleFont;
      var subItemTextBrush = new SolidBrush(System.Drawing.SystemColors.ControlDarkDark);//new SolidBrush(Color.FromArgb(93, 92, 92));
      g.DrawString(sho.GetPropertyValue(SystemProperties.FileType, typeof(String)).Value.ToString(),
                                                                                                                                                                      subItemFont, subItemTextBrush, lblrectSubiTem2, fmt);
      if (sho.Parent.IsFileSystem) {
        var size = sho.GetPropertyValue(SystemProperties.FileSize, typeof(Int64)).Value;
        if (size != null) {
          g.DrawString(ShlWapi.StrFormatByteSize(Int64.Parse(size.ToString())), subItemFont, subItemTextBrush, lblrectSubiTem3, fmt);
        }
      }

      subItemFont.Dispose();
      subItemTextBrush.Dispose();
    }

    private void DrawComputerTiledModeView(IListItemEx sho, Graphics g, RectangleF lblrectTiles, StringFormat fmt) {
      var driveInfo = new DriveInfo(sho.ParsingName);
      if (driveInfo.IsReady) {
        ProgressBarRenderer.DrawHorizontalBar(g, new Rectangle((Int32)lblrectTiles.Left, (Int32)lblrectTiles.Bottom + 4, (Int32)lblrectTiles.Width - 10, 10));
        var fullProcent = (100 * (driveInfo.TotalSize - driveInfo.AvailableFreeSpace)) / driveInfo.TotalSize;
        var barWidth = (lblrectTiles.Width - 12) * fullProcent / 100;
        var rec = new Rectangle((Int32)lblrectTiles.Left + 1, (Int32)lblrectTiles.Bottom + 5, (Int32)barWidth, 8);
        var gradRec = new Rectangle(rec.Left, rec.Top - 1, rec.Width, rec.Height + 2);
        var criticalUsed = fullProcent >= 90;
        var warningUsed = fullProcent >= 75;
        var averageUsed = fullProcent >= 50;
        var brush = new LinearGradientBrush(gradRec,
                                                                        criticalUsed ? Color.FromArgb(255, 0, 0) : warningUsed ? Color.FromArgb(255, 224, 0) : averageUsed ? Color.FromArgb(0, 220, 255) : Color.FromArgb(199, 248, 165),
                                                                        criticalUsed ? Color.FromArgb(150, 0, 0) : warningUsed ? Color.FromArgb(255, 188, 0) : averageUsed ? Color.FromArgb(43, 84, 235) : Color.FromArgb(101, 247, 0),
                                                                        LinearGradientMode.Vertical);
        g.FillRectangle(brush, rec);
        brush.Dispose();
        var lblrectSubiTem3 = new RectangleF(lblrectTiles.Left, lblrectTiles.Bottom + 16, lblrectTiles.Width, 15);
        Font subItemFont = System.Drawing.SystemFonts.IconTitleFont;
        var subItemTextBrush = new SolidBrush(System.Drawing.SystemColors.ControlDarkDark);
        g.DrawString($"{ShlWapi.StrFormatByteSize(driveInfo.AvailableFreeSpace)} free of {ShlWapi.StrFormatByteSize(driveInfo.TotalSize)}",
                                                                        subItemFont, subItemTextBrush, lblrectSubiTem3, fmt);

        subItemFont.Dispose();
        subItemTextBrush.Dispose();
      }
    }

    [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
    private void ProcessCustomDraw(ref Message m, ref NMHDR nmhdr) {
      if (nmhdr.hwndFrom == this.LVHandle) {
        #region Starting

        User32.SendMessage(this.LVHandle, 296, User32.MAKELONG(1, 1), 0);
        var nmlvcd = (User32.NMLVCUSTOMDRAW)m.GetLParam(typeof(User32.NMLVCUSTOMDRAW));
        var index = (Int32)nmlvcd.nmcd.dwItemSpec;
        var hdc = nmlvcd.nmcd.hdc;

        var sho = Items.Count > index ? Items[index] : null;

        Color? textColor = null;
        if (sho != null && this.LVItemsColorCodes != null && this.LVItemsColorCodes.Count > 0 && !String.IsNullOrEmpty(sho.Extension)) {
          var extItemsAvailable = this.LVItemsColorCodes.Where(c => c.ExtensionList.Contains(sho.Extension)).Count() > 0;
          if (extItemsAvailable) {
            var color = this.LVItemsColorCodes.SingleOrDefault(c => c.ExtensionList.ToLowerInvariant().Contains(sho.Extension)).TextColor;
            textColor = Color.FromArgb(color.A, color.R, color.G, color.B);
          }
        }

        #endregion Starting

        switch (nmlvcd.nmcd.dwDrawStage) {
          case CustomDraw.CDDS_PREPAINT:

            #region Case

            m.Result = (IntPtr)(CustomDraw.CDRF_NOTIFYITEMDRAW | CustomDraw.CDRF_NOTIFYPOSTPAINT | 0x40);
            break;

          #endregion Case

          case CustomDraw.CDDS_POSTPAINT:

            #region Case

            m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
            break;

          #endregion Case

          case CustomDraw.CDDS_ITEMPREPAINT:

            #region Case

            if (nmlvcd.clrTextBk != 0) {
              if ((nmlvcd.nmcd.uItemState & CDIS.DROPHILITED) == CDIS.DROPHILITED && index != _LastDropHighLightedItemIndex) {
                nmlvcd.nmcd.uItemState = CDIS.DEFAULT;
                Marshal.StructureToPtr(nmlvcd, m.LParam, false);
              }

              if (index == _LastDropHighLightedItemIndex) {
                nmlvcd.nmcd.uItemState |= CDIS.DROPHILITED;
                Marshal.StructureToPtr(nmlvcd, m.LParam, false);
              }

              if (textColor == null) {
                m.Result = (IntPtr)(CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW | 0x40);
              } else {
                nmlvcd.clrText = (UInt32)ColorTranslator.ToWin32(textColor.Value);
                Marshal.StructureToPtr(nmlvcd, m.LParam, false);

                m.Result =
                        (IntPtr)(CustomDraw.CDRF_NEWFONT | CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW);
              }
            } else {
              m.Result =
                      (IntPtr)(CustomDraw.CDRF_SKIPDEFAULT);
            }


            break;

          #endregion Case

          case CustomDraw.CDDS_ITEMPREPAINT | CustomDraw.CDDS_SUBITEM:

            #region Case

            if (textColor == null) {
              m.Result = (IntPtr)CustomDraw.CDRF_DODEFAULT;
            } else {
              nmlvcd.clrText = (UInt32)ColorTranslator.ToWin32(textColor.Value);
              Marshal.StructureToPtr(nmlvcd, m.LParam, false);
              m.Result = (IntPtr)CustomDraw.CDRF_NEWFONT;
            }
            break;

          #endregion Case

          case CustomDraw.CDDS_ITEMPOSTPAINT:
            this.ProcessCustomDrawPostPaint(ref m, nmlvcd, index, hdc, sho, textColor);
            break;
        }
      }
    }

    #endregion Private Methods
  }
}