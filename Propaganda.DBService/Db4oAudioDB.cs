using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Linq;
using Propaganda.Core.Interfaces.Audio;
using Propaganda.Core.Interfaces.Database;
using Propaganda.DBService;
using Propaganda.Domain.Audio;

namespace Propaganda.Audio
{
    /// <summary>
    /// Bridge between the database and the audio module.  
    /// </summary>
    public class Db4oAudioDB : IAudioDB
    {
        #region IAudioDB Members

        public IList<Album> RetrieveAllAlbums(IDBClient db)
        {
            var db4oClient = db as Db4oClient;

            // query to select all albums
            if (db4oClient != null)
            {
                IEnumerable<Album> result = from Album a in db4oClient.Client
                                            select a;

                IList<Album> resultList = result.ToList();

                return resultList;
            }
            return null;
        }

        public IList<Album> RetrieveAllAlbumsByTitle(IDBClient db, Compression compression)
        {
            var db4oClient = db as Db4oClient;

            // query to select all albums
            if (db4oClient != null)
            {
                IEnumerable<Album> result = from Album a in db4oClient.Client
                                            where a.Compression.Equals(compression)
                                            orderby a.Title
                                            select a;

                IList<Album> resultList = result.ToList();

                return resultList;
            }
            return null;
        }

        public IList<Album> RetrieveAllAlbumsByArtist(IDBClient db, Compression compression)
        {
            var db4oClient = db as Db4oClient;

            // query to select all albums
            if (db4oClient != null)
            {
                IEnumerable<Album> result = from Album a in db4oClient.Client
                                            where a.Compression.Equals(compression)
                                            orderby a.Artist.Name
                                            select a;

                IList<Album> resultList = result.ToList();

                return resultList;
            }
            return null;
        }

        public IList<Album> RetrieveAllAlbumsByGenre(IDBClient db, Compression compression)
        {
            var db4oClient = db as Db4oClient;

            // query to select all albums
            if (db4oClient != null)
            {
                IEnumerable<Album> result = from Album a in db4oClient.Client
                                            where a.Compression.Equals(compression)
                                            orderby a.Genre
                                            select a;

                IList<Album> resultList = result.ToList();

                return resultList;
            }

            return null;
        }

        public void AddAlbum(IDBClient db, Album toAdd)
        {
            var db4oClient = db as Db4oClient;

            // add the Album to the database
            if (db4oClient != null) db4oClient.Client.Store(toAdd);
        }

        public void RemoveAlbum(IDBClient db, Album toRemove)
        {
            var db4oClient = db as Db4oClient;

            // remove the Album from the database
            if (db4oClient != null) db4oClient.Client.Delete(toRemove);
        }

        public int RetrieveNumberOfAlbums(IDBClient db)
        {
            var db4oClient = db as Db4oClient;

            // query to select all albums
            if (db4oClient != null)
            {
                IEnumerable<Album> result = from Album a in db4oClient.Client
                                            select a;

                return result.Count();
            }

            return 0;
        }

        public int RetrieveNumberOfTracks(IDBClient db)
        {
            var db4oClient = db as Db4oClient;

            // query to select all tracks
            if (db4oClient != null)
            {
                IEnumerable<Track> result = from Track t in db4oClient.Client
                                            select t;

                return result.Count();
            }
            return 0;
        }

        public Album RetrieveAlbumByName(IDBClient db, string albumName, Compression compression)
        {
            var db4oClient = db as Db4oClient;

            // search for the album
            if (db4oClient != null)
            {
                IEnumerable<Album> result = from Album a in db4oClient.Client
                                            where a.Title == albumName &&
                                                  a.Compression.Equals(compression)
                                            select a;

                // return the first one if anything returned
                if (result.Count() > 0)
                {
                    return result.ToArray()[0];
                }
            }
            return null;
        }

        public IList<Album> RetrieveAlbumsWithoutCoverArt(IDBClient db)
        {
            var db4oClient = db as Db4oClient;

            // search for the albums
            if (db4oClient != null)
            {
                IEnumerable<Album> result = from Album a in db4oClient.Client
                                            where a.CoverArt == Album.IMPORTED_COVERART
                                            select a;

                return result.ToList();
            }

            return null;
        }

