using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	/// <summary>
	/// Summary description for MemoryMarshaler.
	/// </summary>
	public class MemoryMarshaler
	{
		private IntPtr _ptr;
		public MemoryMarshaler(IntPtr ptr)
		{
			_ptr = ptr;
		}

		public void Advance(int cbLength)
		{
			long p = _ptr.ToInt64();
			p += cbLength;
			_ptr = (IntPtr)p;
		}
		public object ParseStruct(System.Type type)
		{
			return ParseStruct(type, true);
		}
		public object ParseStruct(System.Type type, bool moveOffset)
		{
			object o = Marshal.PtrToStructure(_ptr, type);
			if (moveOffset)
				Advance(Marshal.SizeOf(type));
			return o;
		}
		public byte ParseUInt8()
		{
			return (byte)ParseStruct(typeof(byte));
		}
		public UInt16 ParseUInt16()
		{
			return (UInt16)ParseStruct(typeof(UInt16));
		}
		public UInt32 ParseUInt32()
		{
			return (UInt32)ParseStruct(typeof(UInt32));
		}
		public UInt64 ParseUInt64()
		{
			return (UInt64)ParseStruct(typeof(UInt64));
		}
		public IntPtr Ptr
		{
			get
			{
				return _ptr;
			}
			set
			{
				_ptr = value;
			}
		}
	}
}
