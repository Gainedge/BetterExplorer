using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	[ComVisible(false)]
	public enum LVCFMT
	{
		LEFT = 0x0000,
		RIGHT = 0x0001,
		CENTER = 0x0002,
		JUSTIFYMASK = 0x0003,
		IMAGE = 0x0800,
		BITMAP_ON_RIGHT = 0x1000,
		COL_HAS_IMAGES = 0x8000,
		SPLITBUTTON = 0x1000000
	}
	[Flags, ComVisible(false)]
	public enum SHCOLSTATE
	{
		TYPE_STR = 0x1,
		TYPE_INT = 0x2,
		TYPE_DATE = 0x3,
		TYPEMASK = 0xf,
		ONBYDEFAULT = 0x10,
		SLOW = 0x20,
		EXTENDED = 0x40,
		SECONDARYUI = 0x80,
		HIDDEN = 0x100,
		PREFER_VARCMP = 0x200
	}

	[ComVisible(false), StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class LPCSHCOLUMNINIT
	{
		public uint dwFlags; //ulong
		public uint dwReserved; //ulong
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string wszFolder; //[MAX_PATH]; wchar
	}

	[ComVisible(false), StructLayout(LayoutKind.Sequential)]
	public struct SHCOLUMNID
	{
		public Guid fmtid; //GUID
		public uint pid; //DWORD
	}

	[ComVisible(false), StructLayout(LayoutKind.Sequential)]
	public class LPCSHCOLUMNID
	{
		public Guid fmtid; //GUID
		public uint pid; //DWORD
	}

	[ComVisible(false), StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	public struct SHCOLUMNINFO
	{
		public SHCOLUMNID scid; //SHCOLUMNID
		public ushort vt; //VARTYPE
		public LVCFMT fmt; //DWORD
		public uint cChars; //UINT
		public SHCOLSTATE csFlags;  //DWORD
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] //MAX_COLUMN_NAME_LEN
		public string wszTitle; //WCHAR
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] //MAX_COLUMN_DESC_LEN
		public string wszDescription; //WCHAR
	}

	[ComVisible(false), StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class LPCSHCOLUMNDATA
	{
		public uint dwFlags; //ulong
		public uint dwFileAttributes; //dword
		public uint dwReserved; //ulong
		[MarshalAs(UnmanagedType.LPWStr)]
		public string pwszExt; //wchar
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string wszFile; //[MAX_PATH]; wchar
	}

	[ComVisible(false), ComImport, Guid("E8025004-1C42-11d2-BE2C-00A0C9A83DA1"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColumnProvider
	{
		[PreserveSig()]
		int Initialize(LPCSHCOLUMNINIT psci);
		[PreserveSig()]
		int GetColumnInfo(int dwIndex, out SHCOLUMNINFO psci);

		/// <summary>
		/// Note: these objects must be threadsafe!  GetItemData _will_ be called
		/// simultaneously from multiple threads.
		/// </summary>
		[PreserveSig()]
		int GetItemData(LPCSHCOLUMNID pscid, LPCSHCOLUMNDATA pscd, out object /*VARIANT */ pvarData);
	}

}
