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

namespace BetterExplorer.Networks {
	/// <summary>
	/// Interaction logic for AddServer.xaml
	/// </summary>
	public partial class AddServer : Window {
		// TODO: Add code for creating an FTP/WebDAV server instance if OK is clicked.

		public AddServer() {
			InitializeComponent();
			ServerType_SelectionChanged(null, null);
		}

		private void ServerType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			try {
				switch (ServerType.SelectedIndex) {
					case 0:
						txtPort.Text = "21";
						txtPort.IsEnabled = true;
						expSecurity.IsExpanded = false;
						expSecurity.IsEnabled = false;
						chkAnonymous.IsEnabled = true;
						chkPassive.IsEnabled = true;
						break;
					case 1:
						txtPort.Text = "990";
						txtPort.IsEnabled = true;
						expSecurity.IsEnabled = true;
						chkAnonymous.IsEnabled = true;
						chkPassive.IsEnabled = true;
						break;
					case 2:
						txtPort.Text = "";
						txtPort.IsEnabled = false;
						expSecurity.IsExpanded = false;
						expSecurity.IsEnabled = false;
						chkAnonymous.IsEnabled = false;
						chkPassive.IsEnabled = false;
						break;
					default:
						//ServerType.SelectedIndex = 0;
						break;
				}
			}
			catch (Exception) {

			}
		}

		public bool yep = false;

		private void Button1_Click(object sender, RoutedEventArgs e) {
			yep = true;
			this.Close();
		}

		private void Button2_Click(object sender, RoutedEventArgs e) {
			yep = false;
			this.Close();
		}

		private void expSecurity_Expanded(object sender, RoutedEventArgs e) {
			this.Height = 600;
		}

		private void expSecurity_Collapsed(object sender, RoutedEventArgs e) {
			this.Height = 500;
		}

		public NetworkItem GetNetworkItem() {
			switch (ServerType.SelectedIndex) {
				case 0:
					return GetFTPServer();
				case 1:
					return GetFTPSServer();
				case 2:
					return GetWebDAVServer();
				default:
					return null;
			}
		}

		private FTPServer GetFTPServer() {
			return null;
			//return new FTPServer(txtDisplayName.Text, txtAddress.Text, Convert.ToInt32(txtPort.Text), txtUsername.Text, txtPassword.Text, chkAnonymous.IsChecked.Value, chkPassive.IsChecked.Value);
		}

		private FTPSServer GetFTPSServer() {
			return null;
			//return new FTPSServer(txtDisplayName.Text, txtAddress.Text, Convert.ToInt32(txtPort.Text), txtUsername.Text, txtPassword.Text, chkAnonymous.IsChecked.Value, chkPassive.IsChecked.Value, AlexPilotti.FTPS.Client.ESSLSupportMode.DataChannelRequested);
		}

		private WebDAVserver GetWebDAVServer() {
			return null;
			//return new WebDAVserver(txtDisplayName.Text, txtAddress.Text, txtUsername.Text, txtPassword.Text);
		}

		public void ImportNetworkItem(NetworkItem item) {
			txtDisplayName.Text = item.DisplayName;
			txtAddress.Text = item.ServerAddress;
			txtUsername.Text = item.Username;
			txtPassword.Text = item.Password;
			chkAnonymous.IsChecked = item.AnonymousLogin;

			switch (item.AccountService) {
				case AccountService.FTP:
					ServerType.SelectedIndex = 0;
					//chkPassive.IsChecked = (item as FTPServer).PassiveMode;
					txtPort.Text = item.Port.ToString();
					break;
				case AccountService.FTPS:
					ServerType.SelectedIndex = 1;
					//chkPassive.IsChecked = (item as FTPSServer).PassiveMode;
					txtPort.Text = item.Port.ToString();
					break;
				case AccountService.WebDAV:
					ServerType.SelectedIndex = 2;
					break;
				default:
					throw new ArgumentOutOfRangeException("item", "This is not a valid type of Network item.");
			}
		}

	}
}
