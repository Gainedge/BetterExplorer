using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell
{
	/// <summary>
	/// This is the COM interface that a ListView must be given in order for groups in virtual lists to work.
	/// </summary>
	/// <remarks>
	/// This interface is NOT documented by MS. It was found on Greg Chapell's site. This means that there is
	/// no guarantee that it will work on future versions of Windows, nor continue to work on current ones.
	/// </remarks>
	[ComImport(),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
	 Guid("44C09D56-8D3B-419D-A462-7B956B105B47")]
	public interface IOwnerDataCallback
	{
		/// <summary>
		/// Not sure what this does
		/// </summary>
		/// <param name="i"></param>
		/// <param name="pt"></param>
		void GetItemPosition(int i, out POINT pt);

		/// <summary>
		/// Not sure what this does
		/// </summary>
		/// <param name="t"></param>
		/// <param name="pt"></param>
		void SetItemPosition(int t, POINT pt);

		/// <summary>
		/// Get the index of the item that occurs at the n'th position of the indicated group.
		/// </summary>
		/// <param name="groupIndex">Index of the group</param>
		/// <param name="n">Index within the group</param>
		/// <param name="itemIndex">Index of the item within the whole list</param>
		void GetItemInGroup(int groupIndex, int n, out int itemIndex);

		/// <summary>
		/// Get the index of the group to which the given item belongs
		/// </summary>
		/// <param name="itemIndex">Index of the item within the whole list</param>
		/// <param name="occurrenceCount">Which occurences of the item is wanted</param>
		/// <param name="groupIndex">Index of the group</param>
		void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex);

		/// <summary>
		/// Get the number of groups that contain the given item
		/// </summary>
		/// <param name="itemIndex">Index of the item within the whole list</param>
		/// <param name="occurrenceCount">How many groups does it occur within</param>
		void GetItemGroupCount(int itemIndex, out int occurrenceCount);

		/// <summary>
		/// A hint to prepare any cache for the given range of requests
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		void OnCacheHint(LVITEMINDEX i, LVITEMINDEX j);
	}
}
