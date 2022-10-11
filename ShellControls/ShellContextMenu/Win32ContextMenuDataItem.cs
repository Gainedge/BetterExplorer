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

namespace ShellControls.ShellContextMenu {
  public class Win32ContextMenuDataItem {
    public IntPtr hSubmenu { get; set; }
    public string IconBase64 { get; set; }
    public ImageSource Icon { get; set; }
    public int ID { get; set; } // Valid only in current menu to invoke item
    public string Label { get; set; }
    public string CommandString { get; set; }
    public MenuItemType Type { get; set; }
    public Boolean IsNewMenuItem { get; set; }
    public List<Win32ContextMenuDataItem>? SubItems { get; set; }
    public Boolean IsSubMenu => this.SubItems != null && this.SubItems.Any();

    public Boolean IsSeparator => this.Type == MenuItemType.MFT_SEPARATOR;
  }
}
