using System.Collections.Generic;
using System.Linq;
using BExplorer.Shell;

namespace BetterExplorer {

	public class NavigationLog {

		#region Properties
		//private List<ShellItem> HistoryItems = new List<ShellItem>();

		public List<ShellItem> HistoryItemsList { get; private set; }
		public int CurrentLocPos { get; set; }

		public bool CanNavigateBackwards { get { return !(CurrentLocPos == 0 || HistoryItemsList.Count == 0); } }
		public bool CanNavigateForwards { get { return !(CurrentLocPos == HistoryItemsList.Count - 1 || HistoryItemsList.Count == 0); } }

		/*
		public List<ShellItem> BackEntries {
			get {
				List<ShellItem> _BackEntries = new List<ShellItem>();
				if (CurrentLocPos != -1) {
					for (int i = 0; i < CurrentLocPos; i++) {
						_BackEntries.Add(HistoryItemsList[i]);
					}
				}
				return _BackEntries;
			}
		}
		*/

		public List<ShellItem> ForwardEntries {
			get {
				List<ShellItem> _ForwardEntries = new List<ShellItem>();
				if (CurrentLocPos != -1) {
					for (int i = CurrentLocPos; i < HistoryItemsList.Count; i++) {
						_ForwardEntries.Add(HistoryItemsList[i]);
					}
				}
				return _ForwardEntries;
			}
		}

		public ShellItem CurrentLocation {
			get {
				return HistoryItemsList.Count == 0 ? null : HistoryItemsList[CurrentLocPos];
			}
			set {
				HistoryItemsList.Add(value);
				CurrentLocPos = HistoryItemsList.Count - 1;
			}
		}

		#endregion Properties



		public NavigationLog() {
			HistoryItemsList = new List<ShellItem>();
			CurrentLocPos = -1;
		}


		public ShellItem NavigateBack() {
			CurrentLocPos--;
			return HistoryItemsList[CurrentLocPos < 0 ? 0 : CurrentLocPos];
		}

		public ShellItem NavigateForward() {
			CurrentLocPos++;
			return HistoryItemsList[CurrentLocPos > HistoryItemsList.Count - 1 ? HistoryItemsList.Count - 1 : CurrentLocPos];
		}

		public void ClearForwardItems() {
			for (int i = HistoryItemsList.ToArray().Count() - 1; i > CurrentLocPos; i--) {
				HistoryItemsList.RemoveAt(i);
			}
		}



		public void ImportData(NavigationLog log) {
			HistoryItemsList.Clear();
			HistoryItemsList.AddRange(log.HistoryItemsList);
			CurrentLocPos = HistoryItemsList.LastIndexOf(log.CurrentLocation);
		}

		/*
		private void NavigateOnwards(ShellItem loc) {
			HistoryItems.Add(loc);
			CurrentLocPos = HistoryItems.Count - 1;
		}
		*/

	}
}