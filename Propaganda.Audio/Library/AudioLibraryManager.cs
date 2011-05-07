using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Xml;
using Db4objects.Db4o.Ext;
using log4net;
using Propaganda.Core.Interfaces;
using Propaganda.Core.Interfaces.Audio;
using Propaganda.Core.Interfaces.Database;
using Propaganda.Core.Util.Threading;
using Propaganda.Domain;
using Propaganda.Domain.Audio;

namespace Propaganda.Audio.Library
{
    internal class AudioLibraryManager : IComponent
    {
        /// <summary>
        /// A logger for this class
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof (AudioLibraryManager));

        /// <summary>
        /// We'll use a thread pool to speed this up
        /// </summary>
        private FixedThreadPool _libraryThreadPool;

        /// <summary>
        /// Reference to the audio DB
        /// </summary>
        public IAudioDB Database { get; set; }

        private readonly ITimer _coverUpdateTimer;

        private readonly ITimer _libraryUpdateTimer;

         public AudioLibraryManager(ITimer coverArtTimer, ITimer libraryTimer)
        {
            _libraryUpdateTimer = libraryTimer;
            _coverUpdateTimer = coverArtTimer;
        }

        #region IComponent Members

        public void Initialise()
        {
            // initialise library locations
            InitialiseLibraryLocations();

            // prepare the timer for background processing of album cover art
            _coverUpdateTimer.Initialise(60000);
            _coverUpdateTimer.Tick += UpdateCoverArt;
            _coverUpdateTimer.Start();

            // prepare the timer for the background processing of tracks
            _libraryUpdateTimer.Initialise(60000);
            _libraryUpdateTimer.Tick += UpdateLibrary;
            _libraryUpdateTimer.Start();     
        }

        public void Dispose()
        {
            if (null != _libraryThreadPool)
            {
                _libraryThreadPool._keepRunning = false;
                _libraryThreadPool.Shutdown();
            }

            _libraryUpdateTimer.Stop();
            _libraryUpdateTimer.Dispose();

            _coverUpdateTimer.Stop();
            _coverUpdateTimer.Dispose();
        }

        #endregion

        /// <summary>
        /// Read the configuration file, establish what library locations need to be read
        /// </summary>
        private void InitialiseLibraryLocations()
        {
            // get a hook to the DB to use
            IDBClient db = Database.RetrieveClient();

            IList<LibraryLocation> confirmedLocations = new List<LibraryLocation>();

            // get the list from the config file first
            foreach (XmlNode locationNode in GetLibraryLocations())
            {
                string name = locationNode.Attributes["Name"].Value;
                string path = locationNode.Attributes["Path"].Value;

                // either find the existing location or create a new one
                LibraryLocation location = Database.RetrieveLibraryLocation(db, name, path) ??
                                           new AudioLibraryLocation(name, path);


                // add it to the ones that should be there
                confirmedLocations.Add(location);
            }

            // now we'll prune the ones which shouldn't be here
            IList<AudioLibraryLocation> locations = Database.RetrieveLibraryLocations(db);
            if (locations.Count != 0)
            {
                foreach (AudioLibraryLocation location in locations)
                {
                    if (!confirmedLocations.Contains(location))
                    {
                        Database.RemoveLibraryLocation(db, location);
                    }
                }
            }

            // wasteful, but we'll just reconfirm them all
            foreach (AudioLibraryLocation location in confirmedLocations)
            {
                Database.UpdateAddLibraryLocation(db, location);
            }

            db.Close();
        }

        /// <summary>
        /// Retrieve the library locations from the config file
        /// </summary>
        /// <returns></returns>
        private XmlNodeList GetLibraryLocations()
        {
            // TODO These must be calculated some other way
            // load the configuration file
//            var configuration = new XmlDocument();
//            if (_log.IsDebugEnabled)
//            {
//                _log.Debug("Loading configuration from: " + AudioConstants.CONFIG_FILE_PATH);
//            }
//            configuration.Load(AudioConstants.CONFIG_FILE_PATH);
//
//            return configuration.SelectNodes("/Propaganda/LibraryLocations/LibraryLocation");
            throw new NotImplementedException();
        }

        private void RipCD(object sender, ElapsedEventArgs e)
        {
            // TODO This must be resolved from the container
            // use FreeDB to grab all the track information for this album
//            var freedb = new FreeDBService();
//            IList<CDDBEntry> albumMatches = freedb.GetAlbumMatches();
//
            // TODO present the user with a choice if there is more than one
            // for now, we'll pick just one
//            if (albumMatches != null)
//                if (albumMatches.Count > 0)
//                {
//                    CDDBEntry album = albumMatches[0];
//                    Album theAlbum = freedb.GetTracks(album);
//                    IList<Track> theTracks = theAlbum.Tracks;
//
//                    var eacService = new EACService();
                    // TODO make this drive configurable
//                    string tempFolder = eacService.RipFLAC("D", theTracks.Count);
//
//                    string[] files = Directory.GetFiles(tempFolder);
//
                    // check we have a matching number of files and tracks
//                    if (files.Count() == theTracks.Count)
//                    {
                        // process each of the files
//                        for (int i = 0; i < files.Count(); i++)
//                        {
                            // fill in the tag on the file
//                            eacService.TagFile(files[i], theTracks[i]);
                            // move the tag into the library
//                            eacService.MoveToLibrary(files[i], "c:/adi_vsfz/testfolder1", theTracks[i]);
//                        }
//                    }
//                    else
//                    {
//                        _log.Error("Number of files ripped from EAC doesn't match the number of tracks from CDDB");
//                    }
//                }
        }

