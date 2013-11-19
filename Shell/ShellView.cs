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
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Forms;
using GongSolutions.Shell.Interop;
using ComTypes = System.Runtime.InteropServices.ComTypes;

namespace GongSolutions.Shell
{
    /// <summary>
    /// Specifies how list items are displayed in a <see cref="ShellView"/> 
    /// control. 
    /// </summary>
    public enum ShellViewStyle
    {
        /// <summary>
        /// Each item appears as a full-sized icon with a label below it. 
        /// </summary>
        LargeIcon = 1,

        /// <summary>
        /// Each item appears as a small icon with a label to its right. 
        /// </summary>
        SmallIcon,

        /// <summary>
        /// Each item appears as a small icon with a label to its right. 
        /// Items are arranged in columns with no column headers. 
        /// </summary>
        List,

        /// <summary>
        /// Each item appears on a separate line with further information 
        /// about each item arranged in columns. The left-most column 
        /// contains a small icon and label. 
        /// </summary>
        Details,

        /// <summary>
        /// Each item appears with a thumbnail picture of the file's content.
        /// </summary>
        Thumbnail,

        /// <summary>
        /// Each item appears as a full-sized icon with the item label and 
        /// file information to the right of it. 
        /// </summary>
        Tile,

        /// <summary>
        /// Each item appears in a thumbstrip at the bottom of the control,
        /// with a large preview of the seleted item appearing above.
        /// </summary>
        Thumbstrip,
				/// <summary>
				/// Each item appears in a item that occupy the whole view width
				/// </summary>
				Content,
    }

    /// <summary>
    /// Provides a view of a computer's files and folders.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// The <see cref="ShellView"/> control allows you to embed Windows 
    /// Explorer functionality in your Windows Forms applications. The
    /// control provides a view of a single folder's contents, as it would
    /// appear in the right-hand pane in Explorer.
    /// </para>
    /// 
    /// <para>
    /// When a new <see cref="ShellView"/> control is added to a form,
    /// it displays the contents of the Desktop folder. Other folders
    /// can be displayed by calling one of the Navigate methods or setting
    /// the <see cref="CurrentFolder"/> property.
    /// </para>
    /// </remarks>    
    public class ShellView : Control, INotifyPropertyChanged
    {
			public ShellDeffViewSubClassedWindow subclassed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellView"/> class.
        /// </summary>
        public ShellView()
        {
					this.DoubleBuffered = true;
            m_History = new ShellHistory();
            m_MultiSelect = true;
            m_View = ShellViewStyle.LargeIcon;
            Size = new Size(250, 200);
            Navigate(ShellItem.Desktop);
        }

        /// <summary>
        /// Creates a new folder in the folder currently being browsed.
        /// </summary>
        public void CreateNewFolder()
        {
            string name = "New Folder";
            int suffix = 0;

            do
            {
                name = string.Format("{0}\\New Folder ({1})",
                    CurrentFolder.FileSystemPath, ++suffix);
            } while (Directory.Exists(name) || File.Exists(name));

            ERROR result = Shell32.SHCreateDirectory(m_ShellViewWindow, name);

            switch (result)
            {
                case ERROR.FILE_EXISTS:
                case ERROR.ALREADY_EXISTS:
                    throw new IOException("The directory already exists");
                case ERROR.BAD_PATHNAME:
                    throw new IOException("Bad pathname");
                case ERROR.FILENAME_EXCED_RANGE:
                    throw new IOException("The filename is too long");
            }
        }

        /// <summary>
        /// Deletes the item currently selected in the <see cref="ShellView"/>.
        /// </summary>
        public void DeleteSelectedItems()
        {
            //RelativePidl[] pidls = new RelativePidl[SelectedItems.Length];

            //for (int n = 0; n < SelectedItems.Length; ++n) {
            //    pidls[n] = SelectedItems[n].RelativePidl;
            //}

            //m_CurrentFolder.GetChildContextMenu(pidls).InvokeDelete();
        }

