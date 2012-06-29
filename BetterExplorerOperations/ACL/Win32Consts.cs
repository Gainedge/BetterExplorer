using System;
using System.Diagnostics;

namespace Microsoft.Win32.Security
{
	using Win32Structs;

	/// <summary>
	/// Summary description for Win32public consts.
	/// </summary>
	public class Win32Consts
	{
		private static SID_IDENTIFIER_AUTHORITY CreateAuthority(byte[] auth)
		{
			Debug.Assert(auth.Length == 6);
			SID_IDENTIFIER_AUTHORITY res = new SID_IDENTIFIER_AUTHORITY();
			res.Value = auth;
			return res;
		}
		// Authorities
		public static byte[] SECURITY_NULL_SID_AUTHORITY			= {0,0,0,0,0,0};
		public static byte[] SECURITY_WORLD_SID_AUTHORITY			= {0,0,0,0,0,1};
		public static byte[] SECURITY_LOCAL_SID_AUTHORITY			= {0,0,0,0,0,2};
		public static byte[] SECURITY_CREATOR_SID_AUTHORITY			= {0,0,0,0,0,3};
		public static byte[] SECURITY_NON_UNIQUE_AUTHORITY			= {0,0,0,0,0,4};
		public static byte[] SECURITY_RESOURCE_MANAGER_AUTHORITY	= {0,0,0,0,0,9};
		public static byte[] SECURITY_NT_AUTHORITY					= {0,0,0,0,0,5};

		public static SID_IDENTIFIER_AUTHORITY SecurityNullSidAuthority				= CreateAuthority(SECURITY_NULL_SID_AUTHORITY);
		public static SID_IDENTIFIER_AUTHORITY SecurityWorldSidAuthority			= CreateAuthority(SECURITY_WORLD_SID_AUTHORITY);
		public static SID_IDENTIFIER_AUTHORITY SecurityLocalSidAuthority			= CreateAuthority(SECURITY_LOCAL_SID_AUTHORITY);
		public static SID_IDENTIFIER_AUTHORITY SecurityCreatorSidAuthority			= CreateAuthority(SECURITY_CREATOR_SID_AUTHORITY);
		public static SID_IDENTIFIER_AUTHORITY SecurityNonUniqueAuthority			= CreateAuthority(SECURITY_NON_UNIQUE_AUTHORITY);
		public static SID_IDENTIFIER_AUTHORITY SecurityResourceManagerAuthority		= CreateAuthority(SECURITY_RESOURCE_MANAGER_AUTHORITY);
		public static SID_IDENTIFIER_AUTHORITY SecurityNTAuthority					= CreateAuthority(SECURITY_NT_AUTHORITY);

		public const UInt32 SECURITY_NULL_RID                 =0x00000000;
		public const UInt32 SECURITY_WORLD_RID                =0x00000000;
		public const UInt32 SECURITY_LOCAL_RID                =0x00000000;

		public const UInt32 SECURITY_CREATOR_OWNER_RID        =0x00000000;
		public const UInt32 SECURITY_CREATOR_GROUP_RID        =0x00000001;

		public const UInt32 SECURITY_CREATOR_OWNER_SERVER_RID =0x00000002;
		public const UInt32 SECURITY_CREATOR_GROUP_SERVER_RID =0x00000003;
		public const UInt32 SECURITY_DIALUP_RID             =0x00000001;
		public const UInt32 SECURITY_NETWORK_RID            =0x00000002;
		public const UInt32 SECURITY_BATCH_RID              =0x00000003;
		public const UInt32 SECURITY_INTERACTIVE_RID        =0x00000004;
		public const UInt32 SECURITY_SERVICE_RID            =0x00000006;
		public const UInt32 SECURITY_ANONYMOUS_LOGON_RID    =0x00000007;
		public const UInt32 SECURITY_PROXY_RID              =0x00000008;
		public const UInt32 SECURITY_ENTERPRISE_CONTROLLERS_RID =0x00000009;
		public const UInt32 SECURITY_SERVER_LOGON_RID       = SECURITY_ENTERPRISE_CONTROLLERS_RID;
		public const UInt32 SECURITY_PRINCIPAL_SELF_RID     =0x0000000A;
		public const UInt32 SECURITY_AUTHENTICATED_USER_RID =0x0000000B;
		public const UInt32 SECURITY_RESTRICTED_CODE_RID    =0x0000000C;
		public const UInt32 SECURITY_TERMINAL_SERVER_RID    =0x0000000D;
		public const UInt32 SECURITY_REMOTE_LOGON_RID       =0x0000000E;


