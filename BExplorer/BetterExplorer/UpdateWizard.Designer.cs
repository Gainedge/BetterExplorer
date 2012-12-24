namespace BetterExplorer
{
    partial class UpdateWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateWizard));
            this.wizardControl1 = new AeroWizard.WizardControl();
            this.pgAvailableUpdates = new AeroWizard.WizardPage();
            this.lvAvailableUpdates = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pgDownload = new AeroWizard.WizardPage();
            this.pbTotalProgress = new System.Windows.Forms.ProgressBar();
            this.pbFileDownload = new System.Windows.Forms.ProgressBar();
            this.lblTotalProgress = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblCurrentDownload = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblDownloadedBytes = new System.Windows.Forms.Label();
            this.pgFinish = new AeroWizard.WizardPage();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).BeginInit();
            this.pgAvailableUpdates.SuspendLayout();
            this.pgDownload.SuspendLayout();
            this.pgFinish.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardControl1
            // 
            this.wizardControl1.Location = new System.Drawing.Point(0, 0);
            this.wizardControl1.Name = "wizardControl1";
            this.wizardControl1.Pages.Add(this.pgAvailableUpdates);
            this.wizardControl1.Pages.Add(this.pgDownload);
            this.wizardControl1.Pages.Add(this.pgFinish);
            this.wizardControl1.Size = new System.Drawing.Size(549, 419);
            this.wizardControl1.TabIndex = 0;
            this.wizardControl1.Title = "Update BetterExplorer";
            this.wizardControl1.TitleIcon = ((System.Drawing.Icon)(resources.GetObject("wizardControl1.TitleIcon")));
            this.wizardControl1.Finished += new System.EventHandler(this.wizardControl1_Finished);
            this.wizardControl1.SelectedPageChanged += new System.EventHandler(this.wizardControl1_SelectedPageChanged);
            // 
            // pgAvailableUpdates
            // 
            this.pgAvailableUpdates.Controls.Add(this.lvAvailableUpdates);
            this.pgAvailableUpdates.Name = "pgAvailableUpdates";
            this.pgAvailableUpdates.NextPage = this.pgDownload;
            this.pgAvailableUpdates.Size = new System.Drawing.Size(502, 265);
            this.pgAvailableUpdates.TabIndex = 0;
            this.pgAvailableUpdates.Text = "Available Updates";
            // 
            // lvAvailableUpdates
            // 
            this.lvAvailableUpdates.CheckBoxes = true;
            this.lvAvailableUpdates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lvAvailableUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvAvailableUpdates.FullRowSelect = true;
            this.lvAvailableUpdates.Location = new System.Drawing.Point(0, 0);
            this.lvAvailableUpdates.Name = "lvAvailableUpdates";
            this.lvAvailableUpdates.Size = new System.Drawing.Size(502, 265);
            this.lvAvailableUpdates.TabIndex = 0;
            this.lvAvailableUpdates.UseCompatibleStateImageBehavior = false;
            this.lvAvailableUpdates.View = System.Windows.Forms.View.Details;
            this.lvAvailableUpdates.SizeChanged += new System.EventHandler(this.lvAvailableUpdates_SizeChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Description";
            this.columnHeader1.Width = 199;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Version";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Type";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Rquired ver.";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader4.Width = 100;
            // 
            // pgDownload
            // 
            this.pgDownload.AllowBack = false;
            this.pgDownload.Controls.Add(this.pbTotalProgress);
            this.pgDownload.Controls.Add(this.pbFileDownload);
            this.pgDownload.Controls.Add(this.lblTotalProgress);
            this.pgDownload.Controls.Add(this.label2);
            this.pgDownload.Controls.Add(this.lblCurrentDownload);
            this.pgDownload.Controls.Add(this.label3);
            this.pgDownload.Controls.Add(this.lblDownloadedBytes);
            this.pgDownload.Name = "pgDownload";
            this.pgDownload.Size = new System.Drawing.Size(502, 265);
            this.pgDownload.TabIndex = 1;
            this.pgDownload.Text = "Downloading update";
            // 
            // pbTotalProgress
            // 
            this.pbTotalProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pbTotalProgress.Location = new System.Drawing.Point(6, 174);
            this.pbTotalProgress.Name = "pbTotalProgress";
            this.pbTotalProgress.Size = new System.Drawing.Size(485, 20);
            this.pbTotalProgress.TabIndex = 4;
            // 
            // pbFileDownload
            // 
            this.pbFileDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pbFileDownload.Location = new System.Drawing.Point(6, 71);
            this.pbFileDownload.Name = "pbFileDownload";
            this.pbFileDownload.Size = new System.Drawing.Size(485, 20);
            this.pbFileDownload.TabIndex = 3;
            // 
            // lblTotalProgress
            // 
            this.lblTotalProgress.AutoSize = true;
            this.lblTotalProgress.Location = new System.Drawing.Point(6, 199);
            this.lblTotalProgress.Name = "lblTotalProgress";
            this.lblTotalProgress.Size = new System.Drawing.Size(144, 15);
            this.lblTotalProgress.TabIndex = 7;
            this.lblTotalProgress.Text = "0 / 0 updates downloaded";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(6, 145);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Total progress";
            // 
            // lblCurrentDownload
            // 
            this.lblCurrentDownload.AutoSize = true;
            this.lblCurrentDownload.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentDownload.Location = new System.Drawing.Point(6, 41);
            this.lblCurrentDownload.Name = "lblCurrentDownload";
            this.lblCurrentDownload.Size = new System.Drawing.Size(90, 15);
            this.lblCurrentDownload.TabIndex = 5;
            this.lblCurrentDownload.Text = "Downloading....";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(168, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "This may take a few moments.";
            // 
            // lblDownloadedBytes
            // 
            this.lblDownloadedBytes.AutoSize = true;
            this.lblDownloadedBytes.Location = new System.Drawing.Point(6, 96);
            this.lblDownloadedBytes.Name = "lblDownloadedBytes";
            this.lblDownloadedBytes.Size = new System.Drawing.Size(166, 15);
            this.lblDownloadedBytes.TabIndex = 2;
            this.lblDownloadedBytes.Text = "0000 / 0000 bytes downloaded";
            // 
            // pgFinish
            // 
            this.pgFinish.AllowBack = false;
            this.pgFinish.Controls.Add(this.label4);
            this.pgFinish.IsFinishPage = true;
            this.pgFinish.Name = "pgFinish";
            this.pgFinish.Size = new System.Drawing.Size(502, 265);
            this.pgFinish.TabIndex = 2;
            this.pgFinish.Text = "Ready to Install";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(401, 45);
            this.label4.TabIndex = 1;
            this.label4.Text = "To install the update, BetterExplorer will need to close.\r\n\r\nClick on Install to " +
    "close BetterExplorer and then begin installing the update.";
            // 
            // UpdateWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 419);
            this.Controls.Add(this.wizardControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "UpdateWizard";
            this.Text = "Update BetterExplorer";
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).EndInit();
            this.pgAvailableUpdates.ResumeLayout(false);
            this.pgDownload.ResumeLayout(false);
            this.pgDownload.PerformLayout();
            this.pgFinish.ResumeLayout(false);
            this.pgFinish.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.WizardControl wizardControl1;
        private AeroWizard.WizardPage pgAvailableUpdates;
        private AeroWizard.WizardPage pgDownload;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblDownloadedBytes;
        private System.Windows.Forms.ProgressBar pbFileDownload;
        private AeroWizard.WizardPage pgFinish;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView lvAvailableUpdates;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label lblTotalProgress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblCurrentDownload;
        private System.Windows.Forms.ProgressBar pbTotalProgress;
    }
}