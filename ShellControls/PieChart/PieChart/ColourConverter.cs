using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using BaseWPFHelpers;
using BetterExplorer.PieChart;

namespace ShellControls.PieChart.PieChart {
	/// <summary>
	/// Converter which uses the IColorSelector associated with the Legend to
	/// select a suitable color for rendering an item.
	/// </summary>
	[ValueConversion(typeof(object), typeof(Brush))]
	public class ColourConverter : IValueConverter {
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture) {
			// find the item 
			FrameworkElement element = (FrameworkElement)value;
			object item = element.Tag;

			// find the item container
			DependencyObject container = (DependencyObject)Helpers.FindElementOfTypeUp(element, typeof(ListBoxItem));

			// locate the items control which it belongs to
			ItemsControl owner = ItemsControl.ItemsControlFromItemContainer(container);

			// locate the legend
			Legend legend = (Legend)Helpers.FindElementOfTypeUp(owner, typeof(Legend));

			CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(owner.DataContext);

			// locate this item (which is bound to the tag of this element) within the collection
			int index = collectionView.IndexOf(item);

			if (legend.ColorSelector != null)
				return legend.ColorSelector.SelectBrush(item, index);
			else
				return Brushes.Black;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

}
