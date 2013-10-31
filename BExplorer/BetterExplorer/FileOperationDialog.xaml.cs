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
using BEFileOperation;
using System.Windows.Threading;
using System.Collections.Specialized;

namespace BetterExplorer
{
  /// <summary>
  /// Interaction logic for FileOperationDialog.xaml
  /// </summary>
  public partial class FileOperationDialog : Window {

    private bool IsShown = false;
    public int OveralProgress { get; set; }
    System.Windows.Forms.Timer LoadTimer;
    public FileOperationDialog() {
      this.DataContext = this;
      
      InitializeComponent();
	    Background = Microsoft.WindowsAPICodePack.Shell.FileOperations.Theme.Background;
      //ensure win32 handle is created
      var handle = new WindowInteropHelper(this).EnsureHandle();
      ((INotifyCollectionChanged)stack.Items).CollectionChanged += Contents_CollectionChanged;
      //set window background
      var result = SetClassLong(handle, GCL_HBRBACKGROUND, GetSysColorBrush(COLOR_WINDOW));

      XProcAddInHost host = new XProcAddInHost();
      host.AddInAvailableAsync += host_AddInAvailableAsync;
      //XProcAddInHost host2 = new XProcAddInHost(true);
      //host2.AddInAvailableAsync += host2_AddInAvailableAsync;

      //if (!IsShown) {
      //  if (LoadTimer == null) {
      //    LoadTimer = new System.Windows.Forms.Timer();
      //    LoadTimer.Interval = 1500;
      //    LoadTimer.Tick += LoadTimer_Tick;
      //    LoadTimer.Start();
      //  }
      //}

    }

    void host2_AddInAvailableAsync(object sender, EventArgs e)
    {
      ((XProcAddInHost)sender).AddInAvailableAsync -= host_AddInAvailableAsync;
      // Go back to the UI thread and attach the add-in element to the tree.
      Dispatcher.BeginInvoke(new SendOrPostCallback((addinHost) =>
      {
        XProcAddInHost child = (XProcAddInHost)addinHost;
        Border br = new Border();
        br.BorderBrush = Brushes.LightGray;
        br.BorderThickness = new Thickness(1);
        br.Child = child;
        stack.Items.Add(br);
        if (!this.IsVisible)
          Show();
      }), sender);
    }

    void host_AddInAvailableAsync(object sender, EventArgs e)
    {
      ((XProcAddInHost)sender).AddInAvailableAsync -= host_AddInAvailableAsync;
      // Go back to the UI thread and attach the add-in element to the tree.
      Dispatcher.BeginInvoke(new SendOrPostCallback((addinHost) =>
      {
        XProcAddInHost child = (XProcAddInHost)addinHost;
        Border br = new Border();
        br.BorderBrush = Brushes.LightGray;
        br.BorderThickness = new Thickness(1);
        br.Child = child;
        stack.Items.Add(br);
        if (!this.IsVisible)
          Show();
      }), sender);

      

    }

    void Contents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      this.Title = String.Format("{0} tasks running", this.stack.Items.Count);
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
      // Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
      //(Action)(() => {
      //  if (!this.IsShown) {
      //    (sender as System.Windows.Forms.Timer).Stop();
      //    if (Contents.Count(c => c.Visibility == System.Windows.Visibility.Visible) > 0) {
      //      if (this.OwnedWindows.Count > 0)
      //        this.ShowActivated = false;
      //      else
      //        this.ShowActivated = true;
      //      
      //      this.Show();
      //    } else
      //      this.Close();
      //  }
      //}));
    }

    public void SetTaskbarProgress()
    {
      Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.SetProgressValue(this.OveralProgress/this.stack.Items.Count, 100);
      
    }
    void UnloadAddIn(Border item)
    {
      XProcAddInHost host = (XProcAddInHost)item.Child;
      if (host != null)
      {
        item.Child = null;
        host.Dispose();
        // XProcAddInHost.DestroyWindowCore() tells the add-in to shut down.
      }
    }

    private void Window_Closed(object sender, EventArgs e) {
      foreach (Border item in stack.Items)
      {
        Dispatcher.BeginInvoke((Action)delegate()
        {
          UnloadAddIn(item);
        });
        

      }
      Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.SetProgressValue(0, 100);
      Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress);
    }

  }
}
