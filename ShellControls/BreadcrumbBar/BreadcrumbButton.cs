using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using ShellControls.ShellContextMenu;

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
  /// A breadcrumb button is part of a BreadcrumbItem and contains  a header and a dropdown button.
  /// </summary>
  [TemplatePart(Name = partMenu)]
  [TemplatePart(Name = partToggle)]
  [TemplatePart(Name = partButton)]
  [TemplatePart(Name = partDropDown)]
  public class BreadcrumbButton : HeaderedItemsControl {
    const string partMenu = "PART_Menu";
    const string partToggle = "PART_Toggle";
    const string partButton = "PART_button";
    const string partDropDown = "PART_DropDown";
    static BreadcrumbButton() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbButton), new FrameworkPropertyMetadata(typeof(BreadcrumbButton)));
    }

    #region Dependency Properties
    private static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(object), typeof(BreadcrumbButton), new UIPropertyMetadata(null));

    private static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(BreadcrumbButton), new UIPropertyMetadata(null, SelectedItemChangedEvent));


    #endregion

    #region RoutedEvents
    private static readonly RoutedEvent SelectedItemChanged = EventManager.RegisterRoutedEvent("SelectedItemChanged",
        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbButton));

    private static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BreadcrumbButton));
    #endregion

    private ContextMenu contextMenu;
    private Control dropDownBtn;

    public BreadcrumbButton()
        : base() {
      this.CommandBindings.Add(new CommandBinding(SelectCommand, this.SelectCommandExecuted));
      this.CommandBindings.Add(new CommandBinding(OpenOverflowCommand, this.OpenOverflowCommandExecuted, this.OpenOverflowCommandCanExecute));

      this.InputBindings.Add(new KeyBinding(BreadcrumbButton.SelectCommand, new KeyGesture(Key.Enter)));
      this.InputBindings.Add(new KeyBinding(BreadcrumbButton.SelectCommand, new KeyGesture(Key.Space)));
      this.InputBindings.Add(new KeyBinding(BreadcrumbButton.OpenOverflowCommand, new KeyGesture(Key.Down)));
      this.InputBindings.Add(new KeyBinding(BreadcrumbButton.OpenOverflowCommand, new KeyGesture(Key.Up)));
    }


    private bool isPressed = false;

    protected override void OnMouseLeave(MouseEventArgs e) {
      this.IsPressed = false;
    }



    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
      e.Handled = true;
      this.IsPressed = this.isPressed = true;
      base.OnMouseLeftButtonDown(e);
    }


    #region Cursor Stuff

    /// <summary> Retrieves the cursor's position, in screen coordinates. </summary>
    /// <see> See MSDN documentation for further information. </see>
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    /// <summary> Struct representing a point. </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT {
      public int X;
      public int Y;

      public static implicit operator Point(POINT point) {
        return new Point(point.X, point.Y);
      }
    }

    public static Point GetCursorPosition() {
      POINT lpPoint;
      GetCursorPos(out lpPoint);
      return lpPoint;
    }

    #endregion Cursor Stuff

    protected override void OnMouseUp(MouseButtonEventArgs e) {
      e.Handled = true;
      if (e.ChangedButton == MouseButton.Right) {
        if (this.DataContext is IListItemEx data) {
          Point relativePoint = this.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
          Point realCoordinates = Application.Current.MainWindow.PointToScreen(relativePoint);
          //ShellContextMenu cm = new ShellContextMenu(new[] { data });
          //cm.ShowContextMenu(new System.Drawing.Point((int)realCoordinates.X, (int)realCoordinates.Y + (int)this.ActualHeight), 1);
        }
      }
      if (this.isPressed) {
        RoutedEventArgs args = new RoutedEventArgs(BreadcrumbButton.ClickEvent);
        this.RaiseEvent(args);
        selectCommand.Execute(null, this);
      }
      this.IsPressed = this.isPressed = false;
      base.OnMouseUp(e);
    }

    private void SelectCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
      this.SelectedItem = null;
      RoutedEventArgs args = new RoutedEventArgs(Button.ClickEvent);
      this.RaiseEvent(args);
    }

    private void OpenOverflowCommandExecuted(object sender, ExecutedRoutedEventArgs e) {
      this.IsDropDownPressed = true;
    }

    private void OpenOverflowCommandCanExecute(object sender, CanExecuteRoutedEventArgs e) {
      e.CanExecute = this.Items.Count > 0;
    }

    public static RoutedUICommand OpenOverflowCommand {
      get { return openOverflowCommand; }
    }

    public static RoutedUICommand SelectCommand {
      get { return selectCommand; }
    }

    private static RoutedUICommand openOverflowCommand = new RoutedUICommand("Open Overflow", "OpenOverflowCommand", typeof(BreadcrumbButton));
    private static RoutedUICommand selectCommand = new RoutedUICommand("Select", "SelectCommand", typeof(BreadcrumbButton));


    public override void OnApplyTemplate() {
      this.dropDownBtn = this.GetTemplateChild(partDropDown) as Control;
      this.contextMenu = this.GetTemplateChild(partMenu) as ContextMenu;
      if (this.contextMenu != null) {
        this.contextMenu.Opened += new RoutedEventHandler(this.contextMenu_Opened);
      }
      if (this.dropDownBtn != null) {
        this.dropDownBtn.MouseDown += new MouseButtonEventHandler(this.dropDownBtn_MouseDown);
      }

      base.OnApplyTemplate();
      this.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() => {
        var data = this.DataContext as IListItemEx;
        if (data == null || data.DisplayName == "Search.search-ms")
          return;
        if (data != null && data.ParsingName != KnownFolders.Computer.ParsingName && data.ParsingName != KnownFolders.Desktop.ParsingName) {
          if (data.IsSearchFolder)
            return;
          //data = new ShellItem(data.CachedParsingName.ToShellParsingName());
          var aditionalItems = new List<IListItemEx>();
          //ShellItem.IsCareForMessageHandle = false;
          foreach (var item in data) {
            try {
              if (item.IsFolder) {
                aditionalItems.Add(item);
              }
            } catch { }
          }
          //ShellItem.IsCareForMessageHandle = true;
          this.ItemsSource = aditionalItems;
        }
      }));
    }

    void dropDownBtn_MouseDown(object sender, MouseButtonEventArgs e) {
      e.Handled = true;
      this.IsDropDownPressed ^= true;
    }


    /// <summary>
    /// Gets or sets the Image of the BreadcrumbButton.
    /// </summary>
    public ImageSource Image {
      get { return (ImageSource)this.GetValue(ImageProperty); }
      set { this.SetValue(ImageProperty, value); }
    }


    void contextMenu_Opened(object sender, RoutedEventArgs e) {
      this.contextMenu.Items.Clear();
      //this.contextMenu.ItemTemplate = this.ItemTemplate;
      //this.contextMenu.ItemTemplateSelector = this.ItemTemplateSelector;
      foreach (object item in this.Items) {
        if (!(item is MenuItem) && !(item is Separator)) {
          var itemEx = item as IListItemEx;
          MenuItem menuItem = new MenuItem();
          menuItem.DataContext = item;
          menuItem.Header = itemEx.DisplayName;
          //BreadcrumbItem bi = item as BreadcrumbItem;
          //if (bi == null) {
          //  BreadcrumbItem parent = this.TemplatedParent as BreadcrumbItem;
          //  if (parent != null)
          //    bi = parent.ContainerFromItem(item);
          //}
          //if (bi != null)
          //  menuItem.Header = bi.TraceValue.Replace("_", "__");

          //Image image = new Image();
          //image.Source = bi != null ? bi.Image : null;
          //image.SnapsToDevicePixels = true;
          //image.Stretch = Stretch.Fill;
          //image.VerticalAlignment = VerticalAlignment.Center;
          //image.HorizontalAlignment = HorizontalAlignment.Center;
          //image.Width = 16;
          //image.Height = 16;

          menuItem.Icon = itemEx.ThumbnailSource(18, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

          menuItem.Click += new RoutedEventHandler(this.item_Click);
          //var data = bi.Data;
          //if (item != null && (item.Equals(this.SelectedItem) || data == (IListItemEx)this.SelectedItem))
          //  menuItem.FontWeight = FontWeights.Bold;
          //menuItem.ItemTemplate = this.ItemTemplate;
          //menuItem.ItemTemplateSelector = this.ItemTemplateSelector;
          this.contextMenu.Items.Add(menuItem);
        } else {
          this.contextMenu.Items.Add(item);
        }
        //var mi = new Win32ContextMenuItem(null);
        //mi.Label = item.DisplayName;
        //this.contextMenu.Items.Add(mi);
      }
      this.contextMenu.Placement = PlacementMode.Relative;
      this.contextMenu.PlacementTarget = this.dropDownBtn;
      this.contextMenu.VerticalOffset = this.dropDownBtn.ActualHeight + 10;
    }

    void item_Click(object sender, RoutedEventArgs e) {
      this.contextMenu.IsOpen = false;
      MenuItem item = e.Source as MenuItem;
      object dataItem = item.DataContext;
      //RemoveSelectedItem(dataItem);
      //SelectedItem = dataItem;
      FrameworkElement parent = this.TemplatedParent as FrameworkElement;
      while (parent != null && !(parent is BreadcrumbBar))
        parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
      BreadcrumbBar bar = parent as BreadcrumbBar;
      if (bar != null) {
        bar.Path = (dataItem as IListItemEx).ParsingName;
      }
    }
    /*
    /// <summary>
    /// When a BreadcrumbItem is selected from a dropdown menu, the SelectedItem of the new selected item must be set to null.
    /// Since no event is raised when a DependencyProperty is assigned to it's current value, this cannot be recognized at this place,
    /// therefore the SelectedItem DependencyProperty must previously set to null before setting it to it's new value to raise event
    /// when SelectedItem is changed:
    /// </summary>
    /// <param name="dataItem"></param>
    private void RemoveSelectedItem(object dataItem) {
        //var data = (dataItem as BreadcrumbItem).Data as ShellItem;
        if (dataItem != null && dataItem.Equals(SelectedItem))
            SelectedItem = null;
    }
    */

    /// <summary>
    /// Gets or sets the selectedItem.
    /// </summary>	
    public object SelectedItem {
      private get { return this.GetValue(SelectedItemProperty); }
      set { this.SetValue(SelectedItemProperty, value); }
    }

    private static void SelectedItemChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      BreadcrumbButton button = d as BreadcrumbButton;
      if (button.IsInitialized) {
        RoutedEventArgs args = new RoutedEventArgs(SelectedItemChanged);
        button.RaiseEvent(args);
      }
    }

    /// <summary>
    /// Focus this BreadcrumbButton if the focus is currently within the BreadcrumbBar where this BreadcrumbButton is embedded:
    /// </summary>
    protected override void OnMouseEnter(MouseEventArgs e) {
      this.isPressed = e.LeftButton == MouseButtonState.Pressed;
      FrameworkElement parent = this.TemplatedParent as FrameworkElement;
      while (parent != null && !(parent is BreadcrumbBar))
        parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
      BreadcrumbBar bar = parent as BreadcrumbBar;
      if (bar != null && bar.IsKeyboardFocusWithin)
        this.Focus();
      this.IsPressed = this.isPressed;
      base.OnMouseEnter(e);
    }


    private static void OverflowPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      BreadcrumbButton button = d as BreadcrumbButton;
      button.OnOverflowPressedChanged();
    }

    protected virtual void OnOverflowPressedChanged() {
    }


    /// <summary>
    /// Specifies how to display the BreadcrumbButton.
    /// </summary>
    public enum ButtonMode {
      /// <summary>
      /// Display as Breadcrumb.
      /// </summary>
      Breadcrumb,

      /// <summary>
      /// Display as overflow.
      /// </summary>
      Overflow,

      /// <summary>
      /// Display as drop down.
      /// </summary>
      DropDown
    }


    /// <summary>
    /// Gets or sets the ButtonMode for the BreadcrumbButton.
    /// </summary>
    public ButtonMode Mode {
      get { return (ButtonMode)this.GetValue(ModeProperty); }
      set { this.SetValue(ModeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Mode.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ButtonMode), typeof(BreadcrumbButton), new UIPropertyMetadata(ButtonMode.Breadcrumb));


    /// <summary>
    /// Occurs when the Button is clicked.
    /// </summary>
    public event RoutedEventHandler Click {
      add { this.AddHandler(BreadcrumbButton.ClickEvent, value); }
      remove { this.RemoveHandler(BreadcrumbButton.ClickEvent, value); }
    }

    /// <summary>
    /// Occurs when the SelectedItem is changed.
    /// </summary>
    public event RoutedEventHandler Select {
      add { this.AddHandler(BreadcrumbButton.SelectedItemChanged, value); }
      remove { this.RemoveHandler(BreadcrumbButton.SelectedItemChanged, value); }
    }


    /// <summary>
    /// Gets or sets whether the button is pressed.
    /// </summary>
    public bool IsPressed {
      get { return (bool)this.GetValue(IsPressedProperty); }
      set { this.SetValue(IsPressedProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether the button is pressed.
    /// </summary>
    public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register("IsPressed", typeof(bool), typeof(BreadcrumbButton), new UIPropertyMetadata(false));


    /// <summary>
    /// Gets or sets whether the drop down button is pressed.
    /// </summary>
    public bool IsDropDownPressed {
      get { return (bool)this.GetValue(IsDropDownPressedProperty); }
      set { this.SetValue(IsDropDownPressedProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether the drop down button is pressed.
    /// </summary>
    public static readonly DependencyProperty IsDropDownPressedProperty =
            DependencyProperty.Register("IsDropDownPressed", typeof(bool), typeof(BreadcrumbButton), new UIPropertyMetadata(false, OverflowPressedChanged));


    /// <summary>
    /// Gets or sets the DataTemplate for the drop down items.
    /// </summary>
    public DataTemplate DropDownContentTemplate {
      get { return (DataTemplate)this.GetValue(DropDownContentTemplateProperty); }
      set { this.SetValue(DropDownContentTemplateProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DropDownContentTemplate.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DropDownContentTemplateProperty =
            DependencyProperty.Register("DropDownContentTemplate", typeof(DataTemplate), typeof(BreadcrumbButton), new UIPropertyMetadata(null));



    /// <summary>
    /// Gets or sets whether the drop down button is visible.
    /// </summary>
    public bool IsDropDownVisible {
      get { return (bool)this.GetValue(IsDropDownVisibleProperty); }
      set { this.SetValue(IsDropDownVisibleProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether the drop down button is visible.
    /// </summary>
    public static readonly DependencyProperty IsDropDownVisibleProperty =
            DependencyProperty.Register("IsDropDownVisible", typeof(bool), typeof(BreadcrumbButton), new UIPropertyMetadata(true));



    /// <summary>
    /// Gets or sets whether the button is visible.
    /// </summary>
    public bool IsButtonVisible {
      get { return (bool)this.GetValue(IsButtonVisibleProperty); }
      set { this.SetValue(IsButtonVisibleProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether the button is visible.
    /// </summary>
    public static readonly DependencyProperty IsButtonVisibleProperty =
            DependencyProperty.Register("IsButtonVisible", typeof(bool), typeof(BreadcrumbButton), new UIPropertyMetadata(true));


    /// <summary>
    /// Gets or sets whether the Image is visible
    /// </summary>
    public bool IsImageVisible {
      get { return (bool)this.GetValue(IsImageVisibleProperty); }
      set { this.SetValue(IsImageVisibleProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether the Image is visible
    /// </summary>
    public static readonly DependencyProperty IsImageVisibleProperty =
            DependencyProperty.Register("IsImageVisible", typeof(bool), typeof(BreadcrumbButton), new UIPropertyMetadata(true));




    /// <summary>
    /// Gets or sets whether to use visual background style on MouseOver and/or MouseDown.
    /// </summary>
    public bool EnableVisualButtonStyle {
      get { return (bool)this.GetValue(EnableVisualButtonStyleProperty); }
      set { this.SetValue(EnableVisualButtonStyleProperty, value); }
    }

    /// <summary>
    /// Gets or sets whether to use visual background style on MouseOver and/or MouseDown.
    /// </summary>
    public static readonly DependencyProperty EnableVisualButtonStyleProperty =
            DependencyProperty.Register("EnableVisualButtonStyle", typeof(bool), typeof(BreadcrumbButton), new UIPropertyMetadata(true));


  }
}
