using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BExplorer.Shell {

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct LVITEM {
    public LVIF mask;
    public int iItem;
    public int iSubItem;
    public LVIS state;
    public LVIS stateMask;
    [MarshalAs(UnmanagedType.LPTStr)]
    public string pszText;
    public int cchTextMax;
    public int iImage;
    public IntPtr lParam;
    public int iIndent;
    public int iGroupId;
    public int cColumns;
    public IntPtr puColumns;
    public int piColFmt;
    public int iGroup;
  }

  /*
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct LVITEM2 {
    public uint mask;
    public int iItem;
    public int iSubItem;
    public uint state;
    public uint stateMask;
    public IntPtr pszText;
    public int cchTextMax;
    public int iImage;
    public IntPtr lParam;
    public int iIndent;
    public int iGroupId;
    public int cColumns;
    public IntPtr puColumns;
    public int piColFmt;
    public int iGroup;
  }
  */

  [StructLayout(LayoutKind.Sequential)]
  public struct NMHDR {
    // 12/24
    public IntPtr hwndFrom;
    public IntPtr idFrom;
    public int code;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct NMLVEMPTYMARKUP {
    public NMHDR hdr;
    public UInt32 dwFlags;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)]
    public String szMarkup;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct NMLVDISPINFO {
    public NMHDR hdr;
    public LVITEM item;
  }

  /*
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct NMLVDISPINFO2 {
    public NMHDR hdr;
    public LVITEM2 item;
  }
  */

  public enum LVCF {
    LVCF_FMT = 0x1,
    LVCF_WIDTH = 0x2,
    LVCF_TEXT = 0x4,
    LVCF_SUBITEM = 0x8,
    LVCF_MINWIDTH = 0x0040
  }

  /// <summary>Native Listview column</summary>
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct LVCOLUMN {
    public LVCF mask;
    public LVCFMT fmt;
    public Int32 cx;
    public String pszText;
    public Int32 cchTextMax;
    public Int32 iSubItem;
    public int iImage;
    public int iOrder;
    public int cxMin;
    public int cxDefault;
    public int cxIdeal;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct NMHEADER {
    public NMHDR hdr;
    public int iItem;
    public int iButton;
    public IntPtr pitem;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct NMITEMACTIVATE {
    public NMHDR hdr;
    public int iItem;
    public int iSubItem;
    public uint uNewState;
    public uint uOldState;
    public uint uChanged;
    public Point ptAction;
    public IntPtr lParam;
    public uint uKeyFlags;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct NMLISTVIEW {
    public NMHDR hdr;
    public int iItem;
    public int iSubItem;
    public LVIS uNewState;
    public LVIS uOldState;
    public LVIF uChanged;
    public POINT ptAction;
    public IntPtr lParam;
  }

  [StructLayout(LayoutKind.Sequential)]
  public class NMLVKEYDOWN {
    public NMHDR hdr;
    public short wVKey;
    public uint flags;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct NMLVFINDITEM {
    public NMHDR hdr;
    public int iStart;
    public LVFINDINFO lvfi;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct NMLVGETINFOTIP {
    public NMHDR hdr;
    public int dwFlags;
    public IntPtr pszText;
    public int cchTextMax;
    public int iItem;
    public int iSubItem;
    public IntPtr lParam;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct LVFINDINFO {
    public LVFI flags;
    public string psz;
    public IntPtr lParam;
    public int ptX; // was POINT pt
    public int ptY;
    public int vkDirection;
  }

  public enum LVFI {
    LVFI_PARAM = 0x0001,
    LVFI_STRING = 0x0002,
    LVFI_SUBSTRING = 0x0004,  // Same as LVFI_PARTIAL
    LVFI_PARTIAL = 0x0008,
    LVFI_WRAP = 0x0020,
    LVFI_NEARESTXY = 0x0040,
  }

  public enum LVTVIM {
    LVTVIM_COLUMNS = 2,
    LVTVIM_TILESIZE = 1,
    LVTVIM_LABELMARGIN = 4,
  }

  public enum LVTVIF {
    LVTVIF_AUTOSIZE = 0,
    LVTVIF_FIXEDHEIGHT = 2,
    LVTVIF_FIXEDSIZE = 3,
    LVTVIF_FIXEDWIDTH = 1,
  }

  public struct INTEROP_SIZE {
    public int cx;
    public int cy;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct LVTILEVIEWINFO {
    public uint cbSize;
    public uint dwMask;
    public uint dwFlags;
    public INTEROP_SIZE sizeTile;
    public int cLines;
    public User32.RECT rcLabelMargin;
  }


  /*
  /// <SUMMARY> 
  /// Inherited child for the class Graphics encapsulating 
  /// additional functionality for curves and rounded rectangles. 
  /// </SUMMARY> 
  public class ExtendedGraphics {
    private Graphics mGraphics;
    public Graphics Graphics
    {
      get { return this.mGraphics; }
      set { this.mGraphics = value; }
    }

    public ExtendedGraphics(Graphics graphics) {
      this.Graphics = graphics;
    }


    #region Fills a Rounded Rectangle with integers. 
    public void FillRoundRectangle(System.Drawing.Brush brush,
            int x, int y,
            int width, int height, int radius) {

      float fx = Convert.ToSingle(x);
      float fy = Convert.ToSingle(y);
      float fwidth = Convert.ToSingle(width);
      float fheight = Convert.ToSingle(height);
      float fradius = Convert.ToSingle(radius);
      this.FillRoundRectangle(brush, fx, fy,
              fwidth, fheight, fradius);

    }
    #endregion


    #region Fills a Rounded Rectangle with continuous numbers.
    public void FillRoundRectangle(System.Drawing.Brush brush,
            float x, float y,
            float width, float height, float radius) {
      RectangleF rectangle = new RectangleF(x, y, width, height);
      GraphicsPath path = this.GetRoundedRect(rectangle, radius);
      this.Graphics.FillPath(brush, path);
    }
    #endregion


    #region Draws a Rounded Rectangle border with integers. 
    public void DrawRoundRectangle(System.Drawing.Pen pen, int x, int y,
            int width, int height, int radius) {
      float fx = Convert.ToSingle(x);
      float fy = Convert.ToSingle(y);
      float fwidth = Convert.ToSingle(width);
      float fheight = Convert.ToSingle(height);
      float fradius = Convert.ToSingle(radius);
      this.DrawRoundRectangle(pen, fx, fy, fwidth, fheight, fradius);
    }
    #endregion


    #region Draws a Rounded Rectangle border with continuous numbers. 
    public void DrawRoundRectangle(System.Drawing.Pen pen,
            float x, float y,
            float width, float height, float radius) {
      RectangleF rectangle = new RectangleF(x, y, width, height);
      GraphicsPath path = this.GetRoundedRect(rectangle, radius);
      this.Graphics.DrawPath(pen, path);
    }
    #endregion


    #region Get the desired Rounded Rectangle path. 
    private GraphicsPath GetRoundedRect(RectangleF baseRect,
             float radius) {
      // if corner radius is less than or equal to zero, 
      // return the original rectangle 
      if (radius <= 0.0F) {
        GraphicsPath mPath = new GraphicsPath();
        mPath.AddRectangle(baseRect);
        mPath.CloseFigure();
        return mPath;
      }

      // if the corner radius is greater than or equal to 
      // half the width, or height (whichever is shorter) 
      // then return a capsule instead of a lozenge 
      if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
        return GetCapsule(baseRect);

      // create the arc for the rectangle sides and declare 
      // a graphics path object for the drawing 
      float diameter = radius * 2.0F;
      SizeF sizeF = new SizeF(diameter, diameter);
      RectangleF arc = new RectangleF(baseRect.Location, sizeF);
      GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

      // top left arc 
      path.AddArc(arc, 180, 90);

      //// top right arc 
      //arc.X = baseRect.Right;// - diameter;
      //path.AddArc(arc, 270, 0);
      path.AddLine(baseRect.Left + radius, baseRect.Top, baseRect.Right, baseRect.Top);
      path.AddLine(baseRect.Right, baseRect.Top, baseRect.Right, baseRect.Bottom);
      // bottom right arc 
      //arc.Y = baseRect.Bottom - diameter;
      //path.AddArc(arc, 0, 90);

      path.AddLine(baseRect.Right, baseRect.Bottom, baseRect.Left + radius, baseRect.Bottom);
      //path.AddLine(baseRect.Right, baseRect.Top, baseRect.Right, baseRect.Bottom);

      // bottom left arc
      //arc.X = baseRect.Left;
      //path.AddArc(arc, 360, 90);

      path.AddLine(baseRect.Left, baseRect.Bottom - radius, baseRect.Left, baseRect.Top + radius);

      path.CloseFigure();
      return path;
    }
    #endregion

    #region Gets the desired Capsular path. 
    private GraphicsPath GetCapsule(RectangleF baseRect) {
      float diameter;
      RectangleF arc;
      GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
      try {
        if (baseRect.Width > baseRect.Height) {
          // return horizontal capsule 
          diameter = baseRect.Height;
          SizeF sizeF = new SizeF(diameter, diameter);
          arc = new RectangleF(baseRect.Location, sizeF);
          path.AddArc(arc, 90, 180);
          arc.X = baseRect.Right - diameter;
          path.AddArc(arc, 270, 180);
        } else if (baseRect.Width < baseRect.Height) {
          // return vertical capsule 
          diameter = baseRect.Width;
          SizeF sizeF = new SizeF(diameter, diameter);
          arc = new RectangleF(baseRect.Location, sizeF);
          path.AddArc(arc, 180, 180);
          arc.Y = baseRect.Bottom - diameter;
          path.AddArc(arc, 0, 180);
        } else {
          // return circle 
          path.AddEllipse(baseRect);
        }
      } catch {
        path.AddEllipse(baseRect);
      } finally {
        path.CloseFigure();
      }
      return path;
    }
    #endregion
  }
  */

  public static class Extensions {

    /// <summary>
    /// Converts a <see cref="ListViewGroupEx"/> into a <see cref="LVGROUP2"/> (Native ListView Group)
    /// </summary>
    /// <param name="group">The <see cref="ListViewGroupEx"/> you want to convert</param>
    /// <returns></returns>
    public static LVGROUP2 ToNativeListViewGroup(this ListViewGroupEx group) {
      var nativeGroup = new LVGROUP2 {
        cbSize = (UInt32)Marshal.SizeOf(typeof(LVGROUP2)),
        mask = (UInt32)(GroupMask.LVGF_HEADER ^ GroupMask.LVGF_STATE ^ GroupMask.LVGF_GROUPID),
        stateMask = (UInt32)GroupState.LVGS_COLLAPSIBLE,
        state = (UInt32)GroupState.LVGS_COLLAPSIBLE,
        pszHeader = group.Header,
        iGroupId = group.Index
      };

      if (group.Items.Any()) {
        nativeGroup.cItems = group.Items.Count();
        nativeGroup.mask ^= (UInt32)GroupMask.LVGF_ITEMS;
      }

      return nativeGroup;
    }

    /// <summary>
    /// Factory to create <see cref="Collumns"/> (All it does is assign values)
    /// </summary>
    /// <param name="column">Collumns.Name = column.pszText</param>
    /// <param name="pkey"></param>
    /// <param name="type">Collumns.pkey = pkey</param>
    /// <param name="isColumnHandler">Collumns.IsColumnHandler = isColumnHandler</param>
    /// <param name="minWidth">Collumns.MinWidth = minWidth</param>
    /// <returns>The new Collumns</returns>
    public static Collumns ToCollumns(this LVCOLUMN column, PROPERTYKEY pkey, Type type, Boolean isColumnHandler, Int32 minWidth) {
      return new Collumns {
        pkey = pkey,
        Name = column.pszText,
        Width = minWidth,
        IsColumnHandler = isColumnHandler,
        CollumnType = type,
        MinWidth = minWidth
      };
    }

    public static void SetSplitButton(this Collumns column, IntPtr handle, int index) {
      var item = new HDITEM { mask = HDITEM.Mask.Format };

      User32.SendMessage(handle, MSG.HDM_GETITEM, index, ref item);

      item.fmt |= HDITEM.Format.HDF_SPLITBUTTON;

      User32.SendMessage(handle, MSG.HDM_SETITEM, index, ref item);
    }

    /// <summary>
    /// Converts a File/Folder path into a proper string used to create a <see cref="ShellItem"/>
    /// </summary>
    /// <param name="path">The path you want to convert</param>
    /// <returns></returns>
    public static String ToShellParsingName(this String path) {
      if (path.EndsWith("\\") || path.EndsWith("\""))
        path = path.TrimEnd(Char.Parse("\\"), Char.Parse("\""));
      if (path.StartsWith("shell::"))
        return path;
      if (path.StartsWith("%"))
        return Environment.ExpandEnvironmentVariables(path);
      else if (path.StartsWith("::") && !path.StartsWith(@"\\"))
        return $"shell:{path}";
      else if (!path.StartsWith(@"\\")) {
        if (path.Contains(":")) {
          return $"{path}{(path.EndsWith(@"\") ? String.Empty : Path.DirectorySeparatorChar.ToString())}";
        } else {
          try {
            return $"{path}{Path.DirectorySeparatorChar}";
          } catch (Exception) {
            return @"\\" + $"{path}{Path.DirectorySeparatorChar}";
            throw;
          }
        }
      } else
        return path;
    }

    public static System.Runtime.InteropServices.ComTypes.IDataObject GetIDataObject(this IListItemEx[] items, out IntPtr dataObjectPtr) {
      var parent = items[0].Parent ?? items[0];

      var pidls = new IntPtr[items.Length];
      for (var i = 0; i < items.Length; i++)
        pidls[i] = items[i].ILPidl;
      var iidIDataObject = Ole32.IID_IDataObject;
      parent.GetIShellFolder().GetUIObjectOf(IntPtr.Zero, (UInt32)pidls.Length, pidls, ref iidIDataObject, 0, out dataObjectPtr);

      System.Runtime.InteropServices.ComTypes.IDataObject dataObj =
                      (System.Runtime.InteropServices.ComTypes.IDataObject)
                                      Marshal.GetTypedObjectForIUnknown(dataObjectPtr, typeof(System.Runtime.InteropServices.ComTypes.IDataObject));

      return dataObj;
    }

    /*
    public static System.Runtime.InteropServices.ComTypes.IDataObject GetIDataObject(this IListItemEx item, out IntPtr dataObjectPtr) {
      var parent = item.Parent ?? item;

      IntPtr[] pidls = new IntPtr[1];
      pidls[0] = item.ILPidl;
      Guid IID_IDataObject = Ole32.IID_IDataObject;
      parent.GetIShellFolder().GetUIObjectOf(IntPtr.Zero, (uint)pidls.Length, pidls, ref IID_IDataObject, 0, out dataObjectPtr);

      System.Runtime.InteropServices.ComTypes.IDataObject dataObj =
                      (System.Runtime.InteropServices.ComTypes.IDataObject)
                                      Marshal.GetTypedObjectForIUnknown(dataObjectPtr, typeof(System.Runtime.InteropServices.ComTypes.IDataObject));

      return dataObj;
    }
    */

    /// <summary>
    /// Sets the <paramref name="row"/> and <paramref name="column"/> based on the row and column that the <paramref name="hitPoint">point</paramref> falls on
    /// </summary>
    /// <param name="shellView">The <see cref="ShellView"/> you want to test with</param>
    /// <param name="hitPoint">The point on the screen you awnt to look for</param>
    /// <param name="row">The value for row that was hit</param>
    /// <param name="column">The value for column that was hit</param>
    /// <returns>Was the <paramref name="shellView"/> hit at all?</returns>
    public static bool HitTest(this ShellView shellView, Point hitPoint, out int row, out int column) {
      // clear the output values
      row = column = -1;

      // set up some win32 api constant values
      const int LVM_FIRST = 0x1000;
      //const int LVM_SUBITEMHITTEST = (LVM_FIRST + 57);
      const int LVM_HITTEST = (LVM_FIRST + 18);

      const int LVHT_NOWHERE = 0x1;
      const int LVHT_ONITEMICON = 0x2;
      const int LVHT_ONITEMLABEL = 0x4;
      const int LVHT_ONITEMSTATEICON = 0x8;
      const int LVHT_EX_ONCONTENTS = 0x04000000;
      const int LVHT_ONITEM = (LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON | LVHT_EX_ONCONTENTS);

      // set up the return value
      bool hitLocationFound = false;

      // initialize a hittest information structure
      LVHITTESTINFO lvHitTestInfo = new LVHITTESTINFO();
      lvHitTestInfo.pt.x = hitPoint.X;
      lvHitTestInfo.pt.y = hitPoint.Y;

      // send the hittest message to find out where the click was
      if (User32.SendMessage(shellView.LVHandle, LVM_HITTEST, 0, ref lvHitTestInfo) != 0) {
        bool nowhere = ((lvHitTestInfo.flags & LVHT_NOWHERE) != 0);
        bool onItem = ((lvHitTestInfo.flags & LVHT_ONITEM) != 0);

        if (onItem && !nowhere) {
          row = lvHitTestInfo.iItem;
          column = lvHitTestInfo.iSubItem;
          hitLocationFound = true;
        }
      } else if (User32.SendMessage(shellView.LVHandle, LVM_FIRST, 0, ref lvHitTestInfo) != 0) {
        row = 0;
        hitLocationFound = true;
      }

      return hitLocationFound;
    }

    /// <summary>
    /// Converts an <see cref="IShellItemArray"/> into a IShellItem[]
    /// </summary>
    /// <param name="shellItemArray">The Interface you want to convert</param>
    /// <returns></returns>
    public static IShellItem[] ToArray(this IShellItemArray shellItemArray) {
      var items = new List<IShellItem>();
      if (shellItemArray == null)
        return items.ToArray();
      try {
        uint itemCount = 0;
        shellItemArray.GetCount(out itemCount);
        for (uint index = 0; index < itemCount; index++) {
          IShellItem iShellItem = null;
          shellItemArray.GetItemAt(index, out iShellItem);
          items.Add(iShellItem);
        }
      } finally {
        //Marshal.ReleaseComObject(shellItemArray);
      }
      return items.ToArray();
    }

    /*
    public static IListItemEx[] ToIListItemArray(this IShellItemArray shellItemArray) {
      var items = new List<IListItemEx>();
      if (shellItemArray == null) return items.ToArray();
      try {
        uint itemCount = 0;
        shellItemArray.GetCount(out itemCount);
        for (uint index = 0; index < itemCount; index++) {
          IShellItem iShellItem = null;
          shellItemArray.GetItemAt(index, out iShellItem);
          items.Add(FileSystemListItem.InitializeWithIShellItem(IntPtr.Zero, iShellItem));
        }
      } finally {
        Marshal.ReleaseComObject(shellItemArray);
      }
      return items.ToArray();
    }
    */

    /// <summary>
    /// Converts a <see cref="IDataObject"/> into an <see cref="IShellItemArray"/>
    /// </summary>
    /// <param name="dataobject">The Interface you want to convert</param>
    /// <returns></returns>
    public static IShellItemArray ToShellItemArray(this IDataObject dataobject) {
      IShellItemArray shellItemArray;
      var iid = new Guid(InterfaceGuids.IShellItemArray);
      Shell32.SHCreateShellItemArrayFromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)dataobject, iid, out shellItemArray);
      return shellItemArray;
    }

    /// <summary>
    /// Converts a <see cref="IDataObject"/> into an <see cref="IShellItemArray"/>
    /// </summary>
    /// <param name="dataobject">The Interface you want to convert</param>
    /// <returns></returns>
    public static IShellItemArray ToShellItemArray(this System.Windows.IDataObject dataobject) {
      IShellItemArray shellItemArray;
      var iid = new Guid(InterfaceGuids.IShellItemArray);
      Shell32.SHCreateShellItemArrayFromDataObject((System.Runtime.InteropServices.ComTypes.IDataObject)dataobject, iid, out shellItemArray);
      return shellItemArray;
    }

    public static System.Windows.DragDropEffects GetDropEffect(this IDataObject dataObject) {
      var dragDropEffect = System.Windows.DragDropEffects.Copy;
      if (dataObject.GetDataPresent("Preferred DropEffect")) {
        object data = dataObject.GetData("Preferred DropEffect", true);

        if (data is System.IO.Stream) {
          var stream = (Stream)data;
          var reader = new StreamReader(stream);
          int value = reader.Read();
          stream.Position = 0; // This had no apparent effect

          if ((value & 2) == 2) {
            dragDropEffect = System.Windows.DragDropEffects.Move;
          } else {
            dragDropEffect = dragDropEffect = System.Windows.DragDropEffects.Copy;
          }
        }
      }
      return dragDropEffect;
    }

    [DllImport("shell32.dll", CharSet = CharSet.None)]
    public static extern int ILGetSize(IntPtr pidl);

    /*
    public static void Clear(this ConcurrentBag<Tuple<int, PROPERTYKEY, object>> bag) {
      Tuple<int, PROPERTYKEY, object> tmp = null;
      while (!bag.IsEmpty) {
        bag.TryTake(out tmp);
        if (tmp != null) tmp = null;
      }
    }
    */

    public static MemoryStream CreateShellIDList(this IListItemEx[] items) {
      // first convert all files into pidls list
      int pos = 0;
      byte[][] pidls = new byte[items.Count()][];
      foreach (var item in items) {
        // Get pidl based on name
        IntPtr pidl = item.PIDL;
        int pidlSize = ILGetSize(pidl);
        // Copy over to our managed array
        pidls[pos] = new byte[pidlSize];
        Marshal.Copy(pidl, pidls[pos++], 0, pidlSize);
        //ILFree(pidl);
      }

      // Determine where in CIDL we will start pumping PIDLs
      int pidlOffset = 4 * (items.Count() + 2);
      // Start the CIDL stream
      var memStream = new MemoryStream();
      var sw = new BinaryWriter(memStream);
      // Initialize CIDL with a count of files
      sw.Write(items.Count());
      // Calculate and write relative offsets of every pidl starting with root
      sw.Write(pidlOffset);
      pidlOffset += 4; // root is 4 bytes
      foreach (var pidl in pidls) {
        sw.Write(pidlOffset);
        pidlOffset += pidl.Length;
      }

      // Write the root pidl (0) followed by all pidls
      sw.Write(0);
      foreach (var pidl in pidls)
        sw.Write(pidl);
      // stream now contains the CIDL
      return memStream;
    }

    /// <summary>
    /// Is the current <paramref name="checkedItem">Item</paramref> in the <paramref name="currentFolder">Folder</paramref>?
    /// </summary>
    /// <param name="checkedItem">The current item who's container/parent you want to check</param>
    /// <param name="currentFolder">The folder you are looking for</param>
    /// <returns>Is the current <paramref name="checkedItem">Item</paramref> in the <paramref name="currentFolder">Folder</paramref>?</returns>
    /// <remarks>
    /// 1. Special Logic for Library folders (.library-ms)
    /// </remarks>
    public static Boolean IsInCurrentFolder(this IListItemEx checkedItem, IListItemEx currentFolder) {
      var isLibraryContainer = currentFolder?.Extension == ".library-ms";
      if (isLibraryContainer) {
        var library = ShellLibrary.Load(currentFolder.DisplayName, true);
        var libraryFolders = library.Select(w => w).ToArray();
        if (libraryFolders.Count(
          c => c.ParsingName.Equals(checkedItem.Parent?.ParsingName, StringComparison.InvariantCultureIgnoreCase)) > 0) {
          library.Close();
          return true;
        }
        library.Close();
        return false;
      } else {
        return checkedItem?.Parent?.ParsingName == currentFolder?.ParsingName;
      }
    }

    public static IOrderedEnumerable<IListItemEx> SetSortCollumn(this IEnumerable<IListItemEx> items, ShellView view, Boolean isReorder, Collumns column, SortOrder order, Boolean reverseOrder = true) {
      if (column == null) {
        column = view.Collumns.FirstOrDefault();
      }

      try {
        IOrderedEnumerable<IListItemEx> result = null;
        //var selectedItems = this.SelectedItems.ToArray();
        if (column.ID == view.LastSortedColumnId && reverseOrder) {
          // Reverse the current sort direction for this column.
          view.LastSortOrder = view.LastSortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        } else {
          // Set the column number that is to be sorted; default to ascending.
          view.LastSortedColumnId = column.ID;
          view.LastSortOrder = order;
        }

        if (isReorder) {
          var itemsQuery = items.Where(w => view.ShowHidden || !w.IsHidden).OrderByDescending(o => o.IsFolder);
          if (column.CollumnType != typeof(String)) {
            if (order == SortOrder.Ascending) {
              result = itemsQuery.ThenBy(o => o.GetPropertyValue(column.pkey, typeof(String)).Value ?? "1");
            } else {
              result = itemsQuery.ThenByDescending(o => o.GetPropertyValue(column.pkey, typeof(String)).Value ?? "1");
            }
          } else {
            if (order == SortOrder.Ascending) {
              result = itemsQuery.ThenBy(o => o.GetPropertyValue(column.pkey, typeof(String)).Value?.ToString(), NaturalStringComparer.Default);
            } else {
              result = itemsQuery.ThenByDescending(o => o.GetPropertyValue(column.pkey, typeof(String)).Value?.ToString(), NaturalStringComparer.Default);
            }
          }

          var i = 0;
        }


        //var colIndexReal = view.Collumns.IndexOf(view.Collumns.FirstOrDefault(w => w.ID == view.LastSortedColumnId));
        //if (colIndexReal > -1) {
        //  User32.SendMessage(view.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, colIndexReal, 0);
        //  view.SetSortIcon(colIndexReal, order);
        //} else {
        //  User32.SendMessage(view.LVHandle, MSG.LVM_SETSELECTEDCOLUMN, -1, 0);
        //}

        //if (!this.IsRenameInProgress) {
        //  this.SelectItems(selectedItems);
        //}
        return result;
      } catch {
        return null;
      }
    }

    public static Color ToDrawingColor(this System.Windows.Media.Color color) {
      return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public static IntPtr ToWin32Color(this Color color) {
      return (IntPtr)(UInt32)ColorTranslator.ToWin32(color);
    }
  }
}
