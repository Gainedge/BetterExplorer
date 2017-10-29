using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using wyUpdate.Common;

#if WPF
using System.Reflection;
#elif !CLIENT
using System.Windows.Forms;
#endif

namespace wyDay.Controls
{
    internal enum AutoUpdaterStatus
    {
        Nothing = 0,
        UpdateSucceeded = 1,
        UpdateFailed = 2
    }

    internal class AutoUpdaterInfo
    {
#if !CLIENT
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process([In] IntPtr hProcess, [Out, MarshalAs(UnmanagedType.Bool)] out bool lpSystemInfo);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", EntryPoint = "Wow64EnableWow64FsRedirection")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableWow64FSRedirection([MarshalAs(UnmanagedType.Bool)] bool enable);

        static bool? is32on64;

        static bool Is32BitProcessOn64BitProcessor()
        {
            if (is32on64 == null)
            {
                // if we're 64-bit process then just bail out
                if (IntPtr.Size == 8)
                {
                    is32on64 = false;
                    return false;
                }

                UIntPtr proc = GetProcAddress(GetModuleHandle("kernel32.dll"), "IsWow64Process");

                if (proc == UIntPtr.Zero)
                    is32on64 = false;
                else
                {
                    bool retVal;

                    IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);

                    is32on64 = retVal;
                }
            }

            return is32on64.Value;
        }
#endif

        public DateTime LastCheckedForUpdate;
        public UpdateStepOn UpdateStepOn;

        public AutoUpdaterStatus AutoUpdaterStatus = AutoUpdaterStatus.Nothing;

        public string UpdateVersion;
        public string ChangesInLatestVersion;
        public bool ChangesIsRTF;


        public string ErrorTitle;
        public string ErrorMessage;


        readonly string autoUpdateID;

        readonly string[] filenames = new string[2];

        public AutoUpdaterInfo(string auID, string oldAUTempFolder)
        {
            autoUpdateID = auID;

            // get the admin filename
            filenames[0] = GetFilename();

#if CLIENT
            // if tempFolder is not in ApplicationData, then we're updating on behalf of a limited user
            if (oldAUTempFolder != null && !SystemFolders.IsDirInDir(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), oldAUTempFolder))
            {
                // AutoUpdateFiles are stored in: %appdata%\wyUpdate AU\
                // The tempFolder is:             %appdata%\wyUpdate AU\cache\AppGUID\

                // get the limited user's AutoUpdate file
                filenames[1] = Path.Combine(oldAUTempFolder, "..\\..\\" + AutoUpdateID + ".autoupdate");

                // check if LimitedUser AutoUpdateFile exists
                if (!File.Exists(filenames[1]))
                    filenames[1] = null;
            }
#endif

            bool failedToLoad;

            bool firstFailed = false;
            int retriedTimes = 0;

            while (true)
            {
                try
                {
                    // try to load the AutoUpdatefile for limited user
                    if (filenames[1] != null && !firstFailed)
                        Load(filenames[1]);
                    else // load the admin user
                        Load(filenames[0]);

                    failedToLoad = false;
                }
                catch (IOException IOEx)
                {
                    int HResult = Marshal.GetHRForException(IOEx);

                    // if sharing violation
                    if ((HResult & 0xFFFF) == 32)
                    {
                        // sleep for 1/2 second
                        Thread.Sleep(500);

                        // if we're skipping UI and we've already waited 20 seconds for a file to be released
                        // then throw the exception, rollback updates, etc
                        if (retriedTimes != 20)
                        {
                            // otherwise, retry file copy
                            ++retriedTimes;
                            continue;
                        }
                    }

                    failedToLoad = true;

                    // the first has already failed (the second just failed)
                    if (firstFailed)
                        break;

                    firstFailed = true;
                    continue;
                }
                catch
                {
                    failedToLoad = true;

                    // the first has already failed (the second just failed)
                    if (firstFailed)
                        break;

                    firstFailed = true;
                    continue;
                }

                break;
            }

            if (failedToLoad)
            {
                LastCheckedForUpdate = DateTime.MinValue;
                UpdateStepOn = UpdateStepOn.Nothing;
            }
        }

        public string AutoUpdateID
        {
            get
            {
                if (string.IsNullOrEmpty(autoUpdateID))
                {
#if WPF
                    return Path.GetFileName(Assembly.GetEntryAssembly().Location);
#elif CLIENT
                    return Path.GetFileName(VersionTools.SelfLocation);
#else
                    return Path.GetFileName(Application.ExecutablePath);
#endif
                }

                return autoUpdateID;
            }
        }

        string GetFilename()
        {
            string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "wyUpdate AU");

#if !CLIENT
            // Disable filesystem redirection on x64 (mostly for Windows Services)
            if (Is32BitProcessOn64BitProcessor())
                EnableWow64FSRedirection(false);

            try
            {
#endif
                if (!Directory.Exists(filename))
                {
                    Directory.CreateDirectory(filename);
                    File.SetAttributes(filename, FileAttributes.System | FileAttributes.Hidden);
                }
#if !CLIENT
            }
            finally
            {
                // Re-enable filesystem redirection on x64
                if (Is32BitProcessOn64BitProcessor())
                    EnableWow64FSRedirection(true);
            }
