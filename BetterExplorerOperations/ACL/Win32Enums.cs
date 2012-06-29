using System;

namespace Microsoft.Win32.Security
{
    [Flags]
    public enum AccessMask : uint
    {
        COM_RIGHTS_EXECUTE				= 0x00000001,
        COM_RIGHTS_SAFE_FOR_SCRIPTING   = 0x00000002,

        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,
    }
    [Flags]
    public enum AccessType : uint
    {
        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,
    }

    [Flags]
    public enum TokenAccessType : uint
    {
        TOKEN_ASSIGN_PRIMARY    = 0x0001,
        TOKEN_DUPLICATE         = 0x0002,
        TOKEN_IMPERSONATE       = 0x0004,
        TOKEN_QUERY             = 0x0008,
        TOKEN_QUERY_SOURCE      = 0x0010,
        TOKEN_ADJUST_PRIVILEGES = 0x0020,
        TOKEN_ADJUST_GROUPS     = 0x0040,
        TOKEN_ADJUST_DEFAULT    = 0x0080,
        TOKEN_ADJUST_SESSIONID  = 0x0100,

        TOKEN_ALL_ACCESS        = 
            AccessType.STANDARD_RIGHTS_REQUIRED | 
            TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE |
            TOKEN_IMPERSONATE |
            TOKEN_QUERY |
            TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ADJUST_GROUPS |
            TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID,

        TOKEN_READ	= 
            AccessType.STANDARD_RIGHTS_READ      |
            TOKEN_QUERY,

        TOKEN_WRITE =
            AccessType.STANDARD_RIGHTS_WRITE     |
            TOKEN_ADJUST_PRIVILEGES   |
            TOKEN_ADJUST_GROUPS       |
            TOKEN_ADJUST_DEFAULT,

        TOKEN_EXECUTE = 
            AccessType.STANDARD_RIGHTS_EXECUTE,
    }

    [Flags]
    public enum ThreadAccessType : uint
    {
        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,

        // Thread specific
        THREAD_TERMINATE               = 0x0001,  
        THREAD_SUSPEND_RESUME          = 0x0002,  
        THREAD_GET_CONTEXT             = 0x0008,  
        THREAD_SET_CONTEXT             = 0x0010,  
        THREAD_SET_INFORMATION         = 0x0020,  
        THREAD_QUERY_INFORMATION       = 0x0040,  
        THREAD_SET_THREAD_TOKEN        = 0x0080,
        THREAD_IMPERSONATE             = 0x0100,
        THREAD_DIRECT_IMPERSONATION    = 0x0200,
        // begin_ntddk begin_wdm begin_ntifs

        THREAD_ALL_ACCESS         = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x3FF,

    }
    [Flags]
    public enum ProcessAccessType : uint
    {
        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,

        // PROCESS specific
        PROCESS_TERMINATE         =0x0001,
        PROCESS_CREATE_THREAD     =0x0002,  
        PROCESS_SET_SESSIONID     =0x0004, 
        PROCESS_VM_OPERATION      =0x0008,  
        PROCESS_VM_READ           =0x0010,  
        PROCESS_VM_WRITE          =0x0020,  
        PROCESS_DUP_HANDLE        =0x0040,  
        PROCESS_CREATE_PROCESS    =0x0080,  
        PROCESS_SET_QUOTA         =0x0100, 
        PROCESS_SET_INFORMATION   =0x0200,  
        PROCESS_QUERY_INFORMATION =0x0400,  
        PROCESS_SUSPEND_RESUME    =0x0800,  
        PROCESS_ALL_ACCESS        =
            AccessType.STANDARD_RIGHTS_REQUIRED | 
            AccessType.SYNCHRONIZE |
            0xFFF,
    }
    [Flags]
    public enum EventAccessType : uint
    {
        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,

        //EVENT specific
		EVENT_QUERY_STATE				 = 0x0001,
		EVENT_MODIFY_STATE				 = 0x0002,
        EVENT_ALL_ACCESS				 = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE |0x0003,
	}

    [Flags]
    public enum FileAccessType : uint
    {
        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,

        FILE_READ_DATA                   = 0x0001,    // file & pipe
        FILE_WRITE_DATA                  = 0x0002,    // file & pipe
        FILE_APPEND_DATA                 = 0x0004,    // file
        FILE_READ_EA                     = 0x0008,    // file & directory
        FILE_WRITE_EA                    = 0x0010,    // file & directory
        FILE_EXECUTE                     = 0x0020,    // file
        FILE_READ_ATTRIBUTES             = 0x0080,    // all
        FILE_WRITE_ATTRIBUTES            = 0x0100,    // all

