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

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for ConditionalSelectForm.xaml
    /// </summary>
    public partial class ConditionalSelectForm : Window
    {
        System.Globalization.CultureInfo ci;
        public ConditionalSelectForm()
        {
            InitializeComponent();

            dcquery.SelectedDate = DateTime.Today;
            dmquery.SelectedDate = DateTime.Today;
            daquery.SelectedDate = DateTime.Today;
            sizequery1.Text = "0";
            sizequery2.Text = "0";
            namequery.Text = (FindResource("txtFilename") as string);
            ci = System.Threading.Thread.CurrentThread.CurrentCulture;
        }

        public bool CancelAction = true;
        public ConditionalSelectData csd;

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.namefilter.IsEnabled = true;
                this.namequery.IsEnabled = true;
                this.namecase.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.namefilter.IsEnabled = false;
                this.namequery.IsEnabled = false;
                this.namecase.IsEnabled = false;
            }
            catch
            {

            }
        }

        private void sizecheck_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.sizefilter.IsEnabled = true;
                this.sizequery1.IsEnabled = true;
                this.sizequery2.IsEnabled = true;
                this.sizebox1.IsEnabled = true;
                ConditionalSelectComboBoxItem i = (ConditionalSelectComboBoxItem)sizefilter.SelectedItem;
                if (i.IdentifyingName == "Between" || i.IdentifyingName == "NotBetween")
                {
                    this.sizequery2.IsEnabled = true;
                    this.sizebox2.IsEnabled = true;
                }
                else
                {
                    this.sizequery2.IsEnabled = false;
                    this.sizebox2.IsEnabled = false;
                }
            }
            catch
            {

            }
        }

        private void sizecheck_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.sizefilter.IsEnabled = false;
                this.sizequery1.IsEnabled = false;
                this.sizequery2.IsEnabled = false;
                this.sizebox1.IsEnabled = false;
                this.sizebox2.IsEnabled = false;
            }
            catch
            {

            }
        }

        private void sizefilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ConditionalSelectComboBoxItem i = (ConditionalSelectComboBoxItem)e.AddedItems[0];
                if (i.IdentifyingName == "Between" || i.IdentifyingName == "NotBetween")
                {
                    this.sizequery2.IsEnabled = true;
                    this.sizebox2.IsEnabled = true;
                }
                else
                {
                    this.sizequery2.IsEnabled = false;
                    this.sizebox2.IsEnabled = false;
                }
            }
            catch
            {

            }
        }

        private void dccheck_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dcfilter.IsEnabled = true;
                this.dcquery.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void dccheck_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dcfilter.IsEnabled = false;
                this.dcquery.IsEnabled = false;
            }
            catch
            {

            }
        }

        private void dmcheck_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dmfilter.IsEnabled = true;
                this.dmquery.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void dmcheck_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dmfilter.IsEnabled = false;
                this.dmquery.IsEnabled = false;
            }
            catch
            {

            }
        }

        private void dacheck_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dafilter.IsEnabled = true;
                this.daquery.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void dacheck_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.dafilter.IsEnabled = false;
                this.daquery.IsEnabled = false;
            }
            catch
            {

            }
        }

        

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            CancelAction = false;
            ConditionalSelectParameters.FileNameFilterTypes fnf = (ConditionalSelectParameters.FileNameFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.FileNameFilterTypes), ((ConditionalSelectComboBoxItem)namefilter.SelectedItem).IdentifyingName);
            ConditionalSelectParameters.FileSizeFilterTypes fsf = (ConditionalSelectParameters.FileSizeFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.FileSizeFilterTypes), ((ConditionalSelectComboBoxItem)sizefilter.SelectedItem).IdentifyingName);
            FriendlySizeConverter.FileSizeMeasurements sd1 = (FriendlySizeConverter.FileSizeMeasurements)Enum.Parse(typeof(FriendlySizeConverter.FileSizeMeasurements), ((ConditionalSelectComboBoxItem)sizebox1.SelectedItem).IdentifyingName);
            FriendlySizeConverter.FileSizeMeasurements sd2 = (FriendlySizeConverter.FileSizeMeasurements)Enum.Parse(typeof(FriendlySizeConverter.FileSizeMeasurements), ((ConditionalSelectComboBoxItem)sizebox2.SelectedItem).IdentifyingName);
            ConditionalSelectParameters.DateFilterTypes dcf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem)dcfilter.SelectedItem).IdentifyingName);
            ConditionalSelectParameters.DateFilterTypes dmf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem)dmfilter.SelectedItem).IdentifyingName);
            ConditionalSelectParameters.DateFilterTypes daf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem)dafilter.SelectedItem).IdentifyingName);

            csd = new ConditionalSelectData(
                new ConditionalSelectParameters.FileNameParameters(namequery.Text, fnf, namecase.IsChecked.Value),
                new ConditionalSelectParameters.FileSizeParameters(FriendlySizeConverter.GetByteLength(
                    Convert.ToDouble(sizequery1.Text.Replace(",",ci.NumberFormat.NumberDecimalSeparator).Replace(
                      ".",ci.NumberFormat.NumberDecimalSeparator)), sd1),
                      FriendlySizeConverter.GetByteLength(Convert.ToDouble(sizequery2.Text.Replace(
                      ",", ci.NumberFormat.NumberDecimalSeparator).Replace(
                      ".", ci.NumberFormat.NumberDecimalSeparator)), sd2), fsf),
                new ConditionalSelectParameters.DateParameters(dcquery.SelectedDate.Value.Date, dcf),
                new ConditionalSelectParameters.DateParameters(dmquery.SelectedDate.Value.Date, dmf),
                new ConditionalSelectParameters.DateParameters(daquery.SelectedDate.Value.Date, daf),
                namecheck.IsChecked.Value, sizecheck.IsChecked.Value, dccheck.IsChecked.Value, dmcheck.IsChecked.Value, dacheck.IsChecked.Value);
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CancelAction = true;
            this.Close();
        }


        private void namequery_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (namequery.IsKeyboardFocused)
            {
                if (namequery.Text == (FindResource("txtFilename") as string))
                {
                    namequery.Text = "";
                }
            }
            else
            {
                if (namequery.Text == "")
                {
                    namequery.Text = (FindResource("txtFilename") as string);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show((FindResource("txtSelectFiles") as string), "Resource String");
        }


    }
}
