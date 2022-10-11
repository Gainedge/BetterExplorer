using System.Collections.Concurrent;
using System.Threading;
using BExplorer.Shell;
using BExplorer.Shell.Interop;

namespace ShellControls.ShellListView;
public class IconUpdateService {
  private ConcurrentQueue<FSItem> _ItemsForUpdate = new ConcurrentQueue<FSItem>();

  public void QueueForUpdate(FSItem item) {
    this._ItemsForUpdate.Enqueue(item);
  }

  public void ClearQueue() {
    this._ItemsForUpdate.Clear();
  }

  public IconUpdateService() {
    var tUpdate = new Thread((() => {
      while (true) {
        Thread.Sleep(1);
        if (this._ItemsForUpdate.TryDequeue(out var item)) {
          if (item != null && item.IsThumbnailLoaded) {
            continue;
          }

          var res = item.ShellItem.ThumbnailSource(item.IconSize, ShellThumbnailFormatOption.ThumbnailOnly, ShellThumbnailRetrievalOption.Default);
          if (res != null) {
            item.IsThumbnailLoaded = true;
            item.IsNeedRefreshing = false;
            item.RaisePropertyChanged("Thumbnail");
          } else {
            item.IsThumbnailLoaded = true;
            item.IsIconOnly = true;
          }
        }
      } 
    }));
    tUpdate.SetApartmentState(ApartmentState.STA);
    tUpdate.IsBackground = true;
    tUpdate.Start();
  }
}
