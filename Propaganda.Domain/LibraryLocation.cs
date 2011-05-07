using System;

namespace Propaganda.Domain
{
    /// <summary>
    /// Class responsible for managing the library locations
    /// </summary>
    public abstract class LibraryLocation
    {
        /// <summary>
        /// Create a new library location pointing to a specific path
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        protected LibraryLocation(string name, string path)
        {
            Name = name;
            Path = path;
            // create a fake last date which will force this new library to be imported
            LastWritten = new DateTime(1900, 1, 1);
        }

        /// <summary>
        /// File path to this library location
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Name of this library location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Marker to keep track of the last time that this was written to
        /// </summary>
        public DateTime LastWritten { get; set; }
    }
}