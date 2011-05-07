using System.Collections.Generic;

namespace Propaganda.Core.Interfaces.Media
{
    /// <summary>
    /// Interface describing a media player
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// Clear the internal playlist
        /// </summary>
        void ClearPlayList();

        /// <summary>
        /// Play the currently selected item
        /// </summary>
        void Play();

        /// <summary>
        /// Play the selected right now
        /// </summary>
        /// <param name="playableObject"></param>
        void Play(IPlayable playableObject);

        /// <summary>
        /// Enqueue and play the selected items
        /// </summary>
        /// <param name="playableObjects"></param>
        void PlayList(IList<IPlayable> playableObjects);

        /// <summary>
        /// Add the provided <code>IPlayable</code> object to the playlist
        /// </summary>
        /// <param name="playableObject"></param>
        void Enqueue(IPlayable playableObject);

        /// <summary>
        /// Add the provided <code>IPlayable</code> objects to the playlist
        /// </summary>
        /// <param name="playableObjects"></param>
        void EnqueueList(IList<IPlayable> playableObjects);

        /// <summary>
        /// Stop playing.  
        /// </summary>
        void StopPlay();

        /// <summary>
        /// Play the next <code>IPlayable</code> object in the playlist
        /// </summary>
        void Next();

        /// <summary>
        /// Toggle pause
        /// </summary>
        void TogglePause();

        /// <summary>
        /// Play the previous <code>IPlayable</code> object in the playlist
        /// </summary>
        void Previous();
    }
}