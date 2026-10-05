using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using D2RTerrorZone.Models;

namespace D2RTerrorZone.Services
{
    public class D2RunewizardProvider : ITerrorZoneProvider
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://d2runewizard.com/api/terror-zone";

        public D2RunewizardProvider()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // 伪装浏览器请求头（如果 API 偶尔有简单的防护）
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public async Task<TerrorZoneData> GetCurrentAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // 忽略大小写
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<TerrorZoneData>(json, options);
                
                return data;
            }
            catch (Exception)
            {
                // 可以加简单的本地文件日志。不能让程序崩掉。
                // 网络不通、超时、JSON序列化失败都会走到这里
                return null;
            }
        }
    }
}
