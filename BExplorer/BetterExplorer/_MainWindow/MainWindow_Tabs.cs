using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using ContextMenu = Fluent.ContextMenu;
using MenuItem = Fluent.MenuItem;

namespace BetterExplorer {

	//TODO: Fix Folder History/Navigation issue on new tabs
	//TODO: Fix Context Menu Item [Open in new window]
	partial class MainWindow {
		//private Wpf.Controls.TabItem CreateNewWpf.Controls.TabItem(ShellItem DefPath, bool IsNavigate) {
		//	//TODO: figure out what to do with Cloning a tab!
		//	Wpf.Controls.TabItem newt = new Wpf.Controls.TabItem();

		// #region CreateTabbarRKMenu newt.mnu = new ContextMenu();

		// Action<string, RoutedEventHandler> Worker = (x, y) => { MenuItem Item = new MenuItem();
		// Item.Header = x; Item.Tag = newt; Item.Click += y; newt.mnu.Items.Add(Item); };

		// Worker("Close current tab", new RoutedEventHandler(miclosecurrentr_Click)); Worker("Close
		// all tab", new RoutedEventHandler(miclosealltab_Click)); Worker("Close all other tab", new
		// RoutedEventHandler(miclosealltabbd_Click)); newt.mnu.Items.Add(new Separator());
		// Worker("New tab", new RoutedEventHandler(minewtabr_Click)); Worker("Clone tab", new
		// RoutedEventHandler(miclonecurrentr_Click)); newt.mnu.Items.Add(new Separator());

		// MenuItem miundocloser = new MenuItem(); miundocloser.Header = "Undo close tab";
		// miundocloser.IsEnabled = btnUndoClose.IsEnabled; miundocloser.Tag = "UCTI";
		// miundocloser.Click += new RoutedEventHandler(miundocloser_Click); newt.mnu.Items.Add(miundocloser);

		// newt.mnu.Items.Add(new Separator()); Worker("Open in new window", new RoutedEventHandler(miopeninnew_Click));

		// #endregion

		// DefPath.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
		// DefPath.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
		// newt.PreviewMouseMove += newt_PreviewMouseMove; newt.Header =
		// DefPath.GetDisplayName(SIGDN.NORMALDISPLAY); newt.TabIcon =
		// DefPath.Thumbnail.BitmapSource; newt.ShellObject = DefPath; newt.ToolTip =
		// DefPath.ParsingName; newt.IsNavigate = IsNavigate; newt.Index = tcMain.Items.Count;
		// newt.AllowDrop = true; newt.log.CurrentLocation = DefPath;

		// newt.CloseTab += new RoutedEventHandler(newt_CloseTab); newt.DragEnter += new
		// DragEventHandler(newt_DragEnter); newt.DragOver += new DragEventHandler(newt_DragOver);
		// newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove); newt.Drop += new
		// DragEventHandler(newt_Drop); newt.TabSelected += newt_TabSelected;
		// tcMain.Items.Add(newt); LastTabIndex = tcMain.SelectedIndex;

		// tcMain.SelectedIndex = tcMain.Items.Count - 1; tcMain.SelectedItem =
		// tcMain.Items[tcMain.Items.Count - 1];

		// ConstructMoveToCopyToMenu(); NavigateAfterTabChange();

		//	return newt;
		//}


		#region Tab Closers

		private void CloseTab(Wpf.Controls.TabItem thetab, bool allowreopening = true) {
			if (tcMain.SelectedIndex == 0 && tcMain.Items.Count == 1) {
				if (this.IsCloseLastTabCloseApp) {
					Close();
				}
				else {
					ShellListView.Navigate(new ShellItem(this.StartUpLocation));
				}

				return;
			}

			tcMain.RemoveTabItem(thetab);
			ConstructMoveToCopyToMenu();

			if (allowreopening) {
				reopenabletabs.Add(thetab.log);
				btnUndoClose.IsEnabled = true;
				foreach (Wpf.Controls.TabItem item in this.tcMain.Items) {
					foreach (FrameworkElement m in item.mnu.Items) {
						if (m.Tag != null) {
							if (m.Tag.ToString() == "UCTI")
								(m as MenuItem).IsEnabled = true;
						}
					}
				}
			}

			SelectTab(tcMain.Items[tcMain.SelectedIndex] as Wpf.Controls.TabItem);
			//'btnTabCloseC.IsEnabled = tcMain.Items.Count > 1;
			//'there's a bug that has this enabled when there's only one tab open, but why disable it
			//'if it never crashes the program? Closing the last tab simply closes the program, so I
			//'thought, what the heck... let's just keep it enabled. :) -JaykeBird
		}
		#endregion


		#region Tab Changers
		private void ChangeTab(object sender, ExecutedRoutedEventArgs e) {
			t.Stop();
			int selIndex = tcMain.SelectedIndex == tcMain.Items.Count - 1 ? 0 : tcMain.SelectedIndex + 1;
			tcMain.SelectedItem = tcMain.Items[selIndex];
		}

		#endregion


