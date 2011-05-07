namespace Propaganda.Domain.Audio
{
    /// <summary>
    /// Representation of an Artist
    /// </summary>
    public class Artist
    {
        /// <summary>
        /// Create a blank Artist object
        /// </summary>
        public Artist() {}
        
        /// <summary>
        /// Create an Artist with the supplied name
        /// </summary>
        /// <param name="artistName"></param>
        public Artist(string artistName)
        {
            Name = artistName;
        }

        /// <summary>
        /// Name of the Artist
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Artist to be used for a compilation Album
        /// </summary>
        public static Artist VARIOUS_ARTISTS = new Artist(VARIOUS_ARTIST_NAME);

        /// <summary>
        /// Name for an Artist when there is more than one per Album
        /// </summary>
        public const string VARIOUS_ARTIST_NAME = "Various";
    }
}