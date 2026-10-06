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
                if (TryGetTranslation(id, language, out var name))
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

        private bool TryGetTranslation(string id, string language, out string name)
        {
            name = null;
            if (string.IsNullOrWhiteSpace(id)) return false;

            // 1. 直接匹配
            if (TryExtractName(id, language, out name)) return true;

            // 2. 尝试添加或移除 "THE_" 前缀
            if (id.StartsWith("THE_"))
            {
                var stripped = id.Substring(4);
                if (TryExtractName(stripped, language, out name)) return true;
            }
            else
            {
                var added = "THE_" + id;
                if (TryExtractName(added, language, out name)) return true;
            }

            // 3. 尝试去除 _LEVEL_X 后缀（比如含有多层的情况）
            var levelIndex = id.IndexOf("_LEVEL_", System.StringComparison.OrdinalIgnoreCase);
            if (levelIndex > 0)
            {
                var baseId = id.Substring(0, levelIndex);
                if (TryGetTranslation(baseId, language, out name)) return true;
            }

            // 4. 尝试去除或添加结尾的 'S' (单复数容错，例如 TOMB / TOMBS)
            if (id.EndsWith("S"))
            {
                var singular = id.Substring(0, id.Length - 1);
                if (TryExtractName(singular, language, out name)) return true;
            }
            else
            {
                var plural = id + "S";
                if (TryExtractName(plural, language, out name)) return true;
            }

            return false;
        }

        private bool TryExtractName(string key, string language, out string name)
        {
            name = null;
            if (_dictionary.TryGetValue(key, out var translations))
            {
                if (translations.TryGetValue(language, out name) && !string.IsNullOrWhiteSpace(name))
                    return true;

                // 语言回退：优先简体中文，其次英文，再次任意有效翻译
                if (translations.TryGetValue("zh-CN", out name) && !string.IsNullOrWhiteSpace(name))
                    return true;

                if (translations.TryGetValue("en-US", out name) && !string.IsNullOrWhiteSpace(name))
                    return true;

                var fallback = translations.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                if (fallback != null)
                {
                    name = fallback;
                    return true;
                }
            }
            return false;
        }
    }
}
