using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace BetterExplorerShell {
	static class Program {

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			if (!ExecuteControlPanelItem(args[0])) {
				var bexplorerPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\BetterExplorer.exe";
				var procStartInfo = new System.Diagnostics.ProcessStartInfo(bexplorerPath, String.Format("\"{0}\"", args[0]));

				// Now we create a process, assign its ProcessStartInfo and start it
				var proc = new System.Diagnostics.Process() { StartInfo = procStartInfo };
				proc.Start();
			}
		}

		static public bool ExecuteControlPanelItem(String cmd) {
			// Discard control panel items
			const String cpName = @"::{26EE0668-A00A-44D7-9371-BEB064C98683}";

			int cpIndex = cmd.IndexOf(cpName);
			if (cpIndex != 0) return false;



			//if (cmd.IndexOf(@"\::", cpIndex + cpName.Length) <= 0 && cmd != cpName) return false;
			if (!cmd.StartsWith(cpName)) return false;

			String explorerPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			explorerPath += @"\Explorer.exe";

			cmd = cmd.Replace("Fonts", "::{BD84B380-8CA2-1069-AB1D-08000948F534}");

			var procStartInfo = new System.Diagnostics.ProcessStartInfo(explorerPath, String.Format("shell:{0}", cmd));

			// Now we create a process, assign its ProcessStartInfo and start it
			var proc = new System.Diagnostics.Process() { StartInfo = procStartInfo };
			proc.Start();
			return true;
		}
	}
}
