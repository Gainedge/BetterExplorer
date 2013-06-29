using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks.FileSystem
{
    /// <summary>
    /// A representation of an object in the file system of an online location. This class must be inherited.
    /// </summary>
    public abstract class FileSystemObject
    {

        protected string _path;
        protected string _name;
        protected DateTime _timeCreated;
        protected FileSystemObjectType _type;
        protected Directory _parent;

        /// <summary>
        /// The path of this file system object.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// The name of this file system object.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// The parent directory of this file system object. If this is the root directory, the parent will be itself.
        /// </summary>
        public Directory Parent
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// The date this file system object was created. (Might be null.)
        /// </summary>
        public DateTime CreationDate
        {
            get
            {
                return _timeCreated;
            }
        }

        /// <summary>
        /// The type this file system object is.
        /// </summary>
        public FileSystemObjectType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// What type of object this FileSystemObject is
        /// </summary>
        public enum FileSystemObjectType
        {
            /// <summary>
            /// A file. A collection of data that can be downloaded.
            /// </summary>
            File = 0,

            /// <summary>
            /// A directory. A directory may contain one or more sub-objects.
            /// </summary>
            Directory = 1,

            /// <summary>
            /// A system link. This link points to another location on the FTP server.
            /// </summary>
            SymbolicLink = 2,
        }

    }
}
