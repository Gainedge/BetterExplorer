namespace BExplorer.Shell {
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// An enhanced Queue
  /// </summary>
  /// <typeparam name="T">The type of the elements in queue</typeparam>
  public class QueueEx<T> {
    private readonly Queue<T> _Queue = new Queue<T>();

    /// <summary>Removes all objects</summary>
    public void Clear() => this._Queue.Clear();

    /// <summary>Gets the number of elements contained</summary>
    /// <returns>Returns the number of elements in queue</returns>
    public Int32 Count() => this._Queue.Count;

    /// <summary>
    /// Determines whether an element is in the System.Collections.Generic.Queue`1.
    /// </summary>
    /// <param name="item">The object to locate in the System.Collections.Generic.Queue`1. The value can be null for reference types.</param>
    /// <returns>true if item is found in the System.Collections.Generic.Queue`1; otherwise, false.</returns>
    public Boolean Contains(T item) => this._Queue.Contains(item);

    /// <summary>
    /// Removes and returns the object at the beginning of the System.Collections.Generic.Queue(Of T). while (queue.Count == 0) System.Threading.Monitor.Wait(queue);
    /// </summary>
    /// <returns>
    /// The object that is removed from the beginning of the System.Collections.Generic.Queue(Of T).
    /// </returns>
    /// <Exceptions>
    /// System.InvalidOperationException: The System.Collections.Generic.Queue(Of T) is empty.
    /// </Exceptions>
    public T Dequeue() => this._Queue.Dequeue();

    /// <summary>
    /// Adds an object to the end of the System.Collections.Generic.Queue[T] then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
    /// </summary>
    /// <param name="item">The object to add to the System.Collections.Generic.Queue[T]. The value can be null for reference types.</param>
    /// <param name="force">Force the item to update even if it has queue already contains it</param>
    /// <returns>Returns in the item enqued</returns>
    public Boolean Enqueue(T item, Boolean force = false) {
      if (!this._Queue.Contains(item) || force) {
        this._Queue.Enqueue(item);
        return true;
      } else {
        return false;
      }
    }
  }
}