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
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell.FileOperations;
using Microsoft.WindowsAPICodePack.Taskbar;
using NAppUpdate.Framework;
using SevenZip;
using Shell32;
using WindowsHelper;
using wyDay.Controls;
using Clipboards = System.Windows.Forms.Clipboard;
using ContextMenu = Fluent.ContextMenu;
using MenuItem = Fluent.MenuItem;
using Odyssey.Controls;


namespace BetterExplorer {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Fluent.RibbonWindow {

		[Obsolete("Can remove move this!!! Someone should!!!")]
		public bool IsCalledFromLoading;

		[Obsolete("Find a way to remove this!")]
		public bool isOnLoad;


		[Obsolete("Does nothing")]
		private bool IsCloseLastTabCloseApp;

		[Obsolete("Does nothing")]
		private void chkIsLastTabCloseApp_Click(object sender, RoutedEventArgs e) {
			var b = this.chkIsLastTabCloseApp.IsChecked;
			if (b != null)
				this.IsCloseLastTabCloseApp = b.Value;
		}


		[Obsolete("Try to remove. Items are added and removed but this has no functionality")]
		NetworkAccountManager nam = new NetworkAccountManager();



		#region DLLImports

		#region Cursor

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		[StructLayout(LayoutKind.Sequential)]
		internal struct Win32Point {
			public Int32 X;
			public Int32 Y;
		};

		#endregion

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int UnregisterHotKey(IntPtr hwnd, int id);

		[DllImport("winmm.dll")]
		static extern Int32 mciSendString(String command, StringBuilder buffer, Int32 bufferSize, IntPtr hwndCallback);

		#endregion

		#region Variables and Constants

		#region Public member
		public bool IsrestoreTabs;
		public static UpdateManager Updater;
		#endregion

		#region Private Members
		private bool asFolder = false, asImage = false, asArchive = false, asDrive = false, asApplication = false, asLibrary = false, asVirtualDrive = false;
		private MenuItem misa, misd, misag, misdg, misng;
		private bool IsInfoPaneEnabled, IsNavigationPaneEnabled, IsConsoleShown, IsPreviewPaneEnabled;
		private int PreviewPaneWidth = 120, InfoPaneHeight = 150;
		private ShellTreeViewEx ShellTree = new ShellTreeViewEx();
		private ShellView ShellListView = new ShellView();



		private bool IsUpdateCheck;
		private bool IsUpdateCheckStartup;
		private ClipboardMonitor cbm = new ClipboardMonitor();
		private ContextMenu cmHistory = new ContextMenu();
		private System.Windows.Shell.JumpList AppJL = new System.Windows.Shell.JumpList();
		private IntPtr Handle;

		private List<string> Archives = new List<string>(new[] { ".rar", ".zip", ".7z", ".tar", ".gz", ".xz", ".bz2" });
		private List<string> Images = new List<string>(new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".wmf" });
		private List<string> VirDisks = new List<string>(new[] { ".iso", ".bin", ".vhd" });
		private string SelectedArchive = "";
		private bool KeepBackstageOpen = false;
		private string ICON_DLLPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
		bool canlogactions = false;
		string sessionid = DateTime.UtcNow.ToFileTimeUtc().ToString();
		string logdir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\ActionLog\\";
		string satdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\";
		string naddir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\NetworkAccounts\\";
		string sstdir;
		bool OverwriteOnRotate = false;
		NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		System.Windows.Forms.Timer updateCheckTimer = new System.Windows.Forms.Timer();
		DateTime LastUpdateCheck;
		Int32 UpdateCheckInterval;
		Double CommandPromptWinHeight;
		Boolean IsGlassOnRibonMinimized { get; set; }

		List<BExplorer.Shell.LVItemColor> LVItemsColor { get; set; }
		ContextMenu chcm;
		MessageReceiver r;
		#endregion

