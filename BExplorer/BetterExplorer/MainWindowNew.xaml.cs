using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using BExplorer.Shell.Interop;
using Fluent;
using Settings;
using ShellControls;
using ShellControls.ShellListView;
using ShellControls.ShellTreeView;
using WPFUI.Controls;
using TabControl = WPFUI.Controls.TabControl;

namespace BetterExplorer {
  /// <summary>
  /// Interaction logic for MainWindow2.xaml
  /// </summary>
  public partial class MainWindowNew : UIWindow {

    public MainWindowNew() {
      InitializeComponent();
      this.Loaded += OnLoaded;
      this.Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
      this.LoadInitialWindowPositionAndState();

      var initialTabs = BESettings.OpenedTabs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      if (initialTabs.Length == 0 || !BESettings.IsRestoreTabs) {
        var tbItemDef = new ShellTabItem(new ExplorerControl(false));
        tbItemDef.IsSelected = true;
        this.tcMain.Items.Add(tbItemDef);
      }
      if (!BESettings.IsRestoreTabs) {
        return;
      }

      var i = 0;
      foreach (var initialTab in initialTabs) {
        i++;
        var tbItem = new ShellTabItem(new ExplorerControl(initialTab, this.WindowState == WindowState.Normal), i == initialTabs.Length);
        tbItem.IsSelected = i == initialTabs.Length;
        this.tcMain.Items.Add(tbItem);
      }

    }

    private void OnClosing(object sender, CancelEventArgs e) {
      foreach (var shellTabItem in this.tcMain.Items.OfType<ShellTabItem>()) {
        (shellTabItem.Content as ExplorerControl)?.ShellViewEx.Dispose();//.KillAllThreads();
      }
      if (this.WindowState != WindowState.Minimized) {
        this.SaveSettings(String.Concat(from item in this.tcMain.Items.Cast<ShellTabItem>() select ";" + item.AssociatedItem.ParsingName));
      }
    }

    private void LoadInitialWindowPositionAndState() {
      if (BESettings.LastWindowState != 2) {
        this.Width = BESettings.LastWindowWidth;
        this.Height = BESettings.LastWindowHeight;
      }

      var location = new System.Drawing.Point();
      try {
        location = new System.Drawing.Point((int)BESettings.LastWindowPosLeft, (int)BESettings.LastWindowPosTop);
      } catch { }

      this.Left = location.X;
      this.Top = location.Y;

      switch (BESettings.LastWindowState) {
        case 2:
          this.IsLoadMaximize = true;
          this.WindowState = WindowState.Maximized;
          break;
        case 1:
          this.WindowState = WindowState.Minimized;
          break;
        case 0:
          this.WindowState = WindowState.Normal;
          break;
        default:
          this.WindowState = WindowState.Maximized;
          break;
      }

      //this.chkRibbonMinimizedGlass.IsChecked = BESettings.IsGlassOnRibonMinimized;
      //this.TheRibbon.IsMinimized = BESettings.IsRibonMinimized;

      ////CommandPrompt window size
      //this.CommandPromptWinHeight = BESettings.CmdWinHeight;
      //this.rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);

      //if (BESettings.IsConsoleShown) {
      //  this.rCommandPrompt.MinHeight = 100;
      //  this.rCommandPrompt.Height = new GridLength(this.CommandPromptWinHeight);
      //  this.spCommandPrompt.Height = GridLength.Auto;
      //} else {
      //  this.rCommandPrompt.MinHeight = 0;
      //  this.rCommandPrompt.Height = new GridLength(0);
      //  this.spCommandPrompt.Height = new GridLength(0);
    }

