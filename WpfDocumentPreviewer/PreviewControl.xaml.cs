using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace WpfDocumentPreviewer {

  /// <summary>
  /// Interaction logic for PreviewerControl.xaml
  /// </summary>
  public partial class PreviewerControl : UserControl, INotifyPropertyChanged {

    #region Implement INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    public void RaisePropertyChanged(string propertyName) {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    private Bitmap imageSrc;
    private List<string> Images = new List<string>(new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".wmf" });

    public ImageSource ImageSource { get { return IconFromFileName(fileName); } }

    private string fileName;
    public string FileName {
      get { return System.IO.Path.GetFileName(fileName); }
      set {
        fileName = value;
        Task.Run(() => {


          SetFileName(fileName);
          if (!String.IsNullOrEmpty(fileName)) {
            RaisePropertyChanged("ImageSource");
          }
          RaisePropertyChanged("FileName");
        });
      }
    }

    /*
    public static ImageSource BitmapFromUri(Uri source) {
      var bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.UriSource = source;
      bitmap.CacheOption = BitmapCacheOption.OnLoad;
      bitmap.EndInit();
      return bitmap;
    }
    */

    public PreviewerControl() {
      InitializeComponent();
      this.Unloaded += PreviewerControl_Unloaded;
    }

    private void SetFileName(string fileName) {
      if (!String.IsNullOrEmpty(fileName)) {

        Guid? previewGuid = Guid.Empty;
        if (previewHandlerHost1.Open(fileName, out previewGuid) == false) {
          Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            (Action)(() => {
              if ((previewGuid != null && previewGuid.Value != Guid.Empty) || !Images.Contains(System.IO.Path.GetExtension(fileName))) {
                wb1.Visibility = Visibility.Visible;
                var activeX = wb1.GetType().InvokeMember("ActiveXInstance",
                  BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                  null, wb1, new object[] { }) as SHDocVw.WebBrowser;
                activeX.FileDownload += activeX_FileDownload;
                wb1.Navigate(fileName);
                imgh1.Visibility = Visibility.Collapsed;
                wfh1.Visibility = Visibility.Collapsed;
              } else {
                wb1.Visibility = Visibility.Collapsed;
                using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open)) {
                  var bmp = new Bitmap(fs);
                  imageSrc = (Bitmap)bmp.Clone();
                }
                img1.Image = imageSrc;
                img1.Refresh();
                img1.Update();
                imgh1.Visibility = Visibility.Visible;
                wfh1.Visibility = Visibility.Collapsed;
              }
            }));
        } else {
          Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            (Action)(() => {
              imgh1.Visibility = Visibility.Collapsed;
              wb1.Visibility = Visibility.Collapsed;
              wfh1.Visibility = Visibility.Visible;
            }));
        }

      } else {
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            (Action)(() => {
              Guid? previewGuid = Guid.Empty;
              previewHandlerHost1.Open(fileName, out previewGuid);
              wb1.Visibility = Visibility.Collapsed;
              wfh1.Visibility = Visibility.Visible;
              imgh1.Visibility = Visibility.Collapsed;
              img1.Image = null;
              if (imageSrc != null)
                imageSrc.Dispose();
            }));
      }
    }

    void activeX_FileDownload(bool ActiveDocument, ref bool Cancel) {
      if (!ActiveDocument) {
        Cancel = true;
        this.FileName = null;
      }
    }



    void PreviewerControl_Unloaded(object sender, RoutedEventArgs e) {
      previewHandlerHost1.Dispose();
    }

    internal BitmapSource IconFromFileName(string fileName) {
      BitmapImage bmpImage = new BitmapImage();
      if (fileName != null && fileName.Contains(".")) {
        try {
          System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
          Bitmap bmp = icon.ToBitmap();
          MemoryStream strm = new MemoryStream();
          bmp.Save(strm, System.Drawing.Imaging.ImageFormat.Png);
          bmpImage.BeginInit();
          strm.Seek(0, SeekOrigin.Begin);
          bmpImage.StreamSource = strm;
          bmpImage.EndInit();
        } catch { }
      }

      return bmpImage;
    }

    private void wb1_LoadCompleted(object sender, NavigationEventArgs e) {
      if (wb1.Document == null) return;
      try {
        mshtml.IHTMLDocument2 doc = (mshtml.IHTMLDocument2)wb1.Document;
        if (doc.title == "Navigation Canceled") {
          wb1.Visibility = Visibility.Collapsed;
          wfh1.Visibility = Visibility.Visible;
        }
      } catch { }

    }


  }
}
