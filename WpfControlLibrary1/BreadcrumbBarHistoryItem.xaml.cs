using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterExplorerControls {

	/// <summary>
	/// Interaction logic for BreadcrumbBarHistoryItem.xaml
	/// </summary>
	public partial class BreadcrumbBarHistoryItem : MenuItem {
		/// <summary>
		/// An event that clients can use to be notified whenever the 
		/// elements of the list change:
		/// </summary>
		public event EventHandler DeleteRequested;

		public BreadcrumbBarHistoryItem() {
			InitializeComponent();
		}

		protected virtual void OnDeleteRequested(EventArgs e) {
			if (DeleteRequested != null)
				DeleteRequested(this, e);
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e) {
			OnDeleteRequested(EventArgs.Empty);
		}

		private void MenuItem_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Right) {
				this.ContextMenu.IsOpen = true;
				((ContextMenu)this.Parent).IsOpen = true;
				this.ContextMenu.IsOpen = true;
				e.Handled = true;
			}
		}
	}
}