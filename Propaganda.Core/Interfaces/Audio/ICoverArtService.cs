using Propaganda.Domain.Audio;

namespace Propaganda.Core.Interfaces.Audio
{
    /// <summary>
    /// Interface for retrieving cover art.  Only really in place if we want to swap providers
    /// </summary>
    public interface ICoverArtService : IService
    {
        /// <summary>
        /// Retrieve an image from the service matching the provided credentials, 
        /// and store it in the path specified
        /// </summary>
        /// <param name="theAlbum"></param>
        /// <param name="path"></param>
        /// <returns>Return the path to the newly stored cover art</returns>
        string RetrieveImage(Album theAlbum, string path);
    }
}