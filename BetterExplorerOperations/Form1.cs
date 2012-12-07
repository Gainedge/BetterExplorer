using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace BetterExplorerOperations
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CHANGEFILTERSTRUCT filterStatus = new CHANGEFILTERSTRUCT();
            filterStatus.size = (uint)Marshal.SizeOf(filterStatus);
            filterStatus.info = 0;
            ChangeWindowMessageFilterEx(Handle, 0x4A, ChangeWindowMessageFilterExAction.Allow, ref filterStatus);
        }

        #region Shell API

        public enum SYMBOLIC_LINK_FLAG {
          File = 0,
          Directory = 1
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.I1)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SYMBOLIC_LINK_FLAG dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct SymLinkInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpDestination;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpTarget;
            public int SymType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpMsg;

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
            //[MarshalAs(UnmanagedType.LPStr)]
            //public string lpUserName;
            //[MarshalAs(UnmanagedType.LPStr)]
            //public string lpDomain;
            //[MarshalAs(UnmanagedType.LPStr)]
            //public string lpShare;
            //[MarshalAs(UnmanagedType.LPStr)]
            //public string lpSharingName;
            //[MarshalAs(UnmanagedType.LPStr)]
            //public string lpDescription;
            //public int IsSetPermisions;
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeWindowMessageFilterEx(IntPtr hWnd, uint msg, ChangeWindowMessageFilterExAction action, ref CHANGEFILTERSTRUCT changeInfo);
        public enum MessageFilterInfo : uint
        {
            None = 0, AlreadyAllowed = 1, AlreadyDisAllowed = 2, AllowedHigher = 3
        };

        public enum ChangeWindowMessageFilterExAction : uint
        {
            Reset = 0, Allow = 1, DisAllow = 2
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct CHANGEFILTERSTRUCT
        {
            public uint size;
            public MessageFilterInfo info;
        }

        public const int WM_USER = 0x400;
        public const int WM_COPYDATA = 0x4A; 
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct ShareInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpUserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpDomain;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 999)]
            public string lpShare;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 999)]
            public string lpSharingName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpDescription;
            public int IsSetPermisions;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string lpMsg;

        }

        private void SetExplorerAsDefault(bool IsOpenHandled, bool IsRemove)
        {
            if (!IsRemove)
            {
                RegistryKey rk = Registry.ClassesRoot;
                RegistryKey rksh = rk.OpenSubKey(@"Folder\shell", true);
                RegistryKey rks = rk.OpenSubKey(@"Folder\shell\opennewwindow\command", true);
                RegistryKey rksobe = rk.OpenSubKey(@"Folder\shell\openinbetterexplorer",true);
                if (rksobe == null)
                {
                    rk.CreateSubKey(@"Folder\shell\openinbetterexplorer");
                    rk.CreateSubKey(@"Folder\shell\openinbetterexplorer\command");
                    rksobe = rk.OpenSubKey(@"Folder\shell\openinbetterexplorer", true);
                    rksobe.SetValue("", "Open In Better Explorer");
                    RegistryKey rksobec = rk.OpenSubKey(@"Folder\shell\openinbetterexplorer\command", true);
                    String CurrentexePath =
                    System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                    string dir = Path.GetDirectoryName(CurrentexePath);
                    string ExePath = Path.Combine(dir, @"BetterExplorerShell.exe");
                    rksobec.SetValue("", ExePath + " \"%1\"");
                    rksobec.Close();
                    rksobe.Close();
                }
                else
                {
                    RegistryKey rksobec = rk.OpenSubKey(@"Folder\shell\openinbetterexplorer\command", true);
                    if (rksobec == null)
                    {
                        RegistryKey rkcommand = rk.CreateSubKey(@"Folder\shell\openinbetterexplorer\command");
                        String CurrentexePath =
                         System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                        string dir = Path.GetDirectoryName(CurrentexePath);
                        string ExePath = Path.Combine(dir, @"BetterExplorerShell.exe");
                        rkcommand.SetValue("", ExePath + " \"%1\"");
                        rkcommand.Close();
                        rksobec.Close();
                    }
                    else
                    {
                        String CurrentexePath =
                                                 System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                        string dir = Path.GetDirectoryName(CurrentexePath);
                        string ExePath = Path.Combine(dir, @"BetterExplorerShell.exe");
                        rksobec.SetValue("", ExePath + " \"%1\"");
                        rksobec.Close();
                    }
                }


                rksh.SetValue("", "openinbetterexplorer");
                rks.DeleteValue("DelegateExecute");
                rks.Close();
                rksh.Close();
                rk.Close();
                Close();
            }
            else
            {
                RegistryKey rk = Registry.ClassesRoot;
                RegistryKey rks = rk.OpenSubKey(@"Folder\shell\opennewwindow\command", true);
                RegistryKey rksh = rk.OpenSubKey(@"Folder\shell", true);
                rks.SetValue("DelegateExecute", "{11dbb47c-a525-400b-9e80-a54615a090c0}", RegistryValueKind.String);
                rksh.SetValue("", "");
                rks.Close();
                rksh.Close();
                rk.Close();
                Close();
            }
        }
        private void SetExpandFolderTreeOnNavigate(bool IsRemove)
        {
            if (!IsRemove)
            {
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rks = rk.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
                rks.SetValue("NavPaneExpandToCurrentFolder",1,RegistryValueKind.DWord);
                rks.Close();
                rk.Close();
                Close();
            }
            else
            {
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rks = rk.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
                rks.SetValue("NavPaneExpandToCurrentFolder", 0, RegistryValueKind.DWord);
                rks.Close();
                rk.Close();
                Close();
            }
        }

        private List<string> ListPaths(string str)
        {
            List<string> ret = new List<string>();
            string[] Paths = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in Paths)
            {
                ret.Add(item);
            }
            return ret;
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                switch (m.Msg)
                {

                    case WM_COPYDATA:
                        COPYDATASTRUCT cd = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
                        ShareInfo shi;

                        shi = (ShareInfo)Marshal.PtrToStructure(cd.lpData, typeof(ShareInfo));

                        if (shi.lpMsg == "0x77654")
                        {
                            SetExplorerAsDefault(false, false);
                        }
                        if (shi.lpMsg == "0x77655")
                        {
                            SetExplorerAsDefault(false, true);
                        }
                        if (shi.lpMsg == "Share")
                        {

                        }
                        if (shi.lpMsg == "0x88775")
                        {
                            SetExpandFolderTreeOnNavigate(false);
                        }
                        if (shi.lpMsg == "0x88776")
                        {
                            SetExpandFolderTreeOnNavigate(true);
                        }
                        if (shi.lpMsg == "0x88779")
                        {
                            List<string> sources = ListPaths(shi.lpShare);
                            List<string> drops = ListPaths(shi.lpSharingName);

                            for (int val = 0; val < sources.Count; val++)
                            {
                                string source = sources[val];
                                string drop = drops[val];



                                if (source.StartsWith("(f)"))
                                {

                                    CreateSymbolicLink(drop, source.Substring(3), SYMBOLIC_LINK_FLAG.Directory);
                                }
                                else
                                {
                                    CreateSymbolicLink(drop, source, SYMBOLIC_LINK_FLAG.File);
                                }
                            }

                            //WindowsHelper.WindowsAPI.CreateSymbolicLink(shi.lpSharingName, shi.lpShare, (WindowsAPI.SYMBOLIC_LINK_FLAG)shi.IsSetPermisions);
                        }
                        Close();
                        break;
                }
            }
            catch
            {
                Close();
            }
            base.WndProc(ref m);
        }
    }
  
}
