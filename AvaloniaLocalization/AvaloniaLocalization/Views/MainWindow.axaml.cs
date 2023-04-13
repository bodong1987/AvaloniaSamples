using Avalonia.Controls;
using AvaloniaLocalization.Services;

namespace AvaloniaLocalization.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            textBlock_Code.Text = LocalizationProvider.Service["Test Code"];

            LocalizationProvider.Service.OnCultureChanged += (s, e) =>
            {
                textBlock_Code.Text = LocalizationProvider.Service["Test Code"];
            };
        }
    }
}