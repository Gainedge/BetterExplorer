//-----------------------------------------------------------------------
// <copyright file="ToggleSwitchBase.cs">
// (c) 2011 Eric Jensen. All rights reserved.
// This source is subject to the Microsoft Public License.
// See http://www.opensource.org/licenses/MS-PL.
// </copyright>
// <date>15-Sept-2011</date>
// <author>Eric Jensen</author>
// <summary>Base class for the toggle switch control.</summary>
//-----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ToggleSwitch.Utils;

namespace ToggleSwitch
{
	///<summary>
	/// Base class for the toggle switch control.
	///</summary>
	[TemplateVisualState(Name = NormalState, GroupName = CommonStates)]
	[TemplateVisualState(Name = DisabledState, GroupName = CommonStates)]
	[TemplateVisualState(Name = MouseOverState, GroupName = CommonStates)]
	[TemplateVisualState(Name = FocusedState, GroupName = FocusStates)]
	[TemplateVisualState(Name = UnfocusedState, GroupName = FocusStates)]
	[TemplateVisualState(Name = CheckedState, GroupName = CheckStates)]
	[TemplateVisualState(Name = UncheckedState, GroupName = CheckStates)]
	[TemplateVisualState(Name = DraggingState + CheckedState, GroupName = CheckStates)]
	[TemplateVisualState(Name = DraggingState + UncheckedState, GroupName = CheckStates)]
	[TemplatePart(Name = SwitchCheckedPart, Type = typeof(Control))]
	[TemplatePart(Name = SwitchUncheckedPart, Type = typeof(Control))]
	[TemplatePart(Name = SwitchThumbPart, Type = typeof(Thumb))]
	[TemplatePart(Name = SwitchRootPart, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = SwitchTrackPart, Type = typeof(FrameworkElement))]
	[Description("A control which when clicked or dragged toggles between on and off states.")]
	public abstract class ToggleSwitchBase : Control
	{
		#region Constants

		private const string CommonStates = "CommonStates";
		private const string NormalState = "Normal";
		private const string DisabledState = "Disabled";
		private const string MouseOverState = "MouseOver";

		private const string CheckStates = "CheckStates";
		private const string CheckedState = "Checked";
		private const string DraggingState = "Dragging";
		private const string UncheckedState = "Unchecked";

		private const string FocusStates = "FocusStates";
		private const string FocusedState = "Focused";
		private const string UnfocusedState = "Unfocused";

		private const string SwitchRootPart = "SwitchRoot";
		private const string SwitchCheckedPart = "SwitchChecked";
		private const string SwitchUncheckedPart = "SwitchUnchecked";
		private const string SwitchThumbPart = "SwitchThumb";
		private const string SwitchTrackPart = "SwitchTrack";

		private const string CommonPropertiesCategory = "Common Properties";
		private const string AppearanceCategory = "Appearance";

		#endregion

		#region Fields

		/// <summary>
		/// True if the mouse has been captured by this control, false otherwise.
		/// </summary> 
		private bool _isMouseCaptured;

		/// <summary> 
		/// True if the SPACE key is currently pressed, false otherwise. 
		/// </summary>
		private bool _isSpaceKeyDown;

		/// <summary>
		/// True if the mouse's left button is currently down, false otherwise. 
		/// </summary>
		private bool _isMouseLeftButtonDown;

		/// <summary> 
		/// Last known position of the mouse with respect to this Button.
		/// </summary> 
		private Point _mousePosition;

		/// <summary>
		/// True if visual state changes are suspended; false otherwise. 
		/// </summary>
		private bool _suspendStateChanges;

		#endregion

		#region Properties

		protected Thumb SwitchThumb { get; set; }
		protected Control SwitchChecked { get; set; }
		protected Control SwitchUnchecked { get; set; }
		protected FrameworkElement SwitchRoot { get; set; }
		protected FrameworkElement SwitchTrack { get; set; }

		/// <summary>
		/// The current offset of the Thumb.
		/// </summary>
		protected abstract double Offset { get; set; }

		/// <summary>
		/// The current offset of the Thumb when it's in the Checked state.
		/// </summary>
		protected double CheckedOffset { get; set; }

		/// <summary>
		/// The current offset of the Thumb when it's in the Unchecked state.
		/// </summary>
		protected double UncheckedOffset { get; set; }

