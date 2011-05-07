using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Propaganda.Domain.Audio
{
    /// <summary>
    /// Representation of a music Track
    /// </summary>
    public class Track
    {
        /// <summary>
        /// Create a new Track record
        /// </summary>
        /// <param name="trackNumber"></param>
        /// <param name="trackLength"></param>
        /// <param name="album"></param>
        /// <param name="artists"></param>
        /// <param name="title"></param>
        /// <param name="fileName"></param>
        public Track(int trackNumber, IList<Artist> artists, Album album, Duration trackLength, string title,
                     string fileName)
        {
            TrackNumber = trackNumber;
            Artists = artists;
            Album = album;
            TrackLength = trackLength;
            Title = title;
            FileName = fileName;
        }

        /// <summary>
        /// Artists of this Track
        /// </summary>
        public IList<Artist> Artists { get; set; }

        /// <summary>
        /// Track number
        /// </summary>
        public int TrackNumber { get; set; }

        /// <summary>
        /// Length of the Track in seconds
        /// </summary>
        public Duration TrackLength { get; set; }

        /// <summary>
        /// Album this Track belongs to
        /// </summary>
        public Album Album { get; set; }

        /// <summary>
        /// The title of this Track
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The full path to the Track itself
        /// </summary>
        public string FileName { get; set; }
    }
}