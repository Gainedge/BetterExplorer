// ReSharper disable once CheckNamespace

using System.Runtime.InteropServices;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;

namespace Fluent {
  using System;
  using System.Windows;
  using System.Windows.Data;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Threading;
  //using ControlzEx.Behaviors;
  using Fluent.Extensions;
  using Fluent.Helpers;
  using Fluent.Internal.KnownBoxes;
  //using Microsoft.Xaml.Behaviors;

  /// <summary>
  /// Represents basic window for ribbon
  /// </summary>
  [TemplatePart(Name = PART_Icon, Type = typeof(UIElement))]
  [TemplatePart(Name = PART_ContentPresenter, Type = typeof(UIElement))]
  [TemplatePart(Name = PART_RibbonTitleBar, Type = typeof(RibbonTitleBar))]
  [TemplatePart(Name = PART_WindowCommands, Type = typeof(WindowCommands))]
  public class RibbonWindow : Window, IRibbonWindow {
    // ReSharper disable InconsistentNaming
#pragma warning disable SA1310 // Field names must not contain underscore
    private const string PART_Icon = "PART_Icon";
    private const string PART_ContentPresenter = "PART_ContentPresenter";
    private const string PART_RibbonTitleBar = "PART_RibbonTitleBar";
    private const string PART_WindowCommands = "PART_WindowCommands";
#pragma warning restore SA1310 // Field names must not contain underscore
    // ReSharper restore InconsistentNaming
    private FrameworkElement iconImage;

    #region Properties

    #region TitelBar

    /// <summary>
    /// Gets ribbon titlebar
    /// </summary>
    public RibbonTitleBar TitleBar {
      get => (RibbonTitleBar)this.GetValue(TitleBarProperty);
      private set => this.SetValue(titleBarPropertyKey, value);
    }

