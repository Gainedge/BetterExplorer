using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.Common;
using AppLimit.CloudComputing.SharpBox.StorageProvider;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;

namespace BetterExplorer.Networks
{
    class DropBoxAccount : NetworkItem
    {
        private DropBoxRequestToken _CurrentRequestToken = null;
        private ICloudStorageAccessToken _GeneratedToken = null;

        public DropBoxAccount(string displayname, string username, string password, string appkey, string secret)
        {
            _displayname = displayname;
            _username = username;
            _password = password;
            _type = AccountType.OnlineStorage;
            _service = AccountService.Dropbox;
            _appkey = appkey;
            _appsecret = secret;
        }

        public override FileSystem.NetworkFileSystem CreateConnection()
        {

            DropBoxConfiguration config = new DropBoxConfiguration();

            return new FileSystem.SharpBoxFileSystem(null);
            //return base.CreateConnection();
        }
    }
}
