using System;
using System.Reflection;
using System.Threading;

namespace Propaganda.Core.Util.Threading
{
    internal class WorkerThread
    {
        /// <summary>
        /// The thread abstracted by this worker
        /// </summary>
        private readonly Thread _theThread;

        /// <summary>
        /// Flag controlling when to try allocating a task
        /// </summary>
        private readonly AutoResetEvent _waitForNewTask = new AutoResetEvent(false);

        /// <summary>
        /// Boolean flag as to whether or not this worker can accept a job
        /// </summary>
        private volatile bool _isAvailable = true;

        /// <summary>
        /// Current task to be performed by this thread
        /// </summary>
        private Delegate _task;

        /// <summary>
        /// Create this worker thread at the specified priority
        /// </summary>
        /// <param name="priority"></param>
        public WorkerThread(ThreadPriority priority)
        {
            _theThread = new Thread(MainLoop) {Priority = priority};
            _theThread.Start();
        }

        /// <summary>
        /// Return whether or not this is available
        /// </summary>
        public bool IsAvailable
        {
            get { return _isAvailable; }
        }

        /// <summary>
        /// Main loop waits for something to process than proceeds
        /// </summary>
        private void MainLoop()
        {
            while (true)
            {
                _waitForNewTask.WaitOne();
                try
                {
                    if (_task != null)
                    {
                        _task.DynamicInvoke();
                    }
                }
                catch (TargetInvocationException ex)
                {
                    // log an error here - this is usually because it's been stopped mid-run
                }
                finally
                {
                    _task = null;
                    _isAvailable = true;
                }
            }
        }

        /// <summary>
        /// Attempt to implement the worker item on this thread
        /// </summary>
        /// <param name="workItem"></param>
        public void ExecuteWorkItem(WorkerItem workItem)
        {
            if (!_isAvailable)
            {
                throw new InvalidOperationException("Thread is busy");
            }
            _task = workItem._delegate;
            _isAvailable = false;
            _waitForNewTask.Set();
        }

        /// <summary>
        /// Shutdown the thread
        /// </summary>
        public void Abort()
        {
            _theThread.Abort();
        }
    }
}