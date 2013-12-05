using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
		public IntPtr lParam;
		public int    iIndent;
		public int iGroupId;
		public int cColumns;
		public uint puColumns;
		public int piColFmt;
		public int iGroup;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMHDR
	{
		// 12/24
		public IntPtr hwndFrom;
		public IntPtr idFrom;
		public int code;
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

	[StructLayout(LayoutKind.Sequential)]
	public struct NMHEADER
	{
		public NMHDR hdr;
		public int iItem;
		public int iButton;
		public IntPtr pitem;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMCUSTOMDRAW
	{
		// 48/80	
		public NMHDR hdr;
		public int dwDrawStage;
		public IntPtr hdc;
		public User32.RECT rc;
		public IntPtr dwItemSpec;
		public CDIS uItemState;
		public IntPtr lItemlParam;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMLVCUSTOMDRAW
	{
		// 104/136  
		public NMCUSTOMDRAW nmcd;
		public int clrText;
		public int clrTextBk;
		public int iSubItem;
		public int dwItemType;
		public int clrFace;
		public int iIconEffect;
		public int iIconPhase;
		public int iPartId;
		public int iStateId;
		public User32.RECT rcText;
		public int uAlign;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMITEMACTIVATE
	{
		public NMHDR hdr;
		public int iItem;
		public int iSubItem;
		public uint uNewState;
		public uint uOldState;
		public uint uChanged;
		public Point ptAction;
		public IntPtr lParam;
		public uint uKeyFlags;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct NMLISTVIEW
	{
		public NMHDR hdr;
		public int iItem;
		public int iSubItem;
		public uint uNewState;
		public uint uOldState;
		public LVIF uChanged;
		public Point ptAction;
		public IntPtr lParam;
	}

	public enum LVTVIM
	{
		LVTVIM_COLUMNS = 2,
		LVTVIM_TILESIZE = 1,
		LVTVIM_LABELMARGIN = 4,
	}

	public enum LVTVIF
	{
		LVTVIF_AUTOSIZE = 0,
		LVTVIF_FIXEDHEIGHT = 2,
		LVTVIF_FIXEDSIZE = 3,
		LVTVIF_FIXEDWIDTH = 1,
	}
	public struct INTEROP_SIZE
	{
		public int cx;
		public int cy;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct LVTILEVIEWINFO
	{
		public int cbSize;
		public int dwMask;
		public int dwFlags;
		public INTEROP_SIZE sizeTile;
		public int cLines;
		public INTEROP_SIZE rcLabelMargin;
	}
	public static class Extensions
	{

		public static Collumns ToCollumns(this LVCOLUMN column, PROPERTYKEY pkey, Type type, bool IsColumnHandler){
			Collumns col = new Collumns();
			col.pkey = pkey;
			col.Name = column.pszText;
			col.Width = column.cx;
			col.IsColumnHandler = IsColumnHandler;
			col.CollumnType = type;
			return col;
		}

		public static void SetSplitButton(this Collumns column, IntPtr handle, int index){
			
			var item = new HDITEM
			{
				mask = HDITEM.Mask.Format
			};

			if (User32.SendMessage(handle, MSG.HDM_GETITEM, index, ref item) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}


			item.fmt |= HDITEM.Format.HDF_SPLITBUTTON;

			if (User32.SendMessage(handle, MSG.HDM_SETITEM, index, ref item) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
		}

		public static void SetFormat(this LVCOLUMN column, IntPtr handle, int index){
			
			var item = new HDITEM
			{
				mask = HDITEM.Mask.Format
			};

			if (User32.SendMessage(handle, MSG.HDM_GETITEM, index, ref item) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}


			item.fmt |= HDITEM.Format.HDF_SPLITBUTTON;

			if (User32.SendMessage(handle, MSG.HDM_SETITEM, index, ref item) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
		}

		public static String ToShellParsingName(this String path)
		{
			if (path.IndexOf("::") == 0 && path.IndexOf(@"\") == -1)
			{
				return String.Format("shell:{0}", path);
			}
			else
			{
				return path;
			}
		}

		public static void SetSortIcon(this ShellView listViewControl, int columnIndex, SortOrder order)
		{
			IntPtr columnHeader = User32.SendMessage(listViewControl.LVHandle, MSG.LVM_GETHEADER, 0, 0);
			for (int columnNumber = 0; columnNumber <= listViewControl.AvailableVisibleColumns.Count - 1; columnNumber++)
			{
				var item = new HDITEM
				{
					mask = HDITEM.Mask.Format
				};

				if (User32.SendMessage(columnHeader, MSG.HDM_GETITEM, columnNumber, ref item) == IntPtr.Zero)
				{
					throw new Win32Exception();
				}

				if (order != SortOrder.None && columnNumber == columnIndex)
				{
					switch (order)
					{
						case SortOrder.Ascending:
							item.fmt &= ~HDITEM.Format.SortDown;
							item.fmt |= HDITEM.Format.SortUp;
							break;
						case SortOrder.Descending:
							item.fmt &= ~HDITEM.Format.SortUp;
							item.fmt |= HDITEM.Format.SortDown;
							break;
					}
				}
				else
				{
					item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
				}

				if (User32.SendMessage(columnHeader, MSG.HDM_SETITEM, columnNumber, ref item) == IntPtr.Zero)
				{
					throw new Win32Exception();
				}
			}
		}

	}
}
