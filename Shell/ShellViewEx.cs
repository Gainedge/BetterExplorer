using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Microsoft.Test.Tools.WicCop.InteropServices.ComTypes;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Interop;
using System.Collections.Concurrent;

namespace BExplorer.Shell
{
	/// <summary>
	/// Specifies how list items are displayed in a <see cref="ShellView"/> 
	/// control. 
	/// </summary>
	public enum ShellViewStyle
	{
		ExtraLargeIcon,
		LargeIcon,
		/// <summary>
		/// Each item appears as a full-sized icon with a label below it. 
		/// </summary>
		Medium,

		/// <summary>
		/// Each item appears as a small icon with a label to its right. 
		/// </summary>
		SmallIcon,

		/// <summary>
		/// Each item appears as a small icon with a label to its right. 
		/// Items are arranged in columns with no column headers. 
		/// </summary>
		List,

		/// <summary>
		/// Each item appears on a separate line with further information 
		/// about each item arranged in columns. The left-most column 
		/// contains a small icon and label. 
		/// </summary>
		Details,

		/// <summary>
		/// Each item appears with a thumbnail picture of the file's content.
		/// </summary>
		Thumbnail,

		/// <summary>
		/// Each item appears as a full-sized icon with the item label and 
		/// file information to the right of it. 
		/// </summary>
		Tile,

		/// <summary>
		/// Each item appears in a thumbstrip at the bottom of the control,
		/// with a large preview of the seleted item appearing above.
		/// </summary>
		Thumbstrip,
		/// <summary>
		/// Each item appears in a item that occupy the whole view width
		/// </summary>
		Content,


	}

	public class NavigatedEventArgs : EventArgs
	{
		ShellItem m_Folder;
		public NavigatedEventArgs(ShellItem folder)
		{
			m_Folder = folder;
		}
		/// <summary>
		/// The folder that is navigated to.
		/// </summary>
		public ShellItem Folder
		{
			get { return m_Folder; }
			set { m_Folder = value; }
		}
	}

	/// <summary>
	/// Provides information for the <see cref="ShellView.Navigating"/>
	/// event.
	/// </summary>
	public class NavigatingEventArgs : EventArgs
	{
		bool m_Cancel;

		ShellItem m_Folder;

		/// <summary>
		/// Initializes a new instance of the <see cref="NavigatingEventArgs"/>
		/// class.
		/// </summary>
		/// 
		/// <param name="folder">
		/// The folder being navigated to.
		/// </param>
		public NavigatingEventArgs(ShellItem folder)
		{
			m_Folder = folder;
		}

		/// <summary>
		/// Gets/sets a value indicating whether the navigation should be
		/// cancelled.
		/// </summary>
		public bool Cancel
		{
			get { return m_Cancel; }
			set { m_Cancel = value; }
		}

		/// <summary>
		/// The folder being navigated to.
		/// </summary>
		public ShellItem Folder
		{
			get { return m_Folder; }
			set { m_Folder = value; }
		}
	}

	public enum ItemUpdateType
	{
		Renamed,
		Created,
		Deleted,
		Updated
	}

	public class ItemUpdatedEventArgs : EventArgs
	{
		public ItemUpdateType UpdateType { get; private set;}
		public ShellItem PreviousItem { get; private set; }
		public ShellItem NewItem { get; private set; }

		public int NewItemIndex { get; private set; }
		public ItemUpdatedEventArgs(ItemUpdateType type, ShellItem newItem, ShellItem previousItem, int index)
		{
			this.UpdateType = type;
			this.NewItem = newItem;
			this.PreviousItem = previousItem;
			this.NewItemIndex = index;
		}
	}

	public partial class ShellView : UserControl
	{

		public List<Collumns> AllAvailableColumns = new List<Collumns>();
		public List<Collumns> Collumns = new List<Collumns>();
		public ShellNotifications Notifications = new ShellNotifications();
		int _iconSize;
		private Boolean _showCheckBoxes = false;
		private Boolean _ShowHidden;
		BackgroundWorker bw = new BackgroundWorker();
		ConcurrentDictionary<int, Bitmap> cache = new ConcurrentDictionary<int, Bitmap>();
		Thread CacheLoad;
		Boolean Cancel = false;
		Bitmap ExeFallBack16;
		Bitmap ExeFallBack256;
		Bitmap ExeFallBack32;
		Bitmap ExeFallBack48;
		ImageList extra = new ImageList(ImageListSize.ExtraLarge);
		List<int> IndexesWithThumbnail = new List<int>();
		bool IsDoubleNavFinished = false;
		ImageList jumbo = new ImageList(ImageListSize.Jumbo);
		ImageList large = new ImageList(ImageListSize.Large);
		ShellItem m_CurrentFolder;
		ShellHistory m_History;
		ShellViewStyle m_View;
		SyncQueue<int> overlayQueue = new SyncQueue<int>(3000);
		Dictionary<int, int> overlays = new Dictionary<int, int>();
		Thread overlaysThread;
		Thread overlaysThread2;
		System.Windows.Forms.Timer selectionTimer = new System.Windows.Forms.Timer();
		Dictionary<int, int> shieldedIcons = new Dictionary<int, int>();
		SyncQueue<int> shieldQueue = new SyncQueue<int>(3000);
		ImageList small = new ImageList(ImageListSize.SystemSmall);
		Thread thumb;

		SyncQueue<int> ThumbnailsForCacheLoad = new SyncQueue<int>(5000);

		CancellationToken token;

		CancellationTokenSource tokenSource = new CancellationTokenSource();

		SyncQueue<int> waitingThumbnails = new SyncQueue<int>(3000);

		public ShellView()
		{
			InitializeComponent();
			this.Items = new List<ShellItem>();
			this.LVItemsColorCodes = new List<LVItemColor>();

			thumb = new Thread(LoadIcon);
			thumb.IsBackground = true;
			thumb.Priority = ThreadPriority.AboveNormal;
			thumb.Start();
			CacheLoad = new Thread(LoadCacheIcon);
			CacheLoad.IsBackground = true;
			CacheLoad.Priority = ThreadPriority.AboveNormal;
			CacheLoad.Start();
			overlaysThread = new Thread(LoadOverlay);
			overlaysThread.IsBackground = true;
			overlaysThread.Priority = ThreadPriority.BelowNormal;
			overlaysThread.Start();
			overlaysThread2 = new Thread(LoadShield);
			overlaysThread2.IsBackground = true;
			overlaysThread2.Priority = ThreadPriority.BelowNormal;
			overlaysThread2.Start();
			m_History = new ShellHistory();
			token = tokenSource.Token;

			Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO();
			defIconInfo.cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO));

			Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_APPLICATION, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
			ExeFallBack48 = extra.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			ExeFallBack256 = jumbo.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();
			ExeFallBack16 = small.GetIcon(defIconInfo.iSysIconIndex).ToBitmap();

			//SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		}

		/// <summary>
		/// Occurs when the control gains focus
		/// </summary>
		public event EventHandler GotFocus;

		/// <summary>
		/// Occurs when the control loses focus
		/// </summary>
		public event EventHandler LostFocus;

		/// <summary>
		/// Occurs when the <see cref="ShellView"/> control navigates to a 
		/// new folder.
		/// </summary>
		public event EventHandler<NavigatedEventArgs> Navigated;

		/// <summary>
		/// Occurs when the <see cref="ShellView"/>'s current selection 
		/// changes.
		/// </summary>
		/// 
		/// <remarks>
		/// <b>Important:</b> When <see cref="ShowWebView"/> is set to 
		/// <see langref="true"/>, this event will not occur. This is due to 
		/// a limitation in the underlying windows control.
		/// </remarks>
		public event EventHandler SelectionChanged;

		public event EventHandler<ItemUpdatedEventArgs> ItemUpdated;

		public event EventHandler<ViewChangedEventArgs> ViewStyleChanged;

		/// <summary>
		/// Gets a value indicating whether a previous page in navigation 
		/// history is available, which allows the <see cref="NavigateBack"/> 
		/// method to succeed. 
		/// </summary>
		[Browsable(false)]
		public bool CanNavigateBack
		{
			get { return m_History.CanNavigateBack; }
		}

