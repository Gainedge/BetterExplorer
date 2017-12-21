using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BExplorer.Shell;

namespace BetterExplorer {

  /// <summary>
  /// Interaction logic for ConditionalSelectForm.xaml
  /// </summary>
  public partial class ConditionalSelectForm : Window {
    private System.Globalization.CultureInfo ci;
    public bool CancelAction = true;
    public ConditionalSelectData csd;

    private ConditionalSelectForm() {
      this.InitializeComponent();

      this.dcquery.SelectedDate = DateTime.Today;
      this.dmquery.SelectedDate = DateTime.Today;
      this.daquery.SelectedDate = DateTime.Today;
      this.sizequery1.Text = "0";
      this.sizequery2.Text = "0";
      this.namequery.Text = (this.FindResource("txtFilename") as string);
      this.ci = System.Threading.Thread.CurrentThread.CurrentCulture;
    }

    public static void Open(ShellView ShellListView) {
      var csf = new ConditionalSelectForm();
      csf.ShowDialog();
      if (!csf.CancelAction) {
        csf.ConditionallySelectFiles(csf.csd, ShellListView);
      }
    }

    private void ConditionallySelectFiles(ConditionalSelectData csdItem, ShellView shellListView) {
      if (csdItem == null) {
        return;
      }

      //The following items are added
      var matches_Name = new List<BExplorer.Shell._Plugin_Interfaces.IListItemEx>();
      var matches_Size = new List<BExplorer.Shell._Plugin_Interfaces.IListItemEx>();
      var matches_DateCreated = new List<BExplorer.Shell._Plugin_Interfaces.IListItemEx>();
      var matches_DateLastModified = new List<BExplorer.Shell._Plugin_Interfaces.IListItemEx>();
      var matches_LastAccessed = new List<BExplorer.Shell._Plugin_Interfaces.IListItemEx>();

      shellListView.DeSelectAllItems();

      if (csdItem.FilterByFileName) {
        foreach (var item in shellListView.Items) {
          var data = new FileInfo(item.ParsingName);
          string ToFind = csdItem.FileNameData.matchCase ? data.Name : data.Name.ToLowerInvariant();

          switch (csdItem.FileNameData.filter) {
            case ConditionalSelectParameters.FileNameFilterTypes.Contains:
              if (ToFind.Contains(csdItem.FileNameData.matchCase ? csdItem.FileNameData.query : csdItem.FileNameData.query.ToLowerInvariant())) {
                matches_Name.Add(item);
              }

              break;
            case ConditionalSelectParameters.FileNameFilterTypes.StartsWith:
              if (ToFind.StartsWith(csdItem.FileNameData.query)) {
                matches_Name.Add(item);
              }

              break;
            case ConditionalSelectParameters.FileNameFilterTypes.EndsWith:
              if (ToFind.EndsWith(csdItem.FileNameData.query)) {
                matches_Name.Add(item);
              }

              break;
            case ConditionalSelectParameters.FileNameFilterTypes.Equals:
              if (ToFind == csdItem.FileNameData.query) {
                matches_Name.Add(item);
              }

              break;
            case ConditionalSelectParameters.FileNameFilterTypes.DoesNotContain:
              if (!ToFind.Contains(csdItem.FileNameData.query)) {
                matches_Name.Add(item);
              }

              break;
            case ConditionalSelectParameters.FileNameFilterTypes.NotEqualTo:
              if (ToFind != csdItem.FileNameData.query) {
                matches_Name.Add(item);
              }

              break;
            default:
              break;
          }
        }
      } else {
        //Matches_Name.AddRange(shells.Where((x) => !Directory.Exists(x.ParsingName)));
      }

      if (csdItem.FilterByFileSize) {
        foreach (var item in shellListView.Items.Where(w => !w.IsFolder && w.IsFileSystem)) {
          var data = new FileInfo(item.ParsingName);
          switch (csdItem.FileSizeData.filter) {
            case ConditionalSelectParameters.FileSizeFilterTypes.LargerThan:
              if (data.Length > csdItem.FileSizeData.query1) {
                matches_Size.Add(item);
              }

              break;

            case ConditionalSelectParameters.FileSizeFilterTypes.SmallerThan:
              if (data.Length < csdItem.FileSizeData.query1) {
                matches_Size.Add(item);
              }

              break;

            case ConditionalSelectParameters.FileSizeFilterTypes.Equals:
              if (data.Length == csdItem.FileSizeData.query1) {
                matches_Size.Add(item);
              }

              break;

            case ConditionalSelectParameters.FileSizeFilterTypes.Between:
              long largebound, smallbound;
              if (csdItem.FileSizeData.query2 > csdItem.FileSizeData.query1) {
                smallbound = csdItem.FileSizeData.query1;
                largebound = csdItem.FileSizeData.query2;
              } else if (csdItem.FileSizeData.query2 < csdItem.FileSizeData.query1) {
                smallbound = csdItem.FileSizeData.query2;
                largebound = csdItem.FileSizeData.query1;
              } else {
                if (data.Length == csdItem.FileSizeData.query1) {
                  matches_Size.Add(item);
                }

                break;
              }

              if (data.Length > smallbound && data.Length < largebound) {
                matches_Size.Add(item);
              }

              break;

            case ConditionalSelectParameters.FileSizeFilterTypes.NotEqualTo:
              if (data.Length != csdItem.FileSizeData.query1) {
                matches_Size.Add(item);
              }

              break;

            case ConditionalSelectParameters.FileSizeFilterTypes.NotBetween:
              long largebound2, smallbound2;
              if (csdItem.FileSizeData.query2 > csdItem.FileSizeData.query1) {
                smallbound2 = csdItem.FileSizeData.query1;
                largebound2 = csdItem.FileSizeData.query2;
              } else if (csdItem.FileSizeData.query2 < csdItem.FileSizeData.query1) {
                smallbound2 = csdItem.FileSizeData.query2;
                largebound2 = csdItem.FileSizeData.query1;
              } else {
                // they are the same, use Unequal code
                if (data.Length != csdItem.FileSizeData.query1) {
                  matches_Size.Add(item);
                }

                break;
              }

              if (data.Length < smallbound2 || data.Length > largebound2) {
                matches_Size.Add(item);
              }

              break;

            default:
              break;
          }
        }
      } else {
        matches_Size.AddRange(matches_Name);
      }

      if (csdItem.FilterByDateCreated) {
        matches_DateCreated.AddRange(!csdItem.FilterByDateCreated ? matches_Size : this.DateFilter(shellListView.Items, csdItem.DateCreatedData, x => x.CreationTimeUtc));
      }

      if (csdItem.FilterByDateModified) {
        matches_DateLastModified.AddRange(!csdItem.FilterByDateModified ? matches_Size : this.DateFilter(shellListView.Items, csdItem.DateModifiedData, x => x.LastWriteTimeUtc));
      }

      if (csdItem.FilterByDateAccessed) {
        matches_LastAccessed.AddRange(!csdItem.FilterByDateAccessed ? matches_DateLastModified : this.DateFilter(shellListView.Items, csdItem.DateAccessedData, x => x.LastAccessTimeUtc));
      }

      shellListView.SelectItems(
        matches_Name.
        Union(matches_Size).
        Union(matches_Size).
        Union(matches_DateCreated).
        Union(matches_DateLastModified).
        Union(matches_LastAccessed
      ).ToArray());
      shellListView.Focus(false, true);
    }

