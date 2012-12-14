using System;
using System.ComponentModel;
using System.IO;

namespace BetterExplorer
{
    public class Updater
    {

        #region Variables
        Boolean upd = false; // true if currently checking for updates
        System.Net.WebClient updchk = new System.Net.WebClient(); // object that downloads file and update
        string los = ""; // online location of file that will be used to check for updates
        string loc = ""; // location of local file that is checked for if there are updates or not
        string curr = ""; // version of this build
        string tis = ""; // online location of program that will update software
        string tid = ""; // local location of the program that will update software
        #endregion

        #region Properties
        /// <summary>
        /// The remote location of a file that is used to check if an update exists.
        /// </summary>
        public string ServerCheckLocation
        {
            get { return loc; }
            set { loc = value; }
        }

        /// <summary>
        /// The location of a file on the local system that is used to check if an update exists.
        /// </summary>
        public string LocalCheckLocation
        {
            get { return loc; }
            set { loc = value; }
        }

        /// <summary>
        /// A string representation of the current version of this software.
        /// </summary>
        public string CurrentVersion
        {
            get { return curr; }
            set { curr = value; }
        }

        /// <summary>
        /// The location of a file on a remote location that contains a program that will install the update.
        /// </summary>
        public string ServerUpdaterLocation
        {
            get { return tis; }
            set { tis = value; }
        }

        /// <summary>
        /// The location of a file on the local system that contains a program that will install the update.
        /// </summary>
        public string LocalUpdaterLocation
        {
            get { return tid; }
            set { tid = value; }
        }

        #endregion

        private string LoadTextFile(string file)
        {
            string line;
            using (StreamReader sr = new StreamReader(file))
            {
                line = sr.ReadLine();
            }
            return line;
        }

        /// <summary>
        /// Create a new updater to handle checking for and downloading updates.
        /// </summary>
        public Updater()
        {
            updchk.DownloadProgressChanged += updchk_DownloadProgressChanged;
            updchk.DownloadFileCompleted += updchk_DownloadFileCompleted;
        }

        private void updchk_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (upd == true)
            {
                if (e.Error == null)
                {
                    try
                    {
                        if (LoadTextFile(LocalCheckLocation) != CurrentVersion)
                        {
                            OnUpdateAvailable(EventArgs.Empty);
                            // update available
                        }
                        else
                        {
                            OnNoUpdatesNeeded(EventArgs.Empty);
                            // up-to-date
                        }
                    }
                    catch (Exception ex)
                    {
                        OnErrorOccurredWhileChecking(new ExceptionEventArgs(ex));
                        // error
                    }
                }
                else
                {
                    OnErrorOccurredWhileChecking(new ExceptionEventArgs(e.Error));
                    // error
                }
            }
            else
            {
                OnUpdaterDownloadComplete(new PathEventArgs(LocalUpdaterLocation));
                // ready to install update
            }
        }

        void updchk_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (upd == false)
            {
                OnUpdaterDownloadProgressChanged(e);
            }
            else
            {
                OnPackageDownloadProgressChanged(e);
            }
        }

        /// <summary>
        /// Start downloading the updater program.
        /// </summary>
        public void DownloadUpdater()
        {
            upd = false;
            OnDownloadUpdaterBegan(EventArgs.Empty);
            updchk.DownloadFileAsync(new Uri(ServerUpdaterLocation), LocalUpdaterLocation);
        }

        /// <summary>
        /// Download the update data file and check for updates.
        /// </summary>
        public void CheckForUpdates()
        {
            upd = true;
            OnCheckForUpdatesBegan(EventArgs.Empty);
            updchk.DownloadFileAsync(new Uri(ServerCheckLocation), LocalCheckLocation);
        }

        #region Events

        public delegate void PathEventHandler(object sender, PathEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event PathEventHandler UpdaterDownloadComplete;
        //public event EventHandler MouseDoubleClick;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnUpdaterDownloadComplete(PathEventArgs e)
        {
            if (UpdaterDownloadComplete != null)
                UpdaterDownloadComplete(this, e);
        }

        public class PathEventArgs
        {
            string _path;

            public PathEventArgs(string path = null)
            {
                _path = path;
            }

            public string Path
            {
                get
                {
                    return _path;
                }
            }

        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler DownloadUpdaterBegan;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnDownloadUpdaterBegan(EventArgs e)
        {
            if (DownloadUpdaterBegan != null)
                DownloadUpdaterBegan(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler CheckForUpdatesBegan;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnCheckForUpdatesBegan(EventArgs e)
        {
            if (CheckForUpdatesBegan != null)
                CheckForUpdatesBegan(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler UpdateAvailable;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnUpdateAvailable(EventArgs e)
        {
            if (UpdateAvailable != null)
                UpdateAvailable(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler NoUpdatesNeeded;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnNoUpdatesNeeded(EventArgs e)
        {
            if (NoUpdatesNeeded != null)
                NoUpdatesNeeded(this, e);
        }

        public delegate void ExceptionEventHandler(object sender, ExceptionEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event ExceptionEventHandler ErrorOccurredWhileChecking;
        //public event EventHandler MouseDoubleClick;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnErrorOccurredWhileChecking(ExceptionEventArgs e)
        {
            if (ErrorOccurredWhileChecking != null)
                ErrorOccurredWhileChecking(this, e);
        }

        public class ExceptionEventArgs
        {
            Exception _ex;

            public ExceptionEventArgs(Exception ex = null)
            {
                _ex = ex;
            }

            public Exception Exception
            {
                get
                {
                    return _ex;
                }
            }

        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event System.Net.DownloadProgressChangedEventHandler UpdaterDownloadProgressChanged;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnUpdaterDownloadProgressChanged(System.Net.DownloadProgressChangedEventArgs e)
        {
            if (UpdaterDownloadProgressChanged != null)
                UpdaterDownloadProgressChanged(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event System.Net.DownloadProgressChangedEventHandler PackageDownloadProgressChanged;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnPackageDownloadProgressChanged(System.Net.DownloadProgressChangedEventArgs e)
        {
            if (PackageDownloadProgressChanged != null)
                PackageDownloadProgressChanged(this, e);
        }

        #endregion
    }
}
