using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using BetterExplorerControls.Helpers;
using Fluent;

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
      //this.Height = 30;
      //this.Width = 50;
      //this.Placement = PlacementMode.Custom;
      base.CustomPopupPlacementCallback = CustomPopupPlacementCallback;
    }

     private CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupsize, Size targetsize, Point offset) {
       var placement1 =
         new CustomPopupPlacement(new Point(-((popupsize.Width - targetsize.Width) / 2), -popupsize.Height - 5), PopupPrimaryAxis.Horizontal);
       return new[] { placement1 };
    }

    protected override void OnOpened(RoutedEventArgs e) {
      base.OnOpened(e);
      var hwnd = (HwndSource)HwndSource.FromVisual(this);
      hwnd.CompositionTarget.BackgroundColor = Colors.Transparent;
      var nonClientArea = new RibbonWindow.MARGINS();
      nonClientArea.topHeight = 1;
      //nonClientArea.bottomHeight = -1;
      //nonClientArea.leftWidth = -1;
      //nonClientArea.rightWidth = -1;
      RibbonWindow.DwmExtendFrameIntoClientArea(hwnd.Handle, ref nonClientArea);
      //var exStyle = (long)GetWindowLong(hwnd.Handle, (int)GetWindowLongFields.GWL_STYLE);

      //exStyle |= 0x40000;
      //SetWindowLong(hwnd.Handle, (int)AcrylicPopup.GetWindowLongFields.GWL_STYLE, (IntPtr)(0x04000000 | 0x02000000 | 0x00800000 | 0x00C00000 | 0x10000000));
      var preference = RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL;
      DwmSetWindowAttribute(hwnd.Handle, RibbonWindow.DwmWindowAttribute.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));
      AcrylicHelper.SetBlur(hwnd.Handle, AcrylicHelper.AccentFlagsType.Window, AcrylicHelper.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, (uint)Application.Current.Resources["SystemAcrylicTint"]);
    }

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern long DwmSetWindowAttribute(IntPtr hwnd,
      RibbonWindow.DwmWindowAttribute attribute,
      ref RibbonWindow.DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
      uint cbAttribute);

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
  }
}
