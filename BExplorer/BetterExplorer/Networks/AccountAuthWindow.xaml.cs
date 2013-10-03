//using AppLimit.CloudComputing.SharpBox;
//using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for AccountAuthWindow.xaml
    /// </summary>
    public partial class AccountAuthWindow : Window
    {
        public AccountAuthWindow()
        {
            InitializeComponent();
            this.Left = 50;
            this.Top = 50;
        }

        public bool yep = false;
        private bool AllowClose = true;

        public string AddedService = "";
        public object Parameter = null;

        private string _datadir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BExplorer\\NetworkAccounts\\";

        public string DataDirectory
        {
            get
            {
                return _datadir;
            }
            set
            {
                _datadir = value;
            }
        }

        public void LoadStorageServices()
        {
            AddAccountEntry aa1 = new AddAccountEntry();
            aa1.Text = "Dropbox";
            aa1.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/Networks/Icons/dropbox32.png", UriKind.Relative));
            aa1.ItemSelected += Dropbox_ItemSelected;
            pnlServices.Children.Add(aa1);

            AddAccountEntry aa2 = new AddAccountEntry();
            aa2.Text = "Google Drive (not yet added)";
            pnlServices.Children.Add(aa2);
        }

        public void LoadSocialMediaServices()
        {
            AddAccountEntry aa1 = new AddAccountEntry();
            aa1.Text = "Google+ (not yet added)";
            pnlServices.Children.Add(aa1);

            AddAccountEntry aa2 = new AddAccountEntry();
            aa2.Text = "YouTube (not yet added)";
            pnlServices.Children.Add(aa2);

            AddAccountEntry aa3 = new AddAccountEntry();
            aa3.Text = "Facebook (not yet added)";
            pnlServices.Children.Add(aa3);

            AddAccountEntry aa4 = new AddAccountEntry();
            aa4.Text = "Twitter (not yet added)";
            pnlServices.Children.Add(aa4);

            AddAccountEntry aa5 = new AddAccountEntry();
            aa5.Text = "Pastebin.com";
            aa5.ItemSelected += Pastebin_ItemSelected;
            pnlServices.Children.Add(aa5);
        }

        #region Pastebin

        void Pastebin_ItemSelected(object sender, EventArgs e)
        {
            lblLogin.Text = "Login to Pastebin";
            BeginLogin += Pastebin_BeginLogin;
            MainMenu.Visibility = System.Windows.Visibility.Collapsed;
            LoginFrame.Visibility = System.Windows.Visibility.Visible;
        }

        void Pastebin_BeginLogin(object sender, AccountAuthWindow.LoginRoutedEventArgs e)
        {
            try
            {
                PastebinClient li = new PastebinClient(e.Username, e.Password);

                if (li.UserKey != null)
                {
                    Complete(true, "Pastebin");
                }
            }
            catch (System.Net.WebException ex)
            {
                if (MessageBox.Show("Pastebin rejected the login with the following message: \n\n" + ex.Message + "\n \nWould you like to retry after entering new information?", "Login Failed", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {

                }
                else
                {
                    Complete(false, "Pastebin");
                }
            }
        }
        #endregion

        #region Dropbox

        //private DropBoxConfiguration _UsedConfig = null;
        //private DropBoxRequestToken _CurrentRequestToken = null;
        //private ICloudStorageAccessToken _GeneratedToken = null;

        void Dropbox_ItemSelected(object sender, EventArgs e)
        {
            SetUpDropbox();
        }

        private void SetUpDropbox()
        {
            AllowClose = false;
            //_UsedConfig = DropBoxConfiguration.GetStandardConfiguration();
            //_UsedConfig.AuthorizationCallBack = new Uri("http://better-explorer.com/");
            //_UsedConfig.APIVersion = DropBoxAPIVersion.V1;
            //_CurrentRequestToken = DropBoxStorageProviderTools.GetDropBoxRequestToken(_UsedConfig, Networks.DropBoxAuth.Key, Networks.DropBoxAuth.Secret);
            //string AuthUrl = DropBoxStorageProviderTools.GetDropBoxAuthorizationUrl(_UsedConfig, _CurrentRequestToken);
            //this.Navigated += HandleDropboxAuth;
            //this.NavigateTo(AuthUrl);
            MainMenu.Visibility = System.Windows.Visibility.Collapsed;
            this.Width = 900;
            this.Height = 650;
            BrowserFrame.Visibility = System.Windows.Visibility.Visible;
        }

        private void HandleDropboxAuth(object sender, NavigationRoutedEventArgs e)
        {
            //if (_GeneratedToken == null && e.Uri.ToString().StartsWith(_UsedConfig.AuthorizationCallBack.ToString()))
            //{
            //    _GeneratedToken = DropBoxStorageProviderTools.ExchangeDropBoxRequestTokenIntoAccessToken(_UsedConfig, Networks.DropBoxAuth.Key, Networks.DropBoxAuth.Secret, _CurrentRequestToken);

            //    (sender as Window).Close();

            //    CloudStorage cs = new CloudStorage();
            //    cs.Open(_UsedConfig, _GeneratedToken);

            //    Complete(cs.IsOpened, "Dropbox");
            //    //cs.SerializeSecurityToken(_GeneratedToken);

            //    cs.Close();
            //}
        }

        #endregion

        public void NavigateTo(string url)
        {
            wBrowser.Navigate(url);
        }

        public void Complete(bool Success, string Service, object Parameter = null)
        {
            yep = true;
            AddedService = Service;
            if (Success == true)
            {
                SuccessImage.Visibility = System.Windows.Visibility.Visible;
                FailureImage.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                SuccessImage.Visibility = System.Windows.Visibility.Collapsed;
                FailureImage.Visibility = System.Windows.Visibility.Visible;
            }
            ReportText.Text = "Better Explorer was able to connect to this service:";
            ServiceText.Text = Service;
            BrowserFrame.Visibility = System.Windows.Visibility.Collapsed;
            MainMenu.Visibility = System.Windows.Visibility.Collapsed;
            ThankYou.Visibility = System.Windows.Visibility.Visible;
            AllowClose = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            yep = false;
            AllowClose = true;
            this.Close();
        }

        // BrowserFrame
        private void BackButton_Click(object sender, EventArgs e)
        {
            RemoveRoutedEventHandlers(this, NavigatedEvent);
            NavigateTo("about:blank");
            BrowserFrame.Visibility = System.Windows.Visibility.Collapsed;
            MainMenu.Visibility = System.Windows.Visibility.Visible;
            AllowClose = true;
        }

        // LoginFrame
        private void BackButton_Click_1(object sender, EventArgs e)
        {
            RemoveRoutedEventHandlers(this, BeginLoginEvent);
            LoginFrame.Visibility = System.Windows.Visibility.Collapsed;
            MainMenu.Visibility = System.Windows.Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AllowClose = true;
            this.Close();
        }

        #region Navigated Event

        private void wBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            RaiseNavigatedEvent(e.Uri, e.WebResponse);
        }

        public delegate void NavigationRoutedEventHandler(object sender, NavigationRoutedEventArgs e);

        // Create a custom routed event by first registering a RoutedEventID 
        // This event uses the bubbling routing strategy 
        public static readonly RoutedEvent NavigatedEvent = EventManager.RegisterRoutedEvent(
            "Navigated", RoutingStrategy.Bubble, typeof(NavigationRoutedEventHandler), typeof(AccountAuthWindow));

        // Provide CLR accessors for the event 
        public event NavigationRoutedEventHandler Navigated
        {
            add { AddHandler(NavigatedEvent, value); }
            remove { RemoveHandler(NavigatedEvent, value); }
        }

        // This method raises the Tap event 
        void RaiseNavigatedEvent(Uri address, System.Net.WebResponse response)
        {
            NavigationRoutedEventArgs newEventArgs = new NavigationRoutedEventArgs(AccountAuthWindow.NavigatedEvent, address, response);
            newEventArgs.Source = this;
            RaiseEvent(newEventArgs);
        }

        /// <summary>
        /// Removes all event handlers subscribed to the specified routed event from the specified element.
        /// </summary>
        /// <param name="element">The UI element on which the routed event is defined.</param>
        /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
        public static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            if (eventHandlersStore == null) return;

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            // Iteratively remove all routed event handlers from the element.
            foreach (var routedEventHandler in routedEventHandlers)
            {
                element.RemoveHandler(routedEvent, routedEventHandler.Handler);
            }
        }

        public class NavigationRoutedEventArgs : RoutedEventArgs
        {
            private System.Net.WebResponse _resp;
            private Uri _address;

            public NavigationRoutedEventArgs(RoutedEvent RoutedEvent, Uri uri, System.Net.WebResponse response)
            {
                base.RoutedEvent = RoutedEvent;
                _address = uri;
                _resp = response;
            }

            public Uri Uri
            {
                get
                {
                    return _address;
                }
            }

            public System.Net.WebResponse Response
            {
                get
                {
                    return _resp;
                }
            }

            public RoutedEvent RoutedEvent
            {
                get
                {
                    return base.RoutedEvent;
                }
                set
                {
                    base.RoutedEvent = value;
                }
            }

            public object Source
            {
                get
                {
                    return base.Source;
                }
                set
                {
                    base.Source = value;
                }
            }

            public object OriginalSource
            {
                get
                {
                    return base.OriginalSource;
                }
            }

        }
        #endregion

        #region BeginLogin Event

        public delegate void LoginRoutedEventHandler(object sender, LoginRoutedEventArgs e);

        // Create a custom routed event by first registering a RoutedEventID 
        // This event uses the bubbling routing strategy 
        public static readonly RoutedEvent BeginLoginEvent = EventManager.RegisterRoutedEvent(
            "BeginLogin", RoutingStrategy.Bubble, typeof(LoginRoutedEventHandler), typeof(AccountAuthWindow));

        // Provide CLR accessors for the event 
        public event LoginRoutedEventHandler BeginLogin
        {
            add { AddHandler(BeginLoginEvent, value); }
            remove { RemoveHandler(BeginLoginEvent, value); }
        }

        // This method raises the Tap event 
        void RaiseBeginLoginEvent(string username, string password)
        {
            LoginRoutedEventArgs newEventArgs = new LoginRoutedEventArgs(AccountAuthWindow.NavigatedEvent, username, password);
            newEventArgs.Source = this;
            RaiseEvent(newEventArgs);
        }

        public class LoginRoutedEventArgs : RoutedEventArgs
        {
            private string _user;
            private string _pass;

            public LoginRoutedEventArgs(RoutedEvent RoutedEvent, string username, string password)
            {
                base.RoutedEvent = RoutedEvent;
                _user = username;
                _pass = password;
            }

            public string Username
            {
                get
                {
                    return _user;
                }
            }

            public string Password
            {
                get
                {
                    return _pass;
                }
            }

            public RoutedEvent RoutedEvent
            {
                get
                {
                    return base.RoutedEvent;
                }
                set
                {
                    base.RoutedEvent = value;
                }
            }

            public object Source
            {
                get
                {
                    return base.Source;
                }
                set
                {
                    base.Source = value;
                }
            }

            public object OriginalSource
            {
                get
                {
                    return base.OriginalSource;
                }
            }

        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AllowClose == false)
            {
                e.Cancel = true;
                AllowClose = true;
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            RaiseBeginLoginEvent(txtUsername.Text, txtPassword.Password);
        }

    }

}
