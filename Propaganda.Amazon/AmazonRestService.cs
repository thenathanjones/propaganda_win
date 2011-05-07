using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using log4net;
using Propaganda.Core.Interfaces.Audio;
using Propaganda.Core.Util;
using Propaganda.Domain.Audio;

namespace Propaganda.AmazonService
{
    /// <summary>
    /// Cover Art retriever using the Amazon REST service
    /// </summary>
    public class AmazonRestService : ICoverArtService
    {
        /// <summary>
        /// Invalid words and characters which confuse Amazon
        /// </summary>
        private readonly string[] _invalidWords = {
                                                      "volume", "vol", "disc 1", "disc 2", "disc 3", "disc 4", "-", ".",
                                                      "&", ":", ",", "!", "(", ")", "[", "]", "#", "_"
                                                  };

        /// <summary>
        /// A logger for this class
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger(typeof(AmazonRestService));

        
        /// <summary>
        /// Proxy to be used for web requests
        /// </summary>
        private IWebProxy Proxy { get; set; }
       

        #region ICoverArtService Members

        public string RetrieveImage(Album theAlbum, string path)
        {
            // initialise the URL string
            string urlString;

            // generate the hash using the artist and title and the compression
            string coverArtFile =
                GenerateMD5Hash(theAlbum.Artist.Name + "-" + theAlbum.Title + "-" + theAlbum.Compression) + ".jpg";

            // init the full path
            string fullPath = path + "/" + coverArtFile;

            // we'll check first to see if we have the file already
            if (File.Exists(fullPath))
            {
                // return right away without querying Amazon
                return fullPath;
            }

            // retrieve the URL used for the RESTful query
            if (theAlbum.Artist.Name == Artist.VARIOUS_ARTISTS.Name)
            {
                // initialise the search string
                string searchString;

                // corner case discovered from the CARS soundtrack
                if (theAlbum.Genre == "Soundtrack")
                {
                    searchString = theAlbum.Title + " " + theAlbum.Genre;
                }
                else
                {
                    searchString = theAlbum.Title;
                }

                // prepare the string for the search
                searchString = PrepareString(searchString);
                urlString = RetrieveRestURLNoArtist(searchString);
            }
            else
            {
                // prepare the search fields
                string artistName = PrepareString(theAlbum.Artist.Name);
                string title = PrepareString(theAlbum.Title);
                urlString = RetrieveRestURLWithArtistAndTitle(artistName, title);
            }

            XmlDocument amazonResponse = RetrieveDocumentForUrl(urlString);
            if (null != amazonResponse)
            {
                var nsmgr = new XmlNamespaceManager(amazonResponse.NameTable);
                nsmgr.AddNamespace("amazon", "http://webservices.amazon.com/AWSECommerceService/2005-10-05");

                XmlNodeList items = amazonResponse.SelectNodes("/amazon:ItemSearchResponse/amazon:Items/amazon:Item",
                                                               nsmgr);

                // if we didn't get anything, try again via the artist name
                // it's usually that we're being too specific
                if (items != null)
                {
                    if (items.Count == 0 && theAlbum.Artist.Name != Artist.VARIOUS_ARTISTS.Name)
                    {
                        // try a less specific search
                        string searchString = PrepareString(theAlbum.Artist.Name);
                        urlString = RetrieveRestURLWithArtist(searchString);

                        amazonResponse = RetrieveDocumentForUrl(urlString);

                        if (null != amazonResponse)
                            items = amazonResponse.SelectNodes("/amazon:ItemSearchResponse/amazon:Items/amazon:Item",
                                                               nsmgr);
                    }

                    // if we returned some results, lets do some processing
                    if (items != null)
                        if (items.Count > 0)
                        {
                            // check the first album, it's nearly always it
                            XmlNode image = CheckTracks(items[0], theAlbum, nsmgr);

                            // if it wasn't the first, hit up the second
                            if (null == image && items.Count >= 2)
                            {
                                image = CheckTracks(items[1], theAlbum, nsmgr);
                            }

                            // still no love?  hit up the third before we try text matching
                            if (null == image && items.Count >= 3)
                            {
                                image = CheckTracks(items[2], theAlbum, nsmgr);
                            }

                            // check the titles for the best textual match
                            if (null == image)
                            {
                                // take the best textual match
                                XmlNode lowestItem = CheckTitles(items, theAlbum, nsmgr);

                                // if the item met the threshold, this won't be null
                                if (null != lowestItem)
                                {
                                    // retrieve the image
                                    image = lowestItem.SelectSingleNode("amazon:LargeImage/amazon:URL", nsmgr);

                                    // if this is still null, it's because Amazon doesn't have an image for it, even though it has a record
                                    if (null == image)
                                    {
                                        return Album.NO_COVERART;
                                    }
                                }

                                //// then check whether the tracks match
                                //image = CheckTracks(lowestItem, theAlbum, nsmgr);
                            }

                            // check whether we found a match
                            if (null == image)
                            {
                                _log.Info("No matching album for: " + theAlbum.Title + ". Using default");
                                return Album.NO_COVERART;
                            }

                            // get the URL of the image
                            string imageUrl = image.InnerText;

                            // retrieve the image itself as a stream
                            Stream coverArtStream = GetImageFromURL(imageUrl);

                            if (null != coverArtStream)
                            {
                                // create the output stream
                                var outputFile = new FileStream(fullPath, FileMode.Create);

                                int Length = 256;
                                var buffer = new Byte[Length];
                                int bytesRead = coverArtStream.Read(buffer, 0, Length);
                                // write the required bytes
                                while (bytesRead > 0)
                                {
                                    outputFile.Write(buffer, 0, bytesRead);
                                    bytesRead = coverArtStream.Read(buffer, 0, Length);
                                }
                                coverArtStream.Close();
                                outputFile.Close();

                                return fullPath;
                            }

                            // use null to signify to try again next time
                            return null;
                        }
                        else
                        {
                            return null;
                        }
                }
            }
            // failed, we'll try again next time
            return null;
        }

