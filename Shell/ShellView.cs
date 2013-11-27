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
using BExplorer.Shell.Interop;
using ComTypes = System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace BExplorer.Shell
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetColumnbyIndex(IShellView view, bool isAll, int index, out PROPERTYKEY res);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetItemName(IFolderView2 view, int index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetItemLocation(IShellView view, int index, out int pointx, out int pointy);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetColumnInShellView(IShellView view, int count, [MarshalAs(UnmanagedType.LPArray)] PROPERTYKEY[] pk);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetSortColumns(IFolderView2 view, int count, [MarshalAs(UnmanagedType.LPArray)] SORTCOLUMN[] pk);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GetColumnInfobyIndex(IShellView view, bool isAll, int index, out CM_COLUMNINFO res);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int GetColumnInfobyPK(IShellView view, bool isAll, PROPERTYKEY pk, out CM_COLUMNINFO res);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void SetColumnInfobyPK(IShellView view, PROPERTYKEY pk, CM_COLUMNINFO cm);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GetSortColumns(IShellView view, int index, out SORTCOLUMN sc);

        private IntPtr BEHDLL { get; set; }
        public GetItemName _GetItemName;
        public GetItemLocation _GetItemLocation;
        public SetColumnInShellView _SetColumnInShellView;
        public SetSortColumns _SetSortColumns;
        public GetColumnInfobyIndex _GetColumnInfobyIndex;
        public GetColumnInfobyPK _GetColumnInfobyPK;
      public SetColumnInfobyPK _SetColumnInfobyPK;
        public GetSortColumns _GetSortColumns;
        public GetColumnbyIndex _GetColumnbyIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellView"/> class.
        /// </summary>
        public ShellView()
        {
            this.DoubleBuffered = true;
            m_History = new ShellHistory();
            m_MultiSelect = true;
            m_View = ShellViewStyle.LargeIcon;
            Size = new System.Drawing.Size(250, 200);
            Navigate(ShellItem.Desktop);
            BEHDLL = Kernel32.LoadLibrary(Kernel32.Is64bitProcess(Process.GetCurrentProcess()) ? "BEH64.dll" : "BEH32.dll");

            IntPtr GetItemNameP = Kernel32.GetProcAddress(BEHDLL, "GetItemName");
            _GetItemName = (GetItemName)Marshal.GetDelegateForFunctionPointer(GetItemNameP, typeof(GetItemName));

            IntPtr GetItemLocationP = Kernel32.GetProcAddress(BEHDLL, "GetItemLocation");
            _GetItemLocation = (GetItemLocation)Marshal.GetDelegateForFunctionPointer(GetItemLocationP, typeof(GetItemLocation));


            IntPtr SetColumnInShellViewP = Kernel32.GetProcAddress(BEHDLL, "SetColumnInShellView");
            _SetColumnInShellView = (SetColumnInShellView)Marshal.GetDelegateForFunctionPointer(SetColumnInShellViewP, typeof(SetColumnInShellView));


            IntPtr SetSortColumnsP = Kernel32.GetProcAddress(BEHDLL, "SetSortColumns");
            _SetSortColumns = (SetSortColumns)Marshal.GetDelegateForFunctionPointer(SetSortColumnsP, typeof(SetSortColumns));


            IntPtr GetColumnInfobyIndexP = Kernel32.GetProcAddress(BEHDLL, "GetColumnInfobyIndex");
            _GetColumnInfobyIndex = (GetColumnInfobyIndex)Marshal.GetDelegateForFunctionPointer(GetColumnInfobyIndexP, typeof(GetColumnInfobyIndex));


            IntPtr GetColumnInfobyPKP = Kernel32.GetProcAddress(BEHDLL, "GetColumnInfobyPK");
            _GetColumnInfobyPK = (GetColumnInfobyPK)Marshal.GetDelegateForFunctionPointer(GetColumnInfobyPKP, typeof(GetColumnInfobyPK));

						IntPtr SetColumnInfobyPKP = Kernel32.GetProcAddress(BEHDLL, "SetColumnInfobyPK");
						_SetColumnInfobyPK = (SetColumnInfobyPK)Marshal.GetDelegateForFunctionPointer(SetColumnInfobyPKP, typeof(SetColumnInfobyPK));

            IntPtr GetSortColumnsP = Kernel32.GetProcAddress(BEHDLL, "GetSortColumns");
            _GetSortColumns = (GetSortColumns)Marshal.GetDelegateForFunctionPointer(GetSortColumnsP, typeof(GetSortColumns));

            IntPtr GetColumnbyIndexP = Kernel32.GetProcAddress(BEHDLL, "GetColumnbyIndex");
            _GetColumnbyIndex = (GetColumnbyIndex)Marshal.GetDelegateForFunctionPointer(GetColumnbyIndexP, typeof(GetColumnbyIndex));
						Type comType = Type.GetTypeFromCLSID(Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748"));
						ICP = (IColumnProvider)Activator.CreateInstance(comType);
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

				public Dictionary<String, string> FolderSizes = new Dictionary<string, string>();

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

            GC.Collect();
            Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

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
            //foreach (ShellItem item in ShellItem)
            //{
            //    m_ComInterface.SelectItem(item.ILPidl, SVSI.SELECT);
            //}
            LVITEMA item = new LVITEMA();
            item.mask = LVIF.LVIF_STATE;
            item.stateMask = LVIS.LVIS_SELECTED;
            item.state = LVIS.LVIS_SELECTED;
            User32.SendMessage(this.ShellListViewHandle, MSG.LVM_SETITEMSTATE, -1, ref item);
        }

        public void SelectItems(ShellItem[] ShellObjectArray)
        {
            IntPtr pIDL = IntPtr.Zero;
            IFolderView ifv = this.FolderView2;

            IntPtr[] PIDLArray = new IntPtr[ShellObjectArray.Length];
            int i = 0;

            foreach (ShellItem item in ShellObjectArray)
            {
                uint iAttribute;
                Shell32.SHParseDisplayName(item.ParsingName.Replace(@"\\", @"\"), IntPtr.Zero, out pIDL, (uint)0, out iAttribute);

                if (pIDL != IntPtr.Zero)
                {
                    IntPtr pIDLRltv = Shell32.ILFindLastID(item.Pidl);
                    if (pIDLRltv != IntPtr.Zero)
                    {
                        PIDLArray[i] = pIDLRltv;
                    }
                }

                i++;
            }
            NativePoint pt = new NativePoint(0, 0);
            ifv.SelectAndPositionItems((uint)ShellObjectArray.Length, PIDLArray, ref pt, SVSI.SELECT | SVSI.ENSUREVISIBLE | SVSI.FOCUSED | SVSI.DESELECTOTHERS);
        }

        public void DoRename()
        {

            //IsRenameStarted = true;
            m_ComInterface.SelectItem(SelectedItems[0].ILPidl, SVSI.SELECT | SVSI.DESELECTOTHERS |
                    SVSI.EDIT);

        }

        public void EditFile(string Filename)
        {
            Shell32.SHELLEXECUTEINFO info = new Shell32.SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "edit";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            Shell32.ShellExecuteEx(ref info);
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
        [Browsable(false)]
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
                List<ShellItem> items = new List<Shell.ShellItem>();
                var iArray = GetSelectedItemsArray();
                if (iArray != null)
                {
                    try
                    {

                        uint itemCount = 0;
                        iArray.GetCount(out itemCount);
                        for (uint index = 0; index < itemCount; index++)
                        {
                            IShellItem iShellItem = null;
                            iArray.GetItemAt(index, out iShellItem);
                            items.Add(new ShellItem(iShellItem));
                        }

                    }
                    finally
                    {
                        Marshal.ReleaseComObject(iArray);
                    }

                }
                return items.ToArray();
                //uint CFSTR_SHELLIDLIST =
                //    User32.RegisterClipboardFormat("Shell IDList Array");
                //ComTypes.IDataObject selection = GetSelectionDataObject();

                //if (selection != null)
                //{
                //    FORMATETC format = new FORMATETC();
                //    STGMEDIUM storage = new STGMEDIUM();

                //    format.cfFormat = (short)CFSTR_SHELLIDLIST;
                //    format.dwAspect = DVASPECT.DVASPECT_CONTENT;
                //    format.lindex = 0;
                //    format.tymed = TYMED.TYMED_HGLOBAL;

                //    if (selection.QueryGetData(ref format) == 0)
                //    {
                //        selection.GetData(ref format, out storage);

                //        int itemCount = Marshal.ReadInt32(storage.unionmember);
                //        ShellItem[] result = new ShellItem[itemCount];

                //        for (int n = 0; n < itemCount; ++n)
                //        {
                //            int offset = Marshal.ReadInt32(storage.unionmember,
                //                8 + (n * 4));
                //            result[n] = new ShellItem(
                //                m_CurrentFolder,
                //                (IntPtr)((int)storage.unionmember + offset));
                //        }

                //        GlobalFree(storage.unionmember);
                //        return result;
                //    }
                //}
                //return new ShellItem[0];
            }
        }

        internal IShellItemArray GetSelectedItemsArray()
        {
            IShellItemArray iArray = null;
            IFolderView2 iFV2 = this.FolderView2;
            if (iFV2 != null)
            {
                try
                {
                    Guid iidShellItemArray = new Guid(InterfaceGuids.IShellItemArray);
                    object oArray = null;
                    HResult hr = iFV2.Items((uint)SVGIO.SVGIO_SELECTION, ref iidShellItemArray, out oArray);
                    iArray = oArray as IShellItemArray;

                }
                finally
                {
                    //Marshal.ReleaseComObject(iFV2);
                    //iFV2 = null;
                }
            }

            return iArray;
        }

				[Browsable(false)]
				public ShellItem[] Items
				{
					get
					{
						List<ShellItem> items = new List<Shell.ShellItem>();
						var iArray = GetItemsArray();
						if (iArray != null)
						{
							try
							{

								uint itemCount = 0;
								iArray.GetCount(out itemCount);
								for (uint index = 0; index < itemCount; index++)
								{
									IShellItem iShellItem = null;
									iArray.GetItemAt(index, out iShellItem);
									items.Add(new ShellItem(iShellItem));
								}

							}
							finally
							{
								Marshal.ReleaseComObject(iArray);
							}

						}
						return items.ToArray();
						//uint CFSTR_SHELLIDLIST =
						//    User32.RegisterClipboardFormat("Shell IDList Array");
						//ComTypes.IDataObject selection = GetSelectionDataObject();

						//if (selection != null)
						//{
						//    FORMATETC format = new FORMATETC();
						//    STGMEDIUM storage = new STGMEDIUM();

						//    format.cfFormat = (short)CFSTR_SHELLIDLIST;
						//    format.dwAspect = DVASPECT.DVASPECT_CONTENT;
						//    format.lindex = 0;
						//    format.tymed = TYMED.TYMED_HGLOBAL;

						//    if (selection.QueryGetData(ref format) == 0)
						//    {
						//        selection.GetData(ref format, out storage);

						//        int itemCount = Marshal.ReadInt32(storage.unionmember);
						//        ShellItem[] result = new ShellItem[itemCount];

						//        for (int n = 0; n < itemCount; ++n)
						//        {
						//            int offset = Marshal.ReadInt32(storage.unionmember,
						//                8 + (n * 4));
						//            result[n] = new ShellItem(
						//                m_CurrentFolder,
						//                (IntPtr)((int)storage.unionmember + offset));
						//        }

						//        GlobalFree(storage.unionmember);
						//        return result;
						//    }
						//}
						//return new ShellItem[0];
					}
				}

				internal IShellItemArray GetItemsArray()
				{
					IShellItemArray iArray = null;
					IFolderView2 iFV2 = this.FolderView2;
					if (iFV2 != null)
					{
						try
						{
							Guid iidShellItemArray = new Guid(InterfaceGuids.IShellItemArray);
							object oArray = null;
							HResult hr = iFV2.Items((uint)SVGIO.SVGIO_SELECTION, ref iidShellItemArray, out oArray);
							iArray = oArray as IShellItemArray;

						}
						finally
						{
							//Marshal.ReleaseComObject(iFV2);
							//iFV2 = null;
						}
					}

					return iArray;
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
                if (value == ShellViewStyle.Content)
                {
                    ifv.SetCurrentFolderFlags(FOLDERFLAGS.EXTENDEDTILES, FOLDERFLAGS.EXTENDEDTILES);
                    ifv.SetCurrentViewMode(ShellViewStyle.Tile);
                }
                else
                {
                    ifv.SetCurrentFolderFlags(FOLDERFLAGS.EXTENDEDTILES, 0);
                    ifv.SetCurrentViewMode(value);
                }

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
                ifv.SetViewModeAndIconSize(View, value);
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
        /// Gets or sets whether checkboxes are displayed to multi-select items.
        /// </summary>
        public bool ShowCheckboxes
        {
            get
            {
                IFolderView2 ifv = (IFolderView2)m_ComInterface;
                uint sch;
                ifv.GetCurrentFolderFlags(out sch);
                if ((sch & (uint)FOLDERFLAGS.CHECKSELECT) == (uint)FOLDERFLAGS.CHECKSELECT)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                IFolderView2 ifv = (IFolderView2)m_ComInterface;
                if (value == true)
                {
                    ifv.SetCurrentFolderFlags(FOLDERFLAGS.CHECKSELECT, FOLDERFLAGS.CHECKSELECT);
                }
                else
                {
                    ifv.SetCurrentFolderFlags(FOLDERFLAGS.CHECKSELECT, 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets if single-click-open mode is used.
        /// 
        /// In this mode, a single-click will open a file. A file is selected on hover.
        /// </summary>
        public bool SingleClickOpenMode
        {
            get
            {
                IFolderView2 ifv = (IFolderView2)m_ComInterface;
                uint sch;
                ifv.GetCurrentFolderFlags(out sch);
                if ((sch & (uint)FOLDERFLAGS.SINGLECLICKACTIVATE) == (uint)FOLDERFLAGS.SINGLECLICKACTIVATE)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                IFolderView2 ifv = (IFolderView2)m_ComInterface;
                if (value == true)
                {
                    ifv.SetCurrentFolderFlags(FOLDERFLAGS.SINGLECLICKACTIVATE, FOLDERFLAGS.SINGLECLICKACTIVATE);
                }
                else
                {
                    ifv.SetCurrentFolderFlags(FOLDERFLAGS.SINGLECLICKACTIVATE, 0);
                }
            }
        }

        /// <summary>
        /// Occurs when the control gains focus
        /// </summary>
        public event EventHandler GotFocus;

        /// <summary>
        /// Occurs when the control loses focus
        /// </summary>
        public event EventHandler LostFocus;

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

				public IColumnProvider ICP { get; set; }
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
            if (e.KeyCode == Keys.F2) RenameSelectedItem();

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
        //  public override bool PreProcessMessage(ref Message msg)
        //  {
        //      const int WM_KEYDOWN = 0x100;
        //      const int WM_KEYUP = 0x101;
        //      Keys keyCode = (Keys)(int)msg.WParam & Keys.KeyCode;

        //      if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_KEYUP))
        //      {
        //	
        //          switch (keyCode)
        //          {
        //              case Keys.F2:
        //                  RenameSelectedItem();
        //                  return true;
        //              case Keys.Delete:
        //                  DeleteSelectedItems();
        //                  return true;
        //          }
        //      }
        //if (msg.Msg == (int)WM.WM_MOUSEWHEEL || msg.Msg == (int)WM.WM_VSCROLL)
        //{
        //	return true;
        //}

        ////var hr = ((IInputObject)this.GetShellBrowser()).TranslateAcceleratorIO(ref msg);
        //return base.PreProcessMessage(ref msg);
        //  }

        //   /// <summary>
        //   /// Overrides <see cref="Control.WndProc"/>
        //   /// </summary>
        //   /// <param name="m"/>
        //   protected override void WndProc(ref Message m)
        //   {
        //
        //       const int CWM_GETISHELLBROWSER = 0x407;

        //       // Windows 9x sends the CWM_GETISHELLBROWSER message and expects
        //       // the IShellBrowser for the window to be returned or an Access
        //       // Violation occurs. This is pseudo-documented in knowledge base 
        //       // article Q157247.
        //       if (m.Msg == CWM_GETISHELLBROWSER)
        //       {
        //           m.Result = Marshal.GetComInterfaceForObject(m_Browser,
        //               typeof(IShellBrowser));
        //       }
        //       else
        //       {
        //           base.WndProc(ref m);
        //       }
        //   }

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

        internal void OnGotFocus()
        {
            if (GotFocus != null)
            {
                GotFocus(this, EventArgs.Empty);
            }
        }

        internal void OnLostFocus()
        {
            if (LostFocus != null)
            {
                LostFocus(this, EventArgs.Empty);
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

            folderSettings.fFlags |= FOLDERFLAGS.AUTOARRANGE | FOLDERFLAGS.SNAPTOGRID;

            // Tell the IShellView object to create a view window and
            // activate it.
            try
            {
                m_ComInterface.CreateViewWindow(previous, ref folderSettings,
                                             GetShellBrowser(), ref bounds,
                                             out m_ShellViewWindow);
                Task.Run(() =>
                {
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        AvailableVisibleColumns = AvailableColumns(false);
                        if (this.AllAvailableColumns == null)
                        {
                            this.AllAvailableColumns = AvailableColumns(true);
                        }
                    }));
                });
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
        public SubclassListView lv;
        void RecreateShellView()
        {
					this.FolderSizes.Clear();
            if (m_ComInterface != null)
            {
                CreateShellView();
                this.subclassed.SysListviewhandle = this.ShellListViewHandle;
                this.subclassed.AssignHandle(this.m_ShellViewWindow);
                lv.lvhandle = this.ShellListViewHandle;
                lv.AssignHandle(this.ShellListViewHandle);
								
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

        public void ShowFileProperties()
        {
            if (Shell32.SHMultiFileProperties(GetSelectionDataObject(), 0) != 0 /*S_OK*/)
            {
                throw new Win32Exception();
            }
        }
        private const int SW_SHOW = 5;
        private const uint SEE_MASK_INVOKEIDLIST = 12;
        public void ShowFileProperties(string Filename)
        {
            Shell32.SHELLEXECUTEINFO info = new Shell32.SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = Filename;
            info.nShow = SW_SHOW;
            info.fMask = SEE_MASK_INVOKEIDLIST;
            Shell32.ShellExecuteEx(ref info);
        }

        public void DeSelectAllItems()
        {
            this.FolderView2.SelectItem(-1, (uint)SVSI.DESELECTOTHERS);
        }

        public void SelectItem(int Index)
        {
            this.FolderView2.SelectItem(Index, (uint)SVSI.DESELECTOTHERS);
        }

        public void InvertSelection()
        {
            for (int i = 0; i < GetItemsCount(); i++)
            {
                IFolderView2 folderView2 = this.FolderView2;
                IntPtr pidl;
                folderView2.Item(i, out pidl);
                SVSI state;
                folderView2.GetSelectionState(pidl, out state);
                if (state == SVSI.DESELECT || state == SVSI.FOCUSED &&
                        state != SVSI.SELECT)
                {
                    this.m_ComInterface.SelectItem(pidl, SVSI.SELECT);
                }
                else
                {
                    this.m_ComInterface.SelectItem(pidl, SVSI.DESELECT);
                }

            }
        }

        public Collumns[] AvailableColumns(bool All)
        {
            try
            {

                IColumnManager icm = (IColumnManager)m_ComInterface;
                uint columncount = 0;
                HResult res = icm.GetColumnCount(All ? CM_ENUM_FLAGS.CM_ENUM_ALL : CM_ENUM_FLAGS.CM_ENUM_VISIBLE, out columncount);
                PROPERTYKEY[] pk = new PROPERTYKEY[columncount];
                res = icm.GetColumns(All ? CM_ENUM_FLAGS.CM_ENUM_ALL : CM_ENUM_FLAGS.CM_ENUM_VISIBLE, pk, columncount);
                List<Collumns> colls = new List<Collumns>();
                foreach (PROPERTYKEY item in pk)
                {
                    CM_COLUMNINFO ci = new CM_COLUMNINFO();
                    _GetColumnInfobyPK(m_ComInterface, All, item, out ci);
                    Collumns curCol = new Collumns();
                    curCol.Name = ci.wszName;
                    curCol.pkey = item;
                    curCol.Width = (int)ci.uIdealWidth;
                    colls.Add(curCol);
                }

						if (All)
						{
							Type comType = Type.GetTypeFromCLSID(Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748"));
							IColumnProvider ici = (IColumnProvider)Activator.CreateInstance(comType);
							SHCOLUMNINFO shi = new SHCOLUMNINFO();
							ici.GetColumnInfo(0, out shi);
							Collumns curCol = new Collumns();
							curCol.Name = shi.wszTitle;
							curCol.pkey = new PROPERTYKEY() { fmtid = shi.scid.fmtid, pid = (int)shi.scid.pid };
							curCol.Width = (int)shi.vt;
							curCol.IsColumnHandler = true;
							colls.Add(curCol);
							Marshal.ReleaseComObject(ici);
						}
                return colls.ToArray();
            }
            catch (Exception)
            {

                return new Collumns[0];
            }
        }

        public void GetGroupColInfo(out PROPERTYKEY pk, out bool Asc)
        {
            try
            {
                this.FolderView2.GetGroupBy(out pk, out Asc);

            }
            catch (Exception)
            {
                pk = new PROPERTYKEY();
                Asc = false;
            }
        }

        public void GetSortColInfo(out SORTCOLUMN ci)
        {
            try
            {

                int SortColsCount = -1;
                this.FolderView2.GetSortColumnCount(out SortColsCount);
                SORTCOLUMN[] sc = new SORTCOLUMN[SortColsCount];
                if (SortColsCount > 0)
                {
                    this.FolderView2.GetSortColumns(sc, SortColsCount); //_GetSortColumns(GetShellView(), 0, out sc);
                }
                ci = sc[0];
            }
            catch (Exception)
            {
                SORTCOLUMN sc = new SORTCOLUMN();
                ci = sc;
            }
        }

        public void SetGroupCollumn(PROPERTYKEY pk, bool Asc)
        {
            IFolderView2 ifv2 = this.FolderView2;
            IntPtr scptr = Marshal.AllocHGlobal(Marshal.SizeOf(pk));
            Marshal.StructureToPtr(pk, scptr, false);
            ifv2.SetGroupBy(scptr, Asc);
            Marshal.FreeHGlobal(scptr);
        }

        public void SetSortCollumn(PROPERTYKEY pk, SORT Order)
        {

            IFolderView2 ifv2 = this.FolderView2;
            SORTCOLUMN sc = new SORTCOLUMN() { propkey = pk, direction = Order };
            IntPtr scptr = Marshal.AllocHGlobal(Marshal.SizeOf(sc));
            Marshal.StructureToPtr(sc, scptr, false);
            ifv2.SetSortColumns(scptr, 1);
            Marshal.FreeHGlobal(scptr);
        }

				public void SetColInView(PROPERTYKEY pk, bool Remove, bool IsColumnHandler = false)
        {

            if (!Remove)
            {
                PROPERTYKEY[] pkk = new PROPERTYKEY[AvailableVisibleColumns.Length + 1];
                for (int i = 0; i < AvailableVisibleColumns.Length; i++)
                {
                    pkk[i] = AvailableVisibleColumns[i].pkey;
                }

                pkk[AvailableVisibleColumns.Length] = pk;

                IColumnManager icm = (IColumnManager)m_ComInterface;
                icm.SetColumns(pkk, (uint)AvailableVisibleColumns.Length + 1);
                //_SetColumnInShellView(GetShellView(), AvailableVisibleColumns.Length + 1, pkk);
						

                AvailableVisibleColumns = AvailableColumns(false);

						if (IsColumnHandler)
						{
							LVCOLUMN col = new LVCOLUMN();
							col.cchTextMax = 256;
							col.cx = 60;
							col.fmt = LVCFMT.LEFT;
							col.iSubItem = 4;
							col.mask = LVCF.LVCF_FMT | LVCF.LVCF_SUBITEM | LVCF.LVCF_TEXT | LVCF.LVCF_WIDTH;
							col.pszText = "Folder Size";
							var k = User32.SendMessage(this.ShellListViewHandle, MSG.LVM_SETCOLUMN, 4, ref col);
						}
            }
            else
            {
                PROPERTYKEY[] pkk = new PROPERTYKEY[AvailableVisibleColumns.Length - 1];
                int j = 0;
                for (int i = 0; i < AvailableVisibleColumns.Length; i++)
                {
                    if (!(AvailableVisibleColumns[i].pkey.fmtid == pk.fmtid && AvailableVisibleColumns[i].pkey.pid == pk.pid))
                    {
                        pkk[j] = AvailableVisibleColumns[i].pkey;
                        j++;
                    }

                }

                IColumnManager icm = (IColumnManager)m_ComInterface;
                icm.SetColumns(pkk, (uint)AvailableVisibleColumns.Length - 1);
                //_SetColumnInShellView(GetShellView(), AvailableVisibleColumns.Length - 1, pkk);

                AvailableVisibleColumns = AvailableColumns(false);
            }
        }

				public void AddCustomColumn(String header, object value)
				{
					//Type comType = Type.GetTypeFromCLSID(Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748"));
					//IColumnProvider ici = (IColumnProvider)Activator.CreateInstance(comType);
					//LPCSHCOLUMNINIT lpi = new LPCSHCOLUMNINIT();
					//lpi.wszFolder = "C:\\\0";
					//ici.Initialize(lpi);
					//SHCOLUMNINFO shi = new SHCOLUMNINFO();
					//ici.GetColumnInfo(1, out shi);
					//LPCSHCOLUMNID lid = new LPCSHCOLUMNID();
					//lid.fmtid = Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748");
					//lid.pid = 1;
					//LPCSHCOLUMNDATA ldata = new LPCSHCOLUMNDATA();
					//ldata.dwFileAttributes = (uint)FileAttributes.Directory;
					//ldata.wszFile = "C:\\boost154\0";
					//object o = null;
					//ici.GetItemData(lid, ldata, out o);

					//PROPERTYKEY[] pkk = new PROPERTYKEY[AvailableVisibleColumns.Length + 1];
					//for (int i = 0; i < AvailableVisibleColumns.Length; i++)
					//{
					//	pkk[i] = AvailableVisibleColumns[i].pkey;
					//}

					//pkk[AvailableVisibleColumns.Length] = new PROPERTYKEY() { fmtid = Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748"), pid = 1 };

					//IColumnManager icm = (IColumnManager)m_ComInterface;
					//icm.SetColumns(pkk, (uint)AvailableVisibleColumns.Length + 1);
					////_SetColumnInShellView(GetShellView(), AvailableVisibleColumns.Length + 1, pkk);

					//AvailableVisibleColumns = AvailableColumns(false);



				}

        /// <summary>
        /// Returns the number of the items in current view
        /// </summary>
        /// <returns></returns>
        public int GetItemsCount()
        {
            int itemsCount = 0;

            //IFolderView2 iFV2 = GetFolderView2();
            if (this.FolderView2 != null)
            {
                try
                {
                    HResult hr = this.FolderView2.ItemCount((uint)SVGIO.SVGIO_ALLVIEW, out itemsCount);

                    //if (hr != HResult.Ok &&
                    //		hr != HResult.ElementNotFound &&
                    //		hr != HResult.Fail)
                    //{
                    //	throw;
                    //}
                }
                finally
                {
                    //Marshal.ReleaseComObject(iFV2);
                    //iFV2 = null;
                }
            }

            return itemsCount;
        }

        [Browsable(false)]
        public Collumns[] AvailableVisibleColumns
        {
            get;
            set;
        }

        public Collumns[] AllAvailableColumns
        {
            get;
            set;
        }

        public IShellBrowser GetShellBrowser()
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
					//this.FolderSizes.Clear();
					LPCSHCOLUMNINIT lpi = new LPCSHCOLUMNINIT();
						lpi.wszFolder = e.Folder.ParsingName + "\0";
						if (ICP != null)
						{
							Task.Run(() =>
							{
								this.BeginInvoke(new MethodInvoker(() =>
									{
										this.FolderSizes.Clear();
										ICP.Initialize(lpi);
										foreach (var item in this.CurrentFolder)
										{
											var pn = item.ParsingName;
												LPCSHCOLUMNID lid = new LPCSHCOLUMNID();
												lid.fmtid = Guid.Parse("04DAAD08-70EF-450E-834A-DCFAF9B48748");
												lid.pid = 0;
												LPCSHCOLUMNDATA ldata = new LPCSHCOLUMNDATA();
												ldata.dwFileAttributes = item.IsFolder ? (uint)FileAttributes.Directory : 0;
												//ldata.dwFileAttributes = (uint)FileAttributes.Directory;
												if (!item.IsFolder)
												{
													ldata.pwszExt = Path.GetExtension(item.ParsingName);
												}
												ldata.wszFile = pn + "\0";
												object o = 0;
												this.ICP.GetItemData(lid, ldata, out o);
												if (o != null)
												{
													if (this.FolderSizes.ContainsKey(pn))
														this.FolderSizes[pn] = o.ToString();
													else
														this.FolderSizes.Add(pn, o.ToString());
												}

												Thread.Sleep(1);
												Application.DoEvents();

										}
									}));
							});
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
