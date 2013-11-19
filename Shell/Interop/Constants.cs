using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GongSolutions.Shell.Interop
{
	internal static class InterfaceGuids
	{
		// IID GUID strings for relevant Shell COM interfaces.
		internal const string IExplorerBrowser = "DFD3B6B5-C10C-4BE9-85F6-A66969F402F6";
		internal const string IKnownFolderManager = "8BE2D872-86AA-4d47-B776-32CCA40C7018";
		internal const string IFolderView = "cde725b0-ccc9-4519-917e-325d72fab4ce";
		internal const string IFolderView2 = "1af3a467-214f-4298-908e-06b03e0b39f9";
		internal const string IServiceProvider = "6d5140c1-7436-11ce-8034-00aa006009fa";
		internal const string IExplorerPaneVisibility = "e07010ec-bc17-44c0-97b0-46c7c95b9edc";
		internal const string IExplorerBrowserEvents = "361bbdc7-e6ee-4e13-be58-58e2240c810f";
		internal const string IInputObject = "68284fAA-6A48-11D0-8c78-00C04fd918b4";
		internal const string IShellView = "000214E3-0000-0000-C000-000000000046";
		internal const string IDispatch = "00020400-0000-0000-C000-000000000046";
		internal const string DShellFolderViewEvents = "62112AA2-EBE4-11cf-A5FB-0020AFE7292D";
		internal const string IColumnManager = "d8ec27bb-3f3b-4042-b10a-4acfd924d453";
		internal const string ICommDlgBrowser = "000214F1-0000-0000-C000-000000000046";
		internal const string ICommDlgBrowser2 = "10339516-2894-11d2-9039-00C04F8EEB3E";
		internal const string ICommDlgBrowser3 = "c8ad25a1-3294-41ee-8165-71174bd01c57";
		internal const string IDataObject = "0000010E-0000-0000-C000-000000000046";
		internal const string IShellFolderView = "37A378C0-F82D-11CE-AE65-08002B2E1262";
		internal const string IShellBrowser = "000214e2-0000-0000-c000-000000000046";

	}

	internal static class WM
	{
		internal const int WM_CONTEXTMENU = 0x007B;
	}

	internal static class WNM
	{
		const int NM_FIRST = 0;
		public const int NM_KILLFOCUS = (NM_FIRST - 8);
		public const int NM_CUSTOMDRAW = (NM_FIRST - 12);

		const int TTN_FIRST = -520;
		public const int TTN_SHOW = (TTN_FIRST - 1);
		public const int TTN_GETDISPINFOW = (TTN_FIRST - 10);

		const int RBN_FIRST = -831;
		public const int RBN_HEIGHTCHANGE = (RBN_FIRST - 0);
		public const int RBN_BEGINDRAG = (RBN_FIRST - 4);
		public const int RBN_ENDDRAG = (RBN_FIRST - 5);

		const int LVN_FIRST = -100;
		public const int LVN_ITEMCHANGED = (LVN_FIRST - 1);
		public const int LVN_DELETEITEM = (LVN_FIRST - 3);
		public const int LVN_BEGINDRAG = (LVN_FIRST - 9);
		public const int LVN_BEGINRDRAG = (LVN_FIRST - 11);
		public const int LVN_ITEMACTIVATE = (LVN_FIRST - 14);
		public const int LVN_ODSTATECHANGED = (LVN_FIRST - 15);
		public const int LVN_HOTTRACK = (LVN_FIRST - 21);
		public const int LVN_KEYDOWN = (LVN_FIRST - 55);
		public const int LVN_GETINFOTIP = (LVN_FIRST - 58);

		const int UDN_FIRST = -721;        // updown
		public const int UDN_DELTAPOS = (UDN_FIRST - 1);
	}

	public static class LVM
	{
		const int FIRST = 0x1000;				// LVM_FIRST
		public const int SETIMAGELIST = (FIRST + 3);		// LVM_SETIMAGELIST
		public const int GETNEXTITEM = (FIRST + 12);		// LVM_GETNEXTITEM
		public const int GETITEMRECT = (FIRST + 14);		// LVM_GETITEMRECT
		public const int HITTEST = (FIRST + 18);		// LVM_HITTEST
		public const int REDRAWITEMS = (FIRST + 21);		// LVM_REDRAWITEMS
		public const int GETEDITCONTROL = (FIRST + 24);		// LVM_GETEDITCONTROL
		public const int GETHEADER = (FIRST + 31);		// LVM_GETHEADER
		public const int GETITEMSTATE = (FIRST + 44);		// LVM_GETITEMSTATE
		public const int GETITEMSPACING = (FIRST + 51);		// LVM_GETITEMSPACING
		public const int SETEXTENDEDLISTVIEWSTYLE = (FIRST + 54);		// LVM_SETEXTENDEDLISTVIEWSTYLE
		public const int GETEXTENDEDLISTVIEWSTYLE = (FIRST + 55);		// LVM_GETEXTENDEDLISTVIEWSTYLE
		public const int GETSUBITEMRECT = (FIRST + 56);		// LVM_GETSUBITEMRECT
		public const int GETHOTITEM = (FIRST + 61);		// LVM_GETHOTITEM
		public const int GETITEMW = (FIRST + 75);		// LVM_GETITEMW
		public const int GETSTRINGWIDTHW = (FIRST + 87);		// LVM_GETSTRINGWIDTHW	
		public const int SETBKIMAGEW = (FIRST + 138);		// LVM_SETBKIMAGEW
		public const int GETVIEW = (FIRST + 143);		// LVM_GETVIEW
		public const int SETGROUPINFO = (FIRST + 147);		// LVM_SETGROUPINFO
		public const int GETSELECTEDCOLUMN = (FIRST + 174);		// LVM_GETSELECTEDCOLUMN
		public const int SETCOLUMNWIDTH = (FIRST + 30);		// LVM_SETCOLUMNWIDTH
		public const int GETCOLUMNWIDTH = (FIRST + 29);		// LVM_GETCOLUMNWIDTH
		public const int ENSUREVISIBLE = (FIRST + 19);
		public const int ISITEMVISIBLE = (FIRST + 182);

		public const int LVIR_BOUNDS = 0;
		public const int LVIR_ICON = 1;
		public const int LVIR_LABEL = 2;
		public const int LV_VIEW_ICON = 0x0000;
		public const int LV_VIEW_DETAILS = 0x0001;
		public const int LV_VIEW_SMALLICON = 0x0002;
		public const int LV_VIEW_LIST = 0x0003;
		public const int LV_VIEW_TILE = 0x0004;
		public const int LVIF_TEXT = 0x00000001;
		public const int LVIF_STATE = 0x00000008;
		public const int LVNI_SELECTED = 0x0002;
		public const int LVSIL_GROUPHEADER = 3;
		public const int LVGF_STATE = 0x00000004;
		public const int LVGF_TITLEIMAGE = 0x00001000;
		public const int LVGS_COLLAPSED = 0x00000001;
		public const int LVGS_COLLAPSIBLE = 0x00000008;

		// -> LVIS
		//public const int LVIS_FOCUSED		= 0x0001;
		//public const int LVIS_SELECTED		= 0x0002;
		//public const int LVIS_CUT			= 0x0004;
		//public const int LVIS_DROPHILITED	= 0x0008;

		public const int LVS_EX_FULLROWSELECT = 0x00000020;
		//public const int LVS_EX_TRANSPARENTBKGND = 0x00400000; // Background is painted by the parent via WM_PRINTCLIENT
		//public const int LVS_EX_TRANSPARENTSHADOWTEXT = 0x00800000;  // Enable shadow text on transparent backgrounds only (useful with bitmaps)
	}
}
