// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Gainedge ORG">
//   Gainedge ORG
// </copyright>
// <summary>
//   Interaction logic for App.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using Fluent;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using SingleInstanceCore;
using Application = System.Windows.Application;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;

namespace BetterExplorer {
  using System.Diagnostics;

  using MouseEventArgs = System.Windows.Input.MouseEventArgs;

  /// <summary>
  /// Interaction logic for App
  /// </summary>
  public partial class App: ISingleInstance {
    /// <summary>
    /// Gets or sets a value indicating whether app starting in minimized state
    /// </summary>
    public static Boolean IsStartMinimized { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether app starting whit only one initial tab
    /// </summary>
    public static Boolean IsStartWithStartupTab { get; set; }

    #region Culture

    /// <summary>
    /// Sets the UI language
    /// </summary>
    /// <param name="culture">Language code (ex. "en-EN")</param>
    public void SelectCulture(String culture) {
      if (culture == ":null:") {
        culture = CultureInfo.InstalledUICulture.Name;
      }

      // List all our resources      
      var dictionaryList = new List<ResourceDictionary>(Current.Resources.MergedDictionaries);

      // We want our specific culture      
      String requestedCulture = $"Locale.{culture}.xaml";
      ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source?.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
      if (resourceDictionary == null) {
        // If not found, we select our default language         
        requestedCulture = "DefaultLocale.xaml";
        resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source?.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
      }

      // If we have the requested resource, remove it from the list and place at the end.\      
      // Then this language will be our string table to use.      
      if (resourceDictionary != null) {
        Current.Resources.MergedDictionaries.Remove(resourceDictionary);
        Current.Resources.MergedDictionaries.Add(resourceDictionary);
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
    public void SelectCulture(String culture, String filename) {
      if (culture == ":null:") {
        culture = CultureInfo.InstalledUICulture.Name;
      }

      // List all our resources  
      var dictionaryList = new List<ResourceDictionary>(Current.Resources.MergedDictionaries);

      var resourceDictionary = Utilities.Load(filename);
      if (resourceDictionary == null) {
        // if not found, then try from the application's resources
        String requestedCulture = $"Locale.{culture}.xaml";
        resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source?.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
        if (resourceDictionary == null) {
          // If not found, we select our default language             
          requestedCulture = "DefaultLocale.xaml";
          resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source?.OriginalString == "/BetterExplorer;component/Translation/" + requestedCulture);
        }
      }

      // If we have the requested resource, remove it from the list and place at the end.\      
      // Then this language will be our string table to use.      
      if (resourceDictionary != null) {
        try {
          Current.Resources.MergedDictionaries.Remove(resourceDictionary);
        } catch {
        }

        Current.Resources.MergedDictionaries.Add(resourceDictionary);
      }

      // Inform the threads of the new culture      
      Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);
      Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
    }

    #endregion

    /// <summary>
    /// On finishing session
    /// </summary>
    /// <param name="e">SessionEndingCancel EventArgs</param>
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {
      Current.Shutdown();
      Settings.BESettings.SaveSettings();
      base.OnSessionEnding(e);
    }

    /// <summary>
    /// On app start
    /// </summary>
    /// <param name="e">Startup EventArgs</param>
    protected override void OnStartup(StartupEventArgs e) {

      var dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      if (IntPtr.Size == 8)
      {
        dllPath = Path.Combine(dllPath, "X64");
      } else {
        // X32
        dllPath = Path.Combine(dllPath, "X86"); 
      }
      Kernel32.SetDllDirectory(dllPath);

      Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
      var updaterThread = new Thread(new ThreadStart(this.RunAutomaticUpdateChecker));
      updaterThread.Start();

      ToastNotificationManagerCompat.OnActivated += toastArgs => {
        // Obtain the arguments from the notification
        ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

        // Obtain any user input (text boxes, menu selections) from the notification
        var userInput = toastArgs.UserInput;

        // Need to dispatch to UI thread if performing UI operations
        Application.Current.Dispatcher.Invoke(delegate {
          // TODO: Show the corresponding content
          MessageBox.Show("Toast activated. Args: " + toastArgs.Argument);
        });
      };

      Settings.BESettings.LoadSettings();

      //Process process = Process.GetCurrentProcess();
      //process.PriorityClass = ProcessPriorityClass.Normal;

      //// Set the current thread to run at 'Highest' Priority
      //Thread thread = Thread.CurrentThread;
      //thread.Priority = ThreadPriority.Highest;

      Boolean dmi = true;
      System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
      Current.DispatcherUnhandledException += this.Current_DispatcherUnhandledException;
      AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
      System.Windows.Forms.Application.ThreadException += this.Application_ThreadException;

      if (!File.Exists(Path.Combine(KnownFolders.RoamingAppData.ParsingName, @"BExplorer\Settings.sqlite"))) {
        var beAppDataPath = Path.Combine(KnownFolders.RoamingAppData.ParsingName, @"BExplorer");
        if (!Directory.Exists(beAppDataPath)) {
          Directory.CreateDirectory(beAppDataPath);
        }
        File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.sqlite"), Path.Combine(KnownFolders.RoamingAppData.ParsingName, @"BExplorer\Settings.sqlite"));
      }

      //// loads current Ribbon color theme
      try {
        switch (Settings.BESettings.CurrentTheme) {
          case "Dark":
            ThemeManager.ChangeAppTheme(this, "BaseDark");
            UxTheme.AllowDarkModeForApp(true);
            break;
          default:
            ThemeManager.ChangeAppTheme(this, "BaseLight");
            UxTheme.AllowDarkModeForApp(false);
            break;
        }
      } catch (Exception ex) {
        // MessageBox.Show(String.Format("An error occurred while trying to load the theme data from the Registry. \n\r \n\r{0}\n\r \n\rPlease let us know of this issue at http://bugtracker.better-explorer.com/", ex.Message), "RibbonTheme Error - " + ex.ToString());
        MessageBox.Show(
          $"An error occurred while trying to load the theme data from the Registry. \n\r \n\rRibbonTheme Error - {ex}\n\r \n\rPlease let us know of this issue at http://bugtracker.better-explorer.com/",
          ex.Message);
      }

      if (e.Args.Any()) {
        dmi = e.Args.Length >= 1;
        IsStartWithStartupTab = e.Args.Contains("/norestore");

        if (e.Args[0] != "-minimized") {
          this.Properties["cmd"] = e.Args[0];
        } else {
          IsStartMinimized = true;
        }
      }

      bool isFirstInstance = this.InitializeAsFirstInstance("BetterExplorer_SI_IPC");
      if (!isFirstInstance) {
        //If it's not the first instance, arguments are automatically passed to the first instance
        //OnInstanceInvoked will be raised on the first instance
        //You may shut down the current instance
        Current.Shutdown();
      }

      base.OnStartup(e);

      try {
        this.SelectCulture(Settings.BESettings.Locale);
      } catch {
        // MessageBox.Show(String.Format("A problem occurred while loading the locale from the Registry. This was the value in the Registry: \r\n \r\n {0}\r\n \r\nPlease report this issue at http://bugtracker.better-explorer.com/.", Locale));
        MessageBox.Show($"A problem occurred while loading the locale from the Registry. This was the value in the Registry: \r\n \r\n {Settings.BESettings.Locale}\r\n \r\nPlease report this issue at http://bugtracker.better-explorer.com/.");

        this.Shutdown();
      }
    }
    public void RunAutomaticUpdateChecker() {
      Thread.Sleep(5000);
      var updaterPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "BEUpdater.exe");
      var process = Process.Start(updaterPath, "/silent");
      process?.Close();
    }
    protected override void OnExit(ExitEventArgs e) {
      base.OnExit(e);
      SingleInstance.Cleanup();
      Environment.Exit(Environment.ExitCode);
    }

