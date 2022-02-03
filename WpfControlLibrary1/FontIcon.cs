using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterExplorerControls {
  public class FontIcon : Control {

    public static readonly DependencyProperty BaseIconGlyphProperty =
      DependencyProperty.Register(
        name: "BaseIconGlyph",
        propertyType: typeof(String),
        ownerType: typeof(FontIcon),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: String.Empty)
      );
    public static readonly DependencyProperty OverlayIconGlyphProperty =
      DependencyProperty.Register(
        name: "OverlayIconGlyph",
        propertyType: typeof(String),
        ownerType: typeof(FontIcon),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: String.Empty)
      );

    public String BaseIconGlyph {
      get => (String)GetValue(BaseIconGlyphProperty);
      set => SetValue(BaseIconGlyphProperty, value);
    }
    public String OverlayIconGlyph {
      get => (String)GetValue(OverlayIconGlyphProperty);
      set => SetValue(OverlayIconGlyphProperty, value);
    }
    static FontIcon() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(FontIcon), new FrameworkPropertyMetadata(typeof(FontIcon)));
    }
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
    }
  }
}
