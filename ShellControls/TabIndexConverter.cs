using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ShellControls;
public class TabIndexConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    var tabItem = value as TabItem;
    var fromContainer = ItemsControl.ItemsControlFromItemContainer(tabItem)?.ItemContainerGenerator;

    var items = fromContainer?.Items.Cast<TabItem>().Where(x => x.Visibility == Visibility.Visible).ToList();
    var count = items?.Count() ?? 0;

    var index = items?.IndexOf(tabItem) ?? -2;
    if (index == 0)
      return "First";
    else if (count - 1 == index || index == (tabItem.Parent as TabControl)?.SelectedIndex - 1)
      return "Last";
    else
      return "";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return DependencyProperty.UnsetValue;
  }
}
