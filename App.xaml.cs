using System.Windows;
using D2RTerrorZone.Models;
using D2RTerrorZone.Services;
using D2RTerrorZone.ViewModels;
using D2RTerrorZone.UI;

namespace D2RTerrorZone
{
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private TerrorZoneService _tzService;
        private OverlayWindow _overlayWindow;

        static App()
        {
            AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport", true);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                System.Windows.MessageBox.Show(args.ExceptionObject.ToString(), "AppDomain崩溃报告");
            };

            this.DispatcherUnhandledException += (s, args) =>
            {
                System.Windows.MessageBox.Show(args.Exception.ToString(), "崩溃报告");
                args.Handled = true;
            };

            // 1. 初始化所有服务
            var settings = new SettingsService();
            var localization = new LocalizationService();
            var provider = new D2RunewizardProvider();
            var countdown = new CountdownService();
            
            _tzService = new TerrorZoneService(provider, localization, countdown, settings);
            
            // 2. 初始化 ViewModel 和 Window
            var viewModel = new OverlayViewModel(_tzService, settings);
            _overlayWindow = new OverlayWindow(viewModel, _tzService, countdown, settings);

            // 3. 启动后台逻辑
            _tzService.Start();

            // 4. 初始化系统托盘
            InitTrayIcon();

            // 5. 显示窗口
            _overlayWindow.Show();
        }

        private void InitTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Text = "D2R 恐惧地带";
            _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath);
            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += (s, e) => ToggleWindow();

            var menu = new System.Windows.Forms.ContextMenuStrip();
            
            var toggleItem = menu.Items.Add("显示/隐藏");
            toggleItem.Click += (s, e) => ToggleWindow();

            var refreshItem = menu.Items.Add("立刻刷新");
            refreshItem.Click += async (s, e) => await _tzService.RefreshManuallyAsync();

            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

            var exitItem = menu.Items.Add("退出");
            exitItem.Click += (s, e) => 
            {
                _tzService.Stop();
                _notifyIcon.Dispose();
                Current.Shutdown();
            };

            _notifyIcon.ContextMenuStrip = menu;
        }

        private void ToggleWindow()
        {
            if (_overlayWindow.IsVisible)
                _overlayWindow.Hide();
            else
                _overlayWindow.Show();
        }
    }
}

