namespace ShellControls.ShellListView {
	partial class ShellView {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
				if (_Bw != null) {
					_Bw.Dispose();
					_Bw = null;
				}
				
				if (CurrentFolder != null) {
					CurrentFolder.Dispose();
					CurrentFolder = null;
				}
				if (_SelectionTimer != null) {
					_SelectionTimer.Dispose();
					_SelectionTimer = null;
				}
				if (_Small != null) {
					_Small.Dispose();
					_Small = null;
				}
				if (_ResetEvent != null) {
					_ResetEvent.Dispose();
					_ResetEvent = null;
				}
				if (_ResetTimer != null) {
					_ResetTimer.Dispose();
					_ResetTimer = null;
				}
				if (_KeyJumpTimer != null) {
					_KeyJumpTimer.Dispose();
					_KeyJumpTimer = null;
				}
				if (_Kpreselitem != null) {
					_Kpreselitem.Dispose();
					_Kpreselitem = null;
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
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
