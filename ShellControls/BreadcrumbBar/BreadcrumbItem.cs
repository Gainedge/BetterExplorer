using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using Size = System.Windows.Size;

//###################################################################################
// Odyssey.Controls
// (c) Copyright 2008 Thomas Gerber
// All rights reserved.
//
//  THERE IS NO WARRANTY FOR THE PROGRAM, TO THE EXTENT PERMITTED BY
// APPLICABLE LAW.  EXCEPT WHEN OTHERWISE STATED IN WRITING THE COPYRIGHT
// HOLDERS AND/OR OTHER PARTIES PROVIDE THE PROGRAM "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE.  THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE OF THE PROGRAM
// IS WITH YOU.  SHOULD THE PROGRAM PROVE DEFECTIVE, YOU ASSUME THE COST OF
// ALL NECESSARY SERVICING, REPAIR OR CORRECTION.
//###################################################################################


namespace ShellControls.BreadcrumbBar {
  /// <summary>
  /// A breadcrumb item that is part of a BreadcrumbBar and contains a BreadcrumbButton and nested child BreadcrumbItems.
  /// </summary>
  [TemplatePart(Name = partHeader)]
  [TemplatePart(Name = partSelected)]
  public class BreadcrumbItem : Selector {

    #region DependencyProperties

    /// <summary>
    /// Gets or sets whether the dropdown button is pressed.
    /// </summary>
    public static readonly DependencyProperty IsDropDownPressedProperty =
            DependencyProperty.Register("IsDropDownPressed", typeof(bool), typeof(BreadcrumbItem), new UIPropertyMetadata(false, DropDownPressedPropertyChanged));

    /// <summary>
    /// Gets or sets whether the BreadcrumbItem is in overflow mode, which means that the header property is not visible.
    /// </summary>
    public static readonly DependencyProperty IsOverflowProperty =
            DependencyProperty.Register("IsOverflow", typeof(bool), typeof(BreadcrumbItem), new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                    OverflowPropertyChanged));

    /// <summary>
    /// Gets or sets whether this BreadcrumbItem is the Root of a BreadcrumbBar.
    /// </summary>
    public static readonly DependencyProperty IsRootProperty =
            DependencyProperty.Register("IsRoot", typeof(bool), typeof(BreadcrumbItem), new UIPropertyMetadata(false));

    private static readonly DependencyPropertyKey SelectedBreadcrumbPropertyKey =
            DependencyProperty.RegisterReadOnly("SelectedBreadcrumb", typeof(BreadcrumbItem), typeof(BreadcrumbItem),
            new UIPropertyMetadata(null, SelectedBreadcrumbPropertyChanged));

    /// <summary>
    /// Gets or sets the TemplateSelector of an Item.
    /// </summary>
    public static readonly DependencyProperty OverflowItemTemplateSelectorProperty;

    public static readonly DependencyProperty OverflowItemTemplateProperty;