		/// <summary>
		/// Gets a value indicating whether a subsequent page in navigation 
		/// history is available, which allows the <see cref="NavigateForward"/> 
		/// method to succeed. 
		/// </summary>
		[Browsable(false)]
		public bool CanNavigateForward
		{
			get { return m_History.CanNavigateForward; }
		}

		/// <summary>
		/// Gets a value indicating whether the folder currently being browsed
		/// by the <see cref="ShellView"/> has parent folder which can be
		/// navigated to by calling <see cref="NavigateParent"/>.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanNavigateParent
		{
			get
			{
				return m_CurrentFolder != ShellItem.Desktop;
			}
		}

		/// <summary>
		/// Gets/sets a <see cref="ShellItem"/> describing the folder 
		/// currently being browsed by the <see cref="ShellView"/>.
		/// </summary>
		[Browsable(false)]
		public ShellItem CurrentFolder
		{
			get { return m_CurrentFolder; }
			set
			{
				//if (value != m_CurrentFolder)
				//{
				//	Navigate(value);
				//}
				m_CurrentFolder = value;
			}
		}

		// <summary>
		/// Gets the <see cref="ShellView"/>'s navigation history.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ShellHistory History
		{
			get { return m_History; }
		}

		public int IconSize
		{
			get
			{
				return _iconSize;
			}
		}

		public List<ShellItem> Items { get; set; }

		public int LastSortedColumnIndex { get; set; }

		public SortOrder LastSortOrder { get; set; }

		public IntPtr LVHandle { get; set; }

		public List<LVItemColor> LVItemsColorCodes { get; set; }
		public List<ShellItem> SelectedItems
		{
			get
			{
				List<ShellItem> selItems = new List<ShellItem>();
				int index = -2;
				int iStart = -1;

				while (index != -1)
				{
					index = User32.SendMessage(this.LVHandle, LVM.GETNEXTITEM, iStart, LVNI.LVNI_SELECTED);
					iStart = index;
					if (index != -1)
					{
						selItems.Add(this.Items[index]);
					}
				}

				return selItems;

			}
		}

