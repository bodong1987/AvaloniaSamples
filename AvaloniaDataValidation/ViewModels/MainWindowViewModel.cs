using ReactiveUI;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaDataValidation.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        string EmailCore = "";

        [EmailAddress]
        public string Email
        {
            get => EmailCore;
            set => this.RaiseAndSetIfChanged(ref EmailCore, value);
        }
    }
}