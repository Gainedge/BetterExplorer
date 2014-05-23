using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.FileOperations;
using PipesClient;
using PipesServer;
using WindowsHelper;

namespace FileOperation {
	public partial class Form1 : Form {

		#region Properties


		private uint WM_FOWINC = WindowsAPI.RegisterWindowMessage("BE_FOWINC");
		private uint WM_FOBEGIN = WindowsAPI.RegisterWindowMessage("BE_FOBEGIN");
		private uint WM_FOEND = WindowsAPI.RegisterWindowMessage("BE_FOEND");
		private uint WM_FOPAUSE = WindowsAPI.RegisterWindowMessage("BE_FOPAUSE");
		private uint WM_FOSTOP = WindowsAPI.RegisterWindowMessage("BE_FOSTOP");
		private uint WM_FOERROR = WindowsAPI.RegisterWindowMessage("BE_FOERROR");
		private PipeClient _pipeClient;
		private PipeServer _pipeServer;
		private ManualResetEvent _block, _block2;
		private OperationType OPType { get; set; }
		private int CurrentStatus = -1;
		private bool IsShown = true;
		private Thread CopyThread;

		private long OldBytes = 0;
		private long totaltransfered;
		private const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;
		private Dictionary<String, long> oldbyteVlaues = new Dictionary<string, long>();

		public static Guid SourceHandle = Guid.Empty;
		public delegate void NewMessageDelegate(string NewMessage);
		public Boolean Cancel = false;
		public IntPtr MessageReceiverHandle;
		public List<Tuple<String, String, int>> SourceItemsCollection { get; set; }
		public string DestinationLocation { get; set; }

		#endregion

