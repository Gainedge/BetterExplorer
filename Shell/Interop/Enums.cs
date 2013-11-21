using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GongSolutions.Shell.Interop
{
	[Serializable]
	public struct PROPERTYKEY
	{
		public Guid fmtid;
		public uint pid;
	}

	[Flags]
	public enum SIIGBF
	{
		ResizeToFit = 0x00,
		BiggerSizeOk = 0x01,
		MemoryOnly = 0x02,
		IconOnly = 0x04,
		ThumbnailOnly = 0x08,
		InCacheOnly = 0x10,
	}

	public struct Size
	{

		public int Height { get; set; }
		public int Width { get; set; }
	}

	[Flags]
	public enum ThumbnailOptions
	{
		Extract = 0x00000000,
		InCacheOnly = 0x00000001,
		FastExtract = 0x00000002,
		ForceExtraction = 0x00000004,
		SlowReclaim = 0x00000008,
		ExtractDoNotCache = 0x00000020
	}

	[Flags]
	public enum ThumbnailCacheOptions
	{
		Default = 0x00000000,
		LowQuality = 0x00000001,
		Cached = 0x00000002,
	}

	/// <summary>
	/// Thumbnail Alpha Types
	/// </summary>
	public enum ThumbnailAlphaType
	{
		/// <summary>
		/// Let the system decide.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// No transparency
		/// </summary>
		NoAlphaChannel = 1,

		/// <summary>
		/// Has transparency
		/// </summary>
		HasAlphaChannel = 2,
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct ThumbnailId
	{
		[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 16)]
		byte rgbKey;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BITMAP
	{
		public int bmType;
		public int bmWidth;
		public int bmHeight;
		public int bmWidthBytes;
		public ushort bmPlanes;
		public ushort bmBitsPixel;
		public IntPtr bmBits;
	}

	public enum SICHINTF
	{
		SICHINT_DISPLAY = 0x00000000,
		SICHINT_CANONICAL = 0x10000000,
		SICHINT_TEST_FILESYSPATH_IF_NOT_EQUAL = 0x20000000,
		SICHINT_ALLFIELDS = unchecked((int)0x80000000)
	}
}
