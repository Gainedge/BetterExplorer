using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using AlexPilotti.FTPS;
//using AlexPilotti.FTPS.Client;
//using AlexPilotti.FTPS.Common;

namespace BetterExplorer.Networks.FileSystem
{
    class FTPServerFileSystem : NetworkFileSystem
    {

        //private FTPSClient _client;

        //public FTPServerFileSystem(FTPSClient handler)
        //{
        //    _client = handler;
        //}

        //public FTPSClient FTPSClient
        //{
        //    get
        //    {
        //        return _client;
        //    }
        //    set
        //    {
        //        _client = value;
        //    }
        //}

        //private Directory _currDir;

        //public Directory CurrentDirectory
        //{
        //    get
        //    {
        //        return _currDir;
        //    }
        //    set
        //    {
        //        _currDir = value;
        //    }
        //}

        //public override void CloseConnection()
        //{
        //    _client.Close();
        //}

        //public override bool CreateDirectory(Directory dir)
        //{
        //    _client.MakeDir(dir.Path);
        //    return true;
        //}

        //public override bool DeleteDirectory(Directory dir)
        //{
        //    _client.RemoveDir(dir.Path);
        //    return true;
        //}

        //public override bool DeleteFile(File file)
        //{
        //    _client.DeleteFile(file.Name);
        //    return true;
        //}

        //public override bool DownloadFile(File remotefile, string localdir, string localname)
        //{
        //    if (_client.GetFile(remotefile.Name, localdir + localname) == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //public override List<FileSystemObject> GetChildren(Directory dir)
        //{
        //    List<FileSystemObject> fsi = new List<FileSystemObject>();

        //    foreach ( AlexPilotti.FTPS.Common.DirectoryListItem li in _client.GetDirectoryList(dir.Path))
        //    {
        //        if (li.IsDirectory)
        //        {
        //            fsi.Add(new Directory(li.Name, dir.Path + "/" + li.Name, dir, null));
        //        }
        //        else if (li.IsSymLink)
        //        {
        //            fsi.Add(new SymbolicLink(li.Name, dir.Path + "/" + li.Name, dir, li.CreationTime, li.SymLinkTargetPath));
        //        }
        //        else //file
        //        {
        //            fsi.Add(new File(li.Name, dir.Path + "/" + li.Name, dir, li.CreationTime, Convert.ToInt64(li.Size)));
        //        }
        //    }

        //    return fsi;
        //}

        //public override Directory GetDirectory(string path)
        //{
        //    _client.SetCurrentDirectory(path);
        //    string gcd = _client.GetCurrentDirectory();
        //    return new Directory(gcd.Substring(gcd.LastIndexOf("/")), gcd, null, null);
        //}

        //public override Directory GetRootDirectory()
        //{
        //    _client.SetCurrentDirectory("/");
        //    return new Directory("..", "/", null, null);
        //    //return base.GetRootDirectory();
        //}

        //public override bool RenameDirectory(Directory dir, string name)
        //{
        //    // the command used to rename files is said to also work on directories,
        //    // but not all servers play nice with it
        //    return false;
        //}

        //public override bool RenameFile(File file, string newname)
        //{
        //    _client.RenameFile(file.Name, newname);
        //    return true;
        //}

        //public override bool UploadFile(string localdir, string localname, Directory remotedir, string remotename)
        //{
        //    _client.PutFile(localdir + localname, remotedir.Path + "/" + remotename);
        //    return true;
        //}

    }
}
