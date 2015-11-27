using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using BExplorer.Shell.Interop;
using Application = System.Windows.Application;

namespace BExplorer.Shell {
	#region WindowActivator
	public abstract class WindowActivator {

		public abstract void ActivateForm(Window form, Window window, IntPtr hwnd);
	}
	#endregion

	#region BringToFrontActivator
	public class BringToFrontActivator : WindowActivator {
		public override void ActivateForm(Window form, Window window, IntPtr hwnd) {
			var fHandle = (PresentationSource.FromVisual(form) as HwndSource).Handle;
			var wHandle = (PresentationSource.FromVisual(window) as HwndSource).Handle;
			if (window == null || wHandle != fHandle)
			{

				form.Topmost = true;
				form.Topmost = false;
				form.Focus();
				form.Show();
				form.Activate();

				// stop flashing...happens occassionally when switching quickly when activate manuver is fails
				Shell32.FlashWindow(fHandle, 0);
			}
		}
	}
	#endregion

	#region TopMostActivator
	public class TopMostActivator : WindowActivator {
		public override void ActivateForm(Window form, Window window, IntPtr hwnd) {
			var fHandle = (PresentationSource.FromVisual(form) as HwndSource).Handle;
			var wHandle = (PresentationSource.FromVisual(window) as HwndSource).Handle;
			if (window == null || wHandle != fHandle) { 

				// bring to top
				form.Topmost = true;
				form.Topmost = false;

				// set as active form in task bar
				form.Activate();

				// stop flashing...happens occassionally when switching quickly when activate manuver is fails
				Shell32.FlashWindow(fHandle, 0);
			}
		}
	}
	#endregion

	#region SetFGWindowActivator
	public class SetFGWindowActivator : WindowActivator {
		public override void ActivateForm(Window form, Window window, IntPtr hwnd) {
			var fHandle = (PresentationSource.FromVisual(form) as HwndSource).Handle;
			var wHandle = (PresentationSource.FromVisual(window) as HwndSource).Handle;
			if (window == null || wHandle != fHandle) {

				// bring to top
				form.Topmost = true;
				form.Topmost = false;

				// set as active form in task bar
				User32.SetForegroundWindow(fHandle);

				// stop flashing...happens occassionally when switching quickly when activate manuver is fails
				Shell32.FlashWindow(fHandle, 0);
			}
		}
	}
	#endregion

	#region RestoreWindowActivator
	public class RestoreWindowActivator : WindowActivator {
		public override void ActivateForm(Window form, Window window, IntPtr hwnd) {
			var fHandle = (PresentationSource.FromVisual(form) as HwndSource).Handle;
			var wHandle = (PresentationSource.FromVisual(window) as HwndSource).Handle;
			if (window == null || wHandle != fHandle) {

				// set as active form in task bar
				User32.ShowWindow(fHandle, User32.ShowWindowCommands.Restore);
				User32.SetForegroundWindow(fHandle);

				// stop flashing...happens occassionally when switching quickly when activate manuver is fails
				Shell32.FlashWindow(fHandle, 0);
			}
		}
	}
	#endregion

	#region SetFGAttachThreadWindowActivator
	public class SetFGAttachThreadWindowActivator : WindowActivator {
		public override void ActivateForm(Window form, Window window, IntPtr hwnd) {
			var fHandle = (PresentationSource.FromVisual(form) as HwndSource).Handle;
			var wHandle = (PresentationSource.FromVisual(window) as HwndSource).Handle;
			if (window == null || wHandle != fHandle) {
				uint fgProcessId;
				uint spProcessId;
				User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out fgProcessId);
				User32.GetWindowThreadProcessId(fHandle, out spProcessId);

				if (fgProcessId != spProcessId) {
					if (User32.AttachThreadInput(fgProcessId, spProcessId, true)) {
						User32.SetForegroundWindow(fHandle);
						User32.AttachThreadInput(fgProcessId, spProcessId, false);
					}
				} else {
					User32.SetForegroundWindow(fHandle);
				}

				// stop flashing...happens occassionally when switching quickly when activate manuver is fails
				Shell32.FlashWindow(fHandle, 0);
			}
		}
	}
	#endregion

	#region CombinedWindowActivator

	/// <summary>
	/// Uses all the tricks in the book at once to do it...shady
	/// </summary>
	public class CombinedWindowActivator : WindowActivator {
		private const int SW_HIDE = 0;
		private const int SW_SHOWNORMAL = 1;
		private const int SW_NORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;
		private const int SW_MAXIMIZE = 3;
		private const int SW_SHOWNOACTIVATE = 4;
		private const int SW_SHOW = 5;
		private const int SW_MINIMIZE = 6;
		private const int SW_SHOWMINNOACTIVE = 7;
		private const int SW_SHOWNA = 8;
		private const int SW_RESTORE = 9;
		private const int SW_SHOWDEFAULT = 10;
		private const int SW_MAX = 10;

		private const uint SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
		private const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
		private const int SPIF_SENDCHANGE = 0x2;

		public override void ActivateForm(Window form, Window window, IntPtr hwnd) {
			var fHandle = (PresentationSource.FromVisual(form) as HwndSource).Handle;
			var wHandle = window == null ? IntPtr.Zero : (PresentationSource.FromVisual(window) as HwndSource).Handle;
			if (window == null || wHandle != fHandle) {

				IntPtr Dummy = IntPtr.Zero;

				IntPtr hWnd = fHandle;
				if (User32.IsIconic(hWnd)) {
					User32.ShowWindowAsync(hWnd, SW_RESTORE);
				} else {
					//User32.ShowWindowAsync(hWnd, SW_SHOW);
					form.ShowActivated = true;
					form.Show();
				}
				User32.SetForegroundWindow(hWnd);
				form.Activate();
				form.Topmost = true;
				form.Topmost = false;

				// Code from Karl E. Peterson, www.mvps.org/vb/sample.htm
				// Converted to Delphi by Ray Lischner
				// Published in The Delphi Magazine 55, page 16
				// Converted to C# by Kevin Gale
				IntPtr foregroundWindow = User32.GetForegroundWindow();
				if (foregroundWindow != hWnd) {
					uint foregroundThreadId = User32.GetWindowThreadProcessId(foregroundWindow, Dummy);
					uint thisThreadId = User32.GetWindowThreadProcessId(hWnd, Dummy);

					if (User32.AttachThreadInput(thisThreadId, foregroundThreadId, true)) {
						form.Activate();
						form.Topmost = true;
						form.Topmost = false;
						User32.BringWindowToTop(hWnd); // IE 5.5 related hack
						User32.SetForegroundWindow(hWnd);

						User32.AttachThreadInput(thisThreadId, foregroundThreadId, false);
					}
				}

				if (User32.GetForegroundWindow() != hWnd) {
					// Code by Daniel P. Stasinski
					// Converted to C# by Kevin Gale
					IntPtr Timeout = IntPtr.Zero;
					User32.SystemParametersInfo(SPI_GETFOREGROUNDLOCKTIMEOUT, 0, Timeout, 0);
					User32.SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, Dummy, SPIF_SENDCHANGE);
					User32.BringWindowToTop(hWnd); // IE 5.5 related hack
					User32.SetForegroundWindow(hWnd);
					form.Activate();
					form.Topmost = true;
					form.Topmost = false;
					User32.SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, Timeout, SPIF_SENDCHANGE);
				}


				Shell32.FlashWindow(fHandle, 0);

			}


		}
	}
	#endregion


	
}
