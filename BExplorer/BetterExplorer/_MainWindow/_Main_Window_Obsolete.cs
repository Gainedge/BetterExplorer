using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fluent;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Threading;
using Clipboards = System.Windows.Forms.Clipboard;
using MenuItem = Fluent.MenuItem;
using System.Windows.Shell;
using System.Windows.Media.Animation;
using System.Collections.Specialized;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Threading;
using SevenZip;
using ContextMenu = Fluent.ContextMenu;
using System.Globalization;
using NAppUpdate.Framework;
using BetterExplorerControls;
using Microsoft.WindowsAPICodePack.Shell.FileOperations;
using Microsoft.WindowsAPICodePack.Shell.ExplorerBrowser;
using wyDay.Controls;
using System.Security.Principal;
using Shell32;
using Microsoft.WindowsAPICodePack.Taskbar;
using BetterExplorer.Networks;
using System.Xml.Linq;
using System.Text;
using BetterExplorer.UsbEject;
using LTR.IO;
using LTR.IO.ImDisk;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using WindowsHelper;

namespace BetterExplorer {
	partial class MainWindow {

		#region Being Removed

		[Obsolete("Removing this is High Priority!", true)]
		bool IsViewSelection = false, inLibrary = false, inDrive = false;

		/// <summary>Try to make it so you do not need this!</summary>
		bool isGoingBackOrForward; //TODO: Try to make it so you do not need this!

		[Obsolete("Removing this is High Priority!", true)]
		public bool IsCompartibleRename = false;


		/*
		private void Explorer_ViewEnumerationComplete(object sender, EventArgs e) {
			//IsCalledFromViewEnum = true;
			//zoomSlider.Value = ShellListView.ContentOptions.ThumbnailSize;
			//IsCalledFromViewEnum = false;

			//Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() =>
			//{

			//    try
			//    {
			//        IShellView isv = ShellListView.GetShellView();
			//        Collumns[] AllAvailColls = ShellListView.AvailableColumns(true);

			//        SetupColumnsButton(AllAvailColls);

			//        SetSortingAndGroupingButtons();
			//    }
			//    catch (Exception)
			//    {

			//    }


			//}));

			//IsViewSelection = false;
			//SetUpViewGallery();
			//IsViewSelection = true;
			//ShellListView.ContentOptions.CheckSelect = this.isCheckModeEnabled;
		}
		*/


		[Obsolete("Save?", true)]
		private uint GetDeviceNumberForDriveLetter(char letter) {
			List<int> list = ImDiskAPI.GetDeviceList();
			foreach (int item in list) {
				if (ImDiskAPI.QueryDevice(Convert.ToUInt32(item)).DriveLetter == letter) {
					return Convert.ToUInt32(item);
				}
			}

			return 0;
		}

		[Obsolete("Never used", true)]
		private List<char> GetLettersOfVirtualDrives(bool threaded = true) {
			if (threaded) {
				List<char> drives = new List<char>();
				Thread t = new Thread(() => {
					Thread.Sleep(10);
					List<int> list = ImDiskAPI.GetDeviceList();
					foreach (int item in list) {
						drives.Add(ImDiskAPI.QueryDevice(Convert.ToUInt32(item)).DriveLetter);
					}
				});
				t.Start();
				return drives;
			}
			else {
				List<char> drives = new List<char>();
				List<int> list = ImDiskAPI.GetDeviceList();
				foreach (int item in list) {
					try {
						drives.Add(ImDiskAPI.QueryDevice(Convert.ToUInt32(item)).DriveLetter);
					}
					catch {

					}
				}
				return drives;
			}
		}


		[Obsolete("Not Used!!", true)]
		private string GetUseablePath(string path) {
			if (path.StartsWith("::")) {
				string lib = path.Substring(0, path.IndexOf("\\"));
				string gp = GetDefaultFolderfromLibrary(lib);
				if (gp == lib) {
					return path;
				}
				else {
					return gp + path.Substring(path.IndexOf("\\") + 1);
				}
			}
			else {
				return path;
			}
		}

		[Obsolete("Not Used!!", false)]
		private string GetDefaultFolderfromLibrary(string library) {
			try {
				ShellLibrary lib = ShellLibrary.Load(library, true);
				return lib.DefaultSaveFolder;
			}
			catch {
				return library;
			}
		}


		[Obsolete("Not used", true)]
		private bool IsConnectedToInternet() {
			//Code source - Codeplex User Salysle
			//Hope it works...
			int lngFlags = 0;
			return WindowsAPI.InternetGetConnectedState(lngFlags, 0);
		}

		[Obsolete("Not Used!!!", true)]
		public bool Activate(bool restoreIfMinimized) {
			if (restoreIfMinimized && WindowState == WindowState.Minimized) {
				WindowState = PreviouseWindowState == WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
			}

			return Activate();
		}

		#endregion

		#region DLLImports

		//public static System.Drawing.Point GetMousePosition() {
		//	Win32Point w32Mouse = new Win32Point();
		//	GetCursorPos(ref w32Mouse);
		//	return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
		//}
		#endregion


