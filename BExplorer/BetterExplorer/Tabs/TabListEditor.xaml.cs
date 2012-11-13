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

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for TabListEditor.xaml
    /// </summary>
    public partial class TabListEditor : UserControl
    {
        public TabListEditor()
        {
            InitializeComponent();
        }

        public void ImportSavedTabList(SavedTabsList list)
        {
            foreach(string item in list)
            {
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

        void g_DeleteRequested(object sender, RoutedEventArgs e)
        {
            stackPanel1.Children.Remove((sender as UIElement));
        }

        public SavedTabsList ExportSavedTabList()
        {
            SavedTabsList o = new SavedTabsList();
            foreach (TabListEditorItem g in stackPanel1.Children)
            {
                o.Add(g.Path);
            }
            return o;
        }

        private void gridSplitter1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            foreach (TabListEditorItem item in stackPanel1.Children)
            {
                item.TitleColumnWidth = this.NameCol.Width;
            }
        }

        private void gridSplitter1_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            foreach (TabListEditorItem item in stackPanel1.Children)
            {
                item.TitleColumnWidth = this.NameCol.Width;
            }
        }

    }
}
