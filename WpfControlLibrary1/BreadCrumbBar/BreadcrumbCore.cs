///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// LYCJ (c) 2010  - http://www.quickzip.org/components                                                            //
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
using System.Collections;
using System.Windows.Input;

namespace BetterExplorerControls
{
    public interface IHierarchyHelper
    {
        /// <summary>
        /// Used to generate ItemsSource for BreadcrumbCore.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        IEnumerable<object> GetHierarchy(object item, bool includeCurrent);

        /// <summary>
        /// Generate Path from an item;
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        string GetPath(object item);

        /// <summary>
        /// Get Item from path.
        /// </summary>
        /// <param name="rootItem">RootItem or ItemSource which can be used to lookup from.</param>
        /// <param name="path"></param>
        /// <returns></returns>
        object GetItem(object rootItem, string path);

        IEnumerable List(object item);

        string ExtractPath(string pathName);

        string ExtractName(string pathName);

        char Separator { get; }
        StringComparison StringComparisonOption { get; }

        string ParentPath { get; }
        string ValuePath { get; }
        string SubentriesPath { get; }
    }

    public class BreadcrumbCore : ItemsControl
    {
        #region Constructor

        static BreadcrumbCore()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbCore),
                new FrameworkPropertyMetadata(typeof(BreadcrumbCore)));
        }

        public BreadcrumbCore()
        {

            //AddHandler(BreadcrumbItem.SelectedEvent, (RoutedEventHandler)delegate(object sender, RoutedEventArgs args)
            //{
            //    //Debug.WriteLine(this.GetValue(IsDropDownOpenProperty));
            //    this.SetValue(IsDropDownOpenProperty, false);
            //    //args.Handled = true;
            //});

            this.AddValueChanged(ItemsSourceProperty, (o, e) =>
                {
                    if (this.Items.Count > 0)
                    {
                        BreadcrumbItem firstItem = this.ItemContainerGenerator.ContainerFromIndex(0) as BreadcrumbItem;
                        if (firstItem != null)
                        {
                            firstItem.ShowCaption = firstItem.ShowToggle = this.Items.Count == 1;
                        }
                    }
                    updateOverflowedItems();
                });

            AddHandler(BreadcrumbItem.SelectedEvent, (RoutedEventHandler)((o, e) =>
            {
                SelectedBreadcrumbItem = e.OriginalSource as BreadcrumbItem;
                if (SelectedBreadcrumbItem is BreadcrumbItem)
                {
                    var item = (SelectedBreadcrumbItem as BreadcrumbItem);
                    SetValue(SelectedValueProperty, item.DataContext);
                }
                this.SetValue(IsDropDownOpenProperty, false); //Close << drop down when selected.
                RaiseEvent(new RoutedEventArgs(SelectedValueChangedEvent));
                e.Handled = true;
            }));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            var retVal = new BreadcrumbItem(true)
            {
                HeaderTemplate = this.HeaderTemplate,
                IconTemplate = this.IconTemplate
            };

            return retVal;
        }

        private void updateOverflowedItems()
        {
            Stack<Object> overflowedItems = new Stack<object>();
            for (int i = 0; i < Math.Min(LastNonVisible + 1, Items.Count); i++)
            {
                overflowedItems.Push(Items[i]);
            }
            SetValue(OverflowedItemsProperty, overflowedItems);
            SetValue(IsOverflowedProperty, overflowedItems.Count() > DefaultLastNonVisibleIndex + 1);
        }

        public static void OnLastNonVisibleIndexChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            BreadcrumbCore bcore = sender as BreadcrumbCore;
            bcore.updateOverflowedItems();
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// Used by BreadcrumbCorePanel, default (0) the root item is showed in OverflowPanel.
        /// </summary>
        public int DefaultLastNonVisibleIndex { get { return 0; } }
        #endregion

        #region Dependency properties

        #region Events

        public static readonly RoutedEvent SelectedValueChangedEvent = EventManager.RegisterRoutedEvent("SelectedValueChanged",
          RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbCore));

        /// <summary>
        /// SelectedValue changed.
        /// </summary>
        public event RoutedEventHandler SelectedValueChanged
        {
            add { AddHandler(SelectedValueChangedEvent, value); }
            remove { RemoveHandler(SelectedValueChangedEvent, value); }
        }
        #endregion

        #region SelectedBreadcrumbItem / SelectedValue
        public static DependencyProperty SelectedBreadcrumbItemProperty = DependencyProperty.Register("SelectedBreadcrumbItem",
          typeof(BreadcrumbItem), typeof(BreadcrumbCore), new PropertyMetadata(null));

        /// <summary>
        /// The UI item selected.
        /// </summary>
        public BreadcrumbItem SelectedBreadcrumbItem
        {
            get { return (BreadcrumbItem)GetValue(SelectedBreadcrumbItemProperty); }
            set { SetValue(SelectedBreadcrumbItemProperty, value); }
        }

        public static DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue",
         typeof(Object), typeof(BreadcrumbCore), new PropertyMetadata(null));

        /// <summary>
        /// Datacontext of the selected item.
        /// </summary>
        public Object SelectedValue
        {
            get { return (Object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static DependencyProperty IsRootSelectedProperty = DependencyProperty.Register("IsRootSelected",
         typeof(bool), typeof(BreadcrumbCore), new PropertyMetadata(true));

        /// <summary>
        /// Datacontext of the selected item.
        /// </summary>
        public bool IsRootSelected
        {
            get { return (bool)GetValue(IsRootSelectedProperty); }
            set { SetValue(IsRootSelectedProperty, value); }
        }
        #endregion

        #region OverflowedItems, IsOverflowed, LastNonVisible
        public static DependencyProperty OverflowedItemsProperty = DependencyProperty.Register("OverflowedItems",
           typeof(IEnumerable), typeof(BreadcrumbCore), new PropertyMetadata(null));

        /// <summary>
        /// Items to be displayed in OverflowPanel.
        /// </summary>
        public IEnumerable OverflowedItems
        {
            get { return (ICollection)GetValue(OverflowedItemsProperty); }
            set { SetValue(OverflowedItemsProperty, value); }
        }

        public static DependencyProperty IsOverflowedProperty = DependencyProperty.Register("IsOverflowed",
           typeof(bool), typeof(BreadcrumbCore), new PropertyMetadata(false));

        /// <summary>
        /// Only if Overflowed items is more than DefaultLastNonVisibleIndex + 1
        /// </summary>
        public bool IsOverflowed
        {
            get { return (bool)GetValue(IsOverflowedProperty); }
            set { SetValue(IsOverflowedProperty, value); }
        }

        public static DependencyProperty LastNonVisibleIndexProperty = DependencyProperty.Register("LastNonVisibleIndex",
          typeof(int), typeof(BreadcrumbCore), new PropertyMetadata(0, OnLastNonVisibleIndexChanged));

        /// <summary>
        /// Set by BreadcrumbCorePanel to define when items are overflowed.
        /// </summary>
        public int LastNonVisible
        {
            get { return (int)GetValue(LastNonVisibleIndexProperty); }
            set { SetValue(LastNonVisibleIndexProperty, value); }
        }

        #endregion

        #region IsDropDownOpen, DropDownWidth, DropDownHeight, RootItems

        public static readonly DependencyProperty IsDropDownOpenProperty =
          ComboBox.IsDropDownOpenProperty.AddOwner(typeof(BreadcrumbCore),
          new PropertyMetadata(false));

        /// <summary>
        /// Is current dropdown (combobox) opened, this apply to the first &lt;&lt; button only
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty ShowDropDownProperty =
          DependencyProperty.Register("ShowDropDown", typeof(bool), typeof(BreadcrumbCore),
          new PropertyMetadata(true));

        /// <summary>
        /// Whether the first dropdown is shown, set by Breadcrumb.
        /// </summary>
        public bool ShowDropDown
        {
            get { return (bool)GetValue(ShowDropDownProperty); }
            set { SetValue(ShowDropDownProperty, value); }
        }

        public static readonly DependencyProperty DropDownHeightProperty =
            DependencyProperty.Register("DropDownHeight", typeof(double), typeof(BreadcrumbCore), new UIPropertyMetadata(200d));

        /// <summary>
        /// Is current dropdown (combobox) opened, this apply to the first &lt;&lt; button only
        /// </double>
        public double DropDownHeight
        {
            get { return (double)GetValue(DropDownHeightProperty); }
            set { SetValue(DropDownHeightProperty, value); }
        }

        public static readonly DependencyProperty DropDownWidthProperty =
        DependencyProperty.Register("DropDownWidth", typeof(double), typeof(BreadcrumbCore), new UIPropertyMetadata(100d));

        /// <summary>
        /// Is current dropdown (combobox) opened, this apply to the first &lt;&lt; button only
        /// </summary>
        public double DropDownWidth
        {
            get { return (double)GetValue(DropDownWidthProperty); }
            set { SetValue(DropDownWidthProperty, value); }
        }


        public static readonly DependencyProperty RootItemsSourceProperty = DependencyProperty.Register("RootItemsSource",
            typeof(IEnumerable), typeof(BreadcrumbCore), new PropertyMetadata(null));

        /// <summary>
        /// Assigned by Breadcrumb
        /// </summary>
        public IEnumerable RootItemsSource
        {
            get { return (IEnumerable)GetValue(RootItemsSourceProperty); }
            set { SetValue(RootItemsSourceProperty, value); }
        }


        #endregion

        #region HeaderTemplate, IconTemplate

        public static readonly DependencyProperty HeaderTemplateProperty = HeaderedItemsControl
            .HeaderTemplateProperty.AddOwner(typeof(BreadcrumbCore));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty IconTemplateProperty =
            DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(BreadcrumbCore));

        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }

        #endregion

        #endregion
    }
}
