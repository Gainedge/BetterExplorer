using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace GS.Common.IO {
	/// <summary>
	/// Internal class, wraps Overlapped structure, completion port callback and IAsyncResult
	/// </summary>
	sealed class AsyncJob : IAsyncResult, IDisposable {
		#region privates

		private readonly AsyncCallback _callback;
		private readonly object _state;

		private readonly object _eventHandle = new object();
		private bool _completedSynchronously = false;
		private bool _completed = false;
		private uint _numberOfBytesTransferred = 0;
		private uint _errorCode = 0;
		private readonly unsafe NativeOverlapped* _nativeOverlapped;

		#endregion

		/// <summary>
		/// Create instance, automatically allocates NativeOverlapped structure
		/// </summary>
		/// <param name="callback">User specified callback</param>
		/// <param name="state">User specified state</param>
		/// <param name="fileOffset">Start position</param>
		/// <param name="userData">An object or array of objects representing the input or output buffer for the operation. Buffer is pinned until object is disposed.</param>
		public AsyncJob(AsyncCallback callback, object state, UInt64 fileOffset, object userData) {
			_callback = callback;
			_state = state;
			Overlapped ov = new Overlapped(unchecked((int)(fileOffset & 0xFFFFFFFF)), unchecked((int)((fileOffset >> 32) & 0xFFFFFFFF)), IntPtr.Zero, this);
			unsafe { _nativeOverlapped = ov.UnsafePack(completionCallback, userData); }
		}

#if DEBUG
		~AsyncJob() {
			System.Diagnostics.Debug.Fail(String.Format("You forgot to Dispose this instance of {0}.", this.GetType().Name));
		}
#endif

		#region IDisposable

		bool _disposed = false;
		public void Dispose() {
			if (_disposed) return;

			unsafe {
				Overlapped.Unpack(_nativeOverlapped);
				Overlapped.Free(_nativeOverlapped);
			}

			_disposed = true;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region data accessors

		public unsafe NativeOverlapped* OverlappedPtr { get { return _nativeOverlapped; } }
		public uint NumberOfBytesTransferred { get { return _numberOfBytesTransferred; } }
		public uint ErrorCode { get { return _errorCode; } }

		#endregion

		public void CompleteSynchronously() {
			_completedSynchronously = true;
		}

		public void WaitForCompletion() {
			lock (_eventHandle) {
				while (!_completed)
					Monitor.Wait(_eventHandle);
			}
		}

		#region IAsyncResult Members

		public object AsyncState {
			get { return _state; }
		}

		public WaitHandle AsyncWaitHandle {
			get { return null; }
		}

		public bool CompletedSynchronously {
			get { return _completedSynchronously; }
		}

		public bool IsCompleted {
			get { return _completed; }
		}

		#endregion

		#region privates

		private unsafe void completionCallback(uint errorCode, uint numBytes, NativeOverlapped* pOVERLAP) {
			try {
				if (errorCode != 0) {
					System.Diagnostics.Trace.TraceError("OverlappedStream GetQueuedCompletionStatus error: {0}", errorCode);
				}

				lock (_eventHandle) {
					System.Diagnostics.Debug.Assert(!_completed);

					_errorCode = errorCode;
					_numberOfBytesTransferred = numBytes;
					_completed = true;

					if (_callback != null)
						_callback.Invoke(this);

					Monitor.Pulse(_eventHandle);
				}
			} catch (Exception ex) {
				System.Diagnostics.Trace.TraceError("OverlappedStream.completionCallback error, {0}", ex.Message);
			} finally {
				this.Dispose();
			}
		}

		#endregion privates
	}
}
