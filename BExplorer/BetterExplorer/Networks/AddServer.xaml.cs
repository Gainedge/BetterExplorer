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

namespace BetterExplorer.Networks
{
    /// <summary>
    /// Interaction logic for AddServer.xaml
    /// </summary>
    public partial class AddServer : Window
    {
        // TODO: Add code for creating an FTP/WebDAV server instance if OK is clicked.

        public AddServer()
        {
            InitializeComponent();
        }

        public bool yep = false;

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            yep = true;
            this.Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            yep = false;
            this.Close();
        }


    }
}
