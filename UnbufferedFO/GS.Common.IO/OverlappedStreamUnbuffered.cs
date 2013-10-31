using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace GS.Common.IO
{
    /// <summary>
    /// Overlapped Unbuffered file operations
    /// </summary>
    public class OverlappedStreamUnbuffered : OverlappedStream
    {
        #region privates

        private static readonly uint _systemPageSize;

        #endregion privates

        /// <summary>
        /// Represents additional options for creating unbuffered overlapped file stream.
        /// </summary>
        /// 
        [Flags]
        public enum UnbufferedFileOptions : uint
        {
            None = 0,
            WriteThrough = 0x80000000,
            DeleteOnClose = 0x04000000,
            OpenReparsePoint = 0x00200000,
            All = WriteThrough | DeleteOnClose | OpenReparsePoint,
        }

        static OverlappedStreamUnbuffered()
        {
            NativeMethods.SYSTEM_INFO sysInfo = new NativeMethods.SYSTEM_INFO();
            NativeMethods.GetSystemInfo(ref sysInfo);
            _systemPageSize = sysInfo.dwPageSize;
        }

        /// <summary>
        /// Gets the size of system memory page. Usually 4096 on x86/AMD64.
        /// </summary>
        public static uint SystemPageSize { get { return _systemPageSize; } }

        /// <summary>
        /// Initializes a new instance of the OverlappedStreamUnbuffered class
        /// </summary>
        /// <param name="path">A relative or absolute path for the file.  UNC long path file names are supported.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="options">A constant that specifies additional file options.</param>
        /// <param name="attributes">A constant that specifies file attributes which are set when file is created.</param>
        public OverlappedStreamUnbuffered(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share,
            UnbufferedFileOptions options,
            FileAttributes attributes = FileAttributes.Normal) :
            base(path, mode, access, share, unchecked((uint)options | (uint)attributes) | NativeMethods.FILE_FLAG_NO_BUFFERING | NativeMethods.FILE_FLAG_OVERLAPPED)
        {
        }

#if DEBUG
        ~OverlappedStreamUnbuffered()
        {
            System.Diagnostics.Debug.Fail(String.Format("You forgot to Dispose this instance of {0}.", this.GetType().Name));
        }
#endif

        /// <summary>
        /// Begins an asynchronous unbuffered read operation
        /// </summary>
        /// <param name="pBuffer">The unmanaged buffer to read the data into. 
        /// Buffer should be aligned in memory to device sector size boundary. 
        /// Buffer should not be deallocated or moved until operation is completed.</param>
        /// <param name="fileOffset">File pointer</param>
        /// <param name="numberOfBytesToRead">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>An IAsyncResult that represents the asynchronous read, which could still be pending.</returns>
        public IAsyncResult BeginRead(IntPtr pBuffer, UInt64 fileOffset, UInt32 numberOfBytesToRead, AsyncCallback callback, object state)
        {
            if (pBuffer == IntPtr.Zero)
                throw new ArgumentNullException("pBuffer");
            CheckNotDisposed();

            AsyncJob job = null;
            try
            {
                job = new AsyncJob(callback, state, fileOffset, null);
                UInt32 numberOfBytesRead; bool result;
                unsafe
                {
                    result = NativeMethods.ReadFile(_handle, pBuffer, numberOfBytesToRead, out numberOfBytesRead, job.OverlappedPtr);
                }

                if (result)
                    job.CompleteSynchronously();
                else
                    CheckErrorPending();

                AsyncJob ret = job; job = null;
                return ret;
            }
            finally
            {
                if (job != null)
                    job.Dispose();
            }
        }

        /// <summary>
        /// Begins an asynchronous unbuffered write operation
        /// </summary>
        /// <param name="pBuffer">The unmanaged buffer to read the data from. 
        /// Buffer should be aligned in memory to device sector size boundary. 
        /// Buffer should not be deallocated or moved until operation is completed.</param>
        /// <param name="fileOffset">File pointer</param>
        /// <param name="numberOfBytesToWrite">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An IAsyncResult that represents the asynchronous write, which could still be pending.</returns>
        public IAsyncResult BeginWrite(IntPtr pBuffer, UInt64 fileOffset, UInt32 numberOfBytesToWrite, AsyncCallback callback, object state)
        {
            if (pBuffer == IntPtr.Zero)
                throw new ArgumentNullException("pBuffer");
            CheckNotDisposed();

            AsyncJob job = null;
            try
            {
                job = new AsyncJob(callback, state, fileOffset, null);
                UInt32 numberOfBytesWritten; bool result;
                unsafe
                {
                    result = NativeMethods.WriteFile(_handle, pBuffer, numberOfBytesToWrite, out numberOfBytesWritten, job.OverlappedPtr);
                }

                if (result)
                    job.CompleteSynchronously();
                else
                    CheckErrorPending();

                AsyncJob ret = job; job = null;
                return ret;
            }
            finally
            {
                if (job != null)
                    job.Dispose();
            }
        }

        /// <summary>
        /// Reads data from a file and stores it in an array of buffers
        /// </summary>
        /// <param name="segments">Array of pointers to data buffers. 
        /// Each buffer must be at least the size of a system memory page and must be aligned 
        /// on a system memory page size boundary <seealso cref="SystemPageSize"/>. 
        /// The system reads one system memory page of data into each buffer. 
        /// Buffers should not be deallocated or moved until operation is completed.</param>
        /// <param name="fileOffset">File pointer</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>An IAsyncResult that represents the asynchronous read, which could still be pending.</returns>
        public IAsyncResult BeginReadScatter(
            IntPtr[] segments,
            UInt64 fileOffset,
            AsyncCallback callback,
            object state)
        {
            if (segments == null)
                throw new ArgumentNullException("segments");
            CheckNotDisposed();

            NativeMethods.FILE_SEGMENT_ELEMENT[] nativeSegments = new NativeMethods.FILE_SEGMENT_ELEMENT[segments.Length + 1];
            for (int i = 0; i < segments.Length; i++)
                nativeSegments[i].Buffer = segments[i];

            UInt32 numberOfBytesToRead = (uint)segments.Length * _systemPageSize;

            AsyncJob job = null;
            try
            {
                job = new AsyncJob(callback, state, fileOffset, nativeSegments);
                bool result;
                unsafe
                {
                    fixed (NativeMethods.FILE_SEGMENT_ELEMENT* ps = &nativeSegments[0])
                    {
                        result = NativeMethods.ReadFileScatter(_handle, ps, numberOfBytesToRead, IntPtr.Zero, job.OverlappedPtr);
                    }
                }

                if (result)
                    job.CompleteSynchronously();
                else
                    CheckErrorPending();

                AsyncJob ret = job; job = null;
                return ret;

            }
            finally
            {
                if (job != null)
                    job.Dispose();
            }
        }

        /// <summary>
        /// Retrieves data from an array of buffers and writes the data to a file.
        /// </summary>
        /// <param name="segments">Array of pointers to data buffers. 
        /// Each buffer must be at least the size of a system memory page and must be aligned 
        /// on a system memory page size boundary <seealso cref="SystemPageSize"/>. 
        /// The system writes one system memory page of data into each buffer. 
        /// Buffers should not be deallocated or moved until operation is completed.</param>
        /// <param name="fileOffset">File pointer</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An IAsyncResult that represents the asynchronous write, which could still be pending.</returns>
        public IAsyncResult BeginWriteGather(
            IntPtr[] segments,
            UInt64 fileOffset,
            AsyncCallback callback,
            object state)
        {
            if (segments == null)
                throw new ArgumentNullException("segments");
            CheckNotDisposed();

            NativeMethods.FILE_SEGMENT_ELEMENT[] nativeSegments = new NativeMethods.FILE_SEGMENT_ELEMENT[segments.Length + 1];
            for (int i = 0; i < segments.Length; i++)
                nativeSegments[i].Buffer = segments[i];

            UInt32 numberOfBytesToWrite = (uint)segments.Length * _systemPageSize;

            AsyncJob job = null;
            try
            {
                job = new AsyncJob(callback, state, fileOffset, nativeSegments);
                bool result;
                unsafe
                {
                    fixed (NativeMethods.FILE_SEGMENT_ELEMENT* ps = &nativeSegments[0])
                    {
                        result = NativeMethods.WriteFileGather(_handle, ps, numberOfBytesToWrite, IntPtr.Zero, job.OverlappedPtr);
                    }
                }

                if (result)
                    job.CompleteSynchronously();
                else
                    CheckErrorPending();

                AsyncJob ret = job; job = null;
                return ret;
            }
            finally
            {
                if (job != null)
                    job.Dispose();
            }
        }
    }
}
