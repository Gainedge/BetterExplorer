// Stephen Toub

using System;
using System.Collections.Generic;
using System.Text;

namespace FileOperations
{
    public enum CopyEngineResult : uint
    {
        COPYENGINE_OK = 0x0,

        COPYENGINE_S_YES = 0x00270001,
        COPYENGINE_S_NOT_HANDLED = 0x00270003,
        COPYENGINE_S_USER_RETRY = 0x00270004,
        COPYENGINE_S_USER_IGNORED = 0x00270005,
        COPYENGINE_S_MERGE = 0x00270006,
        COPYENGINE_S_DONT_PROCESS_CHILDREN = 0x00270008,
        COPYENGINE_S_ALREADY_DONE = 0x0027000A,
        COPYENGINE_S_PENDING = 0x0027000B,
        COPYENGINE_S_KEEP_BOTH = 0x0027000C,
        COPYENGINE_S_CLOSE_PROGRAM = 0x0027000D, // Close the program using the current file

        // Failure/Error codes
        COPYENGINE_E_USER_CANCELLED = 0x80270000,  // User wants to canceled entire job
        COPYENGINE_E_CANCELLED = 0x80270001,  // Engine wants to canceled entire job, don't set the CANCELLED bit
        COPYENGINE_E_REQUIRES_ELEVATION = 0x80270002,  // Need to elevate the process to complete the operation

        COPYENGINE_E_SAME_FILE = 0x80270003,  // Source and destination file are the same
        COPYENGINE_E_DIFF_DIR = 0x80270004,  // Trying to rename a file into a different location, use move instead
        COPYENGINE_E_MANY_SRC_1_DEST = 0x80270005,  // One source specified, multiple destinations

        COPYENGINE_E_DEST_SUBTREE = 0x80270009,  // The destination is a sub-tree of the source
        COPYENGINE_E_DEST_SAME_TREE = 0x8027000A,  // The destination is the same folder as the source

        COPYENGINE_E_FLD_IS_FILE_DEST = 0x8027000B,  // Existing destination file with same name as folder
        COPYENGINE_E_FILE_IS_FLD_DEST = 0x8027000C,  // Existing destination folder with same name as file

        COPYENGINE_E_FILE_TOO_LARGE = 0x8027000D,  // File too large for destination file system
        COPYENGINE_E_REMOVABLE_FULL = 0x8027000E,  // Destination device is full and happens to be removable

        COPYENGINE_E_DEST_IS_RO_CD = 0x8027000F,  // Destination is a Read-Only CDRom, possibly unformatted
        COPYENGINE_E_DEST_IS_RW_CD = 0x80270010,  // Destination is a Read/Write CDRom, possibly unformatted
        COPYENGINE_E_DEST_IS_R_CD = 0x80270011,  // Destination is a Recordable (Audio, CDRom, possibly unformatted

        COPYENGINE_E_DEST_IS_RO_DVD = 0x80270012,  // Destination is a Read-Only DVD, possibly unformatted
        COPYENGINE_E_DEST_IS_RW_DVD = 0x80270013,  // Destination is a Read/Wrote DVD, possibly unformatted
        COPYENGINE_E_DEST_IS_R_DVD = 0x80270014,  // Destination is a Recordable (Audio, DVD, possibly unformatted

        COPYENGINE_E_SRC_IS_RO_CD = 0x80270015,  // Source is a Read-Only CDRom, possibly unformatted
        COPYENGINE_E_SRC_IS_RW_CD = 0x80270016,  // Source is a Read/Write CDRom, possibly unformatted
        COPYENGINE_E_SRC_IS_R_CD = 0x80270017,  // Source is a Recordable (Audio, CDRom, possibly unformatted

        COPYENGINE_E_SRC_IS_RO_DVD = 0x80270018,  // Source is a Read-Only DVD, possibly unformatted
        COPYENGINE_E_SRC_IS_RW_DVD = 0x80270019,  // Source is a Read/Wrote DVD, possibly unformatted
        COPYENGINE_E_SRC_IS_R_DVD = 0x8027001A,  // Source is a Recordable (Audio, DVD, possibly unformatted

