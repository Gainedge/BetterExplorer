//-----------------------------------------------------------------------
// <copyright file="DropShadowTextBlock.cs" company="Microsoft Corporation copyright 2008.">
// (c) 2008 Microsoft Corporation. All rights reserved.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// </copyright>
// <date>09-Oct-2008</date>
// <author>Martin Grayson</author>
// <summary>A control that displays text, with a drop shadow.</summary>
//-----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Demo.Controls
{
    /// <summary>
    /// A control that displays text, with a drop shadow.
    /// </summary>
    public class DropShadowTextBlock : Control
    {
        /// <summary>
        /// The drop shadow color property.
        /// </summary>
        public static readonly DependencyProperty DropShadowColorProperty = DependencyProperty.Register("DropShadowColor", typeof(Color),
                                                                                                                                          typeof(DropShadowTextBlock),
                                                                                                                                          new PropertyMetadata(DropShadowColorChanged));

        /// <summary>
        /// The drop shadow opacity property.
        /// </summary>
        public static readonly DependencyProperty DropShadowOpacityProperty = DependencyProperty.Register("DropShadowOpacity", typeof(double),
                                                                                                                                            typeof(DropShadowTextBlock),
                                                                                                                                            new PropertyMetadata(DropShadowOpacityChanged));

        /// <summary>
        /// The text property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DropShadowTextBlock), null);

        /// <summary>
        /// The text decorations property.
        /// </summary>
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection),
                                                                                                                                          typeof(DropShadowTextBlock), null);

        /// <summary>
        /// The text wrapping property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping),
                                                                                                                                      typeof(DropShadowTextBlock), null);

        /// <summary>
        /// The drop shadow distance property.
        /// </summary>
        public static readonly DependencyProperty DropShadowDistanceProperty = DependencyProperty.Register("DropShadowDistance", typeof(double),
                                                                                                                                              typeof(DropShadowTextBlock),
                                                                                                                                              new PropertyMetadata(DropShadowDistanceChanged));

        /// <summary>
        /// The drop shadow angle property.
        /// </summary>
        public static readonly DependencyProperty DropShadowAngleProperty = DependencyProperty.Register("DropShadowAngle", typeof(double),
                                                                                                                                          typeof(DropShadowTextBlock),
                                                                                                                                          new PropertyMetadata(DropShadowAngleChanged));

        /// <summary>
        /// Stores the drop shadow brush.
        /// </summary>
        private SolidColorBrush _dropShadowBrush;

        /// <summary>
        /// Stores the drop shadow translate transform.
        /// </summary>
        private TranslateTransform _dropShadowTranslate;

        /// <summary>
        /// DropShadowTextBlock constructor.
        /// </summary>
        public DropShadowTextBlock()
        {
            DefaultStyleKey = typeof(DropShadowTextBlock);
        }

        /// <summary>
        /// Gets or sets the drop shadow color.
        /// </summary>
        [Category("Appearance"), Description("The drop shadow color.")]
        public Color DropShadowColor
        {
            get
            {
                return (Color)GetValue(DropShadowColorProperty);
            }
            set
            {
                SetValue(DropShadowColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the drop shadow opacity.
        /// </summary>
        [Category("Appearance"), Description("The drop shadow opacity.")]
        public double DropShadowOpacity
        {
            get
            {
                return (double)GetValue(DropShadowOpacityProperty);
            }
            set
            {
                SetValue(DropShadowOpacityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the link text.
        /// </summary>
        [Category("Common Properties"), Description("The text content.")]
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the text decorations.
        /// </summary>
        [Category("Common Properties"), Description("The text decorations.")]
        public TextDecorationCollection TextDecorations
        {
            get
            {
                return (TextDecorationCollection)GetValue(TextDecorationsProperty);
            }
            set
            {
                SetValue(TextDecorationsProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the text wrapping.
        /// </summary>
        [Category("Common Properties"), Description("Whether the text wraps.")]
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)GetValue(TextWrappingProperty);
            }
            set
            {
                SetValue(TextWrappingProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the drop shadow distance.
        /// </summary>
        [Category("Appearance"), Description("The drop shadow distance.")]
        public double DropShadowDistance
        {
            get
            {
                return (double)GetValue(DropShadowDistanceProperty);
            }
            set
            {
                SetValue(DropShadowDistanceProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the drop shadow angle.
        /// </summary>
        [Category("Appearance"), Description("The drop shadow angle.")]
        public double DropShadowAngle
        {
            get
            {
                return (double)GetValue(DropShadowAngleProperty);
            }
            set
            {
                SetValue(DropShadowAngleProperty, value);
            }
        }

        /// <summary>
        /// Gets the UI elements out of the template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _dropShadowTranslate = GetTemplateChild("PART_DropShadowTranslate") as TranslateTransform;
            _dropShadowBrush = GetTemplateChild("PART_DropShadowBrush") as SolidColorBrush;
            UpdateDropShadowPosition();
            UpdateDropShadowBrush();
        }

        /// <summary>
        /// Converts degrees into radians.
        /// </summary>
        /// <param name="degrees">The degree value.</param>
        /// <returns>The degrees as radians.</returns>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        /// <summary>
        /// Gets a point offset by a distance and angle (in degrees).
        /// </summary>
        /// <param name="angle">The angle in degrees.</param>
        /// <param name="distance">The distance.</param>
        /// <returns>The offset point.</returns>
        private static Point GetOffset(double angle, double distance)
        {
            double x = Math.Cos(DegreesToRadians(angle)) * distance;
            double y = Math.Tan(DegreesToRadians(angle)) * x;
            return new Point(x, y);
        }

        /// <summary>
        /// Updates the drop shadow.
        /// </summary>
        internal void UpdateDropShadowPosition()
        {
            if (_dropShadowTranslate != null)
            {
                Point offset = GetOffset(DropShadowAngle, DropShadowDistance);

                _dropShadowTranslate.X = offset.X;
                _dropShadowTranslate.Y = offset.Y;
            }
        }

        /// <summary>
        /// Updates the drop shadow brush.
        /// </summary>
        internal void UpdateDropShadowBrush()
        {
            if (_dropShadowBrush != null)
            {
                _dropShadowBrush.Color = DropShadowColor;
                _dropShadowBrush.Opacity = DropShadowOpacity;
            }
        }

        /// <summary>
        /// Updates the drop shadow.
        /// </summary>
        /// <param name="dependencyObject">The drop shadow text block.</param>
        /// <param name="eventArgs">Dependency Property Changed Event Args</param>
        private static void DropShadowDistanceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dropShadowTextBlock = (DropShadowTextBlock)dependencyObject;
            dropShadowTextBlock.UpdateDropShadowPosition();
        }

        /// <summary>
        /// Updates the drop shadow.
        /// </summary>
        /// <param name="dependencyObject">The drop shadow text block.</param>
        /// <param name="eventArgs">Dependency Property Changed Event Args</param>
        private static void DropShadowAngleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dropShadowTextBlock = (DropShadowTextBlock)dependencyObject;
            dropShadowTextBlock.UpdateDropShadowPosition();
        }

        /// <summary>
        /// Updates the drop shadow.
        /// </summary>
        /// <param name="dependencyObject">The drop shadow text block.</param>
        /// <param name="eventArgs">Dependency Property Changed Event Args</param>
        private static void DropShadowColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dropShadowTextBlock = (DropShadowTextBlock)dependencyObject;
            dropShadowTextBlock.UpdateDropShadowBrush();
        }

        /// <summary>
        /// Updates the drop shadow.
        /// </summary>
        /// <param name="dependencyObject">The drop shadow text block.</param>
        /// <param name="eventArgs">Dependency Property Changed Event Args</param>
        private static void DropShadowOpacityChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dropShadowTextBlock = (DropShadowTextBlock)dependencyObject;
            dropShadowTextBlock.UpdateDropShadowBrush();
        }
    }
}