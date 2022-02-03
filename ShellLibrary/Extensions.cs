using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using WPFUI.Win32;
using Brush = System.Drawing.Brush;
using Pen = System.Drawing.Pen;

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
  public struct NMLISTVIEW {
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
      if (path == null) return String.Empty;
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


    public static System.Drawing.Point ToPoint(this IntPtr lparam) {
      return new System.Drawing.Point(lparam.ToInt32() & 0xFFFF, lparam.ToInt32() >> 16);
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
    public static void Clear<T>(this ConcurrentQueue<T> queue) {
      T item;
      while (queue.TryDequeue(out item)) {
        // do nothing
      }
    }

    public static void DrawArrowHead(this Graphics gr, Pen pen, PointF p, float nx, float ny, float length) {
      float ax = length * (-ny - nx);
      float ay = length * (nx - ny);
      PointF[] points =
      {
        new PointF(p.X + ax, p.Y + ay),
        p,
        new PointF(p.X - ay, p.Y + ax)
      };
      gr.DrawLines(pen, points);
    }
    public static int GetDepth(this TreeViewItem item) {
      TreeViewItem parent;
      while ((parent = GetParent(item)) != null) {
        return GetDepth(parent) + 1;
      }
      return 0;
    }

    private static TreeViewItem GetParent(TreeViewItem item) {
      var parent = VisualTreeHelper.GetParent(item);
      while (!(parent is TreeViewItem || parent is System.Windows.Controls.TreeView)) {
        parent = VisualTreeHelper.GetParent(parent);
      }
      return parent as TreeViewItem;
    }
    public static TreeViewItem ContainerFromItemRecursive(this ItemContainerGenerator root, object item) {
      var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
      if (treeViewItem != null)
        return treeViewItem;
      foreach (var subItem in root.Items) {
        treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
        var search = treeViewItem?.ItemContainerGenerator.ContainerFromItemRecursive(item);
        if (search != null)
          return search;
      }
      return null;
    }
    public static GraphicsPath RoundedRect(Rectangle bounds, int radius) {
      int diameter = radius * 2;
      System.Drawing.Size size = new System.Drawing.Size(diameter, diameter);
      Rectangle arc = new Rectangle(bounds.Location, size);
      GraphicsPath path = new GraphicsPath();

      if (radius == 0) {
        path.AddRectangle(bounds);
        return path;
      }

      // top left arc  
      path.AddArc(arc, 180, 90);

      // top right arc  
      arc.X = bounds.Right - diameter;
      path.AddArc(arc, 270, 90);

      // bottom right arc  
      arc.Y = bounds.Bottom - diameter;
      path.AddArc(arc, 0, 90);

      // bottom left arc 
      arc.X = bounds.Left;
      path.AddArc(arc, 90, 90);

      path.CloseFigure();
      return path;
    }
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius) {
      if (graphics == null)
        throw new ArgumentNullException("graphics");
      if (pen == null)
        throw new ArgumentNullException("pen");

      using (GraphicsPath path = RoundedRect(bounds, cornerRadius)) {
        graphics.DrawPath(pen, path);
      }
    }

    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius) {
      if (graphics == null)
        throw new ArgumentNullException("graphics");
      if (brush == null)
        throw new ArgumentNullException("brush");

      using (GraphicsPath path = RoundedRect(bounds, cornerRadius)) {
        graphics.FillPath(brush, path);
      }
    }

    public static Rectangle ToRectangle(this User32.RECT rect, Int32 smallerWith = 0) {
      return new Rectangle(rect.Left + smallerWith, rect.Top + smallerWith, rect.Width - 2 * smallerWith, rect.Height - 2 * smallerWith);
    }

    public static Point ToPoint(this POINT ptPoint) {
      return new Point(ptPoint.x, ptPoint.y);
    }

  }
  public static class StringHelper {
    /// <summary>Allocates a block of memory allocated from the unmanaged COM task allocator sufficient to hold the number of specified characters.</summary>
    /// <param name="count">The number of characters, inclusive of the null terminator.</param>
    /// <param name="memAllocator">The method used to allocate the memory.</param>
    /// <param name="charSet">The character set.</param>
    /// <returns>The address of the block of memory allocated.</returns>
    public static IntPtr AllocChars(uint count, Func<int, IntPtr> memAllocator, CharSet charSet = CharSet.Auto) {
      if (count == 0) return IntPtr.Zero;
      var sz = GetCharSize(charSet);
      var ptr = memAllocator((int)count * sz);
      if (count > 0) {
        if (sz == 1)
          Marshal.WriteByte(ptr, 0);
        else
          Marshal.WriteInt16(ptr, 0);
      }
      return ptr;
    }

    /// <summary>Allocates a block of memory allocated from the unmanaged COM task allocator sufficient to hold the number of specified characters.</summary>
    /// <param name="count">The number of characters, inclusive of the null terminator.</param>
    /// <param name="charSet">The character set.</param>
    /// <returns>The address of the block of memory allocated.</returns>
    public static IntPtr AllocChars(uint count, CharSet charSet = CharSet.Auto) => AllocChars(count, Marshal.AllocCoTaskMem, charSet);

    /// <summary>Copies the contents of a managed <see cref="SecureString"/> object to a block of memory allocated from the unmanaged COM task allocator.</summary>
    /// <param name="s">The managed object to copy.</param>
    /// <param name="charSet">The character set.</param>
    /// <returns>The address, in unmanaged memory, where the <paramref name="s"/> parameter was copied to, or 0 if a null object was supplied.</returns>
    public static IntPtr AllocSecureString(SecureString s, CharSet charSet = CharSet.Auto) {
      if (s == null) return IntPtr.Zero;
      if (GetCharSize(charSet) == 2)
        return Marshal.SecureStringToCoTaskMemUnicode(s);
      return Marshal.SecureStringToCoTaskMemAnsi(s);
    }

    /// <summary>Copies the contents of a managed <see cref="SecureString"/> object to a block of memory allocated from a supplied allocation method.</summary>
    /// <param name="s">The managed object to copy.</param>
    /// <param name="charSet">The character set.</param>
    /// <param name="memAllocator">The method used to allocate the memory.</param>
    /// <returns>The address, in unmanaged memory, where the <paramref name="s"/> parameter was copied to, or 0 if a null object was supplied.</returns>
    public static IntPtr AllocSecureString(SecureString s, CharSet charSet, Func<int, IntPtr> memAllocator) => AllocSecureString(s, charSet, memAllocator, out _);

    /// <summary>Copies the contents of a managed <see cref="SecureString"/> object to a block of memory allocated from a supplied allocation method.</summary>
    /// <param name="s">The managed object to copy.</param>
    /// <param name="charSet">The character set.</param>
    /// <param name="memAllocator">The method used to allocate the memory.</param>
    /// <param name="allocatedBytes">Returns the number of allocated bytes for the string.</param>
    /// <returns>The address, in unmanaged memory, where the <paramref name="s"/> parameter was copied to, or 0 if a null object was supplied.</returns>
    public static IntPtr AllocSecureString(SecureString s, CharSet charSet, Func<int, IntPtr> memAllocator, out int allocatedBytes) {
      allocatedBytes = 0;
      if (s == null) return IntPtr.Zero;
      var chSz = GetCharSize(charSet);
      var encoding = chSz == 2 ? System.Text.Encoding.Unicode : System.Text.Encoding.ASCII;
      var hMem = AllocSecureString(s, charSet);
      var str = chSz == 2 ? Marshal.PtrToStringUni(hMem) : Marshal.PtrToStringAnsi(hMem);
      Marshal.FreeCoTaskMem(hMem);
      if (str == null) return IntPtr.Zero;
      var b = encoding.GetBytes(str);
      var p = memAllocator(b.Length);
      Marshal.Copy(b, 0, p, b.Length);
      allocatedBytes = b.Length;
      return p;
    }

    /// <summary>Copies the contents of a managed String to a block of memory allocated from the unmanaged COM task allocator.</summary>
    /// <param name="s">A managed string to be copied.</param>
    /// <param name="charSet">The character set.</param>
    /// <returns>The allocated memory block, or 0 if <paramref name="s"/> is null.</returns>
    public static IntPtr AllocString(string s, CharSet charSet = CharSet.Auto) => charSet == CharSet.Auto ? Marshal.StringToCoTaskMemAuto(s) : (charSet == CharSet.Unicode ? Marshal.StringToCoTaskMemUni(s) : Marshal.StringToCoTaskMemAnsi(s));

    /// <summary>Copies the contents of a managed String to a block of memory allocated from a supplied allocation method.</summary>
    /// <param name="s">A managed string to be copied.</param>
    /// <param name="charSet">The character set.</param>
    /// <param name="memAllocator">The method used to allocate the memory.</param>
    /// <returns>The allocated memory block, or 0 if <paramref name="s"/> is null.</returns>
    public static IntPtr AllocString(string s, CharSet charSet, Func<int, IntPtr> memAllocator) => AllocString(s, charSet, memAllocator, out _);

    /// <summary>
    /// Copies the contents of a managed String to a block of memory allocated from a supplied allocation method.
    /// </summary>
    /// <param name="s">A managed string to be copied.</param>
    /// <param name="charSet">The character set.</param>
    /// <param name="memAllocator">The method used to allocate the memory.</param>
    /// <param name="allocatedBytes">Returns the number of allocated bytes for the string.</param>
    /// <returns>The allocated memory block, or 0 if <paramref name="s" /> is null.</returns>
    public static IntPtr AllocString(string s, CharSet charSet, Func<int, IntPtr> memAllocator, out int allocatedBytes) {
      if (s == null) { allocatedBytes = 0; return IntPtr.Zero; }
      var b = s.GetBytes(true, charSet);
      var p = memAllocator(b.Length);
      Marshal.Copy(b, 0, p, allocatedBytes = b.Length);
      return p;
    }

    /// <summary>
    /// Zeros out the allocated memory behind a secure string and then frees that memory.
    /// </summary>
    /// <param name="ptr">The address of the memory to be freed.</param>
    /// <param name="sizeInBytes">The size in bytes of the memory pointed to by <paramref name="ptr"/>.</param>
    /// <param name="memFreer">The memory freer.</param>
    public static void FreeSecureString(IntPtr ptr, int sizeInBytes, Action<IntPtr> memFreer) {
      if (IsValue(ptr)) return;
      var b = new byte[sizeInBytes];
      Marshal.Copy(b, 0, ptr, b.Length);
      memFreer(ptr);
    }

    /// <summary>Frees a block of memory allocated by the unmanaged COM task memory allocator for a string.</summary>
    /// <param name="ptr">The address of the memory to be freed.</param>
    /// <param name="charSet">The character set of the string.</param>
    public static void FreeString(IntPtr ptr, CharSet charSet = CharSet.Auto) {
      if (IsValue(ptr)) return;
      if (GetCharSize(charSet) == 2)
        Marshal.ZeroFreeCoTaskMemUnicode(ptr);
      else
        Marshal.ZeroFreeCoTaskMemAnsi(ptr);
    }

    /// <summary>Gets the encoded bytes for a string including an optional null terminator.</summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="nullTerm">if set to <c>true</c> include a null terminator at the end of the string in the resulting byte array.</param>
    /// <param name="charSet">The character set.</param>
    /// <returns>A byte array including <paramref name="value"/> encoded as per <paramref name="charSet"/> and the optional null terminator.</returns>
    public static byte[] GetBytes(this string value, bool nullTerm = true, CharSet charSet = CharSet.Auto) {
      var chSz = GetCharSize(charSet);
      var enc = chSz == 1 ? System.Text.Encoding.ASCII : System.Text.Encoding.Unicode;
      var ret = new byte[enc.GetByteCount(value) + (nullTerm ? chSz : 0)];
      enc.GetBytes(value, 0, value.Length, ret, 0);
      if (nullTerm)
        enc.GetBytes(new[] { '\0' }, 0, 1, ret, ret.Length - chSz);
      return ret;
    }

    /// <summary>Gets the number of bytes required to store the string.</summary>
    /// <param name="value">The string value.</param>
    /// <param name="nullTerm">if set to <c>true</c> include a null terminator at the end of the string in the count if <paramref name="value"/> does not equal <c>null</c>.</param>
    /// <param name="charSet">The character set.</param>
    /// <returns>The number of bytes required to store <paramref name="value"/>. Returns 0 if <paramref name="value"/> is <c>null</c>.</returns>
    public static int GetByteCount(this string value, bool nullTerm = true, CharSet charSet = CharSet.Auto) {
      if (value == null) return 0;
      var chSz = GetCharSize(charSet);
      var enc = chSz == 1 ? System.Text.Encoding.ASCII : System.Text.Encoding.Unicode;
      return enc.GetByteCount(value) + (nullTerm ? chSz : 0);
    }

    /// <summary>Gets the size of a character defined by the supplied <see cref="CharSet"/>.</summary>
    /// <param name="charSet">The character set to size.</param>
    /// <returns>The size of a standard character, in bytes, from <paramref name="charSet"/>.</returns>
    public static int GetCharSize(CharSet charSet = CharSet.Auto) => charSet == CharSet.Auto ? Marshal.SystemDefaultCharSize : (charSet == CharSet.Unicode ? 2 : 1);

    /// <summary>
    /// Allocates a managed String and copies all characters up to the first null character or the end of the allocated memory pool from a string stored in unmanaged memory into it.
    /// </summary>
    /// <param name="ptr">The address of the first character.</param>
    /// <param name="charSet">The character set of the string.</param>
    /// <param name="allocatedBytes">If known, the total number of bytes allocated to the native memory in <paramref name="ptr"/>.</param>
    /// <returns>
    /// A managed string that holds a copy of the unmanaged string if the value of the <paramref name="ptr"/> parameter is not null;
    /// otherwise, this method returns null.
    /// </returns>
    public static string GetString(IntPtr ptr, CharSet charSet = CharSet.Auto, long allocatedBytes = long.MaxValue) {
      if (IsValue(ptr)) return null;
      var sb = new System.Text.StringBuilder();
      unsafe {
        var chkLen = 0L;
        if (GetCharSize(charSet) == 1) {
          for (var uptr = (byte*)ptr; chkLen < allocatedBytes && *uptr != 0; chkLen++, uptr++)
            sb.Append((char)*uptr);
        } else {
          for (var uptr = (ushort*)ptr; chkLen + 2 <= allocatedBytes && *uptr != 0; chkLen += 2, uptr++)
            sb.Append((char)*uptr);
        }
      }
      return sb.ToString();
    }

    /// <summary>
    /// Allocates a managed String and copies all characters up to the first null character or at most <paramref name="length"/> characters from a string stored in unmanaged memory into it.
    /// </summary>
    /// <param name="ptr">The address of the first character.</param>
    /// <param name="length">The number of characters to copy.</param>
    /// <param name="charSet">The character set of the string.</param>
    /// <returns>
    /// A managed string that holds a copy of the unmanaged string if the value of the <paramref name="ptr"/> parameter is not null;
    /// otherwise, this method returns null.
    /// </returns>
    public static string GetString(IntPtr ptr, int length, CharSet charSet = CharSet.Auto) => GetString(ptr, charSet, length * GetCharSize(charSet));

    /// <summary>Indicates whether a specified string is <see langword="null"/>, empty, or consists only of white-space characters.</summary>
    /// <param name="value">The string to test.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="value"/> parameter is <see langword="null"/> or <see cref="string.Empty"/>, or if
    /// value consists exclusively of white-space characters.
    /// </returns>
    public static bool IsNullOrWhiteSpace(string value) => value is null || value.All(c => char.IsWhiteSpace(c));

    /// <summary>Refreshes the memory block from the unmanaged COM task allocator and copies the contents of a new managed String.</summary>
    /// <param name="ptr">The address of the first character.</param>
    /// <param name="charLen">Receives the new character length of the allocated memory block.</param>
    /// <param name="s">A managed string to be copied.</param>
    /// <param name="charSet">The character set of the string.</param>
    /// <returns><c>true</c> if the memory block was reallocated; <c>false</c> if set to null.</returns>
    public static bool RefreshString(ref IntPtr ptr, out uint charLen, string s, CharSet charSet = CharSet.Auto) {
      FreeString(ptr, charSet);
      ptr = AllocString(s, charSet);
      charLen = s == null ? 0U : (uint)s.Length + 1;
      return s != null;
    }

    /// <summary>Writes the specified string to a pointer to allocated memory.</summary>
    /// <param name="value">The string value.</param>
    /// <param name="ptr">The pointer to the allocated memory.</param>
    /// <param name="byteCnt">The resulting number of bytes written.</param>
    /// <param name="nullTerm">if set to <c>true</c> include a null terminator at the end of the string in the count if <paramref name="value"/> does not equal <c>null</c>.</param>
    /// <param name="charSet">The character set of the string.</param>
    /// <param name="allocatedBytes">If known, the total number of bytes allocated to the native memory in <paramref name="ptr"/>.</param>
    public static void Write(string value, IntPtr ptr, out int byteCnt, bool nullTerm = true, CharSet charSet = CharSet.Auto, long allocatedBytes = long.MaxValue) {
      if (value is null) {
        byteCnt = 0;
        return;
      }
      if (ptr == IntPtr.Zero) throw new ArgumentNullException(nameof(ptr));
      var bytes = GetBytes(value, nullTerm, charSet);
      if (bytes.Length > allocatedBytes)
        throw new ArgumentOutOfRangeException(nameof(allocatedBytes));
      byteCnt = bytes.Length;
      Marshal.Copy(bytes, 0, ptr, byteCnt);
    }

    private static bool IsValue(IntPtr ptr) => ptr.ToInt64() >> 16 == 0;
  }
}
