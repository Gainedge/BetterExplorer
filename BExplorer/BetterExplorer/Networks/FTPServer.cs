using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using AlexPilotti.FTPS;
//using AlexPilotti.FTPS.Client;
//using AlexPilotti.FTPS.Common;

namespace BetterExplorer.Networks
{
    public class FTPServer : NetworkItem
    {


        ///// <summary>
        ///// Creates a new instance of an FTP server.
        ///// </summary>
        ///// <param name="displayname">The name to display for this FTP server.</param>
        ///// <param name="hostname">The address of this server.</param>
        ///// <param name="port">The port to use for this FTP server. (usually 21)</param>
        ///// <param name="username">The username to connect to this server with.</param>
        ///// <param name="password">The password to connect to this server with.</param>
        ///// <param name="anonlogin">If true, connect to this server anonymously (without a username and password).</param>
        ///// <param name="passivemode">If true, connect in passive mode. If false, connect in active mode.</param>
        //public FTPServer(string displayname, string hostname, int port, string username, string password, bool anonlogin, bool passivemode)
        //{
        //    _displayname = displayname;
        //    _server = hostname;
        //    _port = port;
        //    _username = username;
        //    _password = password;
        //    _service = AccountService.FTP;
        //    _type = AccountType.Server;
        //    _anon = anonlogin;
        //    _passive = passivemode;
        //}

        //private bool _passive = false;

        //public bool PassiveMode
        //{
        //    get
        //    {
        //        return _passive;
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

        //    ftps.Connect(_server, _port, creds, ESSLSupportMode.ClearText, null, null, 0, 0, 0, 120, true, edc);

        //    FileSystem.FTPServerFileSystem fs = new FileSystem.FTPServerFileSystem(ftps);

        //    return fs;
        //}

    }
}
