using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using SingleInstanceApplication;
using System.Reflection;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using Fluent;
using System.Globalization;
using System.Diagnostics;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CompositionContainer container;
        private AggregateCatalog catalog;

        //[Import("MainWindow")]
        //public new Window MainWindow
        //{
        //    get { return base.MainWindow; }
        //    set { base.MainWindow = value; }
        //}

        /// <summary>
        /// Sets the UI language
        /// </summary>
        /// <param name="culture">Language code (ex. "en-EN")</param>
        public static void SelectCulture(string culture)
        {
            // List all our resources      
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            // We want our specific culture      
            string requestedCulture = string.Format("Locale.{0}.xaml", culture);
            ResourceDictionary resourceDictionary = 
                dictionaryList.FirstOrDefault(d => d.Source.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
            if (resourceDictionary == null)
            {
                // If not found, we select our default language        
                //        
                requestedCulture = "DefaultLocale.xaml";
                resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
            }

            // If we have the requested resource, remove it from the list and place at the end.\      
            // Then this language will be our string table to use.      
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
            // Inform the threads of the new culture      
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                bool dmi = true;

                if (e.Args != null && e.Args.Count() > 0)
                {
                    if (e.Args[0] == "/nw")
                    {
                        dmi = false;
                    }
                    else
                    {
                        //if (e.Args != null && e.Args.Count() > 0)
                        //{
                            this.Properties["cmd"] = e.Args[0];
                            if (e.Args.Count() > 1)
                            {
                                if (e.Args[1] == "/nw")
                                {
                                    dmi = false;
                                }
                            }
                        //}

                        if (dmi == true)
                        {
                            if (!ApplicationInstanceManager.CreateSingleInstance(
                            Assembly.GetExecutingAssembly().GetName().Name,
                            SingleInstanceCallback)) return; // exit, if same app. is running
                        }
                    }
                }
                else
                {
                    if (!ApplicationInstanceManager.CreateSingleInstance(
                    Assembly.GetExecutingAssembly().GetName().Name,
                    SingleInstanceCallback)) return; // exit, if same app. is running
                }
            }
            catch
            {
                MessageBox.Show("There was an error in the startup procedure related to consolidating multiple instances. Please let us know of this issue at http://bugtracker.better-explorer.com/", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string Locale = "";

            try
            {


                base.OnStartup(e);

                
                RegistryKey rk = Registry.CurrentUser;

                try
                {
                    RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);

                    if (rks == null)
                    {
                        rks = rk.CreateSubKey(@"Software\BExplorer");
                    }
                    var regLocale = rks.GetValue(@"Locale", "").ToString();
                    if (String.IsNullOrEmpty(regLocale))
                      Locale = CultureInfo.CurrentUICulture.EnglishName;
                    else
                      Locale = regLocale;

                    SelectCulture(Locale);

                    rks.Close();
                }
                catch
                {
        
                }
                rk.Close();

               
            }
            catch 
            {

                MessageBox.Show("A problem occurred while loading the locale from the Registry. This was the value in the Registry: \r\n \r\n " + Locale + "\r\n \r\nPlease report this issue at http://bugtracker.better-explorer.com/.");
                Shutdown();
            }

            
            

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception lastException = (Exception)e.ExceptionObject;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Fatal(lastException);
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception lastException = e.Exception;
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Fatal(lastException);
        }

        protected override void OnExit(ExitEventArgs e)
        {
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

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Activated"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            try
            {
                var win = MainWindow as MainWindow;
                if (win == null) return;

                // add 1st args
                win.ApendArgs(Environment.GetCommandLineArgs());
            }
            catch 
            {

            }
            
        }


        /// <summary>
        /// Single instance callback handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SingleInstanceApplication.InstanceCallbackEventArgs"/> instance containing the event data.</param>
        private void SingleInstanceCallback(object sender, InstanceCallbackEventArgs args)
        {
            string StartUpLocation = KnownFolders.Libraries.ParsingName;
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", false);

            StartUpLocation =
                 rks.GetValue(@"StartUpLoc", KnownFolders.Libraries.ParsingName).ToString();

            rks.Close();
            rk.Close();

            if (args == null || Dispatcher == null) return;
            Action<bool> d = (bool x) =>
            {
                var win = MainWindow as MainWindow;
                if (win == null) return;

                win.ApendArgs(args.CommandLineArgs);
                //win.Activate(x);
                if (x)
                {
                    ShellObject sho = null;
                    if (args != null && args.CommandLineArgs != null)
                    {
                        if (args.CommandLineArgs.Length > 1 && args.CommandLineArgs[1] != null)
                        {
                            if (args.CommandLineArgs[1] != "t")
                            {
                                if (args.CommandLineArgs[1] == "nw")
                                {
                                    BetterExplorer.MainWindow g = new MainWindow();
                                    g.Show();
                                    return;
                                }
                                else
                                {
                                    String cmd = args.CommandLineArgs[1];
                                    if (cmd.IndexOf("::") == 0)
                                    {
                                       sho = ShellObject.FromParsingName("shell:" + cmd);
                                    }
                                    else
                                        sho = ShellObject.FromParsingName(args.CommandLineArgs[1].Replace("\"", ""));
                                }
                            }
                            else
                            {
                                    if (StartUpLocation.IndexOf("::") == 0 && StartUpLocation.IndexOf(@"\") == -1)
                                        sho = ShellObject.FromParsingName("shell:" + StartUpLocation);
                                    else
                                        try
                                        {
                                            sho = ShellObject.FromParsingName(StartUpLocation);
                                        }
                                        catch
                                        {
                                            sho = (ShellObject)KnownFolders.Libraries;
                                        }
                            }
                        }
                        else
                            if (StartUpLocation.IndexOf("::") == 0 && StartUpLocation.IndexOf(@"\") == -1)
                                sho = ShellObject.FromParsingName("shell:" + StartUpLocation);
                            else
                                try
                                {
                                    sho = ShellObject.FromParsingName(StartUpLocation);
                                }
                                catch
                                {
                                    sho = (ShellObject)KnownFolders.Libraries;
                                }

                        sho.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                        sho.Thumbnail.CurrentSize = new Size(16, 16);
                        CloseableTabItem newt = new CloseableTabItem();
                        newt.Header = sho.GetDisplayName(DisplayNameType.Default);
                        newt.TabIcon = sho.Thumbnail.BitmapSource;
                        newt.Path = sho;
                        win.CloneTab(newt);
                        win.NavigateAfterTabChange();
                        
                        IntPtr MainWinHandle = WindowsHelper.WindowsAPI.FindWindow(null, Process.GetCurrentProcess().MainWindowTitle);
                        if (win.WindowState == WindowState.Minimized)
                        {
                            WindowsHelper.WindowsAPI.ShowWindow((int)MainWinHandle, 
                                (int)WindowsHelper.WindowsAPI.ShowCommands.SW_RESTORE);
                        }
                        WindowsHelper.WindowsAPI.BringWindowToTop(MainWinHandle);
                        WindowsHelper.WindowsAPI.SetForegroundWindow(MainWinHandle);
                        win.Activate();

                    }
                }
            };
            Dispatcher.BeginInvoke(d, true);
        }

       
        
    }
    
}
