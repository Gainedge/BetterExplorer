using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace BetterExplorer
{

	public class NoSystemMenuException : Exception { }

	// Values taken from MSDN.
	public enum ItemFlags
	{ // The item ...
		mfUnchecked = 0x00000000, // ... is not checked
		mfString = 0x00000000, // ... contains a string as label
		mfDisabled = 0x00000002, // ... is disabled
		mfGrayed = 0x00000001, // ... is grayed
		mfChecked = 0x00000008, // ... is checked
		mfPopup = 0x00000010, // ... Is a popup menu. Pass the menu handle 
							  //     of the popup menu into the ID parameter.
		mfBarBreak = 0x00000020, // ... is a bar break
		mfBreak = 0x00000040, // ... is a break
		mfByPosition = 0x00000400, // ... is identified by the position
		mfByCommand = 0x00000000, // ... is identified by it's ID
		mfSeparator = 0x00000800  // ... is a seperator (String and ID parameters
								  //     are ignored).
	}

	/// <summary>
	/// A class that helps to manipulate the system menu
	/// of a passed form.
	/// 
	/// Written by Florian "nohero" Stinglmayr
	/// </summary>
	public class SystemMenu
	{
		// I havn't found any other solution than using plain old
		// WinAPI to get what I want.
		// If you need further information on these functions, their
		// parameters and their meanings you should look them up in
		// the MSDN.

		// All parameters in the [DllImport] should be self explaining.
		// NOTICE: Use never stdcall as calling convention, since Winapi is used.
		// If the underlying structure changes, your program might causing errors
		// that are hard to find.

		// At first we need the GetSystemMenu() function. 
		// This function does not have an Unicode brother
		[DllImport("USER32", EntryPoint = "GetSystemMenu", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
		private static extern IntPtr apiGetSystemMenu(IntPtr WindowHandle, int bReset);

		// And we need the AppendMenu() function. Since .NET uses Unicode
		// we pick the unicode solution.
		[DllImport("USER32", EntryPoint = "AppendMenuW", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
		private static extern int apiAppendMenu(IntPtr MenuHandle, int Flags, int NewID, String Item);

		private IntPtr m_SysMenu = IntPtr.Zero; // Handle to the System Menu

		/// <summary>Appends a separator</summary>
		public bool AppendSeparator() => AppendMenu(0, "", ItemFlags.mfSeparator);

		/// <summary>This uses the ItemFlags.mfString as default value</summary>
		public bool AppendMenu(int ID, String Item) => AppendMenu(ID, Item, ItemFlags.mfString);

		/// <summary>Superseded function.</summary>
		public bool AppendMenu(int ID, String Item, ItemFlags Flags) => apiAppendMenu(m_SysMenu, (int)Flags, ID, Item) == 0;
		
		public static SystemMenu FromWPFForm(Window form)
		{
			var cSysMenu = new SystemMenu();

			cSysMenu.m_SysMenu = apiGetSystemMenu((new WindowInteropHelper(form)).Handle, 0);
			if (cSysMenu.m_SysMenu == IntPtr.Zero)
			{ // Throw an exception on failure
				throw new NoSystemMenuException();
			}

			return cSysMenu;
		}
	}
}