		/// <summary>
		/// The offset of the thumb while it's being dragged.
		/// </summary>
		protected double DragOffset { get; set; }

		/// <summary>
		/// Gets or sets whether the thumb position is being manipulated.
		/// </summary>
		protected bool IsDragging { get; set; }

		/// <summary> 
		/// Gets a value that indicates whether a ToggleSwitch is currently pressed.
		/// </summary> 
		public bool IsPressed { get; protected set; }

		protected abstract PropertyPath SlidePropertyPath { get; }

		#endregion

		#region Dependency Properties

#if SILVERLIGHT

		#region IsFocused

		/// <summary>
		/// Gets a value that determines whether this element has logical focus. 
		/// </summary>
		/// <remarks>
		/// IsFocused will not be set until OnFocus has been called.  It may not 
		/// yet have been set if you check it in your own Focus event handler. 
		/// </remarks>
		public bool IsFocused
		{
			get { return (bool)GetValue(IsFocusedProperty); }
			protected set { SetValue(IsFocusedProperty, value); }
		}

		/// <summary> 
		/// Identifies the IsFocused dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsFocusedProperty =
			DependencyProperty.Register("IsFocused", typeof(bool), typeof(ToggleSwitchBase), null);

		#endregion IsFocused

		#region IsMouseOver

		/// <summary> 
		/// Gets a value indicating whether the mouse pointer is located over
		/// this element.
		/// </summary> 
		/// <remarks>
		/// IsMouseOver will not be set until MouseEnter has been called.  It
		/// may not yet have been set if you check it in your own MouseEnter 
		/// event handler. 
		/// </remarks>
		public bool IsMouseOver
		{
			get { return (bool)GetValue(IsMouseOverProperty); }
			protected set { SetValue(IsMouseOverProperty, value); }
		}

		/// <summary> 
		/// Identifies the IsMouseOver dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsMouseOverProperty =
			DependencyProperty.Register("IsMouseOver", typeof(bool), typeof(ToggleSwitchBase), null);

		#endregion

#endif

		#region ContentTemplate (DependencyProperty)

