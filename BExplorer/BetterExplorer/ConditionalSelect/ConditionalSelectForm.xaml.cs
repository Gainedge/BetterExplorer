using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BExplorer.Shell;

namespace BetterExplorer {

	/// <summary>
	/// Interaction logic for ConditionalSelectForm.xaml
	/// </summary>
	public partial class ConditionalSelectForm : Window {
		private System.Globalization.CultureInfo ci;
		public bool CancelAction = true;
		public ConditionalSelectData csd;

		private ConditionalSelectForm() {
			InitializeComponent();

			dcquery.SelectedDate = DateTime.Today;
			dmquery.SelectedDate = DateTime.Today;
			daquery.SelectedDate = DateTime.Today;
			sizequery1.Text = "0";
			sizequery2.Text = "0";
			namequery.Text = (FindResource("txtFilename") as string);
			ci = System.Threading.Thread.CurrentThread.CurrentCulture;
		}

		public static void Open(ShellView ShellListView) {
			var csf = new ConditionalSelectForm();
			csf.ShowDialog();
			if (!csf.CancelAction) {
				csf.ConditionallySelectFiles(csf.csd, ShellListView);
			}
		}

		private void ConditionallySelectFiles(ConditionalSelectData csd, ShellView ShellListView) {
			if (csd == null) return;
			var shells = ShellListView.Items.ToList();

			//The following items are added
			var Matches_Name = new List<ShellItem>();
			var Matches_Size = new List<ShellItem>();
			var Matches_DateCreated = new List<ShellItem>();
			var Matches_DateLastModified = new List<ShellItem>();
			var Matches_LastAccessed = new List<ShellItem>();

			ShellListView.DeSelectAllItems();

			if (csd.FilterByFileName) {
				foreach (ShellItem item in shells) {
					var data = new FileInfo(item.ParsingName);
					string ToFind = csd.FileNameData.matchCase ? data.Name : data.Name.ToLowerInvariant();

					switch (csd.FileNameData.filter) {
						case ConditionalSelectParameters.FileNameFilterTypes.Contains:
							if (ToFind.Contains(csd.FileNameData.matchCase ? csd.FileNameData.query : csd.FileNameData.query.ToLowerInvariant())) Matches_Name.Add(item);
							break;

						case ConditionalSelectParameters.FileNameFilterTypes.StartsWith:
							if (ToFind.StartsWith(csd.FileNameData.query)) Matches_Name.Add(item);
							break;

						case ConditionalSelectParameters.FileNameFilterTypes.EndsWith:
							if (ToFind.EndsWith(csd.FileNameData.query)) Matches_Name.Add(item);
							break;

						case ConditionalSelectParameters.FileNameFilterTypes.Equals:
							if (ToFind == csd.FileNameData.query) Matches_Name.Add(item);
							break;

						case ConditionalSelectParameters.FileNameFilterTypes.DoesNotContain:
							if (!ToFind.Contains(csd.FileNameData.query)) Matches_Name.Add(item);
							break;

						case ConditionalSelectParameters.FileNameFilterTypes.NotEqualTo:
							if (ToFind != csd.FileNameData.query) Matches_Name.Add(item);
							break;

						default:
							break;
					}
				}
			}
			else {
				//Matches_Name.AddRange(shells.Where((x) => !Directory.Exists(x.ParsingName)));
			}

			if (csd.FilterByFileSize) {
				foreach (ShellItem item in Matches_Name) {
					FileInfo data = new FileInfo(item.ParsingName);
					switch (csd.FileSizeData.filter) {
						case ConditionalSelectParameters.FileSizeFilterTypes.LargerThan:
							if (data.Length > csd.FileSizeData.query1) Matches_Size.Add(item);
							break;

						case ConditionalSelectParameters.FileSizeFilterTypes.SmallerThan:
							if (data.Length < csd.FileSizeData.query1) Matches_Size.Add(item);
							break;

						case ConditionalSelectParameters.FileSizeFilterTypes.Equals:
							if (data.Length == csd.FileSizeData.query1) Matches_Size.Add(item);
							break;

						case ConditionalSelectParameters.FileSizeFilterTypes.Between:
							long largebound, smallbound;
							if (csd.FileSizeData.query2 > csd.FileSizeData.query1) {
								smallbound = csd.FileSizeData.query1;
								largebound = csd.FileSizeData.query2;
							}
							else if (csd.FileSizeData.query2 < csd.FileSizeData.query1) {
								smallbound = csd.FileSizeData.query2;
								largebound = csd.FileSizeData.query1;
							}
							else {
								if (data.Length == csd.FileSizeData.query1) Matches_Size.Add(item);
								break;
							}

							if (data.Length > smallbound && data.Length < largebound) Matches_Size.Add(item);
							break;

						case ConditionalSelectParameters.FileSizeFilterTypes.NotEqualTo:
							if (data.Length != csd.FileSizeData.query1) Matches_Size.Add(item);
							break;

						case ConditionalSelectParameters.FileSizeFilterTypes.NotBetween:
							long largebound2, smallbound2;
							if (csd.FileSizeData.query2 > csd.FileSizeData.query1) {
								smallbound2 = csd.FileSizeData.query1;
								largebound2 = csd.FileSizeData.query2;
							}
							else if (csd.FileSizeData.query2 < csd.FileSizeData.query1) {
								smallbound2 = csd.FileSizeData.query2;
								largebound2 = csd.FileSizeData.query1;
							}
							else {
								// they are the same, use Unequal code
								if (data.Length != csd.FileSizeData.query1) Matches_Size.Add(item);
								break;
							}

							if (data.Length < smallbound2 || data.Length > largebound2) Matches_Size.Add(item);
							break;

						default:
							break;
					}
				}
			}
			else {
				Matches_Size.AddRange(Matches_Name);
			}

			Func<FileInfo, DateTime> GetCreateDate = (x) => x.CreationTimeUtc;
			foreach (var item in !csd.FilterByDateCreated ? Matches_Size : DateFilter(Matches_Size, csd.DateCreatedData, GetCreateDate)) {
				Matches_DateCreated.Add(item);
			}

			Func<FileInfo, DateTime> GetDateModified = (x) => x.LastWriteTimeUtc;
			foreach (var item in !csd.FilterByDateModified ? Matches_Size : DateFilter(Matches_DateCreated, csd.DateModifiedData, GetDateModified)) {
				Matches_DateLastModified.Add(item);
			}

			Func<FileInfo, DateTime> GetDateAccessed = (x) => x.LastAccessTimeUtc;
			foreach (var item in !csd.FilterByDateAccessed ? Matches_DateLastModified : DateFilter(Matches_DateLastModified, csd.DateAccessedData, GetDateAccessed)) {
				Matches_LastAccessed.Add(item);
			}

			//ShellListView.SelectItems(Matches_LastAccessed.ToArray());
			ShellListView.Focus();
		}

