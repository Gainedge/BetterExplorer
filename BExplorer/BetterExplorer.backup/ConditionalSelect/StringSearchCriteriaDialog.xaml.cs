using System.Windows;

namespace BetterExplorer {

	/// <summary>
	/// Interaction logic for StringSearchCriteriaDialog.xaml
	/// </summary>
	public partial class StringSearchCriteriaDialog : Window {
		public bool Confirm = false;

		/*
		public void SetFilterName(string name) {
			string title = FindResource("txtSetFilter") as string;
			textBlock1.Text = title.Replace("(VAL)", name);
		}
		*/

		public StringSearchCriteriaDialog(string property, string value, string displayname) {
			InitializeComponent();
			//SetFilterName(displayname);

			string title = FindResource("txtSetFilter") as string;
			textBlock1.Text = title.Replace("(VAL)", displayname);

			textBox1.Text = Utilities.GetValueOnly(property, value);
			textBox1.Focus();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			Confirm = true;
			this.Close();
		}

		private void button2_Click(object sender, RoutedEventArgs e) {
			Confirm = false;
			this.Close();
		}
	}
}