		#region Variables and Constants
		//string LastItemSelected = "";
		//bool KeepFocusOnExplorer = false;
		//IProgressDialog pd;

		#endregion

		#region Properties

		/*
		/// <summary>
		/// Gets the New context menu
		/// </summary>
		/// <returns>The list of the new context menu items</returns>
		private List<string> GetNewContextMenu() {
			List<string> TheList = new List<string>();
			RegistryKey reg = Registry.CurrentUser;
			RegistryKey classesrk = reg.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Discardable\PostSetup\ShellNew");
			string[] classes = (string[])classesrk.GetValue("Classes");

			foreach (string item in classes) {
				RegistryKey regext = Registry.ClassesRoot;
				RegistryKey extrk = regext.OpenSubKey(item + @"\ShellNew");
				if (extrk != null) {
					string FileName = (string)extrk.GetValue("FileName");
					TheList.Add(FileName);
				}
			}

			classesrk.Close();
			reg.Close();
			return TheList;
		}
		*/

		#endregion


		#region Miscellaneous Helper Functions

		/*
		public ShellItem GetShellItemFromLocation(string Location) {
			ShellItem sho;
			if (Location.IndexOf_("::") == 0 && Location.IndexOf(@"\") == -1)
				sho = new ShellItem("shell:" + Location);
			else
				try {
					sho = new ShellItem(Location);
				}
				catch {
					sho = (ShellItem)KnownFolders.Libraries;
				}
			if (Path.GetExtension(Location).ToLowerInvariant() == ".library-ms") {
				sho = (ShellItem)ShellLibrary.Load(Path.GetFileNameWithoutExtension(Location), true);
			}
			return sho;
		}
		*/


		#endregion

		#region Old Search Code
		//private ShellItem BeforeSearchFolder;

		//Thread backgroundSearchThread;
		// Helper method to do the search on a background thread



		//void bw_DoWork(object sender, DoWorkEventArgs e) {
		//	//ShellListView.Navigate((ShellSearchFolder)e.Argument);
		//}

		//private void searchTextBox1_Search(object sender, RoutedEventArgs e) {

		//	//DoSearch(searchTextBox1.Text);
		//	//StatusBar.UpdateLayout();

		//}
		//int searchcicles = 0;
		//int BeforeSearcCicles = 0;

		#endregion

		#region Archive Commands
		/*
		private void CreateArchive_Show(OutArchiveFormat format) {
			var selectedItems = new List<string>();
			foreach (ShellItem item in ShellListView.SelectedItems) {
				if (Directory.Exists(item.ParsingName)) {
					DirectoryInfo di = new DirectoryInfo(item.ParsingName);
					FileInfo[] Files = di.GetFiles("*", SearchOption.AllDirectories);
					foreach (FileInfo fi in Files) {
						selectedItems.Add(fi.FullName);
					}

				}
				else {
					selectedItems.Add(item.ParsingName);
				}

			}
			if (selectedItems.Count > 0) {
				try {
					var CAI = new CreateArchive(selectedItems,
												true,
												Path.GetDirectoryName(selectedItems[0]),
												format);

					CAI.Show(this.GetWin32Window());


				}
				catch (Exception exception) {
					var dialog = new TaskDialog();
					dialog.StandardButtons = TaskDialogStandardButtons.Ok;
					dialog.Text = exception.Message;
					dialog.Show();
				}
			}
		}
		*/


		#endregion

		#region Image Editing

		/*
		private Bitmap ChangeImageSize(Bitmap img, int width, int height) {
			Bitmap bm_dest = new Bitmap(width, height);
			Graphics gr_dest = Graphics.FromImage(bm_dest);
			gr_dest.DrawImage(img, 0, 0, bm_dest.Width + 1, bm_dest.Height + 1);
			return bm_dest;
		}
		*/



		#endregion

		#region Registry Setting Changes / BetterExplorerOperations Calls / Action Log

		/*
		[StructLayout(LayoutKind.Sequential)]
		public struct SymLinkInfo {
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
			public string lpDestination;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
			public string lpTarget;
			public int SymType;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
			public string lpMsg;
		};
		*/

		#endregion

		#region IsBool Code
		/*
		[Obsolete("Does Nothing")]
		private void RibbonWindow_Activated(object sender, EventArgs e) {
			//if (!backstage.IsOpen)
			//  ShellListView.SetExplorerFocus();

		}

		void fAbout_Closed(object sender, EventArgs e) {

		}


		private void btnCopyto_GotFocus(object sender, RoutedEventArgs e) {

		}

		private void btnCopyto_LostFocus(object sender, RoutedEventArgs e) {

		}

		private void btnMoreColls_DropDownOpened(object sender, EventArgs e) {

		}

		private void btnMoreColls_DropDownClosed(object sender, EventArgs e) {

		}

		/*
		void cmHistory_Closed(object sender, RoutedEventArgs e) {

		}

		void cmHistory_Opened(object sender, RoutedEventArgs e) {

		}
		-/
		*/
		#endregion

		#region Tabs

