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
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace GongSolutions.Shell
{
    /// <summary>
    /// A filename combo box suitable for use in file Open/Save dialogs.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// This control extends the <see cref="ComboBox"/> class to provide
    /// auto-completion of filenames based on the folder selected in a
    /// <see cref="ShellView"/>. The control also automatically navigates 
    /// the ShellView control when the user types a folder path.
    /// </para>
    /// </remarks>
    public class FileNameComboBox : ComboBox
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FileNameComboBox"/>
        /// class.
        /// </summary>
        [Category("Behaviour")]
        [DefaultValue(null)]
        public FileFilterComboBox FilterControl
        {
            get { return m_FilterControl; }
            set { m_FilterControl = value; }
        }

        /// <summary>
        /// Gets/sets the <see cref="ShellView"/> control that the 
        /// <see cref="FileNameComboBox"/> should look for auto-completion
        /// hints.
        /// </summary>
        [Category("Behaviour")]
        [DefaultValue(null)]
        public ShellView ShellView
        {
            get { return m_ShellView; }
            set
            {
                DisconnectEventHandlers();
                m_ShellView = value;
                ConnectEventHandlers();
            }
        }

        /// <summary>
        /// Occurs when a file name is entered into the 
        /// <see cref="FileNameComboBox"/> and the Return key pressed.
        /// </summary>
        public event EventHandler FileNameEntered;

        /// <summary>
        /// Determines whether the specified key is a regular input key or a 
        /// special key that requires preprocessing. 
        /// </summary>
        /// 
        /// <param name="keyData">
        /// One of the <see cref="Keys"/> values.
        /// </param>
        /// 
        /// <returns>
        /// true if the specified key is a regular input key; otherwise, false. 
        /// </returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                return true;
            }
            else
            {
                return base.IsInputKey(keyData);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyDown"/> event.
        /// </summary>
        /// 
        /// <param name="e">
        /// A <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Enter)
            {
                if ((Text.Length > 0) && (!Open(Text)) &&
                    (m_FilterControl != null))
                {
                    m_FilterControl.Filter = Text;
                }
            }

            m_TryAutoComplete = false;
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyPress"/> event.
        /// </summary>
        /// 
        /// <param name="e">
        /// A <see cref="KeyPressEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            m_TryAutoComplete = char.IsLetterOrDigit(e.KeyChar);
        }

        /// <summary>
        /// Raises the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// 
        /// <param name="e">
        /// An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (m_TryAutoComplete)
            {
                try
                {
                    AutoComplete();
                }
                catch (Exception) { }
            }
        }

        void AutoComplete()
        {
            string path;
            string pattern;
            string[] matches;
            bool rooted = true;

            if ((Text == string.Empty) ||
                (Text.IndexOfAny(new char[] { '?', '*' }) != -1))
            {
                return;
            }

            path = Path.GetDirectoryName(Text);
            pattern = Path.GetFileName(Text);

            if (((path == null) || (path == string.Empty)) &&
                ((m_ShellView != null) &&
                m_ShellView.CurrentFolder.IsFileSystem &&
                (m_ShellView.CurrentFolder != ShellItem.Desktop)))
            {
                path = m_ShellView.CurrentFolder.FileSystemPath;
                pattern = Text;
                rooted = false;
            }

            matches = Directory.GetFiles(path, pattern + '*');

            for (int n = 0; n < 2; ++n)
            {
                if (matches.Length > 0)
                {
                    int currentLength = Text.Length;
                    Text = rooted ? matches[0] : Path.GetFileName(matches[0]);
                    SelectionStart = currentLength;
                    SelectionLength = Text.Length;
                    break;
                }
                else
                {
                    matches = Directory.GetDirectories(path, pattern + '*');
                }
            }
        }

        void ConnectEventHandlers()
        {
            if (m_ShellView != null)
            {
                m_ShellView.SelectionChanged += new EventHandler(m_ShellView_SelectionChanged);
            }
        }

        void DisconnectEventHandlers()
        {
            if (m_ShellView != null)
            {
                m_ShellView.SelectionChanged -= new EventHandler(m_ShellView_SelectionChanged);
            }
        }

        bool Open(string path)
        {
            bool result = false;

            if (File.Exists(path))
            {
                if (FileNameEntered != null)
                {
                    FileNameEntered(this, EventArgs.Empty);
                }
                result = true;
            }
            else if (Directory.Exists(path))
            {
                if (m_ShellView != null)
                {
                    m_ShellView.Navigate(path);
                    Text = string.Empty;
                    result = true;
                }
            }
            else
            {
                OpenParentOf(path);
                Text = Path.GetFileName(path);
            }

            if (!Path.IsPathRooted(path) &&
                m_ShellView.CurrentFolder.IsFileSystem)
            {
                result = Open(Path.Combine(m_ShellView.CurrentFolder.FileSystemPath,
                                           path));
            }

            return result;
        }

        void OpenParentOf(string path)
        {
            string parent = Path.GetDirectoryName(path);

            if ((parent != null) && (parent.Length > 0) &&
                Directory.Exists(parent))
            {
                if (m_ShellView != null)
                {
                    m_ShellView.Navigate(parent);
                }
            }
        }

        void m_ShellView_SelectionChanged(object sender, EventArgs e)
        {
            if ((m_ShellView.SelectedItems.Length > 0) &&
                (!m_ShellView.SelectedItems[0].IsFolder) &&
                (m_ShellView.SelectedItems[0].IsFileSystem))
            {
                Text = Path.GetFileName(m_ShellView.SelectedItems[0].FileSystemPath);
            }
        }

        FileFilterComboBox m_FilterControl;
        ShellView m_ShellView;
        bool m_TryAutoComplete;
    }
}
