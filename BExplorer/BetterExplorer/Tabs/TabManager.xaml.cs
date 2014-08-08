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
using System.IO;
using Microsoft.Win32;
using BExplorer.Shell;

namespace BetterExplorer.Tabs {
	/// <summary>
	/// Interaction logic for TabManager.xaml
	/// </summary>
	public partial class TabManager : Window {
		public MainWindow MainForm;
		string selfile, sstdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\";

		public TabManager() {
			InitializeComponent();
		}

		private string GetDefaultLocation() {
			return Utilities.GetRegistryValue("StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();
		}

		private string GetSavedTabsLocation() {
			return Utilities.GetRegistryValue("SavedTabsDirectory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\").ToString();
		}

		public List<string> LoadListOfTabListFiles() {
			var o = new List<string>();
			foreach (string item in Directory.GetFiles(GetSavedTabsLocation())) {
				var obj = new ShellItem(item);
				o.Add(Utilities.RemoveExtensionsFromFile(obj.DisplayName, Utilities.GetExtension(item)));
			}
			return o;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			RefreshList();
		}

		private void RefreshList() {
			stackPanel1.Children.Clear();
			foreach (string item in LoadListOfTabListFiles()) {
				var gli = new SavedTabsListGalleryItem(item);
				gli.Click += new SavedTabsListGalleryItem.PathStringEventHandler(gli_Click);
				stackPanel1.Children.Add(gli);
			}
		}

		private void RefreshListAndLoad(string loc) {
			stackPanel1.Children.Clear();
			foreach (string item in LoadListOfTabListFiles()) {
				var gli = new SavedTabsListGalleryItem(item, item == loc);
				gli.Click += new SavedTabsListGalleryItem.PathStringEventHandler(gli_Click);
				stackPanel1.Children.Add(gli);
			}
		}

		void gli_Click(object sender, Tuple<string> e) {
			foreach (SavedTabsListGalleryItem item in stackPanel1.Children) {
				item.SetDeselected();
			}

			(sender as SavedTabsListGalleryItem).SetSelected();
			tabListEditor1.ImportSavedTabList(SavedTabsList.LoadTabList(GetSavedTabsLocation() + e.Item1 + ".txt"));
			selfile = GetSavedTabsLocation() + e.Item1 + ".txt";
		}

		private void button3_Click(object sender, RoutedEventArgs e) {
			SavedTabsList.SaveTabList(tabListEditor1.ExportSavedTabList(), selfile);
			tabListEditor1.ImportSavedTabList(SavedTabsList.LoadTabList(selfile));
		}

		private void button5_Click(object sender, RoutedEventArgs e) {
			tabListEditor1.AddTab(GetDefaultLocation());
		}

		private void button4_Click(object sender, RoutedEventArgs e) {
			var Entered_Name = NameTabList.Open(this);
			if (Entered_Name != null) {
				SavedTabsList.SaveTabList(SavedTabsList.CreateFromString(GetDefaultLocation()), GetSavedTabsLocation() + Entered_Name + ".txt");
				RefreshListAndLoad(sstdir + Entered_Name + ".txt");
			}
		}

		private void button7_Click(object sender, RoutedEventArgs e) {
			var Entered_Name = NameTabList.Open(this);
			if (Entered_Name != null) {
				SavedTabsList.SaveTabList(tabListEditor1.ExportSavedTabList(), GetSavedTabsLocation() + Entered_Name + ".txt");
				RefreshListAndLoad(sstdir + Entered_Name + ".txt");
			}
		}

		private void button6_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show((FindResource("txtConfirmDelete") as string) + "?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes) {
				System.IO.File.Delete(selfile);
				RefreshList();
			}
		}

		private void button2_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			MainForm.tcMain.NewTab(GetSavedTabsLocation());
			MainForm.Focus();
		}
	}
}
