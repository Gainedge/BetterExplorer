using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {

	#region DataObject - Removed

	/*
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
	[Obsolete("", true)]
	[ComVisible(true)]
	public class DataObject : IDataObject, IDisposable {
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
		public DataObject() {
			storage = new List<KeyValuePair<FORMATETC, STGMEDIUM>>();
		}

		/// <summary>
		/// Releases unmanaged resources.
		/// </summary>
		~DataObject() {
			Dispose(false);
		}

		/// <summary>
		/// Clears the internal storage array.
		/// </summary>
		/// <remarks>
		/// ClearStorage is called by the IDisposable.Dispose method implementation
		/// to make sure all unmanaged references are released properly.
		/// </remarks>
		private void ClearStorage() {
			foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage) {
				STGMEDIUM medium = pair.Value;
				ReleaseStgMedium(ref medium);
			}
			storage.Clear();
		}

		/// <summary>
		/// Releases resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
		}

		/// <summary>
		/// Releases resources.
		/// </summary>
		/// <param name="disposing">Indicates if the call was made by a managed caller, or the garbage collector.
		/// True indicates that someone called the Dispose method directly. False indicates that the garbage collector
		/// is finalizing the release of the object instance.</param>
		private void Dispose(bool disposing) {
			if (disposing) {
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

		public int DAdvise(ref FORMATETC pFormatetc, ADVF advf, IAdviseSink adviseSink, out int connection) {
			throw Marshal.GetExceptionForHR(OLE_E_ADVISENOTSUPPORTED);
		}

		public void DUnadvise(int connection) {
			throw Marshal.GetExceptionForHR(OLE_E_ADVISENOTSUPPORTED);
		}

		public int EnumDAdvise(out IEnumSTATDATA enumAdvise) {
			throw Marshal.GetExceptionForHR(OLE_E_ADVISENOTSUPPORTED);
		}

		public int GetCanonicalFormatEtc(ref FORMATETC formatIn, out FORMATETC formatOut) {
			formatOut = formatIn;
			return DV_E_FORMATETC;
		}

		public void GetDataHere(ref FORMATETC format, ref STGMEDIUM medium) {
			throw new NotSupportedException();
		}

		#endregion // Unsupported functions

		/// <summary>
		/// Gets an enumerator for the formats contained in this DataObject.
		/// </summary>
		/// <param name="direction">The direction of the data.</param>
		/// <returns>An instance of the IEnumFORMATETC interface.</returns>
		public IEnumFORMATETC EnumFormatEtc(DATADIR direction) {
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
		public void GetData(ref FORMATETC format, out STGMEDIUM medium) {
			// Locate the data
			foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage) {
				if ((pair.Key.tymed & format.tymed) > 0
						&& pair.Key.dwAspect == format.dwAspect
						&& pair.Key.cfFormat == format.cfFormat) {
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
		public int QueryGetData(ref FORMATETC format) {
			// We only support CONTENT aspect
			if ((DVASPECT.DVASPECT_CONTENT & format.dwAspect) == 0)
				return DV_E_DVASPECT;

			int ret = DV_E_TYMED;

			// Try to locate the data
			// TODO: The ret, if not S_OK, is only relevant to the last item
			foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage) {
				if ((pair.Key.tymed & format.tymed) > 0) {
					if (pair.Key.cfFormat == format.cfFormat) {
						// Found it, return S_OK;
						return 0;
					}
					else {
						// Found the medium type, but wrong format
						ret = DV_E_CLIPFORMAT;
					}
				}
				else {
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
		public void SetData(ref FORMATETC formatIn, ref STGMEDIUM medium, bool release) {
			// If the format exists in our storage, remove it prior to resetting it
			foreach (KeyValuePair<FORMATETC, STGMEDIUM> pair in storage) {
				if ((pair.Key.tymed & formatIn.tymed) > 0
						&& pair.Key.dwAspect == formatIn.dwAspect
						&& pair.Key.cfFormat == formatIn.cfFormat) {
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
		private STGMEDIUM CopyMedium(ref STGMEDIUM medium) {
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
		private class EnumFORMATETC : IEnumFORMATETC {
			// Keep an array of the formats for enumeration
			private FORMATETC[] formats;
			// The index of the next item
			private int currentIndex = 0;

			/// <summary>
			/// Creates an instance from a list of key value pairs.
			/// </summary>
			/// <param name="storage">List of FORMATETC/STGMEDIUM key value pairs</param>
			internal EnumFORMATETC(IList<KeyValuePair<FORMATETC, STGMEDIUM>> storage) {
				// Get the formats from the list
				formats = new FORMATETC[storage.Count];
				for (int i = 0; i < formats.Length; i++)
					formats[i] = storage[i].Key;
			}

			/// <summary>
			/// Creates an instance from an array of FORMATETC's.
			/// </summary>
			/// <param name="formats">Array of formats to enumerate.</param>
			private EnumFORMATETC(FORMATETC[] formats) {
				// Get the formats as a copy of the array
				this.formats = new FORMATETC[formats.Length];
				formats.CopyTo(this.formats, 0);
			}

			#region IEnumFORMATETC Members

			/// <summary>
			/// Creates a clone of this enumerator.
			/// </summary>
			/// <param name="newEnum">When this function returns, contains a new instance of IEnumFORMATETC.</param>
			public void Clone(out IEnumFORMATETC newEnum) {
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
			public int Next(int celt, FORMATETC[] rgelt, int[] pceltFetched) {
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
			public int Reset() {
				currentIndex = 0;
				return 0; // S_OK
			}

			/// <summary>
			/// Skips the number of elements requested.
			/// </summary>
			/// <param name="celt">The number of elements to skip.</param>
			/// <returns>If there are not enough remaining elements to skip, returns S_FALSE. Otherwise, S_OK is returned.</returns>
			public int Skip(int celt) {
				if (currentIndex + celt > formats.Length)
					return 1; // S_FALSE

				currentIndex += celt;
				return 0; // S_OK
			}

			#endregion
		}
	}
	*/

	#endregion // DataObject - Removed

	#region Native structures

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Point {
		//Made X and Y Uppercase
		public int X, Y;
	}

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Size {
		public int cx, cy;
	}
	*/

	/*
	[StructLayout(LayoutKind.Sequential)]
	public struct ShDragImage {
		public Win32Size sizeDragImage;
		public Win32Point ptOffset;
		public IntPtr hbmpDragImage;
		public int crColorKey;
	}
	*/

	#endregion // Native structures

	#region IDragSourceHelper

	/*
	[ComVisible(true)]
	[ComImport]
	[Guid("DE5BF786-477A-11D2-839D-00C04FD918D0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDragSourceHelper {
		void InitializeFromBitmap(
				[In, MarshalAs(UnmanagedType.Struct)] ref ShDragImage dragImage,
				[In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

		void InitializeFromWindow(
				[In] IntPtr hwnd,
				[In] ref Win32Point pt,
				[In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);
	}
*/

	#endregion // IDragSourceHelper


	namespace DropTargetHelper {
		[ComVisible(true)]
		[ComImport]
		[Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IDropTargetHelper {
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


		[ComImport]
		[Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
		public class DragDropHelper { }

		public static class Get {
			public static IDropTargetHelper Create {
				get {
					return (IDropTargetHelper)new DragDropHelper();
				}
			}
		}
	}
}
