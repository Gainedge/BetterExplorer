using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAPICodePack.Shell;

namespace BetterExplorer
{
    class PathStringCombiner
    {

        public static string CombinePaths(List<ShellObject> paths, string separatorvalue = ";", bool checkforfolders = false)
        {
            string ret = "";

            foreach (ShellObject item in paths)
            {
                if (checkforfolders == false)
                {
                    ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
                }
                else
                {
                    if (item.IsFolder == true)
                    {
                        ret += separatorvalue + "(f)" + item.ParsingName.Replace(@"\\", @"\");
                    }
                    else
                    {
                        ret += separatorvalue + item.ParsingName.Replace(@"\\", @"\");
                    }
                }
            }

            return ret;
        }

        public static string CombinePathsWithSinglePath(string path, List<ShellObject> files, bool checkforfolders = false)
        {
            string ret = "";

            foreach (ShellObject item in files)
            {
                if (checkforfolders == false)
                {
                    ret += ";" + path + item.GetDisplayName(DisplayNameType.Default);
                }
                else
                {
                    if (item.IsFolder == true)
                    {
                        ret += ";(f)" + path + item.GetDisplayName(DisplayNameType.Default);
                    }
                    else
                    {
                        ret += ";" + path + item.GetDisplayName(DisplayNameType.Default);
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
