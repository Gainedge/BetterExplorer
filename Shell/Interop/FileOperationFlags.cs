using System;

namespace BExplorer.Shell.Interop {
	[Flags]
	public enum FileOperationFlags : uint {
		FOF_MULTIDESTFILES = 0x0001,
		FOF_CONFIRMMOUSE = 0x0002,
		/// <summary>Do not display a progress dialog box.</summary>
		FOF_SILENT = 0x0004,                // don't create progress/report
											/// <summary>Give the item being operated on a new name in a move, copy, or rename operation if an item with the target name already exists.</summary>
		FOF_RENAMEONCOLLISION = 0x0008,
		/// <summary>Respond with Yes to All for any dialog box that is displayed. (Don't prompt the user.)</summary>
		FOF_NOCONFIRMATION = 0x0010,        // Don't prompt the user.
		FOF_WANTMAPPINGHANDLE = 0x0020,     // Fill in SHFILEOPSTRUCT.hNameMappings
											// Must be freed using SHFreeNameMappings
											/// <summary>Preserve undo information, if possible. </summary>
		FOF_ALLOWUNDO = 0x0040,
		/// <summary>Perform the operation only on files (not on folders) if a wildcard file name (*.*) is specified.</summary>
		FOF_FILESONLY = 0x0080,             // on *.*, do only files
		FOF_SIMPLEPROGRESS = 0x0100,        // means don't show names of files
											/// <summary>Do not confirm the creation of a new folder if the operation requires one to be created.</summary>
		FOF_NOCONFIRMMKDIR = 0x0200,        // don't confirm making any needed dirs
		FOF_NOERRORUI = 0x0400,             // don't put up error UI
											/// <summary>Do not copy the security attributes of the item.</summary>
		FOF_NOCOPYSECURITYATTRIBS = 0x0800, // dont copy NT file Security Attributes
											/// <summary>Only operate in the local folder. Do not operate recursively into subdirectories.</summary>
		FOF_NORECURSION = 0x1000,           // don't recurse into directories.
											/// <summary>Do not move connected items as a group. Only move the specified files.</summary>
		FOF_NO_CONNECTED_ELEMENTS = 0x2000, // don't operate on connected file elements.
											/// <summary>Send a warning if a file or folder is being destroyed during a delete operation rather than recycled. This flag partially overrides FOF_NOCONFIRMATION.</summary>
		FOF_WANTNUKEWARNING = 0x4000,       // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
		FOF_NORECURSEREPARSE = 0x8000,      // treat reparse points as objects, not containers

		/// <summary>Walk into Shell namespace junctions. By default, junctions are not entered. For more information on junctions, see Specifying a Namespace Extension's Location.</summary>
		FOFX_NOSKIPJUNCTIONS = 0x00010000,  // Don't avoid binding to junctions (like Task folder, Recycle-Bin)
											/// <summary>FOFX_PREFERHARDLINK</summary>
		FOFX_PREFERHARDLINK = 0x00020000,   // Create hard link if possible
											/// <summary>If an operation requires elevated rights and the FOF_NOERRORUI flag is set to disable error UI, display a UAC UI prompt nonetheless.</summary>
		FOFX_SHOWELEVATIONPROMPT = 0x00040000,  // Show elevation prompts when error UI is disabled (use with FOF_NOERRORUI)
												/// <summary>FOFX_EARLYFAILURE</summary>
		FOFX_EARLYFAILURE = 0x00100000,     // Fail operation as soon as a single error occurs rather than trying to process other items (applies only when using FOF_NOERRORUI)
											/// <summary>Rename collisions in such a way as to preserve file name extensions. This flag is valid only when FOF_RENAMEONCOLLISION is also set.</summary>
		FOFX_PRESERVEFILEEXTENSIONS = 0x00200000,  // Rename collisions preserve file extns (use with FOF_RENAMEONCOLLISION)
												   /// <summary>Keep the newer file or folder, based on the Date Modified property, if a collision occurs. This is done automatically with no prompt UI presented to the user.</summary>
		FOFX_KEEPNEWERFILE = 0x00400000,    // Keep newer file on naming conflicts
											/// <summary>Do not use copy hooks.</summary>
		FOFX_NOCOPYHOOKS = 0x00800000,      // Don't use copy hooks
											/// <summary>Do not allow the progress dialog to be minimized.</summary>
		FOFX_NOMINIMIZEBOX = 0x01000000,    // Don't allow minimizing the progress dialog
											/// <summary>Copy the security attributes of the source item to the destination item when performing a cross-volume move operation. Without this flag, the destination item receives the security attributes of its new folder.</summary>
		FOFX_MOVEACLSACROSSVOLUMES = 0x02000000,    // Copy security information when performing a cross-volume move operation
													/// <summary>Do not display the path of the source item in the progress dialog.</summary>
		FOFX_DONTDISPLAYSOURCEPATH = 0x04000000,    // Don't display the path of source file in progress dialog
													/// <summary>Do not display the path of the destination item in the progress dialog.</summary>
		FOFX_DONTDISPLAYDESTPATH = 0x08000000,      // Don't display the path of destination file in progress dialog
	}
}