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
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
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

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214e4-0000-0000-c000-000000000046")]
    public interface IContextMenu
    {
        [PreserveSig]
        HResult QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst,
                                 int idCmdLast, CMF uFlags);

        void InvokeCommand(ref CMINVOKECOMMANDINFO pici);

        [PreserveSig]
        HResult GetCommandString(int idcmd, uint uflags, int reserved,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
            int cch);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214f4-0000-0000-c000-000000000046")]
    public interface IContextMenu2 : IContextMenu
    {
        [PreserveSig]
        new HResult QueryContextMenu(IntPtr hMenu, uint indexMenu,
            int idCmdFirst, int idCmdLast,
            CMF uFlags);

        void InvokeCommand(ref CMINVOKECOMMANDINFO_ByIndex pici);

        [PreserveSig]
        new HResult GetCommandString(int idcmd, uint uflags, int reserved,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
            int cch);

        [PreserveSig]
        HResult HandleMenuMsg(int uMsg, IntPtr wParam, IntPtr lParam);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("bcfce0a0-ec17-11d0-8d10-00a0c90f2719")]
    public interface IContextMenu3 : IContextMenu2
    {
        [PreserveSig]
        new HResult QueryContextMenu(IntPtr hMenu, uint indexMenu, int idCmdFirst,
                             int idCmdLast, CMF uFlags);

        [PreserveSig]
        new HResult InvokeCommand(ref CMINVOKECOMMANDINFO pici);

        [PreserveSig]
        new HResult GetCommandString(int idcmd, uint uflags, int reserved,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder commandstring,
            int cch);

        [PreserveSig]
        new HResult HandleMenuMsg(int uMsg, IntPtr wParam, IntPtr lParam);

        [PreserveSig]
        HResult HandleMenuMsg2(int uMsg, IntPtr wParam, IntPtr lParam,
            out IntPtr plResult);
    }
}
