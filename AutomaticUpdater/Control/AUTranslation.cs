namespace wyDay.Controls
{
    public class AUTranslation
    {
        internal const string C_PrematureExitTitle = "wyUpdate exited prematurely";
        internal const string C_PrematureExitMessage = "wyUpdate ended before the current update step could be completed.";

#if WPF
        // wpf menus require underscores for hint characters (not working, look into this)
        string m_CheckForUpdatesMenu = "Check for updates";
        string m_DownloadUpdateMenu = "Download and Update now";
        string m_InstallUpdateMenu = "Install update now";
        string m_CancelUpdatingMenu = "Cancel updating";
        string m_CancelCheckingMenu = "Cancel update checking";
#else
        string m_CheckForUpdatesMenu = "&Check for updates";
        string m_DownloadUpdateMenu = "&Download and Update now";
        string m_InstallUpdateMenu = "&Install update now";
        string m_CancelUpdatingMenu = "&Cancel updating";
        string m_CancelCheckingMenu = "&Cancel update checking";
#endif

        string m_HideMenu = "Hide";
        string m_ViewChangesMenu = "View changes in version %version%";

        string m_PrematureExitTitle = C_PrematureExitTitle;
        string m_PrematureExitMessage = C_PrematureExitMessage;

        string m_StopChecking = "Stop checking for updates for now";
        string m_StopDownloading = "Stop downloading update for now";
        string m_StopExtracting = "Stop extracting update for now";

        string m_TryAgainLater = "Try again later";
        string m_TryAgainNow = "Try again now";
        string m_ViewError = "View error details";

        string m_CloseButton = "Close";
        string m_ErrorTitle = "Error";

        string m_UpdateNowButton = "Update now";
        string m_ChangesInVersion = "Changes in version %version%";

        string m_FailedToCheck = "Failed to check for updates.";
        string m_FailedToDownload = "Failed to download the update.";
        string m_FailedToExtract = "Failed to extract the update.";

        string m_Checking = "Checking for updates";
        string m_Downloading = "Downloading update";
        string m_Extracting = "Extracting update";
        string m_UpdateAvailable = "Update is ready to be installed.";
        string m_InstallOnNextStart = "Update will be installed on next start.";

        string m_AlreadyUpToDate = "You already have the latest version";
        string m_SuccessfullyUpdated = "Successfully updated to %version%";
        string m_UpdateFailed = "Update failed to install.";

        public string CheckForUpdatesMenu
        {
            get { return m_CheckForUpdatesMenu; }
            set { m_CheckForUpdatesMenu = value; }
        }

        bool ShouldSerializeCheckForUpdatesMenu() { return false; }

        public string FailedToCheck
        {
            get { return m_FailedToCheck; }
            set { m_FailedToCheck = value; }
        }

        bool ShouldSerializeFailedToCheck() { return false; }

        public string FailedToDownload
        {
            get { return m_FailedToDownload; }
            set { m_FailedToDownload = value; }
        }

        bool ShouldSerializeFailedToDownload() { return false; }

        public string FailedToExtract
        {
            get { return m_FailedToExtract; }
            set { m_FailedToExtract = value; }
        }

        bool ShouldSerializeFailedToExtract() { return false; }

        public string Checking
        {
            get { return m_Checking; }
            set { m_Checking = value; }
        }

        bool ShouldSerializeChecking() { return false; }

        public string Downloading
        {
            get { return m_Downloading; }
            set { m_Downloading = value; }
        }

        bool ShouldSerializeDownloading() { return false; }

        public string Extracting
        {
            get { return m_Extracting; }
            set { m_Extracting = value; }
        }

        bool ShouldSerializeExtracting() { return false; }

        public string UpdateAvailable
        {
            get { return m_UpdateAvailable; }
            set { m_UpdateAvailable = value; }
        }

        bool ShouldSerializeUpdateAvailable() { return false; }

        public string InstallOnNextStart
        {
            get { return m_InstallOnNextStart; }
            set { m_InstallOnNextStart = value; }
        }

        bool ShouldSerializeInstallOnNextStart() { return false; }

        public string SuccessfullyUpdated
        {
            get { return m_SuccessfullyUpdated; }
            set { m_SuccessfullyUpdated = value; }
        }

        bool ShouldSerializeSuccessfullyUpdated() { return false; }

        public string UpdateFailed
        {
            get { return m_UpdateFailed; }
            set { m_UpdateFailed = value; }
        }

        bool ShouldSerializeUpdateFailed() { return false; }

        public string AlreadyUpToDate
        {
            get { return m_AlreadyUpToDate; }
            set { m_AlreadyUpToDate = value; }
        }

        bool ShouldSerializeAlreadyUpToDate() { return false; }

        public string DownloadUpdateMenu
        {
            get { return m_DownloadUpdateMenu; }
            set { m_DownloadUpdateMenu = value; }
        }

        bool ShouldSerializeDownloadUpdateMenu() { return false; }

        public string InstallUpdateMenu
        {
            get { return m_InstallUpdateMenu; }
            set { m_InstallUpdateMenu = value; }
        }

        bool ShouldSerializeInstallUpdateMenu() { return false; }

        public string CancelUpdatingMenu
        {
            get { return m_CancelUpdatingMenu; }
            set { m_CancelUpdatingMenu = value; }
        }

        bool ShouldSerializeCancelUpdatingMenu() { return false; }

        public string CancelCheckingMenu
        {
            get { return m_CancelCheckingMenu; }
            set { m_CancelCheckingMenu = value; }
        }

        bool ShouldSerializeCancelCheckingMenu() { return false; }

        public string HideMenu
        {
            get { return m_HideMenu; }
            set { m_HideMenu = value; }
        }

        bool ShouldSerializeHideMenu() { return false; }

        public string ViewChangesMenu
        {
            get { return m_ViewChangesMenu; }
            set { m_ViewChangesMenu = value; }
        }

        bool ShouldSerializeViewChangesMenu() { return false; }

        public string PrematureExitTitle
        {
            get { return m_PrematureExitTitle; }
            set { m_PrematureExitTitle = value; }
        }

        bool ShouldSerializePrematureExitTitle() { return false; }

        public string PrematureExitMessage
        {
            get { return m_PrematureExitMessage; }
            set { m_PrematureExitMessage = value; }
        }

        bool ShouldSerializePrematureExitMessage() { return false; }

        public string StopChecking
        {
            get { return m_StopChecking; }
            set { m_StopChecking = value; }
        }

        bool ShouldSerializeStopChecking() { return false; }

        public string StopDownloading
        {
            get { return m_StopDownloading; }
            set { m_StopDownloading = value; }
        }

        bool ShouldSerializeStopDownloading() { return false; }

        public string StopExtracting
        {
            get { return m_StopExtracting; }
            set { m_StopExtracting = value; }
        }

        bool ShouldSerializeStopExtracting() { return false; }

        public string TryAgainLater
        {
            get { return m_TryAgainLater; }
            set { m_TryAgainLater = value; }
        }

        bool ShouldSerializeTryAgainLater() { return false; }

        public string TryAgainNow
        {
            get { return m_TryAgainNow; }
            set { m_TryAgainNow = value; }
        }

        bool ShouldSerializeTryAgainNow() { return false; }

        public string ViewError
        {
            get { return m_ViewError; }
            set { m_ViewError = value; }
        }

        bool ShouldSerializeViewError() { return false; }

        public string CloseButton
        {
            get { return m_CloseButton; }
            set { m_CloseButton = value; }
        }

        bool ShouldSerializeCloseButton() { return false; }

        public string ErrorTitle
        {
            get { return m_ErrorTitle; }
            set { m_ErrorTitle = value; }
        }

        bool ShouldSerializeErrorTitle() { return false; }

        public string UpdateNowButton
        {
            get { return m_UpdateNowButton; }
            set { m_UpdateNowButton = value; }
        }

        bool ShouldSerializeUpdateNowButton() { return false; }

        public string ChangesInVersion
        {
            get { return m_ChangesInVersion; }
            set { m_ChangesInVersion = value; }
        }

        bool ShouldSerializeChangesInVersion() { return false; }
    }
}