// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow_Tabs.cs" company="Gainedge ORG">
//   Better Explorer 
// </copyright>
// <summary>
//   Defines the MainWindow type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using MenuItem = Fluent.MenuItem;

namespace BetterExplorer {
  using Settings;

  /// <summary>
  /// The main window.
  /// </summary>
  public partial class MainWindow {
    private Wpf.Controls.TabItem _CurrentlySelectedItem = null;

    /// <summary>
    /// Handles initialization for tabs on startup if and only if no startup tab is set.
    /// </summary>
    private void InitializeInitialTabs() {
      var initialTabs = BESettings.OpenedTabs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      if (initialTabs.Length == 0 || !BESettings.IsRestoreTabs) {
        var sho = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, this.tcMain.StartUpLocation.ToShellParsingName());

        if (this.tcMain.Items.OfType<Wpf.Controls.TabItem>().Any()) {
          this.NavigationController(sho);
        }
        else {
          this.SelectTab(this.tcMain.NewTab(sho, true));
        }
      }

      if (!BESettings.IsRestoreTabs) {
        return;
      }

      this.isOnLoad = true;
      var i = 0;
      foreach (String str in initialTabs) {
        try {
          i++;
          if (str.ToLowerInvariant() == "::{22877a6d-37a1-461a-91b0-dbda5aaebc99}") {
            continue;
          }

          var isLastTab = i == initialTabs.Length;
          var tab = this.tcMain.NewTab(FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, str.ToShellParsingName()), isLastTab);
          if (!isLastTab) {
            continue;
          }

          this.SelectTab(tab);
          this.bcbc.SetPathWithoutNavigate(str);
        }
        catch {
          // AddToLog(String.Format("Unable to load {0} into a tab!", str));
          MessageBox.Show("BetterExplorer is unable to load one of the tabs from your last session. Your other tabs are perfectly okay though! \r\n\r\nThis location was unable to be loaded: " + str, "Unable to Create New Tab", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }

      if (this.tcMain.Items.Count == 0) {
        this.tcMain.NewTab();

        var idk = this.tcMain.StartUpLocation.StartsWith("::") ? this.tcMain.StartUpLocation.ToShellParsingName() : this.tcMain.StartUpLocation.Replace("\"", String.Empty);
        this.NavigationController(FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, idk));
        var tabItem = this.tcMain.SelectedItem as Wpf.Controls.TabItem;
        if (tabItem != null) {
          tabItem.ShellObject = this._ShellListView.CurrentFolder;
          tabItem.ToolTip = this._ShellListView.CurrentFolder.ParsingName.Replace("%20", " ").Replace("%3A", ":").Replace("%5C", @"\");
        }
      }

      this.isOnLoad = false;
    }

    /// <summary>
    /// Selects the <paramref name="tab"/> and navigates to it correctly if and only if it is not already selected
    /// </summary>
    /// <param name="tab">The tab you want to select</param>
    private void SelectTab(Wpf.Controls.TabItem tab) {
      if (tab != null && (!tab.ShellObject.Equals(this._ShellListView.CurrentFolder) || tab.ShellObject.IsSearchFolder)) {
        this.tcMain.isGoingBackOrForward = true;
        this.NavigationController(tab.ShellObject);
        var selectedItem = tab; //TODO: Find out if we can replace [selectedItem] with [tab]
        selectedItem.Header = tab.ShellObject.DisplayName;
        selectedItem.Icon = tab.ShellObject.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
        selectedItem.ShellObject = tab.ShellObject;
        var selectedPaths = selectedItem?.SelectedItems;
        if (selectedPaths == null || !selectedPaths.Any()) {
          return;
        }

        foreach (var path in selectedPaths) {
          var sho = this._ShellListView.Items.FirstOrDefault(w => w.ParsingName == path);
          if (sho != null && sho.Equals(this._ShellListView.Items[sho.ItemIndex])) {
            this._ShellListView.SelectItemByIndex(sho.ItemIndex, true);
            selectedPaths.Remove(path);
          }
        }
      }
    }

    /// <summary>
    /// Constructs the copy/move to menu for tabs
    /// </summary>
    private void ConstructMoveToCopyToMenu() {
      this.btnMoveto.Items.Clear();
      this.btnCopyto.Items.Clear();

      var sod = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, ((ShellItem)KnownFolders.Desktop).Pidl);
      var bmpSourceDesktop = sod.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

      var otherLocationMove = Utilities.Build_MenuItem(this.FindResource("miOtherDestCP"), onClick: new RoutedEventHandler(this.btnmtOther_Click));
      var otherLocationCopy = Utilities.Build_MenuItem(this.FindResource("miOtherDestCP"), onClick: new RoutedEventHandler(this.btnctOther_Click));
      var mimDesktop = Utilities.Build_MenuItem(this.FindResource("btnctDesktopCP"), icon: bmpSourceDesktop, onClick: new RoutedEventHandler(this.btnmtDesktop_Click));
      var micDesktop = Utilities.Build_MenuItem(this.FindResource("btnctDesktopCP"), icon: bmpSourceDesktop, onClick: new RoutedEventHandler(this.btnctDesktop_Click));

