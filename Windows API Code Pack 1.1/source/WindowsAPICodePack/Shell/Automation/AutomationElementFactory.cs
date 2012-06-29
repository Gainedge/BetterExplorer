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
using System.Drawing;
using System.Runtime.InteropServices;
using QTTabBarLib.Interop;
using MS.WindowsAPICodePack.Internal;

namespace QTTabBarLib.Automation {
    // This class is for use in AutomationManager.  Never instantiate it
    // elsewhere.

    public class AutomationElementFactory : IDisposable {
        private List<AutomationElement> disposeList;
        private IUIAutomation pAutomation;

        internal AutomationElementFactory(IUIAutomation pAutomation) {
            disposeList = new List<AutomationElement>();
            this.pAutomation = pAutomation;
        }

        internal void AddToDisposeList(AutomationElement elem) {
            disposeList.Add(elem);
        }

        public IUIAutomationTreeWalker CreateTreeWalker() {
            IUIAutomationTreeWalker walker;
            pAutomation.get_ControlViewWalker(out walker);
            return walker;
        }

        public void Dispose() {
            foreach(AutomationElement elem in disposeList) {
                elem.Dispose();
            }
            disposeList.Clear();
        }

        public AutomationElement FromHandle(IntPtr hwnd) {
            try {
                IUIAutomationElement pElement;
                pAutomation.ElementFromHandle(hwnd, out pElement);
                return pElement == null ? null : new AutomationElement(pElement, this);
            }
            catch(COMException) {
                return null;
            }
        }

        public AutomationElement FromPoint(Point pt) {
            try {
                IUIAutomationElement pElement;
                HResult rez = pAutomation.ElementFromPoint(pt, out pElement);
                return pElement == null ? null : new AutomationElement(pElement, this);
            }
            catch(COMException) {
                return null;
            }
        }

        public AutomationElement FromKeyboardFocus() {
            try {
                IUIAutomationElement pElement;
                pAutomation.GetFocusedElement(out pElement);
                return pElement == null ? null : new AutomationElement(pElement, this);
            }
            catch(COMException) {
                return null;
            }
        }
    }
}