    private List<BExplorer.Shell._Plugin_Interfaces.IListItemEx> DateFilter(List<BExplorer.Shell._Plugin_Interfaces.IListItemEx> shells, ConditionalSelectParameters.DateParameters filter, Func<FileInfo, DateTime> GetDate) {
      var outshells = new List<BExplorer.Shell._Plugin_Interfaces.IListItemEx>();

      foreach (var item in shells) {
        var Date = GetDate(new FileInfo(item.ParsingName));

        switch (filter.filter) {
          case ConditionalSelectParameters.DateFilterTypes.EarlierThan:
            if (DateTime.Compare(Date, filter.queryDate) < 0) {
              outshells.Add(item);
            }

            break;

          case ConditionalSelectParameters.DateFilterTypes.LaterThan:
            if (DateTime.Compare(Date, filter.queryDate) > 0) {
              outshells.Add(item);
            }

            break;

          case ConditionalSelectParameters.DateFilterTypes.Equals:
            if (DateTime.Compare(Date, filter.queryDate) == 0) {
              outshells.Add(item);
            }

            break;

          default:
            break;
        }
      }

      return outshells;
    }

    private void sizecheck_Checked(object sender, RoutedEventArgs e) {
      if (!this.IsLoaded) {
        return;
      }

      this.sizefilter.IsEnabled = true;
      this.sizequery1.IsEnabled = true;
      this.sizequery2.IsEnabled = true;
      this.sizebox1.IsEnabled = true;
      var i = (ConditionalSelectComboBoxItem) this.sizefilter.SelectedItem;
      if (i.IdentifyingName == "Between" || i.IdentifyingName == "NotBetween") {
        this.sizequery2.IsEnabled = true;
        this.sizebox2.IsEnabled = true;
      } else {
        this.sizequery2.IsEnabled = false;
        this.sizebox2.IsEnabled = false;
      }
    }

    private void sizecheck_Unchecked(object sender, RoutedEventArgs e) {
      if (!this.IsLoaded) {
        return;
      }

      this.sizefilter.IsEnabled = false;
      this.sizequery1.IsEnabled = false;
      this.sizequery2.IsEnabled = false;
      this.sizebox1.IsEnabled = false;
      this.sizebox2.IsEnabled = false;
    }

    private void sizefilter_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (!this.IsLoaded) {
        return;
      }

