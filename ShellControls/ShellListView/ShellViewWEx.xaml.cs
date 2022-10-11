using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BExplorer.Shell._Plugin_Interfaces;
using BExplorer.Shell.Interop;
using ShellControls.Annotations;
using Size = System.Windows.Size;

namespace ShellControls.ShellListView;
/// <summary>
/// Interaction logic for ShellViewWEx.xaml
/// </summary>
public partial class ShellViewWEx : UserControl, INotifyPropertyChanged {
  public event EventHandler<NavigatingEventArgs> Navigating;
  public event EventHandler<ColumnAddEventArgs> AfterCollumsPopulate;
  public event EventHandler<NavigatedEventArgs> Navigated;
  public WpfObservableRangeCollection<FSItem> Contents { get; set; }
  public IListItemEx CurrentFolder { get; set; }
  public static BitmapSource ExeIcon;

  public static readonly DependencyProperty ItemSizeProperty =
    DependencyProperty.Register(
      nameof(ItemSize), typeof(Size), typeof(ShellViewWEx),
      new PropertyMetadata(new Size(90, 110)));

  public Size ItemSize {
    get { return (Size)GetValue(ItemSizeProperty); }
    set { SetValue(ItemSizeProperty, value); }
  }

  public Int32 IconSize {
    get {
      return this._IconSize;
    }
    set {
      this._IconSize = value;
      this.Contents?.ToList().ForEach(e => {
        e.IsIconOnly = false;
        e.IsThumbnailLoaded = false;
        e.IconSize = value;
        e.IsNeedRefreshing = true;
        e.RaisePropertyChanged("Thumbnail");
      });
      if (value == 48) {
        this.ItemSize = new Size(90, 110);
        //this.Contents.Remove(this.Contents.First());
      } else if (value == 72) {
        this.ItemSize = new Size(110, 130);
      }
    }
  }

  private Int32 _IconSize;
  private IconUpdateService _IconUpdateService = new IconUpdateService();

  public ShellViewWEx() {
    this.Contents = new WpfObservableRangeCollection<FSItem>();
    this.IconSize = 48;
    this.DataContext = this;
    if (ExeIcon == null) {
      var defIconInfo = new Shell32.SHSTOCKICONINFO() { cbSize = (UInt32)Marshal.SizeOf(typeof(Shell32.SHSTOCKICONINFO)) };

      Shell32.SHGetStockIconInfo(Shell32.SHSTOCKICONID.SIID_APPLICATION, Shell32.SHGSI.SHGSI_ICON, ref defIconInfo);
      ExeIcon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
        defIconInfo.hIcon,
        Int32Rect.Empty,
        BitmapSizeOptions.FromEmptyOptions());
    }
    InitializeComponent();
  }

  private void ShellViewItem_DblClick(Object sender, MouseButtonEventArgs e) {
    var selectedItem = (((ListViewItem) sender).Content as FSItem).ShellItem;
    if (selectedItem?.IsFolder == true) {
      this.Navigate(selectedItem);
    } else if (selectedItem.IsLink && selectedItem.ParsingName.EndsWith(".lnk")) {
      var shellLink = new ShellLink(selectedItem.ParsingName);
      var newSho = new FileSystemListItem();
      newSho.Initialize(IntPtr.Zero, shellLink.TargetPIDL);
      if (newSho.IsFolder) {
        this.Navigate(newSho);
      } else {
        this.StartProcessInCurrentDirectory(newSho);
      }

      shellLink.Dispose();
    } else {
      this.StartProcessInCurrentDirectory(selectedItem);
    }
  }

  private void StartProcessInCurrentDirectory(IListItemEx item) {
    var res = Process.Start(new ProcessStartInfo() { FileName = item.ParsingName, WorkingDirectory = this.CurrentFolder.ParsingName, UseShellExecute = true });
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  [NotifyPropertyChangedInvocator]
  protected virtual void OnPropertyChanged([CallerMemberName] String? propertyName = null) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public void Navigate(IListItemEx? destination) {
    
    
    var t = new Thread((() => {
      this._IconUpdateService.ClearQueue();
      this.Contents.Clear();
      this.Navigating?.Invoke(this, new NavigatingEventArgs(destination, true));
      var contents = destination.GetContents(true).OrderByDescending(o => o.IsFolder).ThenBy(o => o.DisplayName).Select(s => new FSItem(this.IconSize, _IconUpdateService) { ShellItem = s });
      this.Contents.AddRange(contents.ToArray());
      this.Navigated?.Invoke(this, new NavigatedEventArgs(destination, this.CurrentFolder, true));
      this.CurrentFolder = destination;
      //this.OnPropertyChanged("Contents");
    }));
    t.SetApartmentState(ApartmentState.STA);
    t.IsBackground = true;
    t.Start();
  }

  public void NavigateParent() {
    if (this.CurrentFolder?.Parent != null) {
      this.Navigate(this.CurrentFolder.Parent);
    }
  }
}
