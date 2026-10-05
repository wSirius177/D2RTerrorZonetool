using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using D2RTerrorZone.Services;
using D2RTerrorZone.ViewModels;

namespace D2RTerrorZone.UI
{
    public partial class OverlayWindow : Window
    {
        private readonly OverlayViewModel _viewModel;
        private readonly TerrorZoneService _tzService;
        private readonly CountdownService _countdown;
        private readonly SettingsService _settings;
        private DispatcherTimer _timer;

        public OverlayWindow(OverlayViewModel viewModel, TerrorZoneService tzService, CountdownService countdown, SettingsService settings)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _tzService = tzService;
            _countdown = countdown;
            _settings = settings;
            
            DataContext = _viewModel;

            Loaded += OverlayWindow_Loaded;
            Closed += OverlayWindow_Closed;
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 恢复窗口位置
            if (_settings.Current.WindowX > 0 && _settings.Current.WindowY > 0)
            {
                Left = _settings.Current.WindowX;
                Top = _settings.Current.WindowY;
            }

            // 启动定时器，每秒刷新一次剩余时间
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // 500ms 刷新一次防止跨秒抖动
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 如果设置了鼠标穿透 (MouseThrough)，需要在加载后设置 Win32 扩展样式
            // 这里为了简单，如果要求穿透可以调用 Win32 API 设 WS_EX_TRANSPARENT
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var next = _countdown.GetNextRefreshTime(DateTimeOffset.Now);
            var remaining = _countdown.GetRemainingTime(next, DateTimeOffset.Now);
            _viewModel.UpdateTimerDisplay(remaining);
        }

        private void OverlayWindow_Closed(object sender, EventArgs e)
        {
            _timer?.Stop();
            // 保存位置
            _settings.Current.WindowX = Left;
            _settings.Current.WindowY = Top;
            _settings.Save();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
