using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using D2RTerrorZone.Services;

namespace D2RTerrorZone.ViewModels
{
    public class OverlayViewModel : INotifyPropertyChanged
    {
        private readonly TerrorZoneService _tzService;
        private readonly SettingsService _settings;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentZone = "等待数据...";
        public string CurrentZone
        {
            get => _currentZone;
            set { _currentZone = value; OnPropertyChanged(); }
        }

        private string _nextZone = "等待数据...";
        public string NextZone
        {
            get => _nextZone;
            set { _nextZone = value; OnPropertyChanged(); }
        }

        private string _remainingTime = "00:00";
        public string RemainingTime
        {
            get => _remainingTime;
            set { _remainingTime = value; OnPropertyChanged(); }
        }

        private string _statusText = "正常";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        // 透明度和字体大小直接绑定到设置
        public double Opacity => _settings.Current.Opacity;
        public int FontSize => _settings.Current.FontSize;
        public bool IsMinimalMode => _settings.Current.MinimalMode;
        public bool IsStandardMode => !_settings.Current.MinimalMode;

        public bool IsTopmost => _settings.Current.TopMost;
        public string TopmostIconColor => _settings.Current.TopMost ? "#FFFFFFFF" : "#FF555555";

        public string LanguageDisplayText
        {
            get
            {
                return _settings.Current.Language switch
                {
                    "zh-CN" => "简",
                    "zh-TW" => "繁",
                    "en-US" => "En",
                    _ => "简"
                };
            }
        }

        // 命令
        public ICommand RefreshCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand ToggleTopmostCommand { get; }

        public OverlayViewModel(TerrorZoneService tzService, SettingsService settings)
        {
            _tzService = tzService;
            _settings = settings;

            RefreshCommand = new RelayCommand(async _ => await _tzService.RefreshManuallyAsync());
            CloseCommand = new RelayCommand(_ => {
                // 隐藏窗口（最小化到系统托盘）
                foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        window.Hide();
                        break;
                    }
                }
            });
            SettingsCommand = new RelayCommand(_ => {
                // 循环切换语言
                _settings.Current.Language = _settings.Current.Language == "zh-CN" ? "en-US" : 
                                            (_settings.Current.Language == "en-US" ? "zh-TW" : "zh-CN");
                _settings.Save();
                NotifySettingsChanged();
                _tzService.UpdateLocalization(); // 强制使用新语言重写翻译并更新 UI
            });
            ToggleTopmostCommand = new RelayCommand(_ => {
                _settings.Current.TopMost = !_settings.Current.TopMost;
                _settings.Save();
                NotifySettingsChanged();
            });

            _tzService.DataUpdated += TzService_DataUpdated;
            _tzService.StatusChanged += TzService_StatusChanged;
        }

        private void TzService_StatusChanged(string status)
        {
            StatusText = status;
        }

        private void TzService_DataUpdated()
        {
            if (_tzService.CurrentZone != null)
                CurrentZone = _tzService.CurrentZone.LocalizedName;
            
            if (_tzService.NextZone != null)
                NextZone = _tzService.NextZone.LocalizedName;
        }

        // 供 UI Timer 每秒调用一次来更新倒计时显示
        public void UpdateTimerDisplay(TimeSpan remaining)
        {
            RemainingTime = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
        }

        public void NotifySettingsChanged()
        {
            OnPropertyChanged(nameof(Opacity));
            OnPropertyChanged(nameof(FontSize));
            OnPropertyChanged(nameof(IsMinimalMode));
            OnPropertyChanged(nameof(IsStandardMode));
            OnPropertyChanged(nameof(LanguageDisplayText));
            OnPropertyChanged(nameof(IsTopmost));
            OnPropertyChanged(nameof(TopmostIconColor));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 简单的 RelayCommand 实现
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        public event EventHandler CanExecuteChanged { add { } remove { } }
        public RelayCommand(Action<object> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute(parameter);
    }
}
