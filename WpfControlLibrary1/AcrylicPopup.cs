using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BetterExplorerControls.Helpers;
using ControlzEx.Standard;
using Fluent;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace BetterExplorerControls {
  public class AcrylicPopup : ContentControl {
    public event EventHandler OnMenuClosed;
    Popup _parentPopup;

    #region Properties
    public bool IsOpen {
      get { return (bool)GetValue(IsOpenProperty); }
      set { SetValue(IsOpenProperty, value); }
    }
    #endregion
    #region DependencyProperties
    //Placement

    public static readonly DependencyProperty PlacementProperty =
                Popup.PlacementProperty.AddOwner(typeof(AcrylicPopup));

    public PlacementMode Placement {
      get { return (PlacementMode)GetValue(PlacementProperty); }
      set { SetValue(PlacementProperty, value); }
    }
    //PlacementTarget
    public static readonly DependencyProperty PlacementTargetProperty =
                Popup.PlacementTargetProperty.AddOwner(typeof(AcrylicPopup));
    public UIElement PlacementTarget {
      get { return (UIElement)GetValue(PlacementTargetProperty); }
      set { SetValue(PlacementTargetProperty, value); }
    }
    //PlacementRectangle
    public static readonly DependencyProperty PlacementRectangleProperty =
                Popup.PlacementRectangleProperty.AddOwner(typeof(AcrylicPopup));
    public Rect PlacementRectangle {
      get { return (Rect)GetValue(PlacementRectangleProperty); }
      set { SetValue(PlacementRectangleProperty, value); }
    }
    //HorizontalOffset
    public static readonly DependencyProperty HorizontalOffsetProperty =
                Popup.HorizontalOffsetProperty.AddOwner(typeof(AcrylicPopup));
    public double HorizontalOffset {
      get { return (double)GetValue(HorizontalOffsetProperty); }
      set { SetValue(HorizontalOffsetProperty, value); }
    }
    //VerticalOffset
    public static readonly DependencyProperty VerticalOffsetProperty =
                Popup.VerticalOffsetProperty.AddOwner(typeof(AcrylicPopup));
    public double VerticalOffset {
      get { return (double)GetValue(VerticalOffsetProperty); }
      set { SetValue(VerticalOffsetProperty, value); }
    }
    public static readonly DependencyProperty IsOpenProperty =
     Popup.IsOpenProperty.AddOwner(
             typeof(AcrylicPopup),
             new FrameworkPropertyMetadata(
                     false,
                     FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                     new PropertyChangedCallback(OnIsOpenChanged)));
    public static readonly DependencyProperty StaysOpenProperty =
      Popup.StaysOpenProperty.AddOwner(
        typeof(AcrylicPopup),
        new FrameworkPropertyMetadata(
          true,
          FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public static readonly DependencyProperty IsPopupDirectionReversedProperty =
      DependencyProperty.Register(
        name: "IsPopupDirectionReversed",
        propertyType: typeof(Boolean),
        ownerType: typeof(AcrylicPopup),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: false)
      );
    public static readonly DependencyProperty CustomPopupPlacementCallbackProperty =
      Popup.CustomPopupPlacementCallbackProperty.AddOwner(
        typeof(AcrylicPopup),
        new FrameworkPropertyMetadata(
          null,
          FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public Boolean IsPopupDirectionReversed {
      get => (Boolean)GetValue(IsPopupDirectionReversedProperty);
      set => SetValue(IsPopupDirectionReversedProperty, value);
    }
    public bool StaysOpen {
      get { return (bool)GetValue(StaysOpenProperty); }
      set { SetValue(StaysOpenProperty, value); }
    }
    public CustomPopupPlacementCallback CustomPopupPlacementCallback {
      get { return (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty); }
      set { SetValue(CustomPopupPlacementCallbackProperty, value); }
    }
    #endregion

    static AcrylicPopup() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicPopup), new FrameworkPropertyMetadata(typeof(AcrylicPopup)));
    }


    public AcrylicPopup() {
      //this.Width = 300;
      //this.Height = 300;
      this.Background = Brushes.Transparent;
      this.Focusable = false;
      this.CustomPopupPlacementCallback = CustomPopupPlacementCallbackLocal;
      //this.Loaded += (sender, args) => this.Height = Double.NaN;
    }


    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var ctrl = (AcrylicPopup)d;
      if ((bool)e.NewValue) {
        //ctrl.Visibility = Visibility.Visible;
        //DoubleAnimation anim = new DoubleAnimation();
        //anim.From = 0;   //Height of the container before animation
        ////anim.To = 460;       //Height of the container after animation
        //anim.SpeedRatio = 5;
        //anim.Completed += (sender, args) => {
        //  ctrl.UpdateLayout();
        //  ctrl.InvalidateMeasure();
        //  ctrl.InvalidateArrange();
        //};
        //ctrl.Height = Double.NaN;
        if (ctrl._parentPopup == null) {
          ctrl.HookupParentPopup();
         // ctrl.IsPopupDirectionReversed = AcrylicPopup.GetIsPopupDirectionReversed(ctrl);
          //ctrl.BeginAnimation(Control.MaxHeightProperty, anim);
        }
      } else {
        GC.WaitForFullGCComplete();
        GC.Collect();
      }
    }

    public static Boolean GetIsPopupDirectionReversed(AcrylicPopup menu) {
      var screenCoordinates = menu.PointToScreen(new Point(0, 0));
      var yBottom = screenCoordinates.Y + menu.ActualHeight;
      var mousePosition = GetMousePosition();

      return mousePosition.Y >= yBottom;
    }
    private void StaysOpenParentPopup(bool newValue) {
      _parentPopup.StaysOpen = newValue;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point {
      public Int32 X;
      public Int32 Y;
    };
    public static Point GetMousePosition() {
      var w32Mouse = new Win32Point();
      GetCursorPos(ref w32Mouse);

      return new Point(w32Mouse.X, w32Mouse.Y);
    }
    private void HookupParentPopup() {
      this._parentPopup = new Popup();
      this._parentPopup.StaysOpen = false;
      this._parentPopup.Closed += OnClosed;
      this._parentPopup.Placement = this.Placement;
      //this._parentPopup.AllowsTransparency = true;
      //this._parentPopup.PopupAnimation = PopupAnimation.Slide;
      Popup.CreateRootPopup(this._parentPopup, this);

      var hwnd = (HwndSource)HwndSource.FromVisual(this._parentPopup.Child);
      hwnd.AddHook(WndProcHooked);
      this.Handle = hwnd.Handle;
      hwnd.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
      this.Background = Brushes.Transparent;
      //SetWindowLong(hwnd.Handle, (int)GetWindowLongFields.GWL_STYLE,  (IntPtr)((long)GetWindowLong(hwnd.Handle, -16) | 0x00800000 | 0x00080000));
      //var res = AnimateWindow(hwnd.Handle, 400, AnimateWindowFlags.AW_SLIDE | AnimateWindowFlags.AW_VER_POSITIVE);
      //var lastError = Marshal.GetHRForLastWin32Error();
      //var exception = Marshal.GetExceptionForHR(lastError).Message;
      AcrylicHelper.SetBlur(hwnd.Handle, AcrylicHelper.AccentFlagsType.Window, AcrylicHelper.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, (uint)Application.Current.Resources["SystemAcrylicTint"]);
      var nonClientArea = new RibbonWindow.MARGINS();
      nonClientArea.topHeight = -1;
      nonClientArea.leftWidth = -1;
      nonClientArea.rightWidth = -1;
      nonClientArea.bottomHeight = -1;
      RibbonWindow.DwmExtendFrameIntoClientArea(hwnd.Handle, ref nonClientArea);
      var preference = RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
      DwmSetWindowAttribute(hwnd.Handle, RibbonWindow.DwmWindowAttribute.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));
      //SetWindowLong(hwnd.Handle, (int)GetWindowLongFields.GWL_STYLE, (IntPtr)(0x04000000 | 0x02000000 | 0x00800000 | 0x00C00000 | 0x10000000));
      //var flag = 3;
      //var res1 = RibbonWindow.DwmSetWindowAttribute(hwnd.Handle, RibbonWindow.DwmWindowAttribute.DWMWA_SYSTEMBACKDROP_TYPE, ref flag, Marshal.SizeOf(typeof(int)));
      //int trueValue = 0x01;
      //var res = RibbonWindow.DwmSetWindowAttribute(hwnd.Handle, RibbonWindow.DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
      dele = new WinEventDelegate(WinEventProc);
      this._winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
      var thread = new Thread(() => {
        MouseHook.Start();
        System.Windows.Threading.Dispatcher.Run();
      });
      thread.Start();
      MouseHook.MouseAction += MouseHookOnMouseAction;
    }

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern long DwmSetWindowAttribute(IntPtr hwnd,
      RibbonWindow.DwmWindowAttribute attribute,
      ref RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
      uint cbAttribute);
    private void OnClosed(object? sender, EventArgs e) {
      this.OnMenuClosed?.Invoke(this, EventArgs.Empty);
      MouseHook.stop();
      UnhookWinEvent(this._winHook);
    }
    private CustomPopupPlacement[] CustomPopupPlacementCallbackLocal(Size popupsize, Size targetsize, Point offset) {
      var placement1 =
        new CustomPopupPlacement(new Point(-((popupsize.Width - targetsize.Width) / 2), targetsize.Height + 5), PopupPrimaryAxis.Horizontal);
      return new[] { placement1 };
    }
    public IntPtr Handle { get; set; }
    private IntPtr _winHook;

    public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
      if (hwnd != this.Handle) {
        this.IsOpen = false;
      }
    }

    protected DependencyObject IsElementPartOfPopup(IInputElement element) {
      var parent = VisualTreeHelper.GetParent((DependencyObject)element);
      while (parent != null && !(parent is AcrylicPopup)) {
        parent = VisualTreeHelper.GetParent(parent);
      }

      if (parent == null) {
        return null;
      }

      return parent;
    }
    private void MouseHookOnMouseAction(object? sender, LLMouseHookArgs e) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
        this.ProcessMouseHookAction));

    }

    protected virtual void ProcessMouseHookAction() {
      try {
        var el = Mouse.DirectlyOver;
        if (el == null || this.IsElementPartOfPopup(el) != this) {
          this.IsOpen = false;
        }
      } catch (Exception exception) {
        this.IsOpen = false;
      }
    }

    private IntPtr WndProcHooked(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      if (msg == 0x0018) {
        //handled = true;
      }
      if (msg == 0x0083) {
        handled = true;
        var rc = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
        // We have to add or remove one pixel on any side of the window to force a flicker free resize.
        // Removing pixels would result in a smaller client area.
        // Adding pixels does not seem to really increase the client area.
        rc.Bottom += 1;

        Marshal.StructureToPtr(rc, lParam, true);
      }

      return IntPtr.Zero;
    }

    public enum GetWindowLongFields {
      // ...
      GWL_STYLE = (-16),
      GWL_EXSTYLE = (-20),
      // ...
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
      int error = 0;
      IntPtr result = IntPtr.Zero;
      // Win32 SetWindowLong doesn't clear error on success
      SetLastError(0);

      if (IntPtr.Size == 4) {
        // use SetWindowLong
        Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
        error = Marshal.GetLastWin32Error();
        result = new IntPtr(tempResult);
      } else {
        // use SetWindowLongPtr
        result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
        error = Marshal.GetLastWin32Error();
      }

      if ((result == IntPtr.Zero) && (error != 0)) {
        throw new System.ComponentModel.Win32Exception(error);
      }

      return result;
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

    private static int IntPtrToInt32(IntPtr intPtr) {
      return unchecked((int)intPtr.ToInt64());
    }

    [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
    public static extern void SetLastError(int dwErrorCode);
    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    WinEventDelegate dele = null;
    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
    private const uint WINEVENT_OUTOFCONTEXT = 0;
    private const uint EVENT_SYSTEM_FOREGROUND = 3;

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32", SetLastError = true)]
    public static extern IntPtr AnimateWindow(IntPtr hwnd, int time, AnimateWindowFlags flags);

    [Flags]
    public enum AnimateWindowFlags : uint {
      AW_HOR_POSITIVE = 0x00000001,
      AW_HOR_NEGATIVE = 0x00000002,
      AW_VER_POSITIVE = 0x00000004,
      AW_VER_NEGATIVE = 0x00000008,
      AW_CENTER = 0x00000010,
      AW_HIDE = 0x00010000,
      AW_ACTIVATE = 0x00020000,
      AW_SLIDE = 0x00040000,
      AW_BLEND = 0x00080000
    }

  }
}
