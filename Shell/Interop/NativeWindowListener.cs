using System;
using System.Security.Permissions;

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
      this.AssignHandle(this.AttachedListView.LVHandle);
    }

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    protected override void WndProc(ref Message msg) {
      
      switch (msg.Msg) {
        case (int)WM.WM_VSCROLL:
            int style = (int)User32.GetWindowLong(this.AttachedListView.LVHandle, -16);
            if ((style & 0x00200000L) == 0x00200000L)
              User32.SetWindowLong(this.AttachedListView.LVHandle, -16, (long)style & ~0x00200000L);
          break;
        case 0x202:
          var h = 1;
          break;
        case -12:
          var nmhdr = (NMHDR)msg.GetLParam(typeof(NMHDR));

          break;

        default:
          break;
      }
      
      base.WndProc(ref msg);

    }
  }
}