		public Form1() {
			InitializeComponent();
			try {
				WindowsHelper.WindowsAPI.CHANGEFILTERSTRUCT filterStatus = new WindowsHelper.WindowsAPI.CHANGEFILTERSTRUCT();
				filterStatus.size = (uint)Marshal.SizeOf(filterStatus);
				filterStatus.info = 0;
				WindowsAPI.ChangeWindowMessageFilterEx(Handle, 0x4A, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
				WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOWINC, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
				WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOBEGIN, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
				WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOEND, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
				WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOPAUSE, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
				WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOSTOP, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
				WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOERROR, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
			}
			catch (Exception) {
				Close();
			}
			_block = new ManualResetEvent(false);
			_block2 = new ManualResetEvent(false);

			SourceItemsCollection = new List<Tuple<string, string, int>>();
			try {
				SourceHandle = Guid.Parse(Environment.GetCommandLineArgs().Where(c => c.StartsWith("ID:")).Single().Substring(3));
			}
			catch (Exception) {

			}

			Text = String.Format("FO{0}", SourceHandle);
			MessageReceiverHandle = WindowsAPI.FindWindow(null, "FOMR" + SourceHandle);
			label1.Text = MessageReceiverHandle.ToString();
			WindowsAPI.SendMessage(MessageReceiverHandle, WM_FOWINC, IntPtr.Zero, IntPtr.Zero);
		}


		void CopyFiles() {
			CurrentStatus = 1;
			_block.WaitOne();
			foreach (var item in SourceItemsCollection.Where(c => c.Item3 == 0)) {
				OldBytes = 0;
				if (this.OPType == OperationType.Copy) {
					if (item.Item3 == 1) {
						if (!Directory.Exists(item.Item2))
							try {
								Directory.CreateDirectory(item.Item2);
							}
							catch (UnauthorizedAccessException) {
								WindowsAPI.SendMessage(MessageReceiverHandle, WM_FOERROR, IntPtr.Zero, IntPtr.Zero);
								Environment.Exit(5);
							}
					}
					else {
						try {
							ProcessItems(item.Item1, item.Item2);
						}
						catch (Exception) {
							WindowsAPI.SendMessage(MessageReceiverHandle, WM_FOERROR, IntPtr.Zero, IntPtr.Zero);
							Environment.Exit(5);
						}

					};
				}
				else if (this.OPType == OperationType.Move) {
					if (item.Item3 == 0) {

						try {
							ProcessItems(item.Item1, item.Item2);
						}
						catch (Exception) {
							WindowsAPI.SendMessage(MessageReceiverHandle, WM_FOERROR, IntPtr.Zero, IntPtr.Zero);
							Environment.Exit(5);
						}
					}

					foreach (var dir in this.SourceItemsCollection.Select(c => ShellObject.FromParsingName(c.Item1)).ToArray().Where(c => c.IsFolder)) {
						DeleteFolderRecursive(new DirectoryInfo(dir.ParsingName));
					}
					GC.WaitForPendingFinalizers();
					GC.Collect();
				}
				else if (this.OPType == OperationType.Delete) {
					foreach (var entry in this.SourceItemsCollection) {
						_block.WaitOne();
						try {

							if (!Directory.Exists(entry.Item1) || (Path.GetExtension(entry.Item1).ToLowerInvariant() == ".zip")) {
								var itemInfo = new FileInfo(entry.Item1);
								if (itemInfo.IsReadOnly)
									File.SetAttributes(entry.Item1, FileAttributes.Normal);
								if (this.DeleteToRB)
									Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(entry.Item1, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
								else
									File.Delete(entry.Item1);

								byte[] data = System.Text.Encoding.Unicode.GetBytes(String.Format("{0}|{1}|{2}|{3}", 1, 0, totaltransfered, entry.Item1));
								WindowsAPI.SendStringMessage(MessageReceiverHandle, data, 0, data.Length);
							}
							else {
								if (this.DeleteToRB) {
									RecycleBin.SendSilent(entry.Item1);
								}
								else {
									DeleteAllFilesFromDir(new DirectoryInfo(entry.Item1));
									DeleteFolderRecursive(new DirectoryInfo(entry.Item1));
								}
							}
						}
						catch (Exception) {
							WindowsAPI.SendMessage(MessageReceiverHandle, WM_FOERROR, IntPtr.Zero, IntPtr.Zero);
							Environment.Exit(5);
						}
					}
				}
			}
		}


		public void ProcessItems(string src, string dst) {
			int size = 2048 * 1024 * 2;	//buffer size
			int current_read_buffer = 0; //pointer to current read buffer
			int last_bytes_read = 0; //number of bytes last read

			if (!Directory.Exists(System.IO.Path.GetDirectoryName(dst)))
				Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dst));

			byte[][] buffer = new byte[2][];
			buffer[0] = new byte[size];
			buffer[1] = new byte[size];

			//Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
			//                (Action)(() =>
			//                {
			//                    lblFileName.Text = System.IO.Path.GetFileNameWithoutExtension(src);
			//                }));

			using (var r = new System.IO.FileStream(src, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, size * 2, System.IO.FileOptions.SequentialScan | System.IO.FileOptions.Asynchronous)) {
				//Microsoft.Win32.SafeHandles.SafeFileHandle hDst = CreateFile(dst, (uint)System.IO.FileAccess.Write, (uint)System.IO.FileShare.None, IntPtr.Zero, (uint)System.IO.FileMode.Create, FILE_FLAG_NO_BUFFERING | FILE_FLAG_SEQUENTIAL_SCAN | FILE_FLAG_OVERLAPPED, IntPtr.Zero);
				var z = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.None, size * 2,
											  FileOptions.WriteThrough | FileFlagNoBuffering | FileOptions.SequentialScan);
				z.Close();
				z.Dispose();
				using (var w = new System.IO.FileStream(dst, FileMode.Open, System.IO.FileAccess.Write, FileShare.ReadWrite, size * 2, true)) {
					current_read_buffer = 0;
					last_bytes_read = r.Read(buffer[current_read_buffer], 0, size); //synchronously read the first buffer
					long l = r.Length;
					//w.SetLength(l);
					long i = 0;
					while (i < l) {
						_block.WaitOne();
						if (Cancel) {
							Environment.Exit(5);
							break;
						}
						IAsyncResult aw = w.BeginWrite(buffer[current_read_buffer], 0, last_bytes_read, new AsyncCallback(CopyFileCallback), 0);
						current_read_buffer = current_read_buffer == 0 ? 1 : 0;
						Thread.CurrentThread.Join(2);
						IAsyncResult ar = r.BeginRead(buffer[current_read_buffer], 0, last_bytes_read, new AsyncCallback(CopyFileCallback), 0);
						i += last_bytes_read;

						if (i > 0) {
							long oldvalbefore = 0;
							oldbyteVlaues.TryGetValue(src, out oldvalbefore);


							long oldval = 0;
							if (oldbyteVlaues.TryGetValue(src, out oldval))
								oldbyteVlaues[src] = i;
							else
								oldbyteVlaues.Add(src, i);

							if (i - oldvalbefore > 0)
								totaltransfered += (i - oldvalbefore);

							byte[] data = System.Text.Encoding.Unicode.GetBytes(String.Format("{0}|{1}|{2}|{3}", i, l, totaltransfered, src));
							WindowsAPI.SendStringMessage(MessageReceiverHandle, data, 0, data.Length);
							if (i == l) {
								//procCompleted++;
								if (this.OPType == OperationType.Move) {
									r.Close();
									r.Dispose();
									FileInfo fi = new FileInfo(src);
									if (fi.IsReadOnly)
										fi.IsReadOnly = false;
									fi.Delete();
								}
							}

							//if (totaltransfered == total)
							//{

							//    if (this.OPType == OperationType.Move)
							//    {
							//        foreach (var dir in this.SourceItemsCollection.Select(c =>  ShellObject.FromParsingName(c.Item1)).ToArray().Where(c => c.IsFolder))
							//        {
							//            DeleteAllFilesFromDir(new DirectoryInfo(dir.ParsingName), false);
							//            DeleteFolderRecursive(new DirectoryInfo(dir.ParsingName), false);
							//        }
							//        GC.WaitForPendingFinalizers();
							//        GC.Collect();
							//    }
							//    Environment.Exit(5);

							//}
						}
						else {
							//oldbyteVlaue = 0;
							oldbyteVlaues[src] = 0;
							if (l == 0)
								Environment.Exit(5);
						}

						last_bytes_read = r.EndRead(ar);
						Thread.Sleep(1);
						w.EndWrite(aw);
					}
				}
			}
		}

