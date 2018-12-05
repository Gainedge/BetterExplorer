using System.Drawing;
using Color = System.Windows.Media.Color;

namespace Settings {
  using System;

  public static class Extensions {
    public static Boolean ToBoolean(this Object value) {
      if (value is Int32) {
        return (Int32?)value == 1;
      }
      else if (value is String) {
        return Boolean.Parse(value.ToString());
      }
      return (Boolean)value;
    }

    public static System.Drawing.Color ToDrawingColor(this Color color) {
      return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static IntPtr ToWin32Color(this System.Drawing.Color color) {
      return (IntPtr)(UInt32)ColorTranslator.ToWin32(color);
    }
  }
}