        /// <summary>
        /// Navigates to the specified <see cref="ShellItem"/>.
        /// </summary>
        /// 
        /// <param name="folder">
        /// The folder to navigate to.
        /// </param>
        public void Navigate(ShellItem folder)
        {
            NavigatingEventArgs e = new NavigatingEventArgs(folder);

            if (Navigating != null)
            {
                Navigating(this, e);
            }

            if (!e.Cancel)
            {
                ShellItem previous = m_CurrentFolder;
                m_CurrentFolder = folder;

                try
                {
                    RecreateShellView();
                    m_History.Add(folder);
										NavigatedEventArgs enav = new NavigatedEventArgs(folder);
                    OnNavigated(enav);
										OnViewChanged(new ViewChangedEventArgs(this.View, this.ThumbnailSize));
                }
                catch (Exception)
                {
                    m_CurrentFolder = previous;
                    RecreateShellView();
                    throw;
                }
            }
        }

        /// <summary>
        /// Navigates to the specified filesystem directory.
        /// </summary>
        /// 
        /// <param name="path">
        /// The path of the directory to navigate to.
        /// </param>
        /// 
        /// <exception cref="DirectoryNotFoundException">
        /// <paramref name="path"/> is not a valid folder.
        /// </exception>
        public void Navigate(string path)
        {
            Navigate(new ShellItem(path));
        }

        /// <summary>
        /// Navigates to the specified standard location.
        /// </summary>
        /// 
        /// <param name="location">
        /// The <see cref="Environment.SpecialFolder"/> to which to navigate.
        /// </param>
        /// 
        /// <remarks>
        /// Standard locations are virtual folders which may be located in 
        /// different places in different versions of Windows. For example 
        /// the "My Documents" folder is normally located at C:\My Documents 
        /// on Windows 98, but is located in the user's "Documents and 
        /// Settings" folder in Windows XP. Using a standard 
        /// <see cref="Environment.SpecialFolder"/> to refer to such folders 
        /// ensures that your application will behave correctly on all 
        /// versions of Windows.
        /// </remarks>
        public void Navigate(Environment.SpecialFolder location)
        {

            // CSIDL_MYDOCUMENTS was introduced in Windows XP but doesn't work 
            // even on that platform. Use CSIDL_PERSONAL instead.
            if (location == Environment.SpecialFolder.MyDocuments)
            {
                location = Environment.SpecialFolder.Personal;
            }

            Navigate(new ShellItem(location));
        }

        /// <summary>
        /// Navigates the <see cref="ShellView"/> control to the previous folder 
        /// in the navigation history. 
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// The WebBrowser control maintains a history list of all the folders
        /// visited during a session. You can use the <see cref="NavigateBack"/>
        /// method to implement a <b>Back</b> button similar to the one in 
        /// Windows Explorer, which will allow your users to return to a 
        /// previous folder in the navigation history. 
        /// </para>
        /// 
        /// <para>
        /// Use the <see cref="CanNavigateBack"/> property to determine whether 
        /// the navigation history is available and contains a previous page. 
        /// This property is useful, for example, to change the enabled state 
        /// of a Back button when the ShellView control navigates to or leaves 
        /// the beginning of the navigation history.
        /// </para>
        /// </remarks>
        /// 
        /// <exception cref="InvalidOperationException">
        /// There is no history to navigate backwards through.
        /// </exception>
        public void NavigateBack()
        {
            m_CurrentFolder = m_History.MoveBack();
            RecreateShellView();
            OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
        }

        /// <summary>
        /// Navigates the <see cref="ShellView"/> control backwards to the 
        /// requested folder in the navigation history. 
        /// </summary>
        /// 
        /// <remarks>
        /// The WebBrowser control maintains a history list of all the folders
        /// visited during a session. You can use the <see cref="NavigateBack"/>
        /// method to implement a drop-down menu on a <b>Back</b> button similar 
        /// to the one in Windows Explorer, which will allow your users to return 
        /// to a previous folder in the navigation history. 
        /// </remarks>
        /// 
        /// <param name="folder">
        /// The folder to navigate to.
        /// </param>
        /// 
        /// <exception cref="Exception">
        /// The requested folder is not present in the 
        /// <see cref="ShellView"/>'s 'back' history.
        /// </exception>
        public void NavigateBack(ShellItem folder)
        {
            m_History.MoveBack(folder);
            m_CurrentFolder = folder;
            RecreateShellView();
            OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
        }

