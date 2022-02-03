using BExplorer.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BExplorer.Shell.Interop;
using ProcessInterface;
using ShellControls.ShellListView;

namespace ConsoleControl {

  /// <summary> The console event handler is used for console events. </summary>
  /// <param name="sender"> The sender. </param>
  /// <param name="args">  
  /// The <see cref="ConsoleControl.ConsoleEventArgs" /> instance containing the event data.
  /// </param>
  public delegate void ConsoleEventHanlder(object sender, Tuple<string> args);

  /// <summary> The Console Control allows you to embed a basic console in your application. </summary>
  [ToolboxBitmap(typeof(resfinder), "ConsoleControl.ConsoleControl.bmp")]
  public partial class ConsoleControl : UserControl {

    #region Properties

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

    private const int WM_VSCROLL = 0x115;
    private const int SB_PAGEBOTTOM = 0x7;

    /// <summary> Current position that input starts at. </summary>
    private int inputStart = -1;

    private bool _ShouldClear = true;

    private bool _IsCodepageSet = false;

    public ShellView ShellListView { get; set; }

    /// <summary>
    /// The last input string (used so that we can make sure we don't echo input twice).
    /// </summary>
    private string lastInput;

    /// <summary> The key mappings. </summary>
    private List<KeyMapping> keyMappings = new List<KeyMapping>();

    /// <summary> Occurs when console output is produced. </summary>
    private event ConsoleEventHanlder OnConsoleOutput;

    /// <summary> Occurs when console input is produced. </summary>
    public event ConsoleEventHanlder OnConsoleInput;

    /// <summary> The internal process interface used to interface with the process. </summary>
    private ProcessInterface.ProcessInterface processInterace = new ProcessInterface.ProcessInterface();

    protected override CreateParams CreateParams {
      get {
        CreateParams cp = base.CreateParams;
        cp.ExStyle |= 0x02000000;
        return cp;
      }
    }

    /// <summary> Gets or sets a value indicating whether [send keyboard commands to process]. </summary>
    /// <value> <c>true</c> if [send keyboard commands to process]; otherwise, <c>false</c>. </value>
    [Category("Console Control"), Description("If true, special keyboard commands like Ctrl-C and tab are sent to the process.")]
    private bool SendKeyboardCommandsToProcess { get; set; }

    /// <summary> Gets a value indicating whether this instance is process running. </summary>
    /// <value> <c>true</c> if this instance is process running; otherwise, <c>false</c>. </value>
    [Browsable(false)]
    public bool IsProcessRunning => processInterace.IsProcessRunning;

    /// <summary> Gets or sets a value indicating whether to show diagnostics. </summary>
    /// <value> <c>true</c> if show diagnostics; otherwise, <c>false</c>. </value>
    [Category("Console Control"), Description("Show diagnostic information, such as exceptions.")]
    public bool ShowDiagnostics { get; set; }

    public String LastLinePath { get; set; }




    #endregion Properties

    #region Control Events

    private void btnPaste_Click(object sender, EventArgs e) {
      this.richTextBoxConsole.AppendText(System.Windows.Forms.Clipboard.GetText());
    }

    private void btnCopy_Click(object sender, EventArgs e) => Clipboard.SetText(richTextBoxConsole.SelectedText);

    private void btnClear_Click(object sender, EventArgs e) {
      this._ShouldClear = true;
      ClearOutput();
    }

    private void richTextBoxConsole_TextChanged(object sender, EventArgs e) { }// => SendMessage(richTextBoxConsole.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);

    #endregion Control Events

    #region Processing

    //Boolean _IsPowerShell = false;


