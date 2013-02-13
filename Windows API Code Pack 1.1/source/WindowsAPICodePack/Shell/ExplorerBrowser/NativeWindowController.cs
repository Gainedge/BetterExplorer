using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace Microsoft.WindowsAPICodePack.Shell.ExplorerBrowser {
  public sealed class NativeWindowController : NativeWindow {
    internal event MessageEventHandler MessageCaptured;

    public NativeWindowController(IntPtr hwnd) {
      AssignHandle(hwnd);
    }

    protected override void WndProc(ref Message m) {
      bool consumed = false;
      if (MessageCaptured != null) {
        try {
          consumed = MessageCaptured(ref m);
        } catch (Exception ex) {
          //TODO: error log here
        }
      }
      if (!consumed) {
        base.WndProc(ref m);
      }
    }

    // The real ReleaseHandle is unsafe.  NEVER EVER EVER call it!
    // Just clear the event subscription list instead.
    public override void ReleaseHandle() {
      MessageCaptured = null;
    }

    public IntPtr OptionalHandle { get; set; }

    internal delegate bool MessageEventHandler(ref Message msg);
  }
}
