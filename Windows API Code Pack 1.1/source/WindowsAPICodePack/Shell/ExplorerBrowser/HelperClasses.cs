using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsAPICodePack.Shell.ExplorerBrowser
{
  public class LVItemColor
  {
    public String ExtensionList { get; private set; }
    public Color TextColor { get; private set; }

    public LVItemColor(String extensions, Color textColor)
    {
      this.ExtensionList = extensions;
      this.TextColor = textColor;
    }
  }
}
