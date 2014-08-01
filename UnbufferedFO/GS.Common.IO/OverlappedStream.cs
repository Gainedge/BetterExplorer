using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace GS.Common.IO {
	[Serializable]
	public class OverlappedStreamException : IOException {
		public OverlappedStreamException() {
		}

		public OverlappedStreamException(string message)
			: base(message) {
		}

		public OverlappedStreamException(string message, Exception innerException)
			: base(message, innerException) {
		}
	}

	/// <summary>
	/// Base class for OverlappedStreamBuffered and OverlappedStreamUnbuffered
	/// </summary>
	public abstract class OverlappedStream : IDisposable {
		/// <summary>
		/// File handle
		/// </summary>
		protected readonly SafeFileHandle _handle;

		internal OverlappedStream(
				string path,
				FileMode mode,
				FileAccess access,
				FileShare share,
				uint file_flags
		) {
			if (mode == FileMode.Append)
				throw new NotSupportedException("Append mode is not supported in OverlappedStream");
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			uint file_access =
					(((access & FileAccess.Read) != 0) ? NativeMethods.GENERIC_READ : 0U) |
					(((access & FileAccess.Write) != 0) ? NativeMethods.GENERIC_WRITE : 0U);
			uint file_share = unchecked(((uint)share & ~(uint)FileShare.Inheritable));
			uint file_creation = unchecked((uint)mode);

			_handle = NativeMethods.CreateFileW(path, file_access, file_share, IntPtr.Zero, file_creation, file_flags, IntPtr.Zero);
			ASSERT(!_handle.IsInvalid);

			ASSERT(ThreadPool.BindHandle(_handle));
		}


		/// <summary>
		/// Flushes the buffers of a specified file and causes all buffered data to be written to a file.
		/// </summary>
		public void Flush() {
			CheckNotDisposed();
			ASSERT(NativeMethods.FlushFileBuffers(_handle));
		}

		/// <summary>
		/// Set length of the file
		/// </summary>
		/// <param name="value">New file length</param>
		public void SetLength(long value) {
			CheckNotDisposed();
			Int64 newPos;
			ASSERT(NativeMethods.SetFilePointerEx(_handle, value, out newPos, NativeMethods.FILE_BEGIN));
			ASSERT(NativeMethods.SetEndOfFile(_handle));
		}

		/// <summary>
		/// Get length of the file
		/// </summary>
		/// <returns>File length in bytes</returns>
		public long GetLength() {
			CheckNotDisposed();
			Int64 size;
			ASSERT(NativeMethods.GetFileSizeEx(_handle, out size));
			return size;
		}

		public bool GetOverlappedResult(IAsyncResult ar, out uint bytes, bool wait) {
			CheckNotDisposed();
			AsyncJob job = ar as AsyncJob;
			if (job == null) throw new ArgumentException("Invalid argument", "asyncResult");
			unsafe {
				return NativeMethods.GetOverlappedResult(_handle, job.OverlappedPtr, out bytes, wait);
			}
		}

		/// <summary>
		/// Cancel all pending IO requests issued by the calling thread for current file.
		/// </summary>
		/// <returns>true if success</returns>
		public bool CancelAsyncIOForThread() {
			CheckNotDisposed();
			return NativeMethods.CancelIo(_handle);
		}

		/// <summary>
		/// Waits for the pending asynchronous read to complete and frees resources.
		/// </summary>
		/// <param name="ar">The reference to the pending asynchronous request to wait for.</param>
		/// <returns>Number of bytes transferred</returns>
		public UInt32 EndRead(IAsyncResult ar) {
			return OverlappedStream.EndOperation(ar, false);
		}

		/// <summary>
		/// Waits for the pending asynchronous write to complete and frees resources.
		/// </summary>
		/// <param name="ar">The reference to the pending asynchronous request to wait for.</param>
		/// <returns>Number of bytes transferred</returns>
		public UInt32 EndWrite(IAsyncResult ar) {
			return OverlappedStream.EndOperation(ar, false);
		}

		/// <summary>
		/// Waits for the pending asynchronous operation to complete and frees resources.
		/// </summary>
		/// <param name="ar">The reference to the pending asynchronous request to wait for.</param>
		/// <returns>Number of bytes transferred</returns>
		public static UInt32 EndOperation(IAsyncResult ar) {
			return EndOperation(ar, false);
		}

		/// <summary>
		/// Waits for the pending asynchronous operation to complete and frees resources.
		/// </summary>
		/// <param name="ar">The reference to the pending asynchronous request to wait for.</param>
		/// <param name="throwOnError">When true, method throws <see cref="OverlappedStreamException"/> if any error detected.</param>
		/// <returns>Number of bytes transferred</returns>
		public static UInt32 EndOperation(IAsyncResult ar, bool throwOnError) {
			AsyncJob job = ar as AsyncJob;
			if (job == null) throw new ArgumentException("Invalid argument", "asyncResult");

			job.WaitForCompletion();
			if (throwOnError)
				ASSERT(job.ErrorCode == 0, unchecked((int)job.ErrorCode));
			return job.NumberOfBytesTransferred;
		}

		internal static void ASSERT(bool result) {
			if (result) return;
			var ex = new System.ComponentModel.Win32Exception();
			throw new OverlappedStreamException(ex.Message, ex);
		}

		internal static void ASSERT(bool result, int error) {
			if (!result) {
				var ex = new System.ComponentModel.Win32Exception(error);
				throw new OverlappedStreamException(ex.Message, ex);
			}
		}

		internal static void CheckErrorPending() {
			int error = Marshal.GetLastWin32Error();
			if (error != NativeMethods.ERROR_IO_PENDING) {
				ASSERT(false, error);
			}
		}

		#region IDisposable

		public void Dispose() {
			Dispose(true);
		}

		bool _disposed = false;
		protected virtual void Dispose(bool disposing) {
			if (!_disposed) {
				if (disposing) {
					if (_handle != null && !_handle.IsInvalid) {
						_handle.Close();
					}
					GC.SuppressFinalize(this);
				}
				_disposed = true;
			}
		}

		protected void CheckNotDisposed() {
			if (_disposed) throw new ObjectDisposedException(this.GetType().Name);
		}

		#endregion
	}
}
