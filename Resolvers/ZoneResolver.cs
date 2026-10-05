using System;
using System.Collections.Generic;
using System.Linq;

namespace D2RTerrorZone.Resolvers
{
    public class ZoneResolver
    {
        // 核心：基于预处理好的 localization.json 键名规则
        // 将 API 返回的英文转为我们在 JSON 里定义的内部 ID
        
        public static List<string> Resolve(string apiEnglishName)
        {
            if (string.IsNullOrWhiteSpace(apiEnglishName))
                return new List<string>();

            // 处理特殊组合区域。拆分并解析为独立的内部 Zone ID
            // 例如 API 返回 "Burial Grounds, Crypt, and the Mausoleum"
            var splitSeparators = new[] { " and the ", ", and ", " and ", " / ", ", " };
            
            if (splitSeparators.Any(s => apiEnglishName.Contains(s, StringComparison.OrdinalIgnoreCase)))
            {
                var parts = apiEnglishName.Split(splitSeparators, StringSplitOptions.RemoveEmptyEntries);
                return parts.Select(p => ResolveSingle(p)).Where(id => !string.IsNullOrEmpty(id)).ToList();
            }

            return new List<string> { ResolveSingle(apiEnglishName) };
        }

        private static string ResolveSingle(string name)
        {
            name = name.Trim();
            
            // 简单的规范化：全大写，空格换下划线，去掉单引号，横杠变下划线
            // 对应我们在 parse.py 中生成 JSON 的逻辑
            var id = name.ToUpper()
                         .Replace(" ", "_")
                         .Replace("'", "")
                         .Replace("-", "_");
            
            // 这里可以加入别名映射 (Aliases) 如果未来 API 名称发生微调
            // 例如: if (id == "CHAOS_SANCTUARY") return "THE_CHAOS_SANCTUARY";
            
            return id;
        }
    }
}
