using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;

namespace BetterExplorer.Networks.FileSystem
{
    class SharpBoxFileSystem : NetworkFileSystem
    {
        CloudStorage _client = null;

        public SharpBoxFileSystem(CloudStorage storage)
        {
            _client = storage;
        }

        public CloudStorage CloudStorage
        {
            get
            {
                return _client;
            }
            set
            {
                _client = value;
            }
        }

        private ICloudDirectoryEntry GetParentOfItem(FileSystemObject item)
        {
            return _client.GetFolder(item.Parent.Path);
        }

        private Directory CreateFileSystemDirectory(ICloudDirectoryEntry item)
        {
            if (item != null)
            {
                return new Directory(item.Name, GetFileSystemPath(item), CreateFileSystemDirectory(item.Parent), item.Modified);
            }
            else
            {
                return null;
            }
        }

        private string GetFileSystemPath(ICloudFileSystemEntry item)
        {
            if (item.Parent != null)
            {
                return GetFileSystemPath(item.Parent) + "/" + item.Name;
            }
            else
            {
                return "/" + item.Name;
            }
        }

        public override void CloseConnection()
        {
            _client.Close();
            //base.CloseConnection();
        }

        public override bool CreateDirectory(Directory dir)
        {
            _client.CreateFolder(dir.Path);
            return true;
            //return base.CreateDirectory(dir);
        }

        public override bool DeleteDirectory(Directory dir)
        {
            return _client.DeleteFileSystemEntry(_client.GetFolder(dir.Path));
            //return base.DeleteDirectory(dir);
        }

        public override bool DeleteFile(File file)
        {
            return _client.DeleteFileSystemEntry(_client.GetFileSystemObject(file.Name, GetParentOfItem(file)));
            //return base.DeleteFile(file);
        }

        public override bool DownloadFile(File remotefile, string localdir, string localname)
        {
            _client.DownloadFile(remotefile.Path, localdir + "/" + localname);
            return true;
            //return base.DownloadFile(remotefile, localdir, localname);
        }

        public override List<FileSystemObject> GetChildren(Directory dir)
        {
            ICloudDirectoryEntry de = _client.GetFolder(dir.Path);
            List<FileSystemObject> children = new List<FileSystemObject>();
            foreach (ICloudFileSystemEntry item in de)
            {
                if (item is ICloudDirectoryEntry) //directory
                {
                    children.Add(CreateFileSystemDirectory(item as ICloudDirectoryEntry));
                }
                else //file
                {
                    children.Add(new File(item.Name, GetFileSystemPath(item), CreateFileSystemDirectory(item.Parent), item.Modified, item.Length));
                }
            }
            return children;
            //return base.GetChildren(dir);
        }

        public override Directory GetDirectory(string path)
        {
            return CreateFileSystemDirectory(_client.GetFolder(path));
            //return base.GetDirectory(path);
        }

        public override Directory GetRootDirectory()
        {
            return CreateFileSystemDirectory(_client.GetRoot());
            //return base.GetRootDirectory();
        }

        public override bool RenameDirectory(Directory dir, string name)
        {
            return _client.RenameFileSystemEntry(dir.Path, name);
            //return base.RenameDirectory(dir, name);
        }

        public override bool RenameFile(File file, string newname)
        {
            return _client.RenameFileSystemEntry(file.Path, newname);
            //return base.RenameFile(file, newname);
        }

        public override bool UploadFile(string localdir, string localname, Directory remotedir, string remotename)
        {
            _client.UploadFile(localdir + "\\" + localname, remotedir.Path, remotename);
            return true;
            //return base.UploadFile(localdir, localname, remotedir, remotename);
        }

    }
}
