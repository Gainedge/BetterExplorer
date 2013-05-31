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
    /// Interaction logic for StringSearchCriteriaDialog.xaml
    /// </summary>
    public partial class SizeSearchCriteriaDialog : Window
    {
        public bool Confirm = false;

        public SizeSearchCriteriaDialog()
        {
            InitializeComponent();
            SetFilterName(FindResource("btnOSizeCP") as string);
        }

        public SizeSearchCriteriaDialog(string property, string value)
        {
            InitializeComponent();

            ConditionalSelectComboBoxItem i = (ConditionalSelectComboBoxItem)sizefilter.SelectedItem;

            if (i.IdentifyingName == "Between")
            {
                this.sizequery2.IsEnabled = true;
                this.sizebox2.IsEnabled = true;
            }
            else
            {
                this.sizequery2.IsEnabled = false;
                this.sizebox2.IsEnabled = false;
            }
            //textBlock1.Text = textBlock1.Text.Replace("<category>", property);
            //textBox1.Text = GetValueOnly(property, value);
            //textBox1.Focus();
        }

        public void UpdateCurrentValue(string value)
        {
            curval.Text = FindResource("txtCurrentValue") + ": " + value;
        }

        public void SetFilterName(string name)
        {
            string title = FindResource("txtSetFilter") as string;
            textBlock1.Text = title.Replace("(VAL)", name);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Confirm = true;
            this.Close();
        }

        private string GetValueOnly(string property, string value)
        {
            return value.Substring(property.Length + 1);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Confirm = false;
            this.Close();
        }

        private void sizefilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ConditionalSelectComboBoxItem i = (ConditionalSelectComboBoxItem)e.AddedItems[0];
                if (i.IdentifyingName == "Between")
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

        private long GetLongFromString(string text)
        {
            try
            {
                return Convert.ToInt32(text);
            }
            catch
            {
                return -1;
            }
        }

        public string GetSizeQuery()
        {
            return HandleSizeQuery(GetLongFromString(sizequery1.Text), GetLongFromString(sizequery2.Text), ((ConditionalSelectComboBoxItem)sizebox1.SelectedItem).Text, ((ConditionalSelectComboBoxItem)sizebox2.SelectedItem).Text);
        }

        private string HandleSizeQuery(long s1, long s2, string m1, string m2)
        {
            string q = "";

            if (s1 == -1)
            {
                return "";
            }

            if (s2 == -1 && ((ConditionalSelectComboBoxItem)sizefilter.SelectedItem).IdentifyingName == "Between")
            {
                s2 = s1;
            }

            ConditionalSelectComboBoxItem i = (ConditionalSelectComboBoxItem)sizefilter.SelectedItem;

            switch (i.IdentifyingName)
            {
                case "LargerThan":
                    return ">" + s1 + m1;
                case "SmallerThan":
                    return "<" + s1 + m1;
                case "Equals":
                    return Convert.ToString(s1) + m1;
                case "Between":
                            string smallbound;
                            string largebound;
                            if (s2 > s1)
                            {
                                smallbound = Convert.ToString(s1) + m1;
                                largebound = Convert.ToString(s2) + m2;
                            }
                            else
                            {
                                if (s2 < s1)
                                {
                                    smallbound = Convert.ToString(s2) + m2;
                                    largebound = Convert.ToString(s1) + m1;
                                }
                                else
                                {
                                    return Convert.ToString(s1 + m1);
                                }
                            }
                    return smallbound + ".." + largebound;
                default:
                    break;
            }

            return q;
        }

    }
}
