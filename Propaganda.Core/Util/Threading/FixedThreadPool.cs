using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Propaganda.Core.Util.Threading
{
    public class FixedThreadPool
    {
        /// <summary>
        /// Flag controlling when queue items are available
        /// </summary>
        private readonly AutoResetEvent _queueModified = new AutoResetEvent(false);

        /// <summary>
        /// Queue of items that need to be processed
        /// </summary>
        private readonly Queue _theQueue = Queue.Synchronized(new Queue());

        /// <summary>
        /// Worker threads used for this
        /// </summary>
        private readonly IList<WorkerThread> _workerThreads = new List<WorkerThread>();

        /// <summary>
        /// This flag is used to shut down the pool if need be
        /// </summary>
        public volatile bool _keepRunning = true;

        /// <summary>
        /// Create a custom thread pool with the provided number of threads running at the provided priority
        /// </summary>
        /// <param name="numThreads"></param>
        /// <param name="priority"></param>
        public FixedThreadPool(int numThreads, ThreadPriority priority)
        {
            // setup the threads
            for (int i = 0; i < numThreads; i++)
            {
                // create a new thread
                _workerThreads.Add(new WorkerThread(priority));
            }
        }

        /// <summary>
        /// Add the supplied item to the queue
        /// </summary>
        /// <param name="workerItem"></param>
        public void QueueWorkerItem(WorkerItem workerItem)
        {
            _theQueue.Enqueue(workerItem);
            _queueModified.Set();
        }

        /// <summary>
        /// Loop through all of the tasks until they're complete
        /// </summary>
        public void Start()
        {
            // loop until we've finished
            while (_keepRunning && _theQueue.Count > 0)
            {
                // if there is an available worker thread
                if (_workerThreads.Count(x => x.IsAvailable) > 0)
                {
                    var workItem = _theQueue.Dequeue() as WorkerItem;

                    var availableThread = _workerThreads.Where(x => x.IsAvailable).First();
                    availableThread.ExecuteWorkItem(workItem);
                }
                else
                {
                    // micro-sleep to prevent thrashing
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Shutdown all the threads
        /// </summary>
        public void Shutdown()
        {
            _keepRunning = false;

            foreach (var thread in _workerThreads)
            {
                thread.Abort();
            }
        }
    }
}