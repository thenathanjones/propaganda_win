using System.Collections.Generic;
using System.Linq;
using Propaganda.Audio;

namespace Propaganda.Domain.Audio
{
    /// <summary>
    /// Representation of a music Album
    /// </summary>
    public class Album
    {
        /// <summary>
        /// Create a blank Album object
        /// </summary>
        public Album()
        {
        }

        /// <summary>
        /// Create an Album with associated metadata
        /// </summary>
        /// <param name="title"></param>
        /// <param name="artist"></param>
        /// <param name="genre"></param>
        /// <param name="releaseYear"></param>
        /// <param name="compression"></param>
        public Album(string title, Artist artist, string genre, int releaseYear, AudioCompression compression)
        {
            Title = title;
            Artist = artist;
            Genre = genre;
            ReleaseYear = releaseYear;
            CoverArt = IMPORTED_COVERART;
            Compression = compression;
        }

        /// <summary>
        /// String reference to the Album cover image
        /// </summary>
        public string CoverArt { get; set; }

        /// <summary>
        /// Title of this Album
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Artist who made this Album
        /// </summary>
        public Artist Artist { get; set; }

        /// <summary>
        /// Genre of the Album
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Year the Album was released
        /// </summary>
        public int ReleaseYear { get; set; }

        /// <summary>
        /// Type of compression used for the albums on this track
        /// </summary>
        public AudioCompression Compression { get; set; }

        /// <summary>
        /// Add the Track to this Album
        /// </summary>
        /// <param name="newTrack"></param>
        public void AddTrack(Track newTrack)
        {
            // modify the track
            newTrack.Album = this;

            // if we don'a have this track on the album...
            if (!_tracks.Any(x => x.TrackNumber == newTrack.TrackNumber))
            {
                // get the first track that has a higher track number
                var higher = _tracks.FirstOrDefault(x => x.TrackNumber > newTrack.TrackNumber);

                // if there isn't one...
                if (higher == null)
                    // ...add it to the end
                    _tracks.Add(newTrack);
                else
                    // ...otherwise insert it just before
                    _tracks.Insert(_tracks.IndexOf(higher)-1, newTrack);
            }
        }

        /// <summary>
        /// Return the Tracks on this Album
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Track> Tracks 
        {
            get
            {
                return _tracks;
            }
        }
        
        /// <summary>
        /// Image for when an album is first imported
        /// </summary>
        public const string IMPORTED_COVERART = "imported.jpg";

        /// <summary>
        /// Image for when a cover art can't be found.
        /// </summary>
        public const string NO_COVERART = "nocover.jpg";

        private IList<Track> _tracks = new List<Track>();
    }
}