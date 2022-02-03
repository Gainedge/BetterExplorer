// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using WPFUI.Background;
using WPFUI.Controls;
using WPFUI.Win32;
using Button = System.Windows.Controls.Button;

namespace WPFUI.Common {
  /// <summary>
  /// Brings the Snap Layout functionality from Windows 11 to a custom <see cref="Controls.TitleBar"/>.
  /// </summary>
  internal sealed class SnapLayout {
    private double _dpiScale = 1.2; //7

    private Window _window;

    private TitleBarButton _button;

    public void Register(Window window, TitleBarButton button) {
      _window = window;
      _button = button;


      HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(button);
      //var hwnd = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
      hwnd.CompositionTarget.BackgroundColor = Colors.Transparent;
      if (hwnd != null) {
        //var newChrome = new WindowChrome();
        //newChrome.GlassFrameThickness = new Thickness(0,115,0,0);
        //newChrome.ResizeBorderThickness = new Thickness(4);
        //newChrome.CornerRadius = new CornerRadius(4);
        //newChrome.UseAeroCaptionButtons = true;
        ////newChrome.NonClientFrameEdges = NonClientFrameEdges.Bottom | NonClientFrameEdges.Left | NonClientFrameEdges.Right;
        //if (window != null) {
        //  WindowChrome.SetWindowChrome(window, newChrome);
        //}

        hwnd.AddHook(HwndSourceHook);
      }
    }

    public static bool IsSupported() {
      return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Build > 20000;
    }

    /// <summary>
    /// Represents the method that handles Win32 window messages.
    /// </summary>
    /// <param name="hWnd">The window handle.</param>
    /// <param name="uMsg">The message ID.</param>
    /// <param name="wParam">The message's wParam value.</param>
    /// <param name="lParam">The message's lParam value.</param>
    /// <param name="handled">A value that indicates whether the message was handled. Set the value to <see langword="true"/> if the message was handled; otherwise, <see langword="false"/>.</param>
    /// <returns>The appropriate return value depends on the particular message. See the message documentation details for the Win32 message being handled.</returns>
    private IntPtr HwndSourceHook(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      User32.WM mouseNotification = (User32.WM)uMsg;

      switch (mouseNotification) {
        case User32.WM.NCMOUSEMOVE:

          //_button.RaiseEvent(new MouseEventArgs(null, (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds));

          //_button.IsMouseOver = true;
          //this._button.IsMouseOver = IsOverButton(wParam, lParam);

          handled = true;
          break;

        case User32.WM.NCMOUSELEAVE:
          //_button.RaiseEvent(new RoutedEventArgs(Button.MouseLeaveEvent, _button));
          this._button.IsMouseOver = false;
          handled = true;
          break;
        case User32.WM.NCLBUTTONDOWN:
          if (IsOverButton(wParam, lParam)) {
            handled = true;
            this._button.IsPressed = true;
          }
          break;
        case User32.WM.NCLBUTTONUP:
          if (IsOverButton(wParam, lParam)) {
            RaiseButtonClick();
            this._button.IsMouseOver = false;
            this._button.IsPressed = false;
            handled = true;
          }

          break;

        case User32.WM.NCHITTEST:
          if (IsOverButton(wParam, lParam)) {
            handled = true;

            //if (_window.WindowState == WindowState.Maximized) {
            //  return new IntPtr((int)HT.MINBUTTON);
            //}
            this._button.IsMouseOver = true;
            return new IntPtr((int)HT.MAXBUTTON);
          } else {
            this._button.IsMouseOver = false;
            this._button.IsPressed = false;
          }

          break;

        default:
          handled = false;
          break;
      }

      //if (uMsg == 0x0083) {
      //  handled = true;
      //  if (wParam != IntPtr.Zero && this._window?.WindowState != WindowState.Maximized) {
      //    var rc = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

      //    // We have to add or remove one pixel on any side of the window to force a flicker free resize.
      //    // Removing pixels would result in a smaller client area.
      //    // Adding pixels does not seem to really increase the client area.
      //    rc.bottom += 1;

      //    Marshal.StructureToPtr(rc, lParam, true);
      //    //  handled = true;

      //  }
      //}

      return IntPtr.Zero;
    }

    private bool IsOverButton(IntPtr wParam, IntPtr lParam) {
      try {
        var mp = User32.GetMousePosition();

        Rect rect = new Rect(_button.PointToScreen(new Point()),
            new Size(_button.Width * _dpiScale, _button.Height * _dpiScale));

        if (rect.Contains(mp)) {
          return true;
        }
      } catch (OverflowException) {
        return true;
      }

      return false;
    }

    private void RaiseButtonClick() {
      if (new ButtonAutomationPeer(_button).GetPattern(PatternInterface.Invoke) is IInvokeProvider invokeProv)
        invokeProv?.Invoke();
    }

    /// <summary> Win32 </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT {
      /// <summary> Win32 </summary>
      public int left;
      /// <summary> Win32 </summary>
      public int top;
      /// <summary> Win32 </summary>
      public int right;
      /// <summary> Win32 </summary>
      public int bottom;

      /// <summary> Win32 </summary>
      public static readonly RECT Empty = new RECT();

      /// <summary> Win32 </summary>
      public int Width => Math.Abs(right - left); // Abs needed for BIDI OS

      /// <summary> Win32 </summary>
      public int Height => bottom - top;

      /// <summary> Win32 </summary>
      public RECT(int left, int top, int right, int bottom) {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
      }


      /// <summary> Win32 </summary>
      public RECT(RECT rcSrc) {
        this.left = rcSrc.left;
        this.top = rcSrc.top;
        this.right = rcSrc.right;
        this.bottom = rcSrc.bottom;
      }

      /// <summary> Win32 </summary>
      public bool IsEmpty =>
        // BUGBUG : On Bidi OS (hebrew arabic) left > right
        left >= right || top >= bottom;

      /// <summary> Return a user friendly representation of this struct </summary>
      public override string ToString() {
        if (this == RECT.Empty) { return "RECT {Empty}"; }
        return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
      }

      /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
      public override bool Equals(object obj) {
        if (!(obj is Rect)) { return false; }
        return (this == (RECT)obj);
      }

      /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
      public override int GetHashCode() {
        return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
      }


      /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
      public static bool operator ==(RECT rect1, RECT rect2) {
        return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
      }

      /// <summary> Determine if 2 RECT are different(deep compare)</summary>
      public static bool operator !=(RECT rect1, RECT rect2) {
        return !(rect1 == rect2);
      }
    }
  }
}