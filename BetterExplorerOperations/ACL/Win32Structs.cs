using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security.Win32Structs
{
	using HANDLE = System.IntPtr;
	using DWORD = System.UInt32;
	using LONG = System.Int32;
	using WORD = System.UInt16;
	using UCHAR = System.Byte;
	using BOOL = System.Int32;
	using BYTE = System.Byte;
	using LARGE_INTEGER = System.Int64;
	using PACL = System.IntPtr;
	using PSID = System.IntPtr;
	using ACCESS_MASK = AccessMask;
	using GUID = System.Guid;
	using PVOID = System.IntPtr;
	using LPWSTR = System.String;

	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_SOURCE
	{
		const int TOKEN_SOURCE_LENGTH = 8;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst=TOKEN_SOURCE_LENGTH)]
		public char[] Name;
		public LUID Indentifier;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct LUID
	{
		public DWORD LowPart;
		public LONG HighPart;
	}	

	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_STATISTICS 
	{
		public LUID TokenId;
		public LUID AuthenticationId;
		public LARGE_INTEGER ExpirationTime;
		public TokenType TokenType;
		public SecurityImpersonationLevel ImpersonationLevel;
		public DWORD DynamicCharged;
		public DWORD DynamicAvailable;
		public DWORD GroupCount;
		public DWORD PrivilegeCount;
		public LUID ModifiedId;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_DEFAULT_DACL 
	{
		public PACL DefaultDacl;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ACL 
	{
        public static readonly int SizeOf = Marshal.SizeOf(typeof(ACL));

        public AclRevision  AclRevision;
		public BYTE  Sbz1;
		public WORD   AclSize;
		public WORD   AceCount;
		public WORD   Sbz2;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ACE
	{
		[FieldOffset(0)]
		public ACE_HEADER Header;
		[FieldOffset(0)]
		public ACCESS_ALLOWED_ACE AccessAllowed;
		[FieldOffset(0)]
		public ACCESS_DENIED_ACE AccessDenied;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ACE_HEADER
	{
		public static readonly int SizeOf = Marshal.SizeOf(typeof(ACE_HEADER));

		public AceType  AceType;
		public AceFlags AceFlags;
		public WORD		AceSize;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct ACCESS_ALLOWED_ACE 
	{
		public static readonly int SizeOf = Marshal.SizeOf(typeof(ACCESS_ALLOWED_ACE));
		public static readonly int SidOffset = Marshal.OffsetOf(typeof(ACCESS_ALLOWED_ACE), "SidStart").ToInt32();

		public ACE_HEADER Header;
		public ACCESS_MASK Mask;
		public DWORD SidStart;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct ACCESS_DENIED_ACE 
	{
		public static readonly int SizeOf = Marshal.SizeOf(typeof(ACCESS_DENIED_ACE));
		public static readonly int SidOffset = Marshal.OffsetOf(typeof(ACCESS_DENIED_ACE), "SidStart").ToInt32();

		public ACE_HEADER Header;
		public ACCESS_MASK Mask;
		public DWORD SidStart;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SYSTEM_AUDIT_ACE 
	{
		public ACE_HEADER Header;
		public ACCESS_MASK Mask;
		public DWORD SidStart;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SYSTEM_ALARM_ACE 
	{
		public ACE_HEADER Header;
		public ACCESS_MASK Mask;
		public DWORD SidStart;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct ACCESS_ALLOWED_OBJECT_ACE 
	{
		public ACE_HEADER Header;
		public ACCESS_MASK Mask;
		public DWORD Flags;
		public GUID ObjectType;
		public GUID InheritedObjectType;
		public DWORD SidStart;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ACCESS_DENIED_OBJECT_ACE 
	{
		public ACE_HEADER Header;
		public ACCESS_MASK Mask;
		public DWORD Flags;
		public GUID ObjectType;
		public GUID InheritedObjectType;
		public DWORD SidStart;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_GROUPS
	{
		public DWORD GroupCount;  
		// Followed by this:
		//SID_AND_ATTRIBUTES Groups[ANYSIZE_ARRAY];
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SID_AND_ATTRIBUTES 
	{
		public PSID Sid;  
		public DWORD Attributes;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_PRIVILEGES 
	{
		public DWORD PrivilegeCount;
		// Followed by this:
		//LUID_AND_ATTRIBUTES Privileges[ANYSIZE_ARRAY];
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct LUID_AND_ATTRIBUTES 
	{
		public LUID Luid;
		public DWORD Attributes;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_OWNER 
	{
		public PSID Owner;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_PRIMARY_GROUP 
	{
		public PSID PrimaryGroup;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct TOKEN_USER 
	{
		public SID_AND_ATTRIBUTES User;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SID_IDENTIFIER_AUTHORITY
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
		public UCHAR []Value;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SECURITY_DESCRIPTOR 
	{
		public static readonly int SizeOf = Marshal.SizeOf(typeof(SECURITY_DESCRIPTOR));

		// We do NOT expose those fields, since the Win32 doc says
		// use "SECURITY_DESCRIPTOR" as a black box.
		private BYTE  Revision;
		private BYTE  Sbz1;
		private SecurityDescriptorControlFlags Control;
		private PSID Owner;
		private PSID Group;
		private PACL Sacl;
		private PACL Dacl;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SECURITY_ATTRIBUTES
	{
		public static readonly int SizeOf = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));

		public DWORD nLength;  
		public PVOID lpSecurityDescriptor;  
		public BOOL bInheritHandle;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PROFILEINFO
	{
		public static readonly int SizeOf = Marshal.SizeOf(typeof(PROFILEINFO));

		public DWORD       dwSize;                 // Set to sizeof(PROFILEINFO) before calling
		public DWORD       dwFlags;                // See PI_ flags defined in userenv.h
		public LPWSTR      lpUserName;             // User name (required)
		public LPWSTR      lpProfilePath;          // Roaming profile path (optional, can be NULL)
		public LPWSTR      lpDefaultPath;          // Default user profile path (optional, can be NULL)
		public LPWSTR      lpServerName;           // Validating domain controller name in netbios format (optional, can be NULL but group NT4 style policy won't be applied)
		public LPWSTR      lpPolicyPath;           // Path to the NT4 style policy file (optional, can be NULL)
		public HANDLE      hProfile;               // Filled in by the function.  Registry key handle open to the root.
	}


    [StructLayout(LayoutKind.Sequential)]
    public struct TRUSTEE
    {
        IntPtr                      pMultipleTrustee;
        MULTIPLE_TRUSTEE_OPERATION  MultipleTrusteeOperation;
        TRUSTEE_FORM                TrusteeForm;
        TRUSTEE_TYPE                TrusteeType;
#if false
        [switch_is(TrusteeForm)]
        union
        {
            [case(TRUSTEE_IS_NAME)]
            LPWSTR                  ptstrName;
            [case(TRUSTEE_IS_SID)]
            SID                    *pSid;
            [case(TRUSTEE_IS_OBJECTS_AND_SID)]
            OBJECTS_AND_SID        *pObjectsAndSid;
            [case(TRUSTEE_IS_OBJECTS_AND_NAME)]
            OBJECTS_AND_NAME_W     *pObjectsAndName;
        };
#else
        IntPtr                      ptstrName;
#endif
    }
}
