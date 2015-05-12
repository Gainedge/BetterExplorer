using System;
using System.Collections.Generic;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;

namespace BetterExplorer {

	internal class PathStringCombiner {

		public static string CombinePaths(List<IListItemEx> paths, string separatorvalue = ";", bool checkforfolders = false) {
			//TODO: Consider inlining this into the two places it is used
			string ret = "";

			foreach (var item in paths) {
				if (!checkforfolders)
					ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
				else if (item.IsFolder)
					ret += String.Format("{0}(f){1}", separatorvalue, item.ParsingName.Replace(@"\\", @"\"));
				else
					ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
			}

			return ret;
		}


		public static string CombinePathsWithSinglePath(string path, List<IListItemEx> files, bool checkforfolders = false) {
			//TODO: Consider inlining in the one spot this is used
			string ret = "";

			foreach (var item in files) {
				if (!checkforfolders)
					ret += String.Format(";{0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
				else if (item.IsFolder)
					ret += String.Format(";(f){0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
				else
					ret += String.Format(";{0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
			}

			return ret;
		}
	}
}