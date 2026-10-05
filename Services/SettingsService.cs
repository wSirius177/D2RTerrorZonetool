using System;
using System.IO;
using System.Text.Json;
using D2RTerrorZone.Models;

namespace D2RTerrorZone.Services
{
    public class SettingsService
    {
        private readonly string _settingsFolder;
        private readonly string _settingsFilePath;

        public AppSettings Current { get; private set; }

        public SettingsService()
        {
            _settingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "D2RTerrorZone");
            _settingsFilePath = Path.Combine(_settingsFolder, "settings.json");
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    Current = new AppSettings();
                }
            }
            catch
            {
                // 如果文件损坏或无权限，使用默认配置
                Current = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(_settingsFolder))
                {
                    Directory.CreateDirectory(_settingsFolder);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(Current, options);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // 忽略保存失败（例如磁盘写保护）
            }
        }
    }
}
