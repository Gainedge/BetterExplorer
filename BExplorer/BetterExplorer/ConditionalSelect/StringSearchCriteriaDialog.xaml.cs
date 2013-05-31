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
    public partial class StringSearchCriteriaDialog : Window
    {
        public bool Confirm = false;

        public StringSearchCriteriaDialog()
        {
            InitializeComponent();
        }

        public StringSearchCriteriaDialog(string property, string value, string displayname)
        {
            InitializeComponent();
            SetFilterName(displayname);
            textBox1.Text = GetValueOnly(property, value);
            textBox1.Focus();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Confirm = true;
            this.Close();
        }

        public void SetFilterName(string name)
        {
            string title = FindResource("txtSetFilter") as string;
            textBlock1.Text = title.Replace("(VAL)", name);
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
    }
}
