using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

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
        Boolean IsCheckForTestBuilds= false;
        #endregion

        #region Properties
        public List<Update> AvailableUpdates { get; set; }
        public int CheckingInterval { get; set; }
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


        public Boolean LoadUpdateFile()
        {
          try {
            this.AvailableUpdates.Clear();
            XmlDocument updateXML = new XmlDocument();
            updateXML.Load(this.ServerCheckLocation);
            foreach (XmlNode updateNode in updateXML.DocumentElement.ChildNodes) {
              var updateType = (UpdateTypes)Convert.ToInt32(updateNode.ChildNodes[1].InnerText);
              if (updateType != UpdateTypes.Nightly & updateType != UpdateTypes.Alpha & updateType != UpdateTypes.Beta) {
                Update update = new Update();
                update.Name = updateNode.Attributes["Name"].Value;
                update.Version = updateNode.ChildNodes[0].InnerText;
                update.Type = (UpdateTypes)Convert.ToInt32(updateNode.ChildNodes[1].InnerText);
                update.RequiredVersion = updateNode.ChildNodes[2].InnerText;
                update.UpdaterFilePath = updateNode.ChildNodes[3].InnerText;
                update.UpdaterFilePath64 = updateNode.ChildNodes[4].InnerText;
                this.AvailableUpdates.Add(update);
              } else if (IsCheckForTestBuilds) {
                Update update = new Update();
                update.Name = updateNode.Attributes["Name"].Value;
                update.Version = updateNode.ChildNodes[0].InnerText;
                update.Type = (UpdateTypes)Convert.ToInt32(updateNode.ChildNodes[1].InnerText);
                update.RequiredVersion = updateNode.ChildNodes[2].InnerText;
                update.UpdaterFilePath = updateNode.ChildNodes[3].InnerText;
                update.UpdaterFilePath64 = updateNode.ChildNodes[4].InnerText;
                this.AvailableUpdates.Add(update);
              }
            }
            Version vCurrent = new Version(CurrentVersion);
            Version vOnline = new Version(this.AvailableUpdates[0].Version);
            return (this.AvailableUpdates.Count > 0 && vOnline > vCurrent);
          } catch (Exception) {

            return false;
          }
        }

        /// <summary>
        /// Create a new updater to handle checking for and downloading updates.
        /// </summary>
        public Updater(String xmlLocation, int checkingInterval, bool IsCheckForTestBuilds)
        {
            updchk.DownloadProgressChanged += updchk_DownloadProgressChanged;
            updchk.DownloadFileCompleted += updchk_DownloadFileCompleted;
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BetterExplorerDownloads")))
              Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BetterExplorerDownloads"));
            this.LocalUpdaterLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BetterExplorerDownloads");
            this.ServerCheckLocation = xmlLocation;
            this.CheckingInterval = checkingInterval;
            this.AvailableUpdates = new List<Update>();
            this.IsCheckForTestBuilds = IsCheckForTestBuilds;
            this.CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }


        private void updchk_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (upd == true)
            {
                if (e.Error == null)
                {
                    try
                    {
                        if (LoadUpdateFile())
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
        public void DownloadUpdater(String location, String DestinationName) {
          upd = false;
          OnDownloadUpdaterBegan(EventArgs.Empty);
          updchk.DownloadFileAsync(new Uri(location), Path.Combine(LocalUpdaterLocation, DestinationName));

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

    public class Update {
      public String Name { get; set; }
      public String Version { get; set; }
      public UpdateTypes Type { get; set; }
      public String RequiredVersion { get; set; }
      public String UpdaterFilePath { get; set; }
      public String UpdaterFilePath64 { get; set; }
    }

    public enum UpdateTypes : int {
      Nightly = 1,
      Alpha,
      Beta,
      ReleaseCandidate,
      Release
    }
}
