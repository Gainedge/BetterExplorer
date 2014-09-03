// BExplorer.Shell - A Windows Shell library for .Net.
// Copyright (C) 2007-2009 Steven J. Kirk
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either 
// version 2 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free 
// Software Foundation, Inc., 51 Franklin Street, Fifth Floor,  
// Boston, MA 2110-1301, USA.
//
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using ComTypes = System.Runtime.InteropServices.ComTypes;

#pragma warning disable 1591

namespace BExplorer.Shell.Interop {
	public class Ole32 {
		[DllImport("ole32.dll")]
		public static extern void CoTaskMemFree(IntPtr pv);

		/*
		[DllImport("ole32.dll")]
		public static extern int DoDragDrop(ComTypes.IDataObject pDataObject,
			IDropSource pDropSource, DragDropEffects dwOKEffect,
			out DragDropEffects pdwEffect);

		[DllImport("ole32.dll")]
		public static extern int RegisterDragDrop(IntPtr hwnd, System.Windows.Forms.IDropTarget pDropTarget);

		[DllImport("ole32.dll")]
		public static extern int RevokeDragDrop(IntPtr hwnd);
		*/

		public static Guid IID_IDataObject {
			get { return new Guid("0000010e-0000-0000-C000-000000000046"); }
		}

		/*
		public static Guid IID_IDropTarget {
			get { return new Guid("00000122-0000-0000-C000-000000000046"); }
		}

		[DllImport("ole32.dll")]
		public static extern int CoMarshalInterThreadInterfaceInStream([In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] object pUnk, out IStream ppStm);

		[DllImport("ole32.dll")]
		public static extern int CoGetInterfaceAndReleaseStream(IStream pStm, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
		*/

		// Are used in activation calls to indicate the execution contexts in which an object is to be run
		[Flags]
		public enum CLSCTX : uint {
			INPROC_SERVER = 0x1,
			INPROC_HANDLER = 0x2,
			LOCAL_SERVER = 0x4,
			INPROC_SERVER16 = 0x8,
			REMOTE_SERVER = 0x10,
			INPROC_HANDLER16 = 0x20,
			RESERVED1 = 0x40,
			RESERVED2 = 0x80,
			RESERVED3 = 0x100,
			RESERVED4 = 0x200,
			NO_CODE_DOWNLOAD = 0x400,
			RESERVED5 = 0x800,
			NO_CUSTOM_MARSHAL = 0x1000,
			ENABLE_CODE_DOWNLOAD = 0x2000,
			NO_FAILURE_LOG = 0x4000,
			DISABLE_AAA = 0x8000,
			ENABLE_AAA = 0x10000,
			FROM_DEFAULT_CONTEXT = 0x20000,
			INPROC = INPROC_SERVER | INPROC_HANDLER,
			SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
			ALL = SERVER | INPROC_HANDLER
		}

		/*
		[StructLayout(LayoutKind.Sequential)]
		internal struct BIND_OPTS3 {
			internal uint cbStruct;
			internal uint grfFlags;
			internal uint grfMode;
			internal uint dwTickCountDeadline;
			internal uint dwTrackFlags;
			internal uint dwClassContext;
			internal uint locale;
			object pServerInfo; // will be passing null, so type doesn't matter
			internal IntPtr hwnd;
		}
		*/

		[DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern HResult CoCreateInstance(
				ref Guid rclsid,
				IntPtr pUnkOuter,
				CLSCTX dwClsContext,
				ref Guid riid,
				out IntPtr ppv);

		/*
		// Retrieves a data object that you can use to access the contents of the clipboard
		[DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int OleGetClipboard(out IntPtr ppDataObj);

		public static Guid CLSID_DragDropHelper = new Guid("{4657278A-411B-11d2-839A-00C04FD918D0}");
		public static Guid CLSID_NewMenu = new Guid("{D969A300-E7FF-11d0-A93B-00A0C90F2719}");
		public static Guid IID_IDragSourceHelper = new Guid("{DE5BF786-477A-11d2-839D-00C04FD918D0}");
		public static Guid IID_IDropTargetHelper = new Guid("{4657278B-411B-11d2-839A-00C04FD918D0}");		

		[DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Interface)]
		internal static extern object CoGetObject(string pszName, [In] ref BIND_OPTS3 pBindOptions, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid);

		[return: MarshalAs(UnmanagedType.Interface)]
		public static object LaunchElevatedCOMObject(Guid Clsid, Guid InterfaceID) {
			string CLSID = Clsid.ToString("B"); // B formatting directive: returns {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx} 
			string monikerName = "Elevation:Administrator!new:" + CLSID;

			BIND_OPTS3 bo = new BIND_OPTS3();
			bo.cbStruct = (uint)Marshal.SizeOf(bo);
			bo.hwnd = IntPtr.Zero;
			bo.dwClassContext = (int)CLSCTX.LOCAL_SERVER;

			object retVal = CoGetObject(monikerName, ref bo, InterfaceID);

			return (retVal);
		}
		*/
	}
}
