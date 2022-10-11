using System;
using System.Windows.Data;
using System.Globalization;

namespace BetterExplorer.PieChart
{
    [ValueConversion(typeof(double), typeof(string))]
    public class FileSizeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = (double)value;
            int unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return String.Format("{0:0.#} {1}", size, units[unit]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
