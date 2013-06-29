using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox.StorageProvider.BaseObjects;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
using AppLimit.CloudComputing.SharpBox.StorageProvider.API;
using AppLimit.CloudComputing.SharpBox.Common.IO;
using AppLimit.CloudComputing.SharpBox.Common.Net;

#if SILVERLIGHT || MONODROID
using System.Net;
#else
using System.Web;
using System.Net;
#endif

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.WebDav.Logic
{
    internal delegate String NameBaseFilterCallback(String targetUrl, IStorageProviderService service, IStorageProviderSession session, String NameBase);

    internal class WebDavRequestResult
    {
        public BaseFileEntry Self {get; set; }
        public List<BaseFileEntry> Childs { get; set; }

        public WebDavRequestResult()
        {
            Childs = new List<BaseFileEntry>();
        }
    }

    internal class WebDavRequestParser
    {
#if !WINDOWS_PHONE && !MONODROID
        public static WebDavRequestResult CreateObjectsFromNetworkStream(Stream data, String targetUrl, IStorageProviderService service, IStorageProviderSession session)
        {
            return CreateObjectsFromNetworkStream(data, targetUrl, service, session, null);
        }

        public static WebDavRequestResult CreateObjectsFromNetworkStream(Stream data, String targetUrl, IStorageProviderService service, IStorageProviderSession session, NameBaseFilterCallback callback)
        {
            WebDavConfiguration config = session.ServiceConfiguration as WebDavConfiguration;

            WebDavRequestResult results = new WebDavRequestResult();            

            try
            {
                String resourceName = String.Empty;
                long resourceLength = 0; 
                DateTime resourceModificationDate = DateTime.Now;
                DateTime resourceCreationDate = DateTime.Now;
                String resourceContentType = String.Empty;
                Boolean bIsHidden = false;
                Boolean bIsDirectory = false;
                Boolean bIsSelf = false;
                
                // build queryless uri
                String queryLessUri = HttpUtilityEx.GetPathAndQueryLessUri(config.ServiceLocator).ToString().TrimEnd('/');

                // work with decoded target url
                String decodedTargetUrl = HttpUtility.UrlDecode(targetUrl);

                XmlTextReader reader = new XmlTextReader(data);    
                while (reader.Read())    
                {
                    switch(reader.NodeType)
                    {                        
                        case XmlNodeType.Element:
                            {                                       
                                // we are on an element and we have to handle this elements
                                String currentElement = reader.Name;                                

                                // we found a resource name
                                if (currentElement.Contains(":href"))
                                {
                                    // check if it is an element with valud namespace prefix
                                    if (!CheckIfNameSpaceDAVSpace(currentElement, reader))
                                        continue;

                                    // go one more step
                                    reader.Read();
                                
                                    // get the name
                                    String nameBase = reader.Value;

                                    // call the filter if needed
                                    if (callback != null)
                                        nameBase = callback(targetUrl, service, session, nameBase);

                                    /* 
                                     * Just for bug fixing 
                                     * 
                                     * Console.WriteLine("   SVC Locator: " + config.ServiceLocator.ToString());                                   
                                     * Console.WriteLine("SVC Locator QL: " + HttpUtilityEx.GetPathAndQueryLessUri(config.ServiceLocator));                                   
                                     * Console.WriteLine("    Target Url: " + targetUrl);                                    
                                     * Console.WriteLine("      NameBase: " + nameBase);
                                     */ 
                                    
                                    // generate namebase for self check
                                    String nameBaseForSelfCheck;

                                    // remove the base url and build for selfcheck
                                    if (nameBase.StartsWith(config.ServiceLocator.ToString()))
                                    {
                                        nameBaseForSelfCheck = HttpUtility.UrlDecode(nameBase);
                                        nameBase = nameBase.Remove(0, config.ServiceLocator.ToString().Length);
                                    }
                                    else
                                    {                                        
                                        nameBaseForSelfCheck = queryLessUri + HttpUtilityEx.PathDecodeUTF8(nameBase);
                                    }
                                    
                                    // trim all trailing slashes
                                    nameBase = nameBase.TrimEnd('/');                                    
                                                                       
                                    // work with the trailing lsahed
                                    nameBaseForSelfCheck = nameBaseForSelfCheck.TrimEnd('/');
                                    if (targetUrl.EndsWith("/"))
                                        nameBaseForSelfCheck += "/";
                                        
                                    

                                    // check if we are self
                                    if (nameBaseForSelfCheck.Equals(decodedTargetUrl))
                                        bIsSelf = true;
                                    else
                                        bIsSelf = false;
                                    
                                    // get the last file or directory name
                                    PathHelper ph = new PathHelper(nameBase);
                                    resourceName = ph.GetFileName();

                                    // unquote name 
                                    resourceName = HttpUtility.UrlDecode(resourceName);                                    
                                } 
                                else if (currentElement.Contains(":ishidden"))
                                {
                                    // check if it is an element with valud namespace prefix
                                    if (!CheckIfNameSpaceDAVSpace(currentElement, reader))
                                        continue;

                                    // go one more step
                                    reader.Read();

                                    // try to parse
                                    int iIsHidden = 0;
                                    if (!int.TryParse(reader.Value, out iIsHidden))
                                        iIsHidden = 0;

                                    // convert
                                    bIsHidden = Convert.ToBoolean(iIsHidden);                                    
                                }
                                else if (currentElement.Contains(":getcontentlength"))
                                {
                                    // check if it is an element with valud namespace prefix
                                    if (!CheckIfNameSpaceDAVSpace(currentElement, reader))
                                        continue;

                                    // go one more step
                                    reader.Read();

                                    // read value
                                    if (!long.TryParse(reader.Value, out resourceLength))
                                        resourceLength = -1;                                    
                                }
                                else if (currentElement.Contains(":creationdate"))                             
                                {
                                    // check if it is an element with valud namespace prefix
                                    if (!CheckIfNameSpaceDAVSpace(currentElement, reader))
                                        continue;

                                    // go one more step
                                    reader.Read();

                                    // parse
                                    if (!DateTime.TryParse(reader.Value, out resourceCreationDate))
                                        resourceCreationDate = DateTime.Now;                                    
                                }
                                else if (currentElement.Contains(":getlastmodified"))
                                {
                                    // check if it is an element with valud namespace prefix
                                    if (!CheckIfNameSpaceDAVSpace(currentElement, reader))
                                        continue;

                                    // go one more step
                                    reader.Read();

                                    // parse
                                    if (!DateTime.TryParse(reader.Value, out resourceModificationDate))
                                        resourceModificationDate = DateTime.Now;                                    
                                }
                                else if (currentElement.Contains(":getcontenttype"))
                                {
                                    // check if it is an element with valud namespace prefix
                                    if (!CheckIfNameSpaceDAVSpace(currentElement, reader))
                                        continue;

                                    // go one more step
                                    reader.Read();

                                    // parse
                                    resourceContentType = reader.Value;
                                }
                                else if (currentElement.Contains(":collection"))
                                {
                                    // check if it is an element with valud namespace prefix
                                    if (!CheckIfNameSpaceDAVSpace(currentElement, reader))
                                        continue;

                                    // set as directory
                                    bIsDirectory = true;
                                }
                                
                                // go ahead
                                break;
                            }

                        case XmlNodeType.EndElement:
                            {
                                // handle the end of an response
                                if (!reader.Name.ToLower().Contains( ":response"))
                                    break;

                                // check if it is an element with valud namespace prefix
                                if (!CheckIfNameSpaceDAVSpace(reader.Name, reader))
                                    continue;

                                // handle the end of an response, this means
                                // create entry
                                BaseFileEntry entry = null;

                                if (bIsDirectory)
                                    entry = new BaseDirectoryEntry(resourceName, resourceLength, resourceModificationDate, service, session);
                                else
                                    entry = new BaseFileEntry(resourceName, resourceLength, resourceModificationDate, service, session);

                                entry.SetPropertyValue("CreationDate", resourceCreationDate);
                                entry.SetPropertyValue("ContentType", resourceContentType);

                                if (!bIsHidden)
                                {
                                    if (bIsSelf)
                                        results.Self = entry;
                                    else
                                        results.Childs.Add(entry);
                                }

                                // reset all state properties
                                resourceName = String.Empty;
                                resourceLength = 0;
                                resourceModificationDate = DateTime.Now;
                                resourceCreationDate = DateTime.Now;
                                resourceContentType = String.Empty;
                                bIsHidden = false;
                                bIsDirectory = false;

                                // go ahead
                                break;
                            }
                        default:
                            {                             
                                break;
                            }
                    }                    
                };

                if (results.Self == null)
                    throw new Exception("Unknown error in webrequest parser");   
            }
            catch (Exception)
            {                                
            }
            
            // go ahead
            return results;
        }    

        /// <summary>
        /// This method checks if the attached 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static bool CheckIfNameSpaceDAVSpace(String element, XmlTextReader reader)
        {
            // split the element into tag and field
            String[] fields = element.Split(':');
           
            // could be that the element has no namespace attached, so it is not part
            // of the webdav response
            if (fields.Length == 1)
                return false;

            // get the namespace list
            IDictionary<String, String> nameSpaceList = reader.GetNamespacesInScope(XmlNamespaceScope.All);
            
            // get the namespace of our node
            if (!nameSpaceList.ContainsKey(fields[0]))
                return false;

            // get the value
            String NsValue = nameSpaceList[fields[0]];

            // compare if it's a DAV namespce
            if (NsValue.ToLower().Equals("dav:"))
                return true;
            else
                return false;
        }
#else
        public static WebDavRequestResult CreateObjectsFromNetworkStream(Stream data, String targetUrl, IStorageProviderService service, IStorageProviderSession session, NameBaseFilterCallback callback)
        {
            return null;
        }
#endif                
    }    
}