		/*
		private void ChangeTab(object sender, ExecutedRoutedEventArgs e) {
			t.Stop();
			//SelectTab(tabControl1.SelectedIndex + 1);
			int selIndex = tabControl1.SelectedIndex == tabControl1.Items.Count - 1 ? 0 : tabControl1.SelectedIndex + 1;
			tabControl1.SelectedItem = tabControl1.Items[selIndex];
			NavigateAfterTabChange();
		}
		*/

		/*
		void CreateTabbarRKMenu(ClosableTabItem tabitem) {
			tabitem.mnu = new ContextMenu();
			MenuItem miclosecurrentr = new MenuItem();
			miclosecurrentr.Header = "Close current tab";
			miclosecurrentr.Tag = tabitem;
			miclosecurrentr.Click += new RoutedEventHandler(miclosecurrentr_Click);
			tabitem.mnu.Items.Add(miclosecurrentr);

			MenuItem miclosealltab = new MenuItem();
			miclosealltab.Header = "Close all tabs";
			miclosealltab.Click += new RoutedEventHandler(miclosealltab_Click);
			tabitem.mnu.Items.Add(miclosealltab);

			MenuItem miclosealltabbd = new MenuItem();
			miclosealltabbd.Header = "Close all other tabs";
			miclosealltabbd.Tag = tabitem;
			miclosealltabbd.Click += new RoutedEventHandler(miclosealltabbd_Click);
			tabitem.mnu.Items.Add(miclosealltabbd);

			tabitem.mnu.Items.Add(new Separator());


			MenuItem minewtabr = new MenuItem();
			minewtabr.Header = "New tab";
			minewtabr.Tag = tabitem;
			minewtabr.Click += new RoutedEventHandler(minewtabr_Click);
			tabitem.mnu.Items.Add(minewtabr);


			MenuItem miclonecurrentr = new MenuItem();
			miclonecurrentr.Header = "Clone tab";
			miclonecurrentr.Tag = tabitem;
			miclonecurrentr.Click += new RoutedEventHandler(miclonecurrentr_Click);
			tabitem.mnu.Items.Add(miclonecurrentr);

			tabitem.mnu.Items.Add(new Separator());


			MenuItem miundocloser = new MenuItem();
			miundocloser.Header = "Undo close tab";
			miundocloser.IsEnabled = btnUndoClose.IsEnabled;
			miundocloser.Tag = "UCTI";
			miundocloser.Click += new RoutedEventHandler(miundocloser_Click);
			tabitem.mnu.Items.Add(miundocloser);

			tabitem.mnu.Items.Add(new Separator());

			MenuItem miopeninnew = new MenuItem();
			miopeninnew.Header = "Open in new window";
			miopeninnew.Tag = tabitem;
			miopeninnew.Click += new RoutedEventHandler(miopeninnew_Click);
			tabitem.mnu.Items.Add(miopeninnew);

		}
		//void miopeninnew_Click(object sender, RoutedEventArgs e) {
		//	MenuItem mi = (sender as MenuItem);
		//	ClosableTabItem ti = mi.Tag as ClosableTabItem;
		//	Process.Start(Assembly.GetExecutingAssembly().GetName().Name, ti.ShellObject.ParsingName + " /nw");
		//	CloseTab(ti);
		//	//throw new NotImplementedException();
		//}

		//void miclosealltabbd_Click(object sender, RoutedEventArgs e) {
		//	MenuItem mi = (sender as MenuItem);
		//	ClosableTabItem ti = mi.Tag as ClosableTabItem;
		//	CloseAllTabsButThis(ti);
		//}

		//void miclosealltab_Click(object sender, RoutedEventArgs e) {
		//	CloseAllTabs(true);
		//}

		//void miclosecurrentr_Click(object sender, RoutedEventArgs e) {
		//	MenuItem mi = (sender as MenuItem);
		//	ClosableTabItem ti = mi.Tag as ClosableTabItem;
		//	CloseTab(ti);
		//}

		//void minewtabr_Click(object sender, RoutedEventArgs e) {
		//	MenuItem mi = (sender as MenuItem);
		//	ClosableTabItem ti = mi.Tag as ClosableTabItem;
		//	NewTab();
		//}

		//void miclonecurrentr_Click(object sender, RoutedEventArgs e) {
		//	MenuItem mi = (sender as MenuItem);
		//	ClosableTabItem ti = mi.Tag as ClosableTabItem;
		//	CloneTab(ti);
		//}

		//void miundocloser_Click(object sender, RoutedEventArgs e) {
		//	//MenuItem mi = (sender as MenuItem);
		//	if (btnUndoClose.IsEnabled) {
		//		btnUndoClose_Click(this, e);
		//	}
		//}

		//void CloseAllTabs(bool CloseFirstTab) {
		//	foreach (ClosableTabItem tab in tabControl1.Items.OfType<ClosableTabItem>().ToArray()) {
		//		CloseTab(tab);
		//	}
		//}


		//void CloseAllTabsButThis(ClosableTabItem tabitem) {
		//	foreach (ClosableTabItem tab in tabControl1.Items.OfType<ClosableTabItem>().ToArray()) {
		//		if (tab != tabitem) CloseTab(tab);
		//	}

		//	ConstructMoveToCopyToMenu();
		//}

		//public void NewTab(bool IsNavigate = true) {
		//	ClosableTabItem newt = new ClosableTabItem();
		//	CreateTabbarRKMenu(newt);

		//	ShellItem DefPath;
		//	if (StartUpLocation.StartsWith("::") && StartUpLocation.IndexOf(@"\") == -1)
		//		DefPath = new ShellItem("shell:" + StartUpLocation);
		//	else
		//		try {
		//			DefPath = new ShellItem(StartUpLocation);
		//		}
		//		catch {
		//			DefPath = (ShellItem)KnownFolders.Libraries;
		//		}

		//	DefPath.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
		//	DefPath.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
		//	newt.PreviewMouseMove += newt_PreviewMouseMove;
		//	newt.Header = DefPath.GetDisplayName(SIGDN.NORMALDISPLAY);
		//	newt.TabIcon = DefPath.Thumbnail.BitmapSource;
		//	newt.ShellObject = DefPath;
		//	newt.ToolTip = DefPath.ParsingName;
		//	newt.IsNavigate = IsNavigate;
		//	newt.Index = tabControl1.Items.Count;
		//	newt.AllowDrop = true;
		//	newt.log.CurrentLocation = DefPath;

		//	newt.CloseTab += new RoutedEventHandler(newt_CloseTab);
		//	newt.DragEnter += new DragEventHandler(newt_DragEnter);
		//	newt.DragOver += new DragEventHandler(newt_DragOver);
		//	newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove);
		//	newt.Drop += new DragEventHandler(newt_Drop);
		//	newt.TabSelected += newt_TabSelected;
		//	tabControl1.Items.Add(newt);
		//	LastTabIndex = tabControl1.SelectedIndex;
		//	tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
		//	tabControl1.SelectedItem = tabControl1.Items[tabControl1.Items.Count - 1];

		//	ConstructMoveToCopyToMenu();
		//	NavigateAfterTabChange();
		//}

		//public void NewTab(ShellItem location, bool IsNavigate = false) {
		//	ClosableTabItem newt = new ClosableTabItem();
		//	CreateTabbarRKMenu(newt);

		//	ShellItem DefPath = location;
		//	DefPath.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
		//	DefPath.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
		//	newt.PreviewMouseMove += newt_PreviewMouseMove;
		//	newt.Header = DefPath.GetDisplayName(BExplorer.Shell.Interop.SIGDN.NORMALDISPLAY);
		//	newt.TabIcon = DefPath.Thumbnail.BitmapSource;
		//	newt.ShellObject = DefPath;
		//	newt.ToolTip = DefPath.ParsingName;
		//	newt.IsNavigate = IsNavigate;
		//	newt.Index = tabControl1.Items.Count;
		//	newt.AllowDrop = true;
		//	newt.log.CurrentLocation = DefPath;

		//	newt.CloseTab += new RoutedEventHandler(newt_CloseTab);
		//	newt.DragEnter += new DragEventHandler(newt_DragEnter);
		//	newt.DragOver += new DragEventHandler(newt_DragOver);
		//	newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove);
		//	newt.Drop += new DragEventHandler(newt_Drop);
		//	newt.TabSelected += newt_TabSelected;
		//	tabControl1.Items.Add(newt);
		//	LastTabIndex = tabControl1.SelectedIndex;
		//	if (IsNavigate) {
		//		tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
		//		tabControl1.SelectedItem = tabControl1.Items[tabControl1.Items.Count - 1];
		//		NavigateAfterTabChange();
		//	}

		//	ConstructMoveToCopyToMenu();
		//}



		//public ClosableTabItem NewTab(string Location, bool IsNavigate = false) {
		//	ClosableTabItem newt = new ClosableTabItem();
		//	CreateTabbarRKMenu(newt);

		//	ShellItem DefPath = new ShellItem(Location);
		//	DefPath.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
		//	DefPath.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
		//	newt.Header = DefPath.GetDisplayName(SIGDN.NORMALDISPLAY);
		//	newt.TabIcon = DefPath.Thumbnail.BitmapSource;
		//	newt.ShellObject = DefPath;
		//	newt.ToolTip = DefPath.ParsingName;
		//	newt.IsNavigate = IsNavigate;
		//	newt.Index = tabControl1.Items.Count;
		//	newt.AllowDrop = true;
		//	newt.CloseTab += new RoutedEventHandler(newt_CloseTab);
		//	newt.DragEnter += new DragEventHandler(newt_DragEnter);
		//	newt.DragOver += new DragEventHandler(newt_DragOver);
		//	newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove);
		//	newt.TabSelected += newt_TabSelected;
		//	newt.Drop += new DragEventHandler(newt_Drop);

		//	tabControl1.Items.Add(newt);
		//	LastTabIndex = tabControl1.SelectedIndex;
		//	if (IsNavigate) {
		//		//IsCancel = true;

		//		tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
		//		tabControl1.SelectedItem = tabControl1.Items[tabControl1.Items.Count - 1];
		//		//IsCancel = false;
		//	}
		//	else {
		//		newt.log.CurrentLocation = DefPath;
		//	}

		//	ConstructMoveToCopyToMenu();
		//	return newt;
		//}
		*/

