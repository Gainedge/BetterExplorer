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

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for MountIso.xaml
    /// </summary>
    public partial class MountIso : Window
    {
        public MountIso()
        {
            InitializeComponent();
            PopulateDriveLetterDropDown();
        }

        public bool yep = false;

        private void PopulateDriveLetterDropDown()
        {
            char[] allchars = new char[] { 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z' };
            List<char> toremove = new List<char>();

            foreach (DriveInfo item in DriveInfo.GetDrives())
            {
                toremove.Add(GetDriveLetterFromDrivePath(item.RootDirectory.FullName));
            }

            foreach (char item in allchars)
            {
                if (toremove.Contains(item) == false)
                {
                    cbbLetter.Items.Add(item);
                }
            }

            cbbLetter.SelectedIndex = 0;

        }

        private char GetDriveLetterFromDrivePath(string path)
        {
            return path.Substring(0, 1).ToCharArray()[0];
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                cbbLetter.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                cbbLetter.IsEnabled = false;
            }
            catch
            {

            }
        }

        // OK
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (txtSize.IsEnabled == true)
            {
                bool noerror = true;

                try
                {
                    long blah = Convert.ToInt64(txtSize.Text);
                }
                catch
                {
                    noerror = false;
                }

                if (noerror == true)
                {
                    yep = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("The value entered in the Drive Size field is not a valid number.", "Error Must Be Resolved", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                yep = true;
                this.Close();
            }
        }

        // Cancel
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            yep = false;
            this.Close();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            try
            {
                txtSize.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void RadioButton_Unchecked_1(object sender, RoutedEventArgs e)
        {
            try
            {
                txtSize.IsEnabled = false;
            }
            catch
            {

            }
        }

    }
}
