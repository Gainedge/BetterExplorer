using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using BExplorer.Shell.Interop;

namespace BetterExplorer {
	public partial class UpdateWizard : Form {
		private bool Resizing = false;
		private Updater CurrentUpdater { get; set; }
		private List<String> UpdateLocalPaths { get; set; }
		public UpdateWizard(Updater updater) {
			this.UpdateLocalPaths = new List<string>();
			this.StartPosition = FormStartPosition.CenterParent;
			this.CurrentUpdater = updater;
			InitializeComponent();
			UxTheme.SetWindowTheme(this.lvAvailableUpdates.Handle, "Explorer", 0);
			foreach (var item in this.CurrentUpdater.AvailableUpdates) {
				ListViewItem lvi = new ListViewItem(item.Name);
				switch (item.Type) {
					case UpdateTypes.Nightly:
						lvi.ForeColor = Color.Red;
						break;
					case UpdateTypes.Alpha:
						lvi.ForeColor = Color.DarkOrange;
						break;
					case UpdateTypes.Beta:
						lvi.ForeColor = Color.Blue;
						break;
					case UpdateTypes.ReleaseCandidate:
						lvi.ForeColor = Color.Brown;
						break;
					case UpdateTypes.Release:
						lvi.ForeColor = Color.Black;
						break;
				}
				lvi.SubItems.Add(item.Version);
				lvi.SubItems.Add(item.Type.ToString());
				lvi.SubItems.Add(item.RequiredVersion);
				lvi.SubItems.Add(item.UpdaterFilePath);
				lvi.SubItems.Add(item.UpdaterFilePath64);
				this.lvAvailableUpdates.Items.Add(lvi);
			}
		}

		private void lvAvailableUpdates_SizeChanged(object sender, EventArgs e) {
			// Don't allow overlapping of SizeChanged calls
			if (!Resizing) {
				// Set the resizing flag
				Resizing = true;

				if (this.lvAvailableUpdates != null) {
					int allColumnSize = 0;
					for (int i = 1; i < this.lvAvailableUpdates.Columns.Count; i++) {
						allColumnSize += this.lvAvailableUpdates.Columns[i].Width;
					}
					this.lvAvailableUpdates.Columns[0].Width = this.lvAvailableUpdates.ClientRectangle.Width - allColumnSize;
				}
			}

			// Clear the resizing flag
			Resizing = false;
		}

		private void wizardControl1_SelectedPageChanged(object sender, EventArgs e) {
			//if (wizardControl1.SelectedPage == pgAvailableUpdates)

			if (wizardControl1.SelectedPage == pgDownload) {
				pgDownload.AllowNext = false;
				CurrentUpdater.UpdaterDownloadComplete += new Updater.PathEventHandler(CurrentUpdater_UpdaterDownloadComplete);
				CurrentUpdater.UpdaterDownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(CurrentUpdater_UpdaterDownloadProgressChanged);
				pbTotalProgress.Maximum = this.lvAvailableUpdates.CheckedItems.Count;
				lblTotalProgress.Text = String.Format("{0}/{1} updates downloaded.", 0, pbTotalProgress.Maximum);
				this.UpdateLocalPaths.Clear();

				for (int i = this.lvAvailableUpdates.CheckedItems.Count - 1; i >= 0; i--) {
					pbFileDownload.Value = 0;
					var item = this.lvAvailableUpdates.CheckedItems[i];
					lblCurrentDownload.Text = String.Format("Downloading {0}", item.SubItems[0].Text);
					CurrentUpdater.DownloadUpdater(item.SubItems[4].Text, Path.GetFileName(item.SubItems[4].Text));
					this.UpdateLocalPaths.Add(Path.Combine(CurrentUpdater.LocalUpdaterLocation, Path.GetFileName(item.SubItems[4].Text)));
					if (Kernel32.IsThis64bitProcess())
						this.UpdateLocalPaths.Add(Path.Combine(CurrentUpdater.LocalUpdaterLocation, Path.GetFileName(item.SubItems[5].Text)));
				}
			}

			if (wizardControl1.SelectedPage.IsFinishPage) {
				wizardControl1.FinishButtonText = "Install";
				wizardControl1.NextButtonShieldEnabled = true;
			}
			else {
				wizardControl1.NextButtonShieldEnabled = false;
			}
		}

		void CurrentUpdater_UpdaterDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) {
			//throw new NotImplementedException();
			pbFileDownload.Value = e.ProgressPercentage;
			lblDownloadedBytes.Text = String.Format("{0}/{1} bytes downloaded.", e.BytesReceived, e.TotalBytesToReceive);
		}

		void CurrentUpdater_UpdaterDownloadComplete(object sender, Tuple<string> e) {
			//pbFileDownload.Value = 0;
			//lblDownloadedBytes.Text = "0000 / 0000 bytes downloaded";
			pbTotalProgress.Value += 1;
			lblTotalProgress.Text = String.Format("{0}/{1} updates downloaded.", pbTotalProgress.Value, pbTotalProgress.Maximum);
			//if (pbTotalProgress.Value == pbTotalProgress.Maximum) ;
			pgDownload.AllowNext = true;
		}

		private void wizardControl1_Finished(object sender, EventArgs e) {
			string ExePath = Utilities.AppDirectoryItem("Updater.exe");

			string filesForExtract = String.Empty;
			foreach (string item in this.UpdateLocalPaths) {
				filesForExtract += String.Format("{0};", item);
			}
			using (Process proc = new Process()) {
				var psi = new ProcessStartInfo {
					FileName = ExePath,
					Verb = "runas",
					UseShellExecute = true,
					Arguments = String.Format("/env /user:Administrator \"{0}\" UP:{1}", ExePath, filesForExtract)
				};

				proc.StartInfo = psi;
				proc.Start();

				//if (proc.ExitCode == -1)
				//  System.Windows.MessageBox.Show("Error in Update!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

			}
			System.Windows.Application.Current.Shutdown();
		}

	}
}
