using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {
	public class TestDialog : IOperationsProgressDialog {
		#region IOperationsProgressDialog Members

		public void StartProgressDialog(IntPtr hwndOwner, OPPROGDLGF flags) {
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void StopProgressDialog() {
			//return HResult.S_OK;
		}

		public void SetOperation(SPACTION action) {
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void SetMode(PDMODE mode) {
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void UpdateProgress(ulong ullPointsCurrent, ulong ullPointsTotal, ulong ullSizeCurrent, ulong ullSizeTotal, ulong ullItemsCurrent, ulong ullItemsTotal) {
			ulong h = 0;
			ulong o = 0;
			//this.GetMilliseconds(ref h, ref o);
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void UpdateLocations(IShellItem psiSource, IShellItem psiTarget, IShellItem psiItem) {
			//throw new NotImplementedException();
			if (psiSource != null) {
				var source = new ShellItem(psiSource);
			}
			if (psiTarget != null) {
				var target = new ShellItem(psiTarget);
			}
			if (psiItem != null) {
				var item = new ShellItem(psiItem);
			}
			//return HResult.S_OK;
		}

		public void ResetTimer() {
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void PauseTimer() {
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void ResumeTimer() {
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void GetMilliseconds(ref ulong pullElapsed,ref ulong pullRemaining) {
			//pullRemaining = 0;
			//pullElapsed = 0;
			//long v = Marshal.ReadInt64((IntPtr)pullRemaining);
			//var timeRemaining = TimeSpan.FromMilliseconds(pullRemaining);
			//var stringRemaining = String.Format("{0:c}", timeRemaining);
			//var timeElapsed = TimeSpan.FromMilliseconds(pullElapsed);
			//var stringElapsed = String.Format("{0:c}", timeElapsed);
			////throw new NotImplementedException();
			//return HResult.S_OK;
		}

		public void GetOperationStatus(ref PDOPSTATUS popstatus) 
		{
			//popstatus = PDOPSTATUS.PDOPS_RUNNING;
			//popstatus = popstatus;
			//popstatus = PDOPSTATUS.PDOPS_PAUSED;
			//throw new NotImplementedException();
			//return HResult.S_OK;
		}

		#endregion

		#region IOperationsProgressDialog Members

		public HResult QueryInterface() {
			throw new NotImplementedException();
		}

		public HResult Addref() {
			throw new NotImplementedException();
		}

		public HResult Release() {
			throw new NotImplementedException();
		}

		#endregion

		#region IOperationsProgressDialog Members


		public void Test(object one, object too, object tree) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
