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
        IsUpdateCheck = rksRoot.GetValue("CheckForUpdates", "False").ToBoolean();
        IsUpdateCheckStartup = rksRoot.GetValue("CheckForUpdatesStartup", "False").ToBoolean();
        UpdateCheckInterval = (int)rksRoot.GetValue("CheckInterval", 7);
        LastUpdateCheck = DateTime.FromBinary(Convert.ToInt64(rksRoot.GetValue("LastUpdateCheck", DateTime.Now.ToBinary())));
        //LastUpdateCheck = DateTime.Parse(rksRoot.GetValue("LastUpdateCheck", DateTime.Now.ToString()).ToString());
        IsConsoleShown = rksRoot.GetValue("IsConsoleShown", "False").ToBoolean();
        EnableActionLog = rksRoot.GetValue("IsConsoleShown", "False").ToBoolean();
        IsGlassOnRibonMinimized = rksRoot.GetValue("RibbonMinimizedGlass", "False").ToBoolean();
        IsPreviewPaneEnabled = rksRoot.GetValue("PreviewPaneEnabled", "False").ToBoolean();
        IsInfoPaneEnabled = rksRoot.GetValue("InfoPaneEnabled", "False").ToBoolean();
        IsTraditionalNameGrouping = rksRoot.GetValue("IsTraditionalNameGrouping", "False").ToBoolean();
        var regLocale = rksRoot.GetValue("Locale", ":null:").ToString();
        Locale = string.IsNullOrEmpty(regLocale) ? System.Globalization.CultureInfo.CurrentUICulture.Name : regLocale;
        IsRestoreTabs = rksRoot.GetValue("IsRestoreTabs", "False").ToBoolean();
        InfoPaneHeight = Convert.ToInt32(rksRoot.GetValue("InfoPaneHeight", 150));
        IsNavigationPaneEnabled = rksRoot.GetValue("NavigationPaneEnabled", "False").ToBoolean();
        IsFileOpExEnabled = rksRoot.GetValue("FileOpExEnabled", "False").ToBoolean();
        IsCustomFO = rksRoot.GetValue("IsCustomFO", "False").ToBoolean();
        SearchBarWidth = Convert.ToDouble(rksRoot.GetValue("SearchBarWidth", 220));
        OverwriteImageWhileEditing = rksRoot.GetValue("OverwriteImageWhileEditing", "False").ToBoolean();
        SavedTabsDirectory = rksRoot.GetValue("SavedTabsDirectory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\BExplorer_SavedTabs\\").ToString();
        OpenedTabs = rksRoot.GetValue("OpenedTabs").ToString();
        LastWindowWidth = Convert.ToDouble(rksRoot.GetValue("LastWindowWidth", 640));
        LastWindowHeight = Convert.ToDouble(rksRoot.GetValue("LastWindowHeight", 480));
        LastWindowPosLeft = Convert.ToDouble(rksRoot.GetValue("LastWindowPosLeft", 0));
        LastWindowPosTop = Convert.ToDouble(rksRoot.GetValue("LastWindowPosTop", 0));
        LastWindowState = Convert.ToInt32(rksRoot.GetValue("LastWindowState", 0));
        IsRibonMinimized = rksRoot.GetValue("IsRibonMinimized", "False").ToBoolean();
        RTLMode = rksRoot.GetValue("RTLMode", "False").ToBoolean();
        AutoSwitchFolderTools = rksRoot.GetValue("AutoSwitchFolderTools", "False").ToBoolean();
        AutoSwitchArchiveTools = rksRoot.GetValue("AutoSwitchArchiveTools", "True").ToBoolean();
        AutoSwitchImageTools = rksRoot.GetValue("AutoSwitchImageTools", "True").ToBoolean();
        AutoSwitchApplicationTools = rksRoot.GetValue("AutoSwitchApplicationTools", "False").ToBoolean();
        AutoSwitchLibraryTools = rksRoot.GetValue("AutoSwitchLibraryTools", "True").ToBoolean();
        AutoSwitchDriveTools = rksRoot.GetValue("AutoSwitchDriveTools", "True").ToBoolean();
        AutoSwitchVirtualDriveTools = rksRoot.GetValue("AutoSwitchVirtualDriveTools", "False").ToBoolean();
        ShowCheckboxes = rksRoot.GetValue("ShowCheckboxes", "False").ToBoolean();
        CmdWinHeight = Convert.ToDouble(rksRoot.GetValue("CmdWinHeight", 100));
        TabBarAlignment = rksRoot.GetValue("TabBarAlignment", "top").ToString();
        HFlyoutEnabled = rksRoot.GetValue("HFlyoutEnabled", "False").ToBoolean();
        PreviewPaneWidth = Convert.ToDouble(rksRoot.GetValue("PreviewPaneWidth", 120));
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
        rks.SetValue("LastUpdateCheck", LastUpdateCheck.ToBinary(), RegistryValueKind.QWord);
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