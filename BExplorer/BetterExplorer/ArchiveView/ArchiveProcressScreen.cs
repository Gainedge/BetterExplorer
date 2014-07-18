using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using SevenZip;

namespace BetterExplorer {

	public partial class ArchiveProcressScreen : Form {
		private string tempPath;
		private readonly IList<string> _fileAndDirectoryFullPaths;
		private readonly string _archivePath;
		private readonly string _archivename;
		private readonly ArchiveAction _action;
		private readonly OutArchiveFormat _format;
		private readonly bool _fastcompression;
		private readonly string _password;
		private readonly CompressionLevel _compresstionlevel;
		private SevenZipCompressor _sevenZipCompressor;
		private string _commpressingFileName;
		private SevenZipExtractor _sevenZipExtractor;
		private readonly int _deltatotaal;
		private SafeThread _thread;
		private const string LIBRARY_PATH = "7z.dll";

		public delegate void ExtractionCompleteEventHandler(object sender, ArchiveEventArgs e);

		public delegate void CompressionCompleteEventHandler(object sender, ArchiveEventArgs e);

		public event ExtractionCompleteEventHandler ExtractionCompleted;

		public event CompressionCompleteEventHandler CompressionCompleted;

		/// <summary>
		///
		/// </summary>
		/// <param name="fileAndDirectoryFullPaths">The full path to all the files and directorys to compress or archives to extract</param>
		/// <param name="archivefullPath">The path where to place the archive or the extracted files and directorys</param>
		/// <param name="format">The compression format(only for compression)</param>
		/// <param name="fastcompression">If you whan to compresss the files fast(only for compression)</param>
		/// <param name="password">(only for compression and if required)</param>
		/// <param name="compresstionlevel">How strong must the compression be (only for compression)</param>
		public ArchiveProcressScreen(IList<String> fileAndDirectoryFullPaths, string archivefullPath, ArchiveAction action, string archivename = null, OutArchiveFormat format = OutArchiveFormat.SevenZip, bool fastcompression = false, string password = null, CompressionLevel compresstionlevel = CompressionLevel.Normal) {
			SevenZipBase.SetLibraryPath(IntPtr.Size == 8 ? "7z64.dll" : "7z32.dll");
			InitializeComponent();
			_fileAndDirectoryFullPaths = fileAndDirectoryFullPaths;
			_archivePath = archivefullPath;
			_archivename = archivename;
			_action = action;
			_format = format;
			_compresstionlevel = compresstionlevel;
			_password = password;
			_fastcompression = fastcompression;
			_deltatotaal = pb_totaalfiles.Value += (int)(100 / fileAndDirectoryFullPaths.Count);
			tempPath = string.Format("{0}//7zip//", Path.GetTempPath());

			pb_compression.Value = 0;
			pb_totaalfiles.Value = 0;
		}

		private void RemoveFile() {
			try {
				_sevenZipExtractor = new SevenZipExtractor(_archivePath);
				_sevenZipCompressor = new SevenZipCompressor(tempPath);
				_sevenZipCompressor.DirectoryStructure = true;
				_sevenZipCompressor.IncludeEmptyDirectories = true;
				_sevenZipCompressor.PreserveDirectoryRoot = true;

				_sevenZipCompressor.Compressing += Compressing;
				_sevenZipCompressor.FileCompressionStarted += FileCompressionStarted;
				_sevenZipCompressor.CompressionFinished += CompressionFinished;
				_sevenZipCompressor.FileCompressionFinished += FileCompressionFinished;

				var fileIndexDictionary = new Dictionary<int, string>();
				foreach (ArchiveFileInfo archiveFileInfo in _sevenZipExtractor.ArchiveFileData) {
					if (!archiveFileInfo.IsDirectory && _fileAndDirectoryFullPaths.Contains(archiveFileInfo.FileName)) {
						fileIndexDictionary.Add(archiveFileInfo.Index, null);
					}
				}

				_sevenZipCompressor.ModifyArchive(_archivePath, fileIndexDictionary);
				Done();
			}
			catch (Exception ex) {
				var dialog = new TaskDialog();
				dialog.StandardButtons = TaskDialogStandardButtons.Ok;
				dialog.Text = ex.Message;
				dialog.Show();
				Invoke(new InvokeNone(Dispose));
			}
		}

