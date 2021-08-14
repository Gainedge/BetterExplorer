using System;
using System.Runtime.InteropServices;

namespace BExplorer.Shell.Interop {
  public static class UxTheme {
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool AllowDarkModeForAppDelegate(Boolean isAllowed);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate bool AllowDarkModeForWindowDelegate(IntPtr hWnd, Boolean isAllow);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void FlushMenuThemesDelegate();

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    public static extern int SetWindowTheme(IntPtr hWnd, String pszSubAppName, int pszSubIdList);

    public static Boolean AllowDarkModeForApp(Boolean isAllowed) {
      IntPtr pDll = Kernel32.LoadLibrary(@"UxTheme.dll");
      IntPtr pFunction = Kernel32.GetProcAddress(pDll, 135);
      if (pFunction == IntPtr.Zero) {
        return false;
      }
      var allowDarkModeForApp = (AllowDarkModeForAppDelegate)Marshal.GetDelegateForFunctionPointer(
        pFunction,
        typeof(AllowDarkModeForAppDelegate));
      var result = allowDarkModeForApp(isAllowed);
      Kernel32.FreeLibrary(pDll);
      return result;
    }
    public static Boolean AllowDarkModeForWindow(IntPtr hWnd, Boolean isAllowed) {
      IntPtr pDll = Kernel32.LoadLibrary(@"UxTheme.dll");
      IntPtr pFunction = Kernel32.GetProcAddress(pDll, 133);
      if (pFunction == IntPtr.Zero) {
        return false;
      }
      var allowDarkModeForWindow = (AllowDarkModeForWindowDelegate)Marshal.GetDelegateForFunctionPointer(
        pFunction,
        typeof(AllowDarkModeForWindowDelegate));
      var result = allowDarkModeForWindow(hWnd, isAllowed);
      Kernel32.FreeLibrary(pDll);
      return result;
    }

    public static Boolean FlushMenuThemes() {
      IntPtr pDll = Kernel32.LoadLibrary(@"UxTheme.dll");
      IntPtr pFunction = Kernel32.GetProcAddress(pDll, 136);
      if (pFunction == IntPtr.Zero) {
        return false;
      }
      var flushMenuThemes = (FlushMenuThemesDelegate)Marshal.GetDelegateForFunctionPointer(
        pFunction,
        typeof(FlushMenuThemesDelegate));
      flushMenuThemes();
      Kernel32.FreeLibrary(pDll);
      return true;
    }
  }
}
