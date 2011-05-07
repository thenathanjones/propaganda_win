using System;
using System.Text;
using System.Windows;

namespace Propaganda.Domain.Video
{
    /// <summary>
    /// Representation of a Video file
    /// </summary>
    public class Video
    {
        /// <summary>
        /// Length of the Track in seconds
        /// </summary>
        private Duration TrackLength { get; set; }

        /// <summary>    
        /// Keep track of the last play position so it can be restarted later
        /// </summary>
        public Duration LastPlayPosition { get; set; }

        public Video() { }

        public Video(string title, string filename, Duration length)
        {
            Title = title;
            TrackLength = length;
            FileName = filename;
        }

        #region IVideoFile Members

        /// <summary>
        /// The title of this Track
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The full path to the Track itself
        /// </summary>
        public string FileName { get; set; }

        public string DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public int LengthInSeconds
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public string RetrieveDisplayName()
        {
            var builder = new StringBuilder();
            builder.Append(string.Format(" {0}", Title));
            return builder.ToString();
        }

        public int RetrieveLength()
        {
            return TrackLength.TimeSpan.Minutes*60 + TrackLength.TimeSpan.Seconds;
        }
    }
}