    /// <summary> Runs a process. </summary>
    /// <param name="fileName">  Name of the file. </param>
    /// <param name="arguments"> The arguments. </param>
    public void StartPowerShell() {
      if (IsProcessRunning) processInterace.StopProcess();

      //_IsPowerShell = true;


      processInterace.StartProcess("powershell.exe", "-NoLogo -NonInteractive -WindowStyle Hidden -ExecutionPolicy Unrestricted");
      Invoke((Action)(() => {
        richTextBoxConsole.ReadOnly = false;
      }));

      //ClearOutput();

      //// Are we showing diagnostics?
      //if (ShowDiagnostics) {
      //	WriteOutput("Preparing to run " + fileName, Color.FromArgb(255, 0, 255, 0));
      //	if (!string.IsNullOrEmpty(arguments))
      //		WriteOutput(" with arguments " + arguments + "." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
      //	else
      //		WriteOutput("." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
      //}

      //// Start the process.
      //this._cmdhandle = processInterace.StartProcess(fileName, arguments);

      //// If we enable input, make the control not read only.
      //if (IsInputEnabled)
      //	richTextBoxConsole.ReadOnly = false;
    }


    /// <summary> Stops the process. </summary>
    public void StopProcess() {
      // Stop the interface.
      processInterace.StopProcess();
      this._IsCodepageSet = false;
    }



    /// <summary> Handles the OnProcessError event of the processInterace control. </summary>
    /// <param name="sender"> The source of the event. </param>
    /// <param name="args">  
    /// The <see cref="ProcessEventArgs" /> instance containing the event data.
    /// </param>
    private void processInterace_OnProcessError(object sender, ProcessInterface.ProcessEventArgs args) {
      // Write the output, in red
      WriteOutput(args.Content, Color.Red);

      // Fire the output event.
      FireConsoleOutputEvent(args.Content);
    }

    /// <summary> Handles the OnProcessOutput event of the processInterace control. </summary>
    /// <param name="sender"> The source of the event. </param>
    /// <param name="args">  
    /// The <see cref="ProcessEventArgs" /> instance containing the event data.
    /// </param>
    private void processInterace_OnProcessOutput(object sender, ProcessInterface.ProcessEventArgs args) {
      // Write the output, in white
      this.richTextBoxConsole.SuspendLayout();
      WriteOutput(args.Content, Color.White);

      // Fire the output event.
      FireConsoleOutputEvent(args.Content);
      this.richTextBoxConsole.ResumeLayout();
    }

    /// <summary> Handles the OnProcessInput event of the processInterace control. </summary>
    /// <param name="sender"> The source of the event. </param>
    /// <param name="args">  
    /// The <see cref="ProcessEventArgs" /> instance containing the event data.
    /// </param>
    private void processInterace_OnProcessInput(object sender, ProcessInterface.ProcessEventArgs args) {
      throw new NotImplementedException();
    }

    /// <summary> Handles the OnProcessExit event of the processInterace control. </summary>
    /// <param name="sender"> The source of the event. </param>
    /// <param name="args">  
    /// The <see cref="ProcessEventArgs" /> instance containing the event data.
    /// </param>
    private void processInterace_OnProcessExit(object sender, ProcessInterface.ProcessEventArgs args) {
      // Are we showing diagnostics?
      if (ShowDiagnostics) {
        WriteOutput(System.Environment.NewLine + processInterace.ProcessFileName + " exited.", Color.FromArgb(255, 0, 255, 0));
      }

      // Read only again.
      Invoke((Action)(() => {
        richTextBoxConsole.ReadOnly = true;
      }));
    }

    /// <summary> Fires the console output event. </summary>
    /// <param name="content"> The content. </param>
    private void FireConsoleOutputEvent(string content) => OnConsoleOutput?.Invoke(this, new Tuple<string>(content));

    /// <summary> Fires the console input event. </summary>
    /// <param name="content"> The content. </param>
    private void FireConsoleInputEvent(string content) => OnConsoleInput?.Invoke(this, new Tuple<string>(content));

    #endregion Processing

    #region Writing
    public void ClearConsole() {
      this._ShouldClear = true;
      ClearOutput();
    }
    private void ClearOutput(bool isSendClear = true, bool isClearAfterEnter = true) {
      if (isClearAfterEnter) {
        if (isSendClear) WriteInput("cls", Color.Black, false);//Clear the real console screen
        richTextBoxConsole.SelectedText = "";
        richTextBoxConsole.Clear();
        inputStart = 0;
      }
    }

