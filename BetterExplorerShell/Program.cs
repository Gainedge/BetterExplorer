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
      //Application.EnableVisualStyles();
      //Application.SetCompatibleTextRenderingDefault(false);
      //Application.Run(new Form1());
      if (!ExecuteControlPanelItem(args[0])) {
        String bexplorerPath = Path.GetDirectoryName(Application.ExecutablePath);
        bexplorerPath += @"\BetterExplorer.exe";

        System.Diagnostics.ProcessStartInfo procStartInfo =
            new System.Diagnostics.ProcessStartInfo(bexplorerPath, args[0]);

        // Now we create a process, assign its ProcessStartInfo and start it
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo = procStartInfo;


        proc.Start();
      }
    }

    static public bool ExecuteControlPanelItem(String cmd) {

      // Discard control panel items
      String cpName = @"::{26EE0668-A00A-44D7-9371-BEB064C98683}";
      int cpIndex = cmd.IndexOf(cpName);
      if (cpIndex != 0) return false;
      if (cmd.IndexOf(@"\::", cpIndex + cpName.Length) <= 0 && cmd != cpName) return false;

      String explorerPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
      explorerPath += @"\Explorer.exe";

      System.Diagnostics.ProcessStartInfo procStartInfo =
          new System.Diagnostics.ProcessStartInfo(explorerPath, cmd);

      // Now we create a process, assign its ProcessStartInfo and start it
      System.Diagnostics.Process proc = new System.Diagnostics.Process();
      proc.StartInfo = procStartInfo;


      proc.Start();
      return true;

    }
  }
}
