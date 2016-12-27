#define ConcurrentQueue

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {

	[System.Diagnostics.DebuggerStepThrough]
	public class SyncQueue<T> {
		private readonly Queue<T> queue = new Queue<T>();
		public void Clear() => this.queue.Clear();
		public int Count() => this.queue.Count;
		public Boolean Contains(T item) => queue.Contains(item);


		/// <summary>
		/// Adds an object to the end of the System.Collections.Generic.Queue[T] then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
		/// </summary>
		/// <param name="item">The object to add to the System.Collections.Generic.Queue[T]. The value can be null for reference types.</param>
		/// <param name="force">Force the item to update even if it has queue already contains it</param>
		public void Enqueue(T item, Boolean force = false) {
			lock (queue) {
				if (force || !queue.Contains(item)) {
					queue.Enqueue(item);

					if (queue.Count == 1) {
						// wake up any blocked dequeue
						System.Threading.Monitor.PulseAll(queue);
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
			lock (queue) {
				while (queue.Count == 0) {
					// wait for item
					System.Threading.Monitor.Wait(queue);
				}

				return queue.Dequeue();
			}
		}
	}

	public class QueueEx<T> {
		private readonly Queue<T> queue = new Queue<T>();
		/// <summary>Removes all objects</summary>
		public void Clear() => this.queue.Clear();

		/// <summary>Gets the number of elements contained</summary>
		public int Count() => this.queue.Count;

		/// <summary>
		/// Determines whether an element is in the System.Collections.Generic.Queue`1.
		/// </summary>
		/// <param name="item">The object to locate in the System.Collections.Generic.Queue`1. The value can be null for reference types.</param>
		/// <returns>true if item is found in the System.Collections.Generic.Queue`1; otherwise, false.</returns>
		public Boolean Contains(T item) => queue.Contains(item);

		/// <summary>
		/// Removes and returns the object at the beginning of the System.Collections.Generic.Queue(Of T). while (queue.Count == 0) System.Threading.Monitor.Wait(queue);
		/// </summary>
		/// <returns>
		/// The object that is removed from the beginning of the System.Collections.Generic.Queue(Of T).
		/// </returns>
		/// <Exceptions>
		/// System.InvalidOperationException: The System.Collections.Generic.Queue(Of T) is empty.
		/// </Exceptions>
		public T Dequeue() => queue.Dequeue();

		/// <summary>
		/// Adds an object to the end of the System.Collections.Generic.Queue[T] then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
		/// </summary>
		/// <param name="item">The object to add to the System.Collections.Generic.Queue[T]. The value can be null for reference types.</param>
		/// <param name="force">Force the item to update even if it has queue already contains it</param>
		public Boolean Enqueue(T item, Boolean force = false) {
			if (!queue.Contains(item) || force) {
				queue.Enqueue(item);
				return true;
			}
			else {
				return false;
			}
		}


	}

	//	public class ConQueue<T> {
	//#if ConcurrentQueue

	//		private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

	//		public void Clear() {
	//			T Item;
	//			while (queue.TryDequeue(out Item)) {

	//			}
	//		}

	//		/// <summary>
	//		/// Adds an object to the end of the System.Collections.Generic.Queue[T] then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
	//		/// </summary>
	//		/// <param name="item">The object to add to the System.Collections.Generic.Queue[T]. The value can be null for reference types.</param>
	//		public void Enqueue(T item) {
	//			lock (queue) {
	//				queue.Enqueue(item);
	//				if (queue.Count == 1) {
	//					// wake up any blocked dequeue
	//					System.Threading.Monitor.PulseAll(queue);
	//				}
	//			}
	//		}

	//		/// <summary>
	//		/// Removes and returns the object at the beginning of the System.Collections.Generic.Queue(Of T). while (queue.Count == 0) System.Threading.Monitor.Wait(queue);
	//		/// </summary>
	//		/// <returns>
	//		/// The object that is removed from the beginning of the System.Collections.Generic.Queue(Of T).
	//		/// </returns>
	//		/// <Exceptions>
	//		/// System.InvalidOperationException: The System.Collections.Generic.Queue(Of T) is empty.
	//		/// </Exceptions>
	//		public T Dequeue() {
	//			T Result;
	//			lock (queue) {
	//				if (queue.Count == 0) {
	//					System.Threading.Monitor.Wait(queue);
	//				}
	//				if (!queue.TryDequeue(out Result)) {
	//					//this.ToString();

	//					//throw new ApplicationException("Why!!");
	//				}
	//			}

	//			return Result;
	//		}

	//#else
	//		public readonly Queue<T> queue = new Queue<T>();

	//		public void Clear() {
	//			this.queue.Clear();
	//		}

	//		/// <summary>
	//		/// Adds an object to the end of the System.Collections.Generic.Queue<T> then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
	//		/// </summary>
	//		/// <param name="item">The object to add to the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
	//		public void Enqueue(T item) {
	//			lock (queue) {
	//				queue.Enqueue(item);

	//				if (queue.Count == 1) {
	//					// wake up any blocked dequeue
	//					System.Threading.Monitor.PulseAll(queue);
	//				}
	//			}
	//		}

	//		/// <summary>
	//		/// Removes and returns the object at the beginning of the System.Collections.Generic.Queue(Of T). while (queue.Count == 0) System.Threading.Monitor.Wait(queue);
	//		/// </summary>
	//		/// <returns>
	//		/// The object that is removed from the beginning of the System.Collections.Generic.Queue(Of T).
	//		/// </returns>
	//		/// <Exceptions>
	//		/// System.InvalidOperationException: The System.Collections.Generic.Queue(Of T) is empty.
	//		/// </Exceptions>
	//		public T Dequeue() {
	//			lock (queue) {
	//				while (queue.Count == 0) {
	//					// wait for item
	//					System.Threading.Monitor.Wait(queue);
	//				}

	//				return queue.Dequeue();
	//			}
	//		}
	//#endif
	//	}

}
