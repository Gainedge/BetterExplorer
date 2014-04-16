using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("973810ae-9599-4b88-9e4d-6ee98c9552da")]
	public interface IEnumAssocHandlers
	{
		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult Next(uint celt,
				out IAssocHandler rgelt,
				out uint pceltFetched);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("F04061AC-1659-4a3f-A954-775AA57FC083")]
	public interface IAssocHandler
	{
		[PreserveSig]
		HResult GetName(out String ppsz);

		[PreserveSig]
		HResult GetUIName(out String ppsz);

		[PreserveSig]
		HResult IsRecommended();

		[PreserveSig]
		HResult Invoke(IDataObject pdo);
	}

}
