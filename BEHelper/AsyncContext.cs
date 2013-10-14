using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BEHelper
{
  public class AsyncContext : IAsyncContext
  {
    private readonly SynchronizationContext _asynchronizationContext;

    /// 
    /// Constructor - Save the context of the creator/current thread
    /// 
    public AsyncContext()
    {
      _asynchronizationContext = SynchronizationContext.Current;
    }

    /// 
    /// Get the context of the creator thread
    /// 
    public SynchronizationContext AsynchronizationContext
    {
      get { return _asynchronizationContext; }
    }

    /// 
    /// Test if the current executing thread is the creator thread
    /// 
    public bool IsAsyncCreatorThread
    {
      get { return SynchronizationContext.Current == AsynchronizationContext; }
    }

    /// 
    /// Post a call to the specified method on the creator thread
    /// 
    /// Method that is to be called
    /// Method parameter/state
    public void AsyncPost(SendOrPostCallback callback, object state)
    {
      if (IsAsyncCreatorThread)
        callback(state); // Call the method directly
      else
        AsynchronizationContext.Post(callback, state);  // Post on creator thread
    }
  }
}
