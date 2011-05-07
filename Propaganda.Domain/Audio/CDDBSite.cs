using System;

namespace Propaganda.Domain.Audio
{
    /// <summary>
    /// Meta information for a CDDB site
    /// </summary>
    public class CDDBSite
    {
        /// <summary>
        /// Only constructor for a CDDB site
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name"></param>
        /// <param name="portNo"></param>
        /// <param name="protocol"></param>
        public CDDBSite(string url, string name, string portNo, string protocol)
        {
            URL = url;
            Name = name;
            PortNo = Int32.Parse(portNo);
            switch (protocol)
            {
                case "cddbp":
                    Protocol = Protocol.CDDBP;
                    break;
                case "http":
                    Protocol = Protocol.HTTP;
                    break;
                default:
                    Protocol = Protocol.HTTP;
                    break;
            }
        }

        /// <summary>
        /// URL of this CDDB site
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Name of this CDDB site
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Port number to access this site
        /// </summary>
        public int PortNo { get; set; }

        /// <summary>
        /// Protocol used by this site
        /// </summary>
        public Protocol Protocol { get; set; }
    }

    /// <summary>
    /// Protocols used to access this site
    /// </summary>
    public enum Protocol
    {
        CDDBP,
        HTTP
    }
}