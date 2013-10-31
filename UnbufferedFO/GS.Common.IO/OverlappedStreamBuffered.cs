using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace GS.Common.IO
{
    /// <summary>
    /// Overlapped Buffered file operations
    /// </summary>
    public class OverlappedStreamBuffered : OverlappedStream
    {
        /// <summary>
        /// Represents additional options for creating buffered overlapped file stream.
        /// </summary>
        [Flags]
        public enum BufferedFileOptions : uint
        {
            None = 0,
            WriteThrough = 0x80000000,
            DeleteOnClose = 0x04000000,
            OpenReparsePoint = 0x00200000,
            SequentialScan = 0x08000000,
            RandomAccess = 0x10000000,
        }

        /// <summary>
        /// Initializes a new instance of the OverlappedStreamBuffered class
        /// </summary>
        /// <param name="path">A relative or absolute path for the file.  UNC long path file names are supported.</param>
        /// <param name="mode">A constant that determines how to open or create the file.</param>
        /// <param name="access">A constant that determines how the file can be accessed.</param>
        /// <param name="share">A constant that determines how the file will be shared by processes.</param>
        /// <param name="options">A constant that specifies additional file options.</param>
        /// <param name="attributes">A constant that specifies file attributes which are set when file is created.</param>
        public OverlappedStreamBuffered(
            string path,
            FileMode mode,
            FileAccess access,
            FileShare share,
            BufferedFileOptions options,
            FileAttributes attributes = FileAttributes.Normal) : 
            base(path, mode, access, share, unchecked((uint)options | (uint)attributes) | NativeMethods.FILE_FLAG_OVERLAPPED)
        {
        }

#if DEBUG
        ~OverlappedStreamBuffered()
        {
            System.Diagnostics.Debug.Fail(String.Format("You forgot to Dispose this instance of {0}.", this.GetType().Name));
        }
#endif

        /// <summary>
        /// Begins an asynchronous read operation
        /// </summary>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="fileOffset">File pointer</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>An IAsyncResult that represents the asynchronous read, which could still be pending.</returns>
        public IAsyncResult BeginRead(
            byte[] buffer,
            long fileOffset,
            int offset,
            int count,
            AsyncCallback callback,
            object state
        )
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            CheckNotDisposed();
            AsyncJob job = null;
            try
            {
                job = new AsyncJob(callback, state, (ulong)fileOffset, buffer);
                UInt32 numberOfBytesRead; bool result;
                unsafe
                {
                    fixed (byte* pb = &buffer[offset])
                    {
                        result = NativeMethods.ReadFile(_handle, new IntPtr(pb), (uint)count, out numberOfBytesRead, job.OverlappedPtr);
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
        /// Begins an asynchronous write operation
        /// </summary>
        /// <param name="buffer">The buffer with data to write.</param>
        /// <param name="fileOffset">File pointer</param>
        /// <param name="offset">The byte offset in buffer at which to begin reading.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An IAsyncResult that represents the asynchronous write, which could still be pending.</returns>
        public IAsyncResult BeginWrite(
            byte[] buffer,
            long fileOffset,
            int offset,
            int count,
            AsyncCallback callback,
            object state
        )
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            CheckNotDisposed();
            AsyncJob job = null;
            try
            {
                job = new AsyncJob(callback, state, (ulong)fileOffset, buffer);
                UInt32 numberOfBytesWritten; bool result;
                unsafe 
                {
                    fixed (byte* pb = &buffer[offset])
                    {
                        result = NativeMethods.WriteFile(_handle, new IntPtr(pb), (uint)count, out numberOfBytesWritten, job.OverlappedPtr);
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
