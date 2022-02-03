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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

#pragma warning disable 1591

namespace BExplorer.Shell.Interop {
  [Flags]
  public enum CMF : uint {
    NORMAL = 0x00000000,
    DEFAULTONLY = 0x00000001,
    VERBSONLY = 0x00000002,
    EXPLORE = 0x00000004,
    NOVERBS = 0x00000008,
    CANRENAME = 0x00000010,
    NODEFAULT = 0x00000020,
    INCLUDESTATIC = 0x00000040,
    EXTENDEDVERBS = 0x00000100,
    SYNCCASCADEMENU = 0x00001000,
    RESERVED = 0xffff0000,
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct CMINVOKECOMMANDINFO {
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

  [StructLayout(LayoutKind.Sequential)]
  public struct CMINVOKECOMMANDINFOEX {
    public int cbSize;
    public CMIC fMask;
    public IntPtr hwnd;
    public IntPtr lpVerb;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpParameters;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpDirectory;
    public int nShow;
    public int dwHotKey;
    public IntPtr hIcon;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpTitle;
    public IntPtr lpVerbW;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpParametersW;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpDirectoryW;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpTitleW;
    public Point ptInvoke;
  }

  /*
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct CMINVOKECOMMANDINFO_ByIndex {
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
  */

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("000214e4-0000-0000-c000-000000000046")]
  public interface IContextMenu {
    /// <summary>Adds commands to a shortcut menu.</summary>
    /// <param name="hmenu">A handle to the shortcut menu. The handler should specify this handle when adding menu items.</param>
    /// <param name="indexMenu">The zero-based position at which to insert the first new menu item.</param>
    /// <param name="idCmdFirst">The minimum value that the handler can specify for a menu item identifier.</param>
    /// <param name="idCmdLast">The maximum value that the handler can specify for a menu item identifier.</param>
    /// <param name="uFlags">Optional flags that specify how the shortcut menu can be changed.</param>
    /// <returns>
    /// If successful, returns an HRESULT value that has its severity value set to SEVERITY_SUCCESS and its code value set to the
    /// offset of the largest command identifier that was assigned, plus one. For example, if idCmdFirst is set to 5 and you add
    /// three items to the menu with command identifiers of 5, 7, and 8, the return value should be MAKE_HRESULT(SEVERITY_SUCCESS, 0,
    /// 8 - 5 + 1). Otherwise, it returns a COM error value.
    /// </returns>
    [PreserveSig]
    HResult QueryContextMenu(IntPtr hmenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

    /// <summary>Carries out the command associated with a shortcut menu item.</summary>
    /// <param name="pici">
    /// A pointer to a CMINVOKECOMMANDINFO or CMINVOKECOMMANDINFOEX structure that contains specifics about the command.
    /// </param>
    [PreserveSig]
    HResult InvokeCommand(in Shell32.CMINVOKECOMMANDINFOEX pici);

    /// <summary>
    /// Gets information about a shortcut menu command, including the help string and the language-independent, or canonical, name
    /// for the command.
    /// </summary>
    /// <param name="idCmd">Menu command identifier offset.</param>
    /// <param name="uType">Flags specifying the information to return.</param>
    /// <param name="pReserved">
    /// Reserved. Applications must specify NULL when calling this method and handlers must ignore this parameter when called.
    /// </param>
    /// <param name="pszName">The reference of the buffer to receive the null-terminated string being retrieved.</param>
    /// <param name="cchMax">Size of the buffer, in characters, to receive the null-terminated string.</param>
    /// <returns>If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
    [PreserveSig]
    HResult GetCommandString([In] IntPtr idCmd, uint uType, [In, Optional] IntPtr pReserved, [Out] IntPtr pszName,
      uint cchMax);

  }

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("000214f4-0000-0000-c000-000000000046")]
  public interface IContextMenu2 : IContextMenu {
    /// <summary>Adds commands to a shortcut menu.</summary>
    /// <param name="hmenu">A handle to the shortcut menu. The handler should specify this handle when adding menu items.</param>
    /// <param name="indexMenu">The zero-based position at which to insert the first new menu item.</param>
    /// <param name="idCmdFirst">The minimum value that the handler can specify for a menu item identifier.</param>
    /// <param name="idCmdLast">The maximum value that the handler can specify for a menu item identifier.</param>
    /// <param name="uFlags">Optional flags that specify how the shortcut menu can be changed.</param>
    /// <returns>
    /// If successful, returns an HRESULT value that has its severity value set to SEVERITY_SUCCESS and its code value set to the
    /// offset of the largest command identifier that was assigned, plus one. For example, if idCmdFirst is set to 5 and you add
    /// three items to the menu with command identifiers of 5, 7, and 8, the return value should be MAKE_HRESULT(SEVERITY_SUCCESS, 0,
    /// 8 - 5 + 1). Otherwise, it returns a COM error value.
    /// </returns>
    [PreserveSig]
    new HResult QueryContextMenu(IntPtr hmenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

    /// <summary>Carries out the command associated with a shortcut menu item.</summary>
    /// <param name="pici">
    /// A pointer to a CMINVOKECOMMANDINFO or CMINVOKECOMMANDINFOEX structure that contains specifics about the command.
    /// </param>
    [PreserveSig]
    new HResult InvokeCommand(in Shell32.CMINVOKECOMMANDINFOEX pici);

    /// <summary>
    /// Gets information about a shortcut menu command, including the help string and the language-independent, or canonical, name
    /// for the command.
    /// </summary>
    /// <param name="idCmd">Menu command identifier offset.</param>
    /// <param name="uType">Flags specifying the information to return.</param>
    /// <param name="pReserved">
    /// Reserved. Applications must specify NULL when calling this method and handlers must ignore this parameter when called.
    /// </param>
    /// <param name="pszName">The reference of the buffer to receive the null-terminated string being retrieved.</param>
    /// <param name="cchMax">Size of the buffer, in characters, to receive the null-terminated string.</param>
    /// <returns>If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
    [PreserveSig]
    new HResult GetCommandString([In] IntPtr idCmd, uint uType, [In, Optional] IntPtr pReserved, [Out] IntPtr pszName, uint cchMax);

    /// <summary>Enables client objects of the IContextMenu interface to handle messages associated with owner-drawn menu items.</summary>
    /// <param name="uMsg">
    /// The message to be processed. In the case of some messages, such as WM_INITMENUPOPUP, WM_DRAWITEM, WM_MENUCHAR, or
    /// WM_MEASUREITEM, the client object being called may provide owner-drawn menu items.
    /// </param>
    /// <param name="wParam">Additional message information. The value of this parameter depends on the value of the uMsg parameter.</param>
    /// <param name="lParam">Additional message information. The value of this parameter depends on the value of the uMsg parameter.</param>
    [PreserveSig]
    HResult HandleMenuMsg(uint uMsg, [In] IntPtr wParam, [In] IntPtr lParam);
  }

