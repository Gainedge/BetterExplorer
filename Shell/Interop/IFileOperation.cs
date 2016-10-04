using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {
	[ComImport]
	[Guid("947aab5f-0a5c-4c13-b4d6-4bf7836fc9f8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IFileOperation {
		uint Advise(IFileOperationProgressSink pfops);
		void Unadvise(uint dwCookie);

		void SetOperationFlags(FileOperationFlags dwOperationFlags);
		void SetProgressMessage([MarshalAs(UnmanagedType.LPWStr)] string pszMessage);
		void SetProgressDialog([MarshalAs(UnmanagedType.Interface)] IOperationsProgressDialog popd);
		void SetProperties([MarshalAs(UnmanagedType.Interface)] object pproparray);
		void SetOwnerWindow(uint hwndParent);

		void ApplyPropertiesToItem(IShellItem psiItem);
		void ApplyPropertiesToItems([MarshalAs(UnmanagedType.Interface)] object punkItems);

		void RenameItem(IShellItem psiItem, [MarshalAs(UnmanagedType.LPWStr)] string pszNewName,
				IFileOperationProgressSink pfopsItem);

		void RenameItems(
				[MarshalAs(UnmanagedType.Interface)] object pUnkItems,
				[MarshalAs(UnmanagedType.LPWStr)] string pszNewName);

		void MoveItem(
				IShellItem psiItem,
				IShellItem psiDestinationFolder,
				[MarshalAs(UnmanagedType.LPWStr)] string pszNewName,
				IFileOperationProgressSink pfopsItem);

		void MoveItems(
				[MarshalAs(UnmanagedType.IUnknown)] object punkItems,
				IShellItem psiDestinationFolder);

		void CopyItem(
				IShellItem psiItem,
				IShellItem psiDestinationFolder,
				[MarshalAs(UnmanagedType.LPWStr)] string pszCopyName,
				IFileOperationProgressSink pfopsItem);

		void CopyItems(
				[MarshalAs(UnmanagedType.IUnknown)] object punkItems,
				IShellItem psiDestinationFolder);

		void DeleteItem(
				IShellItem psiItem,
				IFileOperationProgressSink pfopsItem);

		void DeleteItems([MarshalAs(UnmanagedType.IUnknown)] object punkItems);

		uint NewItem(
				IShellItem psiDestinationFolder,
				FileAttributes dwFileAttributes,
				[MarshalAs(UnmanagedType.LPWStr)] string pszName,
				[MarshalAs(UnmanagedType.LPWStr)] string pszTemplateName,
				IFileOperationProgressSink pfopsItem);

		void PerformOperations();

		[return: MarshalAs(UnmanagedType.Bool)]
		bool GetAnyOperationsAborted();
	}
}
