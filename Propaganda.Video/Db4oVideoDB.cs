using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Linq;
using Propaganda.Core.Interfaces.Database;
using Propaganda.DBService;
using Propaganda.Domain.Video;
using Propaganda.Video.Interfaces;
using Spring.Context.Support;
using Spring.Context;

namespace Propaganda.Video
{
    /// <summary>
    /// Bridge between the database and the video module.  Intended to be pulled out to an interface once the functionality becomes clear
    /// </summary>
    public class Db4oVideoDB : IVideoDB
    {
        #region IVideoDB Members

        /// <summary>
        /// Retrieve a connection to the database
        /// </summary>
        /// <returns></returns>
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

        public IList<VideoLibraryLocation> RetrieveLibraryLocations(IDBClient db)
        {
            var db4oClient = db as Db4oClient;

            if (db4oClient != null)
            {
                IEnumerable<VideoLibraryLocation> result = from VideoLibraryLocation a in db4oClient.Client
                                                           select a;

                return result.ToList();
            }
            return null;
        }

        public VideoLibraryLocation RetrieveLibraryLocation(IDBClient db, string name, string path)
        {
            var db4oClient = db as Db4oClient;

            if (db4oClient != null)
            {
                IEnumerable<VideoLibraryLocation> result = from VideoLibraryLocation a in db4oClient.Client
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

        public void RemoveLibraryLocation(IDBClient db, VideoLibraryLocation toDelete)
        {
            var db4oClient = db as Db4oClient;

            if (db4oClient != null) db4oClient.Client.Delete(toDelete);
        }

        public void UpdateAddLibraryLocation(IDBClient db, VideoLibraryLocation location)
        {
            var db4oClient = db as Db4oClient;

            // add the location to the database
            if (db4oClient != null) db4oClient.Client.Store(location);
        }

        #endregion
    }
}