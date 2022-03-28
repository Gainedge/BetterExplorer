using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using ShellControls.ShellListView;
using WPFUI.Win32;

namespace ShellControls {
  public static class Extensions {
    /// <summary>
    /// Sets the <paramref name="row"/> and <paramref name="column"/> based on the row and column that the <paramref name="hitPoint">point</paramref> falls on
    /// </summary>
    /// <param name="shellView">The <see cref="ShellView"/> you want to test with</param>
    /// <param name="hitPoint">The point on the screen you awnt to look for</param>
    /// <param name="row">The value for row that was hit</param>
    /// <param name="column">The value for column that was hit</param>
    /// <returns>Was the <paramref name="shellView"/> hit at all?</returns>
    public static bool HitTest(this ShellView shellView, Point hitPoint, out int row, out int column) {
      // clear the output values
      row = column = -1;

      // set up some win32 api constant values
      const int LVM_FIRST = 0x1000;
      //const int LVM_SUBITEMHITTEST = (LVM_FIRST + 57);
      const int LVM_HITTEST = (LVM_FIRST + 18);

      const int LVHT_NOWHERE = 0x1;
      const int LVHT_ONITEMICON = 0x2;
      const int LVHT_ONITEMLABEL = 0x4;
      const int LVHT_ONITEMSTATEICON = 0x8;
      const int LVHT_EX_ONCONTENTS = 0x04000000;
      const int LVHT_ONITEM = (LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON | LVHT_EX_ONCONTENTS);

      // set up the return value
      bool hitLocationFound = false;

      // initialize a hittest information structure
      LVHITTESTINFO lvHitTestInfo = new LVHITTESTINFO();
      lvHitTestInfo.pt.x = hitPoint.X;
      lvHitTestInfo.pt.y = hitPoint.Y;

      // send the hittest message to find out where the click was
      if (User32.SendMessage(shellView.LVHandle, LVM_HITTEST, 0, ref lvHitTestInfo) != 0) {
        bool nowhere = ((lvHitTestInfo.flags & ListViewHitTestFlag.LVHT_NOWHERE) != 0);
        bool onItem = ((lvHitTestInfo.flags & ListViewHitTestFlag.LVHT_ONITEM) != 0);

        if (onItem && !nowhere) {
          row = lvHitTestInfo.iItem;
          column = lvHitTestInfo.iSubItem;
          hitLocationFound = true;
        }
      } else if (User32.SendMessage(shellView.LVHandle, LVM_FIRST, 0, ref lvHitTestInfo) != 0) {
        row = 0;
        hitLocationFound = true;
      }

      return hitLocationFound;
    }

    public static IOrderedEnumerable<IListItemEx> SetSortCollumn(this IEnumerable<IListItemEx> items, ShellView view, Boolean isReorder, Collumns column, SortOrder order, Boolean reverseOrder = true) {
      if (column == null) {
        column = view.Collumns.FirstOrDefault();
      }

      try {
        IOrderedEnumerable<IListItemEx> result = null;
        //var selectedItems = this.SelectedItems.ToArray();
        if (column.ID == view.LastSortedColumnId && reverseOrder) {
          // Reverse the current sort direction for this column.
          view.LastSortOrder = view.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        } else {
          // Set the column number that is to be sorted; default to ascending.
          view.LastSortedColumnId = column.ID;
          view.LastSortOrder = order;
        }

        if (isReorder) {
          var itemsQuery = items.Where(w => view.ShowHidden || !w.IsHidden).OrderByDescending(o => o.IsFolder);
          if (column.CollumnType != typeof(String)) {
            if (order == SortOrder.Ascending) {
              result = itemsQuery.ThenBy(o => o.GetPropertyValue(column.pkey, typeof(String)).Value ?? "1");
            } else {
              result = itemsQuery.ThenByDescending(o => o.GetPropertyValue(column.pkey, typeof(String)).Value ?? "1");
            }
          } else {
            if (order == SortOrder.Ascending) {
              result = itemsQuery.ThenBy(o => o.GetPropertyValue(column.pkey, typeof(String)).Value?.ToString(), NaturalStringComparer.Default);
            } else {
              result = itemsQuery.ThenByDescending(o => o.GetPropertyValue(column.pkey, typeof(String)).Value?.ToString(), NaturalStringComparer.Default);
            }
          }

          var i = 0;
        }


        //var colIndexReal = view.Collumns.IndexOf(view.Collumns.FirstOrDefault(w => w.ID == view.LastSortedColumnId));
        //if (colIndexReal > -1) {
        //  User32.SendMessage(view.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, colIndexReal, 0);
        //  view.SetSortIcon(colIndexReal, order);
        //} else {
        //  User32.SendMessage(view.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, -1, 0);
        //}

        //if (!this.IsRenameInProgress) {
        //  this.SelectItems(selectedItems);
        //}
        return result;
      } catch {
        return null;
      }
    }
    public static string RemoveExtensionsFromFile(this string file, string ext) {
      return file.EndsWith(ext) ? file.Remove(file.LastIndexOf(ext, StringComparison.Ordinal), ext.Length) : file;
    }

    public static String ToPrettyFormattedString(this DateTime dateTime) {
      const int SECOND = 1;
      const int MINUTE = 60 * SECOND;
      const int HOUR = 60 * MINUTE;
      const int DAY = 24 * HOUR;
      const int MONTH = 30 * DAY;

      TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);
      double seconds = ts.TotalSeconds;

      // Less than one minute
      if (seconds < 1 * MINUTE)
        return ts.Seconds == 1 ? "A second ago" : ts.Seconds + " seconds ago";

      if (seconds < 60 * MINUTE)
        return ts.Minutes + " minutes ago";

      if (seconds < 120 * MINUTE)
        return "An hour ago";

      if (seconds < 24 * HOUR)
        return ts.Hours + " hours ago";

      if (seconds < 48 * HOUR)
        return "Yesterday";

      if (seconds < 30 * DAY)
        return ts.Days + " days ago";
      var is12Hours = CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern.Contains("tt");
      //if (seconds < 12 * MONTH) {
        int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
        //return months <= 1 ? "one month ago" : dateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss", CultureInfo.GetCultureInfo(Kernel32.GetUserDefaultLCID()));
        return months <= 1 ? "one month ago" : dateTime.ToString("dddd, dd MMMM yyyy   "+ (is12Hours ? "hh:mm tt" : "HH:mm"), CultureInfo.CurrentUICulture);
      //}

      //int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
      //return years <= 1 ? "one year ago" : years + " years ago";
    }

  }
}
