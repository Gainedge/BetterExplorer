using System;
using System.Collections.Specialized;
using Microsoft.WindowsAPICodePack.Shell;
using System.Linq;
using System.Collections.Generic;


namespace Microsoft.WindowsAPICodePack.Controls.WindowsForms {
	/// <summary>
	/// DropData class
	/// </summary>
	public class FileOperationsData : IDisposable {
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing) {
			if (disposing)
				if (Shellobjects != null) {
					Shellobjects.Dispose();
					Shellobjects = null;
				}
		}
		~FileOperationsData() {
			Dispose(false);
		}
		public ShellObject[] ItemsForDrop;
		public StringCollection DropList;
		public string PathForDrop;
		public ShellObjectCollection Shellobjects;
	}
}
