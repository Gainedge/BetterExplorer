using System;
using System.Runtime.InteropServices;

namespace BExplorer.Shell.Interop
{
	public static class UxTheme
	{
		[DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
		public static extern int SetWindowTheme(IntPtr hWnd, String pszSubAppName, int pszSubIdList);
	}
}
