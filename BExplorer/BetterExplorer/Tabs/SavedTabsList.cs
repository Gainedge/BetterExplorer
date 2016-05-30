using System;
using System.Collections.Generic;
using System.IO;

namespace BetterExplorer
{

    /// <summary>
    /// Adds a new string to the tab collection. If a value already exists in the list, though, an
    /// ArgumentOutOfRangeException will be thrown.
    /// </summary>
    public class SavedTabsList : List<string>
    {
        public static SavedTabsList CreateFromString(string values)
        {
            var o = new SavedTabsList();
            o.AddRange(values.Split('|'));
            return o;
        }

        public static SavedTabsList LoadTabList(string file)
        {
            using (var sr = new StreamReader(file))
            {
                return CreateFromString(sr.ReadLine());
            }
        }

        public static void SaveTabList(SavedTabsList locs, string file)
        {
            using (var sw = new StreamWriter(file, false))
            {
				sw.WriteLine(string.Join("|", locs));
			}
        }

        public new void Add(string loc)
        {
            if (!this.Contains(loc))
                base.Add(loc);
            else
                throw new ArgumentOutOfRangeException("loc", "This location already exists within this list.");
        }

        public new void Remove(string loc)
        {
            if (this.Contains(loc))
                base.Remove(loc);
            else
                throw new ArgumentOutOfRangeException("loc", "This location does not exist in this list and cannot be removed.");
        }
    }
}