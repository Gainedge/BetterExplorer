using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using BetterExplorerControls;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;
using ShellLibrary.Interop;
using Clipboard = System.Windows.Forms.Clipboard;
using DataFormats = System.Windows.Forms.DataFormats;

namespace ShellControls.ShellContextMenu {
  public class ShellContextMenuEx {
    private IListItemEx[] SelectedItems;
    private IContextMenu _ContextMenuComInterface;
    private IContextMenu2 _ContextMenuComInterface2;
    private IContextMenu3 _ContextMenuComInterface3;
    private SVGIO _MenuMode;
    private ShellView _ShellView;
    private IntPtr _ResultIntPtr = IntPtr.Zero;
    public ShellContextMenuEx(ShellView shellView) {
      this._ShellView = shellView;
      this.SelectedItems = shellView.SelectedItems.ToArray();
      if (this.SelectedItems.Any()) {
        IntPtr[] pidls = new IntPtr[this.SelectedItems.Length];
        IListItemEx parent = null;

        for (int n = 0; n < this.SelectedItems.Length; ++n) {
          pidls[n] = this.SelectedItems[n].ILPidl;

          if (parent == null) {
            if (this.SelectedItems[n].ParsingName.Equals(ShellItem.Desktop.ParsingName)) {
              parent = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ShellItem.Desktop.Pidl);
            } else {
              parent = this.SelectedItems[n].Parent;
            }
          } else if (!this.SelectedItems[n].Parent.Equals(parent)) {
            throw new Exception("All shell items must have the same parent");
          }
        }

        parent.GetIShellFolder().GetUIObjectOf(this._ShellView.LVHandle, (uint)pidls.Length, pidls, typeof(IContextMenu).GUID, 0, out this._ResultIntPtr);
        this._ContextMenuComInterface = (IContextMenu)Marshal.GetTypedObjectForIUnknown(this._ResultIntPtr, typeof(IContextMenu));
        this._ContextMenuComInterface2 = (IContextMenu2)this._ContextMenuComInterface;
        this._ContextMenuComInterface3 = (IContextMenu3)this._ContextMenuComInterface;
        this._MenuMode = SVGIO.SVGIO_SELECTION;
      }
    }

    public ShellContextMenuEx(IListItemEx folder) {
      this.SelectedItems = new[] { folder };

      var ishellViewPtr = folder.GetIShellFolder().CreateViewObject(IntPtr.Zero, typeof(IShellView).GUID);
      var view = Marshal.GetObjectForIUnknown(ishellViewPtr) as IShellView;
      view?.GetItemObject(SVGIO.SVGIO_BACKGROUND, typeof(IContextMenu).GUID, out this._ResultIntPtr);
      if (view != null)
        Marshal.ReleaseComObject(view);
      this._ContextMenuComInterface = (IContextMenu)Marshal.GetTypedObjectForIUnknown(this._ResultIntPtr, typeof(IContextMenu));
      this._ContextMenuComInterface2 = (IContextMenu2)this._ContextMenuComInterface;
      this._ContextMenuComInterface3 = (IContextMenu3)this._ContextMenuComInterface;
      this._MenuMode = SVGIO.SVGIO_BACKGROUND;
    }

