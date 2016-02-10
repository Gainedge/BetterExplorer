//
// AsyncUnbuffCopy.cs
//
// Authors:
//  Wesley D. Brown <wes@planetarydb.com>
//
// Copyright (C) 2010 SQLServerIO (http://www.SQLServerIO.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BExplorer.Shell {
	public class CopyEventArgs {
		public String InFileName { get; set; }
		public String OutFileName { get; set; }
		public long BytesTransferred { get; set; }
		public long TotalBytes { get; set; }
	}
	public class AsyncUnbuffCopy {

		public event EventHandler<CopyEventArgs> OnProgress;

		//file names
		private static string _inputfile;
		private static string _outputfile;

		//checksum holders
		private static string _infilechecksum;
		private static string _outfilechecksum;

		//show write progress
		private static bool _reportprogress;

		//cursor position
		private static int _origRow;
		private static int _origCol;

		//number of chunks to copy
		private static int _numchunks;

		//track read state and read failed state
		private static bool _readfailed;

		//syncronization object
		private static readonly object Locker1 = new object();

		//buffer size
		public static int CopyBufferSize;
		private static long _infilesize;

		//buffer read
		public static byte[] Buffer1;
		private static int _bytesRead1;

		//buffer overlap
		public static byte[] Buffer2;
		private static bool _buffer2Dirty;
		private static int _bytesRead2;

		//buffer write
		public static byte[] Buffer3;

		//total bytes read
		private static long _totalbytesread;
		private static long _totalbyteswritten;

		//filestreams
		private static FileStream _infile;
		private static FileStream _outfile;

		//secret sauce for unbuffered IO
		const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;

		private static void AsyncReadFile() {
			//open input file
			try {
				_infile = new FileStream(_inputfile, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, FileFlagNoBuffering);
			} catch {
				throw;
			}
			//if we have data read it
			while (_totalbytesread < _infilesize) {

				_bytesRead1 = _infile.Read(Buffer1, 0, CopyBufferSize);
				Monitor.Enter(Locker1);
				try {
					while (_buffer2Dirty) Monitor.Wait(Locker1);
					Buffer.BlockCopy(Buffer1, 0, Buffer2, 0, _bytesRead1);
					_buffer2Dirty = true;
					_bytesRead2 = _bytesRead1;
					_totalbytesread = _totalbytesread + _bytesRead1;
					Monitor.PulseAll(Locker1);

				} catch {
					_readfailed = true;
					throw;
				} finally { Monitor.Exit(Locker1); }
			}
			//clean up open handle
			_infile.Close();
			_infile.Dispose();
		}

		private void AsyncWriteFile() {
			//open output file set length to prevent growth and file fragmentation and close it.
			//We do this to prevent file fragmentation and make the write as fast as possible.
			try {
				_outfile = new FileStream(_outputfile, FileMode.Create, FileAccess.Write, FileShare.None, 8, FileOptions.WriteThrough);

				//set file size to minimum of one buffer to cut down on fragmentation
				_outfile.SetLength(_infilesize > CopyBufferSize ? _infilesize : CopyBufferSize);

				_outfile.Close();
				_outfile.Dispose();
			} catch {
				throw;
			}

			//open file for write unbuffered
			try {
				_outfile = new FileStream(_outputfile, FileMode.Open, FileAccess.Write, FileShare.None, 8, FileOptions.WriteThrough | FileFlagNoBuffering);

			} catch { 
				throw;
			}

			var pctinc = 0.0;
			var progress = pctinc;

			//progress stuff
			if (_reportprogress) {

				pctinc = 100.00 / _numchunks;
			}

			while ((_totalbyteswritten < _infilesize) && !_readfailed) {
				lock (Locker1) {

					while (!_buffer2Dirty) Monitor.Wait(Locker1);

					Buffer.BlockCopy(Buffer2, 0, Buffer3, 0, _bytesRead2);
					_buffer2Dirty = false;

					_totalbyteswritten = _totalbyteswritten + CopyBufferSize;

					Monitor.PulseAll(Locker1);
					//fancy dan in place percent update on each write.
					if (this.OnProgress != null) {
						this.OnProgress.Invoke(this, new CopyEventArgs() {
							InFileName = _inputfile,
							OutFileName = _outputfile,
							BytesTransferred = _totalbyteswritten,
							TotalBytes = _infilesize
						});
					}
					if (_reportprogress) {
						
						Console.SetCursorPosition(_origCol, _origRow);
						if (progress < 101 - pctinc) {
							progress = progress + pctinc;
							Console.Write("%{0}", Math.Round(progress, 0));
						}
					}
				}
				try {
					_outfile.Write(Buffer3, 0, CopyBufferSize);
				} catch {
                    throw;
				}
			}

			//close the file handle that was using unbuffered and write through

			_outfile.Close();
			_outfile.Dispose();

			try {

				_outfile = new FileStream(_outputfile, FileMode.Open, FileAccess.Write, FileShare.None, 8,
																	FileOptions.WriteThrough);
				_outfile.SetLength(_infilesize);
				_outfile.Close();
				_outfile.Dispose();
			} catch {
				throw;
			}
		}

		public int AsyncCopyFileUnbuffered(string inputfile, string outputfile, bool overwrite,bool movefile, bool checksum, int buffersize, bool reportprogress) {

			//report write progress
			_reportprogress = reportprogress;

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

			//clear all flags and handles
			_totalbytesread = 0;
			_totalbyteswritten = 0;
			_bytesRead1 = 0;
			_buffer2Dirty = false;

			//if the overwrite flag is set to false check to see if the file is there.
			if (File.Exists(outputfile) && !overwrite) {

				Console.WriteLine("Destination File Exists!");
				return 0;
			}

			//create the directory if it doesn't exist
			if (!Directory.Exists(outputfile)) {
				try {
					// ReSharper disable AssignNullToNotNullAttribute
					Directory.CreateDirectory(Path.GetDirectoryName(outputfile));
					// ReSharper restore AssignNullToNotNullAttribute
				} catch (Exception e) {

					Console.WriteLine("Create Directory Failed.");
					Console.WriteLine(e.Message);
					throw;
				}
			}

			//get input file size for later use
			var inputFileInfo = new FileInfo(_inputfile);
			_infilesize = inputFileInfo.Length;

			//get number of buffer sized chunks used to correctly display percent complete.
			_numchunks = (int)((_infilesize / CopyBufferSize) <= 0 ? (_infilesize / CopyBufferSize) : 1);


			Console.WriteLine("File Copy Started");

			//create read thread and start it.
			var readfile = new Thread(AsyncReadFile) { Name = "ReadThread", IsBackground = true };
			readfile.Start();


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



			Console.WriteLine("File Copy Done");

			if (checksum) {

				Console.WriteLine("Checksum Source File Started");
				//create checksum read file thread and start it.
				var checksumreadfile = new Thread(GetMD5HashFromInputFile) {
					Name = "checksumreadfile",
					IsBackground = true
				};
				checksumreadfile.Start();


				Console.WriteLine("Checksum Destination File Started");
				//create checksum write file thread and start it.
				var checksumwritefile = new Thread(GetMD5HashFromOutputFile) {
					Name = "checksumwritefile",
					IsBackground = true
				};
				checksumwritefile.Start();

				//hang out until the checksums are done.
				checksumreadfile.Join();
				checksumwritefile.Join();

				if (_infilechecksum.Equals(_outfilechecksum)) {

					Console.WriteLine("Checksum Verified");
				} else {

					Console.WriteLine("Checksum Failed");
					Console.WriteLine("Input File Checksum : {0}", _infilechecksum);
					Console.WriteLine("Output File Checksum: {0}", _outfilechecksum);
				}
			}

			if (movefile && File.Exists(inputfile) && File.Exists(outputfile))
				try {
					File.Delete(inputfile);
				} catch (IOException ioex) {

					Console.WriteLine("File in use or locked");
					Console.WriteLine(ioex.Message);
				} catch (Exception ex) {

					Console.WriteLine("File Failed to Delete");
					Console.WriteLine(ex.Message);
				}
			return 1;
		}

		//hash input file
		public void GetMD5HashFromInputFile() {
			var fs = new FileStream(_inputfile, FileMode.Open, FileAccess.Read, FileShare.None, CopyBufferSize);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(fs);
			fs.Close();

			var sb = new StringBuilder();
			for (var i = 0; i < retVal.Length; i++) {
				sb.Append(retVal[i].ToString("x2"));
			}
			_infilechecksum = sb.ToString();
		}

		//hash output file
		public void GetMD5HashFromOutputFile() {
			var fs = new FileStream(_outputfile, FileMode.Open, FileAccess.Read, FileShare.None, CopyBufferSize);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(fs);
			fs.Close();

			var sb = new StringBuilder();
			for (var i = 0; i < retVal.Length; i++) {
				sb.Append(retVal[i].ToString("x2"));
			}
			_outfilechecksum = sb.ToString();
		}
	}
}