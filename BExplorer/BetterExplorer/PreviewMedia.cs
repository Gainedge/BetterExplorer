using BExplorer.Shell.Interop;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BetterExplorer {

	public partial class PreviewMedia : Form {

		public PreviewMedia() {
			InitializeComponent();
		}

		private Image img;

		public void SetImage(string imagepath) {
			img = Image.FromFile(imagepath);
			pictureBox1.Image = img;
		}

		public void LoadPreview(Point pos) {
			int Xpos;
			int Ypos;
			if (Screen.PrimaryScreen.WorkingArea.Width - pos.X < this.Width + 10) {
				Xpos = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10;
			}
			else {
				Xpos = pos.X;
			}

			if (Screen.PrimaryScreen.WorkingArea.Height - pos.Y < this.Height + 10) {
				Ypos = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 10;
			}
			else {
				Ypos = pos.Y;
			}
			try {
				User32.ShowWindow(this.Handle, (User32.ShowWindowCommands)4);
				User32.SetWindowPos(this.Handle, (IntPtr)(-1), Xpos + 2, Ypos + 2, 0, 0, User32.SetWindowPosFlags.IgnoreResize | User32.SetWindowPosFlags.DoNotActivate);
			}
			catch {
			}
		}

		public void MovePreview(Point NewPos) {
			int Xpos;
			int Ypos;
			if (Screen.PrimaryScreen.WorkingArea.Width - NewPos.X < this.Width + 10) {
				Xpos = Screen.PrimaryScreen.WorkingArea.Width - this.Width - 10;
			}
			else {
				Xpos = NewPos.X;
			}

			if (Screen.PrimaryScreen.WorkingArea.Height - NewPos.Y < this.Height + 10) {
				Ypos = Screen.PrimaryScreen.WorkingArea.Height - this.Height - 10;
			}
			else {
				Ypos = NewPos.Y;
			}

			try {
				User32.SetWindowPos(this.Handle, (IntPtr)(-1), Xpos + 2, Ypos + 2, 0, 0,
						User32.SetWindowPosFlags.IgnoreResize | User32.SetWindowPosFlags.DoNotActivate);
			}
			catch {
			}
		}

		protected override CreateParams CreateParams {
			get {
				CreateParams baseParams = base.CreateParams;

				baseParams.ExStyle |= (int)(
					User32.WindowStylesEx.WS_EX_NOACTIVATE |
				  User32.WindowStylesEx.WS_EX_TOOLWINDOW);

				return baseParams;
			}
		}

		public void Disposeimg() {
			if (img != null)
				img.Dispose();
		}
	}
}