      MenuItem mimDocuments = new MenuItem(), micDocuments = new MenuItem();
      try {
        var sodc = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, ((ShellItem)KnownFolders.Documents).Pidl);
        var bmpSourceDocuments = sodc.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

        mimDocuments = Utilities.Build_MenuItem(this.FindResource("btnctDocumentsCP"), icon: bmpSourceDocuments, onClick: new RoutedEventHandler(this.btnmtDocuments_Click));
        micDocuments = Utilities.Build_MenuItem(this.FindResource("btnctDocumentsCP"), icon: bmpSourceDocuments, onClick: new RoutedEventHandler(this.btnctDocuments_Click));
      }
      catch (Exception) {
        mimDocuments = null;
        micDocuments = null;

        // catch the exception in case the user deleted that basic folder somehow
      }

      MenuItem mimDownloads = new MenuItem(), micDownloads = new MenuItem();
      try {
        var sodd = FileSystemListItem.ToFileSystemItem(this._ShellListView.LVHandle, ((ShellItem)KnownFolders.Downloads).Pidl);
        var bmpSourceDownloads = sodd.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

        mimDownloads = Utilities.Build_MenuItem(this.FindResource("btnctDownloadsCP"), icon: bmpSourceDownloads, onClick: new RoutedEventHandler(this.btnmtDounloads_Click));
        micDownloads = Utilities.Build_MenuItem(this.FindResource("btnctDownloadsCP"), icon: bmpSourceDownloads, onClick: new RoutedEventHandler(this.btnctDounloads_Click));
      }
      catch (Exception) {
        micDownloads = null;
        mimDownloads = null;

        // catch the exception in case the user deleted that basic folder somehow
      }

      if (mimDocuments != null) {
        this.btnMoveto.Items.Add(mimDocuments);
      }

      if (mimDownloads != null) {
        this.btnMoveto.Items.Add(mimDownloads);
      }

      this.btnMoveto.Items.Add(mimDesktop);
      this.btnMoveto.Items.Add(new Separator());

      if (micDocuments != null) {
        this.btnCopyto.Items.Add(micDocuments);
      }

      if (micDownloads != null) {
        this.btnCopyto.Items.Add(micDownloads);
      }

      this.btnCopyto.Items.Add(micDesktop);
      this.btnCopyto.Items.Add(new Separator());

      foreach (var item in this.tcMain.Items.OfType<Wpf.Controls.TabItem>()) {
        var isAdditem = true;
        item.ShellObject = item.ShellObject.Clone();
        foreach (var mii in this.btnCopyto.Items.OfType<MenuItem>().Where(x => x.Tag != null)) {
          if ((mii.Tag as IListItemEx).Equals(item.ShellObject)) {
            isAdditem = false;
          }
        }

        if (isAdditem && item.ShellObject.IsFileSystem) {
          try {
            var so = item.ShellObject;
            var bmpSource = so.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
            this.btnMoveto.Items.Add(Utilities.Build_MenuItem(item.ShellObject.DisplayName, item.ShellObject, bmpSource, onClick: new RoutedEventHandler(this.mim_Click)));
            this.btnCopyto.Items.Add(Utilities.Build_MenuItem(item.ShellObject.DisplayName, item.ShellObject, bmpSource, onClick: new RoutedEventHandler(this.mico_Click)));
          }
          catch {

            // Do nothing if ShellItem is not available anymore and close the problematic item
            // tcMain.RemoveTabItem(item);
          }
        }
      }

      this.btnMoveto.Items.Add(new Separator());
      this.btnMoveto.Items.Add(otherLocationMove);
      this.btnCopyto.Items.Add(new Separator());
      this.btnCopyto.Items.Add(otherLocationCopy);
    }

    /// <summary>
    /// Raised on selection changed in tab control
    /// </summary>
    /// <param name="sender">the tab control itself</param>
    /// <param name="e">Event arguments</param>
    private void tcMain_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.RemovedItems.Count > 0) {
        var tab = e.RemovedItems[0] as Wpf.Controls.TabItem;

        if (tab != null && this._ShellListView.GetSelectedCount() > 0) {
          tab.SelectedItems = this._ShellListView.SelectedItems.Select(s => s.ParsingName).ToList();
        }
      }
      if (e.AddedItems.Count > 0) {
        this.SelectTab(e.AddedItems[0] as Wpf.Controls.TabItem);
      }
    }

  }
}