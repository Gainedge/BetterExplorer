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
    [Guid("8BE2D872-86AA-4d47-B776-32CCA40C7018")]
    public interface IKnownFolderManager
    {
        Guid FolderIdFromCsidl(int nCsidl);

        CSIDL FolderIdToCsidl(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid);

        void GetFolderIds(
            [Out] out IntPtr ppKFId,
            [Out] out uint pCount);

        IKnownFolder GetFolder(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid);

        [PreserveSig]
        HResult GetFolderByName(
            [In] string pszCanonicalName,
            [Out] out IKnownFolder ppkf);

        void RegisterFolder(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            [In] IntPtr pKFD);

        void UnregisterFolder(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid);

        [PreserveSig]
        HResult FindFolderFromPath(
            [In] string pszPath,
            [In] FFFP_MODE mode,
            [Out] out IKnownFolder ppkf);

        [PreserveSig]
        HResult FindFolderFromIDList(
            [In] IntPtr pidl,
            [Out] out IKnownFolder ppkf);

        string Redirect(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            [In] IntPtr hwnd,
            [In] int flags,
            [In] string pszTargetPath,
            [In] uint cFolders,
            [In] Guid[] pExclusion);
    }
}
