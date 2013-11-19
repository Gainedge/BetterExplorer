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

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public enum FCW
    {
        FCW_STATUS = 0x0001,
        FCW_TOOLBAR = 0x0002,
        FCW_TREE = 0x0003,
        FCW_INTERNETBAR = 0x0006,
        FCW_PROGRESS = 0x0008,
    }

    [Flags]
    public enum SBSP : uint
    {
        SBSP_DEFBROWSER = 0x0000,
        SBSP_SAMEBROWSER = 0x0001,
        SBSP_NEWBROWSER = 0x0002,
        SBSP_DEFMODE = 0x0000,
        SBSP_OPENMODE = 0x0010,
        SBSP_EXPLOREMODE = 0x0020,
        SBSP_HELPMODE = 0x0040,
        SBSP_NOTRANSFERHIST = 0x0080,
        SBSP_ABSOLUTE = 0x0000,
        SBSP_RELATIVE = 0x1000,
        SBSP_PARENT = 0x2000,
        SBSP_NAVIGATEBACK = 0x4000,
        SBSP_NAVIGATEFORWARD = 0x8000,
        SBSP_ALLOW_AUTONAVIGATE = 0x10000,
        SBSP_CALLERUNTRUSTED = 0x00800000,
        SBSP_TRUSTFIRSTDOWNLOAD = 0x01000000,
        SBSP_UNTRUSTEDFORDOWNLOAD = 0x02000000,
        SBSP_NOAUTOSELECT = 0x04000000,
        SBSP_WRITENOHISTORY = 0x08000000,
        SBSP_TRUSTEDFORACTIVEX = 0x10000000,
        SBSP_REDIRECT = 0x40000000,
        SBSP_INITIATEDBYHLINKFRAME = 0x80000000,
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E2-0000-0000-C000-000000000046")]
    public interface IShellBrowser
    {
        [PreserveSig]
        HResult GetWindow(out IntPtr phwnd);
        [PreserveSig]
        HResult ContextSensitiveHelp(bool fEnterMode);
        [PreserveSig]
        HResult InsertMenusSB(IntPtr IntPtrShared, IntPtr lpMenuWidths);
        [PreserveSig]
        HResult SetMenuSB(IntPtr IntPtrShared, IntPtr holemenuRes,
                      IntPtr IntPtrActiveObject);
        [PreserveSig]
        HResult RemoveMenusSB(IntPtr IntPtrShared);
        [PreserveSig]
        HResult SetStatusTextSB(IntPtr pszStatusText);
        [PreserveSig]
        HResult EnableModelessSB(bool fEnable);
        [PreserveSig]
        HResult TranslateAcceleratorSB(IntPtr pmsg, ushort wID);

        [PreserveSig]
        HResult BrowseObject(IntPtr pidl, SBSP wFlags);
        [PreserveSig]
        HResult GetViewStateStream(uint grfMode, IntPtr ppStrm);
        [PreserveSig]
        HResult GetControlWindow(FCW id, out IntPtr lpIntPtr);
        [PreserveSig]
        HResult SendControlMsg(FCW id, MSG uMsg, uint wParam, uint lParam, IntPtr pret);
        [PreserveSig]
        HResult QueryActiveShellView(out IShellView ppshv);
        [PreserveSig]
        HResult OnViewWindowActive(IShellView ppshv);
        [PreserveSig]
        HResult SetToolbarItems(IntPtr lpButtons, uint nButtons, uint uFlags);
    }
}
