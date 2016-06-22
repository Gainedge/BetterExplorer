using System;

namespace GS.Common.IO
{
    public class UnmanagedBufferPool {

        public static IntPtr Alloc(int pageSize) => 
            NativeMethods.VirtualAlloc(IntPtr.Zero, (IntPtr)pageSize, NativeMethods.AllocationType.LARGE_PAGES, NativeMethods.MemoryProtection.EXECUTE_READWRITE);
    }
}