		public Boolean ShowCheckboxes
		{
			get{
				return _showCheckBoxes;
			}
			set
			{
				if (value)
				{
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.CheckBoxes, (int)ListViewExtendedStyles.CheckBoxes);
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT);
				}
				else
				{
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.CheckBoxes, 0);
					User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_AUTOCHECKSELECT, 0);
				}
				_showCheckBoxes = value;
			}
		}
		public Boolean ShowHidden
		{
			get
			{
				return _ShowHidden;
			}
			set
			{
				_ShowHidden = value;
				this.RefreshContents();
			}
		}
		/// <summary>
		/// Gets or sets how items are displayed in the control. 
		/// </summary>
		[DefaultValue(ShellViewStyle.Medium), Category("Appearance")]
		public ShellViewStyle View
		{
			get { return m_View; }
			set
			{
				m_View = value;
				var iconsize = 16;
				switch (value)
				{
					case ShellViewStyle.ExtraLargeIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(256);
						iconsize = 256;
						break;
					case ShellViewStyle.LargeIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(96);
						iconsize = 96;
						break;
					case ShellViewStyle.Medium:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_ICON, 0);
						ResizeIcons(48);
						iconsize = 48;
						break;
					case ShellViewStyle.SmallIcon:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_SMALLICON, 0);
						ResizeIcons(16);
						iconsize = 16;
						break;
					case ShellViewStyle.List:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_LIST, 0);
						ResizeIcons(16);
						iconsize = 16;
						break;
					case ShellViewStyle.Details:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_DETAILS, 0);
						ResizeIcons(16);
						iconsize = 16;
						break;
					case ShellViewStyle.Thumbnail:
						break;
					case ShellViewStyle.Tile:
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETVIEW, (int)LV_VIEW.LV_VIEW_TILE, 0);
						ResizeIcons(48);
						iconsize = 48;
						//LVTILEVIEWINFO tvi = new LVTILEVIEWINFO();
						//tvi.cbSize = Marshal.SizeOf(typeof(LVTILEVIEWINFO));
						//tvi.dwMask = (int)LVTVIM.LVTVIM_COLUMNS | (int)LVTVIM.LVTVIM_TILESIZE;
						//tvi.dwFlags = (int)LVTVIF.LVTVIF_AUTOSIZE;
						//tvi.cLines = 4;
						//var a = User32.SendMessage(this.LVHandle, (int)BExplorer.Shell.Interop.MSG.LVM_SETTILEVIEWINFO, 0, tvi);
						break;
					case ShellViewStyle.Thumbstrip:
						break;
					case ShellViewStyle.Content:
						break;
					default:
						break;
				}
				OnViewChanged(new ViewChangedEventArgs(value, iconsize));
				//OnNavigated(new NavigatedEventArgs(ShellItem.Desktop));
			}
		}

		/// <summary>
		/// Runs an application as an another user.
		/// </summary>
		/// <param name="ExePath">The path of the application.</param>
		/// <param name="username">The path of the username to use.</param>
		public static void RunExeAsAnotherUser(string ExePath, string username)
		{
			var psi = new ProcessStartInfo
			{
				FileName = ExePath,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = String.Format("/env /user:{0} \"{1}\"", username, ExePath),
			};
			Process.Start(psi);

		}

		public static void StartCompartabilityWizzard()
		{
			Process.Start("msdt.exe", "-id PCWDiagnostic");
		}

		public void CleanupDrive()
		{
			string DriveLetter = "";
			if (SelectedItems.Count > 0)
			{
				if (Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName))
				{
					DriveLetter = SelectedItems[0].ParsingName;
				}
				else
				{
					DriveLetter = this.CurrentFolder.ParsingName;
				}
			}
			else
			{
				DriveLetter = this.CurrentFolder.ParsingName;
			}
			Process.Start("Cleanmgr.exe", "/d" + DriveLetter.Replace(":\\", ""));
		}

		public string CreateNewFolder()
		{

			string name = "New Folder";
			int suffix = 0;

			do
			{
				if (this.CurrentFolder.Parent != null)
				{
					if (this.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName)
					{
						ShellLibrary lib =
								ShellLibrary.Load(this.CurrentFolder.DisplayName, true);
						name = String.Format("{0}\\New Folder ({1})",
								lib.DefaultSaveFolder, ++suffix);
						lib.Close();
					}
					else
						name = String.Format("{0}\\New Folder ({1})",
								this.CurrentFolder.ParsingName, ++suffix);
				}
				else
					name = String.Format("{0}\\New Folder ({1})",
							this.CurrentFolder.ParsingName, ++suffix);
			} while (Directory.Exists(name) || File.Exists(name));

			ERROR result = Shell32.SHCreateDirectory(IntPtr.Zero, name);

			switch (result)
			{
				case ERROR.FILE_EXISTS:
				case ERROR.ALREADY_EXISTS:
					throw new IOException("The directory already exists");
				case ERROR.BAD_PATHNAME:
					throw new IOException("Bad pathname");
				case ERROR.FILENAME_EXCED_RANGE:
					throw new IOException("The filename is too long");
			}
			return name;
		}

		public string CreateNewFolder(string name)
		{
			int suffix = 0;
			string endname = name;

			do
			{
				if (this.CurrentFolder.Parent != null)
				{
					if (this.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName)
					{
						ShellLibrary lib =
								ShellLibrary.Load(this.CurrentFolder.DisplayName, true);
						endname = String.Format("{0}\\" + name + " ({1})",
								lib.DefaultSaveFolder, ++suffix);
						lib.Close();
					}
					else
						endname = String.Format("{0}\\" + name + " ({1})",
								this.CurrentFolder.ParsingName, ++suffix);
				}
				else
					endname = String.Format("{0}\\" + name + " ({1})",
							this.CurrentFolder.ParsingName, ++suffix);
			} while (Directory.Exists(endname) || File.Exists(endname));

			ERROR result = Shell32.SHCreateDirectory(IntPtr.Zero, endname);

			switch (result)
			{
				case ERROR.FILE_EXISTS:
				case ERROR.ALREADY_EXISTS:
					throw new IOException("The directory already exists");
				case ERROR.BAD_PATHNAME:
					throw new IOException("Bad pathname");
				case ERROR.FILENAME_EXCED_RANGE:
					throw new IOException("The filename is too long");
			}
			return endname;
		}

		public ShellLibrary CreateNewLibrary()
		{
			//      string name = "New Library";
			//      int suffix = 0;
			//      ShellLibrary lib = null;
			//      try
			//      {
			//              lib = ShellLibrary.Load(name, true);
			//      }
			//      catch
			//      {
			////TODO: add log here
			//      }
			//      if (lib != null)
			//      {
			//          do
			//          {
			//              name = String.Format("New Library ({0})",
			//                      ++suffix);
			//              try
			//              {
			//                      lib = ShellLibrary.Load(name, true);
			//              }
			//              catch
			//              {
			//                      lib = null;
			//              }
			//          } while (lib != null); 
			//      }

			//      ShellLibrary libcreate = new ShellLibrary(name, false);

			//      return libcreate.GetDisplayName(DisplayNameType.Default);
			return CreateNewLibrary("New Library");
		}

		public ShellLibrary CreateNewLibrary(string name)
		{
			string endname = name;
			int suffix = 0;
			ShellLibrary lib = null;
			try
			{
				lib = ShellLibrary.Load(endname, true);
			}
			catch
			{

			}
			if (lib != null)
			{
				do
				{
					endname = String.Format(name + "({0})",
							++suffix);
					try
					{
						lib = ShellLibrary.Load(endname, true);
					}
					catch
					{
						lib = null;
					}
				} while (lib != null);
			}

			return new ShellLibrary(endname, false);

			//return libcreate.GetDisplayName(DisplayNameType.Default);
		}
		public void SetLVBackgroundImage(Bitmap bitmap)
		{
			Helpers.SetListViewBackgroundImage(this.LVHandle, bitmap);
		}
		public HResult SetFolderIcon(string wszPath, string wszExpandedIconPath, int iIcon)
		{
			HResult hr;

			Shell32.LPSHFOLDERCUSTOMSETTINGS fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS();
			fcs.dwSize = (uint)Marshal.SizeOf(fcs);
			fcs.dwMask = Shell32.FCSM_ICONFILE;
			fcs.pszIconFile = wszExpandedIconPath.Replace(@"\\", @"\");
			fcs.cchIconFile = 0;
			fcs.iIconIndex = iIcon;

			// Set the folder icon
			hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath.Replace(@"\\", @"\"), Shell32.FCS_FORCEWRITE);

			if (hr == HResult.S_OK)
			{
				// Update the icon cache
				SHFILEINFO sfi = new SHFILEINFO();
				Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(wszPath.Replace(@"\\", @"\")), 0, out sfi, (int)Marshal.SizeOf(sfi), SHGFI.ICONLOCATION);
				int iIconIndex = Shell32.Shell_GetCachedImageIndex(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0);
				Shell32.SHUpdateImage(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0, iIconIndex);
				//RefreshExplorer();
				Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEIMAGE,
				Shell32.HChangeNotifyFlags.SHCNF_DWORD | Shell32.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, IntPtr.Zero,
					(IntPtr)sfi.iIcon);
			}

			this.RefreshItem(Items.IndexOf(Items.Single(s => s.ParsingName == wszPath)));
			return hr;
		}

		public HResult ClearFolderIcon(string wszPath)
		{
			HResult hr;

			Shell32.LPSHFOLDERCUSTOMSETTINGS fcs = new Shell32.LPSHFOLDERCUSTOMSETTINGS();
			fcs.dwSize = (uint)Marshal.SizeOf(fcs);
			fcs.dwMask = Shell32.FCSM_ICONFILE;
			hr = Shell32.SHGetSetFolderCustomSettings(ref fcs, wszPath, Shell32.FCS_FORCEWRITE);
			if (hr == HResult.S_OK)
			{
				// Update the icon cache
				SHFILEINFO sfi = new SHFILEINFO();
				Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(wszPath.Replace(@"\\", @"\")), 0, out sfi, (int)Marshal.SizeOf(sfi), SHGFI.ICONLOCATION);
				int iIconIndex = Shell32.Shell_GetCachedImageIndex(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0);
				Shell32.SHUpdateImage(sfi.szDisplayName.Replace(@"\\", @"\"), sfi.iIcon, 0, iIconIndex);
				Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEIMAGE,
				Shell32.HChangeNotifyFlags.SHCNF_DWORD | Shell32.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, IntPtr.Zero,
					(IntPtr)sfi.iIcon);
			}
			this.RefreshItem(Items.IndexOf(Items.Single(s => s.ParsingName == wszPath)));
			return hr;
		}

		public void DefragDrive()
		{
			string DriveLetter = "";
			if (SelectedItems.Count > 0)
			{
				if (Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName))
				{
					DriveLetter = SelectedItems[0].ParsingName;
				}
				else
				{
					DriveLetter = this.CurrentFolder.ParsingName;
				}
			}
			else
			{
				DriveLetter = this.CurrentFolder.ParsingName;
			}
			Process.Start(Path.Combine(Environment.SystemDirectory, "dfrgui.exe"), "/u /v " + DriveLetter.Replace("\\", ""));
		}

		public void DeSelectAllItems()
		{
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED;
			item.state = 0;
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, -1, ref item);
		}

		/// <summary>
		/// Gives the ShellListView focus
		/// </summary>
		public void Focus()
		{
			User32.SetFocus(this.LVHandle);
		}
		public void FormatDrive(IntPtr handle)
		{
			string DriveLetter = "";
			if (SelectedItems.Count > 0)
			{
				if (Directory.GetLogicalDrives().Contains(SelectedItems[0].ParsingName))
				{
					DriveLetter = SelectedItems[0].ParsingName;
				}
				else
				{
					DriveLetter = this.CurrentFolder.ParsingName;
				}
			}
			else
			{
				DriveLetter = this.CurrentFolder.ParsingName;
			}
			Shell32.FormatDrive(handle, DriveLetter);
		}

		public int GetItemsCount()
		{
			return this.Items.Count;
		}

		public int GetSelectedCount()
		{
			return (int)User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETSELECTEDCOUNT, 0, 0);
		}

		public void InvertSelection()
		{
			int itemCount = (int)User32.SendMessage(this.LVHandle,
								BExplorer.Shell.Interop.MSG.LVM_GETITEMCOUNT, 0, 0);

			for (int n = 0; n < itemCount; ++n)
			{

				var state = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE,
						n, LVIS.LVIS_SELECTED);

				LVITEM item_new = new LVITEM();
				item_new.mask = LVIF.LVIF_STATE;
				item_new.stateMask = LVIS.LVIS_SELECTED;
				item_new.state = (state & LVIS.LVIS_SELECTED) == LVIS.LVIS_SELECTED ? 0 : LVIS.LVIS_SELECTED;
				User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, n, ref item_new);
			}
		}

		public async void LoadCacheIcon()
		{
			while (true)
			{

				try
				{
					//Application.DoEvents();

					var index = ThumbnailsForCacheLoad.Dequeue();
					if (this.Cancel)
						continue;
					var sho = Items[index];
					var thumb = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.Default);
					if (thumb != null)
					{
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
						thumb.Dispose();
						thumb = null;
					}
					Thread.Sleep(2);
				}
				catch (Exception)
				{

				}
			}
		}

		public async void LoadIcon()
		{
			while (true)
			{

				//Application.DoEvents();

				try
				{
					var index = waitingThumbnails.Dequeue();
					if (!this.Cancel)
					{
						var sho = Items[index];
						var icon = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
						if (icon != null)
						{
							if (!cache.ContainsKey(index))
							{
								cache.TryAdd(index, new Bitmap(icon));
								User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
								icon.Dispose();
								icon = null;
							}
							else
							{
								cache[index] = new Bitmap(icon);
								User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
								icon.Dispose();
								icon = null;
							}
						}
					}
					Thread.Sleep(1);
				}
				catch (Exception)
				{

				}
			}
		}

		public void LoadOverlay()
		{
			while (true)
			{
				//Application.DoEvents();
				Thread.Sleep(3);
				//while (overlayQueue.Count == 0)
				//{
				//	Thread.Sleep(5);
				//}

				try
				{
					var index = overlayQueue.Dequeue();
					if (this.Cancel)
						continue;
					//if (this.Cancel)
					//	continue;
					//Application.DoEvents();
					ShellItem sho = null;
					var shoTemp = Items[index];
					if (shoTemp.ParsingName.StartsWith("::"))
					{
						sho = shoTemp;
					}
					else
					{
						sho = new ShellItem(shoTemp.ParsingName);
					}
					int hash = shoTemp.GetHashCode();

					int overlayIndex = 0;
					small.GetIconIndexWithOverlay(sho.Pidl, out overlayIndex);
					if (!overlays.ContainsKey(hash))
					{
						overlays.Add(hash, overlayIndex);
					}
					if (overlayIndex > 0)
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
				}
				catch (Exception)
				{

				}
			}
		}

		public void LoadShield()
		{
			while (true)
			{
				//Application.DoEvents();
				Thread.Sleep(4);

				//while (shieldQueue.Count == 0)
				//{
				//	Thread.Sleep(5);
				//}

				try
				{
					var index = shieldQueue.Dequeue();
					if (this.Cancel)
						continue;
					//Application.DoEvents();
					ShellItem sho = null;
					var shoTemp = Items[index];
					if (shoTemp.ParsingName.StartsWith("::"))
					{
						sho = shoTemp;
					}
					else
					{
						sho = new ShellItem(shoTemp.ParsingName);
					}
					int hash = shoTemp.GetHashCode();

					var shieldOverlay = 0;
					if ((sho.GetShield() & IExtractIconpwFlags.GIL_SHIELD) != 0)
					{
						Shell32.SHSTOCKICONINFO defIconInfo = new Shell32.SHSTOCKICONINFO();
						defIconInfo.cbSize = (uint)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO));
						Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_SHIELD, Shell32.SHGSI.SHGSI_SYSICONINDEX, ref defIconInfo);
						shieldOverlay = defIconInfo.iSysIconIndex;
					}

					if (!shieldedIcons.ContainsKey(hash))
						shieldedIcons.Add(hash, shieldOverlay);
					if (shieldOverlay > 0)
					{
						User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
					}
				}
				catch (Exception)
				{

				}
			}
		}

		public async void Navigate(ShellItem destination)
		{
			////Application.DoEvents();
			this.Notifications.UnregisterChangeNotify();
			overlays.Clear();
			shieldedIcons.Clear();
			cache.Clear();
			Items.Clear();
			waitingThumbnails.Clear();
			overlayQueue.Clear();
			shieldQueue.Clear();
			this.Cancel = true;
			this.cache.Clear();

			Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, 0, 0);

			//Task.Run(() =>
			//{
			//	var array = destination.ToArray();
			//	IsDoubleNavFinished = true;
			//});
			//if (!bw.IsBusy)
			//{
			//	bw.RunWorkerAsync(destination);
			//}

			var pc = new ProgressContext<ShellItem>(destination);
			pc.UpdateProgress += pc_UpdateProgress;
			this.Items = pc.Where(w => this.ShowHidden ? true : w.IsHidden == false).OrderByDescending(o => o.IsFolder).ToList();
			//this.Items = destination.OrderByDescending(o => o.IsFolder).ToArray();

			this.Cancel = false;
			this.LastSortedColumnIndex = 0;
			this.LastSortOrder = SortOrder.Ascending;
			this.SetSortIcon(this.LastSortedColumnIndex, this.LastSortOrder);
			this.m_CurrentFolder = destination;
			Notifications.RegisterChangeNotify(this.Handle, destination, true);
			try
			{
				m_History.Add(destination);
			}
			catch { }

			this.OnNavigated(new NavigatedEventArgs(destination));
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);


			//this.Items = items.Select(s => new ShellItem(s.ToShellParsingName())).OrderByDescending(o => o.IsFolder).ToArray();
			
			IsDoubleNavFinished = false;

		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the previous folder 
		/// in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a <b>Back</b> button similar to the one in 
		/// Windows Explorer, which will allow your users to return to a 
		/// previous folder in the navigation history. 
		/// </para>
		/// 
		/// <para>
		/// Use the <see cref="CanNavigateBack"/> property to determine whether 
		/// the navigation history is available and contains a previous page. 
		/// This property is useful, for example, to change the enabled state 
		/// of a Back button when the ShellView control navigates to or leaves 
		/// the beginning of the navigation history.
		/// </para>
		/// </remarks>
		/// 
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate backwards through.
		/// </exception>
		public void NavigateBack()
		{
			m_CurrentFolder = m_History.MoveBack();
			//RecreateShellView();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control backwards to the 
		/// requested folder in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateBack"/>
		/// method to implement a drop-down menu on a <b>Back</b> button similar 
		/// to the one in Windows Explorer, which will allow your users to return 
		/// to a previous folder in the navigation history. 
		/// </remarks>
		/// 
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		/// 
		/// <exception cref="Exception">
		/// The requested folder is not present in the 
		/// <see cref="ShellView"/>'s 'back' history.
		/// </exception>
		public void NavigateBack(ShellItem folder)
		{
			m_History.MoveBack(folder);
			m_CurrentFolder = folder;
			//RecreateShellView();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control to the next folder 
		/// in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// <para>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the <see cref="NavigateForward"/> 
		/// method to implement a <b>Forward</b> button similar to the one 
		/// in Windows Explorer, allowing your users to return to the next 
		/// folder in the navigation history after navigating backward.
		/// </para>
		/// 
		/// <para>
		/// Use the <see cref="CanNavigateForward"/> property to determine 
		/// whether the navigation history is available and contains a folder 
		/// located after the current one.  This property is useful, for 
		/// example, to change the enabled state of a <b>Forward</b> button 
		/// when the ShellView control navigates to or leaves the end of the 
		/// navigation history.
		/// </para>
		/// </remarks>
		/// 
		/// <exception cref="InvalidOperationException">
		/// There is no history to navigate forwards through.
		/// </exception>
		public void NavigateForward()
		{
			m_CurrentFolder = m_History.MoveForward();
			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates the <see cref="ShellView"/> control forwards to the 
		/// requested folder in the navigation history. 
		/// </summary>
		/// 
		/// <remarks>
		/// The WebBrowser control maintains a history list of all the folders
		/// visited during a session. You can use the 
		/// <see cref="NavigateForward"/> method to implement a drop-down menu 
		/// on a <b>Forward</b> button similar to the one in Windows Explorer, 
		/// which will allow your users to return to a folder in the 'forward'
		/// navigation history. 
		/// </remarks>
		/// 
		/// <param name="folder">
		/// The folder to navigate to.
		/// </param>
		/// 
		/// <exception cref="Exception">
		/// The requested folder is not present in the 
		/// <see cref="ShellView"/>'s 'forward' history.
		/// </exception>
		public void NavigateForward(ShellItem folder)
		{
			m_History.MoveForward(folder);
			m_CurrentFolder = folder;

			//OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
		}

		/// <summary>
		/// Navigates to the parent of the currently displayed folder.
		/// </summary>
		public void NavigateParent()
		{
			Navigate(m_CurrentFolder.Parent);
		}

		public void RefreshContents()
		{
			Navigate(this.CurrentFolder);
		}

		public void RefreshItem(int index)
		{
			if (cache.ContainsKey(index))
			{
				Bitmap bmp = null;
				if (cache.TryRemove(index, out bmp))
				{
					bmp.Dispose();
					bmp = null;
				}
			}
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_REDRAWITEMS, index, index);
		}

		public void RenameItem(int index)
		{
			this.Focus();
			var res = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_EDITLABELW, index, 0);
		}
		public void RenameSelectedItem()
		{
			//User32.EnumChildWindows(this.LVHandle, RenameCallback, IntPtr.Zero);
			RenameCallback(this.LVHandle, IntPtr.Zero);
		}

		public void ResizeIcons(int value)
		{
			try
			{
				_iconSize = value;
				this.Cancel = true;
				cache.Clear();
				waitingThumbnails.Clear();

				//Task.Run(() =>
				//{
				//	for (int i = 0; i < Items.Count(); i++)
				//	{
				//		if (Items[i].ThumbnailIcon != null)
				//		{
				//			Items[i].ThumbnailIcon.Dispose();
				//			Items[i].ThumbnailIcon = null;
				//			//GC.Collect();
				//		}
				//	}
				//	//GC.Collect();
				//});
				//GC.Collect();
				//this.cache.Clear();
				System.Windows.Forms.ImageList il = new System.Windows.Forms.ImageList();
				il.ImageSize = new System.Drawing.Size(value, value);
				System.Windows.Forms.ImageList ils = new System.Windows.Forms.ImageList();
				ils.ImageSize = new System.Drawing.Size(16, 16);
				User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
				User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);
				User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETICONSPACING, 0, (IntPtr)User32.MAKELONG(value + 28, value + 42));
				this.Cancel = false;
			}
			catch (Exception)
			{

			}

		}

		/// <summary>
		/// Runs an application as an administrator.
		/// </summary>
		/// <param name="ExePath">The path of the application.</param>
		public void RunExeAsAdmin(string ExePath)
		{
			var psi = new ProcessStartInfo
			{
				FileName = ExePath,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = String.Format("/env /user:Administrator \"{0}\"", ExePath),
			};
			Process.Start(psi);

		}

		public void SelectAll()
		{
			LVITEM item = new LVITEM();
			item.mask = LVIF.LVIF_STATE;
			item.stateMask = LVIS.LVIS_SELECTED;
			item.state = LVIS.LVIS_SELECTED;
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, -1, ref item);
		}

		public void SelectItems(ShellItem[] ShellObjectArray)
		{
			foreach (ShellItem item in ShellObjectArray)
			{
				LVITEM lvi = new LVITEM();
				lvi.mask = LVIF.LVIF_STATE;
				lvi.stateMask = LVIS.LVIS_SELECTED;
				lvi.state = LVIS.LVIS_SELECTED;
				User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMSTATE, Items.IndexOf(item), ref lvi);
			}
			this.Focus();
		}

		public void SetSortCollumn(int colIndex, SortOrder Order)
		{
			if (colIndex == this.LastSortedColumnIndex)
			{
				// Reverse the current sort direction for this column.
				if (this.LastSortOrder == SortOrder.Ascending)
				{
					this.LastSortOrder = SortOrder.Descending;
				}
				else
				{
					this.LastSortOrder = SortOrder.Ascending;
				}
			}
			else
			{
				// Set the column number that is to be sorted; default to ascending.
				this.LastSortedColumnIndex = colIndex;
				this.LastSortOrder = Order;
			}

			if (Order == SortOrder.Ascending)
			{
				this.Items = this.Items.OrderByDescending(o => o.IsFolder).ThenBy(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToList();
			}
			else
			{
				this.Items = this.Items.OrderByDescending(o => o.IsFolder).ThenByDescending(o => o.GetPropertyValue(this.Collumns[colIndex].pkey, typeof(String)).Value).ToList();
			}
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
			this.SetSortIcon(colIndex, Order);
		}

		internal void OnGotFocus()
		{
			if (GotFocus != null)
			{
				GotFocus(this, EventArgs.Empty);
			}
		}

		internal void OnLostFocus()
		{
			if (LostFocus != null)
			{
				LostFocus(this, EventArgs.Empty);
			}
		}

		internal void OnSelectionChanged()
		{

			if (SelectionChanged != null)
			{
				SelectionChanged(this, EventArgs.Empty);
			}

		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			System.Windows.Forms.ImageList il = new System.Windows.Forms.ImageList();
			il.ImageSize = new System.Drawing.Size(48, 48);
			System.Windows.Forms.ImageList ils = new System.Windows.Forms.ImageList();
			ils.ImageSize = new System.Drawing.Size(16, 16);
			
			ComCtl32.INITCOMMONCONTROLSEX icc = new ComCtl32.INITCOMMONCONTROLSEX();
			icc.dwSize = Marshal.SizeOf(typeof(ComCtl32.INITCOMMONCONTROLSEX));
			icc.dwICC = 1;
			var res = ComCtl32.InitCommonControlsEx(ref icc);
			this.LVHandle = User32.CreateWindowEx(0, "SysListView32", "", User32.WindowStyles.WS_CHILD | User32.WindowStyles.WS_CLIPCHILDREN | User32.WindowStyles.WS_CLIPSIBLINGS | 
																																		(User32.WindowStyles)User32.LVS_EDITLABELS | (User32.WindowStyles)User32.LVS_OWNERDATA | (User32.WindowStyles)User32.LVS_SHOWSELALWAYS, 
																																		0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, this.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			User32.ShowWindow(this.LVHandle, User32.ShowWindowCommands.Show);


			this.AddDefaultColumns();

			
			IntPtr headerhandle = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETHEADER, 0, 0);

			for (int i = 0; i < this.Collumns.Count; i++)
			{
				this.Collumns[i].SetSplitButton(headerhandle, i);
			}
			
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 0, il.Handle);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETIMAGELIST, 1, ils.Handle);
			UxTheme.SetWindowTheme(this.LVHandle, "Explorer", 0);

			this.View = ShellViewStyle.Medium;

			//Navigate((ShellItem)KnownFolders.Desktop);
			

			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.HeaderInAllViews, (int)ListViewExtendedStyles.HeaderInAllViews);
			//WinAPI.SendMessage(handle, WinAPI.LVM.LVM_SetExtendedStyle, (int)WinAPI.ListViewExtendedStyles.LVS_EX_AUTOAUTOARRANGE, (int)WinAPI.ListViewExtendedStyles.LVS_EX_AUTOAUTOARRANGE);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER, (int)ListViewExtendedStyles.LVS_EX_DOUBLEBUFFER);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.FullRowSelect, (int)ListViewExtendedStyles.FullRowSelect);
			User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SetExtendedStyle, (int)ListViewExtendedStyles.HeaderDragDrop, (int)ListViewExtendedStyles.HeaderDragDrop);
			CurrentFolder = (ShellItem)KnownFolders.Desktop;

		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			//this.Invalidate();
			//this.Refresh();
			User32.MoveWindow(this.LVHandle, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, true);
		}

		protected override void WndProc(ref Message m)
		{
			bool isSmallIcons = (View == ShellViewStyle.List || View == ShellViewStyle.SmallIcon || View == ShellViewStyle.Details);

			if (m.Msg == ShellNotifications.WM_SHNOTIFY)
			{
				if (Notifications.NotificationReceipt(m.WParam, m.LParam))
				{
					//var info = (NotifyInfos)Notifications.NotificationsReceived[Notifications.NotificationsReceived.Count - 1];
					foreach (NotifyInfos info in Notifications.NotificationsReceived.ToArray())
					{


						if (info.Notification == ShellNotifications.SHCNE.SHCNE_CREATE || info.Notification == ShellNotifications.SHCNE.SHCNE_MKDIR)
						{
							var obj = new ShellItem(info.Item1);
							if (Items.Count(w => w.ParsingName == obj.ParsingName) == 0 && !String.IsNullOrEmpty(obj.ParsingName))
							{
								Items.Add(obj);
							}
							User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
							if (this.ItemUpdated != null)
								this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Created, obj, null, this.Items.Count - 1));
							Notifications.NotificationsReceived.Remove(info);
						}
						if (info.Notification == ShellNotifications.SHCNE.SHCNE_DELETE || info.Notification == ShellNotifications.SHCNE.SHCNE_RMDIR)
						{
							var obj = new ShellItem(info.Item1);
							if (!String.IsNullOrEmpty(obj.ParsingName))
							{
								Items.Remove(Items.Single(s => s.ParsingName == obj.ParsingName));
								User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Count, 0);
								if (this.ItemUpdated != null)
									this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Deleted, obj, null, -1));
								Notifications.NotificationsReceived.Remove(info);
							}
						}

						if (info.Notification == ShellNotifications.SHCNE.SHCNE_RENAMEFOLDER || info.Notification == ShellNotifications.SHCNE.SHCNE_RENAMEITEM)
						{
							var obj1 = new ShellItem(info.Item1);
							var obj2 = new ShellItem(info.Item2);
							if (!String.IsNullOrEmpty(obj1.ParsingName) && !String.IsNullOrEmpty(obj2.ParsingName))
							{
								int itemIndex = Items.IndexOf(Items.Single(s => s.ParsingName == obj1.ParsingName));


								Items[itemIndex] = obj2;
								User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_UPDATE, itemIndex, 0);
								if (this.ItemUpdated != null)
									this.ItemUpdated.Invoke(this, new ItemUpdatedEventArgs(ItemUpdateType.Renamed, obj2, obj1, itemIndex));
								Notifications.NotificationsReceived.Remove(info);
							}
						}
					}
				}

			}

			base.WndProc(ref m);
			if (m.Msg == 78)
			{
				NMHDR nmhdr = new NMHDR();
				nmhdr = (NMHDR)m.GetLParam(nmhdr.GetType());
				switch ((int)nmhdr.code)
				{
					case WNM.LVN_GETDISPINFOW:
						var nmlv = new NMLVDISPINFO();
						nmlv = (NMLVDISPINFO)m.GetLParam(nmlv.GetType());
						//if ((nmlv.item.mask & LVIF.LVIF_COLUMNS) == LVIF.LVIF_COLUMNS)
						//{
						//	int[] varArray = {0,1,2,3};
						//	IntPtr ptr = Marshal.AllocHGlobal(varArray.Length*Marshal.SizeOf(varArray[0]));
						//	Marshal.Copy(varArray,0,ptr, varArray.Length);
						//	nmlv.item.cColumns = varArray.Length;
						//	nmlv.item.puColumns = (uint)ptr;
						//	Marshal.StructureToPtr(nmlv, m.LParam, false);
						//}
						if ((nmlv.item.mask & LVIF.LVIF_TEXT) == LVIF.LVIF_TEXT)
						{
							if (nmlv.item.iSubItem == 0)
							{
								var currentItem = Items[nmlv.item.iItem];
								nmlv.item.pszText = this.View == ShellViewStyle.Tile ? String.Empty : currentItem.DisplayName;
								Marshal.StructureToPtr(nmlv, m.LParam, false);
							}
							else
							{
								if (isSmallIcons)
								{
									var currentItem = Items[nmlv.item.iItem];
									IShellItem2 isi2 = (IShellItem2)currentItem.m_ComInterface;
									Collumns currentCollumn = this.Collumns[nmlv.item.iSubItem];
									PROPERTYKEY pk = currentCollumn.pkey;
									PropVariant pvar = new PropVariant();
									if (isi2.GetProperty(ref pk, pvar) == HResult.S_OK)
									{
										String value = String.Empty;
										if (pvar.Value != null)
										{
											if (currentCollumn.CollumnType == typeof(DateTime))
											{

												value = ((DateTime)pvar.Value).ToString(Thread.CurrentThread.CurrentCulture);
											}
											else if (currentCollumn.CollumnType == typeof(long))
											{
												value = String.Format("{0} KB", (Math.Ceiling(Convert.ToDouble(pvar.Value.ToString()) / 1024).ToString("# ### ### ##0"))); //ShlWapi.StrFormatByteSize(Convert.ToInt64(pvar.Value.ToString()));
											}
											else
											{
												value = pvar.Value.ToString();
											}
										}
										nmlv.item.pszText = value;
										Marshal.StructureToPtr(nmlv, m.LParam, false);
									}
									pvar.Dispose();
								}
							}
						}

						break;
					case WNM.LVN_COLUMNCLICK:
						NMLISTVIEW nlcv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
						SetSortCollumn(nlcv.iSubItem, this.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending);
						break;
					case WNM.LVN_ITEMACTIVATE:
						var iac = new NMITEMACTIVATE();
						iac = (NMITEMACTIVATE)m.GetLParam(iac.GetType());
						ShellItem selectedItem = Items[iac.iItem];
						if (selectedItem.IsFolder)
							Navigate(selectedItem);
						else
							Process.Start(selectedItem.ParsingName);
						break;
					case WNM.LVN_BEGINSCROLL:

						this.Cancel = true;
						cache.Clear();
						//waitingThumbnails.Clear();
						ThumbnailsForCacheLoad.Clear();
						overlayQueue.Clear();
						shieldQueue.Clear();

						//shieldedIcons.Clear();
						//overlays.Clear();
						GC.Collect();
						break;
					case WNM.LVN_ENDSCROLL:
						this.Cancel = false;
						Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
						break;
					case WNM.LVN_ITEMCHANGED:
						NMLISTVIEW nlv = (NMLISTVIEW)m.GetLParam(typeof(NMLISTVIEW));
						if ((nlv.uChanged & LVIF.LVIF_STATE) == LVIF.LVIF_STATE)
						{
							selectionTimer.Interval = 100;
							selectionTimer.Tick += selectionTimer_Tick;
							if (selectionTimer.Enabled)
							{
								selectionTimer.Stop();
								selectionTimer.Start();
							}
							else
							{
								selectionTimer.Start();
							}
						}

						break;
					case WNM.LVN_ODSTATECHANGED:
						OnSelectionChanged();
						break;
					case WNM.LVN_KEYDOWN:
						NMLVKEYDOWN nkd = (NMLVKEYDOWN)m.GetLParam(typeof(NMLVKEYDOWN));
						switch (nkd.wVKey)
						{
							case (short)Keys.F2:
								RenameSelectedItem();
								break;
							case (short)Keys.Enter:
								if (SelectedItems[0].IsFolder)
									Navigate(SelectedItems[0]);
								else
									Process.Start(SelectedItems[0].ParsingName);
								break;
							default:
								break;
						}
						break;
					case WNM.NM_RCLICK:
						var selitems = this.SelectedItems;
						NMITEMACTIVATE itemActivate = (NMITEMACTIVATE)m.GetLParam(typeof(NMITEMACTIVATE));
						ShellContextMenu cm = new ShellContextMenu(selitems.ToArray());
						cm.ShowContextMenu(this, itemActivate.ptAction, CMF.CANRENAME);
						break;
					case WNM.NM_CLICK:
						break;
					case WNM.NM_SETFOCUS:
						OnGotFocus();
						break;
					case WNM.NM_KILLFOCUS:
						OnLostFocus();
						break;
					case CustomDraw.NM_CUSTOMDRAW:
						{
							if (nmhdr.hwndFrom == this.LVHandle)
							{
								User32.SendMessage(this.LVHandle, 296, User32.MAKELONG(1, 1), 0);
								var nmlvcd = new NMLVCUSTOMDRAW();
								nmlvcd = (NMLVCUSTOMDRAW)m.GetLParam(nmlvcd.GetType());
								var index = (int)nmlvcd.nmcd.dwItemSpec;
								var hdc = nmlvcd.nmcd.hdc;
								ShellItem sho = null;
								try
								{
									sho = Items[index];
								}
								catch (Exception)
								{
									//! Index  is outside of bounds
								}

								System.Drawing.Color? textColor = null;
								if (sho != null)
								{
									if (this.LVItemsColorCodes != null && this.LVItemsColorCodes.Count > 0)
									{
										if (!String.IsNullOrEmpty(sho.Extension))
										{
											var extItemsAvailable = this.LVItemsColorCodes.Where(c => c.ExtensionList.Contains(sho.Extension)).Count() > 0;
											if (extItemsAvailable)
											{
												var color = this.LVItemsColorCodes.Where(c => c.ExtensionList.ToLowerInvariant().Contains(sho.Extension)).Select(c => c.TextColor).SingleOrDefault();
												textColor = color;
											}
										}
									}
								}
								switch (nmlvcd.nmcd.dwDrawStage)
								{
									case CustomDraw.CDDS_PREPAINT:
										m.Result = (IntPtr)CustomDraw.CDRF_NOTIFYITEMDRAW;
										break;
									case CustomDraw.CDDS_ITEMPREPAINT:
										if (textColor != null)
										{
											nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);

											m.Result = (IntPtr)(CustomDraw.CDRF_NEWFONT | CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW);
										}
										else
										{
											m.Result = (IntPtr)(CustomDraw.CDRF_NOTIFYPOSTPAINT | CustomDraw.CDRF_NOTIFYSUBITEMDRAW);
										}
										break;
									case CustomDraw.CDDS_ITEMPREPAINT | CustomDraw.CDDS_SUBITEM:
										if (textColor != null)
										{
											nmlvcd.clrText = ColorTranslator.ToWin32(textColor.Value);
											Marshal.StructureToPtr(nmlvcd, m.LParam, false);
											m.Result = (IntPtr)CustomDraw.CDRF_NEWFONT;
										}
										else
										{
											m.Result = (IntPtr)CustomDraw.CDRF_DODEFAULT;
										}
										break;
									case CustomDraw.CDDS_ITEMPOSTPAINT:
										if (nmlvcd.clrTextBk != 0 && nmlvcd.clrFace != 0 && nmlvcd.clrText != 0)
										{
											var itemBounds = new User32.RECT();
											User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMRECT, index, ref itemBounds);

											var iconBounds = new User32.RECT();

											iconBounds.Left = 1;

											User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMRECT, index, ref iconBounds);
											var hash = -1;

											if (sho != null)
											{
												var overlayIndex = 0;
												var shieleded = 0;



												hash = sho.GetHashCode();
												string shoExtension = sho.Extension;

												if (!overlays.TryGetValue(hash, out overlayIndex))
												{
													overlayQueue.Enqueue(index);
												}
												if (!shieldedIcons.TryGetValue(hash, out shieleded))
												{
													if (shoExtension == ".exe" || shoExtension == ".com" || shoExtension == ".bat")
														shieldQueue.Enqueue(index);
												}

												var thumbnail = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.CacheOnly);
												if (thumbnail != null && IconSize != 16)
												{
													if (((thumbnail.Width > thumbnail.Height && thumbnail.Width != IconSize) || (thumbnail.Width < thumbnail.Height && thumbnail.Height != IconSize)))
													{
														ThumbnailsForCacheLoad.Enqueue(index);
													}
													else
													{
														using (Graphics g = Graphics.FromHdc(hdc))
														{

															if (sho.IsHidden)
																thumbnail = Helpers.ChangeOpacity(thumbnail, 0.5f);
															g.DrawImageUnscaled(thumbnail, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - thumbnail.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - thumbnail.Height) / 2, thumbnail.Width, thumbnail.Height));

															if (this.ShowCheckboxes && View != ShellViewStyle.Details && View != ShellViewStyle.List)
															{
																var nItemState = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_STATEIMAGEMASK);

																if ((int)User32.SendMessage(this.LVHandle, LVM.GETHOTITEM, 0, 0) == index || (int)nItemState == (2 << 12))
																{
																	var lvis = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_SELECTED);
																	var checkboxOffsetH = 14;
																	var checkboxOffsetV = 2;
																	if (View == ShellViewStyle.Tile || View == ShellViewStyle.SmallIcon)
																		checkboxOffsetH = 2;
																	if (View == ShellViewStyle.Tile)
																		checkboxOffsetV = 1;

																	if (lvis != 0)
																	{
																		CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
																	}
																	else
																	{
																		CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
																	}
																}
															}
														}
													}
													thumbnail.Dispose();
													thumbnail = null;
												}
												else
												{
													
													if (((sho.GetIconType() & IExtractIconpwFlags.GIL_PERINSTANCE) == 0 && thumbnail == null) || IconSize == 16)
													{
														if (IconSize != 16)
															ThumbnailsForCacheLoad.Enqueue(index);
														var icon = sho.GetShellThumbnail(IconSize, ShellThumbnailFormatOption.IconOnly);
														if (icon != null)
														{
															using (Graphics g = Graphics.FromHdc(hdc))
															{
																if (sho.IsHidden)
																	icon = Helpers.ChangeOpacity(icon, 0.5f);
																g.DrawImageUnscaled(icon, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - icon.Width) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - icon.Height) / 2, icon.Width, icon.Height));

																if (this.ShowCheckboxes && View != ShellViewStyle.Details && View != ShellViewStyle.List)
																{
																	var nItemState = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_STATEIMAGEMASK);

																	if ((int)User32.SendMessage(this.LVHandle, LVM.GETHOTITEM, 0, 0) == index || (int)nItemState == (2 << 12))
																	{
																		var checkboxOffsetH = 14;
																		var checkboxOffsetV = 2;
																		if (View == ShellViewStyle.Tile || View == ShellViewStyle.SmallIcon)
																			checkboxOffsetH = 2;
																		if (View == ShellViewStyle.Tile)
																			checkboxOffsetV = 1;
																		var lvis = User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMSTATE, index, LVIS.LVIS_SELECTED);
																		if (lvis != 0)
																		{
																			CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
																		}
																		else
																		{
																			CheckBoxRenderer.DrawCheckBox(g, new System.Drawing.Point(itemBounds.Left + checkboxOffsetH, itemBounds.Top + checkboxOffsetV), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
																		}
																	}
																}
															}
															icon.Dispose();
														}
													}
													else
													{
														if (!sho.IsFolder)
														{
															Bitmap bmp = null;
															if (!cache.TryGetValue(index, out bmp) || bmp == null)
															{
																if (thumbnail == null)
																{
																	waitingThumbnails.Enqueue(index);
																	using (Graphics g = Graphics.FromHdc(hdc))
																	{
																		if (IconSize == 16)
																		{
																			g.DrawImage(ExeFallBack16, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		}
																		else if (IconSize <= 48)
																		{
																			g.DrawImage(ExeFallBack48, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		}
																		else if (IconSize <= 256)
																		{
																			g.DrawImage(ExeFallBack256, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																		}
																	}
																}
															}
															else
															{
																if (bmp == null || bmp.Width != IconSize)
																{
																	if (thumbnail == null)
																	{
																		waitingThumbnails.Enqueue(index);
																		using (Graphics g = Graphics.FromHdc(hdc))
																		{
																			if (IconSize == 16)
																			{
																				g.DrawImage(ExeFallBack16, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																			}
																			else if (IconSize <= 48)
																			{
																				g.DrawImage(ExeFallBack48, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																			}
																			else if (IconSize <= 256)
																			{
																				g.DrawImage(ExeFallBack256, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));
																			}
																		}
																	}
																}
																else
																{
																	if (thumbnail == null)
																	{
																		using (Graphics g = Graphics.FromHdc(hdc))
																		{
																			if (sho.IsHidden)
																				bmp = Helpers.ChangeOpacity(bmp, 0.5f);
																			g.DrawImage(bmp, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));

																		}

																	}
																}
															}
														}
														else
														{
															if ((sho.GetIconType() & IExtractIconpwFlags.GIL_PERINSTANCE) == IExtractIconpwFlags.GIL_PERINSTANCE)
															{
																Bitmap bmp = null;
																if (!cache.TryGetValue(index, out bmp) || bmp == null)
																{
																	waitingThumbnails.Enqueue(index);
																}
																else
																{
																	if (thumbnail == null)
																	{
																		using (Graphics g = Graphics.FromHdc(hdc))
																		{
																			if (sho.IsHidden)
																				bmp = Helpers.ChangeOpacity(bmp, 0.5f);
																			g.DrawImage(bmp, new Rectangle(iconBounds.Left + (iconBounds.Right - iconBounds.Left - IconSize) / 2, iconBounds.Top + (iconBounds.Bottom - iconBounds.Top - IconSize) / 2, IconSize, IconSize));

																		}

																	}
																}
															}
														}
													}
												}




												if (overlayIndex != 0)
												{
													if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon)
													{
														small.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - 16));
													}
													else
													{
														if (this.IconSize > 180)
														{
															jumbo.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
														}
														else
															if (this.IconSize > 64)
															{
																extra.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 50));
															}
															else
															{
																large.DrawOverlay(hdc, overlayIndex, new System.Drawing.Point(iconBounds.Left + 10, iconBounds.Bottom - 32));
															}
													}
												}

												if (shieleded != 0)
												{
													if (this.View == ShellViewStyle.Details || this.View == ShellViewStyle.List || this.View == ShellViewStyle.SmallIcon)
													{
														small.DrawIcon(hdc, shieleded, new System.Drawing.Point(iconBounds.Right - 10, iconBounds.Bottom - 10), 8);
													}
													else
													{
														if (this.IconSize > 180)
														{
															jumbo.DrawIcon(hdc, shieleded, new System.Drawing.Point(iconBounds.Right - this.IconSize / 3, iconBounds.Bottom - this.IconSize / 3), this.IconSize / 3);
														}
														else
															if (this.IconSize > 64)
															{
																extra.DrawIcon(hdc, shieleded, new System.Drawing.Point(iconBounds.Right - 60, iconBounds.Bottom - 50));
															}
															else
															{
																large.DrawIcon(hdc, shieleded, new System.Drawing.Point(iconBounds.Right - 42, iconBounds.Bottom - 32));
															}
													}
												}

												if (View == ShellViewStyle.Tile)
												{
													var lableBounds = new User32.RECT();

													lableBounds.Left = 2;

													User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_GETITEMRECT, index, ref lableBounds);

													using (Graphics g = Graphics.FromHdc(hdc))
													{
														StringFormat fmt = new StringFormat();
														fmt.Trimming = StringTrimming.EllipsisCharacter;
														fmt.Alignment = StringAlignment.Center;
														fmt.Alignment = StringAlignment.Near;
														fmt.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;
														fmt.LineAlignment = StringAlignment.Center;



														RectangleF lblrectTiles = new RectangleF(lableBounds.Left, itemBounds.Top + 4, lableBounds.Right - lableBounds.Left, 20);
														//bufferedGraphics.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
														//bufferedGraphics.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
														//bufferedGraphics.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
														Font font = SystemFonts.IconTitleFont;
														SolidBrush textBrush = new SolidBrush(textColor == null ? SystemColors.ControlText : textColor.Value);
														g.DrawString(sho.DisplayName,
																										font,
																										textBrush,
																										lblrectTiles,
																										fmt);
														font.Dispose();
														textBrush.Dispose();

													}
												}

											}


										}
										m.Result = (IntPtr)CustomDraw.CDRF_SKIPDEFAULT;
										break;
								}
							}
						}
						break;
				}
			}

		}

		private static BitmapFrame CreateResizedImage(IntPtr hBitmap, int width, int height, int margin)
		{
			var source = Imaging.CreateBitmapSourceFromHBitmap(
						hBitmap,
						IntPtr.Zero,
						System.Windows.Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions()).Clone();
			Gdi32.DeleteObject(hBitmap);



			var group = new DrawingGroup();
			RenderOptions.SetBitmapScalingMode(
					group, BitmapScalingMode.Fant);
			group.Children.Add(
					new ImageDrawing(source,
							new Rect(0, 0, width, height)));
			var targetVisual = new DrawingVisual();
			var targetContext = targetVisual.RenderOpen();
			targetContext.DrawDrawing(group);
			var target = new RenderTargetBitmap(
					width, height, 96, 96, PixelFormats.Default);
			targetContext.Close();
			target.Render(targetVisual);
			return BitmapFrame.Create(target);
		}

		void bw_DoWork(object sender, DoWorkEventArgs e)
		{
			var dest = e.Argument as ShellItem;
			var array = dest.OrderByDescending(o => o.IsFolder).ToArray();
			array = null;
			GC.Collect();
			e.Result = dest;
		}

		void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			var destination = e.Result as ShellItem;
			//this.Items = destination.OrderByDescending(o => o.IsFolder).ToArray();
			////this.Items = destination.OrderByDescending(o => o.IsFolder).ToArray();

			//this.Cancel = false;
			//this.LastSortedColumnIndex = 0;
			//this.LastSortOrder = SortOrder.Ascending;
			//this.SetSortIcon(this.LastSortedColumnIndex, this.LastSortOrder);
			//this.m_CurrentFolder = destination;
			//try
			//{
			//	m_History.Add(destination);
			//}
			//catch { }

			//this.OnNavigated(new NavigatedEventArgs(destination));
			//User32.SendMessage(this.LVHandle, BExplorer.Shell.Interop.MSG.LVM_SETITEMCOUNT, this.Items.Length, 0);
		}

		//protected override void OnResize(EventArgs e)
		//{
		//	base.OnResize(e);
		//	//User32.SetWindowPos(this.LVHandle, IntPtr.Zero, 0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height, 0);
		//	
		//}
		async void OnNavigated(NavigatedEventArgs e)
		{
			if (Navigated != null)
			{
				Navigated(this, e);
				
			}
			//this.FolderSizes.Clear();
			//LPCSHCOLUMNINIT lpi = new LPCSHCOLUMNINIT();
			//lpi.wszFolder = e.Folder.ParsingName + "\0";
			//if (ICP != null)
			//{
			//	await Task.Run(() =>
			//	{
			//		this.BeginInvoke(new MethodInvoker(() =>
			//		{
			//			this.FolderSizes.Clear();
			//			ICP.Initialize(lpi);
			//			foreach (var item in this.CurrentFolder)
			//			{
			//				var pn = item.ParsingName;
			//				LPCSHCOLUMNID lid = new LPCSHCOLUMNID();
			//				lid.fmtid = Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748");
			//				lid.pid = 0;
			//				LPCSHCOLUMNDATA ldata = new LPCSHCOLUMNDATA();
			//				ldata.dwFileAttributes = item.IsFolder ? (uint)FileAttributes.Directory : 0;
			//				//ldata.dwFileAttributes = (uint)FileAttributes.Directory;
			//				if (!item.IsFolder)
			//				{
			//					ldata.pwszExt = Path.GetExtension(item.ParsingName);
			//				}
			//				ldata.wszFile = pn + "\0";
			//				object o = 0;
			//				this.ICP.GetItemData(lid, ldata, out o);
			//				if (o != null)
			//				{
			//					if (this.FolderSizes.ContainsKey(pn))
			//						this.FolderSizes[pn] = o.ToString();
			//					else
			//						this.FolderSizes.TryAdd(pn, o.ToString());
			//				}

			//				Thread.Sleep(1);
			//				//Application.DoEvents();

			//			}
			//		}));
			//	});
			//}

		}

		void OnViewChanged(ViewChangedEventArgs e)
		{
			if (ViewStyleChanged != null)
			{
				ViewStyleChanged(this, e);
			}
		}
		void pc_UpdateProgress(object sender, ProgressArgs<ShellItem> e)
		{
			//var aaa = e.Item;
		}

		bool RenameCallback(IntPtr hwnd, IntPtr lParam)
		{
			var index = User32.SendMessage(this.LVHandle, LVM.GETNEXTITEM, -1, LVNI.LVNI_SELECTED);
			var res = User32.SendMessage(hwnd, BExplorer.Shell.Interop.MSG.LVM_EDITLABELW, index, 0);
			return false;

			//return true;
		}
		void selectionTimer_Tick(object sender, EventArgs e)
		{
			OnSelectionChanged();
			(sender as System.Windows.Forms.Timer).Stop();
		}
	}
	public class ViewChangedEventArgs : EventArgs
	{
		Int32 m_ThumbnailSize;
		ShellViewStyle m_View;
		public ViewChangedEventArgs(ShellViewStyle view, Int32? thumbnailSize)
		{
			m_View = view;
			if (thumbnailSize != null)
				m_ThumbnailSize = thumbnailSize.Value;
		}
		/// <summary>
		/// The current ViewStyle
		/// </summary>
		public ShellViewStyle CurrentView
		{
			get { return m_View; }
			set { m_View = value; }
		}

		public Int32 ThumbnailSize
		{
			get { return m_ThumbnailSize; }
			set { m_ThumbnailSize = value; }
		}
	}
}
