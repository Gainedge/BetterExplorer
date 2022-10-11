using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFUI.Common;

public static class Extensions {
  public static T? GetLParam<T>(this IntPtr lp) {
    var lparam = Marshal.PtrToStructure(lp, typeof(T));
    if (lparam == null) {
      return default(T);
    }

    return (T)lparam;
  }
}
