using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell
{
	/// <summary>
	/// These values indicate what is the state of the group. These values
	/// are taken directly from the SDK and many are not used by ObjectListView.
	/// </summary>
	[Flags]
	public enum GroupState
	{
		/// <summary>
		/// Normal
		/// </summary>
		LVGS_NORMAL = 0x0,

		/// <summary>
		/// Collapsed
		/// </summary>
		LVGS_COLLAPSED = 0x1,

		/// <summary>
		/// Hidden
		/// </summary>
		LVGS_HIDDEN = 0x2,

		/// <summary>
		/// NoHeader
		/// </summary>
		LVGS_NOHEADER = 0x4,

		/// <summary>
		/// Can be collapsed
		/// </summary>
		LVGS_COLLAPSIBLE = 0x8,

		/// <summary>
		/// Has focus
		/// </summary>
		LVGS_FOCUSED = 0x10,

		/// <summary>
		/// Is Selected
		/// </summary>
		LVGS_SELECTED = 0x20,

		/// <summary>
		/// Is subsetted
		/// </summary>
		LVGS_SUBSETED = 0x40,

		/// <summary>
		/// Subset link has focus
		/// </summary>
		LVGS_SUBSETLINKFOCUSED = 0x80,

		/// <summary>
		/// All styles
		/// </summary>
		LVGS_ALL = 0xFFFF
	}

	/// <summary>
	/// This mask indicates which members of a LVGROUP have valid data. These values
	/// are taken directly from the SDK and many are not used by ObjectListView.
	/// </summary>
	[Flags]
	public enum GroupMask
	{
		/// <summary>
		/// No mask
		/// </summary>
		LVGF_NONE = 0,

		/// <summary>
		/// Group has header
		/// </summary>
		LVGF_HEADER = 1,

		/// <summary>
		/// Group has footer
		/// </summary>
		LVGF_FOOTER = 2,

		/// <summary>
		/// Group has state
		/// </summary>
		LVGF_STATE = 4,

		/// <summary>
		/// 
		/// </summary>
		LVGF_ALIGN = 8,

		/// <summary>
		/// 
		/// </summary>
		LVGF_GROUPID = 0x10,

		/// <summary>
		/// pszSubtitle is valid
		/// </summary>
		LVGF_SUBTITLE = 0x00100,

		/// <summary>
		/// pszTask is valid
		/// </summary>
		LVGF_TASK = 0x00200,

		/// <summary>
		/// pszDescriptionTop is valid
		/// </summary>
		LVGF_DESCRIPTIONTOP = 0x00400,

		/// <summary>
		/// pszDescriptionBottom is valid
		/// </summary>
		LVGF_DESCRIPTIONBOTTOM = 0x00800,

		/// <summary>
		/// iTitleImage is valid
		/// </summary>
		LVGF_TITLEIMAGE = 0x01000,

		/// <summary>
		/// iExtendedImage is valid
		/// </summary>
		LVGF_EXTENDEDIMAGE = 0x02000,

		/// <summary>
		/// iFirstItem and cItems are valid
		/// </summary>
		LVGF_ITEMS = 0x04000,

		/// <summary>
		/// pszSubsetTitle is valid
		/// </summary>
		LVGF_SUBSET = 0x08000,

		/// <summary>
		/// readonly, cItems holds count of items in visible subset, iFirstItem is valid
		/// </summary>
		LVGF_SUBSETITEMS = 0x10000
	}

	/*
	/// <summary>
	/// This mask indicates which members of a GROUPMETRICS structure are valid
	/// </summary>
	[Flags]
	public enum GroupMetricsMask
	{
		/// <summary>
		/// 
		/// </summary>
		LVGMF_NONE = 0,

		/// <summary>
		/// 
		/// </summary>
		LVGMF_BORDERSIZE = 1,

		/// <summary>
		/// 
		/// </summary>
		LVGMF_BORDERCOLOR = 2,

		/// <summary>
		/// 
		/// </summary>
		LVGMF_TEXTCOLOR = 4
	}
	*/

	/*
	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct LVGROUP
	{
		public uint cbSize;
		public uint mask;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszHeader;
		public int cchHeader;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszFooter;
		public int cchFooter;
		public int iGroupId;
		public uint stateMask;
		public uint state;
		public uint uAlign;
	}
	*/

	[StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct LVGROUP2
	{
		public uint cbSize;
		public uint mask;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszHeader;
		public uint cchHeader;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszFooter;
		public int cchFooter;
		public int iGroupId;
		public uint stateMask;
		public uint state;
		public uint uAlign;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszSubtitle;
		public uint cchSubtitle;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszTask;
		public uint cchTask;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszDescriptionTop;
		public uint cchDescriptionTop;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszDescriptionBottom;
		public uint cchDescriptionBottom;
		public int iTitleImage;
		public int iExtendedImage;
		public int iFirstItem;         // Read only
		public int cItems;             // Read only
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pszSubsetTitle;     // NULL if group is not subset
		public uint cchSubsetTitle;
	}
}
