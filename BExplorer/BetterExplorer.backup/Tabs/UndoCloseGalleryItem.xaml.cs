using System.Windows.Controls;
using System.Windows.Input;
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using BExplorer.Shell._Plugin_Interfaces;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for UndoCloseGalleryItem.xaml
    /// </summary>
    public partial class UndoCloseGalleryItem : UserControl
    {
        public delegate void NavigationLogEventHandler(object sender, NavigationLogEventArgs e);
        public NavigationLog nav;
        public event NavigationLogEventHandler Click;

        protected virtual void OnClick(NavigationLogEventArgs e) => Click?.Invoke(this, e);
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e) => OnClick(new NavigationLogEventArgs(nav));
        public void PerformClickEvent() => OnClick(new NavigationLogEventArgs(nav));

        public UndoCloseGalleryItem()
        {
            InitializeComponent();
        }

        public void LoadData(NavigationLog log)
        {
            nav = log;
            var obj = log.CurrentLocation;
            tabTitle.Text = obj.DisplayName;
            image1.Source = obj.ThumbnailSource(16, ShellThumbnailFormatOption.IconOnly, ShellThumbnailRetrievalOption.Default);
            this.ToolTip = obj.ParsingName;
        }
    }
}


public class NavigationLogEventArgs
{
    private NavigationLog _obj;
    public IListItemEx CurrentLocation => _obj.CurrentLocation;
    public NavigationLog NavigationLog => _obj;

    public NavigationLogEventArgs(NavigationLog log)
    {
        _obj = log;
    }
}