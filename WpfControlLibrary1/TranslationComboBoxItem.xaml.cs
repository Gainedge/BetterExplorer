using System.Windows.Controls;
using System.Windows.Media;

namespace BetterExplorerControls {

	/// <summary>
	/// Interaction logic for TranslationComboBoxItem.xaml
	/// </summary>
	public partial class TranslationComboBoxItem : UserControl {
		public bool UsesRTL { get; set; }

		public string LanguageText {
			get { return this.LanguageTextBlock.Text; }
			set { this.LanguageTextBlock.Text = value; }
		}

		public string LocaleCode {
			get { return this.LocaleTextBlock.Text; }
			set { this.LocaleTextBlock.Text = value; }
		}

		public ImageSource CountryFlag {
			get { return this.LangFlag.Source; }
			set { this.LangFlag.Source = value; }
		}

		public TranslationComboBoxItem() {
			InitializeComponent();
			UsesRTL = false;
		}
	}
}