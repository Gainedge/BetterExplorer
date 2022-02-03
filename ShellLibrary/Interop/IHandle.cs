using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellLibrary.Interop {
  public interface IHandle {
    /// <summary>Returns the value of the handle field.</summary>
    /// <returns>An IntPtr representing the value of the handle field.</returns>
    IntPtr DangerousGetHandle();
  }
}
