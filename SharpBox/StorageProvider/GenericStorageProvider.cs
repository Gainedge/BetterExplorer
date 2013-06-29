using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using System.IO;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider
{
    /// <summary>
    /// The generic storage provider class implements platform independent logic as 
    /// base for all managed storage provider. All platform specific logic has to be implemented 
    /// in the storage provider service interface
    /// </summary>
    public class GenericStorageProvider : ICloudStorageProvider, ICloudStorageProviderInternal
    {
        /// <summary>
        /// A specific implementation of a storage service interface which contains
        /// all provider specific logic
        /// </summary>
        protected IStorageProviderService _Service;

        /// <summary>
        /// A provider specific implementation of a session
        /// </summary>
        protected IStorageProviderSession _Session;

        /// <summary>
        /// The constructure need a specific service implementation
        /// </summary>
        /// <param name="service"></param>
        public GenericStorageProvider(IStorageProviderService service)
        {
            _Service = service;
        }

        #region ICloudStorageProvider Members        

        /// <summary>
        /// This method opens a session for the implemented storage provider based on an existing
        /// security token.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken Open(ICloudStorageConfiguration configuration, ICloudStorageAccessToken token)
        {
            // Verify the compatibility of the credentials
            if (!_Service.VerifyAccessTokenType(token))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidCredentialsOrConfiguration);

            // create a new session
            _Session = _Service.CreateSession(token, configuration);

            // check the session
            if (_Session == null)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidCredentialsOrConfiguration);

            // return the accesstoken token
            return _Session.SessionToken;
        }

        /// <summary>
        /// This method closes the established session to a service provider
        /// </summary>
        public void Close()
        {
            // close the session
            _Service.CloseSession(_Session);

            // remove reference
            _Session = null;
        }

        /// <summary>
        /// This methid returns the root node of the virtual filesystem which 
        /// is abstracted by SharpBox
        /// </summary>
        /// <returns></returns>
        public ICloudDirectoryEntry GetRoot()
        {
            return _Service.RequestResource(_Session, "/", null) as ICloudDirectoryEntry;
        }

        /// <summary>
        /// This method returns a filesystem object, this can be files or folders
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public ICloudFileSystemEntry GetFileSystemObject(string path, ICloudDirectoryEntry parent)
        {
            /*
             * This section generates for every higher object the object tree
             */
            PathHelper ph = new PathHelper(path);
            String[] elements = ph.GetPathElements();

            // create the virtual root
            ICloudDirectoryEntry current = parent;

            // build the root
            if (current == null)
            {
                current = GetRoot();
            }

            // check if we request only the root
            if (path.Equals("/"))
                return current;

            // create the path tree
            for (int i = 0; i <= elements.Length - 1; i++)
            {
                String elem = elements[i];

                if (i == elements.Length - 1)
                {
                    // get requested object 
                    ICloudFileSystemEntry requestedObject = _Service.RequestResource(_Session, elem, current);

                    // go ahead on error
                    if (requestedObject == null)
                        return null;
                    else
                        // go ahead
                        return requestedObject;
                }
                else
                {
                    try
                    {
                        // try to get the child
                        current = current.GetChild(elem) as ICloudDirectoryEntry;
                    }
                    catch (SharpBoxException e)
                    {
                        // if not found, create a virtual one
                        if (e.ErrorCode == SharpBoxErrorCodes.ErrorFileNotFound)
                            current = GenericStorageProviderFactory.CreateDirectoryEntry(_Session, elem, current);
                        else
                            throw e;
                    }
                }
            }

            // looks like an error
            return null;
        }

        /// <summary>
        /// This method creates a folder in a given parent folder 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public ICloudDirectoryEntry CreateFolder(string name, ICloudDirectoryEntry parent)
        {
            // solve the parent issue
            if (parent == null)
            {
                parent = GetRoot();

                if (parent == null)
                    return null;
            }

            // double check if the folder which has to be created 
            // is in the folder            
            var childDir = parent.GetChild(name, false);
            if (childDir != null)
                return childDir as ICloudDirectoryEntry;            

            // request the object
            var res = _Service.CreateResource(_Session, name, parent);
            if (res == null)
                return null;

            // go ahead
            return res as ICloudDirectoryEntry;
        }

        /// <summary>
        /// This method removes a given filesystem object from the cloud storage
        /// </summary>
        /// <param name="fsentry"></param>
        /// <returns></returns>
        public bool DeleteFileSystemEntry(ICloudFileSystemEntry fsentry)
        {
            return _Service.DeleteResource(_Session, fsentry);
        }

        /// <summary>
        /// This method moves a specifc filesystem object from his current location
        /// into a new folder
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        public bool MoveFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _Service.MoveResource(_Session, fsentry, newParent);
        }

        /// <summary>
        /// This method moves a specifc filesystem object from his current location
        /// into a new folder
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newParent"></param>
        /// <returns></returns>
        public bool CopyFileSystemEntry(ICloudFileSystemEntry fsentry, ICloudDirectoryEntry newParent)
        {
            return _Service.CopyResource(_Session, fsentry, newParent);
        }

        /// <summary>
        /// This method renames a given filesystem object (file or folder)
        /// </summary>
        /// <param name="fsentry"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public bool RenameFileSystemEntry(ICloudFileSystemEntry fsentry, string newName)
        {
            // save the old name
            String oldName = fsentry.Name;

            // rename the resource
            if (_Service.RenameResource(_Session, fsentry, newName))
            {
                // get the parent
                BaseDirectoryEntry p = fsentry.Parent as BaseDirectoryEntry;

                // remove the old childname
                p.RemoveChildByName(oldName);

                // readd the child
                p.AddChild(fsentry as BaseFileEntry);

                // go ahead
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// This method creates a file in the cloud storage
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ICloudFileSystemEntry CreateFile(ICloudDirectoryEntry parent, string name)
        {
            // build the parent
            if (parent == null)
                parent = GetRoot();

            // build the file entry
            var newEntry = GenericStorageProviderFactory.CreateFileSystemEntry(_Session, name, parent);
            return newEntry;
        }

        /// <summary>
        /// This method returns the absolut URL (with all authentication decorators) for a specific file
        /// system object
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public virtual Uri GetFileSystemObjectUrl(string path, ICloudDirectoryEntry parent)
        {
            String url = _Service.GetResourceUrl(_Session, parent, path);
            return new Uri(url);
        }

        /// <summary>
        /// This method returns the filesystem path (UNIX style) of a specific object
        /// </summary>
        /// <param name="fsObject"></param>
        /// <returns></returns>
        public String GetFileSystemObjectPath(ICloudDirectoryEntry fsObject)
        {
            return GenericHelper.GetResourcePath(fsObject);
        }

        /// <summary>
        /// This method stores the given security token into a tokendictionary
        /// </summary>
        /// <param name="tokendata"></param>
        /// <param name="token"></param>
        public void StoreToken(Dictionary<String, String> tokendata, ICloudStorageAccessToken token)
        {
            _Service.StoreToken(_Session, tokendata, token);
        }

        /// <summary>
        /// This method loads the given token from the token dictionary
        /// </summary>
        /// <param name="tokendata"></param>
        /// <returns></returns>
        public ICloudStorageAccessToken LoadToken(Dictionary<String, String> tokendata)
        {
            return _Service.LoadToken(tokendata);
        }

        /// <summary>
        /// This property returns the current accesstoken
        /// </summary>
        public ICloudStorageAccessToken CurrentAccessToken
        {
            get { return _Session == null ? null : _Session.SessionToken; }
        }

        #endregion
    }
}
