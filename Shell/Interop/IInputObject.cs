using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GongSolutions.Shell.Interop
{
	[ComImport,
		 Guid(InterfaceGuids.IInputObject),
		 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IInputObject
	{
		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult UIActivateIO(bool fActivate, ref System.Windows.Forms.Message pMsg);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult HasFocusIO();

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult TranslateAcceleratorIO(ref System.Windows.Forms.Message pMsg);

	};
}