		#region Tab Creators

		private Wpf.Controls.TabItem CreateNewTab(ShellItem DefPath, bool IsNavigate) {
			//TODO: figure out what to do with Cloning a tab!
			Wpf.Controls.TabItem newt = new Wpf.Controls.TabItem();

			DefPath.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
			DefPath.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			newt.ShellObject = DefPath;

			newt.Header = DefPath.GetDisplayName(SIGDN.NORMALDISPLAY);
			newt.Icon = DefPath.Thumbnail.BitmapSource;
			newt.ToolTip = DefPath.ParsingName;
			//newt.IsNavigate = IsNavigate;
			//newt.Index = tcMain.Items.Count;
			newt.AllowDrop = true;

			//newt.CloseTab += new RoutedEventHandler(newt_CloseTab);
			newt.DragEnter += new DragEventHandler(newt_DragEnter);
			newt.DragOver += new DragEventHandler(newt_DragOver);
			newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove);
			newt.Drop += new DragEventHandler(newt_Drop);

			tcMain.Items.Add(newt);
			//LastTabIndex = tcMain.SelectedIndex;
			//newt.log.CurrentLocation = DefPath;

			if (IsNavigate) {
				tcMain.SelectedIndex = tcMain.Items.Count - 1;
				tcMain.SelectedItem = tcMain.Items[tcMain.Items.Count - 1];
			}

			ConstructMoveToCopyToMenu();

			return newt;
		}

		/// <summary> Re-opens a previously closed tab using that tab's navigation log data. </summary>
		/// <param name="log"> The navigation log data from the previously closed tab. </param>
		public void ReOpenTab(NavigationLog log) {
			var Tab = CreateNewTab(log.CurrentLocation, false);
			Tab.log.ImportData(log);

			//newt.CloseTab += newt_CloseTab;
			//newt.DragEnter += newt_DragEnter;
			//newt.DragOver += newt_DragOver;
			//newt.PreviewMouseMove += newt_PreviewMouseMove;
			//newt.Drop += newt_Drop;

			//newt.log.ImportData(log);
		}

