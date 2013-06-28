using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks
{
    public abstract class NetworkItem
    {
        protected string _displayname;
        protected NetworkAccountManager.AccountService _service;
        protected NetworkAccountManager.AccountType _type;

        protected string _username = "";
        protected string _password = "";
        protected string _appkey = "";
        protected string _appsecret = "";
        protected string _server = "";
        protected int _port = -1;

        protected FileSystem.NetworkFileSystem _fs;

        public string DisplayName
        {
            get
            {
                return _displayname;
            }
        }

        public NetworkAccountManager.AccountService AccountService
        {
            get
            {
                return _service;
            }
        }

        public NetworkAccountManager.AccountType Type
        {
            get
            {
                return _type;
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
        }

        public string AppKey
        {
            get
            {
                return _appkey;
            }
        }

        public string AppSecret
        {
            get
            {
                return _appsecret;
            }
        }

        public string Server
        {
            get
            {
                return _server;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public virtual FileSystem.NetworkFileSystem CreateConnection()
        {
            return null;
        }

    }
}
