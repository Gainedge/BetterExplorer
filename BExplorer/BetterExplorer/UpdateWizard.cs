using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BetterExplorer
{
		public partial class UpdateWizard : Form
		{
			private bool Resizing = false;
			private Updater CurrentUpdater { get; set; }
			public UpdateWizard(Updater updater) {
				this.StartPosition = FormStartPosition.CenterParent;
				this.CurrentUpdater = updater;
				InitializeComponent();
				WindowsHelper.WindowsAPI.SetWindowTheme(this.lvAvailableUpdates.Handle, "Explorer", 0);
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
				if (wizardControl1.SelectedPage == pgDownload) {
					wizardControl1.NextButtonShieldEnabled = true;
					pgDownload.AllowNext = false;
					wizardControl1.NextButtonText = "Install";
					CurrentUpdater.UpdaterDownloadComplete += new Updater.PathEventHandler(CurrentUpdater_UpdaterDownloadComplete);
					CurrentUpdater.UpdaterDownloadProgressChanged += new System.Net.DownloadProgressChangedEventHandler(CurrentUpdater_UpdaterDownloadProgressChanged);
					pbTotalProgress.Maximum = this.lvAvailableUpdates.CheckedItems.Count;
					lblTotalProgress.Text = String.Format("{0}/{1} updates downloaded.", 0, pbTotalProgress.Maximum);
					for (int i = this.lvAvailableUpdates.CheckedItems.Count - 1; i >= 0; i--) {
						pbFileDownload.Value = 0;
						var item = this.lvAvailableUpdates.CheckedItems[i];
						lblCurrentDownload.Text = String.Format("Downloading {0}", item.SubItems[0].Text);
						CurrentUpdater.DownloadUpdater(item.SubItems[4].Text, Path.GetFileName(item.SubItems[4].Text));
					}
				} else {
					wizardControl1.NextButtonShieldEnabled = false;
					wizardControl1.NextButtonText = "Next";
				}
			}

			void CurrentUpdater_UpdaterDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e) {
				//throw new NotImplementedException();
				pbFileDownload.Value = e.ProgressPercentage;
				lblDownloadedBytes.Text = String.Format("{0}/{1} bytes downloaded.", e.BytesReceived, e.TotalBytesToReceive);
			}

			void CurrentUpdater_UpdaterDownloadComplete(object sender, Updater.PathEventArgs e) {
				//pbFileDownload.Value = 0;
				//lblDownloadedBytes.Text = "0000 / 0000 bytes downloaded";
				pbTotalProgress.Value += 1;
				lblTotalProgress.Text = String.Format("{0}/{1} updates downloaded.", pbTotalProgress.Value, pbTotalProgress.Maximum);
				if (pbTotalProgress.Value == pbTotalProgress.Maximum);
					pgDownload.AllowNext = true;
			}

		}
}
