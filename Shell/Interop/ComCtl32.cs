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
    public enum ILD
    {
        NORMAL = 0x00000000,
        TRANSPARENT = 0x00000001,
        MASK = 0x00000010,
        IMAGE = 0x00000020,
        ROP = 0x00000040,
        BLEND25 = 0x00000002,
        BLEND50 = 0x00000004,
        OVERLAYMASK = 0x00000F00,
        PRESERVEALPHA = 0x00001000,
    }

    public class ComCtl32
    {
        [DllImport("comctl32")]
        public static extern bool ImageList_Draw(IntPtr himl,
            int i, IntPtr hdcDst, int x, int y, uint fStyle);

        [DllImport("comctl32")]
        public static extern bool ImageList_GetIconSize(IntPtr himl, out int cx, out int cy);
    }
}
