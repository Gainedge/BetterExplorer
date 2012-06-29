using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Globalization;
using System.Management;
using System.Management.Instrumentation;

namespace Microsoft.WindowsAPICodePack.Shell
{

    public class csSharing
    {
        /// <summary>

        /// Creates the share.

        /// </summary>
        public void AddPermissions(string sharedFolderName, string domain, string userName)
        {

            // Step 1 - Getting the user Account Object
            ManagementObject sharedFolder = GetSharedFolderObject(sharedFolderName);
            if (sharedFolder==null)
            {
                System.Diagnostics.Trace.WriteLine("The shared folder with given name does not exist");
                return;
            }

            ManagementBaseObject securityDescriptorObject = sharedFolder.InvokeMethod("GetSecurityDescriptor", null, null);
            if (securityDescriptorObject == null)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error extracting security descriptor of the shared path {0}.", sharedFolderName));
                return;
            }
            int returnCode = Convert.ToInt32(securityDescriptorObject.Properties["ReturnValue"].Value);
            if (returnCode != 0)
            {
                System.Diagnostics.Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error extracting security descriptor of the shared path {0}. Error Code{1}.", sharedFolderName, returnCode.ToString()));
                return;
            }

            ManagementBaseObject securityDescriptor = securityDescriptorObject.Properties["Descriptor"].Value as ManagementBaseObject;

            // Step 2 -- Extract Access Control List from the security descriptor
            int existingAcessControlEntriesCount = 0;
            ManagementBaseObject[] accessControlList = securityDescriptor.Properties["DACL"].Value as ManagementBaseObject[];

            if (accessControlList == null)
            {
                // If there aren't any entries in access control list or the list is empty - create one
                accessControlList = new ManagementBaseObject[1];
            }
            else
            {
                // Otherwise, resize the list to allow for all new users.
                existingAcessControlEntriesCount = accessControlList.Length;
                Array.Resize(ref accessControlList, accessControlList.Length + 1);
            }


            // Step 3 - Getting the user Account Object
            ManagementObject userAccountObject = GetUserAccountObject(domain, userName);
            ManagementObject securityIdentfierObject = new ManagementObject(string.Format("Win32_SID.SID='{0}'", (string)userAccountObject.Properties["SID"].Value));
            securityIdentfierObject.Get();

            // Step 4 - Create Trustee Object
            ManagementObject trusteeObject = CreateTrustee(domain, userName, securityIdentfierObject);

            // Step 5 - Create Access Control Entry
            ManagementObject accessControlEntry = CreateAccessControlEntry(trusteeObject, false);

            // Step 6 - Add Access Control Entry to the Access Control List
            accessControlList[existingAcessControlEntriesCount] = accessControlEntry;

            // Step 7 - Assign access Control list to security desciptor
            securityDescriptor.Properties["DACL"].Value = accessControlList;

            // Step 8 - Assign access Control list to security desciptor
            ManagementBaseObject parameterForSetSecurityDescriptor = sharedFolder.GetMethodParameters("SetSecurityDescriptor");
            parameterForSetSecurityDescriptor["Descriptor"] = securityDescriptor;
            sharedFolder.InvokeMethod("SetSecurityDescriptor", parameterForSetSecurityDescriptor, null);
        }

        /// <summary>
        /// The method returns ManagementObject object for the shared folder with given name
        /// </summary>
        /// <param name="sharedFolderName">string containing name of shared folder</param>
        /// <returns>Object of type ManagementObject for the shared folder.</returns>
        private static ManagementObject GetSharedFolderObject(string sharedFolderName)
        {
            ManagementObject sharedFolderObject = null;

            //Creating a searcher object to search
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_LogicalShareSecuritySetting where Name = '" + sharedFolderName + "'");
            ManagementObjectCollection resultOfSearch = searcher.Get();
            if (resultOfSearch.Count > 0)
            {
            //The search might return a number of objects with same shared name. I assume there is just going to be one
                foreach (ManagementObject sharedFolder in resultOfSearch)
                {
                    sharedFolderObject = sharedFolder;
                    break;
                }
            }
            return sharedFolderObject;
        }

