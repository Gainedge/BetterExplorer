using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GongSolutions.Shell;

namespace BetterExplorer
{
    public class NavigationLog
    {

			private List<ShellItem> HistoryItems = new List<ShellItem>();

        int CurrentLocIndex = -1;

        public NavigationLog()
        {
            HistoryItems.Clear();
        }

        public void ImportData(NavigationLog log)
        {
            HistoryItems.Clear();
            HistoryItems.AddRange(log.HistoryItemsList);
            CurrentLocIndex = HistoryItems.LastIndexOf(log.CurrentLocation);
        }

				public void NavigateOnwards(ShellItem loc)
        {
            HistoryItems.Add(loc);
            CurrentLocIndex = HistoryItems.Count - 1;
        }

				public ShellItem NavigateBack()
        {

            CurrentLocIndex--;

            try
            {
                return HistoryItems[CurrentLocIndex];
            }
            catch
            {
                CurrentLocIndex++;

                return HistoryItems[CurrentLocIndex];
            }
        }

				public ShellItem NavigateForward()
        {

            CurrentLocIndex++;

            return HistoryItems[CurrentLocIndex];
        }

				public List<ShellItem> HistoryItemsList
        {
            get
            {
                //HistoryItems.Reverse();
                return HistoryItems;
            }
        }

        public int CurrentLocPos
        {
            get
            {
                return CurrentLocIndex;
            }
            set
            {
                CurrentLocIndex = value;
            }
        }

				public List<ShellItem> BackEntries
        {
            get
            {
							List<ShellItem> _BackEntries = new List<ShellItem>();
                if (CurrentLocIndex != -1)
                {
                  for (int i = 0; i < CurrentLocIndex; i++)
                  {
                    _BackEntries.Add(HistoryItems[i]);
                  }
                }
                //_BackEntries.Reverse();
                return _BackEntries;
            }
        }

				public List<ShellItem> ForwardEntries
        {
            get
            {
							List<ShellItem> _ForwardEntries = new List<ShellItem>();
                if (CurrentLocIndex != -1)
                {
                  for (int i = CurrentLocIndex; i < HistoryItems.Count; i++)
                  {
                    _ForwardEntries.Add(HistoryItems[i]);
                  }
                }
                //_BackEntries.Reverse();
                return _ForwardEntries;
            }
        }
        public void ClearForwardItems()
        {
          for (int i = HistoryItems.ToArray().Count() - 1; i > CurrentLocIndex; i--)
          {
            HistoryItems.RemoveAt(i);
          }
        }

        public bool CanNavigateBackwards
        {
            get
            {
                if (CurrentLocIndex == 0 || HistoryItems.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool CanNavigateForwards
        {
            get
            {
                if (CurrentLocIndex == HistoryItems.Count - 1 || HistoryItems.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

				public ShellItem CurrentLocation
        {
            get
            {
                if (HistoryItems.Count == 0)
                {
                    return null;
                }
                else
                    return HistoryItems[CurrentLocIndex];
            }
            set
            {
                NavigateOnwards(value);
            }
        }

    }

    public class ShObjwithNumber
    {
			public ShellItem obj;
        public int number;
    }
}
