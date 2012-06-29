using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;
	using DWORD = System.UInt32;
	using BOOL = System.Int32;

	/// <summary>
	///  Abstract base class for DACL and SACL.
	/// </summary>
	public abstract class Acl : IEnumerable
	{
		private readonly AclRevision _revision;
		private Aces _aces = new Aces();
		private byte[] _nativeAcl = null;

		internal protected Acl(IntPtr pacl)
		{
			// If NULL for the pacl, create an empty DACL
			if (pacl == IntPtr.Zero)
			{
				_revision = AclRevision.ACL_REVISION;
				return;
			}
			MemoryMarshaler m = new MemoryMarshaler(pacl);
			ACL acl = (ACL)m.ParseStruct(typeof(ACL));
			_revision = acl.AclRevision;
			for(int i = 0; i < acl.AceCount; i++)
			{
				Ace ace = Ace.Create(m);
				_aces.Add(ace);
			}
		}
		/// <summary>
		///  Create an empty ACL with a the default revision
		/// </summary>
		protected Acl()
		{
			_revision = AclRevision.ACL_REVISION;
		}
		/// <summary>
		///  Create an empty ACL with a specific revision
		/// </summary>
		public Acl(AclRevision revision)
		{
			_revision = revision;
		}
		/// <summary>
		///  This function is internally called whenever the "_aces" array (content or reference)
		///  is updated, so that we reset the _nativeAces list.
		/// </summary>
		private void Dirty()
		{
			_nativeAcl = null;
		}
		public void SetEmpty()
		{
			_aces = new Aces();
			Dirty();
		}
		public void SetNull()
		{
			_aces = null;
			Dirty();
		}
		public bool IsEmpty
		{
			get
			{
				return (_aces != null) && (_aces.Count == 0);
			}
		}
		public bool IsNull
		{
			get
			{
				return (_aces == null);
			}
		}
		public int AceCount
		{
			get
			{
				return (_aces == null ? 0 : _aces.Count);
			}
		}
		public Ace GetAce(int index)
		{
			if (IsNull)
				throw new NullReferenceException("Can't access ACE because ACL is null");

			return _aces[index];
		}
		/// <summary>
		///  Remove all ACEs having 'sid' as their sid.
		/// </summary>
		/// <param name="sid"></param>
		/// <returns></returns>
		public bool RemoveAces(Sid sid)
		{
			if (IsNull)
				return false;

			// Remove from end to start to avoid indices issues
			bool found = false;
			for(int i = AceCount - 1; i >= 0; i--)
			{
				if (GetAce(i).Sid == sid)
				{
					found = true;
					_aces.RemoveAt(i);
				}
			}

			// If at least one was remove, mark us as "Dirty"
			if (found)
				Dirty();

			return found;
		}
		protected void AddAce(Ace ace)
		{
			if (_aces == null)
				_aces = new Aces();
			_aces.Add(ace);
			Dirty();
		}
		private unsafe byte[] UnsafeGetNativeACL()
		{
			if (IsNull)
				return null;

			if (_nativeAcl != null)
				return _nativeAcl;

			// Re-order ACEs if needed (DACL only)
			PrepareAcesForACL();

			// Compute total size (in bytes)
			int totalSize = ACL.SizeOf;
			for(int i = 0; i < AceCount; i++)
			{
				totalSize += GetAce(i).Size;
			}

			// 1st, copy the Win32 ACL struct
			//ACL acl;
			//acl.AceCount = (UInt16)AceCount;
			//acl.AclRevision = _revision;
			//acl.AclSize = 0; // will be updated later
			//acl.Sbz1 = 0;
			//acl.Sbz2 = 0;
			byte[] res = new byte[totalSize];
			fixed(byte *pacl = res)
			{
				BOOL rc = Win32.InitializeAcl((IntPtr)pacl, (uint)totalSize, (uint)_revision);
				Win32.CheckCall(rc);

				// 2nd, copy every ACE Win32 struct
				for(int i = 0; i < AceCount; i++)
				{
					byte[] aceBytes = GetAce(i).GetNativeACE();
					fixed(byte *pace = aceBytes)
					{
						rc = Win32.AddAce(
							(IntPtr)pacl, (uint)_revision, DWORD.MaxValue,
							(IntPtr)pace, (uint)aceBytes.Length);
						Win32.CheckCall(rc);
					}
				}
			}
			
			_nativeAcl = res;
			return res;
		}
		internal byte[] GetNativeACL()
		{
			try
			{
				return UnsafeGetNativeACL();
			}
			catch(Exception e)
			{
				throw new Exception("Error creating native representation of an ACL", e);
			}
		}
		protected void SetAce(int i, Ace ace)
		{
			_aces.SetAce(i, ace);
		}
		/// <summary>
		///  Derived class can reorder the _aces if needed.
		/// </summary>
		protected abstract void PrepareAcesForACL();

		#region Implementation of IEnumerable
		private static readonly ArrayList _emptyList = new ArrayList();
		public IEnumerator GetEnumerator()
		{
			if (_aces == null)
				return _emptyList.GetEnumerator();

			return _aces.GetEnumerator();
		}

		#endregion
	}
}