  [ComImport]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("bcfce0a0-ec17-11d0-8d10-00a0c90f2719")]
  public interface IContextMenu3 : IContextMenu2 {
    /// <summary>Adds commands to a shortcut menu.</summary>
    /// <param name="hmenu">A handle to the shortcut menu. The handler should specify this handle when adding menu items.</param>
    /// <param name="indexMenu">The zero-based position at which to insert the first new menu item.</param>
    /// <param name="idCmdFirst">The minimum value that the handler can specify for a menu item identifier.</param>
    /// <param name="idCmdLast">The maximum value that the handler can specify for a menu item identifier.</param>
    /// <param name="uFlags">Optional flags that specify how the shortcut menu can be changed.</param>
    /// <returns>
    /// If successful, returns an HRESULT value that has its severity value set to SEVERITY_SUCCESS and its code value set to the
    /// offset of the largest command identifier that was assigned, plus one. For example, if idCmdFirst is set to 5 and you add
    /// three items to the menu with command identifiers of 5, 7, and 8, the return value should be MAKE_HRESULT(SEVERITY_SUCCESS, 0,
    /// 8 - 5 + 1). Otherwise, it returns a COM error value.
    /// </returns>
    [PreserveSig]
    new HResult QueryContextMenu(IntPtr hmenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, CMF uFlags);

    /// <summary>Carries out the command associated with a shortcut menu item.</summary>
    /// <param name="pici">
    /// A pointer to a CMINVOKECOMMANDINFO or CMINVOKECOMMANDINFOEX structure that contains specifics about the command.
    /// </param>
    [PreserveSig]
    new HResult InvokeCommand(in CMINVOKECOMMANDINFOEX pici);

    /// <summary>
    /// Gets information about a shortcut menu command, including the help string and the language-independent, or canonical, name
    /// for the command.
    /// </summary>
    /// <param name="idCmd">Menu command identifier offset.</param>
    /// <param name="uType">Flags specifying the information to return.</param>
    /// <param name="pReserved">
    /// Reserved. Applications must specify NULL when calling this method and handlers must ignore this parameter when called.
    /// </param>
    /// <param name="pszName">The reference of the buffer to receive the null-terminated string being retrieved.</param>
    /// <param name="cchMax">Size of the buffer, in characters, to receive the null-terminated string.</param>
    /// <returns>If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
    [PreserveSig]
    new HResult GetCommandString([In] IntPtr idCmd, uint uType, [In, Optional] IntPtr pReserved, [Out] IntPtr pszName, uint cchMax);

    /// <summary>Enables client objects of the IContextMenu interface to handle messages associated with owner-drawn menu items.</summary>
    /// <param name="uMsg">
    /// The message to be processed. In the case of some messages, such as WM_INITMENUPOPUP, WM_DRAWITEM, WM_MENUCHAR, or
    /// WM_MEASUREITEM, the client object being called may provide owner-drawn menu items.
    /// </param>
    /// <param name="wParam">Additional message information. The value of this parameter depends on the value of the uMsg parameter.</param>
    /// <param name="lParam">Additional message information. The value of this parameter depends on the value of the uMsg parameter.</param>
    [PreserveSig]
    new HResult HandleMenuMsg(uint uMsg, [In] IntPtr wParam, [In] IntPtr lParam);

    /// <summary>Allows client objects of the IContextMenu3 interface to handle messages associated with owner-drawn menu items.</summary>
    /// <param name="uMsg">
    /// The message to be processed. In the case of some messages, such as WM_INITMENUPOPUP, WM_DRAWITEM, WM_MENUCHAR, or
    /// WM_MEASUREITEM, the client object being called may provide owner-drawn menu items.
    /// </param>
    /// <param name="wParam">Additional message information. The value of this parameter depends on the value of the uMsg parameter.</param>
    /// <param name="lParam">Additional message information. The value of this parameter depends on the value of the uMsg parameter.</param>
    /// <param name="result">
    /// The address of an LRESULT value that the owner of the menu will return from the message. This parameter can be NULL.
    /// </param>
    [PreserveSig]
    HResult HandleMenuMsg2(uint uMsg, IntPtr wParam, IntPtr lParam, out IntPtr result);
  }

  [ComImport()]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [GuidAttribute("000214e8-0000-0000-c000-000000000046")]
  public interface IShellExtInit {
    [PreserveSig()]
    int Initialize(
        IntPtr pidlFolder,
        IDataObject lpdobj,
        uint hKeyProgID);
  }
}
