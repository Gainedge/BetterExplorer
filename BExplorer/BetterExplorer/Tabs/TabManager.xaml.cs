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
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.Win32;

namespace BetterExplorer.Tabs
{
    /// <summary>
    /// Interaction logic for TabManager.xaml
    /// </summary>
    public partial class TabManager : Window
    {
        string selfile;
        
        public TabManager()
        {
            InitializeComponent();
        }

        public MainWindow MainForm;

        private string GetDefaultLocation()
        {
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rks = rk.CreateSubKey(@"Software\BExplorer");
            string df = rks.GetValue(@"StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();
            rk.Close();
            rks.Close();
            return df;
        }

        private string GetSavedTabsLocation()
        {
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rks = rk.CreateSubKey(@"Software\BExplorer");
            string df = rks.GetValue(@"SavedTabsDirectory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\").ToString();
            rk.Close();
            rks.Close();
            return df;
        }

        private string GetExtension(string file)
        {
            return file.Substring(file.LastIndexOf("."));
        }

        private string RemoveExtensionsFromFile(string file, string ext)
        {
            if (file.EndsWith(ext) == true)
            {
                return file.Remove(file.LastIndexOf(ext), ext.Length);
            }
            else
            {
                return file;
            }
        }

        string sstdir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\";

        public List<string> LoadListOfTabListFiles()
        {
            List<string> o = new List<string>();
            foreach (string item in Directory.GetFiles(GetSavedTabsLocation()))
            {
                ShellObject obj = ShellObject.FromParsingName(item);
                o.Add(RemoveExtensionsFromFile(obj.GetDisplayName(DisplayNameType.Default), GetExtension(item)));
            }
            return o;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            stackPanel1.Children.Clear();
            foreach (string item in LoadListOfTabListFiles())
            {
                SavedTabsListGalleryItem gli = new SavedTabsListGalleryItem(item);
                gli.Click += new SavedTabsListGalleryItem.PathStringEventHandler(gli_Click);
                stackPanel1.Children.Add(gli);
            }
        }

        private void RefreshListAndLoad(string loc)
        {
            stackPanel1.Children.Clear();
            foreach (string item in LoadListOfTabListFiles())
            {
                if (item == loc)
                {
                    SavedTabsListGalleryItem gli = new SavedTabsListGalleryItem(item, true);
                    gli.Click += new SavedTabsListGalleryItem.PathStringEventHandler(gli_Click);
                    stackPanel1.Children.Add(gli);
                }
                else
                {
                    SavedTabsListGalleryItem gli = new SavedTabsListGalleryItem(item, false);
                    gli.Click += new SavedTabsListGalleryItem.PathStringEventHandler(gli_Click);
                    stackPanel1.Children.Add(gli);
                }
            }
        }

        void gli_Click(object sender, PathStringEventArgs e)
        {
            foreach (SavedTabsListGalleryItem item in stackPanel1.Children)
	        {
		        item.SetDeselected();
	        }

            (sender as SavedTabsListGalleryItem).SetSelected();
            tabListEditor1.ImportSavedTabList(SavedTabsList.LoadTabList(GetSavedTabsLocation() + e.PathString + ".txt"));
            selfile = GetSavedTabsLocation() + e.PathString + ".txt";
            //MessageBox.Show(sstdir + e.PathString + ".txt");
            //throw new NotImplementedException();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            SavedTabsList.SaveTabList(tabListEditor1.ExportSavedTabList(), selfile);
            tabListEditor1.ImportSavedTabList(SavedTabsList.LoadTabList(selfile));
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            tabListEditor1.AddTab(GetDefaultLocation());
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            NameTabList o = new NameTabList();
						o.Owner = this;
            o.ShowDialog();
            if (o.dialogresult == true)
            {
                SavedTabsList.SaveTabList(SavedTabsList.CreateFromString(GetDefaultLocation()), GetSavedTabsLocation() + o.textBox1.Text + ".txt");
                RefreshListAndLoad(sstdir + o.textBox1.Text + ".txt");
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            NameTabList o = new NameTabList();
						o.Owner = this;
            o.ShowDialog();
            if (o.dialogresult == true)
            {
                SavedTabsList.SaveTabList(tabListEditor1.ExportSavedTabList(), GetSavedTabsLocation() + o.textBox1.Text + ".txt");
                RefreshListAndLoad(sstdir + o.textBox1.Text + ".txt");
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show((FindResource("txtConfirmDelete") as string) + "?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                System.IO.File.Delete(selfile);
                RefreshList();
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MainForm.NewTab(GetSavedTabsLocation());
            MainForm.Focus();
        }

    }
}
