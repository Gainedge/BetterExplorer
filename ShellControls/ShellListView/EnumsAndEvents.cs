using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using WPFUI.Win32;

namespace ShellControls.ShellListView;
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

  public class ScrollEventArgs : EventArgs {
    public SCROLLINFO ScrollInfo { get; private set; }
    public Boolean IsPositionChangedOnly { get; set; }

    public ScrollEventArgs(SCROLLINFO scrollInfo) {
      this.ScrollInfo = scrollInfo;
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

    public Point ActionPoint { get; set; }

    public ListViewColumnDropDownArgs(Int32 colIndex, Point pt) {
      this.ColumnIndex = colIndex;
      this.ActionPoint = pt;
    }
  }

  public class TreeViewMiddleClickEventArgs : EventArgs {
    public IListItemEx? Item { get; set; }
  }
  public class NavigatedEventArgs : EventArgs, IDisposable {
    /// <summary> The folder that is navigated to. </summary>
    public IListItemEx? Folder { get; set; }

    public IListItemEx? OldFolder { get; set; }

    public Boolean IsInSameTab { get; set; }

    /// <inheritdoc/>
    public void Dispose() {
      this.Folder?.Dispose();
      this.Folder = null;
      this.OldFolder?.Dispose();
      this.OldFolder = null;
    }

    public NavigatedEventArgs(IListItemEx folder, IListItemEx old) {
      this.Folder = folder;
      this.OldFolder = old;
    }

    public NavigatedEventArgs(IListItemEx folder, IListItemEx old, Boolean isInSame) {
      this.Folder = folder;
      this.OldFolder = old;
      this.IsInSameTab = isInSame;
    }
  }

  /// <summary> Provides information for the <see cref="ShellView.Navigating" /> event. </summary>
  public class NavigatingEventArgs : EventArgs, IDisposable {
    /// <summary> The folder being navigated to. </summary>
    public IListItemEx? Folder { get; private set; }

    public Boolean IsNavigateInSameTab { get; private set; }

    public Boolean IsFirstItemAvailable { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigatingEventArgs"/> class.
    /// </summary>
    /// <param name="folder">The folder being navigated to.</param>
    /// <param name="isInSameTab"></param>
    public NavigatingEventArgs(IListItemEx folder, Boolean isInSameTab) {
      this.Folder = folder;
      this.IsNavigateInSameTab = isInSameTab;
    }

    /// <inheritdoc/>
    public void Dispose() {
      this.Folder = null;
    }
  }

  public class TabUpdateEventArgs : EventArgs, IDisposable {
    /// <summary> The folder being navigated to. </summary>
    public IListItemEx? Folder { get; private set; }

    public Boolean IsBusy { get; private set; }


    public TabUpdateEventArgs(IListItemEx folder, Boolean isBusy) {
      this.Folder = folder;
      this.IsBusy = isBusy;
    }

    /// <inheritdoc/>
    public void Dispose() {
      this.Folder = null;
    }
  }

  public class ColumnAddEventArgs : EventArgs {
    public Collumns? Collumn { get; set; }
    public List<Collumns>? Collumns { get; set; }

    public ColumnAddEventArgs(Collumns? col) {
      this.Collumn = col;
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

    public IListItemEx? PreviousItem { get; private set; }

    public IListItemEx? NewItem { get; private set; }

    public Int32 NewItemIndex { get; private set; }

    public ItemUpdatedEventArgs(ItemUpdateType type, IListItemEx? newItem, IListItemEx? previousItem, Int32 index) {
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
      this.CurrentView = view;
      if (thumbnailSize != null) {
        this.ThumbnailSize = thumbnailSize.Value;
      }
    }
  }

