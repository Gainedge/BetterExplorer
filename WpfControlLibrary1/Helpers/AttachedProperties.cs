using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterExplorerControls
{

	public static class AttachedProperties
	{

		/// <summary>
		/// Skip lookup in UITools.FindAncestor, FindLogicalAncestor and FindVisualChild
		/// </summary>
		public static DependencyProperty SkipLookupProperty =
				DependencyProperty.RegisterAttached("SkipLookup", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false));

		public static bool GetSkipLookup(DependencyObject target)
		{
			return (bool)target.GetValue(SkipLookupProperty);
		}

		public static void SetSkipLookup(DependencyObject target, bool value)
		{
			target.SetValue(SkipLookupProperty, value);
		}




		/// <summary>
		/// Allow ControlUtils.GetScrollContentPresenter to cache the object to the control.
		/// </summary>
		public static DependencyProperty ScrollContentPresenterProperty =
				DependencyProperty.RegisterAttached("ScrollContentPresenter", typeof(ScrollContentPresenter),
				typeof(AttachedProperties), new PropertyMetadata(null));

		public static ScrollContentPresenter GetScrollContentPresenter(DependencyObject target)
		{
			return (ScrollContentPresenter)target.GetValue(ScrollContentPresenterProperty);
		}

		public static void SetScrollContentPresenter(DependencyObject target, ScrollContentPresenter value)
		{
			target.SetValue(ScrollContentPresenterProperty, value);
		}


		#region EnableDrag / Drop

		public static DependencyProperty EnableDragProperty =
			 DependencyProperty.RegisterAttached("EnableDrag", typeof(bool), typeof(AttachedProperties));

		public static bool GetEnableDrag(DependencyObject target)
		{
			return (bool)target.GetValue(EnableDragProperty);
		}

		public static void SetEnableDrag(DependencyObject target, bool value)
		{
			target.SetValue(EnableDragProperty, value);
		}


		public static DependencyProperty EnableDropProperty =
				DependencyProperty.RegisterAttached("EnableDrop", typeof(bool), typeof(AttachedProperties));


		public static bool GetEnableDrop(DependencyObject target)
		{
			return (bool)target.GetValue(EnableDropProperty);
		}

		public static void SetEnableDrop(DependencyObject target, bool value)
		{
			target.SetValue(EnableDropProperty, value);
		}

		#endregion







		public static bool IsValidPosition(this Point point)
		{
			return point.X != double.NaN && point.Y != double.NaN;
		}











	}
}
