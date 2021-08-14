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

namespace BExplorer.Shell.Interop.VistaBridge {
	internal class ShellItemImpl : IDisposable, IShellItem {

		public IntPtr Pidl { get; private set; }

		public ShellItemImpl(IntPtr pidl, bool owner) {
			Pidl = owner ? pidl : Shell32.ILClone(pidl);
		}

		~ShellItemImpl() {
			Dispose(false);
		}

		public HResult BindToHandler(IntPtr pbc, Guid bhid, Guid riid, out IntPtr obj) {
			if (riid == typeof(IShellFolder).GUID) {
				obj = Marshal.GetIUnknownForObject(GetIShellFolder());
				return HResult.S_OK;
			}
			else {
				throw new InvalidCastException();
			}
		}

		public void Dispose() {
			Dispose(true);
		}

		public HResult GetParent(out IShellItem ppsi) {
			IntPtr pidl = Shell32.ILClone(Pidl);
			if (Shell32.ILRemoveLastID(pidl)) {
				ppsi = new ShellItemImpl(pidl, true);
				return HResult.S_OK;
			}
			else {
				ppsi = null;
				return HResult.MK_E_NOOBJECT;
			}
		}

		public IntPtr GetDisplayName(SIGDN sigdnName) {
			if (sigdnName == SIGDN.FILESYSPATH) {
				var result = new StringBuilder(512);
				if (!Shell32.SHGetPathFromIDList(Pidl, result)) throw new ArgumentException();
				return Marshal.StringToHGlobalUni(result.ToString());
			}
			else {
				IShellFolder parentFolder = GetParent().GetIShellFolder();
				IntPtr childPidl = Shell32.ILFindLastID(Pidl);
				var builder = new StringBuilder(512);
				var strret = new STRRET();

				parentFolder.GetDisplayNameOf(childPidl, (SHGNO)((int)sigdnName & 0xffff), out strret);
				ShlWapi.StrRetToBuf(ref strret, childPidl, builder, (uint)builder.Capacity);
				return Marshal.StringToHGlobalUni(builder.ToString());
			}
		}

		public HResult GetAttributes(SFGAO sfgaoMask, out SFGAO psfgaoAttribs) {
			IShellFolder parentFolder = GetParent().GetIShellFolder();
			SFGAO result = sfgaoMask;

			parentFolder.GetAttributesOf(1, new IntPtr[] { Shell32.ILFindLastID(Pidl) }, ref result);
			psfgaoAttribs = result & sfgaoMask;
			return HResult.S_OK;
		}

		public int Compare(IShellItem psi, SICHINT hint) {
			var other = (ShellItemImpl)psi;
			ShellItemImpl myParent = GetParent();
			ShellItemImpl theirParent = other.GetParent();

			if (Shell32.ILIsEqual(myParent.Pidl, theirParent.Pidl)) {
				return myParent.GetIShellFolder().CompareIDs((SHCIDS)hint, Shell32.ILFindLastID(Pidl), Shell32.ILFindLastID(other.Pidl));
			}
			else {
				return 1;
			}
		}

		protected void Dispose(bool dispose) {
			Shell32.ILFree(Pidl);
		}

		private ShellItemImpl GetParent() {
			IntPtr pidl = Shell32.ILClone(Pidl);
			return Shell32.ILRemoveLastID(pidl) ? new ShellItemImpl(pidl, true) : this;
		}

		private IShellFolder GetIShellFolder() {
			IShellFolder desktop = Shell32.SHGetDesktopFolder();
			IntPtr desktopPidl;

			Shell32.SHGetSpecialFolderLocation(IntPtr.Zero, CSIDL.DESKTOP, out desktopPidl); ;

			if (Shell32.ILIsEqual(Pidl, desktopPidl)) {
				return desktop;
			}
			else {
				IntPtr result;
				desktop.BindToObject(Pidl, IntPtr.Zero, typeof(IShellFolder).GUID, out result);
				return (IShellFolder)Marshal.GetTypedObjectForIUnknown(result, typeof(IShellFolder));
			}
		}
	}
}