using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {

	[Obsolete("Can't we just use Queue directly")]
	[System.Diagnostics.DebuggerStepThrough]
	public class SyncQueue<T> {
		/*
		public class OverloadEventArgs : EventArgs {
			public Boolean IsOverloaded { get; private set; }
			public OverloadEventArgs(bool isOverloaded) {
				this.IsOverloaded = isOverloaded;
			}
		}
		*/
		//private bool isHandled = false;
		public readonly Queue<T> queue = new Queue<T>();
		/*
		private readonly int maxSize;
		public SyncQueue(int maxSize) { this.maxSize = maxSize; }
		*/
		////private event EventHandler<OverloadEventArgs> OnQueueOverload;

		public void Clear() {
			this.queue.Clear();
		}

		/// <summary>
		/// Adds an object to the end of the System.Collections.Generic.Queue<T> then runs System.Threading.Monitor.PulseAll(queue) when queue.Count == 1;
		/// </summary>
		/// <param name="item">The object to add to the System.Collections.Generic.Queue<T>. The value can be null for reference types.</param>
		public void Enqueue(T item) {
			lock (queue) {
				/*
				if (queue.Count >= maxSize) {
					//if (OnQueueOverload != null) OnQueueOverload.Invoke(this, new OverloadEventArgs(true));
				}
				*/
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

				/*
				if (queue.Count == maxSize - 1) {
					//// wake up any blocked enqueue
					////System.Threading.Monitor.PulseAll(queue);
					//if (OnQueueOverload != null) OnQueueOverload.Invoke(this, new OverloadEventArgs(false));
				}
				*/
				return item;
			}
		}
	}
}
