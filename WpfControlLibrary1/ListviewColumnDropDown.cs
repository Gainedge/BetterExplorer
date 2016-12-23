using System.Windows;
using System.Windows.Controls;

namespace BetterExplorerControls {

	public class ListviewColumnDropDown : ContextMenu {
		/// <summary>DefaultStyleKeyProperty.OverrideMetadata(typeof(ListviewColumnDropDown), new FrameworkPropertyMetadata(typeof(ListviewColumnDropDown)));</summary>
		static ListviewColumnDropDown() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ListviewColumnDropDown), new FrameworkPropertyMetadata(typeof(ListviewColumnDropDown)));
		}

		/// <summary>
		/// Adds a new <see cref="MenuItem"/> to <see cref="Items"/> with <see cref="MenuItem.IsCheckable"/> = true and <see cref="MenuItem.Header"/> = Item
		/// </summary>
		/// <param name="Item">The item that will be put into the header</param>
		public void AddItem(object Item) {
			this.Items.Add(new MenuItem() { IsCheckable = true, Header = Item });
		}
	}
}
