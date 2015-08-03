using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
//using Microsoft.WindowsAPICodePack.Shell;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
//using WPFPieChart;

namespace BetterExplorer
{

    /// <summary>
    /// Interaction logic for FolderSizeWindow.xaml
    /// </summary>
    public partial class FolderSizeWindow : Window
    {

        #region FolderSizeInfoClass

        private class FolderSizeInfoClass : INotifyPropertyChanged
        {

            #region Properties

            private String myFolderSizeLoc;
            public String FolderSizeLoc
            {
                get { return myFolderSizeLoc; }
                set
                {
                    myFolderSizeLoc = value;
                    RaisePropertyChangeEvent("FolderSizeLoc");
                }
            }

            private double fSize;
            public double FSize
            {
                get { return fSize; }
                set
                {
                    fSize = value;
                    RaisePropertyChangeEvent("FSize");
                }
            }

            #endregion

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            private void RaisePropertyChangeEvent(String propertyName)
            {
                if (PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion INotifyPropertyChanged Members
        }

        #endregion

        #region Structures

        #region Margins

        [DllImport("DwmApi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        private struct MARGINS
        {
            public int Left, Right, Top, Bottom;

            public MARGINS(Thickness t)
            {
                Left = (int)t.Left;
                Right = (int)t.Right;
                Top = (int)t.Top;
                Bottom = (int)t.Bottom;
            }
        }

        #endregion Margins

        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public System.IO.FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh; //changed all to uint, otherwise you run into unexpected overflow
            public uint nFileSizeLow;  //|
            public uint dwReserved0;   //|
            public uint dwReserved1;   //v

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
            public string cAlternate;
        }

        #endregion Structures

        #region DLLImports

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        #endregion DLLImports

        #region Properties

        public const int MAX_PATH = 260;
        public const int MAX_ALTERNATE = 14;

        public BackgroundWorker bgw;

        private ObservableCollection<FolderSizeInfoClass> FInfo;
        private List<FolderSizeInfoClass> FSI = new List<FolderSizeInfoClass>();
        private List<ShellItem> shol = new List<ShellItem>();

        //private int AllDirsCount = 0;

        #endregion Properties

        #region Constructors

        private FolderSizeWindow()
        {
        }

        public static void Open(string dir, Window owner)
        {
            var f = new FolderSizeWindow();
            f.InitializeComponent();
            f.LoadFolderSizeItems(dir);
            f.Owner = owner;
            f.Show();
        }

        #endregion Constructors


        private static bool ExtendGlassFrame(Window window, Thickness margin)
        {
            if (!DwmIsCompositionEnabled())
                return false;

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException("The Window must be shown before extending glass.");

            // Set the background to transparent from both the WPF and Win32 perspectives
            window.Background = Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;

            MARGINS margins = new MARGINS(margin);
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
            return true;
        }




        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // This can't be done any earlier than the SourceInitialized event:
            ExtendGlassFrame(this, new Thickness(-1));
        }

        public void LoadFolderSizeItems(string dir)
        {
            //List<KeyValuePair<string, long>> valueList = new List<KeyValuePair<string, long>>();
            //DirectoryInfo data = new DirectoryInfo(dir);
            ////valueList.Add(new KeyValuePair<string, long>("Current Directory", GetFolderSize(dir, false)));
            //foreach (DirectoryInfo item in data.GetDirectories())
            //{
            //    valueList.Add(new KeyValuePair<string, long>(item.Name, GetFolderSize(item.FullName, true)));
            //}
            //chart1.DataContext = valueList;
            pieChartLayout1.legend1.Head.Text = new ShellItem(dir).DisplayName;
            bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.WorkerReportsProgress = true;
            bgw.WorkerSupportsCancellation = true;
            bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            bgw.RunWorkerAsync(dir);
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                progressBar1.Value = e.ProgressPercentage;
                FInfo = new ObservableCollection<FolderSizeInfoClass>(FSI.Where(w => w.FSize > 0).OrderBy(o => o.FSize));
                this.DataContext = FInfo;
            }
            catch
            {
            }
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = progressBar1.Maximum;
            progressBar1.Visibility = System.Windows.Visibility.Collapsed;
            textBlock1.Visibility = System.Windows.Visibility.Collapsed;
            textBlock1wfx.Visibility = System.Windows.Visibility.Collapsed;
            FInfo = new ObservableCollection<FolderSizeInfoClass>(FSI.Where(w => w.FSize > 0).OrderBy(o => o.FSize));
            this.DataContext = FInfo;
        }

        private long RecurseDirectory(string directory, int level, out int files, out int folders)
        {
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            long size = 0;
            files = 0;
            folders = 0;
            WIN32_FIND_DATA findData;

            //IntPtr findHandle;

            // please note that the following line won't work if you try this on a network folder, like \\Machine\C$
            // simply remove the \\?\ part in this case or use \\?\UNC\ prefix
            //findHandle = FindFirstFile(String.Format(@"\\?\{0}\*", directory), out findData);
            IntPtr findHandle = FindFirstFile($@"\\?\{directory}\*", out findData);
            if (findHandle != INVALID_HANDLE_VALUE)
            {
                do
                {
                    if (bgw.CancellationPending)
                        break;

                    if ((findData.dwFileAttributes & System.IO.FileAttributes.Directory) != 0)
                    {
                        if (findData.cFileName != "." && findData.cFileName != "..")
                        {
                            folders++;

                            int subfiles, subfolders;
                            string subdirectory = directory + (directory.EndsWith(@"\") ? "" : @"\") +
                                    findData.cFileName;
                            if (level != 0)
                            { // allows -1 to do complete search.
                                size += RecurseDirectory(subdirectory, level - 1, out subfiles, out subfolders);

                                folders += subfolders;
                                files += subfiles;
                            }
                        }
                    }
                    else
                    {
                        // File
                        files++;

                        size += (long)findData.nFileSizeLow + (long)findData.nFileSizeHigh * 4294967296;
                    }
                }
                while (FindNextFile(findHandle, out findData));
                FindClose(findHandle);
            }

            return size;
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path).SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            //FSI = FolderSizeInfoClass.ConstructData(e.Argument.ToString());

            //List<FolderSizeInfoClass> FolderInfoSize = new List<FolderSizeInfoClass>();
            DirectoryInfo data = new DirectoryInfo(e.Argument.ToString());
            int i = 0;
            DirectoryInfo[] diri = data.GetDirectories();
            //GetAllDirRec(ShellObject.FromParsingName(data.FullName));

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                             (Action)(() =>
                             {
                                 progressBar1.Maximum = diri.Count();
                             }));

            Parallel.ForEach(diri, (item, state) =>
            {
                if ((sender as BackgroundWorker).CancellationPending)
                {
                    state.Break();
                }
                FolderSizeInfoClass fsi = new FolderSizeInfoClass();
                fsi.FolderSizeLoc = item.Name;
                shol.Clear();
                long retsize = 0;
                int ii = 0;
                int iff = 0;

                try
                {
                    retsize = RecurseDirectory(item.FullName, -1, out ii, out iff);
                }
                catch (Exception)
                {
                }
                fsi.FSize = retsize;
                FSI.Add(fsi);

                (sender as BackgroundWorker).ReportProgress(i++);
            });
            shol.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                BExplorer.Shell.Interop.Shell32.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        private void GetFilesRec(ShellItem path)
        {
            foreach (ShellItem item in path)
            {
                try
                {
                    if (item.IsFolder)
                    {
                        try
                        {
                            GetFilesRec(item);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        shol.Add(item);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            bgw?.CancelAsync();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                BExplorer.Shell.Interop.Shell32.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
    }
}