namespace BetterExplorer
{
    partial class MoreColumns
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
            this.lvColumns = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lvColumns
            // 
            this.lvColumns.CheckBoxes = true;
            this.lvColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvColumns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvColumns.Location = new System.Drawing.Point(0, 0);
            this.lvColumns.MultiSelect = false;
            this.lvColumns.Name = "lvColumns";
            this.lvColumns.Size = new System.Drawing.Size(240, 351);
            this.lvColumns.TabIndex = 0;
            this.lvColumns.UseCompatibleStateImageBehavior = false;
            this.lvColumns.View = System.Windows.Forms.View.Details;
            this.lvColumns.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvColumns_ItemChecked);
            this.lvColumns.MouseLeave += new System.EventHandler(this.lvColumns_MouseLeave);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 200;
            // 
            // MoreColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(240, 351);
            this.Controls.Add(this.lvColumns);
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoreColumns";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "More Columns";
            this.Activated += new System.EventHandler(this.MoreColumns_Activated);
            this.Deactivate += new System.EventHandler(this.MoreColumns_Deactivate);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvColumns;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}