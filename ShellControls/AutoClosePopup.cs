using System;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace ShellControls;
public class AutoClosePopup : Popup {
  private IntPtr _winHook;
  private IntPtr _Handle;
  /// <summary>
  /// 
  /// </summary>
  public AutoClosePopup() {

  }

  protected override void OnOpened(EventArgs e) {
    base.OnOpened(e);

    this._Handle = ((HwndSource)HwndSource.FromVisual(this.Child)).Handle;

    dele = new WinEventDelegate(WinEventProc);
    this._winHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

  }

  protected override void OnClosed(EventArgs e) {
    base.OnClosed(e);
    UnhookWinEvent(this._winHook);
  }

  public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
    if (hwnd != this._Handle) {
      this.IsOpen = false;
    }
  }

  delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
  WinEventDelegate dele = null;
  [DllImport("user32.dll")]
  static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
  private const uint WINEVENT_OUTOFCONTEXT = 0;
  private const uint EVENT_SYSTEM_FOREGROUND = 3;

  [DllImport("user32.dll")]
  static extern bool UnhookWinEvent(IntPtr hWinEventHook);

}
