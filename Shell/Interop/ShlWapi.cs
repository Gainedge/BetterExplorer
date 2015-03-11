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
using System.Text;

#pragma warning disable 1591

namespace BExplorer.Shell.Interop {
	public class ShlWapi {
		[DllImport("shlwapi.dll")]
		public static extern Int32 StrRetToBuf(ref STRRET pstr, IntPtr pidl,
											   StringBuilder pszBuf,
											   UInt32 cchBuf);

		[DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
		public static extern long StrFormatByteSize(
						long fileSize
						, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer
						, int bufferSize);

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int StrCmpLogicalW(string psz1, string psz2);


		/// <summary>
		/// Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
		/// </summary>
		/// <param name="filesize">The numeric value to be converted.</param>
		/// <returns>the converted string</returns>
		public static string StrFormatByteSize(long filesize) {
			StringBuilder sb = new StringBuilder(11);
			StrFormatByteSize(filesize, sb, sb.Capacity);
			return sb.ToString();
		}
	}
}