        /// <summary>
        /// The method returns ManagementObject object for the user folder with given name
        /// </summary>
        /// <param name="domain">string containing domain name of user </param>
        /// <param name="alias">string containing the user's network name </param>
        /// <returns>Object of type ManagementObject for the user folder.</returns>
        private static ManagementObject GetUserAccountObject(string domain, string alias)
        {
            ManagementObject userAccountObject = null;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(string.Format("select * from Win32_Account where Name = '{0}' and Domain='{1}'", alias, domain));
            ManagementObjectCollection resultOfSearch = searcher.Get();
            if (resultOfSearch.Count > 0)
            {
                foreach (ManagementObject userAccount in resultOfSearch)
                {
                    userAccountObject = userAccount;
                    break;
                }
            }
            return userAccountObject;
        }

        /// <summary>
        /// Returns the Security Identifier Sid of the given user
        /// </summary>
        /// <param name="userAccountObject">The user object who's Sid needs to be returned</param>
        /// <returns></returns>

        private static ManagementObject GetAccountSecurityIdentifier(ManagementBaseObject userAccountObject)
        {
            ManagementObject securityIdentfierObject = new ManagementObject(string.Format("Win32_SID.SID='{0}'", (string)userAccountObject.Properties["SID"].Value));
            securityIdentfierObject.Get();
            return securityIdentfierObject;
        }

        /// <summary>

        /// Create a trustee object for the given user

        /// </summary>

        /// <param name="domain">name of domain</param>

        /// <param name="userName">the network name of the user</param>

        /// <param name="securityIdentifierOfUser">Object containing User's sid</param>

        /// <returns></returns>

        private static ManagementObject CreateTrustee(string domain, string userName, ManagementObject securityIdentifierOfUser)
        {
            ManagementObject trusteeObject = new ManagementClass("Win32_Trustee").CreateInstance();
            trusteeObject.Properties["Domain"].Value = domain;
            trusteeObject.Properties["Name"].Value = userName;
            trusteeObject.Properties["SID"].Value = securityIdentifierOfUser.Properties["BinaryRepresentation"].Value;
            trusteeObject.Properties["SidLength"].Value = securityIdentifierOfUser.Properties["SidLength"].Value;
            trusteeObject.Properties["SIDString"].Value = securityIdentifierOfUser.Properties["SID"].Value;
            return trusteeObject;
        }


        /// <summary>

        /// Create an Access Control Entry object for the given user

        /// </summary>

        /// <param name="trustee">The user's trustee object</param>

        /// <param name="deny">boolean to say if user permissions should be assigned or denied</param>

        /// <returns></returns>

        private static ManagementObject CreateAccessControlEntry(ManagementObject trustee, bool deny)
        {
            ManagementObject aceObject = new ManagementClass("Win32_ACE").CreateInstance();

            aceObject.Properties["AccessMask"].Value = 0x1U | 0x2U | 0x4U | 0x8U | 0x10U | 0x20U | 0x40U | 0x80U | 0x100U | 0x10000U | 0x20000U | 0x40000U | 0x80000U | 0x100000U; // all permissions
            aceObject.Properties["AceFlags"].Value = 0x0U; // no flags
            aceObject.Properties["AceType"].Value = deny ? 1U : 0U; // 0 = allow, 1 = deny
            aceObject.Properties["Trustee"].Value = trustee;
            return aceObject;
        }

        public void ShareFolder(string FolderPath, string SharingName, string Description)
        {
 

                ManagementClass management = new ManagementClass("Win32_Share");
                ManagementBaseObject input = management.GetMethodParameters("Create");
                ManagementBaseObject output;

                input["Description"] = Description;
                input["Name"] = SharingName;
                input["Path"] = FolderPath;
                input["Type"] = 0x0;

                output = management.InvokeMethod("Create", input, null);

                if ((uint)(output.Properties["ReturnValue"].Value) != 0)
                {
                    throw new Exception("Unable To Share The Directory");
                }
                //else
                //{
                //    Console.WriteLine("Directory Successfully Shared");
                //}

        }
    }
}
