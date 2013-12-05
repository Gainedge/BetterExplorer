using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	/// <summary>
	/// Indicate flags that modify the property store object retrieved by methods 
	/// that create a property store, such as IShellItem2::GetPropertyStore or 
	/// IPropertyStoreFactory::GetPropertyStore.
	/// </summary>
	[Flags]
	public enum GetPropertyStoreOptions
	{
		/// <summary>
		/// Meaning to a calling process: Return a read-only property store that contains all 
		/// properties. Slow items (offline files) are not opened. 
		/// Combination with other flags: Can be overridden by other flags.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Meaning to a calling process: Include only properties directly from the property
		/// handler, which opens the file on the disk, network, or device. Meaning to a file 
		/// folder: Only include properties directly from the handler.
		/// 
		/// Meaning to other folders: When delegating to a file folder, pass this flag on 
		/// to the file folder; do not do any multiplexing (MUX). When not delegating to a 
		/// file folder, ignore this flag instead of returning a failure code.
		/// 
		/// Combination with other flags: Cannot be combined with GPS_TEMPORARY, 
		/// GPS_FASTPROPERTIESONLY, or GPS_BESTEFFORT.
		/// </summary>
		HandlePropertiesOnly = 0x1,

		/// <summary>
		/// Meaning to a calling process: Can write properties to the item. 
		/// Note: The store may contain fewer properties than a read-only store. 
		/// 
		/// Meaning to a file folder: ReadWrite.
		/// 
		/// Meaning to other folders: ReadWrite. Note: When using default MUX, 
		/// return a single unmultiplexed store because the default MUX does not support ReadWrite.
		/// 
		/// Combination with other flags: Cannot be combined with GPS_TEMPORARY, GPS_FASTPROPERTIESONLY, 
		/// GPS_BESTEFFORT, or GPS_DELAYCREATION. Implies GPS_HANDLERPROPERTIESONLY.
		/// </summary>
		ReadWrite = 0x2,

		/// <summary>
		/// Meaning to a calling process: Provides a writable store, with no initial properties, 
		/// that exists for the lifetime of the Shell item instance; basically, a property bag 
		/// attached to the item instance. 
		/// 
		/// Meaning to a file folder: Not applicable. Handled by the Shell item.
		/// 
		/// Meaning to other folders: Not applicable. Handled by the Shell item.
		/// 
		/// Combination with other flags: Cannot be combined with any other flag. Implies GPS_READWRITE
		/// </summary>
		Temporary = 0x4,

		/// <summary>
		/// Meaning to a calling process: Provides a store that does not involve reading from the 
		/// disk or network. Note: Some values may be different, or missing, compared to a store 
		/// without this flag. 
		/// 
		/// Meaning to a file folder: Include the "innate" and "fallback" stores only. Do not load the handler.
		/// 
		/// Meaning to other folders: Include only properties that are available in memory or can 
		/// be computed very quickly (no properties from disk, network, or peripheral IO devices). 
		/// This is normally only data sources from the IDLIST. When delegating to other folders, pass this flag on to them.
		/// 
		/// Combination with other flags: Cannot be combined with GPS_TEMPORARY, GPS_READWRITE, 
		/// GPS_HANDLERPROPERTIESONLY, or GPS_DELAYCREATION.
		/// </summary>
		FastPropertiesOnly = 0x8,

		/// <summary>
		/// Meaning to a calling process: Open a slow item (offline file) if necessary. 
		/// Meaning to a file folder: Retrieve a file from offline storage, if necessary. 
		/// Note: Without this flag, the handler is not created for offline files.
		/// 
		/// Meaning to other folders: Do not return any properties that are very slow.
		/// 
		/// Combination with other flags: Cannot be combined with GPS_TEMPORARY or GPS_FASTPROPERTIESONLY.
		/// </summary>
		OpensLowItem = 0x10,

		/// <summary>
		/// Meaning to a calling process: Delay memory-intensive operations, such as file access, until 
		/// a property is requested that requires such access. 
		/// 
		/// Meaning to a file folder: Do not create the handler until needed; for example, either 
		/// GetCount/GetAt or GetValue, where the innate store does not satisfy the request. 
		/// Note: GetValue might fail due to file access problems.
		/// 
		/// Meaning to other folders: If the folder has memory-intensive properties, such as 
		/// delegating to a file folder or network access, it can optimize performance by 
		/// supporting IDelayedPropertyStoreFactory and splitting up its properties into a 
		/// fast and a slow store. It can then use delayed MUX to recombine them.
		/// 
		/// Combination with other flags: Cannot be combined with GPS_TEMPORARY or 
		/// GPS_READWRITE
		/// </summary>
		DelayCreation = 0x20,

		/// <summary>
		/// Meaning to a calling process: Succeed at getting the store, even if some 
		/// properties are not returned. Note: Some values may be different, or missing,
		/// compared to a store without this flag. 
		/// 
		/// Meaning to a file folder: Succeed and return a store, even if the handler or 
		/// innate store has an error during creation. Only fail if substores fail.
		/// 
		/// Meaning to other folders: Succeed on getting the store, even if some properties 
		/// are not returned.
		/// 
		/// Combination with other flags: Cannot be combined with GPS_TEMPORARY, 
		/// GPS_READWRITE, or GPS_HANDLERPROPERTIESONLY.
		/// </summary>
		BestEffort = 0x40,

		/// <summary>
		/// Mask for valid GETPROPERTYSTOREFLAGS values.
		/// </summary>
		MaskValid = 0xff,
	}

	[ComImport,
			Guid(InterfaceGuids.IShellItem2),
			InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IShellItem2 : IShellItem
	{
		// Not supported: IBindCtx.
		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult BindToHandler(
				[In] IntPtr pbc,
				[In] ref Guid bhid,
				[In] ref Guid riid,
				[Out, MarshalAs(UnmanagedType.Interface)] out IShellFolder ppv);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetDisplayName(
				[In] SIGDN sigdnName,
				[MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetAttributes([In] SFGAO sfgaoMask, out SFGAO psfgaoAttribs);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Compare(
				[In, MarshalAs(UnmanagedType.Interface)] IShellItem psi,
				[In] uint hint,
				out int piOrder);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
		int GetPropertyStore(
				[In] GetPropertyStoreOptions Flags,
				[In] ref Guid riid,
				[Out, MarshalAs(UnmanagedType.Interface)] out IPropertyStore ppv);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetPropertyStoreWithCreateObject([In] GetPropertyStoreOptions Flags, [In, MarshalAs(UnmanagedType.IUnknown)] object punkCreateObject, [In] ref Guid riid, out IntPtr ppv);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetPropertyStoreForKeys([In] ref PROPERTYKEY rgKeys, [In] uint cKeys, [In] GetPropertyStoreOptions Flags, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.IUnknown)] out IPropertyStore ppv);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetPropertyDescriptionList([In] ref PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult Update([In, MarshalAs(UnmanagedType.Interface)] IBindCtx pbc);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetProperty([In] ref PROPERTYKEY key, [Out] PropVariant ppropvar);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetCLSID([In] ref PROPERTYKEY key, out Guid pclsid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetFileTime([In] ref PROPERTYKEY key, out System.Runtime.InteropServices.ComTypes.FILETIME pft);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetInt32([In] ref PROPERTYKEY key, out int pi);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetString([In] ref PROPERTYKEY key, [MarshalAs(UnmanagedType.LPWStr)] out string ppsz);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetUInt32([In] ref PROPERTYKEY key, out uint pui);

		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetUInt64([In] ref PROPERTYKEY key, out ulong pull);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void GetBool([In] ref PROPERTYKEY key, out int pf);
	}

	/// <summary>
	/// A property store
	/// </summary>
	[ComImport]
	[Guid(InterfaceGuids.IPropertyStore)]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPropertyStore
	{
		/// <summary>
		/// Gets the number of properties contained in the property store.
		/// </summary>
		/// <param name="propertyCount"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetCount([Out] out uint propertyCount);

		/// <summary>
		/// Get a property key located at a specific index.
		/// </summary>
		/// <param name="propertyIndex"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetAt([In] uint propertyIndex, out PROPERTYKEY key);

		/// <summary>
		/// Gets the value of a property from the store
		/// </summary>
		/// <param name="key"></param>
		/// <param name="pv"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult GetValue([In] ref PROPERTYKEY key, [Out] PropVariant pv);

		/// <summary>
		/// Sets the value of a property in the store
		/// </summary>
		/// <param name="key"></param>
		/// <param name="pv"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
		HResult SetValue([In] ref PROPERTYKEY key, [In] PropVariant pv);

		/// <summary>
		/// Commits the changes.
		/// </summary>
		/// <returns></returns>
		[PreserveSig]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		HResult Commit();
	}
}
