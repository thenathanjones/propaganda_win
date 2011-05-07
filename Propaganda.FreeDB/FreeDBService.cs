using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using log4net;
using Propaganda.Audio.Library.CDDB.FreeDB;
using Propaganda.CDFunctions;
using Propaganda.Core.Interfaces.Audio;
using Propaganda.Core.Interfaces.Database;
using Propaganda.Domain.Audio;

namespace Propaganda.FreeDB
{
    /// <summary>
    /// Implementation of the CDDBService using FreeDB (www.freedb.org)
    /// </summary>
    internal class FreeDBService : ICDDBService
    {
        /// <summary>
        /// A logger for this class
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof (FreeDBService));

        #region ICDDBService Members

        public IAudioDB Database { get; set; }

        public CDDrive Drive { get; set; }

        public IList<CDDBSite> GetSites()
        {
            // build up the command string
            var commandBuilder = new StringBuilder();
            commandBuilder.Append("?");
            commandBuilder.Append(FreeDBConstants.COMMAND);
            commandBuilder.Append(FreeDBConstants.COMMAND_SITES);
            commandBuilder.Append("&");
            commandBuilder.Append(GenerateHello());
            commandBuilder.Append("&");
            commandBuilder.Append(GenerateProtocol());

            // make the URL to use
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(FreeDBConstants.URL_FREEDB_MAIN);
            urlBuilder.Append(FreeDBConstants.URL_HTTP_ACCESS);

            // perform the query
            IList<string> freeDBResponse = Query(urlBuilder.ToString(), commandBuilder.ToString());

            // initialise the output list
            IList<CDDBSite> sites = new List<CDDBSite>();

            // parse the query
            if (freeDBResponse.Count > 0)
            {
                // extract the response code
                ResponseCode responseCode = ExtractResponseCode(freeDBResponse[0]);

                switch (responseCode)
                {
                    case ResponseCode.RESPONSE_CODE_DEFAULT:
                        _log.Error("Problem querying sites using command: " + commandBuilder);
                        break;
                    case ResponseCode.RESPONSE_CODE_210:
                        _log.Debug("Everything AOK");

                        // remove the response code from the start
                        freeDBResponse.RemoveAt(0);

                        foreach (string responseLine in freeDBResponse)
                        {
                            string[] response = responseLine.Split(new[] {' '});
                            string url = response[0];
                            string protocol = response[1];
                            string portNo = response[2];

                            var nameBuilder = new StringBuilder();
                            // assume all of the last "words" are part of the name
                            for (int i = 6; i < response.Count(); i++)
                            {
                                nameBuilder.Append(response[i]);
                                nameBuilder.Append(" ");
                            }
                            string name = nameBuilder.ToString().Trim();

                            // create the site object
                            var site = new CDDBSite(url, name, portNo, protocol);
                            sites.Add(site);
                        }
                        break;
                    default:
                        _log.Error("Response came back we weren't expecting, handle it");
                        break;
                }
            }

            return sites;
        }

        public bool IsAlbumPresent
        {
            get
            {
                return GetAlbumMatches().Count > 0;
            }
        }

        public IList<CDDBEntry> GetAlbumMatches()
        {
            // initialise the output list
            IList<CDDBEntry> albums = new List<CDDBEntry>();

            // check the CD first
            if (CheckCDDrive())
            {
                // get the information from the drive to make the match
                string queryString = Drive.GetCDDBQuery();

                // build up the command string
                var commandBuilder = new StringBuilder();
                commandBuilder.Append("?");
                commandBuilder.Append(FreeDBConstants.COMMAND);
                commandBuilder.Append(FreeDBConstants.COMMAND_QUERY);
                commandBuilder.Append("+");
                commandBuilder.Append(queryString);
                commandBuilder.Append("&");
                commandBuilder.Append(GenerateHello());
                commandBuilder.Append("&");
                commandBuilder.Append(GenerateProtocol());

                // make the URL to use
                var urlBuilder = new StringBuilder();
                urlBuilder.Append(FreeDBConstants.URL_FREEDB_MAIN);
                urlBuilder.Append(FreeDBConstants.URL_HTTP_ACCESS);

                // hit up FreeDB for the result
                IList<string> freeDBResponse = Query(urlBuilder.ToString(), commandBuilder.ToString());

                // parse the query
                if (freeDBResponse.Count > 0)
                {
                    // extract the response code
                    ResponseCode responseCode = ExtractResponseCode(freeDBResponse[0]);

                    switch (responseCode)
                    {
                        case ResponseCode.RESPONSE_CODE_DEFAULT:
                            _log.Error("Problem querying album using command: " + commandBuilder);
                            break;
                        case ResponseCode.RESPONSE_CODE_200:
                        case ResponseCode.RESPONSE_CODE_210:
                            _log.Debug("Found album/s");

                            // break up each of the results
                            foreach (string responseLine in freeDBResponse)
                            {
                                string[] lineSegments = responseLine.Split(new[] {' '});
                                string genre = lineSegments[1];
                                string discId = lineSegments[2];

                                // grab each of the artist
                                var artistBuilder = new StringBuilder();
                                int index = 3;
                                for (int i = index; i < lineSegments.Count(); i++)
                                {
                                    // increment the index in the array
                                    index++;
                                    // check it's not the artist/title delimiter
                                    if (lineSegments[i] != "/")
                                    {
                                        artistBuilder.Append(lineSegments[i]);
                                        artistBuilder.Append(" ");
                                    }
                                        // if it is, stop now
                                    else
                                    {
                                        break;
                                    }
                                }
                                string artist = artistBuilder.ToString().Trim();

                                // grab the title
                                var titleBuilder = new StringBuilder();
                                for (int i = index; i < lineSegments.Count(); i++)
                                {
                                    titleBuilder.Append(lineSegments[i]);
                                    titleBuilder.Append(" ");
                                }
                                string title = titleBuilder.ToString().Trim();

                                // create the output object for this listing
                                var result = new CDDBEntry(genre, discId, artist, title);

                                // and add it to the list
                                albums.Add(result);
                            }
                            break;
                        default:
                            _log.Error("Response came back we weren't expecting, handle it");
                            break;
                    }
                }
                Drive.UnLockCD();
            }

            return albums;
        }

