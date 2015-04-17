using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using System.Xml.Linq;
using BetterExplorer.Networks;
using BetterExplorer.UsbEject;
using BetterExplorerControls;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using Fluent;
using LTR.IO.ImDisk;
using Microsoft.Win32;
using NAppUpdate.Framework;
using SevenZip;
using Shell32;
using wyDay.Controls;
using Clipboards = System.Windows.Forms.Clipboard;
using ContextMenu = Fluent.ContextMenu;
using MenuItem = Fluent.MenuItem;
using Odyssey.Controls;
//using BExplorer.Shell.Taskbar;
using BExplorer.Shell.CommonFileDialogs;
using DropTargetHelper = BExplorer.Shell.DropTargetHelper.Get;

using WIN = System.Windows;
using BExplorer.Shell._Plugin_Interfaces;
using System.Windows.Data;


namespace BetterExplorer {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Fluent.RibbonWindow {

		[Obsolete("Try to remove. Items are added and removed but this has no functionality")]
		NetworkAccountManager nam = new NetworkAccountManager();

		#region DLLImports

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref BExplorer.Shell.DataObject.Win32Point pt);

		//[DllImport("user32.dll", SetLastError = true)]
		//private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

		//[DllImport("user32.dll", SetLastError = true)]
		//private static extern int UnregisterHotKey(IntPtr hwnd, int id);

		[DllImport("winmm.dll")]
		static extern Int32 mciSendString(String command, StringBuilder buffer, Int32 bufferSize, IntPtr hwndCallback);

		#endregion

