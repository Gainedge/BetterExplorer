using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks.FileSystem {
	/// <summary>
	/// A representation of a directory in the file system. A directory can contain one or more file system objects.
	/// </summary>
	[Obsolete("I think we should use [System.IO.DirectoryInfo]")]
	public class Directory : FileSystemObject {

		/// <summary>
		/// Creates a new SymbolicLink file system object.
		/// </summary>
		/// <param name="name">The name of the symbolic link.</param>
		/// <param name="path">The path of the symbolic link in the file system.</param>
		/// <param name="creationdate">The date the symbolic link was created.</param>
		/// <param name="target">The target path of the symbolic link.</param>
		public Directory(string name, string path, Directory parent, DateTime? creationdate) {
			_name = name;
			_path = path;
			_parent = parent;
			_type = FileSystemObjectType.Directory;
			if (creationdate.HasValue) {
				_timeCreated = creationdate.Value;
			}
		}
	}
}
