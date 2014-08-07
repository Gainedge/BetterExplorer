using Microsoft.WindowsAPICodePack.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsAPICodePack.Shell
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct FilterSpec
    {
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string Name;
      [MarshalAs(UnmanagedType.LPWStr)]
      internal string Spec;

      internal FilterSpec(string name, string spec)
      {
        Name = name;
        Spec = spec;
      }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct ThumbnailId
    {
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 16)]
      byte rgbKey;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CMINVOKECOMMANDINFO
    {
      public int cbSize;
      public int fMask;
      public IntPtr hwnd;
      public string lpVerb;
      public string lpParameters;
      public string lpDirectory;
      public int nShow;
      public int dwHotKey;
      public IntPtr hIcon;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CMINVOKECOMMANDINFO_ByIndex
    {
      public int cbSize;
      public int fMask;
      public IntPtr hwnd;
      public int iVerb;
      public string lpParameters;
      public string lpDirectory;
      public int nShow;
      public int dwHotKey;
      public IntPtr hIcon;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CMINVOKECOMMANDINFOEX
    {
      public int Size;

      public CMIC Mask;

      public IntPtr Hwnd;

      public IntPtr Verb;

      [MarshalAs(UnmanagedType.LPStr)]
      public string Parameters;

      [MarshalAs(UnmanagedType.LPStr)]
      public string Directory;

      public SW ShowType;

      public int HotKey;

      public IntPtr hIcon;

      [MarshalAs(UnmanagedType.LPStr)]
      public string Title;

      public IntPtr VerbW;

      [MarshalAs(UnmanagedType.LPWStr)]
      public string ParametersW;

      [MarshalAs(UnmanagedType.LPWStr)]
      public string DirectoryW;

      [MarshalAs(UnmanagedType.LPWStr)]
      public string TitleW;

      public BExplorer.Shell.Interop.POINT InvokePoint;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MENUINFO
    {
      public int cbSize;
      public MIM fMask;
      public int dwStyle;
      public int cyMax;
      public IntPtr hbrBack;
      public int dwContextHelpID;
      public int dwMenuData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENUITEMINFO
    {
      public uint cbSize;
      public uint fMask;
      public uint fType;
      public uint fState;
      public uint wID;
      public IntPtr hSubMenu;
      public IntPtr hbmpChecked;
      public IntPtr hbmpUnchecked;
      public IntPtr dwItemData;
      public string dwTypeData;
      public uint cch;
      public IntPtr hbmpItem;

      // return the size of the structure
      public static uint sizeOf
      {
        get { return (uint)Marshal.SizeOf(typeof(MENUITEMINFO)); }
      }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct TVITEMW
    {
      public TVIF mask;
      public IntPtr hItem;
      public TVIS state;
      public TVIS stateMask;
      public string pszText;
      public int cchTextMax;
      public int iImage;
      public int iSelectedImage;
      public int cChildren;
      public int lParam;
    }

    /// <summary>
    /// The STGM constants are flags that indicate 
    /// conditions for creating and deleting the object and access modes 
    /// for the object. 
    /// 
    /// You can combine these flags, but you can only choose one flag 
    /// from each group of related flags. Typically one flag from each 
    /// of the access and sharing groups must be specified for all 
    /// functions and methods which use these constants. 
    /// </summary>
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification="Follows native api.")]
    public enum AccessModes
    {
        /// <summary>
        /// Indicates that, in direct mode, each change to a storage 
        /// or stream element is written as it occurs.
        /// </summary>
        Direct = 0x00000000,

        /// <summary>
        /// Indicates that, in transacted mode, changes are buffered 
        /// and written only if an explicit commit operation is called. 
        /// </summary>
        Transacted = 0x00010000,

        /// <summary>
        /// Provides a faster implementation of a compound file 
        /// in a limited, but frequently used, case. 
        /// </summary>
        Simple = 0x08000000,

        /// <summary>
        /// Indicates that the object is read-only, 
        /// meaning that modifications cannot be made.
        /// </summary>
        Read = 0x00000000,

        /// <summary>
        /// Enables you to save changes to the object, 
        /// but does not permit access to its data. 
        /// </summary>
        Write = 0x00000001,

        /// <summary>
        /// Enables access and modification of object data.
        /// </summary>
        ReadWrite = 0x00000002,

        /// <summary>
        /// Specifies that subsequent openings of the object are 
        /// not denied read or write access. 
        /// </summary>
        ShareDenyNone = 0x00000040,

        /// <summary>
        /// Prevents others from subsequently opening the object in Read mode. 
        /// </summary>
        ShareDenyRead = 0x00000030,

        /// <summary>
        /// Prevents others from subsequently opening the object 
        /// for Write or ReadWrite access.
        /// </summary>
        ShareDenyWrite = 0x00000020,

        /// <summary>
        /// Prevents others from subsequently opening the object in any mode. 
        /// </summary>
        ShareExclusive = 0x00000010,

        /// <summary>
        /// Opens the storage object with exclusive access to the most 
        /// recently committed version.
        /// </summary>
        Priority = 0x00040000,

        /// <summary>
        /// Indicates that the underlying file is to be automatically destroyed when the root 
        /// storage object is released. This feature is most useful for creating temporary files. 
        /// </summary>
        DeleteOnRelease = 0x04000000,

        /// <summary>
        /// Indicates that, in transacted mode, a temporary scratch file is usually used 
        /// to save modifications until the Commit method is called. 
        /// Specifying NoScratch permits the unused portion of the original file 
        /// to be used as work space instead of creating a new file for that purpose. 
        /// </summary>
        NoScratch = 0x00100000,

        /// <summary>
        /// Indicates that an existing storage object 
        /// or stream should be removed before the new object replaces it. 
        /// </summary>
        Create = 0x00001000,

        /// <summary>
        /// Creates the new object while preserving existing data in a stream named "Contents". 
        /// </summary>
        Convert = 0x00020000,

        /// <summary>
        /// Causes the create operation to fail if an existing object with the specified name exists.
        /// </summary>
        FailIfThere = 0x00000000,

        /// <summary>
        /// This flag is used when opening a storage object with Transacted 
        /// and without ShareExclusive or ShareDenyWrite. 
        /// In this case, specifying NoSnapshot prevents the system-provided 
        /// implementation from creating a snapshot copy of the file. 
        /// Instead, changes to the file are written to the end of the file. 
        /// </summary>
        NoSnapshot = 0x00200000,

        /// <summary>
        /// Supports direct mode for single-writer, multireader file operations. 
        /// </summary>
        DirectSingleWriterMultipleReader = 0x00400000
    };
}
