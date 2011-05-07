
using System.Collections.Generic;
using Propaganda.Domain.Audio;

namespace Propaganda.Audio
{
    internal abstract class AudioConstants
    {
        /// <summary>
        /// Supported lossless file types
        /// </summary>
        public static readonly IEnumerable<string> SUPPORTED_LOSSLESS = new List<string>() { ".wav", ".flac", ".ape" };

        /// <summary>
        /// Supported lossy file types
        /// </summary>
        public static readonly IEnumerable<string> SUPPORTED_LOSSY = new List<string>() { ".mp3", ".m4a", ".ogg" };

        

        
    }
}