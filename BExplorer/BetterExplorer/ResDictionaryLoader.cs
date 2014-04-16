using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows;

namespace BetterExplorer {
	class ResDictionaryLoader {

		public static ResourceDictionary Load(string filename) {
			if (File.Exists(filename)) {
				using (FileStream s = new FileStream(filename, FileMode.Open)) {
					return XamlReader.Load(s) as ResourceDictionary;
				}
			}
			else {
				return null;
			}

			/*
			try
			{
				using (FileStream s = new FileStream(filename, FileMode.Open))
				{
					return XamlReader.Load(s) as ResourceDictionary;
				}
			}
			catch
			{
				return null;
			}
			*/
		}
	}
}
