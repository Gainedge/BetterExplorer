using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace BExplorer.Shell {
	public class IIFileOperation : IDisposable {
		[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode, PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Interface)]
		private static extern object SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, ref Guid riid);

		private static readonly Guid CLSID_FileOperation = new Guid("3ad05575-8857-4850-9277-11b85bdb8e09");
		private static readonly Type _fileOperationType = Type.GetTypeFromCLSID(CLSID_FileOperation);
		private static Guid _shellItemGuid = typeof(IShellItem).GUID;

		private bool _disposed;
		private IFileOperation _fileOperation;
		private FileOperationProgressSink _callbackSink;
		private uint _sinkCookie;

		public IIFileOperation() : this(null, false) { }
		public IIFileOperation(IntPtr owner, Boolean isRecycle) : this(null, owner, isRecycle) { }
		public IIFileOperation(IntPtr owner) : this(null, owner, false) { }
		public IIFileOperation(Boolean isRecycle) : this(null, isRecycle) { }
		public IIFileOperation(FileOperationProgressSink callbackSink, Boolean isRecycle) : this(callbackSink, IntPtr.Zero, isRecycle) { }
		public IIFileOperation(FileOperationProgressSink callbackSink, IntPtr owner, Boolean isRecycle) {
			_callbackSink = callbackSink;
			_fileOperation = (IFileOperation)Activator.CreateInstance(_fileOperationType);
			//TestDialog dlg = new TestDialog();
			//_fileOperation.SetProgressDialog(dlg);
			if (isRecycle)
				_fileOperation.SetOperationFlags(FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_ALLOWUNDO);
			else
				_fileOperation.SetOperationFlags(FileOperationFlags.FOF_NOCONFIRMMKDIR);
			if (_callbackSink != null) _sinkCookie = _fileOperation.Advise(_callbackSink);
			if (owner != IntPtr.Zero) _fileOperation.SetOwnerWindow((uint)owner);
		}

		[Obsolete("Not Used", true)]
		public void CopyItem(string source, string destination, string newName) {
			ThrowIfDisposed();
			using (ComReleaser<IShellItem> sourceItem = CreateShellItem(source))
			using (ComReleaser<IShellItem> destinationItem = CreateShellItem(destination)) {
				_fileOperation.CopyItem(sourceItem.Item, destinationItem.Item, newName, null);
			}
		}

		public void CopyItem(IShellItem source, IShellItem destination, string newName) {
			ThrowIfDisposed();
			_fileOperation.CopyItem(source, destination, newName, null);
		}

		[Obsolete("Not Used", true)]
		public void MoveItem(string source, string destination, string newName) {
			ThrowIfDisposed();
			using (ComReleaser<IShellItem> sourceItem = CreateShellItem(source))
			using (ComReleaser<IShellItem> destinationItem = CreateShellItem(destination)) {
				_fileOperation.MoveItem(sourceItem.Item, destinationItem.Item, newName, null);
			}
		}

		public void MoveItem(IShellItem source, IShellItem destination, string newName) {
			ThrowIfDisposed();
			_fileOperation.MoveItem(source, destination, newName, null);
		}

		[Obsolete("Not Used", true)]
		public void RenameItem(string source, string newName) {
			ThrowIfDisposed();
			using (ComReleaser<IShellItem> sourceItem = CreateShellItem(source)) {
				_fileOperation.RenameItem(sourceItem.Item, newName, null);
			}
		}

		public void RenameItem(IShellItem source, string newName) {
			ThrowIfDisposed();
			_fileOperation.RenameItem(source, newName, null);
		}

		[Obsolete("Not Used", true)]
		public void DeleteItem(string source) {
			ThrowIfDisposed();
			using (ComReleaser<IShellItem> sourceItem = CreateShellItem(source)) {
				_fileOperation.DeleteItem(sourceItem.Item, null);
			}
		}

		public void DeleteItem(IShellItem source) {
			ThrowIfDisposed();
			_fileOperation.DeleteItem(source, null);
		}

		/*
		[Obsolete("Not Used", true)]
		public void NewItem(string folderName, string name, FileAttributes attrs) {
			ThrowIfDisposed();
			using (ComReleaser<IShellItem> folderItem = CreateShellItem(folderName)) {
				_fileOperation.NewItem(folderItem.Item, attrs, name, string.Empty, _callbackSink);
			}
		}

		[Obsolete("Not Used", true)]
		public void NewItem(IShellItem folderName, string name, FileAttributes attrs) {
			ThrowIfDisposed();
			_fileOperation.NewItem(folderName, attrs, name, string.Empty, _callbackSink);
		}
		*/
		public void PerformOperations() {
			ThrowIfDisposed();
			try {
				_fileOperation.PerformOperations();
			}
			catch {

			}
		}

		private void ThrowIfDisposed() {
			if (_disposed) throw new ObjectDisposedException(GetType().Name);
		}

		public void Dispose() {
			if (!_disposed) {
				_disposed = true;
				if (_callbackSink != null) _fileOperation.Unadvise(_sinkCookie);
				Marshal.FinalReleaseComObject(_fileOperation);
			}
		}

		private static ComReleaser<IShellItem> CreateShellItem(string path) {
			return new ComReleaser<IShellItem>((IShellItem)SHCreateItemFromParsingName(path, null, ref _shellItemGuid));
		}

	}
}
