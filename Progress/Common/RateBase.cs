/*
 * Developer : Ian Wright
 * Date : 31/05/2012
 * All code (c) Ian Wright. 
 */
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace RateBar
{
	/// <summary>
	/// Represents a Progress Bar that also reports back a Rate
	/// </summary>
	public class RateBase : RangeBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RateBase"/> class.
		/// </summary>
		protected RateBase()
		{ }

		/// <summary>
		/// Initializes the <see cref="RateBase"/> class.
		/// </summary>
		static RateBase()
		{
			RateChangedEvent = EventManager.RegisterRoutedEvent("RateChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(RateBase));
			RateProperty = DependencyProperty.Register("Rate", typeof(double), typeof(RateBase), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(RateBase.OnRateChanged)), new ValidateValueCallback(RateBase.IsValidDoubleValue));
			CaptionProperty = DependencyProperty.Register("Caption", typeof(String), typeof(RateBase), new PropertyMetadata(String.Empty, new PropertyChangedCallback(OnCaptionChanged)));
			MinimumRateProperty = DependencyProperty.Register("RateMinimum", typeof(double), typeof(RateBase), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(RateBase.OnMinimumRateChanged)), new ValidateValueCallback(RateBase.IsValidDoubleValue));
			MaximumRateProperty = DependencyProperty.Register("RateMaximum", typeof(double), typeof(RateBase), new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(RateBase.OnMaximumRateChanged), new CoerceValueCallback(RateBase.CoerceMaximumRate)), new ValidateValueCallback(RateBase.IsValidDoubleValue));
		}

		#region DependencyProperties

		/// <summary>
		/// Identifies the RateChanged routed event.
		/// </summary>
		public static readonly RoutedEvent RateChangedEvent;

		/// <summary>
		/// Identifies the CaptionChanged routed event.
		/// </summary>
		public static readonly RoutedEvent CaptionChangedEvent;

		/// <summary>
		/// Identifies the Rate dependency property.
		/// </summary>
		public static readonly DependencyProperty RateProperty;

		/// <summary>
		/// Identifies the RateMinimumProperty dependency property.
		/// </summary>
		public static readonly DependencyProperty MinimumRateProperty;

		/// <summary>
		/// Identifies the RateMaximumProperty dependency property.
		/// </summary>
		public static readonly DependencyProperty MaximumRateProperty;

		/// <summary>
		/// Identifies the Caption dependency property.
		/// </summary>
		public static readonly DependencyProperty CaptionProperty;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the rate changes.
		/// </summary>
		public event RoutedPropertyChangedEventHandler<Double> RateChanged;

		/// <summary>
		/// Occurs when the rate caption changes.
		/// </summary>
		public event RoutedPropertyChangedEventHandler<String> CaptionChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the current rate of the rate control.
		/// </summary>
		[Category("Behaviour"), Bindable(true)]
		public Double Rate
		{
			get
			{
				return (Double)base.GetValue(RateProperty);
			}
			set
			{
				Rescale(value);
				base.SetValue(RateProperty, value);
			}
		}

		/// <summary>
		/// Provide a change for the RateMaximum to be modified for
		/// re-scaling any graphs etc.
		/// </summary>
		/// <param name="rate">The current rate</param>
		protected virtual void Rescale(Double rate)
		{
			// Attempt to keep a threshold of 80%, if the
			// rate increases above this threshold then
			// increase it. This prevents a graph that just 
			// gets completely full.
			if (rate > (this.RateMaximum * 0.8))
				this.RateMaximum = rate * 1.2;
		}

		/// <summary>
		/// Gets or sets the current minimum rate of the rate control.
		/// </summary>
		[Category("Behaviour"), Bindable(true)]
		public Double RateMinimum
		{
			get
			{
				return (Double)base.GetValue(MinimumRateProperty);
			}
			set
			{
				base.SetValue(MinimumRateProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the current maximum rate of the rate control.
		/// </summary>
		[Category("Behaviour"), Bindable(true)]
		public Double RateMaximum
		{
			get
			{
				return (Double)base.GetValue(MaximumRateProperty);
			}
			set
			{
				base.SetValue(MaximumRateProperty, value);
			}
		}


		/// <summary>
		/// Gets or sets the caption for the rate control
		/// </summary>
		[Category("Behaviour"), Bindable(true)]
		public String Caption
		{
			get
			{
				return (String)base.GetValue(CaptionProperty);
			}
			set
			{
				base.SetValue(CaptionProperty, value);
			}
		}

		#endregion
		
		#region Methods

		/// <summary>
		/// Called when caption changes.
		/// </summary>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
		protected virtual void OnCaptionChanged(String oldValue, String newValue)
		{
			RoutedPropertyChangedEventArgs<String> e = new RoutedPropertyChangedEventArgs<String>(oldValue, newValue);
			e.RoutedEvent = CaptionChangedEvent;
            if (e.RoutedEvent != null)
            {
                base.RaiseEvent(e);
            }
		}

		/// <summary>
		/// Coerces the maximum rate.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		private static object CoerceMaximumRate(DependencyObject d, object value)
		{
			RateBase element = (RateBase)d;
			double minimum = element.RateMinimum;
			if (((double)value) < minimum)
			{
				return minimum;
			}
			return value;
		}
 
		/// <summary>
		/// Called when the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum"/> property changes.
		/// </summary>
		/// <param name="oldMaximum">Old value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum"/> property.</param>
		/// <param name="newMaximum">New value of the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum"/> property.</param>
		protected virtual void OnMaximumRateChanged(double oldMaximum, double newMaximum)
		{
		}

		/// <summary>
		/// Called when [minimum rate changed].
		/// </summary>
		/// <param name="oldMinimum">The old minimum.</param>
		/// <param name="newMinimum">The new minimum.</param>
		protected virtual void OnMinimumRateChanged(double oldMinimum, double newMinimum)
		{
		}

		/// <summary>
		/// Called when the caption changes.
		/// </summary>
		/// <param name="d">The DependencyObject (RateBase).</param>
		/// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RateBase element = (RateBase)d;
			element.OnCaptionChanged((String)e.OldValue, (String)e.NewValue);
		}

		/// <summary>
		/// Called when maximum rate changes.
		/// </summary>
		/// <param name="d">The DependencyObject (RateBase).</param>
		/// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnMaximumRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RateBase element = (RateBase)d;
			element.CoerceValue(RateProperty);
			element.OnMaximumRateChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Called when minimum rate changes.
		/// </summary>
		/// <param name="d">The DependencyObject (RateBase).</param>
		/// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnMinimumRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RateBase element = (RateBase)d;
			element.CoerceValue(MaximumRateProperty);
			element.CoerceValue(RateProperty);
			element.OnMinimumRateChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Called when the rate changes.
		/// </summary>
		/// <param name="d">The DependencyObject (RateBase).</param>
		/// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RateBase element = (RateBase)d;
			element.OnRateChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Called when rate changes.
		/// </summary>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
		protected virtual void OnRateChanged(double oldValue, double newValue)
		{
			RoutedPropertyChangedEventArgs<double> e = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue);
			e.RoutedEvent = RateChangedEvent;
			base.RaiseEvent(e);
		}

		/// <summary>
		/// Determines whether the value is a valid double value.
		/// </summary>
		/// <param name="value">The value.</param>
		private static bool IsValidDoubleValue(object value)
		{
			double num = (double)value;
			return (!Double.IsNaN(num) && !double.IsInfinity(num));
		}

		#endregion
	}
}
