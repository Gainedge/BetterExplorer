using System;
using System.Collections.Generic;
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
using BetterExplorer;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using BExplorer.Shell._Plugin_Interfaces;

namespace BetterExplorer {
	/// <summary>
	/// Interaction logic for UndoCloseGalleryItem.xaml
	/// </summary>
	public partial class UndoCloseGalleryItem : UserControl {
		public delegate void NavigationLogEventHandler(object sender, NavigationLogEventArgs e);
		public NavigationLog nav;
		public event NavigationLogEventHandler Click;

		public UndoCloseGalleryItem() {
			InitializeComponent();
		}

		public void LoadData(NavigationLog log) {
			nav = log;
			var obj = log.CurrentLocation;
			tabTitle.Text = obj.DisplayName;
			image1.Source = obj.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
			this.ToolTip = obj.ParsingName;
		}


		protected virtual void OnClick(NavigationLogEventArgs e) {
			if (Click != null)
				Click(this, e);
		}

		private void UserControl_MouseUp(object sender, MouseButtonEventArgs e) {
			OnClick(new NavigationLogEventArgs(nav));
		}

		public void PerformClickEvent() {
			OnClick(new NavigationLogEventArgs(nav));
		}
	}
}


public class NavigationLogEventArgs {
	private NavigationLog _obj;
	public IListItemEx CurrentLocation { get { return _obj.CurrentLocation; } }
	public NavigationLog NavigationLog { get { return _obj; } }

	public NavigationLogEventArgs(NavigationLog log) {
		_obj = log;
	}
}