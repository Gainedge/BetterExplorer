using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Fluent;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for RibbonItemListDisplay.xaml
    /// </summary>
    public partial class RibbonItemListDisplay : UserControl
    {
        public RibbonItemListDisplay()
        {
            InitializeComponent();
            ShowMenuArrow = false;
            ShowCheck = false;
        }

        public IRibbonControl source;

        [Category("Common")]
        public ImageSource Icon
        {
            get
            {
                return tIcon.Source;
            }
            set
            {
                tIcon.Source = value;
            }
        }

        [Category("Common")]
        public Boolean ShowMenuArrow
        {
            get
            {
                if (MenuArrow.Visibility == System.Windows.Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    MenuArrow.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    MenuArrow.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        [Category("Common")]
        public Boolean ShowCheck
        {
            get
            {
                if (CheckBox.Visibility == System.Windows.Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == true)
                {
                    CheckBox.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    CheckBox.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }


        [Category("Common")]
        public string Header
        {
            get
            {
                return tText.Text;
            }
            set
            {
                tText.Text = value;
            }
        }

        public IRibbonControl SourceControl
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
            }
        }

        private string it = "";

        public string ItemName
        {
            get
            {
                return it;
            }
            set
            {
                it = value;
            }
        }

    }
}
