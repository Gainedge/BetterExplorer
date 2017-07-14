using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell {
  public static class Utilities {
    public static WindowsVersions WindowsVersion {
      get {
        var environmentInfo = Environment.OSVersion.Version;
        if (environmentInfo.Major == 6 && environmentInfo.Minor == 1) {
          return WindowsVersions.Windows7;
        } else if (environmentInfo.Major == 6 && environmentInfo.Minor == 2) {
          return WindowsVersions.Windows8;
        } else if (environmentInfo.Major == 6 && environmentInfo.Minor == 3) {
          return WindowsVersions.Windows81;
        } else  {
          return WindowsVersions.Windows10;
        }
      }
    }
  }
}
