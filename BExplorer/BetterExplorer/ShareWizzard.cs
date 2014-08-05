using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Shell;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;
using System.Management;
using System.Management.Instrumentation;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using WindowsHelper;
using System.Threading;

namespace BetterExplorer {
	public partial class ShareWizzard : Form {
		public string CurrentPath = "";
		public ShareWizzard() {
			InitializeComponent();
			wizardControl1.Pages[1].Initialize += new EventHandler<AeroWizard.WizardPageInitEventArgs>(ShareWizzard_Initialize);
			wizardControl1.Pages[0].Commit += new EventHandler<AeroWizard.WizardPageConfirmEventArgs>(ShareWizzard_Commit);
			SelectQuery query = new SelectQuery("Win32_UserAccount");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			cbUsers.Items.Add("<None>");

			foreach (ManagementObject envVar in searcher.Get()) {
				cbUsers.Items.Add(envVar["Name"] + "@" + envVar["Domain"]);
			}

			cbUsers.SelectedIndex = 0;


		}


		public struct AccessRights {
			public bool IsRead;
			public bool IsWrite;
			public bool IsAll;
		}

		public struct UserRights {
			public string User;
			public AccessRights rights;
		}

		void ShareWizzard_Commit(object sender, AeroWizard.WizardPageConfirmEventArgs e) {
			string ExePath = Utilities.AppDirectoryItem("BetterExplorerOperations.exe");
			Process proc = new Process();
			var psi = new ProcessStartInfo {
				FileName = ExePath,
				Verb = "runas",
				UseShellExecute = true,
				Arguments = "/env /user:" + "Administrator " + "\"" + ExePath + "\"",
			};
			proc.StartInfo = psi;
			proc.Start();
			Thread.Sleep(1000);
			int z = WindowsAPI.sendWindowsStringMessage((int)WindowsAPI.getWindowId(null, "BetterExplorerOperations"), 0,
				"Share", CurrentPath, txtShareName.Text, "");

			proc.WaitForExit();

		}

		public static System.Drawing.Image BitmapSourceToBitmap2(BitmapSource srs) {
			var encoder = new PngBitmapEncoder();
			using (var stream = new MemoryStream()) {
				encoder.Frames.Add(BitmapFrame.Create(srs));
				encoder.Save(stream);
				var img = Image.FromStream(stream);
				return img;
			}
		}


		public void SetIcon(ShellObject obj) {
			obj.Thumbnail.CurrentSize = new System.Windows.Size(128, 128);
			pbIcon.Image = BitmapSourceToBitmap2(obj.Thumbnail.BitmapSource);

		}

		void ShareWizzard_Initialize(object sender, AeroWizard.WizardPageInitEventArgs e) {
			wizardControl1.CancelButtonText = "Close";
		}
		VisualStyleRenderer ItemSelectedRenderer = new VisualStyleRenderer("Explorer::ListView", 1, 3);
		VisualStyleRenderer ItemHoverRenderer = new VisualStyleRenderer("Explorer::ListView", 1, 2);

		private void cbUsers_DrawItem(object sender, DrawItemEventArgs e) {
			//e.Graphics.FillRectangle(Brushes.Bisque, e.Bounds);
			if ((e.State & DrawItemState.Focus) != 0) {
				ItemHoverRenderer.DrawBackground(e.Graphics, e.Bounds);
			}
			else if ((e.State & DrawItemState.Selected) != 0) {
				ItemSelectedRenderer.DrawBackground(e.Graphics, e.Bounds);
			}
			else {
				e.DrawBackground();
			}
			if ((e.State & DrawItemState.ComboBoxEdit) != 0) {
				e.Graphics.FillRectangle(Brushes.White, e.Bounds);
			}
			if (e.Index != -1) {
				e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				e.Graphics.DrawString((string)cbUsers.Items[e.Index], cbUsers.Font, Brushes.Black,
										  e.Bounds);
			}

		}

		private void ShareWizzard_Load(object sender, EventArgs e) {
			ShellObject sho = ShellObject.FromParsingName(CurrentPath);
			txtShareName.Text = sho.GetDisplayName(Microsoft.WindowsAPICodePack.Shell.DisplayNameType.Default);
			sho.Dispose();
		}
	}
}