      ConditionalSelectComboBoxItem i = (ConditionalSelectComboBoxItem)e.AddedItems[0];
      if (i.IdentifyingName == "Between" || i.IdentifyingName == "NotBetween") {
        this.sizequery2.IsEnabled = true;
        this.sizebox2.IsEnabled = true;
      } else {
        this.sizequery2.IsEnabled = false;
        this.sizebox2.IsEnabled = false;
      }
    }

    private void namecheck_CheckChanged(object sender, RoutedEventArgs e) {
      if (!this.IsLoaded) {
        return;
      }

      this.namefilter.IsEnabled = e.RoutedEvent.Name == "Checked";
      this.namequery.IsEnabled = e.RoutedEvent.Name == "Checked";
      this.namecase.IsEnabled = e.RoutedEvent.Name == "Checked";
    }

    private void dccheck_CheckChanged(object sender, RoutedEventArgs e) {
      if (!this.IsLoaded) {
        return;
      }

      this.dcfilter.IsEnabled = e.RoutedEvent.Name == "Checked";
      this.dcquery.IsEnabled = e.RoutedEvent.Name == "Checked";
      ;
    }

    private void dmcheck_CheckChanged(object sender, RoutedEventArgs e) {
      if (!this.IsLoaded) {
        return;
      }

      this.dmfilter.IsEnabled = e.RoutedEvent.Name == "Checked";
      this.dmquery.IsEnabled = e.RoutedEvent.Name == "Checked";
    }

    private void dacheck_CheckChanged(object sender, RoutedEventArgs e) {
      if (!this.IsLoaded) {
        return;
      }

      this.dafilter.IsEnabled = e.RoutedEvent.Name == "Checked";
      this.daquery.IsEnabled = e.RoutedEvent.Name == "Checked";
    }

    private void button2_Click(object sender, RoutedEventArgs e) {
      this.CancelAction = false;
      var fnf = (ConditionalSelectParameters.FileNameFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.FileNameFilterTypes), ((ConditionalSelectComboBoxItem) this.namefilter.SelectedItem).IdentifyingName);
      var fsf = (ConditionalSelectParameters.FileSizeFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.FileSizeFilterTypes), ((ConditionalSelectComboBoxItem) this.sizefilter.SelectedItem).IdentifyingName);
      var sd1 = (FriendlySizeConverter.FileSizeMeasurements)Enum.Parse(typeof(FriendlySizeConverter.FileSizeMeasurements), ((ConditionalSelectComboBoxItem) this.sizebox1.SelectedItem).IdentifyingName);
      var sd2 = (FriendlySizeConverter.FileSizeMeasurements)Enum.Parse(typeof(FriendlySizeConverter.FileSizeMeasurements), ((ConditionalSelectComboBoxItem) this.sizebox2.SelectedItem).IdentifyingName);
      var dcf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem) this.dcfilter.SelectedItem).IdentifyingName);
      var dmf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem) this.dmfilter.SelectedItem).IdentifyingName);
      var daf = (ConditionalSelectParameters.DateFilterTypes)Enum.Parse(typeof(ConditionalSelectParameters.DateFilterTypes), ((ConditionalSelectComboBoxItem) this.dafilter.SelectedItem).IdentifyingName);

      var Part1 = FriendlySizeConverter.GetByteLength(Convert.ToDouble(this.sizequery1.Text.Replace(",", this.ci.NumberFormat.NumberDecimalSeparator).Replace(".", this.ci.NumberFormat.NumberDecimalSeparator)), sd1);
      var Part2 = FriendlySizeConverter.GetByteLength(Convert.ToDouble(this.sizequery2.Text.Replace(",", this.ci.NumberFormat.NumberDecimalSeparator).Replace(".", this.ci.NumberFormat.NumberDecimalSeparator)), sd2);

      this.csd = new ConditionalSelectData(
        new ConditionalSelectParameters.FileNameParameters(this.namequery.Text, fnf, this.namecase.IsChecked.Value),
        new ConditionalSelectParameters.FileSizeParameters(Part1, Part2, fsf),
        new ConditionalSelectParameters.DateParameters(this.dcquery.SelectedDate.Value.Date, dcf),
        new ConditionalSelectParameters.DateParameters(this.dmquery.SelectedDate.Value.Date, dmf),
        new ConditionalSelectParameters.DateParameters(this.daquery.SelectedDate.Value.Date, daf), this.namecheck.IsChecked.Value, this.sizecheck.IsChecked.Value, this.dccheck.IsChecked.Value, this.dmcheck.IsChecked.Value, this.dacheck.IsChecked.Value);
      this.Close();
    }

    private void button1_Click(object sender, RoutedEventArgs e) {
      this.CancelAction = true;
      this.Close();
    }

    private void namequery_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e) {
      if (this.namequery.IsKeyboardFocused) {
        if (this.namequery.Text == (this.FindResource("txtFilename") as string)) {
          this.namequery.Text = "";
        }
      } else {
        if (this.namequery.Text == "") {
          this.namequery.Text = (this.FindResource("txtFilename") as string);
        }
      }
    }
  }
}