using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFUI.Win32;

namespace BExplorer.Shell {
  /// <summary>
  /// Interaction logic for ShellViewControl.xaml
  /// </summary>
  public partial class ShellViewControl : UserControl {
    public ShellViewControl() {
      InitializeComponent();
    }

    public void AddHeaderColumns(List<Collumns> cols) {
      var gridView = this.lvHeader.View as GridView;
      gridView.Columns.Clear();
      foreach (var col in cols) {
        var gridviewcol = new GridViewColumn();
        gridviewcol.Header = col.Name;
        gridviewcol.Width = col.Width;
        gridView.Columns.Add(gridviewcol);
      }

    }
    public void AddHeaderColumn(Collumns col) {
      var gridView = this.lvHeader.View as GridView;
        var gridviewcol = new GridViewColumn();
        gridviewcol.Header = col.Name;
        gridviewcol.Width = col.Width;
        gridView.Columns.Add(gridviewcol);
    }
    public void RemoveHeaderColumn(Collumns col) {
      var gridView = this.lvHeader.View as GridView;
      gridView.Columns.RemoveAt(col.Index);
    }

    public void ClearHeaderColumns() {
      var gridView = this.lvHeader.View as GridView;
      gridView.Columns.Clear();
    }

  }
}
