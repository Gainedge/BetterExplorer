using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace BetterExplorer.PieChart
{
    /// <summary>
    /// A value converter that delegates to String.Format
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class FormattingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string formatString = parameter as string;
            if (formatString != null)
            {
                return string.Format(culture, formatString, value);
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
