using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BetterExplorer {
	static class Utilities {

		static Utilities() {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);
			if (rks == null)
				rks = rk.CreateSubKey(@"Software\BExplorer");
		}

		public static string RemoveExtensionsFromFile(string file, string ext) {
			return file.EndsWith(ext) ? file.Remove(file.LastIndexOf(ext), ext.Length) : file;
		}

		public static string GetExtension(string file) {
			return file.Substring(file.LastIndexOf("."));
		}


		public static void SetRegistryValue(string Name, object Value) {
			using (RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true)) {
				rks.SetValue(Name, Value);
			}
		}

		public static void SetRegistryValue(string Name, object Value, RegistryValueKind Kind) {
			using (RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true)) {
				rks.SetValue(Name, Value, Kind);

			}
		}


		public static Object GetRegistryValue(string Name, string DefaultValue) {
			using (RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true)) {
				return rks.GetValue(Name, DefaultValue);
			}
		}

	}
}
