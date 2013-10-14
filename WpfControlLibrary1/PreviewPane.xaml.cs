using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Shell;
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
    public ExplorerBrowser Browser;

    public BitmapSource Thumbnail { 
      get{
        if (this.Browser.SelectedItems.Count() > 0)
        {
          ShellObject selectedItemsFirst = this.Browser.SelectedItems.First();
          selectedItemsFirst.Thumbnail.CurrentSize = new Size(this.ActualHeight - 20, this.ActualHeight - 20);
          return selectedItemsFirst.Thumbnail.BitmapSource;
        } else {
          return null;
        }
      }
    }

    public String DisplayName
    {
      get
      {
        if (this.Browser.SelectedItems.Count() > 0)
        {
          ShellObject selectedItemsFirst = this.Browser.SelectedItems.First();
          return selectedItemsFirst.GetDisplayName(DisplayNameType.Default);
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
        if (this.Browser.SelectedItems.Count() > 0)
        {
          ShellObject selectedItemsFirst = this.Browser.SelectedItems.First();
          return selectedItemsFirst.Properties.System.ItemTypeText != null ? selectedItemsFirst.Properties.System.ItemTypeText.Value : String.Empty;
        }
        else
        {
          return String.Empty;
        }
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
      
      PropertyChangedEventArgs args = new PropertyChangedEventArgs("Thumbnail");
      this.PropertyChanged.Invoke(this, args);
    }


    public void FillPreviewPane(ExplorerBrowser browser)
    {
      Dispatcher.BeginInvoke(DispatcherPriority.Input, (ThreadStart)(() =>
          {
            if (this.Browser == null)
              this.Browser = browser;
            PropertyChangedEventArgs args = new PropertyChangedEventArgs("Thumbnail");
            this.PropertyChanged.Invoke(this, args);
            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("FileType"));
          }));
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