		public void NewTab(bool IsNavigate = true) {
			ShellItem DefPath;
			if (StartUpLocation.StartsWith("::") && !StartUpLocation.Contains(@"\"))
				DefPath = new ShellItem("shell:" + StartUpLocation);
			else
				try {
					DefPath = new ShellItem(StartUpLocation);
				}
				catch {
					DefPath = (ShellItem)KnownFolders.Libraries;
				}

			CreateNewTab(DefPath, IsNavigate);
		}

		public void NewTab(ShellItem location, bool IsNavigate = false) {
			CreateNewTab(location, IsNavigate);
		}

		public Wpf.Controls.TabItem NewTab(string Location, bool IsNavigate = false) {
			return CreateNewTab(new ShellItem(Location), IsNavigate);
		}

		public void CloneTab(Wpf.Controls.TabItem CurTab) {
			tcMain.CloneTabItem(CurTab);
			ConstructMoveToCopyToMenu();
		}

		#endregion


		#region OnStartup

		private void InitializeInitialTabs() {
			var InitialTabs = Utilities.GetRegistryValue("OpenedTabs", "").ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (InitialTabs.Length == 0 || !IsrestoreTabs) {
				ShellItem sho = new ShellItem(StartUpLocation.ToShellParsingName());
				if (tcMain.Items.OfType<Wpf.Controls.TabItem>().Count() == 0)
					NewTab(sho, true);
				else
					ShellListView.Navigate(sho);
			}
			if (IsrestoreTabs) {
				isOnLoad = true;
				int i = 0;
				foreach (string str in InitialTabs) {
					try {
						i++;
						if (str.ToLowerInvariant() == "::{22877a6d-37a1-461a-91b0-dbda5aaebc99}") {
							if (i == InitialTabs.Length) {
								tcMain.SelectedIndex = InitialTabs.Length - 2;
							}

							continue;
						}

						NewTab(str.ToShellParsingName(), i == InitialTabs.Length);
						if (i == InitialTabs.Count()) {
							ShellItem sho = new ShellItem(str.ToShellParsingName());
							ShellListView.Navigate(sho);
							(tcMain.SelectedItem as Wpf.Controls.TabItem).ShellObject = sho;
							(tcMain.SelectedItem as Wpf.Controls.TabItem).ToolTip = sho.ParsingName;
						}
					}
					catch {
						//AddToLog(String.Format("Unable to load {0} into a tab!", str));
						MessageBox.Show("BetterExplorer is unable to load one of the tabs from your last session. Your other tabs are perfectly okay though! \r\n\r\nThis location was unable to be loaded: " + str, "Unable to Create New Tab", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}

				if (tcMain.Items.Count == 0) {
					NewTab();

					if (StartUpLocation.StartsWith("::"))
						ShellListView.Navigate(new ShellItem(StartUpLocation.ToShellParsingName()));
					else
						ShellListView.Navigate(new ShellItem(StartUpLocation.Replace("\"", "")));

					(tcMain.SelectedItem as Wpf.Controls.TabItem).ShellObject = ShellListView.CurrentFolder;
					(tcMain.SelectedItem as Wpf.Controls.TabItem).ToolTip = ShellListView.CurrentFolder.ParsingName;
				}

				isOnLoad = false;
			}
			breadcrumbBarControl1.ExitEditMode_IfNeeded(true);

			if (tcMain.Items.Count == 1)
				tcMain.SelectedIndex = 0;

			//ShellVView.Visibility = System.Windows.Visibility.Hidden;
		}

		#endregion
		private void ConstructMoveToCopyToMenu() {
			//TODO: Find the parts that will cause the errors and put the try catch around them ONLY or fix the issue!!!

			Func<string, RoutedEventHandler, BitmapSource, MenuItem> Builder = (ResourceName, EventHandler, Bitmap) => {
				MenuItem Item = new MenuItem();
				Item.Focusable = false;
				Item.Header = FindResource(ResourceName);

				if (EventHandler != null) Item.Click += EventHandler;
				if (Bitmap != null) Item.Icon = Bitmap;
				return Item;
			};

			btnMoveto.Items.Clear();
			btnCopyto.Items.Clear();

			ShellItem sod = (ShellItem)KnownFolders.Desktop;
			sod.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			sod.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

			var OtherLocationMove = Builder("miOtherDestCP", new RoutedEventHandler(btnmtOther_Click), null);
			var OtherLocationCopy = Builder("miOtherDestCP", new RoutedEventHandler(btnctOther_Click), null);
			var mimDesktop = Builder("btnctDesktopCP", new RoutedEventHandler(btnmtDesktop_Click), sod.Thumbnail.BitmapSource);
			var micDesktop = Builder("btnctDesktopCP", new RoutedEventHandler(btnctDesktop_Click), sod.Thumbnail.BitmapSource);

			MenuItem mimDocuments = new MenuItem(), micDocuments = new MenuItem();
			try {
				ShellItem sodc = (ShellItem)KnownFolders.Documents;
				sodc.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				sodc.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

				mimDocuments = Builder("btnctDocumentsCP", new RoutedEventHandler(btnmtDocuments_Click), sodc.Thumbnail.BitmapSource);
				micDocuments = Builder("btnctDocumentsCP", new RoutedEventHandler(btnctDocuments_Click), sodc.Thumbnail.BitmapSource);
			}
			catch (Exception) {
				mimDocuments = null;
				micDocuments = null;
				//catch the exception in case the user deleted that basic folder somehow
			}

			MenuItem mimDownloads = new MenuItem(), micDownloads = new MenuItem();
			try {
				ShellItem sodd = (ShellItem)KnownFolders.Downloads;
				sodd.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				sodd.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

				mimDownloads = Builder("btnctDownloadsCP", new RoutedEventHandler(btnmtDounloads_Click), sodd.Thumbnail.BitmapSource);
				micDownloads = Builder("btnctDownloadsCP", new RoutedEventHandler(btnctDounloads_Click), sodd.Thumbnail.BitmapSource);
			}
			catch (Exception) {
				micDownloads = null;
				mimDownloads = null;
				//catch the exception in case the user deleted that basic folder somehow
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

			foreach (var item in tcMain.Items.OfType<Wpf.Controls.TabItem>().ToList()) {
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
						ShellItem so = new ShellItem(item.ShellObject.ParsingName);
						so.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
						so.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

						MenuItem mim = new MenuItem();
						mim.Header = item.ShellObject.GetDisplayName(SIGDN.NORMALDISPLAY);
						mim.Focusable = false;
						mim.Tag = item.ShellObject;
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



		System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
		private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.RemovedItems.Count > 0) {
				var tab = e.RemovedItems[0] as Wpf.Controls.TabItem;

				if (tab != null && this.ShellListView.GetSelectedCount() > 0) {
					tab.SelectedItems = this.ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
				}
			}
			//if (e.AddedItems.Count > 0 && (e.AddedItems[0] as Wpf.Controls.TabItem).Index == tcMain.Items.Count - 1) {
			//	tcMain.Items.OfType<Wpf.Controls.TabItem>().Last().BringIntoView();
			//}
			if (e.AddedItems.Count == 0) return;
			SelectTab(e.AddedItems[0] as Wpf.Controls.TabItem);
		}

		private void SelectTab(Wpf.Controls.TabItem Tab) {
			if (Tab == null) return;
			try {
				isGoingBackOrForward = Tab.log.HistoryItemsList.Count != 0;
				//BeforeLastTabIndex = LastTabIndex;
				if (Tab.ShellObject != ShellListView.CurrentFolder) {
					if (!Keyboard.IsKeyDown(Key.Tab)) {
						ShellListView.Navigate(Tab.ShellObject);
					}
					else {
						t.Interval = 500;
						t.Tag = Tab.ShellObject;
						t.Tick += new EventHandler(t_Tick);
						t.Start();
					}
				}
			}
			catch (StackOverflowException) {
			}
		}

	}
}


