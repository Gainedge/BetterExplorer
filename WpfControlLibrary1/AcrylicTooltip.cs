using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using BetterExplorerControls.Helpers;
using WPFUI.Win32;

namespace BetterExplorerControls {
  public class AcrylicTooltip : ToolTip {
    public static readonly DependencyProperty ToolTipTextProperty =
      DependencyProperty.Register(
        name: "ToolTipText",
        propertyType: typeof(String),
        ownerType: typeof(AcrylicTooltip),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: String.Empty)
      );
    public String ToolTipText {
      get => (String)GetValue(ToolTipTextProperty);
      set => SetValue(ToolTipTextProperty, value);
    }
    static AcrylicTooltip() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicTooltip), new FrameworkPropertyMetadata(typeof(AcrylicTooltip)));
    }

    public AcrylicTooltip() {
      base.CustomPopupPlacementCallback = CustomPopupPlacementCallback;
    }

     private new CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset) {
       var placement1 =
         new CustomPopupPlacement(new Point(-((popupSize.Width - targetSize.Width) / 2), -popupSize.Height - 5), PopupPrimaryAxis.Horizontal);
       return new[] { placement1 };
    }

    protected override void OnOpened(RoutedEventArgs e) {
      base.OnOpened(e);
      var hWnd = (HwndSource)PresentationSource.FromVisual(this);
      if (hWnd != null) {
        if (hWnd.CompositionTarget != null) {
          hWnd.CompositionTarget.BackgroundColor = Colors.Transparent;
        }

        var nonClientArea = new Dwmapi.MARGINS(new Thickness(0, 1, 0, 0));
        Dwmapi.DwmExtendFrameIntoClientArea(hWnd.Handle, ref nonClientArea);
        AcrylicHelper.SetBlur(hWnd.Handle, AcrylicHelper.AccentFlagsType.Window,
          AcrylicHelper.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
          (uint)Application.Current.Resources["SystemAcrylicTint"]);
        var preference = (Int32)Dwmapi.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL;
        Dwmapi.DwmSetWindowAttribute(hWnd.Handle, Dwmapi.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
          ref preference, sizeof(uint));
      }
    }
  }
}
