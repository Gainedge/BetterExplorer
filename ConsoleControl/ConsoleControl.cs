using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ConsoleControl {

	/// <summary> The console event handler is used for console events. </summary>
	/// <param name="sender"> The sender. </param>
	/// <param name="args">  
	/// The <see cref="ConsoleControl.ConsoleEventArgs" /> instance containing the event data.
	/// </param>
	public delegate void ConsoleEventHanlder(object sender, ConsoleEventArgs args);

	/// <summary> The Console Control allows you to embed a basic console in your application. </summary>
	[ToolboxBitmap(typeof(resfinder), "ConsoleControl.ConsoleControl.bmp")]
	public partial class ConsoleControl : UserControl {
		/*
		 * Fix: fix the console not working with different root directories
		 * [C:\] is okay but [D:\] will not work
		 * There should be some code already for this
		 * 
		 * Fix the Info Panel
		 * Fix the Preview Panel
		 */

		//finish



		#region Properties

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

		private const int WM_VSCROLL = 277;
		private const int SB_PAGEBOTTOM = 7;

		/// <summary> The internal process interface used to interface with the process. </summary>
		private ProcessInterface.ProcessInterface processInterace = new ProcessInterface.ProcessInterface();

		/// <summary> Current position that input starts at. </summary>
		private int inputStart = -1;

		/// <summary> The is input enabled flag. </summary>
		private bool isInputEnabled = true;

		/// <summary>
		/// The last input string (used so that we can make sure we don't echo input twice).
		/// </summary>
		private string lastInput;

		/// <summary> Gets or sets a value indicating whether this instance is input enabled. </summary>
		/// <value> <c>true</c> if this instance is input enabled; otherwise, <c>false</c>. </value>
		[Category("Console Control"), Description("If true, the user can key in input.")]
		private bool IsInputEnabled {
			get { return isInputEnabled; }
			set {
				isInputEnabled = value;
				if (IsProcessRunning)
					richTextBoxConsole.ReadOnly = !value;
			}
		}

		/// <summary> The key mappings. </summary>
		private List<KeyMapping> keyMappings = new List<KeyMapping>();

		/// <summary> Occurs when console output is produced. </summary>
		private event ConsoleEventHanlder OnConsoleOutput;

		/// <summary> Occurs when console input is produced. </summary>
		private event ConsoleEventHanlder OnConsoleInput;

		/// <summary> Gets the process interface. </summary>
		[Browsable(false)]
		private ProcessInterface.ProcessInterface Process_Interface { get { return processInterace; } }

		/// <summary> Gets the key mappings. </summary>
		[Browsable(false)]
		private List<KeyMapping> KeyMappings { get { return keyMappings; } }

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
		public bool IsProcessRunning {
			get { return processInterace.IsProcessRunning; }
		}

		/// <summary> Gets or sets a value indicating whether to show diagnostics. </summary>
		/// <value> <c>true</c> if show diagnostics; otherwise, <c>false</c>. </value>
		[Category("Console Control"), Description("Show diagnostic information, such as exceptions.")]
		public bool ShowDiagnostics { get; set; }




		#endregion Properties

		#region Control Events

		private void btnPaste_Click(object sender, EventArgs e) {
			MessageBox.Show("this is not correct as it does not write to the stream...I think O.o!");
			this.richTextBoxConsole.AppendText(System.Windows.Forms.Clipboard.GetText());
		}

		private void btnCopy_Click(object sender, EventArgs e) {
			System.Windows.Forms.Clipboard.SetText(richTextBoxConsole.SelectedText);
		}

		private void btnClear_Click(object sender, EventArgs e) {
			//richTextBoxConsole.Clear();
			//richTextBoxConsole.Text = lastInput;
			//richTextBoxConsole.Text = NewestFolder + ">";
			MessageBox.Show("This does not work correctly yet");
			ClearOutput();
		}

		private void richTextBoxConsole_TextChanged(object sender, EventArgs e) {
			SendMessage(richTextBoxConsole.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
		}

		#endregion Control Events

		#region Processing

		/// <summary> Runs a process. </summary>
		/// <param name="fileName">  Name of the file. </param>
		/// <param name="arguments"> The arguments. </param>
		[Obsolete("Making Private soon", false)]
		private void StartProcess(string fileName, string arguments) {
			if (!IsProcessRunning) {
				processInterace.StartProcess("cmd.exe", null);
				ClearOutput();
			}

			// Are we showing diagnostics?
			if (ShowDiagnostics) {
				WriteOutput("Preparing to run " + fileName, Color.FromArgb(255, 0, 255, 0));
				if (!string.IsNullOrEmpty(arguments))
					WriteOutput(" with arguments " + arguments + "." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
				else
					WriteOutput("." + Environment.NewLine, Color.FromArgb(255, 0, 255, 0));
			}

			// Start the process.
			processInterace.StartProcess(fileName, arguments);

			// If we enable input, make the control not read only.
			if (IsInputEnabled)
				richTextBoxConsole.ReadOnly = false;
		}

		/// <summary> Stops the process. </summary>
		public void StopProcess() {
			// Stop the interface.
			processInterace.StopProcess();
		}

		/// <summary> Handles the OnProcessError event of the processInterace control. </summary>
		/// <param name="sender"> The source of the event. </param>
		/// <param name="args">  
		/// The <see cref="Process_Interface.ProcessEventArgs" /> instance containing the event data.
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
		/// The <see cref="Process_Interface.ProcessEventArgs" /> instance containing the event data.
		/// </param>
		private void processInterace_OnProcessOutput(object sender, ProcessInterface.ProcessEventArgs args) {
			// Write the output, in white
			WriteOutput(args.Content, Color.White);

			// Fire the output event.
			FireConsoleOutputEvent(args.Content);
		}

		/// <summary> Handles the OnProcessInput event of the processInterace control. </summary>
		/// <param name="sender"> The source of the event. </param>
		/// <param name="args">  
		/// The <see cref="Process_Interface.ProcessEventArgs" /> instance containing the event data.
		/// </param>
		private void processInterace_OnProcessInput(object sender, ProcessInterface.ProcessEventArgs args) {
			throw new NotImplementedException();
		}

		/// <summary> Handles the OnProcessExit event of the processInterace control. </summary>
		/// <param name="sender"> The source of the event. </param>
		/// <param name="args">  
		/// The <see cref="Process_Interface.ProcessEventArgs" /> instance containing the event data.
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
		private void FireConsoleOutputEvent(string content) {
			// Get the event.
			var theEvent = OnConsoleOutput;
			if (theEvent != null)
				theEvent(this, new ConsoleEventArgs(content));
		}

		/// <summary> Fires the console input event. </summary>
		/// <param name="content"> The content. </param>
		private void FireConsoleInputEvent(string content) {
			// Get the event.
			var theEvent = OnConsoleInput;
			if (theEvent != null)
				theEvent(this, new ConsoleEventArgs(content));
		}

		#endregion Processing

		#region Writing

		private void ClearOutput() {
			richTextBoxConsole.Clear();
			inputStart = 0;
		}

		/// <summary> Writes the output to the console control. </summary>
		/// <param name="output"> The output. </param>
		/// <param name="color">  The color. </param>
		private void WriteOutput(string output, Color color) {
			//if (string.IsNullOrEmpty(lastInput) == false && (output == lastInput || output.Replace("\r\n", "") == lastInput))
			if (!string.IsNullOrEmpty(lastInput) && (output == lastInput || output.Replace("\r\n", "") == lastInput))
				return;
			//else if (!richTextBoxConsole.Created)
			//	return;

			//richTextBoxConsole.BeginInvoke((Action)(() => {
			// Write the output.
			richTextBoxConsole.SelectionColor = color;
			richTextBoxConsole.SelectedText += output;
			inputStart = richTextBoxConsole.SelectionStart;
			//}));

			/*
			//Invoke((Action)(() => {
			//  Write the output.
			richTextBoxConsole.SelectionColor = color;
			richTextBoxConsole.SelectedText += output;
			inputStart = richTextBoxConsole.SelectionStart;
			//}));
			*/
		}

		/// <summary> Writes the input to the console control. </summary>
		/// <param name="input"> The input. </param>
		/// <param name="color"> The color. </param>		
		private void WriteInput(string input, Color color) {
			//Invoke((Action)(() => {
			lastInput = input;

			// Write the input.
			processInterace.WriteInput(input);

			// Fire the event.
			FireConsoleInputEvent(input);
			//}));
		}

		public void ChangeFolder(string Folder, bool IsFileSystem) {
			string Value = null;

			richTextBoxConsole.ReadOnly = false;
			ClearOutput();

			if (!IsProcessRunning)
				processInterace.StartProcess("cmd.exe", null);

			if (IsFileSystem)
				Value = String.Format("cd \"{0}\"", Folder);


			//ctrlConsole.InternalRichTextBox.Lines.Last().Substring(0, ctrlConsole.InternalRichTextBox.Lines.Last().IndexOf(Char.Parse(@"\")) + 1) != Path.GetPathRoot(Folder)

			//TODO: Try to reduce the amount of logic here
			if (this.richTextBoxConsole.Text != "") {
				//TODO: Figure out if [this.richTextBoxConsole.Text != ""] will ever be [True] 
				var Path = System.IO.Path.GetPathRoot(Folder);
				var LastLine = richTextBoxConsole.Lines.Last();
				var Question = LastLine.Substring(0, LastLine.IndexOf(Char.Parse(@"\")) + 1);
				var Answer = Question != Path;
				if (Answer) {
					Value = Path.TrimEnd(Char.Parse(@"\"));
				}
			}

			WriteInput(Value, Color.Wheat);

			/*
			return;

			//ClearOutput();
			lastInput_ = Value;

			// Write the input.
			processInterace.WriteInput(Value);

			// Fire the event.
			FireConsoleInputEvent(Value);
			*/
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
			IsInputEnabled = true;

			// Disable special commands by default.
			SendKeyboardCommandsToProcess = false;

			// Initialize the keymappings.
			InitialiseKeyMappings();

			// Handle process events.
			processInterace.OnProcessOutput += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessOutput);
			processInterace.OnProcessError += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessError);
			processInterace.OnProcessInput += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessInput);
			processInterace.OnProcessExit += new ProcessInterface.ProcessEventHanlder(processInterace_OnProcessExit);

			// Wait for key down messages on the rich text box.
			richTextBoxConsole.KeyDown += new KeyEventHandler(richTextBoxConsole_KeyDown);
		}

		/*
		/// <summary> Gets the internal rich text box. </summary>
		[Browsable(false)]
		[Obsolete("Will become private soon", false)]
		public RichTextBox InternalRichTextBox {
			get {
				return richTextBoxConsole;
			}
		}
		*/

		/*
		[Obsolete("Will become private soon", false)]
		public void ScrollToBottom() {
			//SendMessage(richTextBoxConsole.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
		}
		*/

		/// <summary> Initializes the key mappings. </summary>
		private void InitialiseKeyMappings() {
			// Map 'tab'.
			keyMappings.Add(new KeyMapping(false, false, false, Keys.Tab, "{TAB}", "\t"));

			// Map 'Ctrl-C'.
			keyMappings.Add(new KeyMapping(true, false, false, Keys.C, "^(c)", "\x03\r\n"));
		}

		/// <summary> Handles the KeyDown event of the richTextBoxConsole control. </summary>
		/// <param name="sender"> The source of the event. </param>
		/// <param name="e">     
		/// The <see cref="System.Windows.Forms.KeyEventArgs" /> instance containing the event data.
		/// </param>
		private void richTextBoxConsole_KeyDown(object sender, KeyEventArgs e) {
			// Are we sending keyboard commands to the process?
			if (SendKeyboardCommandsToProcess && IsProcessRunning) {
				// Get key mappings for this key event?
				var mappings = from k in keyMappings
							   where
							   k.KeyCode == e.KeyCode &&
							   k.IsAltPressed == e.Alt &&
							   k.IsControlPressed == e.Control &&
							   k.IsShiftPressed == e.Shift
							   select k;



				// Go through each mapping, send the message.
				foreach (var mapping in mappings) { //TODO: Find out if we need this [For Each]
					//SendKeysEx.SendKeys(CurrentProcessHwnd, mapping.SendKeysMapping);
					//inputWriter.WriteLine(mapping.StreamMapping);
					//WriteInput("\x3", Color.White, false);
				}

				// If we handled a mapping, we're done here.
				if (mappings.Count() > 0) {
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
				// Get the input.
				string input = richTextBoxConsole.Text.Substring(inputStart, richTextBoxConsole.SelectionStart - inputStart);
				WriteInput(input, Color.White);

			}
		}

		protected override void OnPaintBackground(PaintEventArgs e) {
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