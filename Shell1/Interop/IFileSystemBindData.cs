using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {
  [ComImport, Guid("01E18D10-4D8B-11d2-855D-006008059367"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IFileSystemBindData {
    void SetFileData([In, MarshalAs(UnmanagedType.Struct)] ref WIN32_FIND_DATA pfd);
    void GetFileData([Out, MarshalAs(UnmanagedType.Struct)] out WIN32_FIND_DATA pdf);
  }
}
