using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
//using Windows.Data.Xml.Dom;
//using Windows.Foundation.Collections;
//using Windows.UI.Notifications;
using BExplorer.Shell._Plugin_Interfaces;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BetterExplorer {
  partial class MainWindow {
    private BackgroundWorker UpdaterWorker;
    private Boolean _IsCheckUpdateFromTimer = false;
    private Boolean _IsUpdateNotificationMessageBoxShown = false;

    /// <summary>
    /// Updates the application if a new version is read
    /// </summary>
    /// <param name="ShowUpdateUI">Will you show a <see cref="MessageBox">MessageBox</see> when it is already updating?</param>
    private void CheckForUpdate(bool ShowUpdateUI = true) {
      this.UpdaterWorker = new BackgroundWorker();
      this.UpdaterWorker.WorkerSupportsCancellation = true;
      this.UpdaterWorker.WorkerReportsProgress = true;
      this.UpdaterWorker.DoWork += new DoWorkEventHandler(UpdaterWorker_DoWork);

      if (!this.UpdaterWorker.IsBusy)
        this.UpdaterWorker.RunWorkerAsync();
      else if (ShowUpdateUI)
        MessageBox.Show("Update in progress! Please wait!");
    }

    /// <summary>
    /// Updates the application on a separate thread if and only if it is needed then updates <see cref="LastUpdateCheck"/> to <see cref="DateTime.Now"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UpdaterWorker_DoWork(object sender, DoWorkEventArgs e) {
      this._IsCheckUpdateFromTimer = true;
      Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
        (Action)(() => {
          //autoUpdater.ForceCheckForUpdate(true);
        }));

      Settings.BESettings.LastUpdateCheck = DateTime.Now;
    }

    /// <summary>
    /// If and only if the application's last update check is more then the interval (<see cref="UpdateCheckInterval"/>) then it will update using <see cref="CheckForUpdate"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void updateCheckTimer_Tick(object sender, EventArgs e) {
      var updaterThread = new Thread(((App)Application.Current).RunAutomaticUpdateChecker);
      updaterThread.Start();
      //if (DateTime.Now.Subtract(Settings.BESettings.LastUpdateCheck).Days >= Settings.BESettings.UpdateCheckInterval) {
      //  CheckForUpdate(false);
      //}
    }

    private void MainWindow_OnClosed(Object sender, EventArgs e) {
      
    }
    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("user32.dll", SetLastError = true)]
    private static extern Boolean PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    private void ButtonBase_OnClick(Object sender, RoutedEventArgs e) {

      new ToastContentBuilder()
        .AddArgument("action", "viewConversation")
        .AddArgument("conversationId", 9813)
        .AddText("Andrew sent you a picture")
        .AddText("Check this out, The Enchantments in Washington!")
        .AddButton(new ToastButton()
          .SetContent("Test")
          .AddArgument("action", "reply")
          .SetBackgroundActivation())
        .AddInlineImage(new Uri(@"I:\Picture Library\fKCzjJq.jpg"))
        .AddHeroImage(new Uri(@"I:\Picture Library\nfs-prostreet-hot-girls.jpg"))
        .AddAttributionText("Via BE")
        .AddAppLogoOverride(new Uri(@"I:\Picture Library\Darth-Vader-Side-Profile.jpg"), ToastGenericAppLogoCrop.Circle)
        .Show();

    }
  }
}