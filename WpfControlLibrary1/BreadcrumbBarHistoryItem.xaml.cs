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

namespace BetterExplorerControls
{
    /// <summary>
    /// Interaction logic for BreadcrumbBarHistoryItem.xaml
    /// </summary>
    public partial class BreadcrumbBarHistoryItem : MenuItem
    {
        public BreadcrumbBarHistoryItem()
        {
            InitializeComponent();
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler DeleteRequested;
        //public event EventHandler MouseDoubleClick;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnDeleteRequested(EventArgs e)
        {
            if (DeleteRequested != null)
                DeleteRequested(this, e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteRequested(EventArgs.Empty);
        }

        private void MenuItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                this.ContextMenu.IsOpen = true;
                ((ContextMenu)this.Parent).IsOpen = true;
                this.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
    }
}
