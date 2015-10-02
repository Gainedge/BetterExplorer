using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using MenuItem = Fluent.MenuItem;

namespace BetterExplorer
{

    partial class MainWindow
    {

        private void InitializeInitialTabs()
        {
            var InitialTabs = Utilities.GetRegistryValue("OpenedTabs", "").ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (InitialTabs.Length == 0 || !_IsrestoreTabs)
            {
                var sho = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, tcMain.StartUpLocation.ToShellParsingName());

                if (tcMain.Items.OfType<Wpf.Controls.TabItem>().Any())
                    NavigationController(sho);
                else
                {
                    var tab = tcMain.NewTab(sho, true);
                    this.SelectTab(tab);
                }
            }
            if (_IsrestoreTabs)
            {
                isOnLoad = true;
                int i = 0;
                foreach (string str in InitialTabs)
                {
                    try
                    {
                        i++;
                        if (str.ToLowerInvariant() == "::{22877a6d-37a1-461a-91b0-dbda5aaebc99}")
                        {
                            continue;
                        }
                        var tab = tcMain.NewTab(FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, str.ToShellParsingName()), i == InitialTabs.Length);
                        if (i == InitialTabs.Length)
                            this.SelectTab(tab);
                    }
                    catch
                    {
                        //AddToLog(String.Format("Unable to load {0} into a tab!", str));
                        MessageBox.Show("BetterExplorer is unable to load one of the tabs from your last session. Your other tabs are perfectly okay though! \r\n\r\nThis location was unable to be loaded: " + str, "Unable to Create New Tab", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                if (tcMain.Items.Count == 0)
                {
                    tcMain.NewTab();

                    string idk = tcMain.StartUpLocation.StartsWith("::") ? tcMain.StartUpLocation.ToShellParsingName() : tcMain.StartUpLocation.Replace("\"", "");
                    NavigationController(FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, idk));
                    (tcMain.SelectedItem as Wpf.Controls.TabItem).ShellObject = _ShellListView.CurrentFolder;
                    (tcMain.SelectedItem as Wpf.Controls.TabItem).ToolTip = _ShellListView.CurrentFolder.ParsingName.Replace("%20", " ").Replace("%3A", ":").Replace("%5C", @"\");
                }

                isOnLoad = false;
            }
        }

        private void SelectTab(Wpf.Controls.TabItem tab)
        {
            if (tab == null) return;
            if (!tab.ShellObject.Equals(this._ShellListView.CurrentFolder) || tab.ShellObject.IsSearchFolder)
            {
                tcMain.isGoingBackOrForward = true;
                NavigationController(tab.ShellObject);
                var selectedItem = tab;
                selectedItem.Header = tab.ShellObject.DisplayName;
                var bmpSource = tab.ShellObject.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
                selectedItem.Icon = bmpSource;
                selectedItem.ShellObject = tab.ShellObject;
                if (selectedItem != null)
                {
                    var selectedPaths = selectedItem.SelectedItems;
                    if (selectedPaths != null && selectedPaths.Any())
                    {
                        foreach (var path in selectedPaths.ToArray())
                        {
                            var sho = this._ShellListView.Items.Where(w => w.ParsingName == path).SingleOrDefault();
                            if (sho != null)
                            {
                                var index = this._ShellListView.ItemsHashed[sho.GetUniqueID()];
                                this._ShellListView.SelectItemByIndex(60, true);
                                selectedPaths.Remove(path);
                            }
                        }
                    }
                    else
                    {
                        //this._ShellListView.ScrollToTop();
                    }
                }
            }
        }

        private void ConstructMoveToCopyToMenu()
        {
            btnMoveto.Items.Clear();
            btnCopyto.Items.Clear();

            var sod = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, ((ShellItem)KnownFolders.Desktop).Pidl);
            var bmpSourceDesktop = sod.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

            var OtherLocationMove = Utilities.Build_MenuItem(FindResource("miOtherDestCP"), onClick: new RoutedEventHandler(btnmtOther_Click));
            var OtherLocationCopy = Utilities.Build_MenuItem(FindResource("miOtherDestCP"), onClick: new RoutedEventHandler(btnctOther_Click));
            var mimDesktop = Utilities.Build_MenuItem(FindResource("btnctDesktopCP"), icon: bmpSourceDesktop, onClick: new RoutedEventHandler(btnmtDesktop_Click));
            var micDesktop = Utilities.Build_MenuItem(FindResource("btnctDesktopCP"), icon: bmpSourceDesktop, onClick: new RoutedEventHandler(btnctDesktop_Click));

            MenuItem mimDocuments = new MenuItem(), micDocuments = new MenuItem();
            try
            {
                var sodc = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, ((ShellItem)KnownFolders.Documents).Pidl);
                var bmpSourceDocuments = sodc.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

                mimDocuments = Utilities.Build_MenuItem(FindResource("btnctDocumentsCP"), icon: bmpSourceDocuments, onClick: new RoutedEventHandler(btnmtDocuments_Click));
                micDocuments = Utilities.Build_MenuItem(FindResource("btnctDocumentsCP"), icon: bmpSourceDocuments, onClick: new RoutedEventHandler(btnctDocuments_Click));
            }
            catch (Exception)
            {
                mimDocuments = null;
                micDocuments = null;
                //catch the exception in case the user deleted that basic folder somehow
            }

            MenuItem mimDownloads = new MenuItem(), micDownloads = new MenuItem();
            try
            {
                var sodd = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, ((ShellItem)KnownFolders.Downloads).Pidl);
                var bmpSourceDownloads = sodd.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

                mimDownloads = Utilities.Build_MenuItem(FindResource("btnctDownloadsCP"), icon: bmpSourceDownloads, onClick: new RoutedEventHandler(btnmtDounloads_Click));
                micDownloads = Utilities.Build_MenuItem(FindResource("btnctDownloadsCP"), icon: bmpSourceDownloads, onClick: new RoutedEventHandler(btnctDounloads_Click));
            }
            catch (Exception)
            {
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

            foreach (var item in tcMain.Items.OfType<Wpf.Controls.TabItem>())
            {
                bool IsAdditem = true;

                foreach (var mii in btnCopyto.Items.OfType<MenuItem>().Where(x => x.Tag != null))
                {
                    if ((mii.Tag as IListItemEx).Equals(item.ShellObject))
                    {
                        IsAdditem = false;
                    }
                }

                if (IsAdditem && item.ShellObject.IsFileSystem)
                {
                    try
                    {
                        var so = item.ShellObject;
                        var bmpSource = so.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
                        btnMoveto.Items.Add(Utilities.Build_MenuItem(item.ShellObject.DisplayName, item.ShellObject,
                                                                     bmpSource, onClick: new RoutedEventHandler(mim_Click)));

                        btnCopyto.Items.Add(Utilities.Build_MenuItem(item.ShellObject.DisplayName, item.ShellObject, bmpSource));
                    }
                    catch
                    {
                        //Do nothing if ShellItem is not available anymore and close the problematic item
                        //tcMain.RemoveTabItem(item);
                    }
                }
            }

            btnMoveto.Items.Add(new Separator());
            btnMoveto.Items.Add(OtherLocationMove);
            btnCopyto.Items.Add(new Separator());
            btnCopyto.Items.Add(OtherLocationCopy);
        }

        Wpf.Controls.TabItem _CurrentlySelectedItem = null;

        [Obsolete("I do not think we need this")]
        private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}