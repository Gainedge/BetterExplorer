// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SDK.Samples.VistaBridge.Interop;
using System.Runtime.InteropServices;
using Microsoft.SDK.Samples.VistaBridge.Library.KnownFolders;
using System.Threading;
using System.IO;
using Windows7.DesktopIntegration.Interop;


namespace Windows7.DesktopIntegration
{
    /// <summary>
    /// This type wraps a shell library object and serves as a focal point to manage a specific library instance 
    /// </summary>
    public class ShellLibrary : IDisposable   
    {
        private IShellLibrary _shellLibrary;
        private string _name;

        internal ShellLibrary(IShellLibrary shellLibrary, string name)
        {
            _shellLibrary = shellLibrary;
            _name = name;
        }

        /// <summary>
        /// Create a Shell Library and return a <see cref="ShellLibrary"/> object
        /// </summary>
        /// <param name="name">The library name</param>
        /// <param name="folderToSaveIn">The folder (library) to add the new library to</param>
        /// <returns>A <see cref="ShellLibrary"/> object</returns>
        public static ShellLibrary Create(string name, string folderToSaveIn)
        {
            IShellLibrary newLibrary = new ShellLibraryClass();
            IShellItem folderToSaveInShellItem = Helpers.GetShellItemFromPath(folderToSaveIn);
            IShellItem savesToShellItem;
            newLibrary.Save(folderToSaveInShellItem, name, Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYSAVEFLAGS.LSF_OVERRIDEEXISTING, out savesToShellItem);
            newLibrary.Commit();

            string baseName = System.IO.Path.Combine(folderToSaveIn, name);
            string fullName = System.IO.Path.ChangeExtension(baseName, ".library-ms");

            ShellLibrary shellLibrary = new ShellLibrary(newLibrary, fullName);
            Marshal.ReleaseComObject(savesToShellItem);
            return shellLibrary;
        }

        /// <summary>
        /// Create a Shell Library and return a <see cref="ShellLibrary"/> object
        /// </summary>
        /// <param name="name">The library name</param>
        /// <param name="isPinnedToNavigationPane">Whether the library is pinned to the Explorer window navigatin Pane </param>
        /// <returns>A <see cref="ShellLibrary"/> object</returns>
        public static ShellLibrary Create(string name, bool isPinnedToNavigationPane)
        {
            IShellLibrary newLibrary = new ShellLibraryClass();
            Guid libraryfolderId = new Guid(KFIDGuid.Libraries);
            IShellItem savesToShellItem;
            newLibrary.SaveInKnownFolder(ref libraryfolderId, name, Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYSAVEFLAGS.LSF_OVERRIDEEXISTING, out savesToShellItem);
            newLibrary.Commit();

            string fullName = CreateLibraryFullName(name);
            ShellLibrary shellLibrary = new ShellLibrary(newLibrary, fullName);
            Marshal.ReleaseComObject(savesToShellItem);
            shellLibrary.IsPinnedToNavigationPane = isPinnedToNavigationPane;
            return shellLibrary;
        }

        /// <summary>
        /// Create a Shell Library and return a <see cref="ShellLibrary"/> object
        /// </summary>
        /// <param name="name">The library name</param>
        /// <returns>A <see cref="ShellLibrary"/> object</returns>
        public static ShellLibrary Create(string name)
        {
            return Create(name, false);
        }


        /// <summary>
        /// Load an existing library and create a <see cref="ShellLibrary"/> object that enables 
        /// the management of this library
        /// </summary>
        /// <param name="path">The library to load.</param>
        /// <param name="isWritable">Define the access code to the library</param>
        /// <returns>A <see cref="ShellLibrary"/> object</returns>
        public static ShellLibrary Load(string path, bool isWritable)
        {
            IShellLibrary library = new ShellLibraryClass();
            IShellItem pathShellItem = Helpers.GetShellItemFromPath(path);
            library.LoadLibraryFromItem(pathShellItem, isWritable ? Windows7.DesktopIntegration.Interop.SafeNativeMethods.StorageInstantiationModes.STGM_READWRITE : Windows7.DesktopIntegration.Interop.SafeNativeMethods.StorageInstantiationModes.STGM_READ);
            ShellLibrary shellLibrary = new ShellLibrary(library, path);
            return shellLibrary;
        }


