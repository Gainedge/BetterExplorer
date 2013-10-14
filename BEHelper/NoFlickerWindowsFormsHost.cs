using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;

namespace BEHelper
{
  public class NoFlickerWindowsFormsHost : WindowsFormsHost
  {
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_ASYNCWINDOWPOS = 0x4000;

    [DllImport("user32.dll")]
    extern static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


    protected override void OnWindowPositionChanged(Rect rcBoundingBox)
    {
      if (Handle != IntPtr.Zero)
      {
        SetWindowPos(Handle,
            IntPtr.Zero,
            (int)rcBoundingBox.X,
            (int)rcBoundingBox.Y,
            (int)rcBoundingBox.Width,
            (int)rcBoundingBox.Height,
            SWP_ASYNCWINDOWPOS
            | SWP_NOZORDER
            | SWP_NOACTIVATE);
      }
    }
  } 
}
