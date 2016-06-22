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

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for RibbonItemListDisplay.xaml
	/// </summary>
	public partial class RibbonItemListDisplay : UserControl {
		public string ItemName { get; set; }
		public IRibbonControl SourceControl { get; set; }

		[Category("Common")]
		public ImageSource Icon {
			get { return tIcon.Source; }
			set { tIcon.Source = value; }
		}

		[Category("Common")]
		public Boolean ShowMenuArrow {
			get { return MenuArrow.Visibility == Visibility.Visible; }
			set { MenuArrow.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
		}

		[Category("Common")]
		public Boolean ShowCheck {
			get { return CheckBox.Visibility == Visibility.Visible; }
			set { CheckBox.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
		}

		[Category("Common")]
		public string Header {
			get { return tText.Text; }
			set { tText.Text = value; }
		}

		public RibbonItemListDisplay() {
			InitializeComponent();
			ItemName = "";
			ShowMenuArrow = false;
			ShowCheck = false;
		}
	}
}
