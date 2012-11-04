using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BetterExplorer
{

    /// <summary>
    /// Adds a new string to the tab collection. If a value already exists in the list, though, an ArgumentOutOfRangeException will be thrown.
    /// </summary>
    public class SavedTabsList : List<string>
    {


        public new void Add(string loc)
        {
            if (this.Contains(loc) == false)
            {
                base.Add(loc);
            }
            else
            {
                throw new ArgumentOutOfRangeException("loc", "This location already exists within this list.");
            }
        }

        public new void Remove(string loc)
        {
            if (this.Contains(loc) == false)
            {
                throw new ArgumentOutOfRangeException("loc", "This location does not exist in this list and cannot be removed.");
            }
            else
            {
                base.Remove(loc);
            }
        }

        public void SwitchPlaces(int val1, int val2)
        {
            string o = this[val1];
            string i = this[val2];

            this[val2] = o;
            this[val1] = i;
        }

        public string ListToString()
        {
            string path = null;
            foreach (string item in this)
            {
                //This way i think is better of making multiple line in .Net ;)
                if (string.IsNullOrEmpty(path))
                {
                    path = item;
                }
                else
                {
                    path = path + "|" + item;
                }

            }
            return path;
        }

        public static SavedTabsList CreateFromString(string values)
        {
            SavedTabsList o = new SavedTabsList();
            string[] vals = values.Split('|');
            foreach (string item in vals)
            {
                try
                {
                    o.Add(item);
                }
                catch
                {

                }
            }
            return o;
        }

        public static SavedTabsList LoadTabList(string file)
        {
            string line;
            using (StreamReader sr = new StreamReader(file))
            {
                line = sr.ReadLine();
            }
            return SavedTabsList.CreateFromString(line);
        }

        public static void SaveTabList(SavedTabsList locs, string file)
        {

            //if (Directory.Exists(sstdir) == false)
            //{
            //    Directory.CreateDirectory(sstdir);
            //}

            using (StreamWriter sw = new StreamWriter(file, false))
            {
                sw.WriteLine(locs.ListToString());
            }
        }

    }
}
