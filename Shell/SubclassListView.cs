using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BExplorer.Shell
{
	public class SubclassListView : NativeWindow
	{
		public IntPtr lvhandle;
		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == (int)WM.WM_PAINT || msg.Msg == (int)WM.WM_CREATE || msg.Msg == (int)WM.WM_LBUTTONUP || msg.Msg == (int)WM.WM_LBUTTONDOWN || msg.Msg == (int)WM.WM_KEYDOWN || msg.Msg == (int)WM.WM_KEYUP)
			{
				User32.SendMessage(lvhandle, 296, User32.MAKELONG(1, 1), 0);
				
			}
			base.WndProc(ref msg);
		}

	}
}