    /// <summary> Writes the output to the console control. </summary>
    /// <param name="output"> The output. </param>
    /// <param name="color">  The color. </param>
    private void WriteOutput(string output, Color color) {
      //if (string.IsNullOrEmpty(lastInput) == false && (output == lastInput || output.Replace("\r\n", "") == lastInput))
      if (!string.IsNullOrEmpty(lastInput) && (output == lastInput || output.Replace("\r\n", "") == lastInput))
        return;
      ClearOutput(false, this._ShouldClear);
      // Write the output.
      richTextBoxConsole.SelectionColor = color;
      richTextBoxConsole.SelectedText += output;
      inputStart = richTextBoxConsole.SelectionStart;
    }

    /// <summary> Writes the input to the console control. </summary>
    /// <param name="input"> The input. </param>
    /// <param name="color"> The color. </param>
    /// <param name="isRaiseEvent"> Rise input event or not </param>
    private void WriteInput(string input, Color color, Boolean isRaiseEvent = true) {
      //Invoke((Action)(() => {
      lastInput = input;

      // Write the input.
      processInterace.WriteInput(input);

      // Fire the event.
      if (isRaiseEvent)
        FireConsoleInputEvent(input);
      //}));
    }

    public void EnqueleInput(string input) {
      inputStart = richTextBoxConsole.SelectionStart;
      richTextBoxConsole.SelectionColor = Color.LightBlue;
      richTextBoxConsole.SelectedText += input;

    }

    public void ChangeFolder(string Folder, bool IsFileSystem) {
      string Value = null;

      richTextBoxConsole.ReadOnly = false;


      if (!IsProcessRunning)
        /*this._cmdhandle =*/
        processInterace.StartProcess("cmd.exe", null);

      this._ShouldClear = true;
      ClearOutput();

      if (IsFileSystem)
        Value = $"cd /D \"{Folder}\"";
      if (!this._IsCodepageSet) {
        //Enable UTF-8 for the ConsoleControl
        WriteInput("chcp 65001", Color.Wheat, false);
        this._IsCodepageSet = true;
      }

      WriteInput(Value, Color.Wheat, false);
      this.LastLinePath = Folder;
    }

    #endregion Writing

