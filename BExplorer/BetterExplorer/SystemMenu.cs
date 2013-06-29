using System;
using System.Windows.Forms;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace BetterExplorer
{

    public class NoSystemMenuException : System.Exception
    {
    }

    // Values taken from MSDN.
    public enum ItemFlags
    { // The item ...
        mfUnchecked =   0x00000000, // ... is not checked
        mfString =      0x00000000, // ... contains a string as label
        mfDisabled =    0x00000002, // ... is disabled
        mfGrayed =      0x00000001, // ... is grayed
        mfChecked =     0x00000008, // ... is checked
        mfPopup =       0x00000010, // ... Is a popup menu. Pass the menu handle 
                                    //     of the popup menu into the ID parameter.
        mfBarBreak =    0x00000020, // ... is a bar break
        mfBreak =       0x00000040, // ... is a break
        mfByPosition =  0x00000400, // ... is identified by the position
        mfByCommand =   0x00000000, // ... is identified by it's ID
        mfSeparator =   0x00000800  // ... is a seperator (String and ID parameters
                                    //     are ignored).
    }

    public enum WindowMessages
    {
        wmSysCommand = 0x0112
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
        [DllImport("USER32", EntryPoint = "GetSystemMenu", SetLastError = true,
                CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr apiGetSystemMenu(IntPtr WindowHandle, int bReset);

        // And we need the AppendMenu() function. Since .NET uses Unicode
        // we pick the unicode solution.
        [DllImport("USER32", EntryPoint = "AppendMenuW", SetLastError = true,
                CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.Winapi)]
        private static extern int apiAppendMenu(IntPtr MenuHandle, int Flags, int NewID, String Item);

        // And we may also need the InsertMenu() function.
        [DllImport("USER32", EntryPoint = "InsertMenuW", SetLastError = true,
                CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.Winapi)]
        private static extern int apiInsertMenu(IntPtr hMenu, int Position, int Flags, int NewId, String Item);

        private IntPtr m_SysMenu = IntPtr.Zero; // Handle to the System Menu

        public SystemMenu()
        {
        }

        // Insert a separator at the given position index starting at zero.
        public bool InsertSeparator(int Pos)
        {
            return (InsertMenu(Pos, ItemFlags.mfSeparator | ItemFlags.mfByPosition, 0, ""));
        }

        // Simplified InsertMenu(), that assumes that Pos is relative
        // position index starting at zero
        public bool InsertMenu(int Pos, int ID, String Item)
        {
            return (InsertMenu(Pos, ItemFlags.mfByPosition | ItemFlags.mfString, ID, Item));
        }

        // Insert a menu at the given position. The value of the position depends
        // on the value of Flags. See in the article for a detail description.
        public bool InsertMenu(int Pos, ItemFlags Flags, int ID, String Item)
        {
            return (apiInsertMenu(m_SysMenu, Pos, (Int32)Flags, ID, Item) == 0);
        }

        // Appends a seperator
        public bool AppendSeparator()
        {
            return AppendMenu(0, "", ItemFlags.mfSeparator);
        }

        // This uses the ItemFlags.mfString as default value
        public bool AppendMenu(int ID, String Item)
        {
            return AppendMenu(ID, Item, ItemFlags.mfString);
        }
        // Superseded function.
        public bool AppendMenu(int ID, String Item, ItemFlags Flags)
        {
            return (apiAppendMenu(m_SysMenu, (int)Flags, ID, Item) == 0);
        }

        // Retrieves a new object from a Form object
        public static SystemMenu FromForm(Form Frm)
        {
            SystemMenu cSysMenu = new SystemMenu();

            cSysMenu.m_SysMenu = apiGetSystemMenu(Frm.Handle, 0);
            if (cSysMenu.m_SysMenu == IntPtr.Zero)
            { // Throw an exception on failure
                throw new NoSystemMenuException();
            }

            return cSysMenu;
        }

        public static SystemMenu FromWPFForm(Window form)
        {
            SystemMenu cSysMenu = new SystemMenu();

            cSysMenu.m_SysMenu = apiGetSystemMenu((new WindowInteropHelper(form)).Handle, 0);
            if (cSysMenu.m_SysMenu == IntPtr.Zero)
            { // Throw an exception on failure
                throw new NoSystemMenuException();
            }

            return cSysMenu;
        }

        // Reset's the window menu to it's default
        public static void ResetSystemMenu(Form Frm)
        {
            apiGetSystemMenu(Frm.Handle, 1);
        }

        // Checks if an ID for a new system menu item is OK or not
        public static bool VerifyItemID(int ID)
        {
            return (bool)(ID < 0xF000 && ID > 0);
        }
    }

}