		private void Extract() {
			//setup settings.
			foreach (var fullPath in _fileAndDirectoryFullPaths) {
				_sevenZipExtractor = new SevenZipExtractor(fullPath);
				_sevenZipExtractor.PreserveDirectoryStructure = true;

				_sevenZipExtractor.Extracting += Compressing;
				_sevenZipExtractor.FileExtractionStarted += FileExtractionStarted;
				_sevenZipExtractor.ExtractionFinished += CompressionFinished;
				_sevenZipExtractor.FileExtractionFinished += FileExtractionFinished;

				//Extract files
				_sevenZipExtractor.ExtractArchive(_archivePath);
			}

			Done();
		}

		private delegate void FileInfoEvent(object sender, FileInfoEventArgs e);

		private void FileExtractionFinished(object sender, FileInfoEventArgs e) {
			//pb_totaalfiles.Invoke(new FileInfoEvent((object o, FileInfoEventArgs l) =>
			//{
			//    pb_totaalfiles.Value += (int)(100 / _sevenZipExtractor.FilesCount);
			//}),sender,e);

			pb_compression.Invoke(new InvokeEvent((object o, EventArgs l) => {
				pb_compression.Value = 100;
			}), sender, e);
		}

		private void FileExtractionStarted(object sender, FileInfoEventArgs e) {
			//when the next file is being compressed.
			lbl_commpressing_file.Invoke(new FileInfoEvent((object o, FileInfoEventArgs l) => {
				lbl_commpressing_file.Text = String.Format(_commpressingFileName, l.FileInfo.FileName);
			}), sender, e);

			pb_totaalfiles.Invoke(new FileInfoEvent((object o, FileInfoEventArgs l) => {
				pb_totaalfiles.Value = l.PercentDone;
			}), sender, e);

			pb_compression.Invoke(new InvokeEvent((object o, EventArgs l) => {
				pb_compression.Value = 0;
			}), sender, e);
		}

		private long bytesoffiles = 0;

