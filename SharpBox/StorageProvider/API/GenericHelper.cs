using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppLimit.CloudComputing.SharpBox.StorageProvider.API
{
    internal class GenericHelper
    {        
        public static String GetResourcePath(ICloudFileSystemEntry entry)
        {
            ICloudFileSystemEntry current = entry;
            String path = "";

            while (current != null)
            {
                if (current.Name != "/")
                {
                    if (path == String.Empty)
                        path = current.Name;
                    else
                        path = current.Name + "/" + path;
                }
                else
                    path = "/" + path;

                current = current.Parent;
            }
                      
            return path;
        }        
    }
}
