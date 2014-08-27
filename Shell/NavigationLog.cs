using System.Collections.Generic;
using System.Linq;

namespace BExplorer.Shell {

	/// <summary>Represents a log of where the user has navigated through the file system</summary>
	public class NavigationLog {

		#region Properties

		/// <summary>The locations the user has been to</summary>
		public List<ShellItem> HistoryItemsList { get; private set; }

		/// <summary>the position/place/item the user is currently at in the <see cref="HistoryItemsList">History</see> </summary>
		public int CurrentLocPos { get; set; }

		/// <summary>Can the user navigate backwards?</summary>
		public bool CanNavigateBackwards { get { return !(CurrentLocPos == 0 || HistoryItemsList.Count == 0); } }

		// <summary>Can the user navigate forwards?</summary>
		public bool CanNavigateForwards { get { return !(CurrentLocPos == HistoryItemsList.Count - 1 || HistoryItemsList.Count == 0); } }

		private List<ShellItem> ForwardEntries {
			//TODO: I think these are actually Backwards Entries not Forwards
			get {
				//TODO: Test
				return HistoryItemsList.Skip(CurrentLocPos).ToList();

				/*
				var _ForwardEntries = new List<ShellItem>();
				if (CurrentLocPos != -1) {
					for (int i = CurrentLocPos; i < HistoryItemsList.Count; i++) {
						_ForwardEntries.Add(HistoryItemsList[i]);
					}
				}
				return _ForwardEntries;
				*/
			}
		}

		// <summary>Gets the current ShellItem in HistoryItemsList or Sets the item by adding it to HistoryItemsList and</summary>
		/// <summary>Represents the current ShellItem the user is at</summary>
		/// <remarks>
		/// Gets the item from <see cref="HistoryItemsList"/> at index <see cref="CurrentLocPos"/> (or null if Count = 0).
		/// Sets the Location by adding the ShellItem to <see cref="HistoryItemsList"/> and setting it as the <see cref="CurrentLocPos"/>
		/// </remarks>
		public ShellItem CurrentLocation {
			get {
				return HistoryItemsList.Any() ? HistoryItemsList[CurrentLocPos] : null;
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

		/// <summary>Navigates backwards one item</summary>
		public ShellItem NavigateBack() {
			CurrentLocPos--;
			return HistoryItemsList[CurrentLocPos < 0 ? 0 : CurrentLocPos];
		}

		/// <summary>Navigates forwards one item</summary>
		public ShellItem NavigateForward() {
			CurrentLocPos++;
			return HistoryItemsList[CurrentLocPos > HistoryItemsList.Count - 1 ? HistoryItemsList.Count - 1 : CurrentLocPos];
		}

		/// <summary>Removes any Items Forward of the current position</summary>
		[System.Obsolete("Try to make this private")]
		public void ClearForwardItems() {
			ForwardEntries.ForEach(item => HistoryItemsList.Remove(item));
		}

		/// <summary>Clears all items then adds all items from <param name="log">the log</param></summary>
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