        #endregion

        public string RetrieveImageURL(string artist, string title)
        {
            // formulate the REST request
            var request =
                WebRequest.Create("http://ecs.amazonaws.com/onca/xml?Service=AWSECommerceService&AWSAccessKeyId=" +
                                  AmazonConstants.AMAZON_AWS_KEY_ID +
                                  "&Operation=ItemSearch&SearchIndex=Music&Title=" + title +
                                  "&Artist=" + artist + "&ResponseGroup=Images&version=2008-08-19") as HttpWebRequest;

            if (request != null)
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    // load the response stream
                    var amazonResponse = new XmlDocument();
                    if (response != null)
                    {
                        amazonResponse.Load(response.GetResponseStream());
                    }

                    // retrieve the images
                    XmlNodeList images = amazonResponse.SelectNodes("/ItemSearchResponse/Items/Item/LargeImage/URL");

                    // we'll just take the first one
                    if (images != null)
                    {
                        XmlNode image = images[0];

                        // pass back the image
                        return image.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieve the Amazon RESTful URL for an artist/title search
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private string RetrieveRestURLWithArtistAndTitle(string artist, string title)
        {
            return "http://ecs.amazonaws.com/onca/xml?Service=AWSECommerceService&AWSAccessKeyId=" +
                   AmazonConstants.AMAZON_AWS_KEY_ID + "&Operation=ItemSearch&SearchIndex=Music&Artist=" +
                   artist + "&Title=" + title + "&ResponseGroup=Large&version=2008-08-19";
        }

        /// <summary>
        /// Retrieve the Amazon RESTful URL for an artist search
        /// </summary>
        /// <param name="artist"></param>
        /// <returns></returns>
        private string RetrieveRestURLWithArtist(string artist)
        {
            return "http://ecs.amazonaws.com/onca/xml?Service=AWSECommerceService&AWSAccessKeyId=" +
                   AmazonConstants.AMAZON_AWS_KEY_ID + "&Operation=ItemSearch&SearchIndex=Music&Artist=" +
                   artist + "&ResponseGroup=Large&version=2008-08-19";
        }

        /// <summary>
        /// Retrieve the Amazon RESTful URL for a keyword search
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private string RetrieveRestURLNoArtist(string keywords)
        {
            string keywordString = "&Keywords=" + keywords;

            return "http://ecs.amazonaws.com/onca/xml?Service=AWSECommerceService&AWSAccessKeyId=" +
                   AmazonConstants.AMAZON_AWS_KEY_ID + "&Operation=ItemSearch&SearchIndex=Music" + keywordString +
                   "&ResponseGroup=Large&version=2008-08-19";
        }

        /// <summary>
        /// Retrieve the XML document returned from Amazon when using the supplied URL
        /// </summary>
        /// <param name="urlString"></param>
        /// <returns></returns>
        private XmlDocument RetrieveDocumentForUrl(string urlString)
        {
            // setup the REST request
            var request = WebRequest.Create(urlString) as HttpWebRequest;
            if (request != null)
            {
                request.Timeout = 30000;
                //request.Proxy = _proxy;

                try
                {
                    var response = request.GetResponse() as HttpWebResponse;
                    // load the response stream
                    var amazonResponse = new XmlDocument();
                    if (response != null)
                    {
                        amazonResponse.Load(response.GetResponseStream());
                    }

                    // retrieve the items
                    return amazonResponse;
                }
                catch (WebException ex)
                {
                    _log.Info("Problem occurred when trying to use REST service", ex);
                }
            }

            return null;
        }

        /// <summary>
        /// Compare the titles of the albums returned by Amazon with the current Album
        /// from the DB.  Return the best match if it meets the threshold.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="theAlbum"></param>
        /// <param name="nsmgr"></param>
        /// <returns>Return </returns>
        private XmlNode CheckTitles(XmlNodeList items, Album theAlbum, XmlNamespaceManager nsmgr)
        {
            // start off with a really high number, the only way is down
            int lowestScore = 1000;
            XmlNode lowestItem = null;

            // work through each node
            foreach (XmlNode item in items)
            {
                // extract the title of this item
                XmlNode amazonTitle = item.SelectSingleNode("amazon:ItemAttributes/amazon:Title", nsmgr);

                string amazonTitleString = PrepareString(amazonTitle.InnerText);
                string albumTitle = PrepareString(theAlbum.Title);

                // calculate the distance between this and the actual album
                int distance = LevenshteinDistance.CalculateDistance(amazonTitleString, albumTitle);

                // store the reference if this is the best match
                if (distance < lowestScore)
                {
                    lowestItem = item;
                    lowestScore = distance;
                }
            }

            // send back the best match if it meets the threshold
            if (lowestScore < AmazonConstants.LEVENSHTEIN_THRESHOLD)
            {
                return lowestItem;
            }

            return null;
        }

        /// <summary>
        /// Compare the tracks of this item from Amazon with the tracks on Amazon
        /// </summary>
        /// <param name="amazonItem"></param>
        /// <param name="theAlbum"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        private XmlNode CheckTracks(XmlNode amazonItem, Album theAlbum, XmlNamespaceManager nsmgr)
        {
            XmlNode image = null;

            // extract the tracks out of the XML
            XmlNodeList amazonTracks = amazonItem.SelectNodes("amazon:Tracks/amazon:Disc/amazon:Track", nsmgr);

            // init the error marker
            int incorrectTracks = 0;

            foreach (Track track in theAlbum.Tracks)
            {
                // assume it's not here
                bool found = false;

                string ourTrackTitle = track.Title.ToLower();

                // check against each of the tracks
                if (amazonTracks != null)
                    foreach (XmlNode amazonTrack in amazonTracks)
                    {
                        string trackTitle = amazonTrack.InnerText.ToLower();

                        if (trackTitle.Contains(ourTrackTitle))
                        {
                            // perhaps match track number
                            found = true;
                            break;
                        }
                    }

                // if the track isn't located in the album, increment the incorrect list
                if (!found)
                {
                    incorrectTracks++;
                }
            }

            // if the incorrect tracks is an acceptable level
            if (incorrectTracks <= (theAlbum.Tracks.Count / 4 + 1))
            {
                // retrieve the image
                image = amazonItem.SelectSingleNode("amazon:LargeImage/amazon:URL", nsmgr);
            }

            return image;
        }

        /// <summary>
        /// Take the input string and scrape out all of the attributes which play havoc 
        /// with the Amazon searches
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        private string PrepareString(string searchString)
        {
            // make it lowercase as the search isn't case sensitive anyway
            searchString = searchString.ToLower();

            // remove the unwanted words
            foreach (string invalidWord in _invalidWords)
            {
                // replace them all with spaces, this doesn't mess with Amazon
                searchString = searchString.Replace(invalidWord, " ");
            }

            // remove trailing whitespace
            searchString = searchString.Trim();

            return searchString;
        }


        /// <summary>
        /// Retrieve an image from the supplied string
        /// </summary>
        /// <param name="urlString"></param>
        /// <returns></returns>
        private Stream GetImageFromURL(string urlString)
        {
            // initialize the return value
            Stream coverArtStream = null;

            try
            {
                var request = WebRequest.Create(urlString) as HttpWebRequest;

                // set the timeouts
                if (request != null)
                {
                    request.Timeout = 30000;
                    request.Proxy = Proxy;

                    // execute the request
                    var response = request.GetResponse() as HttpWebResponse;
                    if (response != null)
                    {
                        coverArtStream = response.GetResponseStream();
                    }
                }
            }
            catch (WebException ex)
            {
                _log.Info("Problem occurred when trying to get image stream from: " + urlString, ex);
            }

            return coverArtStream;
        }

        /// <summary>
        /// Generate an MD5 hash for this title.
        /// These are used for the titles of images.
        /// </summary>
        /// <returns></returns>
        private string GenerateMD5Hash(string str)
        {
            // Convert the string into bytes
            Encoder enc = Encoding.Unicode.GetEncoder();

            // Create a buffer large enough to hold the string
            var unicodeText = new byte[str.Length * 2];
            enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);

            // Now that we have a byte array we can ask the CSP to hash it
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(unicodeText);

            // Build the final string by converting each byte
            // into hex and appending it to a StringBuilder
            var sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }

            // And return it
            return sb.ToString();
        }

        #region IComponent Members

        public string Name
        {
            get { return "Amazon REST Service"; }
        }

        public void Initialise()
        {
            // setup the proxy - TODO make this configurable
            //IWebProxy proxy = new WebProxy(AudioModuleConstants.PROXY_HOSTNAME, 8080);
            //ICredentials credentials = new NetworkCredential(AudioModuleConstants.PROXY_USERNAME, AudioModuleConstants.PROXY_PASSWORD);
            //proxy.Credentials = credentials;
            //_proxy = proxy;
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