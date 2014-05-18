using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using Fluent;

namespace BetterExplorer {

	/// <summary> Interaction logic for CustomizeQAT.xaml </summary>
	public partial class CustomizeQAT : Window {
		public MainWindow MainForm;

		#region Helpers

		private RibbonItemListDisplay GetRibbonItemListDisplay(IRibbonControl item) {
			var rils = new RibbonItemListDisplay() {
				SourceControl = item,
				HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
				Header = (item.Header as string),
				ItemName = (item as FrameworkElement).Name
			};

			if (item.Icon != null) {
				rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + item.Icon.ToString(), UriKind.Relative));
			}

			if (item is Fluent.DropDownButton || item is Fluent.SplitButton || item is Fluent.InRibbonGallery) {
				rils.ShowMenuArrow = true;
			}
			else if (item is Fluent.CheckBox) {
				rils.ShowCheck = true;
			}

			return rils;
		}


		private void RefreshQATDialog(Ribbon ribbon) {
			foreach (IRibbonControl item in GetNonQATButtons(ribbon)) {
				//RibbonItemListDisplay rils = new RibbonItemListDisplay();
				//if (item.Icon != null) {
				//	rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + item.Icon.ToString(), UriKind.Relative));
				//}
				//rils.Header = (item.Header as string);
				//rils.SourceControl = item;
				//rils.ItemName = (item as FrameworkElement).Name;
				//rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
				//if (item is Fluent.DropDownButton || item is Fluent.SplitButton || item is Fluent.InRibbonGallery) {
				//	rils.ShowMenuArrow = true;
				//}
				//AllControls.Items.Add(rils);
				AllControls.Items.Add(GetRibbonItemListDisplay(item));
			}

			var AllMenuItems = MainForm.TheRibbon.QuickAccessItems.Select(x => x.Target).ToList();
			var Controls = (from control in MainForm.TheRibbon.QuickAccessToolbarItems
							select control.Key as Control into newControl
							where !AllMenuItems.Any(x => x.Name == newControl.Name)
							select newControl).ToList();

			AllMenuItems.AddRange(Controls);

			foreach (var item in AllMenuItems) {
				//var rils = new RibbonItemListDisplay() {
				//	ItemName = item.Name,
				//	HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
				//	Header = (item as IRibbonControl).Header as string,
				//	SourceControl = (item as IRibbonControl)
				//};

				//if ((item as IRibbonControl).Icon != null) {
				//	rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + (item as IRibbonControl).Icon.ToString(), UriKind.Relative));
				//}

				////TODO: Find out if I need the following [if(...)]
				//if (item is Fluent.DropDownButton || item is Fluent.SplitButton || item is Fluent.InRibbonGallery) {
				//	rils.ShowMenuArrow = true;
				//}

				//QATControls.Items.Add(rils);
				QATControls.Items.Add(GetRibbonItemListDisplay(item as IRibbonControl));
			}
		}

		private List<Fluent.IRibbonControl> GetNonQATButtons(Ribbon ribbon) {
			var rb = new List<Fluent.IRibbonControl>();
			foreach (RibbonGroupBox itg in ribbon.Tabs.SelectMany(x => x.Groups)) {
				foreach (object ic in itg.Items) {
					if (ic is IRibbonControl) {
						if (!(ribbon.IsInQuickAccessToolBar(ic as UIElement))) {
							rb.Add(ic as IRibbonControl);
						}
					}
				}
			}

			return rb.OrderBy(x => x.Header as string).ToList();
		}

		public List<Fluent.IRibbonControl> GetAllButtons() {
			var rb = new List<Fluent.IRibbonControl>();
			foreach (RibbonGroupBox itg in MainForm.TheRibbon.Tabs.SelectMany(x => x.Groups)) {
				foreach (object ic in itg.Items) {
					if (ic is IRibbonControl) {
						rb.Add(ic as IRibbonControl);
					}
				}
			}

			return rb;
		}

		private void AddToList(RibbonItemListDisplay source, bool qatlist = true) {
			//IRibbonControl item = source.SourceControl;
			//RibbonItemListDisplay rils = new RibbonItemListDisplay();
			//rils.Header = (item.Header as string);
			//rils.SourceControl = item;
			//rils.ItemName = (item as FrameworkElement).Name;
			//rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
			//if (item.Icon != null) {
			//	rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + item.Icon.ToString(), UriKind.Relative));
			//}

			//if (item is Fluent.DropDownButton || item is Fluent.SplitButton) {
			//	rils.ShowMenuArrow = true;
			//}
			//else if (item is Fluent.CheckBox) {
			//	rils.ShowCheck = true;
			//}


			if (qatlist) {
				this.QATControls.Items.Add(GetRibbonItemListDisplay(source.SourceControl));
			}
			else {
				this.AllControls.Items.Add(GetRibbonItemListDisplay(source.SourceControl));
			}
		}

		public void CheckAgainstList() {
			var torem = new List<RibbonItemListDisplay>();
			foreach (RibbonItemListDisplay item in this.QATControls.Items) {
				foreach (RibbonItemListDisplay thing in this.AllControls.Items) {
					if (thing.ItemName == item.ItemName) {
						torem.Add(thing);
					}
				}
			}

			foreach (RibbonItemListDisplay item in torem) {
				this.AllControls.Items.Remove(item);
			}
		}

		#endregion Helpers

		#region Buttons

		private void btnAdd_Click(object sender, RoutedEventArgs e) {
			int sel = AllControls.SelectedIndex;
			RibbonItemListDisplay item = AllControls.SelectedValue as RibbonItemListDisplay;
			AllControls.Items.Remove(item);
			AddToList(item, true);


			CheckAgainstList();
			if (sel != 0) {
				AllControls.SelectedIndex = sel - 1;
			}
			else if (AllControls.Items.Count != 0) {
				AllControls.SelectedIndex = 0;
			}
			else {
				btnRemove.IsEnabled = true;
				btnAdd.IsEnabled = false;
			}
		}

		private void btnRemove_Click(object sender, RoutedEventArgs e) {
			int sel = QATControls.SelectedIndex;
			var item = QATControls.SelectedValue as RibbonItemListDisplay;
			QATControls.Items.Remove(item);
			this.AllControls.Items.Clear();
			foreach (IRibbonControl thing in GetAllButtons()) {
				//var rils = new RibbonItemListDisplay();
				//if (thing.Icon != null) {
				//	rils.Icon = new BitmapImage(new Uri(@"/BetterExplorer;component/" + thing.Icon.ToString(), UriKind.Relative));
				//}
				//rils.Header = (thing.Header as string);
				//rils.SourceControl = thing;
				//rils.ItemName = (thing as FrameworkElement).Name;
				//rils.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

				//if (thing is Fluent.DropDownButton || thing is Fluent.SplitButton) {
				//	rils.ShowMenuArrow = true;
				//}
				//else if (thing is Fluent.CheckBox) {
				//	rils.ShowCheck = true;
				//}

				this.AllControls.Items.Add(GetRibbonItemListDisplay(thing));
			}

			CheckAgainstList();
			if (sel != 0) {
				QATControls.SelectedIndex = sel - 1;
			}
			else if (QATControls.Items.Count != 0) {
				QATControls.SelectedIndex = 0;
			}
			else {
				btnRemove.IsEnabled = false;
				btnAdd.IsEnabled = true;
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void btnMoveUp_Click(object sender, RoutedEventArgs e) {
			if (QATControls.SelectedIndex != 0) {
				int sel = QATControls.SelectedIndex;
				Object oItem = QATControls.Items.GetItemAt(sel);
				QATControls.Items.RemoveAt(sel);
				QATControls.Items.Insert(sel - 1, oItem);
				QATControls.SelectedIndex = QATControls.Items.IndexOf(oItem);
				QATControls.ScrollIntoView(QATControls.SelectedItem);
			}
		}

		private void btnMoveDown_Click(object sender, RoutedEventArgs e) {
			if (QATControls.SelectedIndex != (QATControls.Items.Count - 1)) {
				int sel = QATControls.SelectedIndex;
				Object oItem = QATControls.Items.GetItemAt(sel);
				QATControls.Items.RemoveAt(sel);
				QATControls.Items.Insert(sel + 1, oItem);
				QATControls.SelectedIndex = QATControls.Items.IndexOf(oItem);
				QATControls.ScrollIntoView(QATControls.SelectedItem);
			}
		}

		private void btnApply_Click(object sender, RoutedEventArgs e) {
			var Item2 = MainForm.TheRibbon.QuickAccessItems.ToList();
			var list = new List<string>();
			foreach (RibbonItemListDisplay item in this.QATControls.Items) {
				list.Add(item.ItemName);
			}

			MainForm.TheRibbon.ClearQuickAccessToolBar();
			Dictionary<string, IRibbonControl> items = MainForm.GetAllButtonsAsDictionary();
			foreach (string item in list) {
				IRibbonControl ctrl;
				if (items.TryGetValue(item, out ctrl)) {
					MainForm.TheRibbon.AddToQuickAccessToolBar(ctrl as UIElement);
				}
			}
		}

		private void btnOkay_Click(object sender, RoutedEventArgs e) {
			btnApply_Click(sender, e);
			this.Close();
		}

		#endregion Buttons

		private CustomizeQAT() {
			InitializeComponent();
		}

		public static void Open(MainWindow mainWindow, Ribbon ribbon) {
			CustomizeQAT qal = new CustomizeQAT();
			qal.Owner = mainWindow;
			qal.MainForm = mainWindow;
			qal.RefreshQATDialog(ribbon);
			qal.ShowDialog();
		}


	}
}