    #region UnhandledException
    /// <summary>
    /// Handle all exceptions from current domain
    /// </summary>
    /// <param name="sender">Who send the exception</param>
    /// <param name="e">the args</param>
    private void CurrentDomain_UnhandledException(Object sender, UnhandledExceptionEventArgs e) {
      var lastException = (Exception)e.ExceptionObject;
      var logger = NLog.LogManager.GetCurrentClassLogger();
      logger.Fatal(lastException);
    }

    /// <summary>
    /// Handle all exceptions from current dispatcher
    /// </summary>
    /// <param name="sender">Who send the exception</param>
    /// <param name="e">the args</param>
    private void Current_DispatcherUnhandledException(Object sender, DispatcherUnhandledExceptionEventArgs e) {
      var lastException = e.Exception;
      var logger = NLog.LogManager.GetCurrentClassLogger();
      logger.Fatal(lastException);

      e.Handled = true;
    }

    /// <summary>
    /// Handle all exceptions from current thread
    /// </summary>
    /// <param name="sender">Who send the exception</param>
    /// <param name="e">the args</param>
    private void Application_ThreadException(Object sender, ThreadExceptionEventArgs e) {
      var lastException = e.Exception;
      var logger = NLog.LogManager.GetCurrentClassLogger();
      logger.Fatal(lastException);
    }

    #endregion

