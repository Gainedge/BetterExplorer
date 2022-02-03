using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using WPFUI.Background;
using WPFUI.Common;
using WPFUI.Win32;

namespace WPFUI.Controls {
  public class UIWindow : Window {
    public IntPtr Handle { get; set; }
    private WindowChrome _chrome { get; set; }
    public static readonly DependencyProperty GlassMarginProperty =
      DependencyProperty.Register(
        name: "GlassMargin",
        propertyType: typeof(Thickness),
        ownerType: typeof(UIWindow),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Thickness(-1))
      );
    public Boolean IsLoadMaximize { get; set; }
    public Thickness GlassMargin {
      get => (Thickness)GetValue(GlassMarginProperty);
      set => SetValue(GlassMarginProperty, value);
    }
    static UIWindow() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(UIWindow), new FrameworkPropertyMetadata(typeof(UIWindow)));
    }

    public UIWindow() {
      this.Style = FindResource("UiWindow") as Style;
      this.IsLoadMaximize = false;
      this.StateChanged += OnStateChanged;
    }
    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);
    private void OnStateChanged(object? sender, EventArgs e) {
      if (this.WindowState == WindowState.Maximized) {
        this._chrome.ResizeBorderThickness = new Thickness(0);
        //this.WindowStyle = WindowStyle.None;
        //User32.SetWindowLong(this.Handle, -16, User32.GetWindowLong(this.Handle, -16) | 0x00C00000);
        //SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0020 | 0x0001);
        //User32.SetWindowLong(this.Handle, -16, User32.GetWindowLong(this.Handle, -16) | 0x00C00000);
        this.AdjustWindowSizeAndPosForMaximize();
      } else {
        //this.WindowStyle = WindowStyle.SingleBorderWindow;
        this._chrome.ResizeBorderThickness = new Thickness(4);
      }
      //if (this._chrome.GlassFrameThickness.Top > -1) {
      //  if (this.WindowState == WindowState.Maximized) {
      //    this._chrome.GlassFrameThickness = new Thickness(this._chrome.GlassFrameThickness.Left,
      //      this._chrome.GlassFrameThickness.Top + 8, this._chrome.GlassFrameThickness.Right,
      //      this._chrome.GlassFrameThickness.Bottom + 8);
      //    //this._chrome.NonClientFrameEdges = NonClientFrameEdges.Left | NonClientFrameEdges.Right | NonClientFrameEdges.Bottom;
      //  } else {
      //    this._chrome.GlassFrameThickness = new Thickness(this._chrome.GlassFrameThickness.Left,
      //      this._chrome.GlassFrameThickness.Top - 8, this._chrome.GlassFrameThickness.Right,
      //      this._chrome.GlassFrameThickness.Bottom - 8);
      //    //this._chrome.NonClientFrameEdges = NonClientFrameEdges.None;
      //  }
      //}
    }

    public void AdjustWindowSizeAndPosForMaximize() {
      IntPtr monitor = User32.MonitorFromWindow(this.Handle, User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
      if (monitor != IntPtr.Zero) {
        var monitorInfo = User32.GetMonitorInfo(monitor);
        if (!this.IsLoadMaximize) {
          User32.MoveWindow(this.Handle, monitorInfo.rcWork.left, 0, monitorInfo.rcWork.Width,
              monitorInfo.rcWork.Height, false);
        } else {
          this.IsLoadMaximize = false;
          //this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(
          //  () => {
          //    User32.MoveWindow(this.Handle, monitorInfo.rcWork.left, 0, monitorInfo.rcWork.Width,
          //        monitorInfo.rcWork.Height, false);
          //    //SetWindowPos(this.Handle, IntPtr.Zero, monitorInfo.rcWork.left, 0, monitorInfo.rcWork.Width,
          //    //  monitorInfo.rcWork.Height, 0x0040 | 0x4000);
          //  }));
          var t = new Thread((() => {
            User32.MoveWindow(this.Handle, monitorInfo.rcWork.left, 0, monitorInfo.rcWork.Width,
              monitorInfo.rcWork.Height, false);
          }));
          t.Start();
        }
      }
    }

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);

    [Flags]
    public enum DwmWindowAttribute : uint {
      DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
      DWMWA_MICA_EFFECT = 1029,
      DWMWA_WINDOW_CORNER_PREFERENCE = 33,
      DWMWA_SYSTEMBACKDROP_TYPE = 38
    }
    protected override void OnSourceInitialized(EventArgs e) {
      base.OnSourceInitialized(e);
      //WPFUI.Theme.Manager.Switch(WPFUI.Theme.Style.Dark, true);
      var hwnd = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
      if (hwnd != null) {
        this.Handle = hwnd.Handle;
        this._chrome = new WindowChrome();
        this._chrome.GlassFrameThickness = this.GlassMargin;
        this._chrome.ResizeBorderThickness = new Thickness(this.ResizeMode == ResizeMode.NoResize ? 0 : 4);
        this._chrome.CornerRadius = new CornerRadius(0);
        this._chrome.UseAeroCaptionButtons = true;
        //this._chrome.NonClientFrameEdges = NonClientFrameEdges.None;
        //this._chrome.NonClientFrameEdges = NonClientFrameEdges.Left | NonClientFrameEdges.Right | NonClientFrameEdges.Bottom;
        this._chrome.CaptionHeight = SystemParameters.CaptionHeight;
        WindowChrome.SetWindowChrome(this, this._chrome);
        hwnd.CompositionTarget.BackgroundColor = Colors.Transparent;
        this.Background = Brushes.Transparent;
        WPFUI.Theme.Manager.Switch(this, WPFUI.Theme.Style.Dark, true);
        //Manager.Apply(BackgroundType.Mica, this);
        //Manager.ApplyDarkMode(this);
        //SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0020 | 0x0001 | 0x0002);
        //int trueValue = 0x01;
        //int falseValue = 0x00;

        //// Set dark mode before applying the material, otherwise you'll get an ugly flash when displaying the window.
        //var res = DwmSetWindowAttribute(hwnd.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue,
        //    Marshal.SizeOf(typeof(int)));


        ////var res1 = DwmSetWindowAttribute(hwnd.Handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
        //var flag = 3;
        //var res1 = DwmSetWindowAttribute(hwnd.Handle, DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref flag, Marshal.SizeOf(typeof(int)));
        //
      }

      //WPFUI.Background.Manager.ApplyDarkMode(this.Handle);
      //WPFUI.Background.Manager.Apply(WPFUI.Background.BackgroundType.Mica, this.Handle);
      hwnd.AddHook(WndProcHooked);
    }
    [StructLayout(LayoutKind.Sequential)]
    struct WindowPos {
      public IntPtr hwnd;
      public IntPtr hwndInsertAfter;
      public int x;
      public int y;
      public int width;
      public int height;
      public uint flags;
    }
    private IntPtr WndProcHooked(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      if (msg == 0x0084) {
        //var wpos = (WindowPos)Marshal.PtrToStructure(lParam, typeof(WindowPos));
        //wpos.width = (Int32)this.ActualWidth;
        //wpos.height = (Int32)this.ActualHeight;
        //Marshal.StructureToPtr(wpos, lParam, true);
        //handled = true;
        //return new IntPtr((int)HT.MAXBUTTON);
      }
      if (msg == 0x0024) {
        HwndSource source = HwndSource.FromHwnd(hwnd);
        Matrix matrix = source.CompositionTarget.TransformFromDevice;
        User32.MINMAXINFO mmi = (User32.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(User32.MINMAXINFO));
        // Adjust the maximized size and position to fit the work area of the correct monitor
        IntPtr monitor = User32.MonitorFromWindow(hwnd, User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
        //User32.SetWindowLong(this.Handle, -16, User32.GetWindowLong(this.Handle, -16) & ~0x00C00000);
        if (monitor != IntPtr.Zero) {
          var monitorInfo = User32.GetMonitorInfo(monitor);
          var rcWorkArea = monitorInfo.rcWork;
          var rcMonitorArea = monitorInfo.rcMonitor;
          Point dpiIndependentSize =
            matrix.Transform(new Point(
              monitorInfo.rcWork.right - monitorInfo.rcWork.left,
              monitorInfo.rcWork.bottom - monitorInfo.rcWork.top
            ));

          //Convert minimum size
          Point dpiIndenpendentTrackingSize = matrix.Transform(new Point(
            this.MinWidth,
            this.MinHeight
          ));

          //Set the maximized size of the window
          mmi.ptMaxSize.x = (int)dpiIndependentSize.X;
          mmi.ptMaxSize.y = (int)dpiIndependentSize.Y;

          //Set the position of the maximized window
          mmi.ptMaxPosition.x = 0;
          mmi.ptMaxPosition.y = 1;

          //Set the minimum tracking size
          mmi.ptMinTrackSize.x = (int)dpiIndenpendentTrackingSize.X;
          mmi.ptMinTrackSize.y = (int)dpiIndenpendentTrackingSize.Y;
        }

        Marshal.StructureToPtr(mmi, lParam, true);
        //handled = true;
      }

      if (msg == 0x0083) {
        handled = true;
        if (wParam != IntPtr.Zero && this.WindowState != WindowState.Maximized) {
          var rc = (SnapLayout.RECT)Marshal.PtrToStructure(lParam, typeof(SnapLayout.RECT));

          // We have to add or remove one pixel on any side of the window to force a flicker free resize.
          // Removing pixels would result in a smaller client area.
          // Adding pixels does not seem to really increase the client area.
          rc.bottom += 1;
          //rc.right += 1;

          Marshal.StructureToPtr(rc, lParam, true);
          //  handled = true;

        }
      }
      return IntPtr.Zero;
    }

  }
}
