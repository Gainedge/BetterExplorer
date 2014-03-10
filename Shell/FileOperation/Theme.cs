using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BExplorer.Shell
{
	static public class Theme
	{
		public static bool IsWin7
		{
			get { return Name == "Windows 7"; }
		}

		public static string Name { get; set; }
		public static ResizeMode ResizeMode
		{
			get { return IsWin7 ? ResizeMode.NoResize : ResizeMode.CanMinimize; }
		}
		public static Brush Background
		{
			get { return IsWin7 ? Brushes.WhiteSmoke : Brushes.White; }
		}

		static Theme()
		{
			Name =  (Environment.OSVersion.Version.ToString().StartsWith("6.1") ? "Windows 7" : "Windows 8");
		}
	}
}
