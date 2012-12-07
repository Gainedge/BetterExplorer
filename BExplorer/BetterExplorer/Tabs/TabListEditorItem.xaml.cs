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
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for TabListEditorItem.xaml
    /// </summary>
    public partial class TabListEditorItem : UserControl
    {
        private bool ied = false;

        public TabListEditorItem()
        {
            InitializeComponent();
        }

        public TabListEditorItem(string path)
        {
            InitializeComponent();
            Path = path;
        }

        public string Path
        {
            get
            {
                return Convert.ToString(loc.Content);
            }
            set
            {
                try
                {
                    loc.Content = value;
                    ShellObject o = ShellObject.FromParsingName(value);
                    o.Thumbnail.CurrentSize = new System.Windows.Size(24, 24);
                    o.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                    folicon.Source = o.Thumbnail.BitmapSource;
                    title.Content = o.GetDisplayName(DisplayNameType.Default);
                    o.Dispose();
                }
                catch
                {
                    loc.Content = value;
                    folicon.Source = null;
                    title.Content = "";
                }
            }
        }

        public double TitleColumnWidthValue
        {
            get
            {
                return NameCol.Width.Value;
            }
            set
            {
                NameCol.Width = new GridLength(value, GridUnitType.Star);
            }
        }

        public GridLength TitleColumnWidth
        {
            get
            {
                return NameCol.Width;
            }
            set
            {
                NameCol.Width = value;
            }
        }

        public bool IsInEditMode
        {
            get
            {
                return ied;
            }
        }

        public void BeginEditMode()
        {
            ied = true;
            EditGrid.Visibility = System.Windows.Visibility.Visible;
            editpath.Text = Path;
            editpath.Focus();
        }

        private void EditApply_Click(object sender, RoutedEventArgs e)
        {
            CancelEditMode();
            Path = editpath.Text;
        }

        private void EditCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelEditMode();
        }

        public void CancelEditMode()
        {
            ied = false;
            EditGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            BeginEditMode();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            EditBtn.Visibility = System.Windows.Visibility.Visible;
            DeleteBtn.Visibility = System.Windows.Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            EditBtn.Visibility = System.Windows.Visibility.Collapsed;
            DeleteBtn.Visibility = System.Windows.Visibility.Collapsed;
        }

        public event RoutedEventHandler DeleteRequested;

        protected virtual void OnDeleteRequested(RoutedEventArgs e)
        {
            if (DeleteRequested != null)
                DeleteRequested(this, e);
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteRequested(e);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                editpath.Text = dlg.FileName;
            }
        }

        private void editpath_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CancelEditMode();
                Path = editpath.Text;
            }
        }

    }
}
