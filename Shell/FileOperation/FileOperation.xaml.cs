using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BExplorer.Shell
{
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
		public List<AsyncUnbuffCopy> Operations = new List<AsyncUnbuffCopy>();

		public FileOperation() {
			InitializeComponent();
		}

		public void AddOperation(AsyncUnbuffCopy operation) {
			operation.OnProgress += Operation_OnProgress;
			this.Operations.Add(operation);
		}

		private void Operation_OnProgress(object sender, CopyEventArgs e) {
			Double precentComplete = ((Double)e.BytesTransferred/e.TotalBytes)*100d;
			Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
						(Action)(() => {
							this.prOverallProgress.Maximum = 100;
							this.prOverallProgress.Rate = 60;
							this.prOverallProgress.Value = precentComplete;
							if (precentComplete == 100.0) {
								this.ParentContents.Contents.Remove(this);
							}
						}));
			
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
