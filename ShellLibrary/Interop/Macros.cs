using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell.Interop;

namespace ShellLibrary.Interop {
  public static class Macros {
    public static User32.ResourceId MAKEINTRESOURCE(int id) => id;
  }
}
