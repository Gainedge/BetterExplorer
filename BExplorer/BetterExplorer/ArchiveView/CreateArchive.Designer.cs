namespace BetterExplorer
{
    partial class CreateArchive
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbtn_extract = new System.Windows.Forms.RadioButton();
            this.rbtn_compress = new System.Windows.Forms.RadioButton();
            this.gb_format = new System.Windows.Forms.GroupBox();
            this.gb_compressionlevel = new System.Windows.Forms.GroupBox();
            this.cb_fastCompression = new System.Windows.Forms.CheckBox();
            this.txt_password = new System.Windows.Forms.TextBox();
            this.lbl_password = new System.Windows.Forms.Label();
            this.txt_archivePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_execute = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.lbl_archive = new System.Windows.Forms.Label();
            this.txt_archivename = new System.Windows.Forms.TextBox();
            this.btnPickDir = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbtn_extract);
            this.groupBox1.Controls.Add(this.rbtn_compress);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 45);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // rbtn_extract
            // 
            this.rbtn_extract.AutoSize = true;
            this.rbtn_extract.Location = new System.Drawing.Point(150, 19);
            this.rbtn_extract.Name = "rbtn_extract";
            this.rbtn_extract.Size = new System.Drawing.Size(58, 17);
            this.rbtn_extract.TabIndex = 1;
            this.rbtn_extract.TabStop = true;
            this.rbtn_extract.Text = "Extract";
            this.rbtn_extract.UseVisualStyleBackColor = true;
            this.rbtn_extract.CheckedChanged += new System.EventHandler(this.rbtn_extract_CheckedChanged);
            // 
            // rbtn_compress
            // 
            this.rbtn_compress.AutoSize = true;
            this.rbtn_compress.Location = new System.Drawing.Point(22, 19);
            this.rbtn_compress.Name = "rbtn_compress";
            this.rbtn_compress.Size = new System.Drawing.Size(71, 17);
            this.rbtn_compress.TabIndex = 0;
            this.rbtn_compress.TabStop = true;
            this.rbtn_compress.Text = "Compress";
            this.rbtn_compress.UseVisualStyleBackColor = true;
            this.rbtn_compress.CheckedChanged += new System.EventHandler(this.rbtn_compress_CheckedChanged);
            // 
            // gb_format
            // 
            this.gb_format.Location = new System.Drawing.Point(11, 114);
            this.gb_format.Name = "gb_format";
            this.gb_format.Size = new System.Drawing.Size(123, 163);
            this.gb_format.TabIndex = 1;
            this.gb_format.TabStop = false;
            this.gb_format.Text = "Compression format";
            // 
            // gb_compressionlevel
            // 
            this.gb_compressionlevel.Location = new System.Drawing.Point(141, 114);
            this.gb_compressionlevel.Name = "gb_compressionlevel";
            this.gb_compressionlevel.Size = new System.Drawing.Size(129, 163);
            this.gb_compressionlevel.TabIndex = 2;
            this.gb_compressionlevel.TabStop = false;
            this.gb_compressionlevel.Text = "Compression Level";
            // 
            // cb_fastCompression
            // 
            this.cb_fastCompression.AutoSize = true;
            this.cb_fastCompression.Location = new System.Drawing.Point(24, 283);
            this.cb_fastCompression.Name = "cb_fastCompression";
            this.cb_fastCompression.Size = new System.Drawing.Size(109, 17);
            this.cb_fastCompression.TabIndex = 3;
            this.cb_fastCompression.Text = "Fast Compression";
            this.cb_fastCompression.UseVisualStyleBackColor = true;
            // 
            // txt_password
            // 
            this.txt_password.Location = new System.Drawing.Point(141, 303);
            this.txt_password.Name = "txt_password";
            this.txt_password.Size = new System.Drawing.Size(129, 20);
            this.txt_password.TabIndex = 4;
            // 
            // lbl_password
            // 
            this.lbl_password.AutoSize = true;
            this.lbl_password.Location = new System.Drawing.Point(12, 306);
            this.lbl_password.Name = "lbl_password";
            this.lbl_password.Size = new System.Drawing.Size(130, 13);
            this.lbl_password.TabIndex = 5;
            this.lbl_password.Text = "Password (only if required)";
            // 
            // txt_archivePath
            // 
            this.txt_archivePath.Location = new System.Drawing.Point(96, 66);
            this.txt_archivePath.Name = "txt_archivePath";
            this.txt_archivePath.Size = new System.Drawing.Size(142, 20);
            this.txt_archivePath.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Archive path:";
            // 
            // btn_execute
            // 
            this.btn_execute.Location = new System.Drawing.Point(191, 329);
            this.btn_execute.Name = "btn_execute";
            this.btn_execute.Size = new System.Drawing.Size(75, 23);
            this.btn_execute.TabIndex = 8;
            this.btn_execute.Text = "Compress";
            this.btn_execute.UseVisualStyleBackColor = true;
            this.btn_execute.Click += new System.EventHandler(this.Archive);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Location = new System.Drawing.Point(110, 329);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 9;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // lbl_archive
            // 
            this.lbl_archive.AutoSize = true;
            this.lbl_archive.Location = new System.Drawing.Point(12, 95);
            this.lbl_archive.Name = "lbl_archive";
            this.lbl_archive.Size = new System.Drawing.Size(75, 13);
            this.lbl_archive.TabIndex = 10;
            this.lbl_archive.Text = "Archive name:";
            // 
            // txt_archivename
            // 
            this.txt_archivename.Location = new System.Drawing.Point(96, 92);
            this.txt_archivename.Name = "txt_archivename";
            this.txt_archivename.Size = new System.Drawing.Size(170, 20);
            this.txt_archivename.TabIndex = 11;
            // 
            // btnPickDir
            // 
            this.btnPickDir.Location = new System.Drawing.Point(243, 64);
            this.btnPickDir.Name = "btnPickDir";
            this.btnPickDir.Size = new System.Drawing.Size(25, 23);
            this.btnPickDir.TabIndex = 12;
            this.btnPickDir.Text = "...";
            this.btnPickDir.UseVisualStyleBackColor = true;
            this.btnPickDir.Click += new System.EventHandler(this.btnPickDir_Click);
            // 
            // CreateArchive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 362);
            this.Controls.Add(this.btnPickDir);
            this.Controls.Add(this.txt_archivename);
            this.Controls.Add(this.lbl_archive);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_execute);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_archivePath);
            this.Controls.Add(this.lbl_password);
            this.Controls.Add(this.txt_password);
            this.Controls.Add(this.cb_fastCompression);
            this.Controls.Add(this.gb_compressionlevel);
            this.Controls.Add(this.gb_format);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CreateArchive";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Archive";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CreateArchive_FormClosed);
            this.Load += new System.EventHandler(this.CreateArchive_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbtn_extract;
        private System.Windows.Forms.RadioButton rbtn_compress;
        private System.Windows.Forms.GroupBox gb_format;
        private System.Windows.Forms.GroupBox gb_compressionlevel;
        private System.Windows.Forms.CheckBox cb_fastCompression;
        private System.Windows.Forms.TextBox txt_password;
        private System.Windows.Forms.Label lbl_password;
        private System.Windows.Forms.TextBox txt_archivePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_execute;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Label lbl_archive;
        private System.Windows.Forms.TextBox txt_archivename;
        private System.Windows.Forms.Button btnPickDir;
    }
}