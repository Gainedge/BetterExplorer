using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1.Attempt_1 {
	/// <summary>
	/// Interaction logic for ColumnMenu.xaml
	/// </summary>
	public partial class ColumnMenu : Window {
		/// <summary>
		/// Represents the changing of a CheckBox's check state 
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		/// <param name="IsChecked">if set to <c>true</c> [is checked].</param>
		public delegate void CheckChanged(object sender, RoutedEventArgs e, bool IsChecked);

		/// <summary>Occurs when a CheckBox's check has been changed</summary>
		public event CheckChanged OnCheckChanged;

		//public StackPanel Stack { get { return this.sPanel; } }

		public ColumnMenu() {
			InitializeComponent();
		}

		private void Window_Deactivated(object sender, EventArgs e) {
			this.Close();
		}

		public void SetLocation(Control ParentControl) {
			Point ParentLocation = ParentControl.PointToScreen(new Point(0, 0));

			this.Left = ParentLocation.X;
			this.Top = ParentLocation.Y;
		}

		public List<object> CheckedItems() {
			return sPanel.Children.OfType<CheckBox>().Where(x => x.IsChecked.Value).Select(x => x.Content).ToList();
		}

		public ColumnMenu ClearItems() { sPanel.Children.Clear(); return this; }
		public ColumnMenu AddItem(object Item) {
			var CheckBox = new CheckBox();
			CheckBox.Content = Item;



			CheckBox.Checked += CheckBox_Checked;
			CheckBox.Unchecked += CheckBox_Unchecked;

			sPanel.Children.Add(CheckBox);
			return this;
		}

		void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (OnCheckChanged != null) OnCheckChanged(sender, e, false);
		}

		void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e) {
			if (OnCheckChanged != null) OnCheckChanged(sender, e, true);
		}
	}
}