        /// <summary>
        /// Load an existing library and create a <see cref="ShellLibrary"/> object that enables 
        /// the management of this library from a known folder location
        /// </summary>
        /// <param name="knownFolderLibrary">The known folder library to load</param>
        /// <param name="isWritable">Define the access code to the library</param>
        /// <returns>A <see cref="ShellLibrary"/> object</returns>
        public static ShellLibrary Load(KnownFolder knownFolderLibrary, bool isWritable)
        {
            IShellLibrary library = new ShellLibraryClass();
            Guid folderId = knownFolderLibrary.FolderId;
            library.LoadLibraryFromKnownFolder(ref folderId, isWritable ? Windows7.DesktopIntegration.Interop.SafeNativeMethods.StorageInstantiationModes.STGM_READWRITE : Windows7.DesktopIntegration.Interop.SafeNativeMethods.StorageInstantiationModes.STGM_READ);
            string fullName = System.IO.Path.ChangeExtension(knownFolderLibrary.Path, ".library-ms");
            ShellLibrary shellLibrary = new ShellLibrary(library, fullName);
            return shellLibrary;
        }


        /// <summary>
        /// Add new folder to a library
        /// </summary>
        /// <param name="path">The folder to add</param>
        public void AddFolder(string path)
        {

            IShellItem folderShellItem = Helpers.GetShellItemFromPath(path);
            _shellLibrary.AddFolder(folderShellItem);
            _shellLibrary.Commit();
        }

        /// <summary>
        /// Remove a folder from a library
        /// </summary>
        /// <param name="path">The folder to remove</param>
        public void RemoveFolder(string path)
        {
            IShellItem folderShellItem = Helpers.GetShellItemFromPath(path);
            _shellLibrary.RemoveFolder(folderShellItem);
            _shellLibrary.Commit();
        }

        /// <summary>
        /// Set and Get the default save folder
        /// </summary>
        public string DefaultSaveFolder
        {
            get
            {
                IShellItem defaultSaveFoldrShellItem = null;
                try
                {
                    Guid shellItemGuid = new Guid(Microsoft.SDK.Samples.VistaBridge.Interop.IIDGuid.IShellItem);

                    try
                    {
                        _shellLibrary.GetDefaultSaveFolder(Windows7.DesktopIntegration.Interop.SafeNativeMethods.DEFAULTSAVEFOLDERTYPE.DSFT_DETECT, ref shellItemGuid, out defaultSaveFoldrShellItem);
                    }
                    catch
                    {
                        //There is no default save folder (This is the initial state)
                        return String.Empty;
                    }

                    IShellItem resolvedFolder;
                    try
                    {
                        _shellLibrary.ResolveFolder(defaultSaveFoldrShellItem, 1000, ref shellItemGuid, out resolvedFolder);
                        Marshal.ReleaseComObject(defaultSaveFoldrShellItem);
                        defaultSaveFoldrShellItem = resolvedFolder;
                    }
                    catch
                    {
                        //If the library was opened as read only, we will fail to resolve the folder 
                    }

                    string filePath;
                    defaultSaveFoldrShellItem.GetDisplayName(Microsoft.SDK.Samples.VistaBridge.Interop.SafeNativeMethods.SIGDN.SIGDN_FILESYSPATH, out filePath);

                    return filePath;
                }
                finally
                {
                    if (defaultSaveFoldrShellItem != null)
                        Marshal.ReleaseComObject(defaultSaveFoldrShellItem);
                }
            }

            set
            {
                IShellItem defaultSaveFoldrShellItem = Helpers.GetShellItemFromPath(value);
                try
                {
                    _shellLibrary.SetDefaultSaveFolder(Windows7.DesktopIntegration.Interop.SafeNativeMethods.DEFAULTSAVEFOLDERTYPE.DSFT_DETECT, defaultSaveFoldrShellItem);
                    _shellLibrary.Commit();
                }
                finally
                {
                    if (defaultSaveFoldrShellItem != null)
                        Marshal.ReleaseComObject(defaultSaveFoldrShellItem);
                }
            }
        }
        