    /// <summary>
    /// Creates initial tab
    /// </summary>
    /// <param name="win">The main window</param>
    /// <param name="sho">The shell object used for tab creation</param>
    private void CreateInitialTab(MainWindow win, IListItemEx sho) {
      var bmpSource = sho.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
      var newt = new Wpf.Controls.TabItem(sho) {
        Header = sho.DisplayName,
        Icon = bmpSource
      };
      newt.PreviewMouseMove += this.Newt_PreviewMouseMove;
      newt.ToolTip = sho.ParsingName.Replace("%20", " ").Replace("%3A", ":").Replace("%5C", @"\");
      win.tcMain.CloneTabItem(newt);
    }

    /// <summary>
    /// Single instance callback handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="SingleInstanceApplication.InstanceCallbackEventArgs"/> instance containing the event data.</param>
    //private void SingleInstanceCallback(Object sender, InstanceCallbackEventArgs args) {
    //  if (args == null || this.Dispatcher == null) {
    //    return;
    //  }

    //  var startUpLocation = Settings.BESettings.StartupLocation;

    //  Action<Boolean> d = x => {
    //    var win = this.MainWindow as MainWindow;
    //    var windowsActivate = new CombinedWindowActivator();
    //    if (!x || win == null) {
    //      return;
    //    }

    //    win.StateChanged += this.Win_StateChanged;
    //    if (args.CommandLineArgs == null || !args.CommandLineArgs.Any()) {
    //      return;
    //    }

    //    if (args.CommandLineArgs.Length == 1) {
    //      win.Visibility = Visibility.Visible;
    //      windowsActivate.ActivateForm(win, null, IntPtr.Zero);
    //    } else {
    //      if (args.CommandLineArgs[1] == "/nw") {
    //        new MainWindow() { IsMultipleWindowsOpened = true }.Show();
    //      } else {
    //        IListItemEx sho;
    //        if (args.CommandLineArgs[1] == "t") {
    //          win.Visibility = Visibility.Visible;
    //          windowsActivate.ActivateForm(win, null, IntPtr.Zero);

    //          sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, startUpLocation.ToShellParsingName());
    //        } else {
    //          sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, args.CommandLineArgs[1].ToShellParsingName());
    //        }

    //        if (!IsStartMinimized || win.tcMain.Items.Count == 0) {
    //          this.CreateInitialTab(win, sho);
    //        } else if (Settings.BESettings.IsRestoreTabs) {
    //          win.tcMain.Items.Clear();
    //          this.CreateInitialTab(win, sho);
    //        } else if (args.CommandLineArgs.Length > 1 && args.CommandLineArgs[1] != null) {
    //          if (args.CommandLineArgs[1] == "t") {
    //            this.CreateInitialTab(win, sho);
    //          } else {
    //            var cmd = args.CommandLineArgs[1];
    //            sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, cmd.ToShellParsingName());
    //            this.CreateInitialTab(win, sho);
    //          }
    //        } else {
    //          this.CreateInitialTab(win, sho);
    //        }
    //      }

    //      windowsActivate.ActivateForm(win, null, IntPtr.Zero);
    //    }
    //  };
    //  this.Dispatcher.BeginInvoke(d, true);
    //}

    /// <summary>
    /// Rise on change of Main Window state
    /// </summary>
    /// <param name="sender">The main Window</param>
    /// <param name="e">The args</param>
    private void Win_StateChanged(Object sender, EventArgs e) {
      //if ((sender as Window)?.WindowState != WindowState.Minimized) {
      //  ((Window)sender).Visibility = Visibility.Visible;
      //  CombinedWindowActivator windowsActivate = new CombinedWindowActivator();
      //  windowsActivate.ActivateForm(sender as Window, null, IntPtr.Zero);
      //}
    }

    /// <summary>
    /// On mouse move over the tab
    /// </summary>
    /// <param name="sender">Who send it</param>
    /// <param name="e">The args</param>
    private void Newt_PreviewMouseMove(Object sender, MouseEventArgs e) {
      var tabItem = e.Source as Wpf.Controls.TabItem;

      if (tabItem != null && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) {
        DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
      }
    }

    public void OnInstanceInvoked(string[] args) {
      if (args == null || this.Dispatcher == null) {
        return;
      }

      var startUpLocation = Settings.BESettings.StartupLocation;

      Action<Boolean> d = x => {
        var win = this.MainWindow as MainWindow;
        var windowsActivate = new CombinedWindowActivator();
        if (!x || win == null) {
          return;
        }

        win.StateChanged += this.Win_StateChanged;
        if (!args.Any()) {
          return;
        }

        if (args.Length == 1) {
          win.Visibility = Visibility.Visible;
          windowsActivate.ActivateForm(win, null, IntPtr.Zero);
        } else {
          if (args[1] == "/nw") {
            new MainWindow() { IsMultipleWindowsOpened = true }.Show();
          } else {
            IListItemEx sho;
            if (args[1] == "t") {
              win.Visibility = Visibility.Visible;
              windowsActivate.ActivateForm(win, null, IntPtr.Zero);

              sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, startUpLocation.ToShellParsingName());
            } else {
              sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, args[1].ToShellParsingName());
            }

            if (!IsStartMinimized || win.tcMain.Items.Count == 0) {
              this.CreateInitialTab(win, sho);
            } else if (Settings.BESettings.IsRestoreTabs) {
              win.tcMain.Items.Clear();
              this.CreateInitialTab(win, sho);
            } else if (args.Length > 1 && args[1] != null) {
              if (args[1] == "t") {
                this.CreateInitialTab(win, sho);
              } else {
                var cmd = args[1];
                sho = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, cmd.ToShellParsingName());
                this.CreateInitialTab(win, sho);
              }
            } else {
              this.CreateInitialTab(win, sho);
            }
          }

          windowsActivate.ActivateForm(win, null, IntPtr.Zero);
        }
      };
      this.Dispatcher.BeginInvoke(d, true);
    }
  }
}
