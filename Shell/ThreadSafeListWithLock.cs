using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BExplorer.Shell {
	public class ThreadSafeListWithLock<T> : IList<T> {
		private List<T> internalList;

		private readonly object lockList = new object();

		public ThreadSafeListWithLock() {
			internalList = new List<T>();
		}

		// Other Elements of IList implementation

		public IEnumerator<T> GetEnumerator() {
			return Clone().GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return Clone().GetEnumerator();
		}

		public List<T> Clone() {
			List<T> threadClonedList = new List<T>();

			lock (lockList) {
				internalList.ForEach(element => { threadClonedList.Add(element); });
			}

			return (threadClonedList);
		}

		public void Add(T item) {
			lock (lockList) {
				internalList.Add(item);
			}
		}

		public void AddRange(T[] items) {
			lock (lockList) {
				internalList.AddRange(items);
			}
		}

		public bool Remove(T item) {
			bool isRemoved;

			lock (lockList) {
				isRemoved = internalList.Remove(item);
			}

			return (isRemoved);
		}

		public void Clear() {
			lock (lockList) {
				internalList.Clear();
			}
		}

		public bool Contains(T item) {
			bool containsItem;

			lock (lockList) {
				containsItem = internalList.Contains(item);
			}

			return (containsItem);
		}

		public void CopyTo(T[] array, int arrayIndex) {
			lock (lockList) {
				internalList.CopyTo(array, arrayIndex);
			}
		}

		public int Count
		{
			get
			{
				int count;

				lock ((lockList)) {
					count = internalList.Count;
				}

				return (count);
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public int IndexOf(T item) {
			int itemIndex;

			lock ((lockList)) {
				itemIndex = internalList.IndexOf(item);
			}

			return (itemIndex);
		}

		public void Insert(int index, T item) {
			lock ((lockList)) {
				internalList.Insert(index, item);
			}
		}

		public void RemoveAt(int index) {
			lock ((lockList)) {
				internalList.RemoveAt(index);
			}
		}

		public T this[int index]
		{
			get
			{
				lock ((lockList)) {
					return internalList[index];
				}
			}
			set
			{
				lock ((lockList)) {
					internalList[index] = value;
				}
			}
		}
	}
}
