using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks
{
    public abstract class NetworkItem
    {
        protected string _displayname;
        protected AccountData.AccountService _service;
        protected AccountData.AccountType _type;

        protected string _username = "";
        protected string _password = "";
        protected string _appkey = "";
        protected string _appsecret = "";
        protected int _port = -1;

        protected FileSystem.NetworkFileSystem _fs;

        public string DisplayName
        {
            get
            {
                return _displayname;
            }
        }

        public AccountData.AccountService AccountService
        {
            get
            {
                return _service;
            }
        }

        public AccountData.AccountType Type
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

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public FileSystem.NetworkFileSystem FileSystem
        {
            get
            {
                return _fs;
            }
        }

    }
}
