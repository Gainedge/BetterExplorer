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
using System.Drawing;
using Microsoft.WindowsAPICodePack.Shell;
using BExplorer.Shell;
using BExplorer.Shell.Interop;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for ResizeImage.xaml
    /// </summary>
    public partial class ResizeImage : Window
    {
        public int newheight;
        public int newwidth;

        public bool Confirm = false;

        Bitmap cvt;

        bool percsetting = false;

        public ResizeImage()
        {
            InitializeComponent();

            spinner1.Value = 100;
        }

        public ResizeImage(ShellItem file)
        {
            InitializeComponent();

            textBlock1.Text = FindResource("txtFilename") + ": " + file.GetDisplayName(SIGDN.NORMALDISPLAY);
            cvt = new Bitmap(file.ParsingName);
            textBlock2.Text = FindResource("lblHeightCP") + ": " + cvt.Height.ToString();
            textBlock3.Text = FindResource("lblWidthCP") + ": " + cvt.Width.ToString();

            spinner1.Value = 100;

            percsetting = true;

            textBox1.Text = cvt.Width.ToString();
            textBox2.Text = cvt.Height.ToString();

            percsetting = false;
        }

        //public ResizeImage(ShellObject file, string height, string width, string imagename)
        //{
        //    InitializeComponent();

        //    textBlock1.Text = imagename + ": " + file.GetDisplayName(DisplayNameType.Default);
        //    cvt = new Bitmap(file.ParsingName);
        //    textBlock2.Text = height + ": " + cvt.Height.ToString();
        //    textBlock3.Text = width + ": " + cvt.Width.ToString();

        //    spinner1.Value = 100;

        //    percsetting = true;

        //    textBox1.Text = cvt.Width.ToString();
        //    textBox2.Text = cvt.Height.ToString();

        //    percsetting = false;
        //}

        private void spinner1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                newwidth = Convert.ToInt32(Math.Round(Convert.ToDouble(cvt.Width * Convert.ToInt32(spinner1.Value) / 100)));
                newheight = Convert.ToInt32(Math.Round(Convert.ToDouble(cvt.Height * Convert.ToInt32(spinner1.Value) / 100)));

                percsetting = true;

                textBox1.Text = newwidth.ToString();
                textBox2.Text = newheight.ToString();

                percsetting = false;
            }
            catch
            {

            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Confirm = false;
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Confirm = true;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox1 == null || textBox2 == null || percsetting == true)
            {
                return;
            }

            try
            {
                newwidth = Convert.ToInt32(textBox1.Text);
                if (newwidth < 1)
                {
                    throw new ArgumentOutOfRangeException("newwidth", "Cannot be less than 1.");
                }
            }
            catch
            {
                textBox1.Text = "1";
            }

            try
            {
                newheight = Convert.ToInt32(textBox2.Text);
                if (newheight < 1)
                {
                    throw new ArgumentOutOfRangeException("newheight", "Cannot be less than 1.");
                }
            }
            catch
            {
                textBox2.Text = "1";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cvt.Dispose();
        }
    }
}
