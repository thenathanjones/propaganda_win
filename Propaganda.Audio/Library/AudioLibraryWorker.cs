using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Db4objects.Db4o.Ext;
using log4net;
using Propaganda.Core.Interfaces.Audio;
using Propaganda.Core.Interfaces.Database;
using Propaganda.Domain.Audio;
using File=TagLib.File;

namespace Propaganda.Audio.Library
{
    internal class AudioLibraryWorker
    {
        /// <summary>
        /// A logger for this class
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof (AudioLibraryWorker));

        /// <summary>
        /// The tally is used to determine if an album belongs to one person with lots of collaborations 
        /// i.e. "Mark Ronson - Version" or is a true compilation album i.e. "Triple J Hottest 100"
        /// NOTE: This forces us into one directory per album
        /// </summary>
        private readonly IDictionary<Artist, Int32> _tally = new Dictionary<Artist, Int32>();

        /// <summary>
        /// Keep track of the album artist, given usually these are usually the same, this is fine
        /// </summary>
        private Artist _albumArtist;

        /// <summary>
        /// Keep track of the album, given usually a directory means an album
        /// </summary>
        private Album _theAlbum;

        /// <summary>
        /// Reference to the database
        /// </summary>
        public IAudioDB Database { get; set; }

        /// <summary>
        /// Directory this worker is responsible for processing
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Worker method that be threaded for parallel processing of each of the directories
        /// </summary>
        public void WorkerMethod()
        {
            // take a note of the current time
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // keep track of the number of files for reporting
            var numberOfFiles = 0;

            try
            {
                // get a hook to the DB
                IDBClient db = null;

                // retrieve all the files in the supported extensions
                var files = System.IO.Directory.GetFiles(Directory, string.Empty, SearchOption.TopDirectoryOnly).Where(x => AudioConstants.SUPPORTED_LOSSLESS.Concat(AudioConstants.SUPPORTED_LOSSY).Contains(Path.GetExtension(x)));
                
                // open the database if there are files to process
                if (numberOfFiles > 0 && db == null)
                {
                    // get a DB connection
                    db = Database.RetrieveClient();

                    // increment our total
                    numberOfFiles += files.Count();

                    // add these to the list
                    foreach (string file in files)
                    {
                        // process them accordingly
                        ProcessTrack(db, file);
                    }
                }

                // attach the correct artist to the album
                if (_tally.Count > 0)
                {
                    // find the artist that appears the most
                    var highestPair = new KeyValuePair<Artist, int>(null, 0);
                    foreach (var pair in _tally)
                    {
                        if (pair.Value > highestPair.Value)
                        {
                            highestPair = pair;
                        }
                    }

                    // if they aren't on at least half the tracks, it's a various artist album
                    if (highestPair.Value >= (_theAlbum.Tracks.Count / 2))
                    {
                        // only change this in the DB if it's changed
                        if (_theAlbum.Artist.Name != highestPair.Key.Name)
                        {
                            _theAlbum.Artist = highestPair.Key;

                            // update the DB reference
                            Database.UpdateAddAlbum(db, _theAlbum);
                        }
                    }
                    else
                    {
                        _theAlbum.Artist = Artist.VARIOUS_ARTISTS;

                        // update the DB reference
                        Database.UpdateAddAlbum(db, _theAlbum);
                    }
                }

                // commit after each directory
                db.Close();
            }
            catch (IOException e)
            {
                _log.Error("File IO problem when processing directory: " + Directory, e);
            }

            if (_log.IsDebugEnabled)
            {
                _log.Debug("Processing directory '" + Directory + "' took " + stopWatch.Elapsed.TotalSeconds +
                           " seconds.");
                _log.Debug("There were " + numberOfFiles + " files, at " + (stopWatch.Elapsed.TotalSeconds / numberOfFiles) +
                           " seconds per file");
            }
        }

