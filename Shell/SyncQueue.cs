using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {

	//[System.Diagnostics.DebuggerStepThrough]
	public class SyncQueue<T> {
		public readonly Queue<T> queue = new Queue<T>();
		public void Clear() {
			this.queue.Clear();
		}

		/// <summary>
		/// Adds an object to the end of the System.Collections.Generic.Queue<T> then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
		/// </summary>
		/// <param name="item">The object to add to the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
		public void Enqueue(T item) {
			lock (queue) {
				queue.Enqueue(item);
				if (queue.Count == 1) {
					// wake up any blocked dequeue
					System.Threading.Monitor.PulseAll(queue);
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
				T item = queue.Dequeue();

				return item;
			}
		}
	}
}

/*
namespace BExplorer.Shell {

	//[System.Diagnostics.DebuggerStepThrough]
	public class SyncQueue<T> {
		public readonly System.Collections.Concurrent.ConcurrentQueue<T> queue = new System.Collections.Concurrent.ConcurrentQueue<T>();

		public void Clear() {
			//this.queue.Clear();
			T item;
			while (queue.TryDequeue(out item)) {
				// do nothing
			}
		}

		/// <summary>
		/// Adds an object to the end of the System.Collections.Generic.Queue<T> then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
		/// </summary>
		/// <param name="item">The object to add to the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
		public void Enqueue(T item) {
			//lock (queue) {
			//if (!queue.Contains(item))
			queue.Enqueue(item);
			//if (queue.Count == 1) {
			//	// wake up any blocked dequeue
			//	System.Threading.Monitor.PulseAll(queue);
			//}
			//}
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
			//lock (queue) {
			//while (queue.Count == 0) {
			//	// wait for item
			//	System.Threading.Monitor.Wait(queue);
			//}
			//T item = queue.Dequeue();

			T item;
			if (queue.TryDequeue(out item)) {

			}		
			return item;
			//}
		}
	}
}
*/