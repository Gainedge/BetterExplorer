using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShellControls {

  public class ToolbarGroup : ContentControl {
    public static readonly DependencyProperty GroupColorProperty =
      DependencyProperty.Register(
        name: "GroupColor",
        propertyType: typeof(System.Windows.Media.Brush),
        ownerType: typeof(ToolbarGroup),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Transparent));

    public System.Windows.Media.Brush GroupColor {
      get => (System.Windows.Media.Brush)GetValue(GroupColorProperty);
      set => SetValue(GroupColorProperty, value);
    }

    public static readonly DependencyProperty HaveSeparatorProperty =
      DependencyProperty.Register(
        name: "HaveSeparator",
        propertyType: typeof(Boolean),
        ownerType: typeof(ToolbarGroup),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: false));

    public Boolean HaveSeparator {
      get => (Boolean)GetValue(HaveSeparatorProperty);
      set => SetValue(HaveSeparatorProperty, value);
    }

    static ToolbarGroup() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarGroup),
        new FrameworkPropertyMetadata(typeof(ToolbarGroup)));
    }
  }
}
