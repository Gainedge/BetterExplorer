using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppLimit.CloudComputing.SharpBox
{
    /// <summary>
    /// This class contains the limits of a given cloud storage
    /// configuration
    /// </summary>
    public class CloudStorageLimits
    {
        /// <summary>
        /// Default ctor which sets the limits to an 
        /// unlimited value (no limits)
        /// </summary>
        public CloudStorageLimits()
        {
            MaxUploadFileSize = -1;
            MaxDownloadFileSize = -1;
        }

        /// <summary>
        /// Special ctor which allows to initials the limits with
        /// special values in an external protocol provider
        /// </summary>
        /// <param name="MaxUploadFileSize"></param>
        /// <param name="MaxDownloadFileSite"></param>
        public CloudStorageLimits(int MaxUploadFileSize, int MaxDownloadFileSite)
            : this()
        {
            this.MaxUploadFileSize = MaxUploadFileSize;
            this.MaxUploadFileSize = MaxDownloadFileSite;
        }

        /// <summary>
        /// defines the maximum file size in bytes during upload
        /// </summary>
        public int MaxUploadFileSize { get; internal set; }

        /// <summary>
        /// defines the maximum file size in bytes during download
        /// </summary>
        public int MaxDownloadFileSize { get; internal set; }
    }
}
