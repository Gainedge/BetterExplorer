using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;
using TsudaKageyu;
using WPFUI.Win32;

namespace BetterExplorer {

  public partial class IconView : Form {
    private List<IconFile> _Icons = null;
    private ShellView _ShellView;
    private bool _IsLibrary;
    private readonly VisualStyleRenderer _ItemSelectedRenderer = new VisualStyleRenderer("Explorer::ListView", 1, 3);
    private readonly VisualStyleRenderer _ItemHoverRenderer = new VisualStyleRenderer("Explorer::ListView", 1, 2);
    private readonly VisualStyleRenderer _Selectedx2Renderer = new VisualStyleRenderer("Explorer::ListView", 1, 6);

    public IconView() {
      InitializeComponent();
      UxTheme.SetWindowTheme(lvIcons.Handle, "explorer", 0);
    }

    public void LoadIcons(ShellView shellView, bool isLibrary) {
      tbLibrary.Text = @"C:\Windows\System32\imageres.dll";
      this._ShellView = shellView;
      this._IsLibrary = isLibrary;
      ShowDialog();
    }

    private void lvIcons_DrawItem(object sender, DrawListViewItemEventArgs e) {
      e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
      e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
      e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

      if ((e.State & ListViewItemStates.Hot) != 0 && (e.State & ListViewItemStates.Selected) == 0) {
        this._ItemHoverRenderer.DrawBackground(e.Graphics, e.Bounds);
      } else if ((e.State & ListViewItemStates.Hot) != 0 && (e.State & ListViewItemStates.Selected) != 0) {
        this._Selectedx2Renderer.DrawBackground(e.Graphics, e.Bounds);
      } else if ((e.State & ListViewItemStates.Selected) != 0) {
        this._ItemSelectedRenderer.DrawBackground(e.Graphics, e.Bounds);
      } else {
        e.DrawBackground();
      }
      var ico = _Icons[(int)e.Item.Tag].Icon;
      if (ico.Width <= 48) {
        e.Graphics.DrawIcon(_Icons[(int)e.Item.Tag].Icon,
            e.Bounds.X + (e.Bounds.Width - ico.Width) / 2, e.Bounds.Y + (e.Bounds.Height - ico.Height) / 2 - 5);
      }
      e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter | TextFormatFlags.WordEllipsis);
    }

    private void btnLoad_Click(object sender, EventArgs e) {
      var dlg = new System.Windows.Forms.OpenFileDialog() {
        AutoUpgradeEnabled = true,
        Title = "Select icon file",
        Filter = "Icon Files |*.exe;*.dll;*.icl; *.ico"
      };

      if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
        tbLibrary.Text = dlg.FileName;
      }

      var bw = new BackgroundWorker();
      bw.DoWork += new DoWorkEventHandler(bw_DoWork);
      bw.WorkerReportsProgress = true;
      bw.WorkerSupportsCancellation = true;
      bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
      pbProgress.Visible = true;
      bw.RunWorkerAsync();
    }

    private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      lvIcons.BeginUpdate();
      lvIcons.Items.Clear();
      foreach (IconFile icon in _Icons) {
        lvIcons.Items.Add(new ListViewItem("#" + icon.Index.ToString()) { Tag = icon.Index });
      }

      lvIcons.EndUpdate();
      pbProgress.Visible = false;
    }

    private void bw_DoWork(object sender, DoWorkEventArgs e) {
      var ie = new TsudaKageyu.IconExtractor(tbLibrary.Text);
      var i = 0;
      this._Icons = ie.GetAllIcons().Select(s => new IconFile() { Icon = s, Index = i++ }).ToList();
    }

    private void LoadIcons(object Params) {
      Invoke(new MethodInvoker(
              delegate {
                lvIcons.BeginUpdate();
                lvIcons.Items.Clear();
                var ie = new TsudaKageyu.IconExtractor(tbLibrary.Text);
                var i = 0;
                _Icons = ie.GetAllIcons().Select(s => new IconFile() { Icon = s, Index = i++ }).ToList();
                foreach (var icon in _Icons) {
                  lvIcons.Items.Add(new ListViewItem("#" + icon.Index.ToString()) { Tag = icon.Index });
                }
                lvIcons.EndUpdate();
              }));
    }

    private void btnSet_Click(object sender, EventArgs e) {
      var itemIndex = _ShellView.GetFirstSelectedItemIndex();
      this._ShellView.CurrentRefreshedItemIndex = itemIndex;
      if (this._IsLibrary) {
        //this._ShellView.IsLibraryInModify = true;
        var lib = _ShellView.GetFirstSelectedItem() != null ?
          BExplorer.Shell.ShellLibrary.Load(Path.GetFileNameWithoutExtension(_ShellView.GetFirstSelectedItem().ParsingName), false) :
          BExplorer.Shell.ShellLibrary.Load(Path.GetFileNameWithoutExtension(_ShellView.CurrentFolder.ParsingName), false);

        lib.IconResourceId = new BExplorer.Shell.Interop.IconReference(tbLibrary.Text, (int)lvIcons.SelectedItems[0].Tag);
        lib.Close();

        this._ShellView.Items[itemIndex].IsIconLoaded = false;
        this._ShellView.RefreshItem(this._ShellView.GetFirstSelectedItemIndex(), this._ShellView.Items[itemIndex].EnumPIDL);
      } else {
        this._ShellView.SetFolderIcon(this._ShellView.GetFirstSelectedItem().ParsingName, tbLibrary.Text, (int)lvIcons.SelectedItems[0].Tag);
      }

      this.Close();
    }

    private BackgroundWorker bw = new BackgroundWorker();

    private void LoadLib() {
      bw.DoWork += new DoWorkEventHandler(bw_DoWork);
      bw.WorkerReportsProgress = true;
      bw.WorkerSupportsCancellation = true;
      bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
      pbProgress.Visible = true;
      if (bw.IsBusy) {
        bw.CancelAsync();
      }
      bw.RunWorkerAsync();
    }

    private void IconView_Load(object sender, EventArgs e) {
      this.LoadLib();
    }

    private void tbLibrary_KeyUp(object sender, KeyEventArgs e) {
      if (e.KeyData == Keys.Enter) {
        this.LoadLib();
      }
    }

    protected override void WndProc(ref Message m) {
      switch ((WM)m.Msg) {
        case WM.WM_CREATE:
          WPFUI.Background.Manager.ApplyDarkMode(m.HWnd);
          WPFUI.Background.Manager.Apply(WPFUI.Background.BackgroundType.Mica, m.HWnd);
          break;
        case WM.WM_ACTIVATE:
          var margins = new Dwmapi.MARGINS(new Thickness(-1));
          Dwmapi.DwmExtendFrameIntoClientArea(m.HWnd, ref margins);
          m.Result = IntPtr.Zero;
          break;
      }
      base.WndProc(ref m);
    }
  }

  public class ListView : System.Windows.Forms.ListView {

    public ListView() {
      //Activate double buffering
      this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

      //Enable the OnNotifyMessage event so we get a chance to filter out
      // Windows messages before they get to the form's WndProc
      this.SetStyle(ControlStyles.EnableNotifyMessage, true);
    }

    protected override void OnNotifyMessage(Message m) {
      //Filter out the WM_ERASEBKGND message
      if (m.Msg != 0x14) {
        base.OnNotifyMessage(m);
      }
    }
  }
}