//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsAPICodePack.Shell {
	public static class ShellNativeMethods {

		#region Shell Helper Methods

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHCreateShellItemArrayFromDataObject(
			System.Runtime.InteropServices.ComTypes.IDataObject pdo,
			ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out IShellItemArray iShellItemArray);

		/*
		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHCreateShellItemArrayFromShellItem(
			IShellItem pdo,
			ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out IShellItemArray iShellItemArray);
		*/

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int SHCreateItemFromParsingName(
			[MarshalAs(UnmanagedType.LPWStr)] string path,
			// The following parameter is not used - binding context.
			IntPtr pbc,
			ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out IShellItem2 shellItem);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int SHCreateItemFromParsingName(
			[MarshalAs(UnmanagedType.LPWStr)] string path,
			// The following parameter is not used - binding context.
			IntPtr pbc,
			ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int PathParseIconLocation(
			[MarshalAs(UnmanagedType.LPWStr)] ref string pszIconFile);


		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHCreateItemFromIDList(
			/*PCIDLIST_ABSOLUTE*/ IntPtr pidl,
			ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out IShellItem2 ppv);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHParseDisplayName(
			[MarshalAs(UnmanagedType.LPWStr)] string pszName,
			IntPtr pbc,
			out IntPtr ppidl,
			ShellFileGetAttributesOptions sfgaoIn,
			out ShellFileGetAttributesOptions psfgaoOut
		);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHGetIDListFromObject(IntPtr iUnknown,
			out IntPtr ppidl
		);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHGetDesktopFolder(
			[MarshalAs(UnmanagedType.Interface)] out IShellFolder ppshf
		);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int SHCreateShellItem(
			IntPtr pidlParent,
			[In, MarshalAs(UnmanagedType.Interface)] IShellFolder psfParent,
			IntPtr pidl,
			[MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi
		);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint ILGetSize(IntPtr pidl);

		[DllImport("shell32.dll", CharSet = CharSet.None)]
		public static extern void ILFree(IntPtr pidl);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteObject(IntPtr hObject);

		#endregion

		#region Shell Library Helper Methods

		[DllImport("Shell32", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		internal static extern int SHShowManageLibraryUI(
			[In, MarshalAs(UnmanagedType.Interface)] IShellItem library,
			[In] IntPtr hwndOwner,
			[In] string title,
			[In] string instruction,
			[In] BExplorer.Shell.Interop.LibraryManageDialogOptions lmdOptions);

		#endregion

		#region Command Link Definitions

		//internal const int CommandLink = 0x0000000E;
		//internal const uint SetNote = 0x00001609;
		//internal const uint GetNote = 0x0000160A;
		//internal const uint GetNoteLength = 0x0000160B;
		//internal const uint SetShield = 0x0000160C;

		#endregion

		#region Shell notification definitions
		//internal const int MaxPath = 260;

		/*
		[DllImport("shell32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath);
		*/

		[StructLayout(LayoutKind.Sequential)]
		internal struct ShellNotifyStruct {
			internal IntPtr item1;
			internal IntPtr item2;
		};

		[StructLayout(LayoutKind.Sequential)]
		internal struct SHChangeNotifyEntry {
			internal IntPtr pIdl;

			[MarshalAs(UnmanagedType.Bool)]
			internal bool recursively;
		}

		[DllImport("shell32.dll")]
		internal static extern uint SHChangeNotifyRegister(
			IntPtr windowHandle,
			ShellChangeNotifyEventSource sources,
			ShellObjectChangeTypes events,
			uint message,
			int entries,
			ref SHChangeNotifyEntry changeNotifyEntry);

		[DllImport("shell32.dll")]
		internal static extern IntPtr SHChangeNotification_Lock(
			IntPtr windowHandle,
			int processId,
			out IntPtr pidl,
			out uint lEvent);

		[DllImport("shell32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern Boolean SHChangeNotification_Unlock(IntPtr hLock);

		[DllImport("shell32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern Boolean SHChangeNotifyDeregister(uint hNotify);

		[Flags]
		internal enum ShellChangeNotifyEventSource {
			InterruptLevel = 0x0001,
			ShellLevel = 0x0002,
			RecursiveInterrupt = 0x1000,
			NewDelivery = 0x8000
		}



		#endregion

		#region IContextMenu
		[DllImport("user32.dll")]
		public static extern bool SetMenuInfo(IntPtr hmenu,
			ref BExplorer.Shell.Interop.MENUINFO lpcmi);

		[DllImport("user32.dll")]
		public static extern bool GetMenuInfo(IntPtr hmenu,
			ref BExplorer.Shell.Interop.MENUINFO lpcmi);

		[DllImport("user32.dll")]
		public static extern int GetMenuItemCount(IntPtr hMenu);

		[DllImport("user32.dll")]
		public static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem,
			bool fByPosition, ref MENUITEMINFO lpmii);

		[DllImport("user32.dll")]
		public static extern bool DeleteMenu(IntPtr hMenu, int uPosition,
			MF uFlags);

		[DllImport("user32.dll")]
		public static extern int TrackPopupMenuEx(IntPtr hmenu,
			BExplorer.Shell.Interop.TPM fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);
		#endregion
		internal const int InPlaceStringTruncated = 0x00401A0;

		/*
		[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
		public static extern int GetObjectBitmap(IntPtr hObject, int nCount, ref BITMAP lpObject);
		*/
	}
}
