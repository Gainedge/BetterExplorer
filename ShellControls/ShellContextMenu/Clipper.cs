using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ShellControls.ShellContextMenu {
  public sealed class Clipper : Decorator {
    public static readonly DependencyProperty WidthFractionProperty = DependencyProperty.RegisterAttached("WidthFraction", typeof(double), typeof(Clipper), new PropertyMetadata(1d, OnClippingInvalidated), IsFraction);
    public static readonly DependencyProperty HeightFractionProperty = DependencyProperty.RegisterAttached("HeightFraction", typeof(double), typeof(Clipper), new PropertyMetadata(1d, OnClippingInvalidated), IsFraction);
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(Clipper), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ConstraintProperty = DependencyProperty.Register("Constraint", typeof(ConstraintSource), typeof(Clipper), new PropertyMetadata(ConstraintSource.WidthAndHeight, OnClippingInvalidated), IsValidConstraintSource);

    private Size _childSize;
    private DependencyPropertySubscriber _childVerticalAlignmentSubcriber;
    private DependencyPropertySubscriber _childHorizontalAlignmentSubscriber;

    public Clipper() {
      ClipToBounds = true;
    }

    public Brush Background {
      get { return (Brush)GetValue(BackgroundProperty); }
      set { SetValue(BackgroundProperty, value); }
    }

    public ConstraintSource Constraint {
      get { return (ConstraintSource)GetValue(ConstraintProperty); }
      set { SetValue(ConstraintProperty, value); }
    }

    [AttachedPropertyBrowsableForChildren]
    public static double GetWidthFraction(DependencyObject obj) {
      return (double)obj.GetValue(WidthFractionProperty);
    }

    public static void SetWidthFraction(DependencyObject obj, double value) {
      obj.SetValue(WidthFractionProperty, value);
    }

    [AttachedPropertyBrowsableForChildren]
    public static double GetHeightFraction(DependencyObject obj) {
      return (double)obj.GetValue(HeightFractionProperty);
    }

    public static void SetHeightFraction(DependencyObject obj, double value) {
      obj.SetValue(HeightFractionProperty, value);
    }

    protected override Size MeasureOverride(Size constraint) {
      if (Child is null) {
        return Size.Empty;
      }

      switch (Constraint) {
        case ConstraintSource.WidthAndHeight:
          Child.Measure(constraint);
          break;

        case ConstraintSource.Width:
          Child.Measure(new Size(constraint.Width, double.PositiveInfinity));
          break;

        case ConstraintSource.Height:
          Child.Measure(new Size(double.PositiveInfinity, constraint.Height));
          break;

        case ConstraintSource.Nothing:
          Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
          break;
      }

      var finalSize = Child.DesiredSize;
      if (Child is FrameworkElement childElement) {
        if (childElement.HorizontalAlignment == HorizontalAlignment.Stretch && constraint.Width > finalSize.Width && !double.IsInfinity(constraint.Width)) {
          finalSize.Width = constraint.Width;
        }

        if (childElement.VerticalAlignment == VerticalAlignment.Stretch && constraint.Height > finalSize.Height && !double.IsInfinity(constraint.Height)) {
          finalSize.Height = constraint.Height;
        }
      }

      _childSize = finalSize;

      finalSize.Width *= GetWidthFraction(Child);
      finalSize.Height *= GetHeightFraction(Child);

      return finalSize;
    }

    protected override Size ArrangeOverride(Size arrangeSize) {
      if (Child is null) {
        return Size.Empty;
      }

      var childSize = _childSize;
      var clipperSize = new Size(Math.Min(arrangeSize.Width, childSize.Width * GetWidthFraction(Child)),
                                 Math.Min(arrangeSize.Height, childSize.Height * GetHeightFraction(Child)));
      var offsetX = 0d;
      var offsetY = 0d;

      if (Child is FrameworkElement childElement) {
        if (childSize.Width > clipperSize.Width) {
          switch (childElement.HorizontalAlignment) {
            case HorizontalAlignment.Right:
              offsetX = -(childSize.Width - clipperSize.Width);
              break;

            case HorizontalAlignment.Center:
              offsetX = -(childSize.Width - clipperSize.Width) / 2;
              break;
          }
        }

        if (childSize.Height > clipperSize.Height) {
          switch (childElement.VerticalAlignment) {
            case VerticalAlignment.Bottom:
              offsetY = -(childSize.Height - clipperSize.Height);
              break;

            case VerticalAlignment.Center:
              offsetY = -(childSize.Height - clipperSize.Height) / 2;
              break;
          }
        }
      }

      Child.Arrange(new Rect(new Point(offsetX, offsetY), childSize));

      return clipperSize;
    }

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved) {
      void UpdateLayout(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (e.NewValue.Equals(HorizontalAlignment.Stretch) || e.NewValue.Equals(VerticalAlignment.Stretch)) {
          InvalidateMeasure();
        } else {
          InvalidateArrange();
        }
      }

      _childHorizontalAlignmentSubscriber?.Unsubscribe();
      _childVerticalAlignmentSubcriber?.Unsubscribe();

      if (visualAdded is FrameworkElement childElement) {
        _childHorizontalAlignmentSubscriber = new DependencyPropertySubscriber(childElement, HorizontalAlignmentProperty, UpdateLayout);
        _childVerticalAlignmentSubcriber = new DependencyPropertySubscriber(childElement, VerticalAlignmentProperty, UpdateLayout);
      }
    }

    protected override void OnRender(DrawingContext drawingContext) {
      base.OnRender(drawingContext);
      drawingContext.DrawRectangle(Background, null, new Rect(RenderSize));
    }

    private static bool IsFraction(object value) {
      var numericValue = (double)value;
      return numericValue >= 0d && numericValue <= 1d;
    }

    private static void OnClippingInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      if (d is UIElement element && VisualTreeHelper.GetParent(element) is Clipper translator) {
        translator.InvalidateMeasure();
      }
    }

    private static bool IsValidConstraintSource(object value) {
      return Enum.IsDefined(typeof(ConstraintSource), value);
    }
  }

  public enum ConstraintSource {
    WidthAndHeight,
    Width,
    Height,
    Nothing
  }

  public class DependencyPropertySubscriber : DependencyObject {
    private static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DependencyPropertySubscriber), new PropertyMetadata(null, ValueChanged));

    private readonly PropertyChangedCallback _handler;

    public DependencyPropertySubscriber(DependencyObject dependencyObject, DependencyProperty dependencyProperty, PropertyChangedCallback handler) {
      if (dependencyObject is null) {
        throw new ArgumentNullException(nameof(dependencyObject));
      }

      if (dependencyProperty is null) {
        throw new ArgumentNullException(nameof(dependencyProperty));
      }

      _handler = handler ?? throw new ArgumentNullException(nameof(handler));

      var binding = new Binding() { Path = new PropertyPath(dependencyProperty), Source = dependencyObject, Mode = BindingMode.OneWay };
      BindingOperations.SetBinding(this, ValueProperty, binding);
    }

    public void Unsubscribe() {
      BindingOperations.ClearBinding(this, ValueProperty);
    }

    private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      ((DependencyPropertySubscriber)d)._handler(d, e);
    }
  }
}