#endif

            filename = Path.Combine(filename, AutoUpdateID + ".autoupdate");

            return filename;
        }

        // not using registry because .NET 2.0 has bad support for x64/x86 access
        public void Save()
        {
#if !CLIENT
            // Disable filesystem redirection on x64 (mostly for Windows Services)
            if (Is32BitProcessOn64BitProcessor())
                EnableWow64FSRedirection(false);

            try
            {
#endif
                int retriedTimes = 0;

                while (true)
                {
                    try
                    {
                        // save for each filename
                        Save(filenames[0]);

                        if (filenames[1] != null)
                            Save(filenames[1]);
                    }
                    catch (IOException IOEx)
                    {
                        int HResult = Marshal.GetHRForException(IOEx);

                        // if sharing violation
                        if ((HResult & 0xFFFF) == 32)
                        {
                            // sleep for 1/2 second
                            Thread.Sleep(500);

                            // if we're skipping UI and we've already waited 20 seconds for a file to be released
                            // then throw the exception, rollback updates, etc
                            if (retriedTimes == 20)
                                throw;

                            // otherwise, retry file copy
                            ++retriedTimes;
                            continue;
                        }

                        throw;
                    }

                    break;
                }
#if !CLIENT
            }
            finally
            {
                // Re-enable filesystem redirection on x64
                if (Is32BitProcessOn64BitProcessor())
                    EnableWow64FSRedirection(true);
            }
#endif
        }

        void Save(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                // Write any file-identification data you want to here
                WriteFiles.WriteHeader(fs, "AUIF");

#if CLIENT
                UpdateStepOn = UpdateStepOn.Nothing;
#endif

                // Date last checked for update
                WriteFiles.WriteDateTime(fs, 0x01, LastCheckedForUpdate);

                // update step on
                WriteFiles.WriteInt(fs, 0x02, (int)UpdateStepOn);

#if CLIENT
                // only save the AutoUpdaterStatus when wyUpdate writes the file
                WriteFiles.WriteInt(fs, 0x03, (int)AutoUpdaterStatus);
#endif

                if (!string.IsNullOrEmpty(UpdateVersion))
                    WriteFiles.WriteString(fs, 0x04, UpdateVersion);

                if (!string.IsNullOrEmpty(ChangesInLatestVersion))
                {
                    WriteFiles.WriteString(fs, 0x05, ChangesInLatestVersion);

                    WriteFiles.WriteBool(fs, 0x06, ChangesIsRTF);
                }


#if CLIENT
                if (!string.IsNullOrEmpty(ErrorTitle))
                    WriteFiles.WriteString(fs, 0x07, ErrorTitle);

                if (!string.IsNullOrEmpty(ErrorMessage))
                    WriteFiles.WriteString(fs, 0x08, ErrorMessage);
#endif

                fs.WriteByte(0xFF);
            }
        }

        void Load(string filename)
        {
#if !CLIENT
            // Disable filesystem redirection on x64 (mostly for Windows Services)
            if (Is32BitProcessOn64BitProcessor())
                EnableWow64FSRedirection(false);

            try
            {
#endif
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    if (!ReadFiles.IsHeaderValid(fs, "AUIF"))
                    {
                        //free up the file so it can be deleted
                        fs.Close();
                        throw new Exception("Auto update state file ID is wrong.");
                    }

                    byte bType = (byte)fs.ReadByte();
                    while (!ReadFiles.ReachedEndByte(fs, bType, 0xFF))
                    {
                        switch (bType)
                        {
                            case 0x01: // Date last checked for update
                                LastCheckedForUpdate = ReadFiles.ReadDateTime(fs);
                                break;

                            case 0x02: // update step on
                                UpdateStepOn = (UpdateStepOn) ReadFiles.ReadInt(fs);
                                break;

                            case 0x03:
                                AutoUpdaterStatus = (AutoUpdaterStatus) ReadFiles.ReadInt(fs);
                                break;

                            case 0x04: // update succeeded
                                UpdateVersion = ReadFiles.ReadString(fs);
                                break;

                            case 0x05:
                                ChangesInLatestVersion = ReadFiles.ReadString(fs);
                                break;

                            case 0x06:
                                ChangesIsRTF = ReadFiles.ReadBool(fs);
                                break;

                            case 0x07: // update failed
                                ErrorTitle = ReadFiles.ReadString(fs);
                                break;

                            case 0x08:
                                ErrorMessage = ReadFiles.ReadString(fs);
                                break;

                            default:
                                ReadFiles.SkipField(fs, bType);
                                break;
                        }

                        bType = (byte)fs.ReadByte();
                    }
                }
#if !CLIENT
            }
            finally
            {
                // Re-enable filesystem redirection on x64
                if (Is32BitProcessOn64BitProcessor())
                    EnableWow64FSRedirection(true);
            }
#endif
        }

        public void ClearSuccessError()
        {
            AutoUpdaterStatus = AutoUpdaterStatus.Nothing;

            UpdateVersion = null;
            ChangesInLatestVersion = null;
            ChangesIsRTF = false;

            ErrorTitle = null;
            ErrorMessage = null;
        }
    }
}
