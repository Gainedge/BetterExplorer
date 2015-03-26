using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell._Plugin_Interfaces {
  public class FileSystemListItem : IListItemEx {
    private ShellItem _Item { get; set; }
    #region IListItemEx<FileSystemListItem> Members

    public IShellItem ComInterface {
      get {
        //var item = ShellItem.ToShellParsingName(this.ParsingName);
        return this._Item.ComInterface;
      }
    }

    public string DisplayName { get; set; }

    public bool IsNeedRefreshing { get; set; }

    public bool IsInvalid { get; set; }

    public bool IsOnlyLowQuality { get; set; }

    public bool IsThumbnailLoaded { get; set; }

    public bool IsInitialised { get; set; }

    public int OverlayIconIndex { get; set; }

    public Interop.IExtractIconPWFlags IconType { get; set; }

    public IntPtr ILPidl {
      get {
        return this._Item.ILPidl;
      }
    }

    public IntPtr PIDL {
      get {
        return this._Item.Pidl;
      }
    }

    public IntPtr AbsolutePidl {
      get {
        return this._Item.AbsolutePidl;
      }
    }

    public int ShieldedIconIndex { get; set; }

    public bool IsIconLoaded { get; set; }

    public string ParsingName { get; set; }

    public string Extension { get; set; }

    public string FileSystemPath { get; set; }

    public bool IsBrowsable { get; set; }

    public bool IsFolder { get; set; }

    public bool HasSubFolders {
      get {
        return this._Item.HasSubFolders;
      }
    }

    public bool IsHidden { get; set; }

    public bool IsFileSystem { get; set; }

    public bool IsNetworkPath { get; set; }

    public bool IsDrive { get; set; }

    public bool IsShared { get; set; }

    public void Initialize(IntPtr lvHandle, IntPtr pidl, int index) {
      var item = new ShellItem(pidl);
      this.DisplayName = item.DisplayName;
      this.ParsingName = item.ParsingName;
      this.ItemIndex = index;
      this.ParentHandle = lvHandle;
      this.IsFileSystem = item.IsFileSystem;
      this.IsNetworkPath = item.IsNetworkPath;
      this.Extension = item.Extension;
      this.IsDrive = item.IsDrive;
      this.IsHidden = item.IsHidden;
      this.OverlayIconIndex = -1;
      this.ShieldedIconIndex = -1;
      //this.HasSubFolders = item.HasSubFolders;
      this.IsShared = item.IsShared;
      this.IconType = item.IconType;
      this.IsFolder = item.IsFolder;
      this.IsSearchFolder = item.IsSearchFolder;
      this._Item = item;
    }

    public void InitializeWithParent(ShellItem parent, IntPtr lvHandle, IntPtr pidl, int index) {
      var item = new ShellItem(parent, pidl);
      this.DisplayName = item.DisplayName;
      this.ParsingName = item.ParsingName;
      this.ItemIndex = index;
      this.ParentHandle = lvHandle;
      this.IsFileSystem = item.IsFileSystem;
      this.IsNetworkPath = item.IsNetworkPath;
      this.Extension = item.Extension;
      this.IsDrive = item.IsDrive;
      this.IsHidden = item.IsHidden;
      this.OverlayIconIndex = -1;
      this.ShieldedIconIndex = -1;
      //this.HasSubFolders = item.HasSubFolders;
      this.IsShared = item.IsShared;
      this.IconType = item.IconType;
      this.IsFolder = item.IsFolder;
      this.IsSearchFolder = item.IsSearchFolder;
      this._Item = item;
      //item.Dispose();
    }
    public ShellSearchFolder searchFolder { get; set; }

    public void InitializeWithShellItem(ShellSearchFolder item, IntPtr lvHandle, int index) {
      this.DisplayName = item.DisplayName;
      this.ParsingName = item.ParsingName;
      this.ItemIndex = index;
      this.ParentHandle = lvHandle;
      this.IsFileSystem = item.IsFileSystem;
      this.IsNetworkPath = item.IsNetworkPath;
      this.Extension = item.Extension;
      this.IsDrive = item.IsDrive;
      this.IsHidden = item.IsHidden;
      this.OverlayIconIndex = -1;
      this.ShieldedIconIndex = -1;
      //this.HasSubFolders = item.HasSubFolders;
      this.IsShared = item.IsShared;
      this.IconType = item.IconType;
      this.IsFolder = item.IsFolder;
      this.IsSearchFolder = item.IsSearchFolder;
      this._Item = item;
      this.searchFolder = item;
      //item.Dispose();
    }

    public Dictionary<Interop.PROPERTYKEY, object> ColumnValues { get; set; }


    public int ItemIndex { get; set; }

    public IntPtr ParentHandle { get; set; }

    public void Initialize(IntPtr lvHandle, string path, int index) {
      var item = new ShellItem(path);
      this.DisplayName = item.DisplayName;
      this.ParsingName = item.ParsingName;
      this.ItemIndex = index;
      this.ParentHandle = lvHandle;
      this.IsFileSystem = item.IsFileSystem;
      this.IsNetworkPath = item.IsNetworkPath;
      this.Extension = item.Extension;
      this.IsDrive = item.IsDrive;
      this.IsHidden = item.IsHidden;
      this.OverlayIconIndex = -1;
      this.ShieldedIconIndex = -1;
      //this.HasSubFolders = item.HasSubFolders;
      this.IsShared = item.IsShared;
      this.IconType = item.IconType;
      this.IsFolder = item.IsFolder;
      this.IsSearchFolder = item.IsSearchFolder;
      this._Item = item;
      //item.Dispose();
    }

    public void Initialize(IntPtr lvHandle, string path) {
      throw new NotImplementedException();
    }

    IListItemEx[] IListItemEx.GetSubItems(bool isEnumHidden) {
      throw new NotImplementedException();
    }


    public IEnumerator<IListItemEx> GetEnumerator() {
      IShellFolder folder = this.GetIShellFolder();
      IEnumIDList enumId = ShellItem.GetIEnumIDList(folder, SHCONTF.FOLDERS | SHCONTF.INCLUDEHIDDEN | SHCONTF.INCLUDESUPERHIDDEN | SHCONTF.INIT_ON_FIRST_NEXT | SHCONTF.STORAGE | SHCONTF.CHECKING_FOR_CHILDREN | SHCONTF.INIT_ON_FIRST_NEXT |
          SHCONTF.NONFOLDERS | SHCONTF.FASTITEMS | SHCONTF.ENABLE_ASYNC);
      uint count;
      IntPtr pidl;

      if (enumId == null) {
        yield break;
      }

      HResult result = enumId.Next(1, out pidl, out count);
      var i = 0;
      while (result == HResult.S_OK) {
        var fsi = new FileSystemListItem();
        fsi.InitializeWithParent(this._Item, this.ParentHandle, pidl, i++);
        yield return fsi;
        Shell32.ILFree(pidl);
        result = enumId.Next(1, out pidl, out count);
      }

      if (result != HResult.S_FALSE) {
        Marshal.ThrowExceptionForHR((int)result);
      }

      yield break;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    public PropVariant GetPropertyValue(PROPERTYKEY pkey, Type type) {
      return this._Item.GetPropertyValue(pkey, type);
    }


    public System.Drawing.Bitmap Thumbnail(int size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source) {
      var bmp = this._Item.GetShellThumbnail(size, format, source);
      return bmp;
    }

    public System.Windows.Media.Imaging.BitmapSource ThumbnailSource(int size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source) {
      this._Item.Thumbnail.CurrentSize = new System.Windows.Size(size, size);
      this._Item.Thumbnail.RetrievalOption = source;
      this._Item.Thumbnail.FormatOption = format;
      return this._Item.Thumbnail.BitmapSource;
    }

    public bool IsSearchFolder { get; set; }

    public IListItemEx Parent {
      get {
        if (this._Item.Parent == null)
          return null;
        var parent = new FileSystemListItem();
        parent.Initialize(this.ParentHandle, this._Item.Parent.Pidl, 0);
        return parent;
      }
    }

    public IShellFolder GetIShellFolder() {
      return this._Item.GetIShellFolder();
    }
    #endregion



    #region IDisposable Members

    public void Dispose() {
      if (this._Item != null) {
        this._Item.Dispose();
      }
    }

    #endregion


    #region IListItemEx Members


    

    #endregion

    #region IListItemEx Members


    

    #endregion

    #region IListItemEx Members


    public bool IsLink {
      get {
        return this._Item.IsLink;
      }
    }

    #endregion

    #region IListItemEx Members


    public void Initialize(IntPtr lvHandle, IntPtr pidl) {
      throw new NotImplementedException();
    }

    #endregion

    #region IListItemEx Members


    public string ToolTipText {
      get { return this._Item.ToolTipText; }
    }

    #endregion

    #region IListItemEx Members


    

    #endregion

    #region IListItemEx Members




    #endregion

    #region IListItemEx Members

    #endregion

    #region IListItemEx Members


    System.IO.DriveInfo IListItemEx.GetDriveInfo() {
      throw new NotImplementedException();
    }

    #endregion

    #region IListItemEx Members


    public HResult ExtractAndDrawThumbnail(IntPtr hdc, uint iconSize, out WTS_CACHEFLAGS flags, User32.RECT iconBounds, out bool retrieved, bool isHidden, bool isRefresh = false) {
      var res = this._Item.Thumbnail.ExtractAndDrawThumbnail(hdc, iconSize, out flags, iconBounds, out retrieved, isHidden, isRefresh);
      return res;
    }

    #endregion

    #region IListItemEx Members


    public IntPtr GetHBitmap(int iconSize, bool isThumbnail, bool isForce = false) {
      var bmp = this._Item.Thumbnail.GetHBitmap(iconSize, isThumbnail, isForce);
      return bmp;
    }

    #endregion

    #region IEquatable<IListItemEx> Members

    public bool Equals(IListItemEx other) {
      if (other == null) return false;
      return other.ParsingName.Equals(this.ParsingName, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Implements the == (equality) operator.
    /// </summary>
    /// <param name="leftShellObject">First object to compare.</param>
    /// <param name="rightShellObject">Second object to compare.</param>
    /// <returns>True if leftShellObject equals rightShellObject; false otherwise.</returns>
    public static bool operator ==(FileSystemListItem leftShellObject, FileSystemListItem rightShellObject) {
      if ((object)leftShellObject == null) {
        return ((object)rightShellObject == null);
      }
      return leftShellObject.Equals(rightShellObject);
    }

    /// <summary>
    /// Implements the != (inequality) operator.
    /// </summary>
    /// <param name="leftShellObject">First object to compare.</param>
    /// <param name="rightShellObject">Second object to compare.</param>
    /// <returns>True if leftShellObject does not equal leftShellObject; false otherwise.</returns>
    public static bool operator !=(FileSystemListItem leftShellObject, FileSystemListItem rightShellObject) {
      return !(leftShellObject == rightShellObject);
    }

    #endregion

    public static FileSystemListItem ToFileSystemItem(IntPtr parentHandle, String path) {
      var fsItem = new FileSystemListItem();
      fsItem.Initialize(parentHandle, path, 0);
      return fsItem;
    }

    public static FileSystemListItem ToFileSystemItem(IntPtr parentHandle, IntPtr pidl) {
      var fsItem = new FileSystemListItem();
      fsItem.Initialize(parentHandle, pidl, 0);
      return fsItem;
    }

    public static FileSystemListItem ToFileSystemItem(IntPtr parentHandle, ShellSearchFolder folder) {
      var fsItem = new FileSystemListItem();
      fsItem.InitializeWithShellItem(folder, parentHandle, 0);
      return fsItem;
    }

    #region IListItemEx Members


    public string GetDisplayName(SIGDN type) {
      return this._Item.GetDisplayName(type);
    }

    #endregion

    #region IListItemEx Members


    public IExtractIconPWFlags GetShield() {
      return this._Item.GetShield();
    }

    #endregion

    #region IListItemEx Members


    public int GetSystemImageListIndex(IntPtr pidl, ShellIconType type, ShellIconFlags flags) {
      var info = new SHFILEINFO();
      IntPtr result = Shell32.SHGetFileInfo(pidl, 0, out info, Marshal.SizeOf(info),
                          SHGFI.Icon | SHGFI.SysIconIndex | SHGFI.OverlayIndex | SHGFI.PIDL | (SHGFI)type | (SHGFI)flags);

      if (result == IntPtr.Zero) {
        throw new Exception("Error retrieving shell folder icon");
      }

      User32.DestroyIcon(info.hIcon);
      return info.iIcon;
    }

    #endregion

    #region IListItemEx Members


    public Boolean RefreshThumb(int iconSize, out WTS_CACHEFLAGS flags) {
      return this._Item.Thumbnail.RefreshThumbnail((uint)iconSize, out flags);
    }

    #endregion

    #region IListItemEx Members


    public IntPtr Icon { get; set; }

    #endregion

    #region IEqualityComparer<IListItemEx> Members

    public bool Equals(IListItemEx x, IListItemEx y) {
      return x.Equals(y);
    }

    public int GetHashCode(IListItemEx obj) {
      return 0;
      if (Object.ReferenceEquals(obj, null)) return 0;

      //Get hash code for the Name field if it is not null.
      int hashProductName = obj.ParsingName == null ? 0 : obj.ParsingName.GetHashCode();

      ////Get hash code for the Code field.
      //int hashProductCode = product.Code.GetHashCode();

      //Calculate the hash code for the product.
      return hashProductName;
    }

    #endregion
  }
}
