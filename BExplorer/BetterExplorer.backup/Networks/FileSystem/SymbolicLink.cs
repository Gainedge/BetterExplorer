using System;

namespace BetterExplorer.Networks.FileSystem
{
	/// <summary>
	/// A representation of a symbolic link in the file system. A symbolic link has a link to another location in the file system.
	/// </summary>
	public class SymbolicLink : FileSystemObject
	{
		/// <summary>
		/// The target path of the symbolic link.
		/// </summary>
		public string Target { get; private set; }

		/// <summary>
		/// Creates a new SymbolicLink file system object.
		/// </summary>
		/// <param name="name">The name of the symbolic link.</param>
		/// <param name="path">The path of the symbolic link in the file system.</param>
		/// <param name="creationdate">The date the symbolic link was created.</param>
		/// <param name="target">The target path of the symbolic link.</param>
		public SymbolicLink(string name, string path, Directory parent, DateTime? creationdate, string target)
		{
			this.Name = name;
			this.Path = path;
			this.Target = target;
			this.Parent = parent;
			this.Type = FileSystemObjectType.SymbolicLink;
			if (creationdate.HasValue)
			{
				this.CreationDate = creationdate.Value;
			}
		}
	}
}
