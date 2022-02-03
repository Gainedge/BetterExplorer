using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using Microsoft.AspNetCore.Server.IIS.Core;
using ShellControls.ShellListView;
using TabControl = WPFUI.Controls.TabControl;

namespace ShellControls {
  [TemplatePart(Name = "PART_TabCloseButton", Type = typeof(Button))]
  public class ShellTabItem : TabItem {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(ShellTabItem), new UIPropertyMetadata(null));
    public static readonly DependencyProperty AllowDeleteProperty = DependencyProperty.Register("AllowDelete", typeof(bool), typeof(ShellTabItem), new UIPropertyMetadata(true));
    //public static readonly DependencyProperty IsSelectedTabProperty =
    //  DependencyProperty.Register("IsSelectedTab",
    //    typeof(bool),
    //    typeof(ShellTabItem),
    //    new FrameworkPropertyMetadata(
    //      false,
    //      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
    //      new PropertyChangedCallback(OnIsSelectedChanged)));

    //private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    //  var tabItem = (ShellTabItem)d;
    //  if ((bool)e.NewValue) {
    //    //var explorerBrowser = tabItem.Content as ExplorerControl;
    //    //explorerBrowser?.InitializeShellBrowser();
    //  }
    //}

    //public Boolean IsSelectedTab {
    //  get { return (Boolean)GetValue(IsSelectedTabProperty); }
    //  set { SetValue(IsSelectedTabProperty, value); }
    //}

    /// <summary>
    /// Provides a place to display an Icon on the Header and on the DropDown Context Menu
    /// </summary>
    public object Icon {
      get { return GetValue(IconProperty); }
      set { SetValue(IconProperty, value); }
    }

    /// <summary>
    /// Allow the Header to be Deleted by the end user
    /// </summary>
    public bool AllowDelete {
      get { return (bool)GetValue(AllowDeleteProperty); }
      set { SetValue(AllowDeleteProperty, value); }
    }

    public IListItemEx AssociatedItem { get; set; }
    public NavigationLog Log { get; set; }

    static ShellTabItem() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(ShellTabItem), new FrameworkPropertyMetadata(typeof(ShellTabItem)));
    }

    public ShellTabItem(ExplorerControl hostedControl, Boolean isSelected = false) {
      this.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
      this.Content = hostedControl;
      this.Log = new NavigationLog();
      hostedControl.Log = this.Log;
      hostedControl.AllowNavigation = isSelected;
      //hostedControl.OnBackClick += (sender, args) => {
      //  (sender as ShellView)?.Navigate_Full(this.Log.NavigateBack(), true);
      //};
      //hostedControl.OnForwardClick += (sender, args) => {
      //  (sender as ShellView)?.Navigate_Full(this.Log.NavigateForward(), true);
      //};
      hostedControl.OnUpdateTabInfo += (sender, args) => {
        this.AssociatedItem = args.Folder;
        this.Icon = args.Folder.ThumbnailSource(24, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
        this.Header = args.Folder.DisplayName;
      };

    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
      
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
      base.OnMouseLeftButtonUp(e);
      this.IsSelected = true;
      (this.Content as ExplorerControl)?.ShellViewEx.Focus();
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      //if (FindParent<WPFUI.Controls.Button>(e.OriginalSource as FrameworkElement) == null) {
      //  e.Handled = true;
      //  this.IsSelected = true;
      //}
    }

    public static T FindParent<T>(DependencyObject child) where T : DependencyObject {
      //get parent item
      DependencyObject parentObject = VisualTreeHelper.GetParent(child);

      //we've reached the end of the tree
      if (parentObject == null) return null;

      //check if the parent matches the type we're looking for
      T parent = parentObject as T;
      if (parent != null)
        return parent;
      else
        return FindParent<T>(parentObject);
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      var tabControl = this.Parent as TabControl;
      //if (tabControl != null) {
      //  //tabControl.SelectionChanged += (sender, args) => {
      //    if (this.IsSelectedTab) {
      //      var explorerBrowser = this.Content as ExplorerControl;
      //      explorerBrowser?.InitializeShellBrowser();
      //    }
      //  //};
      //}

      var closeButton = this.GetPart<WPFUI.Controls.Button>("PART_TabCloseButton");
      if (closeButton != null) {
        closeButton.PreviewMouseLeftButtonDown += (sender, args) => {
          args.Handled = true;
          tabControl?.Items.Remove(this);
        };
      }

    }
    internal T GetPart<T>(string name)
      where T : DependencyObject {
      return this.GetTemplateChild(name) as T;
    }
  }
}
