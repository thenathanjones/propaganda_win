using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using Propaganda.Core.Interfaces.Audio;
using Propaganda.Domain.Audio;
using File=TagLib.File;

namespace Propaganda.EAC
{
    /// <summary>
    /// Class for interacting with EAC
    /// </summary>
    public class EACService : IRippingService
    {
        /// <summary>
        /// A hook to the process
        /// We need this to start or stop it
        /// </summary>
        private Process _eacProcess;

        /// <summary>
        /// A logger for this class
        /// </summary>
        private ILog _log = LogManager.GetLogger(typeof (EACService));

        /// <summary>
        /// Used to check our progress, we need to know the number of tracks
        /// </summary>
        private int _numberOfTracks;

        /// <summary>
        /// File system watcher used to check our progress
        /// </summary>
        private FileSystemWatcher _progressWatcher;

        #region IRippingService Members

        public string RipFLAC(string driveLetter, int numberTracks)
        {
            // we need a temp path
            string path = System.IO.Path.GetTempPath();

            // store the number of tracks for this process
            _numberOfTracks = numberTracks;

            // init the file system watcher
            // TODO
            //InitFileSystemWatcher(path, "*.flac");
            _progressWatcher.EnableRaisingEvents = true;

            // build the options string
            var optionBuilder = new StringBuilder();
            optionBuilder.Append(EACConstants.OPTION_AUTOSTART);
            optionBuilder.Append(" ");
            //optionBuilder.Append(EACConstants.OPTION_DRIVE);
            //optionBuilder.Append(" ");
            //optionBuilder.Append(driveLetter + ":");
            //optionBuilder.Append(" ");
            optionBuilder.Append(EACConstants.OPTION_NOCDTEXT);
            optionBuilder.Append(" ");
            optionBuilder.Append(EACConstants.OPTION_CLOSE);
            string options = optionBuilder.ToString();

            // run EAC to rip the CD
            RunEAC(options);

            // we've finished, stop watching
            _progressWatcher.EnableRaisingEvents = false;

            // pass back the temporary folder
            return path;
        }

        public string RipMP3192(string driveLetter, int numberTracks)
        {
            throw new NotImplementedException();
        }

        public string RipMP3VBR192(string driveLetter, int numberTracks)
        {
            throw new NotImplementedException();
        }

        public string RipMP3320(string driveLetter, int numberTracks)
        {
            throw new NotImplementedException();
        }

        public void TagFile(string filePath, Track theTrack)
        {
            // Create the file which we'll add the tag info to
            File toTag = File.Create(filePath);

            // fill out the tag information from the track
            toTag.Tag.Album = theTrack.Album.Title;
            toTag.Tag.Title = theTrack.Title;
            toTag.Tag.Track = (uint) theTrack.TrackNumber;
            toTag.Tag.Year = (uint) theTrack.Album.ReleaseYear;
            toTag.Tag.Genres = new[] {theTrack.Album.Genre};
            var artistNames = new List<string>();
            foreach (Artist artist in theTrack.Artists)
            {
                artistNames.Add(artist.Name);
            }
            toTag.Tag.AlbumArtists = artistNames.ToArray();

            // save our changes in the file
            toTag.Save();

            // now reverse engineer info for the track
            theTrack.TrackLength = toTag.Properties.Duration;
            if (toTag.MimeType == "taglib/flac")
            {
                theTrack.Album.Compression = Compression.Lossless;
            }
            else if (toTag.MimeType == "taglib/mp3")
            {
                theTrack.Album.Compression = Compression.Lossy;
            }
        }

        public void MoveToLibrary(string tempPath, string libraryLocation, Track theTrack)
        {
            // work out the new path
            string artistFolder = libraryLocation + "/" + theTrack.Album.Artist.Name;
            // create the directory if it doesn't exist
            if (!Directory.Exists(artistFolder))
            {
                Directory.CreateDirectory(artistFolder);
            }

            // repeat the process for the album
            string albumFolder = artistFolder + "/" + theTrack.Album.Title;
            // create the directory if it doesn't exist
            if (!Directory.Exists(albumFolder))
            {
                Directory.CreateDirectory(albumFolder);
            }

            // Propaganda only creates FLAC and MP3, so we can determine this via the compression
            string extension = "";
            switch (theTrack.Album.Compression)
            {
                case Compression.Lossy:
                    extension = ".mp3";
                    break;
                case Compression.Lossless:
                    extension = ".flac";
                    break;
            }

            // move file the from the temporary location to its new location
            string newPath = albumFolder + "/" + theTrack.TrackNumber + "-" + theTrack.Title + extension;
            System.IO.File.Move(tempPath, newPath);

            // add the new path to the track
            theTrack.FileName = newPath;
        }

        #endregion

        /// <summary>
        /// After each of the registry changes has been made, run EAC with the provided options
        /// </summary>
        private void RunEAC(string options)
        {
            // if it's currently already running, stop it
            if (null != _eacProcess)
            {
                _eacProcess.Kill();
            }

            // create a new process
            _eacProcess = new Process();

            // fill out the extra info
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "C:/Program Files/Exact Audio Copy/eac.exe";
            startInfo.Arguments = options;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // attach them to the process
            _eacProcess.StartInfo = startInfo;

            // rip away
            _eacProcess.Start();
            _eacProcess.WaitForExit();
        }

        #region IComponent Members

        public string Name
        {
            get { return "EAC CD Ripping Service"; }
        }

        public void Initialise()
        {
            // TODO Nothing to do here?
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            // TODO Nothing to do here?
        }

        #endregion
    }
}