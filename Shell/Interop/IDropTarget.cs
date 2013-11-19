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
using System.Runtime.InteropServices.ComTypes;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000122-0000-0000-C000-000000000046")]
    public interface IDropTarget
    {
        void DragEnter(IDataObject pDataObj, int grfKeyState,
                       Point pt, ref int pdwEffect);
        void DragOver(int grfKeyState, Point pt, ref int pdwEffect);
        void DragLeave();
        void Drop(IDataObject pDataObj, int grfKeyState,
                 Point pt, ref int pdwEffect);
    }
}
