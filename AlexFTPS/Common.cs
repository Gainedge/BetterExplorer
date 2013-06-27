/*
 *  Copyright 2008 Alessandro Pilotti
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation; either version 2.1 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA 
 */

using System;
using System.Security.Authentication;

namespace AlexPilotti.FTPS.Common
{
    public enum ETransferMode { ASCII, Binary }

    public enum ETextEncoding { ASCII, UTF8 }

    public class FTPReply
    {
        private int code;
        private string message;

        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Code, Message);
        }
    }

    public class DirectoryListItem
    {
        private string flags;
        private string owner;
        private string group;
        private bool isDirectory;
        private bool isSymLink;
        private string name;
        private ulong size;
        private DateTime creationTime;
        private string symLinkTargetPath;

        public ulong Size
        {
            get { return size; }
            set { size = value; }
        }

        public string SymLinkTargetPath
        {
            get { return symLinkTargetPath; }
            set { symLinkTargetPath = value; }
        }

        public string Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        public string Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public string Group
        {
            get { return group; }
            set { group = value; }
        }

        public bool IsDirectory
        {
            get { return isDirectory; }
            set { isDirectory = value; }
        }

        public bool IsSymLink
        {
            get { return isSymLink; }
            set { isSymLink = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public DateTime CreationTime
        {
            get { return creationTime; }
            set { creationTime = value; }
        }
    }

    /// <summary>
    /// Encapsulates the SSL/TLS algorithms connection information.
    /// </summary>
    public class SslInfo
    {
        SslProtocols sslProtocol;

        CipherAlgorithmType cipherAlgorithm;
        int cipherStrength;

        HashAlgorithmType hashAlgorithm;
        int hashStrength;

        ExchangeAlgorithmType keyExchangeAlgorithm;
        int keyExchangeStrength;

        public SslProtocols SslProtocol
        {
            get { return sslProtocol; }
            set { sslProtocol = value; }
        }

        public CipherAlgorithmType CipherAlgorithm
        {
            get { return cipherAlgorithm; }
            set { cipherAlgorithm = value; }
        }

        public int CipherStrength
        {
            get { return cipherStrength; }
            set { cipherStrength = value; }
        }

        public HashAlgorithmType HashAlgorithm
        {
            get { return hashAlgorithm; }
            set { hashAlgorithm = value; }
        }

        public int HashStrength
        {
            get { return hashStrength; }
            set { hashStrength = value; }
        }

        public ExchangeAlgorithmType KeyExchangeAlgorithm
        {
            get { return keyExchangeAlgorithm; }
            set { keyExchangeAlgorithm = value; }
        }

        public int KeyExchangeStrength
        {
            get { return keyExchangeStrength; }
            set { keyExchangeStrength = value; }
        }

        public override string ToString()
        {
            return SslProtocol.ToString() + ", " +
                   CipherAlgorithm.ToString() + " (" + cipherStrength.ToString() + " bit), " +
                   KeyExchangeAlgorithm.ToString() + " (" + keyExchangeStrength.ToString() + " bit), " +
                   HashAlgorithm.ToString() + " (" + hashStrength.ToString() + " bit)";
        }
    }

    public class LogCommandEventArgs : EventArgs
    {
        public LogCommandEventArgs(string commandText)
            : base()
        {
            this.CommandText = commandText;
        }

        public string CommandText { get; private set; }
    }

    public class LogServerReplyEventArgs : EventArgs
    {
        public LogServerReplyEventArgs(FTPReply serverReply)
            : base()
        {
            this.ServerReply = serverReply;
        }

        public FTPReply ServerReply { get; private set; }
    }
}
