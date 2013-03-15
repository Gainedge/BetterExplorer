using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
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
using System.Windows.Threading;
using PipesClient;
using PipesServer;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations {
  /// <summary>
  /// Interaction logic for FileOperation.xaml
  /// </summary>
  public partial class FileOperation : UserControl {
    public ShellObject[] SourceItemsCollection { get; set; }
    public ShellObject DestinationLocation { get; set; }
    public FileOperationDialog ParentContents { get; set; }
    public Boolean Cancel = false;
    public OperationType OperationType { get; set; }
    private ManualResetEvent _block;
    public Guid Handle;
    private int CurrentStatus = -1;
    private bool IsShown = false;
    Thread CopyThread;
    public delegate void NewMessageDelegate(string NewMessage);
    private Boolean IsAdminFO = false;
    private PipeServer _pipeServer;
    private PipeClient _pipeClient;
    System.Windows.Forms.Timer LoadTimer = new System.Windows.Forms.Timer();
    uint WM_FOWINC = WindowsAPI.RegisterWindowMessage("BE_FOWINC");
    uint WM_FOBEGIN = WindowsAPI.RegisterWindowMessage("BE_FOBEGIN");
    uint WM_FOEND = WindowsAPI.RegisterWindowMessage("BE_FOEND");
    uint WM_FOPAUSE = WindowsAPI.RegisterWindowMessage("BE_FOPAUSE");
    uint WM_FOSTOP = WindowsAPI.RegisterWindowMessage("BE_FOSTOP");

    public FileOperation(ShellObject[] _SourceItems, ShellObject _DestinationItem, OperationType _opType = OperationType.Copy) {
      _block = new ManualResetEvent(false);
      _block.Set();

      _pipeServer = new PipeServer();
      _pipeServer.PipeMessage += new DelegateMessage(PipesMessageHandler);
      _pipeServer.PipeFinished += _pipeServer_PipeFinished;
      this.SourceItemsCollection = _SourceItems;
      this.DestinationLocation = _DestinationItem;
      this.OperationType = _opType;
      this.LoadTimer.Interval = 1000;
      this.LoadTimer.Tick += LoadTimer_Tick;
      this.LoadTimer.Start();
      
      CopyThread = new Thread(new ThreadStart(CopyFiles));
      CopyThread.IsBackground = false;
      CopyThread.Start();
      CopyThread.Join(1);
      Thread.Sleep(1);
      InitializeComponent();
      this.Handle = Guid.NewGuid();
      _pipeServer.Listen("DATACH" + Handle.ToString());
      switch (_opType) {
        case OperationType.Copy:
          lblOperation.Text = "Copying ";
          break;
        case OperationType.Move:
          lblOperation.Text = "Moving ";
          break;
        case OperationType.Delete:
          lblOperation.Text = "Deleting ";
          break;
        case OperationType.Rename:
          lblOperation.Text = "Renaming ";
          break;
        case OperationType.Decomress:
          lblOperation.Text = "Decompressing ";
          break;
        case OperationType.Compress:
          lblOperation.Text = "Compressing ";
          break;
        default:
          break;
      }
      lblFrom.Text = SourceItemsCollection[0].Parent.GetDisplayName(DisplayNameType.Default);
      lblTo.Text = DestinationLocation.GetDisplayName(DisplayNameType.Default);
      
    }

    void _pipeServer_PipeFinished(object sender, EventArgs e) {
      _pipeClient = new PipeClient();
      _pipeClient.Send("PIPEDrain", "CCH" + Handle.ToString());
    }


    void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled) {
      if (!handled) {
        if (msg.message == WM_FOWINC) {

          //WindowsAPI.COPYDATASTRUCT mystr = new WindowsAPI.COPYDATASTRUCT();
          //Marshal.PtrToStructure(msg.lParam, mystr);
          MessageBox.Show("bla");
        }
      }
    }

    void LoadTimer_Tick(object sender, EventArgs e) {
      this.Visibility = System.Windows.Visibility.Visible;
      LoadTimer.Stop();
    }

    CopyFileCallbackAction CopyCallback(ShellObject src, String dst, object state, long totalFileSize, long totalBytesTransferred) {
      //Console.WriteLine("{0}\t{1}", totalFileSize, totalBytesTransferred);
      _block.WaitOne();
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() => {

                   if (totalBytesTransferred > 0) {
                     if (totalBytesTransferred - oldbyteVlaue > 0)
                       totaltransfered += (totalBytesTransferred - oldbyteVlaue);
                     oldbyteVlaue = totalBytesTransferred;
                     prFileProgress.Value = Math.Round((double)(totalBytesTransferred * 100 / totalFileSize), 0);
                     if (totalBytesTransferred == totalFileSize) {
                       procCompleted++;
                     }
                     prOverallProgress.Value = Math.Round((totaltransfered / (double)totalSize) * 100D);
                     lblProgress.Text = Math.Round((totaltransfered / (Decimal)totalSize) * 100M, 2).ToString("F2") + " % complete"; //Math.Round((prOverallProgress.Value * 100 / prOverallProgress.Maximum) + prFileProgress.Value / prOverallProgress.Maximum, 2).ToString("F2") + " % complete";
                     if (procCompleted == CopyItemsCount) {
                       CloseCurrentTask();
                     }
                   } else {
                     oldbyteVlaue = 0;
                     if (prFileProgress != null)
                       prFileProgress.Value = 0;
                   }
                 }));

      if (Cancel)
        return CopyFileCallbackAction.Cancel;

      return CopyFileCallbackAction.Continue;
    }

    ulong totalSize = 0;
    int CopyItemsCount = 0;
    void CopyFiles() {

      CurrentStatus = 1;
      _block.WaitOne();
      List<KeyValuePair<ShellObject, String>> collection = new List<KeyValuePair<ShellObject,string>>();
      List<Tuple<ShellObject, String, String, Int32>> CopyItems = new List<Tuple<ShellObject, String, String, Int32>>();
      
      string newDestination = String.Empty;
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() => {
                   this.prOverallProgress.IsIndeterminate = true;
                   lblProgress.Text = "Counting Files";
                 }));

      collection = CustomFileOperations.GetCopyDataAll(SourceItemsCollection, DestinationLocation.ParsingName);
      //for (int i = 0; i < SourceItemsCollection.Count(); i++) {
      //  CustomFileOperations.GetCopyData(SourceItemsCollection[i], DestinationLocation.ParsingName);
      //}
      if (SourceItemsCollection[0].IsFolder && SourceItemsCollection[0].Properties.System.FileExtension.Value == null) {
        if (DestinationLocation == SourceItemsCollection[0].Parent) {
          int suffix = 0;
          newDestination = System.IO.Path.Combine(DestinationLocation.ParsingName, SourceItemsCollection[0].GetDisplayName(DisplayNameType.Default));

          do {
            newDestination = String.Format("{0} - Copy ({1})", System.IO.Path.Combine(DestinationLocation.ParsingName, SourceItemsCollection[0].GetDisplayName(DisplayNameType.Default)), ++suffix);
          } while (Directory.Exists(newDestination) || File.Exists(newDestination));
        }
      }
      totalSize = 0;
      int index = 0;
      List<CollisionInfo> colissions = new List<CollisionInfo>();
      string newdestinationname = String.Empty;

      foreach (var item in collection) {
        if (DestinationLocation == item.Key.Parent) {
          int suffix = 0;
          newDestination = System.IO.Path.Combine(DestinationLocation.ParsingName, System.IO.Path.GetFileNameWithoutExtension(item.Key.GetDisplayName(DisplayNameType.Default)));

          do {
            newDestination = String.Format("{0} - Copy ({1})", System.IO.Path.Combine(DestinationLocation.ParsingName, System.IO.Path.GetFileNameWithoutExtension(item.Key.GetDisplayName(DisplayNameType.Default))), ++suffix);
          } while (Directory.Exists(newDestination) || File.Exists(newDestination + System.IO.Path.GetExtension(item.Key.ParsingName)));
        } else {

          
          if (!item.Key.IsFolder || item.Key.Properties.System.FileExtension.Value != null) {
            var oldDestinationName = String.IsNullOrEmpty(newDestination) ? item.Value : item.Value.Replace(System.IO.Path.Combine(DestinationLocation.ParsingName, System.IO.Path.GetFileNameWithoutExtension(SourceItemsCollection[0].ParsingName)), newDestination);
            newdestinationname = oldDestinationName;
            if (File.Exists(newdestinationname)) {
              colissions.Add(new CollisionInfo() { item = ShellObject.FromParsingName(item.Key.ParsingName), index = index, Correspondingitem = ShellObject.FromParsingName(newdestinationname) });
              int suffix = 0;
              newdestinationname = oldDestinationName.Remove(oldDestinationName.LastIndexOf("."));

              do {
                newdestinationname = String.Format("{0} - Copy ({1})", oldDestinationName.Remove(oldDestinationName.LastIndexOf(".")), ++suffix);
              } while (File.Exists(newdestinationname + System.IO.Path.GetExtension(item.Key.ParsingName)));


            } else {
              newdestinationname = string.Empty;
            }
          }
        }

        totalSize += item.Key.Properties.System.Size.Value.HasValue ? item.Key.Properties.System.Size.Value.Value : 0;
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() => {
                   lblProgress.Text = "Counting Files - " + WindowsAPI.StrFormatByteSize((long)totalSize);
                 }));


        CopyItems.Add(new Tuple<ShellObject, String, String, Int32>(item.Key, item.Value, String.IsNullOrEmpty(newdestinationname) ? String.IsNullOrEmpty(newDestination) ? item.Value : item.Value.Replace(System.IO.Path.Combine(DestinationLocation.ParsingName, System.IO.Path.GetFileNameWithoutExtension(item.Key.ParsingName)), newDestination) : newdestinationname + System.IO.Path.GetExtension(item.Key.ParsingName), 0));
        index++;
      }

     
      if (colissions.Count > 0) {
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() => {
                   CollisionDialog dlg = new CollisionDialog(colissions, SourceItemsCollection[0].Parent, DestinationLocation);

                   dlg.Owner = ParentContents;

                   if (dlg.ShowDialog() == true) {
                     colissions = dlg.collisions;
                   } else {
                     CopyItems.Clear();
                     colissions.Clear();
                   }
                 }));
      }
      int itemIndex = 0;
      var itemsForSkip = colissions.Where( c => (!c.IsChecked & !c.IsCheckedC)|| (!c.IsChecked & c.IsCheckedC)).Select(c => c.index).ToArray();
      foreach (var itemIndexRemove in itemsForSkip.OrderByDescending(c => c)) {
        CopyItems.RemoveAt(itemIndexRemove);
      }
      CopyItemsCount = CopyItems.Count();
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
               (Action)(() => {
                 //prOverallProgress.Maximum = CopyItems.Count();
                 lblItemsCount.Text = CopyItems.Count().ToString();
               }));
      if (CopyItems.Count == 0) {
        CloseCurrentTask();
      }
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() => {
                   this.prOverallProgress.IsIndeterminate = false;
                 }));
      int counter = 0;
      foreach (var file in CopyItems) {
        switch (this.OperationType)
	      {
		      case OperationType.Copy:
            if (!CustomFileOperations.FileOperationCopy(file, CopyFileOptions.None, CopyCallback, itemIndex, colissions)) {
              int error = Marshal.GetLastWin32Error();
              if (error == 5) {
                if (!isProcess) {
                  String CurrentexePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                  string dir = System.IO.Path.GetDirectoryName(CurrentexePath);
                  string ExePath = System.IO.Path.Combine(dir, @"FileOperation.exe");
                  
                  using (Process proc = new Process()) {
                    var psi = new ProcessStartInfo {
                      FileName = ExePath,
                      Verb = "runas",
                      UseShellExecute = true,
                      Arguments = String.Format("/env /user:Administrator \"{0}\" ID:{1}", ExePath, Handle)
                    };

                    proc.StartInfo = psi;
                    proc.Start();
                  }
                  this.IsAdminFO = true;
                  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                     (Action)(() => {
                       prFileProgress.Foreground = Brushes.Blue;
                       prOverallProgress.Foreground = Brushes.Blue;
                     }));
                  _pipeClient = new PipeClient();
                  _pipeClient.Send("OP|COPY", "CCH" + Handle.ToString());
                  
                  isProcess = true;
                }
                var currentItem = colissions.Where(c => c.item == file.Item1).SingleOrDefault();
                var destPath = "";
                if (currentItem != null) {
                if (!currentItem.IsCheckedC && currentItem.IsChecked) {
                  destPath = file.Item2;
                } else if (currentItem.IsCheckedC && currentItem.IsChecked) {
                  destPath = file.Item3;
                }
                } else {
                destPath = file.Item3;
                }
                _pipeClient = new PipeClient();
                _pipeClient.Send("INPUT|" + file.Item1.ParsingName + "|" + destPath, "CCH" + Handle.ToString());
                if (itemIndex == CopyItems.Count - 1) {
                  _pipeClient.Send("END FO INIT", "CCH" + Handle.ToString());
                }
                break;

              } else {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() => {
                   if (error == 1225)
                     CloseCurrentTask();
                   else {
                     prFileProgress.Foreground = Brushes.Red;
                     prOverallProgress.Foreground = Brushes.Red;
                   }
                 }));
              }
            };
           break;
          case OperationType.Move:
           CustomFileOperations.FileOperationMove(file, Microsoft.WindowsAPICodePack.Shell.CustomFileOperations.MoveFileFlags.MOVEFILE_COPY_ALLOWED | CustomFileOperations.MoveFileFlags.MOVEFILE_WRITE_THROUGH, CopyCallback, itemIndex, colissions);
           break;
          case OperationType.Delete:
           break;
          case OperationType.Rename:
           break;
          case OperationType.Decomress:
           break;
          case OperationType.Compress:
           break;
          default:
           break;
	      }
              
        itemIndex++; 
      }

      //If we have MoveOperation to same volume it is basically a rename opearion that happend immediately so we close the window
      if (OperationType == FileOperations.OperationType.Move) {
        if (System.IO.Path.GetPathRoot(CopyItems[0].Item1.ParsingName) == System.IO.Path.GetPathRoot(DestinationLocation.ParsingName))
          CloseCurrentTask();
      }

    }
    bool isProcess = false;
    private void btnPause_Click(object sender, RoutedEventArgs e) {
      if (CurrentStatus == 1) {
        _block.Reset();
        _pipeClient = new PipeClient();
        _pipeClient.Send("COMMAND|PAUSE", "CCH" + Handle.ToString());
        prFileProgress.Foreground = Brushes.Orange;
        prOverallProgress.Foreground = Brushes.Orange;
        CurrentStatus = 2;
      } else {
        _block.Set();
        _pipeClient = new PipeClient();
        _pipeClient.Send("COMMAND|CONTINUE", "CCH" + Handle.ToString());
        CurrentStatus = 1;
        if (this.IsAdminFO) {
          prFileProgress.Foreground = Brushes.Blue;
          prOverallProgress.Foreground = Brushes.Blue;
        } else {
          prFileProgress.Foreground = new SolidColorBrush(Color.FromRgb(0x01, 0xD3, 0x28));
          prOverallProgress.Foreground = new SolidColorBrush(Color.FromRgb(0x01, 0xD3, 0x28));
        }
      }
    }

    private void CloseCurrentTask() {
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
               (Action)(() => {
                 this.Cancel = true;
                 this.CopyThread.Abort();
                 FileOperationDialog parentWindow = (FileOperationDialog)Window.GetWindow(this);
                 if (parentWindow != null) {
                   parentWindow.Contents.Remove(this);

                   if (parentWindow.Contents.Count == 0)
                     parentWindow.Close();
                 } else {
                   ParentContents.Contents.Remove(this);
                   if (ParentContents.Contents.Count == 0)
                     ParentContents.Close();
                 }
               }));
    }
    private void btnStop_Click(object sender, RoutedEventArgs e) {
        this.Cancel = true;
        _pipeClient = new PipeClient();
        _pipeClient.Send("COMMAND|STOP", "CCH" + Handle.ToString());
    }

    long totaltransfered = 0;
    long oldbyteVlaue = 0;
    int procCompleted = 0;
    private void PipesMessageHandler(string message) {
      try {
        
        Dispatcher.Invoke(new NewMessageDelegate((PipesMessageHandler) => {
          char unicodeSeparator = (char)0x00;
          var lastChar = message.IndexOf(unicodeSeparator);
          var newMessage = message.Remove(lastChar);
          var data = newMessage.Split(Char.Parse("|"));
          var totalBytesTransferred = Convert.ToInt64(data[0]);
          var totalFileSize = Convert.ToInt64(data[1]);
          if (totalBytesTransferred > 0) {
            if (totalBytesTransferred - oldbyteVlaue > 0)
              totaltransfered += (totalBytesTransferred - oldbyteVlaue);
            oldbyteVlaue = totalBytesTransferred;
            prFileProgress.Value = Math.Round((double)(totalBytesTransferred * 100 / totalFileSize), 0);
            if (totalBytesTransferred == totalFileSize) {
              procCompleted++;
            }
            prOverallProgress.Value = Math.Round((totaltransfered / (double)totalSize) * 100D);
            lblProgress.Text = Math.Round((totaltransfered / (Decimal)totalSize) * 100M,2).ToString("F2") + " % complete"; //Math.Round((prOverallProgress.Value * 100 / prOverallProgress.Maximum) + prFileProgress.Value / prOverallProgress.Maximum, 2).ToString("F2") + " % complete";
            if (procCompleted == CopyItemsCount) {
              CloseCurrentTask();
            }
          } else {
            oldbyteVlaue = 0;
            if (prFileProgress != null)
              prFileProgress.Value = 0;
          }
        }), message);
       
      } catch (Exception ex) {

        Debug.WriteLine(ex.Message);
      }

    }


    private void UserControl_Unloaded_1(object sender, RoutedEventArgs e) {
      _pipeServer.PipeMessage -= new DelegateMessage(PipesMessageHandler);
      _pipeServer = null;
      this.Cancel = true;
      this.CopyThread.Abort();
      _pipeClient = new PipeClient();
      _pipeClient.Send("COMMAND|CLOSE", "CCH" + Handle.ToString());
    }
  }

  public enum OperationType {
    Copy,
    Move,
    Delete,
    Rename,
    Decomress,
    Compress
  }

 
}
