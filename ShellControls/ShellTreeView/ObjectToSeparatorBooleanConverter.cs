using System;
using System.Windows.Data;

namespace ShellControls.ShellTreeView
{
  public class ObjectToSeparatorBooleanConverter : IValueConverter
  {
    public object Convert(
      object value, Type targetType,
      object parameter, System.Globalization.CultureInfo culture)
    {
      return value?.GetType() == typeof(ShellTreeView);            
    }

    public object ConvertBack(
      object value, Type targetType,
      object parameter, System.Globalization.CultureInfo culture)
    {
      // I don't think you'll need this
      throw new Exception("Can't convert back");
    }
  }
}
