using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BExplorer.Shell._Plugin_Interfaces;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace BExplorer.Shell {
  /// <summary>Contains file operations that has a callback for UI operations/display</summary>
  /// <remarks>Every operation uses the callback if possible</remarks>
  public class IIFileOperation : IDisposable {
    private static readonly Guid _CLSIDFileOperation = new Guid("3ad05575-8857-4850-9277-11b85bdb8e09");
    private static readonly Type _FileOperationType = Type.GetTypeFromCLSID(_CLSIDFileOperation);

    private bool _Disposed;
    private IFileOperation _FileOperation;
    private IFileOperationProgressSink _CallbackSink;
    private FileOperation _ProgressDialog;
    private uint _SinkCookie;
    private Boolean _IsCopyInSameFolder { get; set; }
    private IListItemEx _ControlItem { get; set; }
    public ManualResetEvent Reset = new ManualResetEvent(true);

    /// <summary>
    /// 
    /// </summary>
    public IIFileOperation() : this(null, false) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner">The object that owns the file?</param>
    public IIFileOperation(IntPtr owner) : this(null, owner, false) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isRecycle">Allows Undo (likely adds to recycle bin)</param>
    public IIFileOperation(Boolean isRecycle) : this(null, isRecycle) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner">The object that owns the file?</param>
    /// <param name="isRecycle">Allows Undo (likely adds to recycle bin)</param>
    public IIFileOperation(IntPtr owner, Boolean isRecycle) : this(null, owner, isRecycle) { }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="callbackSink"></param>
    /// <param name="isRecycle"></param>
    public IIFileOperation(IFileOperationProgressSink callbackSink, Boolean isRecycle) : this(callbackSink, IntPtr.Zero, isRecycle) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callbackSink"></param>
    /// <param name="owner"></param>
    /// <param name="isRecycle"></param>
    public IIFileOperation(IFileOperationProgressSink callbackSink, IntPtr owner, Boolean isRecycle, IOperationsProgressDialog dialog = null) {
      this._CallbackSink = callbackSink;
      this._FileOperation = (IFileOperation)Activator.CreateInstance(_FileOperationType);

      var flags = isRecycle ? FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_ALLOWUNDO : FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_NOCONFIRMATION;
      this._FileOperation.SetOperationFlags(flags);

      if (this._CallbackSink != null) this._FileOperation.Advise(_CallbackSink, out this._SinkCookie);
      if (owner != IntPtr.Zero) this._FileOperation.SetOwnerWindow((uint)owner);
      if (dialog != null) {
        this._ProgressDialog = (FileOperation)dialog;
        this._ProgressDialog.FOReset = this.Reset;
        this._FileOperation.SetProgressDialog(this._ProgressDialog);
      }
    }

    public IIFileOperation(IFileOperationProgressSink callbackSink, IntPtr owner, Boolean isRecycle, Boolean isCopyInSameFolder, IListItemEx controlItem, IOperationsProgressDialog dialog = null) {
      this._CallbackSink = callbackSink;
      this._IsCopyInSameFolder = isCopyInSameFolder;
      this._ControlItem = controlItem;
      _FileOperation = (IFileOperation)Activator.CreateInstance(_FileOperationType);

      if (!isRecycle)
        this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_NOCONFIRMMKDIR);// | FileOperationFlags.FOF_NOCONFIRMATION);

      if (_CallbackSink != null) _FileOperation.Advise(_CallbackSink, out this._SinkCookie);
      if (owner != IntPtr.Zero) _FileOperation.SetOwnerWindow((uint)owner);
      if (dialog != null) {
        this._ProgressDialog = (FileOperation)dialog;
        this._ProgressDialog.FOReset = this.Reset;
        this._FileOperation.SetProgressDialog(this._ProgressDialog);
      }
    }

    /// <summary>
    /// Declares a new item that is to be created in a specified location. Does this silently and automatically renames items if needed.
    /// </summary>
    /// <param name="destinationFolder">The destination folder that will contain the new item.</param>
    /// <param name="name">The new item name</param>
    /// <param name="attributes">A bitwise value that specifies the file system attributes for the file or folder. See GetFileAttributes for possible values</param>
    public void NewItem(IListItemEx destinationFolder, String name, FileAttributes attributes) {
      this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_RENAMEONCOLLISION | FileOperationFlags.FOF_SILENT);
      this._FileOperation.NewItem(destinationFolder.ComInterface, attributes, name, null, null);
    }

    /// <summary>
    /// Copies the source item into the destination
    /// </summary>
    /// <param name="source">The item being copied</param>
    /// <param name="destination">The location to be copied to</param>
    public void CopyItem(IShellItem source, IListItemEx destination) {
      this.ThrowIfDisposed();
      if (this._IsCopyInSameFolder) {
        this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_RENAMEONCOLLISION | FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMMKDIR);
      }
      this._FileOperation.CopyItem(source, destination.ComInterface, null, null);
    }

    /// <summary>
    /// Copies items to a destination
    /// </summary>
    /// <param name="source">The items you want to copy</param>
    /// <param name="destination">The destination you want to copy the <paramref name="source"/> items</param>
    public void CopyItems(IShellItemArray source, IListItemEx destination) {
      this.ThrowIfDisposed();
      if (this._ControlItem.Equals(destination)) {
        this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_RENAMEONCOLLISION | FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMMKDIR);
      }

      List<IListItemEx> flatItemsList = new List<IListItemEx>();
      foreach (var shellItem in source.ToArray()) {
        var iListItem = FileSystemListItem.InitializeWithIShellItem(IntPtr.Zero, shellItem);
        if (iListItem.IsFolder) {
            flatItemsList.Add(iListItem);
            flatItemsList.AddRange(iListItem.GetContents(true, true));
        } else {
          flatItemsList.Add(iListItem);
        }
      }

      var duplicatedItems = new List<IListItemEx>();
      foreach (var lookupItem in flatItemsList.Where(w => !w.IsFolder)) {
        if (File.Exists(lookupItem.ParsingName.Replace(this._ControlItem.ParsingName, destination.ParsingName))) {
          duplicatedItems.Add(lookupItem);
        }
      }

      if (duplicatedItems.Count > 0 && !this._IsCopyInSameFolder) {
        Application.Current.Dispatcher.Invoke(DispatcherPriority.Render,
          (Action) (() => {
              this._ProgressDialog.pnlFileCollision.Visibility = Visibility.Visible;
              this._ProgressDialog.pnlFileOp.Visibility = Visibility.Collapsed;
              this._ProgressDialog.lblItemsCount.Text = flatItemsList.Count.ToString();
              this._ProgressDialog.lblFrom.Text = this._ControlItem.DisplayName;
              this._ProgressDialog.lblTo.Text = destination.DisplayName;
              this._ProgressDialog.lblOperation.Text = "Copying ";
              this._ProgressDialog.lblDuplicatedCount.Text = "Destination folder have " + duplicatedItems.Count + " duplicated items";
              this._ProgressDialog.Owner.OperationDialog.AddFileOperation(this._ProgressDialog);
              this.Reset.Reset();

            }
          ));
        this.Reset.WaitOne();
        if (this._ProgressDialog.IsReplaceAll) {
          this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMMKDIR);
        }
      }
      this._FileOperation.CopyItems(source, destination.ComInterface);
    }

    /// <summary>
    /// Moves the source item into the destination with a new name
    /// </summary>
    /// <param name="source">The item being moved</param>
    /// <param name="destination">The location to be moved to</param>
    /// <param name="newName">The new name of the file</param>
    public void MoveItem(IShellItem source, IListItemEx destination, String? newName) {
      this.ThrowIfDisposed();
      this._FileOperation.MoveItem(source, destination.ComInterface, newName, null);
    }

    /// <summary>
    /// Moves items to a destination
    /// </summary>
    /// <param name="source">The item being moved</param>
    /// <param name="destination">The location to be moved to</param>
    public void MoveItems(IShellItemArray source, IListItemEx destination) {
      this.ThrowIfDisposed();
      //this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_RENAMEONCOLLISION | FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMMKDIR);
      this._FileOperation.MoveItems(source, destination.ComInterface);
    }

    /// <summary>
    /// Renames the source item
    /// </summary>
    /// <param name="source">The IShellItem to be renamed</param>
    /// <param name="newName">The new name</param>
    public void RenameItem(IShellItem source, string newName) {
      this._FileOperation.SetOperationFlags(FileOperationFlags.FOF_SILENT);
      this._FileOperation.RenameItem(source, newName, null);
    }

    /// <summary>
    /// Declares a single item that is to be deleted. (Exception when <see cref="_Disposed"/> is <c>True</c>) 
    /// </summary>
    /// <param name="source">The item to be deleted</param>
    /// <exception cref="ObjectDisposedException">When <see cref="_Disposed"/> is <c>True</c></exception>
    public void DeleteItem(IListItemEx source) {
      this.ThrowIfDisposed();
      this._FileOperation.DeleteItem(source.ComInterface, null);
    }

    /// <summary>
    /// Declares a set of items that are to be deleted. (Exception when <see cref="_Disposed"/> is <c>True</c>) 
    /// </summary>
    /// <param name="source">The items to be deleted</param>
    /// <exception cref="ObjectDisposedException">When <see cref="_Disposed"/> is <c>True</c></exception>
    public void DeleteItems(IShellItemArray source) {
      this.ThrowIfDisposed();
      this._FileOperation.DeleteItems(source);
    }

    /// <summary>
    /// Preforms PerformOperations
    /// </summary>
    public void PerformOperations() {
      this.ThrowIfDisposed();
      try {
        this._FileOperation.PerformOperations();
      }
      catch {
      }
    }

    /// <summary>
    /// Returns GetAnyOperationsAborted
    /// </summary>
    /// <returns></returns>
    public bool GetAnyOperationAborted() {
      this.ThrowIfDisposed();
      return this._FileOperation.GetAnyOperationsAborted();
    }

    /// <summary>Is the item <see cref="_Disposed">Disposed</see>?</summary>
    private void ThrowIfDisposed() {
      if (this._Disposed) throw new ObjectDisposedException(GetType().Name);
    }

    /// <summary>
    /// Disposes of the object
    /// </summary>
    public void Dispose() {
      if (!this._Disposed) {
        this._Disposed = true;
        if (this._CallbackSink != null) this._FileOperation.Unadvise(this._SinkCookie);
        Marshal.FinalReleaseComObject(this._FileOperation);
      }
    }
  }
}
