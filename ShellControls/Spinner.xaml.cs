using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShellControls {
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class Spinner : UserControl {
    public static readonly DependencyProperty IsWorkingProperty =
      DependencyProperty.Register(
        name: "IsWorking",
        propertyType: typeof(Boolean),
        ownerType: typeof(Spinner),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback:IsWorkingChangedCallback)
      );

    private static void IsWorkingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var spinner = (Spinner)d;
      if ((Boolean)e.NewValue == false) {
        spinner._SpinnerStoryboard.Stop(spinner.ptPath);
      } else {
        spinner._SpinnerStoryboard.Begin(spinner.ptPath, true);
      }
    }

    public static readonly DependencyProperty RotationAngleProperty =
      DependencyProperty.Register(
        name: "RotationAngle",
        propertyType: typeof(Double),
        ownerType: typeof(Spinner),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0D)
      );
    public bool IsWorking {
      get { return (bool)GetValue(IsWorkingProperty); }
      set { SetValue(IsWorkingProperty, value); }
    }
    public double RotationAngle {
      get { return (double)GetValue(RotationAngleProperty); }
      set { SetValue(RotationAngleProperty, value); }
    }
    private readonly Storyboard _SpinnerStoryboard = new();
    public Spinner() {
      InitializeComponent();
      this.DataContext = this;
      this.Loaded += (sender, args) => {
        var animation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1)) { RepeatBehavior = RepeatBehavior.Forever };
        Storyboard.SetTargetName(animation, "transform");
        Storyboard.SetTargetProperty(animation, new PropertyPath(RotateTransform.AngleProperty));

        // Create a storyboard to contain the animation.
        this._SpinnerStoryboard.Children.Add(animation);
        this._SpinnerStoryboard.Begin(this.ptPath, true);

      };
    }
  }
}
