// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Windows7.DesktopIntegration.Interop;

namespace Windows7.DesktopIntegration
{
    internal sealed class ProxyWindow : Form
    {
        public CustomWindowsManager WindowsManager
        {
            private get;
            set;
        }

        private IntPtr _proxyingFor;

        public IntPtr RealWindow
        {
            get { return _proxyingFor; }
        }

        public ProxyWindow(IntPtr proxyingFor)
        {
            _proxyingFor = proxyingFor;
            Size = new Size(1, 1);

            StringBuilder text = new StringBuilder(256);
            UnsafeNativeMethods.GetWindowText(_proxyingFor, text, text.Capacity);
            Text = text.ToString();
        }

        protected override void WndProc(ref Message m)
        {
            if (WindowsManager != null)
            {
                WindowsManager.DispatchMessage(ref m);
            }
            base.WndProc(ref m);
        }
    }
}