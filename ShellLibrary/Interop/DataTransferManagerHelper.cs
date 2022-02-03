using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using BExplorer.Shell.Interop;
using WinRT;

namespace ShellLibrary.Interop {
  public static class DataTransferManagerHelper {
    static readonly Guid _dtm_iid = new Guid(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

    static IDataTransferManagerInterop DataTransferManagerInterop {
      get {
        return DataTransferManager.As<IDataTransferManagerInterop>();
      }
    }

    public static DataTransferManager GetForWindow(IntPtr hwnd) {
      DataTransferManagerInterop.GetForWindow(hwnd, _dtm_iid, out var result);
      return MarshalInterface<DataTransferManager>.FromAbi(result);
    }

    public static void ShowShareUIForWindow(IntPtr hwnd) {
      DataTransferManagerInterop.ShowShareUIForWindow(hwnd);
    }

    [ComImport, Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDataTransferManagerInterop {
      HResult GetForWindow([In] IntPtr appWindow, [In] ref Guid riid, [Out] out IntPtr pDataTransfermanager );
      void ShowShareUIForWindow(IntPtr appWindow);
    }
  }
}
