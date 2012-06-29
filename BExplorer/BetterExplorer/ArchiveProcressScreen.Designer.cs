namespace BetterExplorer
{
    partial class ArchiveProcressScreen
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
            this.pb_compression = new System.Windows.Forms.ProgressBar();
            this.lbl_compressing_to = new System.Windows.Forms.Label();
            this.lbl_commpressing_file = new System.Windows.Forms.Label();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.pb_totaalfiles = new System.Windows.Forms.ProgressBar();
            this.cb_closewhendone = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // pb_compression
            // 
            this.pb_compression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pb_compression.Location = new System.Drawing.Point(10, 57);
            this.pb_compression.Name = "pb_compression";
            this.pb_compression.Size = new System.Drawing.Size(262, 23);
            this.pb_compression.TabIndex = 0;
            // 
            // lbl_compressing_to
            // 
            this.lbl_compressing_to.AutoSize = true;
            this.lbl_compressing_to.Location = new System.Drawing.Point(13, 9);
            this.lbl_compressing_to.Name = "lbl_compressing_to";
            this.lbl_compressing_to.Size = new System.Drawing.Size(106, 13);
            this.lbl_compressing_to.TabIndex = 1;
            this.lbl_compressing_to.Text = "Compressing files to :";
            // 
            // lbl_commpressing_file
            // 
            this.lbl_commpressing_file.AutoSize = true;
            this.lbl_commpressing_file.Location = new System.Drawing.Point(13, 32);
            this.lbl_commpressing_file.Name = "lbl_commpressing_file";
            this.lbl_commpressing_file.Size = new System.Drawing.Size(89, 13);
            this.lbl_commpressing_file.TabIndex = 2;
            this.lbl_commpressing_file.Text = "Compressing file: ";
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.Location = new System.Drawing.Point(197, 117);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 3;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.CancelCompression);
            // 
            // pb_totaalfiles
            // 
            this.pb_totaalfiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pb_totaalfiles.Location = new System.Drawing.Point(10, 87);
            this.pb_totaalfiles.Name = "pb_totaalfiles";
            this.pb_totaalfiles.Size = new System.Drawing.Size(262, 23);
            this.pb_totaalfiles.TabIndex = 4;
            // 
            // cb_closewhendone
            // 
            this.cb_closewhendone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cb_closewhendone.AutoSize = true;
            this.cb_closewhendone.Checked = true;
            this.cb_closewhendone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_closewhendone.Location = new System.Drawing.Point(16, 121);
            this.cb_closewhendone.Name = "cb_closewhendone";
            this.cb_closewhendone.Size = new System.Drawing.Size(108, 17);
            this.cb_closewhendone.TabIndex = 5;
            this.cb_closewhendone.Text = "Close when done";
            this.cb_closewhendone.UseVisualStyleBackColor = true;
            // 
            // ArchiveProcressScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 150);
            this.Controls.Add(this.cb_closewhendone);
            this.Controls.Add(this.pb_totaalfiles);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.lbl_commpressing_file);
            this.Controls.Add(this.lbl_compressing_to);
            this.Controls.Add(this.pb_compression);
            this.MaximizeBox = false;
            this.Name = "ArchiveProcressScreen";
            this.Text = "Performing Action...";
            this.Shown += new System.EventHandler(this.ArchiveProcressScreen_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pb_compression;
        private System.Windows.Forms.Label lbl_compressing_to;
        private System.Windows.Forms.Label lbl_commpressing_file;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.ProgressBar pb_totaalfiles;
        private System.Windows.Forms.CheckBox cb_closewhendone;
    }
}