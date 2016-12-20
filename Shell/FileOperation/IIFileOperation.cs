using BExplorer.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using BExplorer.Shell._Plugin_Interfaces;
using System.Linq;

namespace BExplorer.Shell {
	/// <summary>Contains file operations that has a callback for UI operations/display</summary>
	/// <remarks>Every operation uses the callback if possible</remarks>
	public class IIFileOperation : IDisposable {
		private static readonly Guid _CLSIDFileOperation = new Guid("3ad05575-8857-4850-9277-11b85bdb8e09");
		private static readonly Type _FileOperationType = Type.GetTypeFromCLSID(_CLSIDFileOperation);

		private bool _Disposed;
		private IFileOperation _FileOperation;
		private FileOperationProgressSink _CallbackSink;
		private uint _SinkCookie;
		private Boolean _IsCopyInSameFolder { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IIFileOperation() : this(null, false) { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner">The object that owns the file?</param>
		public IIFileOperation(IntPtr owner) : this(null, owner, false) { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isRecycle">Allows Undo (likely adds to recycle bin)</param>
		public IIFileOperation(Boolean isRecycle) : this(null, isRecycle) { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner">The object that owns the file?</param>
		/// <param name="isRecycle">Allows Undo (likely adds to recycle bin)</param>
		public IIFileOperation(IntPtr owner, Boolean isRecycle) : this(null, owner, isRecycle) { }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="callbackSink"></param>
		/// <param name="isRecycle"></param>
		public IIFileOperation(FileOperationProgressSink callbackSink, Boolean isRecycle) : this(callbackSink, IntPtr.Zero, isRecycle) { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callbackSink"></param>
		/// <param name="owner"></param>
		/// <param name="isRecycle"></param>
		public IIFileOperation(FileOperationProgressSink callbackSink, IntPtr owner, Boolean isRecycle) {
			this._CallbackSink = callbackSink;
			this._FileOperation = (IFileOperation)Activator.CreateInstance(_FileOperationType);

			var flags = isRecycle ? FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_ALLOWUNDO : FileOperationFlags.FOF_NOCONFIRMMKDIR;
			this._FileOperation.SetOperationFlags(flags);

			if (this._CallbackSink != null) this._SinkCookie = this._FileOperation.Advise(_CallbackSink);
			if (owner != IntPtr.Zero) this._FileOperation.SetOwnerWindow((uint)owner);
		}

		public IIFileOperation(FileOperationProgressSink callbackSink, IntPtr owner, Boolean isRecycle, Boolean isCopyInSameFolder) {
			this._CallbackSink = callbackSink;
			this._IsCopyInSameFolder = isCopyInSameFolder;
			_FileOperation = (IFileOperation)Activator.CreateInstance(_FileOperationType);

			if (!isRecycle)
				this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_NOCONFIRMMKDIR);

			if (_CallbackSink != null) _SinkCookie = _FileOperation.Advise(_CallbackSink);
			if (owner != IntPtr.Zero) _FileOperation.SetOwnerWindow((uint)owner);
		}

		/// <summary>
		/// Declares a new item that is to be created in a specified location. Does this silently and automatically renames items if needed.
		/// </summary>
		/// <param name="destinationFolder">The destination folder that will contain the new item.</param>
		/// <param name="name">The new item name</param>
		/// <param name="attributes">A bitwise value that specifies the file system attributes for the file or folder. See GetFileAttributes for possible values</param>
		public void NewItem(IListItemEx destinationFolder, String name, FileAttributes attributes) {
			this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_RENAMEONCOLLISION | FileOperationFlags.FOF_SILENT);
			this._FileOperation.NewItem(destinationFolder.ComInterface, attributes, name, null, null);
		}

		/// <summary>
		/// Copies the source item into the destination
		/// </summary>
		/// <param name="source">The item being copied</param>
		/// <param name="destination">The location to be copied to</param>
		public void CopyItem(IShellItem source, IListItemEx destination) {
			this.ThrowIfDisposed();
			if (this._IsCopyInSameFolder) {
				this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_RENAMEONCOLLISION | FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMMKDIR);
			}
			this._FileOperation.CopyItem(source, destination.ComInterface, null, null);
		}

		/// <summary>
		/// Copies items to a destination
		/// </summary>
		/// <param name="source">The items you want to copy</param>
		/// <param name="destination">The destination you want to copy the <paramref name="source"/> items</param>
		public void CopyItems(IShellItemArray source, IListItemEx destination) {
			this.ThrowIfDisposed();
			if (this._IsCopyInSameFolder) {
				this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_RENAMEONCOLLISION | FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMMKDIR);
			}
			this._FileOperation.CopyItems(source, destination.ComInterface);
		}

		/// <summary>
		/// Moves the source item into the destination with a new name
		/// </summary>
		/// <param name="source">The item being moved</param>
		/// <param name="destination">The location to be moved to</param>
		/// <param name="newName">The new name of the file</param>
		public void MoveItem(IShellItem source, IListItemEx destination, string newName) {
			this.ThrowIfDisposed();
			this._FileOperation.MoveItem(source, destination.ComInterface, newName, null);
		}

		/// <summary>
		/// Moves items to a destination
		/// </summary>
		/// <param name="source">The item being moved</param>
		/// <param name="destination">The location to be moved to</param>
		public void MoveItems(IShellItemArray source, IListItemEx destination) {
			this.ThrowIfDisposed();
			this._FileOperation.MoveItems(source, destination.ComInterface);
		}

		/// <summary>
		/// Renames the source item
		/// </summary>
		/// <param name="source">The IShellItem to be renamed</param>
		/// <param name="newName">The new name</param>
		public void RenameItem(IShellItem source, string newName) {
			this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_SILENT);
			this._FileOperation.RenameItem(source, newName, null);
		}

