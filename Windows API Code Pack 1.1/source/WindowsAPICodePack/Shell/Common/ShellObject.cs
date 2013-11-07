//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Shell.Resources;
using MS.WindowsAPICodePack.Internal;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing;
using WindowsHelper;
using System.Text;
using System.Drawing.Imaging;

namespace Microsoft.WindowsAPICodePack.Shell
{
    /// <summary>
    /// The base class for all Shell objects in Shell Namespace.
    /// </summary>
    abstract public class ShellObject : IDisposable, IEquatable<ShellObject>
    {

        #region Public Static Methods

        /// <summary>
        /// Creates a ShellObject subclass given a parsing name.
        /// For file system items, this method will only accept absolute paths.
        /// </summary>
        /// <param name="parsingName">The parsing name of the object.</param>
        /// <returns>A newly constructed ShellObject object.</returns>
        public static ShellObject FromParsingName(string parsingName)
        {
            return ShellObjectFactory.Create(parsingName);
        }

        /// <summary>
        /// Indicates whether this feature is supported on the current platform.
        /// </summary>
        public static bool IsPlatformSupported
        {
            get
            {
                // We need Windows Vista onwards ...
                return CoreHelpers.RunningOnVista;
            }
        }

        #endregion

        #region Internal Fields

        /// <summary>
        /// Internal member to keep track of the native IShellItem2
        /// </summary>
        internal IShellItem2 nativeShellItem;

        #endregion

        #region Constructors

        internal ShellObject()
        {
        }

        internal ShellObject(IShellItem2 shellItem)
        {
            nativeShellItem = shellItem;
        }

        #endregion

        #region Protected Fields

        /// <summary>
        /// Parsing name for this Object e.g. c:\Windows\file.txt,
        /// or ::{Some Guid} 
        /// </summary>
        private string _internalParsingName;

        /// <summary>
        /// A friendly name for this object that' suitable for display
        /// </summary>
        private string _internalName;

        /// <summary>
        /// PID List (PIDL) for this object
        /// </summary>
        private IntPtr _internalPIDL = IntPtr.Zero;
        private IntPtr _internalPIDL2 = IntPtr.Zero;

        #endregion

        #region Internal Properties

        /// <summary>
        /// Return the native ShellFolder object as newer IShellItem2
        /// </summary>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">If the native object cannot be created.
        /// The ErrorCode member will contain the external error code.</exception>
        virtual internal IShellItem2 NativeShellItem2
        {
            get
            {
                if (nativeShellItem == null && ParsingName != null)
                {
                    Guid guid = new Guid(ShellIIDGuid.IShellItem2);
                    int retCode = ShellNativeMethods.SHCreateItemFromParsingName(ParsingName, IntPtr.Zero, ref guid, out nativeShellItem);

                    if (nativeShellItem == null || !CoreErrorHelper.Succeeded(retCode))
                    {
                        throw new ShellException(LocalizedMessages.ShellObjectCreationFailed, Marshal.GetExceptionForHR(retCode));
                    }
                }
                return nativeShellItem;
            }
        }

        /// <summary>
        /// Return the native ShellFolder object
        /// </summary>
        virtual public IShellItem NativeShellItem
        {
            get { return NativeShellItem2; }
        }

        /// <summary>
        /// Gets access to the native IPropertyStore (if one is already
        /// created for this item and still valid. This is usually done by the
        /// ShellPropertyWriter class. The reference will be set to null
        /// when the writer has been closed/commited).
        /// </summary>
        internal IPropertyStore NativePropertyStore { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the native shell item that maps to this shell object. This is necessary when the shell item 
        /// changes after the shell object has been created. Without this method call, the retrieval of properties will
        /// return stale data. 
        /// </summary>
        /// <param name="bindContext">Bind context object</param>
        public void Update(IBindCtx bindContext)
        {
            HResult hr = HResult.Ok;

            if (NativeShellItem2 != null)
            {
                hr = NativeShellItem2.Update(bindContext);
            }

            if (CoreErrorHelper.Failed(hr))
            {
                throw new ShellException(hr);
            }
        }

        public System.IO.DriveInfo GetDriveInfo()
        {

            if (IsDrive == true || IsNetDrive == true)
            {
                return new DriveInfo(ParsingName);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Public Properties

        private ShellProperties properties;
        /// <summary>
        /// Gets an object that allows the manipulation of ShellProperties for this shell item.
        /// </summary>
        public ShellProperties Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new ShellProperties(this);
                }
                return properties;
            }
        }


