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
using System.Drawing;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public enum SVGIO : uint
    {
        SVGIO_BACKGROUND = 0,
        SVGIO_SELECTION = 0x1,
        SVGIO_ALLVIEW = 0x2,
        SVGIO_CHECKED = 0x3,
        SVGIO_TYPE_MASK = 0xf,
        SVGIO_FLAG_VIEWORDER = 0x80000000,
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214E3-0000-0000-C000-000000000046")]
    public interface IShellView
    {
        void GetWindow(out IntPtr windowHandle);
        void ContextSensitiveHelp(bool fEnterMode);
        [PreserveSig]
        long TranslateAcceleratorA(IntPtr message);
        void EnableModeless(bool enable);
        void UIActivate(UInt32 activtionState);
        void Refresh();
        void CreateViewWindow(
            IShellView previousShellView,
            ref FOLDERSETTINGS folderSetting,
            IShellBrowser shellBrowser,
            ref Rectangle bounds,
            out IntPtr handleOfCreatedWindow);
        void DestroyViewWindow();
        void GetCurrentInfo(ref FOLDERSETTINGS pfs);
        void AddPropertySheetPages([In, MarshalAs(UnmanagedType.U4)] uint reserved, [In]ref IntPtr functionPointer, [In] IntPtr lparam);
        void SaveViewState();
        void SelectItem(IntPtr pidlItem, [MarshalAs(UnmanagedType.U4)] SVSI flags);

        [PreserveSig]
        int GetItemObject(
            [In] SVGIO AspectOfView,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);
    }
}
