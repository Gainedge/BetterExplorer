using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BExplorer.Shell {
	public class FilterMenu_Strings : ContextMenu {
		/// <summary>
		/// Represents the changing of a CheckBox's check state 
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		/// <param name="IsChecked">if set to <c>true</c> [is checked].</param>
		public delegate void CheckChanged(object sender, RoutedEventArgs e, bool IsChecked);

		/// <summary>Occurs when a CheckBox's check has been changed</summary>
		public event CheckChanged OnCheckChanged;

		public void SetItems(params string[] Items) {
			SetItems(Items);
		}


		/// <summary>
		/// Sets the items.
		/// </summary>
		/// <param name="Items">The items you want to use.</param>
		public void SetItems(IEnumerable<string> Items) {
			this.Items.Clear();
			foreach (var Item in Items) {
				//this.Height += 25;
				var CheckBox = new CheckBox();
				CheckBox.Content = Item;

				CheckBox.Checked += CheckBox_Checked;
				CheckBox.Unchecked += CheckBox_Unchecked;

				this.Items.Add(CheckBox);
			}
		}

		/// <summary>
		/// Gets the [Content] of the checked CheckBoxes.
		/// </summary>
		/// <returns></returns>
		public List<string> CheckedItems() {
			return this.Items.OfType<CheckBox>().Where(x => x.IsChecked.Value).Select(x => x.Content.ToString()).ToList();
		}

		public void Activate(Control Container) {
			Container.ContextMenu = this;
			Container.ContextMenu.IsEnabled = true;
			Container.ContextMenu.PlacementTarget = Container;
			Container.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
			Container.ContextMenu.IsOpen = true;
		}

		void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
			if (OnCheckChanged != null) OnCheckChanged(sender, e, false);
		}

		void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e) {
			if (OnCheckChanged != null) OnCheckChanged(sender, e, true);
		}
	}
}
