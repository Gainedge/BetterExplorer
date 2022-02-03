using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using WPFUI.Win32;

namespace BExplorer.Shell {
  public static class Defaults {

    /// <summary>
    /// Convert and Collumns structure to LVCOLUMN (Native Listview Column)
    /// </summary>
    /// <param name="col">the column</param>
    /// <param name="isDetails"></param>
    /// <returns>resulting LVCOLUMN structure</returns>
    public static LVCOLUMN ToNativeColumn(this Collumns col, bool isDetails = false, Int32 width = -1) {
      LVCOLUMN column = new LVCOLUMN();
      column.mask = LVCF.LVCF_FMT | LVCF.LVCF_TEXT | LVCF.LVCF_WIDTH | LVCF.LVCF_MINWIDTH | LVCF.LVCF_SUBITEM;
      if (isDetails)
        column.cx = width == -1 ? col.Width : width;
      column.pszText = col.Name;
      column.iSubItem = col.Index;
      column.fmt = col.CollumnType == typeof(long) ? (LVCFMT.RIGHT) : (LVCFMT.LEFT);
      if (isDetails)
        column.cxMin = col.MinWidth;
      return column;
    }


  }

  public static class MediaProperties {
    public static PROPERTYKEY Rating = new PROPERTYKEY() {
      fmtid = Guid.Parse("64440492-4c8b-11d1-8b70-080036b11a03"),
      pid = 9
    };

    public static PROPERTYKEY Dimensions = new PROPERTYKEY() {
      fmtid = Guid.Parse("6444048f-4c8b-11d1-8b70-080036b11a03"),
      pid = 13
    };
  }

  public static class SystemProperties {
    public static PROPERTYKEY FileSize = new PROPERTYKEY() { fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 12 };
    public static PROPERTYKEY LinkTarget = new PROPERTYKEY() { fmtid = Guid.Parse("b9b4b3fc-2b51-4a42-b5d8-324146afcf25"), pid = 2 };
    public static PROPERTYKEY FileType = new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 4 };
    public static PROPERTYKEY DriveFreeSpace = new PROPERTYKEY() { fmtid = Guid.Parse("9b174b35-40ff-11d2-a27e-00c04fc30871"), pid = 7 };
    public static PROPERTYKEY DateModified = new PROPERTYKEY() { fmtid = Guid.Parse("b725f130-47ef-101a-a5f1-02608c9eebac"), pid = 14 };
    public static PROPERTYKEY PerceivedType = new PROPERTYKEY() { fmtid = Guid.Parse("28636aa6-953d-11d2-b5d6-00c04fd918d0"), pid = 9 };

  }

  public static class SpecialProperties {
    public static PROPERTYKEY PropListTileInfo = new PROPERTYKEY() { fmtid = Guid.Parse("C9944A21-A406-48FE-8225-AEC7E24C211B"), pid = 3 };
  }
}
