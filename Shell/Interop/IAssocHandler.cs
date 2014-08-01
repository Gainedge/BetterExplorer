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

	[ComImport, Guid("F04061AC-1659-4a3f-A954-775AA57FC083"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComConversionLoss]
	public interface IAssocHandler {

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetName([MarshalAs(UnmanagedType.LPWStr), Out] String ppsz);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetUIName([MarshalAs(UnmanagedType.LPWStr), Out] String ppsz);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetIconLocation([MarshalAs(UnmanagedType.LPWStr), Out] String ppszPath, [Out] int pIndex);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult IsRecommended();

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult MakeDefault([MarshalAs(UnmanagedType.LPWStr), In] String pszDescription);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult Invoke([In] IDataObject pdo);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult CreateInvoker([In] IDataObject pdo, [Out] IntPtr ppInvoker);
	}


}
