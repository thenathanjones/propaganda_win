using Propaganda.Domain.Audio;

namespace Propaganda.Core.Interfaces.Audio
{
    /// <summary>
    /// Interface specifying the different types of ripping which are needed to be done
    /// </summary>
    public interface IRippingService : IService
    {
        /// <summary>
        /// Rip a CD to FLAC lossless files from the provided drive
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <param name="numberTracks"></param>
        /// <returns>Temporary folder used for ripping</returns>
        string RipFLAC(string driveLetter, int numberTracks);

        /// <summary>
        /// Rip a CD to a 192kbps MP3 file from the provided drive
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <param name="numberTracks"></param>
        /// <returns>Temporary folder used for ripping</returns>
        string RipMP3192(string driveLetter, int numberTracks);

        /// <summary>
        /// Rip a CD to a 192kbps VBR MP3 file from the provided drive
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <param name="numberTracks"></param>
        /// <returns>Temporary folder used for ripping</returns>
        string RipMP3VBR192(string driveLetter, int numberTracks);

        /// <summary>
        /// Rip a CD to a 320kbps MP3 file from the provided drive
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <param name="numberTracks"></param>
        /// <returns>Temporary folder used for ripping</returns>
        string RipMP3320(string driveLetter, int numberTracks);

        /// <summary>
        /// Tag the file pointed to by the path with the information from the Track
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="theTrack"></param>
        void TagFile(string filePath, Track theTrack);

        /// <summary>
        /// Move the file from its temporary path, to its new location in the library
        /// </summary>
        /// <param name="tempPath"></param>
        /// <param name="libraryLocation"></param>
        /// <param name="theTrack"></param>
        void MoveToLibrary(string tempPath, string libraryLocation, Track theTrack);
    }
}