using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct LVITEM
	{
		public LVIF mask;
		public int iItem;
		public int iSubItem;
		public LVIS state;
		public LVIS stateMask;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszText;
		public int cchTextMax;
		public int iImage;
		public uint lParam;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMHDR
	{
		// 12/24
		public IntPtr hwndFrom;
		public IntPtr idFrom;
		public long code;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct NMLVDISPINFO
	{
		public NMHDR hdr;
		public LVITEM item;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct NMLVDISPINFO_NOTEXT
	{
		public NMHDR hdr;
		public LVITEM_NOTEXT item;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct LVITEM_NOTEXT
	{
		public LVIF mask;
		public Int32 iItem;
		public Int32 iSubItem;
		public LVIS state;
		public LVIS stateMask;
		public IntPtr pszText;
		public Int32 cchTextMax;
		public Int32 iImage;
		public IntPtr lParam;
		public Int32 iIndent;
	}

	public enum LVCF{
  LVCF_FMT = 0x1,
  LVCF_WIDTH = 0x2,
  LVCF_TEXT = 0x4,
  LVCF_SUBITEM = 0x8
	}

  [StructLayout(LayoutKind.Sequential)]
  public struct LVCOLUMN
  {
      public LVCF mask;
      public LVCFMT fmt;
      public Int32 cx;
      public String pszText;
      public Int32 cchTextMax;
      public Int32 iSubItem;
  }
	public static class Extensions
	{

	}
}
