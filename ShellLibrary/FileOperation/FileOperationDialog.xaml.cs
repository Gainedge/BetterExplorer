using Fluent;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace BExplorer.Shell {

  /// <summary>
  /// Interaction logic for FileOperationDialog.xaml
  /// </summary>
  public partial class FileOperationDialog : RibbonWindow {
    private bool IsShown = false;

    public int OveralProgress { get; set; }

    public ObservableCollection<FileOperation> Contents { get; set; }

    private DispatcherTimer LoadTimer;

    public FileOperationDialog() {
      this.DataContext = this;
      Contents = new ObservableCollection<FileOperation>();
      Contents.CollectionChanged += Contents_CollectionChanged;
      InitializeComponent();
      this.Owner = Application.Current.MainWindow;
      //var Name = Environment.OSVersion.Version.ToString().StartsWith("6.1") ? "Windows 7" : "Windows 8";
      //Background = Name == "Windows 7" ? Brushes.WhiteSmoke : Brushes.White;
      //Background = Theme.Background;

      //ensure win32 handle is created
      //var handle = new WindowInteropHelper(this).EnsureHandle();

      //set window background
      /*var result =*/
      //SetClassLong(handle, GCL_HBRBACKGROUND, ((System.Windows.Media.Color)System.Windows.Application.Current.Resources["Fluent.Ribbon.Brushes.RibbonTabControl.Background"]).ToDrawingColor().ToWin32Color());

      //if (!IsShown) {
      if (LoadTimer == null) {
        LoadTimer = new DispatcherTimer();
        LoadTimer.Interval = TimeSpan.FromMilliseconds(500);
        LoadTimer.Tick += LoadTimer_Tick;
      }
      LoadTimer.Stop();
      //}
    }

    public void AddFileOperation(FileOperation op) {
      if (!this.Contents.Contains(op)) {
        this.Contents.Add(op);
        if (this.Contents.Count == 1) {
          this.LoadTimer.Start();
          //this.Show();
        }
      } else {
        op.pnlFileCollision.Visibility = Visibility.Collapsed;
        op.pnlFileOp.Visibility = Visibility.Visible;
      }
    }

    public void RemoveFileOperation(FileOperation op) {
      this.Contents.Remove(op);
      if (this.Contents.Count == 0) {
        op.Owner.OperationDialog = null;
        this.Close();
      }
    }

    private void Contents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      this.Title = $"{this.Contents.Count} tasks running";
    }

    public static IntPtr SetClassLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
      //check for x64
      if (IntPtr.Size > 4)
        return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
      else
        return new IntPtr(SetClassLongPtr32(hWnd, nIndex, unchecked((uint)dwNewLong.ToInt32())));
    }

    private const int GCL_HBRBACKGROUND = -10;
    private const int COLOR_WINDOW = 5;

    [DllImport("user32.dll", EntryPoint = "SetClassLong")]
    public static extern uint SetClassLongPtr32(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")]
    public static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSysColorBrush(int nIndex);

    protected override void OnContentRendered(EventArgs e) {
      base.OnContentRendered(e);
      this.IsShown = true;
    }

    private void LoadTimer_Tick(object sender, EventArgs e) {
      //  Dispatcher.Invoke(DispatcherPriority.Normal,
      //          (Action)(() => {
      //            if (!this.IsShown) {
      //              (sender as DispatcherTimer).Stop();
      //              if (Contents.Any(c => c.Visibility == Visibility.Visible)) {
      //                this.ShowActivated = this.OwnedWindows.Count <= 0;

      //                /*
      //                Why was this duplicate code here
      //                if (this.OwnedWindows.Count > 0)
      //  this.ShowActivated = false;
      //else
      //  this.ShowActivated = true;
      //                */

      //                this.Show();
      //              } else {
      //                this.Close();
      //              }
      //            }
      //          }));
      this.LoadTimer.Stop();
      this.Show();
      this.SizeToContent = SizeToContent.Height;
    }

    /*
public void SetTaskbarProgress() {
//Taskbar.TaskbarManager.Instance.SetProgressValue(this.OveralProgress/this.Contents.Count, 100);
}
*/

    private void Window_Closed(object sender, EventArgs e) {
      this.LoadTimer.Stop();
      foreach (FileOperation item in this.Contents) {
        item.Owner.OperationDialog = null;
      }
      //Taskbar.TaskbarManager.Instance.SetProgressValue(0, 100);
      //Taskbar.TaskbarManager.Instance.SetProgressState(Taskbar.TaskbarProgressBarState.NoProgress);
    }
  }
}