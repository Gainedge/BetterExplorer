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
        /// The maximum file size (in bytes) that most servers will allow for upload and download size (for those with such limits). A warning should occur if a file size is above this number.
        /// </summary>
        public const int MaxSafeFileSize = 524288000;

        public virtual bool UploadFile(string localdir, string localname, Directory remotedir, string remotename)
        {
            return false;
        }

        public virtual bool DownloadFile(File remotefile, string localdir, string localname)
        {
            return false;
        }

        public virtual bool RenameFile(File file, string newname)
        {
            return false;
        }

        public virtual bool DeleteFile(File file)
        {
            return false;
        }

        public virtual bool CreateDirectory(Directory dir)
        {
            return false;
        }

        public virtual bool RenameDirectory(Directory dir, string name)
        {
            return false;
        }

        public virtual bool DeleteDirectory(Directory dir)
        {
            return false;
        }

        public virtual Directory GetDirectory(string path)
        {
            return null;
        }

        public virtual Directory GetRootDirectory()
        {
            return null;
        }

        public virtual List<FileSystemObject> GetChildren(Directory dir)
        {
            return null;
        }

        public virtual void CloseConnection()
        {

        }

    }
}
