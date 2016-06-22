using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell {
	public class ListViewEditor : NativeWindow {

		public ListViewEditor(IntPtr hwnd) {
			this.AssignHandle(hwnd);
		}

		internal void OnHandleDestroyed(object sender, EventArgs e) {
			// Window was destroyed, release hook.
			ReleaseHandle();
		}

		protected override void WndProc(ref Message m) {
			switch (m.Msg) {
				case (int)WM.WM_WINDOWPOSCHANGING:
					User32.WINDOWPOS wp = (User32.WINDOWPOS)System.Runtime.InteropServices.Marshal.PtrToStructure(m.LParam, typeof(User32.WINDOWPOS));
					if (true) // ... if somecondition ...
					{
						// modify the location with x,y:
						//wp.x = 0;
						wp.y = wp.y - 15;
					}
					System.Runtime.InteropServices.Marshal.StructureToPtr(wp, m.LParam, true);
					break;
			}
			base.WndProc(ref m);
		}
	}
}
