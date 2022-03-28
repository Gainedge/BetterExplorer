using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using BetterExplorerControls.Helpers;
using ControlzEx.Standard;
using Fluent;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Point = System.Windows.Point;

namespace BetterExplorerControls {
  public class AcrylicContextMenu : ContextMenu {
    static AcrylicContextMenu() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicContextMenu), new FrameworkPropertyMetadata(typeof(AcrylicContextMenu)));
    }

    public AcrylicContextMenu() {
      //this.AllowsTransparency = true;
      //this.Background = System.Windows.Media.Brushes.Transparent;
      //this.WindowStyle = WindowStyle.None;
      //this.Width = 300;
      //this.Height = 300;
      //this.Height = 500;
      //this.Focusable = false;
      ////this.Deactivated += (sender, args) => this.Close();
      //this.MouseRightButtonUp += (sender, args) => MessageBox.Show("Click");
      //this.Loaded += OnLoaded;
      //this.Closed += (sender, args) => MouseHook.stop();

    }

    //private void MouseHookOnMouseAction(object? sender, LLMouseHookArgs e) {
    //  this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
    //    () => {
    //      try {
    //        var screenCoordinates = this.PointToScreen(new Point(0, 0));
    //        var clientRectangle = new Rectangle((int)screenCoordinates.X, (int)screenCoordinates.Y, (int)this.Width,
    //          (int)this.Height);
    //        if (!clientRectangle.Contains(e.pt.x, e.pt.y)) {
    //          this.Close();
    //        }
    //      } catch (Exception exception) {
    //        this.Close();
    //      }
    //    }));

    //}

    protected override void OnOpened(RoutedEventArgs e) {
      base.OnOpened(e);
      var hwnd = (HwndSource)PresentationSource.FromVisual(this);
      hwnd.CompositionTarget.BackgroundColor = Colors.Transparent;
      AcrylicHelper.SetBlur(hwnd.Handle, AcrylicHelper.AccentFlagsType.Window, AcrylicHelper.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, (uint)Application.Current.Resources["SystemAcrylicTint"]);
      var nonClientArea = new RibbonWindow.MARGINS();
      nonClientArea.topHeight = -1;
      nonClientArea.bottomHeight = -1;
      nonClientArea.leftWidth = -1;
      nonClientArea.rightWidth = -1;
      RibbonWindow.DwmExtendFrameIntoClientArea(hwnd.Handle, ref nonClientArea);
      var preference = RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
      DwmSetWindowAttribute(hwnd.Handle, RibbonWindow.DwmWindowAttribute.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));
    }

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern long DwmSetWindowAttribute(IntPtr hwnd,
      RibbonWindow.DwmWindowAttribute attribute,
      ref RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
      uint cbAttribute);

    //protected override void OnSourceInitialized(EventArgs e) {
    //  base.OnSourceInitialized(e);
    //  var hwnd = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
    //  var newChrome = new WindowChrome();
    //  newChrome.GlassFrameThickness = new Thickness(0, 1, 0, 0);
    //  newChrome.ResizeBorderThickness = new Thickness(0);
    //  newChrome.CornerRadius = new CornerRadius(0);
    //  newChrome.CaptionHeight = 0;
    //  newChrome.UseAeroCaptionButtons = true;
    //  WindowChrome.SetWindowChrome(this, newChrome);
    //  AcrylicHelper.SetBlur(hwnd.Handle, AcrylicHelper.AccentFlagsType.Window, AcrylicHelper.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, 0x00282828);
    //  var exStyle = NativeMethods.GetWindowStyleEx(hwnd.Handle);

    //  exStyle |= WS_EX.TOOLWINDOW;
    //  NativeMethods.SetWindowStyleEx(hwnd.Handle, exStyle);

    //  var style = NativeMethods.GetWindowStyle(hwnd.Handle);

    //  style |= WS.POPUP;
    //  NativeMethods.SetWindowStyle(hwnd.Handle, style);
    //  hwnd.AddHook(WndProcHooked);

    //}

    private IntPtr WndProcHooked(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled) {

      if (msg == 0x0021) {
        handled = true;
        return new IntPtr(0x0003);
      } else return IntPtr.Zero;
    }
  }
}
