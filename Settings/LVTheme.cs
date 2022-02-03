using System.Windows.Media;

namespace Settings {
  public class LVTheme {
    public Color BackgroundColor { get; private set; }
    public Color BackgroundColorTree { get; private set; }

    public Color TextColor { get; private set; }

    public Color SelectionColor { get; private set; }
    public Color MouseOverColor { get; private set; }
    public Color SelectionFocusedColor { get; private set; }

    public Color SelectionBorderColor { get; private set; }

    public Color HeaderSelectionColor { get; private set; }

    public Color HeaderBackgroundColor { get; private set; }

    public Color HeaderDividerColor { get; private set; }

    public Color SortColumnColor { get; private set; }
    public Color HeaderArrowColor { get; private set; }

    public Color AdornerBorderColor { get; private set; }
    public Color AdornerHoleColor { get; private set; }
    public Color AdornerBackgroundColor { get; private set; }

    public LVTheme(ThemeColors color) {
      switch (color) {
        case ThemeColors.Light:
          this.SelectionColor = Color.FromArgb(75, 2, 163, 255);
          this.SelectionFocusedColor = Color.FromArgb(75, 2, 163, 255);
          this.MouseOverColor = Color.FromArgb(75, 146, 212, 250);
          this.SelectionBorderColor = Color.FromArgb(75, 10, 127, 237);
          this.HeaderSelectionColor = Color.FromArgb(255, 235, 235, 235);
          this.TextColor = Colors.Black;
          this.HeaderBackgroundColor = Colors.White;
          this.SortColumnColor = Color.FromRgb(235, 244, 254);
          this.HeaderDividerColor = Color.FromRgb(235, 244, 254);
          this.HeaderArrowColor = Color.FromArgb(75, 10, 127, 237);
          this.BackgroundColorTree = Colors.White;
          this.AdornerBorderColor = Color.FromRgb(46, 46, 46);
          this.AdornerBackgroundColor = Color.FromRgb(57, 57, 57);
          this.AdornerHoleColor = Colors.White;
          this.BackgroundColor = Colors.White;
          break;
        case ThemeColors.Dark:
          this.MouseOverColor = Color.FromArgb(78, 60, 59, 59);
          this.SelectionColor = Color.FromArgb(255, 32, 31, 31);
          this.SelectionFocusedColor = Color.FromArgb(75, 220, 220, 220);
          this.SelectionBorderColor = Color.FromArgb(218, 52, 159, 224);
          this.HeaderSelectionColor = Color.FromArgb(255, 6, 6, 6);
          this.TextColor = Color.FromRgb(204, 204, 204);
          this.HeaderBackgroundColor = Color.FromRgb(0, 0, 0);
          this.SortColumnColor = Color.FromArgb(255, 6, 6, 6);
          this.HeaderDividerColor = Color.FromRgb(79, 79, 79);
          this.HeaderArrowColor = Color.FromArgb(75, 220, 220, 220);
          this.BackgroundColorTree = Color.FromArgb(255, 0, 0, 0);
          this.AdornerBorderColor = Color.FromArgb(255, 168, 168, 168);
          this.AdornerBackgroundColor = Color.FromArgb(255, 37, 37, 37);
          this.AdornerHoleColor = Color.FromArgb(255, 168, 168, 168);
          this.BackgroundColor = Color.FromRgb(0, 0, 0);
          break;
      }



    }
  }
}