		private void Compress() {
			//setup settings.
			try {
				_sevenZipCompressor = new SevenZipCompressor(tempPath);
				_sevenZipCompressor.ArchiveFormat = _format;
				_sevenZipCompressor.CompressionMethod = CompressionMethod.Default;
				_sevenZipCompressor.DirectoryStructure = true;
				_sevenZipCompressor.IncludeEmptyDirectories = true;
				_sevenZipCompressor.FastCompression = _fastcompression;
				_sevenZipCompressor.PreserveDirectoryRoot = false;
				_sevenZipCompressor.CompressionLevel = _compresstionlevel;

				_sevenZipCompressor.Compressing += Compressing;
				_sevenZipCompressor.FileCompressionStarted += FileCompressionStarted;
				_sevenZipCompressor.CompressionFinished += CompressionFinished;
				_sevenZipCompressor.FileCompressionFinished += FileCompressionFinished;

				try {
					if (_password != null) {
						for (int i = 0; i < _fileAndDirectoryFullPaths.Count; i++) {
							if (!Directory.Exists(_fileAndDirectoryFullPaths[i]))
								continue;

							//Compress directories
							var strings = _fileAndDirectoryFullPaths[i].Split('/');
							if (_fileAndDirectoryFullPaths.Count == 1) {
								_sevenZipCompressor.CompressDirectory(_fileAndDirectoryFullPaths[i],
																	  String.Format("{0}/{1}.{2}",
																					_archivePath,
																					_archivename,
																					SelectExtention(_format)));
							}
							else {
								_sevenZipCompressor.CompressDirectory(_fileAndDirectoryFullPaths[i],
																	  String.Format("{0}/{1}_{3}.{2}",
																					_archivePath,
																					_archivename,
																					SelectExtention(_format),
																					_fileAndDirectoryFullPaths[i].Split('\\')
																							.Last()),
																	  _password);
							}

							//remove the directorys from the list so they will not be compressed as files as wel
							_fileAndDirectoryFullPaths.Remove(_fileAndDirectoryFullPaths[i]);
						}
						//compress files
						if (_fileAndDirectoryFullPaths.Count > 0) {
							_sevenZipCompressor.FileCompressionFinished += FileCompressionFinished;
							_sevenZipCompressor.CompressFilesEncrypted(
																	   String.Format("{0}/{1}.{2}",
																					 _archivePath,
																					 _archivename,
																					 SelectExtention(_format)),
																	   _password,
																	   _fileAndDirectoryFullPaths.ToArray());
						}
					}
					else {
						for (int i = 0; i < _fileAndDirectoryFullPaths.Count; i++)
						//var fullPath in _fileAndDirectoryFullPaths)
						{
							FileInfo fi = new FileInfo(_fileAndDirectoryFullPaths[i]);
							bytesoffiles += fi.Length;

							if (!Directory.Exists(_fileAndDirectoryFullPaths[i]))
								continue;

							//Compress directorys
							var strings = _fileAndDirectoryFullPaths[i].Split('/');
							if (_fileAndDirectoryFullPaths.Count == 1) {
								_sevenZipCompressor.CompressDirectory(_fileAndDirectoryFullPaths[i],
																	  String.Format("{0}/{1}.{2}",
																					_archivePath,
																					_archivename,
																					SelectExtention(_format)));
							}
							else {
								_sevenZipCompressor.CompressDirectory(_fileAndDirectoryFullPaths[i],
																	  String.Format("{0}/{1}_{3}.{2}",
																					_archivePath,
																					_archivename,
																					SelectExtention(_format),
																					_fileAndDirectoryFullPaths[i].Split('\\')
																							.Last()));
							}

							//reset compression bar
							//FileCompressionFinished(null, null);

							//remove the directorys from the list so they will not be compressed as files as wel
							_fileAndDirectoryFullPaths.Remove(_fileAndDirectoryFullPaths[i]);
						}
						//compress files.
						if (_fileAndDirectoryFullPaths.Count > 0) {
							_sevenZipCompressor.FileCompressionFinished += FileCompressionFinished;
							_sevenZipCompressor.CompressFiles(
															  String.Format("{0}/{1}.{2}",
																			_archivePath,
																			_archivename,
																			SelectExtention(_format)),
															  _fileAndDirectoryFullPaths.ToArray());
						}
					}

					pb_totaalfiles.Invoke(new InvokeNone(() => { pb_totaalfiles.Value = 100; }));
					Done();
				}
				catch (ThreadInterruptedException) {
					Dispose(true);
				}
			}
			catch (Exception ex) {
				var dialog = new TaskDialog();
				dialog.StandardButtons = TaskDialogStandardButtons.Ok;
				dialog.Text = ex.Message;
				dialog.Show();
			}
		}

		public delegate void InvokeNone();

		private void Done() {
			btn_cancel.Invoke(new InvokeNone(() => {
				btn_cancel.Click -= CancelCompression;
				btn_cancel.Click += Close;
				btn_cancel.Text = "Close";

				if (cb_closewhendone.Checked)
					Dispose();
			}));
		}

		private string SelectExtention(OutArchiveFormat format) {
			switch (format) {
				case OutArchiveFormat.Zip:
					return OutArchiveFormat.Zip.ToString().ToLower();

				case OutArchiveFormat.XZ:
					return OutArchiveFormat.XZ.ToString().ToLower();

				case OutArchiveFormat.Tar:
					return OutArchiveFormat.Tar.ToString().ToLower();

				case OutArchiveFormat.SevenZip:
					return "7z";

				case OutArchiveFormat.GZip:
					return "gz";

				case OutArchiveFormat.BZip2:
					return "bz2";
			}

			return OutArchiveFormat.Zip.ToString().ToLower();
		}

		public delegate void InvokeEvent(object sender, EventArgs e);

		private void FileCompressionFinished(object sender, EventArgs e) {
			pb_compression.Invoke(new InvokeEvent((object o, EventArgs l) => { pb_compression.Value = 100; }), sender, e);
		}

