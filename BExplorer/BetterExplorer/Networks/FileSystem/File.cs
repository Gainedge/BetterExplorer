using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks.FileSystem
{
    /// <summary>
    /// A representation of a file in the file system. A file can be downloaded.
    /// </summary>
    public class File : FileSystemObject
    {
        private long _size = -1;

        /// <summary>
        /// Creates a new file system object of the File type.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="path">The path to access the file.</param>
        /// <param name="creationdate">The date the file was created.</param>
        /// <param name="size">The size, in bytes, of the file.</param>
        public File(string name, string path, Directory parent, DateTime? creationdate, long? size)
        {
            _name = name;
            _path = path;
            _parent = parent;
            _type = FileSystemObjectType.File;
            if (size.HasValue)
            {
                _size = size.Value;
            }
            if (creationdate.HasValue)
            {
                _timeCreated = creationdate.Value;
            }
        }

        public long Size
        {
            get
            {
                return _size;
            }
        }

    }
}
