// BExplorer.Shell - A Windows Shell library for .Net.
// Copyright (C) 2007-2009 Steven J. Kirk
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either 
// version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free 
// Software Foundation, Inc., 51 Franklin Street, Fifth Floor,  
// Boston, MA 2110-1301, USA.
//
using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace BExplorer.Shell.Interop
{
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

    public enum LVSIL
    {
        LVSIL_NORMAL = 0,
        LVSIL_SMALL = 1,
        LVSIL_STATE = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENUINFO
    {
        public int cbSize;
        public MIM fMask;
        public int dwStyle;
        public int cyMax;
        public IntPtr hbrBack;
        public int dwContextHelpID;
        public int dwMenuData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENUITEMINFO
    {
        public int cbSize;
        public MIIM fMask;
        public uint fType;
        public uint fState;
        public int wID;
        public IntPtr hSubMenu;
        public int hbmpChecked;
        public int hbmpUnchecked;
        public int dwItemData;
        public string dwTypeData;
        public uint cch;
        public int hbmpItem;
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
			  FIRST = 0x1000,
        WM_COMMAND = 0x0111,
        WM_VSCROLL = 0x0115,
        LVM_SETIMAGELIST = 0x1003,
        LVM_GETITEMCOUNT = 0x1004,
        LVM_GETITEMA = 0x1005,
        LVM_EDITLABEL = 0x1017,
        TVM_SETIMAGELIST = 4361,
        TVM_SETITEMW = 4415,
			  LVM_GETITEMSTATE = (FIRST + 44),
				LVM_GETITEMRECT = (FIRST + 14),
				LVM_SETITEMSTATE = (FIRST + 43),
    }

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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct TVITEMW
    {
        public TVIF mask;
        public IntPtr hItem;
        public TVIS state;
        public TVIS stateMask;
        public string pszText;
        public int cchTextMax;
        public int iImage;
        public int iSelectedImage;
        public int cChildren;
        public int lParam;
    }

		[Flags]
		public enum CDIS
		{
			SELECTED = 0x0001,		// This flag does not work correctly for owner-drawn list-view controls that have the LVS_SHOWSELALWAYS style.
			GRAYED = 0x0002,
			DISABLED = 0x0004,
			CHECKED = 0x0008,
			FOCUS = 0x0010,
			DEFAULT = 0x0020,
			HOT = 0x0040,
			MARKED = 0x0080,
			INDETERMINATE = 0x0100,
			SHOWKEYBOARDCUES = 0x0200,
			NEARHOT = 0x0400,
			OTHERSIDEHOT = 0x0800,
			DROPHILITED = 0x1000,
		}

    class User32
    {
        [DllImport("user32.dll")]
        public static extern bool DeleteMenu(IntPtr hMenu, int uPosition,
            MF uFlags);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("user32.dll")]
        public static extern IntPtr EnumChildWindows(IntPtr parentHandle,
            Win32Callback callback, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetMenuInfo(IntPtr hmenu,
            ref MENUINFO lpcmi);

        [DllImport("user32.dll")]
        public static extern int GetMenuItemCount(IntPtr hMenu);

        [DllImport("user32.dll")]
        public static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem,
            bool fByPosition, ref MENUITEMINFO lpmii);

        [DllImport("user32.dll")]
        public static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, MSG Msg,
            int wParam, int lParam);

				[DllImport("user32.dll")]
				public static extern int SendMessage(IntPtr hWnd, int Msg,
						IntPtr wParam, IntPtr lParam);

				[DllImport("user32.dll")]
				public static extern int SendMessage(IntPtr hWnd, MSG Msg,
						int wParam, ref RECT lparam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, MSG Msg,
            int wParam, ref LVITEMA lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, MSG Msg,
            int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, MSG Msg,
            int wParam, ref TVITEMW lParam);

        [DllImport("user32.dll")]
        public static extern bool SetMenuInfo(IntPtr hmenu,
            ref MENUINFO lpcmi);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd,
            IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenuEx(IntPtr hmenu,
            TPM fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

				[DllImport("user32.dll", SetLastError = true)]
				public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,  string windowTitle);

				[StructLayout(LayoutKind.Sequential)]
				public struct RECT
				{
					public int Left;        // x position of upper-left corner
					public int Top;         // y position of upper-left corner
					public int Right;       // x position of lower-right corner
					public int Bottom;      // y position of lower-right corner
				}

				[StructLayout(LayoutKind.Sequential)]
				public struct NMHDR
				{
					// 12/24
					public IntPtr hwndFrom;
					public IntPtr idFrom;
					public long code;
				}

				[StructLayout(LayoutKind.Sequential)]
				public struct NMCUSTOMDRAW
				{
					// 48/80	
					public NMHDR hdr;
					public int dwDrawStage;
					public IntPtr hdc;
					public RECT rc;
					public IntPtr dwItemSpec;
					public CDIS uItemState;
					public IntPtr lItemlParam;
				}

				[StructLayout(LayoutKind.Sequential)]
				public struct NMLVCUSTOMDRAW
				{
					// 104/136  
					public NMCUSTOMDRAW nmcd;
					public int clrText;
					public int clrTextBk;
					public int iSubItem;
					public int dwItemType;
					public int clrFace;
					public int iIconEffect;
					public int iIconPhase;
					public int iPartId;
					public int iStateId;
					public RECT rcText;
					public int uAlign;
				}

				[DllImport("user32.dll", CharSet = CharSet.Auto)]
				public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

				public const int WM_CHANGEUISTATE = 296;
				public const int UIS_SET = 1;
				public const int UISF_HIDEFOCUS = 0x1;

				private static short LOWORD(int dw)
				{
					short loWord = 0;
					ushort andResult = (ushort)(dw & 0x00007FFF);
					ushort mask = 0x8000;
					if ((dw & 0x8000) != 0)
					{
						loWord = (short)(mask | andResult);
					}
					else
					{
						loWord = (short)andResult;
					}
					return loWord;
				}

				public static int MAKELONG(int wLow, int wHigh)
				{
					int low = (int)LOWORD(wLow);
					short high = LOWORD(wHigh);
					int product = 0x00010000 * (int)high;
					int makeLong = (int)(low | product);
					return makeLong;
				}





    }
}