        /// <summary>
        /// Navigates the <see cref="ShellView"/> control to the next folder 
        /// in the navigation history. 
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        /// The WebBrowser control maintains a history list of all the folders
        /// visited during a session. You can use the <see cref="NavigateForward"/> 
        /// method to implement a <b>Forward</b> button similar to the one 
        /// in Windows Explorer, allowing your users to return to the next 
        /// folder in the navigation history after navigating backward.
        /// </para>
        /// 
        /// <para>
        /// Use the <see cref="CanNavigateForward"/> property to determine 
        /// whether the navigation history is available and contains a folder 
        /// located after the current one.  This property is useful, for 
        /// example, to change the enabled state of a <b>Forward</b> button 
        /// when the ShellView control navigates to or leaves the end of the 
        /// navigation history.
        /// </para>
        /// </remarks>
        /// 
        /// <exception cref="InvalidOperationException">
        /// There is no history to navigate forwards through.
        /// </exception>
        public void NavigateForward()
        {
            m_CurrentFolder = m_History.MoveForward();
            RecreateShellView();
            OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
        }

        /// <summary>
        /// Navigates the <see cref="ShellView"/> control forwards to the 
        /// requested folder in the navigation history. 
        /// </summary>
        /// 
        /// <remarks>
        /// The WebBrowser control maintains a history list of all the folders
        /// visited during a session. You can use the 
        /// <see cref="NavigateForward"/> method to implement a drop-down menu 
        /// on a <b>Forward</b> button similar to the one in Windows Explorer, 
        /// which will allow your users to return to a folder in the 'forward'
        /// navigation history. 
        /// </remarks>
        /// 
        /// <param name="folder">
        /// The folder to navigate to.
        /// </param>
        /// 
        /// <exception cref="Exception">
        /// The requested folder is not present in the 
        /// <see cref="ShellView"/>'s 'forward' history.
        /// </exception>
        public void NavigateForward(ShellItem folder)
        {
            m_History.MoveForward(folder);
            m_CurrentFolder = folder;
            RecreateShellView();
            OnNavigated(new NavigatedEventArgs(m_CurrentFolder));
        }

        /// <summary>
        /// Navigates to the parent of the currently displayed folder.
        /// </summary>
        public void NavigateParent()
        {
            Navigate(m_CurrentFolder.Parent);
        }

