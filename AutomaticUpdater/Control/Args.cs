using System;

namespace wyDay.Controls
{
    /// <summary>Reports the progress in the current update step.</summary>
    /// <param name="sender">Sender</param>
    /// <param name="progress">Progress</param>
    public delegate void UpdateProgressChanged(object sender, int progress);

    /// <summary>Represents the method that will handle an event that is cancelable.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A BeforeArgs that allows you to canel the step.</param>
    public delegate void BeforeHandler(object sender, BeforeArgs e);

    /// <summary>Represents the method that will handle a failure event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A FailArgs that contains the failure info.</param>
    public delegate void FailHandler(object sender, FailArgs e);

    /// <summary>Represents the method that will handle a success event.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A SuccessArgs that contains the success info.</param>
    public delegate void SuccessHandler(object sender, SuccessArgs e);

    /// <summary>
    /// Event data for a successful update step.
    /// </summary>
    public class SuccessArgs : EventArgs
    {
        /// <summary>
        /// The new version of your application.
        /// </summary>
        public string Version { get; set; }
    }

    /// <summary>
    /// Event data for a failed step.
    /// </summary>
    public class FailArgs : EventArgs
    {
        /// <summary>
        /// The brief description of the error.
        /// </summary>
        public string ErrorTitle { get; set; }

        /// <summary>
        /// A more detailed description of the error.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// wyUpdate closed before it should've.
        /// </summary>
        public bool wyUpdatePrematureExit { get; set; }
    }

    /// <summary>
    /// Event data for before an update step is processed.
    /// </summary>
    public class BeforeArgs : EventArgs
    {
        /// <summary>
        /// Prevent the current step from happening.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
