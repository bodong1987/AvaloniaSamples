using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace AvaloniaDataValidation.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnShowError(object sender, RoutedEventArgs e)
        {
            if(!DataValidationErrors.GetHasErrors(textBox_Errors))
            {
                DataValidationErrors.SetError(textBox_Errors, new ArgumentException("A custom exception error"));
            }
            else
            {
                DataValidationErrors.SetError(textBox_Errors, null);
            }
            
        }
    }
}