		public const UInt32 SECURITY_LOGON_IDS_RID          =0x00000005;
		public const UInt32 SECURITY_LOGON_IDS_RID_COUNT    =3;

		public const UInt32 SECURITY_LOCAL_SYSTEM_RID       =0x00000012;
		public const UInt32 SECURITY_LOCAL_SERVICE_RID      =0x00000013;
		public const UInt32 SECURITY_NETWORK_SERVICE_RID    =0x00000014;

		public const UInt32 SECURITY_NT_NON_UNIQUE                 =0x00000015;
		public const UInt32 SECURITY_NT_NON_UNIQUE_SUB_AUTH_COUNT  =3;

		public const UInt32 SECURITY_BUILTIN_DOMAIN_RID     =0x00000020;
		// Well-known users ...

		public const UInt32 DOMAIN_USER_RID_ADMIN          =0x000001F4;
		public const UInt32 DOMAIN_USER_RID_GUEST          =0x000001F5;
		public const UInt32 DOMAIN_USER_RID_KRBTGT         =0x000001F6;



		// well-known groups ...

		public const UInt32 DOMAIN_GROUP_RID_ADMINS        =0x00000200;
		public const UInt32 DOMAIN_GROUP_RID_USERS         =0x00000201;
		public const UInt32 DOMAIN_GROUP_RID_GUESTS        =0x00000202;
		public const UInt32 DOMAIN_GROUP_RID_COMPUTERS     =0x00000203;
		public const UInt32 DOMAIN_GROUP_RID_CONTROLLERS   =0x00000204;
		public const UInt32 DOMAIN_GROUP_RID_CERT_ADMINS   =0x00000205;
		public const UInt32 DOMAIN_GROUP_RID_SCHEMA_ADMINS =0x00000206;
		public const UInt32 DOMAIN_GROUP_RID_ENTERPRISE_ADMINS =0x00000207;
		public const UInt32 DOMAIN_GROUP_RID_POLICY_ADMINS =0x00000208;




		// well-known aliases ...

		public const UInt32 DOMAIN_ALIAS_RID_ADMINS        =0x00000220;
		public const UInt32 DOMAIN_ALIAS_RID_USERS         =0x00000221;
		public const UInt32 DOMAIN_ALIAS_RID_GUESTS        =0x00000222;
		public const UInt32 DOMAIN_ALIAS_RID_POWER_USERS   =0x00000223;

		public const UInt32 DOMAIN_ALIAS_RID_ACCOUNT_OPS   =0x00000224;
		public const UInt32 DOMAIN_ALIAS_RID_SYSTEM_OPS    =0x00000225;
		public const UInt32 DOMAIN_ALIAS_RID_PRINT_OPS     =0x00000226;
		public const UInt32 DOMAIN_ALIAS_RID_BACKUP_OPS    =0x00000227;

		public const UInt32 DOMAIN_ALIAS_RID_REPLICATOR    =0x00000228;
		public const UInt32 DOMAIN_ALIAS_RID_RAS_SERVERS   =0x00000229;
		public const UInt32 DOMAIN_ALIAS_RID_PREW2KCOMPACCESS =0x0000022A;
		public const UInt32 DOMAIN_ALIAS_RID_REMOTE_DESKTOP_USERS =0x0000022B;
		public const UInt32 DOMAIN_ALIAS_RID_NETWORK_CONFIGURATION_OPS =0x0000022C;


        //TODO: Check this is 64-bit compatible...
        public static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr((long)unchecked((int)0x80000000));
        public static readonly IntPtr HKEY_CURRENT_USER = new IntPtr((long)unchecked((int)0x80000001));
        public static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr((long)unchecked((int)0x80000002));
        public static readonly IntPtr HKEY_USERS = new IntPtr((long)unchecked((int)0x80000003));
    }
}
