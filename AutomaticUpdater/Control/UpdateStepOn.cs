namespace wyDay.Controls
{
    /// <summary>
    /// Represents the update steps that the AutomaticUpdater goes through.
    /// </summary>
    public enum UpdateStepOn
    {
        /// <summary>
        /// The AutomaticUpdater isn't doing anything.
        /// </summary>
        Nothing = 0,

        /// <summary>
        /// The AutomaticUpdater is checking for updates.
        /// </summary>
        Checking = 1,

        /// <summary>
        /// Updates are available and ready to be downloaded and installed.
        /// </summary>
        UpdateAvailable = 2,

        /// <summary>
        /// The AutomaticUpdater is downloading updates.
        /// </summary>
        DownloadingUpdate = 3,

        /// <summary>
        /// Updates are downloaded and ready to be extracted and installed.
        /// </summary>
        UpdateDownloaded = 4,

        /// <summary>
        /// The AutomaticUpdater is extracting updates.
        /// </summary>
        ExtractingUpdate = 5,

        /// <summary>
        /// Updates have downloaded and extracted and are ready to be installed.
        /// </summary>
        UpdateReadyToInstall = 6
    }
}