		///<summary>
		/// DependencyProperty for the <see cref="ControlTemplate">ControlTemplate</see> property.
		///</summary>
		public static readonly DependencyProperty ContentTemplateProperty =
			DependencyProperty.Register("ContentTemplate", typeof(ControlTemplate), typeof(ToggleSwitchBase),
#if SILVERLIGHT
			new PropertyMetadata(OnLayoutDependancyPropertyChanged));
#else
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, OnLayoutDependancyPropertyChanged));
#endif

		///<summary>
		/// The template applied to the <see cref="CheckedContent">Checked</see> and <see cref="UncheckedContent">Unchecked</see> content properties.
		///</summary>
		[Description("The template applied to the Checked and Unchecked content properties.")]
		public ControlTemplate ContentTemplate
		{
			get { return (ControlTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

		#endregion

		#region CheckedContent (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="CheckedContent">CheckedContent</see> property.
		///</summary>
		public static readonly DependencyProperty CheckedContentProperty =
			DependencyProperty.Register("CheckedContent", typeof(object), typeof(ToggleSwitchBase),
#if SILVERLIGHT
			new PropertyMetadata("ON", OnLayoutDependancyPropertyChanged));
#else
			new FrameworkPropertyMetadata("ON", FrameworkPropertyMetadataOptions.AffectsArrange, OnLayoutDependancyPropertyChanged));
#endif

		///<summary>
		/// The content shown on the checked side of the toggle switch
		///</summary>
		[Category(CommonPropertiesCategory)]
		[Description("The content shown on the checked side of the toggle switch")]
		public object CheckedContent
		{
			get { return GetValue(CheckedContentProperty); }
			set { SetValue(CheckedContentProperty, value); }
		}

		#endregion

		#region CheckedForeground (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="CheckedForeground">CheckedForeground</see> property.
		///</summary>
		public static readonly DependencyProperty CheckedForegroundProperty =
			DependencyProperty.Register("CheckedForeground", typeof(Brush), typeof(ToggleSwitchBase), null);

		///<summary>
		/// The brush used for the foreground of the checked side of the toggle switch.
		///</summary>
		[Description("The brush used for the foreground of the checked side of the toggle switch.")]
		public Brush CheckedForeground
		{
			get { return (Brush)GetValue(CheckedForegroundProperty); }
			set { SetValue(CheckedForegroundProperty, value); }
		}

		#endregion

		#region CheckedBackground (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="CheckedBackground">CheckedBackground</see> property.
		///</summary>
		public static readonly DependencyProperty CheckedBackgroundProperty =
			DependencyProperty.Register("CheckedBackground", typeof(Brush), typeof(ToggleSwitchBase), null);

		///<summary>
		/// The brush used for the background of the checked side of the toggle switch.
		///</summary>
		[Description("The brush used for the background of the checked side of the toggle switch.")]
		public Brush CheckedBackground
		{
			get { return (Brush)GetValue(CheckedBackgroundProperty); }
			set { SetValue(CheckedBackgroundProperty, value); }
		}

		#endregion

		#region UncheckedContent (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="UncheckedContent">UncheckedContent</see> property.
		///</summary>
		public static readonly DependencyProperty UncheckedContentProperty =
			DependencyProperty.Register("UncheckedContent", typeof(object), typeof(ToggleSwitchBase),
#if SILVERLIGHT
			new PropertyMetadata("OFF", OnLayoutDependancyPropertyChanged));
#else
			new FrameworkPropertyMetadata("OFF", FrameworkPropertyMetadataOptions.AffectsArrange, OnLayoutDependancyPropertyChanged));
#endif

		///<summary>
		/// The content shown on the unchecked side of the toggle switch.
		///</summary>
		[Category(CommonPropertiesCategory)]
		[Description("The content shown on the unchecked side of the toggle switch.")]
		public object UncheckedContent
		{
			get { return GetValue(UncheckedContentProperty); }
			set { SetValue(UncheckedContentProperty, value); }
		}

		#endregion

		#region UncheckedForeground (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="UncheckedForeground">UncheckedForeground</see> property.
		///</summary>
		public static readonly DependencyProperty UncheckedForegroundProperty =
			DependencyProperty.Register("UncheckedForeground", typeof(Brush), typeof(ToggleSwitchBase), null);

		///<summary>
		/// The brush used for the foreground of the Unchecked side of the toggle switch.
		///</summary>
		[Description("The brush used for the foreground of the Unchecked side of the toggle switch.")]
		public Brush UncheckedForeground
		{
			get { return (Brush)GetValue(UncheckedForegroundProperty); }
			set { SetValue(UncheckedForegroundProperty, value); }
		}

		#endregion

		#region UncheckedBackground (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="UncheckedBackground">UncheckedBackground</see> property.
		///</summary>
		public static readonly DependencyProperty UncheckedBackgroundProperty =
			DependencyProperty.Register("UncheckedBackground", typeof(Brush), typeof(ToggleSwitchBase), null);

		///<summary>
		/// The brush used for the background of the Unchecked side of the toggle switch.
		///</summary>
		[Description("The brush used for the background of the Unchecked side of the toggle switch.")]
		public Brush UncheckedBackground
		{
			get { return (Brush)GetValue(UncheckedBackgroundProperty); }
			set { SetValue(UncheckedBackgroundProperty, value); }
		}

		#endregion

		#region Elasticity (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="Elasticity">Elasticity</see> property.
		///</summary>
		public static readonly DependencyProperty ElasticityProperty =
			DependencyProperty.Register("Elasticity", typeof(double), typeof(ToggleSwitchBase), new PropertyMetadata(0.5));

		///<summary>
		/// Determines the percentage of the way the <see cref="Thumb">thumb</see> must be dragged before the switch changes it's <see cref="IsChecked">IsChecked</see> state.
		///</summary>
		///<remarks>
		/// This value must be within the range of 0.0 - 1.0. 
		///</remarks>
		[Category(CommonPropertiesCategory)]
		[Description("Determines the percentage of the way the thumb must be dragged before the switch changes it's IsChecked state.")]
		public double Elasticity
		{
			get { return ((double)GetValue(ElasticityProperty)).Clamp(0.0, 1.0); }
			set { SetValue(ElasticityProperty, value.Clamp(0, 1.0)); }
		}

		#endregion

		#region ThumbTemplate (DependencyProperty)

		///<summary>
		/// DependencyProperty for the <see cref="ThumbTemplate">ThumbTemplate</see> property.
		///</summary>
		public static readonly DependencyProperty ThumbTemplateProperty =
			DependencyProperty.Register("ThumbTemplate", typeof(ControlTemplate), typeof(ToggleSwitchBase),
#if SILVERLIGHT
			new PropertyMetadata(OnLayoutDependancyPropertyChanged));
#else
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, OnLayoutDependancyPropertyChanged));
#endif

		///<summary>
		/// The <see cref="Thumb">thumb's</see> control template.
		///</summary>
		[Description("The thumb's control template.")]
		public ControlTemplate ThumbTemplate
		{
			get { return (ControlTemplate)GetValue(ThumbTemplateProperty); }
			set { SetValue(ThumbTemplateProperty, value); }
		}

		#endregion

		#region ThumbBrush (Dependency Property)

		///<summary>
		/// DependencyProperty for the <see cref="ThumbBrush">ThumbBrush</see> property.
		///</summary>
		public static readonly DependencyProperty ThumbBrushProperty =
			DependencyProperty.Register("ThumbBrush", typeof(Brush), typeof(ToggleSwitchBase), null);

		///<summary>
		/// The brush used to fill the <see cref="Thumb">thumb</see>.
		///</summary>
		[Description("The brush used to fill the thumb.")]
		public Brush ThumbBrush
		{
			get { return (Brush)GetValue(ThumbBrushProperty); }
			set { SetValue(ThumbBrushProperty, value); }
		}

		#endregion

		#region ThumbSize (DependencyProperty)

		///<summary>
		/// DependencyProperty for the <see cref="ThumbSize">ThumbSize</see> property.
		///</summary>
		public static readonly DependencyProperty ThumbSizeProperty =
			DependencyProperty.Register("ThumbSize", typeof(double), typeof(ToggleSwitchBase),
#if SILVERLIGHT
			new PropertyMetadata(40.0, OnLayoutDependancyPropertyChanged));
