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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct COMDLG_FILTERSPEC
    {
        public string pszName;
        public string pszSpec;
    }

    public enum FDE_OVERWRITE_RESPONSE
    {
        FDEOR_DEFAULT = 0,
        FDEOR_ACCEPT = 0x1,
        FDEOR_REFUSE = 0x2
    }

    public enum FDE_SHAREVIOLATION_RESPONSE
    {
        FDESVR_DEFAULT = 0,
        FDESVR_ACCEPT = 0x1,
        FDESVR_REFUSE = 0x2
    }

    public enum FDAP
    {
        FDAP_BOTTOM = 0,
        FDAP_TOP = 0x1
    }

    enum FILEOPENDIALOGOPTIONS
    {
        FOS_OVERWRITEPROMPT = 0x2,
        FOS_STRICTFILETYPES = 0x4,
        FOS_NOCHANGEDIR = 0x8,
        FOS_PICKFOLDERS = 0x20,
        FOS_FORCEFILESYSTEM = 0x40,
        FOS_ALLNONSTORAGEITEMS = 0x80,
        FOS_NOVALIDATE = 0x100,
        FOS_ALLOWMULTISELECT = 0x200,
        FOS_PATHMUSTEXIST = 0x800,
        FOS_FILEMUSTEXIST = 0x1000,
        FOS_CREATEPROMPT = 0x2000,
        FOS_SHAREAWARE = 0x4000,
        FOS_NOREADONLYRETURN = 0x8000,
        FOS_NOTESTFILECREATE = 0x10000,
        FOS_HIDEMRUPLACES = 0x20000,
        FOS_HIDEPINNEDPLACES = 0x40000,
        FOS_NODEREFERENCELINKS = 0x100000,
        FOS_DONTADDTORECENT = 0x2000000,
        FOS_FORCESHOWHIDDEN = 0x10000000,
        FOS_DEFAULTNOMINIMODE = 0x20000000,
        FOS_FORCEPREVIEWPANEON = 0x40000000
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
    public interface IFileDialog
    {
        [PreserveSig]
        HResult Show(IntPtr hwndParent);

        void SetFileTypes(uint cFileTypes,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
            COMDLG_FILTERSPEC[] rgFilterSpec);

        void SetFileTypeIndex(uint iFileType);

        uint GetFileTypeIndex();

        HResult Advise(IFileDialogEvents pfde);

        void Unadvise(ushort dwCookie);

        void SetOptions(ushort fos);

        ushort GetOptions();

        void SetDefaultFolder(IShellItem psi);

        void SetFolder(IShellItem psi);

        IShellItem GetFolder();

        IShellItem GetCurrentSelection();

        void SetFileName(string pszName);

        string GetFileName();

        void SetTitle(string pszTitle);

        void SetOkButtonLabel(string pszText);

        void SetFileNameLabel(string pszLabel);

        IShellItem GetResult();

        void AddPlace(IShellItem psi, FDAP fdap);

        void SetDefaultExtension(string pszDefaultExtension);

        void Close(int hr);

        void SetClientGuid(Guid guid);

        void ClearClientData();

        void SetFilter(IShellItemFilter pFilter);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("973510db-7d7f-452b-8975-74a85828d354")]
    public interface IFileDialogEvents
    {
        [PreserveSig]
        HResult OnFileOk(IFileDialog pfd);

        [PreserveSig]
        HResult OnFolderChanging(IFileDialog pfd, IShellItem psiFolder);

        [PreserveSig]
        HResult OnFolderChange(IFileDialog pfd);

        [PreserveSig]
        HResult OnSelectionChange(IFileDialog pfd);

        [PreserveSig]
        HResult OnShareViolation(IFileDialog pfd, IShellItem psi,
            out FDE_SHAREVIOLATION_RESPONSE pResponse);

        [PreserveSig]
        HResult OnTypeChange(IFileDialog pfd);

        [PreserveSig]
        HResult OnOverwrite(IFileDialog pfd, IShellItem psi, out FDE_OVERWRITE_RESPONSE pResponse);
    };

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("2659B475-EEB8-48b7-8F07-B378810F48CF")]
    public interface IShellItemFilter
    {
        int IncludeItem(IShellItem psi);
        int GetEnumFlagsForItem(IShellItem psi, out SHCONTF pgrfFlags);
    };
}
