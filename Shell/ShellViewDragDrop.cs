using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BExplorer.Shell
{
	/// <summary>
	/// Custom DragDrop handler class
	/// </summary>
	public class ShellViewDragDrop : IDropTarget
	{
		/// <summary>
		/// Constructor for the ShellView Drop Target
		/// </summary>
		/// <param name="oldTarget">The original IDropTarget hooked from the IExplorerBrowser interface</param>
		public ShellViewDragDrop()
		{
		}


		public void DragEnter(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj, int grfKeyState, System.Drawing.Point pt, ref int pdwEffect)
		{


			AsyncDataObject pADataObject = new AsyncDataObject(pDataObj);
			pADataObject.SetAsyncMode(true);
			pdwEffect = (int)DragDropEffects.Copy;
			//_oldDropTarget.DragEnter(pADataObject, grfKeyState, pt, ref pdwEffect);


		}


		public void DragOver(int grfKeyState, System.Drawing.Point pt, ref int pdwEffect)
		{
			pdwEffect = (int)DragDropEffects.Copy;
			//_oldDropTarget.DragOver(grfKeyState, pt, ref pdwEffect);

		}

		/// <summary>
		/// Drag Leave event for the custom IDroptarget
		/// </summary>
		public void DragLeave()
		{

			//_oldDropTarget.DragLeave();

		}

		public void Drop(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj, int grfKeyState, System.Drawing.Point pt, ref int pdwEffect)
		{
			AsyncDataObject pADataObject = new AsyncDataObject(pDataObj);
			pADataObject.SetAsyncMode(true);
			//_oldDropTarget.Drop(pADataObject, grfKeyState, pt, ref pdwEffect);
			pdwEffect = (int)DragDropEffects.Copy;
		}

		#region Old Code (May be reenabled when using old Listview style)
		//[StructLayout(LayoutKind.Sequential)]
		//struct POINT {
		//  public int x;
		//  public int y;
		//}

		//[StructLayout(LayoutKind.Sequential)]
		//struct LVHITTESTINFO {
		//  public POINT pt;
		//  public uint flags;
		//  public int iItem;
		//  public int iSubItem;
		//}

		//bool IsLeftClick = false;
		//[DllImport("User32.dll")]
		//private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref LVHITTESTINFO lParam);

		//int LastItemIndex = -1;
		//ShellObject[] ParseDrop(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj)
		//{
		//		List<ShellObject> result = new List<ShellObject>();
		//		FORMATETC format = new FORMATETC();
		//		STGMEDIUM medium = new STGMEDIUM();

		//		format.cfFormat = 15;
		//		format.dwAspect = DVASPECT.DVASPECT_CONTENT;
		//		format.lindex = 0;
		//		format.ptd = IntPtr.Zero;
		//		format.tymed = TYMED.TYMED_HGLOBAL;

		//		pDataObj.GetData(ref format, out medium);
		//		ExplorerBrowser.GlobalLock(medium.unionmember);

		//		try
		//		{
		//				ShellObject parentFolder = null;
		//				int count = Marshal.ReadInt32(medium.unionmember);
		//				int offset = 4;

		//				for (int n = 0; n <= count; ++n)
		//				{
		//						int pidlOffset = Marshal.ReadInt32(medium.unionmember, offset);
		//						int pidlAddress = (int)medium.unionmember + pidlOffset;

		//						if (n == 0)
		//						{
		//								parentFolder = ShellObjectFactory.Create(new IntPtr(pidlAddress));
		//						}
		//						else
		//						{
		//								result.Add(ShellObjectFactory.Create(ExplorerBrowser.CreateItemWithParent(GetIShellFolder(parentFolder.NativeShellItem), new IntPtr(pidlAddress))));
		//						}

		//						offset += 4;
		//				}
		//		}
		//		finally
		//		{
		//				Marshal.FreeHGlobal(medium.unionmember);
		//		}

		//		return result.ToArray();
		//}

		//private bool HitTest(IntPtr ShellViewHandle, Point hitPoint, out int row, out int column)
		//{
		//		// clear the output values
		//		row = column = -1;

		//		// set up some win32 api constant values
		//		const int LVM_FIRST = 0x1000;
		//		//const int LVM_SUBITEMHITTEST = (LVM_FIRST + 57);
		//		const int LVM_HITTEST = (LVM_FIRST + 18);

		//		const int LVHT_NOWHERE = 0x1;
		//		const int LVHT_ONITEMICON = 0x2;
		//		const int LVHT_ONITEMLABEL = 0x4;
		//		const int LVHT_ONITEMSTATEICON = 0x8;
		//		const int LVHT_EX_ONCONTENTS = 0x04000000;
		//		const int LVHT_ONITEM = (LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON | LVHT_EX_ONCONTENTS);

		//		// set up the return value
		//		bool hitLocationFound = false;

		//		// initialise a hittest information structure
		//		LVHITTESTINFO lvHitTestInfo = new LVHITTESTINFO();
		//		lvHitTestInfo.pt.x = hitPoint.X;
		//		lvHitTestInfo.pt.y = hitPoint.Y;

		//		// send the hittest message to find out where the click was
		//		if (SendMessage(ShellViewHandle, LVM_HITTEST, 0, ref lvHitTestInfo) != 0)
		//		{

		//				bool nowhere = ((lvHitTestInfo.flags & LVHT_NOWHERE) != 0);
		//				bool onItem = ((lvHitTestInfo.flags & LVHT_ONITEM) != 0);

		//				if (onItem && !nowhere)
		//				{
		//						row = lvHitTestInfo.iItem;
		//						column = lvHitTestInfo.iSubItem;
		//						hitLocationFound = true;
		//				}


		//		}
		//		else if (SendMessage(ShellViewHandle, LVM_FIRST, 0, ref lvHitTestInfo) != 0)
		//		{
		//				row = 0;
		//				hitLocationFound = true;
		//		}

		//		return hitLocationFound;
		//}
		//private const int LVM_FIRST = 0x1000;
		//private const int LVM_GETITEMCOUNT = LVM_FIRST + 4;
		//private const int LVM_GETITEM = LVM_FIRST + 75;
		//private const int LVIF_TEXT = 0x0001;
		//private const int LVIS_DROPHILITED = 0x0008;
		//private const int LVM_SETITEMSTATE = LVM_FIRST + 43;
		//const int LVIF_STATE = 0x0008;


		//[DllImport("user32.dll")]
		//private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam,
		//																				 IntPtr lParam);


		//[DllImport("BEH.dll", CallingConvention = CallingConvention.Cdecl)]
		//private static extern bool IsItemFolder(IFolderView2 view, int index);

		//[DllImport("BEH.dll", CallingConvention = CallingConvention.Cdecl)]
		//private static extern IntPtr GetItemName(IFolderView2 view, int index);



		//private DragDropEffects Effect;

		//void mi_Click(object sender, EventArgs e)
		//{
		//		Effect = DragDropEffects.Copy;
		//}


		//public void DoCopy(object Data)
		//{
		//		DropData DataDrop = (DropData)Data;

		//		if (DataDrop.DropList == null)
		//		{
		//				using (FileOperation fileOp = new FileOperation())
		//				{
		//						foreach (ShellObject item in DataDrop.ItemsForDrop)
		//						{

		//								string New_Name = "";
		//								if (Path.GetExtension(item.ParsingName) == "")
		//								{

		//										New_Name = item.GetDisplayName(DisplayNameType.Default);
		//								}
		//								else
		//								{
		//										New_Name = Path.GetFileName(item.ParsingName);
		//								}
		//								if (!File.Exists(DataDrop.PathForDrop))
		//								{
		//										fileOp.CopyItem(item.ParsingName, DataDrop.PathForDrop, New_Name);
		//								}

		//						}

		//						fileOp.PerformOperations();
		//				}
		//		}
		//		else
		//		{
		//				using (FileOperation fileOp = new FileOperation())
		//				{
		//						foreach (string item in DataDrop.DropList)
		//						{

		//								string New_Name = "";
		//								if (Path.GetExtension(item) == "")
		//								{
		//										ShellObject shi = ShellObject.FromParsingName(item);
		//										New_Name = shi.GetDisplayName(DisplayNameType.Default);
		//								}
		//								else
		//								{
		//										New_Name = Path.GetFileName(item);
		//								}
		//								if (!File.Exists(DataDrop.PathForDrop))
		//								{
		//										fileOp.CopyItem(item, DataDrop.PathForDrop, New_Name);
		//								}

		//						}

		//						fileOp.PerformOperations();
		//				}
		//		}
		//}

		//public void DoMove(object Data)
		//{
		//		DropData DataDrop = (DropData)Data;

		//		using (FileOperation fileOp = new FileOperation())
		//		{
		//				foreach (ShellObject item in DataDrop.ItemsForDrop)
		//				{

		//						string New_Name = "";
		//						if (Path.GetExtension(item.ParsingName) == "")
		//						{

		//								New_Name = item.GetDisplayName(DisplayNameType.Default);
		//						}
		//						else
		//						{
		//								New_Name = Path.GetFileName(item.ParsingName);
		//						}
		//						if (!File.Exists(DataDrop.PathForDrop))
		//						{
		//								fileOp.MoveItem(item.ParsingName, DataDrop.PathForDrop, New_Name);
		//						}

		//				}

		//				fileOp.PerformOperations();
		//		}
		//}

		//public IShellFolder GetIShellFolder(IShellItem sitem)
		//{
		//		IShellFolder result;
		//		((IShellItem2)sitem).BindToHandler(IntPtr.Zero,
		//																			 BHID.SFObject, typeof(IShellFolder).GUID, out result);
		//		return result;
		//}

		//private ShellObject[] ParseShellIDListArray(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj)
		//{
		//		List<ShellObject> result = new List<ShellObject>();
		//		System.Runtime.InteropServices.ComTypes.FORMATETC format = new System.Runtime.InteropServices.ComTypes.FORMATETC();
		//		System.Runtime.InteropServices.ComTypes.STGMEDIUM medium = new System.Runtime.InteropServices.ComTypes.STGMEDIUM();

		//		format.cfFormat = (short)ExplorerBrowser.RegisterClipboardFormat("Shell IDList Array");
		//		format.dwAspect = System.Runtime.InteropServices.ComTypes.DVASPECT.DVASPECT_CONTENT;
		//		format.lindex = 0;
		//		format.ptd = IntPtr.Zero;
		//		format.tymed = System.Runtime.InteropServices.ComTypes.TYMED.TYMED_HGLOBAL;

		//		pDataObj.GetData(ref format, out medium);
		//		ExplorerBrowser.GlobalLock(medium.unionmember);

		//		try
		//		{
		//				ShellObject parentFolder = null;
		//				int count = Marshal.ReadInt32(medium.unionmember);
		//				int offset = 4;

		//				for (int n = 0; n <= count; ++n)
		//				{
		//						int pidlOffset = Marshal.ReadInt32(medium.unionmember, offset);
		//						int pidlAddress = (int)medium.unionmember + pidlOffset;

		//						if (n == 0)
		//						{
		//								parentFolder = ShellObjectFactory.Create(new IntPtr(pidlAddress));
		//						}
		//						else
		//						{
		//								result.Add(ShellObjectFactory.Create(ExplorerBrowser.CreateItemWithParent(GetIShellFolder(parentFolder.NativeShellItem), new IntPtr(pidlAddress))));
		//						}

		//						offset += 4;
		//				}
		//		}
		//		finally
		//		{
		//				Marshal.FreeHGlobal(medium.unionmember);
		//		}

		//		return result.ToArray();
		//} 
		#endregion
	}
}
