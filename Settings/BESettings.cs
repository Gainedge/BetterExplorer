using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

//TODO: Upgrade this to use a NuGet package that will save this to a file for us!!
//TODO Consider doing save on property change
//Make sure GetValue and rks.GetValue are ONLY used here and not in the MainWindow

namespace Settings {

	/// <summary>
	/// Contains all settings for Better Explorer (all settings will be moved here eventually)
	/// </summary>
	public static class BESettings {
		/// <summary>When did the application last check for an update</summary>
		public static DateTime LastUpdateCheck { get; set; }
		/// <summary>How often the application should check for an update</summary>
		public static Int32 UpdateCheckInterval { get; set; }
		/// <summary>The saved application theme</summary>
		public static String CurrentTheme { get; set; }
		/// <summary>The version track this is on {0: release, 1: test}</summary>
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
		public static double SearchBarWidth { get; set; }
		public static string Locale { get; set; }
		public static bool EnableActionLog { get; set; }
		public static bool IsGlassOnRibonMinimized { get; set; }

		/// <summary>
		/// Loads all the settings in BESettings from the parent registry Software\BExplorer
		/// </summary>
		public static void LoadSettings() {
			using (var rkRoot = Registry.CurrentUser)
			using (var rksRoot = rkRoot.OpenSubKey(@"Software\BExplorer", true) ?? rkRoot.CreateSubKey(@"Software\BExplorer")) {

				/*
				foreach (var Property in typeof(BESettings).GetProperties()) {
					rksRoot?.GetValue("StartUpLoc", "shell:::{031E4825-7B94-4DC3-B131-E946B44C8DD5}").ToString();
				}
				*/

				StartupLocation = rksRoot?.GetValue("StartUpLoc", "shell:::{031E4825-7B94-4DC3-B131-E946B44C8DD5}").ToString();
				UpdateCheckType = (int)rksRoot.GetValue("UpdateCheckType", 0);
				CurrentTheme = (string)rksRoot.GetValue("CurrentTheme", "Blue");
				IsUpdateCheck = bool.Parse(rksRoot.GetValue("CheckForUpdates", "False").ToString());
				IsUpdateCheckStartup = bool.Parse(rksRoot.GetValue("CheckForUpdatesStartup", "False").ToString());
				UpdateCheckInterval = (int)rksRoot.GetValue("CheckInterval", 7);
				//LastUpdateCheck = DateTime.FromBinary(Convert.ToInt64(rksRoot?.GetValue("LastUpdateCheck", 0)));
				LastUpdateCheck = DateTime.Parse(rksRoot.GetValue("LastUpdateCheck", DateTime.Now.ToString()).ToString());
				IsConsoleShown = bool.Parse(rksRoot.GetValue("IsConsoleShown", "False").ToString());
				EnableActionLog = bool.Parse(rksRoot.GetValue("IsConsoleShown", "False").ToString());
				IsGlassOnRibonMinimized = bool.Parse(rksRoot.GetValue("RibbonMinimizedGlass", "False").ToString());
				IsPreviewPaneEnabled = Boolean.Parse(rksRoot.GetValue("PreviewPaneEnabled", "False").ToString());
				IsInfoPaneEnabled = Boolean.Parse(rksRoot.GetValue("InfoPaneEnabled", "False").ToString());
				IsTraditionalNameGrouping = bool.Parse(rksRoot.GetValue("IsTraditionalNameGrouping", "False").ToString());
				var regLocale = rksRoot.GetValue("Locale", string.Empty).ToString();
				Locale = string.IsNullOrEmpty(regLocale) ? System.Globalization.CultureInfo.CurrentUICulture.Name : regLocale;
				IsRestoreTabs = bool.Parse(rksRoot.GetValue("IsRestoreTabs", "False").ToString());
				InfoPaneHeight = (int)rksRoot.GetValue("InfoPaneHeight", 150);
				IsNavigationPaneEnabled = bool.Parse(rksRoot.GetValue("NavigationPaneEnabled", "False").ToString());
				IsFileOpExEnabled = bool.Parse(rksRoot.GetValue("FileOpExEnabled", "False").ToString());
				IsCustomFO = bool.Parse(rksRoot.GetValue("IsCustomFO", "False").ToString());
				SearchBarWidth = Convert.ToDouble(rksRoot.GetValue("SearchBarWidth", 220));
			}
		}

		/// <summary>
		/// Saves all the settings in BESettings inside the parent registry Software\BExplorer
		/// </summary>
		public static void SaveSettings() {
			using (RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true)) {
				rks.SetValue("StartUpLoc", StartupLocation);
				rks.SetValue("CheckForUpdates", IsUpdateCheck);
				rks.SetValue("CheckInterval", UpdateCheckInterval);
				rks.SetValue("CheckForUpdatesStartup", IsUpdateCheckStartup);
				rks.SetValue("UpdateCheckType", UpdateCheckType);
				rks.SetValue("IsConsoleShown", IsConsoleShown);
				rks.SetValue("EnableActionLog", EnableActionLog);
				rks.SetValue("IsGlassOnRibonMinimized", IsGlassOnRibonMinimized);
				rks.SetValue("LastUpdateCheck", LastUpdateCheck, RegistryValueKind.String);
				rks.SetValue("InfoPaneHeight", InfoPaneHeight, RegistryValueKind.DWord);
				rks.SetValue("CurrentTheme", CurrentTheme);
				rks.SetValue("PreviewPaneEnabled", IsPreviewPaneEnabled);
				rks.SetValue("InfoPaneEnabled", IsInfoPaneEnabled);
				rks.SetValue("IsTraditionalNameGrouping", IsTraditionalNameGrouping);
				rks.SetValue("Locale", Locale);
				rks.SetValue("IsRestoreTabs", IsRestoreTabs);
				rks.SetValue("InfoPaneHeight", InfoPaneHeight);
				rks.SetValue("NavigationPaneEnabled", IsNavigationPaneEnabled);
				rks.SetValue("FileOpExEnabled", IsFileOpExEnabled);
				rks.SetValue("IsCustomFO", IsCustomFO);
				rks.SetValue("SearchBarWidth", SearchBarWidth);
			}
		} //TODO: Make sure you only use this 1 time when the application closes OR when a new instance is opened
	}
}