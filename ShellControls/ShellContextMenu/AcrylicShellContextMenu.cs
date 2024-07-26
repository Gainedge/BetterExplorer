using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BetterExplorerControls;
using WPFUI.Common;

namespace ShellControls.ShellContextMenu {
  public class AcrylicShellContextMenu : AcrylicPopup {

    public static readonly DependencyProperty IsSimpleMenuProperty =
      DependencyProperty.Register(
        name: "IsSimpleMenu",
        propertyType: typeof(Boolean),
        ownerType: typeof(AcrylicPopup),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: false)
      );

    public Boolean IsSimpleMenu {
      get => (Boolean)GetValue(IsSimpleMenuProperty);
      set => SetValue(IsSimpleMenuProperty, value);
    }

    public ObservableCollection<FontIconButton> BaseItems { get; set; }
    public ObservableCollection<Win32ContextMenuItem> MenuItems { get; set; }

    public AcrylicShellContextMenu Parent { get; set; }

    public Double IconSpace {
      get {
        var isIconExists = this.MenuItems.Count(c => !String.IsNullOrEmpty(c.IconBase64) || c.IsChecked || c.Glyph != Icon.Empty || c.Icon != null) > 0;
        return isIconExists ? 20D : 0D;
      }
    }

    static AcrylicShellContextMenu() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicShellContextMenu), new FrameworkPropertyMetadata(typeof(AcrylicShellContextMenu)));
    }
    public AcrylicShellContextMenu() {
      //this.Height = 300;
      //this.MaxHeight = 460;
      this.BaseItems = new ObservableCollection<FontIconButton>();
      this.MenuItems = new ObservableCollection<Win32ContextMenuItem>();
      //this.MenuItems.CollectionChanged += this.MenuItems_CollectionChanged;
      this.DataContext = this;
    }

    //private void MenuItems_CollectionChanged(Object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
    //  var mousePos = AcrylicPopup.GetMousePosition();
    //  GetWindowRect(this.Handle, out var rect);
    //  this.IsPopupDirectionReversed = rect.Bottom <= (Int32)mousePos.Y;
    //}

    private Boolean CheckForChildren(AcrylicShellContextMenu source, AcrylicShellContextMenu menu) {
      if (menu == null) return false;
      if (menu.Handle == source.Handle) {
        return true;
      }
      foreach (var win32ContextMenuItem in source.MenuItems) {
        if (win32ContextMenuItem.SubMenuPopup != null) {
          if (this.CheckForChildren(win32ContextMenuItem.SubMenuPopup, win32ContextMenuItem.SubMenuPopup)) {
            return true;
          }
        }
      }

      return false;
    }
    protected override void ProcessMouseHookAction() {
      try {
        var el = Mouse.DirectlyOver;
        var assocPopup = (AcrylicShellContextMenu)this.IsElementPartOfPopup(el);
        if (el == null || !this.CheckForChildren(this, assocPopup)) {
          this.IsOpen = false;
        }
      } catch (Exception exception) {
        this.IsOpen = false;
      }
    }
  }
}
