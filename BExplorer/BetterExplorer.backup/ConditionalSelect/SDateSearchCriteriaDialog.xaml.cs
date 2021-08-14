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

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for SDateSearchCriteriaDialog.xaml
	/// </summary>
	public partial class SDateSearchCriteriaDialog : Window {
		public bool Confirm = false;

		public string DateCriteria {
			get {
				return GetDateCriteria((dcfilter.SelectedItem as ConditionalSelectComboBoxItem).IdentifyingName, dcquery.SelectedDate, datePicker2.SelectedDate);
			}
			set {
				//UpdateCurrentValue(value);
				CurVal.Text = FindResource("txtCurrentValue") + ": " + value;
			}
		}

		public SDateSearchCriteriaDialog(string displayname) {
			InitializeComponent();
			//SetFilterName(displayname);


			string title = FindResource("txtSetFilter") as string;
			textBlock1.Text = title.Replace("(VAL)", displayname);
		}

		//public void UpdateCurrentValue(string value) {
		//	CurVal.Text = FindResource("txtCurrentValue") + ": " + value;
		//}

		//public void SetFilterName(string name) {
		//	string title = FindResource("txtSetFilter") as string;
		//	textBlock1.Text = title.Replace("(VAL)", name);
		//}

		public string GetDateCriteria(string str, DateTime? par1, DateTime? par2) {
			DateTime dat1, dat2;

			if (par1.HasValue)
				dat1 = par1.Value;
			else
				return "";

			if (par2.HasValue)
				dat2 = par2.Value;
			else if (str == "Between")
				return dat1.ToShortDateString();
			else
				dat2 = new DateTime();


			switch (str) {
				case "Earlier":
					return "<" + dat1.ToShortDateString();
				case "Later":
					return ">" + dat1.ToShortDateString();
				case "Equals":
					return dat1.ToShortDateString();
				case "Between":
					DateTime smallbound, largebound;
					if (dat2 > dat1) {
						smallbound = dat1;
						largebound = dat2;
					}
					else if (dat1 < dat2) {
						smallbound = dat2;
						largebound = dat1;
					}
					else {
						return dat1.ToShortDateString();
					}

					return smallbound.ToShortDateString() + ".." + largebound.ToShortDateString();
				default:
					return "";
			}
		}

		private void dcfilter_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (datePicker2 != null) {
				this.datePicker2.IsEnabled = (e.AddedItems[0] as ConditionalSelectComboBoxItem).IdentifyingName == "Between";
			}
		}

		private void button2_Click(object sender, RoutedEventArgs e) {
			Confirm = true;
			this.Close();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			Confirm = false;
			this.Close();
		}
	}
}
