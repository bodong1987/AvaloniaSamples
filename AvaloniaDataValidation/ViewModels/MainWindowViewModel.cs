using ReactiveUI;
using System;
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

        public string[] CombItems => new string[]
        {
            "C",
            "C++",
            "C#",
        };

        string ProgramLanguageCore = "C";

        [LaungageValidation]
        public string ProgramLanguage
        {
            get => ProgramLanguageCore;
            set 
            {
                // can't use this
//                 if(value == "C#")
//                 {
//                     throw new ArgumentException($"{nameof(ProgramLanguage)} can not use C#");
//                 }

                this.RaiseAndSetIfChanged(ref ProgramLanguageCore, value);
            }
        }
    }

    internal class LaungageValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is string lan)
            {
                if (lan == "C#")
                {
                    return new ValidationResult($"[{validationContext.DisplayName}] can not be C#");
                }
            }

            return ValidationResult.Success;
        }
    }
}