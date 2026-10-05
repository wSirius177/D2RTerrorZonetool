using System;
using System.Threading;
using System.Threading.Tasks;
using D2RTerrorZone.Models;
using D2RTerrorZone.Resolvers;

namespace D2RTerrorZone.Services
{
    public class TerrorZoneService
    {
        private readonly ITerrorZoneProvider _provider;
        private readonly LocalizationService _localization;
        private readonly CountdownService _countdown;
        private readonly SettingsService _settings;

        public TerrorZone CurrentZone { get; private set; }
        public TerrorZone NextZone { get; private set; }
        public DateTimeOffset NextRefreshTime { get; private set; }

        // 事件通知 UI 数据已经更新
        public event Action DataUpdated;
        public event Action<string> StatusChanged;

        private CancellationTokenSource _refreshCts;

        public TerrorZoneService(
            ITerrorZoneProvider provider, 
            LocalizationService localization, 
            CountdownService countdown,
            SettingsService settings)
        {
            _provider = provider;
            _localization = localization;
            _countdown = countdown;
            _settings = settings;
        }

        public void Start()
        {
            _refreshCts = new CancellationTokenSource();
            _ = RunAutoRefreshLoopAsync(_refreshCts.Token);
        }

        public void Stop()
        {
            _refreshCts?.Cancel();
        }

        /// <summary>
        /// 手动刷新。不改变下一个自动刷新周期的整点时间边界
        /// </summary>
        public async Task RefreshManuallyAsync()
        {
            StatusChanged?.Invoke("正在更新…");
            await FetchAndUpdateAsync(false);
        }

        private async Task RunAutoRefreshLoopAsync(CancellationToken token)
        {
            // 启动时立即获取一次
            await FetchAndUpdateAsync(true);

            while (!token.IsCancellationRequested)
            {
                NextRefreshTime = _countdown.GetNextRefreshTime(DateTimeOffset.Now);
                var delay = _countdown.GetRemainingTime(NextRefreshTime, DateTimeOffset.Now);
                
                // 等待直到下一个 :00 或 :30 边界
                try
                {
                    await Task.Delay(delay, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                // 边界到达，开始请求数据
                StatusChanged?.Invoke("正在更新…");
                await FetchWithRetryUntilChangedAsync(token);
            }
        }

        private async Task FetchWithRetryUntilChangedAsync(CancellationToken token)
        {
            var oldCurrentOriginal = CurrentZone?.OriginalName;
            
            // API 更新可能有延迟，这里实现文档中要求的短间隔重试策略: T+0, T+5, T+10, T+20, T+30...
            int[] retryDelays = { 0, 5000, 5000, 10000, 10000, 15000 };
            
            for (int i = 0; i < retryDelays.Length; i++)
            {
                if (token.IsCancellationRequested) break;

                if (retryDelays[i] > 0)
                {
                    await Task.Delay(retryDelays[i], token);
                }

                var success = await FetchAndUpdateAsync(false);
                if (success && CurrentZone?.OriginalName != oldCurrentOriginal)
                {
                    // 确认数据已经变化，跳出重试
                    return;
                }
            }

            // 如果一直没变，继续回退为正常状态，不抛出明显错误提示
            StatusChanged?.Invoke("正常");
            DataUpdated?.Invoke();
        }

        private async Task<bool> FetchAndUpdateAsync(bool isStartup)
        {
            try
            {
                var data = await _provider.GetCurrentAsync(CancellationToken.None);
                if (data != null && !string.IsNullOrEmpty(data.Current))
                {
                    CurrentZone = ParseZone(data.Current);
                    NextZone = ParseZone(data.Next);

                    StatusChanged?.Invoke("正常");
                    DataUpdated?.Invoke();
                    return true;
                }
                
                if (!isStartup)
                {
                    StatusChanged?.Invoke("网络暂时不可用，稍后重试");
                }
                return false;
            }
            catch
            {
                if (!isStartup)
                {
                    StatusChanged?.Invoke("网络暂时不可用，稍后重试");
                }
                return false;
            }
        }

        private TerrorZone ParseZone(string apiName)
        {
            if (string.IsNullOrEmpty(apiName)) return null;

            var ids = ZoneResolver.Resolve(apiName);
            var localized = _localization.GetLocalizedName(ids, _settings.Current.Language);

            return new TerrorZone
            {
                OriginalName = apiName,
                ZoneIds = ids,
                LocalizedName = localized
            };
        }

        public void UpdateLocalization()
        {
            if (CurrentZone != null)
            {
                CurrentZone.LocalizedName = _localization.GetLocalizedName(CurrentZone.ZoneIds, _settings.Current.Language);
            }
            if (NextZone != null)
            {
                NextZone.LocalizedName = _localization.GetLocalizedName(NextZone.ZoneIds, _settings.Current.Language);
            }
            DataUpdated?.Invoke();
        }
    }
}
