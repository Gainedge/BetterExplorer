using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BetterExplorerControls {
  public class FontIconButton : Button {
    public AcrylicTooltip _Tooltip;
    public static readonly DependencyProperty BaseIconGlyphProperty =
      DependencyProperty.Register(
        name: "BaseIconGlyph",
        propertyType: typeof(String),
        ownerType: typeof(FontIconButton),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: String.Empty)
      );
    public static readonly DependencyProperty HeaderProperty =
      DependencyProperty.Register(
        name: "Header",
        propertyType: typeof(String),
        ownerType: typeof(FontIconButton),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: String.Empty)
      );
    public static readonly DependencyProperty OverlayIconGlyphProperty =
      DependencyProperty.Register(
        name: "OverlayIconGlyph",
        propertyType: typeof(String),
        ownerType: typeof(FontIconButton),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: String.Empty)
      );
    public static readonly DependencyProperty MouseIsOverProperty = DependencyProperty.Register(
      "MouseIsOver", typeof(bool), typeof(FontIconButton),
      new FrameworkPropertyMetadata(false, OnMouseIsOverChanged));

    public static readonly DependencyProperty IsCheckButtonProperty = DependencyProperty.Register(
      "IsCheckButton", typeof(bool), typeof(FontIconButton),
      new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
      "IsChecked", typeof(bool), typeof(FontIconButton),
      new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty ToolTipProperty =
      DependencyProperty.Register(
        name: "ToolTip",
        propertyType: typeof(String),
        ownerType: typeof(FontIconButton),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: String.Empty)
      );

    public static readonly DependencyProperty FlyoutProperty =
      DependencyProperty.Register(
        name: "Flyout",
        propertyType: typeof(Object),
        ownerType: typeof(FontIconButton),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: null)
      );
    public static readonly DependencyProperty GlyphProperty =
      DependencyProperty.Register(
        name: "Glyph",
        propertyType: typeof(WPFUI.Common.Icon),
        ownerType: typeof(FontIconButton),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: WPFUI.Common.Icon.Empty)
      );
    public WPFUI.Common.Icon Glyph {
      get => (WPFUI.Common.Icon)GetValue(GlyphProperty);
      set => SetValue(GlyphProperty, value);
    }
    public Object Flyout {
      get => GetValue(FlyoutProperty);
      set => SetValue(FlyoutProperty, value);
    }
    public String Header {
      get => (String)GetValue(HeaderProperty);
      set => SetValue(HeaderProperty, value);
    }
    public String BaseIconGlyph {
      get => (String)GetValue(BaseIconGlyphProperty);
      set => SetValue(BaseIconGlyphProperty, value);
    }
    public String OverlayIconGlyph {
      get => (String)GetValue(OverlayIconGlyphProperty);
      set => SetValue(OverlayIconGlyphProperty, value);
    }
    public Boolean MouseIsOver {
      get => (Boolean)GetValue(MouseIsOverProperty);
      set => SetValue(MouseIsOverProperty, value);
    }
    public Boolean IsCheckButton {
      get => (Boolean)GetValue(IsCheckButtonProperty);
      set => SetValue(IsCheckButtonProperty, value);
    }
    public Boolean IsChecked {
      get => (Boolean)GetValue(IsCheckedProperty);
      set => SetValue(IsCheckedProperty, value);
    }
    static FontIconButton() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FontIconButton), new FrameworkPropertyMetadata(typeof(FontIconButton)));
    }
    private static void OnMouseIsOverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var button = (FontIconButton)d;
      // handle code here. example:
      //if ((bool)e.NewValue == true) {
      //  if (button._Tooltip == null && !String.IsNullOrEmpty(button.ToolTip)) {
      //    button._Tooltip = new AcrylicTooltip();
      //    button._Tooltip.TooltipText = button.ToolTip;
      //    button._Tooltip.Placement = PlacementMode.Relative;
      //    button._Tooltip.LayoutUpdated += (sender, args) => {
      //      if (button._Tooltip != null) {
      //        button._Tooltip.VerticalOffset = -button._Tooltip.ActualHeight - 5;
      //        button._Tooltip.HorizontalOffset = -((button._Tooltip.ActualWidth - button.ActualWidth) / 2);
      //      }
      //    };
          
      //    button._Tooltip.PlacementTarget = button;
      //    //button._Tooltip.IsOpen = true;
      //  }
      //} else {
      //  if (button._Tooltip != null) {
      //    //button._Tooltip.IsOpen = false;
      //    button._Tooltip = null;
      //  }
      //}
    }

    protected override void OnClick() {
      base.OnClick();
      if (this.Flyout != null) {
        var popup = new AcrylicPopup();
        popup.Placement = PlacementMode.Custom;
        popup.PlacementTarget = this;
        popup.Content = this.Flyout;
        popup.IsOpen = true;
      }
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
      base.OnMouseEnter(e);
      
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
      base.OnMouseLeave(e);
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
    }
  }
}
