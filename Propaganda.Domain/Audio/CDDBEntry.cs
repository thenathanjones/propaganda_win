namespace Propaganda.Domain.Audio
{
    /// <summary>
    /// Container class to store the results of a CDDB query command
    /// </summary>
    public class CDDBEntry
    {
        /// <summary>
        /// Create a new CDDBEntry
        /// </summary>
        /// <param name="genre"></param>
        /// <param name="discId"></param>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        public CDDBEntry(string genre, string discId, string artist, string title)
        {
            Genre = genre;
            DiscID = discId;
            ArtistName = artist;
            Title = title;
        }

        /// <summary>
        /// Genre of this album
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// CDDB Disc ID of this CD
        /// </summary>
        public string DiscID { get; set; }

        /// <summary>
        /// Artist(s) of this CD
        /// </summary>
        public string ArtistName { get; set; }

        /// <summary>
        /// Title of this CD
        /// </summary>
        public string Title { get; set; }
    }
}