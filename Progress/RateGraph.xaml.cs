/*
 * Developer : Ian Wright
 * Date : 31/05/2012
 * All code (c) Ian Wright. 
 */
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RateBar {
  /// <summary>
  /// Interaction logic for RateGraph.xaml
  /// </summary>
  public partial class RateGraph : RateBase {
    /// <summary>
    /// The polygon of points
    /// </summary>
    private Polygon polygon;
    private Rectangle rectangle;
    private Axis axis;

    private bool isloaded = false;

    /// <summary>
    /// The list of points
    /// </summary>
    public ObservableCollection<Double[]> ratePoints = new ObservableCollection<Double[]>();

    /// <summary>
    /// Initializes a new instance of the <see cref="RateGraph"/> class.
    /// </summary>
    public RateGraph() {
      InitializeComponent();
      this.Template = (ControlTemplate)this.Resources["rateGraphTemplate"];

      // Find the polygon once loaded, it's points
      // will be modified as we go along
      this.Loaded += (o, e) => {
        this.polygon = this.Template.FindName("graph", this) as Polygon;
        this.rectangle = this.Template.FindName("graphBck", this) as Rectangle;
        this.axis = this.Template.FindName("rcAxis", this) as Axis;

        // Ensure we have the bottom left point of a graph to fill correctly
        // and another point which will be moved
        this.ratePoints.Add(new Double[] { 0, 0 });
        this.ratePoints.Add(new Double[] { 0, 0 });
        isloaded = true;
      };
    }

    public void SetBackGroundColor(Color brush) {
      this.polygon.Fill = new SolidColorBrush(brush);
      this.rectangle.Fill = new SolidColorBrush(Color.FromArgb(90, brush.R, brush.G, brush.B));
      this.axis.Brush = new SolidColorBrush(Color.FromArgb(120, brush.R, brush.G, brush.B));
      this.axis.UpdateUI();
    }

    protected override void OnValueChanged(double oldValue, double newValue) {
      if (isloaded) {
        //this.polygon = this.Template.FindName("graph", this) as Polygon;
        // Modify the Maximum if the Rate exceeds it
        if (this.Rate * 1.5 > this.RateMaximum)
          this.RateMaximum = this.Rate * 1.5;
        this.RateMaximum = Math.Max(this.RateMaximum, this.Rate * 1.5);

        // Move the existing point along the X-axis, this ensures our fill works correctly.
        if (this.ratePoints.Count > 0)
          this.ratePoints[0] = new Double[] { this.Value, 0 };
        else
          this.ratePoints.Add(new Double[] { this.Value, 0 });

        // Add on the new point
        this.ratePoints.Add(new Double[] { this.Value, this.Rate });


        this.polygon.Points = new PointCollection(this.ratePoints.Select(dba => {
          // Don't adjust the height for the line that runs alone the bottom
          return new Point(CalculateX(dba[0]), CalculateY(dba[1]));
        }).AsParallel());

        // Update the base rate
        base.OnValueChanged(oldValue, newValue);
      }

    }

    ///// <inheritdoc />
    //protected override void OnRateChanged(double oldValue, double newValue)
    //{
    //  if (isloaded)
    //  {
    //    // Modify the Maximum if the Rate exceeds it
    //    if (newValue * 1.2 > this.RateMaximum)
    //      this.RateMaximum = newValue * 1.2;

    //    // Move the existing point along the X-axis, this ensures our fill works correctly.
    //    if (this.ratePoints.Count > 0)
    //      this.ratePoints[0] = new Double[] { this.Value, 0 };
    //    else
    //      this.ratePoints.Add(new Double[] { this.Value, 0 });

    //    // Add on the new point
    //    this.ratePoints.Add(new Double[] { this.Value, newValue });


    //    this.polygon.Points = new PointCollection(this.ratePoints.Select(dba =>
    //      {
    //        // Don't adjust the height for the line that runs alone the bottom
    //        return new Point(CalculateX(dba[0]), CalculateY(dba[1]));
    //      }).AsParallel());

    //    // Update the base rate
    //    base.OnRateChanged(oldValue, newValue);
    //  }
    //}

    /// <summary>
    /// Returns the X position of a point on the graph based on the progress value
    /// </summary>
    /// <param name="progressValue">The progress value to calculate the X point for</param>
    private Double CalculateX(Double progressValue) {
      return (progressValue - 0.03) / this.Maximum * this.Width;
    }

    /// <summary>
    /// Returns the Y position of a point on the graph based on the rate value
    /// </summary>
    /// <param name="rateValue">The rate value to calculate the Y point for</param>
    private Double CalculateY(Double rateValue) {
      // Just return the height for 0 values to keep the graph on the baseline
      if (rateValue == this.RateMinimum)
        return this.Height;

      // The range that the graph is currently displaying
      Double range = this.RateMaximum - this.RateMinimum;
      return this.Height - ((this.Height / range) * rateValue);
    }
  }
}
