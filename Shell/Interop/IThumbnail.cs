using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GongSolutions.Shell.Interop
{
	[ComImportAttribute()]
	[GuidAttribute("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	interface IShellItemImageFactory
	{
		[PreserveSig]
		HResult GetImage(
		[In, MarshalAs(UnmanagedType.Struct)] Size size,
		[In] SIIGBF flags,
		[Out] out IntPtr phbm);
	}

	[ComImport,
	Guid(InterfaceGuids.IThumbnailCache),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IThumbnailCache
	{
		void GetThumbnail([In] IShellItem pShellItem,
		[In] uint cxyRequestedThumbSize,
		[In] ThumbnailOptions flags,
		[Out] out ISharedBitmap ppvThumb,
		[Out] out ThumbnailCacheOptions pOutFlags,
		[Out] ThumbnailId pThumbnailID);

		void GetThumbnailByID([In] ThumbnailId thumbnailID,
		[In] uint cxyRequestedThumbSize,
		[Out] out ISharedBitmap ppvThumb,
		[Out] out ThumbnailCacheOptions pOutFlags);
	}

	[ComImport,
	Guid(InterfaceGuids.ISharedBitmap),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface ISharedBitmap
	{
		void GetSharedBitmap([Out] out IntPtr phbm);
		void GetSize([Out] out Size pSize);
		void GetFormat([Out] out ThumbnailAlphaType pat);
		void InitializeBitmap([In] IntPtr hbm, [In] ThumbnailAlphaType wtsAT);
		void Detach([Out] out IntPtr phbm);
	}
}
