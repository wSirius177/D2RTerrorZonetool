using System;
using System.Text.Json.Serialization;

namespace D2RTerrorZone.Models
{
    public class AppSettings
    {
        public string Language { get; set; } = "zh-CN";
        public double Opacity { get; set; } = 0.85;
        public bool TopMost { get; set; } = true;
        public bool MouseThrough { get; set; } = false;
        public bool MinimalMode { get; set; } = false;
        public int FontSize { get; set; } = 14;
        public bool Notifications { get; set; } = true;
        public bool Startup { get; set; } = false;
        public double WindowX { get; set; } = 100;
        public double WindowY { get; set; } = 100;
    }
}
