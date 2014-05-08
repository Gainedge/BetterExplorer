using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Fluent;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Threading;
using Clipboards = System.Windows.Forms.Clipboard;
using MenuItem = Fluent.MenuItem;
using System.Windows.Shell;
using System.Windows.Media.Animation;
using System.Collections.Specialized;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Threading;
using SevenZip;
using ContextMenu = Fluent.ContextMenu;
using System.Globalization;
using NAppUpdate.Framework;
using BetterExplorerControls;
using Microsoft.WindowsAPICodePack.Shell.FileOperations;
using Microsoft.WindowsAPICodePack.Shell.ExplorerBrowser;
using wyDay.Controls;
using System.Security.Principal;
using Shell32;
using Microsoft.WindowsAPICodePack.Taskbar;
using BetterExplorer.Networks;
using System.Xml.Linq;
using System.Text;
using BetterExplorer.UsbEject;
using LTR.IO;
using LTR.IO.ImDisk;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using WindowsHelper;



namespace BetterExplorer {

	//TODO: Make all updating private and handled in [Updater]
	partial class MainWindow {
		BackgroundWorker UpdaterWorker;
		int UpdateCheckType;

		void CheckForUpdate(bool ShowUpdateUI = true) {
			this.UpdaterWorker = new BackgroundWorker();
			this.UpdaterWorker.WorkerSupportsCancellation = true;
			this.UpdaterWorker.WorkerReportsProgress = true;
			this.UpdaterWorker.DoWork += new DoWorkEventHandler(UpdaterWorker_DoWork);

			if (!this.UpdaterWorker.IsBusy)
				this.UpdaterWorker.RunWorkerAsync(ShowUpdateUI);
			else if (ShowUpdateUI)
				MessageBox.Show("Update in progress! Please wait!");

			// var informalVersion = (Assembly.GetExecutingAssembly().GetCustomAttributes(
			//typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault() as AssemblyInformationalVersionAttribute).InformationalVersion;
		}


		void UpdaterWorker_DoWork(object sender, DoWorkEventArgs e) {
			Updater updater = new Updater("http://update.better-explorer.com/update.xml", 5, UpdateCheckType == 1);
			if (updater.LoadUpdateFile()) {
				if ((bool)e.Argument) {
					Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
						(Action)(() => {
							UpdateWizard updateWizzard = new UpdateWizard(updater);
							updateWizzard.ShowDialog(this.GetWin32Window());
						}));
				}
				else {
					Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
						(Action)(() => {
							stiUpdate.Content = FindResource("stUpdateAvailableCP").ToString().Replace("VER", updater.AvailableUpdates[0].Version);
							stiUpdate.Foreground = System.Windows.Media.Brushes.Red;
						}));
				}
			}
			else {
				Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
						(Action)(() => {
							stiUpdate.Content = FindResource("stUpdateNotAvailableCP").ToString();
							stiUpdate.Foreground = System.Windows.Media.Brushes.Black;
							if ((bool)e.Argument)
								MessageBox.Show(FindResource("stUpdateNotAvailableCP").ToString());
						}));
			}

			Utilities.SetRegistryValue("LastUpdateCheck", DateTime.Now.ToBinary(), RegistryValueKind.QWord);
			LastUpdateCheck = DateTime.Now;
		}

		void updateCheckTimer_Tick(object sender, EventArgs e) {
			if (DateTime.Now.Subtract(LastUpdateCheck).Days >= UpdateCheckInterval) {
				CheckForUpdate(false);
			}
		}

	}
}