		/*
		private void ConstructMoveToCopyToMenu() {
			btnMoveto.Items.Clear();
			btnCopyto.Items.Clear();
			MenuItem OtherLocationMove = new MenuItem();
			OtherLocationMove.Focusable = false;
			OtherLocationMove.Header = FindResource("miOtherDestCP");
			OtherLocationMove.Click += new RoutedEventHandler(btnmtOther_Click);
			MenuItem OtherLocationCopy = new MenuItem();
			OtherLocationCopy.Focusable = false;
			OtherLocationCopy.Header = FindResource("miOtherDestCP");
			OtherLocationCopy.Click += new RoutedEventHandler(btnctOther_Click);

			MenuItem mimDesktop = new MenuItem();
			ShellItem sod = (ShellItem)KnownFolders.Desktop;
			sod.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			sod.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
			mimDesktop.Focusable = false;
			mimDesktop.Icon = sod.Thumbnail.BitmapSource;
			mimDesktop.Header = FindResource("btnctDesktopCP");
			mimDesktop.Click += new RoutedEventHandler(btnmtDesktop_Click);
			MenuItem micDesktop = new MenuItem();
			micDesktop.Focusable = false;
			micDesktop.Icon = sod.Thumbnail.BitmapSource;
			micDesktop.Header = FindResource("btnctDesktopCP");
			micDesktop.Click += new RoutedEventHandler(btnctDesktop_Click);


			MenuItem mimDocuments = new MenuItem(), micDocuments = new MenuItem();
			try {
				ShellItem sodc = (ShellItem)KnownFolders.Documents;
				sodc.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				sodc.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
				mimDocuments.Focusable = false;
				mimDocuments.Icon = sodc.Thumbnail.BitmapSource;
				mimDocuments.Header = FindResource("btnctDocumentsCP");
				mimDocuments.Click += new RoutedEventHandler(btnmtDocuments_Click);

				micDocuments.Focusable = false;
				micDocuments.Icon = sodc.Thumbnail.BitmapSource;
				micDocuments.Header = FindResource("btnctDocumentsCP");
				micDocuments.Click += new RoutedEventHandler(btnctDocuments_Click);

			}
			catch (Exception) {
				mimDocuments = null;
				micDocuments = null;
				//ctach the exception in case the user deleted that basic folder somehow
			}

			MenuItem mimDownloads = new MenuItem(), micDownloads = new MenuItem();
			try {

				ShellItem sodd = (ShellItem)KnownFolders.Downloads;
				sodd.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				sodd.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
				mimDownloads.Focusable = false;
				mimDownloads.Icon = sodd.Thumbnail.BitmapSource;
				mimDownloads.Header = FindResource("btnctDownloadsCP");
				mimDownloads.Click += new RoutedEventHandler(btnmtDounloads_Click);

				micDownloads.Focusable = false;
				micDownloads.Icon = sodd.Thumbnail.BitmapSource;
				micDownloads.Header = FindResource("btnctDownloadsCP");
				micDownloads.Click += new RoutedEventHandler(btnctDounloads_Click);

			}
			catch (Exception) {
				micDownloads = null;
				mimDownloads = null;
				//ctach the exception in case the user deleted that basic folder somehow
			}

			if (mimDocuments != null)
				btnMoveto.Items.Add(mimDocuments);

			if (mimDownloads != null)
				btnMoveto.Items.Add(mimDownloads);

			btnMoveto.Items.Add(mimDesktop);
			btnMoveto.Items.Add(new Separator());

			if (micDocuments != null)
				btnCopyto.Items.Add(micDocuments);

			if (micDownloads != null)
				btnCopyto.Items.Add(micDownloads);

			btnCopyto.Items.Add(micDesktop);
			btnCopyto.Items.Add(new Separator());

			foreach (var item in tabControl1.Items.OfType<ClosableTabItem>().ToList()) {
				bool IsAdditem = true;
				foreach (object mii in btnCopyto.Items) {
					if (mii is MenuItem) {
						if ((mii as MenuItem).Tag != null) {
							if (((mii as MenuItem).Tag as ShellItem) == item.ShellObject) {
								IsAdditem = false;
								break;
							}
						}
					}
				}
				if (IsAdditem && item.ShellObject.IsFileSystem) {
					try {
						MenuItem mim = new MenuItem();
						mim.Header = item.ShellObject.GetDisplayName(SIGDN.NORMALDISPLAY);
						mim.Focusable = false;
						mim.Tag = item.ShellObject;
						ShellItem so = new ShellItem(item.ShellObject.ParsingName);
						so.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
						so.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
						mim.Icon = so.Thumbnail.BitmapSource;
						mim.Click += new RoutedEventHandler(mim_Click);
						btnMoveto.Items.Add(mim);
						MenuItem mic = new MenuItem();
						mic.Focusable = false;
						mic.Header = item.ShellObject.GetDisplayName(SIGDN.NORMALDISPLAY);
						mic.Tag = item.ShellObject;
						mic.Icon = so.Thumbnail.BitmapSource;
						mic.Click += new RoutedEventHandler(mimc_Click);
						btnCopyto.Items.Add(mic);
					}
					catch {
						//Do nothing if ShellItem is not available anymore and close the problematic item
						CloseTab(item);
					}
				}
			}
			btnMoveto.Items.Add(new Separator());
			btnMoveto.Items.Add(OtherLocationMove);
			btnCopyto.Items.Add(new Separator());
			btnCopyto.Items.Add(OtherLocationCopy);

		}
		*/