        /// <summary>
        /// Gets the parsing name for this ShellItem.
        /// </summary>
        virtual public string ParsingName
        {
            get
            {
                if (_internalParsingName == null && nativeShellItem != null)
                {
                    _internalParsingName = ShellHelper.GetParsingName(nativeShellItem);
                }
                return _internalParsingName ?? string.Empty;
            }
            protected set
            {
                _internalParsingName = value;
            }
        }

        /// <summary>
        /// Gets the normal display for this ShellItem.
        /// </summary>
        virtual public string Name
        {
            get
            {
                if (_internalName == null && NativeShellItem != null)
                {
                    IntPtr pszString = IntPtr.Zero;
                    HResult hr = NativeShellItem.GetDisplayName(ShellItemDesignNameOptions.Normal, out pszString);
                    if (hr == HResult.Ok && pszString != IntPtr.Zero)
                    {
                        _internalName = Marshal.PtrToStringAuto(pszString);

                        // Free the string
                        Marshal.FreeCoTaskMem(pszString);

                    }
                }
                return _internalName;
            }

            protected set
            {
                this._internalName = value;
            }
        }
        [ComImport,
       Guid("b3a4b685-b685-4805-99d9-5dead2873236"),
       InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
       ComConversionLoss]
        public interface IParentAndItem
        {
            void GetParentAndItem(
             out IntPtr ppidlParent,
            out IntPtr ppsf,
            out IntPtr ppidlChild);
        }
        /// <summary>
        /// Gets the PID List (PIDL) for this ShellItem.
        /// </summary>
        public virtual IntPtr PIDL
        {
            get
            {
                // Get teh PIDL for the ShellItem
                if (_internalPIDL == IntPtr.Zero && NativeShellItem != null)
                {
                    _internalPIDL = ShellHelper.PidlFromShellItem(NativeShellItem);
                }

                return _internalPIDL;
            }
            set
            {
                this._internalPIDL = value;
            }
        }

        public IntPtr PIDL2
        {
            get
            {
                // Get teh PIDL for the ShellItem
                if (_internalPIDL2 == IntPtr.Zero && NativeShellItem != null)
                {
                    IntPtr a = new IntPtr(), b = new IntPtr(), c = new IntPtr();
                    IParentAndItem ipi = (IParentAndItem)NativeShellItem;
                    ipi.GetParentAndItem(out a, out b, out c);
                    _internalPIDL2 = c;
                }

                return _internalPIDL2;
            }
            set
            {
                this._internalPIDL2 = value;
            }
        }


        /// <summary>
        /// Overrides object.ToString()
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Returns the display name of the ShellFolder object. DisplayNameType represents one of the 
        /// values that indicates how the name should look. 
        /// See <see cref="Microsoft.WindowsAPICodePack.Shell.DisplayNameType"/>for a list of possible values.
        /// </summary>
        /// <param name="displayNameType">A disaply name type.</param>
        /// <returns>A string.</returns>
        public virtual string GetDisplayName(DisplayNameType displayNameType)
        {
            string returnValue = null;

            HResult hr = HResult.Ok;

            if (NativeShellItem2 != null)
            {
                hr = NativeShellItem2.GetDisplayName((ShellItemDesignNameOptions)displayNameType, out returnValue);
            }

            if (hr != HResult.Ok)
            {
                throw new ShellException(LocalizedMessages.ShellObjectCannotGetDisplayName, hr);
            }

            return returnValue;
        }

