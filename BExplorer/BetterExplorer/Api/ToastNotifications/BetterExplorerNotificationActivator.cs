using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BetterExplorer.Api.ToastNotifications {
  [ClassInterface(ClassInterfaceType.None)]
  [ComSourceInterfaces(typeof(INotificationActivationCallback))]
  [Guid("A0176474-ED11-437F-8EBF-B53D64C41FC3"), ComVisible(true)]
  public class BetterExplorerNotificationActivator : NotificationActivator {
    public override void OnActivated(String arguments, NotificationUserInput userInput, String appUserModelId) {
      
    }
  }
}
