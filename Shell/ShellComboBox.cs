// BExplorer.Shell - A Windows Shell library for .Net.
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell
{
    /// <summary>
    /// Provides a drop-down list displaying the Windows Shell namespace.
    /// </summary>
    /// 
    /// <remarks>
    /// The <see cref="ShellComboBox"/> class displays a view of the Windows
    /// Shell namespace in a drop-down list similar to that displayed in
    /// a file open/save dialog.
    /// </remarks>
    public class ShellComboBox : Control
    {
        ComboBox m_Combo = new ComboBox();
        TextBox m_Edit = new TextBox();
        ShellView m_ShellView;
        bool m_Editable;
        ShellItem m_RootFolder = ShellItem.Desktop;
        ShellItem m_SelectedFolder;
        bool m_ChangingLocation;
        bool m_ShowFileSystemPath;
        bool m_CreatingItems;
        bool m_SelectAll;
        ShellNotificationListener m_ShellListener = new ShellNotificationListener();
        static ShellItem m_Computer;


        /// <summary>
        /// Initializes a new instance of the <see cref="ShellComboBox"/> class.
        /// </summary>
        public ShellComboBox()
        {
            m_Combo.Dock = DockStyle.Fill;
            m_Combo.DrawMode = DrawMode.OwnerDrawFixed;
            m_Combo.DropDownStyle = ComboBoxStyle.DropDownList;
            m_Combo.DropDownHeight = 300;
            m_Combo.ItemHeight = SystemInformation.SmallIconSize.Height + 1;
            m_Combo.Parent = this;
            m_Combo.Click += new EventHandler(m_Combo_Click);
            m_Combo.DrawItem += new DrawItemEventHandler(m_Combo_DrawItem);
            m_Combo.SelectedIndexChanged += new EventHandler(m_Combo_SelectedIndexChanged);

            m_Edit.Anchor = AnchorStyles.Left | AnchorStyles.Top |
                            AnchorStyles.Right | AnchorStyles.Bottom;
            m_Edit.BorderStyle = BorderStyle.None;
            m_Edit.Left = 8 + SystemInformation.SmallIconSize.Width;
            m_Edit.Top = 4;
            m_Edit.Width = Width - m_Edit.Left - 3 - SystemInformation.VerticalScrollBarWidth;
            m_Edit.Parent = this;
            m_Edit.Visible = false;
            m_Edit.GotFocus += new EventHandler(m_Edit_GotFocus);
            m_Edit.LostFocus += new EventHandler(m_Edit_LostFocus);
            m_Edit.KeyDown += new KeyEventHandler(m_Edit_KeyDown);
            m_Edit.MouseDown += new MouseEventHandler(m_Edit_MouseDown);
            m_Edit.BringToFront();

            m_ShellListener.DriveAdded += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.DriveRemoved += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.FolderCreated += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.FolderDeleted += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.FolderRenamed += new ShellItemChangeEventHandler(m_ShellListener_ItemRenamed);
            m_ShellListener.FolderUpdated += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.ItemCreated += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.ItemDeleted += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.ItemRenamed += new ShellItemChangeEventHandler(m_ShellListener_ItemRenamed);
            m_ShellListener.ItemUpdated += new ShellItemEventHandler(m_ShellListener_ItemUpdated);
            m_ShellListener.SharingChanged += new ShellItemEventHandler(m_ShellListener_ItemUpdated);

            m_SelectedFolder = ShellItem.Desktop;
            m_Edit.Text = GetEditString();

            if (m_Computer == null)
            {
                m_Computer = new ShellItem(Environment.SpecialFolder.MyComputer);
            }

            CreateItems();
        }

        /// <summary>
        /// Gets/sets a value indicating whether the combo box is editable.
        /// </summary>
        [DefaultValue(false)]
        public bool Editable
        {
            get { return m_Editable; }
            set { m_Edit.Visible = m_Editable = value; }
        }

        /// <summary>
        /// Gets/sets a value indicating whether the full file system path 
        /// should be displayed in the main portion of the control.
        /// </summary>
        [DefaultValue(false)]
        public bool ShowFileSystemPath
        {
            get { return m_ShowFileSystemPath; }
            set
            {
                m_ShowFileSystemPath = value;
                m_Combo.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets the folder that the <see cref="ShellComboBox"/> should
        /// display as the root folder.
        /// </summary>
        [Browsable(false)]
        public ShellItem RootFolder
        {
            get { return m_RootFolder; }
            set
            {
                m_RootFolder = value;
                if (!m_RootFolder.IsParentOf(m_SelectedFolder)) m_SelectedFolder = m_RootFolder;
                CreateItems();
            }
        }

        /// <summary>
        /// Gets/sets the folder currently selected in the 
        /// <see cref="ShellComboBox"/>.
        /// </summary>
        [Browsable(false)]
        public ShellItem SelectedFolder
        {
            get { return m_SelectedFolder; }
            set
            {
                if (m_SelectedFolder != value)
                {
                    m_SelectedFolder = value;
                    CreateItems();
                    m_Edit.Text = GetEditString();
                    NavigateShellView();
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets/sets a <see cref="ShellView"/> whose navigation should be
        /// controlled by the combo box.
        /// </summary>
        [DefaultValue(null), Category("Behavior")]
        public ShellView ShellView
        {
            get { return m_ShellView; }
            set
            {
                if (m_ShellView != null)
                    m_ShellView.Navigated -= m_ShellView_Navigated;

                m_ShellView = value;

                if (m_ShellView != null)
                {
                    m_ShellView.Navigated += m_ShellView_Navigated;
                    m_ShellView_Navigated(m_ShellView, new NavigatedEventArgs(m_ShellView.CurrentFolder, m_ShellView.CurrentFolder));
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="ShellComboBox"/>'s 
        /// <see cref="SelectedFolder"/> property changes.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Occurs when the <see cref="ShellComboBox"/> control wants to know
        /// if it should include a folder in its view.
        /// </summary>
        /// 
        /// <remarks>
        /// This event allows the folders displayed in the 
        /// <see cref="ShellComboBox"/> control to be filtered.
        /// </remarks>
        //public event FilterItemEventHandler FilterItem;

        internal bool ShouldSerializeRootFolder()
        {
            return m_RootFolder != ShellItem.Desktop;
        }

        internal bool ShouldSerializeSelectedFolder()
        {
            return m_SelectedFolder != ShellItem.Desktop;
        }

        void CreateItems()
        {
            if (!m_CreatingItems)
            {
                try
                {
                    m_CreatingItems = true;
                    m_Combo.Items.Clear();
                    CreateItem(m_RootFolder, 0);
                }
                finally
                {
                    m_CreatingItems = false;
                }
            }
        }

        void CreateItems(ShellItem folder, int indent)
        {
            IEnumerator<ShellItem> e = folder.GetEnumerator(
                SHCONTF.FOLDERS | SHCONTF.INCLUDEHIDDEN);

            while (e.MoveNext())
            {
                if (ShouldCreateItem(e.Current))
                {
                    CreateItem(e.Current, indent);
                }
            }
        }

        void CreateItem(ShellItem folder, int indent)
        {
            int index = m_Combo.Items.Add(new ComboItem(folder, indent));

            if (folder == m_SelectedFolder)
            {
                m_Combo.SelectedIndex = index;
            }

            if (ShouldCreateChildren(folder))
            {
                CreateItems(folder, indent + 1);
            }
        }

        [Obsolete("Always returns true")]
        bool ShouldCreateItem(ShellItem folder)
        {
            //TODO: Remove this method if we can!!!

            //FilterItemEventArgs e = new FilterItemEventArgs(folder);
            //ShellItem myComputer = new ShellItem(Environment.SpecialFolder.MyComputer);

            //e.Include = false;

            //if (ShellItem.Desktop.IsImmediateParentOf(folder) ||
            //    m_Computer.IsImmediateParentOf(folder))
            //{
            //    e.Include = folder.IsFileSystemAncestor;
            //}
            //else if ((folder == m_SelectedFolder) ||
            //           folder.IsParentOf(m_SelectedFolder))
            //{
            //    e.Include = true;
            //}

            //if (FilterItem != null)
            //{
            //    FilterItem(this, e);
            //}

            //return e.Include;
            return true;
        }

        bool ShouldCreateChildren(ShellItem folder)
        {
            return (folder == m_Computer) ||
                   (folder == ShellItem.Desktop) ||
                   folder.IsParentOf(m_SelectedFolder);
        }

        string GetEditString() => m_ShowFileSystemPath && m_SelectedFolder.IsFileSystem ? m_SelectedFolder.FileSystemPath : m_SelectedFolder.DisplayName;

        void NavigateShellView()
        {
            if (m_ShellView != null && !m_ChangingLocation)
            {
                try
                {
                    m_ChangingLocation = true;
                    //m_ShellView.Navigate(m_SelectedFolder);
                    //m_ShellView.Navigate_Full(m_SelectedFolder, false);
                }
                catch (Exception)
                {
                    //SelectedFolder = m_ShellView.CurrentFolder;
                }
                finally
                {
                    m_ChangingLocation = false;
                }
            }
        }

        void m_Combo_Click(object sender, EventArgs e) => OnClick(e);

        void m_Combo_DrawItem(object sender, DrawItemEventArgs e)
        {
            int iconWidth = SystemInformation.SmallIconSize.Width;
            int indent = ((e.State & DrawItemState.ComboBoxEdit) == 0) ? (iconWidth / 2) : 0;

            if (e.Index != -1)
            {
                string display;
                ComboItem item = (ComboItem)m_Combo.Items[e.Index];
                Color textColor = SystemColors.WindowText;
                Rectangle textRect;
                int textOffset;
                SizeF size;

                if ((e.State & DrawItemState.ComboBoxEdit) != 0)
                {
                    // Don't draw the folder location in the edit box when
                    // the control is Editable as the edit control will
                    // take care of that.
                    display = m_Editable ? string.Empty : GetEditString();
                }
                else
                {
                    display = item.Folder.DisplayName;
                }

                size = TextRenderer.MeasureText(display, m_Combo.Font);

                textRect = new Rectangle(e.Bounds.Left + iconWidth + (item.Indent * indent) + 3, e.Bounds.Y, (int)size.Width, e.Bounds.Height);
                textOffset = (int)((e.Bounds.Height - size.Height) / 2);

                // If the text is being drawin in the main combo box edit area,
                // draw the text 1 pixel higher - this is how it looks in Windows.
                if ((e.State & DrawItemState.ComboBoxEdit) != 0)
                {
                    textOffset -= 1;
                }

                if ((e.State & DrawItemState.Selected) != 0)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, textRect);
                    textColor = SystemColors.HighlightText;
                }
                else
                {
                    e.DrawBackground();
                }

                if ((e.State & DrawItemState.Focus) != 0)
                {
                    ControlPaint.DrawFocusRectangle(e.Graphics, textRect);
                }

                SystemImageList.DrawSmallImage(
                    e.Graphics,
                    new Point(e.Bounds.Left + (item.Indent * indent), e.Bounds.Top),
                    item.Folder.GetSystemImageListIndex(ShellIconType.SmallIcon,
                    ShellIconFlags.OverlayIndex),
                    (e.State & DrawItemState.Selected) != 0
                );
                TextRenderer.DrawText(e.Graphics, display, m_Combo.Font, new Point(textRect.Left, textRect.Top + textOffset), textColor);
            }
        }

        void m_Combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_CreatingItems)
            {
                SelectedFolder = ((ComboItem)m_Combo.SelectedItem).Folder;
            }
        }

        void m_Edit_GotFocus(object sender, EventArgs e)
        {
            m_Edit.SelectAll();
            m_SelectAll = true;
        }

        void m_Edit_LostFocus(object sender, EventArgs e)
        {
            m_SelectAll = false;
        }

        void m_Edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string path = m_Edit.Text;

                if ((path == string.Empty) ||
                    (string.Compare(path, "Desktop", true) == 0))
                {
                    SelectedFolder = ShellItem.Desktop;
                    return;
                }

                if (Directory.Exists(path))
                {
                    SelectedFolder = new ShellItem(path);
                    return;
                }

                path = Path.Combine(m_SelectedFolder.FileSystemPath, path);

                if (Directory.Exists(path))
                {
                    SelectedFolder = new ShellItem(path);
                    return;
                }
            }
        }

        void m_Edit_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_SelectAll)
            {
                m_Edit.SelectAll();
                m_SelectAll = false;
            }
            else
            {
                m_Edit.SelectionStart = m_Edit.Text.Length;
            }
        }

        void m_ShellView_Navigated(object sender, NavigatedEventArgs e)
        {
            if (!m_ChangingLocation)
            {
                try
                {
                    m_ChangingLocation = true;
                    Changed?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    m_ChangingLocation = false;
                }
            }
        }

        void m_ShellListener_ItemRenamed(object sender, ShellItemChangeEventArgs e) => CreateItems();
        void m_ShellListener_ItemUpdated(object sender, ShellItemEventArgs e) => CreateItems();

        class ComboItem
        {
            public ShellItem Folder;
            public int Indent;

            public ComboItem(ShellItem folder, int indent)
            {
                Folder = folder;
                Indent = indent;
            }
        }
    }
}
