using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BExplorer.Shell.Interop
{
	// <summary>
	/// Represents the different retrieval options for the thumbnail or icon,
	/// such as extracting the thumbnail or icon from a file, 
	/// from the cache only, or from memory only.
	/// </summary>
	public enum ShellThumbnailRetrievalOption
	{
		/// <summary>
		/// The default behavior loads a thumbnail. If there is no thumbnail for the current ShellItem,  
		/// the icon is retrieved. The thumbnail or icon is extracted if it is not currently cached.
		/// </summary>
		Default,

		/// <summary>
		/// The CacheOnly behavior returns a cached thumbnail if it is available. Allows access to the disk,
		/// but only to retrieve a cached item. If no cached thumbnail is available, a cached per-instance icon is returned but  
		/// a thumbnail or icon is not extracted.
		/// </summary>
		CacheOnly = SIIGBF.InCacheOnly,

		/// <summary>
		/// The MemoryOnly behavior returns the item only if it is in memory. The disk is not accessed even if the item is cached. 
		/// Note that this only returns an already-cached icon and can fall back to a per-class icon if 
		/// an item has a per-instance icon that has not been cached yet. Retrieving a thumbnail, 
		/// even if it is cached, always requires the disk to be accessed, so this method should not be 
		/// called from the user interface (UI) thread without passing ShellThumbnailCacheOptions.MemoryOnly.
		/// </summary>
		MemoryOnly = SIIGBF.MemoryOnly,
	}

	/// <summary>
	/// Represents the format options for the thumbnails and icons.
	/// </summary>    
	public enum ShellThumbnailFormatOption
	{
		/// <summary>
		/// The default behavior loads a thumbnail. An HBITMAP for the icon of the item is retrieved if there is no thumbnail for the current Shell Item.
		/// </summary>
		Default,

		/// <summary>
		/// The ThumbnailOnly behavior returns only the thumbnails, never the icon. Note that not all items have thumbnails 
		/// so ShellThumbnailFormatOption.ThumbnailOnly can fail in these cases.
		/// </summary>
		ThumbnailOnly = SIIGBF.ThumbnailOnly,

		/// <summary>
		/// The IconOnly behavior returns only the icon, never the thumbnail.
		/// </summary>
		IconOnly = SIIGBF.IconOnly,
	}
	/// <summary>
	/// Represents a thumbnail or an icon for a ShellObject.
	/// </summary>
	public class ShellThumbnail : IDisposable
	{
		#region Private members

		/// <summary>
		/// Native shellItem
		/// </summary>
		private IShellItem shellItemNative;

		/// <summary>
		/// Internal member to keep track of the current size
		/// </summary>
		private System.Windows.Size currentSize = new System.Windows.Size(256, 256);

		#endregion

		#region Constructors

		public void Dispose()
		{
			if (shellItemNative != null)
			{
				Marshal.FinalReleaseComObject(shellItemNative);
				shellItemNative = null;
			}
		}
		/// <summary>
		/// Internal constructor that takes in a parent ShellObject.
		/// </summary>
		/// <param name="shellObject"></param>
		internal ShellThumbnail(ShellItem shellObject)
		{
			if (shellObject != null && shellObject.m_ComInterface != null)
			{
				shellItemNative = shellObject.m_ComInterface;
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets the default size of the thumbnail or icon. The default is 32x32 pixels for icons and 
		/// 256x256 pixels for thumbnails.
		/// </summary>
		/// <remarks>If the size specified is larger than the maximum size of 1024x1024 for thumbnails and 256x256 for icons,
		/// an <see cref="System.ArgumentOutOfRangeException"/> is thrown.
		/// </remarks>
		public System.Windows.Size CurrentSize
		{
			get { return currentSize; }
			set
			{
				// Check for 0; negative number check not required as System.Windows.Size only allows positive numbers.
				if (value.Height == 0 || value.Width == 0)
				{
					throw new System.ArgumentOutOfRangeException("value", "Thumbnail is null");
				}

				System.Windows.Size size = (FormatOption == ShellThumbnailFormatOption.IconOnly) ?
						DefaultIconSize.Maximum : DefaultThumbnailSize.Maximum;

				if (value.Height > size.Height || value.Width > size.Width)
				{
					throw new System.ArgumentOutOfRangeException("value", "Wrong Size!");
				}

				currentSize = value;
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon image in <see cref="System.Drawing.Bitmap"/> format.
		/// Null is returned if the ShellObject does not have a thumbnail or icon image.
		/// </summary>
		public Bitmap Bitmap { get { return GetBitmap(CurrentSize); } }

		/// <summary>
		/// Gets the thumbnail or icon image in <see cref="System.Windows.Media.Imaging.BitmapSource"/> format. 
		/// Null is returned if the ShellObject does not have a thumbnail or icon image.
		/// </summary>
		public BitmapSource BitmapSource { get { return GetBitmapSource(CurrentSize); } }

		/// <summary>
		/// Gets the thumbnail or icon image in <see cref="System.Drawing.Icon"/> format. 
		/// Null is returned if the ShellObject does not have a thumbnail or icon image.
		/// </summary>
		public Icon Icon
		{
			get
			{
				try
				{
					return Icon.FromHandle(Bitmap.GetHicon());
				}
				catch
				{

					return null;
				}
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in small size and <see cref="System.Drawing.Bitmap"/> format.
		/// </summary>
		public Bitmap SmallBitmap
		{
			get
			{
				return GetBitmap(DefaultIconSize.Small, DefaultThumbnailSize.Small);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in small size and <see cref="System.Windows.Media.Imaging.BitmapSource"/> format.
		/// </summary>
		public BitmapSource SmallBitmapSource
		{
			get
			{
				return GetBitmapSource(DefaultIconSize.Small, DefaultThumbnailSize.Small);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in small size and <see cref="System.Drawing.Icon"/> format.
		/// </summary>
		public Icon SmallIcon { get { return Icon.FromHandle(SmallBitmap.GetHicon()); } }

		/// <summary>
		/// Gets the thumbnail or icon in Medium size and <see cref="System.Drawing.Bitmap"/> format.
		/// </summary>
		public Bitmap MediumBitmap
		{
			get
			{
				return GetBitmap(DefaultIconSize.Medium, DefaultThumbnailSize.Medium);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in medium size and <see cref="System.Windows.Media.Imaging.BitmapSource"/> format.
		/// </summary>
		public BitmapSource MediumBitmapSource
		{
			get
			{
				return GetBitmapSource(DefaultIconSize.Medium, DefaultThumbnailSize.Medium);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in Medium size and <see cref="System.Drawing.Icon"/> format.
		/// </summary>
		public Icon MediumIcon { get { return Icon.FromHandle(MediumBitmap.GetHicon()); } }

		/// <summary>
		/// Gets the thumbnail or icon in large size and <see cref="System.Drawing.Bitmap"/> format.
		/// </summary>
		public Bitmap LargeBitmap
		{
			get
			{
				return GetBitmap(DefaultIconSize.Large, DefaultThumbnailSize.Large);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in large size and <see cref="System.Windows.Media.Imaging.BitmapSource"/> format.
		/// </summary>
		public BitmapSource LargeBitmapSource
		{
			get
			{
				return GetBitmapSource(DefaultIconSize.Large, DefaultThumbnailSize.Large);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in Large size and <see cref="System.Drawing.Icon"/> format.
		/// </summary>
		public Icon LargeIcon { get { return Icon.FromHandle(LargeBitmap.GetHicon()); } }

		/// <summary>
		/// Gets the thumbnail or icon in extra large size and <see cref="System.Drawing.Bitmap"/> format.
		/// </summary>
		public Bitmap ExtraLargeBitmap
		{
			get
			{
				return GetBitmap(DefaultIconSize.ExtraLarge, DefaultThumbnailSize.ExtraLarge);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in Extra Large size and <see cref="System.Windows.Media.Imaging.BitmapSource"/> format.
		/// </summary>
		public BitmapSource ExtraLargeBitmapSource
		{
			get
			{
				return GetBitmapSource(DefaultIconSize.ExtraLarge, DefaultThumbnailSize.ExtraLarge);
			}
		}

		/// <summary>
		/// Gets the thumbnail or icon in Extra Large size and <see cref="System.Drawing.Icon"/> format.
		/// </summary>
		public Icon ExtraLargeIcon { get { return Icon.FromHandle(ExtraLargeBitmap.GetHicon()); } }

		/// <summary>
		/// Gets or sets a value that determines if the current retrieval option is cache or extract, cache only, or from memory only.
		/// The default is cache or extract.
		/// </summary>
		public ShellThumbnailRetrievalOption RetrievalOption { get; set; }

		private ShellThumbnailFormatOption formatOption = ShellThumbnailFormatOption.Default;
		/// <summary>
		/// Gets or sets a value that determines if the current format option is thumbnail or icon, thumbnail only, or icon only.
		/// The default is thumbnail or icon.
		/// </summary>
		public ShellThumbnailFormatOption FormatOption
		{
			get { return formatOption; }
			set
			{
				formatOption = value;

				// Do a similar check as we did in CurrentSize property setter,
				// If our mode is IconOnly, then our max is defined by DefaultIconSize.Maximum. We should make sure 
				// our CurrentSize is within this max range
				if (FormatOption == ShellThumbnailFormatOption.IconOnly
						&& (CurrentSize.Height > DefaultIconSize.Maximum.Height || CurrentSize.Width > DefaultIconSize.Maximum.Width))
				{
					CurrentSize = DefaultIconSize.Maximum;
				}
			}

		}

		/// <summary>
		/// Gets or sets a value that determines if the user can manually stretch the returned image.
		/// The default value is false.
		/// </summary>
		/// <remarks>
		/// For example, if the caller passes in 80x80 a 96x96 thumbnail could be returned. 
		/// This could be used as a performance optimization if the caller will need to stretch 
		/// the image themselves anyway. Note that the Shell implementation performs a GDI stretch blit. 
		/// If the caller wants a higher quality image stretch, they should pass this flag and do it themselves.
		/// </remarks>
		public bool AllowBiggerSize { get; set; }

		#endregion

		#region Private Methods

		private SIIGBF CalculateFlags()
		{
			SIIGBF flags = 0x0000;

			if (AllowBiggerSize)
			{
				flags |= SIIGBF.BiggerSizeOk;
			}

			if (RetrievalOption == ShellThumbnailRetrievalOption.CacheOnly)
			{
				flags |= SIIGBF.InCacheOnly;
			}
			else if (RetrievalOption == ShellThumbnailRetrievalOption.MemoryOnly)
			{
				flags |= SIIGBF.MemoryOnly;
			}

			if (FormatOption == ShellThumbnailFormatOption.IconOnly)
			{
				flags |= SIIGBF.IconOnly;
			}
			else if (FormatOption == ShellThumbnailFormatOption.ThumbnailOnly)
			{
				flags |= SIIGBF.ThumbnailOnly;
			}

			return flags;
		}

		private IntPtr GetHBitmap(System.Windows.Size size)
		{
			IntPtr hbitmap = IntPtr.Zero;

			if (shellItemNative == null)
				return IntPtr.Zero;
			// Create a size structure to pass to the native method
			Size nativeSIZE = new Size();
			nativeSIZE.Width = Convert.ToInt32(size.Width);
			nativeSIZE.Height = Convert.ToInt32(size.Height);

			// Use IShellItemImageFactory to get an icon
			// Options passed in: Resize to fit
			HResult hr = ((IShellItemImageFactory)shellItemNative).GetImage(nativeSIZE, CalculateFlags(), out hbitmap);

			if (hr == HResult.S_OK) { return hbitmap; }
			else
			{
				return IntPtr.Zero;
			}
			//else if ((uint)hr == 0x8004B200 && FormatOption == ShellThumbnailFormatOption.ThumbnailOnly)
			//{
			//    // Thumbnail was requested, but this ShellItem doesn't have a thumbnail.
			//    throw new InvalidOperationException(LocalizedMessages.ShellThumbnailDoesNotHaveThumbnail, Marshal.GetExceptionForHR((int)hr));
			//}
			//else if ((uint)hr == 0x80040154) // REGDB_E_CLASSNOTREG
			//{
			//    throw new NotSupportedException(LocalizedMessages.ShellThumbnailNoHandler, Marshal.GetExceptionForHR((int)hr));
			//}

			//throw new ShellException(hr);
		}

		public static Bitmap CopyHBitmapToBitmap(IntPtr nativeHBitmap)
		{
			// Get width, height and the address of the pixel data for the native HBitmap
			BITMAP bitmapStruct = new BITMAP();
			Gdi32.GetObjectBitmap(nativeHBitmap, Marshal.SizeOf(bitmapStruct), ref bitmapStruct);

			// Create a managed bitmap that has its pixel data pointing to the pixel data of the native HBitmap
			// No memory is allocated for its pixel data
			Bitmap managedBitmapPointer = new Bitmap(
					bitmapStruct.bmWidth, bitmapStruct.bmHeight, bitmapStruct.bmWidth * 4, PixelFormat.Format32bppArgb, bitmapStruct.bmBits);

			// Create a managed bitmap and allocate memory for pixel data
			Bitmap managedBitmapReal = new Bitmap(bitmapStruct.bmWidth, bitmapStruct.bmHeight, PixelFormat.Format32bppArgb);

			// Copy the pixels of the native HBitmap into the canvas of the managed bitmap
			Graphics graphics = Graphics.FromImage(managedBitmapReal);
			graphics.DrawImage(managedBitmapPointer, 0, 0);
			graphics.Dispose();
			// Delete the native HBitmap object and free memory
			Gdi32.DeleteObject(nativeHBitmap);

			// Return the managed bitmap, clone of the native HBitmap, with correct transparency
			return managedBitmapReal;
		}

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
			{
				Bitmap resBmp = GetlAlphaBitmapFromBitmapData(bmpData);
				//bmp.Dispose();
				bmpData = null;
				return resBmp;
			}
			bmpData = null;
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

		private Bitmap GetBitmap(System.Windows.Size iconOnlySize, System.Windows.Size thumbnailSize)
		{
			return GetBitmap(FormatOption == ShellThumbnailFormatOption.IconOnly ? iconOnlySize : thumbnailSize);
		}

		private Bitmap GetBitmap(System.Windows.Size size)
		{
			IntPtr hBitmap = GetHBitmap(size);

			// return a System.Drawing.Bitmap from the hBitmap
			Bitmap returnValue = null;
			if (hBitmap != IntPtr.Zero)
				returnValue = GetBitmapFromHBitmap(hBitmap);

			// delete HBitmap to avoid memory leaks
			Gdi32.DeleteObject(hBitmap);

			return returnValue;
		}

		private BitmapSource GetBitmapSource(System.Windows.Size iconOnlySize, System.Windows.Size thumbnailSize)
		{
			if (thumbnailSize == DefaultThumbnailSize.Small)
				FormatOption = ShellThumbnailFormatOption.IconOnly;
			return GetBitmapSource(FormatOption == ShellThumbnailFormatOption.IconOnly ? iconOnlySize : thumbnailSize);
		}

		private BitmapSource GetBitmapSource(System.Windows.Size size)
		{
			//FIXME: fix the cache retrieval options
			RetrievalOption = ShellThumbnailRetrievalOption.Default;
			IntPtr hBitmap = GetHBitmap(size);

			// return a System.Media.Imaging.BitmapSource
			// Use interop to create a BitmapSource from hBitmap.
			if (hBitmap != IntPtr.Zero)
			{
				BitmapSource returnValue = Imaging.CreateBitmapSourceFromHBitmap(
						hBitmap,
						IntPtr.Zero,
						System.Windows.Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions()).Clone();

				// delete HBitmap to avoid memory leaks
				Gdi32.DeleteObject(hBitmap);
				return returnValue;
			}

			return null;
		}

		#endregion

	}
}
