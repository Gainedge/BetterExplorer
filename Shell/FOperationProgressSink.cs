using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using BExplorer.Shell._Plugin_Interfaces;

namespace BExplorer.Shell {
	public class FOperationProgressSink : FileOperationProgressSink {

		private ShellView _View { get; set; }
		private IListItemEx CurrentFolder { get; set; }

		public FOperationProgressSink(ShellView view) {
			this._View = view;
			this.CurrentFolder = view.CurrentFolder;
		}
		public FOperationProgressSink() {
			//For the situation when no need to have ShellVioew included
		}

		public override void UpdateProgress(uint iWorkTotal, uint iWorkSoFar) {
			//base.UpdateProgress(iWorkTotal, iWorkSoFar);
			if (iWorkSoFar == iWorkTotal) {
		  	//this._View.UnvalidateDirectory();
			}
		}
		public override void PreDeleteItem(uint dwFlags, IShellItem psiItem) { }

		[HandleProcessCorruptedStateExceptions]
		public override void PostDeleteItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, uint hrDelete, IShellItem psiNewlyCreated) {
			if (hrDelete == 2555912) {
				var theNewItem = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiItem);
			  if (this._View.CurrentFolder.Equals(theNewItem.Parent)) {
			    Shell32.SHChangeNotify(
			      theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_RMDIR : Shell32.HChangeNotifyEventID.SHCNE_DELETE,
			      Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.PIDL, IntPtr.Zero);
			  }
			  theNewItem.Dispose();
			}
		  //if (psiItem != null) {
		  //  Marshal.FinalReleaseComObject(psiItem);
		  //}

    //  if (psiNewlyCreated != null) {
    //    Marshal.FinalReleaseComObject(psiNewlyCreated);
    //  }
      //Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

		public override void PreCopyItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName) {
			this._View.IsSupressedTumbGeneration = true;
		}

		[HandleProcessCorruptedStateExceptions]
		public override void PostCopyItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName, uint hrCopy, IShellItem psiNewlyCreated) {
			if (hrCopy == 0) {
        var destination = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiDestinationFolder);
			  if (destination.Equals(this._View.CurrentFolder)) {
			    var theNewItem = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiNewlyCreated);
			    Shell32.SHChangeNotify(
			      theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_MKDIR : Shell32.HChangeNotifyEventID.SHCNE_CREATE,
			      Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.PIDL, IntPtr.Zero);
			    this._View.IsSupressedTumbGeneration = false;
			    Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEITEM,
			      Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.PIDL, IntPtr.Zero);
			    theNewItem.Dispose();
			  }
        destination.Dispose();
			}
      //if (psiItem != null) {
      //  Marshal.FinalReleaseComObject(psiItem);
      //}

      //if (psiNewlyCreated != null) {
      //  Marshal.FinalReleaseComObject(psiNewlyCreated);
      //}
      //Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

		[HandleProcessCorruptedStateExceptions]
		public override void PostMoveItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName, uint hrMove, IShellItem psiNewlyCreated) {
			if (hrMove == 0) {
        var destination = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiDestinationFolder);
			  if (destination.Equals(this._View.CurrentFolder)) {
			    var theOldItem = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiItem);
			    var theNewItem = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiNewlyCreated);
			    Shell32.SHChangeNotify(
			      theOldItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_RMDIR : Shell32.HChangeNotifyEventID.SHCNE_DELETE,
			      Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theOldItem.PIDL, IntPtr.Zero);
			    this._View.IsSupressedTumbGeneration = false;
			    Shell32.SHChangeNotify(
			      theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_MKDIR : Shell32.HChangeNotifyEventID.SHCNE_CREATE,
			      Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.PIDL, IntPtr.Zero);
			    theOldItem.Dispose();
			    theNewItem.Dispose();
			  }
			}
      //Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }

		public override void PreMoveItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName) {
			this._View.IsSupressedTumbGeneration = true;
		}

	  public override void PreRenameItem(uint dwFlags, IShellItem psiItem, string pszNewName) {
	    this._View.IsRenameInProgress = true;
	  }

    public override void PostRenameItem(uint dwFlags, IShellItem psiItem, string pszNewName, uint hrRename, IShellItem psiNewlyCreated) {
	    if (hrRename == 2555912 && psiItem != null && psiNewlyCreated != null) {
	      var oldItem = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiItem);
	      var newItem = FileSystemListItem.InitializeWithIShellItem(this._View.LVHandle, psiNewlyCreated);
	      this._View.UpdateItem(oldItem, newItem);
	    }
      this._View.IsRenameInProgress = false;
    }

  }
}
