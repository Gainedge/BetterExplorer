//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Shell;
using MS.WindowsAPICodePack.Internal;
using System.Text;
using Microsoft.WindowsAPICodePack.Shell.Interop;
using System.Collections;
using WindowsHelper;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System.Security;

namespace Microsoft.WindowsAPICodePack.Controls
{

    public enum ShellViewGetItemObject
    {
        Background = 0x00000000,
        Selection = 0x00000001,
        AllView = 0x00000002,
        Checked = 0x00000003,
        TypeMask = 0x0000000F,
        ViewOrderFlag = unchecked((int)0x80000000)
    }

    [Flags]
    internal enum FolderOptions
    {
        AutoArrange = 0x00000001,
        AbbreviatedNames = 0x00000002,
        SnapToGrid = 0x00000004,
        OwnerData = 0x00000008,
        BestFitWindow = 0x00000010,
        Desktop = 0x00000020,
        SingleSelection = 0x00000040,
        NoSubfolders = 0x00000080,
        Transparent = 0x00000100,
        NoClientEdge = 0x00000200,
        NoScroll = 0x00000400,
        AlignLeft = 0x00000800,
        NoIcons = 0x00001000,
        ShowSelectionAlways = 0x00002000,
        NoVisible = 0x00004000,
        SingleClickActivate = 0x00008000,
        NoWebView = 0x00010000,
        HideFilenames = 0x00020000,
        CheckSelect = 0x00040000,
        NoEnumRefresh = 0x00080000,
        NoGrouping = 0x00100000,
        FullRowSelect = 0x00200000,
        NoFilters = 0x00400000,
        NoColumnHeaders = 0x00800000,
        NoHeaderInAllViews = 0x01000000,
        ExtendedTiles = 0x02000000,
        TriCheckSelect = 0x04000000,
        AutoCheckSelect = 0x08000000,
        NoBrowserViewState = 0x10000000,
        SubsetGroups = 0x20000000,
        UseSearchFolders = 0x40000000,
        AllowRightToLeftReading = unchecked((int)0x80000000)
    }

    internal enum FolderViewMode
    {
        Auto = -1,
        First = 1,
        Icon = 1,
        SmallIcon = 2,
        List = 3,
        Details = 4,
        Thumbnail = 5,
        Tile = 6,
        Thumbstrip = 7,
        Content = 8,
        Last = 8
    }

    internal enum ExplorerPaneState
    {
        DoNotCare = 0x00000000,
        DefaultOn = 0x00000001,
        DefaultOff = 0x00000002,
        StateMask = 0x0000ffff,
        InitialState = 0x00010000,
        Force = 0x00020000
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal class FolderSettings
    {
        public FolderViewMode ViewMode;
        public FolderOptions Options;
    }

    [Flags]
    internal enum ExplorerBrowserOptions
    {
        NavigateOnce = 0x00000001,
        ShowFrames = 0x00000002,
        AlwaysNavigate = 0x00000004,
        NoTravelLog = 0x00000008,
        NoWrapperWindow = 0x00000010,
        HtmlSharepointView = 0x00000020
    }

    internal enum CommDlgBrowserStateChange
    {
        SetFocus = 0,
        KillFocus = 1,
        SelectionChange = 2,
        Rename = 3,
        StateChange = 4
    }

    internal enum CommDlgBrowserNotifyType
    {
        Done = 1,
        Start = 2
    }

    internal enum CommDlgBrowser2ViewFlags
    {
        ShowAllFiles = 0x00000001,
        IsFileSave = 0x00000002,
        AllowPreviewPane = 0x00000004,
        NoSelectVerb = 0x00000008,
        NoIncludeItem = 0x00000010,
        IsFolderPicker = 0x00000020
    }

    // Disable warning if a method declaration hides another inherited from a parent COM interface
    // To successfully import a COM interface, all inherited methods need to be declared again with 
    // the exception of those already declared in "IUnknown"
#pragma warning disable 108


