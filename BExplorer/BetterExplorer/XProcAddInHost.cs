using System;
using System.Diagnostics;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using AddInInterface;
using System.Runtime.Remoting.Lifetime;
using System.Windows;

namespace BetterExplorer
{
    /// <summary>
    /// The AddIn object attaches itself to this "site" object. It is separate from XProcAddInHost because
    /// it needs to be derived from MBR to be remotable.
    /// </summary>
    class XProcAddInSite: MarshalByRefObject, IXProcAddInSite
    {
        XProcAddInHost _host;

        internal XProcAddInSite(XProcAddInHost host)
        {
            _host = host;
        }

        public override object InitializeLifetimeService()
        {
            // Set up the host as the sponsor of the lease.
            ILease lease = (ILease)base.InitializeLifetimeService();
            if (lease.CurrentState == LeaseState.Initial)
            {
                //lease.RenewOnCallTime = TimeSpan.FromSeconds(7); //Just for testing
                //lease.InitialLeaseTime = TimeSpan.FromSeconds(7); //
                lease.Register(_host);
            }
            return lease;
        }

        #region IXProcAddInSite implemenentation

        // Note: These calls arrive on thread pool threads. UI work must be done on the UI thread.
        // We switch easily via the Dispatcher.

        public int HostProcessId { get { return Process.GetCurrentProcess().Id; } }

        public void SetAddIn(IXProcAddIn addin)
        {
            _host.SetAddIn(addin);
        }

        public bool TabOut(TraversalRequest request)
        {
            return (bool)_host.Dispatcher.Invoke((DispatcherOperationCallback)delegate(object tr)
            {
                return _host.MoveFocus((TraversalRequest)tr);
            }, request);
        }

        public bool TranslateAccelerator(MSG msg)
        {
            return (bool)_host.Dispatcher.Invoke((DispatcherOperationCallback)delegate(object m)
            {
                // We delegate key processing to the containing HwndSource, via IKIS. It will see that 
                // a child window ("sink") has focus and will do special routing of the input events using
                // that child window as the forced target. Thus, any element up to the root visual has a 
                // chance to see and handle the events.
                Debug.Assert(((IKeyboardInputSink)_host).HasFocusWithin());
                IKeyboardInputSink hostSink = (IKeyboardInputSink)PresentationSource.FromVisual(_host);
                MSG m2 = (MSG)m;
                // - IKIS has special rules about which messages are passed to which method.
                // Here we assume the add-in bubbles only raw key messages plus mnemonic keys (Alt+something).
                // - Even though the call has hopped from thread to thread, Keyboard.Modifiers produces
                // correct result here because the input queue and states of the host's and add-in's UI
                // threads are synchronized. (The window manager automatically attaches thread input when
                // a window is parented to a cross-thread one.)
                if (m2.message == Win32.WM_SYSCHAR || m2.message == Win32.WM_SYSDEADCHAR)
                    return hostSink.OnMnemonic(ref m2, Keyboard.Modifiers);
                else
                    return hostSink.TranslateAccelerator(ref m2, Keyboard.Modifiers);
            }, msg);
        }

        #endregion
    };

    class XProcAddInHost: HwndHost, IKeyboardInputSink, ISponsor
    {
        static IpcServerChannel _ipcChannel = Utils.RegisterServerChannel("BEFileOperationHost");
        static int _addinCounter;
        static DispatcherWorkerThread _workerThread = new DispatcherWorkerThread();

        XProcAddInSite _site;
        ManualResetEvent _addinAvailableEvent = new ManualResetEvent(false);
        IXProcAddIn _addin;

        internal XProcAddInHost()
        {
            // Do all startup work asyncrhonously for better responsiveness.
            Action d = delegate
            {
                _site = new XProcAddInSite(this);
                // Register the site object with RemotingServices so that the add-in can connect to it.
                // The URI is passed to addin.exe's command line.
                string addinSiteUri = "site"+(++_addinCounter);
                RemotingServices.Marshal(_site, addinSiteUri);
                addinSiteUri = _ipcChannel.GetChannelUri()+"/"+addinSiteUri;
                Process addinProcess = Process.Start(
                    AppDomain.CurrentDomain.BaseDirectory + "\\BEFileOperation.exe", addinSiteUri);
            };
            d.BeginInvoke(null, null);
        }

        internal event EventHandler AddInAvailableAsync;

        internal void SetAddIn(IXProcAddIn addin)
        {
            _addin = addin;
            _addinAvailableEvent.Set();
            if (AddInAvailableAsync != null)
                AddInAvailableAsync(this, null);
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _addinAvailableEvent.WaitOne();

            IntPtr addinWindow = _addin.AddInWindow;
            Win32.SetParent(addinWindow, hwndParent.Handle);

            Action onAttached = _addin.OnAddInAttached;
            onAttached.BeginInvoke(null, null);

            return new HandleRef(this, addinWindow);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _addin.ShutDown();
        }

        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            Win32.SetWindowPos(Handle, IntPtr.Zero,
                                           (int)rcBoundingBox.X,
                                           (int)rcBoundingBox.Y,
                                           (int)rcBoundingBox.Width,
                                           (int)rcBoundingBox.Height,
                                           Win32.SWP_ASYNCWINDOWPOS
                                           | Win32.SWP_NOZORDER
                                           | Win32.SWP_NOACTIVATE); 
        }

        bool IKeyboardInputSink.OnMnemonic(ref MSG msg, ModifierKeys modifiers)
        {
            //TODO: Optionally, forward mnemonics "down" to the add-in's HwndSource (via IKIS).
            // This would look much like TranslateAccelerator but going in the opposite direction.
            return false;
        }

        bool IKeyboardInputSink.TabInto(TraversalRequest request)
        {
            // Same problem as in AddIn.IKeyboardInputSite.OnNoMoreTabStops(): The UI threads deadlock because
            // of IPC's hard blocking and because focus change entails sending window messages. 
            // Workaround is to do the call through a worker thread while this one remains responsive to 
            // window messages (thanks to the DispatcherSynchonizationContext).
            return (bool)_workerThread.Dispatcher.Invoke((DispatcherOperationCallback)delegate(object tr)
            {
                return _addin.TabInto((TraversalRequest)tr);
            }, request);
        }

        TimeSpan ISponsor.Renewal(ILease lease)
        {
            return Handle == IntPtr.Zero ? TimeSpan.Zero : lease.RenewOnCallTime;
        }
    };
}
