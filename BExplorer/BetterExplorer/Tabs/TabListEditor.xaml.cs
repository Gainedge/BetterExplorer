using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BetterExplorer {

	/// <summary>
	/// Interaction logic for TabListEditor.xaml
	/// </summary>
	public partial class TabListEditor : UserControl {

		public TabListEditor() {
			InitializeComponent();
		}

		public void ImportSavedTabList(SavedTabsList list) {
			stackPanel1.Children.Clear();
			foreach (string item in list) {
				//MessageBox.Show(item);
				TabListEditorItem g = new TabListEditorItem(item);
				g.TitleColumnWidth = NameCol.Width;
				g.Width = this.Width;
				g.VerticalAlignment = System.Windows.VerticalAlignment.Top;
				g.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
				g.DeleteRequested += new RoutedEventHandler(g_DeleteRequested);
				stackPanel1.Children.Add(g);
			}
		}

		public void AddTab(string loc) {
			TabListEditorItem g = new TabListEditorItem(loc);
			g.TitleColumnWidth = NameCol.Width;
			g.Width = this.Width;
			g.VerticalAlignment = System.Windows.VerticalAlignment.Top;
			g.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
			g.DeleteRequested += new RoutedEventHandler(g_DeleteRequested);
			stackPanel1.Children.Add(g);
		}

		public SavedTabsList ExportSavedTabList() {
			var o = new SavedTabsList();
			o.AddRange(stackPanel1.Children.OfType<TabListEditorItem>().Select(x => x.Path));
			return o;
			//foreach (TabListEditorItem g in stackPanel1.Children) {
			//	o.Add(g.Path);
			//}
		}

		private void g_DeleteRequested(object sender, RoutedEventArgs e) {
			stackPanel1.Children.Remove((sender as UIElement));
		}

		private void gridSplitter1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			foreach (TabListEditorItem item in stackPanel1.Children) {
				item.TitleColumnWidth = this.NameCol.Width;
			}
		}

		private void gridSplitter1_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e) {
			foreach (TabListEditorItem item in stackPanel1.Children) {
				item.TitleColumnWidth = this.NameCol.Width;
			}
		}
	}
}