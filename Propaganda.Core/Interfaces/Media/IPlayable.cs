namespace Propaganda.Core.Interfaces.Media
{
    /// <summary>
    /// Interface representing something with a filename that can be passed to a media service to be played
    /// </summary>
    public interface IPlayable
    {
        /// <summary>
        /// Title of this <code>IPlayable</code> object
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The file name of this file so it can be played
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Return the display name of this <code>IPlayable</code> object
        /// </summary>
        /// <returns></returns>
        string DisplayName { get; }

        /// <summary>
        /// Retrieve the length of this <code>IPlayable</code> object in seconds
        /// </summary>
        /// <returns></returns>
        int LengthInSeconds { get; }
    }
}