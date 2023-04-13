using AvaloniaLocalization.Services;

namespace AvaloniaLocalization.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        public CultureInfoData[] AvailableCultures => LocalizationProvider.Service.AvailableCultures.ToArray();

        public CultureInfoData SelectedCulture
        {
            get => LocalizationProvider.Service.SelectedCulture;
            set => LocalizationProvider.Service.SelectedCulture = value;
        }
    }
}