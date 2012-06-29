using System;

namespace Microsoft.Win32.Security
{
	public class Sids
	{
		public static Sid Null
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNullSidAuthority, Win32Consts.SECURITY_NULL_RID);
			}
		}
		public static Sid World
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityWorldSidAuthority, Win32Consts.SECURITY_WORLD_RID);
			}
		}
		public static Sid Local
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityLocalSidAuthority, Win32Consts.SECURITY_LOCAL_RID);
			}
		}
		public static Sid CreatorOwner
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityCreatorSidAuthority, Win32Consts.SECURITY_CREATOR_OWNER_RID);
			}
		}
		public static Sid CreatorGroup
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityCreatorSidAuthority, Win32Consts.SECURITY_CREATOR_GROUP_RID);
			}
		}
		public static Sid CreatorOwnerServer
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityCreatorSidAuthority, Win32Consts.SECURITY_CREATOR_OWNER_SERVER_RID);
			}
		}
		public static Sid CreatorGroupServer
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityCreatorSidAuthority, Win32Consts.SECURITY_CREATOR_GROUP_SERVER_RID);
			}
		}
		// NT Authority
		public static Sid Dialup
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_DIALUP_RID);
			}
		}
		public static Sid Network
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_NETWORK_RID);
			}
		}
		public static Sid Batch
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BATCH_RID);
			}
		}
		public static Sid Interactive
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_INTERACTIVE_RID);
			}
		}
		public static Sid Service
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_SERVICE_RID);
			}
		}
		public static Sid AnonymousLogon
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_ANONYMOUS_LOGON_RID);
			}
		}
		public static Sid Proxy
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_PROXY_RID);
			}
		}
		public static Sid ServerLogon
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_SERVER_LOGON_RID);
			}
		}
		public static Sid Self
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_PRINCIPAL_SELF_RID);
			}
		}
		public static Sid AuthenticatedUser
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_AUTHENTICATED_USER_RID);
			}
		}
		public static Sid RestrictedCode
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_RESTRICTED_CODE_RID);
			}
		}
		public static Sid TerminalServer
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_TERMINAL_SERVER_RID);
			}
		}
		public static Sid System
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_LOCAL_SYSTEM_RID);
			}
		}

		// NT Authority\BUILTIN
		public static Sid Admins
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_ADMINS);
			}
		}
		public static Sid Users
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_USERS);
			}
		}
		public static Sid Guests
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_GUESTS);
			}
		}
		public static Sid PowerUsers
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_POWER_USERS);
			}
		}
		public static Sid AccountOps
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_ACCOUNT_OPS);
			}
		}
		public static Sid SystemOps
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_SYSTEM_OPS);
			}
		}
		public static Sid PrintOps
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_PRINT_OPS);
			}
		}
		public static Sid BackupOps
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_BACKUP_OPS);
			}
		}
		public static Sid Replicator
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_REPLICATOR);
			}
		}
		public static Sid RasServers
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_RAS_SERVERS);
			}
		}
		public static Sid PreW2KAccess
		{
			get
			{
				return Sid.Create(Win32Consts.SecurityNTAuthority, Win32Consts.SECURITY_BUILTIN_DOMAIN_RID, Win32Consts.DOMAIN_ALIAS_RID_PREW2KCOMPACCESS);
			}
		}
	}
}
