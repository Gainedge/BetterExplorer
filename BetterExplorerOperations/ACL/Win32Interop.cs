using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;

	using HANDLE = System.IntPtr;
	using DWORD = System.UInt32;
	using BOOL = System.Int32;
	using LPVOID = System.IntPtr;
	using PSID = System.IntPtr;
	using PACL = System.IntPtr;
	using HLOCAL = System.IntPtr;
	using PDWORD = System.IntPtr;
	using PUCHAR = System.IntPtr;
	using UCHAR = System.Byte;
	using PSID_IDENTIFIER_AUTHORITY = System.IntPtr;
	using PTOKEN_PRIVILEGES = System.IntPtr;
	using PSECURITY_DESCRIPTOR = System.IntPtr;
	using SECURITY_DESCRIPTOR_CONTROL = SecurityDescriptorControlFlags;
	using LPCTSTR = System.String;
    using HKEY = System.IntPtr;
    using LONG = System.Int32;

	/// <summary>
	/// Summary description for Win32Interop.
	/// </summary>
	public class Win32
	{
		public const BOOL FALSE = 0;
		public const BOOL TRUE = 1;

		public const int SUCCESS = 0;
		public const int ERROR_SUCCESS = 0;
		public const int ERROR_ACCESS_DENIED = 5;
		public const int ERROR_BAD_LENGTH = 24;
		public const int ERROR_INSUFFICIENT_BUFFER = 122;
		public const int ERROR_NO_TOKEN = 1008;
		public const int ERROR_NOT_ALL_ASSIGNED = 1300;
		public const int ERROR_NONE_MAPPED = 1332;

		public static bool ToBool(BOOL bValue)
		{
			return (bValue != Win32.FALSE);
		}
		public static BOOL FromBool(bool bValue)
		{
			return (bValue ? Win32.TRUE : Win32.FALSE);
		}
		public static IntPtr AllocGlobal(uint cbSize)
		{
			return AllocGlobal((int)cbSize);
		}
		public static IntPtr AllocGlobal(int cbSize)
		{
			return Marshal.AllocHGlobal(cbSize);
		}
		public static void FreeGlobal(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				return;
			Marshal.FreeHGlobal(ptr);
		}
		public static DWORD GetLastError()
		{
			return (DWORD)Marshal.GetLastWin32Error();
		}
		public static void ThrowLastError()
		{
			Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		}
		public static void CheckCall(bool funcResult)
		{
			if (! funcResult)
			{
				ThrowLastError();
			}
		}
		public static void CheckCall(BOOL funcResult)
		{
			CheckCall(funcResult != 0);
		}
		public static void CheckCall(HANDLE funcResult)
		{
			CheckCall(!IsNullHandle(funcResult));
		}
		public static bool IsNullHandle(HANDLE ptr)
		{
			return (ptr == IntPtr.Zero);
		}

		const string Kernel32 = "kernel32.dll";
		const string Advapi32 = "Advapi32.dll";
		const string Userenv = "Userenv.dll";

		///////////////////////////////////////////////////////////////////////////////
		///
		/// KERNEL32.DLL
		///
		[DllImport(Kernel32, CallingConvention=CallingConvention.Winapi)]
		public static extern void SetLastError(DWORD dwErrCode);


		/*
		[DllImport(Kernel32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern HANDLE CreateEvent(
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(SecurityAttributesMarshaler))]
			SecurityAttributes lpEventAttributes, // SA
			BOOL bManualReset,                    // reset type
			BOOL bInitialState,                   // initial state
			string lpName                         // object name
			);
		*/

		[DllImport(Kernel32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern HANDLE OpenProcess(ProcessAccessType dwDesiredAccess, BOOL bInheritHandle, DWORD dwProcessId);
		[DllImport(Kernel32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern HANDLE OpenThread(ThreadAccessType dwDesiredAccess, BOOL bInheritHandle, DWORD dwThreadId);

		[DllImport(Kernel32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL CloseHandle(HANDLE handle);

		[DllImport(Kernel32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern HLOCAL LocalFree(HLOCAL hMem);

		///////////////////////////////////////////////////////////////////////////////
		///
		/// ADVAPI32.DLL
		///
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL OpenProcessToken(HANDLE hProcess, TokenAccessType dwDesiredAccess, out HANDLE hToken);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL OpenThreadToken(HANDLE ThreadHandle, DWORD DesiredAccess, BOOL OpenAsSelf, out HANDLE TokenHandle);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL IsTokenRestricted(HANDLE TokenHandle);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL GetTokenInformation(
			HANDLE TokenHandle, 
			TokenInformationClass TokenInformationClass, 
			LPVOID TokenInformation, 
			DWORD TokenInformationLength, 
			out DWORD ReturnLength);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi)]
		public static extern DWORD GetLengthSid(PSID pSid);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi)]
		public static extern DWORD GetSidLengthRequired(UCHAR nSubAuthorityCount);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL IsValidSid(PSID pSid);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern PDWORD GetSidSubAuthority(PSID pSid, DWORD nSubAuthority);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern PUCHAR GetSidSubAuthorityCount(PSID pSid);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern PSID_IDENTIFIER_AUTHORITY GetSidIdentifierAuthority(PSID pSid);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL EqualSid(PSID pSid1, PSID pSid2);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL EqualPrefixSid(PSID pSid1, PSID pSid2);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL InitializeSid(PSID Sid, [In]ref SID_IDENTIFIER_AUTHORITY pIdentifierAuthority, UCHAR nSubAuthorityCount);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL InitializeAcl(PACL pAcl, DWORD nAclLength, DWORD dwAclRevision);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true)]
		public static extern BOOL AddAce(PACL pAcl, DWORD dwAceRevision, DWORD dwStartingAceIndex, LPVOID pAceList, DWORD nAceListLength);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL LookupAccountSid(
			string lpSystemName, 
			PSID Sid,
			[Out] char[] Name,
			ref DWORD cchName,
			[Out] char [] ReferencedDomainName,
			ref DWORD cchReferencedDomainName,
			out SID_NAME_USE peUse);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL LookupAccountName(
			string lpSystemName, 
			string lpAccountName, 
			PSID Sid, 
			ref DWORD cbSid, 
			[Out] char[] DomainName, 
			ref DWORD cbDomainName, 
			out SID_NAME_USE peUse
			);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL ConvertSidToStringSid(PSID Sid, out IntPtr StringSid);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL LookupPrivilegeName(
			string lpSystemName, 
			[In]ref LUID lpLuid, 
			[Out] char[] lpName, 
			ref DWORD cbName);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL LookupPrivilegeValue(string lpSystemName, string lpName, out LUID Luid);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL AdjustTokenPrivileges(
			HANDLE TokenHandle, 
			BOOL DisableAllPrivileges, 
			PTOKEN_PRIVILEGES NewState, 
			DWORD BufferLength, 
			PTOKEN_PRIVILEGES PreviousState, 
			out DWORD ReturnLength);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL InitializeSecurityDescriptor(PSECURITY_DESCRIPTOR pSecurityDescriptor, DWORD dwRevision);
		
        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL GetSecurityDescriptorControl(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			out SECURITY_DESCRIPTOR_CONTROL pControl, 
			out DWORD lpdwRevision
			);

        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL GetSecurityDescriptorGroup(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			out PSID pGroup, 
			out BOOL lpbGroupDefaulted
			);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, CharSet=CharSet.Auto)]
		public static extern DWORD GetSecurityDescriptorLength(
			PSECURITY_DESCRIPTOR pSecurityDescriptor
			);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL GetSecurityDescriptorOwner(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			out PSID pOwner, 
			out BOOL lpbOwnerDefaulted
			);
		
        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern BOOL GetSecurityDescriptorDacl(
            PSECURITY_DESCRIPTOR pSecurityDescriptor, 
            out BOOL lpbDaclPresent, 
            ref PACL pDacl,     // By ref, because if "present" == false, value is unchanged
            out BOOL lpbDaclDefaulted
            );

        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL GetSecurityDescriptorSacl(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			out BOOL lpbSaclPresent, 
			ref PACL pSacl,     // By ref, because if "present" == false, value is unchanged
			out BOOL lpbSaclDefaulted
			);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL MakeAbsoluteSD(
			PSECURITY_DESCRIPTOR pSelfRelativeSD, 
			PSECURITY_DESCRIPTOR pAbsoluteSD, 
			ref DWORD lpdwAbsoluteSDSize, 
			PACL pDacl, 
			ref DWORD lpdwDaclSize, 
			PACL pSacl, 
			ref DWORD lpdwSaclSize, 
			PSID pOwner, 
			ref DWORD lpdwOwnerSize, 
			PSID pPrimaryGroup, 
			ref DWORD lpdwPrimaryGroupSize
			);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL MakeSelfRelativeSD(
			PSECURITY_DESCRIPTOR pAbsoluteSD, 
			PSECURITY_DESCRIPTOR pSelfRelativeSD, 
			ref DWORD lpdwBufferLength
			);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL CopySid(
			DWORD nDestinationSidLength, 
			PSID pDestinationSid, 
			PSID pSourceSid
			);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL SetSecurityDescriptorControl(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			SECURITY_DESCRIPTOR_CONTROL ControlBitsOfInterest, 
			SECURITY_DESCRIPTOR_CONTROL ControlBitsToSet
			);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL SetSecurityDescriptorDacl(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			BOOL bDaclPresent, 
			PACL pDacl, 
			BOOL bDaclDefaulted
			);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL SetSecurityDescriptorGroup(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			PSID pGroup, 
			BOOL bGroupDefaulted
			);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL SetSecurityDescriptorOwner(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			PSID pOwner, 
			BOOL bOwnerDefaulted
			);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL SetSecurityDescriptorSacl(
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			BOOL bSaclPresent, 
			PACL pSacl, 
			BOOL bSaclDefaulted
			);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=false, CharSet=CharSet.Auto)]
		public static extern DWORD GetSecurityInfo(
			HANDLE handle,
			SE_OBJECT_TYPE ObjectType,
			SECURITY_INFORMATION SecurityInfo,
			ref PSID ppsidOwner,
			ref PSID ppsidGroup,
			ref PACL ppDacl,
			ref PACL ppSacl,
			ref PSECURITY_DESCRIPTOR ppSecurityDescriptor);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=false, CharSet=CharSet.Auto)]
		public static extern DWORD SetSecurityInfo(
			HANDLE handle,
			SE_OBJECT_TYPE ObjectType,
			SECURITY_INFORMATION SecurityInfo,
			PSID psidOwner,
			PSID psidGroup,
			PACL pDacl,
			PACL pSacl);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=false, CharSet=CharSet.Auto)]
		public static extern DWORD GetNamedSecurityInfo(
			LPCTSTR pObjectName,		//REVIEW: Why is it documented as LPTSTR
			SE_OBJECT_TYPE ObjectType,
			SECURITY_INFORMATION SecurityInfo,
			ref PSID ppsidOwner,
			ref PSID ppsidGroup,
			ref PACL ppDacl,
			ref PACL ppSacl,
			ref PSECURITY_DESCRIPTOR ppSecurityDescriptor);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=false, CharSet=CharSet.Auto)]
		public static extern DWORD SetNamedSecurityInfo(
			LPCTSTR pObjectName,		//REVIEW: Why is it documented as LPTSTR
			SE_OBJECT_TYPE ObjectType,
			SECURITY_INFORMATION SecurityInfo,
		    [In,MarshalAs(UnmanagedType.LPArray)] byte[] psidOwner,
            [In,MarshalAs(UnmanagedType.LPArray)] byte[] psidGroup,
            [In,MarshalAs(UnmanagedType.LPArray)] byte[] pDacl,
            [In,MarshalAs(UnmanagedType.LPArray)] byte[] pSacl);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL GetFileSecurity(
			LPCTSTR lpFileName, 
			SECURITY_INFORMATION RequestedInformation, 
			PSECURITY_DESCRIPTOR pSecurityDescriptor, 
			DWORD nLength, 
			out DWORD lpnLengthNeeded
			);

        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern BOOL SetFileSecurity(
            LPCTSTR lpFileName, // file name
            SECURITY_INFORMATION SecurityInformation, // contents
            PSECURITY_DESCRIPTOR pSecurityDescriptor // SD
            );

        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern LONG RegGetKeySecurity(
            HKEY hKey,                                // handle to key
            SECURITY_INFORMATION SecurityInformation, // request
            PSECURITY_DESCRIPTOR pSecurityDescriptor, // SD
            ref DWORD lpcbSecurityDescriptor            // buffer size
            );

        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern LONG RegSetKeySecurity(
            HKEY hKey,
            SECURITY_INFORMATION SecurityInformation,
            PSECURITY_DESCRIPTOR pSecurityDescriptor
            );

        [DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
        public static extern LONG RegOpenKey(
            HKEY hKey,
            LPCTSTR lpSubKey,
            out HKEY phkResult
            );

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern LONG RegCloseKey(HKEY hKey);

		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL GetKernelObjectSecurity(
			HANDLE Handle,                             // handle to object
			SECURITY_INFORMATION RequestedInformation, // request
			PSECURITY_DESCRIPTOR pSecurityDescriptor,  // SD
			DWORD nLength,                             // size of SD
			out DWORD lpnLengthNeeded                    // required size of buffer
			);
		[DllImport(Advapi32, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL SetKernelObjectSecurity(
			HANDLE Handle,
			SECURITY_INFORMATION SecurityInformation,
			PSECURITY_DESCRIPTOR SecurityDescriptor
			);

		///////////////////////////////////////////////////////////////////////////////
		///
		/// USERENV.DLL (NT4+ only) 
		///
		[DllImport(Userenv, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL UnloadUserProfile(
			HANDLE hToken,   // user token
			HANDLE hProfile  // handle to registry key
			);

		[DllImport(Userenv, CallingConvention=CallingConvention.Winapi, SetLastError=true, CharSet=CharSet.Auto)]
		public static extern BOOL LoadUserProfile(
			HANDLE hToken,               // user token
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=4)]
			ref PROFILEINFO lpProfileInfo  // profile
			);
	}
}
