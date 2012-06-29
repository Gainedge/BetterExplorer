using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;
	/// <summary>
	///  Abstract base class for AceAccessAllowed and AceAccessDenied.
	///  This class relies on the fact that the ACCESS_ALLOWED_ACE and ACCESS_DENIED_ACE
	///  are identical.
	/// </summary>
	public abstract class AceAccess : Ace
	{
		private bool _allowed;
		internal AceAccess(MemoryMarshaler m, bool allowed)
		{
			_allowed = allowed;

			ACCESS_ALLOWED_ACE ace = (ACCESS_ALLOWED_ACE)m.ParseStruct(typeof(ACCESS_ALLOWED_ACE), false);
			m.Advance(OffsetOfSid());
			BaseInit(ace.Header, (AccessType)ace.Mask, new Sid(m.Ptr));
		}
		protected AceAccess(Sid sid, AceFlags flags, AccessType accessType, bool allowed)
		{
			_allowed = allowed;

			BaseInit(
				(allowed ? AceType.ACCESS_ALLOWED_ACE_TYPE : AceType.ACCESS_DENIED_ACE_TYPE),
				OffsetOfSid() + sid.Size, flags, sid, accessType);
		}
		/// <summary>
		///  The native (Win32) representation of this Ace as an managed array of bytes.
		///  The array can be pinned using unsafe code to pass it to a Win32 function requiring
		///  a pointer to an ACE structure.
		/// </summary>
		/// <returns></returns>
		public override byte[] GetNativeACE()
		{
			CheckInvariant();
											
			byte[] aceBytes = new byte[this.Size];

			// First copy the ACE structure
			UnsafeCopyAceToNativeArray(aceBytes);

			// Now copy the Sid data
			Array.Copy(_sid.GetNativeSID(), 0, aceBytes, OffsetOfSid(), _sid.Size);
			return aceBytes;
		}
		protected override int OffsetOfSid()
		{
			Debug.Assert(ACCESS_ALLOWED_ACE.SizeOf == 12);
			Debug.Assert(ACCESS_ALLOWED_ACE.SizeOf == ACCESS_DENIED_ACE.SizeOf);
			Debug.Assert(ACCESS_ALLOWED_ACE.SidOffset == ACCESS_DENIED_ACE.SidOffset);

			return ACCESS_ALLOWED_ACE.SidOffset;
		}
		protected virtual unsafe void UnsafeCopyAceToNativeArray(byte[] aceBytes)
		{
			ACCESS_ALLOWED_ACE aceStruct;
			aceStruct.Header = this._header;
			aceStruct.Mask = (AccessMask)this._accessType;
			aceStruct.SidStart = 0;
	
			// First copy the ACE structure
			fixed(byte *pace = aceBytes)
			{
				Marshal.StructureToPtr(aceStruct, (IntPtr)pace, false);
			}
		}
		public bool IsAllowed
		{
			get
			{
				return _allowed;
			}
		}
		public bool IsInherited
		{
			get
			{
				return (this.Flags & AceFlags.INHERITED_ACE) != 0;
			}
		}
		public virtual bool IsObjectAce
		{
			get
			{
				// Object ACE not yet supported
				return false;
			}
		}
	}

	/// <summary>
	///  An access allowed ACE
	/// </summary>
	public class AceAccessAllowed : AceAccess
	{
		/// <summary>
		///  Internal: Create an ACE from a given memory marshaler
		/// </summary>
		internal AceAccessAllowed(MemoryMarshaler m) 
			: base(m, true)
		{
		}
		/// <summary>
		///  Create a new Ace given a Sid, an access type and an set of flags
		/// </summary>
		/// <param name="sid">The sid (must be valid)</param>
		/// <param name="accessMask">The access accessMask</param>
		/// <param name="flags">The list of flags</param>
		public AceAccessAllowed(Sid sid, AccessType accessType, AceFlags flags)
			: base(sid, flags, accessType, true)
		{
		}
		/// <summary>
		///  Create a new Ace given a Sid, an access type and a default set of flags
		/// </summary>
		/// <param name="sid">The sid (must be valid)</param>
		/// <param name="accessMask">The access accessMask</param>
		public AceAccessAllowed(Sid sid, AccessType accessType)
			: base(sid, 0, accessType, true)
		{
		}
	}
	/// <summary>
	///  An access denied ACE
	/// </summary>
	public class AceAccessDenied : AceAccess
	{
		internal AceAccessDenied(MemoryMarshaler m) : base(m, false)
		{
		}
		public AceAccessDenied(Sid sid, AccessType accessType, AceFlags flags)
			: base(sid, flags, accessType, false)
		{
		}
		public AceAccessDenied(Sid sid, AccessType accessType)
			: base(sid, 0, accessType, false)
		{
		}
	}
}
