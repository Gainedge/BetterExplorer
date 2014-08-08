using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Taskbar {
	public static class TaskBar {
		public static void SetCurrentProcessAppId(string appId) {
			Shell32.SetCurrentProcessExplicitAppUserModelID(appId);
		}
	}
}
