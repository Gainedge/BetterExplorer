using System;
using System.Windows;
using System.Windows.Interop;


namespace BetterExplorer {

	class Wpf32Window : System.Windows.Forms.IWin32Window {
		public IntPtr Handle { get; private set; }

		public Wpf32Window(Window wpfWindow) {
			Handle = new WindowInteropHelper(wpfWindow).Handle;
		}
	}

	public static class WindowExtensions {
		public static System.Windows.Forms.IWin32Window GetWin32Window(this Window parent) {
			return new Wpf32Window(parent);
		}
	}
}