    private static readonly DependencyPropertyKey titleBarPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TitleBar), typeof(RibbonTitleBar), typeof(RibbonWindow), new PropertyMetadata());

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="TitleBar"/>.
    /// </summary>
    public static readonly DependencyProperty TitleBarProperty = titleBarPropertyKey.DependencyProperty;

    #endregion

    /// <summary>
    /// Gets or sets the height which is used to render the window title.
    /// </summary>
    public double TitleBarHeight {
      get => (double)this.GetValue(TitleBarHeightProperty);
      set => this.SetValue(TitleBarHeightProperty, value);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="TitleBarHeight"/>.
    /// </summary>
    public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(nameof(TitleBarHeight), typeof(double), typeof(RibbonWindow), new PropertyMetadata(DoubleBoxes.Zero));

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> which is used to render the window title.
    /// </summary>
    public Brush TitleForeground {
      get => (Brush)this.GetValue(TitleForegroundProperty);
      set => this.SetValue(TitleForegroundProperty, value);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="TitleForeground"/>.
    /// </summary>
    public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(nameof(TitleForeground), typeof(Brush), typeof(RibbonWindow), new PropertyMetadata());

    /// <summary>
    /// Gets or sets the <see cref="Brush"/> which is used to render the window title background.
    /// </summary>
    public Brush TitleBackground {
      get => (Brush)this.GetValue(TitleBackgroundProperty);
      set => this.SetValue(TitleBackgroundProperty, value);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="TitleBackground"/>.
    /// </summary>
    public static readonly DependencyProperty TitleBackgroundProperty = DependencyProperty.Register(nameof(TitleBackground), typeof(Brush), typeof(RibbonWindow), new PropertyMetadata());

    /// <summary>
    /// Using a DependencyProperty as the backing store for WindowCommands.  This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty WindowCommandsProperty = DependencyProperty.Register(nameof(WindowCommands), typeof(WindowCommands), typeof(RibbonWindow), new PropertyMetadata());

    /// <summary>
    /// Gets or sets the window commands
    /// </summary>
    public WindowCommands WindowCommands {
      get => (WindowCommands)this.GetValue(WindowCommandsProperty);
      set => this.SetValue(WindowCommandsProperty, value);
    }

    /// <summary>
    /// Gets or sets resize border thickness
    /// </summary>
    public Thickness ResizeBorderThickness {
      get => (Thickness)this.GetValue(ResizeBorderThicknessProperty);
      set => this.SetValue(ResizeBorderThicknessProperty, value);
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for ResizeBorderTickness.  This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(RibbonWindow), new PropertyMetadata(new Thickness(8D))); //WindowChromeBehavior.GetDefaultResizeBorderThickness()));

    /// <summary>
    /// Gets or sets glass border thickness
    /// </summary>
    public Thickness GlassFrameThickness {
      get => (Thickness)this.GetValue(GlassFrameThicknessProperty);
      set => this.SetValue(GlassFrameThicknessProperty, value);
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for GlassFrameThickness.
    /// GlassFrameThickness != 0 enables the default window drop shadow.
    /// </summary>
    public static readonly DependencyProperty GlassFrameThicknessProperty =
        DependencyProperty.Register(nameof(GlassFrameThickness), typeof(Thickness), typeof(RibbonWindow), new PropertyMetadata(new Thickness(1)));

    /// <summary>
    /// Gets or sets whether icon is visible
    /// </summary>
    public bool IsIconVisible {
      get => (bool)this.GetValue(IsIconVisibleProperty);
      set => this.SetValue(IsIconVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets whether icon is visible
    /// </summary>
    public static readonly DependencyProperty IsIconVisibleProperty = DependencyProperty.Register(nameof(IsIconVisible), typeof(bool), typeof(RibbonWindow), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));

    // todo check if IsCollapsed and IsAutomaticCollapseEnabled should be reduced to one shared property for RibbonWindow and Ribbon

    /// <summary>
    /// Gets whether window is collapsed
    /// </summary>
    public bool IsCollapsed {
      get => (bool)this.GetValue(IsCollapsedProperty);
      set => this.SetValue(IsCollapsedProperty, value);
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for IsCollapsed.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(RibbonWindow), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Defines if the Ribbon should automatically set <see cref="IsCollapsed"/> when the width or height of the owner window drop under <see cref="Ribbon.MinimalVisibleWidth"/> or <see cref="Ribbon.MinimalVisibleHeight"/>
    /// </summary>
    public bool IsAutomaticCollapseEnabled {
      get => (bool)this.GetValue(IsAutomaticCollapseEnabledProperty);
      set => this.SetValue(IsAutomaticCollapseEnabledProperty, value);
    }

    /// <summary>
    /// Using a DependencyProperty as the backing store for IsCollapsed.
    /// This enables animation, styling, binding, etc...
    /// </summary>
    public static readonly DependencyProperty IsAutomaticCollapseEnabledProperty = DependencyProperty.Register(nameof(IsAutomaticCollapseEnabled), typeof(bool), typeof(RibbonWindow), new PropertyMetadata(BooleanBoxes.TrueBox));

    /// <summary>
    /// Defines if the taskbar should be ignored and hidden while the window is maximized.
    /// </summary>
    public bool IgnoreTaskbarOnMaximize {
      get => (bool)this.GetValue(IgnoreTaskbarOnMaximizeProperty);
      set => this.SetValue(IgnoreTaskbarOnMaximizeProperty, value);
    }

    /// <summary>
    /// <see cref="DependencyProperty"/> for <see cref="IgnoreTaskbarOnMaximize"/>.
    /// </summary>
    public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty = DependencyProperty.Register(nameof(IgnoreTaskbarOnMaximize), typeof(bool), typeof(RibbonWindow), new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register(nameof(IsResizable), typeof(bool), typeof(Window), new PropertyMetadata(true));

    public Boolean IsResizable {
      get => (bool)this.GetValue(IsResizableProperty);
      set => this.SetValue(IsResizableProperty, value);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Static constructor
    /// </summary>
    static RibbonWindow() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonWindow), new FrameworkPropertyMetadata(typeof(RibbonWindow)));

      BorderThicknessProperty.OverrideMetadata(typeof(RibbonWindow), new FrameworkPropertyMetadata(new Thickness(1)));
      WindowStyleProperty.OverrideMetadata(typeof(RibbonWindow), new FrameworkPropertyMetadata(WindowStyle.None));
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS {
      public int leftWidth;
      public int rightWidth;
      public int topHeight;
      public int bottomHeight;
    }
    [DllImport("dwmapi.dll", PreserveSig = true)]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
    /// <summary>
    /// Default constructor
    /// </summary>
    public RibbonWindow() {
      this.SizeChanged += this.OnSizeChanged;
      this.Loaded += this.OnLoaded;
      this.InitializeWindowChromeBehavior();
      //this.GlassFrameThickness = new Thickness(0, 1, 0, 0);



    }

    private void InitializeWindowChromeBehavior() {
      //var behavior = new WindowChromeBehavior();
      //BindingOperations.SetBinding(behavior, WindowChromeBehavior.ResizeBorderThicknessProperty, new Binding { Path = new PropertyPath(ResizeBorderThicknessProperty), Source = this });
      //BindingOperations.SetBinding(behavior, WindowChromeBehavior.IgnoreTaskbarOnMaximizeProperty, new Binding { Path = new PropertyPath(IgnoreTaskbarOnMaximizeProperty), Source = this });

      //Interaction.GetBehaviors(this).Add(behavior);
    }
    #endregion

    #region Overrides



    #endregion

    // Size change to collapse ribbon
    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
      this.MaintainIsCollapsed();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
      if (this.SizeToContent == SizeToContent.Manual) {
        return;
      }

      this.RunInDispatcherAsync(() => {
        // Fix for #454 while also keeping #473
        var availableSize = new Size(this.TitleBar.ActualWidth, this.TitleBar.ActualHeight);
        this.TitleBar.Measure(availableSize);
        this.TitleBar.ForceMeasureAndArrange();
      }, DispatcherPriority.ApplicationIdle);
    }
    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

    /// <summary>
    /// used to overwrite WndProc
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSourceInitialized(EventArgs e) {
      base.OnSourceInitialized(e);
      var hWndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
      hWndSource.CompositionTarget.BackgroundColor = Colors.Black;

      var nonClientArea = new MARGINS();
      nonClientArea.topHeight = 1;
      DwmExtendFrameIntoClientArea(hWndSource.Handle, ref nonClientArea);

      hWndSource.AddHook(WndProcHooked);
      RECT rcClient;
      GetWindowRect(hWndSource.Handle, out rcClient);
      // FRAMECHANGED | NOMOVE | NOSIZE
      //if (this.WindowState != WindowState.Maximized) {
      SetWindowPos(hWndSource.Handle, IntPtr.Zero, rcClient.left, rcClient.top, rcClient.Width, rcClient.Height, 0x0020);
      //}
      //var source = PresentationSource.FromVisual(this) as HwndSource;
      ////_ModifyStyle(source.Handle, WS.VISIBLE, 0);
      //source.AddHook(WndProcHooked);
      //var margins = new MARGINS();
      //margins.topHeight = 1;
      //DwmExtendFrameIntoClientArea(source.Handle, ref margins);
      //var compositor = new WindowAccentCompositor(this);
      //compositor.Color = Color.FromArgb(96, 63, 62, 62);
      //compositor.IsEnabled = true;
    }
    public enum HT {
      ERROR = -2,
      TRANSPARENT = -1,
      NOWHERE = 0,
      CLIENT = 1,
      CAPTION = 2,
      SYSMENU = 3,
      GROWBOX = 4,
      MENU = 5,
      HSCROLL = 6,
      VSCROLL = 7,
      MINBUTTON = 8,
      MAXBUTTON = 9,
      LEFT = 10,
      RIGHT = 11,
      TOP = 12,
      TOPLEFT = 13,
      TOPRIGHT = 14,
      BOTTOM = 15,
      BOTTOMLEFT = 16,
      BOTTOMRIGHT = 17,
      BORDER = 18,
      OBJECT = 19,
      CLOSE = 20,
      HELP = 21
    }
    private static readonly HT[,] hitTestBorders =
                                                       {
                                                            { HT.TOPLEFT,    HT.TOP,     HT.TOPRIGHT },
                                                            { HT.LEFT,       HT.CLIENT,  HT.RIGHT },
                                                            { HT.BOTTOMLEFT, HT.BOTTOM,  HT.BOTTOMRIGHT },
                                                       };
    private HT _HitTestNca(Rect windowPosition, Point mousePosition) {
      // Determine if hit test is for resizing, default middle (1,1).
      var uRow = 1;
      var uCol = 1;
      var onResizeBorder = false;

      // Only get this once from the property to improve performance
      var resizeBorderThickness = this.ResizeBorderThickness;

      // Determine if the point is at the top or bottom of the window.
      if (mousePosition.Y >= windowPosition.Top
          && mousePosition.Y < windowPosition.Top + resizeBorderThickness.Top) {
        onResizeBorder = mousePosition.Y < (windowPosition.Top + resizeBorderThickness.Top);
        uRow = 0; // top (caption or resize border)
      } else if (mousePosition.Y < windowPosition.Bottom
                 && mousePosition.Y >= windowPosition.Bottom - (int)resizeBorderThickness.Bottom) {
        uRow = 2; // bottom
      }

      // Determine if the point is at the left or right of the window.
      if (mousePosition.X >= windowPosition.Left
          && mousePosition.X < windowPosition.Left + (int)resizeBorderThickness.Left) {
        uCol = 0; // left side
      } else if (mousePosition.X < windowPosition.Right
                 && mousePosition.X >= windowPosition.Right - resizeBorderThickness.Right) {
        uCol = 2; // right side
      }

      // If the cursor is in one of the top edges by the caption bar, but below the top resize border,
      // then resize left-right rather than diagonally.
      if (uRow == 0
          && uCol != 1
          && onResizeBorder == false) {
        uRow = 1;
      }

      var ht = hitTestBorders[uRow, uCol];

      if (ht == HT.TOP
          && onResizeBorder == false) {
        ht = HT.CAPTION;
      }

      return ht;
    }

    public T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject {
      // Confirm parent and childName are valid. 
      if (parent == null) return null;
      T foundChild = null;
      int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < childrenCount; i++) {
        var child = VisualTreeHelper.GetChild(parent, i);
        // If the child is not of the request child type child
        T childType = child as T;
        if (childType == null) {
          // recursively drill down the tree
          foundChild = FindChild<T>(child, childName);
          // If the child is found, break so we do not overwrite the found child. 
          if (foundChild != null) break;
        } else if (!string.IsNullOrEmpty(childName)) {
          var frameworkElement = child as FrameworkElement;
          // If the child's name is set for search
          if (frameworkElement != null && frameworkElement.Name == childName) {
            // if the child's name is of the request name
            foundChild = FindChild<T>(child, childName);
            break;
          }
        } else {
          // child element found.
          foundChild = (T)child;
          break;
        }
      }
      return foundChild;
    }
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

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
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [Flags]
    [CLSCompliant(false)]
    public enum WS : uint {
      OVERLAPPED = 0x00000000,
      POPUP = 0x80000000,
      CHILD = 0x40000000,
      MINIMIZE = 0x20000000,
      VISIBLE = 0x10000000,
      DISABLED = 0x08000000,
      CLIPSIBLINGS = 0x04000000,
      CLIPCHILDREN = 0x02000000,
      MAXIMIZE = 0x01000000,
      BORDER = 0x00800000,
      DLGFRAME = 0x00400000,
      VSCROLL = 0x00200000,
      HSCROLL = 0x00100000,
      SYSMENU = 0x00080000,
      THICKFRAME = 0x00040000,
      GROUP = 0x00020000,
      TABSTOP = 0x00010000,

      MINIMIZEBOX = 0x00020000,
      MAXIMIZEBOX = 0x00010000,

      CAPTION = BORDER | DLGFRAME,
      TILED = OVERLAPPED,
      ICONIC = MINIMIZE,
      SIZEBOX = THICKFRAME,
      TILEDWINDOW = OVERLAPPEDWINDOW,

      OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX,
      POPUPWINDOW = POPUP | BORDER | SYSMENU,
      CHILDWINDOW = CHILD,
    }
    public static IntPtr SetWindowLongPtr(IntPtr hwnd, int nIndex, IntPtr dwNewLong) {
      if (IntPtr.Size == 8) {
        return SetWindowLongPtr64(hwnd, nIndex, dwNewLong);
      }

      return new IntPtr(SetWindowLongPtr32(hwnd, nIndex, dwNewLong.ToInt32()));
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    private Rect _GetWindowRect(IntPtr handle) {
      // Get the window rectangle.
      var windowPosition = new RECT();
      GetWindowRect(handle, out windowPosition);
      return new Rect(windowPosition.left, windowPosition.top, windowPosition.Width, windowPosition.Height);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern int GetWindowLongPtr64(IntPtr hWnd, int nIndex);
    private bool _ModifyStyle(IntPtr handle, WS removeStyle, WS addStyle) {
      var dwStyle = GetWindowLongPtr64(handle, -16);
      var dwNewStyle = ((WS)dwStyle & ~removeStyle) | addStyle;
      if ((WS)dwStyle == dwNewStyle) {
        return false;
      }

      SetWindowLongPtr(handle, -16, (IntPtr)dwNewStyle);
      return true;
    }

    private Boolean _IsFirstTimeMaximize = true;
    [DllImport("dwmapi.dll")]
    static extern bool DwmDefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref IntPtr plResult);
    private IntPtr WndProcHooked(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      //if (msg == 0x0046) {
      //  handled = this.WindowState == WindowState.Maximized;
      //  return (IntPtr)1;
      //}
      var result = IntPtr.Zero;
      //var defDwm = DwmDefWindowProc(hwnd, msg, wParam, lParam, ref result);
      //if (msg == 0x0006) {
      //  MARGINS margins = new MARGINS();
      //  margins.topHeight = 1;

      //  var hr = DwmExtendFrameIntoClientArea(hwnd, ref margins);
      //  handled = true;
      //  return IntPtr.Zero;
      //}
      var cp = this.GetPart<UIElement>(PART_WindowCommands) as ContentPresenter;
      var ct = cp.Content as WindowCommands;
      var max = ct.Template.FindName("PART_Max", ct) as System.Windows.Controls.Button;
      var restore = ct.Template.FindName("PART_Restore", ct) as System.Windows.Controls.Button;
      var closeBtn = ct.Template.FindName("PART_Close", ct) as System.Windows.Controls.Button;
      var minBtn = ct.Template.FindName("PART_Min", ct) as System.Windows.Controls.Button;
      var mp = GetMousePosition();
      AutomationPeer theBtnRestorePeer = UIElementAutomationPeer.CreatePeerForElement(restore);
      Rect rectRestore = theBtnRestorePeer.GetBoundingRectangle();
      AutomationPeer theBtnMaxPeer = UIElementAutomationPeer.CreatePeerForElement(max);
      Rect rectMax = theBtnMaxPeer.GetBoundingRectangle();
      AutomationPeer theBtnClosePeer = UIElementAutomationPeer.CreatePeerForElement(closeBtn);
      Rect rectClose = theBtnClosePeer.GetBoundingRectangle();
      AutomationPeer theBtnMinPeer = UIElementAutomationPeer.CreatePeerForElement(minBtn);
      Rect rectMin = theBtnMinPeer.GetBoundingRectangle();

      if (msg == 0x0084 && result == IntPtr.Zero) {


        if (rectRestore.Contains(mp.X, mp.Y) || rectMax.Contains(mp.X, mp.Y)) {
          max.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.MouseOver.Background"];
          restore.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.MouseOver.Background"];
          closeBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          minBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          handled = true;
          return (IntPtr)9;
        } else if (rectClose.Contains(mp.X, mp.Y)) {
          max.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          restore.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          closeBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CloseButton.MouseOver.Background"];
          minBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          handled = true;
          return (IntPtr)20;
        } else if (rectMin.Contains(mp.X, mp.Y)) {
          max.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          restore.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          closeBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          minBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.MouseOver.Background"];
          handled = true;
          return (IntPtr)8;
        } else {
          max.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          restore.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          closeBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          minBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
        }
        var dpi = this.GetDpi();
        var mousePosScreen = Utility.GetPoint(lParam); //new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam));
        var windowPosition = this._GetWindowRect(hwnd);

        var ht = IntPtr.Zero;
        if (this.IsResizable) {
          var mousePosWindow = mousePosScreen;
          mousePosWindow.Offset(-windowPosition.X, -windowPosition.Y);
          mousePosWindow = DpiHelper.DevicePixelsToLogical(mousePosWindow, dpi.DpiScaleX, dpi.DpiScaleY);
          ht = (IntPtr) this._HitTestNca(DpiHelper.DeviceRectToLogical(windowPosition, dpi.DpiScaleX, dpi.DpiScaleY),
            DpiHelper.DevicePixelsToLogical(mousePosScreen, dpi.DpiScaleX, dpi.DpiScaleY));
          handled = true;
        }
        return ht;
      }

      if (msg == 0x0086) {
        var lRet = DefWindowProc(hwnd, 0x0086, wParam, new IntPtr(-1));
        // We don't have any non client area, so we can just discard this message by handling it 
        handled = true;
        return lRet;
      }

      if (msg == 0x0085) {
        // We don't have any non client area, so we can just discard this message by handling it 
        handled = true;
        return new IntPtr(1);
      }

      if (msg == 0x0083) {
        //if (wParam == new IntPtr(1)) {
        //  //this._ModifyStyle(hwnd, 0, WS.VISIBLE);
        //  handled = true;
        //  return IntPtr.Zero;
        //}
        handled = true;

        if (wParam != IntPtr.Zero && this.WindowState != WindowState.Maximized) {
          if (!this._IsFirstTimeMaximize) {
            var rc = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

            // We have to add or remove one pixel on any side of the window to force a flicker free resize.
            // Removing pixels would result in a smaller client area.
            // Adding pixels does not seem to really increase the client area.
            rc.bottom += 1;

            Marshal.StructureToPtr(rc, lParam, true);
            //  handled = true;
          } else {
            //  handled = true;
            this._IsFirstTimeMaximize = false;
          }
        }
      }

      if (msg == 0x00A1 && (wParam == (IntPtr)9 || wParam == new IntPtr(20) || wParam == new IntPtr(8))) {
        handled = true;
        if (wParam == new IntPtr(20)) {
          closeBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CloseButton.Pressed.Background"];
        } else if (wParam == new IntPtr(9)) {
          max.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Pressed.Background"];
          restore.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Pressed.Background"];
        } else if (wParam == new IntPtr(8)) {
          minBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Pressed.Background"];
        }
      }


      if (msg == 0x0024) {
        WmGetMinMaxInfo(hwnd, lParam, (int)MinWidth, (int)MinHeight);
        //handled = true;
      }

      if (msg == 0x00A2) {
        if (wParam == (IntPtr)9) {
          max.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          restore.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          if (this.WindowState == WindowState.Maximized) {
            SystemCommands.RestoreWindow(this);
          } else {
            SystemCommands.MaximizeWindow(this);
          }
        } else if (wParam == (IntPtr)20) {
          closeBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          SystemCommands.CloseWindow(this);
        } else if (wParam == (IntPtr)8) {
          minBtn.Background = (Brush)Application.Current.Resources["Fluent.Ribbon.Brushes.WindowCommands.CaptionButton.Background"];
          SystemCommands.MinimizeWindow(this);

        }
      }
      return IntPtr.Zero;
    }

    private void MaintainIsCollapsed() {
      if (this.IsAutomaticCollapseEnabled == false) {
        return;
      }

      if (this.ActualWidth < Ribbon.MinimalVisibleWidth
          || this.ActualHeight < Ribbon.MinimalVisibleHeight) {
        this.IsCollapsed = true;
      } else {
        this.IsCollapsed = false;
      }
    }

    /// <inheritdoc />
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      this.TitleBar = this.GetTemplateChild(PART_RibbonTitleBar) as RibbonTitleBar;

      if (this.iconImage != null) {
        this.iconImage.MouseDown -= this.HandleIconMouseDown;
      }

      if (this.WindowCommands == null) {
        this.WindowCommands = new WindowCommands();
      }

      this.iconImage = this.GetPart<FrameworkElement>(PART_Icon);

      if (this.iconImage != null) {
        this.iconImage.MouseDown += this.HandleIconMouseDown;
      }

      this.GetPart<UIElement>(PART_Icon)?.SetValue(WindowChrome.IsHitTestVisibleInChromeProperty, true);
      this.GetPart<UIElement>(PART_WindowCommands)?.SetValue(WindowChrome.IsHitTestVisibleInChromeProperty, true);
    }

    /// <inheritdoc />
    protected override void OnStateChanged(EventArgs e) {
      base.OnStateChanged(e);

      // todo: remove fix if we update to ControlzEx 4.0
      if (this.WindowState == WindowState.Maximized
          && this.SizeToContent != SizeToContent.Manual) {
        this.SizeToContent = SizeToContent.Manual;
      }

      this.RunInDispatcherAsync(() => this.TitleBar?.ForceMeasureAndArrange(), DispatcherPriority.Background);
    }

    private void HandleIconMouseDown(object sender, MouseButtonEventArgs e) {
      switch (e.ChangedButton) {
        case MouseButton.Left:
          if (e.ClickCount == 1) {
            e.Handled = true;

            WindowSteeringHelper.ShowSystemMenu(this, this.PointToScreen(new Point(0, this.TitleBarHeight)));
          } else if (e.ClickCount == 2) {
            e.Handled = true;

#pragma warning disable 618
            SystemCommands.CloseWindow(this);
#pragma warning restore 618
          }

          break;

        case MouseButton.Right:
          e.Handled = true;

          WindowSteeringHelper.ShowSystemMenu(this, e);
          break;
      }
    }

    /// <summary>
    /// Gets the template child with the given name.
    /// </summary>
    /// <typeparam name="T">The interface type inheirted from DependencyObject.</typeparam>
    /// <param name="name">The name of the template child.</param>
    internal T GetPart<T>(string name)
        where T : DependencyObject {
      return this.GetTemplateChild(name) as T;
    }

    public const int MONITOR_DEFAULTTONULL = 0x00000000;
    public const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
    public const int MONITOR_DEFAULTTONEAREST = 0x00000002;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hmonitor, [In, Out] MONITORINFOEX info);

    [DllImport("user32")]
    internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam, int minWidth, int minHeight) {
      IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
      if (monitor != IntPtr.Zero) {
        MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
        MONITORINFOEX monitorInfo = new MONITORINFOEX();
        monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
        GetMonitorInfo(monitor, monitorInfo);
        RECT rcWorkArea = monitorInfo.rcWork;
        RECT rcMonitorArea = monitorInfo.rcMonitor;
        mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
        mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
        //if (mmi.ptMaxPosition.y == 0)
        //  mmi.ptMaxPosition.y = 1;
        mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
        mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
        mmi.ptMaxTrackSize.x = mmi.ptMaxSize.x;
        mmi.ptMaxTrackSize.y = mmi.ptMaxSize.y;
        Marshal.StructureToPtr(mmi, lParam, true);
      }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public class MONITORINFOEX {
      internal int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
      internal RECT rcMonitor = new RECT();
      internal RECT rcWork = new RECT();
      internal int dwFlags = 0;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      internal char[] szDevice = new char[32];
    }
    /// <summary>
    /// POINT aka POINTAPI
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
      /// <summary>
      /// x coordinate of point.
      /// </summary>
      public int x;
      /// <summary>
      /// y coordinate of point.
      /// </summary>
      public int y;

      /// <summary>
      /// Construct a point of coordinates (x,y).
      /// </summary>
      public POINT(int x, int y) {
        this.x = x;
        this.y = y;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO {
      public POINT ptReserved;
      public POINT ptMaxSize;
      public POINT ptMaxPosition;
      public POINT ptMinTrackSize;
      public POINT ptMaxTrackSize;
    };

    /// <summary>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO {
      /// <summary>
      /// </summary>            
      public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

      /// <summary>
      /// </summary>            
      public RECT rcMonitor = new RECT();

      /// <summary>
      /// </summary>            
      public RECT rcWork = new RECT();

      /// <summary>
      /// </summary>            
      public int dwFlags = 0;
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