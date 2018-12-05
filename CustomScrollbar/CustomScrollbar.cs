using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Settings;


namespace CustomScrollbar {

  [Designer(typeof(ScrollbarControlDesigner))]
  public class CustomScrollbar : UserControl {

    protected Color moChannelColor = Color.Empty;
    protected Int32 ArrowsSize = 22;

    //protected Image moUpArrowImage_Over = null;
    //protected Image moUpArrowImage_Down = null;

    //protected Image moDownArrowImage_Over = null;
    //protected Image moDownArrowImage_Down = null;
    protected Image moThumbArrowImage = null;

    protected Image moThumbTopImage = null;
    protected Image moThumbTopSpanImage = null;
    protected Image moThumbBottomImage = null;
    protected Image moThumbBottomSpanImage = null;
    protected Image moThumbMiddleImage = null;

    protected int moLargeChange = 10;
    protected int moSmallChange = 1;
    protected int moMinimum = 0;
    protected int moMaximum = 100;
    protected int moValue = 0;
    private int nClickPoint;

    protected int moThumbTop = 0;

    protected bool moAutoSize = false;

    public LVTheme Theme { get; set; }

    private bool moThumbDown = false;
    private bool moThumbDragging = false;

    public new event EventHandler Scroll = null;
    public event EventHandler ValueChanged = null;

    private int GetThumbHeight() {
      int nTrackHeight = (this.Height - (this.ArrowsSize + this.ArrowsSize));
      float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
      int nThumbHeight = (int)fThumbHeight;

      if (nThumbHeight > nTrackHeight) {
        nThumbHeight = nTrackHeight;
        fThumbHeight = nTrackHeight;
      }
      if (nThumbHeight < 56) {
        nThumbHeight = 56;
        fThumbHeight = 56;
      }

      return nThumbHeight;
    }

