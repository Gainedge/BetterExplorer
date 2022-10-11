using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ShellLibrary.Interop;

namespace BExplorer.Shell.Interop {
	[ComImport]
	[Guid("947aab5f-0a5c-4c13-b4d6-4bf7836fc9f8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IFileOperation {
		HResult Advise(IFileOperationProgressSink pfops, out uint pdwCookie);
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

		/// <summary>
		/// Declares a set of items that are to be deleted.
		/// </summary>
		/// <param name="punkItems">Pointer to the IUnknown of the IShellItemArray, IDataObject, or IEnumShellItems object which represents the group of items to be deleted. You can also point to an IPersistIDList object to represent a single item, effectively accomplishing the same function as IFileOperation::DeleteItem.</param>
		void DeleteItems([MarshalAs(UnmanagedType.IUnknown)] object punkItems);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psiDestinationFolder">Pointer to an IShellItem that specifies the destination folder that will contain the new item.</param>
		/// <param name="dwFileAttributes">A bitwise value that specifies the file system attributes for the file or folder. See GetFileAttributes for possible values.</param>
		/// <param name="pszName">Pointer to the file name of the new item, for instance Newfile.txt. This is a null-terminated, Unicode string.</param>
		/// <param name="pszTemplateName">Pointer to the name of the template file (for example Excel9.xls) that the new item is based on, stored in one of the following locations</param>
		/// <param name="pfopsItem">Pointer to an IFileOperationProgressSink object to be used for status and failure notifications. If you call IFileOperation::Advise for the overall operation, progress status and error notifications for the creation operation are included there, so set this parameter to NULL.</param>
		/// <returns></returns>
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
