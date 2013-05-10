using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations
{
	static class Theme
	{
		public static string Name { get; set; }

		static Theme()
		{
			Name = ConfigurationManager.AppSettings["FileOperationStyle"] ??
						 (Environment.OSVersion.Version.ToString().StartsWith("6.1") ? "Windows 7" : "Windows 8");
		}
	}

	public class FOWindow : Window
	{
		public FOWindow()
		{
			if (Theme.Name == "Windows 7")
			{
				Background = Brushes.WhiteSmoke;
			}
		}
	}
}
