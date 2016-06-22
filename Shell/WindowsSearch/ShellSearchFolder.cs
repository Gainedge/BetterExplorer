//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell
{

    /// <summary>
    /// Create and modify search folders.
    /// </summary>
    public class ShellSearchFolder : ShellItem
    {

        #region Locals

        private string[] searchScopePaths;
        private SearchCondition searchCondition;
        private ISearchFolderItemFactory NativeSearchFolderItemFactory { get; set; }


        #endregion Locals

        #region Constructors

        /// <summary>
        /// Create a simple search folder. Once the appropriate parameters are set,
        /// the search folder can be enumerated to get the search results.
        /// </summary>
        /// <param name="searchCondition">Specific condition on which to perform the search (property and expected value)</param>
        /// <param name="searchScopePath">List of folders/paths to perform the search on. These locations need to be indexed by the system.</param>
        public ShellSearchFolder(SearchCondition searchCondition, params ShellItem[] searchScopePath)
        {
            NativeSearchFolderItemFactory = (ISearchFolderItemFactory)new SearchFolderItemFactoryCoClass();
            this.SearchCondition = searchCondition;

            if (searchScopePath != null && searchScopePath.Length > 0 && searchScopePath[0] != null)
            {
                this.SearchScopePaths = searchScopePath.Select(cont => cont.ParsingName);
            }
            ComInterface = this.m_SearchComInterface;
        }

        /*
		/// <summary>
		/// Create a simple search folder. Once the appropiate parameters are set,
		/// the search folder can be enumerated to get the search results.
		/// </summary>
		/// <param name="searchCondition">Specific condition on which to perform the search (property and expected value)</param>
		/// <param name="searchScopePath">List of folders/paths to perform the search on. These locations need to be indexed by the system.</param>
		public ShellSearchFolder(SearchCondition searchCondition, params string[] searchScopePath) {
			ComInterface = this.m_SearchComInterface;
			NativeSearchFolderItemFactory = (ISearchFolderItemFactory)new SearchFolderItemFactoryCoClass();
			this.SearchCondition = searchCondition;

			if (searchScopePath != null && searchScopePath.Length > 0 && searchScopePath[0] != null) {
				this.SearchScopePaths = searchScopePath;
			}
		}
		*/
        #endregion Constructors

        public IShellItem m_SearchComInterface
        {
            get
            {
                IShellItem shellItem;
                var guid = new Guid(InterfaceGuids.IShellItem);
                int hr = NativeSearchFolderItemFactory.GetShellItem(ref guid, out shellItem);
                if (hr != 0) throw Marshal.GetExceptionForHR((int)hr);
                return shellItem;
            }
        }

        /// <summary>
        /// Gets the Microsoft.WindowsAPICodePack.Shell.SearchCondition of the search.
        /// When this property is not set, the resulting search will have no filters applied.
        /// </summary>
        public SearchCondition SearchCondition
        {
            get { return searchCondition; }
            private set
            {
                searchCondition = value;
                NativeSearchFolderItemFactory.SetCondition(searchCondition.NativeSearchCondition);
            }
        }

        /// <summary>
        /// Gets the search scope, as specified using an array of locations to search.
        /// The search will include this location and all its subcontainers. The default is FOLDERID_Profile
        /// </summary>
        public IEnumerable<string> SearchScopePaths
        {
            get
            {
                foreach (var scopePath in searchScopePaths)
                {
                    yield return scopePath;
                }
            }
            private set
            {
                searchScopePaths = value.ToArray();
                var shellItems = new List<IShellItem>(searchScopePaths.Length);

                Guid shellItemGuid = new Guid(InterfaceGuids.IShellItem);
                Guid shellItemArrayGuid = new Guid(InterfaceGuids.IShellItemArray);

                // Create IShellItem for all the scopes we were given
                foreach (string path in searchScopePaths)
                {
                    IShellItem scopeShellItem = Shell32.SHCreateItemFromParsingName(path, IntPtr.Zero, shellItemGuid);
                    if (scopeShellItem != null) shellItems.Add(scopeShellItem);
                }

                // Create a new IShellItemArray
                IShellItemArray scopeShellItemArray = new ShellItemArray(shellItems.ToArray());

                // Set the scope on the native ISearchFolderItemFactory
                HResult hResult = NativeSearchFolderItemFactory.SetScope(scopeShellItemArray);

                if (hResult != HResult.S_OK) throw new Exception(hResult.ToString());
            }
        }

        /// <summary>
        /// Creates a list of stack keys, as specified. If this method is not called,
        /// by default the folder will not be stacked.
        /// </summary>
        /// <param name="canonicalNames">Array of canonical names for properties on which the folder is stacked.</param>
        /// <exception cref="System.ArgumentException">If one of the given canonical names is invalid.</exception>
        public void SetStacks(params string[] canonicalNames)
        {
            if (canonicalNames == null) throw new ArgumentNullException("canonicalNames");
            List<PROPERTYKEY> propertyKeyList = new List<PROPERTYKEY>();

            foreach (string prop in canonicalNames)
            {
                // Get the PropertyKey using the canonicalName passed in
                PROPERTYKEY propKey;
                int result = PropertySystemNativeMethods.PSGetPropertyKeyFromName(prop, out propKey);
                if (result == 0) throw new ArgumentException("", "canonicalNames", Marshal.GetExceptionForHR(result));
                propertyKeyList.Add(propKey);
            }

            if (propertyKeyList.Count > 0)
            {
                SetStacks(propertyKeyList.ToArray());
            }
        }

        /// <summary>
        /// Creates a list of stack keys, as specified. If this method is not called,
        /// by default the folder will not be stacked.
        /// </summary>
        /// <param name="propertyKeys">Array of property keys on which the folder is stacked.</param>
        public void SetStacks(params PROPERTYKEY[] propertyKeys)
        {
            if (propertyKeys?.Length > 0)
            {
                NativeSearchFolderItemFactory.SetStacks((uint)propertyKeys.Length, propertyKeys);
            }

            //if (propertyKeys != null && propertyKeys.Length > 0) {
            //	NativeSearchFolderItemFactory.SetStacks((uint)propertyKeys.Length, propertyKeys);
            //}
        }

        /// <summary>
        /// Sets the search folder display name.
        /// </summary>
        public void SetDisplayName(string displayName)
        {
            HResult hr = NativeSearchFolderItemFactory.SetDisplayName(displayName);
            if (hr != HResult.S_OK) throw new Exception(hr.ToString());
        }

        /// <summary>
        /// Sets the search folder icon size.
        /// The default settings are based on the FolderTypeID which is set by the
        /// SearchFolder::SetFolderTypeID method.
        /// </summary>
        public void SetIconSize(int value)
        {
            HResult hr = NativeSearchFolderItemFactory.SetIconSize(value);
            if (hr != HResult.S_OK) throw new Exception(hr.ToString());
        }

        /// <summary>
        /// Sets a search folder type ID, as specified.
        /// </summary>
        public void SetFolderTypeID(Guid value)
        {
            HResult hr = NativeSearchFolderItemFactory.SetFolderTypeID(value);
            if (hr != HResult.S_OK) throw new Exception(hr.ToString());
        }

        /// <summary>
        /// Sets folder logical view mode. The default settings are based on the FolderTypeID which is set
        /// by the SearchFolder::SetFolderTypeID method.
        /// </summary>
        /// <param name="mode">The logical view mode to set.</param>
        public void SetFolderLogicalViewMode(FolderLogicalViewMode mode)
        {
            HResult hr = NativeSearchFolderItemFactory.SetFolderLogicalViewMode(mode);
            if (hr != HResult.S_OK) throw new Exception(hr.ToString());
        }
		
        /// <summary>
        /// Creates a new column list whose columns are all visible,
        /// given an array of PropertyKey structures. The default is based on FolderTypeID.
        /// </summary>
        /// <remarks>This property may not work correctly with the ExplorerBrowser control.</remarks>
        public void SetVisibleColumns(PROPERTYKEY[] value)
        {
            HResult hr = NativeSearchFolderItemFactory.SetVisibleColumns(value == null ? 0 : (uint)value.Length, value);
            if (hr != HResult.S_OK) throw Marshal.GetExceptionForHR((int)hr);
        }

        ///// <summary>
        ///// Creates a list of sort column directions, as specified.
        ///// </summary>
        ///// <remarks>This property may not work correctly with the ExplorerBrowser control.</remarks>
        //public void SortColumns(SortColumn[] value)
        //{
        //		//HResult hr = NativeSearchFolderItemFactory.SetSortColumns(value == null ? 0 : (uint)value.Length, value);

        //		//if (!CoreErrorHelper.Succeeded(hr))
        //		//{
        //		//		throw new ShellException(LocalizedMessages.ShellSearchFolderUnableToSetSortColumns, Marshal.GetExceptionForHR((int)hr));
        //		//}
        //}

        ///// <summary>
        ///// Sets a group column, as specified. If no group column is specified, no grouping occurs.
        ///// </summary>
        ///// <remarks>This property may not work correctly with the ExplorerBrowser control.</remarks>
        ////public void SetGroupColumn(PropertyKey propertyKey)
        ////{
        ////		HResult hr = NativeSearchFolderItemFactory.SetGroupColumn(ref propertyKey);

        ////		if (!CoreErrorHelper.Succeeded(hr)) { throw new ShellException(hr); }
        ////}
    }
}