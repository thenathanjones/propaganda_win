using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Db4objects.Db4o.Ext;
using log4net;
using Propaganda.Core.Interfaces;
using Propaganda.Core.Interfaces.Database;
using Propaganda.Core.Util.Threading;
using Propaganda.Domain;
using Propaganda.Domain.Video;
using Propaganda.Video.Interfaces;

namespace Propaganda.Video.Library
{
    internal class VideoLibraryManager : IComponent
    {
        public string Name
        {
            get { return "Video Library Manager"; }
        }

        private readonly ITimer _coverUpdateTimer;

        private readonly ITimer _libraryUpdateTimer;

        /// <summary>
        /// A logger for this class
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof(VideoLibraryManager));

        /// <summary>
        /// We'll use a thread pool to speed this up
        /// </summary>
        private FixedThreadPool _libraryThreadPool;

        /// <summary>
        /// Reference to the DB
        /// </summary>
        public IVideoDB Database { get; set; }

        public VideoLibraryManager(ITimer coverArtTimer, ITimer libraryTimer)
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

            //// get the list from the config file first
            //foreach (XmlNode locationNode in GetLibraryLocations())
            //{
            //    string name = locationNode.Attributes["Name"].Value;
            //    string path = locationNode.Attributes["Path"].Value;
            //    LibraryLocation location = Database.RetrieveLibraryLocation(db, name, path);

            //    // if this is a new location, add it to the DB
            //    if (null == location)
            //    {
            //        location = new VideoLibraryLocation(name, path);
            //    }

            //    // add it to the ones that should be there
            //    confirmedLocations.Add(location);
            //}

            // now we'll prune the ones which shouldn't be here
            IList<VideoLibraryLocation> locations = Database.RetrieveLibraryLocations(db);
            if (locations.Count != 0)
            {
                foreach (VideoLibraryLocation location in locations)
                {
                    if (!confirmedLocations.Contains(location))
                    {
                        Database.RemoveLibraryLocation(db, location);
                    }
                }
            }

            // wasteful, but we'll just reconfirm them all
            foreach (VideoLibraryLocation location in confirmedLocations)
            {
                Database.UpdateAddLibraryLocation(db, location);
            }

            db.Close();
            // -------------------------------------------------------------------------------------------
        }

        /// <summary>
        /// Update the library when invoked via the timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateLibrary()
        {
            _libraryUpdateTimer.Stop();
            try
            {
                // lower the priority of this thread
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                // get a hook to the DB
                IDBClient db = Database.RetrieveClient();


                // get the audio library locations
                IList<VideoLibraryLocation> locations = Database.RetrieveLibraryLocations(db);

                // get the current timestamp, we'll use this in case anything gets modified while this is happening
                DateTime beforeUpdate = DateTime.Now;

                _log.Info("Starting library update at: " + beforeUpdate.ToLocalTime());

                try
                {
                    // recurse through each of the library locations
                    foreach (VideoLibraryLocation location in locations)
                    {
                        if (_log.IsDebugEnabled)
                        {
                            _log.Debug("Updating library location: " + location.Path);
                        }

                        //    // initialise the list of directories to process
                        //    IList<string> directoriesToProcess = new List<string>();

                        //    // start traversing down each directory
                        //    ProcessDirectory(directoriesToProcess, location.Path, location.LastWritten);

                        //    // if there was any processing needed to be done
                        //    if (directoriesToProcess.Count > 0)
                        //    {
                        //        _libraryThreadPool = new FixedThreadPool(5, ThreadPriority.Lowest);
                        //        _log.Debug("Created custom thread pool for library with 5 threads");

                        //        // make all the workers
                        //        for (int i = 0; i < directoriesToProcess.Count; i++)
                        //        {
                        //            if (!_theModule._theCore._keepRunning)
                        //            {
                        //                _libraryThreadPool._keepRunning = false;
                        //                return;
                        //            }

                        //            // create the worker thread          
                        //            var worker = new LibraryWorker(_theModule, directoriesToProcess[i]);

                        //            // attach it to a worker for the pool
                        //            var workerItem =
                        //                new WorkerItem(new LibraryWorker.DirectoryWorker(worker.WorkerMethod));

                        //            // add it to the pool
                        //            _libraryThreadPool.QueueWorkerItem(workerItem);

                        //            // start the show
                        //            _libraryThreadPool.Start();
                        //        }
                        //    }

                        // reset the reference to when this update run started
                        location.LastWritten = beforeUpdate;

                        // store this updated location back in the DB
                        Database.UpdateAddLibraryLocation(db, location);

                        // commit after each location.  if something goes wrong, we wont have to redo
                        db.Commit();
                    }

                    TimeSpan elapsedTime = DateTime.Now - beforeUpdate;
                    _log.Info("Finished library update at: " + DateTime.Now.ToLocalTime() + ".  Took: " +
                              elapsedTime.TotalSeconds + " seconds");

                    // close the db
                    db.Close();
                }
                catch (DatabaseClosedException)
                {
                    _log.Debug("The database has been closed prematurely");
                    _libraryThreadPool._keepRunning = false;
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

                    //// get the audio library locations
                    //IList<Album> albums = _theModule._theDatabase.RetrieveAlbumsWithoutCoverArt(db);

                    // get the current timestamp, we'll use this in case anything gets modified while this is happening
                    DateTime beforeUpdate = DateTime.Now;

                    //_log.Info("Started cover art update at: " + DateTime.Now.ToLocalTime());

                    //// recurse through each of the library locations
                    //foreach (Album album in albums)
                    //{
                    //    if (!_theModule._theCore._keepRunning)
                    //    {
                    //        return;
                    //    }

                    //    bool artistPresent = (null != album._artist.Name);
                    //    bool titlePresent = (null != album._title);
                    //    // if this has the imported cover art 
                    //    if (artistPresent && titlePresent && album._coverArt == AudioModuleConstants.IMPORTED_COVERART)
                    //    {
                    //        // try to retrieve the cover art from Amazon
                    //        var amazon = new AmazonRestService();
                    //        string coverArt = amazon.RetrieveImage(album, AudioModuleConstants.IMAGE_FOLDER);

                    //        // if successful, store it on the album
                    //        if (null != coverArt)
                    //        {
                    //            // add the cover art with the required prefix
                    //            string coverArtPath = AudioModuleConstants.IMAGES_PREFIX + "/" + coverArt;
                    //            album._coverArt = coverArtPath;
                    //            _theModule._theDatabase.UpdateAddAlbum(db, album);
                    //            db.Commit();
                    //        }
                    //    }
                    //}

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
    }
}