using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Fluent;

namespace BExplorer.Shell {
  /// <summary>
  /// Interaction logic for CollisionDialog.xaml
  /// </summary>
  public partial class CollisionDialog : RibbonWindow {
    public List<CollisionItem> Collisions { get; set; }
    public CollisionDialog() {
      this.Collisions = new List<CollisionItem>();
      this.DataContext = this;
      InitializeComponent();
    }
  }
}
