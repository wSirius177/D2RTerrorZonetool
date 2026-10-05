using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace D2RTerrorZone.Services
{
    public class LocalizationService
    {
        // 字典结构: ZoneID -> (语言 -> 翻译文本)
        private Dictionary<string, Dictionary<string, string>> _dictionary;

        public LocalizationService()
        {
            _dictionary = new Dictionary<string, Dictionary<string, string>>();
            LoadDictionary();
        }

        private void LoadDictionary()
        {
            try
            {
                // 读取运行目录下的 Data/localization.json
                var filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "localization.json");
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    _dictionary = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
                }
            }
            catch
            {
                // 如果发生异常（如文件损坏），不崩溃，将使用空字典兜底
            }
        }

        public string GetLocalizedName(List<string> zoneIds, string language = "zh-CN")
        {
            if (zoneIds == null || !zoneIds.Any()) return "未知区域";

            var names = new List<string>();
            foreach (var id in zoneIds)
            {
                if (_dictionary.TryGetValue(id, out var translations) && translations.TryGetValue(language, out var name))
                {
                    names.Add(name);
                }
                else
                {
                    // 如果找不到对应翻译（可能是新出的区域），回退显示 ID，防止程序白屏或报错
                    names.Add(id.Replace("_", " "));
                }
            }

            // 如果有多个区域组合，使用 " / " 拼接。符合游戏里习惯
            return string.Join(" / ", names);
        }
    }
}
