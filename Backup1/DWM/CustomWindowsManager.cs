// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Drawing;
using System.Windows.Forms;
using Windows7.DesktopIntegration.Interop;
using VistaBridgeInterop = Microsoft.SDK.Samples.VistaBridge.Interop;

namespace Windows7.DesktopIntegration
{
    /// <summary>
    /// The event arguments object for the thumbnail request
    /// and peek preview request events published by the
    /// <see cref="CustomWindowsManager"/> class.
    /// </summary>
    public class BitmapRequestedEventArgs : EventArgs
    {
        private Bitmap _bitmap;
        private int _width, _height;

        internal BitmapRequestedEventArgs(int width, int height, bool defaultDisplayFrame)
        {
            _width = width;
            _height = height;
            DisplayFrameAroundBitmap = defaultDisplayFrame;
        }

        /// <summary>
        /// Gets the requested bitmap's width.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }
        /// <summary>
        /// Gets the requested bitmap's height.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Sets a flag indicating whether to use the window screenshot
        /// instead of specifying a bitmap.  Note that using this feature
        /// is risky because not always a window screenshot can easily
        /// be snapped.  For example, minimized windows do not ever draw
        /// and therefore a bitmap of them can't be taken.
        /// </summary>
        public bool UseWindowScreenshot
        {
            internal get;
            set;
        }

        /// <summary>
        /// Specifies whether the provided bitmap is already mirrored.
        /// DWM thumbnail and peek bitmaps must be mirrored vertically.
        /// If this flag is set to <b>false</b>, the <see cref="CustomWindowsManager"/>
        /// will mirror the bitmap automatically.
        /// </summary>
        public bool DoNotMirrorBitmap
        {
            internal get;
            set;
        }

        /// <summary>
        /// Specifies whether a standard window frame will be displayed
        /// around the bitmap.  If the bitmap represents a top-level window,
        /// you would probably set this flag to <b>true</b>.  If the bitmap
        /// represents a child window (or a frameless window), you would
        /// probably set this flag to <b>false</b>.
        /// </summary>
        public bool DisplayFrameAroundBitmap
        {
            internal get;
            set;
        }

        /// <summary>
        /// Sets the bitmap requested by the sender.
        /// If the bitmap doesn't have the right dimensions,
        /// the DWM may scale it or not render certain areas
        /// as appropriate - it is the user's responsibility
        /// to render a bitmap with the proper dimensions.
        /// </summary>
        public Bitmap Bitmap
        {
            internal get
            {
                return _bitmap;
            }
            set
            {
                _bitmap = value;
            }
        }
    }

    /// <summary>
    /// Manages custom window representations in taskbar thumbnails
    /// and DWM peek preview (Aero Peek).
    /// </summary>
    public sealed class CustomWindowsManager
    {
        /// <summary>
        /// Creates a new instance of this class from the specified
        /// window handle.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <returns>A new instance of this class wrapping the 
        /// specified window handle.</returns>
        public static CustomWindowsManager CreateWindowsManager(IntPtr hwnd)
        {
            return CreateWindowsManager(hwnd, IntPtr.Zero);
        }
        /// <summary>
        /// Creates a new instance of this class from the specified
        /// window handle and parent window handle.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="parentHwnd">The parent window handle.</param>
        /// <returns>A new instance of this class wrapping the
        /// specified window handle and parent window handle.</returns>
        public static CustomWindowsManager CreateWindowsManager(
            IntPtr hwnd, IntPtr parentHwnd)
        {
            if (parentHwnd == IntPtr.Zero)
                return new CustomWindowsManager(hwnd);

            ProxyWindow proxy = new ProxyWindow(hwnd);

            Windows7Taskbar.TaskbarList.UnregisterTab(parentHwnd);

            Windows7Taskbar.TaskbarList.RegisterTab(proxy.Handle, parentHwnd);
            Windows7Taskbar.TaskbarList.SetTabOrder(proxy.Handle, IntPtr.Zero);
            Windows7Taskbar.TaskbarList.ActivateTab(proxy.Handle);
            
            return new CustomWindowsManager(proxy, parentHwnd);
            
            //TODO: Think about ordering
        }

        private IntPtr _hwnd;
        private IntPtr _hwndParent;
        private ProxyWindow _proxyWindow;

        /// <summary>
        /// Creates a new instance of this class using the
        /// specified window handle and enables custom window
        /// preview on the window.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        internal CustomWindowsManager(IntPtr hwnd)
        {
            _hwnd = hwnd;
            Windows7Taskbar.EnableCustomWindowPreview(hwnd);
        }
        internal CustomWindowsManager(ProxyWindow proxy, IntPtr hwndParent)
        {
            _hwnd = proxy.RealWindow;
            _hwndParent = hwndParent;
            _proxyWindow = proxy;//Just keep it alive
            _proxyWindow.WindowsManager = this;

            Windows7Taskbar.EnableCustomWindowPreview(WindowToTellDwmAbout);
        }

