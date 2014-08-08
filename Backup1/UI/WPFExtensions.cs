// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;

namespace Windows7.DesktopIntegration.WindowsForms
{
    /// <summary>
    /// Contains extension methods for easier Windows Forms interoperability.
    /// </summary>
	public static class WPFExtensions
    {
		private static IntPtr GetWindowHandle(Window window)
		{
			return (new WindowInteropHelper(window)).Handle;
		}

    	/// <summary>
        /// Sets this Window's application id.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="appId">The application id.</param>
		public static void SetAppId(this Window form, string appId)
        {
            Windows7Taskbar.SetWindowAppId(GetWindowHandle(form), appId);
        }
        /// <summary>
        /// Gets this Window's application id.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns>The form's application id.</returns>
		public static string GetAppId(this Window form)
        {
            return Windows7Taskbar.GetWindowAppId(GetWindowHandle(form));
        }
        /// <summary>
        /// Enables custom window preview on this Window, meaning that
        /// the DWM will send messages when a live thumbnail preview
        /// or a live peek will be necessary.
        /// </summary>
        /// <param name="form">The form.</param>
        public static void EnableCustomWindowPreview(this Window form)
        {
            Windows7Taskbar.EnableCustomWindowPreview(GetWindowHandle(form));
        }
        /// <summary>
        /// Disables custom window preview on this Window, meaning that
        /// the DWM will not send messages when a live thumbnail preview
        /// or a live peek will be necessary, but instead rely on its
        /// default rendering.
        /// </summary>
        /// <param name="form">The form.</param>
        public static void DisableCustomWindowPreview(this Window form)
        {
            Windows7Taskbar.DisableCustomWindowPreview(GetWindowHandle(form));
        }
        /// <summary>
        /// Sets this Window's iconic thumbnail to the specified bitmap.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="bitmap">The bitmap.</param>
        public static void SetIconicThumbnail(this Window form, Bitmap bitmap)
        {
            Windows7Taskbar.SetIconicThumbnail(GetWindowHandle(form), bitmap);
        }
        /// <summary>
        /// Sets this Window's peek (live preview) bitmap.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="displayFrame">Whether to display a standard window
        /// frame around the bitmap.</param>
        public static void SetPeekBitmap(this Window form, Bitmap bitmap, bool displayFrame)
        {
            Windows7Taskbar.SetPeekBitmap(GetWindowHandle(form), bitmap, displayFrame);
        }
        /// <summary>
        /// Draws the specified overlay icon over this Window's taskbar button.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="icon">The overlay icon.</param>
        /// <param name="description">The overlay icon's description.</param>
        public static void SetTaskbarOverlayIcon(this Window form, Icon icon, string description)
        {
            Windows7Taskbar.SetTaskbarOverlayIcon(GetWindowHandle(form), icon, description);
        }
       
        /// <summary>
        /// Sets the progress bar of this Window's taskbar button to the
        /// specified percentage.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="percent">The progress percentage.</param>
        public static void SetTaskbarProgress(this Window form, float percent)
        {
            Windows7Taskbar.SetProgressState(
                GetWindowHandle(form), Windows7Taskbar.ThumbnailProgressState.Normal);
            Windows7Taskbar.SetProgressValue(
                GetWindowHandle(form), (ulong)percent, 100);
        }
        /// <summary>
        /// Sets the progress bar of this Window's taskbar button to the
        /// specified state.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="state">The taskbar progress state.</param>
        public static void SetTaskbarProgressState(this Window form, Windows7Taskbar.ThumbnailProgressState state)
        {
            Windows7Taskbar.SetProgressState(GetWindowHandle(form), state);
        }
        /// <summary>
        /// Creates a background worker that dispatches progress notifications
        /// to the application's taskbar button.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns>An instance of <see cref="BackgroundWorker"/> that reports
        /// progress through the application's taskbar button.</returns>
        public static BackgroundWorker CreateProgressEnabledBackgroundWorker(
            this Window form)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (o, e) =>
                form.SetTaskbarProgress(e.ProgressPercentage);
            worker.RunWorkerCompleted += (o, e) =>
                form.SetTaskbarProgressState(Windows7Taskbar.ThumbnailProgressState.NoProgress);
            return worker;
        }
        /// <summary>
        /// Creates a taskbar thumbnail button manager for this Window.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns>An object of type <see cref="ThumbButtonManager"/>
        /// that can be used to add and manage thumbnail toolbar buttons.</returns>
        public static ThumbButtonManager CreateThumbButtonManager(
            this Window form)
        {
            return new ThumbButtonManager(GetWindowHandle(form));
        }
        /// <summary>
        /// Creates a jump list manager for this Window.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns>An object of type <see cref="JumpListManager"/>
        /// that can be used to manage the application's jump list.</returns>
        public static JumpListManager CreateJumpListManager(
            this Window form)
        {
            return new JumpListManager(GetWindowHandle(form));
        }

        /// <summary>
        /// Specifies that only a portion of the form's client area
        /// should be used in the form's thumbnail.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="clipRect">The rectangle that specifies the clipped region.</param>
        public static void SetThumbnailClip(
            this Window form, Rectangle clipRect)
        {
            Windows7Taskbar.SetThumbnailClip(GetWindowHandle(form), clipRect);
        }

        /// <summary>
        /// Sets the specified form's thumbnail tooltip.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="tooltip">The tooltip text.</param>
        public static void SetThumbnailTooltip(this Window form, string tooltip)
        {
            Windows7Taskbar.SetThumbnailTooltip(GetWindowHandle(form), tooltip);
        }
    }
}