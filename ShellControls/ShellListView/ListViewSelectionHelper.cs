using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ShellControls.ShellListView;
public class ListViewSelectionHelper : FrameworkElement
    {
        #region MultiSelect
        public static readonly DependencyProperty MultiSelectProperty
            = DependencyProperty.RegisterAttached("MultiSelect",
                typeof(bool), typeof(ListViewSelectionHelper),
                new PropertyMetadata(new PropertyChangedCallback(OnMultiSelectInvalidated)));

        public static bool GetMultiSelect(DependencyObject sender)
        {
            return (bool)sender.GetValue(MultiSelectProperty);
        }

        public static void SetMultiSelect(DependencyObject sender, bool value)
        {
            sender.SetValue(MultiSelectProperty, value);
        }
        #endregion
        #region PreviewDrag
        public static readonly DependencyProperty PreviewDragProperty
            = DependencyProperty.RegisterAttached("PreviewDrag",
                typeof(bool), typeof(ListViewSelectionHelper));

        public static bool GetPreviewDrag(DependencyObject sender)
        {
            return (bool)sender.GetValue(PreviewDragProperty);
        }

        public static void SetPreviewDrag(DependencyObject sender, bool value)
        {
            sender.SetValue(PreviewDragProperty, value);
        }
        #endregion
        #region isDragging
        protected static readonly DependencyProperty IsDraggingProperty
            = DependencyProperty.RegisterAttached("isDragging",
                typeof(bool), typeof(ListViewSelectionHelper));

        protected static bool GetIsDragging(DependencyObject sender)
        {
            return (bool)sender.GetValue(IsDraggingProperty);
        }

        protected static void SetIsDragging(DependencyObject sender, bool value)
        {
            sender.SetValue(IsDraggingProperty, value);
            //if (!value)
            //{
            //    _lastPos = _initialPoint;
            //    _startPos = _initialPoint;

            //    ListView lvSender = (ListView)sender;
            //    if (GetPreviewDrag(lvSender))
            //        lvSender.Background = new SolidColorBrush(GetBackground(lvSender));
            //}
        }
        #endregion
        #region isMousePressed
        protected static readonly DependencyProperty IsMousePressedProperty
            = DependencyProperty.RegisterAttached("isMousePressed",
                typeof(bool), typeof(ListViewSelectionHelper));

        protected static bool GetIsMousePressed(DependencyObject sender)
        {
            return (bool)sender.GetValue(IsMousePressedProperty);
        }

        protected static void SetIsMousePressed(DependencyObject sender, bool value)
        {
            sender.SetValue(IsMousePressedProperty, value);
            if (!value)
            {
                _lastPos = _initialPoint;
                _startPos = _initialPoint;

                ListView lvSender = (ListView)sender;
                if (GetPreviewDrag(lvSender))
                    lvSender.Background = new SolidColorBrush(GetBackground(lvSender));
            }
        }
        #endregion
        #region Background
        protected static readonly DependencyProperty BackgroundProperty
            = DependencyProperty.RegisterAttached("Background",
                typeof(Color), typeof(ListViewSelectionHelper));

        protected static Color GetBackground(DependencyObject sender)
        {
            return (Color)sender.GetValue(BackgroundProperty);
        }

        protected static void SetBackground(DependencyObject sender, Color value)
        {
            sender.SetValue(BackgroundProperty, value);
        }
        #endregion

        private static List<ListViewItem> _selectedItem = new List<ListViewItem>();
        //Fix: 08-15-08 PreviewDrag window shown in wrong spot when start dragging (before mouse start moving)
        private static Point _initialPoint = new Point(-1, -1);
        private static Point _startPos = _initialPoint;
        private static Point _lastPos = _initialPoint;
        private static ListViewItem _itemUnderMouse = null;
        private static bool _itemAlreadySelected = false;
        private static object lockObj = new object();

        private static void OnMultiSelectInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)dependencyObject;

            if (!(element is ListView))
                throw new ArgumentException("Element not ListView");

            ListView lvElement = (ListView)element;
            if ((bool)e.NewValue == true)
            {
                SetMultiSelect(element, true);
                if (lvElement.Background is SolidColorBrush)
                    SetBackground(lvElement, (lvElement.Background as SolidColorBrush).Color);
                else SetPreviewDrag(lvElement, false);

                //Fix: 05-02-08 Multiple ---> Extended, or Shift+Select not work as intended.
                lvElement.SelectionMode = SelectionMode.Extended;
                lvElement.MouseDown += new MouseButtonEventHandler(lvElement_MouseDown);
                lvElement.PreviewMouseDown += new MouseButtonEventHandler(lvElement_PreviewMouseDown);
                lvElement.PreviewMouseMove += new MouseEventHandler(lvElement_PreviewMouseMove);
                lvElement.PreviewMouseUp += new MouseButtonEventHandler(lvElement_PreviewMouseUp);

                lvElement.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(lvElement_ScrollChanged));         
                //lvElement.IsDeferredScrollingEnabled = false;

            }
            else
            {
                SetMultiSelect(element, false);
                lvElement.PreviewMouseDown -= new MouseButtonEventHandler(lvElement_PreviewMouseDown);
                lvElement.PreviewMouseMove -= new MouseEventHandler(lvElement_PreviewMouseMove);
                lvElement.PreviewMouseUp -= new MouseButtonEventHandler(lvElement_PreviewMouseUp);
                lvElement.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(lvElement_ScrollChanged));

            }
        }


        static ListViewItem getSelectedItem(ListView lvSender, Point position)
        {
            HitTestResult r = VisualTreeHelper.HitTest(lvSender, position);
            if (r == null) return null;

            DependencyObject obj = r.VisualHit;
            while (!(obj is ListView) && (obj != null))
            {
                obj = VisualTreeHelper.GetParent(obj);

                if (obj is ListViewItem)
                    return obj as ListViewItem;
            }

            return null;
        }

        static bool _updatingSelectedItem = false;
        static void updateSelection(ListView lvSender, Point current)
        {
            //if (Point.Subtract(current, _lastPos).Length < 5)
            //    return;

            if (lvSender.SelectionMode == SelectionMode.Single)
                return;

            lock (lockObj)
            {
                Rect selectRect = new Rect(_startPos, current);
                Rect unselectRect = new Rect(_startPos, _lastPos);
                _lastPos = current;

                //Unselect all visible selected items (by using _lastPos) no matter it's current selected or not.
                VisualTreeHelper.HitTest(lvSender, UnselectHitTestFilterFunc,
                    new HitTestResultCallback(SelectResultCallback),
                    new GeometryHitTestParameters(new RectangleGeometry(unselectRect)));

                //Select all visible items in select region.
                VisualTreeHelper.HitTest(lvSender, SelectHitTestFilterFunc,
                    new HitTestResultCallback(SelectResultCallback),
                    new GeometryHitTestParameters(new RectangleGeometry(selectRect)));

                if (!GetPreviewDrag(lvSender))
                {
                    lvSender.SelectedItems.Clear();


                    try
                    {
                        _updatingSelectedItem = true;
                        foreach (ListViewItem item in _selectedItem)
                            item.IsSelected = true;                        
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    finally
                    {
                        _updatingSelectedItem = false;
                    }
                    lvSender.Focus();
                }
                else if (_startPos != _initialPoint)
                    lvSender.Background =
                    new DrawingBrush(DrawRectangle(selectRect, lvSender.ActualWidth, lvSender.ActualHeight, GetBackground(lvSender)));
            }

        }

        
        private static void lvElement_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView lvSender = sender as ListView;
            if (GetIsDragging(lvSender))
            {                
                updateSelection(lvSender, _lastPos);                
            }

            
            //Debug.WriteLine(DateTime.Now.Second.ToString() + ": " + "Scroll");
        }


        static DependencyObject getParentViewOrViewItemOrScrollBar(DependencyObject e)
        {
            while (e != null && !(e is ListView) && !(e is ListViewItem) && !(e is ScrollBar) && !(e is GridViewColumnHeader) && !(e is Expander))
                e = VisualTreeHelper.GetParent(e);
            if (e != null)
                return e;

            return null;
        }
        static bool isOverViewOrScrollBar(DependencyObject e)
        {
            DependencyObject obj = getParentViewOrViewItemOrScrollBar(e);
            return (obj is ListView || obj is ScrollBar);
        }

        static void lvElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //ListView lvSender = sender as ListView;

            //if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1)
            //{
            //    _itemUnderMouse = getSelectedItem(lvSender, e.GetPosition(lvSender));
            //    _itemAlreadySelected = (_itemUnderMouse != null && _itemUnderMouse.IsSelected);
            //    if (!_itemAlreadySelected)
            //    {
            //        lvSender.SelectedIndex = -1;
            //        e.Handled = true;
            //    }
            //}
        }

        static void lvElement_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
            ListView lvSender = sender as ListView;


            SetIsDragging(lvSender, false);

            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                bool _dragging = true;

                _itemUnderMouse = getSelectedItem(lvSender, e.GetPosition(lvSender));
                _itemAlreadySelected = (_itemUnderMouse != null && _itemUnderMouse.IsSelected);
                string controlName = e.MouseDevice.DirectlyOver.GetType().Name;

                DependencyObject _itemOnObj = getParentViewOrViewItemOrScrollBar(e.MouseDevice.DirectlyOver as DependencyObject);
                bool _mouseOverFileList = (_itemOnObj is ScrollBar) || (_itemOnObj is GridViewColumnHeader) || (_itemOnObj is Expander);
                if (_mouseOverFileList) return;
                if (controlName == "TextBoxView") return; //Editing.

                //if ((controlName == "ScrollChrome") || (controlName == "Rectangle") || (controlName == "TextBoxView"))
                //{
                //    return;
                //}

                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                   || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
                {
                    //if (_itemAlreadySelected) _itemUnderMouse.IsSelected = true;
                    //_itemUnderMouse.Focus();                    
                    return;
                }

                if (_itemAlreadySelected)
                {
                    //_dragging = false;  //04-10-09 FIX : File List Dragging Bug                    
                    return;
                    //_itemUnderMouse.IsSelected = true; 
                    //lvSender.SelectionMode = SelectionMode.Extended; 
                }
                else
                {
                    lvSender.SelectedItem = null;

                }

                //if (_itemAlreadySelected)
                ////if (_itemUnderMouse != null)
                //{
                //    lvSender.SelectionMode = SelectionMode.Single;
                //    //_itemUnderMouse.IsSelected = true;
                //    //12-06-08 comment this line so dragcanvas can work.
                //    //e.Handled = true;
                //    return;
                //}
                //else
                //{
                //    lvSender.SelectionMode = SelectionMode.Multiple;
                //    //lvSender.SelectedItems.Clear();
                //}

                _startPos = e.MouseDevice.GetPosition(lvSender);
                SetIsMousePressed(lvSender, true);
                //e.Handled = true;
            }


        }
        
        static void lvElement_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ListView lvSender = sender as ListView;

            //if (e.LeftButton == MouseButtonState.Released)
            //{
            //    SetIsMousePressed(lvSender, false);
            //    SetIsDragging(lvSender, false);
            //}

            if (GetIsMousePressed(lvSender))
            {
                if (!GetIsDragging(lvSender))
                {
                    Point position = e.GetPosition(null);
                    if (Math.Abs(position.X - _startPos.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(position.Y - _startPos.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        SetIsDragging(lvSender, true);
                        //_lastPos = _startPos;
                        if (!_updatingSelectedItem)
                            _selectedItem.Clear();
                        lvSender.CaptureMouse();
                        e.Handled = true;
                    }
                    //if (_itemUnderMouse != null)
                    //    _itemUnderMouse.IsSelected = true;
                    //lvSender.SelectionMode = SelectionMode.Multiple;
                }
                else
                //if (GetIsDragging(lvSender))
                {
                    updateSelection(lvSender, e.MouseDevice.GetPosition(lvSender));
                    e.Handled = true;
                }
            }
            

            //SetIsDragging(lvSender, _dragging);
            //if (_dragging)
            //{
            //    _startPos = e.MouseDevice.GetPosition(lvSender);
            //    _lastPos = _startPos;
            //    if (!_updatingSelectedItem)
            //        _selectedItem.Clear();

            //    lvSender.CaptureMouse();
            //    if (_itemUnderMouse != null)
            //        _itemUnderMouse.IsSelected = true;
            //    lvSender.SelectionMode = SelectionMode.Multiple;
            //    //lvSender.SelectedItems.Clear();   
            //    e.Handled = true;
            //}
            //if (GetIsDragging(lvSender))
            //{
            //    updateSelection(lvSender, e.MouseDevice.GetPosition(lvSender));
            //}

        }

        static void lvElement_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftCtrl) ||
               Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.RightCtrl))
                return;

            ListView lvSender = sender as ListView;

            if (_itemUnderMouse != null)
            {
                if (_itemAlreadySelected)
                {
                    lvSender.SelectedItem = null;
                    lvSender.SelectionMode = SelectionMode.Single;
                }

                //_itemUnderMouse.IsSelected = true;
                //Keyboard.Focus(_itemUnderMouse);
                
            }            

            
            SetIsMousePressed(lvSender, false);            

            if (GetIsDragging(lvSender))
            {
                if (GetPreviewDrag(lvSender))
                {
                    //if (_selectedItem.Count == 0)
                    //{
                    //    ListViewItem itemUnderMouse = GetSelectedItem(lvSender, e.GetPosition(lvSender));
                    //    if (itemUnderMouse != null)
                    //        itemUnderMouse.IsSelected = true;
                    //    e.Handled = false;
                    //    return;
                    //}
                    //else
                    if (_selectedItem.Count != 0)
                    {
                        lvSender.SelectedItems.Clear();
                        try
                        {
                            _updatingSelectedItem = true;
                            foreach (ListViewItem item in _selectedItem)
                                item.IsSelected = true;
                        }
                        finally
                        {
                            _updatingSelectedItem = false;
                        }
                    }

                    
                }

                SetIsDragging(lvSender, false);
                e.Handled = true;
            }
            else if (e.MouseDevice.DirectlyOver.Focusable)
                return;

            //if (lvSender.SelectedItems.Count == 1)
            //    lvSender.Focus();
            lvSender.SelectionMode = SelectionMode.Extended;
        }

        public static HitTestResultBehavior SelectResultCallback(HitTestResult result)
        {
            return HitTestResultBehavior.Continue;
        }

        public static HitTestFilterBehavior SelectHitTestFilterFunc(DependencyObject potentialHitTestTarget)
        {
            if (potentialHitTestTarget is ListViewItem)
            {
                ListViewItem item = potentialHitTestTarget as ListViewItem;
                //item.IsSelected = true;                
                if (!_updatingSelectedItem)
                    _selectedItem.Add(item);

                return HitTestFilterBehavior.ContinueSkipChildren;
            }

            return HitTestFilterBehavior.Continue;
        }

        public static HitTestFilterBehavior UnselectHitTestFilterFunc(DependencyObject potentialHitTestTarget)
        {
            if (potentialHitTestTarget is ListViewItem)
            {
                ListViewItem item = potentialHitTestTarget as ListViewItem;
                //item.IsSelected = true;                
                if (!_updatingSelectedItem)
                    _selectedItem.Remove(item);

                return HitTestFilterBehavior.ContinueSkipChildren;
            }

            return HitTestFilterBehavior.Continue;
        }

        protected static Drawing DrawRectangle(Rect rect, double actualWidth, double actualHeight, Color background)
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                if (rect.Top > 0 && rect.Left > 0)
                {
                    drawingContext.DrawRectangle(new SolidColorBrush(background), null,
                        new Rect(0, 0, actualWidth, actualHeight));

                    Brush selectionBrush = new SolidColorBrush(SystemColors.HighlightColor);
                    selectionBrush.Opacity = 0.5;

                    double x = Math.Max(0, rect.X);
                    double y = Math.Max(0, rect.Y);
                    double width = rect.X > 0 ? rect.Width : rect.Width + rect.X;
                    double height = rect.Y > 0 ? rect.Height : rect.Height + rect.Y;

                    drawingContext.DrawRectangle(selectionBrush, new Pen(SystemColors.ActiveBorderBrush, 1.0),
                        new Rect(x, y, width, height));
                }
                return drawingGroup;
            }
        }

    }