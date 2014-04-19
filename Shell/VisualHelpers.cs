using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell
{
  public class VisualHelpers
  {

    #region Private Methods

    protected static GraphicsPath GetRoundedRect(RectangleF rect, float diameter)
    {
      GraphicsPath path = new GraphicsPath();

      if (diameter <= 0.0f)
      {
        path.AddRectangle(rect);
      }
      else
      {
        RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
        path.AddArc(arc, 180, 90);
        arc.X = rect.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = rect.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = rect.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
      }

      return path;
    }
    private static void DrawOverlay(Graphics g, Rectangle bounds, Pen borderPen, float cornerRadius, Size boundsPadding, Color? fillGradientFrom, Color? fillGradientTo, LinearGradientMode fillGradientMode, bool isDetails)
    {
      System.Drawing.Drawing2D.LinearGradientBrush fillBrush = null;
      if (isDetails)
      {
        bounds.Width = bounds.Width - 1;
        bounds.Height = bounds.Height - 1;
      }
      bounds.Inflate(boundsPadding);
      GraphicsPath path = GetRoundedRect(bounds, cornerRadius);
      if (fillGradientFrom != null && fillGradientTo != null)
      {
        fillBrush = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, fillGradientFrom.Value, fillGradientTo.Value, fillGradientMode);
      }
      if (fillBrush != null)
        g.FillPath(fillBrush, path);
      if (borderPen != null)
        g.DrawPath(borderPen, path);
    }
    #endregion

    #region Public Methods

    public static void DrawSelected(Graphics g, Rectangle bounds, Size padding, bool isDetails)
    {
      DrawOverlay(g, bounds, new Pen(Color.FromArgb(154, 223, 251)), isDetails ? 2.0f : 6.0f, padding, Color.FromArgb(0, 163, 217, 225), Color.FromArgb(0xD6, 0xC1, 0xDC, 0xFC), LinearGradientMode.Vertical, isDetails);
    }
    public static void DrawHot(Graphics g, Rectangle bounds, Size padding, bool isDetails)
    {
      DrawOverlay(g, bounds, new Pen(Color.FromArgb(154, 223, 251)), isDetails ? 2.0f : 6.0f, padding, Color.FromArgb(0, 255, 255, 255), Color.FromArgb(64, 183, 237, 240), LinearGradientMode.ForwardDiagonal, isDetails);
    }
    #endregion

    
  }
}
