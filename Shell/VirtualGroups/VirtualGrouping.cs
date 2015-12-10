using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace BExplorer.Shell {

	[Guid("6FC61F50-80E8-49b4-B200-3F38D3865ABD")]
	public class VirtualGrouping : IOwnerDataCallback {

		private ShellView _VirtualListView { get; set; }

		public VirtualGrouping(ShellView view) { this._VirtualListView = view; }

		public void GetItemPosition(int i, out Interop.POINT pt) { throw new NotImplementedException(); }

		public void SetItemPosition(int t, Interop.POINT pt) { throw new NotImplementedException(); }

		public void GetItemGroupCount(int itemIndex, out int occurrenceCount) => occurrenceCount = 1;

		public void OnCacheHint(Interop.LVITEMINDEX i, Interop.LVITEMINDEX j) { }

		public void GetItemInGroup(int groupIndex, int n, out int itemIndex) {
			if (this._VirtualListView.Groups == null || this._VirtualListView.Groups.Count == 0) {
				itemIndex = -1;
				return;
			}

			var group = this._VirtualListView.Groups.SingleOrDefault(w => w.Index == groupIndex);
			if (group == null) {
				itemIndex = -1;
				return;
			}
			var itemInGroup = group.Items[n];
			itemIndex = itemInGroup.ItemIndex;
		}


		public void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex) {
			groupIndex = -1;


			var itemControlWide = this._VirtualListView.Items[itemIndex];

			//if (this._VirtualListView.IsGroupsEnabled && itemControlWide.GroupIndex == -1)
			//{
			//	foreach (var group in this._VirtualListView.Groups) {
			//		if (group.Items.Any(c => c.Equals(itemControlWide))) {
			//			groupIndex = group.Index;
			//			break;
			//		}
			//	}
			//}
			//else
			//{
				groupIndex = itemControlWide.GroupIndex;
			//}
		}
	}
}