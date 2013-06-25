/*
 * Developer : Ian Wright
 * Date : 31/05/2012
 * All code (c) Ian Wright. 
 */
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RateBar
{
	/// <summary>
	/// Axis Control
	/// </summary>
	public class Axis : ContentControl
	{
		#region Locals

		private Canvas canvas;

		private Brush brush;
		private double thickness;

		private bool horizontalGridVisible;
		private int horizontalGridLineCount;
		private bool verticalGridVisible;
		private int verticalGridLineCount;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Axis"/> class.
		/// </summary>
		public Axis()
		{
			#region Initialize Defaults

			this.IsHitTestVisible = false;
			this.canvas = new Canvas();
			this.canvas.ClipToBounds = true;
			this.Content = canvas;

			this.horizontalGridVisible = true;
			this.horizontalGridLineCount = 5;
			this.verticalGridVisible = true;
			this.verticalGridLineCount = 9;

			this.Thickness = 1;
			this.Brush = Brushes.LightGray;

			#endregion

			this.UpdateUI();
			
		}

		#region Properties

		/// <summary>
		/// Gets or sets a value indicating whether the horizontal grid is visible.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the horizontal grid is visible; otherwise, <c>false</c>.
		/// </value>
		public bool HorizontalGridVisible
		{
			get { return this.horizontalGridVisible; }
			set { this.horizontalGridVisible = value; }
		}

		/// <summary>
		/// Gets or sets the number of lines used in the horizontal grid.
		/// </summary>
		/// <value>
		/// The horizontal grid line count.
		/// </value>
		public int HorizontalGridLineCount
		{
			get { return this.horizontalGridLineCount; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("The HorizontalGridLineCount must be at least 1");

				this.horizontalGridLineCount = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of lines used in the vertical grid.
		/// </summary>
		/// <value>
		/// The vertical grid line count.
		/// </value>
		public int VerticalGridLineCount
		{
			get { return this.verticalGridLineCount; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("The VerticalGridLineCount must be at least 1");

				this.verticalGridLineCount = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the vertical grid is visible.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the vertical grid is visible; otherwise, <c>false</c>.
		/// </value>
		public bool VerticalGridVisible
		{
			get { return this.verticalGridVisible; }
			set
			{
				this.verticalGridVisible = value;
			}
		}

		/// <summary>
		/// Gets or sets the Brush used to render the lines.
		/// </summary>
		/// <value>
		/// The brush.
		/// </value>
		public Brush Brush
		{
			get { return this.brush; }
			set
			{
				this.brush = value;
			}
		}

		/// <summary>
		/// Gets or sets the thickness of the Brush used to render the lines.
		/// </summary>
		/// <value>
		/// The thickness.
		/// </value>
		public Double Thickness
		{
			get { return this.thickness; }
			set
			{
				this.thickness = value;
			}
		}

		#endregion

		/// <summary>
		/// Update the Rendering of the Control when the render size changes
		/// </summary>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			this.UpdateUI();
		}

		/// <summary>
		/// Render the control
		/// </summary>
		private void UpdateUI()
		{
			if (Double.IsNaN(this.Height) || Double.IsNaN(this.Width))
				return;
	
			this.canvas.Children.Clear();

			RenderHorizontalGrid();
			RenderVerticalGrid();
		}

		/// <summary>
		/// Render the Horizontal Grid
		/// </summary>
		protected virtual void RenderHorizontalGrid()
		{
			// We aren't displaying the horizontal grid
			if (this.HorizontalGridVisible == false)
				return;

			// Determine the relative height
			Double relativeHeight = this.RenderSize.Height / (double)this.HorizontalGridLineCount;

			// Render each line
			for (int i = 0; i <= this.HorizontalGridLineCount; i++)
			{
				Line line = new Line()
				{
					X1 = 0,
					X2 = this.Width,
					Y1 = (double)i * relativeHeight,
					Y2 = (double)i * relativeHeight,
					Stroke = this.Brush,
					StrokeThickness = this.Thickness
				};
				this.canvas.Children.Add(line);
			}
		}

		/// <summary>
		/// Render the Vertical Grid
		/// </summary>
		protected virtual void RenderVerticalGrid()
		{
			// We aren't displaying the horizontal grid
			if (this.VerticalGridVisible == false)
				return;

			// Determine the relative width
			Double relativeWidth = this.RenderSize.Width / (double)this.VerticalGridLineCount;

			// Render each line
			for (int i = 0; i <= this.VerticalGridLineCount; i++)
			{
				Line line = new Line()
				{
					X1 = (double)i * relativeWidth,
					X2 = (double)i * relativeWidth,
					Y1 = 0,
					Y2 = this.Height,
					Stroke = this.Brush,
					StrokeThickness = this.Thickness
				};
				this.canvas.Children.Add(line);
			}
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <inheritdoc />
		public void FirePropertyChangedEvent(object sender, PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(sender, e);
			}
		}

		#endregion
	}
}