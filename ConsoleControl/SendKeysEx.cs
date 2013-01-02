using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ConsoleControl
{
    /// <summary>
    /// Provides additional functionality in the vein of SendKeys.
    /// </summary>
    public class SendKeysEx
    {
        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Sends the keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public static void SendKeys(IntPtr windowHandle, string keys)
        {
            //  Get the foreground window.
            IntPtr foregroundWindow = GetForegroundWindow();

            //  Set the specified window as the foreground window.
            SetForegroundWindow(windowHandle);

            //  Send the keys.
            System.Windows.Forms.SendKeys.Send(keys);

            //  Restore the window handle.
            SetForegroundWindow(foregroundWindow);
        }
    }
}