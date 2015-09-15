using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GS.Common.IO {
	public class UnmanagedBufferPool {

		public static IntPtr Alloc(int pageSize) {
			return NativeMethods.VirtualAlloc(IntPtr.Zero, (IntPtr)pageSize, NativeMethods.AllocationType.LARGE_PAGES,
				NativeMethods.MemoryProtection.EXECUTE_READWRITE);
		}
	}
}
