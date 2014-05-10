using System.Windows;
using System.Windows.Controls;

namespace BetterExplorerControls {

	public static class AttachedProperties {

		#region EnableDrag / Drop

		public static DependencyProperty EnableDragProperty =
			 DependencyProperty.RegisterAttached("EnableDrag", typeof(bool), typeof(AttachedProperties));

		[System.Obsolete("Not used!", true)]
		public static bool GetEnableDrag(DependencyObject target) {
			return (bool)target.GetValue(EnableDragProperty);
		}

		[System.Obsolete("Not used!", true)]
		public static void SetEnableDrag(DependencyObject target, bool value) {
			target.SetValue(EnableDragProperty, value);
		}

		public static DependencyProperty EnableDropProperty =
				DependencyProperty.RegisterAttached("EnableDrop", typeof(bool), typeof(AttachedProperties));

		[System.Obsolete("Not used!", true)]
		public static bool GetEnableDrop(DependencyObject target) {
			return (bool)target.GetValue(EnableDropProperty);
		}

		[System.Obsolete("Not used!", true)]
		public static void SetEnableDrop(DependencyObject target, bool value) {
			target.SetValue(EnableDropProperty, value);
		}

		#endregion EnableDrag / Drop

		/// <summary> Skip lookup in UITools.FindAncestor, FindLogicalAncestor and FindVisualChild </summary>
		public static DependencyProperty SkipLookupProperty =
				DependencyProperty.RegisterAttached("SkipLookup", typeof(bool), typeof(AttachedProperties), new PropertyMetadata(false));

		public static bool GetSkipLookup(DependencyObject target) {
			return (bool)target.GetValue(SkipLookupProperty);
		}

		[System.Obsolete("Not used!", true)]
		public static void SetSkipLookup(DependencyObject target, bool value) {
			target.SetValue(SkipLookupProperty, value);
		}

		/// <summary>
		/// Allow ControlUtils.GetScrollContentPresenter to cache the object to the control.
		/// </summary>
		[System.Obsolete("Not used!", false)]
		public static DependencyProperty ScrollContentPresenterProperty =
				DependencyProperty.RegisterAttached("ScrollContentPresenter", typeof(ScrollContentPresenter),
				typeof(AttachedProperties), new PropertyMetadata(null));

		[System.Obsolete("Not used!", true)]
		public static ScrollContentPresenter GetScrollContentPresenter(DependencyObject target) {
			return (ScrollContentPresenter)target.GetValue(ScrollContentPresenterProperty);
		}


		[System.Obsolete("Not used!", true)]
		public static void SetScrollContentPresenter(DependencyObject target, ScrollContentPresenter value) {
			target.SetValue(ScrollContentPresenterProperty, value);
		}

		[System.Obsolete("Not used!", true)]
		public static bool IsValidPosition(this Point point) {
			return point.X != double.NaN && point.Y != double.NaN;
		}
	}
}