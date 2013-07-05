using System;
using System.Runtime.InteropServices;
using WindowsHelper;

// David Amenta - dave@DaveAmenta.com
// 05/05/08
// Updated 04/18/2010 for x64

namespace Microsoft.WindowsAPICodePack.Controls.WindowsForms
{
    /// <summary>
    /// Send files directly to the recycle bin.
    /// </summary>
    public class RecycleBin
    {
       

        private static bool IsWOW64Process()
        {
            return IntPtr.Size == 8;
        }

        /// <summary>
        /// Send file to recycle bin
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
        public static bool Send(string path, WindowsAPI.FileOperationFlags flags)
        {
            try
            {
                if (IsWOW64Process())
                {
                  WindowsAPI.SHFILEOPSTRUCT_x64 fs = new WindowsAPI.SHFILEOPSTRUCT_x64();
                  fs.wFunc = WindowsAPI.FileOperationType.FO_DELETE;
                    // important to double-terminate the string.
                    fs.pFrom = path + '\0' + '\0';
                    fs.fFlags = WindowsAPI.FileOperationFlags.FOF_ALLOWUNDO | flags;
                    WindowsAPI.SHFileOperation_x64(ref fs);
                }
                else
                {
                  WindowsAPI.SHFILEOPSTRUCT_x86 fs = new WindowsAPI.SHFILEOPSTRUCT_x86();
                  fs.wFunc = WindowsAPI.FileOperationType.FO_DELETE;
                    // important to double-terminate the string.
                    fs.pFrom = path + '\0' + '\0';
                    fs.fFlags = WindowsAPI.FileOperationFlags.FOF_ALLOWUNDO | flags;
                    WindowsAPI.SHFileOperation_x86(ref fs);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool Send(string path)
        {
          return Send(path, WindowsAPI.FileOperationFlags.FOF_WANTNUKEWARNING | WindowsAPI.FileOperationFlags.FOF_ALLOWUNDO);
        }

        /// <summary>
        /// Send file silently to recycle bin.  Surpress dialog, surpress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool SendSilent(string path)
        {
          return Send(path, WindowsAPI.FileOperationFlags.FOF_NOCONFIRMATION | WindowsAPI.FileOperationFlags.FOF_NOERRORUI | WindowsAPI.FileOperationFlags.FOF_SILENT);
        }
    }
}
