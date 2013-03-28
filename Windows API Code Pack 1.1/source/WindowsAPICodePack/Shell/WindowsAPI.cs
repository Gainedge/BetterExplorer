using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.Versioning;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using MS.WindowsAPICodePack.Internal;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;
using Microsoft.Win32;
using System.Security;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Controls;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Xml;

namespace WindowsHelper
{
    public class WindowsAPI
    {
        [DllImport("user32")]
        public static extern int ReleaseCapture();

        #region Enums
        public enum ShowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }

        public enum SIGDN : uint
        {

            NORMALDISPLAY = 0,
            PARENTRELATIVEPARSING = 0x80018001,
            PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000
        }

        /// <summary>
        /// Standart Window messages
        /// </summary>
        public enum WndMsg
        {
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_SETFOCUS = 0x0007,
            WM_KILLFOCUS = 0x0008,
            WM_SETVISIBLE = 0x0009,
            WM_ENABLE = 0x000A,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_CLOSE = 0x0010,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUIT = 0x0012,
            WM_QUERYOPEN = 0x0013,
            WM_ERASEBKGND = 0x0014,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_ENDSESSION = 0x0016,
            WM_SYSTEMERROR = 0x0017,
            WM_SHOWWINDOW = 0x0018,
            WM_CTLCOLOR = 0x0019,
            WM_WININICHANGE = 0x001A,
            WM_SETTINGCHANGE = 0x001A,
            WM_DEVMODECHANGE = 0x001B,
            WM_ACTIVATEAPP = 0x001C,
            WM_FONTCHANGE = 0x001D,
            WM_TIMECHANGE = 0x001E,
            WM_CANCELMODE = 0x001F,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,
            WM_QUEUESYNC = 0x0023,
            WM_GETMINMAXINFO = 0x0024,
            WM_PAINTICON = 0x0026,
            WM_ICONERASEBKGND = 0x0027,
            WM_NEXTDLGCTL = 0x0028,
            WM_ALTTABACTIVE = 0x0029,
            WM_SPOOLERSTATUS = 0x002A,
            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,
            WM_SETHOTKEY = 0x0032,
            WM_GETHOTKEY = 0x0033,
            WM_XBUTTONUP = 0x020C,
            //public const uint WM_FILESYSCHANGE        = 0x0034;
            //public const uint WM_ISACTIVEICON         = 0x0035;
            //public const uint WM_QUERYPARKICON        = 0x0036;
            WM_QUERYDRAGICON = 0x0037,
            WM_COMPAREITEM = 0x0039,
            //public const uint WM_TESTING              = 0x003a;
            //public const uint WM_OTHERWINDOWCREATED = 0x003c;
            WM_GETOBJECT = 0x003D,
            //public const uint WM_ACTIVATESHELLWINDOW        = 0x003e;
            WM_COMPACTING = 0x0041,
            WM_COMMNOTIFY = 0x0044,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_POWER = 0x0048,
            WM_COPYDATA = 0x004A,
            WM_CANCELJOURNAL = 0x004B,
            WM_NOTIFY = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_TCARD = 0x0052,
            WM_HELP = 0x0053,
            WM_USERCHANGED = 0x0054,
            WM_NOTIFYFORMAT = 0x0055,
            WM_CONTEXTMENU = 0x007B,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_DISPLAYCHANGE = 0x007E,
            WM_GETICON = 0x007F,

            // Non-Client messages
            WM_SETICON = 0x0080,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,
            WM_GETDLGCODE = 0x0087,
            WM_SYNCPAINT = 0x0088,
            //public const uint WM_SYNCTASK       = 0x0089;
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            //public const uint WM_NCXBUTTONDOWN    = 0x00ab;
            //public const uint WM_NCXBUTTONUP      = 0x00ac;
            //public const uint WM_NCXBUTTONDBLCLK  = 0x00ad;
            WM_KEYDOWN = 0x0100,
            WM_KEYFIRST = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,
            WM_KEYLAST = 0x0108,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_COMPOSITION = 0x010F,
            WM_IME_KEYLAST = 0x010F,
            WM_INITDIALOG = 0x0110,
            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            //public const uint WM_SYSTIMER       = 0x0118;
            WM_MENUSELECT = 0x011F,
            WM_MENUCHAR = 0x0120,
            WM_ENTERIDLE = 0x0121,
            WM_MENURBUTTONUP = 0x0122,
            WM_MENUDRAG = 0x0123,
            WM_MENUGETOBJECT = 0x0124,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_MENUCOMMAND = 0x0126,
            //public const uint WM_CHANGEUISTATE    = 0x0127;
            //public const uint WM_UPDATEUISTATE    = 0x0128;
            //public const uint WM_QUERYUISTATE     = 0x0129;
            //
            //public const uint WM_LBTRACKPOINT     = 0x0131;
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEFIRST = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MOUSEWHEEL = 0x020A,
            WM_MOUSELAST = 0x020D,
            //public const uint WM_XBUTTONDOWN      = 0x020B;
            //public const uint WM_XBUTTONUP        = 0x020C;
            //public const uint WM_XBUTTONDBLCLK    = 0x020D;
            WM_PARENTNOTIFY = 0x0210,
            WM_ENTERMENULOOP = 0x0211,
            WM_EXITMENULOOP = 0x0212,
            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,
            WM_POWERBROADCAST   = 0x0218,
            WM_DEVICECHANGE = 0x0219,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIACTIVATE = 0x0222,
            WM_MDIRESTORE = 0x0223,
            WM_MDINEXT = 0x0224,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDITILE = 0x0226,
            WM_MDICASCADE = 0x0227,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIGETACTIVE = 0x0229,
            /* D&D messages */
            //public const uint WM_DROPOBJECT     = 0x022A;
            //public const uint WM_QUERYDROPOBJECT  = 0x022B;
            //public const uint WM_BEGINDRAG      = 0x022C;
            //public const uint WM_DRAGLOOP       = 0x022D;
            //public const uint WM_DRAGSELECT     = 0x022E;
            //public const uint WM_DRAGMOVE       = 0x022F;
            WM_MDISETMENU = 0x0230,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_DROPFILES = 0x0233,
            WM_MDIREFRESHMENU = 0x0234,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_AFXFIRST = 0x0360,
            WM_AFXLAST = 0x037F,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,
            WM_APP = 0x8000,
            WM_USER = 0x0400,

            // Our "private" ones
            WM_MOUSE_ENTER = 0x0401,
            WM_MOUSE_LEAVE = 0x0402,
            WM_ASYNC_MESSAGE = 0x0403,
            WM_REFLECT = WM_USER + 0x1c00
        }

        /// <summary>
        /// Button pressed states
        /// </summary>
        public enum MK
        {
            MK_CONTROL = 0x0008,
            MK_SHIFT = 0x0004,
            /// <summary>
            /// Left mouse button is clicked.
            /// </summary>
            MK_LBUTTON = 0x0001,
            MK_RBUTTON = 0x0002,
            MK_MBUTTON = 0x0010,
            MK_ALT= 0x0020
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
            LVM_FIRST = 0x1000,
            LVM_GETHEADER = (LVM_FIRST + 31),
            HDM_GETITEMCOUNT = 0x1200,
            HDM_GETORDERARRAY = 0x1200 + 17,
            HDM_GETITEM = 0x1200 + 11,
            LVM_INSERTCOLUMN = (LVM_FIRST + 27),
            LVM_GETCOLUMN = (LVM_FIRST + 25),
            LVM_DELETECOLUMN = (LVM_FIRST + 28),
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WM_CHAR = 0x105,
            WM_SYSKEYDOWN = 0x104,
            WM_SYSKEYUP = 0x105,
            LVM_SETITEMSTATE = LVM_FIRST + 43


        }

        public enum SVSIF : uint
        {
            SVSI_DESELECT = 0x00000000,
            SVSI_SELECT = 0x00000001,
            SVSI_EDIT = 0x00000003,
            SVSI_DESELECTOTHERS = 0x00000004,
            SVSI_ENSUREVISIBLE = 0x00000008,
            SVSI_FOCUSED = 0x00000010,
            SVSI_TRANSLATEPT = 0x00000020,
            SVSI_SELECTIONMARK = 0x00000040,
            SVSI_POSITIONITEM = 0x00000080,
            SVSI_CHECK = 0x00000100,
            SVSI_CHECK2 = 0x00000200,
            SVSI_KEYBOARDSELECT = 0x00000401,
            SVSI_NOTAKEFOCUS = 0x40000000
        };
        #endregion


