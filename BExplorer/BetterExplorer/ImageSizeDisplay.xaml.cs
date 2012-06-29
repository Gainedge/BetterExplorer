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

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for ImageSizeDisplay.xaml
    /// </summary>
    public partial class ImageSizeDisplay : UserControl
    {
        public ImageSizeDisplay()
        {
            InitializeComponent();
        }

        public string WidthName
        {
            get
            {
                return textBlock1.Text;
            }
            set
            {
                textBlock1.Text = value;
            }
        }

        public string HeightName
        {
            get
            {
                return textBlock2.Text;
            }
            set
            {
                textBlock2.Text = value;
            }
        }

        public string WidthData
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        public string HeightData
        {
            get
            {
                return textBox2.Text;
            }
            set
            {
                textBox2.Text = value;
            }
        }

    }
}
