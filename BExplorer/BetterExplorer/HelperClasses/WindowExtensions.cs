using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace BetterExplorer {
  #region Helper External Classes

  public static class WindowExtensions {
    public static System.Windows.Forms.IWin32Window GetWin32Window(this Window parent) {
      return new Wpf32Window(parent);
    }
  }
  #endregion
}
