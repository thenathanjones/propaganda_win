using System;
using System.Collections.Generic;
using System.Threading;
using Propaganda.Core.Interfaces;
using Propaganda.Domain;
using Un4seen.Bass;

namespace Propaganda.Bass
{
    public class BassService : IService
    {
        /// <summary>
        /// Handle to the current stream
        /// </summary>
        private int _currentHandle;
        
        /// <summary>
        /// Handle to the current sync callback
        /// </summary>
        private int _currentSyncHandle;

        /// <summary>
        /// Current state of he player
        /// </summary>
        private PlayState _currentState;

        /// <summary>
        /// Length of the currently playing track 
        /// </summary>
        private string _currentTrackLength = "";

        /// <summary>
        /// Title of the currently playing track
        /// </summary>
        private string _currentTrackTitle = "";

        /// <summary>
        /// Timer to update the track position
        /// </summary>
        private ITimer _elapsedTimer;

        /// <summary>
        /// Current IPlayable objects enqueued
        /// </summary>
        private IList<IMediaFile> _currentPlaylist;

        /// <summary>
        /// Index in the current playlist of what is playing
        /// </summary>
        private int _currentIndex { get; set; }

        private SYNCPROC _ended;

        public BassService(ITimer elapsedTimer)
        {
            _elapsedTimer = elapsedTimer;
        }

        public void Initialise()
        {
            // register BASS to avoid the splash screen
            BassNet.Registration("iamfrank@bigpond.net.au", "2X293202311298");

            // initialise BASS
            Un4seen.Bass.Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero, new Guid());

            // initialise a blank playlist
            _currentPlaylist = new List<IMediaFile>();

            // update the track information every half a second
            _elapsedTimer.Initialise(500);
            _elapsedTimer.Tick += UpdateElapsed;
        }

        private void UpdateElapsed()
        {
            
            // retrieve the position from BASS
            var bytePosition = Un4seen.Bass.Bass.BASS_ChannelGetPosition(_currentHandle, BASSMode.BASS_POS_BYTES);

            // convert it into seconds (truncate it)
            var seconds = (int)Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(_currentHandle, bytePosition);
        }

        public void Dispose()
        {
            if (_currentHandle != 0)
            {
                StopPlay();
                Un4seen.Bass.Bass.BASS_StreamFree(_currentHandle);
            }

            Un4seen.Bass.Bass.BASS_Free();
        }

        public void ClearPlayList()
        {
            // reset the index
            _currentIndex = 0;
            // clear the list itself
            _currentPlaylist.Clear();
        }

        public void Play()
        {
            if (_currentState == PlayState.Paused)
            {
                // delegate to pause to "unpause"
                TogglePause();
                return;
            }

            // get the filename
            string filename = _currentPlaylist[_currentIndex].FileName;

            var handle = Un4seen.Bass.Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_AUTOFREE);

            // stop anything already playing
            if (_currentState == PlayState.Playing)
                StopPlay();

            // play the new song
            if (Un4seen.Bass.Bass.BASS_ChannelPlay(handle, false))
            {
                _currentState = PlayState.Playing;
                _currentHandle = handle;

                _ended = new SYNCPROC(PlaybackEnded);
                _currentSyncHandle = Un4seen.Bass.Bass.BASS_ChannelSetSync(_currentHandle,
                                                              BASSSync.BASS_SYNC_ONETIME | BASSSync.BASS_SYNC_END, 0,
                                                              _ended, IntPtr.Zero);

                // update the title, this will be picked up by the timer
                _currentTrackTitle = _currentPlaylist[_currentIndex].DisplayName;
                _elapsedTimer.Start();
            }
        }

        private void PlaybackEnded(int handle, int channel, int data, IntPtr user)
        {
            // flag the new state
            _currentState = PlayState.Ended;
            
            // clean up
            StopPlay();

            // attempt to play the next song
            var newIndex = _currentIndex + 1;
            if (newIndex < _currentPlaylist.Count)
                Next();
        }

        public void Play(IMediaFile playableObject)
        {
            _currentState = PlayState.Initialising;

            // clear the playlist first
            ClearPlayList();

            // enqueue the item
            Enqueue(playableObject);

            // stop files (even in paused state)
            StopPlay();

            // start play
            Play();
        }

        public void PlayList(IList<IMediaFile> playableObjects)
        {
            _currentState = PlayState.Initialising;

            // clear the playlist first
            ClearPlayList();

            // enqueue the item
            EnqueueList(playableObjects);

            // stop files (even in paused state)
            StopPlay();

            // start play
            Play();
        }

        public void Enqueue(IMediaFile playableObject)
        {
            _currentPlaylist.Add(playableObject);
        }

        public void EnqueueList(IList<IMediaFile> playableObjects)
        {
            // add these to the end of the playlist
            foreach (IMediaFile playableObject in playableObjects)
            {
                _currentPlaylist.Add(playableObject);
            }
        }

        public void StopPlay()
        {
            // if you're using this, and it's already stopped, clear the playlist
            if (_currentState == PlayState.Stopped)
                ClearPlayList();

            // if this is the first time, just stop
            if (_currentHandle != 0)
            {
                // remove the sync
                Un4seen.Bass.Bass.BASS_ChannelRemoveSync(_currentHandle, _currentSyncHandle);
                _currentSyncHandle = 0;
                _ended = null;

                // clean up
                Un4seen.Bass.Bass.BASS_ChannelStop(_currentHandle);
                Un4seen.Bass.Bass.BASS_StreamFree(_currentHandle);
                _currentHandle = 0;
            }

            _currentState = PlayState.Stopped;
        }

        public void Next()
        {
            // work out the next position
            int newIndex = _currentIndex + 1;

            // if we have enough room in the list to do this
            if (newIndex < _currentPlaylist.Count)
            {
                // increment the index and play
                _currentIndex++;
                Play();
            }
        }

        public void TogglePause()
        {
            if (_currentHandle != 0)
            {
                if (_currentState == PlayState.Playing)
                {
                    Un4seen.Bass.Bass.BASS_ChannelSlideAttribute(_currentHandle, BASSAttribute.BASS_ATTRIB_VOL, 0, 500);

                    // wait for the slide to finish
                    while (Un4seen.Bass.Bass.BASS_ChannelIsSliding(_currentHandle, BASSAttribute.BASS_ATTRIB_VOL))
                        Thread.Sleep(50);

                    if (Un4seen.Bass.Bass.BASS_ChannelPause(_currentHandle))
                    {
                        _currentState = PlayState.Paused;
                    }
                }
                else if (_currentState == PlayState.Paused)
                {
                    Un4seen.Bass.Bass.BASS_ChannelSlideAttribute(_currentHandle, BASSAttribute.BASS_ATTRIB_VOL, 1, 500);
                
                    if (Un4seen.Bass.Bass.BASS_ChannelPlay(_currentHandle, false))
                    {
                        _currentState = PlayState.Playing;
                    }
                }
            }
        }

        public void Previous()
        {
            // work out the next position
            int newIndex = _currentIndex - 1;

            // if we have enough room in the list to do this
            if (newIndex >= 0)
            {
                // increment the index and play
                _currentIndex--;
                Play();
            }
        }

        #region IComponent Members

        public string Name
        {
            get { return "Bass Service"; }
        }

        #endregion
    }

    public enum PlayState
    {
        Error,
        Initialising,
        Playing,
        Paused,
        Ended,
        Stopped
    }
}


