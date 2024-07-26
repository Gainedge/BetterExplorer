using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using WPFUI.Background;
using WPFUI.Common;

// ReSharper disable once IdentifierTypo
namespace WPFUI.Controls {
  /// <summary>
  /// 
  /// </summary>
  [TemplatePart(Name = "ContentPresenter", Type = typeof(ContentPresenter))]
  public class UIWindow : Window {
    /// <summary>
    /// 
    /// </summary>
    public IntPtr Handle { get; set; }
    private WindowChrome _chrome { get; set; }
    public static readonly DependencyProperty ThemeBackgroundTypeProperty =
      DependencyProperty.Register(
        name: nameof(ThemeBackgroundType),
        propertyType: typeof(BackgroundType),
        ownerType: typeof(UIWindow),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: BackgroundType.Default)
      );
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty GlassMarginProperty =
      DependencyProperty.Register(
        name: nameof(GlassMargin),
        propertyType: typeof(Thickness),
        ownerType: typeof(UIWindow),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: new Thickness(-1))
      );
    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty CaptionHeightProperty =
      DependencyProperty.Register(
        name: nameof(CaptionHeight),
        propertyType: typeof(Double),
        ownerType: typeof(UIWindow),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: SystemParameters.CaptionHeight)
      );
    /// <summary>
    /// 
    /// </summary>
    public BackgroundType ThemeBackgroundType {
      get => (BackgroundType)GetValue(ThemeBackgroundTypeProperty);
      set => SetValue(ThemeBackgroundTypeProperty, value);
    }
    /// <summary>
    /// 
    /// </summary>
    public Thickness GlassMargin {
      get => (Thickness)GetValue(GlassMarginProperty);
      set => SetValue(GlassMarginProperty, value);
    }
    /// <summary>
    /// 
    /// </summary>
    public Double CaptionHeight {
      get => (Double)GetValue(CaptionHeightProperty);
      set => SetValue(CaptionHeightProperty, value);
    }
    private ContentPresenter _ContentPresenter { get; set; }
    static UIWindow() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(UIWindow), new FrameworkPropertyMetadata(typeof(UIWindow)));
    }

    /// <inheritdoc />
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      this._ContentPresenter = this.GetTemplateChild("ContentPresenter") as ContentPresenter;

    }
    /// <summary>
    /// 
    /// </summary>
    public UIWindow() {
      this.StateChanged += this.OnStateChanged;
    }

    private void OnStateChanged(Object sender, EventArgs e) {
      if (this.WindowState == WindowState.Maximized) {
        this._chrome.ResizeBorderThickness = new Thickness(0);
        this._ContentPresenter.Margin = new Thickness(8);
        this._chrome.CaptionHeight = this.CaptionHeight;
      } else if (this.WindowState == WindowState.Normal) {
        this._chrome.ResizeBorderThickness = new Thickness(4);
        this._ContentPresenter.Margin = new Thickness(0);
        if (this.CaptionHeight - 16 >= SystemParameters.CaptionHeight) {
          this._chrome.CaptionHeight = this.CaptionHeight - 16;
        }
      }
    }

    /// <inheritdoc />
    protected override void OnSourceInitialized(EventArgs e) {
      base.OnSourceInitialized(e);
      // ReSharper disable once IdentifierTypo
      var hwnd = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
      if (hwnd != null) {
        this.Handle = hwnd.Handle;
        this._chrome = new WindowChrome {
          GlassFrameThickness = this.GlassMargin,
          ResizeBorderThickness = new Thickness(this.ResizeMode == ResizeMode.NoResize ? 0 : 4),
          CornerRadius = new CornerRadius(0),
          UseAeroCaptionButtons = false,
          CaptionHeight = this.CaptionHeight
        };
        //this._chrome.NonClientFrameEdges = NonClientFrameEdges.Bottom | NonClientFrameEdges.Left | NonClientFrameEdges.Right;
        WindowChrome.SetWindowChrome(this, this._chrome);
        if (hwnd.CompositionTarget != null) {
          hwnd.CompositionTarget.BackgroundColor = Colors.Transparent;
        }

        this.Background = Brushes.Transparent;
        Theme.Manager.Switch(this, Settings.BESettings.CurrentTheme == "Dark" ? Theme.Style.Dark : Theme.Style.Light, this.ThemeBackgroundType);
      }
      hwnd?.AddHook(WndProcHooked);
    }

    // ReSharper disable once IdentifierTypo
    private IntPtr WndProcHooked(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, ref Boolean handled) {
      switch (msg) {
        case 0x0083: {
          handled = true;
          if (wParam != IntPtr.Zero && this.WindowState != WindowState.Maximized) {
            var rc = lParam.GetLParam<SnapLayout.RECT>();
            // We have to add or remove one pixel on any side of the window to force a flicker free resize.
            // Removing pixels would result in a smaller client area.
            // Adding pixels does not seem to really increase the client area.
            rc.bottom += 1;
            //rc.right += 1;

            Marshal.StructureToPtr(rc, lParam, true);
            //  handled = true;

          }

          break;
        }
      }

      return IntPtr.Zero;
    }

  }
}
