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
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using GongSolutions.Shell.Interop;

namespace GongSolutions.Shell
{
    /// <summary>
    /// A file-filter combo box suitable for use in open/save file dialogs.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// This control extends the <see cref="ComboBox"/> class to provide
    /// automatic filtering of shell items according to wildcard patterns. 
    /// By setting the control's <see cref="ShellView"/> property, the control
    /// will automatically filter items in a ShellView.
    /// </para>
    /// 
    /// <para>
    /// The <see cref="FilterItems"/> property accepts a filter string 
    /// similar to that accepted by the standard <see cref="FileDialog"/>
    /// class, for example: 
    /// <b>"Text files (*.txt)|*.txt|All files (*.*)|*.*"</b>
    /// </para>
    /// 
    /// <para>
    /// The currently selected filter is selected by the <see cref="Filter"/>
    /// property. This should be set to one of the filter patterns specified
    /// in <see cref="FilterItems"/>, e.g. <b>"*.txt"</b>.
    /// </para>
    /// </remarks>
    public class FileFilterComboBox : ComboBox
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFilterComboBox"/>
        /// class.
        /// </summary>
        public FileFilterComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            m_Regex = GenerateRegex(m_Filter);
        }

        /// <summary>
        /// Gets or sets the current filter string, which determines the 
        /// items that appear in the control's <see cref="ShellView"/>.
        /// </summary>
        [Category("Behaviour"), DefaultValue("*.*")]
        public string Filter
        {
            get { return m_Filter; }
            set
            {
                if ((value != null) && (value.Length > 0))
                {
                    m_Filter = value;
                }
                else
                {
                    m_Filter = "*.*";
                }

                m_Regex = GenerateRegex(m_Filter);

                foreach (FilterItem item in Items)
                {
                    if (item.Contains(m_Filter))
                    {
                        try
                        {
                            m_IgnoreSelectionChange = true;
                            SelectedItem = item;
                        }
                        finally
                        {
                            m_IgnoreSelectionChange = false;
                        }
                    }
                }

                if (m_ShellView != null)
                {
                    m_ShellView.RefreshContents();
                }
            }
        }

        /// <summary>
        /// This property does not apply to <see cref="FileFilterComboBox"/>.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(ComboBoxStyle.DropDownList)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return base.DropDownStyle; }
            set { base.DropDownStyle = value; }
        }

        /// <summary>
        /// Gets/sets the filter string which determines the choices that 
        /// appear in the control's drop-down list.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// For each filtering option, the filter string contains a 
        /// description of the filter, followed by the vertical bar (|) and
        /// the filter pattern. The strings for different filtering options
        /// are separated by the vertical bar.
        /// </para>
        /// 
        /// <para>
        /// If the filter itself does not appear in the description then
        /// it will be automatically added when the control is displayed.
        /// For example, in the example below the "Video files" entry will
        /// be displayed as "Video files (*.avi, *.wmv)". Beacuse the "All 
        /// files" entry already has the filter string present in its 
        /// description, it will not be added again.
        /// </para>
        /// 
        /// <para>
        /// <example>
        /// "Video files|*.avi, *.wmv|All files (*.*)|*.*"
        /// </example>
        /// </para>
        /// </remarks>
        [Category("Behaviour")]
        public string FilterItems
        {
            get { return m_FilterItems; }
            set
            {
                int selection;

                Items.Clear();
                Items.AddRange(FilterItem.ParseFilterString(value, m_Filter,
                    out selection));

                if (selection != -1)
                {
                    SelectedIndex = selection;
                }
                else
                {
                    try
                    {
                        m_IgnoreSelectionChange = true;
                        SelectedIndex = Items.Count - 1;
                    }
                    finally
                    {
                        m_IgnoreSelectionChange = false;
                    }
                }

                m_FilterItems = value;
            }
        }

        /// <summary>
        /// This property does not apply to <see cref="FileFilterComboBox"/>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ComboBox.ObjectCollection Items
        {
            get { return base.Items; }
        }

        /// <summary>
        /// Gets/sets the <see cref="ShellView"/> control that the 
        /// <see cref="FileFilterComboBox"/> should filter the items of.
        /// </summary>
        [DefaultValue(null)]
        [Category("Behaviour")]
        public ShellView ShellView
        {
            get { return m_ShellView; }
            set
            {
                if (m_ShellView != null)
                {
                    m_ShellView.FilterItem -= new FilterItemEventHandler(m_ShellView_FilterItem);
                }

                m_ShellView = value;

                if (m_ShellView != null)
                {
                    m_ShellView.FilterItem += new FilterItemEventHandler(m_ShellView_FilterItem);
                }
            }
        }

        /// <summary>
        /// Generates a <see cref="Regex"/> object equivalent to the 
        /// provided wildcard.
        /// </summary>
        /// 
        /// <param name="wildcard">
        /// The wildcard to generate a regex for.
        /// </param>
        /// 
        /// <returns>
        /// A regex equivalent to the wildcard.
        /// </returns>
        public static Regex GenerateRegex(string wildcard)
        {
            string[] wildcards = wildcard.Split(',');
            StringBuilder regexString = new StringBuilder();

            foreach (string s in wildcards)
            {
                if (regexString.Length > 0)
                {
                    regexString.Append('|');
                }

                regexString.Append(
                    Regex.Escape(s).
                    Replace(@"\*", ".*").
                    Replace(@"\?", "."));
            }

            return new Regex(regexString.ToString(),
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Raises the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// 
        /// <param name="e">
        /// An EventArgs that contains the event data.
        /// </param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (!m_IgnoreSelectionChange)
            {
                if (SelectedItem != null)
                {
                    Filter = ((FilterItem)SelectedItem).Filter;
                }
            }
        }

        void m_ShellView_FilterItem(object sender, FilterItemEventArgs e)
        {
            // Include items that are present in the filesystem, and are a 
            // folder, or match the current regex.
            if (e.Include)
            {
                e.Include = (e.Item.IsFileSystem || e.Item.IsFileSystemAncestor) &&
                    (e.Item.IsFolder || m_Regex.IsMatch(e.Item.FileSystemPath));
            }
        }

        string m_Filter = "*.*";
        string m_FilterItems = "";
        ShellView m_ShellView;
        Regex m_Regex;
        bool m_IgnoreSelectionChange;
    }
}
