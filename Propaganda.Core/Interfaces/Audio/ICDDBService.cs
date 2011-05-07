using System.Collections.Generic;
using Propaganda.CDFunctions;
using Propaganda.Domain.Audio;

namespace Propaganda.Core.Interfaces.Audio
{
    /// <summary>
    /// Interface describing the expected functionality of a CDDB service for retrieving track information.
    /// While initially intended for use against FreeDB, it could be expanded for use with Gracenote etc.
    /// </summary>
    public interface ICDDBService : IService
    {
        /// <summary>
        /// Reference to the database
        /// </summary>
        IAudioDB Database { get; set; }

        /// <summary>
        /// CD Drive used for processing track information
        /// </summary>
        CDDrive Drive { get; set; }

        /// <summary>
        /// Check for alternate sites for this service
        /// </summary>
        /// <returns></returns>
        IList<CDDBSite> GetSites();

        /// <summary>
        /// Check whether or not there is information for the CD in the drive
        /// </summary>
        /// <returns></returns>
        bool IsAlbumPresent { get; }

        /// <summary>
        /// Return the names of the Albums returned in CDDB for this CD
        /// </summary>
        /// <returns></returns>
        IList<CDDBEntry> GetAlbumMatches();

        /// <summary>
        /// Query the service for all the track information on the CD
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        Album GetTracks(CDDBEntry entry);

        /// <summary>
        /// Query the server with the provided query string
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="queryString"></param>
        /// <returns>Response as a list of response lines</returns>
        IList<string> Query(string serverUrl, string queryString);
    }
}