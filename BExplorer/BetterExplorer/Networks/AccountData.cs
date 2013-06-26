using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks
{
    
    /// <summary>
    /// This class manages all account-related information for Better Explorer.
    /// </summary>
    class AccountData
    {
        // this class will be used to manage the account information file, including anything security-related,
        // and will be a point of communication between the main UI and this information

        // TODO: Add code for storing and retrieving from file (and put in security measures)
        // TODO: Add classes for servers and online ports, and a list here to list them all

        /// <summary>
        /// The type a server is classified by. This will either be FTP, FTPS, or WebDAV.
        /// </summary>
        public enum ServerType
        {
            /// <summary>
            /// An FTP server.
            /// </summary>
            FTP = 0,

            /// <summary>
            /// An FTPS server.
            /// </summary>
            FTPS = 1,

            /// <summary>
            /// A WebDAV server.
            /// </summary>
            WebDAV = 2,
        }

    }
}
