using System;
using System.Globalization;
using System.Windows.Data;

namespace Demo.Utils
{
	public class ScalarValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var oldValue = Double.Parse(value.ToString(), culture);
			if (parameter != null)
			{
				oldValue *= Double.Parse(parameter.ToString(), culture);
			}
			return oldValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var oldValue = Double.Parse(value.ToString(), culture);
			if (parameter != null)
			{
				oldValue /= Double.Parse(parameter.ToString(), culture);
			}
			return oldValue;
		}
	}
}
