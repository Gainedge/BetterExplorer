namespace GongSolutions.Shell {
    partial class ShellItemBrowseForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Common", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("All", System.Windows.Forms.HorizontalAlignment.Left);
            this.tabControl = new System.Windows.Forms.TabControl();
            this.knownFoldersPage = new System.Windows.Forms.TabPage();
            this.knownFolderList = new System.Windows.Forms.ListView();
            this.allFilesPage = new System.Windows.Forms.TabPage();
            this.allFilesView = new GongSolutions.Shell.ShellView();
            this.allFilesToolbar = new GongSolutions.Shell.FileDialogToolbar();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.knownFoldersPage.SuspendLayout();
            this.allFilesPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.knownFoldersPage);
            this.tabControl.Controls.Add(this.allFilesPage);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(622, 402);
            this.tabControl.TabIndex = 0;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_Selected);
            // 
            // knownFoldersPage
            // 
            this.knownFoldersPage.Controls.Add(this.knownFolderList);
            this.knownFoldersPage.Location = new System.Drawing.Point(4, 22);
            this.knownFoldersPage.Name = "knownFoldersPage";
            this.knownFoldersPage.Padding = new System.Windows.Forms.Padding(3);
            this.knownFoldersPage.Size = new System.Drawing.Size(614, 376);
            this.knownFoldersPage.TabIndex = 0;
            this.knownFoldersPage.Text = "Known Folders";
            this.knownFoldersPage.UseVisualStyleBackColor = true;
            // 
            // knownFolderList
            // 
            this.knownFolderList.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "Common";
            listViewGroup1.Name = "common";
            listViewGroup2.Header = "All";
            listViewGroup2.Name = "all";
            this.knownFolderList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.knownFolderList.Location = new System.Drawing.Point(3, 3);
            this.knownFolderList.Name = "knownFolderList";
            this.knownFolderList.Size = new System.Drawing.Size(608, 370);
            this.knownFolderList.TabIndex = 0;
            this.knownFolderList.UseCompatibleStateImageBehavior = false;
            this.knownFolderList.View = System.Windows.Forms.View.Tile;
            this.knownFolderList.DoubleClick += new System.EventHandler(this.knownFolderList_DoubleClick);
            this.knownFolderList.SelectedIndexChanged += new System.EventHandler(this.knownFolderList_SelectedIndexChanged);
            // 
            // allFilesPage
            // 
            this.allFilesPage.Controls.Add(this.allFilesView);
            this.allFilesPage.Controls.Add(this.allFilesToolbar);
            this.allFilesPage.Location = new System.Drawing.Point(4, 22);
            this.allFilesPage.Name = "allFilesPage";
            this.allFilesPage.Padding = new System.Windows.Forms.Padding(3);
            this.allFilesPage.Size = new System.Drawing.Size(614, 376);
            this.allFilesPage.TabIndex = 1;
            this.allFilesPage.Text = "All Files";
            this.allFilesPage.UseVisualStyleBackColor = true;
            // 
            // allFilesView
            // 
            this.allFilesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.allFilesView.Location = new System.Drawing.Point(3, 31);
            this.allFilesView.Name = "allFilesView";
            this.allFilesView.Size = new System.Drawing.Size(608, 342);
            this.allFilesView.StatusBar = null;
            this.allFilesView.TabIndex = 1;
            this.allFilesView.Text = "shellView1";
            this.allFilesView.SelectionChanged += new System.EventHandler(this.fileBrowseView_SelectionChanged);
            // 
            // allFilesToolbar
            // 
            this.allFilesToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.allFilesToolbar.Location = new System.Drawing.Point(3, 3);
            this.allFilesToolbar.Name = "allFilesToolbar";
            this.allFilesToolbar.ShellView = this.allFilesView;
            this.allFilesToolbar.Size = new System.Drawing.Size(608, 28);
            this.allFilesToolbar.TabIndex = 0;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Enabled = false;
            this.okButton.Location = new System.Drawing.Point(478, 420);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(559, 420);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ShellItemBrowseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 455);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.tabControl);
            this.MinimizeBox = false;
            this.Name = "ShellItemBrowseForm";
            this.Text = "Browse for ShellItem";
            this.tabControl.ResumeLayout(false);
            this.knownFoldersPage.ResumeLayout(false);
            this.allFilesPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage knownFoldersPage;
        private System.Windows.Forms.TabPage allFilesPage;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.ListView knownFolderList;
        private ShellView allFilesView;
        private FileDialogToolbar allFilesToolbar;
    }
}