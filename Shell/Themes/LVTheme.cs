namespace BExplorer.Shell.Themes {
  using System.Windows.Media;

  public class LVTheme {
    public Color BackgroundColor { get; private set; }

    public Color TextColor { get; private set; }

    public Color SelectionColor { get; private set; }

    public Color SelectionBorderColor { get; private set; }

    public LVTheme(ThemeColors color) {
      switch (color) {
        case ThemeColors.Light:
          this.SelectionColor = Color.FromArgb(75, 128, 128, 128);
          this.SelectionBorderColor = Color.FromArgb(75, 210, 210, 200);
          break;
        case ThemeColors.Dark:
          this.SelectionColor = Color.FromArgb(75, 128, 128, 128);
          this.SelectionBorderColor = Color.FromArgb(75, 210, 210, 210);
          break;
      }

      this.BackgroundColor = (System.Windows.Media.Color)System.Windows.Application.Current.Resources["WhiteColor"];
      this.TextColor = (System.Windows.Media.Color)System.Windows.Application.Current.Resources["Gray6"];
    }
  }
}
