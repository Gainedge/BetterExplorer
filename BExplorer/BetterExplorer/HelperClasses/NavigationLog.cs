using System.Collections.Generic;
using System.Linq;
using BExplorer.Shell;

namespace BetterExplorer {

	public class NavigationLog {

		#region Properties
		private List<ShellItem> HistoryItems = new List<ShellItem>();

		public List<ShellItem> HistoryItemsList { get { return HistoryItems; } }
		public int CurrentLocPos { get; set; }

		public bool CanNavigateBackwards { get { return !(CurrentLocPos == 0 || HistoryItems.Count == 0); } }
		public bool CanNavigateForwards { get { return !(CurrentLocPos == HistoryItems.Count - 1 || HistoryItems.Count == 0); } }

		public List<ShellItem> BackEntries {
			get {
				List<ShellItem> _BackEntries = new List<ShellItem>();
				if (CurrentLocPos != -1) {
					for (int i = 0; i < CurrentLocPos; i++) {
						_BackEntries.Add(HistoryItems[i]);
					}
				}
				//_BackEntries.Reverse();
				return _BackEntries;
			}
		}

		public List<ShellItem> ForwardEntries {
			get {
				List<ShellItem> _ForwardEntries = new List<ShellItem>();
				if (CurrentLocPos != -1) {
					for (int i = CurrentLocPos; i < HistoryItems.Count; i++) {
						_ForwardEntries.Add(HistoryItems[i]);
					}
				}
				//_BackEntries.Reverse();
				return _ForwardEntries;
			}
		}

		public ShellItem CurrentLocation {
			get {
				return HistoryItems.Count <= 0 ? null : HistoryItems[CurrentLocPos];
				//if (HistoryItems.Count == 0) 
				//	return null;				
				//else
				//	return HistoryItems[CurrentLocPos];
			}
			set {
				//NavigateOnwards(value);
				HistoryItems.Add(value);
				CurrentLocPos = HistoryItems.Count - 1;
			}
		}

		#endregion Properties

		public ShellItem NavigateBack() {
			CurrentLocPos--;
			return HistoryItems[HistoryItems.Count < CurrentLocPos - 1 ? CurrentLocPos : CurrentLocPos + 1];
		}

		public ShellItem NavigateForward() {
			CurrentLocPos++;
			return HistoryItems[CurrentLocPos];
		}

		public void ClearForwardItems() {
			for (int i = HistoryItems.ToArray().Count() - 1; i > CurrentLocPos; i--) {
				HistoryItems.RemoveAt(i);
			}
		}
		#region Public Methods



		#endregion



		public NavigationLog() {
			HistoryItems.Clear();
			CurrentLocPos = -1;
		}

		public void ImportData(NavigationLog log) {
			HistoryItems.Clear();
			HistoryItems.AddRange(log.HistoryItemsList);
			CurrentLocPos = HistoryItems.LastIndexOf(log.CurrentLocation);
		}

		/*
		private void NavigateOnwards(ShellItem loc) {
			HistoryItems.Add(loc);
			CurrentLocPos = HistoryItems.Count - 1;
		}
		*/


	}
}