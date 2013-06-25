using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsHelper;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations {
  /// <summary>
  /// Interaction logic for FileOperationDialog.xaml
  /// </summary>
  public partial class FileOperationDialog : Window {

    private bool IsShown = false;
    public int OveralProgress { get; set; }
    public ObservableCollection<FileOperation> Contents { get; set; }
    System.Windows.Forms.Timer LoadTimer;
    public FileOperationDialog() {
      this.DataContext = this;
      Contents = new ObservableCollection<FileOperation>();
      Contents.CollectionChanged += Contents_CollectionChanged;
      InitializeComponent();
	    Background = Theme.Background;
      //ensure win32 handle is created
      var handle = new WindowInteropHelper(this).EnsureHandle();

      //set window background
      var result = SetClassLong(handle, GCL_HBRBACKGROUND, GetSysColorBrush(COLOR_WINDOW));


      if (!IsShown) {
        if (LoadTimer == null) {
          LoadTimer = new System.Windows.Forms.Timer();
          LoadTimer.Interval = 1500;
          LoadTimer.Tick += LoadTimer_Tick;
          LoadTimer.Start();
        }
      }

    }

    void Contents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      this.Title = String.Format("{0} tasks running", this.Contents.Count);
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
    static extern IntPtr GetSysColorBrush(int nIndex);
    protected override void OnContentRendered(EventArgs e) {
      base.OnContentRendered(e);
      this.IsShown = true;
    }



    void LoadTimer_Tick(object sender, EventArgs e) {
       Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
      (Action)(() => {
        if (!this.IsShown) {
          (sender as System.Windows.Forms.Timer).Stop();
          if (Contents.Count(c => c.Visibility == System.Windows.Visibility.Visible) > 0) {
            if (this.OwnedWindows.Count > 0)
              this.ShowActivated = false;
            else
              this.ShowActivated = true;
            
            this.Show();
          } else
            this.Close();
        }
      }));
    }

    public void SetTaskbarProgress()
    {
      Taskbar.TaskbarManager.Instance.SetProgressValue(this.OveralProgress/this.Contents.Count, 100);
      
    }

    private void Window_Closed(object sender, EventArgs e) {
      foreach (FileOperation item in this.Contents) {
        item.Cancel = true;
      }
      Taskbar.TaskbarManager.Instance.SetProgressValue(0, 100);
      Taskbar.TaskbarManager.Instance.SetProgressState(Taskbar.TaskbarProgressBarState.NoProgress);
    }

  }
}
