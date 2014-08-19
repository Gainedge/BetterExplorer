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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

#pragma warning disable 1591

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
		public IntPtr dwTypeData;
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
		HDM_SETITEM = HDM_FIRST + 12,
		LVM_GETSELECTEDCOUNT = (FIRST + 50),
		TVM_DELETEITEM = (0x1100 + 1),
		LVM_SETICONSPACING = FIRST + 53,
		LVM_UPDATE = FIRST + 42,
		LVM_SETBKIMAGE = (FIRST + 138),
		LVM_GETBKIMAGE = (FIRST + 139),
		LVM_FINDITEM = (FIRST + 83),
		LVM_ENSUREVISISBLE = (FIRST + 19),
		LVM_ARRANGE = (FIRST + 22),
		LVM_GETITEMINDEXRECT = (FIRST + 209),
		LVM_REMOVEALLGROUPS = (FIRST + 160),
		LVM_SETITEMINDEXSTATE = (FIRST + 210),
		LVM_GETCOLUMNORDERARRAY = (FIRST + 59),
		LVM_GETCOLUMNWIDTH = (FIRST + 29),

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



	public class User32 {
		public static readonly string UserPinnedTaskbarItemsPath = "{0}\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\TaskBar\\";
		public static readonly string UserPinnedStartMenuItemsPath = "{0}\\Microsoft\\Internet Explorer\\Quick Launch\\User Pinned\\StartMenu\\";//Changed \\Start Menu
		public static bool IsPinnedToTaskbar(string executablePath) {
			foreach (string pinnedShortcut in Directory.GetFiles(string.Format(UserPinnedTaskbarItemsPath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), "*.lnk")) {
				if (new ShellLink(pinnedShortcut).Target == executablePath)
					return true;
			}

			return false;
		}

		public static bool IsPinnedToStartMenu(string executablePath) {
			var Test = Directory.GetFiles(string.Format(UserPinnedStartMenuItemsPath, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)), "*.lnk");
			return Test.Any(pinnedShortcut => new ShellLink(pinnedShortcut).Target == executablePath);
		}
		public static void PinUnpinToTaskbar(string filePath) {
			PinUnpinTaskbar(filePath, !IsPinnedToTaskbar(filePath));
		}

		/*
		public static void UnpinFromTaskbar(string filePath) {
			PinUnpinTaskbar(filePath, false);
		}
		*/
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

		public static void PinUnpinToStartMenu(string filePath) {
			PinUnpinStartMenu(filePath, !IsPinnedToStartMenu(filePath));
		}

		/*
		public static void UnpinFromStartMenu(string filePath) {
			PinUnpinStartMenu(filePath, false);
		}
		*/

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

		/*
		public int getOSArchitecture() {
			string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
			return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
		}
		*/

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

		[DllImport("user32.dll")]
		public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);
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

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetActiveWindow(IntPtr hWnd);


		[DllImport("user32.dll")]
		public static extern bool DeleteMenu(IntPtr hMenu, int uPosition,
				MF uFlags);

		[DllImport("user32.dll")]
		public static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

		[DllImport("User32.dll")]
		public static extern HResult SendMessage(IntPtr hWnd, int msg, int wParam, LVTILEVIEWINFO lParam);

		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVGROUP2 lParam);

		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, LVFINDINFO lParam);

		[DllImport("User32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVHITTESTINFO lParam);

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


		[DllImport("user32")]
		public static extern int DestroyIcon(IntPtr hIcon);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg,
				int wParam, int lParam);

		[DllImport("user32.dll")]
		public extern static int SendMessage(IntPtr hwnd, uint msg, int count,
		[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4), In, Out]int[] orderArray);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg,
				IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg,
				ref LVITEMINDEX wParam, int lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg,
				int wParam, ref LVCOLUMN lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, MSG Msg,
				int wParam, ref HDITEM lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg,
				IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg,
				int wParam, ref RECT lparam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg,
				ref LVITEMINDEX wParam, ref RECT lparam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg,
				int wParam, ref Shell.LVITEM lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg,
				ref LVITEMINDEX wparam, ref Shell.LVITEM lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg,
				int wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, MSG Msg,
				int wParam, ref TVITEMW lParam);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern UInt32 PrivateExtractIcons(String lpszFile, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, IntPtr[] piconid, UInt32 nIcons, UInt32 flags);

		[DllImport("user32.dll")]
		public static extern bool SetMenuInfo(IntPtr hmenu,
				ref MENUINFO lpcmi);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowLong(IntPtr hwnd, int index);

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
		public static extern bool SetWindowPos(IntPtr hWnd,
				IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
				SetWindowPosFlags uFlags);

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

		[DllImport("user32.dll")]
		public static extern bool RedrawWindow(IntPtr hWnd, ref RECT lprcUpdate, IntPtr hrgnUpdate, uint flags);

		[DllImport("user32.dll")]
		public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport("user32.dll")]
		public static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

		[DllImport("user32.dll")]
		public static extern int TrackPopupMenuEx(IntPtr hmenu,
				TPM fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

		public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		public static IntPtr getWindowId(string className, string windowName) {
			return FindWindow(className, windowName);
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
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
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

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, LVSETINFOTIP lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, LVNI lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(IntPtr hWnd, int Msg, ref LVITEMINDEX wParam, LVNI lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern LVIS SendMessage(IntPtr hWnd, MSG Msg, int wParam, LVIS lParam);

		public const int WM_CHANGEUISTATE = 296;
		public const int UIS_SET = 1;
		public const int UISF_HIDEFOCUS = 0x1;

		public static short LOWORD(int dw) {
			short loWord = 0;
			ushort andResult = (ushort)(dw & 0x00007FFF);
			ushort mask = 0x8000;
			if ((dw & 0x8000) != 0) {
				loWord = (short)(mask | andResult);
			}
			else {
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

		[DllImport("user32.dll")]
		public static extern bool UpdateWindow(IntPtr hWnd);

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
			/// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
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
			/// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
			/// window is not activated.
			/// </summary>
			ShowMinNoActive = 7,
			/// <summary>
			/// Displays the window in its current size and position. This value is
			/// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
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


	}
}
