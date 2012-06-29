using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;

	/// <summary>
	/// Abstract base class of Ace types.
	/// </summary>
	public abstract class Ace
	{
		internal static Ace Create(MemoryMarshaler m)
		{
			IntPtr initialPtr = m.Ptr;	// Save current ptr

			Debug.Assert(Marshal.SizeOf(typeof(ACE_HEADER)) == 4);
			ACE_HEADER head = (ACE_HEADER)m.ParseStruct(typeof(ACE_HEADER), false);
			Ace ace;
			switch(head.AceType)
			{
				case AceType.ACCESS_ALLOWED_ACE_TYPE:
					ace = new AceAccessAllowed(m);
					break;

				case AceType.ACCESS_DENIED_ACE_TYPE:
					ace = new AceAccessDenied(m);
					break;

				// Object ACE not yet supported
/*
				case AceType.ACCESS_ALLOWED_OBJECT_ACE_TYPE:
					ace = new AceAccessAllowedObject(m);
					break;

				case AceType.ACCESS_DENIED_OBJECT_ACE_TYPE:
					ace = new AceAccessDeniedObject(m);
					break;
*/
				default:
					throw new NotSupportedException("Unsupported ACE type: " + head.AceType);
			}

			// Restore initial ptr and move forward the size of the ACE
			m.Ptr = initialPtr;
			m.Advance(head.AceSize);
			return ace;
		}


		protected ACE_HEADER _header;
		protected AccessType _accessType;
		protected Sid _sid;

		protected void BaseInit(AceType type, int size, AceFlags flags, Sid sid, AccessType accessType)
		{
			if (size >= ushort.MaxValue)
				throw new ArgumentException("Ace size is limited to an 16-bit integer", "size");
			if (size <= ACE_HEADER.SizeOf)
				throw new ArgumentException("Ace size must be at least the size of an ACE_HEADER", "size");

			_header.AceType = type;
			_header.AceSize = (ushort)size;
			_header.AceFlags = flags;
			_accessType = accessType;
			_sid = sid;
		}
		protected void BaseInit(ACE_HEADER header, AccessType accessType, Sid sid)
		{
			_header = header;
			_accessType = accessType;
			_sid = sid;
		}
		/// <summary>
		///  Sanety check of the state of our members.
		///  We don't want to make this method Debug-only, because we are dealing with 
		///  security objects!
		/// </summary>
		protected void CheckInvariant()
		{
			int headerSize = _header.AceSize;
			int compSize = OffsetOfSid() + _sid.Size;
			if (headerSize != compSize)
			{
				string msg = string.Format(
					"Invariant of Ace is not verified (size is {0} instead of {1})", 
					headerSize, compSize);
				throw new InvalidOperationException(msg);
			}

			if (_sid == null)
				throw new InvalidOperationException(
					"Invariant of Ace is not verified: sid member is null");
		}
		public AceType Type
		{
			get
			{
				CheckInvariant();
				return _header.AceType;
			}
		}
		public AceFlags Flags
		{
			get
			{
				CheckInvariant();
				return _header.AceFlags;
			}
		}
		public int Size
		{
			get
			{
				CheckInvariant();
				return _header.AceSize;
			}
		}
		public AccessType AccessType
		{
			get
			{
				CheckInvariant();
				return _accessType;
			}
		}
		public Sid Sid
		{
			get
			{
				CheckInvariant();
				return _sid;
			}
		}
		/// <summary>
		///  Derived classes must implement this method to return the native representation
		///  of the Win32 ACE as an array of bytes.
		/// </summary>
		/// <returns></returns>
		public abstract byte[] GetNativeACE();
		/// <summary>
		///  Return the offset of the "SID" struct inside this ACE.
		/// </summary>
		/// <returns></returns>
		protected abstract int OffsetOfSid();
	}
}