        public Album GetTracks(CDDBEntry cddbResult)
        {
            // initialise the album to output
            Album newAlbum = null;

            // build up the command string
            var commandBuilder = new StringBuilder();
            commandBuilder.Append("?");
            commandBuilder.Append(FreeDBConstants.COMMAND);
            commandBuilder.Append(FreeDBConstants.COMMAND_READ);
            commandBuilder.Append("+");
            commandBuilder.Append(cddbResult.Genre);
            commandBuilder.Append("+");
            commandBuilder.Append(cddbResult.DiscID);
            commandBuilder.Append("&");
            commandBuilder.Append(GenerateHello());
            commandBuilder.Append("&");
            commandBuilder.Append(GenerateProtocol());

            // make the URL to use
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(FreeDBConstants.URL_FREEDB_MAIN);
            urlBuilder.Append(FreeDBConstants.URL_HTTP_ACCESS);

            // hit up FreeDB for the result
            IList<string> freeDBResponse = Query(urlBuilder.ToString(), commandBuilder.ToString());

            // parse the query
            if (freeDBResponse.Count > 0)
            {
                // extract the response code
                ResponseCode responseCode = ExtractResponseCode(freeDBResponse[0]);

                switch (responseCode)
                {
                    case ResponseCode.RESPONSE_CODE_DEFAULT:
                        _log.Error("Problem querying album using command: " + commandBuilder);
                        break;
                    case ResponseCode.RESPONSE_CODE_200:
                    case ResponseCode.RESPONSE_CODE_210:
                        _log.Debug("Found track/s");

                        // get a hook to the DB
                        IDBClient db = Database.RetrieveClient();

                        // we'll look for an artist to attach this all to
                        Artist theArtist = Database.RetrieveArtistByName(db, cddbResult.ArtistName);

                        // TODO Check for artists, as opposed to just one

                        // if we don't we'll create one
                        if (null == theArtist)
                        {
                            theArtist = new Artist(cddbResult.ArtistName);
                        }

                        db.Close();

                        // we'll create an album for this with invalid data, we'll fill that out after
                        newAlbum = new Album(cddbResult.Title, theArtist, null, 1900, Compression.Undecided);

                        // a list to keep track of the track lengths in seconds
                        IList<int> trackLengths = new List<int>();

                        // keep track of the track offsets for track lengths
                        int lastTrackOffset = 0;

                        // break up each of the results
                        foreach (string responseLine in freeDBResponse)
                        {
                            // extract the track lengths
                            if (responseLine.Contains("#\t"))
                            {
                                // parse the next offset
                                int newOffset = Int32.Parse(responseLine.Substring(2));

                                // stop the first one being processed
                                if (lastTrackOffset > 0)
                                {
                                    int seconds = (newOffset - lastTrackOffset)/75;
                                    trackLengths.Add(seconds);
                                }

                                // store the new offset
                                lastTrackOffset = newOffset;
                            }

                            // extract the total number of seconds so we can calculate the last tracks length
                            if (responseLine.Contains("# Disc length"))
                            {
                                // parse the total length of the album in seconds
                                int totalLength = Int32.Parse(responseLine.Substring(15, 4));

                                int secondsToDate = lastTrackOffset/75;

                                // extract the length of the last track from this to find out the length of the final track
                                int lastTrackLength = totalLength - secondsToDate;

                                // add this to the lengths
                                trackLengths.Add(lastTrackLength);
                            }

                            // extract the year
                            if (responseLine.Contains("DYEAR"))
                            {
                                string year = responseLine.Substring(6);
                                newAlbum.ReleaseYear = Int32.Parse(year);
                            }

                            // extract a track
                            if (responseLine.Contains("TTITLE"))
                            {
                                // marker for where to parse string to 
                                int indexOfSpace = responseLine.IndexOf('=', 6);
                                string numberString = responseLine.Substring(6, (indexOfSpace - 6));

                                int trackNumber = Int32.Parse(numberString) + 1;

                                string trackTitle = responseLine.Substring(indexOfSpace + 1);
                                // create this with no path as it's not currently set
                                var artists = new List<Artist>();
                                artists.Add(theArtist);
                                var newTrack = new Track(trackNumber, artists, newAlbum,
                                                         new Duration(new TimeSpan(0, 0, trackLengths[trackNumber - 1])),
                                                         trackTitle, null);
                                newAlbum.AddTrack(newTrack);
                            }
                        }
                        break;
                    default:
                        _log.Error("Response came back we weren't expecting, handle it");
                        break;
                }
            }

            // pass back the resulting album
            return newAlbum;
        }