    private String[] _CommandsForRemove = new[] { "rename", "properties", "delete", "cut", "copy", "paste", "windows.modernshare", "view", "arrange", 
                                                  "groupby", "windows.share", "new", "refresh" };
    public void ShowShellContextMenu(ShellView shellView, CMF additionalFlags) {
      this._ShellView = shellView;

      if (this._MenuMode != SVGIO.SVGIO_SELECTION) {
        Guid iise = typeof(IShellExtInit).GUID;
        IntPtr iShellExtInitPtr;
        if (Marshal.QueryInterface(this._ResultIntPtr, ref iise, out iShellExtInitPtr) == (int)HResult.S_OK) {
          var iShellExtInit =
            Marshal.GetTypedObjectForIUnknown(iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;

          try {
            iShellExtInit?.Initialize(_ShellView.CurrentFolder.PIDL, null, 0);
            if (iShellExtInit != null)
              Marshal.ReleaseComObject(iShellExtInit);
            Marshal.Release(iShellExtInitPtr);
          } catch { }
        }
      }

      var hMenu = User32.CreatePopupMenu();
      this._ContextMenuComInterface.QueryContextMenu(hMenu, 0, 1, int.MaxValue, CMF.EXPLORE | CMF.SYNCCASCADEMENU | CMF.CANRENAME );
      var items = new List<Win32ContextMenuItem>();
      var cmControl = new AcrylicShellContextMenu();
      this.EnumMenuItems(hMenu, items, null);
      items.ToList().ForEach(e => e.Owner = cmControl);
      cmControl.IsSimpleMenu = this._MenuMode == SVGIO.SVGIO_BACKGROUND;
      cmControl.MenuItems = items.ToArray();
      cmControl.Placement = PlacementMode.MousePoint;
      if (this._MenuMode != SVGIO.SVGIO_BACKGROUND) {
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Cut", "\xF03D", "\xF03E", () => { this._ShellView.CutSelectedFiles(); cmControl.IsOpen = false; }));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Copy", "\xF021", "\xF022", () => { this._ShellView.CopySelectedFiles(); cmControl.IsOpen = false; }));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Paste", "\xF023", "\xF024", () => { this._ShellView.PasteAvailableFiles(); cmControl.IsOpen = false; }, true, Clipboard.GetDataObject()?.GetDataPresent(DataFormats.FileDrop) == true || Clipboard.GetDataObject()?.GetDataPresent("Shell IDList Array") == true));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Rename", "\xF027", "\xF028", () => { this._ShellView.RenameSelectedItem(); cmControl.IsOpen = false; }, this.SelectedItems.First().IsRenamable));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Share", "\xF025", "\xF026", () => { this._ShellView.OpenShareUI(); cmControl.IsOpen = false; }, this.SelectedItems.First().IsFolder == false));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Delete", "\xF035", "\xF036", () => { this._ShellView.DeleteSelectedFiles(Control.ModifierKeys != Keys.Shift); cmControl.IsOpen = false; }));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Properties", "\xF031", "\xF032", () => { this._ShellView.ShowPropertiesPage(); cmControl.IsOpen = false; }));
      }

      cmControl.OnMenuClosed += (sender, args) => {
        User32.DestroyMenu(hMenu);
        Marshal.FinalReleaseComObject(this._ContextMenuComInterface);
        Marshal.FinalReleaseComObject(this._ContextMenuComInterface2);
        Marshal.FinalReleaseComObject(this._ContextMenuComInterface3);
        this._BitmapHandles.ForEach(e => {
          Gdi32.DeleteObject(e);
        });
        this._BitmapHandles.Clear();
        GC.WaitForPendingFinalizers();
        GC.Collect();
      };
      cmControl.IsOpen = true;
    }

    private FontIconButton CreateBaseCommandButton(String tooltip, String baseIcon, String overlayIcon, Action click, Boolean isVisible = true, Boolean isEnabled = true) {
      var btn = new FontIconButton();
      btn.BaseIconGlyph = baseIcon;
      btn.OverlayIconGlyph = overlayIcon;
      btn.IsEnabled = isEnabled;
      var tooltipCtrl = new AcrylicTooltip();
      tooltipCtrl.Content = tooltip;
      tooltipCtrl.PlacementTarget = btn;
      btn.ToolTip = tooltipCtrl;
      btn.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
      btn.Click += (sender, args) => click();
      return btn;
    }

    public void InvokeItem(int itemID) {
      if (itemID < 0) {
        return;
      }

      try {
        //var currentWindows = User32.GetDesktopWindows();
        var pici = new Shell32.CMINVOKECOMMANDINFOEX();
        //pici.fMask = CMIC.Asyncok | CMIC.FlagNoUi | CMIC.Unicode | CMIC.PtInvoke;
        pici.lpVerb = Macros.MAKEINTRESOURCE(itemID);
        //pici.lpVerbW = Macros.MAKEINTRESOURCE(itemID);
        pici.nShow = User32.ShowWindowCommand.SW_SHOWNORMAL;
        pici.cbSize = (uint)Marshal.SizeOf(pici);
        var hr = this._ContextMenuComInterface.InvokeCommand(pici);
        //Win32API.BringToForeground(currentWindows);
      } catch (Exception ex) when (ex is COMException || ex is UnauthorizedAccessException) {
        Debug.WriteLine(ex);
      }
    }

    private List<IntPtr> _BitmapHandles = new List<IntPtr>();
    private void EnumMenuItems(IntPtr hMenu, List<Win32ContextMenuItem> menuItemsResult, Func<string, bool> itemFilter) {
      var itemCount = User32.GetMenuItemCount(hMenu);
      var mii = new MENUITEMINFO();
      mii.cbSize = (uint)Marshal.SizeOf(mii);
      mii.fMask = MIIM.MIIM_BITMAP | MIIM.MIIM_FTYPE | MIIM.MIIM_STRING | MIIM.MIIM_ID | MIIM.MIIM_SUBMENU | MIIM.MIIM_CHECKMARKS | MIIM.MIIM_DATA;
      for (uint ii = 0; ii < itemCount; ii++) {
        var menuItem = new Win32ContextMenuItem(this, menuItemsResult);
        mii.dwTypeData = Marshal.AllocCoTaskMem(512);
        mii.cch = 511; // https://devblogs.microsoft.com/oldnewthing/20040928-00/?p=37723
        var retval = User32.GetMenuItemInfo(hMenu, ii, true, ref mii);
        if (!retval) {
          //container.Dispose();
          continue;
        }
        menuItem.Type = (MenuItemType)mii.fType;
        menuItem.ID = (int)(mii.wID - 1); // wID - idCmdFirst
        //menuItem.Height = 50;
        //menuItem.Width = 200;
        //if (mii.hSubMenu == IntPtr.Zero) {
        //  menuItem.Click += (sender, args) => {
        //    this.InvokeItem(menuItem.ID);
        //  };
        //}

        if (menuItem.Type == MenuItemType.MFT_STRING) {
          menuItem.Label = mii.dwTypeData.ToString().Replace("&", String.Empty);
          mii.dwTypeData.FreeCotask();
          IntPtr pszName = Marshal.AllocCoTaskMem(512);
          this._ContextMenuComInterface3.GetCommandString((IntPtr)(mii.wID - 1), 0x00000004, IntPtr.Zero, pszName, 511);
          menuItem.CommandString = Marshal.PtrToStringAuto(pszName);
          Marshal.FreeCoTaskMem(pszName);
          if (itemFilter != null && (itemFilter(menuItem.CommandString) || itemFilter(menuItem.Label))) {
            // Skip items implemented in UWP
            //container.Dispose();
            continue;
          }
          if (mii.hbmpItem != IntPtr.Zero && !Enum.IsDefined(typeof(HBITMAP_HMENU), ((IntPtr)mii.hbmpItem).ToInt64())) {
            //Gdi32.ConvertPixelByPixel(mii.hbmpItem, out var width, out var height);
            //menuItem.Icon = Imaging.CreateBitmapSourceFromHIcon(mii.hbmpItem, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //Gdi32.DeleteObject(mii.hbmpItem);
            using (var bitmap = Gdi32.GetBitmapFromHBitmap(mii.hbmpItem)) {
              if (bitmap != null) {
                //using (System.IO.MemoryStream ms = new MemoryStream()) {
                //  bitmap.Save(ms, ImageFormat.Png);
                //  byte[] byteImage = ms.ToArray();
                //  menuItem.IconBase64 = Convert.ToBase64String(byteImage);
                //}
                byte[] bitmapData = (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[]));
                menuItem.IconBase64 = Convert.ToBase64String(bitmapData, 0, bitmapData.Length);
              }
            }
          }
          if (mii.hSubMenu != IntPtr.Zero) {
            menuItem.hSubmenu = mii.hSubMenu;
            var subItems = new List<Win32ContextMenuItem>();
            try {
              var ptr = IntPtr.Zero;
              this._ContextMenuComInterface3.HandleMenuMsg2(279, (IntPtr)mii.hSubMenu, IntPtr.Zero, out ptr);
            } catch (NotImplementedException) {
              // Only for dynamic/owner drawn? (open with, etc)
            }
            EnumMenuItems(mii.hSubMenu, subItems, itemFilter);
            menuItem.SubItems = subItems;
          } 

        }

        if (mii.hbmpItem != IntPtr.Zero) {
          this._BitmapHandles.Add(mii.hbmpItem);
        }
        if (mii.hbmpChecked != IntPtr.Zero) {
          this._BitmapHandles.Add(mii.hbmpChecked);
        }
        if (mii.hbmpUnchecked != IntPtr.Zero) {
          this._BitmapHandles.Add(mii.hbmpUnchecked);
        }
        if (mii.dwItemData != IntPtr.Zero) {
          this._BitmapHandles.Add(mii.dwItemData);
        }
        //container.Dispose();
        if (!this._CommandsForRemove.Contains(menuItem.CommandString?.ToLowerInvariant())) {
          if (menuItem.IsSeparator && (menuItemsResult.LastOrDefault()?.IsSeparator == true || menuItemsResult.Count == 0)) {
            continue;
          }
          menuItemsResult.Add(menuItem);
        }
      }

      if (menuItemsResult.ToArray().LastOrDefault()?.IsSeparator == true) {
        menuItemsResult.RemoveAt(menuItemsResult.Count - 1);
      }

    }
  }
}
