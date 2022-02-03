using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using WPFUI.Controls;
using WPFUI.Win32;

namespace ShellControls.ShellListView {
  public class ListViewColumnHeader : GridViewColumn {


    private ShellView _ShellViewEx;

    public Int32 MinWidth { get; set; }

    private ShellListViewColumnHeader _CollumnHeader { get; set; }
    private Boolean _SkipChange = false;

    public ListViewColumnHeader() {

    }
    public ListViewColumnHeader(ShellView shellViewEx, Collumns collumn, Boolean isSorted, Boolean isAsync = true) {
      this._ShellViewEx = shellViewEx;
      PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty, typeof(GridViewColumn));
      pd.AddValueChanged(this, SizeChanged);
      this._CollumnHeader = new ShellListViewColumnHeader();
      this._CollumnHeader.Content = collumn.Name;
      this._CollumnHeader.Collumn = collumn;
      this._CollumnHeader.SortDirection = this._ShellViewEx.LastSortOrder;
      this._CollumnHeader.Index = shellViewEx.Collumns.IndexOf(collumn);
      //this._SkipChange = true;
      if (isAsync) {
        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
          () => {
            this.MinWidth = collumn.MinWidth;
            this.Width = collumn.Width;
            this._CollumnHeader.IsSorted = isSorted;
          }));
      } else {
        this.MinWidth = collumn.MinWidth;
        this.Width = collumn.Width;
        this._CollumnHeader.IsSorted = isSorted;
      }

      this.Header = this._CollumnHeader;
      this._CollumnHeader.Click += OnHeaderColumn_Click;
    }

    public void UpdateIsSelected() {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => {
        this._CollumnHeader.IsSorted = this._CollumnHeader.Collumn.ID == this._ShellViewEx.LastSortedColumnId;
        this._CollumnHeader.SortDirection = this._ShellViewEx.LastSortOrder;
      }));
    }

    public void SetResizableState(Boolean isEnabled) {
      this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(
        () => {
          this._CollumnHeader.SetColumnsHeaderResizableState(isEnabled);
          if (!isEnabled) {
            this.Width = 200;
          }
        }));
    }
    public void SetColWidth(Int32 width, Boolean isAsync = true) {
      //this._SkipChange = true;
      if (isAsync) {
        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(
          () => this.Width = width));
      } else {
        this.Width = width;
      }
      //this.Width = width;
    }

    private void OnHeaderColumn_Click(object sender, RoutedEventArgs e) {
      this._ShellViewEx.SetSortCollumn(true, this._CollumnHeader.Collumn, this._ShellViewEx.LastSortedColumnId == this._CollumnHeader.Collumn.ID ? this._ShellViewEx.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending : SortOrder.Ascending);
      this._CollumnHeader.IsSorted = true;

    }

    private void SizeChanged(object? sender, EventArgs e) {
      if (!this._SkipChange) {
        var colWidth = (Int32)this.ActualWidth;
        //this._CollumnHeader.UpdateLayout();
        if (colWidth < this.MinWidth) {
          this.Width = this.MinWidth;
          return;
        }

        if (this._ShellViewEx.Collumns.Count > this._CollumnHeader.Index) {
          var iiListView = this._ShellViewEx.GetListViewInterface();
          if (iiListView == null) {
            return;
          }

          this.Dispatcher.BeginInvoke(DispatcherPriority.Render, (ThreadStart)(() => {
            try {
              iiListView.SetColumnWidth(this._CollumnHeader.Index, colWidth);
              this._ShellViewEx.Collumns[this._CollumnHeader.Index].Width = colWidth;
            } catch (Exception exception) { }
          }));
        }
      } else {
        this._SkipChange = false;
      }
    }
  }
}