        #region SendMessage
        [DllImport("User32.dll")]
        public static extern uint RegisterWindowMessage(string lpString);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        //For use with WM_COPYDATA and COPYDATASTRUCT
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);


        //For use with WM_COPYDATA and COPYDATASTRUCT
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);


        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(int hWnd, int Msg, int wParam, int lParam);


        public const int WM_USER = 0x400;
        public const int WM_COPYDATA = 0x4A;
       

        //Used for WM_COPYDATA for string messages
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ShareInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpUserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpDomain;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 999)]
            public string lpShare;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 999)]
            public string lpSharingName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpDescription;
            public int IsSetPermisions;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpMsg;

        }

        public bool bringAppToFront(IntPtr hWnd)
        {
            return (SetForegroundWindow(hWnd) != 0);
        }

        public static int sendWindowsStringMessage(int hWnd, int wParam, string msg, 
            string Share = "", string ShareName = "", string Description = "" ,string Domain = "", string User = "", int Permis = 0x0)
        {
            int result = 0;

            if (hWnd != 0)
            {
                ShareInfo shi = new ShareInfo();
                shi.IsSetPermisions = Permis;
                shi.lpDescription = Description;
                shi.lpSharingName = ShareName;
                shi.lpUserName = User;
                shi.lpShare = Share;
                shi.lpDomain = Domain;
                shi.lpMsg = msg;

                IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(shi));
                Marshal.StructureToPtr(shi, p, false);
                string str = msg + ";" + Share + ";" + ShareName + ";" + Domain +
                    ";" + User + ";" + Description;
                byte[] sarr = System.Text.Encoding.Default.GetBytes(str);
                int len = Marshal.SizeOf(shi);//sarr.Length;
                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.dwData = IntPtr.Zero;
                cds.lpData = p;
                cds.cbData = len;
                int res = SendMessage(hWnd, WM_COPYDATA, wParam, ref cds);
                Marshal.FreeCoTaskMem(p);
                result = res;//SendMessage(hWnd, WM_COPYDATA, wParam, ref cds);
            }

            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SymLinkInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpDestination;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpTarget;
            public int SymType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpMsg;

        }

        public static int sendWindowsMessage(int hWnd, int Msg, int wParam, int lParam)
        {
            int result = 0;

            if (hWnd != 0)
            {
                result = SendMessage(hWnd, Msg, wParam, lParam);
            }

            return result;
        }

        public static IntPtr getWindowId(string className, string windowName)
        {

            return FindWindow(className, windowName);

        } 
        #endregion


        #region FolderViewOptions
        /// <summary>
        /// Exposes methods that allow control of folder view options specific to the Windows 7 and later views.
        ///Members
        ///The IFolderViewOptions interface inherits from the IUnknown interface. IFolderViewOptions also has the following types of members:
        ///<b><i>Remarks</i></b>
        ///<para><b>When to Implement:</b>
        ///An implementation of this interface is provided with Windows as part of CLSID_ExplorerBrowser and CLSID_ShellBrowser. Third parties do not implement this interface.</para>
        ///<para><b>When to Use:</b>
        ///By default, the Windows 7 item view does not support custom positioning, custom ordering, or hyperlinks, which were supported in the Windows Vista list view. Use this interface when you require those features of the older view. If, at some later time, the item view adds support for those features, these options will automatically use the newer view rather than continuing to revert to the older view as they currently do.
        ///Use this interface to turn off animation and scroll tip view options new to Windows 7.
        ///Use this interface to retrieve the current view settings for all of those options.</para>
        /// </summary>
        [ComImport, SuppressUnmanagedCodeSecurity, Guid("3cc974d2-b302-4d36-ad3e-06d93f695d3f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IFolderViewOptions
        {
            /// <summary>
            /// Sets specified options for the view.
            /// </summary>
            /// <param name="fvoMask">Type: FOLDERVIEWOPTIONS
            /// A bitmask made up of one or more of the FOLDERVIEWOPTIONS flags to indicate which options' are being changed. Values in fvoFlags not included in this mask are ignored.
            /// </param>
            /// <param name="fvoFlags">Type: FOLDERVIEWOPTIONS
            /// A bitmask that contains the new values for the options specified in fvoMask. To enable an option, the bitmask should include the FOLDERVIEWOPTIONS flag for that option. To disable an option, the bit used for that FOLDERVIEWOPTIONS flag should be 0.</param>
            /// <returns>If this method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
            [PreserveSig]
            HResult SetFolderViewOptions(int fvoMask, int fvoFlags);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pfvoFlags"></param>
            /// <returns></returns>
            [PreserveSig]
            HResult GetFolderViewOptions(out int pfvoFlags);
        }

        /// <summary>
        /// Used by methods of the IFolderViewOptions interface to activate Windows Vista options not supported by default in Windows 7 and later systems as well as deactivating new Windows 7 options.
        /// </summary>
        public static class FOLDERVIEWOPTIONS
        {
            public const int DEFAULT = 0x00;
            public const int VISTALAYOUT = 0x01;
            public const int CUSTOMPOSITION = 0x02;
            public const int CUSTOMORDERING = 0x04;
            public const int SUPPORTHYPERLINKS = 0x08;
            public const int NOANIMATIONS = 0x10;
            public const int NOSCROLLTIPS = 0x20;
        }
        #endregion

        /// <summary>
        /// Returns the IShellFolder interface from IShellItem
        /// </summary>
        /// <param name="sitem">The IShellItem represented like ShellObject</param>
        /// <returns></returns>
        public static IShellFolder GetIShellFolder(ShellObject sitem) {
          IShellFolder result;
          ((IShellItem2)sitem.nativeShellItem).BindToHandler(IntPtr.Zero,
              BHID.SFObject, typeof(IShellFolder).GUID, out result);
          return result;
        }


        public static IntPtr SendStringMessage(IntPtr hWnd, byte[] array, int startIndex, int length)
        {
            IntPtr ptr = Marshal.AllocHGlobal(IntPtr.Size * 3 + length);
            Marshal.WriteIntPtr(ptr, 0, IntPtr.Zero);
            Marshal.WriteIntPtr(ptr, IntPtr.Size, (IntPtr)length);
            IntPtr dataPtr = new IntPtr(ptr.ToInt64() + IntPtr.Size * 3);
            Marshal.WriteIntPtr(ptr, IntPtr.Size * 2, dataPtr);
            Marshal.Copy(array, startIndex, dataPtr, length);
            IntPtr result = SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ptr);
            Marshal.FreeHGlobal(ptr);
            return result;
        }


        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern IShellItem SHCreateItemWithParent(
            [In] IntPtr pidlParent,
            [In] IShellFolder psfParent,
            [In] IntPtr pidl,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        /// <summary>
        /// Returns IShellItem from parent item and pidl
        /// </summary>
        /// <param name="parent">The parent folder</param>
        /// <param name="pidl">the PIDL of the item</param>
        /// <returns></returns>
        public static IShellItem CreateItemWithParent(IShellFolder parent, IntPtr pidl) {

          return SHCreateItemWithParent(IntPtr.Zero,
                                        parent, pidl, typeof(IShellItem).GUID);
        }


        public static ShellObject[] ParseShellIDListArray(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj) {
          List<ShellObject> result = new List<ShellObject>();
          FORMATETC format = new FORMATETC();
          STGMEDIUM medium = new STGMEDIUM();

          format.cfFormat = (short)RegisterClipboardFormat("Shell IDList Array");
          format.dwAspect = DVASPECT.DVASPECT_CONTENT;
          format.lindex = 0;
          format.ptd = IntPtr.Zero;
          format.tymed = TYMED.TYMED_HGLOBAL;

          pDataObj.GetData(ref format, out medium);
          GlobalLock(medium.unionmember);
          ShellObject parentFolder = null;
          try {

            int count = Marshal.ReadInt32(medium.unionmember);
            int offset = sizeof(UInt32);

            for (int n = 0; n <= count; ++n) {
              int pidlOffset = Marshal.ReadInt32(medium.unionmember, offset);
              int pidlAddress = (int)medium.unionmember + pidlOffset;

              if (n == 0) {
                parentFolder = ShellObjectFactory.Create(new IntPtr(pidlAddress));
              } else {
                result.Add(ShellObjectFactory.Create(WindowsAPI.CreateItemWithParent(WindowsAPI.GetIShellFolder(parentFolder.NativeShellItem), new IntPtr(pidlAddress))));
              }

              offset += 4;
            }
          } finally {
            //Marshal.FreeCoTaskMem(medium.unionmember);// FreeHGlobal(medium.unionmember);
          }

          return result.ToArray();
        }

        /// <summary>
        /// Converts an item identifier list to a file system path. (Note: SHGetPathFromIDList calls the ANSI version, must call SHGetPathFromIDListW for .NET)
        /// </summary>
        /// <param name="pidl">Address of an item identifier list that specifies a file or directory location relative to the root of the namespace (the desktop).</param>
        /// <param name="pszPath">Address of a buffer to receive the file system path. This buffer must be at least MAX_PATH characters in size.</param>
        /// <returns>Returns TRUE if successful, or FALSE otherwise. </returns>
        [DllImport("shell32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder pszPath);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void SHCreateShellItemArrayFromDataObject(
            [In] System.Runtime.InteropServices.ComTypes.IDataObject pdo,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.LPArray)] IShellItemArray ppv);

        public enum SIATTRIBFLAGS {
          SIATTRIBFLAGS_AND = 1,
          SIATTRIBFLAGS_APPCOMPAT = 3,
          SIATTRIBFLAGS_OR = 2
        }
        [ComImport, Guid("B63EA76D-1F85-456F-A19C-48159EFA858B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItemArray {
          // Not supported: IBindCtx
          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid rbhid,
                  [In] ref Guid riid, out IntPtr ppvOut);

          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          void GetPropertyStore([In] int Flags, [In] ref Guid riid, out IntPtr ppv);

          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          void GetPropertyDescriptionList([In] ref PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv);

          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          void GetAttributes([In] SIATTRIBFLAGS dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs);

          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          void GetCount(out uint pdwNumItems);

          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          void GetItemAt([In] uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

          // Not supported: IEnumShellItems (will use GetCount and GetItemAt instead)
          [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
          void EnumItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenumShellItems);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteFile(string lpFileName);

        /// <summary>
        /// Returns the IShellFolder interface from IShellItem
        /// </summary>
        /// <param name="sitem">The IShellItem</param>
        /// <returns></returns>
        public static IShellFolder GetIShellFolder(IShellItem sitem) {
          IShellFolder result;
          ((IShellItem2)sitem).BindToHandler(IntPtr.Zero,
              BHID.SFObject, typeof(IShellFolder).GUID, out result);
          return result;
        }

        [DllImport("user32.dll")]
        public static extern uint RegisterClipboardFormat(string lpszFormat);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("shell32.dll", PreserveSig = false)]
        public static extern void SHCreateItemFromIDList(
            [In] IntPtr pidl,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItem ppv);

        [DllImport("shell32.dll")]
        public static extern int SHGetIDListFromObject([MarshalAs(UnmanagedType.IUnknown)] object punk, out IntPtr ppidl);

        [DllImport("wininet.dll")]
        public static extern bool InternetGetConnectedState(int lpSFlags, int dwReserved);

        public static bool IsThis64bitProcess() {
          return (IntPtr.Size == 8);
        }

        public static bool Is64bitProcess(Process proc) {
          return !Is32bitProcess(proc);
        }

        public static bool Is32bitProcess(Process proc) {
          if (!IsThis64bitProcess()) return true; // we're in 32bit mode, so all are 32bit

          foreach (ProcessModule module in proc.Modules) {
            try {
              string fname = Path.GetFileName(module.FileName).ToLowerInvariant();
              if (fname.Contains("wow64")) {
                return true;
              }
            } catch {
              // wtf
            }
          }

          return false;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR {
          // 12/24
          public IntPtr hwndFrom;
          public IntPtr idFrom;
          public int code;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, ChangeWindowMessageFilterExAction action, ref CHANGEFILTERSTRUCT changeInfo);

        public enum MessageFilterInfo : uint {
          None = 0, AlreadyAllowed = 1, AlreadyDisAllowed = 2, AllowedHigher = 3
        };

        public enum ChangeWindowMessageFilterExAction : uint {
          Reset = 0, Allow = 1, DisAllow = 2
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CHANGEFILTERSTRUCT {
          public uint size;
          public MessageFilterInfo info;
        }

        public static T PtrToStructure<T>(IntPtr p) {
          return (T)Marshal.PtrToStructure(p, typeof(T));
        }

        [DllImport("kernel32.dll",SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SYMBOLIC_LINK_FLAG dwFlags);

        public enum SYMBOLIC_LINK_FLAG
        {
            File = 0,
            Directory = 1
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr pv);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("00000122-0000-0000-C000-000000000046")]
        public interface IDropTarget
        {
            void DragEnter(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj, int grfKeyState,
                           Point pt, ref int pdwEffect);
            void DragOver(int grfKeyState, Point pt, ref int pdwEffect);
            void DragLeave();
            void Drop(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj, int grfKeyState,
                     Point pt, ref int pdwEffect);
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct LVITEM
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
        }

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(
                long fileSize
                , [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer
                , int bufferSize);


        /// <summary>
        /// Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
        /// </summary>
        /// <param name="filelength">The numeric value to be converted.</param>
        /// <returns>the converted string</returns>
        public static string StrFormatByteSize(long filesize) {
          StringBuilder sb = new StringBuilder(11);
          StrFormatByteSize(filesize, sb, sb.Capacity);
          return sb.ToString();
        }

        #region enum HChangeNotifyEventID
        /// <summary>
        /// Describes the event that has occurred.
        /// Typically, only one event is specified at a time.
        /// If more than one event is specified, the values contained
        /// in the <i>dwItem1</i> and <i>dwItem2</i>
        /// parameters must be the same, respectively, for all specified events.
        /// This parameter can be one or more of the following values.
        /// </summary>
        /// <remarks>
        /// <para><b>Windows NT/2000/XP:</b> <i>dwItem2</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem1</i> is not used and should be <see langword="null"/>.</para>
        /// <para><b>Windows 95/98:</b> <i>dwItem1</i> contains the index
        /// in the system image list that has changed.
        /// <i>dwItem2</i> is not used and should be <see langword="null"/>.</para>
        /// </remarks>

        [Flags]
        public enum HChangeNotifyEventID
        {
            /// <summary>
            /// All events have occurred.
            /// </summary>
            SHCNE_ALLEVENTS = 0x7FFFFFFF,

            /// <summary>
            /// A file type association has changed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// must be specified in the <i>uFlags</i> parameter.
            /// <i>dwItem1</i> and <i>dwItem2</i> are not used and must be <see langword="null"/>.
            /// </summary>
            SHCNE_ASSOCCHANGED = 0x08000000,

            /// <summary>
            /// The attributes of an item or folder have changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item or folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_ATTRIBUTES = 0x00000800,

            /// <summary>
            /// A nonfolder item has been created.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_CREATE = 0x00000002,

            /// <summary>
            /// A nonfolder item has been deleted.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the item that was deleted.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DELETE = 0x00000004,

            /// <summary>
            /// A drive has been added.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADD = 0x00000100,

            /// <summary>
            /// A drive has been added and the Shell should create a new window for the drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was added.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEADDGUI = 0x00010000,

            /// <summary>
            /// A drive has been removed. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_DRIVEREMOVED = 0x00000080,

            /// <summary>
            /// Not currently used.
            /// </summary>
            SHCNE_EXTENDED_EVENT = 0x04000000,

            /// <summary>
            /// The amount of free space on a drive has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive on which the free space changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_FREESPACE = 0x00040000,

            /// <summary>
            /// Storage media has been inserted into a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive that contains the new media.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAINSERTED = 0x00000020,

            /// <summary>
            /// Storage media has been removed from a drive.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the root of the drive from which the media was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MEDIAREMOVED = 0x00000040,

            /// <summary>
            /// A folder has been created. <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/>
            /// or <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was created.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_MKDIR = 0x00000008,

            /// <summary>
            /// A folder on the local computer is being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETSHARE = 0x00000200,

            /// <summary>
            /// A folder on the local computer is no longer being shared via the network.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that is no longer being shared.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_NETUNSHARE = 0x00000400,

            /// <summary>
            /// The name of a folder has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous pointer to an item identifier list (PIDL) or name of the folder.
            /// <i>dwItem2</i> contains the new PIDL or name of the folder.
            /// </summary>
            SHCNE_RENAMEFOLDER = 0x00020000,

            /// <summary>
            /// The name of a nonfolder item has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the previous PIDL or name of the item.
            /// <i>dwItem2</i> contains the new PIDL or name of the item.
            /// </summary>
            SHCNE_RENAMEITEM = 0x00000001,

            /// <summary>
            /// A folder has been removed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that was removed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_RMDIR = 0x00000010,

            /// <summary>
            /// The computer has disconnected from a server.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the server from which the computer was disconnected.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// </summary>
            SHCNE_SERVERDISCONNECT = 0x00004000,

            /// <summary>
            /// The contents of an existing folder have changed,
            /// but the folder still exists and has not been renamed.
            /// <see cref="HChangeNotifyFlags.SHCNF_IDLIST"/> or
            /// <see cref="HChangeNotifyFlags.SHCNF_PATH"/> must be specified in <i>uFlags</i>.
            /// <i>dwItem1</i> contains the folder that has changed.
            /// <i>dwItem2</i> is not used and should be <see langword="null"/>.
            /// If a folder has been created, deleted, or renamed, use SHCNE_MKDIR, SHCNE_RMDIR, or
            /// SHCNE_RENAMEFOLDER, respectively, instead.
            /// </summary>
            SHCNE_UPDATEDIR = 0x00001000,

            /// <summary>
            /// An image in the system image list has changed.
            /// <see cref="HChangeNotifyFlags.SHCNF_DWORD"/> must be specified in <i>uFlags</i>.
            /// </summary>
            SHCNE_UPDATEIMAGE = 0x00008000,

            SHCNE_UPDATEITEM = 0x00002000,


        }
        #endregion // enum HChangeNotifyEventID

        #region public enum HChangeNotifyFlags
        /// <summary>
        /// Flags that indicate the meaning of the <i>dwItem1</i> and <i>dwItem2</i> parameters.
        /// The uFlags parameter must be one of the following values.
        /// </summary>
        [Flags]
        public enum HChangeNotifyFlags
        {
            /// <summary>
            /// The <i>dwItem1</i> and <i>dwItem2</i> parameters are DWORD values.
            /// </summary>
            SHCNF_DWORD = 0x0003,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of ITEMIDLIST structures that
            /// represent the item(s) affected by the change.
            /// Each ITEMIDLIST must be relative to the desktop folder.
            /// </summary>
            SHCNF_IDLIST = 0x0000,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHA = 0x0001,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings of
            /// maximum length MAX_PATH that contain the full path names
            /// of the items affected by the change.
            /// </summary>
            SHCNF_PATHW = 0x0005,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERA = 0x0002,
            /// <summary>
            /// <i>dwItem1</i> and <i>dwItem2</i> are the addresses of null-terminated strings that
            /// represent the friendly names of the printer(s) affected by the change.
            /// </summary>
            SHCNF_PRINTERW = 0x0006,
            /// <summary>
            /// The function should not return until the notification
            /// has been delivered to all affected components.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSH = 0x1000,
            /// <summary>
            /// The function should begin delivering notifications to all affected components
            /// but should return as soon as the notification process has begun.
            /// As this flag modifies other data-type flags, it cannot by used by itself.
            /// </summary>
            SHCNF_FLUSHNOWAIT = 0x2000
        }
        #endregion // enum HChangeNotifyFlags


        /// <summary>
        /// Notifies the system of an event that an application has performed. 
        /// An application should use this function if it performs an action that may affect the Shell.
        /// </summary>
        /// <param name="wEventId">Describes the event that has occurred. Typically, only one event is specified at a time. 
        /// If more than one event is specified, the values contained in the dwItem1 and dwItem2 parameters must be the same, respectively, for all specified events. This parameter can be one or more of the following values.</param>
        /// <param name="uFlags">Flags that, when combined bitwise with SHCNF_TYPE, indicate the meaning of the dwItem1 and dwItem2 parameters.</param>
        /// <param name="dwItem1">Optional. First event-dependent value.</param>
        /// <param name="dwItem2">Optional. Second event-dependent value.</param>
        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(HChangeNotifyEventID wEventId,
                                           HChangeNotifyFlags uFlags,
                                           IntPtr dwItem1,
                                           IntPtr dwItem2);

        

        [DllImport("mpr.dll", EntryPoint = "WNetConnectionDialog", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int WNetConnectionDialog(IntPtr whnd, int dwType);

        /// <summary>
        /// Map Network Drive dialog
        /// </summary>
        /// <param name="whnd"></param>
        /// <param name="dwType"></param>
        /// <returns></returns>
        [DllImport("mpr.dll", EntryPoint = "WNetDisconnectDialog", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int WNetDisconnectDialog(IntPtr whnd, int dwType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT pInputs, int cbSize);
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            
            int dx;
            int dy;
            uint mouseData;
            uint dwFlags;
            uint time;
            IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            uint uMsg;
            ushort wParamL;
            ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)] //[FieldOffset(8)] for x64
            public MOUSEINPUT mi;
            [FieldOffset(4)] //[FieldOffset(8)] for x64
            public KEYBDINPUT ki;
            [FieldOffset(4)] //[FieldOffset(8)] for x64
            public HARDWAREINPUT hi;
        }

        [Flags]
        public enum SHGFI : uint
        {
            /// <summary>get icon</summary>
            Icon = 0x000000100,
            /// <summary>get display name</summary>
            DisplayName = 0x000000200,
            /// <summary>get type name</summary>
            TypeName = 0x000000400,
            /// <summary>get attributes</summary>
            Attributes = 0x000000800,
            /// <summary>get icon location</summary>
            IconLocation = 0x000001000,
            /// <summary>return exe type</summary>
            ExeType = 0x000002000,
            /// <summary>get system icon index</summary>
            SysIconIndex = 0x000004000,
            /// <summary>put a link overlay on icon</summary>
            LinkOverlay = 0x000008000,
            /// <summary>show icon in selected state</summary>
            Selected = 0x000010000,
            /// <summary>get only specified attributes</summary>
            Attr_Specified = 0x000020000,
            /// <summary>get large icon</summary>
            LargeIcon = 0x000000000,
            /// <summary>get small icon</summary>
            SmallIcon = 0x000000001,
            /// <summary>get open icon</summary>
            OpenIcon = 0x000000002,
            /// <summary>get shell size icon</summary>
            ShellIconSize = 0x000000004,
            /// <summary>pszPath is a pidl</summary>
            PIDL = 0x000000008,
            /// <summary>use passed dwFileAttribute</summary>
            UseFileAttributes = 0x000000010,
            /// <summary>apply the appropriate overlays</summary>
            AddOverlays = 0x000000020,
            /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
            OverlayIndex = 0x000000040,
        }

        [Flags]
        internal enum CLSCTX {
          CLSCTX_INPROC_SERVER = 0x1,
          CLSCTX_INPROC_HANDLER = 0x2,
          CLSCTX_LOCAL_SERVER = 0x4,
          CLSCTX_REMOTE_SERVER = 0x10,
          CLSCTX_NO_CODE_DOWNLOAD = 0x400,
          CLSCTX_NO_CUSTOM_MARSHAL = 0x1000,
          CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,
          CLSCTX_NO_FAILURE_LOG = 0x4000,
          CLSCTX_DISABLE_AAA = 0x8000,
          CLSCTX_ENABLE_AAA = 0x10000,
          CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,
          CLSCTX_INPROC = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER,
          CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
          CLSCTX_ALL = CLSCTX_SERVER | CLSCTX_INPROC_HANDLER
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BIND_OPTS3 {
          internal uint cbStruct;
          internal uint grfFlags;
          internal uint grfMode;
          internal uint dwTickCountDeadline;
          internal uint dwTrackFlags;
          internal uint dwClassContext;
          internal uint locale;
          object pServerInfo; // will be passing null, so type doesn't matter
          internal IntPtr hwnd;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();

        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        internal static extern object CoGetObject(
           string pszName,
           [In] ref BIND_OPTS3 pBindOptions,
           [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

        [return: MarshalAs(UnmanagedType.Interface)]
        public static object LaunchElevatedCOMObject(Guid Clsid, Guid InterfaceID) {
          string CLSID = Clsid.ToString("B"); // B formatting directive: returns {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx} 
          string monikerName = "Elevation:Administrator!new:" + CLSID;

          BIND_OPTS3 bo = new BIND_OPTS3();
          bo.cbStruct = (uint)Marshal.SizeOf(bo);
          bo.hwnd = IntPtr.Zero;
          bo.dwClassContext = (int)CLSCTX.CLSCTX_LOCAL_SERVER;

          object retVal = CoGetObject(monikerName, ref bo, InterfaceID);

          return (retVal);
        }

        //[ComImport]
        //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        //[Guid("000214e4-0000-0000-c000-000000000046")]
        //public interface IContextMenu
        //{
            
        //    [PreserveSig]
        //    HResult QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst,
        //                             int idCmdLast, Microsoft.WindowsAPICodePack.Shell.ShellContextMenu.CMF uFlags);

        //    void InvokeCommand(ref Microsoft.WindowsAPICodePack.Shell.ShellContextMenu.CMINVOKECOMMANDINFO pici);

        //    [PreserveSig()]
        //    Int32 GetCommandString(
        //        uint idcmd,
        //        GCS uflags,
        //        uint reserved,
        //        [MarshalAs(UnmanagedType.LPArray)]
        //    byte[] commandstring,
        //        int cch);
        //}

        //[Flags]
        //public enum GCS : uint
        //{
        //    VERBA = 0,
        //    HELPTEXTA = 1,
        //    VALIDATEA = 2,
        //    VERBW = 4,
        //    HELPTEXTW = 5,
        //    VALIDATEW = 6
        //}

        //[ComImport]
        //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        //[Guid("000214f4-0000-0000-c000-000000000046")]
        //public interface IContextMenu2 : IContextMenu
        //{
        //    [PreserveSig]
        //    new HResult QueryContextMenu(IntPtr hMenu, uint indexMenu,
        //        int idCmdFirst, int idCmdLast,
        //        Microsoft.WindowsAPICodePack.Shell.ShellContextMenu.CMF uFlags);

        //    void InvokeCommand(ref Microsoft.WindowsAPICodePack.Shell.ShellContextMenu.CMINVOKECOMMANDINFO_ByIndex pici);

        //    [PreserveSig]
        //    new HResult GetCommandString(int idcmd, uint uflags, int reserved,
        //        [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
        //        int cch);

        //    [PreserveSig]
        //    HResult HandleMenuMsg(int uMsg, IntPtr wParam, IntPtr lParam);
        //}

        //[ComImport]
        //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        //[Guid("bcfce0a0-ec17-11d0-8d10-00a0c90f2719")]
        //public interface IContextMenu3 : IContextMenu2
        //{
        //    [PreserveSig]
        //    new HResult QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst,
        //                         int idCmdLast, Microsoft.WindowsAPICodePack.Shell.ShellContextMenu.CMF uFlags);

        //    [PreserveSig]
        //    new HResult InvokeCommand(ref Microsoft.WindowsAPICodePack.Shell.ShellContextMenu.CMINVOKECOMMANDINFO pici);

        //    [PreserveSig]
        //    new HResult GetCommandString(int idcmd, uint uflags, int reserved,
        //        [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
        //        int cch);

        //    [PreserveSig]
        //    new HResult HandleMenuMsg(int uMsg, IntPtr wParam, IntPtr lParam);

        //    [PreserveSig]
        //    HResult HandleMenuMsg2(int uMsg, IntPtr wParam, IntPtr lParam,
        //        out IntPtr plResult);
        //}

        public enum SVGIO {
          SVGIO_BACKGROUND       = 0x00000000,
          SVGIO_SELECTION        = 0x00000001,
          SVGIO_ALLVIEW          = 0x00000002,
          SVGIO_CHECKED          = 0x00000003,
          SVGIO_TYPE_MASK        = 0x0000000F,
        }


        public static string GetAssoc(string pszAssoc  , AssocF flags, AssocStr str)
        {
            uint pcchOut = 0;
            AssocQueryString(flags, str, pszAssoc, null, null, ref pcchOut);
            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            AssocQueryString(flags, str, pszAssoc, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        }

        /// <summary>
        /// The GetDriveType function determines whether a disk drive is a removable, fixed, CD-ROM, RAM disk, or network drive
        /// </summary>
        /// <param name="lpRootPathName">A pointer to a null-terminated string that specifies the root directory and returns information about the disk.A trailing backslash is required. If this parameter is NULL, the function uses the root of the current directory.</param>
        [DllImport("kernel32.dll")]
        public static extern DriveType GetDriveType([MarshalAs(UnmanagedType.LPStr)] string lpRootPathName);

        private const int LVM_FIRST = 0x1000;

        private const int LVM_SETBKIMAGE = (LVM_FIRST + 138);
        private const int LVM_GETBKIMAGE = (LVM_FIRST + 139);

        public enum LVBKIF : int
        {

            SOURCE_NONE = 0x00000000,

            SOURCE_HBITMAP = 0x00000001,

            SOURCE_URL = 0x00000002,

            SOURCE_MASK = 0x00000003,

            STYLE_NORMAL = 0x00000000,

            STYLE_TILE = 0x00000010,

            STYLE_MASK = 0x00000010,

            FLAG_TILEOFFSET = 0x00000100,

            STYLE_WATERMARK = 0x10000000,

            FLAG_ALPHABLEND = 0x20000000

        }

        public struct LVBKIMAGE
        {

            public LVBKIF ulFlags;

            public IntPtr hbm;

            public IntPtr pszImage;

            public uint cchImageMax;

            public int xOffsetPercent;

            public int yOffsetPercent;

        };

        public static void SetListViewBackgroundImage(IntPtr lvHandle, Bitmap bitmap)
        {

            LVBKIMAGE lvBkImage = new LVBKIMAGE();

            lvBkImage.ulFlags = LVBKIF.STYLE_WATERMARK | LVBKIF.FLAG_ALPHABLEND;

            lvBkImage.hbm = bitmap.GetHbitmap();

            lvBkImage.cchImageMax = 0;

            lvBkImage.xOffsetPercent = 100;

            lvBkImage.yOffsetPercent = 100;



            IntPtr lbkImageptr = Marshal.AllocHGlobal(Marshal.SizeOf(lvBkImage));
            Marshal.StructureToPtr(lvBkImage, lbkImageptr, false);
            SendMessage(lvHandle, LVM_SETBKIMAGE, IntPtr.Zero, lbkImageptr);

            Marshal.FreeHGlobal(lbkImageptr);

        }

        public static Bitmap GetListViewBackgroundImage(IntPtr lvHandle)
        {

            LVBKIMAGE lvBkImage = new LVBKIMAGE();

            lvBkImage.ulFlags = LVBKIF.STYLE_WATERMARK | LVBKIF.FLAG_ALPHABLEND;

            SendMessage(lvHandle, LVM_GETBKIMAGE, IntPtr.Zero, ref lvBkImage);

            if (lvBkImage.hbm == IntPtr.Zero)

                return null;

            else

                return Bitmap.FromHbitmap(lvBkImage.hbm);

        }

        public static string LoadResourceString(string libraryName, uint ident, string defaultText) {
          IntPtr libraryHandle = LoadLibrary(libraryName);
          String Text = defaultText;
          if (libraryHandle != IntPtr.Zero) {
            StringBuilder sb = new StringBuilder(1024);
            int size = LoadString(libraryHandle, ident, sb, 1024);
            if (size > 0)
              Text = sb.ToString();
          }
          FreeLibrary(libraryHandle);
          return Text;
        }
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);



        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, MSG Msg,
            int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg,
            IntPtr wParam, ref LVBKIMAGE lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, int Msg,
            int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, MSG Msg,
            int wParam, ref LVITEMA lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct LVITEMA
        {
            public LVIF mask;
            public int iItem;
            public int iSubItem;
            public LVIS state;
            public LVIS stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public int lParam;
        }
        [Flags]
        public enum LVIF
        {
            LVIF_TEXT = 0x0001,
            LVIF_IMAGE = 0x0002,
            LVIF_PARAM = 0x0004,
            LVIF_STATE = 0x0008,
            LVIF_INDENT = 0x0010,
            LVIF_GROUPID = 0x0100,
            LVIF_COLUMNS = 0x0200,
            LVIF_NORECOMPUTE = 0x0800,
            LVIF_DI_SETITEM = 0x1000,
            LVIF_COLFMT = 0x00010000,
        }

        [Flags]
        public enum LVIS
        {
            LVIS_FOCUSED = 0x0001,
            LVIS_SELECTED = 0x0002,
            LVIS_CUT = 0x0004,
            LVIS_DROPHILITED = 0x0008,
            LVIS_ACTIVATING = 0x0020,
            LVIS_OVERLAYMASK = 0x0F00,
            LVIS_STATEIMAGEMASK = 0xF000,
        }

        [DllImport("shell32.dll")]
        public extern static void SHGetSetSettings(ref SHELLSTATE lpss, SSF dwMask, bool bSet);

        [DllImport("shell32.dll", EntryPoint = "#162", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHSimpleIDListFromPath(string szPath);

        #region SSF
        [Flags]
        public enum SSF
        {
            SSF_SHOWALLOBJECTS = 0x00000001,
            SSF_SHOWEXTENSIONS = 0x00000002,
            SSF_HIDDENFILEEXTS = 0x00000004,
            SSF_SERVERADMINUI = 0x00000004,
            SSF_SHOWCOMPCOLOR = 0x00000008,
            SSF_SORTCOLUMNS = 0x00000010,
            SSF_SHOWSYSFILES = 0x00000020,
            SSF_DOUBLECLICKINWEBVIEW = 0x00000080,
            SSF_SHOWATTRIBCOL = 0x00000100,
            SSF_DESKTOPHTML = 0x00000200,
            SSF_WIN95CLASSIC = 0x00000400,
            SSF_DONTPRETTYPATH = 0x00000800,
            SSF_SHOWINFOTIP = 0x00002000,
            SSF_MAPNETDRVBUTTON = 0x00001000,
            SSF_NOCONFIRMRECYCLE = 0x00008000,
            SSF_HIDEICONS = 0x00004000,
            SSF_FILTER = 0x00010000,
            SSF_WEBVIEW = 0x00020000,
            SSF_SHOWSUPERHIDDEN = 0x00040000,
            SSF_SEPPROCESS = 0x00080000,
            SSF_NONETCRAWLING = 0x00100000,
            SSF_STARTPANELON = 0x00200000,
            SSF_SHOWSTARTPAGE = 0x00400000,
            SSF_SHOWSTATUSBAR = 0x04000000
        }
        #endregion

        #region SHELLSTATE
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct SHELLSTATE
        {

            /// fShowAllObjects : 1
            ///fShowExtensions : 1
            ///fNoConfirmRecycle : 1
            ///fShowSysFiles : 1
            ///fShowCompColor : 1
            ///fDoubleClickInWebView : 1
            ///fDesktopHTML : 1
            ///fWin95Classic : 1
            ///fDontPrettyPath : 1
            ///fShowAttribCol : 1
            ///fMapNetDrvBtn : 1
            ///fShowInfoTip : 1
            ///fHideIcons : 1
            ///fWebView : 1
            ///fFilter : 1
            ///fShowSuperHidden : 1
            ///fNoNetCrawling : 1
            public uint bitvector1;

            /// DWORD->unsigned int
            public uint dwWin95Unused;

            /// UINT->unsigned int
            public uint uWin95Unused;

            /// LONG->int
            public int lParamSort;

            /// int
            public int iSortDirection;

            /// UINT->unsigned int
            public uint version;

            /// UINT->unsigned int
            public uint uNotUsed;

            /// fSepProcess : 1
            ///fStartPanelOn : 1
            ///fShowStartPage : 1
            ///fSpareFlags : 13
            public uint bitvector2;
            public uint bitvector3;

            public uint fShowAllObjects
            {
                get
                {
                    return ((uint)((this.bitvector1 & 1u)));
                }
                set
                {
                    this.bitvector1 = ((uint)((value | this.bitvector1)));
                }
            }

            public uint fShowExtensions
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 2u)
                                / 2)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 2)
                                | this.bitvector1)));
                }
            }

            public uint fNoConfirmRecycle
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 4u)
                                / 4)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 4)
                                | this.bitvector1)));
                }
            }

            public uint fShowSysFiles
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 8u)
                                / 8)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 8)
                                | this.bitvector1)));
                }
            }

            public uint fShowCompColor
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 16u)
                                / 16)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 16)
                                | this.bitvector1)));
                }
            }

            public uint fDoubleClickInWebView
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 32u)
                                / 32)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 32)
                                | this.bitvector1)));
                }
            }

            public uint fDesktopHTML
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 64u)
                                / 64)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 64)
                                | this.bitvector1)));
                }
            }

            public uint fWin95Classic
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 128u)
                                / 128)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 128)
                                | this.bitvector1)));
                }
            }

            public uint fDontPrettyPath
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 256u)
                                / 256)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 256)
                                | this.bitvector1)));
                }
            }

            public uint fShowAttribCol
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 512u)
                                / 512)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 512)
                                | this.bitvector1)));
                }
            }

            public uint fMapNetDrvBtn
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 1024u)
                                / 1024)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 1024)
                                | this.bitvector1)));
                }
            }

            public uint fShowInfoTip
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 2048u)
                                / 2048)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 2048)
                                | this.bitvector1)));
                }
            }

            public uint fHideIcons
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 4096u)
                                / 4096)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 4096)
                                | this.bitvector1)));
                }
            }

            public uint fWebView
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 8192u)
                                / 8192)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 8192)
                                | this.bitvector1)));
                }
            }

            public uint fFilter
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 16384u)
                                / 16384)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 16384)
                                | this.bitvector1)));
                }
            }

            public uint fShowSuperHidden
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 32768u)
                                / 32768)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 32768)
                                | this.bitvector1)));
                }
            }

            public uint fNoNetCrawling
            {
                get
                {
                    return ((uint)(((this.bitvector1 & 65536u)
                                / 65536)));
                }
                set
                {
                    this.bitvector1 = ((uint)(((value * 65536)
                                | this.bitvector1)));
                }
            }

            public uint fSepProcess
            {
                get
                {
                    return ((uint)((this.bitvector2 & 1u)));
                }
                set
                {
                    this.bitvector2 = ((uint)((value | this.bitvector2)));
                }
            }

            public uint fStartPanelOn
            {
                get
                {
                    return ((uint)(((this.bitvector2 & 2u)
                                / 2)));
                }
                set
                {
                    this.bitvector2 = ((uint)(((value * 2)
                                | this.bitvector2)));
                }
            }

            public uint fShowStartPage
            {
                get
                {
                    return ((uint)(((this.bitvector2 & 4u)
                                / 4)));
                }
                set
                {
                    this.bitvector2 = ((uint)(((value * 4)
                                | this.bitvector2)));
                }
            }

            public uint fShowStatusBar
            {
                get
                {
                    return ((uint)(((this.bitvector2 & 64u)
                                / 64)));
                }
                set
                {
                    this.bitvector2 = ((uint)(((value * 64)
                                | this.bitvector2)));
                }
            }

            public uint fSpareFlags
            {
                get
                {
                    return ((uint)(((this.bitvector2 & 65528u)
                                / 8)));
                }
                set
                {
                    this.bitvector2 = ((uint)(((value * 8)
                                | this.bitvector2)));
                }
            }
        }
        #endregion


        public enum ListViewExtendedStyles
        {
            /// <summary>
            /// LVS_EX_GRIDLINES
            /// </summary>
            GridLines = 0x00000001,
            /// <summary>
            /// LVS_EX_SUBITEMIMAGES
            /// </summary>
            SubItemImages = 0x00000002,
            /// <summary>
            /// LVS_EX_CHECKBOXES
            /// </summary>
            CheckBoxes = 0x00000004,
            /// <summary>
            /// LVS_EX_TRACKSELECT
            /// </summary>
            TrackSelect = 0x00000008,
            /// <summary>
            /// LVS_EX_HEADERDRAGDROP
            /// </summary>
            HeaderDragDrop = 0x00000010,
            /// <summary>
            /// LVS_EX_FULLROWSELECT
            /// </summary>
            FullRowSelect = 0x00000020,
            /// <summary>
            /// LVS_EX_ONECLICKACTIVATE
            /// </summary>
            OneClickActivate = 0x00000040,
            /// <summary>
            /// LVS_EX_TWOCLICKACTIVATE
            /// </summary>
            TwoClickActivate = 0x00000080,
            /// <summary>
            /// LVS_EX_FLATSB
            /// </summary>
            FlatsB = 0x00000100,
            /// <summary>
            /// LVS_EX_REGIONAL
            /// </summary>
            Regional = 0x00000200,
            /// <summary>
            /// LVS_EX_INFOTIP
            /// </summary>
            InfoTip = 0x00000400,
            /// <summary>
            /// LVS_EX_UNDERLINEHOT
            /// </summary>
            UnderlineHot = 0x00000800,
            /// <summary>
            /// LVS_EX_UNDERLINECOLD
            /// </summary>
            UnderlineCold = 0x00001000,
            /// <summary>
            /// LVS_EX_MULTIWORKAREAS
            /// </summary>
            MultilWorkAreas = 0x00002000,
            /// <summary>
            /// LVS_EX_LABELTIP
            /// </summary>
            LabelTip = 0x00004000,
            /// <summary>
            /// LVS_EX_BORDERSELECT
            /// </summary>
            BorderSelect = 0x00008000,
            /// <summary>
            /// LVS_EX_DOUBLEBUFFER
            /// </summary>
            DoubleBuffer = 0x00010000,
            /// <summary>
            /// LVS_EX_HIDELABELS
            /// </summary>
            HideLabels = 0x00020000,
            /// <summary>
            /// LVS_EX_SINGLEROW
            /// </summary>
            SingleRow = 0x00040000,
            /// <summary>
            /// LVS_EX_SNAPTOGRID
            /// </summary>
            SnapToGrid = 0x00080000,
            /// <summary>
            /// LVS_EX_SIMPLESELECT
            /// </summary>
            SimpleSelect = 0x00100000,
            /// <summary>
            /// LVS_EX_HEADERINALLVIEWS
            /// </summary>
            HeaderInAllViews = 0x02000000,

            AutosizeColumns = 0x10000000
        }


        public static void SetListViewExtendedStyle(IntPtr HWND, ListViewExtendedStyles exStyle, int stylesint)
        {
            ListViewExtendedStyles styles;
            styles = (ListViewExtendedStyles)SendMessage(HWND, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            styles |= exStyle;
            if (stylesint == -1)
            {

                SendMessage(HWND, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
            }
            if (stylesint == 0)
            {
                SendMessage(HWND, (int)ListViewMessages.SetExtendedStyle, -1, 0);
            }

        }

        public enum ListViewMessages
        {
            First = 0x1000,
            SetExtendedStyle = (First + 54),
            GetExtendedStyle = (First + 55),
        }

        [DllImport("ole32.dll")]
        public static extern int RegisterDragDrop(IntPtr hwnd, IDropTarget pDropTarget);

        [DllImport("ole32.dll")]
        public static extern int RevokeDragDrop(IntPtr hwnd);

        public static Guid IID_IDataObject
        {
            get { return new Guid("0000010e-0000-0000-C000-000000000046"); }
        }

        public static Guid IID_IDropTarget
        {
            get { return new Guid("00000122-0000-0000-C000-000000000046"); }
        }
        public static Point PointFromLPARAM(IntPtr lParam) {
          return new Point(
              (short)(((int)lParam) & 0xffff),
              (short)((((int)lParam) >> 0x10) & 0xffff));
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct TVHITTESTINFO {
          public Point pt;
          public int flags;
          public IntPtr hItem;
        }

        public enum NSTCSTYLE2 {
          NSTCS2_DEFAULT = 0,
          NSTCS2_INTERRUPTNOTIFICATIONS = 0x1,
          NSTCS2_SHOWNULLSPACEMENU = 0x2,
          NSTCS2_DISPLAYPADDING = 0x4,
          NSTCS2_DISPLAYPINNEDONLY = 0x8,
          NTSCS2_NOSINGLETONAUTOEXPAND = 0x10,
          NTSCS2_NEVERINSERTNONENUMERATED = 0x20
        } 

        public enum NSTCSTYLE { 
          NSTCS_HASEXPANDOS          = 0x00000001,
          NSTCS_HASLINES             = 0x00000002,
          NSTCS_SINGLECLICKEXPAND    = 0x00000004,
          NSTCS_FULLROWSELECT        = 0x00000008,
          NSTCS_SPRINGEXPAND         = 0x00000010,
          NSTCS_HORIZONTALSCROLL     = 0x00000020,
          NSTCS_ROOTHASEXPANDO       = 0x00000040,
          NSTCS_SHOWSELECTIONALWAYS  = 0x00000080,
          NSTCS_NOINFOTIP            = 0x00000200,
          NSTCS_EVENHEIGHT           = 0x00000400,
          NSTCS_NOREPLACEOPEN        = 0x00000800,
          NSTCS_DISABLEDRAGDROP      = 0x00001000,
          NSTCS_NOORDERSTREAM        = 0x00002000,
          NSTCS_RICHTOOLTIP          = 0x00004000,
          NSTCS_BORDER               = 0x00008000,
          NSTCS_NOEDITLABELS         = 0x00010000,
          NSTCS_TABSTOP              = 0x00020000,
          NSTCS_FAVORITESMODE        = 0x00080000,
          NSTCS_AUTOHSCROLL          = 0x00100000,
          NSTCS_FADEINOUTEXPANDOS    = 0x00200000,
          NSTCS_EMPTYTEXT            = 0x00400000,
          NSTCS_CHECKBOXES           = 0x00800000,
          NSTCS_PARTIALCHECKBOXES    = 0x01000000,
          NSTCS_EXCLUSIONCHECKBOXES  = 0x02000000,
          NSTCS_DIMMEDCHECKBOXES     = 0x04000000,
          NSTCS_NOINDENTCHECKS       = 0x08000000,
          NSTCS_ALLOWJUNCTIONS       = 0x10000000,
          NSTCS_SHOWTABSBUTTON       = 0x20000000,
          NSTCS_SHOWDELETEBUTTON     = 0x40000000,
          NSTCS_SHOWREFRESHBUTTON    = unchecked((int)0x80000000)
        }

        [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("028212A3-B627-47e9-8856-C14265554E4F")]
        public interface INameSpaceTreeControl {
          [PreserveSig]
          int Initialize(IntPtr hwndParent, ref WindowsHelper.WindowsAPI.RECT prc, int nsctsFlags);
          [PreserveSig]
          int TreeAdvise(IntPtr punk, out int pdwCookie);
          [PreserveSig]
          int TreeUnadvise(int dwCookie);
          [PreserveSig]
          int AppendRoot(IShellItem psiRoot, int grfEnumFlags, int grfRootStyle, /*IShellItemFilter*/ IntPtr pif);
          [PreserveSig]
          int InsertRoot(int iIndex, IShellItem psiRoot, int grfEnumFlags, int grfRootStyle, /*IShellItemFilter*/ IntPtr pif);
          [PreserveSig]
          int RemoveRoot(IShellItem psiRoot);
          [PreserveSig]
          int RemoveAllRoots();
          [PreserveSig]
          int GetRootItems(out /*IShellItemArray*/ IntPtr ppsiaRootItems);
          [PreserveSig]
          int SetItemState(IShellItem psi, int nstcisMask, int nstcisFlags);
          [PreserveSig]
          int GetItemState(IShellItem psi, int nstcisMask, out int pnstcisFlags);
          [PreserveSig]
          int GetSelectedItems(out /*IShellItemArray*/ IntPtr psiaItems);
          [PreserveSig]
          int GetItemCustomState(IShellItem psi, out int piStateNumber);
          [PreserveSig]
          int SetItemCustomState(IShellItem psi, int iStateNumber);
          [PreserveSig]
          int EnsureItemVisible(IShellItem psi);
          [PreserveSig]
          int SetTheme(string pszTheme);
          [PreserveSig]
          int GetNextItem(IShellItem psi, int nstcgi, out IShellItem ppsiNext);
          [PreserveSig]
          int HitTest([In] ref Point ppt, out IShellItem ppsiOut);
          [PreserveSig]
          int GetItemRect(IShellItem psi, out WindowsHelper.WindowsAPI.RECT prect);
          [PreserveSig]
          int CollapseAll();
        }

        [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7cc7aed8-290e-49bc-8945-c1401cc9306c")]
        public interface INameSpaceTreeControl2 : INameSpaceTreeControl {
          [PreserveSig]
          int GetControlStyle(NSTCSTYLE nstcsMask, out NSTCSTYLE pnstcsStyle);
          [PreserveSig]
          int GetControlStyle2(NSTCSTYLE2 nstcsMask, out NSTCSTYLE2 pnstcsStyle);
          [PreserveSig]
          int SetControlStyle(NSTCSTYLE nstcsMask, NSTCSTYLE nstcsStyle);
          [PreserveSig]
          int SetControlStyle2(NSTCSTYLE2 nstcsMask, NSTCSTYLE2 nstcsStyle);
        }

        [Serializable]
        public struct PROPERTYKEY {
          public Guid fmtid;
          public uint pid;
        }
        public enum SORT {
          DESCENDING = -1,
          ASCENDING = 1
        }
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct CM_COLUMNINFO {

          /// DWORD->unsigned int
          public int cbSize;

          /// DWORD->unsigned int
          public int dwMask;

          /// DWORD->unsigned int
          public int dwState;

          /// UINT->unsigned int
          public uint uWidth;

          /// UINT->unsigned int
          public uint uDefaultWidth;

          /// UINT->unsigned int
          public uint uIdealWidth;

          /// WCHAR[]
          [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
          public string wszName;
        }
        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        public struct SORTCOLUMN {
          public PROPERTYKEY propkey;
          public SORT direction;
        }

      [StructLayout(LayoutKind.Sequential)]
        public struct Message {
          public IntPtr hwnd;
          public uint message;
          public IntPtr wParam;
          public IntPtr lParam;
          public uint time;
          public Microsoft.WindowsAPICodePack.Controls.POINT pt;
        }

        [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000114-0000-0000-C000-000000000046")]
        public interface IOleWindow {
          [PreserveSig]
          int GetWindow(out IntPtr phwnd);
          [PreserveSig]
          int ContextSensitiveHelp(bool fEnterMode);
        }

        public enum ERROR {
          SUCCESS,
          FILE_EXISTS = 80,
          BAD_PATHNAME = 161,
          ALREADY_EXISTS = 183,
          FILENAME_EXCED_RANGE = 206,
          CANCELLED = 1223,
        }

        [DllImport("Ntshrui.dll")]
        public static extern HResult ShowShareFolderUI(IntPtr hwndParent, IntPtr pszPath);


        public const uint SHOP_FILEPATH = 0x2;
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern bool SHObjectProperties(IntPtr hwnd, uint shopObjectType, [MarshalAs(UnmanagedType.LPWStr)] string pszObjectName, [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyPage);


        [DllImport("shell32.dll", EntryPoint = "#165", CharSet = CharSet.Unicode)]
        public static extern ERROR SHCreateDirectory(IShellView hwnd, string pszPath);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

        [DllImport("shell32.dll")]
        public static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)] string name, IntPtr bindingContext, [Out()] out IntPtr pidl, uint sfgaoIn, [Out()] out uint psfgaoOut);

        [DllImport("shell32.dll")]
        public static extern IntPtr ILFindLastID(IntPtr pidl);
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hwnd, ref Point lpPoint);
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int dwThreadId);
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SHMultiFileProperties(System.Runtime.InteropServices.ComTypes.IDataObject pdtobj, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr handle, int messg, int wparam, int lparam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr handle, int messg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        public static IntPtr FindChildWindow(IntPtr parent, Predicate<IntPtr> pred) {
          IntPtr ret = IntPtr.Zero;
          EnumChildWindows(parent, (hwnd, lParam) => {
            if (pred(hwnd)) {
              ret = hwnd;
              return false;
            }
            return true;
          }, IntPtr.Zero);
          return ret;
        }
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        public static string GetClassName(IntPtr hwnd) {
          StringBuilder lpClassName = new StringBuilder(260);
          GetClassName(hwnd, lpClassName, lpClassName.Capacity);
          return lpClassName.ToString();
        }
        public static IntPtr SendMessage<T>(IntPtr hWnd, uint Msg, IntPtr wParam, ref T lParam) {
          IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(lParam));
          try {
            Marshal.StructureToPtr(lParam, ptr, false);
            IntPtr ret = SendMessage(hWnd, Msg, wParam, ptr);
            lParam = (T)Marshal.PtrToStructure(ptr, typeof(T));
            return ret;
          } finally {
            if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
          }
        }

        [DllImport("oleacc.dll")]
        public static extern int AccessibleObjectFromWindow(
             IntPtr hwnd,
             uint id,
             ref Guid iid,
             [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);   

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        public class SECURITY_ATTRIBUTES
        {
            public int nLength;
            public unsafe byte* lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            string lpReserved;
            string lpDesktop;
            string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        public enum LOGON_TYPE
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK,
            LOGON32_LOGON_BATCH,
            LOGON32_LOGON_SERVICE,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT,
            LOGON32_LOGON_NEW_CREDENTIALS
        }
        public enum LOGON_PROVIDER
        {
            LOGON32_PROVIDER_DEFAULT,
            LOGON32_PROVIDER_WINNT35,
            LOGON32_PROVIDER_WINNT40,
            LOGON32_PROVIDER_WINNT50
        }
        [Flags]
        public enum CreationFlags
        {
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000
        }

        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
         CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr RegisterPowerSettingNotification(
            IntPtr hRecipient,
            ref Guid PowerSettingGuid,
            Int32 Flags);

        public const int WM_POWERBROADCAST = 0x0218;

        public static Guid GUID_BATTERY_PERCENTAGE_REMAINING = new Guid("A7AD8041-B45A-4CAE-87A3-EECBB468A9E1");
        public static Guid GUID_MONITOR_POWER_ON = new Guid(0x02731015, 0x4510, 0x4526, 0x99, 0xE6, 0xE5, 0xA1, 0x7E, 0xBD, 0x1A, 0xEA);
        public static Guid GUID_ACDC_POWER_SOURCE = new Guid(0x5D3E9A59, 0xE9D5, 0x4B00, 0xA6, 0xBD, 0xFF, 0x34, 0xFF, 0x51, 0x65, 0x48);
        public static Guid GUID_POWERSCHEME_PERSONALITY = new Guid(0x245D8541, 0x3943, 0x4422, 0xB0, 0x25, 0x13, 0xA7, 0x84, 0xF6, 0x79, 0xB7);
        public static Guid GUID_MAX_POWER_SAVINGS = new Guid(0xA1841308, 0x3541, 0x4FAB, 0xBC, 0x81, 0xF7, 0x15, 0x56, 0xF2, 0x0B, 0x4A);
        // No Power Savings - Almost no power savings measures are used.
        public static Guid GUID_MIN_POWER_SAVINGS = new Guid(0x8C5E7FDA, 0xE8BF, 0x4A96, 0x9A, 0x85, 0xA6, 0xE2, 0x3A, 0x8C, 0x63, 0x5C);
        // Typical Power Savings - Fairly aggressive power savings measures are used.
        public static Guid GUID_TYPICAL_POWER_SAVINGS = new Guid(0x381B4222, 0xF694, 0x41F0, 0x96, 0x85, 0xFF, 0x5B, 0xB2, 0x60, 0xDF, 0x2E);

        // Win32 decls and defs
        //
        public const int PBT_APMQUERYSUSPEND = 0x0000;
        public const int PBT_APMQUERYSTANDBY = 0x0001;
        public const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
        public const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
        public const int PBT_APMSUSPEND = 0x0004;
        public const int PBT_APMSTANDBY = 0x0005;
        public const int PBT_APMRESUMECRITICAL = 0x0006;
        public const int PBT_APMRESUMESUSPEND = 0x0007;
        public const int PBT_APMRESUMESTANDBY = 0x0008;
        public const int PBT_APMBATTERYLOW = 0x0009;
        public const int PBT_APMPOWERSTATUSCHANGE = 0x000A; // power status
        public const int PBT_APMOEMEVENT = 0x000B;
        public const int PBT_APMRESUMEAUTOMATIC = 0x0012;
        public const int PBT_POWERSETTINGCHANGE = 0x8013; // DPPE

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;

        // This structure is sent when the PBT_POWERSETTINGSCHANGE message is sent.
        // It describes the power setting that has changed and contains data about the change
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }


        [DllImport(@"SHLWAPI", CharSet = CharSet.Auto)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool PathIsDirectory([MarshalAsAttribute(UnmanagedType.LPWStr), In] string pszPath);


        [DllImport(@"User32", EntryPoint = "UnregisterPowerSettingNotification",
         CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnregisterPowerSettingNotification(
            IntPtr handle);

        //Possible values for lpOperation
        //"edit"
        //"explore"
        //"find"
        //"open"
        //"print"

        /// <summary>
        /// Invoke win32 shell
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lpOperation"></param>
        /// <param name="lpFile"></param>
        /// <param name="lpParameters"></param>
        /// <param name="lpDirectory"></param>
        /// <param name="nShowCmd"></param>
        /// <returns></returns>
        [DllImport("shell32.dll")]
        public static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            ShowCommands nShowCmd);

        public static uint PW_CLINETONLY = 0x1;
        public enum ShellEvents
        {
            HSHELL_WINDOWCREATED = 1,
            HSHELL_WINDOWDESTROYED = 2,
            HSHELL_ACTIVATESHELLWINDOW = 3,
            HSHELL_WINDOWACTIVATED = 4,
            HSHELL_GETMINRECT = 5,
            HSHELL_REDRAW = 6,
            HSHELL_TASKMAN = 7,
            HSHELL_LANGUAGE = 8,
            HSHELL_ACCESSIBILITYSTATE = 11,
            HSHELL_FLASH = 0x8006
        }
        public struct DWM_BLURBEHIND
        {
            public int dwFlags;
            public bool fEnable;
            public System.IntPtr hRgnBlur;//HRGN
            public bool fTransitionOnMaximized;
        }
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect,
           int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2,
           int cx, int cy);
        [DllImport("dwmapi")]
        public static extern int DwmEnableBlurBehindWindow(
                    System.IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);
        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);
        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);
        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);
        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);
        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct PSIZE
        {
            public int x;
            public int y;
        }
        [Flags()]
        public enum DwmThumbnailFlags : uint
        {
            DWM_TNP_RECTDESTINATION = 0x1,
            DWM_TNP_RECTSOURCE = 0x2,
            DWM_TNP_OPACITY = 0x4,
            DWM_TNP_VISIBLE = 0x8,
            DWM_TNP_SOURCECLIENTAREAONLY = 0x10
        }

        public delegate void TimerSetDelegate();
        public delegate void TimerCompleteDelegate();
        public const int WAIT_ABANDONED = 0x80,
           WAIT_ABANDONED_0 = 0x80,
           WAIT_FAILED = -1,
           WAIT_IO_COMPLETION = 0xC0,
           WAIT_OBJECT_0 = 0,
           WAIT_OBJECT_1 = 1,
           WAIT_TIMEOUT = 0x102,
            //INFINITE = 0xFFFF,
           ERROR_ALREADY_EXISTS = 183,
           QS_HOTKEY = 0x80,
           QS_KEY = 0x1,
           QS_MOUSEBUTTON = 0x4,
           QS_MOUSEMOVE = 0x2,
           QS_PAINT = 0x20,
           QS_POSTMESSAGE = 0x8,
           QS_SENDMESSAGE = 0x40,
           QS_TIMER = 0x10,
           QS_MOUSE = (QS_MOUSEMOVE | QS_MOUSEBUTTON),
           QS_INPUT = (QS_MOUSE | QS_KEY),
           QS_ALLEVENTS = (QS_INPUT | QS_POSTMESSAGE | QS_TIMER | QS_PAINT | QS_HOTKEY),
           QS_ALLINPUT = (QS_SENDMESSAGE | QS_PAINT | QS_TIMER | QS_POSTMESSAGE |
               QS_MOUSEBUTTON | QS_MOUSEMOVE | QS_HOTKEY | QS_KEY);
        public const UInt32 INFINITE = 0xFFFFFFFF;

        public delegate void TimerAPCProc(
                                            IntPtr lpArgToCompletionRoutine,
                                            UInt32 dwTimerLowValue,
                                            UInt32 dwTimerHighValue);
        [DllImport("user32.dll")]

        public static extern UInt32 RegisterWindowMessageA(string lpString);

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]

        public static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra,[Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        [Flags]

        public enum AssocF
        {

            Init_NoRemapCLSID = 0x1,

            Init_ByExeName = 0x2,

            Open_ByExeName = 0x2,

            Init_DefaultToStar = 0x4,

            Init_DefaultToFolder = 0x8,

            NoUserSettings = 0x10,

            NoTruncate = 0x20,

            Verify = 0x40,

            RemapRunDll = 0x80,

            NoFixUps = 0x100,

            IgnoreBaseClass = 0x200

        }

        public enum AssocStr
        {

            Command = 1,

            Executable,

            FriendlyDocName,

            FriendlyAppName,

            NoOpen,

            ShellNewValue,

            DDECommand,

            DDEIfExec,

            DDEApplication,

            DDETopic

        }

        [DllImport("kernel32.dll")]
        public static extern void ExitThread(uint dwExitCode);

        [DllImport("kernel32.dll")]
        static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance([In] ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        /// <summary>
        /// Formats a Drive
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="drive"></param>
        /// <param name="fmtID"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [DllImport("shell32.dll")]
        public static extern uint SHFormatDrive(IntPtr hwnd, uint drive, SHFormatFlags fmtID,
                               SHFormatOptions options);

        public enum SHFormatFlags : uint
        {
            SHFMT_ID_DEFAULT = 0xFFFF,
        }

        [Flags]
        public enum SHFormatOptions : uint
        {
            SHFMT_OPT_FULL = 0x1,
            SHFMT_OPT_SYSONLY = 0x2,
        }

        public const uint SHFMT_ERROR = 0xFFFFFFFF;
        public const uint SHFMT_CANCEL = 0xFFFFFFFE;
        public const uint SHFMT_NOFORMAT = 0xFFFFFFD;

        /// <summary>
        /// Format a Drive by givven Drive letter
        /// </summary>
        /// <param name="DriveLetter">The Drive letter</param>
        /// <returns>Error or Success Code</returns>
        public static uint FormatDrive(IntPtr Handle ,string DriveLetter)
        {
            DriveInfo drive = new DriveInfo(DriveLetter);
            byte[] bytes = Encoding.ASCII.GetBytes(drive.Name.ToCharArray());
            uint driveNumber = Convert.ToUInt32(bytes[0] - Encoding.ASCII.GetBytes(new[] { 'A' })[0]);
            uint Result = SHFormatDrive(Handle, driveNumber, SHFormatFlags.SHFMT_ID_DEFAULT, 
                 SHFormatOptions.SHFMT_OPT_FULL);
            return Result;
        }


        public static readonly string UserPinnedItemsPath = "{0}\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar\\";
        public static bool IsPinnedToTaskbar(string executablePath)
        {
            foreach (string pinnedShortcut in Directory.GetFiles(string.Format(UserPinnedItemsPath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), "*.lnk"))
            {

 //               var shortcut = new ShellLinkClass.ShellLink(pinnedShortcut);
  //              if (shortcut.Target == executablePath)
   //                 return true;
            }

            return false;
        }
        public static void PinUnpinToTaskbar(string filePath)
        {
            PinUnpin(filePath, !IsPinnedToTaskbar(filePath));
        }

        public static void UnpinFromTaskbar(string filePath)
        {
            PinUnpin(filePath, false);
        }

        private static void PinUnpin(string filePath, bool pin)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            // create the shell application object
            dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            dynamic directory = shellApplication.NameSpace(path);
            dynamic link = directory.ParseName(fileName);

            dynamic verbs = link.Verbs();
            for (int i = 0; i < verbs.Count(); i++)
            {
                dynamic verb = verbs.Item(i);
                string verbName = verb.Name.Replace(@"&", string.Empty).ToLower();

                if ((pin && verbName.Equals("pin to taskbar"))
                || (!pin && verbName.Equals("unpin from taskbar"))
                )
                {
                    verb.DoIt();
                }
            }

            shellApplication = null;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

//        [DllImport("user32.dll")]
//        public static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y,
//           int nReserved, IntPtr hWnd, IntPtr prcRect);
        [DllImport("user32.dll")]
        public static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y,
           int nReserved, IntPtr hWnd, IntPtr prcRect);

        //[DllImport("user32.dll")]
        //public static extern bool GetMenuItemInfo(IntPtr hMenu, uint uItem, bool fByPosition, ref ShellContextMenu.MENUITEMINFO lpmii);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]

        public static extern Boolean DeregisterShellHookWindow(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]

        public static extern Boolean RegisterShellHookWindow(IntPtr hwnd);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        public static extern UInt32 SleepEx(UInt32 dwMilliseconds, bool bAlertable);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll")]
        public static extern bool SetWaitableTimer(IntPtr hTimer,
            [In] ref long pDueTime, int lPeriod,
            TimerAPCProc pfnCompletionRoutine,
            IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("ole32.dll")]
        public static extern void CoFreeUnusedLibraries();

        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        public delegate bool EnumThreadDelegate(IntPtr hwnd, IntPtr lParam);

        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        [DllImport("kernel32.dll")]
        public static extern bool CancelWaitableTimer(IntPtr hTimer);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth,
           int nHeight);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool InvalidateRect(
        IntPtr hWnd,
        ref Rectangle lpRect,
        bool bErase);

        [DllImport("user32.dll")]
        public extern static bool UpdateWindow(IntPtr wnd);


        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern Int32 WaitForSingleObject(IntPtr Handle, uint Wait);


        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECTW rect);

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam,
           IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        public const int SC_CLOSE = 0xF060;
        public const int WM_SYSCOMMAND = 0x0112;

        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }


        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECTW
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECTW(int left_, int top_, int right_, int bottom_)
            {
                Left = left_;
                Top = top_;
                Right = right_;
                Bottom = bottom_;
            }

            public int Height { get { return Bottom - Top; } }
            public int Width { get { return Right - Left; } }
            public System.Drawing.Size Size { get { return new Size(Width, Height); } }

            public Point Location { get { return new Point(Left, Top); } }

            // Handy method for converting to a System.Drawing.Rectangle
            public Rectangle ToRectangle()
            { return Rectangle.FromLTRB(Left, Top, Right, Bottom); }

            public static RECTW FromRectangle(Rectangle rectangle)
            {
                return new RECTW(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public override int GetHashCode()
            {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                  ^ ((Width << 0x1a) | (Width >> 6))
                  ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            public static implicit operator Rectangle(RECTW rect)
            {
                return rect.ToRectangle();
            }

            public static implicit operator RECTW(Rectangle rect)
            {
                return FromRectangle(rect);
            }

            #endregion
        }



        //Stop flashing. The system restores the window to its original state.
        public const UInt32 FLASHW_STOP = 0;
        //Flash the window caption.
        public const UInt32 FLASHW_CAPTION = 1;
        //Flash the taskbar button.
        public const UInt32 FLASHW_TRAY = 2;
        //Flash both the window caption and taskbar button.
        //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        public const UInt32 FLASHW_ALL = 3;
        //Flash continuously, until the FLASHW_STOP flag is set.
        public const UInt32 FLASHW_TIMER = 4;
        //Flash continuously until the window comes to the foreground.
        public const UInt32 FLASHW_TIMERNOFG = 12;


        [DllImport("user32.dll")]
        private static extern uint RealGetWindowClass(IntPtr hWnd, StringBuilder pszType, uint cchType);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);
        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, IntPtr pvParam, SPIF fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(
           int uAction, int uParam, ref bool lpvParam,
           int flags);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref RECT pvParam, SPIF fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, String pvParam, SPIF fWinIni);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref ANIMATIONINFO pvParam, SPIF fWinIni);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32")]
        public static extern int SetLayeredWindowAttributes(IntPtr hWnd, byte crey,
         byte alpha, int flags);

        [DllImport("user32")]
        public static extern int SetWindowLong(IntPtr hWnd, int index, int dwNewLong);

        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        //public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);
        //[DllImport("user32.Dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        /// <summary>
        /// The GetForegroundWindow function returns a handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public struct RECT
        {
            public Int32 Left;
            public Int32 Right;
            public Int32 Top;
            public Int32 Bottom;
        }

        #region SPI
        /// <summary>
        /// SPI_ System-wide parameter - Used in SystemParametersInfo function
        /// </summary>
        [Description("SPI_(System-wide parameter - Used in SystemParametersInfo function )")]
        public enum SPI : uint
        {
            /// <summary>
            /// Determines whether the warning beeper is on.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if the beeper is on, or FALSE if it is off.
            /// </summary>
            SPI_GETBEEP = 0x0001,

            /// <summary>
            /// Turns the warning beeper on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// </summary>
            SPI_SETBEEP = 0x0002,

            /// <summary>
            /// Retrieves the two mouse threshold values and the mouse speed.
            /// </summary>
            SPI_GETMOUSE = 0x0003,

            /// <summary>
            /// Sets the two mouse threshold values and the mouse speed.
            /// </summary>
            SPI_SETMOUSE = 0x0004,

            /// <summary>
            /// Retrieves the border multiplier factor that determines the width of a window's sizing border.
            /// The pvParam parameter must point to an integer variable that receives this value.
            /// </summary>
            SPI_GETBORDER = 0x0005,

            /// <summary>
            /// Sets the border multiplier factor that determines the width of a window's sizing border.
            /// The uiParam parameter specifies the new value.
            /// </summary>
            SPI_SETBORDER = 0x0006,

            /// <summary>
            /// Retrieves the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 2.5 repetitions per second)
            /// through 31 (approximately 30 repetitions per second). The actual repeat rates are hardware-dependent and may vary from
            /// a linear scale by as much as 20%. The pvParam parameter must point to a DWORD variable that receives the setting
            /// </summary>
            SPI_GETKEYBOARDSPEED = 0x000A,

            /// <summary>
            /// Sets the keyboard repeat-speed setting. The uiParam parameter must specify a value in the range from 0
            /// (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second).
            /// The actual repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%.
            /// If uiParam is greater than 31, the parameter is set to 31.
            /// </summary>
            SPI_SETKEYBOARDSPEED = 0x000B,

            /// <summary>
            /// Not implemented.
            /// </summary>
            SPI_LANGDRIVER = 0x000C,

            /// <summary>
            /// Sets or retrieves the width, in pixels, of an icon cell. The system uses this rectangle to arrange icons in large icon view.
            /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CXICON.
            /// To retrieve this value, pvParam must point to an integer that receives the current value.
            /// </summary>
            SPI_ICONHORIZONTALSPACING = 0x000D,

            /// <summary>
            /// Retrieves the screen saver time-out value, in seconds. The pvParam parameter must point to an integer variable that receives the value.
            /// </summary>
            SPI_GETSCREENSAVETIMEOUT = 0x000E,

            /// <summary>
            /// Sets the screen saver time-out value to the value of the uiParam parameter. This value is the amount of time, in seconds,
            /// that the system must be idle before the screen saver activates.
            /// </summary>
            SPI_SETSCREENSAVETIMEOUT = 0x000F,

            /// <summary>
            /// Determines whether screen saving is enabled. The pvParam parameter must point to a bool variable that receives TRUE
            /// if screen saving is enabled, or FALSE otherwise.
            /// </summary>
            SPI_GETSCREENSAVEACTIVE = 0x0010,

            /// <summary>
            /// Sets the state of the screen saver. The uiParam parameter specifies TRUE to activate screen saving, or FALSE to deactivate it.
            /// </summary>
            SPI_SETSCREENSAVEACTIVE = 0x0011,

            /// <summary>
            /// Retrieves the current granularity value of the desktop sizing grid. The pvParam parameter must point to an integer variable
            /// that receives the granularity.
            /// </summary>
            SPI_GETGRIDGRANULARITY = 0x0012,

            /// <summary>
            /// Sets the granularity of the desktop sizing grid to the value of the uiParam parameter.
            /// </summary>
            SPI_SETGRIDGRANULARITY = 0x0013,

            /// <summary>
            /// Sets the desktop wallpaper. The value of the pvParam parameter determines the new wallpaper. To specify a wallpaper bitmap,
            /// set pvParam to point to a null-terminated string containing the name of a bitmap file. Setting pvParam to "" removes the wallpaper.
            /// Setting pvParam to SETWALLPAPER_DEFAULT or null reverts to the default wallpaper.
            /// </summary>
            SPI_SETDESKWALLPAPER = 0x0014,

            /// <summary>
            /// Sets the current desktop pattern by causing Windows to read the Pattern= setting from the WIN.INI file.
            /// </summary>
            SPI_SETDESKPATTERN = 0x0015,

            /// <summary>
            /// Retrieves the keyboard repeat-delay setting, which is a value in the range from 0 (approximately 250 ms delay) through 3
            /// (approximately 1 second delay). The actual delay associated with each value may vary depending on the hardware. The pvParam parameter must point to an integer variable that receives the setting.
            /// </summary>
            SPI_GETKEYBOARDDELAY = 0x0016,

            /// <summary>
            /// Sets the keyboard repeat-delay setting. The uiParam parameter must specify 0, 1, 2, or 3, where zero sets the shortest delay
            /// (approximately 250 ms) and 3 sets the longest delay (approximately 1 second). The actual delay associated with each value may
            /// vary depending on the hardware.
            /// </summary>
            SPI_SETKEYBOARDDELAY = 0x0017,

            /// <summary>
            /// Sets or retrieves the height, in pixels, of an icon cell.
            /// To set this value, set uiParam to the new value and set pvParam to null. You cannot set this value to less than SM_CYICON.
            /// To retrieve this value, pvParam must point to an integer that receives the current value.
            /// </summary>
            SPI_ICONVERTICALSPACING = 0x0018,

            /// <summary>
            /// Determines whether icon-title wrapping is enabled. The pvParam parameter must point to a bool variable that receives TRUE
            /// if enabled, or FALSE otherwise.
            /// </summary>
            SPI_GETICONTITLEWRAP = 0x0019,

            /// <summary>
            /// Turns icon-title wrapping on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// </summary>
            SPI_SETICONTITLEWRAP = 0x001A,

            /// <summary>
            /// Determines whether pop-up menus are left-aligned or right-aligned, relative to the corresponding menu-bar item.
            /// The pvParam parameter must point to a bool variable that receives TRUE if left-aligned, or FALSE otherwise.
            /// </summary>
            SPI_GETMENUDROPALIGNMENT = 0x001B,

            /// <summary>
            /// Sets the alignment value of pop-up menus. The uiParam parameter specifies TRUE for right alignment, or FALSE for left alignment.
            /// </summary>
            SPI_SETMENUDROPALIGNMENT = 0x001C,

            /// <summary>
            /// Sets the width of the double-click rectangle to the value of the uiParam parameter.
            /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered
            /// as a double-click.
            /// To retrieve the width of the double-click rectangle, call GetSystemMetrics with the SM_CXDOUBLECLK flag.
            /// </summary>
            SPI_SETDOUBLECLKWIDTH = 0x001D,

            /// <summary>
            /// Sets the height of the double-click rectangle to the value of the uiParam parameter.
            /// The double-click rectangle is the rectangle within which the second click of a double-click must fall for it to be registered
            /// as a double-click.
            /// To retrieve the height of the double-click rectangle, call GetSystemMetrics with the SM_CYDOUBLECLK flag.
            /// </summary>
            SPI_SETDOUBLECLKHEIGHT = 0x001E,

            /// <summary>
            /// Retrieves the logical font information for the current icon-title font. The uiParam parameter specifies the size of a LOGFONT structure,
            /// and the pvParam parameter must point to the LOGFONT structure to fill in.
            /// </summary>
            SPI_GETICONTITLELOGFONT = 0x001F,

            /// <summary>
            /// Sets the double-click time for the mouse to the value of the uiParam parameter. The double-click time is the maximum number
            /// of milliseconds that can occur between the first and second clicks of a double-click. You can also call the SetDoubleClickTime
            /// function to set the double-click time. To get the current double-click time, call the GetDoubleClickTime function.
            /// </summary>
            SPI_SETDOUBLECLICKTIME = 0x0020,

            /// <summary>
            /// Swaps or restores the meaning of the left and right mouse buttons. The uiParam parameter specifies TRUE to swap the meanings
            /// of the buttons, or FALSE to restore their original meanings.
            /// </summary>
            SPI_SETMOUSEBUTTONSWAP = 0x0021,

            /// <summary>
            /// Sets the font that is used for icon titles. The uiParam parameter specifies the size of a LOGFONT structure,
            /// and the pvParam parameter must point to a LOGFONT structure.
            /// </summary>
            SPI_SETICONTITLELOGFONT = 0x0022,

            /// <summary>
            /// This flag is obsolete. Previous versions of the system use this flag to determine whether ALT+TAB fast task switching is enabled.
            /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
            /// </summary>
            SPI_GETFASTTASKSWITCH = 0x0023,

            /// <summary>
            /// This flag is obsolete. Previous versions of the system use this flag to enable or disable ALT+TAB fast task switching.
            /// For Windows 95, Windows 98, and Windows NT version 4.0 and later, fast task switching is always enabled.
            /// </summary>
            SPI_SETFASTTASKSWITCH = 0x0024,

            //#if(WINVER >= 0x0400)
            /// <summary>
            /// Sets dragging of full windows either on or off. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_SETDRAGFULLWINDOWS = 0x0025,

            /// <summary>
            /// Determines whether dragging of full windows is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled, or FALSE otherwise.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_GETDRAGFULLWINDOWS = 0x0026,

            /// <summary>
            /// Retrieves the metrics associated with the nonclient area of nonminimized windows. The pvParam parameter must point
            /// to a NONCLIENTMETRICS structure that receives the information. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(NONCLIENTMETRICS).
            /// </summary>
            SPI_GETNONCLIENTMETRICS = 0x0029,

            /// <summary>
            /// Sets the metrics associated with the nonclient area of nonminimized windows. The pvParam parameter must point
            /// to a NONCLIENTMETRICS structure that contains the new parameters. Set the cbSize member of this structure
            /// and the uiParam parameter to sizeof(NONCLIENTMETRICS). Also, the lfHeight member of the LOGFONT structure must be a negative value.
            /// </summary>
            SPI_SETNONCLIENTMETRICS = 0x002A,

            /// <summary>
            /// Retrieves the metrics associated with minimized windows. The pvParam parameter must point to a MINIMIZEDMETRICS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MINIMIZEDMETRICS).
            /// </summary>
            SPI_GETMINIMIZEDMETRICS = 0x002B,

            /// <summary>
            /// Sets the metrics associated with minimized windows. The pvParam parameter must point to a MINIMIZEDMETRICS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MINIMIZEDMETRICS).
            /// </summary>
            SPI_SETMINIMIZEDMETRICS = 0x002C,

            /// <summary>
            /// Retrieves the metrics associated with icons. The pvParam parameter must point to an ICONMETRICS structure that receives
            /// the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(ICONMETRICS).
            /// </summary>
            SPI_GETICONMETRICS = 0x002D,

            /// <summary>
            /// Sets the metrics associated with icons. The pvParam parameter must point to an ICONMETRICS structure that contains
            /// the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ICONMETRICS).
            /// </summary>
            SPI_SETICONMETRICS = 0x002E,

            /// <summary>
            /// Sets the size of the work area. The work area is the portion of the screen not obscured by the system taskbar
            /// or by application desktop toolbars. The pvParam parameter is a pointer to a RECT structure that specifies the new work area rectangle,
            /// expressed in virtual screen coordinates. In a system with multiple display monitors, the function sets the work area
            /// of the monitor that contains the specified rectangle.
            /// </summary>
            SPI_SETWORKAREA = 0x002F,

            /// <summary>
            /// Retrieves the size of the work area on the primary display monitor. The work area is the portion of the screen not obscured
            /// by the system taskbar or by application desktop toolbars. The pvParam parameter must point to a RECT structure that receives
            /// the coordinates of the work area, expressed in virtual screen coordinates.
            /// To get the work area of a monitor other than the primary display monitor, call the GetMonitorInfo function.
            /// </summary>
            SPI_GETWORKAREA = 0x0030,

            /// <summary>
            /// Windows Me/98/95:  Pen windows is being loaded or unloaded. The uiParam parameter is TRUE when loading and FALSE
            /// when unloading pen windows. The pvParam parameter is null.
            /// </summary>
            SPI_SETPENWINDOWS = 0x0031,

            /// <summary>
            /// Retrieves information about the HighContrast accessibility feature. The pvParam parameter must point to a HIGHCONTRAST structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(HIGHCONTRAST).
            /// For a general discussion, see remarks.
            /// Windows NT:  This value is not supported.
            /// </summary>
            /// <remarks>
            /// There is a difference between the High Contrast color scheme and the High Contrast Mode. The High Contrast color scheme changes
            /// the system colors to colors that have obvious contrast; you switch to this color scheme by using the Display Options in the control panel.
            /// The High Contrast Mode, which uses SPI_GETHIGHCONTRAST and SPI_SETHIGHCONTRAST, advises applications to modify their appearance
            /// for visually-impaired users. It involves such things as audible warning to users and customized color scheme
            /// (using the Accessibility Options in the control panel). For more information, see HIGHCONTRAST on MSDN.
            /// For more information on general accessibility features, see Accessibility on MSDN.
            /// </remarks>
            SPI_GETHIGHCONTRAST = 0x0042,

            /// <summary>
            /// Sets the parameters of the HighContrast accessibility feature. The pvParam parameter must point to a HIGHCONTRAST structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(HIGHCONTRAST).
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_SETHIGHCONTRAST = 0x0043,

            /// <summary>
            /// Determines whether the user relies on the keyboard instead of the mouse, and wants applications to display keyboard interfaces
            /// that would otherwise be hidden. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if the user relies on the keyboard; or FALSE otherwise.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_GETKEYBOARDPREF = 0x0044,

            /// <summary>
            /// Sets the keyboard preference. The uiParam parameter specifies TRUE if the user relies on the keyboard instead of the mouse,
            /// and wants applications to display keyboard interfaces that would otherwise be hidden; uiParam is FALSE otherwise.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_SETKEYBOARDPREF = 0x0045,

            /// <summary>
            /// Determines whether a screen reviewer utility is running. A screen reviewer utility directs textual information to an output device,
            /// such as a speech synthesizer or Braille display. When this flag is set, an application should provide textual information
            /// in situations where it would otherwise present the information graphically.
            /// The pvParam parameter is a pointer to a BOOL variable that receives TRUE if a screen reviewer utility is running, or FALSE otherwise.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_GETSCREENREADER = 0x0046,

            /// <summary>
            /// Determines whether a screen review utility is running. The uiParam parameter specifies TRUE for on, or FALSE for off.
            /// Windows NT:  This value is not supported.
            /// </summary>
            SPI_SETSCREENREADER = 0x0047,

            /// <summary>
            /// Retrieves the animation effects associated with user actions. The pvParam parameter must point to an ANIMATIONINFO structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(ANIMATIONINFO).
            /// </summary>
            SPI_GETANIMATION = 0x0048,

            /// <summary>
            /// Sets the animation effects associated with user actions. The pvParam parameter must point to an ANIMATIONINFO structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ANIMATIONINFO).
            /// </summary>
            SPI_SETANIMATION = 0x0049,

            /// <summary>
            /// Determines whether the font smoothing feature is enabled. This feature uses font antialiasing to make font curves appear smoother
            /// by painting pixels at different gray levels.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is enabled, or FALSE if it is not.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_GETFONTSMOOTHING = 0x004A,

            /// <summary>
            /// Enables or disables the font smoothing feature, which uses font antialiasing to make font curves appear smoother
            /// by painting pixels at different gray levels.
            /// To enable the feature, set the uiParam parameter to TRUE. To disable the feature, set uiParam to FALSE.
            /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
            /// </summary>
            SPI_SETFONTSMOOTHING = 0x004B,

            /// <summary>
            /// Sets the width, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value.
            /// To retrieve the drag width, call GetSystemMetrics with the SM_CXDRAG flag.
            /// </summary>
            SPI_SETDRAGWIDTH = 0x004C,

            /// <summary>
            /// Sets the height, in pixels, of the rectangle used to detect the start of a drag operation. Set uiParam to the new value.
            /// To retrieve the drag height, call GetSystemMetrics with the SM_CYDRAG flag.
            /// </summary>
            SPI_SETDRAGHEIGHT = 0x004D,

            /// <summary>
            /// Used internally; applications should not use this value.
            /// </summary>
            SPI_SETHANDHELD = 0x004E,

            /// <summary>
            /// Retrieves the time-out value for the low-power phase of screen saving. The pvParam parameter must point to an integer variable
            /// that receives the value. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETLOWPOWERTIMEOUT = 0x004F,

            /// <summary>
            /// Retrieves the time-out value for the power-off phase of screen saving. The pvParam parameter must point to an integer variable
            /// that receives the value. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETPOWEROFFTIMEOUT = 0x0050,

            /// <summary>
            /// Sets the time-out value, in seconds, for the low-power phase of screen saving. The uiParam parameter specifies the new value.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETLOWPOWERTIMEOUT = 0x0051,

            /// <summary>
            /// Sets the time-out value, in seconds, for the power-off phase of screen saving. The uiParam parameter specifies the new value.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETPOWEROFFTIMEOUT = 0x0052,

            /// <summary>
            /// Determines whether the low-power phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETLOWPOWERACTIVE = 0x0053,

            /// <summary>
            /// Determines whether the power-off phase of screen saving is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if enabled, or FALSE if disabled. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_GETPOWEROFFACTIVE = 0x0054,

            /// <summary>
            /// Activates or deactivates the low-power phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETLOWPOWERACTIVE = 0x0055,

            /// <summary>
            /// Activates or deactivates the power-off phase of screen saving. Set uiParam to 1 to activate, or zero to deactivate.
            /// The pvParam parameter must be null. This flag is supported for 32-bit applications only.
            /// Windows NT, Windows Me/98:  This flag is supported for 16-bit and 32-bit applications.
            /// Windows 95:  This flag is supported for 16-bit applications only.
            /// </summary>
            SPI_SETPOWEROFFACTIVE = 0x0056,

            /// <summary>
            /// Reloads the system cursors. Set the uiParam parameter to zero and the pvParam parameter to null.
            /// </summary>
            SPI_SETCURSORS = 0x0057,

            /// <summary>
            /// Reloads the system icons. Set the uiParam parameter to zero and the pvParam parameter to null.
            /// </summary>
            SPI_SETICONS = 0x0058,

            /// <summary>
            /// Retrieves the input locale identifier for the system default input language. The pvParam parameter must point
            /// to an HKL variable that receives this value. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
            /// </summary>
            SPI_GETDEFAULTINPUTLANG = 0x0059,

            /// <summary>
            /// Sets the default input language for the system shell and applications. The specified language must be displayable
            /// using the current system character set. The pvParam parameter must point to an HKL variable that contains
            /// the input locale identifier for the default language. For more information, see Languages, Locales, and Keyboard Layouts on MSDN.
            /// </summary>
            SPI_SETDEFAULTINPUTLANG = 0x005A,

            /// <summary>
            /// Sets the hot key set for switching between input languages. The uiParam and pvParam parameters are not used.
            /// The value sets the shortcut keys in the keyboard property sheets by reading the registry again. The registry must be set before this flag is used. the path in the registry is \HKEY_CURRENT_USER\keyboard layout\toggle. Valid values are "1" = ALT+SHIFT, "2" = CTRL+SHIFT, and "3" = none.
            /// </summary>
            SPI_SETLANGTOGGLE = 0x005B,

            /// <summary>
            /// Windows 95:  Determines whether the Windows extension, Windows Plus!, is installed. Set the uiParam parameter to 1.
            /// The pvParam parameter is not used. The function returns TRUE if the extension is installed, or FALSE if it is not.
            /// </summary>
            SPI_GETWINDOWSEXTENSION = 0x005C,

            /// <summary>
            /// Enables or disables the Mouse Trails feature, which improves the visibility of mouse cursor movements by briefly showing
            /// a trail of cursors and quickly erasing them.
            /// To disable the feature, set the uiParam parameter to zero or 1. To enable the feature, set uiParam to a value greater than 1
            /// to indicate the number of cursors drawn in the trail.
            /// Windows 2000/NT:  This value is not supported.
            /// </summary>
            SPI_SETMOUSETRAILS = 0x005D,

            /// <summary>
            /// Determines whether the Mouse Trails feature is enabled. This feature improves the visibility of mouse cursor movements
            /// by briefly showing a trail of cursors and quickly erasing them.
            /// The pvParam parameter must point to an integer variable that receives a value. If the value is zero or 1, the feature is disabled.
            /// If the value is greater than 1, the feature is enabled and the value indicates the number of cursors drawn in the trail.
            /// The uiParam parameter is not used.
            /// Windows 2000/NT:  This value is not supported.
            /// </summary>
            SPI_GETMOUSETRAILS = 0x005E,

            /// <summary>
            /// Windows Me/98:  Used internally; applications should not use this flag.
            /// </summary>
            SPI_SETSCREENSAVERRUNNING = 0x0061,

            /// <summary>
            /// Same as SPI_SETSCREENSAVERRUNNING.
            /// </summary>
            SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING,
            //#endif /* WINVER >= 0x0400 */

            /// <summary>
            /// Retrieves information about the FilterKeys accessibility feature. The pvParam parameter must point to a FILTERKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(FILTERKEYS).
            /// </summary>
            SPI_GETFILTERKEYS = 0x0032,

            /// <summary>
            /// Sets the parameters of the FilterKeys accessibility feature. The pvParam parameter must point to a FILTERKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(FILTERKEYS).
            /// </summary>
            SPI_SETFILTERKEYS = 0x0033,

            /// <summary>
            /// Retrieves information about the ToggleKeys accessibility feature. The pvParam parameter must point to a TOGGLEKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(TOGGLEKEYS).
            /// </summary>
            SPI_GETTOGGLEKEYS = 0x0034,

            /// <summary>
            /// Sets the parameters of the ToggleKeys accessibility feature. The pvParam parameter must point to a TOGGLEKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(TOGGLEKEYS).
            /// </summary>
            SPI_SETTOGGLEKEYS = 0x0035,

            /// <summary>
            /// Retrieves information about the MouseKeys accessibility feature. The pvParam parameter must point to a MOUSEKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(MOUSEKEYS).
            /// </summary>
            SPI_GETMOUSEKEYS = 0x0036,

            /// <summary>
            /// Sets the parameters of the MouseKeys accessibility feature. The pvParam parameter must point to a MOUSEKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(MOUSEKEYS).
            /// </summary>
            SPI_SETMOUSEKEYS = 0x0037,

            /// <summary>
            /// Determines whether the Show Sounds accessibility flag is on or off. If it is on, the user requires an application
            /// to present information visually in situations where it would otherwise present the information only in audible form.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is on, or FALSE if it is off.
            /// Using this value is equivalent to calling GetSystemMetrics (SM_SHOWSOUNDS). That is the recommended call.
            /// </summary>
            SPI_GETSHOWSOUNDS = 0x0038,

            /// <summary>
            /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
            /// </summary>
            SPI_SETSHOWSOUNDS = 0x0039,

            /// <summary>
            /// Retrieves information about the StickyKeys accessibility feature. The pvParam parameter must point to a STICKYKEYS structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(STICKYKEYS).
            /// </summary>
            SPI_GETSTICKYKEYS = 0x003A,

            /// <summary>
            /// Sets the parameters of the StickyKeys accessibility feature. The pvParam parameter must point to a STICKYKEYS structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(STICKYKEYS).
            /// </summary>
            SPI_SETSTICKYKEYS = 0x003B,

            /// <summary>
            /// Retrieves information about the time-out period associated with the accessibility features. The pvParam parameter must point
            /// to an ACCESSTIMEOUT structure that receives the information. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(ACCESSTIMEOUT).
            /// </summary>
            SPI_GETACCESSTIMEOUT = 0x003C,

            /// <summary>
            /// Sets the time-out period associated with the accessibility features. The pvParam parameter must point to an ACCESSTIMEOUT
            /// structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(ACCESSTIMEOUT).
            /// </summary>
            SPI_SETACCESSTIMEOUT = 0x003D,

            //#if(WINVER >= 0x0400)
            /// <summary>
            /// Windows Me/98/95:  Retrieves information about the SerialKeys accessibility feature. The pvParam parameter must point
            /// to a SERIALKEYS structure that receives the information. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(SERIALKEYS).
            /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
            /// </summary>
            SPI_GETSERIALKEYS = 0x003E,

            /// <summary>
            /// Windows Me/98/95:  Sets the parameters of the SerialKeys accessibility feature. The pvParam parameter must point
            /// to a SERIALKEYS structure that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter
            /// to sizeof(SERIALKEYS).
            /// Windows Server 2003, Windows XP/2000/NT:  Not supported. The user controls this feature through the control panel.
            /// </summary>
            SPI_SETSERIALKEYS = 0x003F,
            //#endif /* WINVER >= 0x0400 */

            /// <summary>
            /// Retrieves information about the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure
            /// that receives the information. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
            /// </summary>
            SPI_GETSOUNDSENTRY = 0x0040,

            /// <summary>
            /// Sets the parameters of the SoundSentry accessibility feature. The pvParam parameter must point to a SOUNDSENTRY structure
            /// that contains the new parameters. Set the cbSize member of this structure and the uiParam parameter to sizeof(SOUNDSENTRY).
            /// </summary>
            SPI_SETSOUNDSENTRY = 0x0041,

            //#if(_WIN32_WINNT >= 0x0400)
            /// <summary>
            /// Determines whether the snap-to-default-button feature is enabled. If enabled, the mouse cursor automatically moves
            /// to the default button, such as OK or Apply, of a dialog box. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if the feature is on, or FALSE if it is off.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETSNAPTODEFBUTTON = 0x005F,

            /// <summary>
            /// Enables or disables the snap-to-default-button feature. If enabled, the mouse cursor automatically moves to the default button,
            /// such as OK or Apply, of a dialog box. Set the uiParam parameter to TRUE to enable the feature, or FALSE to disable it.
            /// Applications should use the ShowWindow function when displaying a dialog box so the dialog manager can position the mouse cursor.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETSNAPTODEFBUTTON = 0x0060,
            //#endif /* _WIN32_WINNT >= 0x0400 */

            //#if (_WIN32_WINNT >= 0x0400) || (_WIN32_WINDOWS > 0x0400)
            /// <summary>
            /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the width.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMOUSEHOVERWIDTH = 0x0062,

            /// <summary>
            /// Retrieves the width, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the width.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMOUSEHOVERWIDTH = 0x0063,

            /// <summary>
            /// Retrieves the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the height.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMOUSEHOVERHEIGHT = 0x0064,

            /// <summary>
            /// Sets the height, in pixels, of the rectangle within which the mouse pointer has to stay for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. Set the uiParam parameter to the new height.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMOUSEHOVERHEIGHT = 0x0065,

            /// <summary>
            /// Retrieves the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. The pvParam parameter must point to a UINT variable that receives the time.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMOUSEHOVERTIME = 0x0066,

            /// <summary>
            /// Sets the time, in milliseconds, that the mouse pointer has to stay in the hover rectangle for TrackMouseEvent
            /// to generate a WM_MOUSEHOVER message. This is used only if you pass HOVER_DEFAULT in the dwHoverTime parameter in the call to TrackMouseEvent. Set the uiParam parameter to the new time.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMOUSEHOVERTIME = 0x0067,

            /// <summary>
            /// Retrieves the number of lines to scroll when the mouse wheel is rotated. The pvParam parameter must point
            /// to a UINT variable that receives the number of lines. The default value is 3.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETWHEELSCROLLLINES = 0x0068,

            /// <summary>
            /// Sets the number of lines to scroll when the mouse wheel is rotated. The number of lines is set from the uiParam parameter.
            /// The number of lines is the suggested number of lines to scroll when the mouse wheel is rolled without using modifier keys.
            /// If the number is 0, then no scrolling should occur. If the number of lines to scroll is greater than the number of lines viewable,
            /// and in particular if it is WHEEL_PAGESCROLL (#defined as UINT_MAX), the scroll operation should be interpreted
            /// as clicking once in the page down or page up regions of the scroll bar.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETWHEELSCROLLLINES = 0x0069,

            /// <summary>
            /// Retrieves the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is
            /// over a submenu item. The pvParam parameter must point to a DWORD variable that receives the time of the delay.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_GETMENUSHOWDELAY = 0x006A,

            /// <summary>
            /// Sets uiParam to the time, in milliseconds, that the system waits before displaying a shortcut menu when the mouse cursor is
            /// over a submenu item.
            /// Windows 95:  Not supported.
            /// </summary>
            SPI_SETMENUSHOWDELAY = 0x006B,

            /// <summary>
            /// Determines whether the IME status window is visible (on a per-user basis). The pvParam parameter must point to a BOOL variable
            /// that receives TRUE if the status window is visible, or FALSE if it is not.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETSHOWIMEUI = 0x006E,

            /// <summary>
            /// Sets whether the IME status window is visible or not on a per-user basis. The uiParam parameter specifies TRUE for on or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETSHOWIMEUI = 0x006F,
            //#endif

            //#if(WINVER >= 0x0500)
            /// <summary>
            /// Retrieves the current mouse speed. The mouse speed determines how far the pointer will move based on the distance the mouse moves.
            /// The pvParam parameter must point to an integer that receives a value which ranges between 1 (slowest) and 20 (fastest).
            /// A value of 10 is the default. The value can be set by an end user using the mouse control panel application or
            /// by an application using SPI_SETMOUSESPEED.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSESPEED = 0x0070,

            /// <summary>
            /// Sets the current mouse speed. The pvParam parameter is an integer between 1 (slowest) and 20 (fastest). A value of 10 is the default.
            /// This value is typically set using the mouse control panel application.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSESPEED = 0x0071,

            /// <summary>
            /// Determines whether a screen saver is currently running on the window station of the calling process.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if a screen saver is currently running, or FALSE otherwise.
            /// Note that only the interactive window station, "WinSta0", can have a screen saver running.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETSCREENSAVERRUNNING = 0x0072,

            /// <summary>
            /// Retrieves the full path of the bitmap file for the desktop wallpaper. The pvParam parameter must point to a buffer
            /// that receives a null-terminated path string. Set the uiParam parameter to the size, in characters, of the pvParam buffer. The returned string will not exceed MAX_PATH characters. If there is no desktop wallpaper, the returned string is empty.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETDESKWALLPAPER = 0x0073,
            //#endif /* WINVER >= 0x0500 */

            //#if(WINVER >= 0x0500)
            /// <summary>
            /// Determines whether active window tracking (activating the window the mouse is on) is on or off. The pvParam parameter must point
            /// to a BOOL variable that receives TRUE for on, or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETACTIVEWINDOWTRACKING = 0x1000,

            /// <summary>
            /// Sets active window tracking (activating the window the mouse is on) either on or off. Set pvParam to TRUE for on or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETACTIVEWINDOWTRACKING = 0x1001,

            /// <summary>
            /// Determines whether the menu animation feature is enabled. This master switch must be on to enable menu animation effects.
            /// The pvParam parameter must point to a BOOL variable that receives TRUE if animation is enabled and FALSE if it is disabled.
            /// If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade or slide animation.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETMENUANIMATION = 0x1002,

            /// <summary>
            /// Enables or disables menu animation. This master switch must be on for any menu animation to occur.
            /// The pvParam parameter is a BOOL variable; set pvParam to TRUE to enable animation and FALSE to disable animation.
            /// If animation is enabled, SPI_GETMENUFADE indicates whether menus use fade or slide animation.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETMENUANIMATION = 0x1003,

            /// <summary>
            /// Determines whether the slide-open effect for combo boxes is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE for enabled, or FALSE for disabled.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETCOMBOBOXANIMATION = 0x1004,

            /// <summary>
            /// Enables or disables the slide-open effect for combo boxes. Set the pvParam parameter to TRUE to enable the gradient effect,
            /// or FALSE to disable it.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETCOMBOBOXANIMATION = 0x1005,

            /// <summary>
            /// Determines whether the smooth-scrolling effect for list boxes is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE for enabled, or FALSE for disabled.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006,

            /// <summary>
            /// Enables or disables the smooth-scrolling effect for list boxes. Set the pvParam parameter to TRUE to enable the smooth-scrolling effect,
            /// or FALSE to disable it.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007,

            /// <summary>
            /// Determines whether the gradient effect for window title bars is enabled. The pvParam parameter must point to a BOOL variable
            /// that receives TRUE for enabled, or FALSE for disabled. For more information about the gradient effect, see the GetSysColor function.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETGRADIENTCAPTIONS = 0x1008,

            /// <summary>
            /// Enables or disables the gradient effect for window title bars. Set the pvParam parameter to TRUE to enable it, or FALSE to disable it.
            /// The gradient effect is possible only if the system has a color depth of more than 256 colors. For more information about
            /// the gradient effect, see the GetSysColor function.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETGRADIENTCAPTIONS = 0x1009,

            /// <summary>
            /// Determines whether menu access keys are always underlined. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if menu access keys are always underlined, and FALSE if they are underlined only when the menu is activated by the keyboard.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETKEYBOARDCUES = 0x100A,

            /// <summary>
            /// Sets the underlining of menu access key letters. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to always underline menu
            /// access keys, or FALSE to underline menu access keys only when the menu is activated from the keyboard.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETKEYBOARDCUES = 0x100B,

            /// <summary>
            /// Same as SPI_GETKEYBOARDCUES.
            /// </summary>
            SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES,

            /// <summary>
            /// Same as SPI_SETKEYBOARDCUES.
            /// </summary>
            SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES,

            /// <summary>
            /// Determines whether windows activated through active window tracking will be brought to the top. The pvParam parameter must point
            /// to a BOOL variable that receives TRUE for on, or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETACTIVEWNDTRKZORDER = 0x100C,

            /// <summary>
            /// Determines whether or not windows activated through active window tracking should be brought to the top. Set pvParam to TRUE
            /// for on or FALSE for off.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETACTIVEWNDTRKZORDER = 0x100D,

            /// <summary>
            /// Determines whether hot tracking of user-interface elements, such as menu names on menu bars, is enabled. The pvParam parameter
            /// must point to a BOOL variable that receives TRUE for enabled, or FALSE for disabled.
            /// Hot tracking means that when the cursor moves over an item, it is highlighted but not selected. You can query this value to decide
            /// whether to use hot tracking in the user interface of your application.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETHOTTRACKING = 0x100E,

            /// <summary>
            /// Enables or disables hot tracking of user-interface elements such as menu names on menu bars. Set the pvParam parameter to TRUE
            /// to enable it, or FALSE to disable it.
            /// Hot-tracking means that when the cursor moves over an item, it is highlighted but not selected.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETHOTTRACKING = 0x100F,

            /// <summary>
            /// Determines whether menu fade animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// when fade animation is enabled and FALSE when it is disabled. If fade animation is disabled, menus use slide animation.
            /// This flag is ignored unless menu animation is enabled, which you can do using the SPI_SETMENUANIMATION flag.
            /// For more information, see AnimateWindow.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETMENUFADE = 0x1012,

            /// <summary>
            /// Enables or disables menu fade animation. Set pvParam to TRUE to enable the menu fade effect or FALSE to disable it.
            /// If fade animation is disabled, menus use slide animation. he The menu fade effect is possible only if the system
            /// has a color depth of more than 256 colors. This flag is ignored unless SPI_MENUANIMATION is also set. For more information,
            /// see AnimateWindow.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETMENUFADE = 0x1013,

            /// <summary>
            /// Determines whether the selection fade effect is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE if disabled.
            /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out
            /// after the menu is dismissed.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETSELECTIONFADE = 0x1014,

            /// <summary>
            /// Set pvParam to TRUE to enable the selection fade effect or FALSE to disable it.
            /// The selection fade effect causes the menu item selected by the user to remain on the screen briefly while fading out
            /// after the menu is dismissed. The selection fade effect is possible only if the system has a color depth of more than 256 colors.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETSELECTIONFADE = 0x1015,

            /// <summary>
            /// Determines whether ToolTip animation is enabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE if disabled. If ToolTip animation is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTips use fade or slide animation.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETTOOLTIPANIMATION = 0x1016,

            /// <summary>
            /// Set pvParam to TRUE to enable ToolTip animation or FALSE to disable it. If enabled, you can use SPI_SETTOOLTIPFADE
            /// to specify fade or slide animation.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETTOOLTIPANIMATION = 0x1017,

            /// <summary>
            /// If SPI_SETTOOLTIPANIMATION is enabled, SPI_GETTOOLTIPFADE indicates whether ToolTip animation uses a fade effect or a slide effect.
            ///  The pvParam parameter must point to a BOOL variable that receives TRUE for fade animation or FALSE for slide animation.
            ///  For more information on slide and fade effects, see AnimateWindow.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETTOOLTIPFADE = 0x1018,

            /// <summary>
            /// If the SPI_SETTOOLTIPANIMATION flag is enabled, use SPI_SETTOOLTIPFADE to indicate whether ToolTip animation uses a fade effect
            /// or a slide effect. Set pvParam to TRUE for fade animation or FALSE for slide animation. The tooltip fade effect is possible only
            /// if the system has a color depth of more than 256 colors. For more information on the slide and fade effects,
            /// see the AnimateWindow function.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETTOOLTIPFADE = 0x1019,

            /// <summary>
            /// Determines whether the cursor has a shadow around it. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if the shadow is enabled, FALSE if it is disabled. This effect appears only if the system has a color depth of more than 256 colors.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETCURSORSHADOW = 0x101A,

            /// <summary>
            /// Enables or disables a shadow around the cursor. The pvParam parameter is a BOOL variable. Set pvParam to TRUE to enable the shadow
            /// or FALSE to disable the shadow. This effect appears only if the system has a color depth of more than 256 colors.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETCURSORSHADOW = 0x101B,

            //#if(_WIN32_WINNT >= 0x0501)
            /// <summary>
            /// Retrieves the state of the Mouse Sonar feature. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSESONAR = 0x101C,

            /// <summary>
            /// Turns the Sonar accessibility feature on or off. This feature briefly shows several concentric circles around the mouse pointer
            /// when the user presses and releases the CTRL key. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off.
            /// For more information, see About Mouse Input.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSESONAR = 0x101D,

            /// <summary>
            /// Retrieves the state of the Mouse ClickLock feature. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled, or FALSE otherwise. For more information, see About Mouse Input.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSECLICKLOCK = 0x101E,

            /// <summary>
            /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button
            /// when that button is clicked and held down for the time specified by SPI_SETMOUSECLICKLOCKTIME. The uiParam parameter specifies
            /// TRUE for on,
            /// or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSECLICKLOCK = 0x101F,

            /// <summary>
            /// Retrieves the state of the Mouse Vanish feature. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if enabled or FALSE otherwise. For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSEVANISH = 0x1020,

            /// <summary>
            /// Turns the Vanish feature on or off. This feature hides the mouse pointer when the user types; the pointer reappears
            /// when the user moves the mouse. The pvParam parameter specifies TRUE for on and FALSE for off. The default is off.
            /// For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSEVANISH = 0x1021,

            /// <summary>
            /// Determines whether native User menus have flat menu appearance. The pvParam parameter must point to a BOOL variable
            /// that returns TRUE if the flat menu appearance is set, or FALSE otherwise.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFLATMENU = 0x1022,

            /// <summary>
            /// Enables or disables flat menu appearance for native User menus. Set pvParam to TRUE to enable flat menu appearance
            /// or FALSE to disable it.
            /// When enabled, the menu bar uses COLOR_MENUBAR for the menubar background, COLOR_MENU for the menu-popup background, COLOR_MENUHILIGHT
            /// for the fill of the current menu selection, and COLOR_HILIGHT for the outline of the current menu selection.
            /// If disabled, menus are drawn using the same metrics and colors as in Windows 2000 and earlier.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFLATMENU = 0x1023,

            /// <summary>
            /// Determines whether the drop shadow effect is enabled. The pvParam parameter must point to a BOOL variable that returns TRUE
            /// if enabled or FALSE if disabled.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETDROPSHADOW = 0x1024,

            /// <summary>
            /// Enables or disables the drop shadow effect. Set pvParam to TRUE to enable the drop shadow effect or FALSE to disable it.
            /// You must also have CS_DROPSHADOW in the window class style.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETDROPSHADOW = 0x1025,

            /// <summary>
            /// Retrieves a BOOL indicating whether an application can reset the screensaver's timer by calling the SendInput function
            /// to simulate keyboard or mouse input. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if the simulated input will be blocked, or FALSE otherwise.
            /// </summary>
            SPI_GETBLOCKSENDINPUTRESETS = 0x1026,

            /// <summary>
            /// Determines whether an application can reset the screensaver's timer by calling the SendInput function to simulate keyboard
            /// or mouse input. The uiParam parameter specifies TRUE if the screensaver will not be deactivated by simulated input,
            /// or FALSE if the screensaver will be deactivated by simulated input.
            /// </summary>
            SPI_SETBLOCKSENDINPUTRESETS = 0x1027,
            //#endif /* _WIN32_WINNT >= 0x0501 */

            /// <summary>
            /// Determines whether UI effects are enabled or disabled. The pvParam parameter must point to a BOOL variable that receives TRUE
            /// if all UI effects are enabled, or FALSE if they are disabled.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETUIEFFECTS = 0x103E,

            /// <summary>
            /// Enables or disables UI effects. Set the pvParam parameter to TRUE to enable all UI effects or FALSE to disable all UI effects.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETUIEFFECTS = 0x103F,

            /// <summary>
            /// Retrieves the amount of time following user input, in milliseconds, during which the system will not allow applications
            /// to force themselves into the foreground. The pvParam parameter must point to a DWORD variable that receives the time.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000,

            /// <summary>
            /// Sets the amount of time following user input, in milliseconds, during which the system does not allow applications
            /// to force themselves into the foreground. Set pvParam to the new timeout value.
            /// The calling thread must be able to change the foreground window, otherwise the call fails.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001,

            /// <summary>
            /// Retrieves the active window tracking delay, in milliseconds. The pvParam parameter must point to a DWORD variable
            /// that receives the time.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002,

            /// <summary>
            /// Sets the active window tracking delay. Set pvParam to the number of milliseconds to delay before activating the window
            /// under the mouse pointer.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003,

            /// <summary>
            /// Retrieves the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request.
            /// The pvParam parameter must point to a DWORD variable that receives the value.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_GETFOREGROUNDFLASHCOUNT = 0x2004,

            /// <summary>
            /// Sets the number of times SetForegroundWindow will flash the taskbar button when rejecting a foreground switch request.
            /// Set pvParam to the number of times to flash.
            /// Windows NT, Windows 95:  This value is not supported.
            /// </summary>
            SPI_SETFOREGROUNDFLASHCOUNT = 0x2005,

            /// <summary>
            /// Retrieves the caret width in edit controls, in pixels. The pvParam parameter must point to a DWORD that receives this value.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETCARETWIDTH = 0x2006,

            /// <summary>
            /// Sets the caret width in edit controls. Set pvParam to the desired width, in pixels. The default and minimum value is 1.
            /// Windows NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETCARETWIDTH = 0x2007,

            //#if(_WIN32_WINNT >= 0x0501)
            /// <summary>
            /// Retrieves the time delay before the primary mouse button is locked. The pvParam parameter must point to DWORD that receives
            /// the time delay. This is only enabled if SPI_SETMOUSECLICKLOCK is set to TRUE. For more information, see About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_GETMOUSECLICKLOCKTIME = 0x2008,

            /// <summary>
            /// Turns the Mouse ClickLock accessibility feature on or off. This feature temporarily locks down the primary mouse button
            /// when that button is clicked and held down for the time specified by SPI_SETMOUSECLICKLOCKTIME. The uiParam parameter
            /// specifies TRUE for on, or FALSE for off. The default is off. For more information, see Remarks and About Mouse Input on MSDN.
            /// Windows 2000/NT, Windows 98/95:  This value is not supported.
            /// </summary>
            SPI_SETMOUSECLICKLOCKTIME = 0x2009,

            /// <summary>
            /// Retrieves the type of font smoothing. The pvParam parameter must point to a UINT that receives the information.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFONTSMOOTHINGTYPE = 0x200A,

            /// <summary>
            /// Sets the font smoothing type. The pvParam parameter points to a UINT that contains either FE_FONTSMOOTHINGSTANDARD,
            /// if standard anti-aliasing is used, or FE_FONTSMOOTHINGCLEARTYPE, if ClearType is used. The default is FE_FONTSMOOTHINGSTANDARD.
            /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise,
            /// SystemParametersInfo fails.
            /// </summary>
            SPI_SETFONTSMOOTHINGTYPE = 0x200B,

            /// <summary>
            /// Retrieves a contrast value that is used in ClearType™ smoothing. The pvParam parameter must point to a UINT
            /// that receives the information.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,

            /// <summary>
            /// Sets the contrast value used in ClearType smoothing. The pvParam parameter points to a UINT that holds the contrast value.
            /// Valid contrast values are from 1000 to 2200. The default value is 1400.
            /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise,
            /// SystemParametersInfo fails.
            /// SPI_SETFONTSMOOTHINGTYPE must also be set to FE_FONTSMOOTHINGCLEARTYPE.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFONTSMOOTHINGCONTRAST = 0x200D,

            /// <summary>
            /// Retrieves the width, in pixels, of the left and right edges of the focus rectangle drawn with DrawFocusRect.
            /// The pvParam parameter must point to a UINT.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFOCUSBORDERWIDTH = 0x200E,

            /// <summary>
            /// Sets the height of the left and right edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFOCUSBORDERWIDTH = 0x200F,

            /// <summary>
            /// Retrieves the height, in pixels, of the top and bottom edges of the focus rectangle drawn with DrawFocusRect.
            /// The pvParam parameter must point to a UINT.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_GETFOCUSBORDERHEIGHT = 0x2010,

            /// <summary>
            /// Sets the height of the top and bottom edges of the focus rectangle drawn with DrawFocusRect to the value of the pvParam parameter.
            /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
            /// </summary>
            SPI_SETFOCUSBORDERHEIGHT = 0x2011,

            /// <summary>
            /// Not implemented.
            /// </summary>
            SPI_GETFONTSMOOTHINGORIENTATION = 0x2012,

            /// <summary>
            /// Not implemented.
            /// </summary>
            SPI_SETFONTSMOOTHINGORIENTATION = 0x2013,
        }
        #endregion // SPI

        public enum SPIF
        {

            None = 0x00,
            /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
            SPIF_UPDATEINIFILE = 0x01,
            /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
            SPIF_SENDCHANGE = 0x02,
            /// <summary>Same as SPIF_SENDCHANGE.</summary>
            SPIF_SENDWININICHANGE = 0x02

        }

        /// <summary>
        /// ANIMATIONINFO specifies animation effects associated with user actions.
        /// Used with SystemParametersInfo when SPI_GETANIMATION or SPI_SETANIMATION action is specified.
        /// </summary>
        /// <remark>
        /// The uiParam value must be set to (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO)) when using this structure.
        /// </remark>
        
        [StructLayout(LayoutKind.Sequential)]
        public struct ANIMATIONINFO
        {
            /// <summary>
            /// Creates an AMINMATIONINFO structure.
            /// </summary>
            /// <param name="iMinAnimate">If non-zero and SPI_SETANIMATION is specified, enables minimize/restore animation.</param>
            public ANIMATIONINFO(Int32 iMinAnimate)
            {
                cbSize = (UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO));
                this.iMinAnimate = iMinAnimate;
            }

            /// <summary>
            /// Always must be set to (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO)).
            /// </summary>
            public UInt32 cbSize;

            /// <summary>
            /// If non-zero, minimize/restore animation is enabled, otherwise disabled.
            /// </summary>
            public Int32 iMinAnimate;
        }

        [DllImport("user32")]
        public extern static int IsWindowVisible(
            IntPtr hWnd);

        public static readonly ulong TARGETWINDOW = WS_VISIBLE;
