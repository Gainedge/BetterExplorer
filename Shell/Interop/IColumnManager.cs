using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	public enum CM_ENUM_FLAGS
	{
		CM_ENUM_ALL = 0x00000001,
		CM_ENUM_VISIBLE = 0x00000002
	};

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid(InterfaceGuids.IColumnManager)]
	public interface IColumnManager
	{

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult SetColumnInfo([In] ref PROPERTYKEY propkey, [In] ref IntPtr pcmci);

		[PreserveSig]
		HResult GetColumnInfo(REFPROPERTYKEY propkey, ref CM_COLUMNINFO pcmci);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetColumnCount(CM_ENUM_FLAGS dwFlags, out uint puCount);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetColumns(CM_ENUM_FLAGS dwFlags, [Out][MarshalAs(UnmanagedType.LPArray)] PROPERTYKEY[] rgkeyOrder, uint cColumns);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult SetColumns([In][MarshalAs(UnmanagedType.LPArray)] PROPERTYKEY[] rgkeyOrder, uint cVisible);
	}

	public enum SORT
	{
		DESCENDING = -1,
		ASCENDING = 1
	}
	[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct CM_COLUMNINFO
	{

		/// DWORD->unsigned int
		public int cbSize;

		/// DWORD->unsigned int
		public CM_MASK dwMask;

		/// DWORD->unsigned int
		public CM_STATE dwState;

		/// UINT->unsigned int
		public CM_SET_WIDTH_VALUE uWidth;

		/// UINT->unsigned int
		public uint uDefaultWidth;

		/// UINT->unsigned int
		public uint uIdealWidth;

		/// WCHAR[]
		[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string wszName;
	}
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct SORTCOLUMN
	{
		public PROPERTYKEY propkey;
		public SORT direction;
	}

	public enum CM_MASK
	{
		CM_MASK_WIDTH = 0x00000001,
		CM_MASK_DEFAULTWIDTH = 0x00000002,
		CM_MASK_IDEALWIDTH = 0x00000004,
		CM_MASK_NAME = 0x00000008,
		CM_MASK_STATE = 0x00000010
	}

	public enum CM_STATE
	{
		CM_STATE_NONE = 0x00000000,
		CM_STATE_VISIBLE = 0x00000001,
		CM_STATE_FIXEDWIDTH = 0x00000002,
		CM_STATE_NOSORTBYFOLDERNESS = 0x00000004,
		CM_STATE_ALWAYSVISIBLE = 0x00000008
	}

	public enum CM_SET_WIDTH_VALUE
	{
		CM_WIDTH_USEDEFAULT = -1,
		CM_WIDTH_AUTOSIZE = -2
	}

}
