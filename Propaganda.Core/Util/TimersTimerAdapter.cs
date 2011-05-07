using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Propaganda.Core.Interfaces;
using System.Timers;

namespace Propaganda.Core.Util
{
    public class TimersTimerAdapter : ITimer
    {
        #region ITimer Members

        public void Initialise(int elapsedTime)
        {
            _timer = new Timer();
            _timer.Interval = (double)elapsedTime;
            _timer.Elapsed += (o, e) => { if (Tick != null) Tick(); };
        }

        public void Start()
        {
            if (_timer != null)
                _timer.Start();
        }

        public void Stop()
        {
            if (_timer != null)
                _timer.Stop();
        }

        public event Action Tick;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _timer.Dispose();
        }

        #endregion

        private Timer _timer;
    }
}