        public Track RetrieveTrackByName(IDBClient db, string trackName, Compression compression)
        {
            var db4oClient = db as Db4oClient;

            // search for the track
            if (db4oClient != null)
            {
                IEnumerable<Track> result = from Track t in db4oClient.Client
                                            where t.Title == trackName &&
                                                  t.Album.Compression.Equals(compression)
                                            select t;

                // return the first one if anything returned
                if (result.Count() > 0)
                {
                    return result.ToArray()[0];
                }
            }
            return null;
        }

        public Track RetrieveTrackByNameAndAlbumName(IDBClient db, string trackName, string albumName,
                                                     Compression compression)
        {
            var db4oClient = db as Db4oClient;

            // search for the track
            if (db4oClient != null)
            {
                IEnumerable<Track> result = from Track t in db4oClient.Client
                                            where t.Title == trackName &&
                                                  t.Album.Title == albumName &&
                                                  t.Album.Compression.Equals(compression)
                                            select t;

                // return the first one if anything returned
                if (result.Count() > 0)
                {
                    return result.ToArray()[0];
                }
            }
            return null;
        }

        public IList<Track> RetrieveTracksByArtist(IDBClient db, Artist theArtist, Compression compression)
        {
            var db4oClient = db as Db4oClient;


            // search for the track
            if (db4oClient != null)
            {
                IEnumerable<Track> result = from Track t in db4oClient.Client
                                            where t.Album.Artist == theArtist &&
                                                  t.Album.Compression.Equals(compression)
                                            select t;

                // return the first one if anything returned
                return result.ToList();
            }

            return null;
        }

        public Artist RetrieveArtistByName(IDBClient db, string artistName)
        {
            var db4oClient = db as Db4oClient;

            // search for the track
            if (db4oClient != null)
            {
                IEnumerable<Artist> result = from Artist a in db4oClient.Client
                                             where a.Name == artistName
                                             select a;

                // return the first one if anything returned
                if (result.Count() > 0)
                {
                    return result.ToArray()[0];
                }
            }
            return null;
        }

        public void UpdateAddTrack(IDBClient db, Track toAdd)
        {
            var db4oClient = db as Db4oClient;

            // add the track to the database
            if (db4oClient != null) db4oClient.Client.Store(toAdd);
        }

        public void UpdateAddArtist(IDBClient db, Artist toAdd)
        {
            var db4oClient = db as Db4oClient;

            // add the artist to the database
            if (db4oClient != null) db4oClient.Client.Store(toAdd);
        }

        public void UpdateAddAlbum(IDBClient db, Album toAdd)
        {
            var db4oClient = db as Db4oClient;

            // add the album to the database
            if (db4oClient != null) db4oClient.Client.Store(toAdd);
        }

        public IList<AudioLibraryLocation> RetrieveLibraryLocations(IDBClient db)
        {
            var db4oClient = db as Db4oClient;

            if (db4oClient != null)
            {
                IEnumerable<AudioLibraryLocation> result = from AudioLibraryLocation a in db4oClient.Client
                                                      select a;

                return result.ToList();
            }

            return null;
        }

        public AudioLibraryLocation RetrieveLibraryLocation(IDBClient db, string name, string path)
        {
            var db4oClient = db as Db4oClient;

            if (db4oClient != null)
            {
                IEnumerable<AudioLibraryLocation> result = from AudioLibraryLocation a in db4oClient.Client
                                                      where a.Name == name && a.Path == path
                                                      select a;

                // return the first one if anything returned
                if (result.Count() > 0)
                {
                    return result.ToArray()[0];
                }
            }
            return null;
        }

        public void RemoveLibraryLocation(IDBClient db, AudioLibraryLocation toDelete)
        {
            var db4oClient = db as Db4oClient;

            if (db4oClient != null) db4oClient.Client.Delete(toDelete);
        }

        public void UpdateAddLibraryLocation(IDBClient db, AudioLibraryLocation location)
        {
            var db4oClient = db as Db4oClient;

            // add the location to the database
            if (db4oClient != null) db4oClient.Client.Store(location);
        }

        public IDBClient RetrieveClient()
        {
            // get a hook to the core
            IApplicationContext context = ContextRegistry.GetContext();
            var dbService = context.GetObject("Db4oService") as DBService.Db4oService;

            if (dbService != null)
            {
                return new Db4oClient {Client = dbService.GetClient()};
            }

            throw new Db4oException("Unable to get a database client");
        }

        #endregion
    }
}