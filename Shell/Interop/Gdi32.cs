using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GongSolutions.Shell.Interop
{
	public static class Gdi32
	{
		[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "GetObject")]
		public static extern int GetObjectBitmap(IntPtr hObject, int nCount, ref BITMAP lpObject);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject(IntPtr hObject);
	}
}
