namespace Propaganda.Domain
{
    /// <summary>
    /// Interface representing something with a filename that can be passed to a media service to be played
    /// </summary>
    public interface IMediaFile
    {
        /// <summary>
        /// Title of this IMediaFile object
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The file name of this file so it can be played
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Return the display name of this IPlayable object
        /// </summary>
        /// <returns></returns>
        string DisplayName { get; }

        /// <summary>
        /// Retrieve the length of this IMediaFile object in seconds
        /// </summary>
        /// <returns></returns>
        double LengthInSeconds { get; }
    }
}