		private List<ShellItem> DateFilter(List<ShellItem> shells, ConditionalSelectParameters.DateParameters filter, Func<FileInfo, DateTime> GetDate) {
			var outshells = new List<ShellItem>();

			foreach (ShellItem item in shells) {
				FileInfo data = new FileInfo(item.ParsingName);
				var Date = GetDate(data);

				switch (filter.filter) {
					case ConditionalSelectParameters.DateFilterTypes.EarlierThan:
						if (DateTime.Compare(Date, filter.queryDate) < 0) outshells.Add(item);
						break;

					case ConditionalSelectParameters.DateFilterTypes.LaterThan:
						if (DateTime.Compare(Date, filter.queryDate) > 0) outshells.Add(item);
						break;

					case ConditionalSelectParameters.DateFilterTypes.Equals:
						if (DateTime.Compare(Date, filter.queryDate) == 0) outshells.Add(item);
						break;

					default:
						break;
				}
			}

			return outshells;
		}

		private void sizecheck_Checked(object sender, RoutedEventArgs e) {
			if (!this.IsLoaded) return;
			this.sizefilter.IsEnabled = true;
			this.sizequery1.IsEnabled = true;
			this.sizequery2.IsEnabled = true;
			this.sizebox1.IsEnabled = true;
			var i = (ConditionalSelectComboBoxItem)sizefilter.SelectedItem;
			if (i.IdentifyingName == "Between" || i.IdentifyingName == "NotBetween") {
				this.sizequery2.IsEnabled = true;
				this.sizebox2.IsEnabled = true;
			}
			else {
				this.sizequery2.IsEnabled = false;
				this.sizebox2.IsEnabled = false;
			}
		}

		private void sizecheck_Unchecked(object sender, RoutedEventArgs e) {
			if (!this.IsLoaded) return;
			this.sizefilter.IsEnabled = false;
			this.sizequery1.IsEnabled = false;
			this.sizequery2.IsEnabled = false;
			this.sizebox1.IsEnabled = false;
			this.sizebox2.IsEnabled = false;
		}