		public void CopyFileCallback(IAsyncResult ar) {
		}

		private void DeleteAllFilesFromDir(DirectoryInfo baseDir, bool isNotAfterMove = true) {
			FileInfo[] files = baseDir.GetFiles("*.*", SearchOption.AllDirectories);
			foreach (var item in files) {
				_block.WaitOne();
				if (Cancel) {
					Close();
					break;
				}
				if (item.IsReadOnly)
					item.IsReadOnly = false;
				item.Delete();
			}
		}
		private void DeleteFolderRecursive(DirectoryInfo baseDir, Boolean isNotAfterMove = true) {
			baseDir.Attributes = FileAttributes.Normal;
			foreach (var childDir in baseDir.GetDirectories()) {
				_block.WaitOne();
				if (Cancel) {
					Close();
					break;
				}
				DeleteFolderRecursive(childDir, isNotAfterMove);
			}

			baseDir.Delete(!isNotAfterMove);
			if (isNotAfterMove) {

			}
		}

		bool DeleteToRB = false;
		protected override void WndProc(ref Message m) {

			if (m.Msg == WindowsAPI.WM_COPYDATA) {
				byte[] b = new Byte[Marshal.ReadInt32(m.LParam, IntPtr.Size)];
				IntPtr dataPtr = Marshal.ReadIntPtr(m.LParam, IntPtr.Size * 2);
				Marshal.Copy(dataPtr, b, 0, b.Length);
				string newMessage = System.Text.Encoding.Unicode.GetString(b);
				if (newMessage.StartsWith("END FO INIT|COPY")) {
					this.OPType = OperationType.Copy;
				}
				if (newMessage.StartsWith("END FO INIT|MOVE")) {
					this.OPType = OperationType.Move;
				}
				if (newMessage.StartsWith("END FO INIT|DELETE")) {
					this.OPType = OperationType.Delete;
				}
				if (newMessage.Contains("DeleteTORB"))
					this.DeleteToRB = true;

				if (newMessage.StartsWith("INPUT|")) {
					var parts = newMessage.Replace("INPUT|", "").Split(Char.Parse("|"));
					SourceItemsCollection.Add(new Tuple<string, string, int>(parts[0].Trim(), parts[1].Trim(), Convert.ToInt32(parts[2].Trim())));

				}
				if (newMessage.StartsWith("END FO INIT")) {
					_block.Set();

					CopyThread = new Thread(new ThreadStart(CopyFiles));
					CopyThread.IsBackground = false;
					CopyThread.Start();
					CopyThread.Join(1);
				}
				if (newMessage.StartsWith("COMMAND|")) {
					var realMessage = newMessage.Replace("COMMAND|", String.Empty);
					switch (realMessage) {
						case "STOP":
							this.Cancel = true;
							_block.Set();
							_block2.Set();
							break;
						case "PAUSE":
							_block.Reset();
							break;
						case "CONTINUE":
							_block.Set();
							break;
						case "CLOSE":
							Close();
							break;
						default:
							break;
					}
				}
			}
			base.WndProc(ref m);
		}
	}

}
