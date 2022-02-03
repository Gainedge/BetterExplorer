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
using System.Windows.Shell;

namespace WPFUI.Controls {
  [TemplatePart(Name = PART_NewTabButton, Type = typeof(Button))]
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

    private TitleBar _Ttilebar;
    private Button _NewButton;
    private StackPanel _ControlButtons;
    private ScrollViewer _ScrollViewer;
    private TabPanel _TabPanel;

    static TabControl() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControl), new FrameworkPropertyMetadata(typeof(TabControl)));
    }

    public TabControl() {
      this.LayoutUpdated += (sender, args) => {
        var maxWidth = this.ActualWidth - this._Ttilebar.ActualWidth - this._ControlButtons.ActualWidth - 22;
        if (this._TabPanel.ActualWidth + 20 > maxWidth) {
          this._ScrollViewer.Width = maxWidth;
        } else {
          this._ScrollViewer.Width = this._TabPanel.ActualWidth + 6;
        }
      };
      //this.SizeChanged += (sender, args) => {
      //  var maxWidth = this.ActualWidth - this._Ttilebar.ActualWidth - this._NewButton.ActualWidth - 20;
      //  if (this._TabPanel.ActualWidth > maxWidth) {
      //    this._ScrollViewer.Width = maxWidth;
      //  } else {
      //    this._ScrollViewer.Width = this._TabPanel.ActualWidth;
      //  }
      //};
      this.SelectionChanged += (sender, args) => {
        var i = 1;
      };
    }

    private Boolean IsElementPartOfTabPanel(IInputElement element) {
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
      if (this.IsElementPartOfTabPanel(e.Source as IInputElement)) {
        if (e.Delta > 0) {
          this._ScrollViewer.LineLeft();
        } else {
          this._ScrollViewer.LineRight();
        }
      }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e) {
      base.OnMouseWheel(e);
      
    }

    public override void OnApplyTemplate() {
      base.OnApplyTemplate();
      this._NewButton = this.GetPart<Button>(PART_NewTabButton);
      this._NewButton.Click += (sender, args) => this.RaiseNewTabEvent();
      this._Ttilebar = this.GetPart<TitleBar>("PART_TitleBar");
      this._ScrollViewer = this.GetPart<ScrollViewer>("PART_ScrollViewer");
      this._ControlButtons = this.GetPart<StackPanel>("PART_ControlButtons");
      this._TabPanel = this.GetPart<TabPanel>("HeaderPanel");
    }

    internal T GetPart<T>(string name)
      where T : DependencyObject {
      return this.GetTemplateChild(name) as T;
    }

  }
}
