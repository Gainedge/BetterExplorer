///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// LYCJ (c) 2010 - http://www.quickzip.org/components                                                            //
// Release under MIT license.                                                                                   //
//                                                                                                               //
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;

namespace BetterExplorerControls
{
    public class BreadcrumbItem : HeaderedItemsControl
    {
        static BreadcrumbItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbItem), new FrameworkPropertyMetadata(typeof(BreadcrumbItem)));
        }

        public BreadcrumbItem(bool isTopLevel = false)
        {
            _isTopLevel = isTopLevel;
            //this.Loaded += delegate { _loaded = true; };
            
        }


        #region Unused code
        //public void raiseShowCaptionEvent(bool value)
        //{
        //    //Fix:69: http://social.msdn.microsoft.com/forums/en-US/wpf/thread/6ec60f31-5a6f-486e-a4ac-309505987735/
        //    //sometimes that element genuinely isn't there yet. (after loaded event / BreadcrumbItem.cs)
        //    try
        //    {
        //        if (value)
        //        {
                    
        //                RaiseEvent(new RoutedEventArgs(ShowingCaptionEvent));
        //        }
        //        else
        //        {

        //                RaiseEvent(new RoutedEventArgs(HidingCaptionEvent));
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}
        public static void OnShowCaptionChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            //if (args.NewValue != args.OldValue)
            //{
            //    BreadcrumbItem item = (BreadcrumbItem)sender;
            //    bool newShowCaption = (bool)args.NewValue;
            //    if (item.ShowCaption != newShowCaption)
            //        if (item._loaded)
            //        {
            //            item.raiseShowCaptionEvent(newShowCaption);
            //        }
            //        else
            //        {
            //            RoutedEventHandler action = null;
            //            action = (RoutedEventHandler)delegate
            //            {
            //                item.Loaded -= action;
            //                if (!item._showCaptionHandled && newShowCaption)
            //                    item.raiseShowCaptionEvent(newShowCaption);
            //                item._showCaptionHandled = true;
            //            };
            //            item.Loaded += action;
            //        }
            //}
        }
        #endregion

        protected override DependencyObject GetContainerForItemOverride()
        {
            BreadcrumbItem retVal = new BreadcrumbItem(false)
            {
                HeaderTemplate = HeaderTemplate,
                IconTemplate = IconTemplate,
            };
            retVal.ShowToggle = false;
            return retVal;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SetValue(IsTopLevelProperty, _isTopLevel);
            SetValue(ShowIconProperty, !_isTopLevel);
            //if (!ShowCaption)
            //    raiseShowCaptionEvent(ShowCaption);

           
            //When clicked, raise selected.
            this.AddHandler(Button.ClickEvent, (RoutedEventHandler)delegate(object sender, RoutedEventArgs args)
            {
                if ((args.OriginalSource is Button))
                    RaiseEvent(new RoutedEventArgs(SelectedEvent));
                args.Handled = true;
            });

            //When selected, close drop down.
            this.AddHandler(BreadcrumbItem.SelectedEvent, (RoutedEventHandler)delegate(object sender, RoutedEventArgs args)
            {
                this.SetValue(IsDropDownOpenProperty, false); 
            });

        }

        public static void OnIsTopLevelChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as BreadcrumbItem).SetValue(ShowIconProperty, !(bool)args.NewValue);
        }

        public static void OnIsOverflowedChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            (sender as BreadcrumbItem).SetValue(ShowIconProperty, (bool)args.NewValue);
        }


        #region Public Properties       

        #region Events

        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent("Selected",
          RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbItem));

        /// <summary>
        /// The current item is clicked.
        /// </summary>
        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }

        //Animation related event - unused.
        //public static readonly RoutedEvent ShowingCaptionEvent = EventManager.RegisterRoutedEvent("ShowingCaption",
        //    RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbItem));

        //public event RoutedEventHandler ShowingCaption
        //{
        //    add { AddHandler(ShowingCaptionEvent, value); }
        //    remove { RemoveHandler(ShowingCaptionEvent, value); }
        //}

        //public static readonly RoutedEvent HidingCaptionEvent = EventManager.RegisterRoutedEvent("HidingCaption",
        //    RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbItem));

        //public event RoutedEventHandler HidingCaption
        //{
        //    add { AddHandler(HidingCaptionEvent, value); }
        //    remove { RemoveHandler(HidingCaptionEvent, value); }
        //}
        #endregion

        #region ShowCaption, Toggle, Icon

        public static readonly DependencyProperty ShowCaptionProperty =
                    DependencyProperty.Register("ShowCaption", typeof(bool), typeof(BreadcrumbItem),
                    new UIPropertyMetadata(true, OnShowCaptionChanged));

        /// <summary>
        /// Display Caption
        /// </summary>
        public bool ShowCaption
        {
            get { return (bool)GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        public static readonly DependencyProperty ShowToggleProperty =
                    DependencyProperty.Register("ShowToggle", typeof(bool), typeof(BreadcrumbItem),
                    new UIPropertyMetadata(true));

        /// <summary>
        /// Display Toggle
        /// </summary>
        public bool ShowToggle
        {
            get { return (bool)GetValue(ShowToggleProperty); }
            set { SetValue(ShowToggleProperty, value); }
        }

        public static readonly DependencyProperty ShowIconProperty =
                    DependencyProperty.Register("ShowIcon", typeof(bool), typeof(BreadcrumbItem),
                    new UIPropertyMetadata(false));

        /// <summary>
        /// Display Icon
        /// </summary>
        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }
        #endregion

        #region IsTopLevel, IsOverflowed, IsShadowItem(Unused), IsSeparator, IsLoading (Unused)
        public static readonly DependencyProperty IsTopLevelProperty =
                    DependencyProperty.Register("IsTopLevel", typeof(bool), typeof(BreadcrumbItem),
                    new UIPropertyMetadata(OnIsTopLevelChanged));

        /// <summary>
        /// IsTopLevel?
        /// </summary>
        public bool IsTopLevel
        {
            get { return (bool)GetValue(IsTopLevelProperty); }
            set { SetValue(IsTopLevelProperty, value); }
        }

        public static readonly DependencyProperty IsOverflowedProperty =
                   DependencyProperty.Register("IsOverflowed", typeof(bool), typeof(BreadcrumbItem),
                   new UIPropertyMetadata(false, OnIsOverflowedChanged));

        /// <summary>
        /// IsOverflowed?
        /// </summary>
        public bool IsOverflowed
        {
            get { return (bool)GetValue(IsOverflowedProperty); }
            set { SetValue(IsOverflowedProperty, value); }
        }

        public static readonly DependencyProperty IsShadowItemProperty =
                   DependencyProperty.Register("IsShadowItem", typeof(bool), typeof(BreadcrumbItem),
                   new UIPropertyMetadata(true));

        /// <summary>
        /// For 1st level BreadcrumbItem, grey color if true. 
        /// </summary>
        public bool IsShadowItem
        {
            get { return (bool)GetValue(IsShadowItemProperty); }
            set { SetValue(IsShadowItemProperty, value); }
        }

        public static readonly DependencyProperty IsSeparatorProperty =
                   DependencyProperty.Register("IsSeparator", typeof(bool), typeof(BreadcrumbItem),
                   new UIPropertyMetadata(false));

        /// <summary>
        /// Display separator, use for 2nd level BreadcrumbItem only.
        /// </summary>
        public bool IsSeparator
        {
            get { return (bool)GetValue(IsSeparatorProperty); }
            set { SetValue(IsSeparatorProperty, value); }
        }


        public static readonly DependencyProperty IsLoadingProperty =
                  DependencyProperty.Register("IsLoading", typeof(bool), typeof(BreadcrumbItem),
                  new UIPropertyMetadata(false));

        /// <summary>
        /// Display separator, use for 2nd level BreadcrumbItem only.
        /// </summary>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }


        #endregion

        #region IsDropDownOpen

        public static readonly DependencyProperty IsDropDownOpenProperty =
           ComboBox.IsDropDownOpenProperty.AddOwner(typeof(BreadcrumbItem),
           new PropertyMetadata(false));

        /// <summary>
        /// Is current dropdown (combobox) opened
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

      
        #endregion

        #region IconTemplate

        public static readonly DependencyProperty IconTemplateProperty =
            DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(BreadcrumbItem));

        /// <summary>
        /// DataTemplate for display the icon.
        /// </summary>
        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }

        #endregion

        #endregion


        #region Data

        //HotTrack _headerHL;
        //bool _loaded = false;
        //bool _showCaptionHandled = false;
        bool _isTopLevel = false;

        #endregion
    }
}
