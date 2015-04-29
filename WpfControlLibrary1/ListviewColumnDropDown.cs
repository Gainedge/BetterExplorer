using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterExplorerControls {
  public class ListviewColumnDropDown : ContextMenu {

    static ListviewColumnDropDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ListviewColumnDropDown),
            new FrameworkPropertyMetadata(typeof(ListviewColumnDropDown)));
    }
    public void AddItem(object Item) {
      MenuItem mnuitem = new MenuItem();
      mnuitem.IsCheckable = true;
      mnuitem.Header = Item;

      this.Items.Add(mnuitem);
      //CheckBox.Checked += CheckBox_Checked;
      //CheckBox.Unchecked += CheckBox_Unchecked;

      //sPanel.Children.Add(CheckBox);
      //return this;
    }
  }
}
