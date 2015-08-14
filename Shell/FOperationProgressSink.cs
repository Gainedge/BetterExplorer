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

namespace BExplorer.Shell
{
    public class FOperationProgressSink : FileOperationProgressSink
    {
        private ShellView _View { get; set; }
        private IListItemEx CurrentFolder { get; set; }

        public FOperationProgressSink(ShellView view)
        {
            this._View = view;
            this.CurrentFolder = view.CurrentFolder;
        }
        public FOperationProgressSink()
        {
            //For the situation when no need to have ShellVioew included
        }

        public override void UpdateProgress(uint iWorkTotal, uint iWorkSoFar)
        {
            //base.UpdateProgress(iWorkTotal, iWorkSoFar);
            if (iWorkSoFar == iWorkTotal || iWorkSoFar > iWorkTotal * 0.97)
            {
                this._View.UnvalidateDirectory();
            }
        }
        public override void PreDeleteItem(uint dwFlags, IShellItem psiItem) => base.PreDeleteItem(dwFlags, psiItem);

        [HandleProcessCorruptedStateExceptions]
        public override void PostDeleteItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, uint hrDelete, IShellItem psiNewlyCreated)
        {
            this._View.UnvalidateDirectory();
            //Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);

            //if (!String.IsNullOrEmpty(obj.ParsingName)) {
            //	if (obj.Parent != null && obj.Parent.Equals(this.CurrentFolder)) {
            //		ShellItem theItem = this._View.Items.SingleOrDefault(s => s.GetHashCode() == obj.GetHashCode());
            //		if (theItem != null) {
            //			this._View.Items.Remove(theItem);
            //			if (this._View.IsGroupsEnabled) {
            //				this._View.SetGroupOrder(false);
            //			}
            //			this._View.SetSortCollumn(this._View.LastSortedColumnIndex, this._View.LastSortOrder, false);
            //		}
            //	}
            //}
            //Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEDIR,
            //	Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, obj.Parent.Pidl, IntPtr.Zero);
            //Shell32.SHChangeNotify(obj.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_RMDIR : Shell32.HChangeNotifyEventID.SHCNE_DELETE,
            //	Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, obj.Pidl, IntPtr.Zero);
            try
            {
                if (psiItem != null)
                {
                    //var obj = FileSystemListItem.ToFileSystemItem(this._View.LVHandle, new ShellItem(psiItem).Pidl);
                    //Shell32.SHChangeNotify(Shell32.HChangeNotifyEventID.SHCNE_UPDATEDIR,
                    //  Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, obj.Parent.PIDL, IntPtr.Zero);
                    //Shell32.SHChangeNotify(obj.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_RMDIR : Shell32.HChangeNotifyEventID.SHCNE_DELETE,
                    //  Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, obj.PIDL, IntPtr.Zero);
                    Marshal.ReleaseComObject(psiItem);
                    psiItem = null;
                }
                if (psiNewlyCreated != null)
                {
                    Marshal.ReleaseComObject(psiNewlyCreated);
                    psiNewlyCreated = null;
                }
            }
            catch (Exception)
            {

            }

        }
        public override void PreCopyItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName)
        {
            //DO NOT REMOVE!!!!
            //base.PreCopyItem(dwFlags, psiItem, psiDestinationFolder, pszNewName);
        }

        [HandleProcessCorruptedStateExceptions]
        public override void PostCopyItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName, uint hrCopy, IShellItem psiNewlyCreated)
        {
            //Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            //System.Windows.Forms.Application.DoEvents();
            //if (psiNewlyCreated == null)
            //	return;
            //var theNewItem = new ShellItem(psiNewlyCreated);
            //Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_MKDIR : Shell32.HChangeNotifyEventID.SHCNE_CREATE,
            //Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);
            //Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_UPDATEDIR : Shell32.HChangeNotifyEventID.SHCNE_UPDATEITEM,
            //		Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);
            //theNewItem.Dispose();
            this._View.UnvalidateDirectory();
            try
            {
                if (psiItem != null)
                {
                    Marshal.ReleaseComObject(psiItem);
                    psiItem = null;
                }
                if (psiNewlyCreated != null)
                {
                    Marshal.ReleaseComObject(psiNewlyCreated);
                    psiNewlyCreated = null;
                }
            }
            catch (Exception)
            {

            }
        }

        [HandleProcessCorruptedStateExceptions]
        public override void PostMoveItem(uint dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, string pszNewName, uint hrMove, IShellItem psiNewlyCreated)
        {
            //Shell32.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            //System.Windows.Forms.Application.DoEvents();
            //if (psiNewlyCreated == null)
            //	return;
            //var theNewItem = new ShellItem(psiNewlyCreated);
            //Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_MKDIR : Shell32.HChangeNotifyEventID.SHCNE_CREATE,
            //Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);
            //Shell32.SHChangeNotify(theNewItem.IsFolder ? Shell32.HChangeNotifyEventID.SHCNE_UPDATEDIR : Shell32.HChangeNotifyEventID.SHCNE_UPDATEITEM,
            //		Shell32.HChangeNotifyFlags.SHCNF_IDLIST | Shell32.HChangeNotifyFlags.SHCNF_FLUSH, theNewItem.Pidl, IntPtr.Zero);
            //theNewItem.Dispose();
            this._View.UnvalidateDirectory();
            try
            {
                if (psiItem != null)
                {
                    Marshal.ReleaseComObject(psiItem);
                    psiItem = null;
                }
                if (psiNewlyCreated != null)
                {
                    Marshal.ReleaseComObject(psiNewlyCreated);
                    psiNewlyCreated = null;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
