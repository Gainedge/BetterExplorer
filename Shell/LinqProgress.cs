using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {

	public class ProgressArgs<T> : EventArgs {
		public T Item { get; private set; }

		public ProgressArgs(T item) {
			this.Item = item;
		}
	}

	public class ProgressContext<T> : IEnumerable<T> {
		private IEnumerable<T> source;

		public ProgressContext(IEnumerable<T> source) {
			this.source = source;
		}

		public event EventHandler<ProgressArgs<T>> UpdateProgress;

		protected virtual void OnUpdateProgress(T item) {
			EventHandler<ProgressArgs<T>> handler = this.UpdateProgress;
			if (handler != null)
				handler(this, new ProgressArgs<T>(item));
		}

		public IEnumerator<T> GetEnumerator() {
			//int count = 0;
			foreach (var item in source) {
				// The yield holds execution until the next iteration,
				// so trigger the update event first.
				OnUpdateProgress(item);
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}
