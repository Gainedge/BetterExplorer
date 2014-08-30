using System.Collections.Generic;
using System.Linq;

namespace BExplorer.Shell {

	/// <summary>Represents a log of where the user has navigated through the file system</summary>
	public class NavigationLog {

		#region Properties

		/// <summary>The locations the user has been to</summary>
		public List<ShellItem> HistoryItemsList { get; private set; }

		/// <summary>the position/place/item the user is currently at in the <see cref="HistoryItemsList">History</see> Where -1 = nothing selected</summary>
		public int CurrentLocPos { get; set; }

		/// <summary>Can the user navigate backwards?</summary>
		public bool CanNavigateBackwards {
			get {
				//return CurrentLocPos != 0 && HistoryItemsList.Any(); 
				//return HistoryItemsList.Count - CurrentLocPos > 0 && CurrentLocPos != -1;
				//return HistoryItemsList.Count - (CurrentLocPos + 1) != 0;
				return CurrentLocPos > 0;
			}
		}

		/// <summary>Can the user navigate forwards?</summary>
		public bool CanNavigateForwards {
			get {
				//return CurrentLocPos != HistoryItemsList.Count - 1 || CurrentLocPos != -1;
				return HistoryItemsList.Count > CurrentLocPos + 1;
			}
		}

		private List<ShellItem> ForwardEntries {
			get {
				return HistoryItemsList.Skip(CurrentLocPos + 1).ToList();
			}
		}

		/// <summary>Represents the current ShellItem the user is at or Sets the item by adding it to HistoryItemsList</summary>
		/// <remarks>
		/// Gets the item from <see cref="HistoryItemsList"/> at index <see cref="CurrentLocPos"/> (or null if Count = 0).
		/// Sets the Location by adding the ShellItem to <see cref="HistoryItemsList"/> and setting it as the <see cref="CurrentLocPos"/>
		/// </remarks>
		public ShellItem CurrentLocation {
			get {
				return HistoryItemsList.ElementAtOrDefault(CurrentLocPos);
				//return HistoryItemsList.Any() ? HistoryItemsList[CurrentLocPos] : null;
			}
			set {
				HistoryItemsList.Add(value);
				CurrentLocPos = HistoryItemsList.Count - 1;
			}
		}

		#endregion Properties

		public NavigationLog(ShellItem StartingLocation) {
			HistoryItemsList = new List<ShellItem>(new[] { StartingLocation });
			CurrentLocPos = 0; //-1;
		}

		/// <summary>Navigates backwards one item</summary>
		public ShellItem NavigateBack() {
			CurrentLocPos--;
			//return HistoryItemsList[CurrentLocPos < 0 ? 0 : CurrentLocPos];
			return HistoryItemsList[CurrentLocPos];
		}

		/// <summary>Navigates forwards one item</summary>
		public ShellItem NavigateForward() {
			CurrentLocPos++;
			//return HistoryItemsList[CurrentLocPos > HistoryItemsList.Count - 1 ? HistoryItemsList.Count - 1 : CurrentLocPos];
			return HistoryItemsList[CurrentLocPos];
		}

		/// <summary>Removes any Items Forward of the current position</summary>
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