    [ComImport,
     TypeLibType(TypeLibTypeFlags.FCanCreate),
     ClassInterface(ClassInterfaceType.None),
     Guid(ExplorerBrowserCLSIDGuid.ExplorerBrowser)]
    internal class ExplorerBrowserClass : IExplorerBrowser
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Initialize(IntPtr hwndParent, [In]ref NativeRect prc, [In] FolderSettings pfs);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void Destroy();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void SetRect([In, Out] ref IntPtr phdwp, NativeRect rcBrowser);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void SetPropertyBag([MarshalAs(UnmanagedType.LPWStr)] string pszPropertyBag);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void SetEmptyText([MarshalAs(UnmanagedType.LPWStr)] string pszEmptyText);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HResult SetFolderSettings(FolderSettings pfs);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HResult Advise(IntPtr psbe, out uint pdwCookie);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HResult Unadvise(uint dwCookie);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void SetOptions([In]ExplorerBrowserOptions dwFlag);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void GetOptions(out ExplorerBrowserOptions pdwFlag);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void BrowseToIDList(IntPtr pidl, uint uFlags);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HResult BrowseToObject([MarshalAs(UnmanagedType.IUnknown)] object punk, uint uFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void FillFromObject([MarshalAs(UnmanagedType.IUnknown)] object punk, int dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern void RemoveAll();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public virtual extern HResult GetCurrentView(ref Guid riid, out IntPtr ppv);
    }


    [ComImport,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid(ExplorerBrowserIIDGuid.IExplorerBrowser)]
    internal interface IExplorerBrowser
    {
        /// <summary>
        /// Prepares the browser to be navigated.
        /// </summary>
        /// <param name="hwndParent">A handle to the owner window or control.</param>
        /// <param name="prc">A pointer to a RECT containing the coordinates of the bounding rectangle 
        /// the browser will occupy. The coordinates are relative to hwndParent. If this parameter is NULL,
        /// then method IExplorerBrowser::SetRect should subsequently be called.</param>
        /// <param name="pfs">A pointer to a FOLDERSETTINGS structure that determines how the folder will be
        /// displayed in the view. If this parameter is NULL, then method IExplorerBrowser::SetFolderSettings
        /// should be called, otherwise, the default view settings for the folder are used.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Initialize(IntPtr hwndParent, [In] ref NativeRect prc, [In] FolderSettings pfs);

        /// <summary>
        /// Destroys the browser.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Destroy();

        /// <summary>
        /// Sets the size and position of the view windows created by the browser.
        /// </summary>
        /// <param name="phdwp">A pointer to a DeferWindowPos handle. This paramater can be NULL.</param>
        /// <param name="rcBrowser">The coordinates that the browser will occupy.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetRect([In, Out] ref IntPtr phdwp, NativeRect rcBrowser);

        /// <summary>
        /// Sets the name of the property bag.
        /// </summary>
        /// <param name="pszPropertyBag">A pointer to a constant, null-terminated, Unicode string that contains
        /// the name of the property bag. View state information that is specific to the application of the 
        /// client is stored (persisted) using this name.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetPropertyBag([MarshalAs(UnmanagedType.LPWStr)] string pszPropertyBag);

        /// <summary>
        /// Sets the default empty text.
        /// </summary>
        /// <param name="pszEmptyText">A pointer to a constant, null-terminated, Unicode string that contains 
        /// the empty text.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetEmptyText([MarshalAs(UnmanagedType.LPWStr)] string pszEmptyText);

        /// <summary>
        /// Sets the folder settings for the current view.
        /// </summary>
        /// <param name="pfs">A pointer to a FOLDERSETTINGS structure that contains the folder settings 
        /// to be applied.</param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SetFolderSettings(FolderSettings pfs);

        /// <summary>
        /// Initiates a connection with IExplorerBrowser for event callbacks.
        /// </summary>
        /// <param name="psbe">A pointer to the IExplorerBrowserEvents interface of the object to be 
        /// advised of IExplorerBrowser events</param>
        /// <param name="pdwCookie">When this method returns, contains a token that uniquely identifies 
        /// the event listener. This allows several event listeners to be subscribed at a time.</param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Advise(IntPtr psbe, out uint pdwCookie);

        /// <summary>
        /// Terminates an advisory connection.
        /// </summary>
        /// <param name="dwCookie">A connection token previously returned from IExplorerBrowser::Advise.
        /// Identifies the connection to be terminated.</param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Unadvise([In] uint dwCookie);

        /// <summary>
        /// Sets the current browser options.
        /// </summary>
        /// <param name="dwFlag">One or more EXPLORER_BROWSER_OPTIONS flags to be set.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetOptions([In]ExplorerBrowserOptions dwFlag);

        /// <summary>
        /// Gets the current browser options.
        /// </summary>
        /// <param name="pdwFlag">When this method returns, contains the current EXPLORER_BROWSER_OPTIONS 
        /// for the browser.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetOptions(out ExplorerBrowserOptions pdwFlag);

        /// <summary>
        /// Browses to a pointer to an item identifier list (PIDL)
        /// </summary>
        /// <param name="pidl">A pointer to a const ITEMIDLIST (item identifier list) that specifies an object's 
        /// location as the destination to navigate to. This parameter can be NULL.</param>
        /// <param name="uFlags">A flag that specifies the category of the pidl. This affects how 
        /// navigation is accomplished</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void BrowseToIDList(IntPtr pidl, uint uFlags);

        /// <summary>
        /// Browse to an object
        /// </summary>
        /// <param name="punk">A pointer to an object to browse to. If the object cannot be browsed, 
        /// an error value is returned.</param>
        /// <param name="uFlags">A flag that specifies the category of the pidl. This affects how 
        /// navigation is accomplished. </param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult BrowseToObject([MarshalAs(UnmanagedType.IUnknown)] object punk, uint uFlags);

        /// <summary>
        /// Creates a results folder and fills it with items.
        /// </summary>
        /// <param name="punk">An interface pointer on the source object that will fill the IResultsFolder</param>
        /// <param name="dwFlags">One of the EXPLORER_BROWSER_FILL_FLAGS</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void FillFromObject([MarshalAs(UnmanagedType.IUnknown)] object punk, int dwFlags);

        /// <summary>
        /// Removes all items from the results folder.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RemoveAll();

        /// <summary>
        /// Gets an interface for the current view of the browser.
        /// </summary>
        /// <param name="riid">A reference to the desired interface ID.</param>
        /// <param name="ppv">When this method returns, contains the interface pointer requested in riid. 
        /// This will typically be IShellView or IShellView2. </param>
        /// <returns></returns>
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentView(ref Guid riid, out IntPtr ppv);
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IServiceProvider),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IServiceProvider
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall)]
        HResult QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject);
    };

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IShellFolderView),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellFolderView
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall)]
        HResult Select(uint dwFlags);
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall)]
        HResult SetClipboard(bool bMove);
    };

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IFolderView),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFolderView
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCurrentViewMode([Out] out uint pViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCurrentViewMode(uint ViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFolder(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Item(int iItemIndex, out IntPtr ppidl);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void ItemCount(uint uFlags, out int pcItems);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Items(uint uFlags, ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectionMarkedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFocusedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetItemPosition(IntPtr pidl, out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSpacing([Out] out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDefaultSpacing(out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetAutoArrange();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectItem(int iItem, uint dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectAndPositionItems(uint cidl, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl, ref NativePoint apt, uint dwFlags);
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IFolderView2),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFolderView2 : IFolderView
    {
        // IFolderView
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentViewMode(out uint pViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCurrentViewMode(uint ViewMode);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFolder(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Item(int iItemIndex, out IntPtr ppidl);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult ItemCount(uint uFlags, out int pcItems);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Items(uint uFlags, ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectionMarkedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetFocusedItem(out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetItemPosition(IntPtr pidl, out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSpacing([Out] out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetDefaultSpacing(out NativePoint ppt);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetAutoArrange();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectItem(int iItem, uint dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SelectAndPositionItems(uint cidl, IntPtr apidl, ref NativePoint apt, uint dwFlags);

        // IFolderView2
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetGroupBy(IntPtr key, bool fAscending);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetGroupBy(out Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser.PROPERTYKEY pkey, out bool pfAscending);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetViewProperty(IntPtr pidl, IntPtr propkey, object propvar);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetViewProperty(IntPtr pidl, IntPtr propkey, out object ppropvar);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetExtendedTileViewProperties(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszPropList);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetText(int iType, [MarshalAs(UnmanagedType.LPWStr)] string pwszText);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCurrentFolderFlags(uint dwMask, uint dwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCurrentFolderFlags(out uint pdwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSortColumnCount(out int pcColumns);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetSortColumns(IntPtr rgSortColumns, int cColumns);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSortColumns(out IntPtr rgSortColumns, int cColumns);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetItem(int iItem, ref Guid riid, out IShellItem ppv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetVisibleItem(int iStart, bool fPrevious, out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectedItem(int iStart, out int piItem);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelection(bool fNoneImpliesFolder, out IShellItemArray ppsia);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetSelectionState(IntPtr pidl, out WindowsHelper.WindowsAPI.SVSIF pdwFlags);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void InvokeVerbOnSelection([In, MarshalAs(UnmanagedType.LPWStr)] string pszVerb);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SetViewModeAndIconSize(int uViewMode, int iImageSize);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetViewModeAndIconSize(out int puViewMode, out int piImageSize);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetGroupSubsetCount(uint cVisibleRows);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetGroupSubsetCount(out uint pcVisibleRows);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetRedraw(bool fRedrawOn);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsMoveInSameFolder();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void DoRename();
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IExplorerPaneVisibility),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IExplorerPaneVisibility
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetPaneState(ref Guid explorerPane, out ExplorerPaneState peps);
    };

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IExplorerBrowserEvents),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IExplorerBrowserEvents
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnNavigationPending(IntPtr pidlFolder);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnViewCreated([MarshalAs(UnmanagedType.IUnknown)]  object psv);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnNavigationComplete(IntPtr pidlFolder);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnNavigationFailed(IntPtr pidlFolder);
    }
    public enum CM_ENUM_FLAGS
    {
        CM_ENUM_ALL = 0x00000001,
        CM_ENUM_VISIBLE = 0x00000002
    } ;

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid(ExplorerBrowserIIDGuid.IColumnManager)]
    public interface IColumnManager
    {
        void SetColumnInfo([In] ref ExplorerBrowser.PROPERTYKEY propkey, [In] ref IntPtr pcmci);

        void GetColumnInfo(ref ExplorerBrowser.PROPERTYKEY propkey, out IntPtr pcmci);

        void GetColumnCount(CM_ENUM_FLAGS dwFlags, out uint puCount);

        void SetColumns(ArrayList rgkeyOrder, uint cVisible);

        void GetColumns(CM_ENUM_FLAGS dwFlags, [Out] [MarshalAs(UnmanagedType.LPArray)] PropertyKey[] rgkeyOrder, uint cColumns);
    }

    #region Unused - Keeping for debugging bug #885228

    //[ComImport,
    // Guid(ExplorerBrowserIIDGuid.ICommDlgBrowser),
    // InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //internal interface ICommDlgBrowser
    //{
    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult OnDefaultCommand(IntPtr ppshv);

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult OnStateChange(
    //        IntPtr ppshv,
    //        CommDlgBrowserStateChange uChange);

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult IncludeObject(
    //        IntPtr ppshv,
    //        IntPtr pidl);
    //}

    //[ComImport,
    // Guid(ExplorerBrowserIIDGuid.ICommDlgBrowser2),
    // InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    //internal interface ICommDlgBrowser2
    //{
    //    // dlg

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult OnDefaultCommand(IntPtr ppshv);

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult OnStateChange(
    //        IntPtr ppshv,
    //        CommDlgBrowserStateChange uChange);

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult IncludeObject(
    //        IntPtr ppshv,
    //        IntPtr pidl);

    //    // dlg2

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult GetDefaultMenuText(
    //        [In] IShellView shellView,
    //        StringBuilder buffer, //A pointer to a buffer that is used by the Shell browser to return the default shortcut menu text.
    //        [In] int bufferMaxLength); //should be max size = 260?

    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult GetViewFlags(CommDlgBrowser2ViewFlags pdwFlags);


    //    [PreserveSig]
    //    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    //    HResult Notify(
    //        IntPtr pshv,
    //        CommDlgBrowserNotifyType notifyType);
    //}

    #endregion

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.ICommDlgBrowser3),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICommDlgBrowser3
    {
        // dlg1
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnDefaultCommand(IntPtr ppshv);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnStateChange(
            IntPtr ppshv,
            CommDlgBrowserStateChange uChange);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult IncludeObject(
            IntPtr ppshv,
            IntPtr pidl);

        // dlg2
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetDefaultMenuText(
            IShellView shellView,
            IntPtr buffer, //A pointer to a buffer that is used by the Shell browser to return the default shortcut menu text.
            int bufferMaxLength); //should be max size = 260?

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetViewFlags(
            [Out] out uint pdwFlags); // CommDlgBrowser2ViewFlags 


        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Notify(
            IntPtr pshv, CommDlgBrowserNotifyType notifyType);

        // dlg3
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentFilter(
            StringBuilder pszFileSpec,
            int cchFileSpec);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnColumnClicked(
            IShellView ppshv,
            int iColumn);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult OnPreViewCreated(IShellView ppshv);
    }

    [ComImport,
   Guid(ExplorerBrowserIIDGuid.IInputObject),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IInputObject
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult UIActivateIO(bool fActivate, ref System.Windows.Forms.Message pMsg);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult HasFocusIO();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult TranslateAcceleratorIO(ref System.Windows.Forms.Message pMsg);

    };

    //--------------------------------------------------------------------------
    //
    // Interface:   IShellBrowser
    //
    //  IShellBrowser interface is the interface that is provided by the shell
    // explorer/folder frame window. When it creates the 'contents pane' of
    // a shell folder (which provides IShellFolder interface), it calls its
    // CreateViewObject member function to create an IShellView object. Then,
    // it calls its CreateViewWindow member to create the 'contents pane'
    // window. The pointer to the IShellBrowser interface is passed to
    // the IShellView object as a parameter to this CreateViewWindow member
    // function call.
    //
    //    +--------------------------+  <-- Explorer window
    //    | [] Explorer              |
    //    |--------------------------+       IShellBrowser
    //    | File Edit View ..        |
    //    |--------------------------|
    //    |        |                 |
    //    |        |              <-------- Content pane
    //    |        |                 |
    //    |        |                 |       IShellView
    //    |        |                 |
    //    |        |                 |
    //    +--------------------------+
    //
    //
    //
    // [Member functions]
    //
    //
    // IShellBrowser::GetWindow(phwnd)
    //
    //   Inherited from IOleWindow::GetWindow.
    //
    //
    // IShellBrowser::ContextSensitiveHelp(fEnterMode)
    //
    //   Inherited from IOleWindow::ContextSensitiveHelp.
    //
    //
    // IShellBrowser::InsertMenusSB(hmenuShared, lpMenuWidths)
    //
    //   Similar to the IOleInPlaceFrame::InsertMenus. The explorer will put
    //  'File' and 'Edit' pulldown in the File menu group, 'View' and 'Tools'
    //  in the Container menu group and 'Help' in the Window menu group. Each
    //  pulldown menu will have a uniqu ID, FCIDM_MENU_FILE/EDIT/VIEW/TOOLS/HELP
    //  The view is allowed to insert menuitems into those sub-menus by those
    //  IDs must be between FCIDM_SHVIEWFIRST and FCIDM_SHVIEWLAST.
    //
    //
    // IShellBrowser::SetMenuSB(hmenuShared, holemenu, hwndActiveObject)
    //
    //   Similar to the IOleInPlaceFrame::SetMenu. The explorer ignores the
    //  holemenu parameter (reserved for future enhancement)  and performs
    //  menu-dispatch based on the menuitem IDs (see the description above).
    //  It is important to note that the explorer will add different
    //  set of menuitems depending on whether the view has a focus or not.
    //  Therefore, it is very important to call ISB::OnViewWindowActivate
    //  whenever the view window (or its children) gets the focus.
    //
    //
    // IShellBrowser::RemoveMenusSB(hmenuShared)
    //
    //   Same as the IOleInPlaceFrame::RemoveMenus.
    //
    //
    // IShellBrowser::SetStatusTextSB(pszStatusText)
    //
    //   Same as the IOleInPlaceFrame::SetStatusText. It is also possible to
    //  send messages directly to the status window via SendControlMsg.
    //
    //
    // IShellBrowser::EnableModelessSB(fEnable)
    //
    //   Same as the IOleInPlaceFrame::EnableModeless.
    //
    //
    // IShellBrowser::TranslateAcceleratorSB(lpmsg, wID)
    //
    //   Same as the IOleInPlaceFrame::TranslateAccelerator, but will be
    //  never called because we don't support EXEs (i.e., the explorer has
    //  the message loop). This member function is defined here for possible
    //  future enhancement.
    //
    //
    // IShellBrowser::BrowseObject(pidl, wFlags)")
    //
    //   The view calls this member to let shell explorer browse to another")
    //  folder. The pidl and wFlags specifies the folder to be browsed.")
    //
    //  Following three flags specifies whether it creates another window or not.
    //   SBSP_SAMEBROWSER  -- Browse to another folder with the same window.
    //   SBSP_NEWBROWSER   -- Creates another window for the specified folder.
    //   SBSP_DEFBROWSER   -- Default behavior (respects the view option).
    //
    //  Following three flags specifies open, explore, or default mode. These   .
    //  are ignored if SBSP_SAMEBROWSER or (SBSP_DEFBROWSER && (single window   .
    //  browser || explorer)).                                                  .
    //   SBSP_OPENMODE     -- Use a normal folder window
    //   SBSP_EXPLOREMODE  -- Use an explorer window
    //   SBSP_DEFMODE      -- Use the same as the current window
    //
    //  Following three flags specifies the pidl.
    //   SBSP_ABSOLUTE -- pidl is an absolute pidl (relative from desktop)
    //   SBSP_RELATIVE -- pidl is relative from the current folder.
    //   SBSP_PARENT   -- Browse the parent folder (ignores the pidl)
    //   SBSP_NAVIGATEBACK    -- Navigate back (ignores the pidl)
    //   SBSP_NAVIGATEFORWARD -- Navigate forward (ignores the pidl)
    //
    //  Following two flags control history manipulation as result of navigate
    //   SBSP_WRITENOHISTORY -- write no history (shell folder) entry
    //   SBSP_NOAUTOSELECT -- suppress selection in history pane
    //
    // IShellBrowser::GetViewStateStream(grfMode, ppstm)
    //
    //   The browser returns an IStream interface as the storage for view
    //  specific state information.
    //
    //   grfMode -- Specifies the read/write access (STGM_READ/WRITE/READWRITE)
    //   ppstm   -- Specifies the IStream *variable to be filled.
    //
    //
    // IShellBrowser::GetControlWindow(id, phwnd)
    //
    //   The shell view may call this member function to get the window handle
    //  of Explorer controls (toolbar or status winodw -- FCW_TOOLBAR or
    //  FCW_STATUS).
    //
    //
    // IShellBrowser::SendControlMsg(id, uMsg, wParam, lParam, pret)
    //
    //   The shell view calls this member function to send control messages to
    //  one of Explorer controls (toolbar or status window -- FCW_TOOLBAR or
    //  FCW_STATUS).
    //
    //
    // IShellBrowser::QueryActiveShellView(IShellView * ppshv)
    //
    //   This member returns currently activated (displayed) shellview object.
    //  A shellview never need to call this member function.
    //
    //
    // IShellBrowser::OnViewWindowActive(pshv)
    //
    //   The shell view window calls this member function when the view window
    //  (or one of its children) got the focus. It MUST call this member before
    //  calling IShellBrowser::InsertMenus, because it will insert different
    //  set of menu items depending on whether the view has the focus or not.
    //
    //
    // IShellBrowser::SetToolbarItems(lpButtons, nButtons, uFlags)
    //
    //   The view calls this function to add toolbar items to the exporer's
    //  toolbar. 'lpButtons' and 'nButtons' specifies the array of toolbar
    //  items. 'uFlags' must be one of FCT_MERGE, FCT_CONFIGABLE, FCT_ADDTOEND.
    //
    //-------------------------------------------------------------------------

    [ComImport, Guid(ExplorerBrowserIIDGuid.IShellBrowser)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    public interface IShellBrowser {
      [PreserveSig]
      int GetWindow(out IntPtr hwnd);
      [PreserveSig]
      int ContextSensitiveHelp(int fEnterMode);

      [PreserveSig]
      int InsertMenusSB(IntPtr hmenuShared, IntPtr lpMenuWidths);
      [PreserveSig]
      int SetMenuSB(IntPtr hmenuShared, IntPtr holemenuRes, IntPtr hwndActiveObject);
      [PreserveSig]
      int RemoveMenusSB(IntPtr hmenuShared);
      [PreserveSig]
      int SetStatusTextSB(IntPtr pszStatusText);
      [PreserveSig]
      int EnableModelessSB(bool fEnable);
      [PreserveSig]
      int TranslateAcceleratorSB(IntPtr pmsg, short wID);
      [PreserveSig]
      int BrowseObject(IntPtr pidl, [MarshalAs(UnmanagedType.U4)] uint wFlags);
      [PreserveSig]
      int GetViewStateStream(uint grfMode, IntPtr ppStrm);
      [PreserveSig]
      int GetControlWindow(uint id, out IntPtr phwnd);
      [PreserveSig]
      int SendControlMsg(uint id, uint uMsg, uint wParam, uint lParam, IntPtr pret);
      [PreserveSig]
      int QueryActiveShellView([MarshalAs(UnmanagedType.Interface)] ref IShellView ppshv);
      [PreserveSig]
      int OnViewWindowActive([MarshalAs(UnmanagedType.Interface)] IShellView pshv);
      [PreserveSig]
      int SetToolbarItems(IntPtr lpButtons, uint nButtons, uint uFlags);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("6d5140c1-7436-11ce-8034-00aa006009fa"), SuppressUnmanagedCodeSecurity]
    public interface _IServiceProvider {
      void QueryService(
              [In, MarshalAs(UnmanagedType.LPStruct)] Guid guid,
              [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
              [MarshalAs(UnmanagedType.Interface)] out object Obj);
    }

    [ComImport,
     Guid(ExplorerBrowserIIDGuid.IShellView),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellView
    {
        // IOleWindow
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetWindow(
            out IntPtr phwnd);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult ContextSensitiveHelp(
            bool fEnterMode);

        // IShellView
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult TranslateAccelerator(
            IntPtr pmsg);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult EnableModeless(
            bool fEnable);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult UIActivate(
            uint uState);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult Refresh();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult CreateViewWindow(
            [MarshalAs(UnmanagedType.IUnknown)] object psvPrevious,
            IntPtr pfs,
            [MarshalAs(UnmanagedType.IUnknown)] object psb,
            IntPtr prcView,
            out IntPtr phWnd);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult DestroyViewWindow();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetCurrentInfo(
            out IntPtr pfs);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult AddPropertySheetPages(
            uint dwReserved,
            IntPtr pfn,
            uint lparam);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SaveViewState();

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SelectItem(
            IntPtr pidlItem,
            WindowsHelper.WindowsAPI.SVSIF uFlags);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult GetItemObject(
            ShellViewGetItemObject uItem,
            ref Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }

    public struct SV2CVW2_PARAMS
    {
        public uint cbSize;

        public IShellView psvPrev;
        public uint pfs; // const
        public IntPtr psbOwner; // IShellBrowser
        public WindowsAPI.RECT prcView; // RECT
        public Guid pvid; // const

        public IntPtr hwndView;
    }

    public struct POINT {
      long x;
      long y;
    }
    [ComImport,
    Guid("88E39E80-3578-11CF-AE69-08002B2E1262"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellView2 : IShellView
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetView(Guid pvid, IntPtr uView);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult CreateViewWindow2(SV2CVW2_PARAMS lpParams);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        HResult SelectAndPositionItem(IntPtr pidlItem, WindowsAPI.SVSIF flags, IntPtr point);

    };

#pragma warning restore 108

}
