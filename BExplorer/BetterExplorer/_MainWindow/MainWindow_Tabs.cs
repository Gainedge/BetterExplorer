using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using MenuItem = Fluent.MenuItem;

namespace BetterExplorer {

	//TODO: Find a way to move CloseTab(...) into TabControl
	partial class MainWindow {
		private void InitializeInitialTabs() {
			tcMain_Setup(null, null);

			var InitialTabs = Utilities.GetRegistryValue("OpenedTabs", "").ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (InitialTabs.Length == 0 || !IsrestoreTabs) {
				var sho = new ShellItem(tcMain.StartUpLocation.ToShellParsingName());

				if (tcMain.Items.OfType<Wpf.Controls.TabItem>().Any())
					//ShellListView.Navigate(sho);
					NavigationController(sho);
				else
					tcMain.NewTab(sho, true);

				/*
				if (tcMain.Items.OfType<Wpf.Controls.TabItem>().Count() == 0)
					tcMain.NewTab(sho, true);
				else
					ShellListView.Navigate(sho);
				*/
			}
			if (IsrestoreTabs) {
				isOnLoad = true;
				int i = 0;
				foreach (string str in InitialTabs) {
					try {
						i++;
						if (str.ToLowerInvariant() == "::{22877a6d-37a1-461a-91b0-dbda5aaebc99}") {
							/*
							if (i == InitialTabs.Length) {
								//tcMain.SelectedIndex = InitialTabs.Length - 2;
							}
							*/
							continue;
						}

						tcMain.NewTab(ShellItem.ToShellParsingName(str), i == InitialTabs.Length);
						if (i == InitialTabs.Count()) {
							ShellItem sho = new ShellItem(str.ToShellParsingName());
							//ShellListView.Navigate(sho);
							NavigationController(sho);
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
					tcMain.NewTab();

					string idk = tcMain.StartUpLocation.StartsWith("::") ? tcMain.StartUpLocation.ToShellParsingName() : tcMain.StartUpLocation.Replace("\"", "");
					NavigationController(new ShellItem(idk));

					/*
					if (tcMain.StartUpLocation.StartsWith("::"))
						ShellListView.Navigate(new ShellItem(tcMain.StartUpLocation.ToShellParsingName()));
					else
						ShellListView.Navigate(new ShellItem(tcMain.StartUpLocation.Replace("\"", "")));
					*/
					(tcMain.SelectedItem as Wpf.Controls.TabItem).ShellObject = ShellListView.CurrentFolder;
					(tcMain.SelectedItem as Wpf.Controls.TabItem).ToolTip = ShellListView.CurrentFolder.ParsingName;
				}

				isOnLoad = false;
			}
			//breadcrumbBarControl1.ExitEditMode_IfNeeded(true);

			//if (tcMain.Items.Count == 1)
			//	tcMain.SelectedIndex = 0;

			//ShellVView.Visibility = System.Windows.Visibility.Hidden;
		}


		private void CloseTab(Wpf.Controls.TabItem thetab, bool allowreopening = true) {
			if (tcMain.SelectedIndex == 0 && tcMain.Items.Count == 1) {
				//if (this.IsCloseLastTabCloseApp) {
				if (chkIsLastTabCloseApp.IsChecked.Value) {
					Close();
				}
				else {
					//ShellListView.Navigate(new ShellItem(tcMain.StartUpLocation));
					NavigationController(new ShellItem(tcMain.StartUpLocation));
				}
				return;
			}

			tcMain.RemoveTabItem(thetab, allowreopening);

			ConstructMoveToCopyToMenu();

			SelectTab(tcMain.SelectedItem as Wpf.Controls.TabItem);
		}

		private void ConstructMoveToCopyToMenu() {
			btnMoveto.Items.Clear();
			btnCopyto.Items.Clear();

			ShellItem sod = (ShellItem)KnownFolders.Desktop;
			sod.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			sod.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

			var OtherLocationMove = Utilities.Build_MenuItem(FindResource("miOtherDestCP"), onClick: new RoutedEventHandler(btnmtOther_Click));
			var OtherLocationCopy = Utilities.Build_MenuItem(FindResource("miOtherDestCP"), onClick: new RoutedEventHandler(btnctOther_Click));
			var mimDesktop = Utilities.Build_MenuItem(FindResource("btnctDesktopCP"), icon: sod.Thumbnail.BitmapSource, onClick: new RoutedEventHandler(btnmtDesktop_Click));
			var micDesktop = Utilities.Build_MenuItem(FindResource("btnctDesktopCP"), icon: sod.Thumbnail.BitmapSource, onClick: new RoutedEventHandler(btnctDesktop_Click));




			MenuItem mimDocuments = new MenuItem(), micDocuments = new MenuItem();
			try {
				ShellItem sodc = (ShellItem)KnownFolders.Documents;
				sodc.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
				sodc.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

				mimDocuments = Utilities.Build_MenuItem(FindResource("btnctDocumentsCP"), icon: sodc.Thumbnail.BitmapSource, onClick: new RoutedEventHandler(btnmtDocuments_Click));
				micDocuments = Utilities.Build_MenuItem(FindResource("btnctDocumentsCP"), icon: sodc.Thumbnail.BitmapSource, onClick: new RoutedEventHandler(btnctDocuments_Click));
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

				mimDownloads = Utilities.Build_MenuItem(FindResource("btnctDownloadsCP"), icon: sodd.Thumbnail.BitmapSource, onClick: new RoutedEventHandler(btnmtDounloads_Click));
				micDownloads = Utilities.Build_MenuItem(FindResource("btnctDownloadsCP"), icon: sodd.Thumbnail.BitmapSource, onClick: new RoutedEventHandler(btnctDounloads_Click));
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

				foreach (object mii in btnCopyto.Items.OfType<MenuItem>().Where(mii => (mii as MenuItem).Tag != null)) {
					if (((mii as MenuItem).Tag as ShellItem) == item.ShellObject) {
						IsAdditem = false;
						break;
					}
					/*
					foreach (object mii in btnCopyto.Items) {
						if (mii is MenuItem) {
							if ((mii as MenuItem).Tag != null) {
								if (((mii as MenuItem).Tag as ShellItem) == item.ShellObject) {
									IsAdditem = false;
									break;
								}
							}
						}
					 */
				}

				if (IsAdditem && item.ShellObject.IsFileSystem) {
					try {
						ShellItem so = new ShellItem(item.ShellObject.ParsingName);
						so.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
						so.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

						btnMoveto.Items.Add(Utilities.Build_MenuItem(item.ShellObject.GetDisplayName(SIGDN.NORMALDISPLAY), item.ShellObject,
																	 so.Thumbnail.BitmapSource, onClick: new RoutedEventHandler(mim_Click)));

						btnCopyto.Items.Add(Utilities.Build_MenuItem(item.ShellObject.GetDisplayName(SIGDN.NORMALDISPLAY), item.ShellObject, so.Thumbnail.BitmapSource));
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

		[Obsolete("Remove this or move it into TabControls!!!!!!!!!!!")]
		private System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();

		private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.RemovedItems.Count > 0) {
				var tab = e.RemovedItems[0] as Wpf.Controls.TabItem;

				if (tab != null && this.ShellListView.GetSelectedCount() > 0) {
					tab.SelectedItems = this.ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
				}
			}

			if (e.AddedItems.Count == 0) return;
			SelectTab(e.AddedItems[0] as Wpf.Controls.TabItem);

			//btnUndoClose
			btnUndoClose.Items.Clear();
			foreach (var item in tcMain.ReopenableTabs) {
				btnUndoClose.Items.Add(item.CurrentLocation);
			}
			this.ShellListView.Focus();

		}

		private void SelectTab(Wpf.Controls.TabItem tab) {
			if (tab == null) return;
			//tcMain.isGoingBackOrForward = tab.log.HistoryItemsList.Any();
			try {
				if (!Keyboard.IsKeyDown(Key.Tab)) {
					if (tab.ShellObject != this.ShellListView.CurrentFolder || tab.ShellObject.IsSearchFolder) {
						if (tab.log.HistoryItemsList.Any())
							isGoingBackOrForward_Test(tab.ShellObject);
						else
							NavigationController(tab.ShellObject);
					}
				}
				else {
					t.Interval = 500;
					t.Tag = tab.ShellObject;
					t.Tick += new EventHandler(t_Tick);
					t.Start();
				}
				//}
			}
			catch (StackOverflowException) {
			}
		}

		public List<string> LoadListOfTabListFiles() {
			var o = new List<string>();

			if (System.IO.Directory.Exists(sstdir)) {
				foreach (string item in System.IO.Directory.GetFiles(sstdir)) {
					ShellItem obj = new ShellItem(item);
					o.Add(Utilities.RemoveExtensionsFromFile(obj.GetDisplayName(SIGDN.NORMALDISPLAY), Utilities.GetExtension(item)));
				}
			}
			return o;
		}
	}
}