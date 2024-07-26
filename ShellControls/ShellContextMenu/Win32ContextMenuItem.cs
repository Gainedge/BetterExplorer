using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BExplorer.Shell.Interop;

namespace ShellControls.ShellContextMenu {
  public enum MenuItemType : uint {
    MFT_STRING = 0,
    MFT_BITMAP = 4,
    MFT_MENUBARBREAK = 32,
    MFT_MENUBREAK = 64,
    MFT_OWNERDRAW = 256,
    MFT_RADIOCHECK = 512,
    MFT_SEPARATOR = 2048,
    MFT_RIGHTORDER = 8192,
    MFT_RIGHTJUSTIFY = 16384
  }

  public enum HBITMAP_HMENU : long {
    HBMMENU_CALLBACK = -1,
    HBMMENU_MBAR_CLOSE = 5,
    HBMMENU_MBAR_CLOSE_D = 6,
    HBMMENU_MBAR_MINIMIZE = 3,
    HBMMENU_MBAR_MINIMIZE_D = 7,
    HBMMENU_MBAR_RESTORE = 2,
    HBMMENU_POPUP_CLOSE = 8,
    HBMMENU_POPUP_MAXIMIZE = 10,
    HBMMENU_POPUP_MINIMIZE = 11,
    HBMMENU_POPUP_RESTORE = 9,
    HBMMENU_SYSTEM = 1
  }

  public class Win32ContextMenuItem : Button {
    public IContextMenu3 IContextMenu { get; set; }
    public IntPtr hSubmenu { get; set; }
    public string IconBase64 { get; set; }
    public ImageSource Icon { get; set; }
    public int ID { get; set; } // Valid only in current menu to invoke item
    public string Label { get; set; }
    public string CommandString { get; set; }
    public MenuItemType Type { get; set; }
    public Boolean IsNewMenuItem { get; set; }
    public List<Win32ContextMenuItem> SubItems { get; set; } = new();

    public AcrylicShellContextMenu SubMenuPopup { get; set; }
    public DispatcherTimer IsOpenTimer = new DispatcherTimer(DispatcherPriority.Normal);

    public Boolean IsSubMenu => this.hSubmenu != IntPtr.Zero || this.SubItems.Any();

    public Boolean IsSeparator => this.Type == MenuItemType.MFT_SEPARATOR;

    public static readonly DependencyProperty IsChildMenuOpenedProperty =
      DependencyProperty.Register(
        name: "IsChildMenuOpened",
        propertyType: typeof(Boolean),
        ownerType: typeof(Win32ContextMenuItem),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: false)
      );

    public static readonly DependencyProperty IsCheckedProperty =
      DependencyProperty.Register(
        name: "IsChecked",
        propertyType: typeof(Boolean),
        ownerType: typeof(Win32ContextMenuItem),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: false)
      );
    public static readonly DependencyProperty GlyphProperty =
      DependencyProperty.Register(
        name: "Glyph",
        propertyType: typeof(WPFUI.Common.Icon),
        ownerType: typeof(Win32ContextMenuItem),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: WPFUI.Common.Icon.Empty)
      );
    public WPFUI.Common.Icon Glyph {
      get => (WPFUI.Common.Icon)GetValue(GlyphProperty);
      set => SetValue(GlyphProperty, value);
    }
    public Boolean IsChecked {
      get => (Boolean)GetValue(IsCheckedProperty);
      set => SetValue(IsCheckedProperty, value);
    }
    public Boolean IsChildMenuOpened {
      get => (Boolean)GetValue(IsChildMenuOpenedProperty);
      set => SetValue(IsChildMenuOpenedProperty, value);
    }

    public AcrylicShellContextMenu Owner { get; set; }
    private ShellContextMenuEx _ShellMenu;

    private IEnumerable<Win32ContextMenuItem> _ParentCollection;

    static Win32ContextMenuItem() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(Win32ContextMenuItem), new FrameworkPropertyMetadata(typeof(Win32ContextMenuItem)));
    }

    public Win32ContextMenuItem(IEnumerable<Win32ContextMenuItem> parentCollection) {
      this.hSubmenu = IntPtr.Zero;
      this.IsOpenTimer.Interval = TimeSpan.FromMilliseconds(500);
      this.IsOpenTimer.Tick += IsOpenTimerOnTick;
      this._ParentCollection = parentCollection;
      //this.SubItems = new RangeObservableCollection<Win32ContextMenuItem>();
      this.DataContext = this;
    }
    public Win32ContextMenuItem(ShellContextMenuEx menu, IEnumerable<Win32ContextMenuItem> parentCollection) {
      this._ShellMenu = menu;
      this.hSubmenu = IntPtr.Zero;
      this.IsOpenTimer.Interval = TimeSpan.FromMilliseconds(500);
      this.IsOpenTimer.Tick += IsOpenTimerOnTick;
      this._ParentCollection = parentCollection;
      //this.SubItems = new RangeObservableCollection<Win32ContextMenuItem>();
      this.Click += (sender, args) => {
        if (!this.IsSubMenu) {
          this.CloseMenuRecursive();
          this._ShellMenu.ShellView.IsRenameNeeded = this.IsNewMenuItem;
          this._ShellMenu.InvokeItem(this.ID);
        }
      };

      this.DataContext = this;
    }

    private void IsOpenTimerOnTick(object? sender, EventArgs e) {
      if (this.SubMenuPopup == null) {
        this.SubMenuPopup = new AcrylicShellContextMenu();
        this.SubMenuPopup.Parent = this.Owner;
      }
      this.SubItems.ToList().ForEach(e => {
        e.Owner = this.SubMenuPopup;
        this.SubMenuPopup.MenuItems.Add(e);
      });
      this.SubMenuPopup.Placement = PlacementMode.Right;
      this.SubMenuPopup.IsSimpleMenu = true;
      this.SubMenuPopup.PlacementTarget = this;
      this.SubMenuPopup.IsOpen = true;
      this.IsChildMenuOpened = true;
      this.IsOpenTimer.Stop();
    }

    private void CloseMenuRecursive() {
      var currentMenu = this.Owner;
      while (currentMenu.Parent != null) {
        currentMenu = currentMenu.Parent;
        currentMenu.IsOpen = false;
      }

      this.Owner.IsOpen = false;
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
      base.OnMouseEnter(e);
      if (this.IsSeparator) {
        return;
      }

      if (this._ParentCollection != null) {
        this._ParentCollection.Where(w => w != this).ToList()
          .ForEach(e => {
            if (e.SubMenuPopup != null) {
              e.SubMenuPopup.IsOpen = false;
              e.IsChildMenuOpened = false;
              e.SubMenuPopup = null;
            }

            if (e.IsOpenTimer.IsEnabled) {
              e.IsOpenTimer.Stop();
            }
          });
      }

      if (this.IsSubMenu) {
        this.IsOpenTimer.Start();
      }
    }
  }
}
