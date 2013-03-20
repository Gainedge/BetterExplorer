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

namespace BetterExplorerControls
{
    /// <summary>
    /// Interaction logic for TranslationComboBoxItem.xaml
    /// </summary>
    public partial class TranslationComboBoxItem : UserControl
    {
        public TranslationComboBoxItem()
        {
            InitializeComponent();
        }

        private bool rtlused = false;

        public bool UsesRTL
        {
            get
            {
                return rtlused;
            }
            set
            {
                rtlused = value;
            }
        }

        public string LanguageText
        {
            get
            {
                return this.LanguageTextBlock.Text;
            }
            set
            {
                this.LanguageTextBlock.Text = value;
            }
        }

        public string LocaleCode
        {
            get
            {
                return this.LocaleTextBlock.Text;
            }
            set
            {
                this.LocaleTextBlock.Text = value;
            }
        }

        public ImageSource CountryFlag
        {
            get
            {
                return this.LangFlag.Source;
            }
            set
            {
                this.LangFlag.Source = value;
            }
        }

    }
}