		/*
		public void NavigateAfterTabChange() {
			ClosableTabItem itb = tabControl1.SelectedItem as ClosableTabItem;

			isGoingBackOrForward = itb.log.HistoryItemsList.Count != 0;
			if (itb != null) {
				try {
					BeforeLastTabIndex = LastTabIndex;

					//tabControl1.SelectedIndex = itb.Index;
					//LastTabIndex = itb.Index;
					//CurrentTabIndex = LastTabIndex;
					if (ShellListView.CurrentFolder == null || itb.ShellObject.ParsingName != ShellListView.CurrentFolder.ParsingName) {
						if (!Keyboard.IsKeyDown(Key.Tab)) {
							ShellListView.Navigate(itb.ShellObject);
						}
						else {
							t.Interval = 500;
							t.Tag = itb.ShellObject;
							t.Tick += new EventHandler(t_Tick);
							t.Start();
						}
					}

					itb.BringIntoView();

				}
				catch (StackOverflowException) {

				}
				//'btnTabCloseC.IsEnabled = tabControl1.Items.Count > 1;
				//'there's a bug that has this enabled when there's only one tab open, but why disable it
				//'if it never crashes the program? Closing the last tab simply closes the program, so I
				//'thought, what the heck... let's just keep it enabled. :) -JaykeBird
			}
		}
		*/


