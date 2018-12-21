using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Annotations;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BExplorer.Shell {
  public class FilesystemTreeViewItem :  INotifyPropertyChanged {

    public IListItemEx FsItem { get; set; }

    public ShellTreeView Owner { get; set; }

    public String DisplayName => this.FsItem.DisplayName;

    public BitmapSource Icon => this.FsItem.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);

    public Boolean IsExpanded { get; set; }

    private Boolean _IsSelected { get; set; }

    public Boolean IsSelected {
      get => this._IsSelected;
      set {
        this._IsSelected = value;
        if (value) {
          //this.Owner.NavigateListView(this.FsItem);
        }
      }
    }

    //public new IEnumerable<Object> Items {
    //  get {
    //   // if (this.IsExpanded) {
    //    var sho = this.FsItem;
    //    var contents = sho.ParsingName != KnownFolders.Computer.ParsingName
    //      ? sho.GetContents(true)
    //        .Where(w => (sho != null && !sho.IsFileSystem && System.IO.Path.GetExtension(sho?.ParsingName)?.ToLowerInvariant() != ".library-ms") ||
    //                    (w.IsFolder || w.IsLink || w.IsDrive)).OrderBy(o => o.DisplayName)
    //      : sho.GetContents(true)
    //        .Where(w => (sho != null && !sho.IsFileSystem && System.IO.Path.GetExtension(sho?.ParsingName)?.ToLowerInvariant() != ".library-ms") || (w.IsFolder || w.IsLink || w.IsDrive));
    //    ShellItem.IsCareForMessageHandle = false;
    //    foreach (var item in contents) {
    //      if (item != null && !item.IsFolder && !item.IsLink) {
    //        continue;
    //      }

    //      //Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

    //      if (item?.IsLink == true) {
    //        try {
    //          var shellLink = new ShellLink(item.ParsingName);
    //          var linkTarget = shellLink.TargetPIDL;
    //          var itemLinkReal = FileSystemListItem.ToFileSystemItem(IntPtr.Zero, linkTarget);
    //          shellLink.Dispose();
    //          if (!itemLinkReal.IsFolder) {
    //            continue;
    //          }
    //        } catch { }
    //      }
    //      yield return new FilesystemTreeViewItem(item);
    //    }
        
    //    //} 
    //    //else {
    //     // yield return new TreeViewItem();
    //    //}
    //  }
    //}

    public ObservableCollection<Object> Items { get; set; }

    public FilesystemTreeViewItem() {

    }

    public FilesystemTreeViewItem(ShellTreeView owner, IListItemEx fsItem) {
      this.Owner = owner;
      this.FsItem = fsItem;
      this.Items = new ObservableCollection<Object>();
      if (this.FsItem.HasSubFolders) {
        this.Items.Add(new TreeViewItem());
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = null) {
      this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
