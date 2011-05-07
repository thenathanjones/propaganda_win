namespace Propaganda.Audio.Library.CDDB.FreeDB
{
    /// <summary>
    /// Class containing all the constants for interacting with FreeDB
    /// </summary>
    internal class FreeDBConstants
    {
        /// <summary>
        /// Protocol level used
        /// </summary>
        internal const string PROTOCOL = "6";

        /// <summary>
        /// URL of the main FreeDB site
        /// </summary>
        internal const string URL_FREEDB_MAIN = "freedb.freedb.org";

        /// <summary>
        /// Path for HTTP-access
        /// </summary>
        internal const string URL_HTTP_ACCESS = "/~cddb/cddb.cgi";

        /// <summary>
        /// User name provided to the FreeDB site
        /// </summary>
        internal const string USER_NAME = "Propaganda-User";

        #region Commands

        /// <summary>
        /// Main prefix for commands
        /// </summary>
        internal const string COMMAND = "cmd=";

        /// <summary>
        /// Command to specify the user information for this query
        /// </summary>
        internal const string COMMAND_HELLO = "hello";

        /// <summary>
        /// Command to specify what protocol we're using
        /// </summary>
        internal const string COMMAND_PROTOCOL = "proto";

        /// <summary>
        /// Command for the query request
        /// </summary>
        internal const string COMMAND_QUERY = "cddb+query";

        /// <summary>
        /// Command for the read request
        /// </summary>
        internal const string COMMAND_READ = "cddb+read";

        /// <summary>
        /// Command for the sites request
        /// </summary>
        internal const string COMMAND_SITES = "sites";

        /// <summary>
        /// Command used to signify the end of a response
        /// </summary>
        internal const string COMMAND_TERMINATOR = ".";

        #endregion
    }

    /// <summary>
    /// Response codes from the FreeDB HTTP service
    /// </summary>
    internal enum ResponseCode
    {
        // exact match returned
        RESPONSE_CODE_200 = 200,
        // no matches found
        RESPONSE_CODE_202 = 202,
        // everything is AOK
        RESPONSE_CODE_210 = 210,
        // possible matches found
        RESPONSE_CODE_211 = 211,
        // no site information available
        RESPONSE_CODE_401 = 401,
        // server error
        RESPONSE_CODE_402 = 402,
        // corrupted DB entry
        RESPONSE_CODE_403 = 403,
        // no handshake
        RESPONSE_CODE_409 = 409,
        // invalid parameters/command
        RESPONSE_CODE_500 = 500,
        // used for everything else
        RESPONSE_CODE_DEFAULT = -1
    }
}