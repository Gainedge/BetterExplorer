using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop {
  public enum PDOPSTATUS {
    PDOPS_RUNNING = 1,
    PDOPS_PAUSED = 2,
    PDOPS_CANCELLED = 3,
    PDOPS_STOPPED = 4,
    PDOPS_ERRORS = 5
  }
  public enum SPACTION : uint {
    SPACTION_NONE,
    SPACTION_MOVING,
    SPACTION_COPYING,
    SPACTION_RECYCLING,
    SPACTION_APPLYINGATTRIBS,
    SPACTION_DOWNLOADING,
    SPACTION_SEARCHING_INTERNET,
    SPACTION_CALCULATING,
    SPACTION_UPLOADING,
    SPACTION_SEARCHING_FILES,
    SPACTION_DELETING,
    SPACTION_RENAMING,
    SPACTION_FORMATTING,
    SPACTION_COPY_MOVING,
  }
  [Flags]
  public enum PDMODE : uint {
    PDM_DEFAULT = 0,
    PDM_RUN = 0x1,
    PDM_PREFLIGHT = 0x2,
    PDM_UNDOING = 0x4,
    PDM_ERRORSBLOCKING = 0x8,
    PDM_INDETERMINATE = 0x10
  }
  [Flags]
  public enum OPPROGDLGF : uint {
    OPPROGDLG_DEFAULT = 0,
    OPPROGDLG_AUTOTIME = 0x00000002,
    OPPROGDLG_ENABLEPAUSE = 0x80,
    OPPROGDLG_ALLOWUNDO = 0x100,
    OPPROGDLG_DONTDISPLAYSOURCEPATH = 0x200,
    OPPROGDLG_DONTDISPLAYDESTPATH = 0x400,
    OPPROGDLG_NOMULTIDAYESTIMATES = 0x800,
    OPPROGDLG_DONTDISPLAYLOCATIONS = 0x1000
  }
  [ComImport]
  [Guid("0C9FB851-E5C9-43EB-A370-F0677B13874C")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IOperationsProgressDialog {
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    HResult StartProgressDialog([In]IntPtr hwndOwner, [In]OPPROGDLGF flags);
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void StopProgressDialog();
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetOperation([In]SPACTION action);
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetMode(PDMODE mode);
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void UpdateProgress([In]ulong ullPointsCurrent, [In]ulong ullPointsTotal, [In]ulong ullSizeCurrent, [In]ulong ullSizeTotal, [In]ulong ullItemsCurrent, [In]ulong ullItemsTotal);
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void UpdateLocations([In]IShellItem psiSource, [In]IShellItem psiTarget, [In, Optional]IShellItem psiItem);
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void ResetTimer();
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void PauseTimer();
    [PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void ResumeTimer();
    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetMilliseconds([Out] ulong pullElapsed, [Out] ulong pullRemaining);

    //[PreserveSig]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetOperationStatus([In, Out] ref PDOPSTATUS popstatus);
    //PDOPSTATUS GetOperationStatus();
  }
}
