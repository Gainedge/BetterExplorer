using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BExplorer.Shell
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
