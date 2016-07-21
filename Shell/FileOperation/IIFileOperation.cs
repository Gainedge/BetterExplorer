using BExplorer.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using BExplorer.Shell._Plugin_Interfaces;
using System.Linq;

namespace BExplorer.Shell
{
	/// <summary>Contains file operations that has a callback for UI operations/display</summary>
	/// <remarks>Every operation uses the callback if possible</remarks>
	public class IIFileOperation : IDisposable
	{
		private static readonly Guid CLSID_FileOperation = new Guid("3ad05575-8857-4850-9277-11b85bdb8e09");
		private static readonly Type _fileOperationType = Type.GetTypeFromCLSID(CLSID_FileOperation);

		private bool _disposed;
		private IFileOperation _fileOperation;
		private FileOperationProgressSink _callbackSink;
		private uint _sinkCookie;

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
		public IIFileOperation(FileOperationProgressSink callbackSink, IntPtr owner, Boolean isRecycle)
		{
			_callbackSink = callbackSink;
			_fileOperation = (IFileOperation)Activator.CreateInstance(_fileOperationType);

			var Flags = isRecycle ? FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_ALLOWUNDO : FileOperationFlags.FOF_NOCONFIRMMKDIR;
			_fileOperation.SetOperationFlags(Flags);

			if (_callbackSink != null) _sinkCookie = _fileOperation.Advise(_callbackSink);
			if (owner != IntPtr.Zero) _fileOperation.SetOwnerWindow((uint)owner);
		}

		/// <summary>
		/// Copies the source item into the destination
		/// </summary>
		/// <param name="source">The item being copied</param>
		/// <param name="destination">The location to be copied to</param>
		public void CopyItem(IShellItem source, IListItemEx destination)
		{
			ThrowIfDisposed();
			_fileOperation.CopyItem(source, destination.ComInterface, null, _callbackSink);
		}

		/// <summary>
		/// Moves the source item into the destination with a new name
		/// </summary>
		/// <param name="source">The item being moved</param>
		/// <param name="destination">The location to be moved to</param>
		/// <param name="newName">The new name of the file</param>
		public void MoveItem(IShellItem source, IShellItem destination, string newName)
		{
			ThrowIfDisposed();
			_fileOperation.MoveItem(source, destination, newName, _callbackSink);
		}

		/// <summary>
		/// Renames the source item
		/// </summary>
		/// <param name="source">The IShellItem to be renamed</param>
		/// <param name="newName">The new name</param>
		public void RenameItem(IShellItem source, string newName)
		{
			ThrowIfDisposed();
			_fileOperation.RenameItem(source, newName, _callbackSink);
		}

		/// <summary>
		/// Deletes the source item
		/// </summary>
		/// <param name="source">The item to delete</param>
		public void DeleteItem(IShellItem source)
		{
			ThrowIfDisposed();
			_fileOperation.DeleteItem(source, _callbackSink);
		}

		/// <summary>
		/// Preforms PerformOperations
		/// </summary>
		public void PerformOperations()
		{
			ThrowIfDisposed();
			try
			{
				_fileOperation.PerformOperations();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Returns GetAnyOperationsAborted
		/// </summary>
		/// <returns></returns>
		public bool GetAnyOperationAborted()
		{
			ThrowIfDisposed();
			return this._fileOperation.GetAnyOperationsAborted();
		}

		private void ThrowIfDisposed()
		{
			if (_disposed) throw new ObjectDisposedException(GetType().Name);
		}

		/// <summary>
		/// Disposes of the object
		/// </summary>
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				if (_callbackSink != null) _fileOperation.Unadvise(_sinkCookie);
				Marshal.FinalReleaseComObject(_fileOperation);
			}
		}
	}
}
