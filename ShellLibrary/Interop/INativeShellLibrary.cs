using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	[ComImport,
		Guid(InterfaceGuids.IShellLibrary),
		CoClass(typeof(ShellLibraryCoClass))]
	internal interface INativeShellLibrary : IShellLibrary
	{
	}

	[ComImport,
	ClassInterface(ClassInterfaceType.None),
	TypeLibType(TypeLibTypeFlags.FCanCreate),
	Guid(InterfaceGuids.ShellLibrary)]
	internal class ShellLibraryCoClass
	{
	}
}
