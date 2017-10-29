namespace wyDay.Controls
{
    partial class frmChanges
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
            this.btnOK = new System.Windows.Forms.Button();
            this.lblTitle = new wyDay.Controls.MLLabel();
            this.richChanges = new wyDay.Controls.RichTextBoxEx();
            this.btnUpdateNow = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSize = true;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(259, 228);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(68, 22);
            this.btnOK.TabIndex = 2;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 13);
            this.lblTitle.TabIndex = 3;
            // 
            // richChanges
            // 
            this.richChanges.BackColor = System.Drawing.SystemColors.Window;
            this.richChanges.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richChanges.Location = new System.Drawing.Point(12, 27);
            this.richChanges.Name = "richChanges";
            this.richChanges.ReadOnly = true;
            this.richChanges.Size = new System.Drawing.Size(315, 184);
            this.richChanges.TabIndex = 1;
            this.richChanges.Text = "";
            // 
            // btnUpdateNow
            // 
            this.btnUpdateNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdateNow.AutoSize = true;
            this.btnUpdateNow.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnUpdateNow.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnUpdateNow.Location = new System.Drawing.Point(161, 228);
            this.btnUpdateNow.Name = "btnUpdateNow";
            this.btnUpdateNow.Size = new System.Drawing.Size(92, 22);
            this.btnUpdateNow.TabIndex = 4;
            this.btnUpdateNow.UseVisualStyleBackColor = true;
            this.btnUpdateNow.Click += new System.EventHandler(this.btnUpdateNow_Click);
            // 
            // frmChanges
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(339, 262);
            this.Controls.Add(this.btnUpdateNow);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.richChanges);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmChanges";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private wyDay.Controls.RichTextBoxEx richChanges;
        private System.Windows.Forms.Button btnOK;
        private wyDay.Controls.MLLabel lblTitle;
        private System.Windows.Forms.Button btnUpdateNow;

    }
}