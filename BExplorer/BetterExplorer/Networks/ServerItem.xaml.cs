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

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        /// <summary>
        /// Called when a user requests to remove the NetworkItem this control represents.
        /// </summary>
        public event NetworkItemEventHandler RequestRemove;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnRequestRemove(NetworkItemEventArgs e)
        {
            if (RequestRemove != null)
                RequestRemove(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        /// <summary>
        /// Called when a user requests to edit the NetworkItem this control represents.
        /// </summary>
        public event NetworkItemEventHandler RequestEdit;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnRequestEdit(NetworkItemEventArgs e)
        {
            if (RequestEdit != null)
                RequestEdit(this, e);
        }

        private NetworkItem _item;

        public void LoadFromNetworkItem(NetworkItem item)
        {
            txtDisplayName.Text = item.DisplayName;
            txtAddress.Text = item.ServerAddress;
            switch (item.AccountService)
	        {
		        case AccountService.FTP:
                    txtType.Text = "FTP";
                    break;
                case AccountService.FTPS:
                    txtType.Text = "FTPS";
                    break;
                case AccountService.WebDAV:
                    txtType.Text = "WebDAV";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("item", "This is not a valid type of Network item.");
	        }

            if (item.AnonymousLogin == true)
            {
                txtUsername.Text = "Anonymous";
            }
            else
            {
                txtUsername.Text = item.Username;
            }

            _item = item;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            OnRequestRemove(new NetworkItemEventArgs(_item));
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            OnRequestEdit(new NetworkItemEventArgs(_item));
        }

    }
}
