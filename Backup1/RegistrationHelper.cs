// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Diagnostics;
using Windows7.DesktopIntegration.Registration;

namespace Windows7.DesktopIntegration
{
    /// <summary>
    /// Helper class for registering file associations.
    /// </summary>
    public static class RegistrationHelper
    {
        private static void InternalRegisterFileAssociations(
            bool unregister, string progId, bool registerInHKCU,
            string appId, string openWith, string[] extensions)
        {

            ProcessStartInfo psi = new ProcessStartInfo( 
                    typeof(Windows7.DesktopIntegration.Registration.RegistrationHelperMain).Assembly.Location);
            psi.Arguments =
                progId + " " +
                registerInHKCU + " "
                + appId
                + " \"" + openWith + "\" " +
                unregister + " "
                + string.Join(" ", extensions);
            psi.UseShellExecute = true;
            psi.Verb = "runas"; //Launch elevated
            Process.Start(psi).WaitForExit();
        }

        /// <summary>
        /// Registers file associations for an application.
        /// </summary>
        /// <param name="progId">The application's ProgID.</param>
        /// <param name="registerInHKCU">Whether to register the
        /// association per-user (in HKCU).  The only supported value
        /// at this time is <b>false</b>.</param>
        /// <param name="appId">The application's app-id.</param>
        /// <param name="openWith">The command and arguments to be used
        /// when opening a shortcut to a document.</param>
        /// <param name="extensions">The extensions to register.</param>
        public static void RegisterFileAssociations(string progId,
            bool registerInHKCU, string appId, string openWith,
            params string[] extensions)
        {
            InternalRegisterFileAssociations(
                false, progId, registerInHKCU, appId, openWith, extensions);
        }

        /// <summary>
        /// Unregisters file associations for an application.
        /// </summary>
        /// <param name="progId">The application's ProgID.</param>
        /// <param name="registerInHKCU">Whether to register the
        /// association per-user (in HKCU).  The only supported value
        /// at this time is <b>false</b>.</param>
        /// <param name="appId">The application's app-id.</param>
        /// <param name="openWith">The command and arguments to be used
        /// when opening a shortcut to a document.</param>
        /// <param name="extensions">The extensions to unregister.</param>
        public static void UnregisterFileAssociations(string progId,
            bool registerInHKCU, string appId, string openWith,
            params string[] extensions)
        {
            InternalRegisterFileAssociations(
                true, progId, registerInHKCU, appId, openWith, extensions);
        }
    }
}