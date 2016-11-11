using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Settings {
	public static class BESettings {
		public static DateTime LastUpdateCheck { get; set; }
		public static Int32 UpdateCheckInterval { get; set; }
		public static String CurrentTheme { get; set; }
		public static Int32 UpdateCheckType { get; set; }
		public static Boolean IsUpdateCheck { get; set; }
		public static Boolean IsUpdateCheckStartup { get; set; }
		public static Boolean IsConsoleShown { get; set; }
		public static Boolean IsInfoPaneEnabled { get; set; }
		public static Int32 InfoPaneHeight { get; set; }
		public static Boolean IsPreviewPaneEnabled { get; set; }
		public static Int32 PreviewPaneWidth { get; set; }
		public static Boolean IsNavigationPaneEnabled { get; set; }
		public static Boolean IsShowCheckboxes { get; set; }
		public static Boolean IsFileOpExEnabled { get; set; }
		public static Boolean IsCustomFO { get; set; }
		public static Boolean IsRestoreTabs { get; set; }
		public static Boolean IsTraditionalNameGrouping { get; set; }
		public static Boolean CanLogAction { get; set; }
		public static String StartupLocation { get; set; }

		public static void LoadSettings() {
			var rkRoot = Registry.CurrentUser;
			var rksRoot = rkRoot.OpenSubKey(@"Software\BExplorer", true) ?? rkRoot.CreateSubKey(@"Software\BExplorer");
			BESettings.StartupLocation = rksRoot?.GetValue("StartUpLoc", "shell:::{031E4825-7B94-4DC3-B131-E946B44C8DD5}").ToString();
			rksRoot?.Close();
			rkRoot.Close();
			
		}
	}
}
