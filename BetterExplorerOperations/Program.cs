using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BetterExplorerOperations
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] arguments)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 mainform = new Form1();
            Application.Run(mainform);
            return mainform.errorCode;
        }
    }
}
