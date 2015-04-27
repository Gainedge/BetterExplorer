using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {

	public static class InterfaceGuids {
		// IID GUID strings for relevant Shell COM interfaces.
		public const string IExplorerBrowser = "DFD3B6B5-C10C-4BE9-85F6-A66969F402F6";
		public const string IKnownFolderManager = "8BE2D872-86AA-4d47-B776-32CCA40C7018";
		public const string IFolderView = "cde725b0-ccc9-4519-917e-325d72fab4ce";
		public const string IFolderView2 = "1af3a467-214f-4298-908e-06b03e0b39f9";
		public const string IServiceProvider = "6d5140c1-7436-11ce-8034-00aa006009fa";
		public const string IExplorerPaneVisibility = "e07010ec-bc17-44c0-97b0-46c7c95b9edc";
		public const string IExplorerBrowserEvents = "361bbdc7-e6ee-4e13-be58-58e2240c810f";
		public const string IInputObject = "68284fAA-6A48-11D0-8c78-00C04fd918b4";
		public const string IShellView = "000214E3-0000-0000-C000-000000000046";
		public const string IDispatch = "00020400-0000-0000-C000-000000000046";
		public const string DShellFolderViewEvents = "62112AA2-EBE4-11cf-A5FB-0020AFE7292D";
		public const string IColumnManager = "d8ec27bb-3f3b-4042-b10a-4acfd924d453";
		public const string ICommDlgBrowser = "000214F1-0000-0000-C000-000000000046";
		public const string ICommDlgBrowser2 = "10339516-2894-11d2-9039-00C04F8EEB3E";
		public const string ICommDlgBrowser3 = "c8ad25a1-3294-41ee-8165-71174bd01c57";
		public const string IDataObject = "0000010E-0000-0000-C000-000000000046";
		public const string IShellFolderView = "37A378C0-F82D-11CE-AE65-08002B2E1262";
		public const string IShellBrowser = "000214e2-0000-0000-c000-000000000046";

		public const string IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
		public const string IShellItem2 = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";
		public const string IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
		public const string IShellLibrary = "11A66EFA-382E-451A-9234-1E0E12EF3085";
		public const string IThumbnailCache = "F676C15D-596A-4ce2-8234-33996F445DB1";
		public const string ISharedBitmap = "091162a4-bc96-411f-aae8-c5122cd03363";

		public const string IShellFolder = "000214E6-0000-0000-C000-000000000046";
		public const string IShellFolder2 = "93F2F68C-1D1B-11D3-A30E-00C04F79ABD1";
		public const string IEnumIDList = "000214F2-0000-0000-C000-000000000046";
		public const string IShellLinkW = "000214F9-0000-0000-C000-000000000046";
		public const string CShellLink = "00021401-0000-0000-C000-000000000046";
		public const string IKnownFolder = "3AA7AF7E-9B36-420c-A8E3-F77D4674A488";
		public const string KnownFolderManager = "4df0c730-df9d-4ae3-9153-aa6b82e9795a";
		public const string ComputerFolder = "0AC0837C-BBF8-452A-850D-79D08E667CA7";
		public const string Favorites = "1777F761-68AD-4D8A-87BD-30B759FA33DD";
		public const string Documents = "FDD39AD0-238F-46AF-ADB4-6C85480369C7";
		public const string Profile = "5E6C858F-0E22-4760-9AFE-EA3317B67173";
		public const string IPropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
		public const string IPropertyStoreCache = "3017056d-9a91-4e90-937d-746c72abbf4f";
		public const string IPropertyDescription = "6F79D558-3E96-4549-A1D1-7D75D2288814";
		public const string IPropertyDescription2 = "57D2EDED-5062-400E-B107-5DAE79FE57A6";
		public const string IPropertyDescriptionList = "1F9FC1D0-C39B-4B26-817F-011967D3440E";
		public const string IPropertyEnumType = "11E1FBF9-2D56-4A6B-8DB3-7CD193A471F2";
		public const string IPropertyEnumType2 = "9B6E051C-5DDD-4321-9070-FE2ACB55E794";
		public const string IPropertyEnumTypeList = "A99400F4-3D84-4557-94BA-1242FB2CC9A6";
		public const string IPropertyStoreCapabilities = "c8e2d566-186e-4d49-bf41-6909ead56acc";

		public const string FileOpenDialog = "DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7";
		public const string FileSaveDialog = "C0B4E2F3-BA21-4773-8DBA-335EC946EB8B";
		public const string ShellLibrary = "D9B3211D-E57F-4426-AAEF-30A806ADD397";
		public const string SearchFolderItemFactory = "14010e02-bbbd-41f0-88e3-eda371216584";
		public const string ConditionFactory = "E03E85B0-7BE3-4000-BA98-6C13DE9FA486";
		public const string QueryParserManager = "5088B39A-29B4-4d9d-8245-4EE289222F66";

		public const string ICondition = "0FC988D4-C935-4b97-A973-46282EA175C8";
		public const string ISearchFolderItemFactory = "a0ffbc28-5482-4366-be27-3e81e78e06c2";
		public const string IConditionFactory = "A5EFE073-B16F-474f-9F3E-9F8B497A3E08";
		public const string IRichChunk = "4FDEF69C-DBC9-454e-9910-B34F3C64B510";
		public const string IPersistStream = "00000109-0000-0000-C000-000000000046";
		public const string IPersist = "0000010c-0000-0000-C000-000000000046";
		public const string IEnumUnknown = "00000100-0000-0000-C000-000000000046";
		public const string IQuerySolution = "D6EBC66B-8921-4193-AFDD-A1789FB7FF57";
		public const string IQueryParser = "2EBDEE67-3505-43f8-9946-EA44ABC8E5B0";
		public const string IQueryParserManager = "A879E3C4-AF77-44fb-8F37-EBD1487CF920";


		public const string GenericLibrary = "5c4f28b5-f869-4e84-8e60-f11db97c5cc7";
		public const string DocumentsLibrary = "7d49d726-3c21-4f05-99aa-fdc2c9474656";
		public const string MusicLibrary = "94d6ddcc-4a68-4175-a374-bd584a510b78";
		public const string PicturesLibrary = "b3690e58-e961-423b-b687-386ebfd83239";
		public const string VideosLibrary = "5fa96407-7e77-483c-ac93-691d05850de8";

		public const string Libraries = "1B3EA5DC-B587-4786-B4EF-BD1DC332AEAE";

	}

	public static class WNM {
		const int NM_FIRST = 0;
		public const int NM_KILLFOCUS = (NM_FIRST - 8);
		public const int NM_CUSTOMDRAW = (NM_FIRST - 12);
		public const int NM_RCLICK = (NM_FIRST - 5);
		public const int NM_SETFOCUS = (NM_FIRST - 7);
		public const int NM_CLICK = (NM_FIRST - 2);

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
		public const int LVN_GETDISPINFOW = (LVN_FIRST - 77);
		public const int LVN_SETDISPINFOA = (LVN_FIRST - 51);
		public const int LVN_BEGINSCROLL = (LVN_FIRST - 80);


		public const int LVN_ENDSCROLL = (LVN_FIRST - 81);
		public const int LVN_COLUMNCLICK = (LVN_FIRST - 8);
		public const int LVN_INCREMENTALSEARCH = (LVN_FIRST - 63);
		public const int LVN_ODFINDITEM = (LVN_FIRST - 79);
		public const int LVN_ENDLABELEDITW = (LVN_FIRST - 76);
		public const int LVN_GROUPINFO = (LVN_FIRST - 88);


		const int UDN_FIRST = -721;        // updown
		public const int UDN_DELTAPOS = (UDN_FIRST - 1);
	}

	public static class LVM {
		const int FIRST = 0x1000;							// LVM_FIRST
		public const int SETIMAGELIST = (FIRST + 3);		// LVM_SETIMAGELIST
		public const int GETNEXTITEM = (FIRST + 12);		// LVM_GETNEXTITEM
		public const int GETNEXTITEMINDEX = (FIRST + 211);
		public const int GETITEMRECT = (FIRST + 14);		// LVM_GETITEMRECT
		public const int HITTEST = (FIRST + 18);			// LVM_HITTEST
		public const int REDRAWITEMS = (FIRST + 21);		// LVM_REDRAWITEMS
		public const int GETEDITCONTROL = (FIRST + 24);		// LVM_GETEDITCONTROL
		public const int GETHEADER = (FIRST + 31);			// LVM_GETHEADER
		public const int GETITEMSTATE = (FIRST + 44);		// LVM_GETITEMSTATE
		public const int GETITEMSPACING = (FIRST + 51);		// LVM_GETITEMSPACING
		public const int SETEXTENDEDLISTVIEWSTYLE = (FIRST + 54);		// LVM_SETEXTENDEDLISTVIEWSTYLE
		public const int GETEXTENDEDLISTVIEWSTYLE = (FIRST + 55);		// LVM_GETEXTENDEDLISTVIEWSTYLE
		public const int GETSUBITEMRECT = (FIRST + 56);		// LVM_GETSUBITEMRECT
		public const int GETHOTITEM = (FIRST + 61);			// LVM_GETHOTITEM
		public const int GETITEMW = (FIRST + 75);			// LVM_GETITEMW
		public const int GETSTRINGWIDTHW = (FIRST + 87);	// LVM_GETSTRINGWIDTHW	
		public const int SETBKIMAGEW = (FIRST + 138);		// LVM_SETBKIMAGEW
		public const int GETVIEW = (FIRST + 143);			// LVM_GETVIEW
		public const int SETGROUPINFO = (FIRST + 147);		// LVM_SETGROUPINFO
		public const int GETSELECTEDCOLUMN = (FIRST + 174);	// LVM_GETSELECTEDCOLUMN
		public const int SETCOLUMNWIDTH = (FIRST + 30);		// LVM_SETCOLUMNWIDTH
		public const int GETCOLUMNWIDTH = (FIRST + 29);		// LVM_GETCOLUMNWIDTH
		public const int ENSUREVISIBLE = (FIRST + 19);
		public const int ISITEMVISIBLE = (FIRST + 182);
		public const int GETTOOLTIPS = (FIRST + 78);
		public const int SETINFOTIP = (FIRST + 173);

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

	[StructLayout(LayoutKind.Sequential)]
	public struct HDITEM {
		public Mask mask;
		public int cxy;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszText;
		public IntPtr hbm;
		public int cchTextMax;
		public Format fmt;
		public IntPtr lParam;
		// _WIN32_IE >= 0x0300 
		public int iImage;
		public int iOrder;
		// _WIN32_IE >= 0x0500
		public uint type;
		public IntPtr pvFilter;
		// _WIN32_WINNT >= 0x0600
		public uint state;

		[Flags]
		public enum Mask {
			Format = 0x4,       // HDI_FORMAT

			
			//HDI_WIDTH = 0x1 //Written By Aaron Campf
		}

		[Flags]
		public enum Format {
			SortDown = 0x200,   // HDF_SORTDOWN
			SortUp = 0x400,     // HDF_SORTUP
			HDF_SPLITBUTTON = 0x01000000,
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct NativeFolderDefinition {
		internal FolderCategory category;
		internal IntPtr name;
		internal IntPtr description;
		internal Guid parentId;
		internal IntPtr relativePath;
		internal IntPtr parsingName;
		internal IntPtr tooltip;
		internal IntPtr localizedName;
		internal IntPtr icon;
		internal IntPtr security;
		internal UInt32 attributes;
		internal DefinitionOptions definitionOptions;
		internal Guid folderTypeId;
	}

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct NMITEMACTIVATE {
		public NMHDR hdr;
		public int iItem;
		public int iSubItem;
		public int uNewState;
		public int uOldState;
		public int uChanged;
		public Point ptAction;
		public IntPtr lParam;
		public int uKeyFlags;
	}
	*/

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct NMHDDISPINFO {
		public NMHDR hdr;
		public int iItem;
		public uint mask;
		public string pszText;
		public int cchTextMax;
		public int iImage;
		public IntPtr lParam;
	}
	*/

	public static class CustomDraw {
		public const int CDRF_DODEFAULT = 0x00000000;
		public const int CDRF_NEWFONT = 0x00000002;
		public const int CDRF_SKIPDEFAULT = 0x00000004;
		public const int CDRF_DOERASE = 0x00000008; // draw the background
		public const int CDRF_NOTIFYPOSTPAINT = 0x00000010;
		public const int CDRF_NOTIFYITEMDRAW = 0x00000020;
		public const int CDRF_NOTIFYSUBITEMDRAW = 0x00000020;

		public const int CDDS_PREPAINT = 0x00000001;
		public const int CDDS_POSTPAINT = 0x00000002;
		public const int CDDS_ITEM = 0x00010000;
		public const int CDDS_SUBITEM = 0x00020000;
		public const int CDDS_ITEMPREPAINT = 0x00010001;
		public const int CDDS_ITEMPOSTPAINT = 0x00010002;
		public const int NM_CUSTOMDRAW = (-12);
	}
}
