using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;
using System.IO;

namespace InstallCustom
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        public Installer1()
        {
            InitializeComponent();
        }
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(IDictionary savedState)
        {


            base.Commit(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
          RegistryKey rk = Registry.ClassesRoot;
          RegistryKey rks = rk.OpenSubKey(@"Folder\shell\opennewwindow\command", true);
          RegistryKey rksh = rk.OpenSubKey(@"Folder\shell", true);
          rks.SetValue("DelegateExecute", "{11dbb47c-a525-400b-9e80-a54615a090c0}", RegistryValueKind.String);
          rksh.SetValue("", "");
          rks.Close();
          rksh.Close();
          rk.Close();
            base.Uninstall(savedState);
            

        }

    }
}
