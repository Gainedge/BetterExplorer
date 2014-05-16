using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using SingleInstanceApplication;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Fluent;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsHelper;
using BExplorer.Shell;
using BExplorer.Shell.Interop;

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		//private CompositionContainer container;
		//private AggregateCatalog catalog;
		public static bool isStartMinimized = false, isStartNewWindows = false, isStartWithStartupTab = false;

		//[Import("MainWindow")]
		//public new Window MainWindow
		//{
		//    get { return base.MainWindow; }
		//    set { base.MainWindow = value; }
		//}

		/*
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Application.Activated"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);

			try {
				var win = MainWindow as MainWindow;
				if (win == null) return;

				//Aaron Canpf => Bookmark
				// add 1st args
				//win.ApendArgs(Environment.GetCommandLineArgs());
			}
			catch { }
		}
		*/

		/*
		protected override void OnExit(ExitEventArgs e) {
			base.OnExit(e);

			//if (container != null)
			//{
			//    container.Dispose();
			//}

			//if (catalog != null)
			//{
			//    catalog.Dispose();
			//}
		}
		*/



		#region Unused

		void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e) {
			//TODO: add code for sleep, resume, etc. modes
		}

		void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
			//TODO: Add code for lock and other reasons
		}

		#endregion

		#region Culture

		/// <summary>
		/// Sets the UI language
		/// </summary>
		/// <param name="culture">Language code (ex. "en-EN")</param>
		public void SelectCulture(string culture) {
			if (culture == ":null:")
				culture = CultureInfo.InstalledUICulture.Name;
			// List all our resources      
			List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
			foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries) {
				dictionaryList.Add(dictionary);
			}
			// We want our specific culture      
			string requestedCulture = string.Format("Locale.{0}.xaml", culture);
			ResourceDictionary resourceDictionary =
					dictionaryList.FirstOrDefault(d => d.Source.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
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
			if (culture == ":null:")
				culture = CultureInfo.InstalledUICulture.Name;
			// List all our resources      
			List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
			foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries) {
				dictionaryList.Add(dictionary);
			}

			ResourceDictionary resourceDictionary = ResDictionaryLoader.Load(filename);
			if (resourceDictionary == null) {
				// if not found, then try from the application's resources
				string requestedCulture = string.Format("Locale.{0}.xaml", culture);
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
				}
				catch {

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
			Exception lastException = (Exception)e.ExceptionObject;
			NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
			logger.Fatal(lastException);
		}

		void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			Exception lastException = e.Exception;
			NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
			logger.Fatal(lastException);
		}

		#endregion


		protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {
			Application.Current.Shutdown();
			base.OnSessionEnding(e);
		}

		protected override void OnStartup(StartupEventArgs e) {
			Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
			SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

			string Locale = "";
			bool dmi = true;

			if (e.Args != null && e.Args.Count() > 0) {
				dmi = e.Args.Any((x) => x == "/nw");
				isStartWithStartupTab = e.Args.Any((x) => x == "/norestore");

				//TODO: Consider Refactoring this so [e.Args[0] != "-minimized"] is inside the switch 
				if (e.Args[0] != "-minimized")
					this.Properties["cmd"] = e.Args[0];
				else
					isStartMinimized = true;
			}

			isStartNewWindows = dmi;
			if (isStartNewWindows) {
				if (!ApplicationInstanceManager.CreateSingleInstance(Assembly.GetExecutingAssembly().GetName().Name, SingleInstanceCallback))
					return; // exit, if same app. is running
			}

			base.OnStartup(e);
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);
			if (rks == null)
				rks = rk.CreateSubKey(@"Software\BExplorer");

			try {
				var regLocale = rks.GetValue(@"Locale", "").ToString();
				Locale = String.IsNullOrEmpty(regLocale) ? CultureInfo.CurrentUICulture.Name : regLocale;
				SelectCulture(Locale);
			}
			catch {
				MessageBox.Show(String.Format("A problem occurred while loading the locale from the Registry. This was the value in the Registry: \r\n \r\n {0}\r\n \r\nPlease report this issue at http://bugtracker.better-explorer.com/.", Locale));
				Shutdown();
			}

			rks.Close();
			rk.Close();
		}

		private void CreateInitialTab(MainWindow win, ShellItem sho) {
			sho.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
			sho.Thumbnail.CurrentSize = new System.Windows.Size(16, 16);

			Wpf.Controls.TabItem newt = new Wpf.Controls.TabItem();
			newt.Header = sho.GetDisplayName(SIGDN.NORMALDISPLAY);
			newt.Icon = sho.Thumbnail.BitmapSource;
			newt.PreviewMouseMove += newt_PreviewMouseMove;
			newt.ToolTip = sho.ParsingName;
			newt.ShellObject = sho;
			win.CloneTab(newt);
		}
		/// <summary>
		/// Single instance callback handler.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="SingleInstanceApplication.InstanceCallbackEventArgs"/> instance containing the event data.</param>
		private void SingleInstanceCallback(object sender, InstanceCallbackEventArgs args) {
			string StartUpLocation = KnownFolders.Libraries.ParsingName;
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", false);
			StartUpLocation = rks.GetValue(@"StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();

			if (args == null || Dispatcher == null) return;
			Action<bool> d = (bool x) => {
				var win = MainWindow as MainWindow;
				var hwnd = (HwndSource.FromVisual(win) as HwndSource).Handle;
				ShellItem sho = null;

				if (!x) return;
				if (!(args != null && args.CommandLineArgs != null)) return;
				if (win == null) return;

				var Check = (args.CommandLineArgs.Length > 1 && args.CommandLineArgs[1] != null && args.CommandLineArgs[1] != "-minimized");
				if (!Check || args.CommandLineArgs[1] == "t") {
					win.Visibility = Visibility.Visible;
					if (win.WindowState == WindowState.Minimized) {
						WindowsAPI.ShowWindow(hwnd, (int)WindowsAPI.ShowCommands.SW_RESTORE);
					};

					sho = new ShellItem(StartUpLocation.ToShellParsingName());
				}
				else if (Check && args.CommandLineArgs[1] != "t") {
					if (args.CommandLineArgs[1] == "/nw") {
						new MainWindow().Show();
						return;
					}
					else {
						String cmd = args.CommandLineArgs[1];
						sho = new ShellItem(cmd.ToShellParsingName());
					}
				}

				//TODO: Try refactoring [Worker] away 
				if (!isStartMinimized || win.tcMain.Items.Count == 0) {
					CreateInitialTab(win, sho);
				}
				else if ((int)rks.GetValue(@"IsRestoreTabs", 1) == 0) {
					win.tcMain.Items.Clear();
					CreateInitialTab(win, sho);
				}
				else if (args.CommandLineArgs.Length > 1 && args.CommandLineArgs[1] != null) {
					String cmd = args.CommandLineArgs[1];
					sho = new ShellItem(cmd.ToShellParsingName());
					CreateInitialTab(win, sho);
				}

				rks.Close();
				rk.Close();

				win.Activate();
				win.Topmost = true;  // important
				win.Topmost = false; // important
				win.Focus();         // important				
			};
			Dispatcher.BeginInvoke(d, true);
		}

		void newt_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
			var tabItem = e.Source as Wpf.Controls.TabItem;

			if (tabItem == null)
				return;
			else if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
				DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
		}
	}
}
