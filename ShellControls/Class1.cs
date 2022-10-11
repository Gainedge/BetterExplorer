using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using ScrollEventArgs = System.Windows.Forms.ScrollEventArgs;

namespace ShellControls;
public class VscrollBarEx : UserControl {
  public event EventHandler<ScrollEventArgs> OnScroll; 
  public SCROLLINFO ScrollInfo { get; set; }
  public VscrollBarEx() {
    this.SetStyle(
      ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                                          | ControlStyles.Selectable | ControlStyles.AllPaintingInWmPaint
                                          | ControlStyles.UserPaint, true);
    //this.BackColor = Color.Beige;
    this.Width = 16;
    this.Visible = false;
  }

  protected override void OnPaint(PaintEventArgs e) {
    var color = Color.FromArgb(128, 20, 20, 20);
    var brush = new System.Drawing.SolidBrush(color);
    e.Graphics.FillRectangle(brush, this.ClientRectangle);
    var percentage = (Double)this.ScrollInfo.nPos / (Double)(this.ScrollInfo.nMax - this.ScrollInfo.nPage) * 100D;
    var thPos = (Int32)Math.Round(this.Height * percentage / 100D, 0);
    var thumbSize = this.Height - (this.Height * ((this.ScrollInfo.nMax - this.Height)/(Double)this.Height));
    if (thumbSize < 50) {
      thumbSize = 50;
    }
    var offset = (Int32)Math.Round(thumbSize * percentage / 100D, 0);
    
    
    e.Graphics.FillRectangle(Brushes.DimGray, new Rectangle(0, thPos - offset, 16, (Int32)thumbSize));
    brush.Dispose();
    base.OnPaint(e);
  }
  public Int32 Value { get; set; }
  protected override void OnMouseMove(MouseEventArgs e) {
    if (e.Button == MouseButtons.Left) {
      var yPos = e.Location.Y;
      if (yPos < 0) {
        yPos = 0;
      }

      if (yPos > this.Height) {
        yPos = this.Height;
      }

      var currentPercentage =  (Int32)Math.Round(((yPos * (this.ScrollInfo.nMax - this.ScrollInfo.nPage))/(Double)this.Height), 0);
      var dy = currentPercentage - this.Value;
      if (Math.Abs(dy) >= 120) {



        //if (yPos == 0) {
        //  dy = -this.Value;
        //}

        this.OnScroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, dy));
        this.Value = currentPercentage;
        this.Refresh();
      }
    }
  }

  //protected override void WndProc(ref Message m) {
  //  if (m.Msg == (int)WM.WM_PAINT) {
  //    PAINTSTRUCT paintStruct;
  //    var hdc = BeginPaint(m.HWnd, out paintStruct);
  //    Gdi32.SetBkMode(hdc, 1);
  //    using (var g = Graphics.FromHdc(hdc)) {
  //      g.Clear(Color.Aqua);
  //    }

  //    EndPaint(m.HWnd, ref paintStruct);
  //    m.Result = IntPtr.Zero;
  //    return;
  //  }

  //  if (m.Msg == (int)WM.WM_NCPAINT) {
  //    m.Result = IntPtr.Zero;
  //    return; 
  //  }

  //  base.WndProc(ref m);
  //}

  [DllImport("user32.dll")]
  static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);
  [DllImport("user32.dll")]
  static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

  [StructLayout(LayoutKind.Sequential)]
  struct PAINTSTRUCT {
    public IntPtr hdc;
    public bool fErase;
    public User32.RECT rcPaint;
    public bool fRestore;
    public bool fIncUpdate;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
  }
}
