using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BetterExplorerControls;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;
using ShellLibrary.Interop;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Forms.Clipboard;
using DataFormats = System.Windows.Forms.DataFormats;
using Icon = WPFUI.Common.Icon;

namespace ShellControls.ShellContextMenu {
  public class ShellContextMenuEx {
    private IListItemEx[] SelectedItems;
    public IContextMenu _ContextMenuComInterface;
    public IContextMenu2 _ContextMenuComInterface2;
    public IContextMenu3 _ContextMenuComInterface3;
    private SVGIO _MenuMode;
    public ShellView ShellView;
    private IntPtr _ResultIntPtr = IntPtr.Zero;
    public ShellContextMenuEx(ShellView shellView) {
      this.ShellView = shellView;
      this.SelectedItems = shellView.SelectedItems.ToArray();
      if (this.SelectedItems.Any()) {
        IntPtr[] pidls = this.SelectedItems.Select(s => s.ILPidl).ToArray();
        IListItemEx parent = shellView.CurrentFolder;

        //for (int n = 0; n < this.SelectedItems.Length; ++n) {
        //  pidls[n] = this.SelectedItems[n].ILPidl;

        //  if (parent == null) {
        //    if (this.SelectedItems[n].ParsingName.Equals(ShellItem.Desktop.ParsingName)) {
        //      parent = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, ShellItem.Desktop.Pidl);
        //    } else {
        //      parent = this.SelectedItems[n].Parent;
        //    }
        //  } else if (!this.SelectedItems[n].Parent.Equals(parent)) {
        //    throw new Exception("All shell items must have the same parent");
        //  }
        //}


        parent.GetIShellFolder().GetUIObjectOf(this.ShellView.LVHandle, (uint)pidls.Length, pidls, typeof(IContextMenu).GUID, 0, out this._ResultIntPtr);
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

    public String[] _CommandsForRemove = new[] { "rename", "properties", "delete", "cut", "copy", "paste", "windows.modernshare", "view", "arrange",
                                                  "groupby", "windows.share", "refresh" };
    public async void ShowShellContextMenu(ShellView shellView, CMF additionalFlags) {
      //ShlWapi.SHSetThreadRef(this._ContextMenuComInterface3);
      this.ShellView = shellView;

      if (this._MenuMode != SVGIO.SVGIO_SELECTION) {
        Guid iise = typeof(IShellExtInit).GUID;
        IntPtr iShellExtInitPtr;
        if (Marshal.QueryInterface(this._ResultIntPtr, ref iise, out iShellExtInitPtr) == (int)HResult.S_OK) {
          var iShellExtInit =
            Marshal.GetTypedObjectForIUnknown(iShellExtInitPtr, typeof(IShellExtInit)) as IShellExtInit;

          try {
            iShellExtInit?.Initialize(this.ShellView.CurrentFolder.PIDL, null, 0);
            if (iShellExtInit != null)
              Marshal.ReleaseComObject(iShellExtInit);
            Marshal.Release(iShellExtInitPtr);
          } catch { }
        }
      }

      
      //var items = new List<Win32ContextMenuItem>();
      var cmControl = new AcrylicShellContextMenu();
      //var t = new Thread(() => {
      //cmControl.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {

      //}));
      //});
      //items.ToList().ForEach(e => e.Owner = cmControl);

      cmControl.IsSimpleMenu = this._MenuMode == SVGIO.SVGIO_BACKGROUND;

      cmControl.Placement = PlacementMode.MousePoint;
      
      if (this._MenuMode != SVGIO.SVGIO_BACKGROUND) {
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Cut", "\xF03D", "\xF03E", Icon.Empty, () => { this.ShellView.CutSelectedFiles(); cmControl.IsOpen = false; }));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Copy", "\xF021", "\xF022", Icon.Empty, () => { this.ShellView.CopySelectedFiles(); cmControl.IsOpen = false; }));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Paste", "\xF023", "\xF024", Icon.Empty, () => { this.ShellView.PasteAvailableFiles(); cmControl.IsOpen = false; }, true, Clipboard.GetDataObject()?.GetDataPresent(DataFormats.FileDrop) == true || Clipboard.GetDataObject()?.GetDataPresent("Shell IDList Array") == true));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Rename", "\xF027", "\xF028", Icon.Empty, () => { this.ShellView.RenameSelectedItem(); cmControl.IsOpen = false; }, this.SelectedItems.First().IsRenamable));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Share", "\xF025", "\xF026", Icon.Empty, () => { this.ShellView.OpenShareUI(); cmControl.IsOpen = false; }, this.SelectedItems.First().IsFolder == false));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Delete", "\xF035", "\xF036", Icon.Empty, () => { this.ShellView.DeleteSelectedFiles(Control.ModifierKeys != Keys.Shift); cmControl.IsOpen = false; }));
        cmControl.BaseItems.Add(this.CreateBaseCommandButton("Properties", "\xF031", "\xF032", Icon.Wrench16, () => { this.ShellView.ShowPropertiesPage(); cmControl.IsOpen = false; }));
      }
      var hMenu = User32.CreatePopupMenu();
      this._ContextMenuComInterface.QueryContextMenu(hMenu, 0, 1, int.MaxValue, CMF.EXPLORE | CMF.SYNCCASCADEMENU | CMF.CANRENAME | additionalFlags | ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? CMF.EXTENDEDVERBS : 0));
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
      //var t = new Thread(async () => {
        //System.Windows.Threading.Dispatcher.Run();
        //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (ThreadStart)(async () => {
          var res = await this.EnumMenuItems(hMenu, null);
      //}));


      //System.Windows.Threading.Dispatcher.Run();
      //res.ForEach(e => e.Owner = cmControl);
      //Thread.Sleep(1000);
      //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(async () => {
      foreach (var win32ContextMenuDataItem in res) {
        var newItem = new Win32ContextMenuItem(this, cmControl.MenuItems);
        newItem.ID = win32ContextMenuDataItem.ID;
        newItem.Owner = cmControl;
        newItem.Label = win32ContextMenuDataItem.Label;
        newItem.CommandString = win32ContextMenuDataItem.CommandString;
        newItem.Icon = win32ContextMenuDataItem.Icon;
        newItem.IconBase64 = win32ContextMenuDataItem.IconBase64;
        newItem.hSubmenu = win32ContextMenuDataItem.hSubmenu;
        newItem.IContextMenu = this._ContextMenuComInterface3;
        if (newItem.CommandString?.ToLowerInvariant() == "new") {
          newItem.Glyph = Icon.AddCircle16;
        }

        if (win32ContextMenuDataItem.IsSubMenu) {
          newItem.SubItems = win32ContextMenuDataItem.SubItems.Select(s => new Win32ContextMenuItem(this, newItem.SubItems)
            { ID = s.ID, Label = s.Label, CommandString = s.CommandString, IconBase64 = s.IconBase64, Type = s.Type, Icon = s.Icon}).ToList();
        }

        newItem.Type = win32ContextMenuDataItem.Type;
        cmControl.MenuItems.Add(newItem);
      }
      //}));
      //});
      //t.SetApartmentState(ApartmentState.STA);
      //t.Start();

    }

    private FontIconButton CreateBaseCommandButton(String tooltip, String baseIcon, String overlayIcon, Icon glyph, Action click, Boolean isVisible = true, Boolean isEnabled = true) {
      var btn = new FontIconButton();
      if (glyph != Icon.Empty) {
        btn.Glyph = glyph;
      } else {
        btn.BaseIconGlyph = baseIcon;
        btn.OverlayIconGlyph = overlayIcon;
      }

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

    public List<IntPtr> _BitmapHandles = new List<IntPtr>();
    public Task<List<Win32ContextMenuDataItem>> EnumMenuItems(IntPtr hMenu, Func<String, Boolean>? itemFilter, Boolean isNewItems = false) {
      return Task<List<Win32ContextMenuDataItem>>.Factory.StartNew((() => {
        //while (true) {
        //  Thread.Sleep(1);
        //  System.Windows.Forms.Application.DoEvents();
        //  return new List<Win32ContextMenuDataItem>();
        //}
        var menuItemsResult = new List<Win32ContextMenuDataItem>();
        var itemCount = User32.GetMenuItemCount(hMenu);
        var mii = new MENUITEMINFO();
        mii.cbSize = (uint)Marshal.SizeOf(mii);
        mii.fMask = MIIM.MIIM_BITMAP | MIIM.MIIM_FTYPE | MIIM.MIIM_STRING | MIIM.MIIM_ID | MIIM.MIIM_SUBMENU |
                    MIIM.MIIM_CHECKMARKS | MIIM.MIIM_DATA;
        for (uint ii = 0; ii < itemCount; ii++) {
          //Thread.Sleep(1);
          //System.Windows.Forms.Application.DoEvents();
          var menuItem = new Win32ContextMenuDataItem();
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
            menuItem.IsNewMenuItem = isNewItems;
            Marshal.FreeCoTaskMem(pszName);
            if (this._CommandsForRemove.Contains(menuItem.CommandString?.ToLowerInvariant())) {
              continue;
            }

            if (mii.hbmpItem != IntPtr.Zero && !Enum.IsDefined(typeof(HBITMAP_HMENU), ((IntPtr)mii.hbmpItem).ToInt64())) {
              //Gdi32.ConvertPixelByPixel(mii.hbmpItem, out var width, out var height);

              mii.hbmpItem = Gdi32.RenderHBitmap(mii.hbmpItem);

              Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, (Action)(async () => {
                if (mii.hbmpItem != IntPtr.Zero) {
                  var icon = Imaging.CreateBitmapSourceFromHBitmap(mii.hbmpItem, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                  icon.Freeze();
                  menuItem.Icon = icon;
                  Gdi32.DeleteObject(mii.hbmpItem);
                }
              }));
              if (mii.hbmpItem != IntPtr.Zero) {
                Gdi32.DeleteObject(mii.hbmpItem);
              }
              //using (var bitmap = Gdi32.GetBitmapFromHBitmap(mii.hbmpItem)) {
              //  if (bitmap != null) {
              //    byte[] bitmapData = (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[]));
              //    menuItem.IconBase64 = Convert.ToBase64String(bitmapData, 0, bitmapData.Length);
              //  }
              //}
            }

            if (mii.hSubMenu != IntPtr.Zero) {
              menuItem.hSubmenu = mii.hSubMenu;
              try {
                var ptr = IntPtr.Zero;
                //if (!menuItem.Label.ToLower().Contains("rar")) {
                this._ContextMenuComInterface3.HandleMenuMsg(279, (IntPtr)mii.hSubMenu, new IntPtr(ii));
                //}
              } catch (NotImplementedException) {
                // Only for dynamic/owner drawn? (open with, etc)
              }

              var itemsSub = EnumMenuItems(mii.hSubMenu, itemFilter, menuItem.CommandString?.ToLowerInvariant() == "new").Result;
              //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(async () => {
              menuItem.SubItems = itemsSub;
              //}));
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
            if (menuItem.IsSeparator &&
                (menuItemsResult.LastOrDefault()?.IsSeparator == true || menuItemsResult.Count == 0)) {
              continue;
            }

            //if (menuItem.CommandString?.ToLowerInvariant() == "new") {
            //  menuItem.Glyph = Icon.AddCircle16;
            //}

            menuItemsResult.Add(menuItem);
          }
          //System.Windows.Threading.Dispatcher.Run();
          //Application.Current.Dispatcher.Invoke(DispatcherPriority.Render,
          //  new Action(delegate { }));
        }

        if (menuItemsResult.ToArray().LastOrDefault()?.IsSeparator == true) {
          menuItemsResult.RemoveAt(menuItemsResult.Count - 1);
        }

        return menuItemsResult;
        //menuItemsResult.AddRange(menuItemResultList);
      }), CancellationToken.None
        , TaskCreationOptions.None
        , TaskScheduler.FromCurrentSynchronizationContext());
    }
  }
}
