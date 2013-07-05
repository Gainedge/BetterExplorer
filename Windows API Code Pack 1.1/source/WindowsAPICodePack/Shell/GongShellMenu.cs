using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Controls;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Shell
{
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
    public uint cbSize;
    public uint fMask;
    public uint fType;
    public uint fState;
    public uint wID;
    public IntPtr hSubMenu;
    public IntPtr hbmpChecked;
    public IntPtr hbmpUnchecked;
    public IntPtr dwItemData;
    public string dwTypeData;
    public uint cch;
    public IntPtr hbmpItem;

    // return the size of the structure
    public static uint sizeOf
    {
      get { return (uint)Marshal.SizeOf(typeof(MENUITEMINFO)); }
    }
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
  public class GongShellContextMenu
  {
    [DllImport("user32.dll")]
    public static extern bool DeleteMenu(IntPtr hMenu, int uPosition,
        MF uFlags);

    [DllImport("user32.dll")]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool GetMenuInfo(IntPtr hmenu,
        ref MENUINFO lpcmi);

    [DllImport("user32.dll")]
    public static extern int GetMenuItemCount(IntPtr hMenu);

    [DllImport("user32.dll")]
    public static extern bool GetMenuItemInfo(IntPtr hMenu, int uItem,
        bool fByPosition, ref MENUITEMINFO lpmii);

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

    private Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser Explorer;
    /// <summary>
    /// Initialises a new instance of the <see cref="ShellContextMenu"/> 
    /// class.
    /// </summary>
    /// 
    /// <param name="item">
    /// The item to which the context menu should refer.
    /// </param>
    public GongShellContextMenu(ShellObject item)
    {
      Initialize(new ShellObject[] { item });
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ShellContextMenu"/> 
    /// class.
    /// </summary>
    /// 
    /// <param name="items">
    /// The items to which the context menu should refer.
    /// </param>
    public GongShellContextMenu(ShellObject[] items)
    {
      Initialize(items);
    }

    public GongShellContextMenu(Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser explorer,  ShellViewGetItemObject items)
    {
      this.Explorer = explorer;
      Initialize(this.Explorer.GetShellView(), items);
    }

    public GongShellContextMenu(Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser explorer, Boolean isNew)
    {
      this.Explorer = explorer;
      Initialize(this.Explorer.NavigationLog.CurrentLocation);
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
      if ((m.Msg == (int)WindowsAPI.WndMsg.WM_COMMAND) && ((int)m.WParam >= m_CmdFirst))
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

    /// <summary>
    /// Invokes the Delete command on the shell item.
    /// </summary>
    public void InvokeDelete()
    {
      GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO invoke = new GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO();
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
      GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO invoke = new GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO();
      invoke.cbSize = Marshal.SizeOf(invoke);
      invoke.lpVerb = "rename";
      m_ComInterface.InvokeCommand(ref invoke);
    }

    public void InvokeSpecialCommand(string verb)
    {
      GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO invoke = new GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO();
      invoke.cbSize = Marshal.SizeOf(invoke);
      invoke.lpVerb = verb;
      m_ComInterface.InvokeCommand(ref invoke);
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
          m_CmdFirst, int.MaxValue, GongSolutions.Shell.ShellContextMenu.CMF.EXPLORE | GongSolutions.Shell.ShellContextMenu.CMF.CANRENAME);
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
    public void ShowContextMenu(Point pos, Boolean isNew = false)
    {
      using (ContextMenu menu = new ContextMenu())
      {
        
        Populate(menu);
        int count = GetMenuItemCount(menu.Handle);
        MENUITEMINFO itemInfo = new MENUITEMINFO();

        itemInfo.cbSize = (uint)Marshal.SizeOf(itemInfo);
        itemInfo.fMask = (uint)MIIM.MIIM_FTYPE;
        for (int i = 0; i < count; i++)
        {
          if (GetMenuItemInfo(menu.Handle, i, true, ref itemInfo))
          {
            var isSep = (itemInfo.fType & 2048) != 0;
            if (i == count - 1)
            {
              GetMenuItemInfo(menu.Handle, i, true, ref itemInfo);
              if ((itemInfo.fType & 2048) != 0)
              {
                DeleteMenu(menu.Handle, i, MF.MF_BYPOSITION);
              }
            } else if (i < count - 1)
              GetMenuItemInfo(menu.Handle, i + 1, true, ref itemInfo);
            if (isSep && (itemInfo.fType & 2048) != 0)
            {
              DeleteMenu(menu.Handle, i, MF.MF_BYPOSITION);
            }
          }
        }

        int command = TrackPopupMenuEx(menu.Handle,
            TPM.TPM_RETURNCMD, pos.X, pos.Y, m_MessageWindow.Handle,
            IntPtr.Zero);
        if (command > 0)
        {
          string info = string.Empty;
          byte[] bytes = new byte[256];
          int index;

          m_ComInterface.GetCommandString(command - m_CmdFirst, 4, 0, bytes, 260);

          index = 0;
          while (index < bytes.Length - 1 && (bytes[index] != 0 || bytes[index + 1] != 0))
          { index += 2; }

          if (index < bytes.Length - 1)
            info = Encoding.Unicode.GetString(bytes, 0, index); //+ 1);

          if (!isNew)
          {
            if (info == "rename")
              Explorer.DoRename();
            else
              InvokeCommand(command - m_CmdFirst);
          }
          else
          {
            if (String.IsNullOrEmpty(info))
            {
              var newItems = GetNewContextMenuItems();
              InvokeCommand(m_ComInterface3, newItems[command - m_CmdFirst - 3], this.Explorer.NavigationLog.CurrentLocation.ParsingName, pos);

            }
            else
            {
              InvokeCommand(m_ComInterface3, info, this.Explorer.NavigationLog.CurrentLocation.ParsingName, pos);
            }
            //InvokeSpecialCommand(info);
          }
        }
      }
    }

    private List<string> GetNewContextMenuItems()
    {
      List<string> TheList = new List<string>();
      RegistryKey reg = Registry.CurrentUser;
      RegistryKey classesrk = reg.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Discardable\PostSetup\ShellNew");
      string[] classes = (string[])classesrk.GetValue("Classes");

      //foreach (string item in classes)
      //{
      //  RegistryKey regext = Registry.ClassesRoot;
      //  RegistryKey extrk = regext.OpenSubKey(item + @"\ShellNew");
      //  if (extrk != null)
      //  {
      //    string FileName = (string)extrk.GetValue("FileName");
      //    TheList.Add(FileName);
      //  }


      //}
      TheList.AddRange(classes);
      classesrk.Close();
      reg.Close();
      return TheList;
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CMINVOKECOMMANDINFOEX
    {
      public int Size;

      public CMIC Mask;

      public IntPtr Hwnd;

      public IntPtr Verb;

      [MarshalAs(UnmanagedType.LPStr)]
      public string Parameters;

      [MarshalAs(UnmanagedType.LPStr)]
      public string Directory;

      public SW ShowType;

      public int HotKey;

      public IntPtr hIcon;

      [MarshalAs(UnmanagedType.LPStr)]
      public string Title;

      public IntPtr VerbW;

      [MarshalAs(UnmanagedType.LPWStr)]
      public string ParametersW;

      [MarshalAs(UnmanagedType.LPWStr)]
      public string DirectoryW;

      [MarshalAs(UnmanagedType.LPWStr)]
      public string TitleW;

      public POINT InvokePoint;
    }



    public void InvokeCommand(GongSolutions.Shell.ShellContextMenu.IContextMenu3 iContextMenu, string cmd, string parentDir, Point ptInvoke)
    {
      CMINVOKECOMMANDINFOEX invoke = new CMINVOKECOMMANDINFOEX();
      invoke.Size = Marshal.SizeOf(typeof(CMINVOKECOMMANDINFOEX));
      invoke.Verb = Marshal.StringToHGlobalAnsi(cmd);
      invoke.Directory = parentDir;
      invoke.VerbW = Marshal.StringToHGlobalUni(cmd);
      invoke.DirectoryW = parentDir;
      invoke.Mask = CMIC.Unicode | CMIC.PtInvoke |
          ((Control.ModifierKeys & Keys.Control) != 0 ? CMIC.ControlDown : 0) |
          ((Control.ModifierKeys & Keys.Shift) != 0 ? CMIC.ShiftDown : 0);
      POINT pt = new POINT();
      pt.x = ptInvoke.X;
      pt.y = ptInvoke.Y;
      invoke.InvokePoint = pt;
      invoke.ShowType = SW.ShowNormal;

      var res = iContextMenu.InvokeCommand(ref invoke);
    }


    /// <summary>
    /// Gets the underlying COM <see cref="IContextMenu"/> interface.
    /// </summary>
    public GongSolutions.Shell.ShellContextMenu.IContextMenu ComInterface
    {
      get { return m_ComInterface; }
      set { m_ComInterface = value; }
    }

    void Initialize(IShellView view, ShellViewGetItemObject items)
    {
      Object result = null;
      Guid iicm = typeof(GongSolutions.Shell.ShellContextMenu.IContextMenu).GUID;
      view.GetItemObject(items, ref iicm, out result);
      m_ComInterface = (GongSolutions.Shell.ShellContextMenu.IContextMenu)result;
      m_ComInterface2 = m_ComInterface as GongSolutions.Shell.ShellContextMenu.IContextMenu2;
      m_ComInterface3 = m_ComInterface as GongSolutions.Shell.ShellContextMenu.IContextMenu3;
      m_MessageWindow = new MessageWindow(this);
    }

    void Initialize(ShellObject[] items)
    {
      IntPtr[] pidls = new IntPtr[items.Length];
      ShellObject parent = null;
      Object result;

      for (int n = 0; n < items.Length; ++n)
      {
        pidls[n] = WindowsAPI.ILFindLastID(items[n].PIDL);

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
      Guid iicm = typeof(GongSolutions.Shell.ShellContextMenu.IContextMenu).GUID;
      WindowsAPI.GetIShellFolder(parent).GetUIObjectOf(IntPtr.Zero,
          (uint)pidls.Length, pidls,
          ref iicm, 0, out result);
      m_ComInterface = (GongSolutions.Shell.ShellContextMenu.IContextMenu)result;
      m_ComInterface2 = m_ComInterface as GongSolutions.Shell.ShellContextMenu.IContextMenu2;
      m_ComInterface3 = m_ComInterface as GongSolutions.Shell.ShellContextMenu.IContextMenu3;
      m_MessageWindow = new MessageWindow(this);
    }

    void Initialize(ShellObject dirItem)
    {
      object iContextMenu = null;
      GetNewContextMenu(dirItem, out iContextMenu, out m_ComInterface);

      m_ComInterface2 = m_ComInterface as GongSolutions.Shell.ShellContextMenu.IContextMenu2;
      m_ComInterface3 = m_ComInterface as GongSolutions.Shell.ShellContextMenu.IContextMenu3;
      m_MessageWindow = new MessageWindow(this);
    }

    void InvokeCommand(int index)
    {
      const int SW_SHOWNORMAL = 1;
      GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO_ByIndex invoke = new GongSolutions.Shell.ShellContextMenu.CMINVOKECOMMANDINFO_ByIndex();
      invoke.cbSize = Marshal.SizeOf(invoke);
      invoke.iVerb = index;
      invoke.nShow = SW_SHOWNORMAL;
      m_ComInterface2.InvokeCommand(ref invoke);
    }

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

    public bool GetNewContextMenu(ShellObject item, out object iContextMenuPtr, out GongSolutions.Shell.ShellContextMenu.IContextMenu iContextMenu)
    {
      Guid CLSID_NewMenu = new Guid("{D969A300-E7FF-11d0-A93B-00A0C90F2719}");
      Guid iicm = typeof(GongSolutions.Shell.ShellContextMenu.IContextMenu).GUID;
      Guid iise = typeof(IShellExtInit).GUID;
      if (WindowsAPI.CoCreateInstance(
              ref CLSID_NewMenu,
              IntPtr.Zero,
              0x1,
              ref iicm,
              out iContextMenuPtr) == (int)HResult.Ok)
      {
        iContextMenu = iContextMenuPtr as GongSolutions.Shell.ShellContextMenu.IContextMenu;

        IntPtr iShellExtInitPtr;
        if (Marshal.QueryInterface(
            Marshal.GetIUnknownForObject(iContextMenuPtr),
            ref iise,
            out iShellExtInitPtr) == (int)HResult.Ok)
        {
          IShellExtInit iShellExtInit = Marshal.GetTypedObjectForIUnknown(
              iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;

          try
          {
            iShellExtInit.Initialize(item.PIDL, IntPtr.Zero, 0);

            Marshal.ReleaseComObject(iShellExtInit);
            Marshal.Release(iShellExtInitPtr);

            return true;
          }
          finally
          {
            
          }
        }
        else
        {
          if (iContextMenu != null)
          {
            Marshal.ReleaseComObject(iContextMenu);
            iContextMenu = null;
          }

          if (iContextMenuPtr != null)
          {
            Marshal.ReleaseComObject(iContextMenuPtr);
            iContextMenuPtr = null;
          }

          return false;
        }
      }
      else
      {
        iContextMenuPtr = IntPtr.Zero;
        iContextMenu = null;
        return false;
      }
    }


    void RemoveShellMenuItems(Menu menu)
    {
      const int tag = 0xAB;
      List<int> remove = new List<int>();
      int count = GetMenuItemCount(menu.Handle);
      MENUINFO menuInfo = new MENUINFO();
      MENUITEMINFO itemInfo = new MENUITEMINFO();

      menuInfo.cbSize = Marshal.SizeOf(menuInfo);
      menuInfo.fMask = MIM.MIM_MENUDATA;
      itemInfo.cbSize = (uint)Marshal.SizeOf(itemInfo);
      itemInfo.fMask = (uint)MIIM.MIIM_ID | (uint)MIIM.MIIM_SUBMENU;

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
      public MessageWindow(GongShellContextMenu parent)
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

      GongShellContextMenu m_Parent;
    }

    MessageWindow m_MessageWindow;
    GongSolutions.Shell.ShellContextMenu.IContextMenu m_ComInterface;
    GongSolutions.Shell.ShellContextMenu.IContextMenu2 m_ComInterface2;
    GongSolutions.Shell.ShellContextMenu.IContextMenu3 m_ComInterface3;
    const int m_CmdFirst = 0x8000;
  }

  [ComImport()]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [GuidAttribute("000214e8-0000-0000-c000-000000000046")]
  public interface IShellExtInit
  {
    [PreserveSig()]
    int Initialize(
        IntPtr pidlFolder,
        IntPtr lpdobj,
        uint hKeyProgID);
  }


}
