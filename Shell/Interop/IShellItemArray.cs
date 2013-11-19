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
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b63ea76d-1f85-456f-a19c-48159efa858b")]
    public interface IShellItemArray
    {
        void BindToHandler(
            [In] IntPtr pbc,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid bhid,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);

        void GetPropertyStore(
            [In] int flags,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);

        void GetPropertyDescriptionList(
            [In] int keyType,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out] out IntPtr ppv);

        void GetAttributes(
            [In] int dwAttribFlags,
            [In] int sfgaoMask,
            [Out] out int psfgaoAttribs);

        void GetCount(
            [Out] out ushort pdwNumItems);

        void GetItemAt(
            [In] ushort dwIndex,
            [Out, MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

        void EnumItems(
            [Out] out IntPtr ppenumShellItems);
    }
}