#else
			new FrameworkPropertyMetadata(40.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, OnLayoutDependancyPropertyChanged));
#endif

		///<summary>
		/// The size of the toggle switch's <see cref="Thumb">thumb</see>.
		///</summary>
		[Category(AppearanceCategory)]
		[Description("The size of the toggle switch's thumb.")]
		public double ThumbSize
		{
			get { return (double)GetValue(ThumbSizeProperty); }
			set { SetValue(ThumbSizeProperty, value); }
		}

		#endregion

		#region IsChecked (DependencyProperty)

		///<summary>
		/// DependencyProperty for the <see cref="IsChecked">IsChecked</see> property.
		///</summary>
		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitchBase),
#if SILVERLIGHT
			new PropertyMetadata(false, OnIsCheckedChanged));
#else
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));
#endif

		///<summary>
		/// Gets or sets whether the control is in the checked state.
		///</summary>
		[Category(CommonPropertiesCategory)]
		[Description("Gets or sets whether the control is in the checked state.")]
		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (ToggleSwitchBase)d;

			if (e.NewValue != e.OldValue)
			{
				if ((bool)e.NewValue)
				{
					control.InvokeChecked(new RoutedEventArgs());
				}
				else
				{
					control.InvokeUnchecked(new RoutedEventArgs());
				}
			}

			control.ChangeCheckStates(true);
		}

		#endregion

#if SILVERLIGHT
		#region ZoomFactor (DependencyProperty)

		///<summary>
		/// DependencyProperty for the <see cref="ZoomFactor">ZoomFactor</see> property.
		///</summary>
		public static readonly DependencyProperty ZoomFactorProperty =
			DependencyProperty.Register("ZoomFactor", typeof(double), typeof(ToggleSwitchBase),
												 new PropertyMetadata(1.0));

		///<remarks>
		/// This property is used in conjunction with the <code>Application.Current.Host.Content.ZoomFactor</code> property
		///  to correctly handle mouse drag events at different browser zoom levels.
		///</remarks>
		public double ZoomFactor
		{
			get { return (double)GetValue(ZoomFactorProperty); }
			set { SetValue(ZoomFactorProperty, value); }
		}

		#endregion
