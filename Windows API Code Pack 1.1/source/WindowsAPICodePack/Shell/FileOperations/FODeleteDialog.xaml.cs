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

namespace Microsoft.WindowsAPICodePack.Shell.FileOperations {
  /// <summary>
  /// Interaction logic for FODeleteDialog.xaml
  /// </summary>
  public partial class FODeleteDialog : Window {

    public String MessageText { get; set; }
    public String MessageCaption { get; set; }
    public ImageSource MessageIcon { get; set; }


    /// <summary>
    /// Main Constructor
    /// </summary>
    public FODeleteDialog() {
      InitializeComponent();
      this.DataContext = this;
    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
      this.DialogResult = true;
      Close();
    }

    private void Button_Click_2(object sender, RoutedEventArgs e) {
      this.DialogResult = false;
      Close();
    }
  }
}
