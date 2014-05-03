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
	//TODO: Fix Tab History/Navigation issue on new tabs
	partial class MainWindow {
		//private ClosableTabItem CreateNewClosableTabItem(ShellItem DefPath, bool IsNavigate) {
		//	//TODO: figure out what to do with Cloning a tab!
		//	ClosableTabItem newt = new ClosableTabItem();

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
		// DefPath.ParsingName; newt.IsNavigate = IsNavigate; newt.Index = tabControl1.Items.Count;
		// newt.AllowDrop = true; newt.log.CurrentLocation = DefPath;

		// newt.CloseTab += new RoutedEventHandler(newt_CloseTab); newt.DragEnter += new
		// DragEventHandler(newt_DragEnter); newt.DragOver += new DragEventHandler(newt_DragOver);
		// newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove); newt.Drop += new
		// DragEventHandler(newt_Drop); newt.TabSelected += newt_TabSelected;
		// tabControl1.Items.Add(newt); LastTabIndex = tabControl1.SelectedIndex;

		// tabControl1.SelectedIndex = tabControl1.Items.Count - 1; tabControl1.SelectedItem =
		// tabControl1.Items[tabControl1.Items.Count - 1];

		// ConstructMoveToCopyToMenu(); NavigateAfterTabChange();

		//	return newt;
		//}


		#region Tab Closers

		private void CloseTab(ClosableTabItem thetab, bool allowreopening = true) {
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

		private void CloseAllTabs(bool CloseFirstTab) {
			foreach (ClosableTabItem tab in tabControl1.Items.OfType<ClosableTabItem>().ToArray()) {
				CloseTab(tab);
			}
		}

		[Obsolete("Consider Inlining")]
		private void CloseAllTabsButThis(ClosableTabItem tabitem) {
			foreach (ClosableTabItem tab in tabControl1.Items.OfType<ClosableTabItem>().ToArray()) {
				if (tab != tabitem) CloseTab(tab);
			}

			ConstructMoveToCopyToMenu();
		}

		#endregion


		#region Tab Changers

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

		private void ChangeTab(object sender, ExecutedRoutedEventArgs e) {
			t.Stop();
			//SelectTab(tabControl1.SelectedIndex + 1);
			int selIndex = tabControl1.SelectedIndex == tabControl1.Items.Count - 1 ? 0 : tabControl1.SelectedIndex + 1;
			tabControl1.SelectedItem = tabControl1.Items[selIndex];
			NavigateAfterTabChange();
		}

		#endregion


		#region Tab Creators

		private ClosableTabItem CreateNewClosableTabItem(ShellItem DefPath, bool IsNavigate) {
			//TODO: figure out what to do with Cloning a tab!
			ClosableTabItem newt = new ClosableTabItem();
			CreateTabbarRKMenu(newt);

			DefPath.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);
			DefPath.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			newt.ShellObject = DefPath;

			newt.Header = DefPath.GetDisplayName(SIGDN.NORMALDISPLAY);
			newt.TabIcon = DefPath.Thumbnail.BitmapSource;
			newt.ToolTip = DefPath.ParsingName;
			newt.IsNavigate = IsNavigate;
			newt.Index = tabControl1.Items.Count;
			newt.AllowDrop = true;

			newt.CloseTab += new RoutedEventHandler(newt_CloseTab);
			newt.DragEnter += new DragEventHandler(newt_DragEnter);
			newt.DragOver += new DragEventHandler(newt_DragOver);
			newt.PreviewMouseMove += new MouseEventHandler(newt_PreviewMouseMove);
			newt.Drop += new DragEventHandler(newt_Drop);

			newt.TabSelected += newt_TabSelected;

			tabControl1.Items.Add(newt);
			LastTabIndex = tabControl1.SelectedIndex;
			newt.log.CurrentLocation = DefPath;

			if (IsNavigate) {
				tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
				tabControl1.SelectedItem = tabControl1.Items[tabControl1.Items.Count - 1];
				NavigateAfterTabChange();
			}

			ConstructMoveToCopyToMenu();

			return newt;
		}

		/// <summary> Re-opens a previously closed tab using that tab's navigation log data. </summary>
		/// <param name="log"> The navigation log data from the previously closed tab. </param>
		public void ReOpenTab(NavigationLog log) {
			var Tab = CreateNewClosableTabItem(log.CurrentLocation, false);
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
			if (StartUpLocation.StartsWith("::") && StartUpLocation.IndexOf(@"\") == -1)
				DefPath = new ShellItem("shell:" + StartUpLocation);
			else
				try {
					DefPath = new ShellItem(StartUpLocation);
				}
				catch {
					DefPath = (ShellItem)KnownFolders.Libraries;
				}

			CreateNewClosableTabItem(DefPath, IsNavigate);

			//tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
			//tabControl1.SelectedItem = tabControl1.Items[tabControl1.Items.Count - 1];

			//ConstructMoveToCopyToMenu();
			//NavigateAfterTabChange();
		}

		public void NewTab(ShellItem location, bool IsNavigate = false) {
			CreateNewClosableTabItem(location, IsNavigate);

			//if (IsNavigate) {
			//	tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
			//	tabControl1.SelectedItem = tabControl1.Items[tabControl1.Items.Count - 1];
			//	NavigateAfterTabChange();
			//}

			//ConstructMoveToCopyToMenu();
		}

		public ClosableTabItem NewTab(string Location, bool IsNavigate = false) {
			return CreateNewClosableTabItem(new ShellItem(Location), IsNavigate);

			//if (IsNavigate) {
			//	tabControl1.SelectedIndex = tabControl1.Items.Count - 1;
			//	tabControl1.SelectedItem = tabControl1.Items[tabControl1.Items.Count - 1];
			//}
			//else {
			//	newt.log.CurrentLocation = DefPath;
			//}

			//ConstructMoveToCopyToMenu();
			//return newt;
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

		#endregion


		[Obsolete("Try to inline this", false)]
		private void CreateTabbarRKMenu(ClosableTabItem tabitem) {
			tabitem.mnu = new ContextMenu();

			Action<string, RoutedEventHandler> Worker = (x, y) => {
				MenuItem Item = new MenuItem();
				Item.Header = x;
				Item.Tag = tabitem;
				Item.Click += y;
				tabitem.mnu.Items.Add(Item);
			};

			Worker("Close current tab", new RoutedEventHandler(miclosecurrentr_Click));
			Worker("Close all tab", new RoutedEventHandler(miclosealltab_Click));
			Worker("Close all other tab", new RoutedEventHandler(miclosealltabbd_Click));
			tabitem.mnu.Items.Add(new Separator());
			Worker("New tab", new RoutedEventHandler(minewtabr_Click));
			Worker("Clone tab", new RoutedEventHandler(miclonecurrentr_Click));
			tabitem.mnu.Items.Add(new Separator());

			MenuItem miundocloser = new MenuItem();
			miundocloser.Header = "Undo close tab";
			miundocloser.IsEnabled = btnUndoClose.IsEnabled;
			miundocloser.Tag = "UCTI";
			miundocloser.Click += new RoutedEventHandler(miundocloser_Click);
			tabitem.mnu.Items.Add(miundocloser);

			tabitem.mnu.Items.Add(new Separator());
			Worker("Open in new window", new RoutedEventHandler(miopeninnew_Click));

			/*
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
			*/
		}

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

			/*
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
			*/

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

				/*
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
				*/
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

	}
}