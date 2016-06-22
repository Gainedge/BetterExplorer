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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Microsoft.API;
using Point = System.Drawing.Point;

namespace BExplorer.Shell.Interop {
	[Flags]
	public enum LVIF : int {
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
	public enum LVIS : int {
		LVIS_FOCUSED = 0x0001,
		LVIS_SELECTED = 0x0002,
		LVIS_CUT = 0x0004,
		LVIS_DROPHILITED = 0x0008,
		LVIS_ACTIVATING = 0x0020,
		LVIS_OVERLAYMASK = 0x0F00,
		LVIS_STATEIMAGEMASK = 0xF000,
	}

	/*
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct LVITEM {
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
*/

	public enum LVSIL {
		LVSIL_NORMAL = 0,
		LVSIL_SMALL = 1,
		LVSIL_STATE = 2,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MENUINFO {
		public int cbSize;
		public MIM fMask;
		public int dwStyle;
		public int cyMax;
		public IntPtr hbrBack;
		public int dwContextHelpID;
		public int dwMenuData;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MENUITEMINFO {
		public uint cbSize;
		public MIIM fMask;
		public uint fType;
		public uint fState;
		public int wID;
		public IntPtr hSubMenu;
		public IntPtr hbmpChecked;
		public IntPtr hbmpUnchecked;
		public IntPtr dwItemData;
		public String dwTypeData;
		public uint cch;
		public IntPtr hbmpItem;
	}

	public enum MF {
		MF_BYCOMMAND = 0x00000000,
		MF_BYPOSITION = 0x00000400,
	}

	public enum MIIM : uint {
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

	public enum MIM : uint {
		MIM_MAXHEIGHT = 0x00000001,
		MIM_BACKGROUND = 0x00000002,
		MIM_HELPID = 0x00000004,
		MIM_MENUDATA = 0x00000008,
		MIM_STYLE = 0x00000010,
		MIM_APPLYTOSUBMENUS = 0x80000000,
	}

	/*
public enum MK {
	MK_LBUTTON = 0x0001,
	MK_RBUTTON = 0x0002,
	MK_SHIFT = 0x0004,
	MK_CONTROL = 0x0008,
	MK_MBUTTON = 0x0010,
	MK_ALT = 0x1000,
}
*/

	public enum MSG {
		FIRST = 0x1000,
		WM_COMMAND = 0x0111,
		WM_VSCROLL = 0x0115,
		LVM_GETITEMCOUNT = 0x1004,
		LVM_GETITEMW = (FIRST + 75),
		LVM_EDITLABELW = 0x1076,//(FIRST + 118),
		LVM_SETITEMW = (FIRST + 76),
		TVM_SETIMAGELIST = 4361,
		TVM_SETITEMW = 4415,
		LVM_GETITEMSTATE = (FIRST + 44),
		LVM_GETITEMRECT = (FIRST + 14),
		LVM_SETITEMSTATE = (FIRST + 43),
		LVM_INSERTCOLUMN = (FIRST + 97),
		LVM_SETCOLUMN = (FIRST + 96),
		LVM_SETCOLUMNWIDTH = (FIRST + 30),
		LVM_DELETECOLUMN = (FIRST + 28),
		LVM_REDRAWITEMS = (FIRST + 21),
		LVM_SETIMAGELIST = (FIRST + 3),
		LVM_SETITEMCOUNT = 4143,
		LVM_SetExtendedStyle = (FIRST + 54),
		LVM_GetExtendedStyle = (FIRST + 55),
		LVM_ISITEMVISIBLE = (FIRST + 182),
		LVM_SETVIEW = (FIRST + 142),
		LVM_SETTILEVIEWINFO = (FIRST + 162),
		LVM_GETHEADER = (FIRST + 31),
		HDM_FIRST = 0x1200,
		HDM_GETITEM = HDM_FIRST + 11,
		HDM_GETITEMDROPDOWNRECT = HDM_FIRST + 25,
		HDM_SETITEM = HDM_FIRST + 12,
		LVM_GETSELECTEDCOUNT = (FIRST + 50),
		TVM_DELETEITEM = (0x1100 + 1),
		LVM_SETICONSPACING = FIRST + 53,
		LVM_UPDATE = FIRST + 42,
		LVM_SETBKIMAGE = (FIRST + 138),
		LVM_GETBKIMAGE = (FIRST + 139),
		LVM_FINDITEM = (FIRST + 83),
		LVM_ENSUREVISIBLE = (FIRST + 19),
		LVM_ARRANGE = (FIRST + 22),
		LVM_GETITEMINDEXRECT = (FIRST + 209),
		LVM_REMOVEALLGROUPS = (FIRST + 160),
		LVM_SETITEMINDEXSTATE = (FIRST + 210),
		LVM_GETCOLUMNORDERARRAY = (FIRST + 59),
		LVM_GETCOLUMNWIDTH = (FIRST + 29),
		TVM_SETHOT = 0x1100 + 58,
		LVM_SETSELECTEDCOLUMN = (FIRST + 140),

	}

	public enum LV_VIEW : int {
		LV_VIEW_DETAILS = 1,
		LV_VIEW_ICON = 0,
		LV_VIEW_LIST = 3,
		LV_VIEW_MAX = 4,
		LV_VIEW_SMALLICON = 2,
		LV_VIEW_TILE = 4,
	}

	[Flags]
	public enum CMIC : uint {
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
	public enum TPM {
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
	public enum TVIF {
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
	public enum TVIS {
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
	public struct TVITEMW {
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
	public enum CDIS {
		SELECTED = 0x0001,      // This flag does not work correctly for owner-drawn list-view controls that have the LVS_SHOWSELALWAYS style.
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

	public class User32 {

		/// <summary>
		/// Retrieves the cursor's position, in screen coordinates.
		/// </summary>
		/// <see>See MSDN documentation for further information.</see>
		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT lpPoint);

		public static Point GetCursorPosition() {
			POINT lpPoint;
			GetCursorPos(out lpPoint);
			//bool success = User32.GetCursorPos(out lpPoint);
			// if (!success)

			return new Point(lpPoint.x, lpPoint.y);// lpPoint;
		}

		[DllImport("user32.dll")]
		public static extern uint GetMenuItemID(IntPtr hMenu, int nPos);

		private const string UserPinnedTaskbarItemsPath = "{0}\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar\\";
		private const string UserPinnedStartMenuItemsPath = "{0}\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\StartMenu\\";
		public static bool IsPinnedToTaskbar(string executablePath) {
			var Test = Directory.GetFiles(string.Format(UserPinnedTaskbarItemsPath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), "*.lnk");
			return Test.Any(pinnedShortcut => new ShellLink(pinnedShortcut).Target == executablePath);
		}

		public static bool IsPinnedToStartMenu(string executablePath) {
			var Test = Directory.GetFiles(string.Format(UserPinnedStartMenuItemsPath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), "*.lnk");
			return Test.Any(pinnedShortcut => new ShellLink(pinnedShortcut).Target == executablePath);
		}
		public static void PinUnpinToTaskbar(string filePath) {
			//PinUnpinTaskbar(filePath, !IsPinnedToTaskbar(filePath));
			var pin = IsPinnedToTaskbar(filePath);

			if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

			// create the shell application object
			dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

			string path = Path.GetDirectoryName(filePath);
			string fileName = Path.GetFileName(filePath);

			dynamic directory = shellApplication.NameSpace(path);
			dynamic link = directory.ParseName(fileName);

			dynamic verbs = link.Verbs();
			for (int i = 0; i < verbs.Count(); i++) {
				dynamic verb = verbs.Item(i);
				string verbName = verb.Name.Replace("&", string.Empty).ToLower();

				if ((pin && verbName.Equals(LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 5386, "pin to taskbar").Replace("&", string.Empty).ToLower()))
				|| (!pin && verbName.Equals(LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 5387, "unpin from taskbar").Replace("&", string.Empty).ToLower()))
				) {
					verb.DoIt();
				}
			}
		}

		/*
private static void PinUnpinTaskbar(string filePath, bool pin) {
	if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

	// create the shell application object
	dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

	string path = Path.GetDirectoryName(filePath);
	string fileName = Path.GetFileName(filePath);

	dynamic directory = shellApplication.NameSpace(path);
	dynamic link = directory.ParseName(fileName);

	dynamic verbs = link.Verbs();
	for (int i = 0; i < verbs.Count(); i++) {
		dynamic verb = verbs.Item(i);
		string verbName = verb.Name.Replace(@"&", string.Empty).ToLower();

		if ((pin && verbName.Equals(LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 5386, "pin to taskbar").Replace(@"&", string.Empty).ToLower()))
		|| (!pin && verbName.Equals(LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 5387, "unpin from taskbar").Replace(@"&", string.Empty).ToLower()))
		) {
			verb.DoIt();
		}
	}

	shellApplication = null;
}
*/

		public static void PinUnpinToStartMenu(string filePath) {
			//PinUnpinStartMenu(filePath, !IsPinnedToStartMenu(filePath));
			var pin = IsPinnedToStartMenu(filePath);

			if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

			// create the shell application object
			dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

			string path = Path.GetDirectoryName(filePath);
			string fileName = Path.GetFileName(filePath);

			dynamic directory = shellApplication.NameSpace(path);
			dynamic link = directory.ParseName(fileName);

			dynamic verbs = link.Verbs();
			for (int i = 0; i < verbs.Count(); i++) {
				dynamic verb = verbs.Item(i);
				string verbName = verb.Name.Replace("&", string.Empty).ToLower();

				if ((pin && verbName.Equals("pin to start menu")) || (!pin && verbName.Equals("unpin from start menu"))) {
					verb.DoIt();
				}
			}

			shellApplication = null;
		}

		/*
public static void UnpinFromStartMenu(string filePath) {
	PinUnpinStartMenu(filePath, false);
}
*/
		/*
private static void PinUnpinStartMenu(string filePath, bool pin) {
	if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

	// create the shell application object
	dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

	string path = Path.GetDirectoryName(filePath);
	string fileName = Path.GetFileName(filePath);

	dynamic directory = shellApplication.NameSpace(path);
	dynamic link = directory.ParseName(fileName);

	dynamic verbs = link.Verbs();
	for (int i = 0; i < verbs.Count(); i++) {
		dynamic verb = verbs.Item(i);
		string verbName = verb.Name.Replace(@"&", string.Empty).ToLower();

		if ((pin && verbName.Equals("pin to start menu")) || (!pin && verbName.Equals("unpin from start menu"))) {
			verb.DoIt();
		}
	}

	shellApplication = null;
}
*/

		/*
public enum OsVersionInfo {
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
*/

		/*
public int getOSArchitecture() {
	string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
	return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
}
*/

		/*
/// <summary>
/// Gets the operating system version
/// </summary>
/// <returns>OsVersionInfo object that contains windows version</returns>
public static OsVersionInfo getOSInfo() {
	//Get Operating system information.
	OperatingSystem os = Environment.OSVersion;
	//Get version information about the os.
	Version vs = os.Version;

	//Variable to hold our return value
	OsVersionInfo operatingSystem = new OsVersionInfo();

	if (os.Platform == PlatformID.Win32Windows) {
		//This is a pre-NT version of Windows
		switch (vs.Minor) {
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
	else if (os.Platform == PlatformID.Win32NT) {
		switch (vs.Major) {
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
*/

		/*
[DllImport("user32.dll")]
public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);
*/

		/// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();
		// For Windows Mobile, replace user32.dll with coredll.dll


		/// <summary>
		/// Gives focus to a given window.
		/// </summary>
		/// <param name="hWnd"></param>
		/// <returns></returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
		public static extern IntPtr SetFocus(IntPtr hWnd);

		/*
[DllImport("user32.dll", SetLastError = true)]
public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

[DllImport("user32.dll", SetLastError = true)]
public static extern IntPtr SetActiveWindow(IntPtr hWnd);
*/


		[DllImport("user32.dll")]
		public static extern bool DeleteMenu(IntPtr hMenu, int uPosition, MF uFlags);

		/*
[DllImport("user32.dll")]
public static extern bool DestroyWindow(IntPtr hWnd);

[DllImport("user32.dll")]
public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
*/

		[DllImport("user32", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
		public static extern bool SetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPTStr)] string text);

		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SendMessage(IntPtr hWnd, int msg, int wParam, LVTILEVIEWINFO lParam);


		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUP2 lParam);

		/*
[DllImport("User32.dll")]
public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, LVFINDINFO lParam);
*/

		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVHITTESTINFO lParam);

		/*
[DllImport("user32.dll")]
public static extern IntPtr EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);
*/

		[DllImport("user32.dll")]
		public static extern bool GetMenuInfo(IntPtr hmenu, ref MENUINFO lpcmi);

		[DllImport("user32.dll")]
		public static extern int GetMenuItemCount(IntPtr hMenu);

		[DllImport("user32.dll")]
		public static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem, bool fByPosition, ref MENUITEMINFO lpmii);

		/*
[DllImport("user32.dll")]
public static extern uint RegisterClipboardFormat(string lpszFormat);
*/

		[DllImport("user32")]
		public static extern int DestroyIcon(IntPtr hIcon);
		[DllImport("user32.dll")]
		public static extern IntPtr CreatePopupMenu();

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool AppendMenu(IntPtr hMenu, MenuFlags uFlags, uint uIDNewItem, string lpNewItem);

		[Flags]
		public enum MenuFlags : uint {
			MF_STRING = 0,
			MF_BYPOSITION = 0x400,
			MF_SEPARATOR = 0x800,
			MF_REMOVE = 0x1000,
			MF_POPUP = 0x00000010,
		}

		[DllImport("user32.dll")]
		public static extern bool InsertMenuItem(IntPtr hMenu, uint uItem, bool fByPosition, [In] ref MENUITEMINFO lpmii);

		[DllImport("user32.dll")]
		public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);

		[DllImport("user32.dll")]
		public static extern bool DestroyMenu(IntPtr hMenu);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg, int wParam, int lParam);