#endif

		#endregion

		#region Events

		///<summary>
		/// Event raised when the toggle switch is unchecked.
		///</summary>
		public event RoutedEventHandler Unchecked;

		protected void InvokeUnchecked(RoutedEventArgs e)
		{
			RoutedEventHandler handler = Unchecked;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		///<summary>
		/// Event raised when the toggle switch is checked.
		///</summary>
		public event RoutedEventHandler Checked;

		protected void InvokeChecked(RoutedEventArgs e)
		{
			RoutedEventHandler handler = Checked;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion

		/// <summary> 
		/// Initializes a new instance of the ToggleSwitchBase class.
		/// </summary>
		protected ToggleSwitchBase()
		{
			Loaded += delegate { UpdateVisualState(false); };
			IsEnabledChanged += OnIsEnabledChanged;
		}

		/// <summary>
		/// Raised while dragging the <see cref="Thumb">Thumb</see>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected abstract void OnDragDelta(object sender, DragDeltaEventArgs e);

		/// <summary>
		/// Raised when the dragging of the <see cref="Thumb">Thumb</see> has completed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected abstract void OnDragCompleted(object sender, DragCompletedEventArgs e);

		/// <summary>
		/// Recalculated the layout of the control.
		/// </summary>
		protected abstract void LayoutControls();

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			RemoveEventHandlers();
			GetTemplateChildren();
			AddEventHandlers();

			LayoutControls();

			VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, false);
			ChangeCheckStates(false);
		}

		/// <summary>
		/// Initializes the control's template parts.
		/// </summary>
		protected virtual void GetTemplateChildren()
		{
			SwitchRoot = GetTemplateChild(SwitchRootPart) as FrameworkElement;
			SwitchThumb = GetTemplateChild(SwitchThumbPart) as Thumb;
			SwitchChecked = GetTemplateChild(SwitchCheckedPart) as Control;
			SwitchUnchecked = GetTemplateChild(SwitchUncheckedPart) as Control;
			SwitchTrack = GetTemplateChild(SwitchTrackPart) as FrameworkElement;
		}

		/// <summary>
		/// Subscribe event listeners.
		/// </summary>
		protected virtual void AddEventHandlers()
		{
			if (SwitchThumb != null)
			{
				SwitchThumb.DragStarted += OnDragStarted;
				SwitchThumb.DragDelta += OnDragDelta;
				SwitchThumb.DragCompleted += OnDragCompleted;
			}
			SizeChanged += OnSizeChanged;
		}

		/// <summary>
		/// Unsubscribe event listeners.
		/// </summary>
		protected virtual void RemoveEventHandlers()
		{
			if (SwitchThumb != null)
			{
				SwitchThumb.DragStarted -= OnDragStarted;
				SwitchThumb.DragDelta -= OnDragDelta;
				SwitchThumb.DragCompleted -= OnDragCompleted;
			}
			SizeChanged -= OnSizeChanged;
		}

		/// <summary>
		/// Raised when a drag has started on the <see cref="Thumb">Thumb</see>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnDragStarted(object sender, DragStartedEventArgs e)
		{
			IsDragging = true;
			DragOffset = Offset;
			ChangeCheckStates(false);
#if SILVERLIGHT
			CaptureMouseInternal();
#endif
		}

		/// <summary>
		/// Called when the control is clicked.
		/// </summary>
		protected void OnClick()
		{
#if SILVERLIGHT
            IsChecked = !IsChecked;
#else
            SetCurrentValue(IsCheckedProperty, !IsChecked);
#endif
        }

        /// <summary>
        /// Raised when the size of the control has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			LayoutControls();
		}

		/// <summary> 
		/// Capture the mouse. 
		/// </summary>
		internal void CaptureMouseInternal()
		{
			if (!_isMouseCaptured)
			{
				_isMouseCaptured = CaptureMouse();
			}
		}

		/// <summary>
		/// Release mouse capture if we already had it. 
		/// </summary>
		protected internal void ReleaseMouseCaptureInternal()
		{
			ReleaseMouseCapture();
			_isMouseCaptured = false;
		}

		/// <summary>
		/// Raised when a dependency property that affects the control's layout has changed.
		/// </summary>
		/// <param name="d">The ToggleSwitch control</param>
		/// <param name="e"></param>
		private static void OnLayoutDependancyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != e.OldValue)
			{
				((ToggleSwitchBase)d).LayoutControls();
			}
		}

		/// <summary>
		/// Called when the IsEnabled property changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_suspendStateChanges = true;
			if (!IsEnabled)
			{
				IsPressed = false;
#if SILVERLIGHT
				IsMouseOver = false;
#endif
				_isMouseCaptured = false;
				_isSpaceKeyDown = false;
				_isMouseLeftButtonDown = false;
			}

			_suspendStateChanges = false;
			UpdateVisualState();
		}

