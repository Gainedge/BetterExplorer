using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;

namespace BetterExplorer
{
    /// <summary>
    /// simple no-arg delegate type; can use this for anonymous methods, e.g.
    /// <code>
    ///     SafeThread safeThrd = new SafeThread((SimpleDelegate) delegate { dosomething(); });
    /// </code>
    /// </summary>
    public delegate void SimpleDelegate();

    /// <summary>
    /// delegate for thread-threw-exception event
    /// </summary>
    /// <param name="thrd">the SafeThread that threw the exception</param>
    /// <param name="ex">the exception throws</param>
    public delegate void ThreadThrewExceptionHandler(SafeThread thrd, Exception ex);

    /// <summary>
    /// delegate for thread-completed event
    /// </summary>
    /// <param name="thrd">the SafeThread that completed processing</param>
    /// <param name="hadException">true if the thread terminated due to an exception</param>
    /// <param name="ex">the exception that terminated the thread, or null if completed successfully</param>
    public delegate void ThreadCompletedHandler(SafeThread thrd, bool hadException, Exception ex);

    /// <summary>
    /// This class implements a Thread wrapper to trap unhandled exceptions 
    /// thrown by the thread-start delegate. Add ThreadException event 
    /// handlers to be notified of such exceptions and take custom actions
    /// (such as restart, clean-up, et al, depending on what the SafeThread was
    /// doing in your application). Add ThreadCompleted event handlers to be
    /// notified when the thread has completed processing.
    /// </summary>
    public partial class SafeThread : MarshalByRefObject
    {
        /// <summary>
        /// internal thread
        /// </summary>
        protected Thread _thread;
        /// <summary>
        /// gets the internal thread being used
        /// </summary>
        public Thread ThreadObject
        {
            get { return _thread; }
        }

        /// <summary>
        /// the thread-start object, if any
        /// </summary>
        protected ThreadStart _Ts;
        /// <summary>
        /// the parameterized thread-start object, if any
        /// </summary>
        protected ParameterizedThreadStart _Pts;
        /// <summary>
        /// the SimpleDelegate target, if any
        /// </summary>
        protected SimpleDelegate dlg;