        public IList<string> Query(string serverUrl, string queryString)
        {
            // setup the CDDB request
            var request = WebRequest.Create("http://" + serverUrl + queryString) as HttpWebRequest;
            request.Timeout = 30000;

            //// setup the proxy
            //IWebProxy proxy = new WebProxy(AudioModuleConstants.PROXY_HOSTNAME, 8080);
            //ICredentials credentials = new NetworkCredential(AudioModuleConstants.PROXY_USERNAME,
            //                                                 AudioModuleConstants.PROXY_PASSWORD);
            //proxy.Credentials = credentials;
            //request.Proxy = proxy;

            // initialise the output list
            IList<string> responseLines = new List<string>();

            try
            {
                var freeDBResponse = request.GetResponse() as HttpWebResponse;

                // load the response stream
                var reader = new StreamReader(freeDBResponse.GetResponseStream());

                // read until the end of the response
                string nextLine;
                while (null != (nextLine = reader.ReadLine()))
                {
                    if (nextLine != FreeDBConstants.COMMAND_TERMINATOR)
                    {
                        responseLines.Add(nextLine);
                    }
                }
            }
            catch (WebException ex)
            {
                _log.Info("Problem occurred when trying to use FreeDB service", ex);
            }

            return responseLines;
        }

        #endregion

        /// <summary>
        /// Generate the hello command part of the command string
        /// </summary>
        /// <returns></returns>
        private string GenerateHello()
        {
            var helloBuilder = new StringBuilder();
            helloBuilder.Append(FreeDBConstants.COMMAND_HELLO);
            helloBuilder.Append("=");
            helloBuilder.Append(FreeDBConstants.USER_NAME);
            helloBuilder.Append("+");
            helloBuilder.Append(Environment.MachineName);
            helloBuilder.Append("+");
            // TODO!?  
            //helloBuilder.Append(theCore.Name);
            helloBuilder.Append("+");
            //helloBuilder.Append(theCore.Version);

            return helloBuilder.ToString();
        }

        /// <summary>
        /// Generate the protocol component of the command string
        /// </summary>
        /// <returns></returns>
        private string GenerateProtocol()
        {
            var protocolBuilder = new StringBuilder();
            protocolBuilder.Append(FreeDBConstants.COMMAND_PROTOCOL);
            protocolBuilder.Append("=");
            protocolBuilder.Append(FreeDBConstants.PROTOCOL);

            return protocolBuilder.ToString();
        }


        /// <summary>
        /// Extracts the response code from the line
        /// Assumes the first word in the response is the code
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private ResponseCode ExtractResponseCode(string line)
        {
            // remove any forward and trailing whitespace
            line = line.Trim();

            // split up the line
            string[] lineComponents = line.Split(new[] {' '});

            // extract the code as the first value
            if (lineComponents.Count() > 0)
            {
                return (ResponseCode) Enum.ToObject(typeof (ResponseCode), Int32.Parse(lineComponents[0]));
            }
            else
            {
                return ResponseCode.RESPONSE_CODE_DEFAULT;
            }
        }

        /// <summary>
        /// Check CD is in the drive
        /// </summary>
        /// <returns></returns>
        private bool CheckCDDrive()
        {
            char[] driveLetters = CDDrive.GetCDDriveLetters();

            if (driveLetters.Length < 1)
            {
                _log.Error("No CD drives present");
                return false;
            }

            Drive = new CDDrive();
            if (!Drive.Open(driveLetters[0]))
            {
                _log.Error("Unable to access drive with letter: " + driveLetters[0]);
                return false;
            }

            if (!Drive.IsCDReady())
            {
                _log.Error("Drive with letter '" + driveLetters[0] + "' not ready.  Check the drive");
                return false;
            }

            // try cycling the lock on the drive
            Drive.LockCD();
            Drive.Refresh();
            Drive.UnLockCD();

            // if we've got this far, we're okay to continue
            return true;
        }

        #region IComponent Members

        public string Name
        {
            get { return "FreeDB CD Information Service"; }
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