		/*
		//'close tab
		void cti_CloseTab(object sender, RoutedEventArgs e) {
			ClosableTabItem curItem = e.Source as ClosableTabItem;
			CloseTab(curItem);
		}

		public void CloneTab(ClosableTabItem CurTab) {
			ClosableTabItem newt = new ClosableTabItem();
			CreateTabbarRKMenu(newt);
			newt.Header = CurTab.Header;
			newt.TabIcon = CurTab.TabIcon;
			newt.ShellObject = CurTab.ShellObject;
			newt.ToolTip = CurTab.ShellObject.ParsingName;
			newt.Index = tabControl1.Items.Count;
			newt.CloseTab += new RoutedEventHandler(newt_CloseTab);
			newt.DragEnter += new DragEventHandler(newt_DragEnter);
			newt.DragLeave += new DragEventHandler(newt_DragLeave);
			newt.DragOver += new DragEventHandler(newt_DragOver);
			newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove);
			newt.Drop += new DragEventHandler(newt_Drop);
			newt.TabSelected += newt_TabSelected;
			newt.AllowDrop = true;
			newt.log.CurrentLocation = CurTab.ShellObject;
			newt.SelectedItems = CurTab.SelectedItems;
			newt.log.ImportData(CurTab.log);
			tabControl1.Items.Add(newt);
			tabControl1.SelectedItem = newt;
			LastTabIndex = tabControl1.SelectedIndex;
			ConstructMoveToCopyToMenu();
			NavigateAfterTabChange();
		}
		*/

		/*
		void SelectTab(int Index) {

		}

		void CloseTab(ClosableTabItem thetab, bool allowreopening = true) {
			if (tabControl1.SelectedIndex == 0 && tabControl1.Items.Count == 1) {
				if (this.IsCloseLastTabCloseApp) {
					Close();
				}
				else {
					ShellListView.Navigate(new ShellItem(this.StartUpLocation));
				}
				return;
			}


			if (thetab.Index == 0 && tabControl1.Items.Count > 1) {
				tabControl1.SelectedIndex = thetab.Index + 1;
			}
			else if (thetab.Index == tabControl1.Items.Count - 1) {
				tabControl1.SelectedIndex = thetab.Index - 1;
			}
			else {
				for (int i = thetab.Index + 1; i < tabControl1.Items.Count; i++) {
					ClosableTabItem tab = tabControl1.Items[i] as ClosableTabItem;
					tab.Index = tab.Index - 1;
				}
			}

			tabControl1.Items.Remove(thetab);

			ConstructMoveToCopyToMenu();

			if (allowreopening) {
				reopenabletabs.Add(thetab.log);
				btnUndoClose.IsEnabled = true;
				foreach (ClosableTabItem item in this.tabControl1.Items) {
					foreach (FrameworkElement m in item.mnu.Items) {
						if (m.Tag != null) {
							if (m.Tag.ToString() == "UCTI")
								(m as MenuItem).IsEnabled = true;
						}
					}
				}
			}


			ClosableTabItem itb = tabControl1.Items[tabControl1.SelectedIndex] as ClosableTabItem;

			isGoingBackOrForward = itb.log.HistoryItemsList.Count != 0;
			if (itb != null) {
				try {
					BeforeLastTabIndex = LastTabIndex;

					//tabControl1.SelectedIndex = itb.Index;
					//LastTabIndex = itb.Index;
					//CurrentTabIndex = LastTabIndex;
					if (itb.ShellObject != ShellListView.CurrentFolder) {
						if (!Keyboard.IsKeyDown(Key.Tab)) {
							ShellListView.Navigate(itb.ShellObject);
						}
						else {
							t.Interval = 500;
							t.Tag = itb.ShellObject;
							t.Tick += new EventHandler(t_Tick);
							t.Start();
						}
					}
				}
				catch (StackOverflowException) {

				}

				//'btnTabCloseC.IsEnabled = tabControl1.Items.Count > 1;
				//'there's a bug that has this enabled when there's only one tab open, but why disable it
				//'if it never crashes the program? Closing the last tab simply closes the program, so I
				//'thought, what the heck... let's just keep it enabled. :) -JaykeBird
			}

			NavigateAfterTabChange();
		}
		*/