//        public static readonly ulong TARGETWINDOW = WS_EX_APPWINDOW;

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        public static extern bool SetPriorityClass(IntPtr hProcess, uint dwPriorityClass);

        public enum DWPriorityClass
        {
            ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000,
            BELOW_NORMAL_PRIORITY_CLASS = 0x00004000,
            HIGH_PRIORITY_CLASS = 0x00000080,
            IDLE_PRIORITY_CLASS = 0x00000040,
            NORMAL_PRIORITY_CLASS = 0x00000020,
            PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000,
            PROCESS_MODE_BACKGROUND_END = 0x00200000,
            REALTIME_PRIORITY_CLASS = 0x00000100

        } ;

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 SWP_NOZORDER = 0x0004;
        public const UInt32 SWP_NOREDRAW = 0x0008;
        public const UInt32 SWP_NOACTIVATE = 0x0010;
        public const UInt32 SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
        public const UInt32 SWP_SHOWWINDOW = 0x0040;
        public const UInt32 SWP_HIDEWINDOW = 0x0080;
        public const UInt32 SWP_NOCOPYBITS = 0x0100;
        public const UInt32 SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
        public const UInt32 SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */

        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static int GWL_ID = (-12);
        public static int GWL_STYLE = (-16);
        public static int GWL_EXSTYLE = (-20);

        // Window Styles
        public const UInt32 WS_OVERLAPPED = 0;
        public const UInt32 WS_POPUP = 0x80000000;
        public const UInt32 WS_CHILD = 0x40000000;
        public const UInt32 WS_MINIMIZE = 0x20000000;
        public const UInt32 WS_VISIBLE = 0x10000000;
        public const UInt32 WS_DISABLED = 0x8000000;
        public const UInt32 WS_CLIPSIBLINGS = 0x4000000;
        public const UInt32 WS_CLIPCHILDREN = 0x2000000;
        public const UInt32 WS_MAXIMIZE = 0x1000000;
        public const UInt32 WS_CAPTION = 0xC00000;      // WS_BORDER or WS_DLGFRAME  
        public const UInt32 WS_BORDER = 0x800000;
        public const UInt32 WS_DLGFRAME = 0x400000;
        public const UInt32 WS_VSCROLL = 0x200000;
        public const UInt32 WS_HSCROLL = 0x100000;
        public const UInt32 WS_SYSMENU = 0x80000;
        public const UInt32 WS_THICKFRAME = 0x40000;
        public const UInt32 WS_GROUP = 0x20000;
        public const UInt32 WS_TABSTOP = 0x10000;
        public const UInt32 WS_MINIMIZEBOX = 0x20000;
        public const UInt32 WS_MAXIMIZEBOX = 0x10000;
        public const UInt32 WS_TILED = WS_OVERLAPPED;
        public const UInt32 WS_ICONIC = WS_MINIMIZE;
        public const UInt32 WS_SIZEBOX = WS_THICKFRAME;

        // Extended Window Styles
        public const UInt32 WS_EX_DLGMODALFRAME = 0x0001;
        public const UInt32 WS_EX_NOPARENTNOTIFY = 0x0004;
        public const UInt32 WS_EX_TOPMOST = 0x0008;
        public const UInt32 WS_EX_ACCEPTFILES = 0x0010;
        public const UInt32 WS_EX_TRANSPARENT = 0x0020;
        public const UInt32 WS_EX_MDICHILD = 0x0040;
        public const UInt32 WS_EX_TOOLWINDOW = 0x0080;
        public const UInt32 WS_EX_WINDOWEDGE = 0x0100;
        public const UInt32 WS_EX_CLIENTEDGE = 0x0200;
        public const UInt32 WS_EX_CONTEXTHELP = 0x0400;
        public const UInt32 WS_EX_RIGHT = 0x1000;
        public const UInt32 WS_EX_LEFT = 0x0000;
        public const UInt32 WS_EX_RTLREADING = 0x2000;
        public const UInt32 WS_EX_LTRREADING = 0x0000;
        public const UInt32 WS_EX_LEFTSCROLLBAR = 0x4000;
        public const UInt32 WS_EX_RIGHTSCROLLBAR = 0x0000;
        public const UInt32 WS_EX_CONTROLPARENT = 0x10000;
        public const UInt32 WS_EX_STATICEDGE = 0x20000;
        public const UInt32 WS_EX_APPWINDOW = 0x40000;
        public const UInt32 WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public const UInt32 WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        public const UInt32 WS_EX_LAYERED = 0x00080000;
        public const UInt32 WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
        public const UInt32 WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
        public const UInt32 WS_EX_COMPOSITED = 0x02000000;
        public const UInt32 WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(IntPtr pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, SHGFI uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int Shell_GetCachedImageIndex(string pwszIconPath, int iIconIndex, uint uIconFlags);

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern string PathFindFileName(string pPath);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern void SHUpdateImage(string pszHashItem, int iIndex, uint uFlags, int iImageIndex);

        /// <summary>Maximal Length of unmanaged Windows-Path-strings</summary>
        private const int MAX_PATH = 260;
        /// <summary>Maximal Length of unmanaged Typename</summary>
        private const int MAX_TYPE = 80;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero;
                iIcon = 0;
                dwAttributes = 0;
                szDisplayName = "";
                szTypeName = "";
            }
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_TYPE)]
            public string szTypeName;
        };

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        public static extern HResult SHGetSetFolderCustomSettings(ref LPSHFOLDERCUSTOMSETTINGS pfcs, string pszPath, UInt32 dwReadWrite);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct LPSHFOLDERCUSTOMSETTINGS
        {
            public UInt32 dwSize;
            public UInt32 dwMask;
            public IntPtr pvid;
            public string pszWebViewTemplate;
            public UInt32 cchWebViewTemplate;
            public string pszWebViewTemplateVersion;
            public string pszInfoTip;
            public UInt32 cchInfoTip;
            public IntPtr pclsid;
            public UInt32 dwFlags;
            public string pszIconFile;
            public UInt32 cchIconFile;
            public int iIconIndex;
            public string pszLogo;
            public UInt32 cchLogo;
        }

        public static UInt32 FCS_READ = 0x00000001;
        public static UInt32 FCS_FORCEWRITE = 0x00000002;
        public static UInt32 FCS_WRITE = FCS_READ | FCS_FORCEWRITE;

        public static UInt32 FCSM_VIEWID          =       0x00000001;    // deprecated
        public static UInt32 FCSM_WEBVIEWTEMPLATE =       0x00000002;  // deprecated
        public static UInt32 FCSM_INFOTIP         =       0x00000004;
        public static UInt32 FCSM_CLSID           =       0x00000008;
        public static UInt32 FCSM_ICONFILE        =       0x00000010;
        public static UInt32 FCSM_LOGO            =       0x00000020;
        public static UInt32 FCSM_FLAGS           =       0x00000040;

        public static UInt32 SHGFI_ICONLOCATION   =       0x000001000;



        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("user32")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        public enum SW
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            NORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            MAXIMIZE = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10,
            FORCEMINIMIZE = 11,
            MAX = 11
        }
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, String pszSubAppName, String pszSubIdList);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, int pszSubAppName, String pszSubIdList);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, String pszSubAppName, int pszSubIdList);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, int pszSubAppName, int pszSubIdList);

        [DllImport("shell32.dll")]
        static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern uint ExtractIconEx(string szFileName, int nIconIndex,
           IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        public static string GetWindowName(IntPtr Hwnd)
        {
            // This function gets the name of a window from its handle
            var Title = new StringBuilder(256);
            GetWindowText(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        

        public static Icon GetSmallIcon(string PathDLL, int Index)
        {
            IntPtr[] hDummy = new IntPtr[1] { IntPtr.Zero };
            IntPtr[] hIconEx = new IntPtr[1] { IntPtr.Zero };
            ExtractIconEx(PathDLL, Index, hDummy, hIconEx, 1);
            Icon extractedIcon = (Icon)Icon.FromHandle(hIconEx[0]).Clone();
            return extractedIcon;
        }
        public static string GetWindowClass(IntPtr Hwnd)
        {
            // This function gets the name of a window class from a window handle
            var Title = new StringBuilder(256);
            RealGetWindowClass(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        public const uint GA_PARENT = 1;
        public const uint GA_ROOT = 2;
        public const uint GA_ROOTOWNER = 3;

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetLastActivePopup(IntPtr hWnd);

        public static string GetWindowModuleFileName(IntPtr hWnd)
        {
            uint processId = 0;
            const int nChars = 1024;
            var filename = new StringBuilder(nChars);
            GetWindowThreadProcessId(hWnd, out processId);
            IntPtr hProcess = OpenProcess(1040, 0, processId);
            GetModuleFileNameEx(hProcess, IntPtr.Zero, filename, nChars);
            CloseHandle(hProcess);
            return (filename.ToString());
        }

        [ComImport, Guid("13709620-C279-11CE-A49E-444553540000")]
        public class Shell32
        {
        }

        [ComImport, Guid("D8F015C0-C278-11CE-A49E-444553540000")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface IShellDispatch
        {
            [DispId(0x60020007)]
            void MinimizeAll();

            void UndoMinimizeALL();
        }

        public sealed class Shell : IDisposable
        {
            Shell32 shell;
            IShellDispatch shellDispatch;

            public Shell()
            {
                shell = new Shell32();
                shellDispatch = (IShellDispatch)shell;
            }

            public void MinimizeAll()
            {
                if (shellDispatch == null)
                    throw new ObjectDisposedException("Shell");

                shellDispatch.MinimizeAll();
            }
            public void UndoMinimizeALL()
            {
                if (shellDispatch == null)
                    throw new ObjectDisposedException("Shell");

                shellDispatch.UndoMinimizeALL();
            }

            public void Dispose()
            {
                try
                {
                    if (shellDispatch != null)
                        Marshal.ReleaseComObject(shellDispatch);

                    if (shell != null)
                        Marshal.ReleaseComObject(shell);
                }

                finally
                {
                    shell = null;
                    shellDispatch = null;
                    GC.SuppressFinalize(this);
                }
            }
        }

        public static string Serialize<T>(T value) {

          if (value == null) {
            return null;
          }

          XmlSerializer serializer = new XmlSerializer(typeof(T));

          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8; //new UnicodeEncoding(false, false); // no BOM in a .NET string
          settings.Indent = false;
          settings.OmitXmlDeclaration = false;

          using (StringWriter textWriter = new StringWriter()) {
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings)) {
              serializer.Serialize(xmlWriter, value);
            }
            return textWriter.ToString();
          }
        }

        public static T Deserialize<T>(string xml) {

          if (string.IsNullOrEmpty(xml)) {
            return default(T);
          }

          XmlSerializer serializer = new XmlSerializer(typeof(T));

          XmlReaderSettings settings = new XmlReaderSettings();
          // No settings need modifying here

          using (TextReader textReader = new StringReader(xml)) {
            using (XmlReader xmlReader = XmlReader.Create(textReader, settings)) {
              return (T)serializer.Deserialize(xmlReader);
            }
          }
        }

        [Serializable]
        public struct FileCopyResultInfo {
          public long FileProgressValue;
          public long OveralProgressValue;
        }

        [Serializable]
        public struct FileCopySourceInfo {
          public string[] SourceFileNames;
          public string DestinationName;
        }

        #region WindowsVersion
        public enum OsVersionInfo
        {
            Unknown,
            Windows95,
            Windows98,
            Windows98SE,
            WindowsME,
            WindowsNT351,
            WindowsNT40,
            Windows2000,
            WindowsXP,
            WindowsVista,
            Windows7,
            Windows2008Server,
            Windows8
        }
        public int getOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }

        /// <summary>
        /// Gets the operating system version
        /// </summary>
        /// <returns>OsVersionInfo object that contains windows version</returns>
        public static OsVersionInfo getOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            OsVersionInfo operatingSystem = new OsVersionInfo();

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = OsVersionInfo.Windows95;
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = OsVersionInfo.Windows98SE;
                        else
                            operatingSystem = OsVersionInfo.Windows98;
                        break;
                    case 90:
                        operatingSystem = OsVersionInfo.WindowsME;
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = OsVersionInfo.WindowsNT351;
                        break;
                    case 4:
                        operatingSystem = OsVersionInfo.WindowsNT40;
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = OsVersionInfo.Windows2000;
                        else
                            operatingSystem = OsVersionInfo.WindowsXP;
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = OsVersionInfo.WindowsVista;
                        else if (vs.Minor == 1)
                            operatingSystem = OsVersionInfo.Windows7;
                        else
                            operatingSystem = OsVersionInfo.Windows8;
                        break;
                    default:
                        operatingSystem = OsVersionInfo.Unknown;
                        break;
                }
            }
            return operatingSystem;
            
        } 
        public string getOSInfoString()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        } 
        #endregion
    }

    static class WNM {
      const int NM_FIRST = 0;
      public const int NM_KILLFOCUS = (NM_FIRST - 8);
      public const int NM_CUSTOMDRAW = (NM_FIRST - 12);

      const int TTN_FIRST = -520;
      public const int TTN_SHOW = (TTN_FIRST - 1);
      public const int TTN_GETDISPINFOW = (TTN_FIRST - 10);

      const int RBN_FIRST = -831;
      public const int RBN_HEIGHTCHANGE = (RBN_FIRST - 0);
      public const int RBN_BEGINDRAG = (RBN_FIRST - 4);
      public const int RBN_ENDDRAG = (RBN_FIRST - 5);

      const int LVN_FIRST = -100;
      public const int LVN_ITEMCHANGED = (LVN_FIRST - 1);
      public const int LVN_DELETEITEM = (LVN_FIRST - 3);
      public const int LVN_BEGINDRAG = (LVN_FIRST - 9);
      public const int LVN_BEGINRDRAG = (LVN_FIRST - 11);
      public const int LVN_ITEMACTIVATE = (LVN_FIRST - 14);
      public const int LVN_ODSTATECHANGED = (LVN_FIRST - 15);
      public const int LVN_HOTTRACK = (LVN_FIRST - 21);
      public const int LVN_KEYDOWN = (LVN_FIRST - 55);
      public const int LVN_GETINFOTIP = (LVN_FIRST - 58);

      const int UDN_FIRST = -721;        // updown
      public const int UDN_DELTAPOS = (UDN_FIRST - 1);
    }

    public class FileInfoPair {
      public ShellObject source { get; set; }
      public string PathDestination { get; set; }
      public string PathDestinationRenamed { get; set; }
    }

    #region ChangeWallpaper
    public class Wallpaper
    {
        public Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched,
            Fit,
            Fill
        }

        public void Set(Uri uri, Style style)
        {
            System.IO.Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

            System.Drawing.Image img = System.Drawing.Image.FromStream(s);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }
            if (style == Style.Fill)
            {
                key.SetValue(@"WallpaperStyle", 10.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Fit)
            {
                key.SetValue(@"WallpaperStyle", 6.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            key.Close();

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    } 
    #endregion


}
