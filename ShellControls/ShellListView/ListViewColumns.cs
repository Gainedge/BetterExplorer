using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ShellControls.ShellListView;
public class ListViewColumns : DependencyObject {
  ///
  /// IsStretched Dependancy property which can be attached to gridview columns.
  ///
  public static readonly DependencyProperty StretchProperty =
      DependencyProperty.RegisterAttached("Stretch",
      typeof(bool),
      typeof(ListViewColumns),
      new UIPropertyMetadata(true, null, OnCoerceStretch));

  ///
  /// Gets the stretch.
  ///
  /// The obj.
  ///
  public static bool GetStretch(DependencyObject obj) {
    return (bool)obj.GetValue(StretchProperty);
  }

  ///
  /// Sets the stretch.
  ///
  /// The obj.
  /// if set to true [value].
  public static void SetStretch(DependencyObject obj, bool value) {
    obj.SetValue(StretchProperty, value);
  }

  ///
  /// Called when [coerce stretch].
  ///
  ///If this callback seems unfamilar then please read
  /// the great blog post by Paul Jackson found here.
  /// http://compilewith.net/2007/08/wpf-dependency-properties.html
  /// The source.
  /// The value.
  ///
  public static object OnCoerceStretch(DependencyObject source, object value) {
    ListView lv = (source as ListView);

    //Ensure we dont have an invalid dependancy object of type ListView.
    if (lv == null) {
      throw new ArgumentException("This property may only be used on ListViews");
    }

    //Setup our event handlers for this list view.
    lv.Loaded += new RoutedEventHandler(lv_Loaded);
    lv.SizeChanged += new SizeChangedEventHandler(lv_SizeChanged);
    return value;
  }

  ///
  /// Handles the SizeChanged event of the lv control.
  ///
  /// The source of the event.
  /// The instance containing the event data.
  private static void lv_SizeChanged(object sender, SizeChangedEventArgs e) {
    ListView lv = (sender as ListView);
    if (lv.IsLoaded) {
      //Set our initial widths.
      SetColumnWidths(lv);
    }
  }

  ///
  /// Handles the Loaded event of the lv control.
  ///
  /// The source of the event.
  /// The instance containing the event data.
  private static void lv_Loaded(object sender, RoutedEventArgs e) {
    ListView lv = (sender as ListView);
    //Set our initial widths.
    SetColumnWidths(lv);
  }

  ///
  /// Sets the column widths.
  ///
  private static void SetColumnWidths(ListView listView) {
    //Pull the stretch columns fromt the tag property.
    List<GridViewColumn> columns = (listView.Tag as List<GridViewColumn>);
    double specifiedWidth = 0;
    GridView gridView = listView.View as GridView;
    if (gridView != null) {
      if (columns == null) {
        //Instance if its our first run.
        columns = new List<GridViewColumn>();
        // Get all columns with no width having been set.
        foreach (GridViewColumn column in gridView.Columns) {
          if (!(column.Width >= 0)) {
            columns.Add(column);
          } else {
            specifiedWidth += column.ActualWidth;
          }
        }
      } else {
        // Get all columns with no width having been set.
        foreach (GridViewColumn column in gridView.Columns) {
          if (!columns.Contains(column)) {
            specifiedWidth += column.ActualWidth;
          }
        }
      }

      // Allocate remaining space equally.
      foreach (GridViewColumn column in columns) {
        double newWidth = (listView.ActualWidth - specifiedWidth) / columns.Count;
        if (newWidth >= 10) {
          column.Width = newWidth - 10;
        }
      }

      //Store the columns in the TAG property for later use.
      listView.Tag = columns;
    }
  }
}
