namespace wyDay.Controls
{
    /// <summary>
    /// Specifies the update type that the AutomaticUpdater uses.
    /// </summary>
    public enum UpdateType 
    {
        /// <summary>
        /// The AutomaticUpdater will check, download, and install the update completely silently.
        /// </summary>
        Automatic = 0, 

        /// <summary>
        /// The AutomaticUpdater will check for and download updatest automatically, then notify the user.
        /// </summary>
        CheckAndDownload = 1,

        /// <summary>
        /// The AutomaticUpdater will only check for updates and notify the user if updates are found.
        /// </summary>
        OnlyCheck = 2,

        /// <summary>
        /// The AutomaticUpdater will do nothing. All checking must be done manually.
        /// </summary>
        DoNothing = 3
    }
}