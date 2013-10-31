using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace GS.Common.IO
{
    [SuppressUnmanagedCodeSecurity()]
    internal unsafe static class NativeMethods
    {
        #region constants

        internal const uint INFINITE = unchecked((uint)-1);

        internal const int ERROR_IO_PENDING = 997;
        internal const uint ERROR_IO_INCOMPLETE = 996;
        internal const uint ERROR_NOACCESS = 998;
        internal const uint ERROR_HANDLE_EOF = 38;

        internal const int ERROR_FILE_NOT_FOUND = 0x2;
        internal const int ERROR_PATH_NOT_FOUND = 0x3;
        internal const int ERROR_INVALID_DRIVE = 0x15;


        internal const uint FILE_BEGIN = 0;
        internal const uint FILE_CURRENT = 1;
        internal const uint FILE_END = 2;

        internal const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        internal const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        internal const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        internal const uint INVALID_HANDLE_VALUE = unchecked((uint)-1);

        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint GENERIC_EXECUTE = 0x20000000;
        internal const uint GENERIC_ALL = 0x10000000;

        internal const uint READ_CONTROL = 0x00020000;
        internal const uint FILE_READ_ATTRIBUTES = 0x0080;
        internal const uint FILE_READ_DATA = 0x0001;
        internal const uint FILE_READ_EA = 0x0008;
        internal const uint STANDARD_RIGHTS_READ = READ_CONTROL;
        internal const uint FILE_APPEND_DATA = 0x0004;
        internal const uint FILE_WRITE_ATTRIBUTES = 0x0100;
        internal const uint FILE_WRITE_DATA = 0x0002;
        internal const uint FILE_WRITE_EA = 0x0010;
        internal const uint STANDARD_RIGHTS_WRITE = READ_CONTROL;

        internal const uint FILE_GENERIC_READ =
            FILE_READ_ATTRIBUTES
            | FILE_READ_DATA
            | FILE_READ_EA
            | STANDARD_RIGHTS_READ;
        internal const uint FILE_GENERIC_WRITE =
            FILE_WRITE_ATTRIBUTES
            | FILE_WRITE_DATA
            | FILE_WRITE_EA
            | STANDARD_RIGHTS_WRITE
            | FILE_APPEND_DATA;

        internal const uint FILE_SHARE_DELETE = 0x00000004;
        internal const uint FILE_SHARE_READ = 0x00000001;
        internal const uint FILE_SHARE_WRITE = 0x00000002;

        internal const uint CREATE_ALWAYS = 2;
        internal const uint CREATE_NEW = 1;
        internal const uint OPEN_ALWAYS = 4;
        internal const uint OPEN_EXISTING = 3;
        internal const uint TRUNCATE_EXISTING = 5;

        internal const uint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
        internal const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        internal const uint FILE_FLAG_OPEN_NO_RECALL = 0x00100000;
        internal const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const uint FILE_FLAG_RANDOM_ACCESS = 0x10000000;
        internal const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;
        internal const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
        internal const uint FILE_ATTRIBUTE_ENCRYPTED = 0x4000;

        #endregion

        #region structs & delegates

        [StructLayout(LayoutKind.Explicit)]
        public struct _PROCESSOR_INFO_UNION
        {
            [FieldOffset(0)]
            internal uint dwOemId;
            [FieldOffset(0)]
            internal ushort wProcessorArchitecture;
            [FieldOffset(2)]
            internal ushort wReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            internal _PROCESSOR_INFO_UNION uProcessorInfo;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort dwProcessorLevel;
            public ushort dwProcessorRevision;
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        internal struct FILE_SEGMENT_ELEMENT
        {
            [FieldOffset(0)]
            public IntPtr Buffer;
            [FieldOffset(0)]
            public UInt64 Alignment;
        }

        internal delegate void OVERLAPPED_COMPLETION_ROUTINE(
            UInt32 dwErrorCode,
            UInt32 dwNumberOfBytesTransfered,
            NativeOverlapped* lpOverlapped);

        #endregion

        #region imports

        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeFileHandle CreateFileW(
            string lpFileName,
            UInt32 dwDesiredAccess,
            UInt32 dwShareMode,
            IntPtr/*LPSECURITY_ATTRIBUTES*/ lpSecurityAttributes,
            UInt32 dwCreationDisposition,
            UInt32 dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool GetDiskFreeSpaceW(
            string lpRootPathName,
            [Out] out UInt32 lpSectorsPerCluster,
            [Out] out UInt32 lpBytesPerSector,
            [Out] out UInt32 lpNumberOfFreeClusters,
            [Out] out UInt32 lpTotalNumberOfClusters);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool GetOverlappedResult(
            SafeFileHandle hFile,
            NativeOverlapped* lpOverlapped,
            [Out] out UInt32 lpNumberOfBytesTransferred,
            bool bWait);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool GetFileSizeEx(SafeFileHandle hFile, out Int64 lpFileSize);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hFile);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool SetEndOfFile(SafeFileHandle hFile);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool SetFilePointerEx(SafeFileHandle hFile, Int64 liDistanceToMove, out Int64 lpNewFilePointer, UInt32 dwMoveMethod);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool ReadFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            UInt32 nNumberOfBytesToRead,
            [Out] out UInt32 lpNumberOfBytesRead,
            NativeOverlapped* lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool WriteFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            UInt32 nNumberOfBytesToWrite,
            [Out] out UInt32 lpNumberOfBytesWritten,
            NativeOverlapped* lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool WriteFileGather(
            SafeFileHandle hFile,
            FILE_SEGMENT_ELEMENT* aSegmentArray,
            UInt32 nNumberOfBytesToWrite,
            IntPtr lpReserved,
            NativeOverlapped* lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool ReadFileScatter(
            SafeFileHandle hFile,
            FILE_SEGMENT_ELEMENT* aSegmentArray,
            UInt32 nNumberOfBytesToRead,
            IntPtr lpReserved,
            NativeOverlapped* lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool CancelIoEx(
            SafeFileHandle hFile,
            NativeOverlapped* lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CancelIo(SafeFileHandle hFile);

        #endregion
    }
}
