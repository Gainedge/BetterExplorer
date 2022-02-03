using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows.Threading;
using BetterExplorerControls.Helpers;
using ControlzEx.Standard;
using Point = System.Windows.Point;

namespace BetterExplorerControls {
  public class AcrylicContextMenu : Window {
    static AcrylicContextMenu() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicContextMenu), new FrameworkPropertyMetadata(typeof(AcrylicContextMenu)));
    }

    public AcrylicContextMenu() {
      //this.AllowsTransparency = true;
      //this.Background = System.Windows.Media.Brushes.Transparent;
      //this.WindowStyle = WindowStyle.None;
      this.ShowInTaskbar = false;
      this.Width = 300;
      //this.Height = 500;
      this.ShowActivated = false;
      this.Focusable = false;
      //this.Deactivated += (sender, args) => this.Close();
      this.MouseRightButtonUp += (sender, args) => MessageBox.Show("Click");
      this.Loaded += OnLoaded;
      this.Closed += (sender, args) => MouseHook.stop();

    }

    private void MouseHookOnMouseAction(object? sender, LLMouseHookArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
        () => {
          try {
            var screenCoordinates = this.PointToScreen(new Point(0, 0));
            var clientRectangle = new Rectangle((int)screenCoordinates.X, (int)screenCoordinates.Y, (int)this.Width,
              (int)this.Height);
            if (!clientRectangle.Contains(e.pt.x, e.pt.y)) {
              this.Close();
            }
          } catch (Exception exception) {
            this.Close();
          }
        }));

    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
      var thread = new Thread(() => {
        MouseHook.Start();
        System.Windows.Threading.Dispatcher.Run();
      });
      thread.Start();
      MouseHook.MouseAction += MouseHookOnMouseAction;
    }

    protected override void OnSourceInitialized(EventArgs e) {
      base.OnSourceInitialized(e);
      var hwnd = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
      var newChrome = new WindowChrome();
      newChrome.GlassFrameThickness = new Thickness(0, 1, 0, 0);
      newChrome.ResizeBorderThickness = new Thickness(0);
      newChrome.CornerRadius = new CornerRadius(0);
      newChrome.CaptionHeight = 0;
      newChrome.UseAeroCaptionButtons = true;
      WindowChrome.SetWindowChrome(this, newChrome);
      AcrylicHelper.SetBlur(hwnd.Handle, AcrylicHelper.AccentFlagsType.Window, AcrylicHelper.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, 0x00282828);
      var exStyle = NativeMethods.GetWindowStyleEx(hwnd.Handle);

      exStyle |= WS_EX.TOOLWINDOW;
      NativeMethods.SetWindowStyleEx(hwnd.Handle, exStyle);

      var style = NativeMethods.GetWindowStyle(hwnd.Handle);

      style |= WS.POPUP;
      NativeMethods.SetWindowStyle(hwnd.Handle, style);
      hwnd.AddHook(WndProcHooked);

    }

    private IntPtr WndProcHooked(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled) {

      if (msg == 0x0021) {
        handled = true;
        return new IntPtr(0x0003);
      } else return IntPtr.Zero;
    }
  }
}
