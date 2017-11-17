//-----------------------------------------------------------------------
// <copyright file="InnerGlowBorder.cs" company="Microsoft Corporation copyright 2008.">
// (c) 2008 Microsoft Corporation. All rights reserved.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// </copyright>
// <date>03-Oct-2008</date>
// <author>Martin Grayson</author>
// <summary>A border that also shows an inner glow.</summary>
//-----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToggleSwitch.Borders
{
	/// <summary>
	/// Content control that draws a glow around its inside.
	/// </summary>
	public class InnerGlowBorder : ContentControl
	{
		/// <summary>
		/// The inner glow opacity property.
		/// </summary>
		public static readonly DependencyProperty InnerGlowOpacityProperty = DependencyProperty.Register("InnerGlowOpacity", typeof(double),
																																		  typeof(InnerGlowBorder), null);

		/// <summary>
		/// The inner glow size property.
		/// </summary>
		public static readonly DependencyProperty InnerGlowSizeProperty = DependencyProperty.Register("InnerGlowSize", typeof(Thickness),
																																	  typeof(InnerGlowBorder),
																																	  new PropertyMetadata(InnerGlowSizeChanged));

		/// <summary>
		/// The corner radius property.
		/// </summary>
		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius),
																																	 typeof(InnerGlowBorder), null);

		/// <summary>
		/// The inner glow color.
		/// </summary>
		public static readonly DependencyProperty InnerGlowColorProperty = DependencyProperty.Register("InnerGlowColor", typeof(Color),
																																		typeof(InnerGlowBorder),
																																		new PropertyMetadata(
																																			Colors.Black,
																																			InnerGlowColorChanged));

		/// <summary>
		/// The clip content property.
		/// </summary>
		public static readonly DependencyProperty ClipContentProperty = DependencyProperty.Register("ClipContent", typeof(bool), typeof(InnerGlowBorder), null);

		/// <summary>
		/// The content z-index property.
		/// </summary>
		public static readonly DependencyProperty ContentZIndexProperty = DependencyProperty.Register("ContentZIndex", typeof(int), typeof(InnerGlowBorder), null);

		/// <summary>
		/// Stores the bottom glow border.
		/// </summary>
		private Border _bottomGlow;

		/// <summary>
		/// Stores the bottom glow stop 0;
		/// </summary>
		private GradientStop _bottomGlowStop0;

		/// <summary>
		/// Stores the bottom glow stop 1.
		/// </summary>
		private GradientStop _bottomGlowStop1;

		/// <summary>
		/// Stores the left glow border.
		/// </summary>
		private Border _leftGlow;

		/// <summary>
		/// Stores the left glow stop 0;
		/// </summary>
		private GradientStop _leftGlowStop0;

		/// <summary>
		/// Stores the left glow stop 1;
		/// </summary>
		private GradientStop _leftGlowStop1;

		/// <summary>
		/// Stores the right glow border.
		/// </summary>
		private Border _rightGlow;

		/// <summary>
		/// Stores the right glow stop 0;
		/// </summary>
		private GradientStop _rightGlowStop0;

		/// <summary>
		/// Stores the right glow stop 1.
		/// </summary>
		private GradientStop _rightGlowStop1;

		/// <summary>
		/// Stores the top glow border.
		/// </summary>
		private Border _topGlow;

		/// <summary>
		/// Stores the top glow stop 0;
		/// </summary>
		private GradientStop _topGlowStop0;

		/// <summary>
		/// Stores the top glow stop 1;
		/// </summary>
		private GradientStop _topGlowStop1;

		/// <summary>
		/// InnerGlowBorder constructor.
		/// </summary>
		public InnerGlowBorder()
		{
			DefaultStyleKey = typeof(InnerGlowBorder);
		}

		/// <summary>
		/// Gets or sets a value indicating whether the content is clipped.
		/// </summary>
		[Category("Appearance"), Description("Sets whether the content is clipped or not.")]
		public bool ClipContent
		{
			get
			{
				return (bool)GetValue(ClipContentProperty);
			}
			set
			{
				SetValue(ClipContentProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the content z-index. 0 for behind shadow, 1 for in-front.
		/// </summary>
		[Category("Appearance"), Description("Set 0 for behind the shadow, 1 for in front.")]
		public int ContentZIndex
		{
			get
			{
				return (int)GetValue(ContentZIndexProperty);
			}
			set
			{
				SetValue(ContentZIndexProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the inner glow opacity.
		/// </summary>
		[Category("Appearance"), Description("The inner glow opacity.")]
		public double InnerGlowOpacity
		{
			get
			{
				return (double)GetValue(InnerGlowOpacityProperty);
			}
			set
			{
				SetValue(InnerGlowOpacityProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the inner glow color.
		/// </summary>
		[Category("Appearance"), Description("The inner glow color.")]
		public Color InnerGlowColor
		{
			get
			{
				return (Color)GetValue(InnerGlowColorProperty);
			}

			set
			{
				SetValue(InnerGlowColorProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the inner glow size.
		/// </summary>
		[Category("Appearance"), Description("The inner glow size.")]
		public Thickness InnerGlowSize
		{
			get
			{
				return (Thickness)GetValue(InnerGlowSizeProperty);
			}

			set
			{
				SetValue(InnerGlowSizeProperty, value);
				UpdateGlowSize(value);
			}
		}

		/// <summary>
		/// Gets or sets the border corner radius.
		/// This is a thickness, as there is a problem parsing CornerRadius types.
		/// </summary>
		[Category("Appearance"), Description("Sets the corner radius on the border.")]
		public CornerRadius CornerRadius
		{
			get
			{
				return (CornerRadius)GetValue(CornerRadiusProperty);
			}

			set
			{
				SetValue(CornerRadiusProperty, value);
			}
		}

		/// <summary>
		/// Gets the template parts out.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_leftGlow = GetTemplateChild("PART_LeftGlow") as Border;
			_topGlow = GetTemplateChild("PART_TopGlow") as Border;
			_rightGlow = GetTemplateChild("PART_RightGlow") as Border;
			_bottomGlow = GetTemplateChild("PART_BottomGlow") as Border;

			_leftGlowStop0 = GetTemplateChild("PART_LeftGlowStop0") as GradientStop;
			_leftGlowStop1 = GetTemplateChild("PART_LeftGlowStop1") as GradientStop;
			_topGlowStop0 = GetTemplateChild("PART_TopGlowStop0") as GradientStop;
			_topGlowStop1 = GetTemplateChild("PART_TopGlowStop1") as GradientStop;
			_rightGlowStop0 = GetTemplateChild("PART_RightGlowStop0") as GradientStop;
			_rightGlowStop1 = GetTemplateChild("PART_RightGlowStop1") as GradientStop;
			_bottomGlowStop0 = GetTemplateChild("PART_BottomGlowStop0") as GradientStop;
			_bottomGlowStop1 = GetTemplateChild("PART_BottomGlowStop1") as GradientStop;

			UpdateGlowColor(InnerGlowColor);
			UpdateGlowSize(InnerGlowSize);
		}

		/// <summary>
		/// Updates the inner glow color.
		/// </summary>
		/// <param name="color">The new color.</param>
		internal void UpdateGlowColor(Color color)
		{
			if (_leftGlowStop0 != null)
			{
				_leftGlowStop0.Color = color;
			}

			if (_leftGlowStop1 != null)
			{
				_leftGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
			}

			if (_topGlowStop0 != null)
			{
				_topGlowStop0.Color = color;
			}

			if (_topGlowStop1 != null)
			{
				_topGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
			}

			if (_rightGlowStop0 != null)
			{
				_rightGlowStop0.Color = color;
			}

			if (_rightGlowStop1 != null)
			{
				_rightGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
			}

			if (_bottomGlowStop0 != null)
			{
				_bottomGlowStop0.Color = color;
			}

			if (_bottomGlowStop1 != null)
			{
				_bottomGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
			}
		}

		/// <summary>
		/// Sets the glow size.
		/// </summary>
		/// <param name="newGlowSize">The new glow size.</param>
		internal void UpdateGlowSize(Thickness newGlowSize)
		{
			if (_leftGlow != null)
			{
				_leftGlow.Width = Math.Abs(newGlowSize.Left);
			}

			if (_topGlow != null)
			{
				_topGlow.Height = Math.Abs(newGlowSize.Top);
			}

			if (_rightGlow != null)
			{
				_rightGlow.Width = Math.Abs(newGlowSize.Right);
			}

			if (_bottomGlow != null)
			{
				_bottomGlow.Height = Math.Abs(newGlowSize.Bottom);
			}
		}

		/// <summary>
		/// Updates the inner glow color when the DP changes.
		/// </summary>
		/// <param name="dependencyObject">The inner glow border.</param>
		/// <param name="eventArgs">The new property event args.</param>
		private static void InnerGlowColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
		{
			if (eventArgs.NewValue != null)
			{
				var innerGlowBorder = (InnerGlowBorder)dependencyObject;
				innerGlowBorder.UpdateGlowColor((Color)eventArgs.NewValue);
			}
		}

		/// <summary>
		/// Updates the glow size.
		/// </summary>
		/// <param name="dependencyObject">The inner glow border.</param>
		/// <param name="eventArgs">Dependency Property Changed Event Args</param>
		private static void InnerGlowSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
		{
			var innerGlowBorder = (InnerGlowBorder)dependencyObject;
			innerGlowBorder.UpdateGlowSize((Thickness)eventArgs.NewValue);
		}
	}
}