using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using AppLimit.CloudComputing.SharpBox;
//using AppLimit.CloudComputing.SharpBox.Common;
//using AppLimit.CloudComputing.SharpBox.StorageProvider;
//using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;

namespace BetterExplorer.Networks
{
    class WebDAVserver : NetworkItem
    {
        public WebDAVserver(string displayname, string address, string username, string password)
        {
            _displayname = displayname;
            _server = address;
            _username = username;
            _password = password;
            _type = AccountType.Server;
            _service = AccountService.WebDAV;
        }

        //public override FileSystem.NetworkFileSystem CreateConnection()
        //{
        //    GenericNetworkCredentials gnc = new GenericNetworkCredentials();
        //    gnc.UserName = _username;
        //    gnc.Password = _password;
        //    WebDavConfiguration webdav = new WebDavConfiguration(new Uri(_server));

        //    CloudStorage storage = new CloudStorage();
        //    storage.Open(webdav, gnc);

        //    return null;
        //}

    }
}