#if SILVERLIGHT
		/// <summary> 
		/// Responds to the GotFocus event.
		/// </summary>
		/// <param name="e">The event data for the GotFocus event.</param> 
		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);
			IsFocused = true;
			UpdateVisualState();
		}
#endif

		/// <summary> 
		/// Responds to the LostFocus event.
		/// </summary> 
		/// <param name="e">The event data for the LostFocus event.</param>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
#if SILVERLIGHT
			IsFocused = false;
#endif
			IsPressed = false;
			ReleaseMouseCaptureInternal();
			_isSpaceKeyDown = false;

			_suspendStateChanges = false;
			UpdateVisualState();
		}

		/// <summary> 
		/// Responds to the KeyDown event.
		/// </summary> 
		/// <param name="e">The event data for the KeyDown event.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Handled)
			{
				return;
			}

			if (OnKeyDownInternal(e.Key))
			{
				e.Handled = true;
			}
		}

		/// <summary> 
		/// Handles the KeyDown event for ButtonBase.
		/// </summary> 
		/// <param name="key">
		/// The keyboard key associated with the event.
		/// </param> 
		/// <returns>True if the event was handled, false otherwise.</returns>
		/// <remarks>
		/// This method exists for the purpose of unit testing since we can't 
		/// set KeyEventArgs.Key to simulate key press events. 
		/// </remarks>
		private bool OnKeyDownInternal(Key key)
		{
			bool handled = false;

			if (IsEnabled)
			{
				if (key == Key.Space)
				{
					if (!_isMouseCaptured && !_isSpaceKeyDown)
					{
						_isSpaceKeyDown = true;
						IsPressed = true;
						CaptureMouseInternal();

						handled = true;
					}
				}
				else if (key == Key.Enter)
				{
					_isSpaceKeyDown = false;
					IsPressed = false;
					ReleaseMouseCaptureInternal();

					OnClick();

					handled = true;
				}
				else if (_isSpaceKeyDown)
				{
					IsPressed = false;
					_isSpaceKeyDown = false;
					ReleaseMouseCaptureInternal();
				}
			}

			return handled;
		}

		/// <summary> 
		/// Responds to the KeyUp event. 
		/// </summary>
		/// <param name="e">The event data for the KeyUp event.</param> 
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.Handled)
			{
				return;
			}

			if (OnKeyUpInternal(e.Key))
			{
				e.Handled = true;
			}
		}

		/// <summary> 
		/// Handles the KeyUp event for ButtonBase. 
		/// </summary>
		/// <param name="key">The keyboard key associated with the event.</param> 
		/// <returns>True if the event was handled, false otherwise.</returns>
		/// <remarks>
		/// This method exists for the purpose of unit testing since we can't 
		/// set KeyEventArgs.Key to simulate key press events.
		/// </remarks>
		private bool OnKeyUpInternal(Key key)
		{
			bool handled = false;

			if (IsEnabled && (key == Key.Space))
			{
				_isSpaceKeyDown = false;

				if (!_isMouseLeftButtonDown)
				{
					ReleaseMouseCaptureInternal();
					if (IsPressed)
					{
						OnClick();
					}

					IsPressed = false;
				}
				else if (_isMouseCaptured)
				{
					bool isValid = IsValidMousePosition();
					IsPressed = isValid;
					if (!isValid)
					{
						ReleaseMouseCaptureInternal();
					}
				}

				handled = true;
			}

			return handled;
		}

#if SILVERLIGHT
		/// <summary>
		/// Responds to the MouseEnter event. 
		/// </summary>
		/// <param name="e">The event data for the MouseEnter event.</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			IsMouseOver = true;
			_suspendStateChanges = false;
			UpdateVisualState();
		}

		/// <summary>
		/// Responds to the MouseLeave event.
		/// </summary> 
		/// <param name="e">The event data for the MouseLeave event.</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			IsMouseOver = false;
			_suspendStateChanges = false;
			UpdateVisualState();
		}
