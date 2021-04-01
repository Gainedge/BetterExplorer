using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BExplorer.Shell {
  /// <summary>
  /// Interaction logic for FileOperation.xaml
  /// </summary>
  public partial class FileOperation : UserControl, IOperationsProgressDialog, IFileOperationProgressSink {
    public ShellView Owner { get; set; }
    public Boolean IsPause { get; set; }
    public Boolean IsStop { get; set; }
    public Guid Handle { get; set; }
    public Thread CurrentThread { get; set; }

    private readonly ManualResetEvent _Reset = new ManualResetEvent(true);
    private readonly System.Diagnostics.Stopwatch _Sw = new System.Diagnostics.Stopwatch();
    private Double _Speed = 0d;
    private Double _SpeedMax = 0d;
    private Double _LastMili = 0d;


    public FileOperation(ShellView view) {
      this.InitializeComponent();
      this.Owner = view;
    }

    public HResult StartProgressDialog(IntPtr hwndOwner, OPPROGDLGF flags) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
        (Action)(() => {
          if (this.Owner.OperationDialog == null) {
            this.Owner.OperationDialog = new FileOperationDialog();
          }
          //this.Owner.OperationDialog.AddFileOperation(this);

        }));

      return HResult.S_OK;
    }

    public void StopProgressDialog() {
      this.Owner.LargeImageList.SupressThumbnailGeneration(false);
      this._Sw.Stop();
      this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
        (Action)(() => {
          this.Owner.OperationDialog.RemoveFileOperation(this);
        }));
    }

    public void SetOperation(SPACTION action) {
      this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
        (ThreadStart)(() => {
          switch (action) {
            case SPACTION.SPACTION_NONE:
              break;
            case SPACTION.SPACTION_MOVING:
              this.Owner.OperationDialog.AddFileOperation(this);
              this.lblOperation.Text = "Moving ";
              this.Owner.LargeImageList.SupressThumbnailGeneration(true);
              break;
            case SPACTION.SPACTION_COPYING:
              this.Owner.OperationDialog.AddFileOperation(this);
              this.lblOperation.Text = "Copying ";
              this.Owner.LargeImageList.SupressThumbnailGeneration(true);
              break;
            case SPACTION.SPACTION_RECYCLING:
              this.lblOperation.Text = "Recycling ";
              break;
            case SPACTION.SPACTION_APPLYINGATTRIBS:
              break;
            case SPACTION.SPACTION_DOWNLOADING:
              break;
            case SPACTION.SPACTION_SEARCHING_INTERNET:
              break;
            case SPACTION.SPACTION_CALCULATING:
              break;
            case SPACTION.SPACTION_UPLOADING:
              break;
            case SPACTION.SPACTION_SEARCHING_FILES:
              break;
            case SPACTION.SPACTION_DELETING:
              this.lblOperation.Text = "Deleting ";
              //this._Reset.Reset();
              //if (MessageBox.Show("Are You sure you want to permanently delete these files?", "Delete permanetly",
              //  MessageBoxButton.YesNo) == MessageBoxResult.No) {
              //  this.CurrentThread.Abort();
              //  this.StopProgressDialog();
              //} else {
              //  this._Reset.Set();
                this.Owner.OperationDialog.AddFileOperation(this);
              //}
              break;
            case SPACTION.SPACTION_RENAMING:
              break;
            case SPACTION.SPACTION_FORMATTING:
              break;
            case SPACTION.SPACTION_COPY_MOVING:
              this.Owner.OperationDialog.AddFileOperation(this);
              this.lblOperation.Text = "Moving ";
              this.Owner.LargeImageList.SupressThumbnailGeneration(true);
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(action), action, null);
          }
        }));
    }

    public void SetMode(PDMODE mode) {
      if (mode == PDMODE.PDM_RUN) {
        //this.sw.Start();
      }
    }

    public void UpdateProgress(UInt64 ullPointsCurrent, UInt64 ullPointsTotal, UInt64 ullSizeCurrent, UInt64 ullSizeTotal,
      UInt64 ullItemsCurrent, UInt64 ullItemsTotal) {
      this._Reset.WaitOne();
      if (ullSizeCurrent > 0 && !this._Sw.IsRunning) {
        this._Sw.Start();
      }

      //this._Reset.WaitOne();
      if (this._Sw.IsRunning && this._Sw.Elapsed.TotalMilliseconds > 0) {
        this._Speed = ((Double) ullSizeCurrent / this._Sw.Elapsed.TotalMilliseconds) * 1000;
        this._SpeedMax = Math.Max(this._SpeedMax, this._Speed);
      }

      Double precentComplete = ((Double)ullSizeCurrent / (Double)ullSizeTotal) * 100d;

      this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
        (ThreadStart)(() => {
          if (!Double.IsNaN(precentComplete) && (this._Sw.Elapsed.TotalMilliseconds - this._LastMili) >= 50) {
            this._LastMili = this._Sw.Elapsed.TotalMilliseconds;
            this.lblProgress.Text = Math.Round(precentComplete, 0) + "% complete";
            this.lblItemsCount.Text = ullItemsTotal.ToString();
            this.lblItemsRemaining.Text =
              $"{ullItemsTotal - ullItemsCurrent} ({ShlWapi.StrFormatByteSize((Int64)ullSizeTotal - (Int64)ullSizeCurrent)})";
            this.prOverallProgress.Maximum = 100;
            this.prOverallProgress.Rate = this._Speed;
            this.prOverallProgress.Caption = "Speed: " + ShlWapi.StrFormatByteSize((Int64)this._Speed) + "/s";
            this.prOverallProgress.Value = precentComplete;
            if (ullSizeCurrent > 0) {
              this.lblTimeLeft.Text = TimeSpan.FromMilliseconds((this._Sw.Elapsed.TotalMilliseconds * (ullSizeTotal - ullSizeCurrent) / ullSizeCurrent) + 1000)
                .ToString(@"hh\:mm\:ss");
            }
          }
        }));

    }

    public void UpdateLocations(IShellItem psiSource, IShellItem psiTarget, IShellItem psiItem) {
      if (psiSource != null) {
        var item = FileSystemListItem.InitializeWithIShellItem(this.Owner.LVHandle, psiSource);
        this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
          (Action)(() => { this.lblFrom.Text = item.DisplayName; }));
        item.Dispose();
      }

      if (psiTarget != null) {
        var item = FileSystemListItem.InitializeWithIShellItem(this.Owner.LVHandle, psiTarget);
        this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
          (Action)(() => { this.lblTo.Text = item.DisplayName; }));
        item.Dispose();
      }
      if (psiItem != null) {
        var item = FileSystemListItem.InitializeWithIShellItem(this.Owner.LVHandle, psiItem);
        this.Dispatcher.BeginInvoke(DispatcherPriority.Render,
          (Action)(() => { this.lblFileName.Text = item.DisplayName; }));
        item.Dispose();
      }
    }

    public void StartOperations() {

    }

    public void FinishOperations(UInt32 hrResult) {

    }

    public void PreRenameItem(UInt32 dwFlags, IShellItem psiItem, String pszNewName) {
      this.Owner.IsRenameInProgress = true;
    }

    public void PostRenameItem(UInt32 dwFlags, IShellItem psiItem, String pszNewName, UInt32 hrRename, IShellItem psiNewlyCreated) {
      this.Owner.BeginInvoke((Action)(() => {
        if (hrRename == 2555912 && psiItem != null && psiNewlyCreated != null) {
          var oldItem = FileSystemListItem.InitializeWithIShellItem(this.Owner.LVHandle, psiItem);
          var newItem = FileSystemListItem.InitializeWithIShellItem(this.Owner.LVHandle, psiNewlyCreated);
          this.Owner.UpdateItem(oldItem, newItem);
        }
        this.Owner.IsRenameInProgress = false;
      }));
    }

    public void PreMoveItem(UInt32 dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, String pszNewName) {
      
    }

    public void PostMoveItem(UInt32 dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, String pszNewName, UInt32 hrMove,
      IShellItem psiNewlyCreated) {

    }

    public HResult PreCopyItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, String pszNewName) {
      //this.Owner.LargeImageList.SupressThumbnailGeneration(true);
      //MessageBox.Show("Text");
      dwFlags &= ~TRANSFER_SOURCE_FLAGS.TSF_OVERWRITE_EXIST;
      dwFlags |= TRANSFER_SOURCE_FLAGS.TSF_RENAME_EXIST;
      return HResult.S_OK;
    }

    public void PostCopyItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, IShellItem psiDestinationFolder, String pszNewName,
      UInt32 hrCopy, IShellItem psiNewlyCreated) {

    }

    public HResult PreDeleteItem(UInt32 dwFlags, IShellItem psiItem) {
      return HResult.S_OK;
    }

    public void PostDeleteItem(TRANSFER_SOURCE_FLAGS dwFlags, IShellItem psiItem, UInt32 hrDelete, IShellItem psiNewlyCreated) {

    }

    public void PreNewItem(UInt32 dwFlags, IShellItem psiDestinationFolder, String pszNewName) {

    }

    public void PostNewItem(UInt32 dwFlags, IShellItem psiDestinationFolder, String pszNewName, String pszTemplateName,
      UInt32 dwFileAttributes, UInt32 hrNew, IShellItem psiNewItem) {

    }

    public HResult UpdateProgress(UInt32 iWorkTotal, UInt32 iWorkSoFar) {
      if (this.IsStop) {
        //return HResult.E_CANCELED;
      }

      return HResult.S_OK;
    }

    public void ResetTimer() {

    }

    public void PauseTimer() {

    }

    public void ResumeTimer() {

    }

    public void GetMilliseconds(UInt64 pullElapsed, UInt64 pullRemaining) {

    }

    public HResult GetOperationStatus(ref PDOPSTATUS popstatus) {
      if (this.IsStop) {
        popstatus = PDOPSTATUS.PDOPS_CANCELLED;
      } else if (this.IsPause) {
        popstatus = PDOPSTATUS.PDOPS_PAUSED;
      } else {
        popstatus = PDOPSTATUS.PDOPS_RUNNING;
      }

      return HResult.S_OK;
    }

    private void BtnPause_OnClick(Object sender, RoutedEventArgs e) {
      if (this.IsPause) {
        this._Sw.Start();
        this._Reset.Set();
        this.btnContinue.Visibility = Visibility.Hidden;
        this.btnPause.Visibility = Visibility.Visible;
        this.prOverallProgress.SetBackGroundColor(Color.FromRgb(6, 176, 37));
      } else {
        this._Reset.Reset();
        this._Sw.Stop();
        this.btnContinue.Visibility = Visibility.Visible;
        this.btnPause.Visibility = Visibility.Hidden;
        this.prOverallProgress.SetBackGroundColor(Colors.Yellow);
      }
      this.IsPause = !this.IsPause;
    }

    private void BtnStop_OnClick(Object sender, RoutedEventArgs e) {
      //this.StopProgressDialog();
      //this.CurrentThread.Abort();
      this.IsStop = true;
    }

    private void FileOperation_OnLoaded(Object sender, RoutedEventArgs e) {
      
    }
  }

  public enum OperationType {
    Copy,
    Move,
    Delete,
    Rename,
    Decomress,
    Compress
  }
}
