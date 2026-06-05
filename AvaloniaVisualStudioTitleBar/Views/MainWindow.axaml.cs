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

        // 最大化时延迟还原所需的状态
        private bool _pendingMaximizedDrag;
        private Point _pressedPointInWindow;
        private PointerPressedEventArgs _pressedArgs;
        private const double MaximizedDragThreshold = 5.0; // 像素阈值，与 Windows 默认拖拽阈值一致

        private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            // 不要在标题栏上的按钮上触发拖动
            if (e.Source is Button || e.Source is MenuItem || e.Source is Menu)
                return;

            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            if (WindowState == WindowState.Maximized)
            {
                // 最大化状态下：仅记录按下位置和原始事件参数，不立即还原
                // 等指针移动超过阈值时再还原并接管拖拽，匹配 Windows 原生体验
                // BeginMoveDrag 只接受 PointerPressedEventArgs，因此必须把按下时的事件保存下来
                _pendingMaximizedDrag = true;
                _pressedPointInWindow = e.GetCurrentPoint(this).Position;
                _pressedArgs = e;
                return;
            }

            // 普通状态下直接交给系统拖动
            BeginMoveDrag(e);
        }

        private void TitleBar_PointerMoved(object sender, PointerEventArgs e)
        {
            if (!_pendingMaximizedDrag)
                return;

            // 鼠标已松开（如系统未送出 Released 时的兜底）
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _pendingMaximizedDrag = false;
                return;
            }

            var current = e.GetCurrentPoint(this).Position;
            var dx = current.X - _pressedPointInWindow.X;
            var dy = current.Y - _pressedPointInWindow.Y;
            if (Math.Abs(dx) < MaximizedDragThreshold && Math.Abs(dy) < MaximizedDragThreshold)
                return;

            // 超过阈值，开始执行"还原 + 重定位 + BeginMoveDrag"
            _pendingMaximizedDrag = false;
            var pressedArgs = _pressedArgs;
            _pressedArgs = null;

            // BeginMoveDrag 只接受 PointerPressedEventArgs；这里使用按下时缓存的 e
            if (pressedArgs == null)
                return;

            if (WindowState != WindowState.Maximized)
            {
                BeginMoveDrag(pressedArgs);
                return;
            }

            // 记录指针在窗口内的水平比例，以便还原后窗口能准确跟随鼠标
            var ratioX = ClientSize.Width > 0 ? _pressedPointInWindow.X / ClientSize.Width : 0.5;

            // 先还原到 Normal
            WindowState = WindowState.Normal;

            // 还原后重新定位窗口，让指针仍然落在标题栏上对应的水平比例位置
            var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
            if (screen != null)
            {
                var scale = screen.Scaling;
                var pointerOnTopLevel = e.GetCurrentPoint(null).Position;
                var newWidth = ClientSize.Width;
                var newLeftPx = (int)(pointerOnTopLevel.X * scale - newWidth * scale * ratioX);
                var newTopPx = (int)(pointerOnTopLevel.Y * scale - 5);
                Position = new PixelPoint(newLeftPx, newTopPx);
            }

            BeginMoveDrag(pressedArgs);
        }

        private void TitleBar_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            // 没有越过阈值就松开 → 视为单击，什么也不做
            _pendingMaximizedDrag = false;
            _pressedArgs = null;
        }

        private void TitleBar_PointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {
            _pendingMaximizedDrag = false;
            _pressedArgs = null;
        }

        private void TitleBar_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (e.Source is Button || e.Source is MenuItem || e.Source is Menu)
                return;

            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseWindow2(object sender, RoutedEventArgs e)
        {
            Window hostWindow = this;
            hostWindow.Close();
        }

        private void MaximizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = this;

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
            Window hostWindow = this;
            hostWindow.WindowState = WindowState.Minimized;
        }

        private void SubscribeToWindowState()
        {
            var ViewModel = DataContext as MainWindowViewModel;

            if (ViewModel.ModernStyle.IsWindowsStyle)
            {
                Window hostWindow = this;

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
                        // 使用 OffScreenMargin，避免顶部出现不可点击的盲区
                        hostWindow.Padding = new Thickness(
                                hostWindow.OffScreenMargin.Left,
                                hostWindow.OffScreenMargin.Top,
                                hostWindow.OffScreenMargin.Right,
                                hostWindow.OffScreenMargin.Bottom);
                        MaximizeToolTip.Content = "Restore Down";
                    }
                });
            }
            else if (ViewModel.ModernStyle.IsMacOSStyle)
            {
                Window hostWindow = this;

                hostWindow.ExtendClientAreaTitleBarHeightHint = 44;
                hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
                {
                    if (s != WindowState.Maximized)
                    {
                        hostWindow.Padding = new Thickness(0, 0, 0, 0);
                    }
                    if (s == WindowState.Maximized)
                    {
                        hostWindow.Padding = new Thickness(
                                hostWindow.OffScreenMargin.Left,
                                hostWindow.OffScreenMargin.Top,
                                hostWindow.OffScreenMargin.Right,
                                hostWindow.OffScreenMargin.Bottom);
                    }
                });
            }
        }
    }
}