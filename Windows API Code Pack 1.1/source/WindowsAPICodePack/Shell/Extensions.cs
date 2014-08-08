using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAPICodePack.Shell {
	public static class Extensions {
		public static Rectangle ToRectangle(this BExplorer.Shell.Interop.User32.RECT rect) {
			return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
		}
	}
}
