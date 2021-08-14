using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using AlexPilotti.FTPS;
//using AlexPilotti.FTPS.Client;
//using AlexPilotti.FTPS.Common;
using System.Security.Cryptography.X509Certificates;

namespace BetterExplorer.Networks
{
    public class FTPSServer : NetworkItem
    {

        ///// <summary>
        ///// Creates a new instance of an FTPS server.
        ///// </summary>
        ///// <param name="displayname">The name to display for this FTPS server.</param>
        ///// <param name="hostname">The address of this server.</param>
        ///// <param name="port">The port to use for this FTPS server. (usually 21 or 990)</param>
        ///// <param name="username">The username to connect to this server with.</param>
        ///// <param name="password">The password to connect to this server with.</param>
        ///// <param name="anonlogin">If true, connect to this server anonymously (without a username and password).</param>
        ///// <param name="passivemode">If true, connect in passive mode. If false, connect in active mode.</param>
        ///// <param name="sslmode">The mode used for interacting with the FTPS server. Specifically, what connections should be secured and when.</param>
        ///// <param name="X509cert">The path of the client X.509 certificate file to use.</param>
        ///// <param name="certpass">The password to access the X.509 certificate file.</param>
        //public FTPSServer(string displayname, string hostname, int port, string username, string password, bool anonlogin, bool passivemode, ESSLSupportMode sslmode, string X509cert = "", string certpass = "")
        //{
        //    _displayname = displayname;
        //    _server = hostname;
        //    _port = port;
        //    _username = username;
        //    _password = password;
        //    _service = AccountService.FTPS;
        //    _type = AccountType.Server;
        //    _anon = anonlogin;
        //    _passive = passivemode;
        //    _sslmode = sslmode;
        //    if (X509cert != "")
        //    {
        //        if (certpass != "")
        //        {
        //            _client = new X509Certificate(X509cert, certpass);
        //        }
        //        else
        //        {
        //            _client = new X509Certificate(X509cert);
        //        }
        //    }
        //    else
        //    {
        //        _client = null;
        //    }
        //}

        //private bool _passive = false;
        //private X509Certificate _client = null;
        //private ESSLSupportMode _sslmode = ESSLSupportMode.DataChannelRequested;

        //public bool PassiveMode
        //{
        //    get
        //    {
        //        return _passive;
        //    }
        //}

        //public X509Certificate ClientCertificate
        //{
        //    get
        //    {
        //        return _client;
        //    }
        //}

        //public override FileSystem.NetworkFileSystem CreateConnection()
        //{
        //    FTPSClient ftps = new FTPSClient();
        //    EDataConnectionMode edc;
        //    if (_passive == true)
        //    {
        //        edc = EDataConnectionMode.Passive;
        //    }
        //    else
        //    {
        //        edc = EDataConnectionMode.Active;
        //    }

        //    System.Net.NetworkCredential creds = null;
        //    if (_anon == false)
        //    {
        //        creds = new System.Net.NetworkCredential(_username, _password);
        //    }

        //    ftps.Connect(_server, _port, creds, _sslmode, null, _client, 0, 0, 0, 120, true, edc);

        //    FileSystem.FTPServerFileSystem fs = new FileSystem.FTPServerFileSystem(ftps);

        //    return fs;
        //}

    }
}
