using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
