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
  }
}