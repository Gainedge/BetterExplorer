using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell._Plugin_Interfaces {

  public interface IListItemEx : IEnumerable<IListItemEx>, IDisposable, IEquatable<IListItemEx> {

    IShellItem ComInterface { get; }

    IListItemEx Parent { get;}

    String DisplayName { get; set; }

    String Extension { get; set; }

    String FileSystemPath { get; set; }

    Int32 ItemIndex { get; set; }

    IntPtr ParentHandle { get; set; }

    Boolean IsNeedRefreshing { get; set; }

    Boolean IsInvalid { get; set; }

    Boolean IsOnlyLowQuality { get; set; }

    Boolean IsThumbnailLoaded { get; set; }

    Boolean IsInitialised { get; set; }

    Int32 OverlayIconIndex { get; set; }

    IExtractIconPWFlags IconType { get; set; }

    IntPtr ILPidl { get; }

    IntPtr PIDL { get; }

    Int32 ShieldedIconIndex { get; set; }

    Boolean IsIconLoaded { get; set; }

    Boolean IsFileSystem { get; set; }

    Boolean IsNetworkPath { get; set; }

    Boolean IsDrive { get; set; }

    Boolean IsSearchFolder { get; set; }

    Bitmap Thumbnail(Int32 size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source);

    BitmapSource ThumbnailSource(Int32 size, ShellThumbnailFormatOption format, ShellThumbnailRetrievalOption source);

    String ParsingName { get; set; }

    Boolean IsBrowsable { get; set; }

    Boolean IsFolder { get; set; }

    Boolean HasSubFolders { get; }

    Boolean IsHidden { get; set; }

    Boolean IsLink { get;  }

    Boolean IsShared { get; set; }

    void Initialize(IntPtr lvHandle, String path);

    void Initialize(IntPtr lvHandle, String path, Int32 index);

    void Initialize(IntPtr lvHandle, IntPtr pidl, int index);

    void Initialize(IntPtr lvHandle, IntPtr pidl);

    void InitializeWithParent(ShellItem parent, IntPtr lvHandle, IntPtr pidl, int index);

    PropVariant GetPropertyValue(PROPERTYKEY pkey, Type type);

    Dictionary<PROPERTYKEY, Object> ColumnValues { get; set; }

    IListItemEx[] GetSubItems(Boolean isEnumHidden);

    IShellFolder GetIShellFolder();

    String ToolTipText { get; }

    IntPtr AbsolutePidl { get; }

    DriveInfo GetDriveInfo();

    HResult ExtractAndDrawThumbnail(IntPtr hdc, uint iconSize, out WTS_CACHEFLAGS flags, User32.RECT iconBounds, out bool retrieved, bool isHidden, bool isRefresh = false);

    IntPtr GetHBitmap(int iconSize, bool isThumbnail, bool isForce = false);

    Boolean RefreshThumb(int iconSize, out WTS_CACHEFLAGS flags);

    String GetDisplayName(BExplorer.Shell.Interop.SIGDN type);

    IExtractIconPWFlags GetShield();

    int GetSystemImageListIndex(IntPtr pidl, ShellIconType type, ShellIconFlags flags); 
		//Work On This and the TODOList

	}
}
