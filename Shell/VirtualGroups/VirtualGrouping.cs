using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace BExplorer.Shell {

	[Guid("6FC61F50-80E8-49b4-B200-3F38D3865ABD")]
	public class VirtualGrouping : IOwnerDataCallback {

		private ShellView _VirtualListView { get; set; }

		public VirtualGrouping(ShellView view) {
			this._VirtualListView = view;
		}

		public void GetItemPosition(int i, out Interop.POINT pt) {
			throw new NotImplementedException();
		}

		public void SetItemPosition(int t, Interop.POINT pt) {
			throw new NotImplementedException();
		}

		public void GetItemGroupCount(int itemIndex, out int occurrenceCount) {
			occurrenceCount = 1;
		}

		public void OnCacheHint(Interop.LVITEMINDEX i, Interop.LVITEMINDEX j) {
		}

		public void GetItemInGroup(int groupIndex, int n, out int itemIndex) {
			if (this._VirtualListView.Groups == null || this._VirtualListView.Groups.Count == 0) {
				itemIndex = -1;
				return;
			}

			var group = this._VirtualListView.Groups[groupIndex];
			var itemInGroup = group.Items[n];

			if (!this._VirtualListView.ItemsHashed.TryGetValue(itemInGroup, out itemIndex)) {
				itemIndex = -1;
			}
		}

		public void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex) {
			var itemControlWide = this._VirtualListView.Items[itemIndex];
			foreach (var group in this._VirtualListView.Groups) {
				if (group.Items.Count(c => c.ParsingName == itemControlWide.ParsingName) > 0) {
					groupIndex = group.Index;
					break;
				}
			}
			groupIndex = -1;
		}
	}
}