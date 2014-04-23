
using BExplorer.Shell;
using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Threading;

namespace BetterExplorerControls
{
  /// <summary>
  /// Interaction logic for PreviewPane.xaml
  /// </summary>
  public partial class PreviewPane : UserControl, INotifyPropertyChanged
  {
    public ShellView Browser;
    private BitmapSource _thumbnail;
    private ShellItem[] SelectedItems;
    public BitmapSource Thumbnail { 
      get{
        return _thumbnail;
      }
    }

    
    public String DisplayName
    {
      get
      {

				if (this.Browser != null && this.Browser.GetSelectedCount() > 0)
        {
          return this.Browser.SelectedItems[0].DisplayName;
        }
        else
        {
          return String.Empty;
        }
      }
    }

    public String FileType
    {
      get
      {
        if (this.Browser != null && this.Browser.SelectedItems.Count() > 0)
        {
					return this.Browser.SelectedItems[0].GetPropertyValue(new PROPERTYKEY() { fmtid = Guid.Parse("B725F130-47EF-101A-A5F1-02608C9EEBAC"), pid = 4 }, typeof(String)).Value.ToString();
        }
        else
        {
          return String.Empty;
        }
				return string.Empty;
      }
    }
    public PreviewPane()
    {
      InitializeComponent();
      DataContext = this;
      this.Loaded += PreviewPane_Loaded;
    }

    void PreviewPane_Loaded(object sender, RoutedEventArgs e)
    {
      this.SizeChanged += PreviewPane_SizeChanged;
    }


    void PreviewPane_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      //Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() =>
      //{
        ShellItem selectedItemsFirst = null;
				if (this.Browser != null && this.Browser.GetSelectedCount() == 1)
        {
          selectedItemsFirst = this.Browser.SelectedItems.First();
          selectedItemsFirst.Thumbnail.CurrentSize = new System.Windows.Size(this.ActualHeight - 20, this.ActualHeight - 20);
					selectedItemsFirst.Thumbnail.FormatOption = BExplorer.Shell.Interop.ShellThumbnailFormatOption.Default;
					selectedItemsFirst.Thumbnail.RetrievalOption = BExplorer.Shell.Interop.ShellThumbnailRetrievalOption.Default;
          icon.Source = selectedItemsFirst.Thumbnail.BitmapSource;
        }

      //}));
    }


    public void FillPreviewPane(ShellView browser)
    {
			if (this.Browser == null)
				this.Browser = browser;

      Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() =>
          {
            

						if (this.Browser.GetSelectedCount() == 1)
						{
                this.SelectedItems = this.Browser.SelectedItems.ToArray();
								this.SelectedItems[0].Thumbnail.CurrentSize = new System.Windows.Size(this.ActualHeight - 20, this.ActualHeight - 20);
								this.SelectedItems[0].Thumbnail.FormatOption = BExplorer.Shell.Interop.ShellThumbnailFormatOption.Default;
								this.SelectedItems[0].Thumbnail.RetrievalOption = BExplorer.Shell.Interop.ShellThumbnailRetrievalOption.Default;
                icon.Source = this.SelectedItems[0].Thumbnail.BitmapSource;
            }

            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("FileType"));
            
          }));
      
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
