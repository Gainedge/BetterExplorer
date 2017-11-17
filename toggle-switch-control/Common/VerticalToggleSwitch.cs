//-----------------------------------------------------------------------
// <copyright file="VerticalToggleSwitch.cs">
// (c) 2011 Eric Jensen. All rights reserved.
// This source is subject to the Microsoft Public License.
// See http://www.opensource.org/licenses/MS-PL.
// </copyright>
// <date>15-Sept-2011</date>
// <author>Eric Jensen</author>
// <summary>Vertically oriented toggle switch control.</summary>
//-----------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ToggleSwitch;

namespace Demo.Controls
{
	///<summary>
	/// Vertically oriented toggle switch control.
	///</summary>
	public class VerticalToggleSwitch : ToggleSwitchBase
	{
		public VerticalToggleSwitch()
		{
			DefaultStyleKey = typeof(VerticalToggleSwitch);
		}

		protected override double Offset
		{
			get { return Canvas.GetTop(SwitchThumb); }
			set
			{
#if WPF
				SwitchTrack.BeginAnimation(Canvas.TopProperty, null);
				SwitchThumb.BeginAnimation(Canvas.TopProperty, null);
#endif
				Canvas.SetTop(SwitchThumb, value);
				Canvas.SetTop(SwitchTrack, value);
			}
		}

		protected override PropertyPath SlidePropertyPath
		{
			get { return new PropertyPath("(Canvas.Top)"); }
		}

		protected override void OnDragDelta(object sender, DragDeltaEventArgs e)
		{
#if SILVERLIGHT
			DragOffset += e.VerticalChange * ZoomFactor;
#else
			DragOffset += e.VerticalChange;
#endif
			Offset = Math.Min(UncheckedOffset, Math.Max(CheckedOffset, DragOffset));
		}

		protected override void LayoutControls()
		{
			if (SwitchThumb == null || SwitchRoot == null)
			{
				return;
			}

			double fullThumbHeight = SwitchThumb.ActualHeight + SwitchThumb.BorderThickness.Top + SwitchThumb.BorderThickness.Bottom;

			if (SwitchChecked != null && SwitchUnchecked != null)
			{
				SwitchChecked.Height = SwitchUnchecked.Height = Math.Max(0, SwitchRoot.ActualHeight - fullThumbHeight / 2);
				SwitchChecked.Padding = new Thickness(0, 0, 0, (SwitchThumb.ActualHeight + +SwitchThumb.BorderThickness.Bottom) / 2);
				SwitchUnchecked.Padding = new Thickness(0, (SwitchThumb.ActualHeight + +SwitchThumb.BorderThickness.Top) / 2, 0, 0);
			}

			UncheckedOffset = SwitchRoot.ActualHeight - SwitchThumb.ActualHeight - SwitchThumb.Margin.Top - SwitchThumb.Margin.Bottom;
			CheckedOffset = 0;

			if (!IsDragging)
			{
				Offset = IsChecked ? CheckedOffset : UncheckedOffset;
				ChangeCheckStates(false);
			}
		}

		protected override void OnDragCompleted(object sender, DragCompletedEventArgs e)
		{
			IsDragging = false;
			bool click = false;

			if ((!IsChecked && DragOffset < ((SwitchRoot.ActualHeight - SwitchThumb.ActualHeight) * (1.0 - Elasticity)))
				 || (IsChecked && DragOffset > ((SwitchRoot.ActualHeight - SwitchThumb.ActualHeight) * Elasticity)))
			{
				double edge = IsChecked ? CheckedOffset : UncheckedOffset;
				if (Offset != edge)
				{
					click = true;
				}
			}
			else if (DragOffset == CheckedOffset || DragOffset == UncheckedOffset)
			{
				click = true;
			}
			else
			{
				ChangeCheckStates(true);
			}

			if (click)
			{
				OnClick();
			}

			DragOffset = 0;
#if SILVERLIGHT
			ReleaseMouseCaptureInternal();
#endif
		}
	}
}