    public CustomScrollbar() {


      InitializeComponent();
      SetStyle(ControlStyles.ResizeRedraw, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.DoubleBuffer, true);

      moChannelColor = Color.Black;
      ThumbBottomImage = Resource.ThumbBottom;
      ThumbBottomSpanImage = Resource.ThumbSpanBottom;
      ThumbTopImage = Resource.ThumbTop;
      ThumbTopSpanImage = Resource.ThumbSpanTop;
      ThumbMiddleImage = Resource.ThumbMiddle;

      this.Width = SystemInformation.VerticalScrollBarWidth;
      base.MinimumSize = new Size(SystemInformation.VerticalScrollBarWidth, this.ArrowsSize + this.ArrowsSize + GetThumbHeight());
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("LargeChange")]
    public int LargeChange {
      get { return moLargeChange; }
      set {
        moLargeChange = value;
        Invalidate();
      }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("SmallChange")]
    public int SmallChange {
      get { return moSmallChange; }
      set {
        moSmallChange = value;
        Invalidate();
      }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Minimum")]
    public int Minimum {
      get { return moMinimum; }
      set {
        moMinimum = value;
        Invalidate();
      }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Maximum")]
    public int Maximum {
      get { return moMaximum; }
      set {
        moMaximum = value;
        Invalidate();
      }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Value")]
    public int Value {
      get { return moValue; }
      set {
        moValue = value;

        int nTrackHeight = (this.Height - (this.ArrowsSize + this.ArrowsSize));
        float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
        int nThumbHeight = (int)fThumbHeight;

        if (nThumbHeight > nTrackHeight) {
          nThumbHeight = nTrackHeight;
          fThumbHeight = nTrackHeight;
        }
        if (nThumbHeight < 56) {
          nThumbHeight = 56;
          fThumbHeight = 56;
        }

        //figure out value
        int nPixelRange = nTrackHeight - nThumbHeight;
        int nRealRange = (Maximum - Minimum) - LargeChange;
        float fPerc = 0.0f;
        if (nRealRange != 0) {
          fPerc = (float)moValue / (float)nRealRange;

        }

        float fTop = fPerc * nPixelRange;
        moThumbTop = (int)fTop;

        Invalidate();
        Application.DoEvents();
      }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Channel Color")]
    public Color ChannelColor {
      get { return moChannelColor; }
      set { moChannelColor = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
    public Image ThumbTopImage {
      get { return moThumbTopImage; }
      set { moThumbTopImage = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
    public Image ThumbTopSpanImage {
      get { return moThumbTopSpanImage; }
      set { moThumbTopSpanImage = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
    public Image ThumbBottomImage {
      get { return moThumbBottomImage; }
      set { moThumbBottomImage = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
    public Image ThumbBottomSpanImage {
      get { return moThumbBottomSpanImage; }
      set { moThumbBottomSpanImage = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
    public Image ThumbMiddleImage {
      get { return moThumbMiddleImage; }
      set { moThumbMiddleImage = value; }
    }

    protected override void OnPaint(PaintEventArgs e) {
      Brush oWhiteBrush = new SolidBrush(this.Theme.HeaderBackgroundColor.ToDrawingColor());
      Brush thumbBrush = new SolidBrush(this.Theme.SelectionColor.ToDrawingColor());
      Brush thumbBrushHover = new SolidBrush(this.Theme.SelectionBorderColor.ToDrawingColor());

      e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

      e.Graphics.FillRectangle(oWhiteBrush, new Rectangle(new Point(0, 0), new Size(this.Width, this.ArrowsSize)));
      var pen = new Pen(this.Theme.TextColor.ToDrawingColor(), 2);
      this.DrawArrowhead(e.Graphics, pen, new PointF(8.5f,8), 0, -4, 1);

      

      //draw channel left and right border colors
      e.Graphics.FillRectangle(oWhiteBrush, new Rectangle(0, this.ArrowsSize, 1, (this.Height - this.ArrowsSize)));
      e.Graphics.FillRectangle(oWhiteBrush, new Rectangle(this.Width - 1, this.ArrowsSize, 1, (this.Height - this.ArrowsSize)));

      //draw channel
      e.Graphics.FillRectangle(oWhiteBrush, new Rectangle(1, this.ArrowsSize, this.Width - 2, (this.Height - this.ArrowsSize)));

      //draw thumb
      int nTrackHeight = (this.Height - (this.ArrowsSize + this.ArrowsSize));
      float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
      int nThumbHeight = (int)fThumbHeight;

      if (nThumbHeight > nTrackHeight) {
        nThumbHeight = nTrackHeight;
        fThumbHeight = nTrackHeight;
      }
      if (nThumbHeight < 56) {
        nThumbHeight = 56;
        fThumbHeight = 56;
      }

      //Debug.WriteLine(nThumbHeight.ToString());

      float fSpanHeight = (fThumbHeight - (ThumbMiddleImage.Height + ThumbTopImage.Height + ThumbBottomImage.Height)) / 2.0f;
      int nSpanHeight = (int)fSpanHeight;

      int nTop = moThumbTop;
      nTop += this.ArrowsSize;
      var rect = new Rectangle(0, nTop, this.Width, nThumbHeight);
      var isHover = rect.Contains(this.PointToClient(Cursor.Position)) || this.moThumbDragging;
      e.Graphics.FillRectangle(isHover ? thumbBrushHover : thumbBrush, rect);
      ////draw top
      ////e.Graphics.DrawImage(ThumbTopImage, new Rectangle(1, nTop, this.Width - 2, ThumbTopImage.Height));
      //e.Graphics.FillRectangle(thumbBrush, new Rectangle(1, nTop, this.Width - 2, ThumbTopImage.Height));
      //nTop += ThumbTopImage.Height;
      ////draw top span
      //Rectangle rect = new Rectangle(1, nTop, this.Width - 2, nSpanHeight);


      ////e.Graphics.DrawImage(ThumbTopSpanImage, 1.0f,(float)nTop, (float)this.Width-2.0f, (float) fSpanHeight*2);
      //e.Graphics.FillRectangle(thumbBrush, 1.0f,(float)nTop, (float)this.Width-2.0f, (float) fSpanHeight*2);

      //nTop += nSpanHeight;
      ////draw middle
      ////e.Graphics.DrawImage(ThumbMiddleImage, new Rectangle(1, nTop, this.Width - 2, ThumbMiddleImage.Height));
      //e.Graphics.FillRectangle(thumbBrush, new Rectangle(1, nTop, this.Width - 2, ThumbMiddleImage.Height));


      //nTop += ThumbMiddleImage.Height;
      ////draw top span
      //rect = new Rectangle(1, nTop, this.Width - 2, nSpanHeight*2);
      ////e.Graphics.DrawImage(ThumbBottomSpanImage, rect);
      //e.Graphics.FillRectangle(thumbBrush, rect);

      //nTop += nSpanHeight;
      ////draw bottom
      ////e.Graphics.DrawImage(ThumbBottomImage, new Rectangle(1, nTop, this.Width - 2, nSpanHeight));
      //e.Graphics.FillRectangle(thumbBrush, new Rectangle(1, nTop, this.Width - 2, (int)fThumbHeight));

      e.Graphics.FillRectangle(oWhiteBrush, new Rectangle(new Point(0, (this.Height - this.ArrowsSize)), new Size(this.Width, this.ArrowsSize)));
      this.DrawArrowhead(e.Graphics, pen, new PointF(8.5f,(this.Height - this.ArrowsSize) + 12), 0, 4, 1);
      pen.Dispose();
      oWhiteBrush.Dispose();
      thumbBrush.Dispose();
      thumbBrushHover.Dispose();
    }

    public override bool AutoSize {
      get {
        return base.AutoSize;
      }
      set {
        base.AutoSize = value;
        if (base.AutoSize) {
          this.Width = 17;
        }
      }
    }

    private void InitializeComponent() {
      this.SuspendLayout();
      // 
      // CustomScrollbar
      // 
      this.Name = "CustomScrollbar";
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CustomScrollbar_MouseDown);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CustomScrollbar_MouseMove);
      this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CustomScrollbar_MouseUp);
      this.MouseHover += (sender, args) => { this.Invalidate(); };
      this.MouseLeave += (sender, args) => { this.Invalidate(); };
      this.ResumeLayout(false);

    }

    private void DrawArrowhead(Graphics gr, Pen pen, PointF p, float nx, float ny, float length) {
      float ax = length * (-ny - nx);
      float ay = length * (nx - ny);
      PointF[] points =
      {
        new PointF(p.X + ax, p.Y + ay),
        p,
        new PointF(p.X - ay, p.Y + ax)
      };
      gr.DrawLines(pen, points);
    }

    private void CustomScrollbar_MouseDown(object sender, MouseEventArgs e) {
      Point ptPoint = this.PointToClient(Cursor.Position);
      int nTrackHeight = (this.Height - (this.ArrowsSize + this.ArrowsSize));
      float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
      int nThumbHeight = (int)fThumbHeight;

      if (nThumbHeight > nTrackHeight) {
        nThumbHeight = nTrackHeight;
        fThumbHeight = nTrackHeight;
      }
      if (nThumbHeight < 56) {
        nThumbHeight = 56;
        fThumbHeight = 56;
      }

      int nTop = moThumbTop;
      nTop += this.ArrowsSize;


      Rectangle thumbrect = new Rectangle(new Point(1, nTop), new Size(ThumbMiddleImage.Width, nThumbHeight));
      if (thumbrect.Contains(ptPoint)) {

        //hit the thumb
        nClickPoint = (ptPoint.Y - nTop);
        //MessageBox.Show(Convert.ToString((ptPoint.Y - nTop)));
        this.moThumbDown = true;
      }

      Rectangle uparrowrect = new Rectangle(new Point(1, 0), new Size(this.Width, this.ArrowsSize));
      if (uparrowrect.Contains(ptPoint)) {

        int nRealRange = (Maximum - Minimum) - LargeChange;
        int nPixelRange = (nTrackHeight - nThumbHeight);
        if (nRealRange > 0) {
          if (nPixelRange > 0) {
            if ((moThumbTop - SmallChange) < 0)
              moThumbTop = 0;
            else
              moThumbTop -= SmallChange;

            //figure out value
            float fPerc = (float)moThumbTop / (float)nPixelRange;
            float fValue = fPerc * (Maximum - LargeChange);

            moValue = (int)fValue;
            Debug.WriteLine(moValue.ToString());
            Application.DoEvents();
            if (ValueChanged != null)
              ValueChanged(this, new EventArgs());

            if (Scroll != null)
              Scroll(this, new EventArgs());

            Invalidate();
          }
        }
      }

      Rectangle downarrowrect = new Rectangle(new Point(1, this.ArrowsSize + nTrackHeight), new Size(this.Width, this.ArrowsSize));
      if (downarrowrect.Contains(ptPoint)) {
        int nRealRange = (Maximum - Minimum) - LargeChange;
        int nPixelRange = (nTrackHeight - nThumbHeight);
        if (nRealRange > 0) {
          if (nPixelRange > 0) {
            if ((moThumbTop + SmallChange) > nPixelRange)
              moThumbTop = nPixelRange;
            else
              moThumbTop += SmallChange;

            //figure out value
            float fPerc = (float)moThumbTop / (float)nPixelRange;
            float fValue = fPerc * (Maximum - LargeChange);

            moValue = (int)fValue;
            Debug.WriteLine(moValue.ToString());
            Application.DoEvents();
            if (ValueChanged != null)
              ValueChanged(this, new EventArgs());

            if (Scroll != null)
              Scroll(this, new EventArgs());

            Invalidate();
          }
        }
      }
    }

    private void CustomScrollbar_MouseUp(object sender, MouseEventArgs e) {
      this.moThumbDown = false;
      this.moThumbDragging = false;
    }

    private void MoveThumb(int y) {
      int nRealRange = Maximum - Minimum;
      int nTrackHeight = (this.Height - (this.ArrowsSize + this.ArrowsSize));
      float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
      int nThumbHeight = (int)fThumbHeight;

      if (nThumbHeight > nTrackHeight) {
        nThumbHeight = nTrackHeight;
        fThumbHeight = nTrackHeight;
      }
      if (nThumbHeight < 56) {
        nThumbHeight = 56;
        fThumbHeight = 56;
      }

      int nSpot = nClickPoint;

      int nPixelRange = (nTrackHeight - nThumbHeight);
      if (moThumbDown && nRealRange > 0) {
        if (nPixelRange > 0) {
          int nNewThumbTop = y - (this.ArrowsSize + nSpot);

          if (nNewThumbTop < 0) {
            moThumbTop = nNewThumbTop = 0;
          } else if (nNewThumbTop > nPixelRange) {
            moThumbTop = nNewThumbTop = nPixelRange;
          } else {
            moThumbTop = y - (this.ArrowsSize + nSpot);
          }
          Application.DoEvents();
          //figure out value

          float fPerc = (float)(moThumbTop) / (float)nPixelRange;
          float fValue = fPerc * (Maximum - LargeChange);
          moValue = (int)fValue;
          Debug.WriteLine(moValue.ToString());
          this.Invalidate();
        }
      }
    }

    private void CustomScrollbar_MouseMove(object sender, MouseEventArgs e) {
      this.Invalidate();

      if (moThumbDown == true) {
        this.moThumbDragging = true;
      }

      if (this.moThumbDragging) {
        MoveThumb(e.Y);
        if (ValueChanged != null)
          ValueChanged(this, new EventArgs());

        if (Scroll != null)
          Scroll(this, new EventArgs());
      }



    }

  }

  internal class ScrollbarControlDesigner : System.Windows.Forms.Design.ControlDesigner {



    public override SelectionRules SelectionRules {
      get {
        SelectionRules selectionRules = base.SelectionRules;
        PropertyDescriptor propDescriptor = TypeDescriptor.GetProperties(this.Component)["AutoSize"];
        if (propDescriptor != null) {
          bool autoSize = (bool)propDescriptor.GetValue(this.Component);
          if (autoSize) {
            selectionRules = SelectionRules.Visible | SelectionRules.Moveable | SelectionRules.BottomSizeable | SelectionRules.TopSizeable;
          } else {
            selectionRules = SelectionRules.Visible | SelectionRules.AllSizeable | SelectionRules.Moveable;
          }
        }
        return selectionRules;
      }
    }
  }
}