        COPYENGINE_E_INVALID_FILES_SRC = 0x8027001B,  // Invalid source path
        COPYENGINE_E_INVALID_FILES_DEST = 0x8027001C,  // Invalid destination path
        COPYENGINE_E_PATH_TOO_DEEP_SRC = 0x8027001D,  // Source Files within folders where the overall path is longer than MAX_PATH
        COPYENGINE_E_PATH_TOO_DEEP_DEST = 0x8027001E,  // Destination files would be within folders where the overall path is longer than MAX_PATH
        COPYENGINE_E_ROOT_DIR_SRC = 0x8027001F,  // Source is a root directory, cannot be moved or renamed
        COPYENGINE_E_ROOT_DIR_DEST = 0x80270020,  // Destination is a root directory, cannot be renamed
        COPYENGINE_E_ACCESS_DENIED_SRC = 0x80270021,  // Security problem on source
        COPYENGINE_E_ACCESS_DENIED_DEST = 0x80270022,  // Security problem on destination
        COPYENGINE_E_PATH_NOT_FOUND_SRC = 0x80270023,  // Source file does not exist, or is unavailable
        COPYENGINE_E_PATH_NOT_FOUND_DEST = 0x80270024,  // Destination file does not exist, or is unavailable
        COPYENGINE_E_NET_DISCONNECT_SRC = 0x80270025,  // Source file is on a disconnected network location
        COPYENGINE_E_NET_DISCONNECT_DEST = 0x80270026,  // Destination file is on a disconnected network location
        COPYENGINE_E_SHARING_VIOLATION_SRC = 0x80270027,  // Sharing Violation on source
        COPYENGINE_E_SHARING_VIOLATION_DEST = 0x80270028,  // Sharing Violation on destination

        COPYENGINE_E_ALREADY_EXISTS_NORMAL = 0x80270029, // Destination exists, cannot replace
        COPYENGINE_E_ALREADY_EXISTS_READONLY = 0x8027002A, // Destination with read-only attribute exists, cannot replace
        COPYENGINE_E_ALREADY_EXISTS_SYSTEM = 0x8027002B, // Destination with system attribute exists, cannot replace
        COPYENGINE_E_ALREADY_EXISTS_FOLDER = 0x8027002C, // Destination folder exists, cannot replace
        COPYENGINE_E_STREAM_LOSS = 0x8027002D, // Secondary Stream information would be lost
        COPYENGINE_E_EA_LOSS = 0x8027002E, // Extended Attributes would be lost
        COPYENGINE_E_PROPERTY_LOSS = 0x8027002F, // Property would be lost
        COPYENGINE_E_PROPERTIES_LOSS = 0x80270030, // Properties would be lost
        COPYENGINE_E_ENCRYPTION_LOSS = 0x80270031, // Encryption would be lost
        COPYENGINE_E_DISK_FULL = 0x80270032, // Entire operation likely won't fit
        COPYENGINE_E_DISK_FULL_CLEAN = 0x80270033, // Entire operation likely won't fit, clean-up wizard available
        COPYENGINE_E_CANT_REACH_SOURCE = 0x80270035, // Can't reach source folder")

        COPYENGINE_E_RECYCLE_UNKNOWN_ERROR = 0x80270035, // ???
        COPYENGINE_E_RECYCLE_FORCE_NUKE = 0x80270036, // Recycling not available (usually turned off,
        COPYENGINE_E_RECYCLE_SIZE_TOO_BIG = 0x80270037, // Item is too large for the recycle-bin
        COPYENGINE_E_RECYCLE_PATH_TOO_LONG = 0x80270038, // Folder is too deep to fit in the recycle-bin
        COPYENGINE_E_RECYCLE_BIN_NOT_FOUND = 0x8027003A, // Recycle bin could not be found or is unavailable
        COPYENGINE_E_NEWFILE_NAME_TOO_LONG = 0x8027003B, // Name of the new file being created is too long
        COPYENGINE_E_NEWFOLDER_NAME_TOO_LONG = 0x8027003C, // Name of the new folder being created is too long
        COPYENGINE_E_DIR_NOT_EMPTY = 0x8027003D, // The directory being processed is not empty

        //  error codes without a more specific group use FACILITY_SHELL and 0x01 in the second lowest byte.
        NETCACHE_E_NEGATIVE_CACHE = 0x80270100, // The item requested is in the negative net parsing cache
        EXECUTE_E_LAUNCH_APPLICATION = 0x80270101, // for returned by command delegates to indicate that they did no work 
        SHELL_E_WRONG_BITDEPTH = 0x80270102, // returned when trying to create a thumbnail extractor at too low a bitdepth for high fidelity
    }
}
