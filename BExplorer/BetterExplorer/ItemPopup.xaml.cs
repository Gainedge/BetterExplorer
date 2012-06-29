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
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for ItemPopup.xaml
    /// </summary>
    public partial class ItemPopup : Window
    {
        public ItemPopup()
        {
            InitializeComponent();
        }
        
        public void ShowAtPosition(Point p, Brush cl = null, string FilePath = "")
        {
            this.Background = cl ?? Brushes.Red;
            this.Left = p.X - this.Width;
            this.Top = p.Y - this.Height;
            this.Visibility = System.Windows.Visibility.Visible;
            ExplorerBrowser.PopFX = (int)this.Left;
            ExplorerBrowser.PopFY = (int)this.Top;

        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            ExplorerBrowser.IsBool = true;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            ExplorerBrowser.IsBool = false;
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ExplorerBrowser.IsBool = true;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Sorry this function is not implemented yet!\r\nPlease wait to next version!");
        }
    }
}
