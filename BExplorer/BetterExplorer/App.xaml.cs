using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SingleInstanceApplication;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Threading;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Interop;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using BExplorer.Shell._Plugin_Interfaces;
using System.IO;

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		public static bool IsStartMinimized, IsStartWithStartupTab;

		#region Culture

		/// <summary>
		/// Sets the UI language
		/// </summary>
		/// <param name="culture">Language code (ex. "en-EN")</param>
		public void SelectCulture(string culture) {
			if (culture == ":null:") culture = CultureInfo.InstalledUICulture.Name;
			// List all our resources      
			var dictionaryList = new List<ResourceDictionary>(Application.Current.Resources.MergedDictionaries);
			// We want our specific culture      
			string requestedCulture = $"Locale.{culture}.xaml";
			ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
			if (resourceDictionary == null) {
				// If not found, we select our default language        
				//        
				requestedCulture = "DefaultLocale.xaml";
				resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
			}

			// If we have the requested resource, remove it from the list and place at the end.\      
			// Then this language will be our string table to use.      
			if (resourceDictionary != null) {
				Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
				Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
			}
			// Inform the threads of the new culture    

			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

		}

		/// <summary>
		/// Sets the UI language
		/// </summary>
		/// <param name="culture">Language code (ex. "en-EN")</param>
		/// <param name="filename">The file to load the resources from</param>
		public void SelectCulture(string culture, string filename) {
			if (culture == ":null:") culture = CultureInfo.InstalledUICulture.Name;
			// List all our resources      
			var dictionaryList = new List<ResourceDictionary>(Application.Current.Resources.MergedDictionaries);

			var resourceDictionary = Utilities.Load(filename);
			if (resourceDictionary == null) {
				// if not found, then try from the application's resources
				string requestedCulture = $"Locale.{culture}.xaml";
				resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
				if (resourceDictionary == null) {
					// If not found, we select our default language        
					//        
					requestedCulture = "DefaultLocale.xaml";
					resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
				}
			}

			// If we have the requested resource, remove it from the list and place at the end.\      
			// Then this language will be our string table to use.      
			if (resourceDictionary != null) {
				try {
					Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
				} catch {
				}

				Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
			}
			// Inform the threads of the new culture      
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
			Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
		}

		#endregion

		#region UnhandledException

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			var lastException = (Exception)e.ExceptionObject;
			var logger = NLog.LogManager.GetCurrentClassLogger();
			logger.Fatal(lastException);
		}

		void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			var lastException = e.Exception;
			var logger = NLog.LogManager.GetCurrentClassLogger();
			logger.Fatal(lastException);

			e.Handled = true;
		}

		#endregion


		protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {
			Application.Current.Shutdown();
			base.OnSessionEnding(e);
		}


		protected override void OnStartup(StartupEventArgs e) {
			string locale = ""; bool dmi = true;
			Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			//System.AppDomain.CurrentDomain.BaseDirectory

			if (!File.Exists(Path.Combine(KnownFolders.RoamingAppData.ParsingName, @"BExplorer\Settings.sqlite"))) {
				File.Copy(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Settings.sqlite"), Path.Combine(KnownFolders.RoamingAppData.ParsingName, @"BExplorer\Settings.sqlite"));
			}

			RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true);
			if (rks == null) {
				rk.CreateSubKey(@"Software\BExplorer");
				rks = rk.OpenSubKey(@"Software\BExplorer", true);
			}

			//// loads current Ribbon color theme
			try {
				var Color = Convert.ToString(rks.GetValue("CurrentTheme", "Blue"));
				Application.Current.Resources.MergedDictionaries.RemoveAt(1);

				switch (Color) {
					case "Blue":
					case "Silver":
					case "Black":
					case "Green":
						Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = new Uri($"pack://application:,,,/Fluent;component/Themes/Office2010/{Color}.xaml") });
						break;
					case "Metro":
						Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Fluent;component/Themes/Office2013/Generic.xaml") });
						break;
					default:
						Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = new Uri($"pack://application:,,,/Fluent;component/Themes/Office2010/{Color}.xaml") });
						break;
				}
			} catch (Exception ex) {
				//MessageBox.Show(String.Format("An error occurred while trying to load the theme data from the Registry. \n\r \n\r{0}\n\r \n\rPlease let us know of this issue at http://bugtracker.better-explorer.com/", ex.Message), "RibbonTheme Error - " + ex.ToString());
				MessageBox.Show($"An error occurred while trying to load the theme data from the Registry. \n\r \n\rRibbonTheme Error - {ex.ToString()}\n\r \n\rPlease let us know of this issue at http://bugtracker.better-explorer.com/", ex.Message);
			}

			rks.Close();
			rk.Close();

			if (e.Args != null && e.Args.Any()) {
				dmi = e.Args.Contains("/nw") || e.Args[0] == "t";
				IsStartWithStartupTab = e.Args.Contains("/norestore");

				if (e.Args[0] != "-minimized")
					this.Properties["cmd"] = e.Args[0];
				else
					IsStartMinimized = true;
			}

			if (dmi && !ApplicationInstanceManager.CreateSingleInstance(Assembly.GetExecutingAssembly().GetName().Name, SingleInstanceCallback))
				return; // exit, if same app. is running

			base.OnStartup(e);

			try {
				var regLocale = Utilities.GetRegistryValue("Locale", "").ToString();
				locale = String.IsNullOrEmpty(regLocale) ? CultureInfo.CurrentUICulture.Name : regLocale;
				SelectCulture(locale);
			} catch {
				//MessageBox.Show(String.Format("A problem occurred while loading the locale from the Registry. This was the value in the Registry: \r\n \r\n {0}\r\n \r\nPlease report this issue at http://bugtracker.better-explorer.com/.", Locale));
				MessageBox.Show($"A problem occurred while loading the locale from the Registry. This was the value in the Registry: \r\n \r\n {locale}\r\n \r\nPlease report this issue at http://bugtracker.better-explorer.com/.");

				Shutdown();
			}
		}

		private void CreateInitialTab(MainWindow win, IListItemEx sho) {
			var bmpSource = sho.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
			var newt = new Wpf.Controls.TabItem(sho) {
				Header = sho.DisplayName,
				Icon = bmpSource
			};
			newt.PreviewMouseMove += newt_PreviewMouseMove;
			newt.ToolTip = sho.ParsingName.Replace("%20", " ").Replace("%3A", ":").Replace("%5C", @"\");
			win.tcMain.CloneTabItem(newt);
		}

		/// <summary>
		/// Single instance callback handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="SingleInstanceApplication.InstanceCallbackEventArgs"/> instance containing the event data.</param>
		/// 
		private void SingleInstanceCallback(object sender, InstanceCallbackEventArgs args) {
			if (args == null || Dispatcher == null) return;
			var startUpLocation = Utilities.GetRegistryValue("StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();

			Action<bool> d = x => {
				var win = MainWindow as MainWindow;

				if (!x) return;
				if (args?.CommandLineArgs == null || !args.CommandLineArgs.Any()) return;
				if (win == null) return;
				if (args.CommandLineArgs[1] == "/nw") {
					new MainWindow() { IsMultipleWindowsOpened = true }.Show();
				} else {
					IListItemEx sho = null;
					if (args.CommandLineArgs[1] == "t") {
						win.Visibility = Visibility.Visible;
						if (win.WindowState == WindowState.Minimized)
							User32.ShowWindow((PresentationSource.FromVisual(win) as HwndSource).Handle, User32.ShowWindowCommands.Restore);

						sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, startUpLocation);
					} else {
						sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, args.CommandLineArgs[1]);
					}

					if (!IsStartMinimized || win.tcMain.Items.Count == 0) {
						CreateInitialTab(win, sho);
					} else if ((int)Utilities.GetRegistryValue("IsRestoreTabs", "1") == 0) {
						win.tcMain.Items.Clear();
						CreateInitialTab(win, sho);
					} else if (args.CommandLineArgs.Length > 1 && args.CommandLineArgs[1] != null) {
						String cmd = args.CommandLineArgs[1];
						sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, cmd);
						CreateInitialTab(win, sho);
					}
				}
				win.Activate();
				win.Topmost = true;  // important
				win.Topmost = false; // important
				win.Focus();         // important				
			};
			Dispatcher.BeginInvoke(d, true);
		}

		void newt_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
			var tabItem = e.Source as Wpf.Controls.TabItem;

			if (tabItem != null && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
				DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
		}
	}
}