        /// <summary>
        /// The event that occurs when a peek bitmap must be provided
        /// for the window.
        /// </summary>
        public event EventHandler<BitmapRequestedEventArgs> PeekRequested;
        /// <summary>
        /// The event that occurs when a thumbnail must be provided
        /// for the window.
        /// </summary>
        public event EventHandler<BitmapRequestedEventArgs> ThumbnailRequested;

        private IntPtr WindowToTellDwmAbout
        {
            get
            {
                if (_proxyWindow == null)
                    return _hwnd;
                else
                    return _proxyWindow.Handle;
            }
        }

        /// <summary>
        /// Invalidates the previews (thumbnail bitmap and peek bitmap)
        /// that the DWM has currently cached.  If this method is not called,
        /// the DWM is free to use old versions of the bitmaps.
        /// </summary>
        public void InvalidatePreviews()
        {
            UnsafeNativeMethods.DwmInvalidateIconicBitmaps(WindowToTellDwmAbout);
        }

        /// <summary>
        /// Disables window preview features for this window.
        /// </summary>
        public void DisablePreview()
        {
            Windows7Taskbar.DisableCustomWindowPreview(WindowToTellDwmAbout);
        }
        /// <summary>
        /// Enables window preview features for this window.
        /// Window preview feature are enabled by default, unless disabled
        /// by the use of the <see cref="DisablePreview"/> method.
        /// </summary>
        public void EnablePreview()
        {
            Windows7Taskbar.EnableCustomWindowPreview(WindowToTellDwmAbout);
        }

        /// <summary>
        /// Notifies the custom window manager that the underlying
        /// window has been closed.  This method must be called when
        /// the window is closed, or residual buttons might remain
        /// in the taskbar and window handle leaks might occur.
        /// </summary>
        public void WindowClosed()
        {
            //TODO: Note that when the child window is closed,
            //the parent doesn't automatically get its taskbar
            //button back.  Whether this is or is not desired
            //is arguable.

            Windows7Taskbar.TaskbarList.UnregisterTab(_hwnd);
            _proxyWindow.Close();
        }

