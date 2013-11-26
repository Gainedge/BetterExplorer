using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExplorer.Shell;
using BExplorer.Shell.Interop;

namespace BetterExplorer
{
    class PathStringCombiner
    {

        public static string CombinePaths(List<ShellItem> paths, string separatorvalue = ";", bool checkforfolders = false)
        {
            string ret = "";

            foreach (ShellItem item in paths)
            {
                if (checkforfolders == false)
                {
                    ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
                }
                else
                {
                    if (item.IsFolder == true)
                    {
											ret += String.Format("{0}(f){1}", separatorvalue, item.ParsingName.Replace(@"\\", @"\"));
                    }
                    else
                    {
                        ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
                    }
                }
            }

            return ret;
        }

        public static string CombinePathsWithSinglePath(string path, List<ShellItem> files, bool checkforfolders = false)
        {
            string ret = "";

            foreach (ShellItem item in files)
            {
							if (checkforfolders == false)
							{
								ret += String.Format(";{0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
							}
							else
							{
								if (item.IsFolder == true)
								{
									ret += String.Format(";(f){0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
								}
								else
								{
									ret += String.Format(";{0}{1}", path, item.GetDisplayName(SIGDN.NORMALDISPLAY));
								}
							}
            }

            return ret;
        }

        public static List<string> ListPaths(string str)
        {
            List<string> ret = new List<string>();
            string[] Paths = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in Paths)
            {
                ret.Add(item);
            }
            return ret;
        }

    }
}
