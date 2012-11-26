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

namespace BetterExplorer.Tabs
{
    /// <summary>
    /// Interaction logic for TabManager.xaml
    /// </summary>
    public partial class TabManager : Window
    {
        public TabManager()
        {
            InitializeComponent();
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
            foreach (string item in Directory.GetFiles(sstdir))
            {
                ShellObject obj = ShellObject.FromParsingName(item);
                o.Add(RemoveExtensionsFromFile(obj.GetDisplayName(DisplayNameType.Default), GetExtension(item)));
            }
            return o;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            stackPanel1.Children.Clear();
            foreach (string item in LoadListOfTabListFiles())
            {
                SavedTabsListGalleryItem gli = new SavedTabsListGalleryItem(item);
                gli.Click += new SavedTabsListGalleryItem.PathStringEventHandler(gli_Click);
                stackPanel1.Children.Add(gli);
            }
        }

        void gli_Click(object sender, PathStringEventArgs e)
        {
            foreach (SavedTabsListGalleryItem item in stackPanel1.Children)
	        {
		        item.SetDeselected();
	        }

            (sender as SavedTabsListGalleryItem).SetSelected();
            SavedTabsList list = SavedTabsList.LoadTabList(sstdir + e.PathString + ".txt");
            tabListEditor1.ImportSavedTabList(list);
            //MessageBox.Show(sstdir + e.PathString + ".txt");
            //throw new NotImplementedException();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
