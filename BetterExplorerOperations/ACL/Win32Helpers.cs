using System;

namespace Microsoft.Win32.Security
{
	using HANDLE = System.IntPtr;
	using DWORD = System.UInt32;

	/// <summary>
	///  Various herlper classes
	/// </summary>
	public sealed class Win32Helpers
	{
		private Win32Helpers()
		{
		}

		public static void GetSecurityInfo(
			HANDLE handle,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo,
			out Sid sidOwner,
			out Sid sidGroup,
			out Dacl dacl,
			out Sacl sacl,
			out SecurityDescriptor secDesc)
		{
			sidOwner = null;
			sidGroup = null;
			dacl = null;
			sacl = null;
			secDesc = null;

			IntPtr ptrOwnerSid = IntPtr.Zero;
			IntPtr ptrGroupSid = IntPtr.Zero;
			IntPtr ptrDacl = IntPtr.Zero;
			IntPtr ptrSacl = IntPtr.Zero;
			IntPtr ptrSecDesc = IntPtr.Zero;

			DWORD rc = Win32.GetSecurityInfo(handle, objectType, securityInfo,
				ref ptrOwnerSid, ref ptrGroupSid, ref ptrDacl, ref ptrSacl, ref ptrSecDesc);

			if (rc != Win32.ERROR_SUCCESS)
			{
				Win32.SetLastError(rc);
				Win32.ThrowLastError();
			}

			try
			{
				if (ptrOwnerSid != IntPtr.Zero)
					sidOwner = new Sid(ptrOwnerSid);

				if (ptrGroupSid != IntPtr.Zero)
					sidGroup = new Sid(ptrGroupSid);

				if (ptrDacl != IntPtr.Zero)
					dacl = new Dacl(ptrDacl);

				if (ptrSacl != IntPtr.Zero)
					sacl = new Sacl(ptrSacl);

				if (ptrSecDesc != IntPtr.Zero)
					secDesc = new SecurityDescriptor(ptrSecDesc, true);
			}
			catch
			{
				if (ptrSecDesc != IntPtr.Zero)
					Win32.LocalFree(ptrSecDesc);
				throw;
			}
		}
		public static void GetNamedSecurityInfo(
			string objectName,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo,
			out Sid sidOwner,
			out Sid sidGroup,
			out Dacl dacl,
			out Sacl sacl,
			out SecurityDescriptor secDesc)
		{
			sidOwner = null;
			sidGroup = null;
			dacl = null;
			sacl = null;
			secDesc = null;

			IntPtr ptrOwnerSid = IntPtr.Zero;
			IntPtr ptrGroupSid = IntPtr.Zero;
			IntPtr ptrDacl = IntPtr.Zero;
			IntPtr ptrSacl = IntPtr.Zero;
			IntPtr ptrSecDesc = IntPtr.Zero;

			DWORD rc = Win32.GetNamedSecurityInfo(objectName, objectType, securityInfo,
				ref ptrOwnerSid, ref ptrGroupSid, ref ptrDacl, ref ptrSacl, ref ptrSecDesc);

			if (rc != Win32.ERROR_SUCCESS)
			{
				Win32.SetLastError(rc);
				Win32.ThrowLastError();
			}

			try
			{
				if (ptrOwnerSid != IntPtr.Zero)
					sidOwner = new Sid(ptrOwnerSid);

				if (ptrGroupSid != IntPtr.Zero)
					sidGroup = new Sid(ptrGroupSid);

				if (ptrDacl != IntPtr.Zero)
					dacl = new Dacl(ptrDacl);

				if (ptrSacl != IntPtr.Zero)
					sacl = new Sacl(ptrSacl);

				if (ptrSecDesc != IntPtr.Zero)
					secDesc = new SecurityDescriptor(ptrSecDesc, true);
			}
			catch
			{
				if (ptrSecDesc != IntPtr.Zero)
					Win32.LocalFree(ptrSecDesc);
				throw;
			}
		}
		public static void SetSecurityInfo(
			HANDLE handle,
			SE_OBJECT_TYPE ObjectType,
			SECURITY_INFORMATION SecurityInfo,
			Sid sidOwner,
			Sid sidGroup,
			Dacl dacl,
			Sacl sacl)
		{
			UnsafeSetSecurityInfo (handle, ObjectType, SecurityInfo,
				sidOwner, sidGroup, dacl, sacl);
		}
		public static void SetNamedSecurityInfo(
			string objectName,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo,
			Sid sidOwner,
			Sid sidGroup,
			Dacl dacl,
			Sacl sacl)
		{
			UnsafeSetNamedSecurityInfo (objectName, objectType, securityInfo,
				sidOwner, sidGroup, dacl, sacl);
		}
		internal unsafe static void UnsafeSetSecurityInfo(
			HANDLE handle,
			SE_OBJECT_TYPE ObjectType,
			SECURITY_INFORMATION SecurityInfo,
			Sid sidOwner,
			Sid sidGroup,
			Dacl dacl,
			Sacl sacl)
		{
			fixed(byte *pSidOwner = (sidOwner != null ? sidOwner.GetNativeSID() : null))
			{
				fixed(byte *pSidGroup = (sidGroup != null ? sidGroup.GetNativeSID() : null))
				{
					fixed(byte *pDacl = (dacl != null ? dacl.GetNativeACL() : null))
					{
						fixed(byte *pSacl = (sacl != null ? sacl.GetNativeACL() : null))
						{
							DWORD rc = Win32.SetSecurityInfo(handle, ObjectType, SecurityInfo,
								(IntPtr)pSidOwner, (IntPtr)pSidGroup, (IntPtr)pDacl, (IntPtr)pSacl);
							if (rc != Win32.ERROR_SUCCESS)
							{
								Win32.SetLastError(rc);
								Win32.ThrowLastError();
							}
						}
					}
				}
			}
		}
		internal unsafe static void UnsafeSetNamedSecurityInfo(
			string objectName,
			SE_OBJECT_TYPE objectType,
			SECURITY_INFORMATION securityInfo,
			Sid sidOwner,
			Sid sidGroup,
			Dacl dacl,
			Sacl sacl)
		{
            byte[] pSidOwner = (sidOwner != null) ? sidOwner.GetNativeSID() : null;
            byte[] pSidGroup = (sidGroup != null) ? sidGroup.GetNativeSID() : null;
            byte[] pDacl = (dacl != null) ? dacl.GetNativeACL() : null;
            byte[] pSacl = (sacl != null) ? sacl.GetNativeACL() : null;

            DWORD rc = Win32.SetNamedSecurityInfo(objectName, objectType, securityInfo,
              pSidOwner, pSidGroup, pDacl, pSacl);

            if (rc != Win32.ERROR_SUCCESS)
            {
                Win32.SetLastError(rc);
                Win32.ThrowLastError();
            }
		}
	}
}
