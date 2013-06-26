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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterExplorer.Networks
{
    /// <summary>
    /// Interaction logic for ServerItem.xaml
    /// </summary>
    public partial class ServerItem : UserControl
    {
        // TODO: Add code to load data from a server class instance or save to a server class instance.

        public ServerItem()
        {
            InitializeComponent();
        }

        public string ServerDisplayName
        {
            get
            {
                return txtDisplayName.Text;
            }
            set
            {
                txtDisplayName.Text = value;
            }
        }

        public string ServerAddress
        {
            get
            {
                return txtAddress.Text;
            }
            set
            {
                txtAddress.Text = value;
            }
        }

        public string Username
        {
            get
            {
                return txtUsername.Text;
            }
            set
            {
                txtUsername.Text = value;
            }
        }

        private string _pass;

        public string Password
        {
            get
            {
                return _pass;
            }
            set
            {
                _pass = value;
            }
        }
    }
}
