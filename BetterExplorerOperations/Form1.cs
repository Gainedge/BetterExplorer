using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using WindowsHelper;

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

        #region API
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

        private void SetExplorerAsDefault(bool IsOpenHandled, bool IsRemove)
        {
            if (!IsRemove)
            {
                RegistryKey rk = Registry.ClassesRoot;
                RegistryKey rks = rk.OpenSubKey(@"Folder\shell\opennewwindow\command", true);
                String CurrentexePath = 
                    System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                string dir = Path.GetDirectoryName(CurrentexePath);
                string ExePath = Path.Combine(dir, @"BetterExplorer.exe");
                rks.SetValue("", ExePath + " \"%1\"");
                rks.DeleteValue("DelegateExecute");
                rks.Close();
                rk.Close();
                Close();
            }
            else
            {
                RegistryKey rk = Registry.ClassesRoot;
                RegistryKey rks = rk.OpenSubKey(@"Folder\shell\opennewwindow\command", true);
                rks.SetValue("DelegateExecute", "{11dbb47c-a525-400b-9e80-a54615a090c0}", RegistryValueKind.String);
                rks.Close();
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
        private void SetSharing(string FolderPath, string Sharename, string Description)
        {
            csSharing csh = new csSharing();
            MessageBox.Show(FolderPath);
            csh.ShareFolder(FolderPath, Sharename, Description,false);
            csh = null;
        }
        private void CreateSymLink()
        {
            
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

                            csSharing csh = new csSharing();

                            csh.ShareFolder(shi.lpShare, shi.lpSharingName, shi.lpDescription, false);
                            csh = null;
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

                                    WindowsHelper.WindowsAPI.CreateSymbolicLink(drop, source.Substring(3), WindowsAPI.SYMBOLIC_LINK_FLAG.Directory);
                                }
                                else
                                {
                                    WindowsHelper.WindowsAPI.CreateSymbolicLink(drop, source, WindowsAPI.SYMBOLIC_LINK_FLAG.File);
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