#endif

		/// <summary> 
		/// Responds to the MouseLeftButtonDown event.
		/// </summary>
		/// <param name="e"> 
		/// The event data for the MouseLeftButtonDown event.
		/// </param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (e.Handled)
			{
				return;
			}

			_isMouseLeftButtonDown = true;

			if (!IsEnabled)
			{
				return;
			}

			e.Handled = true;
			_suspendStateChanges = true;
			Focus();

			CaptureMouseInternal();
			if (_isMouseCaptured)
			{
				IsPressed = true;
			}

			_suspendStateChanges = false;
			UpdateVisualState();
		}

		/// <summary> 
		/// Responds to the MouseLeftButtonUp event.
		/// </summary>
		/// <param name="e"> 
		/// The event data for the MouseLeftButtonUp event.
		/// </param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);
			if (e.Handled)
			{
				return;
			}

			_isMouseLeftButtonDown = false;

			if (!IsEnabled)
			{
				return;
			}

			e.Handled = true;
			if (!_isSpaceKeyDown && IsPressed)
			{
				OnClick();
			}

			if (!_isSpaceKeyDown)
			{
				ReleaseMouseCaptureInternal();
				IsPressed = false;
			}
		}

		/// <summary> 
		/// Responds to the MouseMove event.
		/// </summary> 
		/// <param name="e">The event data for the MouseMove event.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			_mousePosition = e.GetPosition(this);

			if (_isMouseLeftButtonDown &&
				 IsEnabled &&
				 _isMouseCaptured &&
				 !_isSpaceKeyDown)
			{
				IsPressed = IsValidMousePosition();
			}
		}

		/// <summary>
		/// Determine if the mouse is above the button based on its last known 
		/// position.
		/// </summary>
		/// <returns> 
		/// True if the mouse is considered above the button, false otherwise. 
		/// </returns>
		private bool IsValidMousePosition()
		{
			return (_mousePosition.X >= 0.0) &&
					 (_mousePosition.X <= ActualWidth) &&
					 (_mousePosition.Y >= 0.0) &&
					 (_mousePosition.Y <= ActualHeight);
		}

		protected bool GoToState(bool useTransitions, string stateName)
		{
			return VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		protected virtual void ChangeVisualState(bool useTransitions)
		{
			if (!IsEnabled)
			{
				GoToState(useTransitions, DisabledState);
			}
			else
			{
				GoToState(useTransitions, IsMouseOver ? MouseOverState : NormalState);
			}

			if (IsFocused && IsEnabled)
			{
				GoToState(useTransitions, FocusedState);
			}
			else
			{
				GoToState(useTransitions, UnfocusedState);
			}
		}

		protected void UpdateVisualState(bool useTransitions = true)
		{
			if (!_suspendStateChanges)
			{
				ChangeVisualState(useTransitions);
			}
		}

		/// <summary>
		/// Updates the control's layout to reflect the current <see cref="IsChecked">IsChecked</see> state.
		/// </summary>
		/// <param name="useTransitions">Whether to use transitions during the layout change.</param>
		protected virtual void ChangeCheckStates(bool useTransitions)
		{
			var state = IsChecked ? CheckedState : UncheckedState;

			if (IsDragging)
			{
				VisualStateManager.GoToState(this, DraggingState + state, useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, state, useTransitions);
				if (SwitchThumb != null)
				{
					VisualStateManager.GoToState(SwitchThumb, state, useTransitions);
				}
			}

			if (SwitchThumb == null || SwitchTrack == null)
			{
				return;
			}

			var storyboard = new Storyboard();
			var duration = new Duration(useTransitions ? TimeSpan.FromMilliseconds(100) : TimeSpan.Zero);
			var backgroundAnimation = new DoubleAnimation();
			var thumbAnimation = new DoubleAnimation();

			backgroundAnimation.Duration = duration;
			thumbAnimation.Duration = duration;

			double offset = IsChecked ? CheckedOffset : UncheckedOffset;
			backgroundAnimation.To = offset;
			thumbAnimation.To = offset;

			storyboard.Children.Add(backgroundAnimation);
			storyboard.Children.Add(thumbAnimation);

			Storyboard.SetTarget(backgroundAnimation, SwitchTrack);
			Storyboard.SetTarget(thumbAnimation, SwitchThumb);

			Storyboard.SetTargetProperty(backgroundAnimation, SlidePropertyPath);
			Storyboard.SetTargetProperty(thumbAnimation, SlidePropertyPath);

			storyboard.Begin();
		}
	}
}
