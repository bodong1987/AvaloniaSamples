using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace AvaloniaChangeTheme.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        ThemeBox.SelectedItem = AppThemeUtils.CurrentTheme;
        ThemeBox.SelectionChanged += (sender, e) =>
        {
            if (ThemeBox.SelectedItem is ThemeType theme)
            {
                AppThemeUtils.SetTheme(theme);
            }
        };

        ThemeVariantsBox.SelectionChanged += (sender, e) =>
        {
            if (ThemeVariantsBox.SelectedItem is ThemeVariant themeVariant)
            {
                Application.Current!.RequestedThemeVariant = themeVariant;
            }
        };
    }
}
