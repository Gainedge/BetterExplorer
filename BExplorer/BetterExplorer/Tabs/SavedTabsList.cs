using System;
using System.Collections.Generic;
using System.IO;

namespace BetterExplorer {

	public static class SaveTabs {

		/// <summary>
		/// Loads a list of tabs from a file
		/// </summary>
		/// <param name="file">The full name of the file you want to load from</param>
		/// <returns></returns>
		public static List<string> LoadTabList(string file) {
			using (var sr = new StreamReader(file)) {
				return CreateFromString(sr.ReadLine());
			}
		}

		/// <summary>
		/// Saves the list to a file
		/// </summary>
		/// <param name="locs">The list of tabs</param>
		/// <param name="file">The file you want to save to</param>
		public static void SaveTabList(List<string> locs, string file) {
			using (var sw = new StreamWriter(file, false)) {
				sw.WriteLine(string.Join("|", locs));
			}
		}

		/// <summary>
		/// Creates a new <see cref="SavedTabsList"/> and adds each item in <paramref name="values"/> after splitting it on |
		/// </summary>
		/// <param name="values">The items you want added to the list which will be split on |</param>
		/// <returns></returns>
		public static List<string> CreateFromString(string values) {
			return new List<string>(values.Split('|'));
		}
	}
}




//	/// <summary>
//	/// Adds a new string to the tab collection. If a value already exists in the list, though, an
//	/// ArgumentOutOfRangeException will be thrown.
//	/// </summary>
//	public class SavedTabsList : List<string> {
//		/// <summary>
//		/// Creates a new <see cref="SavedTabsList"/> and adds each item in <paramref name="values"/> after splitting it on |
//		/// </summary>
//		/// <param name="values">The items you want added to the list which will be split on |</param>
//		/// <returns></returns>
//		public static SavedTabsList CreateFromString(string values) {
//			var o = new SavedTabsList();
//			o.AddRange(values.Split('|'));
//			return o;
//		}

//		/// <summary>
//		/// Loads a list of tabs from a file
//		/// </summary>
//		/// <param name="file">The full name of the file you want to load from</param>
//		/// <returns></returns>
//		public static SavedTabsList LoadTabList(string file) {
//			using (var sr = new StreamReader(file)) {
//				return CreateFromString(sr.ReadLine());
//			}
//		}

//		/// <summary>
//		/// Saves the list to a file
//		/// </summary>
//		/// <param name="locs">The list of tabs</param>
//		/// <param name="file">The file you want to save to</param>
//		public static void SaveTabList(SavedTabsList locs, string file) {
//			using (var sw = new StreamWriter(file, false)) {
//				sw.WriteLine(string.Join("|", locs));
//			}
//		}

//		/*
//		/// <summary>
//		/// Adds items to the list but throws an ArgumentOutOfRangeException if a duplicate is detected
//		/// </summary>
//		/// <param name="loc"></param>
//		public new void Add(string loc)
//        {
//            if (!this.Contains(loc))
//                base.Add(loc);
//            else
//                throw new ArgumentOutOfRangeException("loc", "This location already exists within this list.");
//        }

//        public new void Remove(string loc)
//        {
//            if (this.Contains(loc))
//                base.Remove(loc);
//            else
//                throw new ArgumentOutOfRangeException("loc", "This location does not exist in this list and cannot be removed.");
//        }
//		*/
//	}
//}