namespace BetterExplorer
{
    partial class IconView
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IconView));
      this.panel1 = new System.Windows.Forms.Panel();
      this.button1 = new System.Windows.Forms.Button();
      this.btnSet = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnLoad = new System.Windows.Forms.Button();
      this.tbLibrary = new System.Windows.Forms.TextBox();
      this.lvIcons = new BetterExplorer.ListView();
      this.imageList1 = new System.Windows.Forms.ImageList(this.components);
      this.pbProgress = new System.Windows.Forms.ProgressBar();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.button1);
      this.panel1.Controls.Add(this.btnSet);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 352);
      this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(574, 45);
      this.panel1.TabIndex = 0;
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button1.Location = new System.Drawing.Point(476, 7);
      this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(88, 27);
      this.button1.TabIndex = 1;
      this.button1.Text = "Cancel";
      this.button1.UseCompatibleTextRendering = true;
      this.button1.UseVisualStyleBackColor = true;
      // 
      // btnSet
      // 
      this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSet.Location = new System.Drawing.Point(382, 7);
      this.btnSet.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnSet.Name = "btnSet";
      this.btnSet.Size = new System.Drawing.Size(88, 27);
      this.btnSet.TabIndex = 0;
      this.btnSet.Text = "Set";
      this.btnSet.UseCompatibleTextRendering = true;
      this.btnSet.UseVisualStyleBackColor = true;
      this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.Color.Transparent;
      this.panel2.Controls.Add(this.btnLoad);
      this.panel2.Controls.Add(this.tbLibrary);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(574, 52);
      this.panel2.TabIndex = 2;
      // 
      // btnLoad
      // 
      this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnLoad.Location = new System.Drawing.Point(530, 14);
      this.btnLoad.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.btnLoad.Name = "btnLoad";
      this.btnLoad.Size = new System.Drawing.Size(34, 27);
      this.btnLoad.TabIndex = 1;
      this.btnLoad.Text = "...";
      this.btnLoad.UseVisualStyleBackColor = true;
      this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
      // 
      // tbLibrary
      // 
      this.tbLibrary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tbLibrary.Location = new System.Drawing.Point(15, 14);
      this.tbLibrary.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.tbLibrary.Name = "tbLibrary";
      this.tbLibrary.Size = new System.Drawing.Size(507, 23);
      this.tbLibrary.TabIndex = 0;
      this.tbLibrary.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbLibrary_KeyUp);
      // 
      // lvIcons
      // 
      this.lvIcons.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lvIcons.LargeImageList = this.imageList1;
      this.lvIcons.Location = new System.Drawing.Point(0, 52);
      this.lvIcons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.lvIcons.MultiSelect = false;
      this.lvIcons.Name = "lvIcons";
      this.lvIcons.OwnerDraw = true;
      this.lvIcons.Size = new System.Drawing.Size(574, 300);
      this.lvIcons.TabIndex = 3;
      this.lvIcons.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lvIcons_DrawItem);
      // 
      // imageList1
      // 
      this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
      this.imageList1.ImageSize = new System.Drawing.Size(48, 48);
      this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
      // 
      // pbProgress
      // 
      this.pbProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pbProgress.Location = new System.Drawing.Point(0, 339);
      this.pbProgress.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.pbProgress.Name = "pbProgress";
      this.pbProgress.Size = new System.Drawing.Size(574, 12);
      this.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.pbProgress.TabIndex = 4;
      this.pbProgress.Visible = false;
      // 
      // IconView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(574, 397);
      this.Controls.Add(this.pbProgress);
      this.Controls.Add(this.lvIcons);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.MinimizeBox = false;
      this.Name = "IconView";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Pick Icon";
      this.Load += new System.EventHandler(this.IconView_Load);
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private ListView lvIcons;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.TextBox tbLibrary;
        private System.Windows.Forms.Button btnSet;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar pbProgress;
    }
}