        /// <summary>
        /// Navigates to the folder currently selected in the 
        /// <see cref="ShellView"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// If the <see cref="ShellView"/>'s <see cref="MultiSelect"/>
        /// property is set, and more than one item is selected in the
        /// ShellView, the first Folder found will be navigated to.
        /// </remarks>
        /// 
        /// <returns>
        /// <see langword="true"/> if a selected folder could be
        /// navigated to, <see langword="false"/> otherwise.
        /// </returns>
        public bool NavigateSelectedFolder()
        {
            ShellItem[] selected = SelectedItems;

            if (selected.Length > 0)
            {
                foreach (ShellItem i in selected)
                {
                    if (i.IsFolder)
                    {
                        Navigate(i);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Refreshes the contents of the <see cref="ShellView"/>.
        /// </summary>
        public void RefreshContents()
        {
            if (m_ComInterface != null) m_ComInterface.Refresh();
        }

        /// <summary>
        /// Begins a rename on the item currently selected in the 
        /// <see cref="ShellView"/>.
        /// </summary>
        public void RenameSelectedItem()
        {
            User32.EnumChildWindows(m_ShellViewWindow, RenameCallback, IntPtr.Zero);
        }

        /// <summary>
        /// Selects all items in the <see cref="ShellView"/>.
        /// </summary>
        public void SelectAll()
        {
            foreach (ShellItem item in ShellItem)
            {
                m_ComInterface.SelectItem(item.ILPidl, SVSI.SVSI_SELECT);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a new folder can be created in
        /// the folder currently being browsed by th <see cref="ShellView"/>.
        /// </summary>
        [Browsable(false)]
        public bool CanCreateFolder
        {
            get
            {
                return m_CurrentFolder.IsFileSystem && !m_CurrentFolder.IsReadOnly;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a previous page in navigation 
        /// history is available, which allows the <see cref="NavigateBack"/> 
        /// method to succeed. 
        /// </summary>
        [Browsable(false)]
        public bool CanNavigateBack
        {
            get { return m_History.CanNavigateBack; }
        }

        /// <summary>
        /// Gets a value indicating whether a subsequent page in navigation 
        /// history is available, which allows the <see cref="NavigateForward"/> 
        /// method to succeed. 
        /// </summary>
        [Browsable(false)]
        public bool CanNavigateForward
        {
            get { return m_History.CanNavigateForward; }
        }

        /// <summary>
        /// Gets a value indicating whether the folder currently being browsed
        /// by the <see cref="ShellView"/> has parent folder which can be
        /// navigated to by calling <see cref="NavigateParent"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanNavigateParent
        {
            get
            {
                return m_CurrentFolder != ShellItem.Desktop;
            }
        }

        /// <summary>
        /// Gets the control's underlying COM IShellView interface.
        /// </summary>
        [Browsable(false)]
        public IShellView ComInterface
        {
            get { return m_ComInterface; }
        }

        /// <summary>
        /// Gets/sets a <see cref="ShellItem"/> describing the folder 
        /// currently being browsed by the <see cref="ShellView"/>.
        /// </summary>
        [Editor(typeof(ShellItemEditor), typeof(UITypeEditor))]
        public ShellItem CurrentFolder
        {
            get { return m_CurrentFolder; }
            set
            {
                if (value != m_CurrentFolder)
                {
                    Navigate(value);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ShellView"/>'s navigation history.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ShellHistory History
        {
            get { return m_History; }
        }

        /// <summary>
        /// Gets a list of the items currently selected in the 
        /// <see cref="ShellView"/>
        /// </summary>
        /// 
        /// <value>
        /// A <see cref="ShellItem"/> array detailing the items currently 
        /// selected in the control. If no items are currently selected, 
        /// an empty array is returned. 
        /// </value>
        [Browsable(false)]
        public ShellItem[] SelectedItems
        {
            get
            {
                uint CFSTR_SHELLIDLIST =
                    User32.RegisterClipboardFormat("Shell IDList Array");
                ComTypes.IDataObject selection = GetSelectionDataObject();

                if (selection != null)
                {
                    FORMATETC format = new FORMATETC();
                    STGMEDIUM storage = new STGMEDIUM();

                    format.cfFormat = (short)CFSTR_SHELLIDLIST;
                    format.dwAspect = DVASPECT.DVASPECT_CONTENT;
                    format.lindex = 0;
                    format.tymed = TYMED.TYMED_HGLOBAL;

                    if (selection.QueryGetData(ref format) == 0)
                    {
                        selection.GetData(ref format, out storage);

                        int itemCount = Marshal.ReadInt32(storage.unionmember);
                        ShellItem[] result = new ShellItem[itemCount];

                        for (int n = 0; n < itemCount; ++n)
                        {
                            int offset = Marshal.ReadInt32(storage.unionmember,
                                8 + (n * 4));
                            result[n] = new ShellItem(
                                m_CurrentFolder,
                                (IntPtr)((int)storage.unionmember + offset));
                        }

                        GlobalFree(storage.unionmember);
                        return result;
                    }
                }
                return new ShellItem[0];
            }
        }

        /// <summary>
        /// Gets/sets a value indicating whether multiple items can be selected
        /// by the user.
        /// </summary>
        [DefaultValue(true), Category("Behaviour")]
        public bool MultiSelect
        {
            get { return m_MultiSelect; }
            set
            {
                m_MultiSelect = value;
                RecreateShellView();
                OnNavigated(new NavigatedEventArgs(ShellItem.Desktop));
            }
        }

        /// <summary>
        /// Gets/sets a value indicating whether a "WebView" is displayed on
        /// the left of the <see cref="ShellView"/> control.
        /// </summary>
        /// 
        /// <remarks>
        /// The WebView is a strip of HTML that appears to the left of a
        /// Windows Explorer window when the window has no Explorer Bar.
        /// It displays general system tasks and locations, as well as
        /// information about the items selected in the window.
        /// 
        /// <para>
        /// <b>Important:</b> When <see cref="ShowWebView"/> is set to 
        /// <see langref="true"/>, the <see cref="SelectionChanged"/>
        /// event will not occur. This is due to a limitation in the 
        /// underlying windows control.
        /// </para>
        /// </remarks>
        [DefaultValue(false), Category("Appearance")]
        public bool ShowWebView
        {
            get { return m_ShowWebView; }
            set
            {
                if (value != m_ShowWebView)
                {
                    m_ShowWebView = value;
                    m_Browser = null;
                    RecreateShellView();
                    OnNavigated(new NavigatedEventArgs(ShellItem.Desktop));
                }
            }
        }

        /// <summary>
        /// Gets/sets a <see cref="C:StatusBar"/> control that the 
        /// <see cref="ShellView"/> should use to display folder details.
        /// </summary>
        public StatusBar StatusBar
        {
            get { return ((ShellBrowser)GetShellBrowser()).StatusBar; }
            set { ((ShellBrowser)GetShellBrowser()).StatusBar = value; }
        }

        /// <summary>
        /// Gets or sets how items are displayed in the control. 
        /// </summary>
        [DefaultValue(ShellViewStyle.LargeIcon), Category("Appearance")]
        public ShellViewStyle View
        {
            get { return m_View; }
            set
            {
                m_View = value;
								IFolderView2 ifv = (IFolderView2)m_ComInterface;
								ifv.SetCurrentViewMode(value);
								ShellViewStyle viewStyle;
								Int32 thumbnailSize;
								ifv.GetViewModeAndIconSize(out viewStyle, out thumbnailSize);
								OnViewChanged(new ViewChangedEventArgs(viewStyle, thumbnailSize));
                //OnNavigated(new NavigatedEventArgs(ShellItem.Desktop));
            }
        }
				private Int32 m_ThumbnailSize;
			[DefaultValue(48), Category("Appearance")]
				public Int32 ThumbnailSize
				{
					set
					{
						IFolderView2 ifv = (IFolderView2)m_ComInterface;
						ifv.SetViewModeAndIconSize(View,value);
						m_ThumbnailSize = value;
						OnViewChanged(new ViewChangedEventArgs(View, value));
					}
					get
					{
						IFolderView2 ifv = (IFolderView2)m_ComInterface;
						if (ifv != null)
						{
							ShellViewStyle viewStyle;
							Int32 thumbnailSize;
							ifv.GetViewModeAndIconSize(out viewStyle, out thumbnailSize);
							return thumbnailSize;
						}
						else
						{
							return 48;
						}
					}
				}
			public List<LVItemColor> LVItemsColorCodes { get; set; }
			public IFolderView2 FolderView2
			{
				get
				{
					return (IFolderView2)m_ComInterface;
				}
			}

        /// <summary>
        /// Occurs when the <see cref="ShellView"/> control wants to know
        /// if it should include an item in its view.
        /// </summary>
        /// 
        /// <remarks>
        /// This event allows the items displayed in the <see cref="ShellView"/>
        /// control to be filtered. You may want to to only list files with
        /// a certain extension, for example.
        /// </remarks>
        public event FilterItemEventHandler FilterItem;

        /// <summary>
        /// Occurs when the <see cref="ShellView"/> control navigates to a 
        /// new folder.
        /// </summary>
        public event EventHandler<NavigatedEventArgs> Navigated;

        /// <summary>
        /// Occurs when the <see cref="ShellView"/> control is about to 
        /// navigate to a new folder.
        /// </summary>
        public event NavigatingEventHandler Navigating;

        /// <summary>
        /// Occurs when the <see cref="ShellView"/>'s current selection 
        /// changes.
        /// </summary>
        /// 
        /// <remarks>
        /// <b>Important:</b> When <see cref="ShowWebView"/> is set to 
        /// <see langref="true"/>, this event will not occur. This is due to 
        /// a limitation in the underlying windows control.
        /// </remarks>
        public event EventHandler SelectionChanged;

				public event EventHandler<ViewChangedEventArgs> ViewStyleChanged;

        #region Hidden Inherited Properties

        /// <summary>
        /// This property does not apply to <see cref="ShellView"/>.
        /// </summary>
        [Browsable(false)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        /// <summary>
        /// This property does not apply to <see cref="ShellView"/>.
        /// </summary>
        [Browsable(false)]
        public override Image BackgroundImage
        {
            get { return base.BackgroundImage; }
            set { base.BackgroundImage = value; }
        }

        /// <summary>
        /// This property does not apply to <see cref="ShellView"/>.
        /// </summary>
        [Browsable(false)]
        public override ImageLayout BackgroundImageLayout
        {
            get { return base.BackgroundImageLayout; }
            set { base.BackgroundImageLayout = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { m_PropertyChanged += value; }
            remove { m_PropertyChanged -= value; }
        }

        #endregion

        /// <summary>
        /// Overrides <see cref="Control.Dispose(bool)"/>
        /// </summary>
        /// 
        /// <param name="disposing"/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_ShellViewWindow != IntPtr.Zero)
                {
                    User32.DestroyWindow(m_ShellViewWindow);
                    m_ShellViewWindow = IntPtr.Zero;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates the actual shell view control.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            CreateShellView();
            OnNavigated(new NavigatedEventArgs(ShellItem.Desktop));
						
						
        }

        /// <summary>
        /// Overrides <see cref="Control.OnPreviewKeyDown"/>.
        /// </summary>
        /// 
        /// <param name="e"/>
        /// <returns/>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if ((e.KeyData == Keys.Up) || (e.KeyData == Keys.Down) ||
                (e.KeyData == Keys.Left) || (e.KeyData == Keys.Right) ||
                (e.KeyData == Keys.Enter) || (e.KeyData == Keys.Delete))
                e.IsInputKey = true;

            if (e.Control && e.KeyCode == Keys.A) SelectAll();

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Overrides <see cref="Control.OnResize"/>.
        /// </summary>
        /// 
        /// <param name="eventargs"/>
        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            User32.SetWindowPos(m_ShellViewWindow, IntPtr.Zero, 0, 0,
                ClientRectangle.Width, ClientRectangle.Height, 0);
        }

        /// <summary>
        /// Overrides <see cref="Control.PreProcessMessage"/>
        /// </summary>
        /// 
        /// <param name="msg"/>
        /// <returns/>
        public override bool PreProcessMessage(ref Message msg)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;
            Keys keyCode = (Keys)(int)msg.WParam & Keys.KeyCode;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_KEYUP))
            {
                switch (keyCode)
                {
                    case Keys.F2:
                        RenameSelectedItem();
                        return true;
                    case Keys.Delete:
                        DeleteSelectedItems();
                        return true;
                }
            }

            return base.PreProcessMessage(ref msg);
        }

        /// <summary>
        /// Overrides <see cref="Control.WndProc"/>
        /// </summary>
        /// <param name="m"/>
        protected override void WndProc(ref Message m)
        {
					if (m.Msg == 78)
					{

					}
            const int CWM_GETISHELLBROWSER = 0x407;

            // Windows 9x sends the CWM_GETISHELLBROWSER message and expects
            // the IShellBrowser for the window to be returned or an Access
            // Violation occurs. This is pseudo-documented in knowledge base 
            // article Q157247.
            if (m.Msg == CWM_GETISHELLBROWSER)
            {
                m.Result = Marshal.GetComInterfaceForObject(m_Browser,
                    typeof(IShellBrowser));
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        internal bool IncludeItem(IntPtr pidl)
        {
            if (FilterItem != null)
            {
                FilterItemEventArgs e = new FilterItemEventArgs(
                    new ShellItem(m_CurrentFolder, pidl));
                FilterItem(this, e);
                return e.Include;
            }
            else
            {
                return true;
            }
        }

        internal new void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
        }

        internal void OnSelectionChanged()
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }

        internal ShellItem ShellItem
        {
            get { return m_CurrentFolder; }
        }

				public IntPtr ShellListViewHandle
				{
					get
					{
						var handle = User32.FindWindowEx(m_ShellViewWindow, IntPtr.Zero, "SysListView32", "FolderView");
						return handle;
					}
				}

        void CreateShellView()
        {
            IShellView previous = m_ComInterface;
            Rectangle bounds = ClientRectangle;
            FOLDERSETTINGS folderSettings = new FOLDERSETTINGS();

            // Create an IShellView object.
            m_ComInterface = CreateViewObject(m_CurrentFolder, Handle);

            // Set the FOLDERSETTINGS.
            folderSettings.ViewMode = (FOLDERVIEWMODE)m_View;

            if (!m_ShowWebView)
            {
                folderSettings.fFlags |= FOLDERFLAGS.NOWEBVIEW;
            }

            if (!m_MultiSelect)
            {
                folderSettings.fFlags |= FOLDERFLAGS.SINGLESEL;
            }

            // Tell the IShellView object to create a view window and
            // activate it.
            try
            {
                m_ComInterface.CreateViewWindow(previous, ref folderSettings,
                                             GetShellBrowser(), ref bounds,
                                             out m_ShellViewWindow);
            }
            catch (COMException ex)
            {
                // If the operation was cancelled by the user (for example
                // because an empty removable media drive was selected, 
                // then "Cancel" pressed in the resulting dialog) convert
                // the exception into something more meaningfil.
                if (ex.ErrorCode == unchecked((int)0x800704C7U))
                {
                    throw new UserAbortException(ex);
                }
            }

            m_ComInterface.UIActivate(1);

            // Disable the window if in design mode, so that user input is
            // passed onto the designer.
            if (DesignMode)
            {
                User32.EnableWindow(m_ShellViewWindow, false);
            }

						
            // Destroy the previous view window.
            if (previous != null) previous.DestroyViewWindow();
        }

        void RecreateShellView()
        {
            if (m_ComInterface != null)
            {
                CreateShellView();
								this.subclassed.SysListviewhandle = this.ShellListViewHandle;
								this.subclassed.AssignHandle(this.m_ShellViewWindow);
                //OnNavigated(new NavigatedEventArgs(ShellItem.Desktop));
            }

            if (m_PropertyChanged != null)
            {
                m_PropertyChanged(this,
                    new PropertyChangedEventArgs("CurrentFolder"));
            }
        }

        bool RenameCallback(IntPtr hwnd, IntPtr lParam)
        {
            int itemCount = User32.SendMessage(hwnd,
                MSG.LVM_GETITEMCOUNT, 0, 0);

            for (int n = 0; n < itemCount; ++n)
            {
                LVITEMA item = new LVITEMA();
                item.mask = LVIF.LVIF_STATE;
                item.iItem = n;
                item.stateMask = LVIS.LVIS_SELECTED;
                User32.SendMessage(hwnd, MSG.LVM_GETITEMA,
                    0, ref item);

                if (item.state != 0)
                {
                    User32.SendMessage(hwnd, MSG.LVM_EDITLABEL, n, 0);
                    return false;
                }
            }

            return true;
        }

        ComTypes.IDataObject GetSelectionDataObject()
        {
            IntPtr result;

            if (m_ComInterface == null)
            {
                return null;
            }

            m_ComInterface.GetItemObject(SVGIO.SVGIO_SELECTION,
                typeof(ComTypes.IDataObject).GUID, out result);

            if (result != IntPtr.Zero)
            {
                ComTypes.IDataObject wrapped =
                    (ComTypes.IDataObject)
                        Marshal.GetTypedObjectForIUnknown(result,
                            typeof(ComTypes.IDataObject));
                return wrapped;
            }
            else
            {
                return null;
            }
        }

        IShellBrowser GetShellBrowser()
        {
            if (m_Browser == null)
            {
                if (m_ShowWebView)
                {
                    m_Browser = new ShellBrowser(this);
                }
                else
                {
                    m_Browser = new DialogShellBrowser(this);
                }
            }
            return m_Browser;
        }

        void OnNavigated(NavigatedEventArgs e)
        {
            if (Navigated != null)
            {
                Navigated(this, e);
            }
        }

				void OnViewChanged(ViewChangedEventArgs e)
				{
					if (ViewStyleChanged != null)
					{
						ViewStyleChanged(this, e);
					}
				}

        bool ShouldSerializeCurrentFolder()
        {
            return m_CurrentFolder != ShellItem.Desktop;
        }

        static IShellView CreateViewObject(ShellItem folder, IntPtr hwndOwner)
        {
            IntPtr result = folder.GetIShellFolder().CreateViewObject(hwndOwner,
                typeof(IShellView).GUID);
            return (IShellView)
                Marshal.GetTypedObjectForIUnknown(result,
                    typeof(IShellView));
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalFree(IntPtr hMem);

        bool m_MultiSelect;
        ShellHistory m_History;
        ShellBrowser m_Browser;
        ShellItem m_CurrentFolder;
        IShellView m_ComInterface;
        public IntPtr m_ShellViewWindow;
        bool m_ShowWebView;
        ShellViewStyle m_View;
        PropertyChangedEventHandler m_PropertyChanged;
    }

    /// <summary>
    /// Provides information for FilterItem events.
    /// </summary>
    public class FilterItemEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterItemEventArgs"/>
        /// class.
        /// </summary>
        /// 
        /// <param name="item">
        /// The item to be filtered.
        /// </param>
        internal FilterItemEventArgs(ShellItem item)
        {
            m_Item = item;
        }

        /// <summary>
        /// Gets/sets a value which will determine whether the item will be
        /// included in the <see cref="ShellView"/>.
        /// </summary>
        public bool Include
        {
            get { return m_Include; }
            set { m_Include = value; }
        }

        /// <summary>
        /// The item to be filtered.
        /// </summary>
        public ShellItem Item
        {
            get { return m_Item; }
        }

        ShellItem m_Item;
        bool m_Include = true;
    }

    /// <summary>
    /// Provides information for the <see cref="ShellView.Navigating"/>
    /// event.
    /// </summary>
    public class NavigatingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigatingEventArgs"/>
        /// class.
        /// </summary>
        /// 
        /// <param name="folder">
        /// The folder being navigated to.
        /// </param>
        public NavigatingEventArgs(ShellItem folder)
        {
            m_Folder = folder;
        }

        /// <summary>
        /// Gets/sets a value indicating whether the navigation should be
        /// cancelled.
        /// </summary>
        public bool Cancel
        {
            get { return m_Cancel; }
            set { m_Cancel = value; }
        }

        /// <summary>
        /// The folder being navigated to.
        /// </summary>
        public ShellItem Folder
        {
            get { return m_Folder; }
            set { m_Folder = value; }
        }

        bool m_Cancel;
        ShellItem m_Folder;
    }

		public class NavigatedEventArgs : EventArgs
		{
			ShellItem m_Folder;
			public NavigatedEventArgs(ShellItem folder)
			{
				m_Folder = folder;
			}
			/// <summary>
			/// The folder that is navigated to.
			/// </summary>
			public ShellItem Folder
			{
				get { return m_Folder; }
				set { m_Folder = value; }
			}
		}

		public class ViewChangedEventArgs : EventArgs
		{
			ShellViewStyle m_View;
			Int32 m_ThumbnailSize;
			public ViewChangedEventArgs(ShellViewStyle view, Int32? thumbnailSize)
			{
				m_View = view;
				if (thumbnailSize != null)
					m_ThumbnailSize = thumbnailSize.Value;
			}
			/// <summary>
			/// The current ViewStyle
			/// </summary>
			public ShellViewStyle CurrentView
			{
				get { return m_View; }
				set { m_View = value; }
			}

			public Int32 ThumbnailSize
			{
				get { return m_ThumbnailSize; }
				set { m_ThumbnailSize = value; }
			}
		}

    /// <summary>
    /// Exception raised when a user aborts a Shell operation.
    /// </summary>
    public class UserAbortException : ExternalException
    {

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="UserAbortException"/> class.
        /// </summary>
        /// 
        /// <param name="e">
        /// The inner exception.
        /// </param>
        public UserAbortException(Exception e)
            : base("User aborted", e)
        {
        }
    }

    /// <summary>
    /// Represents the method that will handle FilterItem events.
    /// </summary>
    public delegate void FilterItemEventHandler(object sender,
        FilterItemEventArgs e);

    /// <summary>
    /// Represents the method that will handle the 
    /// <see cref="ShellView.Navigating"/> event.
    /// </summary>
    public delegate void NavigatingEventHandler(object sender,
        NavigatingEventArgs e);
}
