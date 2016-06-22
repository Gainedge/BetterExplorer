using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterExplorer.Networks
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private SystemMenu m_SystemMenu;
        private const int m_UrlID = 0x101;

        public AuthWindow() { InitializeComponent(); }

        public void NavigateTo(string url) => wBrowser.Navigate(url);

        public delegate void NavigationEventHandler(object sender, NavigationEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event NavigationEventHandler Navigated;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnNavigated(NavigationEventArgs e) => Navigated?.Invoke(this, e);

        private void wBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) => OnNavigated(e);

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource src = HwndSource.FromHwnd(windowHandle);
            src.AddHook(new HwndSourceHook(WndProc));
            try
            {
                m_SystemMenu = SystemMenu.FromWPFForm(this);
                m_SystemMenu.AppendSeparator();
                m_SystemMenu.AppendMenu(m_UrlID, "Copy Current URL");
            }
            catch (NoSystemMenuException)
            {
            }
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0112 && wParam.ToInt32() == m_UrlID) Clipboard.SetText(wBrowser.Source.ToString());
            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource src = HwndSource.FromHwnd(windowHandle);
            src.RemoveHook(new HwndSourceHook(this.WndProc));
        }
    }
}
