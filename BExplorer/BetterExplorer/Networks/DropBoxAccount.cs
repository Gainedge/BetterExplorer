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

        private string _tokenloc;

        public DropBoxAccount(string displayname, string tokenlocation)
        {
            _displayname = displayname;
            _type = AccountType.OnlineStorage;
            _service = AccountService.Dropbox;
            _tokenloc = tokenlocation;
        }

        public string TokenLocation
        {
            get
            {
                return _tokenloc;
            }
            set
            {
                _tokenloc = value;
            }
        }

        public override FileSystem.NetworkFileSystem CreateConnection()
        {
            DropBoxConfiguration config = DropBoxConfiguration.GetStandardConfiguration();

            config.APIVersion = DropBoxAPIVersion.V1;

            CloudStorage cs = new CloudStorage();

            ICloudStorageAccessToken tok = cs.DeserializeSecurityTokenEx(_tokenloc);

            cs.Open(config, tok);

            return new FileSystem.SharpBoxFileSystem(cs);
        }
    }
}
