using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks.FileSystem
{
    /// <summary>
    /// A representation of a symbolic link in the file system. A symbolic link has a link to another location in the file system.
    /// </summary>
    public class SymbolicLink : FileSystemObject
    {
        private string _target;

        /// <summary>
        /// Creates a new SymbolicLink file system object.
        /// </summary>
        /// <param name="name">The name of the symbolic link.</param>
        /// <param name="path">The path of the symbolic link in the file system.</param>
        /// <param name="creationdate">The date the symbolic link was created.</param>
        /// <param name="target">The target path of the symbolic link.</param>
        public SymbolicLink(string name, string path, DateTime? creationdate, string target)
        {
            _name = name;
            _path = path;
            _target = target;
            _type = FileSystemObjectType.SymbolicLink;
            if (creationdate.HasValue)
            {
                _timeCreated = creationdate.Value;
            }
        }

        /// <summary>
        /// The target path of the symbolic link.
        /// </summary>
        public string Target
        {
            get
            {
                return _target;
            }
        }

    }
}
