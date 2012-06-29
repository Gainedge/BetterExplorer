namespace BetterExplorer
{
    partial class ShareWizzard
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
            this.wizardControl1 = new AeroWizard.WizardControl();
            this.wizardPage1 = new AeroWizard.WizardPage();
            this.label2 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.cbUsers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pbIcon = new System.Windows.Forms.PictureBox();
            this.txtShareName = new System.Windows.Forms.TextBox();
            this.wizardPage2 = new AeroWizard.WizardPage();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).BeginInit();
            this.wizardPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // wizardControl1
            // 
            this.wizardControl1.Location = new System.Drawing.Point(0, 0);
            this.wizardControl1.Name = "wizardControl1";
            this.wizardControl1.NextButtonShieldEnabled = true;
            this.wizardControl1.NextButtonText = "&Share";
            this.wizardControl1.Pages.Add(this.wizardPage1);
            this.wizardControl1.Pages.Add(this.wizardPage2);
            this.wizardControl1.Size = new System.Drawing.Size(622, 394);
            this.wizardControl1.TabIndex = 0;
            this.wizardControl1.Title = "Share Wizard";
            // 
            // wizardPage1
            // 
            this.wizardPage1.Controls.Add(this.label2);
            this.wizardPage1.Controls.Add(this.listView1);
            this.wizardPage1.Controls.Add(this.button1);
            this.wizardPage1.Controls.Add(this.cbUsers);
            this.wizardPage1.Controls.Add(this.label1);
            this.wizardPage1.Controls.Add(this.pbIcon);
            this.wizardPage1.Controls.Add(this.txtShareName);
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.Size = new System.Drawing.Size(575, 241);
            this.wizardPage1.TabIndex = 0;
            this.wizardPage1.Text = "Setup Share parameters";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(148, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Permisions";
            this.label2.UseCompatibleTextRendering = true;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.Enabled = false;
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new System.Drawing.Point(148, 111);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(412, 117);
            this.listView1.TabIndex = 5;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "User Name";
            this.columnHeader1.Width = 228;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Read";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Write";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Execute";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(533, 82);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(27, 23);
            this.button1.TabIndex = 4;
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // cbUsers
            // 
            this.cbUsers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cbUsers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUsers.Enabled = false;
            this.cbUsers.FormattingEnabled = true;
            this.cbUsers.Location = new System.Drawing.Point(148, 82);
            this.cbUsers.Name = "cbUsers";
            this.cbUsers.Size = new System.Drawing.Size(385, 24);
            this.cbUsers.Sorted = true;
            this.cbUsers.TabIndex = 3;
            this.cbUsers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cbUsers_DrawItem);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(145, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Share Name";
            this.label1.UseCompatibleTextRendering = true;
            // 
            // pbIcon
            // 
            this.pbIcon.Location = new System.Drawing.Point(3, 3);
            this.pbIcon.Name = "pbIcon";
            this.pbIcon.Size = new System.Drawing.Size(128, 128);
            this.pbIcon.TabIndex = 1;
            this.pbIcon.TabStop = false;
            // 
            // txtShareName
            // 
            this.txtShareName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtShareName.Location = new System.Drawing.Point(148, 29);
            this.txtShareName.Name = "txtShareName";
            this.txtShareName.Size = new System.Drawing.Size(412, 23);
            this.txtShareName.TabIndex = 0;
            // 
            // wizardPage2
            // 
            this.wizardPage2.AllowBack = false;
            this.wizardPage2.AllowNext = false;
            this.wizardPage2.IsFinishPage = true;
            this.wizardPage2.Name = "wizardPage2";
            this.wizardPage2.ShowNext = false;
            this.wizardPage2.Size = new System.Drawing.Size(575, 241);
            this.wizardPage2.TabIndex = 1;
            this.wizardPage2.Text = "Share was created succesfully";
            // 
            // ShareWizzard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 394);
            this.ControlBox = false;
            this.Controls.Add(this.wizardControl1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ShareWizzard";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Share Wizard";
            this.Load += new System.EventHandler(this.ShareWizzard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl1)).EndInit();
            this.wizardPage1.ResumeLayout(false);
            this.wizardPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.WizardControl wizardControl1;
        private AeroWizard.WizardPage wizardPage1;
        private AeroWizard.WizardPage wizardPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbIcon;
        private System.Windows.Forms.TextBox txtShareName;
        private System.Windows.Forms.ComboBox cbUsers;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
    }
}