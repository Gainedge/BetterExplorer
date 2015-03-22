using System;
using System.Collections.Generic;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;

namespace BetterExplorer {

	internal class PathStringCombiner {

		public static string CombinePaths(List<IListItemEx> paths, string separatorvalue = ";", bool checkforfolders = false) {
			string ret = "";

			foreach (ShellItem item in paths) {
				if (!checkforfolders) {
					ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
				}
				else if (item.IsFolder) {
					ret += String.Format("{0}(f){1}", separatorvalue, item.ParsingName.Replace(@"\\", @"\"));
				}
				else {
					ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
				}
			}

			return ret;
		}

		public static string CombinePathsWithSinglePath(string path, List<IListItemEx> files, bool checkforfolders = false) {
			string ret = "";

			foreach (ShellItem item in files) {
				if (!checkforfolders) {
					ret += String.Format(";{0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
				}
				else if (item.IsFolder) {
					ret += String.Format(";(f){0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
				}
				else {
					ret += String.Format(";{0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
				}
			}

			return ret;
		}

		/*
		public static List<string> ListPaths(string str) {
			List<string> ret = new List<string>();
			string[] Paths = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string item in Paths) {
				ret.Add(item);
			}
			return ret;
		}
		 */
	}
}