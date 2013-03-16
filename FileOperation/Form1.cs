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
    public static Guid SourceHandle = Guid.Empty;
    public List<Tuple<String,String>> SourceItemsCollection { get; set; }
    public string DestinationLocation { get; set; }
    private PipeClient _pipeClient;
    private PipeServer _pipeServer;
    public delegate void NewMessageDelegate(string NewMessage);
    public Boolean Cancel = false;
    private OperationType OPType { get; set; }

    private ManualResetEvent _block;
    private ManualResetEvent _block2;
    private int CurrentStatus = -1;
    private bool IsShown = true;
    Thread CopyThread;
    public IntPtr MessageReceiverHandle;
    uint WM_FOWINC = WindowsAPI.RegisterWindowMessage("BE_FOWINC");
    uint WM_FOBEGIN = WindowsAPI.RegisterWindowMessage("BE_FOBEGIN");
    uint WM_FOEND = WindowsAPI.RegisterWindowMessage("BE_FOEND");
    uint WM_FOPAUSE = WindowsAPI.RegisterWindowMessage("BE_FOPAUSE");
    uint WM_FOSTOP = WindowsAPI.RegisterWindowMessage("BE_FOSTOP");

    public Form1() {
      
      InitializeComponent();
      try
      {
          WindowsHelper.WindowsAPI.CHANGEFILTERSTRUCT filterStatus = new WindowsHelper.WindowsAPI.CHANGEFILTERSTRUCT();
          filterStatus.size = (uint)Marshal.SizeOf(filterStatus);
          filterStatus.info = 0;
          WindowsAPI.ChangeWindowMessageFilterEx(Handle, 0x4A, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
          WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOWINC, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
          WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOBEGIN, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
          WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOEND, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
          WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOPAUSE, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
          WindowsAPI.ChangeWindowMessageFilterEx(Handle, WM_FOSTOP, WindowsAPI.ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
      }
      catch (Exception)
      {
          Close();
      }
      _block = new ManualResetEvent(false);
      _block2 = new ManualResetEvent(false);
      
      SourceItemsCollection = new List<Tuple<string, string>>();
      try {
        SourceHandle = Guid.Parse(Environment.GetCommandLineArgs().Where(c => c.StartsWith("ID:")).Single().Substring(3));
      } catch (Exception) {

      }

      Text = String.Format("FO{0}", SourceHandle);
      MessageReceiverHandle = WindowsAPI.FindWindow(null, "FOMR" + SourceHandle);
      label1.Text = MessageReceiverHandle.ToString();
      WindowsAPI.SendMessage(MessageReceiverHandle, WM_FOWINC, IntPtr.Zero, IntPtr.Zero);
    }
    long OldBytes = 0;
    CopyFileCallbackAction CopyCallback(ShellObject src, String dst, object state, long totalFileSize, long totalBytesTransferred) {
      _block.WaitOne();
      
      if (totalBytesTransferred > 0) {
          byte[] data = System.Text.Encoding.Unicode.GetBytes(totalBytesTransferred.ToString() + "|" + totalFileSize.ToString());
          WindowsAPI.SendStringMessage(MessageReceiverHandle, data, 0, data.Length);
          OldBytes = totalBytesTransferred;
      } 

      if (Cancel)
        return CopyFileCallbackAction.Cancel;

      return CopyFileCallbackAction.Continue;
    }

    void CopyFiles() {
      CurrentStatus = 1;
      _block.WaitOne();
      foreach (var item in SourceItemsCollection) {
        OldBytes = 0;
        if (this.OPType == OperationType.Copy) {
          if (!CustomFileOperations.CopyFile(ShellObject.FromParsingName(item.Item1), item.Item2, CopyFileOptions.None, CopyCallback)) {
            int error = Marshal.GetLastWin32Error();
            if (error == 1225 || error == 1235) {
              Cancel = true;
              CopyThread.Abort();
              Environment.Exit(5);
              break;
            }
          }
        }
      }
    }

    protected override void WndProc(ref Message m) {

      if (m.Msg == WindowsAPI.WM_COPYDATA){
        byte[] b = new Byte[Marshal.ReadInt32(m.LParam, IntPtr.Size)];
        IntPtr dataPtr = Marshal.ReadIntPtr(m.LParam, IntPtr.Size * 2);
        Marshal.Copy(dataPtr, b, 0, b.Length);
        string newMessage = System.Text.Encoding.Unicode.GetString(b);
          if (newMessage.StartsWith("END FO INIT|COPY")) {
            this.OPType = OperationType.Copy;
          }
          if (newMessage.StartsWith("INPUT|")) {
            var parts = newMessage.Replace("INPUT|", "").Split(Char.Parse("|"));
            SourceItemsCollection.Add(new Tuple<string, string>(parts[0].Trim(), parts[1].Trim()));
           
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
