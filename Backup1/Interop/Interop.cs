// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using VistaBridgeInterop = Microsoft.SDK.Samples.VistaBridge.Interop;

namespace Windows7.DesktopIntegration.Interop
{

    internal enum KNOWNDESTCATEGORY
    {
        KDC_FREQUENT = 1,
        KDC_RECENT
    }

    internal enum APPDOCLISTTYPE
    {
        ADLT_RECENT = 0,
        ADLT_FREQUENT
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }

    internal enum TBPFLAG
    {
        TBPF_NOPROGRESS = 0,
        TBPF_INDETERMINATE = 0x1,
        TBPF_NORMAL = 0x2,
        TBPF_ERROR = 0x4,
        TBPF_PAUSED = 0x8
    }

    internal enum TBATFLAG
    {
        TBATF_USEMDITHUMBNAIL = 0x1,
        TBATF_USEMDILIVEPREVIEW = 0x2
    }

    internal enum THBMASK
    {
        THB_BITMAP = 0x1,
        THB_ICON = 0x2,
        THB_TOOLTIP = 0x4,
        THB_FLAGS = 0x8
    }

    internal enum THBFLAGS
    {
        THBF_ENABLED = 0,
        THBF_DISABLED = 0x1,
        THBF_DISMISSONCLICK = 0x2,
        THBF_NOBACKGROUND = 0x4,
        THBF_HIDDEN = 0x8
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct THUMBBUTTON
    {
        [MarshalAs(UnmanagedType.U4)]
        public THBMASK dwMask;
        public uint iId;
        public uint iBitmap;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szTip;
        [MarshalAs(UnmanagedType.U4)]
        public THBFLAGS dwFlags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PropertyKey
    {
        public Guid fmtid;
        public uint pid;

        public PropertyKey(Guid fmtid, uint pid)
        {
            this.fmtid = fmtid;
            this.pid = pid;
        }

        public static PropertyKey PKEY_Title = new PropertyKey(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
        public static PropertyKey PKEY_AppUserModel_ID = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);
        public static PropertyKey PKEY_AppUserModel_IsDestListSeparator = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 6);
        public static PropertyKey PKEY_AppUserModel_RelaunchCommand = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 2);
        public static PropertyKey PKEY_AppUserModel_RelaunchDisplayNameResource = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 4);
        public static PropertyKey PKEY_AppUserModel_RelaunchIconResource = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 3);
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct CALPWSTR
    {
        [FieldOffset(0)]
        internal uint cElems;
        [FieldOffset(4)]
        internal IntPtr pElems;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct PropVariant
    {
        [FieldOffset(0)]
        private ushort vt;
        [FieldOffset(8)]
        private IntPtr pointerValue;
        [FieldOffset(8)]
        private byte byteValue;
        [FieldOffset(8)]
        private long longValue;
        [FieldOffset(8)]
        private short boolValue;
        [MarshalAs(UnmanagedType.Struct)]
        [FieldOffset(8)]
        private CALPWSTR calpwstr;

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);

        public VarEnum VarType
        {
            get { return (VarEnum)vt; }
        }

        public void SetValue(String val)
        {
            this.Clear();
            this.vt = (ushort)VarEnum.VT_LPWSTR;
            this.pointerValue = Marshal.StringToCoTaskMemUni(val);
        }
        public void SetValue(bool val)
        {
            this.Clear();
            this.vt = (ushort)VarEnum.VT_BOOL;
            this.boolValue = val ? (short)-1 : (short)0;
        }

        public string GetValue()
        {
            return Marshal.PtrToStringUni(this.pointerValue);
        }

        public void Clear()
        {
            PropVariantClear(ref this);
        }
    }

    internal enum SHARD
    {
        SHARD_PIDL = 0x1,
        SHARD_PATHA = 0x2,
        SHARD_PATHW = 0x3,
        SHARD_APPIDINFO = 0x4, // indicates the data type is a pointer to a SHARDAPPIDINFO structure
        SHARD_APPIDINFOIDLIST = 0x5, // indicates the data type is a pointer to a SHARDAPPIDINFOIDLIST structure
        SHARD_LINK = 0x6, // indicates the data type is a pointer to an IShellLink instance
        SHARD_APPIDINFOLINK = 0x7, // indicates the data type is a pointer to a SHARDAPPIDINFOLINK structure 
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct SHARDAPPIDINFO
    {
        //[MarshalAs(UnmanagedType.Interface)]
        //public object psi;    // The namespace location of the the item that should be added to the recent docs folder.
        //public IntPtr psi;
        public Microsoft.SDK.Samples.VistaBridge.Interop.IShellItem psi;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszAppID;  // The id of the application that should be associated with this recent doc.
    }

    //TODO: Test this as well, currently not tested
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct SHARDAPPIDINFOIDLIST
    {
        public IntPtr pidl;                                        // The idlist for the shell item that should be added to the recent docs folder.
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszAppID;  // The id of the application that should be associated with this recent doc.
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct SHARDAPPIDINFOLINK
    {
        //public IntPtr psl;     // An IShellLink instance that when launched opens a recently used item in the specified 
        //// application. This link is not added to the recent docs folder, but will be added to the
        //// specified application's destination list.
        public IShellLinkW psl;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszAppID;  // The id of the application that should be associated with this recent doc.
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        //Obviously, these GUIDs shouldn't be modified.  The reason they
        //are not readonly is that they are passed with 'ref' to various
        //native methods.
        public static Guid IID_IObjectArray = new Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9");
        public static Guid IID_IObjectCollection = new Guid("5632B1A4-E38A-400A-928A-D4CD63230295");
        public static Guid IID_IPropertyStore = new Guid(VistaBridgeInterop.IIDGuid.IPropertyStore);
        public static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        public const int DWM_SIT_DISPLAYFRAME = 0x00000001;

        public const int DWMWA_FORCE_ICONIC_REPRESENTATION = 7;
        public const int DWMWA_HAS_ICONIC_BITMAP = 10;
        //TODO: DISALLOW_PEEK and FLIP3D_POLICY etc. sound interesting too

        public const int WM_COMMAND = 0x111;
        public const int WM_SYSCOMMAND = 0x112;
        public const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;
        public const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;
        public const int WM_CLOSE = 0x0010;
        public const int WM_ACTIVATE = 0x0006;

        public const int WA_ACTIVE = 1;
        public const int WA_CLICKACTIVE = 2;

        public const int SC_CLOSE = 0xF060;

        public const int MSGFLT_ADD = 1;
        public const int MSGFLT_REMOVE = 2;

        // Thumbbutton WM_COMMAND notification
        public const uint THBN_CLICKED = 0x1800;


        #region Shell Library

        internal enum LIBRARYFOLDERFILTER
        {
            LFF_FORCEFILESYSTEM = 1,
            LFF_STORAGEITEMS = 2,
            LFF_ALLITEMS = 3
        };

        internal enum LIBRARYOPTIONFLAGS
        {
            LOF_DEFAULT = 0,
            LOF_PINNEDTONAVPANE = 0x1,
            LOF_MASK_ALL = 0x1
        };

        internal enum DEFAULTSAVEFOLDERTYPE
        {
            DSFT_DETECT = 1,
            DSFT_PRIVATE = (DSFT_DETECT + 1),
            DSFT_PUBLIC = (DSFT_PRIVATE + 1)
        };


        internal enum LIBRARYSAVEFLAGS
        {
            LSF_FAILIFTHERE = 0,
            LSF_OVERRIDEEXISTING = 0x1,
            LSF_MAKEUNIQUENAME = 0x2
        };

        internal enum LIBRARYMANAGEDIALOGOPTIONS
        {
            LMD_DEFAULT = 0,
            LMD_NOUNINDEXABLELOCATIONWARNING = 0x1
        };

        internal enum StorageInstantiationModes
        {
            STGM_DIRECT = 0x00000000,
            STGM_TRANSACTED = 0x00010000,
            STGM_SIMPLE = 0x08000000,
            STGM_READ = 0x00000000,
            STGM_WRITE = 0x00000001,
            STGM_READWRITE = 0x00000002,
            STGM_SHARE_DENY_NONE = 0x00000040,
            STGM_SHARE_DENY_READ = 0x00000030,
            STGM_SHARE_DENY_WRITE = 0x00000020,
            STGM_SHARE_EXCLUSIVE = 0x00000010,
            STGM_PRIORITY = 0x00040000,
            STGM_DELETEONRELEASE = 0x04000000,
            STGM_NOSCRATCH = 0x00100000,
            STGM_CREATE = 0x00001000,
            STGM_CONVERT = 0x00020000,
            STGM_FAILIFTHERE = 0x00000000,
            STGM_NOSNAPSHOT = 0x00200000,
            STGM_DIRECT_SWMR = 0x00400000
        };
        #endregion
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("shell32.dll")]
        public static extern int SHGetPropertyStoreForWindow(
            IntPtr hwnd,
            ref Guid iid /*IID_IPropertyStore*/,
            [Out(), MarshalAs(UnmanagedType.Interface)]
                out IPropertyStore propertyStore);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetIconicThumbnail(
            IntPtr hwnd, IntPtr hbitmap, uint flags);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetIconicLivePreviewBitmap(
            IntPtr hwnd,
            IntPtr hbitmap,
            ref VistaBridgeInterop.SafeNativeMethods.POINT ptClient,
            uint flags);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetIconicLivePreviewBitmap(
            IntPtr hwnd, IntPtr hbitmap, IntPtr ptClient, uint flags);

        [DllImport("dwmapi.dll")]
        public static extern int DwmInvalidateIconicBitmaps(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeWindowMessageFilter(uint message, uint flags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint RegisterWindowMessage(string lpString);
        public static uint WM_TaskbarButtonCreated
        {
            get
            {
                if (_uTBBCMsg == 0)
                {
                    _uTBBCMsg = RegisterWindowMessage("TaskbarButtonCreated");
                }
                return _uTBBCMsg;
            }
        }

        private static uint _uTBBCMsg;

        [DllImport("shell32.dll")]
        public static extern void SetCurrentProcessExplicitAppUserModelID(
            [MarshalAs(UnmanagedType.LPWStr)] string AppID);
        [DllImport("shell32.dll")]
        public static extern void GetCurrentProcessExplicitAppUserModelID(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] out string AppID);

        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(SHARD flags, IntPtr pv);

        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(
            SHARD flags,
            [MarshalAs(UnmanagedType.LPWStr)] string path);
        public static void SHAddToRecentDocs(string path)
        {
            UnsafeNativeMethods.SHAddToRecentDocs(SHARD.SHARD_PATHW, path);
        }

        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(
            SHARD flags,
            ref SHARDAPPIDINFO appIDInfo);
        public static void SHAddToRecentDocs(ref SHARDAPPIDINFO appIDInfo)
        {
            UnsafeNativeMethods.SHAddToRecentDocs(SHARD.SHARD_APPIDINFO, ref appIDInfo);
        }

        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(
            SHARD flags,
            [MarshalAs(UnmanagedType.LPStruct)] ref SHARDAPPIDINFOIDLIST appIDInfoIDList);
        public static void SHAddToRecentDocs(ref SHARDAPPIDINFOIDLIST appIDInfoIDList)
        {
            UnsafeNativeMethods.SHAddToRecentDocs(SHARD.SHARD_APPIDINFOIDLIST, ref appIDInfoIDList);
        }

        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(
            SHARD flags,
            ref SHARDAPPIDINFOLINK appIDInfoLink);
        public static void SHAddToRecentDocs(ref SHARDAPPIDINFOLINK appIDInfoLink)
        {
            UnsafeNativeMethods.SHAddToRecentDocs(SHARD.SHARD_APPIDINFOLINK, ref appIDInfoLink);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hwnd, ref RECT rect);

        public static bool GetClientSize(IntPtr hwnd, out System.Drawing.Size size)
        {
            RECT rect = new RECT();
            if (!GetClientRect(hwnd, ref rect))
            {
                size = new System.Drawing.Size(-1, -1);
                return false;
            }
            size = new System.Drawing.Size(rect.right, rect.bottom);
            return true;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hwnd, int cmd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(
            IntPtr hwnd,
            ref VistaBridgeInterop.SafeNativeMethods.POINT point);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(
            IntPtr hwnd, StringBuilder str, int maxCount);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt (
            IntPtr hDestDC, int destX, int destY, int width, int height,
            IntPtr hSrcDC, int srcX, int srcY,
            uint operation);
        
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StretchBlt(
            IntPtr hDestDC, int destX, int destY, int destWidth, int destHeight,
            IntPtr hSrcDC, int srcX, int srcY, int srcWidth, int srcHeight,
            uint operation);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
    }

    internal static class NativeLibraryMethods
    {
        [DllImport("Shell32", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        internal static extern int SHShowManageLibraryUI(
                                        [In, MarshalAs(UnmanagedType.Interface)] Microsoft.SDK.Samples.VistaBridge.Interop.IShellItem library,
                                        [In] IntPtr hwndOwner,
                                        [In] string title,
                                        [In] string instruction,
                                        [In] SafeNativeMethods.LIBRARYMANAGEDIALOGOPTIONS lmdOptions);

    }

}