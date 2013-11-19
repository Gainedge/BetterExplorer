// GongSolutions.Shell - A Windows Shell library for .Net.
// Copyright (C) 2007 Steven J. Kirk
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, 
// MA 2110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GongSolutions.Shell.Interop;

namespace GongSolutions.Shell {
    partial class FileDialogForm : Form {
        public FileDialogForm() {
            InitializeComponent();
        }

        void UpdateOpenButtonState() {
            openButton.Enabled = (shellView.SelectedItems.Length > 0) ||
                                 (fileNameCombo.Text.Length > 0);
        }

        void fileNameCombo_TextChanged(object sender, EventArgs e) {
            UpdateOpenButtonState();
        }

        void shellView_DoubleClick(object sender, EventArgs e) {
            m_FileName = shellView.SelectedItems[0].FileSystemPath;
            DialogResult = DialogResult.OK;
        }

        void shellView_SelectionChanged(object sender, EventArgs e) {
            UpdateOpenButtonState();
        }

        void fileNameCombo_FilenameEntered(object sender, EventArgs e) {
            m_FileName = fileNameCombo.Text;
            DialogResult = DialogResult.OK;
        }

        void openButton_Click(object sender, EventArgs e) {
            if (!shellView.NavigateSelectedFolder()) {
                ShellItem[] selected = shellView.SelectedItems;

                if (selected.Length > 0) {
                    m_FileName = selected[0].FileSystemPath;
                } else if (File.Exists(fileNameCombo.Text)) {
                    m_FileName = fileNameCombo.Text;
                }
            }
        }

        void cancelButton_Click(object sender, EventArgs e) {
            Close();
        }

        string m_FileName = string.Empty;
    }
}
