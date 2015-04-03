using BExplorer.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BExplorer.Shell._Plugin_Interfaces;

namespace BExplorer.Shell {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct LVITEM {
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
		public int iIndent;
		public int iGroupId;
		public int cColumns;
		public IntPtr puColumns;
		public int piColFmt;
		public int iGroup;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMHDR {
		// 12/24
		public IntPtr hwndFrom;
		public IntPtr idFrom;
		public int code;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct NMLVDISPINFO {
		public NMHDR hdr;
		public LVITEM item;
	}

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct NMLVDISPINFO_NOTEXT {
		public NMHDR hdr;
		public LVITEM_NOTEXT item;
	}
	*/

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct LVITEM_NOTEXT {
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
	*/


	public enum LVCF {
		LVCF_FMT = 0x1,
		LVCF_WIDTH = 0x2,
		LVCF_TEXT = 0x4,
		LVCF_SUBITEM = 0x8,
		LVCF_MINWIDTH = 0x0040
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct LVCOLUMN {
		public LVCF mask;
		public LVCFMT fmt;
		public Int32 cx;
		public String pszText;
		public Int32 cchTextMax;
		public Int32 iSubItem;
		public int iImage;
		public int iOrder;
		public int cxMin;
		public int cxDefault;
		public int cxIdeal;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMHEADER {
		public NMHDR hdr;
		public int iItem;
		public int iButton;
		public IntPtr pitem;
	}

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct NMCUSTOMDRAW {
		// 48/80	
		public NMHDR hdr;
		public int dwDrawStage;
		public IntPtr hdc;
		public User32.RECT rc;
		public IntPtr dwItemSpec;
		public CDIS uItemState;
		public IntPtr lItemlParam;
	}
	*/

	/*
	//[StructLayout(LayoutKind.Sequential)]
	//public struct NMLVCUSTOMDRAW {
	//	// 104/136  
	//	public NMCUSTOMDRAW nmcd;
	//	public int clrText;
	//	public int clrTextBk;
	//	public int iSubItem;
	//	public int dwItemType;
	//	public int clrFace;
	//	public int iIconEffect;
	//	public int iIconPhase;
	//	public int iPartId;
	//	public int iStateId;
	//	public User32.RECT rcText;
	//	public int uAlign;
	//}
	*/

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct NMTVCUSTOMDRAW {
		// 104/136  
		public NMCUSTOMDRAW nmcd;
		public int clrText;
		public int clrTextBk;
		public int iLevel;
	}
	*/

	[StructLayout(LayoutKind.Sequential)]
	public struct NMITEMACTIVATE {
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
	internal struct NMLISTVIEW {
		public NMHDR hdr;
		public int iItem;
		public int iSubItem;
		public LVIS uNewState;
		public LVIS uOldState;
		public LVIF uChanged;
		public POINT ptAction;
		public IntPtr lParam;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class NMLVKEYDOWN {
		public NMHDR hdr;
		public short wVKey;
		public uint flags;
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct NMLVFINDITEM {
		public NMHDR hdr;
		public int iStart;
		public LVFINDINFO lvfi;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NMLVGETINFOTIP {
		public NMHDR hdr;
		public int dwFlags;
		public IntPtr pszText;
		public int cchTextMax;
		public int iItem;
		public int iSubItem;
		public IntPtr lParam;
	}

	/*
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct LVSETINFOTIP {
		public int cbSize;
		public uint dwFlags;
		public string pszText;
		public int iItem;
		public int iSubItem;
	}
	*/

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct LVFINDINFO {
		public LVFI flags;
		public string psz;
		public IntPtr lParam;
		public int ptX; // was POINT pt
		public int ptY;
		public int vkDirection;
	}


	public enum LVFI {
		LVFI_PARAM = 0x0001,
		LVFI_STRING = 0x0002,
		LVFI_SUBSTRING = 0x0004,  // Same as LVFI_PARTIAL
		LVFI_PARTIAL = 0x0008,
		LVFI_WRAP = 0x0020,
		LVFI_NEARESTXY = 0x0040,
	}

	/*
	public enum LVTVIM {
		LVTVIM_COLUMNS = 2,
		LVTVIM_TILESIZE = 1,
		LVTVIM_LABELMARGIN = 4,
	}
	*/

	/*
	public enum LVTVIF {
		LVTVIF_AUTOSIZE = 0,
		LVTVIF_FIXEDHEIGHT = 2,
		LVTVIF_FIXEDSIZE = 3,
		LVTVIF_FIXEDWIDTH = 1,
	}
	*/

	/*
	public struct INTEROP_SIZE {
		public int cx;
		public int cy;
	}
	*/

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct LVTILEVIEWINFO {
		public int cbSize;
		public int dwMask;
		public int dwFlags;
		public INTEROP_SIZE sizeTile;
		public int cLines;
		public INTEROP_SIZE rcLabelMargin;
	}
	*/

	public static class Extensions {

		public static LVGROUP2 ToNativeListViewGroup(this ListViewGroupEx group) {
			LVGROUP2 nativeGroup = new LVGROUP2();
			nativeGroup.cbSize = (uint)Marshal.SizeOf(typeof(LVGROUP2));
			nativeGroup.mask = (uint)(GroupMask.LVGF_HEADER ^ GroupMask.LVGF_STATE ^ GroupMask.LVGF_GROUPID);
			nativeGroup.stateMask = (uint)GroupState.LVGS_COLLAPSIBLE;
			nativeGroup.state = (uint)GroupState.LVGS_COLLAPSIBLE;
			nativeGroup.pszHeader = group.Header;
			nativeGroup.iGroupId = group.Index;

			if (group.Items.Count() > 0) {
				nativeGroup.cItems = group.Items.Count();
				nativeGroup.mask ^= (uint)GroupMask.LVGF_ITEMS;
			}

			return nativeGroup;
		}

		public static Collumns ToCollumns(this LVCOLUMN column, PROPERTYKEY pkey, Type type, bool IsColumnHandler, int minWidth) {
			Collumns col = new Collumns();
			col.pkey = pkey;
			col.Name = column.pszText;
			col.Width = minWidth;
			col.IsColumnHandler = IsColumnHandler;
			col.CollumnType = type;
			col.MinWidth = minWidth;
			return col;
		}

		public static void SetSplitButton(this Collumns column, IntPtr handle, int index) {
			var item = new HDITEM {
				mask = HDITEM.Mask.Format
			};

			if (User32.SendMessage(handle, BExplorer.Shell.Interop.MSG.HDM_GETITEM, index, ref item) == IntPtr.Zero) {
				throw new Win32Exception();
			}

			item.fmt |= HDITEM.Format.HDF_SPLITBUTTON;

			if (User32.SendMessage(handle, BExplorer.Shell.Interop.MSG.HDM_SETITEM, index, ref item) == IntPtr.Zero) {
				throw new Win32Exception();
			}
		}

		/*
		public static void SetFormat(this LVCOLUMN column, IntPtr handle, int index) {
			var item = new HDITEM {
				mask = HDITEM.Mask.Format
			};

			if (User32.SendMessage(handle, BExplorer.Shell.Interop.MSG.HDM_GETITEM, index, ref item) == IntPtr.Zero) {
				throw new Win32Exception();
			}

			item.fmt |= HDITEM.Format.HDF_SPLITBUTTON;

			if (User32.SendMessage(handle, BExplorer.Shell.Interop.MSG.HDM_SETITEM, index, ref item) == IntPtr.Zero) {
				throw new Win32Exception();
			}
		}
		*/


		/// <summary>
		/// Converts a File/Folder path into a proper string used to create a <see cref="ShellItem"/>
		/// </summary>
		/// <param name="path">The path you want to convert</param>
		/// <returns></returns>
		public static String ToShellParsingName(this String path) {
      if (path.StartsWith("shell::")) return path;
      if (path.StartsWith("%")) {
        return Environment.ExpandEnvironmentVariables(path);
      } else if (path.StartsWith("::") && !path.StartsWith(@"\\"))
        return String.Format("shell:{0}", path);
      //else 
      //	if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
      //	return new ShellItem(String.Format("{0}{1}", path, Path.DirectorySeparatorChar));
      else if (!path.StartsWith(@"\\")) {
        if (path.Contains(":")) {
          return String.Format("{0}{1}", path, path.EndsWith(@"\") ? String.Empty : Path.DirectorySeparatorChar.ToString());
        } else {
          try {
            return String.Format("{0}{1}", path, Path.DirectorySeparatorChar);
          } catch (Exception) {
            return @"\\" + String.Format("{0}{1}", path, Path.DirectorySeparatorChar);
            throw;
          }
        }
      } else
        return path;
		}

		public static System.Runtime.InteropServices.ComTypes.IDataObject GetIDataObject(this IListItemEx[] items, out IntPtr dataObjectPtr) {
			var parent = items[0].Parent != null ? items[0].Parent : items[0];

			IntPtr[] pidls = new IntPtr[items.Length];
			for (int i = 0; i < items.Length; i++)
				pidls[i] = items[i].ILPidl;
			Guid IID_IDataObject = Ole32.IID_IDataObject;
			parent.GetIShellFolder().GetUIObjectOf(IntPtr.Zero, (uint)pidls.Length, pidls, ref IID_IDataObject, 0, out dataObjectPtr);

			System.Runtime.InteropServices.ComTypes.IDataObject dataObj =
					(System.Runtime.InteropServices.ComTypes.IDataObject)
							Marshal.GetTypedObjectForIUnknown(dataObjectPtr, typeof(System.Runtime.InteropServices.ComTypes.IDataObject));

			return dataObj;
		}

		public static System.Runtime.InteropServices.ComTypes.IDataObject GetIDataObject(this IListItemEx item, out IntPtr dataObjectPtr) {
			var parent = item.Parent != null ? item.Parent : item;

			IntPtr[] pidls = new IntPtr[1];
			pidls[0] = item.ILPidl;
			Guid IID_IDataObject = Ole32.IID_IDataObject;
			parent.GetIShellFolder().GetUIObjectOf(IntPtr.Zero, (uint)pidls.Length, pidls, ref IID_IDataObject, 0, out dataObjectPtr);

			System.Runtime.InteropServices.ComTypes.IDataObject dataObj =
					(System.Runtime.InteropServices.ComTypes.IDataObject)
							Marshal.GetTypedObjectForIUnknown(dataObjectPtr, typeof(System.Runtime.InteropServices.ComTypes.IDataObject));

			return dataObj;
		}

		/*
		public static void SetSortIcon(this ShellView listViewControl, int columnIndex, SortOrder order) {
			IntPtr columnHeader = User32.SendMessage(listViewControl.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETHEADER, 0, 0);
			for (int columnNumber = 0; columnNumber <= listViewControl.Collumns.Count - 1; columnNumber++) {
				var item = new HDITEM {
					mask = HDITEM.Mask.Format
				};

				if (User32.SendMessage(columnHeader, BExplorer.Shell.Interop.MSG.HDM_GETITEM, columnNumber, ref item) == IntPtr.Zero) {
					throw new Win32Exception();
				}

				if (order != SortOrder.None && columnNumber == columnIndex) {
					switch (order) {
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
				else {
					item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
				}

				if (User32.SendMessage(columnHeader, BExplorer.Shell.Interop.MSG.HDM_SETITEM, columnNumber, ref item) == IntPtr.Zero) {
					throw new Win32Exception();
				}
			}
		}
		*/
		public static bool HitTest(this ShellView shellView, Point hitPoint, out int row, out int column) {
			// clear the output values
			row = column = -1;

			// set up some win32 api constant values
			const int LVM_FIRST = 0x1000;
			//const int LVM_SUBITEMHITTEST = (LVM_FIRST + 57);
			const int LVM_HITTEST = (LVM_FIRST + 18);

			const int LVHT_NOWHERE = 0x1;
			const int LVHT_ONITEMICON = 0x2;
			const int LVHT_ONITEMLABEL = 0x4;
			const int LVHT_ONITEMSTATEICON = 0x8;
			const int LVHT_EX_ONCONTENTS = 0x04000000;
			const int LVHT_ONITEM = (LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON | LVHT_EX_ONCONTENTS);

			// set up the return value
			bool hitLocationFound = false;

			// initialize a hittest information structure
			LVHITTESTINFO lvHitTestInfo = new LVHITTESTINFO();
			lvHitTestInfo.pt.x = hitPoint.X;
			lvHitTestInfo.pt.y = hitPoint.Y;

			// send the hittest message to find out where the click was
			if (User32.SendMessage(shellView.LVHandle, LVM_HITTEST, 0, ref lvHitTestInfo) != 0) {
				bool nowhere = ((lvHitTestInfo.flags & LVHT_NOWHERE) != 0);
				bool onItem = ((lvHitTestInfo.flags & LVHT_ONITEM) != 0);

				if (onItem && !nowhere) {
					row = lvHitTestInfo.iItem;
					column = lvHitTestInfo.iSubItem;
					hitLocationFound = true;
				}
			}
			else if (User32.SendMessage(shellView.LVHandle, LVM_FIRST, 0, ref lvHitTestInfo) != 0) {
				row = 0;
				hitLocationFound = true;
			}

			return hitLocationFound;
		}

		/// <summary>
		/// Converts an <see cref="IShellItemArray"/> into a IShellItem[]
		/// </summary>
		/// <param name="shellItemArray">The Interface you want to convert</param>
		/// <returns></returns>
		public static IShellItem[] ToArray(this IShellItemArray shellItemArray) {
			var items = new List<IShellItem>();
			if (shellItemArray != null) {
				try {
					uint itemCount = 0;
					shellItemArray.GetCount(out itemCount);
					for (uint index = 0; index < itemCount; index++) {
						IShellItem iShellItem = null;
						shellItemArray.GetItemAt(index, out iShellItem);
						items.Add(iShellItem);
					}
				}
				finally {
					Marshal.ReleaseComObject(shellItemArray);
				}
			}
			return items.ToArray();
		}

		/// <summary>
		/// Converts a <see cref="IDataObject"/> into an <see cref="IShellItemArray"/>
		/// </summary>
		/// <param name="dataobject">The Interface you want to convert</param>
		/// <returns></returns>
		public static IShellItemArray ToShellItemArray(this IDataObject dataobject) {
			IShellItemArray shellItemArray;
			var iid = new Guid(InterfaceGuids.IShellItemArray);
			Shell32.SHCreateShellItemArrayFromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)dataobject, iid, out shellItemArray);
			return shellItemArray;
		}

		/// <summary>
		/// Converts a <see cref="IDataObject"/> into an <see cref="IShellItemArray"/>
		/// </summary>
		/// <param name="dataobject">The Interface you want to convert</param>
		/// <returns></returns>
		public static IShellItemArray ToShellItemArray(this System.Windows.IDataObject dataobject) {
			IShellItemArray shellItemArray;
			var iid = new Guid(InterfaceGuids.IShellItemArray);
			Shell32.SHCreateShellItemArrayFromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)dataobject, iid, out shellItemArray);
			return shellItemArray;
		}

		public static System.Windows.DragDropEffects ToDropEffect(this IDataObject dataObject) {
			var dragDropEffect = System.Windows.DragDropEffects.Copy;
			if (dataObject.GetDataPresent("Preferred DropEffect")) {
				object data = dataObject.GetData("Preferred DropEffect", true);

				if (data is System.IO.Stream) {
					var stream = (Stream)data;
					var reader = new StreamReader(stream);
					int value = reader.Read();
					stream.Position = 0; // This had no apparent effect

					if ((value & 2) == 2) {
						dragDropEffect = System.Windows.DragDropEffects.Move;
					}
					else {
						dragDropEffect = dragDropEffect = System.Windows.DragDropEffects.Copy;
					}
				}
			}
			return dragDropEffect;
		}

		/*
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ILCreateFromPath(string path);
		*/

		[DllImport("shell32.dll", CharSet = CharSet.None)]
		public static extern void ILFree(IntPtr pidl);

		[DllImport("shell32.dll", CharSet = CharSet.None)]
		public static extern int ILGetSize(IntPtr pidl);

		public static void Clear(this ConcurrentBag<Tuple<int, PROPERTYKEY, object>> bag) {
			Tuple<int, PROPERTYKEY, object> tmp = null;
			while (!bag.IsEmpty) {
				bag.TryTake(out tmp);
				if (tmp != null) tmp = null;
			}
		}

		public static MemoryStream CreateShellIDList(this IListItemEx[] items) {
			// first convert all files into pidls list
			int pos = 0;
			byte[][] pidls = new byte[items.Count()][];
			foreach (var item in items) {
				// Get pidl based on name
				IntPtr pidl = item.PIDL;
				int pidlSize = ILGetSize(pidl);
				// Copy over to our managed array
				pidls[pos] = new byte[pidlSize];
				Marshal.Copy(pidl, pidls[pos++], 0, pidlSize);
				ILFree(pidl);
			}

			// Determine where in CIDL we will start pumping PIDLs
			int pidlOffset = 4 * (items.Count() + 2);
			// Start the CIDL stream
			var memStream = new MemoryStream();
			var sw = new BinaryWriter(memStream);
			// Initialize CIDL with a count of files
			sw.Write(items.Count());
			// Calculate and write relative offsets of every pidl starting with root
			sw.Write(pidlOffset);
			pidlOffset += 4; // root is 4 bytes
			foreach (var pidl in pidls) {
				sw.Write(pidlOffset);
				pidlOffset += pidl.Length;
			}

			// Write the root pidl (0) followed by all pidls
			sw.Write(0);
			foreach (var pidl in pidls) sw.Write(pidl);
			// stream now contains the CIDL
			return memStream;
		}
	}
}