        FILE_ALL_ACCESS             =
            STANDARD_RIGHTS_REQUIRED | 
            SYNCHRONIZE 
            | 0x1FF,
        

        FILE_GENERIC_READ          = 
            STANDARD_RIGHTS_READ     |
            FILE_READ_DATA           |
            FILE_READ_ATTRIBUTES     |
            FILE_READ_EA             |
            SYNCHRONIZE,


        FILE_GENERIC_WRITE         = 
            STANDARD_RIGHTS_WRITE    |
            FILE_WRITE_DATA          |
            FILE_WRITE_ATTRIBUTES    |
            FILE_WRITE_EA            |
            FILE_APPEND_DATA         |
            SYNCHRONIZE,

        FILE_GENERIC_EXECUTE      = 
            STANDARD_RIGHTS_EXECUTE  |
            FILE_READ_ATTRIBUTES     |
            FILE_EXECUTE             |
            SYNCHRONIZE,
    }

    [Flags]
    public enum DirectoryAccessType : uint
    {
        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,

        FILE_LIST_DIRECTORY              = 0x0001,    // directory
        FILE_ADD_FILE                    = 0x0002,    // directory
        FILE_ADD_SUBDIRECTORY            = 0x0004,    // directory
        FILE_READ_EA                     = 0x0008,    // file & directory
        FILE_WRITE_EA                    = 0x0010,    // file & directory
        FILE_TRAVERSE                    = 0x0020,    // directory
        FILE_DELETE_CHILD                = 0x0040,    // directory
        FILE_READ_ATTRIBUTES             = 0x0080,    // all
        FILE_WRITE_ATTRIBUTES            = 0x0100,    // all

        FILE_ALL_ACCESS             =
            STANDARD_RIGHTS_REQUIRED | 
            SYNCHRONIZE | 
            0x1FF,

        FILE_GENERIC_READ           = 
            STANDARD_RIGHTS_READ     |
            FILE_LIST_DIRECTORY      |
            FILE_READ_ATTRIBUTES     |
            FILE_READ_EA             |
            SYNCHRONIZE,


        FILE_GENERIC_WRITE          =
            STANDARD_RIGHTS_WRITE    |
            FILE_ADD_FILE            |
            FILE_WRITE_ATTRIBUTES    |
            FILE_WRITE_EA            |
            FILE_ADD_SUBDIRECTORY    |
            SYNCHRONIZE,


        FILE_GENERIC_EXECUTE        = 
            STANDARD_RIGHTS_EXECUTE  |
            FILE_READ_ATTRIBUTES     |
            FILE_TRAVERSE            |
            SYNCHRONIZE,
    }

    [Flags]
    public enum PipeAccessType : uint
    {
        DELETE                          = 0x00010000,
        READ_CONTROL                    = 0x00020000,
        WRITE_DAC                       = 0x00040000,
        WRITE_OWNER                     = 0x00080000,
        SYNCHRONIZE                     = 0x00100000,

        STANDARD_RIGHTS_REQUIRED        = 0x000F0000,

        STANDARD_RIGHTS_READ            = READ_CONTROL,
        STANDARD_RIGHTS_WRITE           = READ_CONTROL,
        STANDARD_RIGHTS_EXECUTE         = READ_CONTROL,

        STANDARD_RIGHTS_ALL             = 0x001F0000,

        SPECIFIC_RIGHTS_ALL             = 0x0000FFFF,

        //
        // AccessSystemAcl access type
        //

        ACCESS_SYSTEM_SECURITY           = 0x01000000,

        //
        // MaximumAllowed access type
        //

        MAXIMUM_ALLOWED                  = 0x02000000,

        //
        //  These are the generic rights.
        //

        GENERIC_READ                     = 0x80000000,
        GENERIC_WRITE                    = 0x40000000,
        GENERIC_EXECUTE                  = 0x20000000,
        GENERIC_ALL                      = 0x10000000,

        FILE_READ_DATA                   = 0x0001,    // file & pipe
        FILE_WRITE_DATA                  = 0x0002,    // file & pipe
        FILE_CREATE_PIPE_INSTANCE        = 0x0004,    // named pipe
        FILE_READ_ATTRIBUTES             = 0x0080,    // all
        FILE_WRITE_ATTRIBUTES            = 0x0100,    // all

        FILE_ALL_ACCESS         =
            STANDARD_RIGHTS_REQUIRED | 
            SYNCHRONIZE | 
            0x1FF,

        FILE_GENERIC_READ       =
            STANDARD_RIGHTS_READ     |
            FILE_READ_DATA           |
            FILE_READ_ATTRIBUTES     |
            SYNCHRONIZE,


