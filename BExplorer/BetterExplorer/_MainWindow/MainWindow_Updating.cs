using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace BetterExplorer {
	partial class MainWindow {
		private BackgroundWorker UpdaterWorker;
		private int UpdateCheckType;
		private Boolean _IsCheckUpdateFromTimer = false;
		private Boolean _IsUpdateNotificationMessageBoxShown = false;
		private void CheckForUpdate(bool ShowUpdateUI = true) {
			this.UpdaterWorker = new BackgroundWorker();
			this.UpdaterWorker.WorkerSupportsCancellation = true;
			this.UpdaterWorker.WorkerReportsProgress = true;
			this.UpdaterWorker.DoWork += new DoWorkEventHandler(UpdaterWorker_DoWork);

			if (!this.UpdaterWorker.IsBusy)
				this.UpdaterWorker.RunWorkerAsync(ShowUpdateUI);
			else if (ShowUpdateUI)
				MessageBox.Show("Update in progress! Please wait!");
		}

		private void UpdaterWorker_DoWork(object sender, DoWorkEventArgs e) {
			this._IsCheckUpdateFromTimer = true;
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
			  (Action)(() => {
				  autoUpdater.ForceCheckForUpdate(true);
			  }));
			Utilities.SetRegistryValue("LastUpdateCheck", DateTime.Now.ToBinary(), RegistryValueKind.QWord);
			LastUpdateCheck = DateTime.Now;
		}

		private void updateCheckTimer_Tick(object sender, EventArgs e) {
			if (DateTime.Now.Subtract(LastUpdateCheck).Days >= UpdateCheckInterval) {
				CheckForUpdate(false);
			}
		}
	}
}