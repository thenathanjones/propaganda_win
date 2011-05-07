using System;

namespace Propaganda.Core.Util.Threading
{
    public class WorkerItem
    {
        /// <summary>
        /// Create a new WorkerItem 
        /// </summary>
        /// <param name="delegateItem"></param>
        public WorkerItem(Action delegateItem)
        {
            _delegate = delegateItem;
        }

        /// <summary>
        /// Delegate that needs to be called for this worker
        /// </summary>
        public Action _delegate { get; private set; }
    }
}