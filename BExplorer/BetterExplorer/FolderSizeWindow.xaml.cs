using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using WPFPieChart;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Shell;
using WindowsHelper;
using System.Runtime.InteropServices;
using System.Globalization;
using Fluent;
using System.Windows.Interop;
using System.Threading.Tasks;

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for FolderSizeWindow.xaml
	/// </summary>
	public partial class FolderSizeWindow : Window {
		#region Margins

		[DllImport("DwmApi.dll")]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);


		public struct MARGINS {
			public MARGINS(Thickness t) {
				Left = (int)t.Left;
				Right = (int)t.Right;
				Top = (int)t.Top;
				Bottom = (int)t.Bottom;
			}
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		#endregion

		//public FolderSizeWindow()
		//{
		//		InitializeComponent();
		//		LoadFolderSizeItems("C:\\");
		//}

		private FolderSizeWindow() { }


		//public FolderSizeWindow(string dir, Window owner = null) {
		//	InitializeComponent();
		//	LoadFolderSizeItems(dir);
		//	this.Owner = owner;
		//}

		public static void Open(string dir, Window owner) {
			var f = new FolderSizeWindow();
			f.InitializeComponent();
			f.LoadFolderSizeItems(dir);
			f.Owner = owner;
			f.Show();
		}

		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			// This can't be done any earlier than the SourceInitialized event:
			ExtendGlassFrame(this, new Thickness(-1));
		}

		[DllImport("dwmapi.dll", PreserveSig = false)]
		static extern bool DwmIsCompositionEnabled();
		public static bool ExtendGlassFrame(Window window, Thickness margin) {
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

		private ObservableCollection<FolderSizeInfoClass> FInfo;
		public BackgroundWorker bgw;
		public void LoadFolderSizeItems(string dir) {

			//List<KeyValuePair<string, long>> valueList = new List<KeyValuePair<string, long>>();
			//DirectoryInfo data = new DirectoryInfo(dir);
			////valueList.Add(new KeyValuePair<string, long>("Current Directory", GetFolderSize(dir, false)));
			//foreach (DirectoryInfo item in data.GetDirectories())
			//{
			//    valueList.Add(new KeyValuePair<string, long>(item.Name, GetFolderSize(item.FullName, true)));
			//}
			//chart1.DataContext = valueList;
			pieChartLayout1.legend1.Head.Text = ShellObject.FromParsingName(dir).GetDisplayName(DisplayNameType.Default);
			bgw = new BackgroundWorker();
			bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
			bgw.WorkerReportsProgress = true;
			bgw.WorkerSupportsCancellation = true;
			bgw.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
			bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
			bgw.RunWorkerAsync(dir);


		}

		void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			try {
				progressBar1.Value = e.ProgressPercentage;
				FInfo = new ObservableCollection<FolderSizeInfoClass>(FSI.Where(w => w.FSize > 0).OrderBy(o => o.FSize));
				this.DataContext = FInfo;
			}
			catch {

			}
		}

		List<FolderSizeInfoClass> FSI = new List<FolderSizeInfoClass>();
		void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			progressBar1.Value = progressBar1.Maximum;
			progressBar1.Visibility = System.Windows.Visibility.Collapsed;
			textBlock1.Visibility = System.Windows.Visibility.Collapsed;
			textBlock1wfx.Visibility = System.Windows.Visibility.Collapsed;
			FInfo = new ObservableCollection<FolderSizeInfoClass>(FSI.Where(w => w.FSize > 0).OrderBy(o => o.FSize));
			this.DataContext = FInfo;
		}

		public const int MAX_PATH = 260;
		public const int MAX_ALTERNATE = 14;

		[StructLayout(LayoutKind.Sequential)]
		public struct FILETIME {
			public uint dwLowDateTime;
			public uint dwHighDateTime;
		};

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WIN32_FIND_DATA {
			public FileAttributes dwFileAttributes;
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

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32", CharSet = CharSet.Unicode)]
		public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

		private long RecurseDirectory(string directory, int level, out int files, out int folders) {
			IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
			long size = 0;
			files = 0;
			folders = 0;
			WIN32_FIND_DATA findData;

			IntPtr findHandle;

			// please note that the following line won't work if you try this on a network folder, like \\Machine\C$
			// simply remove the \\?\ part in this case or use \\?\UNC\ prefix
			findHandle = FindFirstFile(String.Format(@"\\?\{0}\*", directory), out findData);
			if (findHandle != INVALID_HANDLE_VALUE) {

				do {
					if (bgw.CancellationPending)
						break;

					if ((findData.dwFileAttributes & FileAttributes.Directory) != 0) {

						if (findData.cFileName != "." && findData.cFileName != "..") {
							folders++;

							int subfiles, subfolders;
							string subdirectory = directory + (directory.EndsWith(@"\") ? "" : @"\") +
									findData.cFileName;
							if (level != 0)  // allows -1 to do complete search.
														{
								size += RecurseDirectory(subdirectory, level - 1, out subfiles, out subfolders);

								folders += subfolders;
								files += subfiles;
							}
						}
					}
					else {
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
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool FindClose(IntPtr hFindFile);

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt) {
			try {
				var dirFiles = Enumerable.Empty<string>();
				if (searchOpt == SearchOption.AllDirectories) {
					dirFiles = Directory.EnumerateDirectories(path)
															.SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
				}
				return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
			}
			catch (UnauthorizedAccessException) {
				return Enumerable.Empty<string>();
			}
		}

		void bgw_DoWork(object sender, DoWorkEventArgs e) {
			//FSI = FolderSizeInfoClass.ConstructData(e.Argument.ToString());

			//List<FolderSizeInfoClass> FolderInfoSize = new List<FolderSizeInfoClass>();
			DirectoryInfo data = new DirectoryInfo(e.Argument.ToString());
			int i = 0;
			DirectoryInfo[] diri = data.GetDirectories();
			//GetAllDirRec(ShellObject.FromParsingName(data.FullName));

			Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
													(Action)(() => {
														progressBar1.Maximum = diri.Count();
													}));

			Parallel.ForEach(diri, (item, state) => {

				if ((sender as BackgroundWorker).CancellationPending) {
					state.Break();
				}
				FolderSizeInfoClass fsi = new FolderSizeInfoClass();
				fsi.FolderSizeLoc = item.Name;
				shol.Clear();
				long retsize = 0;
				//try
				//{
				//    GetFilesRec(ShellObject.FromParsingName(item.FullName));

				//    foreach (ShellObject item2 in shol)
				//    {
				//        if ((sender as BackgroundWorker).CancellationPending)
				//        {
				//            break;
				//        }
				//        object oo = item2.Properties.System.Size.ValueAsObject;
				//        retsize += Convert.ToInt64(oo);
				//        GC.Collect();
				//    }
				//    //ShellObject o = ShellObject.FromParsingName(dir);
				//}
				//catch (Exception)
				//{


				//}
				int ii = 0;
				int iff = 0;

				try {
					retsize = RecurseDirectory(item.FullName, -1, out ii, out iff);
				}
				catch (Exception) {

				}
				fsi.FSize = retsize;
				FSI.Add(fsi);

				(sender as BackgroundWorker).ReportProgress(i++);



			});
			shol.Clear();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				WindowsAPI.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
			}
		}


		private long GetFolderSize(string dir, bool includesubdirs) {
			shol.Clear();
			long retsize = 0;
			try {
				GetFilesRec(ShellObject.FromParsingName(dir));
				foreach (ShellObject item in shol) {
					object oo = item.Properties.System.Size.ValueAsObject;
					retsize += Convert.ToInt64(oo);
				}
				//ShellObject o = ShellObject.FromParsingName(dir);
			}
			catch (Exception) {


			}

			return retsize;
		}
		int AllDirsCount = 0;
		private void GetAllDirRec(ShellObject path) {
			AllDirsCount = 0;
			ShellContainer con = (ShellContainer)ShellContainer.FromParsingName(path.ParsingName);
			foreach (ShellObject item in con) {
				try {
					if (item.IsFolder) {
						try {
							AllDirsCount++;
							GetFilesRec(item);
						}
						catch (Exception) {

						}
					}
				}
				catch (Exception) {

				}
			}
			con.Dispose();
		}
		List<ShellObject> shol = new List<ShellObject>();

		private void GetFilesRec(ShellObject path) {

			ShellContainer con = (ShellContainer)ShellContainer.FromParsingName(path.ParsingName);
			foreach (ShellObject item in con) {
				try {
					if (item.IsFolder) {
						try {
							GetFilesRec(item);
						}
						catch (Exception) {

						}
					}
					else {
						shol.Add(item);
					}
				}
				catch (Exception) {

				}
			}
			con.Dispose();

		}

		private void chartitself_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			MessageBox.Show(e.AddedItems[0].GetType().ToString());
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			if (bgw != null) {
				bgw.CancelAsync();
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				WindowsAPI.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
			}
		}

		private void button1_Click(object sender, RoutedEventArgs e) {

		}

	}

}
