using System;
using System.Runtime.InteropServices;

namespace BetterExplorerControls.Helpers {
  public static class AcrylicHelper {
    public enum AccentFlagsType {
      Window = 0,
      Popup,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttributeData {
      public WindowCompositionAttribute Attribute;
      public IntPtr Data;
      public int SizeOfData;
    }

    public enum WindowCompositionAttribute {
      // ...
      WCA_ACCENT_POLICY = 19
      // ...
    }

    public enum AccentState {
      ACCENT_DISABLED = 0,
      ACCENT_ENABLE_GRADIENT = 1,
      ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
      ACCENT_ENABLE_BLURBEHIND = 3,
      ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
      ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy {
      public AccentState AccentState;
      public int AccentFlags;
      public uint GradientColor;
      public int AnimationId;
    }

    public enum AcrylicAccentState
    {
      Default             = -1,
      Disabled            = 0,
      Gradient            = 1,
      TransparentGradient = 2,
      BlurBehind          = 3,
      AcrylicBlurBehind   = 4,
    }
    public static void SetBlur(IntPtr hwnd, AccentFlagsType style = AccentFlagsType.Window, AccentState? state = null, uint gradientColor = 0x00FFFFFF) {
      var accent = new AccentPolicy();
      var accentStructSize = Marshal.SizeOf(accent);
      accent.AccentState = state ?? SelectAccentState();

      if (style == AccentFlagsType.Window) {
        accent.AccentFlags = 2;
      } else {
        accent.AccentFlags = 0x20 | 0x40 | 0x80 | 0x100;
      }

      //accent.GradientColor = 0x99FFFFFF;  // 60%の透明度が基本
      accent.GradientColor = gradientColor;  // Tint Colorはここでは設定せず、Bindingで外部から変えられるようにXAML側のレイヤーとして定義

      var accentPtr = Marshal.AllocHGlobal(accentStructSize);
      Marshal.StructureToPtr(accent, accentPtr, false);

      var data = new WindowCompositionAttributeData();
      data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
      data.SizeOfData = accentStructSize;
      data.Data = accentPtr;

      SetWindowCompositionAttribute(hwnd, ref data);

      Marshal.FreeHGlobal(accentPtr);
    }

    [DllImport("user32.dll")]
    public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    public static AccentState SelectAccentState(AcrylicAccentState state = AcrylicAccentState.Default)
    {
      // ウィンドウのアクリル効果を設定する
      AccentState value = state switch
      {
        // ウィンドウ背景のぼかしを行うのはWindows10の場合のみ
        // OSのバージョンに従い、AccentStateを切り替える
        //AcrylicAccentState.Default => SystemInfo.Version.Value switch
        //{
        //  // Windows11環境ではアクリル効果を無効にする
        //  var version when version >= VersionInfos.Windows11_Preview => AccentState.ACCENT_ENABLE_GRADIENT,
        //  // Windows10 1903以降では、ACCENT_ENABLE_ACRYLICBLURBEHINDを用いると、ウィンドウのドラッグ移動などでマウス操作に追従しなくなる。
        //  // ウィンドウの移動/リサイズ中だけ、ACCENT_ENABLE_ACRYLICBLURBEHINDを無効にして、この問題を回避する
        //  //var version when version >= VersionInfos.Windows10_1903 => AccentState.ACCENT_ENABLE_BLURBEHIND,
        //  var version when version >= VersionInfos.Windows10_1809 => AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
        //  var version when version >= VersionInfos.Windows10 => AccentState.ACCENT_ENABLE_BLURBEHIND,
        //  _ => AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT,
        //},
        AcrylicAccentState.Default => AccentState.ACCENT_ENABLE_GRADIENT,
        AcrylicAccentState.Disabled => AccentState.ACCENT_DISABLED,
        AcrylicAccentState.Gradient => AccentState.ACCENT_ENABLE_GRADIENT,
        AcrylicAccentState.TransparentGradient => AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT,
        AcrylicAccentState.BlurBehind => AccentState.ACCENT_ENABLE_BLURBEHIND,
        AcrylicAccentState.AcrylicBlurBehind => AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
        _ => throw new InvalidOperationException(),
      };

      return value;
    }
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct RECT {
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
  }
}
