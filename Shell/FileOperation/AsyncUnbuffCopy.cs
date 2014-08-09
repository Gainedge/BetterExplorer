using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BExplorer.Shell {
	internal class AsyncUnbuffCopy : IDisposable {
		#region setup
		//private static readonly ILog Log = LogManager.GetLogger(typeof(AsyncUnbuffCopy));
		//private readonly bool _isDebugEnabled = Log.IsDebugEnabled;
		public ManualResetEvent blockFO = new ManualResetEvent(false);
		//file names
		private string _inputfile;
		private string _outputfile;

		//checksum holders
		// private string _infilechecksum;
		// private string _outfilechecksum;

		private bool _checksum;

		//show write progress
		private bool _reportprogress;

		//cursor position
		private int _origRow;
		private int _origCol;

		//number of chunks to copy
		private int _numchunks;

		//track read state and read failed state
		private bool _readfailed;

		//syncronization object
		private readonly object _locker1 = new object();

		//buffer size
		public int CopyBufferSize;
		private long _infilesize;

		//buffer read
		public byte[] Buffer1;
		private int _bytesRead1;

		//buffer overlap
		public byte[] Buffer2;
		private bool _buffer2Dirty;

		//buffer write
		public byte[] Buffer3;

		//total bytes read
		private long _totalbytesread;
		private long _totalbyteswritten;

		//filestreams
		private FileStream _infile;
		private FileStream _outfile;

		//secret sauce for unbuffered IO
		const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;

		private byte[] _readhash;
		private byte[] _writehash;
		#endregion

		public void Dispose() {
			if (_infile != null) {
				_infile.Dispose();
				_infile = null;
			}
			if (_outfile != null) {
				_outfile.Dispose();
				_outfile = null;
			}
			if (blockFO != null) {
				blockFO.Dispose();
				blockFO = null;
			}
		}
		private void AsyncReadFile() {

			var md5 = MD5.Create();

			//open input file
			_infile = new FileStream(_inputfile, FileMode.Open, FileAccess.Read, FileShare.None, CopyBufferSize,
			FileFlagNoBuffering | FileOptions.SequentialScan);

			//if we have data read it
			while ((_bytesRead1 = _infile.Read(Buffer1, 0, CopyBufferSize)) != 0) {
				blockFO.WaitOne();
				if (_bytesRead1 < CopyBufferSize)

					if (_checksum)
						md5.TransformBlock(Buffer1, 0, _bytesRead1, Buffer1, 0);

				Monitor.Enter(_locker1);
				try {
					while (_buffer2Dirty) Monitor.Wait(_locker1);
					Buffer.BlockCopy(Buffer1, 0, Buffer2, 0, _bytesRead1);

					_buffer2Dirty = true;
					_totalbytesread = _totalbytesread + _bytesRead1;

					Monitor.PulseAll(_locker1);
					//if (_isDebugEnabled)
					//{
					//	//Log.Debug("Block Read : " + _bytesRead1);
					//	//Log.Debug("Total Read : " + _totalbytesread);
					//}
				}
				catch {
					_readfailed = true;
					throw;
				}
				finally { Monitor.Exit(_locker1); }

			}
			// For last block:
			if (_checksum) {
				md5.TransformFinalBlock(Buffer1, 0, _bytesRead1);
				_readhash = md5.Hash;
			}

			//clean up open handle
			_infile.Close();
			_infile.Dispose();
		}

		private void AsyncWriteFile() {
			//open file for write unbuffered and set length to prevent growth and file fragmentation
			_outfile = new FileStream(_outputfile, FileMode.Create, FileAccess.Write, FileShare.None, 8,
			FileOptions.WriteThrough | FileFlagNoBuffering);

			//set file size to minimum of one buffer to cut down on fragmentation
			_outfile.SetLength((long)(_infilesize > CopyBufferSize ? (Math.Ceiling((double)_infilesize / CopyBufferSize) * CopyBufferSize) : CopyBufferSize));


			var pctinc = 0.0;
			var progress = pctinc;

			//progress stuff
			if (_reportprogress) {
				//if (_isDebugEnabled)
				//{
				//	//Log.Debug("Report Progress : True");
				//}
				pctinc = 100.00 / _numchunks;
			}
			//if (_isDebugEnabled)
			//{
			//	//Log.Debug("_totalbyteswritten : " + _totalbyteswritten);
			//	//Log.Debug("_infilesize - CopyBufferSize: " + (_infilesize - CopyBufferSize));
			//}
			while ((_totalbyteswritten < _infilesize) && !_readfailed) {
				blockFO.WaitOne();
				lock (_locker1) {
					while (!_buffer2Dirty) Monitor.Wait(_locker1);
					Buffer.BlockCopy(Buffer2, 0, Buffer3, 0, CopyBufferSize);
					_buffer2Dirty = false;
					Monitor.PulseAll(_locker1);
					//fancy dan in place percent update on each write.

					if (_reportprogress) {
						Console.SetCursorPosition(_origCol, _origRow);
						if (progress < 101 - pctinc) {
							progress = progress + pctinc;
							Console.Write("%{0}", Math.Round(progress, 0));
						}
					}
				}

				_outfile.Write(Buffer3, 0, CopyBufferSize);

				_totalbyteswritten = _totalbyteswritten + CopyBufferSize;
				//if (_isDebugEnabled)
				//{
				//	//Log.Debug("Written Unbuffered : " + _totalbyteswritten);
				//}
			}
			//close the file handle that was using unbuffered and write through and move the EOF pointer.
			//Log.Debug("Close Write File Unbuffered");
			_outfile.Close();
			_outfile.Dispose();

			//if (_isDebugEnabled)
			//{
			//	//Log.Debug("Open File Set Length");
			//}
			_outfile = new FileStream(_outputfile, FileMode.Open, FileAccess.Write, FileShare.None, 8,
			FileOptions.WriteThrough);
			_outfile.SetLength(_infilesize);
			_outfile.Close();
			_outfile.Dispose();
		}

		public int AsyncCopyFileUnbuffered(string inputfile, string outputfile, bool overwrite, bool movefile, bool checksum, int buffersize, bool reportprogress, int bytessecond) {
			//if (_isDebugEnabled)
			//{
			//	Log.Error("Enter Normal Method");
			//	Log.Debug("inputfile : " + inputfile);
			//	Log.Debug("outputfile : " + outputfile);
			//	Log.Debug("overwrite : " + overwrite);
			//	Log.Debug("movefile : " + movefile);
			//	Log.Debug("checksum : " + checksum);
			//	Log.Debug("buffersize : " + buffersize);
			//	Log.Debug("reportprogress : " + reportprogress);
			//}
			//do we do the checksum?
			_checksum = checksum;

			//report write progress
			_reportprogress = reportprogress;

			//set file name globals
			_inputfile = inputfile;
			_outputfile = outputfile;

			//if the overwrite flag is set to false check to see if the file is there.
			if (File.Exists(outputfile) && !overwrite) {
				//Log.Debug("Destination File Exists!");
				return 0;
			}

			//create the directory if it doesn't exist
			if (!File.Exists(outputfile))
				if (!Directory.Exists(outputfile)) {
					// ReSharper disable AssignNullToNotNullAttribute
					Directory.CreateDirectory(Path.GetDirectoryName(outputfile));
					// ReSharper restore AssignNullToNotNullAttribute
				}
			//get input file size for later use
			var inputFileInfo = new FileInfo(_inputfile);
			_infilesize = inputFileInfo.Length;

			//setup single buffer size in megabytes, remember this will be x3.
			CopyBufferSize = buffersize * 1048576;

			//if (_infilesize < UBCopySetup.SynchronousFileCopySize)
			//{
			//	SynchronousUnbufferedCopy.SyncCopyFileUnbuffered(_inputfile, _outputfile, 1048576, bytessecond, out _readhash);
			//}
			//else
			//{
			//buffer read
			Buffer1 = new byte[CopyBufferSize];

			//buffer overlap
			Buffer2 = new byte[CopyBufferSize];

			//buffer write
			Buffer3 = new byte[CopyBufferSize];

			//clear all flags and handles
			_totalbytesread = 0;
			_totalbyteswritten = 0;
			_bytesRead1 = 0;
			_buffer2Dirty = false;

			//get number of buffer sized chunks used to correctly display percent complete.
			_numchunks = (int)((_infilesize / CopyBufferSize) <= 0 ? (_infilesize / CopyBufferSize) : 1);

			//create read thread and start it.
			var readfile = new Thread(AsyncReadFile) { Name = "Read Thread", IsBackground = true };
			readfile.Start();

			//if (_isDebugEnabled)
			//{
			//	//debug show if we are an even multiple of the file size
			//	//Log.Debug("Number of Chunks: " + _numchunks);
			//}

			//create write thread and start it.
			var writefile = new Thread(AsyncWriteFile) { Name = "WriteThread", IsBackground = true };
			writefile.Start();

			if (_reportprogress) {
				//set fancy curor position
				_origRow = Console.CursorTop;
				_origCol = Console.CursorLeft;
			}

			//wait for threads to finish
			readfile.Join();
			writefile.Join();

			//leave a blank line for the progress indicator
			if (_reportprogress)
				Console.WriteLine();

			//Log.InfoFormat("Async File {0} Done", _inputfile);
			//}

			if (checksum) {
				//if (_isDebugEnabled)
				//{
				//	//Log.Debug("Checksum Destination File Started");
				//}
				//create checksum write file thread and start it.
				var checksumwritefile = new Thread(GetMD5HashFromOutputFile) { Name = "checksumwritefile", IsBackground = true };
				checksumwritefile.Start();

				//hang out until the checksums are done.
				checksumwritefile.Join();

				if (BitConverter.ToString(_readhash) == BitConverter.ToString(_writehash)) {
					//Log.Info("Checksum Verified");
				}
				else {
					//Log.Info("Checksum Failed");

					var sb = new StringBuilder();
					for (var i = 0; i < _readhash.Length; i++) {
						sb.Append(_readhash[i].ToString("x2"));
					}
					//Log.DebugFormat("_readhash output : {0}", sb);


					sb = new StringBuilder();
					for (var i = 0; i < _writehash.Length; i++) {
						sb.Append(_writehash[i].ToString("x2"));
					}
					//Log.DebugFormat("_writehash output : {0}", sb);
					throw new Exception("File Failed Checksum");
				}
			}

			if (movefile && File.Exists(inputfile) && File.Exists(outputfile)) {
				try {
					File.Delete(inputfile);
				}
				catch {
				}
			}
			return 1;
		}

		//hash output file
		public void GetMD5HashFromOutputFile() {
			var md5 = MD5.Create();
			var fs = new FileStream(_outputfile,
			FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize,
			FileFlagNoBuffering | FileOptions.SequentialScan);

			var buff = new byte[CopyBufferSize];
			int bytesread;
			while ((bytesread = fs.Read(buff, 0, buff.Length)) != 0) {
				md5.TransformBlock(buff, 0, bytesread, buff, 0);
			}
			md5.TransformFinalBlock(buff, 0, bytesread);
			_writehash = md5.Hash;
			fs.Close();

		}
	}
}
