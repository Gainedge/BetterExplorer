namespace BetterExplorer
{
    partial class ArchiveDetailView
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
            this.archiveTree = new System.Windows.Forms.TreeView();
            this.btn_extract = new System.Windows.Forms.Button();
            this.btn_view = new System.Windows.Forms.Button();
            this.btn_removefile = new System.Windows.Forms.Button();
            this.btn_checkarchive = new System.Windows.Forms.Button();
            this.lvArchiveDetails = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // archiveTree
            // 
            this.archiveTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.archiveTree.Location = new System.Drawing.Point(13, 43);
            this.archiveTree.Name = "archiveTree";
            this.archiveTree.Size = new System.Drawing.Size(121, 404);
            this.archiveTree.TabIndex = 0;
            this.archiveTree.DoubleClick += new System.EventHandler(this.btn_view_Click);
            // 
            // btn_extract
            // 
            this.btn_extract.Location = new System.Drawing.Point(13, 13);
            this.btn_extract.Name = "btn_extract";
            this.btn_extract.Size = new System.Drawing.Size(75, 23);
            this.btn_extract.TabIndex = 1;
            this.btn_extract.Text = "Extract";
            this.btn_extract.UseVisualStyleBackColor = true;
            this.btn_extract.Click += new System.EventHandler(this.btn_extract_Click);
            // 
            // btn_view
            // 
            this.btn_view.Location = new System.Drawing.Point(95, 13);
            this.btn_view.Name = "btn_view";
            this.btn_view.Size = new System.Drawing.Size(75, 23);
            this.btn_view.TabIndex = 2;
            this.btn_view.Text = "View file";
            this.btn_view.UseVisualStyleBackColor = true;
            this.btn_view.Click += new System.EventHandler(this.btn_view_Click);
            // 
            // btn_removefile
            // 
            this.btn_removefile.Enabled = false;
            this.btn_removefile.Location = new System.Drawing.Point(177, 12);
            this.btn_removefile.Name = "btn_removefile";
            this.btn_removefile.Size = new System.Drawing.Size(75, 23);
            this.btn_removefile.TabIndex = 3;
            this.btn_removefile.Text = "Remove file";
            this.btn_removefile.UseVisualStyleBackColor = true;
            this.btn_removefile.Click += new System.EventHandler(this.btn_removefile_Click);
            // 
            // btn_checkarchive
            // 
            this.btn_checkarchive.Location = new System.Drawing.Point(259, 12);
            this.btn_checkarchive.Name = "btn_checkarchive";
            this.btn_checkarchive.Size = new System.Drawing.Size(101, 23);
            this.btn_checkarchive.TabIndex = 4;
            this.btn_checkarchive.Text = "Check archive";
            this.btn_checkarchive.UseVisualStyleBackColor = true;
            this.btn_checkarchive.Click += new System.EventHandler(this.btn_checkarchive_Click);
            // 
            // lvArchiveDetails
            // 
            this.lvArchiveDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvArchiveDetails.Location = new System.Drawing.Point(13, 43);
            this.lvArchiveDetails.Name = "lvArchiveDetails";
            this.lvArchiveDetails.Size = new System.Drawing.Size(639, 404);
            this.lvArchiveDetails.TabIndex = 5;
            this.lvArchiveDetails.UseCompatibleStateImageBehavior = false;
            this.lvArchiveDetails.ItemActivate += new System.EventHandler(this.lvArchiveDetails_ItemActivate);
            this.lvArchiveDetails.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvArchiveDetails_ItemSelectionChanged);
            // 
            // ArchiveDetailView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 459);
            this.Controls.Add(this.lvArchiveDetails);
            this.Controls.Add(this.btn_checkarchive);
            this.Controls.Add(this.btn_removefile);
            this.Controls.Add(this.btn_view);
            this.Controls.Add(this.btn_extract);
            this.Controls.Add(this.archiveTree);
            this.Name = "ArchiveDetailView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ArchiveDetailView";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView archiveTree;
        private System.Windows.Forms.Button btn_extract;
        private System.Windows.Forms.Button btn_view;
        private System.Windows.Forms.Button btn_removefile;
        private System.Windows.Forms.Button btn_checkarchive;
        private System.Windows.Forms.ListView lvArchiveDetails;
    }
}