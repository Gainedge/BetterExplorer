using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Wpf.Controls
{
    /// <summary>
    /// TabPanel
    /// </summary>
    public class TabPanel : Panel, IScrollInfo
    {
        private int _maxVisibleItems;
//        private double _maxChildWidthOrHeight;
        private readonly List<Rect> _childRects;
        private Dock _tabStripPlacement;

        // scrolling
        private readonly TranslateTransform _translateTransform = new TranslateTransform();
        private Size _extent = new Size(0, 0);
        private Size _oldExtent = new Size(0, 0);
        private Size _viewPort = new Size(0, 0);
        private Size _lastSize = new Size(0, 0);
        private Point _offset = new Point(0, 0);
        private ScrollViewer _scrollOwner;

        private int _firstVisibleIndex;
//        private int _childCount;

        public TabPanel()
        {
            _childRects = new List<Rect>(4);
            base.RenderTransform = _translateTransform;
        }

        #region CLR Properties
        private Dock TabStripPlacement
        {
            get
            {
                Dock dock = Dock.Top;
                TabControl templatedParent = base.TemplatedParent as TabControl;
                if (templatedParent != null)
                {
                    dock = templatedParent.TabStripPlacement;

                }
                return dock;
            }
        }
        private double MinimumChildWidth
        {
            get
            {
                TabControl templatedParent = base.TemplatedParent as TabControl;
                if (templatedParent != null)
                    return templatedParent.TabItemMinWidth;
                return 0;
            }
        }
        private double MinimumChildHeight
        {
            get
            {
                TabControl templatedParent = base.TemplatedParent as TabControl;
                if (templatedParent != null)
                    return templatedParent.TabItemMinHeight;
                return 0;
            }
        }
        private double MaximumChildWidth
        {
            get
            {
                TabControl templatedParent = base.TemplatedParent as TabControl;
                if (templatedParent != null)
                    return templatedParent.TabItemMaxWidth;
                return double.PositiveInfinity;
            }
        }
        private double MaximumChildHeight
        {
            get
            {
                TabControl templatedParent = base.TemplatedParent as TabControl;
                if (templatedParent != null)
                    return templatedParent.TabItemMaxHeight;
                return double.PositiveInfinity;
            }
        }

        private int FirstVisibleIndex
        {
            get { return _firstVisibleIndex; }
            set
            {
                if (_firstVisibleIndex == value)
                    return;
                
                if (value < 0)
                {
                    _firstVisibleIndex = 0;
                    return;
                }
                
                _firstVisibleIndex = value;
                if (LastVisibleIndex > InternalChildren.Count - 1)
                    FirstVisibleIndex--;
            }
        }

        private int LastVisibleIndex
        {
            get { return _firstVisibleIndex + _maxVisibleItems - 1; }
        }
        #endregion

        #region Dependancy Properties

        /// <summary>
        /// CanScrollLeftOrUp Dependancy Property
        /// </summary>
        [Browsable(false)]
        internal bool CanScrollLeftOrUp
        {
            get { return (bool)GetValue(CanScrollLeftOrUpProperty); }
            set { SetValue(CanScrollLeftOrUpProperty, value); }
        }
        internal static readonly DependencyProperty CanScrollLeftOrUpProperty = DependencyProperty.Register("CanScrollLeftOrUp", typeof(bool), typeof(TabPanel), new UIPropertyMetadata(false));

        /// <summary>
        /// CanScrollRightOrDown Dependancy Property
        /// </summary>
        [Browsable(false)]
        internal bool CanScrollRightOrDown
        {
            get { return (bool)GetValue(CanScrollRightOrDownProperty); }
            set { SetValue(CanScrollRightOrDownProperty, value); }
        }
        internal static readonly DependencyProperty CanScrollRightOrDownProperty = DependencyProperty.Register("CanScrollRightOrDown", typeof(bool), typeof(TabPanel), new UIPropertyMetadata(false));

        #endregion

        #region MeasureOverride
        /// <summary>
        /// Measure Override
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            _viewPort = availableSize;
            _tabStripPlacement = TabStripPlacement;

            switch (_tabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    return MeasureHorizontal(availableSize);

                case Dock.Left:
                case Dock.Right:
                    return MeasureVertical(availableSize);
            }

            return new Size();
        }
        #endregion

        #region MeasureHorizontal
        /// <summary>
        /// Measure the tab items for docking at the top or bottom
        /// </summary>
        private Size MeasureHorizontal(Size availableSize)
        {
            double maxChildWidthOrHeight = 0d;
            int childCount = InternalChildren.Count;
            EnsureChildRects();

            double extentWidth = 0d;
            double[] widths = new double[childCount];  // stores the widths of the items for use in the arrange pass

            for (int i = 0; i < childCount; i++)
            {
                TabItem tabItem = InternalChildren[i] as TabItem;
                if (tabItem == null) return new Size();

                SetDimensions(tabItem);

                tabItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                ClearDimensions(tabItem);

                // calculate the maximum child height
                maxChildWidthOrHeight = Math.Max(maxChildWidthOrHeight, Math.Ceiling(tabItem.DesiredSize.Height));

                // calculate the child width while respecting the Maximum & Minimum width constraints
                widths[i] = Math.Min(MaximumChildWidth, Math.Max(MinimumChildWidth, Math.Ceiling(tabItem.DesiredSize.Width)));

                // determines how much horizontal space we require
                extentWidth += widths[i];
            }
            maxChildWidthOrHeight = Math.Max(MinimumChildHeight, Math.Min(MaximumChildHeight, maxChildWidthOrHeight));  // observe the constraints
            _extent = new Size(extentWidth, maxChildWidthOrHeight);

            bool flag = false;
            // 1). all the children fit into the available space using there desired widths
            if (extentWidth <= availableSize.Width)
            {
                _maxVisibleItems = childCount;
                FirstVisibleIndex = 0;

                double left = 0;
                for (int i = 0; i < childCount; i++)
                {
                    _childRects[i] = new Rect(left, 0, widths[i], maxChildWidthOrHeight);
                    left += widths[i];

                    FrameworkElement child = InternalChildren[i] as FrameworkElement;
                    if (child != null) child.Measure(new Size(widths[i], maxChildWidthOrHeight));
                }

                CanScrollLeftOrUp = false;
                CanScrollRightOrDown = false;

                flag = true;
            }

            // 2). all the children fit in the available space if we reduce their widths to a uniform value 
            // while staying within the MinimumChildWidth and MaximumChildWidth constraints
            if (!flag)
            {
                // make sure the width is not greater than the MaximumChildWidth constraints
                double targetWidth = Math.Min(MaximumChildWidth, availableSize.Width / childCount);

                // target width applies now if whether we can fit all items in the available space or whether we are scrolling
                if (targetWidth >= MinimumChildWidth)
                {
                    _maxVisibleItems = childCount;
                    FirstVisibleIndex = 0;

                    extentWidth = 0;
                    double left = 0;

                    for (int i = 0; i < childCount; i++)
                    {
                        extentWidth += targetWidth;
                        widths[i] = targetWidth;
                        _childRects[i] = new Rect(left, 0, widths[i], maxChildWidthOrHeight);
                        left += widths[i];

                        FrameworkElement child = InternalChildren[i] as FrameworkElement;
                        if (child != null) child.Measure(new Size(widths[i], maxChildWidthOrHeight));
                    }
                    _extent = new Size(extentWidth, maxChildWidthOrHeight);

                    flag = true;

                    CanScrollLeftOrUp = false;
                    CanScrollRightOrDown = false;
                }
            }

            // 3) we can not fit all the children in the viewport, so now we will enable scrolling/virtualizing items
            if (!flag)
            {
                _maxVisibleItems = (int)Math.Floor(_viewPort.Width / MinimumChildWidth);            // calculate how many visible children we can show at once
                if (_maxVisibleItems == 0)
                    _maxVisibleItems = 1;

                double targetWidth = availableSize.Width / _maxVisibleItems;                        // calculate the new target width
                FirstVisibleIndex = _firstVisibleIndex;

                extentWidth = 0;
                double left = 0;
                for (int i = 0; i < childCount; i++)
                {
                    extentWidth += targetWidth;
                    widths[i] = targetWidth;

                    _childRects[i] = new Rect(left, 0, widths[i], maxChildWidthOrHeight);
                    left += widths[i];


                    FrameworkElement child = InternalChildren[i] as FrameworkElement;
                    if (child != null) child.Measure(new Size(widths[i], maxChildWidthOrHeight));
                }
                _extent = new Size(extentWidth, maxChildWidthOrHeight);

                CanScrollLeftOrUp = LastVisibleIndex < childCount - 1;
                CanScrollRightOrDown = FirstVisibleIndex > 0;
            }

            return new Size(double.IsInfinity(availableSize.Width) ? _extent.Width : availableSize.Width, maxChildWidthOrHeight);
        }

        #endregion

        #region MeasureVertical

        /// <summary>
        /// Measure the tab items for docking at the left or right
        /// </summary>
        private Size MeasureVertical(Size availableSize)
        {
            int childCount = InternalChildren.Count;
            double maxChildWidthOrHeight = 0d;

            EnsureChildRects();

            double extentHeight = 0d;
            double[] heights = new double[childCount];

            // we will first measure all the children with unlimited space to get their desired sizes
            // this will also get us the height required for all TabItems
            for (int i = 0; i < childCount; i++)
            {
                TabItem tabItem = InternalChildren[i] as TabItem;
                if (tabItem == null) return new Size();

                SetDimensions(tabItem);

                tabItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                ClearDimensions(tabItem);

                // calculate the maximum child width
                maxChildWidthOrHeight = Math.Max(maxChildWidthOrHeight, Math.Ceiling(tabItem.DesiredSize.Width));

                // calculate the child width while respecting the Maximum & Minimum width constraints
                heights[i] = Math.Min(MaximumChildHeight, Math.Max(MinimumChildHeight, Math.Ceiling(tabItem.DesiredSize.Height)));

                // determines how much horizontal space we require
                extentHeight += heights[i];
            }
            maxChildWidthOrHeight = Math.Max(MinimumChildWidth, Math.Min(MaximumChildWidth, maxChildWidthOrHeight));  // observe the constraints
            _extent = new Size(maxChildWidthOrHeight, extentHeight);

            bool flag = false;
            // 1). all the children fit into the available space using there desired widths
            if (extentHeight <= availableSize.Height)
            {
                _maxVisibleItems = childCount;
                FirstVisibleIndex = 0;

                double top = 0;
                for (int i = 0; i < childCount; i++)
                {
                    _childRects[i] = new Rect(0, top, maxChildWidthOrHeight, heights[i]);
                    top += heights[i];

                    FrameworkElement child = InternalChildren[i] as FrameworkElement;
                    if (child != null) child.Measure(new Size(maxChildWidthOrHeight, heights[i]));
                }

                CanScrollLeftOrUp = false;
                CanScrollRightOrDown = false;

                flag = true;
            }

            // 2). all the children fit in the available space if we reduce their widths to a uniform value 
            // while staying within the MinimumChildWidth and MaximumChildWidth constraints
            if (!flag)
            {
                // make sure the width is not greater than the MaximumChildWidth constraints
                double targetHeight = Math.Min(MaximumChildHeight, availableSize.Height / childCount);

                // target width applies now if whether we can fit all items in the available space or whether we are scrolling
                if (targetHeight >= MinimumChildHeight)
                {
                    _maxVisibleItems = childCount;
                    FirstVisibleIndex = 0;

                    extentHeight = 0;
                    double top = 0;

                    for (int i = 0; i < childCount; i++)
                    {
                        extentHeight += targetHeight;
                        heights[i] = targetHeight;
                        _childRects[i] = new Rect(0, top, maxChildWidthOrHeight, heights[i]);
                        top += heights[i];

                        FrameworkElement child = InternalChildren[i] as FrameworkElement;
                        if (child != null) child.Measure(new Size(maxChildWidthOrHeight, heights[i]));
                    }
                    _extent = new Size(maxChildWidthOrHeight, extentHeight);

                    flag = true;

                    CanScrollLeftOrUp = false;
                    CanScrollRightOrDown = false;
                }
            }

            // 3) we can not fit all the children in the viewport, so now we will enable scrolling/virtualizing items
            if (!flag)
            {
                _maxVisibleItems = (int)Math.Floor(_viewPort.Height / MinimumChildHeight);          // calculate how many visible children we can show at once
                double targetHeight = availableSize.Height / _maxVisibleItems;                      // calculate the new target width
                FirstVisibleIndex = _firstVisibleIndex;

                extentHeight = 0;
                double top = 0;
                for (int i = 0; i < childCount; i++)
                {
                    extentHeight += targetHeight;
                    heights[i] = targetHeight;
                    _childRects[i] = new Rect(0, top, maxChildWidthOrHeight, heights[i]);
                    top += heights[i];

                    FrameworkElement child = InternalChildren[i] as FrameworkElement;
                    if (child != null) child.Measure(new Size(maxChildWidthOrHeight, heights[i]));
                }
                _extent = new Size(maxChildWidthOrHeight, extentHeight);

                CanScrollLeftOrUp = LastVisibleIndex < childCount - 1;
                CanScrollRightOrDown = FirstVisibleIndex > 0;
            }

            return new Size(maxChildWidthOrHeight, double.IsInfinity(availableSize.Height) ? _extent.Height : availableSize.Height);

        }
        #endregion

        #region ArrangeOverride

        /// <summary>
        /// Arrange Override
        /// </summary>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // monitors changes to the ScrollViewer extent value
            if (_oldExtent != _extent)
            {
                _oldExtent = _extent;
                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }

            // monitors changes to the parent container size, (ie window resizes)
            if (finalSize != _lastSize)
            {
                _lastSize = finalSize;
                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();
            }

            // monitor scrolling being removed
            bool invalidateMeasure = false;
            if (_extent.Width <= _viewPort.Width && _offset.X > 0)
            {
                _offset.X = 0;
                _translateTransform.X = 0;

                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();

                invalidateMeasure = true;
            }
            if (_extent.Height <= _viewPort.Height && _offset.Y > 0)
            {
                _offset.Y = 0;
                _translateTransform.Y = 0;

                if (_scrollOwner != null)
                    _scrollOwner.InvalidateScrollInfo();

                invalidateMeasure = true;
            }
            if (invalidateMeasure)
                InvalidateMeasure();

            

            // arrange the children
            for (var i = 0; i < InternalChildren.Count; i++)
            {
                InternalChildren[i].Arrange(_childRects[i]);
            }

            // we need these lines as when the Scroll Buttons get Shown/Hidden,
            // the _offset value gets out of line, this will ensure that our scroll position stays in line
            if (InternalChildren.Count > 0)
            {
                _offset = _childRects[FirstVisibleIndex].TopLeft;
                _translateTransform.X = -_offset.X;
                _translateTransform.Y = -_offset.Y;
            }

            return finalSize;
        }
        #endregion

        private void EnsureChildRects()
        {
            while (InternalChildren.Count > _childRects.Count)
                _childRects.Add(new Rect());
        }

        #region IScrollInfo Members

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        public void LineDown()
        {
            LineRight();
        }

        public void LineLeft()
        {
            // this works because we can guarantee that when we are in scroll mode, 
            // there will be children, and they will all be of equal size
            FirstVisibleIndex++;

            if (_tabStripPlacement == Dock.Top || _tabStripPlacement == Dock.Bottom)
                SetHorizontalOffset(HorizontalOffset + _childRects[0].Width);
            else
                SetVerticalOffset(HorizontalOffset + _childRects[0].Height);
        }

        public void LineRight()
        {
            FirstVisibleIndex--;

            if (_tabStripPlacement == Dock.Top || _tabStripPlacement == Dock.Bottom)
                SetHorizontalOffset(HorizontalOffset - _childRects[0].Width);
            else
                SetVerticalOffset(HorizontalOffset - _childRects[0].Height);
        }

        public void LineUp()
        {
            LineLeft();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            InvalidateMeasure();
            UpdateLayout();

            TabControl ic = ItemsControl.GetItemsOwner(this) as TabControl;
            if (ic == null) return Rect.Empty;

            int index = -1;
            var tabsCount = ic.GetTabsCount();
            for (int i = 0; i < tabsCount; i++)
            {
                if (visual.Equals(ic.GetTabItem(i)))
                {
                    index = i;
                    break;
                }
            }
            if (index > -1)
            {
                if (index < FirstVisibleIndex)
                    FirstVisibleIndex = index;
                else if (index > LastVisibleIndex)
                {
                    while (index > LastVisibleIndex)
                        FirstVisibleIndex++;
                }

                InvalidateArrange();
            }

            return Rect.Empty;
        }

        public void MouseWheelDown()
        {
            LineDown();
        }

        public void MouseWheelLeft()
        {
            LineLeft();
        }

        public void MouseWheelRight()
        {
            LineRight();
        }

        public void MouseWheelUp()
        {
            LineUp();
        }

        public void PageDown()
        {
            throw new NotImplementedException();
        }

        public void PageLeft()
        {
            throw new NotImplementedException();
        }

        public void PageRight()
        {
            throw new NotImplementedException();
        }

        public void PageUp()
        {
            throw new NotImplementedException();
        }

        public ScrollViewer ScrollOwner
        {
            get { return _scrollOwner; }
            set { _scrollOwner = value; }
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || _viewPort.Width >= _extent.Width)
                offset = 0;
            else
            {
                if (offset + _viewPort.Width > _extent.Width)
                    offset = _extent.Width - _viewPort.Width;
            }

            _offset.X = offset;
            if (_scrollOwner != null)
                _scrollOwner.InvalidateScrollInfo();

            _translateTransform.X = -offset;

            InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || _viewPort.Height >= _extent.Height)
                offset = 0;
            else
            {
                if (offset + _viewPort.Height > _extent.Height)
                    offset = _extent.Height - _viewPort.Height;
            }

            _offset.Y = offset;
            if (_scrollOwner != null)
                _scrollOwner.InvalidateScrollInfo();

            _translateTransform.Y = -offset;

            InvalidateMeasure();
        }

        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        public double ViewportHeight
        {
            get { return _viewPort.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewPort.Width; }
        }

        #endregion

        #region Helpers

        private static void SetDimensions(TabItem tabItem)
        {
            if (tabItem.Dimension == null)
            {
                // store the original size specifications of the tab
                tabItem.Dimension =
                    new Dimension
                        {
                            Height = tabItem.Height,
                            Width = tabItem.Width,
                            MaxHeight = tabItem.MaxHeight,
                            MaxWidth = tabItem.MaxWidth,
                            MinHeight = tabItem.MinHeight,
                            MinWidth = tabItem.MinWidth
                        };
            }
            else
            {
                // restore the original values for the tab
                tabItem.BeginInit();
                tabItem.Height = tabItem.Dimension.Height;
                tabItem.Width = tabItem.Dimension.Width;
                tabItem.MaxHeight = tabItem.Dimension.MaxHeight;
                tabItem.MaxWidth = tabItem.Dimension.MaxWidth;
                tabItem.MinHeight = tabItem.Dimension.MinHeight;
                tabItem.MinWidth = tabItem.Dimension.MinWidth;
                tabItem.EndInit();
            }
        }

        private static void ClearDimensions(FrameworkElement tabItem)
        {
            // remove any size restrictions from the Header,
            // this is because the TabControl's size restriction properties takes precedence over
            // the individual tab items
            // eg, if the TabControl sets the TabItemMaxWidth property to 300, but the Header
            // has a minWidth of 400, the TabControls value of 300 should be used
            tabItem.BeginInit();
            tabItem.Height = double.NaN;
            tabItem.Width = double.NaN;
            tabItem.MaxHeight = double.PositiveInfinity;
            tabItem.MaxWidth = double.PositiveInfinity;
            tabItem.MinHeight = 0;
            tabItem.MinWidth = 0;
            tabItem.EndInit();
        }

        #endregion
    }
}
