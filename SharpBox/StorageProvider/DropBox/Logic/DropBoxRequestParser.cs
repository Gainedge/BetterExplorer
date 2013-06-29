using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net.oAuth;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using AppLimit.CloudComputing.SharpBox.Exceptions;
using AppLimit.CloudComputing.SharpBox.Common.Net.Json;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using System.Net;
using AppLimit.CloudComputing.SharpBox.Common.Net.Web;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox.Logic
{
    internal class DropBoxRequestParser
    {        
        public static String RequestResourceByUrl(String url, IStorageProviderService service, IStorageProviderSession session, out int netErrorCode)
        {
            return RequestResourceByUrl(url, null, service, session, out netErrorCode);
        }

        public static String RequestResourceByUrl(String url, Dictionary<String, String> parameters, IStorageProviderService service, IStorageProviderSession session, out int netErrorCode)
        {
            // cast the dropbox session
            DropBoxStorageProviderSession dropBoxSession = session as DropBoxStorageProviderSession;

            // instance the oAuthServer
            OAuthService svc = new OAuthService();

            // build the webrequest to protected resource
            WebRequest request = svc.CreateWebRequest(url, WebRequestMethodsEx.Http.Get, null, null, dropBoxSession.Context, (DropBoxToken)dropBoxSession.SessionToken, parameters);

            // get the error code
            WebException ex;

            // perform a simple webrequest 
            Stream s = svc.PerformWebRequest(request, null, out netErrorCode, out ex);            
            if (s == null)
                return "";

            // read the memory stream and convert to string
            return new StreamReader(s).ReadToEnd();            
        }

        public static BaseFileEntry CreateObjectsFromJsonString(String jsonMessage, IStorageProviderService service, IStorageProviderSession session)
        {
            return UpdateObjectFromJsonString(jsonMessage, null, service, session);
        }        
               
        public static BaseFileEntry UpdateObjectFromJsonString(String jsonMessage, BaseFileEntry objectToUpdate, IStorageProviderService service, IStorageProviderSession session)
        {
            // verify if we have a directory or a file
            JsonHelper jc = new JsonHelper();
            if (!jc.ParseJsonMessage(jsonMessage))
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorInvalidFileOrDirectoryName);
            
            Boolean isDir = jc.GetBooleanProperty("is_dir");

            // create the entry
            BaseFileEntry dbentry = null;
            Boolean bEntryOk = false;

            if (isDir)
            {
                if (objectToUpdate == null)
                    dbentry = new BaseDirectoryEntry("Name", 0, DateTime.Now, service, session);
                else
                    dbentry = objectToUpdate as BaseDirectoryEntry;

                bEntryOk = BuildDirectyEntry(dbentry as BaseDirectoryEntry, jc, service, session);
            }
            else
            {
                if (objectToUpdate == null)
                    dbentry = new BaseFileEntry("Name", 0, DateTime.Now, service, session);
                else
                    dbentry = objectToUpdate;

                bEntryOk = BuildFileEntry(dbentry, jc);
            }

            // parse the childs and fill the entry as self
            if (!bEntryOk)
                throw new SharpBoxException(SharpBoxErrorCodes.ErrorCouldNotContactStorageService);

            // set the is deleted flag
            try
            {
                // try to read the is_deleted property
                dbentry.IsDeleted = jc.GetBooleanProperty("is_deleted");
            }
            catch (Exception)
            {
                // the is_deleted proprty is missing (so it's not a deleted file or folder)
                dbentry.IsDeleted = false;
            }

            // return the child
            return dbentry;
        }

        private static Boolean BuildFileEntry(BaseFileEntry fileEntry, JsonHelper jh)
        {
            /*
             *  "revision": 29251,
                "thumb_exists": false,
                "bytes": 37941660,
                "modified": "Tue, 01 Jun 2010 14:45:09 +0000",
                "path": "/Public/2010_06_01 15_53_48_336.nvl",
                "is_dir": false,
                "icon": "page_white",
                "mime_type": "application/octet-stream",
                "size": "36.2MB"
             * */
            
            // set the size
            fileEntry.Length = Convert.ToInt64(jh.GetProperty("bytes"));

            // set the modified time
            fileEntry.Modified = jh.GetDateTimeProperty("modified");

            // build the displayname
            String DropBoxPath = jh.GetProperty("path");
            var arr = DropBoxPath.Split('/');
            fileEntry.Name = arr.Length > 0 ? arr[arr.Length - 1] : DropBoxPath;
            
            // set the hash property if possible
            string hashValue = jh.GetProperty("hash");
            if (hashValue.Length > 0)
                fileEntry.SetPropertyValue("hash", hashValue);

            // set the path property            
            fileEntry.SetPropertyValue("path", DropBoxPath.Equals("/") ? "" : DropBoxPath);            
            
            // set the revision value if possible
            string revValue = jh.GetProperty("rev");
            if (revValue.Length >0)
            {
                fileEntry.SetPropertyValue("rev", revValue); 
            }

            // go ahead
            return true;
        }

        private static Boolean BuildDirectyEntry(BaseDirectoryEntry dirEntry, JsonHelper jh, IStorageProviderService service, IStorageProviderSession session)
        {
            // build the file entry part 
            if (!BuildFileEntry(dirEntry, jh))
                return false;            

            // now take the content 
            List<String> content = jh.GetListProperty("contents");

            if (content.Count == 0)
                return true;
            
            // remove all childs
            dirEntry.ClearChilds();

            // add the childs
            foreach (String jsonContent in content)
            {
                // parse the item
                JsonHelper jc = new JsonHelper();
                if (!jc.ParseJsonMessage(jsonContent))
                    continue;

                // check if we have a directory
                Boolean isDir = jc.GetBooleanProperty("is_dir");

                BaseFileEntry fentry;

                if (isDir)
                {
                    fentry = new BaseDirectoryEntry("Name", 0, DateTime.Now, service, session);                    
                }
                else
                {
                    fentry = new BaseFileEntry("Name", 0, DateTime.Now, service, session);
                }

                // build the file attributes
                BuildFileEntry(fentry, jc);    
                
                // establish parent child realtionship
                dirEntry.AddChild(fentry);
            }

            // set the length
            dirEntry.Length = dirEntry.Count;

            // go ahead
            return true;
        }
    }
}
