namespace BExplorer.Shell
{
	partial class ShellView
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (bw != null)
				{
					bw.Dispose();
					bw = null;
				}
				if (ExeFallBack16 != null)
				{
					ExeFallBack16.Dispose();
					ExeFallBack16 = null;
				}
				if (ExeFallBack256 != null)
				{
					ExeFallBack256.Dispose();
					ExeFallBack256 = null;
				}
				if (ExeFallBack32 != null)
				{
					ExeFallBack32.Dispose();
					ExeFallBack32 = null;
				}
				if (ExeFallBack48 != null)
				{
					ExeFallBack48.Dispose();
					ExeFallBack48 = null;
				}
				if (extra != null)
				{
					extra.Dispose();
					extra = null;
				}
				if (jumbo != null)
				{
					jumbo.Dispose();
					jumbo = null;
				}
				if (large != null)
				{
					large.Dispose();
					large = null;
				}
				if (m_CurrentFolder != null)
				{
					m_CurrentFolder.Dispose();
					m_CurrentFolder = null;
				}
				if (selectionTimer != null)
				{
					selectionTimer.Dispose();
					selectionTimer = null;
				}
				if (small != null)
				{
					small.Dispose();
					small = null;
				}
				if (resetEvent != null)
				{
					resetEvent.Dispose();
					resetEvent = null;
				}
				if (_ResetTimer != null)
				{
					_ResetTimer.Dispose();
					_ResetTimer = null;
				}
				if (_KeyJumpTimer != null)
				{
					_KeyJumpTimer.Dispose();
					_KeyJumpTimer = null;
				}
				if (_kpreselitem != null)
				{
					_kpreselitem.Dispose();
					_kpreselitem = null;
				}
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
			this.SuspendLayout();
			// 
			// ShellViewEx
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Name = "ShellViewEx";
			this.Size = new System.Drawing.Size(516, 438);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
