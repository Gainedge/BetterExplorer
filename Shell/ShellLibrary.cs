//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell
{

    /// <summary>
    /// A Shell Library in the Shell Namespace
    /// </summary>
    public sealed class ShellLibrary : ShellItem, IList<ShellItem>
    {

        #region Private Fields

        private INativeShellLibrary nativeShellLibrary;
        private IKnownFolder knownFolder;

        private static Guid[] FolderTypesGuids =
        {
            new Guid(InterfaceGuids.GenericLibrary),
            new Guid(InterfaceGuids.DocumentsLibrary),
            new Guid(InterfaceGuids.MusicLibrary),
            new Guid(InterfaceGuids.PicturesLibrary),
            new Guid(InterfaceGuids.VideosLibrary)
        };

        #endregion Private Fields

        #region Private Constructor

        private ShellLibrary()
        {
        }

        //Construct the ShellLibrary object from a native Shell Library
        private ShellLibrary(INativeShellLibrary nativeShellLibrary)
            : this()
        {
            this.nativeShellLibrary = nativeShellLibrary;
        }

        /// <summary>
        /// Creates a shell library in the Libraries Known Folder,
        /// using the given IKnownFolder
        /// </summary>
        /// <param name="sourceKnownFolder">KnownFolder from which to create the new Shell Library</param>
        /// <param name="isReadOnly">If <B>true</B> , opens the library in read-only mode.</param>
        private ShellLibrary(IKnownFolder sourceKnownFolder, bool isReadOnly)
            : this()
        {
            Debug.Assert(sourceKnownFolder != null);

            // Keep a reference locally
            knownFolder = sourceKnownFolder;

            nativeShellLibrary = (INativeShellLibrary)new ShellLibraryCoClass();
            var flags = isReadOnly ? AccessModes.Read : AccessModes.ReadWrite;

            // Get the IShellItem2
            base.ComInterface = ((ShellItem)sourceKnownFolder).ComInterface;

            Guid guid = sourceKnownFolder.FolderId;

            // Load the library from the IShellItem2
            try
            {
                nativeShellLibrary.LoadLibraryFromKnownFolder(ref guid, flags);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid Library", "sourceKnownFolder");
            }
            catch (NotImplementedException)
            {
                throw new ArgumentException("Invalid Library", "sourceKnownFolder");
            }
        }

        #endregion Private Constructor

        #region Public Constructors

        /// <summary>
        /// Creates a shell library in the Libraries Known Folder,
        /// using the given shell library name.
        /// </summary>
        /// <param name="libraryName">The name of this library</param>
        /// <param name="overwrite">Allow overwriting an existing library; if one exists with the same name</param>
        public ShellLibrary(string libraryName, bool overwrite)
            : this()
        {
            if (string.IsNullOrEmpty(libraryName))
            {
                throw new ArgumentException("Library Name Empty!", "libraryName");
            }

            this.Name = libraryName;
            Guid guid = new Guid(InterfaceGuids.Libraries);

            var flags = overwrite ? LibrarySaveOptions.OverrideExisting : LibrarySaveOptions.FailIfThere;

            nativeShellLibrary = (INativeShellLibrary)new ShellLibraryCoClass();
            nativeShellLibrary.SaveInKnownFolder(ref guid, libraryName, flags, out m_ComInterface);
        }

        /// <summary>
        /// Creates a shell library in a given Known Folder,
        /// using the given shell library name.
        /// </summary>
        /// <param name="libraryName">The name of this library</param>
        /// <param name="sourceKnownFolder">The known folder</param>
        /// <param name="overwrite">Override an existing library with the same name</param>
        public ShellLibrary(string libraryName, IKnownFolder sourceKnownFolder, bool overwrite)
            : this(libraryName, overwrite)
        {
            /*
			if (string.IsNullOrEmpty(libraryName)) {
				throw new ArgumentException("Library Name Empty!", "libraryName");
			}

			knownFolder = sourceKnownFolder;

			this.Name = libraryName;
			Guid guid = knownFolder.FolderId;

			var flags = overwrite ? LibrarySaveOptions.OverrideExisting : LibrarySaveOptions.FailIfThere;

			nativeShellLibrary = (INativeShellLibrary)new ShellLibraryCoClass();
			nativeShellLibrary.SaveInKnownFolder(ref guid, libraryName, flags, out m_ComInterface);
			*/

            knownFolder = sourceKnownFolder;
        }

        /// <summary>
        /// Creates a shell library in a given local folder,
        /// using the given shell library name.
        /// </summary>
        /// <param name="libraryName">The name of this library</param>
        /// <param name="folderPath">The path to the local folder</param>
        /// <param name="overwrite">Override an existing library with the same name</param>
        public ShellLibrary(string libraryName, string folderPath, bool overwrite)
            : this()
        {
            if (string.IsNullOrEmpty(libraryName))
            {
                throw new ArgumentException("Library Name Empty!", "libraryName");
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException("Library Not Found!");
            }

            this.Name = libraryName;

            LibrarySaveOptions flags = overwrite ? LibrarySaveOptions.OverrideExisting : LibrarySaveOptions.FailIfThere;

            Guid guid = new Guid(InterfaceGuids.IShellItem);

            IShellItem shellItemIn = Shell32.SHCreateItemFromParsingName(folderPath, IntPtr.Zero, guid);

            nativeShellLibrary = (INativeShellLibrary)new ShellLibraryCoClass();
            nativeShellLibrary.Save(shellItemIn, libraryName, flags, out m_ComInterface);
        }

        #endregion Public Constructors

        #region Public Properties

        private string Name { get; set; }

        /// <summary>
        /// The Resource Reference to the icon.
        /// </summary>
        public IconReference IconResourceId
        {
            get
            {
                string iconRef;
                nativeShellLibrary.GetIcon(out iconRef);
                return new IconReference(iconRef);
            }
            set
            {
                nativeShellLibrary.SetIcon(value.ReferencePath);
                nativeShellLibrary.Commit();
            }
        }

        /// <summary>
        /// One of predefined Library types
        /// </summary>
        /// <exception cref="COMException">Will throw if no Library Type is set</exception>
        public LibraryFolderType LibraryType
        {
            get
            {
                Guid folderTypeGuid;
                nativeShellLibrary.GetFolderType(out folderTypeGuid);

                return GetFolderTypefromGuid(folderTypeGuid);
            }
            set
            {
                Guid guid = FolderTypesGuids[(int)value];
                nativeShellLibrary.SetFolderType(ref guid);
                nativeShellLibrary.Commit();
            }
        }

        private static LibraryFolderType GetFolderTypefromGuid(Guid folderTypeGuid)
        {
            for (int i = 0; i < FolderTypesGuids.Length; i++)
            {
                if (folderTypeGuid.Equals(FolderTypesGuids[i]))
                {
                    return (LibraryFolderType)i;
                }
            }
            throw new ArgumentOutOfRangeException("folderTypeGuid", "Invalid Library Type!");
        }

        /// <summary>
        /// By default, this folder is the first location
        /// added to the library. The default save folder
        /// is both the default folder where files can
        /// be saved, and also where the library XML
        /// file will be saved, if no other path is specified
        /// </summary>
        public string DefaultSaveFolder
        {
            get
            {
                var guid = new Guid(InterfaceGuids.IShellItem);

                IShellItem saveFolderItem;

                nativeShellLibrary.GetDefaultSaveFolder(
                        DefaultSaveFolderType.Detect,
                        ref guid,
                        out saveFolderItem);

                return Helpers.GetParsingName(saveFolderItem);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                else if (!Directory.Exists(value))
                    throw new DirectoryNotFoundException("Default save folder not found!");

                string fullPath = new DirectoryInfo(value).FullName;

                var guid = new Guid(InterfaceGuids.IShellItem);
                IShellItem saveFolderItem = Shell32.SHCreateItemFromParsingName(fullPath, IntPtr.Zero, guid);

                nativeShellLibrary.SetDefaultSaveFolder(DefaultSaveFolderType.Detect, saveFolderItem);
                nativeShellLibrary.Commit();
            }
        }

        /// <summary>
        /// Whether the library will be pinned to the
        /// Explorer Navigation Pane
        /// </summary>
        public bool IsPinnedToNavigationPane
        {
            get
            {
                var flags = LibraryOptions.PinnedToNavigationPane;
                nativeShellLibrary.GetOptions(out flags);
                return (flags & LibraryOptions.PinnedToNavigationPane) == LibraryOptions.PinnedToNavigationPane;
            }
            set
            {
                var flags = LibraryOptions.Default;

                if (value)
                    flags |= LibraryOptions.PinnedToNavigationPane;
                else
                    flags &= ~LibraryOptions.PinnedToNavigationPane;

                nativeShellLibrary.SetOptions(LibraryOptions.PinnedToNavigationPane, flags);
                nativeShellLibrary.Commit();
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Close the library, and release its associated file system resources
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        #endregion Public Methods

        #region Internal Properties

        internal const string FileExtension = ".library-ms";

        #endregion Internal Properties

        #region Static Shell Library methods

        /// <summary>
        /// Get a the known folder FOLDERID_Libraries
        /// </summary>
        public static IKnownFolder LibrariesKnownFolder => KnownFolderHelper.FromKnownFolderId(new Guid(InterfaceGuids.Libraries));

        private static ShellLibrary Load_Helper(IShellItem nativeShellItem, string libraryName, bool isReadOnly)
        {
            INativeShellLibrary nativeShellLibrary = (INativeShellLibrary)new ShellLibraryCoClass();
            var flags = isReadOnly ? AccessModes.Read : AccessModes.ReadWrite;
            nativeShellLibrary.LoadLibraryFromItem(nativeShellItem, flags);

            var library = new ShellLibrary(nativeShellLibrary);
            try
            {
                library.ComInterface = (IShellItem)nativeShellItem;
                library.Name = libraryName;
                return library;
            }
            catch
            {
                library.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load the library using a number of options
        /// </summary>
        /// <param name="libraryName">The name of the library</param>
        /// <param name="isReadOnly">If <B>true</B>, loads the library in read-only mode.</param>
        /// <returns>A ShellLibrary Object</returns>
        public static ShellLibrary Load(string libraryName, bool isReadOnly)
        {
            IKnownFolder kf = KnownFolders.Libraries;
            string librariesFolderPath = kf != null ? kf.Path : string.Empty;
            var guid = new Guid(InterfaceGuids.IShellItem);
            string shellItemPath = System.IO.Path.Combine(librariesFolderPath, libraryName + FileExtension);
            IShellItem nativeShellItem = Shell32.SHCreateItemFromParsingName(shellItemPath, IntPtr.Zero, guid);
            return Load_Helper(nativeShellItem, libraryName, isReadOnly);
        }

        /// <summary>
        /// Load the library using a number of options
        /// </summary>
        /// <param name="libraryName">The name of the library.</param>
        /// <param name="folderPath">The path to the library.</param>
        /// <param name="isReadOnly">If <B>true</B>, opens the library in read-only mode.</param>
        /// <returns>A ShellLibrary Object</returns>
        public static ShellLibrary Load(string libraryName, string folderPath, bool isReadOnly)
        {
            // Create the shell item path
            string shellItemPath = System.IO.Path.Combine(folderPath, libraryName + FileExtension);
            IShellItem nativeShellItem = new ShellItem(shellItemPath).ComInterface;
            return Load_Helper(nativeShellItem, libraryName, isReadOnly);
        }


        /// <summary>
        /// Load the library using a number of options
        /// </summary>
        /// <param name="nativeShellItem">IShellItem</param>
        /// <param name="isReadOnly">read-only flag</param>
        /// <returns>A ShellLibrary Object</returns>
        public static ShellLibrary FromShellItem(IShellItem nativeShellItem, bool isReadOnly)
        {
            var nativeShellLibrary = (INativeShellLibrary)new ShellLibraryCoClass();
            var flags = isReadOnly ? AccessModes.Read : AccessModes.ReadWrite;

            nativeShellLibrary.LoadLibraryFromItem(nativeShellItem, flags);
            var library = new ShellLibrary(nativeShellLibrary);
            library.ComInterface = (IShellItem)nativeShellItem;
            return library;
        }

        public static ShellLibrary FromShellItem(ShellItem shellItem, bool isReadOnly)
        {
            var nativeShellLibrary = (INativeShellLibrary)new ShellLibraryCoClass();
            var flags = isReadOnly ? AccessModes.Read : AccessModes.ReadWrite;

            nativeShellLibrary.LoadLibraryFromItem(shellItem.ComInterface, flags);
            var library = new ShellLibrary(nativeShellLibrary);
            library.ComInterface = (IShellItem)shellItem.ComInterface;
            library.Name = shellItem.DisplayName;
            return library;
        }


        /// <summary>
        /// Load the library using a number of options
        /// </summary>
        /// <param name="sourceKnownFolder">A known folder.</param>
        /// <param name="isReadOnly">If <B>true</B>, opens the library in read-only mode.</param>
        /// <returns>A ShellLibrary Object</returns>
        public static ShellLibrary Load(IKnownFolder sourceKnownFolder, bool isReadOnly)
        {
            return new ShellLibrary(sourceKnownFolder, isReadOnly);
        }

        private static void ShowManageLibraryUI(ShellLibrary shellLibrary, IntPtr windowHandle, string title, string instruction, bool allowAllLocations)
        {
            int hr = 0;

            //Thread staWorker = new Thread(() =>
            //{
                hr = Shell32.SHShowManageLibraryUI(
                        shellLibrary.ComInterface,
                        windowHandle,
                        title,
                        instruction,
                        allowAllLocations ?
                             LibraryManageDialogOptions.NonIndexableLocationWarning :
                             LibraryManageDialogOptions.Default);
            //});

            //staWorker.SetApartmentState(ApartmentState.STA);
            //staWorker.Start();
            //staWorker.Join();
        }

				public static void ShowManageLibraryUI(IShellItem shellLibrary, IntPtr windowHandle, string title, string instruction, bool allowAllLocations) {
					int hr = 0;

					//Thread staWorker = new Thread(() =>
					//{
					hr = Shell32.SHShowManageLibraryUI(
									shellLibrary,
									windowHandle,
									title,
									instruction,
									allowAllLocations ?
											 LibraryManageDialogOptions.NonIndexableLocationWarning :
											 LibraryManageDialogOptions.Default);
					//});

					//staWorker.SetApartmentState(ApartmentState.STA);
					//staWorker.Start();
					//staWorker.Join();
				}

		/// <summary>
		/// Shows the library management dialog which enables users to mange the library folders and default save location.
		/// </summary>
		/// <param name="libraryName">The name of the library</param>
		/// <param name="folderPath">The path to the library.</param>
		/// <param name="windowHandle">The parent window,or IntPtr.Zero for no parent</param>
		/// <param name="title">A title for the library management dialog, or null to use the library name as the title</param>
		/// <param name="instruction">An optional help string to display for the library management dialog</param>
		/// <param name="allowAllLocations">If true, do not show warning dialogs about locations that cannot be indexed</param>
		/// <remarks>If the library is already open in read-write mode, the dialog will not save the changes.</remarks>
		public static void ShowManageLibraryUI(string libraryName, string folderPath, IntPtr windowHandle, string title, string instruction, bool allowAllLocations)
        {
            // this method is not safe for MTA consumption and will blow
            // Access Violations if called from an MTA thread so we wrap this
            // call up into a Worker thread that performs all operations in a
            // single threaded apartment
            using (ShellLibrary shellLibrary = ShellLibrary.Load(libraryName, folderPath, true))
            {
                ShowManageLibraryUI(shellLibrary, windowHandle, title, instruction, allowAllLocations);
            }
        }

        /// <summary>
        /// Shows the library management dialog which enables users to mange the library folders and default save location.
        /// </summary>
        /// <param name="libraryName">The name of the library</param>
        /// <param name="windowHandle">The parent window,or IntPtr.Zero for no parent</param>
        /// <param name="title">A title for the library management dialog, or null to use the library name as the title</param>
        /// <param name="instruction">An optional help string to display for the library management dialog</param>
        /// <param name="allowAllLocations">If true, do not show warning dialogs about locations that cannot be indexed</param>
        /// <remarks>If the library is already open in read-write mode, the dialog will not save the changes.</remarks>
        public static void ShowManageLibraryUI(string libraryName, IntPtr windowHandle, string title, string instruction, bool allowAllLocations)
        {
            // this method is not safe for MTA consumption and will blow
            // Access Violations if called from an MTA thread so we wrap this
            // call up into a Worker thread that performs all operations in a
            // single threaded apartment
            using (ShellLibrary shellLibrary = ShellLibrary.Load(libraryName, true))
            {
                ShowManageLibraryUI(shellLibrary, windowHandle, title, instruction, allowAllLocations);
            }
        }

        /// <summary>
        /// Shows the library management dialog which enables users to mange the library folders and default save location.
        /// </summary>
        /// <param name="sourceKnownFolder">A known folder.</param>
        /// <param name="windowHandle">The parent window,or IntPtr.Zero for no parent</param>
        /// <param name="title">A title for the library management dialog, or null to use the library name as the title</param>
        /// <param name="instruction">An optional help string to display for the library management dialog</param>
        /// <param name="allowAllLocations">If true, do not show warning dialogs about locations that cannot be indexed</param>
        /// <remarks>If the library is already open in read-write mode, the dialog will not save the changes.</remarks>
        public static void ShowManageLibraryUI(IKnownFolder sourceKnownFolder, IntPtr windowHandle, string title, string instruction, bool allowAllLocations)
        {
            // this method is not safe for MTA consumption and will blow
            // Access Violations if called from an MTA thread so we wrap this
            // call up into a Worker thread that performs all operations in a
            // single threaded apartment
            using (ShellLibrary shellLibrary = ShellLibrary.Load(sourceKnownFolder, true))
            {
                ShowManageLibraryUI(shellLibrary, windowHandle, title, instruction, allowAllLocations);
            }
        }

        #endregion Static Shell Library methods

        #region Collection Members

        /// <summary>
        /// Add a new FileSystemFolder or SearchConnector
        /// </summary>
        /// <param name="item">The folder to add to the library.</param>
        public void Add(ShellItem item)
        {
            if (item == null) { throw new ArgumentNullException("item"); }

            nativeShellLibrary.AddFolder(item.ComInterface);
            nativeShellLibrary.Commit();
        }

        /// <summary>
        /// Add an existing folder to this library
        /// </summary>
        /// <param name="folderPath">The path to the folder to be added to the library.</param>
        public void Add(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException("Library not found!");
            }

            Add(new ShellItem(folderPath));
        }

        /// <summary>
        /// Clear all items of this Library
        /// </summary>
        public void Clear()
        {
            foreach (ShellItem folder in ItemsList)
            {
                nativeShellLibrary.RemoveFolder(folder.ComInterface);
            }

            nativeShellLibrary.Commit();
        }

        /// <summary>
        /// Remove a folder or search connector
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><B>true</B> if the item was removed.</returns>
        public bool Remove(ShellItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            try
            {
                nativeShellLibrary.RemoveFolder(item.ComInterface);
                nativeShellLibrary.Commit();
            }
            catch (COMException)
            {
                return false;
            }

            return true;
        }

        #endregion Collection Members

        #region Disposable Pattern

        /// <summary>
        /// Release resources
        /// </summary>
        /// <param name="disposing">Indicates that this was called from Dispose(), rather than from the finalizer.</param>
        public override void Dispose(bool disposing)
        {
            if (nativeShellLibrary != null)
            {
                Marshal.ReleaseComObject(nativeShellLibrary);
                nativeShellLibrary = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Release resources
        /// </summary>
        ~ShellLibrary()
        {
            Dispose(false);
        }

        #endregion Disposable Pattern

        #region Private Properties

        private List<ShellItem> ItemsList
        {
            get
            {
                List<ShellItem> list = new List<ShellItem>();
                IShellItemArray itemArray;

                Guid shellItemArrayGuid = new Guid(InterfaceGuids.IShellItemArray);

                HResult hr = nativeShellLibrary.GetFolders(LibraryFolderFilter.AllItems, ref shellItemArrayGuid, out itemArray);

                if (hr != HResult.S_OK)
                    return list;

                uint count;
                itemArray.GetCount(out count);

                for (uint i = 0; i < count; ++i)
                {
                    IShellItem shellItem;
                    itemArray.GetItemAt(i, out shellItem);
                    list.Add(new ShellItem(shellItem as IShellItem));
                }

                if (itemArray != null)
                {
                    Marshal.ReleaseComObject(itemArray);
                    itemArray = null;
                }

                return list;
            }
        }

        #endregion Private Properties

        #region IEnumerable<ShellFileSystemFolder> Members

        /// <summary>
        /// Retrieves the collection enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        new public IEnumerator<ShellItem> GetEnumerator() => ItemsList.GetEnumerator();


        #endregion IEnumerable<ShellFileSystemFolder> Members

        #region IEnumerable Members

        /// <summary>
        /// Retrieves the collection enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ItemsList.GetEnumerator();

        #endregion IEnumerable Members

        #region ICollection<ShellFileSystemFolder> Members

        /// <summary>
        /// Determines if an item with the specified path exists in the collection.
        /// </summary>
        /// <param name="fullPath">The path of the item.</param>
        /// <returns><B>true</B> if the item exists in the collection.</returns>
        public bool Contains(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException("fullPath");
            }

            return ItemsList.Any(folder => string.Equals(fullPath, folder.ParsingName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if a folder exists in the collection.
        /// </summary>
        /// <param name="item">The folder.</param>
        /// <returns><B>true</B>, if the folder exists in the collection.</returns>
        public bool Contains(ShellItem item)
        {
            if (item == null) throw new ArgumentNullException("item");
            return ItemsList.Any(folder => string.Equals(item.ParsingName, folder.ParsingName, StringComparison.OrdinalIgnoreCase));
        }

        #endregion ICollection<ShellFileSystemFolder> Members

        #region IList<FileSystemFolder> Members

        /// <summary>
        /// Searches for the specified FileSystemFolder and returns the zero-based index of the
        /// first occurrence within Library list.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The index of the item in the collection, or -1 if the item does not exist.</returns>
        public int IndexOf(ShellItem item) => ItemsList.IndexOf(item);


        /// <summary>
        /// Inserts a FileSystemFolder at the specified index.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="item">The FileSystemFolder to insert.</param>
        void IList<ShellItem>.Insert(int index, ShellItem item)
        {
            // Index related options are not supported by IShellLibrary doesn't support them.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes an item at the specified index.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        void IList<ShellItem>.RemoveAt(int index)
        {
            // Index related options are not supported by IShellLibrary doesn't support them.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the folder at the specified index
        /// </summary>
        /// <param name="index">The index of the folder to retrieve.</param>
        /// <returns>A folder.</returns>
        public ShellItem this[int index]
        {
            get { return ItemsList[index]; }
            set
            {
                // Index related options are not supported by IShellLibrary
                // doesn't support them.
                throw new NotImplementedException();
            }
        }

        #endregion IList<FileSystemFolder> Members

        #region ICollection<ShellFileSystemFolder> Members

        /// <summary>
        /// Copies the collection to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index in the array at which to start the copy.</param>
        void ICollection<ShellItem>.CopyTo(ShellItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The count of the items in the list.
        /// </summary>
        public int Count => ItemsList.Count;

        /// <summary>
        /// Indicates whether this list is read-only or not.
        /// </summary>
        public bool IsReadOnly => false;

        #endregion ICollection<ShellFileSystemFolder> Members
    }
}