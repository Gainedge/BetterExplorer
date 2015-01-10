using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BExplorer.Shell {
	/// <summary>
	/// Interaction logic for FileOperation.xaml
	/// </summary>
	public partial class FileOperation : UserControl {
		public String[] SourceItemsCollection { get; set; }
		public String DestinationLocation { get; set; }
		public FileOperationDialog ParentContents { get; set; }
		public Boolean Cancel = false;
		public OperationType OperationType { get; set; }
		public Guid Handle;
		const FileOptions FileFlagNoBuffering = (FileOptions)0x20000000;
		private bool DeleteToRB { get; set; }
		public delegate void NewMessageDelegate(string NewMessage);
		IntPtr CorrespondingWinHandle = IntPtr.Zero;
		System.Windows.Forms.Timer LoadTimer = new System.Windows.Forms.Timer();
		DateTime OperationStartTime = DateTime.Now;
		Dictionary<String, long> oldbyteVlaues = new Dictionary<string, long>();
		DateTime LastMeasuredTime = DateTime.Now;

		public FileOperation() {
			InitializeComponent();
		}
	}

	public enum OperationType {
		Copy,
		Move,
		Delete,
		Rename,
		Decomress,
		Compress
	}
}
