using System.Collections.Generic;
using Propaganda.Core.Interfaces.Database;
using Propaganda.Domain.Video;

namespace Propaganda.Video.Interfaces
{
    public interface IVideoDB : IDatabase
    {
        /// <summary>
        /// Find all of the current audio library locations
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        IList<VideoLibraryLocation> RetrieveLibraryLocations(IDBClient db);

        /// <summary>
        /// Find the library location referenced
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        VideoLibraryLocation RetrieveLibraryLocation(IDBClient db, string name, string path);

        /// <summary>
        /// Remove the provided library location
        /// </summary>
        /// <param name="db"></param>
        /// <param name="toDelete"></param>
        void RemoveLibraryLocation(IDBClient db, VideoLibraryLocation toDelete);

        /// <summary>
        /// Add or update a library location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="db"></param>
        void UpdateAddLibraryLocation(IDBClient db, VideoLibraryLocation location);
    }
}