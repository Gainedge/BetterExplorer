using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAPICodePack.Shell;

namespace BetterExplorer
{
    class PathStringCombiner
    {

        public static string CombinePaths(List<ShellObject> paths, bool checkforfolders = false)
        {
            string ret = "";

            foreach (ShellObject item in paths)
            {
                if (checkforfolders == false)
                {
                    ret += ";" + item.ParsingName;
                }
                else
                {
                    if (item.IsFolder == true)
                    {
                        ret += ";(f)" + item.ParsingName;
                    }
                    else
                    {
                        ret += ";" + item.ParsingName;
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
