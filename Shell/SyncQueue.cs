using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell
{
	public class SyncQueue<T>
	{
		public class OverloadEventArgs : EventArgs
		{
			public Boolean IsOverloaded { get; private set; }
			public OverloadEventArgs(bool isOverloaded)
			{
				this.IsOverloaded = isOverloaded;
			}
		}
		private bool isHandled = false;
		private readonly Queue<T> queue = new Queue<T>();
		private readonly int maxSize;
		public SyncQueue(int maxSize) { this.maxSize = maxSize; }
		public event EventHandler<OverloadEventArgs> OnQueueOverload;

		public void Clear()
		{
			this.queue.Clear();
		}

		public void Enqueue(T item)
		{
			lock (queue)
			{
				if (queue.Count >= maxSize)
				{
					if (OnQueueOverload != null) OnQueueOverload.Invoke(this, new OverloadEventArgs(true));
				}
				queue.Enqueue(item);
				if (queue.Count == 1)
				{
					// wake up any blocked dequeue
					System.Threading.Monitor.PulseAll(queue);
				}
			}
		}
		public T Dequeue()
		{
			lock (queue)
			{
				while (queue.Count == 0)
				{
					// wait for item
					System.Threading.Monitor.Wait(queue);
				}
				T item = queue.Dequeue();

				if (queue.Count == maxSize - 1)
				{
					// wake up any blocked enqueue
					//System.Threading.Monitor.PulseAll(queue);
					if (OnQueueOverload != null) OnQueueOverload.Invoke(this, new OverloadEventArgs(false));
				}
				return item;
			}
		}
	}
}