        /// <summary>
        /// Return all folders in a Library
        /// <remarks>This method requires a write access to the library to be able to reflect a folder move or delete change</remarks>
        /// </summary>
        /// <returns>A list of folders</returns>
        public IList<string> GetFolders()
        {
            IShellItemArray itemArray;

            Guid shellItemArrayGuid = new Guid(Microsoft.SDK.Samples.VistaBridge.Interop.IIDGuid.IShellItemArray);
            try
            {
                _shellLibrary.GetFolders(Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYFOLDERFILTER.LFF_ALLITEMS, ref shellItemArrayGuid, out itemArray);
            }
            catch
            {
                return new List<string>();
            }

            uint count;
            itemArray.GetCount(out count);

            List<string> result = new List<string>((int)count);

            for (uint i = 0; i < count; ++i)
            {
                string filePath;
                IShellItem shellItem;
                itemArray.GetItemAt(i, out shellItem);

                try
                {
                    IShellItem resolvedShellItem;
                    Guid shellItemGuid = new Guid(Microsoft.SDK.Samples.VistaBridge.Interop.IIDGuid.IShellItem);
                    _shellLibrary.ResolveFolder(shellItem, 1000, ref shellItemGuid, out resolvedShellItem);
                    resolvedShellItem.GetDisplayName(Microsoft.SDK.Samples.VistaBridge.Interop.SafeNativeMethods.SIGDN.SIGDN_FILESYSPATH, out filePath);
                    Marshal.ReleaseComObject(resolvedShellItem);
                }
                catch
                {
                    //If we can't resolve the folder, we will return the original shell item
                    shellItem.GetDisplayName(Microsoft.SDK.Samples.VistaBridge.Interop.SafeNativeMethods.SIGDN.SIGDN_FILESYSPATH, out filePath);
                }
                
                Marshal.ReleaseComObject(shellItem);
                result.Add(filePath);
            }
            Marshal.ReleaseComObject(itemArray);

            return result;
        }

        /// <summary>
        /// Helper function to create a file name for a library under the default Libraries folder
        /// </summary>
        /// <param name="name">the short library name</param>
        /// <returns>The full library path</returns>
        public static string CreateLibraryFullName(string name)
        {
            string baseName = Path.Combine(KnownFolders.Libraries, name);
            return Path.ChangeExtension(baseName, ".library-ms");
        }

