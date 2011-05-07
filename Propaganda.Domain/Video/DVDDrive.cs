using System;

namespace Propaganda.Domain.Video
{
    public class DVDDrive
    {
        private readonly string m_driveLetter;

        public DVDDrive(string driveLetter)
        {
            m_driveLetter = driveLetter;
        }

        #region IMenuItem Members

        public string ImagePath { get; set; }

        public string MenuTitle { get; set; }

        public Action SelectedAction
        {
            get { return null; }
        }

        #endregion

        #region IVideoFile Members

        public string Title
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string FileName
        {
            get { return "f:\\Top Gear\\Season 8\\Top Gear - [08x01] - 2006.05.07.avi"; }
            //get { return "dvd://" + m_driveLetter + ":\\"; }
            set { FileName = value; }
        }

        public string DisplayName
        {
            get { return m_driveLetter; }
        }

        public int LengthInSeconds
        {
            get { return 0; }
        }

        #endregion
    }
}