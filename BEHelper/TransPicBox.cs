using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BEHelper
{
  public class TransPicBox : Control
  {
    public TransPicBox()
    {
      this.DoubleBuffered = true;
      this.SetStyle(ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.ContainerControl |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.SupportsTransparentBackColor
                    , true);
    }
    public TransPicBox(string text)
      : base(text)
    {
      
    }
    public TransPicBox(string text, int left, int top, int width, int height)
      : base(text, left, top, width, height)
    {
      
    }
    public TransPicBox(Control parent, string text)
      : base(parent, text)
    {
      
    }
    public TransPicBox(Control parent, string text, int left, int top, int width, int height)
      : base(parent, text, left, top, width, height)
    {
      
    }


    private static Bitmap resizeImage(Bitmap imgToResize, Size size)
    {
      int sourceWidth = imgToResize.Width;
      int sourceHeight = imgToResize.Height;

      float nPercent = 0;
      float nPercentW = 0;
      float nPercentH = 0;

      nPercentW = ((float)size.Width / (float)sourceWidth);
      nPercentH = ((float)size.Height / (float)sourceHeight);

      if (nPercentH < nPercentW)
        nPercent = nPercentH;
      else
        nPercent = nPercentW;

      int destWidth = (int)(sourceWidth * nPercent);
      int destHeight = (int)(sourceHeight * nPercent);

      Bitmap b = new Bitmap(destWidth, destHeight);
      Graphics g = Graphics.FromImage(b);
      g.InterpolationMode = InterpolationMode.HighQualityBicubic;

      g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
      g.Dispose();

      return b;
    }


    public Bitmap Image { get; set; }

    protected override CreateParams CreateParams
    {
      get
      {
        CreateParams cp = base.CreateParams;
        cp.ExStyle |= 0x02000000;
        return cp;
      }
    }
    protected override void OnResize(EventArgs e)
    {
      this.Update();
      base.OnResize(e);
    }
    protected override void OnSizeChanged(EventArgs e)
    {
      this.Update();
      base.OnSizeChanged(e);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      if (this.Image != null)
      {
        var res = resizeImage(this.Image, this.ClientSize);
        e.Graphics.FillRectangle(SystemBrushes.Window, this.ClientRectangle);
        e.Graphics.DrawImage(res, (this.Size.Width - res.Width) / 2, (this.Size.Height - res.Height) / 2);
      }
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
      
    }
  }
}
