using System;

namespace BExplorer.Shell.Interop {
  using System.Drawing;
  using System.Windows.Forms;

  [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
  internal class NativeWindowListener : NativeWindow {
    public ShellView AttachedListView { get; set; }
    // Constant value was found in the "windows.h" header file.
    private const int WM_SIZE = 0x0005;

    public NativeWindowListener(ShellView attachedListView) {
      this.AttachedListView = attachedListView;
      var handle = (IntPtr)User32.SendMessage(this.AttachedListView.LVHandle, 
        (0x1000+31), IntPtr.Zero, IntPtr.Zero);
      this.AssignHandle(this.AttachedListView.LVHeaderHandle);
    }

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    protected override void WndProc(ref Message msg) {
      
      switch (msg.Msg) {
        case (int)WM.WM_ERASEBKGND:
          var gr = Graphics.FromHdc(msg.WParam);
          gr.FillRectangle(Brushes.Black, 0, 0, this.AttachedListView.ClientRectangle.Width, 22);
          gr.Dispose();
          break;

        default:
          break;
      }
      
      base.WndProc(ref msg);

    }
  }
}
