using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell._Plugin_Interfaces;

namespace BExplorer.Shell {
  public class CollisionItem {
    public IListItemEx FirstItem { get; private set; }
    public Boolean FirstItemChecked { get; set; }
    public IListItemEx SecondItem { get; private set; }
    public Boolean SecondItemChecked { get; set; }

    public CollisionItem(IListItemEx firstItem, IListItemEx secondItem) {
      this.FirstItem = firstItem;
      this.SecondItem = secondItem;
    }
  }
}