		#region Private Members
		private bool IsrestoreTabs;
		private bool IsCalledFromLoading, isOnLoad;
		private bool asFolder = false, asImage = false, asArchive = false, asDrive = false, asApplication = false, asLibrary = false, asVirtualDrive = false;
		private MenuItem misa, misd, misag, misdg;
		private bool IsInfoPaneEnabled, IsConsoleShown, IsPreviewPaneEnabled;
		private int PreviewPaneWidth = 120, InfoPaneHeight = 150;
		private ShellTreeViewEx ShellTree = new ShellTreeViewEx();
		private ShellView ShellListView = new ShellView();
		private bool IsUpdateCheck;
		private bool IsUpdateCheckStartup;
		private ClipboardMonitor cbm = new ClipboardMonitor();
		private ContextMenu cmHistory = new ContextMenu();
		private WIN.Shell.JumpList AppJL = new WIN.Shell.JumpList();
		private IntPtr Handle;
		private List<string> Archives = new List<string>(new[] { ".rar", ".zip", ".7z", ".tar", ".gz", ".xz", ".bz2" });
		private List<string> Images = new List<string>(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".wmf" });
		private string SelectedArchive = "";
		private bool KeepBackstageOpen = false;
		bool canlogactions = false;
		string sessionid = DateTime.UtcNow.ToFileTimeUtc().ToString();
		string logdir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\ActionLog\\";
		string satdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\";
		string sstdir;
		bool OverwriteOnRotate = false;
		WIN.Forms.Timer updateCheckTimer = new WIN.Forms.Timer();
		DateTime LastUpdateCheck;
		Int32 UpdateCheckInterval;
		Double CommandPromptWinHeight;
		Boolean IsGlassOnRibonMinimized { get; set; }

		List<BExplorer.Shell.LVItemColor> LVItemsColor { get; set; }
		ContextMenu chcm;

		WIN.Forms.Timer focusTimer = new WIN.Forms.Timer() { Interval = 500 };

		#endregion

		public bool IsMultipleWindowsOpened { get; set; }

		#region Events

		private void btnConsolePane_Click(object sender, RoutedEventArgs e) {
			this.IsConsoleShown = btnConsolePane.IsChecked.Value;
			if (btnConsolePane.IsChecked.Value) {
				rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);
				rCommandPrompt.MinHeight = 100;
				spCommandPrompt.Height = GridLength.Auto;
				if (!ctrlConsole.IsProcessRunning)
					ctrlConsole.ChangeFolder(ShellListView.CurrentFolder.ParsingName, ShellListView.CurrentFolder.IsFileSystem);
			}
			else {
				rCommandPrompt.MinHeight = 0;
				rCommandPrompt.Height = new GridLength(0);
				spCommandPrompt.Height = new GridLength(0);
				ctrlConsole.StopProcess();
			}
		}

		private void btnAbout_Click(object sender, RoutedEventArgs e) {
			new fmAbout(this).ShowDialog();
		}

		private void btnBugtracker_Click(object sender, RoutedEventArgs e) {
			Process.Start("http://bugtracker.better-explorer.com");
		}

		private void backstage_IsOpenChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if (((Boolean)e.NewValue)) {
				this.ShellListView.IsFocusAllowed = false;
				backstage.Focus();
			}
			else {
				this.ShellListView.IsFocusAllowed = true;
			}
			autoUpdater.Visibility = Visibility.Visible;
			autoUpdater.UpdateLayout();

			if (KeepBackstageOpen) {
				backstage.IsOpen = true;
				KeepBackstageOpen = false;
			}
		}

		private void TheRibbon_SizeChanged(object sender, SizeChangedEventArgs e) {
			//if (TheRibbon.IsMinimized && this.IsGlassOnRibonMinimized) {
			//	WIN.Point p = ShellViewHost.TransformToAncestor(this).Transform(new WIN.Point(0, 0));
			//	this.GlassBorderThickness = new Thickness(8, this.WindowState == WindowState.Maximized ? p.Y : p.Y - 2, 8, 8);
			//} else if (this.IsGlassOnRibonMinimized) {
			//	WIN.Point p = backstage.TransformToAncestor(this).Transform(new WIN.Point(0, 0));
			//	this.GlassBorderThickness = new Thickness(8, p.Y + backstage.ActualHeight + 2, 8, 8);
			//}

			//try {
			//	this.SetBlur(!TheRibbon.IsMinimized);
			//} catch (Exception) {
			//}
		}

		private void TheRibbon_CustomizeQuickAccessToolbar(object sender, EventArgs e) {
			CustomizeQAT.Open(this, TheRibbon);
		}

		private void LoadInitialWindowPositionAndState() {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", false);

			if (rks == null) return;


			this.Width = Convert.ToDouble(rks.GetValue("LastWindowWidth", "640"));
			this.Height = Convert.ToDouble(rks.GetValue("LastWindowHeight", "480"));

			System.Drawing.Point Location = new System.Drawing.Point();
			try {
				Location = new System.Drawing.Point(
				  Convert.ToInt32(rks.GetValue("LastWindowPosLeft", "0")),
				  Convert.ToInt32(rks.GetValue("LastWindowPosTop", "0"))
				);
			}
			catch { }
			if (Location != null) {
				this.Left = Location.X;
				this.Top = Location.Y;
			}

			switch (Convert.ToInt32(rks.GetValue("LastWindowState"))) {
				case 2:
					this.WindowState = WindowState.Maximized;
					break;
				case 1:
					this.WindowState = WindowState.Minimized;
					break;
				case 0:
					this.WindowState = WindowState.Normal;
					break;
				default:
					this.WindowState = WindowState.Maximized;
					break;
			}

			int isGlassOnRibonMinimized = (int)rks.GetValue("RibbonMinimizedGlass", 1);
			this.IsGlassOnRibonMinimized = isGlassOnRibonMinimized == 1;
			chkRibbonMinimizedGlass.IsChecked = this.IsGlassOnRibonMinimized;

			TheRibbon.IsMinimized = Convert.ToBoolean(rks.GetValue("IsRibonMinimized", false));

			//CommandPrompt window size
			this.CommandPromptWinHeight = Convert.ToDouble(rks.GetValue("CmdWinHeight", 100));
			rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);

			if ((int)rks.GetValue("IsConsoleShown", 0) == 1) {
				rCommandPrompt.MinHeight = 100;
				rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);
				spCommandPrompt.Height = GridLength.Auto;
			}
			else {
				rCommandPrompt.MinHeight = 0;
				rCommandPrompt.Height = new GridLength(0);
				spCommandPrompt.Height = new GridLength(0);
			}

			rks.Close();
		}

		private void LoadColorCodesFromFile() {
			Task.Run(() => {
				var itemColorSettingsLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"BExplorer\itemcolors.cfg");

				if (File.Exists(itemColorSettingsLocation)) {
					var docs = XDocument.Load(itemColorSettingsLocation);

					this.LVItemsColor = docs.Root.Elements("ItemColorRow")
					  .Select(element => new BExplorer.Shell.LVItemColor(element.Elements().ToArray()[0].Value,
								   System.Drawing.Color.FromArgb(Convert.ToInt32(element.Elements().ToArray()[1].Value)))).ToList();
				}
			});
		}

		private void RibbonWindow_Initialized(object sender, EventArgs e) {
			LoadInitialWindowPositionAndState();
			LoadColorCodesFromFile();

			AppCommands.RoutedNewTab.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
			AppCommands.RoutedEnterInBreadCrumbCombo.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Alt));
			AppCommands.RoutedChangeTab.InputGestures.Add(new KeyGesture(Key.Tab, ModifierKeys.Control));
			AppCommands.RoutedCloseTab.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
			AppCommands.RoutedNavigateBack.InputGestures.Add(new KeyGesture(Key.Left, ModifierKeys.Alt));
			AppCommands.RoutedNavigateFF.InputGestures.Add(new KeyGesture(Key.Right, ModifierKeys.Alt));
			AppCommands.RoutedNavigateUp.InputGestures.Add(new KeyGesture(Key.Up, ModifierKeys.Alt));
			AppCommands.RoutedGotoSearch.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
		}

		#region ViewEnumerationComplete

		private void SetSortingAndGroupingButtons() {
			btnSort.Items.Clear();
			btnGroup.Items.Clear();

			try {
				foreach (Collumns item in ShellListView.Collumns.Where(x => x != null)) {
					var lastSortedColumn = ShellListView.Collumns.Where(w => w.ID == this.ShellListView.LastSortedColumnId).SingleOrDefault();
					if (lastSortedColumn != null) {
						var IsChecked1 = (item.pkey.fmtid == lastSortedColumn.pkey.fmtid) && (item.pkey.pid == lastSortedColumn.pkey.pid);
						btnSort.Items.Add(Utilities.Build_MenuItem(item.Name, item, checkable: true, isChecked: IsChecked1, GroupName: "GR2", onClick: mi_Click));
					}
					var IsCheckable2 = ShellListView.LastGroupCollumn != null && (item.pkey.fmtid == ShellListView.LastGroupCollumn.pkey.fmtid) && (item.pkey.pid == ShellListView.LastGroupCollumn.pkey.pid);
					btnGroup.Items.Add(Utilities.Build_MenuItem(item.Name, item, checkable: true, isChecked: IsCheckable2, GroupName: "GR3", onClick: mig_Click));
				}
			}
			catch (Exception ex) {
				//FIXME: I disable this message because of strange null after filter
				MessageBox.Show("BetterExplorer had an issue loading the visible columns for the current view. You might not be able to sort or group items.", ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
			}

			btnSort.Items.Add(new Separator() { Focusable = false });
			misa = Utilities.Build_MenuItem(FindResource("miAscending"), checkable: true, GroupName: "GR1", onClick: misa_Click);
			misd = Utilities.Build_MenuItem(FindResource("miDescending"), checkable: true, GroupName: "GR1", onClick: misd_Click);

			if (this.ShellListView.LastSortOrder == WIN.Forms.SortOrder.Ascending) {
				misa.IsChecked = true;
			}
			else {
				misd.IsChecked = true;
			}

			btnSort.Items.Add(misa);
			btnSort.Items.Add(misd);

			btnGroup.Items.Add(Utilities.Build_MenuItem("(none)", GroupName: "GR3", checkable: true, isChecked: ShellListView.LastGroupCollumn == null, onClick: misng_Click));
			btnGroup.Items.Add(new Separator());

			misag = Utilities.Build_MenuItem(FindResource("miAscending"), checkable: true, GroupName: "GR4", onClick: misag_Click);
			misdg = Utilities.Build_MenuItem(FindResource("miDescending"), checkable: true, GroupName: "GR4", onClick: misag_Click);

			if (this.ShellListView.LastGroupOrder == WIN.Forms.SortOrder.Ascending) {
				misag.IsChecked = true;
			}
			else {
				misdg.IsChecked = true;
			}

			btnGroup.Items.Add(misag);
			btnGroup.Items.Add(misdg);
		}

		void misag_Click(object sender, RoutedEventArgs e) {
			this.ShellListView.SetGroupOrder();
		}

		private void SetupColumnsButton() {
			var allAvailColls = this.ShellListView.AllAvailableColumns;
			btnMoreColls.Items.Clear();
			chcm.Items.Clear();

			for (int j = 1; j < 10; j++) {
				//TODO: Try to remove this Try Catch!!
				try {
					var IsChecked = ShellListView.Collumns.Any(col => col.pkey.fmtid == allAvailColls[j].pkey.fmtid && col.pkey.pid == allAvailColls[j].pkey.pid);
					btnMoreColls.Items.Add(
					  Utilities.Build_MenuItem(allAvailColls[j].Name, allAvailColls[j], checkable: true, onClick: mic_Click, isChecked: IsChecked)
					);
					chcm.Items.Add(
					  Utilities.Build_MenuItem(allAvailColls[j].Name, allAvailColls[j], checkable: true, onClick: mic_Click, isChecked: IsChecked)
					);
				}
				catch (Exception) {
				}
			}

			int ItemsCount = ShellListView.Items.Count;
			sbiItemsCount.Visibility = ItemsCount == 0 ? Visibility.Collapsed : Visibility.Visible;
			sbiItemsCount.Content = ItemsCount == 1 ? "1 item" : ItemsCount + " items";
			sbiSelItemsCount.Visibility = ShellListView.GetSelectedCount() == 0 ? Visibility.Collapsed : Visibility.Visible;
			spSelItems.Visibility = sbiSelItemsCount.Visibility;

			btnMoreColls.Items.Add(new Separator());
			btnMoreColls.Items.Add(Utilities.Build_MenuItem(FindResource("btnMoreColCP"), allAvailColls, onClick: micm_Click));
			btnMoreColls.Tag = allAvailColls;

			chcm.Items.Add(new Separator());
			chcm.Items.Add(Utilities.Build_MenuItem(FindResource("btnMoreColCP"), allAvailColls, onClick: micm_Click));
		}

		#endregion

		void misd_Click(object sender, RoutedEventArgs e) {
			foreach (var item in btnSort.Items.OfType<MenuItem>().Where(item => item.IsChecked && item != (sender as MenuItem))) {
				ShellListView.SetSortCollumn((Collumns)item.Tag, WIN.Forms.SortOrder.Descending);
			}
		}

		void misa_Click(object sender, RoutedEventArgs e) {
			foreach (var item in btnSort.Items.OfType<MenuItem>().Where(item => item.IsChecked && item != (sender as MenuItem))) {
				ShellListView.SetSortCollumn((Collumns)item.Tag, WIN.Forms.SortOrder.Ascending);
			}
		}

		void micm_Click(object sender, RoutedEventArgs e) {
			var fMoreCollumns = new MoreColumns();
			fMoreCollumns.PopulateAvailableColumns((List<Collumns>)(sender as FrameworkElement).Tag, ShellListView, this.PointToScreen(Mouse.GetPosition(this)));
		}

		void mic_Click(object sender, RoutedEventArgs e) {
			var mi = (sender as MenuItem);
			Collumns col = (Collumns)mi.Tag;
			ShellListView.SetColInView(col, !mi.IsChecked);
		}

		void miItem_Click(object sender, RoutedEventArgs e) {
			MenuItem mi = sender as MenuItem;
			ShellItem SaveLoc = mi.Tag as ShellItem;

			if (ShellListView.CurrentFolder.ParsingName.Contains(KnownFolders.Libraries.ParsingName) && ShellListView.CurrentFolder.ParsingName.EndsWith("library-ms")) {
				var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(ShellListView.CurrentFolder.ParsingName), false);
				lib.DefaultSaveFolder = SaveLoc.ParsingName;
				lib.Close();
			}
			else if (ShellListView.GetFirstSelectedItem().ParsingName.Contains(KnownFolders.Libraries.ParsingName)) {
				var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(ShellListView.GetFirstSelectedItem().ParsingName), false);
				lib.DefaultSaveFolder = SaveLoc.ParsingName;
				lib.Close();
			}
		}

		void LinksFolderWarcher_Renamed(object sender, RenamedEventArgs e) {
			Dispatcher.BeginInvoke(DispatcherPriority.Normal,
			  (Action)(() => {
				  foreach (MenuItem item in btnFavorites.Items.OfType<MenuItem>()) {
					  if ((item.Tag as ShellItem).ParsingName == e.OldFullPath)
						  item.Header = Path.GetFileNameWithoutExtension(e.Name);
				  }
			  }));
		}

		void LinksFolderWarcher_Deleted(object sender, FileSystemEventArgs e) {
			Dispatcher.BeginInvoke(DispatcherPriority.Normal,
			  (Action)(() => {
				  /*
				  MenuItem ItemForRemove = null;
				  foreach (MenuItem item in btnFavorites.Items.OfType<MenuItem>()) {
				  if (item.Header.ToString() == Path.GetFileNameWithoutExtension(e.Name))
					ItemForRemove = item;
				  }
				  btnFavorites.Items.Remove(ItemForRemove);
				  */

				  btnFavorites.Items.Remove(btnFavorites.Items.OfType<MenuItem>().Single(item => item.Header.ToString() == Path.GetFileNameWithoutExtension(e.Name)));

			  }));
		}

		void LinksFolderWarcher_Created(object sender, FileSystemEventArgs e) {
			Dispatcher.BeginInvoke(DispatcherPriority.Normal,
			  (Action)(() => {
				  if (Path.GetExtension(e.FullPath).ToLowerInvariant() == ".lnk") {
					  var so = new ShellItem(e.FullPath);
					  so.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					  so.Thumbnail.CurrentSize = new WIN.Size(16, 16);
					  ImageSource icon = so.Thumbnail.BitmapSource;

					  btnFavorites.Items.Add(Utilities.Build_MenuItem(so.DisplayName, so, icon, onClick: mif_Click));
				  }
			  }));
		}

		private void SetUpRibbonTabsVisibilityOnSelectOrNavigate(int selectedItemsCount, IListItemEx selectedItem) {

			#region Search Contextual Tab
			ctgSearch.Visibility = BooleanToVisibiliy(ShellListView.CurrentFolder.IsSearchFolder);
			if (ctgSearch.Visibility == Visibility.Visible && !ShellListView.CurrentFolder.IsSearchFolder) {
				ctgSearch.Visibility = Visibility.Collapsed;
				TheRibbon.SelectedTabItem = HomeTab;
			}
			#endregion

			#region Folder Tools Context Tab
			ctgFolderTools.Visibility = BooleanToVisibiliy((selectedItemsCount == 1 && selectedItem.IsFolder && selectedItem.IsFileSystem && !selectedItem.IsDrive && !selectedItem.IsNetworkPath));

			if (asFolder && ctgFolderTools.Visibility == Visibility.Visible) {
				TheRibbon.SelectedTabItem = ctgFolderTools.Items[0];
			}
			#endregion

			#region Drive Contextual Tab
			ctgDrive.Visibility = BooleanToVisibiliy(ShellListView.CurrentFolder.IsDrive || (selectedItemsCount == 1 && selectedItem != null && (selectedItem.IsDrive || (selectedItem.Parent != null && selectedItem.Parent.IsDrive))));
			if (asDrive && ctgDrive.Visibility == Visibility.Visible && selectedItem != null && selectedItem.IsDrive) {
				TheRibbon.SelectedTabItem = ctgDrive.Items[0];
			}
			#endregion

			#region Library Context Tab
			ctgLibraries.Visibility = BooleanToVisibiliy((selectedItemsCount == 1 && ShellListView.CurrentFolder.Equals(KnownFolders.Libraries)) || (selectedItemsCount == 1 && selectedItem.Parent != null && selectedItem.Parent.Equals(KnownFolders.Libraries)));

			if (ctgLibraries.Visibility == Visibility.Visible && asLibrary) {
				TheRibbon.SelectedTabItem = ctgLibraries.Items[0];
			}

			if (ctgLibraries.Visibility == Visibility.Visible && ShellListView.CurrentFolder.Equals(KnownFolders.Libraries)) {
				if (selectedItem != null && selectedItemsCount == 1)
					SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(selectedItem.ParsingName), false));
			}
			else if (ctgLibraries.Visibility == Visibility.Visible && ShellListView.CurrentFolder.Parent.Equals(KnownFolders.Libraries)) {
				if (selectedItemsCount == 1)
					SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(ShellListView.CurrentFolder.ParsingName), false));
			}
			if (selectedItemsCount == 0) {
				ctgLibraries.Visibility = Visibility.Collapsed;
			}
			#endregion

			#region Archive Contextual Tab
			ctgArchive.Visibility = WIN.Visibility.Collapsed; //TODO: Restore this: BooleanToVisibiliy(selectedItemsCount == 1 && Archives.Contains(Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant()));
			if (asArchive && ctgArchive.Visibility == Visibility.Visible)
				TheRibbon.SelectedTabItem = ctgArchive.Items[0];

			#endregion

			#region Application Context Tab
			ctgExe.Visibility = BooleanToVisibiliy(selectedItemsCount == 1 && !selectedItem.IsFolder && (Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant() == ".exe" || Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant() == ".msi"));
			if (asApplication && ctgExe.Visibility == Visibility.Visible) {
				TheRibbon.SelectedTabItem = ctgExe.Items[0];
			}
			#endregion

			#region Image Context Tab
			ctgImage.Visibility = BooleanToVisibiliy(selectedItemsCount == 1 && !selectedItem.IsFolder && Images.Contains(Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant()));
			if (ctgImage.Visibility == Visibility.Visible) {
				try {
					using (var cvt = new Bitmap(selectedItem.ParsingName)) {

						imgSizeDisplay.WidthData = cvt.Width.ToString();
						imgSizeDisplay.HeightData = cvt.Height.ToString();

						if (asImage) TheRibbon.SelectedTabItem = ctgImage.Items[0];
					}
				}
				catch (Exception) {
					MessageBox.Show("Image was invalid");
				}
			}
			#endregion

			#region Virtual Disk Context Tab
			ctgVirtualDisk.Visibility = BooleanToVisibiliy(selectedItemsCount == 1 && !selectedItem.IsFolder && Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant() == ".iso");
			if (asVirtualDrive && ctgVirtualDisk.Visibility == Visibility.Visible) {
				TheRibbon.SelectedTabItem = ctgVirtualDisk.Items[0];
			}
			#endregion

		}

		private void SetUpStatusBarOnSelectOrNavigate(int selectedItemsCount) {
			spSelItems.Visibility = BooleanToVisibiliy(selectedItemsCount > 0);
			sbiSelItemsCount.Visibility = BooleanToVisibiliy(selectedItemsCount > 0);
			if (selectedItemsCount == 1)
				sbiSelItemsCount.Content = "1 item selected";
			else if (selectedItemsCount > 1)
				sbiSelItemsCount.Content = selectedItemsCount.ToString() + " items selected";
		}

		private void SetUpButtonsStateOnSelectOrNavigate(int selectedItemsCount, IListItemEx selectedItem) {
			btnCopy.IsEnabled = selectedItemsCount > 0;
			btnCopyto.IsEnabled = selectedItemsCount > 0;
			btnMoveto.IsEnabled = selectedItemsCount > 0;
			btnCut.IsEnabled = selectedItemsCount > 0;
			btnDelete.IsEnabled = selectedItem != null && selectedItem.IsFileSystem;
			btnRename.IsEnabled = selectedItem != null && (selectedItem.IsFileSystem || (selectedItem.Parent != null && selectedItem.Parent.Equals(KnownFolders.Libraries)));
			btnProperties3.IsEnabled = selectedItemsCount > 0;
			if (selectedItem != null) {
				var rg = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + Path.GetExtension(selectedItem.ParsingName) + @"\OpenWithProgids");
				if (rg == null)
					btnEdit.IsEnabled = false;
				else {
					string filetype = rg.GetValueNames()[0];
					rg.Close();

					using (var rgtype = Registry.ClassesRoot.OpenSubKey(filetype + @"\shell\edit\command")) {
						btnEdit.IsEnabled = !(rgtype == null);
					}
				}
			}

			btnSelAll.IsEnabled = selectedItemsCount != ShellListView.Items.Count;
			btnSelNone.IsEnabled = selectedItemsCount > 0;
			btnShare.IsEnabled = selectedItemsCount == 1 && selectedItem.IsFolder;
			btnAdvancedSecurity.IsEnabled = selectedItemsCount == 1;
			btnHideSelItems.IsEnabled = ShellListView.CurrentFolder.IsFileSystem;
		}

		private void SetupLibrariesTab(ShellLibrary lib) {
			IsFromSelectionOrNavigation = true;
			chkPinNav.IsChecked = lib.IsPinnedToNavigationPane;
			IsFromSelectionOrNavigation = false;

			foreach (ShellItem item in lib) {
				item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				item.Thumbnail.CurrentSize = new WIN.Size(16, 16);

				btnDefSave.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.Thumbnail.BitmapSource, GroupName: "GRDS1", checkable: true,
							isChecked: item.ParsingName == lib.DefaultSaveFolder, onClick: miItem_Click));
			}

			btnDefSave.IsEnabled = lib.Count != 0;
			lib.Close();
		}

		private void SetupUIOnSelectOrNavigate() {
			Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
				btnDefSave.Items.Clear();
				var selItemsCount = ShellListView.GetSelectedCount();
				var selectedItem = this.ShellListView.GetFirstSelectedItem();
				if (selectedItem != null) {
					var mnu = new ShellContextMenu(this.ShellListView, 1);

					try {
						var tempPoint = btnOpenWith.PointToScreen(new WIN.Point(0, 0));
						var itemMenuCount = mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)btnOpenWith.ActualHeight), 1, false);

						btnOpenWith.IsEnabled = itemMenuCount > 0 && selItemsCount == 1;
					}
					catch (Exception) {

						btnOpenWith.IsEnabled = false;
					}
				}
				else {
					btnOpenWith.IsEnabled = false;
				}
				btnNewItem.IsEnabled = this.ShellListView.CurrentFolder.IsFileSystem || this.ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName;
				if (selectedItem != null && selectedItem.IsFileSystem && IsPreviewPaneEnabled && !selectedItem.IsFolder && selItemsCount == 1) {
					this.Previewer.FileName = selectedItem.ParsingName;
				}
				else if (!String.IsNullOrEmpty(this.Previewer.FileName)) {
					this.Previewer.FileName = null;
				}
				//Set up ribbon contextual tabs on selection changed
				SetUpRibbonTabsVisibilityOnSelectOrNavigate(selItemsCount, selectedItem);
				SetUpButtonsStateOnSelectOrNavigate(selItemsCount, selectedItem);
			}));
		}

		bool IsFromSelectionOrNavigation = false;

		void cbm_ClipboardChanged(object sender, Tuple<WIN.Forms.IDataObject> e) {
			btnPaste.IsEnabled = e.Item1.GetDataPresent(DataFormats.FileDrop) || e.Item1.GetDataPresent("Shell IDList Array");
			btnPasetShC.IsEnabled = e.Item1.GetDataPresent(DataFormats.FileDrop) || e.Item1.GetDataPresent("Shell IDList Array");
		}

		#endregion

		#region Conditional Select

		private void miSelAllByType_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetSelectedCount() > 0) {
				var typePK = new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 4 };
				var flt = ShellListView.SelectedItems.Select(item => item.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant());
				var items = ShellListView.Items.Where(w => flt.Contains(w.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant())).ToArray();
				ShellListView.SelectItems(items);
				btnCondSel.IsDropDownOpen = false;
			}
		}

		private void miSelAllByDate_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetSelectedCount() > 0) {
				var typePK = new PROPERTYKEY() { fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 15 };
				var flt = ShellListView.SelectedItems.Select(item => DateTime.Parse(item.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant()).Date);
				var items = ShellListView.Items.Where(w => flt.Contains(DateTime.Parse(w.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant()).Date)).ToArray();
				ShellListView.SelectItems(items);
				btnCondSel.IsDropDownOpen = false;
			}
		}

		private void btnCondSel_Click(object sender, RoutedEventArgs e) {
			btnCondSel.IsDropDownOpen = false;
			ConditionalSelectForm.Open(ShellListView);
		}

		#endregion

		#region Size Chart

		private void btnFSizeChart_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetSelectedCount() > 0) {
				if ((ShellListView.GetFirstSelectedItem().IsFolder || ShellListView.GetFirstSelectedItem().IsDrive) && ShellListView.GetFirstSelectedItem().IsFileSystem) {
					FolderSizeWindow.Open(ShellListView.GetFirstSelectedItem().ParsingName, this);
					return;
				}
			}
			else if ((ShellListView.CurrentFolder.IsFolder || ShellListView.CurrentFolder.IsDrive) && ShellListView.CurrentFolder.IsFileSystem) {
				FolderSizeWindow.Open(ShellListView.CurrentFolder.ParsingName, this);
			}
		}

		private void btnSizeChart_Click(object sender, RoutedEventArgs e) {
			FolderSizeWindow.Open(ShellListView.CurrentFolder.ParsingName, this);
		}

		#endregion

		#region Home Tab

		private void miJunctionpoint_Click(object sender, RoutedEventArgs e) {
			string PathForDrop = ShellListView.CurrentFolder.ParsingName.Replace(@"\\", @"\");
			var files = new String[0];
			if (Clipboards.ContainsData("Shell IDList Array"))
				files = Clipboards.GetDataObject().ToShellItemArray().ToArray().Select(s => new ShellItem(s).ParsingName).ToArray();
			else
				files = Clipboards.GetFileDropList().OfType<string>().ToArray();

			foreach (string item in files) {
				var o = new ShellItem(item);
				JunctionPointUtils.JunctionPoint.Create(String.Format(@"{0}\{1}", PathForDrop, o.GetDisplayName(SIGDN.NORMALDISPLAY)), o.ParsingName, true);
				AddToLog(String.Format(@"Created Junction Point at {0}\{1} linked to {2}", PathForDrop, o.GetDisplayName(SIGDN.NORMALDISPLAY), o.ParsingName));
			}
		}

		private void miCreateSymlink_Click(object sender, RoutedEventArgs e) {
			StringCollection DropList = Clipboards.GetFileDropList();
			string PathForDrop = ShellListView.CurrentFolder.ParsingName.Replace(@"\\", @"\");
			string ExePath = Utilities.AppDirectoryItem("BetterExplorerOperations.exe");
			int winhandle = (int)User32.getWindowId(null, "BetterExplorerOperations");
			var items = new List<IListItemEx>();// new List<ShellItem>(DropList.OfType<string>().Select(o => new ShellItem(o)));

			string sources = PathStringCombiner.CombinePaths(items, ";", true);
			string drops = PathStringCombiner.CombinePathsWithSinglePath(PathForDrop + @"\", items, false);

			foreach (var item in items) {
				string source = item.ParsingName.Replace(@"\\", @"\");
				string drop = String.Format(@"{0}\{1}", PathForDrop, item.GetDisplayName(SIGDN.NORMALDISPLAY));
			}

			using (var proc = new Process()) {
				proc.StartInfo = new ProcessStartInfo {
					FileName = ExePath,
					Verb = "runas",
					UseShellExecute = true,
					Arguments = String.Format("/env /user:Administrator \"{0}\"", ExePath)
				};

				proc.Start();
				Thread.Sleep(1000);
				proc.WaitForExit();
				if (proc.ExitCode == 1)
					MessageBox.Show("Error in creating symlink", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void btnHistory_Click(object sender, RoutedEventArgs e) {
			ShellListView.ShowPropPage(this.Handle, ShellListView.GetFirstSelectedItem().ParsingName,
			  User32.LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "twext.dll"), 1024, "Previous Versions"));
		}

		private void btnBackstageExit_Click(object sender, RoutedEventArgs e) {
			//! We call Shutdown() so to explicit shutdown the app regardless of windows closing cancel flag.
			if (this.IsMultipleWindowsOpened)
				this.Close();
			else
				Application.Current.Shutdown();
		}

		private void btnNewWindow_Click(object sender, RoutedEventArgs e) {
			// creates a new window
			Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, "/nw");
		}

		void miow_Click(object sender, RoutedEventArgs e) {
			var assocItem = (AssociationItem)(sender as MenuItem).Tag;
			assocItem.Invoke();
		}

		void mif_Click(object sender, RoutedEventArgs e) {
			using (var obj = (sender as MenuItem).Tag as ShellItem)
			using (var lnk = new ShellLink(obj.ParsingName)) {
				NavigationController(FileSystemListItem.ToFileSystemItem(this.ShellListView.LVHandle, lnk.TargetPIDL));
			}
		}

		private void btnCopy_Click(object sender, RoutedEventArgs e) {
			var sc = new StringCollection();
			sc.AddRange(ShellListView.SelectedItems.Select(x => x.ParsingName).ToArray());
			Clipboards.SetFileDropList(sc);
		}

		private void btnPaste_Click(object sender, RoutedEventArgs e) {
			ShellListView.PasteAvailableFiles();
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e) {
			MenuItem_Click(sender, e);
		}

		private void btnRename_Click(object sender, RoutedEventArgs e) {
			ShellListView.RenameSelectedItem();
		}

		private void btnPathCopy_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.SelectedItems.Count > 1)
				Clipboards.SetText(ShellListView.SelectedItems.Select(item => "\r\n" + item.ParsingName).Aggregate((x, y) => x + y).Trim());
			else if (ShellListView.SelectedItems.Count == 1)
				Clipboards.SetText(ShellListView.GetFirstSelectedItem().ParsingName);
			else
				Clipboards.SetText(ShellListView.CurrentFolder.ParsingName);
		}

		private void btnSelAll_Click(object sender, RoutedEventArgs e) {
			ShellListView.SelectAll();
		}

		private void btnSelNone_Click(object sender, RoutedEventArgs e) {
			ShellListView.DeSelectAllItems();
		}

		// Delete > Send to Recycle Bin
		private void MenuItem_Click(object sender, RoutedEventArgs e) {
			ShellListView.DeleteSelectedFiles(true);
		}

		// Delete > Permanently Delete
		private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
			ShellListView.DeleteSelectedFiles(false);
		}

		// New Folder/Library
		private void btnNewFolder_Library(object sender, RoutedEventArgs e) {
			//We should focus the ListView or on some circumstances new folder does not start renaming after folder is created
			this.ShellListView.Focus();
			ShellListView.IsRenameNeeded = true;

			if (ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName)
				ShellListView.CreateNewLibrary(FindResource("btnNewLibraryCP").ToString());
			else
				ShellListView.CreateNewFolder(FindResource("btnNewFolderCP").ToString());
		}

		private void btnProperties_Click(object sender, RoutedEventArgs e) {
			ShellListView.ShowPropPage(this.Handle, ShellListView.GetFirstSelectedItem().ParsingName, "");
		}

		private void btnInvSel_Click(object sender, RoutedEventArgs e) {
			ShellListView.InvertSelection();
		}

		private void btnPasetShC_Click(object sender, RoutedEventArgs e) {
			StringCollection DropList = Clipboards.GetFileDropList();
			string PathForDrop = ShellListView.CurrentFolder.ParsingName;
			foreach (string item in DropList) {
				using (var shortcut = new ShellLink()) {
					var o = new ShellItem(item);
					shortcut.Target = item;
					shortcut.WorkingDirectory = Path.GetDirectoryName(item);
					shortcut.Description = o.GetDisplayName(SIGDN.NORMALDISPLAY);
					shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
					shortcut.Save(String.Format("{0}\\{1}.lnk", PathForDrop, o.GetDisplayName(SIGDN.NORMALDISPLAY)));
					AddToLog(String.Format("Shortcut created at {0}\\{1} from source {2}", PathForDrop, o.GetDisplayName(SIGDN.NORMALDISPLAY), item));
				}
			}
		}

		private void btnctDocuments_Click(object sender, RoutedEventArgs e) {
			SetFOperation(KnownFolders.Documents.ParsingName, BExplorer.Shell.OperationType.Copy);
		}

		private void btnctDesktop_Click(object sender, RoutedEventArgs e) {
			SetFOperation(KnownFolders.Desktop.ParsingName, BExplorer.Shell.OperationType.Copy);
		}

		private void btnctDounloads_Click(object sender, RoutedEventArgs e) {
			SetFOperation(KnownFolders.Downloads.ParsingName, BExplorer.Shell.OperationType.Copy);
		}

		private void btnmtDocuments_Click(object sender, RoutedEventArgs e) {
			SetFOperation(KnownFolders.Documents.ParsingName, BExplorer.Shell.OperationType.Move);
		}

		private void btnmtDesktop_Click(object sender, RoutedEventArgs e) {
			SetFOperation(KnownFolders.Desktop.ParsingName, BExplorer.Shell.OperationType.Move);
		}

		private void btnmtDounloads_Click(object sender, RoutedEventArgs e) {
			SetFOperation(KnownFolders.Downloads.ParsingName, BExplorer.Shell.OperationType.Move);
		}

		private void btnmtOther_Click(object sender, RoutedEventArgs e) {
			var dlg = new FolderSelectDialog();
			if (dlg.ShowDialog()) {
				SetFOperation(dlg.FileName, BExplorer.Shell.OperationType.Move);
			}
		}

		private void SetFOperation(String fileName, BExplorer.Shell.OperationType opType) {
			var obj = FileSystemListItem.ToFileSystemItem(this.ShellListView.LVHandle, fileName.ToShellParsingName());
			if (opType == BExplorer.Shell.OperationType.Copy)
				ShellListView.DoCopy(obj);
			else if (opType == BExplorer.Shell.OperationType.Move)
				ShellListView.DoMove(obj);
		}

		private void SetFOperation(IListItemEx obj, BExplorer.Shell.OperationType opType) {
			if (opType == BExplorer.Shell.OperationType.Copy)
				ShellListView.DoCopy(obj);
			else if (opType == BExplorer.Shell.OperationType.Move)
				ShellListView.DoMove(obj);
		}

		private void btnctOther_Click(object sender, RoutedEventArgs e) {
			var dlg = new FolderSelectDialog();
			if (dlg.ShowDialog() == true) {
				SetFOperation(dlg.FileName, BExplorer.Shell.OperationType.Copy);
			}
			ShellListView.Focus();
		}

		private void btnCopyto_Click(object sender, RoutedEventArgs e) {
			btnctOther_Click(sender, e);
		}

		private void btnMoveto_Click(object sender, RoutedEventArgs e) {
			btnmtOther_Click(sender, e);
		}

		private void btnCut_Click(object sender, RoutedEventArgs e) {
			ShellListView.CutSelectedFiles();
		}

		private void btnNewItem_Click(object sender, RoutedEventArgs e) {
			var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = 0 };
			BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS, true);
		}

		private void btnOpenWith_Click(object sender, RoutedEventArgs e) {
			Process.Start(String.Format("\"{0}\"", ShellListView.GetFirstSelectedItem().ParsingName));
		}

		private void btnEdit_Click(object sender, RoutedEventArgs e) {
			new Process() {
				StartInfo = new ProcessStartInfo {
					FileName = ShellListView.GetFirstSelectedItem().FileSystemPath,
					Verb = "edit",
					UseShellExecute = true,
				}
			}.Start();
		}

		private void btnFavorites_Click(object sender, RoutedEventArgs e) {
			var selectedItems = ShellListView.SelectedItems;
			if (selectedItems.Count == 1) {
				var link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.GetFirstSelectedItem().ParsingName;
				link.Save(String.Format(@"{0}\{1}.lnk", KnownFolders.Links.ParsingName, ShellListView.GetFirstSelectedItem().DisplayName));
				link.Dispose();
			}

			if (selectedItems.Count == 0) {
				var link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.CurrentFolder.ParsingName;
				link.Save(String.Format(@"{0}\{1}.lnk", KnownFolders.Links.ParsingName, ShellListView.CurrentFolder.DisplayName));
				link.Dispose();
			}
		}

		#endregion

		#region Drive Tools / Virtual Disk Tools

		private void btnFormatDrive_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Are you sure you want to do this?", FindResource("btnFormatDriveCP").ToString(), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
				var formatDriveThread = new Thread(() => ShellListView.FormatDrive(IntPtr.Zero));
				formatDriveThread.Start();
			}
		}

		private void btnCleanDrive_Click(object sender, RoutedEventArgs e) {
			ShellListView.CleanupDrive();
		}

		private void btnDefragDrive_Click(object sender, RoutedEventArgs e) {
			ShellListView.DefragDrive();
		}

		private char GetDriveLetterFromDrivePath(string path) {
			return path.Substring(0, 1).ToCharArray()[0];
		}

		private void OpenCDTray(char DriveLetter) {
			mciSendString(String.Format("open {0}: type CDAudio alias drive{1}", DriveLetter, DriveLetter), null, 0, IntPtr.Zero);
			mciSendString(String.Format("set drive{0} door open", DriveLetter), null, 0, IntPtr.Zero);
		}

		private void CloseCDTray(char DriveLetter) {
			mciSendString(String.Format("open {0}: type CDAudio alias drive{1}", DriveLetter, DriveLetter), null, 0, IntPtr.Zero);
			mciSendString(String.Format("set drive{0} door closed", DriveLetter), null, 0, IntPtr.Zero);
		}

		private void btnOpenTray_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetFirstSelectedItem().GetDriveInfo() != null) {
				if (ShellListView.GetFirstSelectedItem().GetDriveInfo().DriveType == DriveType.CDRom) {
					OpenCDTray(GetDriveLetterFromDrivePath(ShellListView.GetFirstSelectedItem().ParsingName));
				}
			}
		}

		private void btnCloseTray_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetFirstSelectedItem().GetDriveInfo() != null) {
				if (ShellListView.GetFirstSelectedItem().GetDriveInfo().DriveType == DriveType.CDRom) {
					CloseCDTray(GetDriveLetterFromDrivePath(ShellListView.GetFirstSelectedItem().ParsingName));
				}
			}
		}

		private void EjectDisk(char DriveLetter) {
			Thread t = new Thread(() => {
				Thread.Sleep(10);
				var vdc = new VolumeDeviceClass();
				foreach (Volume item in vdc.Devices) {
					if (GetDriveLetterFromDrivePath(item.LogicalDrive) == DriveLetter) {
						var veto = item.Eject(false);
						if (veto != BetterExplorer.UsbEject.Native.PNP_VETO_TYPE.TypeUnknown) {
							if (veto == BetterExplorer.UsbEject.Native.PNP_VETO_TYPE.Ok) {
								Dispatcher.BeginInvoke(WIN.Threading.DispatcherPriority.Normal,
									(Action)(() => {
										this.beNotifyIcon.ShowBalloonTip("Information", String.Format("It is safe to remove {0}", item.LogicalDrive), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
										var tabsForRemove = tcMain.Items.OfType<Wpf.Controls.TabItem>()
										  .Where(w => {
											  var root = Path.GetPathRoot(w.ShellObject.ParsingName.ToShellParsingName());
											  return root != null && (w.ShellObject.IsFileSystem &&
													root.ToLowerInvariant() == String.Format("{0}:\\", DriveLetter).ToLowerInvariant());
										  }).ToArray();
										foreach (Wpf.Controls.TabItem tab in tabsForRemove) {
											tcMain.RemoveTabItem(tab);
										}
									}));
							}
							else {
								var message = String.Empty;
								var obj = new ShellItem(item.LogicalDrive);
								switch (veto) {
									case Native.PNP_VETO_TYPE.Ok:
										break;
									case Native.PNP_VETO_TYPE.TypeUnknown:
										break;
									case Native.PNP_VETO_TYPE.LegacyDevice:
										break;
									case Native.PNP_VETO_TYPE.PendingClose:
										break;
									case Native.PNP_VETO_TYPE.WindowsApp:
										break;
									case Native.PNP_VETO_TYPE.WindowsService:
										break;
									case Native.PNP_VETO_TYPE.OutstandingOpen:
										message = String.Format("The device {0} can not be disconnected because is in use!", obj.GetDisplayName(SIGDN.NORMALDISPLAY));
										break;
									case Native.PNP_VETO_TYPE.Device:
										break;
									case Native.PNP_VETO_TYPE.Driver:
										break;
									case Native.PNP_VETO_TYPE.IllegalDeviceRequest:
										break;
									case Native.PNP_VETO_TYPE.InsufficientPower:
										break;
									case Native.PNP_VETO_TYPE.NonDisableable:
										message = String.Format("The device {0} does not support disconnecting!", obj.GetDisplayName(SIGDN.NORMALDISPLAY));
										break;
									case Native.PNP_VETO_TYPE.LegacyDriver:
										break;
									default:
										break;
								}
								Dispatcher.BeginInvoke(WIN.Threading.DispatcherPriority.Normal,
									(Action)(() => this.beNotifyIcon.ShowBalloonTip("Error", message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error)));
							}
						}
						break;
					}
				}
			});
			t.Start();
		}

		private void btnEjectDevice_Click(object sender, RoutedEventArgs e) {
			var firstSelectedItem = ShellListView.GetFirstSelectedItem();
			if (firstSelectedItem.GetDriveInfo() != null) {
				if (firstSelectedItem.GetDriveInfo().DriveType == DriveType.Removable || firstSelectedItem.GetDriveInfo().DriveType == DriveType.Fixed) {
					EjectDisk(GetDriveLetterFromDrivePath(firstSelectedItem.ParsingName));
				}
			}
		}

		// Virtual Disk Tools
		private bool CheckImDiskInstalled() {
			try {
				ImDiskAPI.GetDeviceList();
				return true;
			}
			catch (System.DllNotFoundException) {
				return false;
			}
		}

		public void ShowInstallImDiskMessage() {
			if (MessageBox.Show("It appears you do not have the ImDisk Virtual Disk Driver installed. This driver is used to power Better Explorer's ISO-mounting features. \n\nWould you like to visit ImDisk's website to install the product? (http://www.ltr-data.se/opencode.html/)", "ImDisk Not Found", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
				Process.Start("http://www.ltr-data.se/opencode.html/#ImDisk");
			}
		}

		private void btnAdvMountIso_Click(object sender, RoutedEventArgs e) {
			if (!CheckImDiskInstalled()) {
				ShowInstallImDiskMessage();
				return;
			}

			var mi = new MountIso() { Owner = this };
			mi.ShowDialog();
			if (mi.yep) {
				string DriveLetter = String.Format("{0}:", mi.chkPreselect.IsChecked == true ? ImDiskAPI.FindFreeDriveLetter() : (char)mi.cbbLetter.SelectedItem);
				long size = mi.chkPresized.IsChecked == true ? 0 : Convert.ToInt64(mi.txtSize.Text);

				ImDiskFlags imflags;
				switch (mi.cbbType.SelectedIndex) {
					case 0:
						//Hard Drive
						imflags = ImDiskFlags.DeviceTypeHD;
						break;
					case 1:
						// CD/DVD
						imflags = ImDiskFlags.DeviceTypeCD;
						break;
					case 2:
						// Floppy Disk
						imflags = ImDiskFlags.DeviceTypeFD;
						break;
					case 3:
						// Raw Data
						imflags = ImDiskFlags.DeviceTypeRAW;
						break;
					default:
						imflags = ImDiskFlags.DeviceTypeCD;
						break;
				}

				switch (mi.cbbAccess.SelectedIndex) {
					case 0:
						// Access directly
						imflags |= ImDiskFlags.FileTypeDirect;
						break;
					case 1:
						// Copy to memory
						imflags |= ImDiskFlags.FileTypeAwe;
						break;
					default:
						imflags |= ImDiskFlags.FileTypeDirect;
						break;
				}

				if (mi.chkRemovable.IsChecked == true)
					imflags |= ImDiskFlags.Removable;
				if (mi.chkReadOnly.IsChecked == true)
					imflags |= ImDiskFlags.ReadOnly;

				ImDiskAPI.CreateDevice(size, 0, 0, 0, 0, imflags, ShellListView.GetFirstSelectedItem().ParsingName, false, DriveLetter, IntPtr.Zero);
			}
		}

		private void btnMountIso_Click(object sender, RoutedEventArgs e) {
			try {
				//TO_DO: add the code for mounting images with ImDisk. look the example below!

				var freeDriveLetter = String.Format("{0}:", ImDiskAPI.FindFreeDriveLetter());
				ImDiskAPI.CreateDevice(0, 0, 0, 0, 0, ImDiskFlags.Auto, ShellListView.GetFirstSelectedItem().ParsingName, false, freeDriveLetter, IntPtr.Zero);
			}
			catch (System.DllNotFoundException) {
				ShowInstallImDiskMessage();
			}
			catch (Exception ex) {
				MessageBox.Show("An error occurred while trying to mount this file. \n\n" + ex.Message, ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void btnWriteIso_Click(object sender, RoutedEventArgs e) {
			Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "isoburn.exe"),
					 string.Format("\"{0}\"", ShellListView.GetFirstSelectedItem().ParsingName));
		}

		private void btnUnmountDrive_Click(object sender, RoutedEventArgs e) {
			//SelectedDriveID was NEVER anything but 0
			uint SelectedDriveID = 0;

			try {
				if (!CheckImDiskInstalled())
					ShowInstallImDiskMessage();
				else if ((ImDiskAPI.QueryDevice(SelectedDriveID).Flags & ImDiskFlags.DeviceTypeCD) != 0)
					ImDiskAPI.ForceRemoveDevice(SelectedDriveID);
				else
					ImDiskAPI.RemoveDevice(SelectedDriveID);
			}
			catch {
				if (MessageBox.Show("The drive could not be removed. Would you like to try to force a removal?", "Remove Drive Failed", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
					ImDiskAPI.ForceRemoveDevice(SelectedDriveID);
				}
			}
			ShellListView.RefreshContents();
			ctgDrive.Visibility = WIN.Visibility.Collapsed;
		}

		#endregion

		#region Application Tools

		private void btnRunAsAdmin_Click(object sender, RoutedEventArgs e) {
			//ShellView.RunExeAsAdmin(ShellListView.GetFirstSelectedItem().ParsingName);
			var FileName = ShellListView.GetFirstSelectedItem().ParsingName;
			Process.Start(new ProcessStartInfo {
				FileName = FileName,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = String.Format("/env /user:Administrator \"{0}\"", FileName),
			});
		}

		private void btnPin_Click(object sender, RoutedEventArgs e) {
			User32.PinUnpinToTaskbar(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		private void btnPinToStart_Click(object sender, RoutedEventArgs e) {
			User32.PinUnpinToStartMenu(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		private void btnRunAs_Click(object sender, RoutedEventArgs e) {
			CredUI.RunProcesssAsUser(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		#endregion

		#region Backstage - Information Tab

		private void Button_Click_6(object sender, RoutedEventArgs e) {
			backstage.IsOpen = true;
			autoUpdater.Visibility = WIN.Visibility.Visible;
			autoUpdater.UpdateLayout();

			switch (autoUpdater.UpdateStepOn) {
				case UpdateStepOn.Checking:
				case UpdateStepOn.DownloadingUpdate:
				case UpdateStepOn.ExtractingUpdate:
					autoUpdater.Cancel();
					break;
				case UpdateStepOn.UpdateReadyToInstall:
				case UpdateStepOn.UpdateAvailable:
					break;
				case UpdateStepOn.UpdateDownloaded:
					autoUpdater.InstallNow();
					break;
				default:
					autoUpdater.ForceCheckForUpdate(true);
					break;
			}
		}

		private void Button_Click_7(object sender, RoutedEventArgs e) {
			Process.Start("http://better-explorer.com/");
		}

		#endregion

		#region Path to String HelperFunctions / Other HelperFunctions

		private Visibility BooleanToVisibiliy(bool value) {
			return value ? Visibility.Visible : Visibility.Collapsed;
		}

		private void AddToLog(string value) {
			try {
				if (canlogactions) {
					if (!Directory.Exists(logdir)) Directory.CreateDirectory(logdir);

					using (var sw = new StreamWriter(String.Format("{0}{1}.txt", logdir, sessionid), true)) {
						sw.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " : " + value);
					}
				}
			}
			catch (Exception exe) {
				MessageBox.Show("An error occurred while writing to the log file. This error can be avoided if you disable the action logging feature. Please report this issue at http://bugtracker.better-explorer.com/. \r\n\r\n Here is some information about the error: \r\n\r\n" + exe.Message + "\r\n\r\n" + exe.ToString(), "Error While Writing to Log", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}


		#endregion

		#region Updater

		private void CheckBox_Checked(object sender, RoutedEventArgs e) {
			if (isOnLoad) return;

			Utilities.SetRegistryValue("CheCkForUpdates", 1);
			IsUpdateCheck = true;
			updateCheckTimer.Interval = 3600000 * 3;
			updateCheckTimer.Tick += new EventHandler(updateCheckTimer_Tick);
			updateCheckTimer.Start();

			if (DateTime.Now.Subtract(LastUpdateCheck).Days >= UpdateCheckInterval) {
				CheckForUpdate(false);
			}
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (isOnLoad) return;
			Utilities.SetRegistryValue("CheckForUpdates", 0);
			IsUpdateCheck = false;
		}

		private void rbCheckInterval_Click(object sender, RoutedEventArgs e) {
			if (rbDaily.IsChecked.Value)
				UpdateCheckInterval = 1;
			else if (rbMonthly.IsChecked.Value)
				UpdateCheckInterval = 30;
			else
				UpdateCheckInterval = 7;

			Utilities.SetRegistryValue("CheckInterval", UpdateCheckInterval);
		}

		private void chkUpdateStartupCheck_Click(object sender, RoutedEventArgs e) {
			Utilities.SetRegistryValue("CheckForUpdatesStartup", chkUpdateStartupCheck.IsChecked.Value ? 1 : 0);
			IsUpdateCheckStartup = chkUpdateStartupCheck.IsChecked.Value;
		}

		private void UpdateTypeCheck_Click(object sender, RoutedEventArgs e) {
			Utilities.SetRegistryValue("UpdateCheckType", rbReleases.IsChecked.Value ? 0 : 1);
			UpdateCheckType = rbReleases.IsChecked.Value ? 0 : 1;
		}

		#endregion

		#region On Startup

		private void SetUpFavoritesMenu() {
			Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
				btnFavorites.Visibility = Visibility.Visible;

				var OpenFavorites = new MenuItem() { Header = "Open Favorites" };
				var Path = ((ShellItem)KnownFolders.Links).FileSystemPath;
				OpenFavorites.Click += (x, y) => Process.Start(Path);

				btnFavorites.Items.Add(OpenFavorites);
				btnFavorites.Items.Add(new Separator());

				foreach (ShellItem item in KnownFolders.Links.Where(w => !w.IsHidden)) {
					item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					item.Thumbnail.CurrentSize = new WIN.Size(16, 16);
					btnFavorites.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.Thumbnail.BitmapSource, onClick: mif_Click));
				}
			}));
		}

		private void InitializeExplorerControl() {
			this.ShellTree.NodeClick += ShellTree_NodeClick;
			this.ShellTree.AfterSelect += ShellTree_AfterSelect;
			this.ShellListView.Navigated += ShellListView_Navigated;
			this.ShellListView.ViewStyleChanged += ShellListView_ViewStyleChanged;
			this.ShellListView.SelectionChanged += ShellListView_SelectionChanged;
			this.ShellListView.LVItemsColorCodes = this.LVItemsColor;
			this.ShellListView.ItemUpdated += ShellListView_ItemUpdated;
			this.ShellListView.ColumnHeaderRightClick += ShellListView_ColumnHeaderRightClick;
			this.ShellListView.KeyJumpKeyDown += ShellListView_KeyJumpKeyDown;
			this.ShellListView.KeyJumpTimerDone += ShellListView_KeyJumpTimerDone;
			//this.ShellListView.ItemDisplayed += ShellListView_ItemDisplayed;
			this.ShellListView.Navigating += ShellListView_Navigating;
			this.ShellListView.ItemMiddleClick += (sender, e) => tcMain.NewTab(e.Folder, false);
			this.ShellListView.BeginItemLabelEdit += ShellListView_BeginItemLabelEdit;
			this.ShellListView.EndItemLabelEdit += ShellListView_EndItemLabelEdit;
		}

		void ShellTree_AfterSelect(object sender, NavigatedEventArgs e) {
			//this.bcbc.Path = this.ShellListView.CurrentFolder.ParsingName;
		}

		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			Handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
		}

		private void LoadRegistryRelatedSettings() {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.CreateSubKey(@"Software\BExplorer");

			switch (Convert.ToString(rks.GetValue("CurrentTheme", "Blue"))) {
				case "Blue":
					btnBlue.IsChecked = true;
					break;
				case "Silver":
					btnSilver.IsChecked = true;
					break;
				case "Black":
					btnBlack.IsChecked = true;
					break;
				case "Metro":
					btnMetro.IsChecked = true;
					break;
				default:
					btnBlue.IsChecked = true;
					break;
			}

			LastUpdateCheck = DateTime.FromBinary(Convert.ToInt64(rks.GetValue("LastUpdateCheck", 0)));

			UpdateCheckInterval = (int)rks.GetValue("CheckInterval", 7);

			switch (UpdateCheckInterval) {
				case 1:
					rbDaily.IsChecked = true;
					break;
				case 7:
					rbWeekly.IsChecked = true;
					break;
				case 30:
					rbMonthly.IsChecked = true;
					break;
			}

			UpdateCheckType = (int)rks.GetValue("UpdateCheckType", 1);

			switch (UpdateCheckType) {
				case 0:
					rbReleases.IsChecked = true;
					break;
				case 1:
					rbReleasTest.IsChecked = true;
					break;
			}

			IsUpdateCheck = (int)rks.GetValue("CheckForUpdates", 1) == 1;
			chkUpdateCheck.IsChecked = IsUpdateCheck;

			IsUpdateCheckStartup = (int)rks.GetValue("CheckForUpdatesStartup", 1) == 1;
			chkUpdateStartupCheck.IsChecked = IsUpdateCheckStartup;

			IsConsoleShown = (int)rks.GetValue("IsConsoleShown", 0) == 1;
			btnConsolePane.IsChecked = this.IsConsoleShown;
			chkIsFlyout.IsChecked = (int)rks.GetValue("HFlyoutEnabled", 0) == 1;

			IsInfoPaneEnabled = (int)rks.GetValue("InfoPaneEnabled", 0) == 1;
			IsInfoPaneEnabled = false; //Since the user cannot disable it right now, it should always be disabled
			btnInfoPane.IsChecked = IsInfoPaneEnabled;

			InfoPaneHeight = (int)rks.GetValue("InfoPaneHeight", 150);

			if (IsInfoPaneEnabled) {
				rPreviewPane.Height = new GridLength(InfoPaneHeight);
				rPreviewPaneSplitter.Height = new GridLength(1);
			}
			else {
				rPreviewPane.Height = new GridLength(0);
				rPreviewPaneSplitter.Height = new GridLength(0);
			}

			IsPreviewPaneEnabled = (int)rks.GetValue("PreviewPaneEnabled", 0) == 1;
			btnPreviewPane.IsChecked = IsPreviewPaneEnabled;

			PreviewPaneWidth = (int)rks.GetValue("PreviewPaneWidth", 120);

			if (IsPreviewPaneEnabled) {
				clPreview.Width = new GridLength((double)PreviewPaneWidth);
				clPreviewSplitter.Width = new GridLength(1);
			}
			else {
				clPreview.Width = new GridLength(0);
				clPreviewSplitter.Width = new GridLength(0);
			}

			btnNavigationPane.IsChecked = (int)rks.GetValue("NavigationPaneEnabled", 1) == 1;
			chkShowCheckBoxes.IsChecked = ShellListView.ShowCheckboxes;
			chkIsTerraCopyEnabled.IsChecked = (int)rks.GetValue("FileOpExEnabled", 0) == 1;
			chkIsCFO.IsChecked = (int)rks.GetValue("IsCustomFO", 0) == 1;
			IsrestoreTabs = (int)rks.GetValue("IsRestoreTabs", 1) == 1;

			chkIsRestoreTabs.IsChecked = IsrestoreTabs;

			//if this instance has the /norestore switch, do not load tabs from previous session, even if it is set in the Registry
			if (App.isStartWithStartupTab) {
				IsrestoreTabs = false;
			}

			//this.chkIsLastTabCloseApp.IsChecked = (int)rks.GetValue("IsLastTabCloseApp", 1) == 1;

			canlogactions = (int)rks.GetValue("EnableActionLog", 0) == 1;
			chkLogHistory.IsChecked = canlogactions;
			if (canlogactions) {
				chkLogHistory.Visibility = Visibility.Visible;
				ShowLogsBorder.Visibility = Visibility.Visible;
				paddinglbl8.Visibility = Visibility.Visible;
			}

			// load settings for auto-switch to contextual tab
			asFolder = (int)rks.GetValue("AutoSwitchFolderTools", 0) == 1;
			asArchive = (int)rks.GetValue("AutoSwitchArchiveTools", 1) == 1;
			asImage = (int)rks.GetValue("AutoSwitchImageTools", 1) == 1;
			asApplication = (int)rks.GetValue("AutoSwitchApplicationTools", 0) == 1;
			asLibrary = (int)rks.GetValue("AutoSwitchLibraryTools", 1) == 1;
			asDrive = (int)rks.GetValue("AutoSwitchDriveTools", 1) == 1;
			asVirtualDrive = (int)rks.GetValue("AutoSwitchVirtualDriveTools", 0) == 1;


			chkFolder.IsChecked = asFolder;
			chkArchive.IsChecked = asArchive;
			chkImage.IsChecked = asImage;
			chkApp.IsChecked = asApplication;
			chkLibrary.IsChecked = asLibrary;
			chkDrive.IsChecked = asDrive;
			chkVirtualTools.IsChecked = asVirtualDrive;

			// load OverwriteOnImages setting (default is false)
			bool oor = (int)rks.GetValue("OverwriteImageWhileEditing", 0) == 1;
			OverwriteOnRotate = oor;
			chkOverwriteImages.IsChecked = oor;

			// load Saved Tabs Directory location (if different from default)
			string tdir = Convert.ToString(rks.GetValue("SavedTabsDirectory", satdir));
			txtDefSaveTabs.Text = tdir;
			sstdir = tdir;

			var StartUpLocation = rks.GetValue("StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();
			if (tcMain.StartUpLocation == "") {
				rks.SetValue("StartUpLoc", KnownFolders.Libraries.ParsingName);
				tcMain.StartUpLocation = KnownFolders.Libraries.ParsingName;
			}

			try {
				var rkbe = Registry.ClassesRoot;
				var rksbe = rkbe.OpenSubKey(@"Folder\shell\open\command", RegistryKeyPermissionCheck.ReadSubTree);
				var isThereDefault = rksbe.GetValue("DelegateExecute", "-1").ToString() == "-1";
				chkIsDefault.IsChecked = isThereDefault;
				chkIsDefault.IsEnabled = true;
				rksbe.Close();
				rkbe.Close();
			}
			catch (Exception) {
				chkIsDefault.IsChecked = false;
				chkIsDefault.IsEnabled = false;
			}

			var rkfe = Registry.CurrentUser;
			var rksfe = rk.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", RegistryKeyPermissionCheck.ReadSubTree);
			chkTreeExpand.IsChecked = (int)rksfe.GetValue("NavPaneExpandToCurrentFolder", 0) == 1;
			rksfe.Close();
			rkfe.Close();

			rks.Close();
			rk.Close();


			var sho = new ShellItem(StartUpLocation.ToShellParsingName());
			btnSetCurrentasStartup.Header = sho.DisplayName;
			sho.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			btnSetCurrentasStartup.Icon = sho.Thumbnail.SmallBitmapSource;


			miTabManager.IsEnabled = Directory.Exists(sstdir);
			autoUpdater.DaysBetweenChecks = this.UpdateCheckInterval;

			try {
				autoUpdater.UpdateType = IsUpdateCheck ? UpdateType.OnlyCheck : UpdateType.DoNothing;
				if (IsUpdateCheckStartup) autoUpdater.ForceCheckForUpdate();
			}
			catch (IOException) {
				this.stiUpdate.Content = "Switch to another BetterExplorer window or restart to check for updates.";
				this.btnUpdateCheck.IsEnabled = false;
			}

			if (App.isStartMinimized) {
				this.Visibility = Visibility.Hidden;
				this.WindowState = WindowState.Minimized;
			}

			if (!TheRibbon.IsMinimized) {
				TheRibbon.SelectedTabItem = HomeTab;
				this.TheRibbon.SelectedTabIndex = 0;
			}
		}

		private void SetsUpJumpList() {
			//sets up Jump List
			try {
				AppJL.ShowRecentCategory = true;
				AppJL.ShowFrequentCategory = true;
				WIN.Shell.JumpList.SetJumpList(Application.Current, AppJL);
				AppJL.JumpItems.Add(new JumpTask() {
					ApplicationPath = Process.GetCurrentProcess().MainModule.FileName,
					Arguments = "t",
					Title = "Open Tab",
					Description = "Opens new tab with default location"
				});
				AppJL.JumpItems.Add(new JumpTask() {
					ApplicationPath = Process.GetCurrentProcess().MainModule.FileName,
					Arguments = "/nw",
					Title = "New Window",
					Description = "Creates a new window with default location"
				});

				AppJL.Apply();
			}
			catch {

			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			_keyjumpTimer.Interval = 1000;
			_keyjumpTimer.Tick += _keyjumpTimer_Tick;
			ShellTreeHost.Child = ShellTree;
			ShellViewHost.Child = ShellListView;
			this.tcMain.OnTabClicked += tcMain_OnTabClicked;

			ShellTree.ShellListView = ShellListView;
			this.ctrlConsole.ShellListView = this.ShellListView;

			Task.Run(() => {
				UpdateRecycleBinInfos();
			});

			bool exitApp = false;

			try {
				//Sets up FileSystemWatcher for Favorites folder
				var LinksFolderWarcher = new FileSystemWatcher(KnownFolders.Links.ParsingName);
				LinksFolderWarcher.Created += LinksFolderWarcher_Created;
				LinksFolderWarcher.Deleted += LinksFolderWarcher_Deleted;
				LinksFolderWarcher.Renamed += LinksFolderWarcher_Renamed;
				LinksFolderWarcher.EnableRaisingEvents = true;

				//Set up Favorites Menu
				Task.Run(() => {
					SetUpFavoritesMenu();
				});

				//Load the ShellSettings
				IsCalledFromLoading = true;
				var statef = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
				BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref statef, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS | BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, false);
				chkHiddenFiles.IsChecked = statef.fShowAllObjects == 1;
				ShellListView.ShowHidden = chkHiddenFiles.IsChecked.Value;
				ShellTree.IsShowHiddenItems = chkHiddenFiles.IsChecked.Value;
				chkExtensions.IsChecked = statef.fShowExtensions == 1;
				this.ShellListView.IsFileExtensionShown = statef.fShowExtensions == 1;
				IsCalledFromLoading = false;

				isOnLoad = true;

				//'load from Registry
				LoadRegistryRelatedSettings();

				//'set up Explorer control
				InitializeExplorerControl();

				ViewGallery.SelectedIndex = 2;

				AddToLog("Session Began");
				isOnLoad = false;
				SetsUpJumpList();

				//Setup Clipboard monitor
				cbm.ClipboardChanged += cbm_ClipboardChanged;

				if (exitApp) {
					Application.Current.Shutdown();
					return;
				}

				// Set up Column Header menu
				chcm = new ContextMenu() { Placement = PlacementMode.MousePoint };

				//Set up Version Info
				verNumber.Content = "Version " + (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault() as AssemblyInformationalVersionAttribute).InformationalVersion;
				lblArchitecture.Content = Kernel32.IsThis64bitProcess() ? "64-bit version" : "32-bit version";

				tcMain_Setup(null, null);
				//'set StartUp location
				if (Application.Current.Properties["cmd"] != null && Application.Current.Properties["cmd"].ToString() != "-minimized") {
					//if (Application.Current.Properties["cmd"].ToString() == "/nw")
					//tcMain.NewTab(ShellListView.CurrentFolder, true);
				}
				else {
					InitializeInitialTabs();
				}

				if (!File.Exists("Settings.xml")) return;
				var Settings = XElement.Load("Settings.xml");

				if (Settings.Element("DropDownItems") != null) {
					foreach (var item in Settings.Element("DropDownItems").Elements()) {
						bcbc.DropDownItems.Add(item.Value);
					}
				}

				focusTimer.Tick += focusTimer_Tick;
			}
			catch (Exception exe) {
				MessageBox.Show(String.Format("An error occurred while loading the window. Please report this issue at http://bugtracker.better-explorer.com/. \r\n\r\n Here is some information about the error: \r\n\r\n{0}\r\n\r\n{1}", exe.Message, exe), "Error While Loading", MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}
		Boolean _IsTabSelectionChangedNotAllowed = true;
		void tcMain_OnTabClicked(object sender, Wpf.Controls.TabClickEventArgs e) {
			this.tcMain.IsInTabDragDrop = false;
			tcMain.SelectedItem = e.ClickedItem;
			this._IsTabSelectionChangedNotAllowed = false;
			this._CurrentlySelectedItem = e.ClickedItem;
			this.SelectTab(tcMain.SelectedItem as Wpf.Controls.TabItem);
			this.tcMain.IsInTabDragDrop = true;
			//this._IsTabSelectionChangedAllowed = false;
		}

		#endregion

		#region On Closing

		private void SaveSettings(String openedTabs) {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);
			rks.SetValue("LastWindowWidth", this.Width);
			rks.SetValue("LastWindowHeight", this.Height);
			rks.SetValue("LastWindowPosLeft", this.Left);
			rks.SetValue("LastWindowPosTop", this.Top);
			if (btnBlue.IsChecked == true) {
				rks.SetValue("CurrentTheme", "Blue");
			}
			else if (btnSilver.IsChecked == true) {
				rks.SetValue("CurrentTheme", "Silver");
			}
			else if (btnBlack.IsChecked == true) {
				rks.SetValue("CurrentTheme", "Black");
			}
			else if (btnMetro.IsChecked == true) {
				rks.SetValue("CurrentTheme", "Metro");
			}
			switch (this.WindowState) {
				case WIN.WindowState.Maximized:
					rks.SetValue("LastWindowState", 2);
					break;
				case WIN.WindowState.Minimized:
					rks.SetValue("LastWindowState", 1);
					break;
				case WIN.WindowState.Normal:
					rks.SetValue("LastWindowState", 0);
					break;
				default:
					rks.SetValue("LastWindowState", -1);
					break;
			}

			rks.SetValue("IsRibonMinimized", TheRibbon.IsMinimized);
			rks.SetValue("OpenedTabs", openedTabs);
			rks.SetValue("RTLMode", FlowDirection == WIN.FlowDirection.RightToLeft ? "true" : "false");
			rks.SetValue("AutoSwitchFolderTools", Convert.ToInt32(asFolder));
			rks.SetValue("AutoSwitchArchiveTools", Convert.ToInt32(asArchive));
			rks.SetValue("AutoSwitchImageTools", Convert.ToInt32(asImage));
			rks.SetValue("AutoSwitchApplicationTools", Convert.ToInt32(asApplication));
			rks.SetValue("AutoSwitchLibraryTools", Convert.ToInt32(asLibrary));
			rks.SetValue("AutoSwitchDriveTools", Convert.ToInt32(asDrive));
			rks.SetValue("AutoSwitchVirtualDriveTools", Convert.ToInt32(asVirtualDrive));
			//rks.SetValue("IsLastTabCloseApp", Convert.ToInt32(this.IsCloseLastTabCloseApp));
			//rks.SetValue("IsLastTabCloseApp", Convert.ToInt32(chkIsLastTabCloseApp.IsChecked.Value));

			rks.SetValue("IsConsoleShown", this.IsConsoleShown ? 1 : 0);
			rks.SetValue("TabBarAlignment", this.TabbaBottom.IsChecked == true ? "bottom" : "top");

			if (this.IsPreviewPaneEnabled)
				rks.SetValue("PreviewPaneWidth", (int)clPreview.ActualWidth, RegistryValueKind.DWord);
			if (this.IsInfoPaneEnabled)
				rks.SetValue("InfoPaneHeight", (int)rPreviewPane.ActualHeight, RegistryValueKind.DWord);
			if (this.IsConsoleShown)
				rks.SetValue("CmdWinHeight", rCommandPrompt.ActualHeight, RegistryValueKind.DWord);

			rks.Close();
		}

		private void RibbonWindow_Closing(object sender, CancelEventArgs e) {
			if (this.OwnedWindows.OfType<BExplorer.Shell.FileOperationDialog>().Any()) {
				if (MessageBox.Show("Are you sure you want to cancel all running file operation tasks?", "", MessageBoxButton.YesNo) == MessageBoxResult.No) {
					e.Cancel = true;
					return;
				}
			}

			if (this.WindowState != WIN.WindowState.Minimized) {
				string OpenedTabs = "";
				foreach (Wpf.Controls.TabItem item in tcMain.Items) {
					OpenedTabs += ";" + item.ShellObject.ParsingName;
				}

				SaveSettings(OpenedTabs);
			}
			this.ShellListView.SaveSettingsToDatabase(this.ShellListView.CurrentFolder);
			//SaveHistoryToFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\history.txt", this.bcbc.DropDownItems.OfType<String>().Select(s => s).ToList());
			AddToLog("Session Ended");
			if (!this.IsMultipleWindowsOpened) {
				e.Cancel = true;
				App.isStartMinimized = true;

				this.WindowState = WIN.WindowState.Minimized;
				this.Visibility = WIN.Visibility.Hidden;
			}
			else {
				beNotifyIcon.Visibility = WIN.Visibility.Collapsed;
			}

			if (!File.Exists("Settings.xml")) {
				new XElement("Settings").Save("Settings.xml");
			}
			var Data = bcbc.DropDownItems;

			var Settings = XElement.Load("Settings.xml");
			if (Settings.Element("DropDownItems") == null)
				Settings.Add(new XElement("DropDownItems"));
			else
				Settings.Element("DropDownItems").RemoveAll();


			foreach (var item in bcbc.DropDownItems.OfType<string>().Reverse().Take(15)) {
				Settings.Element("DropDownItems").Add(new XElement("Item", item));
			}

			Settings.Save("Settings.xml");
		}

		#endregion

		#region Change Ribbon Color (Theme)

		public void ChangeRibbonTheme(string ThemeName, bool IsMetro = false) {
			Dispatcher.BeginInvoke(IsMetro ? DispatcherPriority.ApplicationIdle : DispatcherPriority.Render, (ThreadStart)(() => {
				var owner = Window.GetWindow(this);
				Application.Current.Resources.BeginInit();

				if (IsMetro) {
					Application.Current.Resources.MergedDictionaries.RemoveAt(1);
					Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Fluent;component/Themes/Office2013/Generic.xaml") });
				}
				else {
					Application.Current.Resources.MergedDictionaries.RemoveAt(1);
					Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = new Uri(String.Format("pack://application:,,,/Fluent;component/Themes/Office2010/{0}.xaml", ThemeName)) });
				}

				Application.Current.Resources.EndInit();

				if (owner != null) {
					owner.Style = null;
					owner.Style = owner.FindResource("RibbonWindowStyle") as Style;
					owner.Style = null;
				}

				Utilities.SetRegistryValue("CurrentTheme", ThemeName);
			}));
		}

		private void btnSilver_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonTheme("Silver");
			KeepBackstageOpen = true;
		}

		private void btnBlue_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonTheme("Blue");
			KeepBackstageOpen = true;
		}

		private void btnBlack_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonTheme("Black");
			KeepBackstageOpen = true;
		}

		private void btnGreen_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonTheme("Metro", true);
			KeepBackstageOpen = true;
		}

		#endregion

		#region Archive Commands

		private void btnExtractNow_Click(object sender, RoutedEventArgs e) {
			if (chkUseNewFolder.IsChecked == true) {
				string OutputLoc = String.Format("{0}\\{1}", txtExtractLocation.Text, Utilities.RemoveExtensionsFromFile(ShellListView.GetFirstSelectedItem().ParsingName, new FileInfo(ShellListView.GetFirstSelectedItem().ParsingName).Extension));
				try {
					Directory.CreateDirectory(OutputLoc);
					ExtractToLocation(SelectedArchive, OutputLoc);
				}
				catch (Exception) {
					MessageBoxResult wtd = MessageBox.Show(String.Format("The directory {0} already exists. Would you like for BetterExplorer to extract there instead?", OutputLoc), "Folder Exists", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
					switch (wtd) {
						case MessageBoxResult.Cancel:
							break;
						case MessageBoxResult.No:
							break;
						case MessageBoxResult.None:
							break;
						case MessageBoxResult.OK:
							break;
						case MessageBoxResult.Yes:
							ExtractToLocation(SelectedArchive, OutputLoc);
							break;
						default:
							break;
					}
				}
			}
			else {
				ExtractToLocation(SelectedArchive, txtExtractLocation.Text);
			}

		}

		private void btnChooseLocation_Click(object sender, RoutedEventArgs e) {
			var dlg = new FolderSelectDialog();
			if (dlg.ShowDialog() == true) {
				txtExtractLocation.Text = dlg.FileName;
			}
		}

		private void ExtractToLocation(string archive, string output) {
			var selectedItems = new List<string>() { archive };

			var archiveProcressScreen = new ArchiveProcressScreen(selectedItems, output, ArchiveAction.Extract, "");
			archiveProcressScreen.ExtractionCompleted += new ArchiveProcressScreen.ExtractionCompleteEventHandler(ExtractionHasCompleted);
			AddToLog(String.Format("Archive Extracted to {0} from source {1}", output, archive));
			archiveProcressScreen.Show();
		}

		private void ExtractionHasCompleted(object sender, ArchiveEventArgs e) {
			if (chkOpenResults.IsChecked == true) {
				tcMain.NewTab(e.OutputLocation);
			}
		}

		//private void miCreateZIP_Click(object sender, RoutedEventArgs e)
		//{
		//    CreateArchive_Show(OutArchiveFormat.Zip);
		//}

		//private void miCreate7z_Click(object sender, RoutedEventArgs e)
		//{
		//    CreateArchive_Show(OutArchiveFormat.SevenZip);
		//}

		//private void miCreateCAB_Click(object sender, RoutedEventArgs e)
		//{
		//    CreateArchive_Show(OutArchiveFormat.Tar);
		//}


		private void miExtractToLocation_Click(object sender, RoutedEventArgs e) {
			var selectedItems = ShellListView.SelectedItems.Select(item => item.ParsingName).ToList();

			/*
			var selectedItems = new List<string>();
			foreach (ShellItem item in ShellListView.SelectedItems) {
			  selectedItems.Add(item.ParsingName);
			}
			*/
			try {
				var CAI = new CreateArchive(selectedItems, false, ShellListView.GetFirstSelectedItem().ParsingName);
				CAI.Show(this.GetWin32Window());
			}
			catch (Exception exception) {
				//var dialog = new TaskDialog();
				//dialog.StandardButtons = TaskDialogStandardButtons.Ok;
				//dialog.Text = exception.Message;
				//dialog.Show();
			}
		}


		private void miExtractHere_Click(object sender, RoutedEventArgs e) {
			string FileName = ShellListView.GetFirstSelectedItem().ParsingName;
			SevenZipExtractor extractor = new SevenZipExtractor(FileName);
			string DirectoryName = System.IO.Path.GetDirectoryName(FileName);
			string ArchName = System.IO.Path.GetFileNameWithoutExtension(FileName);
			extractor.Extracting += new EventHandler<ProgressEventArgs>(extractor_Extracting);
			extractor.ExtractionFinished += new EventHandler<EventArgs>(extractor_ExtractionFinished);
			extractor.FileExtractionStarted += new EventHandler<FileInfoEventArgs>(extractor_FileExtractionStarted);
			extractor.FileExtractionFinished += new EventHandler<FileInfoEventArgs>(extractor_FileExtractionFinished);
			extractor.PreserveDirectoryStructure = true;
			string Separator = "";
			if (DirectoryName[DirectoryName.Length - 1] != Char.Parse(@"\")) {
				Separator = @"\";
			}
			AddToLog(String.Format("Extracted Archive to {0}{1}{2} from source {3}", DirectoryName, Separator, ArchName, FileName));
			extractor.BeginExtractArchive(DirectoryName + Separator + ArchName);
		}

		void extractor_FileExtractionFinished(object sender, FileInfoEventArgs e) {
			//throw new NotImplementedException();
		}

		void extractor_FileExtractionStarted(object sender, FileInfoEventArgs e) {
			//throw new NotImplementedException();
		}

		void extractor_ExtractionFinished(object sender, EventArgs e) {
			//throw new NotImplementedException();
			if ((sender as SevenZipExtractor) != null) {
				(sender as SevenZipExtractor).Dispose();
			}
		}

		void extractor_Extracting(object sender, ProgressEventArgs e) {
			//throw new NotImplementedException();
		}

		private void btnExtract_Click(object sender, RoutedEventArgs e) {
			miExtractHere_Click(sender, e);
		}

		private void btnCheckArchive_Click(object sender, RoutedEventArgs e) {
			Thread trIntegrityCheck = new Thread(new ThreadStart(DoCheck));
			trIntegrityCheck.Start();
		}


		private void DoCheck() {
			string FileName = ShellListView.GetFirstSelectedItem().ParsingName;
			SevenZipExtractor extractor = new SevenZipExtractor(FileName);
			if (!extractor.Check())
				MessageBox.Show("Not Pass");
			else
				MessageBox.Show("Pass");

			extractor.Dispose();
		}

		private void btnViewArchive_Click(object sender, RoutedEventArgs e) {
			var name = ShellListView.SelectedItems.First().ParsingName;
			string ICON_DLLPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
			var archiveDetailView = new ArchiveDetailView(ICON_DLLPATH, name);
			archiveDetailView.Show(this.GetWin32Window());

			//ArchiveViewWindow g = new ArchiveViewWindow( ShellListView.GetFirstSelectedItem(), IsPreviewPaneEnabled, IsInfoPaneEnabled);
			//g.Show();
		}

		private void btnCreateArchive_Click(object sender, RoutedEventArgs e) {
			//ArchiveCreateWizard acw = new ArchiveCreateWizard(ShellListView.SelectedItems, ShellListView.CurrentFolder.ParsingName);
			//         acw.win = this;
			//         acw.LoadStrings();
			//acw.Show();
			//AddToLog("Archive Creation Wizard begun. Current location: " + ShellListView.CurrentFolder.ParsingName);
		}

		#endregion

		#region Library Commands

		private void btnOLItem_Click(object sender, RoutedEventArgs e) {
			this.ShellListView.IsLibraryInModify = true;
			this.ShellListView.CurrentRefreshedItemIndex = this.ShellListView.GetFirstSelectedItemIndex();
			var NeededFile = ShellListView.GetSelectedCount() == 1 ? ShellListView.GetFirstSelectedItem() : ShellListView.CurrentFolder;
			var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(NeededFile.ParsingName), false);

			switch ((sender as MenuItem).Tag.ToString()) {
				case "gu":
					lib.LibraryType = LibraryFolderType.Generic;
					lib.Close();
					break;
				case "doc":
					lib.LibraryType = LibraryFolderType.Documents;
					lib.Close();
					break;
				case "pic":
					lib.LibraryType = LibraryFolderType.Pictures;
					lib.Close();
					break;
				case "vid":
					lib.LibraryType = LibraryFolderType.Videos;
					lib.Close();
					break;
				case "mu":
					lib.LibraryType = LibraryFolderType.Music;
					lib.Close();
					break;
				default:
					break;
			}
		}

		private void chkPinNav_CheckChanged(object sender, RoutedEventArgs e) {
			this.ShellListView.IsLibraryInModify = true;
			this.ShellListView.CurrentRefreshedItemIndex = this.ShellListView.GetFirstSelectedItemIndex();
			var NeededFile = ShellListView.GetSelectedCount() == 1 ? ShellListView.GetFirstSelectedItem() : ShellListView.CurrentFolder;
			var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(NeededFile.ParsingName), false);


			if (!IsFromSelectionOrNavigation)
				lib.IsPinnedToNavigationPane = e.RoutedEvent.Name == "Checked";

			lib.Close();
		}

		private void btnChangeLibIcon_Click(object sender, RoutedEventArgs e) {
			new IconView().LoadIcons(ShellListView, true);
		}

		private void btnManageLib_Click(object sender, RoutedEventArgs e) {
			this.ShellListView.CurrentRefreshedItemIndex = this.ShellListView.GetFirstSelectedItemIndex();
			try {
				ShellLibrary.ShowManageLibraryUI(ShellListView.GetFirstSelectedItem().DisplayName,
					this.Handle, "Choose which folders will be in this library", "A library gathers content from all of the folders listed below and puts it all in one window for you to see.", true);
			}
			catch {
				ShellLibrary.ShowManageLibraryUI(ShellListView.CurrentFolder.DisplayName,
					this.Handle, "Choose which folders will be in this library", "A library gathers content from all of the folders listed below and puts it all in one window for you to see.", true);
			}
		}

		private void Button_Click_3(object sender, RoutedEventArgs e) {
			this.ShellListView.CurrentRefreshedItemIndex = this.ShellListView.GetFirstSelectedItemIndex();
			//TaskDialog td = new TaskDialog();
			//td.Caption = "Reset Library";
			//td.Icon = TaskDialogStandardIcon.Warning;
			//td.Text = "Would you like to reset this library to the default settings?";
			//td.InstructionText = "Reset Library Properties?";
			//td.FooterIcon = TaskDialogStandardIcon.Information;
			////td.FooterText = "This will reset all the properties to default properties for library type";
			//td.DetailsCollapsedLabel = "More Info";
			//td.DetailsExpandedLabel = "More Info";
			//td.DetailsExpandedText = "This will undo all changes you have made to this library, and reset it to its default state.";
			//td.DetailsExpanded = false;
			//td.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;
			//td.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
			//td.OwnerWindowHandle = this.Handle;
			//if (td.Show() == TaskDialogResult.Yes) {
			this.ShellListView.IsLibraryInModify = true;
			if (ShellListView.GetFirstSelectedItem().DisplayName.ToLowerInvariant() != "documents" &&
				 ShellListView.GetFirstSelectedItem().DisplayName.ToLowerInvariant() != "music" &&
				 ShellListView.GetFirstSelectedItem().DisplayName.ToLowerInvariant() != "videos" &&
				 ShellListView.GetFirstSelectedItem().DisplayName.ToLowerInvariant() != "pictures") {
				var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(ShellListView.GetFirstSelectedItem().ParsingName), false);
				lib.IsPinnedToNavigationPane = true;
				lib.LibraryType = LibraryFolderType.Generic;
				lib.IconResourceId = new IconReference(@"C:\Windows\System32\imageres.dll", 187);
				lib.Close();
			}
			//}
		}

		#endregion

		#region Navigation (Back/Forward Arrows) and Up Button

		private void leftNavBut_Click(object sender, RoutedEventArgs e) {
			tcMain.isGoingBackOrForward = true;
			NavigationController((tcMain.SelectedItem as Wpf.Controls.TabItem).log.NavigateBack());
		}

		private void rightNavBut_Click(object sender, RoutedEventArgs e) {
			tcMain.isGoingBackOrForward = true;
			NavigationController((tcMain.SelectedItem as Wpf.Controls.TabItem).log.NavigateForward());
		}

		private void downArrow_Click(object sender, RoutedEventArgs e) {
			cmHistory.Items.Clear();
			NavigationLog nl = (tcMain.SelectedItem as Wpf.Controls.TabItem).log;
			int i = 0;
			foreach (var item in nl.HistoryItemsList) {
				if (item != null) {
					cmHistory.Items.Add(Utilities.Build_MenuItem(item.DisplayName, item, item.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default),
								 checkable: true, isChecked: i == nl.CurrentLocPos, GroupName: "G1", onClick: miItems_Click));
				}
				i++;
			}

			cmHistory.Placement = PlacementMode.Bottom;
			cmHistory.PlacementTarget = navBarGrid;
			cmHistory.IsOpen = true;
		}

		void miItems_Click(object sender, RoutedEventArgs e) {
			var item = (IListItemEx)(sender as MenuItem).Tag;
			if (item != null) {
				tcMain.isGoingBackOrForward = true;
				NavigationLog nl = (tcMain.Items[tcMain.SelectedIndex] as Wpf.Controls.TabItem).log;
				(sender as MenuItem).IsChecked = true;
				nl.CurrentLocPos = cmHistory.Items.IndexOf(sender);
				NavigationController(item);
			}
		}

		private void btnUpLevel_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.CanNavigateParent) ShellListView.NavigateParent();
		}

		#endregion

		#region View Tab/Status Bar

		private void btnMoreColls_Click(object sender, RoutedEventArgs e) {
			micm_Click(sender, e);
		}

		void mig_Click(object sender, RoutedEventArgs e) {
			if (!this.ShellListView.IsGroupsEnabled) {
				this.ShellListView.EnableGroups();
			}
			this.ShellListView.GenerateGroupsFromColumn((sender as MenuItem).Tag as Collumns);
		}

		private void btnAutosizeColls_Click(object sender, RoutedEventArgs e) {
			this.ShellListView.AutosizeAllColumns(-1);
		}

		private void StatusBar_Folder_Buttons(object sender, RoutedEventArgs e) {
			if (ShellListView == null)
				return;
			else if (sender == btnSbDetails)
				ShellListView.View = ShellViewStyle.Details;
			else if (sender == btnSbIcons)
				ShellListView.View = ShellViewStyle.Medium;
			else if (sender == btnSbTiles)
				ShellListView.View = ShellViewStyle.Tile;
		}

		private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			ShellListView.ResizeIcons((int)e.NewValue);
		}

		void mi_Click(object sender, RoutedEventArgs e) {
			var item = (sender as MenuItem);
			var parentButton = item.Parent as Fluent.DropDownButton;
			var ascitem = (MenuItem)parentButton.Items[parentButton.Items.IndexOf(misa)];

			var Sort = ascitem.IsChecked ? WIN.Forms.SortOrder.Ascending : WIN.Forms.SortOrder.Descending;
			ShellListView.SetSortCollumn((Collumns)item.Tag, Sort);
		}

		void misng_Click(object sender, RoutedEventArgs e) {
			(sender as MenuItem).IsChecked = true;
			if (this.ShellListView.IsGroupsEnabled) {
				this.ShellListView.DisableGroups();
			}
		}

		private void inRibbonGallery1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			e.Handled = true;
			switch (ViewGallery.SelectedIndex) {
				case 0:
					ShellListView.View = ShellViewStyle.ExtraLargeIcon;
					break;
				case 1:
					ShellListView.View = ShellViewStyle.LargeIcon;
					break;
				case 2:
					ShellListView.View = ShellViewStyle.Medium;
					break;
				case 3:
					ShellListView.View = ShellViewStyle.SmallIcon;
					break;
				case 4:
					ShellListView.View = ShellViewStyle.List;
					break;
				case 5:
					ShellListView.View = ShellViewStyle.Details;
					break;
				case 6:
					ShellListView.View = ShellViewStyle.Tile;
					break;
				case 7:
					ShellListView.View = ShellViewStyle.Content;
					break;
				case 8:
					ShellListView.View = ShellViewStyle.Thumbstrip;
					break;
				default:
					break;
			}
		}

		private void chkHiddenFiles_Checked(object sender, RoutedEventArgs e) {
			if (IsCalledFromLoading) return;
			Dispatcher.BeginInvoke(new Action(
			  delegate() {
				  var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = 1 };
				  BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS, true);
				  ShellListView.ShowHidden = true;

				  ShellTree.IsShowHiddenItems = true;
				  ShellTree.RefreshContents();
			  }
			));
		}

		private void chkHiddenFiles_Unchecked(object sender, RoutedEventArgs e) {
			if (IsCalledFromLoading) return;
			Dispatcher.BeginInvoke(new Action(
			  delegate() {
				  var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = 0 };
				  BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWALLOBJECTS, true);
				  ShellListView.ShowHidden = false;

				  ShellTree.IsShowHiddenItems = false;
				  ShellTree.RefreshContents();
			  }
			));
		}

		private void chkExtensions_Checked(object sender, RoutedEventArgs e) {
			if (IsCalledFromLoading) return;
			Dispatcher.BeginInvoke(new Action(
			  delegate() {
				  var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
				  state.fShowExtensions = 1;
				  BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, true);
				  this.ShellListView.IsFileExtensionShown = true;
				  ShellListView.RefreshContents();
			  }
			));
		}

		private void chkExtensions_Unchecked(object sender, RoutedEventArgs e) {
			if (IsCalledFromLoading) return;
			Dispatcher.BeginInvoke(new Action(
			  delegate() {
				  var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
				  state.fShowExtensions = 0;
				  BExplorer.Shell.Interop.Shell32.SHGetSetSettings(ref state, BExplorer.Shell.Interop.Shell32.SSF.SSF_SHOWEXTENSIONS, true);
				  this.ShellListView.IsFileExtensionShown = false;
				  ShellListView.RefreshContents();
			  }
			));
		}

		#endregion

		#region Hide items

		private void btnHideSelItems_Click(object sender, RoutedEventArgs e) {
			foreach (var item in ShellListView.SelectedItems.Where(x => x.IsFolder)) {
				var di = new DirectoryInfo(item.FileSystemPath);
				di.Attributes |= System.IO.FileAttributes.Hidden;
			}
			foreach (var item in ShellListView.SelectedItems.Where(x => !x.IsFolder & !x.IsNetworkPath & !x.IsDrive)) {
				var di = new FileInfo(item.FileSystemPath);
				di.Attributes |= System.IO.FileAttributes.Hidden;
			}

			ShellListView.RefreshContents();
		}

		[Obsolete("Why do we have this")]
		private void miHideItems_Click(object sender, RoutedEventArgs e) {
			//FIXME:
			//ShellItemCollection SelItems = ShellListView.SelectedItems;
			//pd = new IProgressDialog(this.Handle);
			//pd.Title = "Applying attributes";
			//pd.CancelMessage = "Please wait while the operation is cancelled";
			//pd.Maximum = 100;
			//pd.Value = 0;
			//pd.Line1 = "Applying attributes to:";
			//pd.Line3 = "Calculating Time Remaining...";
			//pd.ShowDialog(IProgressDialog.PROGDLG.Normal, IProgressDialog.PROGDLG.AutoTime, IProgressDialog.PROGDLG.NoMinimize);
			//Thread hthread = new Thread(new ParameterizedThreadStart(DoHideShowWithChilds));
			//hthread.Start(SelItems);
		}

		//bool IsHidingUserCancel = false;

		/*
		public void DoHideShowWithChilds(object o) {
		  //FIXME:
		  //IsHidingUserCancel = false;
		  //ShellItemCollection SelItems = o as ShellItemCollection;
		  //List<string> ItemsToHideShow = new List<string>();
		  //foreach (ShellItem obj in SelItems)
		  //{
		  //	ItemsToHideShow.Add(obj.ParsingName);
		  //	if (Directory.Exists(obj.ParsingName))
		  //	{
		  //		DirectoryInfo di = new DirectoryInfo(obj.ParsingName);
		  //		DirectoryInfo[] dirs = di.GetDirectories("*", SearchOption.AllDirectories);
		  //		FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories);
		  //		foreach (DirectoryInfo dir in dirs)
		  //		{
		  //			ItemsToHideShow.Add(dir.FullName);
		  //		}
		  //		foreach (FileInfo file in files)
		  //		{
		  //			ItemsToHideShow.Add(file.FullName);
		  //		}
		  //	}

		  //}

		  //Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //{

		  //	pd.Maximum = (uint)ItemsToHideShow.Count;

		  //}));

		  //foreach (string item in ItemsToHideShow)
		  //{
		  //	try
		  //	{
		  //		Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //		{
		  //			if (pd.HasUserCancelled)
		  //			{
		  //				IsHidingUserCancel = true;

		  //			}
		  //		}));

		  //		if (IsHidingUserCancel)
		  //		{
		  //			break;
		  //		}

		  //		Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //		{

		  //			pd.Line2 = item;

		  //		}));

		  //		System.IO.FileAttributes attr = File.GetAttributes(item);

		  //		if ((attr & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden)
		  //		{
		  //			File.SetAttributes(item, attr & ~System.IO.FileAttributes.Hidden);
		  //		}
		  //		else
		  //		{
		  //			File.SetAttributes(item, attr | System.IO.FileAttributes.Hidden);
		  //		}

		  //	}
		  //	finally
		  //	{

		  //	}

		  //	Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //	{

		  //		pd.Value++;

		  //	}));

		  //}
		  //Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //{

		  //	pd.CloseDialog();
		  //}));
		}
		*/


		/*
		public void DoHideShow(object o) {
		  //FIXME:
		  //ShellItemCollection SelItems = o as ShellItemCollection;
		  //IsHidingUserCancel = false;
		  //foreach (ShellItem item in SelItems)
		  //{
		  //	try
		  //	{
		  //		Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //		{
		  //			if (pd.HasUserCancelled)
		  //			{
		  //				IsHidingUserCancel = true;

		  //			}
		  //		}));

		  //		if (IsHidingUserCancel)
		  //		{
		  //			break;
		  //		}

		  //		Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //		{

		  //			pd.Line2 = item.ParsingName;

		  //		}));

		  //		System.IO.FileAttributes attr = File.GetAttributes(item.ParsingName);

		  //		if ((attr & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden)
		  //		{
		  //			File.SetAttributes(item.ParsingName, attr & ~System.IO.FileAttributes.Hidden);
		  //		}
		  //		else
		  //		{
		  //			File.SetAttributes(item.ParsingName, attr | System.IO.FileAttributes.Hidden);
		  //		}

		  //	}
		  //	finally
		  //	{

		  //	}

		  //	Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //	{

		  //		pd.Value++;

		  //	}));

		  //}
		  //Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
		  //{

		  //	pd.CloseDialog();
		  //}));
		}
		*/



		#endregion

		#region Share Tab Commands (excluding Archive)

		private void btnMapDrive_Click(object sender, RoutedEventArgs e) {
			this.ShellListView.MapDrive(this.Handle, this.ShellListView.SelectedItems.Count() == 1 ? this.ShellListView.GetFirstSelectedItem().ParsingName : String.Empty);
		}

		private void btnDisconectDrive_Click(object sender, RoutedEventArgs e) {
			BExplorer.Shell.Interop.Shell32.WNetDisconnectDialog(this.Handle, 1);
		}

		private void Button_Click_4(object sender, RoutedEventArgs e) {
			this.ShellListView.OpenShareUI();
		}

		private void btnAdvancedSecurity_Click(object sender, RoutedEventArgs e) {
			ShellListView.ShowPropPage(this.Handle, ShellListView.GetFirstSelectedItem().ParsingName, User32.LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 9046, "Security1"));
		}

		#endregion

		#region Change Language

		private void TranslationComboBox_DropDownOpened(object sender, EventArgs e) {
			(sender as Fluent.ComboBox).Focus();
		}

		private void TranslationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			e.Handled = true;
			if (this.IsLoaded) {
				Utilities.SetRegistryValue("Locale", ((TranslationComboBoxItem)e.AddedItems[0]).LocaleCode);
				((App)Application.Current).SelectCulture(((TranslationComboBoxItem)e.AddedItems[0]).LocaleCode);

				if (ShellListView.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
					btnCreateFolder.Header = FindResource("btnNewLibraryCP");		//"New Library";
					stNewFolder.Title = FindResource("btnNewLibraryCP").ToString();	//"New Library";
					stNewFolder.Text = "Creates a new library in the current folder.";
				}
				else {
					btnCreateFolder.Header = FindResource("btnNewFolderCP");		//"New Folder";
					stNewFolder.Title = FindResource("btnNewFolderCP").ToString();	//"New Folder";
					stNewFolder.Text = "Creates a new folder in the current folder";
				}
			}
		}

		private void btnRemoveLangSetting_Click(object sender, RoutedEventArgs e) {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);

			rks.DeleteValue(@"Locale");

			rks.Close();
			rk.Close();
		}

		private void TranslationHelp_Click(object sender, RoutedEventArgs e) {
			Process.Start(TranslationURL.Text);
		}

		#endregion

		#region Image Editing

		private void Convert_Images(object sender, RoutedEventArgs e) {
			ImageFormat format = null; string extension = null;

			if (sender == ConvertToJPG) {
				format = ImageFormat.Jpeg;
				extension = ".jpg";
			}
			else if (sender == ConvertToPNG) {
				format = ImageFormat.Png;
				extension = ".png";
			}
			else if (sender == ConvertToGIF) {
				format = ImageFormat.Gif;
				extension = ".gif";
			}
			else if (sender == ConvertToBMP) {
				format = ImageFormat.Bmp;
				extension = ".bmp";
			}
			else if (sender == ConvertToJPG) {
				format = ImageFormat.Wmf;
				extension = ".wmf";
			}
			else {
				throw new Exception("Invalid Sender");
			}

			foreach (ShellItem item in ShellListView.SelectedItems) {
				System.Drawing.Bitmap cvt = new Bitmap(item.ParsingName);
				string namen = Utilities.RemoveExtensionsFromFile(item.ParsingName, new System.IO.FileInfo(item.ParsingName).Extension);
				try {
					AddToLog("Converted Image from " + item.ParsingName + " to new file " + namen + extension);
					cvt.Save(namen + extension, format);
				}
				catch (Exception) {
					MessageBox.Show("There appears to have been an issue with converting the file. Make sure the filename \"" + Utilities.RemoveExtensionsFromFile(ShellListView.GetFirstSelectedItem().DisplayName, new System.IO.FileInfo(item.ParsingName).Extension) + extension + "\" does already not exist.", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				cvt.Dispose();
			}
		}

		private void Set_Wallpaper(object sender, RoutedEventArgs e) {
			Wallpaper.Style ThisStyle;

			if (sender == btnWallpaper)
				ThisStyle = Wallpaper.Style.Stretched;
			else if (sender == miWallFill)
				ThisStyle = Wallpaper.Style.Fill;
			else if (sender == miWallFit)
				ThisStyle = Wallpaper.Style.Fit;
			else if (sender == miWallStretch)
				ThisStyle = Wallpaper.Style.Stretched;
			else if (sender == miWallTile)
				ThisStyle = Wallpaper.Style.Tiled;
			else if (sender == miWallCenter)
				ThisStyle = Wallpaper.Style.Centered;
			else
				throw new Exception("Invalid Sender");

			Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)(() => {
				Wallpaper TheWall = new Wallpaper();
				TheWall.Set(new Uri(ShellListView.GetFirstSelectedItem().ParsingName), ThisStyle);
			}));
		}


		//private void RotateImages(RotateFlipType Rotation, string DefaultName_Addon) {
		//	foreach (ShellItem item in ShellListView.SelectedItems) {
		//		System.Drawing.Bitmap cvt = new Bitmap(item.ParsingName);
		//		cvt.RotateFlip(Rotation);
		//		if (OverwriteOnRotate) {
		//			cvt.Save(item.ParsingName);
		//		}
		//		else {
		//			string ext = Utilities.GetExtension(item.ParsingName);
		//			string name = item.ParsingName;
		//			string namen = Utilities.RemoveExtensionsFromFile(name, new System.IO.FileInfo(name).Extension);
		//			cvt.Save(namen + DefaultName_Addon + ext);
		//		}
		//		cvt.Dispose();
		//		AddToLog("Rotated image " + item.ParsingName);
		//	}
		//}

		private void RotateImages(object sender, RoutedEventArgs e) {
			RotateFlipType Rotation;
			string DefaultName_Addon = null;

			switch ((sender as Control).Name) {
				case "btnRotateLeft":
					Rotation = RotateFlipType.Rotate270FlipNone;
					DefaultName_Addon = "_Rotated270";
					break;
				case "btnRotateRight":
					Rotation = RotateFlipType.Rotate90FlipNone;
					DefaultName_Addon = "_Rotated90";
					break;
				case "btnFlipX":
					Rotation = RotateFlipType.RotateNoneFlipX;
					DefaultName_Addon = "_FlippedX";
					break;
				case "btnFlipY":
					Rotation = RotateFlipType.RotateNoneFlipY;
					DefaultName_Addon = "_FlippedY";
					break;
				default:
					throw new Exception("Invalid sender");
			}

			foreach (ShellItem item in ShellListView.SelectedItems) {
				var cvt = new Bitmap(item.ParsingName);
				cvt.RotateFlip(Rotation);
				if (OverwriteOnRotate) {
					cvt.Save(item.ParsingName);
				}
				else {
					string ext = Utilities.GetExtension(item.ParsingName);
					string name = item.ParsingName;
					string namen = Utilities.RemoveExtensionsFromFile(name, new System.IO.FileInfo(name).Extension);
					cvt.Save(namen + DefaultName_Addon + ext);
				}
				cvt.Dispose();
				AddToLog("Rotated image " + item.ParsingName);
			}
		}

		//[Obsolete("Merge into a multi control event using RotateImages just like Convert_Images(...)!!!!")]
		//private void btnRotateLeft_Click(object sender, RoutedEventArgs e) {
		//	RotateImages(RotateFlipType.Rotate270FlipNone, "_Rotated270");
		//	/*
		//	foreach (ShellItem item in ShellListView.SelectedItems) {
		//		System.Drawing.Bitmap cvt = new Bitmap(item.ParsingName);
		//		cvt.RotateFlip(RotateFlipType.Rotate270FlipNone);
		//		if (OverwriteOnRotate) {
		//			cvt.Save(item.ParsingName);
		//		}
		//		else {
		//			string ext = Utilities.GetExtension(item.ParsingName);
		//			string name = item.ParsingName;
		//			string namen = Utilities.RemoveExtensionsFromFile(name, new System.IO.FileInfo(name).Extension);
		//			cvt.Save(namen + "_Rotated270" + ext);
		//		}
		//		cvt.Dispose();
		//		AddToLog("Rotated image " + item.ParsingName);
		//	}
		//	*/
		//}

		//[Obsolete("Merge into a multi control event using RotateImages just like Convert_Images(...)!!!!")]
		//private void btnRotateRight_Click(object sender, RoutedEventArgs e) {
		//	RotateImages(RotateFlipType.Rotate90FlipNone, "_Rotated90");
		//	/*
		//	foreach (ShellItem item in ShellListView.SelectedItems) {
		//		System.Drawing.Bitmap cvt = new Bitmap(item.ParsingName);
		//		cvt.RotateFlip(RotateFlipType.Rotate90FlipNone);
		//		if (OverwriteOnRotate) {
		//			cvt.Save(item.ParsingName);
		//		}
		//		else {
		//			string ext = Utilities.GetExtension(item.ParsingName);
		//			string name = item.ParsingName;
		//			string namen = Utilities.RemoveExtensionsFromFile(name, new System.IO.FileInfo(name).Extension);
		//			cvt.Save(namen + "_Rotated90" + ext);
		//		}
		//		cvt.Dispose();
		//		AddToLog("Rotated image " + item.ParsingName);
		//	}
		//	*/
		//}

		//[Obsolete("Merge into a multi control event using RotateImages just like Convert_Images(...)!!!!")]
		//private void btnFlipX_Click(object sender, RoutedEventArgs e) {
		//	RotateImages(RotateFlipType.RotateNoneFlipX, "_FlippedX");
		//	/*
		//	foreach (ShellItem item in ShellListView.SelectedItems) {
		//		System.Drawing.Bitmap cvt = new Bitmap(item.ParsingName);
		//		cvt.RotateFlip(RotateFlipType.RotateNoneFlipX);
		//		if (OverwriteOnRotate) {
		//			cvt.Save(item.ParsingName);
		//		}
		//		else {
		//			string ext = Utilities.GetExtension(item.ParsingName);
		//			string name = item.ParsingName;
		//			string namen = Utilities.RemoveExtensionsFromFile(name, new System.IO.FileInfo(name).Extension);
		//			cvt.Save(namen + "_FlippedX" + ext);
		//		}
		//		cvt.Dispose();
		//		AddToLog("Flipped image " + item.ParsingName);
		//	}
		//	*/
		//}

		//[Obsolete("Merge into a multi control event using RotateImages just like Convert_Images(...)!!!!")]
		//private void btnFlipY_Click(object sender, RoutedEventArgs e) {
		//	RotateImages(RotateFlipType.RotateNoneFlipY, "_FlippedY");
		//	/*
		//	foreach (ShellItem item in ShellListView.SelectedItems) {
		//		System.Drawing.Bitmap cvt = new Bitmap(item.ParsingName);
		//		cvt.RotateFlip(RotateFlipType.RotateNoneFlipY);
		//		if (OverwriteOnRotate) {
		//			cvt.Save(item.ParsingName);
		//		}
		//		else {
		//			string ext = Utilities.GetExtension(item.ParsingName);
		//			string name = item.ParsingName;
		//			string namen = Utilities.RemoveExtensionsFromFile(name, new System.IO.FileInfo(name).Extension);
		//			cvt.Save(namen + "_FlippedY" + ext);
		//		}
		//		cvt.Dispose();
		//		AddToLog("Flipped image " + item.ParsingName);
		//	}
		//	*/
		//}

		private void btnResize_Click(object sender, RoutedEventArgs e) {
			//ResizeImage.Open(ShellListView.GetFirstSelectedItem());
		}

		#endregion

		#region Folder Tools Commands

		private void btnChangeFoldericon_Click(object sender, RoutedEventArgs e) {
			new IconView().LoadIcons(this.ShellListView, false);
		}

		private void btnClearFoldericon_Click(object sender, RoutedEventArgs e) {
			ShellListView.ClearFolderIcon(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		#endregion

		#region Registry Setting Changes / BetterExplorerOperations Calls / Action Log

		private Process Rename_CheckChanged_Helper() {
			string ExePath = Utilities.AppDirectoryItem("BetterExplorerOperations.exe");
			Process proc = new Process();
			proc.StartInfo = new ProcessStartInfo {
				FileName = ExePath,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = String.Format("/env /user:Administrator \"{0}\"", ExePath),
			};
			proc.Start();
			Thread.Sleep(1000);

			return proc;
		}

		private void chkIsDefault_CheckChanged(object sender, RoutedEventArgs e) {
			if (isOnLoad) return;
			var proc = Rename_CheckChanged_Helper();

			if (chkIsDefault.IsChecked == true) {
				//int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
				//int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x77654");
				//proc.WaitForExit();
				//if (proc.ExitCode == -1) {
				//	isOnLoad = true;
				//	(sender as Fluent.CheckBox).IsChecked = false;
				//	isOnLoad = false;
				//	MessageBox.Show("Can't set Better Explorer as default!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				//}
			}
			else {
				//WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x77655");
				//proc.WaitForExit();
				//if (proc.ExitCode == -1) {
				//	isOnLoad = true;
				//	(sender as Fluent.CheckBox).IsChecked = true;
				//	isOnLoad = false;
				//	MessageBox.Show("Can't restore default filemanager!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				//}
			}
		}

		private void chkTreeExpand_CheckChanged(object sender, RoutedEventArgs e) {
			if (isOnLoad) return;
			var proc = Rename_CheckChanged_Helper();

			if (chkTreeExpand.IsChecked == true) {
				//int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
				//int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x88775");
				//proc.WaitForExit();
			}
			else {
				//int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
				//int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x88776");
				//proc.WaitForExit();
			}
		}

		private void btnSetCurrentasStartup_Click(object sender, RoutedEventArgs e) {
			backstage.IsOpen = true;
			string CurrentLocString = ShellListView.CurrentFolder.ParsingName;
			tcMain.StartUpLocation = CurrentLocString;
			btnSetCurrentasStartup.Header = ShellListView.CurrentFolder.DisplayName;
			//btnSetCurrentasStartup.Icon = ShellListView.CurrentFolder.Thumbnail.BitmapSource;

			Utilities.SetRegistryValue("StartUpLoc", CurrentLocString);
		}

		private void chkIsFlyout_CheckChanged(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("HFlyoutEnabled", e.RoutedEvent.Name == "Checked" ? 1 : 0);
			}
		}

		private void chkIsTerraCopyEnabled_CheckChanged(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("FileOpExEnabled", e.RoutedEvent.Name == "Checked" ? 1 : 0);
			}
		}

		private void chkIsRestoreTabs_CheckChanged(object sender, RoutedEventArgs e) {
			IsrestoreTabs = e.RoutedEvent.Name == "Checked";
			Utilities.SetRegistryValue("IsRestoreTabs", IsrestoreTabs ? 1 : 0);
		}

		private void gridSplitter1_DragCompleted(object sender, DragCompletedEventArgs e) {
			Utilities.SetRegistryValue("SearchBarWidth", SearchBarColumn.Width.Value);
		}

		private void SearchBarReset_Click(object sender, RoutedEventArgs e) {
			Utilities.SetRegistryValue("SearchBarWidth", SearchBarColumn.Width.Value);
		}

		private void chkShowCheckBoxes_CheckChanged(object sender, RoutedEventArgs e) {
			ShellListView.ShowCheckboxes = e.RoutedEvent.Name == "Checked";
			ShellListView.RefreshContents();
		}

		private void chkOverwriteImages_Checked(object sender, RoutedEventArgs e) {
			OverwriteOnRotate = e.RoutedEvent.Name == "Checked";
			Utilities.SetRegistryValue("OverwriteImageWhileEditing", e.RoutedEvent.Name == "Checked" ? 1 : 0);
		}

		private void chkIsCFO_Click(object sender, RoutedEventArgs e) {
			Utilities.SetRegistryValue("IsCustomFO", chkIsCFO.IsChecked.Value == true ? 1 : 0);
		}

		private void chkRibbonMinimizedGlass_Click(object sender, RoutedEventArgs e) {
			this.IsGlassOnRibonMinimized = chkRibbonMinimizedGlass.IsChecked.Value == true;
			Utilities.SetRegistryValue("RibbonMinimizedGlass", chkRibbonMinimizedGlass.IsChecked.Value == true ? 1 : 0, RegistryValueKind.DWord);

			if (!TheRibbon.IsMinimized) {
			}
			else if (chkRibbonMinimizedGlass.IsChecked.Value) {
				var p = ShellViewHost.TransformToAncestor(this).Transform(new WIN.Point(0, 0));
				this.GlassBorderThickness = new Thickness(8, p.Y, 8, 8);
			}
			else {
				var p = backstage.TransformToAncestor(this).Transform(new WIN.Point(0, 0));
				this.GlassBorderThickness = new Thickness(8, p.Y + backstage.ActualHeight, 8, 8);
			}
		}

		private void chkLogHistory_CheckChanged(object sender, RoutedEventArgs e) {
			canlogactions = e.RoutedEvent.Name == "Checked";
			Utilities.SetRegistryValue("EnableActionLog", e.RoutedEvent.Name == "Checked" ? 1 : 0);
		}

		private void btnShowLogs_Click(object sender, RoutedEventArgs e) {
			try {
				if (!Directory.Exists(logdir)) Directory.CreateDirectory(logdir);
				tcMain.NewTab(logdir);
				this.backstage.IsOpen = false;
			}
			catch (Exception exe) {
				MessageBox.Show("An error occurred while trying to open the logs folder. Please report this issue at http://bugtracker.better-explorer.com/. \r\n\r\n Here is some information about the error: \r\n\r\n" + exe.Message + "\r\n\r\n" + exe.ToString(), "Error While Opening Log Folder", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		#region Change Pane Visibility

		private void btnNavigationPane_CheckChanged(object sender, RoutedEventArgs e) {
			if (isOnLoad) {
			}
			else if (e.RoutedEvent.Name == "Checked") {
				TreeGrid.ColumnDefinitions[0].Width = new GridLength(200);
				Utilities.SetRegistryValue("NavigationPaneEnabled", 1);
			}
			else {
				TreeGrid.ColumnDefinitions[0].Width = new GridLength(1);
				Utilities.SetRegistryValue("NavigationPaneEnabled", 0);
			}
		}

		private void btnPreviewPane_CheckChanged(object sender, RoutedEventArgs e) {
			if (isOnLoad) {
			}
			else if (e.RoutedEvent.Name == "Checked") {
				this.clPreview.Width = new GridLength((double)this.PreviewPaneWidth);
				this.clPreviewSplitter.Width = new GridLength(1);
				var selectedItem = ShellListView.SelectedItems.FirstOrDefault();
				if (selectedItem != null && selectedItem.IsFileSystem && ShellListView.GetSelectedCount() == 1 && !selectedItem.IsFolder) {
					this.Previewer.FileName = selectedItem.ParsingName;
				}

				Utilities.SetRegistryValue("PreviewPaneEnabled", 1);
				this.IsPreviewPaneEnabled = true;
			}
			else {
				this.clPreview.Width = new GridLength(0);
				this.clPreviewSplitter.Width = new GridLength(0);
				this.Previewer.FileName = null;
				Utilities.SetRegistryValue("PreviewPaneEnabled", 0);
				this.IsPreviewPaneEnabled = false;
			}
		}

		private void btnInfoPane_CheckChanged(object sender, RoutedEventArgs e) {
			if (isOnLoad) {
			}
			else if (e.RoutedEvent.Name == "Checked") {
				this.rPreviewPane.Height = new GridLength(this.InfoPaneHeight);
				this.rPreviewPaneSplitter.Height = new GridLength(1);
				Utilities.SetRegistryValue("InfoPaneEnabled", 1);
				this.IsInfoPaneEnabled = true;
			}
			else {
				this.rPreviewPane.Height = new GridLength(0);
				this.rPreviewPaneSplitter.Height = new GridLength(0);
				Utilities.SetRegistryValue("InfoPaneEnabled", 0);
				this.IsInfoPaneEnabled = false;
			}
		}

		#endregion

		#region Breadcrumb Bar

		/*
		private void RibbonWindow_GotFocus(object sender, RoutedEventArgs e) {
			//'Try To Remove this Method!!'

			////breadcrumbBarControl1.ExitEditMode_IfNeeded();
			if (!backstage.IsOpen)
				ShellListView.Focus();
		}
		*/

		/*
		private void SaveHistoryToFile(string relativepath, List<String> history) {
		  // Write each entry to a file. (the "false" parameter makes sure the file is overwritten.)
		  using (StreamWriter sw = new StreamWriter(relativepath, false, Encoding.UTF8)) {
		  foreach (string item in history) {
			if (!String.IsNullOrEmpty(item.Replace("\0", String.Empty)))
			sw.WriteLine(item);
		  }
		  }

		}
		*/

		/*
		void bbi_Drop(object sender, DragEventArgs e) {
		  var pt = e.GetPosition(sender as IInputElement);

		  if ((sender as BreadcrumbItem).Data.IsFileSystem)
		  e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
		  else
		  e.Effects = DragDropEffects.None;

		  switch (e.Effects) {
		  case DragDropEffects.All:
			break;
		  case DragDropEffects.Copy:
			this.ShellListView.DoCopy(e.Data, (sender as BreadcrumbItem).Data);
			break;
		  case DragDropEffects.Link:
			break;
		  case DragDropEffects.Move:
			this.ShellListView.DoMove(e.Data, (sender as BreadcrumbItem).Data);
			break;
		  case DragDropEffects.None:
			break;
		  case DragDropEffects.Scroll:
			break;
		  default:
			break;
		  }

		  var wpt = new BExplorer.Shell.Win32Point() { X = (int)pt.X, Y = (int)pt.Y };
		  DropTargetHelper.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
		}

		void bbi_DragOver(object sender, DragEventArgs e) {
		  e.Handled = true;

		  if ((sender as BreadcrumbItem).Data.IsFileSystem) {
		  e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
		  }
		  else {
		  e.Effects = DragDropEffects.None;
		  }

		  var ptw = new Win32Point();
		  GetCursorPos(ref ptw);
		  e.Handled = true;
		  var wpt = new BExplorer.Shell.Win32Point() { X = ptw.X, Y = ptw.Y };
		  DropTargetHelper.Create.DragOver(ref wpt, (int)e.Effects);
		}

		void bbi_DragLeave(object sender, DragEventArgs e) {
		  DropTargetHelper.Create.DragLeave();
		}

		void bbi_DragEnter(object sender, DragEventArgs e) {
		  if ((sender as BreadcrumbItem).Data.IsFileSystem) {
		  e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
		  }
		  else {
		  e.Effects = DragDropEffects.None;
		  }

		  Win32Point ptw = new Win32Point();
		  GetCursorPos(ref ptw);
		  e.Effects = DragDropEffects.None;
		  BExplorer.Shell.Win32Point wpt = new BExplorer.Shell.Win32Point();
		  wpt.X = (int)ptw.X;
		  wpt.Y = (int)ptw.Y;
		  DropTargetHelper.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
		}
		*/

		#endregion

		#region Search

		public void DoSearch() {
			try {
				if (edtSearchBox.FullSearchTerms != "") {
					//var searchCondition = SearchConditionFactory.ParseStructuredQuery(edtSearchBox.FullSearchTerms);
					//var shellItem = new ShellItem(ShellListView.CurrentFolder.PIDL);
					//var searchFolder = new ShellSearchFolder(searchCondition, shellItem);
					//NavigationController(FileSystemListItem.ToFileSystemItem(this.ShellListView.LVHandle, searchFolder));
					this.ShellListView.Navigate_Full(edtSearchBox.FullSearchTerms, true, true);
					//shellItem.Dispose();
					//searchFolder.Dispose();
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButton.OK);
			}
		}

		private void edtSearchBox_BeginSearch(object sender, SearchRoutedEventArgs e) {
			DoSearch();
		}

		private void btnSearch_Click(object sender, RoutedEventArgs e) {
			DoSearch();
		}

		private void edtSearchBox_RequestCriteriaChange(object sender, SearchRoutedEventArgs e) {
			if (e.SearchTerms.StartsWith("author:"))
				AuthorToggle_Click(sender, new RoutedEventArgs(e.RoutedEvent));
			else if (e.SearchTerms.StartsWith("ext:"))
				ToggleButton_Click_1(sender, new RoutedEventArgs(e.RoutedEvent));
			else if (e.SearchTerms.StartsWith("subject:"))
				SubjectToggle_Click(sender, new RoutedEventArgs(e.RoutedEvent));
			else if (e.SearchTerms.StartsWith("size:"))
				miCustomSize_Click(sender, new RoutedEventArgs(e.RoutedEvent));
			else if (e.SearchTerms.StartsWith("date:"))
				dcCustomTime_Click(sender, new RoutedEventArgs(e.RoutedEvent));
			else if (e.SearchTerms.StartsWith("modified:"))
				dmCustomTime_Click(sender, new RoutedEventArgs(e.RoutedEvent));
			else {
				var T = "You have discovered an error in this program. Please tell us which filter you were trying to edit and any other details we should know. \r\n\r\nYour filter: ";
				MessageBox.Show(T + e.SearchTerms, "Oops! Found a Bug!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void edtSearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
			ctgSearch.Visibility = Visibility.Visible;
			if (!TheRibbon.IsMinimized) TheRibbon.SelectedTabItem = tbSearch;
		}

		private void edtSearchBox_FiltersCleared(object sender, EventArgs e) {
			scSize.IsChecked = false;
			ExtToggle.IsChecked = false;
			AuthorToggle.IsChecked = false;
			SubjectToggle.IsChecked = false;
			dcsplit.IsChecked = false;
			dmsplit.IsChecked = false;

			/*
			foreach (var item in scSize.Items.OfType<MenuItem>().Union(dcsplit.Items.OfType<MenuItem>()).Union(dmsplit.Items.OfType<MenuItem>()) {
			  item.IsChecked = false;
			}
			*/

			foreach (var item in scSize.Items.OfType<MenuItem>()) {
				item.IsChecked = false;
			}
			foreach (var item in dcsplit.Items.OfType<MenuItem>()) {
				item.IsChecked = false;
			}
			foreach (var item in dmsplit.Items.OfType<MenuItem>()) {
				item.IsChecked = false;
			}
		}

		private void MenuItem_Click_3(object sender, RoutedEventArgs e) {
			edtSearchBox.ModifiedCondition = (string)((FrameworkElement)sender).Tag;
			dmsplit.IsChecked = true;
		}

		private void MenuItem_Click_4(object sender, RoutedEventArgs e) {
			edtSearchBox.DateCondition = (string)((FrameworkElement)sender).Tag;
			dcsplit.IsChecked = true;
		}

		private void ToggleButton_Click(object sender, RoutedEventArgs e) {
			((Fluent.ToggleButton)sender).IsChecked = true;
			edtSearchBox.KindCondition = (string)((FrameworkElement)sender).Tag;
			edtSearchBox.Focus();
		}

		private void MenuItem_Click_5(object sender, RoutedEventArgs e) {
			edtSearchBox.SizeCondition = (string)((FrameworkElement)sender).Tag;
			scSize.IsChecked = true;
		}

		private void ToggleButton_Click_1(object sender, RoutedEventArgs e) {
			StringSearchCriteriaDialog dat = new StringSearchCriteriaDialog("ext", edtSearchBox.ExtensionCondition, FindResource("btnExtCP") as string);
			dat.ShowDialog();
			if (dat.Confirm) {
				edtSearchBox.ExtensionCondition = "ext:" + dat.textBox1.Text;
				ExtToggle.IsChecked = dat.textBox1.Text.Length > 0;
			}
			else {
				ExtToggle.IsChecked = Utilities.GetValueOnly("ext", edtSearchBox.ExtensionCondition).Length > 0;
			}
		}

		private void AuthorToggle_Click(object sender, RoutedEventArgs e) {
			StringSearchCriteriaDialog dat = new StringSearchCriteriaDialog("author", edtSearchBox.AuthorCondition, FindResource("btnAuthorCP") as string);
			dat.ShowDialog();
			if (dat.Confirm) {
				edtSearchBox.AuthorCondition = "author:" + dat.textBox1.Text;
				AuthorToggle.IsChecked = dat.textBox1.Text.Length > 0;
			}
			else {
				AuthorToggle.IsChecked = Utilities.GetValueOnly("author", edtSearchBox.AuthorCondition).Length > 0;
			}
		}

		private void SubjectToggle_Click(object sender, RoutedEventArgs e) {
			StringSearchCriteriaDialog dat = new StringSearchCriteriaDialog("subject", edtSearchBox.SubjectCondition, FindResource("btnSubjectCP") as string);
			dat.ShowDialog();
			if (dat.Confirm) {
				edtSearchBox.SubjectCondition = "subject:" + dat.textBox1.Text;
				SubjectToggle.IsChecked = dat.textBox1.Text.Length > 0;
			}
			else {
				SubjectToggle.IsChecked = Utilities.GetValueOnly("subject", edtSearchBox.SubjectCondition).Length > 0;
			}
		}

		private void miCustomSize_Click(object sender, RoutedEventArgs e) {
			SizeSearchCriteriaDialog dat = new SizeSearchCriteriaDialog();
			string sd = Utilities.GetValueOnly("size", edtSearchBox.SizeCondition);
			dat.curval.Text = sd;
			dat.ShowDialog();

			if (dat.Confirm) {
				edtSearchBox.SizeCondition = "size:" + dat.GetSizeQuery();
				scSize.IsChecked = dat.GetSizeQuery().Length > 0;
			}
			else {
				scSize.IsChecked = dat.GetSizeQuery().Length > 5;
			}
		}

		private void dcCustomTime_Click(object sender, RoutedEventArgs e) {
			SDateSearchCriteriaDialog star = new SDateSearchCriteriaDialog(FindResource("btnODateCCP") as string);

			star.DateCriteria = Utilities.GetValueOnly("date", edtSearchBox.DateCondition);
			//star.textBlock1.Text = "Set Date Created Filter";
			star.ShowDialog();

			if (star.Confirm) {
				edtSearchBox.DateCondition = "date:" + star.DateCriteria;
			}

			dcsplit.IsChecked = edtSearchBox.UseDateCondition;
		}

		private void dmCustomTime_Click(object sender, RoutedEventArgs e) {
			SDateSearchCriteriaDialog star = new SDateSearchCriteriaDialog(FindResource("btnODateModCP") as string);

			star.DateCriteria = Utilities.GetValueOnly("modified", edtSearchBox.ModifiedCondition);
			//star.textBlock1.Text = "Set Date Modified Filter";
			star.ShowDialog();

			if (star.Confirm) {
				edtSearchBox.ModifiedCondition = "modified:" + star.DateCriteria;
			}

			dmsplit.IsChecked = edtSearchBox.UseModifiedCondition;
		}

		#endregion

		#region AutoSwitch

		private void chkFolder_CheckChanged(object sender, RoutedEventArgs e) {
			asFolder = e.RoutedEvent.Name == "Checked";
		}

		/*
		private void chkFolder_Checked(object sender, RoutedEventArgs e) {
		  asFolder = true;
		}

		private void chkFolder_Unchecked(object sender, RoutedEventArgs e) {
		  asFolder = false;
		}
		*/


		private void chkDrive_CheckChanged(object sender, RoutedEventArgs e) {
			asDrive = e.RoutedEvent.Name == "Checked";
		}

		/*
		private void chkDrive_Checked(object sender, RoutedEventArgs e) {
		  asDrive = true;
		}

		private void chkDrive_Unchecked(object sender, RoutedEventArgs e) {
		  asDrive = false;
		}
		*/


		private void chkArchive_CheckChanged(object sender, RoutedEventArgs e) {
			asArchive = e.RoutedEvent.Name == "Checked";
		}

		/*
		private void chkArchive_Checked(object sender, RoutedEventArgs e) {
		  asArchive = true;
		}

		private void chkArchive_Unchecked(object sender, RoutedEventArgs e) {
		  asArchive = false;
		}
		*/


		private void chkApp_CheckChanged(object sender, RoutedEventArgs e) {
			asApplication = e.RoutedEvent.Name == "Checked";
		}

		/*
		private void chkApp_Checked(object sender, RoutedEventArgs e) {
		  asApplication = true;
		}

		private void chkApp_Unchecked(object sender, RoutedEventArgs e) {
		  asApplication = false;
		}
		*/

		private void chkImage_CheckChanged(object sender, RoutedEventArgs e) {
			asImage = e.RoutedEvent.Name == "Checked";
		}

		/*
		private void chkImage_Checked(object sender, RoutedEventArgs e) {
		  asImage = true;
		}

		private void chkImage_Unchecked(object sender, RoutedEventArgs e) {
		  asImage = false;
		}
		*/


		private void chkLibrary_CheckChanged(object sender, RoutedEventArgs e) {
			asLibrary = e.RoutedEvent.Name == "Checked";
		}

		/*
		private void chkLibrary_Checked(object sender, RoutedEventArgs e) {
		  asLibrary = true;
		}

		private void chkLibrary_Unchecked(object sender, RoutedEventArgs e) {
		  asLibrary = false;
		}
		*/

		private void chkVirtualTools_CheckChanged(object sender, RoutedEventArgs e) {
			asVirtualDrive = e.RoutedEvent.Name == "Checked";
		}

		/*
		private void chkVirtualTools_Checked(object sender, RoutedEventArgs e) {
		  asVirtualDrive = true;
		}

		private void chkVirtualTools_Unchecked(object sender, RoutedEventArgs e) {
		  asVirtualDrive = false;
		}
		*/
		#endregion

		#region Tabs
		void mim_Click(object sender, RoutedEventArgs e) {
			MenuItem mi = sender as MenuItem;
			SetFOperation((mi.Tag as IListItemEx), BExplorer.Shell.OperationType.Move);
		}

		private void btnNewTab_Click(object sender, RoutedEventArgs e) {
			tcMain.NewTab();
		}

		private void btnTabClone_Click(object sender, RoutedEventArgs e) {
			tcMain.CloneTabItem(tcMain.SelectedItem as Wpf.Controls.TabItem);
		}

		private void btnTabCloseC_Click(object sender, RoutedEventArgs e) {
			tcMain.RemoveTabItem(tcMain.SelectedItem as Wpf.Controls.TabItem);
		}

		private void btnUndoClose_Click(object sender, RoutedEventArgs e) {
			tcMain.ReOpenTab(tcMain.ReopenableTabs[tcMain.ReopenableTabs.Count - 1]);
			btnUndoClose.IsEnabled = tcMain.ReopenableTabs.Any();
		}

		void gli_Click(object sender, NavigationLogEventArgs e) {
			tcMain.ReOpenTab(e.NavigationLog);
			btnUndoClose.IsEnabled = tcMain.ReopenableTabs.Any();
		}

		void gli_Click(object sender, Tuple<string> e) {
			var list = SavedTabsList.LoadTabList(String.Format("{0}{1}.txt", sstdir, e.Item1));
			for (int i = 0; i < list.Count; i++) {
				var tabitem = tcMain.NewTab(list[i].ToShellParsingName());
				if (i == list.Count - 1)
					tcMain.SelectedItem = tabitem;
			}
		}

		private void GoToSearchBox(object sender, ExecutedRoutedEventArgs e) {
			edtSearchBox.Focus();
			Keyboard.Focus(edtSearchBox);
		}
		void newt_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			tcMain.IsInTabDragDrop = false;
			//this._IsTabSelectionChangedNotAllowed = tcMain.IsInTabDragDrop;
		}
		void newt_PreviewMouseMove(object sender, MouseEventArgs e) {
			var tabItem = e.Source as Wpf.Controls.TabItem;

			if (tabItem == null) {
				tcMain.IsInTabDragDrop = false;
				return;
			}
			else if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) {
				//if (tabItem.IsSelected) {
				//  this._IsTabSelectionChangedAllowed = false;
				//}

				DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
			}
		}
		void newt_Leave(object sender, DragEventArgs e) {
			DropTargetHelper.Create.DragLeave();
		}
		void newt_Drop(object sender, DragEventArgs e) {
			e.Handled = true;
			var tabItemTarget = e.Source as Wpf.Controls.TabItem;

			var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;
			if (tabItemSource != null) {
				if (!tabItemTarget.Equals(tabItemSource)) {
					var tabControl = tabItemTarget.Parent as TabControl;
					int tabState = -1;
					int targetIndex = tabControl.Items.IndexOf(tabItemTarget);


					//int sourceIndex = tabControl.Items.IndexOf(tabItemSource);


					//if (!tabItemSource.IsSelected && tabItemTarget.IsSelected)
					//  tabState = 1;
					//else if (!tabItemSource.IsSelected && !tabItemTarget.IsSelected)
					//  tabState = 2;
					//else
					//  tabState = 0;
					if (tabItemSource == this._CurrentlySelectedItem) {
						tabState = 0;
					}
					else {
						tabState = 1;
					}

					tabControl.Items.Remove(tabItemSource);
					tabControl.Items.Insert(targetIndex, tabItemSource);
					tcMain.IsInTabDragDrop = false;
					if (tabState == 1)
						tabControl.SelectedItem = this._CurrentlySelectedItem; //tabControl.Items.IndexOf(tabItemSource);
					else if (tabState == 0) {
						tabControl.SelectedIndex = targetIndex;
					}
					tcMain.IsInTabDragDrop = true;
				}
			}
			else {
				if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem) {
					e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;

					switch (e.Effects) {
						case DragDropEffects.All:
							break;
						case DragDropEffects.Copy:
							this.ShellListView.DoCopy(e.Data, (sender as Wpf.Controls.TabItem).ShellObject);
							break;
						case DragDropEffects.Link:
							break;
						case DragDropEffects.Move:
							this.ShellListView.DoMove(e.Data, (sender as Wpf.Controls.TabItem).ShellObject);
							break;
						case DragDropEffects.None:
							break;
						case DragDropEffects.Scroll:
							break;
						default:
							break;
					}
				}
				else {
					e.Effects = DragDropEffects.None;
				}

				WIN.Point pt = e.GetPosition(sender as IInputElement);
				var wpt = new BExplorer.Shell.DataObject.Win32Point() { X = (int)pt.X, Y = (int)pt.Y };
				DropTargetHelper.Create.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
				this._TabDropData = null;
			}
		}

		void newt_DragOver(object sender, DragEventArgs e) {
			e.Handled = true;

			var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;

			if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem)
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
			else {
				if (tabItemSource != null) {
					e.Effects = DragDropEffects.Move;
				}
				else {
					e.Effects = DragDropEffects.None;
				}
			}
			BExplorer.Shell.DataObject.DropDescription desc = new BExplorer.Shell.DataObject.DropDescription();


			switch (e.Effects) {
				case DragDropEffects.Copy:
					desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Copy;
					desc.szMessage = "Copy To %1";
					break;
				case DragDropEffects.Move:
					desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Move;
					desc.szMessage = "Move To %1";
					break;
				case DragDropEffects.None:
					desc.type = (int)BExplorer.Shell.DataObject.DropImageType.None;
					desc.szMessage = "Cant drop here!";
					break;
				default:
					desc.type = (int)BExplorer.Shell.DataObject.DropImageType.Invalid;
					desc.szMessage = "";
					break;
			}
			desc.szInsert = (sender as Wpf.Controls.TabItem).ShellObject.DisplayName;
			((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data).SetDropDescription(desc);

			var ptw = new BExplorer.Shell.DataObject.Win32Point();
			GetCursorPos(ref ptw);

			if (e.Data.GetType() != typeof(Wpf.Controls.TabItem)) {
				var wpt = new BExplorer.Shell.DataObject.Win32Point() { X = ptw.X, Y = ptw.Y };
				DropTargetHelper.Create.DragOver(ref wpt, (int)e.Effects);
			}
		}
		System.Runtime.InteropServices.ComTypes.IDataObject _TabDropData;
		void newt_DragEnter(object sender, DragEventArgs e) {
			e.Handled = true;
			var tabItem = e.Source as Wpf.Controls.TabItem;
			if (tabItem == null) return;

			this._TabDropData = (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data;

			if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem)
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
			else
				e.Effects = DragDropEffects.None;

			var ptw = new BExplorer.Shell.DataObject.Win32Point();
			GetCursorPos(ref ptw);
			e.Effects = DragDropEffects.None;
			var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;

			if (tabItemSource == null) {
				var wpt = new BExplorer.Shell.DataObject.Win32Point() { X = ptw.X, Y = ptw.Y };
				DropTargetHelper.Create.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
			}
			else if (e.Data.GetDataPresent(typeof(Wpf.Controls.TabItem))) {
				e.Effects = DragDropEffects.Move;
			}
		}

		#endregion

		#region Tab Controls

		private void FolderTabs_Placement(object sender, RoutedEventArgs e) {
			if (sender == TabbaTop) {
				Grid.SetRow(this.tcMain, 3);
				divNav.Visibility = Visibility.Hidden;
				this.rTabbarTop.Height = new GridLength(25);
				this.rTabbarBot.Height = new GridLength(0);
				this.tcMain.TabStripPlacement = Dock.Top;
			}
			else {
				Grid.SetRow(this.tcMain, 7);
				divNav.Visibility = Visibility.Visible;
				this.rTabbarTop.Height = new GridLength(0);
				this.rTabbarBot.Height = new GridLength(25);
				this.tcMain.TabStripPlacement = Dock.Bottom;
			}
		}

		private void miSaveCurTabs_Click(object sender, RoutedEventArgs e) {
			var objs = new List<IListItemEx>(from Wpf.Controls.TabItem x in tcMain.Items select x.ShellObject);
			String str = PathStringCombiner.CombinePaths(objs, "|");
			var list = SavedTabsList.CreateFromString(str);

			var Name = BetterExplorer.Tabs.NameTabList.Open(this);
			if (Name == null) return;

			if (!Directory.Exists(sstdir)) Directory.CreateDirectory(sstdir);

			SavedTabsList.SaveTabList(list, String.Format("{0}{1}.txt", sstdir, Name));
			miTabManager.IsEnabled = true;
		}

		private void miClearUndoList_Click(object sender, RoutedEventArgs e) {
			tcMain.ReopenableTabs.Clear();
			btnUndoClose.IsDropDownOpen = false;
			btnUndoClose.IsEnabled = false;

			foreach (Wpf.Controls.TabItem item in this.tcMain.Items) {
				foreach (FrameworkElement m in item.mnu.Items) {
					if (m.Tag != null)
						if (m.Tag.ToString() == "UCTI") (m as MenuItem).IsEnabled = false;
				}
			}
		}

		private void btnUndoClose_DropDownOpened(object sender, EventArgs e) {
			rotGallery.Items.Clear();
			foreach (NavigationLog item in tcMain.ReopenableTabs) {
				var gli = new UndoCloseGalleryItem();
				gli.LoadData(item);
				gli.Click += gli_Click;
				rotGallery.Items.Add(gli);
			}
		}

		private void rotGallery_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			e.Handled = true;
			var Item = (e.AddedItems[0] as UndoCloseGalleryItem);
			if (Item != null) {
				Item.PerformClickEvent();
			}
		}

		private void btnChangeTabsFolder_Click(object sender, RoutedEventArgs e) {
			var ctf = new FolderSelectDialog();
			ctf.Title = "Change Tab Folder";
			ctf.InitialDirectory = new DirectoryInfo(sstdir).Parent.FullName;
			if (ctf.ShowDialog()) {
				Utilities.SetRegistryValue("SavedTabsDirectory", ctf.FileName + "\\");
				txtDefSaveTabs.Text = ctf.FileName + "\\";
				sstdir = ctf.FileName + "\\";
			}
		}

		private void stGallery_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			e.Handled = true;
			if (e.AddedItems.Count > 0) {
				(e.AddedItems[0] as SavedTabsListGalleryItem).PerformClickEvent();
			}
		}

		private void btnSavedTabs_DropDownOpened(object sender, EventArgs e) {
			var o = new List<string>();

			if (Directory.Exists(sstdir)) {
				foreach (string item in Directory.GetFiles(sstdir)) {
					var obj = new ShellItem(item);
					o.Add(Utilities.RemoveExtensionsFromFile(obj.GetDisplayName(SIGDN.NORMALDISPLAY), Utilities.GetExtension(item)));
				}
			}

			stGallery.Items.Clear();
			foreach (string item in o) {
				var gli = new SavedTabsListGalleryItem(item);
				gli.Directory = sstdir;
				gli.Click += gli_Click;
				gli.SetUpTooltip((FindResource("tabTabsCP") as string));
				stGallery.Items.Add(gli);
			}
		}

		private void miTabManager_Click(object sender, RoutedEventArgs e) {
			string sstdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\";
			if (Directory.Exists(sstdir)) {
				var man = new Tabs.TabManager();
				man.MainForm = this;
				man.Show();
			}
		}

		private void RibbonWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
			if (tcMain.SelectedItem != null) {
				(tcMain.SelectedItem as Wpf.Controls.TabItem).BringIntoView();
			}
		}

		#endregion

		#region Recycle Bin

		public void UpdateRecycleBinInfos() {
			var rb = KnownFolders.RecycleBin;
			int count = rb.Count();
			//TODO: Find out if we can remove the Dispatcher.BeginInvoke(...)

			if (rb.Any()) {
				var size = (long)rb.Where(c => c.IsFolder == false || !String.IsNullOrEmpty(Path.GetExtension(c.ParsingName))).Sum(c => c.GetPropertyValue(SystemProperties.FileSize, typeof(long)).IsNullOrEmpty ? 0 : (long)Convert.ToDouble(c.GetPropertyValue(SystemProperties.FileSize, typeof(long)).Value));
				Dispatcher.BeginInvoke(WIN.Threading.DispatcherPriority.Background,
					(ThreadStart)(() => {
						miRestoreALLRB.Visibility = Visibility.Visible;
						miEmptyRB.Visibility = Visibility.Visible;
						btnRecycleBin.LargeIcon = @"..\Images\RecycleBinFull32.png";
						btnRecycleBin.Icon = @"..\Images\RecycleBinFull16.png";
						btnRecycleBin.UpdateLayout();
						lblRBItems.Visibility = Visibility.Visible;
						lblRBItems.Text = String.Format("{0} Items", count);
						lblRBSize.Text = ShlWapi.StrFormatByteSize(size);
						lblRBSize.Visibility = Visibility.Visible;
					}));
			}
			else {
				Dispatcher.BeginInvoke(WIN.Threading.DispatcherPriority.Background,
					(ThreadStart)(() => {
						miEmptyRB.Visibility = Visibility.Collapsed;
						miRestoreALLRB.Visibility = Visibility.Collapsed;
						miRestoreRBItems.Visibility = Visibility.Collapsed;
						btnRecycleBin.LargeIcon = @"..\Images\RecycleBinEmpty32.png";
						btnRecycleBin.Icon = @"..\Images\RecycleBinEmpty16.png";
						lblRBItems.Text = "0 Items";
						lblRBItems.Visibility = Visibility.Collapsed;
						lblRBSize.Text = "0 bytes";
						lblRBSize.Visibility = Visibility.Collapsed;
					}));
			}
		}

		private void miEmptyRB_Click(object sender, RoutedEventArgs e) {
			BExplorer.Shell.Interop.Shell32.SHEmptyRecycleBin(this.Handle, string.Empty, 0);
			UpdateRecycleBinInfos();
		}

		private void miOpenRB_Click(object sender, RoutedEventArgs e) {
			NavigationController(FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ((ShellItem)KnownFolders.RecycleBin).Pidl));
		}

		private void miRestoreRBItems_Click(object sender, RoutedEventArgs e) {
			//TODO: Check these changes

			foreach (ShellItem item in ShellListView.SelectedItems.ToArray()) {
				Folder Recycler = new Shell().NameSpace(10);
				for (int i = 0; i < Recycler.Items().Count; i++) {
					Shell32.FolderItem FI = Recycler.Items().Item(i);
					string FileName = Recycler.GetDetailsOf(FI, 0);
					if (Path.GetExtension(FileName) == "") FileName += Path.GetExtension(FI.Path);
					//Necessary for systems with hidden file extensions.
					string FilePath = Recycler.GetDetailsOf(FI, 1);
					if (item.FileSystemPath == Path.Combine(FilePath, FileName)) {
						DoVerb(FI, "ESTORE");
						break;
					}
				}
			}

			UpdateRecycleBinInfos();
		}

		private bool DoVerb(Shell32.FolderItem Item, string Verb) {
			//TODO: Test
			var Found = Item.Verbs().OfType<FolderItemVerb>().SingleOrDefault(FIVerb => FIVerb.Name.ToUpper().Contains(Verb.ToUpper()));
			if (Found != null) Found.DoIt();
			return Found != null;

			/**********************************/
			/**********************************/

			foreach (FolderItemVerb FIVerb in Item.Verbs()) {
				if (FIVerb.Name.ToUpper().Contains(Verb.ToUpper())) {
					FIVerb.DoIt();
					return true;
				}
			}

			return false;
		}

		private void miRestoreALLRB_Click(object sender, RoutedEventArgs e) {
			//var Shl = new Shell();
			Folder Recycler = new Shell().NameSpace(10);

			for (int i = 0; i < Recycler.Items().Count; i++) {
				Shell32.FolderItem FI = Recycler.Items().Item(i);
				DoVerb(FI, "ESTORE");
				//TODO: Why do we have a break here
				break;
			}

			UpdateRecycleBinInfos();
		}

		#endregion

		#region Networks and Accounts ("Sharing Options")

		private void btnAddWebServer_Click(object sender, RoutedEventArgs e) {
			Networks.AddServer asw = new Networks.AddServer();
			asw.Owner = this;
			asw.ShowDialog();
			if (asw.yep) {
				NetworkItem ni = asw.GetNetworkItem();
				nam.Add(ni);
				ServerItem ui = new ServerItem();
				ui.RequestRemove += ui_RequestRemove;
				ui.RequestEdit += ui_RequestEdit;
				ui.LoadFromNetworkItem(ni);
				//pnlServers.Children.Add(ui);
			}
		}

		void ui_RequestEdit(object sender, NetworkItemEventArgs e) {
			Networks.AddServer asw = new Networks.AddServer();
			asw.Owner = this;
			asw.ImportNetworkItem(e.NetworkItem);
			asw.ShowDialog();
			if (asw.yep) {
				nam.Remove(e.NetworkItem);
				//pnlServers.Children.Remove(sender as ServerItem);
				NetworkItem ni = asw.GetNetworkItem();
				nam.Add(ni);
				ServerItem ui = new ServerItem();
				ui.RequestRemove += ui_RequestRemove;
				ui.RequestEdit += ui_RequestEdit;
				ui.LoadFromNetworkItem(ni);
				//pnlServers.Children.Add(ui);
			}
		}

		void ui_RequestRemove(object sender, NetworkItemEventArgs e) {
			if (MessageBox.Show("Are you sure you want to remove this account?", "Remove Account", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
				nam.Remove(e.NetworkItem);
				//pnlServers.Children.Remove(sender as ServerItem);
			}
		}

		private void btnAddStorageService_Click(object sender, RoutedEventArgs e) {
			AccountAuthWindow aaw = new AccountAuthWindow();
			aaw.LoadStorageServices();
			aaw.LoadSocialMediaServices();
			aaw.Owner = this;
			aaw.ShowDialog();
		}

		private void btnAddSocialMedia_Click(object sender, RoutedEventArgs e) {
			AccountAuthWindow aaw = new AccountAuthWindow();
			aaw.LoadSocialMediaServices();
			aaw.Owner = this;
			aaw.ShowDialog();
		}

		#endregion

		#region Console

		private void ctrlConsole_OnConsoleInput(object sender, Tuple<string> args) {

			if (args.Item1.Trim().ToLowerInvariant().StartsWith("cd")) {
				//this.ShellListView.Navigate(new ShellItem(args.Item1.ToLowerInvariant().Replace("cd", String.Empty).Replace("/d", String.Empty).Trim()));
				NavigationController(FileSystemListItem.ToFileSystemItem(this.ShellListView.LVHandle, args.Item1.ToLowerInvariant().Replace("cd", String.Empty).Replace("/d", String.Empty).Trim()));
			}
			Fluent.MenuItem cmi = new MenuItem();
			cmi.Header = args.Item1;
			cmi.Click += cmi_Click;
			this.btnConsoleHistory.Items.Add(cmi);
		}

		void cmi_Click(object sender, RoutedEventArgs e) {
			var item = sender as Fluent.MenuItem;
			this.ctrlConsole.EnqueleInput(item.Header.ToString());
		}

		private void ConsoleClear_Click(object sender, RoutedEventArgs e) {
			this.ctrlConsole.ClearConsole();
		}

		#endregion

		#region EasyAccess

		private void btnEasyAccess_DropDownOpened(object sender, EventArgs e) {
			if (ShellListView.GetSelectedCount() == 1 && ShellListView.GetFirstSelectedItem().IsFolder) {
				mnuIncludeInLibrary.Items.Clear();

				foreach (ShellItem lib in KnownFolders.Libraries) {
					lib.Thumbnail.CurrentSize = new WIN.Size(16, 16);
					mnuIncludeInLibrary.Items.Add(
					  Utilities.Build_MenuItem(lib.DisplayName, ShellLibrary.Load(Path.GetFileNameWithoutExtension(lib.ParsingName), true),
							 lib.Thumbnail.BitmapSource, onClick: mli_Click)
					);
				}

				mnuIncludeInLibrary.Items.Add(new Separator());

				var mln = new MenuItem() { Header = "Create new library" };
				mln.Click += mln_Click;
				mnuIncludeInLibrary.Items.Add(mln);

				mnuIncludeInLibrary.IsEnabled = true;
			}
			else {
				mnuIncludeInLibrary.IsEnabled = false;
			}
		}

		void mln_Click(object sender, RoutedEventArgs e) {
			var lib = ShellListView.CreateNewLibrary(ShellListView.GetFirstSelectedItem().DisplayName);
			if (ShellListView.GetFirstSelectedItem().IsFolder)
				lib.Add(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		void mli_Click(object sender, RoutedEventArgs e) {
			var lib = ShellLibrary.Load(Path.GetFileNameWithoutExtension(((ShellItem)(sender as Fluent.MenuItem).Tag).ParsingName), false);
			if (ShellListView.GetFirstSelectedItem().IsFolder)
				lib.Add(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		#endregion

		#region ShellListView

		[Obsolete("try to move this into ShellViewEx")]
		void ShellListView_EndItemLabelEdit(object sender, bool e) {
			//this.ShellListView.NewName = this.txtEditor.Text;
			ShellListView.FileNameChangeAttempt(this.txtEditor.Text, e);

			this.Editor.Visibility = WIN.Visibility.Collapsed;
			this.Editor.IsOpen = false;
		}

		[Obsolete("try to move this into ShellViewEx")]
		void ShellListView_BeginItemLabelEdit(object sender, RenameEventArgs e) {
			if (this.Editor.IsOpen) return;
			var isSmall = this.ShellListView.IconSize == 16;
			if (isSmall) {
				this.txtEditor.TextWrapping = TextWrapping.WrapWithOverflow;
				this.txtEditor.TextAlignment = TextAlignment.Left;
			}
			else {
				this.txtEditor.TextWrapping = TextWrapping.Wrap;
				this.txtEditor.TextAlignment = TextAlignment.Center;
			}
			var itemRect = this.ShellListView.GetItemBounds(e.ItemIndex, 0);
			var itemLabelRect = this.ShellListView.GetItemBounds(e.ItemIndex, 2);
			this.txtEditor.Text = this.ShellListView.Items[e.ItemIndex].DisplayName;
			var point = this.ShellViewHost.PointToScreen(new WIN.Point(isSmall ? itemLabelRect.Left : itemRect.Left, itemLabelRect.Top - (isSmall ? 1 : 0)));
			this.Editor.HorizontalOffset = point.X;
			this.Editor.VerticalOffset = point.Y;

			this.txtEditor.MaxWidth = isSmall ? Double.PositiveInfinity : itemRect.Width;
			this.txtEditor.MaxHeight = isSmall ? itemLabelRect.Height + 2 : Double.PositiveInfinity;

			this.Editor.Width = isSmall ? this.txtEditor.Width : itemRect.Width;
			this.Editor.Height = this.txtEditor.Height + 2;
			this.Editor.Visibility = WIN.Visibility.Visible;
			this.Editor.IsOpen = true;
			this.txtEditor.Focus();

			if (this.chkExtensions.IsChecked.Value & this.txtEditor.Text.Contains(".") && !this.ShellListView.GetFirstSelectedItem().IsFolder) {
				var lastIndexOfDot = this.txtEditor.Text.LastIndexOf(".");
				this.txtEditor.SelectionStart = 0;
				this.txtEditor.SelectionLength = lastIndexOfDot;
			}
			else {
				this.txtEditor.SelectAll();
			}
			Keyboard.Focus(this.txtEditor);
		}

		void ShellListView_Navigating(object sender, NavigatingEventArgs e) {
			//tcMain.IsInTabDragDrop = true;
			if (this.ShellListView.CurrentFolder == null) return;
			//if (this.bcbc.OnNavigate == null)
			//this.bcbc.OnNavigate = NavigationController;
			Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => {
				var tab = tcMain.SelectedItem as Wpf.Controls.TabItem;
				if (tab != null && this.ShellListView.GetSelectedCount() > 0) {
					if (tab.SelectedItems != null)
						tab.SelectedItems.AddRange(this.ShellListView.SelectedItems.Select(s => s.ParsingName).ToList());
					else
						tab.SelectedItems = this.ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
				}
				this.Title = "Better Explorer - " + e.Folder.DisplayName;
			}));
			//e.Folder.Thumbnail.CurrentSize = new WIN.Size(16, 16);
			//e.Folder.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;

			//var selectedTabItem = tcMain.SelectedItem as Wpf.Controls.TabItem;
			//selectedTabItem.Header = e.Folder.DisplayName;
			//selectedTabItem.Icon = e.Folder.Thumbnail.BitmapSource;
			//selectedTabItem.ShellObject = e.Folder;
			//selectedTabItem.ToolTip = e.Folder.ParsingName;
		}

		/*
		[Obsolete("I don't think we need this")]
		void ShellListView_ItemDisplayed(object sender, ItemDisplayedEventArgs e) {
		  //Date: 8/19/2014	User: Aaron	Note: Added [return;] because the following code seems pointless
		  return;

		  var selectedItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
		  if (selectedItem != null) {
		  var selectedPaths = selectedItem.SelectedItems;
		  var path = e.DisplayedItem.ParsingName;
		  if (selectedPaths != null && selectedPaths.Contains(path)) {
			this.ShellListView.SelectItemByIndex(e.DisplayedItemIndex, true);
			selectedPaths.Remove(path);
		  }
		  }
		}
		*/

		void ShellTree_NodeClick(object sender, WIN.Forms.TreeNodeMouseClickEventArgs e) {
			if (e.Button == WIN.Forms.MouseButtons.Middle) {
				if (e.Node != null && e.Node.Tag != null) {
					var item = e.Node.Tag as IListItemEx;
					if (item != null && item.IsLink) {
						var shellLink = new ShellLink(item.ParsingName);
						item = FileSystemListItem.ToFileSystemItem(this.ShellListView.LVHandle, shellLink.TargetPIDL);
						shellLink.Dispose();
					}

					if (item != null)
						tcMain.NewTab(item, false);
				}
			}
		}

		WIN.Forms.Timer _keyjumpTimer = new WIN.Forms.Timer();

		void ShellListView_KeyJumpTimerDone(object sender, EventArgs e) {
			if (_keyjumpTimer != null) {
				_keyjumpTimer.Stop();
				_keyjumpTimer.Start();
			}
		}

		void _keyjumpTimer_Tick(object sender, EventArgs e) {
			//key jump done
			KeyJumpGrid.IsOpen = false;
			(sender as WIN.Forms.Timer).Stop();
		}

		void ShellListView_KeyJumpKeyDown(object sender, WIN.Forms.KeyEventArgs e) {
			//add key for key jump
			KeyJumpGrid.IsOpen = true;
			txtKeyJump.Text = ShellListView.KeyJumpString;
		}

		void ShellListView_ColumnHeaderRightClick(object sender, WIN.Forms.MouseEventArgs e) {
			//is where the more columns menu should be added
			chcm.IsOpen = true;
		}

		void ShellListView_ItemUpdated(object sender, ItemUpdatedEventArgs e) {
			Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => {
				if (e.UpdateType == ItemUpdateType.RecycleBin) {
					this.UpdateRecycleBinInfos();
				}
				if (e.UpdateType != ItemUpdateType.Renamed && e.UpdateType != ItemUpdateType.Updated) {
					int ItemsCount = ShellListView.Items.Count;
					sbiItemsCount.Visibility = ItemsCount == 0 ? Visibility.Collapsed : Visibility.Visible;
					sbiItemsCount.Content = ItemsCount == 1 ? "1 item" : ItemsCount + " items";
				}
				if (e.UpdateType == ItemUpdateType.Created && this.ShellListView.IsRenameNeeded) {
					ShellListView.SelectItemByIndex(e.NewItemIndex, true, true);
					ShellListView.RenameSelectedItem();
					this.ShellListView.IsRenameNeeded = false;
				}
				if (e.UpdateType == ItemUpdateType.DriveRemoved) {
					foreach (var tab in this.tcMain.Items.OfType<Wpf.Controls.TabItem>().ToArray().Where(w => w.ShellObject.ParsingName.StartsWith(e.NewItem.ParsingName))) {
						this.tcMain.RemoveTabItem(tab, false);
					}
				}
				this.ShellListView.Focus();
			}));
		}

		void ShellListView_SelectionChanged(object sender, EventArgs e) {
			SetupUIOnSelectOrNavigate();

			if (this.IsInfoPaneEnabled) {
				Task.Run(() => this.DetailsPanel.FillPreviewPane(this.ShellListView));
			}

			SetUpStatusBarOnSelectOrNavigate(ShellListView.GetSelectedCount());
		}

		#endregion

		#region On Navigated

		void ShellListView_Navigated(object sender, NavigatedEventArgs e) {
			//Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
			SetupUIOnSelectOrNavigate();

			if (e.OldFolder != this.ShellListView.CurrentFolder) {
				//NavigationController(this.ShellListView.CurrentFolder);
			}
			//if (!e.Folder.Equals(this.ShellListView.CurrentFolder)) {
			this.bcbc.SetPathWithoutNavigate(this.ShellListView.CurrentFolder.ParsingName.ToShellParsingName());
			//}
			Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
				SetupColumnsButton();
				SetSortingAndGroupingButtons();



				if (!tcMain.isGoingBackOrForward) {
					var Current = (tcMain.SelectedItem as Wpf.Controls.TabItem).log;
					Current.ClearForwardItems();
					if (Current.CurrentLocation != e.Folder) Current.CurrentLocation = e.Folder;
				}

				tcMain.isGoingBackOrForward = false;

				SetupUIonNavComplete(e);

				if (this.IsConsoleShown)
					ctrlConsole.ChangeFolder(e.Folder.ParsingName, e.Folder.IsFileSystem);
			}));

			Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
				ConstructMoveToCopyToMenu();
				SetUpJumpListOnNavComplete();
				SetUpButtonVisibilityOnNavComplete(SetUpNewFolderButtons());
			}));


			if (this.IsInfoPaneEnabled) {
				Task.Run(() => {
					this.DetailsPanel.FillPreviewPane(this.ShellListView);
				});
			}
			Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => {
				//if (e.OldFolder != this.ShellListView.CurrentFolder) {
				var selectedItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
				selectedItem.Header = this.ShellListView.CurrentFolder.DisplayName;
				selectedItem.Icon = this.ShellListView.CurrentFolder.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
				selectedItem.ShellObject = this.ShellListView.CurrentFolder;
				if (selectedItem != null) {
					var selectedPaths = selectedItem.SelectedItems;
					if (selectedPaths != null && selectedPaths.Count > 0) {
						foreach (var path in selectedPaths.ToArray()) {
							var sho = this.ShellListView.Items.Where(w => w.ParsingName == path).SingleOrDefault();
							if (sho != null) {
								var index = this.ShellListView.ItemsHashed[sho.GetUniqueID()];
								this.ShellListView.SelectItemByIndex(index, true);
								selectedPaths.Remove(path);
							}
						}
					}
					else {
						this.ShellListView.ScrollToTop();
					}
				}
				//}
			}));
			////This initially setup the statusbar after program opens
			Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
				SetUpStatusBarOnSelectOrNavigate(ShellListView.SelectedItems == null ? 0 : ShellListView.GetSelectedCount());
			}));

			this.ShellListView.Focus();
			//this._IsTabSelectionChangedAllowed = true;
		}

		private void SetupUIonNavComplete(NavigatedEventArgs e) {
			btnSizeChart.IsEnabled = e.Folder.IsFileSystem;
			btnAutosizeColls.IsEnabled = ShellListView.View == ShellViewStyle.Details;

			if (e.Folder.ParsingName == KnownFolders.RecycleBin.ParsingName) {
				if (ShellListView.Items.Any()) miRestoreALLRB.Visibility = Visibility.Visible;
			}
			else {
				miRestoreALLRB.Visibility = Visibility.Collapsed;
			}

			bool isFuncAvail;
			int selectedItemsCount = ShellListView.GetSelectedCount();
			if (selectedItemsCount == 1) {
				isFuncAvail = ShellListView.GetFirstSelectedItem().IsFileSystem || ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName;
			}
			else {
				isFuncAvail = true;
				if (!(ShellListView.CurrentFolder.IsFolder && !ShellListView.CurrentFolder.IsDrive && !ShellListView.CurrentFolder.IsSearchFolder))
					ctgFolderTools.Visibility = Visibility.Collapsed;
			}

			bool IsChanged = selectedItemsCount > 0;
			btnCopy.IsEnabled = IsChanged;
			btnCut.IsEnabled = IsChanged;
			btnRename.IsEnabled = IsChanged;
			btnDelete.IsEnabled = IsChanged && isFuncAvail;
			btnCopyto.IsEnabled = IsChanged;
			btnMoveto.IsEnabled = IsChanged;
			btnSelNone.IsEnabled = IsChanged;

			leftNavBut.IsEnabled = (tcMain.SelectedItem as Wpf.Controls.TabItem).log.CanNavigateBackwards;
			rightNavBut.IsEnabled = (tcMain.SelectedItem as Wpf.Controls.TabItem).log.CanNavigateForwards;
			btnUpLevel.IsEnabled = ShellListView.CanNavigateParent;
		}

		private void SetUpJumpListOnNavComplete() {
			var pIDL = IntPtr.Zero;

			try {
				pIDL = this.ShellListView.CurrentFolder.AbsolutePidl;
				var sfi = new SHFILEINFO();

				//TODO: Why do we even have this if(...)
				if (pIDL != IntPtr.Zero && !ShellListView.CurrentFolder.IsFileSystem) {
					//if (!ShellListView.CurrentFolder.IsFileSystem) {
					IntPtr Res = BExplorer.Shell.Interop.Shell32.SHGetFileInfo(pIDL, 0, out sfi, Marshal.SizeOf(sfi), SHGFI.IconLocation | SHGFI.SmallIcon | SHGFI.PIDL);
				}

				if (ShellListView.CurrentFolder.IsFileSystem) {
					BExplorer.Shell.Interop.Shell32.SHGetFileInfo(Marshal.StringToHGlobalAuto(ShellListView.CurrentFolder.ParsingName), 0, out sfi, Marshal.SizeOf(sfi), SHGFI.IconLocation | SHGFI.SmallIcon);
				}

				WIN.Shell.JumpList.AddToRecentCategory(new JumpTask() {
					ApplicationPath = Process.GetCurrentProcess().MainModule.FileName,
					Arguments = String.Format("\"{0}\"", ShellListView.CurrentFolder.ParsingName),
					Title = ShellListView.CurrentFolder.DisplayName,
					IconResourcePath = sfi.szDisplayName,
					IconResourceIndex = sfi.iIcon
				});

				AppJL.Apply();
			}
			finally {
				if (pIDL != IntPtr.Zero) Marshal.FreeCoTaskMem(pIDL);
			}
		}

		private bool SetUpNewFolderButtons() {
			if (ShellListView.CurrentFolder.Parent == null) {
				return false;
			}
			else if (ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName) {
				btnCreateFolder.Header = FindResource("btnNewLibraryCP");  //"New Library";
				stNewFolder.Title = FindResource("btnNewLibraryCP").ToString();//"New Library";
				stNewFolder.Text = "Creates a new library in the current folder.";
				stNewFolder.Image = new BitmapImage(new Uri(@"/BetterExplorer;component/Images/newlib32.png", UriKind.Relative));
				btnCreateFolder.LargeIcon = @"..\Images\newlib32.png";
				btnCreateFolder.Icon = @"..\Images\newlib16.png";

				return true;
			}
			else if (this.ShellListView.CurrentFolder.IsFileSystem || this.ShellListView.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
				btnCreateFolder.Header = FindResource("btnNewFolderCP");//"New Folder";
				stNewFolder.Title = FindResource("btnNewFolderCP").ToString(); //"New Folder";
				stNewFolder.Text = "Creates a new folder in the current folder";
				stNewFolder.Image = new BitmapImage(new Uri(@"/BetterExplorer;component/Images/folder_new32.png", UriKind.Relative));
				btnCreateFolder.LargeIcon = @"..\Images\folder_new32.png";
				btnCreateFolder.Icon = @"..\Images\folder_new16.png";

				return false;
			}
			else {
				return false;
			}
		}

		private void SetUpButtonVisibilityOnNavComplete(bool isinLibraries) {
			var selectedCount = this.ShellListView.GetSelectedCount();
			if (ShellListView.CurrentFolder.ParsingName.Contains(KnownFolders.Libraries.ParsingName) && ShellListView.CurrentFolder.ParsingName != KnownFolders.Libraries.ParsingName) {
				if (selectedCount == 1) {
					ctgLibraries.Visibility = Visibility.Visible;
					SetupLibrariesTab(ShellLibrary.Load(Path.GetFileNameWithoutExtension(ShellListView.CurrentFolder.ParsingName), false));
				}
				ctgFolderTools.Visibility = Visibility.Collapsed;
				ctgImage.Visibility = Visibility.Collapsed;
				ctgArchive.Visibility = Visibility.Collapsed;
				ctgVirtualDisk.Visibility = Visibility.Collapsed;
				ctgExe.Visibility = Visibility.Collapsed;
			}
			else if (!ShellListView.CurrentFolder.ParsingName.ToLowerInvariant().EndsWith("library-ms")) {
				btnDefSave.Items.Clear();
				ctgLibraries.Visibility = Visibility.Collapsed;
			}

			ctgDrive.Visibility = ShellListView.CurrentFolder.IsDrive ? Visibility.Visible : Visibility.Collapsed;

			if (isinLibraries) {
				ctgFolderTools.Visibility = Visibility.Collapsed;
			}
		}

		private void NavigationController(IListItemEx Destination) {
			if (!Destination.Equals(this.ShellListView.CurrentFolder)) {
				this.ShellListView.Navigate_Full(Destination, true);

				//if (this.ShellListView.CurrentFolder != null)

			}
			//this.bcbc.Path = RemoveLastEmptySeparator(Destination.ParsingName).ToShellParsingName(); //this.ShellListView.CurrentFolder.ParsingName;
		}

		private string RemoveLastEmptySeparator(string path) {
			path = path.Trim();
			int sepLength = 1;
			if (path.EndsWith(@"\")) {
				path = path.Remove(path.Length - sepLength, sepLength);
			}
			return path;
		}
		#endregion

		#region Misc

		public MainWindow() {
			//TaskBar.SetCurrentProcessAppId("{A8795DFC-A37C-41E1-BC3D-6BBF118E64AD}");

			//TODO: Test
			BExplorer.Shell.Interop.Shell32.SetCurrentProcessExplicitAppUserModelID("{A8795DFC-A37C-41E1-BC3D-6BBF118E64AD}");

			this.CommandBindings.AddRange(new[]
			{ 
				new CommandBinding(AppCommands.RoutedNavigateBack, leftNavBut_Click),
				new CommandBinding(AppCommands.RoutedNavigateUp, btnUpLevel_Click),
				new CommandBinding(AppCommands.RoutedGotoSearch, GoToSearchBox),
				new CommandBinding(AppCommands.RoutedNewTab, (sender, e) => tcMain.NewTab()),
				new CommandBinding(AppCommands.RoutedEnterInBreadCrumbCombo, (sender, e) => { this.ShellListView.IsFocusAllowed = false; this.bcbc.SetInputState(); }),
				new CommandBinding(AppCommands.RoutedChangeTab, (sender, e) => {
					//t.Stop();
					int selIndex = tcMain.SelectedIndex == tcMain.Items.Count - 1 ? 0 : tcMain.SelectedIndex + 1;
					tcMain.SelectedItem = tcMain.Items[selIndex];
				}),
				new CommandBinding(AppCommands.RoutedCloseTab, (sender, e) => {
					if (tcMain.SelectedIndex == 0 && tcMain.Items.Count == 1) {
						Close();
						return;
					}
					int CurSelIndex = tcMain.SelectedIndex;
					tcMain.SelectedItem = tcMain.SelectedIndex == 0 ? tcMain.Items[1] : tcMain.Items[CurSelIndex - 1];
					tcMain.Items.RemoveAt(CurSelIndex);
				})
			});

			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);

			// loads current Ribbon color theme
			try {
				var Color = Convert.ToString(rks.GetValue("CurrentTheme", "Blue"));
				switch (Color) {
					case "Blue":
					case "Silver":
					case "Black":
					case "Green":
						ChangeRibbonTheme(Color);
						break;
					case "Metro":
						//ChangeRibbonTheme(Color, true);
						//Do nothing since Metro should be already loaded in App.Startup
						break;
					default:
						ChangeRibbonTheme("Blue");
						break;
				}
			}
			catch (Exception ex) {
				MessageBox.Show(String.Format("An error occurred while trying to load the theme data from the Registry. \n\r \n\r{0}\n\r \n\rPlease let us know of this issue at http://bugtracker.better-explorer.com/", ex.Message), "RibbonTheme Error - " + ex.ToString());
			}

			// loads current UI language (uses en-US if default)
			try {
				//load current UI language in case there is no specified registry value
				var loc = Convert.ToString(rks.GetValue("Locale", ":null:")) == ":null:" ? Thread.CurrentThread.CurrentUICulture.Name : Convert.ToString(rks.GetValue("Locale", ":null:"));
				((App)Application.Current).SelectCulture(loc, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\translation.xaml");
			}
			catch (Exception ex) {
				MessageBox.Show(String.Format("An error occurred while trying to load the locale data from the Registry. \n\r \n\r{0}\n\r \n\rPlease let us know of this issue at http://bugtracker.better-explorer.com/", ex.Message), "Locale Load Error - " + ex);
			}

			// gets values from registry to be applied after initialization
			string lohc = Convert.ToString(rks.GetValue("Locale", ":null:"));
			double sbw = Convert.ToDouble(rks.GetValue("SearchBarWidth", "220"));
			string rtlused = Convert.ToString(rks.GetValue("RTLMode", "notset"));
			string tabba = Convert.ToString(rks.GetValue("TabBarAlignment", "top"));

			rks.Close();
			rk.Close();


			//Main Initialization routine
			InitializeComponent();

			// sets up ComboBox to select the current UI language
			foreach (TranslationComboBoxItem item in this.TranslationComboBox.Items) {
				if (item.LocaleCode == lohc) {
					this.TranslationComboBox.SelectedItem = item;
				}
			}

			bool rtlset = rtlused != "notset";

			if (!rtlset) {
				rtlused = (this.TranslationComboBox.SelectedItem as TranslationComboBoxItem).UsesRTL ? "true" : "false";
			}

			this.SearchBarColumn.Width = new GridLength(sbw);

			// prepares RTL mode
			FlowDirection = rtlused == "true" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

			if (rtlset) {
				//TODO: Find out if we can merge this with [if (!rtlset)]
				rtlused = "notset";
			}

			// sets tab bar alignment
			if (tabba == "top")
				TabbaTop.IsChecked = true;
			else if (tabba == "bottom")
				TabbaBottom.IsChecked = true;
			else
				TabbaTop.IsChecked = true;

			// allows user to change language
			//ReadyToChangeLanguage = true;
		}

		private void beNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {
			this.Visibility = Visibility.Visible;
			if (this.WindowState == WindowState.Minimized) {
				User32.ShowWindow(Handle, User32.ShowWindowCommands.Restore);
			}

			this.Activate();
			this.Topmost = true;  // important
			this.Topmost = false; // important
			this.Focus();         // important
		}

		void ShellListView_ViewStyleChanged(object sender, BExplorer.Shell.ViewChangedEventArgs e) {
			zoomSlider.Value = e.ThumbnailSize;
			btnAutosizeColls.IsEnabled = e.CurrentView == ShellViewStyle.Details;
			btnSbTiles.IsChecked = e.CurrentView == ShellViewStyle.Tile;

			if (e.CurrentView == ShellViewStyle.ExtraLargeIcon) {
				ViewGallery.SelectedIndex = 0;
			}
			else if (e.CurrentView == ShellViewStyle.LargeIcon) {
				ViewGallery.SelectedIndex = 1;
			}
			else if (e.CurrentView == ShellViewStyle.Medium) {
				ViewGallery.SelectedIndex = 2;
				btnSbIcons.IsChecked = true;
			}
			else if (e.CurrentView == ShellViewStyle.SmallIcon) {
				ViewGallery.SelectedIndex = 3;
			}
			else {
				btnSbIcons.IsChecked = false;
			}

			if (e.CurrentView == ShellViewStyle.List) {
				ViewGallery.SelectedIndex = 4;
			}
			else if (e.CurrentView == ShellViewStyle.Details) {
				ViewGallery.SelectedIndex = 5;
				btnSbDetails.IsChecked = true;
			}
			else {
				btnSbDetails.IsChecked = false;
			}

			if (e.CurrentView == ShellViewStyle.Tile) {
				ViewGallery.SelectedIndex = 6;
			}
			else if (e.CurrentView == ShellViewStyle.Content) {
				ViewGallery.SelectedIndex = 7;
			}
			else if (e.CurrentView == ShellViewStyle.Thumbstrip) {
				ViewGallery.SelectedIndex = 8;
			}
		}

		private void btnNewItem_DropDownOpened(object sender, EventArgs e) {
			var mnu = new ShellContextMenu(this.ShellListView, 0);

			var controlPos = btnNewItem.TransformToAncestor(Application.Current.MainWindow).Transform(new WIN.Point(0, 0));
			var tempPoint = PointToScreen(new WIN.Point(controlPos.X, controlPos.Y));
			mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)btnNewItem.ActualHeight));
			btnNewItem.IsDropDownOpen = false;
			//mnu.GeneratetestMenu(this.ShellListView.GetFirstSelectedItem());
		}

		private void mnuPinToStart_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetSelectedCount() == 1) {
				string loc = KnownFolders.StartMenu.ParsingName + @"\" + ShellListView.GetFirstSelectedItem().DisplayName + ".lnk";
				var link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.GetFirstSelectedItem().ParsingName;
				link.Save(loc);
				link.Dispose();

				User32.PinUnpinToStartMenu(loc);
			}

			if (ShellListView.GetSelectedCount() == 0) {
				string loc = KnownFolders.StartMenu.ParsingName + @"\" + ShellListView.CurrentFolder.DisplayName + ".lnk";
				ShellLink link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.CurrentFolder.ParsingName;
				link.Save(loc);
				link.Dispose();

				User32.PinUnpinToStartMenu(loc);
			}
		}

		private void tmpButtonB_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show("This button currently does nothing");
		}

		private void RibbonWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
			if (!Keyboard.IsKeyDown(Key.LeftAlt) && (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)) {
				e.Handled = true;
				var keyText = String.Empty;
				switch (e.Key) {
					case Key.Left:
						keyText = "{LEFT}";
						break;
					case Key.Right:
						keyText = "{RIGHT}";
						break;
					case Key.Up:
						keyText = "{UP}";
						break;
					case Key.Down:
						keyText = "{DOWN}";
						break;
				}

				WIN.Forms.SendKeys.SendWait(keyText);
			}
		}

		private void ToolBar_SizeChanged(object sender, SizeChangedEventArgs e) {
			var toolBar = sender as ToolBar;
			var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
			if (overflowGrid != null) {
				overflowGrid.Visibility = toolBar.HasOverflowItems ? Visibility.Visible : Visibility.Collapsed;
			}

			var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
			if (mainPanelBorder != null) {
				var defaultMargin = new Thickness(0, 0, 11, 0);
				mainPanelBorder.Margin = toolBar.HasOverflowItems ? defaultMargin : new Thickness(0);
			}
		}

		private void btnPaypal_Click(object sender, RoutedEventArgs e) {
			string url = "";

			string business = "dimitarcenevjp@gmail.com";				// your PayPal email
			string description = "Donation%20for%20Better%20Explorer";  // '%20' represents a space. remember HTML!
			string country = "US";										// AU, US, etc.
			string currency = "USD";									// AUD, USD, etc.

			url += "https://www.paypal.com/cgi-bin/webscr" +
			  "?cmd=" + "_donations" +
			  "&business=" + business +
			  "&lc=" + country +
			  "&item_name=" + description +
			  "&currency_code=" + currency +
			  "&bn=" + "PP%2dDonationsBF";

			System.Diagnostics.Process.Start(url);
		}

		private void btnTest_Click(object sender, RoutedEventArgs e) {
			/*
			 We could easily move this to another project and send that method			 
			*/


			//Following could be an example of what the most basic plugin could look like
			//We could also separate plugins so they could be enabled WHEN
			//Always OR Folder_Selected OR File_Selected 
			Action<string, string> Plugin_Example_Activate_Basic = (string Plugin_Path, string Current_FileOrFolder) => {
				Process.Start(Plugin_Path, Current_FileOrFolder);
			};


			var Tab = new Fluent.RibbonTabItem();
			TheRibbon.Tabs.Add(Tab);
			Tab.Header = "Plugins";
			Tab.ToolTip = "Plugins";
			var GroupBox1 = new RibbonGroupBox();
			Tab.Groups.Add(GroupBox1);
			GroupBox1.Header = "Test";

			var XML =
			  @"<Shortcuts>
						<Shortcut Name='Test' Path = 'C:\Aaron'/>
					</Shortcuts>";


			var XDoc = XElement.Parse(XML);
			var Shortcuts = XDoc.Elements("Shortcut");

			var DropDown = new SplitButton();
			GroupBox1.Items.Add(DropDown);

			foreach (var Node in XDoc.Elements("Shortcut")) {
				var Item = new MenuItem() { Header = Node.Attribute("Name").Value };
				Item.Click += (x, y) => {
					Process.Start(Node.Attribute("Path").Value);
				};

				DropDown.Items.Add(Item);
			}
		}

		private void tcMain_Setup(object sender, RoutedEventArgs e) {
			tcMain.newt_DragEnter = newt_DragEnter;
			tcMain.newt_DragOver = newt_DragOver;
			tcMain.newt_Drop = newt_Drop;
			tcMain.newt_Leave = newt_Leave;
			tcMain.newt_GiveFeedback = newt_GiveFeedback;
			tcMain.newt_PreviewMouseMove = newt_PreviewMouseMove;
			tcMain.newt_PreviewMouseDown = newt_PreviewMouseDown;
			tcMain.ConstructMoveToCopyToMenu += ConstructMoveToCopyToMenu;
			tcMain.DefaultTabPath = tcMain.StartUpLocation.ToShellParsingName();
			tcMain.PreviewSelectionChanged += tcMain_PreviewSelectionChanged;
			tcMain.StartUpLocation = Utilities.GetRegistryValue("StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();
			if (tcMain.StartUpLocation == "") {
				Utilities.SetRegistryValue("StartUpLoc", KnownFolders.Libraries.ParsingName);
				tcMain.StartUpLocation = KnownFolders.Libraries.ParsingName;
			}
		}

		void tcMain_PreviewSelectionChanged(object p_oSender, Wpf.Controls.PreviewSelectionChangedEventArgs e) {
			if (tcMain.IsInTabDragDrop) {
				e.Cancel = true;
				return;
			}
			//tcMain.IsInTabDragDrop = true;
			if (e.RemovedItems.Count > 0) {

				var tab = e.RemovedItems[0] as Wpf.Controls.TabItem;

				if (tab != null && this.ShellListView.GetSelectedCount() > 0) {
					tab.SelectedItems = this.ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
				}
			}

			if (e.AddedItems.Count == 0 || tcMain.SelectNewTabOnCreate == false) return;
			var newTab = e.AddedItems[0] as Wpf.Controls.TabItem;
			if (this.ShellListView.CurrentFolder == null || !this.ShellListView.CurrentFolder.Equals(newTab.ShellObject) && tcMain.CurrentTabItem == null) {
				SelectTab(newTab);
			}
			else {
				if (!tcMain.IsSelectionHandled) {
					SelectTab(newTab);

					//btnUndoClose
					btnUndoClose.Items.Clear();
					foreach (var item in tcMain.ReopenableTabs) {
						btnUndoClose.Items.Add(item.CurrentLocation);
					}
				}
				else {
					if (e.RemovedItems.Count == 0) {
						e.Cancel = true;
						SelectTab(newTab);
						tcMain.SelectedItem = e.AddedItems[0];
					}
					else {
						if (e.RemovedItems[0] == tcMain.CurrentTabItem) {
							e.Cancel = true;
							tcMain.IsSelectionHandled = false;
							tcMain.SelectedItem = e.RemovedItems[0];
							tcMain.CurrentTabItem = null;
						}
					}
				}
			}
			tcMain.IsSelectionHandled = false;
			this.ShellListView.Focus();
			this._CurrentlySelectedItem = tcMain.SelectedItem as Wpf.Controls.TabItem;
		}



		private void newt_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
			e.Handled = true;
			e.UseDefaultCursors = true;
			if (this._TabDropData == null)
				return;
			var doo = new WIN.Forms.DataObject(this._TabDropData);
			if (doo.GetDataPresent("DragWindow")) {
				IntPtr hwnd = ShellView.GetIntPtrFromData(doo.GetData("DragWindow"));
				User32.PostMessage(hwnd, 0x403, IntPtr.Zero, IntPtr.Zero);
			}
			else {
				e.UseDefaultCursors = true;
			}

			if (ShellView.IsDropDescriptionValid(this._TabDropData)) {
				e.UseDefaultCursors = false;
				Cursor = Cursors.Arrow;
			}
			else {
				e.UseDefaultCursors = true;
			}

			if (ShellView.IsShowingLayered(doo)) {
				e.UseDefaultCursors = false;
				Cursor = Cursors.Arrow;
			}
			else {
				e.UseDefaultCursors = true;
			}
		}

		private void RibbonWindow_Activated(object sender, EventArgs e) {
			//tcMain.CurrentTabItem = tcMain.SelectedItem as Wpf.Controls.TabItem;
			tcMain.IsSelectionHandled = true;
			focusTimer.Start();
		}

		private void RibbonWindow_StateChanged(object sender, EventArgs e) {
			tcMain.CurrentTabItem = tcMain.SelectedItem as Wpf.Controls.TabItem;
			tcMain.IsSelectionHandled = true;
			if (this.WindowState != WIN.WindowState.Minimized && this.IsActive) {
				focusTimer.Start();
			}
		}

		void focusTimer_Tick(object sender, EventArgs e) {
			this.ShellListView.Focus();
			focusTimer.Stop();
		}

		public Dictionary<string, IRibbonControl> GetAllButtonsAsDictionary() {
			var rb = new Dictionary<string, Fluent.IRibbonControl>();

			foreach (Fluent.RibbonTabItem item in TheRibbon.Tabs) {
				foreach (RibbonGroupBox itg in item.Groups) {
					foreach (var ic in itg.Items.OfType<IRibbonControl>()) {
						rb.Add((ic as FrameworkElement).Name, ic);
					}
				}
			}

			return rb;
		}

		private void btnRefresh_Click(object sender, RoutedEventArgs e) {
			var da = new DoubleAnimation(100, new Duration(new TimeSpan(0, 0, 0, 1, 100))) { FillBehavior = FillBehavior.Stop };
			this.bcbc.BeginAnimation(Odyssey.Controls.BreadcrumbBar.ProgressValueProperty, da);

			ShellListView.RefreshContents();
			SetSortingAndGroupingButtons();
			SetupUIOnSelectOrNavigate();
		}

		void bcbc_OnEditModeToggle(object sender, Odyssey.Controls.EditModeToggleEventArgs e) {
			this.ShellListView.IsFocusAllowed = e.IsExit;
			if (!e.IsExit) {
				this.bcbc.Focus();
			}
		}

		private void bcbc_BreadcrumbItemDropDownOpened(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e) {
			this.ShellListView.IsFocusAllowed = false;
			this.bcbc.Focus();
		}

		private void bcbc_BreadcrumbItemDropDownClosed(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e) {
			this.ShellListView.IsFocusAllowed = true;
		}

		#endregion

		private void btnOpenWith_DropDownOpened(object sender, EventArgs e) {
			var mnu = new ShellContextMenu(this.ShellListView, 1);

			var controlPos = btnOpenWith.TransformToAncestor(Application.Current.MainWindow).Transform(new WIN.Point(0, 0));
			var tempPoint = PointToScreen(new WIN.Point(controlPos.X, controlPos.Y));
			mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)btnOpenWith.ActualHeight), 1);
			btnOpenWith.IsDropDownOpen = false;

		}

		private void btnSort_DropDownOpened(object sender, EventArgs e) {
			var button = sender as DropDownButton;
			foreach (MenuItem item in button.Items) {
				var column = item.Tag as Collumns;
				if (column == this.ShellListView.Collumns.Where(w => w.ID == this.ShellListView.LastSortedColumnId).SingleOrDefault()) {
					item.IsChecked = true;
					break;
				}
			}
			button.Items.OfType<MenuItem>().Last().IsChecked = this.ShellListView.LastSortOrder == WIN.Forms.SortOrder.Descending;
			button.Items.OfType<MenuItem>().ToArray()[button.Items.OfType<MenuItem>().Count() - 2].IsChecked = this.ShellListView.LastSortOrder == WIN.Forms.SortOrder.Ascending;
		}

		private void btnGroup_DropDownOpened(object sender, EventArgs e) {
			var button = sender as DropDownButton;
			foreach (MenuItem item in button.Items) {
				var column = item.Tag as Collumns;
				if (column == this.ShellListView.LastGroupCollumn) {
					item.IsChecked = true;
					break;
				}
			}
			button.Items.OfType<MenuItem>().Last().IsChecked = this.ShellListView.LastGroupOrder == WIN.Forms.SortOrder.Descending;
			button.Items.OfType<MenuItem>().ToArray()[button.Items.OfType<MenuItem>().Count() - 2].IsChecked = this.ShellListView.LastGroupOrder == WIN.Forms.SortOrder.Ascending;
		}

		private void ShellTreeHost_SizeChanged(object sender, SizeChangedEventArgs e) {
			this.ShellTree.Refresh();
			this.ShellListView.Refresh();
			this.ShellListView.Left = 0;
			this.ShellListView.Top = 0;
			this.ShellListView.Width = (int)this.ShellViewHost.ActualWidth;
			this.ShellListView.Height = (int)this.ShellViewHost.ActualHeight;
		}

	}
}