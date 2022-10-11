using System.Collections.Generic;
using WPFUI.Controls;

namespace BExplorer.Shell {
  /// <summary>
  /// Interaction logic for CollisionDialog.xaml
  /// </summary>
  public partial class CollisionDialog : UIWindow {
    public List<CollisionItem> Collisions { get; set; }
    public CollisionDialog() {
      this.Collisions = new List<CollisionItem>();
      this.DataContext = this;
      InitializeComponent();
    }
  }
}
