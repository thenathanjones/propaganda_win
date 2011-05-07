using System.Collections.Generic;
using Propaganda.Core.Interfaces.Database;
using Propaganda.Domain.Audio;

namespace Propaganda.Core.Interfaces.Audio 
{
    public interface IAudioDB : IDatabase
    {
        /// <summary>
        /// Retrieve all of the Albums in the database
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        IList<Album> RetrieveAllAlbums(IDBClient db);

        /// <summary>
        /// Retrieve all of the Albums in the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="compression"
        /// <returns></returns>
        IList<Album> RetrieveAllAlbumsByTitle(IDBClient db, Compression compression);

        /// <summary>
        /// Retrieve all of the Albums in the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        IList<Album> RetrieveAllAlbumsByArtist(IDBClient db, Compression compression);

        /// <summary>
        /// Retrieve all of the Albums in the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        IList<Album> RetrieveAllAlbumsByGenre(IDBClient db, Compression compression);

        /// <summary>
        /// Add an Album to the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="toAdd"></param>
        void AddAlbum(IDBClient db, Album toAdd);

        /// <summary>
        /// Remove an Album from the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="toRemove"></param>
        void RemoveAlbum(IDBClient db, Album toRemove);

        /// <summary>
        /// Retrieve the total number of Albums in the DB
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        int RetrieveNumberOfAlbums(IDBClient db);

        /// <summary>
        /// Retrieve the total number of Tracks in the DB
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        int RetrieveNumberOfTracks(IDBClient db);

        /// <summary>
        /// Retrieve an Album with the supplied name
        /// </summary>
        /// <param name="db"></param>
        /// <param name="albumName"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        Album RetrieveAlbumByName(IDBClient db, string albumName, Compression compression);

        /// <summary>
        /// Retrieve all of the albums that have no cover art
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        IList<Album> RetrieveAlbumsWithoutCoverArt(IDBClient db);

        /// <summary>
        /// Retrieve a Track using the supplied name
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trackName"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        Track RetrieveTrackByName(IDBClient db, string trackName, Compression compression);

        /// <summary>
        /// Retrieve a Track using the supplied name
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trackName"></param>
        /// <param name="albumName"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        Track RetrieveTrackByNameAndAlbumName(IDBClient db, string trackName, string albumName,
                                              Compression compression);

        /// <summary>
        /// Retrieve all Tracks for the supplied Artist
        /// </summary>
        /// <param name="db"></param>
        /// <param name="theArtist"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        IList<Track> RetrieveTracksByArtist(IDBClient db, Artist theArtist, Compression compression);

        /// <summary>
        /// Retrieve an Artist by name
        /// </summary>
        /// <param name="db"></param>
        /// <param name="artistName"></param>
        /// <returns></returns>
        Artist RetrieveArtistByName(IDBClient db, string artistName);

        /// <summary>
        /// Add a Track to the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="toAdd"></param>
        void UpdateAddTrack(IDBClient db, Track toAdd);

        /// <summary>
        /// Add an Artist to the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="toAdd"></param>
        void UpdateAddArtist(IDBClient db, Artist toAdd);

        /// <summary>
        /// Add an Album to the database
        /// </summary>
        /// <param name="db"></param>
        /// <param name="toAdd"></param>
        void UpdateAddAlbum(IDBClient db, Album toAdd);

        /// <summary>
        /// Find all of the current audio library locations
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        IList<AudioLibraryLocation> RetrieveLibraryLocations(IDBClient db);

        /// <summary>
        /// Find the library location referenced
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        AudioLibraryLocation RetrieveLibraryLocation(IDBClient db, string name, string path);

        /// <summary>
        /// Remove the provided library location
        /// </summary>
        /// <param name="db"></param>
        /// <param name="toDelete"></param>
        void RemoveLibraryLocation(IDBClient db, AudioLibraryLocation toDelete);

        /// <summary>
        /// Add or update a library location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="db"></param>
        void UpdateAddLibraryLocation(IDBClient db, AudioLibraryLocation location);
    }
}