using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BEHelper
{
  public interface IAsyncContext
  {
    /// <summary>
    /// Get the context of the creator thread
    /// </summary>
    SynchronizationContext AsynchronizationContext { get; }

    /// <summary>
    /// Test if the current executing thread is the creator thread
    /// </summary>
    bool IsAsyncCreatorThread { get; }

    /// <summary>
    /// Post a call to the specified method on the creator thread
    /// </summary>
    /// <param name="callback">Method that is to be called</param>
    /// <param name="state">Method parameter/state</param>
    void AsyncPost(SendOrPostCallback callback, object state);
  }
}