		/*
		/// <summary>
		/// Re-opens a previously closed tab using that tab's navigation log data.
		/// </summary>
		/// <param name="log">The navigation log data from the previously closed tab.</param>
		public void ReOpenTab(NavigationLog log) {
			ClosableTabItem newt = new ClosableTabItem();
			CreateTabbarRKMenu(newt);

			ShellItem DefPath = log.CurrentLocation;
			DefPath.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
			DefPath.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			newt.Header = DefPath.GetDisplayName(SIGDN.NORMALDISPLAY);
			newt.TabIcon = DefPath.Thumbnail.BitmapSource;
			newt.ShellObject = DefPath;
			newt.ToolTip = DefPath.ParsingName;
			newt.IsNavigate = false;
			newt.Index = tabControl1.Items.Count;
			newt.AllowDrop = true;
			newt.CloseTab += newt_CloseTab;
			newt.DragEnter += newt_DragEnter;
			newt.DragOver += newt_DragOver;
			newt.PreviewMouseMove += newt_PreviewMouseMove;
			newt.Drop += newt_Drop;
			newt.TabSelected += newt_TabSelected;
			tabControl1.Items.Add(newt);
			LastTabIndex = tabControl1.SelectedIndex;
			newt.log.ImportData(log);

			ConstructMoveToCopyToMenu();
			NavigateAfterTabChange();
		}
		*/

		#endregion

		#region Tab Controls
		/*
		private void MoveTabBarToBottom() {
			Grid.SetRow(this.tabControl1, 7);
			divNav.Visibility = Visibility.Visible;
			this.rTabbarTop.Height = new GridLength(0);
			this.rTabbarBot.Height = new GridLength(25);
			this.tabControl1.TabStripPlacement = Dock.Bottom;
		}

		private void MoveTabBarToTop() {
			Grid.SetRow(this.tabControl1, 3);
			divNav.Visibility = Visibility.Hidden;
			this.rTabbarTop.Height = new GridLength(25);
			this.rTabbarBot.Height = new GridLength(0);
			this.tabControl1.TabStripPlacement = Dock.Top;
		}


		private void RadioButton_Checked(object sender, RoutedEventArgs e) {
			MoveTabBarToTop();
		}

		private void RadioButton_Checked_1(object sender, RoutedEventArgs e) {
			MoveTabBarToBottom();
		}
		*/

		/*
		private void btn_ToolTipOpening(object sender, ToolTipEventArgs e) {
			if (sender is SplitButton) {
				if ((sender as SplitButton).IsDropDownOpen) e.Handled = true;
			}
		}
		*/
		#endregion

		#region Customize Quick Access Toolbar

		/*
		public List<string> GetNamesFromRibbonControls(List<IRibbonControl> input) {
			var o = new List<string>();

			foreach (IRibbonControl item in input) {
				o.Add((item as FrameworkElement).Name);
			}

			return o;
		}
		*/


		/*
		public Tuple<string, IRibbonControl> AddOtherButtonForDictionary(IRibbonControl item, Dictionary<string, IRibbonControl> dict) {
			Tuple<string, IRibbonControl> entry = new Tuple<string, IRibbonControl>((item as FrameworkElement).Name, item);
			dict.Add(entry.Item1, entry.Item2);
			return entry;
		}
		*/

		/*
		public Tuple<string, IRibbonControl> AddOtherButtonForLists(IRibbonControl item, List<IRibbonControl> rb, List<string> rs) {
			Tuple<string, IRibbonControl> entry = new Tuple<string, IRibbonControl>((item as FrameworkElement).Name, item);
			rb.Add(item);
			rs.Add(item.Header as string);
			return entry;
		}
		*/

		/*
		public Tuple<string, IRibbonControl> RemoveOtherButtonForLists(IRibbonControl item, List<IRibbonControl> rb, List<string> rs) {
			Tuple<string, IRibbonControl> entry = new Tuple<string, IRibbonControl>((item as FrameworkElement).Name, item);
			rb.Remove(item);
			rs.Remove(item.Header as string);
			return entry;
		}
		*/


