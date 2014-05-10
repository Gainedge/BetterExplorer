///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// LYCJ (c) 2010 - http://www.quickzip.org/components                                                            //
// Release under MIT license.                                                                                   //
//                                                                                                               //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace BetterExplorerControls {
	public class BreadcrumbCorePanel : Panel {
		private Size lastfinalSize;

		protected override Size MeasureOverride(Size availableSize) {
			if (availableSize.Height == double.PositiveInfinity) availableSize = new Size(availableSize.Width, 30);
			//Size resultSize = new Size(0, 0);

			if (Children.Count > 0) {
				//Measure as horizontal stack, right to left.
				double availableWidth = availableSize.Width;
				double maxHeight = 0;
				for (int i = Children.Count - 1; i >= 0; i--) //Allocate from last to first
				{
					var current = Children[i];
					current.Measure(new Size(availableWidth, availableSize.Height));
					availableWidth -= current.DesiredSize.Width;
					maxHeight = Math.Max(maxHeight, current.DesiredSize.Height);
				}
				if (availableWidth <= 0)
					return new Size(availableSize.Width, maxHeight);
				return new Size(availableSize.Width - availableWidth + 1, availableSize.Height);

			}
			return new Size(0, 0);

		}

		#region AttachedProperties

		private BreadcrumbCore findBreadcrumbCore() {
			var bcore = UITools.FindAncestor<BreadcrumbCore>(this);
			return bcore;
		}

		private int GetLastNonVisibleIndex() {
			var bcore = findBreadcrumbCore();
			return bcore == null ? -1 : bcore.LastNonVisible;
		}


		public void SetLastNonVisibleIndex(int value) {
			var bcore = findBreadcrumbCore();
			if (bcore != null)
				bcore.SetValue(BreadcrumbCore.LastNonVisibleIndexProperty, value);
		}

		public void SetLastNonVisibleIndex() {
			var bcore = findBreadcrumbCore();
			if (bcore != null)
				bcore.SetValue(BreadcrumbCore.LastNonVisibleIndexProperty, bcore.DefaultLastNonVisibleIndex);
		}

		#endregion

		private Rect[] arrangeChildren(Size availableSize) {
			Rect[] retVal = new Rect[Children.Count];

			double curX = availableSize.Width;
			SetLastNonVisibleIndex();
			for (int i = Children.Count - 1; i >= 0; i--) //Allocate from last to first
			{
				var current = Children[i];
				if (curX > 0) {
					double startPos = curX - current.DesiredSize.Width;

					if (curX > current.DesiredSize.Width) {
						retVal[i] = new Rect(startPos, 0, current.DesiredSize.Width, availableSize.Height);
						curX -= current.DesiredSize.Width;
					}
					else //Not enough space to allocate current, recalculate the retVal
					{
						retVal[i] = new Rect(0, 0, 0, 0);
						for (int j = i; j < Children.Count; j++)
							retVal[j].Offset(-curX, 0);
						curX = 0;
						SetLastNonVisibleIndex(i);
					}
				}
				else retVal[i] = new Rect(0, 0, 0, 0);
			}
			return retVal;
		}

		protected override Size ArrangeOverride(Size finalSize) {
			lastfinalSize = finalSize;
			var rects = arrangeChildren(finalSize);
			Rect emptyRect = new Rect(0, 0, 0, 0);

			for (int i = 0; i < this.Children.Count; i++) {
				Children[i].Arrange(rects[i]);
				Children[i].SetValue(BreadcrumbItem.IsOverflowedProperty, rects[i] == emptyRect);
			}

			return finalSize;
		}
	}
}
