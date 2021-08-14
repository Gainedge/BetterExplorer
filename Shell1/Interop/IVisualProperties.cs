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
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace BExplorer.Shell.Interop {
    public enum VPWATERMARKFLAGS {
        VPWF_DEFAULT = 0,
        VPWF_ALPHABLEND = 0x1
    }

    public enum VPCOLORFLAGS {
        VPCF_TEXT = 1,
        VPCF_BACKGROUND = 2,
        VPCF_SORTCOLUMN = 3,
        VPCF_SUBTEXT = 4,
        VPCF_TEXTBACKGROUND = 5
    }


    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("e693cf68-d967-4112-8763-99172aee5e5a")]
    public interface IVisualProperties {
        [PreserveSig]
        HResult SetWatermark(IntPtr hBitmap, VPWATERMARKFLAGS vpwf);

        [PreserveSig]
        HResult SetColor(VPCOLORFLAGS vpcf, IntPtr hColor);

        [PreserveSig]
        HResult GetColor(VPCOLORFLAGS vpcf, [Out] out IntPtr hColor);

        [PreserveSig]
        HResult SetItemHeight(Int32 cyItemInPixels);

        [PreserveSig]
        HResult GetItemHeight([Out] out Int32 cyItemInPixels);
    };
}
