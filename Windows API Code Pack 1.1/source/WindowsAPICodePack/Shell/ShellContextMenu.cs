// GongSolutions.Shell - A Windows Shell library for .Net.
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
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;
using FileOperations;
using System.Threading;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Controls;
using MS.WindowsAPICodePack.Internal;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Shell
{
    /// <summary>
    /// Provides support for displaying the context menu of a shell item.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// Use this class to display a context menu for a shell item, either
    /// as a popup menu, or as a main menu. 
    /// </para>
    /// 
    /// <para>
    /// To display a popup menu, simply call <see cref="ShowContextMenu"/>
    /// with the parent control and the position at which the menu should
    /// be shown.
    /// </para>
    /// 
    /// <para>
    /// To display a shell context menu in a Form's main menu, call the
    /// <see cref="Populate"/> method to populate the menu. In addition, 
    /// you must intercept a number of special messages that will be sent 
    /// to the menu's parent form. To do this, you must override 
    /// <see cref="Form.WndProc"/> like so:
    /// </para>
    /// 
    /// <code>
    ///     protected override void WndProc(ref Message m) {
    ///         if ((m_ContextMenu == null) || (!m_ContextMenu.HandleMenuMessage(ref m))) {
    ///             base.WndProc(ref m);
    ///         }
    ///     }
    /// </code>
    /// 
    /// <para>
    /// Where m_ContextMenu is the <see cref="ShellContextMenu"/> being shown.
    /// </para>
    /// 
    /// Standard menu commands can also be invoked from this class, for 
    /// example <see cref="InvokeDelete"/> and <see cref="InvokeRename"/>.
    /// </remarks>
    public class ShellContextMenu
    {

        /// <summary>
        /// Initialises a new instance of the <see cref="ShellContextMenu"/> 
        /// class.
        /// </summary>
        /// 
        /// <param name="item">
        /// The item to which the context menu should refer.
        /// </param>
        public ShellContextMenu(ShellObject item)
        {
            Initialize(new ShellObjectCollection { item });
        }
        public ShellObjectCollection SelItems = null;
        /// <summary>
        /// Initialises a new instance of the <see cref="ShellContextMenu"/> 
        /// class.
        /// </summary>
        /// 
        /// <param name="items">
        /// The items to which the context menu should refer.
        /// </param>
        public ShellContextMenu(ShellObjectCollection items)
        {
            SelItems = items;
            Initialize(items);
        }

        public ShellContextMenu(IShellView view)
        {
            Initialize(view);
        }

        /// <summary>
        /// Handles context menu messages when the <see cref="ShellContextMenu"/>
        /// is displayed on a Form's main menu bar.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// To display a shell context menu in a Form's main menu, call the
        /// <see cref="Populate"/> method to populate the menu with the shell
        /// item's menu items. In addition, you must intercept a number of
        /// special messages that will be sent to the menu's parent form. To
        /// do this, you must override <see cref="Form.WndProc"/> like so:
        /// </para>
        /// 
        /// <code>
        ///     protected override void WndProc(ref Message m) {
        ///         if ((m_ContextMenu == null) || (!m_ContextMenu.HandleMenuMessage(ref m))) {
        ///             base.WndProc(ref m);
        ///         }
        ///     }
        /// </code>
        /// 
        /// <para>
        /// Where m_ContextMenu is the <see cref="ShellContextMenu"/> being shown.
        /// </para>
        /// </remarks>
        /// 
        /// <param name="m">
        /// The message to handle.
        /// </param>
        /// 
        /// <returns>
        /// <see langword="true"/> if the message was a Shell Context Menu
        /// message, <see langword="false"/> if not. If the method returns false,
        /// then the message should be passed down to the base class's
        /// <see cref="Form.WndProc"/> method.
        /// </returns>
        public bool HandleMenuMessage(ref Message m)
        {
            if ((m.Msg == (int)WindowsHelper.WindowsAPI.MSG.WM_COMMAND) && ((int)m.WParam >= m_CmdFirst))
            {
                InvokeCommand((int)m.WParam - m_CmdFirst);
                return true;
            }
            else
            {
                if (m_ComInterface3 != null)
                {
                    IntPtr result;
                    if (m_ComInterface3.HandleMenuMsg2(m.Msg, m.WParam, m.LParam,
                        out result) == HResult.Ok)
                    {
                        m.Result = result;
                        return true;
                    }
                }
                else if (m_ComInterface2 != null)
                {
                    if (m_ComInterface2.HandleMenuMsg(m.Msg, m.WParam, m.LParam)
                            == HResult.Ok)
                    {
                        m.Result = IntPtr.Zero;
                        return true;
                    }
                }
            }
            return false;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CMINVOKECOMMANDINFO
        {
            public int cbSize;
            public int fMask;
            public IntPtr hwnd;
            public string lpVerb;
            public string lpParameters;
            public string lpDirectory;
            public int nShow;
            public int dwHotKey;
            public IntPtr hIcon;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CMINVOKECOMMANDINFO_ByIndex
        {
            public int cbSize;
            public int fMask;
            public IntPtr hwnd;
            public int iVerb;
            public string lpParameters;
            public string lpDirectory;
            public int nShow;
            public int dwHotKey;
            public IntPtr hIcon;
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

        /// <summary>
        /// Invokes the Delete command on the shell item.
        /// </summary>
        public void InvokeDelete()
        {
            CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO();
            invoke.cbSize = Marshal.SizeOf(invoke);
            invoke.lpVerb = "delete";

            try
            {
                m_ComInterface.InvokeCommand(ref invoke);
            }
            catch (COMException e)
            {
                // Ignore the exception raised when the user cancels
                // a delete operation.
                if (e.ErrorCode != unchecked((int)0x800704C7)) throw;
            }
        }

        /// <summary>
        /// Invokes the Rename command on the shell item.
        /// </summary>
        public void InvokeRename()
        {
            CMINVOKECOMMANDINFO invoke = new CMINVOKECOMMANDINFO();
            invoke.cbSize = Marshal.SizeOf(invoke);
            invoke.lpVerb = "rename";
            m_ComInterface.InvokeCommand(ref invoke);
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
        }

        /// <summary>
        /// Populates a <see cref="Menu"/> with the context menu items for
        /// a shell item.
        /// </summary>
        /// 
        /// <remarks>
        /// If this method is being used to populate a Form's main menu
        /// then you need to call <see cref="HandleMenuMessage"/> in the
        /// Form's message handler.
        /// </remarks>
        /// 
        /// <param name="menu">
        /// The menu to populate.
        /// </param>
        public void Populate(Menu menu)
        {
            RemoveShellMenuItems(menu);
            m_ComInterface.QueryContextMenu(menu.Handle, 0,
                m_CmdFirst, int.MaxValue, CMF.EXPLORE | CMF.NORMAL | CMF.CANRENAME |
                    ((Control.ModifierKeys & Keys.Shift) != 0 ? CMF.EXTENDEDVERBS : 0));
        }

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenuEx(IntPtr hmenu,
            TPM fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

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

        

        /// <summary>
        /// Shows a context menu for a shell item.
        /// </summary>
        /// 
        /// <param name="control">
        /// The parent control.
        /// </param>
        /// 
        /// <param name="pos">
        /// The position on <paramref name="control"/> that the menu
        /// should be displayed at.
        /// </param>
        public void ShowContextMenu(Control control, Point pos, IntPtr ShellViewWin)
        {
            using (ContextMenu menu = new ContextMenu())
            {
                pos = control.PointToScreen(pos);
                Populate(menu);
                int command = TrackPopupMenuEx(menu.Handle,
                    TPM.TPM_RETURNCMD, pos.X, pos.Y, m_MessageWindow.Handle,
                    IntPtr.Zero);
                if (command > 0)
                {
                    string info = string.Empty;
                    byte[] bytes = new byte[256];
                    int index;
                    //StringBuilder sb = new StringBuilder(1024);

                     m_ComInterface.GetCommandString(
                        (uint)command - m_CmdFirst,
                        (true ? WindowsHelper.WindowsAPI.GCS.VERBA : WindowsHelper.WindowsAPI.GCS.HELPTEXTA),
                        0,
                        bytes,
                        260);

                    index = 0;
                    while (index < bytes.Length - 1 && (bytes[index] != 0 || bytes[index + 1] != 0))
                    { index += 2; }

                    if (index < bytes.Length - 1)
                        info = Encoding.Default.GetString(bytes, 0, index + 1);

                    string cmd = info;

                    if (cmd.TrimEnd(Char.Parse("\0")) == "paste")
                    {
                        //StringCollection iData = Clipboard.GetFileDropList();
                        
                        ////iData.GetData(DataFormats.FileDrop);

                        //using (FileOperation fileOp = new FileOperation())
                        //{
                        //    foreach (string item in iData)
                        //    {
                        //        //MessageBox.Show(item);
                        //        string New_Name = "";
                        //        if (Path.GetExtension(item) == "")
                        //        {
                        //            ShellObject o = ShellObject.FromParsingName(item);
                        //            New_Name = o.GetDisplayName(DisplayNameType.Default);
                        //        }
                        //        else
                        //        {
                        //            New_Name = Path.GetFileName(item);
                        //        }
                        //        if (SelItems[0].IsFolder)
                        //        {
                        //            fileOp.CopyItem(item, SelItems[0].ParsingName , New_Name);
                        //        }
                        //        else
                        //        {
                        //            fileOp.CopyItem(item, Path.GetDirectoryName(SelItems[0].ParsingName), New_Name);
                        //        }
                                
                        //    }
                        //    //Thread FileopThread = new Thread(new ThreadStart(fileOp.PerformOperations));
                        //    //FileopThread.Start();
                        //    fileOp.PerformOperations();
                        //}
                        Thread FileopThread = new Thread(new ThreadStart(DoFileOP));
                        FileopThread.SetApartmentState(ApartmentState.STA);
                        //FileopThread.ApartmentState = ApartmentState.STA;
                        FileopThread.Start();
                        //DoFileOP();
                    }
                    else if (cmd.TrimEnd(Char.Parse("\0")) == "rename")
                    {
                        //InvokeRename();
                        WindowsAPI.SetFocus(ShellViewWin);
                        int itemCount = WindowsAPI.SendMessage(ShellViewWin,
                            WindowsAPI.MSG.LVM_GETITEMCOUNT, 0, 0);

                        for (int n = 0; n < itemCount; ++n)
                        {
                            WindowsAPI.LVITEMA item = new WindowsAPI.LVITEMA();
                            item.mask = WindowsAPI.LVIF.LVIF_STATE;
                            item.iItem = n;
                            item.stateMask = WindowsAPI.LVIS.LVIS_SELECTED;
                            WindowsAPI.SendMessage(ShellViewWin, WindowsAPI.MSG.LVM_GETITEMA,
                                0, ref item);

                            if (item.state != 0)
                            {
                                WindowsAPI.SendMessage(ShellViewWin, WindowsAPI.MSG.LVM_EDITLABEL, n, 0);
                            }
                        }
                        
                    }
                    else
                    {
                        InvokeCommand(command - m_CmdFirst);
                    }
                    
                }
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            DoFileOP();
        }

        void DoFileOP()
        {
            StringCollection iData = Clipboard.GetFileDropList();

            //iData.GetData(DataFormats.FileDrop);


            using (FileOperation fileOp = new FileOperation())
            {
                foreach (string item in iData)
                {
                    //MessageBox.Show(item);
                    string New_Name = "";
                    if (Path.GetExtension(item) == "")
                    {
                        ShellObject o = ShellObject.FromParsingName(item);
                        New_Name = o.GetDisplayName(DisplayNameType.Default);
                    }
                    else
                    {
                        New_Name = Path.GetFileName(item);
                    }
                    //if (SelItems[0].Properties.System.)
                    //{
                    //    fileOp.CopyItem(item, SelItems[0].ParsingName, New_Name);
                    //}
                    //else
                    //{
                    //    fileOp.CopyItem(item, Path.GetDirectoryName(SelItems[0].ParsingName), New_Name);
                    //}

                }
                //Thread FileopThread = new Thread(new ThreadStart(fileOp.PerformOperations));
                //FileopThread.Start();
                fileOp.PerformOperations();
            }
        }
        /// <summary>
        /// Gets the underlying COM <see cref="IContextMenu"/> interface.
        /// </summary>
        public WindowsHelper.WindowsAPI.IContextMenu ComInterface
        {
            get { return m_ComInterface; }
            set { m_ComInterface = value; }
        }

        [DllImport("shell32.dll", EntryPoint = "#16")]
        public static extern IntPtr ILFindLastID(IntPtr pidl);

        void Initialize(ShellObjectCollection items)
        {
            IntPtr[] pidls = new IntPtr[items.Count];
            ShellObject parent = null;
            IntPtr result;

            for (int n = 0; n < items.Count; ++n)
            {
                pidls[n] = ILFindLastID(items[n].PIDL);

                if (parent == null)
                {
                    if (items[n] == (ShellObject)KnownFolders.Desktop)
                    {
                        parent = (ShellObject)KnownFolders.Desktop;
                    }
                    else
                    {
                        parent = items[n].Parent;

                    }
                }
                else
                {
                    if (items[n].Parent != parent)
                    {
                        throw new Exception("All shell items must have the same parent");
                    }
                }
            }
            Guid ICo = typeof(WindowsHelper.WindowsAPI.IContextMenu).GUID;
            //GetIShellFolder(parent.NativeShellItem2).GetUIObjectOf(IntPtr.Zero,
            //    (uint)pidls.Length, pidls,
            //    ICo, 0, out result);

            GetIShellFolder(parent.NativeShellItem2).GetUIObjectOf(IntPtr.Zero,
                (uint)pidls.Length, pidls[0],
                ref ICo, 0, out result);
            m_ComInterface = (WindowsHelper.WindowsAPI.IContextMenu)
                Marshal.GetTypedObjectForIUnknown(result,
                    typeof(WindowsHelper.WindowsAPI.IContextMenu));
            m_ComInterface2 = m_ComInterface as WindowsHelper.WindowsAPI.IContextMenu2;
            m_ComInterface3 = m_ComInterface as WindowsHelper.WindowsAPI.IContextMenu3;
            m_MessageWindow = new MessageWindow(this);
        }

        public IShellFolder GetIShellFolder(IShellItem2 item)
        {
            IShellFolder result;

            (item).BindToHandler(IntPtr.Zero,
                BHID.SFObject, typeof(IShellFolder).GUID, out result);
           
            return result;
        }

        void Initialize(IShellView view)
        {
            object result;
            Guid Ico = typeof(WindowsHelper.WindowsAPI.IContextMenu).GUID;
            view.GetItemObject(ShellViewGetItemObject.Selection, ref Ico, out result);
            m_ComInterface = (WindowsHelper.WindowsAPI.IContextMenu)result;
            m_ComInterface2 = m_ComInterface as WindowsHelper.WindowsAPI.IContextMenu2;
            m_ComInterface3 = m_ComInterface as WindowsHelper.WindowsAPI.IContextMenu3;
            m_MessageWindow = new MessageWindow(this);
        }
 

        void InvokeCommand(int index)
        {
            const int SW_SHOWNORMAL = 1;
            CMINVOKECOMMANDINFO_ByIndex invoke = new CMINVOKECOMMANDINFO_ByIndex();
            invoke.cbSize = Marshal.SizeOf(invoke);
            invoke.iVerb = index;
            invoke.nShow = SW_SHOWNORMAL;
            m_ComInterface2.InvokeCommand(ref invoke);
        }

        [DllImport("user32.dll")]
        public static extern bool SetMenuInfo(IntPtr hmenu,
            ref MENUINFO lpcmi);
        [DllImport("user32.dll")]
        public static extern int GetMenuItemCount(IntPtr hMenu);

        void TagManagedMenuItems(Menu menu, int tag)
        {
            MENUINFO info = new MENUINFO();

            info.cbSize = Marshal.SizeOf(info);
            info.fMask = MIM.MIM_MENUDATA;
            info.dwMenuData = tag;

            foreach (MenuItem item in menu.MenuItems)
            {
                SetMenuInfo(item.Handle, ref info);
            }
        }
        [DllImport("user32.dll")]
        public static extern bool GetMenuInfo(IntPtr hmenu,
            ref MENUINFO lpcmi);


        [DllImport("user32.dll")]
        public static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem,
            bool fByPosition, ref MENUITEMINFO lpmii);

        [DllImport("user32.dll")]
        public static extern bool DeleteMenu(IntPtr hMenu, int uPosition,
            MF uFlags);

        void RemoveShellMenuItems(Menu menu)
        {
            const int tag = 0xAB;
            List<int> remove = new List<int>();
            int count = GetMenuItemCount(menu.Handle);
            MENUINFO menuInfo = new MENUINFO();
            MENUITEMINFO itemInfo = new MENUITEMINFO();

            menuInfo.cbSize = Marshal.SizeOf(menuInfo);
            menuInfo.fMask = MIM.MIM_MENUDATA;
            itemInfo.cbSize = Marshal.SizeOf(itemInfo);
            itemInfo.fMask = MIIM.MIIM_ID | MIIM.MIIM_SUBMENU;

            // First, tag the managed menu items with an arbitary 
            // value (0xAB).
            TagManagedMenuItems(menu, tag);

            for (int n = 0; n < count; ++n)
            {
                GetMenuItemInfo(menu.Handle, n, true, ref itemInfo);

                if (itemInfo.hSubMenu == IntPtr.Zero)
                {
                    // If the item has no submenu we can't get the tag, so 
                    // check its ID to determine if it was added by the shell.
                    if (itemInfo.wID >= m_CmdFirst) remove.Add(n);
                }
                else
                {
                    GetMenuInfo(itemInfo.hSubMenu, ref menuInfo);
                    if (menuInfo.dwMenuData != tag) remove.Add(n);
                }
            }

            // Remove the unmanaged menu items.
            remove.Reverse();
            foreach (int position in remove)
            {
                DeleteMenu(menu.Handle, position, MF.MF_BYPOSITION);
            }
        }

        class MessageWindow : Control
        {
            public MessageWindow(ShellContextMenu parent)
            {
                m_Parent = parent;
            }

            protected override void WndProc(ref Message m)
            {
                if (!m_Parent.HandleMenuMessage(ref m))
                {
                    base.WndProc(ref m);
                }
            }

            ShellContextMenu m_Parent;
        }

        MessageWindow m_MessageWindow;
        WindowsHelper.WindowsAPI.IContextMenu m_ComInterface;
        WindowsHelper.WindowsAPI.IContextMenu2 m_ComInterface2;
        WindowsHelper.WindowsAPI.IContextMenu3 m_ComInterface3;
        const int m_CmdFirst = 0x8000;
    }

    public class BHID
    {
        public static Guid SFObject
        {
            get { return m_SFObject; }
        }

        public static Guid SFUIObject
        {
            get { return m_SFUIObject; }
        }

        static Guid m_SFObject = new Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");
        static Guid m_SFUIObject = new Guid("3981e225-f559-11d3-8e3a-00c04f6837d5");
    }
}
