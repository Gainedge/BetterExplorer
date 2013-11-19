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
    public struct OLECMDTEXT
    {
        public uint cmdtextf;
        public uint cwActual;
        public uint cwBuf;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public char rgwz;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OLECMD
    {
        public uint cmdID;
        public uint cmdf;
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("b722bccb-4e68-101b-a2bc-00aa00404770")]
    public interface IOleCommandTarget
    {
        void QueryStatus(ref Guid pguidCmdGroup, UInt32 cCmds,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] 
            OLECMD[] prgCmds,
            ref OLECMDTEXT CmdText);
        void Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdExecOpt,
                  ref object pvaIn, ref object pvaOut);
    }
}