        public object GetPropertyValue(PropertyKey propertyKey)
        {
          using (PropVariant propVar = new PropVariant())
          {
            if (this.NativePropertyStore != null)
            {
              // If there is a valid property store for this shell object, then use it.
              this.NativePropertyStore.GetValue(ref propertyKey, propVar);
            }
            else if (this != null)
            {
              // Use IShellItem2.GetProperty instead of creating a new property store
              // The file might be locked. This is probably quicker, and sufficient for what we need
              this.NativeShellItem2.GetProperty(ref propertyKey, propVar);
            }
            else if (NativePropertyStore != null)
            {
              NativePropertyStore.GetValue(ref propertyKey, propVar);
            }

            //Get the value
            return propVar.Value;
          }
        }

        /// <summary>
        /// Gets a value that determines if this ShellObject is a link or shortcut.
        /// </summary>
        public bool IsLink
        {
            get
            {
                try
                {
                    ShellFileGetAttributesOptions sfgao;
                    NativeShellItem.GetAttributes(ShellFileGetAttributesOptions.Link, out sfgao);
                    return (sfgao & ShellFileGetAttributesOptions.Link) != 0;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (NullReferenceException)
                {
                    // NativeShellItem is null
                    return false;
                }
            }
        }

        public bool IsRemovable
        {
            get
            {
                try
                {
                    ShellFileGetAttributesOptions sfgao;
                    NativeShellItem.GetAttributes(ShellFileGetAttributesOptions.Removable, out sfgao);
                    return (sfgao & ShellFileGetAttributesOptions.Removable) != 0;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (NullReferenceException)
                {
                    // NativeShellItem is null
                    return false;
                }
            }
        }

        public bool IsDrive
        {
            get
            {
                try
                {
                    return Directory.GetLogicalDrives().Contains(ParsingName) && 
                        WindowsHelper.WindowsAPI.GetDriveType(ParsingName) != DriveType.Network;

                }
                catch
                {
                    return false;
                }

            }
        }

        public bool IsNetworkPath
        {
          get
          {
            if (!ParsingName.StartsWith("::"))
            {
              if (!ParsingName.StartsWith(@"/") && !ParsingName.StartsWith(@"\"))
              {
                string rootPath = System.IO.Path.GetPathRoot(ParsingName); // get drive's letter
                System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(rootPath); // get info about the drive
                return driveInfo.DriveType == DriveType.Network; // return true if a network drive
              }

              return true; // is a UNC path 
            }
            else
            {
              return false;
            }
          }
        }

        public bool IsNetDrive
        {
            get
            {
                try
                {
                    return Directory.GetLogicalDrives().Contains(ParsingName) &&
                        WindowsHelper.WindowsAPI.GetDriveType(ParsingName) == DriveType.Network;

                }
                catch
                {
                    return false;
                }

            }
        }

        public bool IsSearchFolder
        {
            get
            {
                try
                {
                    
                    return (!ParsingName.StartsWith("::") && !IsFileSystemObject && !ParsingName.StartsWith(@"\\") &&
                        !ParsingName.Contains(":\\")) || ParsingName.EndsWith(".search-ms");

                }
                catch
                {
                    return false;
                }

            }
        }

        /// <summary>
        /// Gets a value that determines if this ShellObject is a file system object.
        /// </summary>
        public bool IsFileSystemObject
        {
            get
            {
                try
                {
                    ShellFileGetAttributesOptions sfgao;
                    NativeShellItem.GetAttributes(ShellFileGetAttributesOptions.FileSystem, out sfgao);
                    return (sfgao & ShellFileGetAttributesOptions.FileSystem) != 0;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (NullReferenceException)
                {
                    // NativeShellItem is null
                    return false;
                }
            }
        }
        /// <summary>
        /// Gets a value that determines if this ShellObject is a folder.
        /// </summary>
        public bool IsFolder
        {
            get
            {
                try
                {
                    ShellFileGetAttributesOptions sfgao;
                    NativeShellItem.GetAttributes(ShellFileGetAttributesOptions.Folder | ShellFileGetAttributesOptions.Stream, out sfgao);
                    return (sfgao & ShellFileGetAttributesOptions.Folder) != 0 && (sfgao & ShellFileGetAttributesOptions.Stream) == 0;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (NullReferenceException)
                {
                    // NativeShellItem is null
                    return false;
                }
            }
        }

        public bool IsBrowsable
        {
          get
          {
            try
            {
              ShellFileGetAttributesOptions sfgao;
              NativeShellItem.GetAttributes(ShellFileGetAttributesOptions.Browsable, out sfgao);
              return (sfgao & ShellFileGetAttributesOptions.Browsable) != 0;
            }
            catch (FileNotFoundException)
            {
              return false;
            }
            catch (NullReferenceException)
            {
              // NativeShellItem is null
              return false;
            }
          }
        }

        public bool IsHidden {
          get {
            try {
              ShellFileGetAttributesOptions sfgao;
              NativeShellItem.GetAttributes(ShellFileGetAttributesOptions.Hidden, out sfgao);
              return (sfgao & ShellFileGetAttributesOptions.Hidden) != 0;
            } catch (FileNotFoundException) {
              return false;
            } catch (NullReferenceException) {
              // NativeShellItem is null
              return false;
            }
          }
        }

        public bool IsShared
        {
            get
            {
                try
                {
                    ShellFileGetAttributesOptions sfgao;
                    NativeShellItem.GetAttributes(ShellFileGetAttributesOptions.Share, out sfgao);
                    return (sfgao & ShellFileGetAttributesOptions.Share) != 0;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (NullReferenceException)
                {
                    // NativeShellItem is null
                    return false;
                }
            }
        }

        public bool HasSubFolders
        {
            get
            {
                try
                {
                    ShellContainer con = (ShellContainer)this;
                    foreach (ShellObject item in con)
                    {
                        if (item.IsFolder)
                        {
                            if (!item.ParsingName.ToLower().EndsWith(".zip") && 
                                !item.ParsingName.ToLower().EndsWith(".cab"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (NullReferenceException)
                {
                    // NativeShellItem is null
                    return false;
                }
            }
        }

        private ShellThumbnail thumbnail;
        /// <summary>
        /// Gets the thumbnail of the ShellObject.
        /// </summary>
        public ShellThumbnail Thumbnail
        {
            get
            {
                if (thumbnail == null) { thumbnail = new ShellThumbnail(this); }
                return thumbnail;
            }
        }

        //public Bitmap GetShellThumbnail(int Size, bool OnlyIcon = false, bool OnlyCache = false, bool OnlyThumbnail = false)
        //{
        //  this.Thumbnail.RetrievalOption = OnlyCache ? ShellThumbnailRetrievalOption.CacheOnly : ShellThumbnailRetrievalOption.Default;

        //  this.Thumbnail.FormatOption = OnlyIcon ? ShellThumbnailFormatOption.IconOnly : ShellThumbnailFormatOption.Default;
        //  this.Thumbnail.CurrentSize = new System.Windows.Size(Size, Size);
        //  return this.Thumbnail.Bitmap;
        //}
        public Bitmap GetBitmapFromHBitmap(IntPtr nativeHBitmap)
        {
          Bitmap bmp = Bitmap.FromHbitmap(nativeHBitmap);

          if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
          {
            //bmp.MakeTransparent(Color.Black);
            return bmp;
          }

          BitmapData bmpData;

          if (IsAlphaBitmap(bmp, out bmpData))
            return GetlAlphaBitmapFromBitmapData(bmpData);

          //if (bmp.GetPixel(1, 1) == Color.FromArgb(255, Color.Black) && RetrievalOption == ShellThumbnailRetrievalOption.CacheOnly)
          //  return null;
          //bmp.MakeTransparent(Color.FromArgb(255, Color.Black));
          return bmp;
        }
        public Bitmap GetBitmapFromHBitmap(Bitmap nativeHBitmap)
        {
          Bitmap bmp = nativeHBitmap;

          if (Bitmap.GetPixelFormatSize(bmp.PixelFormat) < 32)
          {
            //bmp.MakeTransparent(Color.Black);
            return bmp;
          }

          BitmapData bmpData;

          if (IsAlphaBitmap(bmp, out bmpData))
            return GetlAlphaBitmapFromBitmapData(bmpData);

          //if (bmp.GetPixel(1, 1) == Color.FromArgb(255, Color.Black) && RetrievalOption == ShellThumbnailRetrievalOption.CacheOnly)
          //  return null;
          //bmp.MakeTransparent(Color.FromArgb(255, Color.Black));
          return bmp;
        }

        public static Bitmap GetlAlphaBitmapFromBitmapData(BitmapData bmpData)
        {
          Bitmap b = new Bitmap(
                  bmpData.Width,
                  bmpData.Height,
                  bmpData.Stride,
                  PixelFormat.Format32bppArgb,
                  bmpData.Scan0);
          return b;
        }

        public static bool IsAlphaBitmap(Bitmap bmp, out BitmapData bmpData)
        {
          Rectangle bmBounds = new Rectangle(0, 0, bmp.Width, bmp.Height);

          bmpData = bmp.LockBits(bmBounds, ImageLockMode.ReadOnly, bmp.PixelFormat);

          try
          {
            for (int y = 0; y <= bmpData.Height - 1; y++)
            {
              for (int x = 0; x <= bmpData.Width - 1; x++)
              {
                Color pixelColor = Color.FromArgb(
                    Marshal.ReadInt32(bmpData.Scan0, (bmpData.Stride * y) + (4 * x)));

                if (pixelColor.A >= 0 & pixelColor.A <= 255)
                {
                  return true;
                }
              }
            }
          }
          finally
          {
            bmp.UnlockBits(bmpData);
          }

          return false;
        }

        public Bitmap GetShellThumbnail(int Size, ShellThumbnailFormatOption format = ShellThumbnailFormatOption.Default, ShellThumbnailRetrievalOption retrieve = ShellThumbnailRetrievalOption.Default)
        {
          this.Thumbnail.RetrievalOption = retrieve;
          this.Thumbnail.FormatOption = format;
          this.Thumbnail.CurrentSize = new System.Windows.Size(Size, Size);
          return this.Thumbnail.Bitmap;
        }
        public static uint makeDWord(ushort LoWord, ushort HiWord)
        {
          return (uint)(LoWord + (HiWord << 16));
        }
        public WindowsAPI.IExtractIconpwFlags GetIconType()
        {
          try
          {
            var guid = new Guid("000214fa-0000-0000-c000-000000000046");
            object result;
            uint res = 0;
            var ishellfolder = WindowsAPI.GetIShellFolder(this.Parent);
            ishellfolder.GetUIObjectOf(IntPtr.Zero,
            (uint)1, new IntPtr[1] { WindowsAPI.ILFindLastID(this.PIDL) },
            ref guid, ref res, out result);
            var iextract = (WindowsAPI.IExtractIcon)result;
            var str = new StringBuilder(512);
            int index = -1;
            WindowsAPI.IExtractIconpwFlags flags;
            iextract.GetIconLocation(0, str, 512, out index, out flags);

            return flags;
          }
          catch (Exception)
          {

            return 0;
          }
        }

        private ShellObject parentShellObject;
        /// <summary>
        /// Gets the parent ShellObject.
        /// Returns null if the object has no parent, i.e. if this object is the Desktop folder.
        /// </summary>
        public ShellObject Parent
        {
            get
            {
                if (parentShellObject == null && NativeShellItem2 != null)
                {
                    IShellItem parentShellItem;
                    HResult hr = NativeShellItem2.GetParent(out parentShellItem);

                    if (hr == HResult.Ok && parentShellItem != null)
                    {
                        parentShellObject = ShellObjectFactory.Create(parentShellItem);
                    }
                    else if (hr == HResult.NoObject)
                    {
                        // Should return null if the parent is desktop
                        return null;
                    }
                    else
                    {
                        throw new ShellException(hr);
                    }
                }

                return parentShellObject;
            }
        }

        public static ShellObject CreateFromSpecName(string Name)
        {
            return ShellObjectFactory.Create(Name);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Release the native and managed objects
        /// </summary>
        /// <param name="disposing">Indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _internalName = null;
                _internalParsingName = null;
                properties = null;
                thumbnail = null;
                parentShellObject = null;
            }

            if (properties != null)
            {
                properties.Dispose();
            }

            if (_internalPIDL != IntPtr.Zero)
            {
                ShellNativeMethods.ILFree(_internalPIDL);
                _internalPIDL = IntPtr.Zero;
            }

            if (nativeShellItem != null)
            {
                Marshal.ReleaseComObject(nativeShellItem);
                nativeShellItem = null;
            }

            if (NativePropertyStore != null)
            {
                Marshal.ReleaseComObject(NativePropertyStore);
                NativePropertyStore = null;
            }
        }

        /// <summary>
        /// Release the native objects.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement the finalizer.
        /// </summary>
        ~ShellObject()
        {
            Dispose(false);
        }

        #endregion

        #region equality and hashing

        /// <summary>
        /// Returns the hash code of the object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (!hashValue.HasValue)
            {
                uint size = ShellNativeMethods.ILGetSize(PIDL);
                if (size != 0)
                {
                    byte[] pidlData = new byte[size];
                    Marshal.Copy(PIDL, pidlData, 0, (int)size);
                    byte[] hashData = ShellObject.hashProvider.ComputeHash(pidlData);
                    hashValue = BitConverter.ToInt32(hashData, 0);
                }
                else
                {
                    hashValue = 0;
                }

            }
            return hashValue.Value;
        }
        private static MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
        private int? hashValue;

        /// <summary>
        /// Determines if two ShellObjects are identical.
        /// </summary>
        /// <param name="other">The ShellObject to comare this one to.</param>
        /// <returns>True if the ShellObjects are equal, false otherwise.</returns>
        public bool Equals(ShellObject other)
        {
            bool areEqual = false;

            if (other != null)
            {
                IShellItem ifirst = this.NativeShellItem;
                IShellItem isecond = other.NativeShellItem;
                if (ifirst != null && isecond != null)
                {
                    int result = 0;
                    HResult hr = ifirst.Compare(
                        isecond, SICHINTF.SICHINT_ALLFIELDS, out result);

                    areEqual = (hr == HResult.Ok) && (result == 0);
                }
            }

            return areEqual;
        }

        /// <summary>
        /// Returns whether this object is equal to another.
        /// </summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>Equality result.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as ShellObject);
        }

        /// <summary>
        /// Implements the == (equality) operator.
        /// </summary>
        /// <param name="leftShellObject">First object to compare.</param>
        /// <param name="rightShellObject">Second object to compare.</param>
        /// <returns>True if leftShellObject equals rightShellObject; false otherwise.</returns>
        public static bool operator ==(ShellObject leftShellObject, ShellObject rightShellObject)
        {
            if ((object)leftShellObject == null)
            {
                return ((object)rightShellObject == null);
            }
            return leftShellObject.Equals(rightShellObject);
        }

        /// <summary>
        /// Implements the != (inequality) operator.
        /// </summary>
        /// <param name="leftShellObject">First object to compare.</param>
        /// <param name="rightShellObject">Second object to compare.</param>
        /// <returns>True if leftShellObject does not equal leftShellObject; false otherwise.</returns>        
        public static bool operator !=(ShellObject leftShellObject, ShellObject rightShellObject)
        {
            return !(leftShellObject == rightShellObject);
        }


        #endregion
    }
}
