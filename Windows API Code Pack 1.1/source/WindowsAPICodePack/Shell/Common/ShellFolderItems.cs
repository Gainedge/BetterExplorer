//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.WindowsAPICodePack.Internal;
using System.Threading;
using System.Windows;

namespace Microsoft.WindowsAPICodePack.Shell {
	class ShellFolderItems : IEnumerator<ShellObject> {
		#region Private Fields

		private BExplorer.Shell.Interop.IEnumIDList nativeEnumIdList;
		private ShellObject currentItem;
		ShellContainer nativeShellFolder;

		#endregion

		#region Internal Constructor

		internal ShellFolderItems(ShellContainer nativeShellFolder) {
			this.nativeShellFolder = nativeShellFolder;

			HResult hr = nativeShellFolder.NativeShellFolder.EnumObjects(
				IntPtr.Zero,
				ShellFolderEnumerationOptions.Folders | ShellFolderEnumerationOptions.NonFolders | ShellFolderEnumerationOptions.IncludeHidden | ShellFolderEnumerationOptions.EnableAsync | ShellFolderEnumerationOptions.FastItems,
				out nativeEnumIdList);

			/*
			HResult hr = nativeShellFolder.NativeShellFolder.EnumObjects(
				IntPtr.Zero,
				BExplorer.Shell.Interop.SHCONTF.FOLDERS |
				BExplorer.Shell.Interop.SHCONTF.NONFOLDERS | BExplorer.Shell.Interop.SHCONTF.INCLUDEHIDDEN |
				BExplorer.Shell.Interop.SHCONTF.ENABLE_ASYNC | BExplorer.Shell.Interop.SHCONTF.FASTITEMS,
				out nativeEnumIdList);
			*/

			if (!CoreErrorHelper.Succeeded(hr)) {
				if (hr == HResult.Canceled) {
					throw new System.IO.FileNotFoundException();
				}
				else {
					throw new ShellException(hr);
				}
			}


		}

		#endregion

		#region IEnumerator<ShellObject> Members

		public ShellObject Current {
			get {
				return currentItem;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose() {
			if (nativeEnumIdList != null) {
				Marshal.ReleaseComObject(nativeEnumIdList);
				nativeEnumIdList = null;
			}
		}

		#endregion

		#region IEnumerator Members

		object IEnumerator.Current {
			get { return currentItem; }

		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool MoveNext() {
			if (nativeEnumIdList == null) return false;

			IntPtr item;
			uint numItemsReturned;
			uint itemsRequested = 1;
			var hr = nativeEnumIdList.Next(itemsRequested, out item, out numItemsReturned);

			if (numItemsReturned < itemsRequested || hr != BExplorer.Shell.Interop.HResult.S_OK) return false;

			currentItem = ShellObjectFactory.Create(item, nativeShellFolder);

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Reset() {
			if (nativeEnumIdList != null) {
				nativeEnumIdList.Reset();
			}
		}


		#endregion
	}
}
