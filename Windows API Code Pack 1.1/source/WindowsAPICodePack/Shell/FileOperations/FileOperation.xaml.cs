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
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations {
  /// <summary>
  /// Interaction logic for FileOperation.xaml
  /// </summary>
    public partial class FileOperation : UserControl
    {
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
        DateTime OperationStartTime = DateTime.Now;

        public FileOperation(String[] _SourceItems, String _DestinationItem, OperationType _opType = OperationType.Copy, Boolean _deleteToRB = false)
        {
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
            //CopyThread.Join(1);
            //CopyFiles();
            Thread.Sleep(1);

            this.Handle = Guid.NewGuid();

            switch (_opType)
            {
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

        void LoadTimer_Tick(object sender, EventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Visible;
            LoadTimer.Stop();
        }

        private string lastFile = String.Empty;

        long totaltransfered = 0;
        long oldbyteVlaue = 0;
        int procCompleted = 0;
        Dictionary<String, long> oldbyteVlaues = new Dictionary<string, long>();
        DateTime LastMeasuredTime = DateTime.Now;
        long lastTotalTransfered = 0;
        CopyFileCallbackAction CopyCallback(String src, String dst, object state, long totalFileSize, long totalBytesTransferred)
        {
            //Console.WriteLine("{0}\t{1}", totalFileSize, totalBytesTransferred);
            _block.WaitOne();
            Thread.Sleep(1);
            if (totalBytesTransferred > 0)
            {
                long oldvalbefore = 0;
                oldbyteVlaues.TryGetValue(src, out oldvalbefore);


                long oldval = 0;
                if (oldbyteVlaues.TryGetValue(src, out oldval))
                    oldbyteVlaues[src] = totalBytesTransferred;
                else
                    oldbyteVlaues.Add(src, totalBytesTransferred);

                if (totalBytesTransferred - oldvalbefore > 0)
                    totaltransfered += (totalBytesTransferred - oldvalbefore);

                //prFileProgress.Value = Math.Round((double)(totalBytesTransferred * 100 / totalFileSize), 0);
                if (totalBytesTransferred == totalFileSize)
                {
                    procCompleted++;
                    if (OperationType == OperationType.Move)
                    {
                        FileInfo fi = new FileInfo(src);
                        if (fi.IsReadOnly)
                            fi.IsReadOnly = false;
                        fi.Delete();
                    }
                }
                
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    prOverallProgress.Value = Math.Round((totaltransfered / (double)totalSize) * 100D);
                }));
                var dt = DateTime.Now;
                if (dt.Subtract(LastMeasuredTime).Seconds >= 2)
                {
                    var diff = totaltransfered - lastTotalTransfered;
                    var speed = diff / 2;
                    var speedInMB = WindowsAPI.StrFormatByteSize(speed);
                    LastMeasuredTime = dt;
                    lastTotalTransfered = totaltransfered;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                    (Action)(() =>
                    {
                        lblSpeed.Text = speedInMB + "/s";
                    }));
                }

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    lblTime.Text = new DateTime(DateTime.Now.Subtract(OperationStartTime).Ticks).ToString("HH:mm:ss");
                }));
                
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    lblProgress.Text = Math.Round((totaltransfered / (Decimal)totalSize) * 100M, 2).ToString("F2") + " % complete"; //Math.Round((prOverallProgress.Value * 100 / prOverallProgress.Maximum) + prFileProgress.Value / prOverallProgress.Maximum, 2).ToString("F2") + " % complete";
                }));
                if (Math.Round((totaltransfered / (Decimal)totalSize) * 100M, 2) == 100)
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
            }
            else
            {
                oldbyteVlaue = 0;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    if (prFileProgress != null)
                        prFileProgress.Value = 0;
                }));
                if (totalFileSize == 0)
                    CloseCurrentTask();
            }

            if (Cancel)
                return CopyFileCallbackAction.Cancel;

            return CopyFileCallbackAction.Continue;
        }

        ulong totalSize = 0;
        int CopyItemsCount = 0;
        private void StartAdminProcess(List<Tuple<String, String, String, Int32>> CopyItems, List<CollisionInfo> colissions, bool isMove)
        {
            if (IsNeedAdminFO)
            {

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
                            byte[] data2 = Encoding.Unicode.GetBytes(isMove ? "END FO INIT|MOVE" : "END FO INIT|COPY");
                            WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }
        Boolean IsNeedAdminFO = false;
        void CopyFiles()
        {

            CurrentStatus = 1;
            _block.WaitOne();
            List<KeyValuePair<String, String>> collection = new List<KeyValuePair<String, string>>();
            List<Tuple<String, String, String, Int32>> CopyItems = new List<Tuple<String, String, String, Int32>>();
            List<CollisionInfo> colissions = new List<CollisionInfo>();
            List<String> Directories = new List<string>();
            string newDestination = String.Empty;
            int itemIndex = 0;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                       (Action)(() =>
                       {
                           this.prOverallProgress.IsIndeterminate = true;
                           lblProgress.Text = "Counting Files";
                       }));
            if (this.OperationType != FileOperations.OperationType.Delete)
            {
                totalSize = 0;
                foreach (var item in SourceItemsCollection)
                {
                    var shellobj = ShellObject.FromParsingName(item);
                    if (shellobj.IsFolder && shellobj.Properties.System.FileExtension.Value == null)
                    {
                        var newName = String.Empty;
                        if (shellobj.Parent.ParsingName == ShellObject.FromParsingName(DestinationLocation).ParsingName)
                        {
                            var dest = System.IO.Path.Combine(DestinationLocation, System.IO.Path.GetFileName(item));
                            if (Directory.Exists(dest))
                            {
                                int suffix = 0;
                                newName = dest;

                                do
                                {
                                    newName = String.Format("{0} - Copy ({1})", dest, ++suffix);
                                } while (Directory.Exists(newName));
                            }
                        }
                        CopyFolder(item, newName == String.Empty ? DestinationLocation : newName, ref CopyItems, ref colissions, newName != String.Empty);
                    }
                    else
                    {

                        CopyFiles(item, DestinationLocation, ref CopyItems, ref colissions, shellobj.Parent.ParsingName == ShellObject.FromParsingName(DestinationLocation).ParsingName);
                    }
                    shellobj.Dispose();
                }

                if (colissions.Count > 0)
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                             (Action)(() =>
                             {
                                 CollisionDialog dlg = new CollisionDialog(colissions, ShellObject.FromParsingName(SourceItemsCollection[0]).Parent, ShellObject.FromParsingName(DestinationLocation));

                                 dlg.Owner = ParentContents;

                                 if (dlg.ShowDialog() == true)
                                 {
                                     colissions = dlg.collisions;
                                 }
                                 else
                                 {
                                     CopyItems.Clear();
                                     colissions.Clear();
                                 }
                             }));
                }
                itemIndex = 0;
                var itemsForSkip = colissions.Where(c => (!c.IsChecked & !c.IsCheckedC) || (!c.IsChecked & c.IsCheckedC)).Select(c => c.index).ToArray();
                foreach (var itemIndexRemove in itemsForSkip.OrderByDescending(c => c))
                {
                    CopyItems.RemoveAt(itemIndexRemove);
                }
                CopyItemsCount = CopyItems.Where(c => c.Item4 == 0).Count();
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                         (Action)(delegate { lblItemsCount.Text = CopyItems.Count().ToString(CultureInfo.InvariantCulture); }));
                if (CopyItems.Count == 0)
                {
                    CloseCurrentTask();
                }
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                           (Action)(() =>
                           {
                               this.prOverallProgress.IsIndeterminate = false;
                           }));
            }
            else
            {
                foreach (var item in SourceItemsCollection)
                {
                    var itemObj = ShellObject.FromParsingName(item);
                    totalSize += itemObj.Properties.System.Size.Value.HasValue ? itemObj.Properties.System.Size.Value.Value : 0;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                             (Action)(() =>
                             {
                                 lblProgress.Text = String.Format("Counting Files - {0}", WindowsAPI.StrFormatByteSize((long)totalSize));
                             }));
                    itemObj.Dispose();
                }
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                           (Action)(() =>
                           {
                               this.prOverallProgress.IsIndeterminate = false;
                           }));
            }
            int counter = 0;
            var isBreak = false;
            if (this.OperationType != FileOperations.OperationType.Delete)
            {
                var collectionArray = CopyItems.ToArray().OrderByDescending(c => c.Item4);
                //Parallel.ForEach(collectionArray, file =>
                foreach (var file in collectionArray)
                {
                    if (this.Cancel)
                        break;
                    switch (this.OperationType)
                    {
                        case OperationType.Copy:
                            if (file.Item4 == 1)
                            {
                                if (!Directory.Exists(file.Item2))
                                    try
                                    {
                                        Directory.CreateDirectory(file.Item2);
                                    }
                                    catch (UnauthorizedAccessException)
                                    {
                                        StartAdminProcess(CopyItems, colissions, false);
                                        isBreak = true;
                                        break;
                                    }
                            }
                            else
                            {
                                //if (!CustomFileOperations.FileOperationCopy(file, CopyFileOptions.None, CopyCallback, itemIndex, colissions))
                                var currentItem = colissions.Where(c => c.itemPath == file.Item1).SingleOrDefault();
                                try
                                {
                                    if (currentItem != null)
                                    {
                                        if (!currentItem.IsCheckedC && currentItem.IsChecked)
                                        {
                                            CopyFileS(file.Item1, file.Item2);
                                        }
                                        else if (currentItem.IsCheckedC && currentItem.IsChecked)
                                        {
                                            CopyFileS(file.Item1, file.Item3);
                                        }
                                    }
                                    else
                                    {
                                        CopyFileS(file.Item1, file.Item3);
                                    }
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    IsNeedAdminFO = true;
                                    isBreak = true;
                                    break;
                                }
                                catch (Exception)
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                                         (Action)(() =>
                                         {
                                             prFileProgress.Foreground = Brushes.Red;
                                             prOverallProgress.Foreground = Brushes.Red;
                                         }));
                                }

                                //if (false)
                                //{
                                //    int error = Marshal.GetLastWin32Error();
                                //    if (error == 5)
                                //    {

                                //        //StartAdminProcess(CopyItems, colissions, itemIndex, file, false);
                                //        IsNeedAdminFO = true;
                                //        isBreak = true;
                                //        break;
                                //    }
                                //    else
                                //    {
                                //        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                                //         (Action)(() =>
                                //         {
                                //             if (error == 1235)
                                //                 CloseCurrentTask();
                                //             else
                                //             {
                                //                 prFileProgress.Foreground = Brushes.Red;
                                //                 prOverallProgress.Foreground = Brushes.Red;
                                //             }
                                //         }));
                                //    }
                                //}
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
                                            StartAdminProcess(CopyItems, colissions, true);
                                            isBreak = true;
                                            break;

                                        }
                                        else
                                        {
                                            Dispatcher.Invoke(DispatcherPriority.Background,
                                                              (Action)(() =>
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
                                            StartAdminProcess(CopyItems, colissions, true);
                                            isBreak = true;
                                            break;
                                        }
                                        else
                                        {
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                                                              (Action)(() =>
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
                }//);
                if (IsNeedAdminFO)
                    StartAdminProcess(CopyItems, colissions, OperationType == FileOperations.OperationType.Move);
            }
            else
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                          (Action)(() =>
                          {
                              var count = 0;
                              if (!this.DeleteToRB)
                              {
                                  foreach (var item in this.SourceItemsCollection)
                                  {
                                      var itemObj = ShellObject.FromParsingName(item);
                                      if (itemObj.IsFolder && itemObj.Properties.System.FileExtension.Value == null)
                                      {
                                          DirectoryInfo di = new DirectoryInfo(item);
                                          count += di.GetDirectories("*", SearchOption.AllDirectories).Count() + di.GetFiles("*.*", SearchOption.AllDirectories).Count();
                                          count++;
                                      }
                                      else
                                      {
                                          count++;
                                      }
                                  }
                              }
                              prFileProgress.Visibility = System.Windows.Visibility.Collapsed;
                              prOverallProgress.Maximum = this.DeleteToRB ? this.SourceItemsCollection.Count() : count;
                          }));
                var isError = false;
                foreach (var item in this.SourceItemsCollection)
                {
                    _block.WaitOne();
                    try
                    {
                        var itemObj = ShellObject.FromParsingName(item);

                        if (!itemObj.IsFolder || (itemObj.Properties.System.FileExtension.Value != null && itemObj.Properties.System.FileExtension.Value.ToLowerInvariant() == ".zip"))
                        {
                            var itemInfo = new FileInfo(item);
                            if (itemInfo.IsReadOnly)
                                File.SetAttributes(item, FileAttributes.Normal);
                            if (this.DeleteToRB)
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(item, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                            else
                                File.Delete(item);
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                             (Action)(() =>
                             {
                                 prOverallProgress.Value++;
                             }));
                        }
                        else
                        {
                            if (this.DeleteToRB)
                            {
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(item, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
                            }
                            else
                            {
                                DeleteAllFilesFromDir(new DirectoryInfo(item));
                                DeleteFolderRecursive(new DirectoryInfo(item));
                            }
                        }
                        itemObj.Dispose();
                    }
                    catch (UnauthorizedAccessException)
                    {

                    }
                    catch (Exception)
                    {
                        isError = true;
                        Dispatcher.Invoke(DispatcherPriority.Background,
                         (Action)(() =>
                         {
                             prOverallProgress.Foreground = Brushes.Red;
                         }));
                        _block.Reset();
                    }
                }
                if (!isError)
                    CloseCurrentTask();
            }

            //If we have MoveOperation to same volume it is basically a rename opearion that happend immediately so we close the window
            if (OperationType == OperationType.Move)
            {
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

        public void CopyFolder(string sourceFolder, string destFolder, ref List<Tuple<String, String, String, Int32>> CopyItems, ref List<CollisionInfo> collisions, bool isSameFolder)
        {
            var destinationDirBase = System.IO.Path.Combine(destFolder, isSameFolder ? String.Empty : System.IO.Path.GetFileName(sourceFolder));

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
                         (Action)(() =>
                         {
                             lblProgress.Text = "Counting Files - " + WindowsAPI.StrFormatByteSize((long)totalSize);
                         }));
                shellObj.Dispose();
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = System.IO.Path.GetFileName(folder);
                string dest = System.IO.Path.Combine(destinationDirBase, name);
                CopyItems.Add(new Tuple<string, string, string, int>(folder, dest, dest, 1));
                CopyFolder(folder, destinationDirBase, ref CopyItems, ref collisions, false);
            }
        }




        void mr_OnErrorReceived(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Background,
                       (Action)(() =>
                       {
                           prFileProgress.Foreground = Brushes.Red;
                           prOverallProgress.Foreground = Brushes.Red;
                       }));
        }

        void mr_OnInitAdminOP(object sender, MessageEventArgs e)
        {
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
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentStatus == 1)
            {
                _block.Reset();
                imgPause.Source = new ImageSourceConverter().ConvertFromString(@"pack://application:,,,/BetterExplorer;Component/Images/resume.png") as ImageSource;
                if (this.IsAdminFO)
                {
                    byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|PAUSE");
                    WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
                }
                prFileProgress.Foreground = Brushes.Orange;
                prOverallProgress.Foreground = Brushes.Orange;
                CurrentStatus = 2;
            }
            else
            {
                _block.Set();
                imgPause.Source = new ImageSourceConverter().ConvertFromString(@"pack://application:,,,/BetterExplorer;Component/Images/pause.png") as ImageSource;
                CurrentStatus = 1;
                if (this.IsAdminFO)
                {
                    prFileProgress.Foreground = Brushes.Blue;
                    prOverallProgress.Foreground = Brushes.Blue;
                    byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|CONTINUE");
                    WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
                }
                else
                {
                    prFileProgress.Foreground = new SolidColorBrush(Color.FromRgb(0x01, 0xD3, 0x28));
                    prOverallProgress.Foreground = new SolidColorBrush(Color.FromRgb(0x01, 0xD3, 0x28));
                }
            }
        }

        private void DeleteAllFilesFromDir(DirectoryInfo baseDir, bool isNotAfterMove = true)
        {
            FileInfo[] files = baseDir.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (var item in files)
            {
                if (item.IsReadOnly)
                    item.IsReadOnly = false;
                item.Delete();
                if (isNotAfterMove)
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                                      (Action)(() =>
                                          {
                                              prOverallProgress.Value++;
                                          }));
                }
            }
        }

        private void DeleteFolderRecursive(DirectoryInfo baseDir, Boolean isNotAfterMove = true)
        {
            baseDir.Attributes = FileAttributes.Normal;
            foreach (var childDir in baseDir.GetDirectories())
            {
                DeleteFolderRecursive(childDir, isNotAfterMove);
            }
            baseDir.Delete(!isNotAfterMove);
            if (isNotAfterMove)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                                  (Action)(() =>
                                  {
                                      prOverallProgress.Value++;
                                  }));
            }
        }

        private void CloseCurrentTask()
        {
            if (this.IsAdminFO)
            {
                byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|CLOSE");
                WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
            }
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                     (Action)(() =>
                     {
                         try
                         {


                             _block.Set();
                             _block2.Set();
                             //this.Cancel = true;
                             FileOperationDialog parentWindow = (FileOperationDialog)Window.GetWindow(this);
                             if (parentWindow != null)
                             {
                                 parentWindow.Contents.Remove(this);

                                 if (parentWindow.Contents.Count == 0)
                                     parentWindow.Close();
                             }
                             else
                             {
                                 ParentContents.Contents.Remove(this);
                                 if (ParentContents.Contents.Count == 0)
                                     ParentContents.Close();
                             }
                             if (mr != null)
                                 mr.Close();
                             Thread.Sleep(100);

                         }
                         catch (Exception)
                         {

                         }
                         this.CopyThread.Abort();

                     }));

        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _block.Set();
            _block2.Set();
            this.Cancel = true;
            if (this.IsAdminFO)
            {
                byte[] data2 = System.Text.Encoding.Unicode.GetBytes("COMMAND|STOP");
                WindowsAPI.SendStringMessage(CorrespondingWinHandle, data2, 0, data2.Length);
                CloseCurrentTask();
            }
        }

        byte[][] buffer = new byte[2][];
     
        public void CopyFileS(string src, string dst)
        {
            int size = 2048 * 1024 * 2;	//buffer size
            int current_read_buffer = 0; //pointer to current read buffer
            int last_bytes_read = 0; //number of bytes last read


            buffer[0] = new byte[size];
            buffer[1] = new byte[size];

            using (var r = new System.IO.FileStream(src, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, size * 2, System.IO.FileOptions.SequentialScan | System.IO.FileOptions.Asynchronous))
            {
                //Microsoft.Win32.SafeHandles.SafeFileHandle hDst = CreateFile(dst, (uint)System.IO.FileAccess.Write, (uint)System.IO.FileShare.None, IntPtr.Zero, (uint)System.IO.FileMode.Create, FILE_FLAG_NO_BUFFERING | FILE_FLAG_SEQUENTIAL_SCAN | FILE_FLAG_OVERLAPPED, IntPtr.Zero);
                var z = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.None, size * 2,
                                              FileOptions.WriteThrough | FileFlagNoBuffering | FileOptions.SequentialScan);
                z.Close();
                z.Dispose();
                using (var w = new System.IO.FileStream(dst, FileMode.Open, System.IO.FileAccess.Write, FileShare.ReadWrite, size * 2, true))
                {
                    current_read_buffer = 0;
                    last_bytes_read = r.Read(buffer[current_read_buffer], 0, size); //synchronously read the first buffer
                    long l = r.Length;
                    //w.SetLength(l);
                    long i = 0;
                    while (i < l)
                    {
                        _block.WaitOne();
                        if (Cancel)
                        {
                            CloseCurrentTask();
                            break;
                        }
                        IAsyncResult aw = w.BeginWrite(buffer[current_read_buffer], 0, last_bytes_read, new AsyncCallback(CopyFileCallback), 0);
                        current_read_buffer = current_read_buffer == 0 ? 1 : 0;
                        Thread.CurrentThread.Join(2);
                        IAsyncResult ar = r.BeginRead(buffer[current_read_buffer], 0, last_bytes_read, new AsyncCallback(CopyFileCallback), 0);
                        i += last_bytes_read;

                        if (i > 0)
                        {
                            long oldvalbefore = 0;
                            oldbyteVlaues.TryGetValue(src, out oldvalbefore);


                            long oldval = 0;
                            if (oldbyteVlaues.TryGetValue(src, out oldval))
                                oldbyteVlaues[src] = i;
                            else
                                oldbyteVlaues.Add(src, i);

                            if (i - oldvalbefore > 0)
                                totaltransfered += (i - oldvalbefore);


                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                            (Action)(() =>
                            {
                                prFileProgress.Value = Math.Round((double)(i * 100 / l), 0);
                            }));
                            if (i == l)
                            {
                                procCompleted++;
                                if (OperationType == OperationType.Move)
                                {
                                    FileInfo fi = new FileInfo(src);
                                    if (fi.IsReadOnly)
                                        fi.IsReadOnly = false;
                                    fi.Delete();
                                }
                            }
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                            (Action)(() =>
                            {
                                prOverallProgress.Value = Math.Round((totaltransfered / (double)totalSize) * 100D);
                            }));
                            var dt = DateTime.Now;
                            var secs = dt.Subtract(LastMeasuredTime).Seconds; 
                            if (secs >= 2)
                            {
                                var diff = totaltransfered - lastTotalTransfered;
                                var speed = diff / secs;
                                var speedInMB = WindowsAPI.StrFormatByteSize(speed);
                                LastMeasuredTime = DateTime.Now;
                                lastTotalTransfered = totaltransfered;
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                                (Action)(() =>
                                {
                                    lblSpeed.Text = speedInMB + "/s";
                                }));
                            }
                            var et = DateTime.Now.Subtract(OperationStartTime);
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                            (Action)(() =>
                            {
                                lblTime.Text = new DateTime(et.Ticks).ToString("HH:mm:ss");
                                //lblTimeLeft.Text = new DateTime(TimeSpan.FromMilliseconds((double)et.Milliseconds * ((long)totalSize - totaltransfered) / totaltransfered).Ticks).ToString("HH:mm:ss");
                                try
                                {
                                    lblTimeLeft.Text = new DateTime((TimeSpan.FromSeconds((int)(et.TotalSeconds / prOverallProgress.Value * (prOverallProgress.Maximum - prOverallProgress.Value)))).Ticks).ToString("HH:mm:ss");
                                }
                                catch (Exception)
                                {
                                    
                                }
                            }));

                            
                
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                            (Action)(() =>
                            {
                                lblProgress.Text = Math.Round((totaltransfered / (Decimal)totalSize) * 100M, 2).ToString("F2") + " % complete"; //Math.Round((prOverallProgress.Value * 100 / prOverallProgress.Maximum) + prFileProgress.Value / prOverallProgress.Maximum, 2).ToString("F2") + " % complete";
                            }));
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
                        }
                        else
                        {
                            oldbyteVlaue = 0;
                            oldbyteVlaues[src] = 0;
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                            (Action)(() =>
                            {
                                if (prFileProgress != null)
                                    prFileProgress.Value = 0;
                            }));
                            if (l == 0)
                                CloseCurrentTask();
                        }

                        last_bytes_read = r.EndRead(ar);
                        Thread.Sleep(1);
                        w.EndWrite(aw);
                    }
                }
            }
        }
        const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;
        public void CopyFileCallback(IAsyncResult ar)
        {
        }

        #region Test
        //file names
        private string _inputfile;
        private string _outputfile;
        //syncronization object
        private readonly object Locker1 = new object();
        //buffer size
        public int CopyBufferSize = 0;
        private long _infilesize = 0;
        //buffer read
        public byte[] Buffer1;
        private int _bytesRead1;
        //buffer overlap
        public byte[] Buffer2;
        private bool _buffer2Dirty;
        private int _bytesRead2;
        //buffer write
        public byte[] Buffer3;
        //total bytes read
        private long _totalbytesread;
        private long _totalbyteswritten;
        //filestreams
        private FileStream _infile;
        private FileStream _outfile;
        //secret sauce for unbuffered IO

        private void AsyncReadFile()
        {
            //open input file
            _infile = new FileStream(_inputfile, FileMode.Open, FileAccess.Read, FileShare.None, CopyBufferSize,
            FileFlagNoBuffering);
            //if we have data read it
            while (_totalbytesread < _infilesize)
            {
                _bytesRead1 = _infile.Read(Buffer1, 0, CopyBufferSize);
                lock (Locker1)
                {
                    while (_buffer2Dirty) Monitor.Wait(Locker1);
                    Buffer.BlockCopy(Buffer1, 0, Buffer2, 0, _bytesRead1);
                    _buffer2Dirty = true;
                    Monitor.PulseAll(Locker1);
                    _bytesRead2 = _bytesRead1;
                    _totalbytesread = _totalbytesread + _bytesRead1;
                }
            }
            //clean up open handle
            _infile.Close();
            _infile.Dispose();
        }

        private void UpdateUI()
        {
            if (_totalbyteswritten > 0)
            {
                long oldvalbefore = 0;
                oldbyteVlaues.TryGetValue(_inputfile, out oldvalbefore);


                long oldval = 0;
                if (oldbyteVlaues.TryGetValue(_inputfile, out oldval))
                    oldbyteVlaues[_inputfile] = _totalbyteswritten;
                else
                    oldbyteVlaues.Add(_inputfile, _totalbyteswritten);

                if (_totalbyteswritten - oldvalbefore > 0)
                    totaltransfered += (_totalbyteswritten - oldvalbefore);


                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    prFileProgress.Value = Math.Round((double)(_totalbyteswritten * 100 / _infilesize), 0);
                }));
                if (_totalbyteswritten == _infilesize)
                {
                    procCompleted++;
                    if (OperationType == OperationType.Move)
                    {
                        FileInfo fi = new FileInfo(_inputfile);
                        if (fi.IsReadOnly)
                            fi.IsReadOnly = false;
                        fi.Delete();
                    }
                }
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    prOverallProgress.Value = Math.Round((totaltransfered / (double)totalSize) * 100D);
                }));
                var dt = DateTime.Now;
                if (dt.Subtract(LastMeasuredTime).Seconds >= 2)
                {
                    var diff = totaltransfered - lastTotalTransfered;
                    var speed = diff / 2;
                    var speedInMB = WindowsAPI.StrFormatByteSize(speed);
                    LastMeasuredTime = dt;
                    lastTotalTransfered = totaltransfered;
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                    (Action)(() =>
                    {
                        lblSpeed.Text = speedInMB + "/s";
                    }));
                }
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    lblProgress.Text = Math.Round((totaltransfered / (Decimal)totalSize) * 100M, 2).ToString("F2") + " % complete"; //Math.Round((prOverallProgress.Value * 100 / prOverallProgress.Maximum) + prFileProgress.Value / prOverallProgress.Maximum, 2).ToString("F2") + " % complete";
                }));
                if (Math.Round((totaltransfered / (Decimal)totalSize) * 100M, 2) == 100)
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
            }
            else
            {
                oldbyteVlaue = 0;
                oldbyteVlaues[_inputfile] = 0;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                (Action)(() =>
                {
                    if (prFileProgress != null)
                        prFileProgress.Value = 0;
                }));
                if (_infilesize == 0)
                    CloseCurrentTask();
            }
        }

        private void AsyncWriteFile()
        {
            //open output file set length to prevent growth and file fragmentation and close it.
            //We have to do it this way so we can do unbuffered writes to it later
            _outfile = new FileStream(_outputfile, FileMode.Create, FileAccess.Write, FileShare.None, CopyBufferSize,
            FileOptions.WriteThrough);
            _outfile.SetLength(_infilesize);
            _outfile.Close();
            _outfile.Dispose();
            //_bytesRead2 = 0;
            //open file for write unbuffered
            _outfile = new FileStream(_outputfile, FileMode.Open, FileAccess.Write, FileShare.None, CopyBufferSize,
            FileOptions.WriteThrough | FileFlagNoBuffering);
            while (_totalbyteswritten < _infilesize - CopyBufferSize)
            {
                lock (Locker1)
                {
                    while (!_buffer2Dirty) Monitor.Wait(Locker1);

                    Buffer.BlockCopy(Buffer2, 0, Buffer3, 0, CopyBufferSize);
                    _buffer2Dirty = false;
                    Monitor.PulseAll(Locker1);

                }
                _outfile.Write(Buffer3, 0, CopyBufferSize);
                _totalbyteswritten = _totalbyteswritten + CopyBufferSize;
                UpdateUI();
            }
            //close the file handle that was using unbuffered and write through
            _outfile.Close();
            _outfile.Dispose();
            lock (Locker1)
            {
                while (!_buffer2Dirty) Monitor.Wait(Locker1);
                //open file for write buffered We do this so we can write the tail of the file
                //it is a cludge but hey you get what you get in C#
                _outfile = new FileStream(_outputfile, FileMode.Open, FileAccess.Write, FileShare.None, CopyBufferSize,
                FileOptions.WriteThrough);
                //this should always be true but I haven't run all the edge cases yet
                if (_buffer2Dirty)
                {
                    //go to the right position in the file
                    if (_infilesize - _bytesRead2 >= 0)
                    {
                        _outfile.Seek(_infilesize - _bytesRead2, 0);
                        //flush the last buffer syncronus and buffered.
                        _outfile.Write(Buffer2, 0, _bytesRead2);
                    }
                }
            }
            UpdateUI();
            //close the file handle that was using unbuffered and write through
            _outfile.Close();
            _outfile.Dispose();
        }

        public int CopyFileAsync(string inputfile, string outputfile, int buffersize = 16)
        {
            //set file name globals
            _inputfile = inputfile;
            _outputfile = outputfile;
            //setup single buffer size, remember this will be x3.
            CopyBufferSize = buffersize * 1024 * 1024;
            //buffer read
            Buffer1 = new byte[CopyBufferSize];
            //buffer overlap
            Buffer2 = new byte[CopyBufferSize];
            //buffer write
            Buffer3 = new byte[CopyBufferSize];
            //get input file size for later use
            var f = new FileInfo(_inputfile);
            long s1 = f.Length;
            _infilesize = s1;

            //create read thread and start it.
            var readfile = new Thread(AsyncReadFile) { Name = "ReadThread", IsBackground = false };
            readfile.Start();

            //create write thread and start it.
            var writefile = new Thread(AsyncWriteFile) { Name = "WriteThread", IsBackground = false };
            writefile.Start();

            //wait for threads to finish
            readfile.Join();
            writefile.Join();
            return 1;
        } 
        #endregion

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
