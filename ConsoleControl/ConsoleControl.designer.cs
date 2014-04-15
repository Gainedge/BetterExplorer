namespace ConsoleControl
{
  partial class ConsoleControl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.components = new System.ComponentModel.Container();
			this.richTextBoxConsole = new System.Windows.Forms.RichTextBox();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.btnCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.btnPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// richTextBoxConsole
			// 
			this.richTextBoxConsole.AcceptsTab = true;
			this.richTextBoxConsole.BackColor = System.Drawing.Color.Black;
			this.richTextBoxConsole.ContextMenuStrip = this.contextMenuStrip1;
			this.richTextBoxConsole.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxConsole.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.richTextBoxConsole.ForeColor = System.Drawing.Color.White;
			this.richTextBoxConsole.Location = new System.Drawing.Point(0, 0);
			this.richTextBoxConsole.Name = "richTextBoxConsole";
			this.richTextBoxConsole.ReadOnly = true;
			this.richTextBoxConsole.Size = new System.Drawing.Size(150, 150);
			this.richTextBoxConsole.TabIndex = 0;
			this.richTextBoxConsole.Text = "";
			this.richTextBoxConsole.MouseClick += new System.Windows.Forms.MouseEventHandler(this.richTextBoxConsole_MouseClick);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnCopy,
            this.btnPaste});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(153, 70);
			this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
			this.contextMenuStrip1.Opened += new System.EventHandler(this.contextMenuStrip1_Opened);
			// 
			// btnCopy
			// 
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new System.Drawing.Size(152, 22);
			this.btnCopy.Text = "Copy";
			// 
			// btnPaste
			// 
			this.btnPaste.Name = "btnPaste";
			this.btnPaste.Size = new System.Drawing.Size(152, 22);
			this.btnPaste.Text = "Paste";
			// 
			// ConsoleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.richTextBoxConsole);
			this.Name = "ConsoleControl";
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

    }

    #endregion

	private System.Windows.Forms.RichTextBox richTextBoxConsole;
	private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
	private System.Windows.Forms.ToolStripMenuItem btnCopy;
	private System.Windows.Forms.ToolStripMenuItem btnPaste;
  }
}
