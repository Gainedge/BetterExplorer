using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using BaseWPFHelpers;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace BetterExplorer.PieChart
{
    /// <summary>
    /// Obtain the value of the property from the item, which is currently displayed by the pie chart.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class LegendConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // the item which we are displaying is bound to the Tag property
            TextBlock label = (TextBlock)value;
            object item = label.Tag;

            // find the item container
            DependencyObject container = (DependencyObject)Helpers.FindElementOfTypeUp((Visual)value, typeof(ListBoxItem));

            // locate the items control which it belongs to
            ItemsControl owner = ItemsControl.ItemsControlFromItemContainer(container);

            // locate the legend
            Legend legend = (Legend)Helpers.FindElementOfTypeUp(owner, typeof(Legend));
            
            PropertyDescriptorCollection filterPropDesc = TypeDescriptor.GetProperties(item);
            object itemValue = filterPropDesc[legend.PlottedProperty].GetValue(item);
            return itemValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
        
}
