using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFUI.Controls {
  [TemplatePart(Name = PART_NewTabButton, Type = typeof(Button))]
  [TemplatePart(Name = "PART_TitleBar", Type = typeof(TitleBar))]
  public class TabControl : System.Windows.Controls.TabControl {
    private const string PART_NewTabButton = "PART_NewTabButton";
    public static readonly RoutedEvent NewTabEvent = EventManager.RegisterRoutedEvent(
      "NewTab", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(TabControl));


    public event RoutedEventHandler NewTab {
      add { AddHandler(NewTabEvent, value); }
      remove { RemoveHandler(NewTabEvent, value); }
    }

    // This method raises the Tap event
    void RaiseNewTabEvent() {
      RoutedEventArgs newEventArgs = new RoutedEventArgs(TabControl.NewTabEvent);
      RaiseEvent(newEventArgs);
    }
    public static readonly DependencyProperty TabItemMinWidthProperty = DependencyProperty.Register("TabItemMinWidth", typeof(double), typeof(TabControl), new FrameworkPropertyMetadata(180.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public double TabItemMinWidth {
      get { return (double)GetValue(TabItemMinWidthProperty); }
      set { SetValue(TabItemMinWidthProperty, value); }
    }
    private TitleBar _Ttilebar;
    private Button _NewButton;
    private Button _ScrollbarLeft;
    private Button _ScrollbarRight;
    private StackPanel _ControlButtons;
    private ScrollViewer _ScrollViewer;
    private TabPanel _TabPanel;
    private Boolean _IsMouseScroll = false;

    static TabControl() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControl), new FrameworkPropertyMetadata(typeof(TabControl)));
    }

    public TabControl() {

      this.Loaded += (sender, args) => {
        //var parentWin = Window.GetWindow(this);
        //if (parentWin != null) {
        //  parentWin.StateChanged += (o, eventArgs) => {
        //    this._IsMouseScroll = false;

        //  };
        //  parentWin.SizeChanged += (sender, args) => {
        //    this._IsMouseScroll = false;
        //  };
        //}
      };

      this.SizeChanged += (sender, args) => {
        this._IsMouseScroll = false;
      };
      this.SelectionChanged += (sender, args) => {
        (this.SelectedItem as TabItem)?.BringIntoView();
      };
    }

    private Boolean IsElementPartOfTabPanel(IInputElement element) {
      if (element is TabPanel) {
        return true;
      }
      var parent = VisualTreeHelper.GetParent((DependencyObject)element);
      while (parent != null && !(parent is TabPanel)) {
        parent = VisualTreeHelper.GetParent(parent);
      }

      if (parent == null) {
        return false;
      }

      return true;
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
      base.OnPreviewMouseWheel(e);
      this.OnScroll(e);
    }

    private void OnScroll(MouseWheelEventArgs e) {
      if (this.IsElementPartOfTabPanel(e.Source as IInputElement)) {
        this._IsMouseScroll = true;
        if (e.Delta > 0) {
          this._ScrollViewer.LineLeft();
        } else {
          this._ScrollViewer.LineRight();
        }
        //this._IsMouseScroll = false;
      }
    }

    private void UpdateScrollButtonsAvailability(Boolean isFromScrolling = false) {
      if (this._ScrollViewer == null) return;

      var hOffset = this._ScrollViewer.HorizontalOffset;
      hOffset = Math.Max(hOffset, 0);

      var scrWidth = this._ScrollViewer.ScrollableWidth;
      scrWidth = Math.Max(scrWidth, 0);

      if (this._ScrollbarLeft != null) {
        this._ScrollbarLeft.Visibility = scrWidth == 0 || hOffset <= 0 ? Visibility.Collapsed : Visibility.Visible;

        this._ScrollbarLeft.IsEnabled = hOffset > 0;
      }
      if (this._ScrollbarRight != null) {
        this._ScrollbarRight.Visibility = scrWidth == 0 || hOffset >= scrWidth ? Visibility.Collapsed : Visibility.Visible;

        this._ScrollbarRight.IsEnabled = hOffset < scrWidth;
      }

      this.InvalidateMeasure();
      if (!isFromScrolling) {
        (this.SelectedItem as TabItem)?.BringIntoView();
      } 
    }

    /// <summary>
    /// IsItemItsOwnContainerOverride
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override bool IsItemItsOwnContainerOverride(object item) {
      return item is TabItem;
    }

    protected override Size MeasureOverride(Size constraint) {
      var maxWidth = constraint.Width - this._Ttilebar.ActualWidth - this._ControlButtons.ActualWidth - 22 - (this._ScrollbarLeft?.Visibility == Visibility.Visible ? (this._ScrollbarLeft.ActualWidth  + 12) : 0) - (this._ScrollbarRight?.Visibility == Visibility.Visible ? (this._ScrollbarRight.ActualWidth + 12) : 0);
      if (this._TabPanel.ActualWidth + 20 > maxWidth) {
        this._ScrollViewer.Width = maxWidth;
      } else {
        this._ScrollViewer.Width = this._TabPanel.ActualWidth + 6;
      }
      return base.MeasureOverride(constraint);
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      this._NewButton = this.GetPart<Button>(PART_NewTabButton);
      this._ScrollbarLeft = this.GetPart<Button>("PART_ScrollLeft");
      this._ScrollbarRight = this.GetPart<Button>("PART_ScrollRight");
      this._NewButton.Click += (sender, args) => this.RaiseNewTabEvent();
      this._Ttilebar = this.GetPart<TitleBar>("PART_TitleBar");
      this._ScrollViewer = this.GetPart<ScrollViewer>("PART_ScrollViewer");
      this._ControlButtons = this.GetPart<StackPanel>("PART_ControlButtons");
      this._TabPanel = this.GetPart<TabPanel>("HeaderPanel");
      this._TabPanel.PreviewMouseWheel += (sender, args) => this.OnScroll(args);
      this._ScrollViewer.PreviewMouseWheel += (sender, args) => this.OnScroll(args);
      this._ScrollViewer.Loaded += (s, e) => this.UpdateScrollButtonsAvailability();
      this._ScrollViewer.ScrollChanged += (s, e) => this.UpdateScrollButtonsAvailability(this._IsMouseScroll);
      this._ScrollbarLeft.Click += (sender, args) => {
        this._IsMouseScroll = true;
        this._ScrollViewer.LineLeft();
      };
      this._ScrollbarRight.Click += (sender, args) => {
        this._IsMouseScroll = true;
        this._ScrollViewer.LineRight();
      };
    }

    internal T GetPart<T>(string name)
      where T : DependencyObject {
      return this.GetTemplateChild(name) as T;
    }

  }
}
