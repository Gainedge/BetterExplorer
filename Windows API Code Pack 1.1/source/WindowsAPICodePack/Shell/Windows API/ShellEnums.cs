//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsAPICodePack.Shell
{
      
    #region Shell Library Enums

    internal enum LibraryFolderFilter
    {
        ForceFileSystem = 1,
        StorageItems = 2,
        AllItems = 3
    };

    [Flags]
    internal enum LibraryOptions
    {
        Default = 0,
        PinnedToNavigationPane = 0x1,
        MaskAll = 0x1
    };

    internal enum DefaultSaveFolderType
    {
        Detect = 1,
        Private = 2,
        Public = 3
    };

    internal enum LibrarySaveOptions
    {
        FailIfThere = 0,
        OverrideExisting = 1,
        MakeUniqueName = 2
    };

    internal enum LibraryManageDialogOptions
    {
        Default = 0,
        NonIndexableLocationWarning = 1
    };

        
    #endregion

    [Flags]
    public enum TPM
    {
      TPM_LEFTBUTTON = 0x0000,
      TPM_RIGHTBUTTON = 0x0002,
      TPM_LEFTALIGN = 0x0000,
      TPM_CENTERALIGN = 0x000,
      TPM_RIGHTALIGN = 0x000,
      TPM_TOPALIGN = 0x0000,
      TPM_VCENTERALIGN = 0x0010,
      TPM_BOTTOMALIGN = 0x0020,
      TPM_HORIZONTAL = 0x0000,
      TPM_VERTICAL = 0x0040,
      TPM_NONOTIFY = 0x0080,
      TPM_RETURNCMD = 0x0100,
      TPM_RECURSE = 0x0001,
      TPM_HORPOSANIMATION = 0x0400,
      TPM_HORNEGANIMATION = 0x0800,
      TPM_VERPOSANIMATION = 0x1000,
      TPM_VERNEGANIMATION = 0x2000,
      TPM_NOANIMATION = 0x4000,
      TPM_LAYOUTRTL = 0x8000,
    }

    [Flags]
    public enum CMF : uint
    {
      NORMAL = 0x00000000,
      DEFAULTONLY = 0x00000001,
      VERBSONLY = 0x00000002,
      EXPLORE = 0x00000004,
      NOVERBS = 0x00000008,
      CANRENAME = 0x00000010,
      NODEFAULT = 0x00000020,
      INCLUDESTATIC = 0x00000040,
      EXTENDEDVERBS = 0x00000100,
      RESERVED = 0xffff0000,
      DISABLEDVERBS = 0x00000200,
    }

    [Flags]
    public enum SFGAO : uint
    {
      BROWSABLE = 0x8000000,
      CANCOPY = 1,
      CANDELETE = 0x20,
      CANLINK = 4,
      CANMONIKER = 0x400000,
      CANMOVE = 2,
      CANRENAME = 0x10,
      CAPABILITYMASK = 0x177,
      COMPRESSED = 0x4000000,
      CONTENTSMASK = 0x80000000,
      DISPLAYATTRMASK = 0xfc000,
      DROPTARGET = 0x100,
      ENCRYPTED = 0x2000,
      FILESYSANCESTOR = 0x10000000,
      FILESYSTEM = 0x40000000,
      FOLDER = 0x20000000,
      GHOSTED = 0x8000,
      HASPROPSHEET = 0x40,
      HASSTORAGE = 0x400000,
      HASSUBFOLDER = 0x80000000,
      HIDDEN = 0x80000,
      ISSLOW = 0x4000,
      LINK = 0x10000,
      NEWCONTENT = 0x200000,
      NONENUMERATED = 0x100000,
      READONLY = 0x40000,
      REMOVABLE = 0x2000000,
      SHARE = 0x20000,
      STORAGE = 8,
      STORAGEANCESTOR = 0x800000,
      STORAGECAPMASK = 0x70c50008,
      STREAM = 0x400000,
      VALIDATE = 0x1000000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAP
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public ushort bmPlanes;
        public ushort bmBitsPixel;
        public IntPtr bmBits;
    }

    [Flags]
    internal enum FileOpenOptions
    {
        OverwritePrompt = 0x00000002,
        StrictFileTypes = 0x00000004,
        NoChangeDirectory = 0x00000008,
        PickFolders = 0x00000020,
        // Ensure that items returned are filesystem items.
        ForceFilesystem = 0x00000040,
        // Allow choosing items that have no storage.
        AllNonStorageItems = 0x00000080,
        NoValidate = 0x00000100,
        AllowMultiSelect = 0x00000200,
        PathMustExist = 0x00000800,
        FileMustExist = 0x00001000,
        CreatePrompt = 0x00002000,
        ShareAware = 0x00004000,
        NoReadOnlyReturn = 0x00008000,
        NoTestFileCreate = 0x00010000,
        HideMruPlaces = 0x00020000,
        HidePinnedPlaces = 0x00040000,
        NoDereferenceLinks = 0x00100000,
        DontAddToRecent = 0x02000000,
        ForceShowHidden = 0x10000000,
        DefaultNoMiniMode = 0x20000000
    }
    internal enum ControlState
    {
        Inactive = 0x00000000,
        Enable = 0x00000001,
        Visible = 0x00000002
    }
    public enum ShellItemDesignNameOptions
    {
        Normal = 0x00000000,           // SIGDN_NORMAL
        ParentRelativeParsing = unchecked((int)0x80018001),   // SIGDN_INFOLDER | SIGDN_FORPARSING
        DesktopAbsoluteParsing = unchecked((int)0x80028000),  // SIGDN_FORPARSING
        ParentRelativeEditing = unchecked((int)0x80031001),   // SIGDN_INFOLDER | SIGDN_FOREDITING
        DesktopAbsoluteEditing = unchecked((int)0x8004c000),  // SIGDN_FORPARSING | SIGDN_FORADDRESSBAR
        FileSystemPath = unchecked((int)0x80058000),             // SIGDN_FORPARSING
        Url = unchecked((int)0x80068000),                     // SIGDN_FORPARSING
        ParentRelativeForAddressBar = unchecked((int)0x8007c001),     // SIGDN_INFOLDER | SIGDN_FORPARSING | SIGDN_FORADDRESSBAR
        ParentRelative = unchecked((int)0x80080001)           // SIGDN_INFOLDER
    }

    /// <summary>
    /// Indicate flags that modify the property store object retrieved by methods 
    /// that create a property store, such as IShellItem2::GetPropertyStore or 
    /// IPropertyStoreFactory::GetPropertyStore.
    /// </summary>
    [Flags]
    public enum GetPropertyStoreOptions
    {
        /// <summary>
        /// Meaning to a calling process: Return a read-only property store that contains all 
        /// properties. Slow items (offline files) are not opened. 
        /// Combination with other flags: Can be overridden by other flags.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Meaning to a calling process: Include only properties directly from the property
        /// handler, which opens the file on the disk, network, or device. Meaning to a file 
        /// folder: Only include properties directly from the handler.
        /// 
        /// Meaning to other folders: When delegating to a file folder, pass this flag on 
        /// to the file folder; do not do any multiplexing (MUX). When not delegating to a 
        /// file folder, ignore this flag instead of returning a failure code.
        /// 
        /// Combination with other flags: Cannot be combined with GPS_TEMPORARY, 
        /// GPS_FASTPROPERTIESONLY, or GPS_BESTEFFORT.
        /// </summary>
        HandlePropertiesOnly = 0x1,

        /// <summary>
        /// Meaning to a calling process: Can write properties to the item. 
        /// Note: The store may contain fewer properties than a read-only store. 
        /// 
        /// Meaning to a file folder: ReadWrite.
        /// 
        /// Meaning to other folders: ReadWrite. Note: When using default MUX, 
        /// return a single unmultiplexed store because the default MUX does not support ReadWrite.
        /// 
        /// Combination with other flags: Cannot be combined with GPS_TEMPORARY, GPS_FASTPROPERTIESONLY, 
        /// GPS_BESTEFFORT, or GPS_DELAYCREATION. Implies GPS_HANDLERPROPERTIESONLY.
        /// </summary>
        ReadWrite = 0x2,

        /// <summary>
        /// Meaning to a calling process: Provides a writable store, with no initial properties, 
        /// that exists for the lifetime of the Shell item instance; basically, a property bag 
        /// attached to the item instance. 
        /// 
        /// Meaning to a file folder: Not applicable. Handled by the Shell item.
        /// 
        /// Meaning to other folders: Not applicable. Handled by the Shell item.
        /// 
        /// Combination with other flags: Cannot be combined with any other flag. Implies GPS_READWRITE
        /// </summary>
        Temporary = 0x4,

        /// <summary>
        /// Meaning to a calling process: Provides a store that does not involve reading from the 
        /// disk or network. Note: Some values may be different, or missing, compared to a store 
        /// without this flag. 
        /// 
        /// Meaning to a file folder: Include the "innate" and "fallback" stores only. Do not load the handler.
        /// 
        /// Meaning to other folders: Include only properties that are available in memory or can 
        /// be computed very quickly (no properties from disk, network, or peripheral IO devices). 
        /// This is normally only data sources from the IDLIST. When delegating to other folders, pass this flag on to them.
        /// 
        /// Combination with other flags: Cannot be combined with GPS_TEMPORARY, GPS_READWRITE, 
        /// GPS_HANDLERPROPERTIESONLY, or GPS_DELAYCREATION.
        /// </summary>
        FastPropertiesOnly = 0x8,

        /// <summary>
        /// Meaning to a calling process: Open a slow item (offline file) if necessary. 
        /// Meaning to a file folder: Retrieve a file from offline storage, if necessary. 
        /// Note: Without this flag, the handler is not created for offline files.
        /// 
        /// Meaning to other folders: Do not return any properties that are very slow.
        /// 
        /// Combination with other flags: Cannot be combined with GPS_TEMPORARY or GPS_FASTPROPERTIESONLY.
        /// </summary>
        OpensLowItem = 0x10,

        /// <summary>
        /// Meaning to a calling process: Delay memory-intensive operations, such as file access, until 
        /// a property is requested that requires such access. 
        /// 
        /// Meaning to a file folder: Do not create the handler until needed; for example, either 
        /// GetCount/GetAt or GetValue, where the innate store does not satisfy the request. 
        /// Note: GetValue might fail due to file access problems.
        /// 
        /// Meaning to other folders: If the folder has memory-intensive properties, such as 
        /// delegating to a file folder or network access, it can optimize performance by 
        /// supporting IDelayedPropertyStoreFactory and splitting up its properties into a 
        /// fast and a slow store. It can then use delayed MUX to recombine them.
        /// 
        /// Combination with other flags: Cannot be combined with GPS_TEMPORARY or 
        /// GPS_READWRITE
        /// </summary>
        DelayCreation = 0x20,

        /// <summary>
        /// Meaning to a calling process: Succeed at getting the store, even if some 
        /// properties are not returned. Note: Some values may be different, or missing,
        /// compared to a store without this flag. 
        /// 
        /// Meaning to a file folder: Succeed and return a store, even if the handler or 
        /// innate store has an error during creation. Only fail if substores fail.
        /// 
        /// Meaning to other folders: Succeed on getting the store, even if some properties 
        /// are not returned.
        /// 
        /// Combination with other flags: Cannot be combined with GPS_TEMPORARY, 
        /// GPS_READWRITE, or GPS_HANDLERPROPERTIESONLY.
        /// </summary>
        BestEffort = 0x40,

        /// <summary>
        /// Mask for valid GETPROPERTYSTOREFLAGS values.
        /// </summary>
        MaskValid = 0xff,
    }

    public enum ShellItemAttributeOptions
    {
        // if multiple items and the attirbutes together.
        And = 0x00000001,
        // if multiple items or the attributes together.
        Or = 0x00000002,
        // Call GetAttributes directly on the 
        // ShellFolder for multiple attributes.
        AppCompat = 0x00000003,

        // A mask for SIATTRIBFLAGS_AND, SIATTRIBFLAGS_OR, and SIATTRIBFLAGS_APPCOMPAT. Callers normally do not use this value.
        Mask = 0x00000003,

        // Windows 7 and later. Examine all items in the array to compute the attributes. 
        // Note that this can result in poor performance over large arrays and therefore it 
        // should be used only when needed. Cases in which you pass this flag should be extremely rare.
        AllItems = 0x00004000
    }

    internal enum FileDialogEventShareViolationResponse
    {
        Default = 0x00000000,
        Accept = 0x00000001,
        Refuse = 0x00000002
    }
    internal enum FileDialogEventOverwriteResponse
    {
        Default = 0x00000000,
        Accept = 0x00000001,
        Refuse = 0x00000002
    }
    internal enum FileDialogAddPlacement
    {
        Bottom = 0x00000000,
        Top = 0x00000001,
    }

    [Flags]
    internal enum SIIGBF
    {
        ResizeToFit = 0x00,
        BiggerSizeOk = 0x01,
        MemoryOnly = 0x02,
        IconOnly = 0x04,
        ThumbnailOnly = 0x08,
        InCacheOnly = 0x10,
    }

    [Flags]
    internal enum ThumbnailOptions
    {
        Extract = 0x00000000,
        InCacheOnly = 0x00000001,
        FastExtract = 0x00000002,
        ForceExtraction = 0x00000004,
        SlowReclaim = 0x00000008,
        ExtractDoNotCache = 0x00000020
    }

    [Flags]
    internal enum ThumbnailCacheOptions
    {
        Default = 0x00000000,
        LowQuality = 0x00000001,
        Cached = 0x00000002,
    }

    [Flags]
    public enum ShellFileGetAttributesOptions
    {
        /// <summary>
        /// The specified items can be copied.
        /// </summary>
        CanCopy = 0x00000001,

        /// <summary>
        /// The specified items can be moved.
        /// </summary>
        CanMove = 0x00000002,

        /// <summary>
        /// Shortcuts can be created for the specified items. This flag has the same value as DROPEFFECT. 
        /// The normal use of this flag is to add a Create Shortcut item to the shortcut menu that is displayed 
        /// during drag-and-drop operations. However, SFGAO_CANLINK also adds a Create Shortcut item to the Microsoft 
        /// Windows Explorer's File menu and to normal shortcut menus. 
        /// If this item is selected, your application's IContextMenu::InvokeCommand is invoked with the lpVerb 
        /// member of the CMINVOKECOMMANDINFO structure set to "link." Your application is responsible for creating the link.
        /// </summary>
        CanLink = 0x00000004,

        /// <summary>
        /// The specified items can be bound to an IStorage interface through IShellFolder::BindToObject.
        /// </summary>
        Storage = 0x00000008,

        /// <summary>
        /// The specified items can be renamed.
        /// </summary>
        CanRename = 0x00000010,

        /// <summary>
        /// The specified items can be deleted.
        /// </summary>
        CanDelete = 0x00000020,

        /// <summary>
        /// The specified items have property sheets.
        /// </summary>
        HasPropertySheet = 0x00000040,

        /// <summary>
        /// The specified items are drop targets.
        /// </summary>
        DropTarget = 0x00000100,

        /// <summary>
        /// This flag is a mask for the capability flags.
        /// </summary>
        CapabilityMask = 0x00000177,

        /// <summary>
        /// Windows 7 and later. The specified items are system items.
        /// </summary>
        System = 0x00001000,

        /// <summary>
        /// The specified items are encrypted.
        /// </summary>
        Encrypted = 0x00002000,

        /// <summary>
        /// Indicates that accessing the object = through IStream or other storage interfaces, 
        /// is a slow operation. 
        /// Applications should avoid accessing items flagged with SFGAO_ISSLOW.
        /// </summary>
        IsSlow = 0x00004000,

        /// <summary>
        /// The specified items are ghosted icons.
        /// </summary>
        Ghosted = 0x00008000,

        /// <summary>
        /// The specified items are shortcuts.
        /// </summary>
        Link = 0x00010000,

        /// <summary>
        /// The specified folder objects are shared.
        /// </summary>    
        Share = 0x00020000,

        /// <summary>
        /// The specified items are read-only. In the case of folders, this means 
        /// that new items cannot be created in those folders.
        /// </summary>
        ReadOnly = 0x00040000,

        /// <summary>
        /// The item is hidden and should not be displayed unless the 
        /// Show hidden files and folders option is enabled in Folder Settings.
        /// </summary>
        Hidden = 0x00080000,

        /// <summary>
        /// This flag is a mask for the display attributes.
        /// </summary>
        DisplayAttributeMask = 0x000FC000,

        /// <summary>
        /// The specified folders contain one or more file system folders.
        /// </summary>
        FileSystemAncestor = 0x10000000,

        /// <summary>
        /// The specified items are folders.
        /// </summary>
        Folder = 0x20000000,

        /// <summary>
        /// The specified folders or file objects are part of the file system 
        /// that is, they are files, directories, or root directories).
        /// </summary>
        FileSystem = 0x40000000,

        /// <summary>
        /// The specified folders have subfolders = and are, therefore, 
        /// expandable in the left pane of Windows Explorer).
        /// </summary>
        HasSubFolder = unchecked((int)0x80000000),

        /// <summary>
        /// This flag is a mask for the contents attributes.
        /// </summary>
        ContentsMask = unchecked((int)0x80000000),

        /// <summary>
        /// When specified as input, SFGAO_VALIDATE instructs the folder to validate that the items 
        /// pointed to by the contents of apidl exist. If one or more of those items do not exist, 
        /// IShellFolder::GetAttributesOf returns a failure code. 
        /// When used with the file system folder, SFGAO_VALIDATE instructs the folder to discard cached 
        /// properties retrieved by clients of IShellFolder2::GetDetailsEx that may 
        /// have accumulated for the specified items.
        /// </summary>
        Validate = 0x01000000,

        /// <summary>
        /// The specified items are on removable media or are themselves removable devices.
        /// </summary>
        Removable = 0x02000000,

        /// <summary>
        /// The specified items are compressed.
        /// </summary>
        Compressed = 0x04000000,

        /// <summary>
        /// The specified items can be browsed in place.
        /// </summary>
        Browsable = 0x08000000,

        /// <summary>
        /// The items are nonenumerated items.
        /// </summary>
        Nonenumerated = 0x00100000,

        /// <summary>
        /// The objects contain new content.
        /// </summary>
        NewContent = 0x00200000,

        /// <summary>
        /// It is possible to create monikers for the specified file objects or folders.
        /// </summary>
        CanMoniker = 0x00400000,

        /// <summary>
        /// Not supported.
        /// </summary>
        HasStorage = 0x00400000,

        /// <summary>
        /// Indicates that the item has a stream associated with it that can be accessed 
        /// by a call to IShellFolder::BindToObject with IID_IStream in the riid parameter.
        /// </summary>
        Stream = 0x00400000,

        /// <summary>
        /// Children of this item are accessible through IStream or IStorage. 
        /// Those children are flagged with SFGAO_STORAGE or SFGAO_STREAM.
        /// </summary>
        StorageAncestor = 0x00800000,

        /// <summary>
        /// This flag is a mask for the storage capability attributes.
        /// </summary>
        StorageCapabilityMask = 0x70C50008,

        /// <summary>
        /// Mask used by PKEY_SFGAOFlags to remove certain values that are considered 
        /// to cause slow calculations or lack context. 
        /// Equal to SFGAO_VALIDATE | SFGAO_ISSLOW | SFGAO_HASSUBFOLDER.
        /// </summary>
        PkeyMask = unchecked((int)0x81044000),
    }

    [Flags]
    public enum ShellFolderEnumerationOptions 
    {
        CheckingForChildren = 0x0010,
        Folders = 0x0020,
        NonFolders = 0x0040,
        IncludeHidden = 0x0080,
        InitializeOnFirstNext = 0x0100,
        NetPrinterSearch = 0x0200,
        Shareable = 0x0400,
        Storage = 0x0800,
        NavigationEnum = 0x1000,
        FastItems = 0x2000,
        FlatList = 0x4000,
        EnableAsync = 0x8000,
        SuperHidden = 0x10000,

    }


    /// <summary>
    /// CommonFileDialog AddPlace locations
    /// </summary>
    public enum FileDialogAddPlaceLocation
    {
        /// <summary>
        /// At the bottom of the Favorites or Places list.
        /// </summary>
        Bottom = 0x00000000,

        /// <summary>
        /// At the top of the Favorites or Places list.
        /// </summary>
        Top = 0x00000001,
    }

    /// <summary>
    /// One of the values that indicates how the ShellObject DisplayName should look.
    /// </summary>
    public enum DisplayNameType
    {
        /// <summary>
        /// Returns the display name relative to the desktop.
        /// </summary>
        Default = 0x00000000,

        /// <summary>
        /// Returns the parsing name relative to the parent folder.
        /// </summary>
        RelativeToParent = unchecked((int)0x80018001),

        /// <summary>
        /// Returns the path relative to the parent folder in a 
        /// friendly format as displayed in an address bar.
        /// </summary>
        RelativeToParentAddressBar = unchecked((int)0x8007c001),

        /// <summary>
        /// Returns the parsing name relative to the desktop.
        /// </summary>
        RelativeToDesktop = unchecked((int)0x80028000),

        /// <summary>
        /// Returns the editing name relative to the parent folder.
        /// </summary>
        RelativeToParentEditing = unchecked((int)0x80031001),

        /// <summary>
        /// Returns the editing name relative to the desktop.
        /// </summary>
        RelativeToDesktopEditing = unchecked((int)0x8004c000),

        /// <summary>
        /// Returns the display name relative to the file system path.
        /// </summary>
        FileSystemPath = unchecked((int)0x80058000),

        /// <summary>
        /// Returns the display name relative to a URL.
        /// </summary>
        Url = unchecked((int)0x80068000),
    }
    /// <summary>
    /// Available Library folder types
    /// </summary>
    public enum LibraryFolderType
    {
        /// <summary>
        /// General Items
        /// </summary>
        Generic = 0,

        /// <summary>
        /// Documents
        /// </summary>
        Documents,

        /// <summary>
        /// Music
        /// </summary>
        Music,

        /// <summary>
        /// Pictures
        /// </summary>
        Pictures,

        /// <summary>
        /// Videos
        /// </summary>
        Videos

    }

    /// <summary>
    /// Flags controlling the appearance of a window
    /// </summary>
    public enum WindowShowCommand
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        Hide = 0,

        /// <summary>
        /// Activates and displays the window (including restoring
        /// it to its original size and position).
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        Minimized = 2,

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        Maximized = 3,

        /// <summary>
        /// Similar to <see cref="Normal"/>, except that the window
        /// is not activated.
        /// </summary>
        ShowNoActivate = 4,

        /// <summary>
        /// Activates the window and displays it in its current size
        /// and position.
        /// </summary>
        Show = 5,

        /// <summary>
        /// Minimizes the window and activates the next top-level window.
        /// </summary>
        Minimize = 6,

        /// <summary>
        /// Minimizes the window and does not activate it.
        /// </summary>
        ShowMinimizedNoActivate = 7,

        /// <summary>
        /// Similar to <see cref="Normal"/>, except that the window is not
        /// activated.
        /// </summary>
        ShowNA = 8,

        /// <summary>
        /// Activates and displays the window, restoring it to its original
        /// size and position.
        /// </summary>
        Restore = 9,

        /// <summary>
        /// Sets the show state based on the initial value specified when
        /// the process was created.
        /// </summary>
        Default = 10,

        /// <summary>
        /// Minimizes a window, even if the thread owning the window is not
        /// responding.  Use this only to minimize windows from a different
        /// thread.
        /// </summary>
        ForceMinimize = 11
    }

    /// <summary>
    /// Provides a set of flags to be used with <see cref="Microsoft.WindowsAPICodePack.Shell.SearchCondition"/> 
    /// to indicate the operation in <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory"/> methods.
    /// </summary>
    public enum SearchConditionOperation
    {
        /// <summary>
        /// An implicit comparison between the value of the property and the value of the constant.
        /// </summary>
        Implicit = 0,

        /// <summary>
        /// The value of the property and the value of the constant must be equal.
        /// </summary>
        Equal = 1,

        /// <summary>
        /// The value of the property and the value of the constant must not be equal.
        /// </summary>
        NotEqual = 2,

        /// <summary>
        /// The value of the property must be less than the value of the constant.
        /// </summary>
        LessThan = 3,

        /// <summary>
        /// The value of the property must be greater than the value of the constant.
        /// </summary>
        GreaterThan = 4,

        /// <summary>
        /// The value of the property must be less than or equal to the value of the constant.
        /// </summary>
        LessThanOrEqual = 5,

        /// <summary>
        /// The value of the property must be greater than or equal to the value of the constant.
        /// </summary>
        GreaterThanOrEqual = 6,

        /// <summary>
        /// The value of the property must begin with the value of the constant.
        /// </summary>
        ValueStartsWith = 7,

        /// <summary>
        /// The value of the property must end with the value of the constant.
        /// </summary>
        ValueEndsWith = 8,

        /// <summary>
        /// The value of the property must contain the value of the constant.
        /// </summary>
        ValueContains = 9,

        /// <summary>
        /// The value of the property must not contain the value of the constant.
        /// </summary>
        ValueNotContains = 10,

        /// <summary>
        /// The value of the property must match the value of the constant, where '?' 
        /// matches any single character and '*' matches any sequence of characters.
        /// </summary>
        DosWildcards = 11,

        /// <summary>
        /// The value of the property must contain a word that is the value of the constant.
        /// </summary>
        WordEqual = 12,

        /// <summary>
        /// The value of the property must contain a word that begins with the value of the constant.
        /// </summary>
        WordStartsWith = 13,

        /// <summary>
        /// The application is free to interpret this in any suitable way.
        /// </summary>
        ApplicationSpecific = 14
    }

    /// <summary>
    /// Set of flags to be used with <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionFactory"/>.
    /// </summary>
    public enum SearchConditionType
    {
        /// <summary>
        /// Indicates that the values of the subterms are combined by "AND".
        /// </summary>
        And = 0,

        /// <summary>
        /// Indicates that the values of the subterms are combined by "OR".
        /// </summary>
        Or = 1,

        /// <summary>
        /// Indicates a "NOT" comparison of subterms.
        /// </summary>
        Not = 2,

        /// <summary>
        /// Indicates that the node is a comparison between a property and a 
        /// constant value using a <see cref="Microsoft.WindowsAPICodePack.Shell.SearchConditionOperation"/>.
        /// </summary>
        Leaf = 3,
    }

    /// <summary>
    /// Used to describe the view mode.
    /// </summary>
    public enum FolderLogicalViewMode
    {
        /// <summary>
        /// The view is not specified.
        /// </summary>
        Unspecified = -1,

        /// <summary>
        /// This should have the same affect as Unspecified.
        /// </summary>
        None = 0,

        /// <summary>
        /// The minimum valid enumeration value. Used for validation purposes only.
        /// </summary>
        First = 1,

        /// <summary>
        /// Details view.
        /// </summary>
        Details = 1,

        /// <summary>
        /// Tiles view.
        /// </summary>
        Tiles = 2,

        /// <summary>
        /// Icons view.
        /// </summary>
        Icons = 3,

        /// <summary>
        /// Windows 7 and later. List view.
        /// </summary>
        List = 4,

        /// <summary>
        /// Windows 7 and later. Content view.
        /// </summary>
        Content = 5,

        /// <summary>
        /// The maximum valid enumeration value. Used for validation purposes only.
        /// </summary>
        Last = 5
    }

    /// <summary>
    /// The direction in which the items are sorted.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// A default value for sort direction, this value should not be used;
        /// instead use Descending or Ascending.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The items are sorted in descending order. Whether the sort is alphabetical, numerical, 
        /// and so on, is determined by the data type of the column indicated in propkey.
        /// </summary>
        Descending = -1,

        /// <summary>
        /// The items are sorted in ascending order. Whether the sort is alphabetical, numerical, 
        /// and so on, is determined by the data type of the column indicated in propkey.
        /// </summary>
        Ascending = 1,
    }

    /// <summary>
    /// Provides a set of flags to be used with IQueryParser::SetOption and 
    /// IQueryParser::GetOption to indicate individual options.
    /// </summary>
    public enum StructuredQuerySingleOption
    {
        /// <summary>
        /// The value should be VT_LPWSTR and the path to a file containing a schema binary.
        /// </summary>
        Schema,

        /// <summary>
        /// The value must be VT_EMPTY (the default) or a VT_UI4 that is an LCID. It is used
        /// as the locale of contents (not keywords) in the query to be searched for, when no
        /// other information is available. The default value is the current keyboard locale.
        /// Retrieving the value always returns a VT_UI4.
        /// </summary>
        Locale,

        /// <summary>
        /// This option is used to override the default word breaker used when identifying keywords
        /// in queries. The default word breaker is chosen according to the language of the keywords
        /// (cf. SQSO_LANGUAGE_KEYWORDS below). When setting this option, the value should be VT_EMPTY
        /// for using the default word breaker, or a VT_UNKNOWN with an object supporting
        /// the IWordBreaker interface. Retrieving the option always returns a VT_UNKNOWN with an object
        /// supporting the IWordBreaker interface.
        /// </summary>
        WordBreaker,

        /// <summary>
        /// The value should be VT_EMPTY or VT_BOOL with VARIANT_TRUE to allow natural query
        /// syntax (the default) or VT_BOOL with VARIANT_FALSE to allow only advanced query syntax.
        /// Retrieving the option always returns a VT_BOOL.
        /// This option is now deprecated, use SQSO_SYNTAX.
        /// </summary>
        NaturalSyntax,

        /// <summary>
        /// The value should be VT_BOOL with VARIANT_TRUE to generate query expressions
        /// as if each word in the query had a star appended to it (unless followed by punctuation
        /// other than a parenthesis), or VT_EMPTY or VT_BOOL with VARIANT_FALSE to
        /// use the words as they are (the default). A word-wheeling application
        /// will generally want to set this option to true.
        /// Retrieving the option always returns a VT_BOOL.
        /// </summary>
        AutomaticWildcard,

        /// <summary>
        /// Reserved. The value should be VT_EMPTY (the default) or VT_I4.
        /// Retrieving the option always returns a VT_I4.
        /// </summary>
        TraceLevel,

        /// <summary>
        /// The value must be a VT_UI4 that is a LANGID. It defaults to the default user UI language.
        /// </summary>
        LanguageKeywords,

        /// <summary>
        /// The value must be a VT_UI4 that is a STRUCTURED_QUERY_SYNTAX value.
        /// It defaults to SQS_NATURAL_QUERY_SYNTAX.
        /// </summary>
        Syntax,

        /// <summary>
        /// The value must be a VT_BLOB that is a copy of a TIME_ZONE_INFORMATION structure.
        /// It defaults to the current time zone.
        /// </summary>
        TimeZone,

        /// <summary>
        /// This setting decides what connector should be assumed between conditions when none is specified.
        /// The value must be a VT_UI4 that is a CONDITION_TYPE. Only CT_AND_CONDITION and CT_OR_CONDITION
        /// are valid. It defaults to CT_AND_CONDITION.
        /// </summary>
        ImplicitConnector,

        /// <summary>
        /// This setting decides whether there are special requirements on the case of connector keywords (such
        /// as AND or OR). The value must be a VT_UI4 that is a CASE_REQUIREMENT value.
        /// It defaults to CASE_REQUIREMENT_UPPER_IF_AQS.
        /// </summary>
        ConnectorCase,

    }

    /// <summary>
    /// Provides a set of flags to be used with IQueryParser::SetMultiOption 
    /// to indicate individual options.
    /// </summary>
    public enum StructuredQueryMultipleOption
    {
        /// <summary>
        /// The key should be property name P. The value should be a
        /// VT_UNKNOWN with an IEnumVARIANT which has two values: a VT_BSTR that is another
        /// property name Q and a VT_I4 that is a CONDITION_OPERATION cop. A predicate with
        /// property name P, some operation and a value V will then be replaced by a predicate
        /// with property name Q, operation cop and value V before further processing happens.
        /// </summary>
        VirtualProperty,

        /// <summary>
        /// The key should be a value type name V. The value should be a
        /// VT_LPWSTR with a property name P. A predicate with no property name and a value of type
        /// V (or any subtype of V) will then use property P.
        /// </summary>
        DefaultProperty,

        /// <summary>
        /// The key should be a value type name V. The value should be a
        /// VT_UNKNOWN with a IConditionGenerator G. The GenerateForLeaf method of
        /// G will then be applied to any predicate with value type V and if it returns a query
        /// expression, that will be used. If it returns NULL, normal processing will be used
        /// instead.
        /// </summary>
        GeneratorForType,

        /// <summary>
        /// The key should be a property name P. The value should be a VT_VECTOR|VT_LPWSTR,
        /// where each string is a property name. The count must be at least one. This "map" will be
        /// added to those of the loaded schema and used during resolution. A second call with the
        /// same key will replace the current map. If the value is VT_NULL, the map will be removed.
        /// </summary>
        MapProperty,
    }

    /// <summary>
    /// Used by IQueryParserManager::SetOption to set parsing options. 
    /// This can be used to specify schemas and localization options.
    /// </summary>
    public enum QueryParserManagerOption
    {
        /// <summary>
        /// A VT_LPWSTR containing the name of the file that contains the schema binary. 
        /// The default value is StructuredQuerySchema.bin for the SystemIndex catalog 
        /// and StructuredQuerySchemaTrivial.bin for the trivial catalog.
        /// </summary>
        SchemaBinaryName = 0,

        /// <summary>
        /// Either a VT_BOOL or a VT_LPWSTR. If the value is a VT_BOOL and is FALSE, 
        /// a pre-localized schema will not be used. If the value is a VT_BOOL and is TRUE, 
        /// IQueryParserManager will use the pre-localized schema binary in 
        /// "%ALLUSERSPROFILE%\Microsoft\Windows". If the value is a VT_LPWSTR, the value should 
        /// contain the full path of the folder in which the pre-localized schema binary can be found. 
        /// The default value is VT_BOOL with TRUE.
        /// </summary>
        PreLocalizedSchemaBinaryPath = 1,

        /// <summary>
        /// A VT_LPWSTR containing the full path to the folder that contains the 
        /// unlocalized schema binary. The default value is "%SYSTEMROOT%\System32".
        /// </summary>
        UnlocalizedSchemaBinaryPath = 2,

        /// <summary>
        /// A VT_LPWSTR containing the full path to the folder that contains the 
        /// localized schema binary that can be read and written to as needed. 
        /// The default value is "%LOCALAPPDATA%\Microsoft\Windows".
        /// </summary>
        LocalizedSchemaBinaryPath = 3,

        /// <summary>
        /// A VT_BOOL. If TRUE, then the paths for pre-localized and localized binaries 
        /// have "\(LCID)" appended to them, where language code identifier (LCID) is 
        /// the decimal locale ID for the localized language. The default is TRUE.
        /// </summary>
        AppendLCIDToLocalizedPath = 4,

        /// <summary>
        /// A VT_UNKNOWN with an object supporting ISchemaLocalizerSupport. 
        /// This object will be used instead of the default localizer support object.
        /// </summary>
        LocalizerSupport = 5
    }


    [Flags]
    public enum CMIC : uint
    {
      Hotkey = 0x00000020,
      Icon = 0x00000010,
      FlagNoUi = 0x00000400,
      Unicode = 0x00004000,
      NoConsole = 0x00008000,
      Asyncok = 0x00100000,
      NoZoneChecks = 0x00800000,
      ShiftDown = 0x10000000,
      ControlDown = 0x40000000,
      FlagLogUsage = 0x04000000,
      PtInvoke = 0x20000000
    }


    [Flags]
    public enum SW
    {
      Hide = 0,
      ShowNormal = 1,
      Normal = 1,
      ShowMinimized = 2,
      ShowMaximized = 3,
      Maximize = 3,
      ShowNoActivate = 4,
      Show = 5,
      Minimize = 6,
      ShowMinNoActive = 7,
      ShowNa = 8,
      Restore = 9,
      ShowDefault = 10,
    }


    public enum MF
    {
      MF_BYCOMMAND = 0x00000000,
      MF_BYPOSITION = 0x00000400,
    }

    public enum MIIM : uint
    {
      MIIM_STATE = 0x00000001,
      MIIM_ID = 0x00000002,
      MIIM_SUBMENU = 0x00000004,
      MIIM_CHECKMARKS = 0x00000008,
      MIIM_TYPE = 0x00000010,
      MIIM_DATA = 0x00000020,
      MIIM_STRING = 0x00000040,
      MIIM_BITMAP = 0x00000080,
      MIIM_FTYPE = 0x00000100,
    }

    public enum MIM : uint
    {
      MIM_MAXHEIGHT = 0x00000001,
      MIM_BACKGROUND = 0x00000002,
      MIM_HELPID = 0x00000004,
      MIM_MENUDATA = 0x00000008,
      MIM_STYLE = 0x00000010,
      MIM_APPLYTOSUBMENUS = 0x80000000,
    }

    public enum MK
    {
      MK_LBUTTON = 0x0001,
      MK_RBUTTON = 0x0002,
      MK_SHIFT = 0x0004,
      MK_CONTROL = 0x0008,
      MK_MBUTTON = 0x0010,
      MK_ALT = 0x1000,
    }

    public enum MSG
    {
      WM_COMMAND = 0x0111,
      WM_VSCROLL = 0x0115,
      LVM_SETIMAGELIST = 0x1003,
      LVM_GETITEMCOUNT = 0x1004,
      LVM_GETITEMA = 0x1005,
      LVM_EDITLABEL = 0x1017,
      TVM_SETIMAGELIST = 4361,
      TVM_SETITEMW = 4415,
    }

    [Flags]
    public enum TVIF
    {
      TVIF_TEXT = 0x0001,
      TVIF_IMAGE = 0x0002,
      TVIF_PARAM = 0x0004,
      TVIF_STATE = 0x0008,
      TVIF_HANDLE = 0x0010,
      TVIF_SELECTEDIMAGE = 0x0020,
      TVIF_CHILDREN = 0x0040,
      TVIF_INTEGRAL = 0x0080,
    }

    [Flags]
    public enum TVIS
    {
      TVIS_SELECTED = 0x0002,
      TVIS_CUT = 0x0004,
      TVIS_DROPHILITED = 0x0008,
      TVIS_BOLD = 0x0010,
      TVIS_EXPANDED = 0x0020,
      TVIS_EXPANDEDONCE = 0x0040,
      TVIS_EXPANDPARTIAL = 0x0080,
      TVIS_OVERLAYMASK = 0x0F00,
      TVIS_STATEIMAGEMASK = 0xF000,
      TVIS_USERMASK = 0xF000,
    }
}
