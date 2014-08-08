// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Windows7.DesktopIntegration.Interop;

namespace Windows7.DesktopIntegration
{
    /// <summary>
    /// Manages a set of taskbar thumbnail buttons in an application.
    /// </summary>
    public sealed class ThumbButtonManager
    {
        /// <summary>
        /// Initializes a new manager with the specified window handle.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        public ThumbButtonManager(IntPtr hwnd)
        {
            _hwnd = hwnd;
        }

        //TODO: Add support for HIMAGELIST-based toolbars

        /// <summary>
        /// Creates a new taskbar thumbnail button.
        /// </summary>
        /// <param name="id">The button's id.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="tooltip">The button's tooltip.</param>
        /// <returns>An object of type <see cref="ThumbButton"/>
        /// representing the newly created thumbnail button.</returns>
        public ThumbButton CreateThumbButton(uint id, Icon icon, string tooltip)
        {
            return new ThumbButton(this, id, icon, tooltip);
        }

        /// <summary>
        /// Dispatches a Windows message to the thumbnail button event
        /// handlers if appropriate.
        /// </summary>
        /// <remarks>
        /// This method is intended for infrastructure use only.
        /// </remarks>
        /// <param name="message">The message.</param>
        public void DispatchMessage(ref Message message)
        {
            UInt64 wparam = (UInt64)message.WParam.ToInt64();
            UInt32 wparam32 = (UInt32) (wparam & 0xffffffff);   //Clear top 32 bits

            if (message.Msg == SafeNativeMethods.WM_COMMAND &&
                (wparam32 >> 16 == SafeNativeMethods.THBN_CLICKED))
            {
                uint id = wparam32 & 0xffff;    //Bottom 16 bits
                _thumbButtons[id].FireClick();
            }
        }
        /// <summary>
        /// Adds the specified taskbar thumbnail buttons to the application's
        /// thumbnail toolbar.
        /// </summary>
        /// <remarks>
        /// Thumbnail buttons can only be added once - after being added,
        /// they cannot be removed or deleted.  However, they can be shown,
        /// hidden, enabled and disabled.
        /// </remarks>
        /// <param name="buttons">The buttons to add.</param>
        public void AddThumbButtons(params ThumbButton[] buttons)
        {
            Array.ForEach(buttons, b => _thumbButtons.Add(b.Id, b));

            RefreshThumbButtons();
        }
        /// <summary>
        /// Gets a specific thumbnail button by its id.
        /// </summary>
        /// <param name="id">The thumbnail button's id.</param>
        /// <returns>An object of type <see cref="ThumbButton"/>
        /// with the specified id.</returns>
        public ThumbButton this[uint id]
        {
            get
            {
                return _thumbButtons[id];
            }
        }

        #region Implementation

        private bool _buttonsLoaded;
        internal void RefreshThumbButtons()
        {
            THUMBBUTTON[] win32Buttons =
                (from thumbButton in _thumbButtons.Values
                 select thumbButton.Win32ThumbButton).ToArray();
            if (_buttonsLoaded)
            {
                Windows7Taskbar.TaskbarList.ThumbBarUpdateButtons(
                    _hwnd, (uint)win32Buttons.Length, win32Buttons);
            }
            else //First time
            {
                Windows7Taskbar.TaskbarList.ThumbBarAddButtons(
                    _hwnd, (uint)win32Buttons.Length, win32Buttons);
                _buttonsLoaded = true;
            }
        }

        private Dictionary<uint, ThumbButton> _thumbButtons =
            new Dictionary<uint, ThumbButton>();
        private IntPtr _hwnd;

        #endregion
    }

}