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
using System.Text;

#pragma warning disable 1591

namespace GongSolutions.Shell.Interop
{
    public enum HResult
    {
        DRAGDROP_S_CANCEL = 0x00040101,
        DRAGDROP_S_DROP = 0x00040100,
        DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102,
        DATA_S_SAMEFORMATETC = 0x00040130,
        S_OK = 0,
        S_FALSE = 1,
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_NOTIMPL = unchecked((int)0x80004001),
        OLE_E_ADVISENOTSUPPORTED = unchecked((int)80040003),
        MK_E_NOOBJECT = unchecked((int)0x800401E5),
    }
}
