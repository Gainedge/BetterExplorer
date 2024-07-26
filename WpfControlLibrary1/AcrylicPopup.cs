using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using BetterExplorerControls.Helpers;
using Linearstar.Windows.RawInput;
using WPFUI.Win32;
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
    public Boolean IsSmallRounding { get; set; }
    #endregion

    private Boolean _PositionSet = false;

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
      this._parentPopup.Opened += OnOpened;
      this._parentPopup.Closed += OnClosed;
      this._parentPopup.Placement = this.Placement;
      Popup.CreateRootPopup(this._parentPopup, this);

      var hwnd = (HwndSource)HwndSource.FromVisual(this._parentPopup.Child);
      hwnd.AddHook(WndProcHooked);
      this.Handle = hwnd.Handle;
      hwnd.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
      this.Background = Brushes.Transparent;
      AcrylicHelper.SetBlur(hwnd.Handle, AcrylicHelper.AccentFlagsType.Window, AcrylicHelper.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, (uint)Application.Current.Resources["SystemAcrylicTint"]);
      var nonClientArea = new Dwmapi.MARGINS(new Thickness(-1));
      Dwmapi.DwmExtendFrameIntoClientArea(hwnd.Handle, ref nonClientArea);
      //var preference = this.IsSmallRounding ? RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL : RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
      var preference = (Int32)Dwmapi.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
      Dwmapi.DwmSetWindowAttribute(hwnd.Handle, Dwmapi.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));
    }

    private RawInputReceiverWindow _Window;
    private void OnOpened(Object sender, EventArgs e) {
      dele = new WinEventDelegate(WinEventProc);
      this._winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

      var thread = new Thread(() => {
        MouseHook.Start();
        System.Windows.Threading.Dispatcher.Run();
      });
      thread.Start();
      MouseHook.MouseAction += MouseHookOnMouseAction;
      //if (!this._PositionSet) {
      //  var mousePos = AcrylicPopup.GetMousePosition();
      //  GetWindowRect(this.Handle, out var rect);
      //  this.IsPopupDirectionReversed = rect.Bottom <= (Int32)mousePos.Y;
      //  this._PositionSet = true;
      //}
    }

    private void ParentPopupOnOpened(Object sender, EventArgs e) {
      var mousePos = AcrylicPopup.GetMousePosition();
      GetWindowRect(this.Handle, out var rect);
      this.IsPopupDirectionReversed = rect.Bottom < mousePos.Y;
    }

    private void OnClosed(object? sender, EventArgs e) {
      MouseHook.stop();
      MouseHook.MouseAction -= MouseHookOnMouseAction;
      this.OnMenuClosed?.Invoke(this, EventArgs.Empty);
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
      } else {
        if (!this._PositionSet) {
          var mousePos = AcrylicPopup.GetMousePosition();
          GetWindowRect(this.Handle, out var rect);
          this.IsPopupDirectionReversed = rect.Bottom <= (Int32)mousePos.Y;
          this._PositionSet = true;
        }
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
      var t = new Thread((() => {
        //Thread.Sleep(1);
        this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(
          this.ProcessMouseHookAction));
      }));

      t.Start();
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
    Point mousePos = new Point();
    private IntPtr WndProcHooked(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      
      if (msg == 0x00FF) {

      }
      if (msg == 71) {
        //handled = true;
        if (!this._PositionSet) {
          mousePos = AcrylicPopup.GetMousePosition();
          this._PositionSet = true;
        }

        GetWindowRect(this.Handle, out var rect);
        this.IsPopupDirectionReversed = rect.Bottom <= (Int32)mousePos.Y;
          
      }

      return IntPtr.Zero;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    WinEventDelegate dele = null;
    [DllImport("user32.dll")]
    static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
    private const uint WINEVENT_OUTOFCONTEXT = 0;
    private const uint EVENT_SYSTEM_FOREGROUND = 3;

    [DllImport("user32.dll")]
    static extern bool UnhookWinEvent(IntPtr hWinEventHook);


  }
}
