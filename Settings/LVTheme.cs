using System.Windows.Media;

namespace Settings {
  public class LVTheme {
    public Color BackgroundColor { get; private set; }

    public Color TextColor { get; private set; }

    public Color SelectionColor { get; private set; }

    public Color SelectionBorderColor { get; private set; }

    public Color HeaderSelectionColor { get; private set; }

    public Color HeaderBackgroundColor { get; private set; }

    public Color HeaderDividerColor { get; private set; }

    public Color SortColumnColor { get; private set; }

    public LVTheme(ThemeColors color) {
      switch (color) {
        case ThemeColors.Light:
          this.SelectionColor = Color.FromArgb(75, 128, 128, 128);
          this.SelectionBorderColor = Color.FromArgb(109, 96, 96, 91);
          this.HeaderSelectionColor = Color.FromArgb(255, 235, 235, 235);
          this.TextColor = Colors.Black;
          this.HeaderBackgroundColor = Colors.White;
          this.SortColumnColor = Color.FromRgb(59,59,59);
          this.HeaderDividerColor = Colors.DarkGray;
          break;
        case ThemeColors.Dark:
          this.SelectionColor = Color.FromArgb(75, 128, 128, 128);
          this.SelectionBorderColor = Color.FromArgb(75, 210, 210, 210);
          this.HeaderSelectionColor = Color.FromArgb(255, 60, 60, 60);
          this.TextColor = (System.Windows.Media.Color)System.Windows.Application.Current.Resources["Gray6"];
          this.HeaderBackgroundColor = Color.FromRgb(45,45,45);
          this.SortColumnColor = Color.FromRgb(35,35,35);
          this.HeaderDividerColor = Color.FromRgb(79,79,79);
          break;
      }

      this.BackgroundColor = (System.Windows.Media.Color)System.Windows.Application.Current.Resources["WhiteColor"];
      
    }
  }
}
