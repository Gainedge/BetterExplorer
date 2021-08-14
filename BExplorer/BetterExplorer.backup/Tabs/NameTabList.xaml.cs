using System;
using System.Windows;
using System.Windows.Input;

namespace BetterExplorer.Tabs {

	/// <summary>
	/// Interaction logic for NameTabList.xaml
	/// </summary>
	public partial class NameTabList : Window {
		public bool dialogresult = false;

		private NameTabList() {
			InitializeComponent();
		}

		public static String Open(Window Owner) {
			var f = new NameTabList();
			f.Owner = Owner;
			f.ShowDialog();

			if (f.dialogresult) {
				return f.textBox1.Text;
			}
			else {
				return null;
			}
		}

		private void button2_Click(object sender, RoutedEventArgs e) {
			if (textBox1.Text.Contains("\\") || textBox1.Text.Contains("/") || textBox1.Text.Contains(":") || textBox1.Text.Contains("?") || textBox1.Text.Contains("*") || textBox1.Text.Contains("<") || textBox1.Text.Contains(">") || textBox1.Text.Contains("|")) {
				MessageBox.Show(FindResource("txtIllegalCharacters") as string, "Choose a new name", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else {
				dialogresult = true;
				this.Close();
			}
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			dialogresult = false;
			this.Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			textBox1.Focus();
		}

		private void textBox1_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				dialogresult = true;
				this.Close();
			}
		}
	}
}