    /// <summary> Initializes a new instance of the <see cref="ConsoleControl" /> class. </summary>
    public ConsoleControl() {
      // Initialize the component.
      InitializeComponent();

      this.DoubleBuffered = true;
      this.SetStyle(ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.ContainerControl |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.SupportsTransparentBackColor,
                    true);

      // Show diagnostics disabled by default.
      ShowDiagnostics = false;

      // Input enabled by default.
      //IsInputEnabled = true;

      // Disable special commands by default.
      SendKeyboardCommandsToProcess = true;
      this.richTextBoxConsole.HideSelection = false;
      // Initialize the keymappings.
      InitialiseKeyMappings();

      // Handle process events.
      processInterace.OnProcessOutput += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessOutput);
      processInterace.OnProcessError += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessError);
      processInterace.OnProcessInput += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessInput);
      processInterace.OnProcessExit += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessExit);

      // Wait for key down messages on the rich text box.
      richTextBoxConsole.KeyDown += new KeyEventHandler(richTextBoxConsole_KeyDown);
      richTextBoxConsole.MouseUp += richTextBoxConsole_MouseDown;
    }

    void richTextBoxConsole_MouseDown(object sender, MouseEventArgs e) {
      //this.ShellListView.IsFocusAllowed = false;
      richTextBoxConsole.Focus();
    }

    /// <summary> Initializes the key mappings. </summary>
    private void InitialiseKeyMappings() {
      // Map 'tab'.
      keyMappings.Add(new KeyMapping(false, false, false, Keys.Tab, "{TAB}", "\t"));

      // Map 'Ctrl-C'.
      keyMappings.Add(new KeyMapping(true, false, false, Keys.C, "^(c)", "\x03\r\n"));
    }

    [DllImport("User32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);


    private void SendCtrlC(IntPtr hWnd) {
      const uint keyeventfKeyup = 2;
      const byte vkControl = 0x11;
      //hWnd == handle to console window
      //set it to foreground or u can not send commands
      SetForegroundWindow(hWnd);
      //sending keyboard event Ctrl+C
      keybd_event(vkControl, 0, 0, 0);
      keybd_event(0x43, 0, 0, 0);
      keybd_event(0x43, 0, keyeventfKeyup, 0);
      keybd_event(vkControl, 0, keyeventfKeyup, 0);
    }

    private void SendTab(IntPtr hWnd) {
      User32.PostMessage(this.processInterace.Process.MainWindowHandle, 0x0100, (IntPtr)0x09, IntPtr.Zero);
      User32.PostMessage(this.processInterace.Process.MainWindowHandle, 0x0101, (IntPtr)0x09, IntPtr.Zero);
    }

    /// <summary> Handles the KeyDown event of the richTextBoxConsole control. </summary>
    /// <param name="sender"> The source of the event. </param>
    /// <param name="e">     
    /// The <see cref="System.Windows.Forms.KeyEventArgs" /> instance containing the event data.
    /// </param>
    private void richTextBoxConsole_KeyDown(object sender, KeyEventArgs e) {
      // Are we sending keyboard commands to the process?
      if (SendKeyboardCommandsToProcess && IsProcessRunning) {
        richTextBoxConsole.SelectionColor = Color.White;
        // Get key mappings for this key event?
        var mappings = from k in keyMappings
                       where
                       k.KeyCode == e.KeyCode &&
                       k.IsAltPressed == e.Alt &&
                       k.IsControlPressed == e.Control &&
                       k.IsShiftPressed == e.Shift
                       select k;


        if (e.KeyCode == Keys.Tab) {
          SendTab(processInterace.Process.MainWindowHandle);
          //	SendKeysEx.SendKeys(this._cmdhandle, "{TAB}");
        }
        //// Go through each mapping, send the message.
        //foreach (var mapping in mappings) { //TODO: Find out if we need this [For Each]
        //	//SendKeysEx.SendKeys(this._cmdhandle, mapping.SendKeysMapping);
        //	//WriteInput(mapping.StreamMapping, Color.Yellow);
        //	//WriteInput("\x3", Color.White);
        //}

        // If we handled a mapping, we're done here.
        if (mappings.Any()) {
          e.SuppressKeyPress = true;
          return;
        }
      }

      // If we're at the input point and it's backspace, bail.
      if ((richTextBoxConsole.SelectionStart <= inputStart) && e.KeyCode == Keys.Back) e.SuppressKeyPress = true;

      // Are we in the read-only zone?
      if (richTextBoxConsole.SelectionStart < inputStart) {
        // Allow arrows and Ctrl-C.
        if (!(e.KeyCode == Keys.Left ||
            e.KeyCode == Keys.Right ||
            e.KeyCode == Keys.Up ||
            e.KeyCode == Keys.Down ||
            (e.KeyCode == Keys.C && e.Control))) {
          e.SuppressKeyPress = true;
        }

        if (e.KeyData == (Keys.C | Keys.ControlKey))
          MessageBox.Show("CTRL");
        //if (e.KeyCode == Keys.C)
        //{
        //    MessageBox.Show(richTextBoxConsole.SelectedText);
        //    Clipboard.SetText(richTextBoxConsole.SelectedText);
        //}
      }

      // Is it the return key?
      if (e.KeyCode == Keys.Return) {
        this._ShouldClear = false;
        // Get the input.
        string input = richTextBoxConsole.Text.Substring(inputStart, richTextBoxConsole.SelectionStart - inputStart);
        WriteInput(input, Color.White);
      }
    }

    protected override void OnResize(EventArgs e) {
      this.Invalidate();
      base.OnResize(e);
    }

    protected override void OnSizeChanged(EventArgs e) {
      this.Invalidate();
      base.OnSizeChanged(e);
    }
  }
}