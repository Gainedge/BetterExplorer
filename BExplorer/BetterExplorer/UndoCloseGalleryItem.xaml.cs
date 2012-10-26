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
using Microsoft.WindowsAPICodePack.Shell;
using BetterExplorer;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for UndoCloseGalleryItem.xaml
    /// </summary>
    public partial class UndoCloseGalleryItem : UserControl
    {
        public UndoCloseGalleryItem()
        {
            InitializeComponent();
        }

        public NavigationLog nav;

        public void LoadData(NavigationLog log)
        {
            nav = log;
            ShellObject obj = log.CurrentLocation;
            tabTitle.Text = obj.GetDisplayName(DisplayNameType.Default);
            image1.Source = obj.Thumbnail.BitmapSource;
            this.ToolTip = obj.ParsingName;
        }

        public delegate void NavigationLogEventHandler(object sender, NavigationLogEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event NavigationLogEventHandler Click;
        //public event EventHandler MouseDoubleClick;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnClick(NavigationLogEventArgs e)
        {
            if (Click != null)
                Click(this, e);
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OnClick(new NavigationLogEventArgs(nav));
        }

        public void PerformClickEvent()
        {
            OnClick(new NavigationLogEventArgs(nav));
        }
    }
}


public class NavigationLogEventArgs
{
    NavigationLog _obj;

    public NavigationLogEventArgs(NavigationLog log)
    {
        _obj = log;
    }

    public ShellObject CurrentLocation
    {
        get
        {
            return _obj.CurrentLocation;
        }
    }

    public NavigationLog NavigationLog
    {
        get
        {
            return _obj;
        }
    }

}