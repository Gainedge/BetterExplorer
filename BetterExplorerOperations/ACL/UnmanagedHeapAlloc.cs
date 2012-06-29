using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	/// <summary>
	/// Summary description for UnmanagedHeapAlloc.
	/// </summary>
	internal class UnmanagedHeapAlloc : DisposableObject
	{
		private IntPtr _memPtr;
		private uint _cbLength;

		public UnmanagedHeapAlloc(uint cbLength)
		{
			_memPtr = Win32.AllocGlobal(cbLength);
			_cbLength = cbLength;
		}

		protected override void Dispose(bool disposing)
		{
			if (_memPtr != IntPtr.Zero)
			{
				Win32.FreeGlobal(_memPtr);
				_memPtr = IntPtr.Zero;
			}
		}
		public IntPtr Ptr
		{
			get
			{
				if (_memPtr == IntPtr.Zero)
					throw new NullReferenceException("Ptr member is not initialiazed or has already been disposed");

				return _memPtr;
			}
		}
		public uint Size
		{
			get
			{
				return _cbLength;
			}
		}
	}
}
