using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BetterExplorerControls;

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
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: true)
      );
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
    public Spinner() {
      InitializeComponent();
      this.DataContext = this;
      this.Loaded += (sender, args) => {

      };
      //var myStoryboard = new Storyboard();
      //var myDoubleAnimation = new DoubleAnimation(0, 360, new Duration(new TimeSpan(0,0,0,1)));
      //myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
      //Storyboard.SetTargetName(myDoubleAnimation, "asArcSegment");
      //Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(ArcSegment.RotationAngleProperty));
      //myStoryboard.Children.Add(myDoubleAnimation);
      //myStoryboard.Begin(this);
    }
  }
}
