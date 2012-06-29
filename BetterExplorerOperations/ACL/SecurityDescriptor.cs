using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;
	using DWORD = System.UInt32;
	using WORD = System.UInt16;
	using BOOL = System.Int32;
	using HKEY = System.IntPtr;
	using HANDLE = System.IntPtr;

	/// <summary>
	/// Summary description for SecurityDescriptor.
	/// </summary>
	public class SecurityDescriptor : DisposableObject
	{
		public static SecurityDescriptor GetSecurityInfo(
			HANDLE handle,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo)
		{
			Sid sidOwner;
			Sid sidGroup;
			Dacl dacl;
			Sacl sacl;
			SecurityDescriptor secDesc;
			Win32Helpers.GetSecurityInfo(handle, objectType, securityInfo,
				out sidOwner, out sidGroup, out dacl, out sacl,	out secDesc);

			return secDesc;
		}
		public static SecurityDescriptor GetNamedSecurityInfo(
			string objectName,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo)
		{
			Sid sidOwner;
			Sid sidGroup;
			Dacl dacl;
			Sacl sacl;
			SecurityDescriptor secDesc;
			Win32Helpers.GetNamedSecurityInfo(objectName, objectType, securityInfo,
				out sidOwner, out sidGroup, out dacl, out sacl,	out secDesc);

			return secDesc;
		}
		public static SecurityDescriptor GetRegistryKeySecurity(HKEY hKey, SECURITY_INFORMATION secInfo)
        {
            DWORD cbLength = 0;
            int rc = Win32.RegGetKeySecurity(hKey, secInfo, IntPtr.Zero, ref cbLength);
            if (rc != Win32.SUCCESS)
                Win32.SetLastError((uint)rc);

            switch(rc)
            {
                case Win32.SUCCESS:
                    throw new InvalidOperationException("Unexpected return code from RegGetKeySecurity()");

                case Win32.ERROR_INSUFFICIENT_BUFFER:
                    IntPtr secDescPtr = Win32.AllocGlobal(cbLength);
                    try
                    {
                        rc = Win32.RegGetKeySecurity(hKey, secInfo, secDescPtr, ref cbLength);
                        if (rc != Win32.SUCCESS)
                        {
                            Win32.SetLastError((uint)rc);
                            Win32.ThrowLastError();
                        }

                        return new SecurityDescriptor(secDescPtr);
                    }
                    catch
                    {
                        Win32.FreeGlobal(secDescPtr);
                        throw;
                    }

                default:
                    Win32.ThrowLastError();
                    return null; // Never executed
            }
        }

		/// <summary>
		///  Return the security descriptor of a given filename
		/// </summary>
		/// <param name="fileName">The filename</param>
		/// <returns>The security descriptor with DACL, GROUP and OWNER information</returns>
		public static SecurityDescriptor GetFileSecurity(string fileName)
		{
			return GetFileSecurity(fileName, 
				SECURITY_INFORMATION.DACL_SECURITY_INFORMATION |
				//SECURITY_INFORMATION.SACL_SECURITY_INFORMATION | // SACL requires SE_SECURITY_NAME privilege
				SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION |
				SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION);
		}
		/// <summary>
		///  Return the selected components of the security descriptor of a given filename
		/// </summary>
		/// <param name="fileName">The filename</param>
		/// <param name="secInfo">The components of the security descriptor to return</param>
		/// <returns>The security descriptor</returns>
		public static SecurityDescriptor GetFileSecurity(string fileName, SECURITY_INFORMATION secInfo)
		{
			DWORD cbLength;
			BOOL rc = Win32.GetFileSecurity(fileName, secInfo, IntPtr.Zero, 0, out cbLength);
			switch(Win32.GetLastError())
			{
				case Win32.SUCCESS:
					throw new InvalidOperationException("Unexpected return code from GetFileSecurity()");

				case Win32.ERROR_INSUFFICIENT_BUFFER:
					IntPtr secDescPtr = Win32.AllocGlobal(cbLength);
					try
					{
						rc = Win32.GetFileSecurity(fileName, secInfo, secDescPtr, cbLength, out cbLength);
						Win32.CheckCall(rc);

						return new SecurityDescriptor(secDescPtr);
					}
					catch
					{
						Win32.FreeGlobal(secDescPtr);
						throw;
					}

				default:
					Win32.ThrowLastError();
					return null; // Never executed
			}
		}
		public static SecurityDescriptor GetKernelObjectSecurity(HANDLE handle)
		{
			return GetKernelObjectSecurity(handle, 
				SECURITY_INFORMATION.DACL_SECURITY_INFORMATION |
				//SECURITY_INFORMATION.SACL_SECURITY_INFORMATION | // SACL requires SE_SECURITY_NAME privilege
				SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION |
				SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION);
		}
		/// <summary>
		///  Return the selected components of the security descriptor of a given kernel object handle
		/// </summary>
		/// <param name="fileName">The kernel object handle</param>
		/// <param name="secInfo">The components of the security descriptor to return</param>
		/// <returns>The security descriptor</returns>
		public static SecurityDescriptor GetKernelObjectSecurity(HANDLE handle, SECURITY_INFORMATION secInfo)
		{
			DWORD cbLength;
			BOOL rc = Win32.GetKernelObjectSecurity(handle, secInfo, IntPtr.Zero, 0, out cbLength);
			switch(Win32.GetLastError())
			{
				case Win32.SUCCESS:
					throw new InvalidOperationException("Unexpected return code from GetKernelObjectSecurity()");

				case Win32.ERROR_INSUFFICIENT_BUFFER:
					IntPtr secDescPtr = Win32.AllocGlobal(cbLength);
					try
					{
						rc = Win32.GetKernelObjectSecurity(handle, secInfo, secDescPtr, cbLength, out cbLength);
						Win32.CheckCall(rc);

						return new SecurityDescriptor(secDescPtr);
					}
					catch
					{
						Win32.FreeGlobal(secDescPtr);
						throw;
					}

				default:
					Win32.ThrowLastError();
					return null; // Never executed
			}
		}

		// Pointer to unmanged memory containing the security descriptor
		private IntPtr _secDesc;
		private bool _useLocalFree;

		/// <summary>
		/// Internal: Create a security decriptor from a pointer to unmanged memory
		/// </summary>
		/// <param name="secDesc">The pointer to unmanged memory. This instance becomes the owner
		///  of the memory.</param>
		internal SecurityDescriptor(IntPtr secDesc)
			: this(secDesc, false)
		{
		}
		/// <summary>
		///  Create an emtpy security descriptor
		/// </summary>
		public SecurityDescriptor()
			: this(IntPtr.Zero, false)
		{
		}
		internal SecurityDescriptor(IntPtr secDesc, bool useLocalFree)
		{
			_secDesc = secDesc;
			_useLocalFree = useLocalFree;
		}
		protected override void Dispose(bool disposing)
		{
			Clear();
		}
		public void AllocateAndInitializeSecurityDescriptor()
		{
			Clear();

			IntPtr secDesc = Win32.AllocGlobal(SECURITY_DESCRIPTOR.SizeOf);
			try
			{
				BOOL rc = Win32.InitializeSecurityDescriptor(secDesc, 
					(DWORD)SecurityDescriptorRevision.SECURITY_DESCRIPTOR_REVISION);
				Win32.CheckCall(rc);
			}
			catch
			{
				Win32.FreeGlobal(secDesc);
				throw;
			}
			_secDesc = secDesc;
		}
		private void Clear()
		{
			if (_secDesc == IntPtr.Zero)
				return;

			if (! IsSelfRelative)
			{
				BOOL defaulted;
				IntPtr owner;
				BOOL rc = Win32.GetSecurityDescriptorOwner(_secDesc, out owner, out defaulted);
				if (rc != Win32.FALSE)
					Win32.FreeGlobal(owner);

				IntPtr group;
				rc = Win32.GetSecurityDescriptorGroup(_secDesc, out group, out defaulted);
				if (rc != Win32.FALSE)
					Win32.FreeGlobal(group);

				BOOL present;
				IntPtr dacl = IntPtr.Zero;
				rc = Win32.GetSecurityDescriptorDacl(_secDesc, out present, ref dacl, out defaulted);
				if (rc != Win32.FALSE)
					if (present != Win32.FALSE)
						Win32.FreeGlobal(dacl);

				IntPtr sacl = IntPtr.Zero;
				rc = Win32.GetSecurityDescriptorSacl(_secDesc, out present, ref sacl, out defaulted);
				if (rc != Win32.FALSE)
					if (present != Win32.FALSE)
						Win32.FreeGlobal(dacl);

			}
			if (_useLocalFree)
				Win32.LocalFree(_secDesc);
			else
				Win32.FreeGlobal(_secDesc);
			_secDesc = IntPtr.Zero;
		}
		private bool IsSelfRelative
		{
			get
			{
				return ((ControlFlags & SecurityDescriptorControlFlags.SE_SELF_RELATIVE) != 0);
			}
		}
		public SecurityDescriptorControlFlags ControlFlags
		{
			get
			{
				CheckIsValid();

				SecurityDescriptorControlFlags controlFlags;
				DWORD revision;
				BOOL rc = Win32.GetSecurityDescriptorControl(_secDesc, out controlFlags, out revision);
				Win32.CheckCall(rc);
				return (SecurityDescriptorControlFlags)controlFlags;
			}
		}
		public SecurityDescriptorRevision Revision
		{
			get
			{
				CheckIsValid();

				SecurityDescriptorControlFlags controlFlags;
				DWORD revision;
				BOOL rc = Win32.GetSecurityDescriptorControl(_secDesc, out controlFlags, out revision);
				Win32.CheckCall(rc);
				return (SecurityDescriptorRevision)revision;
			}
		}
		private bool AreFlagsSet(params SecurityDescriptorControlFlags []flags)
		{
			SecurityDescriptorControlFlags cf = this.ControlFlags;
			foreach(SecurityDescriptorControlFlags flag in flags)
			{
				if ((cf & flag) == 0)
					return false;
			}
			return true;
		}
		bool IsDaclDefaulted
		{
			get
			{
				return AreFlagsSet(
					SecurityDescriptorControlFlags.SE_DACL_PRESENT,
					SecurityDescriptorControlFlags.SE_DACL_DEFAULTED);
			}
		}
		bool IsDaclPresent
		{
			get
			{
				return AreFlagsSet(
					SecurityDescriptorControlFlags.SE_DACL_PRESENT);
			}
		}
		bool IsGroupDefaulted
		{
			get
			{
				return AreFlagsSet(
					SecurityDescriptorControlFlags.SE_GROUP_DEFAULTED);
			}
		}
		bool IsOwnerDefaulted
		{
			get
			{
				return AreFlagsSet(
					SecurityDescriptorControlFlags.SE_OWNER_DEFAULTED);
			}
		}
		bool IsSaclDefaulted
		{
			get
			{
				return AreFlagsSet(
					SecurityDescriptorControlFlags.SE_SACL_PRESENT,
					SecurityDescriptorControlFlags.SE_SACL_DEFAULTED);
			}
		}
		bool IsSaclPresent
		{
			get
			{
				return AreFlagsSet(
					SecurityDescriptorControlFlags.SE_SACL_PRESENT);
			}
		}
		public Dacl Dacl
		{
			get
			{
				CheckIsValid();

				BOOL present;
				BOOL defaulted;
				IntPtr dacl = IntPtr.Zero;
				BOOL rc = Win32.GetSecurityDescriptorDacl(_secDesc, out present, ref dacl, out defaulted);
				Win32.CheckCall(rc);
				if (present == Win32.FALSE)
					return null;
				else
					return new Dacl(dacl);
			}
			set
			{
				SetDacl(value);
			}
		}
		public Sacl Sacl
		{
			get
			{
				CheckIsValid();

				BOOL present;
				BOOL defaulted;
				IntPtr sacl = IntPtr.Zero;
				BOOL rc = Win32.GetSecurityDescriptorSacl(_secDesc, out present, ref sacl, out defaulted);
				Win32.CheckCall(rc);
				if (present == Win32.FALSE)
					return null;
				else
					return new Sacl(sacl);
			}
			set
			{
				SetSacl(value);
			}
		}
		public Sid Owner
		{
			get
			{
				CheckIsValid();

				BOOL defaulted;
				IntPtr owner;
				BOOL rc = Win32.GetSecurityDescriptorOwner(_secDesc, out owner, out defaulted);
				Win32.CheckCall(rc);
				return new Sid(owner);
			}
			set
			{
				SetOwner(value);
			}
		}
		public Sid Group
		{
			get
			{
				CheckIsValid();

				BOOL defaulted;
				IntPtr group;
				BOOL rc = Win32.GetSecurityDescriptorGroup(_secDesc, out group, out defaulted);
				Win32.CheckCall(rc);
				return new Sid(group);
			}
			set
			{
				SetGroup(value);
			}
		}
		public int Size
		{
			get
			{
				CheckIsValid();
				return (int)Win32.GetSecurityDescriptorLength(_secDesc);
			}
		}
		private void CheckIsValid()
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Security descriptor internal struct is NULL");
			}
		}

		public bool IsValid
		{
			get
			{
				return !IsNull;
			}
		}
		private bool IsNull
		{
			get
			{
				return (_secDesc == IntPtr.Zero);
			}
		}

		public void MakeSeflRelative()
		{
			if (IsNull || IsSelfRelative)
				return;

			DWORD dwLen = 0;
			BOOL rc = Win32.MakeSelfRelativeSD(_secDesc, IntPtr.Zero, ref dwLen);
			if (Marshal.GetLastWin32Error() != Win32.ERROR_INSUFFICIENT_BUFFER)
				Win32.ThrowLastError();

			IntPtr pSD = Win32.AllocGlobal((int)dwLen);
			try
			{
				rc = Win32.MakeSelfRelativeSD(_secDesc, pSD, ref dwLen);
				Win32.CheckCall(rc);

				Clear();
				_secDesc = pSD;
			}
			catch
			{
				Win32.FreeGlobal(pSD);
				throw;
			}
		}

		public void MakeAbsolute()
		{
			if (IsNull)
				return;

			if (!IsSelfRelative)
				return;


			DWORD dwSD = 0, dwOwner = 0, dwGroup = 0, dwDacl = 0, dwSacl = 0;
			BOOL rc = Win32.MakeAbsoluteSD(
				_secDesc, 
				IntPtr.Zero, ref dwSD, 
				IntPtr.Zero, ref dwDacl,
				IntPtr.Zero, ref dwSacl, 
				IntPtr.Zero, ref dwOwner, 
				IntPtr.Zero, ref dwGroup);

			if (Marshal.GetLastWin32Error() != Win32.ERROR_INSUFFICIENT_BUFFER)
				Win32.ThrowLastError();

			IntPtr secDesc = Win32.AllocGlobal(dwSD);
			try
			{
				IntPtr pDacl = Win32.AllocGlobal(dwDacl);
				try
				{
					IntPtr pSacl = Win32.AllocGlobal(dwSacl);
					try
					{
						IntPtr pOwner = Win32.AllocGlobal(dwOwner);
						try
						{
							IntPtr pGroup = Win32.AllocGlobal(dwGroup);
							try
							{
								rc = Win32.MakeAbsoluteSD(
									_secDesc, 
									secDesc, ref dwSD, pDacl, ref dwDacl, pSacl, ref dwSacl, 
									pOwner, ref dwOwner, pGroup, ref dwGroup);
								Win32.CheckCall(rc);

								Clear();
								_secDesc = secDesc;
							} 
							catch 
							{ 
								Win32.FreeGlobal(pGroup); 
								throw;
							}
						} 
						catch 
						{ 
							Win32.FreeGlobal(pOwner); 
							throw;
						}
					} 
					catch 
					{ 
						Win32.FreeGlobal(pSacl); 
						throw;
					}
				} 
				catch 
				{ 
					Win32.FreeGlobal(pDacl); 
					throw;
				}
			} 
			catch 
			{ 
				Win32.FreeGlobal(secDesc); 
				throw;
			}
		}

		public void SetOwner(Sid owner)
		{
			SetOwner(owner, false);
		}
		public void SetOwner(Sid owner, bool bDefaulted)
		{
			UnsafeSetOwner(this, owner, bDefaulted);
		}
		private static unsafe void UnsafeSetOwner(SecurityDescriptor secDesc, Sid owner, bool defaulted)
		{
			if (! owner.IsValid)
				throw new ArgumentException("SID must be valid to set as owner of a security descriptor", "owner");

			secDesc.MakeAbsolute();

			// First we have to get a copy of the old owner ptr, so that
			// we can free it if everything goes well.
			BOOL rc;
			IntPtr pOldOwner;
			if(!secDesc.IsNull)
			{
				BOOL oldDefaulted;
				rc = Win32.GetSecurityDescriptorOwner(secDesc._secDesc, out pOldOwner, out oldDefaulted);
				Win32.CheckCall(rc);
			}
			else
			{
				secDesc.AllocateAndInitializeSecurityDescriptor();
				pOldOwner = IntPtr.Zero;
			}


			DWORD cbSidSize = (DWORD)owner.Size;
			IntPtr pNewOwner = Win32.AllocGlobal(cbSidSize);
			try
			{
				// Copy the SID content to pNewOwner memory
				fixed (byte *pNewSid = owner.GetNativeSID())
				{
					rc = Win32.CopySid(cbSidSize, pNewOwner, (IntPtr)pNewSid);
					Win32.CheckCall(rc);
				}
				// Set the new owner SID
				rc = Win32.SetSecurityDescriptorOwner(secDesc._secDesc, 
					pNewOwner, (defaulted ? Win32.TRUE : Win32.FALSE));
				Win32.CheckCall(rc);

				// Now, we can free the old owner
				Win32.FreeGlobal(pOldOwner);
			}
			catch
			{
				Win32.FreeGlobal(pNewOwner);
				throw;
			}
		}

		public void SetGroup(Sid group)
		{
			SetGroup(group, false);
		}
		public void SetGroup(Sid group, bool defaulted)
		{
			UnsafeSetGroup(this, group, defaulted);
		}
		private static unsafe void UnsafeSetGroup(SecurityDescriptor secDesc, Sid group, bool defaulted)
		{
			if (! group.IsValid)
				throw new ArgumentException("SID must be valid to set as group of a security descriptor", "group");

			secDesc.MakeAbsolute();

			// First we have to get a copy of the old group ptr, so that
			// we can free it if everything goes well.
			BOOL rc;
			IntPtr pOldGroup;
			if(!secDesc.IsNull)
			{
				BOOL oldDefaulted;
				rc = Win32.GetSecurityDescriptorGroup(secDesc._secDesc, out pOldGroup, out oldDefaulted);
				Win32.CheckCall(rc);
			}
			else
			{
				secDesc.AllocateAndInitializeSecurityDescriptor();
				pOldGroup = IntPtr.Zero;
			}


			DWORD cbSidSize = (DWORD)group.Size;
			IntPtr pNewGroup = Win32.AllocGlobal(cbSidSize);
			try
			{
				// Copy the SID content to pNewGroup memory
				fixed (byte *pNewSid = group.GetNativeSID())
				{
					rc = Win32.CopySid(cbSidSize, pNewGroup, (IntPtr)pNewSid);
					Win32.CheckCall(rc);
				}
				// Set the new group SID
				rc = Win32.SetSecurityDescriptorGroup(secDesc._secDesc, 
					pNewGroup, (defaulted ? Win32.TRUE : Win32.FALSE));
				Win32.CheckCall(rc);

				// Now, we can free the old group
				Win32.FreeGlobal(pOldGroup);
			}
			catch
			{
				Win32.FreeGlobal(pNewGroup);
				throw;
			}
		}
		public void SetSacl(Sacl sacl)
		{
			SetSacl(sacl, false);
		}
		public void SetSacl(Sacl sacl, bool defaulted)
		{
			UnsafeSetSacl(this, sacl, defaulted);
		}
		private static void UnsafeSetSacl(SecurityDescriptor secDesc, Sacl sacl, bool defaulted)
		{
			if (sacl == null)
				throw new ArgumentException("Can't set null SACL on a security descriptor", "sacl");

			secDesc.MakeAbsolute();

			// First we have to get a copy of the old group ptr, so that
			// we can free it if everything goes well.
			BOOL rc;
			IntPtr pOldSacl = IntPtr.Zero;
			if(!secDesc.IsNull)
			{
				BOOL oldDefaulted, oldPresent;
				rc = Win32.GetSecurityDescriptorSacl(secDesc._secDesc, out oldPresent, ref pOldSacl, out oldDefaulted);
				Win32.CheckCall(rc);
			}
			else
			{
				secDesc.AllocateAndInitializeSecurityDescriptor();
			}


			IntPtr pNewSacl = IntPtr.Zero;
			try
			{
				if(!sacl.IsNull && !sacl.IsEmpty)
				{
					byte []pacl = sacl.GetNativeACL();
					pNewSacl = Win32.AllocGlobal(pacl.Length);
					Marshal.Copy(pacl, 0, pNewSacl, pacl.Length);
				}

				bool present = (sacl.IsNull || (pNewSacl != IntPtr.Zero));
				rc = Win32.SetSecurityDescriptorSacl(
					secDesc._secDesc, (present ? Win32.TRUE : Win32.FALSE),
					pNewSacl, (defaulted ?  Win32.TRUE : Win32.FALSE));
				Win32.CheckCall(rc);

				Win32.FreeGlobal(pOldSacl);
			}
			catch
			{
				Win32.FreeGlobal(pNewSacl);
                throw;
			}
		}
		public void SetDacl(Dacl dacl)
		{
			SetDacl(dacl, false);
		}
		public void SetDacl(Dacl dacl, bool defaulted)
		{
			if (dacl == null)
				throw new ArgumentException("Can't set null DACL on a security descriptor", "dacl");

			UnsafeSetDacl(this, dacl, defaulted);
		}
		public void SetNullDacl(bool defaulted)
		{
			UnsafeSetDacl(this, null, defaulted);
		}
		private static void UnsafeSetDacl(SecurityDescriptor secDesc, Dacl dacl, bool defaulted)
		{
			secDesc.MakeAbsolute();

			// First we have to get a copy of the old group ptr, so that
			// we can free it if everything goes well.
			BOOL rc;
			IntPtr pOldDacl = IntPtr.Zero;
			if(!secDesc.IsNull)
			{
				BOOL oldDefaulted, oldPresent;
				rc = Win32.GetSecurityDescriptorDacl(secDesc._secDesc, out oldPresent, ref pOldDacl, out oldDefaulted);
				Win32.CheckCall(rc);
			}
			else
			{
				secDesc.AllocateAndInitializeSecurityDescriptor();
			}


			IntPtr pNewDacl = IntPtr.Zero;
			try
			{
				if((dacl != null) && !dacl.IsNull && !dacl.IsEmpty)
				{
					byte []pacl = dacl.GetNativeACL();
					pNewDacl = Win32.AllocGlobal(pacl.Length);
					Marshal.Copy(pacl, 0, pNewDacl, pacl.Length);
				}

				bool present = ((dacl == null) || dacl.IsNull || (pNewDacl != IntPtr.Zero));
				rc = Win32.SetSecurityDescriptorDacl(
					secDesc._secDesc, (present ? Win32.TRUE : Win32.FALSE),
					pNewDacl, (defaulted ?  Win32.TRUE : Win32.FALSE));
				Win32.CheckCall(rc);

				Win32.FreeGlobal(pOldDacl);
			}
			catch
			{
				Win32.FreeGlobal(pNewDacl);
                throw;
			}
		}
		internal IntPtr Ptr
		{
			get
			{
				return _secDesc;
			}
		}
        public void SetFileSecurity(string fileName, SECURITY_INFORMATION secInfo)
        {
            Win32.CheckCall(Win32.SetFileSecurity(fileName, secInfo, this._secDesc));
        }
		public void SetKernelObjectSecurity(HANDLE handle, SECURITY_INFORMATION secInfo)
		{
			Win32.CheckCall(Win32.SetKernelObjectSecurity(handle, secInfo, this._secDesc));
		}
		public void SetRegistryKeySecurity(HKEY hKey, SECURITY_INFORMATION secInfo)
        {
            int rc = Win32.RegSetKeySecurity(hKey, secInfo, this._secDesc);
			if (rc != Win32.SUCCESS)
			{
				Win32.SetLastError((uint)rc);
				Win32.ThrowLastError();
			}
        }
		public void SetSecurityInfo(
			HANDLE handle,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo)
		{
			Sid ownerSid = (((securityInfo & SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION) == 0) ? null : this.Owner);
			Sid groupSid = (((securityInfo & SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION) == 0) ? null : this.Group);
			Dacl dacl = (((securityInfo & SECURITY_INFORMATION.DACL_SECURITY_INFORMATION) == 0) ? null : this.Dacl);
			Sacl sacl = (((securityInfo & SECURITY_INFORMATION.SACL_SECURITY_INFORMATION) == 0) ? null : this.Sacl);
			Win32Helpers.SetSecurityInfo(handle, objectType, securityInfo,
				ownerSid, groupSid, dacl, sacl);
		}
		public void SetNamedSecurityInfo(
			string objectName,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo)
		{
			Sid ownerSid = (((securityInfo & SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION) == 0) ? null : this.Owner);
			Sid groupSid = (((securityInfo & SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION) == 0) ? null : this.Group);
			Dacl dacl = (((securityInfo & SECURITY_INFORMATION.DACL_SECURITY_INFORMATION) == 0) ? null : this.Dacl);
			Sacl sacl = (((securityInfo & SECURITY_INFORMATION.SACL_SECURITY_INFORMATION) == 0) ? null : this.Sacl);
			Win32Helpers.SetNamedSecurityInfo(objectName, objectType, securityInfo,
				ownerSid, groupSid, dacl, sacl);
		}	
	}
}
