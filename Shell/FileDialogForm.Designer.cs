namespace GongSolutions.Shell {
    partial class FileDialogForm {
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
            System.Windows.Forms.Label filenameLabel;
            System.Windows.Forms.Label filetypeLabel;
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.shellView = new GongSolutions.Shell.ShellView();
            this.openButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.toolbar = new GongSolutions.Shell.FileDialogToolbar();
            this.filterCombo = new GongSolutions.Shell.FileFilterComboBox();
            this.fileNameCombo = new GongSolutions.Shell.FileNameComboBox();
            this.placesToolbar = new GongSolutions.Shell.PlacesToolbar();
            filenameLabel = new System.Windows.Forms.Label();
            filetypeLabel = new System.Windows.Forms.Label();
            this.mainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // filenameLabel
            // 
            filenameLabel.AutoSize = true;
            filenameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            filenameLabel.Location = new System.Drawing.Point(99, 271);
            filenameLabel.Name = "filenameLabel";
            filenameLabel.Size = new System.Drawing.Size(70, 29);
            filenameLabel.TabIndex = 0;
            filenameLabel.Text = "File &name:";
            filenameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // filetypeLabel
            // 
            filetypeLabel.AutoSize = true;
            filetypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            filetypeLabel.Location = new System.Drawing.Point(99, 300);
            filetypeLabel.Name = "filetypeLabel";
            filetypeLabel.Size = new System.Drawing.Size(70, 29);
            filetypeLabel.TabIndex = 2;
            filetypeLabel.Text = "Files of &type:";
            filetypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mainLayout
            // 
            this.mainLayout.BackColor = System.Drawing.Color.Transparent;
            this.mainLayout.ColumnCount = 4;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayout.Controls.Add(this.shellView, 1, 1);
            this.mainLayout.Controls.Add(filenameLabel, 1, 2);
            this.mainLayout.Controls.Add(filetypeLabel, 1, 3);
            this.mainLayout.Controls.Add(this.openButton, 3, 2);
            this.mainLayout.Controls.Add(this.cancelButton, 3, 3);
            this.mainLayout.Controls.Add(this.toolbar, 0, 0);
            this.mainLayout.Controls.Add(this.filterCombo, 2, 3);
            this.mainLayout.Controls.Add(this.fileNameCombo, 2, 2);
            this.mainLayout.Controls.Add(this.placesToolbar, 0, 1);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new System.Windows.Forms.Padding(4);
            this.mainLayout.RowCount = 4;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainLayout.Size = new System.Drawing.Size(521, 333);
            this.mainLayout.TabIndex = 1;
            // 
            // shellView
            // 
            this.mainLayout.SetColumnSpan(this.shellView, 3);
            this.shellView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shellView.Location = new System.Drawing.Point(99, 41);
            this.shellView.Name = "shellView";
            this.shellView.Size = new System.Drawing.Size(415, 227);
            this.shellView.StatusBar = null;
            this.shellView.TabIndex = 6;
            this.shellView.Text = "shellView1";
            this.shellView.View = GongSolutions.Shell.ShellViewStyle.List;
            this.shellView.DoubleClick += new System.EventHandler(this.shellView_DoubleClick);
            this.shellView.SelectionChanged += new System.EventHandler(this.shellView_SelectionChanged);
            // 
            // openButton
            // 
            this.openButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.openButton.Enabled = false;
            this.openButton.Location = new System.Drawing.Point(439, 274);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(75, 23);
            this.openButton.TabIndex = 4;
            this.openButton.Text = "&Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.cancelButton.Location = new System.Drawing.Point(439, 303);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // toolbar
            // 
            this.mainLayout.SetColumnSpan(this.toolbar, 4);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolbar.Location = new System.Drawing.Point(7, 7);
            this.toolbar.Name = "toolbar";
            this.toolbar.ShellView = this.shellView;
            this.toolbar.Size = new System.Drawing.Size(507, 28);
            this.toolbar.TabIndex = 7;
            // 
            // filterCombo
            // 
            this.filterCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterCombo.FilterItems = "";
            this.filterCombo.FormattingEnabled = true;
            this.filterCombo.Location = new System.Drawing.Point(175, 303);
            this.filterCombo.Name = "filterCombo";
            this.filterCombo.ShellView = this.shellView;
            this.filterCombo.Size = new System.Drawing.Size(258, 21);
            this.filterCombo.TabIndex = 3;
            // 
            // fileNameCombo
            // 
            this.fileNameCombo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileNameCombo.FilterControl = this.filterCombo;
            this.fileNameCombo.FormattingEnabled = true;
            this.fileNameCombo.Location = new System.Drawing.Point(175, 274);
            this.fileNameCombo.Name = "fileNameCombo";
            this.fileNameCombo.ShellView = this.shellView;
            this.fileNameCombo.Size = new System.Drawing.Size(258, 21);
            this.fileNameCombo.TabIndex = 1;
            this.fileNameCombo.FileNameEntered += new System.EventHandler(this.fileNameCombo_FilenameEntered);
            this.fileNameCombo.TextChanged += new System.EventHandler(this.fileNameCombo_TextChanged);
            // 
            // placesToolbar
            // 
            this.placesToolbar.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.placesToolbar.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.placesToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.placesToolbar.Location = new System.Drawing.Point(7, 41);
            this.placesToolbar.Name = "placesToolbar";
            this.mainLayout.SetRowSpan(this.placesToolbar, 3);
            this.placesToolbar.ShellView = this.shellView;
            this.placesToolbar.Size = new System.Drawing.Size(86, 285);
            this.placesToolbar.TabIndex = 8;
            // 
            // FileDialogForm
            // 
            this.AcceptButton = this.openButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(521, 333);
            this.Controls.Add(this.mainLayout);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileDialogForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "File Dialog";
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button openButton;
        public FileDialogToolbar toolbar;
        public FileFilterComboBox filterCombo;
        public FileNameComboBox fileNameCombo;
        public PlacesToolbar placesToolbar;
        public ShellView shellView;
    }
}
