using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessInterface
{

    /// <summary>
    /// A ProcessEventHandler is a delegate for process input/output events.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ProcessInterface.ProcessEventArgs"/> instance containing the event data.</param>
    public delegate void ProcessEventHanlder(object sender, ProcessEventArgs args);

    /// <summary>
    /// A class the wraps a process, allowing programmatic input and output.
    /// </summary>
    public class ProcessInterface
    {

        #region Properties

        /// <summary>
        /// The input writer.
        /// </summary>
        private StreamWriter inputWriter;

        /// <summary>
        /// The output reader.
        /// </summary>
        private TextReader outputReader;

        /// <summary>
        /// The error reader.
        /// </summary>
        private TextReader errorReader;

        /// <summary>
        /// The output worker.
        /// </summary>
        private BackgroundWorker outputWorker = new BackgroundWorker();

        /// <summary>
        /// The error worker.
        /// </summary>
        private BackgroundWorker errorWorker = new BackgroundWorker();

        /// <summary>
        /// Gets the internal process.
        /// </summary>
        public Process Process { get; private set; }

        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        /// <value>
        /// The name of the process.
        /// </value>
        public string ProcessFileName { get; private set; }

        /// <summary>
        /// Gets the process arguments.
        /// </summary>
        public string ProcessArguments { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is process running.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is process running; otherwise, <c>false</c>.
        /// </value>
        public bool IsProcessRunning
        {
            get
            {
                //TODO: try removing this [Try Catch]
                try
                {
                    return Process != null && !Process.HasExited;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion Properties

        #region Events/DllImport
        /*
		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleOutputCP(uint wCodePageID);
		*/
        /// <summary>
        /// Occurs when process output is produced.
        /// </summary>
        public event ProcessEventHanlder OnProcessOutput;

        /// <summary>
        /// Occurs when process error output is produced.
        /// </summary>
        public event ProcessEventHanlder OnProcessError;

        /// <summary>
        /// Occurs when process input is produced.
        /// </summary>
        public event ProcessEventHanlder OnProcessInput;

        /// <summary>
        /// Occurs when the process ends.
        /// </summary>
        public event ProcessEventHanlder OnProcessExit;

        #endregion Events/DllImport

        #region Public Methods

        /// <summary>
        /// Runs a process.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        public IntPtr StartProcess(string fileName, string arguments)
        {
            //  Create the process start info.
            var processStartInfo = new ProcessStartInfo(fileName, arguments);

            //  Set the options.
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.CreateNoWindow = true;

            //  Specify redirection.
            processStartInfo.StandardOutputEncoding = Encoding.UTF8;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;

            //  Create the process.
            Process = new Process();
            Process.EnableRaisingEvents = true;
            Process.StartInfo = processStartInfo;
            Process.Exited += new EventHandler(currentProcess_Exited);

            //  Start the process.
            try
            {
                //bool processStarted = Process.Start();
                Process.Start();
            }
            catch (Exception e)
            {
                //  Trace the exception.
                Trace.WriteLine("Failed to start process " + fileName + " with arguments '" + arguments + "'");
                Trace.WriteLine(e.ToString());
                return IntPtr.Zero;
            }

            //  Store name and arguments.
            ProcessFileName = fileName;
            ProcessArguments = arguments;

            //  Create the readers and writers.
            inputWriter = Process.StandardInput;
            outputReader = TextReader.Synchronized(new StreamReader(Process.StandardOutput.BaseStream, Encoding.UTF8));
            errorReader = TextReader.Synchronized(Process.StandardError);

            //  Run the workers that read output and error.
            outputWorker.RunWorkerAsync();
            errorWorker.RunWorkerAsync();
            return Process.Handle;
        }

        /// <summary>
        /// Stops the process.
        /// </summary>
        public void StopProcess()
        {
            //  Handle the trivial case.
            if (!IsProcessRunning)
                return;

            //  Kill the process.
            Process.Kill();
        }

        /// <summary>
        /// Writes the input.
        /// </summary>
        /// <param name="input">The input.</param>
        public void WriteInput(string input)
        {
            if (IsProcessRunning)
            {
                inputWriter.WriteLine(input);
                inputWriter.Flush();
            }
        }

        #endregion Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInterface"/> class.
        /// </summary>
        public ProcessInterface()
        {
            //  Configure the output worker.
            outputWorker.WorkerReportsProgress = true;
            outputWorker.WorkerSupportsCancellation = true;
            outputWorker.DoWork += new DoWorkEventHandler(outputWorker_DoWork);
            outputWorker.ProgressChanged += new ProgressChangedEventHandler(outputWorker_ProgressChanged);

            //  Configure the error worker.
            errorWorker.WorkerReportsProgress = true;
            errorWorker.WorkerSupportsCancellation = true;
            errorWorker.DoWork += new DoWorkEventHandler(errorWorker_DoWork);
            errorWorker.ProgressChanged += new ProgressChangedEventHandler(errorWorker_ProgressChanged);
        }

        /// <summary>
        /// Handles the ProgressChanged event of the outputWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void outputWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //  We must be passed a string in the user state.
            if (e.UserState is string)
            {
                //  Fire the output event.
                FireProcessOutputEvent(e.UserState as string);
            }
        }

        /// <summary>
        /// Handles the DoWork event of the outputWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
        private void outputWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (outputWorker.CancellationPending == false)
            {
                //  Any lines to read?
                int count = 0;
                char[] buffer = new char[1024];
                do
                {
                    StringBuilder builder = new StringBuilder();
                    count = outputReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    outputWorker.ReportProgress(0, builder.ToString());
                } while (count > 0);

                System.Threading.Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Handles the ProgressChanged event of the errorWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void errorWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //  The userstate must be a string.
            if (e.UserState is string)
            {
                //  Fire the error event.
                FireProcessErrorEvent(e.UserState as string);
            }
        }

        /// <summary>
        /// Handles the DoWork event of the errorWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
        private void errorWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (errorWorker.CancellationPending == false)
            {
                //  Any lines to read?
                int count = 0;
                char[] buffer = new char[1024];
                do
                {
                    StringBuilder builder = new StringBuilder();
                    count = errorReader.Read(buffer, 0, 1024);
                    builder.Append(buffer, 0, count);
                    errorWorker.ReportProgress(0, builder.ToString());
                } while (count > 0);

                System.Threading.Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Handles the Exited event of the currentProcess control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void currentProcess_Exited(object sender, EventArgs e)
        {
            //  Fire process exited.
            FireProcessExitEvent(Process.ExitCode);

            //  Disable the threads.
            outputWorker.CancelAsync();
            errorWorker.CancelAsync();
            inputWriter = null;
            outputReader = null;
            errorReader = null;
            Process = null;
            ProcessFileName = null;
            ProcessArguments = null;
        }

        /// <summary>
        /// Fires the process output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireProcessOutputEvent(string content) => OnProcessOutput?.Invoke(this, new ProcessEventArgs(content));

        /// <summary>
        /// Fires the process error output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireProcessErrorEvent(string content) => OnProcessError?.Invoke(this, new ProcessEventArgs(content));

        /// <summary>
        /// Fires the process exit event.
        /// </summary>
        /// <param name="code">The code.</param>
        private void FireProcessExitEvent(int code) => OnProcessExit?.Invoke(this, new ProcessEventArgs(code));
    }
}