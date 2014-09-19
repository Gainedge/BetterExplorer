using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BExplorer.Shell {
	public class TreeViewBase : TreeView {
		#region Event Handlers
		public event EventHandler VerticalScroll;
		#endregion

		#region Public Members
		public const int TVS_EX_AUTOHSCROLL = 0x8000;
		public const int TVS_EX_FADEINOUTEXPANDOS = 0x0040;
		#endregion

		#region Private Members
		private const int PRF_CLIENT = 4;

		private const int WM_PRINTCLIENT = 0x0318;
		private const int TVS_EX_DOUBLEBUFFER = 0x0004;
		private const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
		private const int TVM_SETBKCOLOR = TV_FIRST + 29;

		private const int TV_FIRST = 0x1100;
		private const int WM_ERASEBKGND = 0x0014;
		#endregion

		#region Initializer
		public TreeViewBase() {
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.EnableNotifyMessage, true);
		}
		#endregion

		#region Overrides
		protected override void OnPaint(PaintEventArgs e) {
			if (GetStyle(ControlStyles.UserPaint)) {
				Message m = new Message();
				m.HWnd = Handle;
				m.Msg = WM_PRINTCLIENT;
				m.WParam = e.Graphics.GetHdc();
				m.LParam = (IntPtr)PRF_CLIENT;
				DefWndProc(ref m);
				e.Graphics.ReleaseHdc(m.WParam);
			}
			base.OnPaint(e);
		}
		protected override void OnNotifyMessage(Message m) {
			//Filter out the WM_ERASEBKGND message
			if (m.Msg != WM_ERASEBKGND) {
				base.OnNotifyMessage(m);
			}
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			SetDoubleBuffer();
			SetExpandoesStyle();
			SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_AUTOHSCROLL, (IntPtr)TVS_EX_AUTOHSCROLL);
			UxTheme.SetWindowTheme(this.Handle, "Explorer", 0);

		}
		protected override void WndProc(ref Message m) {
			if (m.Msg == WM_ERASEBKGND) {
				m.Result = IntPtr.Zero;
				return;
			}
			if (m.Msg == 0x0115) {
				if (VerticalScroll != null) {
					VerticalScroll.Invoke(this, EventArgs.Empty);
				}
			}

			base.WndProc(ref m);
		}
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.Style |= 0x8000;
				return cp;
			}
		}
		#endregion

		#region Unmanaged
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
		#endregion

		#region Private Methods

		private void SetExpandoesStyle() {
			int Style = 0;

			Style |= TVS_EX_FADEINOUTEXPANDOS;

			if (Style != 0)
				SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_FADEINOUTEXPANDOS, (IntPtr)Style);
		}
		private void SetDoubleBuffer() {
			int Style = 0;

			Style |= TVS_EX_DOUBLEBUFFER;

			if (Style != 0)
				SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)Style);
		}
		#endregion
	}
}
