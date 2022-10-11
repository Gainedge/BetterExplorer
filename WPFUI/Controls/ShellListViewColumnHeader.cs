using System;
using System.DirectoryServices;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using WPFUI.Win32;
using Cursor = System.Windows.Input.Cursor;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MouseEventHandler = System.Windows.Input.MouseEventHandler;

namespace WPFUI.Controls {
  [TemplatePart(Name = "PART_ColGripper", Type = typeof(Thumb))]
  public class ShellListViewColumnHeader : GridViewColumnHeader {
    public static readonly DependencyProperty IndexProperty =
      DependencyProperty.Register(
        name: "Index",
        propertyType: typeof(Int32),
        ownerType: typeof(ShellListViewColumnHeader),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0)
      );
    public static readonly DependencyProperty IsSortedProperty =
      DependencyProperty.Register(
        name: "IsSorted",
        propertyType: typeof(Boolean),
        ownerType: typeof(ShellListViewColumnHeader),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: false)
      );
    public static readonly DependencyProperty SortDirectionProperty =
      DependencyProperty.Register(
        name: "SortDirection",
        propertyType: typeof(SortOrder),
        ownerType: typeof(ShellListViewColumnHeader),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: SortOrder.Ascending)
      );
    public Int32 Index {
      get => (Int32)GetValue(IndexProperty);
      set => SetValue(IndexProperty, value);
    }

    public Collumns Collumn { get; set; }

    public Boolean IsSorted {
      get => (Boolean)GetValue(IsSortedProperty);
      set => SetValue(IsSortedProperty, value);
    }
    public SortOrder SortDirection {
      get => (SortOrder)GetValue(SortDirectionProperty);
      set => SetValue(SortDirectionProperty, value);
    }
    private Thumb _headerGripper;

    private double _originalWidth;

    private Cursor SplitCursor {
      get {
        if (_splitCursorCache == null) {
          _splitCursorCache = GetCursor(c_SPLIT);
        }
        return _splitCursorCache;
      }
    }

    private static Cursor _splitCursorCache = null;


    private Cursor SplitOpenCursor {
      get {
        if (_splitOpenCursorCache == null) {
          _splitOpenCursorCache = GetCursor(c_SPLITOPEN);
        }
        return _splitOpenCursorCache;
      }
    }

    private static Cursor _splitOpenCursorCache = null;

    private double ColumnActualWidth => (Column != null ? Column.ActualWidth : ActualWidth);

    private GridViewColumnHeader _srcHeader;

    // cursor id in embedded win32 resource
    private const int c_SPLIT = 100;
    private const int c_SPLITOPEN = 101;
    public ShellListViewColumnHeader() {
      
    }
    static ShellListViewColumnHeader() {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(ShellListViewColumnHeader), new FrameworkPropertyMetadata(typeof(ShellListViewColumnHeader)));
    }

    public void SetColumnsHeaderResizableState(Boolean isEnabled) {
      if (this._headerGripper != null) {
        this._headerGripper.IsEnabled = isEnabled;
      }
    }
    public override void OnApplyTemplate() {
      base.OnApplyTemplate();

      HookupGripperEvents();

    }
    private void HookupGripperEvents() {
      UnhookGripperEvents();

      _headerGripper = GetTemplateChild("PART_ColGripper") as Thumb;

      if (_headerGripper != null) {
        _headerGripper.DragStarted += new DragStartedEventHandler(OnColumnHeaderGripperDragStarted);
        _headerGripper.DragDelta += new DragDeltaEventHandler(OnColumnHeaderResize);
        _headerGripper.DragCompleted += new DragCompletedEventHandler(OnColumnHeaderGripperDragCompleted);
        _headerGripper.MouseDoubleClick += new MouseButtonEventHandler(OnGripperDoubleClicked);
        _headerGripper.MouseEnter += new MouseEventHandler(OnGripperMouseEnterLeave);
        _headerGripper.MouseLeave += new MouseEventHandler(OnGripperMouseEnterLeave);

        _headerGripper.Cursor = SplitCursor;
      }
    }
    private void MakeParentGotFocus() {
      GridViewHeaderRowPresenter headerRP = this.Parent as GridViewHeaderRowPresenter;
      if (headerRP != null) {
        headerRP.Focus();
      }
    }

    /// <summary>
    /// Clear gripper event
    /// </summary>
    private void UnhookGripperEvents() {
      if (_headerGripper != null) {
        _headerGripper.DragStarted -= new DragStartedEventHandler(OnColumnHeaderGripperDragStarted);
        _headerGripper.DragDelta -= new DragDeltaEventHandler(OnColumnHeaderResize);
        _headerGripper.DragCompleted -= new DragCompletedEventHandler(OnColumnHeaderGripperDragCompleted);
        _headerGripper.MouseDoubleClick -= new MouseButtonEventHandler(OnGripperDoubleClicked);
        _headerGripper.MouseEnter -= new MouseEventHandler(OnGripperMouseEnterLeave);
        _headerGripper.MouseLeave -= new MouseEventHandler(OnGripperMouseEnterLeave);
        _headerGripper = null;
      }
    }


    /// <SecurityNote>
    /// Critical - Asserts permissions required to call Cursor constructor in partial trust
    /// TreatAsSafe - Can only be used to create one of two specific cursors (which are embedded resources within assembly). 
    /// The following Permissions are required to invoke Cursor.LoadFromStream method which writes stream to a temporary file and loads the Cursor from that file.
    /// The Environment permission is safe because even if the caller sets the %TEMP% variable to a critical location 
    /// before executing the method, the caller does not choose the filename that is written to. 
    /// Additionally the temp filename algorithm tries to avoid name collisions. 
    /// Therefore it is reasonably unlikely that the caller can use this method to overwrite a critical file.
    /// The FileIO write permission is safe because from the above justification the file being written to is reasonably safe.
    /// The Unmanaged code permission is safe because it is used for a safe p-invoke to load a cursor from a file 
    /// (the bytes read are never exposed to the caller)
    /// </SecurityNote>
    [SecurityCritical, SecurityTreatAsSafe]
    private Cursor GetCursor(int cursorID) {

      Cursor cursor = null;
      System.IO.Stream stream = null;
      System.Reflection.Assembly assembly = this.GetType().BaseType.Assembly;

      if (cursorID == c_SPLIT) {
        stream = assembly.GetManifestResourceStream("split.cur");
      } else if (cursorID == c_SPLITOPEN) {
        stream = assembly.GetManifestResourceStream("splitopen.cur");
      }

      if (stream != null) {
        PermissionSet permissions = new PermissionSet(null);

        FileIOPermission filePermission = new FileIOPermission(PermissionState.None);
        filePermission.AllLocalFiles = FileIOPermissionAccess.Write;
        permissions.AddPermission(filePermission);

        permissions.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
        permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
        permissions.Assert();

        try {
          cursor = new Cursor(stream);
        } finally {
          CodeAccessPermission.RevertAssert();
        }
      }

      return cursor;
    }

    private void UpdateGripperCursor() {
      if (_headerGripper != null && !_headerGripper.IsDragging) {
        Cursor gripperCursor;

        if (ActualWidth == 0D) {
          gripperCursor = SplitOpenCursor;
        } else {
          gripperCursor = SplitCursor;
        }

        if (gripperCursor != null) {
          _headerGripper.Cursor = gripperCursor;
        }
      }
    }

    // Set column header width and associated column width
    private void UpdateColumnHeaderWidth(double width) {
      if (Column != null) {
        Column.Width = width;
      } else {
        Width = width;
      }
    }

    private bool HandleIsMouseOverChanged() {
      if (ClickMode == ClickMode.Hover) {
        if (IsMouseOver &&
            //1) Gripper doesn't exist; 2) Gripper exists and Mouse isn't on Gripper;
            (_headerGripper == null || !_headerGripper.IsMouseOver)) {
          // Hovering over the button will click in the OnHover click mode
          SetValue(Button.IsPressedProperty, true);
          OnClick();
        } else {
          ClearValue(Button.IsPressedProperty);
        }
        return true;
      }
      return false;
    }

    private void OnColumnHeaderResize(object sender, DragDeltaEventArgs e) {
      double width = ColumnActualWidth + e.HorizontalChange;
      if (width <= 0.0D) {
        width = 0.0;
      }

      UpdateColumnHeaderWidth(width);
      e.Handled = true;
    }
    private void OnColumnHeaderGripperDragCompleted(object sender, DragCompletedEventArgs e) {
      if (e.Canceled) {
        // restore to original width
        UpdateColumnHeaderWidth(_originalWidth);
      }

      UpdateGripperCursor();
      e.Handled = true;
    }
    private void OnColumnHeaderGripperDragStarted(object sender, DragStartedEventArgs e) {
      MakeParentGotFocus();
      _originalWidth = ColumnActualWidth;
      e.Handled = true;
    }
    private void OnGripperDoubleClicked(object sender, MouseButtonEventArgs e) {
      if (Column != null) {
        if (Double.IsNaN(Column.Width)) {
          // force update will be triggered
          Column.Width = Column.ActualWidth;
        }

        Column.Width = Double.NaN;

        e.Handled = true;
      }
    }

    private void OnGripperMouseEnterLeave(object sender, MouseEventArgs e) {
      HandleIsMouseOverChanged();
    }

  }
}
