// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Drawing;
using Windows7.DesktopIntegration.Interop;

namespace Windows7.DesktopIntegration
{
    /// <summary>
    /// Represents a taskbar thumbnail button in the thumbnail toolbar.
    /// </summary>
    public sealed class ThumbButton
    {
        private ThumbButtonManager _manager;

        internal ThumbButton(ThumbButtonManager manager,
            uint id, Icon icon, string tooltip)
        {
            _manager = manager;

            Id = id;
            Icon = icon;
            Tooltip = tooltip;
        }

        /// <summary>
        /// The event that occurs when the taskbar thumbnail button
        /// is clicked.
        /// </summary>
        public event EventHandler Clicked;

        /// <summary>
        /// Gets or sets thumbnail button's id.
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// Gets or sets the thumbnail button's icon.
        /// </summary>
        public Icon Icon { get; set; }
        /// <summary>
        /// Gets or sets the thumbnail button's tooltip.
        /// </summary>
        public string Tooltip { get; set; }

        internal THBFLAGS Flags { get; set; }

        internal THUMBBUTTON Win32ThumbButton
        {
            get
            {
                THUMBBUTTON win32ThumbButton = new THUMBBUTTON();
                win32ThumbButton.iId = Id;
                win32ThumbButton.szTip = Tooltip;
                win32ThumbButton.hIcon = Icon.Handle;
                win32ThumbButton.dwFlags = Flags;

                win32ThumbButton.dwMask = THBMASK.THB_FLAGS;
                if (Tooltip != null)
                    win32ThumbButton.dwMask |= THBMASK.THB_TOOLTIP;
                if (Icon != null)
                    win32ThumbButton.dwMask |= THBMASK.THB_ICON;

                return win32ThumbButton;
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail button's visibility.
        /// </summary>
        public bool Visible
        {
            get
            {
                return (this.Flags & THBFLAGS.THBF_HIDDEN) == 0;
            }
            set
            {
                if (value)
                {
                    this.Flags &= ~(THBFLAGS.THBF_HIDDEN);
                }
                else
                {
                    this.Flags |= THBFLAGS.THBF_HIDDEN;
                }
                _manager.RefreshThumbButtons();
            }
        }
        /// <summary>
        /// Gets or sets the thumbnail button's enabled state.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return (this.Flags & THBFLAGS.THBF_DISABLED) == 0;
            }
            set
            {
                if (value)
                {
                    this.Flags &= ~(THBFLAGS.THBF_DISABLED);
                }
                else
                {
                    this.Flags |= THBFLAGS.THBF_DISABLED;
                }
                _manager.RefreshThumbButtons();
            }
        }

        internal void FireClick()
        {
            if (Clicked != null)
                Clicked(this, EventArgs.Empty);
        }
    }

}