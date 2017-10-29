namespace wyDay.Controls
{
    partial class frmError
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
            this.richError = new wyDay.Controls.RichTextBoxEx();
            this.btnTryAgainLater = new System.Windows.Forms.Button();
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
            // richError
            // 
            this.richError.BackColor = System.Drawing.SystemColors.Window;
            this.richError.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richError.Location = new System.Drawing.Point(12, 27);
            this.richError.Name = "richError";
            this.richError.ReadOnly = true;
            this.richError.Size = new System.Drawing.Size(315, 184);
            this.richError.TabIndex = 1;
            this.richError.Text = "";
            // 
            // btnTryAgainLater
            // 
            this.btnTryAgainLater.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTryAgainLater.AutoSize = true;
            this.btnTryAgainLater.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnTryAgainLater.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnTryAgainLater.Location = new System.Drawing.Point(161, 228);
            this.btnTryAgainLater.Name = "btnTryAgainLater";
            this.btnTryAgainLater.Size = new System.Drawing.Size(92, 22);
            this.btnTryAgainLater.TabIndex = 5;
            this.btnTryAgainLater.UseVisualStyleBackColor = true;
            this.btnTryAgainLater.Click += new System.EventHandler(this.btnTryAgainLater_Click);
            // 
            // frmError
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(339, 262);
            this.Controls.Add(this.btnTryAgainLater);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.richError);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmError";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private wyDay.Controls.RichTextBoxEx richError;
        private System.Windows.Forms.Button btnOK;
        private wyDay.Controls.MLLabel lblTitle;
        private System.Windows.Forms.Button btnTryAgainLater;

    }
}