using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet
{
    internal class BoxNetStorageProvider : WebDavStorageProvider
    {
        public BoxNetStorageProvider()
            : base(new BoxNetStorageProviderService())
        { }
    }
}