		[DllImport("user32.dll")]
		public extern static int SendMessage(IntPtr hwnd, uint msg, int count, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4), In, Out]int[] orderArray);

		/*
[DllImport("user32.dll")]
public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg, IntPtr wParam, IntPtr lParam);

[DllImport("user32.dll")]
public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg, ref LVITEMINDEX wParam, int lParam);
*/

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg, int wParam, ref LVCOLUMN lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg, int wParam, ref HDITEM lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, ref Guid wParam, IntPtr lParam);

		/*
[DllImport("user32.dll")]
public static extern int SendMessage(IntPtr hWnd, MSG Msg, int wParam, ref RECT lparam);
*/

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg, ref LVITEMINDEX wParam, ref RECT lparam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg, int wParam, ref RECT lparam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg, int wParam, ref Shell.LVITEM lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg, ref LVITEMINDEX wparam, ref Shell.LVITEM lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg, int wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg, int wParam, ref TVITEMW lParam);

		/*
[DllImport("User32.dll", CharSet = CharSet.Auto)]
public static extern UInt32 PrivateExtractIcons(String lpszFile, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, IntPtr[] piconid, UInt32 nIcons, UInt32 flags);
*/

		[DllImport("user32.dll")]
		public static extern bool SetMenuInfo(IntPtr hmenu, ref MENUINFO lpcmi);

		/*
[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
*/

		/*
[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
public static extern int GetWindowLong(IntPtr hwnd, int index);
*/
		public struct WINDOWPOS {
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public uint flags;
		}

		[Flags()]
		public enum SetWindowPosFlags : uint {
			/// <summary>If the calling thread and the thread that owns the window are attached to different input queues,
			/// the system posts the request to the thread that owns the window. This prevents the calling thread from
			/// blocking its execution while other threads process the request.</summary>
			/// <remarks>SWP_ASYNCWINDOWPOS</remarks>
			AsynchronousWindowPosition = 0x4000,
			/// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
			/// <remarks>SWP_DEFERERASE</remarks>
			DeferErase = 0x2000,
			/// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
			/// <remarks>SWP_DRAWFRAME</remarks>
			DrawFrame = 0x0020,
			/// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to
			/// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE
			/// is sent only when the window's size is being changed.</summary>
			/// <remarks>SWP_FRAMECHANGED</remarks>
			FrameChanged = 0x0020,
			/// <summary>Hides the window.</summary>
			/// <remarks>SWP_HIDEWINDOW</remarks>
			HideWindow = 0x0080,
			/// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the
			/// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
			/// parameter).</summary>
			/// <remarks>SWP_NOACTIVATE</remarks>
			DoNotActivate = 0x0010,
			/// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid
			/// contents of the client area are saved and copied back into the client area after the window is sized or
			/// repositioned.</summary>
			/// <remarks>SWP_NOCOPYBITS</remarks>
			DoNotCopyBits = 0x0100,
			/// <summary>Retains the current position (ignores X and Y parameters).</summary>
			/// <remarks>SWP_NOMOVE</remarks>
			IgnoreMove = 0x0002,
			/// <summary>Does not change the owner window's position in the Z order.</summary>
			/// <remarks>SWP_NOOWNERZORDER</remarks>
			DoNotChangeOwnerZOrder = 0x0200,
			/// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to
			/// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent
			/// window uncovered as a result of the window being moved. When this flag is set, the application must
			/// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
			/// <remarks>SWP_NOREDRAW</remarks>
			DoNotRedraw = 0x0008,
			/// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
			/// <remarks>SWP_NOREPOSITION</remarks>
			DoNotReposition = 0x0200,
			/// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
			/// <remarks>SWP_NOSENDCHANGING</remarks>
			DoNotSendChangingEvent = 0x0400,
			/// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
			/// <remarks>SWP_NOSIZE</remarks>
			IgnoreResize = 0x0001,
			/// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
			/// <remarks>SWP_NOZORDER</remarks>
			IgnoreZOrder = 0x0004,
			/// <summary>Displays the window.</summary>
			/// <remarks>SWP_SHOWWINDOW</remarks>
			ShowWindow = 0x0040,
		}

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		/// <summary>
		/// The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative to the upper-left corner of the parent window's client area.
		/// </summary>
		/// <param name="hWnd">Handle to the window.</param>
		/// <param name="X">Specifies the new position of the left side of the window.</param>
		/// <param name="Y">Specifies the new position of the top of the window.</param>
		/// <param name="nWidth">Specifies the new width of the window.</param>
		/// <param name="nHeight">Specifies the new height of the window.</param>
		/// <param name="bRepaint">Specifies whether the window is to be repainted. If this parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of moving a child window.</param>
		/// <returns>If the function succeeds, the return value is nonzero.
		/// <para>If the function fails, the return value is zero. To get extended error information, call GetLastError.</para></returns>
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		/*
		[DllImport("user32.dll")]
		public static extern bool RedrawWindow(IntPtr hWnd, ref RECT lprcUpdate, IntPtr hrgnUpdate, uint flags);
		*/

		//[DllImport("user32.dll")]
		//public static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

		//[DllImport("user32.dll")]
		//public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport("user32.dll")]
		public static extern void PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		/*
		[DllImport("user32.dll")]
		public static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);
		*/

		[DllImport("user32.dll")]
		public static extern int TrackPopupMenuEx(IntPtr hmenu, TPM fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		/*
		public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);
		*/

		/*
[DllImport("user32.dll", SetLastError = true)]
public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
*/

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		//public static IntPtr getWindowId(string className, string windowName) {
		//	return FindWindow(className, windowName);
		//}

		//For use with WM_COPYDATA and COPYDATASTRUCT
		[DllImport("User32.dll", EntryPoint = "SendMessage")]
		public static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

		////For use with WM_COPYDATA and COPYDATASTRUCT
		//[DllImport("User32.dll", EntryPoint = "PostMessage")]
		//public static extern int PostMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

		[StructLayout(LayoutKind.Sequential)]
		public struct COPYDATASTRUCT {
			public IntPtr dwData;
			public int cbData;
			public IntPtr lpData;
			//[MarshalAs(UnmanagedType.LPStr)]
			//public string lpUserName;
			//[MarshalAs(UnmanagedType.LPStr)]
			//public string lpDomain;
			//[MarshalAs(UnmanagedType.LPStr)]
			//public string lpShare;
			//[MarshalAs(UnmanagedType.LPStr)]
			//public string lpSharingName;
			//[MarshalAs(UnmanagedType.LPStr)]
			//public string lpDescription;
		}
		public const int WM_COPYDATA = 0x4A;

		[StructLayout(LayoutKind.Sequential)]
		public struct ShareInfo {
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

		//public static int SendWindowsStringMessage(int hWnd, int wParam, string msg,
		//				string Share = "", string ShareName = "", string Description = "", string Domain = "", string User = "", int Permis = 0x0) {
		//	int result = 0;

		//	if (hWnd != 0) {
		//		ShareInfo shi = new ShareInfo();
		//		shi.IsSetPermisions = Permis;
		//		shi.lpDescription = Description;
		//		shi.lpSharingName = ShareName;
		//		shi.lpUserName = User;
		//		shi.lpShare = Share;
		//		shi.lpDomain = Domain;
		//		shi.lpMsg = msg;

		//		IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(shi));
		//		Marshal.StructureToPtr(shi, p, false);
		//		string str = msg + ";" + Share + ";" + ShareName + ";" + Domain +
		//				";" + User + ";" + Description;
		//		byte[] sarr = System.Text.Encoding.Default.GetBytes(str);
		//		int len = Marshal.SizeOf(shi);//sarr.Length;
		//		COPYDATASTRUCT cds = new COPYDATASTRUCT();
		//		cds.dwData = IntPtr.Zero;
		//		cds.lpData = p;
		//		cds.cbData = len;
		//		int res = SendMessage(hWnd, WM_COPYDATA, wParam, ref cds);
		//		Marshal.FreeCoTaskMem(p);
		//		result = res;//SendMessage(hWnd, WM_COPYDATA, wParam, ref cds);
		//	}

		//	return result;
		//}

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

		/*
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);
		*/

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);

		[DllImport("kernel32.dll")]
		public static extern bool FreeLibrary(IntPtr hModule);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT {
			public int Left, Top, Right, Bottom;

			public RECT(int left, int top, int right, int bottom) {
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}

			public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

			public int X {
				get { return Left; }
				set { Right -= (Left - value); Left = value; }
			}

			public int Y {
				get { return Top; }
				set { Bottom -= (Top - value); Top = value; }
			}

			public int Height {
				get { return Bottom - Top; }
				set { Bottom = value + Top; }
			}

			public int Width {
				get { return Right - Left; }
				set { Right = value + Left; }
			}

			public System.Drawing.Point Location {
				get { return new System.Drawing.Point(Left, Top); }
				set { X = value.X; Y = value.Y; }
			}

			public System.Drawing.Size Size {
				get { return new System.Drawing.Size(Width, Height); }
				set { Width = value.Width; Height = value.Height; }
			}

			public static implicit operator System.Drawing.Rectangle(RECT r) {
				return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
			}

			public static implicit operator RECT(System.Drawing.Rectangle r) {
				return new RECT(r);
			}

			public static bool operator ==(RECT r1, RECT r2) {
				return r1.Equals(r2);
			}

			public static bool operator !=(RECT r1, RECT r2) {
				return !r1.Equals(r2);
			}

			public bool Equals(RECT r) {
				return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
			}

			public override bool Equals(object obj) {
				if (obj is RECT)
					return Equals((RECT)obj);
				else if (obj is System.Drawing.Rectangle)
					return Equals(new RECT((System.Drawing.Rectangle)obj));
				return false;
			}

			public override int GetHashCode() {
				return ((System.Drawing.Rectangle)this).GetHashCode();
			}

			public override string ToString() {
				return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMHDR {
			// 12/24
			public IntPtr hwndFrom;
			public IntPtr idFrom;
			public long code;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NMCUSTOMDRAW {
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
		public struct NMLVCUSTOMDRAW {
			public NMCUSTOMDRAW nmcd;
			public uint clrText;
			public uint clrTextBk;
			public int iSubItem;
			public uint dwItemType;
			public uint clrFace;
			public int iIconEffect;
			public int iIconPhase;
			public int iPartId;
			public int iStateId;
			public RECT rcText;
			public uint uAlign;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		/*
[DllImport("user32.dll", CharSet = CharSet.Auto)]
public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, LVSETINFOTIP lParam);

[DllImport("user32.dll", CharSet = CharSet.Auto)]
public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, LVNI lParam);
*/

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int Msg, ref LVITEMINDEX wParam, LVNI lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern LVIS SendMessage(IntPtr hWnd, MSG Msg, int wParam, LVIS lParam);

		/*
public const int WM_CHANGEUISTATE = 296;
public const int UIS_SET = 1;
public const int UISF_HIDEFOCUS = 0x1;
*/

		public static short LOWORD(int dw) {
			short loWord = 0;
			ushort andResult = (ushort)(dw & 0x00007FFF);
			ushort mask = 0x8000;
			if ((dw & 0x8000) != 0) {
				loWord = (short)(mask | andResult);
			} else {
				loWord = (short)andResult;
			}
			return loWord;
		}

		public static int MAKELONG(int wLow, int wHigh) {
			int low = (int)LOWORD(wLow);
			short high = LOWORD(wHigh);
			int product = 0x00010000 * (int)high;
			int makeLong = (int)(low | product);
			return makeLong;
		}

		/*
[DllImport("user32.dll")]
public static extern bool UpdateWindow(IntPtr hWnd);
*/

		[Flags]
		public enum WindowStylesEx : uint {
			/// <summary>
			/// Specifies that a window created with this style accepts drag-drop files.
			/// </summary>
			WS_EX_ACCEPTFILES = 0x00000010,
			/// <summary>
			/// Forces a top-level window onto the taskbar when the window is visible.
			/// </summary>
			WS_EX_APPWINDOW = 0x00040000,
			/// <summary>
			/// Specifies that a window has a border with a sunken edge.
			/// </summary>
			WS_EX_CLIENTEDGE = 0x00000200,
			/// <summary>
			/// Windows XP: Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
			/// </summary>
			WS_EX_COMPOSITED = 0x02000000,
			/// <summary>
			/// Includes a question mark in the title bar of the window. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window.
			/// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
			/// </summary>
			WS_EX_CONTEXTHELP = 0x00000400,
			/// <summary>
			/// The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
			/// </summary>
			WS_EX_CONTROLPARENT = 0x00010000,
			/// <summary>
			/// Creates a window that has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
			/// </summary>
			WS_EX_DLGMODALFRAME = 0x00000001,
			/// <summary>
			/// Windows 2000/XP: Creates a layered window. Note that this cannot be used for child windows. Also, this cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
			/// </summary>
			WS_EX_LAYERED = 0x00080000,
			/// <summary>
			/// Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose horizontal origin is on the right edge. Increasing horizontal values advance to the left.
			/// </summary>
			WS_EX_LAYOUTRTL = 0x00400000,
			/// <summary>
			/// Creates a window that has generic left-aligned properties. This is the default.
			/// </summary>
			WS_EX_LEFT = 0x00000000,
			/// <summary>
			/// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
			/// </summary>
			WS_EX_LEFTSCROLLBAR = 0x00004000,
			/// <summary>
			/// The window text is displayed using left-to-right reading-order properties. This is the default.
			/// </summary>
			WS_EX_LTRREADING = 0x00000000,
			/// <summary>
			/// Creates a multiple-document interface (MDI) child window.
			/// </summary>
			WS_EX_MDICHILD = 0x00000040,
			/// <summary>
			/// Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
			/// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
			/// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
			/// </summary>
			WS_EX_NOACTIVATE = 0x08000000,
			/// <summary>
			/// Windows 2000/XP: A window created with this style does not pass its window layout to its child windows.
			/// </summary>
			WS_EX_NOINHERITLAYOUT = 0x00100000,
			/// <summary>
			/// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
			/// </summary>
			WS_EX_NOPARENTNOTIFY = 0x00000004,
			/// <summary>
			/// Combines the WS_EX_CLIENTEDGE and WS_EX_WINDOWEDGE styles.
			/// </summary>
			WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
			/// <summary>
			/// Combines the WS_EX_WINDOWEDGE, WS_EX_TOOLWINDOW, and WS_EX_TOPMOST styles.
			/// </summary>
			WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
			/// <summary>
			/// The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
			/// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
			/// </summary>
			WS_EX_RIGHT = 0x00001000,
			/// <summary>
			/// Vertical scroll bar (if present) is to the right of the client area. This is the default.
			/// </summary>
			WS_EX_RIGHTSCROLLBAR = 0x00000000,
			/// <summary>
			/// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
			/// </summary>
			WS_EX_RTLREADING = 0x00002000,
			/// <summary>
			/// Creates a window with a three-dimensional border style intended to be used for items that do not accept user input.
			/// </summary>
			WS_EX_STATICEDGE = 0x00020000,
			/// <summary>
			/// Creates a tool window; that is, a window intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
			/// </summary>
			WS_EX_TOOLWINDOW = 0x00000080,
			/// <summary>
			/// Specifies that a window created with this style should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
			/// </summary>
			WS_EX_TOPMOST = 0x00000008,
			/// <summary>
			/// Specifies that a window created with this style should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
			/// To achieve transparency without these restrictions, use the SetWindowRgn function.
			/// </summary>
			WS_EX_TRANSPARENT = 0x00000020,
			/// <summary>
			/// Specifies that a window has a border with a raised edge.
			/// </summary>
			WS_EX_WINDOWEDGE = 0x00000100
		}

		/// <summary>
		/// Window Styles.
		/// The following styles can be specified wherever a window style is required. After the control has been created, these styles cannot be modified, except as noted.
		/// </summary>
		[Flags()]
		public enum WindowStyles : uint {
			/// <summary>The window has a thin-line border.</summary>
			WS_BORDER = 0x800000,

			/// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
			WS_CAPTION = 0xc00000,

			/// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
			WS_CHILD = 0x40000000,

			/// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
			WS_CLIPCHILDREN = 0x2000000,

			/// <summary>
			/// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
			/// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
			/// </summary>
			WS_CLIPSIBLINGS = 0x4000000,

			/// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
			WS_DISABLED = 0x8000000,

			/// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
			WS_DLGFRAME = 0x400000,

			/// <summary>
			/// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
			/// The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
			/// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
			/// </summary>
			WS_GROUP = 0x20000,

			/// <summary>The window has a horizontal scroll bar.</summary>
			WS_HSCROLL = 0x100000,

			/// <summary>The window is initially maximized.</summary>
			WS_MAXIMIZE = 0x1000000,

			/// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
			WS_MAXIMIZEBOX = 0x10000,

			/// <summary>The window is initially minimized.</summary>
			WS_MINIMIZE = 0x20000000,

			/// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
			WS_MINIMIZEBOX = 0x20000,

			/// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
			WS_OVERLAPPED = 0x0,

			/// <summary>The window is an overlapped window.</summary>
			WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

			/// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
			WS_POPUP = 0x80000000u,

			/// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
			WS_POPUPWINDOW = (uint)WS_POPUP | (uint)WS_BORDER | (uint)WS_SYSMENU,

			/// <summary>The window has a sizing border.</summary>
			WS_SIZEFRAME = 0x40000,

			/// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
			WS_SYSMENU = 0x80000,

			/// <summary>
			/// The window is a control that can receive the keyboard focus when the user presses the TAB key.
			/// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.  
			/// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
			/// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
			/// </summary>
			WS_TABSTOP = 0x10000,

			/// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
			WS_VISIBLE = 0x10000000,

			/// <summary>The window has a vertical scroll bar.</summary>
			WS_VSCROLL = 0x200000
		}

		public const int LVS_REPORT = 1;
		public const int LVS_EDITLABELS = 512;
		public const int LVS_OWNERDATA = 0x1000;
		public const int LVS_SHOWSELALWAYS = 0x0008;
		public const int LVS_AUTOARRANGE = 0x100;
		public const int TVS_HASBUTTONS = 0x0001;
		public const int TVS_HASLINES = 0x0002;
		public const int TVS_LINESATROOT = 0x0004;
		public const int TVS_EDITLABELS = 0x0008;
		public const int TVS_DISABLEDRAGDROP = 0x0010;
		public const int TVS_SHOWSELALWAYS = 0x0020;

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr CreateWindowEx(
				 WindowStylesEx dwExStyle,
				 string lpClassName,
				 string lpWindowName,
				 WindowStyles dwStyle,
				 int x,
				 int y,
				 int nWidth,
				 int nHeight,
				 IntPtr hWndParent,
				 IntPtr hMenu,
				 IntPtr hInstance,
				 IntPtr lpParam);

		public enum ShowWindowCommands {
			/// <summary>
			/// Hides the window and activates another window.
			/// </summary>
			Hide = 0,
			/// <summary>
			/// Activates and displays a window. If the window is minimized or
			/// maximized, the system restores it to its original size and position.
			/// An application should specify this flag when displaying the window
			/// for the first time.
			/// </summary>
			Normal = 1,
			/// <summary>
			/// Activates the window and displays it as a minimized window.
			/// </summary>
			ShowMinimized = 2,
			/// <summary>
			/// Maximizes the specified window.
			/// </summary>
			Maximize = 3, // is this the right value?
			/// <summary>
			/// Activates the window and displays it as a maximized window.
			/// </summary>      
			ShowMaximized = 3,
			/// <summary>
			/// Displays a window in its most recent size and position. This value
			/// is similar to Win32.ShowWindowCommand.Normal, except
			/// the window is not activated.
			/// </summary>
			ShowNoActivate = 4,
			/// <summary>
			/// Activates the window and displays it in its current size and position.
			/// </summary>
			Show = 5,
			/// <summary>
			/// Minimizes the specified window and activates the next top-level
			/// window in the Z order.
			/// </summary>
			Minimize = 6,
			/// <summary>
			/// Displays the window as a minimized window. This value is similar to
			/// Win32.ShowWindowCommand.ShowMinimized, except the
			/// window is not activated.
			/// </summary>
			ShowMinNoActive = 7,
			/// <summary>
			/// Displays the window in its current size and position. This value is
			/// similar to Win32.ShowWindowCommand.Show, except the
			/// window is not activated.
			/// </summary>
			ShowNA = 8,
			/// <summary>
			/// Activates and displays the window. If the window is minimized or
			/// maximized, the system restores it to its original size and position.
			/// An application should specify this flag when restoring a minimized window.
			/// </summary>
			Restore = 9,
			/// <summary>
			/// Sets the show state based on the SW_* value specified in the
			/// STARTUPINFO structure passed to the CreateProcess function by the
			/// program that started the application.
			/// </summary>
			ShowDefault = 10,
			/// <summary>
			///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
			/// that owns the window is not responding. This flag should only be
			/// used when minimizing windows from a different thread.
			/// </summary>
			ForceMinimize = 11
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd,
		out uint lpdwProcessId);

		// When you don't want the ProcessId, use this overload and pass 
		// IntPtr.Zero for the second parameter
		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd,
				IntPtr ProcessId);

		[DllImport("user32.dll")]
		public static extern bool AttachThreadInput(uint idAttach,
		uint idAttachTo, bool fAttach);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool BringWindowToTop(IntPtr hWnd);

		//[DllImport("user32.dll", SetLastError = true)]
		//public static extern bool BringWindowToTop(HandleRef hWnd);

		/// <summary>
		/// SPI_ System-wide parameter - Used in SystemParametersInfo function 
		/// </summary>
		[Description("SPI_(System-wide parameter - Used in SystemParametersInfo function )")]
		public enum SPI : uint {
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
			/// Does not work for Windows 7: http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
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

		[Flags]
		public enum SPIF {
			None = 0x00,
			/// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
			SPIF_UPDATEINIFILE = 0x01,
			/// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
			SPIF_SENDCHANGE = 0x02,
			/// <summary>Same as SPIF_SENDCHANGE.</summary>
			SPIF_SENDWININICHANGE = 0x02
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		//[DllImport("user32.dll", CharSet = CharSet.Auto)]
		//public static extern int SystemParametersInfo(int uAction, int uParam, int lpvParam, int fuWinIni);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref uint pvParam, SPIF fWinIni);
		////[DllImport("user32.dll", SetLastError = true)]
		////[return: MarshalAs(UnmanagedType.Bool)]
		////public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, IntPtr pvParam, SPIF fWinIni);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, uint pvParam, SPIF fWinIni);
		[DllImport("user32.dll")]
		static extern bool AllowSetForegroundWindow(int dwProcessId);


		//public static void AttachedThreadInputAction(Action action) {
		//	var foreThread = GetWindowThreadProcessId(GetForegroundWindow(),
		//			IntPtr.Zero);
		//	var appThread = Kernel32.GetCurrentThreadId();
		//	bool threadsAttached = false;

		//	try {
		//		threadsAttached =
		//				foreThread == appThread ||
		//				AttachThreadInput(foreThread, appThread, true);

		//		if (threadsAttached) action();
		//		else throw new ThreadStateException("AttachThreadInput failed.");
		//	} finally {
		//		if (threadsAttached)
		//			AttachThreadInput(foreThread, appThread, false);
		//	}
		//}

		//public static void ForceForegroundWindow(Window win) {
		//	var hWnd = (PresentationSource.FromVisual(win) as HwndSource).Handle;
		//	uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(),
		//			IntPtr.Zero);
		//	uint appThread = Kernel32.GetCurrentThreadId();
		//	uint lockTimeout = 0;

		//	if (foreThread != appThread) {
		//		AttachThreadInput(foreThread, appThread, true);
				
		//		var res = SystemParametersInfo(SPI.SPI_GETFOREGROUNDLOCKTIMEOUT, 0, ref lockTimeout, SPIF.None);
		//		//Debug.WriteLine("OLD: " + lockTimeout.ToString() + ", Error:" + Marshal.GetLastWin32Error().ToString());
		//		res = SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, 0, SPIF.SPIF_SENDWININICHANGE | SPIF.SPIF_UPDATEINIFILE);
		//		//Debug.WriteLine("RES: " + res.ToString());
				
		//		AllowSetForegroundWindow(Process.GetCurrentProcess().Id);
		//		//BringWindowToTop(hWnd);
		//		SetForegroundWindow(hWnd);
		//		win.Activate();
		//		win.Topmost = true;
		//		win.Topmost = false;
		//		//win.Focus();
		//		SystemParametersInfo(SPI.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, (uint)lockTimeout, SPIF.SPIF_SENDWININICHANGE | SPIF.SPIF_UPDATEINIFILE);
		//		AttachThreadInput(foreThread, appThread, false);

				
		//	} else {
		//		//win.Show();
		//		//win.Activate();
		//		//BringWindowToTop(hWnd);
		//		SetForegroundWindow(hWnd);
		//		//win.Topmost = true;
		//		//win.Topmost = false;
		//		win.Activate();
		//		win.Topmost = true;
		//		win.Topmost = false;
		//		//win.Focus();
		//	}
		//}

		/*
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetShellWindow();
		*/
	}
}