		/// <summary>
		/// Declares a single item that is to be deleted. (Exception when <see cref="_Disposed"/> is <c>True</c>) 
		/// </summary>
		/// <param name="source">The item to be deleted</param>
		/// <exception cref="ObjectDisposedException">When <see cref="_Disposed"/> is <c>True</c></exception>
		public void DeleteItem(IListItemEx source) {
			this.ThrowIfDisposed();
			this._FileOperation.DeleteItem(source.ComInterface, null);
		}

		/// <summary>
		/// Declares a set of items that are to be deleted. (Exception when <see cref="_Disposed"/> is <c>True</c>) 
		/// </summary>
		/// <param name="source">The items to be deleted</param>
		/// <exception cref="ObjectDisposedException">When <see cref="_Disposed"/> is <c>True</c></exception>
		public void DeleteItems(IShellItemArray source) {
			this.ThrowIfDisposed();
			this._FileOperation.DeleteItems(source);
		}

		/// <summary>
		/// Preforms PerformOperations
		/// </summary>
		public void PerformOperations() {
			this.ThrowIfDisposed();
			try {
				this._FileOperation.PerformOperations();
			}
			catch {
			}
		}

		/// <summary>
		/// Returns GetAnyOperationsAborted
		/// </summary>
		/// <returns></returns>
		public bool GetAnyOperationAborted() {
			this.ThrowIfDisposed();
			return this._FileOperation.GetAnyOperationsAborted();
		}

		/// <summary>Is the item <see cref="_Disposed">Disposed</see>?</summary>
		private void ThrowIfDisposed() {
			if (this._Disposed) throw new ObjectDisposedException(GetType().Name);
		}

		/// <summary>
		/// Disposes of the object
		/// </summary>
		public void Dispose() {
			if (!this._Disposed) {
				this._Disposed = true;
				if (this._CallbackSink != null) this._FileOperation.Unadvise(this._SinkCookie);
				Marshal.FinalReleaseComObject(this._FileOperation);
			}
		}
	}
}
