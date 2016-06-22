using System.Threading;

namespace BEHelper
{
    [System.Obsolete("Not used", true)]
    public class AsyncContext : IAsyncContext
    {
        private readonly SynchronizationContext _asynchronizationContext;

        /// <summary>Get the context of the creator thread</summary>
        public SynchronizationContext AsynchronizationContext => _asynchronizationContext;

        /// <summary>Test if the current executing thread is the creator thread</summary>
        public bool IsAsyncCreatorThread => SynchronizationContext.Current == AsynchronizationContext;

        /// <summary>Constructor - Save the context of the creator/current thread</summary>
        public AsyncContext()
        {
            _asynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// Post a call to the specified method on the creator thread
        /// </summary>
        /// <param name="callback">Method that is to be called</param>
        /// <param name="state">Method parameter/state</param>
        public void AsyncPost(SendOrPostCallback callback, object state)
        {
            if (IsAsyncCreatorThread)
                callback(state); // Call the method directly
            else
                AsynchronizationContext.Post(callback, state);  // Post on creator thread
        }
    }
}