        /// <summary>
        /// Update the library when invoked via the timer
        /// </summary>
        private void UpdateLibrary()
        {
            try
            {
                // get a hook to the DB
                IDBClient db = Database.RetrieveClient();

                // retrieve the initial number of tracks
                int beforeTracks = Database.RetrieveNumberOfTracks(db);

                // get the audio library locations
                IList<AudioLibraryLocation> locations = Database.RetrieveLibraryLocations(db);

                // get the current timestamp, we'll use this in case anything gets modified while this is happening
                DateTime beforeUpdate = DateTime.Now;

                _log.Info("Starting library update at: " + beforeUpdate.ToLocalTime());

                try
                {
                    // recurse through each of the library locations
                    foreach (AudioLibraryLocation location in locations)
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug("Updating library location: " + location.Path);
                        }

                        // initialise the list of directories to process
                        IList<string> directoriesToProcess = new List<string>();

                        // start traversing down each directory
                        ProcessDirectory(directoriesToProcess, location.Path, location.LastWritten);

                        // if there was any processing needed to be done
                        if (directoriesToProcess.Count > 0)
                        {
                            const int numThreads = 5;
                            _libraryThreadPool = new FixedThreadPool(5, ThreadPriority.Lowest);
                            _log.Debug("Created custom thread pool for library with " + numThreads + " threads");

                            // make all the workers
                            for (int i = 0; i < directoriesToProcess.Count; i++)
                            {
                                var worker = new AudioLibraryWorker();
                                worker.Directory = directoriesToProcess[i];

                                // attach it to a worker for the pool
                                var workerItem = new WorkerItem(worker.WorkerMethod);

                                // add it to the pool
                                _libraryThreadPool.QueueWorkerItem(workerItem);

                                // start the show
                                _libraryThreadPool.Start();
                            }
                        }

                        // reset the reference to when this update run started
                        location.LastWritten = beforeUpdate;

                        // store this updated location back in the DB
                        Database.UpdateAddLibraryLocation(db, location);

                        // commit after each location.  if something goes wrong, we wont have to redo
                        db.Commit();
                    }

                    // get the number of tracks after
                    int afterTracks = Database.RetrieveNumberOfTracks(db);

                    TimeSpan elapsedTime = DateTime.Now - beforeUpdate;
                    _log.Info("Finished library update at: " + DateTime.Now.ToLocalTime() + ".  Took: " +
                              elapsedTime.TotalSeconds + " seconds");
                    _log.Info("Imported " + (afterTracks - beforeTracks) + " tracks");

                    // close the db
                    db.Close();
                }
                catch (DatabaseClosedException)
                {
                    _log.Debug("The database has been closed prematurely");
                }
            }
            catch (Db4oException ex)
            {
                _log.Error("Problem occurred when updating library", ex);
            }
            finally
            {
                _libraryUpdateTimer.Start();
            }
        }

        private void UpdateCoverArt()
        {
           _coverUpdateTimer.Stop();

           try
           {
               // get a hook to the DB
               IDBClient db = Database.RetrieveClient();

               // get the audio library locations
               IList<Album> albums = Database.RetrieveAlbumsWithoutCoverArt(db);

               // get the current timestamp, we'll use this in case anything gets modified while this is happening
               DateTime beforeUpdate = DateTime.Now;

               _log.Info("Started cover art update at: " + DateTime.Now.ToLocalTime());

               // recurse through each of the library locations
               foreach (Album album in albums)
               {
                   //if (!PropagandaCore.CoreInstance._keepRunning)
                   //{
                   //    return;
                   //}

                   bool artistPresent = (null != album.Artist.Name);
                   bool titlePresent = (null != album.Title);
                   // if this has the imported cover art 
                   if (artistPresent && titlePresent && album.CoverArt == Album.IMPORTED_COVERART)
                   {
                       // TODO This must be resolved from the container
                       // try to retrieve the cover art from Amazon
//                       var amazon = new AmazonRestService();
//                       string coverArt = amazon.RetrieveImage(album, AudioConstants.IMAGE_FOLDER);
//
                       // if successful, store it on the album
//                       if (null != coverArt)
//                       {
                           // add the cover art with the required prefix
//                           string coverArtPath = AudioConstants.IMAGES_PREFIX + "/" + coverArt;
//                           album.CoverArt = coverArtPath;
//                           Database.UpdateAddAlbum(db, album);
//                           db.Commit();
//                       }
                   }
               }

               TimeSpan elapsedTime = DateTime.Now - beforeUpdate;
               _log.Info("Finished cover art update at: " + DateTime.Now.ToLocalTime() + ".  Took: " +
                         elapsedTime.TotalSeconds + " seconds");

               db.Close();
           }
           catch (Db4oException)
           {
               // TODO
           }
           finally
           {
               _coverUpdateTimer.Start();
           }
        }

        /// <summary>
        /// Check for directories which have been modified since the 
        /// reference time and recurse down until you can't go any further
        /// </summary>
        /// <param name="toProcess"></param>
        /// <param name="directory"></param>
        /// <param name="reference"></param>
        private void ProcessDirectory(IList<string> toProcess, string directory, DateTime reference)
        {
            try
            {
                // get any sub-directories this may have
                string[] directories = Directory.GetDirectories(directory);

                // go depth first
                foreach (string subdirectory in directories)
                {
                    ProcessDirectory(toProcess, subdirectory, reference);
                }

                // now do the comparison for this directory
                DateTime lastModified = Directory.GetLastWriteTime(directory);

                // if this has been modified since our last check
                if (lastModified > reference)
                {
                    // add this directory to the list of ones to run
                    toProcess.Add(directory);
                }
            }
            catch (IOException e)
            {
                _log.Error("File IO problem when processing directory: " + directory, e);
            }
        }

        #region IComponent Members

        public string Name
        {
            get { return "Audio Library Manager"; }
        }

        #endregion
    }
}