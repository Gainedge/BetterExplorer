using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks
{
    public abstract class NetworkItem
    {
        protected string _displayname;
        protected AccountService _service;
        protected AccountType _type;

        protected string _username = "";
        protected string _password = "";
        protected string _appkey = "";
        protected string _appsecret = "";
        protected string _server = "";
        protected int _port = -1;
        protected bool _anon = false;

        protected FileSystem.NetworkFileSystem _fs;

        public string DisplayName
        {
            get
            {
                return _displayname;
            }
        }

        public AccountService AccountService
        {
            get
            {
                return _service;
            }
        }

        public AccountType Type
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

        public string ServerAddress
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

        public bool AnonymousLogin
        {
            get
            {
                return _anon;
            }
        }

        public virtual FileSystem.NetworkFileSystem CreateConnection()
        {
            return null;
        }

    }
}