        /// <summary>
        /// the thread-start argument object, if any
        /// </summary>
        protected object _arg;
        /// <summary>
        /// gets the thread-start argument, if any
        /// </summary>
        public object ThreadStartArg
        {
            get { return _arg; }
        }
        /// <summary>
        /// the last exception thrown
        /// </summary>
        protected Exception _lastException;
        /// <summary>
        /// gets the last exception thrown
        /// </summary>
        public Exception LastException
        {
            get { return _lastException; }
        }
        /// <summary>
        /// the name of the internal thread
        /// </summary>
        private string _name;
        /// <summary>
        /// gets/sets the name of the internal thread
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null) { return "SafeThread#" + this.GetHashCode().ToString(); }
                return _name;
            }
            set { _name = value; }
        }

        private object _tag;
        /// <summary>
        /// object tag - use to hold extra info about the SafeThread
        /// </summary>
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        /// <summary>
        /// default constructor for SafeThread
        /// </summary>
        public SafeThread()
            : base()
        {
        }

        /// <summary>
        /// SafeThread constructor using ThreadStart object
        /// </summary>
        /// <param name="ts">ThreadStart object to use</param>
        public SafeThread(ThreadStart ts)
            : this()
        {
            _Ts = ts;
            _thread = new Thread(ts);
        }

        /// <summary>
        /// SafeThread constructor using ParameterizedThreadStart object
        /// </summary>
        /// <param name="pts">ParameterizedThreadStart to use</param>
        public SafeThread(ParameterizedThreadStart pts)
            : this()
        {
            _Pts = pts;
            _thread = new Thread(pts);
        }

        /// <summary>
        /// SafeThread constructor using SimpleDelegate object for anonymous methods, e.g.
        /// <code>
        ///     SafeThread safeThrd = new SafeThread((SimpleDelegate) delegate { dosomething(); });
        /// </code>
        /// </summary>
        /// <param name="sd"></param>
        public SafeThread(SimpleDelegate sd)
            : this()
        {
            dlg = sd;
            _Pts = new ParameterizedThreadStart(this.CallDelegate);
            _thread = new Thread(_Pts);
        }

        /// <summary>
        /// thread-threw-exception event
        /// </summary>
        public event ThreadThrewExceptionHandler ThreadException;

        /// <summary>
        /// called when a thread throws an exception
        /// </summary>
        /// <param name="ex">Exception thrown</param>
        protected void OnThreadException(Exception ex)
        {
            try
            {
                if (ex is ThreadAbortException && !ShouldReportThreadAbort)
                {
                    return;
                }

                if (ThreadException != null)
                {
                    ThreadException.Invoke(this, ex);
                }
            }
            catch { }
        }

        /// <summary>
        /// thread-completed event
        /// </summary>
        public event ThreadCompletedHandler ThreadCompleted;

        /// <summary>
        /// called when a thread completes processing
        /// </summary>
        protected void OnThreadCompleted(bool bHadException, Exception ex)
        {
            try
            {
                if (ThreadCompleted != null)
                {
                    ThreadCompleted.Invoke(this, bHadException, ex);
                }
            }
            catch { }
        }

        /// <summary>
        /// starts thread with target if any
        /// </summary>
        protected void startTarget()
        {
            Exception exceptn = null;
            bool bHadException = false;
            try
            {
                bThreadIsAborting = false;
                if (_Ts != null)
                {
                    _Ts.Invoke();
                }
                else if (_Pts != null)
                {
                    _Pts.Invoke(_arg);
                }
            }
            catch (Exception ex)
            {
                bHadException = true;
                exceptn = ex;
                this._lastException = ex;
                OnThreadException(ex);
            }
            finally
            {
                OnThreadCompleted(bHadException, exceptn);
            }
        }

        /// <summary>
        /// thread-start internal method for SimpleDelegate target
        /// </summary>
        /// <param name="arg">unused</param>
        protected void CallDelegate(object arg)
        {
            this.dlg.Invoke();
        }

        /// <summary>
        /// starts thread execution
        /// </summary>
        public void Start()
        {
            _thread = new Thread(new ThreadStart(startTarget));
            _thread.Name = this.Name;
            if (_aptState != null) { _thread.TrySetApartmentState((ApartmentState)_aptState); }
            _thread.Start();
        }

        /// <summary>
        /// starts thread execution with parameter
        /// </summary>
        /// <param name="val">parameter object</param>
        public void Start(object val)
        {
            _arg = val;
            Start();
        }

        /// <summary>
        /// flag to control whether thread-abort exception is reported or not
        /// </summary>
        protected bool bReportThreadAbort = false;
        /// <summary>
        /// gets/sets a flag to control whether thread-abort exception is reported or not
        /// </summary>
        public bool ShouldReportThreadAbort
        {
            get { return bReportThreadAbort; }
            set { bReportThreadAbort = value; }
        }

        /// <summary>
        /// flag for when thread is aborting
        /// </summary>
        protected bool bThreadIsAborting = false;

        /// <summary>
        /// abort the thread execution
        /// </summary>
        public void Abort()
        {
            bThreadIsAborting = true;
            _thread.Abort();
        }

        /// <summary>
        /// gets or sets the Culture for the current thread.
        /// </summary>
        public System.Globalization.CultureInfo CurrentCulture
        {
            get { if (_thread != null) { return _thread.CurrentCulture; } return null; }
        }

        /// <summary>
        /// gets or sets the current culture used by the Resource Manager
        /// to look up culture-specific resources at run time.
        /// </summary>
        public System.Globalization.CultureInfo CurrentUICulture
        {
            get { if (_thread != null) { return _thread.CurrentUICulture; } return null; }
        }

        /// <summary>
        /// gets an System.Threading.ExecutionContext object that contains information
        /// about the various contexts of the current thread.
        /// </summary>
        public ExecutionContext ExecutionContext
        {
            get { if (_thread != null) { return _thread.ExecutionContext; } return null; }
        }

        /// <summary>
        /// Returns an System.Threading.ApartmentState value indicating the apparent state.
        /// </summary>
        /// <returns></returns>
        public ApartmentState GetApartmentState()
        {
            if (_thread != null) { return _thread.GetApartmentState(); }
            return ApartmentState.Unknown;
        }

        /// <summary>
        /// Interrupts a thread that is in the WaitSleepJoin thread state.
        /// </summary>
        public void Interrupt()
        {
            if (_thread != null) { _thread.Interrupt(); }
        }

        /// <summary>
        /// gets a value indicating the execution status of the thread
        /// </summary>
        public bool IsAlive
        {
            get { if (_thread != null) { return _thread.IsAlive; } return false; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not a thread is a background thread
        /// </summary>
        public bool IsBackground
        {
            get { if (_thread != null) { return _thread.IsBackground; } return false; }
            set { if (_thread != null) { _thread.IsBackground = value; } }
        }

        /// <summary>
        /// gets a value indicating whether or not a thread belongs to the managed thread pool
        /// </summary>
        public bool IsThreadPoolThread
        {
            get { if (_thread != null) { return _thread.IsThreadPoolThread; } return false; }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates,
        /// while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        public void Join()
        {
            if (_thread != null) { _thread.Join(); }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses,
        /// while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        /// <param name="millisecondsTimeout">the number of milliseconds to wait for the
        /// thread to terminate</param>
        public bool Join(int millisecondsTimeout)
        {
            if (_thread != null) { return _thread.Join(millisecondsTimeout); }
            return false;
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses,
        /// while continuing to perform standard COM and SendMessage pumping.
        /// </summary>
        /// <param name="timeout">a System.TimeSpan set to the amount of time to wait
        /// for the thread to terminate </param>
        public bool Join(TimeSpan timeout)
        {
            if (_thread != null) { return _thread.Join(timeout); }
            return false;
        }

        /// <summary>
        /// Gets a unique identifier for the current managed thread
        /// </summary>
        public int ManagedThreadId
        {
            get { if (_thread != null) { return _thread.ManagedThreadId; } return 0; }
        }

        /// <summary>
        /// gets or sets a value indicating the scheduling priority of a thread
        /// </summary>
        public ThreadPriority Priority
        {
            get { if (_thread != null) { return _thread.Priority; } return ThreadPriority.Lowest; }
            set { if (_thread != null) { _thread.Priority = value; } }
        }

        private object _aptState;
        /// <summary>
        /// sets the ApartmentState of a thread before it is started
        /// </summary>
        /// <param name="state">ApartmentState</param>
        public void SetApartmentState(ApartmentState state)
        {
            if (_thread != null) { _thread.SetApartmentState(state); }
            else { _aptState = state; }
        }

        /// <summary>
        /// gets a value containing the states of the current thread
        /// </summary>
        public ThreadState ThreadState
        {
            get { if (_thread != null) { return _thread.ThreadState; } return ThreadState.Unstarted; }
        }

        /// <summary>
        /// returns a System.String that represents the current System.Object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_thread != null) { return _thread.ToString(); }
            return base.ToString();
        }

        /// <summary>
        /// sets the ApartmentState of a thread before it is started
        /// </summary>
        /// <param name="state">ApartmentState</param>
        public bool TrySetApartmentState(ApartmentState state)
        {
            if (_thread != null) { return _thread.TrySetApartmentState(state); }
            _aptState = state;
            return false;
        }
    }
}
