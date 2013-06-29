using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.Logic;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet.Logic
{
    internal class BoxNetStorageProviderService : WebDavStorageProviderService
    {
        protected override String OnNameBase(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String NameBase)
        {
            return NameBase.Replace("http", "https");            
        }
    }
}
