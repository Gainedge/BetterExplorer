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
using System.Threading;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for PictureViewerPopup.xaml
    /// </summary>
    public partial class PictureViewerPopup : Window
    {
        public PictureViewerPopup()
        {
            InitializeComponent();
        }
        public void Disposeimg()
        {
            if (bmp != null)
            {
                bmp = null;
                imgPicture.Source = null;
            }
        }
        BitmapImage bmp;
        public void SetImage (string imagepath)
        {
            
            bmp = new BitmapImage(new Uri(imagepath));
            imgPicture.Source = bmp ;

        }
    }
}
