using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("F676C15D-596A-4ce2-8234-33996F445DB1")]
	public interface IThumbnailCache {
		[PreserveSig]
		HResult GetThumbnail(
				[In] IShellItem pShellItem,
				[In] uint cxyRequestedThumbSize,
				[In] WTS_FLAGS flags /*default:  WTS_FLAGS.WTS_EXTRACT*/,
				[Out][MarshalAs(UnmanagedType.Interface)] out ISharedBitmap ppvThumb,
				[Out] WTS_CACHEFLAGS pOutFlags,
				[Out] WTS_THUMBNAILID pThumbnailID
		);
		[PreserveSig]
		HResult GetThumbnailByID(
				[In, MarshalAs(UnmanagedType.Struct)] WTS_THUMBNAILID thumbnailID,
				[In] uint cxyRequestedThumbSize,
				[Out][MarshalAs(UnmanagedType.Interface)] out ISharedBitmap ppvThumb,
				[Out] WTS_CACHEFLAGS pOutFlags
		);
	}
}
