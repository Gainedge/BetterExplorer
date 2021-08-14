using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell {
	public sealed class ObservableCollectionEx<T> : ObservableCollection<T>
		where T : INotifyPropertyChanged {

		public ObservableCollectionEx() {
			CollectionChanged += FullObservableCollectionCollectionChanged;
		}

		/// <summary>
		/// Add items into the collection
		/// </summary>
		/// <param name="pItems">The items to add into the collection</param>
		public ObservableCollectionEx(IEnumerable<T> pItems) : this() {
			foreach (var item in pItems) {
				this.Add(item);
			}
		}

		private void FullObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.NewItems != null) {
				foreach (Object item in e.NewItems) {
					((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
				}
			}
			if (e.OldItems != null) {
				foreach (Object item in e.OldItems) {
					((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
				}
			}
		}

		private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
			NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
			OnCollectionChanged(args);
		}
	}
}
