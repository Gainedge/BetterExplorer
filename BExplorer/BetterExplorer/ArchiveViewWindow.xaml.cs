using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Controls;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Dialogs;
using SevenZip;
using System.Threading;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for ArchiveViewWindow.xaml
    /// </summary>
    public partial class ArchiveViewWindow : Window
    {
        ShellObject archive;
        ExplorerBrowser Explorer = new ExplorerBrowser();

        public ArchiveViewWindow(ShellObject loc, bool IsPreviewPaneEnabled, bool IsInfoPaneEnabled)
        {
            InitializeComponent();

            archive = loc;

            this.Title = "View Archive - " + archive.GetDisplayName(DisplayNameType.Default);

            ShellVView.Child = Explorer;

            Explorer.NavigationOptions.PaneVisibility.Commands = PaneVisibilityState.Hide;
            Explorer.NavigationOptions.PaneVisibility.CommandsOrganize = PaneVisibilityState.Hide;
            Explorer.NavigationOptions.PaneVisibility.CommandsView = PaneVisibilityState.Hide;
            Explorer.NavigationOptions.PaneVisibility.Preview =
                IsPreviewPaneEnabled ? PaneVisibilityState.Show : PaneVisibilityState.Hide;
            Explorer.NavigationOptions.PaneVisibility.Details =
                IsInfoPaneEnabled ? PaneVisibilityState.Show : PaneVisibilityState.Hide;
            Explorer.NavigationOptions.PaneVisibility.Navigation = PaneVisibilityState.Hide;

            Explorer.ContentOptions.FullRowSelect = true;
            Explorer.ContentOptions.CheckSelect = false;
            Explorer.ContentOptions.ViewMode = ExplorerBrowserViewMode.Tile;

            Explorer.NavigationComplete += new EventHandler<NavigationCompleteEventArgs>(Explorer_NavigationComplete);
            Explorer.Navigate(loc);
        }

        void Explorer_NavigationComplete(object sender, NavigationCompleteEventArgs e)
        {
            if (e.NewLocation.ParsingName == archive.ParsingName)
            {
                btnUpLevel.IsEnabled = false;
            }
            else
            {
                btnUpLevel.IsEnabled = true;
            }
            leftNavBut.IsEnabled = Explorer.NavigationLog.CanNavigateBackward;
            rightNavBut.IsEnabled = Explorer.NavigationLog.CanNavigateForward;
            //throw new NotImplementedException();
        }

        private void ExtractArchive_Click(object sender, RoutedEventArgs e)
        {
            string output = ChooseLocation();
            this.Focus();
            if (output != null)
            {
                ExtractToLocation(archive.ParsingName, output);
            }
        }

        private void leftNavBut_Click(object sender, RoutedEventArgs e)
        {
            Explorer.NavigateLogLocation(NavigationLogDirection.Backward);
        }

        private void rightNavBut_Click(object sender, RoutedEventArgs e)
        {
            Explorer.NavigateLogLocation(NavigationLogDirection.Forward);
        }

        private void btnUpLevel_Click(object sender, RoutedEventArgs e)
        {
            Explorer.Navigate(Explorer.NavigationLog.CurrentLocation.Parent);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DoCheck()
        {

            SevenZipExtractor extractor = new SevenZipExtractor(archive.ParsingName);
            if (!extractor.Check())
                MessageBox.Show("Not Pass");
            else
                MessageBox.Show("Pass");

            extractor.Dispose();
        }

        private string ChooseLocation()
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dlg.FileName;
            }
            else
            {
                return null;
            }
        }

        private void ExtractToLocation(string archive, string output)
        {
            var selectedItems = new List<string>() { archive };

            var archiveProcressScreen = new ArchiveProcressScreen(selectedItems,
                               output,
                               ArchiveAction.Extract,
                               "");
            archiveProcressScreen.ExtractionCompleted += new ArchiveProcressScreen.ExtractionCompleteEventHandler(ExtractionHasCompleted);
            archiveProcressScreen.Show();
        }

        private void ExtractionHasCompleted(object sender, ArchiveEventArgs e)
        {

        }

        private void CheckIntegrity_Click(object sender, RoutedEventArgs e)
        {
            Thread trIntegrityCheck = new Thread(new ThreadStart(DoCheck));
            trIntegrityCheck.Start();
        }

    }
}
