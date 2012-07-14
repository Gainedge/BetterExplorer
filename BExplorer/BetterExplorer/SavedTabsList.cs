using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer
{

    /// <summary>
    /// Adds a new string to the tab collection. If a value already exists in the list, though, an ArgumentOutOfRangeException will be thrown.
    /// </summary>
    class SavedTabsList : List<string>
    {
        public void Add(string loc)
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
                    path = path + ";" + item;
                }

            }
            return path;
        }

        public static SavedTabsList CreateFromString(string values)
        {
            SavedTabsList o = new SavedTabsList();
            string[] vals = values.Split(';');
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

    }
}
