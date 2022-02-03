using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFUI.Controls;

public class SearchExecutedRoutedEventArgs : RoutedEventArgs {
  private readonly string _SearchString;

  public SearchExecutedRoutedEventArgs(RoutedEvent routedEvent, String searchString) : base(routedEvent) {
    this._SearchString = searchString;
  }

  public String SearchString {
    get {
      return this._SearchString;
    }
  }
}

[TemplatePart(Name = "PART_SearchButton", Type = typeof(System.Windows.Controls.Button))]
public class SearchBox : TextBox {
  public static readonly RoutedEvent SearchExecutedEvent = EventManager.RegisterRoutedEvent("SearchExecuted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBox));

  public event RoutedEventHandler SearchExecuted {
    add { AddHandler(SearchExecutedEvent, value); }
    remove { RemoveHandler(SearchExecutedEvent, value); }
  }
  public static readonly DependencyProperty WatermarkTextProperty =
    DependencyProperty.Register(
      name: "WatermarkText",
      propertyType: typeof(String),
      ownerType: typeof(SearchBox),
      typeMetadata: new FrameworkPropertyMetadata(defaultValue: "Search")
    );
  public static readonly DependencyProperty IsWatermarkShownProperty =
    DependencyProperty.Register(
      name: "IsWatermarkShown",
      propertyType: typeof(Boolean),
      ownerType: typeof(SearchBox),
      typeMetadata: new FrameworkPropertyMetadata(defaultValue: false)
    );
  public String WatermarkText {
    get { return (String)GetValue(WatermarkTextProperty); }
    set { SetValue(WatermarkTextProperty, value); }
  }
  public Boolean IsWatermarkShown {
    get { return (Boolean)GetValue(IsWatermarkShownProperty); }
    set { SetValue(IsWatermarkShownProperty, value); }
  }
  static SearchBox() {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
  }

  public SearchBox() {
    this.TextChanged += (sender, args) => { this.IsWatermarkShown = !String.IsNullOrEmpty(this.Text); };
  }

  private void RaiseSearchExecutedEvent() {
    SearchExecutedRoutedEventArgs newEventArgs = new SearchExecutedRoutedEventArgs(SearchBox.SearchExecutedEvent, this.Text);
    RaiseEvent(newEventArgs);
  }
  protected override void OnKeyUp(KeyEventArgs e) {
    base.OnKeyUp(e);
    if (e.Key == Key.Enter) {
      this.RaiseSearchExecutedEvent();
    }
  }

  public override void OnApplyTemplate() {
    base.OnApplyTemplate();
    var searchButton = this.GetPart<System.Windows.Controls.Button>("PART_SearchButton");
    if (searchButton != null) {
      searchButton.Click += (sender, args) => this.RaiseSearchExecutedEvent();
    }
  }

  internal T GetPart<T>(string name)
    where T : DependencyObject {
    return this.GetTemplateChild(name) as T;
  }
}
