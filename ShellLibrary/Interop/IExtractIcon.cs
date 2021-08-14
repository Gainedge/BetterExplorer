using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	[ComImport()]
	[Guid("000214fa-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	//http://msdn.microsoft.com/en-us/library/windows/desktop/bb761852(v=vs.85).aspx
	public interface IExtractIcon
	{
		/// <summary>
		/// Gets the location and index of an icon.
		/// </summary>
		/// <param name="uFlags">One or more of the following values. This parameter can also be NULL.use GIL_ Consts</param>
		/// <param name="szIconFile">A pointer to a buffer that receives the icon location. The icon location is a null-terminated string that identifies the file that contains the icon.</param>
		/// <param name="cchMax">The size of the buffer, in characters, pointed to by pszIconFile.</param>
		/// <param name="piIndex">A pointer to an int that receives the index of the icon in the file pointed to by pszIconFile.</param>
		/// <param name="pwFlags">A pointer to a UINT value that receives zero or a combination of the following value</param>
		/// <returns></returns>
		///
		[PreserveSig]
		int GetIconLocation(IExtractIconUFlags uFlags, [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder szIconFile, int cchMax, out int piIndex, out IExtractIconPWFlags pwFlags);

		/// <summary>
		/// Extracts an icon image from the specified location.
		/// </summary>
		/// <param name="pszFile">A pointer to a null-terminated string that specifies the icon location.</param>
		/// <param name="nIconIndex">The index of the icon in the file pointed to by pszFile.</param>
		/// <param name="phiconLarge">A pointer to an HICON value that receives the handle to the large icon. This parameter may be NULL.</param>
		/// <param name="phiconSmall">A pointer to an HICON value that receives the handle to the small icon. This parameter may be NULL.</param>
		/// <param name="nIconSize">The desired size of the icon, in pixels. The low word contains the size of the large icon, and the high word contains the size of the small icon. The size specified can be the width or height. The width of an icon always equals its height.</param>
		/// <returns>
		/// Returns S_OK if the function extracted the icon, or S_FALSE if the calling application should extract the icon.
		/// </returns>
		[PreserveSig]
		int Extract([MarshalAs(UnmanagedType.LPWStr)] string pszFile, uint nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIconSize);
	}

	[Flags()]
	public enum IExtractIconUFlags : uint
	{
		GIL_ASYNC = 0x0020,
		GIL_DEFAULTICON = 0x0040,
		GIL_FORSHELL = 0x0002,
		GIL_FORSHORTCUT = 0x0080,
		GIL_OPENICON = 0x0001,
		GIL_CHECKSHIELD = 0x0200
	}

	[Flags()]
	public enum IExtractIconPWFlags : uint
	{
		GIL_DONTCACHE = 0x0010,
		GIL_NOTFILENAME = 0x0008,
		GIL_PERCLASS = 0x0004,
		GIL_PERINSTANCE = 0x0002,
		GIL_SIMULATEDOC = 0x0001,
		GIL_SHIELD = 0x0200,//Windows Vista only
		GIL_FORCENOSHIELD = 0x0400//Windows Vista only
	}
}
