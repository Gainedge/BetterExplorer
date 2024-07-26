namespace BExplorer.Shell {
  using System;
  using Interop;
  using Microsoft.Win32;

  /// <summary>
  /// Utilities class that holds helper functions
  /// </summary>
  public static class Utilities {
    /// <summary>
    /// Gets current Windows version
    /// </summary>
    public static WindowsVersions WindowsVersion {
      get {
        var environmentInfo = Environment.OSVersion.Version;
        if (environmentInfo.Major == 6 && environmentInfo.Minor == 1) {
          return WindowsVersions.Windows7;
        } else if (environmentInfo.Major == 6 && environmentInfo.Minor == 2) {
          return WindowsVersions.Windows8;
        } else if (environmentInfo.Major == 6 && environmentInfo.Minor == 3) {
          return WindowsVersions.Windows81;
        } else {
          if (environmentInfo.Build >= 22000) {
            return WindowsVersions.Windows11;
          }

          return WindowsVersions.Windows10;
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether Is current maschine running Windows 10
    /// </summary>
    public static Boolean IsWindows10 {
      get {
        var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

        var productName = (String)reg?.GetValue("ProductName");

        return  productName?.StartsWith("Windows 10") ?? false;
      }
    }
  }
}