		#endregion

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
			fmAbout fAbout = new fmAbout(this);
			fAbout.ShowDialog();
		}

		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
			ScrollViewer scviewer = (sender as ScrollViewer);
			scviewer.ScrollToHorizontalOffset(scviewer.HorizontalOffset - e.Delta);
		}

		private void btnBugtracker_Click(object sender, RoutedEventArgs e) {
			Process.Start("http://bugtracker.better-explorer.com");
		}

		private void backstage_IsOpenChanged(object sender, DependencyPropertyChangedEventArgs e) {
			autoUpdater.Visibility = Visibility.Visible;
			autoUpdater.UpdateLayout();

			if (KeepBackstageOpen) {
				backstage.IsOpen = true;
				KeepBackstageOpen = false;
			}
		}

		private void RibbonWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
			IsRenameFromCreate = true;
		}

		private void TheRibbon_SizeChanged(object sender, SizeChangedEventArgs e) {
			//TODO:	[Date: 5/6/2014]	Test this code change
			if (TheRibbon.IsMinimized && this.IsGlassOnRibonMinimized) {
				System.Windows.Point p = ShellViewHost.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
				this.GlassBorderThickness = new Thickness(8, this.WindowState == WindowState.Maximized ? p.Y : p.Y - 2, 8, 8);
			}
			else if (this.IsGlassOnRibonMinimized) {
				System.Windows.Point p = backstage.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
				this.GlassBorderThickness = new Thickness(8, p.Y + backstage.ActualHeight + 2, 8, 8);
			}

			try {
				this.SetBlur(!TheRibbon.IsMinimized);
			}
			catch (Exception) {
			}
		}

		private void TheRibbon_CustomizeQuickAccessToolbar(object sender, EventArgs e) {
			CustomizeQAT.Open(this, TheRibbon);
		}

		private void LoadInitialWindowPositionAndState() {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", false);

			if (rks == null) return;


			this.Width = Convert.ToDouble(rks.GetValue(@"LastWindowWidth", "640"));
			this.Height = Convert.ToDouble(rks.GetValue(@"LastWindowHeight", "480"));

			System.Drawing.Point Location = new System.Drawing.Point();
			try {
				Location = new System.Drawing.Point(
					Convert.ToInt32(rks.GetValue(@"LastWindowPosLeft", "0")),
					Convert.ToInt32(rks.GetValue(@"LastWindowPosTop", "0"))
				);
			}
			catch { }
			if (Location != null) {
				this.Left = Location.X;
				this.Top = Location.Y;
			}

			switch (Convert.ToInt32(rks.GetValue(@"LastWindowState"))) {
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

			int isGlassOnRibonMinimized = (int)rks.GetValue(@"RibbonMinimizedGlass", 1);
			this.IsGlassOnRibonMinimized = isGlassOnRibonMinimized == 1;
			chkRibbonMinimizedGlass.IsChecked = this.IsGlassOnRibonMinimized;

			TheRibbon.IsMinimized = Convert.ToBoolean(rks.GetValue(@"IsRibonMinimized", false));

			//CommandPrompt window size
			this.CommandPromptWinHeight = Convert.ToDouble(rks.GetValue(@"CmdWinHeight", 100));
			rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);

			if ((int)rks.GetValue(@"IsConsoleShown", 0) == 1) {
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
			//FIXME: fix sorting and grouping
			btnSort.Items.Clear();
			btnGroup.Items.Clear();

			// SORTCOLUMN sc = new SORTCOLUMN();

			//ShellListView.GetSortColInfo(out sc);
			// PROPERTYKEY pkg = new PROPERTYKEY();
			//ShellListView.GetGroupColInfo(out pkg, out GroupDir);

			//bool GroupDir = false;

			try {
				foreach (Collumns item in ShellListView.Collumns.Where(x => x != null)) {
					var IsChecked1 = (item.pkey.fmtid == ShellListView.Collumns[ShellListView.LastSortedColumnIndex].pkey.fmtid) && (item.pkey.pid == ShellListView.Collumns[ShellListView.LastSortedColumnIndex].pkey.pid);
					btnSort.Items.Add(Utilities.Build_MenuItem(item.Name, item, checkable: true, isChecked: IsChecked1, GroupName: "GR2", onClick: mi_Click));
					var IsCheckable2 = ShellListView.LastGroupCollumn != null && (item.pkey.fmtid == ShellListView.LastGroupCollumn.pkey.fmtid) && (item.pkey.pid == ShellListView.LastGroupCollumn.pkey.pid);
					btnGroup.Items.Add(Utilities.Build_MenuItem(item.Name, item, checkable: true, isChecked: IsCheckable2, GroupName: "GR3", onClick: mig_Click));
				}
			}
			catch (Exception ex) {
				//FIXME: I disable this message because of strange null after filter
				MessageBox.Show("BetterExplorer had an issue loading the visible columns for the current view. You might not be able to sort or group items.", ex.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
			}

			Separator sp = new Separator();
			sp.Focusable = false;
			btnSort.Items.Add(sp);


			misa = Utilities.Build_MenuItem(FindResource("miAscending"), checkable: true, GroupName: "GR1", onClick: misa_Click);
			misd = Utilities.Build_MenuItem(FindResource("miDescending"), checkable: true, GroupName: "GR1", onClick: misd_Click);

			if (this.ShellListView.LastSortOrder == System.Windows.Forms.SortOrder.Ascending) {
				misa.IsChecked = true;
			}
			else {
				misd.IsChecked = true;
			}

			btnSort.Items.Add(misa);
			btnSort.Items.Add(misd);

			misng = Utilities.Build_MenuItem("(none)", GroupName: "GR3", checkable: true, isChecked: ShellListView.LastGroupCollumn == null, onClick: misng_Click);

			btnGroup.Items.Add(misng);
			Separator spg = new Separator();
			btnGroup.Items.Add(spg);


			misag = Utilities.Build_MenuItem(FindResource("miAscending"), checkable: true, GroupName: "GR4", onClick: misag_Click);
			misdg = Utilities.Build_MenuItem(FindResource("miDescending"), checkable: true, GroupName: "GR4", onClick: misag_Click);

			if (this.ShellListView.LastGroupOrder == System.Windows.Forms.SortOrder.Ascending) {
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

		private void SetupColumnsButton(List<Collumns> allAvailColls) {
			btnMoreColls.Items.Clear();
			chcm.Items.Clear();

			for (int j = 1; j < 10; j++) {
				try {
					MenuItem mic = Utilities.Build_MenuItem(allAvailColls[j].Name, allAvailColls[j], checkable: true, onClick: mic_Click);
					//MenuItem mic = new MenuItem();
					//mic.Header = AllAvailColls[j].Name;
					//mic.Tag = AllAvailColls[j];
					//mic.Click += mic_Click;
					//mic.Focusable = false;
					//mic.IsCheckable = true;
					foreach (Collumns col in ShellListView.Collumns) {
						if (col.pkey.fmtid == allAvailColls[j].pkey.fmtid && col.pkey.pid == allAvailColls[j].pkey.pid) {
							mic.IsChecked = true;
						}
					}
					btnMoreColls.Items.Add(mic);
				}
				catch (Exception) {
				}

				try {
					MenuItem mic = Utilities.Build_MenuItem(allAvailColls[j].Name, allAvailColls[j], checkable: true, onClick: mic_Click);

					//MenuItem mic = new MenuItem();
					//mic.Header = AllAvailColls[j].Name;
					//mic.Tag = AllAvailColls[j];
					//mic.Click += mic_Click;
					//mic.Focusable = false;
					//mic.IsCheckable = true;
					foreach (Collumns col in ShellListView.Collumns) {
						if (col.pkey.fmtid == allAvailColls[j].pkey.fmtid && col.pkey.pid == allAvailColls[j].pkey.pid) {
							mic.IsChecked = true;
						}
					}
					chcm.Items.Add(mic);
				}
				catch (Exception) {
				}
			}

			int ItemsCount = ShellListView.GetItemsCount();
			sbiItemsCount.Visibility = ItemsCount == 0 ? Visibility.Collapsed : Visibility.Visible;
			sbiItemsCount.Content = ItemsCount == 1 ? "1 item" : ItemsCount + " items";
			sbiSelItemsCount.Visibility = ShellListView.GetSelectedCount() == 0 ? Visibility.Collapsed : Visibility.Visible;
			spSelItems.Visibility = sbiSelItemsCount.Visibility;

			btnMoreColls.Items.Add(new Separator());

			btnMoreColls.Items.Add(Utilities.Build_MenuItem(FindResource("btnMoreColCP"), allAvailColls, onClick: micm_Click));

			//MenuItem micm = new MenuItem();
			//micm.Header = FindResource("btnMoreColCP");
			//micm.Focusable = false;
			//btnMoreColls.Tag = AllAvailColls;
			//micm.Tag = AllAvailColls;
			//micm.Click += new RoutedEventHandler(micm_Click);
			//btnMoreColls.Items.Add(micm);

			chcm.Items.Add(new Separator());

			chcm.Items.Add(Utilities.Build_MenuItem(FindResource("btnMoreColCP"), allAvailColls, onClick: micm_Click));

			//MenuItem michm = new MenuItem();
			//michm.Header = FindResource("btnMoreColCP");
			//michm.Focusable = false;
			//michm.Tag = AllAvailColls;
			//michm.Click += new RoutedEventHandler(micm_Click);
			//chcm.Items.Add(michm);

			btnMoreColls.Tag = allAvailColls;
		}
		#endregion

		void misd_Click(object sender, RoutedEventArgs e) {
			//TODO: Test
			foreach (var item in btnSort.Items.OfType<MenuItem>().Where(item => item.IsChecked && item != (sender as MenuItem))) {
				ShellListView.SetSortCollumn(ShellListView.Collumns.IndexOf((Collumns)item.Tag), System.Windows.Forms.SortOrder.Descending);
			}
			/*
			foreach (object item in btnSort.Items) {
				if (item is MenuItem) {
					if (((MenuItem)item).IsChecked && ((MenuItem)item != (sender as MenuItem))) {
						ShellListView.SetSortCollumn(ShellListView.Collumns.IndexOf((Collumns)((MenuItem)item).Tag), System.Windows.Forms.SortOrder.Descending);
					}
				}
			}
			*/
		}

		void misa_Click(object sender, RoutedEventArgs e) {
			//TODO: Test
			foreach (var item in btnSort.Items.OfType<MenuItem>().Where(item => item.IsChecked && item != (sender as MenuItem))) {
				ShellListView.SetSortCollumn(ShellListView.Collumns.IndexOf((Collumns)item.Tag), System.Windows.Forms.SortOrder.Ascending);
			}

			/*
			foreach (object item in btnSort.Items) {
				if (item is MenuItem) {
					if (((MenuItem)item).IsChecked && ((MenuItem)item != (sender as MenuItem))) {
						ShellListView.SetSortCollumn(ShellListView.Collumns.IndexOf((Collumns)((MenuItem)item).Tag), System.Windows.Forms.SortOrder.Ascending);
					}
				}
			}
			*/
		}

		void timerv_Tick(object sender, EventArgs e) {
			var da = new DoubleAnimation(CurrentProgressValue, CurrentProgressValue + 1, new Duration(new TimeSpan(0, 0, 2)));
			da.FillBehavior = FillBehavior.Stop;
			CurrentProgressValue = CurrentProgressValue + 1;
			(sender as DispatcherTimer).Stop();
		}

		void micm_Click(object sender, RoutedEventArgs e) {
			MoreColumns fMoreCollumns = new MoreColumns();
			fMoreCollumns.PopulateAvailableColumns((List<Collumns>)(sender as FrameworkElement).Tag, ShellListView, this.PointToScreen(Mouse.GetPosition(this)));
		}

		void mic_Click(object sender, RoutedEventArgs e) {
			MenuItem mi = (sender as MenuItem);
			Collumns col = (Collumns)mi.Tag;
			ShellListView.SetColInView(col, !mi.IsChecked);
		}

		void miItem_Click(object sender, RoutedEventArgs e) {
			MenuItem mi = sender as MenuItem;
			ShellItem SaveLoc = mi.Tag as ShellItem;

			if (ShellListView.CurrentFolder.ParsingName.Contains(KnownFolders.Libraries.ParsingName) && ShellListView.CurrentFolder.ParsingName.EndsWith("library-ms")) {
				ShellLibrary lib = ShellLibrary.Load(ShellListView.CurrentFolder.DisplayName, false);
				lib.DefaultSaveFolder = SaveLoc.ParsingName;
				lib.Close();
			}
			else if (ShellListView.GetFirstSelectedItem().ParsingName.Contains(KnownFolders.Libraries.ParsingName)) {
				ShellLibrary lib = ShellLibrary.Load(ShellListView.GetFirstSelectedItem().DisplayName, false);
				lib.DefaultSaveFolder = SaveLoc.ParsingName;
				lib.Close();
			}
		}


		void fsw_Renamed(object sender, RenamedEventArgs e) {
			Dispatcher.BeginInvoke(DispatcherPriority.Normal,
				(Action)(() => {
					foreach (MenuItem item in btnFavorites.Items) {
						if ((item.Tag as ShellItem).ParsingName == e.OldFullPath)
							item.Header = Path.GetFileNameWithoutExtension(e.Name);
					}
				}));
		}

		void fsw_Deleted(object sender, FileSystemEventArgs e) {
			Dispatcher.BeginInvoke(DispatcherPriority.Normal,
				(Action)(() => {
					MenuItem ItemForRemove = null;
					foreach (MenuItem item in btnFavorites.Items) {
						if (item.Header.ToString() == Path.GetFileNameWithoutExtension(e.Name))
							ItemForRemove = item;
					}

					btnFavorites.Items.Remove(ItemForRemove);
				}));
		}

		void fsw_Created(object sender, FileSystemEventArgs e) {
			Dispatcher.BeginInvoke(DispatcherPriority.Normal,
				(Action)(() => {
					if (Path.GetExtension(e.FullPath).ToLowerInvariant() == ".lnk") {
						ShellItem so = new ShellItem(e.FullPath);
						so.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
						so.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
						ImageSource icon = so.Thumbnail.BitmapSource;


						btnFavorites.Items.Add(Utilities.Build_MenuItem(so.GetDisplayName(BExplorer.Shell.Interop.SIGDN.NORMALDISPLAY), so, icon, onClick: mif_Click));

						//MenuItem mi = new MenuItem();
						//mi.Header = so.GetDisplayName(BExplorer.Shell.Interop.SIGDN.NORMALDISPLAY);
						//mi.Tag = so;
						//mi.Icon = icon;
						//mi.Click += new RoutedEventHandler(mif_Click);
						//btnFavorites.Items.Add(mi);
					}
				}));
		}

		//bool IsSelectionRized = false;
		//BackgroundWorker bwSelectionChanged = new BackgroundWorker();
		//'Selection change (when an item is selected in a folder)

		/*
		private Boolean SetupEditButton(string item) {			
			RegistryKey rg = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + Path.GetExtension(item) + @"\OpenWithProgids");
			if (rg == null) return false;

			string filetype = rg.GetValueNames()[0];
			rg.Close();

			using (var rgtype = Registry.ClassesRoot.OpenSubKey(filetype + @"\shell\edit\command")) {
				return !(rgtype == null);	
			}
			//TODO: Remove the following Comment!
			//else {
			//	//RegistryKey rgtypeopen = Registry.ClassesRoot.OpenSubKey(filetype + @"\shell\open\command");
			//	//if (rgtypeopen != null)
			//	//{
			//	//  string editcommand = (string)rgtypeopen.GetValue("");

			//	//  isEditAvailable = true;
			//	//  EditComm = editcommand.Replace("\"", "");
			//	//  rgtypeopen.Close();
			//	//}
			//	//else
			//	//{
			//	//  isEditAvailable = false;
			//	//}
			//	isEditAvailable = false;
			//}


			//try
			//{
			//  Shell32.Shell shell = new Shell();
			//  Shell32.Folder folder = null;
			//  folder = shell.NameSpace(Path.GetDirectoryName(item));

			//  var itemInternal = folder.Items().OfType<Shell32.FolderItem>().ToArray()[0];
			//  if (itemInternal != null)
			//  {
			//    isEditAvailable = itemInternal.Verbs().OfType<FolderItemVerb>().Select(s => s.Name).Contains("&Edit");
			//  }
			//  item = null;
			//  folder = null;
			//  shell = null;
			//}
			//catch (Exception)
			//{
			//  isEditAvailable = false;
			//}

			//return isEditAvailable;
		} //TODO: Inline
		*/


		private void SetUpOpenWithButton(ShellItem SelectedItem) {
			btnOpenWith.Items.Clear();
			foreach (var item in SelectedItem.GetAssocList()) {
				btnOpenWith.Items.Add(Utilities.Build_MenuItem(item.DisplayName, item, item.Icon, ToolTip: item.ApplicationPath, onClick: miow_Click));
			}

			btnOpenWith.IsEnabled = btnOpenWith.HasItems;
		}

		private void SetUpRibbonTabsVisibilityOnSelectOrNavigate(int selectedItemsCount, ShellItem selectedItem) {
			#region Search Contextual Tab
			ctgSearch.Visibility = BooleanToVisibiliy(ShellListView.CurrentFolder.IsSearchFolder);
			if (ctgSearch.Visibility == Visibility.Visible && !ShellListView.CurrentFolder.IsSearchFolder) {
				ctgSearch.Visibility = Visibility.Collapsed;
				TheRibbon.SelectedTabItem = HomeTab;
			}
			#endregion

			#region Folder Tools Context Tab
			ctgFolderTools.Visibility = BooleanToVisibiliy((selectedItemsCount == 1 && selectedItem.IsFolder && selectedItem.IsFileSystem && !selectedItem.IsDrive && !selectedItem.IsNetDrive));

			if (asFolder && ctgFolderTools.Visibility == Visibility.Visible) {
				TheRibbon.SelectedTabItem = ctgFolderTools.Items[0];
			}
			#endregion

			#region Drive Contextual Tab
			ctgDrive.Visibility = BooleanToVisibiliy(ShellListView.CurrentFolder.IsDrive || (selectedItemsCount == 1 && selectedItem != null && (selectedItem.IsDrive || (selectedItem.Parent != null && selectedItem.Parent.IsDrive))));
			if (asDrive && ctgDrive.Visibility == Visibility.Visible && (selectedItem != null && selectedItem.IsDrive)) {
				TheRibbon.SelectedTabItem = ctgDrive.Items[0];
			}
			#endregion

			#region Library Context Tab
			var h = ShellListView.CurrentFolder.Equals(KnownFolders.Libraries);
			ctgLibraries.Visibility = BooleanToVisibiliy(ShellListView.CurrentFolder.Equals(KnownFolders.Libraries) || (selectedItemsCount == 1 && selectedItem.Parent != null && selectedItem.Parent.Equals(KnownFolders.Libraries)));

			if (ctgLibraries.Visibility == Visibility.Visible && asLibrary) {
				TheRibbon.SelectedTabItem = ctgLibraries.Items[0];
			}

			if (ctgLibraries.Visibility == Visibility.Visible && ShellListView.CurrentFolder.Equals(KnownFolders.Libraries)) {
				if (selectedItem != null && selectedItemsCount == 1)
					SetupLibrariesTab(ShellLibrary.Load(selectedItem.DisplayName, false));
			}
			else if (ctgLibraries.Visibility == Visibility.Visible && ShellListView.CurrentFolder.Parent.Equals(KnownFolders.Libraries)) {
				if (selectedItemsCount == 1)
					SetupLibrariesTab(ShellLibrary.Load(ShellListView.CurrentFolder.DisplayName, false));
			}
			#endregion

			#region Archive Contextual Tab
			ctgArchive.Visibility = BooleanToVisibiliy(selectedItemsCount == 1 && Archives.Contains(Path.GetExtension(selectedItem.ParsingName).ToLowerInvariant()));
			if (asArchive && ctgArchive.Visibility == Visibility.Visible) {
				TheRibbon.SelectedTabItem = ctgArchive.Items[0];
			}
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
				Bitmap cvt = new Bitmap(selectedItem.ParsingName);

				imgSizeDisplay.WidthData = cvt.Width.ToString();
				imgSizeDisplay.HeightData = cvt.Height.ToString();

				if (asImage) TheRibbon.SelectedTabItem = ctgImage.Items[0];

				cvt.Dispose();
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

		private void SetUpButtonsStateOnSelectOrNavigate(int selectedItemsCount, ShellItem selectedItem) {
			btnCopy.IsEnabled = selectedItemsCount > 0;
			btnCopyto.IsEnabled = selectedItemsCount > 0;
			btnMoveto.IsEnabled = selectedItemsCount > 0;
			btnCut.IsEnabled = selectedItemsCount > 0;
			btnDelete.IsEnabled = selectedItem != null && selectedItem.IsFileSystem;
			btnRename.IsEnabled = selectedItem != null && (selectedItem.IsFileSystem || (selectedItem.Parent != null && selectedItem.Parent.Equals(KnownFolders.Libraries)));
			btnProperties3.IsEnabled = selectedItemsCount > 0;
			if (selectedItem != null) {
				RegistryKey rg = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + Path.GetExtension(selectedItem.ParsingName) + @"\OpenWithProgids");
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

			btnSelAll.IsEnabled = selectedItemsCount != ShellListView.GetItemsCount();
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
				item.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

				btnDefSave.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.Thumbnail.BitmapSource, GroupName: "GRDS1", checkable: true,
																isChecked: item.ParsingName == lib.DefaultSaveFolder, onClick: miItem_Click));


				//MenuItem miItem = new MenuItem();
				//miItem.Header = item.GetDisplayName(SIGDN.NORMALDISPLAY);
				//miItem.Tag = item;
				//miItem.Icon = item.Thumbnail.BitmapSource;
				//miItem.GroupName = "GRDS1";
				//miItem.IsCheckable = true;
				//miItem.IsChecked = item.ParsingName == lib.DefaultSaveFolder;
				//miItem.Click += new RoutedEventHandler(miItem_Click);
				//btnDefSave.Items.Add(miItem);
			}

			btnDefSave.IsEnabled = !(lib.Count == 0);
			lib.Close();
		}

		private void SetupUIOnSelectOrNavigate(bool isNavigate = false) {
			//private void SetupUIOnSelectOrNavigate(int SelItemsCount, bool isNavigate = false) {
			var SelItemsCount = ShellListView.GetSelectedCount();

			btnDefSave.Items.Clear();
			var selectedItem = this.ShellListView.GetFirstSelectedItem();
			if (selectedItem != null && !isNavigate) {
				SetUpOpenWithButton(selectedItem);
			}

			if (selectedItem != null && selectedItem.IsFileSystem && IsPreviewPaneEnabled && !selectedItem.IsFolder && SelItemsCount == 1) {
				this.Previewer.FileName = selectedItem.ParsingName;
			}
			else if (!String.IsNullOrEmpty(this.Previewer.FileName)) {
				this.Previewer.FileName = null;
			}
			//Set up ribbon contextual tabs on selection changed
			SetUpRibbonTabsVisibilityOnSelectOrNavigate(SelItemsCount, selectedItem);
			SetUpButtonsStateOnSelectOrNavigate(SelItemsCount, selectedItem);
		}

		[Obsolete("try to remove this if possible")]
		bool IsFromSelectionOrNavigation = false;
		// background worker code removed. hopefully we don't need it still... Lol.

		void cbm_ClipboardChanged(object sender, Tuple<System.Windows.Forms.IDataObject> e) {
			btnPaste.IsEnabled = e.Item1.GetDataPresent(DataFormats.FileDrop) || e.Item1.GetDataPresent("Shell IDList Array");
			btnPasetShC.IsEnabled = e.Item1.GetDataPresent(DataFormats.FileDrop) || e.Item1.GetDataPresent("Shell IDList Array");
		}

		[Obsolete("How do I activate this event and why would I want to do it!!!")]
		private void TheRibbon_IsCollapsedChanged(object sender, DependencyPropertyChangedEventArgs e) {
			this.edtSearchBox.Visibility = this.BooleanToVisibiliy(!(bool)e.NewValue);
		}

		#endregion

		#region Conditional Select

		private void miSelAllByType_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetSelectedCount() > 0) {
				var flt = new List<string>();
				PROPERTYKEY typePK = new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 4 };

				foreach (ShellItem item in ShellListView.SelectedItems) {
					flt.Add(item.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant());
				}

				var items = ShellListView.Items.Where(w => flt.Contains(w.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant())).ToArray();

				ShellListView.SelectItems(items);
				items = null;
				btnCondSel.IsDropDownOpen = false;
			}

		}

		private void miSelAllByDate_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetSelectedCount() > 0) {
				var flt = new List<DateTime>();
				PROPERTYKEY typePK = new PROPERTYKEY() { fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 15 };

				foreach (ShellItem item in ShellListView.SelectedItems) {
					flt.Add(DateTime.Parse(item.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant()).Date);
				}
				ShellListView.SelectItems(
					ShellListView.Items.Where(w => flt.Contains(DateTime.Parse(w.GetPropertyValue(typePK, typeof(String)).Value.ToString().ToLowerInvariant()).Date)).ToArray()
				);

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
			//TODO:	[Date: 5/6/2014]	Try to combine the 2 if {...} into an If & Else
			if (ShellListView.GetSelectedCount() > 0) {
				if ((ShellListView.GetFirstSelectedItem().IsFolder || ShellListView.GetFirstSelectedItem().IsDrive) && ShellListView.GetFirstSelectedItem().IsFileSystem) {
					FolderSizeWindow.Open(ShellListView.GetFirstSelectedItem().ParsingName, this);
					return;
				}
			}

			if ((ShellListView.CurrentFolder.IsFolder || ShellListView.CurrentFolder.IsDrive) && ShellListView.CurrentFolder.IsFileSystem) {
				FolderSizeWindow.Open(ShellListView.CurrentFolder.ParsingName, this);
			}
		}

		private void btnSizeChart_Click(object sender, RoutedEventArgs e) {
			FolderSizeWindow.Open(ShellListView.CurrentFolder.ParsingName, this);
		}

		#endregion

		#region Home Tab

		private void miJunctionpoint_Click(object sender, RoutedEventArgs e) {
			StringCollection DropList = System.Windows.Forms.Clipboard.GetFileDropList();
			string PathForDrop = ShellListView.CurrentFolder.ParsingName.Replace(@"\\", @"\");
			foreach (string item in DropList) {
				ShellItem o = new ShellItem(item);
				JunctionPointUtils.JunctionPoint.Create(String.Format(@"{0}\{1}", PathForDrop, o.GetDisplayName(SIGDN.NORMALDISPLAY)), o.ParsingName, true);
				AddToLog(String.Format(@"Created Junction Point at {0}\{1} linked to {2}", PathForDrop, o.GetDisplayName(SIGDN.NORMALDISPLAY), o.ParsingName));
			}
		}

		private void miCreateSymlink_Click(object sender, RoutedEventArgs e) {
			StringCollection DropList = System.Windows.Forms.Clipboard.GetFileDropList();
			string PathForDrop = ShellListView.CurrentFolder.ParsingName.Replace(@"\\", @"\");
			string ExePath = Utilities.AppDirectoryItem("BetterExplorerOperations.exe");


			int winhandle = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");

			List<ShellItem> items = new List<ShellItem>();
			foreach (string item in DropList) {
				ShellItem o = new ShellItem(item);
				items.Add(o);
				AddToLog(String.Format(@"Created Symbolic Link at {0}\\{1} linked to {2}", PathForDrop, o.GetDisplayName(SIGDN.NORMALDISPLAY), o.ParsingName));
			}

			string sources = PathStringCombiner.CombinePaths(items, ";", true);
			string drops = PathStringCombiner.CombinePathsWithSinglePath(PathForDrop + @"\", items, false);


			foreach (var item in items) {
				string source = item.ParsingName.Replace(@"\\", @"\");
				string drop = String.Format(@"{0}\{1}", PathForDrop, item.GetDisplayName(SIGDN.NORMALDISPLAY));
			}

			//for (int val = 0; val < items.Count; val++) {
			//	string source = items[val].ParsingName.Replace(@"\\", @"\");
			//	string drop = String.Format(@"{0}\{1}", PathForDrop, items[val].GetDisplayName(SIGDN.NORMALDISPLAY));
			//}

			using (Process proc = new Process()) {
				proc.StartInfo = new ProcessStartInfo {
					FileName = ExePath,
					Verb = "runas",
					UseShellExecute = true,
					Arguments = String.Format("/env /user:Administrator \"{0}\"", ExePath)
				};

				proc.Start();
				Thread.Sleep(1000);
				int res = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x88779", sources, drops, "", "", "");
				proc.WaitForExit();
				if (proc.ExitCode == 1)
					MessageBox.Show("Error in creating symlink", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void btnHistory_Click(object sender, RoutedEventArgs e) {
			ShellListView.ShowPropPage(this.Handle, ShellListView.GetFirstSelectedItem().ParsingName,
				WindowsAPI.LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "twext.dll"), 1024, "Previous Versions"));
		}

		private void btnBackstageExit_Click(object sender, RoutedEventArgs e) {
			//! We call Shutdown() so to explicit shutdown the app regardless of windows closing cancel flag.
			if (r != null)
				r.Close();

			Application.Current.Shutdown();
		}

		private void btnNewWindow_Click(object sender, RoutedEventArgs e) {
			// creates a new window
			var k = System.Reflection.Assembly.GetExecutingAssembly().Location;
			Process.Start(k, "/nw");
		}


		void miow_Click(object sender, RoutedEventArgs e) {
			MenuItem item = (sender as MenuItem);
			var assocItem = (AssociationItem)item.Tag;
			assocItem.Invoke();
		}

		void mif_Click(object sender, RoutedEventArgs e) {
			MenuItem item = (sender as MenuItem);

			var obj = (item.Tag as ShellItem);
			ShellLink lnk = new ShellLink(obj.ParsingName);

			var obj2 = new ShellItem(lnk.TargetPIDL);
			ShellListView.Navigate(obj2);

			lnk.Dispose();
			obj.Dispose();
		}

		private void btnCopy_Click(object sender, RoutedEventArgs e) {
			StringCollection sc = new StringCollection();
			sc.AddRange(ShellListView.SelectedItems.Select(x => x.ParsingName).ToArray());

			//foreach (ShellItem item in ShellListView.SelectedItems) {
			//	sc.Add(item.ParsingName);
			//}

			System.Windows.Forms.Clipboard.SetFileDropList(sc);
		}

		private void btnPaste_Click(object sender, RoutedEventArgs e) {
			ShellListView.PasteAvailableFiles();
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e) {
			MenuItem_Click(sender, e);
		}

		private void btnRename_Click(object sender, RoutedEventArgs e) {
			IsRenameFromCreate = false;
			ShellListView.RenameSelectedItem();
		}

		private void btnPathCopy_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.SelectedItems.Count() > 1) {
				string path = null;
				foreach (ShellItem item in ShellListView.SelectedItems) {
					if (string.IsNullOrEmpty(path)) {
						path = item.ParsingName;
					}
					else {
						path = String.Format("{0}\r\n{1}", path, item.ParsingName);
					}
				}

				Clipboards.SetText(path);
			}
			else if (ShellListView.SelectedItems.Count() == 1) {
				Clipboards.SetText(ShellListView.GetFirstSelectedItem().ParsingName);
			}
			else {
				Clipboards.SetText(ShellListView.CurrentFolder.ParsingName);
			}
		}


		private void btnSelAll_Click(object sender, RoutedEventArgs e) {
			ShellListView.SelectAll();
		}

		private void btnSelNone_Click(object sender, RoutedEventArgs e) {
			ShellListView.DeSelectAllItems();
		}

		// Delete > Send to Recycle Bin
		private void MenuItem_Click(object sender, RoutedEventArgs e) {
			string path = null;
			foreach (ShellItem item in ShellListView.SelectedItems) {
				if (string.IsNullOrEmpty(path)) {
					path = item.ParsingName;
				}
				else {
					path = String.Format("{0} {1}", path, item.ParsingName);
				}
			}

			AddToLog(String.Format("The following files have been moved to the Recycle Bin: {0}", path));
			ShellListView.DeleteSelectedFiles(true);
		}

		// Delete > Permanently Delete
		private void MenuItem_Click_1(object sender, RoutedEventArgs e) {
			ShellListView.DeleteSelectedFiles(false);
		}

		public bool IsRenameFromCreate = false;

		// New Folder/Library
		private void Button_Click_2(object sender, RoutedEventArgs e) {
			//We should focus the ListView or on some circumstances new folder does not start renaming after folder is created
			this.ShellListView.Focus();
			string path = "";

			//bool IsLib = false;
			IsRenameFromCreate = true;
			if (ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName) {
				path = ShellListView.CreateNewLibrary(FindResource("btnNewLibraryCP").ToString()).DisplayName;
				//IsLib = true;
			}
			else {
				path = ShellListView.CreateNewFolder(FindResource("btnNewFolderCP").ToString());
			}

			//WindowsAPI.SHChangeNotify(WindowsAPI.HChangeNotifyEventID.SHCNE_MKDIR,
			//WindowsAPI.HChangeNotifyFlags.SHCNF_PATHW | WindowsAPI.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, Marshal.StringToHGlobalAuto(path.Replace(@"\\", @"\")), IntPtr.Zero);

			//IsLibW = IsLib;
			//IsAfterFolderCreate = true;
			//this.ShellListView.Focus();
		}


		private void btnProperties_Click(object sender, RoutedEventArgs e) {
			/*
			if (ShellListView.SelectedItems.Count() > 0) {
				ShellListView.ShowFileProperties();
			}
			else {
				ShellListView.ShowFileProperties(ShellListView.CurrentFolder.ParsingName);
			}
			*/
			//We must have a file selected!
			ShellListView.ShowFileProperties(ShellListView.CurrentFolder.ParsingName);
			ShellListView.Focus();
		}

		private void btnInvSel_Click(object sender, RoutedEventArgs e) {
			ShellListView.InvertSelection();
		}

		private void btnPasetShC_Click(object sender, RoutedEventArgs e) {
			StringCollection DropList = System.Windows.Forms.Clipboard.GetFileDropList();
			string PathForDrop = ShellListView.CurrentFolder.ParsingName;
			foreach (string item in DropList) {
				using (ShellLink shortcut = new ShellLink()) {
					ShellItem o = new ShellItem(item);
					shortcut.Target = item;
					shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(item);
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
			CommonOpenFileDialog dlg = new CommonOpenFileDialog();
			dlg.IsFolderPicker = true;
			if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
				SetFOperation(dlg.FileName, BExplorer.Shell.OperationType.Move);
			}
		}

		private void SetFOperation(String fileName, BExplorer.Shell.OperationType opType) {
			var obj = new ShellItem(fileName);
			if (opType == BExplorer.Shell.OperationType.Copy)
				ShellListView.DoCopy(obj);
			else if (opType == BExplorer.Shell.OperationType.Move)
				ShellListView.DoMove(obj);
		}

		private void SetFOperation(ShellItem obj, BExplorer.Shell.OperationType opType) {
			if (opType == BExplorer.Shell.OperationType.Copy)
				ShellListView.DoCopy(obj);
			else if (opType == BExplorer.Shell.OperationType.Move)
				ShellListView.DoMove(obj);
		}

		private void btnctOther_Click(object sender, RoutedEventArgs e) {
			var dlg = new CommonOpenFileDialog();
			dlg.IsFolderPicker = true;
			if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
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
			//AddToLog("The following files have been cut: " + PathStringCombiner.CombinePaths(ShellListView.SelectedItems.ToList(), " ", false));
			ShellListView.CutSelectedFiles();
		}

		private void btnNewItem_Click(object sender, RoutedEventArgs e) {
			var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE() { fShowAllObjects = 0 };
			WindowsAPI.SHGetSetSettings(ref state, WindowsAPI.SSF.SSF_SHOWALLOBJECTS, true);
		}

		private void btnOpenWith_Click(object sender, RoutedEventArgs e) {
			Process.Start(String.Format("\"{0}\"", ShellListView.GetFirstSelectedItem().ParsingName));
		}

		private void btnEdit_Click(object sender, RoutedEventArgs e) {
			//TODO: Code this!!
			System.Windows.Forms.MessageBox.Show("This button currently does nothing");
			//ShellListView.EditFile( ShellListView.GetFirstSelectedItem().ParsingName);
		}

		private void btnFavorites_Click(object sender, RoutedEventArgs e) {
			var selectedItems = ShellListView.SelectedItems;
			if (selectedItems.Count() == 1) {
				ShellLink link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.GetFirstSelectedItem().ParsingName;
				link.Save(String.Format(@"{0}\{1}.lnk", KnownFolders.Links.ParsingName, ShellListView.GetFirstSelectedItem().GetDisplayName(BExplorer.Shell.Interop.SIGDN.NORMALDISPLAY)));
				link.Dispose();
			}

			if (selectedItems.Count() == 0) {
				ShellLink link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.CurrentFolder.ParsingName;
				link.Save(String.Format(@"{0}\{1}.lnk", KnownFolders.Links.ParsingName, ShellListView.CurrentFolder.GetDisplayName(BExplorer.Shell.Interop.SIGDN.NORMALDISPLAY)));
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
				VolumeDeviceClass vdc = new VolumeDeviceClass();
				foreach (Volume item in vdc.Devices) {
					if (GetDriveLetterFromDrivePath(item.LogicalDrive) == DriveLetter) {
						var veto = item.Eject(false);
						if (veto != BetterExplorer.UsbEject.Native.PNP_VETO_TYPE.TypeUnknown) {
							if (veto == BetterExplorer.UsbEject.Native.PNP_VETO_TYPE.Ok) {
								Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
												(Action)(() => {
													this.beNotifyIcon.ShowBalloonTip("Information", String.Format("It is safe to remove {0}", item.LogicalDrive), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
													var tabsForRemove = tcMain.Items.OfType<Wpf.Controls.TabItem>()
															.Where(w => {
																var root = Path.GetPathRoot(w.ShellObject.ParsingName);
																return root != null && (w.ShellObject.IsFileSystem &&
																																				root.ToLowerInvariant() ==
																																				String.Format("{0}:\\", DriveLetter).ToLowerInvariant());
															}).ToArray();
													foreach (Wpf.Controls.TabItem tab in tabsForRemove) {
														CloseTab(tab, false);
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
								Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
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
			if (ShellListView.GetFirstSelectedItem().GetDriveInfo() != null) {
				if (ShellListView.GetFirstSelectedItem().GetDriveInfo().DriveType == DriveType.Removable) {
					EjectDisk(GetDriveLetterFromDrivePath(ShellListView.GetFirstSelectedItem().ParsingName));
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

			MountIso mi = new MountIso();
			mi.Owner = this;
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
				//TODO: add the code for mounting images with ImDisk. look the example below!

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
			ctgDrive.Visibility = System.Windows.Visibility.Collapsed;
		}

		#endregion

		#region Application Tools

		private void btnRunAsAdmin_Click(object sender, RoutedEventArgs e) {
			ShellListView.RunExeAsAdmin(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		private void btnPin_Click(object sender, RoutedEventArgs e) {
			WindowsAPI.PinUnpinToTaskbar(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		private void btnPinToStart_Click(object sender, RoutedEventArgs e) {
			WindowsAPI.PinUnpinToStartMenu(ShellListView.GetFirstSelectedItem().ParsingName);
			//if (ShellListView.GetSelectedItemsCount() == 1)
			//{
			//    ShellLink link = new ShellLink();
			//    link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
			//    link.Target =  ShellListView.GetFirstSelectedItem().ParsingName;
			//    link.Save(KnownFolders.StartMenu.ParsingName + @"\" +
			//         ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY) + ".lnk");
			//    link.Dispose();
			//}
		}

		private void btnRunAs_Click(object sender, RoutedEventArgs e) {
			CredUI.RunProcesssAsUser(ShellListView.GetFirstSelectedItem().ParsingName);
		}

		#endregion

		#region Backstage - Information Tab

		private void Button_Click_6(object sender, RoutedEventArgs e) {
			backstage.IsOpen = true;
			autoUpdater.Visibility = System.Windows.Visibility.Visible;
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

		public void ExportColumnDataToTextFile(string filename) {
			var eb = new Microsoft.WindowsAPICodePack.Controls.WindowsForms.ExplorerBrowser();
			eb.InitBrowser();
			var cols = eb.AvailableColumnsList(true);

			int acount = 0;
			foreach (var item in cols) {
				using (StreamWriter sw = new StreamWriter(filename, true)) {
					// new Tuple<String, PROPERTYKEY, Type>("Date Modified", new PROPERTYKEY(){fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 14}, typeof(DateTime))
					sw.WriteLine("{\"A" + acount + "\", new Tuple<String, PROPERTYKEY, Type>(\"" + item.Name + "\", new PROPERTYKEY(){fmtid = Guid.Parse(\"" + item.pkey.fmtid + "\"), pid = " + item.pkey.pid + "}, typeof(String))},");
					acount++;
				}
			}
		}

		private Visibility BooleanToVisibiliy(bool value) {
			return value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
		}

		List<DependencyObject> hitTestList = null;
		HitTestResultBehavior CollectAllVisuals_Callback(HitTestResult result) {
			if (result == null || result.VisualHit == null)
				return HitTestResultBehavior.Stop;

			hitTestList.Add(result.VisualHit);
			return HitTestResultBehavior.Continue;
		}

		private void AddToLog(string value) {
			try {
				if (canlogactions) {
					if (!Directory.Exists(logdir)) Directory.CreateDirectory(logdir);

					using (StreamWriter sw = new StreamWriter(String.Format("{0}{1}.txt", logdir, sessionid), true)) {
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
			if (rbDaily.IsChecked.Value) {
				UpdateCheckInterval = 1;
			}
			else if (rbMonthly.IsChecked.Value) {
				UpdateCheckInterval = 30;
			}
			else {
				UpdateCheckInterval = 7;
			}

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
			//TODO: Fix these [try catch]s a try catch in a [foreach] NO!
			Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
				try {
					btnFavorites.Visibility = Visibility.Visible;
					foreach (ShellItem item in KnownFolders.Links.Where(w => !w.IsHidden)) {
						//TODO: Try to remove this try catch
						try {
							item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
							item.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
							btnFavorites.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.Thumbnail.BitmapSource, onClick: mif_Click));

							//MenuItem mi = new MenuItem();
							//mi.Header = item.GetDisplayName(SIGDN.NORMALDISPLAY);
							//mi.Tag = item;
							//mi.Focusable = false;
							//mi.Icon = item.Thumbnail.BitmapSource;
							//mi.Click += new RoutedEventHandler(mif_Click);
							//btnFavorites.Items.Add(mi);
						}
						catch (Exception) {
						}
					}
				}
				catch (Exception) {
					btnFavorites.Visibility = Visibility.Collapsed;
				}
			}));
		}

		private void InitializeExplorerControl() {
			this.ShellTree.NodeClick += ShellTree_NodeClick;
			this.ShellListView.Navigated += ShellListView_Navigated;
			this.ShellListView.ViewStyleChanged += ShellListView_ViewStyleChanged;
			this.ShellListView.SelectionChanged += ShellListView_SelectionChanged;
			this.ShellListView.LVItemsColorCodes = this.LVItemsColor;
			this.ShellListView.ItemUpdated += ShellListView_ItemUpdated;
			this.ShellListView.ColumnHeaderRightClick += ShellListView_ColumnHeaderRightClick;
			this.ShellListView.KeyJumpKeyDown += ShellListView_KeyJumpKeyDown;
			this.ShellListView.KeyJumpTimerDone += ShellListView_KeyJumpTimerDone;
			this.ShellListView.ItemDisplayed += ShellListView_ItemDisplayed;
			this.ShellListView.Navigating += ShellListView_Navigating;
			this.ShellListView.ItemMiddleClick += (sender, e) => tcMain.NewTab(e.Folder, false);
			this.ShellListView.BeginItemLabelEdit += ShellListView_BeginItemLabelEdit;
			this.ShellListView.EndItemLabelEdit += ShellListView_EndItemLabelEdit;
		}

		void ShellListView_EndItemLabelEdit(object sender, EventArgs e) {
			this.Editor.Visibility = System.Windows.Visibility.Collapsed;
			this.Editor.IsOpen = false;
		}

		void ShellListView_BeginItemLabelEdit(object sender, RenameEventArgs e) {
			var isSmall = this.ShellListView.IconSize == 16;
			var itemRect = this.ShellListView.GetItemBounds(e.ItemIndex, 0);
			var itemLabelRect = this.ShellListView.GetItemBounds(e.ItemIndex, 2);
			this.txtEditor.Text = this.ShellListView.Items[e.ItemIndex].DisplayName;
			var point = this.ShellViewHost.PointToScreen(new System.Windows.Point(isSmall ? itemLabelRect.Left : itemRect.Left, itemLabelRect.Top - (isSmall ? 1 : 0)));
			this.Editor.HorizontalOffset = point.X;
			this.Editor.VerticalOffset = point.Y;

			this.txtEditor.MaxWidth = isSmall ? Double.PositiveInfinity : itemRect.Width;
			this.txtEditor.MaxHeight = isSmall ? itemLabelRect.Height + 2 : Double.PositiveInfinity;

			this.Editor.Width = isSmall ? this.txtEditor.Width : itemRect.Width;
			this.Editor.Height = this.txtEditor.Height + 2;
			this.Editor.Visibility = System.Windows.Visibility.Visible;
			this.Editor.IsOpen = true;
			this.txtEditor.Focus();
			this.txtEditor.SelectAll();
		}


		void ShellListView_Navigating(object sender, NavigatingEventArgs e) {
			if (this.ShellListView.CurrentFolder == null) return;
			this._IsBreadcrumbBarSelectionChnagedAllowed = false;
			//if (!e.IsNavigateInSameTab || e.Folder == this.ShellListView.CurrentFolder)
			this.bcbc.PathConversion -= path_conversation;
			this.bcbc.Path = e.Folder.IsSearchFolder ? e.Folder.Pidl.ToString() : e.Folder.ParsingName;
			this.bcbc.PathConversion += path_conversation;
			var tab = tcMain.SelectedItem as Wpf.Controls.TabItem;
			if (tab != null && this.ShellListView.GetSelectedCount() > 0) {
				if (tab.SelectedItems != null) {
					tab.SelectedItems.AddRange(this.ShellListView.SelectedItems.Select(s => s.ParsingName).ToList());
				}
				else {
					tab.SelectedItems = this.ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
				}
			}
			this.Title = "Better Explorer - " + e.Folder.GetDisplayName(BExplorer.Shell.Interop.SIGDN.NORMALDISPLAY);
			e.Folder.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
			e.Folder.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			try {
				Wpf.Controls.TabItem selectedTabItem = (tcMain.SelectedItem as Wpf.Controls.TabItem);
				selectedTabItem.Header = e.Folder.DisplayName;
				selectedTabItem.Icon = e.Folder.Thumbnail.BitmapSource;
				selectedTabItem.ShellObject = e.Folder;
				selectedTabItem.ToolTip = e.Folder.ParsingName;
			}
			catch (Exception) {
			}
		}


		void ShellListView_ItemDisplayed(object sender, ItemDisplayedEventArgs e) {
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

		void ShellTree_NodeClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e) {
			if (e.Button == System.Windows.Forms.MouseButtons.Middle) {
				if (e.Node != null && e.Node.Tag != null)
					tcMain.NewTab(e.Node.Tag as ShellItem, false);
			}
		}


		System.Windows.Forms.Timer _keyjumpTimer = new System.Windows.Forms.Timer();

		void ShellListView_KeyJumpTimerDone(object sender, EventArgs e) {
			if (_keyjumpTimer != null) {
				_keyjumpTimer.Stop();
				_keyjumpTimer.Start();
			}
		}

		void ShellListView_KeyJumpKeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
			//add key for key jump
			KeyJumpGrid.IsOpen = true;
			txtKeyJump.Text = ShellListView.KeyJumpString;
		}

		void ShellListView_ColumnHeaderRightClick(object sender, System.Windows.Forms.MouseEventArgs e) {
			//is where the more columns menu should be added
			chcm.IsOpen = true;
		}

		void ShellListView_ItemUpdated(object sender, ItemUpdatedEventArgs e) {
			if (e.UpdateType != ItemUpdateType.Renamed && e.UpdateType != ItemUpdateType.Updated) {
				int ItemsCount = ShellListView.GetItemsCount();
				sbiItemsCount.Visibility = ItemsCount == 0 ? Visibility.Collapsed : Visibility.Visible;
				sbiItemsCount.Content = ItemsCount == 1 ? "1 item" : ItemsCount + " items";
			}
			if (e.UpdateType == ItemUpdateType.Created && (IsRenameFromCreate || this.ShellListView.IsRenameNeeded)) {
				this.ShellListView.SelectItemByIndex(e.NewItemIndex, true, true);
				ShellListView.RenameItem(e.NewItemIndex);
				IsRenameFromCreate = false;
				this.ShellListView.IsRenameNeeded = false;
			}
			this.ShellListView.Focus();
		}


		void ShellListView_SelectionChanged(object sender, EventArgs e) {
			Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
				SetupUIOnSelectOrNavigate();
			}));

			if (this.IsInfoPaneEnabled) {
				Task.Run(() => this.DetailsPanel.FillPreviewPane(this.ShellListView));
			}

			SetUpStatusBarOnSelectOrNavigate(ShellListView.GetSelectedCount());
		}

		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			Handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
		}

		private void LoadRegistryRelatedSettings() {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.CreateSubKey(@"Software\BExplorer");

			switch (Convert.ToString(rks.GetValue(@"CurrentTheme", "Blue"))) {
				case "Blue":
					btnBlue.IsChecked = true;
					break;
				case "Silver":
					btnSilver.IsChecked = true;
					break;
				case "Black":
					btnBlack.IsChecked = true;
					break;
				case "Green":
					btnGreen.IsChecked = true;
					break;
				default:
					btnBlue.IsChecked = true;
					break;
			}

			LastUpdateCheck = DateTime.FromBinary(Convert.ToInt64(rks.GetValue(@"LastUpdateCheck", 0)));

			UpdateCheckInterval = (int)rks.GetValue(@"CheckInterval", 7);

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

			UpdateCheckType = (int)rks.GetValue(@"UpdateCheckType", 1);

			switch (UpdateCheckType) {
				case 0:
					rbReleases.IsChecked = true;
					break;
				case 1:
					rbReleasTest.IsChecked = true;
					break;
			}


			int HFlyoutEnabled = (int)rks.GetValue(@"HFlyoutEnabled", 0);

			int UpdateCheck = (int)rks.GetValue(@"CheckForUpdates", 1);
			IsUpdateCheck = (UpdateCheck == 1);
			chkUpdateCheck.IsChecked = IsUpdateCheck;

			int UpdateCheckStartup = (int)rks.GetValue(@"CheckForUpdatesStartup", 1);
			IsUpdateCheckStartup = (UpdateCheckStartup == 1);
			chkUpdateStartupCheck.IsChecked = IsUpdateCheckStartup;

			int isConsoleShown = (int)rks.GetValue(@"IsConsoleShown", 0);
			IsConsoleShown = (isConsoleShown == 1);
			btnConsolePane.IsChecked = this.IsConsoleShown;

			//IsHFlyoutEnabled = (HFlyoutEnabled == 1);
			//chkIsFlyout.IsChecked = IsHFlyoutEnabled;
			chkIsFlyout.IsChecked = (HFlyoutEnabled == 1);

			int InfoPaneEnabled = (int)rks.GetValue(@"InfoPaneEnabled", 0);

			IsInfoPaneEnabled = (InfoPaneEnabled == 1);
			btnInfoPane.IsChecked = IsInfoPaneEnabled;

			InfoPaneHeight = (int)rks.GetValue(@"InfoPaneHeight", 150);

			if (IsInfoPaneEnabled) {
				rPreviewPane.Height = new GridLength(InfoPaneHeight);
				rPreviewPaneSplitter.Height = new GridLength(1);
			}
			else {
				rPreviewPane.Height = new GridLength(0);
				rPreviewPaneSplitter.Height = new GridLength(0);
			}

			int PreviewPaneEnabled = (int)rks.GetValue(@"PreviewPaneEnabled", 0);

			IsPreviewPaneEnabled = (PreviewPaneEnabled == 1);
			btnPreviewPane.IsChecked = IsPreviewPaneEnabled;

			PreviewPaneWidth = (int)rks.GetValue(@"PreviewPaneWidth", 120);

			if (IsPreviewPaneEnabled) {
				clPreview.Width = new GridLength((double)PreviewPaneWidth);
				clPreviewSplitter.Width = new GridLength(1);
			}
			else {
				clPreview.Width = new GridLength(0);
				clPreviewSplitter.Width = new GridLength(0);
			}

			int NavigationPaneEnabled = (int)rks.GetValue(@"NavigationPaneEnabled", 1);

			IsNavigationPaneEnabled = (NavigationPaneEnabled == 1);
			btnNavigationPane.IsChecked = IsNavigationPaneEnabled;

			//isCheckModeEnabled = ShellListView.ShowCheckboxes;
			//chkShowCheckBoxes.IsChecked = isCheckModeEnabled;

			chkShowCheckBoxes.IsChecked = ShellListView.ShowCheckboxes;

			int ExFileOpEnabled = (int)rks.GetValue(@"FileOpExEnabled", 0);
			//IsExtendedFileOpEnabled = (ExFileOpEnabled == 1);
			//ShellListView.IsExFileOpEnabled = IsExtendedFileOpEnabled;
			//chkIsTerraCopyEnabled.IsChecked = IsExtendedFileOpEnabled;
			chkIsTerraCopyEnabled.IsChecked = (ExFileOpEnabled == 1);

			int cfoEnabled = (int)rks.GetValue(@"IsCustomFO", 0);
			chkIsCFO.IsChecked = cfoEnabled == 1;

			/*
			int CompartibleRename = (int)rks.GetValue(@"CompartibleRename", 1);
			IsCompartibleRename = (CompartibleRename == 1);
			*/

			//chkIsCompartibleRename.IsChecked = IsCompartibleRename;

			int RestoreTabs = (int)rks.GetValue(@"IsRestoreTabs", 1);
			IsrestoreTabs = (RestoreTabs == 1);

			chkIsRestoreTabs.IsChecked = IsrestoreTabs;

			//if this instance has the /norestore switch, do not load tabs from previous session, even if it is set in the Registry
			if (App.isStartWithStartupTab) {
				IsrestoreTabs = false;
			}

			int IsLastTabCloseApp = (int)rks.GetValue(@"IsLastTabCloseApp", 1);
			//IsCloseLastTabCloseApp = (IsLastTabCloseApp == 1);
			//this.chkIsLastTabCloseApp.IsChecked = IsCloseLastTabCloseApp;
			this.chkIsLastTabCloseApp.IsChecked = (IsLastTabCloseApp == 1);


			int LogActions = (int)rks.GetValue(@"EnableActionLog", 0);

			canlogactions = (LogActions == 1);
			chkLogHistory.IsChecked = canlogactions;
			if (LogActions == 1) {
				chkLogHistory.Visibility = System.Windows.Visibility.Visible;
				ShowLogsBorder.Visibility = System.Windows.Visibility.Visible;
				paddinglbl8.Visibility = System.Windows.Visibility.Visible;
			}

			// load settings for auto-switch to contextual tab
			asFolder = ((int)rks.GetValue(@"AutoSwitchFolderTools", 0) == 1);
			asArchive = ((int)rks.GetValue(@"AutoSwitchArchiveTools", 1) == 1);
			asImage = ((int)rks.GetValue(@"AutoSwitchImageTools", 1) == 1);
			asApplication = ((int)rks.GetValue(@"AutoSwitchApplicationTools", 0) == 1);
			asLibrary = ((int)rks.GetValue(@"AutoSwitchLibraryTools", 1) == 1);
			asDrive = ((int)rks.GetValue(@"AutoSwitchDriveTools", 1) == 1);
			asVirtualDrive = ((int)rks.GetValue(@"AutoSwitchVirtualDriveTools", 0) == 1);


			chkFolder.IsChecked = asFolder;
			chkArchive.IsChecked = asArchive;
			chkImage.IsChecked = asImage;
			chkApp.IsChecked = asApplication;
			chkLibrary.IsChecked = asLibrary;
			chkDrive.IsChecked = asDrive;
			chkVirtualTools.IsChecked = asVirtualDrive;

			// load OverwriteOnImages setting (default is false)
			int oor = (int)rks.GetValue(@"OverwriteImageWhileEditing", 0);
			OverwriteOnRotate = (oor == 1);
			chkOverwriteImages.IsChecked = (oor == 1);

			// load Saved Tabs Directory location (if different from default)
			string tdir = Convert.ToString(rks.GetValue(@"SavedTabsDirectory", satdir));
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




			ShellItem sho = new ShellItem(StartUpLocation.ToShellParsingName());
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
				this.Visibility = System.Windows.Visibility.Hidden;
				this.WindowState = System.Windows.WindowState.Minimized;
				//this.ShowInTaskbar = false;
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
				System.Windows.Shell.JumpList.SetJumpList(Application.Current, AppJL);
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

			ShellTree.ShellListView = ShellListView;
			this.ctrlConsole.ShellListView = this.ShellListView;

			Task.Run(() => {
				UpdateRecycleBinInfos();
			});



			r = new MessageReceiver();
			r.OnMessageReceived += r_OnMessageReceived;
			r.Show();

			bool exitApp = false;

			try {
				Task.Run(() => {
					if (WindowsAPI.getOSInfo() == WindowsAPI.OsVersionInfo.Windows8) {
						var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
						WindowsAPI.SHGetSetSettings(ref state, WindowsAPI.SSF.SSF_SHOWSTATUSBAR, false);
						if (state.fShowStatusBar == 1) {
							var newState = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
							newState.fShowStatusBar = 0;
							WindowsAPI.SHGetSetSettings(ref newState, WindowsAPI.SSF.SSF_SHOWSTATUSBAR, true);
						}
					}
				});



				//Sets up FileSystemWatcher for Favorites folder
				try {
					//TODO: Find out why we gave this, it is NEVER USED. After this method I assume it will be disposed!!
					FileSystemWatcher fsw = new FileSystemWatcher(KnownFolders.Links.ParsingName);
					fsw.Created += fsw_Created;
					fsw.Deleted += fsw_Deleted;
					fsw.Renamed += fsw_Renamed;
					fsw.EnableRaisingEvents = true;
				}
				catch {
				}

				//Set up breadcrumb bar drag/drop functionality
				//breadcrumbBarControl1.SetDragHandlers(new DragEventHandler(bbi_DragEnter), new DragEventHandler(bbi_DragLeave), new DragEventHandler(bbi_DragOver), new DragEventHandler(bbi_Drop));

				//Set up Favorites Menu
				Task.Run(() => {
					SetUpFavoritesMenu();
				});

				//Set up ListView Color codes 
				//ShellListView.LVItemsColorCodes = this.LVItemsColor;


				//Load the ShellSettings
				IsCalledFromLoading = true;
				var statef = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
				WindowsAPI.SHGetSetSettings(ref statef, WindowsAPI.SSF.SSF_SHOWALLOBJECTS | WindowsAPI.SSF.SSF_SHOWEXTENSIONS, false);
				chkHiddenFiles.IsChecked = (statef.fShowAllObjects == 1);
				ShellListView.ShowHidden = chkHiddenFiles.IsChecked.Value;
				ShellTree.IsShowHiddenItems = chkHiddenFiles.IsChecked.Value;
				chkExtensions.IsChecked = (statef.fShowExtensions == 1);
				IsCalledFromLoading = false;

				isOnLoad = true;

				//'load from Registry
				LoadRegistryRelatedSettings();

				//'set up Explorer control
				InitializeExplorerControl();


				ViewGallery.SelectedIndex = 2;
				// set up history on breadcrumb bar (currently missing try-catch statement in order to catch error)
				try {
					Task.Run(() => {
						//breadcrumbBarControl1.ClearHistory();
						//breadcrumbBarControl1.HistoryItems = ReadHistoryFromFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\history.txt");
					});
				}
				catch (FileNotFoundException) {
					logger.Warn(String.Format("History file not found at location:{0}\\history.txt", Environment.SpecialFolder.LocalApplicationData));
				}

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
				chcm = new ContextMenu();
				chcm.Placement = PlacementMode.MousePoint;

				//Set up Version Info
				verNumber.Content = "Version " + (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault() as AssemblyInformationalVersionAttribute).InformationalVersion;
				lblArchitecture.Content = WindowsAPI.Is64bitProcess(Process.GetCurrentProcess()) ? "64-bit version" : "32-bit version";

				//'set StartUp location
				if (Application.Current.Properties["cmd"] != null && Application.Current.Properties["cmd"].ToString() != "-minimized") {
					String cmd = Application.Current.Properties["cmd"].ToString();

					if (cmd == "/nw")
						tcMain.NewTab(ShellListView.CurrentFolder, true);
				}
				else {
					InitializeInitialTabs();
				}
				//this.Activate(true);

			}
			catch (Exception exe) {
				MessageBox.Show(String.Format("An error occurred while loading the window. Please report this issue at http://bugtracker.better-explorer.com/. \r\n\r\n Here is some information about the error: \r\n\r\n{0}\r\n\r\n{1}", exe.Message, exe), "Error While Loading", MessageBoxButton.OK, MessageBoxImage.Error);
			}

		}

		void _keyjumpTimer_Tick(object sender, EventArgs e) {
			//key jump done
			KeyJumpGrid.IsOpen = false;
			(sender as System.Windows.Forms.Timer).Stop();
		}

		#endregion

		#region On Closing

		private void SaveSettings(String openedTabs) {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);
			rks.SetValue(@"LastWindowWidth", this.Width);
			rks.SetValue(@"LastWindowHeight", this.Height);
			rks.SetValue(@"LastWindowPosLeft", this.Left);
			rks.SetValue(@"LastWindowPosTop", this.Top);
			if (btnBlue.IsChecked == true) {
				rks.SetValue(@"CurrentTheme", "Blue");
			}
			else if (btnSilver.IsChecked == true) {
				rks.SetValue(@"CurrentTheme", "Silver");
			}
			else if (btnBlack.IsChecked == true) {
				rks.SetValue(@"CurrentTheme", "Black");
			}
			else if (btnGreen.IsChecked == true) {
				rks.SetValue(@"CurrentTheme", "Green");
			}
			switch (this.WindowState) {
				case System.Windows.WindowState.Maximized:
					rks.SetValue(@"LastWindowState", 2);
					break;
				case System.Windows.WindowState.Minimized:
					rks.SetValue(@"LastWindowState", 1);
					break;
				case System.Windows.WindowState.Normal:
					rks.SetValue(@"LastWindowState", 0);
					break;
				default:
					rks.SetValue(@"LastWindowState", -1);
					break;
			}

			rks.SetValue(@"IsRibonMinimized", TheRibbon.IsMinimized);
			rks.SetValue(@"OpenedTabs", openedTabs);
			rks.SetValue(@"RTLMode", FlowDirection == System.Windows.FlowDirection.RightToLeft ? "true" : "false");
			rks.SetValue(@"AutoSwitchFolderTools", Convert.ToInt32(asFolder));
			rks.SetValue(@"AutoSwitchArchiveTools", Convert.ToInt32(asArchive));
			rks.SetValue(@"AutoSwitchImageTools", Convert.ToInt32(asImage));
			rks.SetValue(@"AutoSwitchApplicationTools", Convert.ToInt32(asApplication));
			rks.SetValue(@"AutoSwitchLibraryTools", Convert.ToInt32(asLibrary));
			rks.SetValue(@"AutoSwitchDriveTools", Convert.ToInt32(asDrive));
			rks.SetValue(@"AutoSwitchVirtualDriveTools", Convert.ToInt32(asVirtualDrive));
			//rks.SetValue(@"IsLastTabCloseApp", Convert.ToInt32(this.IsCloseLastTabCloseApp));
			rks.SetValue(@"IsLastTabCloseApp", Convert.ToInt32(chkIsLastTabCloseApp.IsChecked.Value));

			rks.SetValue(@"IsConsoleShown", this.IsConsoleShown ? 1 : 0);
			rks.SetValue(@"TabBarAlignment", this.TabbaBottom.IsChecked == true ? "bottom" : "top");

			if (this.IsPreviewPaneEnabled)
				rks.SetValue(@"PreviewPaneWidth", (int)clPreview.ActualWidth, RegistryValueKind.DWord);
			if (this.IsInfoPaneEnabled)
				rks.SetValue(@"InfoPaneHeight", (int)rPreviewPane.ActualHeight, RegistryValueKind.DWord);
			if (this.IsConsoleShown)
				rks.SetValue(@"CmdWinHeight", rCommandPrompt.ActualHeight, RegistryValueKind.DWord);

			rks.Close();
		}

		private void RibbonWindow_Closing(object sender, CancelEventArgs e) {
			if (App.isStartNewWindows) {
				if (r != null) r.Close();
			}

			if (this.OwnedWindows.OfType<BExplorer.Shell.FileOperationDialog>().Count() > 0) {
				if (MessageBox.Show("Are you sure you want to cancel all running file operation tasks?", "", MessageBoxButton.YesNo) == MessageBoxResult.No) {
					e.Cancel = true;
					return;
				}
			}

			if (this.WindowState != System.Windows.WindowState.Minimized) {
				string OpenedTabs = "";
				foreach (Wpf.Controls.TabItem item in tcMain.Items) {
					OpenedTabs += ";" + item.ShellObject.ParsingName;
				}

				SaveSettings(OpenedTabs);
			}
			this.ShellListView.SaveSettingsToDatabase(this.ShellListView.CurrentFolder);
			SaveHistoryToFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\history.txt", this.bcbc.DropDownItems.OfType<String>().Select(s => s).ToList());
			AddToLog("Session Ended");
			//if (!App.isStartNewWindows) {
			e.Cancel = true;
			App.isStartMinimized = true;
			this.WindowState = System.Windows.WindowState.Minimized;
			this.Visibility = System.Windows.Visibility.Hidden;
			//}
		}

		#endregion

		#region On Navigated

		void ShellListView_Navigated(object sender, NavigatedEventArgs e) {
			SetupColumnsButton(this.ShellListView.AllAvailableColumns);
			SetSortingAndGroupingButtons();
			//SetUpBreadcrumbbarOnNavComplete(e);

			if (!tcMain.isGoingBackOrForward) {
				var Current = (tcMain.SelectedItem as Wpf.Controls.TabItem).log;
				if (Current.ForwardEntries.Count() > 1) Current.ClearForwardItems();
				if (Current.CurrentLocation != e.Folder) Current.CurrentLocation = e.Folder;
			}

			tcMain.isGoingBackOrForward = false;


			SetupUIonNavComplete(e);

			if (this.IsConsoleShown)
				ctrlConsole.ChangeFolder(e.Folder.ParsingName, e.Folder.IsFileSystem);

			Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
				ConstructMoveToCopyToMenu();
				SetUpJumpListOnNavComplete();
				SetUpButtonVisibilityOnNavComplete(SetUpNewFolderButtons());
				SetupUIOnSelectOrNavigate(true);
			}));

			if (this.IsInfoPaneEnabled) {
				Task.Run(() => {
					this.DetailsPanel.FillPreviewPane(this.ShellListView);
				});
			}
			if (e.OldFolder != this.ShellListView.CurrentFolder) {
				var selectedItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
				selectedItem.Header = this.ShellListView.CurrentFolder.DisplayName;
				selectedItem.Icon = this.ShellListView.CurrentFolder.Thumbnail.SmallBitmapSource;
				selectedItem.ShellObject = this.ShellListView.CurrentFolder;
				if (selectedItem != null) {
					var selectedPaths = selectedItem.SelectedItems;
					if (selectedPaths != null) {
						foreach (var path in selectedPaths.ToArray()) {
							var sho = this.ShellListView.Items.Where(w => w.CachedParsingName == path).SingleOrDefault();
							if (sho != null) {
								var index = this.ShellListView.ItemsHashed[sho];
								this.ShellListView.SelectItemByIndex(index, true);
								selectedPaths.Remove(path);
							}
						}
					}
				}
			}

			//This initially setup the statusbar after program opens
			Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
				SetUpStatusBarOnSelectOrNavigate(ShellListView.SelectedItems == null ? 0 : ShellListView.GetSelectedCount());
			}));

			this.ShellListView.Focus();
		}

		private void SetupUIonNavComplete(NavigatedEventArgs e) {
			btnSizeChart.IsEnabled = e.Folder.IsFileSystem;
			btnAutosizeColls.IsEnabled = ShellListView.View == ShellViewStyle.Details;

			if (e.Folder.ParsingName == KnownFolders.RecycleBin.ParsingName) {
				if (ShellListView.GetItemsCount() > 0) miRestoreALLRB.Visibility = Visibility.Visible;
			}
			else {
				miRestoreALLRB.Visibility = Visibility.Collapsed;
			}
			int selectedItemsCount = ShellListView.GetSelectedCount();
			bool IsChanged = (selectedItemsCount > 0);
			bool isFuncAvail;
			if (selectedItemsCount == 1) {
				isFuncAvail = ShellListView.GetFirstSelectedItem().IsFileSystem || ShellListView.CurrentFolder.ParsingName == KnownFolders.Libraries.ParsingName;
			}
			else {
				if (!(ShellListView.CurrentFolder.IsFolder && !ShellListView.CurrentFolder.IsDrive && !ShellListView.CurrentFolder.IsSearchFolder))
					ctgFolderTools.Visibility = Visibility.Collapsed;
				isFuncAvail = true;
			}
			btnCopy.IsEnabled = IsChanged;
			//btnPathCopy.IsEnabled = IsChanged;
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
				//uint iAttribute;
				pIDL = this.ShellListView.CurrentFolder.AbsolutePidl;
				WindowsAPI.SHFILEINFO sfi = new WindowsAPI.SHFILEINFO();
				IntPtr Res = IntPtr.Zero;
				if (pIDL != IntPtr.Zero && !ShellListView.CurrentFolder.IsFileSystem) {
					//if (!ShellListView.CurrentFolder.IsFileSystem) {
					Res = WindowsAPI.SHGetFileInfo(pIDL, 0, ref sfi, (uint)Marshal.SizeOf(sfi), SHGFI.IconLocation | SHGFI.SmallIcon | SHGFI.PIDL);
				}

				if (ShellListView.CurrentFolder.IsFileSystem) {
					WindowsAPI.SHGetFileInfo(ShellListView.CurrentFolder.ParsingName, 0, ref sfi, (uint)Marshal.SizeOf(sfi), (uint)SHGFI.IconLocation | (uint)SHGFI.SmallIcon);
				}

				System.Windows.Shell.JumpList.AddToRecentCategory(new JumpTask() {
					ApplicationPath = Process.GetCurrentProcess().MainModule.FileName,
					Arguments = String.Format("\"{0}\"", ShellListView.CurrentFolder.ParsingName),
					Title = ShellListView.CurrentFolder.GetDisplayName(SIGDN.NORMALDISPLAY),
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
			if (ShellListView.CurrentFolder.ParsingName.Contains(KnownFolders.Libraries.ParsingName) && ShellListView.CurrentFolder.ParsingName != KnownFolders.Libraries.ParsingName) {
				ctgLibraries.Visibility = Visibility.Visible;
				ctgFolderTools.Visibility = Visibility.Collapsed;
				ctgImage.Visibility = Visibility.Collapsed;
				ctgArchive.Visibility = Visibility.Collapsed;
				ctgVirtualDisk.Visibility = Visibility.Collapsed;
				ctgExe.Visibility = Visibility.Collapsed;

				try {
					SetupLibrariesTab(ShellLibrary.Load(ShellListView.CurrentFolder.GetDisplayName(SIGDN.NORMALDISPLAY), false));
				}
				catch {
				}
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

		/*
		private void SetUpBreadcrumbbarOnNavComplete(NavigatedEventArgs e) {
			//this.breadcrumbBarControl1.LoadDirectory(e.Folder);

			//this._IsBreadcrumbBarSelectionChnagedAllowed = true;
			//this.bcbc.UpdateLayout();
			//this.bcbc.Path = e.Folder.ParsingName;
			//this.breadcrumbBarControl1.LastPath = e.Folder.ParsingName;
			//var folder = e.isInSameTab ? e.Folder : e.Folder;
			//this.Title = "Better Explorer - " + e.Folder.GetDisplayName(BExplorer.Shell.Interop.SIGDN.NORMALDISPLAY);
			//e.Folder.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
			//e.Folder.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			//try {
			//	Wpf.Controls.TabItem selectedTabItem = (tcMain.SelectedItem as Wpf.Controls.TabItem);
			//	selectedTabItem.Header = folder.DisplayName;
			//	selectedTabItem.Icon = e.Folder.Thumbnail.BitmapSource;
			//	selectedTabItem.ShellObject = e.Folder;
			//	selectedTabItem.ToolTip = e.Folder.ParsingName;
			//} catch (Exception) {
			//}

			//try {

			//Wpf.Controls.TabItem it = new Closable_TabItem();
			//CreateTabbarRKMenu(it);

			//(tcMain.Items[tcMain.SelectedIndex] as Wpf.Controls.TabItem).Path = ShellListView.CurrentFolder;

			if (!tcMain.isGoingBackOrForward) {
				var Current = (tcMain.SelectedItem as Wpf.Controls.TabItem).log;
				if (Current.ForwardEntries.Count() > 1) Current.ClearForwardItems();
				if (Current.CurrentLocation != e.Folder) Current.CurrentLocation = e.Folder;
			}

			tcMain.isGoingBackOrForward = false;
		}
		*/
		#endregion

		#region Change Ribbon Color (Theme)

		public void ChangeRibbonThemeL(string ThemeName) {
			Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
				Resources.BeginInit();
				Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(String.Format("pack://application:,,,/Fluent;component/Themes/Office2010/{0}.xaml", ThemeName)) });
				Resources.MergedDictionaries.RemoveAt(0);
				Resources.EndInit();
			}));
		}

		public void ChangeRibbonTheme(string ThemeName, bool IsMetro = false) {
			Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
				Application.Current.Resources.BeginInit();
				Application.Current.Resources.MergedDictionaries.RemoveAt(1);
				if (IsMetro) {
					Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = new Uri(String.Format("pack://application:,,,/Fluent;component/Themes/Metro/{0}.xaml", "White")) });
				}
				else {
					Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = new Uri(String.Format("pack://application:,,,/Fluent;component/Themes/Office2010/{0}.xaml", ThemeName)) });
				}
				Application.Current.Resources.EndInit();
			}));
		}

		private void btnSilver_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonTheme("Silver");
			Utilities.SetRegistryValue("CurrentTheme", "Silver");
			KeepBackstageOpen = true;
		}

		private void btnBlue_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonTheme("Blue");
			Utilities.SetRegistryValue("CurrentTheme", "Blue");
			KeepBackstageOpen = true;
		}

		private void btnBlack_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonTheme("Black");
			btnBlack.IsChecked = true;

			Utilities.SetRegistryValue("CurrentTheme", "Black");
			KeepBackstageOpen = true;
		}

		private void btnGreen_Click(object sender, RoutedEventArgs e) {
			ChangeRibbonThemeL("Green");
			Utilities.SetRegistryValue("CurrentTheme", "Green");
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
			var dlg = new CommonOpenFileDialog();
			dlg.IsFolderPicker = true;
			if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
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
				var dialog = new TaskDialog();
				dialog.StandardButtons = TaskDialogStandardButtons.Ok;
				dialog.Text = exception.Message;
				dialog.Show();
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
			ShellLibrary lib = null;
			this.ShellListView.CurrentRefreshedItemIndex = this.ShellListView.GetFirstSelectedItemIndex();
			if (ShellListView.GetSelectedCount() == 1) {
				lib = ShellLibrary.Load(ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY), false);
			}
			else {
				lib = ShellLibrary.Load(ShellListView.CurrentFolder.GetDisplayName(SIGDN.NORMALDISPLAY), false);
			}
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
			ShellLibrary lib = null;
			this.ShellListView.CurrentRefreshedItemIndex = this.ShellListView.GetFirstSelectedItemIndex();
			if (ShellListView.GetSelectedCount() == 1) {
				lib = ShellLibrary.Load(ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY), false);
			}
			else {
				lib = ShellLibrary.Load(ShellListView.CurrentFolder.GetDisplayName(SIGDN.NORMALDISPLAY), false);
			}
			if (!IsFromSelectionOrNavigation) {
				lib.IsPinnedToNavigationPane = e.RoutedEvent.Name == "Checked";
			}

			lib.Close();
		}

		private void btnChangeLibIcon_Click(object sender, RoutedEventArgs e) {
			IconView iv = new IconView();
			iv.LoadIcons(ShellListView, true);
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
			TaskDialog td = new TaskDialog();
			td.Caption = "Reset Library";
			td.Icon = TaskDialogStandardIcon.Warning;
			td.Text = "Would you like to reset this library to the default settings?";
			td.InstructionText = "Reset Library Properties?";
			td.FooterIcon = TaskDialogStandardIcon.Information;
			//td.FooterText = "This will reset all the properties to default properties for library type";
			td.DetailsCollapsedLabel = "More Info";
			td.DetailsExpandedLabel = "More Info";
			td.DetailsExpandedText = "This will undo all changes you have made to this library, and reset it to its default state.";
			td.DetailsExpanded = false;
			td.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;
			td.StandardButtons = TaskDialogStandardButtons.Yes | TaskDialogStandardButtons.No;
			td.OwnerWindowHandle = this.Handle;
			if (td.Show() == TaskDialogResult.Yes) {
				if (ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY).ToLowerInvariant() != "documents" &&
								 ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY).ToLowerInvariant() != "music" &&
								 ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY).ToLowerInvariant() != "videos" &&
								 ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY).ToLowerInvariant() != "pictures") {
					ShellLibrary lib = ShellLibrary.Load(ShellListView.GetFirstSelectedItem().DisplayName, false);
					lib.IsPinnedToNavigationPane = true;
					lib.LibraryType = LibraryFolderType.Generic;
					lib.IconResourceId = new IconReference(@"C:\Windows\System32\imageres.dll", 187);
					lib.Close();
				}
			}
		}

		#endregion

		#region Navigation (Back/Forward Arrows) and Up Button

		private void leftNavBut_Click(object sender, RoutedEventArgs e) {
			tcMain.isGoingBackOrForward = true;
			ShellListView.Navigate((tcMain.SelectedItem as Wpf.Controls.TabItem).log.NavigateBack(), false, true);
		}

		private void rightNavBut_Click(object sender, RoutedEventArgs e) {
			tcMain.isGoingBackOrForward = true;
			ShellListView.Navigate((tcMain.SelectedItem as Wpf.Controls.TabItem).log.NavigateForward(), false, true);
		}

		private void downArrow_Click(object sender, RoutedEventArgs e) {
			cmHistory.Items.Clear();
			NavigationLog nl = (tcMain.SelectedItem as Wpf.Controls.TabItem).log;
			int i = 0;
			foreach (ShellItem item in nl.HistoryItemsList) {
				if (item != null) {
					item.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
					item.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

					cmHistory.Items.Add(Utilities.Build_MenuItem(item.GetDisplayName(SIGDN.NORMALDISPLAY), item, item.Thumbnail.BitmapSource,
																 checkable: true, isChecked: i == nl.CurrentLocPos, GroupName: "G1", onClick: miItems_Click));
					//MenuItem mi = new MenuItem();
					//mi.Header = item.GetDisplayName(SIGDN.NORMALDISPLAY);
					//mi.Tag = item;
					//mi.Icon = item.Thumbnail.BitmapSource;
					//mi.IsCheckable = true;
					//mi.IsChecked = (i == nl.CurrentLocPos);
					//mi.GroupName = "G1";
					//mi.Click += new RoutedEventHandler(miItems_Click);
					//cmHistory.Items.Add(mi);
				}
				i++;
			}

			cmHistory.Placement = PlacementMode.Bottom;
			cmHistory.PlacementTarget = navBarGrid;
			cmHistory.IsOpen = true;
		}

		void miItems_Click(object sender, RoutedEventArgs e) {
			ShellItem item = (ShellItem)(sender as MenuItem).Tag;
			if (item != null) {
				tcMain.isGoingBackOrForward = true;
				NavigationLog nl = (tcMain.Items[tcMain.SelectedIndex] as Wpf.Controls.TabItem).log;
				(sender as MenuItem).IsChecked = true;
				nl.CurrentLocPos = cmHistory.Items.IndexOf((sender as MenuItem));
				ShellListView.Navigate(item, false, true);
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
			try {
				ShellListView.ResizeIcons((int)e.NewValue);
			}
			catch (NullReferenceException) {
			}
		}

		private void btnRefresh_Click(object sender, RoutedEventArgs e) {
			ShellListView.RefreshContents();
			ShellTree.RefreshContents();
		}

		void mi_Click(object sender, RoutedEventArgs e) {
			MenuItem item = (sender as MenuItem);
			var parentButton = item.Parent as Fluent.DropDownButton;
			MenuItem ascitem = (MenuItem)parentButton.Items[parentButton.Items.IndexOf(misa)];

			var Sort = ascitem.IsChecked ? System.Windows.Forms.SortOrder.Ascending : System.Windows.Forms.SortOrder.Descending;
			ShellListView.SetSortCollumn(ShellListView.Collumns.IndexOf((Collumns)item.Tag), Sort);
		}
		void misng_Click(object sender, RoutedEventArgs e) {
			MenuItem item = (sender as MenuItem);
			item.IsChecked = true;
			if (this.ShellListView.IsGroupsEnabled) {
				this.ShellListView.DisableGroups();
			}
		}


		private void inRibbonGallery1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
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
					var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
					state.fShowAllObjects = 1;
					WindowsAPI.SHGetSetSettings(ref state, WindowsAPI.SSF.SSF_SHOWALLOBJECTS, true);
					ShellListView.ShowHidden = true;
					ShellTree.RefreshContents();
				}
			));
		}

		private void chkHiddenFiles_Unchecked(object sender, RoutedEventArgs e) {
			if (IsCalledFromLoading) return;
			Dispatcher.BeginInvoke(new Action(
				delegate() {
					var state = new BExplorer.Shell.Interop.Shell32.SHELLSTATE();
					state.fShowAllObjects = 0;
					WindowsAPI.SHGetSetSettings(ref state, WindowsAPI.SSF.SSF_SHOWALLOBJECTS, true);
					ShellListView.ShowHidden = false;
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
					WindowsAPI.SHGetSetSettings(ref state, WindowsAPI.SSF.SSF_SHOWEXTENSIONS, true);
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
					WindowsAPI.SHGetSetSettings(ref state, WindowsAPI.SSF.SSF_SHOWEXTENSIONS, true);
					ShellListView.RefreshContents();
				}
			));
		}

		#endregion

		#region Hide items

		[Obsolete("Does Nothing")]
		private void btnHideSelItems_Click(object sender, RoutedEventArgs e) {
			//FIXME:
			//ShellItemCollection SelItems = ShellListView.SelectedItems;
			//pd = new IProgressDialog(this.Handle);
			//pd.Title = "Applying attributes";
			//pd.CancelMessage = "Please wait while the operation is cancelled";
			//pd.Maximum = (uint)SelItems.Count;
			//pd.Value = 0;
			//pd.Line1 = "Applying attributes to:";
			//pd.Line3 = "Calculating Time Remaining...";
			//pd.ShowDialog(IProgressDialog.PROGDLG.Normal, IProgressDialog.PROGDLG.AutoTime, IProgressDialog.PROGDLG.NoMinimize);
			//Thread hthread = new Thread(new ParameterizedThreadStart(DoHideShow));
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

		#endregion

		#region Share Tab Commands (excluding Archive)

		private void btnMapDrive_Click(object sender, RoutedEventArgs e) {
			WindowsAPI.MapDrive(this.Handle, ShellListView.SelectedItems.Count() == 1 ? ShellListView.GetFirstSelectedItem().ParsingName : String.Empty);
		}

		private void btnDisconectDrive_Click(object sender, RoutedEventArgs e) {
			WindowsAPI.WNetDisconnectDialog(this.Handle, 1);
		}

		[Obsolete("Does Nothing")]
		private void Button_Click_4(object sender, RoutedEventArgs e) {
			//ShellListView.OpenShareUI();
		}

		private void btnAdvancedSecurity_Click(object sender, RoutedEventArgs e) {
			ShellListView.ShowPropPage(this.Handle, ShellListView.GetFirstSelectedItem().ParsingName, WindowsAPI.LoadResourceString(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), 9046, "Security1"));
		}

		#endregion

		#region Change Language

		private void TranslationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (ReadyToChangeLanguage) {
				Utilities.SetRegistryValue("Locale", ((TranslationComboBoxItem)e.AddedItems[0]).LocaleCode);
				((App)Application.Current).SelectCulture(((TranslationComboBoxItem)e.AddedItems[0]).LocaleCode);

				if (ShellListView.CurrentFolder.Parent.ParsingName == KnownFolders.Libraries.ParsingName) {
					btnCreateFolder.Header = FindResource("btnNewLibraryCP");  //"New Library";
					stNewFolder.Title = FindResource("btnNewLibraryCP").ToString();//"New Library";
					stNewFolder.Text = "Creates a new library in the current folder.";
				}
				else {
					btnCreateFolder.Header = FindResource("btnNewFolderCP");//"New Folder";
					stNewFolder.Title = FindResource("btnNewFolderCP").ToString(); //"New Folder";
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
					MessageBox.Show("There appears to have been an issue with converting the file. Make sure the filename \"" + Utilities.RemoveExtensionsFromFile(ShellListView.GetFirstSelectedItem().GetDisplayName(SIGDN.NORMALDISPLAY), new System.IO.FileInfo(item.ParsingName).Extension) + extension + "\" does already not exist.", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
				case "btnFlipX_Click":
					Rotation = RotateFlipType.RotateNoneFlipX;
					DefaultName_Addon = "_FlippedX";
					break;
				case "btnFlipY_Click":
					Rotation = RotateFlipType.RotateNoneFlipY;
					DefaultName_Addon = "_FlippedY";
					break;
				default:
					throw new Exception("Invalid sender");
			}

			foreach (ShellItem item in ShellListView.SelectedItems) {
				System.Drawing.Bitmap cvt = new Bitmap(item.ParsingName);
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
			ResizeImage.Open(ShellListView.GetFirstSelectedItem());
		}

		#endregion

		#region Folder Tools Commands
		private void btnChangeFoldericon_Click(object sender, RoutedEventArgs e) {
			IconView iv = new IconView();
			iv.LoadIcons(this.ShellListView, false);
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
				int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
				int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x77654");
				proc.WaitForExit();
				if (proc.ExitCode == -1) {
					isOnLoad = true;
					(sender as Fluent.CheckBox).IsChecked = false;
					isOnLoad = false;
					MessageBox.Show("Can't set Better Explorer as default!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			else {
				WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x77655");
				proc.WaitForExit();
				if (proc.ExitCode == -1) {
					isOnLoad = true;
					(sender as Fluent.CheckBox).IsChecked = true;
					isOnLoad = false;
					MessageBox.Show("Can't restore default filemanager!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void chkTreeExpand_CheckChanged(object sender, RoutedEventArgs e) {
			if (isOnLoad) return;
			var proc = Rename_CheckChanged_Helper();

			if (chkTreeExpand.IsChecked == true) {
				int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
				int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x88775");
				proc.WaitForExit();
			}
			else {
				int h = (int)WindowsAPI.getWindowId(null, "BetterExplorerOperations");
				int jj = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0, "0x88776");
				proc.WaitForExit();
			}
		}


		private void btnSetCurrentasStartup_Click(object sender, RoutedEventArgs e) {
			backstage.IsOpen = true;
			string CurrentLocString = ShellListView.CurrentFolder.ParsingName;
			tcMain.StartUpLocation = CurrentLocString;
			btnSetCurrentasStartup.Header = ShellListView.CurrentFolder.GetDisplayName(SIGDN.NORMALDISPLAY);
			btnSetCurrentasStartup.Icon = ShellListView.CurrentFolder.Thumbnail.BitmapSource;

			Utilities.SetRegistryValue("StartUpLoc", CurrentLocString);
		}

		private void chkIsFlyout_Checked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("HFlyoutEnabled", 1);
				//IsHFlyoutEnabled = true;
			}
		}

		private void chkIsFlyout_Unchecked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("HFlyoutEnabled", 0);
				//IsHFlyoutEnabled = false;
			}
		}


		private void chkIsTerraCopyEnabled_Checked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("FileOpExEnabled", 1);
				//IsExtendedFileOpEnabled = true;
			}
		}

		private void chkIsTerraCopyEnabled_Unchecked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("FileOpExEnabled", 0);
				//IsExtendedFileOpEnabled = false;
			}
		}

		private void chkIsCompartibleRename_Checked(object sender, RoutedEventArgs e) {
			Utilities.SetRegistryValue("CompartibleRename", 1);
		}

		private void chkIsRestoreTabs_Checked(object sender, RoutedEventArgs e) {
			IsrestoreTabs = true;
			Utilities.SetRegistryValue("IsRestoreTabs", 1);
		}

		private void chkIsRestoreTabs_Unchecked(object sender, RoutedEventArgs e) {
			IsrestoreTabs = false;
			Utilities.SetRegistryValue("IsRestoreTabs", 0);
		}

		private void chkIsVistaStyleListView_Checked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("IsVistaStyleListView", 1);
			}
		}

		private void chkIsVistaStyleListView_Unchecked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("IsVistaStyleListView", 0);
			}
		}

		private void gridSplitter1_DragCompleted(object sender, DragCompletedEventArgs e) {
			Utilities.SetRegistryValue("SearchBarWidth", SearchBarColumn.Width.Value);
		}

		private void SearchBarReset_Click(object sender, RoutedEventArgs e) {
			Utilities.SetRegistryValue("SearchBarWidth", SearchBarColumn.Width.Value);
		}

		private void chkShowCheckBoxes_Checked(object sender, RoutedEventArgs e) {
			ShellListView.ShowCheckboxes = true;
			//this.isCheckModeEnabled = true;
			ShellListView.RefreshContents();
		}

		private void chkShowCheckBoxes_Unchecked(object sender, RoutedEventArgs e) {
			ShellListView.ShowCheckboxes = false;
			//this.isCheckModeEnabled = false;
			ShellListView.RefreshContents();
		}

		private void chkOverwriteImages_Checked(object sender, RoutedEventArgs e) {
			OverwriteOnRotate = true;
			Utilities.SetRegistryValue("OverwriteImageWhileEditing", 1);
		}

		private void chkOverwriteImages_Unchecked(object sender, RoutedEventArgs e) {
			OverwriteOnRotate = false;
			Utilities.SetRegistryValue("OverwriteImageWhileEditing", 0);
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
				System.Windows.Point p = ShellViewHost.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
				this.GlassBorderThickness = new Thickness(8, p.Y, 8, 8);
			}
			else {
				System.Windows.Point p = backstage.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
				this.GlassBorderThickness = new Thickness(8, p.Y + backstage.ActualHeight, 8, 8);
			}
		}

		private void chkLogHistory_Checked(object sender, RoutedEventArgs e) {
			canlogactions = true;
			Utilities.SetRegistryValue("EnableActionLog", 1);
		}

		private void chkLogHistory_Unchecked(object sender, RoutedEventArgs e) {
			canlogactions = false;
			Utilities.SetRegistryValue("EnableActionLog", 0);
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

		[Obsolete("Does Nothing")]
		private void btnNavigationPane_Checked(object sender, RoutedEventArgs e) {
			//if (!isOnLoad)
			//ChangePaneVisibility(0x3, true);
		}

		[Obsolete("Does Nothing")]
		private void btnNavigationPane_Unchecked(object sender, RoutedEventArgs e) {
			//if (!isOnLoad)
			//ChangePaneVisibility(0x3, false);
		}

		private void btnPreviewPane_Checked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				//ChangePaneVisibility(0x2, true);
				this.clPreview.Width = new GridLength((double)this.PreviewPaneWidth);
				this.clPreviewSplitter.Width = new GridLength(1);
				var selectedItem = ShellListView.SelectedItems.FirstOrDefault();
				if (selectedItem != null && selectedItem.IsFileSystem && ShellListView.GetSelectedCount() == 1 && !selectedItem.IsFolder) {
					this.Previewer.FileName = selectedItem.ParsingName;
				}

				Utilities.SetRegistryValue("PreviewPaneEnabled", 1);
				this.IsPreviewPaneEnabled = true;
			}
		}

		private void btnPreviewPane_Unchecked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				//ChangePaneVisibility(0x2, false);
				this.clPreview.Width = new GridLength(0);
				this.clPreviewSplitter.Width = new GridLength(0);
				this.Previewer.FileName = null;
				Utilities.SetRegistryValue("PreviewPaneEnabled", 0);
				this.IsPreviewPaneEnabled = false;
			}
		}

		private void btnInfoPane_Unchecked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				//ChangePaneVisibility(0x1, false);
				this.rPreviewPane.Height = new GridLength(0);
				this.rPreviewPaneSplitter.Height = new GridLength(0);
				Utilities.SetRegistryValue("InfoPaneEnabled", 0);
				this.IsInfoPaneEnabled = false;
				var selectedItem = ShellListView.SelectedItems.FirstOrDefault();
				if (selectedItem != null) {
					//PreviewPanel.FillPreviewPane(Explorer);
				}
			}
		}

		private void btnInfoPane_Checked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				//ChangePaneVisibility(0x1, true);
				this.rPreviewPane.Height = new GridLength(this.InfoPaneHeight);
				this.rPreviewPaneSplitter.Height = new GridLength(1);
				Utilities.SetRegistryValue("InfoPaneEnabled", 1);
				this.IsInfoPaneEnabled = true;
			}
		}

		// Legacy Pane Code
		private void chkIsInfoPane_Checked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				IsInfoPaneEnabled = true;
				Utilities.SetRegistryValue("InfoPaneEnabled", 1);
				//ChangePaneVisibility(0x1, true);
			}
		}

		private void chkIsInfoPane_Unchecked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("InfoPaneEnabled", 0);
				IsInfoPaneEnabled = false;
				//ShellListView.NavigationOptions.PaneVisibility.Details = PaneVisibilityState.Hide;
				//ShellListView.Navigate(ShellListView.CurrentFolder);
			}
		}

		private void chkIsPreviewPane_Unchecked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("PreviewPaneEnabled", 0);
				IsPreviewPaneEnabled = false;
				//ShellListView.NavigationOptions.PaneVisibility.Preview = PaneVisibilityState.Hide;
				//ShellListView.Navigate(ShellListView.CurrentFolder);
			}
		}

		private void chkIsPreviewPane_Checked(object sender, RoutedEventArgs e) {
			if (!isOnLoad) {
				Utilities.SetRegistryValue("PreviewPaneEnabled", 1);
				IsPreviewPaneEnabled = true;
				//ShellListView.NavigationOptions.PaneVisibility.Preview = PaneVisibilityState.Show;
				//ShellListView.SetState();
				//ShellListView.Navigate(ShellListView.CurrentFolder);
			}
		}


		#endregion

		#region Breadcrumb Bar


		private void RibbonWindow_GotFocus(object sender, RoutedEventArgs e) {
			//breadcrumbBarControl1.ExitEditMode_IfNeeded();
			if (!backstage.IsOpen)
				ShellListView.Focus();
		}

		private void SaveHistoryToFile(string relativepath, List<String> history) {
			// Write each entry to a file. (the "false" parameter makes sure the file is overwritten.)
			using (StreamWriter sw = new StreamWriter(relativepath, false, Encoding.UTF8)) {
				foreach (string item in history) {
					if (!String.IsNullOrEmpty(item.Replace("\0", String.Empty)))
						sw.WriteLine(item);
				}
			}

		}


		void bbi_Drop(object sender, DragEventArgs e) {
			System.Windows.Point pt = e.GetPosition(sender as IInputElement);

			if (((sender as BreadcrumbItem).Data as ShellItem).IsFileSystem)
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
			else
				e.Effects = DragDropEffects.None;

			switch (e.Effects) {
				case DragDropEffects.All:
					break;
				case DragDropEffects.Copy:
					this.ShellListView.DoCopy(e.Data, ((sender as BreadcrumbItem).Data as ShellItem));
					break;
				case DragDropEffects.Link:
					break;
				case DragDropEffects.Move:
					this.ShellListView.DoMove(e.Data, ((sender as BreadcrumbItem).Data as ShellItem));
					break;
				case DragDropEffects.None:
					break;
				case DragDropEffects.Scroll:
					break;
				default:
					break;
			}

			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			BExplorer.Shell.Win32Point wpt = new BExplorer.Shell.Win32Point();
			wpt.x = (int)pt.X;
			wpt.y = (int)pt.Y;
			dropHelper.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
		}

		void bbi_DragOver(object sender, DragEventArgs e) {
			e.Handled = true;

			if (((sender as BreadcrumbItem).Data as ShellItem).IsFileSystem) {
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
			}
			else {
				e.Effects = DragDropEffects.None;
			}

			Win32Point ptw = new Win32Point();
			GetCursorPos(ref ptw);
			e.Handled = true;
			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			BExplorer.Shell.Win32Point wpt = new BExplorer.Shell.Win32Point();
			wpt.x = (int)ptw.X;
			wpt.y = (int)ptw.Y;
			dropHelper.DragOver(ref wpt, (int)e.Effects);
		}

		void bbi_DragLeave(object sender, DragEventArgs e) {
			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			dropHelper.DragLeave();
		}

		void bbi_DragEnter(object sender, DragEventArgs e) {
			if (((sender as BreadcrumbItem).Data as ShellItem).IsFileSystem) {
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
			}
			else {
				e.Effects = DragDropEffects.None;
			}

			Win32Point ptw = new Win32Point();
			GetCursorPos(ref ptw);
			e.Effects = DragDropEffects.None;
			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			BExplorer.Shell.Win32Point wpt = new BExplorer.Shell.Win32Point();
			wpt.x = (int)ptw.X;
			wpt.y = (int)ptw.Y;
			dropHelper.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
		}

		#endregion

		#region Search

		int CurrentProgressValue = 0;

		public void DoSearch() {
			try {
				if (edtSearchBox.FullSearchTerms != "") {
					SearchCondition searchCondition = SearchConditionFactory.ParseStructuredQuery(edtSearchBox.FullSearchTerms);
					ShellSearchFolder searchFolder = new ShellSearchFolder(searchCondition, ShellListView.CurrentFolder);
					//var test = searchFolder.ParsingName;
					this.ShellListView.CurrentFolder = searchFolder;
					ShellListView.Navigate(searchFolder, false, false);
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
			//TODO: Test this new, clearer code
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

		//private void MenuItem_Checked(object sender, RoutedEventArgs e) {
		//	e.Handled = true;
		//	bool isThereChecked = false;

		//	//TODO: Test
		//	foreach (var item in ((sender as MenuItem).Parent as SplitButton).Items.OfType<MenuItem>()) {
		//		if (item.IsChecked) {
		//			isThereChecked = true;
		//			break;
		//		}
		//	}

		//	/*
		//	foreach (object item in ((sender as MenuItem).Parent as SplitButton).Items) {
		//		if (item is MenuItem) {
		//			if ((item as MenuItem).IsChecked) {
		//				isThereChecked = true;
		//				break;
		//			}
		//		}
		//	}
		//	*/
		//	((sender as MenuItem).Parent as SplitButton).IsChecked = isThereChecked;
		//}

		//private void MenuItem_Unchecked(object sender, RoutedEventArgs e) {
		//	e.Handled = true;
		//	bool isThereChecked = false;


		//	//TODO: Test
		//	foreach (var item in ((sender as MenuItem).Parent as SplitButton).Items.OfType<MenuItem>()) {
		//		if (item.IsChecked) {
		//			isThereChecked = true;
		//			break;
		//		}
		//	}

		//	/*
		//	foreach (object item in ((sender as MenuItem).Parent as SplitButton).Items) {
		//		if (item is MenuItem) {
		//			if ((item as MenuItem).IsChecked) {
		//				isThereChecked = true;
		//				break;
		//			}
		//		}
		//	}
		//	*/

		//	((sender as MenuItem).Parent as SplitButton).IsChecked = isThereChecked;
		//}

		[Obsolete("This does nothing")]
		private void MenuItem_Click_2(object sender, RoutedEventArgs e) {
			e.Handled = true;
			//(sender as MenuItem).IsChecked = !(sender as MenuItem).IsChecked;
		}

		/*
		private void ctgSearch_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
			System.Windows.Forms.MessageBox.Show("Feature not added");
			//if (((System.Windows.Visibility)e.NewValue) == System.Windows.Visibility.Collapsed)
			//    TheRibbon.SelectedTabItem = HomeTab;
		}
		*/


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

		private void chkFolder_Checked(object sender, RoutedEventArgs e) {
			asFolder = true;
		}

		private void chkFolder_Unchecked(object sender, RoutedEventArgs e) {
			asFolder = false;
		}

		private void chkDrive_Checked(object sender, RoutedEventArgs e) {
			asDrive = true;
		}

		private void chkDrive_Unchecked(object sender, RoutedEventArgs e) {
			asDrive = false;
		}

		private void chkArchive_Checked(object sender, RoutedEventArgs e) {
			asArchive = true;
		}

		private void chkArchive_Unchecked(object sender, RoutedEventArgs e) {
			asArchive = false;
		}

		private void chkApp_Checked(object sender, RoutedEventArgs e) {
			asApplication = true;
		}

		private void chkApp_Unchecked(object sender, RoutedEventArgs e) {
			asApplication = false;
		}

		private void chkImage_Checked(object sender, RoutedEventArgs e) {
			asImage = true;
		}

		private void chkImage_Unchecked(object sender, RoutedEventArgs e) {
			asImage = false;
		}

		private void chkLibrary_Checked(object sender, RoutedEventArgs e) {
			asLibrary = true;
		}

		private void chkLibrary_Unchecked(object sender, RoutedEventArgs e) {
			asLibrary = false;
		}


		private void chkVirtualTools_Checked(object sender, RoutedEventArgs e) {
			asVirtualDrive = true;
		}

		private void chkVirtualTools_Unchecked(object sender, RoutedEventArgs e) {
			asVirtualDrive = false;
		}

		#endregion

		#region Tabs

		void mimc_Click(object sender, RoutedEventArgs e) {
			MenuItem mi = sender as MenuItem;
			SetFOperation((mi.Tag as ShellItem), BExplorer.Shell.OperationType.Copy);
		}

		void mim_Click(object sender, RoutedEventArgs e) {
			MenuItem mi = sender as MenuItem;
			SetFOperation((mi.Tag as ShellItem), BExplorer.Shell.OperationType.Move);
		}

		private void btnNewTab_Click(object sender, RoutedEventArgs e) {
			tcMain.NewTab();
		}

		private void btnTabClone_Click(object sender, RoutedEventArgs e) {
			tcMain.CloneTabItem(tcMain.Items[tcMain.SelectedIndex] as Wpf.Controls.TabItem);
		}

		private void btnTabCloseC_Click(object sender, RoutedEventArgs e) {
			CloseTab(tcMain.SelectedItem as Wpf.Controls.TabItem);
		}

		void newt_CloseTab(object sender, RoutedEventArgs e) {
			CloseTab(e.Source as Wpf.Controls.TabItem);
		}

		private void btnUndoClose_Click(object sender, RoutedEventArgs e) {
			tcMain.ReOpenTab(tcMain.ReopenableTabs[tcMain.ReopenableTabs.Count - 1]);
			//reopenabletabs.RemoveAt(reopenabletabs.Count - 1);
			btnUndoClose.IsEnabled = tcMain.ReopenableTabs.Count != 0;
		}

		void gli_Click(object sender, NavigationLogEventArgs e) {
			tcMain.ReOpenTab(e.NavigationLog);
			//reopenabletabs.Remove((sender as UndoCloseGalleryItem).nav);
			btnUndoClose.IsEnabled = tcMain.ReopenableTabs.Count != 0;
		}

		void gli_Click(object sender, Tuple<string> e) {
			var list = SavedTabsList.LoadTabList(String.Format("{0}{1}.txt", sstdir, e.Item1));
			for (int i = 0; i < list.Count; i++) {
				var tabitem = tcMain.NewTab(list[i].ToShellParsingName());
				if (i == list.Count - 1)
					tcMain.SelectedItem = tabitem;
			}
		}

		void t_Tick(object sender, EventArgs e) {
			if (!Keyboard.IsKeyDown(Key.Tab)) {
				var item = (sender as System.Windows.Forms.Timer).Tag as ShellItem;
				//this.ShellListView.Cancel = true;
				if (item != this.ShellListView.CurrentFolder || item.IsSearchFolder) {
					this.ShellListView.SaveSettingsToDatabase(this.ShellListView.CurrentFolder);
					this.ShellListView.CurrentFolder = item;
					ShellListView.Navigate(item, false, false);
				}
				(sender as System.Windows.Forms.Timer).Stop();
			}
		}

		private void tcMain_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Middle) {
				tcMain.CloneTabItem(tcMain.SelectedItem as Wpf.Controls.TabItem);
			}
		}

		private void GoToSearchBox(object sender, ExecutedRoutedEventArgs e) {
			edtSearchBox.Focus();
			Keyboard.Focus(edtSearchBox);
		}

		void newt_PreviewMouseMove(object sender, MouseEventArgs e) {
			var tabItem = e.Source as Wpf.Controls.TabItem;

			if (tabItem == null)
				return;
			else if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
				DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
		}


		void newt_Drop(object sender, DragEventArgs e) {
			e.Handled = true;
			var tabItemTarget = e.Source as Wpf.Controls.TabItem;

			var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;
			if (tabItemSource != null) {
				if (!tabItemTarget.Equals(tabItemSource)) {
					var tabControl = tabItemTarget.Parent as TabControl;
					int tabState = -1;
					int sourceIndex = tabControl.Items.IndexOf(tabItemSource);
					int targetIndex = tabControl.Items.IndexOf(tabItemTarget);
					if (!tabItemSource.IsSelected && tabItemTarget.IsSelected)
						tabState = 1;
					else if (!tabItemSource.IsSelected && !tabItemTarget.IsSelected)
						tabState = 2;
					else
						tabState = 0;

					tabControl.Items.Remove(tabItemSource);
					tabControl.Items.Insert(targetIndex, tabItemSource);

					if (tabState == 1)
						tabControl.SelectedIndex = sourceIndex;
					else if (tabState == 0)
						tabControl.SelectedIndex = targetIndex;
				}
			}
			else {
				System.Windows.Point pt = e.GetPosition(sender as IInputElement);

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

				IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
				BExplorer.Shell.Win32Point wpt = new BExplorer.Shell.Win32Point();
				wpt.x = (int)pt.X;
				wpt.y = (int)pt.Y;
				dropHelper.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
			}

		}

		void newt_DragOver(object sender, DragEventArgs e) {
			e.Handled = true;

			if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem) {
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
			}
			else {
				e.Effects = DragDropEffects.None;
			}

			Win32Point ptw = new Win32Point();
			GetCursorPos(ref ptw);
			e.Handled = true;

			if (e.Data.GetType() != typeof(Wpf.Controls.TabItem)) {
				IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
				BExplorer.Shell.Win32Point wpt = new BExplorer.Shell.Win32Point();
				wpt.x = (int)ptw.X;
				wpt.y = (int)ptw.Y;
				dropHelper.DragOver(ref wpt, (int)e.Effects);
			}
		}

		void newt_DragLeave(object sender, DragEventArgs e) {
			IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
			dropHelper.DragLeave();
		}

		void newt_DragEnter(object sender, DragEventArgs e) {
			e.Handled = true;
			var tabItem = e.Source as Wpf.Controls.TabItem;

			if (tabItem == null)
				return;

			//if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
			//{
			//    DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
			//}
			//if (e.Data.GetDataPresent(DataFormats.FileDrop))
			//{
			if ((sender as Wpf.Controls.TabItem).ShellObject.IsFileSystem) {
				e.Effects = (e.KeyStates & DragDropKeyStates.ControlKey) == DragDropKeyStates.ControlKey ? DragDropEffects.Copy : DragDropEffects.Move;
			}
			else {
				e.Effects = DragDropEffects.None;
			}


			Win32Point ptw = new Win32Point();
			GetCursorPos(ref ptw);
			e.Effects = DragDropEffects.None;
			var tabItemSource = e.Data.GetData(typeof(Wpf.Controls.TabItem)) as Wpf.Controls.TabItem;
			//TODO: fix this!!!
			if (tabItemSource == null) {
				IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
				BExplorer.Shell.Win32Point wpt = new BExplorer.Shell.Win32Point();
				wpt.x = (int)ptw.X;
				wpt.y = (int)ptw.Y;
				dropHelper.DragEnter(this.Handle, (System.Runtime.InteropServices.ComTypes.IDataObject)e.Data, ref wpt, (int)e.Effects);
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
			var objs = new List<ShellItem>(from Wpf.Controls.TabItem x in tcMain.Items select x.ShellObject);
			//foreach (Wpf.Controls.TabItem item in tcMain.Items) {
			//	objs.Add(item.ShellObject);
			//}

			String str = PathStringCombiner.CombinePaths(objs, "|");
			var list = SavedTabsList.CreateFromString(str);

			//BetterExplorer.Tabs.NameTabList ntl = new BetterExplorer.Tabs.NameTabList();
			//ntl.Owner = this;
			//ntl.ShowDialog();


			var Name = BetterExplorer.Tabs.NameTabList.Open(this);
			if (Name == null) return;

			//if (ntl.dialogresult) {
			if (!System.IO.Directory.Exists(sstdir)) System.IO.Directory.CreateDirectory(sstdir);

			SavedTabsList.SaveTabList(list, String.Format("{0}{1}.txt", sstdir, Name));
			miTabManager.IsEnabled = true;
			//if (!miTabManager.IsEnabled)
			//miTabManager.IsEnabled = true;
			//}
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
				UndoCloseGalleryItem gli = new UndoCloseGalleryItem();
				gli.LoadData(item);
				gli.Click += gli_Click;
				rotGallery.Items.Add(gli);
			}
		}

		private void rotGallery_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var Item = (e.AddedItems[0] as UndoCloseGalleryItem);
			if (Item != null) {
				Item.PerformClickEvent();
			}
		}

		private void btnChangeTabsFolder_Click(object sender, RoutedEventArgs e) {
			var ctf = new CommonOpenFileDialog("Change Tab Folder");
			ctf.IsFolderPicker = true;
			ctf.Multiselect = false;
			ctf.InitialDirectory = new DirectoryInfo(sstdir).Parent.FullName;
			if (ctf.ShowDialog() == CommonFileDialogResult.Ok) {
				Utilities.SetRegistryValue("SavedTabsDirectory", ctf.FileName + "\\");
				txtDefSaveTabs.Text = ctf.FileName + "\\";
				sstdir = ctf.FileName + "\\";
			}
		}

		private void stGallery_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				(e.AddedItems[0] as SavedTabsListGalleryItem).PerformClickEvent();
			}
			catch {
			}
		}

		private void btnSavedTabs_DropDownOpened(object sender, EventArgs e) {
			stGallery.Items.Clear();
			foreach (string item in LoadListOfTabListFiles()) {
				SavedTabsListGalleryItem gli = new SavedTabsListGalleryItem(item);
				gli.Directory = sstdir;
				gli.Click += gli_Click;
				gli.SetUpTooltip((FindResource("tabTabsCP") as string));
				stGallery.Items.Add(gli);
			}
		}

		private void miTabManager_Click(object sender, RoutedEventArgs e) {
			string sstdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\";
			if (Directory.Exists(sstdir)) {
				BetterExplorer.Tabs.TabManager man = new Tabs.TabManager();
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

		#region Customize Quick Access Toolbar

		public Dictionary<string, IRibbonControl> GetAllButtonsAsDictionary() {
			var rb = new Dictionary<string, Fluent.IRibbonControl>();

			foreach (Fluent.RibbonTabItem item in TheRibbon.Tabs) {
				foreach (RibbonGroupBox itg in item.Groups) {
					foreach (var ic in itg.Items.OfType<IRibbonControl>()) {
						rb.Add((ic as FrameworkElement).Name, ic);
					}
				}

				//	foreach (object ic in itg.Items) {
				//		if (ic is IRibbonControl) rb.Add((ic as FrameworkElement).Name, (ic as IRibbonControl));
				//	}
				//}
			}

			return rb;
		}

		#endregion

		#region Recycle Bin

		public void UpdateRecycleBinInfos() {
			var rb = KnownFolders.RecycleBin;
			int count = rb.Count(); //TODO: Find out if we can remove [count]

			if (rb.Count() == 0) {
				Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
												 (ThreadStart)(() => {
													 miEmptyRB.Visibility = System.Windows.Visibility.Collapsed;
													 miRestoreALLRB.Visibility = System.Windows.Visibility.Collapsed;
													 miRestoreRBItems.Visibility = System.Windows.Visibility.Collapsed;
													 btnRecycleBin.LargeIcon = @"..\Images\RecycleBinEmpty32.png";
													 btnRecycleBin.Icon = @"..\Images\RecycleBinEmpty16.png";
													 lblRBItems.Text = "0 Items";
													 lblRBItems.Visibility = System.Windows.Visibility.Collapsed;
													 lblRBSize.Text = "0 bytes";
													 lblRBSize.Visibility = System.Windows.Visibility.Collapsed;
												 }));
			}
			else {
				var size = (long)rb.Where(c => c.IsFolder == false || !String.IsNullOrEmpty(Path.GetExtension(c.ParsingName))).Sum(c => c.GetPropertyValue(SystemProperties.FileSize, typeof(long)).IsNullOrEmpty ? 0 : (long)Convert.ToDouble(c.GetPropertyValue(SystemProperties.FileSize, typeof(long)).Value));
				Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
												 (ThreadStart)(() => {
													 miRestoreALLRB.Visibility = System.Windows.Visibility.Visible;
													 miEmptyRB.Visibility = System.Windows.Visibility.Visible;
													 btnRecycleBin.LargeIcon = @"..\Images\RecycleBinFull32.png";
													 btnRecycleBin.Icon = @"..\Images\RecycleBinFull16.png";
													 btnRecycleBin.UpdateLayout();
													 lblRBItems.Visibility = System.Windows.Visibility.Visible;
													 lblRBItems.Text = String.Format("{0} Items", count);
													 lblRBSize.Text = WindowsAPI.StrFormatByteSize(size);
													 lblRBSize.Visibility = System.Windows.Visibility.Visible;
												 }));
			}
		}

		private void miEmptyRB_Click(object sender, RoutedEventArgs e) {
			WindowsAPI.SHEmptyRecycleBin(this.Handle, string.Empty, 0);
			UpdateRecycleBinInfos();
		}

		private void miOpenRB_Click(object sender, RoutedEventArgs e) {
			ShellListView.Navigate((ShellItem)KnownFolders.RecycleBin);
		}

		private void miRestoreRBItems_Click(object sender, RoutedEventArgs e) {
			foreach (ShellItem item in ShellListView.SelectedItems.ToArray()) {
				//TODO: Fix this
				//RestoreFromRB(item.Name);
			}
			UpdateRecycleBinInfos();
		}

		private bool RestoreFromRB(string Item) {
			var Shl = new Shell();
			Folder Recycler = Shl.NameSpace(10);
			for (int i = 0; i < Recycler.Items().Count; i++) {
				Shell32.FolderItem FI = Recycler.Items().Item(i);
				string FileName = Recycler.GetDetailsOf(FI, 0);
				if (Path.GetExtension(FileName) == "") FileName += Path.GetExtension(FI.Path);
				//Necessary for systems with hidden file extensions.
				string FilePath = Recycler.GetDetailsOf(FI, 1);
				if (Item == Path.Combine(FilePath, FileName)) {
					DoVerb(FI, "ESTORE");
					return true;
				}
			}
			return false;
		}

		private bool DoVerb(Shell32.FolderItem Item, string Verb) {
			foreach (FolderItemVerb FIVerb in Item.Verbs()) {
				if (FIVerb.Name.ToUpper().Contains(Verb.ToUpper())) {
					FIVerb.DoIt();
					return true;
				}
			}
			return false;
		}

		private void miRestoreALLRB_Click(object sender, RoutedEventArgs e) {
			var Shl = new Shell();
			Folder Recycler = Shl.NameSpace(10);

			for (int i = 0; i < Recycler.Items().Count; i++) {
				Shell32.FolderItem FI = Recycler.Items().Item(i);
				DoVerb(FI, "ESTORE");
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
				this.ShellListView.Navigate(new ShellItem(args.Item1.ToLowerInvariant().Replace("cd", String.Empty).Replace("/d", String.Empty).Trim()));
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
					Fluent.MenuItem mli = new MenuItem();
					mli.Header = lib.DisplayName;
					lib.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
					mli.Icon = lib.Thumbnail.BitmapSource;
					mli.Tag = ShellLibrary.Load(Path.GetFileNameWithoutExtension(lib.ParsingName), true);
					mli.Click += mli_Click;
					mnuIncludeInLibrary.Items.Add(mli);
				}

				mnuIncludeInLibrary.Items.Add(new Separator());

				Fluent.MenuItem mln = new MenuItem();
				mln.Header = "Create new library";
				mln.Click += mln_Click;
				mnuIncludeInLibrary.Items.Add(mln);

				mnuIncludeInLibrary.IsEnabled = true;
			}
			else {
				mnuIncludeInLibrary.IsEnabled = false;
			}
		}

		void mln_Click(object sender, RoutedEventArgs e) {
			ShellLibrary lib = ShellListView.CreateNewLibrary(ShellListView.GetFirstSelectedItem().DisplayName);
			if (ShellListView.GetFirstSelectedItem().IsFolder) {
				lib.Add(ShellListView.GetFirstSelectedItem().ParsingName);
			}
		}

		void mli_Click(object sender, RoutedEventArgs e) {
			ShellLibrary lib = ShellLibrary.Load(((ShellItem)(sender as Fluent.MenuItem).Tag).DisplayName, false);
			if (ShellListView.GetFirstSelectedItem().IsFolder) {
				lib.Add(ShellListView.GetFirstSelectedItem().ParsingName);
			}
		}

		#endregion

		#region Misc

		public MainWindow() {
			TaskbarManager.Instance.ApplicationId = "{A8795DFC-A37C-41E1-BC3D-6BBF118E64AD}";

			CommandBinding cbNavigateBack = new CommandBinding(AppCommands.RoutedNavigateBack, leftNavBut_Click);
			this.CommandBindings.Add(cbNavigateBack);
			CommandBinding cbNavigateFF = new CommandBinding(AppCommands.RoutedNavigateFF, rightNavBut_Click);
			this.CommandBindings.Add(cbNavigateFF);
			CommandBinding cbNavigateUp = new CommandBinding(AppCommands.RoutedNavigateUp, btnUpLevel_Click);
			this.CommandBindings.Add(cbNavigateUp);
			CommandBinding cbGoToSearchBox = new CommandBinding(AppCommands.RoutedGotoSearch, GoToSearchBox);
			this.CommandBindings.Add(cbGoToSearchBox);



			CommandBinding cbnewtab = new CommandBinding(AppCommands.RoutedNewTab, (sender, e) =>
				tcMain.NewTab()
			);
			this.CommandBindings.Add(cbnewtab);
			CommandBinding cbGotoCombo = new CommandBinding(AppCommands.RoutedEnterInBreadCrumbCombo, (sender, e) => { this.ShellListView.IsFocusAllowed = false; this.bcbc.SetInputState(); });
			this.CommandBindings.Add(cbGotoCombo);

			CommandBinding cbChangeTab = new CommandBinding(AppCommands.RoutedChangeTab, (sender, e) => {
				t.Stop();
				int selIndex = tcMain.SelectedIndex == tcMain.Items.Count - 1 ? 0 : tcMain.SelectedIndex + 1;
				tcMain.SelectedItem = tcMain.Items[selIndex];
			});
			this.CommandBindings.Add(cbChangeTab);

			CommandBinding cbCloseTab = new CommandBinding(AppCommands.RoutedCloseTab, (sender, e) => {
				if (tcMain.SelectedIndex == 0 && tcMain.Items.Count == 1) {
					Close();
					return;
				}

				int CurSelIndex = tcMain.SelectedIndex;
				tcMain.SelectedItem = tcMain.SelectedIndex == 0 ? tcMain.Items[1] : tcMain.Items[CurSelIndex - 1];
				tcMain.Items.RemoveAt(CurSelIndex);
			});

			this.CommandBindings.Add(cbCloseTab);


			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);

			//   ExplorerBrowser.SetCustomDialogs(false);//Convert.ToInt32(rks.GetValue(@"IsCustomFO", 0)) == 1);
			//ExplorerBrowser.IsOldSysListView = Convert.ToInt32(rks.GetValue(@"IsVistaStyleListView", 1)) == 1;

			// loads current Ribbon color theme
			try {
				switch (Convert.ToString(rks.GetValue(@"CurrentTheme", "Blue"))) {
					case "Blue":
						ChangeRibbonTheme("Blue");
						break;
					case "Silver":
						ChangeRibbonTheme("Silver");
						break;
					case "Black":
						ChangeRibbonTheme("Black");
						break;
					case "Green":
						ChangeRibbonTheme("Green");
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

				string loc;
				if (Convert.ToString(rks.GetValue(@"Locale", ":null:")) == ":null:") {
					//load current UI language in case there is no specified registry value
					loc = Thread.CurrentThread.CurrentUICulture.Name; ;
				}
				else {
					loc = Convert.ToString(rks.GetValue(@"Locale", ":null:"));
				}

				((App)Application.Current).SelectCulture(loc, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\translation.xaml");

			}
			catch (Exception ex) {
				MessageBox.Show(String.Format("An error occurred while trying to load the locale data from the Registry. \n\r \n\r{0}\n\r \n\rPlease let us know of this issue at http://bugtracker.better-explorer.com/", ex.Message), "Locale Load Error - " + ex);
			}

			// gets values from registry to be applied after initialization
			string lohc = Convert.ToString(rks.GetValue(@"Locale", ":null:"));
			double sbw = Convert.ToDouble(rks.GetValue(@"SearchBarWidth", "220"));
			string rtlused = Convert.ToString(rks.GetValue(@"RTLMode", "notset"));
			string tabba = Convert.ToString(rks.GetValue(@"TabBarAlignment", "top"));

			rks.Close();
			rk.Close();


			//Main Initialization routine
			InitializeComponent();

			//isOnLoad = true;
			////chkOldSysListView.IsChecked = ExplorerBrowser.IsOldSysListView;
			//isOnLoad = false;

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


			// sets size of search bar
			this.SearchBarColumn.Width = new GridLength(sbw);

			/*
			// store 1st value
			PreviouseWindowState = WindowState;
			*/

			// attach to event (used to store prev. win. state)
			//FIXME: fix the event
			//LayoutUpdated += Window_LayoutUpdated;

			// prepares RTL mode

			FlowDirection = rtlused == "true" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

			if (rtlset) { //TODO: Find out if we can merge this with [if (!rtlset)]
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
			ReadyToChangeLanguage = true;
			/*
			Task.Run(() => {
				LoadInternalList();
			});
			*/
		}

		private void beNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e) {
			this.Visibility = Visibility.Visible;
			if (this.WindowState == WindowState.Minimized) {
				WindowsAPI.ShowWindow(Handle, (int)WindowsAPI.ShowCommands.SW_RESTORE);
			}

			this.Activate();
			this.Topmost = true;  // important
			this.Topmost = false; // important
			this.Focus();         // important
		}

		void ShellListView_ViewStyleChanged(object sender, BExplorer.Shell.ViewChangedEventArgs e) {
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

			btnSbTiles.IsChecked = e.CurrentView == ShellViewStyle.Tile;
			if (e.CurrentView == ShellViewStyle.Tile) {
				ViewGallery.SelectedIndex = 6;
			}

			if (e.CurrentView == ShellViewStyle.Content) {
				ViewGallery.SelectedIndex = 7;
			}
			else if (e.CurrentView == ShellViewStyle.Thumbstrip) {
				ViewGallery.SelectedIndex = 8;
			}

			//IsCalledFromViewEnum = true;
			zoomSlider.Value = e.ThumbnailSize;
			//IsCalledFromViewEnum = false;
			btnAutosizeColls.IsEnabled = e.CurrentView == ShellViewStyle.Details;
		}

		void r_OnMessageReceived(object sender, EventArgs e) {
			new Thread(() => {
				Thread.Sleep(1000);
				UpdateRecycleBinInfos();
			}).Start();
		}

		private void btnNewItem_DropDownOpened(object sender, EventArgs e) {
			ShellContextMenu mnu = new ShellContextMenu(this.ShellListView, 0);
			var controlPos = btnNewItem.TransformToAncestor(Application.Current.MainWindow).Transform(new System.Windows.Point(0, 0));
			var tempPoint = PointToScreen(new System.Windows.Point(controlPos.X, controlPos.Y));
			mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)btnNewItem.ActualHeight));
			btnNewItem.IsDropDownOpen = false;
		}

		private void mnuPinToStart_Click(object sender, RoutedEventArgs e) {
			if (ShellListView.GetSelectedCount() == 1) {
				string loc = KnownFolders.StartMenu.ParsingName + @"\" + ShellListView.GetFirstSelectedItem().DisplayName + ".lnk";
				ShellLink link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.GetFirstSelectedItem().ParsingName;
				link.Save(loc);
				link.Dispose();

				WindowsAPI.PinUnpinToStartMenu(loc);
			}

			if (ShellListView.GetSelectedCount() == 0) {
				string loc = KnownFolders.StartMenu.ParsingName + @"\" + ShellListView.CurrentFolder.DisplayName + ".lnk";
				ShellLink link = new ShellLink();
				link.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
				link.Target = ShellListView.CurrentFolder.ParsingName;
				link.Save(loc);
				link.Dispose();

				WindowsAPI.PinUnpinToStartMenu(loc);
			}
		}

		private void tmpButtonB_Click(object sender, RoutedEventArgs e) {
			ExportColumnDataToTextFile(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\ColumnData.txt");
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

				System.Windows.Forms.SendKeys.SendWait(keyText);
			}
		}

		private void ToolBar_SizeChanged(object sender, SizeChangedEventArgs e) {
			ToolBar toolBar = sender as ToolBar;
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
			//TODO: Consider using
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
				var Item = new MenuItem();
				Item.Header = Node.Attribute("Name").Value;
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
			tcMain.newt_PreviewMouseMove = newt_PreviewMouseMove;
			tcMain.ConstructMoveToCopyToMenu += ConstructMoveToCopyToMenu;
			tcMain.DefaultTabPath = tcMain.StartUpLocation.ToShellParsingName();

			tcMain.StartUpLocation = Utilities.GetRegistryValue("StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();
			if (tcMain.StartUpLocation == "") {
				Utilities.SetRegistryValue("StartUpLoc", KnownFolders.Libraries.ParsingName);
				tcMain.StartUpLocation = KnownFolders.Libraries.ParsingName;
			}
		}

		System.Windows.Forms.Timer focusTimer = new System.Windows.Forms.Timer();
		private void RibbonWindow_Activated(object sender, EventArgs e) {
			focusTimer.Interval = 500;
			focusTimer.Tick += focusTimer_Tick;
			focusTimer.Start();
		}

		void focusTimer_Tick(object sender, EventArgs e) {
			this.ShellListView.Focus();
			(sender as System.Windows.Forms.Timer).Stop();
		}

		#endregion

		private void RibbonWindow_StateChanged(object sender, EventArgs e) {
			if (this.WindowState != System.Windows.WindowState.Minimized) {
				focusTimer.Interval = 500;
				focusTimer.Tick += focusTimer_Tick;
				focusTimer.Start();
			}
		}

		private void txtEditor_TextChanged(object sender, TextChangedEventArgs e) {
			this.ShellListView.NewName = this.txtEditor.Text;
		}

		private void txtEditor_PreviewKeyDown(object sender, KeyEventArgs e) {
			//if (e.Key == Key.Escape)
			//{
			//	this.ShellListView.EndLabelEdit(true);
			//}
			//if (e.Key == Key.Enter)
			//{
			//	this.ShellListView.EndLabelEdit();
			//}
		}

		private void Editor_Closed(object sender, EventArgs e) {
			this.ShellListView.Focus();
			var index = this.ShellListView.ItemForRename;
			this.ShellListView.ItemForRename = -1;
			this.ShellListView.UpdateItem(index);
			//FocusManager.SetIsFocusScope(this, true);
			//MessageBox.Show(FocusManager.GetFocusedElement(this).ToString());
		}

		private void Editor_Opened(object sender, EventArgs e) {
			//FocusManager.SetIsFocusScope(this.txtEditor, true);
		}

		private void pop_items(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e) {
			Odyssey.Controls.BreadcrumbItem item = e.Item;
			if (item.Items.Count == 0) {
				PopulateFolders(item);
				e.Handled = true;
			}
		}

		private async void PopulateFolders(Odyssey.Controls.BreadcrumbItem item) {
			//Dispatcher.Invoke(DispatcherPriority.Normal,
			//	(Action)(() => {
			if (item.Items.Count > 0) {
				this._IsBreadcrumbBarSelectionChnagedAllowed = true;
				return;
			}
			string trace = item.TraceValue;
			var shellitem = item.DataContext as ShellItem;
			if (trace.Equals(((ShellItem)KnownFolders.Computer).DisplayName)) {
				foreach (ShellItem s in KnownFolders.Computer) {
					item.Items.Add(s);
				}
			}

			this._IsBreadcrumbBarSelectionChnagedAllowed = true;
			//}));
		}

		private void Refresh_Click(object sender, RoutedEventArgs e) {
			this.ShellListView.RefreshContents();
			DoubleAnimation da = new DoubleAnimation(100, new Duration(new TimeSpan(0, 0, 0, 1, 100)));
			da.FillBehavior = FillBehavior.Stop;
			this.bcbc.BeginAnimation(Odyssey.Controls.BreadcrumbBar.ProgressValueProperty, da);
		}

		private void bcbc_Loaded(object sender, RoutedEventArgs e) {
			this.bcbc.Root = ((ShellItem)KnownFolders.Desktop);
			foreach (ShellItem item in ((ShellItem)KnownFolders.Desktop).Where(w => w.IsFolder)) {
				this.bcbc.RootItem.Items.Add(item);
			}
			this.bcbc.UpdateLayout();
			this.bcbc.Path = this.ShellListView.CurrentFolder.ParsingName;
			this.bcbc.OnEditModeToggle += bcbc_OnEditModeToggle;
		}

		void bcbc_OnEditModeToggle(object sender, Odyssey.Controls.EditModeToggleEventArgs e) {
			if (!e.IsExit) {
				this.ShellListView.IsFocusAllowed = false;
				this.bcbc.Focus();
			}
			else {
				this.ShellListView.IsFocusAllowed = true;
			}
		}
		bool _IsBreadcrumbBarSelectionChnagedAllowed = false;
		private void bcbc_SelectedChanged(object sender, RoutedEventArgs e) {

		}

		private void bcbc_BreadcrumbItemDropDownOpened(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e) {
			this.ShellListView.IsFocusAllowed = false;
			this.bcbc.Focus();
		}

		private void bcbc_BreadcrumbItemDropDownClosed(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e) {
			this.ShellListView.IsFocusAllowed = true;
		}

		private void path_conversation(object sender, Odyssey.Controls.PathConversionEventArgs e) {
			var newPath = e.DisplayPath;
			if (newPath != null && newPath.StartsWith("%")) {
				newPath = Environment.ExpandEnvironmentVariables(newPath);
			}
			if (e.Mode == Odyssey.Controls.PathConversionEventArgs.ConversionMode.EditToDisplay && _IsBreadcrumbBarSelectionChnagedAllowed) {
				Int64 pidl;
				bool isValidPidl = Int64.TryParse(newPath.ToShellParsingName().TrimEnd(Char.Parse(@"\")), out pidl);
				ShellItem item = null;
				try {
					if (isValidPidl) {
						item = new ShellItem((IntPtr)pidl);
					}
					else {
						item = new ShellItem(newPath.ToShellParsingName());
					}
					if (item != this.ShellListView.CurrentFolder) {
						//this.ShellListView.SaveSettingsToDatabase(this.ShellListView.CurrentFolder);
						//this.ShellListView.CurrentFolder = item;
						this.ShellListView.Navigate(item, false, true);
					}
					//}
				}
				catch { }
			}
		}

		private void path_changed(object sender, RoutedPropertyChangedEventArgs<string> e) {

		}
		private void edtSearchBox_Loaded(object sender, RoutedEventArgs e) {
			this.edtSearchBox.RequestCancel += edtSearchBox_RequestCancel;
		}

		void edtSearchBox_RequestCancel(object sender, EventArgs e) {
			//this.ShellListView.Cancel = true;
		}

		private void btnOpenWith_DropDownOpened(object sender, EventArgs e) {
			//this.ShellListView.GetFirstSelectedItem().GetAssocList();
			//ShellContextMenu mnu = new ShellContextMenu(this.ShellListView, this.ShellListView.GetFirstSelectedItem(), 1);
			//var controlPos = btnNewItem.TransformToAncestor(Application.Current.MainWindow)
			//								.Transform(new System.Windows.Point(0, 0));
			//var tempPoint = PointToScreen(new System.Windows.Point(controlPos.X, controlPos.Y));
			//mnu.ShowContextMenu(new System.Drawing.Point((int)tempPoint.X, (int)tempPoint.Y + (int)btnOpenWith.ActualHeight), 1);
			//btnOpenWith.IsDropDownOpen = false;
		}

	}
}