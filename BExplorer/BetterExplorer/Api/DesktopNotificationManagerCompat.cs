// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Windows.UI.Notifications;

namespace BetterExplorer.Api {

  public class DesktopNotificationManagerCompat {
    public const string TOAST_ACTIVATED_LAUNCH_ARG = "-ToastActivated";
    private static bool _registeredAumidAndComServer;
    private static string _aumid;
    private static bool _registeredActivator;
    /// <summary>
    /// If not running under the Desktop Bridge, you must call this method to register your AUMID with the Compat library and to
    /// register your COM CLSID and EXE in LocalServer32 registry. Feel free to call this regardless, and we will no-op if running
    /// under Desktop Bridge. Call this upon application startup, before calling any other APIs.
    /// </summary>
    /// <param name="aumid">An AUMID that uniquely identifies your application.</param>
    public static void RegisterAumidAndComServer<T>(string aumid)
      where T : NotificationActivator {
      if (string.IsNullOrWhiteSpace(aumid)) {
        throw new ArgumentException("You must provide an AUMID.", nameof(aumid));
      }

      // If running as Desktop Bridge
      if (DesktopBridgeHelpers.IsRunningAsUwp()) {
        // Clear the AUMID since Desktop Bridge doesn't use it, and then we're done.
        // Desktop Bridge apps are registered with platform through their manifest.
        // Their LocalServer32 key is also registered through their manifest.
        DesktopNotificationManagerCompat._aumid = null;
        DesktopNotificationManagerCompat._registeredAumidAndComServer = true;
        return;
      }

      DesktopNotificationManagerCompat._aumid = aumid;

      String exePath = Process.GetCurrentProcess().MainModule.FileName;
      DesktopNotificationManagerCompat.RegisterComServer<T>(exePath);

      DesktopNotificationManagerCompat._registeredAumidAndComServer = true;
    }
    private static void RegisterComServer<T>(String exePath)
      where T : NotificationActivator {
      // We register the EXE to start up when the notification is activated
      string regString = String.Format("SOFTWARE\\Classes\\CLSID\\{{{0}}}\\LocalServer32", typeof(T).GUID);
      var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(regString);

      // Include a flag so we know this was a toast activation and should wait for COM to process
      // We also wrap EXE path in quotes for extra security
      key.SetValue(null, '"' + exePath + '"' + " " + DesktopNotificationManagerCompat.TOAST_ACTIVATED_LAUNCH_ARG);
    }
    /// <summary>
    /// Registers the activator type as a COM server client so that Windows can launch your activator.
    /// </summary>
    /// <typeparam name="T">Your implementation of NotificationActivator. Must have GUID and ComVisible attributes on class.</typeparam>
    public static void RegisterActivator<T>()
      where T : NotificationActivator {
      // Register type
      var regService = new RegistrationServices();

      regService.RegisterTypeForComClients(
        typeof(T),
        RegistrationClassContext.LocalServer,
        RegistrationConnectionType.MultipleUse);

      DesktopNotificationManagerCompat._registeredActivator = true;
    }
    /// <summary>
    /// Creates a toast notifier. You must have called <see cref="RegisterActivator{T}"/> first (and also <see cref="RegisterAumidAndComServer(string)"/> if you're a classic Win32 app), or this will throw an exception.
    /// </summary>
    /// <returns></returns>
    public static ToastNotifier CreateToastNotifier() {
      DesktopNotificationManagerCompat.EnsureRegistered();

      if (DesktopNotificationManagerCompat._aumid != null) {
        // Non-Desktop Bridge
        return ToastNotificationManager.CreateToastNotifier(DesktopNotificationManagerCompat._aumid);
      } else {
        // Desktop Bridge
        return ToastNotificationManager.CreateToastNotifier();
      }
    }
    /// <summary>
    /// Gets the <see cref="DesktopNotificationHistoryCompat"/> object. You must have called <see cref="RegisterActivator{T}"/> first (and also <see cref="RegisterAumidAndComServer(string)"/> if you're a classic Win32 app), or this will throw an exception.
    /// </summary>
    public static DesktopNotificationHistoryCompat History {
      get {
        DesktopNotificationManagerCompat.EnsureRegistered();

        return new DesktopNotificationHistoryCompat(DesktopNotificationManagerCompat._aumid);
      }
    }
    private static void EnsureRegistered() {
      // If not registered AUMID yet
      if (!DesktopNotificationManagerCompat._registeredAumidAndComServer) {
        // Check if Desktop Bridge
        if (DesktopBridgeHelpers.IsRunningAsUwp()) {
          // Implicitly registered, all good!
          DesktopNotificationManagerCompat._registeredAumidAndComServer = true;
        } else {
          // Otherwise, incorrect usage
          throw new Exception("You must call RegisterAumidAndComServer first.");
        }
      }

      // If not registered activator yet
      if (!DesktopNotificationManagerCompat._registeredActivator) {
        // Incorrect usage
        throw new Exception("You must call RegisterActivator first.");
      }
    }
    /// <summary>
    /// Gets a boolean representing whether http images can be used within toasts. This is true if running under Desktop Bridge.
    /// </summary>
    public static bool CanUseHttpImages { get { return DesktopBridgeHelpers.IsRunningAsUwp(); } }
    /// <summary>
    /// Code from https://github.com/qmatteoq/DesktopBridgeHelpers/edit/master/DesktopBridge.Helpers/Helpers.cs
    /// </summary>
    private class DesktopBridgeHelpers {
      const long APPMODEL_ERROR_NO_PACKAGE = 15700L;
      [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);
      private static bool? _isRunningAsUwp;
      public static bool IsRunningAsUwp() {
        if (DesktopBridgeHelpers._isRunningAsUwp == null) {
          if (DesktopBridgeHelpers.IsWindows7OrLower) {
            DesktopBridgeHelpers._isRunningAsUwp = false;
          } else {
            int length = 0;
            StringBuilder sb = new StringBuilder(0);
            int result = DesktopBridgeHelpers.GetCurrentPackageFullName(ref length, sb);

            sb = new StringBuilder(length);
            result = DesktopBridgeHelpers.GetCurrentPackageFullName(ref length, sb);

            DesktopBridgeHelpers._isRunningAsUwp = result != DesktopBridgeHelpers.APPMODEL_ERROR_NO_PACKAGE;
          }
        }

        return DesktopBridgeHelpers._isRunningAsUwp.Value;
      }
      private static bool IsWindows7OrLower {
        get {
          int versionMajor = Environment.OSVersion.Version.Major;
          int versionMinor = Environment.OSVersion.Version.Minor;
          double version = versionMajor + (double)versionMinor / 10;
          return version <= 6.1;
        }
      }
    }
  }

}