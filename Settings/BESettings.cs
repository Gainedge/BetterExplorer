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
		public static Double InfoPaneHeight { get; set; }
		public static Boolean IsPreviewPaneEnabled { get; set; }
		public static Double PreviewPaneWidth { get; set; }
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
		public static bool OverwriteImageWhileEditing { get; set; }
		public static string SavedTabsDirectory { get; set; }
		public static string OpenedTabs { get; set; }
		public static double LastWindowWidth { get; set; }
		public static double LastWindowHeight { get; set; }
		public static double LastWindowPosLeft { get; set; }
		public static double LastWindowPosTop { get; set; }
		public static int LastWindowState { get; set; }
		public static bool IsRibonMinimized { get; set; }
		public static bool RTLMode { get; set; }
		public static bool AutoSwitchFolderTools { get; set; }
		public static bool AutoSwitchArchiveTools { get; set; }
		public static bool AutoSwitchImageTools { get; set; }
		public static bool AutoSwitchApplicationTools { get; set; }
		public static bool AutoSwitchLibraryTools { get; set; }
		public static bool AutoSwitchDriveTools { get; set; }
		public static bool AutoSwitchVirtualDriveTools { get; set; }
		public static bool ShowCheckboxes { get; set; }
		public static double CmdWinHeight { get; set; }
		public static string TabBarAlignment { get; set; }
		public static bool HFlyoutEnabled { get; set; }

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
				var regLocale = rksRoot.GetValue("Locale", ":null:").ToString();
				Locale = string.IsNullOrEmpty(regLocale) ? System.Globalization.CultureInfo.CurrentUICulture.Name : regLocale;
				IsRestoreTabs = bool.Parse(rksRoot.GetValue("IsRestoreTabs", "False").ToString());
				InfoPaneHeight = int.Parse(rksRoot.GetValue("InfoPaneHeight", 150).ToString());
				IsNavigationPaneEnabled = bool.Parse(rksRoot.GetValue("NavigationPaneEnabled", "False").ToString());
				IsFileOpExEnabled = bool.Parse(rksRoot.GetValue("FileOpExEnabled", "False").ToString());
				IsCustomFO = bool.Parse(rksRoot.GetValue("IsCustomFO", "False").ToString());
				SearchBarWidth = Convert.ToDouble(rksRoot.GetValue("SearchBarWidth", 220));
				OverwriteImageWhileEditing = bool.Parse(rksRoot.GetValue("OverwriteImageWhileEditing", "False").ToString());
				SavedTabsDirectory = rksRoot.GetValue("SavedTabsDirectory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\").ToString();
				OpenedTabs = rksRoot.GetValue("OpenedTabs").ToString();
				LastWindowWidth = double.Parse(rksRoot.GetValue("LastWindowWidth", 640).ToString());
				LastWindowHeight = double.Parse(rksRoot.GetValue("LastWindowHeight", 480).ToString());
				LastWindowPosLeft = double.Parse(rksRoot.GetValue("LastWindowPosLeft", 0).ToString());
				LastWindowPosTop = double.Parse(rksRoot.GetValue("LastWindowPosTop", 0).ToString());
				LastWindowState = int.Parse(rksRoot.GetValue("LastWindowState", 0).ToString());
				IsRibonMinimized = bool.Parse(rksRoot.GetValue("IsRibonMinimized", "False").ToString());
				RTLMode = bool.Parse(rksRoot.GetValue("RTLMode", "False").ToString());
				AutoSwitchFolderTools = bool.Parse(rksRoot.GetValue("AutoSwitchFolderTools", "False").ToString());
				AutoSwitchArchiveTools = bool.Parse(rksRoot.GetValue("AutoSwitchArchiveTools", "True").ToString());
				AutoSwitchImageTools = bool.Parse(rksRoot.GetValue("AutoSwitchImageTools", "True").ToString());
				AutoSwitchApplicationTools = bool.Parse(rksRoot.GetValue("AutoSwitchApplicationTools", "False").ToString());
				AutoSwitchLibraryTools = bool.Parse(rksRoot.GetValue("AutoSwitchLibraryTools", "True").ToString());
				AutoSwitchDriveTools = bool.Parse(rksRoot.GetValue("AutoSwitchDriveTools", "True").ToString());
				AutoSwitchVirtualDriveTools = bool.Parse(rksRoot.GetValue("AutoSwitchVirtualDriveTools", "False").ToString());
				ShowCheckboxes = bool.Parse(rksRoot.GetValue("ShowCheckboxes", "False").ToString());
				CmdWinHeight = double.Parse(rksRoot.GetValue("CmdWinHeight", 100).ToString());
				TabBarAlignment = rksRoot.GetValue("TabBarAlignment", "top").ToString();
				HFlyoutEnabled = bool.Parse(rksRoot.GetValue("HFlyoutEnabled", "False").ToString());
				PreviewPaneWidth = double.Parse(rksRoot.GetValue("PreviewPaneWidth", 120).ToString());
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
				rks.SetValue("OverwriteImageWhileEditing", OverwriteImageWhileEditing);
				rks.SetValue("SavedTabsDirectory", SavedTabsDirectory);
				rks.SetValue("OpenedTabs", OpenedTabs);
				rks.SetValue("LastWindowWidth", LastWindowWidth);
				rks.SetValue("LastWindowHeight", LastWindowHeight);
				rks.SetValue("LastWindowPosLeft", LastWindowPosLeft);
				rks.SetValue("LastWindowPosTop", LastWindowPosTop);
				rks.SetValue("LastWindowState", LastWindowState);
				rks.SetValue("IsRibonMinimized", IsRibonMinimized);
				rks.SetValue("OpenedTabs", OpenedTabs);
				rks.SetValue("RTLMode", RTLMode);
				rks.SetValue("AutoSwitchFolderTools", AutoSwitchFolderTools);
				rks.SetValue("AutoSwitchArchiveTools", AutoSwitchArchiveTools);
				rks.SetValue("AutoSwitchImageTools", AutoSwitchImageTools);
				rks.SetValue("AutoSwitchApplicationTools", AutoSwitchApplicationTools);
				rks.SetValue("AutoSwitchLibraryTools", AutoSwitchLibraryTools);
				rks.SetValue("AutoSwitchDriveTools", AutoSwitchDriveTools);
				rks.SetValue("AutoSwitchVirtualDriveTools", AutoSwitchVirtualDriveTools);
				rks.SetValue("ShowCheckboxes", ShowCheckboxes);
				rks.SetValue("CmdWinHeight", CmdWinHeight);
				rks.SetValue("TabBarAlignment", TabBarAlignment);
				rks.SetValue("HFlyoutEnabled", HFlyoutEnabled);
				rks.SetValue("PreviewPaneWidth", PreviewPaneWidth);
			}
		} //TODO: Make sure you only use this 1 time when the application closes OR when a new instance is opened
	}
}