		private void sizefilter_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (!this.IsLoaded) return;
			ConditionalSelectComboBoxItem i = (ConditionalSelectComboBoxItem)e.AddedItems[0];
			if (i.IdentifyingName == "Between" || i.IdentifyingName == "NotBetween") {
				this.sizequery2.IsEnabled = true;
				this.sizebox2.IsEnabled = true;
			}
			else {
				this.sizequery2.IsEnabled = false;
				this.sizebox2.IsEnabled = false;
			}
		}

		private void namecheck_CheckChanged(object sender, RoutedEventArgs e) {
			if (!this.IsLoaded) return;
			this.namefilter.IsEnabled = e.RoutedEvent.Name == "Checked";
			this.namequery.IsEnabled = e.RoutedEvent.Name == "Checked";
			this.namecase.IsEnabled = e.RoutedEvent.Name == "Checked";
		}

		private void dccheck_CheckChanged(object sender, RoutedEventArgs e) {
			if (!this.IsLoaded) return;
			this.dcfilter.IsEnabled = e.RoutedEvent.Name == "Checked";
			this.dcquery.IsEnabled = e.RoutedEvent.Name == "Checked"; ;
		}

		private void dmcheck_CheckChanged(object sender, RoutedEventArgs e) {
			if (!this.IsLoaded) return;
			this.dmfilter.IsEnabled = e.RoutedEvent.Name == "Checked";
			this.dmquery.IsEnabled = e.RoutedEvent.Name == "Checked";
		}

		private void dacheck_CheckChanged(object sender, RoutedEventArgs e) {
			if (!this.IsLoaded) return;
			this.dafilter.IsEnabled = e.RoutedEvent.Name == "Checked";
			this.daquery.IsEnabled = e.RoutedEvent.Name == "Checked";
		}

		private void button2_Click(object sender, RoutedEventArgs e) {
			CancelAction = false;
			var fnf = (ConditionalSelectParameters.FileNameFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.FileNameFilterTypes), ((ConditionalSelectComboBoxItem)namefilter.SelectedItem).IdentifyingName);
			var fsf = (ConditionalSelectParameters.FileSizeFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.FileSizeFilterTypes), ((ConditionalSelectComboBoxItem)sizefilter.SelectedItem).IdentifyingName);
			var sd1 = (FriendlySizeConverter.FileSizeMeasurements)Enum.Parse(typeof(FriendlySizeConverter.FileSizeMeasurements), ((ConditionalSelectComboBoxItem)sizebox1.SelectedItem).IdentifyingName);
			var sd2 = (FriendlySizeConverter.FileSizeMeasurements)Enum.Parse(typeof(FriendlySizeConverter.FileSizeMeasurements), ((ConditionalSelectComboBoxItem)sizebox2.SelectedItem).IdentifyingName);
			var dcf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem)dcfilter.SelectedItem).IdentifyingName);
			var dmf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem)dmfilter.SelectedItem).IdentifyingName);
			var daf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem)dafilter.SelectedItem).IdentifyingName);

			var Part1 = FriendlySizeConverter.GetByteLength(Convert.ToDouble(sizequery1.Text.Replace(",", ci.NumberFormat.NumberDecimalSeparator).Replace(".", ci.NumberFormat.NumberDecimalSeparator)), sd1);
			var Part2 = FriendlySizeConverter.GetByteLength(Convert.ToDouble(sizequery2.Text.Replace(",", ci.NumberFormat.NumberDecimalSeparator).Replace(".", ci.NumberFormat.NumberDecimalSeparator)), sd2);

			csd = new ConditionalSelectData(
				new ConditionalSelectParameters.FileNameParameters(namequery.Text, fnf, namecase.IsChecked.Value),
				new ConditionalSelectParameters.FileSizeParameters(Part1, Part2, fsf),
				new ConditionalSelectParameters.DateParameters(dcquery.SelectedDate.Value.Date, dcf),
				new ConditionalSelectParameters.DateParameters(dmquery.SelectedDate.Value.Date, dmf),
				new ConditionalSelectParameters.DateParameters(daquery.SelectedDate.Value.Date, daf),
				namecheck.IsChecked.Value, sizecheck.IsChecked.Value, dccheck.IsChecked.Value, dmcheck.IsChecked.Value, dacheck.IsChecked.Value);
			this.Close();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			CancelAction = true;
			this.Close();
		}

		private void namequery_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e) {
			if (namequery.IsKeyboardFocused) {
				if (namequery.Text == (FindResource("txtFilename") as string)) {
					namequery.Text = "";
				}
			}
			else {
				if (namequery.Text == "") {
					namequery.Text = (FindResource("txtFilename") as string);
				}
			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e) {
			MessageBox.Show((FindResource("txtSelectFiles") as string), "Resource String");
		}
	}
}