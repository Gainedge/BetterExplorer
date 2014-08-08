using BExplorer.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsHelper;

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations {
	/// <summary>
	/// Interaction logic for CollisionDialog.xaml
	/// </summary>
	public partial class CollisionDialog : Window {
		public List<CollisionInfo> collisions { get; set; }
		public ObservableCollection<CollisionItem> Contents { get; set; }
		public CollisionDialog(List<CollisionInfo> _collisions, ShellItem SourceDestination, ShellItem Destination) {
			InitializeComponent();
			this.collisions = _collisions;
			Contents = new ObservableCollection<CollisionItem>();
			foreach (var item in _collisions) {
				Contents.Add(new CollisionItem() { DataContext = item });
			}
			this.lblFromfolder.Text = SourceDestination.DisplayName;
			this.lblTofolder.Text = Destination.DisplayName;
			this.DataContext = this;
		}

		private void btnContinue_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = true;
			this.Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = false;
			this.Close();
		}

		private void svScroll_SizeChanged(object sender, SizeChangedEventArgs e) {
			//var h = svScroll.ViewportWidth;
			//this.clSpace.Width =new GridLength(svScroll.VerticalScrollBarVisibility == ScrollBarVisibility.Visible?15:20); 
		}

		private void icItems_SizeChanged(object sender, SizeChangedEventArgs e) {
			this.clSpace.Width = new GridLength(e.NewSize.Width < pnlContainer.ActualWidth ? 5 : 20);
		}

		private void chkFirst_Click(object sender, RoutedEventArgs e) {
			foreach (var item in this.Contents) {
				item.chkFirst.IsChecked = chkFirst.IsChecked;
			}
		}

		private void chkSecond_Click(object sender, RoutedEventArgs e) {
			foreach (var item in this.Contents) {
				item.chkSecond.IsChecked = chkSecond.IsChecked;
			}
		}
	}

	public class CollisionInfo {
		public String itemPath { get; set; }
		public String CorrespondingItemPath { get; set; }
		private ShellItem item {
			get {
				return new ShellItem(itemPath);
			}
		}
		private ShellItem Correspondingitem {
			get {
				return new ShellItem(CorrespondingItemPath);
			}
		}
		public int index { get; set; }
		public ImageSource ItemImage {
			get {
				item.Thumbnail.FormatOption = BExplorer.Shell.Interop.ShellThumbnailFormatOption.Default;
				item.Thumbnail.CurrentSize = new Size(64, 64);
				return item.Thumbnail.BitmapSource;
			}
		}
		public String ItemName {
			get {
				return item.DisplayName;
			}
		}
		public String ItemSize {
			get {
				return WindowsAPI.StrFormatByteSize(0);
			}
		}
		public String ItemSizeC {
			get {
				return WindowsAPI.StrFormatByteSize(0);
			}
		}
		public String ItemDmodif {
			get {
				return "";// item.Properties.System.DateModified.Value.ToString();
			}
		}
		public String ItemDmodifC {
			get {
				return "";// Correspondingitem.Properties.System.DateModified.Value.ToString();
			}
		}
		public ImageSource ItemImageC {
			get {
				Correspondingitem.Thumbnail.FormatOption = BExplorer.Shell.Interop.ShellThumbnailFormatOption.Default;
				Correspondingitem.Thumbnail.CurrentSize = new Size(64, 64);
				return Correspondingitem.Thumbnail.BitmapSource;
			}
		}
		public String ItemNameC {
			get {
				return Correspondingitem.DisplayName;
			}
		}
		public bool IsChecked { get; set; }
		public bool IsCheckedC { get; set; }
	}
}
