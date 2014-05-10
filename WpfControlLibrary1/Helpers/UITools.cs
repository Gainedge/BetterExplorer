using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BetterExplorerControls {

	public static partial class UITools {

		public static void AddValueChanged<T>(this T obj, DependencyProperty property, EventHandler handler) where T : DependencyObject {
			DependencyPropertyDescriptor desc = DependencyPropertyDescriptor.FromProperty(property, typeof(T));
			desc.AddValueChanged(obj, handler);
		}

		public static void AddValueChangedDispatcher<T>(this T obj, DependencyProperty property,
				EventHandler handler, DispatcherPriority priority) where T : DependencyObject {
			AddValueChanged(obj, property, (o, e) => {
				obj.Dispatcher.BeginInvoke(priority, handler);
			});
		}

		public static T FindAncestor<T>(DependencyObject obj) where T : DependencyObject {
			while (obj != null) {
				T o = obj as T;

				if (o != null)
					return o;

				obj = VisualTreeHelper.GetParent(obj);
				if (obj != null && AttachedProperties.GetSkipLookup(obj))
					obj = null;
			}
			return default(T);
		}

		public static T FindLogicalAncestor<T>(DependencyObject obj) where T : DependencyObject {
			while (obj != null) {
				T o = obj as T;

				if (o != null)
					return o;

				obj = LogicalTreeHelper.GetParent(obj);

				if (obj != null && AttachedProperties.GetSkipLookup(obj))
					obj = null;
			}
			return default(T);
		}

		public static T FindAncestor<T>(this UIElement obj) where T : UIElement {
			return FindAncestor<T>((DependencyObject)obj);
		}

		private static bool SameDataContext(DependencyObject obj1, DependencyObject obj2) {
			var dc1 = obj1.GetValue(FrameworkElement.DataContextProperty);
			var dc2 = obj2.GetValue(FrameworkElement.DataContextProperty);
			return dc1 == dc2;
		}

		//http://stackoverflow.com/questions/665719/wpf-animate-listbox-scrollviewer-horizontaloffset
		public static T FindVisualChild<T>(DependencyObject obj, Func<T, bool> filter = null, int level = -1) where T : DependencyObject {
			if (filter == null)
				filter = (t) => true;

			// Search immediate children first (breadth-first)
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);

				if (child != null && child is T && filter((T)child)) {
					return (T)child;
				}
				else if (level == -1 || level > 0) {
					if (!AttachedProperties.GetSkipLookup(child)) {
						T childOfChild = FindVisualChild<T>(child, filter, level == -1 ? -1 : level - 1);

						if (childOfChild != null)
							return childOfChild;
					}
				}
			}

			return null;
		}

		public static IEnumerable<T> FindAllVisualChildren<T>(DependencyObject obj, Func<T, bool> filter = null) where T : DependencyObject {
			if (filter == null)
				filter = (t) => true;

			// Search immediate children first (breadth-first)
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);

				if (child != null && child is T) {
					yield return (T)child;
					foreach (var gc in FindAllVisualChildren<T>(child, filter))
						yield return gc;
				}
			}
		}

		//http://pwnedcode.wordpress.com/2009/04/01/find-a-control-in-a-wpfsilverlight-visual-tree-by-name/
		public static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject {
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
				var child = VisualTreeHelper.GetChild(parent, i);
				string controlName = child.GetValue(Control.NameProperty) as string;
				if (controlName == name) {
					return child as T;
				}
				else {
					T result = FindVisualChildByName<T>(child, name);
					if (result != null)
						return result;
				}
			}
			return null;
		}

		#region GetScreenMousePosition

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			public int X;
			public int Y;

			public POINT(int x, int y) {
				this.X = x;
				this.Y = y;
			}

			public static implicit operator System.Drawing.Point(POINT p) {
				return new System.Drawing.Point(p.X, p.Y);
			}

			public static implicit operator POINT(System.Drawing.Point p) {
				return new POINT(p.X, p.Y);
			}
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out POINT lpPoint);

		public static Point GetScreenMousePosition() {
			POINT p;
			GetCursorPos(out p);
			return new Point(p.X, p.Y);
		}

		#endregion GetScreenMousePosition

		/// <summary> Add a slash "\" to end of input if not exists. </summary>
		[System.Obsolete("Not used!", true)]
		public static string AppendSlash(string input) {
			return input.EndsWith(@"\") ? input : input + @"\";
		}

		[System.Obsolete("Not used!", true)]
		public static string SizeInK(Int64 size) {
			return SizeInK((UInt64)size);
		}

		[System.Obsolete("Not used!", false)]
		public static string SizeInK(UInt64 size) {
			if (size == 0)
				return "0 kb";

			float sizeink = ((float)size / 1024);
			if (sizeink <= 999.99)
				return sizeink.ToString("#0.00") + " kb";

			float sizeinm = sizeink / 1024;
			if (sizeinm <= 999.99)
				return sizeinm.ToString("###,###,###,##0.#") + " mb";

			float sizeing = sizeinm / 1024;
			return sizeing.ToString("###,###,###,##0.#") + " GB";
		}

		/// <summary> Take a listbox and a position and return the listbox item at that spot. </summary>
		/// <param name="sender">   </param>
		/// <param name="position"> </param>
		/// <returns> </returns>
		public static ListBoxItem GetListBoxItem(ListBox sender, Point position) {
			return GetItemByPosition<ListBoxItem, ListBox>(sender, position);
		}

		/// <summary> Take a treeview and a position and return the treeview item at that spot. </summary>
		/// <param name="sender">   </param>
		/// <param name="position"> </param>
		/// <returns> </returns>
		public static TreeViewItem GetTreeViewItem(TreeView sender, Point position) {
			return GetItemByPosition<TreeViewItem, TreeView>(sender, position);
		}

		/// <summary>
		/// Take a container (e.g. listbox) and a position and return the item (e.g. listboxItem) at
		/// that spot.
		/// </summary>
		/// <param name="container"> </param>
		/// <param name="position">  </param>
		/// <returns> </returns>
		public static I GetItemByPosition<I, C>(C container, Point position)
			where C : UIElement
			where I : UIElement {
			DependencyObject obj = null;
			//Bug#66
			VisualTreeHelper.HitTest(
			container,
			(o) => {
				if (UITools.FindAncestor<I>(o) != null)
					return HitTestFilterBehavior.Continue;
				else return HitTestFilterBehavior.ContinueSkipSelf;
			},
			(r) => {
				obj = r.VisualHit;
				return HitTestResultBehavior.Stop;
			},
			new PointHitTestParameters(position));
			//if (r == null) return null;

			//DependencyObject obj = r.VisualHit;
			while (!(obj is C) && (obj != null)) {
				obj = VisualTreeHelper.GetParent(obj);

				if (obj is I)
					return obj as I;
			}

			return null;
		}

		/// <summary> Return whether mouse over an selected item. </summary>
		/// <param name="lbSender"> </param>
		/// <returns> </returns>
		public static bool IsMouseOverSelectedItem(ListBox lbSender) {
			ListBoxItem _selectedItem = GetListBoxItem(lbSender, Mouse.GetPosition(lbSender));
			if (_selectedItem != null && lbSender.SelectedItem != null)
				return lbSender.SelectedItems.Contains(lbSender.ItemContainerGenerator.ItemFromContainer(_selectedItem));
			return false;
		}

		/// <summary> Return whether mouse over an selected item. </summary>
		/// <param name="lbSender"> </param>
		/// <returns> </returns>
		public static bool IsMouseOverSelectedItem(TreeView tvSender) {
			TreeViewItem _selectedItem = UITools.GetTreeViewItem(tvSender, Mouse.GetPosition(tvSender));
			if (_selectedItem != null && tvSender.SelectedItem != null)
				return tvSender.SelectedItem.Equals(_selectedItem.DataContext);
			return false;
		}

		public static bool IsMouseOverSelectedItem(FrameworkElement control) {
			if (control is TreeView)
				return IsMouseOverSelectedItem(control as TreeView);
			else if (control is ListBox)
				return IsMouseOverSelectedItem(control as ListBox);
			return true;
		}

		/// <summary> Return whether mouse over scroll bar </summary>
		/// <param name="sender"> </param>
		/// <param name="e">      </param>
		/// <returns> </returns>
		public static bool IsMouseOverScrollbar(UIElement sender) {
			Point ptMouse = Mouse.GetPosition(sender);
			HitTestResult res = VisualTreeHelper.HitTest(sender, ptMouse);
			if (res == null)
				return false;

			DependencyObject depObj = res.VisualHit;
			while (depObj != null) {
				if (depObj is ScrollBar)
					return true;

				// VisualTreeHelper works with objects of type Visual or Visual3D. If the current
				// object is not derived from Visual or Visual3D, then use the LogicalTreeHelper to
				// find the parent element.
				if (depObj is Visual || depObj is System.Windows.Media.Media3D.Visual3D)
					depObj = VisualTreeHelper.GetParent(depObj);
				else
					depObj = LogicalTreeHelper.GetParent(depObj);
			}

			return false;
		}

		/// <summary> Given an element inside a treeViewItem, return it's parent treeviewitem </summary>
		/// <param name="obj"> </param>
		/// <returns> </returns>
		public static TreeViewItem GetParentTreeViewItem(FrameworkElement obj) {
			FrameworkElement temp = obj;
			while (!(temp is TreeViewItem) && (temp != null))
				temp = (FrameworkElement)VisualTreeHelper.GetParent(temp);

			return (TreeViewItem)temp;
		}

		/// <summary> Given a DragEventArgs, find out the "highlighted" treeviewitem. </summary>
		/// <param name="e"> </param>
		/// <returns> </returns>
		public static TreeViewItem GetSelectedTreeViewItem(DragEventArgs e) {
			HitTestResult htr = VisualTreeHelper.HitTest((Visual)e.Source,
					e.GetPosition((IInputElement)e.Source));
			return GetParentTreeViewItem((FrameworkElement)htr.VisualHit);
		}

		public static ListBoxItem GetSelectedListBoxItem(Visual sender, Point position) {
			DependencyObject obj = null;
			//Bug#66
			VisualTreeHelper.HitTest(
			sender,
			(o) => {
				if (UITools.FindAncestor<ListBoxItem>(o) != null)
					return HitTestFilterBehavior.Continue;
				else return HitTestFilterBehavior.ContinueSkipSelf;
			},
			(r) => {
				obj = r.VisualHit;
				return HitTestResultBehavior.Stop;
			},
			new PointHitTestParameters(position));
			//if (r == null) return null;

			//DependencyObject obj = r.VisualHit;
			while (!(obj is ListBox) && (obj != null)) {
				obj = VisualTreeHelper.GetParent(obj);

				if (obj is ListBoxItem)
					return obj as ListBoxItem;
			}

			return null;
		}

		/// <summary> Reset scrollbar position of virtualizing panel if scrollBar becomes invisible </summary>
		/// <param name="listView"> </param>
		public static void HandleScrollBarInvisible(this ListView listView) {
			ScrollViewer scrollView = UITools.FindVisualChild<ScrollViewer>(listView);

			DependencyPropertyDescriptor vertScrollbarVisibilityDescriptor =
					DependencyPropertyDescriptor.FromProperty(
					ScrollViewer.ComputedVerticalScrollBarVisibilityProperty, typeof(ScrollViewer));

			vertScrollbarVisibilityDescriptor.AddValueChanged(scrollView, delegate {
				if (scrollView.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Collapsed) {
					VirtualizingPanel panel = UITools.FindVisualChild<VirtualizingPanel>(listView);
					if (panel is IScrollInfo) {
						(panel as IScrollInfo).SetVerticalOffset(0);
					}
				}
			});

			DependencyPropertyDescriptor horzScrollbarVisibilityDescriptor =
					DependencyPropertyDescriptor.FromProperty(
					ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty, typeof(ScrollViewer));

			horzScrollbarVisibilityDescriptor.AddValueChanged(scrollView, delegate {
				if (scrollView.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Collapsed) {
					VirtualizingPanel panel = UITools.FindVisualChild<VirtualizingPanel>(listView);
					if (panel is IScrollInfo) {
						(panel as IScrollInfo).SetHorizontalOffset(0);
					}
				}
			});
		}
	}
}