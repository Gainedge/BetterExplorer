using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace BetterExplorer.Networks
{
    
    /// <summary>
    /// This class manages all account-related information for Better Explorer.
    /// </summary>
    public class NetworkAccountManager : List<NetworkItem>
    {
        // this class will be used to manage the account information file, including anything security-related,
        // and will be a point of communication between the main UI and this information

        // TODO: Add code for storing and retrieving from file
        // TODO: Decide upon an encryption service (DES, Rjindael, etc.) and add code for
        //       "decrypt file to memory" and "encrypt memory to file".

        /*
         * Account File Structure:
         * 
         * "
         * ;account:key:secret:username:password;account:username:password;account:address:username:password;
         * "
         * 
        */

        /// <summary>
        /// What service is connected to a given NetworkAccount instance.
        /// </summary>
        public enum AccountService
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

            /// <summary>
            /// A Dropbox account.
            /// </summary>
            Dropbox = 3,

            /// <summary>
            /// A Box.net account.
            /// </summary>
            BoxDotNet = 4,
        }

        /// <summary>
        /// The type of this NetworkAccount (i.e. server, online storage, or social media) which also defines what is allowed in terms of capabilities.
        /// </summary>
        public enum AccountType
        {
            /// <summary>
            /// A FTP or WebDAV server. It allows uploading files, and viewing/downloading files.
            /// </summary>
            Server = 0,

            /// <summary>
            /// An online storage service. It allows uploading files, and viewing/downloading files.
            /// </summary>
            OnlineStorage = 1,

            /// <summary>
            /// A social media service. It allows uploading files.
            /// </summary>
            SocialMedia = 2,
        }

        private string _PassFile;
        private string _DataFile;
        private bool _MasterUsed;
        private SecureString _MasterPW;

        // Call this function to remove the key from memory after use for security.
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint="RtlZeroMemory")]
        public static extern bool ZeroMemory(ref string Destination, int Length);
		
        // Function to Generate a 64 bits Key.
        static string GenerateKey()
        {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();

            // Use the Automatically generated key for Encryption. 
            return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        }

        static void EncryptFile(string sInputFilename, string sOutputFilename, string sKey)
        {
            FileStream fsInput = new FileStream(sInputFilename,
                FileMode.Open,
                FileAccess.Read);

            FileStream fsEncrypted = new FileStream(sOutputFilename,
                            FileMode.Create,
                            FileAccess.Write);

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted,
               desencrypt,
               CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        static void DecryptFile(string sInputFilename, string sOutputFilename, string sKey)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //Set initialization vector.
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename,
               FileMode.Open,
               FileAccess.Read);

            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread,
               desdecrypt,
               CryptoStreamMode.Read);
            //Print the contents of the decrypted file.
            StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        } 

    }
}
