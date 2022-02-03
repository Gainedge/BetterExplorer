using System.Windows;
using System.Windows.Controls;

namespace ShellControls.ShellContextMenu {
  public class BaseCommandsPanel: Control {
    static BaseCommandsPanel() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseCommandsPanel), new FrameworkPropertyMetadata(typeof(BaseCommandsPanel)));
    }
  }
}