        /// <summary>
        /// Get or set (rename) the library short name
        /// </summary>
        public string Name 
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_name);
            }
            set
            {
                string directory = Path.GetDirectoryName(_name);
                string ext = Path.GetExtension(_name);
                string newBaseName = Path.Combine(directory, value);
                string newName = Path.ChangeExtension(newBaseName, ext);

                File.Move(_name, newName);
                _name = newName;
            }
        }

        /// <summary>
        /// Get or set (rename) the library Full (path) name
        /// </summary>
        public string FullName
        {
            get
            {
                return _name;
            }
            set
            {
                File.Move(_name, value);
                _name = value;
            }
        }

        /// <summary>
        /// Delete the library
        /// <remarks>This method delete the shell library and Dispose the .NET ShellLibrary instance</remarks>
        /// </summary>
        public void Delete()
        {
            File.Delete(_name);
            Dispose();
        }

        /// <summary>
        /// Delete a library
        /// </summary>
        /// <param name="libraryPath">the .library-ms file to delete</param>
        public static void Delete(string libraryPath)
        {
            File.Delete(libraryPath);
        }

        /// <summary>
        /// Get and Set the library Icon. 
        /// The format is a string that contains the resource Dll and the resource index
        /// </summary>
        public string Icon
        {
            get
            {
                try
                {
                    string icon;
                    _shellLibrary.GetIcon(out icon);
                    return icon;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                _shellLibrary.SetIcon(value);
                _shellLibrary.Commit();
            }
        }

        /// <summary>
        /// Set or Get whether the library is pinned to the Explorer window navigatin Pane 
        /// </summary>
        public bool IsPinnedToNavigationPane
        {
            get
            {
                Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYOPTIONFLAGS lofOptions;

                _shellLibrary.GetOptions(out lofOptions);

                return (lofOptions & Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYOPTIONFLAGS.LOF_PINNEDTONAVPANE) != 0;
            }
            set
            {
                _shellLibrary.SetOptions(Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYOPTIONFLAGS.LOF_PINNEDTONAVPANE, value ? Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYOPTIONFLAGS.LOF_PINNEDTONAVPANE : Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYOPTIONFLAGS.LOF_DEFAULT);
                _shellLibrary.Commit();
            }
        }

        /// <summary>
        /// Get &amp; Set the folder type id
        /// </summary>
        public Guid FolderTypeId
        {
            get
            {
                Guid folderTypeId = Guid.Empty;

                try
                {
                    _shellLibrary.GetFolderType(out folderTypeId);
                }
                catch
                {
                }

                return folderTypeId;
            }
            set
            {
                _shellLibrary.SetFolderType(ref value);
                _shellLibrary.Commit();
            }
        }

        /// <summary>
        /// Shows the library management dialog which enables users to mange the library folders and default save location.
        /// </summary>
        /// <param name="path">The path to the library</param>
        /// <param name="hwndOwner">The parent windows</param>
        /// <param name="title">A title for the library management dialog, or NULL to use the library name as the title</param>
        /// <param name="instruction">An optional help string to display for the library management dialog</param>
        /// <param name="allowAllLocations">If true, do not show warning dialogs about locations that cannot be indexed</param>
        /// <returns>true if the user cliked O.K, false if the user clicked Cancel</returns>
        public static bool ShowManageLibraryUI(string path, IntPtr hwndOwner, string title, string instruction, bool allowAllLocations)
        {
            // this method is not safe for MTA consumption and will blow
            // Access Violations if called from an MTA thread so we wrap this
            // call up into a Worker thread that performs all operations in a
            // single threaded apartment
            
            int hr = 0;

            Thread staWorker = new Thread( ()=>
                {
                    IShellItem library = null;
                    try
                    {
                        library = Helpers.GetShellItemFromPath(path);
                        hr = NativeLibraryMethods.SHShowManageLibraryUI(library, hwndOwner, title, instruction, allowAllLocations ? Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYMANAGEDIALOGOPTIONS.LMD_NOUNINDEXABLELOCATIONWARNING : Windows7.DesktopIntegration.Interop.SafeNativeMethods.LIBRARYMANAGEDIALOGOPTIONS.LMD_DEFAULT);
                    }
                    catch (Exception ex)
                    {
                        hr = Marshal.GetHRForException(ex);
                    }
                    finally
                    {
                        if (library != null)
                            Marshal.ReleaseComObject(library);
                    }
                });
            
            staWorker.SetApartmentState(ApartmentState.STA);
            staWorker.Start();
            staWorker.Join();
           
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);

            return hr != 1; //1 is the value of S_FAIL 
        }

        /// <summary>
        /// Shows the library management dialog which enables users to mange the library folders and default save location.
        /// </summary>
        /// <param name="knownFolder">A known folder based library</param>
        /// <param name="hwndOwner">The parent windows</param>
        /// <param name="title">A title for the library management dialog, or NULL to use the library name as the title</param>
        /// <param name="instruction">An optional help string to display for the library management dialog</param>
        /// <param name="allowAllLocations">If true, do not show warning dialogs about locations that cannot be indexed</param>
        /// <returns>true if the user cliked O.K, false if the user clicked Cancel</returns>
        public static bool ShowManageLibraryUI(KnownFolder knownFolder, IntPtr hwndOwner, string title, string instruction, bool allowAllLocations)
        {
            return ShowManageLibraryUI(knownFolder.Path, hwndOwner, title, instruction, allowAllLocations);
        }

        #region IDisposable Members
        /// <summary>
        /// Relese the IShellLibrary COM object
        /// </summary>
        public void Dispose()
        {
            Marshal.ReleaseComObject(_shellLibrary);
            _shellLibrary = null;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}