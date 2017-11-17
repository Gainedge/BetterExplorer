//-----------------------------------------------------------------------
// <copyright file="ClippingBorder.cs" company="Microsoft Corporation copyright 2008.">
// (c) 2008 Microsoft Corporation. All rights reserved.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// </copyright>
// <date>07-Oct-2008</date>
// <author>Martin Grayson</author>
// <summary>A border that clips its contents.</summary>
//-----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToggleSwitch.Borders
{
	/// <summary>
	/// A border that clips its contents.
	/// </summary>
	public class ClippingBorder : ContentControl
	{
		/// <summary>
		/// The corner radius property.
		/// </summary>
		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius),
																																	 typeof(ClippingBorder),
																																	 new PropertyMetadata(CornerRadiusChanged));

		/// <summary>
		/// The clip content property.
		/// </summary>
		public static readonly DependencyProperty ClipContentProperty = DependencyProperty.Register("ClipContent", typeof(bool), typeof(ClippingBorder),
																																	new PropertyMetadata(ClipContentChanged));

		/// <summary>
		/// Stores the main border.
		/// </summary>
		private Border _border;

		/// <summary>
		/// Stores the clip responsible for clipping the bottom left corner.
		/// </summary>
		private RectangleGeometry _bottomLeftClip;

		/// <summary>
		/// Stores the bottom left content control.
		/// </summary>
		private ContentControl _bottomLeftContentControl;

		/// <summary>
		/// Stores the clip responsible for clipping the bottom right corner.
		/// </summary>
		private RectangleGeometry _bottomRightClip;

		/// <summary>
		/// Stores the bottom right content control.
		/// </summary>
		private ContentControl _bottomRightContentControl;

		/// <summary>
		/// Stores the clip responsible for clipping the top left corner.
		/// </summary>
		private RectangleGeometry _topLeftClip;

		/// <summary>
		/// Stores the top left content control.
		/// </summary>
		private ContentControl _topLeftContentControl;

		/// <summary>
		/// Stores the clip responsible for clipping the top right corner.
		/// </summary>
		private RectangleGeometry _topRightClip;

		/// <summary>
		/// Stores the top right content control.
		/// </summary>
		private ContentControl _topRightContentControl;

		/// <summary>
		/// ClippingBorder constructor.
		/// </summary>
		public ClippingBorder()
		{
			DefaultStyleKey = typeof(ClippingBorder);
			SizeChanged += ClippingBorderSizeChanged;
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
		/// Gets the UI elements out of the template.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_border = GetTemplateChild("PART_Border") as Border;
			_topLeftContentControl = GetTemplateChild("PART_TopLeftContentControl") as ContentControl;
			_topRightContentControl = GetTemplateChild("PART_TopRightContentControl") as ContentControl;
			_bottomRightContentControl = GetTemplateChild("PART_BottomRightContentControl") as ContentControl;
			_bottomLeftContentControl = GetTemplateChild("PART_BottomLeftContentControl") as ContentControl;

			if (_topLeftContentControl != null)
			{
				_topLeftContentControl.SizeChanged += ContentControlSizeChanged;
			}

			_topLeftClip = GetTemplateChild("PART_TopLeftClip") as RectangleGeometry;
			_topRightClip = GetTemplateChild("PART_TopRightClip") as RectangleGeometry;
			_bottomRightClip = GetTemplateChild("PART_BottomRightClip") as RectangleGeometry;
			_bottomLeftClip = GetTemplateChild("PART_BottomLeftClip") as RectangleGeometry;

			UpdateClipContent(ClipContent);

			UpdateCornerRadius(CornerRadius);
		}

		/// <summary>
		/// Sets the corner radius.
		/// </summary>
		/// <param name="newCornerRadius">The new corner radius.</param>
		internal void UpdateCornerRadius(CornerRadius newCornerRadius)
		{
			if (_border != null)
			{
				_border.CornerRadius = newCornerRadius;
			}

			if (_topLeftClip != null)
			{
				_topLeftClip.RadiusX = _topLeftClip.RadiusY = newCornerRadius.TopLeft - (Math.Min(BorderThickness.Left, BorderThickness.Top) / 2);
			}

			if (_topRightClip != null)
			{
				_topRightClip.RadiusX = _topRightClip.RadiusY = newCornerRadius.TopRight - (Math.Min(BorderThickness.Top, BorderThickness.Right) / 2);
			}

			if (_bottomRightClip != null)
			{
				_bottomRightClip.RadiusX = _bottomRightClip.RadiusY = newCornerRadius.BottomRight - (Math.Min(BorderThickness.Right, BorderThickness.Bottom) / 2);
			}

			if (_bottomLeftClip != null)
			{
				_bottomLeftClip.RadiusX = _bottomLeftClip.RadiusY = newCornerRadius.BottomLeft - (Math.Min(BorderThickness.Bottom, BorderThickness.Left) / 2);
			}

			UpdateClipSize(new Size(ActualWidth, ActualHeight));
		}

		/// <summary>
		/// Updates whether the content is clipped.
		/// </summary>
		/// <param name="clipContent">Whether the content is clipped.</param>
		internal void UpdateClipContent(bool clipContent)
		{
			if (clipContent)
			{
				if (_topLeftContentControl != null)
				{
					_topLeftContentControl.Clip = _topLeftClip;
				}

				if (_topRightContentControl != null)
				{
					_topRightContentControl.Clip = _topRightClip;
				}

				if (_bottomRightContentControl != null)
				{
					_bottomRightContentControl.Clip = _bottomRightClip;
				}

				if (_bottomLeftContentControl != null)
				{
					_bottomLeftContentControl.Clip = _bottomLeftClip;
				}

				UpdateClipSize(new Size(ActualWidth, ActualHeight));
			}
			else
			{
				if (_topLeftContentControl != null)
				{
					_topLeftContentControl.Clip = null;
				}

				if (_topRightContentControl != null)
				{
					_topRightContentControl.Clip = null;
				}

				if (_bottomRightContentControl != null)
				{
					_bottomRightContentControl.Clip = null;
				}

				if (_bottomLeftContentControl != null)
				{
					_bottomLeftContentControl.Clip = null;
				}
			}
		}

		/// <summary>
		/// Updates the corner radius.
		/// </summary>
		/// <param name="dependencyObject">The clipping border.</param>
		/// <param name="eventArgs">Dependency Property Changed Event Args</param>
		private static void CornerRadiusChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
		{
			var clippingBorder = (ClippingBorder)dependencyObject;
			clippingBorder.UpdateCornerRadius((CornerRadius)eventArgs.NewValue);
		}

		/// <summary>
		/// Updates the content clipping.
		/// </summary>
		/// <param name="dependencyObject">The clipping border.</param>
		/// <param name="eventArgs">Dependency Property Changed Event Args</param>
		private static void ClipContentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
		{
			var clippingBorder = (ClippingBorder)dependencyObject;
			clippingBorder.UpdateClipContent((bool)eventArgs.NewValue);
		}

		/// <summary>
		/// Updates the clips.
		/// </summary>
		/// <param name="sender">The clipping border</param>
		/// <param name="e">Size Changed Event Args.</param>
		private void ClippingBorderSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (ClipContent)
			{
				UpdateClipSize(e.NewSize);
			}
		}

		/// <summary>
		/// Updates the clip size.
		/// </summary>
		/// <param name="sender">A content control.</param>
		/// <param name="e">Size Changed Event Args</param>
		private void ContentControlSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (ClipContent)
			{
				UpdateClipSize(new Size(ActualWidth, ActualHeight));
			}
		}

		/// <summary>
		/// Updates the clip size.
		/// </summary>
		/// <param name="size">The control size.</param>
		private void UpdateClipSize(Size size)
		{
			if (size.Width > 0 || size.Height > 0)
			{
				double contentWidth = Math.Max(0, size.Width - BorderThickness.Left - BorderThickness.Right);
				double contentHeight = Math.Max(0, size.Height - BorderThickness.Top - BorderThickness.Bottom);

				if (_topLeftClip != null)
				{
					_topLeftClip.Rect = new Rect(0, 0, contentWidth + (CornerRadius.TopLeft * 2), contentHeight + (CornerRadius.TopLeft * 2));
				}

				if (_topRightClip != null)
				{
					_topRightClip.Rect = new Rect(0 - CornerRadius.TopRight, 0, contentWidth + CornerRadius.TopRight, contentHeight + CornerRadius.TopRight);
				}

				if (_bottomRightClip != null)
				{
					_bottomRightClip.Rect = new Rect(0 - CornerRadius.BottomRight, 0 - CornerRadius.BottomRight, contentWidth + CornerRadius.BottomRight,
																contentHeight + CornerRadius.BottomRight);
				}

				if (_bottomLeftClip != null)
				{
					_bottomLeftClip.Rect = new Rect(0, 0 - CornerRadius.BottomLeft, contentWidth + CornerRadius.BottomLeft, contentHeight + CornerRadius.BottomLeft);
				}
			}
		}
	}
}