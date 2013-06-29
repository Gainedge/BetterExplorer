using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.BoxNet
{
    /// <summary>
    /// The specific configuration for box.net
    /// </summary>
    public class BoxNetConfiguration : WebDavConfiguration
    {
        /// <summary>
        /// ctor
        /// </summary>
        public BoxNetConfiguration()
            : base(new Uri("https://www.box.com/dav"))
        { }

        /// <summary>
        /// This method returns a standard configuration for 1and1 
        /// </summary>
        /// <returns></returns>
        static public BoxNetConfiguration GetBoxNetConfiguration()
        {
            // set the right url
            BoxNetConfiguration config = new BoxNetConfiguration();
            config.Limits = new CloudStorageLimits();
            config.Limits.MaxDownloadFileSize = 2000 * 1024 * 1024;
            config.Limits.MaxUploadFileSize = config.Limits.MaxDownloadFileSize;

            // box.net does not support a valid ssl
            config.TrustUnsecureSSLConnections = true;

            // go ahead
            return config;
        }

    }
}
