using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell.Interop;
using MS.WindowsAPICodePack.Internal;

namespace Microsoft.WindowsAPICodePack.Shell {
	public static class CoreErrorHelper_Help {

		public static bool Succeeded(MS.WindowsAPICodePack.Internal.HResult result) {
			return CoreErrorHelper.Succeeded(result);
		}

		public static bool Succeeded(int result) {
			return CoreErrorHelper.Succeeded(result);
		}

		public static bool Matches(int result, int win32ErrorCode) {
			return CoreErrorHelper.Matches(result, win32ErrorCode);
		}
	}
}
