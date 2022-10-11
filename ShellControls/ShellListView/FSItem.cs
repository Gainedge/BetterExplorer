using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Annotations;
using BExplorer.Shell.Interop;

namespace ShellControls.ShellListView;
public class FSItem : INotifyPropertyChanged {
  private IconUpdateService _IconUpdateService;

  public Int32 IconSize { get; set; }
  //public static readonly DependencyProperty ThumbnailProperty =
  //  DependencyProperty.Register(
  //    nameof(Thumbnail), typeof(BitmapSource), typeof(FSItem),
  //    new PropertyMetadata(null));

  //public BitmapSource Thumbnail {
  //  get { return (BitmapSource)GetValue(ThumbnailProperty); }
  //  set { SetValue(ThumbnailProperty, value); }
  //}
  //public BitmapSource Thumbnail { get; set; }
  //private async Task LoadThumbnailAsync(string imagePath)
  //{
  //  Source = await Task.Run(() =>
  //  {
  //    using (var stream = File.OpenRead(imagePath))
  //    {
  //      var bi = new BitmapImage();
  //      bi.BeginInit();
  //      bi.CacheOption = BitmapCacheOption.OnLoad;
  //      bi.StreamSource = stream;
  //      bi.EndInit();
  //      bi.Freeze();
  //      return bi;
  //    }
  //  });
  //}
  public string DisplayName => this.ShellItem.DisplayName;
  public Boolean IsIconOnly { get; set; }
  public Boolean IsNeedRefreshing { get; set; }
  public Boolean IsThumbnailLoaded { get; set; }

  public BitmapSource Icon => this.ShellItem.ThumbnailSource(this.IconSize, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

  public BitmapSource DefaultIcon {
    get {
      return ShellViewWEx.ExeIcon;
    }
  }

  public BitmapSource Thumbnail {
    get {
      //if (this.IsThumbnailLoaded) return null;
      var isPerInstance = false;//(this.ShellItem.IconType & IExtractIconPWFlags.GIL_PERINSTANCE) == IExtractIconPWFlags.GIL_PERINSTANCE;
      if (isPerInstance) {
        return this.DefaultIcon;
      }

      if (this.IsIconOnly) {
        return this.Icon;
      }
      var thumbnail = this.ShellItem.ThumbnailSource(this.IconSize, ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.CacheOnly);
      if (thumbnail != null && thumbnail.PixelWidth < this.IconSize) {
        this.IsNeedRefreshing = true;
      }

      if (thumbnail != null && thumbnail.PixelWidth == this.IconSize && !this.IsNeedRefreshing) {
        return thumbnail;
      }
      if (this.IsNeedRefreshing || thumbnail == null) {
        if (this.IsNeedRefreshing || !this.IsIconOnly) {
          this._IconUpdateService.QueueForUpdate(this);
        }
        //var t = new Thread((() => {
        //  //Dispatcher.BeginInvoke(new Action(() => {
        //  var thu = this.ShellItem.ThumbnailSource(64, ShellThumbnailFormatOption.Default, ShellThumbnailRetrievalOption.Default);
        //  if (thu != null) {
        //    this.OnPropertyChanged("Thumbnail");
        //    //thu.Dispose();
        //  } else {
        //    this._IsIconOnly = true;
        //  }

        //  this.OnPropertyChanged("Thumbnail");
        //  //}), DispatcherPriority.Background);
        //}));
        //t.SetApartmentState(ApartmentState.STA);
        //t.Start();
        if (this.IsNeedRefreshing && thumbnail != null) {
          return thumbnail;
        }

        return this.Icon;
      }

      return thumbnail;
    }
  }

  public IListItemEx ShellItem { get; set; }

  public event PropertyChangedEventHandler? PropertyChanged;

  [NotifyPropertyChangedInvocator]
  protected virtual void OnPropertyChanged([CallerMemberName] String? propertyName = null) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public void RaisePropertyChanged(string? propertyName = null) {
    this.OnPropertyChanged(propertyName);
  }

  public FSItem(Int32 initialIconSize, IconUpdateService service) {
    this.IconSize = initialIconSize;
    this._IconUpdateService = service;
    
  }
}
