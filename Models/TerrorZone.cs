using System.Collections.Generic;

namespace D2RTerrorZone.Models
{
    // 用于我们在 UI 和内部逻辑流转的解析后数据结构
    public class TerrorZone
    {
        // API 返回的原始英文字符串
        public string OriginalName { get; set; }
        
        // 经过 ZoneResolver 解析出的内部唯一 ID 集合
        public List<string> ZoneIds { get; set; }
        
        // 经过 LocalizationService 翻译出的最终显示文本
        public string LocalizedName { get; set; }
    }
}
