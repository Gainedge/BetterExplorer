using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ShellControls.ShellContextMenu {
  public class Base64ImageConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
      string s = value as string;

      if (s == null)
        return null;

      BitmapImage bi = new BitmapImage();

      bi.BeginInit();
      bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(s));
      bi.EndInit();

      return bi;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
