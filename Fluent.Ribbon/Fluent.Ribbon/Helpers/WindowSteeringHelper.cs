using System.Runtime.InteropServices;

namespace Fluent.Helpers {
  using System;
  using System.Reflection;
  using System.Windows;
  using System.Windows.Input;

  /// <summary>
  /// Class which offers helper methods for steering the window
  /// </summary>
  public static class WindowSteeringHelper {
    private static readonly PropertyInfo criticalHandlePropertyInfo = typeof(Window).GetProperty("CriticalHandle", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly object[] emptyObjectArray = new object[0];

    /// <summary>
    /// Shows the system menu at the current mouse position.
    /// </summary>
    /// <param name="e">The mouse event args.</param>
    /// <param name="handleDragMove">Defines if window dragging should be handled.</param>
    /// <param name="handleStateChange">Defines if window state changes should be handled.</param>
    public static void HandleMouseLeftButtonDown(MouseButtonEventArgs e, bool handleDragMove, bool handleStateChange) {
      var dependencyObject = e.Source as DependencyObject;

      if (dependencyObject == null) {
        return;
      }

      HandleMouseLeftButtonDown(dependencyObject, e, handleDragMove, handleStateChange);
    }

    [DllImport("user32.dll")]
    static extern bool ReleaseCapture();
    [DllImport("User32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

#pragma warning disable 618
    /// <summary>
    /// Shows the system menu at the current mouse position.
    /// </summary>
    /// <param name="dependencyObject">The object which was the source of the mouse event.</param>
    /// <param name="e">The mouse event args.</param>
    /// <param name="handleDragMove">Defines if window dragging should be handled.</param>
    /// <param name="handleStateChange">Defines if window state changes should be handled.</param>
    public static void HandleMouseLeftButtonDown(DependencyObject dependencyObject, MouseButtonEventArgs e, bool handleDragMove, bool handleStateChange) {
      var window = Window.GetWindow(dependencyObject);

      if (window == null) {
        return;
      }

      if (handleDragMove
          && e.ClickCount == 1) {
        e.Handled = true;

        // taken from DragMove internal code
        window.VerifyAccess();

        // for the touch usage
        ReleaseCapture();

        var criticalHandle = (IntPtr)criticalHandlePropertyInfo.GetValue(window, emptyObjectArray);
        // DragMove works too, but not on maximized windows
        SendMessage(criticalHandle, 0x0112, 0xF012, 0);
        SendMessage(criticalHandle, 0x0202, 0, 0);
      } else if (handleStateChange
            && e.ClickCount == 2
            && (window.ResizeMode == ResizeMode.CanResize || window.ResizeMode == ResizeMode.CanResizeWithGrip)) {
        e.Handled = true;

        if (window.WindowState == WindowState.Normal) {
          SystemCommands.MaximizeWindow(window);
        } else {
          SystemCommands.RestoreWindow(window);
        }
      }
    }

#pragma warning restore 618

    /// <summary>
    /// Shows the system menu at the current mouse position.
    /// </summary>
    /// <param name="dependencyObject">The object which was the source of the mouse event.</param>
    /// <param name="e">The mouse event args.</param>
    public static void ShowSystemMenu(DependencyObject dependencyObject, MouseButtonEventArgs e) {
      var window = Window.GetWindow(dependencyObject);

      if (window == null) {
        return;
      }

      ShowSystemMenu(window, e);
    }

    /// <summary>
    /// Shows the system menu at the current mouse position.
    /// </summary>
    /// <param name="window">The window for which the system menu should be shown.</param>
    /// <param name="e">The mouse event args.</param>
    public static void ShowSystemMenu(Window window, MouseButtonEventArgs e) {
      e.Handled = true;

#pragma warning disable 618
      SystemCommands.ShowSystemMenu(window, window.PointToScreen(e.GetPosition(window)));
#pragma warning restore 618
    }

    /// <summary>
    /// Shows the system menu at <paramref name="screenLocation"/>.
    /// </summary>
    /// <param name="window">The window for which the system menu should be shown.</param>
    /// <param name="screenLocation">The location at which the system menu should be shown.</param>
    public static void ShowSystemMenu(Window window, Point screenLocation) {
#pragma warning disable 618
      SystemCommands.ShowSystemMenu(window, screenLocation);
#pragma warning restore 618
    }
  }
}