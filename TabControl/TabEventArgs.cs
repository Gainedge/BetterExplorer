using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Wpf.Controls
{
    public class TabItemEventArgs : EventArgs
    {
        public TabItem TabItem { get; private set; }

        public TabItemEventArgs(TabItem item)
        {
            TabItem = item;
        }
    }
    public class NewTabItemEventArgs : EventArgs
    {
        /// <summary>
        ///     The object to be used as the Content for the new TabItem
        /// </summary>
        public object Content { get; set; }
    }

    public class TabItemCancelEventArgs : CancelEventArgs
    {
        public TabItem TabItem { get; private set; }

        public TabItemCancelEventArgs(TabItem item)
        {
            TabItem = item;
        }
    }
}
