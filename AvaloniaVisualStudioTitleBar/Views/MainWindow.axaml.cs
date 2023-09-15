using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Avalonia.Media;
using System.Threading.Tasks;
using AvaloniaVisualStudioTitleBar.ViewModels;
using Avalonia.Input;

namespace AvaloniaVisualStudioTitleBar.Views
{
    // reference : https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainWindowViewModel();
            DataContext = vm;

            SubscribeToWindowState();
        }

        private void CloseWindow(object sender, TappedEventArgs e)
        {
            CloseWindow2(sender, e);
        }

        private void CloseWindow2(object sender, RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.Close();
        }

        private void MaximizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;

            if (hostWindow.WindowState == WindowState.Normal)
            {
                hostWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                hostWindow.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)this.VisualRoot;
            hostWindow.WindowState = WindowState.Minimized;
        }

        private async void SubscribeToWindowState()
        {
            var ViewModel = DataContext as MainWindowViewModel;

            if (ViewModel.ModernStyle.IsWindowsStyle)
            {
                Window hostWindow = (Window)this.VisualRoot;

                while (hostWindow == null)
                {
                    hostWindow = (Window)this.VisualRoot;
                    await Task.Delay(50);
                }

                hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
                {
                    if (s != WindowState.Maximized)
                    {
                        winMaximizeIcon.Data = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                        hostWindow.Padding = new Thickness(0, 0, 0, 0);
                        MaximizeToolTip.Content = "Maximize";
                    }
                    if (s == WindowState.Maximized)
                    {
                        winMaximizeIcon.Data = Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                        hostWindow.Padding = new Thickness(7, 7, 7, 7);
                        MaximizeToolTip.Content = "Restore Down";

                        // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                        /*hostWindow.Padding = new Thickness(
                                hostWindow.OffScreenMargin.Left,
                                hostWindow.OffScreenMargin.Top,
                                hostWindow.OffScreenMargin.Right,
                                hostWindow.OffScreenMargin.Bottom);*/
                    }
                });
            }
            else if (ViewModel.ModernStyle.IsMacOSStyle)
            {
                Window hostWindow = (Window)this.VisualRoot;

                while (hostWindow == null)
                {
                    hostWindow = (Window)this.VisualRoot;
                    await Task.Delay(50);
                }

                hostWindow.ExtendClientAreaTitleBarHeightHint = 44;
                hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
                {
                    if (s != WindowState.Maximized)
                    {
                        hostWindow.Padding = new Thickness(0, 0, 0, 0);
                    }
                    if (s == WindowState.Maximized)
                    {
                        hostWindow.Padding = new Thickness(7, 7, 7, 7);

                        // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                        /*hostWindow.Padding = new Thickness(
                                hostWindow.OffScreenMargin.Left,
                                hostWindow.OffScreenMargin.Top,
                                hostWindow.OffScreenMargin.Right,
                                hostWindow.OffScreenMargin.Bottom);*/
                    }
                });
            }
        }
    }
}