		public List<Fluent.IRibbonControl> GetAllButtons() {
			List<Fluent.IRibbonControl> rb = new List<Fluent.IRibbonControl>();
			List<string> rs = new List<string>();

			foreach (RibbonTabItem item in TheRibbon.Tabs) {
				foreach (RibbonGroupBox itg in item.Groups) {
					foreach (object ic in itg.Items) {
						if (ic is IRibbonControl) {
							rb.Add(ic as IRibbonControl);
							rs.Add((ic as IRibbonControl).Header as string);
						}
					}
				}
			}

			rs.Sort();

			return SortNames(rb, rs);
		}

		public List<Fluent.IRibbonControl> GetNonQATButtons() {
			var rb = new List<Fluent.IRibbonControl>();
			var rs = new List<string>();

			foreach (RibbonTabItem item in TheRibbon.Tabs) {
				foreach (RibbonGroupBox itg in item.Groups) {
					foreach (object ic in itg.Items) {
						if (ic is IRibbonControl) {
							if (!(TheRibbon.IsInQuickAccessToolBar(ic as UIElement))) {
								rb.Add(ic as IRibbonControl);
								rs.Add((ic as IRibbonControl).Header as string);
							}
						}
					}
				}
			}

			rs.Sort();
			return SortNames(rb, rs);
		}

		/*
		public void AddOtherButton(IRibbonControl item, bool test, List<IRibbonControl> rb, List<string> rs) {
			if (TheRibbon.IsInQuickAccessToolBar(item as UIElement) == test) {
				rb.Add(item);
				rs.Add(item.Header as string);
			}
		}
		*/

		/*
		public List<Fluent.IRibbonControl> GetQATButtonsSorted() {
			List<Fluent.IRibbonControl> rb = new List<Fluent.IRibbonControl>();
			List<string> rs = new List<string>();

			foreach (RibbonTabItem item in TheRibbon.Tabs) {
				foreach (RibbonGroupBox itg in item.Groups) {
					foreach (object ic in itg.Items) {
						if (ic is IRibbonControl) {
							if (TheRibbon.IsInQuickAccessToolBar(ic as UIElement)) {
								rb.Add(ic as IRibbonControl);
								rs.Add((ic as IRibbonControl).Header as string);
							}
						}
					}
				}
			}

			rs.Sort();
			return SortNames(rb, rs);
		}
		*/

		/*
		public List<IRibbonControl> GetQATButtons() {
			List<Fluent.IRibbonControl> rb = new List<Fluent.IRibbonControl>();

			foreach (UIElement item in TheRibbon.QuickAccessToolbarItems.Keys) {
				rb.Add(item as IRibbonControl);
			}

			return rb;
		}
		*/

		#endregion


		/*
		/// <summary>
		/// Occurs on layout change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_LayoutUpdated(object sender, EventArgs e) {
			PreviouseWindowState = WindowState;
		}
		*/

		/*
		void Explorer_ExplorerBrowserMouseLeave(object sender, EventArgs e) {
			//if (Itmpop != null)
			//{
			//    Itmpop.Visibility = System.Windows.Visibility.Hidden;
			//}
		}
		*/

		/*
		void ExplorerBrowserControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e) {
			//TheStatusBar.InvalidateVisual();
			//TheStatusBar.UpdateLayout();
		}
		*/

		/*
		void ExplorerBrowserControl_ClientSizeChanged(object sender, EventArgs e) {
			//TheStatusBar.InvalidateVisual();
			//TheStatusBar.UpdateLayout();
		}
		*/

		/*
		void ShellListView_ItemMiddleClick(object sender, NavigatedEventArgs e) {
			NewTab(e.Folder);
		}
		*/

		/*
		private void SetUpConsoleWindow(NavigatedEventArgs e) {
			ctrlConsole.ChangeFolder(e.Folder.ParsingName, e.Folder.IsFileSystem);


			/*
			try {
				//ctrlConsole.ChangeFolder(e.Folder.ParsingName);
				//return;

				var Folder = e.Folder.ParsingName;
				ctrlConsole.NewestFolder = Folder;
				if (e.Folder.IsFileSystem) {
					//ctrlConsole.WriteInput(String.Format("cd \"{0}\"", Folder), System.Drawing.Color.Red, false);
					ctrlConsole.ChangeFolder(String.Format("cd \"{0}\"", Folder));
				}
				else if (!ctrlConsole.InternalRichTextBox.Lines.Any()) { }
				else if (ctrlConsole.InternalRichTextBox.Lines.Last().Substring(0, ctrlConsole.InternalRichTextBox.Lines.Last().IndexOf(Char.Parse(@"\")) + 1) != Path.GetPathRoot(Folder)) {
					//ctrlConsole.WriteInput(Path.GetPathRoot(Folder).TrimEnd(Char.Parse(@"\")), System.Drawing.Color.Red, false);
					ctrlConsole.ChangeFolder(Path.GetPathRoot(Folder).TrimEnd(Char.Parse(@"\")));
				}
			}
			catch (Exception) {
				// catch all expetions for illigal path
			}
			-/
		}
		*/

		/*
		private void chkIsCompartibleRename_Unchecked(object sender, RoutedEventArgs e) {
			IsCompartibleRename = false;
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);
			rks.SetValue(@"CompartibleRename", 0);
			rks.Close();
			rk.Close();
		}
		*/


	}
}
