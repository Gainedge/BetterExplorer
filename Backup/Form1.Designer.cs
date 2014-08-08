namespace Taskbar_AppId
{
	partial class Form1
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.AppIdTextBox = new System.Windows.Forms.TextBox();
			this.SetAppIdButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(73, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Application ID";
			// 
			// AppIdTextBox
			// 
			this.AppIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.AppIdTextBox.Location = new System.Drawing.Point(103, 6);
			this.AppIdTextBox.Name = "AppIdTextBox";
			this.AppIdTextBox.Size = new System.Drawing.Size(169, 20);
			this.AppIdTextBox.TabIndex = 1;
			// 
			// SetAppIdButton
			// 
			this.SetAppIdButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SetAppIdButton.Location = new System.Drawing.Point(124, 33);
			this.SetAppIdButton.Name = "SetAppIdButton";
			this.SetAppIdButton.Size = new System.Drawing.Size(148, 23);
			this.SetAppIdButton.TabIndex = 2;
			this.SetAppIdButton.Text = "Set AppId";
			this.SetAppIdButton.UseVisualStyleBackColor = true;
			this.SetAppIdButton.Click += new System.EventHandler(this.SetAppIdButton_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 68);
			this.Controls.Add(this.SetAppIdButton);
			this.Controls.Add(this.AppIdTextBox);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "Windows 7 taskbar";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox AppIdTextBox;
		private System.Windows.Forms.Button SetAppIdButton;
	}
}

