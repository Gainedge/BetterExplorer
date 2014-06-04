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

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for ConditionalSelectComboBoxItem.xaml
	/// </summary>
	public partial class ConditionalSelectComboBoxItem : TextBlock {
		public string IdentifyingName { get; set; }
		public ConditionalSelectComboBoxItem() {
			InitializeComponent();
		}
	}
}
