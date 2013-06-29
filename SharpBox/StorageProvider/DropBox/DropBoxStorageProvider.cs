using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth.Impl;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using System.Net;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox
{
    internal class DropBoxStorageProvider : GenericStorageProvider
    {
        public DropBoxStorageProvider()
            : base(new DropBoxStorageProviderService())
        { 
        }

        public override Uri GetFileSystemObjectUrl(string path, ICloudDirectoryEntry parent)
        {
            // get the filesystem
            ICloudFileSystemEntry entry = GetFileSystemObject(path, parent);

            // get the download url
            String url = DropBoxStorageProviderService.GetDownloadFileUrlInternal(this._Session, entry);
            
            // get the right session
            DropBoxStorageProviderSession session = (DropBoxStorageProviderSession)_Session;

            // generate the oauth url
            OAuthService svc = new OAuthService();
            url = svc.GetProtectedResourceUrl(url, session.Context, session.SessionToken as DropBoxToken, null, WebRequestMethodsEx.Http.Get);            

            // go ahead
            return new Uri(url);
        }
    }
}
