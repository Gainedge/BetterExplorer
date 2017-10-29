using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using wyUpdate.Common;


namespace wyDay.Controls {
  internal class UpdateHelper : IDisposable {
    // Constants
    const int MaxSendRetries = 20;
    const int MilliSecsBetweenRetry = 250;

    // 20 * 250 = 5000 ms = 5 seconds for an error to show

    Process ClientProcess;

    string m_CompleteWULoc;
    string m_wyUpdateLocation = "wyUpdate.exe";

    public string wyUpdateLocation {
      get { return m_wyUpdateLocation; }
      set {
        m_wyUpdateLocation = value;

        // Try to create the complete wyUpdate path (expanding relative paths, etc.) . If it fails,
        // we'll be notifying the user later. (Hence the empty "catch" block).
        try {
          m_CompleteWULoc = Path.GetFullPath(Path.IsPathRooted(value)
                              ? value
                              : Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), value)
                            );
        } catch { }
      }
    }

    // extra arguments for wyUpdate
    public string ExtraArguments;

    PipeClient pipeClient;


    BackgroundWorker bw;

    public event UpdateStepMismatchHandler UpdateStepMismatch;
    public event ResponseHandler ProgressChanged;
    public event ResponseHandler PipeServerDisconnected;
    public event EventHandler ResendRestartInfo;

    public UpdateStep UpdateStep = UpdateStep.CheckForUpdate;

    [DllImport("user32.dll")]
    static extern int ShowWindow(int hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(int hWnd);

    readonly Stack<UpdateHelperData> sendBuffer = new Stack<UpdateHelperData>(1);
    readonly List<UpdateHelperData> receivedBuffer = new List<UpdateHelperData>();
    public bool BufferResponse;

    #region Dispose

    /// <summary>Indicates whether this instance is disposed.</summary>
    private bool isDisposed;

    /// <summary>
    /// Finalizes an instance of the <see cref="UpdateHelper"/> class.
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="UpdateHelper"/> is reclaimed by garbage collection.
    /// </summary>
    ~UpdateHelper() {
      Dispose(false);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">Result: <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
      if (!isDisposed) {
        // Not already disposed ?
        if (disposing) {
          // dispose managed resources
          // Not already disposed ?
          if (pipeClient != null) {
            pipeClient.Dispose(); // Dispose it
            pipeClient = null; // Its now inaccessible
          }
          if (bw != null) {
            bw.Dispose(); // Dispose it
            bw = null; // Its now inaccessible
          }
        }

        // free unmanaged resources
        // Set large fields to null.

        // Instance is disposed
        isDisposed = true;
      }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    public void FlushResponses() {
      lock (receivedBuffer) {
        foreach (var resp in receivedBuffer) {
          // process the response
          ProcessReceivedMessage(resp);
        }

        receivedBuffer.Clear();
        BufferResponse = false;
      }
    }

    public UpdateHelper() {
      // Try to create the complete wyUpdate path. If it fails,
      // we'll be notifying the user later. (Hence the empty "catch" block).
      try {
        m_CompleteWULoc = Path.GetFullPath(Path.IsPathRooted(m_wyUpdateLocation)
                            ? m_wyUpdateLocation
                            : Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), m_wyUpdateLocation)
                          );
      } catch { }

      CreateNewPipeClient();
    }

    void CreateNewPipeClient() {
      if (pipeClient != null) {
        pipeClient.MessageReceived -= SafeProcessReceivedMessage;
        pipeClient.ServerDisconnected -= ServerDisconnected;
      }

      pipeClient = new PipeClient();

      pipeClient.MessageReceived += SafeProcessReceivedMessage;
      pipeClient.ServerDisconnected += ServerDisconnected;
    }

    bool StartClient() {
      if (string.IsNullOrEmpty(m_CompleteWULoc))
        throw new Exception("The wyUpdate executable path supplied is not valid. Make sure wyUpdate exists on disk.");

      // get the unique pipe name (the last 246 chars of the complete path)
      string pipeName = UpdateHelperData.PipenameFromFilename(m_CompleteWULoc);

      // first try to connect to the pipe
      if (!pipeClient.Connected)
        pipeClient.Connect(pipeName);

      if (pipeClient.Connected) {
        // request the processId
        if (!RetrySend((new UpdateHelperData(UpdateAction.GetwyUpdateProcessID)).GetByteArray()))
          throw new Exception("Failed to get the wyUpdate Process ID.");

        return true;
      }

      if (!File.Exists(m_CompleteWULoc))
        throw new Exception("The wyUpdate executable was not found: " + m_CompleteWULoc);

      ClientProcess = new Process {
        StartInfo =
                                  {
                                            FileName = m_CompleteWULoc,

                                            // start the client in automatic update mode (a.k.a. wait mode)
                                            Arguments = "/autoupdate",

                                            WindowStyle = ProcessWindowStyle.Hidden
                                        }
      };

      if (!string.IsNullOrEmpty(ExtraArguments))
        ClientProcess.StartInfo.Arguments += " " + ExtraArguments;

      ClientProcess.Start();

      TryToConnectToPipe(pipeName);

      return pipeClient.Connected;
    }

    void TryToConnectToPipe(string pipename) {
      // try to connect to the pipe - bail out if it takes longer than 30 seconds
      for (int retries = 0; !pipeClient.Connected && retries < 120; retries++) {
        pipeClient.Connect(pipename);

        // wait half a second
        if (!pipeClient.Connected) {
          if (ClientProcess == null || ClientProcess.HasExited) {
            ClientProcess = null;
            break;
          }

          Thread.Sleep(250);
        }
      }

      if (!pipeClient.Connected && ClientProcess != null) {
        // try to kill the process without throwing any exceptions
        try {
          ClientProcess.Kill();
        } catch { }

        ClientProcess = null;
      }
    }

    void ServerDisconnected() {
      ClientProcess = null;

      if (PipeServerDisconnected != null)
        PipeServerDisconnected(this, new UpdateHelperData(Response.Failed, UpdateStep, AUTranslation.C_PrematureExitTitle, AUTranslation.C_PrematureExitMessage));
    }

    void bw_DoWork(object sender, DoWorkEventArgs e) {
      // start the client if it's not already running.
      if (ClientProcess == null) {
        if (!StartClient())
          throw new Exception("wyUpdate failed to start.");
      }

      if (!RetrySend(((UpdateHelperData)e.Argument).GetByteArray()))
        throw new Exception("Message failed to send message to pipe server");
    }

    /// <summary>Tries to send a message MaxSendRetries waiting MilliSecsBetweenRetry.</summary>
    /// <param name="message">Message to send.</param>
    /// <returns>True if success.</returns>
    bool RetrySend(byte[] message) {
      bool messageFailedToSend;

      for (int retries = 0;

          // try to send the message
          (messageFailedToSend = !pipeClient.SendMessage(message))
              && retries < MaxSendRetries
              && pipeClient.Connected;

          retries++) {
        // wait between retries
        Thread.Sleep(MilliSecsBetweenRetry);
      }

      // if the client process has already exited, just say the send succeeded
      // otherwise return whether the send actually succeeded
      return pipeClient.Connected ? !messageFailedToSend : true;
    }

    void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      bw.DoWork -= bw_DoWork;
      bw.RunWorkerCompleted -= bw_RunWorkerCompleted;
      bw.Dispose();
      bw = null;

      // error occurs when a message fails to send or wyUpdate fails to start
      if (e.Error != null) {
        // if the process is running - try to kill it
        try {
          if (ClientProcess != null && !ClientProcess.HasExited)
            ClientProcess.Kill();
        } catch { }

        // set the client process to null so it can be restarted
        // when the user retries.
        ClientProcess = null;

        // clear the to-send stack
        sendBuffer.Clear();

        // inform the AutomaticUpdater that wyUpdate is no longer running
        if (PipeServerDisconnected != null)
          PipeServerDisconnected(this, new UpdateHelperData(Response.Failed, UpdateStep, AUTranslation.C_PrematureExitTitle, e.Error.Message));
      } else {
        // process the next in stack
        if (sendBuffer.Count > 0) {
          RecreateBackgroundWorker();

          UpdateHelperData uhd = sendBuffer.Pop();
          UpdateStep = uhd.UpdateStep;

          // begin sending to the client
          bw.RunWorkerAsync(uhd);
        }
      }
    }

    public void ForceRecheckForUpdate() {
      SendAsync(new UpdateHelperData(UpdateStep.ForceRecheckForUpdate));
    }

    public void CheckForUpdate() {
      SendAsync(new UpdateHelperData(UpdateStep.CheckForUpdate));
    }

    public void DownloadUpdate() {
      SendAsync(new UpdateHelperData(UpdateStep.DownloadUpdate));
    }

    public void BeginExtraction() {
      SendAsync(new UpdateHelperData(UpdateStep.BeginExtraction));
    }

    public void RestartInfo(string fileToExecute, string autoUpdateID, string argumentsForFiles, bool isAService) {
      UpdateHelperData uhd = new UpdateHelperData(UpdateStep.RestartInfo);

      uhd.ExtraData.Add(fileToExecute);
      uhd.ExtraDataIsRTF.Add(isAService);

      if (!string.IsNullOrEmpty(autoUpdateID)) {
        uhd.ExtraData.Add(autoUpdateID);
        uhd.ExtraDataIsRTF.Add(false);

        if (!string.IsNullOrEmpty(argumentsForFiles)) {
          uhd.ExtraData.Add(argumentsForFiles);
          uhd.ExtraDataIsRTF.Add(false);
        }
      }

      SendAsync(uhd);
    }

    int ClientWindowHandleToShow;

    public void InstallNow() {
      // show & set as foreground window ( SW_RESTORE = 9)
      ShowWindow(ClientWindowHandleToShow, 9);
      SetForegroundWindow(ClientWindowHandleToShow);

      //begin installing the update
      SendAsync(new UpdateHelperData(UpdateStep.Install));
    }

    public void Cancel() {
      SendAsync(new UpdateHelperData(UpdateAction.Cancel));
    }

    void RecreateBackgroundWorker() {
      // Fixes windows service bugs: http://wyday.com/forum/viewtopic.php?f=1&t=2949
      Application.DoEvents();
      bw = new BackgroundWorker();
      bw.DoWork += bw_DoWork;
      bw.RunWorkerCompleted += bw_RunWorkerCompleted;
    }

    void SendAsync(UpdateHelperData uhd) {
      // if currently working, add the new message to the stack
      if (bw != null) {
        sendBuffer.Push(uhd);
      } else {
        RecreateBackgroundWorker();

        // process immediately
        UpdateStep = uhd.UpdateStep == UpdateStep.ForceRecheckForUpdate ? UpdateStep.CheckForUpdate : uhd.UpdateStep;

        // begin sending to the client
        bw.RunWorkerAsync(uhd);
      }
    }

    void SafeProcessReceivedMessage(byte[] message) {
      // Cast the data to the type of object we sent:
      UpdateHelperData data = UpdateHelperData.FromByteArray(message);

      if (BufferResponse) {
        lock (receivedBuffer) {
          // only buffer important messages
          if (data.ResponseType != Response.Progress)
            receivedBuffer.Add(data);
        }

        // checks if we're still buffering responses, if so don't process it now
        if (BufferResponse)
          return;
      }

      ProcessReceivedMessage(data);
    }

    void ProcessReceivedMessage(UpdateHelperData data) {
      if (data.Action == UpdateAction.GetwyUpdateProcessID) {
        ClientProcess = Process.GetProcessById(data.ProcessID);
        return;
      }

      if (data.Action == UpdateAction.NewWyUpdateProcess) {
        // disconnect from the existing pipeclient
        pipeClient.Disconnect();

        CreateNewPipeClient();

        try {
          ClientProcess = Process.GetProcessById(data.ProcessID);
        } catch (Exception) {
          // inform the AutomaticUpdater that wyUpdate is no longer running
          if (PipeServerDisconnected != null)
            PipeServerDisconnected(this, new UpdateHelperData(Response.Failed, UpdateStep, AUTranslation.C_PrematureExitTitle, "Failed to connect to the new version of wyUpdate.exe"));
        }

        TryToConnectToPipe(data.ExtraData[0]);

        // if the process is running - try to kill it
        if (!pipeClient.Connected) {
          // inform the AutomaticUpdater that wyUpdate is no longer running
          if (PipeServerDisconnected != null)
            PipeServerDisconnected(this, new UpdateHelperData(Response.Failed, UpdateStep, AUTranslation.C_PrematureExitTitle, "Failed to connect to the new version of wyUpdate.exe"));
        }

        // begin where we left off
        // if update step == RestartInfo, we need to send the restart info as well
        if (ResendRestartInfo != null && UpdateStep == UpdateStep.RestartInfo)
          ResendRestartInfo(this, EventArgs.Empty);
        else
          SendAsync(new UpdateHelperData(UpdateStep));

        return;
      }

      if (data.UpdateStep != UpdateStep) {
        // this occurs when wyUpdate is on a separate step from the AutoUpdater

        UpdateStep prev = UpdateStep;

        // set new update step
        UpdateStep = data.UpdateStep;

        // tell AutoUpdater that the message we sent didn't respond in kind
        // e.g. we sent RestartInfo, and wyUpdate responded with DownloadUpdate
        // meaning we can't update yet, we're just begginning downloading the update
        if (UpdateStepMismatch != null)
          UpdateStepMismatch(this, data.ResponseType, prev);
      }

      // wyUpdate will give us its main Window Handle so we can pass focus to it
      if (data.Action == UpdateAction.UpdateStep && data.UpdateStep == UpdateStep.RestartInfo)
        ClientWindowHandleToShow = data.ProcessID;

      if (data.ResponseType != Response.Nothing && ProgressChanged != null)
        ProgressChanged(this, data);
    }
  }

  internal delegate void ResponseHandler(object sender, UpdateHelperData e);
  internal delegate void UpdateStepMismatchHandler(object sender, Response respType, UpdateStep previousStep);
}
