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
using System.IO;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace BExplorer.Shell.Interop {
	public class Kernel32 {

		/// <summary>
		/// The GetDriveType function determines whether a disk drive is a removable, fixed, CD-ROM, RAM disk, or network drive
		/// </summary>
		/// <param name="lpRootPathName">A pointer to a null-terminated string that specifies the root directory and returns information about the disk.A trailing backslash is required. If this parameter is NULL, the function uses the root of the current directory.</param>
		[DllImport("kernel32.dll")]
		public static extern DriveType GetDriveType([MarshalAs(UnmanagedType.LPStr)] string lpRootPathName);

		public static bool IsThis64bitProcess() {
			return IntPtr.Size == 8;
		}

		/*
		[DllImport("kernel32.dll")]
		public static extern IntPtr GlobalLock(IntPtr hMem);
		*/

		/*
		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr LoadLibrary(string lpFileName);

		public enum FINDEX_INFO_LEVELS {
			FindExInfoStandard = 0,
			FindExInfoBasic = 1
		}

		public enum FINDEX_SEARCH_OPS {
			FindExSearchNameMatch = 0,
			FindExSearchLimitToDirectories = 1,
			FindExSearchLimitToDevices = 2
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct WIN32_FIND_DATA {
			public uint dwFileAttributes;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternateFileName;
		}

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FindClose(IntPtr hFindFile);

		public const int FIND_FIRST_EX_CASE_SENSITIVE = 1;
		public const int FIND_FIRST_EX_LARGE_FETCH = 2;

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr FindFirstFileEx(
				string lpFileName,
				FINDEX_INFO_LEVELS fInfoLevelId,
				out WIN32_FIND_DATA lpFindFileData,
				FINDEX_SEARCH_OPS fSearchOp,
				IntPtr lpSearchFilter,
				int dwAdditionalFlags);
		*/

		/*
		public static bool Is64bitProcess(Process proc) {
			return !Is32bitProcess(proc);
		}
		*/
		/*
		public static bool Is32bitProcess(Process proc) {
			if (!IsThis64bitProcess()) return true; // we're in 32bit mode, so all are 32bit

			foreach (ProcessModule module in proc.Modules) {
				try {
					string fname = Path.GetFileName(module.FileName).ToLowerInvariant();
					if (fname.Contains("wow64")) {
						return true;
					}
				}
				catch {
					// wtf
				}
			}

			return false;
		}
		*/
	}
}