        private void ProcessTrack(IDBClient db, string file)
        {
            // open the file using taglib
            try
            {
                // work out the compression type from the extension
                var compression = Compression.Lossy;
                if (AudioConstants.SUPPORTED_LOSSLESS.Contains(Path.GetExtension(file)))
                    compression = Compression.Lossless;

                File musicFile = File.Create(file);

                // check whether or not the tag information is valid
                if (null == musicFile.Tag.Album || null == musicFile.Tag.Title)
                {
                    _log.Error("Invalid tag information for: " + file);
                }
                else
                {
                    // retrieve the album artist first
                    if (null == _albumArtist)
                    {
                        // find the artist that should be for this album
                        _albumArtist = Database.RetrieveArtistByName(db, musicFile.Tag.AlbumArtists[0]);

                        // check if we have an existing album artist
                        if (null == _albumArtist)
                        {
                            // if not, create one
                            _albumArtist = new Artist(musicFile.Tag.AlbumArtists[0]);
                        }
                    }

                    // have an album to work with
                    if (null == _theAlbum)
                    {
                        // we'll attempt to find an album to add it to
                        _theAlbum = Database.RetrieveAlbumByName(db, musicFile.Tag.Album, compression);

                        // if there isn't an existing album
                        if (null == _theAlbum)
                        {
                            // create a new album
                            _theAlbum = new Album(musicFile.Tag.Album, _albumArtist, musicFile.Tag.FirstGenre,
                                                  (int) musicFile.Tag.Year, compression);
                        }
                    }
                    else
                    {
                        // make sure we have the right album
                        if (_theAlbum.Title != musicFile.Tag.Album)
                        {
                            // we'll attempt to find an album to add it to or create a new one
                            _theAlbum = Database.RetrieveAlbumByName(db, musicFile.Tag.Album, compression) ??
                                        new Album(musicFile.Tag.Album, _albumArtist, musicFile.Tag.FirstGenre,
                                            (int) musicFile.Tag.Year, compression);

                            // if there isn't an existing album
                        }
                    }

                    // initialise the output track
                    Track theTrack;
                    var trackArtists = new List<Artist>();

                    // special case for tracks that have more than one artist
                    if (musicFile.Tag.Performers.Count() > 1)
                    {
                        foreach (var artist in musicFile.Tag.Performers)
                        {
                            // we'll try with the album artist first
                            var performer = _albumArtist;                            
                            if (artist != _albumArtist.Name)
                                performer = Database.RetrieveArtistByName(db, artist) ?? new Artist(artist);                            
                           
                            trackArtists.Add(performer);
                        }
                    }
                    else
                    {
                        // we'll try with the album artist first
                        if (musicFile.Tag.FirstPerformer == _albumArtist.Name)
                        {
                            trackArtists.Add(_albumArtist);
                        }
                        else
                        {
                            var performer = Database.RetrieveArtistByName(db, musicFile.Tag.FirstPerformer) ??
                                            new Artist(musicFile.Tag.FirstPerformer);
                            trackArtists.Add(performer);
                        }

                        // check for a track in the local object instead of hitting the DB
                        try
                        {
                            // TODO not sure if this will work with the multiple artists now
                            _theAlbum.Tracks.First(
                                x => (x.TrackNumber == (int) musicFile.Tag.Track && x.Title == musicFile.Tag.Title));
                        }
                        catch (InvalidOperationException) {}
                    }

                    // update the running tally
                    foreach (var artist in trackArtists)
                    {
                        int result = 0;
                        if (_tally.ContainsKey(artist))
                            result = _tally[artist];

                        if (0 == result)
                            _tally.Add(artist, 1);
                        else
                            _tally[artist] = ++result;
                        
                    }

                    // create a new track 
                    theTrack = new Track((int) musicFile.Tag.Track, trackArtists, _theAlbum,
                                         musicFile.Properties.Duration, musicFile.Tag.Title, file);

                    // add the new track to the album
                    _theAlbum.AddTrack(theTrack);


                    // update the reference in the DB
                    Database.UpdateAddTrack(db, theTrack);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                _log.Error("Taglib had problem reading: " + file, e);
                return;
            }
            catch (Db4oException e)
            {
                _log.Error("DB problem when processing track: " + file, e);
            }
            catch (IOException e)
            {
                _log.Error("File IO problem when processing track: " + file, e);
            }
        }
    }
}