using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GongSolutions.Shell
{
	public class TreeViewBase : TreeView
	{
		public TreeViewBase()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.EnableNotifyMessage, true);
		}

		//protected override CreateParams CreateParams
		//{
		//	get
		//	{
		//		CreateParams cp = base.CreateParams;
		//		cp.ExStyle |= 0x02000000;
		//		return cp;
		//	}
		//}
		private const int WM_ERASEBKGND = 0x0014;
		private const int TV_FIRST = 0x1100;
		private const int TVM_SETBKCOLOR = TV_FIRST + 29;
		private const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
		private const int TVS_EX_DOUBLEBUFFER = 0x0004;
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg,
				IntPtr wParam, IntPtr lParam);
		private void UpdateExtendedStyles()
		{
			int Style = 0;

			Style |= TVS_EX_DOUBLEBUFFER;

			if (Style != 0)
				SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)Style);
		}
		//protected override void WndProc(ref Message m)
		//{
		//	if (m.Msg == WM_ERASEBKGND)
		//	{
		//		m.Result = IntPtr.Zero;
		//		return;
		//	}
		//	base.WndProc(ref m);
		//}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			UpdateExtendedStyles();

		}
		protected override void OnNotifyMessage(Message m)
		{
			//Filter out the WM_ERASEBKGND message
			if (m.Msg != WM_ERASEBKGND)
			{
				base.OnNotifyMessage(m);
			}
		}

		private const int WM_PRINTCLIENT = 0x0318;
		private const int PRF_CLIENT = 4;
		protected override void OnPaint(PaintEventArgs e)
		{
			if (GetStyle(ControlStyles.UserPaint))
			{
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
	}
}
