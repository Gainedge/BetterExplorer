using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BExplorer.Shell {
	public class FOperationProgressSink : FileOperationProgressSink {
		private ShellView _View { get; set; }
		private ShellItem CurrentFolder { get; set; }

		public FOperationProgressSink(ShellView view) {
			this._View = view;
			this.CurrentFolder = view.CurrentFolder;
		}
		public override void UpdateProgress(uint iWorkTotal, uint iWorkSoFar) {
			base.UpdateProgress(iWorkTotal, iWorkSoFar);
		}
		public override void PreDeleteItem(uint dwFlags, IShellItem psiItem) {
			//Thread.Sleep(100000);
			base.PreDeleteItem(dwFlags, psiItem);
		}
		public override void PostDeleteItem(TRANSFER_SOURCE_FLAGS dwFlags, Interop.IShellItem psiItem, uint hrDelete, Interop.IShellItem psiNewlyCreated) {
			var obj = new ShellItem(psiItem);
			if (!String.IsNullOrEmpty(obj.ParsingName)) {
				if (obj.Parent != null && obj.Parent.Equals(this.CurrentFolder)) {
					ShellItem theItem = this._View.Items.SingleOrDefault(s => s.GetHashCode() == obj.GetHashCode());
					if (theItem != null) {
						this._View.Items.Remove(theItem);
						if (this._View.IsGroupsEnabled) {
							this._View.SetGroupOrder(false);
						}
						this._View.SetSortCollumn(this._View.LastSortedColumnIndex, this._View.LastSortOrder, false);
					}
				}
			}

		}
		public override void PreCopyItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName) {
			var item = new ShellItem(psiItem);
			//base.PreCopyItem(dwFlags, psiItem, psiDestinationFolder, pszNewName);
		}
		public override void PostCopyItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName, uint hrCopy, IShellItem psiNewlyCreated) {
			var theNewItem = new ShellItem(psiNewlyCreated);
			Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_MKDIR : Shell32.HChangeNotifyEventID.SHCNE_CREATE,
			Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);
			Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_UPDATEDIR : Shell32.HChangeNotifyEventID.SHCNE_UPDATEITEM,
					Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);

			//base.PostCopyItem(dwFlags, psiItem, psiDestinationFolder, pszNewName, hrCopy, psiNewlyCreated);
		}
		public override void PostMoveItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName, uint hrMove, IShellItem psiNewlyCreated) {
			var theNewItem = new ShellItem(psiNewlyCreated);
			Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_MKDIR : Shell32.HChangeNotifyEventID.SHCNE_CREATE,
			Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);
			Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_UPDATEDIR : Shell32.HChangeNotifyEventID.SHCNE_UPDATEITEM,
					Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);
			//base.PostMoveItem(dwFlags, psiItem, psiDestinationFolder, pszNewName, hrMove, psiNewlyCreated);
		}
	}
}
