//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using QTTabBarLib.Interop;
using WindowsHelper;

namespace QTTabBarLib.Automation {
    // All interaction with AutomationElements MUST be done in a thread other
    // than the UI thread.  Use this class to execute code in the Automation 
    // thread.
    // http://msdn.microsoft.com/en-us/library/ee671692%28VS.85%29.aspx

    public class AutomationManager : IDisposable {
        private static readonly Guid IID_IUIAutomation = new Guid("{30CBE57D-D9D0-452A-AB13-7AC5AC4825EE}");
        private static readonly Guid CLSID_CUIAutomation = new Guid("{FF48DBA4-60EF-4201-AA87-54103EEF594E}");
        private IUIAutomation pAutomation;
        private Thread automationThread = new Thread(AutomationThreadEntry);
        private List<Worker> workerQueue = new List<Worker>();
        private volatile bool killingThread;
        private volatile bool threadDying;

        public delegate T Query<out T>(AutomationElementFactory factory);

        private interface Worker {
            void DoWork();
        }

        private class Worker<T> : Worker {
            private T ret;
            private IUIAutomation pAutomation;
            private Query<T> query;

            public Worker(IUIAutomation pAutomation, Query<T> query) {
                this.pAutomation = pAutomation;
                this.query = query;
                Complete = false;
            }

            public void DoWork() {
                try {
                    using(AutomationElementFactory man = new AutomationElementFactory(pAutomation)) {
                        ret = query(man);
                    }    
                }
                catch(Exception exception) {
                    //QTUtility2.MakeErrorLog(exception, "Automation Thread");
                }
                Complete = true;
            }

            public T GetReturn() {
                return ret;
            }

            public bool Complete { get; private set; }
        }

        public AutomationManager() {
            Guid rclsid = CLSID_CUIAutomation;
            Guid riid = IID_IUIAutomation;
            object obj;
            WindowsAPI.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj);
            pAutomation = obj as IUIAutomation;
            automationThread.Start(this);
        }

        ~AutomationManager() {
            Dispose();
        }

        public void Dispose() {
            if(automationThread.ThreadState != ThreadState.Stopped) {
                lock(automationThread) {
                    killingThread = true;
                    while(!threadDying) {
                        Monitor.PulseAll(automationThread);
                        Monitor.Wait(automationThread);
                    }
                }
            }
            if(pAutomation != null) {
                Marshal.ReleaseComObject(pAutomation);
                pAutomation = null;
            }
            GC.SuppressFinalize(this);
        }

        private static void AutomationThreadEntry(object param) {
            AutomationManager manager = (AutomationManager)param;
            lock(manager.automationThread) {
                // This if statement addresses a weird corner-case:
                // what if DoQuery or Dispose somehow got the lock
                // before we did?  If so, we can't Wait here, or 
                // we'll deadlock.
                if(manager.workerQueue.Count == 0 && !manager.killingThread) {
                    Monitor.Wait(manager.automationThread);   
                }

                while(!manager.killingThread) {
                    foreach(Worker worker in manager.workerQueue) {
                        worker.DoWork();    
                    }
                    manager.workerQueue.Clear();
                    Monitor.PulseAll(manager.automationThread);
                    Monitor.Wait(manager.automationThread);
                }
                manager.threadDying = true;
                Monitor.PulseAll(manager.automationThread);
            }
        }

        public T DoQuery<T>(Query<T> query) {
            lock(automationThread) {
                if(threadDying) return default(T);
                Worker<T> worker = new Worker<T>(pAutomation, query);
                workerQueue.Add(worker);
                do {
                    Monitor.Pulse(automationThread);
                    Monitor.Wait(automationThread);
                } 
                while(!worker.Complete && !threadDying);
                return threadDying ? default(T) : worker.GetReturn();
            }
        }
    }
}
