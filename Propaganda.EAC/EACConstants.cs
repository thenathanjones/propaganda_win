namespace Propaganda.EAC
{
    /// <summary>
    /// Constants for interacting with EAC
    /// </summary>
    internal class EACConstants
    {
        #region EAC Command Line options

        /// <summary>
        /// Start the ripping process automatically
        /// </summary>
        internal const string OPTION_AUTOSTART = "-extractmp3";

        /// <summary>
        /// Close EAC when this is complete
        /// </summary>
        internal const string OPTION_CLOSE = "-close";

        /// <summary>
        /// Specify which drive to use
        /// </summary>
        internal const string OPTION_DRIVE = "-drive";

        /// <summary>
        /// Ignore the CD Text on the CD
        /// </summary>
        internal const string OPTION_NOCDTEXT = "-nocdtext";

        #endregion
    }
}