    /// <summary>
    /// Gets or sets the ImageSource.
    /// </summary>
    public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(BreadcrumbItem), new UIPropertyMetadata(null));

    /// <summary>
    /// Gets or sets the Trace string to build the Path.
    /// </summary>
    public static readonly DependencyProperty TraceProperty =
            DependencyProperty.Register("Trace", typeof(object), typeof(BreadcrumbItem), new UIPropertyMetadata(null, TracePropertyChanged));

    /// <summary>
    /// Gets or sets the Header.
    /// </summary>
    public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(BreadcrumbItem), new UIPropertyMetadata(null, HeaderPropertyChanged));

    public static readonly DependencyProperty HeaderTemplateProperty;

    public static readonly DependencyProperty HeaderTemplateSelectorProperty;

    #endregion

    const string partHeader = "PART_Header";
    const string partSelected = "PART_Selected";

    static BreadcrumbItem() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbItem), new FrameworkPropertyMetadata(typeof(BreadcrumbItem)));

      OverflowItemTemplateProperty = BreadcrumbBar.OverflowItemTemplateProperty.AddOwner(typeof(BreadcrumbItem),
              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

      OverflowItemTemplateSelectorProperty = BreadcrumbBar.OverflowItemTemplateSelectorProperty.AddOwner(typeof(BreadcrumbItem),
              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

      HeaderTemplateSelectorProperty = BreadcrumbBar.BreadcrumbItemTemplateSelectorProperty.AddOwner(typeof(BreadcrumbItem),
              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

      HeaderTemplateProperty = BreadcrumbBar.BreadcrumbItemTemplateProperty.AddOwner(typeof(BreadcrumbItem),
              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
      base.OnItemsChanged(e);
    }

    private static void HeaderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
      BreadcrumbItem item = sender as BreadcrumbItem;
    }

    private static void TracePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
      BreadcrumbItem item = sender as BreadcrumbItem;

      RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>(e.OldValue, e.NewValue, TraceChangedEvent);
      item.RaiseEvent(args);
      //if (args.OriginalSource == item.BreadcrumbBar.RootItem) {
      //	item.BreadcrumbBar.BuildBreadcrumbsFromPath(item.BreadcrumbBar.GetDisplayPath());
      //}
    }

    /// <summary>
    /// Occurs when the IsDropDownPressed property is changed.
    /// </summary>
    public event RoutedPropertyChangedEventHandler<object> DropDownPressedChanged {
      add { this.AddHandler(DropDownPressedChangedEvent, value); }
      remove { this.RemoveHandler(DropDownPressedChangedEvent, value); }
    }

    /// <summary>
    /// Occurs when the IsDropDownPressed property is changed.
    /// </summary>
    public static readonly RoutedEvent DropDownPressedChangedEvent = EventManager.RegisterRoutedEvent("DropDownPressedChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(BreadcrumbItem));

    /// <summary>
    /// Occurs when the Trace property is changed.
    /// </summary>
    public event RoutedPropertyChangedEventHandler<object> TraceChanged {
      add { this.AddHandler(TraceChangedEvent, value); }
      remove { this.RemoveHandler(TraceChangedEvent, value); }
    }

    /// <summary>
    /// Occurs when the Trace property is changed.
    /// </summary>
    public static readonly RoutedEvent TraceChangedEvent = EventManager.RegisterRoutedEvent("TraceChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(BreadcrumbItem));


    /// <summary>
    /// Creates a new instance of BreadcrumbItem.
    /// </summary>
    public BreadcrumbItem()
        : base() {
    }


    /// <summary>
    /// Creates a new BreadcrumbItem out of the specified data.
    /// </summary>
    /// <param name="dataContext">The DataContext for the BreadcrumbItem</param>
    /// <returns>DataContext if dataContext is a Breadcrumbitem, otherwhise a new BreadcrumbItem.</returns>
    public static BreadcrumbItem CreateItem(object dataContext) {
      BreadcrumbItem item = dataContext as BreadcrumbItem;
      if (item == null && dataContext != null) {
        item = new BreadcrumbItem();
        item.DataContext = dataContext;
      }
      return item;
    }

    private static void SelectedBreadcrumbPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      BreadcrumbItem item = d as BreadcrumbItem;
      item.OnSelectedBreadcrumbChanged(e.OldValue, e.NewValue);
    }

    /// <summary>
    /// Occurs when the selected BreadcrumbItem is changed.
    /// </summary>
    /// <param name="oldItem"></param>
    /// <param name="newItem"></param>
    protected virtual void OnSelectedBreadcrumbChanged(object oldItem, object newItem) {
      if (this.SelectedBreadcrumb != null) {
        this.SelectedBreadcrumb.SelectedItem = null;
      }
    }

    protected override bool IsItemItsOwnContainerOverride(object item) {
      return item is BreadcrumbItem;
    }

    protected override DependencyObject GetContainerForItemOverride() {
      BreadcrumbItem item = new BreadcrumbItem();
      return item;
    }

    private FrameworkElement headerControl;
    private FrameworkElement selectedControl;


    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      this.headerControl = this.GetTemplateChild(partHeader) as FrameworkElement;
      this.selectedControl = this.GetTemplateChild(partSelected) as FrameworkElement;

      this.ApplyBinding();
    }

    public IListItemEx Data {
      get {
        //return DataContext != null ? DataContext : this;
        return (IListItemEx)this.DataContext;
      }
    }



    /// <summary>
    /// Gets or sets wheter the dropdown button is pressed.
    /// </summary>
    public bool IsDropDownPressed {
      get { return (bool)this.GetValue(IsDropDownPressedProperty); }
      set { this.SetValue(IsDropDownPressedProperty, value); }
    }


    /// <summary>
    /// Occurs when the IsDropDownPressed property is changed.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    public static void DropDownPressedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      BreadcrumbItem item = d as BreadcrumbItem;
      item.OnDropDownPressedChanged();
    }


    /// <summary>
    /// Occurs when the DropDown button is pressed or released.
    /// </summary>
    protected virtual void OnDropDownPressedChanged() {
      RoutedEventArgs args = new RoutedEventArgs(DropDownPressedChangedEvent);
      this.RaiseEvent(args);
    }



    /// <summary>
    /// Gets whether the breadcrumb item is overflowed which means it is not visible in the breadcrumb bar but in the
    /// drop down menu of the breadcrumb bar.
    /// </summary>
    public bool IsOverflow {
      get { return (bool)this.GetValue(IsOverflowProperty); }
      private set { this.SetValue(IsOverflowProperty, value); }
    }

    /// <summary>
    /// Occurs when the Overflow property is changed.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    public static void OverflowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      BreadcrumbItem item = d as BreadcrumbItem;
      item.OnOverflowChanged((bool)e.NewValue);
    }



    /// <summary>
    /// Set to true, to collapse the item if SelectedItem is not null. otherwise false.
    /// </summary>
    public bool IsRoot {
      get { return (bool)this.GetValue(IsRootProperty); }
      set { this.SetValue(IsRootProperty, value); }
    }


    /// <summary>
    /// Occurs when the Overflow property is changed.
    /// </summary>
    public static readonly RoutedEvent OverflowChangedEvent = EventManager.RegisterRoutedEvent("OverflowChanged",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbItem));

    /// <summary>
    /// Occurs when the Overflow property is changed.
    /// </summary>
    protected virtual void OnOverflowChanged(bool newValue) {
      RoutedEventArgs args = new RoutedEventArgs(OverflowChangedEvent);
      this.RaiseEvent(args);
    }


    /// <summary>
    /// Perform a special measurement that checks whether to collapse the header.
    /// </summary>
    /// <param name="constraint"></param>
    /// <returns></returns>
    protected override Size MeasureOverride(Size constraint) {
      if (this.SelectedItem != null) {
        this.headerControl.Visibility = Visibility.Visible;
        this.headerControl.Measure(constraint);
        Size size = new Size(constraint.Width - this.headerControl.DesiredSize.Width, constraint.Height);
        this.selectedControl.Measure(new Size(double.PositiveInfinity, constraint.Height));
        double width = this.headerControl.DesiredSize.Width + this.selectedControl.DesiredSize.Width;
        if (width > constraint.Width || (this.IsRoot && this.SelectedItem != null)) {
          this.headerControl.Visibility = Visibility.Collapsed;
        }
      } else if (this.headerControl != null) this.headerControl.Visibility = Visibility.Visible;
      this.IsOverflow = this.headerControl != null ? this.headerControl.Visibility != Visibility.Visible : false;

      Size result = base.MeasureOverride(constraint);
      return result;
    }


    protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
      if (this.SelectedItem == null) {
        this.SelectedBreadcrumb = null;
      } else {
        this.SelectedBreadcrumb = this.ContainerFromItem(this.SelectedItem);
      }
      base.OnSelectionChanged(e);

    }

    /// <summary>
    /// Generates a new BreadcrumbItem out of the specified item.
    /// </summary>
    /// <param name="item">The item for which to create a new BreadcrumbItem.</param>
    /// <returns>Item, if item is a BreadcrumbItem, otherwhise a newly created BreadcrumbItem.</returns>
    public BreadcrumbItem ContainerFromItem(object item) {
      BreadcrumbItem result = item as BreadcrumbItem;
      if (result == null) {
        result = CreateItem(item);
        if (result != null) {
          this.AddLogicalChild(result);
          result.ApplyTemplate();
        }
      }
      return result;
    }


    /// <summary>
    /// Gets the selected BreadcrumbItem.
    /// </summary>
    public BreadcrumbItem SelectedBreadcrumb {
      get { return (BreadcrumbItem)this.GetValue(SelectedBreadcrumbProperty); }
      private set { this.SetValue(SelectedBreadcrumbPropertyKey, value); }
    }


    public static readonly DependencyProperty SelectedBreadcrumbProperty = SelectedBreadcrumbPropertyKey.DependencyProperty;

    /*
public DataTemplateSelector BreadcrumbTemplateSelector { get; set; }
*/

    /*
public DataTemplate BreadcrumbItemTemplate { get; set; }
*/

    public DataTemplateSelector OverflowItemTemplateSelector {
      get { return (DataTemplateSelector)this.GetValue(OverflowItemTemplateSelectorProperty); }
      set { this.SetValue(OverflowItemTemplateSelectorProperty, value); }
    }


    public DataTemplate OverflowItemTemplate {
      get { return (DataTemplate)this.GetValue(OverflowItemTemplateProperty); }
      set { this.SetValue(OverflowItemTemplateProperty, value); }
    }




    /// <summary>
    /// Gets or sets the image that is used to display this item.
    /// </summary>
    public ImageSource Image {
      get { return (ImageSource)this.GetValue(ImageProperty); }
      set { this.SetValue(ImageProperty, value); }
    }

    /// <summary>
    /// Gets or sets the Trace of the breadcrumb
    /// </summary>
    public object Trace {
      get { return (object)this.GetValue(TraceProperty); }
      set { this.SetValue(TraceProperty, value); }
    }


    /// <summary>
    /// Gets or sets the Binding to the Trace property. This is not a dependency property.
    /// </summary>
    public BindingBase TraceBinding { get; set; }


    /// <summary>
    /// Gets or sets the Binding to the Image property.  This is not a dependency property.
    /// </summary>
    public BindingBase ImageBinding { get; set; }


    /// <summary>
    /// Gets or sets the header for the breadcrumb item.
    /// </summary>
    public object Header {

      get { return (object)this.GetValue(HeaderProperty); }
      set { this.SetValue(HeaderProperty, value); }
    }

    /// <summary>
    /// Gets or sets the header template.
    /// </summary>
    public DataTemplate HeaderTemplate {
      get { return (DataTemplate)this.GetValue(HeaderTemplateProperty); }
      set { this.SetValue(HeaderTemplateProperty, value); }
    }

    /// <summary>
    /// Gets or sets the header template selector.
    /// </summary>
    public DataTemplateSelector HeaderTemplateSelector {
      get { return (DataTemplateSelector)this.GetValue(HeaderTemplateSelectorProperty); }
      set { this.SetValue(HeaderTemplateSelectorProperty, value); }
    }

    /// <summary>
    ///  Appies the binding to the breadcrumb item.
    /// </summary>
    public void ApplyBinding() {
      object item = this.DataContext;
      if (item == null) return;

      BreadcrumbItem root = this;
      DataTemplate template = this.HeaderTemplate;
      DataTemplateSelector templateSelector = this.HeaderTemplateSelector;
      if (templateSelector != null) {

        template = templateSelector.SelectTemplate(item, root);
      }
      if (template == null) {
        DataTemplateKey key = GetResourceKey(item);
        if (key != null) template = this.TryFindResource(key) as DataTemplate;
      }

      root.SelectedItem = null;

      var hdt = template as HierarchicalDataTemplate;
      if (template != null) {
        root.Header = template.LoadContent();
      } else {
        root.Header = ((IListItemEx)item).DisplayName;
      }
      root.DataContext = item;

      if (hdt != null) {
        // bind the Items to the hierarchical data template:
        root.SetBinding(BreadcrumbItem.ItemsSourceProperty, hdt.ItemsSource);
      }

      BreadcrumbBar bar = this.BreadcrumbBar;

      if (bar != null) {
        if (this.TraceBinding == null) this.TraceBinding = bar.TraceBinding;
        if (this.ImageBinding == null) this.ImageBinding = bar.ImageBinding;
      }

      if (this.TraceBinding != null) {
        root.SetBinding(BreadcrumbItem.TraceProperty, this.TraceBinding);
      }
      if (this.ImageBinding != null) {
        root.SetBinding(BreadcrumbItem.ImageProperty, this.ImageBinding);
      }


      this.ApplyProperties(item);
    }



    /// <summary>
    /// Gets the parent BreadcrumbBar container.
    /// </summary>
    public BreadcrumbBar BreadcrumbBar {
      get {
        DependencyObject current = this;
        while (current != null) {
          current = LogicalTreeHelper.GetParent(current);
          if (current is BreadcrumbBar) return current as BreadcrumbBar;
        }
        return null;
      }
    }

    /// <summary>
    /// Gets the parent BreadcrumbItem, otherwise null.
    /// </summary>
    public BreadcrumbItem ParentBreadcrumbItem {
      get {
        BreadcrumbItem parent = LogicalTreeHelper.GetParent(this) as BreadcrumbItem;
        return parent;
      }
    }

    private static DataTemplateKey GetResourceKey(object item) {
      XmlDataProvider xml = item as XmlDataProvider;

      if (xml != null) {
        return new DataTemplateKey(xml.XPath);
      } else {
        var node = item as XmlNode;
        return node == null ? new DataTemplateKey(item.GetType()) : new DataTemplateKey(node.Name);
      }
    }


    private void ApplyProperties(object item) {
      var e = new ApplyPropertiesEventArgs(item, this, BreadcrumbBar.ApplyPropertiesEvent);
      e.Image = this.Image;
      e.Trace = this.Trace;
      e.TraceValue = this.TraceValue;
      this.RaiseEvent(e);
      this.Image = (item as IListItemEx)?.ThumbnailSource(18, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
      this.Trace = e.Trace;
    }

    /// <summary>
    /// Gets the Trace string from the Trace property.
    /// </summary>
    public string TraceValue {
      get {
        XmlNode xml = this.Trace as XmlNode;
        if (xml != null) {
          return xml.Value;
        }
        if (this.Trace != null) return this.Trace.ToString();
        if (this.Header != null) {
          if (this.Header.ToString().ToLower().StartsWith("system.")) {
            return this.Trace.ToString();
          } else {
            return this.Header.ToString();
          }
        }
        return string.Empty;
      }
    }

    /// <summary>
    /// Gets the item that represents the specified trace otherwise null.
    /// </summary>
    /// <param name="trace">The Trace property of the associated BreadcrumbItem.</param>
    /// <returns>An object included in Items, otherwise null.</returns>
    public object GetTraceItem(IListItemEx trace) => this.Items.OfType<IListItemEx>().FirstOrDefault(s => s != null && s.Equals(trace));


    protected override IEnumerator LogicalChildren {
      get {
        object content = this.SelectedBreadcrumb; ;
        if (content == null) {
          return base.LogicalChildren;
        }
        if (base.TemplatedParent != null) {
          DependencyObject current = content as DependencyObject;
          if (current != null) {
            DependencyObject parent = LogicalTreeHelper.GetParent(current);
            if ((parent != null) && (parent != this)) {
              return base.LogicalChildren;
            }
          }
        }

        object[] array = new object[] { this.SelectedBreadcrumb };
        return array.GetEnumerator();
      }
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue) {
      this.SelectedItem = null;
    }

    /// <summary>
    /// Gets or sets whether the button is visible.
    /// </summary>
    public bool IsButtonVisible {
      get { return (bool)this.GetValue(IsButtonVisibleProperty); }
      set { this.SetValue(IsButtonVisibleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsButtonVisible.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsButtonVisibleProperty =
        DependencyProperty.Register("IsButtonVisible", typeof(bool), typeof(BreadcrumbItem), new UIPropertyMetadata(true));

    /*
/// <summary>
/// Gets or sets whether the Image is visible.
/// </summary>
public bool IsImageVisible {
  get { return (bool)GetValue(IsImageVisibleProperty); }
  set { SetValue(IsImageVisibleProperty, value); }
}
*/

    /*
/// <summary>
/// Gets or sets whether the Image is visible.
/// </summary>
public static readonly DependencyProperty IsImageVisibleProperty =
    DependencyProperty.Register("IsImageVisible", typeof(bool), typeof(BreadcrumbItem), new UIPropertyMetadata(false));
*/
  }
}