        /// <summary>
        /// Dispatches a window message so that the appropriate events
        /// can be invoked.
        /// </summary>
        /// <param name="m">The window message, typically obtained
        /// from a Windows Forms or WPF window procedure.</param>
        public void DispatchMessage(ref Message m)
        {
            if (m.Msg == SafeNativeMethods.WM_ACTIVATE && _hwndParent != IntPtr.Zero)
            {
                if (((int)m.WParam) == SafeNativeMethods.WA_ACTIVE ||
                    ((int)m.WParam) == SafeNativeMethods.WA_CLICKACTIVE)
                {
                    VistaBridgeInterop.UnsafeNativeMethods.SendMessage(
                        _hwnd, (uint)m.Msg, m.WParam, m.LParam);

                    //TODO: Technically, we should also test if the child
                    //isn't visible.  If it is, no need to send the message.
                }
            }
            if (m.Msg == SafeNativeMethods.WM_SYSCOMMAND && _hwndParent != IntPtr.Zero)
            {
                if (((int)m.WParam) == SafeNativeMethods.SC_CLOSE)
                {
                    VistaBridgeInterop.UnsafeNativeMethods.SendMessage(
                        _hwnd, SafeNativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    WindowClosed();
                }
            }
            if (m.Msg == SafeNativeMethods.WM_DWMSENDICONICTHUMBNAIL)
            {
                int width = (int)(((long)m.LParam) >> 16);
                int height = (int)(((long)m.LParam) & (0xFFFF));

                BitmapRequestedEventArgs b = new BitmapRequestedEventArgs(width, height, true);
                ThumbnailRequested(this, b);

                if (b.UseWindowScreenshot)
                {
                    //The actual application window might scale pretty badly
                    //with these parameters.  Note that the following
                    //scaling is still not as good as what DWM does on
                    //its own, because by default it will take the window
                    //dimensions in consideration.  When using custom
                    //preview, DWM gives us default window dimensions
                    //instead of taking the window dimensions in consideration.

                    Size clientSize;
                    UnsafeNativeMethods.GetClientSize(_hwnd, out clientSize);

                    float thumbnailAspect = ((float)width) / height;
                    float windowAspect = ((float)clientSize.Width) / clientSize.Height;

                    if (windowAspect > thumbnailAspect)
                    {
                        //Wider than the thumbnail, make the thumbnail height smaller:
                        height = (int)(height * (thumbnailAspect / windowAspect));
                    }
                    if (windowAspect < thumbnailAspect)
                    {
                        //The thumbnail is wider, make the width smaller:
                        width = (int)(width * (windowAspect / thumbnailAspect));
                    }

                    b.Bitmap = ScreenCapture.GrabWindowBitmap(_hwnd, new Size(width, height));
                }
                else if (!b.DoNotMirrorBitmap)
                {
                    b.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                Windows7Taskbar.SetIconicThumbnail(WindowToTellDwmAbout, b.Bitmap);
                b.Bitmap.Dispose(); //TODO: Is it our responsibility?
            }
            else if (m.Msg == SafeNativeMethods.WM_DWMSENDICONICLIVEPREVIEWBITMAP)
            {
                Size clientSize;
                if (!UnsafeNativeMethods.GetClientSize(_hwnd, out clientSize))
                {
                    clientSize = new Size(50,50);//Best guess
                }

                BitmapRequestedEventArgs b = new BitmapRequestedEventArgs(
                    clientSize.Width, clientSize.Height, _hwndParent == IntPtr.Zero);
                PeekRequested(this, b);

                if (b.UseWindowScreenshot)
                {
                    b.Bitmap = ScreenCapture.GrabWindowBitmap(_hwnd, clientSize);
                }
                else if (!b.DoNotMirrorBitmap)
                {
                    b.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                if (_hwndParent != IntPtr.Zero)
                {
                    Point offset = WindowUtilities.GetParentOffsetOfChild(_hwnd, _hwndParent);
                    Windows7Taskbar.SetPeekBitmap(WindowToTellDwmAbout, b.Bitmap, offset, b.DisplayFrameAroundBitmap);
                }
                else
                {
                    Windows7Taskbar.SetPeekBitmap(WindowToTellDwmAbout, b.Bitmap, b.DisplayFrameAroundBitmap);
                }
                b.Bitmap.Dispose(); //TODO: Is it our responsibility?
            }
        }
    }

    /// <summary>
    /// Contains utility methods associated with screen capturing.
    /// </summary>
    public static class ScreenCapture
    {
        /// <summary>
        /// Captures a screenshot of the specified window at the specified
        /// bitmap size.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="bitmapSize">The requested bitmap size.</param>
        /// <returns>A screen capture of the window.</returns>
        public static Bitmap GrabWindowBitmap(IntPtr hwnd, Size bitmapSize)
        {
            IntPtr windowDC = IntPtr.Zero;
            try
            {
                Size realWindowSize;
                UnsafeNativeMethods.GetClientSize(hwnd, out realWindowSize);

                windowDC = UnsafeNativeMethods.GetWindowDC(hwnd);

                Bitmap targetBitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height);

                //NOTE: Technically, we could grab a standard bitmap
                //and then RotateFlip() it.  However, since we're doing
                //a StretchBlt anyway, it's probably faster to do here.
                using (Graphics targetGr = Graphics.FromImage(targetBitmap))
                {
                    IntPtr targetDC = targetGr.GetHdc();

                    uint operation = 0x00CC0020 /*SRCCOPY*/
                                   | 0x40000000 /*CAPTUREBLT*/;

                    Size ncArea = WindowUtilities.GetNonClientArea(hwnd);

                    bool success = UnsafeNativeMethods.StretchBlt(
                        targetDC, 0, 0, targetBitmap.Width, targetBitmap.Height,
                        windowDC, ncArea.Width, realWindowSize.Height + ncArea.Height - 1, realWindowSize.Width, -realWindowSize.Height,
                        operation);

                    targetGr.ReleaseHdc();

                    targetGr.DrawString("Windows 7 Bridge",
                        new Font(FontFamily.GenericMonospace, 12),
                        new SolidBrush(Color.Red),
                        new PointF(10, 10));

                    return targetBitmap;
                }
            }
            finally
            {
                if (windowDC != IntPtr.Zero)
                {
                    UnsafeNativeMethods.ReleaseDC(hwnd, windowDC);
                }
            }
        }
    }

    internal static class WindowUtilities
    {
        public static Point GetParentOffsetOfChild(IntPtr hwnd, IntPtr hwndParent)
        {
            var childScreenCoord = new VistaBridgeInterop.SafeNativeMethods.POINT(0, 0);
            UnsafeNativeMethods.ClientToScreen(
                hwnd, ref childScreenCoord);
            var parentScreenCoord = new VistaBridgeInterop.SafeNativeMethods.POINT(0, 0);
            UnsafeNativeMethods.ClientToScreen(
                hwndParent, ref parentScreenCoord);
            Point offset = new Point(
                childScreenCoord.X - parentScreenCoord.X,
                childScreenCoord.Y - parentScreenCoord.Y);
            return offset;
        }

        public static Size GetNonClientArea(IntPtr hwnd)
        {
            var c = new VistaBridgeInterop.SafeNativeMethods.POINT(0, 0);
            UnsafeNativeMethods.ClientToScreen(hwnd, ref c);
            var r = new RECT();
            UnsafeNativeMethods.GetWindowRect(hwnd, ref r);

            return new Size(c.X - r.left, c.Y - r.top);
        }
    }
}