        FILE_GENERIC_WRITE     =
            STANDARD_RIGHTS_WRITE    |
            FILE_WRITE_DATA          |
            FILE_WRITE_ATTRIBUTES    |
            SYNCHRONIZE,


        FILE_GENERIC_EXECUTE   = 
            STANDARD_RIGHTS_EXECUTE  |
            FILE_READ_ATTRIBUTES     |
            SYNCHRONIZE,
    }

    public enum SecurityImpersonationLevel : uint
    {
        SecurityAnonymous,
        SecurityIdentification,
        SecurityImpersonation,
        SecurityDelegation
    }

    public enum TokenType : uint
    {
        TokenPrimary = 1,
        TokenImpersonation
    }

    public enum TokenInformationClass : uint
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert
    }

    public enum AceType : byte
    {
        ACCESS_ALLOWED_ACE_TYPE                 = 0x0,
        ACCESS_DENIED_ACE_TYPE                  = 0x1,
        SYSTEM_AUDIT_ACE_TYPE                   = 0x2,
        SYSTEM_ALARM_ACE_TYPE                   = 0x3,
        //ACCESS_MAX_MS_V2_ACE_TYPE               = 0x3,

        ACCESS_ALLOWED_COMPOUND_ACE_TYPE        = 0x4,
        //ACCESS_MAX_MS_V3_ACE_TYPE               = 0x4,

        //ACCESS_MIN_MS_OBJECT_ACE_TYPE           = 0x5,
        ACCESS_ALLOWED_OBJECT_ACE_TYPE          = 0x5,
        ACCESS_DENIED_OBJECT_ACE_TYPE           = 0x6,
        SYSTEM_AUDIT_OBJECT_ACE_TYPE            = 0x7,
        SYSTEM_ALARM_OBJECT_ACE_TYPE            = 0x8,
        //ACCESS_MAX_MS_OBJECT_ACE_TYPE           = 0x8,

        //ACCESS_MAX_MS_V4_ACE_TYPE               = 0x8,
        //ACCESS_MAX_MS_ACE_TYPE                  = 0x8,

        ACCESS_ALLOWED_CALLBACK_ACE_TYPE        = 0x9,
        ACCESS_DENIED_CALLBACK_ACE_TYPE         = 0xA,
        ACCESS_ALLOWED_CALLBACK_OBJECT_ACE_TYPE = 0xB,
        ACCESS_DENIED_CALLBACK_OBJECT_ACE_TYPE  = 0xC,
        SYSTEM_AUDIT_CALLBACK_ACE_TYPE          = 0xD,
        SYSTEM_ALARM_CALLBACK_ACE_TYPE          = 0xE,
        SYSTEM_AUDIT_CALLBACK_OBJECT_ACE_TYPE   = 0xF,
        SYSTEM_ALARM_CALLBACK_OBJECT_ACE_TYPE   = 0x10,

        //ACCESS_MAX_MS_V5_ACE_TYPE               = 0x10,
    }

    [Flags]
    public enum AceFlags : byte
    {
        OBJECT_INHERIT_ACE                = 0x1,
        CONTAINER_INHERIT_ACE             = 0x2,
        NO_PROPAGATE_INHERIT_ACE          = 0x4,
        INHERIT_ONLY_ACE                  = 0x8,
        INHERITED_ACE                     = 0x10,
        VALID_INHERIT_FLAGS               = 0x1F,
        SUCCESSFUL_ACCESS_ACE_FLAG        = 0x40,
        FAILED_ACCESS_ACE_FLAG            = 0x80,
    }

    public enum SID_NAME_USE : uint
    {
        SidTypeUser = 1,
        SidTypeGroup,
        SidTypeDomain,
        SidTypeAlias,
        SidTypeWellKnownGroup,
        SidTypeDeletedAccount,
        SidTypeInvalid,
        SidTypeUnknown,
        SidTypeComputer
    }

    [Flags]
    public enum PrivilegeAttributes : uint
    {
        /*
        SE_PRIVILEGE_DISABLED            = 0,
        SE_PRIVILEGE_ENABLED_BY_DEFAULT  = 0x00000001,
        SE_PRIVILEGE_ENABLED             = 0x00000002,
        SE_PRIVILEGE_USED_FOR_ACCESS     = 0x80000000,
        */
        Disabled          = 0,
        EnabledByDefault  = 0x00000001,
        Enabled           = 0x00000002,
        UsedForAccess     = 0x80000000,

    }

    [Flags]
	
    public enum GroupAttributes : uint
    {
        /*
        SE_GROUP_MANDATORY              = 0x00000001,
        SE_GROUP_ENABLED_BY_DEFAULT     = 0x00000002,
        SE_GROUP_ENABLED                = 0x00000004,
        SE_GROUP_OWNER                  = 0x00000008,
        SE_GROUP_USE_FOR_DENY_ONLY      = 0x00000010,
        SE_GROUP_LOGON_ID               = 0xC0000000,
        SE_GROUP_RESOURCE               = 0x20000000,
        */
        Mandatory         = 0x00000001,
        EnabledByDefault  = 0x00000002,
        Enabled           = 0x00000004,
        Owner             = 0x00000008,
        UseForDenyOnly    = 0x00000010,
        LogonId           = 0xC0000000,
        Resource          = 0x20000000,
    }
    [Flags]
    public enum UserAttributes : uint
    {
    }
    public enum AclRevision : byte
    {
        ACL_REVISION     = 2,
        ACL_REVISION_DS  = 4,
    }
    [Flags]
    public enum AceObjectFlags : uint
    {
        ACE_OBJECT_TYPE_PRESENT           = 0x1,
        ACE_INHERITED_OBJECT_TYPE_PRESENT = 0x2,
    }
    public enum SecurityDescriptorRevision
    {
        SECURITY_DESCRIPTOR_REVISION = 1
    }
    [Flags]
    public enum SecurityDescriptorControlFlags : ushort // WORD
    {
        SE_OWNER_DEFAULTED               = 0x0001,
        SE_GROUP_DEFAULTED               = 0x0002,
        SE_DACL_PRESENT                  = 0x0004,
        SE_DACL_DEFAULTED                = 0x0008,
        SE_SACL_PRESENT                  = 0x0010,
        SE_SACL_DEFAULTED                = 0x0020,
        SE_DACL_AUTO_INHERIT_REQ         = 0x0100,
        SE_SACL_AUTO_INHERIT_REQ         = 0x0200,
        SE_DACL_AUTO_INHERITED           = 0x0400,
        SE_SACL_AUTO_INHERITED           = 0x0800,
        SE_DACL_PROTECTED                = 0x1000,
        SE_SACL_PROTECTED                = 0x2000,
        SE_RM_CONTROL_VALID              = 0x4000,
        SE_SELF_RELATIVE                 = 0x8000,
    }

    [Flags]
    public enum SECURITY_INFORMATION : uint
    {
        OWNER_SECURITY_INFORMATION       = 0x00000001,
        GROUP_SECURITY_INFORMATION       = 0x00000002,
        DACL_SECURITY_INFORMATION        = 0x00000004,
        SACL_SECURITY_INFORMATION        = 0x00000008,

        // Win2k only
        PROTECTED_DACL_SECURITY_INFORMATION     = 0x80000000,
        // Win2k only
        PROTECTED_SACL_SECURITY_INFORMATION     = 0x40000000,
        // Win2k only
        UNPROTECTED_DACL_SECURITY_INFORMATION   = 0x20000000,
        // Win2k only
        UNPROTECTED_SACL_SECURITY_INFORMATION   = 0x10000000,
    }

    public enum MULTIPLE_TRUSTEE_OPERATION : uint
    {
        NO_MULTIPLE_TRUSTEE,
        TRUSTEE_IS_IMPERSONATE,
    }

    public enum TRUSTEE_FORM : uint
    {
        TRUSTEE_IS_SID,
        TRUSTEE_IS_NAME,
        TRUSTEE_BAD_FORM,
        TRUSTEE_IS_OBJECTS_AND_SID,
        TRUSTEE_IS_OBJECTS_AND_NAME
    }

    public enum TRUSTEE_TYPE : uint
    {
        TRUSTEE_IS_UNKNOWN,
        TRUSTEE_IS_USER,
        TRUSTEE_IS_GROUP,
        TRUSTEE_IS_DOMAIN,
        TRUSTEE_IS_ALIAS,
        TRUSTEE_IS_WELL_KNOWN_GROUP,
        TRUSTEE_IS_DELETED,
        TRUSTEE_IS_INVALID,
        TRUSTEE_IS_COMPUTER
    }

	public enum SE_OBJECT_TYPE : uint
	{
		SE_UNKNOWN_OBJECT_TYPE = 0,
		SE_FILE_OBJECT, 
		SE_SERVICE, 
		SE_PRINTER, 
		SE_REGISTRY_KEY, 
		SE_LMSHARE, 
		SE_KERNEL_OBJECT, 
		SE_WINDOW_OBJECT, 
		SE_DS_OBJECT, 
		SE_DS_OBJECT_ALL, 
		SE_PROVIDER_DEFINED_OBJECT, 
		SE_WMIGUID_OBJECT, 
		SE_REGISTRY_WOW64_32KEY
	}
}
