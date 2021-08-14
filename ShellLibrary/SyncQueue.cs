namespace BExplorer.Shell {

  using System;
  using System.Collections.Generic;

  /// <summary>
  /// A Queue for sync actions
  /// </summary>
  /// <typeparam name="T">The type of the elements in queue</typeparam>
  [System.Diagnostics.DebuggerStepThrough]
  public class SyncQueue<T> {
    private readonly Queue<T> _Queue = new Queue<T>();

    public void Clear() => this._Queue.Clear();

    public Int32 Count() => this._Queue.Count;

    public Boolean Contains(T item) => this._Queue.Contains(item);

    /// <summary>
    /// Adds an object to the end of the System.Collections.Generic.Queue[T] then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
    /// </summary>
    /// <param name="item">The object to add to the System.Collections.Generic.Queue[T]. The value can be null for reference types.</param>
    /// <param name="force">Force the item to update even if it has queue already contains it</param>
    public void Enqueue(T item, Boolean force = false) {
      lock (this._Queue) {
        if (force || !this._Queue.Contains(item)) {
          this._Queue.Enqueue(item);

          if (this._Queue.Count == 1) {
            // wake up any blocked dequeue
            System.Threading.Monitor.PulseAll(this._Queue);
          }
        }
      }

    }

    /// <summary>
    /// Removes and returns the object at the beginning of the System.Collections.Generic.Queue(Of T). while (queue.Count == 0) System.Threading.Monitor.Wait(queue);
    /// </summary>
    /// <returns>
    /// The object that is removed from the beginning of the System.Collections.Generic.Queue(Of T).
    /// </returns>
    /// <Exceptions>
    /// System.InvalidOperationException: The System.Collections.Generic.Queue(Of T) is empty.
    /// </Exceptions>
    public T Dequeue() {
      lock (this._Queue) {
        while (this._Queue.Count == 0) {
          // wait for item
          System.Threading.Monitor.Wait(this._Queue);
        }

        return this._Queue.Dequeue();
      }
    }
  }
}
