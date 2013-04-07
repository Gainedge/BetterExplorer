using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
using Microsoft.VisualBasic;

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations {
  /// <summary>
  /// Interaction logic for FileOperation.xaml
  /// </summary>
  public partial class FileOperation : UserControl {
    public String[] SourceItemsCollection { get; set; }
    public String DestinationLocation { get; set; }
    public FileOperationDialog ParentContents { get; set; }
    public Boolean Cancel = false;
    public OperationType OperationType { get; set; }
    private ManualResetEvent _block;
    private MessageReceiver mr;
    public Guid Handle;
    private int CurrentStatus = -1;
    private bool IsShown = false;
    private bool DeleteToRB { get; set; }
    Thread CopyThread;
    public delegate void NewMessageDelegate(string NewMessage);
    private Boolean IsAdminFO = false;
    IntPtr CorrespondingWinHandle = IntPtr.Zero;
    System.Windows.Forms.Timer LoadTimer = new System.Windows.Forms.Timer();
    private ManualResetEvent _block2;

    public FileOperation(String[] _SourceItems, String _DestinationItem, OperationType _opType = OperationType.Copy, Boolean _deleteToRB = false) {
      _block = new ManualResetEvent(false);
      _block2 = new ManualResetEvent(false);
      _block.Set();

      this.SourceItemsCollection = _SourceItems;
      this.DestinationLocation = _DestinationItem;
      this.OperationType = _opType;
      this.DeleteToRB = _deleteToRB;
      this.LoadTimer.Interval = 1000;
      this.LoadTimer.Tick += LoadTimer_Tick;
      this.LoadTimer.Start();
      InitializeComponent();
      ShellObject firstSourceItem = ShellObject.FromParsingName(SourceItemsCollection[0]);
      lblFrom.Text = firstSourceItem.Parent.GetDisplayName(DisplayNameType.Default);
      lblTo.Text = System.IO.Path.GetFileNameWithoutExtension(DestinationLocation);
      firstSourceItem.Parent.Dispose();
      firstSourceItem.Dispose();
      CopyThread = new Thread(new ThreadStart(CopyFiles));
      CopyThread.IsBackground = false;
      CopyThread.Start();
      CopyThread.Join(1);
      Thread.Sleep(1);

      this.Handle = Guid.NewGuid();

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

    }

    void LoadTimer_Tick(object sender, EventArgs e) {
      this.Visibility = System.Windows.Visibility.Visible;
      LoadTimer.Stop();
    }

    private string lastFile = String.Empty;
    CopyFileCallbackAction CopyCallback(String src, String dst, object state, long totalFileSize, long totalBytesTransferred) {
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
                         if (OperationType == OperationType.Move)
                         {
                             FileInfo fi = new FileInfo(src);
                             if (fi.IsReadOnly)
                                 fi.IsReadOnly = false;
                             fi.Delete();
                         }
                     }
                     prOverallProgress.Value = Math.Round((totaltransfered / (double)totalSize) * 100D);
                     lblProgress.Text = Math.Round((totaltransfered / (Decimal)totalSize) * 100M, 2).ToString("F2") + " % complete"; //Math.Round((prOverallProgress.Value * 100 / prOverallProgress.Maximum) + prFileProgress.Value / prOverallProgress.Maximum, 2).ToString("F2") + " % complete";
                     if (totaltransfered == (long)totalSize)
                     {
                       CloseCurrentTask();
                       if (OperationType == OperationType.Move)
                       {
                           foreach (var dir in this.SourceItemsCollection.Select(ShellObject.FromParsingName).ToArray().Where(c => c.IsFolder))
                           {
                               DeleteAllFilesFromDir(new DirectoryInfo(dir.ParsingName), false);
                               DeleteFolderRecursive(new DirectoryInfo(dir.ParsingName), false);
                           }
                           GC.WaitForPendingFinalizers();
                           GC.Collect();
                       }
                     }
                   } else {
                     oldbyteVlaue = 0;
                     if (prFileProgress != null)
                       prFileProgress.Value = 0;
                     if (totalFileSize == 0)
                       CloseCurrentTask();
                   }
                 }));

      if (Cancel)
        return CopyFileCallbackAction.Cancel;

      return CopyFileCallbackAction.Continue;
    }

    ulong totalSize = 0;
    int CopyItemsCount = 0;
    private void StartAdminProcess(List<Tuple<String, String, String, Int32>> CopyItems, List<CollisionInfo> colissions, int i, Tuple<String, String, String, Int32> file) {
      if (!isProcess) {

        String currentexePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
        string dir = System.IO.Path.GetDirectoryName(currentexePath);
        string exePath = System.IO.Path.Combine(dir, @"FileOperation.exe");

        try
        {
            using (Process proc = new Process())
            {
                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Verb = "runas",
                    UseShellExecute = true,
                    Arguments = String.Format("/env /user:Administrator \"{0}\" ID:{1}", exePath, Handle)
                };

                proc.StartInfo = psi;
                proc.Start();
            }
            this.IsAdminFO = true;

            Dispatcher.Invoke(DispatcherPriority.Background,
               (Action)(() =>
               {
                   mr = new MessageReceiver("FOMR" + this.Handle.ToString());
                   mr.OnMessageReceived += mr_OnMessageReceived;
                   mr.OnInitAdminOP += mr_OnInitAdminOP;
                   mr.OnErrorReceived += mr_OnErrorReceived;
                   prFileProgress.Foreground = Brushes.Blue;
                   prOverallProgress.Foreground = Brushes.Blue;
               }));



            isProcess = true;
            _block2.WaitOne();
            CorrespondingWinHandle = WindowsAPI.FindWindow(null, "FO" + this.Handle.ToString());
            for (int j = 0; j < CopyItems.Count; j++)
            {
                var currentItem = colissions.Where(c => c.itemPath == CopyItems[j].Item1).SingleOrDefault();
                var destPath = "";
                if (currentItem != null)
                {
                    if (!currentItem.IsCheckedC && currentItem.IsChecked)
                    {
                        destPath = CopyItems[j].Item2;
                    }
                    else if (currentItem.IsCheckedC && currentItem.IsChecked)
                    {
                        destPath = CopyItems[j].Item3;
                    }
                }
                else
                {
                    destPath = CopyItems[j].Item3;
                }
                byte[] data = System.Text.Encoding.Unicode.GetBytes(String.Format("INPUT|{0}|{1}|{2}", CopyItems[j].Item1, destPath, CopyItems[j].Item4));
                WindowsAPI.SendStringMessage(CorrespondingWinHandle, data, 0, data.Length);

                if (j == CopyItems.Count - 1)
                {
                    byte[] data2 = Encoding.Unicode.GetBytes("END FO INIT|COPY");
                    WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
                }
            }
        }
        catch (Exception)
        {
            
        }
      }
    }
    void CopyFiles() {

      CurrentStatus = 1;
      _block.WaitOne();
      List<KeyValuePair<String, String>> collection = new List<KeyValuePair<String, string>>();
      List<Tuple<String, String, String, Int32>> CopyItems = new List<Tuple<String, String, String, Int32>>();
      List<CollisionInfo> colissions = new List<CollisionInfo>();
      List<String> Directories = new List<string>();
      string newDestination = String.Empty;
      int itemIndex = 0;
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() => {
                   this.prOverallProgress.IsIndeterminate = true;
                   lblProgress.Text = "Counting Files";
                 }));
      if (this.OperationType != FileOperations.OperationType.Delete) {
          totalSize = 0;
          foreach (var item in SourceItemsCollection)
          {
              var shellobj = ShellObject.FromParsingName(item);
              if (shellobj.IsFolder) {
                var newName = String.Empty;
                if (shellobj.Parent.ParsingName == ShellObject.FromParsingName(DestinationLocation).ParsingName) {
                  var dest = System.IO.Path.Combine(DestinationLocation, System.IO.Path.GetFileName(item));
                  if (Directory.Exists(dest)) {
                    int suffix = 0;
                    newName = dest;

                    do {
                      newName = String.Format("{0} - Copy ({1})", dest , ++suffix);
                    } while (Directory.Exists(newName));
                  }
                }
                CopyFolder(item, newName == String.Empty ? DestinationLocation : newName, ref CopyItems, ref colissions, newName != String.Empty);
              } else {
                
                CopyFiles(item, DestinationLocation, ref CopyItems, ref colissions, shellobj.Parent.ParsingName == ShellObject.FromParsingName(DestinationLocation).ParsingName);
              }
              shellobj.Dispose();
          }

        if (colissions.Count > 0) {
          Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                   (Action)(() => {
                     CollisionDialog dlg = new CollisionDialog(colissions, ShellObject.FromParsingName(SourceItemsCollection[0]).Parent, ShellObject.FromParsingName(DestinationLocation));

                     dlg.Owner = ParentContents;

                     if (dlg.ShowDialog() == true) {
                       colissions = dlg.collisions;
                     } else {
                       CopyItems.Clear();
                       colissions.Clear();
                     }
                   }));
        }
        itemIndex = 0;
        var itemsForSkip = colissions.Where(c => (!c.IsChecked & !c.IsCheckedC) || (!c.IsChecked & c.IsCheckedC)).Select(c => c.index).ToArray();
        foreach (var itemIndexRemove in itemsForSkip.OrderByDescending(c => c)) {
          CopyItems.RemoveAt(itemIndexRemove);
        }
        CopyItemsCount = CopyItems.Where(c => c.Item4 == 0).Count();
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(delegate { lblItemsCount.Text = CopyItems.Count().ToString(CultureInfo.InvariantCulture); }));
        if (CopyItems.Count == 0) {
          CloseCurrentTask();
        }
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                   (Action)(() => {
                     this.prOverallProgress.IsIndeterminate = false;
                   }));
      } else {
        foreach (var item in SourceItemsCollection) {
          var itemObj = ShellObject.FromParsingName(item);
          totalSize += itemObj.Properties.System.Size.Value.HasValue ? itemObj.Properties.System.Size.Value.Value : 0;
          Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                   (Action)(() => {
                     lblProgress.Text = String.Format("Counting Files - {0}", WindowsAPI.StrFormatByteSize((long)totalSize));
                   }));
          itemObj.Dispose();
        }
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                   (Action)(() => {
                     this.prOverallProgress.IsIndeterminate = false;
                   }));
      }
      int counter = 0;
      var isBreak = false;
      if (this.OperationType != FileOperations.OperationType.Delete) {
        var collectionArray = CopyItems.ToArray().OrderByDescending(c => c.Item4);
        foreach (var file in collectionArray) {
          switch (this.OperationType) {
            case OperationType.Copy:
              if (file.Item4 == 1) {
                if (!Directory.Exists(file.Item2))
                  try {
                    Directory.CreateDirectory(file.Item2);
                  } catch (UnauthorizedAccessException) {
                    StartAdminProcess(CopyItems, colissions, itemIndex, file);
                    isBreak = true;
                    break;
                  }
              } else {
                if (!CustomFileOperations.FileOperationCopy(file, CopyFileOptions.None, CopyCallback, itemIndex, colissions)) {
                  int error = Marshal.GetLastWin32Error();
                  if (error == 5) {
                    StartAdminProcess(CopyItems, colissions, itemIndex, file);
                    isBreak = true;
                    break;
                  } else {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                     (Action)(() => {
                       if (error == 1235)
                         CloseCurrentTask();
                       else {
                         prFileProgress.Foreground = Brushes.Red;
                         prOverallProgress.Foreground = Brushes.Red;
                       }
                     }));
                  }
                }
              };
              break;
            case OperationType.Move:
                  if (file.Item4 == 0)
                  {
                      if (System.IO.Path.GetPathRoot(CopyItems[0].Item1) ==
                          System.IO.Path.GetPathRoot(DestinationLocation))
                      {
                          if (!CustomFileOperations.FileOperationMove(file, CustomFileOperations.MoveFileFlags.NONE,
                                                                      CopyCallback, itemIndex, colissions))
                          {
                              int error = Marshal.GetLastWin32Error();
                              if (error == 5)
                              {
                                  if (!isProcess)
                                  {
                                      String CurrentexePath =
                                          System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]
                                              .FullyQualifiedName;
                                      string dir = System.IO.Path.GetDirectoryName(CurrentexePath);
                                      string ExePath = System.IO.Path.Combine(dir, @"FileOperation.exe");

                                      using (Process proc = new Process())
                                      {
                                          var psi = new ProcessStartInfo
                                              {
                                                  FileName = ExePath,
                                                  Verb = "runas",
                                                  UseShellExecute = true,
                                                  Arguments =
                                                      String.Format("/env /user:Administrator \"{0}\" ID:{1}", ExePath,
                                                                    Handle)
                                              };

                                          proc.StartInfo = psi;
                                          proc.Start();
                                      }
                                      this.IsAdminFO = true;

                                      Dispatcher.Invoke(DispatcherPriority.Background,
                                                        (Action) (() =>
                                                            {
                                                                mr = new MessageReceiver("FOMR" + this.Handle.ToString());
                                                                mr.OnMessageReceived += mr_OnMessageReceived;
                                                                mr.OnInitAdminOP += mr_OnInitAdminOP;
                                                                mr.OnErrorReceived += mr_OnErrorReceived;
                                                                prFileProgress.Foreground = Brushes.Blue;
                                                                prOverallProgress.Foreground = Brushes.Blue;
                                                            }));



                                      isProcess = true;
                                      _block2.WaitOne();
                                      CorrespondingWinHandle = WindowsAPI.FindWindow(null, "FO" + this.Handle);
                                  }
                                  var currentItemM = colissions.SingleOrDefault(c => c.itemPath == file.Item1);
                                  var destPathM = "";
                                  if (currentItemM != null)
                                  {
                                      if (!currentItemM.IsCheckedC && currentItemM.IsChecked)
                                      {
                                          destPathM = file.Item2;
                                      }
                                      else if (currentItemM.IsCheckedC && currentItemM.IsChecked)
                                      {
                                          destPathM = file.Item3;
                                      }
                                  }
                                  else
                                  {
                                      destPathM = file.Item3;
                                  }
                                  byte[] dataM =
                                      Encoding.Unicode.GetBytes(String.Format("INPUT|{0}|{1}", file.Item1, destPathM));
                                  WindowsAPI.SendStringMessage(CorrespondingWinHandle, dataM, 0, dataM.Length);

                                  if (itemIndex == CopyItems.Count - 1)
                                  {
                                      byte[] data2 = Encoding.Unicode.GetBytes("END FO INIT|MOVE");
                                      WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
                                  }
                                  break;

                              }
                              else
                              {
                                  Dispatcher.Invoke(DispatcherPriority.Background,
                                                    (Action) (() =>
                                                        {
                                                            if (error == 1235)
                                                                CloseCurrentTask();
                                                            else
                                                            {
                                                                prFileProgress.Foreground = Brushes.Red;
                                                                prOverallProgress.Foreground = Brushes.Red;
                                                            }
                                                        }));
                              }
                          }
                      }
                      else
                      {
                          if (!CustomFileOperations.FileOperationCopy(file, CopyFileOptions.None, CopyCallback,
                                                                      itemIndex, colissions))
                          {
                              int error = Marshal.GetLastWin32Error();
                              if (error == 5)
                              {
                                  StartAdminProcess(CopyItems, colissions, itemIndex, file);
                                  isBreak = true;
                                  break;
                              }
                              else
                              {
                                  Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                                                    (Action) (() =>
                                                        {
                                                            if (error == 1235)
                                                                CloseCurrentTask();
                                                            else
                                                            {
                                                                prFileProgress.Foreground = Brushes.Red;
                                                                prOverallProgress.Foreground = Brushes.Red;
                                                            }
                                                        }));
                              }
                          }
                          
                      }
                  }
                  ;
              break;
            case OperationType.Rename:
              break;
            case OperationType.Decomress:
              break;
            case OperationType.Compress:
              break;
          }

          itemIndex++;
          if (isBreak)
            break;
        }
        if (!IsAdminFO)
          CloseCurrentTask();
      } else {
         Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                   (Action)(() => {
                     var count = 0;
                     if (!this.DeleteToRB) {
                       foreach (var item in this.SourceItemsCollection) {
                         var itemObj = ShellObject.FromParsingName(item);
                         if (itemObj.IsFolder) {
                           DirectoryInfo di = new DirectoryInfo(item);
                           count += di.GetDirectories("*", SearchOption.AllDirectories).Count() + di.GetFiles("*.*", SearchOption.AllDirectories).Count();
                           count++;
                         } else {
                           count++;
                         }
                       }
                     }
                     prFileProgress.Visibility = System.Windows.Visibility.Collapsed;
                     prOverallProgress.Maximum = this.DeleteToRB?this.SourceItemsCollection.Count():count;
                   }));
              var isError = false;
              foreach (var item in this.SourceItemsCollection) {
                _block.WaitOne();
                try {
                  var itemObj = ShellObject.FromParsingName(item);

                  if (!itemObj.IsFolder || (itemObj.Properties.System.FileExtension.Value != null && itemObj.Properties.System.FileExtension.Value.ToLowerInvariant() == ".zip")) {
                    var itemInfo = new FileInfo(item);
                    if (itemInfo.IsReadOnly)
                      File.SetAttributes(item, FileAttributes.Normal);
                      if (this.DeleteToRB)
                      Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(item, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                    else
                      File.Delete(item);
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                     (Action)(() => {
                       prOverallProgress.Value++;
                     }));
                  } else {
                    if (this.DeleteToRB) {
                      Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(item, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                    } else {
                      DeleteAllFilesFromDir(new DirectoryInfo(item));
                      DeleteFolderRecursive(new DirectoryInfo(item));
                    }
                  }
                  itemObj.Dispose();
                } catch (UnauthorizedAccessException){

                } catch (Exception) {
                  isError = true;
                  Dispatcher.Invoke(DispatcherPriority.Background,
                   (Action)(() => {
                     prOverallProgress.Foreground = Brushes.Red;
                   }));
                  _block.Reset();
                }
              }
              if (!isError)
                CloseCurrentTask();
      }

      //If we have MoveOperation to same volume it is basically a rename opearion that happend immediately so we close the window
      if (OperationType == OperationType.Move) {
        if (System.IO.Path.GetPathRoot(CopyItems[0].Item1) == System.IO.Path.GetPathRoot(DestinationLocation))
          CloseCurrentTask();
      }

    }

    public void CopyFiles(string sourceFile, string destFolder, ref List<Tuple<String, String, String, Int32>> CopyItems, ref List<CollisionInfo> collisions, bool isSame)
    {
        string name = System.IO.Path.GetFileName(sourceFile);
        string dest = System.IO.Path.Combine(destFolder, name);
        string newName = String.Empty;
        if (File.Exists(dest))
        {
            if (!isSame)
              collisions.Add(new CollisionInfo() { itemPath = sourceFile, index = 0, CorrespondingItemPath = dest });
            int suffix = 0;
            newName = dest.Remove(dest.LastIndexOf("."));

            do
            {
                newName = String.Format("{0} - Copy ({1})", dest.Remove(dest.LastIndexOf(".")), ++suffix);
            } while (File.Exists(newName + System.IO.Path.GetExtension(sourceFile)));
        }
        CopyItems.Add(new Tuple<string, string, string, int>(sourceFile, dest, newName == String.Empty ? dest : newName + System.IO.Path.GetExtension(sourceFile), 0));
        var shellObj = ShellObject.FromParsingName(sourceFile);
        totalSize += shellObj.Properties.System.Size.Value.HasValue ? shellObj.Properties.System.Size.Value.Value : 0;
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                 (Action)(() =>
                 {
                     lblProgress.Text = "Counting Files - " + WindowsAPI.StrFormatByteSize((long)totalSize);
                 }));
        shellObj.Dispose();
    }

    public void CopyFolder(string sourceFolder, string destFolder, ref List<Tuple<String,String,String,Int32>> CopyItems, ref List<CollisionInfo> collisions, bool isSameFolder)
    {
        var destinationDirBase = System.IO.Path.Combine(destFolder, isSameFolder?String.Empty:System.IO.Path.GetFileName(sourceFolder));
        //if (!Directory.Exists(destFolder))
        //    Directory.CreateDirectory(destFolder);

        //if (!Directory.Exists(destinationDirBase))
        //    Directory.CreateDirectory(destinationDirBase);

        string[] files = Directory.GetFiles(sourceFolder);
        foreach (string file in files)
        {
            string name = System.IO.Path.GetFileName(file);
            string dest = System.IO.Path.Combine(destinationDirBase, name);
            string newName = String.Empty;
            if (File.Exists(dest))
            {
                collisions.Add(new CollisionInfo() { itemPath = file, index = 0, CorrespondingItemPath = dest });
                int suffix = 0;
                newName = dest.Remove(dest.LastIndexOf("."));

                do
                {
                    newName = String.Format("{0} - Copy ({1})", dest.Remove(dest.LastIndexOf(".")), ++suffix);
                } while (File.Exists(newName + System.IO.Path.GetExtension(file)));
            }
            CopyItems.Add(new Tuple<string, string, string, int>(file, dest, newName == String.Empty ? dest : newName + System.IO.Path.GetExtension(file), 0));
            var shellObj = ShellObject.FromParsingName(file);
            totalSize += shellObj.Properties.System.Size.Value.HasValue ? shellObj.Properties.System.Size.Value.Value : 0;
              Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                       (Action)(() => {
                         lblProgress.Text = "Counting Files - " + WindowsAPI.StrFormatByteSize((long)totalSize);
                       }));
              shellObj.Dispose();
        }
        string[] folders = Directory.GetDirectories(sourceFolder);
        foreach (string folder in folders)
        {
            string name = System.IO.Path.GetFileName(folder);
            string dest = System.IO.Path.Combine(destinationDirBase, name);
            CopyItems.Add(new Tuple<string,string,string,int>(folder,dest,dest,1));
            CopyFolder(folder, destinationDirBase, ref CopyItems, ref collisions, false);
        }
    }




    void mr_OnErrorReceived(object sender, MessageEventArgs e) {
      Dispatcher.Invoke(DispatcherPriority.Background,
                 (Action)(() => {
                   prFileProgress.Foreground = Brushes.Red;
                   prOverallProgress.Foreground = Brushes.Red;
                 }));
    }

    void mr_OnInitAdminOP(object sender, MessageEventArgs e) {
      _block2.Set();
    }

    void mr_OnMessageReceived(object sender, MessageEventArgs e)
    {
        Dispatcher.Invoke(new Action(() =>
        {
            char unicodeSeparator = (char)0x00;
            var lastChar = e.Message.IndexOf(unicodeSeparator);
            var newMessage = e.Message;
            if (Char.IsDigit(newMessage[0]))
            {
                var data = newMessage.Split(Char.Parse("|"));
                var totalBytesTransferred = Convert.ToInt64(data[0]);
                var totalFileSize = Convert.ToInt64(data[1]);
                var totaltransfered = Convert.ToInt64(data[2]);
                if (totalBytesTransferred > 0)
                {
                    //if (totalBytesTransferred - oldbyteVlaue > 0)
                    //  totaltransfered += (totalBytesTransferred - oldbyteVlaue);
                    //oldbyteVlaue = totalBytesTransferred;
                    prFileProgress.Value = Math.Round((double)(totalBytesTransferred * 100 / totalFileSize), 0);
                    if (totalBytesTransferred == totalFileSize)
                    {
                        procCompleted++;
                    }
                    prOverallProgress.Value = Math.Round((totaltransfered / (double)totalSize) * 100D);
                    lblProgress.Text = Math.Round((totaltransfered / (decimal)totalSize) * 100M, 2).ToString("F2") + " % complete"; //Math.Round((prOverallProgress.Value * 100 / prOverallProgress.Maximum) + prFileProgress.Value / prOverallProgress.Maximum, 2).ToString("F2") + " % complete";
                    if (totaltransfered == (long)totalSize)
                    {
                        CloseCurrentTask();
                    }
                }
                else
                {
                    oldbyteVlaue = 0;
                    if (prFileProgress != null)
                        prFileProgress.Value = 0;
                }
            }
        }));
    }

    bool isProcess = false;
    private void btnPause_Click(object sender, RoutedEventArgs e) {
      if (CurrentStatus == 1) {
        _block.Reset();
        imgPause.Source = new ImageSourceConverter().ConvertFromString(@"pack://application:,,,/BetterExplorer;Component/Images/resume.png") as ImageSource;
        if (this.IsAdminFO) {
          byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|PAUSE");
          WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
        }
        prFileProgress.Foreground = Brushes.Orange;
        prOverallProgress.Foreground = Brushes.Orange;
        CurrentStatus = 2;
      } else {
        _block.Set();
        imgPause.Source = new ImageSourceConverter().ConvertFromString(@"pack://application:,,,/BetterExplorer;Component/Images/pause.png") as ImageSource;
        CurrentStatus = 1;
        if (this.IsAdminFO) {
          prFileProgress.Foreground = Brushes.Blue;
          prOverallProgress.Foreground = Brushes.Blue;
          byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|CONTINUE");
          WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
        } else {
          prFileProgress.Foreground = new SolidColorBrush(Color.FromRgb(0x01, 0xD3, 0x28));
          prOverallProgress.Foreground = new SolidColorBrush(Color.FromRgb(0x01, 0xD3, 0x28));
        }
      }
    }

    private void DeleteAllFilesFromDir(DirectoryInfo baseDir, bool isNotAfterMove = true) {
     FileInfo[] files = baseDir.GetFiles("*.*", SearchOption.AllDirectories);
     foreach (var item in files) {
       if (item.IsReadOnly)
         item.IsReadOnly = false;
       item.Delete();
         if (isNotAfterMove)
         {
             Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                               (Action) (() =>
                                   {
                                       prOverallProgress.Value++;
                                   }));
         }
     }
    }

    private void DeleteFolderRecursive(DirectoryInfo baseDir, Boolean isNotAfterMove = true) {
      baseDir.Attributes = FileAttributes.Normal;
      foreach (var childDir in baseDir.GetDirectories()) {
        DeleteFolderRecursive(childDir, isNotAfterMove);
      }
      baseDir.Delete(!isNotAfterMove);
      if (isNotAfterMove){
        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                          (Action) (() => {
                                            prOverallProgress.Value++;
                          }));
      }
    }

    private void CloseCurrentTask() {
      Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
               (Action)(() => {
                 try {
                   
                   this.Cancel = true;
                   _block.Set();
                   _block2.Set();
                   
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
                   if (mr != null)
                     mr.Close();
                   Thread.Sleep(100);
                   
                 } catch (Exception) {

                 }
                 this.CopyThread.Abort();
                 
               }));
    }
    private void btnStop_Click(object sender, RoutedEventArgs e) {
      _block.Set();
      _block2.Set();
        this.Cancel = true;
        if (this.IsAdminFO) {
          byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|STOP");
          WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
          CloseCurrentTask();
        }
    }

    long totaltransfered = 0;
    long oldbyteVlaue = 0;
    int procCompleted = 0;


    private void UserControl_Unloaded_1(object sender, RoutedEventArgs e) {
      this.Cancel = true;
      this.CopyThread.Abort();
      if (this.IsAdminFO) {
        byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|CLOSE");
        WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
      }
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
