using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BExplorer.Shell {
  public class HandlingEventTrigger : System.Windows.Interactivity.EventTrigger
  {
    protected override void OnEvent(System.EventArgs eventArgs)
    {
      var routedEventArgs = eventArgs as RoutedEventArgs;
      if (routedEventArgs != null)
        routedEventArgs.Handled = true;

      base.OnEvent(eventArgs);
    }
  }
}
