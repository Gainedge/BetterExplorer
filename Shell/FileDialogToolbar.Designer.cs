namespace GongSolutions.Shell {
    partial class FileDialogToolbar {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.Windows.Forms.Label lookInLabel;
            System.Windows.Forms.ToolStripMenuItem viewThumbnailsMenu;
            System.Windows.Forms.ToolStripMenuItem viewTilesMenu;
            System.Windows.Forms.ToolStripMenuItem viewIconsMenu;
            System.Windows.Forms.ToolStripMenuItem viewListMenu;
            System.Windows.Forms.ToolStripMenuItem viewDetailsMenu;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileDialogToolbar));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.backButton = new System.Windows.Forms.ToolStripButton();
            this.upButton = new System.Windows.Forms.ToolStripButton();
            this.newFolderButton = new System.Windows.Forms.ToolStripButton();
            this.viewMenuButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.shellComboBox = new GongSolutions.Shell.ShellComboBox();
            lookInLabel = new System.Windows.Forms.Label();
            viewThumbnailsMenu = new System.Windows.Forms.ToolStripMenuItem();
            viewTilesMenu = new System.Windows.Forms.ToolStripMenuItem();
            viewIconsMenu = new System.Windows.Forms.ToolStripMenuItem();
            viewListMenu = new System.Windows.Forms.ToolStripMenuItem();
            viewDetailsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lookInLabel
            // 
            lookInLabel.AutoSize = true;
            lookInLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            lookInLabel.Location = new System.Drawing.Point(3, 0);
            lookInLabel.Name = "lookInLabel";
            lookInLabel.Size = new System.Drawing.Size(45, 31);
            lookInLabel.TabIndex = 10;
            lookInLabel.Text = "Look &in:";
            lookInLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // viewThumbnailsMenu
            // 
            viewThumbnailsMenu.Name = "viewThumbnailsMenu";
            viewThumbnailsMenu.Size = new System.Drawing.Size(138, 22);
            viewThumbnailsMenu.Tag = "5";
            viewThumbnailsMenu.Text = "Thumbnails";
            viewThumbnailsMenu.Click += new System.EventHandler(this.viewThumbnailsMenu_Click);
            // 
            // viewTilesMenu
            // 
            viewTilesMenu.Name = "viewTilesMenu";
            viewTilesMenu.Size = new System.Drawing.Size(138, 22);
            viewTilesMenu.Tag = "6";
            viewTilesMenu.Text = "Tiles";
            viewTilesMenu.Click += new System.EventHandler(this.viewThumbnailsMenu_Click);
            // 
            // viewIconsMenu
            // 
            viewIconsMenu.Name = "viewIconsMenu";
            viewIconsMenu.Size = new System.Drawing.Size(138, 22);
            viewIconsMenu.Tag = "1";
            viewIconsMenu.Text = "Icons";
            viewIconsMenu.Click += new System.EventHandler(this.viewThumbnailsMenu_Click);
            // 
            // viewListMenu
            // 
            viewListMenu.Name = "viewListMenu";
            viewListMenu.Size = new System.Drawing.Size(138, 22);
            viewListMenu.Tag = "3";
            viewListMenu.Text = "List";
            viewListMenu.Click += new System.EventHandler(this.viewThumbnailsMenu_Click);
            // 
            // viewDetailsMenu
            // 
            viewDetailsMenu.Name = "viewDetailsMenu";
            viewDetailsMenu.Size = new System.Drawing.Size(138, 22);
            viewDetailsMenu.Tag = "4";
            viewDetailsMenu.Text = "Details";
            viewDetailsMenu.Click += new System.EventHandler(this.viewThumbnailsMenu_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(lookInLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.toolStrip, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.shellComboBox, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(478, 31);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backButton,
            this.upButton,
            this.newFolderButton,
            this.viewMenuButton});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip.Location = new System.Drawing.Point(377, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(101, 31);
            this.toolStrip.TabIndex = 11;
            this.toolStrip.TabStop = true;
            this.toolStrip.Text = "toolStrip1";
            // 
            // backButton
            // 
            this.backButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.backButton.Enabled = false;
            this.backButton.Image = ((System.Drawing.Image)(resources.GetObject("backButton.Image")));
            this.backButton.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(23, 28);
            this.backButton.Text = "toolStripButton1";
            this.backButton.ToolTipText = "Back";
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // upButton
            // 
            this.upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.upButton.Enabled = false;
            this.upButton.Image = ((System.Drawing.Image)(resources.GetObject("upButton.Image")));
            this.upButton.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(23, 28);
            this.upButton.Text = "toolStripButton2";
            this.upButton.ToolTipText = "Up One Level";
            this.upButton.Click += new System.EventHandler(this.upButton_Click);
            // 
            // newFolderButton
            // 
            this.newFolderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newFolderButton.Enabled = false;
            this.newFolderButton.Image = ((System.Drawing.Image)(resources.GetObject("newFolderButton.Image")));
            this.newFolderButton.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.newFolderButton.Name = "newFolderButton";
            this.newFolderButton.Size = new System.Drawing.Size(23, 28);
            this.newFolderButton.Text = "toolStripButton3";
            this.newFolderButton.ToolTipText = "New Folder";
            this.newFolderButton.Click += new System.EventHandler(this.newFolderButton_Click);
            // 
            // viewMenuButton
            // 
            this.viewMenuButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.viewMenuButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            viewThumbnailsMenu,
            viewTilesMenu,
            viewIconsMenu,
            viewListMenu,
            viewDetailsMenu});
            this.viewMenuButton.Enabled = false;
            this.viewMenuButton.Image = ((System.Drawing.Image)(resources.GetObject("viewMenuButton.Image")));
            this.viewMenuButton.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(233)))), ((int)(((byte)(216)))));
            this.viewMenuButton.Name = "viewMenuButton";
            this.viewMenuButton.Size = new System.Drawing.Size(29, 28);
            this.viewMenuButton.Text = "toolStripDropDownButton1";
            this.viewMenuButton.ToolTipText = "Views";
            // 
            // shellComboBox
            // 
            this.shellComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shellComboBox.Enabled = false;
            this.shellComboBox.Location = new System.Drawing.Point(54, 3);
            this.shellComboBox.Name = "shellComboBox";
            this.shellComboBox.Size = new System.Drawing.Size(320, 25);
            this.shellComboBox.TabIndex = 9;
            this.shellComboBox.Text = "shellComboBox1";
            this.shellComboBox.Changed += new System.EventHandler(this.shellComboBox_Changed);
            // 
            // FileDialogToolbar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FileDialogToolbar";
            this.Size = new System.Drawing.Size(478, 31);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton backButton;
        private System.Windows.Forms.ToolStripButton upButton;
        private System.Windows.Forms.ToolStripButton newFolderButton;
        private System.Windows.Forms.ToolStripDropDownButton viewMenuButton;
        private ShellComboBox shellComboBox;

    }
}
