using BExplorer.Shell._Plugin_Interfaces;
using Fluent;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BExplorer.Shell.Interop;

namespace BExplorer.Shell {
  /// <summary>
  /// Interaction logic for DeleteNotificationDialog.xaml
  /// </summary>
  public partial class DeleteNotificationDialog : RibbonWindow {
    public DeleteNotificationDialog() {
      InitializeComponent();
      this.Owner = Application.Current.MainWindow;
      this.IsIconVisible = false;
    }

    public static Boolean? ShowNotificationDialog(IListItemEx[] items) {
      var dialog = new DeleteNotificationDialog();
      var countItems = items.Length;
      if (countItems == 1) {
        var firstItem = items.First();
        dialog.lblName.Text = firstItem.DisplayName;
        if (firstItem.IsFolder) {
          dialog.Title = "Delete folder permanently";
          dialog.icoMain.Source = new BitmapImage(new Uri("pack://application:,,,/BetterExplorer;Component/Images/folder_full_delete_d.png"));
          dialog.lblDateModif.Text = ((DateTime) firstItem.GetPropertyValue(SystemProperties.DateModified, typeof(DateTime)).Value).ToString(Thread.CurrentThread.CurrentUICulture);
        } else {
          dialog.Title = "Delete file permanently";
          dialog.icoMain.Source = new BitmapImage(new Uri("pack://application:,,,/BetterExplorer;Component/Images/delete-file-d.png"));
          dialog.lblType.Text = "Item type: " + firstItem.GetPropertyValue(SystemProperties.FileType, typeof(String)).Value.ToString();
          dialog.lblDimentions.Text = "Size: " + ShlWapi.StrFormatByteSize(Convert.ToInt64(firstItem.GetPropertyValue(SystemProperties.FileSize, typeof(Int64)).Value.ToString()));
          dialog.lblDateModif.Text = "Date modified: " + ((DateTime)firstItem.GetPropertyValue(SystemProperties.DateModified, typeof(DateTime)).Value).ToString(Thread.CurrentThread.CurrentUICulture);
        }

        dialog.imgThumbnail.Margin = new Thickness(25);
        dialog.imgThumbnail.Source = firstItem.ThumbnailSource(128, ShellThumbnailFormatOption.Default,
          ShellThumbnailRetrievalOption.Default);
        dialog.pnlInfo.Margin = new Thickness(0, 25, 15, 25);
      } else if (countItems > 1) {
        dialog.Title = "Delete multiple items permanently";
        dialog.icoMain.Source = new BitmapImage(new Uri("pack://application:,,,/BetterExplorer;Component/Images/documents_files_icon_d.png"));
        dialog.imgThumbnail.Margin = new Thickness(10);
        dialog.pnlInfo.Margin = new Thickness(0);
      }
      return dialog.ShowDialog();
    }

    private void ButtonYes_OnClick(Object sender, RoutedEventArgs e) {
      this.DialogResult = true;
    }

    private void ButtonNo_OnClick(Object sender, RoutedEventArgs e) {
      this.DialogResult = false;
    }
  }
}