		private void CompressionFinished(object sender, EventArgs e) {
			bool isExtract = (_action == ArchiveAction.Extract);
			pb_compression.Invoke(new InvokeEvent((object o, EventArgs l) => { pb_compression.Value = 100; }), sender, e);
			lbl_commpressing_file.Invoke(new InvokeEvent((object o, EventArgs el) => {
				lbl_commpressing_file.Text = isExtract ? "Extracting Complete!" : "Compressing Complete!";
			}), sender, e);
			if (isExtract == true) {
				Done();
				ExtractionCompleted(sender, new ArchiveEventArgs(_archivePath));
			}
			else {
				CompressionCompleted(sender, new ArchiveEventArgs(_archivePath));
			}

			//pb_totaalfiles.Invoke(new InvokeEvent((object o, EventArgs l) => { pb_totaalfiles.Value += _deltatotaal; }), sender, e);
			//when compression is finished.
			if (sender is SevenZipExtractor)
				(sender as SevenZipExtractor).Dispose();
		}

		private delegate void ProgressEvent(object sender, ProgressEventArgs e);

		private void Compressing(object sender, ProgressEventArgs e) {
			foreach (string item in _fileAndDirectoryFullPaths) {
				if (item.EndsWith(CurFileString)) {
					CurrentFile = new FileInfo(item);
				}
			}

			//increes percentage from progressbar.
			pb_compression.Invoke(new ProgressEvent((object o, ProgressEventArgs l) => {
				int FileProcentDone = (int)(((float)l.PercentDelta / (float)CurrentFile.Length) * 100);
				pb_compression.Value += FileProcentDone;
			}), sender, e);
		}

		private FileInfo CurrentFile;
		private int inte = -1;
		private string CurFileString = "";

		private delegate void FileEvent(object sender, FileNameEventArgs e);

		private void FileCompressionStarted(object sender, FileNameEventArgs e) {
			e.Cancel = IsCancel;
			inte++;
			CurFileString = e.FileName;
			//when the next file is being compressed.
			pb_compression.Invoke(new InvokeEvent((object o, EventArgs l) => {
				pb_compression.Value = 0;
			}), sender, e);

			lbl_commpressing_file.Invoke(new FileEvent((object o, FileNameEventArgs l) => {
				lbl_commpressing_file.Text = String.Format(_commpressingFileName, l.FileName);
			}), sender, e);

			pb_totaalfiles.Invoke(new FileEvent((object o, FileNameEventArgs l) => {
				pb_totaalfiles.Value = l.PercentDone;
			}), sender, e);
		}

		private bool IsCancel = false;

		public void CancelCompression(object sender, EventArgs e) {
			_thread.Abort();

			IsCancel = true;

			//while (!_thread.IsAlive)
			//{
			//    Dispose(true);
			//}
		}

		public void Close(object sender, EventArgs e) {
			Dispose(true);
		}

		private void ArchiveProcressScreen_Shown(object sender, EventArgs e) {
			switch (_action) {
				case ArchiveAction.Compress:
					_commpressingFileName = "Compressing \"{0}\"";
					lbl_compressing_to.Text = String.Format("Compressing to \"{0}\"", _archivePath);
					_thread = new SafeThread(new ThreadStart(Compress));
					_thread.ShouldReportThreadAbort = false;

					_thread.ThreadException += new ThreadThrewExceptionHandler(_thread_ThreadException);
					break;

				case ArchiveAction.Extract:
					_commpressingFileName = "Extracting \"{0}\"";
					lbl_compressing_to.Text = String.Format("Extracting to \"{0}\"", _archivePath);
					_thread = new SafeThread(new ThreadStart(Extract));
					break;

				case ArchiveAction.RemoveFile:
					_commpressingFileName = "Compressing \"{0}\"";
					lbl_compressing_to.Text = String.Format("Compressing to \"{0}\"", _archivePath);
					_thread = new SafeThread(new ThreadStart(RemoveFile));
					break;
			}

			_thread.Start();
		}

		private void _thread_ThreadException(SafeThread thrd, Exception ex) {
		}
	}

	[Obsolete("Should be changed into a Tuple<string>")]
	public class ArchiveEventArgs {

		public string OutputLocation {
			get;
			private set;
		}


		public ArchiveEventArgs(string output) {
			OutputLocation = output;
		}


	}

	public enum ArchiveAction {
		Compress,
		Extract,
		RemoveFile
	}
}