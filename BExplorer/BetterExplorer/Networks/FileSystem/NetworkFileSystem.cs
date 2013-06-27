using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks.FileSystem
{
    /// <summary>
    /// Network file system manager. This class communicates between the representations of the file system and the system online.
    /// </summary>
    public abstract class NetworkFileSystem
    {
        /// <summary>
        /// The top directory of the file system.
        /// </summary>
        protected Directory _topdir;

        // TODO: Add child classes for FTP and WebDAV

        public bool UploadFile(string localdir, string localname, Directory remotedir, string remotename)
        {
            return false;
        }

        public bool DownloadFile(File remotefile, string localdir, string localname)
        {
            return false;
        }

        public bool RenameFile(File file, string newname)
        {
            return false;
        }

        public bool DeleteFile(File file)
        {
            return false;
        }

        public bool CreateDirectory(Directory dir)
        {
            return false;
        }

        public bool RenameDirectory(Directory dir, string name)
        {
            return false;
        }

        public bool DeleteDirectory(Directory dir)
        {
            return false;
        }

        public Directory GetDirectory(string path)
        {
            return null;
        }

        public List<FileSystemObject> GetChildren(Directory dir)
        {
            return null;
        }

    }
}
