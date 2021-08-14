using System;

namespace BetterExplorer.Networks.FileSystem
{
	/// <summary>
	/// A representation of a file in the file system. A file can be downloaded.
	/// </summary>
	[Obsolete("I think we should use [System.IO.FileInfo]")]
	public class File : FileSystemObject
	{

		/// <summary>The size, in bytes, of the file.</summary>
		public long Size { get; private set; }

		/// <summary>
		/// Creates a new file system object of the File type.
		/// </summary>
		/// <param name="name">The name of the file.</param>
		/// <param name="path">The path to access the file.</param>
		/// <param name="creationdate">The date the file was created.</param>
		/// <param name="size">The size, in bytes, of the file.</param>
		public File(string name, string path, Directory parent, DateTime? creationdate, long? size)
		{

			this.Name = name;
			this.Path = path;
			this.Parent = parent;
			this.Type = FileSystemObjectType.File;
			if (size.HasValue)
			{
				this.Size = size.Value;
			}
			if (creationdate.HasValue)
			{
				this.CreationDate = creationdate.Value;
			}
		}
	}
}
