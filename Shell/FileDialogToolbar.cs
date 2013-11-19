// GongSolutions.Shell - A Windows Shell library for .Net.
// Copyright (C) 2007-2009 Steven J. Kirk
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either 
// version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free 
// Software Foundation, Inc., 51 Franklin Street, Fifth Floor,  
// Boston, MA 2110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GongSolutions.Shell
{
    /// <summary>
    /// Provides a toolbar suitable for use in file Open/Save dialogs.
    /// </summary>
    /// 
    /// <remarks>
    /// This control provides a toolbar containing a 
    /// <see cref="ShellComboBox"/> and the various navigation buttons as
    /// found in a standard file dialog. By setting the 
    /// <see cref="ShellView"/> property, the toolbar will automatically 
    /// control the navigation of a ShellView> control in response to the 
    /// user's actions.
    /// </remarks>
    public partial class FileDialogToolbar : UserControl
    {

        /// <summary>
        /// Initializes a new instance of the FileDialogToolbar control.
        /// </summary>
        public FileDialogToolbar()
        {
            InitializeComponent();
            Dock = DockStyle.Top;
            toolStrip.Renderer = new CustomRenderer();
        }

        /// <summary>
        /// Gets/sets the root folder displayed in the toolbar's drop-down
        /// folder list.
        /// </summary>
        [Editor(typeof(ShellItemEditor), typeof(UITypeEditor))]
        public ShellItem RootFolder
        {
            get { return shellComboBox.RootFolder; }
            set { shellComboBox.RootFolder = value; }
        }


        /// <summary>
        /// Gets/sets the folder currently selected in the toolbar's combo box.
        /// </summary>
        [Editor(typeof(ShellItemEditor), typeof(UITypeEditor))]
        public ShellItem SelectedFolder
        {
            get { return shellComboBox.SelectedFolder; }
            set { shellComboBox.SelectedFolder = value; }
        }

        /// <summary>
        /// Gets/sets a <see cref="ShellView"/> whose navigation should be
        /// controlled by the toolbar.
        /// </summary>
        [DefaultValue(null), Category("Behaviour")]
        public ShellView ShellView
        {
            get { return shellComboBox.ShellView; }
            set
            {
                shellComboBox.ShellView = value;
                shellComboBox.Enabled = viewMenuButton.Enabled = (value != null);
                UpdateButtons();
            }
        }

        /// <summary>
        /// Occurs when the <see cref="FileDialogToolbar"/> needs to know 
        /// what items it should display in its drop-down list.
        /// </summary>
        public event FilterItemEventHandler FilterItem
        {
            add { shellComboBox.FilterItem += value; }
            remove { shellComboBox.FilterItem -= value; }
        }

        bool ShouldSerializeRootFolder()
        {
            return shellComboBox.ShouldSerializeRootFolder();
        }

        bool ShouldSerializeSelectedFolder()
        {
            return shellComboBox.ShouldSerializeSelectedFolder();
        }

        void UpdateButtons()
        {
            if (ShellView != null)
            {
                backButton.Enabled = ShellView.CanNavigateBack;
                upButton.Enabled = ShellView.CanNavigateParent;
                newFolderButton.Enabled = ShellView.CanCreateFolder;
            }
        }

        void shellComboBox_Changed(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        void shellView_Navigated(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        void backButton_Click(object sender, EventArgs e)
        {
            ShellView.NavigateBack();
        }

        void upButton_Click(object sender, EventArgs e)
        {
            ShellView.NavigateParent();
        }

        void newFolderButton_Click(object sender, EventArgs e)
        {
            ShellView.CreateNewFolder();
        }

        void viewThumbnailsMenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            ShellView.View = (ShellViewStyle)Convert.ToInt32(item.Tag);
        }

        class CustomRenderer : ToolStripSystemRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
        }
    }
}
