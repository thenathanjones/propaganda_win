using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Propaganda.Core.Interfaces
{
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// Initialise the timer
        /// </summary>
        /// <param name="elapsedTime">Time between elapsed events in milliseconds</param>
        void Initialise(int elapsedTime);

        /// <summary>
        /// Start timer events
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the timer events
        /// </summary>
        void Stop();

        /// <summary>
        /// Event raised when the timer has elapsed
        /// </summary>
        event Action Tick;
    }
}
