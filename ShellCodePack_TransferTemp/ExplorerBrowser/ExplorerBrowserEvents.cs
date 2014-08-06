//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.WindowsAPICodePack.Shell;
using System.Drawing;

namespace Microsoft.WindowsAPICodePack.Controls
{
   
    /// <summary>
    /// Event argument for The NavigationPending event
    /// </summary>
    public class NavigationPendingEventArgs : EventArgs
    {
        /// <summary>
        /// The location being navigated to
        /// </summary>
        public ShellObject PendingLocation { get; set; }

        /// <summary>
        /// Set to 'True' to cancel the navigation.
        /// </summary>
        public bool Cancel { get; set; }

    }

    public class ExplorerKeyUPEventArgs : EventArgs
    {
        public int Key { get; set; }
    }

    public class ExplorerMoiseWheelArgs : EventArgs
    {
      public Int64 Delta { get; set; }
      public bool IsCTRL { get; set; }
      public Point MouseLocation { get; set; }
      public bool IsOutsideExplorer { get; set; }
      public bool Handled { get; set; }
    }

    /// <summary>
    /// Event argument for The NavigationComplete event
    /// </summary>
    public class NavigationCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// The new location of the explorer browser
        /// </summary>
        public ShellObject NewLocation { get; set; }
    }

    /// <summary>
    /// Event argument for the NavigatinoFailed event
    /// </summary>
    public class NavigationFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The location the the browser would have navigated to.
        /// </summary>
        public ShellObject FailedLocation { get; set; }
    }

    public class ViewChangedEventArgs : EventArgs
    {
        public int ThumbnailSize { get; set; }
        public ExplorerBrowserViewMode View { get; set; }
    }

    public class ExplorerAUItemEventArgs : EventArgs
    {
        
        public string ElementClass { get; set; }
        public System.Windows.Rect ElementRectangle { get; set; }
        public ShellObject Item { get; set; }
        public int Elementindex { get; set; }
        public bool IsElementBackground { get; set; }

    }

    public class ExplorerMouseEventArgs : EventArgs {
      public ShellObject Item { get; set; }
    }
}