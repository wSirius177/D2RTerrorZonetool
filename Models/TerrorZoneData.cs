using System.Text.Json.Serialization;

namespace D2RTerrorZone.Models
{
    public class TerrorZoneData
    {
        // 对应 d2runewizard API 的结构
        [JsonPropertyName("current")]
        public string Current { get; set; }

        [JsonPropertyName("next")]
        public string Next { get; set; }
    }
}
