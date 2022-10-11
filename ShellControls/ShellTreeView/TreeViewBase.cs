using System;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using Settings;
using Timer = System.Windows.Forms.Timer;

namespace ShellControls.ShellTreeView {
  public class TreeViewBase : TreeView {
    [DllImport("user32.dll")]
    static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);
    #region Event Handlers
    public event EventHandler VerticalScroll;
    public event EventHandler StyleChanged;
    #endregion

    #region Public Members
    public const int TVS_EX_AUTOHSCROLL = 0x0020;
    public const int TVS_EX_FADEINOUTEXPANDOS = 0x0040;
    public int RightOffset = 0;

    public System.Windows.Controls.Primitives.ScrollBar VScrollBar {
      get {
        return this._VScrollBar;
      }
      set {
        this._VScrollBar = value;
        this._VScrollBar.ValueChanged += (sender, args) => {
          //SCROLLINFO scrollinfo = new SCROLLINFO();
          //scrollinfo.cbSize = (uint)Marshal.SizeOf( typeof( SCROLLINFO ) );
          //scrollinfo.fMask = ScrollInfoMask.SIF_POS;
          //scrollinfo.nPos = (int)args.NewValue;
          //SetScrollInfo(this.Handle, 1, ref scrollinfo, true);
          if (!this._PreventValueChangeEvent) {
            int pos = (int)args.NewValue;
            SCROLLINFO scrollinfo = new SCROLLINFO();
            scrollinfo.cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO));
            scrollinfo.fMask = ScrollInfoMask.SIF_POS;
            scrollinfo.nPos = pos;
            SetScrollInfo(this.Handle, 1, ref scrollinfo, true);
            IntPtr msgPosition = new IntPtr((pos << 16) + 4);
            //pos <<= 16;
            //uint wParam = (uint)4 | (uint)pos;
            User32.SendMessage(this.Handle, 0x0115, msgPosition, IntPtr.Zero);
          } else {
            this._PreventValueChangeEvent = false;
          }
        };
      }
    }

    #endregion

    #region Private Members
    private const int PRF_CLIENT = 4;

    private const int WM_PRINTCLIENT = 0x0318;
    private const int TVS_EX_DOUBLEBUFFER = 0x0004;
    private const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
    private const int TVM_SETBKCOLOR = TV_FIRST + 29;
    private const int TVM_SETTEXTCOLOR = TV_FIRST + 30;

    private const int TV_FIRST = 0x1100;
    private const int WM_ERASEBKGND = 0x0014;
    private System.Windows.Controls.Primitives.ScrollBar _VScrollBar;
    private Thread _ScrollThread;
    private ManualResetEvent _ResetEvent = new ManualResetEvent(true);
    private Timer _ScrollBarTimer = new Timer();
    private Boolean _PreventValueChangeEvent = false;
    private Boolean _IsKillThreads = false;
    #endregion

    #region Initializer
    public TreeViewBase() {
      //SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.EnableNotifyMessage | ControlStyles.AllPaintingInWmPaint, true);
      //ResizeRedraw = true;
      this.ItemHeight = 32;
      this.Font = new Font(this.Font.FontFamily, 8);
      this._ScrollBarTimer.Interval = 400;
      this._ScrollBarTimer.Tick += ScrollBarTimerOnTick;
      this.Resize += (sender, args) => {
        this.Refresh();
        //var handle = this.Handle;
        this._ResetEvent.Set();
        if (!this._ScrollBarTimer.Enabled) {
          this._ScrollBarTimer.Start();
        }

        //this.BeginInvoke((Action)(() => { this.UpdateScrollbarInfo(); }));
      };
      this.MouseWheel += OnMouseWheel;
      this.AfterExpand += OnAfterExpand;
      this.AfterCollapse += OnAfterCollapse;
    }

    private void OnAfterCollapse(object sender, TreeViewEventArgs e) {
      this._ResetEvent.Set();
      if (!this._ScrollBarTimer.Enabled) {
        this._ScrollBarTimer.Start();
      }
    }

    private void OnAfterExpand(object sender, TreeViewEventArgs e) {
      this.RequestScrollBarUpdate();
    }

    private void ScrollBarTimerOnTick(object? sender, EventArgs e) {
      this._ResetEvent.Reset();
      this._ScrollBarTimer.Stop();
    }

    public void RequestScrollBarUpdate() {
      this._ResetEvent.Set();
      if (!this._ScrollBarTimer.Enabled) {
        this._ScrollBarTimer.Start();
      }
    }

    public void UpdateScrollbarInfo() {
      var scrollInfo = this.GetScrollPosition();
      if (scrollInfo.nMax - scrollInfo.nPage != this.VScrollBar.Maximum) {
        this.VScrollBar.Maximum = scrollInfo.nMax - scrollInfo.nPage + 1;
        this.VScrollBar.Minimum = scrollInfo.nMin;
        this.VScrollBar.LargeChange = scrollInfo.nPage;
        //  //this.sbVertical.LargeChange = 120*3;
        //  //var vpsize = this.ShellViewEx.ClientRectangle.Height / 16D;
        //  //if (vpsize)
        var thumbSize = (this.ClientRectangle.Height * (this.ClientRectangle.Height / 20)) /
                        (this.VScrollBar.Maximum + (this.ClientRectangle.Height / 20));
        if (thumbSize < 50) {
          var vp = (this.VScrollBar.Maximum * 50) / (this.ClientRectangle.Height - 50);
          this.VScrollBar.ViewportSize = vp;
        } else {
          this.VScrollBar.ViewportSize = this.ClientRectangle.Height / 20;
        }
      }

      if (this.VScrollBar.Value != scrollInfo.nPos) {
        this._PreventValueChangeEvent = true;
        this.VScrollBar.Value = scrollInfo.nPos;
      }
    }

    private void OnMouseWheel(object? sender, MouseEventArgs e) {
      this.BeginInvoke((Action)(() => { this.UpdateScrollbarInfo(); }));
      //var scrollInfo = this.GetScrollPosition();
      //this.VScrollBar.Maximum = scrollInfo.nMax - scrollInfo.nPage;
      //this.VScrollBar.Minimum = scrollInfo.nMin;
      //this.VScrollBar.LargeChange = scrollInfo.nPage;
      ////  //this.sbVertical.LargeChange = 120*3;
      ////  //var vpsize = this.ShellViewEx.ClientRectangle.Height / 16D;
      ////  //if (vpsize)
      //var thumbSize = (this.ClientRectangle.Height * (this.ClientRectangle.Height / 20)) /
      //                (this.VScrollBar.Maximum + (this.ClientRectangle.Height / 20));
      //if (thumbSize < 50) {
      //  var vp = (this.VScrollBar.Maximum * 50) / (this.ClientRectangle.Height - 50);
      //  this.VScrollBar.ViewportSize = vp;
      //} else {
      //  this.VScrollBar.ViewportSize = this.ClientRectangle.Height / 20;
      //}

      //this.VScrollBar.Value = scrollInfo.nPos;
    }

    #endregion

    #region Overrides
    //protected override void OnPaint(PaintEventArgs e) {
    //  if (GetStyle(ControlStyles.UserPaint)) {
    //    Message m = new Message();
    //    m.HWnd = Handle;
    //    m.Msg = WM_PRINTCLIENT;
    //    m.WParam = e.Graphics.GetHdc();
    //    m.LParam = (IntPtr)PRF_CLIENT;
    //    DefWndProc(ref m);
    //    e.Graphics.ReleaseHdc(m.WParam);
    //  }
    //  base.OnPaint(e);
    //}
    //protected override void OnNotifyMessage(Message m) {
    //  //Filter out the WM_ERASEBKGND message
    //  if (m.Msg != WM_ERASEBKGND) {
    //    base.OnNotifyMessage(m);
    //  }
    //}

    protected override void OnHandleCreated(EventArgs e) {
      base.OnHandleCreated(e);
      SetDoubleBuffer();
      SetExpandoesStyle();
      SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_AUTOHSCROLL, (IntPtr)TVS_EX_AUTOHSCROLL);
      //SendMessage(this.Handle, TVM_SETBKCOLOR, IntPtr.Zero, Color.Black.ToWin32Color());
      //SendMessage(this.Handle, TVM_SETTEXTCOLOR, IntPtr.Zero, Color.White.ToWin32Color());
      //UxTheme.AllowDarkModeForApp(true);
      //UxTheme.AllowDarkModeForWindow(this.Handle, true);
      //UxTheme.SetWindowTheme(this.Handle, "Explorer", 0);
      this.ChangeTheme(ThemeColors.Dark);
      try {
        this._ScrollThread = new Thread(() => {
          while (!this._IsKillThreads) {
            Thread.Sleep(100);
            this._ResetEvent.WaitOne();
            this.BeginInvoke((Action)(() => { this.UpdateScrollbarInfo(); }));
          }
        });
        this._ScrollThread.Priority = ThreadPriority.Lowest;
        this._ScrollThread.IsBackground = true;
        this._ScrollThread.Start();
      } catch (ThreadInterruptedException ex) {
        
      }

    }

    protected override void OnHandleDestroyed(EventArgs e) {
      this._IsKillThreads = true;
      base.OnHandleDestroyed(e);
    }

    public void ChangeTheme(ThemeColors theme) {
      var themeStruct = new LVTheme(theme);
      //UxTheme.AllowDarkModeForApp(theme == ThemeColors.Dark);
      SendMessage(this.Handle, TVM_SETBKCOLOR, IntPtr.Zero, themeStruct.BackgroundColorTree.ToDrawingColor().ToWin32Color());
      SendMessage(this.Handle, TVM_SETTEXTCOLOR, IntPtr.Zero, themeStruct.TextColor.ToDrawingColor().ToWin32Color());
      UxTheme.AllowDarkModeForWindow(this.Handle, theme == ThemeColors.Dark);
      UxTheme.SetWindowTheme(this.Handle, "Explorer", 0);
      //UxTheme.FlushMenuThemes();
      //this.Theme = new LVTheme(theme);
      //this.VScroll.Theme = this.Theme;
      //this._IIListView.SetBackgroundColor(this.Theme.HeaderBackgroundColor.ToDrawingColor().ToWin32Color());
      //this._IIListView.SetTextColor(this.Theme.TextColor.ToDrawingColor().ToWin32Color());
      //this._IIVisualProperties.SetColor(VPCOLORFLAGS.VPCF_SORTCOLUMN, this.Theme.SortColumnColor.ToDrawingColor().ToWin32Color());

    }
    public SCROLLINFO GetScrollPosition() {
      var scrollinfo = new SCROLLINFO {
        cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO)),
        fMask = ScrollInfoMask.SIF_ALL
      };
      if (User32.GetScrollInfo(this.Handle, (int)SBOrientation.SB_VERT, ref scrollinfo)) {
        return scrollinfo;
      } else {
        return new SCROLLINFO();
      }
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct STYLESTRUCT {
      public long styleOld;
      public long styleNew;
    }
    [HandleProcessCorruptedStateExceptions]
    protected override void WndProc(ref Message m) {
      base.WndProc(ref m);
      try {
        if (m.Msg == WM_ERASEBKGND) {
          m.Result = IntPtr.Zero;
          return;
        }
        if (m.Msg == (int)WM.WM_NCPAINT) {
          var currentStyle = User32.GetWindowLong(this.Handle, -16);
          var vScrollbarWidth = 0;
          if ((currentStyle & 0x00200000) != 0) {
            vScrollbarWidth = 18;
            try {
              this.BeginInvoke((Action)(() => { this.UpdateScrollbarInfo(); }));
              this.BeginInvoke((Action)(() => {
                this.VScrollBar.Visibility = Visibility.Visible;
              }));

            } catch (Exception ex) {

            }

          } else {
            try {
              this.VScrollBar.Visibility = Visibility.Collapsed;
            } catch (Exception ex) {
            
            }
          }

          this.RightOffset = vScrollbarWidth;
          //this.StyleChanged?.Invoke(this, EventArgs.Empty);
        }

        if (m.Msg == 0x007D) {
          if (m.WParam == (IntPtr)(-16)) {
            var structStyle = (STYLESTRUCT)m.GetLParam(typeof(STYLESTRUCT));
            var vScrollbarWidth = 0;
            if ((structStyle.styleNew & 0x00200000) != 0 && (structStyle.styleOld & 0x00200000) == 0) {
              this.RightOffset = 18;
              //this.StyleChanged?.Invoke(this, EventArgs.Empty);
              this.BeginInvoke((Action)(() => { this.UpdateScrollbarInfo(); }));
              this.VScrollBar.Visibility = Visibility.Visible;
            }
            if ((structStyle.styleOld & 0x00200000) != 0 && (structStyle.styleNew & 0x00200000) == 0) {
              this.RightOffset = 0;
              this.VScrollBar.Visibility = Visibility.Collapsed;
              //this.StyleChanged?.Invoke(this, EventArgs.Empty);
            }
            //this.RightOffset = vScrollbarWidth;
            //User32.MoveWindow(this.Handle, 0, 0, this.ClientRectangle.Width + vScrollbarWidth, this.ClientRectangle.Height, false);
          }
        }

      } catch (AccessViolationException) { }
    }
    protected override CreateParams CreateParams {
      get {
        CreateParams cp = base.CreateParams;
        cp.Style |= 0x8000; // TVS_NOHSCROLL
        return cp;
      }
    }
    #endregion

    #region Unmanaged
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    #endregion

    #region Private Methods

    private void SetExpandoesStyle() {
      int Style = 0;

      Style |= TVS_EX_FADEINOUTEXPANDOS;

      if (Style != 0)
        SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_FADEINOUTEXPANDOS, (IntPtr)Style);
    }
    private void SetDoubleBuffer() {
      int Style = 0;

      Style |= TVS_EX_DOUBLEBUFFER;

      if (Style != 0)
        SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)Style);
    }
    #endregion
  }
}
