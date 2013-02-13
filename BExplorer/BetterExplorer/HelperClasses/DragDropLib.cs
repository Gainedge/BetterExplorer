namespace BetterExplorer
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    #region DataObject

    /// <summary>
    /// Implements the COM version of IDataObject including SetData.
    /// </summary>
    /// <remarks>
    /// <para>Use this object when using shell (or other unmanged) features
    /// that utilize the clipboard and/or drag and drop.</para>
    /// <para>The System.Windows.DataObject (.NET 3.0) and
    /// System.Windows.Forms.DataObject do not support SetData from their COM
    /// IDataObject interface implementation.</para>
    /// <para>To use this object with .NET drag and drop, create an instance
    /// of System.Windows.DataObject (.NET 3.0) or System.Window.Forms.DataObject
    /// passing an instance of DataObject as the only constructor parameter. For
    /// example:</para>
    /// <code>
    /// System.Windows.DataObject data = new System.Windows.DataObject(new DragDropLib.DataObject());
    /// </code>
    /// </remarks>
    [ComVisible(true)]
    public class DataObject : IDataObject, IDisposable
    {
        #region Unmanaged functions

        // These are helper functions for managing STGMEDIUM structures

        [DllImport("urlmon.dll")]
        private static extern int CopyStgMedium(ref STGMEDIUM pcstgmedSrc, ref STGMEDIUM pstgmedDest);
        [DllImport("ole32.dll")]
        private static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        #endregion // Unmanaged functions

        // Our internal storage is a simple list
        private IList<KeyValuePair<FORMATETC, STGMEDIUM>> storage;

        /// <summary>
        /// Creates an empty instance of DataObject.
        /// </summary>
        public DataObject()
        {
            storage = new List<KeyValuePair<FORMATETC, STGMEDIUM>>();
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        ~DataObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clears the internal storage array.
        /// </summary>
        /// <remarks>
        /// ClearStorage is called by the IDisposable.Dispose method implementation
        /// to make sure all unmanaged references are released properly.
        /// </remarks>
        private void ClearStorage()
        {
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage)
            {
                STGMEDIUM medium = pair.Value;
                ReleaseStgMedium(ref medium);
            }
            storage.Clear();
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <param name="disposing">Indicates if the call was made by a managed caller, or the garbage collector.
        /// True indicates that someone called the Dispose method directly. False indicates that the garbage collector
        /// is finalizing the release of the object instance.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No managed objects to release
            }

            // Always release unmanaged objects
            ClearStorage();
        }

        #region COM IDataObject Members

        #region COM constants

        private const int OLE_E_ADVISENOTSUPPORTED = unchecked((int)0x80040003);

        private const int DV_E_FORMATETC = unchecked((int)0x80040064);
        private const int DV_E_TYMED = unchecked((int)0x80040069);
        private const int DV_E_CLIPFORMAT = unchecked((int)0x8004006A);
        private const int DV_E_DVASPECT = unchecked((int)0x8004006B);

        #endregion // COM constants

        #region Unsupported functions

        public int DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection)
        {
            throw Marshal.GetExceptionForHR(OLE_E_ADVISENOTSUPPORTED);
        }

        public void DUnadvise(int connection)
        {
            throw Marshal.GetExceptionForHR(OLE_E_ADVISENOTSUPPORTED);
        }

        public int EnumDAdvise(out IEnumSTATDATA enumAdvise)
        {
            throw Marshal.GetExceptionForHR(OLE_E_ADVISENOTSUPPORTED);
        }

        public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut)
        {
            formatOut = formatIn;
            return DV_E_FORMATETC;
        }

        public void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium)
        {
            throw new NotSupportedException();
        }

        #endregion // Unsupported functions

        /// <summary>
        /// Gets an enumerator for the formats contained in this DataObject.
        /// </summary>
        /// <param name="direction">The direction of the data.</param>
        /// <returns>An instance of the IEnumFORMATETC interface.</returns>
        public IEnumFORMATETC EnumFormatEtc(DATADIR direction)
        {
            // We only support GET
            if (DATADIR.DATADIR_GET == direction)
                return new EnumFORMATETC(storage);

            throw new NotImplementedException("OLE_S_USEREG");
        }

        /// <summary>
        /// Gets the specified data.
        /// </summary>
        /// <param name="format">The requested data format.</param>
        /// <param name="medium">When the function returns, contains the requested data.</param>
        public void GetData(ref FORMATETC format, out STGMEDIUM medium)
        {
            // Locate the data
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage)
            {
                if ((pair.Key.tymed & format.tymed) > 0
                    && pair.Key.dwAspect == format.dwAspect
                    && pair.Key.cfFormat == format.cfFormat)
                {
                    // Found it. Return a copy of the data.
                    STGMEDIUM source = pair.Value;
                    medium = CopyMedium(ref source);
                    return;
                }
            }

            // Didn't find it. Return an empty data medium.
            medium = new STGMEDIUM();
        }

        /// <summary>
        /// Determines if data of the requested format is present.
        /// </summary>
        /// <param name="format">The request data format.</param>
        /// <returns>Returns the status of the request. If the data is present, S_OK is returned.
        /// If the data is not present, an error code with the best guess as to the reason is returned.</returns>
        public int QueryGetData(ref FORMATETC format)
        {
            // We only support CONTENT aspect
            if ((DVASPECT.DVASPECT_CONTENT & format.dwAspect) == 0)
                return DV_E_DVASPECT;

            int ret = DV_E_TYMED;

            // Try to locate the data
            // TODO: The ret, if not S_OK, is only relevant to the last item
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage)
            {
                if ((pair.Key.tymed & format.tymed) > 0)
                {
                    if (pair.Key.cfFormat == format.cfFormat)
                    {
                        // Found it, return S_OK;
                        return 0;
                    }
                    else
                    {
                        // Found the medium type, but wrong format
                        ret = DV_E_CLIPFORMAT;
                    }
                }
                else
                {
                    // Mismatch on medium type
                    ret = DV_E_TYMED;
                }
            }

            return ret;
        }

        /// <summary>
        /// Sets data in the specified format into storage.
        /// </summary>
        /// <param name="formatIn">The format of the data.</param>
        /// <param name="medium">The data.</param>
        /// <param name="release">If true, ownership of the medium's memory will be transferred
        /// to this object. If false, a copy of the medium will be created and maintained, and
        /// the caller is responsible for the memory of the medium it provided.</param>
        public void SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release)
        {
            // If the format exists in our storage, remove it prior to resetting it
            foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage)
            {
                if ((pair.Key.tymed & formatIn.tymed) > 0
                    && pair.Key.dwAspect == formatIn.dwAspect
                    && pair.Key.cfFormat == formatIn.cfFormat)
                {
                    storage.Remove(pair);
                    break;
                }
            }

            // If release is true, we'll take ownership of the medium.
            // If not, we'll make a copy of it.
            STGMEDIUM sm = medium;
            if (!release)
                sm = CopyMedium(ref medium);

            // Add it to the internal storage
            KeyValuePair<FORMATETC, STGMEDIUM> addPair = new KeyValuePair<FORMATETC, STGMEDIUM>(formatIn, sm);
            storage.Add(addPair);
        }
  
        /// <summary>
        /// Creates a copy of the STGMEDIUM structure.
        /// </summary>
        /// <param name="medium">The data to copy.</param>
        /// <returns>The copied data.</returns>
        private STGMEDIUM CopyMedium(ref STGMEDIUM medium)
        {
            STGMEDIUM sm = new STGMEDIUM();
            int hr = CopyStgMedium(ref medium, ref sm);
            if (hr != 0)
                throw Marshal.GetExceptionForHR(hr);

            return sm;

        }

        #endregion

        /// <summary>
        /// Helps enumerate the formats available in our DataObject class.
        /// </summary>
        [ComVisible(true)]
        private class EnumFORMATETC : IEnumFORMATETC
        {
            // Keep an array of the formats for enumeration
            private FORMATETC[] formats;
            // The index of the next item
            private int currentIndex = 0;

            /// <summary>
            /// Creates an instance from a list of key value pairs.
            /// </summary>
            /// <param name="storage">List of FORMATETC/STGMEDIUM key value pairs</param>
            internal EnumFORMATETC(IList<KeyValuePair<FORMATETC, STGMEDIUM>> storage)
            {
                // Get the formats from the list
                formats = new FORMATETC[storage.Count];
                for (int i = 0; i < formats.Length; i++)
                    formats[i] = storage[i].Key;
            }

            /// <summary>
            /// Creates an instance from an array of FORMATETC's.
            /// </summary>
            /// <param name="formats">Array of formats to enumerate.</param>
            private EnumFORMATETC(FORMATETC[] formats)
            {
                // Get the formats as a copy of the array
                this.formats = new FORMATETC[formats.Length];
                formats.CopyTo(this.formats, 0);
            }

            #region IEnumFORMATETC Members

            /// <summary>
            /// Creates a clone of this enumerator.
            /// </summary>
            /// <param name="newEnum">When this function returns, contains a new instance of IEnumFORMATETC.</param>
            public void Clone(out IEnumFORMATETC newEnum)
            {
                EnumFORMATETC ret = new EnumFORMATETC(formats);
                ret.currentIndex = currentIndex;
                newEnum = ret;
            }

            /// <summary>
            /// Retrieves the next elements from the enumeration.
            /// </summary>
            /// <param name="celt">The number of elements to retrieve.</param>
            /// <param name="rgelt">An array to receive the formats requested.</param>
            /// <param name="pceltFetched">An array to receive the number of element fetched.</param>
            /// <returns>If the fetched number of formats is the same as the requested number, S_OK is returned.
            /// There are several reasons S_FALSE may be returned: (1) The requested number of elements is less than
            /// or equal to zero. (2) The rgelt parameter equals null. (3) There are no more elements to enumerate.
            /// (4) The requested number of elements is greater than one and pceltFetched equals null or does not
            /// have at least one element in it. (5) The number of fetched elements is less than the number of
            /// requested elements.</returns>
            public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched)
            {
                // Start with zero fetched, in case we return early
                if (pceltFetched != null && pceltFetched.Length > 0)
                    pceltFetched[0] = 0;

                // This will count down as we fetch elements
                int cReturn = celt;

                // Short circuit if they didn't request any elements, or didn't
                // provide room in the return array, or there are not more elements
                // to enumerate.
                if (celt <= 0 || rgelt == null || currentIndex >= formats.Length)
                    return 1; // S_FALSE

                // If the number of requested elements is not one, then we must
                // be able to tell the caller how many elements were fetched.
                if ((pceltFetched == null || pceltFetched.Length < 1) && celt != 1)
                    return 1; // S_FALSE

                // If the number of elements in the return array is too small, we
                // throw. This is not a likely scenario, hence the exception.
                if (rgelt.Length < celt)
                    throw new ArgumentException("The number of elements in the return array is less than the number of elements requested");

                // Fetch the elements.
                for (int i = 0; currentIndex < formats.Length && cReturn > 0; i++, cReturn--, currentIndex++)
                    rgelt[i] = formats[currentIndex];

                // Return the number of elements fetched
                if (pceltFetched != null && pceltFetched.Length > 0)
                    pceltFetched[0] = celt - cReturn;

                // cReturn has the number of elements requested but not fetched.
                // It will be greater than zero, if multiple elements were requested
                // but we hit the end of the enumeration.
                return (cReturn == 0) ? 0 : 1; // S_OK : S_FALSE
            }

            /// <summary>
            /// Resets the state of enumeration.
            /// </summary>
            /// <returns>S_OK</returns>
            public int Reset()
            {
                currentIndex = 0;
                return 0; // S_OK
            }

            /// <summary>
            /// Skips the number of elements requested.
            /// </summary>
            /// <param name="celt">The number of elements to skip.</param>
            /// <returns>If there are not enough remaining elements to skip, returns S_FALSE. Otherwise, S_OK is returned.</returns>
            public int Skip(int celt)
            {
                if (currentIndex + celt > formats.Length)
                    return 1; // S_FALSE

                currentIndex += celt;
                return 0; // S_OK
            }

            #endregion
        }
    }

    #endregion // DataObject

    #region Native structures

    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Size
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ShDragImage
    {
        public Win32Size sizeDragImage;
        public Win32Point ptOffset;
        public IntPtr hbmpDragImage;
        public int crColorKey;
    }

    #endregion // Native structures

    #region IDragSourceHelper

    [ComVisible(true)]
    [ComImport]
    [Guid("DE5BF786-477A-11D2-839D-00C04FD918D0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDragSourceHelper
    {
        void InitializeFromBitmap(
            [In, MarshalAs(UnmanagedType.Struct)] ref ShDragImage dragImage,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

        void InitializeFromWindow(
            [In] IntPtr hwnd,
            [In] ref Win32Point pt,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);
    }

    #endregion // IDragSourceHelper

    #region IDropTargetHelper

    [ComVisible(true)]
    [ComImport]
    [Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDropTargetHelper
    {
        void DragEnter(
            [In] IntPtr hwndTarget,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
            [In] ref Win32Point pt,
            [In] int effect);

        void DragLeave();

        void DragOver(
            [In] ref Win32Point pt,
            [In] int effect);

        void Drop(
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
            [In] ref Win32Point pt,
            [In] int effect);

        void Show(
            [In] bool show);
    }

    #endregion // IDropTargetHelper

    #region DragDropHelper

    [ComImport]
    [Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
    public class DragDropHelper { }

    #endregion // DragDropHelper
}

#region SWF extensions

#region IDataObject extensions

namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using DragDropLib;
    using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
    using Point = System.Drawing.Point;
    using BetterExplorer;

    public static class SwfDataObjectExtensions
    {
        #region DLL imports

        [DllImport("gdiplus.dll")]
        private static extern bool DeleteObject(IntPtr hgdi);

        #endregion // DLL imports

        /// <summary>
        /// Sets the drag image as the rendering of a control.
        /// </summary>
        /// <param name="dataObject">The DataObject to set the drag image on.</param>
        /// <param name="control">The Control to render as the drag image.</param>
        /// <param name="cursorOffset">The location of the cursor relative to the control.</param>
        public static void SetDragImage(this IDataObject dataObject, Control control, System.Drawing.Point cursorOffset)
        {
            int width = control.Width;
            int height = control.Height;

            Bitmap bmp = new Bitmap(width, height);
            control.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));

            SetDragImage(dataObject, bmp, cursorOffset);
        }

        /// <summary>
        /// Sets the drag image.
        /// </summary>
        /// <param name="dataObject">The DataObject to set the drag image on.</param>
        /// <param name="image">The drag image.</param>
        /// <param name="cursorOffset">The location of the cursor relative to the image.</param>
        public static void SetDragImage(this IDataObject dataObject, Image image, System.Drawing.Point cursorOffset)
        {
            ShDragImage shdi = new ShDragImage();

            Win32Size size;
            size.cx = image.Width;
            size.cy = image.Height;
            shdi.sizeDragImage = size;

            Win32Point wpt;
            wpt.x = cursorOffset.X;
            wpt.y = cursorOffset.Y;
            shdi.ptOffset = wpt;

            shdi.crColorKey = Color.Magenta.ToArgb();

            // This HBITMAP will be managed by the DragDropHelper
            // as soon as we pass it to InitializeFromBitmap. If we fail
            // to make the hand off, we'll delete it to prevent a mem leak.
            IntPtr hbmp = GetHbitmapFromImage(image);
            shdi.hbmpDragImage = hbmp;

            try
            {
                IDragSourceHelper sourceHelper = (IDragSourceHelper)new DragDropHelper();

                try
                {
                    sourceHelper.InitializeFromBitmap(ref shdi, (ComIDataObject)dataObject);
                }
                catch (NotImplementedException ex)
                {
                    throw new Exception("A NotImplementedException was caught. This could be because you forgot to construct your DataObject using a DragDropLib.DataObject", ex);
                }
            }
            catch
            {
                DeleteObject(hbmp);
            }
        }

        /// <summary>
        /// Gets an HBITMAP from any image.
        /// </summary>
        /// <param name="image">The image to get an HBITMAP from.</param>
        /// <returns>An HBITMAP pointer.</returns>
        /// <remarks>
        /// The caller is responsible to call DeleteObject on the HBITMAP.
        /// </remarks>
        private static IntPtr GetHbitmapFromImage(Image image)
        {
            if (image is Bitmap)
            {
                return ((Bitmap)image).GetHbitmap();
            }
            else
            {
                Bitmap bmp = new Bitmap(image);
                return bmp.GetHbitmap();
            }
        }
    }
}

#endregion // IDataObject extensions

#region DragDropLib extensions

namespace DragDropLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Drawing;
    using BetterExplorer;

    public static class SwfDragDropLibExtensions
    {
        /// <summary>
        /// Converts a System.Windows.Point value to a DragDropLib.Win32Point value.
        /// </summary>
        /// <param name="pt">Input value.</param>
        /// <returns>Converted value.</returns>
        public static Win32Point ToWin32Point(this Point pt)
        {
            Win32Point wpt = new Win32Point();
            wpt.x = pt.X;
            wpt.y = pt.Y;
            return wpt;
        }
    }
}

