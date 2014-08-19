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

		public FOperationProgressSink(ShellView view) {
			this._View = view;
		}
		public override void UpdateProgress(uint iWorkTotal, uint iWorkSoFar) {
			base.UpdateProgress(iWorkTotal, iWorkSoFar);
		}
		public override void PreDeleteItem(uint dwFlags, IShellItem psiItem) {
			//Thread.Sleep(100000);
			base.PreDeleteItem(dwFlags, psiItem);
		}
		public override void PostDeleteItem(uint dwFlags, Interop.IShellItem psiItem, uint hrDelete, Interop.IShellItem psiNewlyCreated) {
			//base.PostDeleteItem(dwFlags, psiItem, hrDelete, psiNewlyCreated);
			var deletedItem = new ShellItem(psiItem);
			//this._View.Items.Remove(this._View.Items.Where(w => w.GetHashCode() == deletedItem.GetHashCode()).SingleOrDefault());
			//var i = 0;
			//this._View.ItemsHashed = this._View.Items.Distinct().ToDictionary(k => k, el => i++);
			Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_DELETE, Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSHNOWAIT, deletedItem.Pidl, IntPtr.Zero);
			Shell32.ILFree(deletedItem.Pidl);
			deletedItem.Dispose();
		}
	}
}
