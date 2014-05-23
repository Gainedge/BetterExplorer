using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace BEHelper {
	public class AsyncObservableCollection<T> : ObservableCollection<T>, IAsyncContext {
		private readonly AsyncContext _asyncContext = new AsyncContext();

		#region IAsyncContext Members
		public SynchronizationContext AsynchronizationContext { get { return _asyncContext.AsynchronizationContext; } }
		public bool IsAsyncCreatorThread { get { return _asyncContext.IsAsyncCreatorThread; } }
		public void AsyncPost(SendOrPostCallback callback, object state) { _asyncContext.AsyncPost(callback, state); }
		#endregion

		public AsyncObservableCollection() { }
		public AsyncObservableCollection(IEnumerable<T> list) : base(list) { }
		public AsyncObservableCollection(List<T> list) : base(list) { }

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
			AsyncPost(RaiseCollectionChanged, e);
		}

		private void RaiseCollectionChanged(object param) {
			base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
			AsyncPost(RaisePropertyChanged, e);
		}

		private void RaisePropertyChanged(object param) {
			base.OnPropertyChanged((PropertyChangedEventArgs)param);
		}
	}
}
