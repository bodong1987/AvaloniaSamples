using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using AvaloniaChangeTheme.ViewModels;
using AvaloniaChangeTheme.Views;

namespace AvaloniaChangeTheme;

public partial class App : Application
{
    public override void Initialize()
    {
        AppThemeUtils.BeforeInitialize();

        AvaloniaXamlLoader.Load(this);

        AppThemeUtils.AfterInitialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