    private void SaveSettings(String openedTabs) {
      BESettings.LastWindowWidth = this.Width;
      BESettings.LastWindowHeight = this.Height;
      BESettings.LastWindowPosLeft = this.Left;
      BESettings.LastWindowPosTop = this.Top;

      //BESettings.CurrentTheme = this.btnTheme.IsChecked == true ? "Dark" : "Light";

      switch (this.WindowState) {
        case WindowState.Maximized:
          BESettings.LastWindowState = 2;
          break;
        case WindowState.Minimized:
          BESettings.LastWindowState = 1;
          break;
        case WindowState.Normal:
          BESettings.LastWindowState = 0;
          break;
        default:
          BESettings.LastWindowState = -1;
          break;
      }

      BESettings.OpenedTabs = openedTabs;
      BESettings.RTLMode = this.FlowDirection == FlowDirection.RightToLeft;
      //BESettings.ShowCheckboxes = this._ShellListView.ShowCheckboxes;
      //BESettings.TabBarAlignment = this.TabbaBottom.IsChecked == true ? "bottom" : "top";

      //if (BESettings.IsPreviewPaneEnabled)
      //  BESettings.PreviewPaneWidth = this.clPreview.ActualWidth;
      //if (BESettings.IsInfoPaneEnabled)
      //  BESettings.InfoPaneHeight = this.rPreviewPane.ActualHeight;
      //if (BESettings.IsConsoleShown)
      //  BESettings.CmdWinHeight = this.rCommandPrompt.ActualHeight;

      BESettings.SaveSettings();
    }


    bool isDragStarted = false;
    private void MainWindowNew_OnLoaded(object sender, RoutedEventArgs e) {


    }

    private void tabcontrol_DragEnter(object sender, DragEventArgs e) {
      if (e.Data.GetDataPresent(typeof(ShellTabItem))) {
        e.Effects = DragDropEffects.Move;
      }
    }

    private void tabcontrol_Drop(object sender, DragEventArgs e) {
      TabControl tabcontrol = sender as TabControl;
      ShellTabItem draggeditem = e.Data.GetData(typeof(ShellTabItem)) as ShellTabItem;
      var isDraggedItemSelected = draggeditem.IsSelected;
      Point droppedPoint = e.GetPosition(tabcontrol);
      GeneralTransform transform;
      int index = -1;
      for (int i = 0; i < tabcontrol.Items.Count; i++) {
        ShellTabItem item = tabcontrol.Items[i] as ShellTabItem;
        transform = item.TransformToVisual(tabcontrol);
        Rect rect = transform.TransformBounds(new Rect() { X = 0, Y = 0, Width = item.ActualWidth, Height = item.ActualHeight });
        if (rect.Contains(droppedPoint)) {
          if (!item.Equals(draggeditem)) {
            index = i;
          }
          break;
        }
      }
      if (index != -1) {
        tabcontrol.Items.Remove(draggeditem);
        tabcontrol.Items.Insert(index, draggeditem);
        if (isDraggedItemSelected) {
          tabcontrol.SelectedIndex = index;
        }
      }

      isDragStarted = false;
    }

    private void TcMain_OnMouseMove(object sender, MouseEventArgs e) {
      if (!(e.Source is ShellTabItem tabItem)) {
        return;
      }

      if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) {
        DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
      }
    }

    private void TcMain_OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
      if (Mouse.LeftButton == MouseButtonState.Released) {
        e.Action = DragAction.Cancel;
        isDragStarted = false;
      }
    }

    private void TcMain_OnPreviewMouseMove(object sender, MouseEventArgs e) {
      if (!(e.Source is ShellTabItem tabItem)) {
        return;
      }

      if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) {
        DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
      }
    }

    private void TcMain_OnDrop(object sender, DragEventArgs e) {
      if (e.Source is ShellTabItem tabItemTarget &&
          e.Data.GetData(typeof(ShellTabItem)) is ShellTabItem tabItemSource &&
          !tabItemTarget.Equals(tabItemSource) &&
          tabItemTarget.Parent is TabControl tabControl) {
        var currentlySelectedIndex = tabControl.SelectedIndex;
        int targetIndex = tabControl.Items.IndexOf(tabItemTarget);

        tabControl.Items.Remove(tabItemSource);
        tabControl.Items.Insert(targetIndex, tabItemSource);
        if (currentlySelectedIndex > targetIndex) {
          tabControl.SelectedIndex = currentlySelectedIndex + 1;
        } else {
          tabControl.SelectedIndex = currentlySelectedIndex;
        }
        //tabItemSource.IsSelected = true;
      }
    }

    private void TcMain_OnNewTab(object sender, RoutedEventArgs e) {
      var tbItem = new ShellTabItem(new ExplorerControl(true), true);
      tbItem.IsSelected = true;
      this.tcMain.Items.Add(tbItem);
    }
  }
}