#endregion // DragDropLib extensions

#region IDropTargetHelper extensions

namespace DragDropLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing;
    using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
    using BetterExplorer;

    public static class SwfDropTargetHelperExtensions
    {
        /// <summary>
        /// Notifies the DragDropHelper that the specified Control received
        /// a DragEnter event.
        /// </summary>
        /// <param name="dropHelper">The DragDropHelper instance to notify.</param>
        /// <param name="control">The Control the received the DragEnter event.</param>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragEnter(this IDropTargetHelper dropHelper, Control control, IDataObject data, Point cursorOffset, DragDropEffects effect)
        {
            IntPtr controlHandle = IntPtr.Zero;
            if (control != null)
                controlHandle = control.Handle;
            Win32Point pt = SwfDragDropLibExtensions.ToWin32Point(cursorOffset);
            dropHelper.DragEnter(controlHandle, (ComIDataObject)data, ref pt, (int)effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Control received
        /// a DragOver event.
        /// </summary>
        /// <param name="dropHelper">The DragDropHelper instance to notify.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragOver(this IDropTargetHelper dropHelper, Point cursorOffset, DragDropEffects effect)
        {
            Win32Point pt = SwfDragDropLibExtensions.ToWin32Point(cursorOffset);
            dropHelper.DragOver(ref pt, (int)effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Control received
        /// a Drop event.
        /// </summary>
        /// <param name="dropHelper">The DragDropHelper instance to notify.</param>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void Drop(this IDropTargetHelper dropHelper, IDataObject data, Point cursorOffset, DragDropEffects effect)
        {
            Win32Point pt = SwfDragDropLibExtensions.ToWin32Point(cursorOffset);
            dropHelper.Drop((ComIDataObject)data, ref pt, (int)effect);
        }
    }
}

#endregion // IDropTargetHelper extensions

#region DropTargetHelper class

namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using DragDropLib;
    using BetterExplorer;

    public static class DropTargetHelper
    {
        /// <summary>
        /// Internal instance of the DragDropHelper.
        /// </summary>
        private static IDropTargetHelper s_instance = (IDropTargetHelper)new DragDropHelper();

        static DropTargetHelper()
        {
        }

        /// <summary>
        /// Notifies the DragDropHelper that the specified Control received
        /// a DragEnter event.
        /// </summary>
        /// <param name="control">The Control the received the DragEnter event.</param>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragEnter(Control control, IDataObject data, System.Drawing.Point cursorOffset, DragDropEffects effect)
        {
            SwfDropTargetHelperExtensions.DragEnter(s_instance, control, data, cursorOffset, effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Control received
        /// a DragOver event.
        /// </summary>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragOver(System.Drawing.Point cursorOffset, DragDropEffects effect)
        {
            SwfDropTargetHelperExtensions.DragOver(s_instance, cursorOffset, effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Control received
        /// a DragLeave event.
        /// </summary>
        public static void DragLeave()
        {
            s_instance.DragLeave();
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Control received
        /// a DragOver event.
        /// </summary>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void Drop(IDataObject data, System.Drawing.Point cursorOffset, DragDropEffects effect)
        {
            SwfDropTargetHelperExtensions.Drop(s_instance, data, cursorOffset, effect);
        }

        /// <summary>
        /// Tells the DragDropHelper to show or hide the drag image.
        /// </summary>
        /// <param name="show">True to show the image. False to hide it.</param>
        public static void Show(bool show)
        {
            s_instance.Show(show);
        }
    }
}

#endregion // DropTargetHelper class

#endregion // SWF extensions

#region WPF extensions

#region IDataObject extensions

namespace System.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using DragDropLib;
    using Color = System.Windows.Media.Color;
    using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
    using DrawingColor = System.Drawing.Color;
    using DrawingColorPalette = System.Drawing.Imaging.ColorPalette;
    using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;
    using PixelFormat = System.Windows.Media.PixelFormat;
    using DrawingRectangle = System.Drawing.Rectangle;
    using System.Drawing.Imaging;
    using System.Windows.Controls;
    using Bitmap = System.Drawing.Bitmap;
    using BetterExplorer;

    public static class WpfDataObjectExtensions
    {
        #region DLL imports

        [DllImport("gdiplus.dll")]
        private static extern bool DeleteObject(IntPtr hgdi);

        #endregion // DLL imports

        /// <summary>
        /// Sets the drag image by rendering the specified UIElement.
        /// </summary>
        /// <param name="dataObject">The DataObject to set the drag image for.</param>
        /// <param name="element">The element to render as the drag image.</param>
        /// <param name="cursorOffset">The offset of the cursor relative to the UIElement.</param>
        public static void SetDragImage(this IDataObject dataObject, UIElement element, Point cursorOffset)
        {
            Size size = element.RenderSize;

            // Get the device's DPI so we render at full size
            int dpix, dpiy;
            GetDeviceDpi(element, out dpix, out dpiy);

            // Create our renderer at full size
            RenderTargetBitmap renderSource = new RenderTargetBitmap(
                (int)size.Width, (int)size.Height, dpix, dpiy, PixelFormats.Pbgra32);

            // Render the element
            renderSource.Render(element);

            // Set the drag image by the bitmap source
            SetDragImage(dataObject, renderSource, cursorOffset);
        }

        /// <summary>
        /// Sets the drag image from a BitmapSource.
        /// </summary>
        /// <param name="dataObject">The DataObject on which to set the drag image.</param>
        /// <param name="image">The image source.</param>
        /// <param name="cursorOffset">The offset relative to the bitmap image.</param>
        public static void SetDragImage(this IDataObject dataObject, BitmapSource image, Point cursorOffset)
        {
            // Our internal routine requires an HBITMAP, so we'll convert the
            // BitmapSource to a System.Drawing.Bitmap.
            Bitmap bmp = GetBitmapFromBitmapSource(image, Colors.Magenta);
            
            // Sets the drag image from a Bitmap
            SetDragImage(dataObject, bmp, cursorOffset);
        }

        /// <summary>
        /// Sets the drag image.
        /// </summary>
        /// <param name="dataObject">The DataObject to set the drag image on.</param>
        /// <param name="image">The drag image.</param>
        /// <param name="cursorOffset">The location of the cursor relative to the image.</param>
        private static void SetDragImage(this IDataObject dataObject, Bitmap bitmap, Point cursorOffset)
        {
            ShDragImage shdi = new ShDragImage();

            Win32Size size;
            size.cx = bitmap.Width;
            size.cy = bitmap.Height;
            shdi.sizeDragImage = size;

            Win32Point wpt;
            wpt.x = (int)cursorOffset.X;
            wpt.y = (int)cursorOffset.Y;
            shdi.ptOffset = wpt;

            shdi.crColorKey = DrawingColor.Magenta.ToArgb();

            // This HBITMAP will be managed by the DragDropHelper
            // as soon as we pass it to InitializeFromBitmap. If we fail
            // to make the hand off, we'll delete it to prevent a mem leak.
            IntPtr hbmp = bitmap.GetHbitmap();
            shdi.hbmpDragImage = hbmp;

            try
            {
                IDragSourceHelper sourceHelper = (IDragSourceHelper)new DragDropHelper();

                try
                {
                    sourceHelper.InitializeFromBitmap(ref shdi, (ComIDataObject)dataObject);
                }
                catch (NotImplementedException ex)
                {
                    throw new Exception("A NotImplementedException was caught. This could be because you forgot to construct your DataObject using a DragDropLib.DataObject", ex);
                }
            }
            catch
            {
                // We failed to initialize the drag image, so the DragDropHelper
                // won't be managing our memory. Release the HBITMAP we allocated.
                DeleteObject(hbmp);
            }
        }

        #region Helper methods

        /// <summary>
        /// Gets the device capabilities.
        /// </summary>
        /// <param name="reference">A reference UIElement for getting the relevant device caps.</param>
        /// <param name="dpix">The horizontal DPI.</param>
        /// <param name="dpiy">The vertical DPI.</param>
        private static void GetDeviceDpi(Visual reference, out int dpix, out int dpiy)
        {
            Matrix m = PresentationSource.FromVisual(reference).CompositionTarget.TransformToDevice;
            dpix = (int)(96 * m.M11);
            dpiy = (int)(96 * m.M22);
        }

        /// <summary>
        /// Gets a System.Drawing.Bitmap from a BitmapSource.
        /// </summary>
        /// <param name="source">The source image from which to create our Bitmap.</param>
        /// <param name="transparencyKey">The transparency key. This is used by the DragDropHelper
        /// in rendering transparent pixels.</param>
        /// <returns>An instance of Bitmap which is a copy of the BitmapSource's image.</returns>
        private static Bitmap GetBitmapFromBitmapSource(BitmapSource source, Color transparencyKey)
        {
            // Copy at full size
            Int32Rect sourceRect = new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight);
            
            // Convert to our destination pixel format
            DrawingPixelFormat pxFormat = ConvertPixelFormat(source.Format);

            // Create the Bitmap, full size, full rez
            Bitmap bmp = new Bitmap(sourceRect.Width, sourceRect.Height, pxFormat);
            // If the format is an indexed format, copy the color palette
            if ((pxFormat & DrawingPixelFormat.Indexed) == DrawingPixelFormat.Indexed)
                ConvertColorPalette(bmp.Palette, source.Palette);

            // Get the transparency key as a System.Drawing.Color
            DrawingColor transKey = transparencyKey.ToDrawingColor();

            // Lock our Bitmap bits, we need to write to it
            BitmapData bmpData = bmp.LockBits(
                sourceRect.ToDrawingRectangle(),
                ImageLockMode.ReadWrite,
                pxFormat);
            {
                // Copy the source bitmap data to our new Bitmap
                source.CopyPixels(sourceRect, bmpData.Scan0, bmpData.Stride * sourceRect.Height, bmpData.Stride);

                // The drag image seems to work in full 32-bit color, except when
                // alpha equals zero. Then it renders those pixels at black. So
                // we make a pass and set all those pixels to the transparency key
                // color. This is only implemented for 32-bit pixel colors for now.
                if ((pxFormat & DrawingPixelFormat.Alpha) == DrawingPixelFormat.Alpha)
                    ReplaceTransparentPixelsWithTransparentKey(bmpData, transKey);
            }
            // Done, unlock the bits
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        /// <summary>
        /// Replaces any pixel with a zero alpha value with the specified transparency key.
        /// </summary>
        /// <param name="bmpData">The bitmap data in which to perform the operation.</param>
        /// <param name="transKey">The transparency color. This color is rendered transparent
        /// by the DragDropHelper.</param>
        /// <remarks>
        /// This function only supports 32-bit pixel formats for now.
        /// </remarks>
        private static void ReplaceTransparentPixelsWithTransparentKey(BitmapData bmpData, DrawingColor transKey)
        {
            DrawingPixelFormat pxFormat = bmpData.PixelFormat;

            if (DrawingPixelFormat.Format32bppArgb == pxFormat
                || DrawingPixelFormat.Format32bppPArgb == pxFormat)
            {
                int transKeyArgb = transKey.ToArgb();

                // We will just iterate over the data... we don't care about pixel location,
                // just that every pixel is checked.
                unsafe
                {
                    byte* pscan = (byte*)bmpData.Scan0.ToPointer();
                    {
                        for (int y = 0; y < bmpData.Height; ++y, pscan += bmpData.Stride)
                        {
                            int* prgb = (int*)pscan;
                            for (int x = 0; x < bmpData.Width; ++x, ++prgb)
                            {
                                // If the alpha value is zero, replace this pixel's color
                                // with the transparency key.
                                if ((*prgb & 0xFF000000L) == 0L)
                                    *prgb = transKeyArgb;
                            }
                        }
                    }
                }
            }
            else
            {
                // If it is anything else, we aren't supporting it, but we
                // won't throw, cause it isn't an error
                System.Diagnostics.Trace.TraceWarning("Not converting transparent colors to transparency key.");
                return;
            }
        }

        /// <summary>
        /// Converts a System.Windows.Media.Color to System.Drawing.Color.
        /// </summary>
        /// <param name="color">System.Windows.Media.Color value to convert.</param>
        /// <returns>System.Drawing.Color value.</returns>
        private static DrawingColor ToDrawingColor(this Color color)
        {
            return DrawingColor.FromArgb(
                color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Converts a System.Windows.Int32Rect to a System.Drawing.Rectangle value.
        /// </summary>
        /// <param name="rect">The System.Windows.Int32Rect to convert.</param>
        /// <returns>The System.Drawing.Rectangle converted value.</returns>
        private static DrawingRectangle ToDrawingRectangle(this Int32Rect rect)
        {
            return new DrawingRectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Converts the entries in a BitmapPalette to ColorPalette entries.
        /// </summary>
        /// <param name="destPalette">ColorPalette destination palette.</param>
        /// <param name="bitmapPalette">BitmapPalette source palette.</param>
        private static void ConvertColorPalette(DrawingColorPalette destPalette, BitmapPalette bitmapPalette)
        {
            DrawingColor[] destEntries = destPalette.Entries;
            IList<Color> sourceEntries = bitmapPalette.Colors;

            if (destEntries.Length < sourceEntries.Count)
                throw new ArgumentException("Destination palette has less entries than the source palette");

            for (int i = 0, count = sourceEntries.Count; i < count; ++i)
                destEntries[i] = sourceEntries[i].ToDrawingColor();
        }

        /// <summary>
        /// Converts a System.Windows.Media.PixelFormat instance to a
        /// System.Drawing.Imaging.PixelFormat value.
        /// </summary>
        /// <param name="pixelFormat">The input PixelFormat.</param>
        /// <returns>The converted value.</returns>
        private static DrawingPixelFormat ConvertPixelFormat(PixelFormat pixelFormat)
        {
            if (PixelFormats.Bgr24 == pixelFormat)
                return DrawingPixelFormat.Format24bppRgb;
            if (PixelFormats.Bgr32 == pixelFormat)
                return DrawingPixelFormat.Format32bppRgb;
            if (PixelFormats.Bgr555 == pixelFormat)
                return DrawingPixelFormat.Format16bppRgb555;
            if (PixelFormats.Bgr565 == pixelFormat)
                return DrawingPixelFormat.Format16bppRgb565;
            if (PixelFormats.Bgra32 == pixelFormat)
                return DrawingPixelFormat.Format32bppArgb;
            if (PixelFormats.BlackWhite == pixelFormat)
                return DrawingPixelFormat.Format1bppIndexed;
            if (PixelFormats.Gray16 == pixelFormat)
                return DrawingPixelFormat.Format16bppGrayScale;
            if (PixelFormats.Indexed1 == pixelFormat)
                return DrawingPixelFormat.Format1bppIndexed;
            if (PixelFormats.Indexed4 == pixelFormat)
                return DrawingPixelFormat.Format4bppIndexed;
            if (PixelFormats.Indexed8 == pixelFormat)
                return DrawingPixelFormat.Format8bppIndexed;
            if (PixelFormats.Pbgra32 == pixelFormat)
                return DrawingPixelFormat.Format32bppPArgb;
            if (PixelFormats.Prgba64 == pixelFormat)
                return DrawingPixelFormat.Format64bppPArgb;
            if (PixelFormats.Rgb24 == pixelFormat)
                return DrawingPixelFormat.Format24bppRgb;
            if (PixelFormats.Rgb48 == pixelFormat)
                return DrawingPixelFormat.Format48bppRgb;
            if (PixelFormats.Rgba64 == pixelFormat)
                return DrawingPixelFormat.Format64bppArgb;

            throw new NotSupportedException("The pixel format of the source bitmap is not supported.");
        }

        #endregion // Helper methods
    }
}

#endregion // IDataObject extensions

#region DragDropLib extensions

namespace DragDropLib
{
    using System;
    using System.Windows;
    using BetterExplorer;

    public static class WpfDragDropLibExtensions
    {
        /// <summary>
        /// Converts a System.Windows.Point value to a DragDropLib.Win32Point value.
        /// </summary>
        /// <param name="pt">Input value.</param>
        /// <returns>Converted value.</returns>
        public static Win32Point ToWin32Point(this Point pt)
        {
            Win32Point wpt = new Win32Point();
            wpt.x = (int)pt.X;
            wpt.y = (int)pt.Y;
            return wpt;
        }
    }
}

#endregion // DragDropLib extensions

#region IDropTargetHelper extensions

namespace DragDropLib
{
    using System;
    using System.Windows;
    using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
    using System.Windows.Interop;
    using BetterExplorer;

    public static class WpfDropTargetHelperExtensions
    {
        /// <summary>
        /// Notifies the DragDropHelper that the specified Window received
        /// a DragEnter event.
        /// </summary>
        /// <param name="dropHelper">The DragDropHelper instance to notify.</param>
        /// <param name="window">The Window the received the DragEnter event.</param>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragEnter(this IDropTargetHelper dropHelper, Window window, IDataObject data, Point cursorOffset, DragDropEffects effect)
        {
            IntPtr windowHandle = IntPtr.Zero;
            if (window != null)
                windowHandle = (new WindowInteropHelper(window)).Handle;
            Win32Point pt = WpfDragDropLibExtensions.ToWin32Point(cursorOffset);
            dropHelper.DragEnter(windowHandle, (ComIDataObject)data, ref pt, (int)effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Window received
        /// a DragOver event.
        /// </summary>
        /// <param name="dropHelper">The DragDropHelper instance to notify.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragOver(this IDropTargetHelper dropHelper, Point cursorOffset, DragDropEffects effect)
        {
            Win32Point pt = WpfDragDropLibExtensions.ToWin32Point(cursorOffset);
            dropHelper.DragOver(ref pt, (int)effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Window received
        /// a Drop event.
        /// </summary>
        /// <param name="dropHelper">The DragDropHelper instance to notify.</param>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void Drop(this IDropTargetHelper dropHelper, IDataObject data, Point cursorOffset, DragDropEffects effect)
        {
            Win32Point pt = WpfDragDropLibExtensions.ToWin32Point(cursorOffset);
            dropHelper.Drop((ComIDataObject)data, ref pt, (int)effect);
        }
    }
}

#endregion // IDropTargetHelper extensions

#region DropTargetHelper class

namespace System.Windows
{
    using System;
    using System.Windows;
    using DragDropLib;
    using BetterExplorer;

    public static class DropTargetHelper
    {
        /// <summary>
        /// Internal instance of the DragDropHelper.
        /// </summary>
        private static IDropTargetHelper s_instance = (IDropTargetHelper)new DragDropHelper();

        static DropTargetHelper()
        {
        }

        /// <summary>
        /// Notifies the DragDropHelper that the specified Window received
        /// a DragEnter event.
        /// </summary>
        /// <param name="window">The Window the received the DragEnter event.</param>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragEnter(Window window, IDataObject data, Point cursorOffset, DragDropEffects effect)
        {
            WpfDropTargetHelperExtensions.DragEnter(s_instance, window, data, cursorOffset, effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Window received
        /// a DragOver event.
        /// </summary>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void DragOver(Point cursorOffset, DragDropEffects effect)
        {
            WpfDropTargetHelperExtensions.DragOver(s_instance, cursorOffset, effect);
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Window received
        /// a DragLeave event.
        /// </summary>
        public static void DragLeave()
        {
            s_instance.DragLeave();
        }

        /// <summary>
        /// Notifies the DragDropHelper that the current Window received
        /// a DragOver event.
        /// </summary>
        /// <param name="data">The DataObject containing a drag image.</param>
        /// <param name="cursorOffset">The current cursor's offset relative to the window.</param>
        /// <param name="effect">The accepted drag drop effect.</param>
        public static void Drop(IDataObject data, Point cursorOffset, DragDropEffects effect)
        {
            WpfDropTargetHelperExtensions.Drop(s_instance, data, cursorOffset, effect);
        }

        /// <summary>
        /// Tells the DragDropHelper to show or hide the drag image.
        /// </summary>
        /// <param name="show">True to show the image. False to hide it.</param>
        public static void Show(bool show)
        {
            s_instance.Show(show);
        }
    }
}

#endregion // DropTargetHelper class

#endregion // WPF extensions
