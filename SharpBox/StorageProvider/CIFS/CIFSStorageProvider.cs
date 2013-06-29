using System;
using System.Collections.Generic;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web.Dav;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;
using System.Net;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS.Logic;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.CIFS
{
	internal class CIFSStorageProvider : GenericStorageProvider
	{              
        public CIFSStorageProvider()
            : base( new CIFSStorageProviderService())
		{}		
    }
}

