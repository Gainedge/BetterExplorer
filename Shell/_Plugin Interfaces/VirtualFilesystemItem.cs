using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Media.Imaging;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell._Plugin_Interfaces {
  
  public class VirtualFilesystemItem : IListItemEx, IFileSystemBindData, IDisposable {
    protected WIN32_FIND_DATA _fd;

    //private VirtualFilesystemItem(ref WIN32_FIND_DATA pfd) { _fd = pfd; }

    public VirtualFilesystemItem(String path, System.IO.FileAttributes attributes) {
      this._fd = new WIN32_FIND_DATA() {
        dwFileAttributes = (uint)attributes
      };

      this.CreateSimplePidl(path);
    }

    public void GetFileData([MarshalAs(UnmanagedType.Struct), Out] out WIN32_FIND_DATA pdf) {
      pdf = _fd;
    }

    public void SetFileData([In, MarshalAs(UnmanagedType.Struct)] ref WIN32_FIND_DATA pfd) {
      _fd = pfd;
    }

    void CreateBindCtx(out IBindCtx pbc) {
      System.Runtime.InteropServices.ComTypes.BIND_OPTS bo = new System.Runtime.InteropServices.ComTypes.BIND_OPTS() {
        cbStruct = Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.BIND_OPTS)),
        dwTickCountDeadline = 0,
        grfFlags = 0,
        grfMode = Shell32.STGM_CREATE
      };
      try {
        Ole32.CreateBindCtx(0, out pbc);
        pbc.SetBindOptions(ref bo);
        pbc.RegisterObjectParam(Shell32.STR_FILE_SYS_BIND_DATA, this);
      } catch {
        pbc = null;
        throw;
      }
    }

    void CreateSimplePidl(string path) {
      IBindCtx pbc;
      IntPtr ppidl = IntPtr.Zero;
      uint sfgao;
      this.CreateBindCtx(out pbc);
      Shell32.SHParseDisplayName(path, pbc, out ppidl, 0, out sfgao);
      this.EnumPIDL = ppidl;
    }

    public IShellFolder GetParent(ref IntPtr pidl) {
      Guid iid = typeof(IShellFolder).GUID;
      IShellFolder ppv;
      Shell32.SHBindToParent(this.EnumPIDL, ref iid, out ppv, out pidl);
      return ppv;
    }

    bool _disposed = false;
    public IntPtr ParentPIDL { get; set; }
    public IntPtr EnumPIDL { get; set; }
    public IExtractIconPWFlags GetIconType() {
      throw new NotImplementedException();
    }

    public IShellItem ComInterface {
      get {
        object item = null;
        Guid iid = typeof(IShellItem).GUID;
        Shell32.SHCreateItemFromIDList(this.EnumPIDL, ref iid, out item);
        return (IShellItem)item;
      }
    }

    public IListItemEx Parent {
      get {
        Guid iid = typeof(IShellFolder).GUID;
        IntPtr pidl = IntPtr.Zero;
        IShellFolder ppv;
        Shell32.SHBindToParent(this.EnumPIDL, ref iid, out ppv, out pidl);
        this.ParentPIDL = pidl;
        var parent = new FileSystemListItem();
        parent.Initialize(IntPtr.Zero, pidl);
        return parent;
      }
    }

    public String DisplayName { get; }
    public String Extension { get; }
    public String FileSystemPath { get; }
    public Int32 ItemIndex { get; set; }
    public IntPtr ParentHandle { get; set; }
    public Boolean IsNeedRefreshing { get; set; }
    public Boolean IsInvalid { get; set; }
    public Boolean IsProcessed { get; set; }
    public Boolean IsOnlyLowQuality { get; set; }
    public Boolean IsThumbnailLoaded { get; set; }
    public Boolean IsInitialised { get; set; }
    public Int32 OverlayIconIndex { get; set; }
    public Int32 GroupIndex { get; set; }
    public Int32 IconIndex { get; set; }
    public System.Drawing.Size IconSize { get; set; }
    public IExtractIconPWFlags IconType { get; set; }
    public IntPtr ILPidl { get; }
    public IntPtr PIDL { get; }
    public IShellFolder IFolder { get; set; }
    public Int32 ShieldedIconIndex { get; set; }
    public Boolean IsIconLoaded { get; set; }
    public Boolean IsFileSystem { get; }
    public Boolean IsNetworkPath { get; }
    public Boolean IsDrive { get; }
    public Boolean IsSearchFolder { get; }
    public Bitmap Thumbnail(Int32 size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source) {
      throw new NotImplementedException();
    }

    public BitmapSource ThumbnailSource(Int32 size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source) {
      throw new NotImplementedException();
    }

    public BitmapSource BitmapSource { get; }
    public String ParsingName { get; }
    public Boolean IsBrowsable { get; }
    public Boolean IsFolder { get; }
    public Boolean HasSubFolders { get; }
    public Boolean IsHidden { get; }
    public Boolean IsLink { get; }
    public Boolean IsShared { get; }
    public Boolean IsSlow { get; }
    public Boolean IsParentSearchFolder { get; set; }
    public void Initialize(IntPtr lvHandle, String path) {
      throw new NotImplementedException();
    }

    public void Initialize(IntPtr lvHandle, String path, Int32 index) {
      throw new NotImplementedException();
    }

    public void Initialize(IntPtr lvHandle, IntPtr pidl, Int32 index) {
      throw new NotImplementedException();
    }

    public void Initialize(IntPtr lvHandle, IntPtr pidl) {
      throw new NotImplementedException();
    }

    public void InitializeWithParent(IntPtr parent, IntPtr lvHandle, IntPtr pidl, Int32 index) {
      throw new NotImplementedException();
    }

    public IListItemEx Clone(Boolean isHardCloning = false) {
      throw new NotImplementedException();
    }

    public PropVariant GetPropertyValue(PROPERTYKEY pkey, Type type) {
      throw new NotImplementedException();
    }

    public Dictionary<PROPERTYKEY, Object> ColumnValues { get; set; }
    public IEnumerable<IntPtr> GetItemsForCount(Boolean isEnumHidden) {
      throw new NotImplementedException();
    }

    public IEnumerable<IListItemEx> GetContents(Boolean isEnumHidden, Boolean isFlatList = false) {
      throw new NotImplementedException();
    }

    public IShellFolder GetIShellFolder() {
      throw new NotImplementedException();
    }

    public String ToolTipText { get; }
    public IntPtr AbsolutePidl { get; }
    public DriveInfo GetDriveInfo() {
      throw new NotImplementedException();
    }

    public Boolean IsRCWSet { get; set; }
    public Int32 RCWThread { get; set; }

    public HResult ExtractAndDrawThumbnail(IntPtr hdc, UInt32 iconSize, out WTS_CACHEFLAGS flags, User32.RECT iconBounds,
      out Boolean retrieved, Boolean isHidden, Boolean isRefresh = false) {
      throw new NotImplementedException();
    }

    public HResult NavigationStatus { get; set; }
    public IntPtr GetHBitmap(Int32 iconSize, Boolean isThumbnail, Boolean isForce = false, Boolean isBoth = false) {
      throw new NotImplementedException();
    }

    public Boolean RefreshThumb(Int32 iconSize, out WTS_CACHEFLAGS flags) {
      throw new NotImplementedException();
    }

    public String GetDisplayName(SIGDN type) {
      throw new NotImplementedException();
    }

    public IExtractIconPWFlags GetShield() {
      throw new NotImplementedException();
    }

    public Int32 GetSystemImageListIndex(IntPtr pidl, ShellIconType type, ShellIconFlags flags) {
      throw new NotImplementedException();
    }

    public Int32 GetUniqueID() {
      throw new NotImplementedException();
    }

    public Int32[] cColumns { get; set; }

    public void Dispose() {
      if (!_disposed) {
        if (this.EnumPIDL != IntPtr.Zero) {
          try {
            Marshal.FreeCoTaskMem(this.EnumPIDL);
          } catch { } finally {
            this.EnumPIDL = IntPtr.Zero;
          }
        }
        GC.SuppressFinalize(this);
      }
      _disposed = true;
    }

    public IEnumerator<IListItemEx> GetEnumerator() {
      throw new NotImplementedException();
    }

    public Boolean Equals(IListItemEx other) {
      throw new NotImplementedException();
    }

    ~VirtualFilesystemItem() { Dispose(); }
    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    public Boolean Equals(IListItemEx x, IListItemEx y) {
      throw new NotImplementedException();
    }

    public Int32 GetHashCode(IListItemEx obj) {
      throw new NotImplementedException();
    }
  }
}
