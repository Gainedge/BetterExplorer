using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace BExplorer.Shell
{
	[Guid("6FC61F50-80E8-49b4-B200-3F38D3865ABD")]
	public class VirtualGrouping : IOwnerDataCallback
	{

		#region Private Members
		private ShellView _VirtualListView { get; set; }
		#endregion

		#region Initializer
		public VirtualGrouping()
		{
			
		}
		public VirtualGrouping(ShellView view)
		{
			this._VirtualListView = view;
		}
		#endregion

		#region IOwnerDataCallback Memebrs
		public void GetItemPosition(int i, out Interop.POINT pt)
		{
			throw new NotImplementedException();
		}

		public void SetItemPosition(int t, Interop.POINT pt)
		{
			throw new NotImplementedException();
		}

		public void GetItemInGroup(int groupIndex, int n, out int itemIndex)
		{
			var group = this._VirtualListView.Groups[groupIndex];
			var itemInGroup = group.Items[n];

			itemIndex = this._VirtualListView.ItemsHashed[itemInGroup];
		}
		public void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex)
		{
			var itemControlWide = this._VirtualListView.Items[itemIndex];
			foreach (var group in this._VirtualListView.Groups)
			{
				if (group.Items.Count(c => c.CachedParsingName == itemControlWide.CachedParsingName) > 0)
				{
					groupIndex = this._VirtualListView.Groups.IndexOf(group);
					break;
				}
			}
			groupIndex = -1;
		}

		public void GetItemGroupCount(int itemIndex, out int occurrenceCount)
		{
			occurrenceCount = 1;
		}

		public void OnCacheHint(Interop.LVITEMINDEX i, Interop.LVITEMINDEX j)
		{
			
		} 
		#endregion
	}
}
