using System;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;

namespace ShellControls {
  public class ListViewEditor : NativeWindow {

    private ShellView _ShellViewEx { get; set; }

    public ListViewEditor(IntPtr hwnd, ShellView shellViewEx) {
      this._ShellViewEx = shellViewEx;
      this.AssignHandle(hwnd);
    }

    internal void OnHandleDestroyed(object sender, EventArgs e) {
      // Window was destroyed, release hook.
      this.ReleaseHandle();
    }

    protected override void WndProc(ref Message m) {
      switch (m.Msg) {
        case (int)WM.WM_WINDOWPOSCHANGING:
          User32.WINDOWPOS wp = (User32.WINDOWPOS)System.Runtime.InteropServices.Marshal.PtrToStructure(m.LParam, typeof(User32.WINDOWPOS));
          if (this._ShellViewEx.IconSize != 16 && this._ShellViewEx._ItemForRename > -1) {
            //      var lvi = default(LVITEMINDEX);
            //      lvi.iItem = this._ShellViewEx._ItemForRename;
            //      lvi.iGroup = this._ShellViewEx.GetGroupIndex(this._ShellViewEx._ItemForRename);
            //      var labelBounds = new User32.RECT() { Left = 2 };
            //      User32.SendMessage(this._ShellViewEx.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref labelBounds);
            //      var itemBounds = new User32.RECT();
            //      User32.SendMessage(this._ShellViewEx.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref itemBounds);
            //      itemBounds.Bottom = itemBounds.Bottom + 18;
            //      //labelBounds.Left = labelBounds.Left + 8;
            //      //labelBounds.Right = labelBounds.Right - 8;
            //      //labelBounds.Bottom = labelBounds.Bottom + 18;
            //      if (this._ShellViewEx.View != ShellViewStyle.Details) {
            //        labelBounds.Left = labelBounds.Left + 6;
            //        labelBounds.Right = labelBounds.Right - 12;
            //        labelBounds.Top = labelBounds.Top - 8;
            //        labelBounds.Bottom = labelBounds.Bottom - 8;
            //      }
            //      var labelBoundsReal = new User32.RECT() { Left = 2 };
            //      User32.SendMessage(this._ShellViewEx.LVHandle, MSG.LVM_GETITEMINDEXRECT, ref lvi, ref labelBoundsReal);

            var align = User32.TextFormatFlags.Center;
            //      if (this._ShellViewEx.View == ShellViewStyle.Tile) {
            //        align = User32.TextFormatFlags.Default | User32.TextFormatFlags.SingleLine;
            //        labelBoundsReal.Top = labelBoundsReal.Top + 16;
            //        labelBoundsReal.Left = labelBoundsReal.Left + 2;
            //        labelBoundsReal.Bottom = labelBoundsReal.Bottom + 16;
            //        labelBoundsReal.Right = labelBoundsReal.Right - 16;
            //      } else {
            //        if (labelBounds.Left <= itemBounds.Left + 16) {
            //          labelBoundsReal.Left = labelBoundsReal.Left + 8;
            //          labelBoundsReal.Right = labelBoundsReal.Right - 10;
            //        }
            //        labelBoundsReal.Top = labelBoundsReal.Top - 24;
            //        labelBoundsReal.Bottom = labelBoundsReal.Bottom - 8;
            //        labelBoundsReal.Left = labelBoundsReal.Left + 3;
            //        labelBoundsReal.Right = labelBoundsReal.Right + 3;
            //      }
            //      var rc = new User32.RECT(wp.x, wp.y, wp.x + wp.cx, wp.y + wp.cy);
            //      if (rc.Width > 0 && rc.Height > 0) {
            //        var g = User32.GetDC(this._ShellViewEx.LVHandle);
            //        var text = User32.GetText(m.HWnd);
            //        var height = User32.DrawText(g, text, -1, ref labelBoundsReal, align | User32.TextFormatFlags.EditControl | User32.TextFormatFlags.CalcRect | User32.TextFormatFlags.WordBreak | User32.TextFormatFlags.NoPrefix);
            //        labelBoundsReal.Bottom = labelBoundsReal.Top + height + 2;


            //        User32.ReleaseDC(this._ShellViewEx.LVHandle, g);

            var itemForRename = this._ShellViewEx.Items[this._ShellViewEx._ItemForRename];
            var lb = itemForRename.LabelBounds;
            var g = User32.GetDC(this._ShellViewEx.LVHandle);
            var text = User32.GetText(m.HWnd);
            var height = User32.DrawText(g, text, -1, ref lb, align | User32.TextFormatFlags.EditControl | User32.TextFormatFlags.CalcRect | User32.TextFormatFlags.WordBreak | User32.TextFormatFlags.NoPrefix);
             User32.ReleaseDC(this._ShellViewEx.LVHandle, g);
            wp.x = itemForRename.LabelBounds.X - 6;
            wp.y = itemForRename.LabelBounds.Y - 2;
            wp.cx = itemForRename.LabelBounds.Width + 12;
            wp.cy = height + 2;

            System.Runtime.InteropServices.Marshal.StructureToPtr(wp, m.LParam, true);
            //      }
          }
          break;
      }
      base.WndProc(ref m);
    }
  }
}
