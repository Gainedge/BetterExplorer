using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShellControls.BreadcrumbBar {

  public class AeroChrome : ContentControl {
    static AeroChrome() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(AeroChrome), new FrameworkPropertyMetadata(typeof(AeroChrome)));
    }


    public bool RenderPressed {
      get { return (bool)this.GetValue(RenderPressedProperty); }
      set { this.SetValue(RenderPressedProperty, value); }
    }

    public bool IsLeft {
      get { return (bool)this.GetValue(IsLeftProperty); }
      set { this.SetValue(IsLeftProperty, value); }
    }

    public static readonly DependencyProperty RenderPressedProperty =
        DependencyProperty.Register("RenderPressed", typeof(bool), typeof(AeroChrome), new UIPropertyMetadata(false));

    public static readonly DependencyProperty IsLeftProperty =
      DependencyProperty.Register("IsLeft", typeof(bool), typeof(AeroChrome), new UIPropertyMetadata(false));


    public bool RenderMouseOver {
      get { return (bool)this.GetValue(RenderMouseOverProperty); }
      set { this.SetValue(RenderMouseOverProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RenderMouseOver.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RenderMouseOverProperty =
        DependencyProperty.Register("RenderMouseOver", typeof(bool), typeof(AeroChrome), new UIPropertyMetadata(false));



    public Brush MouseOverBackground {
      get { return (Brush)this.GetValue(MouseOverBackgroundProperty); }
      set { this.SetValue(MouseOverBackgroundProperty, value); }
    }

    public static readonly DependencyProperty MouseOverBackgroundProperty =
        DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(AeroChrome), new UIPropertyMetadata(null));



    public Brush MousePressedBackground {
      get { return (Brush)this.GetValue(MousePressedBackgroundProperty); }
      set { this.SetValue(MousePressedBackgroundProperty, value); }
    }

    // Using a DependencyProperty as the backing store for MousePressedBackground.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MousePressedBackgroundProperty =
        DependencyProperty.Register("MousePressedBackground", typeof(Brush), typeof(AeroChrome), new UIPropertyMetadata(null));


  }
}
