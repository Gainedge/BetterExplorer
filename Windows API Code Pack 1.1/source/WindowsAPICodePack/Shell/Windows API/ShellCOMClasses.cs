//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsAPICodePack.Shell {
	[ComImport,
	Guid(BExplorer.Shell.Interop.InterfaceGuids.IShellLibrary),
	CoClass(typeof(ShellLibraryCoClass))]
	internal interface INativeShellLibrary : IShellLibrary {
	}

	[ComImport,
	ClassInterface(ClassInterfaceType.None),
	TypeLibType(TypeLibTypeFlags.FCanCreate),
	Guid(BExplorer.Shell.Interop.InterfaceGuids.ShellLibrary)]
	internal class ShellLibraryCoClass {
	}
}
