using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace WPFPieChart {

	public class FolderSizeInfoClass : INotifyPropertyChanged {

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChangeEvent(String propertyName) {
			if (PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion INotifyPropertyChanged Members

		#region Properties

		private String myFolderSizeLoc;
		public String FolderSizeLoc {
			get { return myFolderSizeLoc; }
			set {
				myFolderSizeLoc = value;
				RaisePropertyChangeEvent("FolderSizeLoc");
			}
		}

		private double fSize;
		public double FSize {
			get { return fSize; }
			set {
				fSize = value;
				RaisePropertyChangeEvent("FSize");
			}
		}

		#endregion

		[Obsolete("Not used, should remove", true)]
		public static List<FolderSizeInfoClass> ConstructData(string Dir) {
			var FolderInfoSize = new List<FolderSizeInfoClass>();
			DirectoryInfo data = new DirectoryInfo(Dir);
			foreach (DirectoryInfo item in data.GetDirectories()) {
				FolderSizeInfoClass fsi = new FolderSizeInfoClass();
				fsi.FolderSizeLoc = item.Name;
				fsi.FSize = GetFolderSize(item.FullName, true);
				FolderInfoSize.Add(fsi);
			}

			return FolderInfoSize;
		}

		private static long GetFolderSize(string dir, bool includesubdirs) {
			//TODO: Test
			long retsize = 0;
			var Selected_SearchOption = includesubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			try {
				DirectoryInfo data = new DirectoryInfo(dir);
				foreach (FileInfo item in data.GetFiles("*.*", Selected_SearchOption)) {
					retsize += item.Length;
				}
			}
			catch (Exception) {
			}

			/*
			try {
				DirectoryInfo data = new DirectoryInfo(dir);
				if (includesubdirs) {
					foreach (FileInfo item in data.GetFiles("*.*", SearchOption.AllDirectories)) {
						retsize += item.Length;
					}
				}
				else {
					foreach (FileInfo item in data.GetFiles("*.*", SearchOption.TopDirectoryOnly)) {
						retsize += item.Length;
					}
				}
			}
			catch (Exception) {
			}
			*/
			return retsize;
		}

	}
}